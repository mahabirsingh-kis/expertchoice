Imports System.Xml
Imports System.Web.Script.Serialization

Partial Class ProjectParticipantsPage
    Inherits clsComparionCorePage

    Private sampleResults As Boolean = False

    Private SHOW_COMBINED_ALL_GROUP As Boolean = False 'A1153
    ' D3954 ===
    Private _UsersList As List(Of clsApplicationUser) = Nothing
    Private _WSList As List(Of clsWorkspace) = Nothing
    Private _UWFullList As List(Of clsUserWorkgroup) = Nothing
    Private _UWPrjList As List(Of clsUserWorkgroup) = Nothing
    ' D3954 ==

    Public Const Flt_Separator As String = vbTab
    Public Const OPT_MAX_USERS_COUNT_TO_LOAD_PROGRESS As Integer = 150

    Public _evalProgress As Dictionary(Of Integer, Dictionary(Of String, UserEvaluationProgressData)) = Nothing
    Private _evalPrgDT As DateTime? = Nothing  ' D7386

    Public IsWidget As Boolean = False

    Private _Api As DashboardWebAPI = Nothing
    Public ReadOnly Property Api As DashboardWebAPI
        Get
            If _Api Is Nothing Then _Api = New DashboardWebAPI
            Return _Api
        End Get
    End Property

    Public Sub New()
        MyBase.New(_PGID_PROJECT_USERS)
    End Sub

    ' D3954 ===
    Public ReadOnly Property UsersList As List(Of clsApplicationUser)
        Get
            If _UsersList Is Nothing Then
                If CurrentPageID = _PGID_ADMIN_USERSLIST Then
                    _UsersList = App.DBUsersByWorkgroupID(App.ActiveWorkgroup.ID)
                Else
                    App.CheckProjectManagerUsers(PRJ)   ' D6600
                    _UsersList = App.DBUsersByProjectID(App.ProjectID)  ' D6600
                End If
            End If
            Return _UsersList
        End Get
    End Property

    Public ReadOnly Property WSList As List(Of clsWorkspace)
        Get
            If _WSList Is Nothing Then _WSList = App.DBWorkspacesByProjectID(App.ProjectID)
            Return _WSList
        End Get
    End Property

    Public ReadOnly Property UWFullList As List(Of clsUserWorkgroup)
        Get
            If _UWFullList Is Nothing Then _UWFullList = App.DBUserWorkgroupsByWorkgroupID(App.ActiveWorkgroup.ID)
            Return _UWFullList
        End Get
    End Property

    Public ReadOnly Property UWPrjList As List(Of clsUserWorkgroup)
        Get
            If _UWPrjList Is Nothing Then _UWPrjList = App.DBUserWorkgroupsByProjectIDWorkgroupID(App.ProjectID, App.ActiveWorkgroup.ID)
            Return _UWPrjList
        End Get
    End Property

    Public Function UserByID(tUserID As Integer) As clsApplicationUser
        Return clsApplicationUser.UserByUserID(tUserID, UsersList)
    End Function

    Public Function UserByEmail(sEmail As String) As clsApplicationUser
        Return clsApplicationUser.UserByUserEmail(sEmail, UsersList)
    End Function

    Public Function WSByUserID(tUserID As Integer) As clsWorkspace
        Return clsWorkspace.WorkspaceByUserIDAndProjectID(tUserID, App.ProjectID, WSList)
    End Function

    Public Function UWByUserID(tUserID As Integer) As clsUserWorkgroup
        Return clsUserWorkgroup.UserWorkgroupByUserIDAndWorkgroupID(tUserID, App.ActiveWorkgroup.ID, UWPrjList)
    End Function
    ' D3954 ==

    'A1153 ===
    Public ReadOnly Property IsReadOnly As Boolean
        Get
            With App
                If CurrentPageID = _PGID_ADMIN_USERSLIST Then Return Not .CanUserDoAction(ecActionType.at_alManageWorkgroupUsers, .ActiveUserWorkgroup, .ActiveWorkgroup)
                Return App.IsActiveProjectReadOnly
            End With
        End Get
    End Property

    Public Property CantBePMList As List(Of Integer)
        Get
            Dim tSessVar As Object = Session("Sess_CantBePMList")
            Dim deserialized As List(Of Integer) = Nothing
            If tSessVar IsNot Nothing AndAlso Not String.IsNullOrEmpty(CType(tSessVar, String)) Then
                Dim serializer = New JavaScriptSerializer()
                deserialized = serializer.Deserialize(Of List(Of Integer))(CType(tSessVar, String))
            End If
            Return deserialized
        End Get
        Set(value As List(Of Integer))
            Dim serializer = New JavaScriptSerializer()
            Dim serialized As String = serializer.Serialize(value)
            If Session("Sess_CantBePMList") Is Nothing Then
                Session.Add("Sess_CantBePMList", serialized)
            Else
                Session("Sess_CantBePMList") = serialized
            End If
        End Set
    End Property

    Public Property sessLDAPUsers As Dictionary(Of String, String)
        Get
            Dim tSessVar As Object = Session("LDAPUsers_data")
            Dim deserialized As Dictionary(Of String, String) = Nothing
            If tSessVar IsNot Nothing AndAlso Not String.IsNullOrEmpty(CType(tSessVar, String)) Then
                Dim serializer = New JavaScriptSerializer()
                deserialized = serializer.Deserialize(Of Dictionary(Of String, String))(CType(tSessVar, String))
            End If
            Return deserialized
        End Get
        Set(value As Dictionary(Of String, String))
            Dim serializer = New JavaScriptSerializer()
            Dim serialized As String = serializer.Serialize(value)
            If Session("LDAPUsers_data") Is Nothing Then
                Session.Add("LDAPUsers_data", serialized)
            Else
                Session("LDAPUsers_data") = serialized
            End If
        End Set
    End Property

    ReadOnly Property SESSION_EVAL_PROGRESS As String
        Get
            Return String.Format("ParticipantsEvalProgress_{0}", App.ProjectID.ToString)
        End Get
    End Property

    ' D7386 ===
    ReadOnly Property SESSION_EVAL_PRG_DT As String
        Get
            Return String.Format("ParticipantsEvalPrg_DT_{0}", App.ProjectID.ToString)
        End Get
    End Property
    ' D7386 ==

    Public Property evalProgress As Dictionary(Of Integer, Dictionary(Of String, UserEvaluationProgressData))
        Get
            If _evalProgress Is Nothing Then
                Dim tSessVar = Session(SESSION_EVAL_PROGRESS)
                If tSessVar IsNot Nothing AndAlso (TypeOf tSessVar Is Dictionary(Of Integer, Dictionary(Of String, UserEvaluationProgressData))) Then
                    _evalProgress = CType(tSessVar, Dictionary(Of Integer, Dictionary(Of String, UserEvaluationProgressData)))
                End If
            End If
            Return _evalProgress
        End Get
        ' D7386 ===
        Set(value As Dictionary(Of Integer, Dictionary(Of String, UserEvaluationProgressData)))
            _evalProgress = value
            _evalPrgDT = If(value Is Nothing Or Not App.HasActiveProject, Nothing, PRJ.LastModify)
            Session(SESSION_EVAL_PRG_DT) = _evalPrgDT
            If value Is Nothing Then Session.Remove(SESSION_EVAL_PROGRESS) Else Session(SESSION_EVAL_PROGRESS) = value
        End Set
        ' D7386 ==
    End Property

    Public Function GetUsersData(Optional LoadProgress As Boolean = False) As String

        If CurrentPageID = _PGID_ADMIN_USERSLIST Then Return GetWorkgroupUsers()

        Dim sRes As String = ""
        If PM Is Nothing OrElse PRJ.ProjectStatus = ecProjectStatus.psTemplate Then Return "[]"
        If UsersList.Count <= OPT_MAX_USERS_COUNT_TO_LOAD_PROGRESS AndAlso evalProgress Is Nothing Then LoadProgress = True

        Dim tCurrentFiltersList = CurrentUsersFiltersList
        Dim AttributesList = PM.Attributes.GetUserAttributes().Where(Function(attr) Not attr.IsDefault).ToList

        Dim fCanManageWkg As Boolean = App.CanUserDoAction(ecActionType.at_alManageWorkgroupUsers, App.ActiveUserWorkgroup, App.ActiveWorkgroup)    ' D4965
        Dim tPrjUsers As List(Of ECTypes.clsUser) = PM.UsersList
        Dim tUWList As List(Of clsUserWorkgroup) = Nothing
        If CurrentPageID = _PGID_PROJECT_USERS OrElse CurrentPageID = _PGID_RISK_INVITE_USERS Then tUWList = App.DBUserWorkgroupsByProjectIDWorkgroupID(App.ProjectID, App.ActiveWorkgroup.ID)     ' D4965
        Dim AdminWID As Integer = App.ActiveWorkgroup.GetDefaultRoleGroupID(ecRoleLevel.rlApplicationLevel, ecRoleGroupType.gtAdministrator)

        PM.CombinedGroups.UpdateDynamicGroups()
        Dim gUsers As New Dictionary(Of Integer, HashSet(Of Integer))
        Dim groups As New List(Of clsCombinedGroup)
        For Each group As clsCombinedGroup In PM.CombinedGroups.GroupsList
            If group.CombinedUserID <> COMBINED_USER_ID Then
                Dim hs As New HashSet(Of Integer)
                For Each u As clsUser In group.UsersList
                    hs.Add(u.UserID)
                Next
                gUsers.Add(group.ID, hs)
                groups.Add(group)
            End If
        Next

        Dim totalMadeLikelihood As Integer
        Dim totalJudgmentsLikelihood As Integer

        Dim totalMadeImpact As Integer
        Dim totalJudgmentsImpact As Integer

        If LoadProgress AndAlso CurrentPageID <> _PGID_RISK_INVITE_USERS Then ' if not eval progress for controls
            Dim uList As New List(Of clsUser)
            For Each tUser As clsApplicationUser In UsersList
                Dim u As clsUser = PM.GetUserByEMail(tUser.UserEmail)
                If u IsNot Nothing Then
                    uList.Add(u)
                End If
            Next
            ' D7386 ===
            Dim Prg As New Dictionary(Of Integer, Dictionary(Of String, UserEvaluationProgressData))
            If PM.IsRiskProject AndAlso CurrentPageID = _PGID_PROJECT_USERS Then
                Dim oldHID As Integer = PM.ActiveHierarchy
                PM.ActiveHierarchy = ECHierarchyID.hidLikelihood
                Prg.Add(ECHierarchyID.hidLikelihood, PM.StorageManager.Reader.GetEvaluationProgress(uList, ECHierarchyID.hidLikelihood, totalMadeLikelihood, totalJudgmentsLikelihood))
                PM.ActiveHierarchy = ECHierarchyID.hidImpact
                Prg.Add(ECHierarchyID.hidImpact, PM.StorageManager.Reader.GetEvaluationProgress(uList, ECHierarchyID.hidImpact, totalMadeImpact, totalJudgmentsImpact))
                PM.ActiveHierarchy = oldHID
            Else
                Prg.Add(PM.ActiveHierarchy, PM.StorageManager.Reader.GetEvaluationProgress(uList, PM.ActiveHierarchy, totalMadeLikelihood, totalJudgmentsLikelihood))
            End If
            evalProgress = Prg
            ' D7386 ==
        End If

        For Each tUser As clsApplicationUser In UsersList
            If tUser IsNot Nothing Then

                Dim sName As String = tUser.UserName
                Dim AHPUserID As Integer = -1
                Dim tPrjUser As clsUser = clsApplicationUser.AHPUserByUserEmail(tUser.UserEmail, tPrjUsers)
                Dim tWeight As Double = 0
                If tPrjUser IsNot Nothing Then
                    AHPUserID = tPrjUser.UserID
                    If Not String.IsNullOrEmpty(tPrjUser.UserName) Then sName = tPrjUser.UserName   ' D6710
                    tWeight = Math.Round(tPrjUser.Weight, 4)
                End If

                Dim sPrjLink As String = "" 'CreateLogonURL(tUser, PRJ, "pipe=yes", ApplicationURL(False, False)) - create on demand only, see "get_link" action
                'Dim fCanBePM As Boolean = App.CanUserBePM(App.ActiveWorkgroup, tUser.UserID, PRJ, False, True, clsUserWorkgroup.UserWorkgroupByUserIDAndWorkgroupID(tUser.UserID, App.ActiveWorkgroup.ID, tUWList), tWS)

                Dim fIsDisabled As Integer = 0
                Dim tGrpID As Integer = -1
                Dim sRole As String = ""
                Dim fCanBePM As Boolean = False
                Dim tWS As clsWorkspace = WSByUserID(tUser.UserID)
                If tWS IsNot Nothing Then
                    tGrpID = tWS.GroupID
                    'TODO: AD to add a workspace for controls
                    If tWS.Status(PRJ.isImpact) = ecWorkspaceStatus.wsDisabled Then fIsDisabled = 1
                    Dim tRole As clsRoleGroup = App.ActiveWorkgroup.RoleGroup(tWS.GroupID, App.ActiveWorkgroup.RoleGroups)
                    If tRole IsNot Nothing Then
                        sRole = tRole.Name
                        fCanBePM = tRole.ActionStatus(ecActionType.at_mlModifyModelHierarchy) = ecActionStatus.asGranted
                        Dim tUW As clsUserWorkgroup = UWByUserID(tUser.UserID)
                        If tUW IsNot Nothing AndAlso tUW.Status = ecUserWorkgroupStatus.uwDisabled Then fIsDisabled = 2
                        'If Not fCanBePM AndAlso tUW IsNot Nothing Then fCanBePM = App.CanUserDoAction(ecActionType.at_alCreateNewModel, tUW, App.ActiveWorkgroup)
                        If Not fCanBePM Then fCanBePM = tRole.ActionStatus(ecActionType.at_alCreateNewModel) = ecActionStatus.asGranted
                    End If
                End If

                If tUser.Status = ecUserStatus.usDisabled Then fIsDisabled = 2
                Dim tUserVisible As Boolean = True
                'If IsFilterApplied AndAlso tCurrentFiltersList IsNot Nothing AndAlso tCurrentFiltersList.Count > 0 Then
                '    If tCurrentFiltersList.Where(Function(r) r.IsChecked AndAlso r.SelectedAttributeID.Equals(DYNAMIC_ATTRIBUTE_GROUP_INCONSISTENCY_ID)).Count > 0 Then
                '        'TODO: AC - call a function to update the Inconsistencies for all users
                '    End If
                '    tUserVisible = IsUserIncludedInFilter(tPrjUser, tCurrentFiltersList)
                'End If
                If tUserVisible Then
                    'A1289 === check if links/actions are available 
                    Dim IsLinkEnabled As Boolean = True
                    Dim IsLinkGoEnabled As Boolean = True
                    Dim IsPreviewLinkEnabled As Boolean = True
                    ' User Link Enabled/Disabled
                    'If tUser.CannotBeDeleted() AndAlso (App.ActiveUser Is Nothing OrElse Not App.ActiveUser.CannotBeDeleted) Then IsLinkEnabled = False ' D7314
                    If tUser.CannotBeDeleted() Then IsLinkEnabled = False
                    'If IsLinkEnabled AndAlso Not App.CanUserModifyProject(tUser.UserID, App.ProjectID, App.ActiveUserWorkgroup, tWS) Then IsLinkEnabled = False    ' -D4392

                    Dim IsWSDisabled As Boolean = tWS.Status(PM.ActiveHierarchy = ECHierarchyID.hidImpact) = ecWorkspaceStatus.wsDisabled
                    Dim CanEvaluateProject As Boolean = App.CanUserDoProjectAction(ecActionType.at_mlEvaluateModel, tUser.UserID, App.ProjectID, App.ActiveUserWorkgroup, App.ActiveWorkgroup)
                    ' User Open Link Enabled/Disabled
                    If tUser.CannotBeDeleted() Then IsLinkGoEnabled = False
                    'If IsLinkGoEnabled AndAlso Not App.CanUserModifyProject(tUser.UserID, App.ProjectID, App.ActiveUserWorkgroup, tWS, App.ActiveWorkgroup) Then IsLinkGoEnabled = False   ' -D4392
                    If IsLinkGoEnabled AndAlso App.ActiveProject IsNot Nothing AndAlso (App.ActiveProject.isTeamTimeImpact OrElse App.ActiveProject.isTeamTimeLikelihood) AndAlso App.ActiveUser IsNot Nothing AndAlso App.ActiveUser.UserID = App.ActiveProject.MeetingOwnerID Then IsLinkGoEnabled = False
                    If IsLinkGoEnabled AndAlso IsWSDisabled Then IsLinkGoEnabled = False
                    Dim tUW As clsUserWorkgroup = clsUserWorkgroup.UserWorkgroupByUserIDAndWorkgroupID(tUser.UserID, App.ActiveWorkgroup.ID, tUWList)   ' D6972
                    ' D4965 ===
                    If tUser.UserID <> App.ActiveUser.UserID AndAlso IsLinkGoEnabled AndAlso Not fCanManageWkg AndAlso (CurrentPageID = _PGID_PROJECT_USERS OrElse CurrentPageID = _PGID_RISK_INVITE_USERS) AndAlso App.CanUserModifyProject(tUser.UserID, App.ProjectID, tUW, tWS, App.ActiveWorkgroup) Then  ' D6497
                        IsLinkGoEnabled = False
                    End If
                    If Not IsLinkGoEnabled Then IsLinkEnabled = False
                    '  D4965 ==

                    ' User Preview Enabled/Disabled
                    If Not CanEvaluateProject Then IsPreviewLinkEnabled = False
                    If IsPreviewLinkEnabled AndAlso (App.ActiveProject.isTeamTimeImpact OrElse App.ActiveProject.isTeamTimeLikelihood) Then IsPreviewLinkEnabled = False
                    'A1289 ==

                    ' D6972 ===
                    If IsLinkEnabled AndAlso IsLinkGoEnabled AndAlso fCanBePM AndAlso tUW IsNot Nothing Then
                        If tUW.RoleGroupID = AdminWID Then
                            IsLinkEnabled = False
                            IsLinkGoEnabled = False
                        End If
                    End If
                    ' D6972 ==

                If isSSO() Then
                    IsLinkEnabled = False
                    IsLinkGoEnabled = False
                End If

                    Dim madeCount As Integer = 0
                    Dim totalCount As Integer = 0
                    Dim sEvaluationProgress As String = ""
                    Dim userLastJudgmentTime As DateTime = VERY_OLD_DATE

                    'If fHasData Then    ' D6489
                    If CurrentPageID = _PGID_RISK_INVITE_USERS Then
                        PM.PipeBuilder.GetControlsEvaluationProgress(AHPUserID, madeCount, totalCount, userLastJudgmentTime)
                    Else
                        If evalProgress IsNot Nothing Then
                            If PM.IsRiskProject AndAlso CurrentPageID = _PGID_PROJECT_USERS Then
                                If evalProgress.ContainsKey(ECHierarchyID.hidLikelihood) AndAlso evalProgress(ECHierarchyID.hidLikelihood).ContainsKey(tUser.UserEmail.ToLower) Then    ' D7408
                                    With evalProgress(ECHierarchyID.hidLikelihood)(tUser.UserEmail.ToLower)
                                        madeCount += .EvaluatedCount
                                        totalCount += .TotalCount
                                        If .LastJudgmentTime.HasValue AndAlso .LastJudgmentTime.Value > userLastJudgmentTime Then
                                            userLastJudgmentTime = .LastJudgmentTime.Value
                                        End If
                                    End With
                                End If
                                If evalProgress.ContainsKey(ECHierarchyID.hidImpact) AndAlso evalProgress(ECHierarchyID.hidImpact).ContainsKey(tUser.UserEmail.ToLower) Then    ' D7408
                                    With evalProgress(ECHierarchyID.hidImpact)(tUser.UserEmail.ToLower)
                                        madeCount += .EvaluatedCount
                                        totalCount += .TotalCount
                                        If .LastJudgmentTime.HasValue AndAlso .LastJudgmentTime.Value > userLastJudgmentTime Then
                                            userLastJudgmentTime = .LastJudgmentTime.Value
                                        End If
                                    End With
                                End If
                            Else
                                If evalProgress.ContainsKey(PM.ActiveHierarchy) AndAlso evalProgress(PM.ActiveHierarchy).ContainsKey(tUser.UserEmail.ToLower) Then  ' D7408
                                    With evalProgress(PM.ActiveHierarchy)(tUser.UserEmail.ToLower)
                                        madeCount = .EvaluatedCount
                                        totalCount = .TotalCount
                                        If .LastJudgmentTime.HasValue Then
                                            userLastJudgmentTime = .LastJudgmentTime.Value
                                        End If
                                    End With
                                End If
                            End If

                            If madeCount > totalCount Then madeCount = totalCount
                        End If
                    End If

                    Dim fPrc = If(totalCount = 0, 100, CSng(Math.Round(madeCount * 100 / totalCount, 1)))
                    Dim sPrc = fPrc.ToString("F1")

                    sEvaluationProgress = JS_SafeString(String.Format("{0}% ({1}/{2})", sPrc, madeCount, totalCount))

                    Dim sAttrVals As String = ""
                    Dim i As Integer = 0
                    For Each tAttr As clsAttribute In AttributesList
                        If Not tAttr.IsDefault AndAlso tAttr.Type = CurrentAttributesType Then
                            Dim sVal As String = ""
                            Dim tAttrVal As Object = PM.Attributes.GetAttributeValue(tAttr.ID, AHPUserID)
                            If tAttrVal IsNot Nothing Then
                                Select Case tAttr.ValueType
                                    Case AttributeValueTypes.avtBoolean
                                        sVal = CStr(IIf(CBool(tAttrVal), "true", "false"))
                                    Case AttributeValueTypes.avtDouble
                                        sVal = JS_SafeNumber(CDbl(tAttrVal))
                                    Case AttributeValueTypes.avtString
                                        sVal = "'" + JS_SafeString(CStr(tAttrVal)) + "'"
                                    Case AttributeValueTypes.avtLong
                                        sVal = JS_SafeNumber(CLng(tAttrVal))
                                    Case AttributeValueTypes.avtEnumeration, AttributeValueTypes.avtEnumerationMulti
                                        ' not implemented
                                End Select
                            End If
                            sAttrVals += String.Format(",'v{0}':{1}", i, If(sVal = "", "''", sVal))
                            i += 1
                        End If
                    Next

                    Dim sGrpVals As String = ""
                    'Dim tCombinedAll As clsCombinedGroup = Nothing
                    'Dim groupsList As New List(Of clsCombinedGroup)

                    'For Each tGrp As clsCombinedGroup In PM.CombinedGroups.GroupsList
                    '    If tGrp.CombinedUserID = COMBINED_USER_ID Then
                    '        tCombinedAll = tGrp
                    '    Else
                    '        groupsList.Add(tGrp)
                    '    End If
                    'Next

                    'If SHOW_COMBINED_ALL_GROUP AndAlso tCombinedAll IsNot Nothing Then groupsList.Add(tCombinedAll)

                    i = 0
                    For Each tGrp As clsCombinedGroup In groups
                        'tGrp.ApplyRules()
                        'sGrpVals += String.Format(",'g{0}':{1}", i, Bool2JS(tGrp.UsersList.Contains(tPrjUser)))
                        sGrpVals += String.Format(",'g{0}':{1}", i, Bool2JS(gUsers.ContainsKey(tGrp.ID) AndAlso tPrjUser IsNot Nothing AndAlso gUsers(tGrp.ID).Contains(tPrjUser.UserID))) ' D7405 + D7415
                        i += 1
                    Next

                    sRes += CStr(IIf(sRes = "", "", ",")) + String.Format("{{""id"":{0},""email"":'{1}',""name"":'{2}',""ahp_id"":{3},""role"":'{4}',""priority"":{5},""has_data"":{6},""dis"":{7},""has_psw"":{8},""link"":'{9}',""grp_id"":{10},""pm"":{11},""can_edit"":{12},""link_enabled"":{13},""linkgo_enabled"":{14},""preview_enabled"":{15},""last_judg"":'{16}',""last_judg_sort"":{17},""total"":{18},""made"":{19},""progress"":'{20}',""locked"":{23},""adm"":{24}{21}{22}}}", tUser.UserID, JS_SafeString(tUser.UserEmail), JS_SafeString(sName), AHPUserID, sRole, JS_SafeNumber(tWeight), Bool2Num(madeCount > 0), Bool2JS(fIsDisabled > 0), Bool2Num(isSSO() Or tUser.HasPassword), sPrjLink, tGrpID, Bool2Num(fCanBePM), Bool2Num(Not tUser.CannotBeDeleted AndAlso fIsDisabled <> 2), Bool2JS(IsLinkEnabled AndAlso fIsDisabled <> 2), Bool2JS(IsLinkGoEnabled AndAlso fIsDisabled <> 2), Bool2JS(IsPreviewLinkEnabled), If(Not madeCount > 0 OrElse userLastJudgmentTime = VERY_OLD_DATE, "", userLastJudgmentTime.ToString), userLastJudgmentTime.Ticks, totalCount, madeCount, JS_SafeString(sEvaluationProgress), sAttrVals, sGrpVals, Bool2JS(tUser.PasswordStatus >= _DEF_PASSWORD_ATTEMPTS), Bool2JS(tUser.CannotBeDeleted))  ' D7146 + D7281
                End If
            End If
        Next

        Return String.Format("[{0}]", sRes)
    End Function

    Public Function GetGroupsData() As String
        Dim sRes As String = ""
        For Each tGrp As clsRoleGroup In App.ActiveWorkgroup.RoleGroups
            Dim fCanEdit As Boolean = tGrp.ActionStatus(ecActionType.at_mlModifyModelHierarchy) = ecActionStatus.asGranted
            Dim fCanView As Boolean = tGrp.ActionStatus(ecActionType.at_mlViewModel) = ecActionStatus.asGranted
            Dim fCanEval As Boolean = tGrp.ActionStatus(ecActionType.at_mlEvaluateModel) = ecActionStatus.asGranted
            sRes += String.Format("{0}[{1},'{2}',{3},{4},{5}]", IIf(sRes = "", "", ","), JS_SafeNumber(tGrp.ID), JS_SafeString(tGrp.Name), Bool2Num(fCanEdit), Bool2Num(fCanView), Bool2Num(fCanEval))
        Next
        Return String.Format("[{0}]", sRes)
    End Function

    'A1153 ===
    Public Function GetResultGroupsData() As String
        Dim sRes As String = ""
        If (CurrentPageID = _PGID_PROJECT_USERS OrElse CurrentPageID = _PGID_RISK_INVITE_USERS) AndAlso PM IsNot Nothing Then
            For Each tGrp As clsCombinedGroup In PM.CombinedGroups.GroupsList.Where(Function(g) CType(g, clsCombinedGroup).CombinedUserID <> COMBINED_USER_ID)
                tGrp.ApplyRules()
                Dim sGrpUsers As String = ""
                For Each tUser As clsUser In tGrp.UsersList
                    sGrpUsers += CStr(IIf(sGrpUsers = "", "", ",")) + tUser.UserID.ToString
                Next
                sRes += CStr(IIf(sRes = "", "", ",")) + String.Format("[{0},'{1}',[{2}],{3}]", JS_SafeNumber(tGrp.ID), JS_SafeString(tGrp.Name), sGrpUsers, CStr(IIf(String.IsNullOrEmpty(tGrp.Rule), "0", "1")))
            Next
        End If
        Return String.Format("[{0}]", sRes)
    End Function
    'A1153 ==

    ' -D6363
    'Public Function CanChangeProjectOnlineStatus(Optional tProject As clsProject = Nothing) As Boolean
    '    If tProject Is Nothing Then tProject = App.ActiveProject
    '    Dim sError As String = ""
    '    Return tProject IsNot Nothing AndAlso tProject.ProjectStatus = ecProjectStatus.psActive AndAlso
    '           Not tProject.isMarkedAsDeleted AndAlso Not tProject.isTeamTimeImpact AndAlso Not tProject.isTeamTimeLikelihood AndAlso
    '           Not IsReadOnly AndAlso App.CanChangeProjectOnlineStatus(tProject, sError)
    'End Function

