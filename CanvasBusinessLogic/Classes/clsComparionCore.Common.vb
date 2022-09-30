Imports ECCore
Imports ECCore.ECTypes 'M0332 
Imports ExpertChoice.Service
Imports ECSecurity.ECSecurity
Imports Canvas
Imports GenericDBAccess.ECGenericDatabaseAccess
Imports SpyronControls.Spyron.Core
Imports System.IO

Namespace ExpertChoice.Data

    <Serializable()> Partial Public Class clsComparionCore
        Implements IDisposable  ' D2236

        ' D0461 ===
        Private _Error As clsApplicationError   ' D0117

        Private _LanguageCurrentCode As String
        Private _CurrentLanguage As clsLanguageResource
        Private _DefaultLanguage As clsLanguageResource
        Private _LanguagesList As List(Of clsLanguageResource)

        Private _CurrentUser As clsApplicationUser   ' D0045 + D0046


        'Private _CurrentProject As clsProject  ' -D0511
        Private _ProjectID As Integer   ' D0511
        Private _ProjectsList As List(Of clsProject)        ' D0466
        'Private _WorkgroupProjects As List(Of clsProject)   ' D0469    '-D0511

        Private _SystemWorkgroup As clsWorkgroup
        Private _CurrentWorkgroup As clsWorkgroup
        Private _StartupWorkgroup As clsWorkgroup           ' D0584
        Private _ActiveAvailableWorkgroups As List(Of clsWorkgroup) = Nothing ' D4659

        Private _CurrentUserWorkgroup As clsUserWorkgroup   ' D0465

        Private _CurrentRoleGroup As clsRoleGroup           ' D0465
        Private _CurrentProjectGroup As clsRoleGroup        ' D0466

        Private _CurrentWorkspace As clsWorkspace           ' D0465
        Private _Workspaces As List(Of clsWorkspace)

        Private _SystemUserWorkgroups As List(Of clsUserWorkgroup)  ' D0465
        Private _UserWorkgroups As List(Of clsUserWorkgroup)
        ' D0461 ==
        Private _UserTemplates As List(Of clsUserTemplate)          ' D0795
        Private _UserTemplatesDefault As List(Of clsUserTemplate)   ' D2184

        Public _EULA_checked As Boolean     ' D0287 + D0472
        Private _EULA_valid As Boolean      ' D0287

        Private _Options As clsComparionCoreOptions   ' D0460 + D0465

        Private _isEvalSiteDefault As Boolean = False   ' D6267 + D6359

        Public _DefaultPipeParamSets As Dictionary(Of String, String) = Nothing    ' D0832 + D2256

        Private _SurveysManager As clsSurveysManager    ' D0488
        Private _SurveysList As ArrayList               ' D0488

        Private _SilverLightChecked As clsExtra         ' D0529

        Private _HashCache As Dictionary(Of String, String) ' D0899

        Private _DB_InstanceID As String       ' D0557

        Private _WipeoutTimeout As Integer  ' D0944

        Private _CheckedUserWorkgroups As Boolean = False   ' D0969

        Private _LastSnapshot As clsSnapshot = Nothing      ' D3509
        Private _LastSnapshotChecked As Date = Now.AddDays(-1)  ' D3757

        Private _InstanceID As Long = 0     ' D3967

        Public isServiceRun As Boolean = False  ' D2236
        Public isMobileBrowser As Boolean = False ' D2949 + D3183

        Private LastPrjLogDT As DateTime = Now  ' D3571
        Private LastPrjLogMsg As String = ""    ' D3571

        ' D4920 ===
        Public Property Antigua_UserEmail As String = ""
        Public Property Antigua_UserName As String = ""
        Public Property Antigua_UserID As Integer = -1
        Public Property Antigua_MeetingID As Long = 0
        ' D4920 ==

        Public SSO_User As Boolean = False  ' D6552
        Public Property OriginalSessionUser As String = ""  ' D7363
        Public Property MFA_Requested As Boolean = False    ' D7502

        Public Property isAlexaUser As Boolean = False      ' D7584

        Public Sub New()
            _Error = Nothing        ' D0017
            _Options = New clsComparionCoreOptions  ' D0460
            ' D0461 ===
            _LanguageCurrentCode = _LANG_DEFCODE
            _CurrentLanguage = Nothing
            _DefaultLanguage = Nothing
            _LanguagesList = Nothing
            '_CurrentProject = Nothing  ' -D0511
            _ProjectID = -1 ' D0511
            _ProjectsList = Nothing         ' D0466
            '_WorkgroupProjects = Nothing    ' D0469 -D0511
            _CurrentUser = Nothing
            _CurrentWorkgroup = Nothing
            _StartupWorkgroup = Nothing ' D0584
            _CurrentRoleGroup = Nothing ' D0465
            _CurrentUserWorkgroup = Nothing
            _CurrentWorkspace = Nothing ' D0465
            _UserWorkgroups = Nothing   ' D0465
            _Workspaces = Nothing
            _EULA_checked = False   ' D0287
            _EULA_valid = False     ' D0287
            _Database = Nothing
            ' D0461 ==
            _CanvasMasterConnDefinition = Nothing ' D0330
            _CanvasProjectsConnDefinition = Nothing ' D0478
            '_SpyronMasterConnDefinition = Nothing ' D0330 -D6423
            _SurveysManager = Nothing       ' D0488
            _SurveysList = Nothing
            _SilverLightChecked = Nothing   ' D0529
            _DB_InstanceID = ""    ' D0557
            _HashCache = New Dictionary(Of String, String)  ' D0899
            _WipeoutTimeout = -1        ' D0944
        End Sub


#Region "Common properties: Options, ApplicationError"

        Public Function GetCoreVersion() As Version
            ' D0367 ===
            Dim tOriginalVersion As Version = System.Reflection.Assembly.GetExecutingAssembly.GetName.Version
            Return New Version(tOriginalVersion.Major, tOriginalVersion.Minor, tOriginalVersion.Build, RevisionNumber(_FILE_ROOT))   ' D0473
            ' D0367 ==
        End Function

        Public Property Options() As clsComparionCoreOptions
            Get
                Return _Options
            End Get
            Set(ByVal value As clsComparionCoreOptions)
                _Options = value
            End Set
        End Property

        Public ReadOnly Property isOptionsLoaded() As Boolean
            Get
                Return Not _Options Is Nothing AndAlso _Options.CanvasMasterDBName <> ""
            End Get
        End Property

        ' D0117 ===
        Public Property ApplicationError() As clsApplicationError
            Get
                If _Error Is Nothing Then _Error = New clsApplicationError ' D0465
                Return _Error
            End Get
            Set(ByVal value As clsApplicationError)
                _Error = value
            End Set
        End Property
        ' D0117 ==

        ' D6006 ===
        Public Sub ApplicationErrorInitAndSaveLog(ByVal ErrType As ecErrorStatus, ByVal tPageID As Integer, ByVal sErrorMessage As String, Optional ByVal ErrObject As Object = Nothing, Optional ByVal sErrorSrcURL As String = "", Optional ByVal tDetails As Exception = Nothing)
            ApplicationError.Init(ErrType, tPageID, sErrorMessage, ErrObject, sErrorSrcURL, tDetails)
            Try
                DBSaveLog(dbActionType.actShowRTE, If(ApplicationError.Status = ecErrorStatus.errRTE, dbObjectType.einfRTE, dbObjectType.einfMessage), -1, sErrorMessage, String.Format("Details: {0};", IIf(tDetails IsNot Nothing OrElse String.IsNullOrEmpty(tDetails.StackTrace), "none", tDetails.StackTrace)))
            Catch ex As Exception
            End Try
        End Sub
        ' D6006 ==

        ' D0557 ===
        Public ReadOnly Property DatabaseID() As String
            Get
                If _DB_InstanceID = "" AndAlso isCanvasMasterDBValid Then   ' D3936
                    Dim DB_InstanceExtra As clsExtra = DBExtraRead(clsExtra.Params2Extra(-1, ecExtraType.Common, ecExtraProperty.DatabaseID, ""))
                    If DB_InstanceExtra IsNot Nothing Then _DB_InstanceID = CStr(DB_InstanceExtra.Value)
                    If _DB_InstanceID.Trim = "" Then
                        ' D3936 ===
                        Try

                            ' AD: sys props of MS SQL databases;
                            ' SELECT [name], [state], [state_desc], [recovery_model], [recovery_model_desc], [database_guid] FROM sys.databases INNER JOIN sys.database_recovery_status ON sys.database_recovery_status.database_id = sys.databases.database_id
                            ' get GUID of database
                            ' SELECT database_guid FROM sys.databases INNER JOIN sys.database_recovery_status ON sys.database_recovery_status.database_id = sys.databases.database_id WHERE name='Canvas'

                            Dim tParams As New List(Of Object)
                            tParams.Add(Options.CanvasMasterDBName)
                            Dim DBID As Object = Database.ExecuteScalarSQL("SELECT database_guid FROM sys.databases INNER JOIN sys.database_recovery_status ON sys.database_recovery_status.database_id = sys.databases.database_id WHERE name=?", tParams)
                            If DBID IsNot Nothing AndAlso Not IsDBNull(DBID) Then _DB_InstanceID = CStr(DBID).ToLower
                        Catch ex As Exception
                        End Try
                        If _DB_InstanceID = "" Then
                            ' D3936 ==
                            '_InstanceID = Guid.NewGuid.ToString
                            ' D3934 ===
                            Dim sName As String = System.Environment.MachineName
                            If sName Is Nothing Then sName = ""
                            If Options IsNot Nothing AndAlso Options.CanvasMasterDBName IsNot Nothing Then sName += Options.CanvasMasterDBName
                            _DB_InstanceID = GetMD5(sName)    ' D1982 + D3934: was _FILE_ROOT
                        End If
                        ' D3934 ==
                        If DBExtraWrite(clsExtra.Params2Extra(-1, ecExtraType.Common, ecExtraProperty.DatabaseID, _DB_InstanceID)) Then DBSaveLog(dbActionType.actCreate, dbObjectType.einfOther, -1, "Create Database InstanceID", _DB_InstanceID) ' D1982
                        DebugInfo("Create DB Instance ID: " + _DB_InstanceID, _TRACE_INFO)
                    End If
                End If
                Return _DB_InstanceID
            End Get
        End Property
        ' D0557 ==

        ' D3934 ===
        Public Function GetInstanceID() As Long
            If _InstanceID <> 0 Then Return _InstanceID ' D3967
            Dim EnvKeys As String() = {"PROCESSOR_IDENTIFIER", "NUMBER_OF_PROCESSORS", "PROCESSOR_REVISION"}
            Const _Separator As String = "#"
            Const _Version As Byte = 1  ' // Up to version 15 (4 bits)
            Dim sPCName As String = ""
            Dim sDBName As String = ""
            ' D3936 ===
            If isCanvasMasterDBValid Then
                Dim sMachineName As String = System.Environment.MachineName
                If sMachineName Is Nothing Then sMachineName = ""
                sPCName = sMachineName
                If Not String.IsNullOrEmpty(System.Environment.UserName) Then sPCName += "\" + System.Environment.UserName
                For Each sKey As String In EnvKeys
                    If System.Environment.GetEnvironmentVariables.Contains(sKey) Then sPCName += _Separator + System.Environment.GetEnvironmentVariable(sKey)
                Next
                'Dim sHWInfo As String = HardwareInfo.GetHDDVolumeSerial
                sPCName += _Separator + HardwareInfo.GetDriveSerialNumber(CChar(_FILE_ROOT.Substring(0))).ToString("X8")
                If Options IsNot Nothing Then sDBName = Options.CanvasMasterDBName
                If sDBName Is Nothing Then sDBName = ""
                If Database IsNot Nothing AndAlso Not String.IsNullOrEmpty(Database.ServerName) Then sDBName = Database.ServerName + _Separator + sDBName
                If DatabaseID <> "" Then sDBName += _Separator + DatabaseID

                If isCanvasMasterDBValid Then
                    Try
                        Dim masterDT As Object = Database.ExecuteScalarSQL("SELECT create_date FROM sys.databases WHERE name='master'")
                        If masterDT IsNot Nothing AndAlso Not IsDBNull(masterDT) Then sDBName += _Separator + CType(masterDT, DateTime).ToString("yyMMddHHmmss")
                    Catch ex As Exception
                    End Try
                End If
                sDBName = sDBName.ToLower.Replace("localhost", sMachineName).Replace("127.0.0.1", sMachineName)
            End If
            ' D3936 ==
            _InstanceID = (CLng(_Version) << 60) + ((CLng(CRC32.Compute(sPCName.ToLower)) << 36) >> 4) + CRC32.Compute(sDBName.ToLower) ' D3967
            Return _InstanceID  ' D3967
        End Function

        Public Function GetInstanceID_AsString() As String
            Return GetInstanceID.ToString("X16").Insert(8, "-")
        End Function
        ' D3934 ==

        ' D6311 ===
        Private ReadOnly Property _TEMPL_SNAPSHOT_PROJECT As String
            Get
                Return ResString("templ_Project")
            End Get
        End Property

        ' D6311 ==

        ' D0944 ===
        Public Property WipeoutProjectsTimeout(Optional ByVal UserID As Integer = -1) As Integer
            Get
                If _WipeoutTimeout < 0 Then
                    Dim ValExtra As clsExtra = Nothing
                    If UserID = -1 AndAlso ActiveUser IsNot Nothing Then UserID = ActiveUser.UserID
                    If UserID > 0 Then ValExtra = DBExtraRead(clsExtra.Params2Extra(UserID, ecExtraType.Common, ecExtraProperty.WipeoutProjectsTimeout))
                    If ValExtra IsNot Nothing AndAlso Not Integer.TryParse(ValExtra.Value.ToString, _WipeoutTimeout) Then _WipeoutTimeout = -1
                    If _WipeoutTimeout <= 0 Then _WipeoutTimeout = _DEF_PROJECTS_WIPEOUP_DAYS
                End If
                Return _WipeoutTimeout
            End Get
            Set(ByVal value As Integer)
                If value <> _WipeoutTimeout AndAlso value > 0 Then  ' D0946
                    If UserID = -1 AndAlso ActiveUser IsNot Nothing Then UserID = ActiveUser.UserID
                    If UserID > 0 Then
                        If DBExtraWrite(clsExtra.Params2Extra(UserID, ecExtraType.Common, ecExtraProperty.WipeoutProjectsTimeout, value)) Then _WipeoutTimeout = value
                    End If
                End If
            End Set
        End Property
        ' D0944 ==

        ' D1382 ===
        Public Property ReviewAccountEnabled(ByVal fReviewAccountAvailable As Boolean) As Boolean
            Get
                Dim fEnabled As Boolean = False
                If ActiveUser IsNot Nothing AndAlso fReviewAccountAvailable Then
                    Dim tParam As clsExtra = DBExtraRead(clsExtra.Params2Extra(ActiveUser.UserID, ecExtraType.Common, ecExtraProperty.ReviewAccountEnabled, 0))
                    If tParam IsNot Nothing AndAlso tParam.Value.ToString = "1" Then fEnabled = True
                End If
                Return fEnabled
            End Get
            Set(ByVal value As Boolean)
                If fReviewAccountAvailable Then DBExtraWrite(clsExtra.Params2Extra(ActiveUser.UserID, ecExtraType.Common, ecExtraProperty.ReviewAccountEnabled, value))
            End Set
        End Property
        ' D1382 ==

        '' D3738 -D3741 ===
        'Public Property QuickHelp_AutoShow As Boolean
        '    Get
        '        If HasActiveProject() Then Return CBool(ActiveProject.ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_QUICKHELP_AUTOSHOW, UNDEFINED_USER_ID)) Else Return False
        '    End Get
        '    Set(value As Boolean)
        '        If HasActiveProject() Then
        '            With ActiveProject.ProjectManager
        '                .Attributes.SetAttributeValue(ATTRIBUTE_QUICKHELP_AUTOSHOW, UNDEFINED_USER_ID, AttributeValueTypes.avtBoolean, value, Guid.Empty, Guid.Empty)
        '                .Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID, UNDEFINED_USER_ID)
        '            End With
        '            'ActiveProject.onProjectSaved.Invoke(ActiveProject, "Set QuickHelp AutoShow", False, "Set as " + value.ToString)
        '        End If
        '    End Set
        'End Property
        '' D3738 ==

        ' D4920 ===
        Public Sub Antigua_ResetCredentials()
            Antigua_MeetingID = 0
            Antigua_UserEmail = ""
            Antigua_UserName = ""
            Antigua_UserID = -1
        End Sub
        ' D4920 ==

#End Region


#Region "Checkers"

        Public Sub CheckCanvasMasterDBDefaults()
            If isCanvasProjectsDBValid AndAlso Database.Connect Then
                CheckWorkgroup(SystemWorkgroup, True)

                '' D0727 ===
                'Dim tRolesExtra As clsExtra = clsExtra.Params2Extra(-1, ecExtraType.Common, ecExtraProperty.RoleGroupsVersion, "")
                'Dim tRolesExtraDB As clsExtra = DBExtraRead(tRolesExtra)
                'If tRolesExtraDB IsNot Nothing Then tRolesExtra = tRolesExtraDB
                'If tRolesExtra.Value = "" Then
                '    DebugInfo("Detect outdated role groups", _TRACE_WARNING)
                '    Dim AllWorkgroups As List(Of clsWorkgroup) = DBWorkgroupsAll(True, True)
                '    For Each tWkg As clsWorkgroup In AllWorkgroups
                '        Dim tUWList As List(Of clsUserWorkgroup) = DBUserWorkgroupsByWorkgroupID(tWkg.ID)
                '        Dim DefECAM As Integer = tWkg.GetDefaultRoleGroupID(ecRoleLevel.rlApplicationLevel, 4)    ' ECAM
                '        Dim DefAM As Integer = tWkg.GetDefaultRoleGroupID(ecRoleLevel.rlApplicationLevel, ecRoleGroupType.gtAccountManager)
                '        Dim DefSupport As Integer = tWkg.GetDefaultRoleGroupID(ecRoleLevel.rlApplicationLevel, 8)   ' Tech Support
                '        Dim DefUser As Integer = tWkg.GetDefaultRoleGroupID(ecRoleLevel.rlApplicationLevel, ecRoleGroupType.gtUser)
                '        If DefECAM >= 0 Or DefSupport >= 0 Then
                '            For Each tUW As clsUserWorkgroup In tUWList
                '                Dim fUpdated As Boolean = False
                '                If tUW.RoleGroupID = DefECAM AndAlso DefAM >= 0 Then
                '                    tUW.RoleGroupID = DefAM
                '                    fUpdated = True
                '                End If
                '                If tUW.RoleGroupID = DefSupport AndAlso DefUser >= 0 Then
                '                    tUW.RoleGroupID = DefUser
                '                    fUpdated = True
                '                End If
                '                If fUpdated Then
                '                    ' DBUserWorkgroupUpdate(tUW, False, String.Format("Re-link user to other RoleGroup ({0})", IIf(tUW.RoleGroupID = DefAM, "ECAM>AM", "TechSupprt>User")))
                '                End If
                '            Next
                '            If tWkg.Status <> ecWorkgroupStatus.wsSystem Then CheckWorkgroup(tWkg, tWkg.Status = ecWorkgroupStatus.wsSystem)
                '            If DefECAM > 0 Then
                '                Dim ECAMGroup As clsRoleGroup = tWkg.RoleGroup(DefECAM, tWkg.RoleGroups)
                '                ' DBRoleGroupDelete(ECAMGroup, String.Format("Delete role group '{0}'", ECAMGroup.Name))
                '            End If
                '            If DefSupport > 0 Then
                '                Dim SupportGroup As clsRoleGroup = tWkg.RoleGroup(DefSupport, tWkg.RoleGroups)
                '                ' DBRoleGroupDelete(SupportGroup, String.Format("Delete role group '{0}'", SupportGroup.Name))
                '            End If
                '        End If
                '    Next
                '    tRolesExtra.Value = "001"
                '    ' DBExtraWrite(tRolesExtra)
                '    UserWorkgroups = Nothing
                'End If
                '' D0727 =

                Dim fUWUpdated As Boolean = False

                Dim SQL As String = String.Format("SELECT COUNT(ID) FROM {0} WHERE {1}>={2}", _TABLE_USERS, _FLD_USERS_STATUS, clsApplicationUser._mask_CantBeDelete)
                Dim AdmCount As Object = Database.ExecuteScalarSQL(SQL)
                Dim HasAdmin As Boolean = Not IsDBNull(AdmCount) AndAlso AdmCount IsNot Nothing AndAlso CInt(AdmCount) > 0  ' D2256

                ' if 'admin' not found
                'If tList.Count = 0 Then
                If Not HasAdmin Then
                    Dim adminUser As New clsApplicationUser
                    adminUser.UserEmail = _DB_DEFAULT_ADMIN_LOGIN
                    adminUser.UserPassword = _DB_DEFAULT_ADMIN_PSW
                    adminUser.UserName = ResString("lblDefaultAdminName")
                    adminUser.Comment = "Pre-defined Administrator account" ' D0093
                    adminUser.OwnerID = 0
                    adminUser.CannotBeDeleted = True    ' D0095
                    adminUser.Status = ecUserStatus.usEnabled
                    If DBUserUpdate(adminUser, True, "Create default Admin account") Then
                        ' D6446 ===
                        If CanvasMasterDBVersion >= "0.9996" Then
                            Dim tParams As New List(Of Object)
                            tParams.Add(-9) ' Special value For Set virtual "-1"
                            tParams.Add(adminUser.UserEmail)
                            Database.ExecuteSQL(String.Format("UPDATE {0} SET PasswordStatus=? WHERE {1}=?", clsComparionCore._TABLE_USERS, clsComparionCore._FLD_USERS_EMAIL), tParams)
                        End If
                        ' D6446 ==
                        AttachWorkgroup(adminUser.UserID, SystemWorkgroup, SystemWorkgroup.GetDefaultRoleGroupID(ecRoleLevel.rlSystemLevel, ecRoleGroupType.gtAdministrator), "Auto-attach admin to system workgroup")
                        fUWUpdated = True
                    End If
                End If
                If fUWUpdated Then UserWorkgroups = Nothing
            End If
        End Sub

        ' D4606 ===
        Public Function CheckUserWorkgroups(ByRef tUser As clsApplicationUser, ByRef tWkgList As List(Of clsWorkgroup), ByRef tUWList As List(Of clsUserWorkgroup)) As Boolean
            ' D4606 ===
            If tUser Is Nothing Then Return False
            If Options.isSingleModeEvaluation OrElse Options.isLoggedInWithMeetingID Then Return True   ' D6415 + D6420
            ' D0491 ===
            Dim fNeedReloadUWList As Boolean = False
            If tWkgList Is Nothing Then tWkgList = DBWorkgroupsAll(True, True)
            If tUWList Is Nothing Then tUWList = DBUserWorkgroupsByUserID(tUser.UserID)

            Dim fCanManageAllWkg As Boolean = False
            If SystemWorkgroup IsNot Nothing Then
                Dim SystemUWG As clsUserWorkgroup = clsUserWorkgroup.UserWorkgroupByUserIDAndWorkgroupID(tUser.UserID, SystemWorkgroup.ID, tUWList)
                If SystemUWG IsNot Nothing AndAlso SystemUWG.Status <> ecUserWorkgroupStatus.uwDisabled Then
                    fCanManageAllWkg = SystemWorkgroup.RoleGroupID(ecRoleGroupType.gtAdministrator) = SystemUWG.RoleGroupID
                End If
            End If

            If tUser.CannotBeDeleted OrElse fCanManageAllWkg Then
                ' D4606 ==
                For Each tWG As clsWorkgroup In tWkgList
                    Dim tmpUW As clsUserWorkgroup = clsUserWorkgroup.UserWorkgroupByUserIDAndWorkgroupID(tUser.UserID, tWG.ID, tUWList)  ' D4606
                    If tmpUW Is Nothing Then
                        CheckWorkgroup(tWG, tWG.Status = ecWorkgroupStatus.wsSystem)
                        If AttachWorkgroup(tUser.UserID, tWG, tWG.GetDefaultRoleGroupID(ecRoleLevel.rlApplicationLevel, ecRoleGroupType.gtAdministrator)) Is Nothing AndAlso ApplicationError.Status = ecErrorStatus.errWrongLicense Then
                            ' D1994 ===
                            If tUWList.Count = 0 AndAlso Not fNeedReloadUWList AndAlso tWkgList.Count > 2 AndAlso tWG.Status <> ecWorkgroupStatus.wsSystem Then
                                LicenseInitError(ApplicationError.Message, True)
                                Return False
                                'Return ecAuthenticateError.aeWrongLicense
                            Else
                                ApplicationError.Reset()
                            End If
                            ' D1994 ==
                        Else
                            fNeedReloadUWList = True
                        End If
                        ' D4606 ===
                    Else
                        Dim RG As Integer = tWG.RoleGroupID(ecRoleGroupType.gtAdministrator)
                        If RG > 0 AndAlso RG <> tmpUW.RoleGroupID Then
                            tmpUW.RoleGroupID = RG
                            DBUserWorkgroupUpdate(tmpUW, False, String.Format("Change user permissions to System Manager ({0})", tWG.Name))
                            fNeedReloadUWList = True
                        End If
                        ' D4606 ==
                    End If
                Next
                ' D4606 ===
            Else
                For Each tmpUW As clsUserWorkgroup In tUWList
                    Dim tWG As clsWorkgroup = clsWorkgroup.WorkgroupByID(tmpUW.WorkgroupID, tWkgList)
                    If tWG IsNot Nothing Then   ' D4722
                        Dim RG_Admin As Integer = tWG.RoleGroupID(ecRoleGroupType.gtAdministrator)
                        Dim RG_WM As Integer = tWG.RoleGroupID(ecRoleGroupType.gtWorkgroupManager)
                        ' D6255 ===
                        Dim fCanBePO As Boolean = CanUserDoAction(ecActionType.at_alCreateNewModel, tmpUW, tWG)
                        If Not fCanBePO OrElse (RG_Admin > 0 AndAlso RG_Admin = tmpUW.RoleGroupID AndAlso RG_WM > 0) Then
                            Dim WSList As List(Of clsWorkspace) = DBWorkspacesByUserIDWorkgroupID(tUser.UserID, tWG.ID)
                            Dim fHasPrj As Boolean = WSList.Count > 0
                            If Not fCanBePO Then
                                Dim tLst As List(Of clsProject) = CheckProjectsList(tUser, tWG, , tmpUW, WSList)
                                fHasPrj = tLst.Count > 0
                            End If
                            If fHasPrj AndAlso fCanBePO Then
                                tmpUW.RoleGroupID = RG_WM
                                DBUserWorkgroupUpdate(tmpUW, False, String.Format("Change user permissions to Workgroup Manager ({0})", tWG.Name))
                                fNeedReloadUWList = True
                            Else
                                ' D6415 // Disable due to a spurious issue when user can be detached fully
                                'If Not fHasPrj Then
                                '    If DBUserWorkgroupDelete(tmpUW, False, String.Format("Detach user since no attached models ({0})", tWG.Name)) Then fNeedReloadUWList = True
                                'End If
                                ' D6255 ==
                            End If
                        End If
                    End If
                Next
                ' D4606 ==
            End If
            If fNeedReloadUWList Then tUWList = DBUserWorkgroupsByUserID(tUser.UserID) ' D4606
            ' D0491 ==
            Return True
        End Function
        ' D4606 ==

        Public Sub CheckWorkgroup(ByRef tWorkgroup As clsWorkgroup, ByVal fAsSystem As Boolean) ' D0491
            Dim fWorkgroupUpdated As Boolean = False
            If CanvasMasterDBVersion < "0.92" Then Exit Sub ' D0515
            If fAsSystem Then
                If SystemWorkgroup Is Nothing Then
                    tWorkgroup = New clsWorkgroup
                    tWorkgroup.Name = _DB_DEFAULT_SYSWORKGROUP_NAME
                    tWorkgroup.Status = ecWorkgroupStatus.wsSystem
                    tWorkgroup.Comment = "Default system workgroup"
                    fWorkgroupUpdated = DBWorkgroupUpdate(tWorkgroup, True, "Create system workgroup")
                    SystemWorkgroup = Nothing   ' D0612
                End If
            End If
            If Not tWorkgroup Is Nothing Then

                ' Try to reload role groups before check when list is empty
                If tWorkgroup.RoleGroups.Count = 0 Then tWorkgroup.RoleGroups = DBRoleGroupsByWorkgroupID(tWorkgroup.ID) ' D0491

                ' Check Groups and add default when is empty
                If CheckRoleGroup(ecRoleGroupType.gtAdministrator, tWorkgroup) Then fWorkgroupUpdated = True
                If tWorkgroup.Status = ecWorkgroupStatus.wsSystem Then
                    If CheckRoleGroup(ecRoleGroupType.gtECAccountManager, tWorkgroup) Then fWorkgroupUpdated = True
                    If CheckRoleGroup(ecRoleGroupType.gtTechSupport, tWorkgroup) Then fWorkgroupUpdated = True ' -D0727 + D3288
                End If
                If tWorkgroup.Status = ecWorkgroupStatus.wsEnabled Then
                    If CheckRoleGroup(ecRoleGroupType.gtUser, tWorkgroup) Then fWorkgroupUpdated = True
                    If CheckRoleGroup(ecRoleGroupType.gtEvaluator, tWorkgroup) Then fWorkgroupUpdated = True
                    If CheckRoleGroup(ecRoleGroupType.gtViewer, tWorkgroup) Then fWorkgroupUpdated = True ' D2239
                    If CheckRoleGroup(ecRoleGroupType.gtEvaluatorAndViewer, tWorkgroup) Then fWorkgroupUpdated = True ' D2857
                    If CheckRoleGroup(ecRoleGroupType.gtProjectManager, tWorkgroup) Then fWorkgroupUpdated = True ' D2780
                    If CheckRoleGroup(ecRoleGroupType.gtProjectOrganizer, tWorkgroup) Then fWorkgroupUpdated = True ' D2780
                    If CheckRoleGroup(ecRoleGroupType.gtWorkgroupManager, tWorkgroup) Then fWorkgroupUpdated = True ' D2780
                End If
            End If
            If fWorkgroupUpdated Then tWorkgroup = DBWorkgroupByID(tWorkgroup.ID, True, True)
        End Sub

        Private Function CheckRoleGroup(ByVal GroupType As ecRoleGroupType, ByRef tWorkgroup As clsWorkgroup) As Boolean
            Dim fUpdated As Boolean = False
            Dim RG As clsRoleGroup
            RG = tWorkgroup.RoleGroup(GroupType)
            Dim tOldWkg As clsWorkgroup = _CurrentWorkgroup ' D7189
            _CurrentWorkgroup = tWorkgroup  ' D7189
            If RG Is Nothing Then
                RG = New clsRoleGroup
                RG.WorkgroupID = tWorkgroup.ID
                RG.GroupType = GroupType
                RG.Status = ecRoleGroupStatus.gsEnabled
                Select Case GroupType
                    Case ecRoleGroupType.gtAdministrator
                        RG.RoleLevel = ecRoleLevel.rlApplicationLevel
                        If Not tWorkgroup Is Nothing AndAlso tWorkgroup.Status = ecWorkgroupStatus.wsSystem Then RG.RoleLevel = ecRoleLevel.rlSystemLevel ' D0513
                        RG.Status = ecRoleGroupStatus.gsAdministratorDefault    ' D0068
                        ' D0087 ===

                        ' -D2780 + D3288
                    Case ecRoleGroupType.gtTechSupport
                        RG.RoleLevel = ecRoleLevel.rlSystemLevel
                        RG.Status = ecRoleGroupStatus.gsTechSupportDefault

                    Case ecRoleGroupType.gtECAccountManager
                        RG.RoleLevel = ecRoleLevel.rlSystemLevel
                        RG.Status = ecRoleGroupStatus.gsECAccountManagerDefault
                        ' D0114 + D2780 ===
                    Case ecRoleGroupType.gtProjectOrganizer
                        RG.RoleLevel = ecRoleLevel.rlApplicationLevel   ' D0727
                        RG.Status = ecRoleGroupStatus.gsProjectOrganizerDefault
                        ' D0114 ==
                    Case ecRoleGroupType.gtWorkgroupManager
                        RG.RoleLevel = ecRoleLevel.rlApplicationLevel
                        RG.Status = ecRoleGroupStatus.gsWorkgroupManagerDefault
                    Case ecRoleGroupType.gtUser
                        RG.RoleLevel = ecRoleLevel.rlApplicationLevel
                        RG.Status = ecRoleGroupStatus.gsUserDefault     ' D0052
                    Case ecRoleGroupType.gtProjectManager
                        RG.RoleLevel = ecRoleLevel.rlModelLevel
                        RG.Status = ecRoleGroupStatus.gsProjectManagerDefault ' D0052
                        ' D0087 + D2780 ==
                    Case ecRoleGroupType.gtEvaluator
                        RG.RoleLevel = ecRoleLevel.rlModelLevel
                        RG.Status = ecRoleGroupStatus.gsEvaluatorDefault
                        ' D2239 ===
                    Case ecRoleGroupType.gtViewer
                        RG.RoleLevel = ecRoleLevel.rlModelLevel
                        RG.Status = ecRoleGroupStatus.gsViewerDefault
                        ' D2239 ==
                        ' D2857 ===
                    Case ecRoleGroupType.gtEvaluatorAndViewer
                        RG.RoleLevel = ecRoleLevel.rlModelLevel
                        RG.Status = ecRoleGroupStatus.gsEvaluatorAndViewerDefault
                        ' D2239 ==
                End Select
                If RG.Status <> ecRoleGroupStatus.gsEnabled And RG.Name = "" Then RG.Name = ResString(String.Format(ResString("lblProps_Template"), RG.GroupType.ToString)) ' D0087
                If DBRoleGroupUpdate(RG, True, String.Format("Restore default RoleGroup '{0}'", ResString("lbl_" + RG.Status.ToString))) Then   ' D0499
                    tWorkgroup.RoleGroups.Add(RG)
                    fUpdated = True
                End If
            Else
                ' D0513 ===
                If Not RG Is Nothing AndAlso RG.Status = ecRoleGroupStatus.gsAdministratorDefault AndAlso Not tWorkgroup Is Nothing AndAlso tWorkgroup.Status = ecWorkgroupStatus.wsSystem AndAlso RG.RoleLevel <> ecRoleLevel.rlSystemLevel Then
                    RG.RoleLevel = ecRoleLevel.rlSystemLevel
                    DBRoleGroupUpdate(RG, False, String.Format("Fix default '{0}' system workgroup role level", ResString("lbl_" + RG.Status.ToString)))
                    fUpdated = True
                End If

                ' D0727 ===
                If CurrentLanguage IsNot Nothing Then   ' D0728
                    Dim sDefName As String = ResString(String.Format(ResString("lblProps_Template"), RG.GroupType.ToString))
                    If RG IsNot Nothing AndAlso (RG.Name <> sDefName OrElse (RG.GroupType = ecRoleGroupType.gtProjectOrganizer AndAlso RG.RoleLevel <> ecRoleLevel.rlApplicationLevel)) Then
                        RG.Name = sDefName
                        If RG.GroupType = ecRoleGroupType.gtProjectOrganizer AndAlso RG.RoleLevel <> ecRoleLevel.rlApplicationLevel Then RG.RoleLevel = ecRoleLevel.rlApplicationLevel
                        DBRoleGroupUpdate(RG, False, String.Format("Update Role Group name up to default '{0}'", sDefName))
                        fUpdated = True
                    End If
                    ' D0727 ==
                    If RG.Status <> ecRoleGroupStatus.gsEnabled And RG.Name = "" Then RG.Name = ResString(String.Format(ResString("lblProps_Template"), RG.GroupType.ToString)) ' D0087
                End If
                ' D0513 ==
            End If
            Dim Actions() As ecActionType
            Select Case GroupType
                Case ecRoleGroupType.gtAdministrator
                    ' D0087 ===
                    Actions = _DEFROLE_ADMINISTRATOR

                    ' -D2780 + D3288
                Case ecRoleGroupType.gtTechSupport
                    Actions = _DEFROLE_TECHSUPPORT
                Case ecRoleGroupType.gtECAccountManager
                    Actions = _DEFROLE_ECACCOUNTMANAGER

                    ' D2780 ===
                Case ecRoleGroupType.gtWorkgroupManager
                    Actions = _DEFROLE_WORKGROUPMANAGER
                Case ecRoleGroupType.gtProjectOrganizer    ' D0114
                    Actions = _DEFROLE_PROJECTORGANIZER
                    ' D2780 ==

                Case ecRoleGroupType.gtUser
                    Actions = _DEFROLE_USER
                Case ecRoleGroupType.gtProjectManager   ' D2780
                    Actions = _DEFROLE_PROJECTMANAGER   ' D2780
                    ' D0087 ==
                Case ecRoleGroupType.gtEvaluator
                    Actions = _DEFROLE_EVALUATOR
                Case ecRoleGroupType.gtViewer   ' D2239
                    Actions = _DEFROLE_VIEWER   ' D2239
                Case ecRoleGroupType.gtEvaluatorAndViewer    ' D2857
                    Actions = _DEFROLE_EVALUATOR_N_VIEWER    ' D2857
                Case Else
                    Actions = Nothing
            End Select
            If Not Actions Is Nothing Then
                ' Check Actions list for restore or update
                ' D0190 ===
                For Each tAct As ecActionType In Actions
                    If RG.ActionStatus(tAct) = ecActionStatus.asUnspecified Then
                        Dim tAction As New clsRoleAction
                        tAction.ActionType = tAct
                        tAction.RoleGroupID = RG.ID
                        tAction.Status = ecActionStatus.asGranted
                        If DBRoleActionUpdate(tAction, True, String.Format("Restore RoleAction '{1}' for '{0}'", RG.Name, ResString("lbl_" + tAct.ToString))) Then ' D0499
                            RG.Actions.Add(tAction)
                            fUpdated = True
                        End If
                    End If
                Next
                ' D0190 ==

                ' Check actions over default
                ' D0727 ===
                For Each tAct As clsRoleAction In RG.Actions
                    If String.IsNullOrEmpty(tAct.Comment) AndAlso Array.IndexOf(Actions, tAct.ActionType) < 0 Then  ' D0967
                        DBRoleActionDelete(tAct, String.Format("Remove RoleAction '{0}' over default", ResString("lbl_" + tAct.ActionType.ToString)))
                        fUpdated = True
                    End If
                Next
                ' D0727 ==

            End If ' if not Action is Nothing
            _CurrentWorkgroup = tOldWkg ' D7189
            Return fUpdated
        End Function

        ' D7001 ===
        Public Function CheckProjectManagerUserAsActive(ByVal tProject As clsProject) As Boolean
            Dim fResult As Boolean = False
            ' D6973 ===
            If tProject.ProjectManager.User Is Nothing AndAlso ActiveUser IsNot Nothing Then
                Dim _prjUser As clsUser = tProject.ProjectManager.GetUserByEMail(ActiveUser.UserEmail)
                ' D6987 ===
                If _prjUser Is Nothing AndAlso ActiveUser IsNot Nothing Then
                    _prjUser = tProject.ProjectManager.AddUser(ActiveUser.UserEmail, True)
                    If _prjUser IsNot Nothing Then
                        _prjUser.UserName = ActiveUser.UserEmail
                        tProject.SaveUsersInfo(_prjUser, "Add users to project",,, _prjUser.UserEMail)
                    End If
                End If
                ' D6987 ==
                If _prjUser IsNot Nothing Then tProject.ProjectManager.User = _prjUser
                fResult = tProject.ProjectManager.User IsNot Nothing
            End If
            ' D6973 ==

            Return fResult
        End Function
        ' D7001 ==

        ' D0483 ===
        ''' <summary>
        ''' This function will check attached from App users in Project manager and will add missed users to decision. Also checking the PM users and attach missing when possible
        ''' </summary>
        ''' <param name="tProject"></param>
        ''' <remarks></remarks>
        Public Function CheckProjectManagerUsers(ByVal tProject As clsProject, Optional ImportUsersFromPM As Boolean = True) As Boolean   ' D6600 + D6725
            ECCore.MiscFuncs.PrintDebugInfo("******* CheckProjectManagerUsers started - ")

            Dim fResult As Boolean = False
            Dim Lst As List(Of clsApplicationUser) = Nothing
            If Not tProject Is Nothing Then
                Lst = DBUsersByProjectID(tProject.ID)
                Dim fHasEmptyNames As Boolean = tProject.ProjectManager.UsersList.Find(Function(u) u.UserName = "" OrElse u.UserName.ToLower = u.UserEMail.ToLower) IsNot Nothing   ' D6725
                If Lst IsNot Nothing AndAlso (Lst.Count <> tProject.ProjectManager.UsersList.Count OrElse fHasEmptyNames) Then   ' D0828 + D0830 + D0843 + D6600 + D6725
                    Dim fHasChanged As Boolean = False
                    Dim sLogMsg As String = ""  ' D6600
                    For Each tAppUser As clsApplicationUser In Lst
                        Dim tPrjUser As clsUser = tProject.ProjectManager.GetUserByEMail(tAppUser.UserEmail)
                        If tPrjUser Is Nothing Then
                            tProject.ProjectManager.AddUser(tAppUser.UserEmail, True, tAppUser.UserName)
                            sLogMsg += String.Format("{0}{1}", If(sLogMsg = "", "", ","), tAppUser.UserEmail)
                            fHasChanged = True
                        Else
                            If tAppUser.UserName <> "" AndAlso (tPrjUser.UserName.Trim = "" OrElse tPrjUser.UserName.ToLower = tPrjUser.UserEMail.ToLower) AndAlso Not tPrjUser.UserName.Equals(tAppUser.UserName, StringComparison.CurrentCultureIgnoreCase) Then  ' D7000
                                tPrjUser.UserName = tAppUser.UserName
                                fHasChanged = True
                            End If
                        End If
                    Next
                    If fHasChanged Then
                        tProject.ProjectManager.StorageManager.Writer.SaveModelStructure()
                        If sLogMsg <> "" Then DBSaveLog(dbActionType.actModify, dbObjectType.einfProject, tProject.ID, "Restore/Update project users", sLogMsg) ' D6600
                        fResult = True
                        CheckProjectManagerUserAsActive(tProject)
                    End If
                    ' D6600 ===
                    If ImportUsersFromPM Then   ' D6725
                        Dim fAttached As Integer = ImportUsersFromProjectManager(tProject, ActiveUser, tProject.ProjectManager.UsersList, False, False)
                        If fAttached > 0 Then
                            'DBSaveLog(dbActionType.actModify, dbObjectType.einfProject, tProject.ID, "Attach users from PM", fAttached.ToString)   ' already saved on scan
                            fResult = True
                        End If
                    End If
                    ' D6600 ==
                End If
                If fResult Then SnapshotSaveProject(ecSnapShotType.Auto, "Restore users list", tProject.ID, False)
            End If
            ECCore.MiscFuncs.PrintDebugInfo("******* CheckProjectManagerUsers ended - ")
            Return fResult
        End Function
        ' D0483 ==

        ' D0503 ===
        Public Function CheckUsersListBeforeUpload(ByVal sUploadedFilename As String, ByVal ConnectionString As String, ByVal ProviderType As DBProviderType, ByVal StorageType As ECModelStorageType, ByVal fCheckUserEmails As Boolean, ByRef sError As String, fShowMaxUsersWarning As Boolean) As Boolean ' D0345 + D0378 + D1519
            Dim fPassed As Boolean = False
            sError = ""

            Dim UsersList As List(Of clsUser) = Nothing

            ' D2132 ===
            If StorageType = ECModelStorageType.mstTextFile Then
                Dim sFile As String = My.Computer.FileSystem.ReadAllText(sUploadedFilename)
                Dim tmpUsers As List(Of clsUser) = clsTextModel.ReadUsers(sFile, sError)
                If tmpUsers IsNot Nothing Then
                    UsersList = New List(Of clsUser)
                    For Each tUser As clsUser In tmpUsers
                        UsersList.Add(tUser)
                    Next
                End If
            Else
                ' D2132 ==
                ' D0378 ===
                Dim FS As IO.FileStream = Nothing
                Try
                    FS = New IO.FileStream(sUploadedFilename, FileMode.Open, FileAccess.Read) ' D0387
                    UsersList = MiscFuncs.GetUsersList(ConnectionString, StorageType, ProviderType, -1, FS)    ' D0152 + D0329
                    ' D0378 ==
                Catch ex As Exception
                    sError = ex.Message
                    UsersList = Nothing
                    DebugInfo(ex.Message, _TRACE_RTE)   ' D0330
                Finally
                    If Not FS Is Nothing Then FS.Close()
                    ' D0378 ==
                End Try
            End If

            'Dim AllUsers As List(Of clsApplicationUser) = DBUsersAll()  ' D0803

            ' D1489 ===
            Dim mTotalUsers As Integer = 0
            Dim mPossibleNewUsers As Integer = 0
            Dim mMaxWorkgroupUsers As Long = UNLIMITED_VALUE
            If fShowMaxUsersWarning AndAlso ActiveWorkgroup IsNot Nothing AndAlso ActiveWorkgroup.License.isValidLicense Then mMaxWorkgroupUsers = ActiveWorkgroup.License.GetParameterMaxByID(ecLicenseParameter.MaxUsersInWorkgroup) ' D1519
            ' D1489 ==

            Dim fHasEmptyEmail As Boolean = False   ' D3191
            If Not UsersList Is Nothing Then
                Dim NotPassedUsers As String = ""
                For Each tUser As clsUser In UsersList
                    If CheckProjectUserCouldBeImported(tUser, ActiveUser) Then    ' D0085 + D0129 + D0503
                        Dim tAppUser As clsApplicationUser = Nothing    ' D0503
                        'Dim tAppUser As clsApplicationUser = clsApplicationUser.UserByUserEmail(tUser.UserEMail, AllUsers) ' D0803
                        If Not CheckProjectUserEmail(tUser, tAppUser, fCheckUserEmails) Then  ' D0345 + D0503 + D0520
                            ' D0811 ===
                            If _USE_USER_EMAIL_FOR_EMPTY_FACILITATOR AndAlso tUser.UserID = 0 AndAlso tUser.UserName.ToLower = "facilitator" AndAlso MiscFuncs.GetUserByEmail(UsersList, ActiveUser.UserEmail) Is Nothing Then  ' D0916
                                ' skip this facilitator
                                If ActiveUser IsNot Nothing Then tUser.UserEMail = ActiveUser.UserEmail
                            Else
                                ' D0811 ==
                                Dim sName As String = tUser.UserName
                                If tUser.UserEMail <> "" AndAlso sName.ToLower <> tUser.UserEMail Then sName = String.Format("{0} ({1})", sName, tUser.UserEMail) ' D3168
                                If sName = "" Then sName = String.Format("id#{0}", tUser.UserID)
                                NotPassedUsers += String.Format("<li>{0}</li>", sName)
                                If tUser.UserEMail = "" Then fHasEmptyEmail = True ' D3191
                            End If
                        End If
                        mTotalUsers += 1
                        If mMaxWorkgroupUsers > 0 AndAlso tUser IsNot Nothing AndAlso tUser.UserEMail <> "" Then
                            Dim fNewUser As Boolean = True
                            Dim tmpUser As clsApplicationUser = tAppUser
                            If tmpUser Is Nothing Then tmpUser = DBUserByEmail(tUser.UserEMail)
                            If tmpUser IsNot Nothing Then
                                If ActiveUser IsNot Nothing AndAlso ActiveUser.UserID = tmpUser.UserID Then
                                    fNewUser = False
                                Else
                                    If DBUserWorkgroupByUserIDWorkgroupID(tmpUser.UserID, ActiveWorkgroup.ID) IsNot Nothing Then fNewUser = False
                                End If
                            End If
                            If fNewUser Then mPossibleNewUsers += 1
                        End If
                    End If
                Next
                If NotPassedUsers = "" Then

                    ' D0213 ===
                    Dim sDuplicates As String = ""
                    Dim FoundUsers As New ArrayList
                    For i As Integer = 0 To UsersList.Count - 2
                        Dim tUser1 As clsUser = CType(UsersList(i), clsUser)
                        If Not FoundUsers.Contains(tUser1.UserEMail.ToLower) Then
                            Dim Dups As String = ""
                            For j As Integer = i + 1 To UsersList.Count - 1
                                Dim tUser2 As clsUser = CType(UsersList(j), clsUser)
                                If String.Equals(tUser1.UserEMail, tUser2.UserEMail, StringComparison.InvariantCultureIgnoreCase) Then
                                    Dups += String.Format("; #{0}", tUser2.UserID)
                                    FoundUsers.Add(tUser2.UserEMail.ToLower)
                                End If
                            Next
                            If Dups <> "" Then
                                Dups = String.Format("#{0}{1}", tUser1.UserID, Dups)
                                sDuplicates += String.Format("<li>{0} ({1})</li>", tUser1.UserEMail, Dups)
                            End If
                        End If
                    Next

                    If sDuplicates = "" Then
                        'fPassed = True ' -D1519
                    Else
                        sError = String.Format(ResString("msgDuplicatesEmailsList"), sDuplicates)
                        fPassed = False
                    End If
                    ' D0213 ==

                Else
                    ' D3191 ===
                    If fHasEmptyEmail Then
                        sError = String.Format(ResString("msgWrongOrEmptyEmailsList"), NotPassedUsers)
                    Else
                        sError = String.Format(ResString("msgWrongEmailsList"), NotPassedUsers, ResString("lblOptionCheckEmailsOnUpload"))  ' D3168
                    End If
                    ' D3191 ==
                End If

                ' D1519 ===
                If sError = "" AndAlso ActiveWorkgroup IsNot Nothing AndAlso fShowMaxUsersWarning Then
                    If mMaxWorkgroupUsers > 0 AndAlso mMaxWorkgroupUsers < (ActiveWorkgroup.License.GetParameterValueByID(ecLicenseParameter.MaxUsersInWorkgroup) + mPossibleNewUsers) Then
                        sError = LicenseErrorMessage(ActiveWorkgroup.License, ecLicenseParameter.MaxUsersInWorkgroup, True)
                        ApplicationError.Init(ecErrorStatus.errWrongLicense, -1, sError)    ' D1519
                        ApplicationError.CustomData = ecLicenseParameter.MaxUsersInWorkgroup.ToString ' D1519
                    Else
                        Dim mMaxPrjUsers As Long = ActiveWorkgroup.License.GetParameterMaxByID(ecLicenseParameter.MaxUsersInProject) ' D1530
                        If mMaxPrjUsers > 0 AndAlso mMaxPrjUsers < mTotalUsers Then ' D1530
                            sError = LicenseErrorMessage(ActiveWorkgroup.License, ecLicenseParameter.MaxUsersInProject, True)
                            ApplicationError.Init(ecErrorStatus.errWrongLicense, -1, sError)    ' D1519
                            ApplicationError.CustomData = ecLicenseParameter.MaxUsersInProject.ToString ' D1519
                        End If
                    End If
                End If
                ' D1519 ==
            Else
                If sError = "" Then sError = ResString("msgWrongUploadFile")
            End If
            If sError = "" Then fPassed = True ' D1519
            Return fPassed
        End Function
        ' D0503 ==

#End Region


#Region "Resources and Languages"

        ' D0030 ===
        Public Function ResString(ByVal sResourceName As String, Optional ByVal fAsIsIfMissed As Boolean = False) As String
            ' D0461 ===
            Dim sRes As String = ""
            Dim fExisted As Boolean = False
            If Not CurrentLanguage Is Nothing Then
                sRes = CurrentLanguage.GetString(sResourceName, "", fExisted)
            End If
            If (Not fExisted Or CurrentLanguage Is Nothing) AndAlso CurrentLanguage IsNot DefaultLanguage AndAlso DefaultLanguage IsNot Nothing Then    ' D0144
                sRes = DefaultLanguage.GetString(sResourceName, "", fExisted)
            End If
            If Not fExisted Then sRes = CStr(IIf(fAsIsIfMissed, sResourceName, String.Format(_RES_WARNING_MSG, sResourceName)))
            ' D0461 ==
            Return sRes
        End Function

        ''' <summary>
        ''' Scan folder for .resx files and try use each as language-file
        ''' </summary>
        ''' <param name="sPath">Folder name (or local path) for seeking .resx files</param>
        ''' <remarks></remarks>
        Public Shared Function LanguagesScanFolder(ByVal sPath As String) As List(Of clsLanguageResource)
            Dim Languages As New List(Of clsLanguageResource)
            Try
                Dim AllFiles As System.Collections.ObjectModel.ReadOnlyCollection(Of String) = My.Computer.FileSystem.GetFiles(sPath)
                DebugInfo(String.Format("Scan language files '{0}'", _FILE_RESOURCE_EXT))
                For Each sFileName As String In AllFiles
                    If Path.GetExtension(sFileName).ToLower = _FILE_RESOURCE_EXT And Not Path.GetFileName(sFileName).StartsWith("~") Then ' D0210 + D0219
                        Dim Lng As New clsLanguageResource
                        Lng.ResxFilename = sFileName
                        If Lng.isLoaded Then
                            Languages.Add(Lng)
                            DebugInfo(String.Format("Language file '{0}' loaded", sFileName))
                        Else
                            DebugInfo(String.Format("Couldn't load language file '{0}'!", sFileName), _TRACE_WARNING)
                        End If
                    End If
                Next
            Catch ex As Exception
                DebugInfo(ex.Message, _TRACE_RTE)   ' D0330
            End Try
            Return Languages
        End Function

        ' D0461 ===
        Public Property CurrentLanguage() As clsLanguageResource
            Get
                If _CurrentLanguage Is Nothing Then
                    _CurrentLanguage = clsLanguageResource.LanguageByCode(LanguageCode, Languages)
                    If _CurrentLanguage Is Nothing Then _CurrentLanguage = DefaultLanguage
                End If
                Return _CurrentLanguage
            End Get
            Set(ByVal value As clsLanguageResource)
                _CurrentLanguage = value
            End Set
        End Property

        Public Property DefaultLanguage() As clsLanguageResource
            Get
                If _DefaultLanguage Is Nothing Then _DefaultLanguage = clsLanguageResource.LanguageByCode(_LANG_DEFCODE, Languages)
                Return _DefaultLanguage
            End Get
            Set(ByVal value As clsLanguageResource)
                _DefaultLanguage = value
            End Set
        End Property
        ' D0461 ==

        Public Property LanguageCode() As String
            Get
                Return _LanguageCurrentCode
            End Get
            Set(ByVal value As String)
                ' D0461 ===
                If _LanguageCurrentCode.ToLower <> value.ToLower Then
                    _LanguageCurrentCode = value
                    _CurrentLanguage = Nothing
                End If
                ' D0461 ==
            End Set
        End Property

        Public ReadOnly Property Languages() As List(Of clsLanguageResource)
            Get
                If _LanguagesList Is Nothing Then
                    _LanguagesList = LanguagesScanFolder(_FILE_DATA_RESX)   ' D0461
                End If
                Return _LanguagesList
            End Get
        End Property
        ' D0030 ==

        ' D0150  + D0461 ===
        Public Sub ResetLanguages(Optional ByVal fReloadLanguagesList As Boolean = False)
            If fReloadLanguagesList Then _LanguagesList = Nothing
            _DefaultLanguage = Nothing
            _CurrentLanguage = Nothing
        End Sub
        ' D0150 + D0461 ==

