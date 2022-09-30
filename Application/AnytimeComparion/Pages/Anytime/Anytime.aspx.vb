Imports System.Web.Script.Serialization
Imports System.Web.Services
Imports ExpertChoice.Results
Imports HtmlAgilityPack
Imports Pages.external_classes
Imports ExpertChoice.Service
Imports System.Runtime.InteropServices
Imports System.Globalization

Public Class Anytime
    Inherits UI.Page

    Public App As clsComparionCore
    Public _roles As Integer
    Private Const AutoAdvanceMaxJudgments As Integer = 5

    Private Const InconsistencySortingEnabled As String = "InconsistencySortingEnabled"
    Private Const InconsistencySortingOrder As String = "InconsistencySortingOrder"
    Private Const BestFit As String = "BestFit"
    Private Const AlwaysShowAutoAdvance As Boolean = True

    Private Const Sess_WrtNode As String = "Sess_WrtNode"
    Private Const Sess_Recreate As String = "Recreate_Pipe"
    Private Const SessionIsFirstTime As String = "IsFirstTime"
    Private Const SessionAutoAdvanceJudgmentsCount As String = "AutoAdvanceJudgmentsCount"

    Private Const SessionAutoAdvance As String = "AutoAdvance"
    Private Const SessionIncreaseJudgmentsCount As String = "IncreaseJudgmentsCount"
    Private Const SessionIsJudgmentAlreadySaved As String = "IsJudgmentAlreadySaved"
    Private Const SessionMultiCollapse As String = "SessionMultiCollapse"
    Private Const SessionSinglePwCollapse As String = "SessionSinglePwCollapse"
    Private Const SessionParentNodeGuid As String = "SessionParentNodeGuid"

    Private _OriginalAHPUser As clsUser = Nothing
    Public readOnlyUser As Boolean = False

    Public CanShowTitleApplyTo As Boolean = True
    Public CanShowApplyTo As Boolean = True
    Private _IntensityScale As clsMeasurementScale = Nothing

    Public Const EncryptAllCookies As Boolean = False
    Private Const CookieEncryptionPrefix As String = "@n(#"


#Region "Page load"
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim txtWelcomeResource1 As String = ""
        Session("culture") = "en-US"
        'Dim App = CType(Session("App"), clsComparionCore)
        Dim App = AppSession()

        If App IsNot Nothing AndAlso Not Request.QueryString.HasKeys Then
            If App.ActiveUser Is Nothing Then
                Response.Redirect("~/")
            End If

            Session("topHeader") = True

            If Not IsPostBack AndAlso App.HasActiveProject() Then
                Dim updatedProject = App.DBProjectByID(App.ProjectID)
                Dim projectIndex = App.ActiveProjectsList.IndexOf(App.ActiveProject)
                If projectIndex > -1 Then
                    App.ActiveProjectsList(projectIndex) = updatedProject
                Else
                    App.ActiveProjectsList.Add(updatedProject)
                End If
                Session("PWsteps") = Nothing
            End If

            HttpContext.Current.Session("showMessage") = False

            If App.ActiveProject IsNot Nothing AndAlso App.ActiveProject.isTeamTime Then
                Dim ecAppId = App.DBTeamTimeSessionAppID(App.ActiveProject.ID)

                If ecAppId = PipeParameters.ecAppliationID.appComparion Then
                    Response.Redirect(redirectToComparionTeamtime())
                Else
                End If
            End If

            'CheckForNextProjectAndRedirectIfRequired(App)
            InitializeSessions(App)

            Try
                Dim CurrentUser As clsApplicationUser = New clsApplicationUser()
                Dim mCurrentUser As clsApplicationUser = New clsApplicationUser()
                Dim isDataInstance = False
                Dim recreatePipe = False
                NotRecreatePipe = False

                If AnytimeClass.UserIsReadOnly() Then
                    CurrentUser = App.DBUserByID(AnytimeClass.GetReadOnlyUserID())
                    mCurrentUser = CurrentUser
                Else
                    CurrentUser = App.ActiveUser
                    mCurrentUser = App.ActiveUser
                    isDataInstance = True
                    recreatePipe = True

                    If NotRecreatePipe Then
                        recreatePipe = False
                    End If

                    NotRecreatePipe = True
                End If

                If App.ActiveProject.IsRisk Then
                    Dim ProjectManager = App.ActiveProject.ProjectManager
                    ProjectManager.ActiveHierarchy = CInt((If(App.ActiveProject.isImpact, ECHierarchyID.hidImpact, ECHierarchyID.hidLikelihood)))
                    If ProjectManager.ActiveHierarchy = CInt(ECHierarchyID.hidImpact) AndAlso Not Object.ReferenceEquals(ProjectManager.PipeParameters.CurrentParameterSet, ProjectManager.PipeParameters.ImpactParameterSet) Then ProjectManager.PipeParameters.CurrentParameterSet = ProjectManager.PipeParameters.ImpactParameterSet
                    If ProjectManager.ActiveHierarchy = CInt(ECHierarchyID.hidLikelihood) AndAlso Not Object.ReferenceEquals(ProjectManager.PipeParameters.CurrentParameterSet, ProjectManager.PipeParameters.DefaultParameterSet) Then ProjectManager.PipeParameters.CurrentParameterSet = ProjectManager.PipeParameters.DefaultParameterSet
                End If

                Dim AnytimeUser = App.ActiveProject.ProjectManager.GetUserByEMail(CurrentUser.UserEmail)
                _OriginalAHPUser = AnytimeUser
                AnytimeClass.SetUser(AnytimeUser, True, Not IsPostBack And Not IsCallback)

                If AnytimeUser Is Nothing Then
                    AnytimeUser = App.ActiveProject.ProjectManager.AddUser(App.ActiveUser.UserEmail, True, App.ActiveUser.UserName)
                    App.ActiveProject.ProjectManager.StorageManager.Writer.SaveModelStructure()
                End If

                Dim Project = App.ActiveProject
                Dim fHasEvals As Boolean = Not App.Options.isSingleModeEvaluation
                Dim isPM As Boolean = App.CanUserModifyProject(CurrentUser.UserID, App.ProjectID, Uw, Ws, App.ActiveWorkgroup)

                If isPM Then
                    Session("AT_isOwner") = 1
                Else
                    Session("AT_isOwner") = 0
                End If

                If fHasEvals Then
                End If

                Session("Project") = Project

                If Not Project.isOnline AndAlso Not isPM Then
                    'Response.Redirect("~/?project=" & Project.ProjectName & "&is_offline=true")
                    AnytimeClass.CheckProjectIsAccessible(Project, App.ActiveUser.UserEmail)
                End If

                Dim WorkSpace = CType(App.ActiveWorkspace, clsWorkspace)
                'Dim CurrentStep = App.ActiveWorkspace.get_ProjectStep(App.ActiveProject.isImpact)
                Dim CurrentStep = App.ActiveWorkspace.ProjectStep(App.ActiveProject.isImpact)
                If (Session("AT_CurrentStep") Is Nothing) Then
                    Session("AT_CurrentStep") = If(CurrentStep < 0, 1, CurrentStep)
                End If
                Dim paramStep = GetParameterStepFromSession(App)

                If paramStep > 0 Then
                    CurrentStep = paramStep
                    CurrentStep = If(CurrentStep < 1, 1, CurrentStep)

                    If Project.Pipe.Count < paramStep Then
                        CurrentStep = 1
                    End If

                    'WorkSpace.set_ProjectStep(App.ActiveProject.isImpact, CurrentStep)
                    CurrentStep = WorkSpace.ProjectStep(App.ActiveProject.isImpact)
                    Session("AT_CurrentStep") = CurrentStep
                    Session(SessionIsFirstTime + App.ProjectID.ToString()) = False
                End If

                App.DBWorkspaceUpdate(WorkSpace, False, Nothing)
            Catch
                Session("AT_CurrentStep") = 1
            End Try
            'Session("AT_CurrentStep") = 1

            If Request.Cookies("loadedScreens") Is Nothing Then
                Dim cookie As HttpCookie = New HttpCookie("loadedScreens")
                cookie.Values.Add("pairwise", "0")
                cookie.Values.Add("multiPairwise", "0")
                cookie.Values.Add("direct", "0")
                cookie.Values.Add("multiDirect", "0")
                cookie.Values.Add("ratings", "0")
                cookie.Values.Add("multiRatings", "0")
                cookie.Values.Add("stepFunction", "0")
                cookie.Values.Add("utility", "0")
                cookie.Values.Add("localResults", "0")
                cookie.Values.Add("globalResults", "0")
                cookie.Values.Add("survey", "0")
                cookie.Values.Add("sensitivity", "0")
                cookie.Expires = DateTime.Now.AddDays(10)
                Response.Cookies.Add(cookie)
            End If

            If App.ActiveUser Is Nothing Then
                Response.Redirect(ResolveUrl("~/"))
            End If

            Dim passcode = If(Session("passcode") IsNot Nothing, Session("passcode").ToString(), "0")
            'GetDataOfPipeStep(1)
            If passcode <> "0" Then
                If AnytimeClass.RedirectAnonAndSignupLinks(App, passcode) Then
                    'Dim redirectUrl As String = AnytimeClass.GetComparionHashLink()
                    '_OriginalAHPUser = Nothing
                    'App.Logout()
                    'HttpContext.Current.Session.Clear()
                    'HttpContext.Current.Session.Abandon()
                    'HttpContext.Current.Response.Cookies("rmberme").Expires = DateTime.Now.AddDays(-1)
                    'HttpContext.Current.Response.Cookies("fullname").Expires = DateTime.Now.AddDays(-1)


                    'Response.Redirect(AnytimeClass.GetComparionHashLink().ToString())
                End If
            End If

            If CBool(Session("LoggedInViaHash")) OrElse Session("passcode") IsNot Nothing Then
                BindUserControl(If(Convert.ToInt32(Session("AT_CurrentStep")) = -1, 1, Convert.ToInt32(Session("AT_CurrentStep"))))
            End If
        Else
            If Request.QueryString("hash") IsNot Nothing AndAlso Not Request.Path.Contains("Password.aspx") Then
                'App = New clsComparionCore()
                Session("UserSpecificHashErrorMessage") = Nothing
                Session("LoggedInViaHash") = True
                'Session(Constants.Sess_FromComparion) = Request.QueryString("from") IsNot Nothing AndAlso Request.QueryString("from") = "comparion"
                Session(Constants.Sess_FromComparion) = False
                Session(Constants.Sess_LoginMethod) = 1
                Session("thispage") = Page
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

                'Dim [readOnly] = If(Request.QueryString("readonly") Is Nothing, "", Request.QueryString("readonly").Trim().ToLower())
                'readOnlyUser = If([readOnly] = "true" OrElse [readOnly] = "yes" OrElse [readOnly] = "1", True, False)
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
                If Not AnytimeClass.UserIsReadOnly() Then
                    Dim check_login = App.Logon(email, pass, pscode, False, True, False)
                End If

                'If readOnlyUser Then
                '    Dim myObjectJson = New JavaScriptSerializer().Serialize(App)
                '    Dim Cookie = New HttpCookie("AppCookie", myObjectJson)
                '    Cookie.Expires = DateTime.Now.AddDays(1)
                '    Response.Cookies.Add(Cookie)
                'End If

                If String.IsNullOrEmpty(sResults) Then
                    Response.Redirect("~/AnytimeComparion/Default.aspx?pageError=invalidLink&debug=" & sInputs)
                End If

                'Do shortcuts and also read query strings
                If Not readQueryStrings(Request.QueryString.AllKeys) Then
                    Response.Redirect("?hash=" & Request.QueryString("hash"))
                End If

                Dim project = App.DBProjectByPasscode(pscode)
                If project IsNot Nothing Then
                    Dim customTitle As String = ""
                    LoadCustomSignUpPageContent(project, customTitle)

                    If App.ActiveProject IsNot Nothing Then
                        'AnytimeClass.CheckProjectIsAccessible(App.ActiveProject, email)
                    Else
                        AnytimeClass.CheckProjectIsAccessible(project, email)
                    End If

                    Dim anyTimeUrl = "~/AnytimeComparion/pages/Anytime/Anytime.aspx"
                    If allowsignup <> "" Then

                        'For Anonymous Link
                        If anonymous <> "" AndAlso anonymous <> "0" Then
                            Session("NewUser") = True
                            Dim AuthRes As ecAuthenticateError = SiteMaster.forceSignUponAnonymous(project)
                            Context.Session("User") = App.ActiveUser
                            Context.Session("Project") = App.ActiveProject
                            InitName(App.ActiveUser)

                            Try
                                Response.Cookies("anonymous").Expires = DateTime.Now.AddDays(1)
                                Response.Cookies("anonymous").Value = App.ActiveUser.UserEmail
                            Catch
                                Session("pageError") = AnytimeClass.GetMessageIfProjectIsOfflineOrArchived(project)
                                If AuthRes.ToString() <> "" Then
                                    Response.Redirect($"~/AnytimeComparion/Default.aspx?pageError={AuthRes}&passCode=" & pscode)
                                Else
                                    Response.Redirect("~/AnytimeComparion/Default.aspx?pageError=inviteNoAccess&passCode=" & pscode)
                                End If
                            End Try

                            Response.Redirect(anyTimeUrl)
                        End If

                        'For Passcode Link
                        If App.ActiveUser IsNot Nothing AndAlso anonymous <> "0" Then
                            Login.StartAnytime(project.ID)
                            Response.Redirect(anyTimeUrl)
                        Else
                            'For Sign Up Link
                            If App.ActiveUser IsNot Nothing Then
                                Session("NewUser") = True

                                'if user is logged in
                                SiteMaster.storepageinfo()
                                Dim AppUser = App.DBUserByEmail(App.ActiveUser.UserEmail)
                                Dim Authres = App.Logon(AppUser.UserEmail, AppUser.UserPassword, pscode, False, True, False)
                                Context.Session("User") = App.ActiveUser
                                Context.Session("Project") = App.ActiveProject
                                Login.StartAnytime(project.ID)
                                Response.Redirect(anyTimeUrl)
                            Else
                                'If User Is Not yet Then logged In
                                Session("NewUser") = True

                                If Request.Cookies("anonymous") IsNot Nothing AndAlso Not String.IsNullOrEmpty(Request.Cookies("anonymous").Value) AndAlso Not CBool(Session(Constants.Sess_RemoveAnonymCookie)) Then
                                    Dim useremail = CStr(Request.Cookies("anonymous").Value)
                                    Dim AppUser = App.DBUserByEmail(useremail)
                                    Dim Authres = App.Logon(AppUser.UserEmail, AppUser.UserPassword, pscode, False, True, False)
                                    Context.Session("User") = App.ActiveUser
                                    Context.Session("Project") = App.ActiveProject
                                    Login.StartAnytime(project.ID)
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
                                'Session("customTitle") = customTitle

                                Response.Redirect("~/AnytimeComparion/Default.aspx")
                            End If
                        End If
                    Else

                        Try

                            If sParamss.Count < 5 Then
                                Dim action = Request.QueryString("action")
                                If Not AnytimeClass.UserIsReadOnly() Then
                                    Dim authres = App.Logon(email, pass, pscode, False, True, False)
                                End If
                                App.ActiveProject = project
                                Context.Session("User") = App.ActiveUser
                                Context.Session("Project") = App.ActiveProject
                                Context.Session("App") = App
                                InitName(App.ActiveUser)

                                If action = "eval_teamtime" Then
                                ElseIf action = "eval_anytime" Then
                                    HttpContext.Current.Session("Sess_WrtNode") = If(App IsNot Nothing AndAlso App.ActiveProject IsNot Nothing, CType(App.ActiveProject.HierarchyObjectives.GetLevelNodes(0)(0), clsNode), Nothing)
                                    Response.Redirect(anyTimeUrl)
                                End If
                            End If

                        Catch
                        End Try

                        If pscode <> "" Then
                            Dim message = String.Empty

                            If Not AnytimeClass.UserIsReadOnly() Then
                                Dim authres = App.Logon(email, pass, pscode, False, True, False)
                                Select Case authres
                                    Case ecAuthenticateError.aeProjectLocked
                                        message = TeamTimeClass.ParseAllTemplates(App.GetMessageByAuthErrorCode(authres), App.ActiveUser, project)
                                        message = message.Replace("''", "'" & project.ProjectName & "'")
                                        Session("UserSpecificHashErrorMessage") = message
                                        Response.Redirect("~/AnytimeComparion/Default.aspx?pageError=inviteNoAccess&passCode=" & pscode)
                                    Case ecAuthenticateError.aeUserWorkgroupLocked, ecAuthenticateError.aeWorkspaceLocked, ecAuthenticateError.aeUserLockedByWrongPsw
                                        message = TeamTimeClass.ParseAllTemplates(App.GetMessageByAuthErrorCode(authres), App.ActiveUser, project)
                                        message = message.Replace("''", "'" & project.ProjectName & "'")
                                        Session("UserSpecificHashErrorMessage") = message
                                    Case Else
                                End Select
                            End If

                            SiteMaster.storepageinfo()

                            If App.ActiveUser IsNot Nothing Then

                                If email = "pm" Then
                                    Context.Session("UserType") = "pm"
                                End If

                                If email = "evaluator" Then
                                    Context.Session("UserType") = "evaluator"
                                End If

                                If email = "participant" Then
                                    Context.Session("UserType") = "participant"
                                Else
                                    Context.Session("User") = App.ActiveUser
                                    Context.Session("Project") = App.ActiveProject
                                End If

                                If sParamss.Count < 4 Then
                                    InitName(App.ActiveUser)
                                    If Not AnytimeClass.UserIsReadOnly() Then
                                        Login.StartAnytime(project.ID)
                                    End If
                                    Response.Redirect(anyTimeUrl)
                                End If

                                If evaluation_what = "yes" Then
                                    InitName(App.ActiveUser)
                                    If Not AnytimeClass.UserIsReadOnly() Then
                                        Login.StartAnytime(project.ID)
                                    End If
                                    Response.Redirect(anyTimeUrl)
                                End If
                            End If

                            If evaluation_what = "1" Then

                                If App.ActiveUser IsNot Nothing Then

                                    If Not Login.isPM() Then
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
                            Response.Redirect("~/AnytimeComparion/Default.aspx?pageError=inviteNoAccess&passCode=" & pscode)
                        Case ecAuthenticateError.aeUserWorkgroupLocked, ecAuthenticateError.aeWorkspaceLocked, ecAuthenticateError.aeWrongPasscode, ecAuthenticateError.aeNoUserFound
                            Dim message = TeamTimeClass.ParseAllTemplates(App.GetMessageByAuthErrorCode(authres), App.ActiveUser, project)
                            Session("UserSpecificHashErrorMessage") = message
                        Case Else
                    End Select
                End If
            ElseIf Request.QueryString("passcode") IsNot Nothing AndAlso Request.QueryString("passcode").ToString() <> "" Then
                Dim passcode As String = Request.QueryString("passcode")
                Response.Redirect(ResolveUrl("~/AnytimeComparion/Login.aspx?passcode=" + passcode))
                'ElseIf Request.QueryString("accError") IsNot Nothing AndAlso Request.QueryString("accError").ToString() <> "" Then
                '    If Session("UserSpecificHashErrorMessage") IsNot Nothing AndAlso Session("UserSpecificHashErrorMessage").ToString() <> "" Then
                Dim script As String = $"<script type='text/javascript'> alert('{Session("UserSpecificHashErrorMessage")}');</script>"
                ClientScript.RegisterClientScriptBlock(Me.GetType(), "AlertBox", script)
                '    End If
                '    Response.Redirect(ResolveUrl("~/Default.aspx"))
            Else
                Response.Redirect(ResolveUrl("~/"))
                'If Not readQueryStrings(Request.QueryString.AllKeys) Then
                '    Response.Redirect(ResolveUrl("~/"))
                'End If
            End If
            'Response.Redirect(ResolveUrl("~/"))
        End If
    End Sub

    Private Async Sub AllPairHtml(OutputModel As AnytimeOutputModel)

        Await Task.Run(Sub()
                           AllPairWiseControl.BindHtml(OutputModel)
                       End Sub)
    End Sub

    Public Async Sub BindUserControl(ByVal stepNo As Integer)
        Dim App = AppSession()
        Threading.Thread.Sleep(1000)
        'CType(Session("App"), clsComparionCore)
        Session("output") = Nothing
        If App IsNot Nothing Then
            If stepNo > 0 Then
                Dim anytimeOutputModel As AnytimeOutputModel = New AnytimeOutputModel()
                anytimeOutputModel = CType(GetDataOfPipeStep(stepNo), AnytimeOutputModel)
                If anytimeOutputModel IsNot Nothing Then
                    'make visible false all user controls then make them visible true as per page type
                    SurveyControl.Visible = False
                    InformationControl.Visible = False
                    LocalResultsControl.Visible = False
                    GlobalResultsControl.Visible = False
                    AllPairWiseControl.Visible = False
                    PairwiseControl.Visible = False
                    MultiRatingsControl.Visible = False
                    RatingsControl.Visible = False
                    MultiDirectControl.Visible = False
                    UtilityCurveControl.Visible = False
                    DirectComparisonControl.Visible = False
                    SensitivitiesAnalysisControl.Visible = False

                    'If anytimeOutputModel IsNot Nothing Then
                    '    If anytimeOutputModel.page_type = "atAllPairwise" Then
                    '        If Session("SelectionIDBack") IsNot Nothing Then
                    '            Dim pairwiseLine As clsPairwiseLine = anytimeOutputModel.multi_pw_data(CInt(Session("SelectionIDBack").ToString()))
                    '            Session("SelectionIDBack") = Nothing
                    '            anytimeOutputModel.multi_pw_data.Clear()
                    '            anytimeOutputModel.multi_pw_data.Add(pairwiseLine)
                    '        End If
                    '    End If
                    'End If
                    Dim dta = If(anytimeOutputModel IsNot Nothing, JsonConvert.SerializeObject(anytimeOutputModel), "")
                    Dim dta1 = JObject.Parse(dta)
                    hdnOutput.Value = dta1.ToString().Replace("<b>", "").Replace("</b>", "")
                    'Dim dta2 As String = HttpUtility.UrlEncode(dta1.ToString().Trim())
                    'ClientScript.RegisterClientScriptBlock(Page.GetType(), "blah", $"storeOutputVal(decodeURIComponent('{dta2}'));", True)

                    If anytimeOutputModel.returnValue Is Nothing AndAlso (anytimeOutputModel.lockedInfo Is Nothing OrElse Not anytimeOutputModel.lockedInfo.status) AndAlso anytimeOutputModel.validTimeline AndAlso anytimeOutputModel.access_avaiable Then
                        Session("topHeader") = True
                        Session("WorkgroupName") = App.ActiveWorkgroup.Name
                        Session("ProjectName") = App.ActiveProject.ProjectName
                        Dim openproject As HttpCookie = Request.Cookies("OpenProjectName")
                        If openproject Is Nothing Then
                            Dim openprojectname As New HttpCookie("OpenProjectName")
                            openprojectname.Values("OpenProjectName") = App.ActiveProject.ProjectName
                            Response.Cookies.Add(openprojectname)
                        Else
                            openproject.Values("OpenProjectName") = App.ActiveProject.ProjectName
                            Response.Cookies.Add(openproject)
                        End If

                        If anytimeOutputModel.page_type = "atSpyronSurvey" Then
                            SurveyControl.Visible = True
                            SurveyControl.bindHtml(anytimeOutputModel.PipeParameters, anytimeOutputModel.isPipeViewOnly)
                        ElseIf anytimeOutputModel.page_type = "atInformationPage" Then
                            InformationControl.Visible = True
                            InformationControl.BindHtml(anytimeOutputModel.information_message, anytimeOutputModel.non_pw_type)
                            'hdnWelcomeText.Value = anytimeOutputModel.information_message
                        ElseIf anytimeOutputModel.page_type = "atShowLocalResults" Then
                            LocalResultsControl.Visible = True
                            LocalResultsControl.bindHtml(anytimeOutputModel.PipeParameters, anytimeOutputModel.step_task, anytimeOutputModel.parentnodeID,
                                                 anytimeOutputModel.current_step, anytimeOutputModel.isPipeViewOnly, anytimeOutputModel.cluster_phrase)
                        ElseIf anytimeOutputModel.page_type = "atShowGlobalResults" Then
                            GlobalResultsControl.Visible = True
                            GlobalResultsControl.bindHtml(anytimeOutputModel.PipeParameters, anytimeOutputModel.step_task, anytimeOutputModel.current_step,
                                                  anytimeOutputModel.question, anytimeOutputModel.nameAlternatives, anytimeOutputModel.cluster_phrase, anytimeOutputModel.isPipeViewOnly)
                        ElseIf anytimeOutputModel.page_type = "atAllPairwise" Then
                            AllPairWiseControl.Visible = True
                            Session("output") = anytimeOutputModel
                            Await Task.Run(Sub() AllPairWiseControl.BindHtml(anytimeOutputModel))
                        ElseIf anytimeOutputModel.page_type = "atPairwise" Then
                            PairwiseControl.Visible = True
                            PairwiseControl.bindHtml(anytimeOutputModel)
                        ElseIf anytimeOutputModel.page_type = "atSensitivityAnalysis" Then
                            SensitivitiesAnalysisControl.Visible = True
                            SensitivitiesAnalysisControl.bindHtml(anytimeOutputModel)
                        ElseIf anytimeOutputModel.page_type = "atNonPWOneAtATime" Then
                            If anytimeOutputModel.non_pw_type = "mtRatings" Then
                                RatingsControl.Visible = True
                                RatingsControl.bindHtml(anytimeOutputModel)
                            ElseIf anytimeOutputModel.non_pw_type = "mtDirect" Then
                                DirectComparisonControl.Visible = True
                                DirectComparisonControl.bindHtml(anytimeOutputModel)
                            Else
                                UtilityCurveControl.Visible = True
                                If anytimeOutputModel.UCData IsNot Nothing Then
                                    Dim UCData As UCDataModel = anytimeOutputModel.UCData
                                    Dim hdnXMin As HiddenField = CType(UtilityCurveControl.FindControl("hdnXMin"), HiddenField)
                                    hdnXMin.Value = UCData.XMinValue.ToString()
                                    Dim hdnXMax As HiddenField = CType(UtilityCurveControl.FindControl("hdnXMax"), HiddenField)
                                    hdnXMax.Value = UCData.XMaxValue.ToString()
                                    Dim hdnDecreasing As HiddenField = CType(UtilityCurveControl.FindControl("hdnDecreasing"), HiddenField)
                                    hdnDecreasing.Value = UCData.Decreasing.ToString()
                                    Dim hdnCurvature As HiddenField = CType(UtilityCurveControl.FindControl("hdnCurvature"), HiddenField)
                                    hdnCurvature.Value = UCData.Curvature.ToString()
                                    Dim hdnXValue As HiddenField = CType(UtilityCurveControl.FindControl("hdnXValue"), HiddenField)
                                    hdnXValue.Value = UCData.XValue.ToString()
                                End If
                                UtilityCurveControl.bindHtml(anytimeOutputModel)
                            End If
                        ElseIf anytimeOutputModel.page_type = "atNonPWAllChildren" Or anytimeOutputModel.page_type = "atNonPWAllCovObjs" Then
                            If anytimeOutputModel.non_pw_type = "mtRatings" Then
                                Dim hdnactive_multi_index As HiddenField = CType(MultiRatingsControl.FindControl("hdnactive_multi_index"), HiddenField)
                                MultiRatingsControl.Visible = True
                                MultiRatingsControl.bindHtml(anytimeOutputModel, hdnactive_multi_index)
                            ElseIf anytimeOutputModel.non_pw_type = "mtDirect" Then
                                MultiDirectControl.Visible = True
                                MultiDirectControl.bindHtml(anytimeOutputModel)
                            End If
                        End If
                    Else
                        If Not CBool(HttpContext.Current.Session(Constants.SessionIsPipeViewOnly)) Then
                            footer1.Visible = False

                            If anytimeOutputModel.lockedInfo IsNot Nothing AndAlso anytimeOutputModel.lockedInfo.status Then
                                divProjectErrorMsg.Visible = True
                                divProjectError.InnerHtml = "<div class='error_msg_logo'>
                        <div class='navbar-header container'>
                            <div class='d-flex'>
                                <!-- LOGO -->
                                <div class='navbar-brand-box'>
                                    <a href='javascript:void(0);' class='logo logo-dark'>
                                        <span class='d-lg-none d-block'>
                                            <img src='../../img/fav.png' alt='' height='32'>
                                        </span>
                                        <span class='logo-lg d-none d-lg-block'>
                                            <img src='../../img/logo.png' alt='' height='60'>
                                        </span>
                                    </a>
                                </div>
                            </div>
                        </div>
                    </div><div  class='container'><div class='row'><div class='col-md-12'><h4 style='margin:1em 6em'>" + anytimeOutputModel.lockedInfo.message + "</h4></div></div></div>"
                            ElseIf Not anytimeOutputModel.validTimeline AndAlso anytimeOutputModel.TimelineMessage IsNot Nothing Then
                                divProjectErrorMsg.Visible = True
                                divProjectError.InnerHtml = anytimeOutputModel.TimelineMessage
                            ElseIf Not anytimeOutputModel.access_avaiable AndAlso anytimeOutputModel.accessDisabledMessage IsNot Nothing Then
                                divProjectErrorMsg.Visible = True
                                divProjectError.InnerHtml = anytimeOutputModel.accessDisabledMessage
                            ElseIf anytimeOutputModel.returnValue IsNot Nothing Then
                                divProjectErrorMsg.Visible = True
                                Session("pageError") = "modelClosed"
                                Response.Redirect("~/AnytimeComparion/Default.aspx?pageError=modelClosed")
                            End If
                        End If
                    End If
                    MakeVariableList()
                End If
            End If
        End If
    End Sub

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
        Else
            Session(Constants.SessionIsPipeViewOnly) = False
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
        Dim App = AppSession()
        'CType(HttpContext.Current.Session("App"), clsComparionCore)

        For Each key As String In QueryStrings

            If key = "clear" Or key = "c" Then

                Select Case Request.QueryString(key)
                    Case "1"
                        Response.Cookies("anonymous").Expires = DateTime.Now.AddDays(-1)
                        Response.Cookies("anonymous").Value = ""
                        Session(Constants.Sess_RemoveAnonymCookie) = True

                        If App.ActiveUser IsNot Nothing Then
                            returnVal = False
                            Login.logout()
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
                            Login.logout()
                        End If
                End Select
            End If
        Next

        Return returnVal
    End Function

    Private Sub LoadCustomSignUpPageContent(ByVal project As clsProject, <Out> ByRef Optional customTitle As String = "")
        If project Is Nothing Then Return
        'customTitle.InnerHtml = $"Sign Up Below<br/> {project.ProjectName}"
        customTitle = $"Sign Up Below<br/><hr/> {project.ProjectName}"
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

        customTitle = If(project.ProjectManager.Parameters.InvitationCustomTitle <> "", signUpTitle, $"Sign up or log in below.<br/><hr/> {project.ProjectName}")
        'customTitle = signUpContent
        'customTitle.InnerHtml = If(project.ProjectManager.Parameters.InvitationCustomTitle <> "", signUpTitle, $"Sign up or log in below.<br/> {project.ProjectName}")
        'customContent.InnerHtml = signUpContent
    End Sub

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
                If Login.isPM() = False Then
                    _roles = 1
                Else
                    _roles = 0
                End If
            End If

        Catch
        End Try
    End Sub

    Protected Sub Page_Unload(ByVal sender As Object, ByVal e As EventArgs)
        If _OriginalAHPUser IsNot Nothing Then
            AnytimeClass.SetUser(_OriginalAHPUser, False, False)
        End If
    End Sub

    'Redirect project if project teamtime is True
    <WebMethod(EnableSession:=True)>
    Public Shared Function redirectToComparionTeamtime() As String
        Dim app = AppSession()
        'CType(HttpContext.Current.Session("App"), clsComparionCore)
        Dim hash = GeckoClass.CreateLogonURL(app.ActiveUser, app.ActiveProject, True, "", "")
        Dim url = HttpContext.Current.Request.Url
        Dim link = If(url.Host.Equals("localhost"), $"{url.Scheme}://{url.Host}:20180/{hash}", $"{url.Scheme}://{url.Authority}/{hash}")
        Return link.Replace("//r.", "//").Replace("//r-", "//")
    End Function

    'checking for next project if user current step is last step of current project
    Private Sub CheckForNextProjectAndRedirectIfRequired(ByVal app As clsComparionCore)
        If app.ActiveProject Is Nothing OrElse app.ActiveProject.isTeamTime Then Return
        Dim context = HttpContext.Current
        'Dim currentStep = app.ActiveWorkspace.get_ProjectStep(app.ActiveProject.isImpact)
        Dim currentStep = app.ActiveWorkspace.ProjectStep(app.ActiveProject.isImpact)
        Dim isFirstTime = context.Session(SessionIsFirstTime + app.ProjectID.ToString()) Is Nothing OrElse CBool(context.Session(SessionIsFirstTime + app.ProjectID.ToString()))
        If app.ActiveProject.Pipe.Count <> currentStep OrElse isFirstTime Then Return
        Dim nextProject = AnytimeClass.GetNextProject(app.ActiveProject)

        If app.ActiveProject.ProjectManager.Parameters.EvalOpenNextProjectAtFinish AndAlso nextProject IsNot Nothing AndAlso Not nextProject.isMarkedAsDeleted Then
            Dim url = context.Request.Url
            Dim redirectUrl = If(url.Host.Equals("localhost"), $"{url.Scheme}://{url.Host}:20180/AnytimeComparion/Pages/Anytime/Anytime.aspx", $"{url.Scheme}://{url.Authority}/")
            redirectUrl = GeckoClass.CreateLogonURL(app.ActiveUser, nextProject, False, "step=1", redirectUrl, "")

            If Not String.IsNullOrEmpty(redirectUrl) Then
                context.Response.Redirect(redirectUrl)
            End If
        End If
    End Sub

    Private Shared Function CheckForNextProjectAndRedirectIfRequiredAtOutput(ByVal app As clsComparionCore) As String
        If app.ActiveProject Is Nothing OrElse app.ActiveProject.isTeamTime Then Return ""
        Dim context = HttpContext.Current
        'Dim currentStep = app.ActiveWorkspace.get_ProjectStep(app.ActiveProject.isImpact)
        Dim nextProject = AnytimeClass.GetNextProject(app.ActiveProject)

        If app.ActiveProject.ProjectManager.Parameters.EvalOpenNextProjectAtFinish AndAlso nextProject IsNot Nothing AndAlso Not nextProject.isMarkedAsDeleted Then
            Dim url = context.Request.Url
            Dim redirectUrl = If(url.Host.Equals("localhost"), $"{url.Scheme}://{url.Host}:20180/AnytimeComparion/Pages/Anytime/Anytime.aspx", $"{url.Scheme}://{url.Authority}/")
            redirectUrl = GeckoClass.CreateLogonURL(app.ActiveUser, nextProject, False, "step=1", redirectUrl, "")

            If Not String.IsNullOrEmpty(redirectUrl) Then
                '.Response.Redirect(redirectUrl)
                Return redirectUrl
            End If
        End If
    End Function

    'Initializing sessions with values
    Private Sub InitializeSessions(ByVal app As clsComparionCore)
        If app.ActiveProject IsNot Nothing Then

            If Session(SessionIsFirstTime + app.ProjectID.ToString()) Is Nothing Then
                Session(SessionIsFirstTime + app.ProjectID.ToString()) = True
            End If

            If Session(SessionAutoAdvanceJudgmentsCount + app.ProjectID.ToString()) Is Nothing Then
                Session(SessionAutoAdvanceJudgmentsCount + app.ProjectID.ToString()) = 0
            End If

            If Session(SessionAutoAdvance + app.ProjectID.ToString()) Is Nothing Then
                Session(SessionAutoAdvance + app.ProjectID.ToString()) = app.ActiveProject.PipeParameters.AllowAutoadvance
            End If

            If Session(SessionIsJudgmentAlreadySaved) Is Nothing Then
                Session(SessionIsJudgmentAlreadySaved) = False
            End If
        End If
    End Sub

    Public Shared Property ExpectedValueString As String()
        Get
            Dim context As HttpContext = HttpContext.Current
            Dim expectedValue = CType(context.Session(Constants.Sess_ExpectedValue), String())
            Return expectedValue
        End Get
        Set(ByVal value As String())
            Dim context As HttpContext = HttpContext.Current
            context.Session(Constants.Sess_ExpectedValue) = value
        End Set
    End Property

    'get step number from session
    Private Function GetParameterStepFromSession(ByVal app As clsComparionCore) As Integer
        Dim [step] = If(Session(Constants.SessionParamStep) Is Nothing, 0, CInt(Session(Constants.SessionParamStep)))
        Dim mode = If(Session(Constants.SessionNonRMode) Is Nothing, "", CStr(Session(Constants.SessionNonRMode)))
        Dim nodeGuid = If(Session(Constants.SessionNonRNode) Is Nothing, "", CStr(Session(Constants.SessionNonRNode)))
        Dim mtType = If(Session(Constants.SessionNonRMtType) Is Nothing, -1, CInt(Session(Constants.SessionNonRMtType)))

        If mode = "searchresults" AndAlso nodeGuid <> "" Then
            Dim index = 0
            Dim hasStep = False

            For Each action In app.ActiveProject.Pipe
                index += 1

                If action.ActionType = ActionType.atShowLocalResults Then
                    Dim actionData = CType(action.ActionData, clsShowLocalResultsActionData)

                    If actionData.ParentNode.NodeGuidID.ToString() = nodeGuid Then
                        [step] = index
                        hasStep = True
                        Exit For
                    End If
                End If

                If action.ActionType = ActionType.atShowGlobalResults Then

                    If Not hasStep Then
                        [step] = index
                    End If

                    Dim actionData = CType(action.ActionData, clsShowGlobalResultsActionData)

                    If actionData.WRTNode.NodeGuidID.ToString() = nodeGuid Then
                        hasStep = True
                        Exit For
                    End If
                End If
            Next

            If Not hasStep Then
                [step] = 0
                Session(Constants.SessionIsInterResultStepFound) = False
            End If
        ElseIf mode = "getstep" AndAlso nodeGuid <> "" AndAlso (mtType = 0 OrElse mtType = 1) Then
            Dim nGuid = Guid.Empty

            If Guid.TryParse(nodeGuid, nGuid) Then
                Dim node = app.ActiveProject.HierarchyObjectives.GetNodeByID(nGuid)
                node = If(node Is Nothing, app.ActiveProject.HierarchyAlternatives.GetNodeByID(nGuid), node)
                [step] = app.ActiveProject.ProjectManager.PipeBuilder.GetFirstEvalPipeStepForNode(node, -1)
                [step] = If(node Is Nothing, 0, [step] + 1)
            End If
        End If

        Return [step]
    End Function