#Region "Filtering by participant attributes"
    'A1157 ===
    ReadOnly Property SESSION_FILTER_RULES As String
        Get
            Return String.Format("ParticipantsFilterRules_{0}", App.ProjectID.ToString)
        End Get
    End Property

    Public ReadOnly Property ActiveProjectHasParticipantAttributes As Boolean
        Get
            Dim AttributesList = PM.Attributes.GetUserAttributes().Where(Function(attr) Not attr.IsDefault).ToList
            If App IsNot Nothing AndAlso PRJ IsNot Nothing AndAlso PM IsNot Nothing AndAlso AttributesList IsNot Nothing Then
                For Each attr In AttributesList
                    If attr.Type = CurrentAttributesType AndAlso Not attr.IsDefault Then Return True
                Next
            End If
            Return False
        End Get
    End Property

    'Private _IsFilterApplied As Boolean = True
    'Public Property IsFilterApplied As Boolean
    '    Get
    '        Return _IsFilterApplied
    '    End Get
    '    Set(value As Boolean)
    '        _IsFilterApplied = value
    '    End Set
    'End Property

    Private _FilterCombination As FilterCombinations = FilterCombinations.fcAnd
    Public Property FilterCombination As FilterCombinations
        Get
            Return _FilterCombination
        End Get
        Set(value As FilterCombinations)
            _FilterCombination = value
        End Set
    End Property

    ReadOnly Property SESSION_USERS_FILTER_RULES As String
        Get
            Return String.Format("SynthesisUserFilterRulesAlts_{0}", App.ProjectID.ToString)
        End Get
    End Property

    Private _CurrentUsersFiltersList As List(Of clsFilterItem) = Nothing
    Public Property CurrentUsersFiltersList As List(Of clsFilterItem)
        Get
            If _CurrentUsersFiltersList Is Nothing Then
                Dim tSessVar = Session(SESSION_USERS_FILTER_RULES)
                If tSessVar Is Nothing OrElse Not (TypeOf tSessVar Is List(Of clsFilterItem)) Then
                    _CurrentUsersFiltersList = New List(Of clsFilterItem)
                    Session.Add(SESSION_USERS_FILTER_RULES, _CurrentUsersFiltersList)
                Else
                    _CurrentUsersFiltersList = CType(tSessVar, List(Of clsFilterItem))
                End If
            End If
            Return _CurrentUsersFiltersList
        End Get
        Set(value As List(Of clsFilterItem))
            _CurrentUsersFiltersList = value
            Session(SESSION_USERS_FILTER_RULES) = value
        End Set
    End Property

    Private ReadOnly Property CurrentAttributesType As AttributeTypes
        Get
            Return AttributeTypes.atUser
        End Get
    End Property

    Public Function GetAttribData() As String
        Dim sList As String = "[]"
        If (CurrentPageID = _PGID_PROJECT_USERS OrElse CurrentPageID = _PGID_RISK_INVITE_USERS) AndAlso PM IsNot Nothing Then
            sList = ""
            Dim AttributesList = PM.Attributes.GetUserAttributes().Where(Function(attr) Not attr.IsDefault).ToList

            With PM
                Dim fIsEmpty As Boolean = CurrentUsersFiltersList.Count = 0
                For Each tAttr As clsAttribute In AttributesList
                    If Not tAttr.IsDefault AndAlso tAttr.Type = CurrentAttributesType Then
                        Dim sVals As String = ""
                        If tAttr.ValueType = AttributeValueTypes.avtEnumeration OrElse tAttr.ValueType = AttributeValueTypes.avtEnumerationMulti Then
                            Dim tVals As clsAttributeEnumeration = .Attributes.GetEnumByID(tAttr.EnumID)
                            If tVals IsNot Nothing Then
                                For Each tVal As clsAttributeEnumerationItem In tVals.Items
                                    sVals += CStr(IIf(sVals = "", "", ",")) + String.Format("['{0}','{1}']", tVal.ID, tVal.Value)
                                Next
                            End If
                        Else
                            If tAttr.DefaultValue IsNot Nothing Then
                                Select Case tAttr.ValueType
                                    Case AttributeValueTypes.avtBoolean
                                        sVals = CStr(IIf(CBool(tAttr.DefaultValue), "true", "false"))
                                    Case AttributeValueTypes.avtDouble
                                        sVals = JS_SafeNumber(CDbl(tAttr.DefaultValue))
                                    Case AttributeValueTypes.avtString
                                        sVals = "'" + JS_SafeString(CStr(tAttr.DefaultValue)) + "'"
                                    Case AttributeValueTypes.avtLong
                                        sVals = JS_SafeNumber(CLng(tAttr.DefaultValue))
                                End Select
                            End If
                        End If
                        'Dim userVals As String = ""
                        'For Each attrVal As clsAttributeValue In .Attributes.ValuesList
                        '    If attrVal.AttributeID.Equals(tAttr.ID) Then
                        '        Dim sValue As String = ""
                        '        Select Case tAttr.ValueType
                        '            Case AttributeValueTypes.avtBoolean
                        '                sValue = CStr(IIf(CBool(attrVal.Value), "true", "false"))
                        '            Case AttributeValueTypes.avtDouble
                        '                sValue = JS_SafeNumber(CDbl(attrVal.Value))
                        '            Case AttributeValueTypes.avtString
                        '                sValue = "'" + JS_SafeString(CStr(attrVal.Value)) + "'"
                        '            Case AttributeValueTypes.avtLong
                        '                sValue = JS_SafeNumber(CLng(attrVal.Value))
                        '            Case AttributeValueTypes.avtEnumeration, AttributeValueTypes.avtEnumerationMulti
                        '                ' not implemented
                        '        End Select
                        '        userVals += CStr(IIf(userVals = "", "", ",")) + String.Format("[{0},{1}]", attrVal.UserID, sValue)
                        '    End If
                        'Next
                        'sList += CStr(IIf(sList = "", "", ",")) + String.Format("['{0}','{1}',{2},[{3}],[{4}]]", tAttr.ID, JS_SafeString(tAttr.Name), CInt(tAttr.ValueType), sVals, userVals)
                        sList += CStr(IIf(sList = "", "", ",")) + String.Format("['{0}','{1}',{2},[{3}]]", tAttr.ID, JS_SafeString(tAttr.Name), CInt(tAttr.ValueType), sVals)
                        If fIsEmpty Then CurrentUsersFiltersList.Add(New clsFilterItem With {.IsChecked = False, .SelectedAttributeID = tAttr.ID, .FilterOperation = FilterOperations.Equal})
                    End If
                Next
                ' add Inconsistency item
                'sList += IIf(sList = "", "", ",") + String.Format("['{0}','{1}',{2},[{3}]]", ATTRIBUTE_GROUP_BY_INCONSISTENCY_ID, JS_SafeString(ResString("optGroupByInconsistency")), CInt(AttributeValueTypes.avtDouble), "")
            End With
        End If
        Return sList
    End Function

    Public Function GetUsersFilterData() As String
        Dim sFlt As String = ""
        With PM
            Dim i As Integer = 0
            While i < CurrentUsersFiltersList.Count
                Dim tVal As clsFilterItem = CurrentUsersFiltersList(i)
                Dim sVal As String = ""
                Dim tAttr = .Attributes.GetAttributeByID(tVal.SelectedAttributeID)
                If tAttr IsNot Nothing Then
                    Select Case tAttr.ValueType
                        Case AttributeValueTypes.avtEnumeration
                            sVal = CStr(IIf(tVal.FilterEnumItemID.Equals(Guid.Empty), "", tVal.FilterEnumItemID.ToString))
                        Case AttributeValueTypes.avtEnumerationMulti
                            sVal = ""
                            If tVal.FilterEnumItemsIDs IsNot Nothing Then
                                For Each ID As Guid In tVal.FilterEnumItemsIDs
                                    If sVal.Length > 0 Then sVal += ";"
                                    sVal += ID.ToString
                                Next
                            End If

                        Case Else
                            sVal = tVal.FilterText
                    End Select
                    sFlt += CStr(IIf(sFlt = "", "", ",")) + String.Format("['{0}','{1}',{2},'{3}']", IIf(tVal.IsChecked, 1, 0), tAttr.ID, CInt(tVal.FilterOperation), JS_SafeString(sVal))
                    i += 1
                Else
                    CurrentUsersFiltersList.RemoveAt(i)
                End If
            End While
        End With
        Return sFlt
    End Function
    'A1157 ==

    Private Sub ParseAttributesFilter(sFilter As String, sComb As String)
        CurrentUsersFiltersList.Clear()
        Dim AttributesList = PM.Attributes.GetUserAttributes().Where(Function(attr) Not attr.IsDefault).ToList
        If sFilter <> "" Then
            If sComb <> "" Then
                If sComb = "0" Then FilterCombination = FilterCombinations.fcAnd
                If sComb = "1" Then FilterCombination = FilterCombinations.fcOr
            End If

            Dim sRows() As String = sFilter.Trim.Split(CChar(vbLf))
            For Each sRow As String In sRows
                Dim tVals() As String = sRow.Split((CChar(Flt_Separator)))
                If tVals.Length >= 3 Then
                    Dim sChecked As String = tVals(0)
                    Dim sAttrID As String = tVals(1)
                    Dim sOperID As String = tVals(2)
                    Dim sFilterText As String = ""
                    If tVals.Length >= 4 Then sFilterText = tVals(3)

                    Dim tFilterItem As New clsFilterItem With {.FilterCombination = FilterCombination}
                    Dim tAttrID As Guid = New Guid(sAttrID)

                    With PM
                        For Each tAttr As clsAttribute In AttributesList
                            If tAttr.Type = CurrentAttributesType AndAlso tAttr.ID.Equals(tAttrID) Then
                                tFilterItem.SelectedAttributeID = tAttr.ID
                                Exit For
                            End If
                        Next
                    End With

                    tFilterItem.IsChecked = (sChecked = "1")
                    tFilterItem.FilterOperation = CType(CInt(sOperID), FilterOperations)
                    tFilterItem.FilterText = sFilterText

                    Dim tAttribute = PM.Attributes.GetAttributeByID(tFilterItem.SelectedAttributeID)
                    If tAttribute IsNot Nothing AndAlso tAttribute.Type = CurrentAttributesType AndAlso tAttribute.ValueType = AttributeValueTypes.avtEnumeration Then
                        tFilterItem.FilterEnumItemID = New Guid(sFilterText)
                    End If
                    If tAttribute IsNot Nothing AndAlso tAttribute.Type = CurrentAttributesType AndAlso tAttribute.ValueType = AttributeValueTypes.avtEnumerationMulti Then
                        Dim sGuids As String() = sFilterText.Split(CChar(";"))
                        For Each sGuid In sGuids
                            If sGuid.Length > 0 Then
                                If tFilterItem.FilterEnumItemsIDs Is Nothing Then tFilterItem.FilterEnumItemsIDs = New List(Of Guid)
                                tFilterItem.FilterEnumItemsIDs.Add(New Guid(sGuid))
                            End If
                        Next
                    End If

                    CurrentUsersFiltersList.Add(tFilterItem)
                End If
            Next
        End If
    End Sub

    Public Function IsUserIncludedInFilter(user As ECCore.clsUser, tCurrentFiltersList As List(Of clsFilterItem)) As Boolean
        Dim retVal As Boolean = False

        If user IsNot Nothing AndAlso tCurrentFiltersList IsNot Nothing AndAlso tCurrentFiltersList.Count > 0 Then
            Dim filterOp As FilterCombinations = FilterCombinations.fcAnd
            filterOp = tCurrentFiltersList(0).FilterCombination

            Dim tNoneRuleChecked As Boolean = True

            For Each cfi As clsFilterItem In tCurrentFiltersList
                If cfi.FilterOperation <> FilterOperations.None AndAlso cfi.IsChecked Then
                    tNoneRuleChecked = False

                    Dim isCurrentRulePassed As Boolean = False

                    Dim tAttribute = PM.Attributes.GetAttributeByID(cfi.SelectedAttributeID)
                    Dim ObjValue As Object = PM.Attributes.GetAttributeValue(cfi.SelectedAttributeID, user.UserID)

                    If ObjValue IsNot Nothing Then
                        Select Case tAttribute.ValueType
                            Case ECCore.Attributes.AttributeValueTypes.avtString
                                Dim StrValue As String = CStr(ObjValue).ToLower
                                Select Case cfi.FilterOperation
                                    Case FilterOperations.Contains
                                        If StrValue.Contains(cfi.FilterText.ToString.ToLower) Then isCurrentRulePassed = True
                                    Case FilterOperations.Equal
                                        If StrValue.Trim = cfi.FilterText.ToString.ToLower.Trim Then isCurrentRulePassed = True
                                    Case FilterOperations.NotEqual
                                        If StrValue.Trim <> cfi.FilterText.ToString.ToLower.Trim Then isCurrentRulePassed = True
                                    Case FilterOperations.StartsWith
                                        If StrValue.Trim.StartsWith(cfi.FilterText.ToString.ToLower.Trim) Then isCurrentRulePassed = True
                                End Select
                            Case ECCore.Attributes.AttributeValueTypes.avtBoolean
                                Dim Value As Boolean = CBool(ObjValue)
                                Select Case cfi.FilterOperation
                                    Case FilterOperations.IsTrue
                                        If Value Then isCurrentRulePassed = True
                                    Case FilterOperations.IsFalse
                                        If Not Value Then isCurrentRulePassed = True
                                End Select
                            Case ECCore.Attributes.AttributeValueTypes.avtDouble
                                Dim Value, FilterDouble As Double
                                Value = CDbl(ObjValue)
                                If String2Double(CStr(cfi.FilterText), FilterDouble) Then
                                    Select Case cfi.FilterOperation
                                        Case FilterOperations.Equal
                                            If Value = FilterDouble Then isCurrentRulePassed = True
                                        Case FilterOperations.NotEqual
                                            If Value <> FilterDouble Then isCurrentRulePassed = True
                                        Case FilterOperations.GreaterThan
                                            If Value > FilterDouble Then isCurrentRulePassed = True
                                        Case FilterOperations.GreaterThanOrEqual
                                            If Value >= FilterDouble Then isCurrentRulePassed = True
                                        Case FilterOperations.LessThan
                                            If Value < FilterDouble Then isCurrentRulePassed = True
                                        Case FilterOperations.LessThanOrequal
                                            If Value <= FilterDouble Then isCurrentRulePassed = True
                                    End Select
                                Else
                                    If cfi.FilterOperation = FilterOperations.NotEqual Then isCurrentRulePassed = True
                                End If
                            Case ECCore.Attributes.AttributeValueTypes.avtLong
                                Dim Value, FilterLong As Long
                                Value = CLng(ObjValue)
                                If Long.TryParse(CStr(cfi.FilterText), FilterLong) Then
                                    Select Case cfi.FilterOperation
                                        Case FilterOperations.Equal
                                            If Value = FilterLong Then isCurrentRulePassed = True
                                        Case FilterOperations.NotEqual
                                            If Value <> FilterLong Then isCurrentRulePassed = True
                                        Case FilterOperations.GreaterThan
                                            If Value > FilterLong Then isCurrentRulePassed = True
                                        Case FilterOperations.GreaterThanOrEqual
                                            If Value >= FilterLong Then isCurrentRulePassed = True
                                        Case FilterOperations.LessThan
                                            If Value < FilterLong Then isCurrentRulePassed = True
                                        Case FilterOperations.LessThanOrequal
                                            If Value <= FilterLong Then isCurrentRulePassed = True
                                    End Select
                                Else
                                    If cfi.FilterOperation = FilterOperations.NotEqual Then isCurrentRulePassed = True
                                End If
                            Case ECCore.Attributes.AttributeValueTypes.avtEnumeration
                                Dim tEnumID As Guid = CType(ObjValue, Guid)
                                Select Case cfi.FilterOperation
                                    Case FilterOperations.Equal
                                        If tEnumID.Equals(cfi.FilterEnumItemID) Then isCurrentRulePassed = True
                                    Case FilterOperations.NotEqual
                                        If Not tEnumID.Equals(cfi.FilterEnumItemID) Then isCurrentRulePassed = True
                                End Select
                            Case ECCore.Attributes.AttributeValueTypes.avtEnumerationMulti
                                Dim tEnumIDs As String = CStr(ObjValue)
                                Select Case cfi.FilterOperation
                                    Case FilterOperations.Contains
                                        If Not String.IsNullOrEmpty(tEnumIDs) AndAlso cfi.FilterEnumItemsIDs IsNot Nothing AndAlso cfi.FilterEnumItemsIDs.Count > 0 Then
                                            Dim tContainsAll As Boolean = cfi.FilterEnumItemsIDs.Count > 0
                                            For Each value In cfi.FilterEnumItemsIDs
                                                If Not tEnumIDs.Contains(value.ToString) Then
                                                    tContainsAll = False
                                                    Exit For
                                                End If
                                            Next
                                            If tContainsAll Then isCurrentRulePassed = True
                                        End If
                                    Case FilterOperations.Equal
                                        If Not String.IsNullOrEmpty(tEnumIDs) AndAlso cfi.FilterEnumItemsIDs IsNot Nothing AndAlso cfi.FilterEnumItemsIDs.Count > 0 Then
                                            Dim tEqual As Boolean = cfi.FilterEnumItemsIDs.Count > 0 AndAlso cfi.FilterEnumItemsIDs.Count = tEnumIDs.Split(CChar(";")).Count
                                            For Each value In cfi.FilterEnumItemsIDs
                                                If Not tEnumIDs.Contains(value.ToString) Then
                                                    tEqual = False
                                                    Exit For
                                                End If
                                            Next
                                            If tEqual Then isCurrentRulePassed = True
                                        End If
                                    Case FilterOperations.NotEqual
                                        If cfi.FilterEnumItemsIDs IsNot Nothing AndAlso cfi.FilterEnumItemsIDs.Count > 0 Then
                                            If Not String.IsNullOrEmpty(tEnumIDs) Then
                                                Dim tEqual As Boolean = cfi.FilterEnumItemsIDs.Count > 0 AndAlso cfi.FilterEnumItemsIDs.Count = tEnumIDs.Split(CChar(";")).Count
                                                For Each value In cfi.FilterEnumItemsIDs
                                                    If Not tEnumIDs.Contains(value.ToString) Then
                                                        tEqual = False
                                                        Exit For
                                                    End If
                                                Next
                                                If Not tEqual Then isCurrentRulePassed = True
                                            Else
                                                isCurrentRulePassed = True
                                            End If
                                        Else
                                            If Not String.IsNullOrEmpty(tEnumIDs) Then isCurrentRulePassed = True
                                        End If
                                End Select
                        End Select
                    End If

                    'apply set
                    Dim indexOfFirstCheckedRule As Integer = 0
                    For i As Integer = 0 To tCurrentFiltersList.Count - 1
                        If tCurrentFiltersList(i).IsChecked Then
                            indexOfFirstCheckedRule = i
                            Exit For
                        End If
                    Next

                    If tCurrentFiltersList.IndexOf(cfi) = indexOfFirstCheckedRule Then
                        If isCurrentRulePassed Then retVal = True
                    Else
                        Select Case filterOp
                            Case FilterCombinations.fcAnd
                                If Not isCurrentRulePassed Then retVal = False
                            Case FilterCombinations.fcOr
                                If isCurrentRulePassed Then retVal = True
                        End Select
                    End If
                    filterOp = cfi.FilterCombination
                End If
            Next

            If tNoneRuleChecked Then retVal = True
        Else
            retVal = True
        End If

        Return retVal
    End Function

    Private Function GetCurrentFilterRuleXML() As String
        Dim tCurrentFiltersList = CurrentUsersFiltersList
        If tCurrentFiltersList.Count > 0 Then

            Dim xDoc As XDocument = <?xml version="1.0" encoding="utf-8" standalone="yes"?>
                                    <!--FilterCombination values - 0 = AND, 1 = OR -->
                                    <Root>
                                        <Settings FilterCombination=""></Settings>
                                        <Rules></Rules>
                                    </Root>

            xDoc.Root.Element("Settings").Attribute("FilterCombination").Value = CInt(tCurrentFiltersList(0).FilterCombination).ToString

            For Each cfi As clsFilterItem In tCurrentFiltersList
                If cfi.IsChecked AndAlso Not cfi.SelectedAttributeID.Equals(Guid.Empty) Then
                    Dim tAttribute As clsAttribute = PM.Attributes.GetAttributeByID(cfi.SelectedAttributeID)
                    If tAttribute IsNot Nothing Then
                        Dim fOperation As Integer = CInt(cfi.FilterOperation)
                        Dim FilterOperationName = [Enum].GetName(GetType(FilterOperations), fOperation)
                        Dim FilterOperationID As Integer = Convert.ToInt32(fOperation)
                        Dim FilterText As String = ""
                        If cfi.FilterText IsNot Nothing Then FilterText = cfi.FilterText.Trim '.ToLower
                        If tAttribute.Name IsNot Nothing AndAlso tAttribute.Name.Trim <> "" Then
                            Dim el As XElement = New XElement("Rule", New XAttribute("AttributeID", tAttribute.ID.ToString), New XAttribute("AttributeName", tAttribute.Name), New XAttribute("OperationName", FilterOperationName), New XAttribute("OperationID", FilterOperationID), New XAttribute("FilterText", FilterText))
                            xDoc.Root.Element("Rules").Add(el)
                        End If
                    End If
                End If
            Next

            Dim sb As StringBuilder = New StringBuilder()
            Dim xws As XmlWriterSettings = New XmlWriterSettings()
            xws.OmitXmlDeclaration = False
            xws.Indent = True

            Using xw = XmlWriter.Create(sb, xws)
                xDoc.WriteTo(xw)
            End Using
            Return sb.ToString
        End If
        Return ""
    End Function

