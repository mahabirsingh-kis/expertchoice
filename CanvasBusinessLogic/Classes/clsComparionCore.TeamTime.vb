Imports ECCore.ECTypes
Imports Canvas
Imports ECSecurity.ECSecurity

Namespace ExpertChoice.Data

    Partial Public Class clsComparionCore


        Public ReadOnly Property isTeamTimeAvailable() As Boolean
            Get
                If Not _TEAMTIME_AVAILABLE Then Return False ' D0494
                Dim fPassed As Boolean = False
                Dim SysWG As clsWorkgroup = SystemWorkgroup
                If Options.CheckLicense And Not SysWG Is Nothing Then    ' D0315
                    fPassed = SysWG.License.CheckParameterByID(ecLicenseParameter.TeamTimeEnabled, Nothing, True)   ' D0913
                    If fPassed And Not ActiveWorkgroup Is Nothing And Not SysWG Is ActiveWorkgroup Then
                        fPassed = ActiveWorkgroup.License.CheckParameterByID(ecLicenseParameter.TeamTimeEnabled, Nothing, True) ' D0913
                    End If
                End If
                Return fPassed
            End Get
        End Property

        ' D0487 ===
        Public Function TeamTimeSessionStep(ByRef tProject As clsProject, ByRef tUser As clsApplicationUser) As Integer
            If tProject Is Nothing Or tUser Is Nothing Then Return -1
            If Not tProject.isTeamTime Then Return -1
            Dim tWS As clsWorkspace = clsWorkspace.WorkspaceByUserIDAndProjectID(tProject.MeetingOwnerID, tProject.ID, Workspaces)  ' D0504
            If tWS Is Nothing Then tWS = DBWorkspaceByUserIDProjectID(tProject.MeetingOwnerID, tProject.ID) ' D0504
            If tWS Is Nothing Then Return -1 Else Return tWS.ProjectStep(tProject.isImpact) ' D1945
        End Function
        ' D0487 ==

        ''' <summary>
        ''' Set project as synchronous session
        ''' </summary>
        ''' <param name="tProject">Set status isSynchronous for this Project</param>
        ''' <param name="SessionUsers">list of clsApplicationUsers, who are included in this session</param>
        ''' <returns>True then successful</returns>
        ''' <remarks></remarks>
        Public Function TeamTimeStartSession(ByRef tMeetingOwner As clsApplicationUser, ByRef tProject As clsProject, tHierarchyID As ECHierarchyID, ByRef SessionUsers As List(Of clsApplicationUser), ByRef SessionsWorkspaces As List(Of clsWorkspace), Optional fSetProjectOnline As Boolean = True, Optional AppID As ecAppliationID = ecAppliationID.appUnknown, Optional isConsensusView As Boolean = False) As Boolean  ' D0383 + D0506 + D0510 + D1734 + D3207 + D4274 + D6553
            Dim fResult As Boolean = False
            If tMeetingOwner IsNot Nothing AndAlso tProject IsNot Nothing AndAlso tProject.isValidDBVersion AndAlso SessionUsers IsNot Nothing AndAlso Workspaces IsNot Nothing Then   ' D1454
                If tProject.LockInfo IsNot Nothing AndAlso tProject.LockInfo.isLockAvailable(tMeetingOwner.UserID) Then DBProjectLockInfoWrite(ECLockStatus.lsLockForTeamTime, tProject.LockInfo, tMeetingOwner, Now.AddSeconds(If(isConsensusView, _DEF_LOCK_TT_CONSENSUS_VIEW, _DEF_LOCK_TT_SESSION_TIMEOUT))) ' D0506 + D0589 + D1074 + D1454 + D6553
                ' D0226 ===
                If fSetProjectOnline AndAlso Not tProject.isOnline Then   ' D0300 + D0748 + D3207
                    tProject.isOnline = True    ' D0300 + D0748
                    If tProject.PipeParameters.TeamTimeAllowMeetingID Then tProject.isPublic = True ' D1219
                    DBProjectUpdate(tProject, False, "Make project On-line for start TeamTime")
                End If
                ' D0226 ==

                ' D0516 ===
                For Each tWS As clsWorkspace In Workspaces
                    If tWS.isInTeamTime(tProject.isImpact) AndAlso tWS.ProjectID <> tProject.ID Then    ' D1945
                        Dim tPrj As clsProject = clsProject.ProjectByID(tWS.ProjectID, ActiveProjectsList)
                        If Not tPrj Is Nothing AndAlso tPrj.ID <> ProjectID Then
                            If tPrj.MeetingStatus(tMeetingOwner) = ECTeamTimeStatus.tsTeamTimeSessionOwner Then
                                TeamTimeEndSession(tPrj, False)
                            Else
                                tWS.Status(tProject.isImpact) = ecWorkspaceStatus.wsEnabled ' D1945
                                tWS.TeamTimeStatus(tProject.isImpact) = ecWorkspaceStatus.wsUnknown    ' D0660 + D1945
                                DBWorkspaceUpdate(tWS, False, String.Format("Detach from TeamTime session '{0}' by request", clsMeetingID.AsString(tPrj.MeetingID)))
                            End If
                        End If
                    End If
                Next
                ' D0516 ==

                ' D0754 ===
                For Each tPrj As clsProject In ActiveProjectsList
                    If tPrj.ID <> tProject.ID AndAlso tPrj.isTeamTime AndAlso tPrj.MeetingOwnerID = tMeetingOwner.UserID Then
                        TeamTimeEndSession(tPrj, False)
                    End If
                Next
                ' D0754 ==

                'If Not tProject.isTeamTime Or tProject.MeetingOwnerID <> tMeetingOwner.UserID Or Not tProject.ProjectManager.PipeBuilder.IsSynchronousSession Then   ' D0506 + D0520
                'If Not tProject.isTeamTime Or tProject.MeetingOwnerID <> tMeetingOwner.UserID Then   ' D0506 + D0519
                Dim fProjectUpdate As Boolean = Not tProject.isTeamTime Or tProject.MeetingOwnerID <> tMeetingOwner.UserID  ' D0522
                tProject.isTeamTime = True
                tProject.MeetingOwner = tMeetingOwner   ' D0506
                tProject.ProjectManager.PipeBuilder.PipeCreated = False ' D0440
                If tHierarchyID = ECHierarchyID.hidImpact Then tProject.ProjectManager.ActiveHierarchy = ECHierarchyID.hidImpact ' D1734
                If tHierarchyID = ECHierarchyID.hidLikelihood Then tProject.ProjectManager.ActiveHierarchy = ECHierarchyID.hidLikelihood ' D1734
                'tProject.ProjectManager.PipeBuilder.IsSynchronousSession = True     ' D0506 -D0522
                'tProject.ProjectManager.PipeParameters.CurrentParameterSet = tProject.ProjectManager.PipeParameters.ImpactParameterSet    ' D0520 - D0522
                If fProjectUpdate Then
                    DBTeamTimeDataDelete(tProject.ID, Nothing)   ' D1275
                    fResult = DBProjectUpdate(tProject, False, "Start TeamTime session")
                Else
                    fResult = True ' D0506 + D0522
                End If
                'End If

                ' -D2164
                '' D0506 ===
                'Dim sSQL As String = String.Format("SELECT * FROM Workspace WHERE Status IN ({0},{1}) AND ProjectID<>{2}", CInt(ecWorkspaceStatus.wsSynhronousActive), CInt(ecWorkspaceStatus.wsSynhronousReadOnly), tProject.ID)
                'Dim tList As List(Of Dictionary(Of String, Object)) = Database.SelectBySQL(sSQL)
                'Dim TTWSList As New List(Of clsWorkspace)
                'If tList IsNot Nothing Then ' D1454
                '    For Each tRow As Dictionary(Of String, Object) In tList
                '        TTWSList.Add(DBParse_Workspace(tRow))
                '    Next
                'End If

                'For Each tWS In clsWorkspace.WorkspacesByProjectID(tProject.ID, TTWSList)
                '    If tWS.isInTeamTime AndAlso clsApplicationUser.UserByUserID(tWS.UserID, SessionUsers) Is Nothing Then
                '        tWS.Status = ecWorkspaceStatus.wsEnabled
                '        tWS.TeamTimeStatus = ecWorkspaceStatus.wsUnknown    ' D0660
                '        DBWorkspaceUpdate(tWS, False, "Detach from TeamTime session")
                '    End If
                'Next
                '' D0506  ==

                ' D4369 ===
                Dim EvalGrpIDs As New List(Of Integer)
                For Each tGrp As clsRoleGroup In ActiveWorkgroup.RoleGroups
                    If tGrp.ActionStatus(ecActionType.at_mlEvaluateModel) = ecActionStatus.asGranted Then EvalGrpIDs.Add(tGrp.ID)
                Next
                ' D4369 ==

                Dim WSReal As List(Of clsWorkspace) = DBWorkspacesByProjectID(tProject.ID)  ' D0506

                ' D0646 ===
                For Each tWS As clsWorkspace In WSReal
                    Dim tWSNew As clsWorkspace = clsWorkspace.WorkspaceByUserIDAndProjectID(tWS.UserID, tWS.ProjectID, SessionsWorkspaces)
                    Dim fNeedReset As Boolean = tWSNew Is Nothing
                    If tWSNew IsNot Nothing AndAlso tWS.isInTeamTime(tProject.isImpact) AndAlso Not tWSNew.isInTeamTime(tProject.isImpact) Then fNeedReset = True ' D1945
                    If Not EvalGrpIDs.Contains(tWS.GroupID) Then fNeedReset = True ' D4369
                    If fNeedReset Then
                        If tWS.Status(tProject.isImpact) <> ecWorkspaceStatus.wsDisabled Then tWS.Status(tProject.isImpact) = ecWorkspaceStatus.wsEnabled ' D1742 + D1945
                        tWS.TeamTimeStatusLikelihood = ecWorkspaceStatus.wsUnknown    ' D0660 + D1945
                        tWS.TeamTimeStatusImpact = ecWorkspaceStatus.wsUnknown    ' D1945
                        DBWorkspaceUpdate(tWS, False, "Exclude from TeamTime session")
                    End If
                Next
                ' D0646 ==

                For Each tUser As clsApplicationUser In SessionUsers
                    ' D0383 ===
                    Dim tWSCustom As clsWorkspace = clsWorkspace.WorkspaceByUserIDAndProjectID(tUser.UserID, tProject.ID, SessionsWorkspaces) ' D0383 + D0506
                    Dim tWSReal As clsWorkspace = clsWorkspace.WorkspaceByUserIDAndProjectID(tUser.UserID, tProject.ID, WSReal) ' D0383 + D0506
                    Dim tWSOrigReal As clsWorkspace = tWSReal.Clone
                    If tWSReal IsNot Nothing AndAlso EvalGrpIDs.Contains(tWSReal.GroupID) Then  ' D4369
                        If tWSReal IsNot Nothing AndAlso tWSCustom IsNot Nothing Then   ' D1454
                            ' D1945 ===
                            'If tWSCustom.isInTeamTime AndAlso Not tWSReal.isInTeamTime Then tWSReal.Status = ecWorkspaceStatus.wsSynhronousActive ' D0382
                            'If Not tWSReal.isInTeamTime Then tWSReal.Status = ecWorkspaceStatus.wsSynhronousActive ' D0382
                            If tWSCustom.TeamTimeStatus(tProject.isImpact) = ecWorkspaceStatus.wsEnabled Then tWSCustom.TeamTimeStatus(tProject.isImpact) = ecWorkspaceStatus.wsSynhronousActive ' D3154
                            tWSReal.Status(tProject.isImpact) = tWSCustom.TeamTimeStatus(tProject.isImpact)     ' D0660 + D2203
                            If Not tWSReal.isInTeamTime(tProject.isImpact) AndAlso tWSCustom.isInTeamTime(tProject.isImpact) Then tWSReal.TeamTimeStatus(tProject.isImpact) = tWSCustom.TeamTimeStatus(tProject.isImpact) ' D4153
                            If tWSReal.Status(tProject.isImpact) = ecWorkspaceStatus.wsEnabled OrElse tWSReal.Status(tProject.isImpact) = ecWorkspaceStatus.wsUnknown Then tWSReal.Status(tProject.isImpact) = tWSOrigReal.Status(tProject.isImpact) ' D2203
                            ' D1741 ===
                            If tWSCustom.Status(tProject.isImpact) = ecWorkspaceStatus.wsDisabled AndAlso tWSReal.isInTeamTime(tProject.isImpact) Then
                                tWSReal.TeamTimeStatus(tProject.isImpact) = ecWorkspaceStatus.wsUnknown
                                tWSReal.Status(tProject.isImpact) = ecWorkspaceStatus.wsDisabled
                            End If
                            ' D1741 + D1945 ==
                            If tWSOrigReal.isInTeamTime(tProject.isImpact) <> tWSReal.isInTeamTime(tProject.isImpact) Then fResult = fResult And DBWorkspaceUpdate(tWSReal, False, CStr(IIf(tWSReal.isInTeamTime(tProject.isImpact), "Attach to", "Detach from")) + " TeamTime session") ' D0296 + D0506 + D1741 + D2164
                            ' D0383 ==
                        End If
                    End If
                Next
                DBTeamTimeDataWrite(tProject.ID, tProject.MeetingOwnerID, ecExtraProperty.TeamTimeSessionPasscode, CStr(IIf(tHierarchyID = ECHierarchyID.hidImpact, tProject.PasscodeImpact, tProject.PasscodeLikelihood)), True) ' D1734
                DBTeamTimeDataWrite(tProject.ID, tProject.MeetingOwnerID, ecExtraProperty.TeamTimeSessionAppID, CInt(AppID).ToString) ' D4274
            End If
            If fResult Then Workspaces = Nothing ' D0544
            Return fResult
        End Function

        ' D1261 ===
        Public Function TeamTimeResumeSession(ByVal tMeetingOwner As clsApplicationUser, ByVal tProject As clsProject, tHierarchyID As ECHierarchyID, Optional ByVal tUsersList As List(Of clsApplicationUser) = Nothing, Optional ByVal tWorkspaces As List(Of clsWorkspace) = Nothing, Optional ByVal tOnlineSessions As List(Of clsOnlineUserSession) = Nothing, Optional fStartFromConsensus As Boolean = False, Optional tConsensusStep As Integer = -1, Optional AppID As ecAppliationID = ecAppliationID.appUnknown) As Boolean  ' D1734 + D3151 + D4274
            Dim fResult As Boolean = False
            If tMeetingOwner IsNot Nothing Then
                CheckProjectManagerUsers(tProject)
                If tUsersList Is Nothing Then tUsersList = DBUsersByProjectID(ProjectID)
                If tWorkspaces Is Nothing Then tWorkspaces = DBWorkspacesByProjectID(ProjectID)
                'If tOnlineSessions Is Nothing Then tOnlineSessions = DBOnlineSessions()    ' D3207

                ' D3151 ===
                Dim _TeamTime_Pipe As clsTeamTimePipe = Nothing
                If fStartFromConsensus Then
                    _TeamTime_Pipe = New clsTeamTimePipe(tProject.ProjectManager, tProject.ProjectManager.User)
                    _TeamTime_Pipe.Override_ResultsMode = True
                    _TeamTime_Pipe.ResultsViewMode = ResultsView.rvBoth
                    Dim TTUsersList As New List(Of clsUser)
                    For Each tAppUser As clsApplicationUser In tUsersList
                        Dim tPrjUser As clsUser = tProject.ProjectManager.GetUserByEMail(tAppUser.UserEmail)
                        If tPrjUser IsNot Nothing Then TTUsersList.Add(tPrjUser)
                    Next
                    _TeamTime_Pipe.VerifyUsers(TTUsersList)
                    _TeamTime_Pipe.CreatePipe()
                End If
                Dim tAction As clsAction = Nothing
                If _TeamTime_Pipe IsNot Nothing AndAlso tConsensusStep >= 0 AndAlso tConsensusStep < _TeamTime_Pipe.Pipe.Count Then tAction = CType(_TeamTime_Pipe.Pipe(tConsensusStep), clsAction)
                ' D3151 ==

                Dim tSessionUsers As New List(Of clsApplicationUser)   ' D2168
                Dim tSessionWorkspaces As New List(Of clsWorkspace)    ' D2168
                For Each tWS As clsWorkspace In tWorkspaces
                    Dim tPrjUser As clsApplicationUser = clsApplicationUser.UserByUserID(tWS.UserID, tUsersList)
                    Dim tAHPUser As clsUser = Nothing
                    If tPrjUser IsNot Nothing Then tAHPUser = tProject.ProjectManager.GetUserByEMail(tPrjUser.UserEmail)
                    ' D3151 ===
                    ' When started from Consensus view: add all participants, who has permissions for evaluate specified step:
                    If fStartFromConsensus AndAlso _TeamTime_Pipe IsNot Nothing AndAlso tAction IsNot Nothing Then
                        If (tWS.UserID = tMeetingOwner.UserID OrElse tWS.UserID = ActiveUser.UserID OrElse tWS.Status(tProject.isImpact) <> ecWorkspaceStatus.wsSynhronousReadOnly) AndAlso
                            _TeamTime_Pipe.GetStepAvailabilityForUser(tAction.ActionType, tAction, tAHPUser) Then
                            If tWS.TeamTimeStatus(tProject.isImpact) = ecWorkspaceStatus.wsDisabled OrElse tWS.TeamTimeStatus(tProject.isImpact) = ecWorkspaceStatus.wsUnknown Then
                                tWS.TeamTimeStatus(tProject.isImpact) = ecWorkspaceStatus.wsEnabled
                            End If
                            tWS.Status(tProject.isImpact) = tWS.TeamTimeStatus(tProject.isImpact)
                            tSessionWorkspaces.Add(tWS)
                            tSessionUsers.Add(tPrjUser)
                        End If
                    Else
                        ' D3151 ==
                        ' For regular TT session resume:
                        If tWS.Status(tProject.isImpact) <> ecWorkspaceStatus.wsDisabled AndAlso (tWS.UserID = tMeetingOwner.UserID OrElse (tProject.isTeamTime AndAlso tWS.isInTeamTime(tProject.isImpact)) OrElse (Not tProject.isTeamTime AndAlso tWS.Status(tProject.isImpact) <> ecWorkspaceStatus.wsDisabled) OrElse clsOnlineUserSession.OnlineSessionByUserID(tWS.UserID, tOnlineSessions) IsNot Nothing) Then ' D1741
                            If tWS.TeamTimeStatus(tProject.isImpact) <> ecWorkspaceStatus.wsUnknown AndAlso tWS.Status(tProject.isImpact) <> ecWorkspaceStatus.wsDisabled AndAlso Not tWS.isInTeamTime(tProject.isImpact) Then tWS.Status(tProject.isImpact) = tWS.TeamTimeStatus(tProject.isImpact) ' D1945
                            ' D2164 ===
                            If tPrjUser IsNot Nothing Then
                                If tAHPUser.SyncEvaluationMode = SynchronousEvaluationMode.semNone Then
                                    tWS.Status(tProject.isImpact) = ecWorkspaceStatus.wsEnabled
                                    tWS.TeamTimeStatus(tProject.isImpact) = ecWorkspaceStatus.wsUnknown
                                End If
                                ' D2168 + D2174 ===
                                If (tAHPUser.SyncEvaluationMode <> SynchronousEvaluationMode.semNone OrElse tWS.TeamTimeStatus(tProject.isImpact) <> ecWorkspaceStatus.wsUnknown) AndAlso tWS.Status(tProject.isImpact) <> ecWorkspaceStatus.wsDisabled Then
                                    If tWS.TeamTimeStatus(tProject.isImpact) = ecWorkspaceStatus.wsUnknown Then
                                        If tAHPUser.SyncEvaluationMode = SynchronousEvaluationMode.semByFacilitatorOnly Then tWS.TeamTimeStatus(tProject.isImpact) = ecWorkspaceStatus.wsSynhronousReadOnly Else tWS.TeamTimeStatus(tProject.isImpact) = ecWorkspaceStatus.wsSynhronousActive
                                    End If
                                    ' D2174 ==
                                    tSessionWorkspaces.Add(tWS)
                                    tSessionUsers.Add(tPrjUser)    ' D3151
                                End If
                                ' D2168 ==
                            End If
                            ' D2164 ==
                        End If
                    End If
                Next

                If fStartFromConsensus Then _TeamTime_Pipe = Nothing ' D3151

                fResult = TeamTimeStartSession(tMeetingOwner, tProject, CType(tProject.ProjectManager.ActiveHierarchy, ECHierarchyID), tSessionUsers, tSessionWorkspaces, Not fStartFromConsensus, AppID, fStartFromConsensus)  ' D2168 + D3207 + D4274 + D6553
            End If
            Return fResult
        End Function
        ' D1261 ==


        '' D0219 ===
        'Public Function TeamTimeResetUsers(ByVal tProject As clsProject, ByVal WSlist As List(Of clsWorkspace)) As Integer  ' D0505
        '    Dim UsersCount As Integer = 0
        '    If Not tProject Is Nothing Then
        '        tProject.ProjectManager.PipeBuilder.IsSynchronousSession = False    ' D0440
        '        tProject.ProjectManager.PipeBuilder.PipeCreated = False     ' D0440
        '        If WSlist Is Nothing Then WSlist = DBWorkspacesByProjectID(tProject.ID) ' D0505
        '        For Each tWS As clsWorkspace In WSlist  ' D0296 + D0505
        '            If tWS.isInTeamTime Then   ' D0382
        '                tWS.Status = ecWorkspaceStatus.wsEnabled
        '                DBWorkspaceUpdate(tWS, False, "Detach from Synchronous session")    ' D0296 + D0505
        '                UsersCount += 1
        '            End If
        '        Next
        '    End If
        '    DebugInfo("Reset synchronous users", _TRACE_INFO)
        '    Return UsersCount
        'End Function
        '' D0219 ==

        ' D0506 ===
        Public Sub TeamTimeEndSession(ByRef tProject As clsProject, ByVal fResetMeetingID As Boolean)   ' D0388 + D0510
            If Not tProject Is Nothing Then
                DBTeamTimeDataDelete(tProject.ID, Nothing)   ' D1275
                'Database.ExecuteSQL(String.Format("DELETE FROM {0} WHERE {1}={2} AND {3}={4}", _TABLE_EXTRA, _FLD_EXTRA_TYPEID, CInt(ecExtraType.TeamTime), _FLD_EXTRA_OBJECTID, tProject.ID))    ' D0507
                If tProject.isValidDBVersion AndAlso tProject.IsProjectLoaded Then  ' D3397
                    If tProject.isTeamTimeImpact AndAlso tProject.ProjectManager.ActiveHierarchy <> ECHierarchyID.hidImpact Then tProject.ProjectManager.ActiveHierarchy = ECHierarchyID.hidImpact ' D2063
                    If tProject.isTeamTimeLikelihood AndAlso tProject.ProjectManager.ActiveHierarchy <> ECHierarchyID.hidLikelihood Then tProject.ProjectManager.ActiveHierarchy = ECHierarchyID.hidLikelihood ' D2063
                End If
                If tProject.isTeamTime Then
                    If tProject.LockInfo.isLockAvailable(tProject.MeetingOwnerID) Then DBProjectLockInfoWrite(ECLockStatus.lsUnLocked, tProject.LockInfo, tProject.MeetingOwner, Now) ' D0589
                    Dim PrjWS As List(Of clsWorkspace) = DBWorkspacesByProjectID(tProject.ID)
                    For Each tWS As clsWorkspace In PrjWS
                        If tWS.isInTeamTime(tProject.isImpact) AndAlso tWS.Status(tProject.isImpact) <> ecWorkspaceStatus.wsDisabled Then ' D1741 + D1945
                            tWS.Status(tProject.isImpact) = ecWorkspaceStatus.wsEnabled ' D1945
                            'tWS.TeamTimeStatus = ecWorkspaceStatus.wsUnknown    ' D0660 -D0744
                            DBWorkspaceUpdate(tWS, False, "Detach from TeamTime session")
                        End If
                    Next
                    tProject.isTeamTime = False
                    tProject.MeetingOwner = Nothing     ' D0660
                    'If tProject.PipeParameters.SynchStartVotingOnceFacilitatorAllows Then tProject.ProjectParticipating = ecProjectParticipating.ppOffline ' D0219 + D0300 -D0748
                    If tProject.isValidDBVersion AndAlso tProject.IsProjectLoaded Then   ' D1457
                        If tProject.PipeParameters.SynchStartVotingOnceFacilitatorAllows Then tProject.isOnline = False ' D0219 + D0300 + D0748
                        tProject.ProjectManager.PipeBuilder.IsSynchronousSession = False    ' D0440
                        tProject.ProjectManager.PipeBuilder.PipeCreated = False             ' D0440
                        tProject.ProjectManager.PipeParameters.CurrentParameterSet = tProject.ProjectManager.PipeParameters.DefaultParameterSet ' D0522
                    End If
                    'tProject.ResetProject(True)     ' D0519 'C0563
                    If fResetMeetingID Then
                        tProject.MeetingIDLikelihood = clsMeetingID.ReNew   ' D1709
                        tProject.MeetingIDImpact = clsMeetingID.ReNew       ' D1709
                    End If

                    ' -D1275
                    'DBExtraDelete(clsExtra.Params2Extra(ProjectID, ecExtraType.TeamTime, ecExtraProperty.TeamTimeSessionData))      ' D1251
                    'DBExtraDelete(clsExtra.Params2Extra(ProjectID, ecExtraType.TeamTime, ecExtraProperty.TeamTimeSessionDateTime))  ' D1254

                    DBProjectUpdate(tProject, False, "End TeamTime session")
                    Workspaces = Nothing
                End If
            End If
        End Sub
        ' D0506 ==

        ' D0432 ===
        '1)	When one or more of the objectives are set to one of these measurement methods (Utility Curve, Advanced Utility Curve, Step Function, Direct Input) AND “Use Keypads” option is selected.

        '2)	When one or more of the objectives are set to one of these measurement methods (Utility Curve, Advanced Utility Curve, Step Function, Direct Input) AND “Use Keypads” option is NOT selected.

        '3)	When Rating Scale has more than 9 intensities and “Use Keypads” option is checked.

        '4)	When the Shared Input Source is set.

        '5)	When the TeamTime session is set to evaluate either of the following
        'a.	Only objectives 
        'b.	Both objectives and alternatives
        'AND Project Owner’s role is not set to evaluate any objective.

        '6)	When the TeamTime session is set to evaluate either of the following
        'a.	Only objectives 
        'b.	Both objectives and alternatives
        'AND Project Owner’s role is not set to evaluate a few objectives.

        '7)	When the TeamTime session is set to evaluate either of the following
        'a.	Only alternatives 
        'b.	Both objectives and alternatives
        'AND Project Owner’s role is not set to evaluate any alternative.

        '8)	When the TeamTime session is set to evaluate either of the following
        'a.	Only alternatives 
        'b.	Both objectives and alternatives
        'AND Project Owner’s role is not set to evaluate only a few alternatives.

        Public Function TeamTimeCheckProject(ByRef tProject As clsProject, ByVal fUseKeypdas As Boolean) As List(Of Integer)  ' D0433 + D0456 + D0510
            Dim Res As New List(Of Integer) ' D0433
            If Not tProject Is Nothing Then ' D0487
                '                If tProject.isTeamTime Then  ' D0487 -D0663
                If Not tProject.PipeParameters.ForceDefaultParameters AndAlso (tProject.PipeParameters.CurrentParameterSet IsNot tProject.PipeParameters.ImpactParameterSet) Then tProject.PipeParameters.CurrentParameterSet = tProject.PipeParameters.ImpactParameterSet ' D0663 + D0805
                With tProject.ProjectManager.ProjectAnalyzer
                    If fUseKeypdas Then  ' D0442 + D0456
                        If .HasAdvancedMeasurementTypeAndKeyPadsAreON Then Res.Add(1) ' D0433
                        If .RatingScaleHasMoreThan9IntensitiesAndKeyPadsAreON() Then Res.Add(3) ' D0433
                    Else
                        If .HasAdvancedMeasurementTypeAndKeyPadsAreOFF Then Res.Add(2) ' D0433
                    End If

                    If .SharedInputSourceIsInUse Then Res.Add(4) ' D0433

                    ' D0487 ===
                    Dim tOwnerAHPUserID As Integer = -1
                    Dim tOwner As clsApplicationUser = tProject.MeetingOwner
                    If tOwner Is Nothing Then tOwner = ActiveUser ' D0663
                    If Not tOwner Is Nothing Then
                        Dim AHPUser As clsUser = tProject.ProjectManager.GetUserByEMail(tOwner.UserEmail)
                        If Not AHPUser Is Nothing Then tOwnerAHPUserID = AHPUser.UserID
                    End If
                    ' D0487 ==

                    If .ProjectOwnerCannotEvaluateAnyObjective(tOwnerAHPUserID) Then Res.Add(5)
                    If .ProjectOwnerCannotEvaluateSomeObjectives(tOwnerAHPUserID) Then Res.Add(6)
                    If .ProjectOwnerCannotEvaluateAnyAlternative(tOwnerAHPUserID) Then Res.Add(7)
                    If .ProjectOwnerCannotEvaluateSomeAlternatives(tOwnerAHPUserID) Then Res.Add(8)
                End With
                'End If
            End If
            Return Res
        End Function

        Public Function TeamTimeCheckerMessage(ByVal Res As Integer) As String
            Dim names() As String = {"NoErrors", _
                                     "HasAdvancedMeasurementTypeAndKeyPadsAreON", _
                                     "HasAdvancedMeasurementTypeAndKeyPadsAreOFF", _
                                     "RatingScaleHasMoreThan9IntensitiesAndKeyPadsAreON", _
                                     "SharedInputSourceIsInUse", _
                                     "ProjectOwnerCannotEvaluateAnyObjective", _
                                     "ProjectOwnerCannotEvaluateSomeObjectives", _
                                     "ProjectOwnerCannotEvaluateAnyAlternative", _
                                     "ProjectOwnerCannotEvaluateSomeAlternatives"}
            Dim msg As String = ""
            If Res >= 0 And Res <= names.GetUpperBound(0) Then msg = ResString(String.Format("msgSync_{0}", names(Res)))
            Return msg
        End Function
        ' D0432 ==

        ' D0450 ===
        Public Function TeamTimeCheckMaxUsers(ByRef tProject As clsProject, ByRef tUser As clsApplicationUser, ByRef WSList As List(Of clsWorkspace)) As Boolean    ' D0510
            Dim max As Integer = Options.SynchronousMaxUsers     ' D0459
            If max < 0 Or tProject Is Nothing Then Return False
            If Not tProject.isTeamTime Then Return False
            Dim Cnt As Integer = 0
            Dim fIsNew As Boolean = True
            Dim UID As Integer = -1
            If Not tUser Is Nothing Then UID = tUser.UserID
            If WSList Is Nothing Then Return False ' D0509
            For Each tWS As clsWorkspace In WSList
                If tWS.isInTeamTime(tProject.isImpact) Then ' D1945
                    Cnt += 1
                    If tWS.UserID = UID Then fIsNew = False
                End If
            Next
            Return Cnt > max AndAlso fIsNew
        End Function
        ' D0450 ==

    End Class

End Namespace