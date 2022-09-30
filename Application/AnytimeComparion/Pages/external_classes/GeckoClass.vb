Imports ExpertChoice.Web
Imports ExpertChoice.Data
Imports ExpertChoice.Service
Imports ECCore
Imports Canvas
Imports SpyronControls.Spyron.Core
Imports ECWeb = ExpertChoice.Web
Imports ExpertChoice.Results

Public Class GeckoClass
    Private Const SessionNodeSizes As String = "SessionNodeSizes"

    Public Shared ReadOnly Property timeOutMessage As String
        Get
            'if(string.IsNullOrEmpty(_timeOutMessage))
            '_timeOutMessage = TeamTimeClass.ResString("msgTimeoutGecko");
            Dim lTimeOutMessage = TeamTimeClass.ResString("msgTimeoutGecko")
            Return lTimeOutMessage
        End Get
        'set
        '{
        '    _timeOutMessage = value;
        '}
    End Property

    Public Shared Function CreateLogonURL(ByVal tUser As clsApplicationUser, ByVal tProject As clsProject, ByVal isTeamTime As Boolean, ByVal sOtherParams As String, ByVal sPagePath As String, ByVal Optional sPasscode As String = Nothing) As String
        Dim App = Anytime.AppSession()
        Dim sURL As String = ""

        If (tUser IsNot Nothing) Then
            sURL = String.Format("{0}={1}&{2}={3}", _PARAM_EMAIL, HttpUtility.UrlEncode(tUser.UserEmail), _PARAM_PASSWORD, HttpUtility.UrlEncode(tUser.UserPassword))

            If (tProject IsNot Nothing) Then
                sURL += String.Format("&{0}={1}", _PARAM_PASSCODE, HttpUtility.UrlEncode(Convert.ToString((If(String.IsNullOrEmpty(sPasscode), tProject.Passcode, sPasscode)))))
            End If

            sURL += If(isTeamTime, "&TTOnly=1", "&pipe=yes")
        End If

        If Not String.IsNullOrEmpty(sOtherParams) Then
            If Not String.IsNullOrEmpty(sURL) Then sURL += "&"
            sURL += sOtherParams
        End If

        sURL = CryptService.EncodeURL(sURL, App.DatabaseID)
        Dim sLink As String = Convert.ToString((If(sPagePath.Contains("?"), "&", "?")))

        If App.Options.UseTinyURL Then
            Dim PID As Integer = -1
            Dim UID As Integer = -1
            If tProject IsNot Nothing Then PID = tProject.ID
            If tUser IsNot Nothing Then UID = tUser.UserID
            sURL = String.Format("{0}{3}{2}={1}", sPagePath, App.CreateTinyURL(sURL, PID, UID), _PARAMS_TINYURL(0), sLink)
        Else
            sURL = String.Format("{0}{3}{2}={1}", sPagePath, sURL, _PARAMS_KEY(0), sLink)
        End If

        Return sURL
    End Function

    Public Shared Function NodeList(ByVal nodes As List(Of clsNode), ByVal action As clsAction, ByVal Optional isChild As Boolean = False) As Object
        Dim App = Anytime.AppSession()
        Dim Level = New Object(nodes.Count - 1)() {}

        For i As Integer = 0 To Level.GetLength(0) - 1
            Dim priority = GetNodePrty(action, Convert.ToString(nodes(i).NodeID))

            If nodes(i).Children.Count > 0 Then
                Level(i) = New Object(5) {}
                Level(i)(0) = nodes(i).NodeID
                Level(i)(1) = nodes(i).NodeName
                Level(i)(2) = priority
                Level(i)(3) = 1

                If App.ActiveProject.isTeamTime Then
                    Level(i)(4) = TeamTimeClass.TeamTime.PipeBuilder.GetFirstEvalPipeStepForNode(nodes(i), -1) + 1
                Else
                    Level(i)(4) = App.ActiveProject.ProjectManager.PipeBuilder.GetFirstEvalPipeStepForNode(nodes(i), -1) + 1
                End If

                Level(i)(5) = NodeList(nodes(i).Children, action, True)
            Else
                Level(i) = New Object(4) {}
                Level(i)(0) = nodes(i).NodeID
                Level(i)(1) = nodes(i).NodeName
                Level(i)(2) = priority
                If Double.IsNaN(priority) Then Level(i)(2) = 0
                Level(i)(3) = 1

                If App.ActiveProject.isTeamTime Then
                    Level(i)(4) = TeamTimeClass.TeamTime.PipeBuilder.GetFirstEvalPipeStepForNode(CType(nodes(i), clsNode)) + 1
                Else
                    Level(i)(4) = App.ActiveProject.ProjectManager.PipeBuilder.GetFirstEvalPipeStepForNode(nodes(i), -1) + 1
                End If
            End If
        Next

        Return Level
    End Function

    Friend Shared Function GetNodePrty(ByVal action As clsAction, ByVal sNodeId As String) As Double
        Dim sValue As Double = 0.00
        Dim app = Anytime.AppSession()
        'Dim hierarchy = app.ActiveProject.ProjectManager.get_Hierarchy(app.ActiveProject.ProjectManager.ActiveHierarchy)
        Dim hierarchy = app.ActiveProject.ProjectManager.Hierarchy(app.ActiveProject.ProjectManager.ActiveHierarchy)

        If Not String.IsNullOrEmpty(sNodeId) AndAlso True AndAlso hierarchy IsNot Nothing Then
            Dim NID As Integer = -1

            If Integer.TryParse(sNodeId, NID) Then
                Dim tNode As clsNode = hierarchy.GetNodeByID(NID)

                'If hierarchy.ProjectManager.IsRiskProject AndAlso hierarchy.HierarchyID = CInt(ECTypes.ECHierarchyID.hidLikelihood) AndAlso (tNode.get_ParentNode() Is Nothing OrElse tNode.RiskNodeType = ECTypes.RiskNodeType.ntCategory) Then
                If hierarchy.ProjectManager.IsRiskProject AndAlso hierarchy.HierarchyID = CInt(ECTypes.ECHierarchyID.hidLikelihood) AndAlso (tNode.ParentNode() Is Nothing OrElse tNode.RiskNodeType = ECTypes.RiskNodeType.ntCategory) Then
                Else

                    Try
                        Dim globalActionData = CType(action.ActionData, clsShowGlobalResultsActionData)
                        Dim UserID4Tree = app.ActiveProject.ProjectManager.UserID

                        'If globalActionData.ResultsViewMode = Canvas.CanvasTypes.ResultsView.rvGroup OrElse (globalActionData.ResultsViewMode = Canvas.CanvasTypes.ResultsView.rvBoth AndAlso Not globalActionData.get_CanShowIndividualResults(UserID4Tree)) Then
                        If globalActionData.ResultsViewMode = Canvas.CanvasTypes.ResultsView.rvGroup OrElse (globalActionData.ResultsViewMode = Canvas.CanvasTypes.ResultsView.rvBoth AndAlso Not globalActionData.CanShowIndividualResults(UserID4Tree)) Then
                            UserID4Tree = ECTypes.COMBINED_USER_ID
                        End If

                        tNode.CalculateLocal(UserID4Tree)
                        'If tNode IsNot Nothing Then sValue = (100 * tNode.get_LocalPriority(UserID4Tree))
                        If tNode IsNot Nothing Then sValue = (100 * tNode.LocalPriority(UserID4Tree))
                    Catch
                    End Try
                End If
            End If
        End If

        Return sValue
    End Function

    Public Shared Function GetPipeStepHint(ByVal Action As clsAction, ByVal Optional tExtraParam As Object = Nothing, ByVal Optional fCanBePathInteractive As Boolean = False, ByVal Optional fGetResultsCustomTitle As Boolean = False) As String
        Dim App = Anytime.AppSession()
        Dim sRes As String = ""

        If App.HasActiveProject() AndAlso Action IsNot Nothing AndAlso Action.ActionData IsNot Nothing Then

            Dim Hierarchy As clsHierarchy = App.ActiveProject.HierarchyObjectives
            Dim tParams As Dictionary(Of String, String) = New Dictionary(Of String, String)()
            Dim isImpact As Boolean = App.ActiveProject.ProjectManager.ActiveHierarchy = CInt(ECTypes.ECHierarchyID.hidImpact)
            Dim IsRiskWithControls As Boolean = False

            Select Case Action.ActionType
                Case ActionType.atInformationPage
                    Dim sPage As String = ""

                    Select Case (CType(Action.ActionData, clsInformationPageActionData)).Description.ToLower()
                        Case "welcome"
                            sPage = "lblWelcome"
                        Case "thankyou"
                            sPage = "lblThankYou"
                    End Select

                    sRes = String.Format(TeamTimeClass.ResString("lblEvaluationInfoPage"), TeamTimeClass.ResString(sPage))

                Case ActionType.atSpyronSurvey
                    Dim sHint As String = TeamTimeClass.ResString("lblSpyronSurvey")
                    Dim UsersList As Dictionary(Of String, clsComparionUser) = New Dictionary(Of String, clsComparionUser)()
                    UsersList.Add(App.ActiveUser.UserEmail, New SpyronControls.Spyron.Core.clsComparionUser With {
                    .ID = App.ActiveProject.ProjectManager.UserID,
                    .UserName = App.ActiveProject.ProjectManager.User.UserName
                })
                    Dim Data As clsSpyronSurveyAction = CType(Action.ActionData, clsSpyronSurveyAction)
                    App.SurveysManager.ActiveUserEmail = App.ActiveUser.UserEmail
                    Dim tSurvey As SpyronControls.Spyron.Core.clsSurveyInfo = App.SurveysManager.GetSurveyInfoByProjectID(App.ProjectID, CType(Data.SurveyType, SpyronControls.Spyron.Core.SurveyType), UsersList)

                    If (tSurvey IsNot Nothing) Then

                        Dim tmpSurvey As SpyronControls.Spyron.Core.clsSurvey = tSurvey.Survey(App.ActiveUser.UserEmail)

                        If tmpSurvey IsNot Nothing AndAlso Data.StepNumber > 0 AndAlso tmpSurvey.Pages IsNot Nothing AndAlso tmpSurvey.Pages.Count <= Data.StepNumber AndAlso Data.StepNumber > 0 Then
                            Dim tPage As SpyronControls.Spyron.Core.clsSurveyPage = CType(tmpSurvey.Pages(Data.StepNumber - 1), clsSurveyPage)
                            If (tPage IsNot Nothing) Then sHint = String.Format("{0}: {1}", sHint, tPage.Title)
                        End If
                    End If

                    sHint = StringFuncs.ParseStringTemplates(TeamTimeClass.ResString(sHint, True, False), tParams)
                    sRes = sHint
                    Exit Select

                Case ActionType.atShowLocalResults
                    Dim LRData As clsShowLocalResultsActionData = CType(Action.ActionData, clsShowLocalResultsActionData)

                    If (LRData.ParentNode IsNot Nothing) Then
                        tParams.Add(ECWeb.Options._TEMPL_NODENAME, GetWRTNodeNameWithPath(LRData.ParentNode, fCanBePathInteractive))
                        Dim LRfIsPWOutcomes As Boolean = LRData.PWOutcomesNode IsNot Nothing AndAlso LRData.ParentNode.MeasureType() = ECMeasureType.mtPWOutcomes
                        If LRfIsPWOutcomes Then tParams.Add(ECWeb.Options._TEMPL_JUSTNODE, GetWRTNodeNameWithPath(LRData.PWOutcomesNode, fCanBePathInteractive))

                        If tExtraParam IsNot Nothing Then
                            sRes = Convert.ToString((If(LRData.ParentNode.ParentNode() IsNot Nothing, "lblEvaluationResultIntensities", "lblEvaluationResultObjectiveIntensities")))
                        Else

                            If LRfIsPWOutcomes Then

                                If LRData.PWOutcomesNode IsNot Nothing AndAlso LRData.PWOutcomesNode.IsAlternative Then
                                    sRes = Convert.ToString((If(LRData.ParentNode.ParentNode() IsNot Nothing, "lblEvaluationResultPW{0}AltsHierarchy", "lblEvaluationResultPW{0}Alts")))
                                Else
                                    sRes = Convert.ToString((If(LRData.ParentNode.ParentNode() IsNot Nothing, "lblEvaluationResultPW{0}Hierarchy", "lblEvaluationResultPW{0}")))
                                End If

                                Dim sName As String = "Outcomes"

                                If Action.PWONode IsNot Nothing AndAlso Action.PWONode.MeasurementScale IsNot Nothing Then
                                    Dim tRS As clsRatingScale = CType(Action.PWONode.MeasurementScale, clsRatingScale)
                                    If tRS.IsPWofPercentages Then sName = "Percentages"
                                    If tRS.IsExpectedValues Then sName = "ExpectedValues"
                                End If

                                sRes = String.Format(sRes, sName)
                            Else

                                If LRData.ParentNode.IsTerminalNode Then

                                    If App.isRiskEnabled Then

                                        If isImpact Then
                                            sRes = Convert.ToString((If(Hierarchy IsNot Nothing AndAlso Hierarchy.Nodes.Count = 1, "lblEvaluationResultAlternativesNoObjImpact", "lblEvaluationResultAlternativesImpact")))
                                        Else
                                            sRes = Convert.ToString((If(Hierarchy IsNot Nothing AndAlso Hierarchy.Nodes.Count = 1, "lblEvaluationResultAlternativesNoObjsRisk", "lblEvaluationResultAlternativesRisk")))
                                        End If
                                    Else
                                        sRes = "lblEvaluationResultAlternatives"
                                    End If
                                Else

                                    If App.isRiskEnabled Then

                                        If Not isImpact AndAlso LRData.ParentNode.RiskNodeType = ECTypes.RiskNodeType.ntCategory Then
                                            sRes = "lblEvaluationResultObjective_Category"
                                        Else

                                            If LRData.ParentNode.ParentNode() Is Nothing Then
                                                sRes = Convert.ToString((If(isImpact, "lblEvaluationResultObjectiveGoalImpact", "lblEvaluationResultObjectiveGoalRisk")))
                                            Else
                                                sRes = Convert.ToString((If(isImpact, "lblEvaluationResultObjectiveImpact", "lblEvaluationResultObjectiveRisk")))
                                            End If
                                        End If
                                    Else
                                        sRes = "lblEvaluationResultObjective"
                                    End If
                                End If
                            End If
                        End If

                        sRes = TeamTimeClass.ResString(sRes, True, False)

                        If LRData.ParentNode IsNot Nothing AndAlso fGetResultsCustomTitle Then
                            Dim tAddGUID = Guid.Empty

                            If tExtraParam IsNot Nothing AndAlso TypeOf tExtraParam Is clsMeasurementScale Then
                                Dim temp = CType(tExtraParam, clsMeasurementScale)
                                tAddGUID = temp.GuidID
                            End If

                            Dim ClusterTitle = App.ActiveProject.ProjectManager.PipeBuilder.GetClusterTitleForResults(LRData.ParentNode.NodeGuidID, tAddGUID)

                            If ClusterTitle = "" OrElse StringFuncs.HTML2Text(ClusterTitle) = "" Then
                                ClusterTitle = sRes
                            End If

                            Dim ClusterTitleIsCustom = ClusterTitle.Trim().ToLower() <> sRes.Trim().ToLower()

                            If ClusterTitleIsCustom AndAlso ClusterTitle.Trim() <> "" Then
                                sRes = ClusterTitle
                            End If
                        End If

                        sRes = TeamTimeClass.PrepareTask(StringFuncs.ParseStringTemplates(sRes, tParams), tExtraParam)
                    End If

                Case ActionType.atShowGlobalResults

                    If App.ActiveProject.HierarchyObjectives.Nodes.Count > 0 Then
                        tParams.Add(ECWeb.Options._TEMPL_NODENAME, GetWRTNodeNameWithPath(App.ActiveProject.HierarchyObjectives.Nodes(0), fCanBePathInteractive))

                        If App.isRiskEnabled Then
                            sRes = Convert.ToString((If(isImpact, "lblEvaluationResultsOverallImpact", "lblEvaluationResultsOverallRisk")))
                        Else
                            sRes = "lblEvaluationResultsOverall"
                        End If

                        sRes = StringFuncs.ParseStringTemplates(TeamTimeClass.ResString(sRes, True, False), tParams)
                    End If

                Case ActionType.atPairwise, ActionType.atPairwiseOutcomes
                    Dim Act As clsPairwiseMeasureData = CType(Action.ActionData, clsPairwiseMeasureData)
                    Dim parentNode As clsNode = Nothing
                    Dim H As clsHierarchy = Nothing
                    Dim fIsPWOutcomes As Boolean = Action.ActionType = ActionType.atPairwiseOutcomes

                    If fIsPWOutcomes Then
                        parentNode = Action.ParentNode
                    Else
                        parentNode = App.ActiveProject.HierarchyObjectives.GetNodeByID(Act.ParentNodeID)
                        If parentNode Is Nothing Then parentNode = App.ActiveProject.HierarchyAlternatives.GetNodeByID(Act.ParentNodeID)
                        If parentNode IsNot Nothing Then H = CType((If(parentNode.IsAlternative OrElse parentNode.IsTerminalNode, App.ActiveProject.HierarchyAlternatives, App.ActiveProject.HierarchyObjectives)), clsHierarchy)
                    End If

                    Dim tNodeLeft As clsNode = New clsNode()
                    Dim tNodeRight As clsNode = New clsNode()

                    If fIsPWOutcomes Then
                        App.ActiveProject.ProjectManager.PipeBuilder.GetPWNodes(Action, Act, tNodeLeft, tNodeRight)
                        tParams.Add(ECWeb.Options._TEMPL_JUSTNODE, GetWRTNodeNameWithPath(Action.PWONode, fCanBePathInteractive))
                    Else

                        If H IsNot Nothing Then
                            tNodeLeft = H.GetNodeByID(Act.FirstNodeID)
                            tNodeRight = H.GetNodeByID(Act.SecondNodeID)
                        End If
                    End If

                    If parentNode IsNot Nothing Then tParams.Add(ECWeb.Options._TEMPL_NODENAME, GetWRTNodeNameWithPath(parentNode, fCanBePathInteractive))
                    If tNodeLeft IsNot Nothing Then tParams.Add(ECWeb.Options._TEMPL_NODE_A, StringFuncs.JS_SafeHTML(tNodeLeft.NodeName))
                    If tNodeRight IsNot Nothing Then tParams.Add(ECWeb.Options._TEMPL_NODE_B, StringFuncs.JS_SafeHTML(tNodeRight.NodeName))

                    If fIsPWOutcomes Then
                        Dim tRS As clsRatingScale = CType(Action.PWONode.MeasurementScale, clsRatingScale)

                        If tRS IsNot Nothing Then
                            sRes = Convert.ToString((If(parentNode IsNot Nothing AndAlso parentNode.IsAlternative, (If(Action.PWONode IsNot Nothing AndAlso Action.PWONode.ParentNode() Is Nothing, "lblEvaluationPWOutcomesAltsGoal", "lblEvaluationPWOutcomesAlts")), "lblEvaluationPWOutcomes")))
                            If parentNode.Level > 1 Then sRes = "lblEvaluationPWOutcomesLevels"
                            If tRS.IsPWofPercentages Then sRes = "lblEvaluationPWPercentages"
                            If tRS.IsExpectedValues Then sRes = "lblEvaluationExpectedValues"
                        End If
                    Else

                        If App.isRiskEnabled AndAlso parentNode.RiskNodeType = ECTypes.RiskNodeType.ntCategory Then
                            sRes = Convert.ToString((If(parentNode.IsTerminalNode, "lblEvaluationPWAlt_Category", "lblEvaluationPW_Category")))
                        Else
                            sRes = Convert.ToString((If(parentNode.Level = 0, "lblEvaluationPWNoObj", (If(App.isRiskEnabled AndAlso Not isImpact, (If(parentNode.IsTerminalNode, "lblEvaluationPWLikelihoodAlts", "lblEvaluationPWLikelihood")), "lblEvaluationPW")))))
                        End If
                    End If

                    sRes = StringFuncs.ParseStringTemplates(TeamTimeClass.ResString(sRes), tParams)
                Case ActionType.atAllPairwise, ActionType.atAllPairwiseOutcomes
                    Dim tNode As clsNode = (CType(Action.ActionData, clsAllPairwiseEvaluationActionData)).ParentNode

                    If (tNode IsNot Nothing) Then
                        tParams.Add(ECWeb.Options._TEMPL_NODENAME, GetWRTNodeNameWithPath(tNode, fCanBePathInteractive))

                        If Action.ActionType = ActionType.atAllPairwiseOutcomes Then
                            If Action.ParentNode IsNot Nothing Then tParams.Add(ECWeb.Options._TEMPL_JUSTNODE, GetWRTNodeNameWithPath(Action.ParentNode, fCanBePathInteractive))

                            If Action.ParentNode IsNot Nothing AndAlso Action.ParentNode.IsAlternative Then
                                sRes = Convert.ToString((If(tNode IsNot Nothing AndAlso tNode.ParentNode() Is Nothing, "task_MultiPairwise_Alternatives_PW{0}_Goal", "task_MultiPairwise_Alternatives_PW{0}")))
                            Else
                                sRes = Convert.ToString((If(tNode IsNot Nothing AndAlso tNode.ParentNode() Is Nothing, "task_MultiPairwise_Hierarchy_PW{0}", "task_MultiPairwise_Objectives_PW{0}")))
                            End If

                            Dim tRs As clsRatingScale = CType(Action.PWONode.MeasurementScale, clsRatingScale)
                            Dim sName As String = "Outcomes"
                            If tRs.IsPWofPercentages Then sName = "Percentages"
                            If tRs.IsExpectedValues Then sName = "ExpectedValues"
                            sRes = String.Format(sRes, sName)
                        Else
                            sRes = "lblEvaluationAllPW"

                            If App.isRiskEnabled AndAlso Not isImpact Then
                                sRes = Convert.ToString((If(tNode IsNot Nothing AndAlso tNode.IsTerminalNode, "lblEvaluationAllPWLikelihoodAlt", "lblEvaluationAllPWLikelihoodObj")))
                            End If
                        End If

                        sRes = StringFuncs.ParseStringTemplates(TeamTimeClass.ResString(sRes), tParams)
                    End If

                Case ActionType.atNonPWOneAtATime
                    Dim data As clsOneAtATimeEvaluationActionData = CType(Action.ActionData, clsOneAtATimeEvaluationActionData)

                    If IsRiskWithControls Then
                    Else

                        If data IsNot Nothing AndAlso data.Judgment IsNot Nothing Then

                            Select Case data.MeasurementType
                                Case ECMeasureType.mtDirect
                                    Dim tDirect As clsDirectMeasureData = CType(data.Judgment, clsDirectMeasureData)
                                    tParams.Add(ECWeb.Options._TEMPL_NODE_A, StringFuncs.JS_SafeHTML(data.Node.NodeName))
                                    Dim tH As clsHierarchy = Nothing

                                    If Action.IsFeedback And App.ActiveProject.ProjectManager.FeedbackOn Then
                                        tH = App.ActiveProject.HierarchyObjectives
                                    Else

                                        If data.Node.IsTerminalNode Then
                                            tH = App.ActiveProject.HierarchyAlternatives
                                        Else
                                            tH = App.ActiveProject.HierarchyObjectives
                                        End If
                                    End If

                                    tParams.Add(ECWeb.Options._TEMPL_NODENAME, GetWRTNodeNameWithPath(tH.GetNodeByID(tDirect.NodeID), fCanBePathInteractive))

                                    If App.isRiskEnabled Then

                                        If isImpact Then
                                            sRes = Convert.ToString((If(data.Node.Level > 0, "lblEvaluationDirectImpact", "lblEvaluationDirectImpactNoLevels")))
                                        Else
                                            sRes = Convert.ToString((If(data.Node.Level = 0, "lblEvaluationDirectRiskNoObj", "lblEvaluationDirectRisk")))
                                        End If

                                        tParams.Add(ECWeb.Options._TEMPL_NODETYPE, Convert.ToString((If(data.Node.IsTerminalNode, ECWeb.Options._TEMPL_ALTERNATIVE, ECWeb.Options._TEMPL_OBJECTIVE))))
                                    Else
                                        sRes = "lblEvaluationDirect"
                                    End If

                                    sRes = StringFuncs.ParseStringTemplates(TeamTimeClass.ResString(sRes), tParams)
                                Case ECMeasureType.mtStep
                                    Dim tStep As clsStepMeasureData = CType(data.Judgment, clsStepMeasureData)
                                    Dim tParentNode As clsNode = CType(data.Node.Hierarchy.GetNodeByID(tStep.ParentNodeID), clsNode)
                                    Dim tAlt As clsNode = Nothing

                                    If tParentNode.IsTerminalNode Then
                                        tAlt = data.Node.Hierarchy.ProjectManager.AltsHierarchy(data.Node.Hierarchy.ProjectManager.ActiveAltsHierarchy).GetNodeByID(tStep.NodeID)
                                    Else
                                        tAlt = data.Node.Hierarchy.GetNodeByID(tStep.NodeID)
                                    End If

                                    tParams.Add(ECWeb.Options._TEMPL_NODE_A, GetWRTNodeNameWithPath(tParentNode, fCanBePathInteractive))
                                    tParams.Add(ECWeb.Options._TEMPL_NODENAME, StringFuncs.JS_SafeHTML(tAlt.NodeName))

                                    If App.isRiskEnabled Then

                                        If isImpact Then
                                            sRes = Convert.ToString((If(tParentNode.ParentNode() Is Nothing, "lblEvaluationStepGoalImpact", "lblEvaluationStepImpact")))
                                        Else
                                            sRes = Convert.ToString((If(tParentNode.ParentNode() Is Nothing, "lblEvaluationStepGoalRisk", "lblEvaluationStepRisk")))
                                        End If
                                    Else
                                        sRes = "lblEvaluationStep"
                                    End If

                                    sRes = StringFuncs.ParseStringTemplates(TeamTimeClass.ResString(sRes), tParams)
                                Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtCustomUtilityCurve
                                    Dim CurvetParentNode As clsNode = CType(data.Node.Hierarchy.GetNodeByID((CType(data.Judgment, clsUtilityCurveMeasureData)).ParentNodeID), clsNode)
                                    tParams.Add(ECWeb.Options._TEMPL_NODE_A, GetWRTNodeNameWithPath(CurvetParentNode, fCanBePathInteractive))
                                    Dim CurvetAlt As clsNode = Nothing
                                    Dim tAlt As clsNode = Nothing

                                    If CurvetParentNode.IsTerminalNode Then
                                        tAlt = data.Node.Hierarchy.ProjectManager.AltsHierarchy(data.Node.Hierarchy.ProjectManager.ActiveAltsHierarchy).GetNodeByID((CType(data.Judgment, clsUtilityCurveMeasureData)).NodeID)
                                    Else
                                        tAlt = data.Node.Hierarchy.GetNodeByID((CType(data.Judgment, clsUtilityCurveMeasureData)).NodeID)
                                    End If

                                    tParams.Add(ECWeb.Options._TEMPL_NODENAME, StringFuncs.JS_SafeHTML(tAlt.NodeName))

                                    Select Case data.MeasurementType
                                        Case ECMeasureType.mtAdvancedUtilityCurve
                                            sRes = "task_AdvancedUtilityCurve"
                                        Case Else

                                            If App.isRiskEnabled Then

                                                If isImpact Then
                                                    sRes = Convert.ToString((If(CurvetParentNode.ParentNode() Is Nothing, "lblEvaluationUCGoalImpact", "lblEvaluationUCImpact")))
                                                Else
                                                    sRes = Convert.ToString((If(CurvetParentNode.ParentNode() Is Nothing, "lblEvaluationUCGoalRisk", "lblEvaluationUCRisk")))
                                                End If
                                            Else
                                                sRes = "lblEvaluationUC"
                                            End If
                                    End Select

                                    sRes = StringFuncs.ParseStringTemplates(TeamTimeClass.ResString(sRes), tParams)
                                Case Else
                                    Dim isAlt As Boolean = data.Node IsNot Nothing AndAlso (data.Node.IsAlternative OrElse data.Node.IsTerminalNode)
                                    Dim tData As clsNonPairwiseMeasureData = CType(data.Judgment, clsNonPairwiseMeasureData)
                                    Dim curvetNode As clsNode = Nothing
                                    Dim tNode As clsNode = Nothing

                                    If isAlt Then
                                        tNode = App.ActiveProject.HierarchyAlternatives.GetNodeByID(tData.NodeID)
                                    Else
                                        tNode = App.ActiveProject.HierarchyObjectives.GetNodeByID(tData.NodeID)

                                        If tNode Is Nothing Then
                                            tNode = App.ActiveProject.HierarchyAlternatives.GetNodeByID(tData.NodeID)
                                            isAlt = True
                                        End If
                                    End If

                                    If tNode IsNot Nothing Then tParams.Add(ECWeb.Options._TEMPL_NODE_A, StringFuncs.JS_SafeHTML(tNode.NodeName))
                                    tParams.Add(ECWeb.Options._TEMPL_NODE_B, StringFuncs.JS_SafeHTML(data.Node.NodeName))
                                    Dim fHasLevels As Boolean = data.Node.Level > 0

                                    If App.isRiskEnabled Then

                                        If isImpact Then
                                            sRes = Convert.ToString((If(fHasLevels, "lblEvaluationRatingImpact", "lblEvaluationRatingImpactNoLevels")))
                                        Else
                                            sRes = Convert.ToString((If(fHasLevels, "lblEvaluationRatingRisk", "lblEvaluationRatingNoLevelsRisk")))
                                        End If
                                    Else
                                        sRes = Convert.ToString((If(fHasLevels, "lblEvaluationRating", "lblEvaluationRatingNoLevels")))
                                    End If

                                    If data.Node IsNot Nothing AndAlso Not isAlt Then sRes += "Obj"
                                    sRes = StringFuncs.ParseStringTemplates(TeamTimeClass.ResString(sRes), tParams)
                            End Select
                        End If
                    End If

                Case ActionType.atNonPWAllChildren
                    Dim NPCdata As clsAllChildrenEvaluationActionData = CType(Action.ActionData, clsAllChildrenEvaluationActionData)

                    If NPCdata IsNot Nothing AndAlso NPCdata.ParentNode IsNot Nothing Then

                        Select Case NPCdata.MeasurementType
                            Case ECMeasureType.mtRatings

                                If App.isRiskEnabled Then

                                    If isImpact Then
                                        sRes = Convert.ToString((If(NPCdata.ParentNode.IsAlternative, "task_MultiRatings_AllCovObjImpact", (If(NPCdata.ParentNode.ParentNode() Is Nothing, "task_MultiRatings_AllAltsGoalImpact", "task_MultiRatings_AllAltsImpact")))))
                                    Else
                                        sRes = Convert.ToString((If(NPCdata.ParentNode Is Nothing OrElse NPCdata.ParentNode.ParentNode() Is Nothing, "lblEvaluationMultiDirectDataLikelihood", (If(NPCdata.ParentNode.IsTerminalNode, "task_MultiRatings_AllAltsRisk", (If(NPCdata.ParentNode.RiskNodeType = ECTypes.RiskNodeType.ntCategory, "task_MultiRatings_AllObjRisk_Cat", "task_MultiRatings_AllObjRisk")))))))
                                    End If

                                    If Hierarchy IsNot Nothing AndAlso Hierarchy.Nodes.Count = 1 Then sRes = "task_MultiRatings_AllAlts_NoObj"
                                Else
                                    sRes = "task_MultiRatings_AllAlts"
                                End If

                            Case ECMeasureType.mtDirect

                                If App.isRiskEnabled Then

                                    If NPCdata.ParentNode.IsTerminalNode Then

                                        If isImpact Then
                                            sRes = Convert.ToString((If(NPCdata.ParentNode.Level = 0, "lblEvaluationMultiDirectDataAltsGoalRisk", "lblEvaluationMultiDirectDataAltsRisk")))
                                        Else
                                            sRes = Convert.ToString((If(NPCdata.ParentNode.Level = 0, "lblEvaluationMultiDirectDataAltsGoalLikelihood", "lblEvaluationMultiDirectDataAltsLikelihood")))
                                        End If
                                    Else

                                        If Not isImpact Then
                                            sRes = Convert.ToString((If(NPCdata.ParentNode.Level > 0, "lblEvaluationMultiDirectDataLevelsLikelihood", "lblEvaluationMultiDirectDataLikelihood")))
                                        Else
                                            sRes = Convert.ToString((If(NPCdata.ParentNode.ParentNode() Is Nothing, "lblEvaluationMultiDirectDataGoalRisk", "lblEvaluationMultiDirectDataRiskObj")))
                                        End If
                                    End If
                                Else

                                    If NPCdata.ParentNode.IsTerminalNode Then
                                        sRes = "lblEvaluationMultiDirectDataAlts"
                                    Else
                                        sRes = "lblEvaluationMultiDirectData"
                                    End If
                                End If
                        End Select

                        tParams.Add(ECWeb.Options._TEMPL_NODENAME, GetWRTNodeNameWithPath(NPCdata.ParentNode, fCanBePathInteractive))
                        tParams.Add(ECWeb.Options._TEMPL_EVALCOUNT, Convert.ToString(NPCdata.Children.Count))
                        sRes = StringFuncs.ParseStringTemplates(TeamTimeClass.ResString(sRes), tParams)
                    End If

                Case ActionType.atNonPWAllCovObjs
                    Dim NPAdata As clsAllCoveringObjectivesEvaluationActionData = CType(Action.ActionData, clsAllCoveringObjectivesEvaluationActionData)

                    If (NPAdata IsNot Nothing) Then
                        tParams.Add(ECWeb.Options._TEMPL_NODENAME, GetWRTNodeNameWithPath(NPAdata.Alternative, fCanBePathInteractive))

                        If NPAdata.MeasurementType = ECMeasureType.mtDirect Then

                            If App.isRiskEnabled Then
                                sRes = Convert.ToString((If(Hierarchy.Nodes.Count <= 1, "lblEvaluationAllCovObjsRiskNoObj", "lblEvaluationAllCovObjsRisk")))
                            Else
                                sRes = "lblEvaluationAllCovObjs"
                            End If
                        End If

                        If NPAdata.MeasurementType = ECMeasureType.mtRatings Then

                            If App.isRiskEnabled Then

                                If isImpact Then
                                    sRes = Convert.ToString((If(App.ActiveProject.HierarchyObjectives.GetMaxLevel() < 1, "task_MultiRatings_AllCovObjGoalImpact", "task_MultiRatings_AllCovObjImpact")))
                                Else
                                    sRes = Convert.ToString((If(App.ActiveProject.HierarchyObjectives.GetMaxLevel() < 1, "task_MultiRatings_AllCovObjGoal", "task_MultiRatings_AllCovObj")))
                                End If
                            Else
                                sRes = "lblEvaluationAllCovObjsRatings"
                            End If
                        End If

                        sRes = StringFuncs.ParseStringTemplates(TeamTimeClass.ResString(sRes), tParams)
                    End If

                Case ActionType.atAllEventsWithNoSource
                    Dim AEWdata As clsAllEventsWithNoSourceEvaluationActionData = CType(Action.ActionData, clsAllEventsWithNoSourceEvaluationActionData)

                    If (AEWdata IsNot Nothing) Then

                        Select Case AEWdata.MeasurementType
                            Case ECMeasureType.mtRatings, ECMeasureType.mtDirect
                                sRes = "lblEvaluationNoSources"
                        End Select
                    End If

                    If Not String.IsNullOrEmpty(sRes) Then sRes = StringFuncs.ParseStringTemplates(TeamTimeClass.ResString(sRes), tParams)
                Case ActionType.atSensitivityAnalysis

                    Select Case (CType(Action.ActionData, clsSensitivityAnalysisActionData)).SAType
                        Case SAType.satDynamic
                            sRes = TeamTimeClass.ResString("lblEvaluationDynamicSA")
                        Case SAType.satGradient
                            sRes = TeamTimeClass.ResString("lblEvaluationGradientSA")
                        Case SAType.satPerformance
                            sRes = TeamTimeClass.ResString("lblEvaluationPerformanceSA")
                    End Select
            End Select
        End If

        Return TeamTimeClass.PrepareTask(sRes, tExtraParam)
    End Function

    Public Shared Function GetWRTNodeNameWithPath(ByVal tNode As clsNode, ByVal CanBeInteractive As Boolean) As String
        Dim App = Anytime.AppSession()
        Dim DontShowPath As Boolean = App.ActiveProject.PipeParameters.ShowFullObjectivePath = PipeParameters.ecShowObjectivePath.DontShowPath
        Dim sName As String = ""

        If tNode IsNot Nothing Then
            sName = StringFuncs.JS_SafeHTML(tNode.NodeName)
            Dim sDivider As String = StringFuncs.JS_SafeHTML(TeamTimeClass.ResString("lblObjectivePathDivider"))
            Dim sPath As String = ""

            While tNode.ParentNode() IsNot Nothing
                If tNode.ParentNode().ParentNode() IsNot Nothing Then sPath = StringFuncs.JS_SafeHTML(tNode.ParentNode().NodeName) & sDivider & sPath
                tNode = tNode.ParentNode()
            End While

            If DontShowPath AndAlso CanBeInteractive AndAlso Not String.IsNullOrEmpty(sPath) Then
            Else

                If Not DontShowPath Then
                    Return sPath & sName
                Else
                    Return sName
                End If
            End If
        End If

        Return sName
    End Function

    Public Shared Function GetInfodocParams(ByVal NodeID As Guid, ByVal WRTNodeID As Guid, ByVal Optional is_multi As Boolean = False) As String
        'expand
        Dim App = Anytime.AppSession()
        Dim test = App.ActiveProject.ProjectManager.Attributes.GetAttributeValue(Attributes.ATTRIBUTE_INFODOC_PARAMS_GECKO_ID, NodeID, WRTNodeID).ToString()

        If test Is Nothing Then
            test = ""
        End If

        Return test
    End Function

    Public Shared Function GetWelcomeThankYouIncFile(ByVal fIsThankYou As Boolean, ByVal fIsImpact As Boolean, ByVal fIsOpportunity As Boolean) As String
        Dim App = Anytime.AppSession()
        Dim sName As String = ""

        If App.isRiskEnabled Then

            If fIsThankYou Then

                If fIsOpportunity Then
                    sName = Convert.ToString((If(fIsImpact, Consts._FILE_TEMPL_THANKYOU_IMPACT_OPPORTUNITY, Consts._FILE_TEMPL_THANKYOU_LIKELIHOOD_OPPORTUNITY)))
                Else
                    sName = Convert.ToString((If(fIsImpact, Consts._FILE_TEMPL_THANKYOU_IMPACT, Consts._FILE_TEMPL_THANKYOU_LIKELIHOOD)))
                End If
            Else

                If fIsOpportunity Then
                    sName = Convert.ToString((If(fIsImpact, Consts._FILE_TEMPL_WELCOME_IMPACT_OPPORTUNITY, Consts._FILE_TEMPL_WELCOME_LIKELIHOOD_OPPORTUNITY)))
                Else
                    sName = Convert.ToString((If(fIsImpact, Consts._FILE_TEMPL_WELCOME_IMPACT, Consts._FILE_TEMPL_WELCOME_LIKELIHOOD)))
                End If
            End If
        Else

            If fIsOpportunity Then
                sName = Convert.ToString((If(fIsThankYou, Consts._FILE_TEMPL_THANKYOU_OPPORTUNITY, Consts._FILE_TEMPL_WELCOME_EVALUATE_OPPORTUNITY)))
            Else
                sName = Convert.ToString((If(fIsThankYou, Consts._FILE_TEMPL_THANKYOU, Consts._FILE_TEMPL_WELCOME_EVALUATE)))
            End If
        End If

        sName = GetIncFile(sName)
        If App.isRiskEnabled AndAlso Not System.IO.Directory.Exists(sName) Then sName = GetIncFile(Convert.ToString((If(fIsThankYou, Consts._FILE_TEMPL_THANKYOU, Consts._FILE_TEMPL_WELCOME_EVALUATE))))
        Return sName
    End Function

    Public Shared Function GetIncFile(ByVal sFilename As String) As String
        Dim App = Anytime.AppSession()
        Dim sPath As String = Consts._FILE_DATA_INC + App.LanguageCode & "\" & sFilename
        If System.IO.File.Exists(sPath) Then Return sPath
        sPath = Consts._FILE_DATA_INC + Consts._LANG_DEFCODE & "\" & sFilename

        If System.IO.File.Exists(sPath) Then
            Return sPath
        Else
            Return Consts._FILE_DATA_INC & sFilename
        End If
    End Function

    Public Shared Function loadStepButtons(ByVal [step] As Integer) As String
        Dim context As HttpContext = HttpContext.Current
        Dim app = Anytime.AppSession()
        Dim stepButtons = ""
        Dim totalStep = app.ActiveProject.Pipe.Count
        Dim left = 15
        Dim right = 15

        If [step] < 15 Then
            right = 15 + [step]
        ElseIf [step] > totalStep - 15 Then
            Dim no = [step] - (totalStep - 15)
            left = 15 + no
        End If

        For i As Integer = 0 To totalStep - 1

            If i = 0 Then
                stepButtons = AnytimeClass.GetStepData(i, stepButtons, True)
                stepButtons += ","
            ElseIf i = totalStep - 1 Then
                stepButtons = AnytimeClass.GetStepData(i, stepButtons, True)
            ElseIf i >= [step] - left AndAlso i <= [step] + right Then
                stepButtons = AnytimeClass.GetStepData(i, stepButtons, True)
                stepButtons += ","
            End If
        Next

        Return stepButtons
    End Function

    Public Shared Function GetEvalPipeStepsList(ByVal wrtnodeID As Integer, ByVal CurrentStep As Integer, ByVal OutcomesNode As clsNode) As List(Of StepsPairs)
        Dim App = Anytime.AppSession()
        Dim list As List(Of StepsPairs) = New List(Of StepsPairs)()
        Dim action As clsAction = Nothing

        For i As Integer = 0 To CurrentStep - 1
            action = CType(App.ActiveProject.ProjectManager.Pipe(i), clsAction)

            Select Case action.ActionType
                Case ActionType.atPairwise, ActionType.atPairwiseOutcomes
                    Dim pwd As ECCore.clsPairwiseMeasureData = CType(action.ActionData, ECCore.clsPairwiseMeasureData)

                    If (action.ActionType = ActionType.atPairwise And pwd.ParentNodeID = wrtnodeID) Or (action.ActionType = ActionType.atPairwiseOutcomes And action.ParentNode IsNot Nothing AndAlso action.ParentNode.NodeID = wrtnodeID) Then
                        Dim pair As StepsPairs = New StepsPairs()
                        pair.Obj1 = pwd.FirstNodeID
                        pair.Obj2 = pwd.SecondNodeID
                        pair.Value = pwd.Value
                        pair.Advantage = pwd.Advantage
                        pair.IsUndefined = pwd.IsUndefined
                        pair.StepNumber = i
                        list.Add(pair)
                    End If

                Case ActionType.atAllPairwise
                    Dim pwd2 As clsAllPairwiseEvaluationActionData = CType(action.ActionData, clsAllPairwiseEvaluationActionData)

                    If pwd2.Judgments IsNot Nothing AndAlso pwd2.Judgments.Count > 0 Then

                        For Each judgment As clsPairwiseMeasureData In pwd2.Judgments

                            If judgment.ParentNodeID = wrtnodeID Then
                                Dim pair As StepsPairs = New StepsPairs()
                                pair.Obj1 = judgment.FirstNodeID
                                pair.Obj2 = judgment.SecondNodeID
                                pair.Value = judgment.Value
                                pair.Advantage = judgment.Advantage
                                pair.IsUndefined = judgment.IsUndefined
                                pair.StepNumber = i
                                list.Add(pair)
                            End If
                        Next
                    End If

                Case ActionType.atAllPairwiseOutcomes
                    Dim pwd3 As clsAllPairwiseEvaluationActionData = CType(action.ActionData, clsAllPairwiseEvaluationActionData)

                    If pwd3.ParentNode.NodeID = wrtnodeID Then

                        If pwd3.Judgments IsNot Nothing AndAlso pwd3.Judgments.Count > 0 Then

                            For Each judgment As clsPairwiseMeasureData In pwd3.Judgments
                                Dim pair As StepsPairs = New StepsPairs()
                                pair.Obj1 = judgment.FirstNodeID
                                pair.Obj2 = judgment.SecondNodeID
                                pair.Value = judgment.Value
                                pair.Advantage = judgment.Advantage
                                pair.IsUndefined = judgment.IsUndefined
                                pair.StepNumber = i
                                list.Add(pair)
                            Next
                        End If
                    End If
            End Select
        Next

        Dim parentNode As clsNode = Nothing

        If OutcomesNode IsNot Nothing Then
            parentNode = OutcomesNode
        Else
            parentNode = App.ActiveProject.ProjectManager.Hierarchy(App.ActiveProject.ProjectManager.ActiveHierarchy).GetNodeByID(wrtnodeID)
        End If

        If parentNode IsNot Nothing Then
            Dim judgments As List(Of clsCustomMeasureData) = Nothing

            If OutcomesNode IsNot Nothing Then
                judgments = parentNode.PWOutcomesJudgments.JudgmentsFromUser(App.ActiveProject.ProjectManager.User.UserID)
            Else
                judgments = parentNode.Judgments.JudgmentsFromUser(App.ActiveProject.ProjectManager.User.UserID)
            End If

            For Each J As clsCustomMeasureData In judgments

                If Not J.IsUndefined Then

                    Try
                        Dim jj = J
                        Dim pwData As clsPairwiseMeasureData = CType(jj, clsPairwiseMeasureData)
                        Dim exists As Boolean = False

                        For Each Pair As StepsPairs In list

                            If (Pair.Obj1 = pwData.FirstNodeID And Pair.Obj2 = pwData.SecondNodeID) Or (Pair.Obj2 = pwData.FirstNodeID And Pair.Obj1 = pwData.SecondNodeID) Then
                                exists = True
                            End If
                        Next

                        If Not exists Then
                            Dim pair As StepsPairs = New StepsPairs()
                            pair.Obj1 = pwData.FirstNodeID
                            pair.Obj2 = pwData.SecondNodeID
                            pair.Value = pwData.Value
                            pair.Advantage = pwData.Advantage
                            pair.IsUndefined = pwData.IsUndefined
                            pair.StepNumber = -1
                            list.Add(pair)
                        End If

                    Catch
                    End Try
                End If
            Next
        End If

        Return list
    End Function

    Public Shared Function getCommonParams(ByVal node_params As String) As String
        Dim delimiterChars As Char() = {"&"c}
        Dim params_str As String() = node_params.Split(delimiterChars)
        Dim tmp_str As String = ""

        If params_str.Length > 0 Then

            For Each s As String In params_str

                If Not s.Contains("t"c) Then

                    If tmp_str <> "" Then
                        tmp_str += "&" & s
                    Else
                        tmp_str += s
                    End If
                End If
            Next
        End If

        Return tmp_str
    End Function

    Public Shared Sub SetInfodocParams(ByVal NodeID As Guid, ByVal WRTNodeID As Guid, ByVal value As String, ByVal Optional is_multi As Boolean = False)
        Dim App = Anytime.AppSession()

        If App IsNot Nothing Then
            Dim isPM As Boolean = App.CanUserModifyProject(App.ActiveUser.UserID, App.ProjectID, AnytimeClass.Uw, AnytimeClass.Ws, App.ActiveWorkgroup)

            If isPM Then
                Dim value_params = value.Split("&"c)

                If value_params.Length < 4 Then
                    Dim temp_value = GetInfodocParams(NodeID, WRTNodeID)
                    Dim temp_value_arr = temp_value.Split("&"c)

                    If temp_value_arr.Length >= 4 Then
                        value = value_params(0) & "&" & value_params(1) & "&" & value_params(2) & "&" & temp_value_arr(3)
                    End If
                End If

                If is_multi Then
                    App.ActiveProject.ProjectManager.Attributes.SetAttributeValue(ATTRIBUTE_INFODOC_PARAMS_GECKO_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtString, value, NodeID, WRTNodeID)
                Else
                    App.ActiveProject.ProjectManager.Attributes.SetAttributeValue(ATTRIBUTE_INFODOC_PARAMS_GECKO_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtString, value, NodeID, WRTNodeID)
                End If

                App.ActiveProject.ProjectManager.Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, App.ActiveProject.ProjectManager.StorageManager.ProjectLocation, App.ActiveProject.ProjectManager.StorageManager.ProviderType, App.ActiveProject.ProjectManager.StorageManager.ModelID, UNDEFINED_USER_ID)
            End If
        End If
    End Sub

    Public Shared Function GetInfoDocData(ByVal node_type As String, ByVal node_location As String, ByVal current_step As Integer, ByVal Optional node_id As Integer = 0, ByVal Optional node_guid As String = "") As Object
        Dim context As HttpContext = HttpContext.Current
        Dim App = Anytime.AppSession()
        Dim _ObjectType = Consts.reObjectType.Unspecified
        Dim CurrentProject = App.ActiveProject
        Dim tempnode = New clsNode()
        Dim sBaseURL = Consts._FILE_ROOT
        Dim ObjHierarchy = CType(CurrentProject.HierarchyObjectives, clsHierarchy)
        Dim ParentNode As clsNode = Nothing
        Dim FirstNode As clsNode = Nothing
        Dim SecondNode As clsNode = Nothing
        Dim is_CovObj As Boolean = False
        Dim is_Multi As Boolean = False
        Dim path As String = Consts._FILE_MHT_FILES
        Dim tAction As clsAction = Nothing
        Dim nodes = New List(Of Object)()
        Dim TeamTimeData = CType(context.Session("team"), clsTeamTimePipe)

        If TeamTimeClass.isTeamTime Then
            tAction = TeamTimeClass.GetAction(current_step)
            nodes = CType(TeamTimeClass.getnodes(current_step), List(Of Object))
        Else
            tAction = AnytimeClass.GetAction(current_step)
            nodes = CType(AnytimeClass.Getnodes(current_step), List(Of Object))
        End If

        Dim qh_help_id = New PipeParameters.ecEvaluationStepType()

        Select Case tAction.ActionType
            Case ActionType.atInformationPage
                Dim info As clsInformationPageActionData = CType(tAction.ActionData, clsInformationPageActionData)

                If info.Description.ToLower() = "welcome" Then
                    qh_help_id = PipeParameters.ecEvaluationStepType.Welcome
                Else
                    qh_help_id = PipeParameters.ecEvaluationStepType.ThankYou
                End If

            Case ActionType.atPairwise
                ParentNode = CType(nodes(6), clsNode)
                FirstNode = CType(nodes(4), clsNode)
                SecondNode = CType(nodes(5), clsNode)
                Dim PairwiseType = App.ActiveProject.ProjectManager.PipeBuilder.GetPairwiseTypeForNode(ParentNode).ToString()

                If PairwiseType = "ptVerbal" Then
                    qh_help_id = PipeParameters.ecEvaluationStepType.VerbalPW
                Else
                    qh_help_id = PipeParameters.ecEvaluationStepType.GraphicalPW
                End If

            Case ActionType.atNonPWOneAtATime
                Dim temp_ActionData = CType(tAction.ActionData, clsOneAtATimeEvaluationActionData)

                If temp_ActionData.Node IsNot Nothing AndAlso temp_ActionData.Judgment IsNot Nothing Then
                    Dim measuretype = CType(tAction.ActionData, clsNonPairwiseEvaluationActionData)

                    Select Case (CType(tAction.ActionData, clsNonPairwiseEvaluationActionData)).MeasurementType
                        Case ECMeasureType.mtRatings
                            qh_help_id = PipeParameters.ecEvaluationStepType.Ratings
                            ParentNode = CType(nodes(4), clsNode)
                            FirstNode = CType(nodes(5), clsNode)
                        Case ECMeasureType.mtDirect
                            qh_help_id = PipeParameters.ecEvaluationStepType.DirectInput
                            ParentNode = CType(nodes(4), clsNode)
                            FirstNode = CType(nodes(5), clsNode)
                        Case ECMeasureType.mtStep
                            qh_help_id = PipeParameters.ecEvaluationStepType.StepFunction
                            ParentNode = CType(nodes(4), clsNode)
                            FirstNode = CType(nodes(5), clsNode)
                        Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                            qh_help_id = PipeParameters.ecEvaluationStepType.UtilityCurve
                            ParentNode = CType(nodes(4), clsNode)
                            FirstNode = CType(nodes(5), clsNode)
                    End Select
                End If

            Case ActionType.atAllPairwise, ActionType.atAllPairwiseOutcomes
                is_Multi = True
                Dim fIsPWOutcomes As Boolean = tAction.ActionType = ActionType.atAllPairwiseOutcomes
                Dim AllPwData As clsAllPairwiseEvaluationActionData = CType(tAction.ActionData, clsAllPairwiseEvaluationActionData)
                ParentNode = AllPwData.ParentNode
                Dim fAlts As Boolean = AllPwData.ParentNode.IsTerminalNode

                If fAlts Then
                    FirstNode = App.ActiveProject.HierarchyAlternatives.GetNodeByID(node_id)
                    SecondNode = App.ActiveProject.HierarchyAlternatives.GetNodeByID(node_id)
                Else
                    FirstNode = App.ActiveProject.HierarchyObjectives.GetNodeByID(node_id)
                    SecondNode = App.ActiveProject.HierarchyObjectives.GetNodeByID(node_id)
                End If

                Dim PWType = App.ActiveProject.ProjectManager.PipeBuilder.GetPairwiseTypeForNode(AllPwData.ParentNode)

                If PWType = CanvasTypes.PairwiseType.ptVerbal AndAlso AllPwData.Judgments.Count <= 3 Then
                    qh_help_id = PipeParameters.ecEvaluationStepType.VerbalPW

                    If (fAlts AndAlso App.ActiveProject.PipeParameters.ForceGraphicalForAlternatives) OrElse (Not fAlts AndAlso App.ActiveProject.PipeParameters.ForceGraphical) Then
                        PWType = CanvasTypes.PairwiseType.ptGraphical
                        qh_help_id = PipeParameters.ecEvaluationStepType.GraphicalPW
                    End If
                End If

            Case ActionType.atNonPWAllChildren, ActionType.atNonPWAllCovObjs
                is_Multi = True
                ParentNode = CType(nodes(4), clsNode)

                If node_type <> "0" Then
                    Dim temp_node_id = Guid.Empty

                    If node_guid <> "" And node_guid <> "0" Then
                        temp_node_id = New Guid(node_guid)
                    End If

                    If temp_node_id <> Guid.Empty Then
                        FirstNode = CurrentProject.HierarchyAlternatives.GetNodeByID(temp_node_id)

                        If FirstNode Is Nothing Then
                            FirstNode = CurrentProject.HierarchyObjectives.GetNodeByID(temp_node_id)
                        End If
                    End If

                    If FirstNode Is Nothing Then
                        FirstNode = CurrentProject.HierarchyAlternatives.GetNodeByID(node_id)

                        If FirstNode Is Nothing Then
                            FirstNode = CurrentProject.HierarchyObjectives.GetNodeByID(temp_node_id)
                        End If
                    End If

                    SecondNode = FirstNode
                End If

                Dim now_pw_all As clsNonPairwiseEvaluationActionData = CType(tAction.ActionData, clsNonPairwiseEvaluationActionData)

                Select Case now_pw_all.MeasurementType
                    Case ECMeasureType.mtRatings
                        qh_help_id = PipeParameters.ecEvaluationStepType.MultiRatings
                    Case ECMeasureType.mtDirect
                        qh_help_id = PipeParameters.ecEvaluationStepType.MultiDirectInput
                End Select

            Case ActionType.atShowLocalResults
                is_Multi = True
                ParentNode = CType(nodes(4), clsNode)
                qh_help_id = PipeParameters.ecEvaluationStepType.IntermediateResults
            Case ActionType.atShowGlobalResults
                ParentNode = CType(nodes(4), clsNode)
                qh_help_id = PipeParameters.ecEvaluationStepType.OverallResults
            Case ActionType.atSpyronSurvey
                qh_help_id = PipeParameters.ecEvaluationStepType.Survey
            Case ActionType.atSensitivityAnalysis
                Dim sensitivities As clsSensitivityAnalysisActionData = CType(tAction.ActionData, clsSensitivityAnalysisActionData)

                Select Case sensitivities.SAType
                    Case SAType.satDynamic
                        qh_help_id = PipeParameters.ecEvaluationStepType.DynamicSA
                    Case SAType.satGradient
                        qh_help_id = PipeParameters.ecEvaluationStepType.GradientSA
                    Case SAType.satPerformance
                        qh_help_id = PipeParameters.ecEvaluationStepType.PerformanceSA
                End Select
        End Select

        Dim ParentNodeID = -1

        If ParentNode IsNot Nothing Then
            ParentNodeID = ParentNode.NodeID
        End If

        If node_type <> "0" AndAlso node_type <> "quick-help" Then

            If node_location = "1" Then
                tempnode = FirstNode
            Else
                tempnode = SecondNode
            End If

            If is_CovObj Then

                If node_type = "-1" Then
                    tempnode = ParentNode
                End If
            End If

            If node_type = "2" OrElse (is_CovObj AndAlso node_type = "-1") Then
                _ObjectType = If(tempnode IsNot Nothing AndAlso tempnode.IsAlternative, Consts.reObjectType.Alternative, Consts.reObjectType.Node)
            Else

                If node_type = "3" Then
                    _ObjectType = Consts.reObjectType.AltWRTNode
                ElseIf node_type = "4" Then
                    _ObjectType = Consts.reObjectType.MeasureScale
                    ParentNodeID = -1
                    tempnode = ParentNode
                Else
                    _ObjectType = Consts.reObjectType.Node
                    ParentNodeID = -1
                    tempnode = ParentNode
                End If
            End If

            Try

                If is_CovObj AndAlso node_type = "3" Then
                    path = InfodocService.Infodoc_Path(CurrentProject.ID, CurrentProject.ProjectManager.ActiveHierarchy, _ObjectType, ParentNodeID.ToString(), tempnode.NodeID)
                ElseIf node_type = "4" Then
                    path = InfodocService.Infodoc_Path(CurrentProject.ID, CurrentProject.ProjectManager.ActiveHierarchy, _ObjectType, tempnode.MeasurementScale.GuidID.ToString(), ParentNodeID)
                Else
                    path = InfodocService.Infodoc_Path(CurrentProject.ID, CurrentProject.ProjectManager.ActiveHierarchy, _ObjectType, tempnode.NodeID.ToString(), ParentNodeID)
                End If

            Catch ex As Exception

            End Try
        Else

            If node_type = "quick-help" Then
                Dim ObjID As String = InfodocService.GetQuickHelpObjectID(qh_help_id, ParentNode)
                path = InfodocService.Infodoc_Path(CurrentProject.ID, CurrentProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.QuickHelp, ObjID, -1)
            Else

                If is_CovObj Then
                    tempnode = CType(nodes(4), clsNode)
                End If
            End If
        End If

        Dim test = tempnode.NodeID
        Dim kur = _ObjectType
        Dim l = ParentNodeID
        Dim data = New Object(6, 6) {}

        If is_CovObj AndAlso node_type = "3" Then
            data(0, 1) = ParentNode
            data(0, 3) = tempnode.NodeID
        ElseIf is_CovObj AndAlso node_type = "0" Then
            data(0, 1) = tempnode
            data(0, 3) = tempnode.NodeID
        Else
            data(0, 1) = tempnode
            data(0, 3) = ParentNodeID
        End If

        data(0, 0) = path
        data(0, 2) = _ObjectType
        data(0, 4) = is_Multi
        data(0, 5) = is_CovObj
        data(0, 6) = tAction.ActionType
        Return data
    End Function
    Public Shared Function getInfodoc_sizes(ByVal isPm As Boolean) As String()()
        Dim App = Anytime.AppSession()
        Dim NodeSizes As String()() = If(HttpContext.Current.Session(SessionNodeSizes) Is Nothing, New String(2)() {}, CType(HttpContext.Current.Session(SessionNodeSizes), String()()))
        Dim getSizesFromProject = (isPm OrElse HttpContext.Current.Session(SessionNodeSizes) Is Nothing)

        If getSizesFromProject Then

            Try
                Dim Sizes = App.ActiveProject.PipeParameters.InfoDocSize
                Dim NodeSections = Sizes.Split(";"c)
                NodeSizes(0) = New String(1) {}
                NodeSizes(1) = New String(1) {}
                NodeSizes(2) = New String(1) {}

                If Sizes <> "" Then

                    For Each NodeSection As String In NodeSections
                        Dim nodenames = NodeSection.Split("="c)
                        Dim nodeElements = nodenames(1).Split(":"c)

                        If nodeElements(0) Is Nothing Then
                            nodeElements(0) = "60%"
                        End If

                        If nodeElements(1) Is Nothing Then
                            nodeElements(1) = "150"
                        End If

                        Select Case nodenames(0)
                            Case "pw_node"
                                NodeSizes(1)(0) = nodeElements(0)
                                NodeSizes(1)(1) = nodeElements(1)
                            Case "pw_wrt"
                                NodeSizes(2)(0) = nodeElements(0)
                                NodeSizes(2)(1) = nodeElements(1)
                            Case "pw_goal"
                                NodeSizes(0)(0) = nodeElements(0)
                                NodeSizes(0)(1) = nodeElements(1)
                        End Select
                    Next
                End If

            Catch
                NodeSizes = Nothing
            End Try

            HttpContext.Current.Session(SessionNodeSizes) = NodeSizes
        End If

        Return NodeSizes
    End Function

    Public Shared Sub setInfodoc_sizes(ByVal width As String, ByVal height As String, ByVal index As Integer, ByVal is_multi As Boolean, ByVal isPm As Boolean)
        Dim App = Anytime.AppSession()
        Dim NodeSizes As String()() = If(HttpContext.Current.Session(SessionNodeSizes) Is Nothing, New String(2)() {}, CType(HttpContext.Current.Session(SessionNodeSizes), String()()))
        NodeSizes(index)(0) = width
        NodeSizes(index)(1) = height

        If isPm Then
            Dim tempstring = ""

            Try
                Dim Sizes = App.ActiveProject.PipeParameters.InfoDocSize
                Dim NodeSections = Sizes.Split(";"c)

                Select Case index
                    Case 0
                        Sizes += (If(Sizes = "", "", ";")) & "pw_goal=" & NodeSizes(index)(0) & ":" & NodeSizes(index)(1)
                    Case 1
                        Sizes += (If(Sizes = "", "", ";")) & "pw_node=" & NodeSizes(index)(0) & ":" & NodeSizes(index)(1)
                    Case 2
                        Sizes += (If(Sizes = "", "", ";")) & "pw_wrt=" & NodeSizes(index)(0) & ":" & NodeSizes(index)(1)
                End Select

                NodeSections = Sizes.Split(";"c)
                Dim pair = ""
                Dim tempNodeNames = New List(Of String)()

                For Each NodeSection As String In NodeSections
                    Dim NodeSectionTemp = ""

                    If NodeSection <> "" Then
                        Dim nodenames = NodeSection.Split("="c)

                        If Not tempNodeNames.Contains(nodenames(0)) Then

                            Select Case nodenames(0)
                                Case "pw_node"

                                    If index = 1 Then
                                        pair = "pw_goal"
                                        NodeSectionTemp = nodenames(0) & "=" & NodeSizes(index)(0) & ":" & NodeSizes(index)(1)
                                        Exit Select
                                    End If

                                    NodeSectionTemp = NodeSection
                                Case "pw_wrt"

                                    If index = 2 Then
                                        pair = ""
                                        NodeSectionTemp = nodenames(0) & "=" & NodeSizes(index)(0) & ":" & NodeSizes(index)(1)
                                        Exit Select
                                    End If

                                    NodeSectionTemp = NodeSection
                                Case "pw_goal"

                                    If index = 0 Then
                                        pair = "pw_node"
                                        NodeSectionTemp = nodenames(0) & "=" & NodeSizes(index)(0) & ":" & NodeSizes(index)(1)
                                        Exit Select
                                    End If

                                    NodeSectionTemp = NodeSection
                            End Select

                            If is_multi AndAlso pair <> "" Then
                                tempNodeNames.Add(pair)
                                NodeSectionTemp += ";" & pair & "=" & NodeSizes(index)(0) & ":" & NodeSizes(index)(1)
                            End If

                            tempstring += If(tempstring = "", NodeSectionTemp, ";" & NodeSectionTemp)
                            tempNodeNames.Add(nodenames(0))
                        End If
                    End If
                Next

            Catch e As Exception
                tempstring = "pw_goal=60%;150px;pw_wrt=60%;150px;pw_goal=60%;150px"
            End Try

            App.ActiveProject.PipeParameters.InfoDocSize = tempstring
            App.ActiveProject.SaveProjectOptions("Save infodoc frame settings")
        End If

        HttpContext.Current.Session(SessionNodeSizes) = NodeSizes
    End Sub
End Class