#End Region

#Region "Page events"

    Private Sub ProjectParticipantsPage_PreInit(sender As Object, e As EventArgs) Handles Me.PreInit
        If Not App.isAuthorized Then FetchAccess()  ' D6492
        ' D4755 ===
        Select Case CheckVar("mode", "").ToLower
            Case "controls"
                CurrentPageID = _PGID_RISK_INVITE_USERS
            Case "wgusers"
                CurrentPageID = _PGID_ADMIN_USERSLIST
            Case Else   '"users"
                CurrentPageID = _PGID_PROJECT_USERS
        End Select
        IsWidget = CheckVar("is_widget", False)
        If CurrentPageID <> _PGID_ADMIN_USERSLIST AndAlso Not App.HasActiveProject Then FetchAccess(_PGID_PROJECTSLIST)
        ' D4755 ==
    End Sub

    Private Sub ProjectParticipantsPage_InitComplete(sender As Object, e As EventArgs) Handles Me.InitComplete
        _evalPrgDT = Nothing
        If Session(SESSION_EVAL_PRG_DT) IsNot Nothing Then _evalPrgDT = CDate(Session(SESSION_EVAL_PRG_DT))
        ' D7386 + D7389 ==
        If evalProgress IsNot Nothing AndAlso App.HasActiveProject Then
            If (Not _evalPrgDT.HasValue OrElse _evalPrgDT.Value < PRJ.LastModify OrElse Not PM.LastModifyTime.HasValue OrElse _evalPrgDT.Value < PM.LastModifyTime) Then evalProgress = Nothing
            If evalProgress IsNot Nothing AndAlso _evalPrgDT.HasValue Then
                For Each tUser As clsUser In PM.UsersList
                    If tUser.LastJudgmentTime > _evalPrgDT.Value Then
                        evalProgress = Nothing
                        Exit For
                    End If
                Next
                If evalProgress IsNot Nothing AndAlso App.LastSnapshot IsNot Nothing AndAlso App.LastSnapshot.DateTime > _evalPrgDT.Value Then evalProgress = Nothing
            End If
        End If
        ' D7386 + D7389 ==
        If Not isCallback AndAlso Not IsPostBack Then
            App.CheckProjectManagerUsers(App.ActiveProject)
        End If
        If isAJAX() OrElse Str2Bool(GetParam(Request.Params, "ajax"), False) Then Ajax_Callback(Request.Form.ToString)
    End Sub
