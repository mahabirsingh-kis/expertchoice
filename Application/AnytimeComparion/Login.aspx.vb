Imports Pages.external_classes
Imports System.Web.Services

Public Class Login
    Inherits Page
    Public Shared debugging As String
    Public App As clsComparionCore

#Region "Page load"
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim app = CType(Session("App"), clsComparionCore)
        Dim messageError = ""
        Dim isMasterExists = app.isCanvasMasterDBValid
        Dim isProjectsExists = app.isCanvasProjectsDBValid
        Dim isDbExists = (isMasterExists And isProjectsExists)

        If Not isDbExists Then
            Dim dbName = ""

            If Not isProjectsExists Then
                dbName = app.Options.CanvasProjectsDBName
            End If

            If Not isMasterExists Then
                dbName = app.Options.CanvasMasterDBName
            End If

            messageError = "<div style='text-align:center' class='alert-box alert radius tt-alert msg-stat' data-alert>" & String.Format(TeamTimeClass.ResString("msgErrorDBConnection"), dbName) & "<a href='#' class='close'>&times;</a></div>"
        End If

        If Session("User") IsNot Nothing Then
        End If

        If Request.QueryString.HasKeys AndAlso Request.QueryString("passcode") IsNot Nothing AndAlso Request.QueryString("passcode").ToString() <> "" Then
            Dim passcode As String = Request.QueryString("passcode")
            AccessCodeTxt.Value = passcode
            Session("passcode") = passcode
        End If

        Dim userRole = False

        If app.ActiveUser IsNot Nothing Then
            userRole = isPM()
        End If

        If app.ActiveUser IsNot Nothing Then
            'UserName.InnerHtml = app.ActiveUser.UserName
        End If

        HttpContext.Current.Session("isMember") = userRole

        If app.ActiveUser IsNot Nothing Then

            If HttpContext.Current.Session("LoggedInViaHash") IsNot Nothing AndAlso CBool(HttpContext.Current.Session("LoggedInViaHash")) Then
            Else
                If Not HttpContext.Current.Request.Path.EndsWith("my-projects.aspx", StringComparison.InvariantCultureIgnoreCase) AndAlso Request.QueryString("hash") Is Nothing Then Response.Redirect("~/pages/my-projects.aspx")
            End If
        End If

        If Request.Cookies("Filters") Is Nothing Then
            Dim filter = New HttpCookie("Filters")
            filter.Values.Add("ProjectStatus", "1")
            filter.Values.Add("ProjectAccess", "1")
            filter.Values.Add("LastAccess", "0")
            filter.Values.Add("LastModified", "0")
            filter.Values.Add("DateCreated", "0")
            filter.Values.Add("OverallJudgmentProcess", "0")
            filter.Expires = DateTime.Now.AddDays(10)
            Response.Cookies.Add(filter)
        End If

        If Request.Cookies("HideWarningMessage") Is Nothing Then
            Dim warningCookie = New HttpCookie("HideWarningMessage", "1") With {
            .HttpOnly = True,
            .Expires = DateTime.Now.AddDays(1)
        }
            Response.Cookies.Add(warningCookie)
        End If

        If Request.QueryString("noMsg") IsNot Nothing AndAlso Request.QueryString("noMsg") = "1" Then
            Response.Cookies("HideWarningMessage").Value = "1"
        End If

        Div2.InnerHtml = messageError
        hashLinkMessage.InnerText = GetLastHash()
        If hashLinkMessage.InnerText.Length > 0 Then
            hashLinkMessage.InnerHtml = "<span>Click <a href='/?hash=" + hashLinkMessage.InnerText + "'>here</a> to return to your evaluation.</span>"
        Else
            hashLinkMessage.InnerHtml = "<span>If you have a Comparion evaluation link, please use it to return to your evaluation.</span>"
        End If
        modalTitle.InnerText = TeamTimeClass.ResString("lblPasswordRestore")
        msgPasswordRestore.InnerText = TeamTimeClass.ResString("msgPasswordRestore")
        Session("thispage") = Page
    End Sub

    Public Shared Function GetLastHash() As String
        If HttpContext.Current.Request.Cookies("LastHash") IsNot Nothing Then
            Return HttpContext.Current.Request.Cookies("LastHash").Value
        Else
            Return ""
        End If
    End Function

