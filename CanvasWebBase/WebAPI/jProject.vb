Imports System.Reflection

Namespace ExpertChoice.WebAPI

    ' D6348 ===
    Public Enum ecProjectStateOnOpen
        psRegular = 0
        psNew = 1
        psBOGGSAT = 2
        psAnytimePipe = 3 ' D6406
    End Enum
    ' D6348 ==

    <Serializable> Public Class jProjectShort
        Inherits clsJsonObject

        Public Property ID As Integer = -1
        Public Property Name As String = ""
        Public Property Description As String = ""
        Public Property AccessCode As String = ""
        Public Property AccessCodeImpact As String = ""
        Public Property MeetingID As String = ""
        Public Property MeetingIDImpact As String = ""
        Public Property CS_MeetingID As String = ""         ' D4920
        Public Property CS_MeetingIDImpact As String = ""   ' D4920
        Public Property isRisk As Boolean = False
        Public Property isOnline As Boolean = False
        Public Property isPublic As Boolean = False         ' D6471
        Public Property ProjectType As Integer = CInt(Canvas.ProjectType.ptRegular) ' D4978
        Public Property ProjectStatus As ecProjectStatus = ecProjectStatus.psActive
        Public Property isMarkedAsDeleted As Boolean = False
        Public Property CreationDate As Date? = Nothing
        Public Property ModifyDate As Date? = Nothing
        Public Property LastAccessDate As Date? = Nothing
        Public Property TeamTimeStatus As Integer = -1  ' D4742

        Shared Function CreateFromBaseObject(tprj As clsProject) As jProjectShort
            If tprj IsNot Nothing Then
                Return New jProjectShort With {
                    .ID = tprj.ID,
                    .Name = tprj.ProjectName,
                    .Description = ExpertChoice.Service.StringFuncs.HTML2Text(tprj.Comment).Trim,
                    .AccessCode = tprj.PasscodeLikelihood, .AccessCodeImpact = tprj.PasscodeImpact,
                    .isRisk = tprj.IsRisk,
                    .ProjectStatus = tprj.ProjectStatus,
                    .isMarkedAsDeleted = tprj.isMarkedAsDeleted,
                    .ProjectType = tprj.ProjectTypeData,
                    .CreationDate = tprj.Created, .ModifyDate = tprj.LastModify, .LastAccessDate = tprj.LastVisited,
                    .isOnline = tprj.isOnline,
                    .isPublic = tprj.isPublic,
                    .MeetingID = clsMeetingID.AsString(tprj.MeetingIDLikelihood), .MeetingIDImpact = clsMeetingID.AsString(tprj.MeetingIDImpact),
                    .CS_MeetingID = clsMeetingID.AsString(tprj.MeetingIDLikelihood, clsMeetingID.ecMeetingIDType.Antigua), .CS_MeetingIDImpact = clsMeetingID.AsString(tprj.MeetingIDImpact, clsMeetingID.ecMeetingIDType.Antigua),
                    .TeamTimeStatus = If(tprj.isTeamTimeLikelihood, 1, If(tprj.isTeamTimeImpact, 2, 0)) ' D4920 + D4978 + D6471
                }
            Else
                Return Nothing  ' D4602
            End If
        End Function

    End Class


    <Serializable> Public Class jProject
        Inherits jProjectShort

        Public Property Version As String = ""
        Public Property Starred As Boolean = False       'A1715
        Public Property OwnerID As Integer = -1
        Public Property OwnerEmail As String = ""
        Public Property OwnerName As String = ""
        'Public Property HasJudgments As Boolean = False
        Public Property UserRole As String = ""
        Public Property HasSurveys As String = ""       ' D4626
        Public Property CanOpen As Boolean = True
        Public Property CanModify As Boolean = False
        Public Property CanEvaluate As Boolean = False  ' D4941
        Public Property CanView As Boolean = False      ' D4941
        Public Property LockStatus As ECLockStatus = ECLockStatus.lsUnLocked
        Public Property LockedUserID As Integer = -1    ' D4980
        Public Property LockedBy As String = ""
        Public Property LockExpires As Date? = Nothing
        Public Property OnlineUsers As Integer = 0      ' D4980
        'Public Property UsersCount As Integer = 0
        Public Property LastVisited As Date? = Nothing  ' D6443
        Public Property ComplexStatus As String = ""    ' D5040

        ' D5011 ===
        ''' <summary>
        ''' Fill all project data after call jProjectShort.CreateFromBaseObject(). Please note, it doesn't call ProjectFillOwner() and not init .OnlineUsers
        ''' </summary>
        ''' <param name="tPrj"></param>
        ''' <param name="proj"></param>
        Shared Sub FillProjectData(App As clsComparionCore, ByRef tPrj As jProject, proj As clsProject)
            If tPrj IsNot Nothing AndAlso proj IsNot Nothing AndAlso App IsNot Nothing Then
                Dim tWS As clsWorkspace = clsWorkspace.WorkspaceByUserIDAndProjectID(App.ActiveUser.UserID, proj.ID, App.Workspaces)
                ' D4741 ===
                If App.ActiveUser IsNot Nothing Then
                    If tWS IsNot Nothing AndAlso App.ActiveWorkgroup IsNot Nothing Then
                        Dim tGrp As clsRoleGroup = App.ActiveWorkgroup.RoleGroup(tWS.GroupID)
                        If tGrp IsNot Nothing Then tPrj.UserRole = tGrp.Name
                        If tWS.LastModify.HasValue Then tPrj.LastVisited = tWS.LastModify.Value ' D6443
                    End If
                    ' D7159 ===
                    Dim tUW As clsUserWorkgroup = App.ActiveUserWorkgroup
                    If tUW IsNot Nothing AndAlso tUW.Status <> ecUserWorkgroupStatus.uwDisabled Then
                        tPrj.CanModify = App.CanUserModifyProject(App.ActiveUser.UserID, proj.ID, tUW, tWS, App.ActiveWorkgroup)    ' D4741 + D4965 + D4980
                        tPrj.CanEvaluate = Not proj.isMarkedAsDeleted AndAlso proj.ProjectStatus = ecProjectStatus.psActive AndAlso (proj.isValidDBVersion OrElse proj.isDBVersionCanBeUpdated) AndAlso App.CanUserDoProjectAction(ecActionType.at_mlEvaluateModel, App.ActiveUser.UserID, proj.ID, App.ActiveUserWorkgroup, App.ActiveWorkgroup) AndAlso (tPrj.CanModify OrElse tWS IsNot Nothing AndAlso tWS.StatusLikelihood <> ecWorkspaceStatus.wsDisabled)  ' D4941 + D4980
                        ' D9404 ===
                        If Not proj.isMarkedAsDeleted Then tPrj.CanView = False
                        If Not proj.isMarkedAsDeleted AndAlso (proj.isValidDBVersion OrElse proj.isDBVersionCanBeUpdated) Then
                            tPrj.CanView = App.CanUserDoProjectAction(ecActionType.at_mlViewModel, App.ActiveUser.UserID, proj.ID, tUW, App.ActiveWorkgroup)           ' D4941 + D4980
                            If Not tPrj.CanView AndAlso proj.ProjectStatus = ecProjectStatus.psTemplate AndAlso App.CanUserDoAction(ecActionType.at_alCreateNewModel, tUW, App.ActiveWorkgroup) Then tPrj.CanView = True    ' D6407
                        End If
                        ' D9404 ==
                    End If
                    ' D7159 ==
                    If proj.LockInfo IsNot Nothing Then
                        tPrj.LockStatus = proj.LockInfo.LockStatus
                        If tPrj.LockStatus <> ECLockStatus.lsUnLocked AndAlso proj.LockInfo.LockExpiration > Now Then
                            Dim tUser As clsApplicationUser = Nothing
                            If proj.LockInfo.LockerUserID = App.ActiveUser.UserID Then
                                tUser = App.ActiveUser
                            Else
                                tUser = App.DBUserByID(proj.LockInfo.LockerUserID)
                                ' -D4980
                            End If
                            If tUser IsNot Nothing Then
                                tPrj.LockedBy = tUser.UserEmail
                                tPrj.LockedUserID = tUser.UserID
                            End If
                            tPrj.LockExpires = proj.LockInfo.LockExpiration
                        Else
                            tPrj.LockStatus = ECLockStatus.lsUnLocked
                        End If
                    End If
                End If
                    If tPrj.LockStatus = ECLockStatus.lsLockForSystem Then ' D4980
                    tPrj.CanModify = False
                    tPrj.CanEvaluate = False   ' D4941
                    tPrj.CanView = False       ' D4941
                End If
                tPrj.CanOpen = tPrj.CanModify OrElse tPrj.CanView OrElse tPrj.CanEvaluate  ' D4941
                ' D4741 ==
                tPrj.Starred = tWS IsNot Nothing AndAlso tWS.isStarred  ' D5006    //App.ActiveWorkgroup.StarredProjectsIDsByUserEmail.ContainsKey(App.ActiveUser.UserEmail) AndAlso App.ActiveWorkgroup.StarredProjectsIDsByUserEmail(App.ActiveUser.UserEmail).Contains(tPrj.ID) 'A1715
                tPrj.Version = proj.DBVersion.GetVersionString
            End If
        End Sub

        Shared Sub GetActionResultByProject(Page As clsComparionCorePage, ByRef tRes As jActionResult, tPrj As clsProject, Optional IgnoreTTSessions As Boolean = False, Optional tProjectState As ecProjectStateOnOpen = ecProjectStateOnOpen.psRegular, Optional openProject As Boolean = True)    ' D6053 + D6060 + D6348 + D7175
            If tRes IsNot Nothing AndAlso tPrj IsNot Nothing Then
                If tPrj.isValidDBVersion OrElse tPrj.isDBVersionCanBeUpdated Then
                    With Page.App
                        Dim PgID As Integer = _DEF_PGID_ONEVALUATEPROJECT   ' D6060
                        ' D6348 ===
                        Dim PgIDPM As Integer
                        Select Case tProjectState
                            Case ecProjectStateOnOpen.psNew
                                PgIDPM = If(Page.App.isRiskEnabled, If(_OPT_MY_RISK_REWARD_ALLOWED AndAlso tPrj.isMyRiskRewardModel, _DEF_PGID_ONNEW_MYRISKREWARD, _DEF_PGID_ONNEWPROJECT_RISK), _DEF_PGID_ONNEWPROJECT)    ' D6799 + D7067
                            Case ecProjectStateOnOpen.psBOGGSAT
                                PgIDPM = If(Page.App.isRiskEnabled, _DEF_PGID_ONNEW_BOGGSAT_RISK, _DEF_PGID_ONNEW_BOGGSAT)
                            Case Else
                                PgIDPM = If(Page.App.isRiskEnabled, If(_OPT_MY_RISK_REWARD_ALLOWED AndAlso tPrj.isMyRiskRewardModel, _DEF_PGID_ONSELECTPROJECT_MYRISKREWARD, _DEF_PGID_ONSELECTPROJECT_RISK), _DEF_PGID_ONSELECTPROJECT)   ' D6799 + D7067
                        End Select
                        ' D6348 ==
                        ' D4943 ===
                        Dim fCanOpen As Boolean = True
                        ' D4983 ===
                        If tPrj.LockInfo IsNot Nothing AndAlso tPrj.LockInfo.LockStatus <> ECLockStatus.lsUnLocked AndAlso tPrj.LockInfo.isExpired Then
                            tPrj.LockInfo.LockStatus = ECLockStatus.lsUnLocked
                        End If
                        ' D4983 ==
                        If tPrj.LockInfo IsNot Nothing AndAlso tPrj.LockInfo.LockStatus = ECLockStatus.lsLockForSystem Then
                            tRes.Data = "system"
                            tRes.Message = Page.ResString("msgEvaluationLockedBySystem")
                            fCanOpen = False
                        Else
                            ' D4943 ==
                            Dim tWS As clsWorkspace = clsWorkspace.WorkspaceByUserIDAndProjectID(.ActiveUser.UserID, tPrj.ID, .Workspaces)
                            ' D4755 ===
                            Dim fCanManage As Boolean = .CanUserModifyProject(.ActiveUser.UserID, tPrj.ID, .ActiveUserWorkgroup, tWS, .ActiveWorkgroup)
                            Dim fCanEvaluate As Boolean = .CanUserDoProjectAction(ecActionType.at_mlEvaluateModel, .ActiveUser.UserID, tPrj.ID, .ActiveUserWorkgroup, .ActiveWorkgroup) ' D4939
                            If fCanManage OrElse fCanEvaluate Then  ' D4939
                                ' D6253 ===
                                If fCanManage AndAlso tProjectState = ecProjectStateOnOpen.psRegular AndAlso (tPrj.isValidDBVersion OrElse tPrj.isDBVersionCanBeUpdated) AndAlso (Not _OPT_MY_RISK_REWARD_ALLOWED OrElse Not tPrj.isMyRiskRewardModel) Then   ' D6348 + D6813 + D7066
                                    If Not String.IsNullOrEmpty(tPrj.ProjectManager.ProjectDescription) OrElse Not String.IsNullOrEmpty(tPrj.Comment) OrElse tPrj.ProjectStatus = ecProjectStatus.psMasterProject Then  ' D6399 + D6773
                                        PgIDPM = If(tPrj.IsRisk, _DEF_PGID_ONSELECTPROJECT_RISK_INFODOC, _DEF_PGID_ONSELECTPROJECT_INFODOC)
                                    End If
                                End If
                                If tProjectState = ecProjectStateOnOpen.psAnytimePipe Then PgIDPM = _DEF_PGID_ONEVALUATEPROJECT ' D6406
                                ' D6253 ==
                                If Not IgnoreTTSessions AndAlso tPrj.LockInfo IsNot Nothing AndAlso tPrj.LockInfo.LockStatus = ECLockStatus.lsLockForAntigua Then   ' D6053
                                    tRes.Data = "antigua"
                                    If fCanManage AndAlso tPrj.LockInfo.isLockedByUser(.ActiveUser) Then
                                        PgID = PgIDPM ' D4948 + D6060
                                    Else
                                        tRes.Message = Page.ResString("msgEvaluationLockedAntigua")
                                        fCanOpen = False
                                    End If
                                End If
                                If tPrj.isTeamTime Then
                                    If fCanManage AndAlso tPrj.LockInfo IsNot Nothing AndAlso tPrj.LockInfo.isLockedByUser(.ActiveUser) Then ' D4948
                                        'PgID = _PGID_TEAMTIME_STATUS    ' D4742
                                        PgID = PgIDPM ' D4948 + D6060
                                        tRes.Data = "teamtime"          ' D4742
                                    Else
                                        ' D4992 ===
                                        If fCanManage Then
                                            PgID = PgIDPM   ' D6060
                                            tRes.Data = "join_teamtime"
                                        Else
                                            PgID = _PGID_TEAMTIME
                                        End If
                                        ' D4992 ==
                                    End If
                                Else
                                    If fCanManage Then
                                        ' D4755 ==
                                        '-A1593 PgID = If(.isRiskEnabled, _PGID_PROJECT_PROPERTIES, _PGID_STRUCTURE_HIERARCHY)
                                        PgID = PgIDPM 'A1593 + D6060
                                        If Not IgnoreTTSessions Then    ' D6053
                                            ' D4943 ===
                                            For Each tmpPrj As clsProject In .ActiveProjectsList
                                                If tmpPrj.ID <> tPrj.ID AndAlso tmpPrj.isTeamTime AndAlso tmpPrj.MeetingOwnerID = .ActiveUser.UserID Then
                                                    If tmpPrj.LockInfo IsNot Nothing AndAlso tmpPrj.LockInfo.LockStatus = ECLockStatus.lsLockForTeamTime AndAlso Not tmpPrj.LockInfo.isExpired Then ' D4983
                                                        tRes.Data = "confirm_teamtime"
                                                        tRes.ObjectID = tPrj.ID
                                                        tRes.Message = String.Format(Page.ResString("confOpenWhileTeamTime"), ExpertChoice.Service.StringFuncs.SafeFormString(tmpPrj.ProjectName))   ' D4958
                                                        tRes.URL = Page.PageURL(PgID,, True)    ' D6359
                                                        fCanOpen = False
                                                        Exit Sub   ' sorry )
                                                    End If
                                                End If
                                            Next
                                            ' D4943 ==
                                        End If
                                    Else
                                        ' D6390 ===
                                        If fCanEvaluate AndAlso tPrj.isOnline Then
                                            PgID = _DEF_PGID_ONEVALUATEPROJECT   ' D6060
                                        Else
                                            If .CanUserDoProjectAction(ecActionType.at_mlViewModel, .ActiveUser.UserID, tPrj.ID, .ActiveUserWorkgroup, .ActiveWorkgroup) Then
                                                PgID = _DEF_PGID_ONVIEWPROJECT
                                            Else
                                                fCanOpen = False
                                            End If
                                        End If
                                        ' D6390 ==
                                    End If
                                End If
                            Else
                                ' D6404 ===
                                If tPrj.ProjectStatus = ecProjectStatus.psTemplate AndAlso .CanUserDoAction(ecActionType.at_alCreateNewModel, .ActiveUserWorkgroup, .ActiveWorkgroup) Then
                                    PgID = PgIDPM
                                Else
                                    ' D6404 ==
                                    PgID = _DEF_PGID_ONVIEWPROJECT    ' D4939 + D6060 // overall results for viewers

                                    ' D6908 ===
                                    If tPrj.ProjectStatus = ecProjectStatus.psActive AndAlso Not fCanEvaluate AndAlso Not fCanManage AndAlso fCanOpen AndAlso .CanUserDoProjectAction(ecActionType.at_mlViewOverallResults, .ActiveUser.UserID, tPrj.ID, .ActiveUserWorkgroup, .ActiveWorkgroup) Then
                                        If tPrj.ProjectManager.Reports.ByCategory(ECCore.ecReportCategory.Dashboard).Count > 0 Then
                                            PgID = _PGID_ANALYSIS_DASHBOARD
                                        End If
                                    End If
                                    ' D6908 ==

                                End If
                            End If
                            If fCanOpen Then    ' D4943
                                If openProject Then .ProjectID = tPrj.ID    ' D7175
                                tRes.ObjectID = tPrj.ID
                            Else
                                tRes.Result = ecActionResult.arError    ' D4943
                            End If
                        End If
                        tRes.URL = Page.PageURL(PgID,, True)    ' D6359
                    End With
                End If
            End If
        End Sub

        Overloads Shared Function CreateFromBaseObject(App As clsComparionCore, proj As clsProject) As jProject ' D5032
            Dim tPrj As jProject = CType(clsJsonObject.doInherit(jProjectShort.CreateFromBaseObject(proj), GetType(jProject)), jProject) ' D7267
            If tPrj IsNot Nothing Then jProject.FillProjectData(App, tPrj, proj)
            Return tPrj
        End Function
        ' D5011 ==

        ' D5040 ===
        Shared Function GetProjectByID(App As clsComparionCore, Optional ID As Integer = -1) As jProject
            If App IsNot Nothing Then
                Dim PrjID As Integer = If(ID <= 0, App.ProjectID, ID)
                If PrjID > 0 Then
                    ' D6022 ===
                    Dim tPrj As clsProject = clsProject.ProjectByID(PrjID, App.ActiveProjectsList)
                    If tPrj IsNot Nothing Then
                        Dim tProjects As New List(Of clsProject)
                        tProjects.Add(tPrj)
                        Dim Lst As List(Of jProject) = jProject.GetProjectsList(App, tProjects)
                        If Lst IsNot Nothing AndAlso Lst.Count > 0 Then Return Lst(0)
                    End If
                    ' D6022 ==
                End If
            End If
            Return Nothing
        End Function
        ' D5040 ==

        'Shared Function GetProjectsList(App As clsComparionCore, Optional tPrjID As Integer = -1) As List(Of jProject)  ' D5040
        Shared Function GetProjectsList(App As clsComparionCore, Optional ProjectsList As List(Of clsProject) = Nothing) As List(Of jProject)  ' D5040 + D6022
            Dim tList As New List(Of jProject)
            ' D4611 ===

            'Dim tPrjList As New List(Of clsProject)
            'If tPrjID > 0 Then
            '    Dim tPrj As clsProject = clsProject.ProjectByID(tPrjID, App.ActiveProjectsList)
            '    If tPrj IsNot Nothing Then tPrjList.Add(tPrj)
            'Else
            '    tPrjList = App.ActiveProjectsList
            'End If
            If ProjectsList Is Nothing Then ProjectsList = App.ActiveProjectsList ' D6022

            If ProjectsList IsNot Nothing AndAlso ProjectsList.Count > 0 Then
                ' D4611 ==

                Dim tUsers As New List(Of clsApplicationUser)
                If App.ActiveWorkgroup IsNot Nothing Then
                    Dim tDBUsers As List(Of Dictionary(Of String, Object)) = App.Database.SelectBySQL(String.Format("SELECT DISTINCT U.* FROM {0} U LEFT JOIN {1} P ON P.OwnerID=U.ID WHERE P.WorkgroupID={2}", clsComparionCore._TABLE_USERS, clsComparionCore._TABLE_PROJECTS, App.ActiveWorkgroup.ID))
                    For Each tRow As Dictionary(Of String, Object) In tDBUsers
                        tUsers.Add(App.DBParse_ApplicationUser(tRow))
                    Next
                End If

                ' D4980 ===
                Dim onlineSess As List(Of clsOnlineUserSession) = App.DBOnlineSessions
                Dim tMe As clsOnlineUserSession = clsOnlineUserSession.OnlineSessionByUserID(App.ActiveUser.UserID, onlineSess)
                If tMe IsNot Nothing Then onlineSess.Remove(tMe)
                ' D4980 ==

                'Dim tWSList As New List(Of clsWorkspace)
                'If App.ActiveUser IsNot Nothing Then tWSList = App.DBWorkspacesByUserID(App.ActiveUser.UserID)
                ProjectsList.ForEach(Sub(proj)  ' D4611
                                         Dim tPrj As jProject = jProject.CreateFromBaseObject(App, proj)
                                         If tPrj IsNot Nothing Then
                                             ' D5011 ===
                                             If proj IsNot Nothing AndAlso proj.OwnerID > 0 Then    ' D7405
                                                 ' D5033 ===
                                                 Dim tOwner As clsApplicationUser = clsApplicationUser.UserByUserID(proj.OwnerID, tUsers)
                                                 If tOwner IsNot Nothing Then
                                                     tPrj.OwnerID = tOwner.UserID
                                                     tPrj.OwnerEmail = tOwner.UserEmail
                                                     tPrj.OwnerName = If(tOwner.UserName = "", tOwner.UserEmail, tOwner.UserName)
                                                 End If
                                                 ' D5033 ==
                                             End If
                                             tPrj.OnlineUsers = clsOnlineUserSession.OnlineSessionsByProjectID(tPrj.ID, onlineSess).Count   ' D4980
                                             tPrj.ComplexStatus = tPrj.GetComplexStatus    ' D5040
                                             ' D5011 ==
                                             If tPrj.Name = "" Then tPrj.Name = String.Format("#{0}", tPrj.ID)  ' D6656
                                             tList.Add(tPrj)
                                         End If
                                     End Sub)

                If App.ActiveWorkgroup IsNot Nothing Then
                    Const sPID As String = "ProjectID"
                    Const sType As String = "StructureType"
                    Dim isSinglePrj As Boolean = ProjectsList IsNot Nothing AndAlso ProjectsList.Count = 1 AndAlso ProjectsList(0) IsNot Nothing    ' D6452
                    Dim sSQL As String = String.Format("SELECT M.{0}, M.{1} FROM ModelStructure as M, {2} as P WHERE P.{3}={4} AND M.{0}=P.{5} AND" + If(isSinglePrj, " M.{0}={10} AND", "") + " (M.{1}={6} OR M.{1}={7} OR M.{1}={8} OR M.{1}={9})", sPID, sType, clsComparionCore._TABLE_PROJECTS, clsComparionCore._FLD_PROJECTS_WRKGID, App.ActiveWorkgroup.ID, clsComparionCore._FLD_PROJECTS_ID, CInt(StructureType.stSpyronStructureWelcome), CInt(StructureType.stSpyronStructureThankYou), CInt(StructureType.stSpyronStructureImpactWelcome), CInt(StructureType.stSpyronStructureImpactThankyou), If(isSinglePrj, ProjectsList(0).ID, 0))   ' D5040 + D6022 + D6452
                    Dim tSList As List(Of Dictionary(Of String, Object)) = App.Database.SelectBySQL(sSQL)
                    For Each tRow As Dictionary(Of String, Object) In tSList
                        If Not IsDBNull(tRow(sPID)) AndAlso Not IsDBNull(tRow(sType)) Then
                            Dim tID As Integer
                            Dim tType As Integer
                            If Integer.TryParse(CStr(tRow(sPID)), tID) AndAlso Integer.TryParse(CStr(tRow(sType)), tType) Then
                                For Each tmpPrj As jProject In tList
                                    If tmpPrj.ID = tID Then
                                        ' D4626 ===
                                        If tType = StructureType.stSpyronStructureWelcome Then tmpPrj.HasSurveys += "LW"
                                        If tType = StructureType.stSpyronStructureThankYou Then tmpPrj.HasSurveys += "LT"
                                        If tType = StructureType.stSpyronStructureImpactWelcome Then tmpPrj.HasSurveys += "IW"
                                        If tType = StructureType.stSpyronStructureImpactThankyou Then tmpPrj.HasSurveys += "IT"
                                        ' D4626 ==
                                        Exit For
                                    End If
                                Next
                            End If
                        End If
                    Next
                End If

            End If

            Return tList
        End Function

        ' D5040 ===
        Private Function GetComplexStatus() As String
            Return String.Format("{0}-{1}-{2}-{3}", CInt(LockStatus), TeamTimeStatus, OnlineUsers, If(isOnline, 1, 0))   ' D6369
        End Function
        ' D5040 ==

    End Class

    ' D5032 ===
    Public Class jProjectsLimits
        Public Property CanCreateNew As Boolean = False
        Public Property License_LifetimeLimit As Long = ECSecurity.ECSecurity.UNLIMITED_VALUE
        Public Property License_LifetimeValue As Long = 0
        Public Property License_TotalLimit As Long = ECSecurity.ECSecurity.UNLIMITED_VALUE
        Public Property License_TotalValue As Long = 0
        Public Property License_OnlineLimit As Long = ECSecurity.ECSecurity.UNLIMITED_VALUE
        Public Property License_OnlineValue As Long = 0
    End Class
    ' D5032 ==

End Namespace