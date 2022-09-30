Imports Canvas
Imports ECCore
Imports ExpertChoice.Data
Imports AnytimeComparion.Pages.external_classes
Imports System.Globalization
Imports ExpertChoice.Results
Imports System.Net
Imports Newtonsoft.Json
Imports ExpertChoice.Service
Imports SpyronControls.Spyron.Core
Imports System.Web.Services
Imports SpyronControls.Spyron.Data

Public Class AnytimeClass
    Public Const SessJudgments As String = "Judgments{0}"
    Private Const SessDatetime As String = "JudgmentsDateTime"
    Private Const SessSaved As String = "JudgmentsSaved"
    Private Const SessParentId As String = "JudgmentsNodeID"
    Private Const MimeIdentifier As String = "MIME"
    Private Const InconsistencySortingEnabled As String = "InconsistencySortingEnabled"
    Private Const SessionShowUnassessed As String = "SessionShowUnassessed"
    Private Const SessionObjectivesHighestResult As String = "SessionObjectivesHighestResult"

    Public Enum EcHierarchyId
        HidLikelihood = 0
        HidImpact = 2
    End Enum

    Public Shared Sub SetUser(ByVal tUser As ECTypes.clsUser, ByVal fCheckDataInstance As Boolean, ByVal fRecreatePipe As Boolean)
        Dim context As HttpContext = HttpContext.Current
        Dim app = CType(context.Session("App"), clsComparionCore)

        If fCheckDataInstance AndAlso IsDataInstance AndAlso app.ActiveProject.ProjectManager.DataInstances.Count > 0 Then
            Dim di = app.ActiveProject.ProjectManager.DataInstances(0)
            tUser = di.User
        End If

        If tUser IsNot Nothing AndAlso (app.ActiveProject.ProjectManager.User IsNot Nothing OrElse (app.ActiveProject.ProjectManager.User IsNot Nothing AndAlso tUser.UserID <> app.ActiveProject.ProjectManager.User.UserID)) Then
            app.ActiveProject.ProjectManager.User = tUser
            app.ActiveProject.LastModify = DateTime.Today
        End If

        Dim fForcePipeRebuild As Boolean = False
        Dim IsIntensities As Boolean = False

        If app.ActiveProject.HierarchyObjectives.HierarchyType = ECTypes.ECHierarchyType.htMeasure AndAlso Not IsIntensities Then
            app.ActiveProject.ProjectManager.ActiveHierarchy = If(app.ActiveProject.isImpact, CInt(ECTypes.ECHierarchyID.hidImpact), CInt(ECTypes.ECHierarchyID.hidLikelihood))
        End If

        app.ActiveProject.ProjectManager.ActiveHierarchy = If(app.ActiveProject.isImpact, CInt(ECTypes.ECHierarchyID.hidImpact), CInt(ECTypes.ECHierarchyID.hidLikelihood))

        If fRecreatePipe Then

            If app.ActiveProject.ProjectManager.ActiveHierarchy = 2 AndAlso Not Equals(app.ActiveProject.ProjectManager.PipeParameters.CurrentParameterSet, app.ActiveProject.ProjectManager.PipeParameters.ImpactParameterSet) Then
                app.ActiveProject.ProjectManager.PipeParameters.CurrentParameterSet = app.ActiveProject.ProjectManager.PipeParameters.ImpactParameterSet
            End If

            If app.ActiveProject.ProjectManager.ActiveHierarchy = 0 AndAlso Not Equals(app.ActiveProject.ProjectManager.PipeParameters.CurrentParameterSet, app.ActiveProject.ProjectManager.PipeParameters.DefaultParameterSet) Then
                app.ActiveProject.ProjectManager.PipeParameters.CurrentParameterSet = app.ActiveProject.ProjectManager.PipeParameters.DefaultParameterSet
            End If
        End If

        If app.ActiveProject.ProjectManager.User IsNot Nothing Then
        End If

        If Not UserIsReadOnly() AndAlso fRecreatePipe Then
            app.CheckAndAssignUserRole(app.ActiveProject, app.ActiveUser.UserEmail)
        End If

        If fRecreatePipe OrElse fForcePipeRebuild Then
            app.SurveysManager.ActiveWorkgroupID = app.ActiveWorkgroup.ID
            app.ActiveSurveysList = Nothing

            If app.isSpyronAvailable Then
                app.ActiveProject.ProjectManager.PipeBuilder.GetSurveyInfo = AddressOf app.SurveysManager.GetSurveyStepsCountByGUID
            End If

            app.ActiveProject.ProjectManager.PipeBuilder.PipeCreated = False

            If fForcePipeRebuild Then
                app.ActiveProject.ProjectManager.PipeBuilder.PipeCreated = False
            End If

            app.ActiveProject.ProjectManager.PipeBuilder.CreatePipe()
        End If
    End Sub

    Public Shared ReadOnly Property IsDataInstance As Boolean
        Get
            Return False
        End Get
    End Property

    Public Shared ReadOnly Property Uw As clsUserWorkgroup
        Get
            Dim _uw As clsUserWorkgroup
            Dim app = CType(HttpContext.Current.Session("App"), clsComparionCore)
            Dim tUserId = app.ActiveUser.UserID

            If UserIsReadOnly() Then
                tUserId = GetReadOnlyUserID()
            End If

            _uw = clsUserWorkgroup.UserWorkgroupByUserIDAndWorkgroupID(tUserId, app.ActiveWorkgroup.ID, app.UserWorkgroups)

            If _uw IsNot Nothing Then
                _uw = app.DBUserWorkgroupByUserIDWorkgroupID(tUserId, app.ActiveWorkgroup.ID)
            End If

            Return _uw
        End Get
    End Property

    Public Shared ReadOnly Property Ws As clsWorkspace
        Get
            Dim _ws As clsWorkspace
            Dim app = CType(HttpContext.Current.Session("App"), clsComparionCore)
            Dim tUserId = app.ActiveUser.UserID

            If UserIsReadOnly() Then
                tUserId = GetReadOnlyUserID()
            End If

            _ws = clsWorkspace.WorkspaceByUserIDAndProjectID(tUserId, app.ProjectID, app.Workspaces)

            If _ws Is Nothing Then
                _ws = app.DBWorkspaceByUserIDProjectID(tUserId, app.ProjectID)

                If UserIsReadOnly() Then
                    app.Workspaces.Add(_ws)
                End If
            End If

            If _ws Is Nothing Then
            End If

            Return _ws
        End Get
    End Property

    Public Shared Function GetAction(ByVal stepNumber As Integer) As clsAction
        Dim app = CType(HttpContext.Current.Session("App"), clsComparionCore)

        If Not app.HasActiveProject() OrElse stepNumber < 1 OrElse stepNumber > app.ActiveProject.Pipe.Count OrElse app.ActiveProject.Pipe.Count < 1 Then
            Return Nothing
        Else
            Dim action = CType(app.ActiveProject.Pipe(stepNumber - 1), clsAction)
            Return action
        End If
    End Function

    Public Shared Function UserIsReadOnly() As Boolean
        Return CBool(HttpContext.Current.Session(Constants.SessionIsPipeViewOnly))
    End Function

    Public Shared Function GetReadOnlyUserID() As Integer
        Return CInt(HttpContext.Current.Session(Constants.SessionViewOnlyUserId))
    End Function

    Public Shared Function Action(ByVal [step] As Integer) As clsAction
        Dim _action As clsAction = Nothing
        Dim app = CType(HttpContext.Current.Session("App"), clsComparionCore)

        If [step] > 0 AndAlso [step] <= app.ActiveProject.Pipe.Count Then
            _action = GetAction([step])
        Else
            Dim tUserId = app.ActiveUser.UserID

            If UserIsReadOnly() Then
                Dim tUser = app.DBUserByID(GetReadOnlyUserID())
                tUserId = tUser.UserID
            End If

            Dim anytimeUser = app.ActiveProject.ProjectManager.GetUserByEMail(app.ActiveUser.UserEmail)
            SetUser(anytimeUser, True, True)
            Dim workSpace = app.DBWorkspaceByUserIDProjectID(anytimeUser.UserID, app.ProjectID)
            'Dim currentStep = workSpace.get_ProjectStep(app.ActiveProject.isImpact)
            If Not workSpace Is Nothing Then
                Dim currentStep = workSpace.ProjectStep(app.ActiveProject.isImpact)

                If currentStep > 0 Then
                    _action = GetAction(currentStep)
                Else
                    _action = GetAction(1)
                End If
            End If
        End If

        Return _action
    End Function

    Public Shared Property ShowUnassessed As Boolean
        Get
            Dim show_Unassessed As Boolean = HttpContext.Current.Session(SessionShowUnassessed) IsNot Nothing AndAlso CBool(HttpContext.Current.Session(SessionShowUnassessed))
            Return show_Unassessed
        End Get
        Set(ByVal value As Boolean)
            HttpContext.Current.Session(SessionShowUnassessed) = value
        End Set
    End Property

    Public Shared Function IsUndefined(ByVal action As clsAction) As Boolean
        Dim undefined As Boolean = False

        Select Case action.ActionType
            Case ActionType.atPairwise
                Dim pw As clsPairwiseMeasureData = CType(action.ActionData, clsPairwiseMeasureData)
                Return pw.IsUndefined
            Case ActionType.atAllPairwise, ActionType.atAllPairwiseOutcomes
                Dim allPw As clsAllPairwiseEvaluationActionData = CType(action.ActionData, clsAllPairwiseEvaluationActionData)

                If allPw.Judgments IsNot Nothing Then

                    For Each tPw As clsPairwiseMeasureData In allPw.Judgments

                        If tPw.IsUndefined Then
                            undefined = True
                        End If
                    Next
                End If

                Return undefined
            Case ActionType.atNonPWOneAtATime
                Dim nonPwData As clsOneAtATimeEvaluationActionData = CType(action.ActionData, clsOneAtATimeEvaluationActionData)

                If nonPwData.Judgment IsNot Nothing Then
                    Dim nonPwJudgment As clsNonPairwiseMeasureData = CType(nonPwData.Judgment, clsNonPairwiseMeasureData)

                    If nonPwJudgment.IsUndefined Then
                        undefined = True
                    End If
                End If

                Return undefined
            Case ActionType.atNonPWAllChildren
                Dim nonAllPwData As clsAllChildrenEvaluationActionData = CType(action.ActionData, clsAllChildrenEvaluationActionData)

                If nonAllPwData.Children IsNot Nothing Then

                    For Each tAlt As clsNode In nonAllPwData.Children
                        Dim md As clsNonPairwiseMeasureData = nonAllPwData.GetJudgment(tAlt)

                        If md IsNot Nothing AndAlso md.IsUndefined Then
                            undefined = True
                        End If
                    Next
                End If

                Return undefined
            Case ActionType.atNonPWAllCovObjs
                Dim nonAllPwObjsData As clsAllCoveringObjectivesEvaluationActionData = CType(action.ActionData, clsAllCoveringObjectivesEvaluationActionData)

                If nonAllPwObjsData.CoveringObjectives IsNot Nothing Then

                    For Each tAlt As clsNode In nonAllPwObjsData.CoveringObjectives
                        Dim md As clsNonPairwiseMeasureData = nonAllPwObjsData.GetJudgment(tAlt)

                        If md IsNot Nothing AndAlso md.IsUndefined Then
                            undefined = True
                        End If
                    Next
                End If

                Return undefined
            Case ActionType.atAllEventsWithNoSource
                Dim allEventsData As clsAllEventsWithNoSourceEvaluationActionData = CType(action.ActionData, clsAllEventsWithNoSourceEvaluationActionData)

                If allEventsData.Alternatives IsNot Nothing Then

                    For Each tAlt As clsNode In allEventsData.Alternatives
                        Dim md As clsNonPairwiseMeasureData = allEventsData.GetJudgment(tAlt)

                        If md IsNot Nothing AndAlso md.IsUndefined Then
                            undefined = True
                        End If
                    Next
                End If

                Return undefined
            Case ActionType.atSpyronSurvey
                Return False
        End Select

        Return False
    End Function

    Public Shared Function GetStepStatus(ByVal action As clsAction) As Boolean
        If action IsNot Nothing AndAlso action.ActionData IsNot Nothing Then

            Select Case action.ActionType
                Case ActionType.atPairwise
                    Dim actionData As clsPairwiseMeasureData = CType(action.ActionData, clsPairwiseMeasureData)

                    If actionData.IsUndefined Then
                        Return True
                    End If

                Case ActionType.atPairwiseOutcomes
                    Dim actionData As clsPairwiseMeasureData = CType(action.ActionData, clsPairwiseMeasureData)

                    If actionData.IsUndefined Then
                        Return True
                    End If

                Case ActionType.atAllPairwise, ActionType.atAllPairwiseOutcomes
                    Dim allPwData As clsAllPairwiseEvaluationActionData = CType(action.ActionData, clsAllPairwiseEvaluationActionData)

                    For Each tJud As clsPairwiseMeasureData In allPwData.Judgments

                        If tJud.IsUndefined Then
                            Return True
                        End If
                    Next

                Case ActionType.atNonPWOneAtATime
                    Dim actionData As clsOneAtATimeEvaluationActionData = CType(action.ActionData, clsOneAtATimeEvaluationActionData)

                    If actionData.Node IsNot Nothing AndAlso actionData.Judgment IsNot Nothing Then

                        Select Case (CType(action.ActionData, clsNonPairwiseEvaluationActionData)).MeasurementType
                            Case ECMeasureType.mtRatings
                                Dim nonpwData As clsNonPairwiseMeasureData = CType(actionData.Judgment, clsNonPairwiseMeasureData)

                                If nonpwData.IsUndefined Then
                                    Return True
                                End If

                            Case ECMeasureType.mtDirect
                                Dim directData As clsDirectMeasureData = CType(actionData.Judgment, clsDirectMeasureData)

                                If directData.IsUndefined Then
                                    Return True
                                End If

                            Case ECMeasureType.mtStep
                                Dim stepData As clsStepMeasureData = CType(actionData.Judgment, clsStepMeasureData)

                                If stepData.IsUndefined Then
                                    Return True
                                End If

                            Case ECMeasureType.mtRegularUtilityCurve
                                Dim ucData As clsUtilityCurveMeasureData = CType(actionData.Judgment, clsUtilityCurveMeasureData)

                                If ucData.IsUndefined Then
                                    Return True
                                End If
                        End Select
                    End If

                Case ActionType.atNonPWAllChildren, ActionType.atNonPWAllCovObjs
                    Dim nowPwAll As clsNonPairwiseEvaluationActionData = CType(action.ActionData, clsNonPairwiseEvaluationActionData)

                    Select Case nowPwAll.MeasurementType
                        Case ECMeasureType.mtRatings

                            If TypeOf action.ActionData Is clsAllChildrenEvaluationActionData Then
                                Dim multiNonPwData As clsAllChildrenEvaluationActionData = CType(action.ActionData, clsAllChildrenEvaluationActionData)

                                For Each tAlt As clsNode In multiNonPwData.Children
                                    Dim r As clsRatingMeasureData = CType(multiNonPwData.GetJudgment(tAlt), clsRatingMeasureData)

                                    If r.IsUndefined Then
                                        Return True
                                    End If
                                Next
                            End If

                            If TypeOf action.ActionData Is clsAllCoveringObjectivesEvaluationActionData Then
                                Dim multiNonPwData2 As clsAllCoveringObjectivesEvaluationActionData = CType(action.ActionData, clsAllCoveringObjectivesEvaluationActionData)

                                For Each tAlt As clsNode In multiNonPwData2.CoveringObjectives
                                    Dim r As clsRatingMeasureData = CType(multiNonPwData2.GetJudgment(tAlt), clsRatingMeasureData)

                                    If r.IsUndefined Then
                                        Return True
                                    End If
                                Next
                            End If

                        Case ECMeasureType.mtDirect

                            If TypeOf action.ActionData Is clsAllChildrenEvaluationActionData Then
                                Dim multiDirectData1 As clsAllChildrenEvaluationActionData = CType(action.ActionData, clsAllChildrenEvaluationActionData)

                                For Each tAlt As clsNode In multiDirectData1.Children
                                    Dim dd As clsDirectMeasureData = CType(multiDirectData1.GetJudgment(tAlt), clsDirectMeasureData)

                                    If dd.IsUndefined Then
                                        Return True
                                    End If
                                Next
                            End If

                            If TypeOf action.ActionData Is clsAllCoveringObjectivesEvaluationActionData Then
                                Dim multiDirectData2 As clsAllCoveringObjectivesEvaluationActionData = CType(action.ActionData, clsAllCoveringObjectivesEvaluationActionData)

                                For Each tAlt As clsNode In multiDirectData2.CoveringObjectives
                                    Dim dd As clsDirectMeasureData = CType(multiDirectData2.GetJudgment(tAlt), clsDirectMeasureData)

                                    If dd.IsUndefined Then
                                        Return True
                                    End If
                                Next
                            End If
                    End Select
            End Select
        End If

        Return False
    End Function

    Public Shared ReadOnly Property CombinedUserID As Integer
        Get
            Dim app = CType(HttpContext.Current.Session("App"), clsComparionCore)
            Return app.ActiveProject.ProjectManager.Parameters.ResultsCustomCombinedUID
        End Get
    End Property

    Private Shared Function getClipBoardData() As String
        Dim clipBoard = ""
        Dim Model = CType(HttpContext.Current.Session(Constants.SessionModel), DataModel)

        For i As Integer = 0 To Model.ObjectivesData.Count - 1

            For j As Integer = 0 To i - 1
                clipBoard += vbTab
            Next

            For j As Integer = 0 To Model.ObjectivesData.Count - 1

                If i < j Then
                    Dim p = ""

                    For Each pair As StepsPairs In Model.StepPairs

                        If pair.Obj1 = Model.ObjectivesData(i).ID AndAlso pair.Obj2 = Model.ObjectivesData(j).ID Then
                            p += vbTab & (If(pair.Advantage >= 0, "", "-")) + pair.Value.ToString("F")
                            Exit For
                        End If

                        If pair.Obj2 = Model.ObjectivesData(i).ID AndAlso pair.Obj1 = Model.ObjectivesData(j).ID Then
                            p += vbTab & (If(pair.Advantage >= 0, "-", "")) + pair.Value.ToString("F")
                            Exit For
                        End If
                    Next

                    If p = "" Then
                        clipBoard += vbTab
                    End If

                    clipBoard += p
                End If
            Next

            clipBoard += vbLf
        Next

        Return clipBoard
    End Function

    Public Shared Function GetPairsData() As String
        Dim app = CType(HttpContext.Current.Session("App"), clsComparionCore)
        Dim Model = CType(HttpContext.Current.Session(Constants.SessionModel), DataModel)

        If HttpContext.Current.Session(InconsistencySortingEnabled) Is Nothing Then
            HttpContext.Current.Session(InconsistencySortingEnabled) = True
        End If

        Dim retVal As String = ""

        If Model IsNot Nothing AndAlso Model.StepPairs IsNot Nothing Then

            For Each item As StepsPairs In Model.StepPairs
                retVal += Convert.ToString((If(Not String.IsNullOrEmpty(retVal), ",", ""))) & $"[{item.StepNumber},{item.Value},{item.Obj1},{item.Obj2},{item.Advantage},{item.BestFitValue},{item.BestFitAdvantage},{item.Rank},{(If(item.IsUndefined, "1", "0"))}]"
            Next
        End If

        Return $"[{retVal}]"
    End Function

    Public Shared Property ObjectivesHighestResult As Double
        Get
            Dim showUnassessed As Double = If(HttpContext.Current.Session(SessionObjectivesHighestResult) Is Nothing, 0, CDbl(HttpContext.Current.Session(SessionObjectivesHighestResult)))
            Return showUnassessed
        End Get
        Set(ByVal value As Double)
            HttpContext.Current.Session(SessionObjectivesHighestResult) = value
        End Set
    End Property

    Public Shared Function GetObjectivesData() As String
        Dim retVal As String = ""
        Dim Model = CType(HttpContext.Current.Session(Constants.SessionModel), DataModel)

        If Model IsNot Nothing AndAlso Model.ObjectivesDataSorted IsNot Nothing Then

            For Each obj As Objective In Model.ObjectivesDataSorted
                If obj.Value > ObjectivesHighestResult Then ObjectivesHighestResult = obj.Value
                retVal += Convert.ToString((If(Not String.IsNullOrEmpty(retVal), ",", ""))) & $"[{obj.ID},'{obj.Value}','{StringFuncs.JS_SafeString(obj.Name)}','{StringFuncs.Double2String(obj.Value * 100, 2, True)}']"
            Next
        End If

        Return $"[{retVal}]"
    End Function

    Private Shared Function IsPairwiseMeasureType(ByVal mt As ECMeasureType) As Boolean
        Return mt = ECMeasureType.mtPairwise OrElse mt = ECMeasureType.mtPWAnalogous
    End Function

    Private Shared Function IsNewUser(ByVal app As clsComparionCore) As Boolean
        Dim newUser = HttpContext.Current.Session("NewUser") IsNot Nothing AndAlso CBool(HttpContext.Current.Session("NewUser"))

        If Not newUser AndAlso app.ActiveUser IsNot Nothing Then
            newUser = (app.ActiveUser.Created.HasValue AndAlso DateTime.Compare(app.ActiveUser.Created.Value.AddMinutes(1), DateTime.Now) > 0)
        End If

        Return newUser
    End Function

    Public Shared Function GetNextProject(ByVal project As clsProject) As clsProject
        Dim nextProject As clsProject = Nothing

        If project IsNot Nothing Then
            Dim app = CType(HttpContext.Current.Session("App"), clsComparionCore)
            Dim parameters = project.ProjectManager.Parameters

            If parameters.EvalOpenNextProjectAtFinish AndAlso parameters.EvalNextProjectPasscodeAtFinish <> "" Then
                Dim passCode = parameters.EvalNextProjectPasscodeAtFinish.ToLower().Replace(clsProjectParametersWithDefaults.OPT_PASSCODE_RISK_CONTROLS_PIPE_PREFIX.ToLower(), "")

                If passCode <> "" Then
                    nextProject = app.DBProjectByPasscode(passCode)

                    If nextProject Is Nothing Then
                        parameters.EvalOpenNextProjectAtFinish = False
                        parameters.Save()
                    ElseIf nextProject.ProjectStatus <> ecProjectStatus.psActive OrElse nextProject.WorkgroupID <> app.ActiveProject.WorkgroupID Then
                        parameters.EvalOpenNextProjectAtFinish = False
                        parameters.Save()
                    End If
                End If
            End If
        End If

        Return nextProject
    End Function

    Public Shared Function CanUserModifyProject(ByVal project As clsProject, ByVal email As String) As Boolean
        Dim app = CType(HttpContext.Current.Session("App"), clsComparionCore)
        Dim user = app.DBUserByEmail(email)
        If project Is Nothing OrElse user Is Nothing Then Return False
        Dim workGroup = app.DBWorkgroupByID(project.WorkgroupID)
        Dim userWorkGroup = app.DBUserWorkgroupByUserIDWorkgroupID(user.UserID, workGroup.ID)
        Dim workSpace = app.DBWorkspaceByUserIDProjectID(user.UserID, project.ID)
        Return app.CanUserModifyProject(user.UserID, project.ID, userWorkGroup, workSpace, workGroup)
    End Function

    Public Shared Sub SetQuickHelpObjectIdInSession(ByVal qhObjectId As String)
        HttpContext.Current.Session(Constants.SessionQhNode) = qhObjectId
    End Sub

    Private Shared _koToken As String = String.Empty
    Private Shared _koExpiresIn As DateTime = DateTime.MinValue

    Public Shared Sub CheckProjectIsAccessible(ByVal project As clsProject, ByVal email As String)
        Dim message = GetProjectInAccessibleMessage(project, email, True)

        If message.Length > 0 Then
            HttpContext.Current.Session("UserSpecificHashErrorMessage") = message
            HttpContext.Current.Response.Redirect("~/?accError=cannotAccessProject")
        End If
    End Sub

    Public Shared Function GetProjectInAccessibleMessage(ByVal project As clsProject, ByVal email As String, ByVal canEmailEmpty As Boolean) As String
        If (canEmailEmpty AndAlso String.IsNullOrEmpty(email)) OrElse CBool(HttpContext.Current.Session(Constants.SessionIsPipeViewOnly)) Then Return String.Empty
        email = If(String.IsNullOrEmpty(email), Guid.NewGuid().ToString(), email)
        'Dim isPm = CanUserModifyProject(project, email)
        If CanUserModifyProject(project, email) Then Return String.Empty
        'Dim message = GetMessageIfProjectIsOfflineOrArchived(project)
        Return GetMessageIfProjectIsOfflineOrArchived(project)
    End Function

    Public Shared Function GetMessageIfProjectIsOfflineOrArchived(ByVal project As clsProject) As String
        Dim message = String.Empty

        If Not project.isOnline Then
            message = String.Format(TeamTimeClass.ResString("msgAuthProjectDisabled"), project.ProjectName)
        ElseIf project.LockInfo.LockStatus <> ECLockStatus.lsUnLocked Then
            message = String.Format(TeamTimeClass.ResString("msgEvaluationLocked"), project.ProjectName)
        ElseIf project.ProjectStatus = ecProjectStatus.psArchived Then
            message = String.Format(TeamTimeClass.ResString("msgAuthProjectReadOnly"), project.ProjectName)
        End If

        Return message
    End Function

    Friend Shared Function GetComparionHashLink() As String
        Dim context = HttpContext.Current
        Dim url = HttpContext.Current.Request.Url
        Dim comparionUrl = If(url.Host.Equals("localhost"), $"{url.Scheme}://{url.Host}:20180/", $"{url.Scheme}://{url.Authority}/")
        comparionUrl = comparionUrl.Replace("//r.", "//").Replace("//r-", "//")
        Dim hashLink = If(context.Session("hash") Is Nothing, (If(context.Session("hashLink") Is Nothing, String.Empty, context.Session("hashLink").ToString())), context.Session("hash").ToString())
        context.Session("hashLink") = hashLink
        Dim isNewUser = If(context.Session("NewUser") Is Nothing, False, CBool(context.Session("NewUser")))
        context.Session("NewUser") = isNewUser

        If isNewUser Then
            Dim app = CType(context.Session("App"), clsComparionCore)
            hashLink = GeckoClass.CreateLogonURL(app.ActiveUser, app.ActiveProject, False, "", "")
            comparionUrl += $"{hashLink}"
            hashLink = hashLink.Replace("?hash=", "")
            Dim hashCookie = New HttpCookie("LastHash", hashLink) With {
                .HttpOnly = True,
                .Expires = DateTime.Now.AddDays(1)
            }
            context.Response.Cookies.Add(hashCookie)
        Else
            comparionUrl += $"?hash={hashLink}"
        End If

        Return $"{comparionUrl}&ignoreval=yes"
    End Function

    Public Shared Property JudgmentsSaved As Boolean
        Get
            Dim sessVal = TeamTimeClass.get_SessVar(SessSaved)
            Return sessVal = "1"
        End Get
        Set(ByVal value As Boolean)
            TeamTimeClass.set_SessVar(SessSaved, If(value, "1", "0"))
        End Set
    End Property

    Friend Shared Function RedirectAnonAndSignupLinks(ByVal app As clsComparionCore, ByVal passCode As String) As Boolean
        Dim activeProjectInsight = If(app.ActiveProject, app.DBProjectByPasscode(passCode))
        Dim welcomeInsightSurvey = activeProjectInsight.ProjectManager.StorageManager.Reader.IsWelcomeSurveyAvailable(activeProjectInsight.isImpact) AndAlso activeProjectInsight.ProjectManager.PipeParameters.ShowWelcomeSurvey
        Dim thankYouInsightSurvey = activeProjectInsight.ProjectManager.StorageManager.Reader.IsThankYouSurveyAvailable(activeProjectInsight.isImpact) AndAlso activeProjectInsight.ProjectManager.PipeParameters.ShowThankYouSurvey
        Dim newUser = IsNewUser(app)
        Dim hasAltOrObjQuestionType = False

        If activeProjectInsight.isValidDBVersion AndAlso (welcomeInsightSurvey OrElse thankYouInsightSurvey) AndAlso newUser Then
            app.ActiveProject = app.DBProjectByPasscode(passCode)
            Dim pipeSteps = app.ActiveProject.Pipe.Count

            For i = 1 To pipeSteps
                Dim anytimeAction = GetAction(i)

                If anytimeAction.ActionType = ActionType.atSpyronSurvey Then

                    If CheckInsightSurveyHasObjectiveOrAltenatives(app, anytimeAction) Then
                        hasAltOrObjQuestionType = True
                        Exit For
                    End If
                End If
            Next
        End If

        Return hasAltOrObjQuestionType
    End Function

    Private Shared Function CheckInsightSurveyHasObjectiveOrAltenatives(ByVal app As clsComparionCore, ByVal anytimeAction As clsAction) As Boolean
        Dim userList = New Dictionary(Of String, clsComparionUser)()
        userList.Add(app.ActiveUser.UserEmail, New clsComparionUser With {
            .ID = app.ActiveProject.ProjectManager.UserID,
            .UserName = app.ActiveProject.ProjectManager.User.UserName
        })
        Dim actionData = CType(anytimeAction.ActionData, clsSpyronSurveyAction)
        app.SurveysManager.ActiveUserEmail = app.ActiveUser.UserEmail
        'Dim surveyInfo = app.SurveysManager.get_GetSurveyInfoByProjectID(app.ProjectID, CType(actionData.SurveyType, SurveyType), userList)
        Dim surveyInfo = app.SurveysManager.GetSurveyInfoByProjectID(app.ProjectID, CType(actionData.SurveyType, SurveyType), userList)
        If surveyInfo Is Nothing Then Return False
        'Dim survey = surveyInfo.get_Survey(app.ActiveUser.UserEmail)
        Return surveyInfo.Survey(app.ActiveUser.UserEmail).isSurveyContainsPipeModifiers()
    End Function

    Public Shared Function get_StepInformation(ByVal app As clsComparionCore, ByVal Optional previousStep As Integer = -1, ByVal Optional first As Integer = 1, ByVal Optional last As Integer = -1) As String
        Dim steps = ""
        'Dim disableStep = False

        If previousStep > -1 Then
            steps = GetStepData(previousStep - 1, steps)
        Else
            ShowUnassessed = False
            If last <= 0 Then last = app.ActiveProject.Pipe.Count

            For i As Integer = first - 1 To last - 1
                steps = GetStepData(i, steps)
                If i < last - 1 Then steps += ","
            Next
        End If

        Return steps
    End Function

    Public Shared Function GetStepData(ByVal [step] As Integer, ByVal steps As String, ByVal Optional fButton As Boolean = False) As String
        Dim app = CType(HttpContext.Current.Session("App"), clsComparionCore)
        Dim action = GetAction([step] + 1)
        Dim content = GeckoClass.GetPipeStepHint(action, Nothing)
        Dim color = ""
        'Dim background = ""
        Dim isUndefined = 0

        Select Case GetStepStatus(action)
            Case False

                Select Case GetAction([step] + 1).ActionType
                    Case ActionType.atShowLocalResults, ActionType.atShowGlobalResults, ActionType.atInformationPage, ActionType.atSurvey, ActionType.atSensitivityAnalysis, ActionType.atSpyronSurvey
                        color = "#0058a3"
                    Case Else
                        color = "#000"
                End Select

            Case True
                color = "#e74c3c"

                If Not app.ActiveProject.PipeParameters.AllowMissingJudgments Then
                    isUndefined = 1
                End If

                ShowUnassessed = True
        End Select

        Dim rgx As Regex = New Regex("<.*?>")
        Dim stepinfo = ""

        If fButton Then
            stepinfo = Convert.ToString([step] + 1)
        Else
            isUndefined = [step] + 1
            stepinfo = String.Format(TeamTimeClass.ResString("btnEvaluationStepHint"), [step] + 1, rgx.Replace(content.Replace("'", """"), " "))
        End If

        'steps += $"['{stepinfo}','{color}','{background}','{isUndefined}']"
        steps += $"['{stepinfo}','{color}','','{isUndefined}']"
        Return steps
    End Function

    Private Shared Function CanShowCombinedResults(ByVal data As clsShowLocalResultsActionData) As Boolean
        If CombinedUserID = ECTypes.COMBINED_USER_ID Then
            Return data.CanShowGroupResults()
        Else

            Try
                Return data.CanShowResultsForUser(CombinedUserID)
            Catch
                Return True
            End Try
        End If
    End Function

    Public Shared Function CreateLocalResults(ByVal [step] As Integer, ByVal Optional normalization As Integer = 1) As Object
        'Dim context = HttpContext.Current
        HttpContext.Current.Session("normalization") = normalization
        Dim app = CType(HttpContext.Current.Session("App"), clsComparionCore)
        Dim actionData = TryCast(AnytimeClass.Action([step]).ActionData, clsShowLocalResultsActionData)
        Dim currentNode = New Object(2) {}
        Dim highestResult = 0.00
        Dim messagenote = ""
        Dim showTitle = True
        Dim resultsTitle = ""

        Select Case app.ActiveProject.PipeParameters.LocalResultsView
            Case CanvasTypes.ResultsView.rvIndividual
                showTitle = actionData.CanShowIndividualResults
            Case CanvasTypes.ResultsView.rvGroup
                showTitle = CanShowCombinedResults(actionData)
            Case Else
                showTitle = actionData.CanShowIndividualResults OrElse CanShowCombinedResults(actionData)
        End Select

        currentNode(0) = actionData.ParentNode.NodeName

        If actionData.ParentNode.Children.Count > 0 Then
            currentNode(1) = app.ActiveProject.PipeParameters.NameObjectives
            currentNode(2) = True
        Else
            currentNode(1) = app.ActiveProject.PipeParameters.NameAlternatives
            currentNode(2) = False
        End If

        app.ActiveProject.ProjectManager.CheckCustomCombined()
        Dim resultsData = Anytime.GetIntermediateResultsData(normalization)

        For Each result In resultsData
            Dim individualResults As Double = 0
            Dim combinedResults As Double = 0
            Double.TryParse(result(2), individualResults)

            If individualResults > highestResult Then
                highestResult = Convert.ToDouble(result(2))
            End If

            Double.TryParse(result(3), combinedResults)

            If combinedResults > highestResult Then
                highestResult = Convert.ToDouble(result(3))
            End If
        Next

        'Dim action = (CType(GetAction(CInt([step]) - 1), clsAction))
        Dim stepPairsData = GetPairsData()
        Dim objectivesData = GetObjectivesData()
        Dim inconsistencyRatioStatus As Boolean
        Dim with1 = app.ActiveProject.ProjectManager
        Dim sEmail = app.ActiveUser.UserEmail
        Dim ahpUser = with1.GetUserByEMail(sEmail)
        Dim ahpUserId = ahpUser.UserID
        Dim defaultsort = CInt(app.ActiveProject.PipeParameters.LocalResultsSortMode)
        Dim inconsistencyRatio = actionData.InconsistencyIndividual
        Dim judgmentType = (CType(GetAction(CInt([step]) - 1), clsAction)).ActionType
        Dim measurementType = ""
        Dim JudgmentSaved = False
        Dim equalMessage = False
        Dim parentNode As clsNode = New clsNode()
        Dim pos = 0
        Dim neg = 0
        Dim mid = 0
        Dim totalJudgment = 0
        Dim IsIntensities = False

        If HttpContext.Current.Session(Constants.Sess_ShowEqualOnce) Is Nothing Then
            Dim ObjNodes = New List(Of Object())()

            For Each node As clsNode In app.ActiveProject.HierarchyObjectives.Nodes
                Dim cluster = New Object(1) {}
                cluster(0) = node.NodeID
                cluster(1) = False
                ObjNodes.Add(cluster)
            Next

            HttpContext.Current.Session(Constants.Sess_ShowEqualOnce) = ObjNodes
        End If

        Dim clusterMessage = CType(HttpContext.Current.Session(Constants.Sess_ShowEqualOnce), List(Of Object()))
        Dim ShowEqualOnce = False
        Dim clusterIndex = -1

        For i As Integer = 0 To clusterMessage.Count - 1

            If CInt(clusterMessage(i)(0)) = actionData.ParentNode.NodeID Then

                If Not CBool(clusterMessage(i)(1)) Then
                    ShowEqualOnce = True
                    clusterIndex = i
                End If
            End If
        Next

        Select Case judgmentType
            Case ActionType.atPairwise, ActionType.atPairwiseOutcomes
                Dim pwData = CType(AnytimeClass.Action([step] - 1).ActionData, clsPairwiseMeasureData)
                parentNode = CType(app.ActiveProject.HierarchyObjectives.GetNodeByID(pwData.ParentNodeID), clsNode)
                measurementType = app.ActiveProject.ProjectManager.PipeBuilder.GetPairwiseTypeForNode(parentNode).ToString()
                Dim sStep = app.ActiveProject.ProjectManager.PipeBuilder.GetFirstEvalPipeStepForNode(actionData.ParentNode, [step])

                If Action(sStep + 1).ActionType = ActionType.atAllPairwise Then
                    [step] -= 1
                    GoTo _Select1_CaseDefault
                End If

                If ShowEqualOnce Then

                    For i As Integer = sStep + 1 To [step] - 1
                        Dim tVal = CType(Action(i).ActionData, clsPairwiseMeasureData)

                        If Not tVal.IsUndefined Then

                            If tVal.Advantage = 0 Then
                                mid += 1
                            ElseIf tVal.Advantage > 0 Then
                                pos += 1
                            Else
                                neg += 1
                            End If
                        End If
                    Next

                    totalJudgment = actionData.ParentNode.Judgments.JudgmentsFromUser(ahpUser.UserID).Count

                    If (pos + mid >= totalJudgment OrElse neg + mid >= totalJudgment) AndAlso ((pos + mid >= 5) OrElse (neg + mid >= 5)) Then
                        messagenote = TeamTimeClass.ResString("msgSameSideJudgments")
                        equalMessage = True
                    End If
                End If

                GoTo _Select1_CaseDefault
            Case ActionType.atAllPairwise, ActionType.atAllPairwiseOutcomes
                Dim sStep = app.ActiveProject.ProjectManager.PipeBuilder.GetFirstEvalPipeStepForNode(actionData.ParentNode, [step])
                Dim pwData = CType(Action(sStep + 1).ActionData, clsAllPairwiseEvaluationActionData)
                parentNode = CType(app.ActiveProject.HierarchyObjectives.GetNodeByID(pwData.ParentNode.NodeID), clsNode)
                measurementType = app.ActiveProject.ProjectManager.PipeBuilder.GetPairwiseTypeForNode(parentNode).ToString()

                If sStep >= 0 AndAlso ShowEqualOnce Then

                    For Each tVal As clsPairwiseMeasureData In pwData.Judgments

                        If Not tVal.IsUndefined Then

                            If tVal.Advantage = 0 Then
                                mid += 1
                            ElseIf tVal.Advantage > 0 Then
                                pos += 1
                            Else
                                neg += 1
                            End If
                        End If
                    Next

                    If (((pos + mid >= pwData.Judgments.Count) AndAlso neg < 1) OrElse (neg + mid >= pwData.Judgments.Count) AndAlso pos < 1) AndAlso ((pos + mid >= 5) OrElse (neg + mid >= 5)) Then
                        messagenote = TeamTimeClass.ResString("msgSameSideJudgments")
                        equalMessage = True
                    End If
                End If

                GoTo _Select1_CaseDefault
            Case Else
_Select1_CaseDefault:
                If TeamTimeClass.get_SessVar(SessParentId) <> parentNode.NodeID.ToString() Then TeamTimeClass.set_SessVar(SessSaved, Nothing)
                TeamTimeClass.set_SessVar(SessParentId, parentNode.NodeID.ToString())

                If Not JudgmentsSaved Then
                    SaveJudgments(parentNode.NodeID)
                End If

                JudgmentSaved = JudgmentsSaved
        End Select

        If Not IsIntensities AndAlso (pos = totalJudgment OrElse neg = totalJudgment) Then
        End If

        resultsTitle = GeckoClass.GetPipeStepHint(Action([step]), Nothing, True, True)

        If UserIsReadOnly() Then
            ahpUser = app.DBUserByID(GetReadOnlyUserID())
            ahpUserId = ahpUser.UserID
            sEmail = ahpUser.UserEMail
        End If

        If actionData.ShowConsistency AndAlso (app.ActiveProject.PipeParameters.ShowConsistencyRatio And Not actionData.OnlyMainDiagonalEvaluated()) Then
            inconsistencyRatio = Convert.ToSingle(If(IsCombinedUserID(ahpUserId), actionData.InconsistencyCombined, actionData.InconsistencyIndividual))
            inconsistencyRatioStatus = Not ((resultsData.Count > 15) AndAlso Not actionData.OnlyMainDiagonalEvaluated())
        Else
            inconsistencyRatioStatus = False
        End If

        Dim canshowresults = New With {
            Key .individual = actionData.CanShowIndividualResults,
            Key .combined = CanShowCombinedResults(actionData)
        }

        If Not equalMessage Then

            If Not canshowresults.individual AndAlso Not canshowresults.combined Then
                messagenote = TeamTimeClass.ResString("sInsufficientData")
            ElseIf Not canshowresults.individual Then
                messagenote = TeamTimeClass.ResString("msgNoEvalDataIndividualResults")
            ElseIf Not canshowresults.combined Then
                messagenote = TeamTimeClass.ResString("msgNoEvalDataGroupResults")
            End If
        End If

        'Dim showIndex As Boolean = app.ActiveProject.ProjectManager.Parameters.ResultsLocalShowIndex
        Dim isAlternative = actionData.ParentNode.IsTerminalNode
        Dim lblredNumbers = ""

        If isAlternative Then
            lblredNumbers = TeamTimeClass.ResString("sInconsistencyToDoRed")
        Else
            lblredNumbers = TeamTimeClass.ResString("sInconsistencyToDoRedAlts")
        End If

        lblredNumbers = TeamTimeClass.PrepareTask(lblredNumbers)
        'Dim clipBoardData = getClipBoardData()
        Dim columnValueCombined = "Combined Results"

        If app.ActiveProject.ProjectManager.Parameters.ResultsCustomCombinedName <> "" AndAlso (actionData.ResultsViewMode = CanvasTypes.ResultsView.rvGroup OrElse actionData.ResultsViewMode = CanvasTypes.ResultsView.rvBoth) Then
            columnValueCombined = app.ActiveProject.ProjectManager.Parameters.ResultsCustomCombinedName
        End If

        Dim pipeParameters = New With {
        Key .CurrentNode = currentNode,
        Key .highest_result = highestResult,
        Key .results_data = resultsData,
        Key .defaultsort = defaultsort,
        Key .InconsistencyRatio = inconsistencyRatio,
        Key .InconsistencyRatioStatus = inconsistencyRatioStatus,
        Key .JudgmentType = judgmentType.ToString(),
        Key .StepPairsData = stepPairsData,
        Key .ObjectivesData = objectivesData,
        Key .matrix_highest_result = ObjectivesHighestResult,
        Key .messagenote = messagenote,
        Key .canshowresults = canshowresults,
        Key .showIndex = app.ActiveProject.ProjectManager.Parameters.ResultsLocalShowIndex,
        Key .isAlternative = isAlternative,
        Key .MeasurementType = measurementType,
        Key .JudgmentsSaved = JudgmentSaved,
        Key .lblredNumbers = lblredNumbers,
        Key .equalMessage = equalMessage,
        Key .ClipBoardData = getClipBoardData(),
        Key .resultsTitle = resultsTitle,
        Key .showTitle = showTitle,
        Key .columnValueCombined = columnValueCombined
    }
        Return pipeParameters
    End Function

    Public Shared Sub SaveJudgments(ByVal parentId As Integer)
        Dim App = CType(HttpContext.Current.Session("App"), clsComparionCore)
        Dim tNode As clsNode = App.ActiveProject.HierarchyObjectives.GetNodeByID(parentId)

        If tNode IsNot Nothing AndAlso IsPairwiseMeasureType(tNode.MeasureType()) Then
            Dim tUserId As Integer = App.ActiveProject.ProjectManager.UserID
            Dim tJudgments As List(Of clsCustomMeasureData) = New List(Of clsCustomMeasureData)()

            For Each tJud As clsCustomMeasureData In tNode.Judgments.UsersJudgments(tUserId)
                Dim pwMeasureData As clsPairwiseMeasureData = CType(tJud, clsPairwiseMeasureData)
                Dim newpwMd As clsPairwiseMeasureData = New clsPairwiseMeasureData(pwMeasureData.FirstNodeID, pwMeasureData.SecondNodeID, pwMeasureData.Advantage, pwMeasureData.Value, pwMeasureData.ParentNodeID, tUserId, pwMeasureData.IsUndefined, pwMeasureData.Comment)
                tJudgments.Add(newpwMd)
            Next

            HttpContext.Current.Session(String.Format(SessJudgments, parentId)) = tJudgments
            HttpContext.Current.Session(SessDatetime) = App.ActiveProject.ProjectManager.User.LastJudgmentTime
        End If
    End Sub

    Public Shared ReadOnly Property IsImpact As Boolean
        Get
            Dim app = CType(HttpContext.Current.Session("App"), clsComparionCore)
            Return app.ActiveProject.ProjectManager.ActiveHierarchy = CInt(EcHierarchyId.HidImpact)
        End Get
    End Property

    Public Shared Function CreateSurvey(ByVal [step] As Integer) As Object
        Dim context As HttpContext = HttpContext.Current
        Dim app = CType(context.Session("App"), clsComparionCore)
        Dim pipeParameters As Object = Nothing
        Dim nodelist = New ArrayList()
        Dim alternativelist = New List(Of Object)()
        Dim objectivelist = New List(Of Object)()
        Dim surveyPage As clsSurveyPage = Nothing
        Dim questionNumbering = New List(Of Integer)()
        Dim altStringAll = New With {Key .NodeName = "All", Key .isDisabled = True}
        alternativelist.Insert(0, altStringAll)
        Dim data As clsSpyronSurveyAction = CType(Action(Convert.ToInt32([step])).ActionData, clsSpyronSurveyAction)
        Dim tUserList As List(Of clsUser) = MiscFuncs.ECMiscFuncs.GetUsersList(app.CanvasMasterConnectionDefinition.ConnectionString, clsProject.StorageType, app.ActiveProject.ProviderType, app.ProjectID)
        Dim aUsersList As Dictionary(Of String, clsComparionUser) = New Dictionary(Of String, clsComparionUser)()

        For Each user As clsUser In tUserList
            aUsersList.Add(user.UserEMail, New clsComparionUser With {
                .ID = user.UserID,
                .UserName = user.UserName
            })
        Next

        app.SurveysManager.ActiveUserEmail = app.ActiveUser.UserEmail
        Dim tUser = CType(app.ActiveProject.ProjectManager.User, clsUser)
        Dim tSurvey As clsSurveyInfo = app.SurveysManager.GetSurveyInfoByProjectID(app.ProjectID, CType(data.SurveyType, SurveyType), aUsersList)

        If tSurvey IsNot Nothing Then
            surveyPage = CType(tSurvey.Survey(tUser.UserEMail).Pages(data.StepNumber - 1), clsSurveyPage)
            If surveyPage Is Nothing AndAlso tSurvey.Survey(tUser.UserEMail).Pages.Count > 0 Then surveyPage = CType(tSurvey.Survey(tUser.UserEMail).Pages(0), clsSurveyPage)
        End If

        If surveyPage IsNot Nothing Then
            Dim surveyAnswers = New String(surveyPage.Questions.Count - 1)() {}
            Dim surveyQuestions = New Object(surveyPage.Questions.Count - 1) {}
            Dim respondent As clsRespondent = tSurvey.Survey(tUser.UserEMail).RespondentByEmail(tUser.UserEMail)

            If respondent Is Nothing Then
                respondent = New clsRespondent()
                respondent.Email = tUser.UserEMail
                respondent.Name = tUser.UserName
                tSurvey.SurveyDataProvider(tUser.UserEMail).SaveStreamRespondentAnswers(respondent)
                respondent = surveyPage.Survey.RespondentByEmail(tUser.UserEMail)
            End If

            For i As Integer = 0 To surveyPage.Questions.Count - 1
                Dim question = CType(surveyPage.Questions(i), clsQuestion)
                Dim answer As clsAnswer = Nothing

                If respondent IsNot Nothing Then
                    answer = respondent.AnswerByQuestionGUID(question.AGUID)
                End If

                If Not tSurvey.HideIndexNumbers Then questionNumbering.Add(surveyPage.Survey.GetQuestionPageIndex(question.AGUID))

                If answer IsNot Nothing Then
                    Dim answerbyQuestionString = New String(2) {answer.AnswerValuesString, question.AGUID.ToString(), question.AllowSkip.ToString()}
                    answerbyQuestionString(0) = answerbyQuestionString(0).Replace("""", "")

                    Select Case question.Type
                        Case QuestionType.qtVariantsRadio
                            Dim vars = New clsVariant()

                            If answer.AnswerVariants.Count > 0 Then
                                vars = CType(answer.AnswerVariants(0), clsVariant)
                            End If

                            If vars.Type = VariantType.vtOtherLine Then
                                Dim variantCount = question.Variants.Count - 1
                                Dim [variant] = CType(question.Variants(variantCount), clsVariant)
                                If [variant].Type = VariantType.vtOtherLine Then answerbyQuestionString(0) += ":"
                            End If

                        Case QuestionType.qtVariantsCheck, QuestionType.qtVariantsCombo

                            If answer.AnswerVariants.Count > 0 Then
                                Dim variantCount = answer.AnswerVariants.Count - 1
                                Dim [variant] = CType(answer.AnswerVariants(variantCount), clsVariant)
                                If [variant].Type = VariantType.vtOtherLine Then answerbyQuestionString(0) += ":"
                            End If

                        Case QuestionType.qtObjectivesSelect
                            Dim objStringAll = New With {
                            Key .NodeName = "All",
                            Key .isDisabled = True,
                            Key .level = 0
                        }
                            answerbyQuestionString(0) = objStringAll.isDisabled & ";"
                            objectivelist = GetObjectivesListforSurvey(app.ActiveProject.HierarchyObjectives.Nodes, answerbyQuestionString(0))
                            objectivelist.Insert(0, objStringAll)
                        Case QuestionType.qtAlternativesSelect
                            answerbyQuestionString(0) = altStringAll.isDisabled & ";"

                            For Each node As clsNode In app.ActiveProject.HierarchyAlternatives.Nodes
                                Dim alternativeitem = New With {
                                Key .NodeName = node.NodeName,
                                Key .isDisabled = Not node.DisabledForUser(tUser.UserID)
                            }
                                alternativelist.Add(alternativeitem)

                                If Not alternativeitem.isDisabled Then
                                    altStringAll = New With {
                                    Key .NodeName = "All",
                                    Key .isDisabled = False
                                }
                                End If

                                answerbyQuestionString(0) = altStringAll.isDisabled & ";"
                            Next

                        Case QuestionType.qtComment
                            question.AllowSkip = True
                            answerbyQuestionString(2) = question.AllowSkip.ToString()
                    End Select

                    surveyAnswers(i) = answerbyQuestionString
                Else
                    Dim answerbyQuestionString = New String(2) {"", question.AGUID.ToString(), question.AllowSkip.ToString()}

                    Select Case question.Type
                        Case QuestionType.qtObjectivesSelect
                            Dim objStringAll = New With {
                            Key .NodeName = "All",
                            Key .isDisabled = True,
                            Key .level = 0
                        }
                            answerbyQuestionString(0) = objStringAll.isDisabled & ";"
                            objectivelist = GetObjectivesListforSurvey(app.ActiveProject.HierarchyObjectives.Nodes, answerbyQuestionString(0))
                            objectivelist.Insert(0, objStringAll)
                        Case QuestionType.qtAlternativesSelect
                            answerbyQuestionString(0) = altStringAll.isDisabled & ";"

                            For Each node As clsNode In app.ActiveProject.HierarchyAlternatives.Nodes
                                Dim alternativeitem = New With {
                                Key .NodeName = node.NodeName,
                                Key .isDisabled = Not node.DisabledForUser(tUser.UserID)
                            }
                                alternativelist.Add(alternativeitem)

                                If Not alternativeitem.isDisabled Then
                                    altStringAll = New With {
                                    Key .NodeName = "All",
                                    Key .isDisabled = False
                                }
                                End If

                                answerbyQuestionString(0) += altStringAll.isDisabled & ";"
                            Next

                        Case QuestionType.qtComment
                            question.AllowSkip = True
                            answerbyQuestionString(2) = question.AllowSkip.ToString()
                    End Select

                    surveyAnswers(i) = answerbyQuestionString
                End If

                If question.Text.Contains(MimeIdentifier) Then
                    Dim unpackedText = Infodoc_Unpack(tSurvey.ProjectID, 0, reObjectType.SurveyQuestion, question.AGUID.ToString(), question.Text, True, True, -1)

                    If unpackedText = "" Then
                        Dim path = context.Server.MapPath("~/")
                        _FILE_ROOT = context.Server.MapPath("~/")
                        _FILE_MHT_FILES = IO.Path.GetFullPath(IO.Path.Combine(path, "DocMedia/MHTFiles/"))
                        Dim sBasePath = Infodoc_Path(tSurvey.ProjectID, app.ActiveProject.ProjectManager.ActiveHierarchy, reObjectType.SurveyQuestion, question.AGUID.ToString(), -1)
                        Dim sBaseUrl = $"{HttpContext.Current.Request.Url.Scheme}://{HttpContext.Current.Request.Url.Host}"
                        Dim samp = Infodoc_Pack(question.Text, sBaseUrl, sBasePath)
                        unpackedText = Infodoc_Unpack(tSurvey.ProjectID, 0, reObjectType.SurveyQuestion, question.AGUID.ToString(), samp, True, True, -1)
                    End If

                    question.Text = unpackedText
                End If

                Dim titleStartIndex As Integer = question.Text.IndexOf("<TITLE>", StringComparison.CurrentCultureIgnoreCase)

                If titleStartIndex >= 0 Then
                    Dim titleEndIndex As Integer = question.Text.IndexOf("</TITLE>", titleStartIndex, StringComparison.CurrentCultureIgnoreCase)
                    titleEndIndex += 8
                    Dim titleString As String = question.Text.Substring(titleStartIndex, titleEndIndex - titleStartIndex)
                    question.Text = question.Text.Replace(titleString, "<TITLE></TITLE>")
                End If

                If question.Text.Contains(vbCrLf) Then
                    question.Text = question.Text.Replace(vbCrLf, "")
                End If

                surveyPage.Questions(i) = question
            Next

            If respondent Is Nothing OrElse respondent.Answers.Count < 1 Then
                Dim need = False
                ReadPageAnswers(need, [step], surveyAnswers)
            End If

            Dim surveyContent = New With {
            Key .Title = surveyPage.Title,
            Key .Questions = surveyPage.Questions
        }
            pipeParameters = New With {
            Key .SurveyPage = surveyContent,
            Key .SurveyAnswers = surveyAnswers,
            Key .alternativelist = alternativelist,
            Key .objectivelist = objectivelist,
            Key .QuestionNumbering = questionNumbering
        }
        End If

        Return pipeParameters
    End Function

    Public Shared Function ReadPageAnswers(ByRef needToRebuildPipe As Boolean, ByVal [step] As Integer, ByVal respondentAnswers As String()()) As String
        Dim context As HttpContext = HttpContext.Current
        Dim app = CType(context.Session("App"), clsComparionCore)
        Dim pipeParameters = New Object()
        Dim surveyPage As clsSurveyPage = Nothing
        Dim surveyAnswers = New List(Of String)()
        Dim data As clsSpyronSurveyAction = CType(Action(Convert.ToInt32([step])).ActionData, clsSpyronSurveyAction)
        Dim tUserList As List(Of clsUser) = MiscFuncs.ECMiscFuncs.GetUsersList(app.CanvasMasterConnectionDefinition.ConnectionString, clsProject.StorageType, app.ActiveProject.ProviderType, app.ProjectID)
        Dim aUsersList As Dictionary(Of String, clsComparionUser) = New Dictionary(Of String, clsComparionUser)()
        Dim groupAnswers = ""

        For Each user As ECTypes.clsUser In tUserList
            aUsersList.Add(user.UserEMail, New clsComparionUser With {
                .ID = user.UserID,
                .UserName = user.UserName
            })
        Next

        app.SurveysManager.ActiveUserEmail = app.ActiveUser.UserEmail
        Dim tUser = CType(app.ActiveProject.ProjectManager.User, clsUser)
        Dim tSurvey As clsSurveyInfo = app.SurveysManager.GetSurveyInfoByProjectID(app.ProjectID, CType(data.SurveyType, SurveyType), aUsersList)

        If (tSurvey IsNot Nothing) Then
            surveyPage = CType(tSurvey.Survey(tUser.UserEMail).Pages(data.StepNumber - 1), clsSurveyPage)
            If surveyPage Is Nothing AndAlso tSurvey.Survey(tUser.UserEMail).Pages.Count > 0 Then surveyPage = CType(tSurvey.Survey(tUser.UserEMail).Pages(0), clsSurveyPage)
        End If

        If surveyPage IsNot Nothing AndAlso surveyPage.Survey IsNot Nothing Then
            Dim respondent As clsRespondent = surveyPage.Survey.RespondentByEmail(tUser.UserEMail)

            If respondent Is Nothing Then
                respondent = New clsRespondent()
                respondent.Email = tUser.UserEMail
                respondent.Name = tUser.UserName
                tSurvey.SurveyDataProvider(tUser.UserEMail).SaveStreamRespondentAnswers(respondent)
                respondent = surveyPage.Survey.RespondentByEmail(tUser.UserEMail)
            End If

            If respondent IsNot Nothing Then

                For Each aQuestion As clsQuestion In surveyPage.Questions
                    Dim idx = surveyPage.Questions.IndexOf(aQuestion)
                    If aQuestion.Type = QuestionType.qtAlternativesSelect Or aQuestion.Type = QuestionType.qtObjectivesSelect Or aQuestion.LinkedAttributeID <> Guid.Empty Then needToRebuildPipe = True
                    Dim answer = New clsAnswer()
                    answer.Question = aQuestion
                    answer.AnswerDate = DateTime.Now
                    Dim wIndex = -1

                    If Not respondent.Answers.Contains(answer) And True Then

                        If respondent.AnswerByQuestionGUID(aQuestion.AGUID) IsNot Nothing Then
                            Dim ans = respondent.AnswerByQuestionGUID(aQuestion.AGUID)
                            Dim ind = GetIndexofChild(respondentAnswers, aQuestion.AGUID.ToString())

                            If aQuestion.AGUID.ToString() = respondentAnswers(ind)(1) Then
                                ReadAnswer(ans, aQuestion, respondentAnswers(ind)(0))
                                wIndex = ind
                            Else
                                ReadAnswer(ans, aQuestion, respondentAnswers(idx)(0))
                                wIndex = idx
                            End If
                        Else
                            respondent.Answers.Add(answer)
                            Dim ans = respondent.AnswerByQuestionGUID(aQuestion.AGUID)
                            Dim ind = GetIndexofChild(respondentAnswers, aQuestion.AGUID.ToString())

                            If aQuestion.AGUID.ToString() = respondentAnswers(ind)(1) Then
                                ReadAnswer(ans, aQuestion, respondentAnswers(ind)(0))
                                wIndex = ind
                            Else
                                ReadAnswer(ans, aQuestion, respondentAnswers(idx)(0))
                                wIndex = idx
                            End If
                        End If
                    End If

                    If (aQuestion.Type = QuestionType.qtAlternativesSelect Or aQuestion.Type = QuestionType.qtObjectivesSelect) And (app.ActiveProject.ProjectManager IsNot Nothing) Then
                        app.ActiveProject.ProjectManager.StorageManager.Writer.SaveUserDisabledNodes(app.ActiveProject.ProjectManager.User)
                    End If

                    If (respondentAnswers(wIndex)(0) <> "" AndAlso respondentAnswers(wIndex)(0) <> ":") And Not aQuestion.LinkedAttributeID.Equals(Guid.Empty) Then
                        groupAnswers = respondentAnswers(wIndex)(0)
                        Dim AAttribute As clsAttribute = Nothing
                        SetUserAttribute(aQuestion.LinkedAttributeID, respondent.ID, AttributeValueTypes.avtString, respondentAnswers(wIndex)(0))

                        For Each atr As clsAttribute In app.ActiveProject.ProjectManager.Attributes.GetUserAttributes()

                            If atr.ID.Equals(aQuestion.LinkedAttributeID) Then
                                AAttribute = atr
                                Exit For
                            End If
                        Next

                        If AAttribute IsNot Nothing Then
                            Dim AnswerString As String = ""

                            If aQuestion.Type = QuestionType.qtVariantsCheck Then
                                Dim splitAnswers = groupAnswers.Split(";"c)

                                For Each value As String In splitAnswers

                                    If (groupAnswers <> "" AndAlso groupAnswers <> ":") Then
                                        AnswerString += (If(AnswerString = "", "", ";")) & """" & value.Trim() & """"
                                    End If
                                Next
                            Else
                                If (groupAnswers <> "" AndAlso groupAnswers <> ":") Then AnswerString = """" & respondentAnswers(wIndex)(0).Trim() & """"
                            End If

                            SetUserAttribute(aQuestion.LinkedAttributeID, respondent.ID, AAttribute.ValueType, AnswerString)
                        End If
                    End If
                Next

                If True Then
                    Dim a As clsSurveyDataProvider
                    tSurvey.SurveyDataProvider(respondent.Email).SaveStreamRespondentAnswers(respondent)
                    a = tSurvey.SurveyDataProvider(respondent.Email)
                End If
            End If
        End If

        Return ""
    End Function

    Private Shared Function GetObjectivesListforSurvey(ByVal nodes As List(Of clsNode), ByRef AnswerString As String) As List(Of Object)
        Dim app = CType(HttpContext.Current.Session("App"), clsComparionCore)
        Dim tUser = CType(app.ActiveProject.ProjectManager.User, clsUser)
        Dim level = New List(Of Object)()

        For Each node As clsNode In nodes
            Dim idx = nodes.IndexOf(node)

            If Not node.IsAlternative Then
                Dim objectiveitem = New With {
                Key .NodeName = node.NodeName,
                Key .isDisabled = idx = 0 OrElse Not node.DisabledForUser(tUser.UserID),
                Key .level = node.Level
            }
                AnswerString += objectiveitem.isDisabled & ";"
                level.Add(objectiveitem)
            End If
        Next

        Return level
    End Function

    Private Shared Function GetIndexofChild(ByVal items As String()(), ByVal compareString As String) As Integer
        For Each item As String() In items
            If item(1) = compareString Then
                Return Array.IndexOf(items, item)
            End If
        Next
        Return -1
    End Function

    Public Shared Sub ReadAnswer(ByVal answer As clsAnswer, ByVal question As clsQuestion, ByVal key As String)
        Dim context As HttpContext = HttpContext.Current
        Dim app = CType(context.Session("App"), clsComparionCore)
        Dim hierarchy = app.ActiveProject.ProjectManager.Hierarchy(app.ActiveProject.ProjectManager.ActiveHierarchy)

        If question.Type <> QuestionType.qtComment Then

            If answer Is Nothing Then
                answer = New clsAnswer()
                answer.Question = question
            End If

            answer.AnswerDate = DateTime.Now

            If question.Type = QuestionType.qtObjectivesSelect Then
                Dim objectives = key.Split(";")
                Dim objectiveshierarchy = app.ActiveProject.HierarchyObjectives.Nodes

                For Each node As clsNode In objectiveshierarchy

                    If Not node.IsAlternative Then
                        Dim idx = objectiveshierarchy.IndexOf(node)
                        'node.DisabledForUser(hierarchy.ProjectManager.User.UserID, Not Convert.ToBoolean(objectives(idx + 1)))
                        Dim a As Boolean = node.DisabledForUser(hierarchy.ProjectManager.User.UserID)
                    End If
                Next
            End If

            If question.Type = QuestionType.qtAlternativesSelect Then
                Dim alternatives = key.Split(";"c)
                Dim altshierarchy = app.ActiveProject.HierarchyAlternatives.Nodes

                For Each node As clsNode In app.ActiveProject.HierarchyAlternatives.Nodes

                    If node.IsAlternative Then
                        Dim idx = altshierarchy.IndexOf(node)
                        'node.DisabledForUser(hierarchy.ProjectManager.User.UserID, Not Convert.ToBoolean(alternatives(idx + 1)))
                        Dim b As Boolean = node.DisabledForUser(hierarchy.ProjectManager.User.UserID)
                    End If
                Next
            End If

            answer.AnswerVariants.Clear()

            If key IsNot Nothing Then

                Select Case question.Type
                    Case QuestionType.qtOpenLine, QuestionType.qtOpenMemo
                        Dim K As String = Nothing
                        Dim AVariant As clsVariant = Nothing
                        K = key

                        If Not String.IsNullOrEmpty(K) Then
                            AVariant = New clsVariant()
                            AVariant.VariantValue = K
                            AVariant.Type = VariantType.vtOtherLine
                            answer.AnswerVariants.Add(AVariant)
                        End If

                    Case QuestionType.qtVariantsCheck, QuestionType.qtImageCheck
                        Dim answerVariants = key.Split(";"c)
                        Array.Sort(answerVariants, StringComparer.InvariantCulture)

                        If answer.AnswerVariants.Count < 1 Then

                            For Each aVariant As clsVariant In question.Variants
                                Dim idx = question.Variants.IndexOf(aVariant)

                                If aVariant.Type = VariantType.vtText Then
                                    Dim ansIdx = Array.IndexOf(answerVariants, aVariant.Text)

                                    If ansIdx > -1 Then

                                        If answerVariants(ansIdx) = aVariant.Text Then

                                            If key <> "" Then
                                                aVariant.VariantValue = answerVariants(ansIdx)
                                                aVariant.Text = answerVariants(ansIdx)
                                                answer.AnswerVariants.Add(aVariant)
                                            End If
                                        End If
                                    End If
                                End If

                                If (aVariant.Type = VariantType.vtOtherLine) Or (aVariant.Type = VariantType.vtOtherMemo) Then
                                    Dim othervalue = ""

                                    For Each other As String In answerVariants

                                        If key <> "" Then
                                            If other.Contains(":") Then othervalue = other
                                        End If
                                    Next

                                    Dim ansIdx = Array.IndexOf(answerVariants, othervalue)

                                    If ansIdx > -1 Then
                                        Dim values = othervalue.Split(":"c)
                                        aVariant.VariantValue = values(0)
                                        answer.AnswerVariants.Add(aVariant)
                                    End If
                                End If
                            Next
                        Else

                            For Each aVariant As clsVariant In answer.AnswerVariants
                                Dim idx = answer.AnswerVariants.IndexOf(aVariant)

                                If (aVariant.Type = VariantType.vtOtherLine) Or (aVariant.Type = VariantType.vtOtherMemo) Or (aVariant.Type = VariantType.vtText) Then

                                    If key <> "" Then
                                        aVariant.VariantValue = answerVariants(idx)
                                    End If
                                End If
                            Next
                        End If

                    Case QuestionType.qtObjectivesSelect
                    Case QuestionType.qtAlternativesSelect
                    Case QuestionType.qtVariantsRadio

                        If answer.AnswerVariants.Count < 1 Then
                            Dim pass = False

                            For Each aVariant As clsVariant In question.Variants

                                If (aVariant.Type = VariantType.vtOtherMemo) Or (aVariant.Type = VariantType.vtText) Then

                                    If key <> "" Then

                                        If aVariant.VariantValue = key Then
                                            pass = True
                                            answer.AnswerVariants.Add(aVariant)
                                        End If
                                    End If
                                End If

                                If aVariant.Type = VariantType.vtOtherLine AndAlso Not pass Then

                                    If aVariant.VariantValue <> key Then
                                        aVariant.VariantValue = key
                                        answer.AnswerVariants.Add(aVariant)
                                    End If
                                End If
                            Next
                        Else

                            For Each aVariant As clsVariant In answer.AnswerVariants

                                If (aVariant.Type = VariantType.vtOtherLine) Or (aVariant.Type = VariantType.vtOtherMemo) Or (aVariant.Type = VariantType.vtText) Then

                                    If key <> "" Then
                                        aVariant.VariantValue = key
                                    End If
                                End If
                            Next
                        End If

                    Case QuestionType.qtVariantsCombo

                        If answer.AnswerVariants.Count < 1 Then

                            For Each aVariant As clsVariant In question.Variants

                                If aVariant.VariantValue.Contains(key) Then

                                    If (aVariant.Type = VariantType.vtText) Then

                                        If key <> "" Then
                                            answer.AnswerVariants.Add(aVariant)
                                            Exit For
                                        End If
                                    End If
                                Else

                                    If (aVariant.Type = VariantType.vtOtherLine) Or (aVariant.Type = VariantType.vtOtherMemo) Then

                                        If key <> "" Then
                                            aVariant.VariantValue = key
                                            answer.AnswerVariants.Add(aVariant)
                                            Exit For
                                        End If
                                    End If
                                End If
                            Next
                        Else

                            For Each aVariant As clsVariant In answer.AnswerVariants

                                If (aVariant.Type = VariantType.vtOtherLine) Or (aVariant.Type = VariantType.vtOtherMemo) Or (aVariant.Type = VariantType.vtText) Then

                                    If key <> "" Then
                                        aVariant.VariantValue = key
                                    End If
                                End If
                            Next
                        End If

                    Case QuestionType.qtNumber, QuestionType.qtNumberColumn
                        Dim K As String = Nothing
                        Dim AVariant As clsVariant = Nothing
                        K = key

                        If Not String.IsNullOrEmpty(K) Then
                            AVariant = New clsVariant()
                            AVariant.VariantValue = K
                            AVariant.Type = VariantType.vtOtherLine
                            answer.AnswerVariants.Add(AVariant)
                        End If
                End Select
            End If
        End If
    End Sub

    Public Shared Function SetUserAttribute(ByVal ID As Guid, ByVal UserID As Integer, ByVal ValueType As AttributeValueTypes, ByVal Value As Object) As Boolean
        Dim context As HttpContext = HttpContext.Current
        Dim app = CType(context.Session("App"), clsComparionCore)
        Dim _with1 = app.ActiveProject.ProjectManager

        If _with1.Attributes IsNot Nothing Then
            Dim res As Boolean = False

            Select Case ValueType
                Case AttributeValueTypes.avtString
                    res = _with1.Attributes.SetAttributeValue(ID, UserID, ValueType, Convert.ToString(Value), Guid.Empty, Guid.Empty)
                Case AttributeValueTypes.avtLong
                    Dim v As Long = 0

                    If Long.TryParse(Convert.ToString(Value), v) Then
                        res = _with1.Attributes.SetAttributeValue(ID, UserID, ValueType, v, Guid.Empty, Guid.Empty)
                    End If

                Case AttributeValueTypes.avtDouble
                    Dim v As Double = 0

                    If String2Double(Convert.ToString(Value), v) Then
                        res = _with1.Attributes.SetAttributeValue(ID, UserID, ValueType, Convert.ToDouble(v), Guid.Empty, Guid.Empty)
                    End If

                Case AttributeValueTypes.avtBoolean
                    Dim v As Boolean = False

                    If Str2Bool(Convert.ToString(Value), v) Then
                        res = _with1.Attributes.SetAttributeValue(ID, UserID, ValueType, v, Guid.Empty, Guid.Empty)
                    End If
            End Select

            Dim ss = _with1.Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, _with1.StorageManager.ProjectLocation, _with1.StorageManager.ProviderType, _with1.StorageManager.ModelID, UserID)
            Return res
        End If

        Return False
    End Function

    Public Shared Function CreateGlobalResults(ByVal [step] As Integer, ByVal Optional normalization As Integer = 1, ByVal Optional customWrtNodeID As Integer = -1) As Object
        Dim wrtNodeId = -1
        Dim wrtNodeName = ""
        Dim app = CType(HttpContext.Current.Session("App"), clsComparionCore)
        Dim globalActionData = CType(Action([step]).ActionData, clsShowGlobalResultsActionData)
        globalActionData.WRTNode = app.ActiveProject.HierarchyObjectives.GetLevelNodes(0)(0)
        wrtNodeId = globalActionData.WRTNode.NodeID
        wrtNodeName = globalActionData.WRTNode.NodeName
        'Dim defaultsort = CInt(app.ActiveProject.PipeParameters.GlobalResultsSortMode)
        Dim hierarchy = New Object(app.ActiveProject.HierarchyObjectives.Nodes.Count - 1)() {}
        Dim highestResult = 0.00

        For i As Integer = 0 To app.ActiveProject.HierarchyObjectives.Nodes.Count - 1
            hierarchy(i) = New Object(2) {}
            hierarchy(i)(0) = app.ActiveProject.HierarchyObjectives.Nodes(i).NodeID
            hierarchy(i)(1) = app.ActiveProject.HierarchyObjectives.Nodes(i).NodeName
            hierarchy(i)(2) = app.ActiveProject.HierarchyObjectives.Nodes(i).WRTRelativeAPriority
        Next

        If customWrtNodeID = -1 Then customWrtNodeID = globalActionData.WRTNode.NodeID
        app.ActiveProject.ProjectManager.CheckCustomCombined()
        Dim dataobject = Anytime.GetOverallResultsData(normalization, customWrtNodeID)
        'Dim resultsData = CType(dataobject(0), List(Of List(Of Object)))

        For Each result As List(Of Object) In CType(dataobject(0), List(Of List(Of Object)))

            If Convert.ToDouble(result(2)) > highestResult Then
                highestResult = Convert.ToDouble(result(2))
            End If

            If Convert.ToDouble(result(3)) > highestResult Then
                highestResult = Convert.ToDouble(result(3))
            End If
        Next

        wrtNodeName = CStr(dataobject(2))
        Dim sEmail As String = app.ActiveUser.UserEmail
        Dim ahpUser As clsUser = app.ActiveProject.ProjectManager.GetUserByEMail(sEmail)
        Dim ahpUserId As Integer = ahpUser.UserID

        If UserIsReadOnly() Then
            ahpUser = app.DBUserByID(GetReadOnlyUserID())
            ahpUserId = ahpUser.UserID
            sEmail = ahpUser.UserEMail
        End If

        Dim showIndex As Boolean = app.ActiveProject.ProjectManager.Parameters.ResultsGlobalShowIndex
        'Dim canshowresults = dataobject(1)
        'Dim messagenote As String() = CType(dataobject(3), String())
        Dim columnValueCombined = "Combined Results"

        If app.ActiveProject.ProjectManager.Parameters.ResultsCustomCombinedName <> "" AndAlso (globalActionData.ResultsViewMode = CanvasTypes.ResultsView.rvGroup OrElse globalActionData.ResultsViewMode = CanvasTypes.ResultsView.rvBoth) Then
            columnValueCombined = app.ActiveProject.ProjectManager.Parameters.ResultsCustomCombinedName
        End If

        Dim pipeParameters = New With {
        Key .WrtNodeID = wrtNodeId,
        Key .Hierarchy = hierarchy,
        Key .highest_result = highestResult,
        Key .defaultsort = CInt(app.ActiveProject.PipeParameters.GlobalResultsSortMode),
        Key .results_data = CType(dataobject(0), List(Of List(Of Object))),
        Key .canshowresult = dataobject(1),
        Key .WrtNodeName = wrtNodeName,
        Key .messagenote = CType(dataobject(3), String()),
        Key .showIndex = showIndex,
        Key .columnValueCombined = columnValueCombined,
        Key .ExpectedValue = Anytime.ExpectedValueString
    }
        Return pipeParameters
    End Function

    Public Shared Function GetPrecisionForRatings(ByVal tScale As clsRatingScale) As Integer
        Dim sPrec As Integer = 0

        If tScale IsNot Nothing Then

            For Each tIntens As clsRating In tScale.RatingSet
                Dim tLen As Integer = tIntens.Value.ToString(CultureInfo.InvariantCulture).Length - 4
                sPrec = If(tLen > sPrec, tLen, sPrec)
            Next

            sPrec = If(sPrec > 3, 3, sPrec)
        End If

        Return sPrec
    End Function

End Class