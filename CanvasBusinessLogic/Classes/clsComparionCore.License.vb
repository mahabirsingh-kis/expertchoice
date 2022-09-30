Imports ECSecurity.ECSecurity

Namespace ExpertChoice.Data

    Partial Public Class clsComparionCore

        ' D3966 ===
        Private _LastDT As Nullable(Of DateTime) = Nothing
        Private _LastCheckDT As Nullable(Of DateTime) = Nothing
        Private Const _CHECK_DT_PERIOD As Integer = 10  ' in minutes

        Public LicenseOption_ShowDraft As Boolean = False   ' D7603

        Private _IsRisk As Boolean? = Nothing

        Private Sub compareDate(DT As DateTime)
            If DT > _LastDT Then _LastDT = DT
        End Sub

        Private Sub compareDate(DT As Nullable(Of DateTime))
            If DT.HasValue Then compareDate(DT.Value)
        End Sub
        ' D3966 ==

        ' D0257 ===
        Private Function _licExpirationDate(ByVal tWorkgroup As clsWorkgroup, ByVal Parameter As Object) As Long    ' D0261
            ' D3966 ===
            If _LastCheckDT Is Nothing Then _LastCheckDT = Now.AddYears(-1)
            ' Check on start and every 5 minutes
            If _LastDT Is Nothing OrElse Not _LastDT.HasValue OrElse _LastCheckDT.Value.AddMinutes(_CHECK_DT_PERIOD) < Now Then
                ' For initial let's to start with 1 year before current time
                _LastDT = Now.AddYears(-1)

                ' check creation date for a system workgroup
                If SystemWorkgroup IsNot Nothing AndAlso SystemWorkgroup.License.isValidLicense Then
                    Dim tVal As Long = SystemWorkgroup.License.GetParameterMaxByID(ecLicenseParameter.CreatedAt)
                    If tVal <> 0 AndAlso tVal <> -1 AndAlso tVal <> UNLIMITED_DATE AndAlso tVal <> UNLIMITED_VALUE Then compareDate(Date.FromBinary(tVal))
                    compareDate(SystemWorkgroup.Created)
                    compareDate(SystemWorkgroup.LastVisited)
                End If

                ' check creation date for a system workgroup
                If ActiveWorkgroup IsNot Nothing AndAlso ActiveWorkgroup.License.isValidLicense Then
                    Dim tVal As Long = ActiveWorkgroup.License.GetParameterMaxByID(ecLicenseParameter.CreatedAt)
                    If tVal <> 0 AndAlso tVal <> -1 AndAlso tVal <> UNLIMITED_DATE AndAlso tVal <> UNLIMITED_VALUE Then compareDate(Date.FromBinary(tVal))
                    compareDate(ActiveWorkgroup.Created)
                    compareDate(ActiveWorkgroup.LastVisited)
                End If

                Try
                    ' Try to get the max datetime from different tables
                    Dim sSQl As String = "SELECT TOP 1 DT FROM (" + _
                                         "(SELECT TOP 1 DT FROM Logs ORDER BY DT DESC ) UNION " + _
                                         "(SELECT TOP 1 CASE WHEN LastVisited > LastModify THEN LastVisited ELSE LastModify END as DT FROM Projects ORDER BY LastVisited DESC, LastModify DESC, Created DESC) UNION " + _
                                         "(SELECT TOP 1 LastVisited as DT FROM UserWorkgroups ORDER BY LastVisited DESC) UNION " + _
                                         "(SELECT TOP 1 LastVisited as DT FROM Users ORDER BY LastVisited DESC) UNION " + _
                                         "(SELECT TOP 1 ModifyDate as DT FROM UserData ORDER BY ModifyDate DESC) UNION " + _
                                         "(SELECT TOP 1 ModifyDate as DT FROM ModelStructure ORDER BY ModifyDate DESC)" + _
                                         ") as DT ORDER BY 1 DESC"

                    Dim tData As Object = Database.ExecuteScalarSQL(sSQl)
                    If tData IsNot Nothing AndAlso Not IsDBNull(tData) Then compareDate(CDate(tData))
                Catch ex As Exception
                End Try

                If _LastDT.HasValue Then _LastDT.Value.AddHours(-12)
                _LastCheckDT = Now
            End If

            Dim DT As DateTime = Date.Now
            If _LastDT.HasValue AndAlso _LastDT > DT Then DT = _LastDT.Value

            Dim sNow As String = DT.ToString
            ' D3966 ==
            Return CDate(sNow).ToBinary ' Only date, without current time
        End Function

        ' D3946 ===
        Private Function _licInstanceID(ByVal tWorkgroup As clsWorkgroup, ByVal Parameter As Object) As Long
            Return GetInstanceID()
        End Function
        ' D3946 ==

        ' D3965 ===
        Private Function _licSelfHost(ByVal tWorkgroup As clsWorkgroup, ByVal Parameter As Object) As Long
            Return 0
        End Function
        ' D3965 ==

        Private Function _licWorkgroupsTotal(ByVal tWorkgroup As clsWorkgroup, ByVal Parameter As Object) As Long ' D0261
            Return CLng(Database.ExecuteScalarSQL(String.Format("SELECT COUNT(ID) as cnt FROM Workgroups WHERE Status<>{0}", CInt(ecWorkgroupStatus.wsSystem))))  ' D0471
        End Function

        ' D0265 ===
        Private Function _licGetModelsCount(ByVal tWorkgroup As clsWorkgroup, ByVal fOnlyOnline As Boolean) As Long ' D0417 (Active changed to On-line)
            Dim WGID As Integer = -1
            If Not tWorkgroup Is Nothing Then
                If tWorkgroup.Status <> ecWorkgroupStatus.wsSystem Then WGID = tWorkgroup.ID
            End If
            ' D0498 ===
            Dim sCond = ""
            If fOnlyOnline Then
                '          ver 1 -- old                 ?                ?                    ver 0 -- old      ?               ver 2 (actual)        online            not deleted       active
                sCond = "((status&57344=8192 and (status&504=64 or status&504=128)) or (status&57344=0 and status&134=0) or (status&57344=16384 and status&1024<>0 and status&2048=0 and status&56=0))" ' D0803
            Else
                '           ver 2               
                sCond = "((status&57344<>16384) or (status&57344=16384 and (status&56=0 or status&56=8)))"     ' D4036
            End If
            If WGID > 0 Then sCond += CStr(IIf(sCond = "", "", " AND ")) + String.Format("WorkgroupID={0}", WGID)
            If sCond <> "" Then sCond = " WHERE " + sCond
            Dim cnt As Long = CLng(Database.ExecuteScalarSQL(String.Format("SELECT COUNT(ID) as cnt FROM Projects{0}", sCond)))   ' D0471
            ' D0404 + D0498 ==
            Return cnt
        End Function

        ' D0498 ===
        Private Function _licGetRoleGroupIDs(ByVal tWorkgroup As clsWorkgroup, ByVal sActionTypesList As String, ByVal tRoleLevel As ecRoleLevel) As String
            Dim sWG As String = ""
            If Not tWorkgroup Is Nothing Then sWG = String.Format("WorkgroupID={0}", tWorkgroup.ID)
            Dim GrpIDs As String = ""
            Dim sModSQL As String = String.Format("SELECT DISTINCT(G.ID) FROM RoleGroups as G LEFT JOIN RoleActions as A ON G.ID=A.RoleGroupID AND A.ActionType IN ({0}) AND A.Status=0 WHERE G.RoleLevel={2} AND G.Status<>-1 AND A.ID>0{1}", sActionTypesList, IIf(sWG = "", "", " AND G." + sWG), CInt(tRoleLevel))
            Dim tList As List(Of Dictionary(Of String, Object)) = Database.SelectBySQL(sModSQL)
            For Each tRow As Dictionary(Of String, Object) In tList
                If tRow.ContainsKey("ID") AndAlso Not IsDBNull(tRow("ID")) Then GrpIDs += CStr(IIf(GrpIDs = "", "", ",")) + CStr(CInt(tRow("ID")))
            Next
            Return GrpIDs
        End Function
        ' D0498 ==

        ''' <summary>
        ''' Get max count of users on model(s)
        ''' </summary>
        ''' <param name="tWorkgroup"></param>
        ''' <param name="tProject">Use Nothing for check all projects in Specified Workspace</param>
        ''' <param name="fCanModifyProject"></param>
        ''' <param name="fOnlyActive"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function _licGetMaxUsersCount(ByVal tWorkgroup As clsWorkgroup, ByVal tProject As clsProject, ByVal fCanModifyProject As Boolean, ByVal fOnlyActive As Boolean, fCheckGroupIDs As Boolean) As Long  ' D1482
            Dim tMax As Long = 0
            ' D0498 + D1482 ===
            If Not tWorkgroup Is Nothing AndAlso tWorkgroup.Status <> ecWorkgroupStatus.wsSystem Then
                Dim GrpIDs As String = ""
                If fCheckGroupIDs Then GrpIDs = _licGetRoleGroupIDs(tWorkgroup, CStr(CInt(ecActionType.at_mlModifyModelHierarchy)) + "," + CStr(CInt(ecActionType.at_mlModifyAlternativeHierarchy)) + "," + CStr(CInt(ecActionType.at_mlManageProjectOptions)), ecRoleLevel.rlModelLevel) ' D2549
                Dim sCond As String = ""
                If Not tProject Is Nothing Then sCond += String.Format(" AND W.ProjectID={0}", tProject.ID)
                If fOnlyActive Then sCond += " AND W.Status=0"
                If fCheckGroupIDs AndAlso GrpIDs <> "" Then sCond += String.Format(" AND W.GroupID {1}IN ({0})", GrpIDs, IIf(fCanModifyProject, "", "NOT ")) ' D2549
                Dim sSQL As String = String.Format("SELECT TOP 1 COUNT(W.ID) as cnt FROM Workspace W LEFT JOIN Projects P ON P.ID=W.ProjectID WHERE P.WorkgroupID={0}{1} GROUP BY W.ProjectID ORDER BY cnt DESC", tWorkgroup.ID, sCond)
                tMax = CLng(Database.ExecuteScalarSQL(sSQL))
            End If
            ' D0498 + D1482 ==
            Return tMax
        End Function

        ' D1482 ===
        Private Function _licGetWorkgroupUsersCount(ByVal tWorkgroup As clsWorkgroup) As Long
            Dim tMax As Long = 0
            If tWorkgroup IsNot Nothing Then
                Dim sSQL As String = String.Format("SELECT COUNT(W.ID) as cnt FROM UserWorkgroups W WHERE WorkgroupID={0}", tWorkgroup.ID)
                tMax = CLng(Database.ExecuteScalarSQL(sSQL))
            End If
            Return tMax
        End Function
        ' D1482 ==

        'Private Function _licGetOwnerProjects(ByVal tWorkgroup As clsWorkgroup, ByVal tUser As clsApplicationUser) As Long
        '    Dim cnt As Long = 0
        '    If Not tWorkgroup Is Nothing And Not tUser Is Nothing Then
        '        If tWorkgroup.Status = ecWorkgroupStatus.wsSystem Then Return 0 ' D0404
        '        'For Each tWS As clsWorkspace In UserWorkspaces(tUser.UserID)
        '        '    ' D0404 ===
        '        '    Dim tProject As clsProject = Project(tWS.ProjectID, ProjectsAll)
        '        '    If Not tProject Is Nothing Then
        '        '        If tProject.WorkgroupID = tWorkgroup.ID AndAlso CanModifyProject(tWS.ProjectID, tUser) Then cnt += 1
        '        '    End If
        '        '    ' D0404 ==
        '        'Next
        '    End If
        '    Return cnt
        'End Function

        'Private Function _licGetMaxOwnerProjects(ByVal tWorkgroup As clsWorkgroup, ByVal tUser As clsApplicationUser) As Long
        '    Dim tMax As Long = 0
        '    'If Not tWorkgroup Is Nothing Then
        '    '    Dim UsersLst As New ArrayList
        '    '    If tUser Is Nothing Then
        '    '        UsersLst = WorkgroupUsers(tWorkgroup.ID)
        '    '    Else
        '    '        UsersLst.Add(tUser)
        '    '    End If
        '    '    For Each tUsr As clsApplicationUser In UsersLst
        '    '        Dim curMax As Long = _licGetOwnerProjects(tWorkgroup, tUsr)
        '    '        If curMax > tMax Then tMax = curMax
        '    '    Next
        '    'End If
        '    Return tMax
        'End Function
        ' D0265 ==

        Private Function _licModelsTotal(ByVal tWorkgroup As clsWorkgroup, ByVal Parameter As Object) As Long       ' D0261
            Return _licGetModelsCount(tWorkgroup, False)    ' D0265
        End Function

        ' D0264 ===
        Private Function _licOnlineModelsTotal(ByVal tWorkgroup As clsWorkgroup, ByVal Parameter As Object) As Long ' D0417 (Active changed to On-line)
            Return _licGetModelsCount(tWorkgroup, True)     ' D0265
        End Function

        ' Version for count Owners as Admin, WkgManager, ProjectOrganizer;
        'Private Function _licOwners(ByVal tWorkgroup As clsWorkgroup, ByVal Parameter As Object) As Long
        '    If tWorkgroup.Status = ecWorkgroupStatus.wsSystem Then Return 0 ' D0265
        '    Dim cnt As Long = 0
        '    If Not tWorkgroup Is Nothing Then
        '        ' D0498 ===
        '        Dim GrpIDs As String = _licGetRoleGroupIDs(tWorkgroup, CStr(CInt(ecActionType.at_alCreateNewModel)) + "," + CStr(CInt(ecActionType.at_alUploadModel)) + "," + CStr(CInt(ecActionType.at_alManageAnyModel)), ecRoleLevel.rlApplicationLevel)
        '        If GrpIDs <> "" Then
        '            Dim sSQL As String = String.Format("SELECT COUNT(ID) as cnt FROM UserWorkgroups WHERE WorkgroupID={0} AND RoleGroupID In ({1})", tWorkgroup.ID, GrpIDs) ' D2582
        '            cnt = CLng(Database.ExecuteScalarSQL(sSQL))
        '        End If
        '        ' D0498 ==
        '    End If
        '    Return cnt
        'End Function

        ' Version for count Owners only as ProjectOrganizers AND Workgroup Managers;
        ' D2780 ===
        Private Function _licOwners(ByVal tWorkgroup As clsWorkgroup, ByVal Parameter As Object) As Long
            Dim cnt As Long = 0
            If tWorkgroup IsNot Nothing AndAlso tWorkgroup.Status <> ecWorkgroupStatus.wsSystem Then
                ' D2790 ===
                Dim GrpIDs As String = String.Format("{0},{1}", tWorkgroup.GetDefaultRoleGroupID(ecRoleLevel.rlApplicationLevel, ecRoleGroupType.gtProjectOrganizer), tWorkgroup.GetDefaultRoleGroupID(ecRoleLevel.rlApplicationLevel, ecRoleGroupType.gtWorkgroupManager))
                Dim sSQL As String = String.Format("SELECT COUNT(ID) as cnt FROM UserWorkgroups WHERE WorkgroupID={0} AND RoleGroupID IN ({1})", tWorkgroup.ID, GrpIDs)
                ' D2790 ==
                cnt = CLng(Database.ExecuteScalarSQL(sSQL))
            End If
            Return cnt
        End Function
        ' D0264 + D2780 ==

        ' D0265 ===
        ' -D2548
        'Private Function _licConcurrentEvaluators(ByVal tWorkgroup As clsWorkgroup, ByVal Parameter As Object) As Long
        '    Dim tProject As clsProject = Nothing
        '    If Not Parameter Is Nothing Then
        '        If TypeOf (Parameter) Is clsProject Then tProject = CType(Parameter, clsProject)
        '    End If
        '    Return _licGetMaxUsersCount(tWorkgroup, tProject, False, True, True)    ' D1482
        'End Function

        ' -D2548
        'Private Function _licEvaluators(ByVal tWorkgroup As clsWorkgroup, ByVal Parameter As Object) As Long
        '    Dim tProject As clsProject = Nothing
        '    If Not Parameter Is Nothing Then
        '        If TypeOf (Parameter) Is clsProject Then tProject = CType(Parameter, clsProject)
        '    End If
        '    Return _licGetMaxUsersCount(tWorkgroup, tProject, False, False, True)   ' D1482
        'End Function

        Private Function _licFacilitatorsInModel(ByVal tWorkgroup As clsWorkgroup, ByVal Parameter As Object) As Long
            Dim tProject As clsProject = Nothing
            If Not Parameter Is Nothing Then
                If TypeOf (Parameter) Is clsProject Then tProject = CType(Parameter, clsProject)
            End If
            Return _licGetMaxUsersCount(tWorkgroup, tProject, True, False, True)    ' D1482
        End Function

        ' D1482 ===
        Private Function _licMaxUsersInWorkgroup(ByVal tWorkgroup As clsWorkgroup, ByVal Parameter As Object) As Long
            Return _licGetWorkgroupUsersCount(tWorkgroup)
        End Function

        Private Function _licMaxUsersInProject(ByVal tWorkgroup As clsWorkgroup, ByVal Parameter As Object) As Long
            Dim tProject As clsProject = Nothing
            If Not Parameter Is Nothing Then
                If TypeOf (Parameter) Is clsProject Then tProject = CType(Parameter, clsProject)
            End If
            Return _licGetMaxUsersCount(tWorkgroup, tProject, False, False, False)  ' D1482
        End Function
        ' D1482 ==

        ' -D2548
        'Private Function _licModelsPerOwner(ByVal tWorkgroup As clsWorkgroup, ByVal Parameter As Object) As Long
        '    Dim tUser As clsApplicationUser = Nothing
        '    If Not Parameter Is Nothing Then
        '        If TypeOf (Parameter) Is clsApplicationUser Then tUser = CType(Parameter, clsApplicationUser)
        '    End If
        '    Return _licGetMaxOwnerProjects(tWorkgroup, tUser)
        'End Function
        ' D0265 ==

        ' D0285 ===
        Private Function _licSynchronous(ByVal tWorkgroup As clsWorkgroup, ByVal Parameter As Object) As Long
            Return CLng(IIf(_TEAMTIME_AVAILABLE, 1, 0))   ' D0494 + D0513
            'Return CLng(IIf(Options.ShowDraftPages Or Not isDraftPage(_PAGESLIST_SYNCHRONOUS) < 0, 1, 0))    ' D0315 + D0459
        End Function

        Private Function _licSpyron(ByVal tWorkgroup As clsWorkgroup, ByVal Parameter As Object) As Long
            If clsLicense.SPYRON_ALLOW_FOR_ALL Then Return 1 ' D0820
            Return CLng(IIf(_SPYRON_AVAILABLE, 1, 0))     ' D0513
            'Return CLng(IIf(_SPYRON_AVAILABLE And (Options.ShowDraftPages Or Not isDraftPage(_PAGESLIST_SPYRON)), 1, 0)) ' D0315 + D0459
        End Function
        ' D0285 ==

        ' D0741 ===
        Private Function _licResourceAligner(ByVal tWorkgroup As clsWorkgroup, ByVal Parameter As Object) As Long
            Return CLng(IIf(_RA_AVAILABLE, 1, 0))
        End Function
        ' D0741 ==

        ' D0741 ===
        Private Function _licResourceAlignerUseGurobi(ByVal tWorkgroup As clsWorkgroup, ByVal Parameter As Object) As Long
            Return CLng(IIf(_RA_GUROBI_AVAILABLE, 1, 0))
        End Function
        ' D0741 ==

        ' D0912 ===
        Private Function _licExportEnabled(ByVal tWorkgroup As clsWorkgroup, ByVal Parameter As Object) As Long
            Return CLng(IIf(_EXPORT_AVAILABLE, 1, 0))
        End Function

        Private Function _licCommercialUse(ByVal tWorkgroup As clsWorkgroup, ByVal Parameter As Object) As Long ' D0917
            Return CLng(IIf(_COMMERCIAL_USE, 1, 0))   ' D0917
        End Function

        Private Function _licMaxObjectives(ByVal tWorkgroup As clsWorkgroup, ByVal Parameter As Object) As Long
            Return 0
        End Function

        Private Function _licMaxLevels(ByVal tWorkgroup As clsWorkgroup, ByVal Parameter As Object) As Long
            Return 0
        End Function

        Private Function _licMaxAlternatives(ByVal tWorkgroup As clsWorkgroup, ByVal Parameter As Object) As Long
            Return 0
        End Function

        ' D4564 ===
        Private Function GetProjectsCountExceptMaster(tLst As List(Of clsProject)) As Long
            Dim tCnt As Long = 0
            If tLst IsNot Nothing Then
                tLst.ForEach(Sub(p) If p.ProjectStatus <> ecProjectStatus.psMasterProject AndAlso p.ProjectStatus <> ecProjectStatus.psTemplate Then tCnt += 1)
            End If
            Return tCnt
        End Function
        ' D4564 ==

        Private Function _licLifetimeProjects(ByVal tWorkgroup As clsWorkgroup, ByVal Parameter As Object) As Long
            ' Use Parameter as int value for ignore Project with ID on get max value of existing projects inside the workgroup (case 267)
            If tWorkgroup Is Nothing Or CanvasMasterDBVersion < "0.992" Then Return 0
            Dim tDBValue As Long = 0
            Dim tPrjValue As Long = 0
            Dim tVal As Object = Database.ExecuteScalarSQL(String.Format("SELECT LifetimeProjects FROM {0} WHERE {1}={2}", _TABLE_WORKGROUPS, _FLD_WORKGROUPS_ID, tWorkgroup.ID))
            If Not IsDBNull(tVal) Then tDBValue = CInt(tVal)
            ' D4564 ===
            Dim tEncValue As clsExtra = DBExtraRead(clsExtra.Params2Extra(tWorkgroup.ID, ecExtraType.Workgroup, ecExtraProperty.LifetimeProjects))
            If tEncValue IsNot Nothing Then
                Dim sVal As String = Service.CryptService.DecodeString(CStr(tEncValue.Value), Nothing, False)
                Dim tDBValueEnc As Long = 0
                If Long.TryParse(sVal, tDBValueEnc) Then
                    tDBValueEnc = tDBValueEnc Xor tWorkgroup.ID
                    If tDBValueEnc > tDBValue Then tDBValue = tDBValueEnc
                End If
            End If
            ' D5464 ==
            Dim sCond As String = "((status&57344<>16384) or (status&57344=16384 and (status&56=0 or status&56=8)))"     ' D4038
            If tWorkgroup.Status = ecWorkgroupStatus.wsSystem Then
                tVal = Database.ExecuteScalarSQL(String.Format("SELECT COUNT(ID) FROM {0} WHERE {1}", _TABLE_PROJECTS, sCond))    ' D4038
                If Not IsDBNull(tVal) Then tPrjValue = CInt(tVal) Else tPrjValue = GetProjectsCountExceptMaster(DBProjectsAll())    ' D4564
            Else
                ' D0960 ===
                Dim sSQL As String = String.Format("SELECT COUNT(ID) FROM {0} WHERE {1}={2} AND {3}", _TABLE_PROJECTS, _FLD_PROJECTS_WRKGID, tWorkgroup.ID, sCond)  ' D4038
                If Parameter IsNot Nothing AndAlso TypeOf (Parameter) Is Integer Then sSQL = String.Format("{0} AND {1}<>{2}", sSQL, _FLD_PROJECTS_ID, CInt(Parameter))
                tVal = Database.ExecuteScalarSQL(sSQL)
                ' D0960 ==
                If Not IsDBNull(tVal) Then tPrjValue = CInt(tVal) Else tPrjValue = GetProjectsCountExceptMaster(DBProjectsByWorkgroupID(tWorkgroup.ID))    ' D4564
            End If
            If tDBValue < tPrjValue Then tDBValue = tPrjValue
            If tWorkgroup.LifeTimeProjects < tDBValue Then tWorkgroup.LifeTimeProjects = CInt(tDBValue) ' D0923
            Return tDBValue
        End Function

        ' -D2548
        'Private Function _licViewOnlyUsers(ByVal tWorkgroup As clsWorkgroup, ByVal Parameter As Object) As Long
        '    Return 0
        'End Function
        ' D0912 ==

        ' D2057 ===
        Private Function _licRisk(ByVal tWorkgroup As clsWorkgroup, ByVal Parameter As Object) As Long
            Return CLng(IIf(_RISK_ENABLED, 1, 0))
        End Function
        ' D2057 ==

        ' D3586 ===
        Private Function _licRiskTreatments(ByVal tWorkgroup As clsWorkgroup, ByVal Parameter As Object) As Long
            Return CLng(IIf(_RISK_ENABLED, 1, 0))
        End Function

        Private Function _licRiskTreatmentsOptimization(ByVal tWorkgroup As clsWorkgroup, ByVal Parameter As Object) As Long
            Return CLng(IIf(_RISK_ENABLED, 1, 0))
        End Function
        ' D3586 ==

        ' D2644 ===
        Public Sub WorkgroupLicenseInit(ByRef tWorkgroup As clsWorkgroup)
            If tWorkgroup IsNot Nothing Then
                tWorkgroup.License.Parameters.Clear()
                ' D0471 ===
                ' common params for each workgroup
                LicenseAddDefaultParameter(tWorkgroup, ecLicenseParameter.CreatedAt)   ' D3965
                LicenseAddDefaultParameter(tWorkgroup, ecLicenseParameter.ExpirationDate)

                If tWorkgroup.License.isSystemLicense Then
                    LicenseAddDefaultParameter(tWorkgroup, ecLicenseParameter.MaxWorkgroupsTotal)

                    LicenseAddDefaultParameter(tWorkgroup, ecLicenseParameter.MaxProjectsTotal)
                    LicenseAddDefaultParameter(tWorkgroup, ecLicenseParameter.MaxLifetimeProjects)  ' D0924

                    LicenseAddDefaultParameter(tWorkgroup, ecLicenseParameter.MaxUsersInWorkgroup)      ' D1482
                Else

                    ' for non-system workgroup
                    LicenseAddDefaultParameter(tWorkgroup, ecLicenseParameter.MaxProjectsTotal)
                    LicenseAddDefaultParameter(tWorkgroup, ecLicenseParameter.MaxLifetimeProjects)  ' D0924
                    LicenseAddDefaultParameter(tWorkgroup, ecLicenseParameter.MaxProjectsOnline)

                    LicenseAddDefaultParameter(tWorkgroup, ecLicenseParameter.MaxUsersInWorkgroup)      ' D1482
                    LicenseAddDefaultParameter(tWorkgroup, ecLicenseParameter.MaxProjectCreatorsInWorkgroup)

                    LicenseAddDefaultParameter(tWorkgroup, ecLicenseParameter.MaxUsersInProject)
                    LicenseAddDefaultParameter(tWorkgroup, ecLicenseParameter.MaxPMsInProject)
                    ' D0909 ===
                    LicenseAddDefaultParameter(tWorkgroup, ecLicenseParameter.MaxObjectives)
                    LicenseAddDefaultParameter(tWorkgroup, ecLicenseParameter.MaxLevelsBelowGoal)
                    LicenseAddDefaultParameter(tWorkgroup, ecLicenseParameter.MaxAlternatives)
                    ' D0909 ==

                    'LicenseAddDefaultParameter(tWorkgroup, ecLicenseParameter.MaxEvaluatorsInModel)    ' NotInUse
                    'LicenseAddDefaultParameter(tWorkgroup, ecLicenseParameter.MaxModelsPerOwner)   ' NotInUse
                    'LicenseAddDefaultParameter(tWorkgroup, ecLicenseParameter.MaxConcurrentEvaluatorsInModel)       ' NotInUse
                    'LicenseAddDefaultParameter(tWorkgroup, ecLicenseParameter.MaxViewOnlyUsers)     ' NotInUse
                End If
                LicenseAddDefaultParameter(tWorkgroup, ecLicenseParameter.TeamTimeEnabled)
                LicenseAddDefaultParameter(tWorkgroup, ecLicenseParameter.SpyronEnabled)
                LicenseAddDefaultParameter(tWorkgroup, ecLicenseParameter.ResourceAlignerEnabled)  ' D0741
                If Not tWorkgroup.License.isSystemLicense Then LicenseAddDefaultParameter(tWorkgroup, ecLicenseParameter.AllowUseGurobi) ' D3923 + D3925
                ' D0471 ==
                LicenseAddDefaultParameter(tWorkgroup, ecLicenseParameter.ExportEnabled)  ' D0909
                LicenseAddDefaultParameter(tWorkgroup, ecLicenseParameter.CommercialUseEnabled)  ' D0909 + D0917
                LicenseAddDefaultParameter(tWorkgroup, ecLicenseParameter.RiskEnabled)    ' D2057

                If Not tWorkgroup.License.isSystemLicense Then  ' D3952
                    LicenseAddDefaultParameter(tWorkgroup, ecLicenseParameter.RiskTreatments)               ' D3586
                    LicenseAddDefaultParameter(tWorkgroup, ecLicenseParameter.RiskTreatmentsOptimization)   ' D3586
                Else
                    LicenseAddDefaultParameter(tWorkgroup, ecLicenseParameter.isSelfHost)   ' D3965
                End If

                LicenseAddDefaultParameter(tWorkgroup, ecLicenseParameter.InstanceID)   ' D3946
            End If
        End Sub
        ' D2644 ==

        Public Sub LicenseAddDefaultParameter(ByVal tWorkgroup As clsWorkgroup, ByVal tParamID As ecLicenseParameter) ' D0261 + D0913
            Dim licParam As New clsLicenseParameter
            licParam.ID = tParamID
            licParam.Workgroup = tWorkgroup ' D0261
            Select Case tParamID
                Case ecLicenseParameter.ExpirationDate
                    licParam.FunctionValue = New LicenseValueFunction(AddressOf _licExpirationDate)
                    licParam.Name = "licExpirationDate"
                Case ecLicenseParameter.MaxWorkgroupsTotal
                    licParam.FunctionValue = New LicenseValueFunction(AddressOf _licWorkgroupsTotal)
                    licParam.Name = "licWorkgroupsTotal"
                Case ecLicenseParameter.MaxProjectsTotal
                    licParam.FunctionValue = New LicenseValueFunction(AddressOf _licModelsTotal)
                    licParam.Name = "licModelsTotal"
                    ' D0264 + D0417 ===
                Case ecLicenseParameter.MaxProjectsOnline
                    licParam.FunctionValue = New LicenseValueFunction(AddressOf _licOnlineModelsTotal)
                    licParam.Name = "licOnlineModelsTotal"
                    ' D0417 ==
                Case ecLicenseParameter.MaxProjectCreatorsInWorkgroup
                    licParam.FunctionValue = New LicenseValueFunction(AddressOf _licOwners)
                    licParam.Name = "licOwners"
                    ' -D2548
                    'Case ecLicenseParameter.MaxConcurrentEvaluatorsInModel
                    '    licParam.FunctionValue = New LicenseValueFunction(AddressOf _licConcurrentEvaluators)
                    '    licParam.Name = "licConcurrentModelEvaluators"
                    ' -D2548
                    'Case ecLicenseParameter.MaxEvaluatorsInModel
                    '    licParam.FunctionValue = New LicenseValueFunction(AddressOf _licEvaluators)
                    '    licParam.Name = "licModelEvaluators"
                Case ecLicenseParameter.MaxPMsInProject
                    licParam.FunctionValue = New LicenseValueFunction(AddressOf _licFacilitatorsInModel)
                    licParam.Name = "licModelFacilitators"
                    ' -D2548
                    'Case ecLicenseParameter.MaxModelsPerOwner
                    '    licParam.FunctionValue = New LicenseValueFunction(AddressOf _licModelsPerOwner)
                    '    licParam.Name = "licModelsPerOwner"
                    ' D0264 ==
                    ' D0285 ===
                Case ecLicenseParameter.TeamTimeEnabled
                    licParam.FunctionValue = New LicenseValueFunction(AddressOf _licSynchronous)
                    licParam.Name = "licSynchronousEnabled"
                Case ecLicenseParameter.SpyronEnabled
                    licParam.FunctionValue = New LicenseValueFunction(AddressOf _licSpyron)
                    licParam.Name = "licSpyronEnabled"
                    ' D0285 ==
                    ' D0741 ===
                Case ecLicenseParameter.ResourceAlignerEnabled
                    licParam.FunctionValue = New LicenseValueFunction(AddressOf _licResourceAligner)
                    licParam.Name = "licRAEnabled"
                    ' D0741 ==
                    ' D3923 ===
                Case ecLicenseParameter.AllowUseGurobi
                    licParam.FunctionValue = New LicenseValueFunction(AddressOf _licResourceAlignerUseGurobi)
                    licParam.Name = "licAllowUseGurobi"
                    ' D3923 ==
                    ' D0909 ===
                Case ecLicenseParameter.ExportEnabled
                    licParam.FunctionValue = New LicenseValueFunction(AddressOf _licExportEnabled)   ' D0912
                    licParam.Name = "licExportEnabled"
                Case ecLicenseParameter.CommercialUseEnabled
                    licParam.FunctionValue = New LicenseValueFunction(AddressOf _licCommercialUse)   ' D0912 + D0917
                    licParam.Name = "licCommercialUseEnabled"   ' D0917
                Case ecLicenseParameter.MaxLifetimeProjects
                    licParam.FunctionValue = New LicenseValueFunction(AddressOf _licLifetimeProjects)   ' D0912
                    licParam.Name = "licMaxLifetimeProjects"
                Case ecLicenseParameter.MaxObjectives
                    licParam.FunctionValue = New LicenseValueFunction(AddressOf _licMaxObjectives)   ' D0912
                    licParam.Name = "licMaxObjectives"
                Case ecLicenseParameter.MaxLevelsBelowGoal
                    licParam.FunctionValue = New LicenseValueFunction(AddressOf _licMaxLevels)   ' D0912
                    licParam.Name = "licMaxLevels"
                Case ecLicenseParameter.MaxAlternatives
                    licParam.FunctionValue = New LicenseValueFunction(AddressOf _licMaxAlternatives)   ' D0912
                    licParam.Name = "licMaxAlternatives"
                    ' -D2548
                    'Case ecLicenseParameter.MaxViewOnlyUsers
                    '    licParam.FunctionValue = New LicenseValueFunction(AddressOf _licViewOnlyUsers)   ' D0912
                    '    licParam.Name = "licMaxViewOnlyUsers"
                    ' D0909 ==
                    ' D1482 ===
                Case ecLicenseParameter.MaxUsersInWorkgroup
                    licParam.FunctionValue = New LicenseValueFunction(AddressOf _licMaxUsersInWorkgroup)
                    licParam.Name = "licMaxUsersInWorkgroup"
                Case ecLicenseParameter.MaxUsersInProject
                    licParam.FunctionValue = New LicenseValueFunction(AddressOf _licMaxUsersInProject)
                    licParam.Name = "licMaxUsersInProject"
                    ' D1482 ==
                    ' D2057 ===
                Case ecLicenseParameter.RiskEnabled
                    licParam.FunctionValue = New LicenseValueFunction(AddressOf _licRisk)
                    licParam.Name = "licRiskEnabled"
                    ' D2057 ==
                    ' D3586 ===
                Case ecLicenseParameter.RiskTreatments
                    licParam.FunctionValue = New LicenseValueFunction(AddressOf _licRiskTreatments)
                    licParam.Name = "licRiskTreatments"
                Case ecLicenseParameter.RiskTreatmentsOptimization
                    licParam.FunctionValue = New LicenseValueFunction(AddressOf _licRiskTreatmentsOptimization)
                    licParam.Name = "licRiskTreatmentsOptimization"
                    ' D3586 ==
                    ' D0741 ===
                Case ecLicenseParameter.InstanceID
                    licParam.FunctionValue = New LicenseValueFunction(AddressOf _licInstanceID)
                    licParam.Name = "licInstanceID"
                    ' D0741 ==
                    ' D3965 ===
                Case ecLicenseParameter.isSelfHost
                    licParam.FunctionValue = New LicenseValueFunction(AddressOf _licSelfHost)
                    licParam.Name = "licIsSelfHost"
                    ' D3965 ==

            End Select
            If Not licParam Is Nothing Then
                tWorkgroup.License.Parameters.Add(licParam)
            End If
        End Sub
        ' D0257 ==

        ' D0264 ===
        Public Function LicenseErrorMessage(ByVal tLicense As clsLicense, ByVal tWrongID As ecLicenseParameter, Optional ByVal fShowMax As Boolean = True) As String   ' D0913 + D0925
            If tLicense Is Nothing Then Return ""
            ' D0925 + D2439 ===
            Dim sMsg As String
            Select Case tWrongID
                Case ecLicenseParameter.MaxUsersInWorkgroup
                    sMsg = String.Format(ResString("errLicenseMaxWkgUsers"), tLicense.GetParameterMaxByID(tWrongID))
                    fShowMax = False
                Case ecLicenseParameter.MaxUsersInProject
                    sMsg = String.Format(ResString("errLicenseMaxPrjUsers"), tLicense.GetParameterMaxByID(tWrongID))
                    fShowMax = False
                Case ecLicenseParameter.InstanceID  ' D3947
                    sMsg = String.Format(ResString("errLicenseWrongInstanceID"), GetInstanceID_AsString)    ' D3947
                Case Else
                    sMsg = String.Format(ResString("errLicense"), ResString(tLicense.GetParameterNameByID(tWrongID)))
            End Select
            ' D2439 ==
            If fShowMax AndAlso tWrongID <> ecLicenseParameter.InstanceID Then  ' D3947
                Dim tMax As Long = tLicense.GetParameterMaxByID(tWrongID)
                ' D0954 ===
                Dim sMax As String = tMax.ToString
                If tWrongID = ecLicenseParameter.ExpirationDate Then sMax = DateTime.FromBinary(tMax).ToShortDateString
                If tMax <> UNLIMITED_VALUE Then sMsg = String.Format("{0} ({1})", sMsg, sMax)
                ' D0954 ==
            End If
            Return sMsg
            ' D0925 ==
        End Function

        Public Sub LicenseInitError(ByVal sMessage As String, Optional ByVal fOnlyWarning As Boolean = False)
            If sMessage = "" Then sMessage = ResString("errLicense")
            If ApplicationError.Status <> ecErrorStatus.errWrongLicense Then ApplicationError.Init(ecErrorStatus.errWrongLicense, -1, sMessage, fOnlyWarning) ' D0257 + D0262 + D0264 + D0460
            'If ApplicationError.Status <> ecErrorStatus.errWrongLicense Then ApplicationError.Init(ecErrorStatus.errWrongLicense, _PGID_ERROR_503, sMessage, fOnlyWarning) ' D0257 + D0262 + D0264
        End Sub
        ' D0264 ==

        ' D0262 ===
        Public Function CheckLicense(ByVal tWorkgroup As clsWorkgroup, ByRef sLicenseMessage As String, ByVal fCheckOnlyExpiration As Boolean) As Boolean    ' D0266
            If Not Options.CheckLicense Or tWorkgroup Is Nothing Then Return True ' D0315
            If Not tWorkgroup.License.isValidLicense Then
                If Not tWorkgroup.License.isValidLicense Then
                    Dim sErrorMessage As String = ResString("errLicenseFile")
                    If tWorkgroup.License.LicenseKey.Trim = "" Then
                        sErrorMessage = ResString("errNoLicenseKey")
                    Else
                        If tWorkgroup.License.LicenseContent Is Nothing Then sErrorMessage = ResString("errNoLicenseData")
                    End If
                    If Not sLicenseMessage Is Nothing Then sLicenseMessage = sErrorMessage
                    Return False
                End If
            Else
                ' D0264 + D0266 ===
                Dim tLicItem As ecLicenseParameter = ecLicenseParameter.Unknown   ' D0913
                Dim isValid As Boolean = True
                If fCheckOnlyExpiration Then
                    tLicItem = ecLicenseParameter.ExpirationDate
                    isValid = tWorkgroup.License.CheckParameterByID(tLicItem, Nothing, True)
                    ' D3947 ===
                    If isValid Then
                        tLicItem = ecLicenseParameter.InstanceID
                        isValid = tWorkgroup.License.CheckParameterByID(tLicItem, Nothing, True)
                    End If
                    ' D3947 ==
                    ' D6568 ===
                    If isValid Then
                        tLicItem = ecLicenseParameter.MaxWorkgroupsTotal
                        isValid = tWorkgroup.License.CheckParameterByID(tLicItem, Nothing, True)
                    End If
                    ' D6568 ==
                Else
                    ' D4871 ===
                    Dim WrongItem As Long = -1
                    isValid = tWorkgroup.License.CheckAllParameters(WrongItem) ' D2256
                    If Not isValid Then tLicItem = CType(WrongItem, ecLicenseParameter)
                    ' D4871 ==
                    'isValid = tWorkgroup.License.CheckAllParameters(CLng(tLicItem)) ' D2256
                End If
                If Not isValid And tLicItem <> ecLicenseParameter.Unknown Then
                    If Not sLicenseMessage Is Nothing Then sLicenseMessage = LicenseErrorMessage(tWorkgroup.License, tLicItem)
                    Return False
                End If
                ' D0264 + D0266 ==
            End If
            Return True
        End Function

        '        Public Function CheckAllLicenses(ByRef sErrorMessage As String, ByRef tWrongWorkgroup As clsWorkgroup, ByVal fCheckOnlyExpiration As Boolean) As Boolean    ' D0266
        '            tWrongWorkgroup = Nothing
        '            sErrorMessage = ""
        '            If Not Options.CheckLicense Or Workgroups Is Nothing Then Return True ' D0315
        '            For Each tWG As clsWorkgroup In Workgroups
        '                If Not CheckLicense(tWG, sErrorMessage, fCheckOnlyExpiration) Then  ' D0266
        '                    tWrongWorkgroup = tWG
        '                    Return False
        '                End If
        '            Next
        '            Return True
        '        End Function
        '        ' D0262 ==

        ' D0912 ===
        Public ReadOnly Property isExportAvailable() As Boolean
            Get
                If Not _EXPORT_AVAILABLE Then Return False
                Dim fPassed As Boolean = False
                Dim SysWG As clsWorkgroup = SystemWorkgroup
                If Options.CheckLicense And Not SysWG Is Nothing Then
                    fPassed = SysWG.License.CheckParameterByID(ecLicenseParameter.ExportEnabled, Nothing, True) ' D0913
                    If fPassed And Not ActiveWorkgroup Is Nothing And Not SysWG Is ActiveWorkgroup Then
                        fPassed = ActiveWorkgroup.License.CheckParameterByID(ecLicenseParameter.ExportEnabled, Nothing, True)   ' D0913
                    End If
                End If
                Return fPassed
            End Get
        End Property

        Public ReadOnly Property isCommercialUseEnabled() As Boolean    ' D0917
            Get
                If Not _COMMERCIAL_USE Then Return False ' D0917
                Dim fPassed As Boolean = False
                Dim SysWG As clsWorkgroup = SystemWorkgroup
                If Options.CheckLicense And Not SysWG Is Nothing Then
                    fPassed = SysWG.License.CheckParameterByID(ecLicenseParameter.CommercialUseEnabled, Nothing, True) ' D0913 + D0917
                    If fPassed And Not ActiveWorkgroup Is Nothing And Not SysWG Is ActiveWorkgroup Then
                        fPassed = ActiveWorkgroup.License.CheckParameterByID(ecLicenseParameter.CommercialUseEnabled, Nothing, True)   ' D0913 + D0917
                    End If
                End If
                Return fPassed
            End Get
        End Property
        ' D0912 ==

        ' D2057 ===
        Public ReadOnly Property isRiskEnabled() As Boolean
            Get
                ' D6328 ===
                If _IsRisk Is Nothing Then
                    If _RISK_ENABLED AndAlso Options.CheckLicense Then  ' D2257
                        ' D2254 ===
                        If ActiveWorkgroup IsNot Nothing AndAlso ActiveWorkgroup.License.isValidLicense Then
                            _IsRisk = ActiveWorkgroup.License.CheckParameterByID(ecLicenseParameter.RiskEnabled, Nothing, True)
                        Else
                            If SystemWorkgroup IsNot Nothing AndAlso SystemWorkgroup.License IsNot Nothing Then _IsRisk = SystemWorkgroup.License.CheckParameterByID(ecLicenseParameter.RiskEnabled, Nothing, True) Else _IsRisk = False ' D2295
                        End If
                    End If
                End If
                Return _IsRisk.Value
                ' D6328 ==
            End Get
        End Property
        ' D2057 ==

        Public Sub isRiskForceValue(fRiskEnabled As Boolean)
            _IsRisk = fRiskEnabled
        End Sub

        ' D3586 ===
        Public ReadOnly Property isRiskTreatmentEnabled() As Boolean
            Get
                Dim fPassed As Boolean = isRiskEnabled
                If fPassed Then
                    If ActiveWorkgroup IsNot Nothing AndAlso ActiveWorkgroup.License.isValidLicense Then
                        fPassed = ActiveWorkgroup.License.CheckParameterByID(ecLicenseParameter.RiskTreatments, Nothing, True)
                    End If
                End If
                Return fPassed
            End Get
        End Property

        Public ReadOnly Property isRiskTreatmentOptimizationEnabled() As Boolean
            Get
                Dim fPassed As Boolean = isRiskTreatmentEnabled ' D3591
                If fPassed Then
                    If ActiveWorkgroup IsNot Nothing AndAlso ActiveWorkgroup.License.isValidLicense Then
                        fPassed = ActiveWorkgroup.License.CheckParameterByID(ecLicenseParameter.RiskTreatmentsOptimization, Nothing, True)
                    End If
                End If
                Return fPassed
            End Get
        End Property
        ' D3586 ==

        ' D2922 ===
        Public ReadOnly Property isRAAvailable() As Boolean
            Get
                Dim fPassed As Boolean = False
                Dim SysWG As clsWorkgroup = SystemWorkgroup
                If _RA_AVAILABLE AndAlso Options.CheckLicense And Not SysWG Is Nothing Then ' D4526
                    fPassed = SysWG.License.CheckParameterByID(ecLicenseParameter.ResourceAlignerEnabled, Nothing, True)
                    If fPassed And Not ActiveWorkgroup Is Nothing And Not SysWG Is ActiveWorkgroup Then
                        fPassed = ActiveWorkgroup.License.CheckParameterByID(ecLicenseParameter.ResourceAlignerEnabled, Nothing, True)
                    End If
                End If
                Return fPassed
            End Get
        End Property
        ' D2922 ==

        ' D3923 ===
        Public ReadOnly Property isGurobiAvailable() As Boolean
            Get
                Dim fPassed As Boolean = isRAAvailable AndAlso _RA_GUROBI_AVAILABLE ' D4526
                If fPassed Then
                    If ActiveWorkgroup IsNot Nothing AndAlso ActiveWorkgroup.License.isValidLicense Then
                        fPassed = ActiveWorkgroup.License.CheckParameterByID(ecLicenseParameter.AllowUseGurobi, Nothing, True)
                    End If
                End If
                Return fPassed
            End Get
        End Property
        ' D3923 ==

        ' D4512 ===
        Public ReadOnly Property isXAAvailable() As Boolean
            Get
                'Return _RA_XA_AVAILABLE     ' D7543
                Return _RA_XA_AVAILABLE AndAlso (Not _RA_XA_AVAILABLE_WHEN_DRAFT_ONLY OrElse LicenseOption_ShowDraft)  ' D7543 + D7603
            End Get
        End Property
        ' D4512 ==

        ' D4071 ===
        Public ReadOnly Property isBaronAvailable() As Boolean
            Get
                Return _RA_BARON_AVAILABLE  ' D7543
            End Get
        End Property

        'Public Function isRASolversAvailable(isRiskionOptimizer As Boolean) As Boolean
        '    If isRiskionOptimizer Then Return isGurobiAvailable AndAlso isBaronAvailable Else Return isGurobiAvailable
        'End Function
        ' D4071 ==

        ' D3965 ===
        Public ReadOnly Property isSelfHost() As Boolean
            Get
                Return SystemWorkgroup Is Nothing OrElse Not SystemWorkgroup.License.isValidLicense OrElse SystemWorkgroup.License.GetParameterMaxByID(ecLicenseParameter.isSelfHost) > 0
            End Get
        End Property
        ' D3965 ==

    End Class

End Namespace