
Partial Class WorkgroupsListPage
    Inherits clsComparionCorePage

    Private _AllowedWorkgroups As List(Of clsWorkgroup) = Nothing   ' D0474
    'Private _ActiveID As Integer = -1   ' D0227

    Private _OnlineUsers As List(Of clsOnlineUserSession) = Nothing ' D1345
    Private _UsersList As New List(Of clsApplicationUser)   ' D1345

    Public ReadOnly Property AllowedWorkgroups() As List(Of clsWorkgroup)   ' D1031
        Get
            If _AllowedWorkgroups Is Nothing Then
                ' D0262 + D0474 ===
                _AllowedWorkgroups = New List(Of clsWorkgroup)
                Dim WGList As List(Of clsWorkgroup) = App.AvailableWorkgroups(App.ActiveUser, App.UserWorkgroups, True, True)   ' D3288
                Dim tWasSystemAdded As Boolean = False  ' D0588
                ' D4833 ===
                Dim fCanViewAll As Boolean = App.CanUserDoSystemWorkgroupAction(ecActionType.at_slManageAnyWorkgroup, App.ActiveUser.UserID)
                Dim fCanViewLicenses As Boolean = App.CanUserDoSystemWorkgroupAction(ecActionType.at_slViewLicenseOwnWorkgroup, App.ActiveUser.UserID)
                Dim UWList As List(Of clsUserWorkgroup) = App.DBUserWorkgroupsByUserID(App.ActiveUser.UserID)
                For Each tWG As clsWorkgroup In WGList
                    Dim tUW As clsUserWorkgroup = clsUserWorkgroup.UserWorkgroupByUserIDAndWorkgroupID(App.ActiveUser.UserID, tWG.ID, UWList)
                    Dim fCanManageOrView As Boolean = tUW IsNot Nothing AndAlso (App.CanUserDoAction(ecActionType.at_alManageWorkgroupRights, tUW, tWG) OrElse App.CanUserDoAction(ecActionType.at_alManageAnyModel, tUW, tWG) OrElse (App.isWorkgroupOwner(tWG, App.ActiveUser, tUW) AndAlso App.CanUserDoAction(ecActionType.at_slViewLicenseOwnWorkgroup, tUW, tWG)))
                    'If App.CanUserDoSystemWorkgroupAction(ecActionType.at_slManageAnyWorkgroup, App.ActiveUser.UserID) OrElse tWG.Status <> ecWorkgroupStatus.wsSystem Then    ' D0289
                    If fCanViewAll OrElse fCanViewLicenses OrElse fCanManageOrView Then    ' D0289 + D3295
                        If fCanViewAll AndAlso tWG.Status = ecWorkgroupStatus.wsSystem Then
                            ' D4833 ==
                            _AllowedWorkgroups.Insert(0, tWG)
                            tWasSystemAdded = True  ' D0588
                            ' D0587 ===
                        Else
                            ' D4833 ===
                            If tWG.Status <> ecWorkgroupStatus.wsSystem AndAlso (fCanViewAll OrElse fCanManageOrView OrElse App.CanUserDoAction(ecActionType.at_slViewLicenseOwnWorkgroup, tUW, tWG)) Then
                                If App.isStartupWorkgroup(tWG) Then
                                    If fCanViewAll OrElse fCanManageOrView Then _AllowedWorkgroups.Insert(CType(IIf(_AllowedWorkgroups.Count > 0 AndAlso tWasSystemAdded, 1, 0), Integer), tWG) ' D0588
                                    ' D4833 ==
                                Else
                                    _AllowedWorkgroups.Add(tWG)
                                End If
                            End If
                        End If
                    End If
                Next
            End If
            Return _AllowedWorkgroups
        End Get
    End Property


    ' D4984 ===
    Public Function CanCreateWorkgroup() As Boolean
        If App.CanUserDoAction(ecActionType.at_slCreateWorkgroup, App.ActiveUserWorkgroup) AndAlso HasPermission(_PGID_ADMIN_WORKGROUP_CREATE, Nothing) Then
            If Not App.SystemWorkgroup.License.isValidLicense OrElse App.SystemWorkgroup.License.GetParameterMaxByID(ecLicenseParameter.MaxWorkgroupsTotal) = UNLIMITED_VALUE OrElse App.SystemWorkgroup.License.CheckParameterByID(ecLicenseParameter.MaxWorkgroupsTotal, , False) Then
                Return True
            End If
        End If
        Return False
    End Function
    ' D4984 ==

    ' D1031 ===
    Private Function GetJSDate(DT As Nullable(Of DateTime)) As Long
        If DT.HasValue Then Return CLng((DT.Value.Subtract(New Date(1970, 1, 1)).Ticks) / 10000) Else Return -1
    End Function

    Public Function GetWorkgroupsList() As String
        Dim sList As String = ""
        Dim DTShift As Date = New Date(1970, 1, 1)
        If _OnlineUsers Is Nothing Then _OnlineUsers = App.DBOnlineSessions ' D1345
        Dim UWList As List(Of clsUserWorkgroup) = App.DBUserWorkgroupsByUserID(App.ActiveUser.UserID)   ' D3288
        Dim tLicItem As Long = -1   ' D3952
        Dim isSystemValid As Boolean = App.SystemWorkgroup IsNot Nothing AndAlso App.SystemWorkgroup.License.isValidLicense AndAlso App.SystemWorkgroup.License.CheckAllParameters(tLicItem)    ' D3952
        For Each tWkg As clsWorkgroup In AllowedWorkgroups
            Dim mExpires As Nullable(Of DateTime) = Nothing
            If tWkg.License IsNot Nothing Then mExpires = Date.FromBinary(tWkg.License.GetParameterMaxByID(ecLicenseParameter.ExpirationDate))
            Dim sUserEmail As String = ""   ' D1346
            Dim sUserName As String = ""    ' D1346
            Dim fHasPsw As Boolean = False
            ' D1344 + D1346 ===
            Dim UID As Integer = tWkg.ECAMID
            If UID <= 0 Then UID = tWkg.OwnerID
            If UID > 0 Then
                Dim tUser As clsApplicationUser = clsApplicationUser.UserByUserID(UID, _UsersList)
                ' D1346 ==
                If tUser Is Nothing Then
                    tUser = App.DBUserByID(UID)
                    If tUser IsNot Nothing Then _UsersList.Add(tUser)
                End If
                If tUser IsNot Nothing Then
                    sUserEmail = tUser.UserEmail    ' D1346
                    sUserName = tUser.UserName      ' D1346
                    fHasPsw = If(isSSO(), True, tUser.HasPassword)    ' D6552
                End If
            End If
            Dim mUsersCount As Integer
            Dim tData As Object = App.Database.ExecuteScalarSQL(CType(IIf(tWkg.Status = ecWorkgroupStatus.wsSystem, "SELECT COUNT(ID) as cnt FROM USERS", "SELECT COUNT(ID) as cnt FROM UserWorkgroups WHERE WorkgroupID=" + tWkg.ID.ToString), String))
            If tData IsNot Nothing AndAlso Not IsDBNull(tData) Then mUsersCount = CInt(tData)
            ' D1344 ==
            ' D1346 + D4833 ===
            Dim tUW As clsUserWorkgroup = clsUserWorkgroup.UserWorkgroupByUserIDAndWorkgroupID(App.ActiveUser.UserID, tWkg.ID, UWList)
            Dim isOwner As Boolean = App.isWorkgroupOwner(tWkg, App.ActiveUser, tUW)
            ' D4981 ===
            Dim fCanEdit As Boolean = isOwner
            Dim fCanDelete As Boolean = isOwner AndAlso tWkg.Status <> ecWorkgroupStatus.wsSystem AndAlso App.CanUserDoAction(ecActionType.at_slDeleteOwnWorkgroup, tUW, tWkg)
            Dim fCanAddPrj As Boolean = isSystemValid AndAlso tWkg.License.isValidLicense AndAlso tWkg.License.CheckParameterByID(ecLicenseParameter.ExpirationDate) AndAlso tWkg.Status <> ecWorkgroupStatus.wsSystem AndAlso App.CanUserDoAction(ecActionType.at_alManageAnyModel, tUW, tWkg)    ' D3333 + D3952 
            Dim fCanSeeLic As Boolean = isOwner OrElse App.CanUserDoAction(ecActionType.at_slViewLicenseOwnWorkgroup, tUW, tWkg)
            Dim fCanSeeStat As Boolean = isOwner OrElse App.CanUserDoAction(ecActionType.at_slViewOwnWorkgroupReports, tUW, tWkg)   ' D7269
            ' D4833 ==
            If fCanDelete AndAlso App.ActiveWorkgroup IsNot Nothing AndAlso tWkg.ID = App.ActiveWorkgroup.ID Then fCanDelete = False
            ' D4981 ==
            ' D1346 ===
            Dim sExpires As String = ""
            If mExpires.HasValue Then
                If mExpires.Value >= Date.Now.AddYears(20) Then sExpires = ResString("lblNever") Else sExpires = mExpires.Value.ToString("yyyy-MM-dd")
            End If
            sList += If(sList = "", "", "," + vbCrLf) + String.Format("[{0},'{1}',{2},{3},{4},{5},{6},'{7}','{8}',{9},{10},{11},{12},{13},{14},{15},{16},'{17}','{18}',{19},{20},{21}]", tWkg.ID, JS_SafeString(ShortString(tWkg.Name, 70, False)), GetJSDate(tWkg.Created), GetJSDate(tWkg.LastVisited), GetJSDate(mExpires), If(mExpires.HasValue AndAlso mExpires < Date.Now, 1, 0), tWkg.License.GetParameterValueByID(ecLicenseParameter.MaxProjectsTotal), JS_SafeString(sUserEmail), JS_SafeString(sUserName), If(fHasPsw, 1, 0), clsOnlineUserSession.OnlineSessionsByWorkgroupID(tWkg.ID, _OnlineUsers).Count, mUsersCount, CInt(tWkg.Status), If(fCanEdit, 2, 0) + If(fCanDelete, 1, 0), IIf(tWkg.License.isValidLicense AndAlso tWkg.License.CheckParameterByID(ecLicenseParameter.RiskEnabled, Nothing, True), 1, 0), IIf(tWkg.License IsNot Nothing AndAlso tWkg.License.isValidLicense, 1, 0), IIf(fCanAddPrj, 1, 0), JS_SafeString(tWkg.OpportunityID), JS_SafeString(sExpires), Bool2Num(tWkg.License.isValidLicense AndAlso tWkg.License.CheckParameterByID(ecLicenseParameter.InstanceID, Nothing, True)), Bool2Num(fCanSeeLic), Bool2Num(fCanSeeStat))  ' D1344 + D2257 + D2647 + D3288 + D3333 + D3947 + D4833 + D7269
            ' D1346 + D3426 ==
        Next
        Return sList
    End Function
    ' D1031 ==

    Public Sub New()
        MyBase.New(_PGID_ADMIN_WORKGROUPS)
    End Sub

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        AlignVerticalCenter = False
        AlignHorizontalCenter = True
        ShowTopNavigation = False
        '_ActiveID = CheckVar(_PARAM_ID, _ActiveID)  ' D0227

        ' D2212 ===
        If App.SystemWorkgroup IsNot Nothing AndAlso App.isAuthorized AndAlso App.ActiveUser.CannotBeDeleted AndAlso App.CanvasMasterDBVersion >= "0.9995" Then
            Dim tRes As Object = App.Database.ExecuteScalarSQL(String.Format("SELECT COUNT({0}) as Cnt FROM {1} WHERE {0}={2}", clsComparionCore._FLD_WKGPARAMS_WKGID, clsComparionCore._TABLE_WKGPARAMS, App.SystemWorkgroup.ID))
            If tRes IsNot Nothing AndAlso CInt(tRes) <= 0 Then btnExtractParams.Visible = True
        End If
        ' D2212 ==

        ' D0092 ===
        Dim sAction As String = EcSanitizer.GetSafeHtmlFragment(CheckVar(_PARAM_ACTION, ""))  ' Anti-XSS
        ' D1346 ===
        Select Case sAction.ToLower

            ' D7206 ===
            Case "refresh"
                App.ResetWorkgroupsList()
                Response.Redirect(PageURL(CurrentPageID, GetTempThemeURI(False)), True)
                ' D7206 ==

            Case _ACTION_DELETE
                ' D1346 ==
                Dim WGID As Integer = 0
                Integer.TryParse(CheckVar(_PARAM_ID, WGID.ToString), WGID)
                Dim WG As clsWorkgroup = App.DBWorkgroupByID(WGID)  ' D0474
                If Not WG Is Nothing Then
                    Dim tUW As clsUserWorkgroup = clsUserWorkgroup.UserWorkgroupByUserIDAndWorkgroupID(App.ActiveUser.UserID, WG.ID, App.UserWorkgroups)    ' D4981
                    Dim fCanDelete As Boolean = WG.Status <> ecWorkgroupStatus.wsSystem AndAlso App.isWorkgroupOwner(WG, App.ActiveUser, tUW) AndAlso App.CanUserDoAction(ecActionType.at_slDeleteOwnWorkgroup, tUW, WG)    ' D4981 + D4981
                    If WG.Status <> ecWorkgroupStatus.wsSystem AndAlso fCanDelete Then  ' D1346
                        App.DBWorkgroupDelete(WG)   ' D0474 + D0502
                        App.Workspaces = Nothing    ' D0474
                        App.ResetWorkgroupsList()     ' D4984
                    End If
                    ' D2861 ===
                    Dim sURL As String = PageURL(CurrentPageID, GetTempThemeURI(False))
                    If isSLTheme() AndAlso sURL.ToLower.IndexOf("refresh=") < 1 Then sURL += If(sURL.IndexOf("?") > 0, "&", "?") + "refresh=1"
                    Response.Redirect(sURL, True)    ' D0717 + D0729 + D0766
                    ' D2861 ==
                End If

            Case "export"

                Dim sContent As String = ""
                Dim sIDs As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("ids", "")) ' Anti-XSS
                If sIDs = "all" Then
                    sIDs = ""
                    For Each tWkg As clsWorkgroup In AllowedWorkgroups
                        sIDs += CType(IIf(sIDs = "", "", "|"), String) + tWkg.ID.ToString
                    Next
                End If

                'If _OnlineUsers Is Nothing Then _OnlineUsers = App.DBOnlineSessions
                Dim IDs As String() = sIDs.Split(CChar("|"))
                For Each sID As String In IDs
                    Dim ID As Integer = -1
                    If Integer.TryParse(sID, ID) Then
                        Dim tWkg As clsWorkgroup = clsWorkgroup.WorkgroupByID(ID, AllowedWorkgroups)
                        If tWkg IsNot Nothing Then

                            Dim mExpires As Nullable(Of DateTime) = Nothing
                            If tWkg.License IsNot Nothing Then mExpires = Date.FromBinary(tWkg.License.GetParameterMaxByID(ecLicenseParameter.ExpirationDate))
                            Dim sUserEmail As String = ""
                            Dim sUserName As String = ""
                            Dim fHasPsw As Boolean = False
                            Dim UID As Integer = tWkg.ECAMID
                            If UID <= 0 Then UID = tWkg.OwnerID
                            If UID > 0 Then
                                Dim tUser As clsApplicationUser = clsApplicationUser.UserByUserID(UID, _UsersList)
                                If tUser Is Nothing Then
                                    tUser = App.DBUserByID(UID)
                                    If tUser IsNot Nothing Then _UsersList.Add(tUser)
                                End If
                                If tUser IsNot Nothing Then
                                    sUserEmail = tUser.UserEmail
                                    sUserName = tUser.UserName
                                    fHasPsw = If(isSSO(), True, tUser.HasPassword)    ' D6552
                                End If
                            End If
                            Dim mUsersCount As Integer
                            Dim tData As Object = App.Database.ExecuteScalarSQL(CType(IIf(tWkg.Status = ecWorkgroupStatus.wsSystem, "SELECT COUNT(ID) as cnt FROM USERS", "SELECT COUNT(ID) as cnt FROM UserWorkgroups WHERE WorkgroupID=" + tWkg.ID.ToString), String))
                            If tData IsNot Nothing AndAlso Not IsDBNull(tData) Then mUsersCount = CInt(tData)
                            sContent += String.Format("{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}{0}{8}{0}{9}{0}{10}{0}{11}", vbTab, tWkg.Name, tWkg.License.GetParameterValueByID(ecLicenseParameter.MaxProjectsTotal), mUsersCount, tWkg.Created, tWkg.LastModify, tWkg.LastVisited, mExpires, sUserEmail, sUserName, IIf(fHasPsw, 1, 0), tWkg.OpportunityID) + vbCrLf  ' D3333
                        End If
                    End If
                Next

                If sContent <> "" Then sContent = String.Format("Workgroup name{0}Projects{0}Users{0}Created{0}Last modified{0}Last visited{0}Expires{0}Owner E-mail{0}Owner Name{0}Has psw", vbTab) + vbCrLf + sContent

                DownloadContent(sContent, "application/vnd.ms-excel", "Workgroups.csv", dbObjectType.einfWorkgroup) ' D6593
                'RawResponseStart()
                'Response.AppendHeader("Content-Disposition", "attachment; filename=""Workgroups.csv""")
                'Response.ContentType = "mime-type=application/vnd.ms-excel"
                'Response.AddHeader("Content-Length", sContent.Length.ToString)
                'Response.Write(sContent)
                'If Response.IsClientConnected Then RawResponseEnd()

        End Select
        ' D0092 ==
        ' D2861 ===
        If isSLTheme() AndAlso Not IsPostBack AndAlso Not isCallback AndAlso CheckVar("refresh", False) Then
            ClientScript.RegisterStartupScript(GetType(String), "Refresh", "ReloadWorkgroups();", True)
        End If
        ' D2861 ==
        ScriptManager.RegisterStartupScript(Me, GetType(String), "init", "Init();", True)   ' D3060
    End Sub

    ' D2212 ===
    Protected Sub btnExtractParams_Click(sender As Object, e As EventArgs) Handles btnExtractParams.Click
        For Each tWkg As clsWorkgroup In AllowedWorkgroups
            App.DBWorkgroupExtractLicenseParameters(tWkg)
        Next
        'Response.Redirect(Request.Url.OriginalString, True)    ' D6766
    End Sub
    ' D2212 ==

End Class
