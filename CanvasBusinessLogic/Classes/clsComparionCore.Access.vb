Imports System.Linq
Imports System.Web.Script.Serialization
Imports ECSecurity.ECSecurity
Imports ExpertChoice.Service

Namespace ExpertChoice.Data

    ''' <summary>
    ''' The way how the user authentication will be performed.
    ''' <remark>You don't need to specify that parameter in the most cases, since the code tries to detect the credentials data/type automatically, especially in case of Hash using.</remark>
    ''' </summary>
    Public Enum ecAuthenticateWay   ' D0511
        ''' <summary>
        ''' Regular login - Used as the basic user authentication via web site/web form (email, password, passcode, rememberme can be used)
        ''' </summary>
        awRegular = 0
        ''' <summary>
        ''' Start a meeting. Deprecated for now.
        ''' </summary>
        awStartMeeting = 1
        ''' <summary>
        ''' Join the meeting - Can be used for join to the TeamTime or Collaborative Structuring (Brainstorming) meetings  (email and meetingid can be used)
        ''' </summary>
        awJoinMeeting = 2
        ''' <summary>
        ''' Using the hash/token - More complex and universal way to user authorize by unique, encrypted hash key (hash parameter can be used)
        ''' </summary>
        awTokenizedURL = 3  ' D0511
        ''' <summary>
        ''' Login by encoded credentials line. Deprecated for now.
        ''' </summary>
        awCredentials = 4   ' D0557
        awSSO = 5           ' D6532
    End Enum

    ' D7502 ===
    Public Enum ecPinCodeType
        mfaNone = 0
        mfaAlexa = 1
        mfaEmail = 2
    End Enum
    ' D7502 ==

    Partial Public Class clsComparionCore

        ' D0466 ===
        Public Sub Logout(Optional ByVal sLogMessage As String = "")    ' D0486
            If Not ActiveUser Is Nothing Then

                ' D0510 ===
                Dim sSQL As String = String.Format("SELECT * FROM Projects WHERE LockStatus=1 AND LockedByUserID={0}", ActiveUser.UserID)
                Dim tList As List(Of Dictionary(Of String, Object)) = Database.SelectBySQL(sSQL)
                For Each tRow As Dictionary(Of String, Object) In tList
                    Dim tProject As clsProject = DBParse_Project(tRow)
                    If tProject.LockInfo.isLockedByUser(ActiveUser.UserID) AndAlso tProject.LockInfo.LockStatus = ECLockStatus.lsLockForModify Then DBProjectLockInfoWrite(ECLockStatus.lsUnLocked, tProject.LockInfo, ActiveUser, Now) ' D0589
                Next
                ' D0510 ==

                DBSaveLog(dbActionType.actLogout, dbObjectType.einfUser, ActiveUser.UserID, "", "") ' D0469
                Try ' D0496
                    Database.ExecuteSQL(String.Format("UPDATE {0} SET isOnline=0 WHERE {1}={2}", _TABLE_USERS, _FLD_USERS_ID, ActiveUser.UserID)) ' D0494
                Catch ex As Exception   ' D0496
                End Try
                _EULA_checked = False   ' D0473
                _EULA_valid = True      ' D0473
            End If
            ' D0521 ===
            SetActiveWorkgroup(Nothing)
            SetActiveUser(Nothing)
            ActiveProject = Nothing
            ActiveProjectsList = Nothing
            ' D0521 ==
            ApplicationError.Reset()    ' D0531
            ' D0659 ===
            Options.SingleModeProjectPasscode = ""
            Options.isLoggedInWithMeetingID = False
            Options.OnlyTeamTimeEvaluation = False
            Options.ShowAppNavigation = True
            Options.RiskionRiskRewardMode = False   ' D6813
            'Database.Close()    ' D2238
            ' D0659 ==
            Options.UserRoleGroupID = -1        ' D1936
            Options.WorkgroupRoleGroupID = -1   ' D2287
            MFA_Requested = False   ' D7502
            HashCodesReset()    ' D4936 + D7114
            Antigua_ResetCredentials()  ' D4920
        End Sub
        ' D0466 ==

        ' D4622 ===
        Public Function GetLastUsedWorkgroupID(tUWList As List(Of clsUserWorkgroup), tWkgList As List(Of clsWorkgroup)) As Integer ' D4842
            Dim WkgID As Integer = -1
            Dim tLastDT As New Date?
            Dim SysWkgID As Integer = -1    ' D4623
            If SystemWorkgroup IsNot Nothing Then SysWkgID = SystemWorkgroup.ID ' D4623
            For Each UW As clsUserWorkgroup In tUWList
                If UW.WorkgroupID <> SysWkgID AndAlso UW.Status = ecUserWorkgroupStatus.uwEnabled AndAlso Not UW.isExpired Then ' D4623
                    ' D4842 ===
                    Dim _tLastDT As Date? = tLastDT
                    Dim _WkgID As Integer = -1
                    If (UW.Created.HasValue AndAlso (Not _tLastDT.HasValue OrElse UW.Created.Value > _tLastDT.Value)) Then
                        _tLastDT = UW.Created
                        _WkgID = UW.WorkgroupID
                    End If
                    If (UW.LastVisited.HasValue AndAlso (Not _tLastDT.HasValue OrElse UW.LastVisited.Value > _tLastDT.Value)) Then
                        _tLastDT = UW.LastVisited
                        _WkgID = UW.WorkgroupID
                    End If
                    If _WkgID > 0 AndAlso tWkgList IsNot Nothing Then
                        Dim tWkg As clsWorkgroup = clsWorkgroup.WorkgroupByID(_WkgID, tWkgList)
                        If tWkg IsNot Nothing AndAlso tWkg.License IsNot Nothing AndAlso tWkg.License.isValidLicense AndAlso tWkg.License.CheckParameterByID(ecLicenseParameter.ExpirationDate) Then
                            tLastDT = _tLastDT
                            WkgID = _WkgID
                        End If
                    End If
                    ' D4842 ==
                End If
            Next
            Return WkgID
        End Function
        ' D4622 ==

        ' D0469 ===
        Public Function Logon(ByVal sEmail As String, ByVal sPassword As String, ByRef sPasscode As String, ByVal fIgnoreProjectStatus As Boolean, fAllowBlankPsw As Boolean, AllowSpecialLogon As Boolean) As ecAuthenticateError  ' D0511 + D1672 + D1724 + D7205 + D7327
            DebugInfo("Start to user authorization...", _TRACE_INFO)
            sEmail = sEmail.Trim
            sPasscode = sPasscode.Trim
            ' D1401 ===
            While sPassword.Length > 0 AndAlso sPassword.StartsWith(" ")
                sPassword = sPassword.Substring(1)
            End While
            ' D1401 ==
            If sEmail = "" Then
                DebugInfo("E-mail is empty", _TRACE_WARNING)
                Return ecAuthenticateError.aeNoUserFound
            End If

            If sPassword = ResString("lblDummyBlankPassword") Or sPassword.ToLower = "[blank]" Then sPassword = "" ' D0820

            Dim tUser As clsApplicationUser = DBUserByEmail(sEmail)
            If tUser Is Nothing Then
                DebugInfo(String.Format("User with specified E-mail '{0}' not found", sEmail), _TRACE_WARNING)
                Return ecAuthenticateError.aeNoUserFound
            End If

            ' D6065 ===
            If _DEF_PASSWORD_ATTEMPTS_AUTOUNLOCK AndAlso tUser.PasswordStatus >= _DEF_PASSWORD_ATTEMPTS AndAlso tUser.LastVisited.HasValue AndAlso tUser.LastVisited.Value.AddMinutes(_DEF_PASSWORD_ATTEMPTS_LOCK_TIMEOUT) < Now Then   ' D6074
                ' D6087 ===
                ''DBUserUpdate(tUser, False, "Reset user lock status")
                'Dim tParams As New List(Of Object)
                'tUser.PasswordStatus = 0
                'tParams.Add(sEmail)
                'Database.ExecuteSQL(String.Format("UPDATE {0} SET PasswordStatus={2} WHERE {1}=?", clsComparionCore._TABLE_USERS, clsComparionCore._FLD_USERS_EMAIL, tUser.PasswordStatus), tParams)
                'Dim sComment As String = "Unlock user due to lock timeout"
                'DBSaveLog(dbActionType.actUnLock, dbObjectType.einfUser, tUser.UserID, sComment, GetClientIP())
                DBUserUnlock(tUser,, "Unlock user due to lock timeout")
                ' D6087 ==
            End If
            ' D6065 ==

            ' D2213 ===
            If tUser.PasswordStatus >= _DEF_PASSWORD_ATTEMPTS Then
                DebugInfo("User account is locked by wrong password attempts", _TRACE_WARNING)
                Return ecAuthenticateError.aeUserLockedByWrongPsw
            End If
            ' D2213 ==
            If tUser.UserPassword <> sPassword Then
                DebugInfo("Wrong user password", _TRACE_WARNING)
                Return ecAuthenticateError.aeWrongPassword
            End If
            If tUser.Status = ecUserStatus.usDisabled Then
                DebugInfo("User account is locked", _TRACE_WARNING)
                Return ecAuthenticateError.aeUserLocked
            End If

            ' D4606 ===
            Dim UWList As List(Of clsUserWorkgroup) = DBUserWorkgroupsByUserID(tUser.UserID)
            Dim WkgList As List(Of clsWorkgroup) = DBWorkgroupsAll(True, True)
            If Not CheckUserWorkgroups(tUser, WkgList, UWList) Then Return ecAuthenticateError.aeWrongLicense
            ' D4606 ==

            Dim WkgID As Integer = tUser.DefaultWorkgroupID
            Dim fCanResetWkg As Boolean = False ' D1175

            ' D3952 ===
            If SystemWorkgroup Is Nothing OrElse SystemWorkgroup.License Is Nothing OrElse Not SystemWorkgroup.License.isValidLicense Then
                If tUser Is Nothing OrElse Not tUser.CannotBeDeleted Then Return ecAuthenticateError.aeWrongLicense
                WkgID = -1
            End If

            Dim sError As String = ""
            If SystemWorkgroup IsNot Nothing AndAlso SystemWorkgroup.License IsNot Nothing AndAlso SystemWorkgroup.License.isValidLicense AndAlso Not CheckLicense(SystemWorkgroup, sError, True) Then     ' D4871
                If tUser IsNot Nothing AndAlso Not tUser.CannotBeDeleted Then
                    If Not SystemWorkgroup.License.CheckParameterByID(ecLicenseParameter.ExpirationDate) Then Return ecAuthenticateError.aeSystemWorkgroupExpired ' D4982
                    If Not SystemWorkgroup.License.CheckParameterByID(ecLicenseParameter.InstanceID) Then Return ecAuthenticateError.aeWrongInstanceID
                    If Not SystemWorkgroup.License.CheckParameterByID(ecLicenseParameter.MaxWorkgroupsTotal) Then Return ecAuthenticateError.aeTotalWorkgroupsLimit ' D6568
                    LicenseInitError(sError, True)  ' D4871
                    Return ecAuthenticateError.aeWrongLicense  ' D4871
                Else
                    WkgID = SystemWorkgroup.ID
                End If
            End If
            ' D3952 ==

            If WkgID > 0 Then
                Dim defUW As clsUserWorkgroup = clsUserWorkgroup.UserWorkgroupByUserIDAndWorkgroupID(tUser.UserID, WkgID, UWList)
                If defUW Is Nothing OrElse defUW.Status = ecUserWorkgroupStatus.uwDisabled Then ' D7160
                    DebugInfo("Default user workgroup not found. Continue with unpicked.", _TRACE_WARNING)
                    WkgID = -1
                Else
                    fCanResetWkg = UWList.Count > 1     ' D1175
                End If
            End If

            Dim tProject As clsProject = Nothing
            If sPasscode <> "" Then
                tProject = DBProjectByPasscode(sPasscode)
                If tProject Is Nothing Then
                    DebugInfo(String.Format("Project with passcode '{0}' not found", _TRACE_WARNING))
                    Return ecAuthenticateError.aeWrongPasscode
                Else
                    ' D0789 + D7247 ===
                    If tProject.isMarkedAsDeleted Then
                        If AllowSpecialLogon Then
                            tProject = Nothing
                        Else
                            DebugInfo("Project is marked as deleted")
                            Return ecAuthenticateError.aeDeletedProject
                        End If
                    End If
                    ' D0789 + D7247 ==
                End If
            End If

            Dim tWSList As List(Of clsWorkspace) = DBWorkspacesByUserID(tUser.UserID)   ' D2436
            If Not tProject Is Nothing Then
                ' D0487 ===
                WkgID = tProject.WorkgroupID
                DebugInfo("Use workgroup for specified project", _TRACE_INFO)
                Dim tPrjUW As clsUserWorkgroup = clsUserWorkgroup.UserWorkgroupByUserIDAndWorkgroupID(tUser.UserID, tProject.WorkgroupID, UWList)   ' D0499
                If tPrjUW Is Nothing Then
                    DebugInfo("User isn't attached to workgroup. Try to do it.", _TRACE_INFO)
                    tPrjUW = AttachWorkgroupByProject(tUser.UserID, tProject, ecRoleGroupType.gtUser, fIgnoreProjectStatus)   ' D0493 + D0511
                    If Not tPrjUW Is Nothing Then
                        UWList.Add(tPrjUW)
                    Else
                        If ApplicationError.Status = ecErrorStatus.errWrongLicense Then Return ecAuthenticateError.aeWrongLicense ' D1490
                        'If tProject.ProjectParticipating <> ecProjectParticipating.ppAccessCodeEnabled Then Return ecAuthenticateError.aeProjectLocked Else Return ecAuthenticateError.aeWorkgroupLocked   ' -D0748
                        If Not tProject.isPublic Then Return ecAuthenticateError.aePasscodeNotAllowed ' D1057 + D2017
                        If Not tProject.isOnline Then Return ecAuthenticateError.aeProjectLocked Else Return ecAuthenticateError.aeWorkgroupLocked ' D0748
                    End If
                End If
                ' D0487 ==
                ' D0505 ===
                'If tProject.ProjectStatus <> ecProjectStatus.psActive AndAlso Not tUser.CannotBeDeleted AndAlso Not CanUserDoAction(ecActionType.at_alManageAnyModel, tPrjUW) Then  ' D0740
                'Dim tmpWkg As clsWorkgroup = DBWorkgroupByID(WkgID, True, True)   ' D2436
                Dim tmpWkg As clsWorkgroup = clsWorkgroup.WorkgroupByID(WkgID, WkgList)   ' D2436 +  D4606
                If tProject.ProjectStatus <> ecProjectStatus.psActive AndAlso Not tUser.CannotBeDeleted AndAlso Not CanUserDoAction(ecActionType.at_alManageAnyModel, tPrjUW, tmpWkg) AndAlso Not CanUserModifyProject(tUser.UserID, tProject.ID, tPrjUW, clsWorkspace.WorkspaceByUserIDAndProjectID(tUser.UserID, tProject.ID, tWSList), tmpWkg) Then  ' D0740 + D2436
                    DebugInfo("Project has status " + tProject.ProjectStatus.ToString, _TRACE_INFO)
                    Return ecAuthenticateError.aeProjectReadOnly    ' D2489
                End If
                ' D0505 ==
            End If

            ' D0499 ===
            If UWList.Count = 0 AndAlso Not AllowSpecialLogon Then Return ecAuthenticateError.aeNoProjectsFound    ' D7205
            ' D0499 ==

            If WkgID <= 0 Then
                If UWList.Count = 1 Then
                    WkgID = UWList(0).WorkgroupID   ' D0475
                    DebugInfo("User has only one attached workgroup. Use it.", _TRACE_INFO)
                Else
                    ' D4842 ===
                    WkgID = GetLastUsedWorkgroupID(UWList, WkgList)  ' D4622
                    If WkgID < 0 Then   ' D4622
                        'ActiveUser = tUser
                        'UserWorkgroups = UWList
                        'DebugInfo("User has few attached workgroups, but we don't know, which one we should to use. Please check workgroup :)", _TRACE_WARNING)
                        If AllowSpecialLogon Then ActiveUser = tUser    ' D7205
                        Return ecAuthenticateError.aeNoWorkgroupSelected    ' D7205
                        ' D4842 ==
                    End If
                End If
            End If

            Dim fResetWkg As Boolean = False    ' D1175
            Dim tUW As clsUserWorkgroup = clsUserWorkgroup.UserWorkgroupByUserIDAndWorkgroupID(tUser.UserID, WkgID, UWList) ' D0475
            If tUW Is Nothing Then
                ' D1175 ===
                If fCanResetWkg AndAlso sPasscode = "" Then ' D2618
                    fResetWkg = True
                Else
                    DebugInfo("UserWokgroup not found", _TRACE_WARNING)
                    Return ecAuthenticateError.aeWorkgroupLocked
                End If
                ' D1175 ==
            Else    ' D1179
                If tUW.Status = ecUserWorkgroupStatus.uwDisabled Then
                    ' D1175 ===
                    If fCanResetWkg AndAlso sPasscode = "" Then    ' D2618
                        fResetWkg = True
                    Else
                        DebugInfo("UserWokgroup is disabled", _TRACE_WARNING)
                        Return ecAuthenticateError.aeUserWorkgroupLocked    ' D2618
                    End If
                    ' D1175 ==
                End If
                If tUW.isExpired Then
                    ' D1175 ===
                    If fCanResetWkg Then
                        fResetWkg = True
                    Else
                        DebugInfo("UserWokgroup is expired", _TRACE_WARNING)
                        Return ecAuthenticateError.aeUserWorkgroupExpired
                    End If
                    ' D1175 ==
                End If
            End If

            ' D1175 ===
            If fResetWkg Then
                ' D4842 ===
                WkgID = GetLastUsedWorkgroupID(UWList, WkgList)  ' D4622
                If WkgID < 0 Then   ' D4622
                    'ActiveUser = tUser
                    'UserWorkgroups = UWList
                    'DebugInfo("Default workgroup reset. User has few attached workgroups, Let him choose one.", _TRACE_WARNING)
                    If Not AllowSpecialLogon Then Return ecAuthenticateError.aeWorkgroupLocked  ' D7205
                    ' D4842 ==
                End If
            End If
            ' D1175 ==

            ' D7205 ===
            If WkgID < 0 Then
                If AllowSpecialLogon Then
                    ActiveUser = tUser
                    Return ecAuthenticateError.aeNoWorkgroupSelected
                Else
                    Return ecAuthenticateError.aeWorkgroupLocked
                End If
            End If
            ' D7205 ==

            If tUW Is Nothing Then Return ecAuthenticateError.aeWorkgroupLocked ' D1179

            'Dim tWkg As clsWorkgroup = DBWorkgroupByID(WkgID, True, True)
            Dim tWkg As clsWorkgroup = clsWorkgroup.WorkgroupByID(WkgID, WkgList)   ' D4606
            If tWkg Is Nothing Then
                ' D1175 ===
                If fCanResetWkg Then
                    fResetWkg = True
                Else
                    DebugInfo("Workgroup not found", _TRACE_WARNING)
                    Return ecAuthenticateError.aeWorkgroupLocked
                End If
                ' D1175 ==
            Else    ' D1179
                If tWkg.Status = ecWorkgroupStatus.wsDisabled Then
                    ' D1175 ===
                    If fCanResetWkg Then
                        fResetWkg = True
                    Else
                        DebugInfo("Workgroup is disabled", _TRACE_WARNING)
                        Return ecAuthenticateError.aeWorkgroupLocked
                    End If
                    ' D1175 ==
                End If
                If tWkg.Status <> ecWorkgroupStatus.wsSystem Then
                    If Not tWkg.License.isValidLicense Then
                        ' D1175 ===
                        If fCanResetWkg Then
                            fResetWkg = True
                        Else
                            DebugInfo("License is not valid!", _TRACE_WARNING)
                            Return ecAuthenticateError.aeWrongLicense
                        End If
                        ' D1175 ==
                    End If
                End If

                ' D2920 ===
                If tWkg.IsOldWorkgroup AndAlso tUser IsNot Nothing Then
                    ' If this is old wkg, let's to check if he is PM and Wkg member, need toupdate their permissions _before_ login;
                    If CheckPMsWhoIsWkgMember(tWkg, tUser.UserID).Count > 0 Then fResetWkg = True
                End If
                ' D2920 ==
            End If

            ' D1175 ===
            If fResetWkg Then
                ' D4842 ===
                WkgID = GetLastUsedWorkgroupID(UWList, WkgList)  ' D4622
                If WkgID < 0 Then   ' D4622
                    'ActiveUser = tUser
                    'UserWorkgroups = UWList
                    'DebugInfo("Default workgroup reset. User has few attached workgroups, Let him choose one.", _TRACE_WARNING)
                    Return ecAuthenticateError.aeWorkgroupExpired
                    ' D4842 ==
                End If
            End If
            ' D1175 ==

            If tWkg Is Nothing Then Return ecAuthenticateError.aeNoProjectsFound ' D1179

            If tWkg.Status <> ecWorkgroupStatus.wsSystem Then   ' D1175
                If Not tWkg.License.CheckParameterByID(ecLicenseParameter.ExpirationDate, Nothing, True) Then   ' D0913
                    ' D0811 ===
                    'If sPasscode = "" AndAlso tUser.DefaultWorkgroupID > 0 AndAlso DBUserWorkgroupsByUserID(tUser.UserID).Count > 1 Then
                    If sPasscode = "" AndAlso tUser.DefaultWorkgroupID > 0 AndAlso UWList IsNot Nothing AndAlso UWList.Count > 1 Then   ' D1134
                        tUser.DefaultWorkgroupID = -1
                        DBUserUpdate(tUser, False, "Reset default workgroup (old one is expired)")
                        Return Logon(sEmail, sPassword, sPasscode, fIgnoreProjectStatus, fAllowBlankPsw, False)    ' D1672 + D1724
                    Else
                        ' D4841 ===
                        'If (UWList IsNot Nothing AndAlso UWList.Count > 1) OrElse (tUser IsNot Nothing AndAlso tUser.CannotBeDeleted) Then
                        If tUser IsNot Nothing AndAlso tUser.CannotBeDeleted Then   ' D4842
                            ActiveUser = tUser
                            UserWorkgroups = UWList
                            ActiveWorkgroup = SystemWorkgroup
                            DebugInfo("Default workgroup reset to System due to unable choose another.", _TRACE_WARNING)
                            Return ecAuthenticateError.aeNoErrors   ' D48482
                        End If
                        ' D4841 ==
                    End If
                    ' D0811 ==
                    DebugInfo(String.Format("License for workgroup '{0}' is expired", tWkg.Name), _TRACE_WARNING)
                    'ApplicationError.Message = LicenseErrorMessage(tWkg.License, ecLicenseParameter.ExpirationDate, False)  ' D1208
                    'Return ecAuthenticateError.aeWrongLicense
                    If CanUserDoAction(ecActionType.at_alCreateNewModel, tUW, tWkg) Then
                        Return ecAuthenticateError.aeWorkgroupExpired
                    Else
                        Return ecAuthenticateError.aeWorkgroupExpiredEval
                    End If
                End If

                ' D3947 ===
                If Not tWkg.License.CheckParameterByID(ecLicenseParameter.InstanceID, Nothing, True) Then
                    DebugInfo(String.Format("License for workgroup '{0}' have wrong InstanceID", tWkg.Name), _TRACE_WARNING)
                    ' D3952 ===
                    If tUser IsNot Nothing AndAlso tUser.CannotBeDeleted Then
                        tWkg = SystemWorkgroup
                        'If tWkg IsNot Nothing Then tUW = DBUserWorkgroupByUserIDWorkgroupID(tUser.UserID, tWkg.ID)
                        If tWkg IsNot Nothing Then tUW = clsUserWorkgroup.UserWorkgroupByUserIDAndWorkgroupID(tUser.UserID, tWkg.ID, UWList) ' D4606
                        tProject = Nothing
                        sPasscode = ""
                    Else
                        ' D3952 ==
                        Return ecAuthenticateError.aeWrongInstanceID
                    End If
                End If
                ' D3947 ==

            End If

            Dim tRG As clsRoleGroup = tWkg.RoleGroup(tUW.RoleGroupID, tWkg.RoleGroups)
            If tRG Is Nothing Then
                DebugInfo("Role Group not found", _TRACE_WARNING)
                Return ecAuthenticateError.aeGroupLocked
            End If
            If tRG.Status = ecRoleGroupStatus.gsDisabled Then
                DebugInfo("Role Group is disabled", _TRACE_WARNING)
                Return ecAuthenticateError.aeGroupLocked
            End If

            If tWkg.Status <> ecWorkgroupStatus.wsSystem Then   ' D0475
                If tProject Is Nothing AndAlso tWSList.Count = 1 AndAlso (UWList Is Nothing OrElse UWList.Count = 1) AndAlso Not (tRG.ActionStatus(ecActionType.at_alCreateNewModel) = ecActionStatus.asGranted Or tRG.ActionStatus(ecActionType.at_alUploadModel) = ecActionStatus.asGranted Or tRG.ActionStatus(ecActionType.at_alManageAnyModel) = ecActionStatus.asGranted) Then    ' D0499 + D0878
                    DebugInfo("User has only one active project. Use it.", _TRACE_INFO)
                    tProject = DBProjectByID(tWSList(0).ProjectID)
                    ' D0864 ===
                    If tProject IsNot Nothing Then
                        Dim tWS As clsWorkspace = clsWorkspace.WorkspaceByUserIDAndProjectID(tUser.UserID, tProject.ID, tWSList)
                        Dim tPG As clsRoleGroup = tWkg.RoleGroup(tWS.GroupID, tWkg.RoleGroups)
                        If tPG IsNot Nothing AndAlso (tPG.ActionStatus(ecActionType.at_mlDeleteModel) = ecActionStatus.asGranted Or
                           tPG.ActionStatus(ecActionType.at_mlManageModelUsers) = ecActionStatus.asGranted Or
                           tPG.ActionStatus(ecActionType.at_mlManageProjectOptions) = ecActionStatus.asGranted Or
                           tPG.ActionStatus(ecActionType.at_mlModifyAlternativeHierarchy) = ecActionStatus.asGranted Or
                           tPG.ActionStatus(ecActionType.at_mlModifyModelHierarchy) = ecActionStatus.asGranted) Then tProject = Nothing
                        If tProject IsNot Nothing AndAlso (tProject.ProjectStatus <> ecProjectStatus.psActive OrElse tProject.isTeamTimeImpact OrElse tProject.isTeamTimeLikelihood) Then tProject = Nothing ' D2957
                    End If
                    ' D0864 ==
                End If

                ' D7205 ===
                If AllowSpecialLogon AndAlso tProject Is Nothing Then
                    Dim PrjList As List(Of clsWorkspace) = DBWorkspacesByUserIDWorkgroupID(tUser.UserID, tWkg.ID)
                    If PrjList IsNot Nothing AndAlso PrjList.Count > 0 Then tProject = DBProjectByID(PrjList(0).ProjectID)
                    If tProject IsNot Nothing AndAlso (tProject.ProjectStatus <> ecProjectStatus.psActive OrElse tProject.isMarkedAsDeleted OrElse tProject.isTeamTimeImpact OrElse tProject.isTeamTimeLikelihood) Then tProject = Nothing  ' D7247
                End If
                ' D7205 ==
            End If

            ' D3314 ===
            If tProject IsNot Nothing AndAlso Not tProject.isValidDBVersion AndAlso Not tProject.isDBVersionCanBeUpdated Then
                tProject = Nothing
                sPasscode = ""
            End If
            ' D3314 ==

            If Not tProject Is Nothing Then
                If sPasscode = "" Then sPasscode = tProject.Passcode ' D1724 + D2928
                Dim tWS As clsWorkspace = clsWorkspace.WorkspaceByUserIDAndProjectID(tUser.UserID, tProject.ID, tWSList)
                If tWS Is Nothing Then
                    If Not fIgnoreProjectStatus AndAlso Not tProject.isPublic Then
                        DebugInfo("Access code is not available for new users.", _TRACE_WARNING)    ' D1726
                        Return ecAuthenticateError.aePasscodeNotAllowed ' D0818 + D1219 + D1726
                    End If
                    DebugInfo("Workspace not found. Try to attach project", _TRACE_WARNING)
                    Dim fCanManage As Boolean = tRG.ActionStatus(ecActionType.at_alManageAnyModel) = ecActionStatus.asGranted   ' D2287
                    tWS = AttachProject(tUser, tProject, Not fCanManage, tWkg.GetDefaultRoleGroupID(ecRoleLevel.rlModelLevel, CType(IIf(fCanManage, ecRoleGroupType.gtProjectManager, ecRoleGroupType.gtEvaluator), ecRoleGroupType)), "", False)  ' D0487 + D0499 + D2287 + D2644 + D2780
                    If Not tWS Is Nothing Then
                        tWSList.Add(tWS)
                    Else
                        If ApplicationError.Status = ecErrorStatus.errWrongLicense Then Return ecAuthenticateError.aeWrongLicense Else Return ecAuthenticateError.aeWorkspaceLocked ' D0487 + D1490
                    End If
                End If
                If tWS.Status(tProject.isImpact) = ecWorkspaceStatus.wsDisabled Then    ' D1945
                    DebugInfo("Workspace is locked", _TRACE_WARNING)
                    Return ecAuthenticateError.aeWorkspaceLocked
                End If

                ' D2928 ===
                If tProject.isMarkedAsDeleted Then
                    DebugInfo("Project is marked as deleted")
                    Return ecAuthenticateError.aeDeletedProject
                End If
                If tProject.ProjectStatus <> ecProjectStatus.psActive AndAlso Not tUser.CannotBeDeleted AndAlso Not CanUserDoAction(ecActionType.at_alManageAnyModel, tUW, tWkg) AndAlso Not CanUserModifyProject(tUser.UserID, tProject.ID, tUW, tWS, tWkg) Then
                    DebugInfo("Project has status " + tProject.ProjectStatus.ToString, _TRACE_INFO)
                    Return ecAuthenticateError.aeProjectReadOnly
                End If
                ' D2928 ==

                ' D0850 ===
                If tProject.ProjectStatus = ecProjectStatus.psActive AndAlso Not tProject.isOnline AndAlso
                    (Not fIgnoreProjectStatus AndAlso Not CanUserModifyProject(tUser.UserID, tProject.ID, tUW, tWS, tWkg) AndAlso
                     Not CanUserDoProjectAction(ecActionType.at_mlUsePredefinedReports, tUser.UserID, tProject.ID, tUW, tWkg)) Then ' D0873 + D0931 + D2650
                    DebugInfo("Project is off-line and user can't modify or view it", _TRACE_WARNING)
                    Return ecAuthenticateError.aeProjectLocked
                End If
                ' D0850 ==
                ' D2433 ===
                If ((tProject.isTeamTimeImpact AndAlso Not tWS.isInTeamTime(True)) OrElse
                    (tProject.isTeamTimeLikelihood AndAlso Not tWS.isInTeamTime(False))) AndAlso
                     tProject.isValidDBVersion AndAlso Not tProject.PipeParameters.TeamTimeAllowMeetingID Then
                    DebugInfo("Not allowed to join to this TT session")
                    Return ecAuthenticateError.aeSynchronousUserNotAllowed
                End If
                ' D2433 ==
                DebugInfo(String.Format("Set active project as passcode '{0}'", sPasscode), _TRACE_INFO)
            End If

            ActiveUser = tUser
            ActiveWorkgroup = tWkg
            UserWorkgroups = UWList
            ActiveUserWorkgroup = tUW
            Workspaces = tWSList

            ' D3544 ===
            ' When user logged and no active project specified, reset all previous in his userworkgroups
            If ActiveUser IsNot Nothing AndAlso Database.Connect Then
                Dim tParams As New List(Of Object)
                tParams.Add(ActiveUser.UserID)
                Dim sSQL As String = String.Format("UPDATE {0} SET {1}=-1 WHERE {2}=? AND {1}>0", _TABLE_USERWORKGROUPS, _FLD_USERWRKG_LASTPROJECTID, _FLD_USERWRKG_USERID)
                'If tProject IsNot Nothing Then
                '    sSQL += String.Format(" AND {0}<>?", _FLD_USERWRKG_WRKGID)
                '    tParams.Add(tProject.WorkgroupID)
                'End If
                Database.ExecuteSQL(sSQL, tParams)

                If ActiveUser.PasswordStatus >= 0 AndAlso (_DEF_PASSWORD_ATTEMPTS < 2 OrElse ActiveUser.PasswordStatus < _DEF_PASSWORD_ATTEMPTS) Then   ' D7327
                    ' D7305 ===
                    If _DEF_PASSWORD_COMPLEXITY Then
                        Dim Res As List(Of String) = ValidatePswComplexity(ActiveUser.UserPassword, False, ActiveUser, Nothing)
                        If Res IsNot Nothing AndAlso Res.Count > 0 Then
                            ActiveUser.PasswordStatus = -1  ' not for admins
                            If ActiveUser.PasswordStatus < 0 Then DBSaveLog(dbActionType.actRedirect, dbObjectType.einfUser, ActiveUser.UserID, "Password is not suitable for requirements", String.Join(", ", Res.ToArray()))
                        End If
                        ' D7327 ===
                    Else
                        If Not fAllowBlankPsw AndAlso Not ActiveUser.HasPassword Then
                            ActiveUser.PasswordStatus = -1
                            If ActiveUser.PasswordStatus < 0 Then DBSaveLog(dbActionType.actRedirect, dbObjectType.einfUser, ActiveUser.UserID, "Password is blank, but must be specified", "")
                        End If
                        ' D7327 ==
                    End If
                    ' D7305 ==
                End If
            End If
            ' D3544 ==

            If Not tProject Is Nothing Then
                ActiveProject = tProject ' D0491
                ' D1724 ===
                If ActiveProject IsNot Nothing Then
                    If HasActiveProject() AndAlso ActiveProject.isValidDBVersion Then
                        If sPasscode.ToLower = ActiveProject.PasscodeLikelihood.ToLower Then ActiveProject.ProjectManager.ActiveHierarchy = ECCore.ECHierarchyID.hidLikelihood
                        If ActiveProject.IsRisk AndAlso sPasscode.ToLower = ActiveProject.PasscodeImpact.ToLower Then ActiveProject.ProjectManager.ActiveHierarchy = ECCore.ECHierarchyID.hidImpact ' D2898
                    End If
                    ' D1724 ==
                End If
            End If

            Return ecAuthenticateError.aeNoErrors
        End Function
        ' D0469 ==

        ' D0423 ===
        Public Function LogonByCredentials(ByVal sCredentials As String) As ecAuthenticateError
            Dim Res As ecAuthenticateError = ecAuthenticateError.aeUnknown
            Dim tCred As New clsCredentials
            If clsCredentials.TryParseHash(sCredentials, DatabaseID, tCred) Then    ' D0826
                Dim fNeedAuth As Boolean = True
                If isAuthorized Then
                    If ActiveUser.UserEmail.ToLower = tCred.UserEmail.ToLower Then
                        fNeedAuth = False
                        Res = ecAuthenticateError.aeNoErrors    ' D0822
                    End If
                End If
                If fNeedAuth Then
                    If tCred.InstanceID.ToLower = DatabaseID.ToLower Then
                        If isServiceRun AndAlso Not String.IsNullOrEmpty(tCred.SessionID) AndAlso Not tCred.SessionID.EndsWith("@") Then Options.SessionID = tCred.SessionID + "@" ' D2289
                        Dim tUser As clsApplicationUser = DBUserByEmail(tCred.UserEmail)    ' D0486
                        If Not tUser Is Nothing Then
                            Res = Logon(tUser.UserEmail, tUser.UserPassword, tCred.Passcode, True, True, False)    ' D0486 + D0931 + D1672 + D1724 + D7327
                            ' D0718 ===
                            'If Res = ecAuthenticateError.aeNoWorkgroupSelected OrElse (tCred.Passcode = "" AndAlso ActiveWorkgroup IsNot Nothing AndAlso tCred.WorkgroupID >= 0 AndAlso tCred.WorkgroupID <> ActiveWorkgroup.ID) Then   ' D4564
                            If (tCred.Passcode = "" AndAlso ActiveWorkgroup IsNot Nothing AndAlso tCred.WorkgroupID >= 0 AndAlso tCred.WorkgroupID <> ActiveWorkgroup.ID) Then   ' D4564 + D4842
                                Dim tWkg As clsWorkgroup = clsWorkgroup.WorkgroupByID(tCred.WorkgroupID, AvailableWorkgroups(ActiveUser, UserWorkgroups))
                                If tWkg IsNot Nothing AndAlso tWkg.Status = ecWorkgroupStatus.wsEnabled Then ' D1175
                                    ActiveWorkgroup = tWkg
                                    ActiveUserWorkgroup = clsUserWorkgroup.UserWorkgroupByUserIDAndWorkgroupID(ActiveUser.UserID, tWkg.ID, UserWorkgroups)
                                    Res = ecAuthenticateError.aeNoErrors
                                End If
                            End If
                            ' D0718 ==
                        Else
                            Res = ecAuthenticateError.aeNoUserFound
                        End If
                    Else
                        Res = ecAuthenticateError.aeWrongCredentials ' D0557 + D0823 + D3947
                    End If
                Else
                    Res = ecAuthenticateError.aeNoErrors
                End If
                If Res = ecAuthenticateError.aeNoErrors Then
                    '-D0486: project should be set after user auth
                    'If ProjectID <= 0 Then
                    '    Dim tProject As clsProject = DBProjectByPasscode(tCred.Passcode)    ' D0486
                    '    If Not tProject Is Nothing Then ProjectID = tProject.ID
                    'End If
                End If
            Else
                Res = ecAuthenticateError.aeWrongCredentials
            End If
            Return Res
        End Function
        ' D0423 ==

        ' D0421 ===
        'Public Function LogonAndGetCredentials(ByVal sEmail As String, ByVal sPassword As String, ByRef sPasscode As String, ByRef sCredentialsHash As String) As ecAuthenticateError    ' D1672 + D1724
        '    Dim fRes As ecAuthenticateError = Logon(sEmail, sPassword, sPasscode, False)   ' D0486 + D1672 + D1724
        '    If fRes = ecAuthenticateError.aeNoErrors Then
        '        If isAuthorized AndAlso sPasscode <> "" Then
        '            '-D0486: project should be set after user auth
        '            'Dim tProject As clsProject = Project(sPasscode) ' D0432
        '            'If Not tProject Is Nothing Then ProjectID = tProject.ID ' D0432
        '            Dim WkgID As Integer = -1
        '            If ActiveWorkgroup IsNot Nothing Then WkgID = ActiveWorkgroup.ID ' D0718
        '            sCredentialsHash = New clsCredentials(ActiveUser.UserEmail, sPasscode, DatabaseID, WkgID, Options.SessionID).HashString    ' D0718 + D2289
        '        Else
        '            fRes = ecAuthenticateError.aeNoProjectsFound
        '        End If
        '    End If
        '    Return fRes
        'End Function

        Public Function isValidCredentials(ByVal sCredentalsHash As String, Optional ByRef tCredentals As clsCredentials = Nothing) As Boolean
            Dim fResult As Boolean = False
            Dim tmpCredentials As New clsCredentials
            DebugInfo("Start to check credentials string...")
            If clsCredentials.TryParseHash(sCredentalsHash, DatabaseID, tmpCredentials) Then    ' D0826
                If tmpCredentials.InstanceID.ToLower = DatabaseID.ToLower Then  ' D0557
                    If tmpCredentials.UserEmail <> "" AndAlso (tmpCredentials.Passcode <> "" Or tmpCredentials.WorkgroupID >= 0) Then    ' D0718
                        DebugInfo("Credentials string is valid")
                        Dim tmpUser As clsApplicationUser = DBUserByEmail(tmpCredentials.UserEmail) ' D0486

                        If tmpCredentials.Passcode <> "" Then       ' D0718
                            Dim tmpPrj As clsProject = DBProjectByPasscode(tmpCredentials.Passcode)    ' D0486
                            If Not tmpUser Is Nothing AndAlso Not tmpPrj Is Nothing Then
                                If Not DBWorkspaceByUserIDProjectID(tmpUser.UserID, tmpPrj.ID) Is Nothing Then  ' D0486
                                    fResult = True
                                    If Not tCredentals Is Nothing Then tCredentals = tmpCredentials
                                    DebugInfo("User and Project are checked")
                                End If
                            End If
                        End If

                        ' D0718 ===
                        If tmpCredentials.WorkgroupID >= 0 Then
                            Dim tUW As clsUserWorkgroup = DBUserWorkgroupByUserIDWorkgroupID(tmpUser.UserID, tmpCredentials.WorkgroupID)
                            If tUW IsNot Nothing Then
                                fResult = True
                                If Not tCredentals Is Nothing Then tCredentals = tmpCredentials
                                DebugInfo("User and WorkgroupID are checked")
                            End If
                        End If
                        ' D0718 ==

                    End If
                End If
            Else
                DebugInfo("Wrong credentials string", _TRACE_WARNING)
            End If
            Return fResult
        End Function
        ' D0421 ==

        ' D0505 ===
        'Public Function LogonTeamTime(ByVal sUserEmail As String, ByVal sUserPassword As String) As ecAuthenticateError  ' D1672 + D1724
        '    Dim res As ecAuthenticateError = Logon(sUserEmail, sUserPassword, "", False)   ' D1672 + D1724
        '    If ActiveUser Is Nothing Then
        '        res = ecAuthenticateError.aeNoUserFound
        '    Else
        '        If res = ecAuthenticateError.aeNoErrors Or res = ecAuthenticateError.aeNoWorkgroupSelected Then ' D0521
        '            Dim TTList As List(Of clsWorkspace) = DBWorkspacesTeamTimeByUserID(ActiveUser.UserID)   ' D0506
        '            If TTList.Count = 1 Then
        '                ' D0521 ===
        '                Dim tPrj As clsProject = DBProjectByID(TTList(0).ProjectID)
        '                If Not tPrj Is Nothing Then
        '                    If ActiveWorkgroup Is Nothing Then ActiveWorkgroup = DBWorkgroupByID(tPrj.WorkgroupID)
        '                    If Not ActiveWorkgroup Is Nothing Then res = ecAuthenticateError.aeNoErrors
        '                End If
        '                ProjectID = TTList(0).ProjectID
        '                ' D0521 ==
        '            End If
        '            If ActiveProject Is Nothing Or TTList.Count <> 1 Then res = ecAuthenticateError.aeNoSynchronousProject
        '        End If
        '    End If
        '    Return res
        'End Function
        ' D0505 ==

        ' D0466 ===
        Public Function CanUserDoAction(ByVal tActionType As ecActionType, ByVal tUserWorkgroup As clsUserWorkgroup, Optional ByVal tWorkgroup As clsWorkgroup = Nothing) As Boolean    ' D0873
            If tActionType = ecActionType.atUnspecified Then Return True
            If Options.isSingleModeEvaluation Then
                Return tActionType = ecActionType.at_mlEvaluateModel
            End If
            If tWorkgroup Is Nothing Then tWorkgroup = ActiveWorkgroup ' D0873
            ' D4981 ===
            ' Check is it system action: use SystemWorkgroup and related UserWorkgroup in that case
            Select Case tActionType
                'Case ecActionType.at_slCreateWorkgroup, ecActionType.at_slDeleteAnyWorkgroup, ecActionType.at_slManageAllUsers,
                '    ecActionType.at_slManageAnyWorkgroup, ecActionType.at_slSetLicenseAnyWorkgroup, ecActionType.at_slViewAnyWorkgroupLogs,
                '    ecActionType.at_slViewAnyWorkgroupReports, ecActionType.at_slViewLicenseAnyWorkgroup,
                '    ecActionType.at_slDeleteOwnWorkgroup, ecActionType.at_slManageOwnWorkgroup, ecActionType.at_slSetLicenseOwnWorkgroup,
                '    ecActionType.at_slViewLicenseOwnWorkgroup, ecActionType.at_slViewOwnWorkgroupLogs, ecActionType.at_slViewOwnWorkgroupReports
                Case ecActionType.at_slCreateWorkgroup, ecActionType.at_slDeleteAnyWorkgroup, ecActionType.at_slManageAllUsers,
                    ecActionType.at_slManageAnyWorkgroup, ecActionType.at_slSetLicenseAnyWorkgroup, ecActionType.at_slViewAnyWorkgroupLogs,
                    ecActionType.at_slViewAnyWorkgroupReports, ecActionType.at_slViewLicenseAnyWorkgroup    ' D7269
                    If isAuthorized Then
                        tWorkgroup = SystemWorkgroup
                        tUserWorkgroup = clsUserWorkgroup.UserWorkgroupByUserIDAndWorkgroupID(ActiveUser.UserID, tWorkgroup.ID, SystemUserWorkgroups)
                    End If
            End Select
            ' D4981 ==
            If tWorkgroup Is Nothing OrElse tUserWorkgroup Is Nothing Then Return False ' D0873
            Dim RG As clsRoleGroup = Nothing
            If ActiveUserWorkgroup Is tUserWorkgroup Then RG = ActiveRoleGroup
            If RG Is Nothing Then RG = tWorkgroup.RoleGroup(tUserWorkgroup.RoleGroupID) ' D0873
            If RG Is Nothing Then Return False
            Dim fRes As Boolean = (RG.ActionStatus(tActionType) = ecActionStatus.asGranted)
            ' D7270 ===
            If Not fRes AndAlso isAuthorized Then
                Select Case tActionType
                    Case ecActionType.at_slDeleteOwnWorkgroup, ecActionType.at_slManageOwnWorkgroup, ecActionType.at_slSetLicenseOwnWorkgroup,
                        ecActionType.at_slViewLicenseOwnWorkgroup, ecActionType.at_slViewOwnWorkgroupLogs, ecActionType.at_slViewOwnWorkgroupReports
                        If tWorkgroup.OwnerID = ActiveUser.UserID Then fRes = True
                    Case ecActionType.at_slManageOwnWorkgroup
                        If tWorkgroup.OwnerID = ActiveUser.UserID OrElse tWorkgroup.ECAMID = ActiveUser.UserID Then fRes = True
                End Select
            End If
            ' D7270 ==
            Return fRes
        End Function

        Public Function CanUserDoProjectAction(ByVal tActionType As ecActionType, ByVal tUserID As Integer, ByVal tProjectID As Integer, ByVal tUserWorkgroup As clsUserWorkgroup, Optional ByVal tWorkgroup As clsWorkgroup = Nothing) As Boolean   ' D0471 + D0873
            If tWorkgroup Is Nothing Then tWorkgroup = ActiveWorkgroup ' D0873
            If tUserID < 0 Or tProjectID < 0 Or tWorkgroup Is Nothing Then Return False ' D0873
            If tActionType = ecActionType.atUnspecified Then Return True
            If Options.isSingleModeEvaluation Then
                Return tActionType = ecActionType.at_mlEvaluateModel
            End If
            Dim RG As clsRoleGroup = Nothing
            If Not ActiveUser Is Nothing AndAlso HasActiveProject() AndAlso tUserID = ActiveUser.UserID AndAlso tProjectID = ProjectID Then    ' D0471 + D0873
                RG = ActiveProjectRoleGroup
            Else
                Dim WS As clsWorkspace = clsWorkspace.WorkspaceByUserIDAndProjectID(tUserID, tProjectID, Workspaces)
                If WS Is Nothing Then WS = DBWorkspaceByUserIDProjectID(tUserID, tProjectID)
                If WS Is Nothing Then
                    If tUserWorkgroup Is Nothing Then Return False
                    If tActionType = ecActionType.at_mlDeleteModel Then
                        Dim RealRG As clsRoleGroup = tWorkgroup.RoleGroup(tUserWorkgroup.RoleGroupID)   ' D0873
                        If RealRG Is Nothing Then Return False
                        Return RealRG.ActionStatus(ecActionType.at_alDeleteAnyModel) = ecActionStatus.asGranted
                    End If
                    If CanUserDoAction(ecActionType.at_alManageAnyModel, tUserWorkgroup, tWorkgroup) Then   ' D0873
                        WS = New clsWorkspace
                        WS.ProjectID = tProjectID
                        WS.StatusLikelihood = ecWorkspaceStatus.wsEnabled   ' D1945
                        WS.StatusImpact = ecWorkspaceStatus.wsEnabled       ' D1945
                        WS.UserID = tUserID
                        WS.GroupID = tWorkgroup.GetDefaultRoleGroupID(ecRoleLevel.rlModelLevel, ecRoleGroupType.gtProjectManager) ' D0873 + D2780
                    Else
                        Return False
                    End If
                End If
                RG = tWorkgroup.RoleGroup(WS.GroupID)   ' D0873
            End If
            If Not RG Is Nothing Then Return RG.ActionStatus(tActionType) = ecActionStatus.asGranted
            Return False
        End Function

        ' D4729 + D4735 ===
        Public Function CanUserCreateNewProject(ByRef Optional sError As String = Nothing) As Boolean   ' D6290
            Dim fResult As Boolean = False
            Dim sErr As String = ""
            If isAuthorized Then
                If CanUserDoAction(ecActionType.at_alCreateNewModel, ActiveUserWorkgroup, ActiveWorkgroup) OrElse CanUserDoAction(ecActionType.at_alUploadModel, ActiveUserWorkgroup, ActiveWorkgroup) Then
                    If ActiveWorkgroup IsNot Nothing AndAlso ActiveWorkgroup.License IsNot Nothing AndAlso ActiveWorkgroup.License.isValidLicense Then
                        If ActiveWorkgroup.License.CheckParameterByID(ecLicenseParameter.MaxLifetimeProjects,, False) Then
                            If ActiveWorkgroup.License.CheckParameterByID(ecLicenseParameter.MaxProjectsTotal,, False) Then
                                fResult = True
                            Else
                                sErr = LicenseErrorMessage(ActiveWorkgroup.License, ecLicenseParameter.MaxProjectsTotal)
                            End If
                        Else
                            sErr = LicenseErrorMessage(ActiveWorkgroup.License, ecLicenseParameter.MaxLifetimeProjects)
                        End If
                    Else
                        sErr = ResString("msgAuthWrongLicense")
                    End If
                Else
                    sErr = ResString("msgCantCreateProject")
                End If
            Else
                sErr = ResString("msgPleaseAuthorize")
            End If
            If sError IsNot Nothing Then sError = sErr
            Return fResult
        End Function
        ' D4729 + D4735 ==

        ' D4743 ===
        Public Function CanChangeProjectOnlineStatus(tPrj As clsProject, ByRef sMessage As String) As Boolean
            Dim fRes As Boolean = False
            If tPrj IsNot Nothing Then
                If tPrj.ProjectStatus <> ecProjectStatus.psActive OrElse tPrj.isMarkedAsDeleted Then
                    sMessage = ResString("lblProjectReadOnly")
                Else
                    ' D4556 ===
                    If tPrj.LockInfo.LockExpiration.HasValue AndAlso tPrj.LockInfo.LockExpiration.Value > Now Then
                        Select Case tPrj.LockInfo.LockStatus
                            Case ECLockStatus.lsLockForAntigua, ECLockStatus.lsLockForTeamTime
                                If tPrj.isOnline Then sMessage = ResString("lblProjectLockedByAntigua")   ' D6341 + D7375
                            Case ECLockStatus.lsLockForSystem
                                sMessage = ResString("lblProjectLockedBySystem")
                            Case ECLockStatus.lsLockForModify
                                Dim sLockUserName As String = "<unknown>"
                                If tPrj.LockInfo IsNot Nothing AndAlso tPrj.LockInfo.LockerUserID > 0 AndAlso Not tPrj.LockInfo.isLockedByUser(ActiveUser.UserID) Then
                                    Dim tUser As clsApplicationUser = DBUserByID(tPrj.LockInfo.LockerUserID)
                                    If tUser IsNot Nothing Then sLockUserName = CStr(IIf(tUser.UserName <> "", tUser.UserName, tUser.UserEmail))
                                    sMessage = String.Format(ResString("lblProjectLockedBy"), sLockUserName)
                                End If
                        End Select
                    End If
                    If sMessage = "" Then
                        If CanUserModifyProject(ActiveUser.UserID, tPrj.ID, ActiveUserWorkgroup, clsWorkspace.WorkspaceByUserIDAndProjectID(ActiveUser.UserID, tPrj.ID, Workspaces), ActiveWorkgroup) Then
                            If tPrj.isTeamTimeImpact OrElse tPrj.isTeamTimeLikelihood Then
                                If tPrj.isOnline Then sMessage = ResString("lblProjectLockedByTeamTime")    ' D7375
                            Else
                                fRes = True
                                If Not tPrj.isOnline Then   ' D5046
                                    Dim MaxOnline As Long = ActiveWorkgroup.License.GetParameterMaxByID(ecLicenseParameter.MaxProjectsOnline)
                                    If MaxOnline <> UNLIMITED_VALUE AndAlso MaxOnline <> LICENSE_NOVALUE Then
                                        Dim CurOnline As Long = ActiveWorkgroup.License.GetParameterValueByID(ecLicenseParameter.MaxProjectsOnline)
                                        If CurOnline >= MaxOnline Then
                                            sMessage = String.Format(ResString("lblProjectLockedByLicenseLimit"), MaxOnline)
                                            fRes = False
                                        End If
                                    End If
                                End If
                            End If
                        Else
                            sMessage = ResString("lblProjectReadOnly")
                        End If
                    End If
                    'Dim CanEdit As Boolean = tPrj.isMarkedAsDeleted OrElse tPrj.ProjectStatus <> ecProjectStatus.psActive OrElse Not CanUserModifyProject(ActiveUser.UserID, tPrj.ID, ActiveUserWorkgroup, clsWorkspace.WorkspaceByUserIDAndProjectID(ActiveUser.UserID, tPrj.ID, Workspaces), ActiveWorkgroup)
                    ''If tPrj IsNot Nothing AndAlso tPrj.ProjectStatus = ecProjectStatus.psActive AndAlso Not tPrj.isMarkedAsDeleted AndAlso Not tPrj.isTeamTimeImpact AndAlso Not tPrj.isTeamTimeLikelihood AndAlso Not IsReadOnly Then
                    'If CanEdit Then
                    '    fRes = tPrj.isOnline OrElse ActiveWorkgroup.License.CheckParameterByID(ecLicenseParameter.MaxProjectsOnline, Nothing, False)
                    'End If
                End If
            End If
            Return fRes
        End Function
        ' D4743 ==

        Public Function CanActiveUserModifyProject(ByVal tProjectID As Integer) As Boolean
            Return isAuthorized AndAlso CanUserModifyProject(ActiveUser.UserID, tProjectID, ActiveUserWorkgroup, clsWorkspace.WorkspaceByUserIDAndProjectID(ActiveUser.UserID, tProjectID, Workspaces), ActiveWorkgroup)
        End Function

        Public Function CanUserModifyProject(ByVal tUserID As Integer, ByVal tProjectID As Integer, ByVal tUserWorkgroup As clsUserWorkgroup, ByVal tWorkspace As clsWorkspace, Optional ByVal tWorkgroup As clsWorkgroup = Nothing) As Boolean    ' D0471 + D0835 + D0873
            'If Options.isSingleModeEvaluation Then Return False    ' -D1718
            ' D0873 ===
            If tWorkgroup Is Nothing Then tWorkgroup = ActiveWorkgroup
            If tWorkgroup IsNot Nothing AndAlso tUserWorkgroup IsNot Nothing AndAlso tWorkgroup.ID <> tUserWorkgroup.WorkgroupID Then tWorkgroup = Nothing
            If tWorkgroup Is Nothing AndAlso tUserWorkgroup IsNot Nothing Then tWorkgroup = DBWorkgroupByID(tUserWorkgroup.WorkgroupID, True, True)
            If tWorkgroup Is Nothing OrElse SystemWorkgroup Is Nothing Then Return False ' D1728            
            If tWorkgroup.Status = ecWorkgroupStatus.wsSystem Then Return False
            If (tWorkgroup.License IsNot Nothing AndAlso Not tWorkgroup.License.CheckParameterByID(ecLicenseParameter.ExpirationDate) OrElse Not tWorkgroup.License.CheckParameterByID(ecLicenseParameter.InstanceID)) OrElse (SystemWorkgroup.License Is Nothing OrElse Not SystemWorkgroup.License.isValidLicense OrElse Not SystemWorkgroup.License.CheckParameterByID(ecLicenseParameter.ExpirationDate) OrElse Not SystemWorkgroup.License.CheckParameterByID(ecLicenseParameter.InstanceID)) Then Return False ' D0811 + D0913 + D1728 + D3947
            ' D0873 ==
            If CanUserDoAction(ecActionType.at_alManageAnyModel, tUserWorkgroup, tWorkgroup) Then Return True ' D0873
            For Each tAction As ecActionType In _ROLES_MODIFYPROJECT
                If CanUserDoProjectAction(tAction, tUserID, tProjectID, tUserWorkgroup, tWorkgroup) Then Return True ' D0471 + D0873
            Next
            ' D0835 ===
            If tWorkspace IsNot Nothing Then
                Dim tGroup As clsRoleGroup = tWorkgroup.RoleGroup(tWorkspace.GroupID)  ' D0873
                If tGroup IsNot Nothing AndAlso (tGroup.ActionStatus(ecActionType.at_mlDeleteModel) = ecActionStatus.asGranted OrElse
                   tGroup.ActionStatus(ecActionType.at_mlManageModelUsers) = ecActionStatus.asGranted OrElse
                   tGroup.ActionStatus(ecActionType.at_mlManageProjectOptions) = ecActionStatus.asGranted OrElse
                   tGroup.ActionStatus(ecActionType.at_mlModifyAlternativeHierarchy) = ecActionStatus.asGranted OrElse
                   tGroup.ActionStatus(ecActionType.at_mlModifyModelHierarchy) = ecActionStatus.asGranted) Then Return True
            End If
            ' D0835 ==
            Return False
        End Function

        Public Function CanUserModifySomeProject(ByVal tUserID As Integer, ByVal tProjectsList As List(Of clsProject), ByVal tUserWorkgroup As clsUserWorkgroup, ByVal tWorkspaces As List(Of clsWorkspace)) As Boolean ' D0471 + D0835
            If Options.isSingleModeEvaluation Then Return False
            If ActiveWorkgroup Is Nothing Then Return False
            If ActiveWorkgroup.Status = ecWorkgroupStatus.wsSystem Then Return False
            'If Not ActiveWorkgroup.License.CheckParameterByID(EXPIRATION_DATE) Or Not SystemWorkgroup.License.CheckParameterByID(EXPIRATION_DATE) Then Return False ' D0811
            If CanUserDoAction(ecActionType.at_alManageAnyModel, tUserWorkgroup) Or
               CanUserDoAction(ecActionType.at_alCreateNewModel, tUserWorkgroup) Or
               CanUserDoAction(ecActionType.at_alUploadModel, tUserWorkgroup) Then Return True
            If tProjectsList Is Nothing Then tProjectsList = ActiveProjectsList
            If Not tProjectsList Is Nothing Then
                For Each tPrj As clsProject In tProjectsList
                    If CanUserModifyProject(tUserID, tPrj.ID, tUserWorkgroup, clsWorkspace.WorkspaceByUserIDAndProjectID(tUserID, tPrj.ID, tWorkspaces)) Then Return True ' D0835
                Next
            End If
            Return False
        End Function
        ' D0072 + D0466 ==

        ' D0473 ===
        Public ReadOnly Property CanUserDoSystemWorkgroupAction(ByVal tActionType As ecActionType, ByVal tUserID As Integer) As Boolean
            Get
                If SystemWorkgroup Is Nothing Or tUserID <= 0 Then Return False ' D0502
                Dim SystemUWG As clsUserWorkgroup = clsUserWorkgroup.UserWorkgroupByUserIDAndWorkgroupID(tUserID, SystemWorkgroup.ID, SystemUserWorkgroups) ' D0502
                'If SystemUWG Is Nothing Then SystemUWG = DBUserWorkgroupByUserIDWorkgroupID(tUserID, SystemWorkgroup.ID)
                If SystemUWG Is Nothing Then Return False
                If SystemUWG.Status = ecUserWorkgroupStatus.uwDisabled Then Return False
                Dim RG As clsRoleGroup = SystemWorkgroup.RoleGroup(SystemUWG.RoleGroupID)
                If RG Is Nothing Then Return False
                If Options.isSingleModeEvaluation Then Return False
                Dim fAllowed As Boolean = (RG.ActionStatus(tActionType) = ecActionStatus.asGranted) ' D0512
                Return fAllowed     ' D0512 // keep this for trace
            End Get
        End Property

        ' D2794 ===
        Public Function GetWorkspacesWhereUserIsPM(tUserID As Integer) As List(Of clsWorkspace)
            Dim tRes As New List(Of clsWorkspace)

            If ActiveWorkgroup IsNot Nothing Then

                Dim sSQL As String = String.Format("SELECT WS.* FROM Workspace WS LEFT JOIN Projects P ON WS.ProjectID = P.ID " +
                                                   "INNER JOIN RoleGroups R ON R.WorkgroupID=P.WorkgroupID AND WS.GroupID=R.ID AND R.RoleLevel={0} AND R.GroupType={1} AND R.Status={2} " +
                                                   "WHERE P.WorkgroupID={3} AND WS.UserID={4}",
                                                   CInt(ecRoleLevel.rlModelLevel), CInt(ecRoleGroupType.gtProjectManager), CInt(ecRoleGroupStatus.gsProjectManagerDefault),
                                                   ActiveWorkgroup.ID, tUserID)

                Dim Data As List(Of Dictionary(Of String, Object)) = Database.SelectBySQL(sSQL)
                If Data IsNot Nothing AndAlso Data.Count > 0 Then
                    For Each tRow As Dictionary(Of String, Object) In Data
                        Dim tWS As clsWorkspace = DBParse_Workspace(tRow)
                        If tWS IsNot Nothing Then tRes.Add(tWS)
                    Next
                End If
            End If

            Return tRes
        End Function

        Public Function GetProjectsWithPMsCount(tUserIDs As List(Of Integer)) As Dictionary(Of Integer, String)
            Dim tRes As New Dictionary(Of Integer, String)
            If ActiveWorkgroup IsNot Nothing AndAlso tUserIDs IsNot Nothing AndAlso tUserIDs.Count > 0 Then

                Dim sIDs As String = ""
                For Each tUID As Integer In tUserIDs
                    sIDs += CStr(IIf(sIDs = "", "", ",")) + tUID.ToString
                Next

                Dim sSQL As String = String.Format("SELECT P.ID, P.ProjectName, COUNT(P.ID) as PMs, SUM(CASE WHEN WS.UserID IN ({0}) THEN 1 ELSE 0 END) as HasCur " +
                                                   "FROM Projects P LEFT JOIN Workspace WS ON WS.ProjectID = P.ID " +
                                                   "INNER JOIN RoleGroups R ON R.WorkgroupID=P.WorkgroupID AND WS.GroupID=R.ID AND R.RoleLevel={1} AND R.GroupType={2} AND R.Status = {3} " +
                                                   "WHERE P.WorkgroupID={4} GROUP BY P.ID, P.ProjectName ORDER BY P.ProjectName",
                                                    sIDs,
                                                   CInt(ecRoleLevel.rlModelLevel), CInt(ecRoleGroupType.gtProjectManager), CInt(ecRoleGroupStatus.gsProjectManagerDefault),
                                                   ActiveWorkgroup.ID)

                Dim Data As List(Of Dictionary(Of String, Object)) = Database.SelectBySQL(sSQL)
                If Data IsNot Nothing AndAlso Data.Count > 0 Then

                    For Each tRow As Dictionary(Of String, Object) In Data
                        Dim PID As Integer = CInt(tRow("ID"))
                        Dim PrjName As String = CStr(tRow("ProjectName"))
                        Dim PMs As Integer = CInt(tRow("PMs"))
                        Dim HasCur As Integer = CInt(tRow("HasCur"))
                        If HasCur > 0 Then
                            tRes.Add(PID, String.Format("{0}{1}", IIf(PMs - HasCur > 0, "", ResString("lblProjectMarkWhenNoPMs") + " "), PrjName))
                        End If
                    Next
                End If

            End If
            Return tRes
        End Function
        ' D2794 ==

        ' D2772 ===
        ''' <summary>
        ''' Function for check specified or all users in project in workgroup. Selected PMs only, but who is workgroup member. Will set their as Project Organazers.
        ''' </summary>
        ''' <param name="tWorkgroup">Workgroup</param>
        ''' <param name="tUserID">Optional, checked all users in workgroup when -1</param>
        ''' <returns>List of updated UserIDs</returns>
        ''' <remarks></remarks>
        Public Function CheckPMsWhoIsWkgMember(tWorkgroup As clsWorkgroup, Optional tUserID As Integer = -1) As List(Of Integer)
            Dim tRes As New List(Of Integer)

            If tWorkgroup IsNot Nothing AndAlso tWorkgroup.Status <> ecWorkgroupStatus.wsSystem AndAlso tWorkgroup.IsOldWorkgroup Then  ' D2821

                Dim sSQL As String = String.Format("SELECT DISTINCT(W.UserID) as WS_UID, UW.*, U.Email FROM Workspace W " +
                                                   "LEFT JOIN Projects P ON P.ID=W.ProjectID " +
                                                   "LEFT JOIN RoleGroups RG ON RG.ID=W.GroupID " +
                                                   "LEFT JOIN UserWorkgroups UW ON UW.UserID = W.UserID AND UW.WorkgroupID=P.WorkgroupID " +
                                                   "LEFT JOIN RoleGroups RGW ON RGW.ID=UW.RoleGroupID " +
                                                   "LEFT JOIN Users U ON U.ID=W.UserID " +
                                                   "WHERE P.WorkgroupID={0} AND RG.RoleLevel = {1} AND RG.GroupType={2} AND RG.Status = {3} " +
                                                   "AND RGW.RoleLevel={4} AND RGW.GroupType={5} AND RGW.Status={6}{7}",
                                                   tWorkgroup.ID,
                                                   CInt(ecRoleLevel.rlModelLevel), CInt(ecRoleGroupType.gtProjectManager), CInt(ecRoleGroupStatus.gsProjectManagerDefault),
                                                   CInt(ecRoleLevel.rlApplicationLevel), CInt(ecRoleGroupType.gtUser), CInt(ecRoleGroupStatus.gsUserDefault),
                                                   IIf(tUserID > 0, " AND W.UserID = " + tUserID.ToString, "")) ' D2793

                Dim Data As List(Of Dictionary(Of String, Object)) = Database.SelectBySQL(sSQL)
                If Data IsNot Nothing AndAlso Data.Count > 0 Then

                    Dim tGrpID As Integer = tWorkgroup.GetDefaultRoleGroupID(ecRoleLevel.rlApplicationLevel, ecRoleGroupType.gtProjectOrganizer)

                    For Each tRow As Dictionary(Of String, Object) In Data
                        Dim tUW As clsUserWorkgroup = DBParse_UserWorkgroup(tRow)
                        If tUW IsNot Nothing Then
                            tUW.RoleGroupID = tGrpID
                            If DBUserWorkgroupUpdate(tUW, False, String.Format("Update user '{0}' to Project Organizer because he is PM", tRow("Email").ToString)) Then tRes.Add(tUW.UserID)
                        End If
                    Next
                End If
            End If

            Return tRes
        End Function
        ' D2772 ==

        ' D4645 ===
        Public Function isSystemManager(Optional tUser As clsApplicationUser = Nothing) As Boolean
            If tUser Is Nothing Then tUser = ActiveUser
            If tUser IsNot Nothing Then
                Return CanUserDoSystemWorkgroupAction(ecActionType.at_slManageAnyWorkgroup, tUser.UserID)
            End If
            Return False
        End Function
        ' D4645 ==

        ' D0474 ===
        Public Overloads Function isWorkgroupOwner(ByVal tWorkgroup As clsWorkgroup, ByVal tUser As clsApplicationUser, ByVal tUserWorkgroup As clsUserWorkgroup) As Boolean
            If tWorkgroup Is Nothing OrElse tUser Is Nothing Then Return False
            If tUser.CannotBeDeleted OrElse CanUserDoSystemWorkgroupAction(ecActionType.at_slManageAnyWorkgroup, tUser.UserID) Then Return True ' D4981
            'Return (tWorkgroup.OwnerID = tUser.UserID OrElse tWorkgroup.ECAMID = tUser.UserID) OrElse CanUserDoAction(ecActionType.at_slManageAnyWorkgroup, tUserWorkgroup) ' D7273
            If (CanUserDoAction(ecActionType.at_slManageOwnWorkgroup, tUserWorkgroup) AndAlso (tWorkgroup.OwnerID = tUser.UserID OrElse tWorkgroup.ECAMID = tUser.UserID)) OrElse
                CanUserDoAction(ecActionType.at_slManageAnyWorkgroup, tUserWorkgroup) Then Return True Else Return (tWorkgroup.OwnerID = tUser.UserID) ' D0092 + D7273
        End Function

        Public Function isWorkgroupAvailable(ByVal tWorkgroup As clsWorkgroup, ByVal tUser As clsApplicationUser, ByVal tUserWorkgroup As clsUserWorkgroup) As Boolean
            If tUser Is Nothing Or tWorkgroup Is Nothing Or tUserWorkgroup Is Nothing Then Return False
            If Not tUser.CannotBeDeleted AndAlso tUserWorkgroup.Status = ecUserWorkgroupStatus.uwDisabled Then Return False ' D0896
            If tUser.CannotBeDeleted Or tWorkgroup.Status <> ecWorkgroupStatus.wsDisabled Then Return True ' D0896
            ' if disabled Workgroup
            If CanUserDoSystemWorkgroupAction(ecActionType.at_slManageOwnWorkgroup, tUser.UserID) OrElse CanUserDoSystemWorkgroupAction(ecActionType.at_slDeleteOwnWorkgroup, tUser.UserID) OrElse CanUserDoSystemWorkgroupAction(ecActionType.at_slManageOwnWorkgroup, tUser.UserID) OrElse CanUserDoSystemWorkgroupAction(ecActionType.at_slSetLicenseOwnWorkgroup, tUser.UserID) OrElse CanUserDoSystemWorkgroupAction(ecActionType.at_slViewLicenseOwnWorkgroup, tUser.UserID) OrElse CanUserDoSystemWorkgroupAction(ecActionType.at_slViewOwnWorkgroupLogs, tUser.UserID) Then Return True ' D0289 + D0474
            Return isWorkgroupOwner(tWorkgroup, tUser, tUserWorkgroup) ' D0289 + D0474
            'Return CanDoAnyAdministrativeAction(tUser, tUserWorkgroup.ID) And (WG.OwnerID = tUser.UserID Or WG.ECAMID = tUser.UserID)  ' D0289
        End Function

        Public Function AvailableWorkgroups(ByVal tUser As clsApplicationUser, Optional ByVal tUserWorkgroups As List(Of clsUserWorkgroup) = Nothing, Optional fLoadRoleGroups As Boolean = False, Optional fGetListForViewAllLicense As Boolean = False) As List(Of clsWorkgroup) ' D1644 + D3288
            Dim _AvailableWorkgroups As New List(Of clsWorkgroup)
            If tUser IsNot Nothing AndAlso tUser Is ActiveUser AndAlso _ActiveAvailableWorkgroups IsNot Nothing AndAlso (Not fLoadRoleGroups OrElse (_ActiveAvailableWorkgroups.Count > 0 AndAlso _ActiveAvailableWorkgroups(0).RoleGroups.Count > 0)) Then Return _ActiveAvailableWorkgroups ' D4659 + D4684
            If Not tUser Is Nothing Then
                Dim _AllWorkgroups As List(Of clsWorkgroup) = DBWorkgroupsAll(fLoadRoleGroups, fLoadRoleGroups)   ' D1644 + D2935
                If tUserWorkgroups Is Nothing Then tUserWorkgroups = DBUserWorkgroupsByUserID(tUser.UserID)
                Dim fNeedViewAllWorkgroups As Boolean = (fGetListForViewAllLicense AndAlso CanUserDoSystemWorkgroupAction(ecActionType.at_slViewLicenseAnyWorkgroup, tUser.UserID)) ' D3288
                ' D3289 ===
                If fNeedViewAllWorkgroups Then
                    Dim SystemUWG As clsUserWorkgroup = clsUserWorkgroup.UserWorkgroupByUserIDAndWorkgroupID(ActiveUser.UserID, SystemWorkgroup.ID, SystemUserWorkgroups) ' D0502
                    If SystemUWG Is Nothing OrElse (SystemUWG.ExpirationDate.HasValue AndAlso SystemUWG.ExpirationDate.Value < Now) Then fNeedViewAllWorkgroups = False ' D3295
                End If
                Dim isSystemValid As Boolean = SystemWorkgroup IsNot Nothing AndAlso SystemWorkgroup.License.isValidLicense AndAlso SystemWorkgroup.License.CheckParameterByID(ecLicenseParameter.ExpirationDate) AndAlso SystemWorkgroup.License.CheckParameterByID(ecLicenseParameter.InstanceID) ' D3952
                If Not isSystemValid AndAlso Not fGetListForViewAllLicense Then fNeedViewAllWorkgroups = False ' D3952
                ' D3289 ==
                For Each tWG As clsWorkgroup In _AllWorkgroups
                    Dim tUW As clsUserWorkgroup = clsUserWorkgroup.UserWorkgroupByUserIDAndWorkgroupID(tUser.UserID, tWG.ID, tUserWorkgroups)
                    If tUser.CannotBeDeleted OrElse fNeedViewAllWorkgroups OrElse (tUW IsNot Nothing AndAlso (Not tUW.ExpirationDate.HasValue OrElse tUW.ExpirationDate.Value > Now)) Then  ' D3288 + D3289 + D3295
                        If isWorkgroupAvailable(tWG, tUser, tUW) OrElse fNeedViewAllWorkgroups Then
                            If isSystemValid OrElse (fGetListForViewAllLicense AndAlso tUser.CannotBeDeleted) OrElse tWG.Status = ecWorkgroupStatus.wsSystem Then _AvailableWorkgroups.Add(tWG) ' D3288 + D3952
                        End If
                    End If
                Next
            End If
            If tUser IsNot Nothing AndAlso tUser Is ActiveUser Then _ActiveAvailableWorkgroups = _AvailableWorkgroups  ' D4659
            Return _AvailableWorkgroups
        End Function
        ' D0474 ==

        ' D4984 ===
        Public Sub ResetWorkgroupsList()
            ' D7676 ===
            Dim WkgID As Integer = If(_CurrentWorkgroup Is Nothing, -1, _CurrentWorkgroup.ID)
            _ActiveAvailableWorkgroups = Nothing
            If WkgID > 0 Then
                _CurrentWorkgroup = Nothing
                SetActiveWorkgroup(clsWorkgroup.WorkgroupByID(WkgID, AvailableWorkgroups(ActiveUser)))
            End If
            ' D7676 ==
        End Sub
        ' D4984 ==

        ' D6634 ===
        Public Function AvailableWorkgroupsAsWM() As List(Of clsWorkgroup)
            Dim tLst As New List(Of clsWorkgroup)
            If isAuthorized Then
                Dim WkgList As List(Of clsWorkgroup) = AvailableWorkgroups(ActiveUser, UserWorkgroups, True)
                If CanUserDoSystemWorkgroupAction(ecActionType.at_slManageAnyWorkgroup, ActiveUser.UserID) Then
                    tLst = WkgList
                Else
                    For Each tWkg As clsWorkgroup In WkgList
                        Dim tUW As clsUserWorkgroup = clsUserWorkgroup.UserWorkgroupByUserIDAndWorkgroupID(ActiveUser.UserID, tWkg.ID, UserWorkgroups)
                        If tUW IsNot Nothing Then
                            If CanUserDoAction(ecActionType.at_slViewOwnWorkgroupLogs, tUW, tWkg) OrElse CanUserDoAction(ecActionType.at_slViewOwnWorkgroupReports, tUW, tWkg) OrElse CanUserDoAction(ecActionType.at_slManageOwnWorkgroup, tUW, tWkg) OrElse CanUserDoAction(ecActionType.at_alManageAnyModel, tUW, tWkg) Then tLst.Add(tWkg)  ' D7269
                        End If
                    Next
                End If
            End If
            Return tLst
        End Function
        ' D6634 ==

        ' D0589 ===
        Public Function GetMessageByLockStatus(ByVal tLockInfo As clsProjectLockInfo, ByVal tProject As clsProject, ByVal tCurrentUserID As Integer, ByVal fUseStyle As Boolean) As String
            Dim sPrjLock = ""
            If tLockInfo IsNot Nothing AndAlso tProject IsNot Nothing Then
                Select Case tLockInfo.LockStatus
                    Case ECLockStatus.lsUnLocked
                        sPrjLock = ResString(CStr(IIf(tProject.ProjectStatus <> ecProjectStatus.psActive, "lblReadOnly", "lblProjectNotLocked")))
                    Case ECLockStatus.lsLockForAntigua
                        sPrjLock = ResString("lblProjectLockedForAntigua")
                        If fUseStyle Then sPrjLock = String.Format("<span style='color:#336633'>{0}</span>", sPrjLock)
                    Case ECLockStatus.lsLockForModify
                        If tLockInfo.LockerUserID = tCurrentUserID Then
                            sPrjLock = ResString("lblProjectLockedByMe")
                            If fUseStyle Then sPrjLock = String.Format("<span style='color:#336633'>{0}</span>", sPrjLock)
                        Else
                            sPrjLock = String.Format(ResString("lblProjectLockedBy"), HTMLUserLinkEmail(DBUserByID(tLockInfo.LockerUserID)))
                            If fUseStyle Then sPrjLock = String.Format("<span class='error'>{0}</span>", sPrjLock)
                        End If
                    Case ECLockStatus.lsLockForSystem
                        sPrjLock = ResString("lblProjectLockedBySystem")
                        If fUseStyle Then sPrjLock = String.Format("<span style='color:#339933'>{0}</span>", sPrjLock)
                End Select
            End If
            If tProject.isTeamTime Or (tLockInfo IsNot Nothing AndAlso tLockInfo.LockStatus = ECLockStatus.lsLockForTeamTime) Then  ' D0760
                sPrjLock = ResString("lblSynchronousProject")
                If fUseStyle Then sPrjLock = String.Format("<span style='color:#336633'>{0}</span>", sPrjLock) ' D3863
            End If
            Return sPrjLock
        End Function
        ' D0589 ==

        ' D2413 ===
        Public Function GetPswReminderBodyText(sBodyMsg As String, fIsPM As Boolean, fIsAdmin As Boolean) As String ' D7327
            Return If(fIsAdmin, ResString("bodyPswReminderAdmin"), String.Format("{0}" + vbCrLf + vbCrLf + "{1}", IIf(fIsPM, ResString("bodyPswReminderPM"), ResString("bodyPswReminderUser")), sBodyMsg))  ' D7327
        End Function
        ' D2413 ==

        ' D0214 ===
        Public Function GetMessageByAuthErrorCode(ByVal tErrCode As ecAuthenticateError, Optional ByVal sPasscode As String = "") As String
            Dim sErrorMessage As String = ""
            ' D1207 ===
            If sPasscode <> "" AndAlso (tErrCode = ecAuthenticateError.aeProjectLocked OrElse tErrCode = ecAuthenticateError.aeWrongPasscode OrElse tErrCode = ecAuthenticateError.aeWorkspaceLocked OrElse tErrCode = ecAuthenticateError.aeProjectReadOnly) Then  ' D1737 + D2928
                Dim tmpPrj As clsProject = DBProjectByPasscode(sPasscode)
                If tmpPrj IsNot Nothing Then sPasscode = ShortString(tmpPrj.ProjectName, 30, True)
            End If
            ' D1207 ==
            Select Case tErrCode
                ' D2213 ===
                'Case ecAuthenticateError.aeNoUserFound
                '    sErrorMessage = ResString("msgWrongAuthInfo")
                'Case ecAuthenticateError.aeWrongPassword
                '    sErrorMessage = ResString("msgWrongPassword")
                Case ecAuthenticateError.aeNoUserFound, ecAuthenticateError.aeWrongPassword
                    sErrorMessage = ResString("msgWrongEmailOrPassword")
                    ' D2213 ==
                Case ecAuthenticateError.aeWrongPasscode
                    sErrorMessage = ResString("msgWrongPasscode")
                Case ecAuthenticateError.aeNoProjectsFound
                    sErrorMessage = ResString("msgNoProjectsFound")
                Case ecAuthenticateError.aeGroupLocked
                    sErrorMessage = ResString("msgAuthGroupDisabled")
                Case ecAuthenticateError.aeProjectLocked
                    sErrorMessage = String.Format(ResString("msgAuthProjectDisabled"), sPasscode)
                Case ecAuthenticateError.aeWorkgroupLocked
                    sErrorMessage = ResString("msgAuthWorkgroupDisabled")
                Case ecAuthenticateError.aeWorkspaceLocked
                    sErrorMessage = String.Format(ResString("msgAuthWorkspaceDisabled"), sPasscode)   ' D1518 + D1737
                Case ecAuthenticateError.aeUserLocked
                    sErrorMessage = String.Format(ResString("msgAuthUserDisabled"), sPasscode)
                Case ecAuthenticateError.aeUserLockedByWrongPsw ' D2213
                    sErrorMessage = String.Format(ResString(If(_DEF_PASSWORD_ATTEMPTS_AUTOUNLOCK, "msgUserLockedByWrongPswTimeout", "msgUserLockedByWrongPsw")), ResString("mnuForgottenPassword"))    ' D2213 + D6074
                'Case ecAuthenticateError.aeNoWorkgroupSelected
                '    sErrorMessage = ResString("errLoadingWorkgroups")
                Case ecAuthenticateError.aeNoSynchronousProject
                    sErrorMessage = ResString("msgAuthNoSynchronousProject")
                Case ecAuthenticateError.aeWrongLicense ' D0263
                    If ApplicationError.Status = ecErrorStatus.errWrongLicense And ApplicationError.Message <> "" Then sErrorMessage = ApplicationError.Message Else sErrorMessage = ResString("msgAuthWrongLicense") ' D0263 + D0348
                    ' D0390 ===
                Case ecAuthenticateError.aeWrongMeetingID
                    sErrorMessage = ResString("msgAuthWrongMeetingID")
                Case ecAuthenticateError.aeNoSynchronousStarted
                    sErrorMessage = ResString("msgAuthInactiveSynchronousProject")
                Case ecAuthenticateError.aeUseRegularLogon
                    sErrorMessage = String.Format(ResString("msgAuthUseRegularLogon"), "SwitchMeeting(1);")
                Case ecAuthenticateError.aeMeetingIDNotAllowed
                    sErrorMessage = ResString("msgAuthMeetingIDNotAllowed")
                    ' D0390 ==
                Case ecAuthenticateError.aeUserWorkgroupExpired  ' D0429 + D0471
                    sErrorMessage = ResString("msgAuthUserWorkspaceExpired")    ' D0429
                Case ecAuthenticateError.aeWrongCredentials ' D4841
                    sErrorMessage = ResString("errInvalidHash") ' D4841
                Case ecAuthenticateError.aeSynchronousFull
                    sErrorMessage = String.Format(ResString("msgAuthSynchronousFull"), Options.SynchronousMaxUsers)  ' D0450 + D0459
                    ' -D2038
                Case ecAuthenticateError.aeSynchronousUserNotAllowed    ' D0659
                    sErrorMessage = ResString("msgAuthSynchronousUserNotAllowed")   ' D0659
                Case ecAuthenticateError.aeDeletedProject   ' D1601
                    sErrorMessage = ResString("msgAuthDeletedProject")   ' D1601
                Case ecAuthenticateError.aePasscodeNotAllowed   ' D1726
                    sErrorMessage = ResString("msgDisabledPasscode")   ' D1726
                Case ecAuthenticateError.aeProjectReadOnly  ' D2489
                    sErrorMessage = String.Format(ResString("msgAuthProjectReadOnly"), sPasscode)   ' D2489
                Case ecAuthenticateError.aeUserWorkgroupLocked  ' D2618
                    sErrorMessage = ResString("msgUserWorkgroupLocked") ' D2618
                Case ecAuthenticateError.aeEvaluationNotAllowed  ' D2808
                    sErrorMessage = ResString("msgEvaluationNotAllowed") ' D2808
                    ' D3303 ===
                Case ecAuthenticateError.aeWorkgroupExpired
                    sErrorMessage = ResString("msgWorkgroupExpired")
                Case ecAuthenticateError.aeWorkgroupExpiredEval
                    sErrorMessage = ResString("msgWorkgroupExpiredEval")
                    ' D3303 ==
                Case ecAuthenticateError.aeSystemWorkgroupExpired   ' D4982
                    sErrorMessage = ResString("msgSystemWorkgroupExpired")  ' D4982
                    ' D3952 ===
                Case ecAuthenticateError.aeWrongInstanceID
                    sErrorMessage = String.Format(ResString("errLicenseWrongInstanceID"), GetInstanceID_AsString)
                    ' D3952 ==
                Case ecAuthenticateError.aeTotalWorkgroupsLimit   ' D6568
                    sErrorMessage = ResString("msgTotalWorkgroupsLimitExceeded")  ' D6568
                Case ecAuthenticateError.aeLocalhostAllowedOnly   ' D6640
                    sErrorMessage = ResString("msgLocalhostLoginOnly")      ' D6640
                Case ecAuthenticateError.aeMFA_Required
                    sErrorMessage = ResString("msgMFARequired")             ' D7502
                Case Else   ' D0046
                    sErrorMessage = String.Format("{0}: {1}", ResString("msgAuthResult"), tErrCode.ToString)
            End Select
            Return sErrorMessage
        End Function
        ' D0214 ==

        ' D0473 ===
        Public Function CheckUserEULA(ByVal tUserWorkgroup As clsUserWorkgroup) As Boolean
            If tUserWorkgroup Is Nothing OrElse ActiveWorkgroup Is Nothing Then Return True     ' D6465 // for avoid to ask the EULA when no logged properly
            If ActiveWorkgroup.Status = ecWorkgroupStatus.wsSystem Then Return True
            'If Not CanUserDoAction(ecActionType.at_alCreateNewModel, tUserWorkgroup) AndAlso Not CanUserDoAnySystemWorkgroupAction(tUser, tUserWorkgroupID) And Not CanDoAnyAdministrativeAction(tUser, tUserWorkgroupID) Then Return True ' D0323
            If Not CanUserDoAction(ecActionType.at_alCreateNewModel, tUserWorkgroup) Then Return True ' D0473
            Dim fValid As Boolean = _EULA_valid
            If Not _EULA_checked Then
                If Options.CheckEULA AndAlso CanvasMasterDBVersion >= "0.92" Then   ' D0515
                    Dim sUserEULA As String = ""    ' D0919
                    Dim tEULA As Object = Database.ExecuteScalarSQL(String.Format("SELECT EULAversion FROM {0} WHERE {1}={2}", _TABLE_USERS, _FLD_USERS_ID, tUserWorkgroup.UserID))
                    If Not IsDBNull(tEULA) Then sUserEULA = CStr(tEULA) ' D0919
                    fValid = sUserEULA >= _EULA_REVISION ' D0919 + D6465
                    ' D0919 ===
                    Dim sWkgEULA As String = ActiveWorkgroup.EULAFile   ' D0922
                    If sWkgEULA <> "" Then
                        Dim sUWEULA As String = DBUserWorkgroupEULAVersion(tUserWorkgroup.WorkgroupID, tUserWorkgroup.UserID)
                        fValid = sWkgEULA >= sUWEULA OrElse sWkgEULA >= sUserEULA   ' D6465
                    End If
                    ' D0919 ==
                Else
                    fValid = True
                End If
                _EULA_valid = fValid
                _EULA_checked = True
            End If
            Return fValid
        End Function
        ' D0473 ==

        ' D0529 ==
        Public Property SilverLightChecker() As clsExtra
            Get
                If _SilverLightChecked Is Nothing Then
                    _SilverLightChecked = New clsExtra
                    _SilverLightChecked.ExtraProperty = ecExtraProperty.Version
                    _SilverLightChecked.ExtraType = ecExtraType.SilverLight
                    _SilverLightChecked.ObjectID = -1
                    If ActiveUser IsNot Nothing Then _SilverLightChecked.ObjectID = ActiveUser.UserID ' D0530
                    Dim Res As clsExtra = DBExtraRead(_SilverLightChecked)
                    If Res IsNot Nothing Then _SilverLightChecked = Res
                End If
                Return _SilverLightChecked
            End Get
            Set(ByVal value As clsExtra)
                _SilverLightChecked = value
            End Set
        End Property
        ' D0529 ==

        ' D6657 ===
        Public Function GetPswComplexityPattern() As String
            Dim sPattern As String = If(_DEF_PASSWORD_COMPLEXITY, "aB1$cd2Efg@hij3Klm#Nop!Qrs4Tuv$wX5yz6#A1$Bcd2Efg@-", "abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyz")
            Return sPattern.Substring(0, If(_DEF_PASSWORD_COMPLEXITY AndAlso _DEF_PASSWORD_MIN_LENGTH > 2, _DEF_PASSWORD_MIN_LENGTH - 1, 1)) ' D7128 + D7327
        End Function

        Public Function GetUserPswHashes(tUser As clsApplicationUser) As SortedDictionary(Of String, String)
            Dim Lst As New SortedDictionary(Of String, String)
            If tUser IsNot Nothing Then
                Dim tExtra As clsExtra = DBExtraRead(clsExtra.Params2Extra(tUser.UserID, ecExtraType.User, ecExtraProperty.UserPswHashesJsonData))
                If tExtra IsNot Nothing Then
                    If tExtra.Value IsNot Nothing Then
                        Dim jss As New JavaScriptSerializer()
                        Lst = jss.Deserialize(Of SortedDictionary(Of String, String))(CStr(tExtra.Value))
                        ' D6658 ===
                        If tUser.HasPassword Then
                            Dim tLastPsw As Date = If(tUser IsNot Nothing AndAlso tUser.LastModify.HasValue, tUser.LastModify.Value, If(tUser IsNot Nothing AndAlso tUser.Created.HasValue, tUser.Created.Value, Now))
                            Dim sCurMD5 As String = GetMD5(tUser.UserPassword)
                            If Lst.Count = 0 OrElse Lst.Last().Value <> sCurMD5 AndAlso Not Lst.ContainsKey(Date2ULong(tLastPsw).ToString) Then Lst.Add(Date2ULong(tLastPsw).ToString, sCurMD5) ' D7126
                        End If
                        ' D6658 ==
                    End If
                End If
            End If
            Return Lst
        End Function

        Public Sub SetUserPswHashes(UserID As Integer, value As SortedDictionary(Of String, String))
            Dim sValue As String = ""
            Dim jss As New JavaScriptSerializer()
            If value IsNot Nothing Then
                If value.Count > _DEF_PASSWORD_HASHES_MAX Then
                    Dim tmp As New SortedDictionary(Of String, String)
                    For i As Integer = value.Count - _DEF_PASSWORD_HASHES_MAX To value.Count - 1
                        With value.ElementAt(i)
                            tmp.Add(.Key, .Value)
                        End With
                    Next
                    value = tmp
                End If
                sValue = jss.Serialize(value)
            End If
            DBExtraWrite(clsExtra.Params2Extra(UserID, ecExtraType.User, ecExtraProperty.UserPswHashesJsonData, sValue))
        End Sub

        Public Function ValidatePswComplexity(sPassword As String, fAllowBlankPsw As Boolean, Optional tUser As clsApplicationUser = Nothing, Optional sOldPassword As String = Nothing) As List(Of String)    ' D6658 + D7719
            Dim sResult As New List(Of String)
            If Not fAllowBlankPsw Then  ' D7719
                If sPassword = "" Then
                    sResult.Add(ResString("msgBlankPsw"))
                Else
                    If _DEF_PASSWORD_COMPLEXITY Then
                        If _DEF_PASSWORD_MIN_LENGTH > 0 AndAlso sPassword.Length < _DEF_PASSWORD_MIN_LENGTH Then sResult.Add(String.Format(ResString("msgPswTooShort"), _DEF_PASSWORD_MIN_LENGTH))
                        If _DEF_PASSWORD_MAX_LENGTH > 0 AndAlso _DEF_PASSWORD_MAX_LENGTH > _DEF_PASSWORD_MIN_LENGTH AndAlso sPassword.Length > _DEF_PASSWORD_MAX_LENGTH Then sResult.Add(String.Format(ResString("msgPswTooLong"), _DEF_PASSWORD_MAX_LENGTH))
                        ' D7305 ===
                        Dim upperCnt As Integer = 0
                        Dim lowerCnt As Integer = 0
                        Dim digitCnt As Integer = 0
                        Dim specialCnt As Integer = 0
                        For Each sChar As Char In sPassword
                            If Char.IsDigit(sChar) Then
                                digitCnt += 1
                            Else
                                If Char.IsLetter(sChar) Then
                                    If Char.IsUpper(sChar) Then
                                        upperCnt += 1
                                    Else
                                        If Char.IsLower(sChar) Then
                                            lowerCnt += 1
                                        End If
                                    End If
                                Else
                                    specialCnt += 1
                                End If
                            End If
                        Next
                        If upperCnt = 0 OrElse lowerCnt = 0 OrElse digitCnt = 0 OrElse specialCnt = 0 Then
                            sResult.Add(ResString("msgPswComplexity"))
                        End If

                        ' D7305 ==
                        'If String.IsNullOrEmpty(sOldPassword) AndAlso tUser IsNot Nothing Then sOldPassword = tUser.UserPassword
                        If sOldPassword IsNot Nothing Then  ' D7305 // check only when change the psw: old is specified
                            If _DEF_PASSWORD_MIN_CHANGES > 0 Then
                                'Dim cntDiff As Integer = Math.Abs(sOldPassword.Length - sPassword.Length)
                                'If cntDiff < _DEF_PASSWORD_MIN_CHANGES Then
                                '    Dim cnt As Integer = If(sOldPassword.Length < sPassword.Length, sOldPassword, sPassword).Length
                                '    For i As Integer = 0 To cnt - 1
                                '        If sOldPassword(i) <> sPassword(i) Then cntDiff += 1
                                '    Next
                                'End If
                                Dim cntDiff As Integer = Levenshtein_Distance(sOldPassword, sPassword)  ' D7132
                                If cntDiff < _DEF_PASSWORD_MIN_CHANGES Then sResult.Add(String.Format(ResString("msgPswClose"), _DEF_PASSWORD_MIN_CHANGES))
                            Else
                                If sOldPassword = sPassword Then sResult.Add(ResString("msgSamePsw"))
                            End If
                            If _DEF_PASSWORD_KEEP_HASHES > 0 AndAlso tUser IsNot Nothing Then   ' D6658 + D7305
                                Dim History As SortedDictionary(Of String, String) = GetUserPswHashes(tUser)
                                Dim sCurPswMD5 As String = GetMD5(sPassword)
                                Dim cnt As Integer = History.Count
                                If cnt > _DEF_PASSWORD_KEEP_HASHES Then cnt = _DEF_PASSWORD_KEEP_HASHES
                                For i As Integer = 1 To cnt
                                    If History.ElementAt(History.Count - i).Value = sCurPswMD5 Then
                                        sResult.Add(String.Format(ResString("msgPswUsedBefore"), _DEF_PASSWORD_KEEP_HASHES))
                                        Exit For
                                    End If
                                Next
                            End If
                        End If
                    End If
                End If
            End If
            Return sResult
        End Function
        ' D6657 ==

        ' D6659 ===
        Public Function SetUserPassword(ByRef tUser As clsApplicationUser, sPassword As String, fKeepHashes As Boolean, fAllowBlankPsw As Boolean, ByRef sError As String) As Boolean  ' D7719
            Dim fResult As Boolean = False
            If tUser Is Nothing Then
                sError = "Can't find the specific user"
            Else
                If tUser.UserPassword = sPassword AndAlso Not _DEF_PASSWORD_COMPLEXITY Then    ' D7137
                    'If Not fAllowBlankPsw OrElse sPassword <> "" Then   ' D7133
                    '    sError = "Nothing to change: the same password"   ' D6397
                    'Else
                    fResult = True  ' D7133
                    'End If
                Else
                    Dim tCheck As List(Of String) = ValidatePswComplexity(sPassword, fAllowBlankPsw, tUser, tUser.UserPassword) ' D7719
                    If tCheck IsNot Nothing AndAlso tCheck.Count > 0 Then
                        sError = String.Format("Unable to save password:<br> {0}", String.Join("; ", tCheck))
                    Else
                        If sPassword <> "" Then
                            Dim UserHashes As SortedDictionary(Of String, String) = GetUserPswHashes(tUser)
                            UserHashes.Add(Date2ULong(Now).ToString, GetMD5(sPassword))
                            SetUserPswHashes(tUser.UserID, UserHashes)
                        End If
                        tUser.UserPassword = sPassword
                        If (tUser.PasswordStatus > 0) Then tUser.PasswordStatus = 0
                        If Not fKeepHashes Then ' D7137
                            DBTinyURLDelete(-1, -2, tUser.UserID) ' D7137
                        End If
                        fResult = DBUserUpdate(tUser, False, String.Format("Set user password. Keep hashes: {0}", Bool2YesNo(fKeepHashes)))    ' D7719
                        If fResult Then
                            Dim DT As String = Date2ULong(Now).ToString()
                            DBExtraWrite(clsExtra.Params2Extra(tUser.UserID, ecExtraType.User, ecExtraProperty.UserSessionTerminate, DT)) ' D7356
                        End If
                    End If
                End If
            End If
            Return fResult
        End Function
        ' D6659 ==

        ' D7536 ===
        Public Function CheckSessionTerminate(tUser As clsApplicationUser) As DateTime?
            Dim TerminateDT As Date? = Nothing
            If tUser IsNot Nothing Then
                Dim tSess As clsExtra = DBExtraRead(clsExtra.Params2Extra(tUser.UserID, ecExtraType.User, ecExtraProperty.UserSessionTerminate))
                If tSess IsNot Nothing Then
                    TerminateDT = Now
                    Dim DT As Date = Now.AddSeconds(1)
                    If tSess.Value IsNot Nothing Then
                        Dim DLng As Long
                        If Long.TryParse(CStr(tSess.Value), DLng) Then
                            DT = Date.FromBinary(DLng)
                            TerminateDT = DT
                            If Now > DT.AddSeconds(_DEF_SESS_TIMEOUT) Then
                                DBExtraDelete(tSess)
                                TerminateDT = Nothing
                            End If
                        End If
                    End If
                End If
            End If
            Return TerminateDT
        End Function
        ' D7536 ==

        ' D7502 ===
        Public Function GetMFA_Mode(Optional tUser As clsApplicationUser = Nothing) As ecPinCodeType
            ' TODO: check user MFA settings there
            If tUser IsNot Nothing Then
                '...
            End If
            ' Check Site settings for MFA:
            If _MFA_REQUIRED Then
                If _MFA_EMAIL_ALLOWED Then Return ecPinCodeType.mfaEmail
            End If
            Return ecPinCodeType.mfaNone
        End Function
        ' D7502 ==

    End Class

End Namespace