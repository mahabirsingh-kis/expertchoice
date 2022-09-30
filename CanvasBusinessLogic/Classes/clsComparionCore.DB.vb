Imports System.IO
Imports System.Data.Common
Imports GenericDBAccess.ECGenericDatabaseAccess
Imports ExpertChoice.Database
Imports ExpertChoice.Service
Imports ECCore
Imports ECCore.ECTypes
Imports ECSecurity.ECSecurity
Imports Canvas
Imports System.Web
Imports SpyronControls.Spyron.Core
Imports System.Runtime.Serialization.Formatters.Binary

Namespace ExpertChoice.Data

    Partial Public Class clsComparionCore
        Implements IDisposable

        'Private _ProjectsList As ArrayList
        <NonSerializedAttribute()> Private _Database As clsDatabaseAdvanced

        Private _CanvasMasterConnDefinition As clsConnectionDefinition = Nothing   ' D0330 + MF0332
        Private _CanvasProjectsConnDefinition As clsConnectionDefinition = Nothing   ' D0330 + MF0332 + D0478
        'Private _SpyronMasterConnDefinition As clsConnectionDefinition = Nothing   ' D0330 + MF0332 -D6423
        'Private _SpyronProjectsConnDefinition As clsConnectionDefinition = Nothing  ' D0488 -D6423

        ' D0465 ===
        Private _CanvasMasterDBValid As Boolean = False
        Private _CanvasMasterDBVersion As String = ""
        Private _CanvasMasterDBChecked As Boolean = False

        Private _CanvasProjectsDBValid As Boolean = False
        Private _CanvasProjectsDBVersion As String = ""
        Private _CanvasProjectsDBChecked As Boolean = False

        ' -D6423 ===
        'Private _SpyronMasterDBValid As Boolean = False
        'Private _SpyronMasterDBChecked As Boolean = False

        'Private _SpyronProjectsDBValid As Boolean = False
        'Private _SpyronProjectsDBChecked As Boolean = False
        ' -D6423 ==
        ' D0465 ==

        Public Const _OnlineSessionsTimeout As Integer = 5                      ' D0501 + D1785
        Private _OnlineSessionsCache As List(Of clsOnlineUserSession) = Nothing ' D0501
        Private _OnlineSessionsLoaded As Nullable(Of DateTime) = Nothing        ' D0501

        Private LastDates As New Dictionary(Of String, KeyValuePair(Of Integer, DateTime))    ' D3562

        Private _isMasterConnected As Boolean = False
        Private _isComparionDBExists As Boolean = False

        Private Function dbExists() As Boolean
            Return _isMasterConnected AndAlso _isComparionDBExists
        End Function

#Region "Definitions for tables"

        ' D0461 ===
        Public Const _TABLE_USERS As String = "Users"
        ' D0462 ===
        Public Const _TABLE_WORKGROUPS As String = "Workgroups"
        Public Const _TABLE_WKGPARAMS As String = "WorkgroupParams"     ' D2212
        Public Const _TABLE_ROLEGROUPS As String = "RoleGroups"
        Public Const _TABLE_ROLEACTIONS As String = "RoleActions"
        Public Const _TABLE_USERWORKGROUPS As String = "UserWorkgroups"
        Public Const _TABLE_USERTEMPLATES As String = "UserTemplates"   ' D0795
        Public Const _TABLE_WORKSPACE As String = "Workspace"    ' D0464
        Public Const _TABLE_PROJECTS As String = "Projects"
        Public Const _TABLE_EXTRA As String = "Extra"  ' D0464
        Public Const _TABLE_TEAMTIMEDATA As String = "TeamTimeData" ' D1275
        Public Const _TABLE_SNAPSHOTS As String = "Snapshots"  ' D3509
        ' D0462 ==

#End Region


#Region "Definitions for table fields"

        Public Const _FLD_USERS_ID As String = "ID"
        Public Const _FLD_USERS_EMAIL As String = "Email"
        Public Const _FLD_USERS_STATUS As String = "Status"   ' D0463
        Public Const _FLD_USERS_LASTVISITED As String = "LastVisited"   ' D0491
        Public Const _FLD_USERS_LASTPAGEID As String = "LastPageID"     ' D0491
        Public Const _FLD_USERS_LASTURL As String = "LastURL"           ' D0491
        ' D0461 ==
        ' D0463 ===
        Public Const _FLD_WORKGROUPS_ID As String = "ID"
        Public Const _FLD_WORKGROUPS_NAME As String = "Name"
        Public Const _FLD_WORKGROUPS_STATUS As String = "Status"
        Public Const _FLD_WORKGROUPS_LASTVISITED As String = "LastVisited" ' D0491

        Public Const _FLD_ROLEGROUPS_ID As String = "ID"
        Public Const _FLD_ROLEGROUPS_PID As String = "ParentID"
        Public Const _FLD_ROLEGROUPS_WRKGID As String = "WorkgroupID"
        Public Const _FLD_ROLEGROUPS_NAME As String = "Name"
        Public Const _FLD_ROLEGROUPS_LEVEL As String = "RoleLevel"
        Public Const _FLD_ROLEGROUPS_TYPE As String = "GroupType"
        Public Const _FLD_ROLEGROUPS_STATUS As String = "Status"

        Public Const _FLD_ROLEACTIONS_ID As String = "ID"
        Public Const _FLD_ROLEACTIONS_GRPID As String = "RoleGroupID"
        Public Const _FLD_ROLEACTIONS_TYPE As String = "ActionType"
        Public Const _FLD_ROLEACTIONS_STATUS As String = "Status"
        ' D0463 ==
        ' D0464 ===
        Public Const _FLD_USERWRKG_ID As String = "ID"
        Public Const _FLD_USERWRKG_USERID As String = "UserID"
        Public Const _FLD_USERWRKG_WRKGID As String = "WorkgroupID"
        Public Const _FLD_USERWRKG_ROLEID As String = "RoleGroupID"
        Public Const _FLD_USERWRKG_STATUS As String = "Status"
        Public Const _FLD_USERWRKG_EXPIRATION As String = "ExpirationDate"
        Public Const _FLD_USERWRKG_LASTVISITED As String = "LastVisited"        ' D0491
        Public Const _FLD_USERWRKG_LASTPROJECTID As String = "LastProjectID"    ' D0491

        Public Const _FLD_WKGPARAMS_WKGID As String = "WorkgroupID"    ' D2212
        Public Const _FLD_WKGPARAMS_PARAMID As String = "ParameterID"   ' D2212

        Public Const _FLD_USERTPL_ID As String = "ID"               ' D0795
        Public Const _FLD_USERTPL_USERID As String = "UserID"       ' D0795
        Public Const _FLD_USERTPL_NAME As String = "TemplateName"   ' D0795
        Public Const _FLD_USERTPL_DATA As String = "Stream"         ' D0795

        Public Const _FLD_WORKSPACE_ID As String = "ID"
        Public Const _FLD_WORKSPACE_USERID As String = "UserID"
        Public Const _FLD_WORKSPACE_PRJID As String = "ProjectID"
        Public Const _FLD_WORKSPACE_GRPID As String = "GroupID"
        Public Const _FLD_WORKSPACE_STATUS As String = "Status"
        Public Const _FLD_WORKSPACE_STATUS_IMPACT As String = "Status2" ' D1945
        Public Const _FLD_WORKSPACE_STEP As String = "Step"     ' D0862
        Public Const _FLD_WORKSPACE_STEP_IMPACT As String = "Step2"     ' D1945
        Public Const _FLD_WORKSPACE_LASTMODIFY As String = "LastModify" ' D6443

        Public Const _FLD_PROJECTS_ID As String = "ID"
        Public Const _FLD_PROJECTS_WRKGID As String = "WorkgroupID"
        Public Const _FLD_PROJECTS_PASSCODE As String = "Passcode"
        Public Const _FLD_PROJECTS_PASSCODE_IMPACT As String = "Passcode2"  ' D1709
        Public Const _FLD_PROJECTS_NAME As String = "ProjectName"           ' D0497
        Public Const _FLD_PROJECTS_MEETINGID As String = "MeetingID"
        Public Const _FLD_PROJECTS_MEETINGID_IMPACT As String = "MeetingID2"    ' D1709
        Public Const _FLD_PROJECTS_STATUS As String = "Status"
        Public Const _FLD_PROJECTS_STATUS_IMPACT As String = "Status2"      ' D1944
        Public Const _FLD_PROJECTS_LASTVISITED As String = "LastVisited"    ' D0491
        Public Const _FLD_PROJECTS_LASTMODIFY As String = "LastModify"      ' D0496

        Public Const _FLD_EXTRA_ID As String = "ID"
        Public Const _FLD_EXTRA_TYPEID As String = "TypeID"
        Public Const _FLD_EXTRA_OBJECTID As String = "ObjectID"
        Public Const _FLD_EXTRA_PROPID As String = "PropertyID"
        Public Const _FLD_EXTRA_PROPVALUE As String = "PropertyValue"
        ' D0464 ==

        ' D3509 ===
        Public Const _FLD_SNAPSHOTS_ID As String = "ID"
        Public Const _FLD_SNAPSHOTS_DT As String = "DT"
        Public Const _FLD_SNAPSHOTS_PRJ_ID As String = "ProjectID"
        Public Const _FLD_SNAPSHOTS_TYPE As String = "SnapshotType"
        Public Const _FLD_SNAPSHOTS_STREAM As String = "Stream"
        Public Const _FLD_SNAPSHOTS_WORKSPACE As String = "Workspace"
        Public Const _FLD_SNAPSHOTS_COMMENT As String = "Comment"
        ' D3509 ==

#End Region


#Region "Connection definitions and master Database connections"

        ' D0330 ===
        Public Property CanvasMasterConnectionDefinition() As clsConnectionDefinition 'MF0332 + D0478
            Get
                ' D0465 ===
                If _CanvasMasterConnDefinition Is Nothing AndAlso Options.CanvasMasterDBName <> "" Then
                    _CanvasMasterConnDefinition = getConnectionDefinition(Options.CanvasMasterDBName)
                End If
                ' D0465 ==
                Return _CanvasMasterConnDefinition
            End Get
            Set(ByVal value As clsConnectionDefinition) 'MF0332
                _CanvasMasterConnDefinition = value
            End Set
        End Property

        ' D0478 ===
        Public Property CanvasProjectsConnectionDefinition() As clsConnectionDefinition 'MF0332
            Get
                ' D0465 ===
                If _CanvasProjectsConnDefinition Is Nothing AndAlso Options.CanvasProjectsDBName <> "" Then
                    _CanvasProjectsConnDefinition = getConnectionDefinition(Options.CanvasProjectsDBName)
                End If
                ' D0465 ==
                Return _CanvasProjectsConnDefinition
            End Get
            Set(ByVal value As clsConnectionDefinition) 'MF0332
                _CanvasProjectsConnDefinition = value
            End Set
        End Property
        ' D0478 ==

        ' -D6423 ===
        'Public Property SpyronMasterConnectionDefinition() As clsConnectionDefinition 'MF0332
        '    Get
        '        ' D0465 ===
        '        If _SpyronMasterConnDefinition Is Nothing AndAlso Options.SpyronMasterDBName <> "" Then
        '            _SpyronMasterConnDefinition = getConnectionDefinition(Options.SpyronMasterDBName)
        '        End If
        '        ' D0465 ==
        '        Return _SpyronMasterConnDefinition
        '    End Get
        '    Set(ByVal value As clsConnectionDefinition) 'MF0332
        '        _SpyronMasterConnDefinition = value
        '    End Set
        'End Property

        '' D0488 ===
        'Public Property SpyronProjectsConnectionDefinition() As clsConnectionDefinition
        '    Get
        '        ' D0465 ===
        '        If _SpyronProjectsConnDefinition Is Nothing AndAlso Options.SpyronProjectsDBName <> "" Then
        '            _SpyronProjectsConnDefinition = getConnectionDefinition(Options.SpyronProjectsDBName)
        '        End If
        '        ' D0465 ==
        '        Return _SpyronProjectsConnDefinition
        '    End Get
        '    Set(ByVal value As clsConnectionDefinition) 'MF0332
        '        _SpyronProjectsConnDefinition = value
        '    End Set
        'End Property
        '' D0488 ==
        ' -D6423 ==

        Public ReadOnly Property DefaultProvider() As DBProviderType
            Get
                Return CanvasMasterConnectionDefinition.ProviderType  ' D0342 + D0372 + D0348
            End Get
        End Property
        ' D0330 ==

        ' D0009 ===
        Public Property Database() As clsDatabaseAdvanced
            Get
                'If _Database Is Nothing AndAlso isCanvasMasterDBValid Then ' -D0490
                If _Database Is Nothing AndAlso Options IsNot Nothing AndAlso Not String.IsNullOrEmpty(Options.CanvasMasterDBName) Then   ' D0490 + D3930
                    _Database = New clsDatabaseAdvanced(clsDatabaseAdvanced.GetConnectionString(Options.CanvasMasterDBName), DefaultProvider)    ' D0330 + D0372 + D0458
                    DebugInfo("Init database connection", _TRACE_INFO)
                End If
                Return _Database
            End Get
            Set(ByVal value As clsDatabaseAdvanced)
                _Database = value
            End Set
        End Property
        ' D0009 ==

#End Region


#Region "Check DBs"

        ' D0465 ===
        Private Sub CheckCanvasMasterDB()
            _CanvasMasterDBValid = DBCheckDatabase(ecDBType.dbCanvasMaster, _CanvasMasterDBVersion)
            _CanvasMasterDBChecked = True
            _InstanceID = 0     ' D3967
        End Sub

        Private Sub CheckCanvasProjectsDB()
            _CanvasProjectsDBValid = DBCheckDatabase(ecDBType.dbCanvasProjects, _CanvasProjectsDBVersion)
            _CanvasProjectsDBChecked = True
        End Sub

        ' -D6423 ===
        'Private Sub CheckSpyronMasterDB()
        '    _SpyronMasterDBValid = DBCheckDatabase(ecDBType.dbSpyronMaster)
        '    _SpyronMasterDBChecked = True
        'End Sub

        'Private Sub CheckSpyronProjectsDB()
        '    _SpyronProjectsDBValid = DBCheckDatabase(ecDBType.dbSpyronProjects)
        '    _SpyronProjectsDBChecked = True
        'End Sub
        ' -D6423 ==

        Public ReadOnly Property isCanvasMasterDBValid() As Boolean
            Get
                If Not _CanvasMasterDBChecked Then CheckCanvasMasterDB()
                Return _CanvasMasterDBValid
            End Get
        End Property

        Public ReadOnly Property isCanvasProjectsDBValid() As Boolean
            Get
                If Not _CanvasProjectsDBChecked Then CheckCanvasProjectsDB()
                Return _CanvasProjectsDBValid
            End Get
        End Property

        ' -D6423 ===
        'Public ReadOnly Property isSpyronMasterDBValid() As Boolean
        '    Get
        '        If Not _SpyronMasterDBChecked Then CheckSpyronMasterDB()
        '        Return _SpyronMasterDBValid
        '    End Get
        'End Property

        'Public ReadOnly Property isSpyronProjectsDBValid() As Boolean
        '    Get
        '        If Not _SpyronProjectsDBChecked Then CheckSpyronProjectsDB()
        '        Return _SpyronProjectsDBValid
        '    End Get
        'End Property

        Public ReadOnly Property CanvasMasterDBVersion() As String
            Get
                If Not _CanvasMasterDBChecked Then CheckCanvasMasterDB()
                Return _CanvasMasterDBVersion
            End Get
        End Property

        Public ReadOnly Property CanvasProjectsDBVersion() As String
            Get
                If Not _CanvasProjectsDBChecked Then CheckCanvasProjectsDB()
                Return _CanvasProjectsDBVersion
            End Get
        End Property

        ' D0490 ===
        Public Sub ResetDBChecks()
            _CanvasMasterDBChecked = False
            _CanvasProjectsDBChecked = False
            '_SpyronMasterDBChecked = False     ' -D6423
            '_SpyronProjectsDBChecked = False   ' -D6423
            _InstanceID = 0     ' D3967
        End Sub
        ' D0490 ==

        ' D0465 ==
        Public Function DBCheckDatabase(ByVal fDatabase As ecDBType, Optional ByRef sVersionVar As String = Nothing, Optional ByRef sDatabaseName As String = Nothing) As Boolean
            Dim fResult As Boolean = False
            Dim sVersion As String = ""
            ' D0376 ===
            Dim sDatabase As String = ""

            Dim fProvider As DBProviderType
            Dim sCheckSQL As String = ""
            Dim sVersionSQL As String = ""

            Select Case fDatabase
                Case ecDBType.dbCanvasMaster
                    sCheckSQL = "SELECT OBJECT_ID('Projects')"
                    '                    sVersionSQL = String.Format("SELECT PropertyValue FROM Extra WHERE TypeID = {0} AND PropertyID = {1}", CInt(ecExtraType.Common), CInt(ecExtraProperty.Version))
                    sVersionSQL = "IF OBJECT_ID('dbo.ExtraInfo') IS NULL BEGIN" + vbCr +
                                  " SELECT PropertyValue FROM Extra WHERE TypeID = 1 AND PropertyID = 1 " + vbCr +
                                  "END" + vbCr +
                                  "ELSE BEGIN" + vbCr +
                                  " SELECT ObjectValue FROM ExtraInfo WHERE ExtraType=99 AND ObjectProperty='DatabaseVersion'" + vbCr +
                                  "END"
                    fProvider = CanvasMasterConnectionDefinition.ProviderType
                    sDatabase = Options.CanvasMasterDBName
                Case ecDBType.dbCanvasProjects
                    sCheckSQL = "SELECT OBJECT_ID('ModelStructure')"
                    sVersionSQL = "SELECT PropertyValue FROM Properties WHERE PropertyName = 'DatabaseVersion'"
                    fProvider = CanvasProjectsConnectionDefinition.ProviderType ' D0478
                    sDatabase = Options.CanvasProjectsDBName
                    ' -D6423 ===
                    'Case ecDBType.dbSpyronMaster
                    '    sCheckSQL = "SELECT OBJECT_ID('Surveys')"
                    '    fProvider = SpyronMasterConnectionDefinition.ProviderType
                    '    sDatabase = Options.SpyronMasterDBName
                    '    ' D0375 ===
                    'Case ecDBType.dbSpyronProjects
                    '    sCheckSQL = "SELECT OBJECT_ID('SurveyStructure')"
                    '    fProvider = SpyronMasterConnectionDefinition.ProviderType
                    '    sDatabase = Options.SpyronProjectsDBName
                    '' D0375 ==
                    ' -D6423 ==
            End Select

            If sDatabase = "" Or sCheckSQL = "" Then
                sVersion = "Can't detect DB name"
            Else

                If Not sDatabaseName Is Nothing Then sDatabaseName = sDatabase ' D0475

                DebugInfo(String.Format("Check DB '{0}'...", sDatabase))   ' D0430
                Dim sConnString As String = clsDatabaseAdvanced.GetConnectionString(sDatabase)  ' D0458

                Using MasterDB As New clsDatabaseAdvanced(clsDatabaseAdvanced.GetConnectionString("master"), fProvider)
                    _isMasterConnected = MasterDB.Connect 'MRFXXX Ask AD to add the status of this variable to the install screen (Master DB Connected:True/False)
                    Dim dbCountResult = ""
                    Dim dbCountObj As Object = MasterDB.ExecuteScalarSQL(String.Format("select count(*) from sys.databases where name = '{0}'", sDatabase))
                    If Not IsDBNull(dbCountObj) Then dbCountResult = CStr(dbCountObj)
                    _isComparionDBExists = (dbCountResult = "1")
                End Using

                Using DB As New clsDatabaseAdvanced(sConnString, fProvider) ' D2235
                    If _isMasterConnected AndAlso _isComparionDBExists AndAlso DB.Connect Then
                        Try ' D0496
                            Dim Res As Object = DB.ExecuteScalarSQL(sCheckSQL)
                            fResult = Not IsDBNull(Res)
                            If fResult AndAlso Not sVersionVar Is Nothing AndAlso sVersionSQL <> "" Then
                                Dim sVer As Object = DB.ExecuteScalarSQL(sVersionSQL)
                                If Not IsDBNull(sVer) Then sVersion = CStr(sVer) ' D0468 '- "ver." prefix
                                If sVersion = "" Then sVersion = "Missed tables" ' D0549
                            Else
                                sVersion = "undef. ver."  'MRF (why is this being hit over and over?)
                            End If
                            DebugInfo(String.Format("DB '{0}' checked. Version: {1}", sDatabase, sVersion))   ' D0430
                        Catch ex As Exception
                            DebugInfo("Error on check DB: " + ex.Message)   ' D0496
                        End Try
                        DB.Close()
                    Else
                        Dim msg = ""
                        If Not _isMasterConnected Then
                            msg = "could not connect to master database"
                            sVersion = msg
                            Database.LastError = msg
                            DebugInfo(msg)
                        ElseIf Not _isComparionDBExists Then
                            msg = sDatabase + " does not exist"
                            sVersion = msg
                            Database.LastError = msg
                            DebugInfo(msg)
                        ElseIf Database IsNot Nothing AndAlso Database.LastError = "" Then
                            Database.LastError = DB.LastError ' D2157
                        End If
                    End If
                End Using

            End If

            If Not sVersionVar Is Nothing Then sVersionVar = sVersion

            'AddLog(String.Format("Check DB '{0}': {1}", sDatabase, IIf(fResult, "exists" + IIf(sVersion <> "", ", " + sVersion, ""), IIf(sVersion = "", "missed", sVersion))))
            Return fResult
        End Function
        ' D0465 ===

#End Region