#End Region

    Public Shared Function AppSession() As clsComparionCore
        Dim app = CType(HttpContext.Current.Session("App"), clsComparionCore)
        If app IsNot Nothing Then
            Dim PWsteps = If(HttpContext.Current.Session("PWsteps") IsNot Nothing, CType(HttpContext.Current.Session("PWsteps"), List(Of clsAction)), Nothing)
            If PWsteps IsNot Nothing AndAlso Not app.ActiveProject.Pipe.Count() = PWsteps.Count() Then
                app.ActiveProject.Pipe.Clear()
                For Each stp In PWsteps
                    app.ActiveProject.Pipe.Add(stp)
                Next
            End If
        End If
        Return app
    End Function
    Public Shared Function GetDataOfPipeStep(ByVal stepId As Integer) As Object

        'Dim message = ""
        'Dim Context As System.Web.HttpContext.Current

        'stepId = 2

        Dim App As clsComparionCore = AppSession()
        'CType(HttpContext.Current.Session("App"), clsComparionCore)
        Dim Params As New Dictionary(Of String, String)
        If App.ActiveProject Is Nothing Then
            Dim returnOutput As AnytimeOutputModel = New AnytimeOutputModel()
            returnOutput.returnValue = New ReturnValMessage()
            returnOutput.returnValue.message = GeckoClass.timeOutMessage
            returnOutput.returnValue.status = "timeout"
            'Dim oSerializer = New JavaScriptSerializer()
            'Dim obj As Object = oSerializer.Serialize(returnValue)
            Return returnOutput
        End If
        If CheckProject() Then
            Return False
        End If

        ' HttpContext.Current.Session("AT_CurrentStep") = 2

        Dim CurrentStep = CInt(HttpContext.Current.Session("AT_CurrentStep"))
        CurrentStep = If(CurrentStep <= 0, 1, CurrentStep)
        'Dim stepId As Integer = 1
        If CurrentStep <> stepId Then
            HttpContext.Current.Session(SessionIsJudgmentAlreadySaved) = False
        End If

        If CurrentStep > App.ActiveProject.Pipe.Count OrElse stepId > App.ActiveProject.Pipe.Count Then
            CurrentStep = 1
            stepId = CurrentStep
        End If

        If Not CBool(HttpContext.Current.Session(Constants.SessionIsPipeViewOnly)) Then
            For i = 1 To stepId
                Dim result = App.ActiveProject.Pipe(i - 1)
                If result.ActionType = ActionType.atSpyronSurvey Then
                    Dim PipeObj = New Object()
                    PipeObj = AnytimeClass.CreateSurvey(CInt(i))
                    If PipeObj IsNot Nothing Then
                        Dim html As StringBuilder = New StringBuilder()
                        Dim dta = JsonConvert.SerializeObject(PipeObj)
                        Dim dta1 = JObject.Parse(dta)

                        For j = 0 To dta1("SurveyPage")("Questions").Count - 1
                            Dim SurveyAnswers = dta1("SurveyAnswers")(j)
                            If SurveyAnswers IsNot Nothing Then
                                If SurveyAnswers(2).ToString().ToLower() = "false" Then
                                    If SurveyAnswers(0) IsNot Nothing AndAlso SurveyAnswers(0).ToString().Trim() = "" Then
                                        CurrentStep = i
                                        stepId = i
                                        Exit For
                                    End If
                                    If dta1("SurveyPage")("Questions")(j)("Type").ToString() = "12" Then
                                        If SurveyAnswers(0) IsNot Nothing AndAlso Not CBool(SurveyAnswers(0).ToString().Contains("True")) Then
                                            CurrentStep = i
                                            stepId = i
                                            Exit For
                                        End If
                                    End If
                                    If dta1("SurveyPage")("Questions")(j)("Type").ToString() = "13" Then
                                        Dim count As Integer = 0
                                        For k = 1 To dta1("alternativelist").Count - 1
                                            If CBool(dta1("alternativelist")(k)("isDisabled")) Then
                                                count = count + 1
                                            End If
                                        Next
                                        If count = 0 Then
                                            CurrentStep = i
                                            stepId = i
                                            Exit For
                                        End If
                                    End If
                                End If
                            End If
                        Next
                    End If
                End If
            Next
        End If

        Dim parent_node_info, question, StepTask, InformationMessage As String
        Dim PairwiseType = ""
        Dim first_node_info = ""
        Dim second_node_info = ""
        parent_node_info = ""
        Dim wrt_first_node_info = ""
        Dim wrt_second_node_info = ""
        question = ""
        Dim wording = ""
        StepTask = ""
        InformationMessage = ""
        Dim FirstNodeName, SecondNodeName, ParentNodeName, ChildNodeName As String
        ChildNodeName = ""
        FirstNodeName = ""
        SecondNodeName = ""
        ParentNodeName = ""
        Dim PWValue As Double = -1
        Dim PWAdvantage As Double = -1
        Dim CurrentValue As Double = -1
        Dim is_auto_advance As Boolean = False
        Dim IsUndefined = False
        Dim PipeParameters = New Object()

        Dim ParentNodeID As Integer = -1
        Dim MultiPW_Data = New List(Of clsPairwiseLine)()
        Dim MultiNonPW_Data = New List(Of clsRatingLine)()
        Dim step_intervals = New List(Of String())()
        Dim NonPWType = ""
        Dim precision As Integer = 0
        Dim intensities = New List(Of String())()
        Dim multi_intensities = New List(Of String())()
        Dim NonPWValue = ""
        Dim is_direct = False
        Dim showPriorityAndDirectValue As Boolean = True

        Dim judgment_made As Integer = 0
        Dim overall As Double = 0.00
        Dim total_evaluation As Integer = 0
        Dim piecewise As Boolean = False
        Dim Comment As String = ""
        Dim show_comments As Boolean = False

        Dim ParentNodeGUID As Guid = New Guid()
        Dim LeftNodeGUID As Guid = New Guid()
        Dim RightNodeGUID As Guid = New Guid()
        Dim LeftNodeWrtGUID As Guid = New Guid()
        Dim RightNodeWrtGUID As Guid = New Guid()

        Dim infodoc_params As String() = New String(4) {}
        Dim done_redirect_url As String = ""
        Dim logout_at_end = False
        Dim is_infodoc_tooltip As Boolean = False
        Dim multi_GUIDs = New List(Of String())()
        Dim multi_infodoc_params As List(Of String()) = New List(Of String())()


        Dim qh_info = ""
        Dim qh_yt_info = ""
        Dim qh_help_id = New ecEvaluationStepType()
        Dim qh_tnode_id = 0
        Dim saType = String.Empty
        Dim StepNode As clsNode = Nothing
        Dim show_qh_automatically As Boolean = False
        Dim isPM As Boolean = False

        'Dim HasQHCluster As Boolean = False
        'Dim QHApplySteps As New List(Of Integer)
        'Dim clsPipeBuilder As clsPipeBuilder = New clsPipeBuilder()
        'Dim ApplyNodes As List(Of clsNode) = clsPipeBuilder.GetPipeStepClusters(stepId, HasQHCluster, QHApplySteps)

        Dim scaleDescriptions As List(Of ScaleDescriptions) = New List(Of ScaleDescriptions)()
        Dim isFirstTime = CBool(HttpContext.Current.Session(SessionIsFirstTime + "" + Convert.ToString(App.ProjectID)))
        HttpContext.Current.Session(SessionIsFirstTime + "" + Convert.ToString(App.ProjectID)) = False


        Dim NodesData = New List(Of String())()
        Dim firstUnassessedStep = GetFirstUnassessed(App)
        Dim pipeHelpUrl As String = ""
        Dim is_qh_shown = False
        Dim multi_collapse_default = New List(Of Boolean)()
        Dim dont_show_qh = If(getQHSettingCookies() = "False", False, True)
        Dim show_qh_setting = Not dont_show_qh
        'Dim output As Object = Nothing
        Dim output As AnytimeOutputModel = New AnytimeOutputModel()
        Dim UCData As UCDataModel = Nothing

        If Not App Is Nothing Then
            Dim PWsteps = If(HttpContext.Current.Session("PWsteps") IsNot Nothing, CType(HttpContext.Current.Session("PWsteps"), List(Of clsAction)), Nothing)
            If PWsteps IsNot Nothing AndAlso Not App.ActiveProject.Pipe.Count() = PWsteps.Count() Then
                App.ActiveProject.Pipe.Clear()
                For Each stp In PWsteps
                    App.ActiveProject.Pipe.Add(stp)
                Next

                CurrentStep = CurrentStep + 1
                stepId = stepId + 1
                'Dim stepId As Integer = 1
                If CurrentStep <> stepId Then
                    HttpContext.Current.Session(SessionIsJudgmentAlreadySaved) = False
                End If

                If CurrentStep > App.ActiveProject.Pipe.Count OrElse stepId > App.ActiveProject.Pipe.Count Then
                    CurrentStep = 1
                    stepId = CurrentStep
                End If
            End If

            Dim LastJudgmentTime = DateTime.Now
            judgment_made = App.ActiveProject.ProjectManager.GetMadeJudgmentCount(App.ActiveProject.ProjectManager.ActiveHierarchy, App.ActiveProject.ProjectManager.UserID, DateTime.Now)

            If judgment_made = 0 AndAlso isFirstTime Then
                stepId = 1
            End If

            Dim multiCollapseStatus = If(HttpContext.Current.Session(SessionMultiCollapse) Is Nothing, New Dictionary(Of String, List(Of Boolean))(), CType(HttpContext.Current.Session(SessionMultiCollapse), Dictionary(Of String, List(Of Boolean))))
            Dim exists As Boolean = multiCollapseStatus.ContainsKey(App.ProjectID & "_" + Convert.ToString(stepId))

            If Not exists Then
                multi_collapse_default.Add(False)
                multi_collapse_default.Add(False)
                multi_collapse_default.Add(False)
                multi_collapse_default.Add(False)
                multi_collapse_default.Add(False)
            Else
                multi_collapse_default = multiCollapseStatus(App.ProjectID.ToString() & "_" + stepId.ToString())
            End If

            Dim user_id = App.ActiveUser.UserID

            If AnytimeClass.UserIsReadOnly() Then
                user_id = AnytimeClass.GetReadOnlyUserID()
            End If

            isPM = App.CanUserModifyProject(user_id, App.ProjectID, Uw, Ws, App.ActiveWorkgroup)
            'Dim nodes = App.ActiveProject.HierarchyObjectives.Nodes
            ' stepId = 2
            'For Each Node As clsNode In App.ActiveProject.HierarchyObjectives.Nodes
            '    Dim ActionType = AnytimeClass.Action(stepId).ActionType
            '    Dim node_type = Node.FeedbackMeasureType(False).ToString().Replace("mt", "")
            '    'Dim node_type As ECMeasureType = Node.get_MeasureType(False).ToString().Replace("mt", "")
            '    Dim action_type = AnytimeClass.Action(stepId).ActionType.ToString()

            '    If action_type.Contains(Node.FeedbackMeasureType(False).ToString().Replace("mt", "")) Then
            '        Dim temp_node As String() = {"", ""}
            '        temp_node(0) = Node.NodeID.ToString()
            '        temp_node(1) = Node.NodeName
            '        NodesData.Add(temp_node)
            '    End If
            'Next

            For i = 0 To App.ActiveProject.HierarchyObjectives.Nodes.Count - 1
                Dim Node As clsNode = App.ActiveProject.HierarchyObjectives.Nodes(i)

                Dim ActionType = AnytimeClass.Action(stepId).ActionType
                Dim node_type = Node.FeedbackMeasureType(False).ToString().Replace("mt", "")
                'Dim node_type As ECMeasureType = Node.get_MeasureType(False).ToString().Replace("mt", "")
                Dim action_type = AnytimeClass.Action(stepId).ActionType.ToString()

                If action_type.Contains(Node.FeedbackMeasureType(False).ToString().Replace("mt", "")) Then
                    Dim temp_node As String() = {"", ""}
                    temp_node(0) = Node.NodeID.ToString()
                    temp_node(1) = Node.NodeName
                    NodesData.Add(temp_node)
                End If
            Next


            'Dim doneOptions = New Dictionary(Of String, Object)() From {
            '    {"logout", App.ActiveProject.PipeParameters.LogOffAtTheEnd},
            '    {"redirect", App.ActiveProject.PipeParameters.RedirectAtTheEnd},
            '    {"url", App.ActiveProject.PipeParameters.TerminalRedirectURL},
            '    {"closeTab", App.ActiveProject.ProjectManager.Parameters.EvalCloseWindowAtFinish},
            '    {"openProject", App.ActiveProject.ProjectManager.Parameters.EvalOpenNextProjectAtFinish},
            '    {"stayAtEval", Not App.ActiveProject.PipeParameters.RedirectAtTheEnd AndAlso Not App.ActiveProject.ProjectManager.Parameters.EvalCloseWindowAtFinish AndAlso Not App.ActiveProject.ProjectManager.Parameters.EvalOpenNextProjectAtFinish}
            '}
            Dim doneOptions As doneOptionsModel = New doneOptionsModel()
            doneOptions.logout = App.ActiveProject.PipeParameters.LogOffAtTheEnd.ToString()
            doneOptions.redirect = App.ActiveProject.PipeParameters.RedirectAtTheEnd.ToString()
            doneOptions.url = App.ActiveProject.PipeParameters.TerminalRedirectURL.ToString()
            doneOptions.closeTab = App.ActiveProject.ProjectManager.Parameters.EvalCloseWindowAtFinish.ToString()
            doneOptions.openProject = App.ActiveProject.ProjectManager.Parameters.EvalOpenNextProjectAtFinish.ToString()
            doneOptions.nextProjectURL = If(doneOptions.openProject = "True", CheckForNextProjectAndRedirectIfRequiredAtOutput(App), "")
            'doneOptions.nextProjectURL = CheckForNextProjectAndRedirectIfRequiredAtOutput(App)
            doneOptions.stayAtEval = CType(Not App.ActiveProject.PipeParameters.RedirectAtTheEnd AndAlso Not App.ActiveProject.ProjectManager.Parameters.EvalCloseWindowAtFinish AndAlso Not App.ActiveProject.ProjectManager.Parameters.EvalOpenNextProjectAtFinish, String)

            Dim pipeOptions As pipeOptionsModel = New pipeOptionsModel()
            pipeOptions.hideNavigation = Not App.ActiveProject.PipeParameters.ShowProgressIndicator
            pipeOptions.disableNavigation = Not App.ActiveProject.PipeParameters.AllowNavigation
            pipeOptions.showUnassessed = App.ActiveProject.PipeParameters.ShowNextUnassessed
            pipeOptions.dontAllowMissingJudgment = Not App.ActiveProject.PipeParameters.AllowMissingJudgments
            If HttpContext.Current.Session(SessionAutoAdvance + "" + Convert.ToString(App.ProjectID)) Is Nothing Then
                is_auto_advance = App.ActiveProject.PipeParameters.AllowAutoadvance
                'If (Not App.ActiveProject.PipeParameters.AllowAutoadvance OrElse AlwaysShowAutoAdvance) AndAlso App.ActiveProject.PipeParameters.AllowNavigation Then
                '    is_auto_advance = True
                'End If
                'If (App.ActiveProject.PipeParameters.AllowNavigation) Then
                '    is_auto_advance = True
                'Else
                '    is_auto_advance = App.ActiveProject.PipeParameters.AllowAutoadvance
                'End If
                HttpContext.Current.Session(SessionAutoAdvance + "" + Convert.ToString(App.ProjectID)) = is_auto_advance
            Else
                is_auto_advance = Convert.ToBoolean(HttpContext.Current.Session(SessionAutoAdvance + "" + Convert.ToString(App.ProjectID)))
            End If

            If HttpContext.Current.Session("InfodocMode") Is Nothing Then
                If App.ActiveProject.PipeParameters.ShowInfoDocsMode = CanvasTypes.ShowInfoDocsMode.sidmPopup Then
                    is_infodoc_tooltip = True
                    HttpContext.Current.Session("InfodocMode") = "1"
                End If
            Else
                If HttpContext.Current.Session("InfodocMode").ToString() = "1" Then
                    is_infodoc_tooltip = True
                End If
            End If
            show_comments = App.ActiveProject.PipeParameters.ShowComments
            Dim Project = App.ActiveProject
            Dim user_email = App.ActiveUser.UserEmail
            Dim user_name = App.ActiveUser.UserName
            Dim isDataInstance = False
            Dim recreatePipe = False

            If AnytimeClass.UserIsReadOnly() Then
                Dim CurrentUser = App.DBUserByID(AnytimeClass.GetReadOnlyUserID())
                Dim mCurrentUser = CurrentUser
                user_email = CurrentUser.UserEmail
                user_name = CurrentUser.UserName
            Else
                isDataInstance = True
                recreatePipe = True

                If NotRecreatePipe Then
                    isDataInstance = False
                    recreatePipe = False
                End If

                NotRecreatePipe = True
            End If
            Dim AnytimeUser = CType(App.ActiveProject.ProjectManager.GetUserByEMail(user_email), clsUser)
            Dim DTs As DateTime = DateTime.Now
            Dim totalObjective = App.ActiveProject.HierarchyObjectives.Nodes.Count
            Dim totalAlternative = App.ActiveProject.HierarchyAlternatives.Nodes.Count
            total_evaluation = App.ActiveProject.ProjectManager.GetTotalJudgmentCount(Project.ProjectManager.ActiveHierarchy, App.ActiveProject.ProjectManager.UserID)

            Dim overallcalc As Double = (Convert.ToDouble(judgment_made) / Convert.ToDouble(total_evaluation) * 100)
            overall = If(Double.IsNaN(overallcalc), 0, overallcalc)
            AnytimeClass.SetUser(AnytimeUser, isDataInstance, recreatePipe)

            Dim PreviousStep = App.ActiveWorkspace.ProjectStep(App.ActiveProject.isImpact)
            App.ActiveWorkspace.ProjectStep(App.ActiveProject.isImpact) = stepId
            Dim workspace = App.ActiveWorkspace
            App.DBWorkspaceUpdate(workspace, False, Nothing)
            CurrentStep = stepId
            HttpContext.Current.Session("AT_CurrentStep") = CurrentStep
            If HttpContext.Current.Session("AT_PreviousStep") Is Nothing Or CurrentStep <> PreviousStep Then
                HttpContext.Current.Session("AT_PreviousStep") = PreviousStep
            Else
                PreviousStep = Convert.ToInt32(HttpContext.Current.Session("AT_PreviousStep"))
            End If
            If stepId < 1 Then
                stepId = 0
            End If
            If (stepId > App.ActiveProject.Pipe.Count) Then
                stepId = App.ActiveProject.Pipe.Count
            End If
            Dim AnytimeAction = AnytimeClass.GetAction(stepId)
            'Dim path = Context.Server.MapPath("~/")
            'Consts._FILE_MHT_FILES = System.IO.Path.GetFullPath(System.IO.Path.Combine(path, "DocMedia/MHTFiles/"))
            'Consts._FILE_ROOT = Context.Server.MapPath("~/")
            Select Case AnytimeAction.ActionType
                'Welcome and Thank You  
                Case ActionType.atInformationPage
                    InformationUserCtrl.GetInformationPageData(StepNode, AnytimeAction, App, CurrentStep, pipeHelpUrl, qh_help_id, InformationMessage, NonPWType, AnytimeClass.ShowUnassessed)

                'Case ActionType.atPairwise
                '    ParentNodeID = CInt(PairwiseUserCtrl.GetPairWiseData(AnytimeAction, App, StepNode, Comment, PairwiseType, pipeHelpUrl, qh_help_id, PipeWarning, infodoc_params, question, wording, parent_node_info, first_node_info,
                '   second_node_info, wrt_first_node_info, wrt_second_node_info, FirstNodeName, SecondNodeName, ParentNodeName, PWValue.ToString(), PWAdvantage.ToString(), IsUndefined.ToString(), ParentNodeGUID, LeftNodeGUID, RightNodeGUID, ParentNodeID))
                Case ActionType.atAllPairwise
                    AllPairWiseUserCtrl.GetAllPairWiseData(AnytimeAction, App, StepNode, multi_GUIDs, PairwiseType, MultiPW_Data, ParentNodeName, ParentNodeGUID, StepTask, parent_node_info, pipeHelpUrl, ParentNodeID, infodoc_params, qh_help_id)
                Case ActionType.atAllPairwiseOutcomes
                    AllPairWiseUserCtrl.GetAllPairWiseData(AnytimeAction, App, StepNode, multi_GUIDs, PairwiseType, MultiPW_Data, ParentNodeName, ParentNodeGUID, StepTask, parent_node_info, pipeHelpUrl, ParentNodeID, infodoc_params, qh_help_id)

                    '5.atNonPWOneAtATime'
                'Case ActionType.atNonPWOneAtATime
                '    NonPairWiseOneAtATimeUserCtrl.GetNonPairWiseOneAtATimeData(AnytimeAction, App, StepNode)

                    '7.atNonPWAllCovObjs'
                'Case ActionType.atNonPWAllCovObjs
                '    NonPairWiseAllCovObjsUserCtrl.GetNonPairWiseAllCovObjData(AnytimeAction, App, StepNode, IsUndefined)
