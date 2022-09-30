Imports ExpertChoice.Web
Imports ExpertChoice.Data
Imports System.Web.Script.Serialization
Imports ECCore
Imports ExpertChoice.Service
Imports AnytimeComparion.Pages.external_classes
Imports System.Web.Services

Public Class SiteMaster
    Inherits MasterPage
    Public Shared Header As Boolean = True
    Public Shared Footer As Boolean = True

    Private Const AntiXsrfTokenKey As String = "__AntiXsrfToken"
    Private Const AntiXsrfUserNameKey As String = "__AntiXsrfUserName"
    Private _antiXsrfTokenValue As String
    Public App As clsComparionCore
    Public corepage As clsComparionCorePage
    Public _roles As Integer
    'Public constants As Pages.external_classes.Constants

#Region "Page load"
    Public ReadOnly Property GoogleUA As String
        Get
            Return WebConfigOption(_OPT_GOOGLE_UID, _DEF_GOOGLE_UA, False)
        End Get
    End Property

    'Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load

    'End Sub
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        Dim App = CType(Session("App"), clsComparionCore)

        If App.ActiveUser Is Nothing Then
            Response.Redirect("~/login.aspx")
        End If

        RedirectWhenOldSessionExists(App)
        Session(Constants.Sess_FromComparion) = If(Session(Constants.Sess_FromComparion) Is Nothing, False, CBool(Session(Constants.Sess_FromComparion)))

        If Session(Constants.SessionIsPipeViewOnly) Is Nothing Then
            Session(Constants.SessionIsPipeViewOnly) = False
        End If

        If Session(Constants.SessionViewOnlyUserId) Is Nothing Then
            Session(Constants.SessionViewOnlyUserId) = -1
        End If

        If Session(Constants.SessionIsInterResultStepFound) Is Nothing Then
            Session(Constants.SessionIsInterResultStepFound) = True
        End If

        Session(Constants.Sess_SignUp_ProjName) = ""
        Dim context = HttpContext.Current
        checkQueryString()
        Session(Constants.Sess_RemoveAnonymCookie) = False
        Session("thispage") = Page
        'topHeader.Visible = Header
        'footerControl.Visible = Footer

        If App.ActiveUser IsNot Nothing Then
            CurrentUser.InnerText = App.ActiveUser.UserName
        End If

        If App IsNot Nothing Then
            If App.ActiveProject IsNot Nothing Then
                Dim curuser As ECTypes.clsUser = App.ActiveProject.ProjectManager.GetUserByEMail(App.ActiveUser.UserEmail)

                If curuser Is Nothing Then
                    curuser = App.ActiveProject.ProjectManager.AddUser(App.ActiveUser.UserEmail, True, App.ActiveUser.UserName)
                    App.ActiveProject.ProjectManager.StorageManager.Writer.SaveModelStructure()
                End If

                'UserNameLabel_mobile.InnerText = curuser.UserName
                'Project_Name.InnerText = App.ActiveProject.ProjectName
            Else
                If App.ActiveUser IsNot Nothing Then
                    UserWorkSpaceUpdateLastProjectOnly()
                End If
            End If

            If Request.QueryString("steps") IsNot Nothing Then
                If Request.QueryString("steps").Trim().ToString() <> "" Then
                    If Request.QueryString("steps").ToString() = "false" Then context.Session("steps") = Nothing
                End If
            End If

            If Request.QueryString("teamtime") IsNot Nothing Then
                If Request.QueryString("teamtime").Trim().ToString() <> "" And Request.QueryString("teamtime").ToString() = "stop" Then
                    App.TeamTimeEndSession(App.ActiveProject, False)
                    Response.Redirect("~")
                End If
            End If

            If Request.QueryString("hash") IsNot Nothing AndAlso Not Request.Path.Contains("Password.aspx") Then
                Session("UserSpecificHashErrorMessage") = Nothing
                Session("LoggedInViaHash") = True
                Session(Constants.Sess_FromComparion) = Request.QueryString("from") IsNot Nothing AndAlso Request.QueryString("from") = "comparion"
                Session(Constants.Sess_LoginMethod) = 1
                Dim evaluation_session = New String() {"TTOnly", "Pipe"}
                Dim otherparams = New String() {"req"}
                Dim showmessage = New String() {"msg"}
                Dim messagecontent = New String() {"msgcnt"}
                Dim rgid = New String() {_PARAM_ROLEGROUP}
                Dim sInputs As String = Request.QueryString("hash").Trim()

                If sInputs.Contains(" ") Then
                    Dim splitResult = sInputs.Split(" "c)
                    sInputs = splitResult(0)
                End If

                Dim sResults As String = App.DecodeTinyURL(sInputs)
                sResults = CryptService.DecodeURL(sResults, App.DatabaseID)
                Dim sParamss As NameValueCollection = HttpUtility.ParseQueryString(sResults)
                Dim email = Common.ParamByName(sParamss, _PARAMS_EMAIL)
                Dim pass = Common.ParamByName(sParamss, _PARAMS_PASSWORD)
                Dim pscode = Common.ParamByName(sParamss, _PARAMS_PASSCODE)
                Dim allowsignup = Common.ParamByName(sParamss, _PARAMS_SIGNUP)
                Dim signupmode = Common.ParamByName(sParamss, _PARAMS_SIGNUP_MODE)
                Dim req = Common.ParamByName(sParamss, otherparams)
                Dim anonymous = Common.ParamByName(sParamss, _PARAMS_ANONYMOUS_SIGNUP)
                Dim evaluation_what = Common.ParamByName(sParamss, evaluation_session)
                Dim msg = Common.ParamByName(sParamss, showmessage)
                Dim msgcnt = Common.ParamByName(sParamss, messagecontent)
                Dim rg = Common.ParamByName(sParamss, rgid)
                Dim [step] = Common.GetParam(sParamss, _PARAM_STEP)
                SetParameterStepInSession(App, [step], email)
                RedirectRestrictedUser(App, email, pscode, Common.ParamByName(sParamss, {_PARAM_WKG_ROLEGROUP}))
                Session(Constants.Sess_RoleGroup) = rg
                Session("passcode") = pscode
                'Dim passcode = pscode
                Dim check_login = App.Logon(email, pass, pscode, False, True, False)

                If String.IsNullOrEmpty(sResults) Then
                    Response.Redirect("~/?pageError=invalidLink&debug=" & sInputs)
                End If

                If Not readQueryStrings(Request.QueryString.AllKeys) Then
                    Response.Redirect("?hash=" & Request.QueryString("hash"))
                End If

                Dim project = App.DBProjectByPasscode(pscode)
                If project IsNot Nothing Then
                    LoadCustomSignUpPageContent(project)

                    If App.ActiveProject IsNot Nothing Then
                        AnytimeClass.CheckProjectIsAccessible(App.ActiveProject, email)
                    Else
                        AnytimeClass.CheckProjectIsAccessible(project, email)
                    End If

                    Dim anyTimeUrl = "~/pages/Anytime/Anytime.aspx"
                    If allowsignup <> "" Then

                        If anonymous <> "" AndAlso anonymous <> "0" Then
                            Session("NewUser") = True
                            forceSignUponAnonymous(project)
                            context.Session("User") = App.ActiveUser
                            context.Session("Project") = App.ActiveProject
                            InitName(App.ActiveUser)

                            Try
                                Response.Cookies("anonymous").Expires = DateTime.Now.AddDays(1)
                                Response.Cookies("anonymous").Value = App.ActiveUser.UserEmail
                            Catch
                                Response.Redirect("~/?pageError=inviteNoAccess&passCode=" & pscode)
                            End Try

                            Response.Redirect(anyTimeUrl)
                        End If

                        If App.ActiveUser IsNot Nothing AndAlso anonymous <> "0" Then
                            _Login.StartAnytime(project.ID)
                            Response.Redirect(anyTimeUrl)
                        Else

                            If App.ActiveUser IsNot Nothing Then
                                Session("NewUser") = True
                                storepageinfo()
                                Dim AppUser = App.DBUserByEmail(App.ActiveUser.UserEmail)
                                Dim Authres = App.Logon(AppUser.UserEmail, AppUser.UserPassword, pscode, False, True, False)
                                context.Session("User") = App.ActiveUser
                                context.Session("Project") = App.ActiveProject
                                _Login.StartAnytime(project.ID)
                                Response.Redirect(anyTimeUrl)
                            Else
                                Session("NewUser") = True

                                If Request.Cookies("anonymous") IsNot Nothing AndAlso Not String.IsNullOrEmpty(Request.Cookies("anonymous").Value) AndAlso Not CBool(Session(Constants.Sess_RemoveAnonymCookie)) Then
                                    Dim useremail = CStr(Request.Cookies("anonymous").Value)
                                    Dim AppUser = App.DBUserByEmail(useremail)
                                    Dim Authres = App.Logon(AppUser.UserEmail, AppUser.UserPassword, pscode, False, True, False)
                                    context.Session("User") = App.ActiveUser
                                    context.Session("Project") = App.ActiveProject
                                    _Login.StartAnytime(project.ID)
                                    Response.Redirect(anyTimeUrl)
                                End If

                                Response.Cookies("anonymous").Expires = DateTime.Now.AddDays(-1)
                                Response.Cookies("anonymous").Value = Nothing
                                Session(Constants.Sess_SignUp) = True
                                Session(Constants.Sess_SignUp_ProjName) = project.ProjectName
                                Session(Constants.Sess_SignUp_Passcode) = pscode
                                Session(Constants.Sess_SignUpMode) = signupmode
                                Session(Constants.Sess_Requirements) = req
                                Session(Constants.Sess_ShowMessage) = msg
                                Session(Constants.Sess_InviteMessage) = msgcnt
                            End If
                        End If
                    Else

                        Try

                            If sParamss.Count < 5 Then
                                Dim action = Request.QueryString("action")
                                Dim authres = App.Logon(email, pass, pscode, False, True, False)
                                App.ActiveProject = project
                                context.Session("User") = App.ActiveUser
                                context.Session("Project") = App.ActiveProject
                                context.Session("App") = App
                                InitName(App.ActiveUser)

                                If action = "eval_teamtime" Then
                                ElseIf action = "eval_anytime" Then
                                    HttpContext.Current.Session("Sess_WrtNode") = CType(App.ActiveProject.HierarchyObjectives.GetLevelNodes(0)(0), clsNode)
                                    Response.Redirect(anyTimeUrl)
                                End If
                            End If

                        Catch
                        End Try

                        If pscode <> "" Then
                            Dim authres = App.Logon(email, pass, pscode, False, True, False)
                            Dim message = String.Empty

                            Select Case authres
                                Case ecAuthenticateError.aeProjectLocked
                                    message = TeamTimeClass.ParseAllTemplates(App.GetMessageByAuthErrorCode(authres), App.ActiveUser, project)
                                    message = message.Replace("''", "'" & project.ProjectName & "'")
                                    Session("UserSpecificHashErrorMessage") = message
                                    Response.Redirect("~/?pageError=inviteNoAccess&passCode=" & pscode)
                                Case ecAuthenticateError.aeUserWorkgroupLocked, ecAuthenticateError.aeWorkspaceLocked, ecAuthenticateError.aeUserLockedByWrongPsw
                                    message = TeamTimeClass.ParseAllTemplates(App.GetMessageByAuthErrorCode(authres), App.ActiveUser, project)
                                    message = message.Replace("''", "'" & project.ProjectName & "'")
                                    Session("UserSpecificHashErrorMessage") = message
                                Case Else
                            End Select

                            storepageinfo()

                            If App.ActiveUser IsNot Nothing Then

                                If email = "pm" Then
                                    context.Session("UserType") = "pm"
                                End If

                                If email = "evaluator" Then
                                    context.Session("UserType") = "evaluator"
                                End If

                                If email = "participant" Then
                                    context.Session("UserType") = "participant"
                                Else
                                    context.Session("User") = App.ActiveUser
                                    context.Session("Project") = App.ActiveProject
                                End If

                                If sParamss.Count < 4 Then
                                    InitName(App.ActiveUser)
                                    _Login.StartAnytime(project.ID)
                                    Response.Redirect(anyTimeUrl)
                                End If

                                If evaluation_what = "yes" Then
                                    InitName(App.ActiveUser)
                                    _Login.StartAnytime(project.ID)
                                    Response.Redirect(anyTimeUrl)
                                End If
                            End If

                            If evaluation_what = "1" Then

                                If App.ActiveUser IsNot Nothing Then

                                    If Not _Login.isPM() Then
                                    Else
                                        Session("isMember") = True

                                        If App.ActiveProject.isTeamTime Then
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    End If
                Else
                    Dim authres = App.Logon(email, pass, pscode, False, True, False)

                    Select Case authres
                        Case ecAuthenticateError.aeProjectLocked
                            Response.Redirect("~/?pageError=inviteNoAccess&passCode=" & pscode)
                        Case ecAuthenticateError.aeUserWorkgroupLocked, ecAuthenticateError.aeWorkspaceLocked, ecAuthenticateError.aeWrongPasscode, ecAuthenticateError.aeNoUserFound
                            Dim message = TeamTimeClass.ParseAllTemplates(App.GetMessageByAuthErrorCode(authres), App.ActiveUser, project)
                            Session("UserSpecificHashErrorMessage") = message
                        Case Else
                    End Select
                End If
            Else

                If Not readQueryStrings(Request.QueryString.AllKeys) Then
                    Response.Redirect("~")
                End If
            End If

            If Request.Cookies("rmberme") IsNot Nothing AndAlso Request.Cookies("fullname") IsNot Nothing Then
                Session("UserType") = Request.Cookies("usernam").Value.ToString()
                Session("users") = Request.Cookies("fullname").Value.ToString()
            End If

            InitName(App.ActiveUser)

            If Request.Cookies("rmberme") IsNot Nothing AndAlso Request.Cookies("usernam") IsNot Nothing AndAlso Request.Cookies("passwor") IsNot Nothing AndAlso (Session("remove_anonymous") IsNot Nothing AndAlso CBool(Session("remove_anonymous"))) Then
                Dim email = Request.Cookies("usernam").Value.ToString()
                Dim password = Request.Cookies("passwor").Value.ToString()
                Dim sPasscode = ""

                If App.ActiveUser Is Nothing Then
                    Dim AuthRes = App.Logon(email, password, sPasscode, False, True, False)
                    Response.Redirect("~/pages/my-projects.aspx")
                End If

                context.Session("User") = App.ActiveUser
                context.Session("App") = App
            End If
        End If
    End Sub

    Protected Sub Page_Init(ByVal sender As Object, ByVal e As EventArgs)
        If ForceSSL(Request) AndAlso Not Request.IsSecureConnection AndAlso Request.Url IsNot Nothing AndAlso Request.Url.Host <> "localhost" AndAlso Not String.IsNullOrEmpty(Request.Url.AbsoluteUri) Then
            Response.Redirect(Request.Url.AbsoluteUri.Replace("http:", "https:"), True)
        End If

        Dim requestCookie = Request.Cookies(AntiXsrfTokenKey)
        Dim requestCookieGuidValue As Guid

        If requestCookie IsNot Nothing AndAlso Guid.TryParse(requestCookie.Value, requestCookieGuidValue) Then
            _antiXsrfTokenValue = requestCookie.Value
            Page.ViewStateUserKey = _antiXsrfTokenValue
        Else
            _antiXsrfTokenValue = Guid.NewGuid().ToString("N")
            Page.ViewStateUserKey = _antiXsrfTokenValue
            Dim responseCookie = New HttpCookie(AntiXsrfTokenKey) With {
                .HttpOnly = True,
                .Value = _antiXsrfTokenValue
            }

            If FormsAuthentication.RequireSSL AndAlso Request.IsSecureConnection Then
                responseCookie.Secure = True
            End If

            Response.Cookies.[Set](responseCookie)
        End If

        'Page.PreLoad += master_Page_PreLoad()
        'Page_PreRender()
        restoreApplicationStartTime()
    End Sub

    Private Sub restoreApplicationStartTime()
        If Request.Cookies("StartDate") IsNot Nothing Then
            Dim startDate = CType(Application("StartDate"), DateTime)
            Dim cookieStartDate = Request.Cookies("StartDate").Value

            If cookieStartDate <> startDate.ToString() Then
                Response.Cookies("loadedScreens").Expires = DateTime.Now.AddDays(-1)
                Response.Cookies("StartDate").Value = startDate.ToString()
                Response.Cookies("StartDate").Expires = DateTime.Now.AddDays(30)
            End If
        End If

        If Request.Cookies("StartDate") Is Nothing Then
            Response.Cookies("loadedScreens").Expires = DateTime.Now.AddDays(-1)
            Dim startDate = CType(Application("StartDate"), DateTime)
            Response.Cookies("StartDate").Value = startDate.ToString()
            Response.Cookies("StartDate").Expires = DateTime.Now.AddDays(30)
        End If
    End Sub

    Protected Sub Page_PreRender(ByVal sender As Object, ByVal e As EventArgs)
        If Not IsPostBack Then
            'Set Anti-XSRF token
            ViewState(AntiXsrfTokenKey) = Page.ViewStateUserKey
            ViewState(AntiXsrfUserNameKey) = If(Context.User.Identity.Name, String.Empty)
        Else
            'Validate the Anti-XSRF token
            If CStr(ViewState(AntiXsrfTokenKey)) <> _antiXsrfTokenValue OrElse CStr(ViewState(AntiXsrfUserNameKey)) <> (If(Context.User.Identity.Name, String.Empty)) Then
                Throw New InvalidOperationException("Validation of Anti-XSRF token failed.")
            End If
        End If
    End Sub

    Private Sub RedirectWhenOldSessionExists(ByVal app As clsComparionCore)
        Dim hash = If(HttpContext.Current.Request.QueryString("hash") Is Nothing, String.Empty, Request.QueryString("hash").Trim())

        If app IsNot Nothing Then
            If hash.Length > 0 AndAlso app.ActiveUser IsNot Nothing Then
                _Login.logout()
                HttpContext.Current.Response.Redirect(HttpContext.Current.Request.Url.AbsoluteUri)
            End If
        End If
    End Sub

    Private Sub checkQueryString()
        Try
            If Request.QueryString("debug") IsNot Nothing And Request.QueryString("hash") IsNot Nothing Then

                If Request.QueryString("debug").ToString() <> "" And Request.QueryString("hash").ToString() <> "" Then
                    Session("debug") = Request.QueryString("debug")
                    Session("hash") = Request.QueryString("hash")

                    If Session("hash") IsNot Nothing AndAlso Not String.IsNullOrEmpty(CStr(Session("hash"))) Then
                        Dim hashCookie As HttpCookie = New HttpCookie("LastHash", CStr(Session("hash"))) With {
                    .HttpOnly = True,
                    .Expires = DateTime.Now.AddDays(1)
                }
                        Response.Cookies.Add(hashCookie)
                    End If

                    If Session("hashinCR") Is Nothing Then Session("hashinCR") = Request.QueryString("hash")
                End If
            End If

            If Request.QueryString("LoginviaMeetingID") IsNot Nothing Then
                If Request.QueryString("LoginviaMeetingID").ToString() <> "" Then
                    Session("LoginviaMeetingID") = Request.QueryString("LoginviaMeetingID")
                End If
            End If
            Session(Constants.Sess_ForceError) = True
            If Not String.IsNullOrEmpty(Request.QueryString("forceError")) Then Session(Constants.Sess_ForceError) = Request.QueryString("forceError")
        Catch ex As Exception

        End Try
    End Sub

    Public Shared Function UserWorkSpaceUpdateLastProjectOnly() As Boolean
        Dim App = CType(HttpContext.Current.Session("App"), clsComparionCore)
        Try
            App.Database.ExecuteSQL("UPDATE userworkgroups SET LastProjectID=-1 WHERE UserID='" & App.ActiveUser.UserID & "' and WorkGroupID='" + App.ActiveUserWorkgroup.WorkgroupID & "'")
            Return True
        Catch
            Return False
        End Try
    End Function

    Private Sub SetParameterStepInSession(ByVal app As clsComparionCore, ByVal paramStep As String, ByVal email As String)
        Dim stepString = If(Request.QueryString("step") Is Nothing, paramStep, Request.QueryString("step"))
        If Not String.IsNullOrEmpty(stepString) Then
            Dim [step] As Integer

            If Integer.TryParse(stepString, [step]) Then
                Session(Constants.SessionParamStep) = [step]
            End If
        End If

        Dim mode = If(Request.QueryString("mode") Is Nothing, "", Request.QueryString("mode").Trim().ToLower())
        Dim nodeGuid = If(Request.QueryString("node") Is Nothing, "", Request.QueryString("node").Trim().ToLower())
        Dim mtType = If(Request.QueryString("mt_type") Is Nothing, "", Request.QueryString("mt_type").Trim().ToLower())
        Dim [readOnly] = If(Request.QueryString("readonly") Is Nothing, "", Request.QueryString("readonly").Trim().ToLower())
        Dim id = If(Request.QueryString("id") Is Nothing, "", Request.QueryString("id").Trim().ToLower())

        If String.IsNullOrEmpty(nodeGuid) Then
            nodeGuid = If(Request.QueryString("node_id") Is Nothing, "", Request.QueryString("node_id").Trim().ToLower())
        End If

        If (mode = "searchresults" OrElse mode = "getstep") AndAlso nodeGuid <> "" Then
            Session(Constants.SessionNonRMode) = mode
            Session(Constants.SessionNonRNode) = nodeGuid
            Dim mtTypeValue = -1
            Integer.TryParse(mtType, mtTypeValue)
            Session(Constants.SessionNonRMtType) = mtTypeValue
        End If

        If [readOnly] = "true" OrElse [readOnly] = "yes" OrElse [readOnly] = "1" Then
            Session(Constants.SessionIsPipeViewOnly) = True
        End If

        Dim user As clsApplicationUser = Nothing
        Dim userId As Integer

        If Integer.TryParse(id, userId) Then
            user = app.DBUserByID(userId)
        ElseIf Not String.IsNullOrEmpty(email) Then
            user = app.DBUserByEmail(email)
        End If

        If user IsNot Nothing Then
            Session(Constants.SessionViewOnlyUserId) = user.UserID
        End If
    End Sub

    Private Sub RedirectRestrictedUser(ByVal app As clsComparionCore, ByVal email As String, ByVal passCode As String, ByVal userRole As String)
        If Not IsUserRestricted(app, email, passCode, userRole) OrElse CBool(Session(Constants.SessionIsPipeViewOnly)) Then Return
        'Dim context = HttpContext.Current
        'Dim redirectUrl = AnytimeClass.GetComparionHashLink()
        Context.Session.Clear()
        Context.Session.Abandon()
        Context.Response.Cookies("rmberme").Expires = DateTime.Now.AddDays(-1)
        Context.Response.Cookies("fullname").Expires = DateTime.Now.AddDays(-1)
        Context.Response.Redirect(AnytimeClass.GetComparionHashLink())
    End Sub

    Private Function IsUserRestricted(ByVal app As clsComparionCore, ByVal email As String, ByVal passCode As String, ByVal userRole As String) As Boolean
        Dim roleGroup As clsRoleGroup = Nothing

        If String.IsNullOrEmpty(email) Then
            Dim workGroupRoleId = -1
            Integer.TryParse(userRole, workGroupRoleId)
            roleGroup = app.DBRoleGroupByID(workGroupRoleId, False)
        Else
            Dim sqlText = "SELECT DISTINCT G.* FROM RoleGroups G LEFT JOIN Projects P ON P.WorkgroupID = G.WorkgroupID 
                            LEFT JOIN Workspace WS ON WS.GroupID = G.ID LEFT JOIN Users U ON U.ID = WS.UserID 
                            WHERE (P.Passcode = ? OR P.Passcode2 = ?) AND (LOWER(U.Email) = ?)
                            ORDER BY G.Created DESC"
            Dim sqlParams = New List(Of Object) From {
                passCode,
                passCode,
                email
            }
            Dim groupList = app.Database.SelectBySQL(sqlText, sqlParams)

            If groupList IsNot Nothing AndAlso groupList.Count > 0 Then
                roleGroup = app.DBParse_RoleGroup(groupList.First())
            End If
        End If

        Return (roleGroup Is Nothing OrElse roleGroup.GroupType = ecRoleGroupType.gtViewer)
    End Function

    Private Function readQueryStrings(ByVal QueryStrings As String()) As Boolean
        Dim returnVal = True
        Dim App = CType(HttpContext.Current.Session("App"), clsComparionCore)

        For Each key As String In QueryStrings

            If key = "clear" Or key = "c" Then

                Select Case Request.QueryString(key)
                    Case "1"
                        Response.Cookies("anonymous").Expires = DateTime.Now.AddDays(-1)
                        Response.Cookies("anonymous").Value = ""
                        Session(Constants.Sess_RemoveAnonymCookie) = True

                        If App.ActiveUser IsNot Nothing Then
                            returnVal = False
                            _Login.logout()
                        End If

                    Case "sort"
                        Response.Cookies("ProjectListSort").Expires = DateTime.Now.AddDays(-1)
                        Response.Cookies("ProjectListSort").Value = Nothing
                    Case "rememberme"
                        Response.Cookies("rmberme").Expires = DateTime.Now.AddDays(-1)
                        Response.Cookies("rmberme").Value = Nothing
                    Case "equalMessage"
                        Response.Cookies("equalMessage").Expires = DateTime.Now.AddDays(-1)
                        Response.Cookies("equalMessage").Value = Nothing
                    Case "all"
                        Dim myCookies As String() = Request.Cookies.AllKeys

                        For Each cookie As String In myCookies
                            Response.Cookies(cookie).Expires = DateTime.Now.AddDays(-1)
                        Next

                        If App.ActiveUser IsNot Nothing Then
                            returnVal = False
                            _Login.logout()
                        End If
                End Select
            End If
        Next

        Return returnVal
    End Function

    Private Sub LoadCustomSignUpPageContent(ByVal project As clsProject)
        If project Is Nothing Then Return
        'customTitle.InnerHtml = $"Sign Up Below<br/> {project.ProjectName}"
        Dim signUpContent = ""
        Dim signUpTitle = String.Format(TeamTimeClass.ResString("msgSignUpEvaluate"), project.ProjectName)

        If project.ProjectManager.Parameters.InvitationCustomTitle <> "" Then
            signUpTitle = StringFuncs.SafeFormString(TeamTimeClass.ParseAllTemplates(project.ProjectManager.Parameters.InvitationCustomTitle, Nothing, project))
        End If

        If project.ProjectManager.Parameters.InvitationCustomText <> "" Then

            If project.ProjectManager.Parameters.InvitationCustomText.Trim().ToString() <> "" Then
                Dim htmlString = project.ProjectManager.Parameters.InvitationCustomText.Trim()

                If InfodocService.isMHT(htmlString) Then
                    htmlString = InfodocService.Infodoc_Unpack(project.ID, project.ProjectManager.ActiveHierarchy, Consts.reObjectType.ExtraInfo, _PARAM_INVITATION_CUSTOM, htmlString, True, True, -1)
                End If

                signUpContent = StringFuncs.HTML2TextWithSafeTags(htmlString, (StringFuncs._DEF_SAFE_TAGS & "IMG;TABLE;TR;TH;TD;")).Trim()

                If signUpContent <> "" Then
                    signUpContent = $"{TeamTimeClass.ParseAllTemplates(signUpContent, Nothing, project)}"
                End If
            End If
        End If

        'customTitle.InnerHtml = If(project.ProjectManager.Parameters.InvitationCustomTitle <> "", signUpTitle, $"Sign up or log in below.<br/> {project.ProjectName}")
        'customContent.InnerHtml = signUpContent
    End Sub

    Public Shared Function forceSignUponAnonymous(ByVal project As clsProject, ByVal Optional name As String = "") As ecAuthenticateError
        Dim App = CType(HttpContext.Current.Session("App"), clsComparionCore)
        Const chars As String = "abcdefghijklmnopqrstuvwxyz0123456789"
        Dim random As Random = New Random()
        Dim random_char = New String(Enumerable.Repeat(chars, 8).[Select](Function(s) s(random.[Next](s.Length))).ToArray())
        Dim email = "Anonym-" & project.Passcode & "_" & random_char
        If name = "" Then name = email
        Dim Authres = ecAuthenticateError.aeNoErrors

        If HttpContext.Current.Request.Cookies("anonymous") IsNot Nothing AndAlso Not String.IsNullOrEmpty(HttpContext.Current.Request.Cookies("anonymous").Value) Then
            email = HttpContext.Current.Request.Cookies("anonymous").Value
            Dim passcode = project.Passcode
            Authres = App.Logon(email, "", passcode, False, True, False)
            _Login.StartAnytime(project.ID)
        Else
            Dim user = App.UserWithSignup(email, name, "", "Sign-up via URL", "", False)

            If user Is Nothing Then
                forceSignUponAnonymous(project)
            Else
                Dim passcode = project.Passcode
                Authres = App.Logon(email, "", passcode, False, True, False)
                _Login.StartAnytime(project.ID)
                Global_asax.ServerError += Authres & " " + email & " " + passcode
            End If
        End If

        Return Authres
    End Function

    Public Sub InitName(ByVal user As clsApplicationUser)
        Try
            Session("users") = user.UserName
            Dim users As String = CStr(Session("users"))

            If users IsNot Nothing AndAlso users <> "" Then
            ElseIf user IsNot Nothing Then
            Else
                Dim UserType As String = CStr(Session("UserType"))

                If UserType IsNot Nothing Then

                    If UserType = "pm" Then
                    End If

                    If UserType = "evaluator" Then
                    End If

                    If UserType = "participant" Then
                    End If
                Else
                End If
            End If

            If App IsNot Nothing AndAlso App.ActiveUser IsNot Nothing Then
                If _Login.isPM() = False Then
                    _roles = 1
                Else
                    _roles = 0
                End If
            End If

        Catch
        End Try
    End Sub

    Public Shared Function storepageinfo() As Boolean
        Dim App = CType(HttpContext.Current.Session("App"), clsComparionCore)

        Try
            App.Database.ExecuteSQL("UPDATE users SET isOnline=1 WHERE ID='" & App.ActiveUser.UserID & "'")
            Return True
        Catch
            Return False
        End Try
    End Function


    <WebMethod(EnableSession:=True)>
    Public Shared Function getCurrentProjectInfo() As Object
        Dim App = CType(HttpContext.Current.Session("App"), clsComparionCore)
        Dim has_project = False
        Dim is_teamtime = False
        Dim is_online = False
        Dim project_name = ""
        Dim wkgname = ""
        Dim meetingID As Long = -1

        If App.ActiveUser IsNot Nothing Then

            If App.ActiveProject IsNot Nothing Then
                has_project = True
                project_name = App.ActiveProject.ProjectName

                If App.ActiveProject.isTeamTime Then
                    is_teamtime = True
                    is_online = App.ActiveProject.isOnline
                End If

                'meetingID = App.ActiveProject.get_MeetingID()
                meetingID = App.ActiveProject.MeetingID()
            End If

            wkgname = App.ActiveWorkgroup.Name
        End If

        'Dim accessCode = If(App.ActiveProject Is Nothing, "", App.ActiveProject.get_Passcode(App.ActiveProject.isImpact))
        Dim accessCode = If(App.ActiveProject Is Nothing, "", App.ActiveProject.Passcode(App.ActiveProject.isImpact))
        Dim output = New With {
            Key .has_project = has_project,
            Key .is_teamtime = is_teamtime,
            Key .project_id = App.ProjectID,
            Key .is_online = is_online,
            Key .project_name = project_name,
            Key .access_code = accessCode,
            Key .workgroup_name = wkgname,
            Key .meetingID = meetingID
        }

        Dim oSerializer = New System.Web.Script.Serialization.JavaScriptSerializer()
        Return oSerializer.Serialize(output)
    End Function
#End Region

End Class