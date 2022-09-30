Imports System.Collections.ObjectModel
Imports System.IO

Partial Class InstallPage
    Inherits clsComparionCorePage

    Private CanvasMasterDBExists As Boolean = False
    Private CanvasMasterDBVersion As String = ""
    Private CanvasProjectsDBExists As Boolean = False
    Private CanvasProjectsDBVersion As String = ""
    ' -D6423 ===
    'Private SpyronMasterDBExists As Boolean = False
    'Private SpyronMasterDBVersion As String = ""
    'Private SpyronProjectsDBExists As Boolean = False   ' D0375
    'Private SpyronProjectsDBVersion As String = ""      ' D0375
    ' -D6423 ==

    Public Sub New()
        MyBase.New(_PGID_DB_SETUP)  ' D0371
    End Sub

    Protected Sub Page_Init(sender As Object, e As EventArgs) Handles Me.Init
        AlignHorizontalCenter = False   ' D0585
        AlignVerticalCenter = False
        If IsPostBack OrElse isCallback OrElse isAJAX Then tbLogs.Text += vbCrLf
        InitStatus()
        ' D0370 ===

        If CanvasMasterDBExists AndAlso CanvasProjectsDBExists Then ' D0402
            'If Not App.isAuthorized Then Response.Redirect(_URL_ROOT, True) ' FetchAccess(_PGID_ERROR_403)
            'If Not App.CanDoAnySystemWorkgroupAction() Then FetchAccess(_PGID_ERROR_403)   ' -D0475
            ' D0475 ===
            If App.ActiveUser Is Nothing Then FetchAccess(_PGID_ERROR_403)
            If Not App.ActiveUser.CannotBeDeleted AndAlso Not App.CanUserDoSystemWorkgroupAction(ecActionType.at_slManageAnyWorkgroup, App.ActiveUser.UserID) Then FetchAccess(_PGID_ERROR_403) ' D0475
            ' D0475 ==
        End If
        ' D0370 ==
        If Not IsPostBack And Not isCallback Then
            If btnAdminDecisions.Visible Then btnAdminDecisions.OnClientClick = String.Format("document.location.href='{0}'; return false;", PageURL(_PGID_DB_DECISIONS)) ' D0371
            ' D3821 ===
            If Not btnAdminDecisions.Visible Then
                btnSysMessage.Visible = False
                btnSettings.Visible = False
            Else
                btnSysMessage.OnClientClick = String.Format("document.location.href='{0}'; return false;", PageURL(_PGID_SYSTEM_MESSAGE))   ' D0796
                btnSettings.OnClientClick = String.Format("document.location.href='{0}'; return false;", PageURL(_PGID_SYSTEM_SETTINGS))
                'btnSettings.Visible = WebConfigOption(_OPT_LIC_PSW_HASH, "", True) <> "" AndAlso Not App.isSelfHost     ' D3949 + D3965 -D7019
            End If
            If Not _OPT_USE_CUSTOM_SETTINGS Then btnSettings.Visible = False
            ' D3821 ==
        End If
    End Sub

    Protected Sub Page_PreRender(sender As Object, e As EventArgs) Handles Me.PreRender
        'tbLogs.Text += vbCrLf
        btnAdminDecisions.Visible = False
    End Sub

    Private Sub AddLog(sMessage As String)
        tbLogs.Text += Now.ToString("T") + ": " + sMessage + vbCrLf
        DebugInfo(sMessage)
    End Sub

    Private Function IsDBExists(DBType As ecDBType, ByRef sVersion As String) As Boolean
        Dim sDatabase As String = ""
        Dim fResult As Boolean = App.DBCheckDatabase(DBType, sVersion, sDatabase)   ' D0475
        Dim dbTypeName As String = ""
        Select Case DBType
            Case ecDBType.dbCanvasMaster
                dbTypeName = "Comparion Master"
            Case ecDBType.dbCanvasProjects
                dbTypeName = "Comparion Projects"
                ' -D6423 ===
                'Case ecDBType.dbSpyronMaster
                '    dbTypeName = "Spyron Master"
                'Case ecDBType.dbSpyronProjects
                '    dbTypeName = "Spyon Projects"
                ' -D6423 ==
        End Select
        Dim msg = String.Format("{0} {1}: {2}", dbTypeName, IIf(fResult, "exists", "missing"), IIf(sVersion <> "", sVersion, "undefined"))
        AddLog(msg)
        Return fResult
    End Function

    Private Sub InitStatus()
        CanvasMasterDBExists = IsDBExists(ecDBType.dbCanvasMaster, CanvasMasterDBVersion)
        CanvasProjectsDBExists = IsDBExists(ecDBType.dbCanvasProjects, CanvasProjectsDBVersion)
        'SpyronMasterDBExists = IsDBExists(ecDBType.dbSpyronMaster, SpyronMasterDBVersion)  ' -D6423
        'SpyronProjectsDBExists = IsDBExists(ecDBType.dbSpyronProjects, SpyronProjectsDBVersion) ' D0375 + D0377 + D0475 -D6423

        btnCreateAll.Enabled = Not CanvasMasterDBExists OrElse Not CanvasProjectsDBExists ' Or Not SpyronProjectsDBExists Or Not SpyronMasterDBExists ' D0375 + D0377 + D0475 + D6423
        If btnCreateAll.Enabled Then btnCreateAll.Focus()   ' D6446

        btnAdminDecisions.Visible = CanvasMasterDBExists And CanvasProjectsDBExists And (Not isDraftPage(_PGID_DB_DECISIONS) Or ShowDraftPages())     ' D0371 + D0373 + D0459

        ' D0781 ===
        Dim sSettings As String = ""
        If ShowDraftPages() Then
            For Each sName As String In ConfigurationManager.AppSettings.AllKeys
                sSettings += String.Format("<li>{0}: <b>{1}</b>", sName, ConfigurationManager.AppSettings(sName))
            Next
            sSettings = String.Format("Application settings: <a href='' onclick='ShowSettings(); return false;' class='actions dashed'><span id='settings_link'>Show</span></a><ul type='square' id='settings' style='display:none; margin-left:1em'>{0}</ul>", sSettings)
        End If
        ' D0781 ==

        Const sMissed As String = "<span class='error'>missed</span>"
        Const sExists As String = "<span style='color:#006633'>exists</span>"

        Dim sVM As String = ""  ' D3936
        If Request.IsLocal AndAlso DetectVirtualMachine() Then sVM = " [VM]" ' D3936

        lblInfo.Text = String.Format("<ul type='square' style='margin-left:2em' class='lst'>" +
                                        "<li>InstanceID: <b><a href='' class='actions dashed' onclick='CopyText(""{15}""); return false;'>{15}</a></b>" + vbCrLf +
                                        "<li>Server name: <b>{17}</b> {16}" + vbCrLf +
                                        "<li>DB instance: <b>{0}</b>" + vbCrLf +
                                        "<li>DB user name: <b>{1}</b>" + vbCrLf +
                                        "<li>DB provider type: <b>{2}</b>" + vbCrLf +
                                        "<li>Canvas MasterDB name: <b>{3}</b>" + vbCrLf +
                                        "<li>Canvas MasterDB status: <b>{4}</b>" + vbCrLf +
                                        "<li>Canvas ProjectsDB name: <b>{5}</b>" + vbCrLf +
                                        "<li>Canvas ProjectsDB status: <b>{6}</b>" + vbCrLf +
                                        "<li>Session timeout: <b>{11}</b> minutes" + vbCrLf +
                                        "<!--li>Lock timeout: <b>{12}</b> seconds" + vbCrLf +
                                        "<li>TeamTime default refresh: <b>{13}</b> seconds-->" + vbCrLf +
                                        "<li>FIPS mode: <b>{18}</b>" + vbCrLf +
                                        "<!--li>{14}</ul-->",
                                        App.CanvasMasterConnectionDefinition.Server,
                                        App.CanvasMasterConnectionDefinition.UserName + If(App.CanvasMasterConnectionDefinition.Trusted <> "", " {trusted}", ""),
                                        App.CanvasMasterConnectionDefinition.ProviderType.ToString,
                                        App.Options.CanvasMasterDBName,
                                        If(CanvasMasterDBExists, sExists, sMissed) + If(CanvasMasterDBVersion = "", "", ", " + String.Format("<span {0} title='{2}'>{1}</span>", If(_DB_MINVER_MASTERDB = CanvasMasterDBVersion, "", "class='error'"), CanvasMasterDBVersion, _DB_MINVER_MASTERDB)),
                                        App.Options.CanvasProjectsDBName,
                                        If(CanvasProjectsDBExists, sExists, sMissed) + If(CanvasProjectsDBVersion = "", "", ", " + String.Format("<span {0} title='{2}'>{1}</span>", If(_DB_MINVER_PROJECTSDB = CanvasProjectsDBVersion, "", "class='error'"), CanvasProjectsDBVersion, _DB_MINVER_PROJECTSDB)),
                                        "", "", "", "",
                                        SessionTimeout / 60, App.Options.ProjectLockTimeout, SynchronousRefresh,
                                        sSettings, JS_SafeString(App.GetInstanceID_AsString()), sVM, Environment.MachineName, Bool2YesNo(ECSecurity.ECSecurity.FIPS_MODE))  ' D0475 + D0496 + D0776 + D0781 + D3934 + D3936 + D6423 + D6689
        ' D0377 ==

        ' -D6423 === // after CanvasProject
        '"<li>Spyron MasterDB name: <b>{7}</b>" + vbCrLf +
        '"<li>Spyron MasterDB status: <b>{8}</b>" + vbCrLf +
        '"<li>Spyron ProjectsDB name: <b>{9}</b>" + vbCrLf +
        '"<li>Spyron ProjectsDB status: <b>{10}</b>" + vbCrLf +
        ' - D6423

        ' -D6423 === // after CanvasProject
        '                                App.Options.SpyronMasterDBName,
        '                                If(SpyronMasterDBExists, sExists, sMissed) + If(SpyronMasterDBVersion = "", "", ", " + SpyronMasterDBVersion),
        '                                App.Options.SpyronProjectsDBName,
        '                                If(SpyronProjectsDBExists, sExists, sMissed) + If(SpyronProjectsDBVersion = "", "", ", " + SpyronProjectsDBVersion),
        ' -D6423 ==

        ' -D3762
        '' D2228 ===
        'If App.Database.Connect Then
        '    Dim StData As List(Of Dictionary(Of String, Object)) = App.Database.SelectBySQL(String.Format("SELECT COUNT(status) as cnt, status FROM MASTER.DBO.SYSPROCESSES WHERE DB_NAME(DBID) = '{0}' AND DBID != 0 GROUP BY status ORDER BY status", App.Database.DatabaseName))
        '    If StData IsNot Nothing Then
        '        lblInfo.Text += "<li>DB connections: "
        '        For Each tConn As Dictionary(Of String, Object) In StData
        '            lblInfo.Text += String.Format("{0}:{1}; ", CStr(tConn("status")).Trim, tConn("cnt"))
        '        Next
        '        lblInfo.Text += "</li>"
        '    End If
        'End If
        '' D2228 ==


        App.ApplicationError.Reset()  ' D0475
    End Sub

    Private Sub InitPatches()
        Dim fHasEnabled As Boolean = False

        Dim cmver As String = CanvasMasterDBVersion.Replace(_DB_VERSION_STRING, "").Trim
        Dim cpver As String = CanvasProjectsDBVersion.Replace(_DB_VERSION_STRING, "").Trim
        'Dim smver As String = SpyronMasterDBVersion.Replace(_DB_VERSION_STRING, "").Trim       ' -D6423
        'Dim spver As String = SpyronProjectsDBVersion.Replace(_DB_VERSION_STRING, "").Trim   ' D0375 -D6423

        Try
            Dim AllFiles As ReadOnlyCollection(Of String) = MyComputer.FileSystem.GetFiles(_FILE_DATA_SQL)
            DebugInfo(String.Format("Scan folder '{0}' for patches...", _FILE_DATA_SQL))
            cblPatches.Items.Clear()
            For Each sFileName As String In AllFiles
                If Path.GetExtension(sFileName).ToLower = ".sql" AndAlso
                   Path.GetFileName(sFileName).ToLower.IndexOf("patch") >= 0 AndAlso
                   Path.GetFileName(sFileName).ToLower.IndexOf("~") <> 0 Then

                    Dim sName As String = Path.GetFileNameWithoutExtension(sFileName).ToLower

                    Dim fEnabled As Boolean = True
                    Dim sVersion As String = ""
                    For Each S As Char In sName
                        If (S >= "0" And S <= "9") Or S = "." Then sVersion += S
                    Next

                    Select Case sName(sName.Length - 1)
                        Case CChar("m")
                            If sVersion <> "" Then sName = "Patch for Canvas MasterDB " + _DB_VERSION_STRING + sVersion
                            fEnabled = CanvasMasterDBExists And (cmver = sVersion Or cmver = _DB_VERSION_UNKNOWN)
                        Case CChar("p")
                            If sVersion <> "" Then sName = "Patch for Canvas ProjectsDB " + _DB_VERSION_STRING + sVersion
                            fEnabled = CanvasProjectsDBExists And (cpver = sVersion Or cpver = _DB_VERSION_UNKNOWN)
                            ' -D6423 ===
                            'Case CChar("s")
                            '    If sVersion <> "" Then sName = "Patch for Spyron MasterDB " + _DB_VERSION_STRING + sVersion
                            '    fEnabled = SpyronMasterDBExists And (smver = sVersion Or smver = _DB_VERSION_UNKNOWN)
                            '    ' D0375 ===
                            'Case CChar("u")
                            '    If sVersion <> "" Then sName = "Patch for Survey ProjectsDB " + _DB_VERSION_STRING + sVersion
                            '    fEnabled = SpyronProjectsDBExists And (spver = sVersion Or spver = _DB_VERSION_UNKNOWN)
                            '    ' D0375 ==
                            ' -D6423 ==
                    End Select

                    Dim P As New ListItem

                    P.Value = Path.GetFileName(sFileName)
                    P.Text = sName.Replace("_", " ")
                    P.Enabled = fEnabled

                    Dim sPatch As String = File_GetContent(sFileName)
                    sPatch = ShortString(sPatch.Replace(vbTab, " ").Replace("  ", " "), 500, True)
                    Dim ce As Integer = sPatch.IndexOf("*/")
                    If ce > 0 Then sPatch = sPatch.Substring(0, ce).Replace("/*", "")
                    sPatch = sPatch.Trim
                    If sPatch <> "" Then P.Attributes.Add("title", sPatch.Replace("""", "'").Replace(vbCrLf, "<br>")) ' D0228

                    If P.Enabled Then
                        P.Attributes.Add("onclick", "SwitchPatch()")
                        P.Attributes.Add("onchange", "SwitchPatch()")
                        fHasEnabled = True
                    End If

                    cblPatches.Items.Add(P)
                End If
            Next
        Catch ex As Exception
            DebugInfo(ex.Message, _TRACE_RTE)   ' D0330
        End Try

        btnApplyPatch.Enabled = cblPatches.Items.Count <> 0 And fHasEnabled
        AddLog(String.Format("Found patches: {0}", cblPatches.Items.Count))
        If cblPatches.Items.Count = 0 Then
            lblPatches.InnerHtml = "<h5 class='gray' style='margin-top:7em'>Patches not found</h5>"
        Else
            If Not IsPostBack And Not isCallback Then ClientScript.RegisterStartupScript(GetType(String), "Init", "SwitchPatch();", True)
        End If
    End Sub

    Protected Sub cblPatches_Load(sender As Object, e As EventArgs) Handles cblPatches.Load
        If cblPatches.Items.Count = 0 And Not IsPostBack And Not isCallback Then
            InitPatches()
        End If
    End Sub

    Private Function CreateDatabase(DB As ecDBType) As Boolean
        Dim fProvider As DBProviderType = App.DefaultProvider   ' D0457
        Dim sDatabase As String = ""
        Dim sCreateFile As String = ""
        Dim sCreateSQL As String = ""
        Dim sError As String = ""
        Dim fResult As Boolean = False  ' D4138

        Select Case DB  ' D0375
            Case ecDBType.dbCanvasMaster
                fProvider = App.CanvasMasterConnectionDefinition.ProviderType     ' D0457
                sCreateFile = _FILE_SQL_CANVASDB
                sDatabase = App.Options.CanvasMasterDBName
            Case ecDBType.dbCanvasProjects
                fProvider = App.CanvasProjectsConnectionDefinition.ProviderType     ' D0457
                sCreateFile = _FILE_SQL_CANVAS_STREAMSDB
                sDatabase = App.Options.CanvasProjectsDBName
                ' -D6423 ===
                'Case ecDBType.dbSpyronMaster
                '    fProvider = App.SpyronMasterConnectionDefinition.ProviderType     ' D0457
                '    sCreateFile = _FILE_SQL_SPYRONDB
                '    sDatabase = App.Options.SpyronMasterDBName
                '    ' D0375 ===
                'Case ecDBType.dbSpyronProjects
                '    fProvider = App.SpyronMasterConnectionDefinition.ProviderType     ' D0457
                '    sCreateFile = _FILE_SQL_SPYRON_STREAMSDB
                '    sDatabase = App.Options.SpyronProjectsDBName
                '    ' D0375 ==
                ' -D6423 ==
            Case Else
                sError = "Unknown command"
        End Select

        If sError = "" Then
            sCreateSQL = File_GetContent(sCreateFile, sError)
            If sCreateSQL = "" Then sError = String.Format("SQL '{0}' script is empty!", sCreateFile)
        End If

        fResult = sError = ""   ' D4138

        If fResult Then
            DebugInfo(String.Format("Start to create database '{0}'...", sDatabase))

            Dim sConnString As String = clsDatabaseAdvanced.GetConnectionString(sDatabase)  ' D0459

            Dim isMasterConnected As Boolean = False
            Dim isComparionDBExists As Boolean = False
            Using MasterDB As New clsDatabaseAdvanced(clsDatabaseAdvanced.GetConnectionString("master"), fProvider)
                isMasterConnected = MasterDB.Connect
                Dim dbCountResult = ""
                Dim dbCountObj As Object = MasterDB.ExecuteScalarSQL(String.Format("select count(*) from sys.databases where name = '{0}'", sDatabase))
                If Not IsDBNull(dbCountObj) Then dbCountResult = CStr(dbCountObj)
                isComparionDBExists = (dbCountResult = "1")
            End Using

            If isMasterConnected And Not isComparionDBExists Then
                fResult = clsDatabaseAdvanced.CreateDatabase(sDatabase, fProvider, sError)
                isComparionDBExists = fResult
                AddLog(String.Format("Create database: {0}", IIf(fResult, "done", "failed")))
                If Not fResult Then AddLog("Error: " + sError) ' D0531
            End If

            If fResult Then
                fResult = clsDatabaseAdvanced.ExecuteSQLScript(sCreateSQL, sConnString, fProvider, sError, False)   ' D0475
                AddLog(String.Format("Create tables: {0}", IIf(fResult, "done", "failed")))
                If Not fResult Then AddLog("Error: " + sError) ' D0531
            End If

        End If
        ' D0499 ===
        Dim sLogMsg As String = CStr(IIf(fResult, String.Format("Database with tables '{0}' created successfully", sDatabase), String.Format("Error while create database '{0}': {1}", sDatabase, sError)))
        App.DBSaveLog(dbActionType.actCreate, dbObjectType.einfDatabase, -1, "Create " + DB.ToString, sLogMsg)
        AddLog(sLogMsg)
        ' D0499 ==
        Return fResult
    End Function

    Protected Sub btnApplyPatch_Click(sender As Object, e As EventArgs) Handles btnApplyPatch.Click
        Dim fHasChanged As Boolean = False
        For Each tItem As ListItem In cblPatches.Items
            If tItem.Enabled And tItem.Selected And tItem.Value <> "" Then
                AddLog(String.Format("Loading patch '{0}'...", tItem.Value))

                Dim sError As String = ""
                Dim sFilename As String = tItem.Value
                Dim sSQL As String = File_GetContent(_FILE_DATA_SQL + sFilename, sError)

                If sError = "" And sSQL <> "" Then

                    Dim sName As String = Path.GetFileNameWithoutExtension(sFilename).ToLower
                    Dim sDatabase As String = ""
                    Dim fProvider As DBProviderType = DBProviderType.dbptUnspecified     ' D0457
                    Dim DB As ecDBType

                    Select Case sName(sName.Length - 1)
                        Case CChar("m")
                            DB = ecDBType.dbCanvasMaster
                            fProvider = App.CanvasMasterConnectionDefinition.ProviderType ' D0457
                            sDatabase = App.Options.CanvasMasterDBName
                        Case CChar("p")
                            DB = ecDBType.dbCanvasProjects
                            fProvider = App.CanvasProjectsConnectionDefinition.ProviderType ' D0457
                            sDatabase = App.Options.CanvasProjectsDBName
                            ' -D6423 ===
                            'Case CChar("s")
                            '    DB = ecDBType.dbSpyronMaster
                            '    fProvider = App.SpyronMasterConnectionDefinition.ProviderType ' D0457
                            '    sDatabase = App.Options.SpyronMasterDBName
                            '    ' D0375 ===
                            'Case CChar("u")
                            '    DB = ecDBType.dbSpyronProjects
                            '    fProvider = App.SpyronMasterConnectionDefinition.ProviderType ' D0457
                            '    sDatabase = App.Options.SpyronProjectsDBName
                            '    ' D0375 ==
                            ' -D6423 ==
                    End Select

                    If sDatabase <> "" Then

                        Dim fResult As Boolean = clsDatabaseAdvanced.ExecuteSQLScript(sSQL, clsDatabaseAdvanced.GetConnectionString(sDatabase), fProvider, sError, True) ' D0459 + D0475
                        ' D0420 ===
                        Dim sMessage As String = CStr(IIf(fResult, String.Format("Database '{0}' patched successfully", sDatabase), String.Format("Error patch for DB '{0}': {1}", sDatabase, sError)))
                        AddLog(sMessage)
                        App.DBSaveLog(dbActionType.actApplyPatch, dbObjectType.einfDatabase, -1, sDatabase, String.Format("Apply Patch '{0}': {1}", sFilename, sMessage))   ' D0496
                        ' D0420 ==
                        If fResult Then fHasChanged = True

                    Else
                        AddLog("Can't detect database")
                    End If

                Else
                    AddLog(CStr(IIf(sError <> "", String.Format("Error occured: {0}", sError), String.Format("Can't load SQL from '{0}'", sFilename))))
                End If

            End If
        Next
        If fHasChanged Then
            App.ResetDBChecks() ' D0549
            InitStatus()
            InitPatches()
        End If
    End Sub

    ' D0375 ===
    Protected Sub btnCreateAll_Click(sender As Object, e As EventArgs) Handles btnCreateAll.Click
        ' D4138 ===
        Dim fResult As Boolean = True
        If Not CanvasMasterDBExists Then fResult = CreateDatabase(ecDBType.dbCanvasMaster)
        If fResult AndAlso Not CanvasProjectsDBExists Then fResult = CreateDatabase(ecDBType.dbCanvasProjects)
        'If fResult AndAlso Not SpyronMasterDBExists AndAlso _FILE_SQL_SPYRONDB <> "" Then fResult = CreateDatabase(ecDBType.dbSpyronMaster) ' D0377 -D6423
        'If fResult AndAlso Not SpyronProjectsDBExists AndAlso _FILE_SQL_SPYRON_STREAMSDB <> "" Then fResult = CreateDatabase(ecDBType.dbSpyronProjects) ' D0377 + D0475 -D6423
        ' D4138 ==
        App.ResetDBChecks()   ' D0490
        App.ApplicationError.Reset()    ' D0612
        App.CheckCanvasMasterDBDefaults() ' D0490
        InitStatus()
        If fResult Then
            ' D6446 ===
            'AddLog("Please close all of your browsers to clear the cache.")
            tbLogs.Text += String.Format(vbNewLine + vbNewLine + "Press ""Continue"" button or return back to the main page and try to login as '{0}'." + vbNewLine + vbNewLine, _DB_DEFAULT_ADMIN_LOGIN)
            SetCookie(_COOKIE_EMAIL, _DB_DEFAULT_ADMIN_LOGIN, True, True)
            SetCookie(_COOKIE_PASSWORD, _DB_DEFAULT_ADMIN_PSW, True, True)
            SetCookie(_COOKIE_MEETING_ID, "")
            SetCookie(_COOKIE_REMEMBER, "0")
            btnReturn.Text = ResString("btnContinue")
            Dim sURL As String = String.Format("{0}={1}&{2}={3}", _PARAM_EMAIL, HttpUtility.UrlEncode(_DB_DEFAULT_ADMIN_LOGIN), _PARAM_PASSWORD, HttpUtility.UrlEncode(_DB_DEFAULT_ADMIN_PSW))
            btnReturn.OnClientClick = String.Format("loadURL('../?{0}={1}'); return false;", _PARAMS_KEY(0), JS_SafeString(EncodeURL(sURL, App.DatabaseID)))
            btnReturn.Focus()
            ' D6446 ==
        End If
    End Sub
    ' D0375 ==

End Class
