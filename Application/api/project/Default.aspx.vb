Imports SpyronControls.Spyron.Core

Partial Class ProjectsWebAPI
    Inherits clsComparionCorePage

    Public _OPT_HISTORY_COUNT As Integer = 15       ' D6443
    Public _OPT_HISTORY_ALL_WKG As Boolean = False  ' D6443
    Public _OPT_HISTORY_AFTER As Date = Date.Parse("2019-11-06 0:00:00") ' D6443

    Delegate Sub funcProjectAction(tProject As clsProject, tData As Object, ByRef sError As String)    ' D5033

    Public Sub New()
        MyBase.New(_PGID_WEBAPI)
    End Sub

    Private Function _Page() As mpWebAPI
        Return CType(Master, mpWebAPI)
    End Function

    ' D5032 ===
    Public Function List(Optional Reload As Boolean = False) As jActionResult
        FetchIfNotAuthorized()
        Dim Res As New jActionResult
        Dim tLst As New jProjectsLimits
        If App.ActiveWorkgroup IsNot Nothing Then
            If App.ActiveWorkgroup.Status <> ecWorkgroupStatus.wsSystem Then
                ' D6326 ===
                If Not Str2Bool(WebConfigOption(_OPT_IGNORE_NEW_MASTERPRJ, "0")) AndAlso App.CanUserDoAction(ecActionType.at_alCreateNewModel, App.ActiveUserWorkgroup, App.ActiveWorkgroup) Then   ' D6398
                    Dim SessName As String = String.Format("CheckMaster_{0}", App.ActiveWorkgroup.ID)
                    If Session(SessName) Is Nothing Then
                        If App.CheckWorkgroupMasterProjects(App.ActiveWorkgroup, False, False) Then Reload = True
                        Session(SessName) = True
                    End If
                End If
                ' D6326 ==
                If Reload Then App.ActiveProjectsList = Nothing ' D5033
                Res.Data = jProject.GetProjectsList(App)    ' D5040
            Else
                Res.Message = "No projects in the System workgroup"
            End If
            If App.ActiveWorkgroup.License IsNot Nothing AndAlso App.ActiveWorkgroup.License.isValidLicense Then
                tLst.License_LifetimeLimit = App.ActiveWorkgroup.License.GetParameterMaxByID(ecLicenseParameter.MaxLifetimeProjects)
                tLst.License_LifetimeValue = App.ActiveWorkgroup.License.GetParameterValueByID(ecLicenseParameter.MaxLifetimeProjects)
                tLst.License_TotalLimit = App.ActiveWorkgroup.License.GetParameterMaxByID(ecLicenseParameter.MaxProjectsTotal)
                tLst.License_TotalValue = App.ActiveWorkgroup.License.GetParameterValueByID(ecLicenseParameter.MaxProjectsTotal)
                tLst.License_OnlineLimit = App.ActiveWorkgroup.License.GetParameterMaxByID(ecLicenseParameter.MaxProjectsOnline)
                tLst.License_OnlineValue = App.ActiveWorkgroup.License.GetParameterValueByID(ecLicenseParameter.MaxProjectsOnline)
                tLst.CanCreateNew = App.CanUserCreateNewProject(Nothing)
                Res.Tag = tLst
            End If
            Res.Result = If(Res.Message = "", ecActionResult.arSuccess, ecActionResult.arError)
        End If
        Return Res
    End Function

    Public Function List_Short(Optional Reload As Boolean = False) As jActionResult
        FetchIfNotAuthorized()
        Dim Res As New jActionResult With {.Result = ecActionResult.arError}
        If App.ActiveWorkgroup IsNot Nothing Then
            If App.ActiveWorkgroup.Status <> ecWorkgroupStatus.wsSystem Then
                Dim tLst As New List(Of jProjectShort)
                If Reload Then App.ActiveProjectsList = Nothing ' D5033
                App.ActiveProjectsList.ForEach(Sub(proj)
                                                   tLst.Add(jProjectShort.CreateFromBaseObject(proj))
                                               End Sub)
                Res.Result = ecActionResult.arSuccess
                Res.Data = tLst
            Else
                Res.Message = "No projects in the System workgroup"
            End If
        End If
        Return Res
    End Function

    ' D5033 ===
    Public Function ProjectbyID(ID As Integer) As jActionResult
        FetchIfNotAuthorized()
        ' D5040 ===
        Dim Res As New jActionResult With {
            .ObjectID = ID,
            .Data = jProject.GetProjectByID(App, ID)
        }
        If Res.Data IsNot Nothing Then
            ' D5040 ==
            Res.Result = ecActionResult.arSuccess
        Else
            FetchProjectNotFound()
        End If
        Return Res
        ' D5033 ==
    End Function

    ' D5033 ===
    Public Function ProjectByID_Short(ID As Integer) As jActionResult
        FetchIfNotAuthorized()
        Dim Res As New jActionResult
        Dim tPrj As clsProject = clsProject.ProjectByID(ID, App.ActiveProjectsList)
        If tPrj IsNot Nothing Then
            Res.Result = ecActionResult.arSuccess
            Res.Data = jProjectShort.CreateFromBaseObject(tPrj)
        Else
            FetchProjectNotFound()
        End If
        Return Res
        ' D5033 ==
    End Function

    Public Function ActiveProject() As jActionResult   ' D5033
        Return ProjectbyID(App.ProjectID)
    End Function

    Public Function ActiveProject_Short() As jActionResult ' D5033
        Return ProjectByID_Short(App.ProjectID)
    End Function

    Public Function Open(ID As Integer, Optional IgnoreOnlineUsers As Boolean = False, Optional sPasscode As String = "") As jActionResult  ' D7154
        FetchIfNotAuthorized()
        Dim tRes = New jActionResult
        Dim tPrj As clsProject = clsProject.ProjectByID(ID, App.ActiveProjectsList)
        If tPrj IsNot Nothing Then
            Dim OldPrjID As Integer = App.ProjectID
            If tPrj.isMarkedAsDeleted Then
                tRes.Message = ResString("msgCanOpenDeletedPrj")
                tRes.Result = ecActionResult.arWarning
            Else
                If tPrj.isValidDBVersion OrElse tPrj.isDBVersionCanBeUpdated Then
                    tRes.Result = ecActionResult.arSuccess
                    tRes.Data = If(tPrj.isValidDBVersion, "", "upgrade") ' D4740
                    ' D6600 ===
                    If (tPrj.isValidDBVersion) Then
                        App.CheckProjectManagerUsers(tPrj)
                        ' D7154 ===
                        If App.isRiskEnabled AndAlso Not String.IsNullOrEmpty(sPasscode) Then
                            Select Case sPasscode.ToLower
                                Case tPrj.PasscodeLikelihood.ToLower
                                    tPrj.ProjectManager.ActiveHierarchy = ECHierarchyID.hidLikelihood
                                    tPrj.ProjectManager.PipeParameters.CurrentParameterSet = tPrj.ProjectManager.PipeParameters.DefaultParameterSet
                                Case tPrj.PasscodeImpact.ToLower
                                    tPrj.ProjectManager.ActiveHierarchy = ECHierarchyID.hidImpact
                                    tPrj.ProjectManager.PipeParameters.CurrentParameterSet = tPrj.ProjectManager.PipeParameters.ImpactParameterSet
                            End Select
                        End If
                        ' D7154 ==
                    End If
                    ' D6600 ==
                    jProject.GetActionResultByProject(Me, tRes, tPrj)
                Else
                    tRes.Result = ecActionResult.arWarning
                    tRes.Message = ResString("msgCantReadProjectDBVersion")
                End If
            End If
            ' D4846 ===
            If tRes.Message = "" AndAlso Not IgnoreOnlineUsers Then
                If App.CanUserModifyProject(App.ActiveUser.UserID, ID, App.ActiveUserWorkgroup, clsWorkspace.WorkspaceByUserIDAndProjectID(App.ActiveUser.UserID, ID, App.Workspaces), App.ActiveWorkgroup) Then    ' D4939
                    Dim isAntigua As Boolean = tPrj.LockInfo IsNot Nothing AndAlso tPrj.LockInfo.LockStatus = ECLockStatus.lsLockForAntigua ' D5003
                    If Not isAntigua AndAlso Not tPrj.isTeamTimeLikelihood AndAlso Not tPrj.isTeamTimeImpact Then   ' D5003 + D7213
                        Dim tOnline As List(Of clsOnlineUserSession) = App.DBOnlineSessions()
                        Dim UsersList As String = ""
                        Dim PMEMail As String = ""
                        For Each tSess As clsOnlineUserSession In tOnline
                            If tSess.ProjectID = ID AndAlso tSess.UserID <> App.ActiveUser.UserID Then
                                Dim isPM As Boolean = App.CanUserModifyProject(tSess.UserID, ID, App.DBUserWorkgroupByUserIDWorkgroupID(tSess.UserID, App.ActiveWorkgroup.ID), App.DBWorkspaceByUserIDProjectID(tSess.UserID, ID), App.ActiveWorkgroup)
                                If isPM Then
                                    PMEMail += String.Format("{0}{1}", If(PMEMail = "", "", ", "), tSess.UserEmail)
                                End If
                                UsersList += String.Format("{0}&nbsp;&#149; {1}", If(UsersList = "", "", "<br>"), tSess.UserEmail)
                            End If
                        Next
                        If UsersList <> "" OrElse PMEMail <> "" Then
                            Dim sMsg As String = ""
                            ' D6114 ===
                            If UsersList <> "" Then UsersList = String.Format("<p>{0}<br>{1}</p>", ResString("lblOnlineUsersList"), UsersList)
                            If PMEMail = "" Then    ' no PMs in the model
                                sMsg = ResString("msgOpenProjectWithOnlineEvals")
                            Else
                                sMsg = String.Format(ResString("msgOpenProjectWithOnlinePM"), PMEMail, UsersList)
                            End If
                            ' D6114 ==
                            tRes.Data = "confirm_online"
                            tRes.Message = sMsg
                            App.ProjectID = OldPrjID
                        End If
                    End If
                End If
            End If
            ' D4846 ==
        Else
            FetchProjectNotFound()
        End If
        Return tRes
    End Function

    Public Function Close(Optional ID As Integer = -1) As jActionResult ' D5076
        FetchIfNotAuthorized()
        Dim Res As New jActionResult
        If App.ProjectID = ID OrElse ID = -1 Then   ' D5076
            ' D7306 ===
            ' Reset last opee model for avoid to reopen in on next logon
            If App.ActiveUserWorkgroup IsNot Nothing AndAlso App.Database.Connect Then
                Dim tParams As New List(Of Object)
                tParams.Add(-1)
                tParams.Add(App.ActiveUserWorkgroup.ID)
                App.Database.ExecuteSQL(String.Format("UPDATE {0} SET {1}=? WHERE {2}=?", clsComparionCore._TABLE_USERWORKGROUPS, clsComparionCore._FLD_USERWRKG_LASTPROJECTID, clsComparionCore._FLD_USERWRKG_ID), tParams)
            End If
            ' D7306 ==
            App.ProjectID = -1
            Res.ObjectID = ID
            Res.Result = ecActionResult.arSuccess
        Else
            Res.Result = ecActionResult.arWarning
            Res.Message = ResString("msgProjectNotFound")
        End If
        Return Res
    End Function

    Public Function Close_Active() As jActionResult
        Return Close(App.ProjectID)
    End Function

    ' D5076 ===
    Public Function Set_Lock(Status As ECLockStatus, Optional ID As Integer = -1) As jActionResult
        FetchIfNotAuthorized()
        Dim Res As New jActionResult With {
            .Result = ecActionResult.arError,
            .Message = "Project Not found"
        }
        If ID = -1 Then ID = App.ProjectID
        Dim tPrj As clsProject = FetchIfCantEditProject(ID)
        If tPrj IsNot Nothing Then
            If tPrj.LockInfo Is Nothing Then tPrj.LockInfo = App.DBProjectLockInfoRead(ID)
            If tPrj.LockInfo IsNot Nothing AndAlso tPrj.LockInfo.isLockAvailable(App.ActiveUser) Then
                Select Case tPrj.LockInfo.LockStatus
                    Case ECLockStatus.lsLockForSystem
                        Res.Result = ecActionResult.arWarning
                        Res.Message = ResString("lblProjectLockedBySystem")
                    Case Else
                        ' D5080 ===
                        If tPrj.LockInfo.LockStatus <> Status Then
                            ' D6709 ===
                            With tPrj.LockInfo
                                If App.isAuthorized Then .LockerUserID = App.ActiveUser.UserID
                                .ProjectID = tPrj.ID
                                .LockStatus = Status
                            End With
                            ' D6709 ==
                            App.DBProjectLockInfoWrite(Status, tPrj.LockInfo, App.ActiveUser, If(Status = ECLockStatus.lsLockForModify, Date.Now.AddSeconds(SessionTimeout), Date.Now))
                        End If
                        Res.Message = ""
                        Res.Result = ecActionResult.arSuccess
                        Res.Tag = tPrj.LockInfo.LockStatus
                        ' D5080 ==
                        jProject.GetActionResultByProject(Me, Res, tPrj, True)  ' D6053
                End Select
                ' D5080 ===
            Else
                Res.Message = ResString("errCantChangeLock")
                Res.Result = ecActionResult.arError
                ' D5080 ==
            End If
        End If
        Return Res
    End Function
    ' D5076 ==

    Public Function Create(Name As String, Optional Status As ecProjectStatus = ecProjectStatus.psActive, Optional Src_ID As Integer = -1, Optional sDescription As String = "", Optional RiskionModelType As ProjectType = ProjectType.ptRegular, Optional UseWkgWording As Boolean = False, Optional TimeFrame As String = "") As jActionResult   ' D6301 + D6307 + D6317 + D7235
        FetchIfNotAuthorized()
        Dim sError As String = ""
        Dim Res As New jActionResult    ' D6289
        If App.CanUserCreateNewProject(sError) Then ' D4938
            ' D7196 ===
            If Src_ID <= 0 Then
                Dim FirstPrjID As Integer = -1
                For Each tmpPrj As clsProject In App.ActiveProjectsList
                    If tmpPrj.ProjectStatus = ecProjectStatus.psMasterProject AndAlso Not tmpPrj.isMarkedAsDeleted Then
                        If FirstPrjID < 0 Then FirstPrjID = tmpPrj.ID
                        If tmpPrj.ProjectName.ToLower.Contains(If(App.isRiskEnabled, _DEF_MASTER_RISKION, _DEF_MASTER_COMPARION)) Then
                            Src_ID = tmpPrj.ID
                            Exit For
                        End If
                    End If
                Next
                If Src_ID <= 0 AndAlso FirstPrjID > 0 Then Src_ID = FirstPrjID
            End If
            ' D7196 ==
            Dim tPrj As clsProject = App.ProjectCreate(Name, Status, RiskionModelType, Src_ID, Res.Message)   ' D4770 + D6795
            If tPrj IsNot Nothing Then
                tPrj.ProjectManager.Parameters.SpecialMode = _OPT_MODE_ALEXA_PROJECT    ' D7584
                ' D6301 + D6307 ===
                Dim fUpdatePM As Boolean = False
                ' D6795 ===
                Dim fUpdatePrj As Boolean = True    ' D7584 due to set a special mode for alexa project
                'Dim fUpdate As Boolean = False
                If App.isRiskEnabled Then
                    If tPrj.PipeParameters.ProjectType <> RiskionModelType Then
                        tPrj.PipeParameters.ProjectType = RiskionModelType
                        fUpdatePM = True
                    End If
                    If tPrj.RiskionProjectType <> RiskionModelType Then
                        tPrj.RiskionProjectType = RiskionModelType
                        fUpdatePrj = True
                    End If
                    'App.DBProjectSyncProjectType(tPrj)
                    ' D6795 ==
                End If
                ' D6301 ==
                If UseWkgWording Then
                    If App.ProjectWordingUpdateWithWorkgroupWording(tPrj, False) Then
                        tPrj.SaveStructure("Init empty project data", , False)
                        'fUpdatePM = True   ' -D6795
                    End If
                End If
                ' D6317 ===
                If Not String.IsNullOrEmpty(sDescription) Then
                    tPrj.Comment = sDescription
                    tPrj.ProjectManager.ProjectDescription = SafeFormString(tPrj.Comment).Replace(vbLf, "<br>")
                    'App.DBProjectUpdate(tPrj, False)
                    fUpdatePrj = True   ' D6795
                    fUpdatePM = True
                End If
                ' D6317 ==
                'A1568 ===
                If tPrj.ProjectManager.Parameters.Hierarchy_WasShownToPM = "" Then
                    tPrj.ProjectManager.Parameters.Hierarchy_WasShownToPM = Boolean.FalseString
                    fUpdatePM = True    ' D6795
                    'tPrj.ProjectManager.Parameters.Save()
                End If
                'A1568 ==
                ' D7235 ===
                If App.isRiskEnabled AndAlso Not String.IsNullOrEmpty(TimeFrame) Then
                    tPrj.ProjectManager.Parameters.TimeFrame = TimeFrame
                    fUpdatePM = True
                End If
                ' D7235 ==
                If fUpdatePM Then tPrj.SaveProjectOptions(, False, False)
                If fUpdatePrj Then App.DBProjectUpdate(tPrj, False) ' D6795
                Res.Result = ecActionResult.arSuccess
                Dim isBOGGSAT As Boolean = tPrj.ProjectName.ToLower.Contains("boggsat")     ' D6348
                If Src_ID > 0 Then
                    Dim SrcPrj As clsProject = clsProject.ProjectByID(Src_ID, App.ActiveProjectsList)
                    If SrcPrj IsNot Nothing AndAlso SrcPrj.ProjectName.ToLower.Contains("boggsat") Then isBOGGSAT = True
                End If
                jProject.GetActionResultByProject(Me, Res, tPrj, True, If(isBOGGSAT, ecProjectStateOnOpen.psBOGGSAT, ecProjectStateOnOpen.psNew))    ' D4737 + D6060 + D6348
                ' D7263 ===
                If (Res.Data Is Nothing OrElse CStr(Res.Data) = "") AndAlso tPrj.ProjectStatus = ecProjectStatus.psActive AndAlso Str2Bool(GetParam(_Page.Params, "use_nodesets", True)) Then
                    If Res.URL = PageURL(If(App.isRiskEnabled, _DEF_PGID_ONNEWPROJECT_RISK, _DEF_PGID_ONNEWPROJECT)) Then
                        ' D7290 ===
                        'Res.URL = PageURL(_PGID_STRUCTURE_OBJECTIVES_HIERARCHY, "dlg=nodesets")
                        If Session(_SESS_OPEN_DLG) Is Nothing Then
                            Session.Add(_SESS_OPEN_DLG, "nodesets")  ' D7290
                            Res.URL = PageURL(If(App.isRiskEnabled, _PGID_STRUCTURE_OBJECTIVES_HIERARCHY, _PGID_STRUCTURE_HIERARCHY))    ' D7532
                        End If
                        ' D7290 ==
                    End If
                End If
                ' D6307 + D7263 ==
            Else
                Res.Result = ecActionResult.arError
                If Res.Message = ResString("msgCantReadProjectMasterPrj") Then Res.Tag = "wrongmaster"  ' D6994
            End If
            ' D4735 ==
        Else
            FetchNoPermissions(True, sError)
        End If
        Return Res
    End Function

    Public Function Copy(Src_ID As Integer, Name As String, Optional [Type] As ProjectType = ProjectType.ptRegular, Optional DBVer As Integer = -1, Optional hide_objname As Boolean = False, Optional hide_altname As Boolean = False, Optional save_users As Boolean = True, Optional hide_useremail As Boolean = False, Optional hide_username As Boolean = False, Optional save_snapshots As Boolean = False) As jActionResult
        FetchIfNotAuthorized()
        Dim sError As String = ""
        Dim tRes = New jActionResult
        If App.CanUserCreateNewProject(sError) Then ' D4938
            Dim tPrj As clsProject = clsProject.ProjectByID(Src_ID, App.ActiveProjectsList)
            If tPrj IsNot Nothing Then
                If tPrj.isMarkedAsDeleted Then
                    tRes.Message = ResString("msgCanOpenDeletedPrj")
                    tRes.Result = ecActionResult.arWarning
                Else
                    If tPrj.isValidDBVersion OrElse tPrj.isDBVersionCanBeUpdated Then
                        Dim sObjName As String = If(hide_objname, ParseString(If(App.isRiskEnabled, "%%Source%%", "%%Objective%%")), "")
                        Dim sObjImpactName As String = If(String.IsNullOrEmpty(sObjName), "", ParseString("%%Objective%%"))
                        Dim sAltName As String = If(hide_altname, ParseString(If(App.isRiskEnabled, "%%Event%%", "%%Alternative%%")), "")
                        Dim tCopyPrj As clsProject = App.ProjectSaveAs(Name.Trim, Src_ID, ecProjectStatus.psActive, tRes.Message,
                                                                               save_users, sObjName, sAltName, sObjImpactName,
                                                                               hide_useremail, hide_username, DBVer,
                                                                               [Type], save_snapshots)  ' D4770
                        If tCopyPrj IsNot Nothing Then
                            tRes.Result = ecActionResult.arSuccess
                            'If tCopyPrj.isValidDBVersion Then
                            '    jProject.GetActionResultByProject(Me, tRes, tCopyPrj, True, ecProjectStateOnOpen.psRegular)    ' D4739 + D6053 + D6060 + D6348
                            'End If
                            ' D7175 ===
                            jProject.GetActionResultByProject(Me, tRes, tCopyPrj, True, ecProjectStateOnOpen.psRegular, tCopyPrj.isValidDBVersion)    ' D4739 + D6053 + D6060 + D6348
                            If Not tCopyPrj.isValidDBVersion Then
                                tRes.URL = ""
                                tRes.ObjectID = -1
                                If tRes.Tag Is Nothing Then tRes.Tag = jProject.CreateFromBaseObject(App, tCopyPrj)
                            End If
                            ' D7175 ==
                        Else
                            tRes.Result = ecActionResult.arError
                        End If
                    Else
                        tRes.Result = ecActionResult.arWarning
                        tRes.Message = ResString("msgCantReadProjectDBVersion")
                    End If
                End If
            Else
                FetchProjectNotFound()
            End If
        Else
            FetchNoPermissions(True, ParseString(sError))
        End If
        Return tRes
    End Function

    Public Function Upgrade(Optional pgid As Integer = -1) As jActionResult
        FetchIfNoActiveProject()
        Dim tRes As New jActionResult
        If Not App.ActiveProject.isValidDBVersion Then  ' D5038
            Dim fUpdate As Boolean = App.DBProjectUpdateToLastVersion(App.ActiveProject)   ' D0497
            If fUpdate Then ' D1429
                jProject.GetActionResultByProject(Me, tRes, App.ActiveProject, True)    ' D6053
                tRes.Result = ecActionResult.arSuccess
                If pgid > 0 Then tRes.URL = PageURL(pgid, GetTempThemeURI(False))    ' D5040
            Else
                App.ActiveProject.IgnoreDBVersion = True  ' D1429 (?)
                tRes.Message = String.Format("{0}. {1}", ResString("msgCantUpgradeProject"), String.Format(ResString("msgWrongProjectDBVersion"), App.ActiveProject.DBVersion.GetVersionString, GetCurrentDBVersion.GetVersionString))    ' D1429
                tRes.Result = ecActionResult.arError
            End If
        Else
            tRes.Message = String.Format("No need to project upgrade since it's in the latest version")    ' D5038
            tRes.Result = ecActionResult.arWarning
        End If
        Return tRes
    End Function

    ' D5033 ===

    Private Function IterateProjectsList(IDs As String, func As funcProjectAction, Data As Object, CheckMarkedAsDeleted As Boolean) As jActionResult
        Dim tSuccess As New List(Of Integer)
        Dim tError As New List(Of KeyValuePair(Of String, String))
        Dim ID_List As String() = IDs.Trim().Split(CChar(","))
        For Each sID As String In ID_List
            Dim ID As Integer = -1
            Dim sError As String = ""
            If Integer.TryParse(sID, ID) Then
                Dim tPrj As clsProject = clsProject.ProjectByID(ID, App.ActiveProjectsList)
                If tPrj IsNot Nothing Then
                    If Not CheckMarkedAsDeleted OrElse Not tPrj.isMarkedAsDeleted Then
                        func(tPrj, Data, sError)
                    Else
                        sError = "Project marked as deleted"
                    End If
                    If ID = App.ProjectID Then App.ActiveProject.ResetProject() ' D6290
                Else
                    sError = "Project not found"
                End If
            Else
                sError = "Wrong ProjectID"
            End If
            If String.IsNullOrEmpty(sError) Then
                tSuccess.Add(ID)    ' D5039
            Else
                tError.Add(New KeyValuePair(Of String, String)(sID, sError))   ' D5039
            End If
        Next
        If tError.Count = 0 Then App.ActiveProjectsList = Nothing   ' D6289
        Return New jActionResult With {
            .Data = tSuccess,
            .Tag = tError,
            .ObjectID = App.ProjectID,
            .Message = If(tError.Count > 0, tError.First.Value, ""),    ' D6341
            .Result = If(tError.Count = 0, ecActionResult.arSuccess, ecActionResult.arWarning)
        }
    End Function

    Private Sub project_setMarkAsDeleted(tProject As clsProject, tData As Object, ByRef sError As String)
        If tProject IsNot Nothing Then
            Dim isMarked As Boolean = CBool(tData)
            If tProject.isMarkedAsDeleted <> isMarked Then
                If App.CanActiveUserModifyProject(tProject.ID) Then    ' D4965
                    tProject.isMarkedAsDeleted = isMarked
                    If App.DBProjectUpdate(tProject, False, String.Format("Mark project as {0}deleted", If(isMarked, "", "un"))) Then
                        ' D4679 ==
                        If isMarked AndAlso tProject.ID = App.ProjectID Then App.ProjectID = -1
                    Else
                        sError = "Unable to update data"
                    End If
                Else
                    sError = "No permissions"
                End If
            Else
                sError = "Nothing to change"
            End If
        End If
    End Sub

    Public Function Delete(IDs As String) As jActionResult
        Return IterateProjectsList(IDs, AddressOf project_setMarkAsDeleted, True, False)
    End Function

    Public Function UnDelete(IDs As String) As jActionResult
        Return IterateProjectsList(IDs, AddressOf project_setMarkAsDeleted, False, False)
    End Function

    Private Sub project_wipeout(tProject As clsProject, tData As Object, ByRef sError As String)
        If tProject IsNot Nothing Then
            If App.CanActiveUserModifyProject(tProject.ID) Then    ' D4965
                If App.DBProjectDelete(tProject, True) Then
                    If tProject.ID = App.ProjectID Then App.ProjectID = -1
                    App.ActiveProjectsList.Remove(tProject)
                Else
                    sError = "Unable to update data"
                End If
            Else
                sError = "No permissions"
            End If
        End If
    End Sub

    Public Function Wipeout(IDs As String) As jActionResult
        Return IterateProjectsList(IDs, AddressOf project_wipeout, Nothing, False)
    End Function

    '' D6289 ===
    'Private Function Check_Wipeout() As jActionResult
    '    _Page.FetchIfNotAuthorized()
    '    Dim sError As String = ""
    '    Dim Res As New jActionResult
    '    If App.CanUserModifySomeProject(App.ActiveUser.UserID, App.ActiveProjectsList, App.ActiveUserWorkgroup, App.Workspaces) Then    ' D6290
    '        Dim Lst As New List(Of Integer)
    '        Dim LastDT As DateTime = Now.AddDays(-App.WipeoutProjectsTimeout(App.ActiveUser.UserID))
    '        Dim sToday As String = Date.Now().AddDays(1).Date.Ticks.ToString
    '        Dim sLastShow As String = GetCookie(_COOKIE_WIPEOUT + App.ActiveWorkgroup.ID.ToString, "")
    '        If String.IsNullOrEmpty(sLastShow) OrElse sLastShow < sToday Then
    '            For Each tPrj As clsProject In App.ActiveProjectsList
    '                If tPrj.isMarkedAsDeleted AndAlso tPrj.LastModify.HasValue AndAlso tPrj.LastModify.Value.Date <= LastDT.Date AndAlso
    '                App.CanUserModifyProject(App.ActiveUser.UserID, tPrj.ID, App.ActiveUserWorkgroup, clsWorkspace.WorkspaceByUserIDAndProjectID(App.ActiveUser.UserID, tPrj.ID, App.Workspaces), App.ActiveWorkgroup) Then
    '                    Lst.Add(tPrj.ID)
    '                End If
    '            Next
    '            If Lst.Count > 0 Then
    '                Res.Message = String.Format(ResString("msgProjectsWipeout"), Lst.Count, App.WipeoutProjectsTimeout(App.ActiveUser.UserID))
    '            End If
    '            SetCookie(_COOKIE_WIPEOUT + App.ActiveWorkgroup.ID.ToString, sToday)
    '        End If
    '        Res.Data = Lst
    '        Res.Result = ecActionResult.arSuccess
    '    Else
    '        _Page.FetchNoPermissions(True, sError)
    '    End If
    '    Return Res
    'End Function
    '' D6289 ==

    ' D6326 ===
    Public Function Restore_Defaults() As jActionResult
        Dim fUpdated As Boolean = False
        If App.CanUserCreateNewProject() Then
            Dim SessName As String = String.Format("CheckMaster_{0}", App.ActiveWorkgroup.ID)
            If App.CheckWorkgroupMasterProjects(App.ActiveWorkgroup, False, True) Then fUpdated = True
            Session(SessName) = True
        End If
        Return New jActionResult With {
            .Result = ecActionResult.arSuccess,
            .Data = fUpdated,
            .ObjectID = App.ActiveWorkgroup.ID
        }
    End Function
    ' D6326 ==

    Private Sub project_setArchived(tProject As clsProject, tData As Object, ByRef sError As String)
        If tProject IsNot Nothing Then
            Dim tStatus As ecProjectStatus = If(CBool(tData), ecProjectStatus.psArchived, ecProjectStatus.psActive)
            If tProject.ProjectStatus <> tStatus Then '
                If App.CanActiveUserModifyProject(tProject.ID) Then    ' D4965
                    tProject.ProjectStatus = tStatus
                    If Not App.DBProjectUpdate(tProject, False, If(tStatus = ecProjectStatus.psArchived, "Archive", "Activate")) Then
                        sError = "Unable to update data"
                    End If
                Else
                    sError = "No permissions"
                End If
            Else
                sError = "Nothing to change"
            End If
        End If
    End Sub

    Public Function Set_Archived(IDs As String, Archived As Boolean) As jActionResult
        Return IterateProjectsList(IDs, AddressOf project_setArchived, Archived, True)
    End Function

    Private Sub project_setOnline(tProject As clsProject, tData As Object, ByRef sError As String)
        If tProject IsNot Nothing Then
            Dim isOnline As Boolean = CBool(tData)
            If tProject.isOnline <> isOnline OrElse (isOnline AndAlso Not tProject.isPublic) Then
                If App.CanChangeProjectOnlineStatus(tProject, sError) Then
                    tProject.isOnline = isOnline
                    If isOnline Then tProject.isPublic = True
                    If Not App.DBProjectUpdate(tProject, False, If(isOnline, "On-line", "Off-line") + " project") Then
                        sError = "Unable to update data"
                    End If
                End If
                If sError <> "" Then sError = ParseString(sError)    ' D5065
            Else
                sError = "Nothing to change"
            End If
        End If
    End Sub

    Public Function Set_Online(ID As Integer, Online As Boolean) As jActionResult
        Return IterateProjectsList(ID.ToString, AddressOf project_setOnline, Online, True)
    End Function

    ''TODO: redo list of projects for replace on upload and get rid off that func
    'Private Function getDT(tDate As Date?) As String
    '    If tDate IsNot Nothing AndAlso tDate.HasValue Then Return JS_SafeString(String.Format("{0} {1}", tDate.Value.ToShortDateString, tDate.Value.ToShortTimeString))
    '    Return ""
    'End Function

    Public Function Upload(File As HttpPostedFile, Status As ecProjectStatus) As jActionResult
        Dim tRes As New jActionResult
        If File IsNot Nothing Then
            If App.CanUserCreateNewProject(tRes.Message) Then ' D4938
                Dim sTmpName As String = File_CreateTempName()
                Try
                    File.SaveAs(sTmpName)
                Catch ex As Exception
                    tRes.Result = ecActionResult.arError
                    tRes.Message = "Unable to save uploaded file"
                End Try

                If MyComputer.FileSystem.FileExists(sTmpName) Then

                    Dim sPasscode As String = App.ProjectUniquePasscode("", -1)
                    Dim sOrigName As String = File.FileName
                    ' Check is it archive and extract it
                    ' D4757 ===
                    If isSupportedArchive(sOrigName) Then
                        Dim ExtList As New ArrayList
                        ExtList.Add(_FILE_EXT_AHP.ToLower)
                        ExtList.Add(_FILE_EXT_AHPX.ToLower)
                        ExtList.Add(_FILE_EXT_AHPS.ToLower) ' D0378
                        ExtList.Add(_FILE_EXT_TXT.ToLower)  ' D2132

                        Dim sExtractedFile As String = ExtractArchiveForFile(App, sOrigName, sTmpName, ExtList, sPasscode, tRes.Message, sOrigName)
                        If tRes.Message = "" AndAlso sExtractedFile <> "" Then
                            sTmpName = sExtractedFile
                        Else
                            tRes.Result = ecActionResult.arError
                            If tRes.Message = "" Then tRes.Message = ResString("errExtractFiles")
                        End If
                        ExtList = Nothing
                    End If

                    If tRes.Result <> ecActionResult.arError Then
                        Dim isRiskion As Boolean = App.isRiskEnabled ' D6696
                        Dim tProject As clsProject = App.ProjectCreateFromFile(sTmpName, sOrigName, sPasscode, Status, False, WebOptions.AllowBlankPsw, ReviewAccount, tRes.Message, isRiskion, GetNameByFilename(sOrigName))   ' D4976 + D4986 + D6696 + D7218
                        App.ActiveProjectsList = Nothing
                        If tProject Is Nothing Then
                            tRes.Result = ecActionResult.arError
                        Else

                            App.ProjectCheckPropertiesFromPM(tProject)  ' D6588

                            tRes.Result = ecActionResult.arSuccess
                            jProject.GetActionResultByProject(Me, tRes, tProject, True) ' D6053

                            ' D6696 ===
                            If isRiskion <> App.isRiskEnabled Then
                                If App.isRiskEnabled Then
                                    tRes.Data = "hid"
                                    tRes.Message = ResString("msgUploadedComparionModel2Riskion")
                                Else
                                    'tRes.Message = ResString("msgUploadedRiskModel2Comparion") ' -D6804 // per EF request
                                End If
                            End If
                            ' D6696 ==

                            If tProject.ProjectGUID <> Guid.Empty AndAlso String.IsNullOrEmpty(tRes.Message) Then   ' D6625
                                Dim tExistedLst As List(Of clsProject) = clsProject.ProjectsByGUID(tProject.ProjectGUID, App.ActiveProjectsList, False)
                                If tExistedLst IsNot Nothing AndAlso tExistedLst.Count > 1 Then
                                    Dim tIDs As New List(Of Integer)    ' D5039
                                    Dim tWGList As New List(Of clsWorkgroup)
                                    tWGList.Add(App.ActiveWorkgroup)
                                    tExistedLst.Sort(New clsProjectComparer(ecProjectSort.srtProjectDateTime, SortDirection.Descending, tWGList))
                                    For Each tPrj As clsProject In tExistedLst
                                        If tPrj.ID <> tProject.ID AndAlso tPrj.ProjectStatus = ecProjectStatus.psActive AndAlso Not tPrj.isMarkedAsDeleted Then
                                            tIDs.Add(tPrj.ID)   ' D5039
                                        End If
                                    Next
                                    ' D5039 ===
                                    If tIDs.Count > 0 Then
                                        tRes.Data = {"replace", tProject.ProjectName}
                                        tRes.Tag = tIDs
                                    End If
                                    ' D5039 ==
                                End If
                            End If
                            ' D0893 ==

                        End If
                        If tRes.Data Is Nothing AndAlso tProject IsNot Nothing AndAlso (Not tProject.isValidDBVersion AndAlso tProject.isDBVersionCanBeUpdated) Then tRes.Data = "upgrade" ' D6236 + D6298
                        If tRes.Message <> "" Then tRes.Message = ParseAllTemplates(tRes.Message, App.ActiveUser, tProject)
                    End If
                    File_Erase(sTmpName)
                    App.ActiveProjectsList = Nothing
                    ' D4748 ==
                End If
            Else
                FetchNoPermissions(True, tRes.Message)
            End If
        Else
            FetchNotFound(True, "Can't find/read uploaded file")
        End If
        Return tRes
    End Function

    ' D6441 ===
    Public Function Update(ID As Integer, Params As NameValueCollection) As jActionResult
        Dim tProject As clsProject = FetchIfCantEditProject(ID)
        Dim fSave As Boolean = False

        Dim sName As String = GetParam(Params, "name", True)
        If Not String.IsNullOrEmpty(sName) AndAlso tProject.ProjectName <> sName Then
            tProject.ProjectName = sName
            fSave = True
        End If
        Dim sComment As String = GetParam(Params, "description", True)
        If Not String.IsNullOrEmpty(sComment) AndAlso tProject.Comment <> sComment Then
            tProject.Comment = sComment
            fSave = True
        End If
        Dim sPasscode As String = GetParam(Params, "accesscode", True)
        If Not String.IsNullOrEmpty(sPasscode) AndAlso tProject.Passcode <> sPasscode Then
            sPasscode = App.ProjectUniquePasscode(sPasscode, tProject.ID)
            tProject.Passcode = sPasscode
            fSave = True
        End If

        Dim sPasscodeImpact As String = GetParam(Params, "accesscodeimpact", True)
        If Not String.IsNullOrEmpty(sPasscodeImpact) AndAlso tProject.PasscodeImpact <> sPasscodeImpact Then
            sPasscodeImpact = App.ProjectUniquePasscode(sPasscodeImpact, tProject.ID)
            tProject.PasscodeImpact = sPasscodeImpact
            fSave = True
        End If

        Dim sOnline As String = GetParam(Params, "isonline", True)
        If Not String.IsNullOrEmpty(sOnline) AndAlso tProject.isOnline <> Str2Bool(sOnline) Then
            tProject.isOnline = Str2Bool(sOnline)
            fSave = True
        End If

        If fSave Then
            App.DBProjectUpdate(tProject, False, "Update project data")
        End If

        Return New jActionResult With {
            .Result = ecActionResult.arSuccess,
            .ObjectID = tProject.ID,
            .Data = jProject.GetProjectByID(App, tProject.ID),
            .Message = If(fSave, "", ResString("msgNoChanges"))
        }
    End Function
    ' D6441 ==

    ' D6247 ===
    Public Function Project_HID_Move(prj_id As Integer, dest As Integer) As jActionResult
        Dim tProject As clsProject = FetchIfCantEditProject(prj_id)
        Dim sMessage As String = ""
        With tProject.ProjectManager
            Select Case dest
                Case CInt(ECHierarchyID.hidLikelihood)
                    Dim Impact As clsHierarchy = .Hierarchy(ECHierarchyID.hidImpact)
                    If Impact IsNot Nothing Then
                        Dim hID As Integer = Impact.HierarchyID
                        Dim hGUID As Guid = Impact.HierarchyGuidID
                        Dim hName As String = Impact.HierarchyName
                        Dim Likelihood As clsHierarchy = .Hierarchy(CInt(ECHierarchyID.hidLikelihood))
                        If Likelihood IsNot Nothing Then
                            Impact.HierarchyID = Likelihood.HierarchyID
                            Impact.HierarchyGuidID = Likelihood.HierarchyGuidID
                            Impact.HierarchyName = Likelihood.HierarchyName
                            Likelihood.HierarchyID = hID
                            Likelihood.HierarchyGuidID = hGUID
                            Likelihood.HierarchyName = hName
                            sMessage = "Use uploaded Comparion project as Likelihood"
                        End If
                    End If
                Case -1
                    If .CopyHierarchy(CInt(ECHierarchyID.hidImpact), CInt(ECHierarchyID.hidLikelihood)) IsNot Nothing Then
                        sMessage = "Use uploaded Comparion project for both hierarchies"
                    End If
            End Select
        End With
        If sMessage <> "" Then tProject.SaveStructure(sMessage, True)
        Dim tRes = New jActionResult With {.Result = ecActionResult.arSuccess}
        jProject.GetActionResultByProject(Me, tRes, tProject, True) ' D6348
        Return tRes
    End Function
    ' D6247 ==

    Public Function Replace(Src_ID As Integer, Dest_ID As Integer) As jActionResult
        Dim tRes = New jActionResult
        If App.ProjectReplace(Src_ID, Dest_ID, tRes.Message) Then tRes.Result = ecActionResult.arSuccess Else tRes.Result = ecActionResult.arError
        Return tRes
    End Function

    Public Function TeamTime_Stop(ID As Integer) As jActionResult
        Dim tPrj As clsProject = FetchIfCantEditProject(ID)
        Dim tRes = New jActionResult
        If tPrj.isTeamTimeLikelihood OrElse tPrj.isTeamTimeImpact Then  ' D7213
            App.TeamTimeEndSession(tPrj, False)
            tRes.Result = ecActionResult.arSuccess
            tRes.ObjectID = ID
            tRes.Data = jProject.GetProjectByID(App, ID).ToJSON   ' D5040
        Else
            tRes.Message = "No TeamTime session"
            tRes.Result = ecActionResult.arWarning
        End If
        Return tRes
    End Function

    Public Function List_Starred() As jActionResult
        FetchIfNotAuthorized()
        Dim Res As New jActionResult
        Dim tList As New List(Of jProjectShort)
        Dim tStarredWS As List(Of clsWorkspace) = clsWorkspace.WorkspacesStarred(App.Workspaces)    ' D5014
        tStarredWS.Sort(New clsWorkspaceByDateDescComparer) ' D5014
        For Each tWS As clsWorkspace In tStarredWS
            Dim tPrj As clsProject = clsProject.ProjectByID(tWS.ProjectID, App.ActiveProjectsList)
            If tPrj IsNot Nothing AndAlso Not tPrj.isMarkedAsDeleted Then
                Dim tData As jProject = jProject.CreateFromBaseObject(App, tPrj)
                If tData IsNot Nothing Then tList.Add(tData)
            End If
        Next
        Res.Result = ecActionResult.arSuccess
        Res.Data = tList
        Return Res
    End Function

    Public Function Starred_Toggle(ID As Integer) As jActionResult
        Dim tPrj As clsProject = FetchIfWrongProject(ID)
        Dim tRes = New jActionResult
        Dim tWS As clsWorkspace = clsWorkspace.WorkspaceByUserIDAndProjectID(App.ActiveUser.UserID, ID, App.Workspaces)
        If tWS Is Nothing AndAlso App.CanActiveUserModifyProject(ID) Then
            tWS = App.AttachProject(App.ActiveUser, tPrj, False, App.ActiveWorkgroup.GetDefaultRoleGroupID(ecRoleLevel.rlModelLevel, ecRoleGroupType.gtProjectManager), "", True)
        End If
        If tWS IsNot Nothing Then
            tWS.isStarred = Not tWS.isStarred
            If App.DBWorkspaceUpdate(tWS, tWS.ID <= 0, If(tWS.isStarred, "Starred", "Un-starred") + " Project") Then
                tRes.Result = ecActionResult.arSuccess
                tRes.Data = tWS.isStarred
                tRes.ObjectID = ID
                App.Workspaces = Nothing    ' D5010
            End If
        Else
            FetchIfCantEditProject()
        End If
        Return tRes
    End Function

    Public Function Starred_Clear() As jActionResult
        Dim tStarredWS As List(Of clsWorkspace) = clsWorkspace.WorkspacesStarred(App.Workspaces)
        Dim tIDs As New List(Of Integer)
        For Each tWS As clsWorkspace In tStarredWS
            If tWS IsNot Nothing Then
                tWS.isStarred = False
                If App.DBWorkspaceUpdate(tWS, False, "Un-starred Project") Then tIDs.Add(tWS.ProjectID)
            End If
        Next
        Return New jActionResult With {
            .Result = ecActionResult.arSuccess,
            .Data = False,
            .Tag = tIDs
        }   ' D5040
    End Function
    ' D5033 ==

    ' D6443 ===
    Public Function List_History() As jActionResult
        FetchIfNotAuthorized()
        Dim Res As New jActionResult
        Dim tList As New List(Of jProjectShort)

        Dim sSQL As String = String.Format("SELECT TOP ({0}) P.* FROM Workspace W LEFT JOIN Projects P ON P.ID=W.ProjectID WHERE W.UserID=? AND W.LastModify>?{1} ORDER BY W.LastModify DESC", _OPT_HISTORY_COUNT, If(_OPT_HISTORY_ALL_WKG, "", " AND P.WorkgroupID=?"))
        Dim tParams As New List(Of Object)
        tParams.Add(App.ActiveUser.UserID)
        tParams.Add(_OPT_HISTORY_AFTER)
        If Not _OPT_HISTORY_ALL_WKG Then tParams.Add(App.ActiveWorkgroup.ID)
        Dim tPrjList As List(Of Dictionary(Of String, Object)) = App.Database.SelectBySQL(sSQL, tParams)

        For Each tRow As Dictionary(Of String, Object) In tPrjList
            Dim tPrj As clsProject = App.DBParse_Project(tRow)
            If tPrj IsNot Nothing AndAlso Not tPrj.isMarkedAsDeleted Then
                Dim tData As jProject = jProject.CreateFromBaseObject(App, tPrj)
                If tData IsNot Nothing Then tList.Add(tData)
            End If
        Next

        Res.Result = ecActionResult.arSuccess
        Res.Tag = Now  ' D6881
        Res.Data = tList
        Return Res
    End Function
    ' D6443 ==

    ' D5039 ===
    Private Function Get_Lock(ID As Integer) As jActionResult
        FetchNotImplemented()
        'Dim tPrj As clsProject = _Page.FetchIfWrongProject(ID)
        'Dim Res As New jActionResult With {
        '    .Result = ecActionResult.arSuccess
        '}
        'Dim tLock As clsProjectLockInfo = App.DBProjectLockInfoRead(ID)
        'If tLock IsNot Nothing Then
        'End If
        Return Nothing
    End Function
    ' D5039 ==

    Public Function Landing_Info(IDs As List(Of String)) As jActionResult
        FetchIfNoActiveProject()
        Dim tRes As New jActionResult With {.Result = ecActionResult.arSuccess}
        If IDs IsNot Nothing Then
            Dim tList As New Dictionary(Of String, String)
            Dim PM As clsProjectManager = App.ActiveProject.ProjectManager
            For Each id As String In IDs
                Dim pgid As Integer
                Integer.TryParse(id, pgid)
                Select Case pgid Mod _PGID_MAX_MOD
                    Case _PGID_STRUCTURE_HIERARCHY
                        Dim nCount = PM.ActiveObjectives.Nodes.Count
                        tList.Add(id, nCount.ToString)
                        If nCount > 0 Then tList.Add("goal", PM.ActiveObjectives.Nodes(0).NodeName)
                    Case _PGID_STRUCTURE_ALTERNATIVES
                        tList.Add(id, PM.ActiveAlternatives.Nodes.Count.ToString)
                    Case _PGID_PROJECT_USERS
                        'tList.Add(id, PM.UsersList.Count.ToString)
                        tList.Add(id, App.DBWorkspacesByProjectID(App.ProjectID).Count.ToString)
                        ' D6054 ===
                    Case _PGID_SURVEY_EDIT_PRE, _PGID_SURVEY_EDIT_POST
                        Dim sType As SurveyType = If(App.ActiveProject.isImpact, If(pgid = _PGID_SURVEY_EDIT_POST, SurveyType.stImpactThankyouSurvey, SurveyType.stImpactWelcomeSurvey), If(pgid = _PGID_SURVEY_EDIT_POST, SurveyType.stThankyouSurvey, SurveyType.stWelcomeSurvey))
                        Dim ASurvey As clsSurveyInfo = App.SurveysManager.GetSurveyInfoByProjectID(App.ProjectID, sType, Nothing)
                        If ASurvey IsNot Nothing Then
                            Dim Cnt As Integer = 0
                            Dim tSurvey As clsSurvey = ASurvey.Survey("-")
                            If tSurvey IsNot Nothing Then
                                For Each tPage As clsSurveyPage In tSurvey.Pages
                                    Cnt += tPage.Questions.Count
                                Next
                            End If
                            tList.Add(id, Cnt.ToString)
                        End If
                        ' D6054 ==
                    Case _PGID_ANTIGUA_MEETING, _PGID_ANTIGUA_MEETING_LIKELIHOOD, _PGID_ANTIGUA_MEETING_IMPACT
                        If PM.HasCollaborativeStructuringData(App.ActiveProject.ID) Then tList.Add(id, "has_structuring_data")
                End Select
            Next
            tRes.Data = tList
        End If
        Return tRes
    End Function

    Public Function Eval_Progress() As jActionResult
        FetchIfNoActiveProject()
        Dim tRes As New jActionResult With {.Result = ecActionResult.arSuccess}
        Dim PM As clsProjectManager = App.ActiveProject.ProjectManager
        Dim result As Double = 0
        Dim totalCount As Integer = 0
        Dim madeCount As Integer = 0
        Dim usersList As List(Of clsApplicationUser) = App.DBUsersByProjectID(App.ProjectID)

        Dim uList As New List(Of clsUser)
        For Each appuser As clsApplicationUser In usersList
            Dim user As clsUser = PM.GetUserByEMail(appuser.UserEmail)
            If user IsNot Nothing Then uList.Add(user)
        Next

        Dim res As Dictionary(Of String, UserEvaluationProgressData) = PM.StorageManager.Reader.GetEvaluationProgress(uList, PM.ActiveHierarchy, madeCount, totalCount)

        If totalCount > 0 Then
            result = CInt(100 * madeCount / totalCount)
        End If
        tRes.Data = result
        Return tRes
    End Function

    ' D6387 ===
    Public Function Snapshot_Add(Comment As String) As jActionResult
        Dim tResult As New jActionResult
        Comment = If(Comment Is Nothing, "", SafeFormString(Comment.Trim))
        Dim sMessage As String = ResString("lblSnapshotOnDemand")
        If Comment <> "" AndAlso App.CanvasMasterDBVersion < "0.99992" Then sMessage = Comment
        Dim tSnapshot As clsSnapshot = App.SnapshotSaveProject(ecSnapShotType.Manual, sMessage, App.ProjectID, True, Comment)
        If tSnapshot IsNot Nothing Then
            tResult.Result = ecActionResult.arSuccess
            tResult.ObjectID = tSnapshot.Idx
            tResult.Data = tSnapshot.SnapshotID
        Else
            tResult.Result = ecActionResult.arError
            tResult.Message = ResString("errCantCreateSnapshot")
        End If
        Return tResult
    End Function
    ' D6387 ==

    Private Sub ProjectsWebAPI_Load(sender As Object, e As EventArgs) Handles Me.Load
        FetchIfNotAuthorized()
        If App.ActiveWorkgroup Is Nothing Then FetchNoPermissions(, "NO_WORKGROUP") ' D7203

        Select Case _Page.Action

            Case "list"
                _Page.ResponseData = List(Str2Bool(GetParam(_Page.Params, "reload", True))) ' D5033

            Case "list_short"
                _Page.ResponseData = List_Short(Str2Bool(GetParam(_Page.Params, "reload", True)))   ' D5033

            Case "projectbyid"
                Dim ID As Integer = -1
                Integer.TryParse(GetParam(_Page.Params, _PARAM_ID, True), ID)
                _Page.ResponseData = ProjectbyID(ID)

            Case "projectbyid_short"
                Dim ID As Integer = -1
                Integer.TryParse(GetParam(_Page.Params, _PARAM_ID, True), ID)
                _Page.ResponseData = ProjectByID_Short(ID)

            Case "activeproject"
                _Page.ResponseData = ActiveProject()

            Case "activeproject_short"
                _Page.ResponseData = ActiveProject_Short()

            Case "open"
                Dim ID As Integer = -1
                Integer.TryParse(GetParam(_Page.Params, _PARAM_ID, True), ID)
                _Page.ResponseData = Open(ID, GetParam(_Page.Params, "online", True).ToLower = "ignore", GetParam(_Page.Params, "passcode", True))    ' D5065 + D7154

            Case "close"
                Dim ID As Integer = -1
                Integer.TryParse(GetParam(_Page.Params, _PARAM_ID, True), ID)
                If ID <= 0 Then ID = App.ProjectID
                _Page.ResponseData = Close(ID)

            Case "close_active"
                _Page.ResponseData = Close_Active()

                ' D5033 ===
            Case "delete"
                _Page.ResponseData = Delete(GetParam(_Page.Params, "ids", True))

            Case "undelete"
                _Page.ResponseData = UnDelete(GetParam(_Page.Params, "ids", True))

            Case "wipeout"
                _Page.ResponseData = Wipeout(GetParam(_Page.Params, "ids", True))

            'Case "check_wipeout"    ' D6289
            '    _Page.ResponseData = Check_Wipeout()    ' D6289

            Case "restore_defaults" ' D6326
                _Page.ResponseData = Restore_Defaults()    ' D6326

            Case "set_archived"
                _Page.ResponseData = Set_Archived(GetParam(_Page.Params, "ids", True), Str2Bool(GetParam(_Page.Params, "archived", True)))

            Case "set_online"
                Dim ID As Integer = -1
                Integer.TryParse(GetParam(_Page.Params, _PARAM_ID, True), ID)
                _Page.ResponseData = Set_Online(ID, Str2Bool(GetParam(_Page.Params, "online", True)))   ' D5039
                ' D5033 ==

            Case "copy"
                Dim DBVer As Integer = GetCurrentDBVersion.MinorVersion
                Integer.TryParse(GetParam(_Page.Params, "dbver", True), DBVer)
                If DBVer < 16 OrElse DBVer > GetCurrentDBVersion.MinorVersion Then DBVer = -1
                Dim SrcID As Integer = -1
                Integer.TryParse(GetParam(_Page.Params, "src_id", True), SrcID)
                ' D6775 ===
                Dim sType As String = GetParam(_Page.Params, "type", True)
                Dim fType As ProjectType = Str2ProjectType(sType)
                If sType = "" AndAlso App.isRiskEnabled Then
                    Dim tSrcPrj As clsProject = clsProject.ProjectByID(SrcID, App.ActiveProjectsList)
                    If tSrcPrj IsNot Nothing Then
                        fType = tSrcPrj.RiskionProjectType
                    End If
                End If
                _Page.ResponseData = Copy(SrcID, GetParam(_Page.Params, "name", True), fType, DBVer,
                                          Str2Bool(GetParam(_Page.Params, "hide_objname", True)), Str2Bool(GetParam(_Page.Params, "hide_altname", True)),
                                          Str2Bool(GetParam(_Page.Params, "save_users", True)), Str2Bool(GetParam(_Page.Params, "hide_useremail", True)),
                                          Str2Bool(GetParam(_Page.Params, "hide_username", True)), Str2Bool(GetParam(_Page.Params, "save_snapshots", True)))  ' D4770
                ' D6775 ==

            Case "create"
                Dim Name As String = GetParam(_Page.Params, "name", True)
                Dim SrcID As Integer = -1
                Integer.TryParse(GetParam(_Page.Params, "src_id", True), SrcID)
                ' D6301 ===
                Dim MType As Integer = CInt(ProjectType.ptRegular)
                Integer.TryParse(GetParam(_Page.Params, "model_type", True), MType)
                If Not [Enum].IsDefined(GetType(ProjectType), MType) Then MType = CInt(ProjectType.ptRegular)
                _Page.ResponseData = Create(Name, Str2ProjectStatus(GetParam(_Page.Params, "status", True)), SrcID, GetParam(_Page.Params, "description", True), CType(MType, ProjectType), Str2Bool(GetParam(_Page.Params, "wkg_wording", True)), GetParam(_Page.Params, "timeframe", True))   ' D6307 + D7235
                ' D6301 ==

            Case "upgrade"
                ' D5040 ===
                Dim PgID As Integer = -1
                Integer.TryParse(GetParam(_Page.Params, "pgid", True), PgID)
                _Page.ResponseData = Upgrade(PgID)
                ' D5040 ==

                ' D5033 ===
            Case "upload"
                Dim File As HttpPostedFile = Nothing
                If Request IsNot Nothing AndAlso Request.Files.Count > 0 Then File = Request.Files(0)
                _Page.ResponseData = Upload(File, Str2ProjectStatus(GetParam(_Page.Params, "status", True)))

                ' D6441 ===
            Case "update"
                Dim PrjID As Integer = -1
                Integer.TryParse(GetParam(_Page.Params, _PARAM_ID, True), PrjID)
                _Page.ResponseData = Update(PrjID, _Page.Params)
                ' D6441 ==

            Case "replace"
                Dim SrcID As Integer = -1
                Integer.TryParse(GetParam(_Page.Params, "src_id", True), SrcID)
                Dim DestID As Integer = -1
                Integer.TryParse(GetParam(_Page.Params, "dest_id", True), DestID)
                _Page.ResponseData = Replace(SrcID, DestID)

                ' D6247 ===
            Case "project_hid_move"
                Dim PrjID As Integer = -1
                Integer.TryParse(GetParam(_Page.Params, "prj_id", True), PrjID)
                Dim DestID As Integer = -1
                Integer.TryParse(GetParam(_Page.Params, "dest_id", True), DestID)
                If DestID <> CInt(ECHierarchyID.hidLikelihood) AndAlso DestID <> -1 Then DestID = CInt(ECHierarchyID.hidImpact)
                _Page.ResponseData = Project_HID_Move(PrjID, DestID)
                ' D6247 ==

            Case "teamtime_stop"
                Dim ID As Integer = -1
                Integer.TryParse(GetParam(_Page.Params, "id", True), ID)
                _Page.ResponseData = TeamTime_Stop(ID)

            Case "list_starred"
                _Page.ResponseData = List_Starred()

            Case "starred_toggle"
                Dim ID As Integer = -1
                Integer.TryParse(GetParam(_Page.Params, "id", True), ID)
                _Page.ResponseData = Starred_Toggle(ID)

            Case "starred_clear"
                _Page.ResponseData = Starred_Clear()
                ' D5033 ==

            Case "list_history" ' D6443
                _Page.ResponseData = List_History() ' D6443

                ' D5039 ===
            Case "get_lock"
                Dim ID As Integer = -1
                Integer.TryParse(GetParam(_Page.Params, "id", True), ID)
                _Page.ResponseData = Get_Lock(ID)

            Case "set_lock"
                Dim ID As Integer = -1
                Integer.TryParse(GetParam(_Page.Params, "id", True), ID)
                ' D5080 ===
                If ID = -1 Then ID = App.ProjectID
                Dim Lock As Boolean = Str2Bool(GetParam(_Page.Params, "lock", False))
                _Page.ResponseData = Set_Lock(If(Lock, ECLockStatus.lsLockForModify, ECLockStatus.lsUnLocked), ID)
                ' D5039 + D5080 ==

            Case "landing_info"
                _Page.ResponseData = Landing_Info(GetParam(_Page.Params, "ids", True).Split(CChar(",")).ToList)

            Case "eval_progress"
                _Page.ResponseData = Eval_Progress()

            Case "snapshot_add"     ' D6387
                _Page.ResponseData = Snapshot_Add(GetParam(_Page.Params, "comment", True))  ' D6387

        End Select
    End Sub
    ' D5032 ==

    ' D5014 ===
    <Serializable()> Public Class clsWorkspaceByDateDescComparer
        Implements IComparer(Of clsWorkspace)

        Public Function Compare(ByVal A As clsWorkspace, ByVal B As clsWorkspace) As Integer Implements IComparer(Of clsWorkspace).Compare
            Dim OldDate As Date = DateAdd(DateInterval.Year, -10, Date.Now())
            Dim A_ As Date = If(A.Created.HasValue, A.Created.Value, OldDate)
            If A.LastModify.HasValue Then A_ = A.LastModify.Value
            Dim B_ As Date = If(B.Created.HasValue, B.Created.Value, OldDate)
            If B.LastModify.HasValue Then B_ = B.LastModify.Value
            Return -Date.Compare(A_, B_)
        End Function
    End Class
    ' D5014 ==

End Class