#End Region

    Private Sub Ajax_Callback(data As String)
        'Dim args As NameValueCollection = HttpUtility.ParseQueryString(data)
        'Dim sAction As String = CheckVar(_PARAM_ACTION).ToLower()
        'Dim sAction As String = CheckVar(_PARAM_ACTION, "")
        Dim args As NameValueCollection = HttpUtility.ParseQueryString(data)
        Dim sAction As String = GetParam(args, "action").ToLower
        Dim sResult As String = ""

        If Not String.IsNullOrEmpty(sAction) Then
            Dim AttributesList As List(Of clsAttribute) = New List(Of clsAttribute)
            If PM IsNot Nothing AndAlso CurrentPageID <> _PGID_ADMIN_USERSLIST Then AttributesList = PM.Attributes.GetUserAttributes().Where(Function(a) Not a.IsDefault).ToList

            Select Case sAction
                Case "refresh_full"
                    Dim fLoadProgress As Boolean = False
                    If HasParam(args, "load_progress") Then Str2Bool(GetParam(args, "load_progress"), fLoadProgress)
                    sResult = String.Format("['{0}', {1}, {2}]", sAction, GetUsersData(fLoadProgress), GetAttributesData())

                Case "save_changes"
                    Dim N As Integer = CheckVar("n", 0)      ' D7570
                    Dim fPrjChanged As Boolean = False
                    Dim tWeightChanged As Boolean = False
                    'Dim tAttribtuesRead As Boolean = False
                    Dim sMsg As String = ""         ' D7570
                    Dim sEmailsList As String = ""  ' D7570

                    For i As Integer = 0 To N - 1
                        Dim fAttrChanged As Boolean = False

                        Dim sEmail As String = GetParam(args, "key_" + i.ToString)
                        If sEmail <> "" Then
                            'Dim tId As Integer = CInt(CheckVar("key_" + i.ToString, -1))
                            Dim tUser As clsApplicationUser = UserByEmail(sEmail)

                            If CurrentPageID = _PGID_ADMIN_USERSLIST Then
                                If tUser IsNot Nothing AndAlso Not tUser.CannotBeDeleted Then
                                    Dim sNewName As String = GetParam(args, "name_" + i.ToString)
                                    If sNewName <> vbTab.ToString Then
                                        If tUser.UserName <> sNewName Then
                                            tUser.UserName = sNewName
                                            If App.DBUserUpdate(tUser, False, "Update user info (HTML)") Then

                                            End If
                                        End If
                                    End If

                                    Dim sDisabled As String = GetParam(args, "dis_" + i.ToString)
                                    If sDisabled <> "" Then
                                        SetWGUserDisabled(tUser.UserID, Str2Bool(sDisabled))
                                    End If
                                End If
                            Else
                                If tUser IsNot Nothing Then
                                    Dim tPrjUser As clsUser = PM.UsersList.FirstOrDefault(Function(u) (u.UserEMail.ToLower = tUser.UserEmail.ToLower)) 'PM.GetUserByEMail(tUser.UserEmail)
                                    ' D6707 ===
                                    If tPrjUser Is Nothing Then
                                        tPrjUser = PM.AddUser(tUser.UserEmail, True)
                                        sMsg = "Add new participant"   ' D7570
                                        fPrjChanged = True
                                    End If
                                    ' D6707 ==
                                    If tPrjUser IsNot Nothing Then
                                        If args("name_" + i.ToString) IsNot Nothing Then
                                            Dim sNewName As String = GetParam(args, "name_" + i.ToString).Trim()
                                            If sNewName <> tUser.UserName Then
                                                tUser.UserName = sNewName
                                                tPrjUser.UserName = tUser.UserName
                                                sMsg = "Edit participant name" ' D7570
                                                fPrjChanged = True
                                            End If
                                        End If
                                        Dim sDisabled As String = GetParam(args, "dis_" + i.ToString)
                                        If sDisabled <> "" Then
                                            SetPrjUserDisabled(tUser.UserID, Str2Bool(sDisabled))
                                        End If
                                        If args("priority_" + i.ToString) IsNot Nothing Then
                                            Dim tValue As Double
                                            If String2Double(GetParam(args, "priority_" + i.ToString), tValue) AndAlso tPrjUser.Weight <> tValue Then
                                                tPrjUser.Weight = CSng(tValue)
                                                tWeightChanged = True
                                                If sMsg = "" Then sMsg = "Edit participant weight"   ' D7570
                                                fPrjChanged = True
                                            End If
                                        End If
                                        Dim j As Integer = 0
                                        For Each tAttr As clsAttribute In AttributesList
                                            If Not tAttr.IsDefault AndAlso tAttr.Type = CurrentAttributesType Then
                                                If args(String.Format("v{0}_{1}", j, i)) IsNot Nothing Then
                                                    'If Not tAttribtuesRead Then
                                                    '    PM.Attributes.ReadAttributeValues(AttributesStorageType.astStreamsDatabase, PM.StorageManager.ProjectLocation, PM.StorageManager.ProviderType, PM.StorageManager.ModelID, UNDEFINED_USER_ID)
                                                    '    tAttribtuesRead = True
                                                    'End If
                                                    Dim sAttrVal As String = GetParam(args, String.Format("v{0}_{1}", j, i))
                                                    Select Case tAttr.ValueType
                                                        Case AttributeValueTypes.avtString
                                                            PM.Attributes.SetAttributeValue(tAttr.ID, tPrjUser.UserID, AttributeValueTypes.avtString, sAttrVal, Guid.Empty, Guid.Empty)
                                                            fAttrChanged = True
                                                        Case AttributeValueTypes.avtDouble
                                                            Dim tValue As Double
                                                            If String2Double(sAttrVal, tValue) Then
                                                                PM.Attributes.SetAttributeValue(tAttr.ID, tPrjUser.UserID, AttributeValueTypes.avtDouble, tValue, Guid.Empty, Guid.Empty)
                                                                fAttrChanged = True
                                                            End If
                                                        Case AttributeValueTypes.avtLong
                                                            Dim tValue As Long
                                                            If Long.TryParse(sAttrVal, tValue) Then
                                                                PM.Attributes.SetAttributeValue(tAttr.ID, tPrjUser.UserID, AttributeValueTypes.avtLong, tValue, Guid.Empty, Guid.Empty)
                                                                fAttrChanged = True
                                                            End If
                                                        Case AttributeValueTypes.avtBoolean
                                                            PM.Attributes.SetAttributeValue(tAttr.ID, tPrjUser.UserID, AttributeValueTypes.avtBoolean, Str2Bool(sAttrVal), Guid.Empty, Guid.Empty)
                                                            fAttrChanged = True
                                                    End Select
                                                End If
                                                j += 1
                                            End If
                                        Next

                                        If fAttrChanged Then
                                            ' write attribute values
                                            If sMsg = "" Then sMsg = "Edit participant attribute values"    ' D7570
                                            PM.Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, PM.StorageManager.ProjectLocation, PM.StorageManager.ProviderType, PM.StorageManager.ModelID, tPrjUser.UserID)
                                        End If

                                        j = 0
                                        For Each tResGroup As clsCombinedGroup In PM.CombinedGroups.GroupsList.Where(Function(g) CType(g, clsCombinedGroup).CombinedUserID <> COMBINED_USER_ID)
                                            If args(String.Format("g{0}_{1}", j, i)) IsNot Nothing Then
                                                Dim sGrpVal As String = GetParam(args, String.Format("g{0}_{1}", j, i))
                                                If Str2Bool(sGrpVal) Then
                                                    If Not tResGroup.UsersList.Contains(tPrjUser) Then
                                                        tResGroup.UsersList.Add(tPrjUser)
                                                        fPrjChanged = True
                                                        If sMsg = "" Then sMsg = "Edit participant group"   ' D7570
                                                    End If
                                                Else
                                                    If tResGroup.UsersList.Contains(tPrjUser) Then
                                                        tResGroup.UsersList.Remove(tPrjUser)
                                                        fPrjChanged = True
                                                        If sMsg = "" Then sMsg = "Edit participant group"   ' D7570
                                                    End If
                                                End If
                                            End If
                                            j += 1
                                        Next
                                    End If
                                End If
                            End If
                            sEmailsList += If(sEmailsList = "", "", ", ") + sEmail    ' D7570
                        End If
                    Next

                    If tWeightChanged Then
                        If Not PM.PipeParameters.UseWeights OrElse Not PM.CalculationsManager.UseUserWeights Then
                            PM.PipeParameters.UseWeights = True
                            PM.CalculationsManager.UseUserWeights = True
                            'Save settings
                            PRJ.SaveProjectOptions("Auto enable participant weights", , False, "")
                        End If
                    End If

                    If fPrjChanged Then
                        ' save project users
                        'PM.StorageManager.Writer.SaveModelStructure()
                        PRJ.SaveStructure(sMsg,,, sEmailsList)  ' D7570
                    End If

                    sResult = String.Format("['{0}','{1}',{2},{3}]", sAction, JS_SafeString(sResult), GetUsersData(), GetResultGroupsData())
                'Case "edit_name"
                '    Dim UID As Integer = CheckVar("id", -1)
                '    Dim tUser As clsApplicationUser = UserByID(UID)
                '    If tUser IsNot Nothing AndAlso Not tUser.CannotBeDeleted Then
                '        If CurrentPageID = _PGID_ADMIN_USERSLIST Then
                '            Dim sNewName As String = CheckVar("name", tUser.UserName).Trim
                '            If tUser.UserName <> sNewName Then
                '                tUser.UserName = sNewName
                '                If App.DBUserUpdate(tUser, False, "Update user info (HTML)") Then
                '                    sResult = sNewName
                '                End If
                '            End If
                '        Else
                '            Dim tPrjUser As clsUser = clsApplicationUser.AHPUserByUserEmail(tUser.UserEmail, PM.UsersList)
                '            Dim sNewName As String = CheckVar("name", tUser.UserName).Trim
                '            If tPrjUser IsNot Nothing AndAlso tPrjUser.UserName <> sNewName Then
                '                tPrjUser.UserName = sNewName
                '                PM.StorageManager.Writer.SaveModelStructure()
                '            End If
                '            If tPrjUser IsNot Nothing Then sResult = tPrjUser.UserName
                '        End If
                '    End If
                '    If sResult = "" Then sResult = "error"
                '    sResult = String.Format("['{0}','{1}']", sAction, JS_SafeString(sResult))

                Case "disable"
                    Dim UID As Integer = CheckVar("id", -1) ' D7570 
                    Dim tWS As clsWorkspace = WSByUserID(UID)
                    If CurrentPageID = _PGID_ADMIN_USERSLIST Then
                        sResult = Bool2Num(SetWGUserDisabled(UID, CheckVar("dis", False))).ToString     ' D7570
                    Else
                        sResult = Bool2Num(SetPrjUserDisabled(UID, CheckVar("dis", False))).ToString    ' D7570
                    End If
                    sResult = String.Format("['{0}','{1}']", sAction, sResult)

                ' D6658 ===
                Case "userpsw"
                    sResult = ""
                    If isSSO() Then ' D6552
                        sResult = String.Format("['{0}',true,'Not allowed when SSO']", JS_SafeString(sAction)) ' D6552
                    Else
                        Dim fKeepHashes As Boolean = Str2Bool(GetParam(args, "keep_hashes"))    ' D7119
                    Dim UID As Integer = CheckVar("id", -1) ' D7570
                        Dim tUser As clsApplicationUser = UserByID(UID)
                        ' D6659 ===
                        Dim sOldPsw As String = tUser.UserPassword ' D7119
                    If App.SetUserPassword(tUser, GetParam(args, "val"), fKeepHashes, AllowBlankPsw, sResult) AndAlso sResult = "" Then   ' D7119
                            ' D7119 ===
                            If fKeepHashes Then
                                TinyURLUpdateUserPsw(tUser.UserID, sOldPsw, tUser.UserPassword)
                            End If
                            ' D7119 ==
                            App.DBUserUnlock(tUser, tUser.UserPassword, If(tUser.PasswordStatus <= 0, "Set a user new password", "Unlock the user when creating a new password"))    ' D7137
                            If Not isSSO_Only() Then    ' D7432
                                Dim sSubject As String = ParseAllTemplates(ResString("subjReminder"), tUser, PRJ)
                                'Dim sBody As String = ParseAllTemplates(App.ResString("bodyPswReminderPM") + vbCrLf + App.ResString("bodyReminder"), tUser, PRJ)
                                Dim sBody As String = ParseAllTemplates(App.GetPswReminderBodyText(ResString("bodyReminder", False, False), False, App.CanUserDoSystemWorkgroupAction(ecActionType.at_slManageAnyWorkgroup, tUser.UserID)), tUser, PRJ) ' D7327
                                If Not SendMail(WebOptions.SystemEmail, tUser.UserEmail, sSubject, sBody, sResult) Then ' D6397
                                    App.DBSaveLog(dbActionType.actSendEmail, dbObjectType.einfUser, tUser.UserID, "Send e-mail", sResult)
                                    sResult = ""
                                End If
                            End If
                            '' D7357 ===
                            'If tUser.UserID = App.ActiveUser.UserID Then
                            '    Dim tDTSess As DateTime? = App.CheckSessionTerminate(App.ActiveUser)
                            '    If tDTSess.HasValue Then SessVar(_SESS_CMD_TERMINATE) = Date2ULong(tDTSess.Value).ToString
                            'End If
                            '' D7357 ==
                        End If
                            ' D6659 ==
                            sResult = String.Format("['{0}',{1},'{2}']", JS_SafeString(sAction), Bool2Num(tUser.HasPassword), JS_SafeString(sResult)) ' D6397
                    End If
                    ' D6658 ==

                ' D7146 ===
                Case "unlock"
                    Dim fResult As Boolean = False
                    Dim sEmail As String = GetParam(args, "email")
                    Dim tUser As clsApplicationUser = UserByEmail(sEmail)
                    If tUser IsNot Nothing AndAlso tUser.PasswordStatus > 0 Then
                        fResult = App.DBUserUnlock(tUser)
                    End If
                    sResult = String.Format("['{0}','{1}',{2}]", JS_SafeString(sAction), JS_SafeString(tUser.UserEmail), Bool2JS(tUser.PasswordStatus >= _DEF_PASSWORD_ATTEMPTS))
                    ' D7146 ==

                    ' D6562 ===
                Case "reset_psw"
                    If AllowBlankPsw() Then
                        Dim sIDs As String() = CheckVar("lst").Trim.Trim(CChar(",")).Split(CChar(","))
                        For Each sEmail As String In sIDs
                            Dim tUser As clsApplicationUser = UserByEmail(sEmail)
                            If tUser IsNot Nothing Then
                                If Not tUser.CannotBeDeleted AndAlso tUser.HasPassword Then
                                    tUser.UserPassword = ""
                                    If (tUser.PasswordStatus > 0) Then tUser.PasswordStatus = 0
                                    App.DBUserUpdate(tUser, False, String.Format("Reset user password"))
                                End If
                            End If
                        Next
                    Else
                        sResult = "Blank passwords are not allowed"
                    End If
                    sResult = String.Format("['{0}','{1}','{2}']", JS_SafeString(sAction), CheckVar("lst"), JS_SafeString(sResult))
                    ' D6562 ==

                Case "get_link", "open_link", "get_controls_link", "view"   ' D6734
                    Dim sEmail As String = GetParam(args, _PARAM_EMAIL).Trim()   ' Anti-XSS
                    ' D6312 ===
                    Dim tUser As clsApplicationUser = App.DBUserByEmail(sEmail)
                    If tUser IsNot Nothing Then
                        'For Each tUser As clsApplicationUser In UsersList
                        '    If tUser.UserID = ID Then sPrjLink = CreateLogonURL(tUser, PRJ, "pipe=yes", ApplicationURL(False, False))
                        'Next
                        If CurrentPageID <> _PGID_ADMIN_USERSLIST Then
                            Dim isEvalSite As Boolean = False
                            If App.Options.EvalSiteURL <> "" AndAlso App.isEvalURL_EvalSite Then
                                Dim tmpUW As clsUserWorkgroup = UWByUserID(tUser.UserID)
                                Dim tmpWS As clsWorkspace = WSByUserID(tUser.UserID)
                                'isEvalSite = Not App.CanUserModifyProject(tUser.UserID, App.ProjectID, tmpUW, tmpWS, App.ActiveWorkgroup) AndAlso App.CanUserDoProjectAction(ecActionType.at_mlEvaluateModel, tUser.UserID, App.ProjectID, tmpUW, App.ActiveWorkgroup)
                                isEvalSite = Not tUser.CannotBeDeleted AndAlso App.CanUserDoProjectAction(ecActionType.at_mlEvaluateModel, tUser.UserID, App.ProjectID, tmpUW, App.ActiveWorkgroup) ' D6318
                            End If
                            Dim _sEvalURL As String = App.Options.EvalSiteURL
                            If App.Options.EvalSiteURL <> "" AndAlso Not isEvalSite Then
                                App.Options.EvalSiteURL = ""
                            End If
                            Dim isOpen As Boolean = sAction.ToLower.StartsWith("open_")
                            If isOpen Then App.Options.EvalURLPostfix = "&ignoreoffline=yes&ignorepsw=yes"  ' D6364
                            Dim sPrjLink As String = ""
                            If sAction = "get_controls_link" Then
                                sPrjLink = ParseAllTemplates(_TEMPL_URL_EVALUATE_CONTROLS, tUser, App.ActiveProject)
                            Else
                                ' D7456 ===
                                Dim sTpl As String = _TEMPL_URL_EVALUATE
                                Dim sHID As String = CheckVar("hid", "")
                                Select Case sHID
                                    Case CInt(ECHierarchyID.hidLikelihood).ToString
                                        sTpl = _TEMPL_URL_EVALUATE_LIKELIHOOD
                                    Case CInt(ECHierarchyID.hidImpact).ToString
                                        sTpl = _TEMPL_URL_EVALUATE_IMPACT
                                End Select
                                sPrjLink = ParseAllTemplates(sTpl, tUser, App.ActiveProject)
                                ' D7456 ==
                                If sAction.ToLower = "view" AndAlso Not App.isRiskEnabled Then sPrjLink = String.Format("{0}&{1}=yes", sPrjLink, _PARAM_READONLY)  ' D6734
                            End If
                            ' D6363 ===
                            If App.Options.EvalSiteURL <> "" AndAlso sAction.ToLower.StartsWith("open_") Then PrepareProjectForOpenPipe(App.ActiveProject)   ' D6364
                            Dim sError As String = ""
                            sResult = String.Format("['{0}','{1}',{2},{3},{4}]", sAction, JS_SafeString(sPrjLink), Bool2Num(isOpen OrElse (App.ActiveProject.isOnline AndAlso App.ActiveProject.isPublic)), Bool2Num(App.CanChangeProjectOnlineStatus(App.ActiveProject, sError)), Bool2JS(isEvalSite))
                            If isOpen Then App.Options.EvalURLPostfix = ""  ' D6364
                            ' D6363 ==
                            If _sEvalURL <> "" AndAlso Not isEvalSite Then
                                App.Options.EvalSiteURL = _sEvalURL
                            End If
                        Else
                            sResult = String.Format("['{0}','{1}']", sAction, JS_SafeString(ParseAllTemplates(_TEMPL_URL_LOGIN, tUser, App.ActiveProject)))
                        End If
                    End If
                    ' D6312 ==

                    'A1153 ===
                Case "set_prj_online"
                    Dim tPrj = App.ActiveProject
                    tPrj.isOnline = True
                    If Not App.ActiveWorkgroup.License.CheckParameterByID(ecLicenseParameter.MaxProjectsOnline, Nothing, False) Then tPrj.isOnline = False

                    App.DBProjectUpdate(tPrj, False, Trim("Set Project online."))
                    sResult = String.Format("['{0}']", sAction)
                'Case "user_res_group_chk"
                '    Dim uID As Integer = CheckVar("uid", -1)
                '    Dim gID As Integer = CheckVar("gid", -1)
                '    Dim chk As Boolean = CheckVar("chk", False)
                '    If gID <> -1 AndAlso uID <> -1 Then
                '        Dim tGrp = CType(PM.CombinedGroups.GetGroupByCombinedID(gID), ECCore.clsCombinedGroup)
                '        Dim tUser = PM.GetUserByID(uID)
                '        If tGrp IsNot Nothing AndAlso tUser IsNot Nothing Then
                '            Dim fChanged As Boolean = False
                '            Dim msg As String = ""
                '            If Not chk AndAlso tGrp.UsersList.Contains(tUser) Then
                '                tGrp.UsersList.Remove(tUser)
                '                msg = String.Format("Remove user '{1}' from Group '{0}'", tGrp.Name, tUser.UserEMail)
                '                fChanged = True
                '            End If
                '            If chk AndAlso Not tGrp.UsersList.Contains(tUser) Then
                '                tGrp.UsersList.Add(tUser)
                '                msg = String.Format("Add user '{1}' to Group '{0}'", tGrp.Name, tUser.UserEMail)
                '                fChanged = True
                '            End If
                '            If fChanged Then
                '                PRJ.SaveStructure("Update user group", , , msg)
                '                sResult = "ok"
                '            End If
                '        End If
                '    End If
                '    If sResult = "" Then sResult = "error"
                '    sResult = String.Format("['{0}','{1}']", sAction, sResult)
                'Case "show_res_groups"
                '    ShowResultsGroups = Str2Bool((GetParam(args, "chk")))
                '    sResult = "ok"
                '    sResult = String.Format("['{0}','{1}']", sAction, sResult)

                Case "rename_res_group"
                    Dim grpId As Integer = CheckVar("id", -1)
                    Dim sName As String = GetParam(args, "name").Trim
                    Dim grp As clsGroup = PM.CombinedGroups.GetGroupByID(grpId)
                    If grp IsNot Nothing Then
                        grp.Name = sName
                        PRJ.SaveStructure(String.Format("Rename result group '{0}'", ShortString(sName, 40)))
                    End If
                    'sResult = String.Format("[{0}]", GetResultGroupsData())
                    sResult = String.Format("['{0}',{1},{2}]", sAction, "[]", GetResultGroupsData())

                Case "add_res_group"
                    Dim sName As String = GetParam(args, "name").Trim
                    Dim grp As clsCombinedGroup = PM.CombinedGroups.AddCombinedGroup(sName)
                    If grp IsNot Nothing Then
                        PRJ.SaveStructure(String.Format("Add result group '{0}'", ShortString(sName, 40)))
                    End If
                    sResult = String.Format("['{0}',{1},{2}]", sAction, GetUsersData(), GetResultGroupsData())

                Case "del_res_group"
                    Dim grpId As Integer = CheckVar("id", -1)
                    Dim grp As clsGroup = PM.CombinedGroups.GetGroupByID(grpId)
                    If grp IsNot Nothing Then
                        Dim sName As String = grp.Name
                        PM.CombinedGroups.DeleteGroup(grp)
                        PRJ.SaveStructure(String.Format("Remove result group '{0}'", ShortString(sName, 40)))
                    End If
                    'sResult = String.Format("[{0}]", GetResultGroupsData())
                    sResult = String.Format("['{0}',{1},{2}]", sAction, "[]", GetResultGroupsData())

                Case "res_groups_reorder"
                    Dim fChanged As Boolean = False
                    Dim lst As String = GetParam(args, "lst")
                    If Not String.IsNullOrEmpty(lst) Then
                        Dim indices As String() = lst.Split(CChar(","))
                        Dim newOrderList As New List(Of clsGroup)
                        For Each id As String In indices
                            Dim idx As Integer = CInt(id)
                            Dim grpN As clsCombinedGroup = CType(PM.CombinedGroups.GetGroupByID(idx), clsCombinedGroup)
                            newOrderList.Add(grpN)
                            PM.CombinedGroups.GroupsList.Remove(grpN)
                        Next
                        For Each grp As clsGroup In newOrderList
                            PM.CombinedGroups.GroupsList.Add(grp)
                        Next
                        fChanged = True
                    End If
                    If fChanged Then
                        PRJ.SaveStructure("Reorder groups")
                    End If
                    'sResult = String.Format("[{0}]", GetResultGroupsData())
                    sResult = String.Format("['{0}',{1},{2}]", sAction, GetUsersData(), GetResultGroupsData())

                Case "create_dyn_res_group"
                    Dim sName As String = GetParam(args, "name")
                    Dim sFilter As String = GetParam(args, "filter")
                    Dim sComb As String = GetParam(args, "combination")
                    ParseAttributesFilter(sFilter, sComb)
                    Dim NewGroup As clsCombinedGroup = PM.CombinedGroups.AddCombinedGroup(sName)
                    NewGroup.Rule = GetCurrentFilterRuleXML()
                    NewGroup.ApplyRules()
                    PRJ.SaveStructure("Create new result group", , , String.Format("With rule, '{0}'", sName))
                    'sResult = String.Format("[[{0}],[{1}]]", GetUsersData(), GetResultGroupsData())
                    sResult = String.Format("['{0}',{1},{2}]", sAction, GetUsersData(), GetResultGroupsData())

                Case "filter_by_attr"
                    Dim sFilter As String = GetParam(args, "filter")
                    Dim sComb As String = GetParam(args, "combination")
                    ParseAttributesFilter(sFilter, sComb)
                    'sResult = String.Format("[{0}]", GetUsersData())
                    sResult = String.Format("['{0}',{1}]", sAction, GetUsersData())

                Case "remove_users" ' remove participants
                    Dim n As Integer = DoDetachProjectUsers(GetStringList(GetParam(args, "lst")))    ' D7311
                    'sResult = String.Format("['{0}',[],{1}]", sAction, n)
                    sResult = String.Format("['{0}','{1}',{2}]", sAction, JS_SafeString(String.Format(ResString("msgRemovedParticipants"), n)), n)  ' D6431

                Case "remove_users_by_judg" ' remove participants
                    Dim sParam As String = GetParam(args, "param")
                    Dim n As Integer = 0

                    Select Case sParam
                        Case "not_comp"
                            n = DetachProjectUsersWhoNotCompletedJudgments()
                        Case "no_judgm"
                            n = DetachProjectUsersWhoHaveNoJudgments()
                    End Select

                    sResult = String.Format("['{0}',{1}, {2}]", sAction, GetUsersData(), n)
                Case "erase_judgments"
                    Dim sOptions As String = (GetParam(args, "options"))
                    Dim lst As List(Of String) = GetStringList((GetParam(args, "lst")))
                    Dim n As Integer
                    If PM.IsRiskProject Then
                        Dim L As Boolean = String.IsNullOrEmpty(sOptions) OrElse sOptions.Length <> 5 OrElse sOptions(0) = "1"
                        Dim I As Boolean = String.IsNullOrEmpty(sOptions) OrElse sOptions.Length <> 5 OrElse sOptions(2) = "1"
                        Dim C As Boolean = String.IsNullOrEmpty(sOptions) OrElse sOptions.Length <> 5 OrElse sOptions(4) = "1"
                        Dim LMode As Char = CChar("2")
                        Dim IMode As Char = CChar("2")
                        If sOptions.Length = 5 Then
                            LMode = sOptions(1)
                            IMode = sOptions(3)
                        End If
                        n = EraseJudgments(lst, False, 0, L, Val(LMode), I, Val(IMode), C)
                    Else
                        Dim Mode As Integer = 2
                        'If Not String.IsNullOrEmpty(sOptions) AndAlso sOptions.Length > 1 Then 'AS/16986
                        If Not String.IsNullOrEmpty(sOptions) AndAlso sOptions.Length > 0 Then 'AS/16986
                            Mode = Val(sOptions(0))
                        End If
                        n = EraseJudgments(lst, True, Mode, False, 0, False, 0, False)
                    End If
                    sResult = String.Format("['{0}', {1}, {2}]", sAction, GetUsersData(evalProgress IsNot Nothing), n)

                Case "copy_judgments"
                    Dim n As Integer = 0
                    Dim fromID As Integer
                    Dim tMode As Integer
                    Dim toIDs = GetIntegerList((GetParam(args, "to")))
                    If Integer.TryParse(GetParam(args, "from"), fromID) AndAlso Integer.TryParse(GetParam(args, "mode"), tMode) Then
                        Dim CopyMode As ECCore.ECTypes.CopyJudgmentsMode = CType(tMode, ECCore.ECTypes.CopyJudgmentsMode)
                        Dim pwOnly As Boolean = Str2Bool(GetParam(args, "pw_only"))

                        Dim fromUser As clsUser = PM.GetUserByID(fromID)
                        If fromUser IsNot Nothing Then
                            For Each toUserID As Integer In toIDs
                                Dim toUser As clsUser = PM.GetUserByID(toUserID)
                                If toUser IsNot Nothing Then
                                    If CurrentPageID = _PGID_RISK_INVITE_USERS Then
                                        If PM.CopyUserJudgmentsForControls(fromUser.UserEMail, toUser.UserEMail, CopyMode, pwOnly) Then
                                            n += 1
                                        End If
                                    Else
                                        If PM.CopyUserJudgments2(fromUser.UserEMail, toUser.UserEMail, CopyMode, pwOnly, False) Then
                                            n += 1
                                        End If
                                    End If
                                End If
                            Next
                        End If
                    End If
                    sResult = String.Format("['{0}', {1}, {2}]", sAction, GetUsersData(evalProgress IsNot Nothing), n)

                Case "user_state"
                    Dim n As Integer = SetUsersState(GetStringList(GetParam(args, "lst")), Not Str2Bool(GetParam(args, "chk")))
                    sResult = String.Format("['{0}', {1}, {2}]", sAction, "[]", n)

                Case "set_user_role"
                    Dim UserPermission As ecRoleGroupType = ecRoleGroupType.gtEvaluator
                    Dim lst = GetStringList(GetParam(args, "lst"))
                    Select Case GetParam(args, "value")
                        Case "0"
                            UserPermission = ecRoleGroupType.gtEvaluator
                        Case "1"
                            UserPermission = ecRoleGroupType.gtProjectManager
                        Case "2"
                            UserPermission = ecRoleGroupType.gtViewer
                        Case "3"
                            UserPermission = ecRoleGroupType.gtEvaluatorAndViewer
                    End Select

                    Dim WMGroupIDs As New List(Of Integer)
                    For Each tRoleGroup In App.ActiveWorkgroup.RoleGroups
                        If tRoleGroup.GroupType = ecRoleGroupType.gtAdministrator OrElse tRoleGroup.GroupType = ecRoleGroupType.gtWorkgroupManager Then WMGroupIDs.Add(tRoleGroup.ID)
                    Next

                    'Dim tUWList As List(Of clsUserWorkgroup) = App.DBUserWorkgroupsByWorkgroupID(App.ActiveWorkgroup.ID)   ' -D4752
                    Dim OnlyBePM As String = ""
                    Dim OnlyBePMList As New List(Of Integer)
                    Dim CantBePM As String = ""
                    Dim _CantBePMList As New List(Of Integer)
                    Dim list As New List(Of Integer)
                    For Each tUser In UsersList
                        Dim tUW As clsUserWorkgroup = clsUserWorkgroup.UserWorkgroupByUserIDAndWorkgroupID(tUser.UserID, App.ActiveWorkgroup.ID, UWFullList)    ' D4752
                        Dim CanBePM As Boolean = App.CanUserBePM(App.ActiveWorkgroup, tUser.UserID, App.ActiveProject, True, True)
                        If lst.Contains(tUser.UserEmail) Then
                            If UserPermission = ecRoleGroupType.gtProjectManager AndAlso Not CanBePM Then
                                If _CantBePMList.Count < 10 Then CantBePM += String.Format(" • {0}<br/>", tUser.UserEmail)
                                If _CantBePMList.Count = 10 Then CantBePM += "..."
                                _CantBePMList.Add(tUser.UserID)
                            Else
                                If UserPermission <> ecRoleGroupType.gtProjectManager AndAlso WMGroupIDs.IndexOf(tUW.RoleGroupID) >= 0 Then
                                    If OnlyBePMList.Count < 10 Then OnlyBePM += String.Format(" • {0}<br/>", tUser.UserEmail)
                                    If OnlyBePMList.Count = 10 Then OnlyBePM += "..." ' D3094
                                    OnlyBePMList.Add(tUser.UserID)
                                Else
                                    list.Add(tUser.UserID)
                                End If
                            End If
                        End If
                    Next

                    Dim sMsg As String = ""

                    If OnlyBePMList.Count > 0 Then
                        sMsg = String.Format(ResString("msgCantResetPM"), OnlyBePM)
                    End If

                    Dim proceedSetPMs As Boolean = False

                    CantBePMList = _CantBePMList
                    If _CantBePMList.Count > 0 Then
                        proceedSetPMs = True
                        sMsg = String.Format(ResString("msgAskSetPM"), CantBePM)
                    End If

                    Dim n As Integer = 0
                    If list.Count > 0 Then n = App.UpdateUserRoleGroup(list, UserPermission, UsersList, WSList, UWPrjList)

                    If App.ApplicationError.Status = ecErrorStatus.errWrongLicense Then
                        sMsg = App.ApplicationError.Message
                        App.ApplicationError.Reset()
                    End If

                    'sResult = String.Format("[[{0}], {1}, {2}, '{3}']", GetUsersData(), n, CInt(IIf(sMsg.Length > 0, 1, 0)), JS_SafeString(sMsg))
                    sResult = String.Format("['{0}', {1}, {2}, {3}, '{4}',{5}]", sAction, GetUsersData(), n, CInt(IIf(sMsg.Length > 0, 1, 0)), JS_SafeString(sMsg), Bool2JS(proceedSetPMs))
                    ' sResult format : [[0], 1, 2, '3'], where 0 = UsersData, 1 = number of users updated, 2 = is need to show message (0/1), 3 = text message
                Case "set_pms"
                    Dim sMsg As String = ""

                    Dim n As Integer = 0
                    If CantBePMList.Count > 0 Then
                        Dim UserPermission As Integer = -1
                        For Each tWkgGrp As clsRoleGroup In App.ActiveWorkgroup.RoleGroups
                            If tWkgGrp.RoleLevel = ecRoleLevel.rlApplicationLevel AndAlso tWkgGrp.GroupType = ecRoleGroupType.gtProjectOrganizer Then
                                UserPermission = tWkgGrp.ID
                                Exit For
                            End If
                        Next
                        n = UpdateWorkgroupUsersGroupIDForProject(CantBePMList, UserPermission, True).Count

                        UserPermission = -1
                        For Each tWkgGrp As clsRoleGroup In App.ActiveWorkgroup.RoleGroups
                            If tWkgGrp.RoleLevel = ecRoleLevel.rlModelLevel AndAlso tWkgGrp.GroupType = ecRoleGroupType.gtProjectManager Then
                                UserPermission = tWkgGrp.ID
                                Exit For
                            End If
                        Next
                        App.UpdateUserRoleGroup(CantBePMList, ecRoleGroupType.gtProjectManager, UsersList, WSList, UWPrjList)
                    End If

                    If App.ApplicationError.Status = ecErrorStatus.errWrongLicense Then
                        sMsg = App.ApplicationError.Message
                        App.ApplicationError.Reset()
                    End If

                    sResult = String.Format("['{0}', {1}, {2}, {3}, '{4}', false]", "set_user_role", GetUsersData(), n, CInt(IIf(sMsg.Length > 0, 1, 0)), JS_SafeString(sMsg))
                Case "get_workgroup_users"
                    sResult = String.Format("['{0}', {1}]", sAction, GetWorkgroupUsersForProject())

                Case "add_clipboard_users"
                    Dim sData As String = GetParam(args, "data")
                    Dim sLines As String() = sData.Split(Chr(10))
                    Dim fGeneratePassword As Boolean = Str2Bool(GetParam(args, "generate_pass"))
                    Dim fSendMail As Boolean = Str2Bool(GetParam(args, "send_mail"))
                    If CurrentPageID = _PGID_ADMIN_USERSLIST Then
                        ' D2783 ===
                        Dim tGrpID As Integer = -1

                        ' D3296 ===
                        If App.ActiveWorkgroup.Status = ecWorkgroupStatus.wsSystem Then
                            For Each tRole As clsRoleGroup In App.ActiveWorkgroup.RoleGroups
                                If tRole.RoleLevel = ecRoleLevel.rlSystemLevel AndAlso tRole.GroupType = ecRoleGroupType.gtTechSupport Then
                                    tGrpID = tRole.ID
                                    Exit For
                                End If
                            Next
                        Else
                            For Each tRole As clsRoleGroup In App.ActiveWorkgroup.RoleGroups
                                If tRole.RoleLevel = ecRoleLevel.rlApplicationLevel AndAlso tRole.GroupType = ecRoleGroupType.gtUser Then
                                    tGrpID = tRole.ID
                                    Exit For
                                End If
                            Next
                        End If
                        ' D3296 ==
                        sResult = AttachWorkgroupUsers(sAction, sLines, tGrpID, Not fGeneratePassword, fSendMail)   ' D6561
                    Else
                        Dim tGrpId As Integer = Integer.MinValue
                        Integer.TryParse(GetParam(args, "add_to_group"), tGrpId)
                        sResult = AttachUsers("add_users", sLines.ToList, fGeneratePassword, fSendMail, tGrpId)
                    End If

                Case "add_users"
                    Dim N As Integer = CInt(GetParam(args, "count"))
                    ' D3954 ==
                    Dim UserData As New List(Of String)
                    For i As Integer = 1 To N
                        Dim emailN As String = HttpUtility.UrlDecode(GetParam(args, "mail" + i.ToString))
                        Dim nameN As String = HttpUtility.UrlDecode(GetParam(args, "name" + i.ToString))
                        If Not String.IsNullOrEmpty(emailN) Then UserData.Add(String.Format("{0}{2}{1}", emailN, nameN, vbTab))
                    Next
                    Dim fGeneratePassword As Boolean = Str2Bool(GetParam(args, "generate_pass"))
                    Dim fSendMail As Boolean = Str2Bool(GetParam(args, "send_mail"))
                    Dim tGrpId As Integer = Integer.MinValue
                    Integer.TryParse(GetParam(args, "add_to_group"), tGrpId)

                    sResult = AttachUsers(sAction, UserData, fGeneratePassword, fSendMail, tGrpId) 'A1281
                    ' D3954 ==       

                Case "add_prj_users_from_ad"
                    Dim UserData As New List(Of String)
                    Dim sLst As String = GetParam(args, "lst")
                    Dim Emails As New List(Of String)
                    For Each sEmail As String In sLst.Split(CType(",", Char()))
                        sEmail = sEmail.Trim
                        If sEmail <> "" Then
                            Emails.Add(sEmail)
                        End If
                    Next
                    Dim fGeneratePassword As Boolean = Str2Bool(GetParam(args, "generate_pass"))
                    Dim fSendMail As Boolean = Str2Bool(GetParam(args, "send_mail"))
                    Dim tGrpId As Integer = Integer.MinValue
                    Integer.TryParse(GetParam(args, "add_to_group"), tGrpId)
                    Dim tsessLDAPUsers = sessLDAPUsers
                    For Each sEmail As String In Emails
                        If tsessLDAPUsers.ContainsKey(sEmail) Then
                            Dim tUserToAdd As KeyValuePair(Of String, String) = New KeyValuePair(Of String, String)(sEmail, tsessLDAPUsers(sEmail))
                            UserData.Add(String.Format("{0} {1}", sEmail, tsessLDAPUsers(sEmail), vbTab))
                        End If
                    Next
                    sResult = AttachUsers(sAction, UserData, fGeneratePassword, fSendMail, tGrpId)

                Case "add_from_wg"
                    'A1531 ===
                    Dim WGUsers As Dictionary(Of Integer, clsApplicationUser) = App.DBUsersByWorkgroupID(App.ActiveWorkgroup.ID).ToDictionary(Of Integer, clsApplicationUser)(Function(u As clsApplicationUser) u.UserID, Function(u As clsApplicationUser) u)
                    Dim fGeneratePassword As Boolean = Str2Bool(GetParam(args, "generate_pass"))
                    Dim fSendMail As Boolean = Str2Bool(GetParam(args, "send_mail"))
                    Dim fAllPMs As Boolean = Str2Bool(GetParam(args, "all_pms"))
                    Dim tGrpId As Integer = Integer.MinValue
                    Integer.TryParse(GetParam(args, "add_to_group"), tGrpId)
                    'A1531 ==
                    ' D3954 ===
                    Dim sLst As String = GetParam(args, "lst")
                    Dim IDs As New List(Of String)
                    Dim tID As Integer = -1
                    If sLst = "all" Then
                        For Each wgUserKVP As KeyValuePair(Of Integer, clsApplicationUser) In WGUsers
                            Dim wgUser As clsApplicationUser = wgUserKVP.Value
                            IDs.Add(String.Format("{0}{1}{2}", wgUser.UserEmail, vbTab, wgUser.UserName))
                        Next
                    Else
                        For Each sID As String In sLst.Split(CType(",", Char()))
                            sID = sID.Trim
                            'A1531 ===
                            If sID <> "" AndAlso Integer.TryParse(sID, tID) AndAlso WGUsers.ContainsKey(tID) Then
                                Dim wgUser As clsApplicationUser = WGUsers(tID)
                                IDs.Add(String.Format("{0}{1}{2}", wgUser.UserEmail, vbTab, wgUser.UserName))
                            End If
                            'A1531 ==
                        Next
                    End If
                    sResult = AttachUsers(sAction, IDs, fGeneratePassword, fSendMail, tGrpId, fAllPMs)
                    ' D3954 ==

                Case "add_from_ad"
                    Dim sLst As String = GetParam(args, "lst")
                    Dim Emails As New List(Of String)
                    For Each sEmail As String In sLst.Split(CType(",", Char()))
                        sEmail = sEmail.Trim
                        If sEmail <> "" Then
                            Emails.Add(sEmail)
                        End If
                    Next
                    Dim fGeneratePassword As Boolean = Str2Bool(GetParam(args, "generate_pass"))
                    Dim fSendMail As Boolean = Str2Bool(GetParam(args, "send_mail"))
                    Dim fAllPMs As Boolean = Str2Bool(GetParam(args, "all_pms"))
                    Dim sLines As List(Of String) = New List(Of String)
                    Dim tsessLDAPUsers = sessLDAPUsers
                    For Each sEmail As String In Emails
                        If tsessLDAPUsers.ContainsKey(sEmail) Then
                            Dim tUserToAdd As KeyValuePair(Of String, String) = New KeyValuePair(Of String, String)(sEmail, tsessLDAPUsers(sEmail))
                            sLines.Add(String.Format("{0} {1}", sEmail, tsessLDAPUsers(sEmail)))
                        End If
                    Next
                    Dim tGrpID As Integer = App.ActiveWorkgroup.GetDefaultRoleGroupID(ecRoleLevel.rlModelLevel, ecRoleGroupType.gtUser)
                    If fAllPMs Then tGrpID = App.ActiveWorkgroup.GetDefaultRoleGroupID(ecRoleLevel.rlModelLevel, ecRoleGroupType.gtProjectManager)
                    sResult = AttachWorkgroupUsers(sAction, sLines.ToArray, tGrpID, Not fGeneratePassword, fSendMail)   ' D6561

                Case "set_user_priority"
                    Dim email As String = GetParam(args, "uid")
                    Dim lst As List(Of String) = GetStringList(GetParam(args, "lst"))   ' D7311
                    Dim sValue As String = GetParam(args, "val")
                    Dim Value As Double = 0
                    If String2Double(sValue, Value) Then
                        If lst.Count = 0 Then lst.Add(email)
                        Dim fChanged As Boolean = False

                        For Each user As clsApplicationUser In UsersList
                            If lst.Contains(user.UserEmail) AndAlso user.Weight <> Value Then
                                Dim prjUser As ECCore.clsUser = PM.GetUserByEMail(user.UserEmail)
                                If prjUser IsNot Nothing AndAlso prjUser.Weight <> Value Then
                                    prjUser.Weight = CSng(Value)
                                End If
                                fChanged = True
                            End If
                        Next

                        If fChanged Then
                            PRJ.SaveStructure("Update User Weight")

                            If Not PM.PipeParameters.UseWeights OrElse Not PM.CalculationsManager.UseUserWeights Then
                                PM.PipeParameters.UseWeights = True
                                PM.CalculationsManager.UseUserWeights = True
                                'Save settings
                                PRJ.SaveProjectOptions("Auto enable user weights", , False, "")
                            End If
                        End If
                    End If
                    sResult = String.Format("['{0}', {1}]", sAction, "[]")
                Case "remove_wg_users"
                    Dim lst As List(Of String) = GetStringList(GetParam(args, "lst"))   ' D7311
                    Dim tRemoved As Integer = 0
                    If lst.Count > 0 Then tRemoved = App.DetachWorkgroup(lst)  ' D4718 + D6431
                    sResult = String.Format("['{0}','{1}',{2}]", sAction, JS_SafeString(String.Format(ResString("msgRemovedParticipants"), tRemoved)), tRemoved)  ' D4718
                Case "disable_wg_users", "enable_wg_users"
                    Dim lst As List(Of String) = GetStringList(GetParam(args, "lst"))   ' D7311
                    Dim isDisabled As Boolean = sAction = "disable_wg_users"
                    Dim tUpdated As Integer = 0
                    If lst IsNot Nothing AndAlso App.ActiveWorkgroup IsNot Nothing Then ' D1627
                        'Dim tUWList As List(Of clsUserWorkgroup) = App.DBUserWorkgroupsByWorkgroupID(App.ActiveWorkgroup.ID)   ' -D4752
                        Dim sMessage As String = CStr(IIf(isDisabled, "Disable", "Enable")) + " user in workgroup (HTML)"
                        For Each ID As String In lst
                            Dim usr = UserByEmail(ID)
                            If usr IsNot Nothing Then   ' D7311
                                Dim tUW As clsUserWorkgroup = clsUserWorkgroup.UserWorkgroupByUserIDAndWorkgroupID(usr.UserID, App.ActiveWorkgroup.ID, UWFullList) ' D4752
                                If tUW IsNot Nothing Then
                                    tUW.Status = CType(IIf(isDisabled, ecUserWorkgroupStatus.uwDisabled, ecUserWorkgroupStatus.uwEnabled), ecUserWorkgroupStatus)
                                    If Not App.DBUserWorkgroupUpdate(tUW, False, sMessage) Then tUW = Nothing
                                End If
                                If tUW IsNot Nothing Then tUpdated += 1
                            End If
                        Next
                    End If
                    sResult = String.Format("['{0}','{1}',{2}]", sAction, JS_SafeString(String.Format(ResString("msgUpdatedUsers"), tUpdated)), "[]")

                Case "permission_wg_user"
                    Dim lst As List(Of String) = GetStringList(GetParam(args, "lst"))
                    Dim grpId As Integer = CheckVar("grp_id", -1)
                    sResult = UpdateWorkgroupUsersGroupID(sAction, lst, grpId, True)

                Case "wg_user_decisions"
                    Dim tUserID As Integer = CheckVar("uid", -1)
                    ' D4752 ===
                    'Dim tPrjIDs As List(Of Integer) = GetUserProjectIDs(tUserID)
                    Dim retVal As String = ""
                    Dim sMarkedAsDeleted As String = SafeFormString(ResString("lblMarkedAsDeleted"))    ' D4752
                    Dim tPrjList As List(Of clsProject) = App.DBProjectsByUserID(tUserID)
                    'For Each tPrjID As Integer In tPrjIDs
                    For Each tPrj As clsProject In tPrjList.Where(Function(p) p.WorkgroupID = App.ActiveWorkgroup.ID) 'A1700
                        'Dim tPrj As clsProject = clsProject.ProjectByID(tPrjID, App.ActiveProjectsList)
                        'If tPrj IsNot Nothing Then
                        If tPrj.isMarkedAsDeleted Then
                            'retVal += JS_SafeString(String.Format("<li class='gray' title='{0}'>{1}</li>", sMarkedAsDeleted, SafeFormString(tPrj.ProjectName)))
                        Else
                            retVal += JS_SafeString(String.Format("<li><a href='' onclick='openProject({0}); return false;' class='actions'>{1}</a></li>", tPrj.ID, SafeFormString(tPrj.ProjectName)))
                        End If
                        'End If
                    Next
                    sResult = String.Format("['{0}','{1}']", sAction, JS_SafeString("<div style='overflow: auto; max-height: 400px; max-width: 850px;'><ul type='square'>") + retVal + "</ul></div>")
                    ' D4752 ==

                Case "get_ldap_users"
                    Dim sError As String = ""
                    Dim tUsersArr As String = ""
                    Dim sLDAPPath As String = GetParam(args, "srv")
                    Dim sLDAPDN As String = GetParam(args, "dn")
                    Dim sLDAPPW As String = GetParam(args, "pw")
                    'Dim tList As Dictionary(Of String, String) = GetLDAPUsersList("DAPC", "cn=Manager,dc=maxcrc,dc=com", "secret", "", sError)
                    If CheckLDAPConnection(sLDAPPath, sLDAPDN, sLDAPPW, sError) OrElse sampleResults Then
                        Dim tList As Dictionary(Of String, String) = GetLDAPUsersList(sLDAPPath, sLDAPDN, sLDAPPW, "", sError)
                        If sampleResults Then
                            ' === test data
                            tList = New Dictionary(Of String, String)
                            For i As Integer = 0 To 100
                                tList.Add("user_" + i.ToString, "user_" + i.ToString + "@domain.com")
                            Next
                            sError = ""
                            ' ==
                        End If
                        sessLDAPUsers = tList
                        If tList IsNot Nothing Then
                            Dim tExistingUsers As New HashSet(Of String)
                            For Each usr In UsersList
                                tExistingUsers.Add(usr.UserEmail)
                            Next
                            For Each kvp As KeyValuePair(Of String, String) In tList
                                If Not tExistingUsers.Contains(kvp.Key) Then
                                    tUsersArr += If(tUsersArr = "", "", ",") + String.Format("{{chk:false,email:'{0}',name:'{1}'}}", JS_SafeString(kvp.Key), JS_SafeString(kvp.Value))
                                End If
                            Next
                        End If
                    End If
                    sResult = String.Format("['{0}',[{1}],'{2}']", sAction, tUsersArr, JS_SafeString(sError))

                Case "get_filtered_users_data"
                    Dim sFilter As String = GetParam(args, "filter")
                    Dim sComb As String = GetParam(args, "combination")
                    ParseUsersAttributesFilter(sFilter, sComb)
                    sResult = String.Format("[""{0}"",{1}]", sAction, GetFilteredUsersData())

                Case "rename_column"
                    Dim AttrIndex As Integer = CInt(GetParam(args, "clmn"))
                    Dim NewName As String = GetParam(args, "name").Trim
                    If AttrIndex >= 0 AndAlso AttrIndex < AttributesList.Count AndAlso NewName.Length > 0 Then
                        AttributesList(AttrIndex).Name = NewName
                        Dim attr As clsAttribute = AttributesList(AttrIndex) 'PM.Attributes.GetAttributeByID(AttributesList(AttrIndex).ID)
                        If attr IsNot Nothing Then
                            attr.Name = NewName
                            SaveAttributes(String.Format("Rename column '{0}'", ShortString(NewName, 40, True)))    ' D3790
                        End If
                    End If
                    sResult = String.Format("['{0}',{1},{2}]", sAction, GetAttributesData(), Api.GetProjectSurveysQuestionsList())

                Case "add_column"
                    Dim NewName As String = GetParam(args, "name").Trim
                    Dim NewType As AttributeValueTypes = CType(GetParam(args, "type"), AttributeValueTypes)
                    Dim attr As clsAttribute = PM.Attributes.AddAttribute(Guid.NewGuid(), NewName, AttributeTypes.atUser, NewType, Nothing, False, Guid.Empty)
                    If attr IsNot Nothing Then
                        'try to assign the default value
                        If NewType = AttributeValueTypes.avtString OrElse NewType = AttributeValueTypes.avtLong OrElse NewType = AttributeValueTypes.avtDouble OrElse NewType = AttributeValueTypes.avtBoolean Then
                            Dim DefVal As String = GetParam(args, "def_val").Trim    ' Anti-XSS
                            If Not String.IsNullOrEmpty(DefVal) Then
                                Select Case NewType
                                    Case AttributeValueTypes.avtString
                                        attr.DefaultValue = DefVal
                                    Case AttributeValueTypes.avtLong
                                        Dim tIntVal As Integer
                                        If Integer.TryParse(DefVal, tIntVal) Then attr.DefaultValue = tIntVal
                                    Case AttributeValueTypes.avtDouble
                                        Dim tDblVal As Double
                                        If String2Double(DefVal, tDblVal) Then attr.DefaultValue = tDblVal
                                    Case AttributeValueTypes.avtBoolean
                                        Dim tIntVal As Integer
                                        If Integer.TryParse(DefVal, tIntVal) Then attr.DefaultValue = tIntVal = 1
                                End Select
                            End If
                        End If

                        SaveAttributes(String.Format("Add attribute '{0}'", ShortString(NewName, 40)))    ' D3790
                    End If

                    sResult = String.Format("['{0}',{1},{2}]", sAction, GetAttributesData(), Api.GetProjectSurveysQuestionsList())

                Case "del_column"
                    Dim AttrIndex As Integer = CheckVar("clmn", -1)
                    If AttrIndex >= 0 AndAlso AttrIndex < AttributesList.Count Then
                        Dim attr As clsAttribute = AttributesList(AttrIndex)
                        If attr IsNot Nothing Then
                            Dim sName As String = AttributesList(AttrIndex).Name    ' D3790
                            PM.Attributes.RemoveAttribute(attr.ID)
                            SaveAttributes(String.Format("Delete column '{0}'", ShortString(sName, 40)))    ' D3790
                        End If
                    End If
                    sResult = String.Format("['{0}',{1},{2}]", sAction, GetAttributesData(), Api.GetProjectSurveysQuestionsList())

                Case "set_default_value"
                    Dim AttrIndex As Integer = CheckVar("clmn", -1)
                    Dim value As String = GetParam(args, "def_val").Trim
                    Dim fValueChanged As Boolean = False
                    If AttrIndex >= 0 AndAlso AttrIndex < AttributesList.Count Then
                        Dim attr As clsAttribute = AttributesList(AttrIndex)
                        If attr IsNot Nothing Then
                            Select Case attr.ValueType
                                Case AttributeValueTypes.avtString
                                    If String.IsNullOrEmpty(value) Then
                                        attr.DefaultValue = Nothing
                                    Else
                                        attr.DefaultValue = value
                                    End If
                                    fValueChanged = True
                                Case AttributeValueTypes.avtBoolean
                                    If value = "1" OrElse value = "0" Then
                                        attr.DefaultValue = value = "1"
                                        fValueChanged = True
                                    End If
                                Case AttributeValueTypes.avtLong
                                    If String.IsNullOrEmpty(value) Then
                                        attr.DefaultValue = Nothing
                                        fValueChanged = True
                                    Else
                                        Dim intValue As Integer
                                        If Integer.TryParse(value, intValue) Then
                                            attr.DefaultValue = intValue
                                            fValueChanged = True
                                        End If
                                    End If
                                Case AttributeValueTypes.avtDouble
                                    If String.IsNullOrEmpty(value) Then
                                        attr.DefaultValue = Nothing
                                        fValueChanged = True
                                    Else
                                        Dim dblValue As Double
                                        If String2Double(value, dblValue) Then
                                            attr.DefaultValue = dblValue
                                            fValueChanged = True
                                        End If
                                    End If
                                Case AttributeValueTypes.avtEnumeration
                                    Dim ItemIndex As Integer = CheckVar("item_index", -1)
                                    If value = "1" Then
                                        Dim aEnum As clsAttributeEnumeration = PM.Attributes.GetEnumByID(attr.EnumID)
                                        If aEnum IsNot Nothing Then
                                            If ItemIndex >= 0 AndAlso ItemIndex < aEnum.Items.Count Then
                                                attr.DefaultValue = aEnum.Items(ItemIndex).ID
                                                fValueChanged = True
                                            End If
                                        End If
                                    Else
                                        attr.DefaultValue = Nothing
                                        fValueChanged = True
                                    End If
                                Case AttributeValueTypes.avtEnumerationMulti
                                    'not implemented
                            End Select
                            If fValueChanged Then SaveAttributes(String.Format("Set default for '{0}'", ShortString(App.GetAttributeName(attr), 40))) ' D3790
                        End If
                    End If
                    'sResult = String.Format("['{0}',{1},{2}]", sAction, GetAttributesData(), GetUsersData())
                    sResult = String.Format("['{0}']", sAction)

                Case "attributes_reorder"
                    Dim fChanged As Boolean = False
                    Dim lst As String = GetParam(args, "lst")
                    If Not String.IsNullOrEmpty(lst) Then
                        Dim attrList As List(Of clsAttribute) = PM.Attributes.GetUserAttributes().Where(Function(a) Not a.IsDefault).ToList
                        Dim globalI As New List(Of Integer)
                        For Each attr In attrList
                            globalI.Add(PM.Attributes.AttributesList.IndexOf(attr))
                        Next
                        Dim indices As String() = lst.Split(CChar(","))
                        Dim i As Integer = 0
                        For Each id As String In indices
                            Dim idx As Integer = CInt(id)
                            If idx >= 0 AndAlso idx < attrList.Count AndAlso idx <> i Then
                                Dim attrN As clsAttribute = attrList(idx)
                                PM.Attributes.AttributesList.RemoveAt(globalI(i))
                                PM.Attributes.AttributesList.Insert(globalI(i), attrN)
                                fChanged = True
                            End If
                            i += 1
                        Next
                    End If
                    If fChanged Then
                        SaveAttributes("Reorder attributes")    ' D3790
                    End If
                    sResult = String.Format("['{0}',{1},{2}]", sAction, GetAttributesData(), GetUsersData())

                Case "open_project"
                    Dim ID As Integer = CheckVar("id", -1)

                    Dim tPrj As clsProject = clsProject.ProjectByID(ID, App.ActiveProjectsList)
                    If tPrj IsNot Nothing Then
                        If tPrj.isValidDBVersion OrElse tPrj.isDBVersionCanBeUpdated Then
                            App.ProjectID = tPrj.ID
                        End If
                    End If

                    sResult = String.Format("['{0}']", sAction)

                Case "create_attr_from_survey"
                    Dim QID As String = GetParam(args, "question_id")
                    Dim QName As String = GetParam(args, "name")
                    Dim QIsWelc As Boolean = Str2Bool(GetParam(args, "is_welcome_survey"))
                    Dim CreateGroups As Boolean = Str2Bool(GetParam(args, "create_groups"))
                    Dim QGroups As String = GetParam(args, "groups")
                    Dim AttrGUID As Guid

                    Api.AddUserAttributeFromSurvey(QID, AttrGUID, QName, AttributeValueTypes.avtString, "", QIsWelc)

                    If CreateGroups Then
                        Dim Names As String() = QGroups.Split(CChar(vbTab))
                        If Names.Length > 0 Then
                            Dim idx As Integer = 1   ' D7513
                            Dim Groups As New Dictionary(Of String, String)
                            For Each Name As String In Names
                                If Name.Trim() <> "" Then
                                    ' D7513 ===
                                    Dim sName As String = Name.Trim
                                    If Groups.ContainsKey(sName) Then sName = String.Format("{0}_{1}", sName, idx)
                                    Groups.Add(sName, Api.GetFilterRule(AttrGUID, Name))
                                    idx += 1
                                    ' D7513 ==
                                End If
                            Next

                            If Groups.Count > 0 Then
                                Api.CreateNewResultGroupsWithRules(Groups)
                            End If
                        End If
                    End If

                    sResult = String.Format("['{0}',{1},{2},{3},{4}]", sAction, GetAttributesData(), GetUsersData(), Api.GetProjectSurveysQuestionsList(), GetResultGroupsData())
            End Select
        End If

        'If sResult = "" Then sResult = "empty" ' D3954

        If sAction <> "" Then
            Response.Clear()
            Response.ContentType = "text/plain"
            Response.Write(sResult)
            Response.End()
        End If
    End Sub

    Private Sub ParseUsersAttributesFilter(sFilter As String, sComb As String)
        CurrentUsersFiltersList.Clear()
        If sFilter <> "" Then
            If sComb <> "" Then
                If sComb = "0" Then FilterCombination = FilterCombinations.fcAnd
                If sComb = "1" Then FilterCombination = FilterCombinations.fcOr
            End If

            Dim sRows() As String = sFilter.Trim.Split(CChar(vbLf))
            For Each sRow As String In sRows
                Dim tVals() As String = sRow.Split((CChar(Flt_Separator)))
                If tVals.Length >= 3 Then
                    Dim sChecked As String = tVals(0)
                    Dim sAttrID As String = tVals(1)
                    Dim sOperID As String = tVals(2)
                    Dim sFilterText As String = ""
                    If tVals.Length > 3 Then sFilterText = tVals(3)

                    Dim tFilterItem As New clsFilterItem With {.FilterCombination = FilterCombination}
                    Dim tAttrID As Guid = New Guid(sAttrID)

                    With PM
                        For Each tAttr As clsAttribute In .Attributes.AttributesList
                            If tAttr.Type = AttributeTypes.atUser AndAlso tAttr.ID.Equals(tAttrID) Then
                                tFilterItem.SelectedAttributeID = tAttr.ID
                                Exit For
                            End If
                        Next
                    End With

                    tFilterItem.IsChecked = (sChecked = "1")
                    tFilterItem.FilterOperation = CType(CInt(sOperID), FilterOperations)
                    tFilterItem.FilterText = sFilterText

                    Dim tAttribute = PM.Attributes.GetAttributeByID(tFilterItem.SelectedAttributeID)
                    If tAttribute IsNot Nothing AndAlso tAttribute.Type = AttributeTypes.atUser AndAlso tAttribute.ValueType = AttributeValueTypes.avtEnumeration Then
                        tFilterItem.FilterEnumItemID = New Guid(sFilterText)
                    End If
                    If tAttribute IsNot Nothing AndAlso tAttribute.Type = AttributeTypes.atUser AndAlso tAttribute.ValueType = AttributeValueTypes.avtEnumerationMulti Then
                        Dim sGuids As String() = sFilterText.Split(CChar(";"))
                        For Each sGuid In sGuids
                            If sGuid.Length > 0 Then
                                If tFilterItem.FilterEnumItemsIDs Is Nothing Then tFilterItem.FilterEnumItemsIDs = New List(Of Guid)
                                tFilterItem.FilterEnumItemsIDs.Add(New Guid(sGuid))
                            End If
                        Next
                    End If

                    CurrentUsersFiltersList.Add(tFilterItem)
                End If
            Next
        End If
    End Sub

    Private Sub SaveAttributes(sComment As String)
        PM.Attributes.WriteAttributes(AttributesStorageType.astStreamsDatabase, PM.StorageManager.ProjectLocation, PM.StorageManager.ProviderType, PM.StorageManager.ModelID)
        If sComment <> "" Then App.ActiveProject.SaveRA("Edit participant attributes", , , sComment)
    End Sub

    Public Function GetFilteredUsersData() As String
        Dim sCol As String = ""
        sCol += If(sCol = "", "", ",") + String.Format("['{0}','{1}',{2},{3},'{4}']", "Email Address", "left", Bool2JS(True), Bool2JS(True), "email")
        sCol += If(sCol = "", "", ",") + String.Format("['{0}','{1}',{2},{3},'{4}']", "Participant Name", "left", Bool2JS(True), Bool2JS(True), "name")

        Dim UserAttrList = PM.Attributes.AttributesList.Where(Function(a) a.Type = AttributeTypes.atUser AndAlso a.IsDefault = False)

        Dim i As Integer = 0
        For Each attr As clsAttribute In UserAttrList
            Dim cellClass As String = "left"
            sCol += If(sCol = "", "", ",") + String.Format("['{0}','{1}',{2},{3},'v{4}']", JS_SafeString(attr.Name), cellClass, Bool2JS(True), Bool2JS(True), i)
            i += 1
        Next

        Dim sRes As String = ""
        Dim tCurrentFiltersList = CurrentUsersFiltersList

        Dim tPrjUsers As List(Of ECTypes.clsUser) = PM.UsersList

        For Each tUser As ECTypes.clsUser In tPrjUsers
            Dim tUserVisible As Boolean = True

            If tCurrentFiltersList IsNot Nothing AndAlso tCurrentFiltersList.Count > 0 Then
                If tCurrentFiltersList.Where(Function(r) r.IsChecked AndAlso r.SelectedAttributeID.Equals(DYNAMIC_ATTRIBUTE_GROUP_INCONSISTENCY_ID)).Count > 0 Then
                    'TODO: AC - call a function to update the Inconsistencies for all users
                End If
                tUserVisible = IsUserIncludedInFilter(tUser, tCurrentFiltersList)
            End If

            If tUserVisible Then
                i = 0
                sRes += If(sRes = "", "", ",") + String.Format("{{'email':'{0}','name':'{1}'", JS_SafeString(tUser.UserEMail), JS_SafeString(tUser.UserName))
                For Each attr As clsAttribute In UserAttrList
                    sRes += String.Format(",'v{0}':'{1}'", i, JS_SafeString(PM.Attributes.GetAttributeValueString(attr.ID, Guid.Empty, tUser.UserID)))
                    i += 1
                Next
                sRes += "}"
            End If
        Next

        Return String.Format("[{0}], [{1}]", sCol, sRes)
    End Function

    Private Function GetIntegerList(sLst As String) As List(Of Integer)
        Dim iIDs As New List(Of Integer)
        If sLst = "all" Then
            For Each user As clsApplicationUser In UsersList
                iIDs.Add(user.UserID)
            Next
        Else
            Dim sIDs As String() = sLst.Split(CChar(","))
            For Each id As String In sIDs
                Dim i As Integer
                If Integer.TryParse(id, i) Then iIDs.Add(i)
            Next
        End If
        Return iIDs
    End Function

    Private Function GetStringList(sLst As String) As List(Of String)
        Dim iIDs As New List(Of String)
        If sLst = "all" Then
            For Each user As clsApplicationUser In UsersList
                iIDs.Add(user.UserEmail)
            Next
        Else
            Dim sIDs As String() = sLst.Split(CChar(","))
            For Each id As String In sIDs
                If Not String.IsNullOrEmpty(id.Trim()) Then iIDs.Add(id.Trim())  ' D6431 + D7311
            Next
        End If
        Return iIDs
    End Function

    Public Function GetRoleGroupsMenu() As String
        Dim retVal As String = ""
        If App.ActiveWorkgroup IsNot Nothing Then
            For Each rg As clsRoleGroup In App.ActiveWorkgroup.RoleGroups.Where(Function(grp) App.ActiveWorkgroup.Status = ecWorkgroupStatus.wsSystem OrElse (grp.RoleLevel = ecRoleLevel.rlApplicationLevel AndAlso grp.ID <> ecRoleGroupType.gtECAccountManager)) ' D6887
                retVal += If(retVal = "", "", "," + Environment.NewLine) + String.Format("{{ text: '{0}', onClick: function () {{ onClickToolbar('permission_wg_user', {1}); }} }}", JS_SafeString(ResString(String.Format("lbl_{0}", rg.GroupType.ToString))), rg.ID)  ' D6535
            Next
        End If
        Return retVal
    End Function

    Public Function GetWorkgroupUsers() As String
        If Not App.isAuthorized OrElse App.ActiveWorkgroup Is Nothing Then Return "[]"  ' D6639
        Dim retVal As String = ""

        'Dim WkgUsersList As List(Of clsApplicationUser) = App.DBUsersByWorkgroupID(App.ActiveWorkgroup.ID)
        'Dim tUWList As List(Of clsUserWorkgroup) = App.DBUserWorkgroupsByWorkgroupID(App.ActiveWorkgroup.ID)   ' -D4752

        If App.ActiveWorkgroup.IsOldWorkgroup Then App.CheckPMsWhoIsWkgMember(App.ActiveWorkgroup)

        ' D4752 ===
        Dim tPrjList As New NameValueCollection()
        Dim tPrjCntList As List(Of Dictionary(Of String, Object)) = App.Database.SelectBySQL(String.Format("SELECT COUNT(W.ID) as Cnt, UserID FROM Workspace W LEFT JOIN PROJECTS P ON P.ID=W.ProjectID WHERE P.WorkgroupID={0} AND (P.Status & 2048)=0 GROUP BY W.UserID", App.ActiveWorkgroup.ID))
        For Each tRow As Dictionary(Of String, Object) In tPrjCntList
            If Not IsDBNull(tRow("UserID")) Then
                tPrjList.Add(CStr(tRow("UserID")), CStr(tRow("Cnt")))
            End If
        Next
        ' D4752 ==

        Dbg("Start loading workgroup users: ")
        For Each tUser As clsApplicationUser In UsersList
            Dim sRole As String = ""
            Dim tGrpID As Integer = -1
            Dim tUW As clsUserWorkgroup = clsUserWorkgroup.UserWorkgroupByUserIDAndWorkgroupID(tUser.UserID, App.ActiveWorkgroup.ID, UWFullList)    ' D4752
            If tUW IsNot Nothing Then
                Dim tRole As clsRoleGroup = App.ActiveWorkgroup.RoleGroup(tUW.RoleGroupID, App.ActiveWorkgroup.RoleGroups)
                If tRole IsNot Nothing Then
                    sRole = tRole.Name
                    tGrpID = tRole.ID
                End If
            End If
            Dim tDisabled As Boolean = True
            Dim tDecisions As Integer = 0
            If tUW IsNot Nothing Then
                tDisabled = tUW.Status = ecUserWorkgroupStatus.uwDisabled

                ' D4752 ===
                'Dim Cnt As Object = App.Database.ExecuteScalarSQL(String.Format("SELECT COUNT(W.ID) FROM Workspace W LEFT JOIN PROJECTS P ON P.ID=W.ProjectID WHERE P.WorkgroupID={0} AND W.UserID={1} AND (P.Status & 2048)=0", tUW.WorkgroupID, tUW.UserID))
                'If Not IsDBNull(Cnt) Then tDecisions = CInt(Cnt)
                Dim sCnt As String = tPrjList(tUW.UserID.ToString)
                If Not String.IsNullOrEmpty(sCnt) Then Integer.TryParse(sCnt, tDecisions)
                ' D4752 ==
            End If
            retVal += If(retVal = "", "", ",") + String.Format("{{""id"":{0},""email"":'{1}',""name"":'{2}',""role_id"":{3},""role_name"":'{4}',""decisions"":{5},""dis"":{6},""checked"":{7},""can_edit"":{8},""last_visited"":{9},""has_psw"":{10}}}", tUser.UserID, JS_SafeString(tUser.UserEmail), JS_SafeString(tUser.UserName), tGrpID, JS_SafeString(sRole), tDecisions, Bool2JS(tDisabled), "false", Bool2Num(Not tUser.CannotBeDeleted), If(tUW IsNot Nothing AndAlso tUW.LastVisited.HasValue, JsonConvert.SerializeObject(tUW.LastVisited.Value), """"""), Bool2JS(isSSO() Or tUser.HasPassword))   ' D6024
        Next

        Dbg("End loading workgroup users")

        Return String.Format("[{0}]", retVal)
    End Function

    Private Sub Dbg(info As String)
        Debug.Print(info + "   ( " + Now.ToString("hh:mm:ss.ffff tt") + ")")
    End Sub

    Public Function GetWorkgroupUsersForProject() As String
        Dim retVal As String = ""

        'Dim WkgUsersList As List(Of clsApplicationUser) = App.DBUsersByWorkgroupID(App.ActiveWorkgroup.ID)

        If App.ActiveWorkgroup.IsOldWorkgroup Then App.CheckPMsWhoIsWkgMember(App.ActiveWorkgroup)

        If (App.ActiveUser.CannotBeDeleted OrElse App.CanUserDoAction(ecActionType.at_alManageWorkgroupUsers, App.ActiveUserWorkgroup) OrElse App.CanUserDoAction(ecActionType.at_alCreateNewModel, App.ActiveUserWorkgroup) OrElse App.CanUserDoAction(ecActionType.at_alUploadModel, App.ActiveUserWorkgroup)) Then
            For Each tUser As clsApplicationUser In App.DBUsersByWorkgroupID(App.ActiveWorkgroup.ID)
                If PM.GetUserByEMail(tUser.UserEmail) Is Nothing Then
                    Dim sRole As String = ""
                    Dim tGrpID As Integer = -1
                    'Dim tWS As clsWorkspace = WSByUserID(tUser.UserID)
                    Dim tUW As clsUserWorkgroup = clsUserWorkgroup.UserWorkgroupByUserIDAndWorkgroupID(tUser.UserID, App.ActiveWorkgroup.ID, UWFullList)
                    If tUW IsNot Nothing Then
                        Dim tRole As clsRoleGroup = App.ActiveWorkgroup.RoleGroup(tUW.RoleGroupID, App.ActiveWorkgroup.RoleGroups)
                        If tRole IsNot Nothing Then
                            sRole = tRole.Name
                        End If
                    End If
                    retVal += CStr(IIf(retVal = "", "", ",")) + String.Format("{{chk:{0},email:'{1}',name:'{2}',role:'{3}',id:{4}}}", "false", JS_SafeString(tUser.UserEmail), JS_SafeString(tUser.UserName), JS_SafeString(sRole), tUser.UserID)
                End If
            Next
        End If

        Return String.Format("[{0}]", retVal)
    End Function

    Public Function GetAttributesData() As String
        Dim sAttrs As String = ""
        Dim attrIndex As Integer = 0
        If App.HasActiveProject AndAlso CurrentPageID <> _PGID_ADMIN_USERSLIST Then
            Dim linkedAttributesIDs As List(Of Guid) = Api.GetLinkedUserAttributesIDs()

            For Each attr As clsAttribute In PM.Attributes.GetUserAttributes().Where(Function(a) Not a.IsDefault)
                Dim sValue As String = "''" ' enum attribute items or string/int/double/bool default value
                If attr.DefaultValue IsNot Nothing Then sValue = attr.DefaultValue.ToString()

                Select Case attr.ValueType
                    Case AttributeValueTypes.avtString '0
                        If attr.DefaultValue IsNot Nothing Then sValue = String.Format("'{0}'", JS_SafeString(attr.DefaultValue))
                    Case AttributeValueTypes.avtBoolean '1
                        If attr.DefaultValue IsNot Nothing Then sValue = String.Format("{0}", If(CBool(attr.DefaultValue), 1, 0))
                    Case AttributeValueTypes.avtLong '2
                        If attr.DefaultValue IsNot Nothing Then sValue = String.Format("{0}", JS_SafeNumber(attr.DefaultValue))
                    Case AttributeValueTypes.avtDouble '3
                        If attr.DefaultValue IsNot Nothing Then sValue = String.Format("{0}", JS_SafeNumber(attr.DefaultValue))
                    Case AttributeValueTypes.avtEnumeration, AttributeValueTypes.avtEnumerationMulti '4 - 5
                        ' not implemented
                End Select

                'sAttrs += If(sAttrs = "", "", ",") + String.Format("[{0},'{1}',{2},{3},{4}]", attrIndex, JS_SafeString(App.GetAttributeName(attr)), CInt(attr.ValueType), sValue, Bool2JS(Not attr.ID.Equals(ATTRIBUTE_CONTROL_CATEGORY_ID))) ' Index, Name, ValueType, Default Value (or available enum values), Can attribute be removed
                sAttrs += If(sAttrs = "", "", ",") + String.Format("[{0},'{1}','{2}',{3},{4},{5},{6}]", attrIndex, attr.ID.ToString, JS_SafeString(App.GetAttributeName(attr)), CInt(attr.ValueType), sValue, Bool2JS(Not attr.ID.Equals(ATTRIBUTE_CONTROL_CATEGORY_ID)), Bool2JS(linkedAttributesIDs.Contains(attr.ID))) ' Index, Name, ValueType, Default Value (or available enum values), Can attribute be removed, is read_only
                attrIndex += 1
            Next
        End If
        Return String.Format("[{0}]", sAttrs)
    End Function

