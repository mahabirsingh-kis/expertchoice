Imports System.Threading
Imports DevExpress.Web.ASPxCallback

Partial Class WorkgroupTemplatesPages
    Inherits clsComparionCorePage

    Private Const _KEEP_STARTUP_PROJECTS_USERS As Boolean = True    ' D2815

    Private _Workgroup As clsWorkgroup = Nothing
    Private _UserWorkgroup As clsUserWorkgroup = Nothing    ' D4833
    Private _FilesList As List(Of String)           ' D1081 + D2601
    Private _isDemo As Boolean = False              ' D0357
    Private _FileListDT As Nullable(Of DateTime) = Nothing  ' D1194

    Public StartupProjects As String = ""           ' D2815

    Private Const RefreshTimeoutSeconds As Integer = 300    ' D1194

    ' D1081 ===
    Public ReadOnly Property isTemplates() As Boolean
        Get
            Return CheckVar("type", "").ToLower() <> "samples"
        End Get
    End Property
    ' D1081 ==

    ' D4833 ===
    Public ReadOnly Property UserWorkgroup As clsUserWorkgroup
        Get
            If _UserWorkgroup Is Nothing Then
                If CurrentWorkgroup IsNot Nothing Then _UserWorkgroup = App.DBUserWorkgroupByUserIDWorkgroupID(App.ActiveUser.UserID, CurrentWorkgroup.ID)
            End If
            Return _UserWorkgroup
        End Get
    End Property
    ' D4833 ==

    Public Property CurrentWorkgroup() As clsWorkgroup
        Get
            If _Workgroup Is Nothing Then
                Dim WGID As Integer = 0
                If Integer.TryParse(CheckVar(_PARAM_ID, WGID.ToString), WGID) Then
                    If WGID <= 0 AndAlso App.ActiveWorkgroup IsNot Nothing Then WGID = App.ActiveWorkgroup.ID   ' D6245
                    _Workgroup = App.DBWorkgroupByID(WGID)  ' D0476
                    If _Workgroup Is Nothing OrElse Not App.isWorkgroupAvailable(_Workgroup, App.ActiveUser, UserWorkgroup) Then _Workgroup = Nothing ' D0476 + D4833 + D6245
                End If
            End If
            Return _Workgroup
        End Get
        Set(value As clsWorkgroup)
            _Workgroup = value
        End Set
    End Property

    ' D1194 ===
    Private Property FileListDateTime() As DateTime
        Get
            If _FileListDT.HasValue Then Return _FileListDT.Value
            If Session("tpl_dt") IsNot Nothing Then _FileListDT = CType(Session("tpl_dt"), DateTime)
            If Not _FileListDT.HasValue Then _FileListDT = Now
            Return _FileListDT.Value
        End Get
        Set(value As DateTime)
            Session("tpl_dt") = value
            _FileListDT = value
        End Set
    End Property
    ' D1194 ==

    ' D2811 ===
    Public Function GetPath() As String
        If isTemplates Then
            Return CStr(IIf(App.isRiskEnabled, _FILE_DATA_TEMPLATES_RISK, _FILE_DATA_TEMPLATES))
        Else
            Return CStr(IIf(App.isRiskEnabled, _FILE_DATA_SAMPLES_RISK, _FILE_DATA_SAMPLES))
        End If
    End Function
    ' D2811 ==

    ' D0356 ===
    Public ReadOnly Property FilesList() As List(Of String) ' D2601
        Get
            If _FilesList Is Nothing Or FileListDateTime.AddSeconds(RefreshTimeoutSeconds) < Now Then   ' D1081 + D1194
                _FilesList = GetProjectFilesList(GetPath, SupportedProjectFiles4Upload)   ' D2601 + D2811
                FileListDateTime = Now  ' D1194
            End If
            Return _FilesList   ' D1081
        End Get
    End Property
    ' D0356 ==

    Public Sub New()
        MyBase.New(_PGID_ADMIN_WORKGROUP_TEMPLATES)
    End Sub

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        AlignHorizontalCenter = True    ' D1081
        AlignVerticalCenter = True      ' D1081

        btnOK.Text = ResString("btnOK")
        btnCancel.Text = ResString("btnCancel")
        MainModal.ProgressBlankImage = BlankImage ' D6315

        If Not IsPostBack AndAlso Not isCallback Then   ' D1081
            If CurrentWorkgroup.ID = App.ActiveWorkgroup.ID Then App.ActiveProjectsList = Nothing ' D1081
            ASPxRoundPanelLoading.HeaderText = ResString("lblPleaseWait")   ' D0360
            ClientScript.RegisterStartupScript(GetType(String), "InitForm", "InitForm();", True)

            Dim ExistedNames As New ArrayList   ' D1194
            lblExisted.Text = ""    ' D1081
            Dim Lst As List(Of clsProject) = App.DBProjectsByWorkgroupID(CurrentWorkgroup.ID)   ' D1081
            For Each tPrj As clsProject In Lst ' D0476
                If Not tPrj.isMarkedAsDeleted AndAlso ((isTemplates AndAlso (tPrj.ProjectStatus = ecProjectStatus.psTemplate OrElse tPrj.ProjectStatus = ecProjectStatus.psMasterProject)) Or (Not isTemplates AndAlso tPrj.ProjectStatus = ecProjectStatus.psActive)) Then ' D0499 + D1081 + D2479
                    lblExisted.Text += String.Format("<li>{0}</li>", ShortString(CType(IIf(tPrj.ProjectName = "", SafeFormString(tPrj.Passcode), SafeFormString(tPrj.ProjectName)), String), 60)) ' D0360 + D0563 + D1081
                    ExistedNames.Add(tPrj.ProjectName.Trim.ToLower) ' D1194
                End If
            Next

            ' D2814 ===
            ASPxPageControlSources.TabPages(0).Text = ResString(CStr(IIf(isTemplates, "lblAddWorkgroupTemplate", "lblAddWorkgroupSamples")))
            ASPxPageControlSources.TabPages(1).Text = _DB_DEFAULT_STARTUPWORKGROUP_NAME
            ASPxPageControlDest.TabPages(0).Text = ResString(CStr(IIf(isTemplates, "lblExistedWorkgroupTemplates", "lblExistedWorkgroupSamples")))
            If App.isStartupWorkgroup(CurrentWorkgroup) Then ASPxPageControlSources.TabPages(1).Visible = False ' D2815
            If App.StartupWorkgroup Is Nothing OrElse Not ASPxPageControlSources.TabPages(1).Visible Then  ' D2815
                ASPxPageControlSources.TabPages(1).Enabled = False
                ASPxPageControlSources.ActiveTabIndex = 0
            End If
            ' D2814 ==

            Dim sNoFiles As String = CStr(IIf(isTemplates, "msgNoTemplatesFound", "msgNoSamplesFound"))    ' D1081
            sNoFiles = String.Format("<div id='new'><i>{0}</i></div>", ResString(sNoFiles)) ' D2815

            If lblExisted.Text <> "" Then
                lblExisted.Text = String.Format("<ul type='square' style='margin:0px 0px 0px 3.5ex; text-align:left;'>{0}<div id='new'></div></ul>", lblExisted.Text)    ' D1081 + D1224
            Else
                lblExisted.Text = sNoFiles  ' D0360 + D1081 + D2815
            End If

            ' D1081 ===
            If FilesList.Count > 0 Then
                lblFilesList.Text = ""
                For i As Integer = 0 To FilesList.Count - 1
                    lblFilesList.Text += String.Format("<div id='t{0}'><input type='checkbox' id='tpl{0}' name='tpl{0}' value=""{4}"" {3} onclick='{2}' onchange='{2}' onkeydown='{2}'><label for='tpl{0}'>{1}</label></div>", i, SafeFormString(ShortString(FilesList(i), 60, True)), "InitButton();", IIf(ExistedNames.IndexOf(GetNameByFilename(FilesList(i)).ToLower) >= 0, "", " checked "), FilesList(i))   ' D0361 + D1194 + D1224
                Next
            Else
                lblFilesList.Text = sNoFiles     ' D0360 + D1081 + D2815
            End If
            ' D1081 ==

            ' D2815 ===
            If App.StartupWorkgroup IsNot Nothing Then
                Dim tPrjList As List(Of clsProject) = App.DBProjectsByWorkgroupID(App.StartupWorkgroup.ID)
                If tPrjList IsNot Nothing Then
                    lblStartup.Text = ""
                    StartupProjects = ""
                    Dim i As Integer = 0
                    For Each tprj As clsProject In tPrjList
                        If Not tprj.isMarkedAsDeleted AndAlso _
                           ((isTemplates AndAlso tprj.ProjectStatus = ecProjectStatus.psTemplate) OrElse _
                           (Not isTemplates AndAlso tprj.ProjectStatus = ecProjectStatus.psActive)) Then
                            lblStartup.Text += String.Format("<div id='t{0}'><input type='checkbox' id='prj{0}' name='ptj{0}' value=""{4}"" {3} onclick='{2}' onchange='{2}' onkeydown='{2}'><label for='prj{0}'>{1}</label></div>", i, SafeFormString(ShortString(tprj.ProjectName, 60, True)), "InitButton();", IIf(ExistedNames.IndexOf(tprj.ProjectName.ToLower) >= 0, "", " checked "), tprj.ID)
                            StartupProjects += CType(IIf(StartupProjects = "", "", ","), String) + String.Format("'{0}'", JS_SafeString(ShortString(tprj.ProjectName, 55, True)))
                            i += 1
                        End If
                    Next
                    If lblStartup.Text = "" Then lblStartup.Text = sNoFiles
                End If
            End If
            ' D2815 ==

            If Not CurrentWorkgroup.License.isValidLicense Then btnOK.Visible = False ' D2815

            ClientScript.RegisterStartupScript(GetType(String), "init", "onResize();", True)    ' D2814
        End If
    End Sub

    Protected Sub ASPxCallbackPasscode_Callback(source As Object, e As CallbackEventArgs) Handles ASPxCallbackPasscode.Callback
        Dim args As NameValueCollection = HttpUtility.ParseQueryString(URLDecode(e.Parameter))
        Dim sStep As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "step")) ' Anti-XSS
        Dim sCmd As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "cmd"))   ' Anti-XSS
        Dim sMode As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "mode"))    ' D2815 + Anti-XSS
        Dim iStep As Integer = 0
        ' D0357 ===
        Dim sMessage As String = ""
        If Integer.TryParse(sStep, iStep) Then
            If _isDemo Then
                Thread.Sleep(2000)
                sMessage = "[demo] " + sCmd
            Else

                If sMode = "0" Then ' Files
                    Dim sFilename As String = sCmd
                    If sCmd <> "" Then sFilename = GetPath() + sCmd ' D1081 + D2811
                    If IO.File.Exists(sFilename) Then
                        Dim sRes As String = DoCreateModel(sFilename)   ' D1081
                        ' D0360 ===
                        If sRes = "" Then
                            sMessage = String.Format(ResString("msgWTDecisionUploaded"), ShortString(sCmd, 38))
                        Else
                            sMessage = String.Format("<span class='error'>{1}: {0}</span>", sRes, ResString("lblError"))
                        End If
                    Else
                        sMessage = String.Format("<span class='error'>" + ResString("msgWTFileNotFound") + "</span>", ShortString(sCmd, 35))
                    End If
                    ' D0360 ==
                End If

                If sMode = "1" Then ' Startup_default
                    Dim PrjID As Integer = -1
                    If Integer.TryParse(sCmd, PrjID) AndAlso App.StartupWorkgroup IsNot Nothing Then
                        Dim tPrj As clsProject = App.DBProjectByID(PrjID)
                        If tPrj IsNot Nothing AndAlso tPrj.WorkgroupID = App.StartupWorkgroup.ID Then
                            Dim sRes As String = CopyProject(tPrj)
                            If sRes = "" Then
                                sMessage = String.Format(ResString("msgWTDecisionCopied"), ShortString(tPrj.ProjectName, 38))
                            Else
                                sMessage = String.Format("<span class='error'>{1}: {0}</span>", sRes, ResString("lblError"))
                            End If
                        End If
                    End If
                End If

            End If
            iStep += 1
            e.Result = String.Format("[{0}, '{1}']", iStep, JS_SafeString(ShortString(sMessage, 80)))
        Else
            iStep = 100000
            sMessage = "Something wrong. Reload page."
        End If
        ' D0357 ==
    End Sub
    ' D0356 ==

    ' D0357 ===
    Private Function DoCreateModel(sFilename As String) As String ' D1081
        Dim sError As String = ""   ' D2600
        If CurrentWorkgroup IsNot Nothing AndAlso CurrentWorkgroup.Status <> ecWorkgroupStatus.wsSystem Then CreateModelFromFile(App, sFilename, If(isTemplates, ecProjectStatus.psTemplate, ecProjectStatus.psActive), CurrentWorkgroup.ID, False, sError) ' D2600 + D2915
        Return sError
    End Function
    ' D0357 ==

    ' D2815 ===
    Private Function CopyProject(tSrcProject As clsProject) As String
        Dim sRes As String = ""

        If CurrentWorkgroup.License.isValidLicense AndAlso CurrentWorkgroup.License.CheckParameterByID(ecLicenseParameter.MaxProjectsTotal, Nothing, False) Then

            Dim fCopyOK As Boolean = False
            Dim tDestProject As clsProject = tSrcProject.Clone()
            tDestProject.PasscodeLikelihood = App.ProjectUniquePasscode("", -1)
            tDestProject.PasscodeImpact = App.ProjectUniquePasscode("", -1)
            tDestProject.WorkgroupID = CurrentWorkgroup.ID
            tDestProject.OwnerID = App.ActiveUser.UserID
            If App.DBProjectCreate(tDestProject, "Copy decision from startup workgroup") Then
                fCopyOK = App.DBProjectCopy(tSrcProject, clsProject.StorageType, tDestProject.ConnectionString, tDestProject.ProviderType, tDestProject.ID, False)  ' D3774
                If fCopyOK Then
                    Dim fHasECAM As Boolean = False
                    Dim UsersOrig As List(Of clsUser) = tDestProject.ProjectManager.UsersList
                    Dim Users As New List(Of clsUser)
                    Users.AddRange(UsersOrig.ToArray)
                    For Each tUser As clsUser In Users
                        If App.ActiveUser IsNot Nothing AndAlso String.Compare(App.ActiveUser.UserEmail, tUser.UserEMail, True) = 0 Then
                            fHasECAM = True
                        Else
                            If Not _KEEP_STARTUP_PROJECTS_USERS Then tDestProject.ProjectManager.DeleteUser(tUser)
                        End If
                    Next

                    If tSrcProject.ProjectStatus <> ecProjectStatus.psTemplate AndAlso tSrcProject.ProjectStatus <> ecProjectStatus.psMasterProject Then

                        Dim DefPMGrpID As Integer = CurrentWorkgroup.GetDefaultRoleGroupID(ecRoleLevel.rlModelLevel, ecRoleGroupType.gtProjectManager)
                        Dim DefEvalGrpID As Integer = CurrentWorkgroup.GetDefaultRoleGroupID(ecRoleLevel.rlModelLevel, ecRoleGroupType.gtEvaluator)
                        Dim DefUserGrpID As Integer = CurrentWorkgroup.GetDefaultRoleGroupID(ecRoleLevel.rlApplicationLevel, ecRoleGroupType.gtUser)

                        If App.ActiveUser IsNot Nothing AndAlso Not fHasECAM Then
                            App.AttachProject(App.ActiveUser, tDestProject, False, DefPMGrpID, "", False)
                        End If

                        If _KEEP_STARTUP_PROJECTS_USERS Then

                            Dim tUWList As List(Of clsUserWorkgroup) = App.DBUserWorkgroupsByWorkgroupID(CurrentWorkgroup.ID)
                            For Each tPrjUser As clsUser In Users
                                Dim tUser As clsApplicationUser = App.DBUserByEmail(tPrjUser.UserEMail)
                                If tUser IsNot Nothing Then
                                    Dim tUW As clsUserWorkgroup = clsUserWorkgroup.UserWorkgroupByUserIDAndWorkgroupID(tUser.UserID, CurrentWorkgroup.ID, tUWList)
                                    If tUW Is Nothing Then
                                        tUW = App.AttachWorkgroup(tUser.UserID, CurrentWorkgroup, DefUserGrpID, "Attach user to wkg from startup project")
                                        If tUW IsNot Nothing Then tUWList.Add(tUW)
                                    End If
                                    App.AttachProject(tUser, tDestProject, False, DefEvalGrpID, "Attach to project from startup", False)
                                End If
                                If App.ApplicationError.Status <> ecErrorStatus.errNone Then Exit For
                            Next

                        Else

                            For Each tUser As clsUser In Users
                                tDestProject.ProjectManager.DeleteUser(tUser)
                            Next
                        End If
                    End If
                Else

                    App.DBProjectDelete(tDestProject, True)
                    sRes = String.Format(ResString("msgWTCantCopyPrj"), ShortString(tSrcProject.ProjectName, 40, True))

                End If
            Else

                sRes = ResString("msgWTCantCreatePrj")

            End If
        Else

            sRes = ParseAllTemplates(App.LicenseErrorMessage(CurrentWorkgroup.License, ecLicenseParameter.MaxProjectsTotal, True), App.ActiveUser, App.ActiveProject) ' D2904

        End If

        If App.ApplicationError.Status <> ecErrorStatus.errNone Then sRes = App.ApplicationError.Message

        Return sRes
    End Function

    Private Sub WorkgroupTemplatesPages_PreInit(sender As Object, e As EventArgs) Handles Me.PreInit
        If CurrentWorkgroup Is Nothing OrElse Not App.CanUserDoAction(ecActionType.at_alManageAnyModel, UserWorkgroup, CurrentWorkgroup) Then FetchAccess(_PGID_ADMIN_WORKGROUPS) ' D4833
        CustomWorkgroupPermissions = CurrentWorkgroup ' D7270
        CurrentPageID = If(isTemplates, _PGID_ADMIN_WORKGROUP_TEMPLATES, _PGID_ADMIN_WORKGROUP_SAMPLES)   ' D1224
    End Sub

    '' D1081 ===
    'Private Function GetXMLPipeParamsFilename() As String
    '    Dim XMLFile As String = WebConfigOption(IIf(App.isRiskEnabled, WebOptions._OPT_DEFAULTPIPEPARAMS_RISK, WebOptions._OPT_DEFAULTPIPEPARAMS), "", True)    ' D2575
    '    If XMLFile <> "" Then If Not MyComputer.FileSystem.FileExists(_FILE_DATA_SETTINGS + XMLFile) Then XMLFile = ""
    '    If XMLFile = "" Then XMLFile = _FILE_SETTINGS_DEFPIPEPARAMS
    '    Return XMLFile
    'End Function
    '' D1081 ==

End Class
