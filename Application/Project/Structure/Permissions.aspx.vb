Partial Class Permission4AltsPage
    Inherits clsComparionCorePage

    Public Sub New()
        MyBase.New(_PGID_STRUCTURE_PERMISSION_ALTS)
    End Sub

    Public ReadOnly Property PRJ As clsProject
        Get
            Return App.ActiveProject
        End Get
    End Property

    ReadOnly Property PM As clsProjectManager
        Get
            Return App.ActiveProject.ProjectManager
        End Get
    End Property

    Private ReadOnly Property SESS_PERMISSIONS_FOR As String
        Get
            Return String.Format("SESS_PERMISSIONS_FOR_{0}", App.ProjectID.ToString)
        End Get
    End Property

    Public _PermissionsFor As Integer = -1 ' 0 - for users, 1 - for groups
    Public Property PermissionsFor As Integer
        Get
            If _PermissionsFor < 0 Then
                Dim tSessVar = Session(SESS_PERMISSIONS_FOR)
                If tSessVar IsNot Nothing AndAlso TypeOf tSessVar Is Integer Then
                    _PermissionsFor = CInt(tSessVar)
                Else
                    _PermissionsFor = CInt(IIf(App.ActiveProject.ProjectStatus = ecProjectStatus.psTemplate, 1, 0)) ' users by default if not Template
                End If
            End If
            Return _PermissionsFor
        End Get
        Set(value As Integer)
            _PermissionsFor = value
            Session(SESS_PERMISSIONS_FOR) = value
        End Set
    End Property

    Private ReadOnly Property SESS_CHECKED_USERS As String
        Get
            Return String.Format("SESS_PERMISSIONS_CHECKED_USERS_{0}", App.ProjectID.ToString)
        End Get
    End Property

    Public _CheckedUsers As List(Of Integer) = Nothing
    Public Property CheckedUsers As List(Of Integer)
        Get
            If _CheckedUsers Is Nothing Then
                Dim tSessVar = Session(SESS_CHECKED_USERS)
                If tSessVar IsNot Nothing AndAlso TypeOf tSessVar Is List(Of Integer) Then
                    _CheckedUsers = CType(tSessVar, List(Of Integer))
                Else
                    _CheckedUsers = New List(Of Integer) ' check only first user/group by default
                End If
                If _CheckedUsers Is Nothing Then _CheckedUsers = New List(Of Integer)
                If _CheckedUsers.Count = 0 AndAlso PM.UsersList IsNot Nothing AndAlso PM.UsersList.Count > 0 Then _CheckedUsers.Add(PM.UsersList(0).UserID)
            End If
            Return _CheckedUsers
        End Get
        Set(value As List(Of Integer))
            _CheckedUsers = value
            Session(SESS_CHECKED_USERS) = value
        End Set
    End Property

    Private ReadOnly Property SESS_CHECKED_GROUPS As String
        Get
            Return String.Format("SESS_PERMISSIONS_CHECKED_GROUPS_{0}", App.ProjectID.ToString)
        End Get
    End Property

    Public _CheckedGroups As List(Of Integer) = Nothing
    Public Property CheckedGroups As List(Of Integer)
        Get
            If _CheckedGroups Is Nothing Then
                Dim tSessVar = Session(SESS_CHECKED_GROUPS)
                If tSessVar IsNot Nothing AndAlso TypeOf tSessVar Is List(Of Integer) Then
                    _CheckedGroups = CType(tSessVar, List(Of Integer))
                Else
                    _CheckedGroups = New List(Of Integer) ' check only first user/group by default
                End If
                If _CheckedGroups Is Nothing Then _CheckedGroups = New List(Of Integer)
                If _CheckedGroups.Count = 0 AndAlso PM.CombinedGroups.GroupsList IsNot Nothing AndAlso PM.CombinedGroups.GroupsList.Count > 0 Then _CheckedGroups.Add(CType(PM.CombinedGroups.GroupsList(0), clsCombinedGroup).CombinedUserID)
            End If
            Return _CheckedGroups
        End Get
        Set(value As List(Of Integer))
            _CheckedGroups = value
            Session(SESS_CHECKED_GROUPS) = value
        End Set
    End Property

    Public Function GetCheckList(tPermissionsFor As Integer) As String
        Dim sRes As String = ""
        If tPermissionsFor = 0 Then
            'For Each user As clsUser In PM.UsersList
            '    sRes += String.Format("<li style='list-style-type: none; margin: 0; padding: 0; white-space: nowrap;' title='{1}'><div class='divCheckbox'><label><input type='checkbox' class='cb_multiselect_users' data-id='{0}' {2} onclick='onParticipantCheckboxChecked(event, this, {0}, 0);'>{1}</label></div></li>", user.UserID, JS_SafeString(CStr(IIf(user.UserName = "", user.UserEMail, user.UserName))), IIf(CheckedUsers.Contains(user.UserID), " checked='checked' ", ""))
            'Next
            For Each user As clsUser In PM.UsersList
                sRes += If(sRes = "", "", ",") + String.Format("{{ ""key"" : ""{0}"", ""text"" : ""{1}"", ""checked"" : {2} }}", user.UserID, JS_SafeString(CStr(IIf(user.UserName = "", user.UserEMail, user.UserName))), Bool2JS(CheckedUsers.Contains(user.UserID)))
            Next
        End If
        If tPermissionsFor = 1 Then
            'For Each group As clsCombinedGroup In PM.CombinedGroups.GroupsList
            '    sRes += String.Format("<li style='list-style-type: none; margin: 0; padding: 0; white-space: nowrap;' title='{1}'><div class='divCheckbox'><label><input type='checkbox' class='cb_multiselect_groups' data-id='{0}' {2} onclick='onParticipantCheckboxChecked(event, this, {0}, 1);'>{1}</label></div></li>", group.CombinedUserID, JS_SafeString(group.Name), IIf(CheckedGroups.Contains(group.CombinedUserID), " checked='checked' ", ""))
            'Next
            For Each group As clsCombinedGroup In PM.CombinedGroups.GroupsList
                sRes += If(sRes = "", "", ",") + String.Format("{{ ""key"" : ""{0}"", ""text"" : ""{1}"", ""checked"" : {2} }}", group.CombinedUserID, JS_SafeString(group.Name), Bool2JS(CheckedGroups.Contains(group.CombinedUserID)))
            Next
        End If
        Return String.Format("[{0}]", sRes)
    End Function

    Public Function GetCheckedUsersList(tPermissionsFor As Integer) As String
        Dim sRes As String = ""
        Dim list As List(Of Integer) = Nothing
        If tPermissionsFor = 0 Then list = CheckedUsers
        If tPermissionsFor = 1 Then list = CheckedGroups

        For Each ID As Integer In list
            sRes += CStr(IIf(sRes = "", "", ",")) + ID.ToString
        Next

        Return String.Format("[{0}]", sRes)
    End Function

    Public Function GetCheckedUsersIDs(tFor As Integer) As List(Of Integer)
        If tFor = 0 Then
            Return CheckedUsers
        Else
            Return CheckedGroups
        End If
    End Function

    Public Function GetRows(UserIDs As List(Of Integer), ShowStat As Boolean) As String
        Return datagrid_GetRows(PM, UserIDs, , ShowStat, PM.IsRiskProject AndAlso PM.ActiveObjectives.HierarchyID = ECHierarchyID.hidLikelihood)
    End Function

    Public Function GetColumns(UserIDs As List(Of Integer), ShowStat As Boolean) As String
        Return datagrid_GetColumns(PM, UserIDs, Not (PM.IsRiskProject AndAlso PM.ActiveObjectives.HierarchyID = ECHierarchyID.hidLikelihood), ShowStat)
    End Function

    Public Function GetHierarchyColumns() As String
        Return datagrid_GetHierarchyColumns(PM)
    End Function

    'A1201 ===
    Public Function GetAttributes() As String
        Return datagrid_GetAttributes(PM, App)
    End Function
    'A1201 ==

    Private Function JSRolesToCore(role As Integer) As RolesValueType
        Dim aRole As RolesValueType = RolesValueType.rvtUndefined
        Select Case role
            Case 0
                aRole = RolesValueType.rvtRestricted
            Case 1
                aRole = RolesValueType.rvtAllowed
            Case 2
                aRole = RolesValueType.rvtUndefined
        End Select
        Return aRole
    End Function

    'A1205 ===
    Protected Sub Ajax_Callback(data As String)
        Dim args As NameValueCollection = HttpUtility.ParseQueryString(data)
        Dim sAction As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "action")).ToLower ' Anti-XSS
        Dim tResult As String = CStr(IIf(String.IsNullOrEmpty(sAction), "", String.Format("['{0}','']", sAction)))
        Dim sSnapshotMessage As String = "Set role(s)"
        Dim sSnapshotComment As String = ""
        Select Case sAction
            Case "setallroles"
                Dim Role As Integer = CInt(EcSanitizer.GetSafeHtmlFragment(GetParam(args, "val")))
                Dim tUsers As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "chkusers")).Trim()
                Dim rm As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "rm")).Trim()
                Dim UserIDs As New List(Of Integer)
                For Each s As String In tUsers.Split(CChar(","))
                    Dim ID As Integer = 0
                    If Integer.TryParse(s, ID) AndAlso Not UserIDs.Contains(ID) Then
                        UserIDs.Add(ID)
                    End If
                Next

                Dim TerminalObjectiveIDs As New List(Of Integer)
                Dim NonTerminalObjectiveIDs As New List(Of Integer)
                For Each obj In PM.Hierarchy(PM.ActiveHierarchy).Nodes
                    If obj.IsTerminalNode Then TerminalObjectiveIDs.Add(obj.NodeID) Else NonTerminalObjectiveIDs.Add(obj.NodeID)
                Next

                Dim AlternativeIDs As New List(Of Integer)
                For Each alt In PM.AltsHierarchy(PM.ActiveAltsHierarchy).Nodes
                    AlternativeIDs.Add(alt.NodeID)
                Next
                If rm = "objs" Or rm = "both" Then PM.UsersRoles.SetRolesForObjectives(UserIDs, NonTerminalObjectiveIDs, JSRolesToCore(Role))
                If rm = "alts" Or rm = "both" Then PM.UsersRoles.SetRolesForAlternatives(UserIDs, TerminalObjectiveIDs, AlternativeIDs, JSRolesToCore(Role))
                tResult = String.Format("{{""action"":""{0}"", ""checkedusers"":[{1}], ""rows"":{2}, ""cols"":{3}}}", sAction, tUsers, GetRows(UserIDs, False), GetColumns(UserIDs, False)).Replace("'", """")
                sSnapshotComment = String.Format("{0} {3} for {1} {2}(s)", sAction, JS_SafeString(UserIDs.Count), If(PermissionsFor = 0, "user", "group"), If(Role = 0, "restricted", If(Role = 1, "allowed", "undefined")))
            Case "setrolerange"
                Dim tColIDs As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "colids")).Trim()
                Dim tRowIDs As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "rowids")).Trim()
                Dim Role As Integer = CInt(EcSanitizer.GetSafeHtmlFragment(GetParam(args, "val")))
                Dim tUsers As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "chkusers")).Trim()
                Dim UserIDs As New List(Of Integer)
                For Each s As String In tUsers.Split(CChar(","))
                    Dim ID As Integer = 0
                    If Integer.TryParse(s, ID) AndAlso Not UserIDs.Contains(ID) Then
                        UserIDs.Add(ID)
                    End If
                Next

                Dim ObjectiveIDs As New List(Of Integer)
                For Each s As String In tColIDs.Split(CChar(","))
                    Dim ID As Integer = 0
                    If Integer.TryParse(s, ID) AndAlso Not ObjectiveIDs.Contains(ID) Then
                        ObjectiveIDs.Add(ID)
                    End If
                Next

                Dim AlternativeIDs As New List(Of Integer)
                For Each s As String In tRowIDs.Split(CChar(","))
                    Dim ID As Integer = 0
                    If Integer.TryParse(s, ID) AndAlso Not AlternativeIDs.Contains(ID) Then
                        AlternativeIDs.Add(ID)
                    End If
                Next

                PM.UsersRoles.SetRolesForAlternatives(UserIDs, ObjectiveIDs, AlternativeIDs, JSRolesToCore(Role))
                tResult = String.Format("{{""action"":""{0}"", ""checkedusers"":[{1}], ""rows"":{2}, ""cols"":{3}}}", sAction, tUsers, GetRows(UserIDs, False), GetColumns(UserIDs, False)).Replace("'", """")
                sSnapshotComment = String.Format("{0} {3} for {1} {2}(s)", sAction, JS_SafeString(UserIDs.Count), If(PermissionsFor = 0, "user", "group"), If(Role = 0, "restricted", If(Role = 1, "allowed", "undefined")))
            Case "setobjrole"
                Dim tObjID As Integer = CInt(EcSanitizer.GetSafeHtmlFragment(GetParam(args, "objid")))
                Dim Role As Integer = CInt(EcSanitizer.GetSafeHtmlFragment(GetParam(args, "val")))
                Dim tUsers As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "chkusers")).Trim()
                Dim UserIDs As New List(Of Integer)
                For Each s As String In tUsers.Split(CChar(","))
                    Dim ID As Integer = 0
                    If Integer.TryParse(s, ID) AndAlso Not UserIDs.Contains(ID) Then
                        UserIDs.Add(ID)
                    End If
                Next
                Dim ObjIDs As New List(Of Integer)
                ObjIDs.Add(tObjID)
                PM.UsersRoles.SetRolesForObjectives(UserIDs, ObjIDs, JSRolesToCore(Role))
                tResult = String.Format("{{""action"":""{0}"",""role"":{1}}}", sAction, Role)
                sSnapshotComment = String.Format("{0} {3} for {1} {2}(s)", sAction, JS_SafeString(UserIDs.Count), If(PermissionsFor = 0, "user", "group"), If(Role = 0, "restricted", If(Role = 1, "allowed", "undefined")))
            Case "setcellrole"
                Dim ColID As Integer = CInt(EcSanitizer.GetSafeHtmlFragment(GetParam(args, "colid")))
                Dim RowID As Integer = CInt(EcSanitizer.GetSafeHtmlFragment(GetParam(args, "rowid")))
                Dim Role As Integer = CInt(EcSanitizer.GetSafeHtmlFragment(GetParam(args, "val")))
                Dim tUsers As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "chkusers")).Trim()
                Dim UserIDs As New List(Of Integer)
                For Each s As String In tUsers.Split(CChar(","))
                    Dim ID As Integer = 0
                    If Integer.TryParse(s, ID) AndAlso Not UserIDs.Contains(ID) Then
                        UserIDs.Add(ID)
                    End If
                Next
                Dim ObjectiveIDs As New List(Of Integer)
                ObjectiveIDs.Add(ColID)
                Dim AlternativeIDs As New List(Of Integer)
                AlternativeIDs.Add(RowID)
                PM.UsersRoles.SetRolesForAlternatives(UserIDs, ObjectiveIDs, AlternativeIDs, JSRolesToCore(Role))
                'Dim RolesStat As List(Of RolesStatistics) = PM.GetRolesStatistics()
                'Dim NewStat As String = If(ColID = -1, "0", GetRoleStatStringByGUIDs(RolesStat, PM.ActiveAlternatives.GetNodeByID(RowID).NodeGuidID, PM.ActiveObjectives.GetNodeByID(ColID).NodeGuidID))
                'tResult = String.Format("{{""action"":""{0}"",""role"":{1},""colid"":{2},""rowid"":{3},""cellstat"":""{4}""}}", sAction, Role, ColID, RowID, NewStat)
                tResult = String.Format("{{""action"":""{0}"",""role"":{1},""colid"":{2},""rowid"":{3},""cellstat"":""{4}""}}", sAction, Role, ColID, RowID, "")
                sSnapshotComment = String.Format("{0} {3} for {1} {2}(s)", sAction, JS_SafeString(UserIDs.Count), If(PermissionsFor = 0, "user", "group"), If(Role = 0, "restricted", If(Role = 1, "allowed", "undefined")))
            Case "pasteroles"
                Dim copyFromUserID As Integer = CInt(EcSanitizer.GetSafeHtmlFragment(GetParam(args, "copyfromuserid")))
                Dim rm As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "rm")).Trim()
                Dim ACopyOptions As CopyRolesOptions = CopyRolesOptions.ObjectivesAndAlternatives
                If rm = "alts" Then ACopyOptions = CopyRolesOptions.Alternatives
                If rm = "objs" Then ACopyOptions = CopyRolesOptions.Objectives
                Dim tUsers As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "chkusers")).Trim()
                Dim UserIDs As New List(Of Integer)
                For Each s As String In tUsers.Split(CChar(","))
                    Dim ID As Integer = 0
                    If Integer.TryParse(s, ID) AndAlso Not UserIDs.Contains(ID) Then
                        UserIDs.Add(ID)
                    End If
                Next
                PM.UsersRoles.CopyUserPermissions(copyFromUserID, UserIDs, ACopyOptions)
                tResult = String.Format("{{""action"":""{0}"", ""checkedusers"":[{1}], ""rows"":{2}, ""cols"":{3}}}", "activate_users", tUsers, GetRows(UserIDs, False), GetColumns(UserIDs, False)).Replace("'", """")
                sSnapshotComment = String.Format("{0} to {1} {2}(s)", "copy roles", JS_SafeString(UserIDs.Count), If(PermissionsFor = 0, "user", "group"))
            Case "select_columns"
                Dim tColumnsIDs As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "column_ids"))
                SelectedColumns(App.ActiveProject) = tColumnsIDs
                tResult = String.Format("{{""action"":""{0}"",""value"":""""}}", sAction)
            Case "permissions_for_changed"
                PermissionsFor = CInt(EcSanitizer.GetSafeHtmlFragment(GetParam(args, "value")))
                tResult = String.Format("{{""action"":""{0}"",""value"":{1}}}", sAction, PermissionsFor)
            Case "activate_users", "showstatistic"
                Dim tFor As Integer = CInt(EcSanitizer.GetSafeHtmlFragment(GetParam(args, "for")))
                Dim tIDs As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "ids")).Trim()

                Dim tCheckedList As List(Of Integer) = New List(Of Integer)

                Dim sIDs() As String = tIDs.Split(CChar(","))
                If sIDs IsNot Nothing Then
                    For Each s As String In sIDs
                        Dim ID As Integer = 0
                        If Integer.TryParse(s, ID) AndAlso Not tCheckedList.Contains(ID) Then
                            tCheckedList.Add(ID)
                        End If
                    Next
                End If

                If tFor = 0 Then CheckedUsers = tCheckedList
                If tFor = 1 Then CheckedGroups = tCheckedList

                tResult = String.Format("{{""action"":""{0}"", ""checkedusers"":{1}, ""rows"":{2}, ""cols"":{3}, ""is_for"":{4}}}", sAction, GetCheckedUsersList(tFor), GetRows(tCheckedList, sAction = "showstatistic"), GetColumns(tCheckedList, sAction = "showstatistic"), tFor).Replace("'", """")
        End Select

        If tResult <> "" Then
            If sSnapshotComment <> "" Then
                App.ActiveProject.SaveStructure(sSnapshotMessage, True, True, sSnapshotComment)
            End If
            Response.Clear()
            Response.ContentType = "text/plain"
            Response.Write(tResult)
            Response.End()
        End If
    End Sub

    Protected Sub Page_InitComplete(sender As Object, e As EventArgs) Handles Me.InitComplete
        Ajax_Callback(Request.Form.ToString)
    End Sub
    'A1205 ==

End Class