#Region "Attach project users"
    Private sAddUsersMsg As String = ""

    Public Function AttachUsers(sAction As String, UsersList As List(Of String), Optional tGenerateRandomPassword As Boolean = False, Optional tSendMailNotification As Boolean = False, Optional tResultGroupID As Integer = Integer.MinValue, Optional fAddAllAsPMs As Boolean = False) As String 'A1281 + A1531
        Dim NewUsers As New List(Of Integer)
        Dim _CantBePMList As New List(Of Integer)
        Dim tUsersLst As List(Of clsApplicationUser) = App.AttachProjectUsers(UsersList, If(fAddAllAsPMs, ecRoleGroupType.gtProjectManager, ecRoleGroupType.gtEvaluator), Not tGenerateRandomPassword, True, False, NewUsers, _CantBePMList) 'A1531

        If tSendMailNotification AndAlso tUsersLst IsNot Nothing AndAlso tUsersLst.Count > 0 AndAlso NewUsers IsNot Nothing Then ' D6492 + D7286
            For Each UserID As Integer In NewUsers
                Dim tUser As clsApplicationUser = clsApplicationUser.UserByUserID(UserID, tUsersLst)
                If tUser IsNot Nothing Then
                    Dim sSubject As String = ParseAllTemplates(App.ResString("subjSignupNotification"), tUser, PRJ) ' App.ResString due to not auto-parse for active user
                    Dim sBody As String = ParseAllTemplates(App.ResString(If(isSSO_Only(), "bodySignupNotificationSSO", "bodySignupNotification")), tUser, PRJ)    ' D4767 + D7432
                    Dim sError As String = ""
                    SendMail(WebOptions.SystemEmail, tUser.UserEmail, sSubject, sBody, sError)
                End If
            Next
        End If

        'A1281 ===
        If tResultGroupID > Integer.MinValue Then
            Dim tGroup As ECCore.Groups.clsCombinedGroup = Nothing
            tGroup = CType(PM.CombinedGroups.GetGroupByID(tResultGroupID), ECCore.clsCombinedGroup)
            If tGroup IsNot Nothing AndAlso NewUsers IsNot Nothing AndAlso NewUsers.Count > 0 Then
                For Each UserID As Integer In NewUsers
                    Dim tUser As clsApplicationUser = clsApplicationUser.UserByUserID(UserID, tUsersLst)
                    If tUser IsNot Nothing Then
                        Dim prjUser As ECCore.clsUser = PM.GetUserByEMail(tUser.UserEmail)
                        If prjUser IsNot Nothing AndAlso (Not tGroup.UsersList.Contains(prjUser)) Then tGroup.UsersList.Add(prjUser)
                    End If
                Next
            End If
            'PRJ.SaveStructure("Update user group", , , String.Format("Added {1} user(s) to group ('{0}')", tGroup.Name, NewUsers.Count))
        End If
        'A1281 ==

        PRJ.SaveStructure("Add users", , , String.Format("Added {0} user(s)", If(NewUsers Is Nothing, 0, NewUsers.Count)))  ' D7286

        If App.ApplicationError.Status = ecErrorStatus.errWrongLicense Then
            sAddUsersMsg = App.ApplicationError.Message
            App.ApplicationError.Reset()
        End If

        Dim CantBePM As String = If(_CantBePMList.Count > 0, "<br/>", "")
        For Each tUser In tUsersLst
            If _CantBePMList.Contains(tUser.UserID) Then
                If _CantBePMList.Count < 10 Then CantBePM += String.Format(" • {0}<br/>", tUser.UserEmail)
                If _CantBePMList.Count = 10 Then CantBePM += "..."
            End If
        Next

        Dim proceedSetPMs As Boolean = False
        Dim sMsg As String = ""

        CantBePMList = _CantBePMList
        If _CantBePMList.Count > 0 Then
            proceedSetPMs = True
            sMsg = String.Format(ResString("msgAskSetPM"), CantBePM)
        End If

        evalProgress = Nothing  ' D7386

        'Return String.Format("[[{0}], {1}, '{2}']", GetUsersData(), GetResultGroupsData(), JS_SafeString(sAddUsersMsg))
        Return String.Format("['{0}', {1}, {2}, '{3}','{4}',{5}]", sAction, GetUsersData(), GetResultGroupsData(), JS_SafeString(sAddUsersMsg), JS_SafeString(sMsg), Bool2JS(proceedSetPMs))
    End Function