#End Region


#Region "Users"

        ' D0466 ===
        Private Sub SetActiveUser(ByVal tUser As clsApplicationUser)
            If Not _CurrentUser Is tUser Then
                If tUser Is Nothing Then
                    isAlexaUser = False     ' D7584
                    DebugInfo("Reset Active user", _TRACE_INFO)
                Else
                    If tUser.Session IsNot Nothing Then tUser.Session.LastAccess = Now  ' D0494 + D7205
                    DebugInfo(String.Format("Set Active user as '{0}'", tUser.UserEmail), _TRACE_INFO)  ' D0471
                    ' D2213 ===
                    'DBUpdateDateTime(_TABLE_USERS, _FLD_USERS_LASTVISITED, tUser.UserID, _FLD_USERS_ID) ' D0491
                    If Database.Connect Then
                        Dim tParams As New List(Of Object)
                        tParams.Add(Now)
                        tParams.Add(tUser.UserID)
                        Try
                            Database.ExecuteSQL(String.Format("UPDATE {0} SET {1}=?{3} WHERE {2}=?", _TABLE_USERS, _FLD_USERS_LASTVISITED, _FLD_USERS_ID, IIf(tUser.PasswordStatus > 0 AndAlso tUser.PasswordStatus < _DEF_PASSWORD_ATTEMPTS AndAlso CanvasMasterDBVersion >= "0.9996", ", PasswordStatus=0", "")), tParams)    ' D2215
                        Catch ex As Exception
                        End Try
                    End If
                    ' D2213 ==
                End If
                _CurrentUser = tUser
                _EULA_checked = False   ' D0473
                _EULA_valid = True      ' D0473
                _CurrentUserWorkgroup = Nothing
                _CurrentWorkspace = Nothing
                _UserWorkgroups = Nothing
                _UserTemplates = Nothing    ' D0795
                _Workspaces = Nothing
                _CurrentProjectGroup = Nothing
                _CurrentRoleGroup = Nothing
            End If
        End Sub
        ' D0466 ==

        Public Property ActiveUser() As clsApplicationUser
            Get
                Return _CurrentUser
            End Get
            Set(ByVal value As clsApplicationUser)
                SetActiveUser(value)    ' D0466
            End Set
        End Property

        Public ReadOnly Property isAuthorized() As Boolean
            Get
                'Return Not ActiveUser Is Nothing AndAlso Not ActiveWorkgroup Is Nothing
                Return Not ActiveUser Is Nothing    ' D0476
            End Get
        End Property

        ' D9402 ===
        Public ReadOnly Property isAntiguaAuthorized As Boolean
            Get
                Return Antigua_MeetingID > 0 AndAlso Antigua_UserEmail <> ""
            End Get
        End Property
        ' D9402 ==

        ' D0092 ===
        Public Function UserWithSignup(ByVal sUserEmail As String, Optional ByVal sUserName As String = "", Optional ByVal sUserPassword As String = "", Optional ByVal sComment As String = "", Optional ByRef fExistedUserEmail As String = Nothing, Optional fAskNewPassword As Boolean = True) As clsApplicationUser  ' D0828 + D2215 + D2868
            sUserEmail = sUserEmail.Trim    ' D0225
            Dim newUser As clsApplicationUser = DBUserByEmail(sUserEmail)  ' D0151 + D0466
            'A0259 === If fExisteduserEmail IsNot Nothing Then fExisteduserEmail = CStr(IIf(newUser IsNot Nothing, newUser.UserEmail, "")) ' D0828
            If fExistedUserEmail IsNot Nothing Then
                If newUser IsNot Nothing Then fExistedUserEmail = newUser.UserEmail Else fExistedUserEmail = ""
            End If
            'A0259 ==
            If newUser Is Nothing And sUserEmail <> "" Then
                newUser = New clsApplicationUser
                newUser.UserEmail = sUserEmail
                If sUserName = "" Then sUserName = sUserEmail
                newUser.UserName = sUserName
                newUser.UserPassword = sUserPassword
                newUser.Comment = sComment  ' D0226
                If fAskNewPassword Then newUser.PasswordStatus = -1 ' D2215
                If Not ActiveUser Is Nothing Then newUser.OwnerID = ActiveUser.UserID
                DBUserUpdate(newUser, True, "Fast user sign-up")    ' D0473
            End If
            Return newUser  'D0466
        End Function
        ' D0092 ==

        ' D0475 ===
        Public ReadOnly Property OwnerUser(ByVal OwnerID As Integer) As clsApplicationUser
            Get
                If OwnerID <= 0 Then Return Nothing
                If Not ActiveUser Is Nothing Then If ActiveUser.UserID = OwnerID Then Return ActiveUser
                Return DBUserByID(OwnerID)
            End Get
        End Property

        Public ReadOnly Property OwnerEmail(ByVal OwnerID As Integer) As String
            Get
                Dim tUser As clsApplicationUser = OwnerUser(OwnerID)
                If tUser Is Nothing Then Return "" Else Return tUser.UserEmail
            End Get
        End Property
        ' D0475 ==

#End Region