#Region "Parse base structures"

        Public Function DBParse_ApplicationUser(ByVal tData As Dictionary(Of String, Object)) As clsApplicationUser
            If Not dbExists() Then Return Nothing
            Dim tUser As clsApplicationUser = Nothing
            If Not tData Is Nothing Then
                If Not tData(_FLD_USERS_EMAIL) Is Nothing AndAlso Not IsDBNull(tData(_FLD_USERS_EMAIL)) Then
                    tUser = New clsApplicationUser
                    tUser.UserID = CInt(tData(_FLD_USERS_ID))
                    tUser.UserEmail = CStr(tData(_FLD_USERS_EMAIL))
                    tUser.DBStatus = CInt(tData(_FLD_USERS_STATUS))   ' D0463 + D0475
                    If Not IsDBNull(tData("FullName")) Then tUser.UserName = CStr(tData("FullName"))
                    If Not IsDBNull(tData("Password")) Then tUser.UserPassword = CStr(tData("Password"))
                    If Not IsDBNull(tData("Comment")) Then tUser.Comment = CStr(tData("Comment"))
                    If Not IsDBNull(tData("DefaultWGID")) Then tUser.DefaultWorkgroupID = CInt(tData("DefaultWGID"))
                    If Not IsDBNull(tData("OwnerID")) Then tUser.OwnerID = CInt(tData("OwnerID")) ' D1643
                    ' D0494 ===
                    If tData.ContainsKey("Created") AndAlso Not IsDBNull(tData("Created")) Then tUser.Created = CType(tData("Created"), DateTime)
                    If tData.ContainsKey("LastModify") AndAlso Not IsDBNull(tData("LastModify")) Then tUser.LastModify = CType(tData("LastModify"), DateTime)
                    If tData.ContainsKey("LastVisited") AndAlso Not IsDBNull(tData("LastVisited")) Then tUser.LastVisited = CType(tData("LastVisited"), DateTime)   ' D6062
                    If tData.ContainsKey("isOnline") AndAlso Not IsDBNull(tData("isOnline")) Then tUser.isOnline = CBool(tData("isOnline"))
                    If tUser.isOnline Then
                        tData.Add("UserID", tUser.UserID)
                        tUser.Session = DBParse_Session(tData)
                        If tUser.isOnline AndAlso tUser.Session.LastAccess.HasValue Then tUser.isOnline = tUser.Session.LastAccess.Value > Now.AddSeconds(-_DEF_SESS_TIMEOUT)
                    Else
                        tUser.Session = New clsOnlineUserSession
                    End If
                    If tData.ContainsKey("LastWorkgroupID") AndAlso Not IsDBNull(tData("LastWorkgroupID")) Then tUser.Session.WorkgroupID = CInt(tData("LastWorkgroupID")) ' D0494
                    If tData.ContainsKey("PasswordStatus") AndAlso Not IsDBNull(tData("PasswordStatus")) Then
                        ' D6446 ===
                        Dim st As Integer = CInt(tData("PasswordStatus"))
                        If st = -1 AndAlso tUser.CannotBeDeleted Then st = -9
                        tUser.PasswordStatus = st ' D2213
                        ' D6446 ==
                    End If
                    tUser.Session.UserEmail = tUser.UserEmail
                    tUser.Session.UserID = tUser.UserID
                    ' D0494 ==
                End If
            End If
            Return tUser
        End Function


        ' D0463 ===
        Public Function DBParse_Workgroup(ByVal tData As Dictionary(Of String, Object)) As clsWorkgroup
            If Not dbExists() Then Return Nothing
            Dim tWorkgroup As clsWorkgroup = Nothing
            If Not tData Is Nothing Then
                If Not tData(_FLD_WORKGROUPS_NAME) Is Nothing AndAlso Not IsDBNull(tData(_FLD_WORKGROUPS_NAME)) Then
                    tWorkgroup = New clsWorkgroup
                    tWorkgroup.ID = CInt(tData(_FLD_WORKGROUPS_ID))
                    tWorkgroup.Name = CStr(tData(_FLD_WORKGROUPS_NAME))
                    tWorkgroup.Status = CType(tData(_FLD_WORKGROUPS_STATUS), ecWorkgroupStatus)  ' D0491
                    If Not IsDBNull(tData("Comment")) Then tWorkgroup.Comment = CStr(tData("Comment"))
                    If Not IsDBNull(tData("OwnerID")) Then tWorkgroup.OwnerID = CInt(tData("OwnerID"))
                    If Not IsDBNull(tData("ECAMID")) Then tWorkgroup.ECAMID = CInt(tData("ECAMID"))
                    ' D0494 ===
                    If tData.ContainsKey("Created") AndAlso Not IsDBNull(tData("Created")) Then tWorkgroup.Created = CType(tData("Created"), DateTime)
                    If tData.ContainsKey("LastModify") AndAlso Not IsDBNull(tData("LastModify")) Then tWorkgroup.LastModify = CType(tData("LastModify"), DateTime)
                    If tData.ContainsKey("LastVisited") AndAlso Not IsDBNull(tData("LastVisited")) Then tWorkgroup.LastVisited = CType(tData("LastVisited"), DateTime)
                    ' D0494 ==
                    ' D0922 ==
                    If CanvasMasterDBVersion >= "0.992" Then
                        If tData.ContainsKey("EULAFile") AndAlso Not IsDBNull(tData("EULAFile")) Then tWorkgroup.EULAFile = CStr(tData("EULAFile"))
                        If tData.ContainsKey("LifetimeProjects") AndAlso Not IsDBNull(tData("LifetimeProjects")) Then tWorkgroup.LifeTimeProjects = CInt(tData("LifetimeProjects"))
                    End If
                    ' D0922 ==
                    If CanvasMasterDBVersion >= "0.9998" Then If tData.ContainsKey("OpportunityID") AndAlso Not IsDBNull(tData("OpportunityID")) Then tWorkgroup.OpportunityID = CStr(tData("OpportunityID")) ' D3333
                    ' D2384 ==
                    If CanvasMasterDBVersion >= "0.9997" Then
                        If tData.ContainsKey("WordingTemplates") AndAlso Not IsDBNull(tData("WordingTemplates")) Then
                            Try
                                Dim tWording() As Byte = CType(tData("WordingTemplates"), Byte())
                                Dim fs As New IO.MemoryStream
                                fs.Write(tWording, 0, tWording.Length)
                                fs.Seek(0, SeekOrigin.Begin)
                                Dim formatter As New BinaryFormatter
                                tWorkgroup.WordingTemplates = DirectCast(formatter.Deserialize(fs), Dictionary(Of String, String))
                            Catch ex As Exception
                                DebugInfo("Error on deserialize workgroup wording" + ex.Message, _TRACE_RTE)
                            End Try
                        End If
                    End If
                    ' D2384 ==
                    tWorkgroup.License.Enabled = Options.CheckLicense
                    If Options.CheckLicense And Not IsDBNull(tData("LicenseData")) Then
                        If Not tData("LicenseKey") Is Nothing Then tWorkgroup.License.LicenseKey = CStr(tData("LicenseKey"))
                        Dim LicenseBytes() As Byte = CType(tData("LicenseData"), Byte())
                        tWorkgroup.License.LicenseContent = New MemoryStream(LicenseBytes)
                        tWorkgroup.License.isSystemLicense = tWorkgroup.Status = ecWorkgroupStatus.wsSystem
                        WorkgroupLicenseInit(tWorkgroup) ' D2644
                    End If
                End If
            End If
            Return tWorkgroup
        End Function

        Public Function DBParse_RoleGroup(ByVal tData As Dictionary(Of String, Object)) As clsRoleGroup
            If Not dbExists() Then Return Nothing
            Dim tGroup As clsRoleGroup = Nothing
            If Not tData Is Nothing Then
                If Not tData(_FLD_ROLEGROUPS_NAME) Is Nothing AndAlso Not IsDBNull(tData(_FLD_ROLEGROUPS_NAME)) Then
                    If Not IsDBNull(tData(_FLD_ROLEGROUPS_TYPE)) AndAlso [Enum].IsDefined(GetType(ecRoleGroupType), tData(_FLD_ROLEGROUPS_TYPE)) Then   ' D2781
                        tGroup = New clsRoleGroup
                        tGroup.ID = CInt(tData(_FLD_ROLEGROUPS_ID))
                        tGroup.WorkgroupID = CInt(tData(_FLD_ROLEGROUPS_WRKGID))
                        tGroup.RoleLevel = CType(tData(_FLD_ROLEGROUPS_LEVEL), ecRoleLevel)  ' D0491
                        tGroup.GroupType = CType(tData(_FLD_ROLEGROUPS_TYPE), ecRoleGroupType)   ' D0491
                        tGroup.Status = CType(tData(_FLD_ROLEGROUPS_STATUS), ecRoleGroupStatus)  ' D0491
                        tGroup.Name = CStr(tData(_FLD_ROLEGROUPS_NAME))
                        If Not IsDBNull(tData("Comment")) Then tGroup.Comment = CStr(tData("Comment"))
                        If tData.ContainsKey("Created") AndAlso Not IsDBNull(tData("Created")) Then tGroup.Created = CType(tData("Created"), DateTime) ' D0494
                        If tData.ContainsKey("LastModify") AndAlso Not IsDBNull(tData("LastModify")) Then tGroup.LastModify = CType(tData("LastModify"), DateTime) ' D0494
                    Else
                        DebugInfo("Unknown RoleGroup #" + tData(_FLD_ROLEGROUPS_TYPE).ToString + " '" + tData(_FLD_ROLEGROUPS_NAME).ToString + "'(new or outdated).Skip it", _TRACE_INFO) ' D2781
                    End If
                End If
            End If
            Return tGroup
        End Function

        Public Function DBParse_RoleAction(ByVal tData As Dictionary(Of String, Object)) As clsRoleAction
            If Not dbExists() Then Return Nothing
            Dim tAction As clsRoleAction = Nothing
            If Not tData Is Nothing Then
                If tData(_FLD_ROLEACTIONS_ID) IsNot Nothing AndAlso Not IsDBNull(tData(_FLD_ROLEACTIONS_ID)) Then
                    If Not IsDBNull(tData(_FLD_ROLEACTIONS_TYPE)) AndAlso [Enum].IsDefined(GetType(ecActionType), tData(_FLD_ROLEACTIONS_TYPE)) Then   ' D2779
                        tAction = New clsRoleAction
                        tAction.ID = CInt(tData(_FLD_ROLEACTIONS_ID))
                        tAction.RoleGroupID = CInt(tData(_FLD_ROLEACTIONS_GRPID))
                        tAction.Status = CType(tData(_FLD_ROLEACTIONS_STATUS), ecActionStatus)   ' D0491
                        tAction.ActionType = CType(tData(_FLD_ROLEACTIONS_TYPE), ecActionType)   ' D0491
                        If Not IsDBNull(tData("Comment")) Then tAction.Comment = CStr(tData("Comment"))
                    Else
                        DebugInfo("Found an unknown RoleAction '" + tData(_FLD_ROLEACTIONS_TYPE).ToString + "', ignore it", _TRACE_INFO)
                    End If
                End If
            End If
            Return tAction
        End Function
        ' D0463 ==

        ' D0464 ===
        Public Function DBParse_UserWorkgroup(ByVal tData As Dictionary(Of String, Object)) As clsUserWorkgroup
            If Not dbExists() Then Return Nothing
            Dim tUW As clsUserWorkgroup = Nothing
            If Not tData Is Nothing Then
                If Not tData(_FLD_USERWRKG_ID) Is Nothing AndAlso Not IsDBNull(tData(_FLD_USERWRKG_ID)) Then
                    tUW = New clsUserWorkgroup
                    tUW.ID = CInt(tData(_FLD_USERWRKG_ID))
                    tUW.RoleGroupID = CInt(tData(_FLD_USERWRKG_ROLEID))
                    tUW.UserID = CInt(tData(_FLD_USERWRKG_USERID))
                    tUW.WorkgroupID = CInt(tData(_FLD_USERWRKG_WRKGID))
                    tUW.Status = CType(tData(_FLD_USERWRKG_STATUS), ecUserWorkgroupStatus)  ' D0491
                    If Not IsDBNull(tData("Comment")) Then tUW.Comment = CStr(tData("Comment"))
                    If Not IsDBNull(tData(_FLD_USERWRKG_EXPIRATION)) Then tUW.ExpirationDate = CType(tData(_FLD_USERWRKG_EXPIRATION), DateTime)
                    If tData.ContainsKey("Created") AndAlso Not IsDBNull(tData("Created")) Then tUW.Created = CType(tData("Created"), DateTime) ' D0494
                    'If tData.ContainsKey("LastModify") AndAlso Not IsDBNull(tData("LastModify")) Then tUW.LastModify = CType(tData("LastModify"), DateTime) ' D0494
                    If tData.ContainsKey("LastVisited") AndAlso Not IsDBNull(tData("LastVisited")) Then tUW.LastVisited = CType(tData("LastVisited"), DateTime) ' D0494 + D4622
                    If tData.ContainsKey("LastProjectID") AndAlso Not IsDBNull(tData("LastProjectID")) Then tUW.LastProjectID = CInt(tData("LastProjectID")) ' D0494
                End If
            End If
            Return tUW
        End Function

        Public Function DBParse_Project(ByVal tData As Dictionary(Of String, Object)) As clsProject
            If Not dbExists() Then Return Nothing
            Dim tProject As clsProject = Nothing
            If Not tData Is Nothing Then
                If Not tData(_FLD_PROJECTS_PASSCODE) Is Nothing AndAlso Not IsDBNull(tData(_FLD_PROJECTS_PASSCODE)) Then
                    Dim sUserEmail As String = ""
                    If Not ActiveUser Is Nothing Then sUserEmail = ActiveUser.UserEmail
                    tProject = New clsProject(Options.ProjectLoadOnDemand, Options.ProjectForceAllowedAlts, sUserEmail, isRiskEnabled, AddressOf onProjectSavingEvent, AddressOf onProjectSavedEvent, Options.ProjectUseDataMapping, AddressOf onProjectUpdateLastModifyEvent)  ' D2255 + D3571 + D4465 + D4535
                    tProject.ConnectionString = CanvasProjectsConnectionDefinition.ConnectionString ' D0478
                    tProject.ID = CInt(tData(_FLD_PROJECTS_ID))
                    tProject.WorkgroupID = CInt(tData(_FLD_PROJECTS_WRKGID))
                    tProject.PasscodeLikelihood = CStr(tData(_FLD_PROJECTS_PASSCODE)).Trim  ' D1709
                    If tData.ContainsKey(_FLD_PROJECTS_PASSCODE_IMPACT) AndAlso Not IsDBNull(tData(_FLD_PROJECTS_PASSCODE_IMPACT)) Then tProject.PasscodeImpact = CStr(tData(_FLD_PROJECTS_PASSCODE_IMPACT)).Trim ' D1709
                    If tData.ContainsKey(_FLD_PROJECTS_MEETINGID) AndAlso Not IsDBNull(tData(_FLD_PROJECTS_MEETINGID)) Then clsMeetingID.TryParse(CStr(tData(_FLD_PROJECTS_MEETINGID)), tProject.MeetingIDLikelihood) Else tProject.MeetingIDLikelihood = clsMeetingID.ReNew ' D0512 + D1709
                    If tData.ContainsKey(_FLD_PROJECTS_MEETINGID_IMPACT) AndAlso Not IsDBNull(tData(_FLD_PROJECTS_MEETINGID_IMPACT)) Then clsMeetingID.TryParse(CStr(tData(_FLD_PROJECTS_MEETINGID_IMPACT)), tProject.MeetingIDImpact) Else tProject.MeetingIDImpact = clsMeetingID.ReNew ' D1709
                    ' D0504 ===
                    tProject.StatusDataLikelihood = CInt(tData(_FLD_PROJECTS_STATUS))
                    If tData.ContainsKey(_FLD_PROJECTS_STATUS_IMPACT) AndAlso Not IsDBNull(tData(_FLD_PROJECTS_STATUS_IMPACT)) AndAlso CInt(tData(_FLD_PROJECTS_STATUS_IMPACT)) <> 0 Then tProject.StatusDataImpact = CInt(tData(_FLD_PROJECTS_STATUS_IMPACT)) Else tProject.StatusDataImpact = tProject.StatusDataLikelihood ' D1944
                    If (tProject.isTeamTimeImpact OrElse tProject.isTeamTimeLikelihood) AndAlso tData.ContainsKey("MeetingOwnerID") AndAlso Not IsDBNull(tData("MeetingOwnerID")) Then   ' D0512 + D2203
                        Dim UID As Integer = CInt(tData("MeetingOwnerID"))
                        If UID >= 0 Then tProject.MeetingOwner = DBUserByID(UID)
                        If tProject.MeetingOwner Is Nothing Then
                            tProject.isTeamTimeLikelihood = False   ' D2203
                            tProject.isTeamTimeImpact = False       ' D2203
                        End If
                    End If
                    ' D0504 ==
                    If (tProject.isTeamTimeImpact OrElse tProject.isTeamTimeLikelihood) AndAlso tProject.MeetingOwnerID <= 0 Then
                        tProject.isTeamTimeLikelihood = False ' D0528 + D2203
                        tProject.isTeamTimeImpact = False   ' D2203
                    End If
                    If Not IsDBNull(tData("ProjectName")) Then tProject.ProjectName = CStr(tData("ProjectName")).Replace(Microsoft.VisualBasic.Strings.ChrW(8206), "") Else tProject.ProjectName = tProject.Passcode    ' D4797 // replace non-visual char
                    'If Not IsDBNull(tData("FileName")) Then tProject.FileName = CStr(tData("FileName")) ' D0499    ' -D1193
                    If Not IsDBNull(tData("OwnerID")) Then tProject.OwnerID = CInt(tData("OwnerID"))
                    If Not IsDBNull(tData("Comment")) Then tProject.Comment = CStr(tData("Comment"))
                    ' D0494 ===
                    If tData.ContainsKey("Created") AndAlso Not IsDBNull(tData("Created")) Then tProject.Created = CType(tData("Created"), DateTime)
                    If tData.ContainsKey("LastModify") AndAlso Not IsDBNull(tData("LastModify")) Then tProject.LastModify = CType(tData("LastModify"), DateTime)
                    If tData.ContainsKey("LastVisited") AndAlso Not IsDBNull(tData("LastVisited")) Then tProject.LastVisited = CType(tData("LastVisited"), DateTime)
                    If Not tProject.LastModify.HasValue AndAlso tProject.Created.HasValue Then tProject.LastModify = tProject.Created.Value ' D0842
                    ' D0494 ==
                    If tData.ContainsKey("GUID") AndAlso Not IsDBNull(tData("GUID")) Then tProject.ProjectGUID = New Guid(CStr(tData("GUID"))) ' D0892
                    If tData.ContainsKey("ReplacedID") AndAlso Not IsDBNull(tData("ReplacedID")) Then Integer.TryParse(tData("ReplacedID").ToString, tProject.ReplacedID) ' D0893
                    If tData.ContainsKey("ProjectType") AndAlso Not IsDBNull(tData("ProjectType")) Then Integer.TryParse(tData("ProjectType").ToString, tProject.ProjectTypeData) ' D3438
                    If tData.ContainsKey("LockStatus") AndAlso Not IsDBNull(tData("LockStatus")) Then tProject.LockInfo = DBParse_ProjectLockInfo(tData, tProject) ' D0483 + D0494 + D1074
                    ' D6546 ===
                    If tProject.LockInfo IsNot Nothing Then tProject.LockInfo.ProjectID = tProject.ID   ' D0494
                    ' D0589 ===
                    If tProject.LockInfo IsNot Nothing AndAlso (tProject.isTeamTimeImpact OrElse tProject.isTeamTimeLikelihood) Then    ' D2203
                        If (tProject.LastVisited.HasValue AndAlso tProject.LastVisited.Value.AddSeconds(_DEF_LOCK_TT_SESSION_TIMEOUT) < Now) OrElse (tProject.LastModify.HasValue AndAlso tProject.LastModify.Value.AddSeconds(_DEF_LOCK_TT_SESSION_TIMEOUT) < Now) Then
                            'TeamTimeEndSession(tProject, False)
                            DBTeamTimeDataDelete(tProject.ID, Nothing)
                            tProject.isTeamTime = False
                            tProject.MeetingOwner = Nothing
                            DBProjectUpdate(tProject, False, "End TeamTime session")
                            If tProject.LockInfo.LockStatus = ECLockStatus.lsLockForTeamTime Then DBProjectLockInfoWrite(ECLockStatus.lsUnLocked, tProject.LockInfo, tProject.MeetingOwner, Now)
                        Else
                            If tProject.LockInfo.LockStatus <> ECLockStatus.lsLockForTeamTime Then tProject.LockInfo.LockStatus = ECLockStatus.lsLockForTeamTime
                        End If
                    End If
                    ' D0589 + D6546 ==
                End If
            End If
            Return tProject
        End Function

        ' D0481 ===
        Public Function DBParse_ProjectLockInfo(ByVal tData As Dictionary(Of String, Object), ByVal tProject As clsProject) As clsProjectLockInfo    ' D1074
            If Not dbExists() Then Return Nothing
            Dim tLock As New clsProjectLockInfo ' D0483
            If Not tData Is Nothing Then
                If tData.ContainsKey("LockStatus") AndAlso Not tData("LockStatus") Is Nothing AndAlso tData.ContainsKey("LockedByUserID") AndAlso Not IsDBNull(tData("LockedByUserID")) Then    ' D0494
                    If Not IsDBNull(tData("LockStatus")) Then
                        Dim fLocked As Boolean = CInt(tData("LockStatus")) <> 0 ' D0483
                        If fLocked AndAlso Not IsDBNull(tData("LockedByUserID")) Then   ' D0483
                            Dim UID = CInt(tData("LockedByUserID"))
                            If UID > 0 Then
                                tLock.LockerUserID = UID    ' D0494
                                'tLock.LockerUser = DBParse_ApplicationUser(tData)  ' -D0494
                                ' D0483 ===
                                'tLock.LockerUser.UserID = UID  ' D0494
                                tLock.ProjectID = CInt(tData(_FLD_PROJECTS_ID))
                                Dim tLockStatus As ECLockStatus = ECLockStatus.lsUnLocked   ' D0589
                                If Not IsDBNull(tData("LockStatus")) Then tLockStatus = CType(tData("LockStatus"), ECLockStatus) ' D0589
                                If Not IsDBNull(tData("LockExpiration")) Then tLock.LockExpiration = CDate(tData("LockExpiration"))
                                If tLock.LockExpiration.HasValue AndAlso UID > 0 Then
                                    'If tLock.LockExpiration < Now AndAlso tLockStatus <> ECLockStatus.lsLockForTeamTime Then   ' D0589
                                    If tLock.LockExpiration < Now Then   ' D0589 + D1074
                                        DebugInfo(String.Format("Project lock is expired ({0})", tLock.LockExpiration.Value), _TRACE_WARNING)
                                        ' D7184 ===
                                        If tLockStatus <> ECLockStatus.lsLockForTeamTime Then
                                            tLockStatus = ECLockStatus.lsUnLocked
                                            DBProjectLockInfoWrite(ECLockStatus.lsUnLocked, tLock, ActiveUser, tLock.LockExpiration)
                                        Else
                                            ' D7184 ==
                                            If tLockStatus = ECLockStatus.lsLockForTeamTime AndAlso tProject.isValidDBVersion Then  ' D6546
                                                TeamTimeEndSession(tProject, False) ' D1074 + D2766
                                                DBProjectLockInfoWrite(ECLockStatus.lsUnLocked, tLock, Nothing, Now)  ' D0494 + D0589
                                            End If
                                            tLockStatus = ECLockStatus.lsUnLocked   ' D0589
                                        End If
                                    End If
                                    End If
                                ' D0483 ==
                                tLock.LockStatus = tLockStatus  ' D0589
                            End If
                        End If
                        If Not fLocked Then
                            tLock.LockerUserID = -1 ' D0494
                        End If
                    End If
                End If
            End If
            Return tLock
        End Function
        ' D0481 ==

        Public Function DBParse_Workspace(ByVal tData As Dictionary(Of String, Object)) As clsWorkspace
            If Not dbExists() Then Return Nothing
            Dim tWorkspace As clsWorkspace = Nothing
            If Not tData Is Nothing Then
                If Not tData(_FLD_WORKSPACE_ID) Is Nothing AndAlso Not IsDBNull(tData(_FLD_WORKSPACE_ID)) Then
                    tWorkspace = New clsWorkspace
                    tWorkspace.ID = CInt(tData(_FLD_WORKSPACE_ID))
                    tWorkspace.GroupID = CInt(tData(_FLD_WORKSPACE_GRPID))
                    tWorkspace.ProjectID = CInt(tData(_FLD_WORKSPACE_PRJID))
                    tWorkspace.UserID = CInt(tData(_FLD_WORKSPACE_USERID))
                    tWorkspace.StatusDataLikelihood = CInt(tData(_FLD_WORKSPACE_STATUS))  ' D0491 + D1945 + D5006 + D5089
                    If tData.ContainsKey(_FLD_WORKSPACE_STATUS_IMPACT) AndAlso Not IsDBNull(tData(_FLD_WORKSPACE_STATUS_IMPACT)) Then tWorkspace.StatusDataImpact = CType(tData(_FLD_WORKSPACE_STATUS_IMPACT), ecWorkspaceStatus) ' D1945 + D5006
                    If Not IsDBNull(tData(_FLD_WORKSPACE_STEP)) Then tWorkspace.ProjectStepLikelihood = CInt(tData(_FLD_WORKSPACE_STEP)) ' D1945
                    If tData.ContainsKey(_FLD_WORKSPACE_STEP_IMPACT) AndAlso Not IsDBNull(tData(_FLD_WORKSPACE_STEP_IMPACT)) Then tWorkspace.ProjectStepImpact = CInt(tData(_FLD_WORKSPACE_STEP_IMPACT)) ' D1945
                    If Not IsDBNull(tData("Comment")) Then tWorkspace.Comment = CStr(tData("Comment"))
                    If tData.ContainsKey("Created") AndAlso Not IsDBNull(tData("Created")) Then tWorkspace.Created = CType(tData("Created"), DateTime) ' D0494
                    If tData.ContainsKey("LastModify") AndAlso Not IsDBNull(tData("LastModify")) Then tWorkspace.LastModify = CType(tData("LastModify"), DateTime) ' D0494
                    If tData.ContainsKey("TTStatus") AndAlso Not IsDBNull(tData("TTStatus")) Then tWorkspace.TeamTimeStatusLikelihood = CType(tData("TTStatus"), ecWorkspaceStatus) ' D0660 + D1945
                    If tData.ContainsKey("TTStatus2") AndAlso Not IsDBNull(tData("TTStatus2")) Then tWorkspace.TeamTimeStatusImpact = CType(tData("TTStatus2"), ecWorkspaceStatus) ' D1945
                    If tWorkspace.StatusLikelihood = ecWorkspaceStatus.wsDisabled AndAlso tWorkspace.isInTeamTime(False) Then tWorkspace.TeamTimeStatusLikelihood = ecWorkspaceStatus.wsDisabled ' D1741  + D1945
                    If tWorkspace.StatusImpact = ecWorkspaceStatus.wsDisabled AndAlso tWorkspace.isInTeamTime(True) Then tWorkspace.TeamTimeStatusImpact = ecWorkspaceStatus.wsDisabled ' D1945
                End If
            End If
            Return tWorkspace
        End Function

        ' D0795 ===
        Public Function DBParse_UserTemplate(ByVal tData As Dictionary(Of String, Object)) As clsUserTemplate
            If Not dbExists() Then Return Nothing
            Dim tUserTpl As clsUserTemplate = Nothing
            If Not tData Is Nothing Then
                If Not tData(_FLD_USERTPL_ID) Is Nothing AndAlso Not IsDBNull(tData(_FLD_USERTPL_ID)) Then
                    tUserTpl = New clsUserTemplate
                    tUserTpl.ID = CInt(tData(_FLD_USERTPL_ID))
                    tUserTpl.UserID = CInt(tData(_FLD_USERTPL_USERID))
                    If Not IsDBNull(tData("TemplateName")) Then tUserTpl.TemplateName = CStr(tData("TemplateName"))
                    If Not IsDBNull(tData("Comment")) Then tUserTpl.Comment = CStr(tData("Comment"))
                    If Not IsDBNull(tData("StructureType")) Then tUserTpl.TemplateType = CType(tData("StructureType"), StructureType)
                    'If tData.ContainsKey("XMLName") AndAlso Not IsDBNull(tData("XMLName")) Then tUserTpl.XMLName = CStr(tData("XMLName")) ' D1945
                    ' D0802 ===
                    If Not IsDBNull(tData("Stream")) Then
                        Select Case tUserTpl.TemplateType
                            Case StructureType.stPipeOptions
                                Dim MS As New MemoryStream
                                MS.Write(CType(tData("Stream"), Byte()), 0, CInt(tData("StreamSize")))
                                tUserTpl.TemplateData = New clsPipeParamaters
                                tUserTpl.TemplateData.ReadFromStream(MS)
                                Debug.WriteLine(tUserTpl.CalculateHash)
                        End Select
                    End If
                    ' D0802 ==
                    If Not IsDBNull(tData("ModifyDate")) Then tUserTpl.LastModify = CType(tData("ModifyDate"), DateTime)
                    ' D2184 ===
                    If CanvasMasterDBVersion >= "0.9994" Then
                        'If Not IsDBNull(tData("XMLName")) Then tUserTpl.XMLName = CStr(tData("XMLName"))
                        If Not IsDBNull(tData("StreamHash")) Then tUserTpl.TemplateHash = CStr(tData("StreamHash"))
                        If Not IsDBNull(tData("IsCustom")) Then tUserTpl.IsCustom = CInt(tData("IsCustom")) <> 0
                    End If
                    ' D2184 ==

                End If
            End If
            Return tUserTpl
        End Function
        ' D0795 ==

        Public Function DBParse_Session(ByVal tData As Dictionary(Of String, Object)) As clsOnlineUserSession
            If Not dbExists() Then Return Nothing
            Dim tSession As New clsOnlineUserSession
            If Not tData Is Nothing Then
                If tData.ContainsKey("UserID") AndAlso Not tData("UserID") Is Nothing Then
                    tSession.UserID = CInt(tData("UserID"))
                    If tData.ContainsKey("Email") AndAlso Not IsDBNull(tData("Email")) Then tSession.UserEmail = CStr(tData("Email"))
                    If tData.ContainsKey("LastVisited") AndAlso Not IsDBNull(tData("LastVisited")) Then tSession.LastAccess = CType(tData("LastVisited"), DateTime) ' D0501
                    If tData.ContainsKey("LastPageID") AndAlso Not IsDBNull(tData("LastPageID")) Then tSession.PageID = CInt(tData("LastPageID"))
                    If tData.ContainsKey("LastURL") AndAlso Not IsDBNull(tData("LastURL")) Then tSession.URL = CStr(tData("LastURL"))
                    If tData.ContainsKey("LastWorkgroupID") AndAlso Not IsDBNull(tData("LastWorkgroupID")) Then tSession.WorkgroupID = CInt(tData("LastWorkgroupID"))
                    If tData.ContainsKey("LastProjectID") AndAlso Not IsDBNull(tData("LastProjectID")) Then tSession.ProjectID = CInt(tData("LastProjectID"))
                    If tData.ContainsKey("SessionID") AndAlso Not IsDBNull(tData("SessionID")) Then tSession.SessionID = CStr(tData("SessionID"))
                    If tData.ContainsKey("RoleGroupID") AndAlso Not IsDBNull(tData("RoleGroupID")) Then tSession.RoleGroupID = CInt(tData("RoleGroupID")) ' D4818
                End If
            End If
            Return tSession
        End Function
        ' D0464 ==

        ' D0494 ===
        Public Function DBParse_Extra(ByVal tData As Dictionary(Of String, Object)) As clsExtra
            If Not dbExists() Then Return Nothing
            Dim tExtra As clsExtra = Nothing
            If Not tData Is Nothing Then
                If tData.ContainsKey(_FLD_EXTRA_ID) AndAlso Not tData(_FLD_EXTRA_ID) Is Nothing AndAlso Not IsDBNull(tData(_FLD_EXTRA_ID)) Then ' D0494
                    tExtra = New clsExtra
                    tExtra.ID = CInt(tData(_FLD_EXTRA_ID))
                    tExtra.ExtraType = CType(tData(_FLD_EXTRA_TYPEID), ecExtraType) ' D0491
                    tExtra.ExtraProperty = CType(tData(_FLD_EXTRA_PROPID), ecExtraProperty) ' D0491
                    tExtra.ObjectID = CInt(tData(_FLD_EXTRA_OBJECTID))
                    If Not IsDBNull(tData(_FLD_EXTRA_PROPVALUE)) Then tExtra.Value = tData(_FLD_EXTRA_PROPVALUE)
                    'If tData.ContainsKey("Changed") AndAlso Not IsDBNull(tData("Changed")) Then tExtra.LastChanged = CType(tData("Changed"), DateTime) ' D2214 - D2259
                End If
            End If
            Return tExtra
        End Function
        ' D0494 ==

        ' D3509 ===
        Public Function DBParse_Snapshot(ByVal tData As Dictionary(Of String, Object), fParseStream As Boolean) As clsSnapshot
            If Not dbExists() Then Return Nothing
            Dim tSnapshot As clsSnapshot = Nothing
            If Not tData Is Nothing Then
                If tData(_FLD_SNAPSHOTS_DT) IsNot Nothing AndAlso Not IsDBNull(tData(_FLD_SNAPSHOTS_PRJ_ID)) Then
                    tSnapshot = New clsSnapshot
                    tSnapshot.ID = CInt(tData(_FLD_SNAPSHOTS_ID))
                    tSnapshot.DateTime = CType(tData(_FLD_SNAPSHOTS_DT), DateTime)
                    tSnapshot.ProjectID = CInt(tData(_FLD_SNAPSHOTS_PRJ_ID))
                    tSnapshot.Type = CType(tData(_FLD_SNAPSHOTS_TYPE), ecSnapShotType)
                    tSnapshot.ProjectStreamMD5 = CStr(tData("StreamMD5"))
                    tSnapshot.ProjectWorkspaceMD5 = CStr(tData("WorkspaceMD5"))
                    tSnapshot.Comment = CStr(tData(_FLD_SNAPSHOTS_COMMENT))

                    ' D3729 ===
                    ' DB 0.99992
                    If tData.ContainsKey("SnapshotIdx") AndAlso tData("SnapshotIdx") IsNot Nothing AndAlso Not IsDBNull(tData("SnapshotIdx")) Then tSnapshot.Idx = CInt(tData("SnapshotIdx"))
                    If tData.ContainsKey("RestoredFrom") AndAlso tData("RestoredFrom") IsNot Nothing AndAlso Not IsDBNull(tData("RestoredFrom")) Then tSnapshot.RestoredFrom = CInt(tData("RestoredFrom")) ' D3731
                    If tData.ContainsKey("Details") AndAlso tData("Details") IsNot Nothing AndAlso Not IsDBNull(tData("Details")) Then tSnapshot.Details = CStr(tData("Details")) ' D3731
                    ' D3729 ==

                    If tData.ContainsKey("StreamBytes") AndAlso tData("StreamBytes") IsNot Nothing AndAlso Not IsDBNull(tData("StreamBytes")) Then tSnapshot.ProjectStreamSize = CInt(tData("StreamBytes")) ' D3775
                    If tData.ContainsKey("WksBytes") AndAlso tData("WksBytes") IsNot Nothing AndAlso Not IsDBNull(tData("WksBytes")) Then tSnapshot.ProjectWorkspaceSize = CInt(tData("WksBytes")) ' D3775

                    If fParseStream Then
                        If Not IsDBNull(tData(_FLD_SNAPSHOTS_STREAM)) Then
                            Dim StreamData() As Byte = CType(tData(_FLD_SNAPSHOTS_STREAM), Byte())
                            tSnapshot.ProjectStream = New MemoryStream(StreamData)
                        End If
                        tSnapshot.ProjectWorkspace = CStr(tData("Workspace"))
                    End If
                End If
            End If
            Return tSnapshot
        End Function
        ' D3509 ==

#End Region


