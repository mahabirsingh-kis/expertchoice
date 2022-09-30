

Partial Class ProjectLookupPage
    Inherits clsComparionCorePage

    Public Const PRJ_MAX_COUNT As Integer = 200
    'Public Const DT As String = "yyyy-MM-dd"
    Public Const DT As String = "yyyy-MM-dd HH:mm:ss"

    Private Const _SESS_WKG_LIST As String = "WkgLitsPrjLookup" ' D3621

    Public Sub New()
        MyBase.New(_PGID_ADMIN_PRJ_LOOKUP)
    End Sub

    ' D3621 ===
    Private ReadOnly Property WorkgroupIDs As String
        Get
            Dim sLst As String = ""
            If Session(_SESS_WKG_LIST) Is Nothing Then
                If Not App.ActiveUser.CannotBeDeleted AndAlso Not App.CanUserDoSystemWorkgroupAction(ecActionType.at_slManageAnyWorkgroup, App.ActiveUser.UserID) Then    ' D4684
                    Dim WkgList As List(Of clsWorkgroup) = App.AvailableWorkgroups(App.ActiveUser, App.UserWorkgroups, True, False)
                    For Each tUW As clsUserWorkgroup In App.UserWorkgroups
                        Dim tWkg As clsWorkgroup = clsWorkgroup.WorkgroupByID(tUW.WorkgroupID, WkgList)
                        If App.CanUserDoAction(ecActionType.at_alManageAnyModel, tUW, tWkg) OrElse App.CanUserDoAction(ecActionType.at_alCanBePM, tUW, tWkg) Then sLst += String.Format("{0}{1}", IIf(sLst = "", "", ","), tUW.WorkgroupID) ' D4460
                    Next
                    If sLst = "" Then sLst = "-999999" ' D4460
                End If
                Session.Add(_SESS_WKG_LIST, sLst)
            Else
                sLst = CStr(Session(_SESS_WKG_LIST))
            End If
            Return sLst
        End Get
    End Property
    ' D3621 ==

    Public Function CanSeeRisk() As Boolean
        Return App.isRiskEnabled    ' D3863
        'If App.SystemWorkgroup IsNot Nothing AndAlso App.SystemWorkgroup.License IsNot Nothing Then Return App.SystemWorkgroup.License.CheckParameterByID(ecLicenseParameter.RiskEnabled, Nothing, True) Else Return False
    End Function

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        ShowNavigation = False
        If isAJAX() Then

            Dim sResult As String = ""

            Select Case CheckVar(_PARAM_ACTION, "").ToLower

                Case "open"
                    Dim PrjID As Integer
                    If Integer.TryParse(CheckVar("id", ""), PrjID) Then
                        Dim tPrj As clsProject = App.DBProjectByID(PrjID)
                        If tPrj IsNot Nothing AndAlso Not tPrj.isMarkedAsDeleted Then
                            Dim tWkg As clsWorkgroup = App.DBWorkgroupByID(tPrj.WorkgroupID)
                            If tWkg IsNot Nothing AndAlso tWkg.License IsNot Nothing AndAlso tWkg.License.isValidLicense AndAlso tWkg.License.CheckParameterByID(ecLicenseParameter.ExpirationDate) AndAlso tWkg.License.CheckParameterByID(ecLicenseParameter.InstanceID) Then ' D3947
                                sResult = CreateLogonURL(App.ActiveUser, tPrj, CStr(IIf(tPrj.isTeamTimeImpact OrElse tPrj.isTeamTimeLikelihood, "pipe=no", "")), _URL_ROOT, tPrj.PasscodeLikelihood, , True)  ' D4616
                            End If
                        End If
                        If sResult = "" Then sResult = " " ' D4620
                    End If

                Case "load"
                    Dim sText As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("text", "").Trim())  'Anti-XSS

                    Dim sSQL = String.Format("SELECT TOP {0} * FROM Projects WHERE (ProjectName LIKE ? ESCAPE '`' OR Comment LIKE ? ESCAPE '`' OR REPLACE(Passcode, '-', '') LIKE ? ESCAPE '`' OR REPLACE(Passcode2, '-', '') LIKE ? ESCAPE '`') {1} ORDER BY LastVisited DESC, ID DESC", PRJ_MAX_COUNT * 2, IIf(WorkgroupIDs = "", "", " AND WorkgroupID IN (" + WorkgroupIDs + ")"))  ' D3621
                    Dim tParams As New List(Of Object)
                    Dim sParam As String = "%" + sText.Replace("%", "`%").Replace("-", "") + "%"    ' D6701
                    tParams.Add(sParam)
                    tParams.Add(sParam)
                    tParams.Add(sParam)
                    tParams.Add(sParam)
                    Dim tList As List(Of Dictionary(Of String, Object)) = App.Database.SelectBySQL(sSQL, tParams)

                    Dim WkgList As List(Of clsWorkgroup) = App.DBWorkgroupsAll(True, True)      ' D4580

                    Dim OnlineUsers As List(Of clsOnlineUserSession) = App.DBOnlineSessions()   ' D3863
                    Dim CanSeeAllPrjList As New Dictionary(Of Integer, Boolean)

                    sResult = ""
                    Dim Cnt As Integer = 0
                    For Each tRow As Dictionary(Of String, Object) In tList
                        Dim tProject As clsProject = App.DBParse_Project(tRow)

                        Dim sWkgName As String = ""
                        Dim fWkgEnabled As Boolean = False

                        Dim tWkg As clsWorkgroup = clsWorkgroup.WorkgroupByID(tProject.WorkgroupID, WkgList)
                        If tWkg IsNot Nothing Then
                            sWkgName = tWkg.Name
                            fWkgEnabled = tWkg.License IsNot Nothing AndAlso tWkg.License.isValidLicense AndAlso tWkg.License.CheckParameterByID(ecLicenseParameter.ExpirationDate) AndAlso tWkg.License.CheckParameterByID(ecLicenseParameter.InstanceID)   ' D3947
                        End If

                        Dim tUW As clsUserWorkgroup = Nothing   ' D6880
                        ' D4460 ===
                        Dim fCanSeeAllPrj As Boolean = False
                        If CanSeeAllPrjList.ContainsKey(tWkg.ID) Then
                            fCanSeeAllPrj = CanSeeAllPrjList(tWkg.ID)
                        Else
                            tUW = clsUserWorkgroup.UserWorkgroupByUserIDAndWorkgroupID(App.ActiveUser.UserID, tWkg.ID, App.UserWorkgroups)
                            fCanSeeAllPrj = App.CanUserDoAction(ecActionType.at_alManageAnyModel, tUW, tWkg)
                            CanSeeAllPrjList.Add(tWkg.ID, fCanSeeAllPrj)
                        End If

                        Dim fCanSeePrj As Boolean = fCanSeeAllPrj
                        Dim tWS As clsWorkspace = Nothing   ' D6880
                        If Not fCanSeePrj Then
                            tWS = App.DBWorkspaceByUserIDProjectID(App.ActiveUser.UserID, tProject.ID)  ' D6880
                            fCanSeePrj = Not tProject.isMarkedAsDeleted AndAlso tWS IsNot Nothing
                        End If
                        ' D6880 ===
                        If fCanSeePrj AndAlso Not fCanSeeAllPrj AndAlso (tProject.ProjectStatus <> ecProjectStatus.psActive OrElse Not tProject.isOnline) Then
                            If tUW Is Nothing Then tUW = clsUserWorkgroup.UserWorkgroupByUserIDAndWorkgroupID(App.ActiveUser.UserID, tWkg.ID, App.UserWorkgroups)
                            If tWS Is Nothing Then tWS = App.DBWorkspaceByUserIDProjectID(App.ActiveUser.UserID, tProject.ID)
                            If Not App.CanUserModifyProject(App.ActiveUser.UserID, tProject.ID, tUW, tWS, tWkg) Then
                                fCanSeePrj = False
                            End If
                        End If
                        ' D6880 ==

                        If fWkgEnabled AndAlso fCanSeePrj Then
                            ' D4460 ==
                            Dim sStatus As String = ResString("lbl_" + tProject.ProjectStatus.ToString)
                            If tProject.isMarkedAsDeleted Then sStatus = ResString("lblMarkedAsDeleted")

                            Dim sCreated As String = "&nbsp;"
                            Dim sVisited As String = "&nbsp;"
                            Dim sModified As String = "&nbsp;"
                            If tProject.Created.HasValue Then sCreated = tProject.Created.Value.ToString(DT)
                            If tProject.LastVisited.HasValue Then sVisited = tProject.LastVisited.Value.ToString(DT)
                            If tProject.LastModify.HasValue Then sModified = tProject.LastModify.Value.ToString(DT)

                            ' D3863 ===
                            Dim sLock As String = ""
                            If tProject.LockInfo IsNot Nothing AndAlso tProject.LockInfo.LockStatus <> ECLockStatus.lsUnLocked Then
                                sLock = App.GetMessageByLockStatus(tProject.LockInfo, tProject, App.ActiveUser.UserID, False)
                                ' D3866 ===
                                If tProject.LockInfo.LockerUserID > 0 AndAlso tProject.LockInfo.LockerUserID <> App.ActiveUser.UserID Then
                                    Dim tUser As clsApplicationUser = App.DBUserByID(tProject.LockInfo.LockerUserID)
                                    If tUser IsNot Nothing Then sLock += String.Format(" ({0})", tUser.UserEmail)
                                End If
                                ' D3866 ==
                            End If
                            ' D3863 ==

                            'PrjID, PrjName, PrjComment, Passcode, Passcode2, PrjEnabled, WkgName, WkgEnabled, PrjStatus, Online, Created, LastVisited, Modified
                            sResult += CStr(If(sResult = "", "", ",")) + String.Format("[{0},'{1}','{2}','{3}','{4}',{5},'{6}',{7},'{8}','{9}','{10}','{11}','{12}','{13}',{14},{15}]",
                                                                                    tProject.ID, JS_SafeString(SafeFormString(tProject.ProjectName)), JS_SafeString(SafeFormString(If(tProject.Comment = "", "", tProject.Comment)).Trim),
                                                                                    JS_SafeString(SafeFormString(tProject.PasscodeLikelihood)), JS_SafeString(If(tProject.PasscodeImpact = tProject.PasscodeLikelihood, "&nbsp;", SafeFormString(tProject.PasscodeImpact))),
                                                                                    If(tProject.isMarkedAsDeleted, 0, 1), JS_SafeString(sWkgName), If(fWkgEnabled, 1, 0),
                                                                                    JS_SafeString(sStatus), JS_SafeString(IIf(tProject.isOnline, ResString("lblYes"), "&nbsp;")), sCreated, sVisited, sModified, JS_SafeString(sLock), If(tProject.isTeamTimeImpact, 2, If(tProject.isTeamTimeLikelihood, 1, 0)), clsOnlineUserSession.OnlineSessionsByProjectID(tProject.ID, OnlineUsers).Count)    ' D3863
                            Cnt += 1
                            If Cnt >= PRJ_MAX_COUNT Then Exit For
                        End If
                    Next

                    sResult = "[" + sResult + "]"

            End Select

            If sResult <> "" Then
                RawResponseStart()
                Response.ContentType = "text/plain"
                'Response.AddHeader("Content-Length", CStr(sResult))
                Response.Write(sResult.Trim)    ' D4620
                Response.End()
                'RawResponseEnd()
            End If


        End If
    End Sub

End Class