#Region "Projects"

        ' D0466 ===
        Private Sub SetActiveProject(ByRef tProject As clsProject)
            If (tProject Is Nothing AndAlso _ProjectID <> -1) OrElse (tProject IsNot Nothing AndAlso tProject.ID <> _ProjectID) Then ' D0471 + D0511 + D0830
                ActiveSurveysList = Nothing    ' D1148
                ' D0511 ===
                If HasActiveProject() AndAlso ActiveProject IsNot Nothing AndAlso ActiveProject.IsProjectLoaded Then
                    'ActiveProject.ProjectManager.CloseProject()
                    ActiveProject.ProjectManager = Nothing
                End If
                ' D0511 ==
                If tProject Is Nothing Then
                    If _ProjectID > 0 Then DBSaveLog(dbActionType.actClose, dbObjectType.einfProject, _ProjectID, "", "", If(ActiveUser Is Nothing, -1, ActiveUser.UserID), If(ActiveWorkgroup Is Nothing, -1, ActiveWorkgroup.ID)) ' D2236 + D6635
                    DebugInfo("Reset active project", _TRACE_INFO)
                    ' D3554 ===
                    If _ProjectID > 0 Then
                        DBUpdateDateTime(_TABLE_PROJECTS, _FLD_PROJECTS_LASTVISITED, _ProjectID, _FLD_PROJECTS_ID)
                        If ActiveWorkgroup IsNot Nothing Then DBUpdateDateTime(_TABLE_WORKGROUPS, _FLD_WORKGROUPS_LASTVISITED, ActiveWorkgroup.ID, _FLD_WORKGROUPS_ID)
                        If ActiveUserWorkgroup IsNot Nothing Then DBUpdateDateTime(_TABLE_USERWORKGROUPS, _FLD_USERWRKG_LASTVISITED, ActiveUserWorkgroup.ID, _FLD_USERWRKG_ID)
                        If ActiveWorkspace IsNot Nothing Then DBUpdateDateTime(_TABLE_WORKSPACE, _FLD_WORKSPACE_LASTMODIFY, ActiveWorkspace.ID, _FLD_WORKSPACE_ID)  ' D6443
                        If ActiveUser IsNot Nothing Then DBUpdateDateTime(_TABLE_USERS, _FLD_USERS_LASTVISITED, ActiveUser.UserID, _FLD_USERS_ID)
                    End If
                    ' D3554 ==
                    _ProjectID = -1     ' D0511
                    _LastSnapshot = Nothing ' D3726
                Else
                    _LastSnapshot = Nothing     ' D3726
                    DBSaveLog(dbActionType.actOpen, dbObjectType.einfProject, tProject.ID, "", "")  ' D2236
                    ' D4956 ===
                    If _TEAMTIME_FINISH_ON_OPEN_OTHER_MODEL AndAlso Workspaces IsNot Nothing AndAlso isAuthorized Then
                        'For Each tWS As clsWorkspace In Workspaces
                        '    If tWS.ProjectID <> tProject.ID Then
                        '        If tWS.isInTeamTime(False) OrElse (tProject.IsRisk AndAlso tWS.isInTeamTime(True)) Then
                        '            Dim tmpPrj As clsProject = clsProject.ProjectByID(tWS.ProjectID, ActiveProjectsList)
                        '            If tmpPrj IsNot Nothing AndAlso tmpPrj.WorkgroupID = tProject.WorkgroupID Then
                        '                If tmpPrj.MeetingOwnerID = ActiveUser.UserID Then
                        '                    DebugInfo("Finish TT session for other model")
                        '                    TeamTimeEndSession(tmpPrj, False)
                        '                End If
                        '            End If
                        '        End If
                        '    End If
                        'Next
                        If _ProjectID > 0 Then
                            Dim oldPrj As clsProject = clsProject.ProjectByID(_ProjectID, ActiveProjectsList)
                            If oldPrj IsNot Nothing AndAlso tProject.WorkgroupID = oldPrj.WorkgroupID Then
                                Dim oldWS As clsWorkspace = clsWorkspace.WorkspaceByUserIDAndProjectID(ActiveUser.UserID, _ProjectID, Workspaces)
                                If oldWS IsNot Nothing AndAlso oldWS.isInTeamTime(False) OrElse (tProject.IsRisk AndAlso oldWS.isInTeamTime(True)) Then
                                    If oldPrj.MeetingOwnerID = ActiveUser.UserID Then
                                        DebugInfo("Finish TT session for other model")
                                        TeamTimeEndSession(oldPrj, False)
                                    End If
                                End If
                            End If
                        End If
                    End If
                    ' D4956 ==
                    ' D1709 ===
                    Dim fUpdated As Boolean = False
                    If CanvasMasterDBVersion >= "0.98" AndAlso tProject.CheckGUID Then fUpdated = True
                    'If isRiskEnabled AndAlso CanvasMasterDBVersion >= "0.9991" AndAlso (tProject.PasscodeImpact = "" OrElse tProject.PasscodeImpact = tProject.PasscodeLikelihood) Then ' D2291
                    If CanvasMasterDBVersion >= "0.9991" AndAlso (tProject.PasscodeImpact = "" OrElse tProject.PasscodeImpact = tProject.PasscodeLikelihood) Then ' D2291 + D2898
                        tProject.PasscodeImpact = ProjectUniquePasscode("", -1)
                        tProject.PasscodeImpact = ProjectUniquePasscode(tProject.PasscodeImpact, tProject.ID)   ' D3686
                        fUpdated = True
                    End If
                    'If isRiskEnabled AndAlso CanvasMasterDBVersion >= "0.9991" AndAlso (tProject.MeetingIDImpact(True) <= 0) Then   ' D2291
                    If CanvasMasterDBVersion >= "0.9991" AndAlso (tProject.MeetingIDImpact(True) <= 0) Then   ' D2291 + D2898
                        tProject.MeetingIDImpact = clsMeetingID.ReNew
                        fUpdated = True
                    End If
                    If fUpdated Then DBProjectUpdate(tProject, False, "Init project GUID/Passcode/MeeingID") ' D0892 + D0894 + D2898
                    'If tProject.MeetingIDImpact(True) > 0 Then DBProjectByMeetingID(tProject.MeetingIDImpact(True), tProject.ID) ' D3320
                    'If tProject.MeetingIDLikelihood(True) > 0 Then DBProjectByMeetingID(tProject.MeetingIDLikelihood(True), tProject.ID) ' D3320
                    ' D1709 ==
                    SnapshotSaveProject(ecSnapShotType.RestorePoint, "Open model", tProject.ID, True)    ' D3576 + D3689 + D6311 // move here for avoid snapshot duplicates on init some GUIDs, MeetingIDs
                    tProject.LastVisited = Now  ' D0494
                    DebugInfo(String.Format("Set active project as '{0}'", tProject.Passcode), _TRACE_INFO)     ' D0471
                    DBUpdateDateTime(_TABLE_PROJECTS, _FLD_PROJECTS_LASTVISITED, tProject.ID, _FLD_PROJECTS_ID) ' D0491
                    _ProjectID = tProject.ID    ' D0511
                End If
                ' D0833 ===
                'If Not ActiveUserWorkgroup Is Nothing AndAlso ActiveUserWorkgroup.WorkgroupID = tProject.WorkgroupID AndAlso Database.Connect Then
                If Not ActiveUserWorkgroup Is Nothing AndAlso Database.Connect AndAlso _ProjectID > 0 Then    ' D6785
                    Dim tParams As New List(Of Object)
                    tParams.Add(_ProjectID)
                    tParams.Add(ActiveUserWorkgroup.ID)
                    Database.ExecuteSQL(String.Format("UPDATE {0} SET {1}=? WHERE {2}=?", _TABLE_USERWORKGROUPS, _FLD_USERWRKG_LASTPROJECTID, _FLD_USERWRKG_ID), tParams)  ' D0491
                End If
                ' D0833 ==
                ' D0930 ===
                If tProject IsNot Nothing Then
                    Dim tWS As clsWorkspace = clsWorkspace.WorkspaceByUserIDAndProjectID(ActiveUser.UserID, _ProjectID, Workspaces)
                    If tWS Is Nothing Then
                        Dim fCanEditDecision As Boolean = CanUserDoAction(ecActionType.at_alManageAnyModel, ActiveUserWorkgroup, ActiveWorkgroup)
                        tWS = AttachProject(ActiveUser, tProject, Not fCanEditDecision, ActiveWorkgroup.GetDefaultRoleGroupID(ecRoleLevel.rlModelLevel, CType(IIf(fCanEditDecision, ecRoleGroupType.gtProjectManager, ecRoleGroupType.gtEvaluator), ecRoleGroupType)), "", False)    ' D2287 + D2644
                        _Workspaces = Nothing
                    End If
                    tProject.TeamTimeCheckStatus()    ' D2063
                End If
                ' D0930 ==
                '_CurrentProject = tProject  ' -D0511
                _CurrentWorkspace = Nothing
                _CurrentProjectGroup = Nothing
                ' D3554 ===
                If tProject IsNot Nothing Then
                    DBUpdateDateTime(_TABLE_PROJECTS, _FLD_PROJECTS_LASTVISITED, tProject.ID, _FLD_PROJECTS_ID) ' D0491
                    tProject.LastVisited = Now    ' D6443
                    If ActiveWorkgroup IsNot Nothing Then
                        DBUpdateDateTime(_TABLE_WORKGROUPS, _FLD_WORKGROUPS_LASTVISITED, ActiveWorkgroup.ID, _FLD_WORKGROUPS_ID)
                        ActiveWorkgroup.LastVisited = Now    ' D6443
                    End If
                    If ActiveUserWorkgroup IsNot Nothing Then
                        DBUpdateDateTime(_TABLE_USERWORKGROUPS, _FLD_USERWRKG_LASTVISITED, ActiveUserWorkgroup.ID, _FLD_USERWRKG_ID)
                        ActiveUserWorkgroup.LastVisited = Now    ' D6443
                    End If
                    If ActiveWorkspace IsNot Nothing Then
                        DBUpdateDateTime(_TABLE_WORKSPACE, _FLD_WORKSPACE_LASTMODIFY, ActiveWorkspace.ID, _FLD_WORKSPACE_ID)  ' D6443
                        ActiveWorkspace.LastModify = Now    ' D6443
                    End If
                    If ActiveUser IsNot Nothing Then
                        DBUpdateDateTime(_TABLE_USERS, _FLD_USERS_LASTVISITED, ActiveUser.UserID, _FLD_USERS_ID)
                        ActiveUser.LastVisited = Now    ' D6443
                    End If
                    If Antigua_MeetingID > 0 AndAlso tProject.MeetingIDLikelihood <> Antigua_MeetingID AndAlso tProject.MeetingIDImpact <> Antigua_MeetingID Then Antigua_ResetCredentials()    ' D6485
                Else
                    Antigua_ResetCredentials()  ' D6485
                End If
                ' D3554 ==
            End If
        End Sub

        Public Function HasActiveProject() As Boolean
            'Return Not _CurrentProject Is Nothing  ' -D0511
            Return ProjectID > 0  ' D0511
        End Function

        Public Property ProjectID() As Integer
            Get
                'If HasActiveProject() Then Return ActiveProject.ID Else Return -1  ' -D0511
                Return _ProjectID   ' D0511
            End Get
            Set(ByVal value As Integer)
                Dim fNewProject As Boolean = True
                If _ProjectID > 0 Then fNewProject = _ProjectID <> value ' D0511
                If fNewProject Then
                    ' D0491 ===
                    Dim tProject As clsProject = Nothing
                    If value > 0 AndAlso Not ActiveWorkgroup Is Nothing Then
                        tProject = clsProject.ProjectByID(value, ActiveProjectsList)
                        'If Not tProject Is Nothing Then tProject = DBProjectByID(value)    '-D0510
                        _LastSnapshot = Nothing ' D3509
                    End If
                    SetActiveProject(tProject)
                    ' D0491 ==
                End If
            End Set
        End Property
        ' D0466 ==

        ' D0465 ===
        Public Property ActiveProject() As clsProject
            Get
                'Return  _CurrentProject ' -D0511
                Return clsProject.ProjectByID(_ProjectID, ActiveProjectsList)
            End Get
            Set(ByVal value As clsProject)
                SetActiveProject(value) ' D0491
            End Set
        End Property

        'A1411 ===
        Public ReadOnly Property IsActiveProjectReadOnly As Boolean
            Get
                If Not HasActiveProject() Then Return True
                Return Not CanUserModifyProject(ActiveUser.UserID, ActiveProject.ID, ActiveUserWorkgroup, DBWorkspaceByUserIDProjectID(ActiveUser.UserID, ActiveProject.ID), ActiveWorkgroup)
            End Get
        End Property
        'A1411 ==

        Public ReadOnly Property IsActiveProjectStructureReadOnly As Boolean
            Get
                If Not HasActiveProject() Then Return True
                Return IsActiveProjectReadOnly OrElse ActiveProject.LockInfo.LockStatus = ECLockStatus.lsLockForAntigua OrElse ActiveProject.ProjectStatus = ecProjectStatus.psArchived
            End Get
        End Property

        ' -D0511
        '' D0510 ===
        'Private Sub CheckActiveProject(ByRef tProject As clsProject)
        '    ' D0510 ===
        '    If Not _CurrentProject Is Nothing AndAlso Not tProject Is Nothing Then
        '        If _CurrentProject.ID = tProject.ID Then
        '            If _CurrentProject.IsProjectLoaded Then tProject.ProjectManager = _CurrentProject.ProjectManager
        '            _CurrentProject = tProject
        '        End If
        '    End If
        '    ' D0510 ==
        'End Sub
        '' D0510 ==

        ' -D0511
        '' D0469 ===
        'Public ReadOnly Property ActiveWorkgroupProjects() As List(Of clsProject)
        '    Get
        '        If _WorkgroupProjects Is Nothing Then
        '            _WorkgroupProjects = New List(Of clsProject)
        '            If Not ActiveWorkgroup Is Nothing Then
        '                If ActiveWorkgroup.Status <> ecWorkgroupStatus.wsSystem Then
        '                    _WorkgroupProjects = DBProjectsByWorkgroupID(ActiveWorkgroup.ID)
        '                    'If Not _CurrentProject Is Nothing Then CheckActiveProject(clsProject.ProjectByID(ProjectID, _ProjectsList)) ' D0510 -D0511
        '                End If
        '            End If
        '        End If
        '        Return _WorkgroupProjects
        '    End Get
        'End Property
        '' D0469 ==

        ' D0466 ===
        Public Property ActiveProjectsList() As List(Of clsProject)
            Get
                If _ProjectsList Is Nothing Then
                    '' D0511 ===
                    'Dim ActivePM As clsProjectManager = Nothing
                    'If _ProjectID > 0 AndAlso Not _ProjectsList Is Nothing Then
                    '    Dim tPrj As clsProject = clsProject.ProjectByID(_ProjectID, _ProjectsList)
                    '    If Not tPrj Is Nothing AndAlso tPrj.IsProjectLoaded Then ActivePM = tPrj.ProjectManager
                    '    tPrj = Nothing
                    'End If
                    '' D0511 ==
                    _ProjectsList = New List(Of clsProject)
                    If Not ActiveWorkgroup Is Nothing AndAlso Not ActiveUser Is Nothing Then
                        If ActiveWorkgroup.Status <> ecWorkgroupStatus.wsSystem Then
                            _ProjectsList = CheckProjectsList(ActiveUser, ActiveWorkgroup, DBProjectsByWorkgroupID(ActiveWorkgroup.ID), ActiveUserWorkgroup, Workspaces)    ' D0468 + D0511
                        End If
                    End If
                    'If Not _CurrentProject Is Nothing Then CheckActiveProject(clsProject.ProjectByID(ProjectID, _ProjectsList)) ' D0510 -D0511
                    '' D0511 ==
                    'If Not ActivePM Is Nothing AndAlso _ProjectID > 0 Then
                    '    Dim tPrj As clsProject = clsProject.ProjectByID(_ProjectID, _ProjectsList)
                    '    If Not tPrj Is Nothing Then tPrj.ProjectManager = ActivePM
                    '    tPrj = Nothing
                    'End If
                    '' D0511 ==
                End If
                Return _ProjectsList
            End Get
            ' D0474 ===
            Set(ByVal value As List(Of clsProject))
                If Not value Is _ProjectsList Then
                    _ProjectsList = value
                    'If value Is Nothing Then _WorkgroupProjects = Nothing ' D0479 -D0511
                End If
            End Set
            ' D0474 ==
        End Property
        ' D0466 ==

        Public Function CheckProjectsList(ByVal tUser As clsApplicationUser, ByVal tWorkgroup As clsWorkgroup, Optional ByVal tCheckingProjectsList As List(Of clsProject) = Nothing, Optional ByVal tUserWorkgroup As clsUserWorkgroup = Nothing, Optional ByRef tWorkspaces As List(Of clsWorkspace) = Nothing) As List(Of clsProject)
            Dim tProjectsList As New List(Of clsProject)
            DebugInfo("Start to check projects list...")
            If Not tUser Is Nothing AndAlso Not tWorkgroup Is Nothing Then

                If tWorkgroup.Status <> ecWorkgroupStatus.wsSystem Then

                    If tCheckingProjectsList Is Nothing Then tCheckingProjectsList = DBProjectsByWorkgroupID(tWorkgroup.ID)
                    ' D0964 ===
                    If tCheckingProjectsList IsNot Nothing AndAlso tWorkgroup.Status <> ecWorkgroupStatus.wsSystem AndAlso tWorkgroup.License IsNot Nothing Then
                        Dim MaxOnline As Long = tWorkgroup.License.GetParameterMaxByID(ecLicenseParameter.MaxProjectsOnline)
                        If MaxOnline <> UNLIMITED_VALUE AndAlso MaxOnline <> LICENSE_NOVALUE Then
                            Dim CurOnline As Long = tWorkgroup.License.GetParameterValueByID(ecLicenseParameter.MaxProjectsOnline)
                            If CurOnline > MaxOnline Then
                                tCheckingProjectsList.Sort(New clsProjectComparer(ecProjectSort.srtProjectDateTime, Web.UI.WebControls.SortDirection.Ascending, Nothing))
                                For Each tPrj As clsProject In tCheckingProjectsList
                                    If CurOnline <= MaxOnline Then Exit For
                                    If tPrj.isOnline Then
                                        tPrj.isOnline = False
                                        If DBProjectUpdate(tPrj, False, "Reset on-line status due to license limitations") Then CurOnline -= 1
                                    End If
                                Next
                                tCheckingProjectsList.Sort(New clsProjectComparer(ecProjectSort.srtProjectName, Web.UI.WebControls.SortDirection.Ascending, Nothing))
                            End If
                        End If
                    End If
                    ' D0964 ==
                    ' D0580 ===
                    If tUserWorkgroup Is Nothing AndAlso tUser Is ActiveUser Then tUserWorkgroup = clsUserWorkgroup.UserWorkgroupByUserIDAndWorkgroupID(tUser.UserID, tWorkgroup.ID, UserWorkgroups)
                    If tUserWorkgroup Is Nothing Then DBUserWorkgroupByUserIDWorkgroupID(tUser.UserID, tWorkgroup.ID)
                    If tWorkspaces Is Nothing AndAlso tUser Is ActiveUser AndAlso Workspaces IsNot Nothing Then tWorkspaces = Workspaces
                    If tWorkspaces Is Nothing Then tWorkspaces = DBWorkspacesByUserID(tUser.UserID)
                    ' D0580 ==
                    Dim tRoleGroup As clsRoleGroup = Nothing
                    If Not tUserWorkgroup Is Nothing Then tRoleGroup = tWorkgroup.RoleGroup(tUserWorkgroup.RoleGroupID)

                    Dim fCanCreateDecisions As Boolean = False
                    Dim fCanEditAnyDecisions As Boolean = False
                    Dim fCanViewAllDecisions As Boolean = False ' D2782

                    If Not tRoleGroup Is Nothing Then
                        fCanCreateDecisions = tRoleGroup.ActionStatus(ecActionType.at_alCreateNewModel) = ecActionStatus.asGranted Or tRoleGroup.ActionStatus(ecActionType.at_alUploadModel) = ecActionStatus.asGranted
                        fCanEditAnyDecisions = tRoleGroup.ActionStatus(ecActionType.at_alManageAnyModel) = ecActionStatus.asGranted
                        fCanViewAllDecisions = tRoleGroup.ActionStatus(ecActionType.at_alViewAllModels) = ecActionStatus.asGranted  ' D2782
                    End If

                    Dim fReloadWS As Boolean = False    ' D0491

                    If Options.isSingleModeEvaluation Then
                        Dim tProject As clsProject = clsProject.ProjectByPasscode(Options.SingleModeProjectPasscode, tCheckingProjectsList)
                        If Not tProject Is Nothing Then
                            tProjectsList.Add(tProject)
                        End If
                    Else

                        If fCanEditAnyDecisions OrElse fCanViewAllDecisions Then    ' D2782

                            tProjectsList = tCheckingProjectsList   ' D0474

                        Else

                            For Each tPrj As clsProject In tCheckingProjectsList
                                Dim tWS As clsWorkspace = clsWorkspace.WorkspaceByUserIDAndProjectID(tUser.UserID, tPrj.ID, tWorkspaces)
                                ' D0491 ===
                                If Not tWS Is Nothing AndAlso tPrj.WorkgroupID = tWorkgroup.ID AndAlso (tWS.StatusLikelihood <> ecWorkspaceStatus.wsDisabled OrElse tWS.StatusImpact <> ecWorkspaceStatus.wsDisabled OrElse fCanEditAnyDecisions) Then    ' D1737 + D1945
                                    ' D4965 ===
                                    Dim fCanSee As Boolean = fCanEditAnyDecisions OrElse (tPrj.ProjectStatus = ecProjectStatus.psActive AndAlso Not tPrj.isMarkedAsDeleted)    ' D6255
                                    If Not fCanSee Then
                                        Dim tGrp As clsRoleGroup = tWorkgroup.RoleGroup(tWS.GroupID, tWorkgroup.RoleGroups)
                                        If tGrp IsNot Nothing AndAlso tGrp.ActionStatus(ecActionType.at_mlManageProjectOptions) = ecActionStatus.asGranted Then fCanSee = True
                                    End If
                                    If fCanSee Then tProjectsList.Add(tPrj)
                                    ' D0491 + D4965 ==
                                End If
                            Next
                        End If

                        ' D0491 ===
                        ' add template/master projects if missed for user, who can edit or manage any decision
                        If fCanCreateDecisions Or fCanEditAnyDecisions Then
                            Dim tPMGroup As Integer = ActiveWorkgroup.GetDefaultRoleGroupID(ecRoleLevel.rlModelLevel, ecRoleGroupType.gtProjectManager) ' D3206
                            For Each tPrj As clsProject In tCheckingProjectsList
                                ' D3206 ===
                                Dim tWS As clsWorkspace = clsWorkspace.WorkspaceByUserIDAndProjectID(tUser.UserID, tPrj.ID, tWorkspaces)
                                If tPrj.ProjectStatus = ecProjectStatus.psActive AndAlso fCanEditAnyDecisions AndAlso tWS IsNot Nothing AndAlso tPMGroup > 0 AndAlso tWS.GroupID <> tPMGroup Then
                                    tWS.GroupID = tPMGroup
                                    DBWorkspaceUpdate(tWS, False, "Set permissions as PM for get access")
                                    fReloadWS = True
                                End If
                                ' D3206 ==
                                If tPrj.WorkgroupID = tWorkgroup.ID AndAlso (tPrj.ProjectStatus = ecProjectStatus.psTemplate OrElse tPrj.ProjectStatus = ecProjectStatus.psMasterProject) AndAlso Not tPrj.isMarkedAsDeleted Then    ' D0789 + D2479
                                    If clsProject.ProjectByID(tPrj.ID, tProjectsList) Is Nothing Then
                                        tProjectsList.Add(tPrj)
                                        fReloadWS = True
                                    End If
                                End If
                            Next
                        End If
                        ' D0491 ==

                    End If ' if not single mode passcode

                    If Not isStartupWorkgroup(ActiveWorkgroup) AndAlso (Options.RestoreAutoArchivedProjects OrElse Options.DoAutoArchiveProjects) Then ' D2812 + D6407
                        ' D0491 ===
                        Dim LastDT As DateTime = Now.AddDays(-Options.ProjectArchiveAccessTimeoutDays)  ' D1568
                        For Each tPrj As clsProject In tProjectsList
                            If tPrj.ProjectName = "" Then tPrj.ProjectName = tPrj.Passcode
                            ' D1568 ===
                            If Options.DoAutoArchiveProjects AndAlso tPrj.ProjectStatus = ecProjectStatus.psActive AndAlso tPrj.LastVisited.HasValue AndAlso tPrj.LastVisited.Value < LastDT AndAlso tPrj.LastModify.HasValue AndAlso tPrj.LastModify.Value < LastDT AndAlso tPrj.Created.HasValue AndAlso tPrj.Created.Value < LastDT Then ' D2933
                                tPrj.ProjectStatus = ecProjectStatus.psArchived
                                DBProjectUpdate(tPrj, False, String.Format("Archive project as not visited (since: {0})", tPrj.LastVisited.Value.ToShortDateString))
                            End If
                            ' D1568 ==

                            ' D2933 ===
                            If Options.RestoreAutoArchivedProjects AndAlso tPrj.ProjectStatus = ecProjectStatus.psArchived AndAlso tPrj.LastVisited.HasValue AndAlso tPrj.LastModify.HasValue AndAlso tPrj.LastVisited.Value.AddDays(Options.ProjectArchiveAccessTimeoutDays) < tPrj.LastModify.Value Then
                                tPrj.ProjectStatus = ecProjectStatus.psActive
                                DBProjectUpdate(tPrj, False, "Activate auto-archived project")
                            End If
                            ' D2933 ==

                            ' -D0930
                            'Dim tWS As clsWorkspace = clsWorkspace.WorkspaceByUserIDAndProjectID(tUser.UserID, tPrj.ID, tWorkspaces)
                            'If tWS Is Nothing Then
                            '    AttachProject(tUser, tPrj, tWorkgroup.GetDefaultRoleGroupID(ecRoleLevel.rlModelLevel, IIf(fCanEditAnyDecisions, ecRoleGroupType.gtProjectOwner, ecRoleGroupType.gtEvaluator)))
                            '    fReloadWS = True
                            'End If
                        Next
                    End If

                    If fReloadWS Then
                        tWorkspaces = Nothing
                    End If
                    ' D0491 ==

                End If ' if non-system workgroup

            End If

            DebugInfo(String.Format("Projects list checked ({0})", tProjectsList.Count))
            Return tProjectsList
        End Function
        ' D0465 ==

        ' D4735 ===
        Public Function ProjectCreate(ByVal sTitle As String, tProjectStatus As ecProjectStatus, tProjectType As ProjectType, tSrcProjectID As Integer, ByRef sError As String) As clsProject   ' D6794
            Dim tPrj As clsProject = Nothing
            Dim fHasSource As Boolean = False
            If tSrcProjectID > 0 Then
                tPrj = clsProject.ProjectByID(tSrcProjectID, ActiveProjectsList)
                If tPrj Is Nothing Then tPrj = DBProjectByID(tSrcProjectID)
                If tPrj IsNot Nothing AndAlso (tPrj.isMarkedAsDeleted OrElse tPrj.WorkgroupID <> ActiveWorkgroup.ID) Then tPrj = Nothing  ' D4736 + D6215
                fHasSource = tPrj IsNot Nothing
                tPrj = Nothing
            End If
            If fHasSource Then
                tPrj = ProjectSaveAs(sTitle, tSrcProjectID, tProjectStatus, sError, False,,,,,,, tProjectType, False)   ' D6794
            Else
                tPrj = ProjectCreateEmpty(sTitle, tProjectStatus, sError)
                If tSrcProjectID > 0 AndAlso String.IsNullOrEmpty(sError) Then sError = "Can't find the source model"   ' D6215
            End If
            Return tPrj
        End Function
        ' D4735 ==

        ' D6307 ===
        ''' <summary>
        ''' Trying to get the wording from the workgroup and updates the pipe params wording (for both hierarchies when Riskion). Also trying to update the Goal naeming when required: return true in that case (hierarchies has been changed)
        ''' </summary>
        ''' <param name="tPrj"></param>
        ''' <param name="fSaveProject"></param>
        ''' <returns></returns>
        Public Function ProjectWordingUpdateWithWorkgroupWording(ByRef tPrj As clsProject, fSaveProject As Boolean) As Boolean
            Dim fUpdatedStructure As Boolean = False
            If ActiveWorkgroup IsNot Nothing AndAlso ActiveWorkgroup.WordingTemplates IsNot Nothing AndAlso ActiveWorkgroup.WordingTemplates.Count > 0 Then
                Dim sNameObjL As String = ""
                Dim sNameObjI As String = ""
                Dim sNameAlts As String = ""
                If isRiskEnabled Then
                    If ActiveWorkgroup.WordingTemplates.ContainsKey(_TPL_RISK_SOURCES) Then sNameObjL = Capitalize(ActiveWorkgroup.WordingTemplates(_TPL_RISK_SOURCES))
                    If ActiveWorkgroup.WordingTemplates.ContainsKey(_TPL_RISK_CONSEQUENCES) Then sNameObjI = Capitalize(ActiveWorkgroup.WordingTemplates(_TPL_RISK_CONSEQUENCES))
                    If ActiveWorkgroup.WordingTemplates.ContainsKey(_TPL_RISK_EVENTS) Then sNameAlts = Capitalize(ActiveWorkgroup.WordingTemplates(_TPL_RISK_EVENTS))
                Else
                    If ActiveWorkgroup.WordingTemplates.ContainsKey(_TPL_COMPARION_OBJS) Then sNameObjL = Capitalize(ActiveWorkgroup.WordingTemplates(_TPL_COMPARION_OBJS))
                    sNameObjI = sNameObjL
                    If ActiveWorkgroup.WordingTemplates.ContainsKey(_TPL_COMPARION_ALTS) Then sNameAlts = Capitalize(ActiveWorkgroup.WordingTemplates(_TPL_COMPARION_ALTS))
                End If

                ' -D6325
                'If sNameObjL <> "" AndAlso tPrj.ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).Nodes(0).NodeName.ToLower <> sNameObjL.ToLower Then
                '    tPrj.ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).Nodes(0).NodeName = sNameObjL
                '    fUpdatedStructure = True
                'End If

                If isRiskEnabled Then

                    ' -D6325
                    'If Not tPrj.ProjectManager.IsValidHierarchyID(ECHierarchyID.hidImpact) Then tPrj.ProjectManager.AddImpactHierarchy()
                    'If sNameObjI <> "" AndAlso tPrj.ProjectManager.Hierarchy(ECHierarchyID.hidImpact).Nodes(0).NodeName.ToLower <> sNameObjI.ToLower Then
                    '    tPrj.ProjectManager.Hierarchy(ECHierarchyID.hidImpact).Nodes(0).NodeName = sNameObjI
                    '    fUpdatedStructure = True
                    'End If

                    tPrj.ProjectManager.PipeParameters.CurrentParameterSet = tPrj.ProjectManager.PipeParameters.ImpactParameterSet
                    If sNameObjI <> "" Then tPrj.ProjectManager.PipeParameters.NameObjectives = sNameObjI
                    If sNameAlts <> "" Then tPrj.ProjectManager.PipeParameters.NameAlternatives = sNameAlts
                End If

                tPrj.ProjectManager.PipeParameters.CurrentParameterSet = tPrj.ProjectManager.PipeParameters.DefaultParameterSet
                If sNameObjL <> "" Then tPrj.ProjectManager.PipeParameters.NameObjectives = sNameObjL
                If sNameAlts <> "" Then tPrj.ProjectManager.PipeParameters.NameAlternatives = sNameAlts

                If fSaveProject Then
                    tPrj.SaveProjectOptions(, False, False)
                    If fUpdatedStructure Then tPrj.SaveStructure("Init empty project data", , False)
                End If
            End If
            Return fUpdatedStructure
        End Function
        ' D6307 ==

        ' D4732 ===
        Public Function ProjectCreateEmpty(ByVal sTitle As String, tProjectStatus As ecProjectStatus, ByRef sError As String) As clsProject
            Dim tPrj As clsProject = Nothing
            If CanUserCreateNewProject(sError) Then
                If sTitle = "" Then sTitle = "New project"
                tPrj = New clsProject(Options.ProjectLoadOnDemand, Options.ProjectForceAllowedAlts, ActiveUser.UserEmail, isRiskEnabled, AddressOf onProjectSavingEvent, AddressOf onProjectSavedEvent, Options.ProjectUseDataMapping, AddressOf onProjectUpdateLastModifyEvent)
                tPrj.ProjectName = sTitle
                tPrj.ProviderType = DefaultProvider
                tPrj.WorkgroupID = ActiveWorkgroup.ID
                tPrj.OwnerID = ActiveUser.UserID
                tPrj.isOnline = False
                tPrj.isPublic = True
                tPrj.PasscodeLikelihood = ProjectUniquePasscode("", -1)
                tPrj.PasscodeImpact = ProjectUniquePasscode("", -1)
                tPrj.ProjectStatus = tProjectStatus

                If DBProjectCreate(tPrj, "Create an empty project") Then
                    If tPrj.HierarchyObjectives.Nodes.Count = 0 Then
                        Dim tNode As ECCore.clsNode = tPrj.HierarchyObjectives.AddNode(-1)
                        tNode.NodeName = If(isRiskEnabled, "Causes", "Goal")
                    End If

                    Dim XMLFile As String = WebConfigOption(If(isRiskEnabled, _OPT_DEFAULTPIPEPARAMS_RISK, _OPT_DEFAULTPIPEPARAMS), "", True)
                    If XMLFile <> "" Then If Not My.Computer.FileSystem.FileExists(_FILE_DATA_SETTINGS + XMLFile) Then XMLFile = ""
                    If XMLFile = "" Then XMLFile = _FILE_SETTINGS_DEFPIPEPARAMS
                    If tPrj.PipeParameters.Read(PipeStorageType.pstXMLFile, _FILE_DATA_SETTINGS + XMLFile, DefaultProvider, tPrj.ID) Then
                        If tPrj.PipeParameters.ProjectName <> "" AndAlso tPrj.ProjectName = "" Then tPrj.ProjectName = tPrj.PipeParameters.ProjectName
                        If tPrj.PipeParameters.ProjectPurpose <> "" AndAlso tPrj.Comment = "" Then tPrj.Comment = tPrj.PipeParameters.ProjectPurpose
                    End If

                    tPrj.ProjectManager.Hierarchy(tPrj.ProjectManager.ActiveHierarchy).DefaultMeasurementTypeForCoveringObjectives = tPrj.ProjectManager.PipeParameters.DefaultCoveringObjectiveMeasurementType
                    tPrj.ProjectManager.Hierarchy(tPrj.ProjectManager.ActiveHierarchy).AltsDefaultContribution = tPrj.ProjectManager.PipeParameters.AltsDefaultContribution
                    tPrj.ProjectManager.Hierarchy(tPrj.ProjectManager.ActiveHierarchy).Nodes(0).MeasureType = tPrj.ProjectManager.Hierarchy(tPrj.ProjectManager.ActiveHierarchy).DefaultMeasurementTypeForCoveringObjectives

                    If ActiveWorkgroup IsNot Nothing AndAlso ActiveWorkgroup.WordingTemplates IsNot Nothing AndAlso ActiveWorkgroup.WordingTemplates.Count > 0 Then
                        Dim sNameObjL As String = "Objectives"
                        Dim sNameObjI As String = sNameObjL
                        Dim sNameAlts As String = "Alternatives"
                        If isRiskEnabled Then
                            If ActiveWorkgroup.WordingTemplates.ContainsKey(_TPL_RISK_SOURCES) Then sNameObjL = Capitalize(ActiveWorkgroup.WordingTemplates(_TPL_RISK_SOURCES))
                            If ActiveWorkgroup.WordingTemplates.ContainsKey(_TPL_RISK_CONSEQUENCES) Then sNameObjI = Capitalize(ActiveWorkgroup.WordingTemplates(_TPL_RISK_CONSEQUENCES))
                            If ActiveWorkgroup.WordingTemplates.ContainsKey(_TPL_RISK_EVENTS) Then sNameAlts = Capitalize(ActiveWorkgroup.WordingTemplates(_TPL_RISK_EVENTS))
                        Else
                            If ActiveWorkgroup.WordingTemplates.ContainsKey(_TPL_COMPARION_OBJS) Then sNameObjL = Capitalize(ActiveWorkgroup.WordingTemplates(_TPL_COMPARION_OBJS))
                            sNameObjI = sNameObjL
                            If ActiveWorkgroup.WordingTemplates.ContainsKey(_TPL_COMPARION_ALTS) Then sNameAlts = Capitalize(ActiveWorkgroup.WordingTemplates(_TPL_COMPARION_ALTS))
                        End If

                        tPrj.ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).Nodes(0).NodeName = sNameObjL

                        If isRiskEnabled Then
                            If Not tPrj.ProjectManager.IsValidHierarchyID(ECHierarchyID.hidImpact) Then tPrj.ProjectManager.AddImpactHierarchy()
                            tPrj.ProjectManager.Hierarchy(ECHierarchyID.hidImpact).Nodes(0).NodeName = sNameObjI
                            'tPrj.HierarchyAlternatives.Nodes(0).NodeName = Capitalize(ActiveWorkgroup.WordingTemplates(_TPL_RISK_EVENTS))

                            tPrj.ProjectManager.PipeParameters.CurrentParameterSet = tPrj.ProjectManager.PipeParameters.ImpactParameterSet
                            tPrj.ProjectManager.PipeParameters.NameObjectives = sNameObjI
                            tPrj.ProjectManager.PipeParameters.NameAlternatives = sNameAlts
                        End If

                        tPrj.ProjectManager.PipeParameters.CurrentParameterSet = tPrj.ProjectManager.PipeParameters.DefaultParameterSet
                        tPrj.ProjectManager.PipeParameters.NameObjectives = sNameObjL
                        tPrj.ProjectManager.PipeParameters.NameAlternatives = sNameAlts
                    End If

                    tPrj.SaveProjectOptions("Init empty project settings", , False)
                    tPrj.SaveStructure("Init empty project data", , False)
                    tPrj.ResetProject(True)

                    Dim tPMGrpID As Integer = ActiveWorkgroup.GetDefaultRoleGroupID(ecRoleLevel.rlModelLevel, ecRoleGroupType.gtProjectManager)
                    Dim tPrjWS = AttachProject(ActiveUser, tPrj, False, tPMGrpID, "", False)


                    'TODO: Call CheckAndAddReviewAccount after create a model
                    'CheckAndAddReviewAccount(ReviewAccount, tPrj, tPMGrpID)

                    CheckProjectManagerUsers(tPrj)
                    ActiveProjectsList = Nothing
                    Workspaces = Nothing
                    tPrj = clsProject.ProjectByID(tPrj.ID, ActiveProjectsList)
                Else
                    sError = ResString("errCreateEmptyProject")
                End If
                Database.Close()
            Else
                If sError = "" Then sError = ResString("msgCantCreateProject")
            End If
            Return tPrj
        End Function

        Public Function ProjectSaveAs(ByVal sTitle As String, ByVal SourceProjectID As Integer, DestProjectStatus As ecProjectStatus, ByRef sError As String, Optional fCopyUsers As Boolean = True, Optional sObjCleanUpName As String = "", Optional sAltCleanUpName As String = "", Optional sObjImpactCleanUpName As String = "", Optional fPaticipantsEmailCleanUp As Boolean = False, Optional fPaticipantsNameCleanUp As Boolean = False, Optional CopyMinorVersion As Integer = -1, Optional fProjectType As ProjectType = ProjectType.ptRegular, Optional CopySnapshots As Boolean = True) As clsProject
            Dim tPrj As clsProject = Nothing
            Dim tSourcePrj As clsProject = DBProjectByID(SourceProjectID)
            If CanUserCreateNewProject(sError) AndAlso tSourcePrj IsNot Nothing Then
                If (tSourcePrj.isValidDBVersion OrElse tSourcePrj.isDBVersionCanBeUpdated) Then
                    tPrj = New clsProject(Options.ProjectLoadOnDemand, Options.ProjectForceAllowedAlts, ActiveUser.UserEmail, tSourcePrj.IsRisk, AddressOf onProjectSavingEvent, AddressOf onProjectSavedEvent, Options.ProjectUseDataMapping, AddressOf onProjectUpdateLastModifyEvent)
                    tPrj.Created = DateTime.Now
                    tPrj.ProjectName = sTitle
                    tPrj.ProviderType = DefaultProvider
                    tPrj.WorkgroupID = ActiveWorkgroup.ID
                    tPrj.OwnerID = ActiveUser.UserID
                    tPrj.PasscodeLikelihood = ProjectUniquePasscode("", -1)
                    tPrj.PasscodeImpact = ProjectUniquePasscode("", -1)
                    tPrj.isOnline = False
                    tPrj.isPublic = True
                    tPrj.Comment = tSourcePrj.Comment

                    Dim fSaveasTemplate As Boolean = DestProjectStatus = ecProjectStatus.psMasterProject OrElse DestProjectStatus = ecProjectStatus.psTemplate

                    Dim fNeedUpdateProject As Boolean = False
                    If Not fSaveasTemplate AndAlso tPrj.ProjectStatus <> ecProjectStatus.psActive Then
                        tPrj.ProjectStatus = ecProjectStatus.psActive
                        fNeedUpdateProject = True
                    End If

                    If tPrj.ProjectStatus <> DestProjectStatus OrElse tPrj.ProjectTypeData <> fProjectType Then ' D4978
                        tPrj.ProjectStatus = DestProjectStatus
                        tPrj.ProjectTypeData = fProjectType ' D4978
                        fNeedUpdateProject = True
                    End If

                    If DBProjectCreate(tPrj, If(tSourcePrj.ProjectStatus = ecProjectStatus.psTemplate OrElse tSourcePrj.ProjectStatus = ecProjectStatus.psMasterProject, String.Format("Create project from {0} '{1}'", If(tSourcePrj.ProjectStatus = ecProjectStatus.psMasterProject, "Default option sets", "Template"), ShortString(tSourcePrj.ProjectName, 45)), "Copy decision from '" + tSourcePrj.ProjectName + "'")) Then

                        If DestProjectStatus <> ecProjectStatus.psActive OrElse Not fCopyUsers OrElse sObjCleanUpName <> "" OrElse sAltCleanUpName <> "" OrElse sObjImpactCleanUpName <> "" OrElse fPaticipantsEmailCleanUp OrElse fPaticipantsNameCleanUp Then CopySnapshots = False

                        If DBProjectCopy(tSourcePrj, ECModelStorageType.mstCanvasStreamDatabase, tPrj.ConnectionString, tPrj.ProviderType, tPrj.ID, CopySnapshots, sObjCleanUpName, sAltCleanUpName, sObjImpactCleanUpName, fPaticipantsEmailCleanUp, fPaticipantsNameCleanUp, CopyMinorVersion) Then
                            tPrj.CheckGUID()
                            tPrj.ResetProject()
                            tPrj.LastVisited = Now  ' D4737

                            If fSaveasTemplate OrElse Not fCopyUsers OrElse (tPrj.ProjectStatus = ecProjectStatus.psMasterProject OrElse tPrj.ProjectStatus = ecProjectStatus.psTemplate) Then
                                Dim UsersOrig As List(Of ECTypes.clsUser) = tPrj.ProjectManager.UsersList
                                Dim Users As New List(Of ECTypes.clsUser)
                                Users.AddRange(UsersOrig.ToArray)
                                For Each tUser As ECTypes.clsUser In Users
                                    tPrj.ProjectManager.DeleteUser(tUser.UserEMail, False)
                                Next
                                tPrj.SaveStructure(, , False)

                                If DestProjectStatus = ecProjectStatus.psMasterProject Then
                                    For Each tHier As clsHierarchy In tPrj.ProjectManager.Hierarchies
                                        If tHier.HierarchyType = ECHierarchyType.htModel AndAlso tHier.Nodes.Count > 1 Then
                                            Dim tGoal As ECCore.clsNode = tHier.Nodes(0)
                                            For i As Integer = tGoal.Children.Count - 1 To 0 Step -1
                                                tHier.DeleteNode(tGoal.Children(i), False)
                                                fNeedUpdateProject = True
                                            Next
                                        End If
                                    Next
                                    If tPrj.HierarchyAlternatives.Nodes.Count > 0 Then
                                        For i As Integer = tPrj.HierarchyAlternatives.Nodes.Count - 1 To 0 Step -1
                                            tPrj.HierarchyAlternatives.DeleteNode(tPrj.HierarchyAlternatives.Nodes(i), False)
                                            fNeedUpdateProject = True
                                        Next
                                    End If
                                    If fNeedUpdateProject Then tPrj.SaveStructure(, , False)
                                End If

                                If fSaveasTemplate Then
                                    tPrj.isOnline = False
                                    tPrj.isPublic = False
                                    fNeedUpdateProject = True
                                End If

                                AttachProject(ActiveUser, tPrj, False, ActiveWorkgroup.GetDefaultRoleGroupID(ecRoleLevel.rlModelLevel, ecRoleGroupType.gtProjectManager), "", False)

                            Else

                                If fPaticipantsEmailCleanUp Then

                                    ImportProjectUsers(tPrj, ActiveUser, True, False)

                                Else

                                    tPrj.Created = tSourcePrj.Created
                                    tPrj.LastModify = tSourcePrj.LastModify
                                    fNeedUpdateProject = True

                                    ' copy participants
                                    For Each tWS As clsWorkspace In DBWorkspacesByProjectID(SourceProjectID)
                                        If tWS.UserID = ActiveUser.UserID OrElse fCopyUsers Then
                                            Dim tNewWS As clsWorkspace = tWS.Clone
                                            tNewWS.ProjectID = tPrj.ID
                                            Dim sMsg As String = String.Format("Copy user to project '{0}'", tPrj.Passcode)
                                            If tWS.UserID = ActiveUser.UserID Then
                                                tNewWS.GroupID = ActiveWorkgroup.GetDefaultRoleGroupID(ecRoleLevel.rlModelLevel, ecRoleGroupType.gtProjectManager)
                                                sMsg = String.Format("Attach Project Owner '{0}' to project '{1}'", ActiveUser.UserEmail, tPrj.Passcode)
                                            End If
                                            DBWorkspaceUpdate(tNewWS, True, sMsg)
                                            DBSaveLog(dbActionType.actCreate, dbObjectType.einfWorkspace, tNewWS.ID, sMsg, "", ActiveUser.UserID, ActiveWorkgroup.ID)
                                        End If
                                    Next
                                End If

                            End If


                            If tPrj.ProjectManager.PipeParameters.ProjectType <> fProjectType Then
                                tPrj.ProjectManager.PipeParameters.ProjectType = fProjectType
                                tPrj.SaveProjectOptions("Init on copy project", False, False)
                                'tPrj.ProjectManager.SavePipeParameters(PipeStorageType.pstStreamsDatabase, tPrj.ProjectManager.StorageManager.ModelID)
                            End If

                            ' D5090 ===
                            If tSourcePrj.ProjectStatus = ecProjectStatus.psMasterProject AndAlso tPrj.ProjectStatus = ecProjectStatus.psActive Then
                                With tPrj.ProjectManager.ResourceAligner
                                    If .Scenarios IsNot Nothing AndAlso .Scenarios.ActiveScenario IsNot Nothing AndAlso .Scenarios.ActiveScenario.TimePeriods IsNot Nothing Then
                                        .Scenarios.ActiveScenario.TimePeriods.TimelineStartDate = Date.Now
                                        .Save()
                                    End If
                                End With
                            End If
                            ' D5090 ==

                            If fNeedUpdateProject Then DBProjectUpdate(tPrj, False, "Update project after copy")

                            ActiveProjectsList = Nothing
                            Workspaces = Nothing
                            tPrj = clsProject.ProjectByID(tPrj.ID, ActiveProjectsList)
                        Else
                            DBProjectDelete(tPrj, True) ' can't copy
                            sError = ResString("errCopyProject")
                        End If
                    Else
                        sError = ResString("errCreateEmptyProject")
                    End If
                Else
                    sError = ResString(If(tSourcePrj.ProjectStatus = ecProjectStatus.psMasterProject, "msgCantReadProjectMasterPrj", "msgCantReadProjectDBVersion"))   ' D6994
                End If
            Else
                If sError = "" Then sError = ResString("msgCantCreateProject")
            End If
            Database.Close()
            Return tPrj
        End Function
        ' D4732 ==

        ' D4756 ===
        ''' <summary>
        ''' Upload model from the file
        ''' Need to extract archive (in case of .zip, .rar) and pass the extracted file there
        ''' </summary>
        ''' <param name="sFile">The real file name with a path to uploaded file</param>
        ''' <param name="sOriginalFilename">Original model name (passed from the client)</param>
        ''' <param name="sPasscode">For save in Logs</param>
        ''' <param name="fCheckEmails">Option for check e0mail addresses on import users</param>
        ''' <param name="sError">Error message. Parse the value with ParseAllTemplates() call if required</param>
        ''' <returns>Nothing in case of error</returns>
        Public Function ProjectCreateFromFile(sFile As String, sOriginalFilename As String, sPasscode As String, tPrjStatus As ecProjectStatus, fCheckEmails As Boolean, fAllowBlankPsw As Boolean, ReviewAccount As String, ByRef sError As String, ByRef isRiskionModel As Boolean, Optional sProjectTitle As String = Nothing) As clsProject  ' D4986 + D6696
            Dim fResult As Boolean = My.Computer.FileSystem.FileExists(sFile)
            If Not fResult OrElse Not isAuthorized OrElse ActiveWorkgroup Is Nothing OrElse Not CanUserCreateNewProject(sError) Then Return Nothing

            DBSaveLog(dbActionType.actCreate, dbObjectType.einfProject, -1, "Upload started", String.Format("File: '{0}' ({1} bytes)", sOriginalFilename, My.Computer.FileSystem.GetFileInfo(sFile).Length))    ' D3113

            ' Create dummy project
            Dim CurrentProject = New clsProject(Options.ProjectLoadOnDemand, Options.ProjectForceAllowedAlts, ActiveUser.UserEmail, isRiskEnabled, AddressOf onProjectSavingEvent, AddressOf onProjectSavedEvent, Options.ProjectUseDataMapping, AddressOf onProjectUpdateLastModifyEvent)
            With CurrentProject
                .ProviderType = DefaultProvider ' D0329 + D0330
                .isOnline = False ' D0300 + D0748
                .isPublic = True
                .ProjectStatus = tPrjStatus
                .PasscodeLikelihood = ProjectUniquePasscode(sPasscode, -1) ' D0286 + D1709
                .PasscodeImpact = ProjectUniquePasscode("", -1)     ' D1709
                '.ProjectGUID = Guid.NewGuid
                .WorkgroupID = ActiveWorkgroup.ID
                .OwnerID = ActiveUser.UserID
                .IsRisk = isRiskEnabled
            End With
            Dim sTextModel As String = ""
            Dim sMessage As String = ""

            '' D0298 ===
            '' Extract if archive
            'If isSupportedArchive(sOriginalFilename) Then
            '    Dim ExtList As New ArrayList
            '    ExtList.Add(_FILE_EXT_AHP.ToLower)
            '    ExtList.Add(_FILE_EXT_AHPX.ToLower)
            '    ExtList.Add(_FILE_EXT_AHPS.ToLower) ' D0378
            '    ExtList.Add(_FILE_EXT_TXT.ToLower)  ' D2132
            '    Dim sExtractedFile As String = ExtractArchiveForFile(Me, sOriginalFilename, sFile, ExtList, sPasscode, sError)
            '    If sError = "" AndAlso sExtractedFile <> "" Then
            '        sFile = sExtractedFile
            '    Else
            '        fResult = False

            '    End If
            '    ExtList = Nothing
            'End If
            '' D0298 ==

            Dim FileConnString As String = ExpertChoice.Database.clsConnectionDefinition.BuildJetConnectionDefinition(sFile, DBProviderType.dbptODBC).ConnectionString   ' D0315 + D0329 + D0330 + MF0332 + D0459

            ' D0378 + D0387 ===
            Dim fStorageType As ECModelStorageType
            Select Case Path.GetExtension(sOriginalFilename).ToLower
                Case _FILE_EXT_AHP
                    fStorageType = ECModelStorageType.mstAHPDatabase
                Case _FILE_EXT_AHPX
                    fStorageType = ECModelStorageType.mstCanvasDatabase
                Case _FILE_EXT_AHPS
                    fStorageType = ECModelStorageType.mstAHPSStream
                Case _FILE_EXT_TXT  ' D2132
                    fStorageType = ECModelStorageType.mstTextFile   ' D2132
                Case Else
                    fResult = False
                    sError = ResString("errUnknownFile")
            End Select
            ' D0387 ==

            If fResult Then

                ' check original file for a valid format
                Select Case fStorageType    ' D0387

                    Case ECModelStorageType.mstAHPSStream ' D0387
                        Dim AHPStream As New FileStream(sFile, FileMode.Open, FileAccess.Read)
                        fResult = MiscFuncs.IsAHPSStream(AHPStream)
                        ' D2753 ===
                        If Not fResult Then
                            sError = ResString("errWrongUploadFile")
                        Else
                            AHPStream.Seek(0, SeekOrigin.Begin)
                            Dim DBVersion As ECCanvasDatabaseVersion = MiscFuncs.GetProjectVersion_AHPSStream(AHPStream)
                            If DBVersion.MajorVersion * 10000 + DBVersion.MinorVersion > GetCurrentDBVersion.MajorVersion * 10000 + GetCurrentDBVersion.MinorVersion Then
                                fResult = False
                                sError = String.Format(ResString("msgWrongProjectDBVersion"), DBVersion.GetVersionString, GetCurrentDBVersion.GetVersionString)
                            End If
                        End If
                        AHPStream.Close()
                        ' D2753 ==

                    Case ECModelStorageType.mstAHPDatabase  ' D0387
                        If _DB_CHECK_AHP_VERSION AndAlso fResult Then ' D0240
                            Dim AHP_DBVer As Single = GetAHPDBVersion(FileConnString, DBProviderType.dbptODBC)  ' D0329
                            If AHP_DBVer < AHP_DB_DEFAULT_VERSION Then
                                fResult = False
                                sError = String.Format(ResString("errOldAHPVersion"), AHP_DB_DEFAULT_VERSION)
                            ElseIf AHP_DBVer < AHP_DB_LATEST_VERSION Then
                                Dim AHP_LastUploadToECC As String = GetAHPLastUploadToECC(FileConnString, DBProviderType.dbptODBC) 'AS/6-28-16===
                                Dim AHP_ChangedInECD As Boolean = If(Len(AHP_LastUploadToECC) > 0, True, False)
                                CheckOld_AHP(FileConnString, DBProviderType.dbptODBC, AHP_DBVer, AHP_ChangedInECD, sError) 'AS/6-28-16==
                            End If 'AS/11-17-15==
                        End If

                    Case ECModelStorageType.mstTextFile
                        Try
                            sTextModel = My.Computer.FileSystem.ReadAllText(sFile)
                            fResult = clsTextModel.isValidContent(sTextModel, sError)
                            ' D2133 ===
                            If fResult Then
                                Dim objMD5 As New Security.Cryptography.MD5CryptoServiceProvider
                                Dim arrData() As Byte = Text.Encoding.UTF8.GetBytes(sTextModel.Length.ToString + SubString(sTextModel, 1024))
                                Dim arrHash() As Byte = objMD5.ComputeHash(arrData)
                                If arrHash.GetLength(0) <> 16 Then Array.Resize(arrHash, 16)
                                CurrentProject.ProjectGUID = New Guid(arrHash)
                            End If
                            ' D2133 ==
                        Catch ex As Exception
                            fResult = False
                            sError = ResString("errReadFile")
                        End Try
                        ' D2132 ==
                End Select
            End If

            ' D0157 ===
            If fResult Then   ' D0345
                fResult = CheckUsersListBeforeUpload(sFile, FileConnString, DBProviderType.dbptODBC, fStorageType, fCheckEmails, sError, True) ' D0172 + D0329 + D0345 + D0503 + D1226 + D1519  //   IgnoreLicenseWarning.Value <> FileUploadProject.FileName
            End If
            ' D0378 ==

            ' D0925 ===
            If fResult AndAlso ActiveWorkgroup.License.isValidLicense Then
                Dim ObjMax As Long = ActiveWorkgroup.License.GetParameterMaxByID(ecLicenseParameter.MaxObjectives)
                Dim AltMax As Long = ActiveWorkgroup.License.GetParameterMaxByID(ecLicenseParameter.MaxAlternatives)
                Dim LevelMax As Long = ActiveWorkgroup.License.GetParameterMaxByID(ecLicenseParameter.MaxLevelsBelowGoal)
                If ObjMax <> UNLIMITED_VALUE Or AltMax <> UNLIMITED_VALUE Or LevelMax <> UNLIMITED_VALUE Then

                    ' D2133 ===
                    Dim ObjsCount As Integer = 1
                    Dim AltsCount As Integer = 0
                    Dim LevelsCount As Integer = 1

                    Select Case fStorageType
                        Case ECModelStorageType.mstTextFile
                            Dim tmpPrj As New clsProject(True, False, ActiveUser.UserEmail, isRiskEnabled, , , Options.ProjectUseDataMapping)  ' D2255 + D4465
                            clsTextModel.ReadModel(tmpPrj, sTextModel, isRiskEnabled, sError)   ' D2426
                            If sError = "" Then ' D3306
                                With tmpPrj.ProjectManager
                                    If .Hierarchies.Count > 0 Then
                                        ObjsCount = .Hierarchy(.ActiveHierarchy).Nodes.Count
                                        LevelsCount = .Hierarchy(.ActiveHierarchy).GetMaxLevel()
                                    End If
                                    If .AltsHierarchies.Count > 0 Then AltsCount = .AltsHierarchy(.ActiveAltsHierarchy).Nodes.Count
                                End With
                            End If
                            tmpPrj = Nothing

                        Case Else
                            Dim tmpPrjManager = New clsProjectManager(True, False, isRiskEnabled) ' D2255
                            With tmpPrjManager
                                .StorageManager.ProviderType = DBProviderType.dbptODBC
                                .StorageManager.ProjectLocation = FileConnString
                                .StorageManager.StorageType = fStorageType
                                If fStorageType = ECModelStorageType.mstAHPSStream Then
                                    .StorageManager.StorageType = ECModelStorageType.mstAHPSFile
                                    .StorageManager.ProjectLocation = sFile
                                End If
                                If fStorageType = ECModelStorageType.mstAHPDatabase Then
                                    FixRAcontraintsTableBeforeUpload(sFile)
                                End If
                                .StorageManager.ReadDBVersion()
                                If .LoadProject(.StorageManager.ProjectLocation, DBProviderType.dbptODBC, .StorageManager.StorageType) Then
                                    If .Hierarchies.Count > 0 Then
                                        ObjsCount = .Hierarchy(.ActiveHierarchy).Nodes.Count
                                        LevelsCount = .Hierarchy(.ActiveHierarchy).GetMaxLevel()
                                    End If
                                    If .AltsHierarchies.Count > 0 Then AltsCount = .AltsHierarchy(.ActiveAltsHierarchy).Nodes.Count
                                End If
                            End With
                            tmpPrjManager = Nothing

                    End Select

                    If ObjMax <> UNLIMITED_VALUE AndAlso ObjsCount > ObjMax + 1 Then
                        sError = LicenseErrorMessage(ActiveWorkgroup.License, ecLicenseParameter.MaxObjectives)
                        fResult = False
                    End If
                    If LevelMax <> UNLIMITED_VALUE AndAlso LevelsCount > LevelMax Then  ' D3304
                        sError += CType(IIf(sError = "", "", ";<br> "), String) + LicenseErrorMessage(ActiveWorkgroup.License, ecLicenseParameter.MaxLevelsBelowGoal)
                        fResult = False
                    End If
                    If AltMax <> UNLIMITED_VALUE AndAlso AltsCount > AltMax Then  ' D3050 + D3304
                        sError += CType(IIf(sError = "", "", ";<br> "), String) + LicenseErrorMessage(ActiveWorkgroup.License, ecLicenseParameter.MaxAlternatives)
                        fResult = False
                    End If
                    ' D2133 ==

                End If
            End If
            ' D0925 ==

            ' Ready to create project and upload data to DB
            If fResult Then   ' D0172
                ' D0157 ==
                Dim sComment As String = String.Format("Upload '{0}' model", Path.GetFileName(sOriginalFilename))    ' D0378

                Dim fUploaded As Boolean = False
                CurrentProject.ResetProject(True)
                ' Create am empty model
                If DBProjectCreate(CurrentProject, sComment) Then ' D0378 + D0387 + D0479

                    Dim isOriginalRisk As Boolean = CurrentProject.IsRisk

                    ' D0378 ===
                    Select Case fStorageType
                        Case ECModelStorageType.mstAHPDatabase  ' D0387
                            FixRAcontraintsTableBeforeUpload(sFile) 'C0427
                            fUploaded = DBProjectCreateFromAHPFile(CurrentProject, sFile, sError) ' D0162 + D0166 + D0479

                        Case ECModelStorageType.mstCanvasDatabase ' D0387
                            fUploaded = DBProjectCreateFromAHPXFile(CurrentProject, sFile, sError) 'C0271 + D0479

                        Case ECModelStorageType.mstAHPSStream ' D0387
                            Dim FS As FileStream = Nothing
                            Try
                                FS = New FileStream(sFile, FileMode.Open, FileAccess.Read)
                                CurrentProject.ResetProject(True)   ' D0387
                                Dim tSnapshotsIdx As Integer = -1   ' D3892
                                fUploaded = CurrentProject.ProjectManager.StorageManager.Writer.SaveFullProjectStream(FS, , , tSnapshotsIdx)  ' D3892
                                ' D3651 ===
                                If fUploaded AndAlso CurrentProject.IsProjectLoaded Then
                                    CurrentProject.ProjectManager.StorageManager.ReadDBVersion()
                                    If CurrentProject.ProjectManager.StorageManager.CanvasDBVersion.MinorVersion <= 30 Then
                                        If CurrentProject.ProjectManager IsNot Nothing AndAlso CurrentProject.ProjectManager.ResourceAligner IsNot Nothing AndAlso CurrentProject.ProjectManager.ResourceAligner.Solver IsNot Nothing AndAlso Not String.IsNullOrEmpty(CurrentProject.ProjectManager.ResourceAligner.Solver.LastError) Then
                                            sError = CurrentProject.ProjectManager.ResourceAligner.Solver.LastError
                                            CurrentProject.ProjectManager.ResourceAligner.Solver.LastError = ""
                                        End If
                                    End If
                                End If
                                ' D3892 ===
                                If _SNAPSHOT_USE_IN_AHPS AndAlso fUploaded AndAlso tSnapshotsIdx > 0 Then
                                    Using tReadStream As New IO.FileStream(sFile, IO.FileMode.Open)
                                        CurrentProject.ResetProject(True)   ' D3907
                                        tReadStream.Seek(tSnapshotsIdx, SeekOrigin.Begin)
                                        Dim Cnt As Integer = SnapshotsAllFromStream(CurrentProject, tReadStream)
                                        If Cnt > 0 Then DBSaveLog(dbActionType.actCreate, dbObjectType.einfSnapshot, CurrentProject.ID, "Restore snapshot(s) on model upload", String.Format("Restored: {0}", Cnt))
                                        tReadStream.Close()
                                    End Using
                                End If
                                ' D3892 ==
                                ' D3651 ==
                            Catch ex As Exception
                                sError = ex.Message
                                fResult = False
                                fUploaded = False
                                DBProjectDelete(CurrentProject, True)
                            Finally
                                If Not FS Is Nothing Then FS.Close()
                            End Try

                            ' D2132 ===
                        Case ECModelStorageType.mstTextFile
                            fUploaded = clsTextModel.ReadModel(CurrentProject, sTextModel, isRiskEnabled, sError)   ' D2426
                            If fUploaded Then
                                CurrentProject.SaveStructure("Save data based on txt file")
                                CurrentProject.ProjectManager.StorageManager.Writer.SaveInfoDocs()  ' D2133
                                CurrentProject.ProjectManager.StorageManager.Writer.SaveModelStructure()
                                If CurrentProject.Comment <> "" Then DBProjectUpdate(CurrentProject, False, "Set project description") ' D2804
                            Else
                                DBProjectDelete(CurrentProject, True)   ' D2146
                            End If
                            ' D2132 ==
                    End Select
                    ' D0378 ==

                    If fUploaded Then

                        If Not CurrentProject.isValidDBVersion Then DBProjectUpdateToLastVersion(CurrentProject)
                        CurrentProject.ResetProject() ' D0138
                        ImportProjectUsers(CurrentProject, ActiveUser, fAllowBlankPsw, fCheckEmails) ' D0113 + D0272 + D0345 + D0479 + D1985

                        ' D6696 ===
                        If fStorageType = ECModelStorageType.mstAHPSStream Then
                            If isRiskEnabled Then
                                Dim tmpPrjManager = New clsProjectManager(True, False, False)
                                With tmpPrjManager
                                    .StorageManager.ProviderType = DBProviderType.dbptODBC
                                    .StorageManager.ProjectLocation = FileConnString
                                    .StorageManager.StorageType = fStorageType
                                    .StorageManager.StorageType = ECModelStorageType.mstAHPSFile
                                    .StorageManager.ProjectLocation = sFile
                                    .StorageManager.ReadDBVersion()
                                    If .LoadProject(.StorageManager.ProjectLocation, DBProviderType.dbptODBC, .StorageManager.StorageType) Then
                                        isRiskionModel = tmpPrjManager.IsValidHierarchyID(ECHierarchyID.hidImpact)
                                    End If
                                End With
                            Else
                                isRiskionModel = CurrentProject.ProjectManager.IsValidHierarchyID(ECHierarchyID.hidImpact)
                            End If
                        End If
                        ' D6696 ==

                        ' D0126 ===
                        CheckAndAddReviewAccount(ReviewAccount, CurrentProject) ' D1382 + D1408
                        CurrentProject.LastVisited = Now
                        CurrentProject.LastModify = Now
                        Dim fDescUpdated As Boolean = True  ' D0178
                        If CurrentProject.Comment = "" AndAlso Not CurrentProject.PipeParameters.ProjectPurpose Is Nothing Then     ' D0174
                            CurrentProject.Comment = CurrentProject.PipeParameters.ProjectPurpose   ' D0174
                            fDescUpdated = True
                        End If
                        ' D4986 ===
                        If Not String.IsNullOrEmpty(sProjectTitle) Then
                            fDescUpdated = True
                            CurrentProject.ProjectName = sProjectTitle
                        End If
                        ' D4986 ==
                        If CurrentProject.ProjectName = "" AndAlso Not CurrentProject.PipeParameters.ProjectName Is Nothing Then    ' D0174
                            fDescUpdated = True
                            CurrentProject.ProjectName = CurrentProject.PipeParameters.ProjectName  ' D0174
                        End If
                        If CurrentProject.ProjectName = "" Then
                            CurrentProject.ProjectName = GetNameByFilename(sOriginalFilename)   ' D7218
                            fDescUpdated = True
                        End If
                        ' D0174 ===
                        If CurrentProject.Comment <> "" And String.IsNullOrEmpty(CurrentProject.PipeParameters.ProjectPurpose) Then
                            CurrentProject.PipeParameters.ProjectPurpose = CurrentProject.Comment
                            fDescUpdated = True
                        End If
                        If CurrentProject.ProjectName <> "" And String.IsNullOrEmpty(CurrentProject.PipeParameters.ProjectName) Then
                            fDescUpdated = True
                            CurrentProject.PipeParameters.ProjectName = CurrentProject.ProjectName
                        End If
                        ' D0174 ==
                        ' D0892 ===
                        If CurrentProject.PipeParameters.ProjectGuid <> Guid.Empty Then
                            CurrentProject.ProjectGUID = CurrentProject.PipeParameters.ProjectGuid
                            fDescUpdated = True
                        Else
                            If CurrentProject.CheckGUID() Then fDescUpdated = True
                        End If
                        ' D0892 ==
                        ' D3438 ===
                        If CInt(CurrentProject.PipeParameters.ProjectType) <> CurrentProject.ProjectTypeData Then  ' D4978
                            CurrentProject.ProjectTypeData = CInt(CurrentProject.PipeParameters.ProjectType)   ' D4978
                            fDescUpdated = True
                        End If
                        ' -D4978
                        'If CurrentProject.PipeParameters.ProjectType = ProjectType.ptRiskAssociated Then
                        '    CurrentProject.isRiskAssociatedModel = True
                        '    fDescUpdated = True
                        'End If
                        ' D3438 ==
                        If fDescUpdated Then
                            'CurrentProject.PipeParameters.Write(PipeStorageType.pstDatabase, CurrentProject.ConnectionString, CurrentProject.ProviderType)    ' D0174 + D0329 'C0349
                            'CurrentProject.PipeParameters.Write(CurrentProject.Pipe_StorageType, CurrentProject.ConnectionString, CurrentProject.ProviderType) 'C0349 + D0369 + D0376 'C0390
                            CurrentProject.PipeParameters.Write(PipeStorageType.pstStreamsDatabase, CurrentProject.ConnectionString, CurrentProject.ProviderType, CurrentProject.ID) 'C0390 + D0479
                            DBProjectUpdate(CurrentProject, False, "Get Project Properties from model")
                        End If
                        ' D0126 ==

                        ' D3924 ===
                        If Not isGurobiAvailable AndAlso CurrentProject.isValidDBVersion AndAlso CurrentProject.ProjectManager.ResourceAligner.Solver.SolverLibrary = raSolverLibrary.raGurobi AndAlso isXAAvailable Then  ' D4512
                            CurrentProject.ProjectManager.ResourceAligner.Solver.SolverLibrary = raSolverLibrary.raXA
                            'CurrentProject.ProjectManager.ResourceAligner.Save()
                            CurrentProject.ProjectManager.Parameters.RASolver = raSolverLibrary.raXA
                            CurrentProject.ProjectManager.Parameters.Save()
                        End If
                        ' D3924 ==
                        ' D7543 ===
                        If Not isXAAvailable AndAlso CurrentProject.isValidDBVersion AndAlso CurrentProject.ProjectManager.ResourceAligner.Solver.SolverLibrary = raSolverLibrary.raXA AndAlso isBaronAvailable Then
                            CurrentProject.ProjectManager.ResourceAligner.Solver.SolverLibrary = raSolverLibrary.raBaron
                            CurrentProject.ProjectManager.Parameters.RASolver = raSolverLibrary.raBaron
                            CurrentProject.ProjectManager.Parameters.Save()
                        End If
                        ' D7543 ==

                        ' All is done during the upload

                    Else
                        DBSaveLog(dbActionType.actCreate, dbObjectType.einfProject, CurrentProject.ID, "Project Uploading Error", sError) ' D0496
                    End If

                End If ' create project
            End If ' check users


            If sError <> "" OrElse Not fResult Then   ' D0172
                If sError.Trim.ToLower = "weight" Then
                    sError = ResString("errUploadWeightTblMissing") ' D0801
                    fResult = False
                End If
                If sError.Trim.ToLower = "organization" Then
                    sError = ResString("errUploadOrganizationTblMissing") ' D0915
                    fResult = False
                End If
                ' D3651 ===
                If sError.Trim.ToLower = "old_ra" Then
                    sError = ResString("errUploadOldRA") ' D3561
                    DBSaveLog(dbActionType.actShowMessage, dbObjectType.einfProject, CurrentProject.ID, "Warning on upload", sError)
                End If
                ' D3651 ==

                'TODO: Continue upload with a partial data
                '' D1519 ===
                'If ApplicationError.Status = ecErrorStatus.errWrongLicense AndAlso ApplicationError.CustomData IsNot Nothing AndAlso (ApplicationError.CustomData = ecLicenseParameter.MaxUsersInProject.ToString OrElse ApplicationError.CustomData = ecLicenseParameter.MaxUsersInWorkgroup.ToString) Then
                '    lblError.Text += "<br>" + ResString("msgYouCanUploadFileWithPartialData")
                '    IgnoreLicenseWarning.Value = EcAntiXss.HtmlEncode(FileUploadProject.FileName)   ' Anti-XSS
                '    ApplicationError.Reset()
                'End If
                '' D1519 ==
            End If

            If Not fResult AndAlso CurrentProject IsNot Nothing AndAlso (CurrentProject.ID <= 0 OrElse Not String.IsNullOrEmpty(sError)) Then CurrentProject = Nothing     ' D6298 ' reset in case of error
            Return CurrentProject
        End Function
        ' D4756 ==

        ' D6588 ===
        Public Function ProjectCheckPropertiesFromPM(tProject As clsProject) As Boolean
            Dim fDescUpdated As Boolean = False
            If tProject.Comment = "" AndAlso Not String.IsNullOrEmpty(tProject.PipeParameters.ProjectPurpose) Then
                fDescUpdated = tProject.Comment <> tProject.PipeParameters.ProjectPurpose
                tProject.Comment = tProject.PipeParameters.ProjectPurpose
            End If
            If tProject.Comment <> "" AndAlso String.IsNullOrEmpty(tProject.PipeParameters.ProjectPurpose) Then
                fDescUpdated = tProject.Comment <> tProject.PipeParameters.ProjectPurpose
                tProject.PipeParameters.ProjectPurpose = tProject.Comment
            End If
            If tProject.ProjectName = "" AndAlso Not String.IsNullOrEmpty(tProject.PipeParameters.ProjectName) Then
                fDescUpdated = tProject.ProjectName <> tProject.PipeParameters.ProjectName
                tProject.ProjectName = tProject.PipeParameters.ProjectName
            End If
            If tProject.ProjectName <> "" AndAlso String.IsNullOrEmpty(tProject.PipeParameters.ProjectName) Then
                fDescUpdated = tProject.ProjectName <> tProject.PipeParameters.ProjectName
                tProject.PipeParameters.ProjectName = tProject.ProjectName
            End If
            If tProject.PipeParameters.ProjectGuid <> Guid.Empty Then
                If Not tProject.ProjectGUID.Equals(tProject.PipeParameters.ProjectGuid) Then
                    tProject.ProjectGUID = tProject.PipeParameters.ProjectGuid
                    fDescUpdated = True
                End If
            Else
                If tProject.CheckGUID() Then fDescUpdated = True
            End If
            If tProject.PipeParameters.ProjectType <> tProject.ProjectTypeData Then
                tProject.ProjectTypeData = CInt(tProject.PipeParameters.ProjectType)
                fDescUpdated = True
            End If
            If tProject.PipeParameters.ProjectType = ProjectType.ptOpportunities AndAlso tProject.RiskionProjectType <> ProjectType.ptOpportunities Then
                tProject.RiskionProjectType = ProjectType.ptOpportunities
                fDescUpdated = True
            End If
            If tProject.PipeParameters.ProjectType = ProjectType.ptRiskAssociated AndAlso Not tProject.isRiskAssociatedModel Then
                tProject.isRiskAssociatedModel = True
                fDescUpdated = True
            End If
            If Not isGurobiAvailable AndAlso tProject.isValidDBVersion AndAlso tProject.ProjectManager.ResourceAligner.Solver.SolverLibrary = raSolverLibrary.raGurobi AndAlso isXAAvailable Then
                tProject.ProjectManager.ResourceAligner.Solver.SolverLibrary = raSolverLibrary.raXA
                tProject.ProjectManager.Parameters.RASolver = raSolverLibrary.raXA
                tProject.ProjectManager.Parameters.Save()
            End If
            ' D7543 ===
            If Not isXAAvailable AndAlso tProject.isValidDBVersion AndAlso tProject.ProjectManager.ResourceAligner.Solver.SolverLibrary = raSolverLibrary.raXA AndAlso isBaronAvailable Then
                tProject.ProjectManager.ResourceAligner.Solver.SolverLibrary = raSolverLibrary.raBaron
                tProject.ProjectManager.Parameters.RASolver = raSolverLibrary.raBaron
                tProject.ProjectManager.Parameters.Save()
            End If
            ' D7543 ==
            If fDescUpdated Then
                tProject.PipeParameters.Write(PipeStorageType.pstStreamsDatabase, tProject.ConnectionString, tProject.ProviderType, tProject.ID)
                DBProjectUpdate(tProject, False, "Get Project Properties from model")
            End If
            CheckProjectManagerUsers(tProject)    ' D6725
            Return fDescUpdated
        End Function
        ' D6588 ==

        ' D4758 + D5032 ===
        Public Function ProjectReplace(SrcID As Integer, DestID As Integer, ByRef sError As String) As Boolean
            Dim tResult As Boolean = False
            Dim tPrjOld As clsProject = clsProject.ProjectByID(DestID, ActiveProjectsList)
            Dim tPrjNew As clsProject = clsProject.ProjectByID(SrcID, ActiveProjectsList)
            If SrcID = DestID Then
                sError = "Can't replace the same project"
            End If
            If tPrjOld Is Nothing Then
                sError = "Destination project not found"
            Else
                If Not isAuthorized OrElse Not CanActiveUserModifyProject(DestID) Then sError = "No permission for modify destination project"
            End If
            If tPrjNew Is Nothing Then
                sError = "Source project not found"
            Else
                If tPrjNew.isMarkedAsDeleted Then sError = "Project is marked as deleted"
                If sError = "" AndAlso (Not isAuthorized OrElse Not CanActiveUserModifyProject(DestID)) Then sError = "No permission for modify source project"
            End If
            If sError = "" AndAlso tPrjNew.WorkgroupID = tPrjOld.WorkgroupID Then
                ' D5033 ==
                tPrjOld.ProjectGUID = Guid.NewGuid
                tPrjOld.ReplacedID = SrcID
                tPrjOld.isMarkedAsDeleted = True
                tPrjOld.LastModify = Now
                tPrjNew.PasscodeLikelihood = tPrjOld.PasscodeLikelihood
                tPrjNew.PasscodeImpact = tPrjOld.PasscodeImpact
                tPrjOld.PasscodeLikelihood = ProjectUniquePasscode("", -1) ' D1709
                tPrjOld.PasscodeImpact = ProjectUniquePasscode("", -1)     ' D1709
                DBProjectUpdate(tPrjOld, False, String.Format("Replace project with new version ('{0}', #{1})", tPrjNew.Passcode, SrcID))
                DBProjectUpdate(tPrjNew, False, String.Format("Set access code from replaced decision (#{0})", DestID))
                tResult = True
            End If
            Return tResult
        End Function
        ' D4758 ==

        ' D0941 ===
        Public Function GetProjectsWipeout() As List(Of clsProject)
            Dim Lst As New List(Of clsProject)
            If ActiveUser IsNot Nothing AndAlso ActiveProjectsList IsNot Nothing Then
                Dim DT As DateTime = Now.AddDays(-WipeoutProjectsTimeout)
                For Each tPrj As clsProject In ActiveProjectsList
                    'If tPrj.isMarkedAsDeleted AndAlso tPrj.LastModify < DT AndAlso tPrj.OwnerID = ActiveUser.UserID AndAlso CanUserDoProjectAction(ecActionType.at_mlDeleteModel, ActiveUser.UserID, tPrj.ID, ActiveUserWorkgroup, ActiveWorkgroup) Then
                    If tPrj.isMarkedAsDeleted AndAlso tPrj.LastModify.HasValue AndAlso tPrj.LastModify.Value.Date <= DT.Date Then ' D0945
                        Lst.Add(tPrj)
                    End If
                Next
            End If
            Return Lst
        End Function
        ' D0941 ==

        ' D0478 ===
        Public Function isUniquePasscode(ByVal sPasscode As String, ByVal tSkipProjectID As Integer) As Boolean
            Dim Prj As clsProject = DBProjectByPasscode(sPasscode.Trim) ' D1712
            Dim fExists As Boolean = Not Prj Is Nothing
            If tSkipProjectID > 0 AndAlso fExists Then fExists = Prj.ID <> tSkipProjectID
            Return Not fExists
        End Function

        Public Function ProjectUniquePasscode(ByVal sPasscode As String, ByVal tSkipProjectID As Integer) As String
            Dim tmpPasscode As String = sPasscode.Trim  ' D1712
            Dim cnt As Integer = 0
            While cnt < 10 AndAlso (tmpPasscode = "" OrElse Not isUniquePasscode(tmpPasscode, tSkipProjectID))  ' D0443 + D1709
                cnt += 1
                'tmpPasscode = GetRandomString(_DEF_PASSCODE_LENGTH, True, False)    ' D1709
                tmpPasscode = clsMeetingID.AsString(clsMeetingID.ReNew(True), clsMeetingID.ecMeetingIDType.Passcode) ' D2674 + D4920
            End While
            Return tmpPasscode
        End Function

        Private Function CheckProjectUserCouldBeImported(ByVal tUser As clsUser, ByVal tOwnerUser As clsApplicationUser) As Boolean
            If tUser Is Nothing Then Return False ' D0218
            'Dim fAllow As Boolean = tUser.UserID <> COMBINED_USER_ID 'C0555
            Dim fAllow As Boolean = Not IsCombinedUserID(tUser.UserID)  'C0555 + D0513 + D0831 + D0916
            If fAllow AndAlso Not tOwnerUser Is Nothing Then fAllow = fAllow Or tUser.UserID = tOwnerUser.UserID '' And tUser.Active
            Return fAllow
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="tAHPUser"></param>
        ''' <param name="tAppUser"></param>
        ''' <param name="fCheckUserEmail"></param>
        ''' <returns></returns>
        ''' <remarks>Pass tAppUser as Nothing for read from DB</remarks>
        Public Function CheckProjectUserEmail(ByVal tAHPUser As clsUser, ByRef tAppUser As clsApplicationUser, ByVal fCheckUserEmail As Boolean) As Boolean    ' D0085 + D0345
            If tAHPUser Is Nothing Then Return False
            If tAHPUser.UserEMail.Trim = "" Then Return False ' D0916
            Dim fAddUser As Boolean = True
            'tAppUser = Nothing ' -D0830
            If tAHPUser.UserEMail.ToLower = ActiveUser.UserEmail.ToLower Then
                tAppUser = ActiveUser
            Else
                If tAppUser Is Nothing Then tAppUser = DBUserByEmail(tAHPUser.UserEMail) ' D0916
            End If
            If tAHPUser.UserEMail.ToLower <> ActiveUser.UserEmail.ToLower AndAlso fCheckUserEmail Then
                If Not isValidEmail(tAHPUser.UserEMail) Then
                    fAddUser = Not tAppUser Is Nothing  ' D0916
                End If
            End If
            Return fAddUser
        End Function

        Public Function ImportProjectUsers(ByVal tProject As clsProject, ByVal tOwnerUser As clsApplicationUser, ByVal fAllowBlankPasswords As Boolean, ByVal fCheckUserEmails As Boolean) As Boolean    ' D0093 + D0272 + D0345
            Dim fImported As Boolean = False
            If Not ActiveWorkgroup Is Nothing And Not tProject Is Nothing Then ' D0093
                Dim ProjectUsers As List(Of clsUser) = MiscFuncs.GetUsersList(tProject.ConnectionString, clsProject.StorageType, tProject.ProviderType, tProject.ID)    'C0281 + D0369 + D0379 + D0478
                ' D0341 ===
                If ProjectUsers Is Nothing Then
                    DebugInfo("Can't read Users from the decision!", _TRACE_WARNING)
                    ProjectUsers = New List(Of clsUser)
                End If
                ' D0341 + D0478 ==

                ' D0811 ===
                If _USE_USER_EMAIL_FOR_EMPTY_FACILITATOR AndAlso tOwnerUser IsNot Nothing Then
                    For Each tUsr As clsUser In ProjectUsers
                        If tUsr.UserEMail = "" AndAlso tUsr.UserID = 0 AndAlso tUsr.UserName.ToLower = "facilitator" AndAlso MiscFuncs.GetUserByEmail(ProjectUsers, tOwnerUser.UserEmail) Is Nothing Then   ' D0916
                            tUsr.UserEMail = tOwnerUser.UserEmail
                            ' D0926 ===
                            Dim tAHPUSer As clsUser = tProject.ProjectManager.GetUserByID(tUsr.UserID)
                            If tAHPUSer IsNot Nothing AndAlso tAHPUSer.UserEMail = "" Then
                                tAHPUSer.UserEMail = tOwnerUser.UserEmail
                                tProject.ProjectManager.StorageManager.Writer.SaveModelStructure()
                            End If
                            ' D0926 ==
                            Exit For
                        End If
                    Next
                End If
                ' D0811 ==

                ' check current owner in this project: add him if not exists
                If Not tOwnerUser Is Nothing Then   ' D0093
                    If MiscFuncs.GetUserByEmail(ProjectUsers, ActiveUser.UserEmail) Is Nothing Then ' D0129 + D0174
                        'Dim Owner As New clsUser
                        'Owner.UserEMail = ActiveUser.UserEmail
                        'Owner.UserName = ActiveUser.UserName
                        'Owner.Active = True
                        'Owner.LastJudgmentTime = Now
                        ''Owner.UserID = MiscFuncs.AddUserToProject_CanvasDatabase(tProject.ConnectionString, tProject.ProviderType, Owner.UserEMail, Owner.UserName, Owner.Active)  ' D0174 + D0329 'C0271 + D0844
                        ''Owner.UserID = ActiveUser.UserID 'C0271 -D0844
                        'tProject.ProjectManager.AddUser(Owner)      ' D0843 -D0844

                        ' D0848 ===
                        Dim tAHPUSer As clsUser = tProject.ProjectManager.AddUser(tOwnerUser.UserEmail, True, tOwnerUser.UserName)      ' D0843
                        If tAHPUSer IsNot Nothing Then
                            If tAHPUSer.UserName = "" Then tAHPUSer.UserName = tOwnerUser.UserName  ' D6725
                            tProject.ProjectManager.StorageManager.Writer.SaveModelStructure() ' D0843
                            ProjectUsers.Add(tAHPUSer)   ' D0844
                        End If
                        ' D0848 ===
                        'ProjectUsers.Add(tProject.ProjectManager.AddUser(Owner.UserEMail, True, Owner.UserName))
                    End If
                End If

                ImportUsersFromProjectManager(tProject, tOwnerUser, ProjectUsers, fCheckUserEmails, fAllowBlankPasswords)    ' D6600
                CheckProjectManagerUsers(tProject, False)  ' D6725
                fImported = True

            End If

            Return fImported
        End Function

        ' D6600 ===
        Public Function ImportUsersFromProjectManager(tProject As clsProject, tOwnerUser As clsApplicationUser, ProjectUsers As List(Of clsUser), fCheckUserEmails As Boolean, fAllowBlankPasswords As Boolean) As Integer
            Dim fAttached As Integer = 0 ' D0830

            ' D0478 ===
            ' detect the workgroup
            Dim ActiveWG As clsWorkgroup = ActiveWorkgroup
            If Not ActiveWG Is Nothing AndAlso tProject.WorkgroupID > 0 AndAlso ActiveWG.ID <> tProject.WorkgroupID Then ActiveWG = Nothing
            If ActiveWG Is Nothing Then ActiveWG = DBWorkgroupByID(tProject.WorkgroupID)

            ' detect the role groups
            Dim DefProjectEvaluatorGrpID As Integer = ActiveWG.GetDefaultRoleGroupID(ecRoleLevel.rlModelLevel, ecRoleGroupType.gtEvaluator)
            ' D0087 ===
            Dim DefProjectOwnerGrpID As Integer = ActiveWG.GetDefaultRoleGroupID(ecRoleLevel.rlModelLevel, ecRoleGroupType.gtProjectManager)    ' D2780
            Dim DefProjectOrganizerGrpID As Integer = ActiveWG.GetDefaultRoleGroupID(ecRoleLevel.rlApplicationLevel, ecRoleGroupType.gtProjectOrganizer)    ' D0727 + D2780
            Dim DefUserGrpID As Integer = ActiveWG.GetDefaultRoleGroupID(ecRoleLevel.rlApplicationLevel, ecRoleGroupType.gtUser)
            ' D0087 ==

            ' get list of already attached users (as list of workspaces and userworkgroups)
            Dim ProjectWS As List(Of clsWorkspace) = DBWorkspacesByProjectID(tProject.ID)
            Dim UWList As List(Of clsUserWorkgroup) = DBUserWorkgroupsByWorkgroupID(ActiveWG.ID)   ' D0840 + D0843
            ' D0478 ==

            Dim fPerformUserCleanup As Boolean = False  ' D1520
            ' check every user and attach, if required
            For Each tPrjUser As clsUser In ProjectUsers
                tPrjUser.UserEMail = tPrjUser.UserEMail.Trim    ' D0478
                If tPrjUser.UserEMail <> "" Then
                    If CheckProjectUserCouldBeImported(tPrjUser, tOwnerUser) Then ' D0085 + D0129 + D0478
                        Dim tAppUser As clsApplicationUser = Nothing    ' D0478
                        ' check user e-mail and get his data if exists in master db
                        If CheckProjectUserEmail(tPrjUser, tAppUser, fCheckUserEmails) Then  ' D0083 + D0345 + D0478
                            Dim DoAttach As Boolean = False
                            Dim isProjectManager As Boolean = False ' D0117
                            Dim isProjectOwner As Boolean = False   ' D0117
                            Dim fCanBePM As Boolean = False         ' D3276
                            ' this is new user?
                            If tAppUser Is Nothing Then
                                tAppUser = New clsApplicationUser
                                tAppUser.UserEmail = tPrjUser.UserEMail.Trim    ' D0225
                                tAppUser.UserName = tPrjUser.UserName
                                tAppUser.Active = tPrjUser.Active
                                tAppUser.Comment = String.Format("Imported from model '{0}'", IIf(tProject.ProjectName = "", tProject.Passcode, tProject.ProjectName))  ' D0916
                                tAppUser.OwnerID = ActiveUser.UserID    ' D0086
                                tAppUser.PasswordStatus = -1    ' D2215
                                If Not fAllowBlankPasswords And tAppUser.UserPassword = "" Then tAppUser.UserPassword = GetRandomString(_DEF_PASSWORD_LENGTH, True, True) ' D0272
                                If DBUserUpdate(tAppUser, True, Nothing) Then DoAttach = True
                            Else
                                ' is this user is already attached to this project?
                                ' D3276 ===
                                Dim tWS As clsWorkspace = clsWorkspace.WorkspaceByUserIDAndProjectID(tAppUser.UserID, tProject.ID, ProjectWS)
                                If tWS Is Nothing Then DoAttach = True ' D0478
                                fCanBePM = CanUserBePM(ActiveWG, tAppUser.UserID, tProject, False, False, clsUserWorkgroup.UserWorkgroupByUserIDAndWorkgroupID(tAppUser.UserID, ActiveWG.ID, UWList))
                                ' D3276 ==
                            End If
                            ' is project owner?
                            isProjectOwner = fCanBePM OrElse tAppUser.UserID = ActiveUser.UserID OrElse tPrjUser.UserID = FACILITATOR_USER_ID   ' D0117 + D3276
                            Dim fCanAttach As Boolean = True    ' D1519
                            If DoAttach Then
                                ' D0844 ===
                                Dim fAdded As Boolean = False
                                If clsUserWorkgroup.UserWorkgroupByUserIDAndWorkgroupID(tAppUser.UserID, tProject.WorkgroupID, UWList) Is Nothing Then
                                    ' D1490 ===
                                    Dim tmpUW As clsUserWorkgroup = AttachWorkgroup(tAppUser.UserID, ActiveWG, CInt(IIf(isProjectManager, DefProjectOrganizerGrpID, DefUserGrpID)), Nothing, True)  ' D2780
                                    If tmpUW Is Nothing AndAlso ApplicationError.Status = ecErrorStatus.errWrongLicense Then
                                        LicenseInitError(ApplicationError.Message, True)
                                        ApplicationError.Reset()
                                        fCanAttach = False  ' D1519
                                        fPerformUserCleanup = True ' D1520
                                    Else
                                        UWList.Add(tmpUW) ' D0830 + D0840
                                        fAdded = True
                                    End If
                                    ' D1490 ==
                                End If
                                If fCanAttach AndAlso clsWorkspace.WorkspaceByUserIDAndProjectID(tAppUser.UserID, tProject.ID, ProjectWS) Is Nothing Then
                                    ' D1490 ===
                                    Dim tmpWS As clsWorkspace = AttachProject(tAppUser, tProject, False, CInt(IIf(isProjectOwner, DefProjectOwnerGrpID, DefProjectEvaluatorGrpID)), Nothing, True)  ' D2287
                                    If tmpWS Is Nothing AndAlso ApplicationError.Status = ecErrorStatus.errWrongLicense Then
                                        LicenseInitError(ApplicationError.Message, True)
                                        ApplicationError.Reset()
                                        fPerformUserCleanup = True  ' D1520
                                        'Exit For
                                    Else
                                        ProjectWS.Add(tmpWS) ' D0830
                                        fAdded = True
                                    End If
                                    ' D1490 ==
                                End If
                                If fAdded Then fAttached += 1
                                ' D0844 ==
                            End If
                        End If
                    End If
                End If
            Next

            If fAttached > 0 Then DBSaveLog(dbActionType.actModify, dbObjectType.einfProject, tProject.ID, String.Format("{0} user(s) attached to project", fAttached), "")    ' D0830 + D1382

            ' D1520 ===
            If fPerformUserCleanup Then
                Dim tPrjUsersReal As List(Of clsApplicationUser) = DBUsersByProjectID(tProject.ID)
                Dim tUsersForDelete As New List(Of Integer)
                For Each tUser As clsUser In tProject.ProjectManager.UsersList
                    If tUser.UserID >= 0 Then
                        If clsApplicationUser.UserByUserEmail(tUser.UserEMail, tPrjUsersReal) Is Nothing Then tUsersForDelete.Add(tUser.UserID)
                    End If
                Next
                tProject.ProjectManager.DeleteUsers(tUsersForDelete)
            End If
            ' D1520 ==

            Return fAttached
        End Function
        ' D6600 ==

        ' D2688 ===
        Public Function CanUserBePM(tWkg As clsWorkgroup, tUserID As Integer, tProject As clsProject, fInitLicenseError As Boolean, fCheckCanBePM As Boolean, Optional tUW As clsUserWorkgroup = Nothing, Optional tWS As clsWorkspace = Nothing) As Boolean    ' D3196
            Dim fResult = False
            If tUW Is Nothing Then tUW = DBUserWorkgroupByUserIDWorkgroupID(tUserID, tWkg.ID)
            Dim fCanBePM As Boolean = tUW IsNot Nothing AndAlso (CanUserDoAction(ecActionType.at_alCanBePM, tUW, tWkg) OrElse CanUserDoAction(ecActionType.at_alManageAnyModel, tUW, tWkg))    ' D2782
            ' D2704 ===
            If fCheckCanBePM Then   ' D3196
                Dim fIsPM As Boolean = False
                If tProject IsNot Nothing Then
                    If tWS Is Nothing Then tWS = DBWorkspaceByUserIDProjectID(tUserID, tProject.ID)
                    If tWS IsNot Nothing Then fIsPM = CanUserModifyProject(tUserID, tProject.ID, tUW, tWS, ActiveWorkgroup)
                End If
                Dim fLicPM As Boolean = tWkg.License.CheckParameterByID(ecLicenseParameter.MaxPMsInProject, tProject, fIsPM)
                ' D2704 ==
                If Not fCanBePM OrElse (fInitLicenseError AndAlso Not fLicPM) Then
                    If Not fCanBePM Then
                        DebugInfo("User can't be project manager because can't create projects. Attached as regular evalautor")
                    End If
                    If Not fLicPM AndAlso ApplicationError.Status = ecErrorStatus.errNone Then  ' D3196
                        DebugInfo("License limit for max project managers is over. Attached as regular evalautor")
                        If fInitLicenseError Then LicenseInitError(LicenseErrorMessage(tWkg.License, ecLicenseParameter.MaxPMsInProject, True), True)
                    End If
                Else
                    fResult = True  ' D3196
                End If
            Else
                fResult = fCanBePM
            End If
            Return fResult
        End Function
        ' D2688 ==

        Public Function AttachProject(ByVal tUser As clsApplicationUser, ByVal tProject As clsProject, fUseLoginRoleGroupID As Boolean, ByVal tRoleGroupID As Integer, Optional ByVal sLogMessage As String = "", Optional ByVal fAddWithoutCheckWS As Boolean = False) As clsWorkspace  ' D0830
            If tUser Is Nothing Or tProject Is Nothing Then Return Nothing
            ' D0830 ===
            Dim WS As clsWorkspace = Nothing
            If Not fAddWithoutCheckWS Then
                WS = clsWorkspace.WorkspaceByUserIDAndProjectID(tUser.UserID, tProject.ID, Workspaces)
                If WS Is Nothing Then WS = DBWorkspaceByUserIDProjectID(tUser.UserID, tProject.ID)
            End If
            ' D0830 ==
            If WS Is Nothing Then

                ' D1490 ===
                Dim tWkg As clsWorkgroup = ActiveWorkgroup
                If tWkg Is Nothing OrElse tWkg.ID <> tProject.WorkgroupID Then tWkg = DBWorkgroupByID(tProject.WorkgroupID, True, True) ' D2287

                Dim tUW As clsUserWorkgroup = DBUserWorkgroupByUserIDWorkgroupID(tUser.UserID, tWkg.ID) ' D2688

                Dim fCanAdd As Boolean = True
                Dim fCanBeEvaluator As Boolean = True   ' D2688
                Dim tPMGrpID As Integer = tWkg.GetDefaultRoleGroupID(ecRoleLevel.rlModelLevel, ecRoleGroupType.gtProjectManager)  ' D2688 + D2780

                If Not tUser.CannotBeDeleted AndAlso tWkg IsNot Nothing AndAlso tWkg.License IsNot Nothing AndAlso tWkg.License.isValidLicense Then
                    If Not tWkg.License.CheckParameterByID(ecLicenseParameter.MaxUsersInProject, tProject, False) Then fCanAdd = False
                    ' D2688 ===
                    If CanUserDoAction(ecActionType.at_alManageAnyModel, tUW, tWkg) Then
                        If tRoleGroupID <> tPMGrpID Then
                            tRoleGroupID = tPMGrpID
                            fCanBeEvaluator = False
                            DebugInfo("User can't be evaluator because can manage any model")
                        End If
                    End If
                    ' D2688 ==
                End If

                ' D2688 ===
                If fCanAdd AndAlso tRoleGroupID = tPMGrpID Then
                    If Not CanUserBePM(tWkg, tUser.UserID, tProject, True, True, tUW, WS) Then  ' D3196
                        If fCanBeEvaluator Then
                            tRoleGroupID = tWkg.GetDefaultRoleGroupID(ecRoleLevel.rlModelLevel, ecRoleGroupType.gtEvaluator)
                        Else
                            fCanAdd = Not tUser.CannotBeDeleted
                            DebugInfo("Can't attach user if he can't be PM")
                        End If
                    End If
                End If
                ' D2688 ==

                If fCanAdd Then
                    ' D2287 ===
                    If fUseLoginRoleGroupID AndAlso Options.WorkgroupRoleGroupID >= 0 Then
                        Dim tRole As clsRoleGroup = tWkg.RoleGroup(Options.WorkgroupRoleGroupID, tWkg.RoleGroups)
                        If tRole IsNot Nothing AndAlso tRole.RoleLevel = ecRoleLevel.rlModelLevel AndAlso tRole.Status <> ecRoleGroupStatus.gsDisabled Then tRoleGroupID = tRole.ID
                    End If
                    If fUseLoginRoleGroupID AndAlso Options.JoinAsPMonAttachProject AndAlso CanUserBePM(tWkg, tUser.UserID, tProject, False, True, tUW, WS) Then tRoleGroupID = tPMGrpID ' D4332
                    Options.JoinAsPMonAttachProject = False ' D4332
                    ' D2287 ==
                    ' D1490 ==
                    DebugInfo("Attach user to decision")    ' D0827
                    WS = New clsWorkspace
                    WS.UserID = tUser.UserID
                    WS.ProjectID = tProject.ID
                    WS.GroupID = tRoleGroupID
                    If sLogMessage = "" Then sLogMessage = String.Format("Attach user '{0}' to project '{1}'", tUser.UserEmail, tProject.Passcode) ' D0830
                    ' D0848 ===
                    If DBWorkspaceUpdate(WS, True, sLogMessage) Then    ' D0830
                        If tProject.ProjectManager.GetUserByEMail(tUser.UserEmail) Is Nothing Then
                            DebugInfo("Add user to project manager")    ' D0827
                            Dim tOldGrp As Integer = tProject.ProjectManager.DefaultGroupID ' D2163
                            ' If Options.UserRoleGroupID >= 0 Then tProject.ProjectManager.DefaultGroupID = Options.UserRoleGroupID ' D2163
                            tProject.ProjectManager.DefaultGroupID = Options.UserRoleGroupID ' D2163 + D7472
                            Dim tAHPUser As clsUser = tProject.ProjectManager.AddUser(tUser.UserEmail, tUser.Active, tUser.UserName)
                            If tAHPUser IsNot Nothing Then
                                tAHPUser.SyncEvaluationMode = SynchronousEvaluationMode.semNone ' D2641
                                tProject.ProjectManager.StorageManager.Writer.SaveModelStructure()
                            End If
                            If ActiveUser IsNot Nothing AndAlso ActiveUser.UserEmail.ToLower = tUser.UserEmail.ToLower Then CheckAndAssignUserRole(tProject, tUser.UserEmail) ' D1937
                            tProject.ProjectManager.DefaultGroupID = tOldGrp    ' D2163
                        End If
                    End If
                    ' D0848 ==
                    ' D1490 ===
                Else
                    If tWkg IsNot Nothing Then LicenseInitError(LicenseErrorMessage(tWkg.License, ecLicenseParameter.MaxUsersInProject, True), True)
                End If
                ' D1490 ==
            End If
            Return WS
        End Function

        ' D3954 ===
        Public Function AttachProjectUsers(ByVal UsersList As List(Of String), ByVal UserPermissions As ecRoleGroupType, ByVal fAllowBlankPassword As Boolean, ByVal fUpdateExistingUsers As Boolean, fAttach4TeamTime As Boolean, ByRef NewUsersAdded As List(Of Integer), ByRef _CantBePMList As List(Of Integer)) As List(Of clsApplicationUser)
            Dim ResList As New List(Of clsApplicationUser)
            If UsersList IsNot Nothing AndAlso ActiveUser IsNot Nothing AndAlso ActiveUserWorkgroup IsNot Nothing AndAlso CanUserModifyProject(ActiveUser.UserID, ProjectID, ActiveUserWorkgroup, ActiveWorkspace) Then ' D6492
                Dim DefGrpID As Integer = ActiveWorkgroup.GetDefaultRoleGroupID(ecRoleLevel.rlModelLevel, UserPermissions)
                Dim PMGrpID As Integer = ActiveWorkgroup.GetDefaultRoleGroupID(ecRoleLevel.rlModelLevel, ecRoleGroupType.gtProjectManager)
                Dim sUserExists As String = ""
                Dim fSaveUsers As Boolean = False
                Dim sLst As String = ""
                For Each tUserString As String In UsersList
                    Dim _defGrpID As Integer = DefGrpID ' D6707
                    tUserString = tUserString.Replace(vbTab, " ").Replace("  ", " ").Trim
                    Dim div_idx As Integer = tUserString.IndexOf(" ", 0)
                    If div_idx < 0 Then div_idx = tUserString.Length
                    Dim sEmail As String = tUserString.Substring(0, div_idx).Trim
                    Dim sName As String = tUserString.Substring(div_idx).Trim
                    If sEmail <> "" Then
                        Dim sPassword As String = ""
                        If Not fAllowBlankPassword Then sPassword = GetRandomString(_DEF_PASSWORD_LENGTH, True, True)
                        Dim tUser As clsApplicationUser = UserWithSignup(sEmail, CStr(IIf(sName = "", sEmail, sName)), sPassword, "Attached to project", sUserExists, True)
                        If tUser IsNot Nothing AndAlso tUser.UserID <> ActiveUser.UserID Then
                            Dim UW As clsUserWorkgroup = AttachWorkgroupByProject(tUser.UserID, ActiveProject, ecRoleGroupType.gtUser, True)
                            Dim tUserWS As clsWorkspace = Nothing
                            If Not UW Is Nothing Then
                                If sName <> "" Then tUser.UserName = sName
                                Dim tGrpID As Integer = DefGrpID
                                If tUser.CannotBeDeleted OrElse CanUserDoAction(ecActionType.at_alManageAnyModel, UW) Then
                                    tGrpID = PMGrpID
                                    _defGrpID = PMGrpID
                                End If
                                tUserWS = AttachProject(tUser, ActiveProject, False, tGrpID)
                            End If
                            Dim fSaveWS As String = ""
                            If tUserWS Is Nothing OrElse fUpdateExistingUsers Then
                                If tUserWS Is Nothing Then tUserWS = DBWorkspaceByUserIDProjectID(tUser.UserID, ProjectID)
                                If tUserWS IsNot Nothing AndAlso tUserWS.GroupID <> _defGrpID Then
                                    tUserWS.GroupID = _defGrpID
                                    If _defGrpID = PMGrpID Then
                                        If Not CanUserBePM(ActiveWorkgroup, tUser.UserID, ActiveProject, True, True, UW, tUserWS) Then
                                            tUserWS.GroupID = ActiveWorkgroup.GetDefaultRoleGroupID(ecRoleLevel.rlModelLevel, ecRoleGroupType.gtEvaluator)
                                            _CantBePMList.Add(tUser.UserID)
                                        End If
                                    End If
                                    fSaveWS = "Set user role on attach to project"
                                End If
                            End If
                            If tUserWS IsNot Nothing Then
                                ResList.Add(tUser)
                                sLst += String.Format("{0}'{1}'", IIf(sLst = "", "", ","), tUser.UserEmail)
                                Dim tPrjUser As ECTypes.clsUser = ActiveProject.ProjectManager.GetUserByEMail(tUser.UserEmail)

                                If fAttach4TeamTime Then
                                    If ActiveProject.isImpact Then tUserWS.TeamTimeStatusImpact = ecWorkspaceStatus.wsSynhronousActive Else tUserWS.TeamTimeStatusLikelihood = ecWorkspaceStatus.wsSynhronousActive
                                    If tPrjUser IsNot Nothing AndAlso tPrjUser.SyncEvaluationMode <> SynchronousEvaluationMode.semOnline Then
                                        tPrjUser.SyncEvaluationMode = SynchronousEvaluationMode.semOnline
                                        fSaveUsers = True
                                    End If
                                    fSaveWS = "Attach user to TeamTime"
                                End If
                                If fSaveWS <> "" Then DBWorkspaceUpdate(tUserWS, False, fSaveWS)
                                '-A1562 If sUserExists = "" AndAlso ActiveWorkgroup.Status = ecWorkgroupStatus.wsEnabled AndAlso NewUsersAdded IsNot Nothing Then NewUsersAdded.Add(tUser.UserID)
                                If ActiveWorkgroup.Status = ecWorkgroupStatus.wsEnabled AndAlso NewUsersAdded IsNot Nothing Then NewUsersAdded.Add(tUser.UserID) 'A1562
                                ' D0760 ==
                            End If
                        End If
                    End If
                Next
                If fSaveUsers Then ActiveProject.SaveUsersInfo(, "Add users to project", , , sLst)
            End If

            Return ResList
        End Function
        ' D3954 ==

        Public Function DetachProject(ByVal tProject As clsProject, ByVal tUser As clsApplicationUser, fRemovePrjUser As Boolean) As Boolean    ' D3954
            Dim fResult As Boolean = False  ' D3954
            If tProject IsNot Nothing AndAlso tUser IsNot Nothing Then ' D0218 + D3954
                DBTinyURLDelete(-1, tProject.ID, tUser.UserID)  ' D0899
                Dim WS As clsWorkspace = clsWorkspace.WorkspaceByUserIDAndProjectID(tUser.UserID, tProject.ID, Workspaces)
                If WS Is Nothing Then WS = DBWorkspaceByUserIDProjectID(tUser.UserID, tProject.ID)
                If Not WS Is Nothing Then fResult = DBWorkspaceDelete(WS.ID, String.Format("Detach user '{0}' from project '{1}", tUser.UserEmail, tProject.Passcode))
                ' D3954 ===
                If fResult AndAlso fRemovePrjUser Then
                    Dim tPrjUser As ECTypes.clsUser = tProject.ProjectManager.GetUserByEMail(tUser.UserEmail)
                    If Not tPrjUser Is Nothing Then tProject.ProjectManager.DeleteUser(tUser.UserEmail, False)
                End If
            End If
            Return fResult
            ' D3954 ==
        End Function
        ' D0478 ==

        ' D4719 ===
        Public Function DetachWorkgroup(ByVal UserIDs As List(Of String)) As Integer
            Dim tRemoved As Integer = 0
            If UserIDs IsNot Nothing AndAlso ActiveWorkgroup IsNot Nothing Then ' D1627
                Dim tUWList As List(Of clsUserWorkgroup) = DBUserWorkgroupsByWorkgroupID(ActiveWorkgroup.ID)
                Dim tWkgList As List(Of clsWorkgroup) = Nothing ' D4606
                If ActiveWorkgroup.Status = ecWorkgroupStatus.wsSystem Then tWkgList = DBWorkgroupsAll(True, True) ' D4606
                For Each ID As String In UserIDs
                    If ID <> ActiveUser.UserEmail Then ' D1726
                        Dim usr = DBUserByEmail(ID)
                        If usr IsNot Nothing Then   ' D7311
                            Dim tUW As clsUserWorkgroup = clsUserWorkgroup.UserWorkgroupByUserIDAndWorkgroupID(usr.UserID, ActiveWorkgroup.ID, tUWList)
                            If tUW IsNot Nothing Then
                                ' D4606 ===
                                If DBUserWorkgroupDelete(tUW, False, "Detach user from workgroup (HTML)") Then
                                    If ActiveWorkgroup.Status = ecWorkgroupStatus.wsSystem Then
                                        Dim tUser_UWList As List(Of clsUserWorkgroup) = DBUserWorkgroupsByUserID(usr.UserID)
                                        CheckUserWorkgroups(DBUserByID(usr.UserID), tWkgList, tUser_UWList)
                                    End If
                                Else
                                    tUW = Nothing
                                End If
                                ' D4606 ==
                            End If
                            If tUW IsNot Nothing Then tRemoved += 1
                        End If
                    End If
                Next
            End If
            Return tRemoved
        End Function
        ' D4719 ==

        ' D1937 ===
        Public Function CheckAndAssignUserRole(tProject As clsProject, sUserEmail As String) As Boolean
            Dim fSet As Boolean = False
            If Options.UserRoleGroupID >= 0 AndAlso tProject IsNot Nothing AndAlso tProject.isValidDBVersion Then
                Dim tAHPUser As clsUser = tProject.ProjectManager.GetUserByEMail(sUserEmail)
                If tAHPUser IsNot Nothing Then
                    Dim fChanged As Boolean = False
                    For Each tGroup As clsCombinedGroup In tProject.ProjectManager.CombinedGroups.GroupsList
                        If tGroup.ID = Options.UserRoleGroupID Then
                            If Not tGroup.ContainsUser(tAHPUser) Then
                                CType(tGroup, clsCombinedGroup).UsersList.Add(tAHPUser)
                                fChanged = True
                            End If
                            ' -D2162 // don't remove user from the other groups if already linked
                            'Else
                            '    If tGroup.ContainsUser(tAHPUser) Then
                            '        CType(tGroup, clsCombinedGroup).UsersList.Remove(tAHPUser)
                            '        fChanged = True
                            '    End If
                        End If
                    Next
                    If fChanged Then fSet = tProject.SaveStructure("Update user role", , , sUserEmail) ' D3731
                End If
            End If
            Return fSet
        End Function
        ' D1937 ==

        ' D1382 ===
        Public Function CheckAndAddReviewAccount(ByVal sReviewAccount As String, ByVal tProject As clsProject, Optional ByVal PMGroupID As Integer = -1) As Boolean
            Dim fAdded As Boolean = False
            If sReviewAccount <> "" AndAlso ReviewAccountEnabled(sReviewAccount <> "") AndAlso tProject IsNot Nothing AndAlso ActiveWorkgroup IsNot Nothing Then
                Dim tUser As clsApplicationUser = UserWithSignup(sReviewAccount, ResString("ReviewAccountName"), "", "Internal EC review model account", Nothing, True) ' D2215
                If tUser IsNot Nothing Then
                    If PMGroupID < 0 Then PMGroupID = ActiveWorkgroup.GetDefaultRoleGroupID(ecRoleLevel.rlModelLevel, ecRoleGroupType.gtProjectManager) ' D2780
                    If AttachWorkgroupByProject(tUser.UserID, tProject, ecRoleGroupType.gtProjectOrganizer, True) IsNot Nothing AndAlso AttachProject(tUser, tProject, False, PMGroupID, "", False) IsNot Nothing Then   ' D2287 + D2644 + D2780
                        DBSaveLog(dbActionType.actModify, dbObjectType.einfProject, tProject.ID, "Attach EC Review Account", "")
                        fAdded = True
                    End If
                End If
            End If
            Return fAdded
        End Function
        ' D1382 ==

        ' D3571 ===
        Public Sub onProjectSavingEvent(tProject As clsProject, sMessage As String, isCriticalChange As Boolean, sSnapshotComment As String)    ' D3731
        End Sub

        Public Sub onProjectSavedEvent(tProject As clsProject, sMessage As String, isCriticalChange As Boolean, sSnapshotComment As String) ' D3731
            SaveProjectLogEvent(tProject, sMessage, isCriticalChange, sSnapshotComment) ' D3731
        End Sub

        Public Sub SaveProjectLogEvent(tProject As clsProject, sMessage As String, isCriticalChange As Boolean, sSnapshotComment As String) ' D3731
            Dim tSnapshot As clsSnapshot = Nothing
            If tProject IsNot Nothing Then tSnapshot = SnapshotSaveProject(CType(IIf(isCriticalChange, ecSnapShotType.RestorePoint, ecSnapShotType.Auto), ecSnapShotType), sMessage, tProject.ID, isCriticalChange, sSnapshotComment) ' D3573 + D3731
            If _OPT_SAVE_PROJECT_DETAILED_LOGS AndAlso tProject IsNot Nothing AndAlso (LastPrjLogDT < Now.AddSeconds(-1) OrElse sMessage <> LastPrjLogMsg) Then
                Dim sComment As String = ""
                If tSnapshot IsNot Nothing AndAlso isCriticalChange Then sComment = String.Format("Add restore point {0}", tSnapshot.SnapshotID) ' D3576
                DBSaveLog(dbActionType.actModify, dbObjectType.einfProject, tProject.ID, sMessage, sComment)    ' D3573
                LastPrjLogDT = Now
                LastPrjLogMsg = sMessage
            End If
        End Sub
        ' D3571 ==

        Public Sub onProjectUpdateLastModifyEvent(tProject As clsProject)
            If tProject IsNot Nothing Then
                Dim tRes As Nullable(Of DateTime) = DBUpdateDateTime(_TABLE_PROJECTS, _FLD_PROJECTS_LASTMODIFY, tProject.ID)
                If tRes.HasValue Then
                    tProject.LastModify = tRes.Value
                    tProject.LastVisited = tRes.Value
                    If tProject.IsProjectLoaded AndAlso tProject.ProjectManager IsNot Nothing AndAlso (Not tProject.ProjectManager.LastModifyTime.HasValue OrElse tProject.ProjectManager.LastModifyTime.Value.AddSeconds(3) < tRes.Value) Then tProject.ProjectManager.LastModifyTime = tRes.Value ' D4541
                End If
            End If
        End Sub