#Region "Application Users"

        ' D0463 ===
        Public Function DBUsersAll() As List(Of clsApplicationUser)
            If Not dbExists() Then Return Nothing
            Dim tList As List(Of Dictionary(Of String, Object)) = Database.SelectFromTable(_TABLE_USERS)
            Dim tUsers As New List(Of clsApplicationUser)
            For Each tRow As Dictionary(Of String, Object) In tList
                tUsers.Add(DBParse_ApplicationUser(tRow))
            Next
            DebugInfo(String.Format("Loaded list of all users ({0})", tUsers.Count))  ' D0464
            Return tUsers
        End Function
        ' D0463 ==

        Public Function DBUserByEmail(ByVal sEmail As String) As clsApplicationUser
            If Not dbExists() Then Return Nothing
            Dim tList As List(Of Dictionary(Of String, Object)) = Database.SelectFromTable(_TABLE_USERS, _FLD_USERS_EMAIL, sEmail)
            DebugInfo(String.Format("Loaded user with e-mail '{0}'", sEmail)) ' D0464
            If tList.Count > 0 Then Return DBParse_ApplicationUser(tList(0)) Else Return Nothing
        End Function

        Public Function DBUserByID(ByVal tUserID As Integer) As clsApplicationUser
            If Not dbExists() Then Return Nothing
            Dim tList As List(Of Dictionary(Of String, Object)) = Database.SelectFromTable(_TABLE_USERS, _FLD_USERS_ID, tUserID)
            DebugInfo(String.Format("Loaded user with UserID #{0}", tUserID)) ' D0464
            If tList.Count > 0 Then Return DBParse_ApplicationUser(tList(0)) Else Return Nothing
        End Function

        ' D0483 ===
        Public Function DBUsersByWorkgroupID(ByVal tWorkgroupID As Integer) As List(Of clsApplicationUser)
            If Not dbExists() Then Return Nothing
            Dim tList As List(Of Dictionary(Of String, Object)) = Database.SelectBySQL(String.Format("SELECT U.* FROM {0} as U, {1} as UW WHERE UW.{4}={5} AND U.{2}=UW.{3} ORDER BY U.{2}", _TABLE_USERS, _TABLE_USERWORKGROUPS, _FLD_USERS_ID, _FLD_USERWRKG_USERID, _FLD_USERWRKG_WRKGID, tWorkgroupID))
            Dim tUsers As New List(Of clsApplicationUser)
            For Each tRow As Dictionary(Of String, Object) In tList
                tUsers.Add(DBParse_ApplicationUser(tRow))
            Next
            DebugInfo(String.Format("Loaded list of users for workgroup #{0}", tWorkgroupID))
            Return tUsers
        End Function

        Public Function DBUsersByProjectID(ByVal tProjectID As Integer) As List(Of clsApplicationUser)
            If Not dbExists() Then Return Nothing

            Dim tList As List(Of Dictionary(Of String, Object)) = Database.SelectBySQL(String.Format("SELECT U.* FROM {0} as U, {1} as WS WHERE WS.{4}={5} AND U.{2}=WS.{3} ORDER BY U.{2}", _TABLE_USERS, _TABLE_WORKSPACE, _FLD_USERS_ID, _FLD_WORKSPACE_USERID, _FLD_WORKSPACE_PRJID, tProjectID))
            Dim tUsers As New List(Of clsApplicationUser)

            ECCore.MiscFuncs.PrintDebugInfo("******* DBParse_ApplicationUser started - ")
            For Each tRow As Dictionary(Of String, Object) In tList
                tUsers.Add(DBParse_ApplicationUser(tRow))
            Next
            ECCore.MiscFuncs.PrintDebugInfo("******* DBParse_ApplicationUser ended - ")

            DebugInfo(String.Format("Loaded list of users for project #{0}", tProjectID))
            ECCore.MiscFuncs.PrintDebugInfo("******* DBUsersByProjectID ended - ")
            Return tUsers
        End Function
        ' D0483 ==

        Public Function DBUsersByProjectIDDictionary(ByVal tProjectID As Integer) As Dictionary(Of String, clsApplicationUser)
            If Not dbExists() Then Return Nothing

            Dim tList As List(Of Dictionary(Of String, Object)) = Database.SelectBySQL(String.Format("SELECT U.* FROM {0} as U, {1} as WS WHERE WS.{4}={5} AND U.{2}=WS.{3} ORDER BY U.{2}", _TABLE_USERS, _TABLE_WORKSPACE, _FLD_USERS_ID, _FLD_WORKSPACE_USERID, _FLD_WORKSPACE_PRJID, tProjectID))
            Dim tUsers As New Dictionary(Of String, clsApplicationUser)

            ECCore.MiscFuncs.PrintDebugInfo("******* DBParse_ApplicationUser started - ")
            For Each tRow As Dictionary(Of String, Object) In tList
                Dim u As clsApplicationUser = DBParse_ApplicationUser(tRow)
                tUsers.Add(u.UserEmail.ToLower, u)  ' D6496
            Next
            ECCore.MiscFuncs.PrintDebugInfo("******* DBParse_ApplicationUser ended - ")

            DebugInfo(String.Format("Loaded list of users for project #{0}", tProjectID))
            ECCore.MiscFuncs.PrintDebugInfo("******* DBUsersByProjectID ended - ")
            Return tUsers
        End Function

        ' D6087 ===
        Public Function DBUserUnlock(ByRef tUser As clsApplicationUser, Optional sNewPassword As String = Nothing, Optional ByVal sUnlockMessage As String = "") As Boolean
            If tUser IsNot Nothing Then
                Dim fResetStatus As Boolean = False
                If tUser.PasswordStatus <> 0 Then   ' D6377
                    tUser.PasswordStatus = 0
                    fResetStatus = True
                End If
                If sNewPassword IsNot Nothing AndAlso (tUser.UserPassword <> sNewPassword OrElse tUser.PasswordStatus < 0) Then ' D6221
                    tUser.UserPassword = sNewPassword
                    tUser.PasswordStatus = 0    ' D6221
                    If fResetStatus Then DBSaveLog(dbActionType.actUnLock, dbObjectType.einfUser, tUser.UserID, If(sUnlockMessage = "", "Unlock the user when creating a new password", sUnlockMessage), GetClientIP())
                    Return DBUserUpdate(tUser, False, "Create user password")
                End If
                If fResetStatus Then
                    Dim tParams As New List(Of Object)
                    tParams.Add(tUser.UserEmail)
                    Database.ExecuteSQL(String.Format("UPDATE {0} SET PasswordStatus={2} WHERE {1}=?", clsComparionCore._TABLE_USERS, clsComparionCore._FLD_USERS_EMAIL, tUser.PasswordStatus), tParams)
                    DBSaveLog(dbActionType.actUnLock, dbObjectType.einfUser, tUser.UserID, If(sUnlockMessage = "", "Unlock user", sUnlockMessage), GetClientIP())
                    Return True
                End If
            End If
            Return False
        End Function
        ' D6087 ==

        ' D0473 ===
        Public Function DBUserUpdate(ByVal tUser As clsApplicationUser, ByVal fAddAsNew As Boolean, Optional ByVal sLogMessage As String = "") As Boolean
            If Not dbExists() Then Return Nothing
            Dim fUpdated As Boolean = False
            If tUser Is Nothing Then Return fUpdated
            ' -D0828
            'If fAddAsNew Then
            '    Dim tmpUser As clsApplicationUser = DBUserByEmail(tUser.UserEmail)
            '    If Not tmpUser Is Nothing Then
            '        tUser.UserID = tmpUser.UserID
            '        If tUser.OwnerID = 0 Then tUser.OwnerID = tmpUser.OwnerID
            '        fAddAsNew = False
            '    End If
            'End If
            If tUser.OwnerID = 0 And Not ActiveUser Is Nothing Then tUser.OwnerID = ActiveUser.UserID
            If tUser.CannotBeDeleted AndAlso tUser.Status <> ecUserStatus.usEnabled Then tUser.Status = ecUserStatus.usEnabled ' D0897

            ' D2213 ===
            Dim sInsertFld As String = ""
            Dim sInsertVal As String = ""
            Dim sUpdateStr As String = ""
            If CanvasMasterDBVersion >= "0.9996" Then
                sInsertFld = ", PasswordStatus"
                sInsertVal = ", ?"
                sUpdateStr = ", PasswordStatus=?"
            End If
            ' D2213 ==

            Dim SQL As String = CStr(IIf(fAddAsNew, String.Format("INSERT INTO Users (OwnerID, DefaultWGID, Email, Password, FullName, Comment, Status, Created{0}) VALUES (?, ?, ?, ?, ?, ?, ?, ?{1})", sInsertFld, sInsertVal),
                                                    String.Format("UPDATE Users SET OwnerID=?, DefaultWGID=?, Email=?, Password=?, FullName=?, Comment=?, Status=?, LastModify=?{0} WHERE ID=?", sUpdateStr)))    ' D0473 + D2213

            Try ' D0828
                Dim tParams As New List(Of Object)
                tParams.Add(tUser.OwnerID)
                tParams.Add(tUser.DefaultWorkgroupID)
                tParams.Add(tUser.UserEmail.Trim)
                tParams.Add(EncodeString(tUser.UserPassword, Nothing, True))    ' D0826
                tParams.Add(tUser.UserName.Trim)
                tParams.Add(tUser.Comment)
                tParams.Add(CInt(tUser.DBStatus))
                tParams.Add(Now)
                If CanvasMasterDBVersion >= "0.9996" Then tParams.Add(tUser.PasswordStatus) ' D2215
                tParams.Add(tUser.UserID)
                DebugInfo(String.Format("{0} User '{1}'", CStr(IIf(fAddAsNew, "Add new", "Update")), tUser.UserEmail))  ' D0830
                fUpdated = Database.ExecuteSQL(SQL, tParams) > 0
                If fUpdated Then
                    If fAddAsNew Then tUser.UserID = clsDatabaseAdvanced.GetLastIdentity(Database) Else HashCodesReset()  ' D6224 + D7114 // for reset hashes when someone can change his psw
                    If sLogMessage IsNot Nothing Then DBSaveLog(CType(IIf(fAddAsNew, dbActionType.actCreate, dbActionType.actModify), dbActionType), dbObjectType.einfUser, tUser.UserID, sLogMessage, "") ' D0499 + D0830
                End If
                ' D0828 ===
            Catch ex As Exception
                If fAddAsNew Then
                    Dim tmpUser As clsApplicationUser = DBUserByEmail(tUser.UserEmail)
                    If Not tmpUser Is Nothing Then
                        tUser.UserID = tmpUser.UserID
                        If tUser.OwnerID = 0 Then tUser.OwnerID = tmpUser.OwnerID
                        Return DBUserUpdate(tUser, False, sLogMessage)
                    End If
                End If
            End Try
            ' D0828 ==
            Return fUpdated
        End Function
        ' D0473 ==

        ' D0508 ===
        Public Function DBUserDelete(ByVal tUser As clsApplicationUser) As Boolean
            If Not dbExists() Then Return Nothing
            Dim fResult As Boolean = False
            If Not tUser Is Nothing Then
                Dim Tr As DbTransaction = Nothing    ' D0041
                Try
                    DebugInfo(String.Format("Delete User '{0}'", tUser.UserEmail))

                    Dim sSQL As String = ""
                    Dim oCommand As DbCommand = Database.SQL(sSQL)
                    Tr = oCommand.Connection.BeginTransaction
                    oCommand.Transaction = Tr

                    sSQL = String.Format("DELETE FROM {0} WHERE {1}={2}", _TABLE_WORKSPACE, _FLD_WORKSPACE_USERID, tUser.UserID)
                    oCommand.CommandText = sSQL
                    DBExecuteNonQuery(Database.ProviderType, oCommand)

                    sSQL = String.Format("DELETE FROM {0} WHERE {1}={2}", _TABLE_USERWORKGROUPS, _FLD_USERWRKG_USERID, tUser.UserID)    ' D7209
                    oCommand.CommandText = sSQL
                    DBExecuteNonQuery(Database.ProviderType, oCommand)

                    sSQL = String.Format("DELETE FROM {0} WHERE {1}={2}", _TABLE_USERS, _FLD_USERS_ID, tUser.UserID)
                    oCommand.CommandText = sSQL
                    DBExecuteNonQuery(Database.ProviderType, oCommand)

                    Tr.Commit()
                    oCommand = Nothing

                    DBTinyURLDelete(-1, -1, tUser.UserID)   ' D0899
                    fResult = True

                Catch ex As Exception
                    DBSaveLog(dbActionType.actDelete, dbObjectType.einfUser, tUser.UserID, "Delete user", "Error: " + ex.Message)
                    If Not Tr Is Nothing Then Tr.Rollback()
                    fResult = False
                Finally
                    If Not Tr Is Nothing Then Tr = Nothing
                    DBSaveLog(dbActionType.actDelete, dbObjectType.einfUser, tUser.UserID, "Delete user", "")
                End Try
            End If

            Return fResult
        End Function
        ' D0508 ==

#End Region


#Region "Workgroups"

        ' D4617 ===
        Private Function DBWorkgroupsParseList(tWkgList As List(Of Dictionary(Of String, Object)), fReadRoleGroups As Boolean, fReadRoleActions As Boolean) As List(Of clsWorkgroup)
            If Not dbExists() Then Return Nothing
            Dim tWorkgroups As New List(Of clsWorkgroup)
            If tWkgList IsNot Nothing Then
                Dim tGroups As List(Of clsRoleGroup) = Nothing
                If fReadRoleGroups Then
                    Dim tList As List(Of Dictionary(Of String, Object)) = Database.SelectFromTable(_TABLE_ROLEGROUPS)
                    tGroups = New List(Of clsRoleGroup)
                    For Each tRow As Dictionary(Of String, Object) In tList
                        Dim tRoleGroup As clsRoleGroup = DBParse_RoleGroup(tRow)
                        If tRoleGroup IsNot Nothing Then    ' D2781
                            tGroups.Add(tRoleGroup)
                        End If
                    Next
                End If
                If fReadRoleActions And fReadRoleGroups AndAlso tGroups IsNot Nothing AndAlso tGroups.Count > 0 Then
                    Dim tList As List(Of Dictionary(Of String, Object)) = Database.SelectFromTable(_TABLE_ROLEACTIONS)
                    For Each tRow As Dictionary(Of String, Object) In tList
                        Dim tRoleAction As clsRoleAction = DBParse_RoleAction(tRow)
                        If tRoleAction IsNot Nothing Then
                            Dim tGrp As clsRoleGroup = tGroups.Find(Function(g) g.ID = tRoleAction.RoleGroupID)
                            If tGrp IsNot Nothing Then tGrp.Actions.Add(tRoleAction)
                        End If
                    Next
                End If
                For Each tRow As Dictionary(Of String, Object) In tWkgList
                    Dim tWorkgroup As clsWorkgroup = DBParse_Workgroup(tRow)
                    If fReadRoleGroups AndAlso tGroups IsNot Nothing AndAlso tGroups.Count > 0 Then tWorkgroup.RoleGroups.AddRange(tGroups.FindAll(Function(g) g.WorkgroupID = tWorkgroup.ID))
                    tWorkgroups.Add(tWorkgroup)
                Next
                DebugInfo(String.Format("Loaded list of workgroups ({0})", tWorkgroups.Count))
            End If
            Return tWorkgroups
        End Function
        ' D4617 ==

        ' D0463 ===
        Public Function DBWorkgroupsAll(Optional ByVal fReadRoleGroups As Boolean = True, Optional ByVal fReadRoleActions As Boolean = True) As List(Of clsWorkgroup)   ' D0466
            Dim tList As List(Of Dictionary(Of String, Object)) = Database.SelectBySQL(String.Format("SELECT * FROM {0} ORDER BY {1}", _TABLE_WORKGROUPS, _FLD_WORKGROUPS_NAME)) ' D0465
            Return DBWorkgroupsParseList(tList, fReadRoleGroups, fReadRoleActions) ' D4617
        End Function

        Public Function DBWorkgroupByID(ByVal tID As Integer, Optional ByVal fReadRoleGroups As Boolean = True, Optional ByVal fReadRoleActions As Boolean = True) As clsWorkgroup  ' D0466
            If Not dbExists() Then Return Nothing
            Dim tList As List(Of Dictionary(Of String, Object)) = Database.SelectFromTable(_TABLE_WORKGROUPS, _FLD_WORKGROUPS_ID, tID)
            If tList.Count > 0 Then
                ' D0464 ===
                Dim tWorkgroup As clsWorkgroup = DBParse_Workgroup(tList(0))
                If fReadRoleGroups Then tWorkgroup.RoleGroups = DBRoleGroupsByWorkgroupID(tWorkgroup.ID, fReadRoleActions)
                DebugInfo(String.Format("Loaded workgroup by ID #{0}", tID))
                Return tWorkgroup
                ' D0464 ==
            Else
                Return Nothing
            End If
        End Function
        ' D0463 ==

        ' D0464 ===
        Public Function DBWorkgroupsByStatus(ByVal tStatus As ecWorkgroupStatus, Optional ByVal fReadRoleGroups As Boolean = True, Optional ByVal fReadRoleActions As Boolean = True) As List(Of clsWorkgroup)  ' D0466
            If Not isCanvasMasterDBValid Then Return New List(Of clsWorkgroup) ' D0540
            If Not dbExists() Then Return Nothing
            Dim tList As List(Of Dictionary(Of String, Object)) = Database.SelectFromTable(_TABLE_WORKGROUPS, _FLD_WORKGROUPS_STATUS, CInt(tStatus))
            Dim tWkgs As New List(Of clsWorkgroup)
            If tList.Count = 1 Then
                Dim tWorkgroup As clsWorkgroup = DBParse_Workgroup(tList(0))
                If fReadRoleGroups Then tWorkgroup.RoleGroups = DBRoleGroupsByWorkgroupID(tWorkgroup.ID, fReadRoleActions)
                tWkgs.Add(tWorkgroup)
            Else
                tWkgs = DBWorkgroupsParseList(tList, fReadRoleGroups, fReadRoleActions) ' D4617
            End If
            Return tWkgs
        End Function
        ' D0464 ==

        ' D0584 ===
        Public Function DBWorkgroupByName(ByVal sName As String, Optional ByVal fReadRoleGroups As Boolean = True, Optional ByVal fReadRoleActions As Boolean = True) As clsWorkgroup
            If Not dbExists() Then Return Nothing
            Dim tList As List(Of Dictionary(Of String, Object)) = Database.SelectFromTable(_TABLE_WORKGROUPS, _FLD_WORKGROUPS_NAME, sName)
            If tList.Count > 0 Then
                Dim tWorkgroup As clsWorkgroup = DBParse_Workgroup(tList(0))
                If fReadRoleGroups Then tWorkgroup.RoleGroups = DBRoleGroupsByWorkgroupID(tWorkgroup.ID, fReadRoleActions)
                DebugInfo(String.Format("Loaded workgroup by name '{0}'", sName))
                Return tWorkgroup
            End If
            Return Nothing
        End Function
        ' D0584 ==

        ' D0475 ===
        Public Function DBWorkgroupUpdate(ByRef tWorkGroup As clsWorkgroup, Optional ByVal AsNewRecord As Boolean = False, Optional ByVal sLogMessage As String = "", Optional fDoExtractLicenseParameters As Boolean = False) As Boolean
            If Not dbExists() Then Return Nothing
            Dim fUpdated As Boolean = False
            If Database.Connect Then
                If tWorkGroup.OwnerID = 0 And Not ActiveUser Is Nothing Then tWorkGroup.OwnerID = ActiveUser.UserID
                Dim SQL As String = CStr(IIf(AsNewRecord, "INSERT INTO Workgroups (OwnerID, ECAMID, Name, Status, LicenseData, LicenseKey, Comment, Created" + CStr(IIf(CanvasMasterDBVersion >= "0.992", ", EULAFile, LifetimeProjects", "")) + CStr(IIf(CanvasMasterDBVersion >= "0.9997", ", WordingTemplates", "")) + CStr(IIf(CanvasMasterDBVersion >= "0.9998", ", OpportunityID", "")) + ") VALUES (?, ?, ?, ?, ?, ?, ?, ?" + CStr(IIf(CanvasMasterDBVersion >= "0.992", ", ?, ?", "")) + CStr(IIf(CanvasMasterDBVersion >= "0.9997", ", ?", "")) + CStr(IIf(CanvasMasterDBVersion >= "0.9998", ", ?", "")) + ")",
                                                          "UPDATE Workgroups SET OwnerID=?, ECAMID=?, Name=?, Status=?, LicenseData=?, LicenseKey=?, Comment=?, LastModify=?" + CStr(IIf(CanvasMasterDBVersion >= "0.992", ", EULAFile=?, LifetimeProjects=?", "")) + CStr(IIf(CanvasMasterDBVersion >= "0.9997", ", WordingTemplates=?", "")) + CStr(IIf(CanvasMasterDBVersion >= "0.9998", ", OpportunityID=?", "")) + " WHERE ID=?"))    ' D2384 + D3333
                Dim tParams As New List(Of Object)
                tParams.Add(tWorkGroup.OwnerID)
                tParams.Add(tWorkGroup.ECAMID)
                tParams.Add(tWorkGroup.Name)
                tParams.Add(CInt(tWorkGroup.Status))
                Dim tLicenseContent() As Byte = {}
                Dim sLicenseKey As String = ""
                If Not tWorkGroup.License Is Nothing And Options.CheckLicense Then
                    If Not tWorkGroup.License.LicenseContent Is Nothing Then
                        Dim LicLength As Integer = CInt(tWorkGroup.License.LicenseContent.Length)
                        Array.Resize(tLicenseContent, LicLength)
                        tWorkGroup.License.LicenseContent.Seek(0, SeekOrigin.Begin)
                        tWorkGroup.License.LicenseContent.Read(tLicenseContent, 0, LicLength)
                        sLicenseKey = tWorkGroup.License.LicenseKey
                    End If
                End If
                tParams.Add(tLicenseContent)
                tParams.Add(sLicenseKey)
                tParams.Add(tWorkGroup.Comment)
                tParams.Add(Now)
                ' D0922 ==
                If CanvasMasterDBVersion >= "0.992" Then
                    tParams.Add(tWorkGroup.EULAFile)
                    tParams.Add(tWorkGroup.LifeTimeProjects)
                End If
                ' D0922 ==
                ' D2384 ===
                If CanvasMasterDBVersion >= "0.9997" Then
                    Dim tWording() As Byte = {}
                    Dim fs As New IO.MemoryStream
                    Dim formatter As New BinaryFormatter
                    Try
                        formatter.Serialize(fs, tWorkGroup.WordingTemplates)
                        Array.Resize(tWording, CInt(fs.Length))
                        fs.Seek(0, SeekOrigin.Begin)
                        fs.Read(tWording, 0, CInt(fs.Length))
                    Catch ex As Exception
                        DebugInfo("Error on serialize workgroup wording: " + ex.Message, _TRACE_RTE)
                    End Try
                    tParams.Add(tWording)
                End If
                ' D2384 ==
                If CanvasMasterDBVersion >= "0.9998" Then tParams.Add(tWorkGroup.OpportunityID) ' D3333
                If Not AsNewRecord Then tParams.Add(tWorkGroup.ID)
                DebugInfo(String.Format("{0} workgroup '{1}'", CStr(IIf(AsNewRecord, "Add new", "Update")), tWorkGroup.ID))
                fUpdated = Database.ExecuteSQL(SQL, tParams) > 0
                If fUpdated Then
                    If AsNewRecord Then tWorkGroup.ID = clsDatabaseAdvanced.GetLastIdentity(Database)
                    DBSaveLog(CType(IIf(AsNewRecord, dbActionType.actCreate, dbActionType.actModify), dbActionType), dbObjectType.einfWorkgroup, tWorkGroup.ID, sLogMessage, "") ' D0499
                    If tWorkGroup.Status = ecWorkgroupStatus.wsSystem AndAlso SystemWorkgroup IsNot Nothing Then SystemWorkgroup = Nothing ' D0612
                    If AsNewRecord OrElse fDoExtractLicenseParameters Then DBWorkgroupExtractLicenseParameters(tWorkGroup) ' D2212
                    If AsNewRecord Then DBWorkgroupUpdateLifetimeProjects(tWorkGroup, 0)    ' D4564
                End If
            End If
            Return fUpdated
        End Function
        ' D0475 ==

        ' D0502 ===
        Public Function DBWorkgroupDelete(ByVal tWorkgroup As clsWorkgroup) As Boolean
            If Not dbExists() Then Return Nothing
            Dim fResult As Boolean = False
            ' D0508 ===
            If Not tWorkgroup Is Nothing Then
                Dim Tr As DbTransaction = Nothing    ' D0041
                Try
                    DebugInfo(String.Format("Delete Workgroup '{0}'", tWorkgroup.Name))

                    Dim PrjList As List(Of clsProject) = DBProjectsByWorkgroupID(tWorkgroup.ID)
                    Dim sPrjList As String = ""
                    For Each tPrj As clsProject In PrjList
                        sPrjList += CStr(IIf(sPrjList = "", "", ",")) + CStr(tPrj.ID)
                    Next

                    If tWorkgroup.RoleGroups.Count = 0 Then tWorkgroup.RoleGroups = DBRoleGroupsByWorkgroupID(tWorkgroup.ID)
                    Dim sGroups As String = ""
                    For Each tGrp As clsRoleGroup In tWorkgroup.RoleGroups
                        sGroups += CStr(IIf(sGroups = "", "", ",")) + CStr(tGrp.ID)
                    Next

                    '-D6446
                    'Dim sSurveys As String = ""
                    'For Each tSrv As clsSurveyInfo In SurveysManager.LoadSurveyList(tWorkgroup.ID)
                    '    sSurveys += CStr(IIf(sSurveys = "", "", ",")) + CStr(tSrv.ID)
                    'Next

                    Dim sSQL As String = ""
                    Dim oCommand As DbCommand = Database.SQL(sSQL)
                    Tr = oCommand.Connection.BeginTransaction
                    oCommand.Transaction = Tr

                    If sPrjList <> "" Then
                        sSQL = String.Format("DELETE FROM {0} WHERE {1} IN ({2})", _TABLE_WORKSPACE, _FLD_WORKSPACE_PRJID, sPrjList)
                        oCommand.CommandText = sSQL
                        DBExecuteNonQuery(Database.ProviderType, oCommand)

                        sSQL = String.Format("DELETE FROM {0} WHERE {1} IN ({2}) AND {3} IN ({4},{5})", _TABLE_EXTRA, _FLD_EXTRA_OBJECTID, sPrjList, _FLD_EXTRA_TYPEID, CInt(ecExtraType.Project), CInt(ecExtraType.TeamTime))
                        oCommand.CommandText = sSQL
                        DBExecuteNonQuery(Database.ProviderType, oCommand)

                        sSQL = String.Format("DELETE FROM ModelStructure WHERE ProjectID IN ({0})", sPrjList)
                        oCommand.CommandText = sSQL
                        DBExecuteNonQuery(Database.ProviderType, oCommand)

                        sSQL = String.Format("DELETE FROM UserData WHERE ProjectID IN ({0})", sPrjList)
                        oCommand.CommandText = sSQL
                        DBExecuteNonQuery(Database.ProviderType, oCommand)

                        sSQL = String.Format("DELETE FROM {0} WHERE {1}={2}", _TABLE_PROJECTS, _FLD_PROJECTS_WRKGID, tWorkgroup.ID)
                        oCommand.CommandText = sSQL
                        DBExecuteNonQuery(Database.ProviderType, oCommand)

                        ' D0899 ===
                        If CanvasMasterDBVersion >= "0.991" Then
                            sSQL = String.Format("DELETE FROM PrivateURLs WHERE ProjectID IN ({0})", sPrjList)
                            oCommand.CommandText = sSQL
                            If DBExecuteNonQuery(Database.ProviderType, oCommand) > 0 Then HashCodesReset()    ' D4936 + D7114
                        End If
                        ' D0899 =
                    End If

                    sSQL = String.Format("DELETE FROM {0} WHERE {1}={2}", _TABLE_USERWORKGROUPS, _FLD_USERWRKG_WRKGID, tWorkgroup.ID)
                    oCommand.CommandText = sSQL
                    DBExecuteNonQuery(Database.ProviderType, oCommand)

                    If sGroups <> "" Then
                        sSQL = String.Format("DELETE FROM {0} WHERE {1} IN ({2})", _TABLE_ROLEACTIONS, _FLD_ROLEACTIONS_GRPID, sGroups)
                        oCommand.CommandText = sSQL
                        DBExecuteNonQuery(Database.ProviderType, oCommand)

                        sSQL = String.Format("DELETE FROM {0} WHERE {1}={2}", _TABLE_ROLEGROUPS, _FLD_ROLEGROUPS_WRKGID, tWorkgroup.ID)
                        oCommand.CommandText = sSQL
                        DBExecuteNonQuery(Database.ProviderType, oCommand)
                    End If

                    ' -D6446 ===
                    'If sSurveys <> "" Then
                    '    sSQL = String.Format("DELETE FROM SurveyStructure WHERE SurveyID IN ({0})", sSurveys)
                    '    oCommand.CommandText = sSQL
                    '    DBExecuteNonQuery(Database.ProviderType, oCommand)

                    '    sSQL = String.Format("DELETE FROM Surveys WHERE WorkgroupID={0}", tWorkgroup.ID)
                    '    oCommand.CommandText = sSQL
                    '    DBExecuteNonQuery(Database.ProviderType, oCommand)
                    'End If
                    ' -D6446 ==

                    ' D2212 ===
                    If CanvasMasterDBVersion >= "0.9995" Then
                        sSQL = String.Format("DELETE FROM {0} WHERE {1}={2}", _TABLE_WKGPARAMS, _FLD_WKGPARAMS_WKGID, tWorkgroup.ID)
                        oCommand.CommandText = sSQL
                        DBExecuteNonQuery(Database.ProviderType, oCommand)
                    End If
                    ' D2212 ==

                    ' D6446 ===
                    sSQL = String.Format("DELETE FROM {0} WHERE {1}={2}", _TABLE_EXTRA, _FLD_EXTRA_OBJECTID, tWorkgroup.ID)
                    oCommand.CommandText = sSQL
                    DBExecuteNonQuery(Database.ProviderType, oCommand)
                    ' D6446 ==

                    sSQL = String.Format("DELETE FROM {0} WHERE {1}={2}", _TABLE_WORKGROUPS, _FLD_WORKGROUPS_ID, tWorkgroup.ID)
                    oCommand.CommandText = sSQL
                    DBExecuteNonQuery(Database.ProviderType, oCommand)

                    Tr.Commit()
                    oCommand = Nothing
                    fResult = True

                Catch ex As Exception
                    DBSaveLog(dbActionType.actDelete, dbObjectType.einfWorkgroup, tWorkgroup.ID, String.Format("Delete Workgroup '{0}'", tWorkgroup.Name), "Error: " + ex.Message)  ' D0588
                    If Not Tr Is Nothing Then Tr.Rollback()
                    fResult = False
                Finally
                    If Not Tr Is Nothing Then Tr = Nothing
                    DBSaveLog(dbActionType.actDelete, dbObjectType.einfWorkgroup, tWorkgroup.ID, String.Format("Delete Workgroup '{0}'", tWorkgroup.Name), "")  ' D0588
                End Try
            End If
            ' D0508 ==

            Return fResult
        End Function
        ' D0502 ==

        ' D2212 ===
        Public Function DBWorkgroupExtractLicenseParameters(tWorkgroup As clsWorkgroup) As Boolean
            If Not dbExists() Then Return Nothing
            Dim fResult As Boolean = False
            If tWorkgroup IsNot Nothing AndAlso CanvasMasterDBVersion >= "0.9995" Then
                Dim sSQL As String = String.Format("DELETE FROM {0} WHERE {1}={2}" + vbCrLf + vbCrLf + vbCrLf, _TABLE_WKGPARAMS, _FLD_WKGPARAMS_WKGID, tWorkgroup.ID)   ' D3911
                If tWorkgroup.License IsNot Nothing AndAlso tWorkgroup.License.isValidLicense Then
                    For Each tLic As clsLicenseParameter In tWorkgroup.License.Parameters
                        ' D3911 ===
                        Dim tVal As Long = tWorkgroup.License.GetParameterMax(tLic)
                        ' D3946 ===
                        Dim Val As Object = tVal
                        If tLic.ID = ecLicenseParameter.ExpirationDate Then Val = BinaryStr2DateTime(tVal.ToString)
                        Dim sVal As String = String.Format(CStr(IIf(tLic.ID = ecLicenseParameter.ExpirationDate, "CAST(CAST('{0}' AS datetime) as float)", "{0}")), Val)
                        ' D3946 ==
                        sSQL += String.Format("INSERT INTO {0} ({1},{2},ParameterValue) VALUES ({3},{4},{5})" + vbCrLf, _TABLE_WKGPARAMS, _FLD_WKGPARAMS_WKGID, _FLD_WKGPARAMS_PARAMID, tWorkgroup.ID, CInt(tLic.ID), sVal)
                    Next
                    Try
                        fResult = Database.ExecuteSQL(sSQL) > 0
                    Catch ex As Exception
                    End Try
                    ' D3911 ==
                    If fResult Then DBSaveLog(dbActionType.actCreate, dbObjectType.einfWorkgroupParameters, tWorkgroup.ID, String.Format("Extract license parameters for '{0}'", tWorkgroup.Name), "Extracted: " + tWorkgroup.License.Parameters.Count.ToString, -1, tWorkgroup.ID) ' D2290
                End If
            End If

            Return fResult
        End Function
        ' D2212 ==

        ' D4564 ===
        Friend Function DBWorkgroupUpdateLifetimeProjects(tWkg As clsWorkgroup, tCnt As Integer) As Boolean
            Dim fUpdated As Boolean = False
            If tWkg IsNot Nothing Then
                fUpdated = Not IsDBNull(Database.ExecuteScalarSQL(String.Format("UPDATE {0} SET LifetimeProjects={1} WHERE {2}={3}", _TABLE_WORKGROUPS, tCnt, _FLD_WORKGROUPS_ID, tWkg.ID)))
                tWkg.LifeTimeProjects = tCnt
                Dim EncValue As String = EncodeString(CStr(tCnt Xor tWkg.ID), Nothing, False)
                DBExtraWrite(clsExtra.Params2Extra(tWkg.ID, ecExtraType.Workgroup, ecExtraProperty.LifetimeProjects, EncValue))
            End If
            Return fUpdated
        End Function
        ' D4564 ==

#End Region