'                    ParentNodeID = PairwiseUserCtrl.GetPairWiseData(AnytimeAction, App, StepNode, Comment, PairwiseType, pipeHelpUrl, qh_help_id, PipeWarning, infodoc_params, question, wording, parent_node_info, first_node_info,
'second_node_info, wrt_first_node_info, wrt_second_node_info, FirstNodeName, SecondNodeName, ParentNodeName, PWValue, PWAdvantage, IsUndefined, ParentNodeGUID, LeftNodeGUID, RightNodeGUID, ParentNodeID)

                Case ActionType.atAllPairwise
                Case ActionType.atAllPairwiseOutcomes
                   ' AllPairWiseUserCtrl.GetAllPairWiseData(AnytimeAction, App, StepNode, multi_GUIDs, PairwiseType, MultiPW_Data, ParentNodeName, ParentNodeGUID, parent_node_info, pipeHelpUrl, ParentNodeID, infodoc_params)


                    'Dim debug_test = infodoc_params
                    'ParentNodeID = PwData.ParentNodeID

                    '8. atShowLocalResults Local Results
                Case ActionType.atShowLocalResults
                    Dim model As LocalResultModel = New LocalResultModel()
                    model = LocalResults.GetDetails(AnytimeAction)
                    StepNode = model.StepNode
                    StepTask = model.StepTask
                    ParentNodeID = model.ParentNodeID
                    ParentNodeGUID = model.ParentNodeGUID
                    PipeParameters = model.PipeParameters
                    question = App.ActiveProject.PipeParameters.NameObjectives
                    'End Local Results

                    '9. atShowGlobalResults Global Results
                Case ActionType.atShowGlobalResults
                    Dim model As GlobalResultModel = New GlobalResultModel()
                    model = GlobalResults.GetDetails(stepId, CurrentStep, App)
                    question = model.question
                    qh_help_id = model.qh_help_id
                    StepTask = model.StepTask
                    PipeParameters = model.PipeParameters
                    ParentNodeID = model.ParentNodeID
                    'End Global Results

                    '10. atSpyronSurvey
                Case ActionType.atSpyronSurvey
                    qh_help_id = ecEvaluationStepType.Survey
                    PipeParameters = AnytimeClass.CreateSurvey(CInt(CurrentStep))

                    '11. atSensitivityAnalysis
                Case ActionType.atSensitivityAnalysis
                    Dim model As SensitivityAnalysisModel = New SensitivityAnalysisModel()
                    model = SensitivitiesAnalysis.GetDetails(AnytimeAction.ActionData, App)
                    StepTask = model.StepTask
                    saType = model.saType
                    qh_help_id = model.qh_help_id

                    'atPairwise
                Case ActionType.atPairwise
                    Dim model As PairwiseModel = New PairwiseModel()
                    model = Pairwise.GetDetails(AnytimeAction, App)
                    StepNode = model.StepNode
                    Comment = model.comment
                    PairwiseType = model.PairwiseType
                    pipeHelpUrl = model.pipeHelpUrl
                    qh_help_id = model.qh_help_id
                    question = model.question
                    wording = model.wording
                    PipeWarning = model.PipeWarning
                    StepTask = model.StepTask
                    first_node_info = model.first_node_info.Replace("<b>", "").Replace("</b>", "")
                    second_node_info = model.second_node_info.Replace("<b>", "").Replace("</b>", "")
                    parent_node_info = model.parent_node_info.Replace("<b>", "").Replace("</b>", "")
                    wrt_first_node_info = model.wrt_first_node_info.Replace("<b>", "").Replace("</b>", "")
                    wrt_second_node_info = model.wrt_second_node_info.Replace("<b>", "").Replace("</b>", "")
                    FirstNodeName = model.FirstNodeName
                    SecondNodeName = model.SecondNodeName
                    ParentNodeName = model.ParentNodeName
                    PWValue = model.PWValue
                    PWAdvantage = model.PWAdvantage
                    IsUndefined = model.IsUndefined
                    ParentNodeGUID = model.ParentNodeGUID
                    LeftNodeGUID = model.LeftNodeGUID
                    RightNodeGUID = model.RightNodeGUID
                    infodoc_params = model.infodoc_params
                    ParentNodeID = model.ParentNodeID

                Case ActionType.atNonPWOneAtATime
                    Dim model As UtilityCurveModel = New UtilityCurveModel()
                    model = ctUtilityCurve.GetDetails(AnytimeAction, App)
                    pipeHelpUrl = model.pipeHelpUrl
                    qh_help_id = model.qh_help_id
                    NonPWType = model.NonPWType
                    NonPWValue = model.NonPWValue
                    is_direct = model.is_direct
                    Comment = model.comment
                    StepNode = model.StepNode
                    StepTask = model.StepTask
                    ParentNodeName = model.ParentNodeName
                    ChildNodeName = model.ChildNodeName
                    ParentNodeGUID = model.ParentNodeGUID
                    LeftNodeGUID = model.LeftNodeGUID
                    infodoc_params = model.infodoc_params
                    showPriorityAndDirectValue = model.showPriorityAndDirectValue
                    precision = model.precision
                    scaleDescriptions = model.scaleDescriptions
                    intensities = model.intensities
                    first_node_info = model.first_node_info
                    parent_node_info = model.parent_node_info
                    wrt_first_node_info = model.wrt_first_node_info
                    IsUndefined = model.IsUndefined
                    ParentNodeID = model.ParentNodeID
                    step_intervals = model.step_intervals
                    CurrentValue = model.CurrentValue
                    piecewise = model.piecewise
                    PipeParameters = model.PipeParameters
                    UCData = model.UCData
                Case ActionType.atNonPWAllChildren, ActionType.atNonPWAllCovObjs
                    MultiRatings.GetDetails(AnytimeAction, App, pipeHelpUrl, qh_help_id, NonPWType, StepNode, showPriorityAndDirectValue, precision, first_node_info,
wrt_first_node_info, IsUndefined, scaleDescriptions, StepTask, MultiNonPW_Data, ParentNodeName, parent_node_info, ParentNodeID, ParentNodeGUID, infodoc_params)

            End Select

            If HttpContext.Current.Session(InconsistencySortingEnabled) Is Nothing Then
                HttpContext.Current.Session(InconsistencySortingEnabled) = False
                HttpContext.Current.Session(BestFit) = False
            End If

            If StepNode IsNot Nothing Then
                qh_tnode_id = If(StepNode.IsAlternative, -StepNode.NodeID, StepNode.NodeID)
            Else
            End If
            Dim AutoShow As Boolean = show_qh_automatically
            Dim isCluster As Boolean = False
            qh_info = App.ActiveProject.ProjectManager.PipeParameters.PipeMessages.GetEvaluationQuickHelpText(App.ActiveProject.ProjectManager, stepId, isCluster, AutoShow)
            show_qh_automatically = AutoShow
            qh_info = Regex.Replace(qh_info, "<title>.*?</title>", "")
            'Dim qh_info_plain_text = qh_info
            If Not qh_info.Contains("img") Then
                qh_info = Regex.Replace(qh_info, "<.*?>", "").Trim()
            End If
            If qh_info <> "" Then
            Else
                If isPM Then
                    App.ActiveProject.ProjectManager.PipeParameters.PipeMessages.SetEvaluationQuickHelpText(App.ActiveProject.ProjectManager, stepId, False, False, "")
                End If
            End If
            Dim ObjID As String = GetQuickHelpObjectID(qh_help_id, StepNode)
            AnytimeClass.SetQuickHelpObjectIdInSession(ObjID)
            qh_info = Infodoc_Unpack(Project.ID, Project.ProjectManager.ActiveHierarchy, Consts.reObjectType.QuickHelp, ObjID, qh_info, True, True, -1)

            qh_yt_info = ParseVideoLinks(qh_info, False)
            Dim defaultQhInfo As String = GetDefaultQhInfo(App, qh_help_id)

            If HttpContext.Current.Session(qh_yt_info) IsNot Nothing Then
                is_qh_shown = True
            End If

            Dim steps = ""
            If PreviousStep <> stepId AndAlso Not (PreviousStep < 1 OrElse PreviousStep > App.ActiveProject.Pipe.Count) Then steps = AnytimeClass.get_StepInformation(App, PreviousStep)

            'Dim extremeMessage = Context.Request.Cookies(Constants.Cook_Extreme) IsNot Nothing
            'Dim showAutoAdvanceModal1 As Boolean = showAutoAdvanceModal(App, AnytimeClass.Action(stepId), "", "")
            'Dim userControlContent = If(AnytimeAction.ActionType <> ActionType.atInformationPage, getUserControl(stepId), "")
            Dim HasCurStepCluster As Boolean = False

            Dim QHApplySteps = New List(Of Integer)()
            Dim ApplyNodes = New List(Of clsNode)()
            ApplyNodes = App.ActiveProject.ProjectManager.PipeBuilder.GetPipeStepClusters(stepId, HasCurStepCluster, QHApplySteps)
            'Dim apply_to = New List(Of String)()
            'Dim qqq = ApplyNodes
            Dim cluster_nodes = New List(Of String())()
            Dim n As Integer = 1

            Dim fCanShow As Boolean = QHApplySteps.Count > 1 OrElse HasCurStepCluster
            If fCanShow AndAlso AnytimeAction IsNot Nothing Then
                'TeamTimeClass.GetPipeStepTask(AnytimeClass.Action(stepId), Nothing, False, False, False)
                Dim sTooltip As String = ""
                Dim isMulti As Boolean = False
                Dim tAGuid As Guid = Guid.Empty
                Select Case AnytimeAction.ActionType
                    Case ActionType.atAllPairwise, ActionType.atAllPairwiseOutcomes, ActionType.atNonPWAllChildren, ActionType.atNonPWAllCovObjs, ActionType.atShowLocalResults
                        isMulti = True
                End Select

                For Each tNode As clsNode In ApplyNodes
                    Dim CurStepNode As clsNode = App.ActiveProject.ProjectManager.PipeBuilder.GetPipeActionNode(AnytimeAction)
                    Dim fActive As Boolean = (tNode Is CurStepNode) OrElse (stepId = QHApplySteps(n - 1))
                    Dim fChecked As Boolean = fActive
                    If Not fChecked AndAlso TeamTimeClass.ClusterPhrase <> "" Then
                        tAGuid = Guid.Empty
                        Dim sStepClusterPhrase As String = ""
                        sStepClusterPhrase = App.ActiveProject.ProjectManager.PipeBuilder.GetClusterTitleForResults(tNode.NodeGuidID, tAGuid)
                        If sStepClusterPhrase = GeckoClass.GetPipeStepHint(AnytimeClass.Action(stepId), Nothing, True, True) Then fChecked = True
                    End If
                    sTooltip += String.Format("<li><nobr><input class='titleNodesCheckbox' type='checkbox' id='{5}{1}'{2}{4} value='{3}'><label for='task_guid_{1}'>{0}</label></nobr></li>", SafeFormString(ShortString(tNode.NodeName, 40, True)), n, IIf(fActive, " disabled", ""), tNode.NodeGuidID.ToString, IIf(fChecked, " checked", ""), "title_guid_")
                    n += 1
                Next
                sTooltip = String.Format("<div style='padding:3ex'><b>{0}</b><ul type=square style='margin:1ex 2em 1ex 1em;{1}'>{2}</ul><div style='text-align:center; margin-top:1em;'><input type='button' class='button button_small' id='btnApplyTo' value='{3}' onclick='saveHeaderContent(); return false;' style='width:15em'></div></div>", TeamTimeClass.ResString("lblEditTaskChooseSteps"), IIf(ApplyNodes.Count > 10, " height:10em; overflow-y:scroll;", ""), sTooltip, TeamTimeClass.ResString("btnEditTaskSaveMulti"), 1)
                output.title_nodes = sTooltip

                n = 1
                sTooltip = ""
                isMulti = False
                tAGuid = Guid.Empty
                Select Case AnytimeAction.ActionType
                    Case ActionType.atAllPairwise, ActionType.atAllPairwiseOutcomes, ActionType.atNonPWAllChildren, ActionType.atNonPWAllCovObjs, ActionType.atShowLocalResults
                        isMulti = True
                End Select

                For Each tNode As clsNode In ApplyNodes
                    Dim CurStepNode As clsNode = App.ActiveProject.ProjectManager.PipeBuilder.GetPipeActionNode(AnytimeAction)
                    Dim fActive As Boolean = (tNode Is CurStepNode) OrElse (stepId = QHApplySteps(n - 1))
                    Dim fChecked As Boolean = fActive
                    If Not fChecked AndAlso TeamTimeClass.ClusterPhrase <> "" Then
                        tAGuid = Guid.Empty
                        Select Case AnytimeAction.ActionType
                            Case ActionType.atShowLocalResults, ActionType.atShowGlobalResults
                                ' D4329 ===
                                tAGuid = tNode.NodeGuidID
                        End Select
                        Dim sStepClusterPhrase As String = ""
                        sStepClusterPhrase = App.ActiveProject.ProjectManager.PipeBuilder.GetClusterPhraseForNode(tNode, isMulti, tAGuid, False)
                        If sStepClusterPhrase = TeamTimeClass.ClusterPhrase Then fChecked = True
                    End If
                    sTooltip += String.Format("<li><nobr><input class='headerNodesCheckbox' type='checkbox' id='{5}{1}'{2}{4} value='{3}'><label for='task_guid_{1}'>{0}</label></nobr></li>", SafeFormString(ShortString(tNode.NodeName, 40, True)), n, IIf(fActive, " disabled", ""), tNode.NodeGuidID.ToString, IIf(fChecked, " checked", ""), "task_guid_")
                    n += 1
                Next
                sTooltip = String.Format("<div style='padding:3ex'><b>{0}</b><ul type=square style='margin:1ex 2em 1ex 1em;{1}'>{2}</ul><div style='text-align:center; margin-top:1em;'><input type='button' class='button button_small' id='btnApplyTo' value='{3}' onclick='saveHeaderContent(); return false;' style='width:15em'></div></div>", TeamTimeClass.ResString("lblEditTaskChooseSteps"), IIf(ApplyNodes.Count > 10, " height:10em; overflow-y:scroll;", ""), sTooltip, TeamTimeClass.ResString("btnEditTaskSaveMulti"), 0)
                output.header_nodes = sTooltip
            End If

            n = 0
            If ApplyNodes.Count > 0 Then
                Dim tCurNode As clsNode = App.ActiveProject.ProjectManager.PipeBuilder.GetPipeActionNode(App.ActiveProject.ProjectManager.Pipe(stepId - 1))
                Dim QuickHelpContent = App.ActiveProject.ProjectManager.PipeParameters.PipeMessages.GetEvaluationQuickHelpText(App.ActiveProject.ProjectManager, stepId, isCluster, AutoShow)
                For Each node As clsNode In ApplyNodes
                    Dim isMulti As Boolean = False
                    Dim tAGuid As Guid = Guid.Empty
                    Dim clusterChecked As Boolean = False
                    Select Case AnytimeAction.ActionType
                        Case ActionType.atAllPairwise, ActionType.atAllPairwiseOutcomes, ActionType.atNonPWAllChildren, ActionType.atNonPWAllCovObjs, ActionType.atShowLocalResults
                            isMulti = True
                    End Select
                    Dim sStepClusterPhrase As String = App.ActiveProject.ProjectManager.PipeBuilder.GetClusterPhraseForNode(node, isMulti, tAGuid, False)
                    If (HttpContext.Current.Session("ClusterPhrase") IsNot Nothing AndAlso sStepClusterPhrase = HttpContext.Current.Session("ClusterPhrase").ToString()) OrElse sStepClusterPhrase = "" Then clusterChecked = True

                    Dim tStep As Integer = QHApplySteps(n)
                    Dim tParentNode As clsNode = ApplyNodes(n)
                    Dim fActive = (stepId = tStep) OrElse (tParentNode Is tCurNode)  ' D4113
                    Dim sName As String = SafeFormString(ShortString(tParentNode.NodeName, 40, True))
                    If fActive Then sName = String.Format("<b>{0}</b>", sName)
                    Dim fChecked As Boolean = fActive
                    Dim AutoShow1 As Boolean = False ' D4082
                    Dim sContent As String = App.ActiveProject.PipeParameters.PipeMessages.GetEvaluationQuickHelpText(App.ActiveProject.ProjectManager, tStep, True, AutoShow1)    ' D3741 + D4079 + D4082
                    If sContent <> "" AndAlso QuickHelpContent = sContent Then fChecked = True '

                    Dim arr As String() = {node.NodeName.ToString(), QHApplySteps(n).ToString(), node.NodeID.ToString(), node.ParentNodeID.ToString(), fChecked.ToString(), clusterChecked.ToString(), node.NodeGuidID.ToString()}
                    cluster_nodes.Add(arr)
                    n += 1
                Next
            End If

            Dim baseUrl As String = HttpContext.Current.Request.Url.Scheme & "://" + HttpContext.Current.Request.Url.Authority + HttpContext.Current.Request.ApplicationPath.TrimEnd("/"c) & "/"
            Dim hashLink = baseUrl & GeckoClass.CreateLogonURL(App.ActiveUser, App.ActiveProject, False, "", "")
            If HttpContext.Current.Request.Cookies("HideWarningMessage") Is Nothing Then
                Dim warningCookie = New HttpCookie("HideWarningMessage", "1") With {
                .HttpOnly = True,
                .Expires = DateTime.Now.AddDays(1)
            }
                HttpContext.Current.Request.Cookies.Add(warningCookie)
            End If

            Dim nextProject = AnytimeClass.GetNextProject(App.ActiveProject)
            Dim nextProjectId = If(nextProject Is Nothing, -1, nextProject.ID)
            Dim hideWarning As Boolean = HttpContext.Current.Request.Cookies("HideWarningMessage").Value = "1"

            If App.ActiveProject.ProjectManager.Parameters.AutoFitInfoDocImages Then
                parent_node_info = RemoveInfoDcoImageStyle(parent_node_info)
            End If
            HttpContext.Current.Session(SessionParentNodeGuid) = ParentNodeGUID

            output.hashLink = hashLink
            output.status = "active"
            output.pipeOptions = pipeOptions
            output.showinfodocnode = App.ActiveProject.PipeParameters.ShowInfoDocs
            output.current_step = stepId
            output.previous_step = PreviousStep
            output.total_pipe_steps = App.ActiveProject.Pipe.Count
            output.is_first_time = isFirstTime
            output.show_auto_advance_modal = showAutoAdvanceModal(App, AnytimeClass.Action(stepId), "", "")
            output.first_unassessed_step = GetFirstUnassessed(App)
            output.help_pipe_root = TeamTimeClass.ResString("help_pipe_root")
            output.help_pipe_url = pipeHelpUrl
            output.page_type = AnytimeAction.ActionType.ToString()
            output.pairwise_type = PairwiseType
            output.first_node = FirstNodeName
            output.second_node = SecondNodeName
            output.parent_node = ParentNodeName
            output.first_node_info = first_node_info
            output.second_node_info = second_node_info
            output.child_node = ChildNodeName
            output.parent_node_info = If(isHTMLEmpty(parent_node_info), "", parent_node_info)
            output.wrt_first_node_info = wrt_first_node_info
            output.wrt_second_node_info = wrt_second_node_info
            output.ScaleDescriptions = scaleDescriptions
            output.question = question
            output.wording = wording
            output.nameAlternatives = App.ActiveProject.PipeParameters.NameAlternatives
            output.information_message = InformationMessage
            output.step_task = StepTask
            output.value = PWValue
            output.advantage = PWAdvantage
            output.IsUndefined = IsUndefined
            output.sRes = If(HttpContext.Current.Session("sRes") Is Nothing, "", App.ResString(CStr(HttpContext.Current.Session("sRes"))))
            output.current_value = -1
            output.comment = Comment
            output.show_comments = show_comments
            output.ShowUnassessed = AnytimeClass.ShowUnassessed
            output.nextUnassessedStep = GetNextUnassessed(stepId)
            output.steps = steps
            output.stepButtons = GeckoClass.loadStepButtons(CurrentStep)
            'output.stepButtons = output.stepButtons.ToString().Substring(0, output.stepButtons.ToString().Length)
            output.usersComments = ""
            output.currentUserEmail = App.ActiveUser.UserEmail
            output.extremeMessage = HttpContext.Current.Request.Cookies(Constants.Cook_Extreme) IsNot Nothing
            output.pipeWarning = PipeWarning
            If HttpContext.Current.Session(Sess_WrtNode) IsNot Nothing Then
                output.sess_wrt_node_id = (CType(HttpContext.Current.Session(Sess_WrtNode), clsNode)).NodeID
            End If
            output.parentnodeID = ParentNodeID
            output.orderbypriority = CBool(HttpContext.Current.Session(InconsistencySortingEnabled))
            output.bestfit = CBool(HttpContext.Current.Session(BestFit))
            output.dont_show = CBool(HttpContext.Current.Session("showMessage"))
            output.multi_GUIDs = multi_GUIDs
            output.multi_pw_data = MultiPW_Data
            output.multi_infodoc_params = multi_infodoc_params
            output.non_pw_type = NonPWType
            output.precision = 0
            output.showPriorityAndDirect = showPriorityAndDirectValue
            output.intensities = intensities
            output.non_pw_value = NonPWValue
            output.is_direct = is_direct
            output.multi_non_pw_data = MultiNonPW_Data
            output.multi_intensities = multi_intensities
            output.step_intervals = step_intervals
            output.piecewise = piecewise
            output.judgment_made = judgment_made
            output.overall = overall
            output.total_evaluation = total_evaluation
            output.ParentNodeGUID = ParentNodeGUID
            output.LeftNodeGUID = LeftNodeGUID
            output.RightNodeGUID = RightNodeGUID
            output.LeftNodeWrtGUID = LeftNodeWrtGUID
            output.RightNodeWrtGUID = RightNodeWrtGUID
            output.infodoc_params = infodoc_params
            output.is_auto_advance = is_auto_advance
            output.PipeParameters = PipeParameters
            output.doneOptions = doneOptions
            output.is_infodoc_tooltip = is_infodoc_tooltip
            output.defaultQhInfo = defaultQhInfo 'GetDefaultQhInfo(App, qh_help_id)
            output.qh_info = qh_info
            output.qh_help_id = qh_help_id
            output.qh_tnode_id = qh_tnode_id
            output.qh_yt_info = qh_yt_info
            output.saType = saType
            output.show_qh_automatically = show_qh_automatically
            output.is_qh_shown = is_qh_shown
            output.dont_show_qh = If(getQHSettingCookies() = "False", False, True)
            output.show_qh_setting = If(getQHSettingCookies() = "False", True, False)
            output.multi_collapse_default = multi_collapse_default
            If HttpContext.Current.Session("ClusterPhrase") IsNot Nothing Then
                output.cluster_phrase = HttpContext.Current.Session("ClusterPhrase").ToString()
            End If
            output.nodes_data = NodesData
            output.UCData = UCData
            output.read_only = AnytimeClass.UserIsReadOnly()
            output.read_only_user = user_email
            output.read_only_username = user_name
            output.collapse_bars = App.ActiveProject.ProjectManager.Parameters.EvalCollapseMultiPWBars
            output.userControlContent = CType(If(AnytimeAction.ActionType <> ActionType.atInformationPage, getUserControl(stepId), ""), String)
            output.isPM = App.CanUserModifyProject(user_id, App.ProjectID, Uw, Ws, App.ActiveWorkgroup)
            output.cluster_nodes = cluster_nodes
            output.has_cluster = HasCurStepCluster
            'output.has_cluster = True
            output.name = App.ActiveProject.ProjectName
            output.owner = App.ActiveProject.ProjectManager.User.UserName
            output.passcode = App.ActiveProject.Passcode
            output.hideBrowserWarning = hideWarning
            output.autoFitInfoDocImages = App.ActiveProject.ProjectManager.Parameters.AutoFitInfoDocImages
            output.autoFitInfoDocImagesOptionText = TeamTimeClass.ResString("optImagesZoom")
            output.framed_info_docs = App.ActiveProject.ProjectManager.Parameters.ShowFramedInfodocsMobile
            output.hideInfoDocCaptions = App.ActiveProject.ProjectManager.Parameters.EvalHideInfodocCaptions
            output.fromComparion = CBool(HttpContext.Current.Session(Constants.Sess_FromComparion))
            output.nextProject = nextProjectId
            output.isPipeViewOnly = CBool(HttpContext.Current.Session(Constants.SessionIsPipeViewOnly))
            output.isPipeStepFound = CBool(HttpContext.Current.Session(Constants.SessionIsInterResultStepFound))
            output.currentProjectInfo = getCurrentProjectInfo()
            output.lockedInfo = If(Not output.isPipeViewOnly, CheckProjectLockStatus(), Nothing)
            Dim validTimeline As Boolean = True
            Dim access_avaiable As Boolean = True
            If Not isPM Then
                If App.ActiveProject.PipeParameters.StartDate.HasValue Then
                    If App.ActiveProject.PipeParameters.StartDate.Value > Now.Date Then
                        validTimeline = False
                        output.TimelineMessage = TeamTimeClass.ResString("msgEvaluationNotStarted")
                        output.TimelineMessage = String.Format(output.TimelineMessage, 5)
                    End If
                End If
                If App.ActiveProject.PipeParameters.EndDate.HasValue Then
                    If App.ActiveProject.PipeParameters.EndDate.Value < Now.Date Then
                        validTimeline = False
                        output.TimelineMessage = TeamTimeClass.ResString("msgEvaluationIsCompleted")
                    End If
                End If
                If Not App.ActiveProject.isPublic And HttpContext.Current.Session("NewUser") IsNot Nothing AndAlso HttpContext.Current.Session("NewUser").ToString() = "True" Then
                    'Dim chknewuser = HttpContext.Current.Session("NewUser").ToString()
                    access_avaiable = False
                    output.accessDisabledMessage = TeamTimeClass.ResString("msgDisabledPasscode")
                End If
            End If
            output.validTimeline = validTimeline
            output.access_avaiable = access_avaiable
            output.initSA = If(AnytimeAction.ActionType.ToString() = "atSensitivityAnalysis", initializeSensitivity(), Nothing)
        End If

        Try
            Return output
        Catch e As Exception
            Return Nothing
        End Try

    End Function

    Private Shared Function RemoveInfoDcoImageStyle(ByVal infoDocHtml As String) As String
        Dim newInfoDocHtml As String = infoDocHtml
        Try
            Dim infoDoc As HtmlDocument = New HtmlDocument()
            infoDoc.LoadHtml(infoDocHtml)
            Dim imageNodes As List(Of HtmlNode) = infoDoc.DocumentNode.SelectNodes("//img").ToList()

            If imageNodes.Count > 0 Then

                For Each imageNode As HtmlNode In imageNodes
                    If imageNode.ParentNode.Name.ToLower() = "font" Then Continue For

                    For Each itemAttribute As HtmlAttribute In imageNode.Attributes.ToList()

                        If itemAttribute.Name.ToLower().Equals("style") OrElse itemAttribute.Name.ToLower().Equals("width") OrElse itemAttribute.Name.ToLower().Equals("height") Then
                            imageNode.Attributes(itemAttribute.Name).Remove()
                        End If
                    Next

                    Dim parentNode As HtmlNode = imageNode.Ancestors().FirstOrDefault()

                    If Not parentNode.Name.Equals("p", StringComparison.CurrentCultureIgnoreCase) Then
                        Dim doc As HtmlDocument = New HtmlDocument()
                        Dim newElement As HtmlNode = doc.CreateElement("p")
                        newElement.AppendChild(imageNode)
                        parentNode.ReplaceChild(newElement, imageNode)
                        parentNode = newElement
                    End If

                    For Each parentAttribute As HtmlAttribute In parentNode.Attributes.ToList()

                        If parentAttribute.Name.ToLower().Equals("style") OrElse parentAttribute.Name.ToLower().Equals("class") Then
                            parentNode.Attributes(parentAttribute.Name).Remove()
                        End If
                    Next

                    parentNode.Attributes.Add("class", "text-center")
                Next

                newInfoDocHtml = infoDoc.DocumentNode.OuterHtml
            End If

        Catch e As Exception
        End Try

        Return newInfoDocHtml
    End Function

    <WebMethod(EnableSession:=True)>
    Public Shared Function getUserControl(ByVal [step] As Integer) As Object
        Dim context As HttpContext = HttpContext.Current
        Dim action = AnytimeClass.GetAction([step])
        Dim userControlUrl = ""
        Dim cookie = If(context.Request.Cookies("loadedScreens"), New HttpCookie("loadedScreens"))
        Dim isLocal As Boolean = HttpContext.Current.Request.IsLocal
        cookie.Values.Add("pairwise", "0")
        cookie.Values.Add("multiPairwise", "0")
        cookie.Values.Add("direct", "0")
        cookie.Values.Add("multiDirect", "0")
        cookie.Values.Add("ratings", "0")
        cookie.Values.Add("multiRatings", "0")
        cookie.Values.Add("stepFunction", "0")
        cookie.Values.Add("utility", "0")
        cookie.Values.Add("localResults", "0")
        cookie.Values.Add("globalResults", "0")
        cookie.Values.Add("survey", "0")
        cookie.Values.Add("sensitivity", "0")
        context.Response.AppendCookie(cookie)

        Select Case action.ActionType
            Case ActionType.atInformationPage
                If cookie.Values("pairwise") = "1" Then Return False
                cookie.Values("pairwise") = "1"
                userControlUrl = "~/AnytimeComparion/pages/anytime/Pairwise.ascx"
            Case ActionType.atPairwise, ActionType.atPairwiseOutcomes
                If cookie.Values("pairwise") = "1" Then Return False
                cookie.Values("pairwise") = "1"
                userControlUrl = "~/AnytimeComparion/pages/anytime/Pairwise.ascx"
            Case ActionType.atAllPairwise, ActionType.atAllPairwiseOutcomes
                If cookie.Values("multiPairwise") = "1" Then Return False
                cookie.Values("multiPairwise") = "1"
                userControlUrl = "~/AnytimeComparion/Pages/Anytime/AllPairWiseUserCtrl.ascx"
            Case ActionType.atNonPWOneAtATime
                Dim measuretype = CType(action.ActionData, clsNonPairwiseEvaluationActionData)

                Select Case (CType(action.ActionData, clsNonPairwiseEvaluationActionData)).MeasurementType
                    Case ECMeasureType.mtDirect
                        If cookie.Values("direct") = "1" Then Return False
                        cookie.Values("direct") = "1"
                        userControlUrl = "~/AnytimeComparion/Pages/Anytime/DirectComparison.ascx"
                    Case ECMeasureType.mtRatings
                        If cookie.Values("ratings") = "1" Then Return False
                        cookie.Values("ratings") = "1"
                        userControlUrl = "~/AnytimeComparion/Pages/Anytime/Ratings.ascx"
                    Case ECMeasureType.mtStep
                        If cookie.Values("stepFunction") = "1" Then Return False
                        cookie.Values("stepFunction") = "1"
                        userControlUrl = "~/AnytimeComparion/Pages/Anytime/StepFunction.ascx"
                    Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtCustomUtilityCurve
                        If cookie.Values("utility") = "1" Then Return False
                        cookie.Values("utility") = "1"
                        userControlUrl = "~/AnytimeComparion/Pages/Anytime/ctUtilityCurve.ascx"
                End Select

            Case ActionType.atNonPWAllChildren, ActionType.atNonPWAllCovObjs
                Dim measuretype = CType(action.ActionData, clsNonPairwiseEvaluationActionData)

                Select Case (CType(action.ActionData, clsNonPairwiseEvaluationActionData)).MeasurementType
                    Case ECMeasureType.mtDirect
                        If cookie.Values("multiDirect") = "1" Then Return False
                        cookie.Values("multiDirect") = "1"
                        userControlUrl = "~/AnytimeComparion/Pages/Anytime/MultiDirect.ascx"
                    Case ECMeasureType.mtRatings
                        If cookie.Values("multiRatings") = "1" Then Return False
                        cookie.Values("multiRatings") = "1"
                        userControlUrl = "~/AnytimeComparion/Pages/Anytime/MultiRatings.ascx"
                End Select

            Case ActionType.atSpyronSurvey, ActionType.atSurvey
                If context.Request.Cookies("loadedScreens")("survey") = "1" Then Return False
                context.Response.Cookies("loadedScreens")("survey") = "1"
                userControlUrl = "~/AnytimeComparion/pages/anytime/Survey.ascx"
            Case ActionType.atSensitivityAnalysis
                If context.Request.Cookies("loadedScreens")("sensitivity") = "1" Then Return False
                context.Response.Cookies("loadedScreens")("sensitivity") = "1"
                userControlUrl = "~/AnytimeComparion/pages/anytime/SensitivitiesAnalysis.ascx"
            Case ActionType.atShowLocalResults
                If cookie.Values("localResults") = "1" Then Return False
                cookie.Values("localResults") = "1"
                userControlUrl = "~/AnytimeComparion/pages/anytime/localresults.ascx"
            Case ActionType.atShowGlobalResults
                If cookie.Values("globalResults") = "1" Then Return False
                cookie.Values("globalResults") = "1"
                userControlUrl = "~/AnytimeComparion/Pages/Anytime/GlobalResults.ascx"
        End Select

        context.Response.AppendCookie(cookie)

        'Using objPage As Page = New Page()
        '    Dim uControl As UserControl = CType(objPage.LoadControl(userControlUrl), UserControl)
        '    objPage.Controls.Add(uControl)
        '    Using sWriter As StringWriter = New StringWriter()
        '        context.Server.Execute(objPage, sWriter, False)
        '        Return sWriter.ToString()
        '    End Using
        'End Using
        Return ""
    End Function

    Private Shared Function IsEvaluation(ByVal Action As clsAction) As Boolean
        Return Action IsNot Nothing AndAlso Action.isEvaluation
    End Function

    Private Shared Sub IncreaseAutoAdvanceJudgmentsCount(ByVal app As clsComparionCore, ByVal action As clsAction)
        Dim judgmentsCount As Integer = CInt(HttpContext.Current.Session(SessionAutoAdvanceJudgmentsCount + app.ProjectID.ToString()))
        Dim isAutoAdvance As Boolean = CBool(HttpContext.Current.Session(SessionAutoAdvance + app.ProjectID.ToString()))

        If IsEvaluation(action) AndAlso Not isAutoAdvance AndAlso judgmentsCount >= 0 AndAlso (action.ActionType = ActionType.atPairwise OrElse action.ActionType = ActionType.atNonPWOneAtATime) Then
            HttpContext.Current.Session(SessionAutoAdvanceJudgmentsCount + app.ProjectID.ToString()) = judgmentsCount + 1
        End If

        HttpContext.Current.Session(SessionIncreaseJudgmentsCount) = False
    End Sub

    Public Shared Function showAutoAdvanceModal(ByVal app As clsComparionCore, ByVal action As clsAction, ByVal pairwiseType As String, ByVal nonPwType As String) As Boolean
        Dim judgmentsCount As Integer = CInt(HttpContext.Current.Session(SessionAutoAdvanceJudgmentsCount + app.ProjectID.ToString()))
        Dim isAutoAdvance As Boolean = CBool(HttpContext.Current.Session(SessionAutoAdvance + app.ProjectID.ToString()))
        Dim showAutoAdvance As Boolean = IsEvaluation(action) AndAlso Not isAutoAdvance AndAlso AutoAdvanceMaxJudgments = judgmentsCount AndAlso ((action.ActionType = ActionType.atPairwise AndAlso pairwiseType <> "ptGraphical") OrElse (action.ActionType = ActionType.atNonPWOneAtATime AndAlso nonPwType <> "mtDirect")) AndAlso Not CBool(HttpContext.Current.Session(SessionIsJudgmentAlreadySaved)) AndAlso Not app.ActiveProject.ProjectManager.Parameters.EvalNoAskAutoAdvance

        If (HttpContext.Current.Session(SessionIncreaseJudgmentsCount) IsNot Nothing AndAlso CBool(HttpContext.Current.Session(SessionIncreaseJudgmentsCount))) OrElse showAutoAdvance Then
            IncreaseAutoAdvanceJudgmentsCount(app, action)
        End If

        Return showAutoAdvance
    End Function

    Private Shared Function GetDefaultQhInfo(ByVal app As clsComparionCore, ByVal qhHelpId As ecEvaluationStepType) As String
        Dim defaultQhInfo = File_GetContent(GeckoClass.GetIncFile(String.Format(_FILE_TEMPL_QUCIK_HELP, qhHelpId.ToString() & If(app.isRiskEnabled, If(app.ActiveProject.isImpact, "_Impact", "_Likelihood"), ""))), "")

        If String.IsNullOrEmpty(defaultQhInfo) AndAlso app.isRiskEnabled Then
            defaultQhInfo = File_GetContent(GeckoClass.GetIncFile(String.Format(_FILE_TEMPL_QUCIK_HELP, qhHelpId.ToString())), "")
        End If

        defaultQhInfo = TeamTimeClass.ParseAllTemplates(defaultQhInfo, app.ActiveUser, app.ActiveProject)
        Return defaultQhInfo
    End Function

    Private Shared Property NotRecreatePipe As Boolean
        Get
            Dim sessVal = TeamTimeClass.get_SessVar(Sess_Recreate)
            Return sessVal.Equals("1")
        End Get
        Set(ByVal value As Boolean)
            TeamTimeClass.set_SessVar(Sess_Recreate, If(value, "1", "0"))
        End Set
    End Property

    Private Shared ReadOnly Property Uw As clsUserWorkgroup
        Get
            Dim app = AppSession()
            'CType(HttpContext.Current.Session("App"), clsComparionCore)
            Return app.ActiveUserWorkgroup
        End Get
    End Property

    Private Shared ReadOnly Property Ws As clsWorkspace
        Get
            Dim app = AppSession()
            'CType(HttpContext.Current.Session("App"), clsComparionCore)
            Return app.ActiveWorkspace
        End Get
    End Property

    <WebMethod(EnableSession:=True)>
    Public Shared Function getQHSettingCookies() As String
        If HttpContext.Current.Request.Cookies("Dont_Show_QH") Is Nothing Then
            HttpContext.Current.Response.Cookies("Dont_Show_QH").Expires = DateTime.Now.AddDays(10)
            HttpContext.Current.Response.Cookies("Dont_Show_QH").Value = False.ToString()
        End If
        Return HttpContext.Current.Request.Cookies("Dont_Show_QH").Value.ToString()
    End Function

    Private Shared Function GetFirstUnassessed(ByVal app As clsComparionCore) As Integer
        Dim returnValue As Integer = 0
        For i As Integer = 1 To app.ActiveProject.Pipe.Count - 1
            If AnytimeClass.IsUndefined(AnytimeClass.GetAction(i)) Then
                returnValue = i
                Exit For
            End If
        Next
        Return returnValue
    End Function

    Public Shared Function CheckProject() As Boolean
        Dim App = AppSession()
        'CType(HttpContext.Current.Session("App"), clsComparionCore)

        If App.ActiveProject IsNot Nothing Then
            Dim project = App.DBProjectByID(App.ActiveProject.ID)

            If project.isTeamTime Then
                App.ActiveProjectsList = Nothing
            End If

            App.ActiveProject = project
            Return project.isTeamTime OrElse project.isTeamTimeLikelihood
        End If

        Return False
    End Function

    Private Shared Property PipeWarning As String
        Get
            If HttpContext.Current.Session(Constants.Sess_PipeWarning) Is Nothing Then HttpContext.Current.Session(Constants.Sess_PipeWarning) = ""
            Return CStr(HttpContext.Current.Session(Constants.Sess_PipeWarning))
        End Get
        Set(ByVal value As String)
            HttpContext.Current.Session(Constants.Sess_PipeWarning) = value
        End Set
    End Property

    <WebMethod(EnableSession:=True)>
    Public Shared Function GetIntermediateResultsData(ByVal rmode As Integer) As List(Of List(Of String))
        Dim Model = New DataModel()
        Dim App = AppSession()
        'CType(HttpContext.Current.Session("App"), clsComparionCore)
        Dim iStep As Integer = App.ActiveWorkspace.ProjectStep(App.ActiveProject.isImpact)
        'ExpectedValueString = New String(2) {}
        Dim _with1 = App.ActiveProject.ProjectManager
        Dim ps As clsAction = CType(_with1.Pipe(iStep - 1), clsAction)
        Dim psLocal As clsShowLocalResultsActionData = CType(ps.ActionData, clsShowLocalResultsActionData)
        Dim sEmail As String = App.ActiveUser.UserEmail

        If AnytimeClass.UserIsReadOnly() Then
            Dim CurrentUser = App.DBUserByID(AnytimeClass.GetReadOnlyUserID())
            sEmail = CurrentUser.UserEmail
        End If

        Dim AHPUser As clsUser = _with1.GetUserByEMail(sEmail)
        Dim AHPUserID As Integer = AHPUser.UserID
        Dim reslist = New List(Of List(Of String))()
        Model.IsForAlternatives = psLocal.ParentNode IsNot Nothing AndAlso psLocal.ParentNode.IsTerminalNode
        Model.PWMode = If(psLocal.ParentNode IsNot Nothing, _with1.PipeBuilder.GetPairwiseTypeForNode(psLocal.ParentNode), Model.PWMode)

        Select Case psLocal.ResultsViewMode
            Case ResultsView.rvNone
                Model.ShowIndividualResults = False
                Model.ShowGroupResults = False
            Case ResultsView.rvIndividual
                Model.ShowIndividualResults = True
                Model.ShowGroupResults = False
            Case ResultsView.rvGroup
                Model.ShowIndividualResults = False
                Model.ShowGroupResults = True
            Case ResultsView.rvBoth
                Model.ShowIndividualResults = True
                Model.ShowGroupResults = True
        End Select

        Dim bCanShowIndividual As Boolean = False
        Dim bCanShowGroup As Boolean = False
        Dim resultmodes = AlternativeNormalizationOptions.anoPriority

        If rmode = 3 Then
            resultmodes = AlternativeNormalizationOptions.anoUnnormalized
        End If

        If rmode = 2 Then
            resultmodes = AlternativeNormalizationOptions.anoPercentOfMax
        End If

        If psLocal.ResultsViewMode = ResultsView.rvIndividual Or psLocal.ResultsViewMode = ResultsView.rvBoth Then
            bCanShowIndividual = psLocal.CanShowIndividualResults
            Model.CanShowIndividualResults = bCanShowIndividual
        Else
            Model.CanShowIndividualResults = False
        End If

        Model.CanEditModel = App.CanUserModifyProject(App.ActiveUser.UserID, App.ActiveProject.ID, App.ActiveUserWorkgroup, App.ActiveWorkspace, App.ActiveWorkgroup)
        Model.ShowKnownLikelihoods = psLocal.ParentNode.MeasureType() = ECMeasureType.mtPWAnalogous

        If psLocal.ResultsViewMode = ResultsView.rvGroup Or psLocal.ResultsViewMode = ResultsView.rvBoth Then
            bCanShowGroup = True
            Model.CanShowGroupResults = bCanShowGroup
        Else
            Model.CanShowGroupResults = False
        End If

        If psLocal.ResultsViewMode = ResultsView.rvBoth AndAlso psLocal.ParentNode.Hierarchy.ProjectManager.UsersList.Count <= 1 Then
            bCanShowGroup = False
            Model.CanShowGroupResults = False
            Model.ShowGroupResults = False
        End If

        Model.InsufficientInfo = Not ((Model.ShowIndividualResults AndAlso Model.CanShowIndividualResults) OrElse (Model.ShowGroupResults AndAlso Model.CanShowGroupResults))

        If Model.ShowGroupResults AndAlso Model.CanShowGroupResults AndAlso Model.ShowIndividualResults AndAlso (Not Model.CanShowIndividualResults) Then
            Model.CanNotShowLocalResults = True
            Model.InsufficientInfo = False
            Model.ShowIndividualResults = False
        End If

        Dim mResultsList As ArrayList = Nothing
        Dim mIndividualResultsList As ArrayList = Nothing
        Dim mGroupResultsList As ArrayList = Nothing

        If psLocal.ResultsViewMode <> ResultsView.rvNone Then

            If bCanShowGroup And (psLocal.ResultsViewMode = ResultsView.rvGroup OrElse psLocal.ResultsViewMode = ResultsView.rvBoth) Then

                If AnytimeClass.CombinedUserID = COMBINED_USER_ID Then
                    mGroupResultsList = CType(psLocal.ResultsList(_with1.CombinedGroups.GetDefaultCombinedGroup().CombinedUserID, AHPUser.UserID).Clone(), ArrayList)
                Else
                    mGroupResultsList = CType(psLocal.ResultsList(AnytimeClass.CombinedUserID, AHPUser.UserID).Clone(), ArrayList)
                End If
            End If

            mIndividualResultsList = CType(psLocal.ResultsList(AHPUser.UserID, AHPUser.UserID).Clone(), ArrayList)
            mResultsList = mIndividualResultsList

            If mResultsList Is Nothing Then
                mResultsList = mGroupResultsList
            End If
        End If

        If (bCanShowIndividual OrElse bCanShowGroup) AndAlso psLocal.ShowExpectedValue Then

            If bCanShowIndividual AndAlso Not IsCombinedUserID(AHPUserID) Then
                Model.ExpectedValueIndiv = psLocal.ExpectedValue(AHPUserID)
                Model.ExpectedValueIndivVisible = True
            End If

            If bCanShowGroup Then
                Model.ExpectedValueComb = psLocal.ExpectedValue(AnytimeClass.CombinedUserID)
                Model.ExpectedValueCombVisible = True
            End If
        End If

        Dim list As List(Of StepsPairs) = GeckoClass.GetEvalPipeStepsList(psLocal.ParentNode.NodeID, iStep, psLocal.PWOutcomesNode)
        Dim numChildren As Integer = 0
        Dim pwJudgments As clsPairwiseJudgments = Nothing
        Dim calcTarget As clsCalculationTarget = New clsCalculationTarget(CalculationTargetType.cttUser, psLocal.ParentNode.Hierarchy.ProjectManager.GetUserByID(AHPUserID))
        Model.StepPairs = New List(Of StepsPairs)()

        Select Case AnytimeClass.GetAction(iStep - 1).ActionType
            Case ActionType.atAllPairwise, ActionType.atPairwise, ActionType.atPairwiseOutcomes, ActionType.atAllPairwiseOutcomes

                If psLocal.PWOutcomesNode Is Nothing Then
                    Dim nodesBelow As List(Of clsNode) = psLocal.ParentNode.GetNodesBelow(AHPUserID)
                    Dim i As Integer = nodesBelow.Count - 1

                    While i >= 0
                        If nodesBelow(i).RiskNodeType = RiskNodeType.ntCategory Then nodesBelow.RemoveAt(i)
                        i += -1
                    End While

                    numChildren = nodesBelow.Count
                    pwJudgments = CType(psLocal.ParentNode.Judgments, clsPairwiseJudgments)
                Else
                    numChildren = (CType(psLocal.ParentNode.MeasurementScale, clsRatingScale)).RatingSet.Count
                    pwJudgments = psLocal.PWOutcomesNode.PWOutcomesJudgments
                End If

                For i As Integer = 1 To CInt((numChildren * (numChildren - 1)) / 2)
                    Dim pwData As clsPairwiseMeasureData = Nothing

                    If psLocal.PWOutcomesNode Is Nothing Then
                        pwData = pwJudgments.GetNthMostInconsistentJudgment(calcTarget, i)
                    Else
                        pwData = pwJudgments.GetNthMostInconsistentJudgmentOutcomes(calcTarget, i, CType(psLocal.ParentNode.MeasurementScale, clsRatingScale))
                    End If

                    If pwData IsNot Nothing Then

                        For Each Pair As StepsPairs In list

                            If (Pair.Obj1 = pwData.FirstNodeID AndAlso Pair.Obj2 = pwData.SecondNodeID) OrElse (Pair.Obj2 = pwData.FirstNodeID AndAlso Pair.Obj1 = pwData.SecondNodeID) Then
                                Pair.Rank = i
                            End If
                        Next
                    End If
                Next

                For Each Pair As StepsPairs In list
                    Dim pwData As clsPairwiseMeasureData = Nothing

                    If psLocal.PWOutcomesNode Is Nothing Then
                        pwData = pwJudgments.GetBestFitJudgment(calcTarget, Pair.Obj1, Pair.Obj2)
                    Else
                        pwData = pwJudgments.GetBestFitJudgmentOutcomes(calcTarget, Pair.Obj1, Pair.Obj2, CType(psLocal.ParentNode.MeasurementScale, clsRatingScale))
                    End If

                    If pwData Is Nothing Then
                        Pair.BestFitValue = 0
                        Pair.BestFitAdvantage = 0
                    Else
                        Pair.BestFitValue = pwData.Value
                        Pair.BestFitAdvantage = pwData.Advantage
                    End If
                Next

                For Each pair As StepsPairs In list
                    Model.StepPairs.Add(pair)
                Next
        End Select

        Model.ParentNodeName = psLocal.ParentNode.NodeName
        Model.ParentID = psLocal.ParentNode.NodeID
        Model.IsParentNodeGoal = psLocal.ParentNode.ParentNode() Is Nothing
        Dim parentNodeKnownLikelihood As String = ""

        If psLocal.ParentNode.ParentNode() IsNot Nothing AndAlso psLocal.ParentNode.ParentNode().MeasureType() = ECMeasureType.mtPWAnalogous Then
            Dim nl As List(Of KnownLikelihoodDataContract) = psLocal.ParentNode.ParentNode().GetKnownLikelihoods()

            For Each item As KnownLikelihoodDataContract In nl
                If item.GuidID.Equals(psLocal.ParentNode.NodeGuidID) AndAlso item.Value > 0 Then parentNodeKnownLikelihood = item.Value.ToString()
            Next
        End If

        Model.ParentNodeKnownLikelihood = parentNodeKnownLikelihood
        Dim PM = App.ActiveProject.ProjectManager

        If psLocal.ResultsViewMode <> ResultsView.rvNone Then

            If bCanShowIndividual And (psLocal.ResultsViewMode = ResultsView.rvIndividual Or psLocal.ResultsViewMode = ResultsView.rvBoth) Then
                PM.CalculationsManager.Calculate(calcTarget, PM.Hierarchy(PM.ActiveHierarchy).Nodes(0), PM.ActiveHierarchy, PM.ActiveAltsHierarchy)
                Model.ParentNodeGlobalPriority = psLocal.ParentNode.WRTGlobalPriority
            End If

            If bCanShowGroup And (psLocal.ResultsViewMode = ResultsView.rvGroup Or psLocal.ResultsViewMode = ResultsView.rvBoth) Then

                If AnytimeClass.CombinedUserID = COMBINED_USER_ID Then
                    Dim CG As clsCombinedGroup = PM.CombinedGroups.GetDefaultCombinedGroup()
                    Dim calcTargetCombined As clsCalculationTarget = New clsCalculationTarget(CalculationTargetType.cttCombinedGroup, CG)
                    PM.CalculationsManager.Calculate(calcTargetCombined, PM.Hierarchy(PM.ActiveHierarchy).Nodes(0), PM.ActiveHierarchy, PM.ActiveAltsHierarchy)
                Else
                    Dim calcTargetCombined As clsCalculationTarget = New clsCalculationTarget(CalculationTargetType.cttUser, psLocal.ParentNode.Hierarchy.ProjectManager.GetUserByID(AnytimeClass.CombinedUserID))
                    PM.CalculationsManager.Calculate(calcTargetCombined, PM.Hierarchy(PM.ActiveHierarchy).Nodes(0), PM.ActiveHierarchy, PM.ActiveAltsHierarchy)
                End If

                Model.ParentNodeGlobalPriorityCombined = psLocal.ParentNode.WRTGlobalPriority
            End If
        End If

        If psLocal.ResultsViewMode <> ResultsView.rvNone Then

            If mResultsList IsNot Nothing Then
                Dim IsSumMore1_Individual As Boolean = False
                Dim IsSumMore1_Group As Boolean = False
                Dim Sum_Individual As Double = 0
                Dim Sum_Group As Double = 0

                For i As Integer = 0 To mResultsList.Count - 1
                    If mIndividualResultsList IsNot Nothing AndAlso mIndividualResultsList.Count > i Then Sum_Individual += (CType(mIndividualResultsList(i), clsResultsItem)).UnnormalizedValue
                    If mGroupResultsList IsNot Nothing AndAlso mGroupResultsList.Count > i Then Sum_Group += (CType(mGroupResultsList(i), clsResultsItem)).UnnormalizedValue
                Next

                If Sum_Individual > 1 + 0.000001 Then IsSumMore1_Individual = True
                If Sum_Group > 1 + 0.000001 Then IsSumMore1_Group = True
                Model.IsPWNLandNormalizedParticipantResults = psLocal.ParentNode.MeasureType() = ECMeasureType.mtPWAnalogous AndAlso IsSumMore1_Individual
                Model.IsPWNLandNormalizedGroupResults = psLocal.ParentNode.MeasureType() = ECMeasureType.mtPWAnalogous AndAlso IsSumMore1_Group
                Model.ObjectivesData.Clear()

                For i As Integer = 0 To mResultsList.Count - 1
                    Dim Res = New Objective()
                    Dim reuse = New List(Of String)()
                    Dim listItem As clsResultsItem = CType(mResultsList(i), clsResultsItem)
                    reuse.Add((i + 1).ToString())
                    Res.Name = listItem.Name
                    reuse.Add(listItem.Name)

                    If True Then
                        Dim R As clsResultsItem = CType(mIndividualResultsList(i), clsResultsItem)
                        reuse.Add(Convert.ToSingle((If(rmode = 1, R.Value, R.UnnormalizedValue))).ToString())
                        Res.Value = If(rmode = 1, R.Value, R.UnnormalizedValue)
                        Res.GlobalValue = Res.Value * Model.ParentNodeGlobalPriority
                    End If

                    If psLocal.ResultsViewMode = ResultsView.rvGroup Or psLocal.ResultsViewMode = ResultsView.rvBoth Then
                        Dim R As clsResultsItem = CType(mGroupResultsList(i), clsResultsItem)
                        reuse.Add(Convert.ToSingle(If(rmode = 1, R.Value, R.UnnormalizedValue)).ToString())
                        Res.CombinedValue = If(rmode = 1, R.Value, R.UnnormalizedValue)
                        Res.GlobalValueCombined = Res.CombinedValue * Model.ParentNodeGlobalPriorityCombined
                    Else
                        reuse.Add("")
                    End If

                    reuse.Add(psLocal.ResultsViewMode.ToString())
                    reuse.Add(listItem.ObjectID.ToString())
                    reslist.Add(reuse)

                    If psLocal.ParentNode.MeasureType() = ECMeasureType.mtPWAnalogous Then
                        Dim KnownLikelihoods As List(Of KnownLikelihoodDataContract) = psLocal.ParentNode.GetKnownLikelihoods()

                        If KnownLikelihoods IsNot Nothing Then
                            Dim k As Integer = 0

                            While k <= KnownLikelihoods.Count - 1
                                Dim lkhd = KnownLikelihoods(k)

                                If listItem.ObjectID = lkhd.ID AndAlso lkhd.Value > 0 Then
                                    Res.AltWithKnownLikelihoodName = lkhd.NodeName
                                    Res.AltWithKnownLikelihoodID = lkhd.ID
                                    Res.AltWithKnownLikelihoodGuidID = lkhd.GuidID
                                    Res.AltWithKnownLikelihoodValue = lkhd.Value

                                    If Res.AltWithKnownLikelihoodValue > 0 Then
                                        Res.AltWithKnownLikelihoodValueString = Res.AltWithKnownLikelihoodValue.ToString()
                                    End If

                                    k = KnownLikelihoods.Count
                                End If

                                k += 1
                            End While
                        End If
                    End If

                    Res.ID = listItem.ObjectID
                    Model.ObjectivesData.Add(Res)
                    Res.Index = Model.ObjectivesData.Count
                Next

                Dim IndivExpectedValue As Double = 0
                Dim CombinedExpectedValue As Double = 0
                Dim CanShowHiddenExpectedValue As Boolean = False

                Select Case resultmodes
                    Case AlternativeNormalizationOptions.anoPriority
                        Dim fSumValue As Double = Model.ObjectivesData.Sum(Function(d) d.Value)
                        Dim fSumCombinedValue As Double = Model.ObjectivesData.Sum(Function(d) d.CombinedValue)
                        'Dim fSumGlobalValue As Double = Model.ObjectivesData.Sum(Function(d) d.GlobalValue)
                        'Dim fSumGlobalCombinedValue As Double = Model.ObjectivesData.Sum(Function(d) d.GlobalValueCombined)
                        If fSumValue = 0 Then fSumValue = 1
                        If fSumCombinedValue = 0 Then fSumCombinedValue = 1
                        'If fSumGlobalValue = 0 Then fSumGlobalValue = 1
                        'If fSumGlobalCombinedValue = 0 Then fSumGlobalCombinedValue = 1

                        For i As Integer = 0 To Model.ObjectivesData.Count - 1
                            'Dim item_loopVariable = Model.ObjectivesData(i)
                            Dim item = Model.ObjectivesData(i)
                            reslist(i)(2) = (item.Value / fSumValue).ToString()
                            reslist(i)(3) = (item.CombinedValue / fSumCombinedValue).ToString()
                        Next

                    Case AlternativeNormalizationOptions.anoPercentOfMax
                        Dim fMaxValue As Double = Model.ObjectivesData.Max(Function(d) d.Value)
                        Dim fMaxCombinedValue As Double = Model.ObjectivesData.Max(Function(d) d.CombinedValue)
                        'Dim fMaxGlobalValue As Double = Model.ObjectivesData.Max(Function(d) d.GlobalValue)
                        'Dim fMaxGlobalCombinedValue As Double = Model.ObjectivesData.Max(Function(d) d.GlobalValueCombined)
                        If fMaxValue = 0 Then fMaxValue = 1
                        If fMaxCombinedValue = 0 Then fMaxCombinedValue = 1
                        'If fMaxGlobalValue = 0 Then fMaxGlobalValue = 1
                        'If fMaxGlobalCombinedValue = 0 Then fMaxGlobalCombinedValue = 1

                        For i As Integer = 0 To Model.ObjectivesData.Count - 1
                            Dim item = New Objective()
                            item = Model.ObjectivesData(i)
                            reslist(i)(2) = (item.Value / fMaxValue).ToString()
                            reslist(i)(3) = (item.CombinedValue / fMaxCombinedValue).ToString()
                        Next
                End Select

                For Each item As Objective In Model.ObjectivesData
                    Dim s = item.Name.Split(" "c)

                    If s.Length > 0 Then
                        Dim d As Double = 0

                        If Double.TryParse(s(0), d) Then
                            CanShowHiddenExpectedValue = True
                            IndivExpectedValue += d * item.Value
                            CombinedExpectedValue += d * item.CombinedValue
                        End If
                    End If
                Next

                ExpectedValueString = New String(2) {}
                If CanShowHiddenExpectedValue Then

                    If bCanShowIndividual Then
                        ExpectedValueString(0) = " Your Expected value = " & If(IndivExpectedValue.ToString() <> "" AndAlso IndivExpectedValue.ToString() <> "NaN" AndAlso IsNumeric(IndivExpectedValue), (Math.Round(IndivExpectedValue, 2)).ToString(), "")
                    End If

                    If bCanShowGroup Then
                        ExpectedValueString(1) = " Combined Expected value = " & If(CombinedExpectedValue.ToString() <> "" AndAlso CombinedExpectedValue.ToString() <> "NaN" AndAlso IsNumeric(CombinedExpectedValue), (Math.Round(CombinedExpectedValue, 2)).ToString(), "")
                    End If
                End If

                ExpectedValueString(2) = If(CanShowHiddenExpectedValue, "1", "0")
                Model.ObjectivesDataSorted = New List(Of Objective)()
                Model.ObjectivesDataSorted.AddRange(Model.ObjectivesData)

                If HttpContext.Current.Session(InconsistencySortingEnabled) Is Nothing Then
                    HttpContext.Current.Session(InconsistencySortingEnabled) = False
                    HttpContext.Current.Session(BestFit) = False
                End If

                If HttpContext.Current.Session(InconsistencySortingOrder) Is Nothing Then
                    HttpContext.Current.Session(InconsistencySortingOrder) = New List(Of Integer)()
                End If

                If CBool(HttpContext.Current.Session(InconsistencySortingEnabled)) Then
                    Model.ObjectivesDataSorted.Sort(Function(obj1, obj2) obj2.Value.CompareTo(obj1.Value))
                    Dim sortOrder = 0
                    Dim sortList = New List(Of Integer)()

                    For Each objective In Model.ObjectivesDataSorted
                        objective.SortOrder = Math.Min(Threading.Interlocked.Increment(sortOrder), sortOrder - 1)
                        sortList.Add(objective.ID)
                    Next

                    HttpContext.Current.Session(InconsistencySortingOrder) = sortList
                Else
                    Dim sortList = CType(HttpContext.Current.Session(InconsistencySortingOrder), List(Of Integer))

                    If sortList.Count > 0 Then
                        Dim objectivesWithOldSort = New List(Of Objective)()

                        For Each objectiveId In sortList
                            Dim objective = Model.ObjectivesDataSorted.First(Function(o) o.ID = objectiveId)

                            If objective IsNot Nothing Then
                                objectivesWithOldSort.Add(objective)
                            End If
                        Next

                        Model.ObjectivesDataSorted = New List(Of Objective)()
                        Model.ObjectivesDataSorted.AddRange(objectivesWithOldSort)
                    End If
                End If
            End If
        End If

        HttpContext.Current.Session(Constants.SessionModel) = Model
        Return reslist
    End Function


    <WebMethod(EnableSession:=True)>
    Public Shared Function GetNextUnassessed(ByVal StartingStep As Integer) As Integer()
        Dim context As HttpContext = HttpContext.Current
        Dim App = AppSession()
        'CType(context.Session("App"), clsComparionCore)
        Dim returnValue As Integer() = New Integer(1) {}
        If StartingStep < 0 Then StartingStep = 0
        Dim unassessed_step As Integer = StartingStep
        Dim unassessed_count = 0

        For i As Integer = 1 To App.ActiveProject.Pipe.Count - 1

            If AnytimeClass.IsUndefined(AnytimeClass.GetAction(i)) Then
                unassessed_count += 1
                If unassessed_count > 2 Then Exit For
            End If
        Next

        For i As Integer = unassessed_step + 1 To App.ActiveProject.Pipe.Count - 1

            If AnytimeClass.IsUndefined(AnytimeClass.GetAction(i)) Then
                unassessed_step = i
                returnValue(0) = unassessed_step
                returnValue(1) = unassessed_count
                Return returnValue
            End If
        Next

        For i As Integer = 1 To App.ActiveProject.Pipe.Count - 1

            If AnytimeClass.IsUndefined(AnytimeClass.GetAction(i)) Then
                unassessed_step = i
                returnValue(0) = unassessed_step
                returnValue(1) = unassessed_count
                Return returnValue
            End If
        Next

        Return Nothing
    End Function

    <WebMethod(EnableSession:=True)>
    Public Shared Function GetOverallResultsData(ByVal rmode As Integer, ByVal wrtnodeID As Integer, ByVal isReload As Boolean) As List(Of Object)
        Dim returnObject = New List(Of Object)()
        Dim context As HttpContext = HttpContext.Current
        Dim App = AppSession()
        'CType(context.Session("App"), clsComparionCore)
        Dim iStep As Integer = App.ActiveWorkspace.ProjectStep(App.ActiveProject.isImpact)
        Dim _with1 = App.ActiveProject.ProjectManager
        Dim ps As clsAction = CType(_with1.Pipe(iStep - 1), clsAction)
        ExpectedValueString = New String(2) {}
        Dim psLocal As clsShowGlobalResultsActionData = CType(AnytimeClass.GetAction(iStep).ActionData, clsShowGlobalResultsActionData)
        psLocal.WRTNode = App.ActiveProject.ProjectManager.Hierarchies(0).GetNodeByID(wrtnodeID)
        If isReload OrElse context.Session(Sess_WrtNode) Is Nothing Then context.Session(Sess_WrtNode) = psLocal.WRTNode
        Dim sEmail As String = App.ActiveUser.UserEmail

        If AnytimeClass.UserIsReadOnly() Then
            Dim CurrentUser = App.DBUserByID(AnytimeClass.GetReadOnlyUserID())
            sEmail = CurrentUser.UserEmail
        End If

        Dim AHPUser As ECTypes.clsUser = _with1.GetUserByEMail(sEmail)
        Dim AHPUserID As Integer = AHPUser.UserID
        Dim reslist = New List(Of List(Of Object))()
        Dim bCanShowIndividual As Boolean = False
        Dim bCanShowGroup As Boolean = False

        If _with1.PipeBuilder.PipeParameters.GlobalResultsView = CanvasTypes.ResultsView.rvIndividual Or _with1.PipeBuilder.PipeParameters.GlobalResultsView = CanvasTypes.ResultsView.rvBoth Then
            bCanShowIndividual = psLocal.CanShowIndividualResults(AHPUserID, psLocal.WRTNode)
        End If

        If _with1.PipeBuilder.PipeParameters.GlobalResultsView = CanvasTypes.ResultsView.rvGroup Or _with1.PipeBuilder.PipeParameters.GlobalResultsView = CanvasTypes.ResultsView.rvBoth Then
            bCanShowGroup = True
        End If

        Dim canshowresult = New With {
        Key .individual = (bCanShowIndividual AndAlso (_with1.PipeParameters.GlobalResultsView = CanvasTypes.ResultsView.rvIndividual OrElse _with1.PipeParameters.GlobalResultsView = CanvasTypes.ResultsView.rvBoth)),
        Key .combined = (bCanShowGroup AndAlso (_with1.PipeParameters.GlobalResultsView = CanvasTypes.ResultsView.rvGroup OrElse _with1.PipeParameters.GlobalResultsView = CanvasTypes.ResultsView.rvBoth))
    }
        Dim messagenote = New String(1) {}
        messagenote(0) = TeamTimeClass.ResString("msgNoOverallResults")

        If Not canshowresult.individual Then
            messagenote(1) = TeamTimeClass.ResString("msgNoEvalDataIndividualResults")
        ElseIf Not canshowresult.combined Then
            messagenote(1) = TeamTimeClass.ResString("msgNoEvalDataGroupResults")
        End If

        Dim mResultsList As ArrayList = Nothing
        Dim mIndividualResultsList As ArrayList = Nothing
        Dim mGroupResultsList As ArrayList = Nothing

        If _with1.PipeBuilder.PipeParameters.GlobalResultsView <> CanvasTypes.ResultsView.rvNone Then
            Dim resultmode = AlternativeNormalizationOptions.anoPriority

            If rmode = 3 Then
                resultmode = AlternativeNormalizationOptions.anoUnnormalized
            End If

            If rmode = 2 Then
                resultmode = AlternativeNormalizationOptions.anoPercentOfMax
            End If

            mIndividualResultsList = CType(psLocal.ResultsList(AHPUser.UserID, AHPUser.UserID, resultmode).Clone(), ArrayList)
            mResultsList = mIndividualResultsList

            If bCanShowGroup And (_with1.PipeBuilder.PipeParameters.GlobalResultsView = CanvasTypes.ResultsView.rvGroup Or _with1.PipeBuilder.PipeParameters.GlobalResultsView = CanvasTypes.ResultsView.rvBoth) Then
                Dim resultmodes = AlternativeNormalizationOptions.anoPriority

                If rmode = 3 Then
                    resultmodes = AlternativeNormalizationOptions.anoUnnormalized
                End If

                If rmode = 2 Then
                    resultmodes = AlternativeNormalizationOptions.anoPercentOfMax
                End If

                If AnytimeClass.CombinedUserID = ECTypes.COMBINED_USER_ID Then
                    mGroupResultsList = CType(psLocal.ResultsList(_with1.CombinedGroups.GetDefaultCombinedGroup().CombinedUserID, AHPUser.UserID, resultmodes).Clone(), ArrayList)
                Else
                    mGroupResultsList = CType(psLocal.ResultsList(AnytimeClass.CombinedUserID, AHPUser.UserID, resultmodes).Clone(), ArrayList)
                End If

                mResultsList = mGroupResultsList
            End If
        End If

        If _with1.PipeBuilder.PipeParameters.GlobalResultsView <> CanvasTypes.ResultsView.rvNone Then

            If mResultsList IsNot Nothing Then
                Dim IsSumMore1_Individual As Boolean = False
                Dim IsSumMore1_Group As Boolean = False
                Dim Sum_Individual As Double = 0
                Dim Sum_Group As Double = 0

                For i As Integer = 0 To mResultsList.Count - 1
                    If mIndividualResultsList IsNot Nothing AndAlso mIndividualResultsList.Count > i Then Sum_Individual += (CType(mIndividualResultsList(i), clsResultsItem)).UnnormalizedValue
                    If mGroupResultsList IsNot Nothing AndAlso mGroupResultsList.Count > i Then Sum_Group += (CType(mGroupResultsList(i), clsResultsItem)).UnnormalizedValue
                Next

                If Sum_Individual > 1 + 0.000001 Then IsSumMore1_Individual = True
                If Sum_Group > 1 + 0.000001 Then IsSumMore1_Group = True
                Dim objlist = New List(Of Objective)()

                For i As Integer = 0 To mResultsList.Count - 1
                    Dim Res = New ExpertChoice.Results.Objective()
                    Dim reuse = New List(Of Object)()
                    Dim listItem As clsResultsItem = CType(mResultsList(i), clsResultsItem)
                    reuse.Add((i + 1))
                    reuse.Add(listItem.Name)
                    Res.Name = listItem.Name

                    If psLocal.CanShowIndividualResults(AHPUserID, psLocal.WRTNode) AndAlso mIndividualResultsList IsNot Nothing AndAlso mIndividualResultsList.Count > 0 AndAlso mIndividualResultsList.Count > i Then
                        Dim R As clsResultsItem = CType(mIndividualResultsList(i), clsResultsItem)
                        reuse.Add(Convert.ToSingle((If(rmode = 1 OrElse rmode = 2, R.Value, R.UnnormalizedValue))).ToString())
                        Res.Value = If(rmode = 1, R.Value, R.UnnormalizedValue)
                        Res.GlobalValue = If(psLocal.WRTNode IsNot Nothing, Res.Value * psLocal.WRTNode.WRTGlobalPriority, 0)
                    Else
                        reuse.Add("0")
                    End If

                    If bCanShowGroup And (_with1.PipeBuilder.PipeParameters.GlobalResultsView = CanvasTypes.ResultsView.rvGroup Or _with1.PipeBuilder.PipeParameters.GlobalResultsView = CanvasTypes.ResultsView.rvBoth) Then
                        Dim R As clsResultsItem = CType(mGroupResultsList(i), clsResultsItem)
                        reuse.Add(Convert.ToSingle((If(rmode = 1 OrElse rmode = 2, R.Value, R.UnnormalizedValue))).ToString())
                        Res.CombinedValue = If(rmode = 1, R.Value, R.UnnormalizedValue)
                        Res.GlobalValueCombined = If(psLocal.WRTNode IsNot Nothing, Res.CombinedValue * psLocal.WRTNode.WRTGlobalPriority, 0)
                    Else
                        reuse.Add("0")
                    End If

                    reuse.Add(App.ActiveProject.PipeParameters.GlobalResultsView.ToString())
                    reslist.Add(reuse)

                    If psLocal.WRTNode.MeasureType() = ECMeasureType.mtPWAnalogous Then
                        Dim KnownLikelihoods As List(Of ECTypes.KnownLikelihoodDataContract) = psLocal.WRTNode.GetKnownLikelihoods()

                        If KnownLikelihoods IsNot Nothing Then
                            Dim k As Integer = 0

                            While k <= KnownLikelihoods.Count - 1
                                Dim lkhd = KnownLikelihoods(k)

                                If listItem.ObjectID = lkhd.ID AndAlso lkhd.Value > 0 Then
                                    Res.AltWithKnownLikelihoodName = lkhd.NodeName
                                    Res.AltWithKnownLikelihoodID = lkhd.ID
                                    Res.AltWithKnownLikelihoodGuidID = lkhd.GuidID
                                    Res.AltWithKnownLikelihoodValue = lkhd.Value

                                    If Res.AltWithKnownLikelihoodValue > 0 Then
                                        Res.AltWithKnownLikelihoodValueString = Res.AltWithKnownLikelihoodValue.ToString()
                                    End If

                                    k = KnownLikelihoods.Count
                                End If

                                k += 1
                            End While
                        End If
                    End If

                    Res.ID = listItem.ObjectID
                    objlist.Add(Res)
                Next

                Dim IndivExpectedValue As Double = 0
                Dim CombinedExpectedValue As Double = 0
                Dim CanShowHiddenExpectedValue As Boolean = False

                For Each item As Objective In objlist
                    Dim s = item.Name.Split(" "c)

                    If s.Length > 0 Then
                        Dim d As Double = 0

                        If Double.TryParse(s(0), d) Then
                            CanShowHiddenExpectedValue = True
                            IndivExpectedValue += d * item.Value
                            CombinedExpectedValue += d * item.CombinedValue
                        End If
                    End If
                Next

                If CanShowHiddenExpectedValue Then

                    If bCanShowIndividual Then
                        ExpectedValueString(0) = $" {TeamTimeClass.ResString("lblExpectedValueIndiv")} {Math.Round(IndivExpectedValue, 4)}"
                    End If

                    If bCanShowGroup Then
                        ExpectedValueString(1) = $" {TeamTimeClass.ResString("lblExpectedValueComb")} {Math.Round(CombinedExpectedValue, 2)}"
                    End If
                End If

                ExpectedValueString(2) = If(CanShowHiddenExpectedValue, "1", "0")
            End If
        End If

        returnObject.Add(reslist)
        returnObject.Add(canshowresult)
        If psLocal.WRTNode IsNot Nothing Then
            returnObject.Add(psLocal.WRTNode.NodeName)
        End If
        Dim dta = JsonConvert.SerializeObject(App.ActiveProject.PipeParameters)
        Dim dta1 = JObject.Parse(dta)
        'Dim ShowBars As Boolean = CType(dta1("ShowBars").ToString(), Boolean)
        Dim ShowBars As Boolean = True
        'returnObject.Add(ShowBars)
        returnObject.Add(messagenote)
        Dim ActiveHierarchies = App.ActiveProject.ProjectManager.GetAllHierarchies()
        psLocal.WRTNode = App.ActiveProject.ProjectManager.Hierarchies(0).GetNodeByID(ActiveHierarchies(0).Nodes(0).NodeID)
        Return returnObject
    End Function

    <WebMethod(EnableSession:=True)>
    Public Shared Sub setMultiBarsMode(ByVal value As Boolean)
        Dim context = HttpContext.Current
        Dim isPipeViewOnly = CBool(context.Session(Constants.SessionIsPipeViewOnly))


        If isPipeViewOnly Then
            Return
        End If

        Dim App = AppSession()
        'CType(context.Session("App"), clsComparionCore)

        If App IsNot Nothing Then
            Dim isPM As Boolean = App.CanUserModifyProject(App.ActiveUser.UserID, App.ProjectID, Uw, Ws, App.ActiveWorkgroup)

            If isPM Then
                App.ActiveProject.ProjectManager.Parameters.EvalCollapseMultiPWBars = value
                App.ActiveProject.SaveProjectOptions("Save eval collapse bars mode")
            End If
        End If

    End Sub
    <WebMethod(EnableSession:=True)>
    Public Shared Function setCurrentStep(ByVal stepNo As Integer) As Boolean
        If stepNo > 0 Then
            Dim CurrentStep = CInt(HttpContext.Current.Session("AT_CurrentStep"))
            If CurrentStep <> stepNo Then
                HttpContext.Current.Session(SessionIsJudgmentAlreadySaved) = False
            End If
            HttpContext.Current.Session("AT_CurrentStep") = stepNo
        Else
            HttpContext.Current.Session("AT_CurrentStep") = 1
        End If

        Return True
    End Function

    Protected Sub hdnPageNo_Click(sender As Object, e As EventArgs)
        Dim stepNo As String = Request.Form("hdnPageNumber")
        'Dim script As String = $"<script type='text/javascript'> alert('stepNo-0-{stepNo}');</script>"
        'ClientScript.RegisterClientScriptBlock(Me.GetType(), "AlertBox0", script)
        'stepNo = If(stepNo Is Nothing OrElse stepNo.Trim() = "" OrElse stepNo.Trim() = "0", Request.Form("hdnPageNumber1"), stepNo)
        'script = $"<script type='text/javascript'> alert('stepNo-{stepNo}');</script>"
        'ClientScript.RegisterClientScriptBlock(Me.GetType(), "AlertBox", script)
        If IsNumeric(stepNo) AndAlso Convert.ToInt32(stepNo) > 0 Then
            'script = $"<script type='text/javascript'> alert('stepNo-2-{stepNo}');</script>"
            'ClientScript.RegisterClientScriptBlock(Me.GetType(), "AlertBox2", script)
            BindUserControl(Convert.ToInt32(stepNo))
            'Session("AT_CurrentStep") = hdnPageNumber.Value
            'Response.Redirect(HttpContext.Current.Request.Url.ToString(), True)
        End If
    End Sub

    <WebMethod(EnableSession:=True)>
    Public Shared Function reviewJudgment(ByVal parentnodeID As Integer, ByVal current_step As Integer) As Integer
        Dim context As HttpContext = HttpContext.Current
        Dim App = AppSession()
        'CType(context.Session("App"), clsComparionCore)
        Dim CurrentNode = App.ActiveProject.HierarchyObjectives.GetNodeByID(parentnodeID)
        Return App.ActiveProject.ProjectManager.PipeBuilder.GetFirstEvalPipeStepForNode(CurrentNode, current_step) + 1
    End Function

    <WebMethod(EnableSession:=True)>
    Public Shared Function redoPairs(ByVal parentnodeID As Integer, ByVal current_step As Integer, ByVal firstNode As String, ByVal secondNode As String, ByVal is_name As Boolean, ByVal add_step As Boolean) As Integer
        Dim context As HttpContext = HttpContext.Current
        Dim app = AppSession()
        'CType(context.Session("App"), clsComparionCore)
        Dim CurrentNode = app.ActiveProject.HierarchyObjectives.GetNodeByID(parentnodeID)
        Dim minStep = app.ActiveProject.ProjectManager.PipeBuilder.GetFirstEvalPipeStepForNode(CurrentNode, current_step) + 1
        Dim sFirstNode = New clsNode()
        Dim sSecondNode = New clsNode()
        AnytimeClass.JudgmentsSaved = True

        If add_step Then
            Dim PID As Integer = parentnodeID
            Dim ID1 As Integer = -1
            Dim ID2 As Integer = -1

            If Integer.TryParse(firstNode, ID1) AndAlso Integer.TryParse(secondNode, ID2) Then
                Dim Data As clsShowLocalResultsActionData = CType(AnytimeClass.GetAction(current_step).ActionData, clsShowLocalResultsActionData)
                'Dim fIsPWOutcomes As Boolean = Data.PWOutcomesNode IsNot Nothing AndAlso Data.ParentNode.get_MeasureType() = ECMeasureType.mtPWOutcomes
                Dim fIsPWOutcomes As Boolean = Data.PWOutcomesNode IsNot Nothing AndAlso Data.ParentNode.MeasureType() = ECMeasureType.mtPWOutcomes

                If fIsPWOutcomes Then
                    app.ActiveProject.ProjectManager.PipeBuilder.AddPairToPipePWOutcomes(Data.PWOutcomesNode, Data.ParentNode, ID1, ID2, current_step - 1)
                Else
                    app.ActiveProject.ProjectManager.PipeBuilder.AddPairToPipe(PID, ID1, ID2, current_step - 1)
                End If

                If Not app.ActiveProject.PipeParameters.ObjectivesPairwiseOneAtATime Then current_step = current_step - 1
                'NotRecreatePipe = True

                context.Session("PWsteps") = Nothing
                context.Session("PWsteps") = app.ActiveProject.Pipe
            End If
        Else

            For i As Integer = minStep To current_step - 1
                Dim action = AnytimeClass.GetAction(i)

                Select Case action.ActionType
                    Case ActionType.atPairwise
                        Dim pwdata = CType(AnytimeClass.GetAction(i).ActionData, clsPairwiseMeasureData)
                        Dim parentNode = app.ActiveProject.HierarchyObjectives.GetNodeByID(pwdata.ParentNodeID)
                        Dim fAlts As Boolean = parentNode.IsTerminalNode

                        If fAlts Then
                            sFirstNode = app.ActiveProject.HierarchyAlternatives.GetNodeByID(pwdata.FirstNodeID)
                            sSecondNode = app.ActiveProject.HierarchyAlternatives.GetNodeByID(pwdata.SecondNodeID)
                        Else
                            sFirstNode = app.ActiveProject.HierarchyObjectives.GetNodeByID(pwdata.FirstNodeID)
                            sSecondNode = app.ActiveProject.HierarchyObjectives.GetNodeByID(pwdata.SecondNodeID)
                        End If

                        Exit Select
                    Case ActionType.atAllPairwise, ActionType.atAllPairwiseOutcomes
                        Dim pwdata = CType(AnytimeClass.GetAction(i).ActionData, clsAllPairwiseEvaluationActionData)
                        Dim parentNode = app.ActiveProject.HierarchyObjectives.GetNodeByID(pwdata.ParentNode.NodeID)
                        Dim fAlts As Boolean = parentNode.IsTerminalNode
                        Dim ID = 0

                        For Each tJud As clsPairwiseMeasureData In pwdata.Judgments
                            Dim idx = pwdata.Judgments.IndexOf(tJud)
                            If fAlts Then
                                sFirstNode = app.ActiveProject.HierarchyAlternatives.GetNodeByID(tJud.FirstNodeID)
                                sSecondNode = app.ActiveProject.HierarchyAlternatives.GetNodeByID(tJud.SecondNodeID)
                            Else
                                sFirstNode = app.ActiveProject.HierarchyObjectives.GetNodeByID(tJud.FirstNodeID)
                                sSecondNode = app.ActiveProject.HierarchyObjectives.GetNodeByID(tJud.SecondNodeID)
                            End If

                            If is_name Then
                                If sFirstNode.NodeName.ToLower() = firstNode.ToLower() AndAlso sSecondNode.NodeName.ToLower() = secondNode.ToLower() Then
                                    'context.Session("SelectionIDBack") = idx
                                    Return -idx
                                End If

                                If sFirstNode.NodeName.ToLower() = secondNode.ToLower() AndAlso sSecondNode.NodeName.ToLower() = firstNode.ToLower() Then
                                    'context.Session("SelectionIDBack") = idx
                                    Return -idx
                                End If
                            Else
                                If sFirstNode.NodeID.ToString() = firstNode AndAlso sSecondNode.NodeID.ToString() = secondNode Then
                                    'context.Session("SelectionIDBack") = idx
                                    Return -idx
                                End If

                                If sFirstNode.NodeID.ToString() = secondNode AndAlso sSecondNode.NodeID.ToString() = firstNode Then
                                    'context.Session("SelectionIDBack") = idx
                                    Return -idx
                                End If
                            End If
                        Next
                        Exit Select
                End Select

                If is_name Then
                    If sFirstNode.NodeName.ToLower() = firstNode.ToLower() AndAlso sSecondNode.NodeName.ToLower() = secondNode.ToLower() Then
                        'context.Session("SelectionIDBack") = i
                        Return i
                    End If

                    If sFirstNode.NodeName.ToLower() = secondNode.ToLower() AndAlso sSecondNode.NodeName.ToLower() = firstNode.ToLower() Then
                        ''zcontext.Session("SelectionIDBack") = i
                        Return i
                    End If
                Else
                    If sFirstNode.NodeID.ToString() = firstNode AndAlso sSecondNode.NodeID.ToString() = secondNode Then
                        'context.Session("SelectionIDBack") = i
                        Return i
                    End If

                    If sFirstNode.NodeID.ToString() = secondNode AndAlso sSecondNode.NodeID.ToString() = firstNode Then
                        'context.Session("SelectionIDBack") = i
                        Return i
                    End If
                End If
            Next
        End If

        Return current_step
    End Function
    <WebMethod(EnableSession:=True)>
    Public Shared Function SetCollapsedCookies(ByVal node_type As String, ByRef output As AnytimeOutputModel) As Object
        AnytimeClass.set_collapse_cookies(node_type, output)
    End Function

    <WebMethod(EnableSession:=True)>
    Public Shared Function setInfodocParams(ByVal NodeID As String, ByVal WrtNodeID As String, ByVal value As String, ByVal is_multi As Boolean, ByVal NodeType As String) As Object
        Dim context = HttpContext.Current
        Dim isPipeViewOnly = CBool(context.Session(Constants.SessionIsPipeViewOnly))

        If isPipeViewOnly Then
            Return Nothing
        End If

        If WrtNodeID = "" Then
            GeckoClass.SetInfodocParams(Guid.Parse(NodeID), Guid.Empty, value, is_multi)
        Else
            GeckoClass.SetInfodocParams(Guid.Parse(NodeID), Guid.Parse(WrtNodeID), value, is_multi)
        End If
        Dim Index = 0
        Dim pair_index = -1
        Dim pair_indeces = New Object(2) {}
        Dim node_guids = New Object(2) {}
        Dim wrt_guids = New Object(2) {}
        'Dim output = CType(context.Session("output"), AnytimeOutputModel)
        Dim output As AnytimeOutputModel = CType(GetDataOfPipeStep(Convert.ToInt32(context.Session("AT_CurrentStep"))), AnytimeOutputModel)
        If (NodeType = "parent-node") Then
            Index = 0
            node_guids(0) = NodeID
            wrt_guids(0) = WrtNodeID
            pair_indeces(0) = 0
            If (output.page_type <> "atPairwise" And Not is_multi) Then
                node_guids(1) = output.LeftNodeGUID
                wrt_guids(1) = ""
                node_guids(2) = output.LeftNodeGUID
                wrt_guids(2) = output.ParentNodeGUID
                pair_indeces(1) = 1
                pair_indeces(2) = 3
            End If
        ElseIf (NodeType = "left-node") Then
            Index = 1
            output.ParentNodeGUID = output.RightNodeGUID
            output.LeftNodeWrtGUID = output.LeftNodeGUID

            node_guids(0) = output.RightNodeGUID
            wrt_guids(0) = output.LeftNodeGUID
            pair_index = 2
            If Not is_multi Then
                pair_indeces(pair_index) = pair_index
                If (output.page_type <> "atPairwise") Then
                    node_guids(1) = output.ParentNodeGUID
                    wrt_guids(1) = ""

                    node_guids(2) = output.LeftNodeGUID
                    wrt_guids(2) = output.ParentNodeGUID

                    pair_indeces(0) = 0
                    pair_indeces(3) = 3
                End If
            End If
        ElseIf (NodeType = "right-node") Then
            Index = 2
            output.ParentNodeGUID = output.LeftNodeGUID
            output.RightNodeWrtGUID = output.RightNodeGUID
            node_guids(0) = output.LeftNodeGUID
            wrt_guids(0) = output.RightNodeGUID
            pair_index = 1
            If Not is_multi Then
                pair_indeces(pair_index) = pair_index
            End If
        ElseIf (NodeType = "wrt-left-node") Then
            Index = 3
            output.ParentNodeGUID = output.RightNodeGUID
            output.LeftNodeWrtGUID = output.ParentNodeGUID
            node_guids(0) = output.RightNodeGUID
            wrt_guids(0) = output.ParentNodeGUID
            pair_index = 4
            If Not is_multi Then
                pair_indeces(pair_index) = pair_index
                If (output.page_type <> "atPairwise") Then
                    node_guids(1) = output.ParentNodeGUID
                    wrt_guids(1) = ""
                    node_guids(2) = output.LeftNodeGUID
                    wrt_guids(2) = ""
                    pair_indeces(0) = 0
                    pair_indeces(1) = 1
                End If
            End If
        ElseIf (NodeType = "wrt-right-node") Then
            Index = 4
            output.ParentNodeGUID = output.LeftNodeGUID
            output.RightNodeWrtGUID = output.ParentNodeGUID
            node_guids(0) = output.LeftNodeGUID
            wrt_guids(0) = output.ParentNodeGUID
            pair_index = 3
            If Not is_multi Then
                pair_indeces(pair_index) = pair_index
            End If
        End If
        If Not is_multi Then
            output.infodoc_params(Index) = value
        End If
    End Function

    <WebMethod(EnableSession:=True)>
    Public Shared Function saveOBPriority(ByVal chb As Boolean) As Object
        Dim context = HttpContext.Current
        context.Session(InconsistencySortingEnabled) = chb

        If Not chb Then
            context.Session(InconsistencySortingOrder) = New List(Of Integer)()
        End If

        Dim App = AppSession()
        'CType(context.Session("App"), clsComparionCore)
        Dim [step] As Integer = App.ActiveWorkspace.ProjectStep(App.ActiveProject.isImpact)

        Dim CurrentUser As clsApplicationUser = New clsApplicationUser()
        CurrentUser = App.DBUserByID(AnytimeClass.GetReadOnlyUserID())
        Dim AnytimeUser = If(CurrentUser IsNot Nothing, App.ActiveProject.ProjectManager.GetUserByEMail(CurrentUser.UserEmail), Nothing)
        AnytimeClass.SetUser(AnytimeUser, True, True)

        Dim result = AnytimeClass.CreateLocalResults([step])
        context.Session("ObjData") = Nothing
        context.Session("PairsData") = Nothing
        Return result
    End Function

    <WebMethod(EnableSession:=True)>
    Public Shared Function doMatrixOperation(ByVal judgment As String, ByVal ID As Integer, ByVal content As String(), ByVal parent As Integer, ByVal invert As Boolean, ByVal rmode As Integer, ByVal judgementId As String) As Object
        Dim context = HttpContext.Current
        Dim isPipeViewOnly = CBool(context.Session(Constants.SessionIsPipeViewOnly))

        If isPipeViewOnly Then
            Return Nothing
        End If

        Dim App = AppSession()
        'CType(context.Session("App"), clsComparionCore)
        Dim _with1 = App.ActiveProject.ProjectManager
        Dim iStep As Integer = App.ActiveWorkspace.ProjectStep(App.ActiveProject.isImpact)
        Dim ps As clsAction = CType(_with1.Pipe(iStep - 1), clsAction)
        Dim AHPUser As ECTypes.clsUser = _with1.GetUserByEMail(App.ActiveUser.UserEmail)
        Dim psLocal As clsShowLocalResultsActionData = CType(AnytimeClass.GetAction(iStep).ActionData, clsShowLocalResultsActionData)
        Dim AHPUserID As Integer = AHPUser.UserID
        Dim operationID As OperationID = CType(ID, OperationID)
        Dim ParentID As Integer = -1
        AnytimeClass.JudgmentsSaved = True
        Dim Model = CType(context.Session(Constants.SessionModel), DataModel)

        If AnytimeClass.CombinedUserID <> ECTypes.COMBINED_USER_ID Then
            App.ActiveProject.ProjectManager.StorageManager.Reader.LoadUserJudgments(App.ActiveProject.ProjectManager.GetUserByID(AnytimeClass.CombinedUserID))
        End If

        If operationID = OperationID.oJudgmentUpdate Then
            Dim Obj1ID As Integer = Convert.ToInt32(content(2))
            Dim Obj2ID As Integer = Convert.ToInt32(content(3))
            Dim Advantage As Integer = Convert.ToInt32(content(4))

            If invert Then
            End If

            Dim tValue = Convert.ToDouble(judgment)
            ParentID = parent
            Dim node As clsNode = App.ActiveProject.HierarchyObjectives.GetNodeByID(ParentID)

            If node IsNot Nothing Then
                Advantage = If(tValue = 0, 0, (If(Advantage = 0, 1, Advantage)))
                Dim isUndefined = tValue = 0
                Dim pwData = (CType(node.Judgments, clsPairwiseJudgments)).PairwiseJudgment(Obj1ID, Obj2ID, AHPUserID)

                If pwData Is Nothing Then
                    pwData = New clsPairwiseMeasureData(Obj1ID, Obj2ID, Advantage, tValue, ParentID, AHPUserID, isUndefined)
                End If

                pwData.Value = tValue
                pwData.Advantage = Advantage
                pwData.IsUndefined = isUndefined
                App.ActiveProject.ProjectManager.StorageManager.Writer.SaveJudgment(node, pwData)

                For i As Integer = 0 To iStep - 1
                    Dim action = App.ActiveProject.ProjectManager.Pipe(i)

                    Select Case action.ActionType
                        Case ActionType.atAllPairwise
                            Dim pwd2 As clsAllPairwiseEvaluationActionData = CType(action.ActionData, clsAllPairwiseEvaluationActionData)

                            If pwd2.Judgments IsNot Nothing Then
                                Dim selectedJudgment = pwd2.Judgments.FirstOrDefault(Function(j) j.FirstNodeID = Obj1ID AndAlso j.SecondNodeID = Obj2ID AndAlso j.ParentNodeID = ParentID)

                                If selectedJudgment IsNot Nothing Then
                                    selectedJudgment.Value = tValue
                                    selectedJudgment.Advantage = Advantage
                                    selectedJudgment.IsUndefined = isUndefined
                                End If
                            End If

                        Case ActionType.atPairwise
                            Dim pairwiseData = CType(action.ActionData, clsPairwiseMeasureData)

                            If pairwiseData IsNot Nothing AndAlso pairwiseData.FirstNodeID = Obj1ID AndAlso pairwiseData.SecondNodeID = Obj2ID AndAlso pairwiseData.ParentNodeID = ParentID Then
                                pairwiseData.Value = tValue
                                pairwiseData.Advantage = Advantage
                                pairwiseData.IsUndefined = isUndefined
                            End If
                    End Select
                Next
            End If

            If context.Session("PairsData") Is Nothing Then context.Session("PairsData") = Model.StepPairs
            If context.Session("ObjData") Is Nothing Then context.Session("ObjData") = Model.ObjectivesDataSorted
            Dim pairsdata = CType(context.Session("PairsData"), List(Of StepsPairs))
            Dim objsdata = CType(context.Session("ObjData"), List(Of Objective))
            Dim pairResult = ""
            Dim objsResult = ""
            Dim inconsistencyRatio = 0.00
            Dim normalization = CInt(context.Session("normalization"))
            HttpContext.Current.Session(InconsistencySortingEnabled) = False
            Dim results = AnytimeClass.CreateLocalResults(iStep, normalization)
            Dim output = New With {
            Key .results = results,
            Key .ObjID = Obj1ID & "" & Obj2ID
        }
            Return output
        End If

        If operationID = OperationID.oRestoreJudgments Then
            context.Session("ObjData") = Nothing
            context.Session("PairsData") = Nothing
            ParentID = parent
            AnytimeClass.RestoreJudgments(ParentID)
            Dim output = New With {Key .PipeParameters = AnytimeClass.CreateLocalResults(iStep)
        }
            Return output
        End If

        If operationID = OperationID.oInvertCurrentJudgment Then
            ParentID = parent
            Dim node As clsNode = Nothing

            If (CType(ps.ActionData, clsShowLocalResultsActionData)).PWOutcomesNode IsNot Nothing Then
                node = (CType(ps.ActionData, clsShowLocalResultsActionData)).PWOutcomesNode
            Else
                node = App.ActiveProject.HierarchyObjectives.GetNodeByID(ParentID)
            End If

            If node IsNot Nothing Then
                Dim pairwiseMeasureDataList = New List(Of clsPairwiseMeasureData)()

                For Each judgement As clsPairwiseMeasureData In node.Judgments.JudgmentsFromUser(AHPUserID)

                    If Not judgement.IsUndefined Then
                        If Convert.ToString(judgement.FirstNodeID) + Convert.ToString(judgement.SecondNodeID) = judgementId Then
                            judgement.Advantage = -judgement.Advantage
                            pairwiseMeasureDataList.Add(judgement)
                        End If
                    End If
                Next

                App.ActiveProject.ProjectManager.StorageManager.Writer.SaveUserJudgments(AHPUserID)

            End If

            Dim output = New With {Key .PipeParameters = AnytimeClass.CreateLocalResults(iStep)
        }
            Return output
        End If

        If operationID = OperationID.oInvertAllJudgments Then
            context.Session("ObjData") = Nothing
            context.Session("PairsData") = Nothing
            ParentID = parent
            Dim node As clsNode = Nothing

            If (CType(ps.ActionData, clsShowLocalResultsActionData)).PWOutcomesNode IsNot Nothing Then
                node = (CType(ps.ActionData, clsShowLocalResultsActionData)).PWOutcomesNode
            Else
                node = App.ActiveProject.HierarchyObjectives.GetNodeByID(ParentID)
            End If

            If node IsNot Nothing Then
                Dim pairwiseMeasureDataList = New List(Of clsPairwiseMeasureData)()

                If (CType(ps.ActionData, clsShowLocalResultsActionData)).PWOutcomesNode IsNot Nothing Then

                    For Each judgement As clsPairwiseMeasureData In node.PWOutcomesJudgments.JudgmentsFromUser(AHPUserID)

                        If Not judgement.IsUndefined Then
                            judgement.Advantage = -judgement.Advantage
                            pairwiseMeasureDataList.Add(judgement)
                        End If
                    Next
                Else

                    For Each judgement As clsPairwiseMeasureData In node.Judgments.JudgmentsFromUser(AHPUserID)

                        If Not judgement.IsUndefined Then
                            judgement.Advantage = -judgement.Advantage
                            pairwiseMeasureDataList.Add(judgement)
                        End If
                    Next
                End If

                App.ActiveProject.ProjectManager.StorageManager.Writer.SaveUserJudgments(AHPUserID)

                For Each action As clsAction In App.ActiveProject.ProjectManager.Pipe

                    Select Case action.ActionType
                        Case ActionType.atAllPairwise, ActionType.atAllPairwiseOutcomes
                            Dim pwd2 As clsAllPairwiseEvaluationActionData = CType(action.ActionData, clsAllPairwiseEvaluationActionData)

                            If pwd2.Judgments IsNot Nothing Then

                                For Each selectedJudgment In pwd2.Judgments

                                    If Not selectedJudgment.IsUndefined Then
                                        Dim changedJudgment = pairwiseMeasureDataList.FirstOrDefault(Function(j) j.FirstNodeID = selectedJudgment.FirstNodeID AndAlso j.SecondNodeID = selectedJudgment.SecondNodeID AndAlso j.ParentNodeID = selectedJudgment.ParentNodeID AndAlso j.Advantage <> selectedJudgment.Advantage)

                                        If changedJudgment IsNot Nothing Then
                                            selectedJudgment.Advantage = -selectedJudgment.Advantage
                                        End If
                                    End If
                                Next
                            End If
                    End Select
                Next
            End If

            Dim output = New With {Key .PipeParameters = AnytimeClass.CreateLocalResults(iStep)
        }
            Return output
        End If

        Return Nothing
    End Function

    <WebMethod(EnableSession:=True)>
    Public Shared Function pasteClipBoardData(ByVal clipBoardData As String, ByVal sameElements As Boolean) As Object
        Dim output = New Dictionary(Of String, Object)() From {
            {"success", False},
            {"data", ""}
        }
        Dim isPipeViewOnly = CBool(HttpContext.Current.Session(Constants.SessionIsPipeViewOnly))

        If isPipeViewOnly Then
            output = New Dictionary(Of String, Object)() From {
                {"success", True},
                {"data", AnytimeClass.CreateLocalResults(CurrentStep)}
            }
            Return output
        End If

        Dim pass = AnytimeClass.GetResultsPipeStepData(clipBoardData, sameElements)
        AnytimeClass.JudgmentsSaved = True

        If pass Then
            output = New Dictionary(Of String, Object)() From {
                {"success", True},
                {"data", AnytimeClass.CreateLocalResults(CurrentStep)}
            }
        Else
            output("data") = "request failed"
        End If

        Return output
    End Function

    Public Shared ReadOnly Property CurrentStep As Integer
        Get
            Dim context As HttpContext = HttpContext.Current
            Dim App = AppSession()
            'CType(context.Session("App"), clsComparionCore)
            Return App.ActiveWorkspace.ProjectStep(App.ActiveProject.isImpact)
        End Get
    End Property

    <WebMethod(EnableSession:=True)>
    Public Shared Function changeNormalization(ByVal normalization As Integer, ByVal [step] As Integer, ByVal fGlobal As Boolean, ByVal wrtNodeID As Integer) As Object
        If fGlobal Then
            Return AnytimeClass.CreateGlobalResults([step], normalization, wrtNodeID)
        Else
            Return AnytimeClass.CreateLocalResults([step], normalization)
        End If
    End Function

    <WebMethod(EnableSession:=True)>
    Public Shared Function loadHierarchy() As Object
        Dim context As HttpContext = HttpContext.Current
        Dim App = AppSession()
        'CType(context.Session("App"), clsComparionCore)
        Dim hierarchies = GeckoClass.NodeList(App.ActiveProject.HierarchyObjectives.GetLevelNodes(0), AnytimeClass.GetAction(CurrentStep))
        Dim output = New Dictionary(Of String, Object)() From {
            {"success", True},
            {"data", hierarchies}
        }
        Return output
    End Function

    <WebMethod(EnableSession:=True)>
    Public Shared Function saveRespondentAnswers(ByVal [step] As Integer, ByVal RespondentAnswers As String()()) As String
        [step] = If(([step] <> -1), [step], 1)
        Dim context = HttpContext.Current
        Dim isPipeViewOnly = CBool(context.Session(Constants.SessionIsPipeViewOnly))

        If isPipeViewOnly Then
            Return ""
        End If

        Dim app = AppSession()
        'CType(context.Session("App"), clsComparionCore)
        Dim NeedtoRebuildPipe = False
        Dim answers = AnytimeClass.ReadPageAnswers(NeedtoRebuildPipe, [step], RespondentAnswers)

        If NeedtoRebuildPipe Then
            NotRecreatePipe = False
            CheckProject()
            app.ActiveProject.ProjectManager.PipeBuilder.PipeCreated = False

            For Each cg As ECCore.Groups.clsCombinedGroup In app.ActiveProject.ProjectManager.CombinedGroups.GroupsList
                cg.ApplyRules()
            Next

            app.ActiveProject.SaveStructure("Save User Group")
            app.ActiveProject.ProjectManager.PipeBuilder.CreatePipe()
            Return AnytimeClass.get_StepInformation(app)
        End If

        Return answers
    End Function

    <WebMethod(EnableSession:=True)>
    Public Shared Sub SaveInfoDocs(ByVal nodetxt As String, ByVal obj As String, ByVal node As String, ByVal current_step As Integer, ByVal node_id As Integer, ByVal node_guid As String, ByVal Guids As String)
        Dim IsRunTaskNodeGUID As Boolean = False
        If HttpContext.Current.Request.Cookies("IsHeaderNode") IsNot Nothing AndAlso Not String.IsNullOrEmpty(HttpContext.Current.Request.Cookies("IsHeaderNode").Value) Then
            If HttpContext.Current.Request.Cookies("IsHeaderNode").Value.ToString() = "Yes" Then
                IsRunTaskNodeGUID = True
            End If
            Dim IsHeaderNode As New HttpCookie("IsHeaderNode")
            IsHeaderNode.Expires = DateTime.Now.AddDays(-1D)
            HttpContext.Current.Response.Cookies.Add(IsHeaderNode)
        End If

        Dim context As HttpContext = HttpContext.Current
        Dim App = AppSession()
        'CType(context.Session("App"), clsComparionCore)
        Dim CurrentProject = App.ActiveProject
        Dim node_data = CType(GeckoClass.GetInfoDocData(obj, node, current_step, node_id, node_guid), Object(,))
        Dim sBasePath = CStr(node_data(0, 0))
        Dim tempnode = CType(node_data(0, 1), clsNode)
        Dim _ObjectType = CType(node_data(0, 2), Consts.reObjectType)
        Dim ParentNodeID = CInt(node_data(0, 3))
        Dim ObjHierarchy = CType(CurrentProject.HierarchyObjectives, clsHierarchy)
        Dim ParentNode = CType(ObjHierarchy.GetNodeByID(ParentNodeID), clsNode)
        Dim is_CovObj = CBool(node_data(0, 5))
        Dim action_type = CType(node_data(0, 6), Canvas.ActionType)
        Dim is_Multi As Boolean = CBool(node_data(0, 4))
        Dim additionalGuid As Guid = Guid.Empty

        If Guids <> "" OrElse action_type = Canvas.ActionType.atShowLocalResults OrElse action_type = Canvas.ActionType.atShowGlobalResults Then
            If ParentNode Is Nothing AndAlso HttpContext.Current.Session(SessionParentNodeGuid) IsNot Nothing Then
                Try
                    ParentNode = CType(ObjHierarchy.GetNodeByID(CType(HttpContext.Current.Session(SessionParentNodeGuid), Guid)), clsNode)
                Catch ex As Exception
                    ParentNode = Nothing
                End Try
            End If
            If ParentNode IsNot Nothing Then
                Dim tGuids As New List(Of Guid)
                If Guids <> "" Then
                    Dim sList As String() = Guids.Split(CType(",", Char()))
                    If sList.Count > 0 Then
                        For Each sGuid As String In sList
                            Try
                                Dim G As New Guid(sGuid)
                                If tGuids.IndexOf(G) < 0 Then tGuids.Add(G)
                            Catch ex As Exception
                            End Try
                        Next
                    End If
                End If
                If tGuids.IndexOf(ParentNode.NodeGuidID) < 0 Then tGuids.Add(ParentNode.NodeGuidID)

                Dim isMulti As Boolean = False
                Select Case action_type
                    Case ActionType.atAllPairwise, ActionType.atAllPairwiseOutcomes, ActionType.atNonPWAllChildren, ActionType.atNonPWAllCovObjs
                        isMulti = True
                        ' D4053 ===
                    Case ActionType.atShowLocalResults
                        isMulti = True
                        ' D4053 ==
                End Select

                For Each tGuid As Guid In tGuids
                    ' D4053 ===
                    Dim tAGuid As Guid = Guid.Empty
                    Select Case action_type
                        Case ActionType.atShowLocalResults, ActionType.atShowGlobalResults, ActionType.atSensitivityAnalysis
                            tAGuid = tGuid
                    End Select
                    If node = "-1" Then
                        ' D7677 ===
                        Dim tAdditionalGuid As Guid = Guid.Empty
                        ''If IsIntensities AndAlso IntensityScale IsNot Nothing Then tAdditionalGuid = IntensityScale.GuidID
                        App.ActiveProject.ProjectManager.PipeBuilder.SetClusterTitleForNode(tGuid, nodetxt, tAdditionalGuid)
                    Else
                        App.ActiveProject.ProjectManager.PipeBuilder.SetClusterPhraseForNode(tGuid, nodetxt, isMulti, tAGuid, False)
                        ' D7677 ==
                    End If
                    ' D4053 ==
                Next
            End If
        ElseIf obj = "0" Then

            If is_CovObj Then
                TeamTimeClass.SetClusterPhraseForNode(tempnode.NodeGuidID, nodetxt, is_Multi, additionalGuid)
            Else

                If action_type = Canvas.ActionType.atShowLocalResults OrElse action_type = Canvas.ActionType.atShowGlobalResults OrElse action_type = Canvas.ActionType.atSensitivityAnalysis Then
                    additionalGuid = ParentNode.NodeGuidID
                End If
                Dim result As Boolean
                If IsRunTaskNodeGUID Then
                    Dim nodeguid As New Guid(TeamTimeClass.TaskNodeGUID)
                    result = TeamTimeClass.SetClusterPhraseForNode(nodeguid, nodetxt, is_Multi, additionalGuid)
                    If result Then
                        App.SaveProjectLogEvent(App.ActiveProject, "Update custom cluster phrase", False, "")
                    End If
                Else
                    result = TeamTimeClass.SetClusterPhraseForNode(ParentNode.NodeGuidID, nodetxt, is_Multi, additionalGuid)
                    If result AndAlso additionalGuid <> Guid.Empty Then
                        App.SaveProjectLogEvent(App.ActiveProject, "Update custom cluster phrase", False, "")
                    End If
                End If
            End If
        Else

            Try
                Dim sInfoDoc = InfodocService.Infodoc_Pack(nodetxt, Consts._FILE_ROOT, sBasePath)

                If _ObjectType = Consts.reObjectType.AltWRTNode Then
                    CurrentProject.ProjectManager.InfoDocs.SetNodeWRTInfoDoc(tempnode.NodeGuidID, ParentNode.NodeGuidID, sInfoDoc)
                ElseIf _ObjectType = Consts.reObjectType.MeasureScale Then
                    tempnode.MeasurementScale.Comment = InfodocService.Infodoc_Pack(nodetxt, Consts._FILE_ROOT, sBasePath)
                Else
                    tempnode.InfoDoc = InfodocService.Infodoc_Pack(nodetxt, Consts._FILE_ROOT, sBasePath)
                End If

                If _ObjectType = Consts.reObjectType.MeasureScale Then
                    CurrentProject.ProjectManager.StorageManager.Writer.SaveModelStructure()
                Else
                    CurrentProject.ProjectManager.StorageManager.Writer.SaveInfoDocs()
                End If

            Catch e As Exception
                Dim test = e
            End Try
        End If
    End Sub

    Public Overloads Function CheckVar(ByVal sVarName As String, ByVal DefValue As String) As String
        Dim Res As String = DefValue
        If sVarName IsNot Nothing AndAlso Request(sVarName) IsNot Nothing Then Res = RemoveBadTags(CStr(Request(sVarName))) ' D1800 + D5039
        Return Res
    End Function

    '<WebMethod(EnableSession:=True)>
    'Public Shared Sub SaveMultiPairwiseData(ByVal multivalues As String)
    '    Dim str As String = "abc"
    'End Sub
    <WebMethod(EnableSession:=True)>
    Public Shared Sub SaveMultiPairwiseData(ByVal [step] As Integer, ByVal multivalues As Object()())
        Try
            Dim context = HttpContext.Current
            Dim isPipeViewOnly = CBool(context.Session(Constants.SessionIsPipeViewOnly))

            If isPipeViewOnly Then
                Return
            End If

            Dim App = AppSession()
            'CType(context.Session("App"), clsComparionCore)
            Dim Action = CType(AnytimeClass.GetAction([step]), clsAction)
            Dim data As clsAllPairwiseEvaluationActionData = CType(Action.ActionData, clsAllPairwiseEvaluationActionData)
            Dim Value As Double = 0

            For ID As Integer = 0 To data.Judgments.Count - 1
                Dim sIDLeft As String = data.Judgments(ID).FirstNodeID.ToString()
                Dim sIDRight As String = data.Judgments(ID).SecondNodeID.ToString()

                For Each tJud As clsPairwiseMeasureData In data.Judgments

                    If tJud.FirstNodeID.ToString() = sIDLeft AndAlso tJud.SecondNodeID.ToString() = sIDRight Then
                        Dim fUpdate As Boolean = False
                        Dim sValue As String = multivalues(ID)(0).ToString()
                        Dim sAdv As String = multivalues(ID)(1).ToString()

                        If String.IsNullOrEmpty(sValue) Then
                            fUpdate = Not tJud.IsUndefined
                            tJud.IsUndefined = True
                        Else
                            Value = Convert.ToDouble(sValue)

                            If Math.Abs(Value) >= Math.Abs(ECCore.TeamTimeFuncs.TeamTimeFuncs.UndefinedValue) Then

                                If Not tJud.IsUndefined Then
                                    tJud.IsUndefined = True
                                    fUpdate = True
                                End If
                            Else
                                Dim Adv As Integer = 0

                                If Integer.TryParse(sAdv, Adv) AndAlso (tJud.IsUndefined OrElse tJud.Value.ToString("F6") <> Value.ToString("F6") OrElse tJud.Advantage <> Adv) Then
                                    tJud.IsUndefined = False
                                    tJud.Value = Math.Abs(Value)
                                    tJud.Advantage = Adv
                                End If

                                fUpdate = True
                            End If
                        End If

                        Dim sComment As String = multivalues(ID)(2).ToString()

                        If sComment <> tJud.Comment Then
                            tJud.Comment = sComment
                            fUpdate = True
                        End If

                        If fUpdate Then

                            If Action.ActionType = ActionType.atAllPairwiseOutcomes Then

                                If Action.PWONode IsNot Nothing Then
                                    Action.PWONode.PWOutcomesJudgments.AddMeasureData(tJud)
                                End If

                                If Action.PWONode IsNot Nothing AndAlso Action.ParentNode IsNot Nothing Then
                                    Action.ParentNode.PWOutcomesJudgments.AddMeasureData(tJud)
                                End If

                                App.ActiveProject.ProjectManager.StorageManager.Writer.SaveJudgment(Action.PWONode, tJud)
                            Else
                                App.ActiveProject.HierarchyObjectives.GetNodeByID(data.ParentNode.NodeID).Judgments.AddMeasureData(tJud)
                                App.ActiveProject.ProjectManager.StorageManager.Writer.SaveJudgment(data.ParentNode, tJud)
                            End If
                        End If

                        Exit For
                    End If
                Next
            Next
        Catch ex As Exception
            Console.WriteLine("error : save multipairwise")
        End Try

    End Sub

    <WebMethod(EnableSession:=True)>
    Public Shared Function SavePairwise(ByVal [step] As Integer, ByVal value As String, ByVal advantage As String, ByVal comments As String, ByVal userId As Integer) As String
        Dim fChanged = False
        Dim context = HttpContext.Current
        Dim isPipeViewOnly = CBool(context.Session(Constants.SessionIsPipeViewOnly))

        If isPipeViewOnly Then
            Return ""
        End If

        Dim app = AppSession()
        'CType(context.Session("App"), clsComparionCore)

        If app IsNot Nothing Then

            Try
                Dim PWsteps = If(context.Session("PWsteps") IsNot Nothing, CType(context.Session("PWsteps"), List(Of clsAction)), Nothing)
                If PWsteps IsNot Nothing AndAlso Not app.ActiveProject.Pipe.Count() = PWsteps.Count() Then
                    app.ActiveProject.Pipe.Clear()
                    For Each stp In PWsteps
                        app.ActiveProject.Pipe.Add(stp)
                    Next
                End If


                Dim action = AnytimeClass.GetAction([step])
                Dim pwData = CType(action.ActionData, clsPairwiseMeasureData)
                Dim parentNode = app.ActiveProject.HierarchyObjectives.GetNodeByID(pwData.ParentNodeID)
                Dim pwType = app.ActiveProject.ProjectManager.PipeBuilder.GetPairwiseTypeForNode(parentNode)

                If context.Request.Cookies(Constants.Cook_Extreme) Is Nothing AndAlso pwType = CanvasTypes.PairwiseType.ptVerbal Then

                    If Convert.ToInt64(value) = 9 Then
                        context.Response.Cookies(Constants.Cook_Extreme).Expires = DateTime.Now.AddDays(1)
                        context.Response.Cookies(Constants.Cook_Extreme).Value = "1"
                        PipeWarning = TeamTimeClass.ResString("msgPWExtreme")
                    End If
                End If

                If userId <> 0 Then
                    Dim actionPwData = CType(action.ActionData, clsPairwiseMeasureData)
                    Dim actionParentNode = action.ParentNode
                    pwData = (CType(actionParentNode.Judgments, clsPairwiseJudgments)).PairwiseJudgment(actionPwData.FirstNodeID, actionPwData.SecondNodeID, userId)

                    If pwData Is Nothing Then
                        pwData = New clsPairwiseMeasureData(actionPwData.FirstNodeID, actionPwData.SecondNodeID, 0, 0, actionParentNode.NodeID, userId, True)
                    End If
                End If

                If value <> "" Then
                    Dim val As Double = 0
                    Dim adv As Integer = 0

                    If StringFuncs.String2Double(value, val) AndAlso Integer.TryParse(advantage, adv) Then

                        If val = -2147483648000 Then

                            If Not pwData.IsUndefined Then
                                pwData.IsUndefined = True
                                fChanged = True
                            End If
                        Else

                            If val = 0 OrElse adv = 0 Then
                                val = 1
                                adv = 0
                            End If

                            pwData.IsUndefined = False

                            If pwData.Value <> val OrElse pwData.Advantage <> adv Then
                                pwData.Value = val
                                pwData.Advantage = adv
                                fChanged = True
                            End If
                        End If
                    End If
                End If

                If app.ActiveProject.PipeParameters.ShowComments AndAlso comments IsNot Nothing Then

                    If comments <> pwData.Comment Then
                        pwData.Comment = comments
                        fChanged = True
                    End If
                End If

                If fChanged Then

                    If action.ActionType = ActionType.atPairwiseOutcomes Then
                        action.ParentNode.PWOutcomesJudgments.AddMeasureData(pwData)
                        app.ActiveProject.ProjectManager.StorageManager.Writer.SaveJudgment(action.PWONode, pwData)
                    Else
                        Dim tNode = app.ActiveProject.HierarchyObjectives.GetNodeByID(pwData.ParentNodeID)
                        tNode.Judgments.AddMeasureData(pwData)
                        app.ActiveProject.ProjectManager.StorageManager.Writer.SaveJudgment(tNode, pwData)
                    End If

                    Dim isAlreadySaved = CBool(context.Session(SessionIsJudgmentAlreadySaved))

                    If pwType = CanvasTypes.PairwiseType.ptVerbal AndAlso Not isAlreadySaved Then
                        context.Session(SessionIncreaseJudgmentsCount) = True
                        context.Session(SessionIsJudgmentAlreadySaved) = True
                    End If
                End If

            Catch e As Exception
                Dim [error] = e
            End Try
        End If

        Return PipeWarning
    End Function

    <WebMethod(EnableSession:=True)>
    Public Shared Function SaveViewAsEvaulator(ByVal ViewAsEvaulator As Boolean) As Boolean
        HttpContext.Current.Session("ViewAsEvaulator") = ViewAsEvaulator
        Return True
    End Function

    <WebMethod(EnableSession:=True)>
    Public Shared Function GetViewAsEvaulator() As Boolean
        If HttpContext.Current.Session("ViewAsEvaulator") IsNot Nothing Then
            Return CBool(HttpContext.Current.Session("ViewAsEvaulator"))
        End If

        Return False
    End Function

    <WebMethod(EnableSession:=True)>
    Public Shared Sub SaveUtilityCurve(ByVal [step] As Integer, ByVal value As String, ByVal sComment As String, ByVal uctype As String)
        Dim context = HttpContext.Current
        Dim isPipeViewOnly = CBool(context.Session(Constants.SessionIsPipeViewOnly))

        If isPipeViewOnly Then
            Return
        End If

        Dim app = AppSession()
        'CType(context.Session("App"), clsComparionCore)
        Dim action = AnytimeClass.GetAction([step])

        If uctype = "Step" Then
            Dim judgment = CType(CType(action.ActionData, clsOneAtATimeEvaluationActionData).Judgment, clsStepMeasureData)
            value = If(value = "-2147483648000", "", value)

            If sComment <> judgment.Comment OrElse value <> judgment.SingleValue.ToString() OrElse (value <> "" AndAlso judgment.IsUndefined) Then
                judgment.Comment = sComment

                If value = "" Then
                    judgment.IsUndefined = True
                Else
                    Dim val As Double = 0

                    If StringFuncs.String2Double(value, val) Then
                        judgment.ObjectValue = val
                        judgment.IsUndefined = False
                    End If
                End If
            End If

            CType(action.ActionData, clsOneAtATimeEvaluationActionData).Node.Judgments.AddMeasureData(judgment)
            app.ActiveProject.ProjectManager.StorageManager.Writer.SaveJudgment(app.ActiveProject.HierarchyObjectives.GetNodeByID(judgment.ParentNodeID), judgment)
        Else
            Dim judgment = CType((CType(action.ActionData, clsOneAtATimeEvaluationActionData)).Judgment, clsUtilityCurveMeasureData)

            value = If(value = "-2147483648000", "", value)

            If sComment <> judgment.Comment OrElse value <> judgment.SingleValue.ToString() OrElse (value <> "" AndAlso judgment.IsUndefined) Then
                judgment.Comment = sComment

                If value = "" Then
                    judgment.IsUndefined = True
                Else
                    Dim val As Double = 0

                    If StringFuncs.String2Double(value, val) Then
                        judgment.ObjectValue = val
                        judgment.IsUndefined = False
                    End If
                End If
            End If

            CType(action.ActionData, clsOneAtATimeEvaluationActionData).Node.Judgments.AddMeasureData(judgment)
            app.ActiveProject.ProjectManager.StorageManager.Writer.SaveJudgment(app.ActiveProject.HierarchyObjectives.GetNodeByID(judgment.ParentNodeID), judgment)
        End If


    End Sub

    <WebMethod(EnableSession:=True)>
    Public Shared Sub SaveMultiRatingsData(ByVal [step] As Integer, ByVal multivalues As Object()(), ByVal intensities As List(Of List(Of String())))
        Dim context = HttpContext.Current
        Dim isPipeViewOnly = CBool(context.Session(Constants.SessionIsPipeViewOnly))

        If isPipeViewOnly Then
            Return
        End If

        Dim app = AppSession()
        'CType(context.Session("App"), clsComparionCore)
        Dim action = AnytimeClass.GetAction([step])

        If TypeOf action.ActionData Is clsAllChildrenEvaluationActionData Then
            Dim data = CType(action.ActionData, clsAllChildrenEvaluationActionData)
            Dim alt As clsNode
            Dim ratingID As Integer
            Dim ratingDirect As Double

            For i = 0 To data.Children.Count - 1
                ratingID = Convert.ToInt16(multivalues(i)(0))
                ratingDirect = Convert.ToDouble(multivalues(i)(1))
                alt = data.Children(i)
                Dim ratings = app.ActiveProject.ProjectManager.MeasureScales.GetRatingScaleByID(data.ParentNode.RatingScaleID())

                If ratings IsNot Nothing Then
                    Dim altRating = ratings.GetRatingByID(ratingID)

                    If altRating Is Nothing AndAlso ratingDirect >= 0 AndAlso ratingDirect <= 1 Then
                        altRating = New clsRating(-1, "Direct input from EC Core", CSng(ratingDirect), Nothing)
                        'If ratingDirect = 0 Then
                        '    altRating = New clsRating(0, "Direct input from EC Core", CSng(ratingDirect), Nothing)
                        'Else
                        '    altRating = New clsRating(-1, "Direct input from EC Core", CSng(ratingDirect), Nothing)
                        'End If

                    End If

                    Dim R = CType(data.GetJudgment(alt), clsRatingMeasureData)

                    If app.ActiveProject.PipeParameters.ShowComments AndAlso R IsNot Nothing Then
                        Dim sComment = multivalues(i)(2).ToString()
                        data.SetData(alt, altRating, sComment)
                    Else
                        data.SetData(alt, altRating)
                    End If

                    If data.ParentNode.IsAlternative Then
                        app.ActiveProject.HierarchyAlternatives.GetNodeByID(data.ParentNode.NodeID).Judgments.AddMeasureData(data.GetJudgment(alt))
                    Else
                        app.ActiveProject.HierarchyObjectives.GetNodeByID(data.ParentNode.NodeID).Judgments.AddMeasureData(data.GetJudgment(alt))
                    End If

                    app.ActiveProject.ProjectManager.StorageManager.Writer.SaveJudgment(data.ParentNode, data.GetJudgment(alt))
                End If
            Next
        End If

        If TypeOf action.ActionData Is clsAllCoveringObjectivesEvaluationActionData Then
            Dim data = CType(action.ActionData, clsAllCoveringObjectivesEvaluationActionData)
            Dim covObj As clsNode
            Dim ratingID As Integer
            Dim ratingDirect As Double

            For i As Integer = 0 To data.CoveringObjectives.Count - 1
                ratingID = Convert.ToInt16(multivalues(i)(0))
                ratingDirect = Convert.ToDouble(multivalues(i)(1))
                covObj = data.CoveringObjectives(i)
                Dim ratings = app.ActiveProject.ProjectManager.MeasureScales.GetRatingScaleByID(covObj.RatingScaleID())

                If ratings IsNot Nothing Then
                    Dim altRating = ratings.GetRatingByID(ratingID)

                    If altRating Is Nothing AndAlso ratingDirect >= 0 AndAlso ratingDirect <= 1 Then
                        altRating = New clsRating(-1, "Direct input from EC Core", CSng(ratingDirect), Nothing)
                    End If

                    Dim R = CType(data.GetJudgment(covObj), clsRatingMeasureData)

                    If app.ActiveProject.PipeParameters.ShowComments AndAlso R IsNot Nothing Then
                        Dim sComment = multivalues(i)(2).ToString()
                        data.SetData(covObj, altRating, sComment)
                    Else
                        data.SetData(covObj, altRating)
                    End If

                    app.ActiveProject.HierarchyObjectives.GetNodeByID(covObj.NodeID).Judgments.AddMeasureData(data.GetJudgment(covObj))
                    app.ActiveProject.ProjectManager.StorageManager.Writer.SaveJudgment(covObj, data.GetJudgment(covObj))
                End If
            Next
        End If

        If TypeOf action.ActionData Is clsAllEventsWithNoSourceEvaluationActionData Then
            Dim data = CType(action.ActionData, clsAllEventsWithNoSourceEvaluationActionData)
            Dim alt As clsNode
            Dim ratingID As Integer
            Dim ratingDirect As Double

            For i As Integer = 0 To data.Alternatives.Count - 1
                ratingID = Convert.ToInt16(multivalues(i)(0))
                ratingDirect = Convert.ToDouble(multivalues(i)(1))
                alt = data.Alternatives(i)
                Dim ratings = app.ActiveProject.ProjectManager.MeasureScales.GetRatingScaleByID(alt.RatingScaleID())

                If ratings IsNot Nothing Then
                    Dim altRating = ratings.GetRatingByID(ratingID)

                    If altRating Is Nothing AndAlso ratingDirect >= 0 AndAlso ratingDirect <= 1 Then
                        altRating = New clsRating(-1, "Direct input from EC Core", CSng(ratingDirect), Nothing)
                    End If

                    Dim R = CType(data.GetJudgment(alt), clsRatingMeasureData)

                    If app.ActiveProject.PipeParameters.ShowComments AndAlso R IsNot Nothing Then
                        Dim sComment = multivalues(i)(2).ToString()
                        data.SetData(alt, altRating, sComment)
                    Else
                        data.SetData(alt, altRating)
                    End If

                    app.ActiveProject.HierarchyAlternatives.GetNodeByID(alt.NodeID).DirectJudgmentsForNoCause.AddMeasureData(data.GetJudgment(alt))
                    app.ActiveProject.ProjectManager.StorageManager.Writer.SaveJudgment(alt, data.GetJudgment(alt))
                End If
            Next
        End If

        Dim isPm = app.CanUserModifyProject(app.ActiveUser.UserID, app.ProjectID, Uw, Ws, app.ActiveWorkgroup)

        If isPm AndAlso app.ActiveProject.ProjectManager.MeasureScales.RatingsScales IsNot Nothing Then
            Dim isChanged = False

            For Each mScale In app.ActiveProject.ProjectManager.MeasureScales.RatingsScales

                For Each intensity In (CType(mScale, clsRatingScale)).RatingSet

                    For Each savedIntensity In intensities
                        Dim modifiedIntensity As String() = savedIntensity.FirstOrDefault(Function(ir) ir(0) = intensity.GuidID.ToString())

                        If modifiedIntensity IsNot Nothing AndAlso intensity.Comment <> modifiedIntensity(1) Then
                            intensity.Comment = modifiedIntensity(1)
                            isChanged = True
                            Exit For
                        End If
                    Next
                Next
            Next

            If isChanged Then
                app.ActiveProject.ProjectManager.StorageManager.Writer.SaveModelStructure()
            End If
        End If
    End Sub


    <WebMethod(EnableSession:=True)>
    Public Shared Sub SaveMultiDirectData(ByVal [step] As Integer, ByVal multivalues As Object()())
        Dim context = HttpContext.Current
        Dim isPipeViewOnly = CBool(context.Session(Constants.SessionIsPipeViewOnly))

        If isPipeViewOnly Then
            Return
        End If

        Dim app = AppSession()
        'CType(context.Session("App"), clsComparionCore)
        Dim action = AnytimeClass.GetAction([step])

        If TypeOf action.ActionData Is clsAllChildrenEvaluationActionData Then
            Dim data = CType(action.ActionData, clsAllChildrenEvaluationActionData)
            Dim value As Double = -1

            For i As Integer = 0 To data.Children.Count - 1
                Dim tNode = data.Children(i)
                Dim directData = CType(data.GetJudgment(tNode), clsDirectMeasureData)
                Dim sValue = multivalues(i)(0).ToString()

                If sValue = "" Then
                    directData.IsUndefined = True
                Else

                    If StringFuncs.String2Double(sValue, value) Then
                        directData.ObjectValue = value
                        directData.IsUndefined = False
                    End If
                End If

                Dim sComment = multivalues(i)(1).ToString()

                If sComment <> directData.Comment Then
                    directData.Comment = sComment
                End If

                CType(action.ActionData, clsAllChildrenEvaluationActionData).ParentNode.Judgments.AddMeasureData(data.GetJudgment(tNode))
                app.ActiveProject.ProjectManager.StorageManager.Writer.SaveJudgment(data.ParentNode, directData)
            Next
        End If

        If TypeOf action.ActionData Is clsAllCoveringObjectivesEvaluationActionData Then
            Dim data = CType(action.ActionData, clsAllCoveringObjectivesEvaluationActionData)
            Dim value As Double = -1

            For i As Integer = 0 To data.CoveringObjectives.Count - 1
                Dim tNode = data.CoveringObjectives(i)
                Dim directData = CType(data.GetJudgment(tNode), clsDirectMeasureData)
                Dim sValue = multivalues(i)(0).ToString()

                If sValue = "" Then
                    directData.IsUndefined = True
                Else

                    If StringFuncs.String2Double(sValue, value) Then
                        directData.ObjectValue = value
                        directData.IsUndefined = False
                    End If
                End If

                Dim sComment = multivalues(i)(1).ToString()

                If sComment <> directData.Comment Then
                    directData.Comment = sComment
                End If

                app.ActiveProject.HierarchyObjectives.GetNodeByID(tNode.NodeID).Judgments.AddMeasureData(directData)
                app.ActiveProject.ProjectManager.StorageManager.Writer.SaveJudgment(data.Alternative, directData)
            Next
        End If

        If TypeOf action.ActionData Is clsAllEventsWithNoSourceEvaluationActionData Then
            Dim data = CType(action.ActionData, clsAllEventsWithNoSourceEvaluationActionData)
            Dim value As Double = -1

            For i As Integer = 0 To data.Alternatives.Count - 1
                Dim tNode = data.Alternatives(i)
                Dim directData = CType(data.GetJudgment(tNode), clsDirectMeasureData)
                Dim sValue = multivalues(i)(0).ToString()

                If sValue = "" Then
                    directData.IsUndefined = True
                Else

                    If StringFuncs.String2Double(sValue, value) Then
                        directData.ObjectValue = value
                        directData.IsUndefined = False
                    End If
                End If

                Dim sComment = multivalues(i)(1).ToString()

                If sComment <> directData.Comment Then
                    directData.Comment = sComment
                End If

                app.ActiveProject.HierarchyAlternatives.GetNodeByID(tNode.NodeID).DirectJudgmentsForNoCause.AddMeasureData(directData)
                app.ActiveProject.ProjectManager.StorageManager.Writer.SaveJudgment(tNode, directData)
            Next
        End If
    End Sub

    <WebMethod(EnableSession:=True)>
    Public Shared Sub setQuickHelpInfo(ByVal tNodeID As Integer, ByVal tEvalStep As PipeParameters.ecEvaluationStepType, ByVal [step] As Integer, ByVal actual_step As Integer, ByVal message As String, ByVal show_qh_automatically As Boolean)
        Dim context = HttpContext.Current
        Dim isPipeViewOnly = CBool(context.Session(Constants.SessionIsPipeViewOnly))
        'Dim regexx As String = "(?:.+?)?(?:\/v\/|watch\/|\?v=|\&v=|youtu\.be\/|\/v=|^youtu\.be\/)([a-zA-Z0-9_-])+"
        'Dim videoUrl As String = ""
        'Dim matching As MatchCollection = Regex.Matches(message, regexx)
        'Dim m As Match

        'For Each m In matching
        '    videoUrl = videoUrl + m.ToString().Replace("<p>", "")
        '    videoUrl = "<iframe width='100%' height='400px' src=" + videoUrl.Replace("watch?v=", "embed/") + "?autoplay=1&mute=1 allowfullscreen></iframe>"
        '    message = message.Replace(m.ToString(), videoUrl)
        'Next m

        If isPipeViewOnly Then
            Return
        End If

        Dim App = AppSession()
        'CType(context.Session("App"), clsComparionCore)

        If App IsNot Nothing Then
            Dim isPM As Boolean = App.CanUserModifyProject(App.ActiveUser.UserID, App.ProjectID, Uw, Ws, App.ActiveWorkgroup)

            If isPM Then
                Dim AutoShow = show_qh_automatically
                Dim nodeGuid As Guid = If(HttpContext.Current.Session(SessionParentNodeGuid) Is Nothing, New Guid(), CType(HttpContext.Current.Session(SessionParentNodeGuid), Guid))
                Dim node As clsNode = AnytimeClass.GetNodeByGuid(nodeGuid)
                Dim objectId As String = InfodocService.GetQuickHelpObjectID(tEvalStep, node)
                Dim basePath As String = InfodocService.Infodoc_Path(App.ActiveProject.ID, App.ActiveProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.QuickHelp, objectId, -1)
                Dim baseUrl = $"{HttpContext.Current.Request.Url.Scheme}://{HttpContext.Current.Request.Url.Host}"
                Dim infoDocQh As String = InfodocService.Infodoc_Pack(message, baseUrl, basePath)
                App.ActiveProject.ProjectManager.PipeParameters.PipeMessages.SetEvaluationQuickHelpText(App.ActiveProject.ProjectManager, [step], False, AutoShow, infoDocQh)
                Dim snapshotComment As String = AnytimeClass.GetNodeTypeAndName(node)
                snapshotComment = String.Format("{0} {1}", tEvalStep.ToString(), snapshotComment).Trim()
                Dim fUpdated As Boolean = App.ActiveProject.PipeParameters.PipeMessages.Save(PipeParameters.PipeStorageType.pstStreamsDatabase, App.ActiveProject.ConnectionString, App.ActiveProject.ProviderType, App.ActiveProject.ID)

                If fUpdated Then
                    App.SaveProjectLogEvent(App.ActiveProject, "Edit quick help", False, snapshotComment)
                End If
            End If
        End If
    End Sub

    <WebMethod(EnableSession:=True)>
    Public Shared Sub setInfodocMode(ByVal value As String)
        Dim context = HttpContext.Current
        Dim isPipeViewOnly = CBool(context.Session(Constants.SessionIsPipeViewOnly))

        If isPipeViewOnly Then
            Return
        End If

        Dim App = AppSession()
        'CType(context.Session("App"), clsComparionCore)

        If App IsNot Nothing Then
            Dim isPM As Boolean = App.CanUserModifyProject(App.ActiveUser.UserID, App.ProjectID, Uw, Ws, App.ActiveWorkgroup)

            If isPM Then

                If value = "1" Then
                    App.ActiveProject.PipeParameters.ShowInfoDocsMode = CanvasTypes.ShowInfoDocsMode.sidmPopup
                Else
                    App.ActiveProject.PipeParameters.ShowInfoDocsMode = CanvasTypes.ShowInfoDocsMode.sidmFrame
                End If

                App.ActiveProject.SaveProjectOptions("Save infodoc mode")
            End If

            HttpContext.Current.Session("InfodocMode") = value
        End If
    End Sub

    <WebMethod(EnableSession:=True)>
    Public Shared Sub setQHSettingCookies(ByVal status As Boolean)
        HttpContext.Current.Response.Cookies("Dont_Show_QH").Expires = DateTime.Now.AddDays(10)
        HttpContext.Current.Response.Cookies("Dont_Show_QH").Value = status.ToString()
    End Sub

    <WebMethod(EnableSession:=True)>
    Public Shared Sub setQuickHelpInfoByCluster(ByVal nodes As List(Of String()), ByVal qh_info As String, ByVal show_qh_automatically As Boolean)
        Dim context = HttpContext.Current
        Dim isPipeViewOnly = CBool(context.Session(Constants.SessionIsPipeViewOnly))

        If isPipeViewOnly Then
            Return
        End If

        Dim App = AppSession()
        'CType(context.Session("App"), clsComparionCore)
        Dim isPM As Boolean = App.CanUserModifyProject(App.ActiveUser.UserID, App.ProjectID, Uw, Ws, App.ActiveWorkgroup)

        For Each node As String() In nodes
            Dim [step] = Int16.Parse(node(1))

            If App IsNot Nothing Then

                If isPM Then
                    Dim AutoShow = show_qh_automatically
                    App.ActiveProject.ProjectManager.PipeParameters.PipeMessages.SetEvaluationQuickHelpText(App.ActiveProject.ProjectManager, [step], True, AutoShow, qh_info)
                    Dim fUpdated As Boolean = App.ActiveProject.PipeParameters.PipeMessages.Save(PipeParameters.PipeStorageType.pstStreamsDatabase, App.ActiveProject.ConnectionString, App.ActiveProject.ProviderType, App.ActiveProject.ID)

                    If fUpdated Then
                        App.SaveProjectLogEvent(App.ActiveProject, "Edit quick help", False, "Edit QuickHelp")
                    End If
                End If
            End If
        Next
    End Sub

    <WebMethod(EnableSession:=True)>
    Public Shared Sub ApplyCustomWordingToNodes(ByVal node_ids As List(Of Integer), ByVal custom_word As String, ByVal is_multi As Boolean, ByVal is_evaluation As Boolean)
        Dim context = HttpContext.Current
        Dim App = AppSession()
        'CType(HttpContext.Current.Session("App"), clsComparionCore)
        Dim ObjHierarchy = CType(App.ActiveProject.HierarchyObjectives, clsHierarchy)

        For Each node_id As Integer In node_ids
            Dim node = CType(ObjHierarchy.GetNodeByID(node_id), clsNode)
            Dim tAdditionGUID = Guid.Empty

            If Not is_evaluation Then
                tAdditionGUID = node.NodeGuidID
            End If

            TeamTimeClass.SetClusterPhraseForNode(node.NodeGuidID, custom_word, is_multi, tAdditionGUID)
        Next
    End Sub
    Public Sub MakeVariableList()   ' D4329
        Dim Tpls As New Dictionary(Of String, String)
        For Each tKey As KeyValuePair(Of String, String) In TeamTimeClass.TaskTemplates
            Tpls.Add(tKey.Key, tKey.Value)
        Next
        Dim sAllTpl As String = ""
        ' D4352 ===
        Dim Lst As String() = _TEMPL_LIST_EVALS
        For Each sTpl As String In Lst
            ' D4352 ==
            sAllTpl += sTpl + vbCrLf
        Next
        Dim ParsedTpl As String() = TeamTimeClass.PrepareTask(sAllTpl).Split(CType(vbCrLf, Char()))
        ' D4352 ===
        Dim tList As Dictionary(Of String, String) = TeamTimeClass.GetUserTemplateReplacements(True)
        For i As Integer = 0 To ParsedTpl.Length - 1
            If ParsedTpl(i) <> "" AndAlso i < Lst.Length Then
                Dim sName As String = Lst(i)
                If tList.Keys.Contains(sName) Then sName = tList(sName)
                Tpls.Add(sName, ParsedTpl(i))
            End If
            ' D4352 ==
        Next

        Dim html As StringBuilder = New StringBuilder()
        If Tpls.Count = 0 Then
            html.Append("<div style='padding:3em'><i>no items</i></div>")
        Else
            Dim idx As Integer = 0
            For Each sKey As String In Tpls.Keys.OrderBy(Of String)(Function(x) x)
                'sTooltip += String.Format("<li{4}><a href='' onclick='return InsertTemplate({3}, ""{2}"")' class='actions'><span id='_tpl_{5}'>{0}</a>: <span id='_val_{5}'>{1}</span>{6}</li>", sKey, If(sKey = _TEMPL_EVAL_OBJECT, Tpls(sKey), SafeFormString(ShortString(HTML2Text(Tpls(sKey)), 50, True))), JS_SafeString(sKey), 0, If(sKey = _TEMPL_EVAL_OBJECT, " id='tplEvalObject' style='display:none; margin:2px 0px;'", " style='margin:2px 0px;'"), idx, If(sKey = _TEMPL_EVAL_OBJECT, " <i>[Focused element]</i>", "")) ' D2830 + D4327 + D4329 + D6960 + D6961
                html.Append($"<li><a id='InsertlnkID{idx}' onclick='InsertText({idx})'>{sKey.Replace("%%", "")}</a>: <span id='_val_{idx}'>{If(sKey = _TEMPL_EVAL_OBJECT, Tpls(sKey), SafeFormString(ShortString(HTML2Text(Tpls(sKey)), 50, True)))}</span></li>")
                idx += 1  ' D6961
            Next
            Dim CVariable As New HttpCookie("CVariable")
            CVariable.Values("CVariable") = html.ToString()
            If (CVariable.Value.Contains("CVariable")) Then
                CVariable.Value = CVariable.Value.Substring(10)
            End If
            Response.Cookies.Add(CVariable)
        End If

        Dim sParam As String = EcSanitizer.GetSafeHtmlFragment("welcome")
        Dim comparionCorePage As clsComparionCorePage
        Dim sTooltip As StringBuilder = New StringBuilder()
        sTooltip.Append($"<ul type=square style='margin:0px 1em 1ex 0px'><b>{TeamTimeClass.ResString("lblEmailVariables")}</b>")
        Dim tCustom As String() = {_TEMPL_APPNAME, _TEMPL_PRJNAME, _TEMPL_PRJPASSCODE, _TEMPL_MEETING_ID, _TEMPL_URL_APP,
                                               _TEMPL_URL_LOGIN, _TEMPL_URL_EVALUATE, _TEMPL_URL_EVALUATE_TT, _TEMPL_URL_MEETINGID,
                                               _TEMPL_URL_EVALUATE_ANONYM, _TEMPL_URL_EVALUATE_SIGNUP, _TEMPL_URL_RESETPSW, _TEMPL_URL_EVALUATE_SIGNUP_EMAILONLY,
                                               _TEMPL_URL_EVALUATE_SIGNUP_EMAILPSW, _TEMPL_URL_EVALUATE_SIGNUP_NAMEONLY, _TEMPL_SERVICE_EMAIL}

        Dim App = AppSession()
        Lst = _TEMPL_LIST_ALL(App.isRiskEnabled)
        For i As Integer = 0 To Lst.Length - 1   ' D2467
            If (Lst(i) <> _TEMPL_MEETING_ID OrElse _MEETING_ID_AVAILABLE) AndAlso (tCustom.Contains(Lst(i)) OrElse sParam.ToLower <> _PARAM_INVITATION_CUSTOM.ToLower) Then    ' D0395 + D4650
                If Lst(i) = _TEMPL_URL_APP OrElse Lst(i) = _TEMPL_URL_MEETINGID OrElse Not Lst(i).StartsWith("%%url_") OrElse sParam.ToLower = _PARAM_INVITATION_EVAL.ToLower OrElse sParam.ToLower = _PARAM_INVITATION_TT.ToLower Then ' D4936
                    If (comparionCorePage IsNot Nothing AndAlso Not comparionCorePage.isSSO_Only()) OrElse Not _TEMPL_LIST_HIDE_URLS.Contains(Lst(i)) Then    ' D6552 + D7444
                        If _TEMPL_LIST_RES(App.isRiskEnabled).Count > i Then
                            'sTooltip += String.Format("<li><a href='' onclick='return InsertTemplate(""{2}"")' class='actions'>{0}</a>: {1}</li>", Lst(i), TeamTimeClass.ResString(_TEMPL_LIST_RES(App.isRiskEnabled)(i)), JS_SafeString(Lst(i)))    ' D0221 + D2467 + A1561
                            sTooltip.Append($"<li><a href='javascript:void(0)' onclick='return InsertText({i})' id='InsertlnkID{i}' class='actions'>{Lst(i).Replace("%%", "")}</a>: {TeamTimeClass.ResString(_TEMPL_LIST_RES(App.isRiskEnabled)(i))}</li>")
                        End If
                    End If
                End If
            End If
        Next
        'sTooltip += String.Format("</ul><div style='margin:1ex' class='text small gray'>{0}</div>", TeamTimeClass.ResString("msgClick2Insert"))
        sTooltip.Append($"</ul><div style='margin:1ex' class='text small gray'>{TeamTimeClass.ResString("msgClick2Insert")}</div>")
        Dim txt As StringBuilder = New StringBuilder
        txt.Append("<li><a id='InsertlnkID0' onclick='InsertText(0)'>alternatives</a>: <span id='_val_0'>objective</span></li><li><a id='InsertlnkID1' onclick='InsertText(1)'>nodeA</a>: <span id='_val_1'>Improve Organizational</span></li><li><a id='InsertlnkID2' onclick='InsertText(2)'>nodeB</a>: <span id='_val_2'>Maintain Serviceability</span></li><li><a id='InsertlnkID3' onclick='InsertText(3)'>nodename</a>: <span id='_val_3'>Goal: Optimize IT Portfolio To Improve Performance</span></li><li><a id='InsertlnkID4' onclick='InsertText(4)'>objectives</a>: <span id='_val_4'>Objectives</span></li><li><a id='InsertlnkID5' onclick='InsertText(5)'>promt_alt</a>: <span id='_val_5'>Alternatives</span></li><li><a id='InsertlnkID6' onclick='InsertText(6)'>promt_alt_word</a>: <span id='_val_6'>alternative</span></li><li><a id='InsertlnkID7' onclick='InsertText(7)'>rateobjwording</a>: <span id='_val_7'>is more important</span></li><li><a id='InsertlnkID8' onclick='InsertText(8)'>ratewording</a>: <span id='_val_8'>is more preferable</span></li>")
        Dim welcomeVar As New HttpCookie("welcomeVar")
        welcomeVar.Values("welcomeVar") = sTooltip.ToString()
        If (welcomeVar.Value.Contains("welcomeVar")) Then
            welcomeVar.Value = welcomeVar.Value.Substring(11)
        End If
        Response.Cookies.Add(welcomeVar)

    End Sub
    <WebMethod(EnableSession:=True)>
    Public Shared Sub setQHCookies(ByVal ProjectID As Integer, ByVal stepNo As Integer, ByVal status As String, ByVal qh_text As String)
        Dim CurrentProjInfo As CurrentProjectInfo = getCurrentProjectInfo()
        ProjectID = CurrentProjInfo.project_id
        Dim token As String = ProjectID & "-" & stepNo & "-qh"
        Dim cookie = token.GetHashCode().ToString()
        HttpContext.Current.Session(qh_text) = True
        HttpContext.Current.Response.Cookies(cookie).Expires = DateTime.Now.AddDays(10)
        HttpContext.Current.Response.Cookies(cookie).Value = status.ToString()
    End Sub

    <WebMethod(EnableSession:=True)>
    Public Shared Function getCurrentProjectInfo() As CurrentProjectInfo
        Dim model As CurrentProjectInfo = New CurrentProjectInfo()
        Dim App = AppSession()
        'CType(HttpContext.Current.Session("App"), clsComparionCore)
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

                meetingID = App.ActiveProject.MeetingID()
            End If

            wkgname = App.ActiveWorkgroup.Name
        End If

        Dim accessCode = If(App.ActiveProject Is Nothing, "", App.ActiveProject.Passcode(App.ActiveProject.isImpact))

        model.has_project = has_project
        model.is_teamtime = is_teamtime
        model.project_id = App.ProjectID
        model.is_online = is_online
        model.project_name = project_name
        model.access_code = accessCode
        model.workgroup_name = wkgname
        model.meetingID = meetingID

        Return model
    End Function

    <WebMethod(EnableSession:=True)>
    Public Shared Sub setCollapseCookies(ByVal projectId As Integer, ByVal stepType As String, ByVal node_type As String, ByVal status As String)
        Dim token As String = $"{projectId}-{stepType}-{node_type}"
        Dim cookie = token.GetHashCode().ToString()
        Dim isMulti = stepType.StartsWith("All", StringComparison.CurrentCultureIgnoreCase)

        If HttpContext.Current.Request.Cookies(cookie) Is Nothing Then
            HttpContext.Current.Response.Cookies(cookie).Value = status
            HttpContext.Current.Response.Cookies(cookie).Expires = DateTime.Now.AddDays(10)
        Else
            HttpContext.Current.Response.Cookies(cookie).Value = status
        End If

        Dim context As HttpContext = HttpContext.Current
        Dim App = AppSession()
        'CType(context.Session("App"), clsComparionCore)
        Dim multiCollapseStatus = If(context.Session(SessionMultiCollapse) Is Nothing, New Dictionary(Of String, List(Of Boolean))(), CType(context.Session(SessionMultiCollapse), Dictionary(Of String, List(Of Boolean))))

        If App IsNot Nothing Then
            Dim isPM As Boolean = App.CanUserModifyProject(App.ActiveUser.UserID, App.ProjectID, Uw, Ws, App.ActiveWorkgroup)

            If isPM Then
                Dim dictionaryKey As String = $"Key{projectId}_{stepType}"
                Dim multi_collapse As List(Of Boolean) = New List(Of Boolean)()
                Dim exists As Boolean = multiCollapseStatus.ContainsKey(dictionaryKey)

                If Not exists Then
                    multi_collapse.Add(False)
                    multi_collapse.Add(False)
                    multi_collapse.Add(False)
                    multi_collapse.Add(False)
                    multi_collapse.Add(False)
                    multiCollapseStatus.Add(dictionaryKey, multi_collapse)
                Else
                    multi_collapse = multiCollapseStatus(dictionaryKey)
                End If

                Dim collapse As Boolean = If(status = "1", True, False)

                If node_type = "pw-nodes" OrElse node_type = "parent-node" Then

                    If isMulti Then
                        multi_collapse(0) = collapse
                        multi_collapse(1) = collapse
                        multi_collapse(2) = collapse
                    Else

                        If node_type = "pw-nodes" Then

                            If stepType = "One_mtDirect" OrElse stepType = "One_mtRatings" OrElse stepType = "One_mtStep" OrElse stepType = "One_mtRegularUtilityCurve" Then
                                multi_collapse(0) = collapse
                                multi_collapse(1) = collapse
                                multi_collapse(3) = collapse
                            Else
                                multi_collapse(1) = collapse
                                multi_collapse(2) = collapse
                            End If
                        Else
                            multi_collapse(0) = collapse
                        End If
                    End If
                ElseIf node_type = "pw-wrt-nodes" Then
                    multi_collapse(3) = collapse
                    multi_collapse(4) = collapse
                End If

                multiCollapseStatus(dictionaryKey) = multi_collapse
                context.Session(SessionMultiCollapse) = multiCollapseStatus
            End If
        End If
    End Sub

    <WebMethod(EnableSession:=True)>
    Public Shared Sub setSingleCollapsePrivateVar(ByVal ProjectID As Integer, ByVal [step] As Integer, ByVal collapsed_status_list As List(Of Boolean))
        Try
            Dim singePwCollapseStatus = If(HttpContext.Current.Session(SessionSinglePwCollapse) Is Nothing, New Dictionary(Of String, List(Of Boolean))(), CType(HttpContext.Current.Session(SessionSinglePwCollapse), Dictionary(Of String, List(Of Boolean))))
            singePwCollapseStatus(ProjectID & "_" & [step]) = collapsed_status_list
            HttpContext.Current.Session(SessionSinglePwCollapse) = singePwCollapseStatus
        Catch e As Exception
        End Try
    End Sub

    <WebMethod(EnableSession:=True)>
    Public Shared Function getCollapseCookies(ByVal projectId As Integer, ByVal stepType As String, ByVal node_type As String, ByVal is_multi As Boolean) As String
        Dim token As String = $"{projectId}-{stepType}-{node_type}"
        Dim cookie = token.GetHashCode().ToString()
        Dim is_collapsed = "0"
        Dim multiCollapseStatus = If(HttpContext.Current.Session(SessionMultiCollapse) Is Nothing, New Dictionary(Of String, List(Of Boolean))(), CType(HttpContext.Current.Session(SessionMultiCollapse), Dictionary(Of String, List(Of Boolean))))
        Dim singePwCollapseStatus = If(HttpContext.Current.Session(SessionSinglePwCollapse) Is Nothing, New Dictionary(Of String, List(Of Boolean))(), CType(HttpContext.Current.Session(SessionSinglePwCollapse), Dictionary(Of String, List(Of Boolean))))

        If HttpContext.Current.Request.Cookies(cookie) Is Nothing Then
            Dim dictionaryKey As String = $"Key{projectId}_{stepType}"

            If is_multi Then
                Dim pm_saved_collapsed = New List(Of Boolean)()
                Dim exists As Boolean = multiCollapseStatus.ContainsKey(dictionaryKey)

                If exists Then
                    pm_saved_collapsed = multiCollapseStatus(dictionaryKey)

                    If node_type = "pw-nodes" Then
                        is_collapsed = If(pm_saved_collapsed(0) = True, "1", "0")
                    Else
                        is_collapsed = If(pm_saved_collapsed(3) = True, "1", "0")
                    End If
                Else
                    is_collapsed = "0"
                End If
            Else
                Dim pm_saved_collapsed = New List(Of Boolean)()
                Dim exists As Boolean = singePwCollapseStatus.ContainsKey(dictionaryKey)

                If exists Then
                    pm_saved_collapsed = singePwCollapseStatus(dictionaryKey)

                    Try

                        If node_type = "pw-nodes" Then
                            is_collapsed = If(pm_saved_collapsed(1) = True, "1", "0")
                        ElseIf node_type = "parent-node" Then
                            is_collapsed = If(pm_saved_collapsed(0) = True, "1", "0")
                        Else
                            is_collapsed = If(pm_saved_collapsed(3) = True, "1", "0")
                        End If

                    Catch e As Exception
                        is_collapsed = "0"
                    End Try
                Else
                    is_collapsed = "0"
                End If
            End If

            HttpContext.Current.Response.Cookies(cookie).Value = is_collapsed
            HttpContext.Current.Response.Cookies(cookie).Expires = DateTime.Now.AddDays(10)
        End If

        Return HttpContext.Current.Request.Cookies(cookie).Value
    End Function

    <WebMethod(EnableSession:=True)>
    Public Shared Function getInfoDocSizes() As String()()
        Dim App = AppSession()
        'CType(HttpContext.Current.Session("App"), clsComparionCore)
        Dim isPm As Boolean = App.ActiveUser IsNot Nothing AndAlso App.CanUserModifyProject(App.ActiveUser.UserID, App.ProjectID, Uw, Ws, App.ActiveWorkgroup)
        Return GeckoClass.getInfodoc_sizes(isPm)
    End Function

    <WebMethod(EnableSession:=True)>
    Public Shared Sub SaveRatings(ByVal [step] As Integer, ByVal RatingID As String, ByVal sComment As String, ByVal intensities As List(Of String()), ByVal UserID As Integer, ByVal DirectValue As String)
        Dim context = HttpContext.Current
        Dim isPipeViewOnly = CBool(context.Session(Constants.SessionIsPipeViewOnly))

        If isPipeViewOnly Then
            Return
        End If

        Dim app = AppSession()
        'CType(context.Session("App"), clsComparionCore)
        Dim action = AnytimeClass.GetAction([step])
        Dim data = CType(action.ActionData, clsOneAtATimeEvaluationActionData)
        Dim ratingData As clsNonPairwiseMeasureData = Nothing

        If UserID = 0 Then
            ratingData = CType(data.Judgment, clsNonPairwiseMeasureData)
        Else
            ratingData = (CType(data.Node.Judgments, clsNonPairwiseJudgments)).GetJudgement((CType(data.Judgment, clsNonPairwiseMeasureData)).NodeID, data.Node.NodeID, UserID)

            If ratingData Is Nothing Then
                ratingData = New clsRatingMeasureData((CType(data.Judgment, clsNonPairwiseMeasureData)).NodeID, data.Node.NodeID, UserID, Nothing, CType(data.Node.MeasurementScale, clsRatingScale), True)
            End If
        End If

        Dim mScale = data.MeasurementScale

        If Not String.IsNullOrEmpty(RatingID) Then

            If RatingID = "-1" Then
                ratingData.IsUndefined = True
            Else

                If mScale IsNot Nothing Then

                    For Each tR As clsRating In (CType(mScale, clsRatingScale)).RatingSet

                        If tR.ID.ToString() = RatingID Then
                            ratingData.IsUndefined = False
                            ratingData.ObjectValue = tR
                            Exit For
                        End If
                    Next
                End If
            End If

            Dim ratingDirect As Double = -1

            If Convert.ToInt64(RatingID) < 0 AndAlso RatingID <> "-1" AndAlso StringFuncs.String2Double(DirectValue, ratingDirect) Then

                If ratingDirect >= 0 AndAlso ratingDirect <= 1 Then
                    ratingData.IsUndefined = False
                    ratingData.ObjectValue = New clsRating(-1, "Direct input from EC Core", CSng(ratingDirect), Nothing)
                End If
            End If

            If sComment <> ratingData.Comment Then
                ratingData.Comment = sComment
            End If

            If data.Node IsNot Nothing Then

                If data.Node.IsAlternative Then
                    app.ActiveProject.HierarchyAlternatives.GetNodeByID(data.Node.NodeID).Judgments.AddMeasureData(ratingData)
                Else
                    app.ActiveProject.HierarchyObjectives.GetNodeByID(data.Node.NodeID).Judgments.AddMeasureData(ratingData)
                End If
            End If

            app.ActiveProject.ProjectManager.StorageManager.Writer.SaveJudgment(data.Node, ratingData)
            Dim isAlreadySaved = CBool(context.Session(SessionIsJudgmentAlreadySaved))

            If Not isAlreadySaved Then
                context.Session(SessionIncreaseJudgmentsCount) = True
                context.Session(SessionIsJudgmentAlreadySaved) = True
            End If

            Dim isChanged = False
            Dim isPm = app.CanUserModifyProject(app.ActiveUser.UserID, app.ProjectID, Uw, Ws, app.ActiveWorkgroup)

            If isPm AndAlso mScale IsNot Nothing Then

                For Each intensity As clsRating In (CType(mScale, clsRatingScale)).RatingSet
                    Dim modifiedIntensity As String() = intensities.FirstOrDefault(Function(i) i(2) = intensity.ID.ToString())

                    If modifiedIntensity IsNot Nothing AndAlso modifiedIntensity.Length = 5 AndAlso intensity.Comment <> modifiedIntensity(4) Then
                        intensity.Comment = modifiedIntensity(4)
                        isChanged = True
                    End If
                Next
            End If

            If isChanged Then
                app.ActiveProject.ProjectManager.StorageManager.Writer.SaveModelStructure()
            End If
        End If
    End Sub


    <WebMethod(EnableSession:=True)>
    Public Shared Sub setInfoDocSizes(ByVal width As String, ByVal height As String, ByVal index As Integer, ByVal is_multi As Boolean)
        Dim context = HttpContext.Current
        Dim isPipeViewOnly = CBool(context.Session(Constants.SessionIsPipeViewOnly))

        If isPipeViewOnly Then
            Return
        End If

        Dim App = AppSession()
        'CType(HttpContext.Current.Session("App"), clsComparionCore)
        Dim isPm As Boolean = App.CanUserModifyProject(App.ActiveUser.UserID, App.ProjectID, Uw, Ws, App.ActiveWorkgroup)
        GeckoClass.setInfodoc_sizes(width, height, index, is_multi, isPm)
    End Sub

    <WebMethod(EnableSession:=True)>
    Public Shared Sub SaveDirect(ByVal [step] As Integer, ByVal value As String, ByVal sComment As String)
        Dim context = HttpContext.Current
        Dim isPipeViewOnly = CBool(context.Session(Constants.SessionIsPipeViewOnly))

        If isPipeViewOnly Then
            Return
        End If

        Dim app = AppSession()
        'CType(context.Session("App"), clsComparionCore)
        Dim action = AnytimeClass.GetAction([step])
        If action Is Nothing OrElse action.ActionType <> ActionType.atNonPWOneAtATime Then Return
        Dim data = CType(action.ActionData, clsOneAtATimeEvaluationActionData)
        Dim directJudgment = CType(data.Judgment, clsDirectMeasureData)

        If value = "-1" OrElse value = "" Then
            directJudgment.IsUndefined = True
        Else
            Dim directValue As Double = -1

            If StringFuncs.String2Double(value, directValue) Then
                directJudgment.ObjectValue = directValue
                directJudgment.IsUndefined = False
            End If
        End If

        If sComment <> directJudgment.Comment Then
            directJudgment.Comment = sComment
        End If

        CType(action.ActionData, clsOneAtATimeEvaluationActionData).Node.Judgments.AddMeasureData(directJudgment)
        app.ActiveProject.ProjectManager.StorageManager.Writer.SaveJudgment(app.ActiveProject.HierarchyObjectives.GetNodeByID(directJudgment.ParentNodeID), directJudgment)
    End Sub
    '''<summary>
    '''Created new function to initialize culture for localize the forms
    ''' </summary>
    Protected Overrides Sub InitializeCulture()
        MyBase.InitializeCulture()
        If (Session("culture") IsNot Nothing) Then
            Dim ci As New CultureInfo(Session("culture").ToString())
            Thread.CurrentThread.CurrentCulture = ci
            Thread.CurrentThread.CurrentUICulture = ci
        End If
    End Sub
    Protected Overrides Sub OnInit(e As System.EventArgs)
        MyBase.OnInit(e)
        If (Session("culture") IsNot Nothing) Then
            Dim ci As New CultureInfo(Session("culture").ToString())
            Thread.CurrentThread.CurrentCulture = ci
            Thread.CurrentThread.CurrentUICulture = ci
        End If
    End Sub

    '''<summary>
    '''Created method to localize client side and server side content
    ''' </summary>

    Public Function GetResourceText(ByVal ResourceKey As String) As String
        Dim retValue As String = ""
        Try
            retValue = GetLocalResourceObject(ResourceKey).ToString()
            If retValue Is Nothing Then retValue = ""
        Catch ex As Exception
            retValue = ""
        End Try
        Return retValue.Trim()
    End Function

    <WebMethod(EnableSession:=True)>
    Public Shared Sub setAutoAdvance(ByVal value As Boolean)
        Dim context As HttpContext = HttpContext.Current
        Dim App = AppSession()
        'CType(context.Session("App"), clsComparionCore)

        If App IsNot Nothing Then
            context.Session(SessionAutoAdvance + App.ProjectID.ToString()) = CBool(value)
        End If
    End Sub

    Public Shared Function CheckProjectLockStatus() As LockedInfo
        Dim context = HttpContext.Current
        Dim sessionName = "IsCheckingProjectLockStatusFirstTime"
        Dim isFirstTime = context.Session(sessionName) Is Nothing OrElse CBool(context.Session(sessionName))

        If Not isFirstTime Then
            System.Threading.Thread.Sleep(1000)
        End If

        Dim app = AppSession()
        'CType(context.Session("App"), clsComparionCore)
        Dim isProjectLocked = True
        Dim lockedMessage = String.Empty
        Dim redirectUrl = String.Empty
        context.Session(sessionName) = False

        If app.ActiveProject IsNot Nothing Then
            Dim project = app.DBProjectByID(app.ProjectID)
            isProjectLocked = Not (project.LockInfo.LockerUserID = app.ActiveUser.UserID OrElse project.LockInfo.LockStatus = ECLockStatus.lsUnLocked)
            lockedMessage = If(isProjectLocked, String.Format(TeamTimeClass.ResString("msgEvaluationLocked"), project.ProjectName), String.Empty)

            If project.isTeamTime Then
                redirectUrl = redirectToComparionTeamtime()
                lockedMessage = TeamTimeClass.ResString("msgTeamTimeRedirection")
                lockedMessage = If(String.IsNullOrEmpty(lockedMessage), "Anytime Evaluation is not available while TeamTime™ is in progress. You will be redirected to TeamTime™ Evaluation in {0} seconds.", lockedMessage)
                lockedMessage = String.Format(lockedMessage, 5)
            End If
        End If

        Dim isPipeViewOnly = CBool(context.Session(Constants.SessionIsPipeViewOnly))

        If isPipeViewOnly Then
            isProjectLocked = False
            lockedMessage = ""
        End If


        Dim lockedInfo = New LockedInfo()
        lockedInfo.status = isProjectLocked
        lockedInfo.message = lockedMessage
        lockedInfo.teamTimeUrl = redirectUrl

        Return lockedInfo
        'Return New JavaScriptSerializer().Serialize(lockedInfo)
    End Function

    <WebMethod(EnableSession:=True)>
    Public Shared Function Ajax_Callback(ByVal data As String) As String
        Dim args As NameValueCollection = HttpUtility.ParseQueryString(data)
        Dim sAction As String = Common.GetParam(args, ExpertChoice.Web.Options._PARAM_ACTION).Trim().ToLower()
        Dim sResult As String = Convert.ToString((If(String.IsNullOrEmpty(sAction), "", sAction)))
        Dim App = AppSession()
        'CType(HttpContext.Current.Session("App"), clsComparionCore)
        Dim sensitivitiesAnalysis = New SensitivitiesAnalysis()

        Select Case sAction
            Case "node"
                Dim tNodeID As Integer = -1

                If Integer.TryParse(Common.GetParam(args, "node_id").ToLower(), tNodeID) Then
                    sensitivitiesAnalysis.ProjectManager = App.ActiveProject.ProjectManager
                    sensitivitiesAnalysis.SetSaUserId(App)
                    sensitivitiesAnalysis.CurrentNode = sensitivitiesAnalysis.ProjectManager.Hierarchy(sensitivitiesAnalysis.ProjectManager.ActiveHierarchy).GetNodeByID(tNodeID)
                    sensitivitiesAnalysis.ObjPriorities.Clear()
                    sensitivitiesAnalysis.AltValues.Clear()
                    sensitivitiesAnalysis.AltValuesInOne.Clear()
                    sensitivitiesAnalysis.ProjectManager.CalculationsManager.InitializeSAGradient(sensitivitiesAnalysis.CurrentNode.NodeID, False, sensitivitiesAnalysis.SAUserID, sensitivitiesAnalysis.ObjPriorities, sensitivitiesAnalysis.AltValues, sensitivitiesAnalysis.AltValuesInOne, 0)
                    sensitivitiesAnalysis.AltValuesInZero = sensitivitiesAnalysis.ProjectManager.CalculationsManager.GetGradientData(sensitivitiesAnalysis.CurrentNode.NodeID, False, sensitivitiesAnalysis.SAUserID, sensitivitiesAnalysis.ObjPriorities, 0)
                    sResult = sensitivitiesAnalysis.GetSAData()
                End If

            Case "normalization"
                Dim tID As Integer = -1

                If Integer.TryParse(Common.GetParam(args, "norm_mode").ToLower(), tID) Then
                    sensitivitiesAnalysis.NormalizationMode = CType(tID, AlternativeNormalizationOptions)
                    sResult = sensitivitiesAnalysis.GetSAData()
                End If

            Case SensitivitiesAnalysis.ACTION_DSA_UPDATE_VALUES
                Dim s_values As String = Common.GetParam(args, "values").Trim()
                Dim values As String() = s_values.Split(Convert.ToChar(","))
                Dim s_ids As String = Common.GetParam(args, "objids").Trim()
                Dim ids As String() = s_ids.Split(Convert.ToChar(","))
                Dim ANewObjPriorities As Dictionary(Of Integer, Double) = New Dictionary(Of Integer, Double)()
                Dim i As Integer = 0

                For Each objID_loopVariable As String In ids
                    Dim objID = objID_loopVariable
                    Dim APrty As Double = 0
                    Double.TryParse(values(i), APrty)
                    ANewObjPriorities.Add(Convert.ToInt32(objID), APrty)
                    i += 1
                Next

                sensitivitiesAnalysis.updateAltValuesinZero(ANewObjPriorities)
                Dim ZeroValuesString As String = ""

                For Each Objitem_loopVariable As KeyValuePair(Of Integer, Dictionary(Of Integer, Double)) In sensitivitiesAnalysis.AltValuesInZero
                    Dim Objitem = Objitem_loopVariable
                    Dim ZeroAltValuesString As String = ""

                    For Each AltItem_loopVariable As KeyValuePair(Of Integer, Double) In Objitem.Value
                        Dim AltItem = AltItem_loopVariable
                        ZeroAltValuesString += Convert.ToString((If(Not String.IsNullOrEmpty(ZeroAltValuesString), ",", ""))) & String.Format("{{altID:{0},val:{1}}}", AltItem.Key, StringFuncs.JS_SafeNumber(AltItem.Value))
                    Next

                    ZeroValuesString += Convert.ToString((If(Not String.IsNullOrEmpty(ZeroValuesString), ",", ""))) & String.Format("[{0},[{1}]]", Objitem.Key, ZeroAltValuesString)
                Next

                sResult = String.Format("[[{0}]]", ZeroValuesString)
        End Select

        If Not String.IsNullOrEmpty(sResult) Then
            Return sResult
        End If

        Return ""
    End Function

    <WebMethod(EnableSession:=True)>
    Public Shared Function initializeSensitivity() As initSAModel
        Return New SensitivitiesAnalysis().initSA()
    End Function

    <WebMethod(EnableSession:=True)>
    Public Shared Function GetModelLockedStatus() As Object
        Return CheckProjectLockStatus()
    End Function

    <WebMethod(EnableSession:=True)>
    Public Shared Function BindAllPairwiseHtml(ByVal fromval As Int16, ByVal toval As Int16) As String
        If HttpContext.Current.Session("output") IsNot Nothing Then
            Dim aModel As AnytimeOutputModel = New AnytimeOutputModel()
            'aModel = CType(If(HttpContext.Current.Session("output") IsNot Nothing, JsonConvert.DeserializeObject(HttpContext.Current.Session("output").ToString()), Nothing), AnytimeOutputModel)
            aModel = If(HttpContext.Current.Session("output") IsNot Nothing, CType(HttpContext.Current.Session("output"), AnytimeOutputModel), Nothing)
            Return AllPairWiseUserCtrl.BindRestHtml(aModel, fromval, toval)
        Else
            Return ""
        End If
    End Function

    Public Function showT2S() As Boolean
        'Return If(_OPT_EVAL_T2S_ALLOWED, App.ActiveProject.ProjectManager.Parameters.EvalTextToSpeech <> ecText2Speech.Disabled , False) ' D6991 + D7005 + D7066
        Return True
    End Function

    Public Function Bool2JS(bValue As Boolean) As String 'A1041
        Return If(bValue, "true", "false")
    End Function

    Public Function COOKIE_T2S_MODE() As String
        Dim App = AppSession()
        'CType(Session("App"), clsComparionCore)
        If Not IsNothing(App) Then
            Return "t2s" + Convert.ToString(App.ProjectID)
        Else
            Return ""
        End If
    End Function
    Public Function GetCookie(ByVal sName As String, Optional ByVal sDefValue As String = "") As String
        If (Request Is Nothing OrElse Request.Cookies(sName) Is Nothing) Then   ' D5065
            Return sDefValue
        Else
            ' D0755 ===
            Dim sVal As String = RemoveXssFromText(Request.Cookies(sName).Value)    ' D6766
            If sVal <> "" AndAlso (EncryptAllCookies OrElse sVal.StartsWith(CookieEncryptionPrefix)) Then sVal = DecodeURL(sVal.Substring(CookieEncryptionPrefix.Length), App.DatabaseID) ' D0826 + D2895
            If sVal = "" Then sVal = sDefValue ' D0829
            Return sVal
            ' D0755 ==
        End If
    End Function
    Public Function IsMobile() As Boolean
        Dim App As clsComparionCore = AppSession()
        'CType(HttpContext.Current.Session("App"), clsComparionCore)
        Return App.isMobileBrowser
    End Function

    Protected Sub RadToolTipClusterSteps_Load(sender As Object, e As EventArgs)
        'Dim isTitle As Boolean = sender Is RadToolTipTitleSteps
        '' D4108 ===
        'Dim HasQHCluster As Boolean = False
        'Dim QHApplySteps As New List(Of Integer)
        '' D4329 ===
        'Dim ApplyNodes As List(Of clsNode) = App.ActiveProject.ProjectManager.PipeBuilder.GetPipeStepClusters(CurStep, HasQHCluster, QHApplySteps)  ' D4116
        'Dim fCanShow As Boolean = QHApplySteps.Count > 1 OrElse HasQHCluster
        'If isTitle Then CanShowTitleApplyTo = fCanShow Else CanShowApplyTo = fCanShow
        'If fCanShow AndAlso CurAction IsNot Nothing Then
        '    ' D4329 ==
        '    Dim sTooltip As String = ""
        '    ' D4116 ===
        '    Dim isMulti As Boolean = False
        '    Dim tAGuid As Guid = Guid.Empty
        '    Select Case CurAction.ActionType
        '        Case ActionType.atAllPairwise, ActionType.atAllPairwiseOutcomes, ActionType.atNonPWAllChildren, ActionType.atNonPWAllCovObjs, ActionType.atShowLocalResults
        '            isMulti = True
        '    End Select
        '    Select Case CurAction.ActionType
        '        Case ActionType.atShowLocalResults, ActionType.atShowGlobalResults
        '            ' D4329 ===
        '            If isTitle Then
        '                If IsIntensities AndAlso IntensityScale IsNot Nothing Then tAGuid = IntensityScale.GuidID
        '            Else
        '                If CurStepNode IsNot Nothing Then tAGuid = CurStepNode.NodeGuidID
        '            End If
        '    End Select
        '    Dim tCurClusterPhrase As String = ""
        '    If isTitle Then
        '        tCurClusterPhrase = App.ActiveProject.ProjectManager.PipeBuilder.GetClusterTitleForResults(CurStepNode.NodeGuidID, tAGuid)
        '    Else
        '        tCurClusterPhrase = App.ActiveProject.ProjectManager.PipeBuilder.GetClusterPhraseForNode(CurStepNode, isMulti, tAGuid, IsRiskWithControls)   ' D4133
        '    End If
        '    ' D4329 ==
        '    Dim i As Integer = 1
        '    For Each tNode As clsNode In ApplyNodes
        '        Dim fActive As Boolean = (tNode Is CurStepNode) OrElse (CurStep = QHApplySteps(i - 1))
        '        Dim fChecked As Boolean = fActive
        '        If Not fChecked AndAlso ClusterPhrase <> "" Then
        '            tAGuid = Guid.Empty
        '            Select Case CurAction.ActionType
        '                Case ActionType.atShowLocalResults, ActionType.atShowGlobalResults
        '                    ' D4329 ===
        '                    If Not isTitle Then tAGuid = tNode.NodeGuidID
        '            End Select
        '            Dim sStepClusterPhrase As String = ""
        '            If isTitle Then
        '                sStepClusterPhrase = App.ActiveProject.ProjectManager.PipeBuilder.GetClusterTitleForResults(tNode.NodeGuidID, tAGuid)
        '            Else
        '                sStepClusterPhrase = App.ActiveProject.ProjectManager.PipeBuilder.GetClusterPhraseForNode(tNode, isMulti, tAGuid, IsRiskWithControls) ' D4133
        '            End If
        '            ' D4329 ==
        '            If sStepClusterPhrase = ClusterPhrase Then fChecked = True
        '        End If
        '        sTooltip += String.Format("<li><nobr><input type='checkbox' id='{5}{1}'{2}{4} value='{3}'><label for='task_guid_{1}'>{0}</label></nobr></li>", SafeFormString(ShortString(tNode.NodeName, 40, True)), i, IIf(fActive, " disabled", ""), tNode.NodeGuidID.ToString, IIf(fChecked, " checked", ""), IIf(isTitle, "title_guid_", "task_guid_"))
        '        ' D4116 ==
        '        i += 1
        '    Next
        '    sTooltip = String.Format("<div style='padding:3ex'><b>{0}</b><ul type=square style='margin:1ex 2em 1ex 1em;{1}'>{2}</ul><div style='text-align:center; margin-top:1em;'><input type='button' class='button button_small' id='btnApplyTo' value='{3}' onclick='SaveTaskMulti({4}); return false;' style='width:15em'></div></div>", ResString("lblEditTaskChooseSteps"), IIf(ApplyNodes.Count > 10, " height:10em; overflow-y:scroll;", ""), sTooltip, ResString("btnEditTaskSaveMulti"), IIf(isTitle, 1, 0)) ' D4329
        '    CType(sender, RadToolTip).Text = sTooltip   ' D4329
        'End If
        '' D4108 ==
    End Sub

    <WebMethod(EnableSession:=True)>
    Public Shared Function UpdateWelcomeText(ByVal welcomeText As String) As Boolean
        Dim App As clsComparionCore = AppSession()
        Dim fUpdated As Boolean = False
        If App IsNot Nothing Then
            Dim basePath As String = Infodoc_Path(App.ActiveProject.ID, App.ActiveProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.PipeMessage, "welcome", -1)
            Dim baseUrl = $"{HttpContext.Current.Request.Url.Scheme}://{HttpContext.Current.Request.Url.Host}"
            Dim sMHT As String = Infodoc_Pack(welcomeText, baseUrl, basePath)

            App.ActiveProject.PipeParameters.PipeMessages.SetWelcomeText(PipeMessageKind.pmkText, App.ActiveProject.ProjectManager.ActiveHierarchy, App.ActiveProject.ProjectManager.ActiveAltsHierarchy, sMHT)
            fUpdated = True
        End If

        If fUpdated Then
            App.ActiveProject.PipeParameters.PipeMessages.Save(PipeStorageType.pstStreamsDatabase, App.ActiveProject.ConnectionString, App.ActiveProject.ProviderType, App.ActiveProject.ID) ' D0329 + D0369 + D0376 'C0420 + D0483 + D1146
            If fUpdated AndAlso App.isAuthorized Then App.SaveProjectLogEvent(App.ActiveProject, "Edit infodoc", False, "welcome") ' D3769 + D4037
        End If

        If fUpdated AndAlso App.ActiveProject IsNot Nothing Then
            App.ActiveProject.ProjectManager.LastModifyTime = Now ' D1292
            App.DBUpdateDateTime(clsComparionCore._TABLE_PROJECTS, clsComparionCore._FLD_PROJECTS_LASTMODIFY, App.ActiveProject.ID)    ' D2453
        End If

        If fUpdated Then
            RawResponseStart()
            HttpContext.Current.Response.ContentType = "text/plain"
            'Response.AddHeader("Content-Length", CStr(sResult))
            Dim sReply As String = TeamTimeClass.ResString("msgSaved")

            HttpContext.Current.Response.Write(sReply)
            ' D4049 ==
            'HttpContext.Current.Response.End()
            HttpContext.Current.Response.Flush()
            HttpContext.Current.Response.SuppressContent = True
            HttpContext.Current.ApplicationInstance.CompleteRequest()
        End If

        Return fUpdated
    End Function

    Public Shared Sub RawResponseStart()
        DebugInfo("Clear response on custom content reply")
        'Response.Buffer = False ' D4946
        'Response.BufferOutput = False   ' D4946
        'Response.Clear()
        ''Response.ClearHeaders()
        HttpContext.Current.Response.ClearContent()
        ' D7625 ===
        HttpContext.Current.Response.Headers.Remove("Server")
        HttpContext.Current.Response.Headers.Remove("X-AspNet-Version")
        HttpContext.Current.Response.Headers.Remove("X-Powered-By")
        HttpContext.Current.Response.AppendHeader("Cache-Control", "no-cache, no-store, must-revalidate")
        HttpContext.Current.Response.AppendHeader("Pragma", "no-cache")
        HttpContext.Current.Response.Cache.SetLastModified(Now())
        HttpContext.Current.Response.Cache.SetExpires(Now())
        HttpContext.Current.Response.Cache.SetMaxAge(TimeSpan.Zero)
        HttpContext.Current.Response.Cache.SetNoStore()
        HttpContext.Current.Response.Cache.SetNoServerCaching()
        HttpContext.Current.Response.Cache.SetCacheability(HttpCacheability.NoCache)
        'If isSecureConnection(Request) Then Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains")
        ' D7625 ==
    End Sub

    Public Sub RawResponseEnd()
        If Response.IsClientConnected Then  ' D1232
            'Response.Flush()
            'Response.Close()   ' -D4946
        End If
        Response.End()
    End Sub

    Protected Sub hdnbtnWelcomeUpdate_Click(sender As Object, e As EventArgs)
        Dim App As clsComparionCore = AppSession()
        Dim fUpdated As Boolean = False
        If App IsNot Nothing Then
            Dim basePath As String = Infodoc_Path(App.ActiveProject.ID, App.ActiveProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.PipeMessage, "welcome", -1)
            Dim baseUrl = $"{HttpContext.Current.Request.Url.Scheme}://{HttpContext.Current.Request.Url.Host}"
            Dim sMHT As String = Infodoc_Pack(hdnWelcomeText.Value, baseUrl, basePath)

            App.ActiveProject.PipeParameters.PipeMessages.SetWelcomeText(PipeMessageKind.pmkText, App.ActiveProject.ProjectManager.ActiveHierarchy, App.ActiveProject.ProjectManager.ActiveAltsHierarchy, sMHT)
            fUpdated = True
        End If

        If fUpdated Then
            App.ActiveProject.PipeParameters.PipeMessages.Save(PipeStorageType.pstStreamsDatabase, App.ActiveProject.ConnectionString, App.ActiveProject.ProviderType, App.ActiveProject.ID) ' D0329 + D0369 + D0376 'C0420 + D0483 + D1146
            If fUpdated AndAlso App.isAuthorized Then App.SaveProjectLogEvent(App.ActiveProject, "Edit infodoc", False, "welcome") ' D3769 + D4037
        End If

        If fUpdated AndAlso App.ActiveProject IsNot Nothing Then
            App.ActiveProject.ProjectManager.LastModifyTime = Now ' D1292
            App.DBUpdateDateTime(clsComparionCore._TABLE_PROJECTS, clsComparionCore._FLD_PROJECTS_LASTMODIFY, App.ActiveProject.ID)    ' D2453
        End If

        If fUpdated Then
            RawResponseStart()
            'HttpContext.Current.Response.ContentType = "text/plain"
            'Response.AddHeader("Content-Length", CStr(sResult))
            Dim sReply As String = TeamTimeClass.ResString("msgSaved")

            'HttpContext.Current.Response.Write(sReply)
            ' D4049 ==
            'HttpContext.Current.Response.End()
            Response.Redirect(Request.RawUrl)
            HttpContext.Current.Response.Flush()
            HttpContext.Current.Response.SuppressContent = True
            HttpContext.Current.ApplicationInstance.CompleteRequest()
        End If
    End Sub

    '<WebMethod(EnableSession:=True)>
    'Public Shared Function SaveClusterPhraseForNode(ByVal GuidsList As String) As Boolean

    '    Dim tGuids As New List(Of Guid)
    '    If GuidsList <> "" Then
    '        Dim sList As String() = GuidsList.Split(CType(",", Char()))
    '        If sList.Count > 0 Then
    '            For Each sGuid As String In sList
    '                Try
    '                    Dim G As New Guid(sGuid)
    '                    If tGuids.IndexOf(G) < 0 Then tGuids.Add(G)
    '                Catch ex As Exception
    '                End Try
    '            Next
    '        End If
    '    End If
    '    If tGuids.IndexOf(AGuid) < 0 Then tGuids.Add(AGuid)
    '    ' D2751 ===
    '    Dim isMulti As Boolean = False
    '    Select Case CurAction.ActionType
    '        Case ActionType.atAllPairwise, ActionType.atAllPairwiseOutcomes, ActionType.atNonPWAllChildren, ActionType.atNonPWAllCovObjs
    '            isMulti = True
    '                    ' D4053 ===
    '        Case ActionType.atShowLocalResults
    '            isMulti = True
    '            ' D4053 ==
    '    End Select
    'End Function

End Class