#End Region


#Region "Workgroups"

        ' D0465 ===
        Public Property SystemWorkgroup() As clsWorkgroup
            Get
                If _SystemWorkgroup Is Nothing Then
                    Dim Lst As List(Of clsWorkgroup) = DBWorkgroupsByStatus(ecWorkgroupStatus.wsSystem)
                    If Lst.Count > 0 Then _SystemWorkgroup = Lst(0)
                End If
                Return _SystemWorkgroup
            End Get
            Set(ByVal value As clsWorkgroup)
                _SystemWorkgroup = value
            End Set
        End Property

        ' D0584 ===
        Public Property StartupWorkgroup() As clsWorkgroup
            Get
                If _StartupWorkgroup Is Nothing Then _StartupWorkgroup = DBWorkgroupByName(_DB_DEFAULT_STARTUPWORKGROUP_NAME)
                Return _StartupWorkgroup
            End Get
            Set(ByVal value As clsWorkgroup)
                _StartupWorkgroup = value
            End Set
        End Property
        ' D0584 ==

        ' D0587 ===
        Public Function isStartupWorkgroup(ByVal tWorkgroup As clsWorkgroup) As Boolean
            If tWorkgroup IsNot Nothing Then If String.Compare(tWorkgroup.Name, _DB_DEFAULT_STARTUPWORKGROUP_NAME, True) = 0 Then Return True
            Return False
        End Function
        ' D0587 ==

        Public Property ActiveWorkgroup() As clsWorkgroup
            Get
                Return _CurrentWorkgroup
            End Get
            Set(ByVal value As clsWorkgroup)
                ' D0466 ===
                If _CurrentWorkgroup Is Nothing OrElse value Is Nothing OrElse (_CurrentWorkgroup IsNot Nothing AndAlso value IsNot Nothing AndAlso _CurrentWorkgroup.ID <> value.ID) Then   ' D6359
                    SetActiveWorkgroup(value)
                End If
                ' D0466 ==
            End Set
        End Property
        ' D0465 ==

        ' D0466 ===
        Private Sub SetActiveWorkgroup(ByVal tWorkgroup As clsWorkgroup)    ' D0471
            If Not _CurrentWorkgroup Is tWorkgroup Then
                If tWorkgroup Is Nothing Then DebugInfo("Reset active workgroup", _TRACE_INFO) Else DebugInfo(String.Format("Set active workgroup as '{0}'", tWorkgroup.Name), _TRACE_INFO) ' D0471
                _IsRisk = Nothing   ' D6328
                If Not tWorkgroup Is Nothing Then
                    ' D2212 + D2289 ===
                    If CanvasMasterDBVersion >= "0.9995" AndAlso tWorkgroup.LastVisited.HasValue AndAlso tWorkgroup.LastVisited.Value.AddDays(1) < Now Then
                        Dim tRes As Integer = Database.ExecuteSQL("SELECT * FROM WorkgroupParams WHERE WorkgroupID=" + tWorkgroup.ID.ToString)
                        If tRes <= 0 Then DBWorkgroupExtractLicenseParameters(tWorkgroup)
                    End If
                    ' D2212 + D2289 ==
                    tWorkgroup.LastVisited = Now    ' D0494
                    CheckWorkgroup(tWorkgroup, tWorkgroup.Status = ecWorkgroupStatus.wsSystem) ' D0475 + D2256
                    DBUpdateDateTime(_TABLE_WORKGROUPS, _FLD_WORKGROUPS_LASTVISITED, tWorkgroup.ID, _FLD_WORKGROUPS_ID)    ' D0491
                    ' D0494 ===
                    If Not ActiveUser Is Nothing Then
                        ActiveUser.Session.WorkgroupID = tWorkgroup.ID
                        ActiveUser.Session.LastAccess = Now
                        Try ' D0496
                            Dim tParams As New List(Of Object)
                            tParams.Add(Now)
                            tParams.Add(tWorkgroup.ID)
                            tParams.Add(ActiveUser.UserID)
                            Database.ExecuteSQL(String.Format("UPDATE {0} SET LastVisited=?, LastWorkgroupID=? WHERE {1}=?", _TABLE_USERS, _FLD_USERS_ID), tParams)
                        Catch ex As Exception   ' D0496
                        End Try
                        ' D2427 ===
                        'If tWorkgroup.License IsNot Nothing AndAlso tWorkgroup.License.isValidLicense AndAlso tWorkgroup.License.CheckParameterByID(ecLicenseParameter.RiskEnabled, Nothing, True) Then    ' -D2476
                        CheckWordingTemplates(tWorkgroup.WordingTemplates, tWorkgroup.License.isValidLicense AndAlso tWorkgroup.License.CheckParameterByID(ecLicenseParameter.RiskEnabled))  ' D2432 + D4413
                        'End If
                        ' D2427 ==
                    End If
                    ' D0494 ==
                    _LastDT = Nothing   ' D3966
                End If
                If _CurrentWorkgroup IsNot Nothing AndAlso ProjectID > 0 Then SetActiveProject(Nothing)   ' D6635
                _CurrentWorkgroup = tWorkgroup
                _ProjectsList = Nothing         ' D0469 
                '_WorkgroupProjects = Nothing    ' D0469 -D0511
                _SurveysList = Nothing          ' D0488
                SetUserWorkgroup(Nothing)
                _DefaultPipeParamSets = Nothing ' D2256
                ' D0969 ===
                If ActiveUser IsNot Nothing AndAlso Not _CheckedUserWorkgroups Then
                    Dim WkgList As New List(Of clsWorkgroup)
                    Dim sData As List(Of Dictionary(Of String, Object)) = Database.SelectBySQL("SELECT UW.*, R.RoleLevel, R.GroupType FROM UserWorkgroups UW LEFT JOIN RoleGroups R ON R.ID = UW.RoleGroupID LEFT JOIN Workgroups W ON W.ID=R.WorkgroupID WHERE UW.WorkgroupID<>W.ID")
                    For Each tRow As Dictionary(Of String, Object) In sData
                        Dim tUW As clsUserWorkgroup = DBParse_UserWorkgroup(tRow)
                        If tUW IsNot Nothing Then
                            Dim tWkg As clsWorkgroup = clsWorkgroup.WorkgroupByID(tUW.WorkgroupID, WkgList)
                            If tWkg Is Nothing Then
                                tWkg = DBWorkgroupByID(tUW.WorkgroupID, True, True)
                                If tWkg IsNot Nothing Then WkgList.Add(tWkg)
                            End If
                            If tWkg IsNot Nothing Then
                                Dim tRoleLevel As Integer = -1
                                Dim tGroupType As Integer = -1
                                If Not IsDBNull(tRow("RoleLevel")) Then tRoleLevel = CInt(tRow("RoleLevel"))
                                If Not IsDBNull(tRow("GroupType")) Then tGroupType = CInt(tRow("GroupType"))
                                Dim RoleGrp As Integer = tWkg.GetDefaultRoleGroupID(CType(tRoleLevel, ecRoleLevel), CType(tGroupType, ecRoleGroupType))
                                If RoleGrp > 0 Then
                                    tUW.RoleGroupID = RoleGrp
                                    DBUserWorkgroupUpdate(tUW, False, "Fix wrong reference to RoleGroup")
                                End If
                            End If
                        End If
                    Next
                    ' D2868 ===
                    If ActiveUser IsNot Nothing AndAlso ActiveUser.CannotBeDeleted AndAlso tWorkgroup IsNot Nothing AndAlso tWorkgroup.Status <> ecWorkgroupStatus.wsSystem AndAlso tWorkgroup.RoleGroups IsNot Nothing Then
                        Dim tUW As clsUserWorkgroup = DBUserWorkgroupByUserIDWorkgroupID(ActiveUser.UserID, tWorkgroup.ID)
                        If tUW IsNot Nothing Then
                            Dim tAdminID As Integer = tWorkgroup.GetDefaultRoleGroupID(ecRoleLevel.rlApplicationLevel, ecRoleGroupType.gtAdministrator)
                            If tAdminID > 0 AndAlso tUW.RoleGroupID <> tAdminID Then
                                tUW.RoleGroupID = tAdminID
                                If DBUserWorkgroupUpdate(tUW, False, "Fix wrong admin permissions in workgroup") Then
                                    UserWorkgroups = Nothing
                                    SetUserWorkgroup(Nothing)
                                End If
                            End If
                        End If
                    End If
                    ' D2868 ==
                    _CheckedUserWorkgroups = True
                End If
                ' D0969 ==
                CheckIsEvalSiteOnly()   ' D6359
            End If
        End Sub
        ' D0466 ==

        ' D6359 ===
        Public Sub CheckIsEvalSiteOnly()
            _isEvalSiteDefault = If(isRiskEnabled, Options.EvalURL4Anytime_Riskion, Options.EvalURL4Anytime) AndAlso Options.EvalSiteURL <> "" AndAlso Options.EvalSiteOnly  ' D6267
        End Sub

        ' D6267 ===
        Public ReadOnly Property isEvalURL_EvalSite As Boolean
            Get
                Return _isEvalSiteDefault
            End Get
        End Property
        ' D6267 + D6359 ==

        ' D6410 ===
        Public Function CanUseEvalURLForProject(tProject As clsProject) As Boolean
            Dim fUseEvalSite As Boolean = Options.EvalSiteURL <> "" AndAlso Not isRiskEnabled
            If fUseEvalSite AndAlso tProject IsNot Nothing Then    ' check dynamic surveys
                Dim SurveyUsers As New Dictionary(Of String, clsComparionUser)
                Dim tSurveyInfo As clsSurveyInfo = Nothing
                If tProject.PipeParameters.ShowWelcomeSurvey Then
                    tSurveyInfo = SurveysManager.GetSurveyInfoByProjectID(tProject.ID, SurveyType.stWelcomeSurvey, SurveyUsers)
                    If tSurveyInfo IsNot Nothing Then
                        If tSurveyInfo.Survey("").isSurveyContainsPipeModifiers Then fUseEvalSite = False
                    End If
                End If
                If tProject.PipeParameters.ShowThankYouSurvey Then
                    tSurveyInfo = SurveysManager.GetSurveyInfoByProjectID(tProject.ID, SurveyType.stThankyouSurvey, SurveyUsers)
                    If tSurveyInfo IsNot Nothing Then
                        If tSurveyInfo.Survey("").isSurveyContainsPipeModifiers Then fUseEvalSite = False
                    End If
                End If
            End If
            Return fUseEvalSite
        End Function
        ' D6410 ==

        ' D4700 ===
        Public Function CheckExpiredAutoSnapshots() As Boolean
            Dim fHasExpired As Boolean = False
            If _OPT_SNAPSHOTS_ENABLE AndAlso _OPT_SNAPSHOTS_EXPIRATION_DAYS > 1 Then
                Dim tExtra = DBExtraRead(clsExtra.Params2Extra(-1, ecExtraType.Common, ecExtraProperty.CheckExpiredSnapshotsDT))
                Dim Val As Long = 0
                If tExtra Is Nothing OrElse (tExtra.Value IsNot Nothing AndAlso Long.TryParse(CStr(tExtra.Value), Val)) Then
                    Dim DT As DateTime = DateTime.FromBinary(Val)
                    If DT.AddHours(12) < Now Then
                        Dim tParams As New List(Of Object)
                        tParams.Add(Now.AddDays(-_OPT_SNAPSHOTS_EXPIRATION_DAYS))
                        Dim tCnt As Object = Database.ExecuteScalarSQL(String.Format("DELETE FROM {0} WHERE {1}<? AND {2}={3}", _TABLE_SNAPSHOTS, _FLD_SNAPSHOTS_DT, _FLD_SNAPSHOTS_TYPE, CInt(ecSnapShotType.Auto)), tParams)
                        If tCnt IsNot Nothing AndAlso CInt(tCnt) > 0 Then
                            fHasExpired = True
                            DBSaveLog(dbActionType.actDelete, dbObjectType.einfSnapshot, -1, "Wipeout expired snapshot(s)", CStr(tCnt))
                        End If
                        DBExtraWrite(clsExtra.Params2Extra(-1, ecExtraType.Common, ecExtraProperty.CheckExpiredSnapshotsDT, Now.ToBinary))
                    End If
                End If
            End If
            Return fHasExpired
        End Function
        ' D4700 ==

        ' D2601 ===
        Public Function CheckWorkgroupMasterProjects(tWorkgroup As clsWorkgroup, fCheckOnly As Boolean, fForceProjectUpdate As Boolean) As Boolean  ' D2617
            Dim fHasOutdated As Boolean = False ' D2603
            If tWorkgroup IsNot Nothing AndAlso tWorkgroup.Status <> ecWorkgroupStatus.wsSystem Then    ' D4564
                DebugInfo("Check for default option sets in startup workgroup", _TRACE_INFO)
                ' D2657 ===
                If Not fForceProjectUpdate Then
                    Dim tExtra = DBExtraRead(clsExtra.Params2Extra(tWorkgroup.ID, ecExtraType.Workgroup, ecExtraProperty.CheckMasterProjectsDate))
                    Dim Val As Long = 0
                    If tExtra IsNot Nothing AndAlso tExtra.Value IsNot Nothing AndAlso Long.TryParse(CStr(tExtra.Value), Val) Then
                        Dim DT As DateTime = DateTime.FromBinary(Val)
                        If DT.AddSeconds(_DEF_SYNC_MASTER_PRJ_TIMEOUT) > Now Then Return False
                    End If
                End If
                ' D2657 ==
                Dim spath As String = CStr(IIf(isRiskEnabled, _FILE_DATA_MASTERPROJECTS_RISK, _FILE_DATA_MASTERPROJECTS))
                Dim fmasterfiles As List(Of String) = GetProjectFilesList(spath, {_FILE_EXT_AHPS})
                If fmasterfiles.Count > 0 Then
                    DebugInfo(String.Format("Found default option sets: {0}. Check existing workgroup projects.", fmasterfiles.Count), _TRACE_INFO)
                    ' D2632 ===
                    Dim sCommonDt As DateTime = DateTime.MinValue
                    If My.Computer.FileSystem.FileExists(spath + _FILE_MASTERPROJECTS_DT) Then
                        Dim sDT As String = My.Computer.FileSystem.ReadAllText(spath + _FILE_MASTERPROJECTS_DT)
                        sDT = sDT.Replace(vbCr, "").Replace(vbLf, "").Trim
                        If Not DateTime.TryParse(sDT, sCommonDt) Then sCommonDt = DateTime.MinValue Else DebugInfo(String.Format("Read last update time: {0}", sCommonDt), _TRACE_INFO)
                    End If
                    ' D2632 ==

                    Dim tprjlist As List(Of clsProject) = DBProjectsByWorkgroupID(tWorkgroup.ID)

                    If My.Computer.FileSystem.FileExists(spath + _FILE_MASTERPROJECTS_WIPE) Then
                        Dim sWipes As String = My.Computer.FileSystem.ReadAllText(spath + _FILE_MASTERPROJECTS_WIPE)
                        Dim sList As String() = sWipes.Replace(vbLf, "").Trim.Split(CChar(vbCr))
                        For Each sWName As String In sList
                            sWName = sWName.Replace(".ahps", "").Replace(".ahpz", "").Replace(".ahp", "").Trim
                            If sWName <> "" Then
                                For Each tprj As clsProject In tprjlist
                                    If tprj.ProjectStatus = ecProjectStatus.psMasterProject AndAlso Not tprj.isMarkedAsDeleted AndAlso sWName = tprj.ProjectName Then
                                        tprj.isMarkedAsDeleted = True
                                        DBProjectUpdate(tprj, False, "Mark default option sets as deleted due to request from wipeslist.txt")
                                    End If
                                Next
                            End If
                        Next
                    End If

                    For Each sfilename As String In fmasterfiles
                        If My.Computer.FileSystem.FileExists(spath + sfilename) Then
                            Try
                                Dim tmppm As New clsProjectManager(True, False, isRiskEnabled)
                                tmppm.StorageManager.ProviderType = DBProviderType.dbptODBC
                                tmppm.StorageManager.ProjectLocation = spath + sfilename
                                tmppm.StorageManager.StorageType = ECModelStorageType.mstAHPSFile
                                tmppm.StorageManager.ReadDBVersion()
                                If tmppm.LoadProject(tmppm.StorageManager.ProjectLocation, DBProviderType.dbptODBC, tmppm.StorageManager.StorageType) Then
                                    Dim tmasterprj As clsProject = Nothing
                                    Dim sname = IO.Path.GetFileNameWithoutExtension(sfilename).Trim
                                    If Not Guid.Equals(tmppm.PipeParameters.ProjectGuid, Guid.Empty) Then
                                        Dim prjguid As Guid = tmppm.PipeParameters.ProjectGuid
                                        Dim tmplist As List(Of clsProject) = clsProject.ProjectsByGUID(prjguid, tprjlist, False)
                                        If tmplist.Count > 0 Then
                                            tmplist.Sort(New clsProjectComparer(ecProjectSort.srtProjectDateTime, Web.UI.WebControls.SortDirection.Descending, Nothing))
                                            ' D2603 ===
                                            Dim idx As Integer = 0
                                            While tmasterprj Is Nothing AndAlso idx < tmplist.Count
                                                If tmplist(idx).ProjectStatus = ecProjectStatus.psMasterProject AndAlso Not tmplist(idx).isMarkedAsDeleted Then tmasterprj = tmplist(idx)
                                                idx += 1
                                            End While
                                            ' D2603 ==
                                            If tmasterprj IsNot Nothing Then DebugInfo(String.Format("Found default option sets by GUID #{0} '{1}' ({2})", tmasterprj.ID, tmasterprj.ProjectName, tmasterprj.ProjectGUID), _TRACE_INFO)
                                        End If
                                    End If
                                    If tmasterprj Is Nothing Then
                                        For Each tprj As clsProject In tprjlist
                                            If tprj.ProjectStatus = ecProjectStatus.psMasterProject AndAlso Not tprj.isMarkedAsDeleted AndAlso sname = tprj.ProjectName Then    ' D2603
                                                tmasterprj = tprj
                                                DebugInfo(String.Format("Found default option sets by name #{0} '{1}' ({2})", tmasterprj.ID, tmasterprj.ProjectName, tmasterprj.Passcode), _TRACE_INFO)
                                                Exit For
                                            End If
                                        Next
                                    End If
                                    If tmasterprj IsNot Nothing Then
                                        Dim tprjinfo As FileInfo = My.Computer.FileSystem.GetFileInfo(spath + sfilename)
                                        If tprjinfo IsNot Nothing Then
                                            ' D2632 ===
                                            Dim DT As DateTime = tprjinfo.CreationTime
                                            If sCommonDt <> DateTime.MinValue Then DT = sCommonDt
                                            If ((Not fCheckOnly AndAlso fForceProjectUpdate) OrElse
                                                (tmasterprj.Created.HasValue AndAlso Not tmasterprj.LastModify.HasValue AndAlso DT > tmasterprj.Created.Value) OrElse
                                                (tmasterprj.LastModify.HasValue AndAlso DT > tmasterprj.LastModify.Value)) Then  ' D2617
                                                ' D2632 ==
                                                DebugInfo(String.Format("Need to update project #{0} '{1}' ({2})", tmasterprj.ID, tmasterprj.ProjectName, tmasterprj.Passcode), _TRACE_INFO)
                                                ' D2603 ===
                                                fHasOutdated = True
                                                If fCheckOnly Then
                                                    Exit For
                                                Else
                                                    Dim fUpdated As Boolean = False
                                                    'If tmasterprj.IsProjectLoaded Then tmasterprj.ProjectManager.CloseProject()
                                                    tmasterprj.ResetProject(True)
                                                    Dim OrigPrjID As Integer = tmasterprj.ID
                                                    Dim NewPrjID As Integer = -tmasterprj.ID
                                                    Dim FS As IO.FileStream = Nothing
                                                    Try
                                                        FS = New IO.FileStream(spath + sfilename, FileMode.Open, FileAccess.Read)
                                                        tmasterprj.ID = NewPrjID
                                                        fUpdated = tmasterprj.ProjectManager.StorageManager.Writer.SaveFullProjectStream(FS)
                                                    Catch ex As Exception
                                                        DebugInfo(ex.Message, _TRACE_RTE)
                                                        DBSaveLog(dbActionType.actUpgrade, dbObjectType.einfProject, OrigPrjID, "Unable to update default option set", ex.Message + vbCrLf + ex.StackTrace)
                                                    Finally
                                                        If Not FS Is Nothing Then FS.Close()
                                                        tmasterprj.ID = OrigPrjID
                                                        'tmasterprj.ProjectManager.CloseProject()
                                                        tmasterprj.ResetProject(True)
                                                        If fUpdated Then
                                                            DBProjectStreamDelete(OrigPrjID)
                                                            If Database.Connect Then
                                                                Dim affected As Integer = Database.ExecuteSQL(String.Format("UPDATE ModelStructure SET ProjectID={1} WHERE ProjectID={0}", NewPrjID, OrigPrjID))
                                                                affected = Database.ExecuteSQL(String.Format("UPDATE UserData SET ProjectID={1} WHERE ProjectID={0}", NewPrjID, OrigPrjID))
                                                            End If
                                                            DBUpdateDateTime(_TABLE_PROJECTS, _FLD_PROJECTS_LASTMODIFY, OrigPrjID, _FLD_PROJECTS_ID)
                                                            DBSaveLog(dbActionType.actUpgrade, dbObjectType.einfProject, OrigPrjID, "Update default project sets", "Success")
                                                        Else
                                                            DBProjectStreamDelete(NewPrjID)
                                                        End If
                                                    End Try
                                                End If
                                                ' D2603 ==
                                            End If
                                        End If
                                    Else
                                        DebugInfo(String.Format("Default option sets for '{0}' can't found. Need to create new one.", sname), _TRACE_INFO)
                                        fHasOutdated = True    ' D2603
                                        If fCheckOnly Then
                                            Exit For ' D2603
                                        Else
                                            Dim serror As String = ""
                                            tmasterprj = CreateModelFromFile(Me, spath + sfilename, ecProjectStatus.psMasterProject, tWorkgroup.ID, False, serror)
                                            If serror = "" Then ' D3361
                                                Dim tprjid As Integer = -1
                                                If tmasterprj IsNot Nothing Then tprjid = tmasterprj.ID
                                                DBSaveLog(dbActionType.actCreate, dbObjectType.einfProject, tprjid, "Create default option sets", CStr(IIf(serror = "", "ok", serror)))
                                            End If
                                        End If
                                    End If
                                    'tmppm.CloseProject()
                                End If
                            Catch ex As Exception
                            End Try
                        End If
                    Next
                Else
                    DebugInfo("Can't find default option sets files", _TRACE_INFO)
                End If
                DBExtraWrite(clsExtra.Params2Extra(tWorkgroup.ID, ecExtraType.Workgroup, ecExtraProperty.CheckMasterProjectsDate, Now.ToBinary))   ' D2657
            End If
            Return fHasOutdated
        End Function
        ' D2601 ==

        ' D2432 ===
        Public Function CheckWordingTemplates(ByRef Templates As Dictionary(Of String, String), isRisk As Boolean) As Boolean   '4413
            Dim TplWording As New Dictionary(Of String, String)
            Dim fHasChanges As Boolean = False
            ' D3626 ===
            'Dim isRisk As Boolean = tWorkgroup.License.isValidLicense AndAlso tWorkgroup.License.CheckParameterByID(ecLicenseParameter.RiskEnabled)    ' -D4413
            If Templates Is Nothing Then Templates = New Dictionary(Of String, String) ' D4413
            Dim sLst As String() = {}
            If isRisk Then sLst = _TPL_LIST_RISK Else sLst = _TPL_LIST_COMPARION
            For Each sName As String In sLst
                ' D3626 ==
                sName = sName.ToLower
                Dim sVal As String
                If Templates.ContainsKey(sName) AndAlso Not String.IsNullOrEmpty(Templates(sName)) Then ' D2431 + D4413
                    sVal = Templates(sName) ' D4413
                Else
                    sVal = ResString(String.Format(CStr(IIf(isRisk, "tpl_risk_{0}", "tpl_comparion_{0}")), sName.Replace("%%", ""))) ' D3636
                    fHasChanges = True
                End If
                TplWording.Add(sName, sVal.ToLower) ' D2448
            Next
            If fHasChanges Then
                Templates = Nothing     ' D4413
                Templates = TplWording  ' D4413
            End If
            Return fHasChanges  ' D2601
        End Function
        ' D2432 ==

        ' D0475 ===
        Public Function AttachWorkgroup(ByRef tUserID As Integer, ByVal tWorkgroup As clsWorkgroup, ByVal tRoleGroupID As Integer, Optional ByVal sLogMessage As String = "", Optional ByVal fAddWithoutCheckUserWkg As Boolean = False) As clsUserWorkgroup   ' D0093 + D0830
            ' D0478 ===
            If Not tWorkgroup Is Nothing Then
                Dim ExistedUWG As clsUserWorkgroup = Nothing    ' D0830

                If Not fAddWithoutCheckUserWkg Then ' D0830
                    ExistedUWG = DBUserWorkgroupByUserIDWorkgroupID(tUserID, tWorkgroup.ID)
                    If Not ExistedUWG Is Nothing Then
                        DebugInfo("User is already attached to workgroup", _TRACE_WARNING)
                        Return ExistedUWG
                    End If
                    ' D0478 ==
                End If

                ' D1490 ===
                Dim fCanAdd As Boolean = True
                If tWorkgroup.License IsNot Nothing AndAlso tWorkgroup.License.isValidLicense Then
                    ' D2685 ===
                    If Not tWorkgroup.License.CheckParameterByID(ecLicenseParameter.MaxUsersInWorkgroup, tWorkgroup, False) Then
                        LicenseInitError(LicenseErrorMessage(tWorkgroup.License, ecLicenseParameter.MaxUsersInWorkgroup, True), True) ' D2685
                        fCanAdd = False
                    Else
                        Dim tGroup As clsRoleGroup = tWorkgroup.RoleGroup(tRoleGroupID, tWorkgroup.RoleGroups)
                        'If tGroup IsNot Nothing AndAlso tGroup.RoleLevel = ecRoleLevel.rlApplicationLevel AndAlso (tGroup.GroupType = ecRoleGroupType.gtWorkgroupManager OrElse tGroup.GroupType = ecRoleGroupType.gtAdministrator OrElse tGroup.GroupType = ecRoleGroupType.gtECAccountManager OrElse tGroup.GroupType = ecRoleGroupType.gtProjectCreator OrElse tGroup.GroupType = ecRoleGroupType.gtTechSupport) Then
                        If tGroup IsNot Nothing AndAlso tGroup.RoleLevel = ecRoleLevel.rlApplicationLevel AndAlso (tGroup.GroupType = ecRoleGroupType.gtProjectOrganizer OrElse tGroup.GroupType = ecRoleGroupType.gtTechSupport) Then  ' D2780 + D3288
                            If Not tWorkgroup.License.CheckParameterByID(ecLicenseParameter.MaxProjectCreatorsInWorkgroup, tWorkgroup, False) Then
                                fCanAdd = False
                                LicenseInitError(LicenseErrorMessage(tWorkgroup.License, ecLicenseParameter.MaxProjectCreatorsInWorkgroup, True), True)
                            End If
                        End If
                    End If
                    ' D2685 ==
                End If

                If fCanAdd Then
                    ' D1490 ==
                    Dim NewUserWG As New clsUserWorkgroup
                    NewUserWG.UserID = tUserID
                    NewUserWG.Status = ecUserWorkgroupStatus.uwEnabled
                    NewUserWG.WorkgroupID = tWorkgroup.ID
                    NewUserWG.RoleGroupID = tRoleGroupID
                    If DBUserWorkgroupUpdate(NewUserWG, True, sLogMessage) Then    ' D0052 + D0093
                        DebugInfo("User attached to workgroup")    ' D0092
                        If Not ActiveUser Is Nothing Then
                            If ActiveUser.UserID = tUserID Then UserWorkgroups = Nothing
                        End If
                        Return NewUserWG
                    End If
                    ' D1490 ===
                End If
                ' D1490 ==

            End If
            Return Nothing
        End Function
        ' D0475 ==

        ' D0478 ===
        Public Function AttachWorkgroupByProject(ByRef tUserID As Integer, ByVal tProject As clsProject, ByVal tWorkgroupGroupType As ecRoleGroupType, ByVal fIgnoreProjectPrivateStatus As Boolean) As clsUserWorkgroup    ' D0493
            If Not tProject Is Nothing Then ' D0493
                'If tProject.ProjectParticipating = ecProjectParticipating.ppAccessCodeEnabled Or fIgnoreProjectPrivateStatus Then   ' D0493 -D0748
                If tProject.isPublic Or fIgnoreProjectPrivateStatus Then   ' D0493 + D0748
                    Dim WG As clsWorkgroup = Nothing
                    If Not ActiveWorkgroup Is Nothing Then
                        If ActiveWorkgroup.ID = tProject.WorkgroupID Then WG = ActiveWorkgroup ' D0493
                    End If
                    If WG Is Nothing Then WG = DBWorkgroupByID(tProject.WorkgroupID)
                    If Not WG Is Nothing Then
                        Return AttachWorkgroup(tUserID, WG, WG.GetDefaultRoleGroupID(ecRoleLevel.rlApplicationLevel, tWorkgroupGroupType), String.Format("Attached by passcode '{0}'", tProject.Passcode)) ' D0046 + D0052 + D0093 + D0493
                    End If
                End If
            End If
            Return Nothing
        End Function
        ' D0478 ==