#End Region

#Region "Detach project users"

    Public Function DoDetachProjectUsers(UserIDs As List(Of Integer), Optional sComment As String = "Remove project users") As Integer
        Dim retVal As Integer = 0
        If UserIDs IsNot Nothing Then
            Dim sLst As String = ""
            Dim tPrjUsers As New List(Of Integer)
            For Each ID As Integer In UserIDs
                Dim tUser As clsApplicationUser = UserByID(ID)
                If tUser IsNot Nothing AndAlso Not (CurrentPageID = _PGID_ADMIN_USERSLIST AndAlso tUser.CannotBeDeleted) AndAlso tUser.UserEmail.ToLower <> App.ActiveUser.UserEmail.ToLower Then   ' D6582
                    sLst += CStr(IIf(sLst = "", "", ", ")) + tUser.UserEmail
                    ' D6489 ===
                    If App.DetachProject(PRJ, tUser, False) Then
                        UsersList.Remove(tUser)
                        retVal += 1
                        Dim tPrjUser As ECTypes.clsUser = PM.GetUserByEMail(tUser.UserEmail)
                        If tPrjUser IsNot Nothing Then tPrjUsers.Add(tPrjUser.UserID)
                    End If
                    ' D6489 ==
                End If
            Next
            PM.DeleteUsers(tPrjUsers)   ' D6489
            PRJ.MakeSnapshot(sComment, sLst)  ' D6489
            'PRJ.SaveStructure("Remove project users", True, , sLst)
        End If
        Return retVal
    End Function

    Public Function DoDetachProjectUsers(UserEmails As List(Of String)) As Integer
        ' D6489 ===
        Dim tLst As New List(Of Integer)
        If UserEmails IsNot Nothing Then
            For Each ID As String In UserEmails
                Dim tUser As clsApplicationUser = UserByEmail(ID)
                If tUser IsNot Nothing Then tLst.Add(tUser.UserID)  ' D6582
            Next
        End If
        Return DoDetachProjectUsers(tLst)
        ' D6489 ==
    End Function

    Private Function DetachProjectUsersWhoNotCompletedJudgments() As Integer
        Dim retVal As Integer = 0
        Dim tPrjDetachUsers As New List(Of Integer)

        Dim uList As New List(Of clsUser)
        For Each tUser As clsApplicationUser In UsersList
            If tUser.UserID <> App.ActiveUser.UserID AndAlso Not tUser.CannotBeDeleted Then
                Dim u As clsUser = PM.GetUserByEMail(tUser.UserEmail)
                If u IsNot Nothing Then uList.Add(u)
            End If
        Next

        Dim madeCount As Integer
        Dim totalCount As Integer
        ' D7386 ===
        If _evalProgress Is Nothing Then
            Dim Prg As New Dictionary(Of Integer, Dictionary(Of String, UserEvaluationProgressData))
            Prg.Add(PM.ActiveHierarchy, PM.StorageManager.Reader.GetEvaluationProgress(uList, PM.ActiveHierarchy, madeCount, totalCount))
            evalProgress = Prg
            ' D7386 ==
        End If

        For Each tUser As clsApplicationUser In UsersList
            If tUser.UserID <> App.ActiveUser.UserID AndAlso Not tUser.CannotBeDeleted Then
                Dim u As clsUser = PM.GetUserByEMail(tUser.UserEmail)
                If u IsNot Nothing AndAlso evalProgress(PM.ActiveHierarchy).ContainsKey(u.UserEMail.ToLower) Then
                    With evalProgress(PM.ActiveHierarchy)(u.UserEMail.ToLower)
                        If .EvaluatedCount < .TotalCount Then
                            tPrjDetachUsers.Add(tUser.UserID)
                        End If
                    End With
                End If
            End If
        Next

        If tPrjDetachUsers.Count > 0 Then retVal = DoDetachProjectUsers(tPrjDetachUsers, "Remove project users who not completed judgments")    ' D6582
        Return retVal
    End Function

    Private Function DetachProjectUsersWhoHaveNoJudgments() As Integer
        Dim retVal As Integer = 0
        Dim tPrjDetachUsers As New List(Of Integer)
        'Dim DataUsers As List(Of Integer) = MiscFuncs.DataExistsForUsers_CanvasStreamDatabase(PRJ.ConnectionString, App.ProjectID, PRJ.ProviderType)
        Dim DataUsers As HashSet(Of Integer) = PM.StorageManager.Reader.DataExistsForUsersHashset(PM.ActiveHierarchy) 'PM.DataExistsForUsers()
        For Each tUser As clsApplicationUser In UsersList
            Dim tPrjUser As ECTypes.clsUser = PM.GetUserByEMail(tUser.UserEmail)
            If App.ActiveUser.UserID <> tUser.UserID AndAlso tPrjUser IsNot Nothing AndAlso Not DataUsers.Contains(tPrjUser.UserID) Then tPrjDetachUsers.Add(tUser.UserID)
        Next
        If tPrjDetachUsers.Count > 0 Then retVal = DoDetachProjectUsers(tPrjDetachUsers, "Remove project users who have no judgments")  ' D6582
        Return retVal
    End Function