#Region "Role Groups"

        ' D0463 ===
        Public Function DBRoleGroupsByWorkgroupID(ByVal tWorkgroupID As Integer, Optional ByVal fReadRoleActions As Boolean = True) As List(Of clsRoleGroup)    ' D0466
            If Not dbExists() Then Return Nothing
            Dim tList As List(Of Dictionary(Of String, Object)) = Database.SelectFromTable(_TABLE_ROLEGROUPS, _FLD_ROLEGROUPS_WRKGID, tWorkgroupID)
            Dim tGroups As New List(Of clsRoleGroup)
            For Each tRow As Dictionary(Of String, Object) In tList
                Dim tRoleGroup As clsRoleGroup = DBParse_RoleGroup(tRow)
                If tRoleGroup IsNot Nothing Then    ' D2781
                    If fReadRoleActions Then tRoleGroup.Actions = DBRoleActionsByRoleGroupID(tRoleGroup.ID) ' D0464
                    tGroups.Add(tRoleGroup)
                End If
            Next
            DebugInfo(String.Format("Loaded list of Role Groups for Workgroup #{0}", tWorkgroupID))
            Return tGroups
        End Function

        Public Function DBRoleGroupByID(ByVal tID As Integer, Optional ByVal fReadRoleActions As Boolean = True) As clsRoleGroup    ' D0466
            If Not dbExists() Then Return Nothing
            Dim tList As List(Of Dictionary(Of String, Object)) = Database.SelectFromTable(_TABLE_ROLEGROUPS, _FLD_ROLEGROUPS_ID, tID)
            If tList.Count > 0 Then
                ' D0464 ===
                Dim tRoleGroup As clsRoleGroup = DBParse_RoleGroup(tList(0))
                If tRoleGroup IsNot Nothing Then    ' D2780
                    If fReadRoleActions Then tRoleGroup.Actions = DBRoleActionsByRoleGroupID(tRoleGroup.ID) ' D0464
                    DebugInfo(String.Format("Loaded Role Group with ID #{0}", tID))
                End If
                Return tRoleGroup
            Else
                ' D0464 ==
                Return Nothing
            End If
        End Function

        Public Function DBRoleGroupsByParentID(ByVal tParentID As Integer, Optional ByVal fReadRoleActions As Boolean = True) As clsRoleGroup   ' D0466
            If Not dbExists() Then Return Nothing
            Dim tList As List(Of Dictionary(Of String, Object)) = Database.SelectFromTable(_TABLE_ROLEGROUPS, _FLD_ROLEGROUPS_PID, tParentID)
            If tList.Count > 0 Then
                ' D0464 ===
                Dim tRoleGroup As clsRoleGroup = DBParse_RoleGroup(tList(0))
                If tRoleGroup IsNot Nothing Then    ' D2781
                    If fReadRoleActions Then tRoleGroup.Actions = DBRoleActionsByRoleGroupID(tRoleGroup.ID) ' D0464
                    DebugInfo(String.Format("Loaded Role Groups with ParentID #{0}", tParentID))
                End If
                Return tRoleGroup
            Else
                ' D0464 ==
                Return Nothing
            End If
        End Function
        ' D0463 ==

        ' D0475 ===
        Public Function DBRoleGroupUpdate(ByRef tGroup As clsRoleGroup, Optional ByVal AsNewRecord As Boolean = False, Optional ByVal sLogMessage As String = "") As Boolean   ' D0046 + D0065
            If Not dbExists() Then Return Nothing
            Dim fUpdated As Boolean = False
            If Database.Connect Then
                Dim SQL As String = CStr(IIf(AsNewRecord, "INSERT INTO RoleGroups (WorkgroupID, RoleLevel, GroupType, Name, Status, Comment, Created) VALUES (?, ?, ?, ?, ?, ?, ?)",
                                                          "UPDATE RoleGroups SET WorkgroupID=?, RoleLevel=?, GroupType=?, Name=?, Status=?, Comment=?, LastModify=? WHERE ID=?"))
                Dim tParams As New List(Of Object)
                tParams.Add(tGroup.WorkgroupID)
                tParams.Add(CInt(tGroup.RoleLevel))
                tParams.Add(CInt(tGroup.GroupType))
                tParams.Add(tGroup.Name)
                tParams.Add(CInt(tGroup.Status))
                tParams.Add(tGroup.Comment)
                tParams.Add(Now)
                If Not AsNewRecord Then tParams.Add(tGroup.ID)
                DebugInfo(String.Format("{0} role group '{1}'", CStr(IIf(AsNewRecord, "Add new", "Update")), tGroup.ID))
                fUpdated = Database.ExecuteSQL(SQL, tParams) > 0
                If fUpdated Then
                    If AsNewRecord Then tGroup.ID = clsDatabaseAdvanced.GetLastIdentity(Database) ' D0458
                    DBSaveLog(CType(IIf(AsNewRecord, dbActionType.actCreate, dbActionType.actModify), dbActionType), dbObjectType.einfRoleGroup, tGroup.ID, sLogMessage, "") ' D0499
                End If
            End If
            Return fUpdated
        End Function
        ' D0475 ==

        ' D0476 ===
        Public Function DBRoleGroupDelete(ByVal tGroup As clsRoleGroup, Optional ByVal sLogMessage As String = "") As Boolean
            If Not dbExists() Then Return Nothing
            Dim fResult As Boolean = False  ' D0496
            If Database.Connect AndAlso Not tGroup Is Nothing Then
                Dim SQL As String = String.Format("DELETE FROM {0} WHERE {1}={2}", _TABLE_ROLEGROUPS, _FLD_ROLEGROUPS_ID, tGroup.ID)
                If Database.ExecuteSQL(SQL) > 0 Then
                    SQL = String.Format("DELETE FROM {0} WHERE {1}={2}", _TABLE_ROLEACTIONS, _FLD_ROLEACTIONS_GRPID, tGroup.ID)
                    fResult = Database.ExecuteSQL(SQL) > 0  ' D0496
                End If
                If sLogMessage = "" Then sLogMessage = tGroup.Name
                DBSaveLog(dbActionType.actDelete, dbObjectType.einfRoleGroup, tGroup.ID, sLogMessage, "")   ' D0496
            End If
            Return fResult  ' D0496
        End Function
        ' D0476 ==

#End Region


#Region "Role Actions"

        ' D0463 ===
        Public Function DBRoleActionsByRoleGroupID(ByVal tRoleGroupID As Integer) As List(Of clsRoleAction)
            If Not dbExists() Then Return Nothing
            Dim tList As List(Of Dictionary(Of String, Object)) = Database.SelectFromTable(_TABLE_ROLEACTIONS, _FLD_ROLEACTIONS_GRPID, tRoleGroupID)
            Dim tActions As New List(Of clsRoleAction)
            For Each tRow As Dictionary(Of String, Object) In tList
                Dim tRoleAction As clsRoleAction = DBParse_RoleAction(tRow)
                If tRoleAction IsNot Nothing Then tActions.Add(tRoleAction) ' D2779
            Next
            DebugInfo(String.Format("Loaded list of Role Actions for RoleGroupID #{0}", tRoleGroupID))
            Return tActions
        End Function

        'Public Function DBRoleActionByID(ByVal tID As Integer) As clsRoleAction
        '    Dim tList As List(Of Dictionary(Of String, Object)) = Database.SelectFromTable(_TABLE_ROLEACTIONS, _FLD_ROLEACTIONS_ID, tID)
        '    DebugInfo(String.Format("Loaded Role Action with ID #{0}", tID))
        '    If tList.Count > 0 Then Return DBParse_RoleAction(tList(0)) Else Return Nothing
        'End Function
        ' D0463 ==

        ' D0475
        Public Function DBRoleActionUpdate(ByRef tAction As clsRoleAction, Optional ByVal AsNewRecord As Boolean = False, Optional ByVal sLogMessage As String = "") As Boolean
            If Not dbExists() Then Return Nothing
            Dim fUpdated As Boolean = False
            If Database.Connect Then
                Dim SQL As String = CStr(IIf(AsNewRecord, "INSERT INTO RoleActions (RoleGroupID, ActionType, Status, Comment) VALUES (?, ?, ?, ?)",
                                                          "UPDATE RoleActions SET RoleGroupID=?, ActionType=?, Status=?, Comment=? WHERE ID=?"))
                Dim tParams As New List(Of Object)
                tParams.Add(tAction.RoleGroupID)
                tParams.Add(CInt(tAction.ActionType))
                tParams.Add(CInt(tAction.Status))
                tParams.Add(tAction.Comment)
                If Not AsNewRecord Then tParams.Add(tAction.ID)
                DebugInfo(String.Format("{0} role action '{1}'", CStr(IIf(AsNewRecord, "Add new", "Update")), tAction.ID))
                fUpdated = Database.ExecuteSQL(SQL, tParams) > 0
                If fUpdated Then
                    If AsNewRecord Then tAction.ID = clsDatabaseAdvanced.GetLastIdentity(Database) ' D0458
                    'DBSaveLog(CType(IIf(AsNewRecord, dbActionType.actCreate, dbActionType.actModify), dbActionType), dbObjectType.einfRoleAction, tAction.ID, sLogMessage, "") ' D0499 -D7205
                End If
            End If
            Return fUpdated
        End Function
        ' D0475 ==

        ' D0476 ===
        Public Function DBRoleActionDelete(ByVal Action As clsRoleAction, Optional ByVal sLogMessage As String = "") As Boolean    ' D0065
            If Not dbExists() Then Return Nothing
            Dim fResult As Boolean = False
            If Database.Connect Then
                Dim SQL As String = String.Format("DELETE FROM {0} WHERE {1}={2}", _TABLE_ROLEACTIONS, _FLD_ROLEACTIONS_ID, Action.ID)
                fResult = Database.ExecuteSQL(SQL) > 0
                If sLogMessage = "" Then sLogMessage = ResString(String.Format(ResString("lblProps_Template"), Action.ActionType.ToString)) ' D0496
                'DBSaveLog(dbActionType.actDelete, dbObjectType.einfRoleAction, Action.ID, sLogMessage, "") ' D0496 -D7205
            End If
            Return fResult
        End Function
        ' D0476 ==

#End Region


#Region "UserWorkgroups"

        ' D0464 ===
        Public Function DBUserWorkgroupsByWorkgroupID(ByVal tWorkgroupID As Integer) As List(Of clsUserWorkgroup)
            If Not dbExists() Then Return Nothing

            Dim tList As List(Of Dictionary(Of String, Object)) = Database.SelectFromTable(_TABLE_USERWORKGROUPS, _FLD_USERWRKG_WRKGID, tWorkgroupID)
            Dim tUWList As New List(Of clsUserWorkgroup)
            ECCore.MiscFuncs.PrintDebugInfo("******* DBParse_UserWorkgroup started - ")
            For Each tRow As Dictionary(Of String, Object) In tList
                Dim tUserWorkgroup As clsUserWorkgroup = DBParse_UserWorkgroup(tRow)
                tUWList.Add(tUserWorkgroup)
            Next
            ECCore.MiscFuncs.PrintDebugInfo("******* DBParse_UserWorkgroup ended - ")
            DebugInfo(String.Format("Loaded list of UserWorkgroups with WorkgroupID #{0}", tWorkgroupID))
            ECCore.MiscFuncs.PrintDebugInfo("******* DBUserWorkgroupsByWorkgroupID ended - ")
            Return tUWList
        End Function

        Public Function DBUserWorkgroupsByUserID(ByVal tUserID As Integer) As List(Of clsUserWorkgroup)
            If Not dbExists() Then Return Nothing
            Dim tList As List(Of Dictionary(Of String, Object)) = Database.SelectFromTable(_TABLE_USERWORKGROUPS, _FLD_USERWRKG_USERID, tUserID)
            Dim tUWList As New List(Of clsUserWorkgroup)
            For Each tRow As Dictionary(Of String, Object) In tList
                Dim tUserWorkgroup As clsUserWorkgroup = DBParse_UserWorkgroup(tRow)
                tUWList.Add(tUserWorkgroup)
            Next
            DebugInfo(String.Format("Loaded list of UserWorkgroups for UserID #{0}", tUserID))
            Return tUWList
        End Function

        ' D0465 ===
        Public Function DBUserWorkgroupByUserIDWorkgroupID(ByVal tUserID As Integer, ByVal tWorkgroupID As Integer) As clsUserWorkgroup
            If Not dbExists() Then Return Nothing
            Dim tParams As New List(Of Object)
            tParams.Add(tUserID)
            tParams.Add(tWorkgroupID)
            Dim tList As List(Of Dictionary(Of String, Object)) = Database.SelectBySQL(String.Format("SELECT * FROM {0} WHERE {1}=? AND {2}=?", _TABLE_USERWORKGROUPS, _FLD_USERWRKG_USERID, _FLD_USERWRKG_WRKGID), tParams)
            DebugInfo(String.Format("Loaded UserWorkgroup with UserID #{0} and WorkgroupID #{1}", tUserID, tWorkgroupID))
            If tList.Count > 0 Then Return DBParse_UserWorkgroup(tList(0)) Else Return Nothing
        End Function
        ' D0465 ==

        ' D3954 ===
        Public Function DBUserWorkgroupsByProjectIDWorkgroupID(ByVal tProjectID As Integer, ByVal tWorkgroupID As Integer) As List(Of clsUserWorkgroup)
            If Not dbExists() Then Return Nothing
            Dim tParams As New List(Of Object)
            tParams.Add(tWorkgroupID)
            tParams.Add(tProjectID)
            Dim tList As List(Of Dictionary(Of String, Object)) = Database.SelectBySQL(String.Format("SELECT UW.* FROM {0} UW LEFT JOIN {1} W ON W.UserID = UW.UserID WHERE UW.WorkgroupID=? AND W.ProjectID=?", _TABLE_USERWORKGROUPS, _TABLE_WORKSPACE), tParams)
            Dim tUWList As New List(Of clsUserWorkgroup)
            For Each tRow As Dictionary(Of String, Object) In tList
                Dim tUserWorkgroup As clsUserWorkgroup = DBParse_UserWorkgroup(tRow)
                tUWList.Add(tUserWorkgroup)
            Next
            DebugInfo(String.Format("Loaded UserWorkgroup for  ProjectID#{0} and WorkgroupID #{1}", tProjectID, tWorkgroupID))
            Return tUWList
        End Function
        ' D3954 ==

        Public Function DBUserWorkgroupByID(ByVal tID As Integer) As clsUserWorkgroup
            If Not dbExists() Then Return Nothing
            Dim tList As List(Of Dictionary(Of String, Object)) = Database.SelectFromTable(_TABLE_USERWORKGROUPS, _FLD_USERWRKG_ID, tID)
            DebugInfo(String.Format("Loaded UserWorkgroup with ID #{0}", tID))
            If tList.Count > 0 Then Return DBParse_UserWorkgroup(tList(0)) Else Return Nothing
        End Function
        ' D0464 ==

        ' D0475 ===
        Public Function DBUserWorkgroupUpdate(ByRef tUserWorkgroup As clsUserWorkgroup, Optional ByVal AsNewRecord As Boolean = False, Optional ByVal sLogMessage As String = "") As Boolean  ' D0046 + D052
            If Not dbExists() Then Return Nothing
            Dim fUpdated As Boolean = False
            If tUserWorkgroup IsNot Nothing AndAlso Database.Connect AndAlso CanvasMasterDBVersion >= "0.92" Then    ' D0515 + D0897
                Dim SQL As String = CStr(IIf(AsNewRecord, "INSERT INTO UserWorkgroups (UserID, WorkgroupID, RoleGroupID, Status, ExpirationDate, Comment, Created) VALUES (?, ?, ?, ?, ?, ?, ?)",
                                                          "UPDATE UserWorkgroups SET UserID=?, WorkgroupID=?, RoleGroupID=?, Status=?, ExpirationDate = ?, Comment=? WHERE ID=?"))  ' D0420

                ' D0897 ===
                ' Avoid to lock administrator in workgroup
                If Not AsNewRecord AndAlso tUserWorkgroup.Status = ecUserWorkgroupStatus.uwDisabled Then
                    Dim tUser As clsApplicationUser = Nothing
                    If ActiveUser IsNot Nothing AndAlso ActiveUser.UserID = tUserWorkgroup.UserID Then tUser = ActiveUser
                    If tUser Is Nothing Then tUser = DBUserByID(tUserWorkgroup.UserID)
                    If tUser IsNot Nothing AndAlso tUser.CannotBeDeleted Then tUserWorkgroup.Status = ecUserWorkgroupStatus.uwEnabled
                End If
                ' D0897 ==

                Dim tParams As New List(Of Object)
                tParams.Add(tUserWorkgroup.UserID)
                tParams.Add(tUserWorkgroup.WorkgroupID)
                tParams.Add(tUserWorkgroup.RoleGroupID)
                tParams.Add(CInt(tUserWorkgroup.Status))
                ' D0420 ===
                Dim Dt As Object = DBNull.Value
                If tUserWorkgroup.ExpirationDate.HasValue Then Dt = tUserWorkgroup.ExpirationDate.Value
                tParams.Add(Dt)
                ' D0420 ==
                tParams.Add(tUserWorkgroup.Comment)
                If Not AsNewRecord Then tParams.Add(tUserWorkgroup.ID) Else tParams.Add(Now)
                DebugInfo(String.Format("{0} UserWorkgroup '{1}'", CStr(IIf(AsNewRecord, "Add new", "Update")), tUserWorkgroup.ID))
                fUpdated = Database.ExecuteSQL(SQL, tParams) > 0
                If fUpdated Then
                    If AsNewRecord Then
                        tUserWorkgroup.ID = clsDatabaseAdvanced.GetLastIdentity(Database)   ' D0458
                    End If
                    If sLogMessage IsNot Nothing Then DBSaveLog(CType(IIf(AsNewRecord, dbActionType.actCreate, dbActionType.actModify), dbActionType), dbObjectType.einfUserWorkgroup, tUserWorkgroup.ID, sLogMessage, "", tUserWorkgroup.UserID, tUserWorkgroup.WorkgroupID) ' D0499 + D0830 + D7354
                End If
            End If
            Return fUpdated
        End Function
        ' D0475 ==

        ' D0479 ===
        Public Function DBUserWorkgroupDelete(ByRef tUserWorkgroup As clsUserWorkgroup, ByVal fDeleteOnlyUserWorkgroup As Boolean, ByVal sLogMessage As String) As Boolean    ' D0181
            If Not dbExists() Then Return Nothing
            Dim fUpdated As Boolean = False
            If Database.Connect Then
                DebugInfo(String.Format("Delete UserWorkgroup '{0}'", tUserWorkgroup.ID))
                fUpdated = Database.ExecuteSQL(String.Format("DELETE FROM {0} WHERE {1}={2}", _TABLE_USERWORKGROUPS, _FLD_USERWRKG_ID, tUserWorkgroup.ID)) > 0
                DBSaveLog(dbActionType.actDelete, dbObjectType.einfUserWorkgroup, tUserWorkgroup.ID, sLogMessage, "") ' D0181
                If fUpdated And Not fDeleteOnlyUserWorkgroup Then
                    Dim PrjList As List(Of clsProject) = DBProjectsByWorkgroupID(tUserWorkgroup.WorkgroupID)
                    If PrjList.Count > 0 Then
                        Dim WSList As List(Of clsWorkspace) = DBWorkspacesByUserID(tUserWorkgroup.UserID)
                        For Each tPrj As clsProject In PrjList
                            Dim WS As clsWorkspace = clsWorkspace.WorkspaceByUserIDAndProjectID(tUserWorkgroup.UserID, tPrj.ID, WSList)
                            If Not WS Is Nothing Then
                                DBTinyURLDelete(-1, tPrj.ID, tUserWorkgroup.UserID) ' D0899
                                DBWorkspaceDelete(WS.ID, String.Format("Detach user from decision '{0}'", tPrj.Passcode))
                            End If
                        Next
                    End If
                    If Not ActiveUser Is Nothing Then
                        If tUserWorkgroup.UserID = ActiveUser.UserID Then
                            UserWorkgroups = Nothing
                            Workspaces = Nothing
                        End If
                    End If
                End If
            End If
            Return fUpdated
        End Function
        ' D0479 ==

        ' D0919 ===
        Public Function DBUserWorkgroupEULAVersion(ByVal tWkgID As Integer, ByVal tUserID As Integer) As String
            If Not dbExists() Then Return Nothing
            Dim EULAFile As String = ""
            If CanvasMasterDBVersion >= "0.992" Then
                Dim tEULAFile As Object = Database.ExecuteScalarSQL(String.Format("SELECT EULAVersion FROM {0} WHERE {1}={2} AND {3}={4}", _TABLE_USERWORKGROUPS, _FLD_USERWRKG_WRKGID, tWkgID, _FLD_USERWRKG_USERID, tUserID))
                If Not IsDBNull(tEULAFile) Then EULAFile = CStr(tEULAFile)
            End If
            Return EULAFile
        End Function
        ' D0919 ==

#End Region


#Region "UserTemplates"

        Public Function DBUserTemplatesAll() As List(Of clsUserTemplate)
            If Not dbExists() Then Return Nothing
            Dim tUserTplList As New List(Of clsUserTemplate)
            If CanvasMasterDBVersion >= "0.96" Then
                Dim tList As List(Of Dictionary(Of String, Object)) = Database.SelectFromTable(_TABLE_USERTEMPLATES, Nothing, Nothing, "*", _FLD_USERTPL_NAME)
                For Each tRow As Dictionary(Of String, Object) In tList
                    Dim tUserTpl As clsUserTemplate = DBParse_UserTemplate(tRow)
                    tUserTplList.Add(tUserTpl)
                Next
                DebugInfo(String.Format("Loaded list of all user templates ({0})", tUserTplList.Count))
            End If
            Return tUserTplList
        End Function

        Public Function DBUserTemplatesByUserID(ByVal tUserID As Integer) As List(Of clsUserTemplate)
            If Not dbExists() Then Return Nothing
            Dim tUTList As New List(Of clsUserTemplate)
            If CanvasMasterDBVersion >= "0.96" Then
                Dim tList As List(Of Dictionary(Of String, Object)) = Database.SelectFromTable(_TABLE_USERTEMPLATES, _FLD_USERTPL_USERID, tUserID, "*", _FLD_USERTPL_NAME)
                For Each tRow As Dictionary(Of String, Object) In tList
                    Dim tUserTemplate As clsUserTemplate = DBParse_UserTemplate(tRow)
                    tUTList.Add(tUserTemplate)
                Next
                DebugInfo(String.Format("Loaded list of UserTemplates for UserID #{0}", tUserID))
            End If
            Return tUTList
        End Function

        Public Function DBUserTemplateByID(ByVal tID As Integer) As clsUserTemplate
            If Not dbExists() Then Return Nothing
            If CanvasMasterDBVersion >= "0.96" Then
                Dim tList As List(Of Dictionary(Of String, Object)) = Database.SelectFromTable(_TABLE_USERTEMPLATES, _FLD_USERTPL_ID, tID)
                DebugInfo(String.Format("Loaded UserTemplate with ID #{0}", tID))
                If tList.Count > 0 Then Return DBParse_UserTemplate(tList(0))
            End If
            Return Nothing
        End Function

        Public Function DBUserTemplateUpdate(ByRef tUserTemplate As clsUserTemplate, Optional ByVal AsNewRecord As Boolean = False, Optional ByVal sLogMessage As String = "") As Boolean
            If Not dbExists() Then Return Nothing
            Dim fUpdated As Boolean = False
            If Database.Connect AndAlso CanvasMasterDBVersion >= "0.96" AndAlso tUserTemplate IsNot Nothing Then    ' D0802
                ' D2184 ===
                Dim sInsert1 As String = ""
                Dim sInsert2 As String = ""
                Dim sUpdate As String = ""
                Dim DB09994 As Boolean = CanvasMasterDBVersion >= "0.9994"
                If DB09994 Then
                    If AsNewRecord Then
                        sInsert1 = ", StreamHash, IsCustom"
                        sInsert2 = ", ?, ?"
                    Else
                        sUpdate = ", StreamHash=?, IsCustom=?"
                    End If
                End If
                Dim SQL As String = CStr(IIf(AsNewRecord, "INSERT INTO UserTemplates (UserID, TemplateName, Comment, StructureType, StreamSize, Stream, ModifyDate" + sInsert1 + ") VALUES (?, ?, ?, ?, ?, ?, ?" + sInsert2 + ")",
                                                              "UPDATE UserTemplates SET UserID=?, TemplateName=?, Comment=?, StructureType=?, StreamSize=?, Stream=?, ModifyDate=?" + sUpdate + " WHERE ID=?"))
                ' D2184 ==
                Dim tParams As New List(Of Object)
                tParams.Add(tUserTemplate.UserID)
                tParams.Add(tUserTemplate.TemplateName)
                tParams.Add(tUserTemplate.Comment)
                tParams.Add(CInt(tUserTemplate.TemplateType))
                ' D0802 ===
                Dim MS As New MemoryStream
                Dim fHasData As Boolean = False
                If tUserTemplate.TemplateData IsNot Nothing Then
                    Select Case tUserTemplate.TemplateType
                        Case StructureType.stPipeOptions
                            tUserTemplate.TemplateData.ProjectName = ""         ' D2183
                            tUserTemplate.TemplateData.ProjectGuid = Nothing    ' D2183
                            MS = tUserTemplate.TemplateData.WriteToStream()
                            tUserTemplate.TemplateHash = tUserTemplate.CalculateHash    ' D2184
                            fHasData = True
                    End Select
                End If
                If fHasData Then
                    tParams.Add(MS.ToArray.Length)
                    tParams.Add(MS.ToArray)
                Else
                    tParams.Add(0)
                    tParams.Add(DBNull.Value)
                End If
                ' D0802 ==
                tUserTemplate.LastModify = Now  ' D2184
                tParams.Add(tUserTemplate.LastModify)   ' D2184

                ' D2184 ===
                If DB09994 Then
                    tParams.Add(tUserTemplate.TemplateHash)
                    tParams.Add(tUserTemplate.IsCustom)
                End If
                ' D2184 ==

                If Not AsNewRecord Then tParams.Add(tUserTemplate.ID)

                DebugInfo(String.Format("{0} UserTemplate '{1}'", CStr(IIf(AsNewRecord, "Add new", "Update")), tUserTemplate.TemplateName))
                fUpdated = Database.ExecuteSQL(SQL, tParams) > 0
                If fUpdated Then
                    If AsNewRecord Then
                        tUserTemplate.ID = clsDatabaseAdvanced.GetLastIdentity(Database)
                    End If
                    DBSaveLog(CType(IIf(AsNewRecord, dbActionType.actCreate, dbActionType.actModify), dbActionType), dbObjectType.einfUserTemplate, tUserTemplate.ID, sLogMessage, tUserTemplate.TemplateHash)  ' D2185
                End If
                Debug.WriteLine(tUserTemplate.CalculateHash)
            End If
            Return fUpdated
        End Function

        Public Function DBUserTemplateDelete(ByRef tUserTemplate As clsUserTemplate, ByVal sLogMessage As String) As Boolean
            If Not dbExists() Then Return Nothing
            Dim fUpdated As Boolean = False
            If Database.Connect AndAlso CanvasMasterDBVersion >= "0.96" Then
                DebugInfo(String.Format("Delete UserTemplate '{0}'", tUserTemplate.TemplateName))
                fUpdated = Database.ExecuteSQL(String.Format("DELETE FROM {0} WHERE {1}={2}", _TABLE_USERTEMPLATES, _FLD_USERTPL_ID, tUserTemplate.ID)) > 0
                If fUpdated Then UserTemplates = Nothing ' D0813
                DBSaveLog(dbActionType.actDelete, dbObjectType.einfUserWorkgroup, tUserTemplate.ID, sLogMessage, tUserTemplate.TemplateName)
            End If
            Return fUpdated
        End Function

#End Region