#End Region


#Region "User workgroups"

        ' D0465 ===
        Public Property ActiveUserWorkgroup() As clsUserWorkgroup
            Get
                If _CurrentUserWorkgroup Is Nothing Then
                    If Not ActiveUser Is Nothing AndAlso Not ActiveWorkgroup Is Nothing Then
                        If Not UserWorkgroups Is Nothing Then _CurrentUserWorkgroup = clsUserWorkgroup.UserWorkgroupByUserIDAndWorkgroupID(ActiveUser.UserID, ActiveWorkgroup.ID, UserWorkgroups)
                        If _CurrentUserWorkgroup Is Nothing Then _CurrentUserWorkgroup = DBUserWorkgroupByUserIDWorkgroupID(ActiveUser.UserID, ActiveWorkgroup.ID)
                    End If
                End If
                Return _CurrentUserWorkgroup
            End Get
            Set(ByVal value As clsUserWorkgroup)
                SetUserWorkgroup(value) ' D0466
            End Set
        End Property

        Public Property UserWorkgroups() As List(Of clsUserWorkgroup)
            Get
                ' D0466 ===
                If _UserWorkgroups Is Nothing Then
                    If Not ActiveUser Is Nothing Then
                        _ActiveAvailableWorkgroups = Nothing    ' D4659
                        _UserWorkgroups = DBUserWorkgroupsByUserID(ActiveUser.UserID)
                    End If
                End If
                ' D0466 ==
                Return _UserWorkgroups
            End Get
            Set(ByVal value As List(Of clsUserWorkgroup))
                ' D0475 ===
                If Not _UserWorkgroups Is value Then
                    _UserWorkgroups = value
                    _CurrentWorkspace = Nothing
                    If Not ActiveWorkgroup Is Nothing AndAlso Not SystemWorkgroup Is Nothing AndAlso ActiveWorkgroup.ID = SystemWorkgroup.ID Then _SystemUserWorkgroups = Nothing ' D0502
                    _ActiveAvailableWorkgroups = Nothing    ' D4659
                End If
                ' D0475 ==
            End Set
        End Property

        Public ReadOnly Property SystemUserWorkgroups() As List(Of clsUserWorkgroup)
            Get
                If _SystemUserWorkgroups Is Nothing Then
                    If SystemWorkgroup Is Nothing Then Return New List(Of clsUserWorkgroup)
                    _SystemUserWorkgroups = DBUserWorkgroupsByWorkgroupID(SystemWorkgroup.ID)
                End If
                Return _SystemUserWorkgroups
            End Get
        End Property
        ' D0465 ==

        ' D0466 ===
        Private Sub SetUserWorkgroup(ByVal tUserWorkgroup As clsUserWorkgroup)
            If Not _CurrentUserWorkgroup Is tUserWorkgroup Then
                If tUserWorkgroup Is Nothing Then
                    DebugInfo("Reset active user workgroup", _TRACE_INFO)
                Else
                    DebugInfo("Set active user workgroup", _TRACE_INFO) ' D0471
                    DBUpdateDateTime(_TABLE_USERWORKGROUPS, _FLD_USERWRKG_LASTVISITED, tUserWorkgroup.ID, _FLD_USERWRKG_ID)    ' D0491
                End If
                _CurrentUserWorkgroup = tUserWorkgroup
                _EULA_checked = False   ' D0473
                _EULA_valid = True      ' D0473
                SetActiveProject(Nothing)
                _CurrentRoleGroup = Nothing
                _CurrentWorkspace = Nothing
                _Workspaces = Nothing   ' D0474
                ' D0491 ===
                If Not tUserWorkgroup Is Nothing AndAlso Options.RestoreLastVisitedProject AndAlso Database.Connect AndAlso CanvasMasterDBVersion >= "0.92" Then    ' D0515
                    Dim tRes As Object = Database.ExecuteScalarSQL(String.Format("SELECT {3} FROM {0} WHERE {1}={2}", _TABLE_USERWORKGROUPS, _FLD_USERWRKG_ID, tUserWorkgroup.ID, _FLD_USERWRKG_LASTPROJECTID))
                    If Not tRes Is Nothing AndAlso Not IsDBNull(tRes) Then ProjectID = CInt(tRes)
                End If
                ' D0491 ==
            End If
        End Sub
        ' D0466 ==