#End Region

#Region "Attach Workgroup Users"
    ' D0768 ===
    Private Function PrepareUserNameLine(ByVal sUser As String) As String
        Return sUser.Replace(vbTab, " ").Replace("  ", " ")
    End Function
    ' D0768 ==

    Public Function AttachWorkgroupUsers(ByVal sAction As String, ByVal UsersList As String(), ByVal tRoleGroupID As Integer, ByVal fAllowBlankPassword As Boolean, ByVal fSendInvitations As Boolean, Optional fUpdateExistingUsers As Boolean = False) As String ' D0760 + D0890 + D3401
        Dim retVal As String = ""
        Dim tAdded As Integer = 0   ' D2685
        If App.ActiveUser IsNot Nothing AndAlso App.ActiveWorkgroup IsNot Nothing AndAlso UsersList IsNot Nothing AndAlso App.CanUserDoAction(ecActionType.at_alManageWorkgroupUsers, App.ActiveUserWorkgroup, App.ActiveWorkgroup) Then  ' D1469 + D1749
            'Dim WkgUsersList As List(Of clsApplicationUser) = App.DBUsersByWorkgroupID(App.ActiveWorkgroup.ID)

            Dim tGroup As clsRoleGroup = App.ActiveWorkgroup.RoleGroup(tRoleGroupID, App.ActiveWorkgroup.RoleGroups)
            If tGroup IsNot Nothing Then    ' D1469
                For Each tUserString As String In UsersList
                    tUserString = PrepareUserNameLine(tUserString)  ' D0768
                    Dim div_idx As Integer = tUserString.IndexOf(" ", 0)
                    If div_idx < 0 Then div_idx = tUserString.Length
                    Dim sEmail As String = tUserString.Substring(0, div_idx).Trim
                    Dim sName As String = tUserString.Substring(div_idx).Trim
                    If sName = "" Then sName = sEmail
                    If sEmail <> "" Then
                        Dim sPassword As String = ""
                        If Not fAllowBlankPassword Then sPassword = GetRandomString(_DEF_PASSWORD_LENGTH, True, True) ' D0760
                        'Dim fUserExists As Boolean = Not App.DBUserByEmail(sEmail) Is Nothing
                        Dim sUserExists As String = ""  ' D0828

                        If App.DBUserByEmail(sEmail) IsNot Nothing Then tAdded += 1 'A1512

                        ' D2685 ===
                        Dim tUser As clsApplicationUser
                        If App.ActiveWorkgroup.License.CheckParameterByID(ecLicenseParameter.MaxUsersInWorkgroup, Nothing, False) Then
                            tUser = App.UserWithSignup(sEmail, sName, sPassword, "User quick sign-up (HTML)", sUserExists, True)      ' D0890 + D2868
                        Else
                            tUser = App.DBUserByEmail(sEmail)
                            If tUser Is Nothing Then
                                App.LicenseInitError(App.LicenseErrorMessage(App.ActiveWorkgroup.License, ecLicenseParameter.MaxUsersInWorkgroup, True), True)
                                tAdded -= 1 'A1512
                            End If
                        End If
                        ' D2685 ==

                        If tUser IsNot Nothing Then
                            Dim UW As clsUserWorkgroup = App.AttachWorkgroup(tUser.UserID, App.ActiveWorkgroup, tRoleGroupID, "Attach to workgroup (HTML)")
                            ' D0890 ===
                            If fUpdateExistingUsers AndAlso UW IsNot Nothing AndAlso tGroup IsNot Nothing AndAlso UW.RoleGroupID <> tGroup.ID Then
                                UW.RoleGroupID = tGroup.ID
                                App.DBUserWorkgroupUpdate(UW, False, "Set role on user attach (HTML)")
                            End If
                            ' D0890 ==
                            If UW IsNot Nothing Then    ' D1490
                                ' D0760 + D3401 ===
                                If fSendInvitations AndAlso App.ActiveWorkgroup.Status = ecWorkgroupStatus.wsEnabled Then  ' D0828
                                    Dim fNewUser As Boolean = (sUserExists = "")
                                    Dim sSignupSubj As String = ParseAllTemplates(App.ResString(CStr(IIf(fNewUser, "subjSignupNotification", "subjAttachWkgNotification"))), tUser, Nothing)
                                    Dim sSignupBody As String = ParseAllTemplates(App.ResString(CStr(IIf(fNewUser, "bodySignupNotification", If(isSSO_Only(), "bodyAttachWkgNotificationSSO", "bodyAttachWkgNotification")))), tUser, Nothing)   ' D7432
                                    Dim sError As String = ""
                                    SendMail(WebOptions.SystemEmail, tUser.UserEmail, sSignupSubj, sSignupBody, sError)
                                End If
                                ' D0760 + D3401 ==
                            End If
                        End If
                    End If
                Next
            End If
        End If

        If tAdded > 0 Then retVal = String.Format(ResString("msgAttachedParticipants"), tAdded)

        Return String.Format("['{0}','{1}',{2}]", sAction, retVal, GetWorkgroupUsers())
    End Function

#End Region

    '#Region "Detach Workgroup Users"

    '    ' D0753 ===
    '    Public Function DetachWorkgroupUsers(ByVal sAction As String, ByVal UserIDs As List(Of Integer)) As String 'todo: AD to check
    '        Dim tRemoved As Integer = 0
    '        If UserIDs IsNot Nothing AndAlso App.ActiveWorkgroup IsNot Nothing Then ' D1627
    '            Dim tUWList As List(Of clsUserWorkgroup) = App.DBUserWorkgroupsByWorkgroupID(App.ActiveWorkgroup.ID)
    '            Dim tWkgList As List(Of clsWorkgroup) = Nothing ' D4606
    '            If App.ActiveWorkgroup.Status = ecWorkgroupStatus.wsSystem Then tWkgList = App.DBWorkgroupsAll(True, True) ' D4606
    '            For Each ID As Integer In UserIDs
    '                If ID <> App.ActiveUser.UserID Then ' D1726
    '                    Dim tUW As clsUserWorkgroup = clsUserWorkgroup.UserWorkgroupByUserIDAndWorkgroupID(ID, App.ActiveWorkgroup.ID, tUWList)
    '                    If tUW IsNot Nothing Then
    '                        ' D4606 ===
    '                        If App.DBUserWorkgroupDelete(tUW, False, "Detach user from workgroup (HTML)") Then
    '                            If App.ActiveWorkgroup.Status = ecWorkgroupStatus.wsSystem Then
    '                                Dim tUser_UWList As List(Of clsUserWorkgroup) = App.DBUserWorkgroupsByUserID(ID)
    '                                App.CheckUserWorkgroups(App.DBUserByID(ID), tWkgList, tUser_UWList)
    '                            End If
    '                        Else
    '                            tUW = Nothing
    '                        End If
    '                        ' D4606 ==
    '                    End If
    '                    If tUW IsNot Nothing Then tRemoved += 1
    '                End If
    '            Next
    '        End If

    '        Return String.Format("['{0}','{1}',{2}]", sAction, JS_SafeString(String.Format(ResString("msgRemovedParticipants"), tRemoved)), GetUsersData())
    '    End Function
    '    ' D0753 ==

    '#End Region

#Region "Update Workgroup Users Roles"
    Public Function UpdateWorkgroupUsersGroupID(ByVal sAction As String, ByVal UserIDs As List(Of String), ByVal tRoleGroupID As Integer, fDoCheckPMs As Boolean) As String ' D2794 'todo: AD to check

        Dim tUpdated As Integer = 0
        If UserIDs IsNot Nothing AndAlso App.ActiveWorkgroup IsNot Nothing Then ' D1627

            ' D2685 ===
            Dim tGroup As clsRoleGroup = App.ActiveWorkgroup.RoleGroup(tRoleGroupID, App.ActiveWorkgroup.RoleGroups)
            If tGroup IsNot Nothing Then
                'Dim tUWList As List(Of clsUserWorkgroup) = App.DBUserWorkgroupsByWorkgroupID(App.ActiveWorkgroup.ID)   ' -D4752
                Dim sMessage As String = "Set user '{1}' group '{0}' (HTML)"
                'Dim fCheckPrjCreator As Boolean = tGroup.RoleLevel = ecRoleLevel.rlApplicationLevel AndAlso (tGroup.GroupType = ecRoleGroupType.gtAccountManager OrElse tGroup.GroupType = ecRoleGroupType.gtAdministrator OrElse tGroup.GroupType = ecRoleGroupType.gtECAccountManager OrElse tGroup.GroupType = ecRoleGroupType.gtProjectCreator OrElse tGroup.GroupType = ecRoleGroupType.gtTechSupport)
                Dim fCheckPrjCreator As Boolean = tGroup.RoleLevel = ecRoleLevel.rlApplicationLevel AndAlso (tGroup.GroupType = ecRoleGroupType.gtWorkgroupManager OrElse tGroup.GroupType = ecRoleGroupType.gtAdministrator OrElse tGroup.GroupType = ecRoleGroupType.gtProjectOrganizer) ' D2780
                ' D2794 ===
                Dim PMGrpID As Integer = App.ActiveWorkgroup.GetDefaultRoleGroupID(ecRoleLevel.rlModelLevel, ecRoleGroupType.gtProjectManager)
                Dim EvalGrpID As Integer = App.ActiveWorkgroup.GetDefaultRoleGroupID(ecRoleLevel.rlModelLevel, ecRoleGroupType.gtEvaluator)
                Dim fDoResetPM As Boolean = fDoCheckPMs AndAlso tRoleGroupID = App.ActiveWorkgroup.GetDefaultRoleGroupID(ecRoleLevel.rlApplicationLevel, ecRoleGroupType.gtUser)
                ' D2794 ==

                Dim tMaxPC As Long = App.ActiveWorkgroup.License.GetParameterMaxByID(ecLicenseParameter.MaxProjectCreatorsInWorkgroup)
                Dim tCurPC As Long = App.ActiveWorkgroup.License.GetParameterValueByID(ecLicenseParameter.MaxProjectCreatorsInWorkgroup)

                For Each UID As String In UserIDs
                    Dim usr = UserByEmail(UID)
                    If usr IsNot Nothing Then   ' D7311
                        Dim tUW As clsUserWorkgroup = clsUserWorkgroup.UserWorkgroupByUserIDAndWorkgroupID(usr.UserID, App.ActiveWorkgroup.ID, UWFullList)    ' D4752
                        If tUW IsNot Nothing Then
                            If tUW.RoleGroupID <> tRoleGroupID AndAlso usr.UserID <> App.ActiveUser.UserID Then ' D2794
                                Dim isPM As Boolean = App.CanUserDoAction(ecActionType.at_alCreateNewModel, tUW, App.ActiveWorkgroup) OrElse App.CanUserDoAction(ecActionType.at_alUploadModel, tUW, App.ActiveWorkgroup)   ' D2704
                                'If fCheckPrjCreator AndAlso Not App.ActiveWorkgroup.License.CheckParameterByID(ecLicenseParameter.MaxProjectCreatorsInWorkgroup, Nothing, isPM) Then    ' D2704
                                If fCheckPrjCreator AndAlso (tMaxPC > 0 AndAlso tMaxPC < tCurPC + CLng(IIf(isPM, 0, 1))) Then    ' D2704
                                    App.LicenseInitError(App.LicenseErrorMessage(App.ActiveWorkgroup.License, ecLicenseParameter.MaxProjectCreatorsInWorkgroup, True), True)
                                    Exit For
                                Else
                                    ' D3684 ===
                                    Dim tOldGrp As clsRoleGroup = App.ActiveWorkgroup.RoleGroup(tUW.RoleGroupID, App.ActiveWorkgroup.RoleGroups)
                                    Dim fWasPO As Boolean = tOldGrp IsNot Nothing AndAlso (tOldGrp.GroupType = ecRoleGroupType.gtProjectOrganizer OrElse tOldGrp.GroupType = ecRoleGroupType.gtWorkgroupManager)
                                    Dim fWasUsr As Boolean = tOldGrp IsNot Nothing AndAlso tOldGrp.GroupType = ecRoleGroupType.gtUser
                                    Dim sPMPrjList As String = ""
                                    ' D3684 ==

                                    If fCheckPrjCreator Then tCurPC += 1 ' D3204
                                    tUW.RoleGroupID = tRoleGroupID
                                    ' D2794 ===
                                    Dim tUser As clsApplicationUser = App.DBUserByID(usr.UserID)
                                    Dim sEmail As String = "?"
                                    If tUser IsNot Nothing Then sEmail = tUser.UserEmail
                                    If App.DBUserWorkgroupUpdate(tUW, False, String.Format(sMessage, tGroup.Name, sEmail)) Then

                                        ' D3684 ===
                                        If (tGroup.GroupType = ecRoleGroupType.gtProjectOrganizer OrElse tGroup.GroupType = ecRoleGroupType.gtWorkgroupManager) AndAlso fWasUsr AndAlso PMGrpID > 0 Then
                                            Dim tmpExtra As clsExtra = App.DBExtraRead(clsExtra.Params2Extra(tUW.ID, ecExtraType.Workgroup, ecExtraProperty.OldPMProjects))
                                            If tmpExtra IsNot Nothing Then
                                                Dim sPrjList As String = CStr(tmpExtra.Value)
                                                If Not String.IsNullOrEmpty(sPrjList) Then
                                                    Dim sSQL As String = String.Format("UPDATE {0} SET {1}=? WHERE {2}=? AND {3} IN ({4})", clsComparionCore._TABLE_WORKSPACE, clsComparionCore._FLD_WORKSPACE_GRPID, clsComparionCore._FLD_WORKSPACE_USERID, clsComparionCore._FLD_WORKSPACE_PRJID, sPrjList)
                                                    Dim tParams As New List(Of Object)
                                                    tParams.Add(PMGrpID)
                                                    tParams.Add(usr.UserID) ' D6453
                                                    Dim tRes As Integer = App.Database.ExecuteSQL(sSQL, tParams)
                                                    If tRes > 0 Then App.DBSaveLog(dbActionType.actRestore, dbObjectType.einfUser, usr.UserID, "Restore user PM roles in projects", "Restored projects: " + tRes.ToString)
                                                    App.DBExtraDelete(tmpExtra)
                                                End If
                                            End If
                                        End If
                                        ' D3684 ==

                                        If fDoResetPM Then
                                            Dim tWSList As List(Of clsWorkspace) = App.GetWorkspacesWhereUserIsPM(usr.UserID)
                                            If tWSList IsNot Nothing Then
                                                For Each tWS As clsWorkspace In tWSList
                                                    If tWS.GroupID <> EvalGrpID Then
                                                        tWS.GroupID = EvalGrpID
                                                        App.DBWorkspaceUpdate(tWS, False, "Set user as Evaluator due to changes workgroup permissions")
                                                        If fWasPO AndAlso sPMPrjList.Length < 1993 Then sPMPrjList += String.Format("{0}{1}", IIf(sPMPrjList = "", "", ","), tWS.ProjectID) ' D3684 // 1993 - for avoid save extra prorty more than 2000 chars
                                                    End If
                                                Next
                                                App.DBSaveLog(dbActionType.actModify, dbObjectType.einfUser, usr.UserID, "Set user as Evaluator due to changes workgroup permissions", String.Format("Updated {0} project(s)", tWSList.Count))
                                            End If

                                            If sPMPrjList <> "" Then App.DBExtraWrite(clsExtra.Params2Extra(tUW.ID, ecExtraType.Workgroup, ecExtraProperty.OldPMProjects, sPMPrjList)) ' D3684

                                        End If
                                    Else
                                        tUW = Nothing
                                    End If
                                    ' D2794 ==
                                End If
                            End If
                        End If
                        If tUW IsNot Nothing AndAlso tUW.RoleGroupID = tRoleGroupID Then tUpdated += 1  ' D7311
                    End If
                    ' D2685 ==
                Next
            End If
        End If

        Dim sMsg As String = String.Format(ResString("msgUpdatedUsers"), tUpdated)
        Dim fIsError As Boolean = False
        If App.ApplicationError.Status = ecErrorStatus.errWrongLicense Then
            sMsg += ". " + App.ApplicationError.Message
            App.ApplicationError.Reset()
            fIsError = True
        End If

        Return String.Format("['{0}','{1}',{2},{3}]", sAction, JS_SafeString(sMsg), GetUsersData(), Bool2JS(fIsError))
    End Function

    Public Function GetUserProjectIDs(ByVal tUserID As Integer) As List(Of Integer)

        Dim tRes As New List(Of Integer)
        If App.ActiveWorkgroup IsNot Nothing Then   ' D1627
            Dim data As List(Of Dictionary(Of String, Object)) = App.Database.SelectBySQL(String.Format("SELECT W.ProjectID as ID FROM Workspace W LEFT JOIN PROJECTS P ON P.ID=W.ProjectID WHERE P.WorkgroupID={0} AND W.UserID={1} AND (P.Status & 2048)=0", App.ActiveWorkgroup.ID, tUserID))   ' D1810
            'Dim tLst As List(Of clsWorkspace) = App.DBWorkspacesByUserID(tUserID)
            If data IsNot Nothing Then
                For Each tRow As Dictionary(Of String, Object) In data
                    If tRow IsNot Nothing AndAlso tRow("ID") IsNot Nothing AndAlso Not IsDBNull(tRow("ID")) Then
                        tRes.Add(CInt(tRow("ID")))
                    End If
                Next
            End If
        End If
        Return tRes
    End Function

    Public Function SetPrjUserDisabled(UID As Integer, fDisabled As Boolean) As Boolean
        Dim tUser As clsApplicationUser = UserByID(UID)
        Dim tWS As clsWorkspace = WSByUserID(UID)
        If tUser IsNot Nothing AndAlso Not tUser.CannotBeDeleted AndAlso tWS IsNot Nothing AndAlso tUser.Status <> ecUserStatus.usDisabled Then
            Dim isImpact As Boolean = PRJ.isImpact
            If (tWS.Status(isImpact) = ecWorkspaceStatus.wsDisabled) <> fDisabled Then
                tWS.Status(isImpact) = If(fDisabled, ecWorkspaceStatus.wsDisabled, ecWorkspaceStatus.wsEnabled)
                tWS.Status(Not isImpact) = tWS.Status(isImpact)
                App.DBWorkspaceUpdate(tWS, False, CStr(IIf(fDisabled, "Disable user in project", "Enable user in project")))
                Return tWS.Status(isImpact) = ecWorkspaceStatus.wsDisabled
            End If
        End If
        Return False
    End Function

    Public Function SetWGUserDisabled(UID As Integer, fDisabled As Boolean) As Boolean
        Dim tUser As clsApplicationUser = UserByID(UID)
        If tUser IsNot Nothing Then
            'Dim tUWList As List(Of clsUserWorkgroup) = App.DBUserWorkgroupsByWorkgroupID(App.ActiveWorkgroup.ID)   ' -D4752
            Dim sMessage As String = CStr(IIf(fDisabled, "Disable", "Enable")) + " user in workgroup"
            Dim tUW As clsUserWorkgroup = clsUserWorkgroup.UserWorkgroupByUserIDAndWorkgroupID(UID, App.ActiveWorkgroup.ID, UWFullList)    ' D4752
            If tUW IsNot Nothing Then
                tUW.Status = CType(IIf(fDisabled, ecUserWorkgroupStatus.uwDisabled, ecUserWorkgroupStatus.uwEnabled), ecUserWorkgroupStatus)
                Return App.DBUserWorkgroupUpdate(tUW, False, sMessage)
            End If
        End If
        Return False
    End Function

