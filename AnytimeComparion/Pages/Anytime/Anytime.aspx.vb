Imports System.Web.Services
Imports Canvas
Imports ECCore
Imports ExpertChoice.Data
Imports ExpertChoice.Service
Imports HtmlAgilityPack
Imports Newtonsoft.Json
Imports AnytimeComparion.Pages.external_classes
Imports System.Web.Script.Serialization
Imports System.IO
Imports ExpertChoice.Results

Public Class Anytime
    Inherits System.Web.UI.Page
    Private Const AutoAdvanceMaxJudgments As Integer = 5

    Private Const InconsistencySortingEnabled As String = "InconsistencySortingEnabled"
    Private Const InconsistencySortingOrder As String = "InconsistencySortingOrder"
    Private Const BestFit As String = "BestFit"

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


#Region "Page load"

    'Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    'End Sub

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        Dim App = CType(Session("App"), clsComparionCore)

        If App.ActiveUser Is Nothing Then
            Response.Redirect("~/login.aspx")
        End If

        If App IsNot Nothing Then
            Session("topHeader") = True

            If Not IsPostBack AndAlso App.HasActiveProject() Then
                Dim updatedProject = App.DBProjectByID(App.ProjectID)
                Dim projectIndex = App.ActiveProjectsList.IndexOf(App.ActiveProject)
                App.ActiveProjectsList(projectIndex) = updatedProject
            End If

            HttpContext.Current.Session("showMessage") = False

            If App.ActiveProject IsNot Nothing AndAlso App.ActiveProject.isTeamTime Then
                Dim ecAppId = App.DBTeamTimeSessionAppID(App.ActiveProject.ID)

                If ecAppId = PipeParameters.ecAppliationID.appComparion Then
                    Response.Redirect(redirectToComparionTeamtime())
                Else
                End If
            End If

            CheckForNextProjectAndRedirectIfRequired(App)
            InitializeSessions(App)

            Try
                Dim CurrentUser As clsApplicationUser = New clsApplicationUser()
                Dim isDataInstance = False
                Dim recreatePipe = False
                NotRecreatePipe = False

                If AnytimeClass.UserIsReadOnly() Then
                    CurrentUser = App.DBUserByID(AnytimeClass.GetReadOnlyUserID())
                Else
                    CurrentUser = App.ActiveUser
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
                    Response.Redirect("~/?project=" & Project.ProjectName & "&is_offline=true")
                End If

                Dim WorkSpace = CType(App.ActiveWorkspace, clsWorkspace)
                'Dim CurrentStep = App.ActiveWorkspace.get_ProjectStep(App.ActiveProject.isImpact)
                Dim CurrentStep = App.ActiveWorkspace.ProjectStep(App.ActiveProject.isImpact)
                Session("AT_CurrentStep") = CurrentStep
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
                    _OriginalAHPUser = Nothing
                    App.Logout()
                    HttpContext.Current.Session.Clear()
                    HttpContext.Current.Session.Abandon()
                    HttpContext.Current.Response.Cookies("rmberme").Expires = DateTime.Now.AddDays(-1)
                    HttpContext.Current.Response.Cookies("fullname").Expires = DateTime.Now.AddDays(-1)


                    Response.Redirect(AnytimeClass.GetComparionHashLink().ToString())
                End If
            End If


            BindUserControl(Convert.ToInt32(Session("AT_CurrentStep")))

        Else
            Response.Redirect(ResolveUrl("~/"))
        End If
    End Sub

    'Protected Sub hdnPageNo_Click(sender As Object, e As EventArgs)
    '    If Convert.ToInt32(hdnPageNumber.Value) > 0 Then
    '        BindUserControl(Convert.ToInt32(hdnPageNumber.Value))
    '    End If
    'End Sub

    Public Sub BindUserControl(ByVal stepNo As Integer)
        'If stepNo <= 1 Then
        'End If
        'Page.ClientScript.RegisterStartupScript(Page.GetType(), "ShowHide", $"setPagination('{stepNo}');", True)
        If stepNo > 0 Then
            Dim anytimeOutputModel As AnytimeOutputModel = New AnytimeOutputModel()
            anytimeOutputModel = GetDataOfPipeStep(stepNo)
            If anytimeOutputModel IsNot Nothing Then
                hdnCurrentStep.Value = anytimeOutputModel.current_step
                hdnTotalSteps.Value = anytimeOutputModel.total_pipe_steps
                hdnparentnodeID.Value = anytimeOutputModel.parentnodeID

                'make visible false all user controls then make them visible true as per page type
                SurveyControl.Visible = False
                InformationControl.Visible = False
                LocalResultsControl.Visible = False
                AllPairWiseControl.Visible = False

                If anytimeOutputModel.page_type = "atSpyronSurvey" Then
                    SurveyControl.Visible = True
                    SurveyControl.bindHtml(anytimeOutputModel.PipeParameters)
                ElseIf anytimeOutputModel.page_type = "atInformationPage" Then
                    InformationControl.Visible = True
                    InformationControl.BindHtml(anytimeOutputModel.information_message)
                ElseIf anytimeOutputModel.page_type = "atShowLocalResults" Then
                    LocalResultsControl.Visible = True
                    LocalResultsControl.bindHtml(anytimeOutputModel.PipeParameters, anytimeOutputModel.step_task)
                ElseIf anytimeOutputModel.page_type = "atAllPairwise" Then
                    AllPairWiseControl.Visible = True
                    AllPairWiseControl.BindHtml(anytimeOutputModel)
                End If
            End If
        End If
    End Sub

    Protected Sub Page_Unload(ByVal sender As Object, ByVal e As EventArgs)
        If _OriginalAHPUser IsNot Nothing Then
            AnytimeClass.SetUser(_OriginalAHPUser, False, False)
        End If
    End Sub

    'Redirect project if project teamtime is True
    <WebMethod(EnableSession:=True)>
    Public Shared Function redirectToComparionTeamtime() As String
        Dim app = CType(HttpContext.Current.Session("App"), clsComparionCore)
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
            Dim redirectUrl = If(url.Host.Equals("localhost"), $"{url.Scheme}://{url.Host}:9793/", $"{url.Scheme}://{url.Authority}/")
            redirectUrl = GeckoClass.CreateLogonURL(app.ActiveUser, nextProject, False, "step=1", redirectUrl, "")

            If Not String.IsNullOrEmpty(redirectUrl) Then
                context.Response.Redirect(redirectUrl)
            End If
        End If
    End Sub

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


    Public Shared Function GetDataOfPipeStep(ByVal stepId As Integer)

        'Dim message = ""
        'Dim Context As System.Web.HttpContext.Current

        'stepId = 2

        Dim App As clsComparionCore = HttpContext.Current.Session("App")
        If App.ActiveProject Is Nothing Then
            Dim returnValue As New ReturnValMessage
            returnValue.message = GeckoClass.timeOutMessage
            returnValue.status = "timeout"
            Dim oSerializer = New JavaScriptSerializer()
            Dim obj As Object = oSerializer.Serialize(returnValue)
            Return obj
        End If
        If CheckProject() Then
            Return False
        End If

        ' HttpContext.Current.Session("AT_CurrentStep") = 2

        Dim CurrentStep = CInt(HttpContext.Current.Session("AT_CurrentStep"))
        'Dim stepId As Integer = 1
        If CurrentStep <> stepId Then
            HttpContext.Current.Session(SessionIsJudgmentAlreadySaved) = False
        End If

        If CurrentStep > App.ActiveProject.Pipe.Count Then
            CurrentStep = 1
            stepId = CurrentStep
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


        Dim scaleDescriptions As List(Of Object) = New List(Of Object)()
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
        Dim UCData As Object = Nothing

        If Not App Is Nothing Then
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
                multi_collapse_default = multiCollapseStatus(App.ProjectID & "_" + stepId)
            End If

            Dim user_id = App.ActiveUser.UserID

            If AnytimeClass.UserIsReadOnly() Then
                user_id = AnytimeClass.GetReadOnlyUserID()
            End If

            isPM = App.CanUserModifyProject(user_id, App.ProjectID, Uw, Ws, App.ActiveWorkgroup)
            'Dim nodes = App.ActiveProject.HierarchyObjectives.Nodes
            ' stepId = 2
            For Each Node As clsNode In App.ActiveProject.HierarchyObjectives.Nodes
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
            doneOptions.logout = App.ActiveProject.PipeParameters.LogOffAtTheEnd
            doneOptions.redirect = App.ActiveProject.PipeParameters.RedirectAtTheEnd
            doneOptions.url = App.ActiveProject.PipeParameters.TerminalRedirectURL
            doneOptions.closeTab = App.ActiveProject.ProjectManager.Parameters.EvalCloseWindowAtFinish
            doneOptions.openProject = App.ActiveProject.ProjectManager.Parameters.EvalOpenNextProjectAtFinish
            doneOptions.stayAtEval = Not App.ActiveProject.PipeParameters.RedirectAtTheEnd AndAlso Not App.ActiveProject.ProjectManager.Parameters.EvalCloseWindowAtFinish AndAlso Not App.ActiveProject.ProjectManager.Parameters.EvalOpenNextProjectAtFinish

            Dim pipeOptions As pipeOptionsModel = New pipeOptionsModel()
            pipeOptions.hideNavigation = Not App.ActiveProject.PipeParameters.ShowProgressIndicator
            pipeOptions.disableNavigation = Not App.ActiveProject.PipeParameters.AllowNavigation
            pipeOptions.showUnassessed = App.ActiveProject.PipeParameters.ShowNextUnassessed
            pipeOptions.dontAllowMissingJudgment = Not App.ActiveProject.PipeParameters.AllowMissingJudgments

            If HttpContext.Current.Session(SessionAutoAdvance + "" + Convert.ToString(App.ProjectID)) Is Nothing Then
                is_auto_advance = App.ActiveProject.PipeParameters.AllowAutoadvance
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
            Dim isDataInstance = False
            Dim recreatePipe = False

            If AnytimeClass.UserIsReadOnly() Then
                Dim CurrentUser = App.DBUserByID(AnytimeClass.GetReadOnlyUserID())
                user_email = CurrentUser.UserEmail
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
                    InformationUserCtrl.GetInformationPageData(StepNode, AnytimeAction, App, CurrentStep, pipeHelpUrl, qh_help_id, InformationMessage)

                Case ActionType.atPairwise
                    ParentNodeID = PairwiseUserCtrl.GetPairWiseData(AnytimeAction, App, StepNode, Comment, PairwiseType, pipeHelpUrl, qh_help_id, PipeWarning, infodoc_params, question, wording, parent_node_info, first_node_info,
                   second_node_info, wrt_first_node_info, wrt_second_node_info, FirstNodeName, SecondNodeName, ParentNodeName, PWValue, PWAdvantage, IsUndefined, ParentNodeGUID, LeftNodeGUID, RightNodeGUID, ParentNodeID)
                Case ActionType.atAllPairwise
                    AllPairWiseUserCtrl.GetAllPairWiseData(AnytimeAction, App, StepNode, multi_GUIDs, PairwiseType, MultiPW_Data, ParentNodeName, ParentNodeGUID, parent_node_info, pipeHelpUrl, ParentNodeID, infodoc_params)
                Case ActionType.atAllPairwiseOutcomes
                    AllPairWiseUserCtrl.GetAllPairWiseData(AnytimeAction, App, StepNode, multi_GUIDs, PairwiseType, MultiPW_Data, ParentNodeName, ParentNodeGUID, parent_node_info, pipeHelpUrl, ParentNodeID, infodoc_params)

                    '5.atNonPWOneAtATime'
                Case ActionType.atNonPWOneAtATime
                    NonPairWiseOneAtATimeUserCtrl.GetNonPairWiseOneAtATimeData(AnytimeAction, App, StepNode)

                    '6.atNonPWAllChildren'
                Case ActionType.atNonPWAllChildren

                    '7.atNonPWAllCovObjs'
                Case ActionType.atNonPWAllCovObjs

                    NonPairWiseAllCovObjsUserCtrl.GetNonPairWiseAllCovObjData(AnytimeAction, App, StepNode)
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
                    model = localresults.GetDetails(AnytimeAction)
                    StepNode = model.StepNode
                    StepTask = model.StepTask
                    ParentNodeID = model.ParentNodeID
                    ParentNodeGUID = model.ParentNodeGUID
                    PipeParameters = model.PipeParameters
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

            End Select

            If HttpContext.Current.Session(InconsistencySortingEnabled) Is Nothing Then
                HttpContext.Current.Session(InconsistencySortingEnabled) = False
                HttpContext.Current.Session(BestFit) = False
            End If

            If StepNode IsNot Nothing Then
                qh_tnode_id = If(StepNode.IsAlternative, -StepNode.NodeID, StepNode.NodeID)
            Else
            End If
            ''Dim AutoShow As Boolean = show_qh_automatically
            'Dim isCluster As Boolean = False
            qh_info = App.ActiveProject.ProjectManager.PipeParameters.PipeMessages.GetEvaluationQuickHelpText(App.ActiveProject.ProjectManager, stepId, False, False)
            ''show_qh_automatically = AutoShow
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
            'Dim defaultQhInfo As String = GetDefaultQhInfo(App, qh_help_id)

            If HttpContext.Current.Session(qh_yt_info) IsNot Nothing Then
                is_qh_shown = True
            End If

            Dim steps = ""
            If PreviousStep <> stepId AndAlso Not (PreviousStep < 1 OrElse PreviousStep > App.ActiveProject.Pipe.Count) Then steps = AnytimeClass.get_StepInformation(App, PreviousStep)

            'Dim extremeMessage = Context.Request.Cookies(Constants.Cook_Extreme) IsNot Nothing
            'Dim showAutoAdvanceModal1 As Boolean = showAutoAdvanceModal(App, AnytimeClass.Action(stepId), "", "")
            'Dim userControlContent = If(AnytimeAction.ActionType <> ActionType.atInformationPage, getUserControl(stepId), "")
            'Dim HasCurStepCluster As Boolean = False

            Dim QHApplySteps = New List(Of Integer)()
            Dim ApplyNodes = New List(Of clsNode)()
            ApplyNodes = App.ActiveProject.ProjectManager.PipeBuilder.GetPipeStepClusters(stepId, False, QHApplySteps)
            'Dim apply_to = New List(Of String)()
            'Dim qqq = ApplyNodes
            Dim cluster_nodes = New List(Of String())()
            Dim n As Integer = 0

            For Each node As clsNode In ApplyNodes
                Dim arr As String() = {node.NodeName.ToString(), QHApplySteps(n).ToString(), node.NodeID.ToString(), node.ParentNodeID.ToString()}
                cluster_nodes.Add(arr)
                n += 1
            Next

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

            parent_node_info = RemoveInfoDcoImageStyle(parent_node_info)
            HttpContext.Current.Session(SessionParentNodeGuid) = ParentNodeGUID


            'output.hashLink = hashLink
            'output.status = "active"
            'output.pipeOptions = pipeOptions
            'output.showinfodocnode = App.ActiveProject.PipeParameters.ShowInfoDocs
            'output.current_step = stepId
            'output.previous_step = PreviousStep
            'output.total_pipe_steps = App.ActiveProject.Pipe.Count
            'output.is_first_time = isFirstTime
            'output.show_auto_advance_modal = showAutoAdvanceModal(App, AnytimeClass.Action(stepId), "", "")
            'output.first_unassessed_step = GetFirstUnassessed(App)
            'output.help_pipe_root = TeamTimeClass.ResString("help_pipe_root")
            'output.help_pipe_url = ""
            'output.page_type = AnytimeAction.ActionType.ToString()
            'output.pairwise_type = ""
            'output.first_node = ""
            'output.second_node = ""
            'output.parent_node = ""
            'output.first_node_info = ""
            'output.second_node_info = ""
            'output.parent_node_info = If(isHTMLEmpty(parent_node_info), "", parent_node_info)
            'output.wrt_first_node_info = ""
            'output.wrt_second_node_info = ""
            'output.ScaleDescriptions = Nothing
            'output.question = question
            'output.wording = ""
            'output.nameAlternatives = App.ActiveProject.PipeParameters.NameAlternatives
            'output.information_message = InformationMessage
            'output.step_task = StepTask
            'output.value = -1
            'output.advantage = -1
            'output.IsUndefined = False
            'output.sRes = If(HttpContext.Current.Session("sRes") Is Nothing, "", App.ResString(CStr(HttpContext.Current.Session("sRes"))))
            'output.current_value = -1
            'output.comment = ""
            'output.show_comments = show_comments
            'output.ShowUnassessed = AnytimeClass.ShowUnassessed
            'output.nextUnassessedStep = GetNextUnassessed(stepId)
            'output.steps = steps
            'output.stepButtons = GeckoClass.loadStepButtons(CurrentStep)
            'output.usersComments = ""
            'output.currentUserEmail = App.ActiveUser.UserEmail
            'output.extremeMessage = HttpContext.Current.Request.Cookies(Constants.Cook_Extreme) IsNot Nothing
            'output.pipeWarning = PipeWarning
            'output.sess_wrt_node_id = (CType(HttpContext.Current.Session(Sess_WrtNode), clsNode)).NodeID
            'output.parentnodeID = ParentNodeID
            'output.orderbypriority = CBool(HttpContext.Current.Session(InconsistencySortingEnabled))
            'output.bestfit = CBool(HttpContext.Current.Session(BestFit))
            'output.dont_show = CBool(HttpContext.Current.Session("showMessage"))
            'output.multi_GUIDs = Nothing
            'output.multi_pw_data = Nothing
            'output.multi_infodoc_params = Nothing
            'output.non_pw_type = ""
            'output.precision = 0
            'output.showPriorityAndDirect = True
            'output.child_node = ""
            'output.intensities = Nothing
            'output.non_pw_value = ""
            'output.is_direct = False
            'output.multi_non_pw_data = Nothing
            'output.multi_intensities = Nothing
            'output.step_intervals = Nothing
            'output.piecewise = False
            'output.judgment_made = judgment_made
            'output.overall = overall
            'output.total_evaluation = total_evaluation
            'output.ParentNodeGUID = ParentNodeGUID
            'output.LeftNodeGUID = Nothing
            'output.RightNodeGUID = Nothing
            'output.LeftNodeWrtGUID = Nothing
            'output.RightNodeWrtGUID = Nothing
            'output.infodoc_params = infodoc_params
            'output.is_auto_advance = is_auto_advance
            'output.PipeParameters = PipeParameters
            'output.doneOptions = doneOptions
            'output.is_infodoc_tooltip = is_infodoc_tooltip
            'output.defaultQhInfo = GetDefaultQhInfo(App, qh_help_id)
            'output.qh_info = qh_info
            'output.qh_help_id = qh_help_id
            'output.qh_tnode_id = qh_tnode_id
            'output.qh_yt_info = qh_yt_info
            'output.saType = saType
            'output.show_qh_automatically = False
            'output.is_qh_shown = is_qh_shown
            'output.dont_show_qh = If(getQHSettingCookies() = "False", False, True)
            'output.show_qh_setting = If(getQHSettingCookies() = "False", True, False)
            'output.multi_collapse_default = multi_collapse_default
            'output.cluster_phrase = HttpContext.Current.Session("ClusterPhrase")
            'output.nodes_data = NodesData
            'output.UCData = Nothing
            'output.read_only = AnytimeClass.UserIsReadOnly()
            'output.read_only_user = user_email
            'output.collapse_bars = App.ActiveProject.ProjectManager.Parameters.EvalCollapseMultiPWBars
            'output.userControlContent = If(AnytimeAction.ActionType <> ActionType.atInformationPage, getUserControl(stepId), "")
            'output.isPM = App.CanUserModifyProject(user_id, App.ProjectID, Uw, Ws, App.ActiveWorkgroup)
            'output.cluster_nodes = cluster_nodes
            'output.has_cluster = False
            'output.name = App.ActiveProject.ProjectName
            'output.owner = App.ActiveProject.ProjectManager.User.UserName
            'output.passcode = App.ActiveProject.Passcode
            'output.hideBrowserWarning = hideWarning
            'output.autoFitInfoDocImages = App.ActiveProject.ProjectManager.Parameters.AutoFitInfoDocImages
            'output.autoFitInfoDocImagesOptionText = TeamTimeClass.ResString("optImagesZoom")
            'output.framed_info_docs = App.ActiveProject.ProjectManager.Parameters.ShowFramedInfodocsMobile
            'output.hideInfoDocCaptions = App.ActiveProject.ProjectManager.Parameters.EvalHideInfodocCaptions
            'output.fromComparion = CBool(HttpContext.Current.Session(Constants.Sess_FromComparion))
            'output.nextProject = nextProjectId
            'output.isPipeViewOnly = CBool(HttpContext.Current.Session(Constants.SessionIsPipeViewOnly))
            'output.isPipeStepFound = CBool(HttpContext.Current.Session(Constants.SessionIsInterResultStepFound))



            'output = New With {
            '    Key .hashLink = hashLink,
            '    Key .status = "active",
            '    Key .pipeOptions = pipeOptions,
            '    Key .showinfodocnode = App.ActiveProject.PipeParameters.ShowInfoDocs,
            '    Key .current_step = stepId,
            '    Key .previous_step = PreviousStep,
            '    Key .total_pipe_steps = App.ActiveProject.Pipe.Count,
            '    Key .is_first_time = isFirstTime,
            '                   Key .first_unassessed_step = firstUnassessedStep,
            '    Key .help_pipe_root = TeamTimeClass.ResString("help_pipe_root"),
            '    Key .help_pipe_url = pipeHelpUrl,
            '    Key .page_type = AnytimeAction.ActionType.ToString(),
            '    Key .pairwise_type = PairwiseType,
            '    Key .first_node = FirstNodeName,
            '    Key .second_node = SecondNodeName,
            '    Key .parent_node = ParentNodeName,
            '    Key .first_node_info = If(StringFuncs.isHTMLEmpty(first_node_info), "", first_node_info),
            '    Key .second_node_info = If(StringFuncs.isHTMLEmpty(second_node_info), "", second_node_info),
            '    Key .parent_node_info = If(StringFuncs.isHTMLEmpty(parent_node_info), "", parent_node_info),
            '    Key .wrt_first_node_info = If(StringFuncs.isHTMLEmpty(wrt_first_node_info), "", wrt_first_node_info),
            '    Key .wrt_second_node_info = If(StringFuncs.isHTMLEmpty(wrt_second_node_info), "", wrt_second_node_info),
            '    Key .ScaleDescriptions = scaleDescriptions,
            '    Key .question = question,
            '    Key .wording = wording,
            '    Key .nameAlternatives = App.ActiveProject.PipeParameters.NameAlternatives,
            '    Key .information_message = InformationMessage,
            '    Key .step_task = StepTask,
            '    Key .value = PWValue,
            '    Key .advantage = PWAdvantage,
            '    Key .IsUndefined = IsUndefined,
            '    Key .sRes = If(HttpContext.Current.Session("sRes") Is Nothing, "", App.ResString(CStr(HttpContext.Current.Session("sRes")))),
            '    Key .current_value = CurrentValue,
            '    Key .comment = Comment,
            '    Key .show_comments = show_comments,
            '    Key .ShowUnassessed = AnytimeClass.ShowUnassessed,
            '    Key .nextUnassessedStep = GetNextUnassessed(stepId),
            '    Key .steps = steps,
            '    Key .stepButtons = GeckoClass.loadStepButtons(CurrentStep),
            '    Key .usersComments = "",
            '    Key .currentUserEmail = App.ActiveUser.UserEmail,
            '    Key .pipeWarning = PipeWarning,
            '    Key .sess_wrt_node_id = (CType(HttpContext.Current.Session(Sess_WrtNode), clsNode)).NodeID,
            '    Key .parentnodeID = ParentNodeID,
            '    Key .orderbypriority = CBool(HttpContext.Current.Session(InconsistencySortingEnabled)),
            '    Key .bestfit = CBool(HttpContext.Current.Session(BestFit)),
            '    Key .dont_show = CBool(HttpContext.Current.Session("showMessage")),
            '    Key .multi_GUIDs = multi_GUIDs,
            '    Key .multi_pw_data = MultiPW_Data,
            '    Key .multi_infodoc_params = multi_infodoc_params,
            '    Key .non_pw_type = NonPWType,
            '    Key .precision = precision,
            '    Key .showPriorityAndDirect = showPriorityAndDirectValue,
            '    Key .child_node = ChildNodeName,
            '    Key .intensities = intensities,
            '    Key .non_pw_value = NonPWValue,
            '    Key .is_direct = is_direct,
            '    Key .multi_non_pw_data = MultiNonPW_Data,
            '    Key .multi_intensities = multi_intensities,
            '    Key .step_intervals = step_intervals,
            '    Key .piecewise = piecewise,
            '    Key .judgment_made = judgment_made,
            '    Key .overall = overall,
            '    Key .total_evaluation = total_evaluation,
            '    Key .ParentNodeGUID = ParentNodeGUID,
            '    Key .LeftNodeGUID = LeftNodeGUID,
            '    Key .RightNodeGUID = RightNodeGUID,
            '    Key .LeftNodeWrtGUID = LeftNodeWrtGUID,
            '    Key .RightNodeWrtGUID = RightNodeWrtGUID,
            '    Key .infodoc_params = infodoc_params,
            '    Key .is_auto_advance = is_auto_advance,
            '    Key .PipeParameters = PipeParameters,
            '    Key .doneOptions = doneOptions,
            '    Key .is_infodoc_tooltip = is_infodoc_tooltip,
            '    Key .qh_info = qh_info,
            '    Key .qh_help_id = qh_help_id,
            '    Key .qh_tnode_id = qh_tnode_id,
            '    Key .qh_yt_info = qh_yt_info,
            '    Key .saType = saType,
            '    Key .show_qh_automatically = show_qh_automatically,
            '    Key .is_qh_shown = is_qh_shown,
            '    Key .dont_show_qh = dont_show_qh,
            '    Key .show_qh_setting = show_qh_setting,
            '    Key .multi_collapse_default = multi_collapse_default,
            '    Key .cluster_phrase = HttpContext.Current.Session("ClusterPhrase"),
            '    Key .nodes_data = NodesData,
            '    Key .UCData = UCData,
            '    Key .read_only = AnytimeClass.UserIsReadOnly(),
            '    Key .read_only_user = user_email,
            '    Key .collapse_bars = App.ActiveProject.ProjectManager.Parameters.EvalCollapseMultiPWBars,
            'Key .isPM = App.CanUserModifyProject(user_id, App.ProjectID, Uw, Ws, App.ActiveWorkgroup),
            '    Key .cluster_nodes = cluster_nodes,
            'Key .name = App.ActiveProject.ProjectName,
            '    Key .owner = App.ActiveProject.ProjectManager.User.UserName,
            '    Key .passcode = App.ActiveProject.Passcode,
            '    Key .hideBrowserWarning = hideWarning,
            '    Key .autoFitInfoDocImages = App.ActiveProject.ProjectManager.Parameters.AutoFitInfoDocImages,
            '    Key .autoFitInfoDocImagesOptionText = TeamTimeClass.ResString("optImagesZoom"),
            '    Key .framed_info_docs = App.ActiveProject.ProjectManager.Parameters.ShowFramedInfodocsMobile,
            '    Key .hideInfoDocCaptions = App.ActiveProject.ProjectManager.Parameters.EvalHideInfodocCaptions,
            '    Key .fromComparion = CBool(HttpContext.Current.Session(Constants.Sess_FromComparion)),
            '    Key .nextProject = nextProjectId,
            '    Key .isPipeViewOnly = CBool(HttpContext.Current.Session(Constants.SessionIsPipeViewOnly)),
            '    Key .isPipeStepFound = CBool(HttpContext.Current.Session(Constants.SessionIsInterResultStepFound))
            '}
            '' Key .userControlContent = userControlContent,
            ''Key .has_cluster = HasCurStepCluster,
            '' Key.defaultQhInfo = defaultQhInfo,
            '' Key .extremeMessage = extremeMessage,
            '' Key .show_auto_advance_modal = showAutoAdvanceModal1,

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
            output.help_pipe_url = ""
            output.page_type = AnytimeAction.ActionType.ToString()
            output.pairwise_type = ""
            output.first_node = ""
            output.second_node = ""
            output.parent_node = ""
            output.first_node_info = ""
            output.second_node_info = ""
            output.parent_node_info = If(isHTMLEmpty(parent_node_info), "", parent_node_info)
            output.wrt_first_node_info = ""
            output.wrt_second_node_info = ""
            output.ScaleDescriptions = Nothing
            output.question = question
            output.wording = ""
            output.nameAlternatives = App.ActiveProject.PipeParameters.NameAlternatives
            output.information_message = InformationMessage
            output.step_task = StepTask
            output.value = -1
            output.advantage = -1
            output.IsUndefined = False
            output.sRes = If(HttpContext.Current.Session("sRes") Is Nothing, "", App.ResString(CStr(HttpContext.Current.Session("sRes"))))
            output.current_value = -1
            output.comment = ""
            output.show_comments = show_comments
            output.ShowUnassessed = AnytimeClass.ShowUnassessed
            output.nextUnassessedStep = GetNextUnassessed(stepId)
            output.steps = steps
            output.stepButtons = GeckoClass.loadStepButtons(CurrentStep)
            output.usersComments = ""
            output.currentUserEmail = App.ActiveUser.UserEmail
            output.extremeMessage = HttpContext.Current.Request.Cookies(Constants.Cook_Extreme) IsNot Nothing
            output.pipeWarning = PipeWarning
            output.sess_wrt_node_id = (CType(HttpContext.Current.Session(Sess_WrtNode), clsNode)).NodeID
            output.parentnodeID = ParentNodeID
            output.orderbypriority = CBool(HttpContext.Current.Session(InconsistencySortingEnabled))
            output.bestfit = CBool(HttpContext.Current.Session(BestFit))
            output.dont_show = CBool(HttpContext.Current.Session("showMessage"))
            output.multi_GUIDs = Nothing
            output.multi_pw_data = Nothing
            output.multi_infodoc_params = Nothing
            output.non_pw_type = ""
            output.precision = 0
            output.showPriorityAndDirect = True
            output.child_node = ""
            output.intensities = Nothing
            output.non_pw_value = ""
            output.is_direct = False
            output.multi_non_pw_data = Nothing
            output.multi_intensities = Nothing
            output.step_intervals = Nothing
            output.piecewise = False
            output.judgment_made = judgment_made
            output.overall = overall
            output.total_evaluation = total_evaluation
            output.ParentNodeGUID = ParentNodeGUID
            output.LeftNodeGUID = Nothing
            output.RightNodeGUID = Nothing
            output.LeftNodeWrtGUID = Nothing
            output.RightNodeWrtGUID = Nothing
            output.infodoc_params = infodoc_params
            output.is_auto_advance = is_auto_advance
            output.PipeParameters = PipeParameters
            output.doneOptions = doneOptions
            output.is_infodoc_tooltip = is_infodoc_tooltip
            output.defaultQhInfo = GetDefaultQhInfo(App, qh_help_id)
            output.qh_info = qh_info
            output.qh_help_id = qh_help_id
            output.qh_tnode_id = qh_tnode_id
            output.qh_yt_info = qh_yt_info
            output.saType = saType
            output.show_qh_automatically = False
            output.is_qh_shown = is_qh_shown
            output.dont_show_qh = If(getQHSettingCookies() = "False", False, True)
            output.show_qh_setting = If(getQHSettingCookies() = "False", True, False)
            output.multi_collapse_default = multi_collapse_default
            output.cluster_phrase = HttpContext.Current.Session("ClusterPhrase")
            output.nodes_data = NodesData
            output.UCData = Nothing
            output.read_only = AnytimeClass.UserIsReadOnly()
            output.read_only_user = user_email
            output.collapse_bars = App.ActiveProject.ProjectManager.Parameters.EvalCollapseMultiPWBars
            output.userControlContent = If(AnytimeAction.ActionType <> ActionType.atInformationPage, getUserControl(stepId), "")
            output.isPM = App.CanUserModifyProject(user_id, App.ProjectID, Uw, Ws, App.ActiveWorkgroup)
            output.cluster_nodes = cluster_nodes
            output.has_cluster = False
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

            'output = New With {
            '    Key .hashLink = hashLink,
            '    Key .status = "active",
            '    Key .pipeOptions = pipeOptions,
            '    Key .showinfodocnode = App.ActiveProject.PipeParameters.ShowInfoDocs,
            '    Key .current_step = stepId,
            '    Key .previous_step = PreviousStep,
            '    Key .total_pipe_steps = App.ActiveProject.Pipe.Count,
            '    Key .is_first_time = isFirstTime,
            '    Key .show_auto_advance_modal = showAutoAdvanceModal(App, AnytimeClass.Action(stepId), "", ""),
            '    Key .first_unassessed_step = GetFirstUnassessed(App),
            '    Key .help_pipe_root = TeamTimeClass.ResString("help_pipe_root"),
            '    Key .help_pipe_url = "",
            '    Key .page_type = AnytimeAction.ActionType.ToString(),
            '    Key .pairwise_type = "",
            '    Key .first_node = "",
            '    Key .second_node = "",
            '    Key .parent_node = "",
            '    Key .first_node_info = "",
            '    Key .second_node_info = "",
            '    Key .parent_node_info = If(StringFuncs.isHTMLEmpty(parent_node_info), "", parent_node_info),
            '    Key .wrt_first_node_info = "",
            '    Key .wrt_second_node_info = "",
            '    Key .ScaleDescriptions = "",
            '    Key .question = question,
            '    Key .wording = "",
            '    Key .nameAlternatives = App.ActiveProject.PipeParameters.NameAlternatives,
            '    Key .information_message = InformationMessage,
            '    Key .step_task = StepTask,
            '    Key .value = -1,
            '    Key .advantage = -1,
            '    Key .IsUndefined = False,
            '    Key .sRes = If(Context.Session("sRes") Is Nothing, "", App.ResString(CStr(Context.Session("sRes")))),
            '    Key .current_value = -1,
            '    Key .comment = "",
            '    Key .show_comments = show_comments,
            '    Key .ShowUnassessed = AnytimeClass.ShowUnassessed,
            '    Key .nextUnassessedStep = GetNextUnassessed(stepId),
            '    Key .steps = steps,
            '    Key .stepButtons = GeckoClass.loadStepButtons(CurrentStep),
            '    Key .usersComments = "",
            '    Key .currentUserEmail = App.ActiveUser.UserEmail,
            '    Key .extremeMessage = Context.Request.Cookies(Constants.Cook_Extreme) IsNot Nothing,
            '    Key .pipeWarning = PipeWarning,
            '    Key .sess_wrt_node_id = (CType(Context.Session(Sess_WrtNode), clsNode)).NodeID,
            '    Key .parentnodeID = ParentNodeID,
            '    Key .orderbypriority = CBool(HttpContext.Current.Session(InconsistencySortingEnabled)),
            '    Key .bestfit = CBool(HttpContext.Current.Session(BestFit)),
            '    Key .dont_show = CBool(HttpContext.Current.Session("showMessage")),
            '    Key .multi_GUIDs = "",
            '    Key .multi_pw_data = "",
            '    Key .multi_infodoc_params = "",
            '    Key .non_pw_type = "",
            '    Key .precision = 0,
            '    Key .showPriorityAndDirect = True,
            '    Key .child_node = "",
            '    Key .intensities = "",
            '    Key .non_pw_value = "",
            '    Key .is_direct = False,
            '    Key .multi_non_pw_data = "",
            '    Key .multi_intensities = "",
            '    Key .step_intervals = "",
            '    Key .piecewise = False,
            '    Key .judgment_made = judgment_made,
            '    Key .overall = overall,
            '    Key .total_evaluation = total_evaluation,
            '    Key .ParentNodeGUID = ParentNodeGUID,
            '    Key .LeftNodeGUID = "",
            '    Key .RightNodeGUID = "",
            '    Key .LeftNodeWrtGUID = "",
            '    Key .RightNodeWrtGUID = "",
            '    Key .infodoc_params = infodoc_params,
            '    Key .is_auto_advance = is_auto_advance,
            '    Key .PipeParameters = PipeParameters,
            '    Key .doneOptions = doneOptions,
            '    Key .is_infodoc_tooltip = is_infodoc_tooltip,
            '    Key .defaultQhInfo = GetDefaultQhInfo(App, qh_help_id),
            '    Key .qh_info = qh_info,
            '    Key .qh_help_id = qh_help_id,
            '    Key .qh_tnode_id = qh_tnode_id,
            '    Key .qh_yt_info = qh_yt_info,
            '    Key .saType = saType,
            '    Key .show_qh_automatically = False,
            '    Key .is_qh_shown = is_qh_shown,
            '    Key .dont_show_qh = If(getQHSettingCookies() = "False", False, True),
            '    Key .show_qh_setting = If(getQHSettingCookies() = "False", True, False),
            '    Key .multi_collapse_default = multi_collapse_default,
            '    Key .cluster_phrase = Context.Session("ClusterPhrase"),
            '    Key .nodes_data = NodesData,
            '    Key .UCData = Nothing,
            '    Key .read_only = AnytimeClass.UserIsReadOnly(),
            '    Key .read_only_user = user_email,
            '    Key .collapse_bars = App.ActiveProject.ProjectManager.Parameters.EvalCollapseMultiPWBars,
            '    Key .userControlContent = If(AnytimeAction.ActionType <> ActionType.atInformationPage, getUserControl(stepId), ""),
            '    Key .isPM = App.CanUserModifyProject(user_id, App.ProjectID, Uw, Ws, App.ActiveWorkgroup),
            '    Key .cluster_nodes = cluster_nodes,
            '    Key .has_cluster = False,
            '    Key .name = App.ActiveProject.ProjectName,
            '    Key .owner = App.ActiveProject.ProjectManager.User.UserName,
            '    Key .passcode = App.ActiveProject.Passcode,
            '    Key .hideBrowserWarning = hideWarning,
            '    Key .autoFitInfoDocImages = App.ActiveProject.ProjectManager.Parameters.AutoFitInfoDocImages,
            '    Key .autoFitInfoDocImagesOptionText = TeamTimeClass.ResString("optImagesZoom"),
            '    Key .framed_info_docs = App.ActiveProject.ProjectManager.Parameters.ShowFramedInfodocsMobile,
            '    Key .hideInfoDocCaptions = App.ActiveProject.ProjectManager.Parameters.EvalHideInfodocCaptions,
            '    Key .fromComparion = CBool(Context.Session(Constants.Sess_FromComparion)),
            '    Key .nextProject = nextProjectId,
            '    Key .isPipeViewOnly = CBool(Context.Session(Constants.SessionIsPipeViewOnly)),
            '    Key .isPipeStepFound = CBool(Context.Session(Constants.SessionIsInterResultStepFound))
            '}

        End If

        Try
            'Dim oSerializer = New JavaScriptSerializer()
            'Return oSerializer.Serialize(output)
            Return output
        Catch e As Exception
            'Dim [error] = e
            'Return JsonConvert.SerializeObject(output, Formatting.Indented, New JsonSerializerSettings With {
            '    .ReferenceLoopHandling = ReferenceLoopHandling.Serialize
            '})
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
                userControlUrl = "~/pages/anytime/Pairwise.ascx"
            Case ActionType.atPairwise, ActionType.atPairwiseOutcomes
                If cookie.Values("pairwise") = "1" Then Return False
                cookie.Values("pairwise") = "1"
                userControlUrl = "~/pages/anytime/Pairwise.ascx"
            Case ActionType.atAllPairwise, ActionType.atAllPairwiseOutcomes
                If cookie.Values("multiPairwise") = "1" Then Return False
                cookie.Values("multiPairwise") = "1"
                userControlUrl = "~/Pages/Anytime/AllPairWiseUserCtrl.ascx"
            Case ActionType.atNonPWOneAtATime
                Dim measuretype = CType(action.ActionData, clsNonPairwiseEvaluationActionData)

                Select Case (CType(action.ActionData, clsNonPairwiseEvaluationActionData)).MeasurementType
                    Case ECMeasureType.mtDirect
                        If cookie.Values("direct") = "1" Then Return False
                        cookie.Values("direct") = "1"
                        userControlUrl = "~/Pages/Anytime/DirectComparison.ascx"
                    Case ECMeasureType.mtRatings
                        If cookie.Values("ratings") = "1" Then Return False
                        cookie.Values("ratings") = "1"
                        userControlUrl = "~/Pages/Anytime/Ratings.ascx"
                    Case ECMeasureType.mtStep
                        If cookie.Values("stepFunction") = "1" Then Return False
                        cookie.Values("stepFunction") = "1"
                        userControlUrl = "~/Pages/Anytime/StepFunction.ascx"
                    Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtCustomUtilityCurve
                        If cookie.Values("utility") = "1" Then Return False
                        cookie.Values("utility") = "1"
                        userControlUrl = "~/Pages/Anytime/UtilityCurve.ascx"
                End Select

            Case ActionType.atNonPWAllChildren, ActionType.atNonPWAllCovObjs
                Dim measuretype = CType(action.ActionData, clsNonPairwiseEvaluationActionData)

                Select Case (CType(action.ActionData, clsNonPairwiseEvaluationActionData)).MeasurementType
                    Case ECMeasureType.mtDirect
                        If cookie.Values("multiDirect") = "1" Then Return False
                        cookie.Values("multiDirect") = "1"
                        userControlUrl = "~/Pages/Anytime/MultiDirect.ascx"
                    Case ECMeasureType.mtRatings
                        If cookie.Values("multiRatings") = "1" Then Return False
                        cookie.Values("multiRatings") = "1"
                        userControlUrl = "~/Pages/Anytime/MultiRatings.ascx"
                End Select

            Case ActionType.atSpyronSurvey, ActionType.atSurvey
                If context.Request.Cookies("loadedScreens")("survey") = "1" Then Return False
                context.Response.Cookies("loadedScreens")("survey") = "1"
                userControlUrl = "~/pages/anytime/Survey.ascx"
            Case ActionType.atSensitivityAnalysis
                If context.Request.Cookies("loadedScreens")("sensitivity") = "1" Then Return False
                context.Response.Cookies("loadedScreens")("sensitivity") = "1"
                userControlUrl = "~/pages/anytime/SensitivitiesAnalysis.ascx"
            Case ActionType.atShowLocalResults
                If cookie.Values("localResults") = "1" Then Return False
                cookie.Values("localResults") = "1"
                userControlUrl = "~/pages/anytime/localresults.ascx"
            Case ActionType.atShowGlobalResults
                If cookie.Values("globalResults") = "1" Then Return False
                cookie.Values("globalResults") = "1"
                userControlUrl = "~/Pages/Anytime/GlobalResults.ascx"
        End Select

        context.Response.AppendCookie(cookie)

        Using objPage As Page = New Page()
            Dim uControl As UserControl = CType(objPage.LoadControl(userControlUrl), UserControl)
            objPage.Controls.Add(uControl)
            Using sWriter As StringWriter = New StringWriter()
                context.Server.Execute(objPage, sWriter, False)
                Return sWriter.ToString()
            End Using
        End Using
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
        Dim judgmentsCount As Integer = HttpContext.Current.Session(SessionAutoAdvanceJudgmentsCount + app.ProjectID.ToString())
        Dim isAutoAdvance As Boolean = HttpContext.Current.Session(SessionAutoAdvance + app.ProjectID.ToString())
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

    <WebMethod(EnableSession:=True)>
    Public Shared Function GetNextUnassessed(ByVal StartingStep As Integer) As Integer()
        Dim App = CType(HttpContext.Current.Session("App"), clsComparionCore)
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
            Dim app = CType(HttpContext.Current.Session("App"), clsComparionCore)
            Return app.ActiveUserWorkgroup
        End Get
    End Property

    Private Shared ReadOnly Property Ws As clsWorkspace
        Get
            Dim app = CType(HttpContext.Current.Session("App"), clsComparionCore)
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
        Dim App = CType(HttpContext.Current.Session("App"), clsComparionCore)

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
        Dim App = CType(HttpContext.Current.Session("App"), clsComparionCore)
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

                For i As Integer = 1 To (numChildren * (numChildren - 1)) / 2
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

                If CanShowHiddenExpectedValue Then

                    If bCanShowIndividual Then
                        ExpectedValueString(0) = " Your Expected value = " & (Math.Round(IndivExpectedValue, 2)).ToString()
                    End If

                    If bCanShowGroup Then
                        ExpectedValueString(1) = " Combined Expected value = " & (Math.Round(CombinedExpectedValue, 2)).ToString()
                    End If
                End If

                ExpectedValueString = New String(2) {}
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

    '<WebMethod(EnableSession:=True)>
    Public Shared Function GetOverallResultsData(ByVal rmode As Integer, ByVal wrtnodeID As Integer, ByVal Optional isReload As Boolean = False) As List(Of Object)
        Dim returnObject = New List(Of Object)()
        Dim context As HttpContext = HttpContext.Current
        Dim App = CType(context.Session("App"), clsComparionCore)
        Dim iStep As Integer = App.ActiveWorkspace.ProjectStep(App.ActiveProject.isImpact)
        Dim _with1 = App.ActiveProject.ProjectManager
        'Dim ps As clsAction = CType(_with1.Pipe(iStep - 1), clsAction)
        'ExpectedValueString = New String(2) {}
        Dim psLocal As clsShowGlobalResultsActionData = CType(AnytimeClass.GetAction(iStep).ActionData, clsShowGlobalResultsActionData)
        psLocal.WRTNode = App.ActiveProject.ProjectManager.Hierarchies(0).GetNodeByID(wrtnodeID)
        If isReload OrElse context.Session(Sess_WrtNode) Is Nothing Then context.Session(Sess_WrtNode) = psLocal.WRTNode
        Dim sEmail As String = App.ActiveUser.UserEmail

        If AnytimeClass.UserIsReadOnly() Then
            Dim CurrentUser = App.DBUserByID(AnytimeClass.GetReadOnlyUserID())
            sEmail = CurrentUser.UserEmail
        End If

        Dim AHPUser As clsUser = _with1.GetUserByEMail(sEmail)
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
            Key .individual = bCanShowIndividual AndAlso (_with1.PipeParameters.GlobalResultsView = CanvasTypes.ResultsView.rvIndividual OrElse _with1.PipeParameters.GlobalResultsView = CanvasTypes.ResultsView.rvBoth),
            Key .combined = bCanShowGroup AndAlso (_with1.PipeParameters.GlobalResultsView = CanvasTypes.ResultsView.rvGroup OrElse _with1.PipeParameters.GlobalResultsView = CanvasTypes.ResultsView.rvBoth)
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

                If AnytimeClass.CombinedUserID = COMBINED_USER_ID Then
                    mGroupResultsList = CType(psLocal.ResultsList(_with1.CombinedGroups.GetDefaultCombinedGroup().CombinedUserID, AHPUser.UserID, resultmodes).Clone(), ArrayList)
                Else
                    mGroupResultsList = CType(psLocal.ResultsList(AnytimeClass.CombinedUserID, AHPUser.UserID, resultmodes).Clone(), ArrayList)
                End If

                mResultsList = mGroupResultsList
            End If
        End If

        If _with1.PipeBuilder.PipeParameters.GlobalResultsView <> ResultsView.rvNone Then

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
                    Dim Res = New Objective()
                    Dim reuse = New List(Of Object)()
                    Dim listItem As clsResultsItem = CType(mResultsList(i), clsResultsItem)
                    reuse.Add((i + 1))
                    reuse.Add(listItem.Name)
                    Res.Name = listItem.Name

                    If psLocal.CanShowIndividualResults(AHPUserID, psLocal.WRTNode) Then
                        Dim R As clsResultsItem = CType(mIndividualResultsList(i), clsResultsItem)
                        reuse.Add(Convert.ToSingle((If(rmode = 1 OrElse rmode = 2, R.Value, R.UnnormalizedValue))).ToString())
                        Res.Value = If(rmode = 1, R.Value, R.UnnormalizedValue)
                        Res.GlobalValue = Res.Value * psLocal.WRTNode.WRTGlobalPriority
                    Else
                        reuse.Add("0")
                    End If

                    If bCanShowGroup And (_with1.PipeBuilder.PipeParameters.GlobalResultsView = ResultsView.rvGroup Or _with1.PipeBuilder.PipeParameters.GlobalResultsView = ResultsView.rvBoth) Then
                        Dim R As clsResultsItem = CType(mGroupResultsList(i), clsResultsItem)
                        reuse.Add(Convert.ToSingle((If(rmode = 1 OrElse rmode = 2, R.Value, R.UnnormalizedValue))).ToString())
                        Res.CombinedValue = If(rmode = 1, R.Value, R.UnnormalizedValue)
                        Res.GlobalValueCombined = Res.CombinedValue * psLocal.WRTNode.WRTGlobalPriority
                    Else
                        reuse.Add("0")
                    End If

                    reuse.Add(App.ActiveProject.PipeParameters.GlobalResultsView.ToString())
                    reslist.Add(reuse)

                    If psLocal.WRTNode.MeasureType() = ECMeasureType.mtPWAnalogous Then
                        Dim KnownLikelihoods As List(Of KnownLikelihoodDataContract) = psLocal.WRTNode.GetKnownLikelihoods()

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
        returnObject.Add(psLocal.WRTNode.NodeName)
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

        Dim App = CType(context.Session("App"), clsComparionCore)

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
        If Convert.ToInt32(hdnPageNumber.Value) > 0 Then
            BindUserControl(Convert.ToInt32(hdnPageNumber.Value))
        End If
    End Sub

    <WebMethod(EnableSession:=True)>
    Public Shared Function reviewJudgment(ByVal parentnodeID As Integer, ByVal current_step As Integer) As Integer
        Dim context As HttpContext = HttpContext.Current
        Dim App = CType(context.Session("App"), clsComparionCore)
        Dim CurrentNode = App.ActiveProject.HierarchyObjectives.GetNodeByID(parentnodeID)
        Return App.ActiveProject.ProjectManager.PipeBuilder.GetFirstEvalPipeStepForNode(CurrentNode, current_step) + 1
    End Function

End Class