#End Region


#Region "Role Groups"

        ' D0465 ===
        Public ReadOnly Property ActiveRoleGroup() As clsRoleGroup
            Get
                If _CurrentRoleGroup Is Nothing AndAlso Not ActiveWorkgroup Is Nothing AndAlso Not ActiveUserWorkgroup Is Nothing Then
                    If ActiveWorkgroup.RoleGroups Is Nothing Then ActiveWorkgroup.RoleGroups = DBRoleGroupsByWorkgroupID(ActiveWorkgroup.ID, True)
                    _CurrentRoleGroup = ActiveWorkgroup.RoleGroup(ActiveUserWorkgroup.RoleGroupID, ActiveWorkgroup.RoleGroups)
                End If
                Return _CurrentRoleGroup
            End Get
        End Property
        ' D0465 ==

        ' D0466 ===
        Public ReadOnly Property ActiveProjectRoleGroup() As clsRoleGroup
            Get
                If _CurrentProjectGroup Is Nothing Then
                    If Not ActiveWorkspace Is Nothing Then
                        _CurrentProjectGroup = ActiveWorkgroup.RoleGroup(ActiveWorkspace.GroupID)   ' D4939
                    End If
                End If
                Return _CurrentProjectGroup ' D4939
            End Get
        End Property
        ' D0466 ==

        Public Function UpdateUserRoleGroup(UserIDs As List(Of Integer), UserPermission As ecRoleGroupType, UsersList As List(Of clsApplicationUser), WSList As List(Of clsWorkspace), UWPrjList As List(Of clsUserWorkgroup)) As Integer
            Dim retVal As Integer = 0
            Dim fSavePrjUsers As Boolean = False

            Dim PRJ As clsProject = ActiveProject
            Dim RoleGroupID As Integer = ActiveWorkgroup.GetDefaultRoleGroupID(ecRoleLevel.rlModelLevel, UserPermission)
            Dim sMessage As String = String.Format("Set project user group '{0}'", ActiveWorkgroup.RoleGroup(RoleGroupID).Name)
            Dim tPMGrpID As Integer = ActiveWorkgroup.GetDefaultRoleGroupID(ecRoleLevel.rlModelLevel, ecRoleGroupType.gtProjectManager)
            Dim ViewerGrpID As Integer = ActiveWorkgroup.GetDefaultRoleGroupID(ecRoleLevel.rlModelLevel, ecRoleGroupType.gtViewer)
            Dim fCheckPrjOwner As Boolean = RoleGroupID = tPMGrpID

            Dim sList As String = ""
            For Each ID As Integer In UserIDs
                Dim tUser As clsApplicationUser = clsApplicationUser.UserByUserID(ID, UsersList)
                Dim tWS As clsWorkspace = clsWorkspace.WorkspaceByUserIDAndProjectID(ID, ProjectID, WSList)
                Dim tUW As clsUserWorkgroup = clsUserWorkgroup.UserWorkgroupByUserIDAndWorkgroupID(tWS.UserID, ActiveWorkgroup.ID, UWPrjList)
                If tWS IsNot Nothing AndAlso ID <> ActiveUser.UserID Then
                    If tWS.GroupID <> RoleGroupID Then
                        ' when need to set as PM, need to check, can we do it
                        If fCheckPrjOwner AndAlso Not CanUserBePM(ActiveWorkgroup, ID, PRJ, True, True, tUW, tWS) Then
                            tWS = Nothing
                        Else
                            Dim fCanUpdate As Boolean = True
                            ' if current user is PM, but WkgManager/Admin, need to check, is it possible to reduce his permissions
                            If tWS.GroupID = tPMGrpID Then
                                If CanUserDoAction(ecActionType.at_alManageAnyModel, tUW, ActiveWorkgroup) Then fCanUpdate = False
                            End If
                            If fCanUpdate Then
                                tWS.GroupID = RoleGroupID
                                If Not DBWorkspaceUpdate(tWS, False, sMessage) Then tWS = Nothing Else If tUser IsNot Nothing Then sList += String.Format("{0}'{1}'", IIf(sList = "", "", ","), tUser.UserEmail)
                            End If
                        End If

                        If tWS IsNot Nothing AndAlso tUser IsNot Nothing Then
                            Dim tPrjUser As ECTypes.clsUser = PRJ.ProjectManager.GetUserByEMail(tUser.UserEmail)
                            If tPrjUser IsNot Nothing Then
                                tPrjUser.Active = tWS.GroupID <> ViewerGrpID
                                fSavePrjUsers = True
                            End If
                        End If
                    End If
                End If
                If tWS IsNot Nothing AndAlso tWS.GroupID = RoleGroupID Then retVal += 1
            Next
            If sList <> "" Then SaveProjectLogEvent(PRJ, "Update user role group", False, sList)
            If fSavePrjUsers Then PRJ.SaveUsersInfo(, "Update project user(s) rolegroup")
            If UserIDs.Contains(ActiveUser.UserID) Then Workspaces = Nothing

            'If App.ApplicationError.Status = ecErrorStatus.errWrongLicense Then
            '    App.ApplicationError.Reset()
            'End If

            Return retVal
        End Function


#End Region


#Region "Workspaces"

        ' D0465 ===
        Public ReadOnly Property ActiveWorkspace() As clsWorkspace
            Get
                If _CurrentWorkspace Is Nothing Then
                    If HasActiveProject() AndAlso isAuthorized Then
                        If Not Workspaces Is Nothing Then _CurrentWorkspace = clsWorkspace.WorkspaceByUserIDAndProjectID(ActiveUser.UserID, ProjectID, Workspaces)
                        If _CurrentWorkspace Is Nothing Then _CurrentWorkspace = DBWorkspaceByUserIDProjectID(ActiveUser.UserID, ProjectID)
                    End If
                End If
                Return _CurrentWorkspace
            End Get
        End Property

        Public Property Workspaces() As List(Of clsWorkspace)
            Get
                ' D0474 ===
                If _Workspaces Is Nothing Then
                    If Not ActiveUser Is Nothing Then _Workspaces = DBWorkspacesByUserID(ActiveUser.UserID)
                End If
                ' D0474 ==
                Return _Workspaces
            End Get
            Set(ByVal value As List(Of clsWorkspace))
                _Workspaces = value
                _CurrentProjectGroup = Nothing  ' D7160
                _CurrentWorkspace = Nothing ' D5016
            End Set
        End Property
        ' D0465 ==

#End Region