#Region "Workspaces"

        ' D0464 ===
        Public Function DBWorkspacesByProjectID(ByVal tProjectID As Integer) As List(Of clsWorkspace)
            If Not dbExists() Then Return Nothing

            Dim tList As List(Of Dictionary(Of String, Object)) = Database.SelectFromTable(_TABLE_WORKSPACE, _FLD_WORKSPACE_PRJID, tProjectID, , _FLD_WORKSPACE_USERID) ' D3512
            Dim tWorkspaces As New List(Of clsWorkspace)

            ECCore.MiscFuncs.PrintDebugInfo("******* DBParse_Workspace started - ")
            For Each tRow As Dictionary(Of String, Object) In tList
                Dim tWorkspace As clsWorkspace = DBParse_Workspace(tRow)
                tWorkspaces.Add(tWorkspace)
            Next
            ECCore.MiscFuncs.PrintDebugInfo("******* DBParse_Workspace ended - ")

            DebugInfo(String.Format("Loaded list of Workspaces with ProjectID #{0}", tProjectID))
            ECCore.MiscFuncs.PrintDebugInfo("******* DBWorkspacesByProjectID ended - ")
            Return tWorkspaces
        End Function

        Public Function DBWorkspacesByUserID(ByVal tUserID As Integer) As List(Of clsWorkspace)
            If Not dbExists() Then Return Nothing
            Dim tList As List(Of Dictionary(Of String, Object)) = Database.SelectFromTable(_TABLE_WORKSPACE, _FLD_WORKSPACE_USERID, tUserID)
            Dim tWorkspaces As New List(Of clsWorkspace)
            For Each tRow As Dictionary(Of String, Object) In tList
                Dim tWorkspace As clsWorkspace = DBParse_Workspace(tRow)
                tWorkspaces.Add(tWorkspace)
            Next
            DebugInfo(String.Format("Loaded list of Workspaces with UserID #{0}", tUserID))
            Return tWorkspaces
        End Function

        ' D4606 ===
        Public Function DBWorkspacesByUserIDWorkgroupID(ByVal tUserID As Integer, tWkgID As Integer) As List(Of clsWorkspace)
            Dim tParams As New List(Of Object)
            tParams.Add(tUserID)
            tParams.Add(tWkgID)
            Dim tList As List(Of Dictionary(Of String, Object)) = Database.SelectBySQL("SELECT W.* FROM Workspace W LEFT JOIN Projects P ON P.ID=W.ProjectID WHERE W.UserID=? AND P.WorkgroupID=? ORDER BY W.LastModify DESC", tParams) ' D7205
            Dim tWorkspaces As New List(Of clsWorkspace)
            For Each tRow As Dictionary(Of String, Object) In tList
                Dim tWorkspace As clsWorkspace = DBParse_Workspace(tRow)
                tWorkspaces.Add(tWorkspace)
            Next
            DebugInfo(String.Format("Loaded list of Workspaces for UserID #{0} and Workgroup #{1}", tUserID, tWkgID))
            Return tWorkspaces
        End Function
        ' D4606 ==

        Public Function DBWorkspaceByID(ByVal tID As Integer) As clsWorkspace
            If Not dbExists() Then Return Nothing
            Dim tList As List(Of Dictionary(Of String, Object)) = Database.SelectFromTable(_TABLE_WORKSPACE, _FLD_WORKSPACE_ID, tID)
            DebugInfo(String.Format("Loaded Workspace with ID #{0}", tID))
            If tList.Count > 0 Then Return DBParse_Workspace(tList(0)) Else Return Nothing
        End Function
        ' D0464 ==

        ' D0466 ===
        Public Function DBWorkspaceByUserIDProjectID(ByVal tUserID As Integer, ByVal tProjectID As Integer) As clsWorkspace
            If Not dbExists() Then Return Nothing
            Dim tParams As New List(Of Object)
            tParams.Add(tUserID)
            tParams.Add(tProjectID)
            Dim tList As List(Of Dictionary(Of String, Object)) = Database.SelectBySQL(String.Format("SELECT * FROM {0} WHERE {1}=? AND {2}=?", _TABLE_WORKSPACE, _FLD_WORKSPACE_USERID, _FLD_WORKSPACE_PRJID), tParams)
            DebugInfo(String.Format("Loaded Workspace with UserID #{0} and ProjectID #{1}", tUserID, tProjectID))
            If tList.Count > 0 Then Return DBParse_Workspace(tList(0)) Else Return Nothing
        End Function
        ' D0466 ==

        ' D0478 ===
        Public Function DBWorkspaceUpdate(ByRef tWorkspace As clsWorkspace, ByVal AsNewRecord As Boolean, ByVal sLogMessage As String) As Boolean
            If Not dbExists() Then Return Nothing
            Dim fUpdated As Boolean = False
            If tWorkspace IsNot Nothing AndAlso Database.Connect AndAlso CanvasMasterDBVersion >= "0.92" Then    ' D0515 + D0897
                Dim SQL As String = CStr(IIf(AsNewRecord, "INSERT INTO Workspace (UserID, ProjectID, GroupID, Status, Step, Comment, Created{0}) VALUES (?, ?, ?, ?, ?, ?, ?{1})",
                                                          "UPDATE Workspace SET UserID=?, ProjectID=?, GroupID=?, Status=?, Step=?, Comment=?, LastModify=?{0} WHERE ID=?"))

                ' D0897 ===
                If Not AsNewRecord AndAlso (tWorkspace.StatusLikelihood = ecWorkspaceStatus.wsDisabled OrElse tWorkspace.StatusImpact = ecWorkspaceStatus.wsDisabled) Then  ' D1945
                    Dim tUser As clsApplicationUser = Nothing
                    If ActiveUser IsNot Nothing AndAlso ActiveUser.UserID = tWorkspace.UserID Then tUser = ActiveUser
                    If tUser Is Nothing Then tUser = DBUserByID(tWorkspace.UserID)
                    If tUser IsNot Nothing AndAlso tUser.CannotBeDeleted Then
                        tWorkspace.StatusLikelihood = ecWorkspaceStatus.wsEnabled   ' D1945
                        tWorkspace.StatusImpact = ecWorkspaceStatus.wsEnabled       ' D1945
                    End If
                End If
                ' D0897 ==

                Dim tParams As New List(Of Object)
                tParams.Add(tWorkspace.UserID)
                tParams.Add(tWorkspace.ProjectID)
                tParams.Add(tWorkspace.GroupID)
                tParams.Add(tWorkspace.StatusDataLikelihood)  ' D1945 + D5006
                tParams.Add(tWorkspace.ProjectStepLikelihood)   ' D1945
                tParams.Add(tWorkspace.Comment)
                tParams.Add(Now)

                ' D1945 ===
                Dim sName As String = ""
                Dim sParams As String = ""
                Dim sUpdate As String = ""

                If CanvasMasterDBVersion >= "0.95" Then    ' D0515
                    sName = ", TTStatus"
                    sParams = ", ?"
                    sUpdate = ", TTStatus=?"

                    tParams.Add(CInt(tWorkspace.TeamTimeStatusLikelihood))

                    If CanvasMasterDBVersion >= "0.9992" Then
                        sName += ", Status2, TTStatus2, Step2"
                        sParams += ", ?, ?, ?"
                        sUpdate += ", Status2=?, TTStatus2=?, Step2=?"
                        tParams.Add(tWorkspace.StatusDataImpact)    ' D5006
                        tParams.Add(CInt(tWorkspace.TeamTimeStatusImpact))
                        tParams.Add(tWorkspace.ProjectStepImpact)
                    End If
                End If

                If AsNewRecord Then
                    SQL = String.Format(SQL, sName, sParams)
                Else
                    tParams.Add(tWorkspace.ID)
                    SQL = String.Format(SQL, sUpdate)
                End If
                ' D1945 ==

                DebugInfo(String.Format("{0} workspace '{1}'", CStr(IIf(AsNewRecord, "Add new", "Update")), tWorkspace.ID))
                fUpdated = Database.ExecuteSQL(SQL, tParams) > 0
                If fUpdated Then
                    If AsNewRecord Then tWorkspace.ID = clsDatabaseAdvanced.GetLastIdentity(Database) ' D0458
                    If _CurrentWorkspace IsNot Nothing AndAlso _CurrentWorkspace.ID = tWorkspace.ID Then _CurrentWorkspace = Nothing    ' D5016
                End If
            End If
            Return fUpdated
        End Function

        Public Function DBWorkspaceDelete(ByRef tWorkspaceID As Integer, ByVal sLogMessage As String) As Boolean
            If Not dbExists() Then Return Nothing
            Dim fUpdated As Boolean = False
            If Database.Connect Then
                Dim SQL As String = String.Format("DELETE FROM Workspace WHERE ID={0}", tWorkspaceID)
                DebugInfo(String.Format("Delete Workspace '{0}'", tWorkspaceID))
                fUpdated = Database.ExecuteSQL(SQL) > 0
                DBSaveLog(dbActionType.actDelete, dbObjectType.einfWorkspace, tWorkspaceID, sLogMessage, "") ' D0496
            End If
            Return fUpdated
        End Function
        ' D0478 ==

        ' D0284 + D0486 ===
        Public Function DBWorkspacesTeamTimeByUserID(ByVal tUserID As Integer) As List(Of clsWorkspace)  ' D0504 + D0506
            If Not dbExists() Then Return Nothing
            Dim WSList As New List(Of clsWorkspace)
            For Each tWS As clsWorkspace In DBWorkspacesByUserID(tUserID)   ' D0504
                If tWS.isInTeamTime(True) OrElse tWS.isInTeamTime(False) Then WSList.Add(tWS) ' D0382 + D1945
            Next
            Return WSList
        End Function
        ' D0284 + D0486 ==

        ' D0862 ===
        Public Function DBWorkspacesResetProjectStep(ByVal tProjectID As Integer) As Boolean
            If Not dbExists() Then Return Nothing
            Dim fUpdated As Boolean = False
            If Database.Connect Then
                Dim SQL As String = String.Format("UPDATE Workspace SET {0}=1 WHERE {0}>1 AND {1}={2}", _FLD_WORKSPACE_STEP, _FLD_WORKSPACE_PRJID, tProjectID)
                DebugInfo(String.Format("Reset step for all users in project #{0}", tProjectID))
                fUpdated = Database.ExecuteSQL(SQL) > 0
                If fUpdated Then Workspaces = Nothing
            End If
            Return fUpdated
        End Function
        ' D0862 ==

#End Region


#Region "Projects"

        ' D0464 ===
        Public Function DBProjectsAll() As List(Of clsProject)
            If Not dbExists() Then Return Nothing
            Dim tList As List(Of Dictionary(Of String, Object)) = Database.SelectFromTable(_TABLE_PROJECTS, Nothing, Nothing, "*", _FLD_PROJECTS_NAME)   ' D0483 + D0494 + D0497
            Dim tProjects As New List(Of clsProject)
            For Each tRow As Dictionary(Of String, Object) In tList
                Dim tProject As clsProject = DBParse_Project(tRow)
                tProjects.Add(tProject)
            Next
            DebugInfo(String.Format("Loaded list of all Projects ({0})", tProjects.Count))
            Return tProjects
        End Function

        Public Function DBProjectsByWorkgroupID(ByVal tWorkgroupID As Integer) As List(Of clsProject)
            If Not dbExists() Then Return Nothing
            Dim tList As List(Of Dictionary(Of String, Object)) = Database.SelectFromTable(_TABLE_PROJECTS, _FLD_PROJECTS_WRKGID, tWorkgroupID, "*", _FLD_PROJECTS_NAME)   ' D0483 + D0494 + D0497
            Dim tProjects As New List(Of clsProject)
            For Each tRow As Dictionary(Of String, Object) In tList
                Dim tProject As clsProject = DBParse_Project(tRow)
                tProjects.Add(tProject)
            Next
            DebugInfo(String.Format("Loaded list of Projects with WorkgroupID #{0}", tWorkgroupID))
            Return tProjects
        End Function

        ' D1385 ===
        Public Function DBProjectsByUserID(ByVal tUserID As Integer, Optional tShowLastCount As Integer = -1) As List(Of clsProject)
            If Not dbExists() Then Return Nothing
            Dim sSQL = String.Format("SELECT {0} P.* FROM {1} P LEFT JOIN {2} W ON W.{3}=P.{4} WHERE W.{5} = ? ORDER BY P.{6}", IIf(tShowLastCount = -1, "", "TOP " + tShowLastCount.ToString), _TABLE_PROJECTS, _TABLE_WORKSPACE, _FLD_WORKSPACE_PRJID, _FLD_PROJECTS_ID, _FLD_WORKSPACE_USERID, IIf(tShowLastCount = -1, _FLD_PROJECTS_NAME, "Created DESC"))
            Dim tParams As New List(Of Object)
            tParams.Add(tUserID)
            Dim tList As List(Of Dictionary(Of String, Object)) = Database.SelectBySQL(sSQL, tParams)
            Dim tProjects As New List(Of clsProject)
            For Each tRow As Dictionary(Of String, Object) In tList
                Dim tProject As clsProject = DBParse_Project(tRow)
                tProjects.Add(tProject)
            Next
            DebugInfo(String.Format("Loaded list of Projects for User #{0}", tUserID))
            Return tProjects
        End Function
        ' D1385 ==

        Public Function DBProjectByPasscode(ByVal sPasscode As String) As clsProject   ' D0471 + D0478
            If Not dbExists() Then Return Nothing
            ' D1709 ===
            Dim sSQL As String = String.Format("SELECT * FROM {0} WHERE {1}=?", _TABLE_PROJECTS, _FLD_PROJECTS_PASSCODE)
            Dim tParams As New List(Of Object)
            tParams.Add(sPasscode)
            If CanvasMasterDBVersion >= "0.9991" Then
                sSQL += String.Format(" OR {0}=?", _FLD_PROJECTS_PASSCODE_IMPACT)
                tParams.Add(sPasscode)
            End If
            Dim tList As List(Of Dictionary(Of String, Object)) = Database.SelectBySQL(sSQL, tParams)
            'Dim tList As List(Of Dictionary(Of String, Object)) = Database.SelectFromTable(_TABLE_PROJECTS, _FLD_PROJECTS_PASSCODE, sPasscode)   ' D0483 + D0494
            ' D1709 ==
            DebugInfo(String.Format("Loaded Project with passcode '{0}'", sPasscode))
            If tList.Count > 0 Then Return DBParse_Project(tList(0)) Else Return Nothing ' D0471
        End Function

        Public Function DBProjectByMeetingID(ByVal sMeetingID As Long, Optional fResetNonUniqueIDsButPrjID As Integer = -1) As clsProject    ' D0468 + D3320
            If Not dbExists() Then Return Nothing
            ' D1709 ===
            Dim sSQL As String = String.Format("SELECT * FROM {0} WHERE {1}=?", _TABLE_PROJECTS, _FLD_PROJECTS_MEETINGID)
            Dim tParams As New List(Of Object)
            tParams.Add(sMeetingID)
            If CanvasMasterDBVersion >= "0.9991" Then
                sSQL += String.Format(" OR {0}=?", _FLD_PROJECTS_MEETINGID_IMPACT)
                tParams.Add(sMeetingID)
            End If
            sSQL += String.Format(" ORDER BY {0} DESC, {1} DESC", _FLD_PROJECTS_LASTVISITED, _FLD_PROJECTS_ID)   ' D3320
            Dim tList As List(Of Dictionary(Of String, Object)) = Database.SelectBySQL(sSQL, tParams)
            'Dim tList As List(Of Dictionary(Of String, Object)) = Database.SelectFromTable(_TABLE_PROJECTS, _FLD_PROJECTS_MEETINGID, sMeetingID)   ' D0483 + D0494
            ' D1709 ==
            DebugInfo(String.Format("Loaded Project with MeetingID '{0}'", sMeetingID))
            If tList.Count > 0 Then
                ' D3320 ===
                Dim idx As Integer = -1
                For i As Integer = 0 To tList.Count - 1
                    Dim tPrj As clsProject = DBParse_Project(tList(i))
                    If tPrj IsNot Nothing Then
                        If idx < 0 Then idx = i
                        If tPrj.isTeamTimeImpact OrElse tPrj.isTeamTimeLikelihood Then idx = i
                        If fResetNonUniqueIDsButPrjID > 0 AndAlso tPrj.ID <> fResetNonUniqueIDsButPrjID Then
                            Dim fDoUpdate As Boolean = False
                            If tPrj.MeetingIDImpact(True) = sMeetingID Then
                                tPrj.MeetingIDImpact = clsMeetingID.ReNew()
                                fDoUpdate = True
                            End If
                            If tPrj.MeetingIDLikelihood(True) = sMeetingID Then
                                tPrj.MeetingIDLikelihood = clsMeetingID.ReNew()
                                fDoUpdate = True
                            End If
                            If fDoUpdate Then DBProjectUpdate(tPrj, False, "Generate new MeetingID, was " + clsMeetingID.AsString(sMeetingID))
                        End If
                    End If
                Next
                If idx >= 0 Then Return DBParse_Project(tList(idx))
            End If
            Return Nothing ' D0468
            ' D3320 ==
        End Function

        Public Function DBProjectByID(ByVal tID As Integer) As clsProject
            If Not dbExists() Then Return Nothing
            Dim tList As List(Of Dictionary(Of String, Object)) = Database.SelectFromTable(_TABLE_PROJECTS, _FLD_PROJECTS_ID, tID)   ' D0483 + D0494
            DebugInfo(String.Format("Loaded Projects with ID #{0}", tID))
            If tList.Count > 0 Then Return DBParse_Project(tList(0)) Else Return Nothing
        End Function
        ' D0464 ==

        ' D0473 ===
        Public Function DBProjectLockInfoWrite(ByVal tLockStatus As ECLockStatus, ByRef tLockInfo As clsProjectLockInfo, ByRef tLockUser As clsApplicationUser, ByVal tExpiration As Nullable(Of DateTime)) As Boolean  ' D0483 + D0589
            If Not dbExists() Then Return Nothing
            If tLockInfo Is Nothing Then Return False ' D0483
            If tExpiration IsNot Nothing AndAlso tExpiration.HasValue AndAlso tExpiration < Now Then tLockStatus = ECLockStatus.lsUnLocked ' D0481 + D0589 + D5080
            If tLockStatus <> ECLockStatus.lsUnLocked Then  ' D0589
                If Not tExpiration.HasValue Then tExpiration = Now.AddSeconds(Options.ProjectLockTimeout) ' D0851
            Else
                If Not tExpiration.HasValue Then tExpiration = Now ' D0481
            End If
            ' D0481 ===
            Dim tParams As New List(Of Object)
            tParams.Add(tExpiration.Value)
            tParams.Add(Now)
            ' D0483 ===
            'tLockInfo.LockerUser = tLockUser   ' -D0494
            tLockInfo.LockExpiration = tExpiration
            Dim UID As Integer = -1
            If Not tLockUser Is Nothing Then UID = tLockUser.UserID
            tLockInfo.LockerUserID = UID    ' D0513
            tLockInfo.LockStatus = tLockStatus  ' D0589
            DebugInfo(String.Format("Set project expiration {0} (prj #{1})", tLockStatus, tLockInfo.ProjectID))    ' D0589
            Database.ExecuteSQL(String.Format("UPDATE {0} SET LockStatus={1}, LockedByUserID={2}, LockExpiration=?, {5}=? WHERE {3}={4}", _TABLE_PROJECTS, CInt(tLockStatus), UID, _FLD_PROJECTS_ID, tLockInfo.ProjectID, clsComparionCore._FLD_PROJECTS_LASTVISITED), tParams) ' D0589 + D6010 + D6821
            ' D0483 ==
            ' D0481 ==
            Return True ' D6821
        End Function
        ' D0473 ==

        ' D0473 + D0483 ===
        Public Function DBProjectLockInfoRead(ByVal tProjectID As Integer) As clsProjectLockInfo
            If Not dbExists() Then Return Nothing
            Dim tProject As clsProject = DBProjectByID(tProjectID)
            If Not tProject Is Nothing Then Return tProject.LockInfo Else Return Nothing
        End Function
        ' D0473 + D0483 ==

        ' D0476 ===
        Public Function DBProjectUpdate(ByRef tProject As clsProject, Optional ByVal AsNewRecord As Boolean = False, Optional ByVal sLogMessage As String = "") As Boolean
            If Not dbExists() OrElse tProject Is Nothing Then Return False
            Dim fUpdated As Boolean = False
            If Database.Connect AndAlso CanvasMasterDBVersion >= "0.92" Then
                Dim tSysLifeTime As Long = 0
                If AsNewRecord Then tSysLifeTime = SystemWorkgroup.License.GetParameterValueByID(ecLicenseParameter.MaxLifetimeProjects)    ' D4564
                If tProject.OwnerID = 0 And Not ActiveUser Is Nothing Then tProject.OwnerID = ActiveUser.UserID ' D0086
                If tProject.isMarkedAsDeleted AndAlso tProject.isOnline Then tProject.isOnline = False ' D0964
                ' D1709 ===
                Dim tParams As New List(Of Object)
                tParams.Add(tProject.OwnerID)
                tParams.Add(tProject.WorkgroupID)
                tParams.Add(tProject.PasscodeLikelihood)    ' D1709
                'tParams.Add(tProject.FileName) ' -D1193
                tParams.Add("")                 ' D1194
                tParams.Add(tProject.ProjectName)
                tParams.Add(tProject.StatusDataLikelihood)  ' D1944
                tParams.Add(tProject.MeetingIDLikelihood)   ' D1734
                If tProject.MeetingOwner Is Nothing Then tParams.Add(-1) Else tParams.Add(tProject.MeetingOwner.UserID) ' D0506
                tParams.Add(tProject.Comment)
                tParams.Add(Now)
                If Not AsNewRecord Then tParams.Add(Now)    ' D6010

                Dim sFields As String = ""
                Dim sValues As String = ""
                Dim sUpdates As String = ""
                If CanvasMasterDBVersion >= "0.98" Then
                    sFields += ", GUID, ReplacedID"
                    sValues += ", ?, ?"
                    sUpdates += ", GUID=?, ReplacedID=?"
                    tProject.CheckGUID()    ' D0892
                    tParams.Add(tProject.ProjectGUID.ToString)  ' D0892
                    tParams.Add(tProject.ReplacedID)    ' D0893
                End If
                If CanvasMasterDBVersion >= "0.9991" Then
                    sFields += ", Passcode2, MeetingID2"
                    sValues += ", ?, ?"
                    sUpdates += ", Passcode2=?, MeetingID2=?"
                    tProject.CheckGUID()    ' D0892
                    tParams.Add(tProject.PasscodeImpact)
                    tParams.Add(tProject.MeetingIDImpact)
                End If
                ' D1944 ===
                If CanvasMasterDBVersion >= "0.9992" Then
                    sFields += ", Status2"
                    sValues += ", ?"
                    sUpdates += ", Status2=?"
                    tParams.Add(tProject.StatusDataImpact)
                End If
                ' D1944 ==
                ' D3438 ===
                If CanvasMasterDBVersion >= "0.9999" Then
                    sFields += ", ProjectType"
                    sValues += ", ?"
                    sUpdates += ", ProjectType=?"
                    tParams.Add(tProject.ProjectTypeData)
                End If
                ' D3438 ==
                Dim SQL As String = CStr(IIf(AsNewRecord, String.Format("INSERT INTO Projects (OwnerID, WorkgroupID, Passcode, FileName, ProjectName, Status, MeetingID, MeetingOwnerID, Comment, Created{0}) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?{1})", sFields, sValues),
                                                          String.Format("UPDATE Projects SET OwnerID=?, WorkgroupID=?, Passcode=?, FileName=?, ProjectName=?, Status=?, MeetingID = ?, MeetingOwnerID=?, Comment=?, LastModify=?, LastVisited=?{0} WHERE ID=?", sUpdates))) ' D0045 +  D0086 + D0329 + D0420 + D0476 + D0506 + D0892 + D0893 + D0894 + D1193 + D1194 + D1709 + D6010
                ' D1709 ==
                If Not AsNewRecord Then tParams.Add(tProject.ID)
                fUpdated = Database.ExecuteSQL(SQL, tParams) > 0
                If fUpdated Then
                    If AsNewRecord Then   ' D3322 + D3361
                        tProject.ID = clsDatabaseAdvanced.GetLastIdentity(Database) ' D0045 + D0458
                        ' D0924 ===
                        If tProject.ProjectStatus <> ecProjectStatus.psMasterProject AndAlso tProject.ProjectStatus <> ecProjectStatus.psTemplate AndAlso CanvasMasterDBVersion >= "0.992" Then  ' D3361 + D4038
                            If SystemWorkgroup IsNot Nothing Then
                                DBWorkgroupUpdateLifetimeProjects(SystemWorkgroup, CInt(tSysLifeTime + 1)) ' D4564
                            End If
                            Dim tWkg As clsWorkgroup = Nothing
                            If ActiveWorkgroup IsNot Nothing AndAlso tProject.WorkgroupID = ActiveWorkgroup.ID Then tWkg = ActiveWorkgroup
                            If tWkg Is Nothing Then tWkg = DBWorkgroupByID(tProject.WorkgroupID, False, False)
                            If tWkg IsNot Nothing Then
                                Dim tCnt As Long = -1
                                If tWkg.License.isValidLicense Then tCnt = tWkg.License.GetParameterValueByID(ecLicenseParameter.MaxLifetimeProjects, tProject.ID) ' D0960
                                If tCnt < tWkg.LifeTimeProjects Then tCnt = tWkg.LifeTimeProjects
                                If tCnt < 0 Then tCnt = 0   ' D4564
                                DBWorkgroupUpdateLifetimeProjects(tWkg, CInt(tCnt) + 1) ' D4564
                                If ActiveWorkgroup IsNot Nothing AndAlso ActiveWorkgroup.ID = tProject.WorkgroupID Then ActiveWorkgroup.LifeTimeProjects = tWkg.LifeTimeProjects ' D5464
                            End If
                        End If
                        ' D0924 ==
                    End If
                End If
                DBSaveLog(CType(IIf(AsNewRecord, dbActionType.actCreate, dbActionType.actModify), dbActionType), dbObjectType.einfProject, tProject.ID, sLogMessage, tProject.Passcode(tProject.isImpact))  ' D0496 + D3112
            End If
            Return fUpdated     ' D0041
        End Function
        ' D0476 ==

        ' D0494 ===
        Public Function DBProjectUpdateToLastVersion(ByRef SrcProject As clsProject) As Boolean
            If Not dbExists() Then Return False
            Dim fUpdated As Boolean = False
            If SrcProject Is Nothing Then Return fUpdated
            ' D0148 ===
            Dim OldVer As ECCanvasDatabaseVersion = SrcProject.DBVersion    ' D0149
            Dim NewVer As ECCanvasDatabaseVersion = GetCurrentDBVersion()
            If IsEqualCanvasDBVersions(OldVer, NewVer) Then Return True
            If SrcProject.isDBVersionCanBeUpdated Then    ' D1429
                ' D0148 ==
                Dim fNeedUnlock As Boolean = Not ActiveUser Is Nothing AndAlso Not SrcProject.LockInfo.isLockAvailable(ActiveUser.UserID) ' D0494 +D0513 + D0641
                If Not ActiveUser Is Nothing Then DBProjectLockInfoWrite(ECLockStatus.lsLockForSystem, SrcProject.LockInfo, ActiveUser, Nothing) ' D0494 + D0513 + D0589
                'SnapshotSaveProject(ecSnapShotType.RestorePoint, String.Format("Create {0} project backup before upgrade up to version {1}", OldVer.GetVersionString, NewVer.GetVersionString), SrcProject.ID, True)    ' D3572
                SnapshotSaveProject(ecSnapShotType.RestorePoint, "Create model backup before upgrade", SrcProject.ID, True, String.Format("Old version: {0}; New version: {1}", OldVer.GetVersionString, NewVer.GetVersionString))    ' D3572 + D3731 + D6311
                If DBProjectCopy(SrcProject, clsProject.StorageType, SrcProject.ConnectionString, SrcProject.ProviderType, -SrcProject.ID, False) Then  ' D3774
                    If DBProjectStreamDelete(SrcProject.ID) Then
                        Using DBStream As New clsDatabaseAdvanced(clsDatabaseAdvanced.GetConnectionString(Options.CanvasProjectsDBName), Database.ProviderType)
                            If DBStream.Connect Then
                                Dim oCommand As DbCommand = DBStream.SQL("UPDATE ModelStructure SET ProjectID=-ProjectID WHERE ProjectID=?")
                                oCommand.Parameters.Add(GetDBParameter(DBStream.ProviderType, "ProjectID", -SrcProject.ID))
                                Dim affected As Integer = DBExecuteNonQuery(DBStream.ProviderType, oCommand)
                                DebugInfo(String.Format("Updated ProjectID for ModelStructure, Project #{0}, affected rows: {1}", SrcProject.ID, affected))
                                oCommand.CommandText = "UPDATE UserData SET ProjectID=-ProjectID WHERE ProjectID=?"
                                affected = DBExecuteNonQuery(DBStream.ProviderType, oCommand)
                                DebugInfo(String.Format("Update ProjectID for UserData, Project #{0}, affected rows: {1}", SrcProject.ID, affected))

                                If OldVer.GetVersionString < "1.1.16" Then
                                    Dim tUserList As List(Of clsUser) = MiscFuncs.GetUsersList(SrcProject.ConnectionString, clsProject.StorageType, SrcProject.ProviderType, SrcProject.ID)
                                    Dim AUsersList As New Dictionary(Of String, clsComparionUser)
                                    For Each User As clsUser In tUserList
                                        AUsersList.Add(User.UserEMail, New clsComparionUser() With {.ID = User.UserID, .UserName = User.UserName})
                                    Next
                                    Dim WelcomeSurveyGUID As String = SrcProject.PipeParameters.PipeMessages.GetWelcomeText(PipeMessageKind.pmkSurvey, SrcProject.HierarchyObjectives.HierarchyID, SrcProject.HierarchyAlternatives.HierarchyID)
                                    Dim ThankyouSurveyGUID As String = SrcProject.PipeParameters.PipeMessages.GetThankYouText(PipeMessageKind.pmkSurvey, SrcProject.HierarchyObjectives.HierarchyID, SrcProject.HierarchyAlternatives.HierarchyID)
                                    Dim tSurveyWelcome As clsSurveyInfo = SurveysManager.GetSurveyInfo(ActiveSurveysList, WelcomeSurveyGUID)
                                    Dim tSurveyThankyou As clsSurveyInfo = SurveysManager.GetSurveyInfo(ActiveSurveysList, ThankyouSurveyGUID)
                                    If tSurveyWelcome IsNot Nothing Then
                                        tSurveyWelcome.SurveyType = SurveyType.stWelcomeSurvey
                                        tSurveyWelcome.ProjectID = SrcProject.ID
                                        tSurveyWelcome.ComparionUsersList = AUsersList
                                        tSurveyWelcome.StorageType = SurveyStorageType.sstDatabaseStream
                                        Dim WelcomeSurveyDP As New SpyronControls.Spyron.Data.clsSurveyDataProvider(tSurveyWelcome)
                                        WelcomeSurveyDP.OpenSurvey("")
                                        WelcomeSurveyDP.SaveSurvey(True)
                                    End If
                                    If tSurveyThankyou IsNot Nothing Then
                                        tSurveyThankyou.SurveyType = SurveyType.stThankyouSurvey
                                        tSurveyThankyou.ProjectID = SrcProject.ID
                                        tSurveyThankyou.ComparionUsersList = AUsersList
                                        tSurveyThankyou.StorageType = SurveyStorageType.sstDatabaseStream
                                        Dim ThankyouSurveyDP As New SpyronControls.Spyron.Data.clsSurveyDataProvider(tSurveyThankyou)
                                        ThankyouSurveyDP.OpenSurvey("")
                                        ThankyouSurveyDP.SaveSurvey(True)
                                    End If
                                End If
                                fUpdated = True
                                DBStream.Close()
                            End If
                        End Using
                    End If
                End If
                If fNeedUnlock AndAlso Not ActiveUser Is Nothing Then DBProjectLockInfoWrite(ECLockStatus.lsUnLocked, SrcProject.LockInfo, ActiveUser, Now) ' D0494 + D0513 + D0589
                'If fUpdated Then DBProjectSyncProjectType(SrcProject) ' D3438
                If fUpdated Then ActiveProjectsList = Nothing ' D0494
                DBSaveLog(dbActionType.actUpgrade, dbObjectType.einfProject, SrcProject.ID, String.Format("Update projectDB to latest version ({0} -> {1})", OldVer.GetVersionString, NewVer.GetVersionString), fUpdated.ToString) ' D0499 + D2289
                SrcProject.DBVersionReset()     ' D1429
                SrcProject.IgnoreDBVersion = True     ' D0622
                SnapshotSaveProject(ecSnapShotType.RestorePoint, "Save model in the latest version", SrcProject.ID, True, String.Format("Upgraded up to the version {0}", SrcProject.DBVersion.GetVersionString))    ' D3572 + D3731 + D6311
            End If
            Return fUpdated
        End Function
        ' D0494 ==

        ' D0591 ===
        Public Function DBProjectStructureLastModifyTime(ByVal tProjectID As Integer) As DateTime
            If Not dbExists() Then Return Nothing
            Dim DT As DateTime = Now.AddYears(-1)
            If Database.Connect Then
                Dim Data As Object = Database.ExecuteScalarSQL(String.Format("SELECT ModifyDate FROM ModelStructure WHERE ProjectID={0} AND StructureType={1}", tProjectID, CInt(StructureType.stModelStructure)))
                If Data IsNot Nothing AndAlso Not IsDBNull(Data) Then
                    DT = CType(Data, DateTime)
                End If
            End If
            Return DT
        End Function
        ' D0591 ==

        '' D3438 ===
        'Public Function DBProjectSyncProjectType(tPrj As clsProject) As Boolean
        '    If Not dbExists() Then Return Nothing
        '    Dim fUpdate As Boolean = False
        '    If tPrj IsNot Nothing AndAlso tPrj.isValidDBVersion AndAlso tPrj.IsProjectLoaded AndAlso CanvasMasterDBVersion >= "0.9999" Then
        '        ' D4978 ===
        '        If tPrj.ProjectTypeData <> tPrj.PipeParameters.ProjectType Then
        '            tPrj.ProjectTypeData = tPrj.PipeParameters.ProjectType
        '            fUpdate = True
        '        End If
        '        'If Not tPrj.isRiskAssociatedModel AndAlso tPrj.ProjectManager.PipeParameters.ProjectType = ProjectType.ptRiskAssociated Then
        '        '    tPrj.isRiskAssociatedModel = True
        '        '    fUpdate = True
        '        'End If
        '        ' D4978 ==
        '        If fUpdate Then DBProjectUpdate(tPrj, False, "Sync ProjectType")
        '    End If
        '    Return fUpdate
        'End Function
        '' D3438 ==