#End Region

#Region "Common function"
    Public Shared Function isPM() As Boolean
        Dim _isPM = False
        Dim App = CType(HttpContext.Current.Session("App"), clsComparionCore)

        If App.ActiveRoleGroup Is Nothing Then
            Return False
        End If

        If App.ActiveRoleGroup.GroupType <> ecRoleGroupType.gtUser Then
            _isPM = True
        End If

        Return _isPM
    End Function
#End Region

    'login function
    <WebMethod(EnableSession:=True)>
    Public Shared Function login(ByVal email As String, ByVal password As String, ByVal passcode As String, ByVal rememberme As String, ByVal MeetingID As String, ByVal meeting_name As String, ByVal meeting_email As String) As Object
        Dim context As HttpContext = HttpContext.Current

        If context.Session(Constants.Sess_LoginMethod) IsNot Nothing Then

            If CInt(context.Session(Constants.Sess_LoginMethod)) <> 1 Then
                context.Session(Constants.Sess_LoginMethod) = 0
            End If
        Else
            context.Session(Constants.Sess_LoginMethod) = 0
        End If

        'LoadLanguange()
        Dim fUserExist As Boolean = True
        Dim success As Boolean = False
        Dim message As String = ""
        Dim sPasscode = ""
        Dim userrole = False
        Dim App = CType(context.Session("App"), clsComparionCore)
        Dim passwordAuth = True

        If passcode <> "" Then
            Diagnostics.Trace.Write("-----------" & passcode & "-----------------")
            Return loginbyPasscode(App, email, password, passcode)
        End If

        If MeetingID <> "" Then
            Return loginbyMeetingID(App, MeetingID, meeting_email, meeting_name)
        End If

        Dim AuthRes = ecAuthenticateError.aeUnknown

        If context IsNot Nothing AndAlso context.Session IsNot Nothing Then

            If email <> "pm" AndAlso email <> "evaluator" AndAlso email <> "participant" Then
                Dim LastWorkgroupID = -1
                Dim currentUser = App.DBUserByEmail(email)

                If currentUser IsNot Nothing Then
                    LastWorkgroupID = currentUser.Session.WorkgroupID
                End If

                If context.Session("passcode") IsNot Nothing AndAlso context.Session("passcode").ToString() <> "" Then
                    sPasscode = If(sPasscode = "" OrElse sPasscode = "0", context.Session("passcode").ToString(), sPasscode)
                End If
                AuthRes = App.Logon(email, password, sPasscode, False, True, False)
                context.Session("App") = App

                Select Case AuthRes
                    Case ecAuthenticateError.aeWrongPassword

                        If password = "" Then
                            passwordAuth = False
                        End If

                    Case Else
                End Select

                If AuthRes <> ecAuthenticateError.aeNoErrors Then
                    message = ProcessLoginAuthentication(App, AuthRes, email)
                    If ecAuthenticateError.aeNoUserFound = AuthRes Then fUserExist = False
                Else
                    Diagnostics.Trace.Write("-4-")
                    Diagnostics.Trace.Write(vbLf & "Success Login!")
                    App.ActiveUser.Session.WorkgroupID = LastWorkgroupID
                    App.DBUserUpdate(App.ActiveUser, False)
                    Dim lst = App.DBProjectsByWorkgroupID(LastWorkgroupID)
                    App.ActiveProject = lst.FirstOrDefault()
                    'Dim workgroup = App.DBWorkgroupByID(LastWorkgroupID)
                    App.ActiveWorkgroup = App.DBWorkgroupByID(LastWorkgroupID)
                    Dim user = App.ActiveUser
                    context.Session("User") = user
                    context.Session("topHeader") = True
                    context.Session("WorkgroupName") = App.ActiveWorkgroup.Name
                    context.Session("ProjectName") = App.ActiveProject.ProjectName
                    context.Response.Cookies("fullname").Value = user.UserName.ToString()
                    Diagnostics.Trace.Write("user:" & user.UserName)
                    success = True
                    fUserExist = True
                End If
            Else

                If email = "pm" Then
                    context.Session("UserType") = "pm"
                End If

                If email = "evaluator" Then
                    context.Session("UserType") = "evaluator"
                End If

                If email = "participant" Then
                    context.Session("UserType") = "participant"
                End If

                success = True
            End If
        End If

        Dim mobile As Boolean = False

        If context.Session("mobile") IsNot Nothing AndAlso CBool(context.Session("mobile")) Then
            mobile = True
        End If

        userrole = isPM()
        Dim output As Object = New With {
            Key .success = success,
            Key .message = message,
            Key .mobile = mobile,
            Key .teamtime = False,
            Key .userrole = userrole,
            Key .passwordAuth = passwordAuth,
            Key .userExist = fUserExist,
            Key .is_invalid = fUserExist AndAlso passwordAuth
        }

        If App.ActiveUser IsNot Nothing Then

            If rememberme = "True" Then
                context.Response.Cookies("usernam").Expires = DateTime.Now.AddDays(10)
                context.Response.Cookies("passwor").Expires = DateTime.Now.AddDays(10)
                context.Response.Cookies("rmberme").Expires = DateTime.Now.AddDays(10)
                context.Response.Cookies("fullname").Expires = DateTime.Now.AddDays(10)
            Else
                context.Response.Cookies("usernam").Expires = DateTime.Now.AddDays(-1)
                context.Response.Cookies("passwor").Expires = DateTime.Now.AddDays(-1)
                context.Response.Cookies("rmberme").Expires = DateTime.Now.AddDays(-1)
                context.Response.Cookies("fullname").Expires = DateTime.Now.AddDays(-1)
            End If

            context.Response.Cookies("rmberme").Value = "1"
            context.Response.Cookies("usernam").Value = email
            context.Response.Cookies("passwor").Value = password
        Else
            context.Response.Cookies("usernam").Expires = DateTime.Now.AddDays(-1)
            context.Response.Cookies("passwor").Expires = DateTime.Now.AddDays(-1)
            context.Response.Cookies("rmberme").Expires = DateTime.Now.AddDays(-1)
            context.Response.Cookies("fullname").Expires = DateTime.Now.AddDays(-1)
        End If

        context.Session("isMember") = userrole
        SiteMaster.storepageinfo()

        Dim oSerializer = New Script.Serialization.JavaScriptSerializer()
        Return oSerializer.Serialize(output)
    End Function

    'loading all languages
    Public Shared Sub LoadLanguange()
        Dim App = CType(HttpContext.Current.Session("App"), clsComparionCore)
        Dim Path = HttpContext.Current.Server.MapPath("~/Res/resx/English.aspx")
        Dim SampleUrl = IO.Path.GetFullPath(IO.Path.Combine(Path, "Res/resx/"))
        Dim languagefolder = TeamTimeClass.LanguagesScanFolder(Path)
        TeamTimeClass.language = clsLanguageResource.LanguageByCode("Resource1", languagefolder)
        App.CurrentLanguage = TeamTimeClass.language
    End Sub

    'checking login authentication
    Private Shared Function ProcessLoginAuthentication(ByVal app As clsComparionCore, ByVal authRes As ecAuthenticateError, ByVal email As String) As String
        Dim message = TeamTimeClass.ParseAllTemplates(app.GetMessageByAuthErrorCode(authRes), app.ActiveUser, Nothing)

        If authRes = ecAuthenticateError.aeWrongPassword Then

            If Single.Parse(app.CanvasMasterDBVersion) >= 0.9996 AndAlso app.Database.Connect() AndAlso email <> "" Then

                Try
                    Dim user = app.DBUserByEmail(email)

                    If user IsNot Nothing Then
                        'Dim newStatus = GetWrongPasswordAttemptsFromLog(app, user)
                        'Dim oldStatus = user.PasswordStatus
                        user.PasswordStatus = GetWrongPasswordAttemptsFromLog(app, user)

                        If user.PasswordStatus <> GetWrongPasswordAttemptsFromLog(app, user) Then
                            Dim sqlParams = New List(Of Object) From {
                                email
                            }
                            app.Database.ExecuteSQL($"UPDATE {clsComparionCore._TABLE_USERS} SET PasswordStatus = {user.PasswordStatus} WHERE {clsComparionCore._FLD_USERS_EMAIL}=?", sqlParams)

                            If user.PasswordStatus = Consts._DEF_PASSWORD_ATTEMPTS Then
                                CheckUserPasswordStatusAndSendEmail(app, user, Consts._DEF_PASSWORD_ATTEMPTS)
                            End If

                            Threading.Thread.Sleep(1000)
                        End If

                        If user.PasswordStatus >= Consts._DEF_PASSWORD_ATTEMPTS Then
                            app.DBUpdateDateTime(clsComparionCore._TABLE_USERS, clsComparionCore._FLD_USERS_LASTVISITED, user.UserID)
                            authRes = ecAuthenticateError.aeUserLockedByWrongPsw
                            message = TeamTimeClass.ParseAllTemplates(app.GetMessageByAuthErrorCode(authRes), app.ActiveUser, Nothing)
                        End If

                        app.DBSaveLogonEvent(email, authRes, ecAuthenticateWay.awRegular, HttpContext.Current.Request, message)
                    End If

                Catch ex As Exception
                End Try
            End If
        End If

        If authRes = ecAuthenticateError.aeUserLockedByWrongPsw Then
            message = String.Format(message, "Password Help?")
            message += "<br> " & String.Format(TeamTimeClass.ResString("msgAuthContact"), WebOptions.SystemEmail).Replace("class='error'", "style='text-decoration: underline;'")
        ElseIf authRes <> ecAuthenticateError.aeWrongPassword Then
            Dim passCode = CStr(HttpContext.Current.Session(Constants.Sess_SignUp_Passcode))

            If Not String.IsNullOrEmpty(passCode) Then
                Dim project = app.DBProjectByPasscode(passCode)
                Dim newMessage = AnytimeClass.GetProjectInAccessibleMessage(project, email, False)
                message = If(String.IsNullOrEmpty(newMessage), message, newMessage)
            End If
        End If

        Return message
    End Function

    'login by passcode/access code
    Public Shared Function loginbyPasscode(ByVal App As clsComparionCore, ByVal email As String, ByVal password As String, ByVal passcode As String) As Object
        Dim success As Boolean = False
        Dim message As String = ""
        Dim mobile As Boolean = False
        Dim teamtime As Boolean = False
        Dim anytime As Boolean = False
        Dim passwordAuth = True
        Dim context As HttpContext = HttpContext.Current

        If App.DBProjectByPasscode(passcode) IsNot Nothing Then
            Dim AuthRes = App.Logon(email, password, passcode, False, True, False)

            If AuthRes = ecAuthenticateError.aeNoErrors Then
                context.Session("User") = App.ActiveUser
                context.Response.Cookies("fullname").Value = App.ActiveUser.UserName.ToString()

                context.Session("ProjectName") = App.ActiveProject.ProjectName
                context.Session("WorkgroupName") = App.ActiveWorkgroup.Name



                If App.ActiveProject.isTeamTime = True Then
                    success = True
                    teamtime = True
                Else
                    success = True
                    anytime = True
                    StartAnytime(App.ProjectID)
                End If
            ElseIf AuthRes = ecAuthenticateError.aeWrongPassword Then

                If password = "" Then
                    passwordAuth = False
                End If
            End If

            If AuthRes <> ecAuthenticateError.aeNoErrors Then
                message = TeamTimeClass.ParseAllTemplates(App.GetMessageByAuthErrorCode(AuthRes), App.ActiveUser, Nothing)
            End If
        Else
            success = False
            message = "The access code you have selected is not recognized. Please check the spelling and try again. If it still does not work, please contact the project manager to get the correct access code"
        End If

        Dim output As Object = New With {
            Key .success = success,
            Key .message = message,
            Key .mobile = mobile,
            Key .teamtime = teamtime,
            Key .passwordAuth = passwordAuth,
            Key .anytime = anytime
        }
        Dim oSerializer = New Script.Serialization.JavaScriptSerializer()
        Return oSerializer.Serialize(output)
    End Function

    'login by meeting id
    Public Shared Function loginbyMeetingID(ByVal App As clsComparionCore, ByVal MeetingID As String, ByVal meeting_email As String, ByVal meeting_name As String) As Object
        Dim success As Boolean = False
        Dim message As String = ""
        Dim mobile As Boolean = False
        Dim teamtime As Boolean = False
        Dim context As HttpContext = HttpContext.Current

        If MeetingID.Contains("-") Then
            MeetingID = MeetingID.Replace("-", "")
        End If

        Dim meet = CType(App.DBProjectByMeetingID(Convert.ToInt64(MeetingID)), clsProject)

        If meet IsNot Nothing Then
            Dim fUserNotExists = False
            'Dim sPasscode = meet.Passcode

            If App.DBUserByEmail(meeting_email) Is Nothing Then
                'Dim CurrentProject = App.DBProjectByPasscode(meet.Passcode)

                If App.DBProjectByPasscode(meet.Passcode).PipeParameters.TeamTimeAllowMeetingID Then
                    Dim sUserExists = ""
                    App.UserWithSignup(meeting_email, meeting_name, "admin", "Quick Sign-up", sUserExists, True)
                    fUserNotExists = True
                Else
                    Return False
                End If
            End If

            Dim user = App.DBUserByEmail(meeting_email)
            'Dim email = user.UserEmail
            'Dim password = user.UserPassword
            Dim AuthRes = App.Logon(user.UserEmail, user.UserPassword, meet.Passcode, False, True, False)
            context.Session("User") = App.ActiveUser
            context.Session("meetingid") = MeetingID
            context.Response.Cookies("fullname").Value = App.ActiveUser.UserName.ToString()
            context.Session("Project") = meet.ProjectName

            If context.Session("mobile") IsNot Nothing AndAlso CBool(context.Session("mobile")) Then
                mobile = True
            End If

            If App.ActiveProject.isTeamTime = True Then
                success = True
                teamtime = True
            Else
                success = True
                teamtime = False
            End If

            Dim URlRedirection = ""

            If App.ActiveProject.isTeamTime Then
            End If

            If App.ActiveProject IsNot Nothing Then

                If Not isPM() Then
                End If
            End If

            SiteMaster.storepageinfo()
            Dim output As Object = New With {
                Key .success = success,
                Key .message = message,
                Key .mobile = mobile,
                Key .teamtime = teamtime,
                Key .url = URlRedirection
            }
            Dim oSerializer = New Script.Serialization.JavaScriptSerializer()
            Return oSerializer.Serialize(output)
        Else
            Dim output As Object = New With {
                Key .success = False,
                Key .message = "Invalid Details. Please try again."
            }
            Dim oSerializer = New Script.Serialization.JavaScriptSerializer()
            Return oSerializer.Serialize(output)
        End If
    End Function

    'start project for survey
    <WebMethod(EnableSession:=True)>
    Public Shared Function StartAnytime(ByVal projID As Integer) As Object
        Dim output As Object = Nothing
        Dim fPass = True

        Try
            Dim App = CType(HttpContext.Current.Session("App"), clsComparionCore)
            App.ProjectID = projID
            Dim Project = App.DBProjectByID(projID)
            'Dim isAdmin = checkRoleGroup

            If checkRoleGroup AndAlso App.ActiveProject.ProjectManager.GetUserByEMail(App.ActiveUser.UserEmail) Is Nothing Then
                'Dim adminEmail = App.ActiveUser.UserEmail
                App.ActiveProject.ProjectManager.AddUser(App.ActiveUser.UserEmail)
            End If

            If App.HasActiveProject() Then

                If Project.isTeamTime Then
                    fPass = False
                Else
                    App.ActiveProject.isTeamTimeLikelihood = False
                    App.ActiveProject.isTeamTimeImpact = False
                    App.ActiveProject.isTeamTime = False
                    'Dim sss = CType(Project.HierarchyObjectives.GetLevelNodes(0)(0), clsNode)
                    HttpContext.Current.Session("Sess_WrtNode") = CType(Project.HierarchyObjectives.GetLevelNodes(0)(0), clsNode)
                    HttpContext.Current.Session(Constants.Sess_ShowEqualOnce) = Nothing
                    AnytimeClass.JudgmentsSaved = False
                    If Not Project.isValidDBVersion AndAlso Project.isDBVersionCanBeUpdated Then App.DBProjectUpdateToLastVersion(Project)

                    If Project.isValidDBVersion Then
                        Project.StatusDataLikelihood = 1
                        Project.StatusDataImpact = 1
                        Project.isOnline = True
                    End If

                    SiteMaster.storepageinfo()
                End If
            End If

            Dim groupId = CStr(HttpContext.Current.Session(Constants.Sess_RoleGroup))
            addUsertoGroup(groupId)
            LoadLanguange()

            Dim context As HttpContext = HttpContext.Current
            context.Session("topHeader") = True
            context.Session("WorkgroupName") = App.ActiveWorkgroup.Name
            context.Session("ProjectName") = App.ActiveProject.ProjectName

            output = New With {
                Key .name = Project.ProjectName,
                Key .owner = App.ActiveProject.ProjectManager.User.UserName,
                Key .start_anytime = fPass,
                Key .passcode = Project.Passcode
            }
        Catch e As Exception
        End Try

        Try
            Dim oSerializer = New Script.Serialization.JavaScriptSerializer()
            Return oSerializer.Serialize(output)
        Catch e As Exception
            'Dim [error] = e
            Return JsonConvert.SerializeObject(output, Formatting.Indented, New JsonSerializerSettings With {
                .ReferenceLoopHandling = ReferenceLoopHandling.Serialize
            })
        End Try
    End Function

    'add users in group
    Public Shared Sub addUsertoGroup(ByVal groupId As String)
        If groupId IsNot Nothing AndAlso groupId <> "" Then
            Dim app = CType(HttpContext.Current.Session("App"), clsComparionCore)
            Dim group = CType(app.ActiveProject.ProjectManager.CombinedGroups.GetGroupByID(Convert.ToInt32(groupId)), clsCombinedGroup)

            If group IsNot Nothing Then
                Dim ahpuser = app.ActiveProject.ProjectManager.GetUserByEMail(app.ActiveUser.UserEmail)

                If Not group.ContainsUser(ahpuser) Then
                    group.UsersList.Add(ahpuser)
                    app.ActiveProject.SaveStructure("Update user group")
                End If
            End If
        End If
    End Sub

    'check logged in user role
    Private Shared ReadOnly Property checkRoleGroup As Boolean
        Get
            Dim isAdmin = False
            Dim App = CType(HttpContext.Current.Session("App"), clsComparionCore)

            Select Case App.ActiveRoleGroup.GroupType
                Case ecRoleGroupType.gtAdministrator, ecRoleGroupType.gtWorkgroupManager, ecRoleGroupType.gtECAccountManager, ecRoleGroupType.gtProjectManager, ecRoleGroupType.gtProjectOrganizer
                    isAdmin = True
            End Select

            Return isAdmin
        End Get
    End Property

    'get count of attempting login with wrong password
    Private Shared Function GetWrongPasswordAttemptsFromLog(ByVal app As clsComparionCore, ByVal user As clsApplicationUser) As Integer
        Dim errorCount = 0

        If _DEF_PASSWORD_ATTEMPTS_AUTOUNLOCK Then
            Dim actionIds = $"{CInt(dbActionType.actLogon)},{CInt(dbActionType.actTokenizedURLLogon)},{CInt(dbActionType.actCredentialsLogon)},{CInt(dbActionType.actUnLock)}"
            Dim sqlCommand = $"SELECT * FROM Logs WHERE ActionID IN ({actionIds}) AND TypeID={CInt(dbObjectType.einfUser)} AND (ObjectID={user.UserID} OR COMMENT LIKE ?) AND DT>? ORDER BY DT"
            Dim sqlParams = New List(Of Object)()
            sqlParams.Add($"{user.UserEmail} %")
            sqlParams.Add(DateTime.Now.AddMinutes(-Consts._DEF_PASSWORD_ATTEMPTS_PERIOD))
            Dim tries = app.Database.SelectBySQL(sqlCommand, sqlParams)

            For Each row In tries
                If row("Result").ToString() IsNot Nothing And row("Result").ToString() <> "" Then Continue For

                If Convert.ToInt32(row("ActionID")) = CInt(dbActionType.actUnLock) Then
                    errorCount = 0
                Else
                    Dim result = row("Result").ToString().ToLower()

                    If result.StartsWith(ecAuthenticateError.aeWrongPassword.ToString().ToLower()) OrElse result.StartsWith(ecAuthenticateError.aeUserLockedByWrongPsw.ToString().ToLower()) Then

                        If result.StartsWith(ecAuthenticateError.aeUserLockedByWrongPsw.ToString().ToLower()) AndAlso errorCount < Consts._DEF_PASSWORD_ATTEMPTS Then
                            errorCount = Consts._DEF_PASSWORD_ATTEMPTS
                        Else
                            errorCount += 1
                        End If
                    Else
                        errorCount = 0
                    End If
                End If
            Next
        Else
            errorCount = user.PasswordStatus
        End If

        errorCount = If(errorCount < 0, 0, errorCount)
        errorCount += 1
        errorCount = If(errorCount > Consts._DEF_PASSWORD_ATTEMPTS, Consts._DEF_PASSWORD_ATTEMPTS, errorCount)
        Return errorCount
    End Function

    'send mail to user due to locked their account
    Private Shared Function CheckUserPasswordStatusAndSendEmail(ByVal app As clsComparionCore, ByVal user As clsApplicationUser, ByVal maxPasswordAttempts As Integer) As Boolean
        Dim result = False

        If user IsNot Nothing AndAlso user.PasswordStatus = maxPasswordAttempts Then
            Dim errorString = ""
            app.DBSaveLog(dbActionType.actLock, dbObjectType.einfUser, user.UserID, "Lock user due to max login attempts" & (If(Consts._DEF_PASSWORD_ATTEMPTS_AUTOUNLOCK, " in a period", "")), ExpertChoice.Service.Common.GetClientIP(HttpContext.Current.Request))
            Dim subject = TeamTimeClass.ParseAllTemplates(TeamTimeClass.ResString("subjLockedPswAttempts", False, False), user, Nothing)
            Dim body = TeamTimeClass.ParseAllTemplates(TeamTimeClass.ResString("bodyLockedPswAttempts", False, False), user, Nothing)
            result = Service.Common.SendMail(WebOptions.SystemEmail, user.UserEmail, subject, body, errorString, "", False, WebOptions.SMTPSSL())
            app.DBSaveLog(dbActionType.actSendEmail, dbObjectType.einfUser, user.UserID, "Email about locking account due to wrong psw", errorString)
        End If

        Return result
    End Function

    'logout function
    <WebMethod(EnableSession:=True)>
    Public Shared Function logout() As Boolean
        'Dim context = HttpContext.Current
        Dim App = CType(HttpContext.Current.Session("App"), clsComparionCore)

        If App.ActiveProject IsNot Nothing Then
            If App.ActiveProject.isTeamTime Then TeamTimeClass.TeamTimeUsersList = Nothing
        End If

        App.Logout()
        HttpContext.Current.Session.Clear()
        HttpContext.Current.Session.Abandon()
        HttpContext.Current.Response.Cookies("rmberme").Expires = DateTime.Now.AddDays(-1)
        HttpContext.Current.Response.Cookies("fullname").Expires = DateTime.Now.AddDays(-1)
        Return True
    End Function

    Public Shared ReadOnly Property CurrentStep As Integer
        Get
            Dim App = CType(HttpContext.Current.Session("App"), clsComparionCore)
            'Return App.ActiveWorkspace.get_ProjectStep(App.ActiveProject.isImpact)
            Return App.ActiveWorkspace.ProjectStep(App.ActiveProject.isImpact)
        End Get
    End Property

    <WebMethod(EnableSession:=True)>
    Public Shared Function loadHierarchy() As Object

        Dim context As HttpContext = HttpContext.Current
        Dim app = CType(context.Session("app"), clsComparionCore)
        Dim hierarchies = GeckoClass.NodeList(app.ActiveProject.HierarchyObjectives.GetLevelNodes(0), AnytimeClass.GetAction(CurrentStep))
        Dim output = New Dictionary(Of String, Object)() From
        {
            {"success", True},
            {"data", hierarchies}
        }
        Return output
    End Function

    <WebMethod(EnableSession:=True)>
    Public Shared Function loadStepList(ByVal first As Integer, ByVal last As Integer) As String
        Dim context As HttpContext = HttpContext.Current
        Dim app = CType(context.Session("app"), clsComparionCore)
        Dim steps = AnytimeClass.get_StepInformation(app, -1, first, last)
        Return steps
    End Function

    'send mail for reset password
    <WebMethod>
    Public Shared Function ResetPassword(ByVal email As String) As Object
        Dim App = CType(HttpContext.Current.Session("App"), clsComparionCore)
        Dim isSuccess = False
        Dim message = ""

        If String.IsNullOrEmpty(email) Then
            message = String.Format(TeamTimeClass.ResString("lblValidatorField"), TeamTimeClass.ResString("lblEmail"))
        Else
            Dim resetUser As clsApplicationUser = App.DBUserByEmail(email)

            If resetUser Is Nothing Then
                message = String.Format(TeamTimeClass.ResString("errUserEmailNotExists"), email)
            Else

                If resetUser.CannotBeDeleted Then
                    message = String.Format(TeamTimeClass.ResString("errResetPswNoAllowed"), email)
                Else
                    isSuccess = Service.Common.SendMail(WebOptions.SystemEmail, email, TeamTimeClass.ParseAllTemplates(TeamTimeClass.ResString("subjReminder", False, False), resetUser, Nothing), TeamTimeClass.ParseAllTemplates(App.GetPswReminderBodyText(TeamTimeClass.ResString("bodyReminder", False, False), False, False), resetUser, Nothing), message, "", False, WebOptions.SMTPSSL())
                    App.DBSaveLog(dbActionType.actSendEmail, dbObjectType.einfUser, resetUser.UserID, "Request for forgotten password", message)
                    message = String.Format(TeamTimeClass.ResString(If(isSuccess, "msgReminderOK", "msgReminderError")), email, WebOptions.SystemEmail)
                End If
            End If
        End If

        Dim returnObject = New With {
            Key .isSuccess = isSuccess,
            Key .message = message
        }
        Dim serializer = New Script.Serialization.JavaScriptSerializer()
        Return serializer.Serialize(returnObject)
    End Function

End Class