#Region "User Templates"

        ' D2256 ===
        Public Property DefaultPipeParamSets() As Dictionary(Of String, String)
            Get
                If _DefaultPipeParamSets Is Nothing AndAlso Options.UseUserTemplates Then  ' D2485
                    ' D2365 ===
                    Dim sParamValue As String = ""
                    If isRiskEnabled Then sParamValue = Options.DefParamSetRisk
                    If sParamValue = "" Then sParamValue = Options.DefParamSet
                    ' D2365 ==
                    Dim Names As String() = sParamValue.Split(CChar(";"))
                    _DefaultPipeParamSets = New Dictionary(Of String, String)
                    If Names.Length > 0 Then
                        For Each sName As String In Names
                            Dim sFullName As String = _FILE_DATA_SETTINGS + sName.Trim
                            If My.Computer.FileSystem.FileExists(sFullName) Then
                                If sName.ToLower.EndsWith(".xml") Then sName = sName.Substring(0, sName.Length - 4)
                                sName = sName.Replace("_", "/").Replace("  ", " ").Trim
                                _DefaultPipeParamSets.Add(sName, sFullName)
                            End If
                        Next
                    End If
                    ' D0832 ==
                End If
                Return _DefaultPipeParamSets
            End Get
            Set(value As Dictionary(Of String, String))
                _DefaultPipeParamSets = value
            End Set
        End Property
        ' D2256 ==

        ' D0832 ===
        Private Sub CheckAndLoadDefaultUserTemplates(ByRef tUserTemplates As List(Of clsUserTemplate))
            If Not Options.UseUserTemplates Then Exit Sub ' D2485
            ' D2256 ===
            Dim sDefSet As Dictionary(Of String, String) = DefaultPipeParamSets
            If tUserTemplates Is Nothing Or sDefSet Is Nothing Or ActiveUser Is Nothing Then Exit Sub
            If tUserTemplates.Count = 0 AndAlso sDefSet.Count > 0 Then
                ' D2256 ==

                ' Check user at first: is he can create project at any workgroup
                Dim fCanManage As Boolean = False
                Dim WGlist As List(Of clsWorkgroup) = AvailableWorkgroups(ActiveUser, UserWorkgroups)
                For Each tUW As clsUserWorkgroup In UserWorkgroups
                    Dim tWG As clsWorkgroup = clsWorkgroup.WorkgroupByID(tUW.WorkgroupID, WGlist)
                    If tWG IsNot Nothing Then
                        Dim tRole As clsRoleGroup = tWG.RoleGroup(tUW.RoleGroupID, tWG.RoleGroups)
                        If tRole Is Nothing Then tRole = DBRoleGroupByID(tUW.RoleGroupID, True)
                        If tRole IsNot Nothing AndAlso tRole.ActionStatus(ecActionType.at_alCreateNewModel) = ecActionStatus.asGranted Then
                            fCanManage = True
                            Exit For
                        End If
                    End If
                Next

                If fCanManage Then

                    ' D2184 ===
                    If UserTemplatesDefault IsNot Nothing Then
                        For Each Templ As clsUserTemplate In UserTemplatesDefault
                            Dim UserTempl As clsUserTemplate = Templ.Clone
                            If ActiveUser IsNot Nothing Then UserTempl.UserID = ActiveUser.UserID ' D2185
                            UserTempl.IsCustom = False
                            If DBUserTemplateUpdate(UserTempl, True, String.Format("Import as default set '{0}'", UserTempl.TemplateName)) Then tUserTemplates.Add(UserTempl)
                        Next
                    End If
                    ' D2184 ==

                    ' -D2184
                    'For Each tParam As KeyValuePair(Of String, String) In Options.DefaultPipeParamSets
                    '    Dim Templ As New clsUserTemplate
                    '    Templ.TemplateName = tParam.Key
                    '    Templ.TemplateType = StructureType.stPipeOptions
                    '    Templ.TemplateData = New clsPipeParamaters
                    '    Templ.UserID = ActiveUser.UserID
                    '    Templ.Comment = "Default options set"
                    '    If Templ.TemplateData.Read(PipeStorageType.pstXMLFile, tParam.Value, DefaultProvider, -1) Then
                    '        If DBUserTemplateUpdate(Templ, True, String.Format("Import as default set '{0}'", tParam.Key)) Then tUserTeamplates.Add(Templ)
                    '    End If
                    'Next
                End If

            End If
        End Sub
        ' D0832 ==

        ' D0795 ===
        Public Property UserTemplates() As List(Of clsUserTemplate)
            Get
                If _UserTemplates Is Nothing AndAlso Options.UseUserTemplates Then ' D2485
                    If Not ActiveUser Is Nothing Then _UserTemplates = DBUserTemplatesByUserID(ActiveUser.UserID)
                    CheckAndLoadDefaultUserTemplates(_UserTemplates)    ' D0832
                End If
                Return _UserTemplates
            End Get
            Set(ByVal value As List(Of clsUserTemplate))
                _UserTemplates = value
            End Set
        End Property
        ' D0795 ==

        ' D2184 ===
        Public Property UserTemplatesDefault() As List(Of clsUserTemplate)
            Get
                If _UserTemplatesDefault Is Nothing AndAlso Options.UseUserTemplates Then  ' D2485
                    _UserTemplatesDefault = New List(Of clsUserTemplate)
                    For Each tParam As KeyValuePair(Of String, String) In DefaultPipeParamSets  ' D2256
                        Dim Templ As New clsUserTemplate
                        Templ.TemplateName = tParam.Key
                        Templ.TemplateType = StructureType.stPipeOptions
                        Templ.TemplateData = New clsPipeParamaters
                        Templ.UserID = ActiveUser.UserID
                        Templ.Comment = "Default options set"
                        'Templ.XMLName = tParam.Key
                        Templ.IsCustom = False
                        Templ.LastModify = File.GetLastWriteTime(tParam.Value)
                        If Templ.TemplateData.Read(PipeStorageType.pstXMLFile, tParam.Value, DefaultProvider, -1) Then
                            Templ.TemplateHash = Templ.CalculateHash
                            _UserTemplatesDefault.Add(Templ)
                        End If
                    Next
                End If
                Return _UserTemplatesDefault
            End Get
            Set(ByVal value As List(Of clsUserTemplate))
                _UserTemplatesDefault = value
            End Set
        End Property

        Public Function UserTemplatesCheckOutdated(fDoUpdateOutdated As Boolean) As Boolean
            Dim fHasOutdated As Boolean = False
            If Options.UseUserTemplates AndAlso UserTemplates IsNot Nothing AndAlso UserTemplatesDefault IsNot Nothing Then ' D2485
                For Each Templ As clsUserTemplate In UserTemplatesDefault
                    Dim UserTemp As clsUserTemplate = clsUserTemplate.UserTemplateByName(Templ.TemplateName, UserTemplates)
                    'If UserTemp IsNot Nothing AndAlso Not UserTemp.IsCustom AndAlso UserTemp.CalculateHash() <> Templ.TemplateHash Then
                    If UserTemp IsNot Nothing AndAlso UserTemp.CalculateHash() <> Templ.TemplateHash Then   ' D2185
                        If fDoUpdateOutdated Then
                            UserTemp.TemplateData = Templ.TemplateData
                            UserTemp.IsCustom = False
                            If ActiveUser IsNot Nothing Then UserTemp.UserID = ActiveUser.UserID ' D2185
                            'UserTemp.XMLName = Templ.XMLName
                            DBUserTemplateUpdate(UserTemp, False, String.Format("Reset options set to default '{0}'", Templ.TemplateName))
                        End If
                        fHasOutdated = True
                    End If
                Next
            End If
            If fHasOutdated AndAlso fDoUpdateOutdated Then UserTemplates = Nothing
            Return fHasOutdated
        End Function
        ' D2184 ==

#End Region


#Region "TinyURL and Pin"

        ' D0895 ===
        Public Function CreateTinyURL(ByVal URL As String, Optional ByVal ProjectID As Integer = -1, Optional ByVal userID As Integer = -1) As String       ' D0899
            Dim retVal As String = String.Empty
            Dim hash As String = GetMD5(URL)    ' D0896
            If _HashCache IsNot Nothing AndAlso _HashCache.ContainsKey(hash) Then Return hash ' D0899
            ' D0899 ===
            If CanvasMasterDBVersion >= "0.99" Then
                Dim SQL As String = "SELECT COUNT(*) as cnt FROM PrivateURLS WHERE Hash = ?"
                Dim tParams As New List(Of Object)
                tParams.Add(hash)
                Dim count As Integer = CInt(Database.ExecuteScalarSQL(SQL, tParams))
                ' D0899 ==
                If count = 0 Then
                    ' D0899 ===
                    tParams.Add(URL)
                    SQL = "INSERT INTO PrivateURLs (Hash, URL) VALUES (?, ?);"
                    If CanvasMasterDBVersion >= "0.991" Then
                        SQL = "INSERT INTO PrivateURLS (Hash, URL, ProjectID, UserID, Created) VALUES (?, ?, ?, ?, ?);"
                        tParams.Add(ProjectID)
                        tParams.Add(userID)
                        tParams.Add(Now)
                    End If
                    count = Database.ExecuteSQL(SQL, tParams)
                    ' D0899 ==
                End If
                If _HashCache IsNot Nothing AndAlso count > 0 Then _HashCache.Add(hash, URL) ' D0899 + D4936
                retVal = hash
            End If
            Return retVal
        End Function

        Public Function DecodeTinyURL(ByVal HashCode As String) As String
            Dim retVal As String = String.Empty
            If CanvasMasterDBVersion >= "0.99" Then  ' D0899
                ' D0899 ===
                Dim SQL As String = "SELECT url FROM PrivateURLs WHERE hash = ?"
                Dim tParams As New List(Of Object)
                tParams.Add(HashCode)
                Dim dt As Object = Database.ExecuteScalarSQL(SQL, tParams)
                ' D0899 ==
                If dt IsNot Nothing Then retVal = CStr(dt)
                If _HashCache IsNot Nothing AndAlso retVal <> "" AndAlso Not _HashCache.ContainsKey(HashCode) Then _HashCache.Add(HashCode, retVal) ' D0899
                If _HashCache IsNot Nothing AndAlso retVal = "" AndAlso _HashCache.ContainsKey(HashCode) Then retVal = _HashCache(HashCode)         ' D4936
            End If
            Return retVal
        End Function
        ' D0895 ==

        ' D7114 ===
        Public Sub HashCodesReset()
            If _HashCache IsNot Nothing Then _HashCache.Clear()
        End Sub
        ' D7114 ==

        ' D7187 ===
        Public Function GetUserPin(PinType As ecPinCodeType, UserID As Integer, Optional ProjectID As Integer = -1, Optional sExtraParams As String = "", Optional createNewPin As Boolean = False) As Tuple(Of Integer, Integer) ' D7501 + D7502
            Dim Pin As Integer = 0
            Dim Timeout As Integer = 0

            If UserID <> 0 AndAlso UserID <> -1 AndAlso CanvasMasterDBVersion >= "0.99" Then
                ' D7502 ===
                Dim PinPrefix As String = ""
                Dim PinTimeout As Integer = 60
                Select Case PinType
                    Case ecPinCodeType.mfaEmail
                        PinPrefix = _OPT_PREFIX_MFA_EMAIL
                        PinTimeout = _DEF_MFA_EMAIL_TIMEOUT
                    Case Else
                        PinPrefix = _OPT_PREFIX_PIN
                        PinTimeout = _DEF_PINCODE_TIMEOUT
                End Select
                ' D7502 ==

                Dim lastDT As DateTime = Now.AddSeconds(-PinTimeout)

                Dim SQL As String = ""
                Dim tParams As New List(Of Object)

                If Not createNewPin Then
                    SQL = "SELECT TOP(1) * FROM PrivateURLS WHERE UserID = ? AND Created > ? ORDER BY Created DESC"
                    tParams.Clear()
                    tParams.Add(UserID)
                    tParams.Add(lastDT)
                    Dim Data As List(Of Dictionary(Of String, Object)) = Database.SelectBySQL(SQL, tParams)
                    If Data.Count > 0 Then
                        Dim tRow = Data(0)
                        If Not IsDBNull(tRow("Hash")) Then
                            Dim sPin As String = CStr(tRow("Hash")).Substring(1)
                            If Not Integer.TryParse(sPin, Pin) Then createNewPin = True
                        End If
                        If Not IsDBNull(tRow("Created")) Then
                            Dim DT As DateTime = CType(tRow("Created"), DateTime)
                            Timeout = PinTimeout - CInt(Math.Ceiling(Now.Subtract(DT).TotalSeconds))    ' D7502
                            If Timeout < 5 Then createNewPin = True
                        End If
                    End If
                End If

                SQL = String.Format("DELETE FROM PrivateURLS WHERE Hash LIKE '" + PinPrefix + "%' AND (Created <= ? OR Created IS NULL{0})", If(createNewPin, " OR UserID = ?", ""))    ' D7501
                tParams.Clear()
                tParams.Add(lastDT)
                If createNewPin Then tParams.Add(UserID)
                Database.ExecuteSQL(SQL, tParams)

                Dim rand As New Random(DateTime.Now.Millisecond)
                If Pin = 0 Then

                    Dim Cnt As Integer = 10
                    While Pin = 0 AndAlso Cnt > 0
                        ' D7508 ===
                        Select Case PinType
                            Case ecPinCodeType.mfaEmail
                                Pin = rand.Next(100000, 999999)
                            Case Else
                                Pin = rand.Next(1000, 9999)
                        End Select
                        ' D7508 ==
                        SQL = String.Format("SELECT ID FROM PrivateURLS WHERE Hash = '{0}{1}'", PinPrefix, Pin)    ' D7501
                        If Database.ExecuteSQL(SQL) > 0 Then Pin = 0
                        Cnt -= 1
                    End While

                    If Pin > 0 Then
                        If sExtraParams.Length > 1023 Then sExtraParams = sExtraParams.Substring(0, 1023)
                        SQL = "INSERT INTO PrivateURLS (Hash, URL, ProjectID, UserID, Created) VALUES (?, ?, ?, ?, ?);"
                        tParams.Clear()
                        tParams.Add(String.Format("{0}{1}", PinPrefix, Pin))   ' D7501
                        tParams.Add(sExtraParams)
                        tParams.Add(ProjectID)
                        tParams.Add(UserID)
                        tParams.Add(Now)
                        Database.ExecuteSQL(SQL, tParams)
                        Timeout = PinTimeout - 1    ' D7502
                    End If
                End If
            End If
            Return Tuple.Create(Of Integer, Integer)(Pin, Timeout)
        End Function

        ' D7504 ===
        Public Function DeleteUserPin(PinType As ecPinCodeType, UserID As Integer) As Boolean
            Dim tRes As Boolean = False
            If UserID <> 0 AndAlso UserID <> -1 AndAlso CanvasMasterDBVersion >= "0.99" Then
                Dim PinPrefix As String = ""
                Select Case PinType
                    Case ecPinCodeType.mfaEmail
                        PinPrefix = _OPT_PREFIX_MFA_EMAIL
                    Case Else
                        PinPrefix = _OPT_PREFIX_PIN
                End Select

                Dim SQL As String = String.Format("DELETE FROM PrivateURLS WHERE Hash LIKE '" + PinPrefix + "%' AND UserID = ?")
                Dim tParams As New List(Of Object)
                tParams.Add(UserID)
                If Database.ExecuteSQL(SQL, tParams) > 0 Then tRes = True
            End If
            Return tRes
        End Function

        Public Function GetUserPinTimeout(tCode As Tuple(Of Integer, Integer)) As Integer
            Return If(tCode Is Nothing, _DEF_MFA_EMAIL_TIMEOUT, tCode.Item2)

            ' timeout for resend:
            'Dim CodeTimeout As Integer = _MFA_CODE_RESEND_TIMEOUT
            'If tCode IsNot Nothing Then CodeTimeout = _MFA_CODE_RESEND_TIMEOUT - _DEF_MFA_EMAIL_TIMEOUT + tCode.Item2
            'If CodeTimeout < 1 Then CodeTimeout = 0
            'If CodeTimeout > _MFA_CODE_RESEND_TIMEOUT Then CodeTimeout = _MFA_CODE_RESEND_TIMEOUT
            'Return CodeTimeout
        End Function
        ' D7504 ==

        Public Function GetUserByPin(PinType As ecPinCodeType, Pin As Integer, ByRef tUser As clsApplicationUser, ByRef tProject As clsProject, ByRef sExtraParams As String) As Boolean  ' D7501 + D7502
            Dim fHasUser As Boolean = False

            ' D7502 ===
            Dim PinPrefix As String = ""
            Dim PinTimeout As Integer = 60
            Select Case PinType
                Case ecPinCodeType.mfaEmail
                    PinPrefix = _OPT_PREFIX_MFA_EMAIL
                    PinTimeout = _DEF_MFA_EMAIL_TIMEOUT
                Case Else
                    PinPrefix = _OPT_PREFIX_PIN
                    PinTimeout = _DEF_PINCODE_TIMEOUT
            End Select
            ' D7502 ==

            Dim lastDT As DateTime = Now.AddSeconds(-PinTimeout)
            Dim SQL As String = "SELECT TOP(1) * FROM PrivateURLS WHERE Hash = ? AND Created > ? ORDER BY Created DESC"
            Dim tParams As New List(Of Object)
            tParams.Add(String.Format("{0}{1}", PinPrefix, Pin))    ' D7501
            tParams.Add(lastDT)
            Dim Data As List(Of Dictionary(Of String, Object)) = Database.SelectBySQL(SQL, tParams)
            If Data.Count > 0 Then
                Dim tRow = Data(0)
                If Not IsDBNull(tRow("UserID")) Then
                    tUser = DBUserByID(CInt(tRow("UserID")))
                    'If tUser IsNot Nothing AndAlso tUser.CannotBeDeleted Then tUser = Nothing
                End If
                If Not IsDBNull(tRow("ProjectID")) Then
                    tProject = DBProjectByID(CInt(tRow("ProjectID")))
                End If
                If Not IsDBNull(tRow("URL")) Then
                    sExtraParams = CStr(tRow("URL"))
                End If
            End If
            If tUser IsNot Nothing Then fHasUser = True

            Return fHasUser
        End Function
        ' D7187 ==

#End Region


        ' -D4538 ===
        '#Region "NewRelic"

        '        Public Const CustomEventTableName = "CustomEvents"
        '        Public CustomEventsListJS() As String = {"ActiveProjectName", "ActiveUserEMail", "ActiveUserName", _
        '                                                 "ActiveWorkgroupName", "SessionID", "HostAddress", _
        '                                                 "LastVisitedPage", "Site"}  ' D3208

        '        Public Function RecordCustomEvent(ByVal tActionType As dbActionType, ByVal tObjectType As dbObjectType, ByVal tObjectID As Integer, ByVal sComment As String, ByVal sResult As String, Optional ByVal UserID As Integer = -1, Optional ByVal WorkgroupID As Integer = -1) As String ' D3205
        '            Dim sData As String = ""    ' D3205
        '            If ActiveUser IsNot Nothing AndAlso WebConfigOption("NewRelic.agentEnabled", "false").ToLower = "true" Then ' D3264
        '                Dim tParams As New Dictionary(Of String, Object)
        '                If HasActiveProject() AndAlso ActiveProject IsNot Nothing Then
        '                    tParams.Add("ActiveProjectName", ActiveProject.ProjectName)
        '                End If
        '                tParams.Add("ActiveUserEMail", ActiveUser.UserEmail)
        '                tParams.Add("ActiveUserName", ActiveUser.UserName)

        '                If ActiveWorkgroup IsNot Nothing Then
        '                    tParams.Add("ActiveWorkgroupName", ActiveWorkgroup.Name)
        '                End If

        '                ' D3183 ===
        '                tParams.Add("UID", Options.UID)
        '                'tParams.Add("isNewUID", Options.isNewUID.ToString)
        '                tParams.Add("SessionID", Options.SessionID)
        '                tParams.Add("UserAgent", Options.UserAgent)
        '                tParams.Add("IsMobileBrowser", isMobileBrowser.ToString)
        '                tParams.Add("HostAddress", Options.HostAddress)
        '                tParams.Add("LastVisitedPage", Options.LastVisitedPageName) ' D3186
        '                tParams.Add("LastVisitedPageGroup", Options.LastVisitedPageGroup)  ' D3186
        '                ' D3183 ==

        '                Dim AppName As String = WebConfigOption("NewRelic.AppName", "")     ' D3212
        '                If AppName <> "" Then tParams.Add("Site", AppName) ' D3212

        '                Dim AObjectType As String = tObjectType.ToString
        '                Dim AActionType As String = tActionType.ToString
        '                tParams.Add("ActionType", AActionType)
        '                tParams.Add("ActionObjectType", AObjectType)

        '                If tParams.Count > 0 AndAlso tParams.ContainsKey("ActionType") Then
        '                    NewRelic.Api.Agent.NewRelic.RecordCustomEvent(CustomEventTableName, tParams)
        '                End If

        '                ' D3205 ===
        '                For Each sKey As String In tParams.Keys
        '                    If Array.IndexOf(CustomEventsListJS, sKey) >= 0 Then sData += String.Format("{2}['{0}', '{1}']", JS_SafeString(sKey), JS_SafeString(tParams(sKey)), IIf(sData = "", "", ","))
        '                Next
        '            End If
        '            Return sData
        '            ' D3205 ==
        '        End Function

        '#End Region
        ' -D4538 ==


