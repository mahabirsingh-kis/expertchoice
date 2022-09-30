Imports System.IO
Imports Telerik.Web.UI  ' D0535
Imports System.Collections.ObjectModel
Imports SearchOption = Microsoft.VisualBasic.FileIO.SearchOption

Partial Class WorkgroupEditPage
    Inherits clsComparionCorePage

    Private _AllowedWorkgroups As List(Of clsWorkgroup) = Nothing
    Private _Workgroup As clsWorkgroup = Nothing
    Private isNewWorkgroup As Boolean = True

    Private Const _KEEP_STARTUP_PROJECTS_USERS As Boolean = True    ' D0981
    Private Const _SESS_PREV_LICENSE As String = "PrevLicense"      ' D2644
    Private Const _SESS_PREV_LIC_KEY As String = "PrevLicKey"       ' D2644
    Private Const _SESS_MSG As String = "LicenseWarning"            ' D2644

    Private ReadOnly Property AllowedWorkgroups() As List(Of clsWorkgroup)
        Get
            If _AllowedWorkgroups Is Nothing Then
                _AllowedWorkgroups = App.AvailableWorkgroups(App.ActiveUser, App.UserWorkgroups)    ' D0475
            End If
            Return _AllowedWorkgroups
        End Get
    End Property

    ' D3333 ===
    Public Function CanEditOppID() As Boolean
        Return App.CanvasMasterDBVersion >= "0.9998"
    End Function
    ' D3333 ==

    Public Property CurrentWorkgroup() As clsWorkgroup
        Get
            If _Workgroup Is Nothing Then
                Dim WGID As Integer = 0
                Dim sAction As String = EcSanitizer.GetSafeHtmlFragment(CheckVar(_PARAM_ACTION, ""))  ' Anti-XSS
                If Integer.TryParse(CheckVar(_PARAM_ID, WGID.ToString), WGID) Then
                    _Workgroup = clsWorkgroup.WorkgroupByID(WGID, AllowedWorkgroups) ' D0261 + D0475
                End If
                If _Workgroup Is Nothing OrElse sAction = _ACTION_NEW Then
                    _Workgroup = New clsWorkgroup
                    _Workgroup.OwnerID = App.ActiveUser.UserID
                    isNewWorkgroup = True
                    CurrentPageID = _PGID_ADMIN_WORKGROUP_CREATE
                Else
                    isNewWorkgroup = False
                    If _Workgroup.RoleGroups.Count = 0 Then _Workgroup.RoleGroups = App.DBRoleGroupsByWorkgroupID(_Workgroup.ID, True) ' D0502
                    CustomWorkgroupPermissions = _Workgroup ' D7270
                    CurrentPageID = _PGID_ADMIN_WORKGROUP_EDIT
                End If
                'ASPxPageControlWorkgroup.ActiveTabPage.Text = PageTitle(CurrentPageID) ' -D3310
            End If
            Return _Workgroup
        End Get
        Set(ByVal value As clsWorkgroup)
            _Workgroup = value
        End Set
    End Property


    Public Sub New()
        MyBase.New(_PGID_ADMIN_WORKGROUP_EDIT)
    End Sub

    ' D7270 ===
    Protected Sub Page_PreInit(ByVal sender As Object, ByVal e As EventArgs) Handles Me.PreInit
        If Not App.isAuthorized Then FetchAccess()
        ' D0264 ===
        If CurrentWorkgroup IsNot Nothing AndAlso isNewWorkgroup Then
            If Not HasPermissionByAction(ecActionType.at_slCreateWorkgroup, True, False) Then FetchAccessByWrongLicense()
        End If
        ' D0264 ==
        If CurrentWorkgroup IsNot Nothing AndAlso Not isNewWorkgroup Then
            Dim tUW As clsUserWorkgroup = clsUserWorkgroup.UserWorkgroupByUserIDAndWorkgroupID(App.ActiveUser.UserID, CurrentWorkgroup.ID, App.UserWorkgroups)
            If Not App.CanUserDoAction(ecActionType.at_slManageOwnWorkgroup, tUW, CurrentWorkgroup) Then FetchAccess(_PGID_ADMIN_WORKGROUPS)
        End If
    End Sub
    ' D7270 ==

    Protected Sub Page_PreLoad(ByVal sender As Object, ByVal e As EventArgs) Handles Me.PreLoad
        AlignHorizontalCenter = True
        AlignVerticalCenter = True
        ' NavigationPageID = _PGID_ADMIN_WORKGROUPS   ' D0267 -D0587
        btnOK.Text = ResString("btnOK") ' D0717
        'btnCancel.Text = ResString("btnCancel")
        'btnCancel.Visible = Not IsPostBack    ' D0729 + D0766 + D0803 + D6446
        tbTitle.Focus()
        'ASPxPageControlWorkgroup.TabPages(0).Text = ResString(IIf(isNewWorkgroup, "lblWorkgroupsTabNew", "lblWorkgroupsTabEdit"))   ' D0121 - D3310
        If Not IsPostBack Then WorkgroupShowData(CurrentWorkgroup)
        'cbDisabled.Visible = Not isNewWorkgroup ' D0244 -D0270
        cbDisabled.Text = ResString("lblDisabledWorkgroup") ' D0244
        cbNoCopyStartupContent.Visible = isNewWorkgroup AndAlso App.StartupWorkgroup IsNot Nothing  ' D0587
        cbNoCopyStartupContent.Text = ResString("lblNoCopyStartupContent")   ' D0587
        If Not IsPostBack AndAlso Not isCallback Then
            btnOK.OnClientClick = String.Format("showLoadingPanel();", btnOK.ClientID)
            ' D2644 ===
            If CheckVar("warning", False) AndAlso Not String.IsNullOrEmpty(SessVar(_SESS_MSG)) Then
                ClientScript.RegisterStartupScript(GetType(String), "Confirm", String.Format("setTimeout(""ConfirmChangeType('{0}');"", 300);", JS_SafeString(SessVar(_SESS_MSG))), True)
            End If
            If CheckVar("reject", False) AndAlso CurrentWorkgroup IsNot Nothing AndAlso CurrentWorkgroup.License IsNot Nothing Then
                CurrentWorkgroup.License.LicenseContent = CType(Session(_SESS_PREV_LICENSE), MemoryStream)
                CurrentWorkgroup.License.LicenseKey = CStr(Session(_SESS_PREV_LIC_KEY))
                Session.Remove(_SESS_PREV_LICENSE)
                Session.Remove(_SESS_PREV_LIC_KEY)
                tbLicenseKey.Text = EcSanitizer.GetSafeHtmlFragment(CurrentWorkgroup.License.LicenseKey)  ' Anti-XSS
                App.DBWorkgroupUpdate(CurrentWorkgroup, False, "Revert license, because another type")
                ShowMessage(String.Format("<b>{0}</b>", ResString("msgLicenseReverted")), False)
            End If
            Session.Remove(_SESS_MSG)
            ' D2644 ==
            If isSLTheme() AndAlso CheckVar("refresh", False) Then
                ClientScript.RegisterStartupScript(GetType(String), "Refresh", "ReloadWorkgroups();", True)
            End If
            ' D3087 ===
            If CurrentWorkgroup.Status <> ecWorkgroupStatus.wsSystem Then

                ' -D6446 ===
                'cbDetailMode.Text = ResString("lblDetailsMode")
                'cbDetailMode.Checked = False
                'If Not isNewWorkgroup AndAlso CurrentWorkgroup.ID > 0 Then
                '    Dim tExtra As clsExtra = App.DBExtraRead(clsExtra.Params2Extra(CurrentWorkgroup.ID, ecExtraType.Workgroup, ecExtraProperty.DetailsMode))
                '    If tExtra IsNot Nothing AndAlso CStr(tExtra.Value) = "1" Then cbDetailMode.Checked = True
                'End If
                ' -D6446 ==
            End If
            ' D3087 ==
        End If
        tbEULAFile.Enabled = App.CanvasMasterDBVersion >= "0.992"   ' D0922
        rowECAM.Visible = CurrentWorkgroup IsNot Nothing AndAlso CurrentWorkgroup.Status <> ecWorkgroupStatus.wsSystem    ' D2780
        'If btnCancel.Visible Then btnCancel.OnClientClick = String.Format("loadURL('{0}'); return false;", JS_SafeString(EcSanitizer.GetSafeHtmlFragment(CheckVar("ret", PageURL(_PGID_ADMIN_WORKGROUPS, GetTempThemeURI(False)))))) ' D2644 + Anti-XSS
        ' D3333 ===
        If lblMessage.Text = "" AndAlso Not CanEditOppID() AndAlso App.ActiveUser.CannotBeDeleted Then
            ShowMessage("<span class='text small gray'><b>(!)</b> You need to <a href='" + PageURL(_PGID_DB_SETUP) + "' style='color:#666699' target='_top'>upgrade</a> MasterDB up to version 0.9998 to be able to specify OpportunityID.</span>", False)
        End If
        ' D3333 ==
    End Sub

    Private Sub WorkgroupShowData(ByVal WG As clsWorkgroup)
        If WG Is Nothing Then Exit Sub
        tbTitle.Text = EcSanitizer.GetSafeHtmlFragment(WG.Name)   ' Anti-XSS
        tbComment.Text = EcSanitizer.GetSafeHtmlFragment(WG.Comment)  ' Anti-XSS
        tbECAM.Text = EcSanitizer.GetSafeHtmlFragment(App.OwnerEmail(WG.ECAMID))    ' D6446    ' Anti-XSS
        tbECAM.Enabled = WG.Status <> ecWorkgroupStatus.wsSystem   ' D0261 + D6446
        '' D0289 ===
        '' D0289 ==
        cbDisabled.Checked = WG.Status = ecWorkgroupStatus.wsDisabled   ' D0244
        ' D0261 ===
        cbDisabled.Enabled = WG.Status <> ecWorkgroupStatus.wsSystem
        tbOpportunityID.Text = EcSanitizer.GetSafeHtmlFragment(WG.OpportunityID) ' D3333 + Anti-XSS
        tbLicenseKey.Text = EcSanitizer.GetSafeHtmlFragment(WG.License.LicenseKey)    ' Anti-XSS
        ' D0261 ==
        rowlLicenseFile.Visible = tbLicenseKey.Visible  ' D0267
        rowlLicenseKey.Visible = tbLicenseKey.Visible   ' D0267
        tbLicenseKey.ToolTip = String.Format(ResString("msgDeleteLicenseKeyword"), _LICENSE_DELETE_KEYWORD)
        tbEULAFile.Text = EcSanitizer.GetSafeHtmlFragment(WG.EULAFile)    ' D0922 + Anti-XSS
    End Sub

    ' D0261 ===
    Private Sub ShowMessage(ByVal sMessage As String, ByVal fError As Boolean)
        If fError Then sMessage = String.Format("<div class='error'><b>{0}</b></div>", sMessage)
        lblMessage.Text = sMessage
        lblMessage.Visible = True
    End Sub
    ' D0261 ==

    Private Function WorkgroupSaveData(ByRef WG As clsWorkgroup, ByRef sWarningMsg As String) As Boolean
        If WG Is Nothing Then Return False ' D0261
        ' D0302 ===
        If EcSanitizer.GetSafeHtmlFragment(tbTitle.Text).Trim() = "" Then ' Anti-XSS
            ShowMessage(ResString("msgEmptyTitle"), True)
            Return False
        End If
        ' D0302 ==
        Dim fResult As Boolean = (WG.Name <> EcSanitizer.GetSafeHtmlFragment(tbTitle.Text) OrElse WG.Comment <> EcSanitizer.GetSafeHtmlFragment(tbComment.Text) OrElse WG.EULAFile <> EcSanitizer.GetSafeHtmlFragment(tbEULAFile.Text))  ' D0092 + D0922 + Anti-XSS
        WG.Name = EcSanitizer.GetSafeHtmlFragment(tbTitle.Text)   ' Anti-XSS
        WG.Comment = EcSanitizer.GetSafeHtmlFragment(tbComment.Text)  ' Anti-XSS

        If tbEULAFile.Enabled Then
            If Not fResult Then fResult = WG.EULAFile <> EcSanitizer.GetSafeHtmlFragment(tbEULAFile.Text) ' D3333 + D3353 + Anti-XSS
            WG.EULAFile = EcSanitizer.GetSafeHtmlFragment(tbEULAFile.Text) ' D0922 + Anti-XSS
        End If
        ' D3333 ===
        If tbOpportunityID.Visible AndAlso CanEditOppID() Then
            If Not fResult Then fResult = WG.OpportunityID <> EcSanitizer.GetSafeHtmlFragment(tbOpportunityID.Text) ' D3353 + Anti-XSS
            WG.OpportunityID = EcSanitizer.GetSafeHtmlFragment(tbOpportunityID.Text)  ' Anti-XSS
        End If
        ' D3333 ==

        Dim fDoExtractLicense As Boolean = False

        ' D0266 ===
        If EcSanitizer.GetSafeHtmlFragment(tbLicenseKey.Text) = _LICENSE_DELETE_KEYWORD Then  ' Anti-XSS
            WG.License.LicenseContent = Nothing
            WG.License.LicenseKey = ""
            fResult = True
            App.DBSaveLog(dbActionType.actEditLicense, dbObjectType.einfWorkgroup, WG.ID, "Delete license info", "") ' D0496
            fDoExtractLicense = True    ' D2212
        Else

            Dim fNeedLog As Boolean = EcSanitizer.GetSafeHtmlFragment(tbLicenseKey.Text).Trim() <> "" Or fileLicenseData.FileName <> "" ' D0296 + Anti-XSS

            ' D2644 ===
            Dim fOldRisk As Boolean = False
            Dim fHasLicense As Boolean = WG.License IsNot Nothing AndAlso WG.License.isValidLicense ' D2679
            If fHasLicense Then fOldRisk = WG.License.GetParameterMaxByID(ecLicenseParameter.RiskEnabled) <> 0 ' D2679
            If isNewWorkgroup AndAlso App.SystemWorkgroup IsNot Nothing AndAlso App.SystemWorkgroup.License IsNot Nothing Then fOldRisk = App.SystemWorkgroup.License.GetParameterMaxByID(ecLicenseParameter.RiskEnabled) <> 0
            Session.Add(_SESS_PREV_LICENSE, WG.License.LicenseContent)
            If WG.License IsNot Nothing Then Session.Add(_SESS_PREV_LIC_KEY, WG.License.LicenseKey)
            ' D2644 ==

            ' D0266 ==
            ' D0261 ===
            If fileLicenseData.FileName <> "" Then
                Dim LicenseData() As Byte = fileLicenseData.FileBytes
                WG.License.LicenseContent = New MemoryStream(LicenseData)
                fResult = True
                fDoExtractLicense = True    ' D2212
                App.WorkgroupLicenseInit(WG)    ' D2644
            End If

            If WG.License.LicenseContent Is Nothing Then
                ShowMessage(ResString("errNoLicenseData"), True)
                Return False
            End If

            If EcSanitizer.GetSafeHtmlFragment(tbLicenseKey.Text).Trim() <> "" And EcSanitizer.GetSafeHtmlFragment(tbLicenseKey.Text) <> WG.License.LicenseKey Then ' Anti-XSS
                WG.License.LicenseKey = EcSanitizer.GetSafeHtmlFragment(tbLicenseKey.Text)    ' Anti-XSS
                fResult = True
            End If

            If WG.License.LicenseKey.Trim = "" Then
                ShowMessage(ResString("errNoLicenseKey"), True)
                Return False
            End If

            If fResult Then
                If Not WG.License.isValidLicense Then
                    ShowMessage(ResString("errLicenseFile"), True)
                    If fNeedLog Then App.DBSaveLog(dbActionType.actEditLicense, dbObjectType.einfWorkgroup, WG.ID, "Edit license data", ResString("errLicenseFile")) ' D0496
                    Return False
                Else
                    If fNeedLog Then App.DBSaveLog(dbActionType.actEditLicense, dbObjectType.einfWorkgroup, WG.ID, "Edit license info", String.Format("File: '{0}'; Key: '{1}'", Path.GetFileName(fileLicenseData.PostedFile.FileName), ShortString(EcSanitizer.GetSafeHtmlFragment(tbLicenseKey.Text), 5, True))) ' D0496 + Anti-XSS
                End If
            End If
            ' D0261 ==

            ' D3947 ===
            If fResult Then
                If Not WG.License.CheckParameterByID(ecLicenseParameter.InstanceID) Then
                    ShowMessage(String.Format(ResString("errLicenseInstanceID"), App.GetInstanceID_AsString()), True)
                    Return False
                End If
            End If

            If fResult Then
                If Not WG.License.CheckParameterByID(ecLicenseParameter.ExpirationDate) Then
                    ShowMessage(ResString("errLicenseExpired"), True)
                    Return False
                End If
            End If
            ' D3947 ==

            ' D2644 ===
            If fDoExtractLicense AndAlso fResult AndAlso WG.License IsNot Nothing AndAlso WG.License.isValidLicense AndAlso (WG.Status <> ecWorkgroupStatus.wsSystem OrElse fHasLicense) Then    ' D2679
                Dim fNewRisk As Boolean = WG.License.GetParameterMaxByID(ecLicenseParameter.RiskEnabled) <> 0
                If fNewRisk <> fOldRisk AndAlso App.SystemWorkgroup IsNot Nothing Then
                    sWarningMsg = ResString(CStr(IIf(fNewRisk, "msgChangeLicenseType2Risk", "msgChangeLicenseType2Comparion")))
                End If
            End If
            ' D2644 ==

        End If

        If fResult And isNewWorkgroup Then
            If App.DBWorkgroupUpdate(WG, True, "", fDoExtractLicense) Then  ' D2212
                App.CheckWorkgroup(WG, WG.Status = ecWorkgroupStatus.wsSystem)
            End If
        End If

        ' D0296 ===
        If WG.OwnerID <= 0 AndAlso App.ActiveUser IsNot Nothing Then WG.OwnerID = App.ActiveUser.UserID ' D2780

        Dim sName As String = ""    ' D2681
        Dim sECAM As String = EcSanitizer.GetSafeHtmlFragment(tbECAM.Text).ToLower()
        If WG.Status <> ecWorkgroupStatus.wsSystem AndAlso tbECAM.Enabled AndAlso (sECAM <> App.OwnerEmail(WG.ECAMID).ToLower OrElse sECAM <> App.OwnerEmail(WG.OwnerID).ToLower) Then ' D0261 + D0289 + D6446 + D7276
            Dim tUserID As Integer = SetUser(WG, EcSanitizer.GetSafeHtmlFragment(tbECAM.Text), sName, WG.ECAMID, "Workgroup Manager")  ' D2681 + D6446 Anti-XSS
            If tUserID <> -2 Then
                fResult = True
                WG.ECAMID = tUserID
                WG.OwnerID = tUserID    ' D7276
            End If
        End If
        ' D0296 ==

        ' D0244 ===
        Dim sComment As String = ""
        If cbDisabled.Enabled And Not WG.Status = ecWorkgroupStatus.wsSystem Then    ' D0261 + D0270
            Dim tStatus As ecWorkgroupStatus = CType(IIf(cbDisabled.Checked, ecWorkgroupStatus.wsDisabled, ecWorkgroupStatus.wsEnabled), ecWorkgroupStatus)
            If tStatus <> WG.Status Then
                WG.Status = tStatus
                fResult = True
                sComment = String.Format("{0} Workgroup", IIf(tStatus = ecWorkgroupStatus.wsDisabled, "Disable", "Enable"))
            End If
        End If
        If fResult Then
            App.DBWorkgroupUpdate(WG, False, sComment)
            App.DBExtraDelete(clsExtra.Params2Extra(WG.ID, ecExtraType.Workgroup, ecExtraProperty.CheckMasterProjectsDate)) ' D7007
            SessVar(String.Format("CheckMaster_{0}", WG.ID)) = Nothing
        End If
        ' D0244 ==

        Dim tUserWG As clsUserWorkgroup = clsUserWorkgroup.UserWorkgroupByUserIDAndWorkgroupID(App.ActiveUser.UserID, WG.ID, App.UserWorkgroups)
        If tUserWG Is Nothing Then
            tUserWG = App.DBUserWorkgroupByUserIDWorkgroupID(App.ActiveUser.UserID, WG.ID)
            If Not tUserWG Is Nothing Then App.UserWorkgroups = Nothing
        End If
        If tUserWG Is Nothing Then
            App.AttachWorkgroup(App.ActiveUser.UserID, WG, WG.GetDefaultRoleGroupID(ecRoleLevel.rlApplicationLevel, CType(IIf(App.ActiveUser.CannotBeDeleted, ecRoleGroupType.gtAdministrator, ecRoleGroupType.gtWorkgroupManager), ecRoleGroupType)), "Attach to Workgroup as Creator")  ' D0496
        End If

        ' D0587 ===
        If fResult AndAlso isNewWorkgroup AndAlso cbNoCopyStartupContent.Visible AndAlso Not cbNoCopyStartupContent.Checked AndAlso App.StartupWorkgroup IsNot Nothing Then
            Dim tECAMUser As clsApplicationUser = App.DBUserByID(WG.ECAMID) ' D0588
            Dim DefPMGrpID As Integer = WG.GetDefaultRoleGroupID(ecRoleLevel.rlModelLevel, ecRoleGroupType.gtProjectManager)  ' D0891 + D2780
            Dim DefEvalGrpID As Integer = WG.GetDefaultRoleGroupID(ecRoleLevel.rlModelLevel, ecRoleGroupType.gtEvaluator)   ' D0891
            Dim DefUserGrpID As Integer = WG.GetDefaultRoleGroupID(ecRoleLevel.rlApplicationLevel, ecRoleGroupType.gtUser)  ' D0891
            Dim CopyList As List(Of clsProject) = App.DBProjectsByWorkgroupID(App.StartupWorkgroup.ID)
            Dim WGUsers As New List(Of clsUser) ' D0891
            For Each tSrcProject As clsProject In CopyList
                If Not tSrcProject.isMarkedAsDeleted Then   ' D0891
                    Dim fCopyOK As Boolean = False
                    Dim tDestProject As clsProject = tSrcProject.Clone()
                    tDestProject.PasscodeLikelihood = App.ProjectUniquePasscode("", -1) ' D1709
                    tDestProject.PasscodeImpact = App.ProjectUniquePasscode("", -1)     ' D1709
                    'If Not App.isUniquePasscode(tDestProject.Passcode, -1) Then tDestProject.Passcode = GetRandomString(_DEF_PASSCODE_LENGTH, True, False) ' -D1709
                    tDestProject.WorkgroupID = WG.ID
                    tDestProject.OwnerID = App.ActiveUser.UserID
                    If App.DBProjectCreate(tDestProject, "Copy decision from startup workgroup") Then
                        fCopyOK = App.DBProjectCopy(tSrcProject, clsProject.StorageType, tDestProject.ConnectionString, tDestProject.ProviderType, tDestProject.ID, False) ' D3774
                        ' D0588 ===
                        If fCopyOK Then
                            Dim fHasECAM As Boolean = False ' D0590
                            Dim UsersOrig As List(Of clsUser) = tDestProject.ProjectManager.UsersList
                            Dim Users As New List(Of clsUser)
                            Users.AddRange(UsersOrig.ToArray)
                            ' D0590 ===
                            For Each tUser As clsUser In Users
                                If tECAMUser IsNot Nothing AndAlso String.Compare(tECAMUser.UserEmail, tUser.UserEMail, True) = 0 Then
                                    fHasECAM = True
                                Else
                                    If Not _KEEP_STARTUP_PROJECTS_USERS Then tDestProject.ProjectManager.DeleteUser(tUser) ' D0981
                                End If
                            Next
                            ' D0590 ==
                            If tSrcProject.ProjectStatus <> ecProjectStatus.psTemplate AndAlso tSrcProject.ProjectStatus <> ecProjectStatus.psMasterProject Then   ' D2479
                                If tECAMUser IsNot Nothing AndAlso Not fHasECAM Then    ' D0590
                                    App.AttachProject(tECAMUser, tDestProject, False, DefPMGrpID, "", False)  ' D0891 + D2287 + D2644
                                End If

                                If _KEEP_STARTUP_PROJECTS_USERS Then

                                    For Each tPrjUser As clsUser In Users
                                        Dim tUser As clsApplicationUser = App.DBUserByEmail(tPrjUser.UserEMail)
                                        If tUser IsNot Nothing AndAlso tUser.UserID <> WG.ECAMID AndAlso tUser.UserID <> App.ActiveUser.UserID Then
                                            If clsApplicationUser.AHPUserByUserID(tPrjUser.UserID, WGUsers) Is Nothing Then
                                                If App.AttachWorkgroup(tUser.UserID, WG, DefUserGrpID, "Attach user to wkg from startup project") IsNot Nothing Then WGUsers.Add(tPrjUser)
                                            End If
                                            App.AttachProject(tUser, tDestProject, False, DefEvalGrpID, "Attach to project from startup", False)    ' D2644
                                        End If
                                    Next

                                Else
                                End If
                            End If

                        End If
                        ' D0588 ==
                    End If
                    If Not fCopyOK AndAlso tDestProject IsNot Nothing Then App.DBProjectDelete(tDestProject, True)
                End If
            Next

            '' D0981 ==
            'If _KEEP_STARTUP_PROJECTS_USERS Then
            '    Dim GrpID As Integer = WG.GetDefaultRoleGroupID(ecRoleLevel.rlApplicationLevel, ecRoleGroupType.gtUser)
            '    If GrpID > 0 Then
            '        For Each tPrjUser In WkgProjectUsers
            '            Dim tUser As clsApplicationUser = App.DBUserByEmail(tPrjUser.UserEMail)
            '            If tUser IsNot Nothing AndAlso tUser.UserID <> WG.ECAMID AndAlso tUser.UserID <> App.ActiveUser.UserID Then
            '                App.AttachWorkgroup(tUser.UserID, WG, GrpID, "Attach user to workgroup from startup project")
            '            End If
            '        Next
            '    End If
            'End If
            '' D0981 ==
        End If
        ' D0587 ==

        ' D6446 ===
        App.ResetWorkgroupsList()     ' D4984
        If WG.Status = ecWorkgroupStatus.wsSystem Then App.SystemWorkgroup = App.DBWorkgroupByID(WG.ID, True, True)
        If App.ActiveWorkgroup IsNot Nothing AndAlso WG.ID = App.ActiveWorkgroup.ID Then
            App.ActiveWorkgroup = Nothing
            App.ActiveWorkgroup = App.DBWorkgroupByID(WG.ID, True, True) ' D0475 + D0612
        End If
        ' D6446 ==

        ' -D6446 ===
        '' D3087 ===
        'If CurrentWorkgroup.ID > 0 AndAlso CurrentWorkgroup.Status <> ecWorkgroupStatus.wsSystem Then
        '    Dim tExtra As clsExtra = clsExtra.Params2Extra(CurrentWorkgroup.ID, ecExtraType.Workgroup, ecExtraProperty.DetailsMode)
        '    If cbDetailMode.Checked Then
        '        tExtra.Value = 1
        '        App.DBExtraWrite(tExtra)
        '    Else
        '        App.DBExtraDelete(tExtra)
        '    End If
        'End If
        '' D3087 ==
        ' -D6446 ==

        Return True
    End Function

    Protected Sub btnOK_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnOK.Click
        ' D0092 ===
        ' D0717 ===
        Dim sURL As String = PageURL(_PGID_ADMIN_WORKGROUPS, GetTempThemeURI(False))    ' D0729 + D0766
        ' D0717 ==
        ' D2644 ===
        Dim sConfirm As String = ""
        Dim fUpdated As Boolean = WorkgroupSaveData(CurrentWorkgroup, sConfirm)
        If Not fUpdated Then Exit Sub ' D0261
        ' If Not fUpdated And isNewWorkgroup And CurrentWorkgroup.ID > 0 Then    '-D0227
        sURL = URLParameter(sURL, _PARAM_ID, CurrentWorkgroup.ID)
        If Not String.IsNullOrEmpty(sConfirm) Then
            sURL = PageURL(_PGID_ADMIN_WORKGROUP_EDIT, GetTempThemeURI(False)) + String.Format("&{0}={1}&warning=1", _PARAM_ID, CurrentWorkgroup.ID)
            'If retURL.Value <> "" Then sURL += "&ret=" + HttpUtility.UrlEncode(retURL.Value)
            SessVar(_SESS_MSG) = sConfirm
        Else
            Session.Remove(_SESS_PREV_LICENSE)
            Session.Remove(_SESS_PREV_LIC_KEY)
        End If
        'End If     ' -D0227
        'If String.IsNullOrEmpty(sConfirm) AndAlso Not String.IsNullOrEmpty(SessVar(_SESS_RET_URL)) Then sURL = SessVar(_SESS_RET_URL) ' D1031 + D6766
        If isSLTheme() AndAlso sURL.ToLower.IndexOf("refresh=") <1 Then sURL += If(sURL.IndexOf("?") > 0, "&", "?") + "refresh=1" ' D2861
        ' D2644 ==
        If App.ApplicationError.Status <> ecErrorStatus.errNone Then
            lblMessage.Text = App.ApplicationError.Message
            lblMessage.CssClass = "error"
            lblMessage.Visible = True
            App.ApplicationError.Reset()
        Else
            ' D6446 ===
            If isNewWorkgroup AndAlso String.IsNullOrEmpty(sConfirm) AndAlso App.SystemWorkgroup IsNot Nothing AndAlso App.ActiveWorkgroup IsNot Nothing AndAlso App.ActiveWorkgroup.Status = ecWorkgroupStatus.wsSystem AndAlso CurrentWorkgroup.Status = ecWorkgroupStatus.wsEnabled AndAlso App.SystemWorkgroup.License IsNot Nothing AndAlso App.SystemWorkgroup.License.isValidLicense AndAlso App.SystemWorkgroup.License.GetParameterValueByID(ecLicenseParameter.MaxWorkgroupsTotal) = 1 Then
                App.ActiveWorkgroup = CurrentWorkgroup
                sURL = PageURL(_PGID_PROJECTSLIST)
            End If
            ' D6446 ==
            Response.Redirect(sURL, True)
        End If

        ' D0092 ==
    End Sub

    ' D0296 ===
    ''' <summary>
    ''' Set user for workgroup (replace the old, if exists). Used for set Owner, AC.
    ''' </summary>
    ''' <param name="tWorkgroup"></param>
    ''' <param name="sNewUserEmail">New User Email</param>
    ''' <param name="OldUserID">UserID for old user</param>
    ''' <param name="sUserPosition">name for new user position (Owner, EC)</param>
    ''' <returns>Returns the UserID for new linked user or -1 when new is absent. Returns -2 when nothing changed.</returns>
    ''' <remarks></remarks>
    Private Function SetUser(ByRef tWorkgroup As clsWorkgroup, ByRef sNewUserEmail As String, ByRef sUserName As String, ByVal OldUserID As Integer, ByVal sUserPosition As String) As Integer  ' D2681
        sNewUserEmail = sNewUserEmail.Trim
        Dim tResult As Integer = -2
        Dim OldUsr As clsApplicationUser = App.DBUserByID(OldUserID)    ' D0502
        If Not OldUsr Is Nothing Then
            Dim OldUWG As clsUserWorkgroup = App.DBUserWorkgroupByUserIDWorkgroupID(OldUserID, tWorkgroup.ID)   ' D0092 + D0502
            If Not OldUWG Is Nothing And Not (OldUsr.CannotBeDeleted Or App.CanUserDoSystemWorkgroupAction(ecActionType.at_slManageOwnWorkgroup, OldUserID)) Then
                OldUWG.RoleGroupID = tWorkgroup.GetDefaultRoleGroupID(ecRoleLevel.rlApplicationLevel, ecRoleGroupType.gtUser)
                If App.DBUserWorkgroupUpdate(OldUWG, False, String.Format("Detach '{0}' as '{1}' from workgroup (set as user)", OldUsr.UserEmail, sUserPosition)) Then tResult = -1 ' D0502
            End If
        End If

        If sNewUserEmail <> "" Then
            ' D2681 ===
            Dim sName As String = ""
            Dim idx As Integer = sNewUserEmail.IndexOf(" ")
            If idx > 0 Then
                sName = sNewUserEmail.Substring(idx).Trim
                sNewUserEmail = sNewUserEmail.Substring(0, idx).Trim
            End If

            Dim Usr As clsApplicationUser = App.UserWithSignup(sNewUserEmail, sName, "", String.Format("Created as '{0}'", sUserPosition), Nothing, True)  ' D2215
            ' D2681 ==
            If Not Usr Is Nothing Then
                Dim UWG As clsUserWorkgroup = App.DBUserWorkgroupByUserIDWorkgroupID(Usr.UserID, tWorkgroup.ID)     ' D0092 + D0502
                Dim GrpID As Integer = tWorkgroup.GetDefaultRoleGroupID(ecRoleLevel.rlApplicationLevel, ecRoleGroupType.gtWorkgroupManager)
                If Usr.CannotBeDeleted Then GrpID = tWorkgroup.GetDefaultRoleGroupID(ecRoleLevel.rlApplicationLevel, ecRoleGroupType.gtAdministrator) ' D0502
                If GrpID > 0 Then   ' D0502
                    If UWG Is Nothing Then
                        UWG = App.AttachWorkgroup(Usr.UserID, tWorkgroup, GrpID, String.Format("Attach '{0}' to workgroup as '{1}'", Usr.UserEmail, sUserPosition))    ' D0093 + D0502
                        If Not UWG Is Nothing Then tResult = UWG.UserID
                    Else
                        UWG.RoleGroupID = GrpID
                        If App.DBUserWorkgroupUpdate(UWG, False, String.Format("Set '{0}' as '{1}' for workgroup ", Usr.UserEmail, sUserPosition)) Then tResult = UWG.UserID ' D0502
                    End If
                End If
            End If
        End If
        Return tResult
    End Function
    ' D0296 ==

    ' D0922 ===
    Protected Sub lblEULAFiles_Load(ByVal sender As Object, ByVal e As EventArgs) Handles lblEULAFiles.Load
        If lblEULAFiles.Text = "" Then
            Dim files As ReadOnlyCollection(Of String) = MyComputer.FileSystem.GetFiles(MapPath(_URL_EULA), SearchOption.SearchTopLevelOnly, "*EULA*.htm*") ' D6104
            For Each sFile As String In files
                lblEULAFiles.Text += String.Format("<div style='margin:2px 1px; text-align:left'>&nbsp;&#149;&nbsp;<a href='' onclick='SetFile(""{0}""); return false;' class='dashed'>{0}</a>&nbsp;<a href='{2}{0}' target=_blank onclick='viewEULA(""{2}{0}"", ""{0}""); return false;'><img src='{1}view_sm.png' width=10 height=10 border=0 alt=''></a></div>", MyComputer.FileSystem.GetName(sFile), ImagePath, _URL_EULA) ' D6104
            Next
        End If
    End Sub
    ' D0922 ==

End Class