#End Region


#Region "Extras"

        ' D0464 ===
        Public Function DBExtrasByObjectID(ByVal tObjectID As Integer) As List(Of clsExtra)
            If Not dbExists() Then Return Nothing
            Dim tList As List(Of Dictionary(Of String, Object)) = Database.SelectFromTable(_TABLE_EXTRA, _FLD_EXTRA_OBJECTID, tObjectID)
            Dim tExtras As New List(Of clsExtra)
            For Each tRow As Dictionary(Of String, Object) In tList
                Dim tExtra As clsExtra = DBParse_Extra(tRow)
                tExtras.Add(tExtra)
            Next
            DebugInfo(String.Format("Loaded list of Extras with ObjectID #{0} ({1})", tObjectID, tExtras.Count))
            Return tExtras
        End Function

        Public Function DBExtrasByDetails(Optional ByVal tObjectID As Integer = Integer.MinValue, Optional ByVal tExtraType As ecExtraType = ecExtraType.Unspecified, Optional ByVal tExtraProperty As ecExtraProperty = ecExtraProperty.Unspecified) As List(Of clsExtra)  ' D2602
            If Not dbExists() Then Return Nothing

            ' D2602 ===
            Dim sSQL As String = String.Format("SELECT * FROM {0}", _TABLE_EXTRA)

            Dim tParams As New List(Of Object)

            Dim sSQLExtra As String = ""
            If tObjectID <> Integer.MinValue Then
                sSQLExtra += CStr(IIf(sSQLExtra = "", " WHERE", " AND")) + String.Format(" {0} = ?", _FLD_EXTRA_ID)
                tParams.Add(tObjectID)
            End If

            If tExtraType <> ecExtraType.Unspecified Then
                sSQLExtra += CStr(IIf(sSQLExtra = "", " WHERE", " AND")) + String.Format(" {0} = ?", _FLD_EXTRA_TYPEID)
                tParams.Add(CInt(tExtraType))
            End If

            If tExtraProperty <> ecExtraProperty.Unspecified Then
                sSQLExtra += CStr(IIf(sSQLExtra = "", " WHERE", " AND")) + String.Format(" {0} = ?", _FLD_EXTRA_PROPID)
                tParams.Add(CInt(tExtraProperty))
            End If

            Dim tList As List(Of Dictionary(Of String, Object)) = Database.SelectBySQL(sSQL + sSQLExtra, tParams)
            ' D2602 ==

            Dim tExtras As New List(Of clsExtra)
            For Each tRow As Dictionary(Of String, Object) In tList
                Dim tExtra As clsExtra = DBParse_Extra(tRow)
                tExtras.Add(tExtra)
            Next

            DebugInfo(String.Format("Loaded list of Extras with ObjectID #{0}, Type '{2}', Property '{3}' ({1})", tObjectID, tExtras.Count, tExtraType, tExtraProperty))
            Return tExtras
        End Function

        ' D0486 ===
        Public Function DBExtraRead(ByVal tExtra As clsExtra) As clsExtra   ' D0507
            If Not dbExists() OrElse Not isCanvasMasterDBValid Then Return Nothing  ' D7199
            Dim sSQL As String = String.Format("SELECT * FROM {0} WHERE {1}=? AND {2}=? AND {3}=?", _TABLE_EXTRA, _FLD_EXTRA_OBJECTID, _FLD_EXTRA_TYPEID, _FLD_EXTRA_PROPID)
            Dim tParams As New List(Of Object)
            ' D0507 ===
            tParams.Add(tExtra.ObjectID)
            tParams.Add(CInt(tExtra.ExtraType))
            tParams.Add(CInt(tExtra.ExtraProperty))
            Dim tList As List(Of Dictionary(Of String, Object)) = Database.SelectBySQL(sSQL, tParams)
            DebugInfo(String.Format("Loaded Extras with ObjectID #{0}, Type '{1}', Property '{2}'", tExtra.ObjectID, tExtra.ExtraType, tExtra.ExtraProperty))
            ' D0507 ==
            If tList.Count > 0 Then Return DBParse_Extra(tList(0)) Else Return Nothing
        End Function

        Public Function DBExtraWrite(ByVal tExtraInfo As clsExtra) As Boolean
            If Not dbExists() OrElse Not isCanvasMasterDBValid Then Return Nothing    ' D7199
            'Dim sSQL As String = String.Format("UPDATE {0} SET {4}=?{5} WHERE {1}=? AND {2}=? AND {3}=?", _TABLE_EXTRA, _FLD_EXTRA_OBJECTID, _FLD_EXTRA_TYPEID, _FLD_EXTRA_PROPID, _FLD_EXTRA_PROPVALUE, IIf(CanvasMasterDBVersion >= "0.9996", ", Changed=?", "")) ' D2214
            Dim sSQL As String = String.Format("UPDATE {0} SET {4}=? WHERE {1}=? AND {2}=? AND {3}=?", _TABLE_EXTRA, _FLD_EXTRA_OBJECTID, _FLD_EXTRA_TYPEID, _FLD_EXTRA_PROPID, _FLD_EXTRA_PROPVALUE) ' D2214 + D2259
            Dim tParams As New List(Of Object)
            tParams.Add(tExtraInfo.Value)
            'If CanvasMasterDBVersion >= "0.9996" Then tParams.Add(Now) ' D2214 - D2259
            tParams.Add(tExtraInfo.ObjectID)
            tParams.Add(CInt(tExtraInfo.ExtraType))
            tParams.Add(CInt(tExtraInfo.ExtraProperty))
            DebugInfo("Save Extra information...")
            Dim Fetched As Integer = Database.ExecuteSQL(sSQL, tParams)
            If Fetched = 0 Then
                'sSQL = String.Format("INSERT INTO {0} ({4}, {1}, {2}, {3}{5}) VALUES (?,?,?,?{6})", _TABLE_EXTRA, _FLD_EXTRA_OBJECTID, _FLD_EXTRA_TYPEID, _FLD_EXTRA_PROPID, _FLD_EXTRA_PROPVALUE, IIf(CanvasMasterDBVersion >= "0.9996", ", Changed", ""), IIf(CanvasMasterDBVersion >= "0.9996", ", ?", ""))  ' D2214
                sSQL = String.Format("INSERT INTO {0} ({4}, {1}, {2}, {3}) VALUES (?,?,?,?)", _TABLE_EXTRA, _FLD_EXTRA_OBJECTID, _FLD_EXTRA_TYPEID, _FLD_EXTRA_PROPID, _FLD_EXTRA_PROPVALUE)  ' D2214 + D2259
                Fetched = Database.ExecuteSQL(sSQL, tParams)
            End If
            Return Fetched > 0
        End Function
        ' D0486 ==

        ' D0487 ===
        Public Function DBExtraDelete(ByVal tExtraInfo As clsExtra) As Boolean
            If Not dbExists() Then Return Nothing
            Dim sSQL As String = String.Format("DELETE FROM {0} WHERE {1}=? AND {2}=? AND {3}=?", _TABLE_EXTRA, _FLD_EXTRA_OBJECTID, _FLD_EXTRA_TYPEID, _FLD_EXTRA_PROPID)
            Dim tParams As New List(Of Object)
            tParams.Add(tExtraInfo.ObjectID)
            tParams.Add(CInt(tExtraInfo.ExtraType))
            tParams.Add(CInt(tExtraInfo.ExtraProperty))
            DebugInfo("Delete Extra information...")
            Return Database.ExecuteSQL(sSQL, tParams) > 0
        End Function
        ' D0487 ==

        Public Function DBExtraByID(ByVal tID As Integer) As clsExtra
            If Not dbExists() Then Return Nothing
            Dim tList As List(Of Dictionary(Of String, Object)) = Database.SelectFromTable(_TABLE_EXTRA, _FLD_EXTRA_ID, tID)
            DebugInfo(String.Format("Loaded Extra with ID #{0}", tID))
            If tList.Count > 0 Then Return DBParse_Extra(tList(0)) Else Return Nothing
        End Function
        ' D0464 ==

#End Region


#Region "TeamTimeData"

        Public Function DBTeamTimeDataRead(ByVal tProjectId As Integer, ByVal tUserID As Integer, ByVal tObjectID As ecExtraProperty, ByRef DT As Nullable(Of DateTime)) As String
            If Not dbExists() Then Return Nothing
            If CanvasMasterDBVersion >= "0.996" Then
                Dim sSQL As String = String.Format("SELECT * FROM {0} WHERE ProjectID=? AND UserID=? AND ObjectID=?", _TABLE_TEAMTIMEDATA)
                Dim tParams As New List(Of Object)
                tParams.Add(tProjectId)
                tParams.Add(tUserID)
                tParams.Add(CInt(tObjectID))
                Dim tList As List(Of Dictionary(Of String, Object)) = Database.SelectBySQL(sSQL, tParams)
                DebugInfo(String.Format("Loaded TeamTime data for ProjectID #{0}, UserID #{1}, Property '{2}'", tProjectId, tUserID, tObjectID))
                If tList.Count > 0 Then
                    Dim tRow As Dictionary(Of String, Object) = tList(0)
                    If tRow.ContainsKey("DT") AndAlso Not IsDBNull(tRow("DT")) Then DT = CDate(tRow("DT"))
                    If tRow.ContainsKey("Data") AndAlso Not IsDBNull(tRow("Data")) Then Return CStr(tRow("Data"))
                End If
            End If
            Return Nothing
        End Function

        Public Function DBTeamTimeDataReadAll(ByVal tProjectId As Integer, ByVal tObjectID As ecExtraProperty, Optional UserID As Integer = Integer.MinValue, Optional EraseLoadedRows As Boolean = False) As List(Of String)    ' D4824
            If Not dbExists() Then Return Nothing
            Dim tRows As List(Of String) = Nothing
            If CanvasMasterDBVersion >= "0.996" Then
                Dim sSQL As String = String.Format("SELECT * FROM {0} WHERE ProjectID=? AND ObjectID=?{1} ORDER BY DT ASC", _TABLE_TEAMTIMEDATA, If(UserID = Integer.MinValue, "", " AND UserID=?"))  ' D4824
                Dim tParams As New List(Of Object)
                tParams.Add(tProjectId)
                tParams.Add(CInt(tObjectID))
                If UserID <> Integer.MinValue Then tParams.Add(UserID)    ' D4824
                Dim tList As List(Of Dictionary(Of String, Object)) = Database.SelectBySQL(sSQL, tParams)
                DebugInfo(String.Format("Loaded TeamTime data rows for ProjectID #{0}, Property '{1}'{2}", tProjectId, tObjectID, If(UserID = Integer.MinValue, "", ", UserID=" + UserID.ToString))) ' D4824
                tRows = New List(Of String)
                Dim sIDs As String = ""  ' D4824
                For Each tRow As Dictionary(Of String, Object) In tList
                    If tRow.ContainsKey("Data") AndAlso Not IsDBNull(tRow("Data")) Then tRows.Add(CStr(tRow("Data")))
                    If tRow.ContainsKey("ID") Then sIDs += String.Format("{0}{1}", If(sIDs = "", "", ","), CInt(tRow("ID")))    ' D4824
                Next
                ' D4824 ===
                If EraseLoadedRows AndAlso sIDs <> "" Then
                    Dim fDeleted As Integer = Database.ExecuteSQL(String.Format("DELETE FROM {0} WHERE ID IN ({1})", _TABLE_TEAMTIMEDATA, sIDs))   ' D4824
                End If
                ' D4824 ==
            End If
            Return tRows
        End Function

        Public Function DBTeamTimeDataWrite(ByVal tProjectId As Integer, ByVal tUserID As Integer, ByVal tObjectID As ecExtraProperty, ByVal sData As String, Optional ByVal fReplaceExisting As Boolean = True) As Boolean
            If Not dbExists() Then Return Nothing
            If CanvasMasterDBVersion >= "0.996" Then
                Dim sSQL As String = String.Format("UPDATE {0} Set Data=?, DT=? WHERE ProjectID=? And UserID=? And ObjectID=?", _TABLE_TEAMTIMEDATA)
                Dim tParams As New List(Of Object)
                tParams.Add(sData)
                tParams.Add(Now)
                tParams.Add(tProjectId)
                tParams.Add(tUserID)
                tParams.Add(CInt(tObjectID))
                DebugInfo("Save TeamTime data...")
                Dim Fetched As Integer = 0
                If fReplaceExisting Then Fetched = Database.ExecuteSQL(sSQL, tParams)
                If Fetched = 0 Then
                    sSQL = String.Format("INSERT INTO {0} (Data, DT, ProjectID, UserID, ObjectID) VALUES (?,?,?,?,?)", _TABLE_TEAMTIMEDATA)
                    Fetched = Database.ExecuteSQL(sSQL, tParams)
                End If
                Return Fetched > 0
            End If
            Return False
        End Function

        Public Function DBTeamTimeDataDelete(ByVal tProjectID As Integer, ByVal tMaxDT As Nullable(Of DateTime), Optional ByVal tObjectID As ecExtraProperty = ecExtraProperty.Unspecified, Optional tUserID As Integer = Integer.MinValue, Optional ByVal tID As Integer = -1) As Boolean  ' D4824
            If Not dbExists() Then Return Nothing
            If CanvasMasterDBVersion >= "0.996" Then
                Dim sSQL As String = String.Format("DELETE FROM {0} WHERE ProjectID=?", _TABLE_TEAMTIMEDATA)    ' D4274 + D4439
                Dim tParams As New List(Of Object)
                tParams.Add(tProjectID)
                If tMaxDT IsNot Nothing AndAlso tMaxDT.HasValue Then
                    sSQL += " And DT<=?"
                    tParams.Add(tMaxDT)
                End If
                If tObjectID <> ecExtraProperty.Unspecified Then
                    sSQL += " And ObjectID=?"
                    tParams.Add(CInt(tObjectID))
                End If
                ' D4824 ===
                If tUserID <> Integer.MinValue Then
                    sSQL += " And UserID=?"
                    tParams.Add(tUserID)
                End If
                ' D4824 ==
                If tID > 0 Then
                    sSQL += " And ID=?"
                    tParams.Add(tID)
                End If
                ' D4439 ===
                If tParams.Count = 1 Then
                    sSQL += " Or (DT<DATEADD(month, -1, SYSDATETIME()))"
                End If
                ' D4439 ==
                DebugInfo("Delete TeamTime data...")
                Return Database.ExecuteSQL(sSQL, tParams) > 0
            End If
            Return False
        End Function
        ' D1275 ==

        ' D4274 ===
        Public Function DBTeamTimeSessionAppID(ByVal tProjectID As Integer) As ecAppliationID
            Dim tRes As ecAppliationID = ecAppliationID.appUnknown
            If CanvasMasterDBVersion >= "0.996" Then
                Dim sSQL As String = String.Format("Select * FROM {0} WHERE ProjectID=? And ObjectID =? ORDER BY DT DESC", _TABLE_TEAMTIMEDATA)
                Dim tParams As New List(Of Object)
                tParams.Add(tProjectID)
                tParams.Add(CInt(ecExtraProperty.TeamTimeSessionAppID))
                Dim tList As List(Of Dictionary(Of String, Object)) = Database.SelectBySQL(sSQL, tParams)
                DebugInfo(String.Format("Loaded TeamTime data row for ProjectID #{0}, Property '{1}'", tProjectID, ecExtraProperty.TeamTimeSessionAppID.ToString))
                If tList.Count > 0 Then
                    If tList(0).ContainsKey("Data") AndAlso Not IsDBNull(tList(0)("Data")) Then
                        Dim sVal As String = CStr(tList(0)("Data"))
                        Dim tID As Integer = -1
                        If Integer.TryParse(sVal, tID) Then tRes = CType(tID, ecAppliationID)
                    End If
                End If
            End If
            Return tRes
        End Function
        ' D1275 ==

#End Region