#Region "Snapshots"

        ' D3509 ===
        Public ReadOnly Property LastSnapshot() As clsSnapshot
            Get
                If _LastSnapshot IsNot Nothing AndAlso _LastSnapshotChecked.AddMilliseconds(_OPT_SNAPSHOTS_CHECKLAST_MSEC) < Now Then _LastSnapshot = Nothing ' D3757
                If ProjectID > 0 AndAlso (_LastSnapshot Is Nothing OrElse _LastSnapshot.ProjectID <> ProjectID) Then  ' D3511
                    _LastSnapshot = DBSnapshotReadLatest(ProjectID, True)
                    _LastSnapshotChecked = Now  ' D3757
                End If
                Return _LastSnapshot
            End Get
        End Property

        ' D3847 ===
        Private Function SnapshotWorkspacesListPack(WSList As List(Of clsWorkspace)) As String
            DebugInfo("Create workspaces for snapshot...")
            Dim sList As String = ""
            If WSList IsNot Nothing Then
                For Each tWS As clsWorkspace In WSList
                    ' no need to save ProjectID since that param is already written with snapshot
                    ' no save for ID, Created, LastModified since it auto-updated on each save to DB
                    sList += String.Format("{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}{0}{8}{0}{9}" + vbCr,
                                           _SNAPSHOT_WORKSPACE_DELIM, tWS.UserID, tWS.GroupID,
                                           CInt(IIf(_OPT_SNAPSHOTS_SAVE_LAST_STEPS, tWS.ProjectStepLikelihood, -1)),
                                           CInt(IIf(_OPT_SNAPSHOTS_SAVE_LAST_STEPS, tWS.ProjectStepImpact, -1)),
                                           CInt(tWS.StatusLikelihood), CInt(tWS.StatusImpact),
                                           CInt(tWS.TeamTimeStatusLikelihood), CInt(tWS.TeamTimeStatusImpact), tWS.Comment)    ' D3511 + D3512 + D3726
                Next
            End If
            DebugInfo("Workspaces snapshot created.")
            Return sList
        End Function
        ' D3847 ==

        Private Function SnapshotWorkspacesPack(tProjectID As Integer) As String
            Return SnapshotWorkspacesListPack(DBWorkspacesByProjectID(tProjectID))  ' D3849
        End Function

        ' D3847 ===
        Private Function SnapshotWorkspacesListUnpack(tProjectID As Integer, sWorkspacesSnapshot As String) As List(Of clsWorkspace)
            DebugInfo("Parse workspaces from snapshot...")
            Dim WSList As New List(Of clsWorkspace)
            If Not String.IsNullOrEmpty(sWorkspacesSnapshot) Then
                Dim sLines As String() = sWorkspacesSnapshot.Split(CChar(vbCr))
                For Each sLine As String In sLines
                    Dim tItems As String() = sLine.Split(_SNAPSHOT_WORKSPACE_DELIM)
                    If tItems.Length = 9 Then   ' D3512 // remove Created, replace ID with UserID
                        Dim UID As Integer
                        If Integer.TryParse(tItems(0), UID) Then
                            Dim tWS As New clsWorkspace()
                            tWS.UserID = UID
                            tWS.ProjectID = tProjectID

                            '0 tWS.UserID, 1 tWS.GroupID, 2 tWS.ProjectStepLikelihood, 3 tWS.ProjectStepImpact, 
                            '4 CInt(tWS.StatusLikelihood), 5 CInt(tWS.StatusImpact), 6 CInt(tWS.TeamTimeStatusLikelihood), 7 CInt(tWS.TeamTimeStatusImpact), 8 tWS.Comment)

                            Dim GrpID As Integer = CInt(tItems(1))
                            Dim StepL As Integer = CInt(tItems(2))
                            Dim StepI As Integer = CInt(tItems(3))
                            Dim StatusL As ecWorkspaceStatus = CType(CInt(tItems(4)), ecWorkspaceStatus)
                            Dim StatusI As ecWorkspaceStatus = CType(CInt(tItems(5)), ecWorkspaceStatus)
                            Dim TTL As ecWorkspaceStatus = CType(CInt(tItems(6)), ecWorkspaceStatus)
                            Dim TTI As ecWorkspaceStatus = CType(CInt(tItems(7)), ecWorkspaceStatus)
                            'Dim DTC As Nullable(Of DateTime) = BinaryStr2DateTime(tItems(8))   ' -D3512
                            Dim sComment As String = tItems(8).Trim

                            tWS.GroupID = GrpID
                            tWS.ProjectStepLikelihood = StepL
                            tWS.ProjectStepImpact = StepI
                            tWS.StatusLikelihood = StatusL
                            tWS.StatusImpact = StatusI
                            tWS.TeamTimeStatusLikelihood = TTL
                            tWS.TeamTimeStatusImpact = TTI
                            'tWS.Created = DTC  ' -D3512
                            tWS.Comment = sComment

                            WSList.Add(tWS)
                        End If
                    End If
                Next

            End If
            Return WSList
        End Function

        ' D3511 ===
        Private Function SnapshotWorkspacesUnpack(tProjectID As Integer, sWorkspacesSnapshot As String, fNeedCheck As Boolean, ByRef sError As String) As Boolean   ' D3893
            DebugInfo("Restore workspaces from snapshot...")
            Dim fResult As Boolean = False
            Dim WSList As List(Of clsWorkspace) = DBWorkspacesByProjectID(tProjectID)
            If WSList IsNot Nothing AndAlso Not String.IsNullOrEmpty(sWorkspacesSnapshot) Then
                Dim fHasChanges As Boolean = False
                ' D3847 ===
                Dim tNewList As List(Of clsWorkspace) = SnapshotWorkspacesListUnpack(tProjectID, sWorkspacesSnapshot)

                ' D3893 ===
                If sError = "" AndAlso fNeedCheck AndAlso ActiveWorkgroup.License IsNot Nothing AndAlso ActiveWorkgroup.License.isValidLicense Then
                    Dim tLimit As Long = ActiveWorkgroup.License.GetParameterMaxByID(ecLicenseParameter.MaxUsersInProject)
                    If tLimit > 0 AndAlso tLimit <> UNLIMITED_VALUE Then
                        If tNewList.Count > tLimit Then sError = CInt(ecLicenseParameter.MaxUsersInProject).ToString
                    End If
                    If sError = "" Then
                        tLimit = ActiveWorkgroup.License.GetParameterMaxByID(ecLicenseParameter.MaxPMsInProject)
                        Dim tLimitEvals As Long = ActiveWorkgroup.License.GetParameterMaxByID(ecLicenseParameter.MaxEvaluatorsInModel)
                        If (tLimit > 0 AndAlso tLimit <> UNLIMITED_VALUE) OrElse (tLimitEvals > 0 AndAlso tLimitEvals <> UNLIMITED_VALUE) Then
                            Dim PMGroups As New List(Of Integer)
                            Dim EvalGroups As New List(Of Integer)
                            For Each tGrp As clsRoleGroup In ActiveWorkgroup.RoleGroups
                                If tGrp.ActionStatus(ecActionType.at_mlModifyModelHierarchy) = ecActionStatus.asGranted OrElse tGrp.ActionStatus(ecActionType.at_mlModifyAlternativeHierarchy) = ecActionStatus.asGranted OrElse tGrp.ActionStatus(ecActionType.at_mlManageProjectOptions) = ecActionStatus.asGranted Then PMGroups.Add(tGrp.ID)
                                If tGrp.ActionStatus(ecActionType.at_mlEvaluateModel) = ecActionStatus.asGranted Then EvalGroups.Add(tGrp.ID)
                            Next
                            Dim PMCnt As Integer = 0
                            Dim EvalCnt As Integer = 0
                            For Each tWS As clsWorkspace In tNewList
                                If PMGroups.Contains(tWS.GroupID) Then PMCnt += 1
                                If EvalGroups.Contains(tWS.GroupID) Then EvalCnt += 1
                            Next
                            If tLimit > 0 AndAlso tLimit <> UNLIMITED_VALUE AndAlso PMCnt > tLimit Then sError = CInt(ecLicenseParameter.MaxPMsInProject).ToString
                            ' skip check evaluators cnt since lic param is not supported yet
                            'If sError = "" AndAlso tLimitEvals > 0 AndAlso tLimitEvals <> UNLIMITED_VALUE AndAlso EvalCnt > tLimitEvals Then sError = CInt(ecLicenseParameter.MaxEvaluatorsInModel).ToString
                        End If
                    End If

                    If sError <> "" Then sError = LicenseErrorMessage(ActiveWorkgroup.License, CType(sError, ecLicenseParameter), False)
                End If

                If tNewList IsNot Nothing AndAlso tNewList.Count > 0 AndAlso (sError Is Nothing OrElse sError = "") Then
                    ' D4156 ===
                    ' Check for 'added' WS: need to delete new workspaces, which is missing in the snapshot
                    For i = WSList.Count - 1 To 0 Step -1
                        Dim tCurWS As clsWorkspace = WSList(i)
                        Dim tOldWS As clsWorkspace = clsWorkspace.WorkspaceByUserIDAndProjectID(tCurWS.UserID, tProjectID, tNewList)
                        If tOldWS Is Nothing AndAlso ActiveUser IsNot Nothing AndAlso tCurWS.UserID <> ActiveUser.UserID Then
                            If DBWorkspaceDelete(tCurWS.ID, "Delete workspace on restore from snapshot") Then WSList.Remove(tCurWS)
                        End If
                    Next
                    ' D4156 ==

                    ' D3893 ==
                    For Each tNewWS As clsWorkspace In tNewList
                        Dim tWS As clsWorkspace = clsWorkspace.WorkspaceByUserIDAndProjectID(tNewWS.UserID, tProjectID, WSList)   ' D3512
                        Dim fIsNew As Boolean = False
                        If tWS Is Nothing Then
                            tWS = tNewWS
                            fIsNew = True
                        End If

                        If fIsNew OrElse tWS.GroupID <> tNewWS.GroupID OrElse
                           (_OPT_SNAPSHOTS_SAVE_LAST_STEPS AndAlso (tWS.ProjectStepLikelihood <> tNewWS.ProjectStepLikelihood OrElse tWS.ProjectStepImpact <> tNewWS.ProjectStepImpact)) OrElse
                           tWS.StatusLikelihood <> tNewWS.StatusLikelihood OrElse tWS.StatusImpact <> tNewWS.StatusImpact OrElse
                           tWS.TeamTimeStatusLikelihood <> tNewWS.TeamTimeStatusLikelihood OrElse tWS.TeamTimeStatusImpact <> tNewWS.TeamTimeStatusImpact OrElse
                           tWS.Comment <> tNewWS.Comment Then ' D3726
                            'Date2ULong(tWS.Created).ToString <> Date2ULong(DTC).ToString OrElse tWS.Comment <> sComment Then
                            fHasChanges = True
                            If Not fIsNew Then
                                tWS.GroupID = tNewWS.GroupID
                                tWS.ProjectStepLikelihood = tNewWS.ProjectStepLikelihood
                                tWS.ProjectStepImpact = tNewWS.ProjectStepImpact
                                tWS.StatusLikelihood = tNewWS.StatusLikelihood
                                tWS.StatusImpact = tNewWS.StatusImpact
                                tWS.TeamTimeStatusLikelihood = tNewWS.TeamTimeStatusLikelihood
                                tWS.TeamTimeStatusImpact = tNewWS.TeamTimeStatusImpact
                                tWS.Comment = tNewWS.Comment
                            End If
                            DBWorkspaceUpdate(tWS, fIsNew, "Restore workspace from snapshot ")
                        End If
                        ' D3847 ==
                    Next
                    fResult = True  ' D3576

                End If
            End If
            DebugInfo("Workspaces restored from snapshot.")
            Return fResult
        End Function

        ' D3511 ==
        Private Function SnapshotStreamsPack(tProject As clsProject) As MemoryStream ' D3510
            DebugInfo("Create streams for snapshot...")
            Dim tStream As New MemoryStream

            ' D3510 ===
            If tProject IsNot Nothing Then
                Dim PM As New clsProjectManager(True, tProject.IsRisk)
                PM.StorageManager.ProjectLocation = tProject.ConnectionString
                PM.StorageManager.ProviderType = tProject.ProviderType
                PM.StorageManager.ModelID = tProject.ID
                Try
                    PM.StorageManager.Reader.LoadFullProjectStream(tStream)
                    DebugInfo("Streams snapshot composed, bytes: " + tStream.Length.ToString)
                Catch
                End Try
                PM = Nothing

                If _OPT_SNAPSHOTS_ARCHIVE AndAlso tStream IsNot Nothing AndAlso tStream.Length >= _OPT_SNAPSHOTS_ARCHIVE_MIN_SIZE Then
                    DebugInfo("Start packing streams snapshot...")
                    ' D3512 ===
                    Dim tPackedStream As MemoryStream = StreamCompress(tStream)
                    ' for debug only
                    'Dim FS As New FileStream("C:\debug_out.gz", FileMode.Create)
                    'tStream.Seek(0, SeekOrigin.Begin)
                    'tStream.CopyTo(FS)
                    'FS.Close()
                    If tPackedStream.Length > 0 AndAlso tPackedStream.Length < tStream.Length Then
                        tStream = New MemoryStream
                        tPackedStream.Seek(0, SeekOrigin.Begin)
                        tPackedStream.CopyTo(tStream)
                        DebugInfo("Packed streams snapshot, bytes: " + tStream.Length.ToString)
                    End If
                    ' D3512 ==
                End If
                If tStream IsNot Nothing Then tStream.Seek(0, SeekOrigin.Begin)
            End If
            ' D3510 ==

            DebugInfo("Streams snapshot created")
            Return tStream
        End Function

        ' D3511 ===
        Private Function SnapshotStreamsUnPack(ByRef tProject As clsProject, tStream As MemoryStream, fCheckProject As Boolean, ByRef sError As String) As Boolean   ' D3893
            DebugInfo("Restore streams for snapshot...")
            Dim fResult As Boolean = False

            ' D3510 ===
            If tProject IsNot Nothing AndAlso tStream IsNot Nothing AndAlso tStream.Length > 0 Then

                tStream.Seek(0, SeekOrigin.Begin)
                Dim isAHPS = tStream.ReadByte() = 241 AndAlso tStream.ReadByte = 173 AndAlso tStream.ReadByte = 118

                If Not isAHPS Then
                    DebugInfo("Start unpacking streams snapshot...")
                    tStream = StreamDecompress(tStream) ' D3512
                    ' for debug only
                    'Dim FS As New FileStream("C:\debug_out.ahps", FileMode.Create)
                    'tStream.Seek(0, SeekOrigin.Begin)
                    'tStream.CopyTo(FS)
                    'FS.Close()
                    DebugInfo("Unpacked streams snapshot, bytes: " + tStream.Length.ToString)
                End If

                If tStream IsNot Nothing Then tStream.Seek(0, SeekOrigin.Begin)

                DebugInfo("Start loading project from snapshot stream...")
                Dim PM As New clsProjectManager(True, tProject.IsRisk)
                PM.StorageManager.ProjectLocation = tProject.ConnectionString
                PM.StorageManager.ProviderType = tProject.ProviderType
                PM.StorageManager.ModelID = -tProject.ID
                PM.StorageManager.StorageType = ECModelStorageType.mstAHPSStream
                PM.StorageManager.ReadDBVersion()
                Try
                    fResult = PM.StorageManager.Writer.SaveFullProjectStream(tStream)
                    DebugInfo("Project loaded from snapshot: " + fResult.ToString)

                    ' D3893 ===
                    If fResult AndAlso fCheckProject AndAlso sError = "" AndAlso ActiveWorkgroup.License IsNot Nothing AndAlso ActiveWorkgroup.License.isValidLicense Then
                        ' D3909 ===
                        'PM.CloseProject()
                        PM = New clsProjectManager(True, False, tProject.IsRisk)
                        PM.StorageManager.ProjectLocation = tProject.ConnectionString
                        PM.StorageManager.ProviderType = tProject.ProviderType
                        PM.StorageManager.ModelID = -tProject.ID
                        PM.StorageManager.StorageType = clsProject.StorageType
                        PM.StorageManager.ReadDBVersion()
                        If PM.LoadProject(tProject.ConnectionString, tProject.ProviderType, clsProject.StorageType, -tProject.ID) Then
                            ' D3909 ==s
                            Dim tLimit As Long = ActiveWorkgroup.License.GetParameterMaxByID(ecLicenseParameter.MaxAlternatives)
                            If tLimit > 0 AndAlso tLimit <> UNLIMITED_VALUE Then
                                For Each tH As clsHierarchy In PM.AltsHierarchies
                                    If tH.Nodes.Count > tLimit Then
                                        sError = CInt(ecLicenseParameter.MaxAlternatives).ToString
                                        Exit For
                                    End If
                                Next
                            End If
                            If sError = "" Then
                                tLimit = ActiveWorkgroup.License.GetParameterMaxByID(ecLicenseParameter.MaxObjectives)
                                Dim tLimitLevels = ActiveWorkgroup.License.GetParameterMaxByID(ecLicenseParameter.MaxLevelsBelowGoal)
                                If (tLimit > 0 AndAlso tLimit <> UNLIMITED_VALUE) OrElse (tLimitLevels > 0 AndAlso tLimitLevels <> UNLIMITED_VALUE) Then
                                    For Each tH As clsHierarchy In PM.Hierarchies
                                        If tLimit > 0 AndAlso tH.Nodes.Count > tLimit Then
                                            sError = CInt(ecLicenseParameter.MaxObjectives).ToString
                                            Exit For
                                        End If
                                        If tLimitLevels > 0 AndAlso tH.GetMaxLevel > tLimitLevels Then
                                            sError = CInt(ecLicenseParameter.MaxLevelsBelowGoal).ToString
                                            Exit For
                                        End If
                                    Next
                                End If
                            End If
                            If sError <> "" Then
                                sError = LicenseErrorMessage(ActiveWorkgroup.License, CType(sError, ecLicenseParameter), False)
                                Throw New Exception(sError)
                            End If
                        End If
                    End If
                    ' D3893 ==

                Catch ex As Exception
                    fResult = False     ' D3893
                    If sError IsNot Nothing AndAlso sError = "" Then sError = ResString("errSnapshotReadStream") ' D3893
                    DebugInfo("Error on load project from snapshot: " + ex.Message)
                    'Wipeout part of streams, which could be saved with -ProjectID
                    Dim sSQL As String = String.Format("DELETE FROM ModelStructure WHERE ProjectID=-{0}" + vbCrLf +
                                                       "DELETE FROM UserData WHERE ProjectID=-{0}", tProject.ID)
                    Database.ExecuteSQL(sSQL)
                    DBSaveLog(dbActionType.actShowRTE, dbObjectType.einfProject, tProject.ID, "Load project stream from snaphot", ex.Message)
                End Try

                If fResult Then
                    DebugInfo("Remove existing streams and replace with restored")
                    Dim sSQL As String = String.Format("DELETE FROM ModelStructure WHERE ProjectID={0}" + vbCrLf +
                                                       "DELETE FROM UserData WHERE ProjectID={0}" + vbCrLf +
                                                       "UPDATE ModelStructure SET ProjectID={0} WHERE ProjectID=-{0}" + vbCrLf +
                                                       "UPDATE UserData SET ProjectID={0} WHERE ProjectID=-{0}", tProject.ID)
                    Dim affected As Integer = Database.ExecuteSQL(sSQL)
                    tProject.ResetProject()
                    DebugInfo("Remove existing streams and replace with restored")
                End If
                PM = Nothing
            End If
            ' D3510 ==

            DebugInfo("Streams snapshot created")
            Return fResult
        End Function
        ' D3511 ==

        Private Function SnapshotCreate(tProject As clsProject, tType As ecSnapShotType, sComment As String) As clsSnapshot  ' D3510
            Dim tSnapshot As clsSnapshot = Nothing
            If tProject IsNot Nothing AndAlso (tProject.isValidDBVersion OrElse tProject.isDBVersionCanBeUpdated) Then  ' D3510
                tSnapshot = New clsSnapshot
                tSnapshot.DateTime = Now
                tSnapshot.ProjectID = tProject.ID   ' D3510
                tSnapshot.Type = tType
                tSnapshot.Comment = sComment
                tSnapshot.ProjectStream = SnapshotStreamsPack(tProject) ' D3510
                If tSnapshot.ProjectStream IsNot Nothing Then
                    'Dim tStreamContent() As Byte = {}
                    'Dim StreamLength As Integer = CInt(tSnapshot.ProjectStream.Length)
                    'Array.Resize(tStreamContent, StreamLength)
                    'tSnapshot.ProjectStream.Seek(0, SeekOrigin.Begin)
                    'tSnapshot.ProjectStream.Read(tStreamContent, 0, StreamLength)
                    'tSnapshot.ProjectStreamMD5 = GetMD5(tStreamContent)
                    tSnapshot.ProjectStreamMD5 = GetMD5(tSnapshot.ProjectStream.ToArray)    ' D3512
                End If
                tSnapshot.ProjectWorkspace = SnapshotWorkspacesPack(tProject.ID)    ' D3510
                tSnapshot.ProjectWorkspaceMD5 = GetMD5(tSnapshot.ProjectWorkspace)
            Else
                DebugInfo("Invalid project for create snapshot")    ' D3510
            End If
            Return tSnapshot
        End Function
        ' D3509 ==

        ' D3511 ===
        Public Function SnapshotSaveProject(Optional fType As ecSnapShotType = ecSnapShotType.Auto, Optional sComment As String = "", Optional tProjectID As Integer = -1, Optional fSaveLogs As Boolean = True, Optional sUserComment As String = "", Optional tRestoredFrom As Integer = 0) As clsSnapshot   ' D3511 + D3573 + D3729
            Dim tSnapshot As clsSnapshot = Nothing
            Dim tPrj As clsProject = Nothing
            If tProjectID = -1 AndAlso HasActiveProject() Then tPrj = ActiveProject
            If tPrj Is Nothing Then tPrj = DBProjectByID(tProjectID)
            If tPrj IsNot Nothing AndAlso isAuthorized Then ' D4037
                Dim tWS As clsWorkspace = Nothing
                If tPrj.ID = ProjectID Then tWS = ActiveWorkspace Else tWS = DBWorkspaceByUserIDProjectID(ActiveUser.UserID, tPrj.ID)
                If tWS IsNot Nothing AndAlso CanUserModifyProject(ActiveUser.UserID, tPrj.ID, ActiveUserWorkgroup, tWS) Then    ' D4037
                    ' D6311 ===
                    Dim tParams As New Generic.Dictionary(Of String, String)
                    tParams.Add("%%project%%", ResString("templ_Project"))
                    tParams.Add("%%projects%%", ResString("templ_Projects"))
                    tParams.Add("%%model%%", ResString("templ_Model"))
                    tParams.Add("%%models%%", ResString("templ_Models"))
                    sComment = ParseStringTemplates(sComment, tParams)
                    sUserComment = ParseStringTemplates(sUserComment, tParams)
                    ' D6311 ==
                    sComment = ShortString(sComment.Replace(vbCr, " ").Replace(vbLf, "").Replace("  ", " ").Trim, 100, True)            ' D4434
                    If _OPT_SNAPSHOTS_SAVE_USEREMAIL AndAlso ActiveUser IsNot Nothing Then sUserComment += If(sUserComment = "", "", " / ") + ActiveUser.UserEmail    ' D7505
                    sUserComment = ShortString(sUserComment.Replace(vbCr, " ").Replace(vbLf, "").Replace("  ", " ").Trim, 100, True)    ' D4434
                    tSnapshot = SnapshotCreate(tPrj, fType, sComment)
                    If tSnapshot IsNot Nothing Then
                        tSnapshot.Details = sUserComment    ' D3729
                        tSnapshot.RestoredFrom = tRestoredFrom  ' D3729
                        If DBSnapshotWrite(tSnapshot) Then
                            If fSaveLogs Then ' D3573
                                DBSaveLog(dbActionType.actCreate, dbObjectType.einfSnapshot, tPrj.ID, "Create snapshot " + tSnapshot.SnapshotID, sComment)  ' D3576
                            End If
                        End If
                    End If
                    _LastSnapshot = tSnapshot
                End If
            End If
            Return tSnapshot
        End Function

        ''' <summary>
        ''' Restore active project from the specified snapshot
        ''' </summary>
        ''' <param name="tSnapshotID">Use -1 for restore to the latest snapshot</param>
        ''' <returns>True when successful</returns>
        ''' <remarks>Project manager will be reset</remarks>
        Public Function SnapshotRestoreProject(Optional tSnapshotID As Integer = -1, Optional tProjectID As Integer = -1, Optional ByRef sError As String = Nothing) As clsSnapshot   ' D3893
            Dim fResult As Boolean = False
            Dim tSnapshot As clsSnapshot = Nothing
            Dim tPrj As clsProject = Nothing
            If tProjectID = -1 AndAlso HasActiveProject() Then tPrj = ActiveProject
            If tPrj Is Nothing Then tPrj = clsProject.ProjectByID(tProjectID, ActiveProjectsList) ' D3726
            If tPrj Is Nothing Then tPrj = DBProjectByID(tProjectID)
            If (tPrj IsNot Nothing) Then

                Dim tOrigSnapshot As MemoryStream = SnapshotStreamsPack(tPrj)   ' D3893

                If tSnapshotID = -1 AndAlso tProjectID = -1 Then
                    tSnapshot = LastSnapshot
                Else
                    If tSnapshotID = -1 Then
                        If tProjectID > 0 Then tSnapshot = DBSnapshotReadLatest(tProjectID, True)
                    Else
                        tSnapshot = DBSnapshotRead(tSnapshotID, True)
                    End If
                End If

                If tSnapshot IsNot Nothing AndAlso tSnapshot.ProjectID = tPrj.ID Then
                    DBProjectLockInfoWrite(ECLockStatus.lsLockForSystem, tPrj.LockInfo, ActiveUser, Nothing)

                    ' D3726 ===
                    Dim fNeedRestoreStreams As Boolean = True
                    Using tCurStream As MemoryStream = SnapshotStreamsPack(tPrj)
                        If tCurStream IsNot Nothing Then
                            Dim tCurStreamMD5 = GetMD5(tCurStream.ToArray)
                            If tCurStreamMD5 = tSnapshot.ProjectStreamMD5 Then fNeedRestoreStreams = False
                        End If
                    End Using
                    ' Need to check project on restore from snapshot when created outside (earlier than project created)
                    Dim fNeedCheck As Boolean = True OrElse Not tPrj.Created.HasValue OrElse tSnapshot.DateTime < tPrj.Created.Value   ' D3893
                    If fNeedRestoreStreams Then fResult = SnapshotStreamsUnPack(tPrj, tSnapshot.ProjectStream, fNeedCheck, sError) ' D3893

                    Dim fNeedRestoreWorkspaces As Boolean = True
                    Dim tCurWS As String = SnapshotWorkspacesPack(tPrj.ID)
                    If tCurWS IsNot Nothing AndAlso tCurWS = tSnapshot.ProjectWorkspace Then fNeedRestoreWorkspaces = False
                    If fNeedRestoreWorkspaces AndAlso (fResult OrElse Not fNeedRestoreStreams) Then
                        ' D3893 ===
                        fResult = SnapshotWorkspacesUnpack(tPrj.ID, tSnapshot.ProjectWorkspace, fNeedCheck, sError)
                        If Not fResult AndAlso fNeedRestoreStreams AndAlso tOrigSnapshot IsNot Nothing Then
                            Dim tOrigStreamMD5 = GetMD5(tOrigSnapshot.ToArray)
                            If tOrigStreamMD5 <> tSnapshot.ProjectStreamMD5 Then SnapshotStreamsUnPack(tPrj, tOrigSnapshot, False, sError)
                        End If
                    End If
                    ' D3726 + D3893 ==

                    DBProjectLockInfoWrite(ECLockStatus.lsUnLocked, tPrj.LockInfo, ActiveUser, Now)
                    DBSaveLog(dbActionType.actRestore, dbObjectType.einfSnapshot, tPrj.ID, "Restore project from snapshot " + CStr(IIf(tSnapshot.Idx > 0, tSnapshot.Idx, tSnapshot.SnapshotID)), String.Format("{0} (Stream restoring: {1}; Workspaces restoring: {2})", IIf(fResult, "Success", "Error"), Bool2YesNo(fNeedRestoreStreams), Bool2YesNo(fNeedRestoreWorkspaces)))  ' D3576 + D3726 + D3731

                    If sError IsNot Nothing AndAlso sError = "" AndAlso Not fNeedRestoreStreams AndAlso Not fNeedRestoreWorkspaces Then sError = ResString("msgSnapshotNoChangesOnRestore") ' D3893

                End If
                If fResult AndAlso tPrj IsNot Nothing Then
                    tPrj.ResetProject() ' D3526
                    tPrj.DBVersionReset()   ' D4076
                    If ProjectID = tProjectID Then ActiveProject.DBVersionReset()   ' D4076
                End If
            End If
            _LastSnapshot = Nothing ' D3726
            If fResult AndAlso tSnapshot IsNot Nothing Then Return tSnapshot Else Return Nothing ' D3576
        End Function
        ' D3511 ==

        ' D3732 ===
        Public Function SnapshotsCheckMissingIdx(ByRef tLst As List(Of clsSnapshot)) As Integer
            Dim tMax As Integer = 0
            If tLst IsNot Nothing Then
                Dim fHasEmpty As Boolean = False
                For Each tSnap As clsSnapshot In tLst
                    If tSnap.Idx > tMax Then tMax = tSnap.Idx
                    If Not fHasEmpty AndAlso tSnap.Idx <= 0 Then fHasEmpty = True
                Next
                If fHasEmpty Then
                    If tMax < tLst.Count Then tMax = tLst.Count
                    Dim fNeedSave As Boolean = CanvasMasterDBVersion >= "0.99992"
                    Dim idx As Integer = tMax
                    For i = 0 To tLst.Count - 1 Step 1
                        tLst(i).Idx = idx
                        If fNeedSave Then DBSnapshotInfoUpdate(tLst(i))
                        idx -= 1
                    Next
                End If
            End If
            Return tMax
        End Function
        ' D3732 ==

        ' D3847 ===
        Public Function Snapshot2Stream(tSnapshot As clsSnapshot, tBW As BinaryWriter) As Boolean
            Dim fRes As Boolean = False
            If tBW IsNot Nothing AndAlso tSnapshot IsNot Nothing Then
                tSnapshot.ProjectStream.Seek(0, SeekOrigin.Begin)
                tBW.Write(tSnapshot.ID)
                tBW.Write(tSnapshot.Idx)
                tBW.Write(Date2ULong(tSnapshot.DateTime))
                tBW.Write(CInt(tSnapshot.Type))
                tBW.Write(tSnapshot.RestoredFrom)
                tBW.Write(tSnapshot.Comment)
                tBW.Write(tSnapshot.Details)
                tBW.Write(tSnapshot.ProjectStreamMD5)
                tBW.Write(tSnapshot.ProjectStreamSize)
                tBW.Write(tSnapshot.ProjectStream.ToArray())
                tBW.Write(tSnapshot.ProjectWorkspaceMD5)
                tBW.Write(tSnapshot.ProjectWorkspaceSize)
                tBW.Write(tSnapshot.ProjectWorkspace)
                fRes = True
            End If
            Return fRes
        End Function

        Public Function SnapshotsAll2Stream(tPrjId As Integer, ByRef tStream As Stream) As Boolean
            Dim fRes As Boolean = False
            Dim tLst As List(Of clsSnapshot) = DBSnapshotsReadAll(tPrjId, True)
            If tLst IsNot Nothing AndAlso tStream IsNot Nothing Then
                'tStream.Seek(0, SeekOrigin.Begin)  ' -D3890 since we can append to file
                Dim tBW As New BinaryWriter(tStream)

                tBW.Write(_SNAPSHOT_STREAM_HEADER.ToCharArray)
                tBW.Write(_SNAPSHOT_STREAM_VERSION)

                tBW.Write(DatabaseID)   ' D3851

                Dim tPrjUsers As List(Of clsApplicationUser) = DBUsersByProjectID(tPrjId)
                Dim tUsers As New List(Of clsApplicationUser)
                For Each tSnap As clsSnapshot In tLst
                    Dim tWSList As List(Of clsWorkspace) = SnapshotWorkspacesListUnpack(tPrjId, tSnap.ProjectWorkspace)
                    If tWSList IsNot Nothing Then
                        For Each tWS As clsWorkspace In tWSList
                            Dim tUser As clsApplicationUser = clsApplicationUser.UserByUserID(tWS.UserID, tUsers)
                            If tUser Is Nothing Then
                                tUser = clsApplicationUser.UserByUserID(tWS.UserID, tPrjUsers)
                                If tUser Is Nothing Then tUser = DBUserByID(tWS.UserID)
                                If tUser IsNot Nothing Then tUsers.Add(tUser)
                            End If
                        Next
                    End If
                Next

                ' D3851 ===
                tBW.Write(tUsers.Count)
                For Each tUser As clsApplicationUser In tUsers
                    tBW.Write(tUser.UserID)
                    tBW.Write(tUser.UserEmail)
                    tBW.Write(tUser.UserName)
                Next

                Dim tGroups As New List(Of clsRoleGroup)
                For Each tGroup As clsRoleGroup In ActiveWorkgroup.RoleGroups
                    If tGroup.RoleLevel = ecRoleLevel.rlModelLevel Then tGroups.Add(tGroup)
                Next

                tBW.Write(tGroups.Count)
                For Each tGroup As clsRoleGroup In tGroups
                    tBW.Write(tGroup.ID)
                    tBW.Write(CInt(tGroup.GroupType))
                Next
                ' D3851 ==

                tBW.Write(tLst.Count)
                For i As Integer = tLst.Count - 1 To 0 Step -1  ' D3882
                    Snapshot2Stream(tLst(i), tBW)   ' D3882
                Next

                'tBW.Close()    ' -D3890
                fRes = True
            End If
            Return fRes
        End Function
        ' D3847 ==

        ' D3851 ===
        Public Function SnapshotFromStream(tBR As BinaryReader, StreamVersion As Integer) As clsSnapshot
            Dim tSnapshot As clsSnapshot = Nothing
            If tBR IsNot Nothing Then
                tSnapshot = New clsSnapshot()
                tSnapshot.ID = tBR.ReadInt32
                tSnapshot.Idx = tBR.ReadInt32
                tSnapshot.DateTime = DateTime.FromBinary(tBR.ReadInt64)
                tSnapshot.Type = CType(tBR.ReadInt32, ecSnapShotType)
                tSnapshot.RestoredFrom = tBR.ReadInt32
                tSnapshot.Comment = tBR.ReadString
                tSnapshot.Details = tBR.ReadString
                tSnapshot.ProjectStreamMD5 = tBR.ReadString
                tSnapshot.ProjectStreamSize = tBR.ReadInt32
                tSnapshot.ProjectStream = New MemoryStream(tSnapshot.ProjectStreamSize)
                tSnapshot.ProjectStream.Write(tBR.ReadBytes(tSnapshot.ProjectStreamSize), 0, tSnapshot.ProjectStreamSize)
                tSnapshot.ProjectWorkspaceMD5 = tBR.ReadString
                tSnapshot.ProjectWorkspaceSize = tBR.ReadInt32
                tSnapshot.ProjectWorkspace = tBR.ReadString
                tSnapshot.ProjectStream.Seek(0, SeekOrigin.Begin)
                If GetMD5(tSnapshot.ProjectStream.ToArray) <> tSnapshot.ProjectStreamMD5 OrElse GetMD5(tSnapshot.ProjectWorkspace) <> tSnapshot.ProjectWorkspaceMD5 Then
                    tSnapshot = Nothing
                End If
            End If
            Return tSnapshot
        End Function

        Public Function SnapshotsAllFromStream(tProject As clsProject, ByRef tStream As Stream) As Integer  ' D3892
            Dim fRes As Integer = -1    ' D3892
            If tStream IsNot Nothing AndAlso tStream.Length > _SNAPSHOT_STREAM_HEADER.Length + 1 Then
                'tStream.Seek(0, SeekOrigin.Begin)  ' -D3890
                Dim tBR As New BinaryReader(tStream)

                Dim sHeader As String = System.Text.Encoding.UTF8.GetString(tBR.ReadBytes(_SNAPSHOT_STREAM_HEADER.Length))
                If sHeader = _SNAPSHOT_STREAM_HEADER Then
                    Dim Version As Integer = tBR.ReadInt16
                    If Version <= _SNAPSHOT_STREAM_VERSION Then

                        Dim sInstanceID As String = tBR.ReadString

                        Dim tDefGrpID As Integer = ActiveWorkgroup.GetDefaultRoleGroupID(ecRoleLevel.rlModelLevel, ecRoleGroupType.gtEvaluator)

                        Dim tPrjUsers As List(Of clsApplicationUser) = DBUsersByProjectID(tProject.ID)
                        Dim tUsersLinks As New Dictionary(Of Integer, Integer)

                        Dim SnapOption As Boolean = _OPT_SNAPSHOTS_ENABLE   ' D3907 // for avoid creating snapshots on model upload/restore
                        _OPT_SNAPSHOTS_ENABLE = False   ' D3907

                        Dim UsersCount As Integer = tBR.ReadInt32
                        For i = 1 To UsersCount
                            Dim tUserID As Integer = tBR.ReadInt32
                            Dim sUserEmail As String = tBR.ReadString
                            Dim sUserName As String = tBR.ReadString

                            If tUserID > 0 AndAlso sUserEmail.Trim <> "" Then
                                Dim tUser As clsApplicationUser = clsApplicationUser.UserByUserEmail(sUserEmail, tPrjUsers)
                                Dim fNewUser As Boolean = True
                                If tUser Is Nothing Then tUser = DBUserByEmail(sUserEmail) Else fNewUser = False
                                If tUser Is Nothing AndAlso _SNAPSHOT_CREATE_NEW_USERS_ONRESTORE Then
                                    If ActiveWorkgroup.License.CheckParameterByID(ecLicenseParameter.MaxUsersInWorkgroup, , False) AndAlso ActiveWorkgroup.License.CheckParameterByID(ecLicenseParameter.MaxUsersInProject, , False) Then
                                        tUser = UserWithSignup(sUserEmail, sUserName, , "Created on unpack Snapshots stream")
                                    End If
                                End If
                                If tUser IsNot Nothing Then
                                    Dim fCanAdd As Boolean = Not fNewUser
                                    If fNewUser Then
                                        Dim tUW As clsUserWorkgroup = DBUserWorkgroupByUserIDWorkgroupID(tUser.UserID, ActiveWorkgroup.ID)
                                        If tUW Is Nothing AndAlso ActiveWorkgroup.License.CheckParameterByID(ecLicenseParameter.MaxUsersInWorkgroup, , False) Then
                                            tUW = AttachWorkgroupByProject(tUser.UserID, tProject, ecRoleGroupType.gtUser, True)
                                        End If
                                        If tUW IsNot Nothing Then fCanAdd = True ' D3907

                                        ' -D3907: Disable due to problem with attach users directly to the model even this is not required
                                        'If tUW IsNot Nothing Then
                                        '    If AttachProject(tUser, tProject, False, tDefGrpID) IsNot Nothing Then fCanAdd = True
                                        'End If

                                    End If
                                    If fCanAdd Then tUsersLinks.Add(tUserID, tUser.UserID)
                                End If
                            End If
                        Next

                        _OPT_SNAPSHOTS_ENABLE = SnapOption  ' D3907

                        Dim tGroupsLinks As New Dictionary(Of Integer, Integer)

                        Dim GroupsCount As Integer = tBR.ReadInt32
                        For i = 1 To GroupsCount
                            Dim tSnapGrpIdx As Integer = tBR.ReadInt32
                            Dim tSnapGrpType As ecRoleGroupType = CType(tBR.ReadInt32, ecRoleGroupType)
                            Dim tGrpIdx As Integer = ActiveWorkgroup.RoleGroupID(tSnapGrpType)
                            If tGrpIdx > 0 Then tGroupsLinks.Add(tSnapGrpIdx, tGrpIdx)
                        Next

                        Dim tProjectWS As List(Of clsWorkspace) = DBWorkspacesByProjectID(tProject.ID)

                        fRes = 0 ' D3882 + D3892
                        Dim Cnt As Integer = tBR.ReadInt32
                        For i = 1 To Cnt Step 1
                            Dim tSnapshot As clsSnapshot = SnapshotFromStream(tBR, Version)
                            If tSnapshot IsNot Nothing Then
                                Dim tWSList As List(Of clsWorkspace) = SnapshotWorkspacesListUnpack(tProject.ID, tSnapshot.ProjectWorkspace)
                                Dim tWSNew As New List(Of clsWorkspace)
                                If tWSList IsNot Nothing Then
                                    For Each tWS As clsWorkspace In tWSList
                                        If tUsersLinks.ContainsKey(tWS.UserID) AndAlso tGroupsLinks.ContainsKey(tWS.GroupID) Then
                                            Dim tNewGrpID As Integer = tGroupsLinks(tWS.GroupID)
                                            Dim tNewUserID As Integer = tUsersLinks(tWS.UserID)
                                            Dim tNewGrp As clsRoleGroup = ActiveWorkgroup.RoleGroup(tNewGrpID)
                                            If tNewGrp IsNot Nothing Then
                                                Dim isPM As Boolean = tNewGrp.ActionStatus(ecActionType.at_mlManageProjectOptions) = ecActionStatus.asGranted OrElse tNewGrp.ActionStatus(ecActionType.at_mlModifyModelHierarchy) = ecActionStatus.asGranted OrElse tNewGrp.ActionStatus(ecActionType.at_mlManageModelUsers) = ecActionStatus.asGranted
                                                If isPM AndAlso Not CanUserBePM(ActiveWorkgroup, tNewUserID, tProject, False, True, , clsWorkspace.WorkspaceByUserIDAndProjectID(tNewUserID, tProject.ID, tProjectWS)) Then
                                                    tNewGrpID = tDefGrpID
                                                End If
                                                tWS.ProjectID = tProject.ID
                                                tWS.UserID = tNewUserID
                                                tWS.GroupID = tNewGrpID
                                                tWSNew.Add(tWS)
                                            End If
                                        End If
                                    Next
                                End If
                                tSnapshot.ProjectWorkspace = SnapshotWorkspacesListPack(tWSNew)
                                tSnapshot.ProjectWorkspaceSize = tSnapshot.ProjectWorkspace.Length
                                tSnapshot.ProjectWorkspaceMD5 = GetMD5(tSnapshot.ProjectWorkspace)  ' D3882

                                tSnapshot.ProjectID = tProject.ID   ' D3882
                                If DBSnapshotAdd(tSnapshot, False) Then fRes += 1 ' D3882 + D3892 + D3898
                            End If
                        Next

                        'fRes = (SavedCnt = Cnt)     ' D3882 - D3892
                    End If
                End If
                'tBR.Close()    ' -D3890
            End If
            Return fRes
        End Function
        ' D3851 ==


#End Region


        '#Region "Infodoc State"

        '        ' D4224 ===
        '        'Public Function GetInfodocState(tApp As ecAppliationID, tAction As clsAction, tNode As clsNode, tWRTNode As clsNode, tDefParams As clsInfodocState) As clsInfodocState
        '        '    Dim tStep As ecEvaluationStepType = ecEvaluationStepType.Other
        '        '    Dim PM As clsProjectManager = Nothing
        '        '    If tWRTNode IsNot Nothing AndAlso tWRTNode.Hierarchy IsNot Nothing AndAlso TypeOf (tWRTNode.Hierarchy.ProjectManager) Is clsProjectManager Then PM = CType(tWRTNode.Hierarchy.ProjectManager, clsProjectManager)
        '        '    If tNode IsNot Nothing AndAlso tNode.Hierarchy IsNot Nothing AndAlso TypeOf (tNode.Hierarchy.ProjectManager) Is clsProjectManager Then PM = CType(tNode.Hierarchy.ProjectManager, clsProjectManager)
        '        '    If tAction IsNot Nothing AndAlso PM IsNot Nothing Then tStep = PM.PipeBuilder.GetPipeActionStepType(tAction)
        '        '    Return GetInfodocState(tApp, PM, tStep, tNode, tWRTNode, tDefParams)
        '        'End Function

        '        'Private Function InfodocStateEncode

        '        Public Function GetInfodocState(tApp As ecAppliationID, tActionType As ecEvaluationStepType, HID As ECHierarchyID, tNodeGUID As Guid, tWRTNodeGUID As Guid, tDefParams As clsInfodocState) As clsInfodocState
        '            Dim tRes As clsInfodocState = tDefParams
        '            If HasActiveProject() Then
        '                Dim NID As Integer = CRC32.ComputeAsInt(tNodeGUID.ToString)
        '                Dim WID As Integer = CRC32.ComputeAsInt(tWRTNodeGUID.ToString)
        '                Dim sOrigParams As String = ActiveProject.PipeParameters.PipeMessages.GetEvaluationInfodocState(HID, tActionType, NID, WID)
        '                If Not String.IsNullOrEmpty(sOrigParams) Then
        '                    If tRes Is Nothing Then tRes = New clsInfodocState
        '                    If Not clsInfodocState.Decode(tApp, sOrigParams, tRes) AndAlso tDefParams Is Nothing Then tRes = Nothing
        '                End If
        '            End If
        '            Return tRes
        '        End Function

        '        Public Sub SetInfodocState(tApp As ecAppliationID, tActionType As ecEvaluationStepType, HID As ECHierarchyID, tNodeGUID As Guid, tWRTNodeGUID As Guid, tParams As clsInfodocState)
        '            If HasActiveProject() AndAlso tParams IsNot Nothing Then
        '                Dim NID As Integer = CRC32.ComputeAsInt(tNodeGUID.ToString)
        '                Dim WID As Integer = CRC32.ComputeAsInt(tWRTNodeGUID.ToString)
        '                Dim sOrigParams As String = ActiveProject.PipeParameters.PipeMessages.GetEvaluationInfodocState(HID, tActionType, NID, WID)
        '                Dim tCoreParams As New clsInfodocState
        '                Dim fHasCore As Boolean = clsInfodocState.Decode(ecAppliationID.appComparion, sOrigParams, tCoreParams)
        '                Dim tGeckoParams As New clsInfodocState
        '                Dim fHasGecko As Boolean = clsInfodocState.Decode(ecAppliationID.appGecko, sOrigParams, tGeckoParams)
        '                If tApp = ecAppliationID.appComparion Then
        '                    fHasCore = True
        '                    tCoreParams = tParams
        '                End If
        '                If tApp = ecAppliationID.appGecko Then
        '                    fHasGecko = True
        '                    tGeckoParams = tParams
        '                End If
        '                Dim sParams As String = ""
        '                If fHasCore Then sParams = tCoreParams.Encode
        '                If fHasGecko Then sParams += CStr(IIf(sParams = "", "", "&")) + tGeckoParams.Encode
        '                ActiveProject.PipeParameters.PipeMessages.SetEvaluationInfodocState(HID, tActionType, NID, WID, sParams)
        '            End If
        '        End Sub
        '        ' D4224 ==

        '#End Region

        Public Function GetAttributeName(tAttr As clsAttribute) As String
            Dim sName As String = ""
            If tAttr IsNot Nothing Then
                If tAttr.IsDefault AndAlso tAttr.ResourceName <> "" Then
                    sName = ResString(tAttr.ResourceName, True)
                    If sName = tAttr.ResourceName Then sName = tAttr.Name
                Else
                    sName = tAttr.Name
                End If
            End If
            Return sName
        End Function

#Region "NodeSets"

        ' D5042 ===
        Public Function NodeSets_RestoreDefaults(HID As ecNodeSetHierarchy, OverrideIfExists As Boolean, Optional ByRef sError As String = Nothing) As Boolean
            If ActiveWorkgroup Is Nothing Then
                If sError IsNot Nothing Then sError = "No active workgroup"
                Return False
            End If
            Dim sMsg As String = ""
            Dim sRoot As String = clsNodeSet.NodeSet_GetPath(HID, ActiveWorkgroup.ID)
            Dim sSamplesRoot As String = clsNodeSet.NodeSet_GetSamplesPath(HID)
            If My.Computer.FileSystem.DirectoryExists(sSamplesRoot) Then
                Dim tFiles As List(Of String) = GetProjectFilesList(sSamplesRoot, {_FILE_EXT_NODESET})
                If tFiles IsNot Nothing Then
                    If My.Computer.FileSystem.DirectoryExists(sRoot) OrElse File_CreateFolder(sRoot) Then
                        For Each sFile As String In tFiles
                            Dim sFileName As String = Path.GetFileName(sFile)
                            Dim DestName As String = String.Format("{0}{1}", sRoot, sFileName)
                            Dim fExists As Boolean = My.Computer.FileSystem.FileExists(DestName)
                            If Not fExists OrElse OverrideIfExists Then
                                If fExists Then File_Erase(DestName)
                                Try
                                    My.Computer.FileSystem.CopyFile(String.Format("{0}{1}", sSamplesRoot, sFile), DestName)
                                Catch ex As Exception
                                    sMsg = "Unable to copy DataSets file from Samples"
                                    Exit For
                                End Try
                            End If
                        Next
                    Else
                        sMsg = "Unable to create folder with DataSets"
                    End If
                End If
            Else
                sMsg = "No DataSet Samples found"
            End If
            If sMsg <> "" AndAlso sError IsNot Nothing Then sError = sMsg
            Return Not String.IsNullOrEmpty(sMsg)
        End Function
        ' D5042 ==

        Public Function NodeSets_GetList(HID As ecNodeSetHierarchy, Optional CreateDefaultsIfMissing As Boolean = False, Optional sError As String = Nothing) As List(Of clsNodeSet)
            If ActiveWorkgroup Is Nothing Then
                If sError IsNot Nothing Then sError = "No active workgroup"
                Return Nothing
            End If
            Dim tRes As New List(Of clsNodeSet)
            Dim sMsg As String = ""

            ' Check if workgroup datasets are exists. Can create a copy of samples in case of required
            Dim sRoot As String = clsNodeSet.NodeSet_GetPath(HID, ActiveWorkgroup.ID)
            If Not My.Computer.FileSystem.DirectoryExists(sRoot) Then
                If CreateDefaultsIfMissing Then
                    NodeSets_RestoreDefaults(HID, False, sMsg)  ' D5042
                End If
            End If

            ' Get the datasets list on case of they are exists and no any issues happened before
            If sMsg = "" AndAlso My.Computer.FileSystem.DirectoryExists(sRoot) Then
                Dim tFiles As List(Of String) = GetProjectFilesList(sRoot, {_FILE_EXT_NODESET})
                ' D5055 ===
                If (tFiles Is Nothing OrElse tFiles.Count = 0) AndAlso CreateDefaultsIfMissing Then
                    NodeSets_RestoreDefaults(HID, False, sMsg)
                    tFiles = GetProjectFilesList(sRoot, {_FILE_EXT_NODESET})
                End If
                ' D5055 ==
                If tFiles IsNot Nothing Then
                    For Each sFile As String In tFiles
                        Dim sContent As String = File_GetContent(String.Format("{0}{1}", sRoot, sFile), sMsg)
                        If String.IsNullOrEmpty(sMsg) Then
                            If sContent.Trim <> "" Then
                                Dim tDS As New clsNodeSet With {
                                        .Name = Web.HttpUtility.UrlDecode(Path.GetFileNameWithoutExtension(sFile).Replace("_", " ").Replace("  ", " ").Trim()),
                                        .Filename = sFile,
                                        .Hierarchy = HID,
                                        .Content = sContent
                                    }
                                tRes.Add(tDS)
                            End If
                        Else
                            Exit For
                        End If
                    Next
                End If
                If String.IsNullOrEmpty(sMsg) AndAlso (tFiles Is Nothing OrElse tFiles.Count = 0) Then sMsg = "DataSets are not exists" ' D5056
            End If

            If sMsg <> "" AndAlso sError IsNot Nothing Then sError = sMsg
            Return tRes
        End Function

#End Region

#Region "IDisposable Support"
        ' D2236 ===
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    If Database IsNot Nothing Then Database.Close()
                End If
                ActiveProjectsList = Nothing
                Database = Nothing
            End If
            Me.disposedValue = True
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
        ' D2236 ==
#End Region

    End Class

End Namespace