#End Region

#Region "Edit Menu"

    Private Function UserCanBeUpdated(SourceUser As clsApplicationUser) As Boolean
        Dim sReviewAccount As String = If(_OPT_ALLOW_REVIEW_ACCOUNT, WebConfigOption(ExpertChoice.Web._OPT_REVIEW_ACCOUNT, "", True).ToLower, "")  ' D5068
        Return Not SourceUser.CannotBeDeleted AndAlso SourceUser.UserEmail.ToLower <> sReviewAccount
    End Function

    Private Function SetUsersState(UserIDs As List(Of String), isDisabled As Boolean) As Integer
        Dim retVal As Integer = 0
        Dim tProjectID As Integer = PRJ.ID

        Dim sMessage As String = CStr(IIf(isDisabled, "Disable project users", "Enable project users"))
        For Each ID As String In UserIDs
            Dim tUser As clsApplicationUser = UserByEmail(ID)
            If tUser IsNot Nothing AndAlso UserCanBeUpdated(tUser) Then ' D7311
                Dim tWS As clsWorkspace = WSByUserID(tUser.UserID)
                If tWS IsNot Nothing Then
                    tWS.StatusLikelihood = CType(IIf(isDisabled, ecWorkspaceStatus.wsDisabled, ecWorkspaceStatus.wsEnabled), ecWorkspaceStatus)
                    tWS.StatusImpact = tWS.StatusLikelihood
                    If Not App.DBWorkspaceUpdate(tWS, False, sMessage) Then tWS = Nothing
                End If
                If tWS IsNot Nothing Then retVal += 1
            End If
        Next
        Dim sUsers As String = UserIDs.Count.ToString + " user(s)"
        If UserIDs.Count = 1 Then
            Dim tUser As clsApplicationUser = UserByEmail(UserIDs(0))
            If tUser IsNot Nothing Then sUsers = String.Format("'{0}'", tUser.UserEmail)
        End If
        If UserIDs.Count > 0 Then App.SaveProjectLogEvent(PRJ, "Update user status", True, String.Format("{0} {1}", IIf(isDisabled, "Disable", "Enable"), sUsers))
        If UserIDs.Contains(App.ActiveUser.UserEmail) Then App.Workspaces = Nothing

        Return retVal
    End Function

    ''' <summary>
    ''' Erase judgments for specific users and specific location (in Riskion)
    ''' </summary>
    ''' <param name="UserIDs"></param>
    ''' <param name="L">Likelihood judgments</param>
    ''' <param name="I">Impact judgments</param>
    ''' <param name="C">Controls judgments</param>
    ''' <returns></returns>
    Private Function EraseJudgments(UserIDs As List(Of String), A As Boolean, Mode As Integer, L As Boolean, LMode As Integer, I As Boolean, IMode As Integer, C As Boolean) As Integer ' LMode/IMode = 0 - Alternatives, 1 - Objectives, 2 - Both; A - active hierarchy, L/I - Likelihood/Impact, C - Controls
        Dim retVal As Integer = 0
        If UserIDs IsNot Nothing Then
            For Each ID As String In UserIDs
                Dim fResult As Boolean = False
                Dim tUser As clsApplicationUser = UserByEmail(ID)
                If tUser IsNot Nothing Then
                    Dim tPrjUser As ECTypes.clsUser = PM.GetUserByEMail(tUser.UserEmail)
                    If tPrjUser IsNot Nothing Then
                        If A Then
                            ' in Comparion remove all judgments
                            Dim JudgmentsType As ECJudgmentsType = ECJudgmentsType.jtObjectivesAndAlternatives
                            If Mode = 0 Then JudgmentsType = ECJudgmentsType.jtAlternatives
                            If Mode = 1 Then JudgmentsType = ECJudgmentsType.jtObjectives
                            fResult = PM.DeleteUserJudgments(tPrjUser, JudgmentsType)
                            'PM.AddEmptyMissingJudgments(PM.ActiveHierarchy, PM.ActiveAltsHierarchy, tPrjUser)
                        End If
                        'in Riskion use options - L, I, C
                        If L Then
                            Dim JudgmentsType As ECJudgmentsType = ECJudgmentsType.jtObjectivesAndAlternatives
                            If LMode = 0 Then JudgmentsType = ECJudgmentsType.jtAlternatives
                            If LMode = 1 Then JudgmentsType = ECJudgmentsType.jtObjectives
                            fResult = PM.DeleteUserJudgments(tPrjUser, JudgmentsType, ECHierarchyID.hidLikelihood)
                        End If
                        If I Then
                            Dim JudgmentsType As ECJudgmentsType = ECJudgmentsType.jtObjectivesAndAlternatives
                            If IMode = 0 Then JudgmentsType = ECJudgmentsType.jtAlternatives
                            If IMode = 1 Then JudgmentsType = ECJudgmentsType.jtObjectives
                            fResult = PM.DeleteUserJudgments(tPrjUser, JudgmentsType, ECHierarchyID.hidImpact)
                        End If
                        If C Then
                            fResult = PM.DeleteUserJudgments(tPrjUser, ECJudgmentsType.jtControls) 'erase judgments for Controls
                        End If
                    End If
                    If fResult Then retVal += 1
                End If
            Next
            PRJ.ResetProject(True)
            App.SaveProjectLogEvent(PRJ, "Erase judgments", True, retVal.ToString + " users")
        End If
        Return retVal
    End Function

    Public Function UpdateWorkgroupUsersGroupIDForProject(ByVal UserIDs As List(Of Integer), ByVal tRoleGroupID As Integer, fDoCheckPMs As Boolean) As List(Of Integer)

        Dim ResList As New List(Of Integer)
        If UserIDs IsNot Nothing AndAlso App.ActiveWorkgroup IsNot Nothing Then ' D1627

            ' D2685 ===
            Dim tGroup As clsRoleGroup = App.ActiveWorkgroup.RoleGroup(tRoleGroupID, App.ActiveWorkgroup.RoleGroups)
            If tGroup IsNot Nothing Then
                Dim tUWList As List(Of clsUserWorkgroup) = App.DBUserWorkgroupsByWorkgroupID(App.ActiveWorkgroup.ID)
                Dim sMessage As String = "Set user '{1}' group '{0}' (SL)"
                'Dim fCheckPrjCreator As Boolean = tGroup.RoleLevel = ecRoleLevel.rlApplicationLevel AndAlso (tGroup.GroupType = ecRoleGroupType.gtAccountManager OrElse tGroup.GroupType = ecRoleGroupType.gtAdministrator OrElse tGroup.GroupType = ecRoleGroupType.gtECAccountManager OrElse tGroup.GroupType = ecRoleGroupType.gtProjectCreator OrElse tGroup.GroupType = ecRoleGroupType.gtTechSupport)
                Dim fCheckPrjCreator As Boolean = tGroup.RoleLevel = ecRoleLevel.rlApplicationLevel AndAlso (tGroup.GroupType = ecRoleGroupType.gtWorkgroupManager OrElse tGroup.GroupType = ecRoleGroupType.gtAdministrator OrElse tGroup.GroupType = ecRoleGroupType.gtProjectOrganizer) ' D2780
                ' D2794 ===
                Dim PMGrpID As Integer = App.ActiveWorkgroup.GetDefaultRoleGroupID(ecRoleLevel.rlModelLevel, ecRoleGroupType.gtProjectManager)
                Dim EvalGrpID As Integer = App.ActiveWorkgroup.GetDefaultRoleGroupID(ecRoleLevel.rlModelLevel, ecRoleGroupType.gtEvaluator)
                Dim fDoResetPM As Boolean = fDoCheckPMs AndAlso tRoleGroupID = App.ActiveWorkgroup.GetDefaultRoleGroupID(ecRoleLevel.rlApplicationLevel, ecRoleGroupType.gtUser)
                ' D2794 ==

                Dim tMaxPC As Long = App.ActiveWorkgroup.License.GetParameterMaxByID(ecLicenseParameter.MaxProjectCreatorsInWorkgroup)
                Dim tCurPC As Long = App.ActiveWorkgroup.License.GetParameterValueByID(ecLicenseParameter.MaxProjectCreatorsInWorkgroup)

                For Each UID As Integer In UserIDs
                    Dim tUW As clsUserWorkgroup = clsUserWorkgroup.UserWorkgroupByUserIDAndWorkgroupID(UID, App.ActiveWorkgroup.ID, tUWList)
                    If tUW IsNot Nothing Then
                        If tUW.RoleGroupID <> tRoleGroupID AndAlso UID <> App.ActiveUser.UserID Then ' D2794
                            Dim isPM As Boolean = App.CanUserDoAction(ecActionType.at_alCreateNewModel, tUW, App.ActiveWorkgroup) OrElse App.CanUserDoAction(ecActionType.at_alUploadModel, tUW, App.ActiveWorkgroup)   ' D2704
                            'If fCheckPrjCreator AndAlso Not App.ActiveWorkgroup.License.CheckParameterByID(ecLicenseParameter.MaxProjectCreatorsInWorkgroup, Nothing, isPM) Then    ' D2704
                            If fCheckPrjCreator AndAlso (tMaxPC > 0 AndAlso tMaxPC < tCurPC + CLng(IIf(isPM, 0, 1))) Then    ' D2704
                                App.LicenseInitError(App.LicenseErrorMessage(App.ActiveWorkgroup.License, ecLicenseParameter.MaxProjectCreatorsInWorkgroup, True), True)
                                Exit For
                            Else
                                ' D3684 ===
                                Dim tOldGrp As clsRoleGroup = App.ActiveWorkgroup.RoleGroup(tUW.RoleGroupID, App.ActiveWorkgroup.RoleGroups)
                                Dim fWasPO As Boolean = tOldGrp IsNot Nothing AndAlso (tOldGrp.GroupType = ecRoleGroupType.gtProjectOrganizer OrElse tOldGrp.GroupType = ecRoleGroupType.gtWorkgroupManager)
                                Dim fWasUsr As Boolean = tOldGrp IsNot Nothing AndAlso tOldGrp.GroupType = ecRoleGroupType.gtUser
                                Dim sPMPrjList As String = ""
                                ' D3684 ==

                                If fCheckPrjCreator Then tCurPC += 1 ' D3204
                                tUW.RoleGroupID = tRoleGroupID
                                ' D2794 ===
                                Dim tUser As clsApplicationUser = App.DBUserByID(UID)
                                Dim sEmail As String = "?"
                                If tUser IsNot Nothing Then sEmail = tUser.UserEmail
                                If App.DBUserWorkgroupUpdate(tUW, False, String.Format(sMessage, tGroup.Name, sEmail)) Then

                                    ' D3684 ===
                                    If (tGroup.GroupType = ecRoleGroupType.gtProjectOrganizer OrElse tGroup.GroupType = ecRoleGroupType.gtWorkgroupManager) AndAlso fWasUsr AndAlso PMGrpID > 0 Then
                                        Dim tmpExtra As clsExtra = App.DBExtraRead(clsExtra.Params2Extra(tUW.ID, ecExtraType.Workgroup, ecExtraProperty.OldPMProjects))
                                        If tmpExtra IsNot Nothing Then
                                            Dim sPrjList As String = CStr(tmpExtra.Value)
                                            If Not String.IsNullOrEmpty(sPrjList) Then
                                                Dim sSQL As String = String.Format("UPDATE {0} SET {1}=? WHERE {2}=? AND {3} IN ({4})", clsComparionCore._TABLE_WORKSPACE, clsComparionCore._FLD_WORKSPACE_GRPID, clsComparionCore._FLD_WORKSPACE_USERID, clsComparionCore._FLD_WORKSPACE_PRJID, sPrjList)
                                                Dim tParams As New List(Of Object)
                                                tParams.Add(PMGrpID)
                                                tParams.Add(UID)
                                                Dim tRes As Integer = App.Database.ExecuteSQL(sSQL, tParams)
                                                If tRes > 0 Then App.DBSaveLog(dbActionType.actRestore, dbObjectType.einfUser, UID, "Restore user PM roles in projects", "Restored projects: " + tRes.ToString)
                                                App.DBExtraDelete(tmpExtra)
                                            End If
                                        End If
                                    End If
                                    ' D3684 ==

                                    If fDoResetPM Then
                                        Dim tWSList As List(Of clsWorkspace) = App.GetWorkspacesWhereUserIsPM(UID)
                                        If tWSList IsNot Nothing Then
                                            For Each tWS As clsWorkspace In tWSList
                                                If tWS.GroupID <> EvalGrpID Then
                                                    tWS.GroupID = EvalGrpID
                                                    App.DBWorkspaceUpdate(tWS, False, "Set user as Evaluator due to changes workgroup permissions")
                                                    If fWasPO AndAlso sPMPrjList.Length < 1993 Then sPMPrjList += String.Format("{0}{1}", IIf(sPMPrjList = "", "", ","), tWS.ProjectID) ' D3684 // 1993 - for avoid save extra prorty more than 2000 chars
                                                End If
                                            Next
                                            App.DBSaveLog(dbActionType.actModify, dbObjectType.einfUser, UID, "Set user as Evaluator due to changes workgroup permissions", String.Format("Updated {0} project(s)", tWSList.Count))
                                        End If

                                        If sPMPrjList <> "" Then App.DBExtraWrite(clsExtra.Params2Extra(tUW.ID, ecExtraType.Workgroup, ecExtraProperty.OldPMProjects, sPMPrjList)) ' D3684

                                    End If
                                Else
                                    tUW = Nothing
                                End If
                                ' D2794 ==
                            End If
                        End If
                    End If
                    If tUW Is Nothing OrElse tUW.RoleGroupID <> tRoleGroupID Then ResList.Add(-UID) Else ResList.Add(UID)
                    ' D2685 ==
                Next
            End If
        End If
        App.UserWorkgroups = Nothing
        App.Database.Close()    ' D2231

        ' D2685 ===
        'If App.ApplicationError.Status = ecErrorStatus.errWrongLicense Then
        '    'SystemMessage(App.ApplicationError.Message, True) 'todo uncomment
        '    App.ApplicationError.Reset()
        'Else
        '    'SystemMessage("", True)
        'End If
        ' D2685 ==
        Return ResList
    End Function

#End Region

#Region "LDAP"
    Public Function CheckLDAPConnection(ByVal sLDAPServer As String, ByVal sUserName As String, ByVal sUserPsw As String, ByRef sError As String) As Boolean
        Dim sRes = ExpertChoice.Service.CheckDirectoryEntry("LDAP://" + sLDAPServer, sUserName, sUserPsw, sError)
        Return sRes
    End Function

    Public Function GetLDAPUsersList(ByVal sLDAPServer As String, ByVal sUserName As String, ByVal sUserPsw As String, ByVal sSearch As String, ByRef ErrorMessage As String) As Dictionary(Of String, String)
        Dim Res As Dictionary(Of String, String) = Nothing
        If sLDAPServer = "" Then ErrorMessage = "LDAP Query not specified"
        If sUserName = "" Then ErrorMessage = "LDAP Username not specified"
        If ErrorMessage = "" Then
            Dim _LDAPUsersFullList As Dictionary(Of String, String) = Nothing
            If _LDAPUsersFullList Is Nothing Then
                Dim _Users As List(Of ECTypes.clsUser) = ExpertChoice.Service.GetLDAPUsersList("LDAP://" + sLDAPServer, LDAPSearchQuery(), sUserName, sUserPsw, sSearch, ErrorMessage)
                If ErrorMessage = "" Then
                    _LDAPUsersFullList = New Dictionary(Of String, String)
                    For Each tUser As ECTypes.clsUser In _Users
                        _LDAPUsersFullList.Add(tUser.UserEMail, tUser.UserName)
                    Next
                    _Users = Nothing
                End If

                Res = _LDAPUsersFullList
            End If
        End If
        Return Res
    End Function

#End Region

End Class