#Region "Create, copy and Upload decisions"

        ' D0478 ===
        Public Function DBProjectCreate(ByRef tProject As clsProject, Optional ByVal sLogMessage As String = "") As Boolean  ' D0046
            If Not dbExists() Then Return Nothing
            If tProject Is Nothing Then Return False ' D0218
            Dim fResult As Boolean = False  ' D0253
            Dim sError As String = ""   ' D0046
            tProject.PasscodeLikelihood = ProjectUniquePasscode(tProject.PasscodeLikelihood, -1)    ' D1709
            tProject.PasscodeImpact = ProjectUniquePasscode(tProject.PasscodeImpact, -1)            ' D1709
            tProject.ConnectionString = CanvasProjectsConnectionDefinition.ConnectionString ' D0478
            fResult = DBProjectUpdate(tProject, True, sLogMessage)
            If Not fResult AndAlso sError <> "" Then DBSaveLog(dbActionType.actCreate, dbObjectType.einfProject, tProject.ID, sLogMessage, String.Format("Error: {0}", sError)) ' D0496
            Return fResult
        End Function
        ' D0478 ==

        ' D0479 ===
        Public Function DBProjectStreamDelete(ByVal ProjectID As Integer) As Boolean    ' D2603
            If Not dbExists() Then Return Nothing
            Dim fResult As Boolean = False
            If Database.Connect Then
                Dim affected As Integer = Database.ExecuteSQL(String.Format("DELETE FROM ModelStructure WHERE ProjectID={0}", ProjectID))
                DebugInfo(String.Format("Delete ModelStructure stream data for Project #{0}, affected rows: {1}", ProjectID, affected))
                affected = Database.ExecuteSQL(String.Format("DELETE FROM UserData WHERE ProjectID={0}", ProjectID))
                DebugInfo(String.Format("Delete UserData stream data for Project #{0}, affected rows: {1}", ProjectID, affected))
                fResult = True
            End If
            Return fResult
        End Function

        Public Function DBProjectDelete(ByVal tProject As clsProject, ByVal fDeleteDecisionData As Boolean) As Boolean
            If Not dbExists() Then Return Nothing
            Dim fResult As Boolean = False ' D0508
            If Database.Connect AndAlso Not tProject Is Nothing Then
                tProject.ResetProject(False)    ' D0253
                fResult = True      ' D0508
                Dim DeletedUW As Integer = 0    ' D0830
                Dim DeletedUsers As Integer = 0 ' D0830

                If fDeleteDecisionData Then fResult = DBProjectStreamDelete(tProject.ID) ' D0369 + D0479

                If fResult Then
                    ' D0508 ===
                    Dim Tr As DbTransaction = Nothing    ' D0041
                    Try
                        DebugInfo(String.Format("Delete Project '{0}' (passcode: {1})", tProject.ProjectName, tProject.Passcode))

                        'Dim tWSList As List(Of clsWorkspace) = DBWorkspacesByProjectID(tProject.ID)

                        Dim sSQL As String = String.Format("DELETE FROM {0} WHERE {1}={2}", _TABLE_WORKSPACE, _FLD_WORKSPACE_PRJID, tProject.ID)

                        Dim oCommand As DbCommand = Database.SQL(sSQL)
                        Tr = oCommand.Connection.BeginTransaction
                        oCommand.Transaction = Tr

                        DBExecuteNonQuery(Database.ProviderType, oCommand)

                        sSQL = String.Format("DELETE FROM {0} WHERE {1}={2} AND {3} IN ({4},{5})", _TABLE_EXTRA, _FLD_EXTRA_OBJECTID, tProject.ID, _FLD_EXTRA_TYPEID, CInt(ecExtraType.Project), CInt(ecExtraType.TeamTime))
                        oCommand.CommandText = sSQL
                        DBExecuteNonQuery(Database.ProviderType, oCommand)

                        ' D1231 ===
                        sSQL = String.Format("DELETE FROM StructureTokens WHERE MeetingID IN (SELECT MeetingID FROM StructureMeetings WHERE ProjectID = {0})", tProject.ID)
                        oCommand.CommandText = sSQL
                        DBExecuteNonQuery(Database.ProviderType, oCommand)

                        sSQL = String.Format("DELETE FROM StructureMeetings WHERE ProjectID={0}", tProject.ID)
                        oCommand.CommandText = sSQL
                        DBExecuteNonQuery(Database.ProviderType, oCommand)
                        ' D1231 ==

                        sSQL = String.Format("DELETE FROM {0} WHERE {1}={2}", _TABLE_PROJECTS, _FLD_PROJECTS_ID, tProject.ID)
                        oCommand.CommandText = sSQL
                        DBExecuteNonQuery(Database.ProviderType, oCommand)

                        ' D3599 ===
                        If isSnapshotsAvailable() Then
                            sSQL = String.Format("DELETE FROM {0} WHERE {1}={2}", _TABLE_SNAPSHOTS, _FLD_SNAPSHOTS_PRJ_ID, tProject.ID)
                            oCommand.CommandText = sSQL
                            DBExecuteNonQuery(Database.ProviderType, oCommand)
                        End If
                        ' D3599 ==

                        ' D0829 ===
                        If _CLEAN_UP_USERS_ON_DELETE Then
                            Dim sRoleGroups As String = ""
                            Dim tWkg As clsWorkgroup = ActiveWorkgroup
                            If tWkg Is Nothing Or tProject.WorkgroupID <> tWkg.ID Then tWkg = DBWorkgroupByID(tProject.WorkgroupID, True, True)
                            If tWkg IsNot Nothing Then

                                For Each tRole As clsRoleGroup In tWkg.RoleGroups
                                    If tRole.RoleLevel = ecRoleLevel.rlApplicationLevel AndAlso
                                       tRole.ActionStatus(ecActionType.at_alCreateNewModel) <> ecActionStatus.asGranted AndAlso
                                       tRole.ActionStatus(ecActionType.at_alDeleteAnyModel) <> ecActionStatus.asGranted AndAlso
                                       tRole.ActionStatus(ecActionType.at_alManageAnyModel) <> ecActionStatus.asGranted AndAlso
                                       tRole.ActionStatus(ecActionType.at_alManageWorkgroupRights) <> ecActionStatus.asGranted AndAlso
                                       tRole.ActionStatus(ecActionType.at_alManageWorkgroupUsers) <> ecActionStatus.asGranted AndAlso
                                       tRole.ActionStatus(ecActionType.at_alUploadModel) <> ecActionStatus.asGranted Then sRoleGroups += CStr(IIf(sRoleGroups = "", "", ",")) + tRole.ID.ToString
                                    'tRole.ActionStatus(ecActionType.at_alManageSurveys) <> ecActionStatus.asGranted AndAlso _
                                Next

                                If sRoleGroups <> "" Then
                                    DebugInfo("Check for workspaces, who doesn't have linked decisions in this workgroup")
                                    sSQL = String.Format("DELETE {1} FROM {1} LEFT JOIN {2} ON {2}.{3} = {1}.{4} WHERE {5}={6} AND ({7} IN ({8})) AND {2}.ID IS NULL", _FLD_USERWRKG_ID, _TABLE_USERWORKGROUPS, _TABLE_WORKSPACE, _FLD_WORKSPACE_USERID, _FLD_USERWRKG_USERID, _FLD_USERWRKG_WRKGID, tProject.WorkgroupID, _FLD_USERWRKG_ROLEID, sRoleGroups)
                                    oCommand.CommandText = sSQL
                                    DeletedUW = DBExecuteNonQuery(Database.ProviderType, oCommand)
                                    DebugInfo("Deleted items: {0}", CStr(DeletedUW))  ' D0830

                                    ' D0830 ===
                                    If DeletedUW > 0 Then
                                        DebugInfo("Check for users, who doesn't have linked workgroups")
                                        sSQL = String.Format("DELETE {0} FROM {0} LEFT JOIN {1} ON {0}.{2} = {1}.{3} WHERE ({1}.{3} IS NULL) AND ({0}.{4} IS NULL)", _TABLE_USERS, _TABLE_USERWORKGROUPS, _FLD_USERS_ID, _FLD_USERWRKG_USERID, _FLD_USERS_LASTVISITED)
                                        oCommand.CommandText = sSQL
                                        DeletedUsers = DBExecuteNonQuery(Database.ProviderType, oCommand)
                                        DebugInfo("Deleted users: {0}", DeletedUsers.ToString)
                                    End If
                                    ' D0830 ==
                                End If
                            End If
                        End If
                        ' D0829 ==

                        Tr.Commit()
                        oCommand = Nothing
                        fResult = True

                    Catch ex As Exception
                        DBSaveLog(dbActionType.actDelete, dbObjectType.einfProject, tProject.ID, String.Format("Delete Project '{0}'", tProject.Passcode), "Error: " + ex.Message)
                        If Not Tr Is Nothing Then Tr.Rollback()
                        fResult = False
                    Finally
                        If Not Tr Is Nothing Then Tr = Nothing
                        If DeletedUW > 0 Then DBSaveLog(dbActionType.actDelete, dbObjectType.einfWorkspace, -1, String.Format("Delete {0} unused user workgroups on project delete", DeletedUW), "") ' D0830
                        If DeletedUsers > 0 Then DBSaveLog(dbActionType.actDelete, dbObjectType.einfUser, -1, String.Format("Delete {0} users without any linked workgroup on project delete", DeletedUsers), "") ' D0830
                        DBSaveLog(dbActionType.actDelete, dbObjectType.einfProject, tProject.ID, String.Format("Delete Project '{0}'{1}", tProject.Passcode, IIf(tProject.LastVisited.HasValue AndAlso tProject.LastVisited.Value.AddDays(WipeoutProjectsTimeout) < Now, " as outdated", "")), "")  ' D0830 + D2658

                    End Try
                    ' D0508 ==

                    If fResult Then

                        DBTinyURLDelete(-1, tProject.ID, -1)    ' D0899

                        ' D0513 ===
                        If ProjectID = tProject.ID Then ProjectID = -1
                        If Not _ProjectsList Is Nothing Then
                            Dim existPrj As clsProject = clsProject.ProjectByID(tProject.ID, ActiveProjectsList)
                            If Not existPrj Is Nothing Then
                                ActiveProjectsList.Remove(existPrj)
                                If Not _Workspaces Is Nothing AndAlso Not ActiveUser Is Nothing Then
                                    Dim existWS As clsWorkspace = clsWorkspace.WorkspaceByUserIDAndProjectID(ActiveUser.UserID, tProject.ID, Workspaces)
                                    If Not existWS Is Nothing Then Workspaces.Remove(existWS)
                                End If
                            End If
                        End If
                        ' D0513 ==

                    End If
                End If

            End If
            Return fResult
        End Function

        ''' <summary>
        ''' Create project database from file.
        ''' </summary>
        ''' <param name="tProject">Reference to object with new Project data. Must be provided. Fields Filename, Passcode and ProjectName will not be updated and should be filled out of this call.</param>
        ''' <param name="sFilename">Existed file with AHP-decision.</param>
        ''' <param name="sErrorMessage">Reference to string for save error details.</param>
        ''' <returns>True, when database created.</returns>
        ''' <remarks>When error occurred, all created database and tables will be automatically deleted (rollback). Transaction used.</remarks>
        Public Function DBProjectCreateFromCanvasFile(ByRef tProject As clsProject, ByVal sFilename As String, ByRef sErrorMessage As String) As Boolean ' D0070 +  D0077 +  D0092
            If Not dbExists() Then Return Nothing
            Dim fUploaded As Boolean = False
            ' D0130 ===
            If Not tProject Is Nothing Then
                Dim ODBCConnString As String = clsDatabaseAdvanced.GetConnectionString(CanvasProjectsConnectionDefinition.DBName, DBProviderType.dbptODBC) ' D0315 + D0330 + D0458 + D0479
                fUploaded = clsDatabaseAdvanced.CopyJetToDatabase(sFilename, ODBCConnString, sErrorMessage)    ' D0330 + D0479
                If Not fUploaded Then DBProjectDelete(tProject, True)
            End If
            ' D0130 ==
            Return fUploaded
        End Function
        ' D0032 ==

        'C0271===
        Public Function DBProjectCreateFromAHPXFile(ByRef tProject As clsProject, ByVal sFileName As String, ByRef sErrorMessage As String, Optional ByVal XMLPipePramsFileName As String = "") As Boolean
            If Not dbExists() Then Return Nothing
            Dim fUploaded As Boolean = False
            If Not tProject Is Nothing Then
                Try
                    Dim sAHPXFileConnStringODBC As String = clsConnectionDefinition.BuildJetConnectionDefinition(sFileName, DBProviderType.dbptODBC).ConnectionString    ' D0166 + D0315 + D0329 + D0330 + MF0332 + D0348
                    Dim AHPXProject As New clsProjectManager(False,,)
                    AHPXProject.StorageManager.StorageType = ECModelStorageType.mstCanvasDatabase 'C0028
                    AHPXProject.StorageManager.ProviderType = DBProviderType.dbptODBC    ' D0329
                    AHPXProject.StorageManager.ProjectLocation = sAHPXFileConnStringODBC 'C0028

                    'Dim dbVersion As ECCanvasDatabaseVersion = GetDBVersion_CanvasStreamDatabase(sAHPXFileConnStringODBC, DBProviderType.dbptODBC) 'C0275
                    Dim dbVersion As ECCanvasDatabaseVersion = GetCurrentDBVersion()
                    AHPXProject.StorageManager.CanvasDBVersion = dbVersion 'C0275

                    If AHPXProject.StorageManager.Reader.LoadProject() Then 'C0028
                        'C0052===
                        Dim PipeParams As New clsPipeParamaters
                        ' D0162 ===
                        If Not String.IsNullOrEmpty(XMLPipePramsFileName) Then
                            If My.Computer.FileSystem.FileExists(XMLPipePramsFileName) And Not tProject Is Nothing Then
                                PipeParams.Read(PipeStorageType.pstXMLFile, XMLPipePramsFileName, AHPXProject.StorageManager.ProviderType, tProject.ID) 'C0390
                            End If
                        End If

                        ' D0523 ===
                        AHPXProject.PipeParameters.TableName = PipeParameters.PROPERTIES_DEFAULT_TABLE_NAME
                        AHPXProject.PipeParameters.PropertyNameColumnName = PipeParameters.PROPERTY_NAME_DEFAULT_DB_COLUMN_NAME
                        AHPXProject.PipeParameters.PropertyValueColumnName = PipeParameters.PROPERTY_VALUE_DEFAULT_DB_COLUMN_NAME
                        ' D0523 ==

                        ' D0162 ==
                        PipeParams.Read(PipeStorageType.pstDatabase, sAHPXFileConnStringODBC, AHPXProject.StorageManager.ProviderType, tProject.ID) 'C0390
                        PipeParams.PipeMessages.Load(PipeStorageType.pstDatabase, sAHPXFileConnStringODBC, AHPXProject.StorageManager.ProviderType, tProject.ID)   ' D0329 'C0420
                        'C0052==

                        AHPXProject.AntiguaDashboard.LoadPanel(AHPXProject.StorageManager.StorageType, sAHPXFileConnStringODBC, AHPXProject.StorageManager.ProviderType, -1) 'C0783
                        AHPXProject.AntiguaRecycleBin.LoadPanel(AHPXProject.StorageManager.StorageType, sAHPXFileConnStringODBC, AHPXProject.StorageManager.ProviderType, -1) 'C0783

                        ParseInfodocsRTF2MHT(AHPXProject, PipeParams) ' D0151 + D0174

                        AHPXProject.StorageManager.StorageType = clsProject.StorageType ' D0479
                        AHPXProject.StorageManager.ProviderType = tProject.ProviderType  ' D0329
                        AHPXProject.StorageManager.ProjectLocation = tProject.ConnectionString 'C0028
                        'If AHPProject.StorageManager.SaveProject(ECModelStorageType.mstCanvasDatabase, tProject.GetConnectionString) Then 'C0028
                        AHPXProject.StorageManager.CanvasDBVersion = GetCurrentDBVersion()
                        AHPXProject.StorageManager.ModelID = tProject.ID
                        If AHPXProject.StorageManager.Writer.SaveProject() Then 'C0028
                            'C0052===
                            Dim fPipeType As PipeStorageType = clsProject.PipeStorageType    ' D0376 + D0487
                            PipeParams.ProjectVersion = AHPXProject.StorageManager.CanvasDBVersion 'C0462
                            PipeParams.Write(fPipeType, AHPXProject.StorageManager.ProjectLocation, AHPXProject.StorageManager.ProviderType, tProject.ID)    ' D0329 + D0369 + D0376
                            PipeParams.PipeMessages.Save(fPipeType, AHPXProject.StorageManager.ProjectLocation, AHPXProject.StorageManager.ProviderType, tProject.ID)    ' D0329 + D0369 + D0376 'C0420
                            'C0052==

                            AHPXProject.AntiguaDashboard.SavePanel(AHPXProject.StorageManager.StorageType, AHPXProject.StorageManager.ProjectLocation, AHPXProject.StorageManager.ProviderType, tProject.ID) 'C0783
                            AHPXProject.AntiguaRecycleBin.SavePanel(AHPXProject.StorageManager.StorageType, AHPXProject.StorageManager.ProjectLocation, AHPXProject.StorageManager.ProviderType, tProject.ID) 'C0783

                            fUploaded = True
                        End If
                        AHPXProject = Nothing
                    End If
                Catch ex As Exception
                    If Not sErrorMessage Is Nothing Then
                        sErrorMessage = ex.Message
                    End If
                    DebugInfo(ex.Message, _TRACE_RTE)   ' D0330
                End Try
                If Not fUploaded Then DBProjectDelete(tProject, True)
            End If
            Return fUploaded
        End Function
        'C0271==

        ' D0070 ===
        Public Function DBProjectCreateFromAHPFile(ByRef tProject As clsProject, ByVal sFileName As String, ByRef sErrorMessage As String, Optional ByVal XMLPipePramsFileName As String = "") As Boolean
            If Not dbExists() Then Return Nothing
            Dim fUploaded As Boolean = False
            If Not tProject Is Nothing Then
                Try
                    Dim sAHPFileConnStringODBC As String = clsConnectionDefinition.BuildJetConnectionDefinition(sFileName, DBProviderType.dbptODBC).ConnectionString    ' D0166 + D0315 + D0329 + D0330 + MF0332 + D0348
                    Dim AHPProject As New clsProjectManager(False, False, isRiskEnabled)    ' D0376 + D2255
                    AHPProject.StorageManager.StorageType = ECModelStorageType.mstAHPDatabase 'C0028
                    AHPProject.StorageManager.ProviderType = DBProviderType.dbptODBC    ' D0329
                    AHPProject.StorageManager.ProjectLocation = sAHPFileConnStringODBC 'C0028
                    If AHPProject.StorageManager.AHPReader.LoadProject() Then 'C0028
                        'C0052===
                        Dim PipeParams As New clsPipeParamaters
                        ' D0162 ===
                        If Not String.IsNullOrEmpty(XMLPipePramsFileName) Then
                            If My.Computer.FileSystem.FileExists(XMLPipePramsFileName) And Not tProject Is Nothing Then
                                PipeParams.Read(PipeStorageType.pstXMLFile, XMLPipePramsFileName, AHPProject.StorageManager.ProviderType, tProject.ID) 'C0390
                            End If
                        End If

                        ' D0523 + D0531 ===
                        PipeParams.TableName = "MProperties"
                        PipeParams.PropertyNameColumnName = "PropertyName"
                        PipeParams.PropertyValueColumnName = "PValue"
                        ' D0523 + D0531 ==

                        ' D0162 ==
                        PipeParams.Read(PipeStorageType.pstDatabase, sAHPFileConnStringODBC, AHPProject.StorageManager.ProviderType, tProject.ID) 'C0390
                        PipeParams.PipeMessages.Load(PipeStorageType.pstDatabase, sAHPFileConnStringODBC, AHPProject.StorageManager.ProviderType, tProject.ID)   ' D0329 'C0420
                        'C0052==


                        AHPProject.AntiguaDashboard.LoadPanel(AHPProject.StorageManager.StorageType, sAHPFileConnStringODBC, AHPProject.StorageManager.ProviderType, -1) 'C0783
                        AHPProject.AntiguaRecycleBin.LoadPanel(AHPProject.StorageManager.StorageType, sAHPFileConnStringODBC, AHPProject.StorageManager.ProviderType, -1) 'C0783

                        AHPProject.Attributes.ReadAttributeValues(AttributesStorageType.astDatabase, sAHPFileConnStringODBC, AHPProject.StorageManager.ProviderType, -1)

                        ECCore.MiscFuncs.LoadSurveysFromAHPFile(sAHPFileConnStringODBC, AHPProject.StorageManager.ProviderType, tProject.ConnectionString, tProject.ProviderType, tProject.ID)

                        ParseInfodocsRTF2MHT(AHPProject, PipeParams) ' D0151 + D0174

                        AHPProject.StorageManager.StorageType = clsProject.StorageType   ' D0479
                        AHPProject.StorageManager.ProviderType = tProject.ProviderType  ' D0329
                        AHPProject.StorageManager.ProjectLocation = tProject.ConnectionString 'C0028
                        AHPProject.StorageManager.CanvasDBVersion = AHPProject.StorageManager.CurrentCanvasDBVersion
                        AHPProject.StorageManager.ModelID = tProject.ID 'C0271
                        If AHPProject.StorageManager.Writer.SaveProject() Then 'C0028
                            'C0052===
                            Dim fPipeType As PipeStorageType = clsProject.PipeStorageType   ' D0376
                            PipeParams.ProjectVersion = AHPProject.StorageManager.CanvasDBVersion 'C0462
                            PipeParams.Write(fPipeType, AHPProject.StorageManager.ProjectLocation, AHPProject.StorageManager.ProviderType, tProject.ID) 'C0271 + D0369 + D0376
                            PipeParams.PipeMessages.Save(fPipeType, AHPProject.StorageManager.ProjectLocation, AHPProject.StorageManager.ProviderType, tProject.ID) 'C0271 + D0369 + D0376 'C0420
                            'C0052==

                            AHPProject.AntiguaDashboard.SavePanel(AHPProject.StorageManager.StorageType, AHPProject.StorageManager.ProjectLocation, AHPProject.StorageManager.ProviderType, tProject.ID) 'C0783
                            AHPProject.AntiguaRecycleBin.SavePanel(AHPProject.StorageManager.StorageType, AHPProject.StorageManager.ProjectLocation, AHPProject.StorageManager.ProviderType, tProject.ID) 'C0783

                            AHPProject.Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, AHPProject.StorageManager.ProjectLocation, AHPProject.StorageManager.ProviderType, tProject.ID, -1)
                            AHPProject.ResourceAligner.Save()
                            AHPProject.ResourceAligner = Nothing
                            fUploaded = True
                        End If
                        AHPProject = Nothing
                    End If
                Catch ex As Exception
                    If Not sErrorMessage Is Nothing Then
                        sErrorMessage = ex.Message
                    End If
                    DebugInfo(ex.Message, _TRACE_RTE)   ' D0330
                End Try
                If Not fUploaded Then DBProjectDelete(tProject, True)
            End If
            Return fUploaded
        End Function
        ' D0070 ==

        Public Function DBProjectCopy(ByRef SrcProject As clsProject, ByVal SrcStorageType As ECModelStorageType, ByVal DestConnString As String, ByVal DestProviderType As DBProviderType, ByVal DestProjectID As Integer, fCopySnapshots As Boolean, Optional sObjCleanUpName As String = Nothing, Optional sAltCleanUpName As String = Nothing, Optional sObjImpactCleanUpName As String = Nothing, Optional ByVal PerformUserEmailsCleanup As Boolean = False, Optional ByVal PerformUserNamesCleanup As Boolean = False, Optional SaveAsDBVersion As Integer = -1) As Boolean 'C0720 + D1182 + D1186 + D2145 + D3432 + D3774
            If Not dbExists() Then Return Nothing
            Dim fResult As Boolean = False
            If SrcProject Is Nothing Then Return fResult ' D0329 + D0458
            If Not SrcProject.LockInfo.isLockAvailable(ActiveUser) Then Return fResult ' D0483 + D0589
            If Not SrcProject.isDBVersionCanBeUpdated Then Return fResult ' D3549
            Dim sError As String = ""
            Dim fNeedUnLock As Boolean = Not ActiveUser Is Nothing AndAlso SrcProject.LockInfo.isLockAvailable(ActiveUser.UserID)  ' D0483 + D0494 + D0513 + D0641
            If Not ActiveUser Is Nothing Then DBProjectLockInfoWrite(ECLockStatus.lsLockForSystem, SrcProject.LockInfo, ActiveUser, Nothing) ' D0483 + D0513 + D0589

            ' D0183 ===
            DebugInfo("Start copy model info…") ' D0827

            'Dim PrjManager As New clsProjectManager(False)    ' D0376 'C0808
            Dim PrjManager As New clsProjectManager(True, False, SrcProject.IsRisk) 'C0808 + D2255

            PrjManager.StorageManager.StorageType = SrcStorageType ' D0479
            PrjManager.StorageManager.ProviderType = SrcProject.ProviderType    ' D0329
            PrjManager.StorageManager.ProjectLocation = SrcProject.ConnectionString ' D0329
            PrjManager.StorageManager.ModelID = SrcProject.ID 'C0278 'C0400
            'PrjManager.UseDataMapping = SrcProject.UseDataMapping   ' D4633
            PrjManager.UseDataMapping = True ' D6228 // Force to load and save data mapping streams not depend on settings
            PrjManager.StorageManager.ReadDBVersion() 'C0039 'C0278 + D0394 'C0400

            DebugInfo("Load original model")    ' D0827
            Dim fStreamsCopied As Boolean = False   ' D1621

            If SaveAsDBVersion < 16 OrElse SaveAsDBVersion > GetCurrentDBVersion.MinorVersion Then SaveAsDBVersion = GetCurrentDBVersion.MinorVersion ' D2145

            If PrjManager.StorageManager.Reader.LoadProject() Then  ' D0394

                Dim WriteVersion As ECCanvasDatabaseVersion = GetCurrentDBVersion() ' D2151
                WriteVersion.MinorVersion = SaveAsDBVersion ' D2151

                If IsEqualCanvasDBVersions(SrcProject.DBVersion, GetCurrentDBVersion) AndAlso SaveAsDBVersion = GetCurrentDBVersion.MinorVersion Then ' D1597 + D2151

                    ' Just copy streams from UserData as is
                    ' D1594 ===
                    Dim sSQL As String = String.Format("INSERT INTO UserData (ProjectID, UserID, DataType, StreamSize, Stream, ModifyDate) SELECT  {0}, UserID, DataType, StreamSize, Stream, ModifyDate FROM UserData WHERE ProjectID={1}", DestProjectID, SrcProject.ID)
                    Database.ResetError()   ' D1597
                    Database.ExecuteSQL(sSQL)
                    fResult = Database.LastError = ""   ' D3361
                    If Not fResult Then DBSaveLog(dbActionType.actCreate, dbObjectType.einfProject, Math.Abs(DestProjectID), "Unable to save project copy", Database.LastError) ' D3359
                    fStreamsCopied = True   ' D1621
                    ' D1594 ==

                    If fCopySnapshots Then DBSnapshotsCopy(SrcProject.ID, DestProjectID) ' D3774

                Else

                    fCopySnapshots = False  ' D3774

                    ' Process every participant when project version is outdated

                    PrjManager.Attributes.ReadAttributes(AttributesStorageType.astStreamsDatabase, SrcProject.ConnectionString, SrcProject.ProviderType, SrcProject.ID)

                    'C0808===
                    Dim i As Integer = 1
                    Dim count As Integer = PrjManager.UsersList.Count

                    For Each user As clsUser In PrjManager.UsersList
                        'Debug.Print("Processing user data (" + i.ToString + "/" + count.ToString + ": " + user.UserEMail)
                        i += 1

                        PrjManager.StorageManager.StorageType = SrcStorageType
                        PrjManager.StorageManager.ProviderType = SrcProject.ProviderType
                        PrjManager.StorageManager.ProjectLocation = SrcProject.ConnectionString
                        PrjManager.StorageManager.ModelID = SrcProject.ID
                        PrjManager.StorageManager.ReadDBVersion()

                        PrjManager.StorageManager.Reader.LoadUserData(user)
                        Dim LJT As DateTime
                        PrjManager.StorageManager.Reader.LoadUserJudgmentsControls(LJT, user)
                        PrjManager.StorageManager.Reader.LoadUserControlsPermissions(user.UserID)

                        PrjManager.Attributes.ReadAttributeValues(AttributesStorageType.astStreamsDatabase, SrcProject.ConnectionString, SrcProject.ProviderType, SrcProject.ID, user.UserID)

                        PrjManager.StorageManager.StorageType = clsProject.StorageType
                        PrjManager.StorageManager.ProviderType = DestProviderType
                        PrjManager.StorageManager.ProjectLocation = DestConnString
                        PrjManager.StorageManager.CanvasDBVersion = WriteVersion    ' D2151
                        PrjManager.StorageManager.ModelID = DestProjectID
                        PrjManager.PipeParameters.ProjectVersion = PrjManager.StorageManager.CanvasDBVersion

                        PrjManager.StorageManager.Writer.SaveUserJudgments(user, user.LastJudgmentTime)  ' D1597
                        PrjManager.StorageManager.Writer.SaveUserPermissions(user)
                        PrjManager.StorageManager.Writer.SaveUserDisabledNodes(user)

                        PrjManager.StorageManager.Writer.SaveUserJudgmentsControls(user)
                        PrjManager.StorageManager.Writer.SaveUserControlsPermissions(user.UserID)

                        PrjManager.Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, DestConnString, DestProviderType, DestProjectID, user.UserID)

                        PrjManager.CleanUpUserDataFromMemory(PrjManager.ActiveObjectives.HierarchyID, user.UserID, , PrjManager.User IsNot user)
                    Next
                    'C0808==

                    i = 1
                    For Each CG As clsCombinedGroup In PrjManager.CombinedGroups.GroupsList
                        'Debug.Print("Processing group data (" + i.ToString + "/" + count.ToString + ": " + CG.CombinedUserID.ToString)
                        i += 1

                        PrjManager.StorageManager.StorageType = SrcStorageType
                        PrjManager.StorageManager.ProviderType = SrcProject.ProviderType
                        PrjManager.StorageManager.ProjectLocation = SrcProject.ConnectionString
                        PrjManager.StorageManager.ModelID = SrcProject.ID
                        PrjManager.StorageManager.ReadDBVersion()

                        PrjManager.StorageManager.Reader.LoadGroupPermissions(CG)

                        PrjManager.StorageManager.StorageType = clsProject.StorageType
                        PrjManager.StorageManager.ProviderType = DestProviderType
                        PrjManager.StorageManager.ProjectLocation = DestConnString
                        PrjManager.StorageManager.CanvasDBVersion = WriteVersion    ' D2151
                        PrjManager.StorageManager.ModelID = DestProjectID
                        PrjManager.PipeParameters.ProjectVersion = PrjManager.StorageManager.CanvasDBVersion

                        PrjManager.StorageManager.Writer.SaveGroupPermissions(CG)
                    Next

                    'PrjManager.Attributes.ReadAttributeValues(AttributesStorageType.astStreamsDatabase, SrcProject.ConnectionString, SrcProject.ProviderType, SrcProject.ID, UNDEFINED_USER_ID)
                    'PrjManager.Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, DestConnString, DestProviderType, DestProjectID, UNDEFINED_USER_ID)
                End If

                ' D1621 ===
                ' Copy streams for Insight surveys
                Database.ExecuteSQL(String.Format("INSERT INTO ModelStructure (ProjectID, StructureType, StreamSize, Stream, ModifyDate) SELECT {0}, StructureType, StreamSize, Stream, ModifyDate FROM ModelStructure WHERE ProjectID={1} AND StructureType IN ({2},{3},{4},{5},{6})", DestProjectID, SrcProject.ID, CInt(StructureType.stSpyronModelVersion), CInt(StructureType.stSpyronStructureWelcome), CInt(StructureType.stSpyronStructureThankYou), CInt(StructureType.stSpyronStructureImpactWelcome), CInt(StructureType.stSpyronStructureImpactThankyou)))
                If Not fStreamsCopied AndAlso Not PerformUserEmailsCleanup Then
                    Database.ExecuteSQL(String.Format("INSERT INTO UserData (ProjectID, UserID, DataType, StreamSize, Stream, ModifyDate) SELECT {0}, UserID, DataType, StreamSize, Stream, ModifyDate FROM UserData WHERE ProjectID={1} AND DataType IN ({2},{3},{4},{5})", DestProjectID, SrcProject.ID, CInt(UserDataType.udtSpyronAnswersWelcome), CInt(UserDataType.udtSpyronAnswersThankYou), CInt(UserDataType.udtSpyronAnswersImpactWelcome), CInt(UserDataType.udtSpyronAnswersImpactThankyou)))
                End If
                ' D1621 ==

                PrjManager.StorageManager.StorageType = SrcStorageType
                PrjManager.StorageManager.ProviderType = SrcProject.ProviderType
                PrjManager.StorageManager.ProjectLocation = SrcProject.ConnectionString
                PrjManager.StorageManager.ModelID = SrcProject.ID
                PrjManager.StorageManager.ReadDBVersion()

                PrjManager.PipeParameters.ProjectVersion = PrjManager.StorageManager.CanvasDBVersion 'C0462
                'PrjManager.PipeParameters.Read(clsProject.PipeStorageType, SrcProject.ConnectionString, DestProviderType, DestProjectID)   'D0376 'C0790
                PrjManager.PipeParameters.Read(clsProject.PipeStorageType, SrcProject.ConnectionString, SrcProject.ProviderType, SrcProject.ID) 'C0790

                PrjManager.AntiguaDashboard.LoadPanel(PrjManager.StorageManager.StorageType, PrjManager.StorageManager.ProjectLocation, PrjManager.StorageManager.ProviderType, SrcProject.ID) 'C0783
                PrjManager.AntiguaRecycleBin.LoadPanel(PrjManager.StorageManager.StorageType, PrjManager.StorageManager.ProjectLocation, PrjManager.StorageManager.ProviderType, SrcProject.ID) 'C0783
                PrjManager.Regions.ReadRegions(PrjManager.StorageManager.StorageType, PrjManager.StorageManager.ProjectLocation, PrjManager.StorageManager.ProviderType, SrcProject.ID)
                PrjManager.AntiguaInfoDocs.LoadAntiguaInfoDocs()    ' D4197

                PrjManager.Parameters.Load()    ' D3834
                PrjManager.Comments.LoadComments()    ' D4265

                PrjManager.ResourceAligner.Load(PrjManager.StorageManager.StorageType, PrjManager.StorageManager.ProjectLocation, PrjManager.StorageManager.ProviderType, SrcProject.ID)

                'PrjManager.StorageManager.Reader.LoadDataMapping()

                'PrjManager.Attributes.ReadAttributes(AttributesStorageType.astStreamsDatabase, SrcProject.ConnectionString, SrcProject.ProviderType, SrcProject.ID)
                'PrjManager.Controls.ReadControls(ECModelStorageType.mstCanvasStreamDatabase, SrcProject.ConnectionString, SrcProject.ProviderType, SrcProject.ID)

                PrjManager.StorageManager.StorageType = clsProject.StorageType  ' D0369 + D0376 + D0479
                PrjManager.StorageManager.ProviderType = DestProviderType   ' D0329
                PrjManager.StorageManager.ProjectLocation = DestConnString
                PrjManager.StorageManager.CanvasDBVersion = WriteVersion    ' D2151
                PrjManager.StorageManager.ModelID = DestProjectID
                PrjManager.PipeParameters.ProjectVersion = PrjManager.StorageManager.CanvasDBVersion 'C0462

                'C0720===
                If sObjCleanUpName IsNot Nothing OrElse sAltCleanUpName IsNot Nothing OrElse PerformUserEmailsCleanup Then   ' D1182
                    DebugInfo("make project clean-up")    ' D0827
                    PrjManager.CleanUpModel(Not String.IsNullOrEmpty(sObjCleanUpName), Not String.IsNullOrEmpty(sAltCleanUpName), PerformUserEmailsCleanup, PerformUserNamesCleanup, sObjCleanUpName, sAltCleanUpName, sObjImpactCleanUpName)    ' D1182 + D1186 + D3432
                End If
                'C0720==

                DebugInfo("Start saving destination model")    ' D0827
                fResult = PrjManager.StorageManager.Writer.SaveProject(True) 'C0808

                If Not fResult Then DBSaveLog(dbActionType.actCreate, dbObjectType.einfProject, DestProjectID, String.Format("Unable to save project copy as ver {0}", SaveAsDBVersion), "") ' D3359

                DebugInfo("Save pipe parameters")    ' D0827

                'If fResult Then SrcProject.PipeParameters.Write(clsProject.PipeStorageType, DestConnString, DestProviderType, DestProjectID) 'C0278 + D0369 + D0376 + D0479 'C0783

                'C0783===
                If fResult Then

                    'C0808===
                    PrjManager.StorageManager.Writer.SaveInfoDocs()
                    PrjManager.StorageManager.Writer.SaveAHPExtraTables()
                    'PrjManager.StorageManager.Writer.SaveDataMapping() ' redundant due to saving on call SaveProject()
                    'C0808==

                    'PrjManager.PipeParameters.Write(clsProject.PipeStorageType, DestConnString, DestProviderType, DestProjectID) 'C0278 + D0369 + D0376 + D0479 'C0783 'C0791
                    PrjManager.PipeParameters.Write(clsProject.PipeStorageType, PrjManager.StorageManager.ProjectLocation, PrjManager.StorageManager.ProviderType, DestProjectID) 'C0278 + D0369 + D0376 + D0479 'C0783 'C0791
                    PrjManager.AntiguaDashboard.SavePanel(PrjManager.StorageManager.StorageType, PrjManager.StorageManager.ProjectLocation, PrjManager.StorageManager.ProviderType, DestProjectID) 'C0783
                    PrjManager.AntiguaRecycleBin.SavePanel(PrjManager.StorageManager.StorageType, PrjManager.StorageManager.ProjectLocation, PrjManager.StorageManager.ProviderType, DestProjectID) 'C0783
                    PrjManager.Regions.WriteRegions(PrjManager.StorageManager.StorageType, PrjManager.StorageManager.ProjectLocation, PrjManager.StorageManager.ProviderType, DestProjectID)
                    PrjManager.AntiguaInfoDocs.SaveAntiguaInfoDocs()    ' D4197

                    PrjManager.Attributes.WriteAttributes(AttributesStorageType.astStreamsDatabase, DestConnString, DestProviderType, DestProjectID)
                    PrjManager.Controls.WriteControls(ECModelStorageType.mstCanvasStreamDatabase, DestConnString, DestProviderType, DestProjectID)

                    PrjManager.ResourceAligner.Save(ECModelStorageType.mstCanvasStreamDatabase, DestConnString, DestProviderType, DestProjectID)

                    If SaveAsDBVersion >= 39 Then PrjManager.Parameters.Save() ' D3834
                    If SaveAsDBVersion >= 40 Then PrjManager.Comments.SaveComments() ' D4265
                End If

                ' -D1621
                ''SL ===
                'Dim DBConn As DbConnection
                'Dim oCommand As DbCommand = GetDBCommand(DestProviderType)
                'DBConn = GetDBConnection(DestProviderType, DestConnString) ' D0350
                'DBConn.Open()
                'oCommand.Connection = DBConn
                'oCommand.CommandText = String.Format("INSERT INTO ModelStructure (ProjectID, StructureType, StreamSize, Stream, ModifyDate) SELECT {0}, StructureType, StreamSize, Stream, ModifyDate FROM ModelStructure WHERE ProjectID = {1} AND (StructureType = 16 OR StructureType = 17 OR StructureType = 18)", DestProjectID, ProjectID)
                'oCommand.ExecuteNonQuery()
                'If Not PerformUserEmailsCleanup Then
                '    oCommand.CommandText = String.Format("INSERT INTO UserData (ProjectID, UserID, DataType, StreamSize, Stream, ModifyDate) SELECT {0}, UserID, DataType, StreamSize, Stream, ModifyDate FROM UserData WHERE (ProjectID = {1} AND (DataType = 6 OR DataType = 7)", DestProjectID, ProjectID)
                '    oCommand.ExecuteNonQuery()
                'End If
                'DBConn.Close()
                ''SL ==

                'C0783==
                ' D0183 ==
                DebugInfo("Project saved")    ' D0827
            Else
                DBSaveLog(dbActionType.actOpen, dbObjectType.einfProject, SrcProject.ID, "Load project for copy", "Unable to read source project properly") ' D3359
                fResult = False ' D0394
            End If
            If fNeedUnLock AndAlso Not ActiveUser Is Nothing Then DBProjectLockInfoWrite(ECLockStatus.lsUnLocked, SrcProject.LockInfo, ActiveUser, Now) ' D0483 + D0513 + D0589
            Return fResult
        End Function
        ' D0479 ==
#End Region


