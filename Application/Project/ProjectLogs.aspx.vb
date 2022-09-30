Partial Class ProjectLogsPage
    Inherits clsComparionCorePage

    Private _Project As clsProject = Nothing
    Private _ProjectChecked As Boolean = False

    Private Const MAX_LOGS_ROWS As Integer = 10000             ' D4434
    Private Const MAX_LOGS_SIZE As Integer = 1 * 1024 * 1024   ' D4434
    Public RealCount As Integer = -1                ' D4434

    Public Sub New()
        MyBase.New(_PGID_PROJECT_LOGS)
    End Sub

    Public ReadOnly Property CurrentProject As clsProject
        Get
            If _Project Is Nothing AndAlso Not _ProjectChecked Then
                Dim PrjID As Integer = CheckVar("prj_id", App.ProjectID)
                If PrjID = App.ProjectID AndAlso App.HasActiveProject Then _Project = App.ActiveProject
                If _Project Is Nothing AndAlso PrjID > 0 Then _Project = App.DBProjectByID(PrjID)
                If _Project IsNot Nothing AndAlso _Project.WorkgroupID <> App.ActiveWorkgroup.ID Then _Project = Nothing
                If _Project IsNot Nothing AndAlso Not App.CanUserModifyProject(App.ActiveUser.UserID, _Project.ID, App.ActiveUserWorkgroup, App.DBWorkspaceByUserIDProjectID(App.ActiveUser.UserID, _Project.ID), App.ActiveWorkgroup) Then _Project = Nothing
                _ProjectChecked = True
            End If
            Return _Project
        End Get
    End Property

    Public Function GetTitle() As String
        If CurrentProject IsNot Nothing Then
            Return String.Format("""{0}"" Model Logs:", SafeFormString(ShortString(CurrentProject.ProjectName, 45, True)))
        Else
            Return PageTitle(CurrentPageID)
        End If
    End Function

    Public Function GetData() As String
        Dim sData As String = ""
        If Not CanSeeLogs() Then Return sData   ' D7602

        Dim sSQL As String = String.Format("SELECT L.*, U.Email, U.FullName FROM Logs L LEFT JOIN Users U ON U.ID = L.UserID WHERE WorkgroupID={0} AND ObjectID={1} AND TypeID IN ({2},{3},{4},{5},{6},{7}) ORDER BY DT DESC", App.ActiveWorkgroup.ID, CurrentProject.ID, CInt(dbObjectType.einfProject), CInt(dbObjectType.einfProjectReport), CInt(dbObjectType.einfFile), CInt(dbObjectType.einfRTE), CInt(dbObjectType.einfOther), CInt(dbObjectType.einfSnapshot)) ' D3576 + D7487
        Dim tLogs As List(Of Dictionary(Of String, Object)) = App.Database.SelectBySQL(sSQL)

        Dim Lines As Integer = 0    ' D4434
        RealCount = tLogs.Count     ' D4434
        For Each tRow As Dictionary(Of String, Object) In tLogs
            ' ID, Date, ActionID, Action, TypeID, Type, ObjectID, User e-mail, User name, Event, Details
            Dim sAction As String = ""
            Dim sType As String = ""

            Select Case CType(tRow("ActionID"), dbActionType)
                Case dbActionType.actClose
                    sAction = "Close"
                Case dbActionType.actCreate
                    sAction = "Create"
                Case dbActionType.actCreateArchive
                    sAction = "Create Archive"
                Case dbActionType.actRestore    ' D7051
                    sAction = "Restore"         ' D7051
                Case dbActionType.actCredentialsLogon
                    sAction = "Credentials logon"
                Case dbActionType.actSSOLogin   ' D6532
                    sAction = "SSO logon"       ' D6532
                Case dbActionType.actDatagridUpload
                    sAction = "DataGrid update"
                Case dbActionType.actDelete
                    sAction = "Delete"
                Case dbActionType.actDownload
                    sAction = "Download"
                Case dbActionType.actEditLicense
                    sAction = "Edit License"
                Case dbActionType.actExtractArchive
                    sAction = "Extract Archive"
                Case dbActionType.actJoinMeeting
                    sAction = "Join session"
                Case dbActionType.actLicenseMessage
                    sAction = "License message"
                Case dbActionType.actLock
                    sAction = "Lock"
                Case dbActionType.actLogon
                    sAction = "Logon"
                Case dbActionType.actLogout
                    sAction = "Logout"
                Case dbActionType.actMakeJudgment
                    sAction = "Make judgment"
                Case dbActionType.actModify
                    sAction = "Modify"
                Case dbActionType.actOpen
                    sAction = "Open"
                Case dbActionType.actRedirect
                    sAction = "Redirect"
                Case dbActionType.actReportEnd
                    sAction = "Create report end"
                Case dbActionType.actReportStart
                    sAction = "Create report start"
                Case dbActionType.actSelect
                    sAction = "Select"
                Case dbActionType.actSendEmail
                    sAction = "Send e-mail"
                Case dbActionType.actSendRTE
                    sAction = "RTE submit"
                Case dbActionType.actServiceRTE
                    sAction = "Service RTE"
                Case dbActionType.actShellError
                    sAction = "Shell error"
                Case dbActionType.actShowMessage
                    sAction = "Show message"
                Case dbActionType.actShowRTE
                    sAction = "Show RTE message"
                Case dbActionType.actRASolveModel   ' D7510
                    sAction = "Solve"               ' D7510
                Case dbActionType.actStartMeeting
                    sAction = "Start meeting"
                Case dbActionType.actTokenizedURLLogon
                    sAction = "Logon by token/URL"
                Case dbActionType.actUnLock
                    sAction = "Unlock"
                Case dbActionType.actUpgrade
                    sAction = "Upgrade"
            End Select

            Select Case CType(tRow("TypeID"), dbObjectType)
                Case dbObjectType.einfFile
                    sType = "DB"
                Case dbObjectType.einfDatabase
                    sType = "file"
                Case dbObjectType.einfMessage
                    sType = "msg"
                Case dbObjectType.einfPage
                    sType = "page"
                Case dbObjectType.einfProject
                    sType = "project"
                Case dbObjectType.einfProjectReport
                    sType = "report"
                Case dbObjectType.einfRoleAction, dbObjectType.einfRoleGroup
                    sType = "permission"
                Case dbObjectType.einfRTE
                    sType = "RTE"
                Case dbObjectType.einfSurvey
                    sType = "Survey"
                Case dbObjectType.einfUser
                    sType = "user"
                Case dbObjectType.einfUserTemplate
                    sType = "user tpl"
                Case dbObjectType.einfUserWorkgroup
                    sType = "user wkg"
                Case dbObjectType.einfWebService
                    sType = "service"
                Case dbObjectType.einfWorkgroup
                    sType = "wkg"
                Case dbObjectType.einfWorkspace
                    sType = "workpace"
                Case dbObjectType.einfWorkgroupParameters
                    sType = "wkg param"
                Case dbObjectType.einfSnapshot  ' D3576
                    sType = "snapshot"  ' D3576
            End Select

            Dim sDT As String = CType(tRow("DT"), DateTime).ToString("yyyy/MM/dd HH:mm:ss")

            sData += String.Format("{0}[{1},'{2}',{3},'{4}',{5},'{6}',{7},'{8}','{9}','{10}','{11}']", IIf(sData = "", "", ","), _
                                       tRow("ID"), JS_SafeString(sDT), tRow("ActionID"), JS_SafeString(sAction), tRow("TypeID"), JS_SafeString(sType), tRow("ObjectID"), JS_SafeString(tRow("Email")), JS_SafeString(tRow("FullName")), JS_SafeString(tRow("Comment")), JS_SafeString(tRow("Result")))

            Lines += 1  ' D4434
            If Lines >= MAX_LOGS_ROWS OrElse sData.Length >= MAX_LOGS_SIZE Then Exit For ' D4434
        Next

        Return sData
    End Function

    Public Function CanSeeLogs() As Boolean
        Return App.isAuthorized AndAlso CurrentProject IsNot Nothing AndAlso HasPermissionByAction(ecActionType.at_slViewOwnWorkgroupLogs, False, True)
    End Function

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
    End Sub

    ' D7602 ===
    Private Sub ProjectLogsPage_InitComplete(sender As Object, e As EventArgs) Handles Me.InitComplete
        If CurrentProject Is Nothing Then
            If Not isSLTheme() Then
                FetchAccess(_PGID_PROJECTSLIST)
            End If
        End If
    End Sub
    ' D7602 ==

End Class
