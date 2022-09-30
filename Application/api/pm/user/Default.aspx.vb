''' <summary>
''' This is base functions for project participants
''' </summary>
''' <remarks>Remark</remarks>
Partial Class PrjUserWebAPI
    Inherits clsComparionCorePage

    Private _Snapshot_Comment_User_Update As String = "Update TT users settings"    ' D6430

    Public Sub New()
        MyBase.New(_PGID_WEBAPI)
    End Sub

    Private Function _Page() As mpWebAPI
        Return CType(Master, mpWebAPI)
    End Function

    ''' <summary>
    ''' Get the list of on-line users for specific project
    ''' </summary>
    ''' <param name="ID">ProjectID that checking users</param>
    ''' <returns>Returns the list of users as <seealso cref="jOnlineUser">jOnlineUser</seealso> in case of Success</returns>
    ''' <permission cref="ecPagePermission.ppProject"/>
    Public Function List_Online(ID As Integer) As jActionResult
        FetchIfCantEditProject(ID)
        Dim tLst As New List(Of jOnlineUser)

        Dim tOnline As List(Of clsOnlineUserSession) = App.DBOnlineSessions()
        Dim PMGrpID As Integer = App.ActiveWorkgroup.GetDefaultRoleGroupID(ecRoleLevel.rlModelLevel, ecRoleGroupType.gtProjectManager)
        For Each tSess As clsOnlineUserSession In tOnline
            If tSess.ProjectID = ID AndAlso tSess.UserID <> App.ActiveUser.UserID Then
                Dim S As jOnlineUser = jOnlineUser.CreateFromBaseObject(tSess)
                S.isPM = tSess.RoleGroupID = PMGrpID
                tLst.Add(S)
            End If
        Next

        Return New jActionResult With {
            .Result = ecActionResult.arSuccess,
            .ObjectID = ID,
            .Data = tLst
        }
    End Function

    ' D6429 ===
    Public Function TeamTime_UsersList() As jActionResult
        FetchIfCantEditProject(App.ProjectID)

        Dim AppUsers As List(Of clsApplicationUser) = App.DBUsersByProjectID(App.ProjectID)
        Dim WS As List(Of clsWorkspace) = App.DBWorkspacesByProjectID(App.ProjectID)
        Dim UW As List(Of clsUserWorkgroup) = App.DBUserWorkgroupsByProjectIDWorkgroupID(App.ProjectID, App.ActiveWorkgroup.ID)
        Dim UsersWithData As HashSet(Of Integer) = App.ActiveProject.ProjectManager.StorageManager.Reader.DataExistsForUsersHashset(App.ActiveProject.ProjectManager.ActiveHierarchy)
        Dim OnlineSess As List(Of clsOnlineUserSession) = App.DBOnlineSessions

        Dim TTUsersList As New List(Of jUserTeamTime)
        For Each tUser As clsApplicationUser In AppUsers

            Dim TTUser As New jUserTeamTime

            Dim tPrjUser As clsUser = App.ActiveProject.ProjectManager.GetUserByEMail(tUser.UserEmail)
            Dim tWS As clsWorkspace = clsWorkspace.WorkspaceByUserIDAndProjectID(tUser.UserID, App.ProjectID, WS)
            Dim tUW As clsUserWorkgroup = clsUserWorkgroup.UserWorkgroupByUserIDAndWorkgroupID(tUser.UserID, App.ActiveWorkgroup.ID, UW)

            If tPrjUser IsNot Nothing Then
                TTUser = jUserTeamTime.CreateFromBaseObject(tPrjUser)
            Else
                TTUser.Name = tUser.UserName
                If tWS IsNot Nothing Then TTUser.AccessMode = clsWorkspace.TeamTimStatusAsSyncMode(tWS.TeamTimeStatus(App.ActiveProject.isImpact))
            End If
            If tUser.UserID = App.ActiveUser.UserID AndAlso TTUser.AccessMode = SynchronousEvaluationMode.semNone Then
                TTUser.AccessMode = SynchronousEvaluationMode.semOnline  ' D6430
                tWS.TeamTimeStatus(App.ActiveProject.isImpact) = ecWorkspaceStatus.wsSynhronousActive   ' D7113
            End If

            TTUser.ID = tUser.UserID    ' must be after the call jUserTeamTime.CreateFromBaseObject()
            TTUser.Email = tUser.UserEmail

            TTUser.isOnline = tUser.UserID = App.ActiveUser.UserID OrElse clsOnlineUserSession.OnlineSessionByUserID(tUser.UserID, OnlineSess) IsNot Nothing
            TTUser.HasData = tPrjUser IsNot Nothing AndAlso UsersWithData.Contains(tPrjUser.UserID)
            TTUser.CanEvaluate = App.CanUserDoProjectAction(ecActionType.at_mlEvaluateModel, tUser.UserID, App.ProjectID, tUW, App.ActiveWorkgroup)
            If TTUser.CanEvaluate Then TTUser.Link = CreateLogonURL(tUser, App.ActiveProject, "TTOnly=1", ApplicationURL(False, True) + "/", App.ActiveProject.Passcode(App.ActiveProject.isImpact), tUW)

            TTUser.Groups = ""  ' D6435
            TTUser.GroupIDs = New List(Of Integer) ' D6435
            For Each group As clsCombinedGroup In App.ActiveProject.ProjectManager.CombinedGroups.GroupsList
                If group.CombinedUserID <> -1 Then
                    If group.UsersList.Contains(tPrjUser) Then
                        TTUser.Groups += If(TTUser.Groups = "", "", ", ") + group.Name
                        TTUser.GroupIDs.Add(group.ID)  ' D6435
                    End If
                End If
            Next
            TTUsersList.Add(TTUser)
        Next

        Return New jActionResult With {
            .Data = TTUsersList,
            .Result = ecActionResult.arSuccess
        }
    End Function
    ' D6429 ==

    ' D6430 ===
    Private Function _TeamTime_PrjUser_Update(tPrjUser As clsUser, AccessMode As SynchronousEvaluationMode, KeyPad As Integer, SaveData As Boolean) As Boolean
        FetchIfCantEditProject(App.ProjectID)  ' D7493
        Dim fRes As Boolean = False
        If tPrjUser IsNot Nothing Then
            If SaveData AndAlso tPrjUser.SyncEvaluationMode = AccessMode AndAlso tPrjUser.VotingBoxID = KeyPad Then SaveData = False    ' D6430
            tPrjUser.SyncEvaluationMode = AccessMode
            If KeyPad > 0 AndAlso KeyPad <= 256 Then tPrjUser.VotingBoxID = KeyPad
            If SaveData Then
                Dim sMode As String = AccessMode.ToString
                If AccessMode = SynchronousEvaluationMode.semVotingBox Then sMode += String.Format(" #{0}", KeyPad)
                Dim sComment As String = String.Format("{0}: {1}", tPrjUser.UserEMail, sMode)
                App.ActiveProject.SaveUsersInfo(tPrjUser, _Snapshot_Comment_User_Update, False, True, sComment)
            End If
            fRes = True
        End If
        Return fRes
    End Function

    Public Function TeamTime_User_Update(Email As String, AccessMode As SynchronousEvaluationMode, KeyPad As Integer) As jActionResult
        FetchIfCantEditProject(App.ProjectID)
        Dim tRes As New jActionResult
        Dim tPrjUser As clsUser = App.ActiveProject.ProjectManager.GetUserByEMail(Email)
        If tPrjUser IsNot Nothing AndAlso _TeamTime_PrjUser_Update(tPrjUser, AccessMode, KeyPad, True) Then
            tRes.ObjectID = tPrjUser.UserID
            tRes.Data = 1   ' D7211
            tRes.Message = ""
            tRes.Result = ecActionResult.arSuccess
        Else
            tRes.Message = "User not found"
            tRes.Result = ecActionResult.arError
        End If
        Return tRes
    End Function

    Public Function TeamTime_UsersList_Update(Users As List(Of jUserTeamTimeOptions)) As jActionResult
        FetchIfCantEditProject(App.ProjectID)
        Dim tRes As New jActionResult
        If Users IsNot Nothing AndAlso Users.Count > 0 Then
            If Users.Count = 1 Then Return TeamTime_User_Update(Users.First().Email, Users.First().AccessMode, Users.First().Keypad)
            Dim Updated As Integer = 0  ' D7211
            Dim sList As String = ""
            For Each tOpt As jUserTeamTimeOptions In Users
                Dim tPrjUser As clsUser = App.ActiveProject.ProjectManager.GetUserByEMail(tOpt.Email)
                If tPrjUser IsNot Nothing AndAlso _TeamTime_PrjUser_Update(tPrjUser, tOpt.AccessMode, tOpt.Keypad, False) Then
                    Updated += 1    ' D7211
                    sList += String.Format("{0}{1}", If(sList = "", "", ", "), tPrjUser.UserEMail)
                End If
            Next
            If Updated > 0 Then App.ActiveProject.SaveUsersInfo(, _Snapshot_Comment_User_Update, False, True, "Update users: " + sList) ' D7211
            tRes.Data = Updated ' D7211
            tRes.Result = ecActionResult.arSuccess
        Else
            tRes.Message = "Nothing to change"
            tRes.Result = ecActionResult.arError
        End If
        Return tRes
    End Function
    ' D6430 ==


    Public Function List(Optional groups As Boolean = True, Optional attribs As Boolean = True, Optional defAttribs As Boolean = True, Optional evalProgress As Boolean = False) As jActionResult
        FetchIfNoActiveProject()

        Return New jActionResult With {
            .Result = ecActionResult.arSuccess,
            .Data = New jUsersList With {
                .Users = jUserProject.GetList(App.ActiveProject.ProjectManager),
                .Groups = If(groups, jUserProject.GetUserGroups(App.ActiveProject.ProjectManager.CombinedGroups.GroupsList), Nothing),
                .Attributes = If(attribs, jUserAttribute.GetList(App.ActiveProject.ProjectManager.Attributes.GetUserAttributes, App.ActiveProject.ProjectManager.Attributes, defAttribs), Nothing),
                .EvalProgress = If(evalProgress, jEvalProgress.GetList(App.ActiveProject.ProjectManager), Nothing)
            }
        }
    End Function

    Public Function GetEvalProgress() As jActionResult
        FetchIfNoActiveProject()

        Return New jActionResult With {
            .Result = ecActionResult.arSuccess,
            .Data = jEvalProgress.GetList(App.ActiveProject.ProjectManager)
        }
    End Function

    Private Sub PrjWebAPI_Load(sender As Object, e As EventArgs) Handles Me.Load
        Select Case _Page.Action

            Case "list"
                Dim optGroups As Boolean = True
                Dim optAttrs As Boolean = True
                Dim optAllAttrs As Boolean = True
                Dim optEvalPrg As Boolean = True
                Str2Bool(GetParam(_Page.Params, "groups", True), optGroups)
                Str2Bool(GetParam(_Page.Params, "attribs", True), optGroups)
                Str2Bool(GetParam(_Page.Params, "defattribs", True), optGroups)
                Str2Bool(GetParam(_Page.Params, "evalprogress", True), optEvalPrg)
                _Page.ResponseData = List(optGroups, optAttrs, optAllAttrs, optEvalPrg)
            Case "getevalprogress"
                _Page.ResponseData = GetEvalProgress()

            Case "list_online"
                Dim ID As Integer = -1
                Integer.TryParse(GetParam(_Page.Params, _PARAM_ID, True), ID)
                If (ID <= 0) Then ID = App.ProjectID
                _Page.ResponseData = List_Online(ID)

            Case "teamtime_userslist"   ' D6429 
                _Page.ResponseData = TeamTime_UsersList()   ' D6429

                ' D6430 ===
            Case "teamtime_user_update"
                Dim Mode As Integer = CInt(SynchronousEvaluationMode.semNone)
                Integer.TryParse(GetParam(_Page.Params, "AccessMode", True), Mode)
                If Not [Enum].IsDefined(GetType(SynchronousEvaluationMode), Mode) Then Mode = SynchronousEvaluationMode.semNone
                Dim Keypad As Integer = -1
                Integer.TryParse(GetParam(_Page.Params, "Keypad", True), Keypad)
                _Page.ResponseData = TeamTime_User_Update(GetParam(_Page.Params, _PARAM_EMAIL, True), CType(Mode, SynchronousEvaluationMode), Keypad)

            Case "teamtime_userslist_update"
                Dim Users As String = GetParam(_Page.Params, "users", True)
                Dim fError As Boolean = False
                If Not String.IsNullOrEmpty(Users) Then
                    Try
                        Dim List As JArray = JsonConvert.DeserializeObject(Of JArray)(Users)
                        Dim Params As New List(Of jUserTeamTimeOptions)
                        If List IsNot Nothing Then
                            For Each tItem As JToken In List
                                Params.Add(tItem.ToObject(Of jUserTeamTimeOptions))
                            Next
                            _Page.ResponseData = TeamTime_UsersList_Update(Params)
                        Else
                            fError = True
                        End If
                    Catch ex As Exception
                        fError = True
                    End Try
                Else
                    fError = True
                End If
                If fError Then FetchWithCode(HttpStatusCode.BadRequest, , "Missing or wrong parameter")
                ' D6430 ==

        End Select
    End Sub

End Class