#Region "Service Functions"

        ' D0496 ===
        Public Sub DBSaveLog(ByVal tActionType As dbActionType, ByVal tObjectType As dbObjectType, ByVal tObjectID As Integer, ByVal sComment As String, ByVal sResult As String, Optional ByVal UserID As Integer = -1, Optional ByVal WorkgroupID As Integer = -1)
            If Not dbExists() Then Return
            If UserID = -1 AndAlso Not _CurrentUser Is Nothing Then UserID = _CurrentUser.UserID ' D2289
            If WorkgroupID = -1 AndAlso Not _CurrentWorkgroup Is Nothing Then WorkgroupID = _CurrentWorkgroup.ID ' D2289
            If isServiceRun Then sComment = ("[CWSw] " + sComment).Trim ' D2236
            ' D7363 ===
            Dim sOrigSession = ""
            If Not String.IsNullOrEmpty(OriginalSessionUser) AndAlso (_CurrentUser Is Nothing OrElse OriginalSessionUser.ToLower <> _CurrentUser.UserEmail.ToLower) Then sOrigSession = "@" + OriginalSessionUser
            If _CurrentUser IsNot Nothing AndAlso isAlexaUser Then sOrigSession += "@Alexa" ' D7584
            If Options.SessionID <> "" Then sResult = String.Format("[{0}{1}] {2}", Options.SessionID, sOrigSession, sResult).TrimEnd
            ' D7363 ==
            Dim tParams As New List(Of Object)
            tParams.Add(Now)
            tParams.Add(tObjectType)
            tParams.Add(tActionType)
            tParams.Add(UserID)
            tParams.Add(WorkgroupID)
            tParams.Add(tObjectID)
            tParams.Add(ShortString(sComment.Trim, 999, True))   ' D0537
            tParams.Add(ShortString(sResult.Trim, 999, True))    ' D0537
            ' -D4538 ===
            'If WebConfigOption("NewRelic.agentEnabled", "false").ToLower = "true" Then
            '    If HasActiveProject() AndAlso ActiveProject IsNot Nothing Then
            '        NewRelic.Api.Agent.NewRelic.AddCustomParameter("ActiveProjectName", ActiveProject.ProjectName)
            '    End If
            '    If ActiveUser IsNot Nothing Then
            '        NewRelic.Api.Agent.NewRelic.AddCustomParameter("ActiveUserEMail", ActiveUser.UserEmail)
            '        NewRelic.Api.Agent.NewRelic.AddCustomParameter("ActiveUserName", ActiveUser.UserName)
            '    End If
            '    If ActiveWorkgroup IsNot Nothing Then
            '        NewRelic.Api.Agent.NewRelic.AddCustomParameter("ActiveWorkgroupName", ActiveWorkgroup.Name)
            '    End If
            '    'RecordCustomEvent(tActionType, tObjectType, tObjectID, sComment, sResult, UserID, WorkgroupID)
            'End If
            ' -D4538 ==
            Try
                Database.ExecuteSQL("INSERT INTO Logs (DT, TypeID, ActionID, UserID, WorkgroupID, ObjectID, Comment, Result) VALUES (?,?,?,?,?,?,?,?)", tParams)
            Catch ex As Exception
                DebugInfo("Error for save log! " + ex.Message, _TRACE_WARNING)
            End Try
        End Sub
        ' D0496 ==

        ' D0487 ===
        Public Function DBUpdateDateTime(ByVal sTable As String, ByVal sProperty As String, ByVal tObjectID As Integer, Optional ByVal sObjectIDField As String = "ID") As Nullable(Of DateTime) ' D4535
            If Not dbExists() Then Return Nothing 'A1446
            Dim tRes As New Nullable(Of DateTime)   ' D4535
            If Database.Connect Then
                Dim tParams As New List(Of Object)

                ' D3562 ===
                Dim tNow As Date = Now

                tParams.Add(tNow)
                tParams.Add(tObjectID)

                Dim tData As New KeyValuePair(Of Integer, Date)(tObjectID, tNow.AddYears(-10))
                If LastDates.ContainsKey(sProperty) Then
                    tData = LastDates(sProperty)
                    LastDates.Remove(sProperty)
                End If

                If tData.Value <> tNow Then
                    Try ' D0505
                        ' D3554 ===
                        Dim sSQL As String
                        If sTable.ToLower = clsComparionCore._TABLE_PROJECTS.ToLower AndAlso sProperty.ToLower = clsComparionCore._FLD_PROJECTS_LASTMODIFY.ToLower Then
                            sSQL = String.Format("UPDATE {0} SET {1}=?, {2}=? WHERE {3}=?", sTable, sProperty, clsComparionCore._FLD_PROJECTS_LASTVISITED, sObjectIDField)
                            tParams.Insert(1, tNow)
                        Else
                            ' D6507 ===
                            If sTable.ToLower = clsComparionCore._TABLE_USERS.ToLower AndAlso sProperty.ToLower = clsComparionCore._FLD_USERS_LASTVISITED.ToLower AndAlso ActiveWorkgroup IsNot Nothing AndAlso isAuthorized Then
                                sSQL = String.Format("UPDATE {0} SET {1}=?, LastWorkgroupID=?, DefaultWGID=?, isOnline=1 WHERE {2}=?", sTable, sProperty, sObjectIDField)    ' D7464
                                tParams.Insert(1, ActiveWorkgroup.ID)
                                tParams.Insert(1, ActiveWorkgroup.ID)   ' D7464
                            Else
                                ' D6507 ==
                                sSQL = String.Format("UPDATE {0} SET {1}=? WHERE {2}=?", sTable, sProperty, sObjectIDField)
                            End If
                        End If
                        Database.ExecuteSQL(sSQL, tParams)  ' D0491
                        ' D3554 ==
                    Catch ex As Exception
                    End Try
                End If
                LastDates.Add(sProperty, New KeyValuePair(Of Integer, Date)(tObjectID, tNow))
                ' D3562 ==
                tRes = tNow ' D4535
            End If
            Return tRes ' D4535
        End Function
        ' D0487 ==

        ' D0501 ===
        Public Function DBOnlineSessions() As List(Of clsOnlineUserSession)
            'Debug.WriteLine("DB online start at " + Now.ToLongTimeString)
            If Not dbExists() Then Return Nothing
            Dim Lst As New List(Of clsOnlineUserSession)
            Dim fNeedLoad As Boolean = True
            If Not _OnlineSessionsCache Is Nothing AndAlso _OnlineSessionsLoaded.HasValue Then fNeedLoad = _OnlineSessionsLoaded.Value < Now.AddSeconds(-_OnlineSessionsTimeout)
            If fNeedLoad Then
                If Database.Connect Then
                    Dim tParams As New List(Of Object)
                    tParams.Add(Now.AddSeconds(-_DEF_SESS_TIMEOUT))
                    If isAuthorized Then tParams.Add(ActiveUser.UserID) ' D6507
                    Dim Users As List(Of Dictionary(Of String, Object)) = Database.SelectBySQL("SELECT U.*, U.ID as UserID, UW.LastProjectID, UW.RoleGroupID FROM Users U LEFT JOIN UserWorkgroups UW ON U.ID=UW.UserID AND UW.WorkgroupID=U.LastWorkgroupID WHERE (U.isOnline>0 AND U.LastVisited>=?)" + If(isAuthorized, " OR U.ID=?", ""), tParams)  ' D6507
                    For Each tRow As Dictionary(Of String, Object) In Users
                        Dim tSess As clsOnlineUserSession = DBParse_Session(tRow)
                        Lst.Add(tSess)
                    Next
                    _OnlineSessionsCache = Lst
                    _OnlineSessionsLoaded = Now
                End If
            Else
                Lst = _OnlineSessionsCache
                'Debug.WriteLine("Return from cache")
            End If
            'Debug.WriteLine("DB online and at " + Now.ToLongTimeString)
            Return Lst
        End Function
        ' D0501 ==

        ' D0557 ===
        Public Sub DBSaveLogonEvent(ByVal sUserEmail As String, ByVal tLogonResult As ecAuthenticateError, Optional ByVal tAuthWay As ecAuthenticateWay = ecAuthenticateWay.awRegular, Optional ByVal tRequest As HttpRequest = Nothing, Optional sErrorMsg As String = "") ' D6068
            If Not dbExists() Then Return
            Dim UID As Integer = -1
            If Not ActiveUser Is Nothing Then UID = ActiveUser.UserID
            Dim sExtra As String = If(String.IsNullOrEmpty(sErrorMsg), "", sErrorMsg)   ' D6068
            ' D6062 ===
            If Not String.IsNullOrEmpty(sUserEmail) AndAlso UID <= 0 Then
                Dim tmpUser As clsApplicationUser = DBUserByEmail(sUserEmail)
                If tmpUser IsNot Nothing Then
                    UID = tmpUser.UserID
                    ' D6068 ===
                    If tLogonResult = ecAuthenticateError.aeUserLockedByWrongPsw OrElse tLogonResult = ecAuthenticateError.aeWrongPassword AndAlso tmpUser.PasswordStatus >= 0 Then
                        sExtra += String.Format("; Attempt {0}/{1} [LP:{2};LT:{3};AU:{4}]", tmpUser.PasswordStatus, _DEF_PASSWORD_ATTEMPTS, _DEF_PASSWORD_ATTEMPTS_PERIOD, _DEF_PASSWORD_ATTEMPTS_LOCK_TIMEOUT, If(_DEF_PASSWORD_ATTEMPTS_AUTOUNLOCK, "Y", "N"))
                    End If
                    ' D6068 ==
                    If SSO_User Then sExtra += "; [SSO]"  ' D6552
                End If
            End If
            ' D6062 ==
            Dim sHost As String = "unknown host"
            Dim sUserAgent As String = "unknown browser"
            Dim sURL As String = "" ' D2289
            If tRequest IsNot Nothing Then
                sHost = GetClientIP(tRequest)   ' D5084
                sUserAgent = tRequest.UserAgent
                sURL = tRequest.Url.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped)   ' D2289
            End If

            ' D0558 ==
            Dim tLogonAction As dbActionType = dbActionType.actLogon
            Select Case tAuthWay
                Case ecAuthenticateWay.awStartMeeting
                    tLogonAction = dbActionType.actStartMeeting
                Case ecAuthenticateWay.awJoinMeeting
                    tLogonAction = dbActionType.actJoinMeeting
                Case ecAuthenticateWay.awTokenizedURL
                    tLogonAction = dbActionType.actTokenizedURLLogon
                Case ecAuthenticateWay.awCredentials
                    tLogonAction = dbActionType.actCredentialsLogon
                Case ecAuthenticateWay.awSSO    ' D6532
                    tLogonAction = dbActionType.actSSOLogin ' D6532
            End Select

            DBSaveLog(tLogonAction, dbObjectType.einfUser, UID, String.Format("{0} ({1}, [{2}])" + vbCrLf + "Application: {3} {4}/{5}", sUserEmail, sHost, sUserAgent, sURL, GetVersion(GetCoreVersion), WebConfigOption("ChangeSet", "IDE", True)).Trim, (tLogonResult.ToString + If(sExtra = "", "", " (" + sExtra.Trim + ")")) + ", SessID: " + Options.SessionID) ' D2289 + D6068 + D7443
            'RecordCustomEvent(tLogonAction, dbObjectType.einfUser, UID, "", "") ' D3186 -D4538
            ' D0558 ==
        End Sub
        ' D0557 ==

#End Region


#Region "TinyURL service"

        ' D0899 ===
        Public Function DBTinyURLDelete(Optional ByVal ID As Integer = -1, Optional ByVal ProjectID As Integer = -1, Optional ByVal UserID As Integer = -1) As Boolean
            If Not dbExists() Then Return Nothing
            If CanvasMasterDBVersion >= "0.991" Then
                Dim tParams As New List(Of Object)
                Dim sSQL As String = ""
                If ProjectID >= 0 Then
                    sSQL += " ProjectID=?"
                    tParams.Add(ProjectID)
                End If
                If UserID >= 0 Then
                    sSQL += CStr(IIf(sSQL = "", "", " AND")) + " UserID=?"
                    tParams.Add(UserID)
                End If
                If ID >= 0 Or sSQL = "" Then
                    sSQL += CStr(IIf(sSQL = "", "", " AND")) + " ID=?"
                    tParams.Add(ID)
                End If
                sSQL = "DELETE FROM PrivateURLs WHERE" + sSQL
                DebugInfo("Delete TinyURL information...")
                If Database.ExecuteSQL(sSQL, tParams) > 0 Then
                    HashCodesReset()        ' D4936 + D7114
                    Return True             ' D4936
                End If
            End If
            Return False
        End Function
        ' D0899 ==

#End Region


#Region "Snapshots"

        ' D3509 ===
        Public Function isSnapshotsAvailable() As Boolean
            Return _OPT_SNAPSHOTS_ENABLE AndAlso CanvasMasterDBVersion >= "0.99991" ' D3829
        End Function

        Private Function DBSnapshotsFieldsList(fParseStream As Boolean) As String
            Dim sFld As String = ""
            If CanvasMasterDBVersion >= "0.99992" Then sFld = ",RestoredFrom,SnapshotIdx,Details" ' D3731
            Return CStr(IIf(fParseStream, "*", String.Format("{0},{1},{2},{3},{4},{5},{6}{7}", _FLD_SNAPSHOTS_ID, _FLD_SNAPSHOTS_DT, _FLD_SNAPSHOTS_PRJ_ID, _FLD_SNAPSHOTS_TYPE, "StreamMD5", "WorkspaceMD5", _FLD_SNAPSHOTS_COMMENT, sFld))) + String.Format(", DATALENGTH({0}) as StreamBytes, DATALENGTH({1}) as WksBytes", _FLD_SNAPSHOTS_STREAM, _FLD_SNAPSHOTS_WORKSPACE) ' D3731 + D3775
        End Function

        ' D3592 ===
        Private Function DBSnapshotsCount(tProjectID As Integer) As Integer
            If Not dbExists() Then Return Nothing
            Dim tCnt As Integer = 0
            If isSnapshotsAvailable() Then
                Dim tRes As Object = Database.ExecuteScalarSQL(String.Format("SELECT Count(id) as cnt FROM {0} WHERE {1}={2}", _TABLE_SNAPSHOTS, _FLD_SNAPSHOTS_PRJ_ID, tProjectID))
                If tRes IsNot Nothing AndAlso Not IsDBNull(tRes) Then tCnt = CInt(tRes)
            End If
            Return tCnt
        End Function

        Private Function DBSnapshotsTrim(tProjectID As Integer) As Integer
            If Not dbExists() Then Return Nothing
            Dim tDeleted As Integer = 0
            If _OPT_SNAPSHOTS_MAX_COUNT > 2 Then
                ' D3898 ===
                Dim tList As List(Of clsSnapshot) = DBSnapshotsReadAll(tProjectID, False)
                Dim fContinue As Boolean = True
                While fContinue AndAlso tList.Count >= _OPT_SNAPSHOTS_MAX_COUNT
                    'Dim CntAuto As Integer = 0
                    Dim CntManual As Integer = 0
                    Dim LastManual As clsSnapshot = Nothing
                    Dim LastAuto As clsSnapshot = Nothing
                    'Dim LastRP As clsSnapshot = Nothing
                    Dim Mindate As DateTime = Now.AddMinutes(_OPT_SNAPSHOTS_KEEP_PERIOD_MINS)
                    For Each tSnap As clsSnapshot In tList
                        If tSnap.Type = ecSnapShotType.Manual Then
                            CntManual += 1
                            LastManual = tSnap
                        Else
                            'CntAuto += 1
                            'If tSnap.Type = ecSnapShotType.RestorePoint Then LastRP = tSnap Else LastAuto = tSnap
                            LastAuto = tSnap
                        End If
                    Next
                    Dim DelSnap As clsSnapshot = Nothing
                    If CntManual > _OPT_SNAPSHOTS_MANUAL_MAX_COUNT AndAlso LastManual IsNot Nothing AndAlso LastManual.DateTime < Mindate Then
                        DelSnap = LastManual
                    Else
                        If LastAuto IsNot Nothing AndAlso LastAuto.DateTime < Mindate Then
                            DelSnap = LastAuto
                            'Else
                            '    If LastRP IsNot Nothing AndAlso LastRP.DateTime < Mindate Then DelSnap = LastRP
                        End If
                    End If
                    If DelSnap IsNot Nothing Then
                        fContinue = DBSnapshotDelete(DelSnap.ID)
                        If fContinue Then
                            fContinue = tList.Remove(DelSnap)
                            tDeleted += 1
                        End If
                    Else
                        fContinue = False
                    End If
                End While
                'tDeleted = Database.ExecuteSQL(String.Format("WITH tab AS (SELECT *,ROW_NUMBER() OVER(PARTITION BY StreamMD5, WorkspaceMD5 ORDER BY SnapshotType DESC, DT DESC) AS rn FROM {0})" + vbCrLf + "DELETE FROM tab WHERE 1 < rn AND ProjectID={1} AND DT<DATEADD(""hour"", -2, GETDATE())", _TABLE_SNAPSHOTS, tProjectID))
                'If tCnt - tDeleted > _OPT_SNAPSHOTS_MAX_COUNT Then
                '    tDeleted += Database.ExecuteSQL(String.Format("DELETE FROM {0} WHERE {1}={2} AND DT<DATEADD(""minute"", -10, GETDATE()) AND ID NOT IN (SELECT TOP {3} ID FROM {0} WHERE {1}={2} ORDER BY ABS(1-{4}) ASC, {4} DESC, DT DESC)", _TABLE_SNAPSHOTS, _FLD_SNAPSHOTS_PRJ_ID, tProjectID, _OPT_SNAPSHOTS_MAX_COUNT - 1, _FLD_SNAPSHOTS_TYPE)) ' D3595
                'End If
                ' D3898 ==
            End If
            Return tDeleted
        End Function
        ' D3592 ==

        ' D3882 ===
        Private Function DBSnapshotAdd(ByRef tSnapshot As clsSnapshot, fAddLogEvent As Boolean) As Boolean  ' D3898
            If Not dbExists() Then Return Nothing
            Dim fResult As Boolean = False
            If tSnapshot IsNot Nothing AndAlso isSnapshotsAvailable() AndAlso Database.Connect AndAlso tSnapshot.ProjectID > 0 Then ' D3882
                DebugInfo("Save snapshot to DB")

                Dim sFields As String = ""
                Dim sParams As String = ""
                If CanvasMasterDBVersion >= "0.99992" Then
                    sFields = ", SnapshotIdx, RestoredFrom, Details"    ' D3731
                    sParams = ", ?, ?, ?"
                End If

                Dim SQL As String = String.Format("INSERT INTO {0} ({1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}{9}) VALUES (?, ?, ?, ?, ?, ?, ?, ?{10})", _TABLE_SNAPSHOTS,
                                                  _FLD_SNAPSHOTS_DT, _FLD_SNAPSHOTS_PRJ_ID, _FLD_SNAPSHOTS_TYPE, _FLD_SNAPSHOTS_COMMENT, _FLD_SNAPSHOTS_STREAM, "StreamMD5", "Workspace", "WorkspaceMD5", sFields, sParams)
                ' D3729 ==
                Dim tParams As New List(Of Object)
                tParams.Add(tSnapshot.DateTime)
                tParams.Add(tSnapshot.ProjectID)
                tParams.Add(tSnapshot.Type)
                tParams.Add(ShortString(tSnapshot.Comment, 250, True))  ' D3653
                If tSnapshot.ProjectStream Is Nothing Then
                    tParams.Add(DBNull.Value)
                Else
                    Dim tStreamContent() As Byte = {}
                    Dim StreamLength As Integer = CInt(tSnapshot.ProjectStream.Length)
                    Array.Resize(tStreamContent, StreamLength)
                    tSnapshot.ProjectStream.Seek(0, SeekOrigin.Begin)
                    tSnapshot.ProjectStream.Read(tStreamContent, 0, StreamLength)
                    tParams.Add(tStreamContent)
                End If
                tParams.Add(tSnapshot.ProjectStreamMD5)
                tParams.Add(tSnapshot.ProjectWorkspace)
                tParams.Add(tSnapshot.ProjectWorkspaceMD5)
                ' D3729 ===
                If CanvasMasterDBVersion >= "0.99992" Then
                    tParams.Add(tSnapshot.Idx)
                    tParams.Add(tSnapshot.RestoredFrom)
                    tParams.Add(ShortString(tSnapshot.Details, 250, True))
                End If
                ' D3729 ==
                DebugInfo(String.Format("Save snapshot for project {0}", tSnapshot.ProjectID))
                fResult = Database.ExecuteSQL(SQL, tParams) > 0
                If fResult Then
                    tSnapshot.ID = clsDatabaseAdvanced.GetLastIdentity(Database)
                    If fAddLogEvent Then DBSaveLog(dbActionType.actCreate, dbObjectType.einfSnapshot, tSnapshot.ProjectID, String.Format("Create snapshot '{0}'", tSnapshot.Comment), tSnapshot.Details) ' D3584 + D3754 + D3898
                End If
                DebugInfo("Snapshot saved")
            End If
            Return fResult
        End Function

        ' D3882 ==

        Public Function DBSnapshotWrite(ByRef tSnapshot As clsSnapshot, Optional fIgnoreLastSnapshot As Boolean = False) As Boolean ' D3756
            If Not dbExists() Then Return Nothing
            Dim fResult As Boolean = False
            If tSnapshot IsNot Nothing AndAlso isSnapshotsAvailable() AndAlso Database.Connect AndAlso tSnapshot.ProjectID > 0 Then ' D3882
                DebugInfo("Save snapshot...")

                ' Read last snapshot for this project and compare MD5s for avoid save duplicate
                'D3688 ===
                'Dim tLastSnapshot As clsSnapshot = LastSnapshot
                'If (tLastSnapshot Is Nothing AndAlso ProjectID <> tSnapshot.ProjectID) OrElse (tLastSnapshot IsNot Nothing AndAlso tLastSnapshot.ProjectID <> tSnapshot.ProjectID) Then tLastSnapshot = DBSnapshotReadLatest(tSnapshot.ProjectID, True) ' D3511 + D3688
                Dim tLastSnapshot As clsSnapshot = Nothing
                If Not fIgnoreLastSnapshot AndAlso LastSnapshot IsNot Nothing AndAlso tSnapshot.ProjectID = LastSnapshot.ProjectID Then tLastSnapshot = LastSnapshot Else tLastSnapshot = DBSnapshotReadLatest(tSnapshot.ProjectID, True) ' D3511 + D3756
                ' D3688 ==

                If tLastSnapshot Is Nothing OrElse tSnapshot.Type = ecSnapShotType.Manual OrElse
                   ((tLastSnapshot.ProjectStreamMD5 <> tSnapshot.ProjectStreamMD5 OrElse tLastSnapshot.ProjectWorkspaceMD5 <> tSnapshot.ProjectWorkspaceMD5 OrElse (tSnapshot.Comment.Contains("RTE") AndAlso (tLastSnapshot Is Nothing OrElse tLastSnapshot.Comment <> tSnapshot.Comment))) AndAlso
                   (Not _OPT_SNAPSHOTS_REPLACE_SIMILAR OrElse tLastSnapshot.Comment <> tSnapshot.Comment)) OrElse
                   (Not _OPT_SNAPSHOTS_CHECK_ONLY_MD5 AndAlso (tLastSnapshot.Comment <> tSnapshot.Comment OrElse tLastSnapshot.Type <> tSnapshot.Type)) OrElse
                   (tLastSnapshot IsNot Nothing AndAlso tLastSnapshot.DateTime.AddMinutes(_OPT_SNAPSHOTS_SIMILAR_PERIOD_MINS) < Now AndAlso tLastSnapshot.GetHashCode() <> tSnapshot.GetHashCode()) Then  ' D3595 + D3688 + D4175 + D7485

                    DBSnapshotsTrim(tSnapshot.ProjectID)  ' D3592

                    ' D3729 ===
                    Dim idx As Integer = 1
                    If tLastSnapshot IsNot Nothing Then
                        If tLastSnapshot.Idx > 0 Then
                            idx = tLastSnapshot.Idx + 1
                        Else
                            idx = SnapshotsCheckMissingIdx(DBSnapshotsReadAll(tSnapshot.ProjectID, False)) + 1  ' D3731
                        End If
                    End If
                    tSnapshot.Idx = idx

                    DBSnapshotAdd(tSnapshot, True)    ' D3882 + D3898

                Else

                    ' D3754 ==
                    If CanvasMasterDBVersion >= "0.99992" AndAlso tSnapshot IsNot Nothing AndAlso tLastSnapshot IsNot Nothing AndAlso _OPT_SNAPSHOTS_REPLACE_SIMILAR AndAlso tSnapshot.Comment = tLastSnapshot.Comment AndAlso
                       (tLastSnapshot.ProjectStreamMD5 <> tSnapshot.ProjectStreamMD5 OrElse tLastSnapshot.ProjectWorkspaceMD5 <> tSnapshot.ProjectWorkspaceMD5) Then

                        tLastSnapshot = DBSnapshotReadLatest(tSnapshot.ProjectID, False)    ' D3757

                        Dim sOrigDetails As String = tSnapshot.Details
                        If LastSnapshot IsNot Nothing AndAlso tSnapshot.Details <> "" AndAlso Not (_SNAPSHOT_DETAILS_DELIM + tLastSnapshot.Details).EndsWith(_SNAPSHOT_DETAILS_DELIM + tSnapshot.Details) Then
                            If tSnapshot.Details.Length + tLastSnapshot.Details.Length < 246 Then
                                tSnapshot.Details = String.Format("{0}{1}{2}", tLastSnapshot.Details, IIf(tLastSnapshot.Details = "", "", _SNAPSHOT_DETAILS_DELIM), tSnapshot.Details)
                            Else
                                tSnapshot.Details = tLastSnapshot.Details
                                If Not tSnapshot.Details.EndsWith("...") Then tSnapshot.Details += _SNAPSHOT_DETAILS_DELIM + "..."
                            End If
                        End If

                        Dim SQL As String = String.Format("UPDATE {0} SET {1}=?, {2}=?, {3}=?, {4}=?, {5}=?, {6}=?, {7}=?, {8}=? WHERE {9}=?", _TABLE_SNAPSHOTS, _
                                                          _FLD_SNAPSHOTS_DT, _FLD_SNAPSHOTS_COMMENT, _FLD_SNAPSHOTS_STREAM, "StreamMD5", "Workspace", "WorkspaceMD5", "Details", "RestoredFrom", _FLD_SNAPSHOTS_ID) ' D4553
                        Dim tParams As New List(Of Object)
                        tParams.Add(tSnapshot.DateTime)
                        tParams.Add(ShortString(tSnapshot.Comment, 250, True))
                        If tSnapshot.ProjectStream Is Nothing Then
                            tParams.Add(DBNull.Value)
                        Else
                            Dim tStreamContent() As Byte = {}
                            Dim StreamLength As Integer = CInt(tSnapshot.ProjectStream.Length)
                            Array.Resize(tStreamContent, StreamLength)
                            tSnapshot.ProjectStream.Seek(0, SeekOrigin.Begin)
                            tSnapshot.ProjectStream.Read(tStreamContent, 0, StreamLength)
                            tParams.Add(tStreamContent)
                        End If
                        tParams.Add(tSnapshot.ProjectStreamMD5)
                        tParams.Add(tSnapshot.ProjectWorkspace)
                        tParams.Add(tSnapshot.ProjectWorkspaceMD5)
                        tParams.Add(ShortString(tSnapshot.Details, 252, True))
                        tParams.Add(tSnapshot.RestoredFrom) ' D4553
                        tParams.Add(tLastSnapshot.ID)
                        DebugInfo(String.Format("Update snapshot for project {0}", tSnapshot.ProjectID))
                        fResult = Database.ExecuteSQL(SQL, tParams) > 0
                        If fResult Then
                            DBSaveLog(dbActionType.actModify, dbObjectType.einfSnapshot, tSnapshot.ProjectID, String.Format("Update snapshot '{0}'", tSnapshot.Comment), sOrigDetails)
                            ' D3756 ===
                        Else
                            _LastSnapshot = Nothing
                            tSnapshot.Details = sOrigDetails
                            Return DBSnapshotWrite(tSnapshot, True)
                            ' D3756 ==
                        End If

                        tSnapshot.ID = tLastSnapshot.ID
                        tSnapshot.Idx = tLastSnapshot.Idx
                        tSnapshot.RestoredFrom = tLastSnapshot.RestoredFrom

                        DebugInfo("Snapshot updated")

                    Else
                        DebugInfo("Skip saving snapshot due to same data")
                    End If
                    ' D3754 ===
                End If

            End If
            Return fResult
        End Function

        Public Function DBSnapshotInfoUpdate(ByRef tSnapshot As clsSnapshot) As Boolean
            If Not dbExists() Then Return Nothing
            Dim fResult As Boolean = False
            If tSnapshot IsNot Nothing AndAlso isSnapshotsAvailable() AndAlso Database.Connect Then
                DebugInfo("Update snapshot info...")
                Dim SQL As String = String.Format("UPDATE {0} SET {1}=?{3} WHERE {2}=?", _TABLE_SNAPSHOTS, _FLD_SNAPSHOTS_COMMENT, _FLD_SNAPSHOTS_ID, IIf(CanvasMasterDBVersion >= "0.99992", ", Details=?, SnapshotIdx=?, RestoredFrom=?", ""))  ' D3729 + D3731 + D4553
                Dim tParams As New List(Of Object)
                tParams.Add(ShortString(tSnapshot.Comment, 250, True))
                If (CanvasMasterDBVersion >= "0.99992") Then
                    tParams.Add(ShortString(tSnapshot.Details, 250, True)) ' D3729
                    tParams.Add(tSnapshot.Idx) ' D3731
                    tParams.Add(tSnapshot.RestoredFrom) ' D4553
                End If
                tParams.Add(tSnapshot.ID)
                fResult = Database.ExecuteSQL(SQL, tParams) > 0
            End If
            Return fResult
        End Function

        ' D3774 ===
        Public Function DBSnapshotsCopy(tProjectSrcID As Integer, tProjectDestID As Integer, Optional tSnapshotID As Integer = -1) As Boolean
            If Not dbExists() Then Return Nothing
            Dim fResult As Boolean = False
            If isSnapshotsAvailable() AndAlso Database.Connect AndAlso CanvasMasterDBVersion >= "0.99992" Then
                DebugInfo("Copy snapshots...")
                Dim sFields As String = "DT, SnapshotType, Stream, StreamMD5, Workspace, WorkspaceMD5, Comment, RestoredFrom, SnapshotIdx, Details"
                Dim SQL As String = String.Format("INSERT INTO {0} (ProjectID, {1}) SELECT {2}, {1} FROM {0} WHERE {3} = {4}{5}", _TABLE_SNAPSHOTS, sFields, tProjectDestID, _FLD_SNAPSHOTS_PRJ_ID, tProjectSrcID, IIf(tSnapshotID > 0, String.Format(" AND {0}={1}", _FLD_SNAPSHOTS_ID, tSnapshotID), ""))
                fResult = Database.ExecuteSQL(SQL) > 0
            End If
            Return fResult
        End Function
        ' D3774 ==

        Public Function DBSnapshotRead(tSnapShotID As Integer, fReadStream As Boolean) As clsSnapshot
            If Not dbExists() Then Return Nothing
            Dim tSS As clsSnapshot = Nothing
            If isSnapshotsAvailable() Then
                Dim tList As List(Of Dictionary(Of String, Object)) = Database.SelectFromTable(_TABLE_SNAPSHOTS, _FLD_SNAPSHOTS_ID, tSnapShotID, DBSnapshotsFieldsList(fReadStream), _FLD_SNAPSHOTS_DT + " DESC")
                DebugInfo(String.Format("Loaded Snapshot with ID #{0}", tSnapShotID))
                If tList.Count > 0 Then Return DBParse_Snapshot(tList(0), fReadStream) Else Return Nothing
            End If
            Return tSS
        End Function

        Public Function DBSnapshotReadLatest(tProjectID As Integer, fReadStream As Boolean) As clsSnapshot
            If Not dbExists() Then Return Nothing
            Dim tSS As clsSnapshot = Nothing
            If isSnapshotsAvailable() Then
                Dim tList As List(Of Dictionary(Of String, Object)) = Database.SelectFromTable(_TABLE_SNAPSHOTS, _FLD_SNAPSHOTS_PRJ_ID, tProjectID, "TOP 1 " + DBSnapshotsFieldsList(fReadStream), _FLD_SNAPSHOTS_DT + " DESC")
                DebugInfo(String.Format("Loaded latest snapshot for ProjectID #{0}", tProjectID))
                If tList.Count > 0 Then Return DBParse_Snapshot(tList(0), fReadStream) Else Return Nothing
            End If
            Return tSS
        End Function

        Public Function DBSnapshotsReadAll(tProjectID As Integer, fReadStream As Boolean) As List(Of clsSnapshot)
            If Not dbExists() Then Return Nothing
            Dim SS_List As New List(Of clsSnapshot)
            If isSnapshotsAvailable() Then
                Dim tList As List(Of Dictionary(Of String, Object)) = Database.SelectFromTable(_TABLE_SNAPSHOTS, _FLD_SNAPSHOTS_PRJ_ID, tProjectID, DBSnapshotsFieldsList(fReadStream), _FLD_SNAPSHOTS_DT + " DESC")   ' D3558
                For Each tRow As Dictionary(Of String, Object) In tList
                    SS_List.Add(DBParse_Snapshot(tRow, fReadStream))
                Next
            End If
            Return SS_List
        End Function

        Public Function DBSnapshotDelete(tSnapShotID As Integer) As Boolean
            If Not dbExists() Then Return Nothing
            Dim fResult As Boolean = False
            If isSnapshotsAvailable() Then
                Dim sSQL As String = String.Format("DELETE FROM {0} WHERE {1}=?", _TABLE_SNAPSHOTS, _FLD_SNAPSHOTS_ID)
                Dim tParams As New List(Of Object)
                tParams.Add(tSnapShotID)
                DebugInfo("Delete snapshot...")
                fResult = Database.ExecuteSQL(sSQL, tParams) > 0
                If fResult Then _LastSnapshot = Nothing ' D3756
            End If
            Return fResult
        End Function

        Public Function DBSnapshotsDeleteAll(tProjectID As Integer) As Integer
            If Not dbExists() Then Return Nothing
            Dim tDeletedCount As Integer = 0
            If isSnapshotsAvailable() Then
                Dim sSQL As String = String.Format("DELETE FROM {0} WHERE {1}=?", _TABLE_SNAPSHOTS, _FLD_SNAPSHOTS_PRJ_ID)
                Dim tParams As New List(Of Object)
                tParams.Add(tProjectID)
                DebugInfo("Delete snapshots for project...")
                tDeletedCount = Database.ExecuteSQL(sSQL, tParams)
                _LastSnapshot = Nothing ' D3756
            End If
            Return tDeletedCount
        End Function
        ' D3509 ==

#End Region

    End Class

End Namespace
