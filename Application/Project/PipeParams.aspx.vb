
Partial Class PipeParamsPage
    Inherits clsComparionCorePage

    Public HasWelcomeSurvey As Boolean = False  ' D3804
    Public HasThankYouSurvey As Boolean = False  ' D3804

    Public _OPT_CUSTOM_COMBINED As Boolean = True    ' D4376 + D4378
    Public _OPT_SHOW_FEEDBACK_ON As Boolean = False

    Public Sub New()
        MyBase.New(_PGID_PROJECT_OPTION_EVALUATE)
    End Sub

    ' D4394 ===
    Public Function ResString_(sResName As String) As String
        Dim sRes As String = App.ResString(sResName)
        If sRes.Contains("%%") Then
            'Dim Tpls As Dictionary(Of String, String) = (From x In PreParsedTemplates Select x).ToDictionary(Function(x) x.Key, Function(x) x.Value)
            Dim sObjsTpls As String() = {_TPL_COMPARION_OBJS, _TPL_RISK_CONSEQUENCES, _TPL_RISK_CONTROLS}
            Dim sAltsTpls As String() = {_TPL_COMPARION_ALTS, _TPL_RISK_EVENTS}
            For Each sName As String In sObjsTpls
                Dim tIdx As Integer = sRes.ToLower.IndexOf(sName.ToLower)
                Dim sTpl As String = sName
                If tIdx >= 0 Then sTpl = sRes.Substring(tIdx, sName.Length)
                sRes = ParseTemplate(sRes, sName, String.Format("<span class='wording_obj'>{0}</span>", sTpl))
            Next
            For Each sName As String In sAltsTpls
                sRes = ParseTemplate(sRes, sName, String.Format("<span class='wording_alt'>{0}</span>", sName))
            Next
            sRes = ParseAllTemplates(sRes, App.ActiveUser, App.ActiveProject)
            'PreParsedTemplates = Tpls
        End If

        Return sRes
    End Function
    ' D4394 ==

    ' D6098 ===
    Public Function isReadOnly() As Boolean
        Return App.ActiveProject.ProjectStatus = ecProjectStatus.psArchived OrElse Not App.CanUserModifyProject(App.ActiveUser.UserID, App.ProjectID, App.ActiveUserWorkgroup, App.ActiveWorkspace, App.ActiveWorkgroup)
    End Function
    ' D6098 ==

    ' D7040 ===
    Public Function HasSurveysLicense() As Boolean
        Return (App.isSpyronAvailable AndAlso App.ActiveWorkgroup.License IsNot Nothing AndAlso App.ActiveWorkgroup.License.isValidLicense AndAlso App.ActiveWorkgroup.License.CheckParameterByID(ecLicenseParameter.SpyronEnabled))
    End Function
    ' D7040 ==

    ' D3804 ===
    Public Function CanEditSurveys() As Boolean
        Return HasSurveysLicense() AndAlso
               (App.ActiveProject.ProjectStatus = ecProjectStatus.psActive OrElse App.ActiveProject.ProjectStatus = ecProjectStatus.psMasterProject) AndAlso
                App.CanUserModifyProject(App.ActiveUser.UserID, App.ProjectID, App.ActiveUserWorkgroup, App.ActiveWorkspace)    ' D7040
    End Function
    ' D3804 ==

    ' D4714 ===
    Public Function CanShowEmbedded() As Boolean
        Return App.isRiskEnabled AndAlso App.ActiveProject.isImpact
    End Function
    ' D4714 ==

    Public Function GetDispWhatVals() As String
        Dim retVal As String = ""
        Dim PP = PM.PipeParameters
        retVal = String.Format("{0},{1},{2},{3},{4},{5},", CInt(PP.LocalResultsView), CInt(PP.LocalResultsSortMode), Bool2JS(PP.ShowExpectedValueLocal), CInt(PP.GlobalResultsView), CInt(PP.GlobalResultsSortMode), Bool2JS(PP.ShowExpectedValueGlobal))
        retVal += String.Format("{0},{1},{2},{3},{4},", Bool2JS(PP.ShowWelcomeScreen), Bool2JS(PP.ShowThankYouScreen), Bool2JS(PP.ShowConsistencyRatio), Bool2JS(PP.ShowInfoDocs), Bool2JS(PP.ShowInfoDocsMode = 0))
        retVal += String.Format("{0},{1},{2},{3},", CInt(PP.ShowFullObjectivePath), Bool2JS(PP.ShowComments), Bool2JS(PP.UseCISForIndividuals), Bool2JS(PP.DisableWarningsOnStepChange))   ' D4100
        retVal += String.Format("{0},{1},{2},{3},{4},", Bool2JS(PP.ShowSensitivityAnalysis), Bool2JS(PP.ShowSensitivityAnalysisGradient), Bool2JS(PP.ShowSensitivityAnalysisPerformance), Bool2JS(PP.CalculateSAForCombined), CInt(PP.SynthesisMode))
        retVal += String.Format("{0},{1},", Bool2JS(PP.ShowWelcomeSurvey), Bool2JS(PP.ShowThankYouSurvey))
        retVal += String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},", Bool2JS(PM.Parameters.ResultsLocalShowIndex), Bool2JS(PM.Parameters.ResultsGlobalShowIndex), Bool2JS(PM.Parameters.EvalCollapseMultiPWBars), Bool2JS(PM.Parameters.EvalShowRewardThankYou), Bool2Num(PM.Parameters.EvalWelcomeSurveyFirst), Bool2Num(PM.Parameters.EvalThankYouSurveyLast), Bool2JS(PM.Parameters.EvalHideInfodocCaptions), Bool2JS(HasEmbeddedContent(PM.Parameters.EvalEmbeddedContent, EmbeddedContentType.RiskResults)), Bool2JS(PM.Parameters.AutoFitInfoDocImages), Bool2JS(HasEmbeddedContent(PM.Parameters.EvalEmbeddedContent, EmbeddedContentType.HeatMap)), CInt(PM.Parameters.EvalTextToSpeech)) 'A1154 + D3953 + D3964 + D3977 + D4713 + D4714 + D6523 + D6664 + D6669 + D6962
        retVal += String.Format("{0},{1},{2},{3},", Bool2JS(Not PM.Parameters.EvalHideLocalNormalizationOptions), Bool2JS(Not PM.Parameters.EvalHideGlobalNormalizationOptions), Bool2JS(PM.Parameters.ResultsLocalShowBars), Bool2JS(PM.Parameters.ResultsGlobalShowBars))    ' D7556 + D7561 + D7580 // last is [36]
        retVal = String.Format("[{0}]", retVal)
        Return retVal
    End Function


    Public Function GetEvalHowVals() As String
        Dim retVal As String = ""
        Dim PP = PM.PipeParameters
        retVal = String.Format("{0},{1},{2},{3},{4},", Bool2JS(Not PP.ShowProgressIndicator), Bool2JS(Not PP.AllowNavigation), Bool2JS(PP.ShowNextUnassessed), If(PP.AllowAutoadvance, 1, If(App.ActiveProject.ProjectManager.Parameters.EvalNoAskAutoAdvance, -1, 0)), Bool2JS(Not PP.AllowMissingJudgments))    ' D3963 + D4165
        retVal += String.Format("{0},'{1}',{2},{3},{4},{5},'{6}',{7}", Bool2JS(PP.RedirectAtTheEnd), JS_SafeString(PP.TerminalRedirectURL), Bool2JS(PP.LogOffAtTheEnd), Bool2JS(PM.Parameters.EvalJoinRiskionPipes), Bool2JS(PM.Parameters.EvalCloseWindowAtFinish), Bool2Num(PM.Parameters.EvalOpenNextProjectAtFinish), JS_SafeString(PM.Parameters.EvalNextProjectPasscodeAtFinish.ToLower), Bool2JS(PM.CalculationsManager.PrioritiesCacheManager.Enabled))    ' D4103 + D4160 + D4162
        retVal = String.Format("[{0}]", retVal)
        Return retVal
        'PP.AllowSwitchNodesInSA
        'PP.AlternativesColumnNameInSA
        'PP.AltsDefaultContribution
        'PP.AltsDefaultContributionImpact
        'PP.AssociatedModelGuidID
        'PP.AssociatedModelIntID
        'PP.CalculateSAWRTGoal
        'PP.CombinedMode
        'PP.DefaultGroupID
        'PP.DefaultNonCoveringObjectiveMeasurementType
        'PP.EndDate
        'PP.EvaluateDiagonalsAdvanced
        'PP.ForceDefaultParameters
        'PP.IdealViewType
        'PP.IncludeIdealAlternative
        'PP.MeasureMode
        'PP.ObjectivesColumnNameInSA
        'PP.PairwiseMatrixEvaluationOrder
        'PP.ShowIdealAlternative
        'PP.ShowSurvey
        'PP.SortSensitivityAnalysis
        'PP.StartDate
        'PP.SynchShowGroupResults
        'PP.SynchStartInPollingMode
        'PP.SynchStartVotingOnceFacilitatorAllows
        'PP.SynchUseOptionsFromDecision
        'PP.SynchUseVotingBoxes
        'PP.UseWeights
        'PP.WRTInfoDocsShowMode
    End Function

    Public Function GetEvalWhatVals() As String
        Dim retVal As String = ""
        Dim PP = PM.PipeParameters
        ' D6630 ===
        If String.IsNullOrEmpty(PP.JudgementPromt) Then
            PP.JudgementPromt = ResString(getWordingResName(False) + "0")
        End If
        If String.IsNullOrEmpty(PP.JudgementAltsPromt) Then
            PP.JudgementAltsPromt = ResString(getWordingResName(True) + "0")
        End If
        ' D6630 ==
        retVal = String.Format("{0},{1},{2},{3},{4},{5},", Bool2JS(PP.EvaluateObjectives), Bool2JS(PP.EvaluateAlternatives), CInt(PP.DefaultNonCoveringObjectiveMeasurementType), CInt(PP.DefaultCoveringObjectiveMeasurementType), CInt(PP.ObjectivesEvalDirection), Bool2JS(PP.FeedbackOn))
        retVal += String.Format("{0},{1},{2},", Bool2JS(PP.ObjectivesPairwiseOneAtATime), Bool2JS(PP.AlternativesPairwiseOneAtATime), CInt(PP.AlternativesEvalMode))
        retVal += String.Format("{0},{1},{2},{3},{4},{5},", CInt(PP.EvaluateDiagonals), CInt(PP.EvaluateDiagonalsAlternatives), Bool2JS(PP.ForceAllDiagonals), Bool2JS(PP.ForceAllDiagonalsForAlternatives), PP.ForceAllDiagonalsLimit, PP.ForceAllDiagonalsLimitForAlternatives)
        retVal += String.Format("{0},{1},{2},{3},{4},{5},", Bool2JS(PP.ForceGraphical), Bool2JS(PP.ForceGraphicalForAlternatives), CInt(PP.GraphicalPWMode), CInt(PP.GraphicalPWModeForAlternatives), CInt(PP.PairwiseType), CInt(PP.PairwiseTypeForAlternatives))
        retVal += String.Format("{0},'{1}','{2}','{3}','{4}',{5},", CInt(PP.ModelEvalOrder), JS_SafeString(PP.NameObjectives), JS_SafeString(PP.NameAlternatives), JS_SafeString(PP.JudgementPromt), JS_SafeString(PP.JudgementAltsPromt), Bool2JS(PM.IsRiskProject))
        retVal += String.Format("{0},{1},{2}", Bool2JS(PM.Parameters.EvalTryParseValuesFromNames), Bool2JS(PM.Parameters.NodeIndexIsVisible), CInt(PM.Parameters.NodeVisibleIndexMode))    ' D4323 + A1342
        retVal = String.Format("[{0}]", retVal)
        Return retVal
    End Function

    ' D6630 ===
    Private Function getWordingResName(isAlts As Boolean) As String
        Dim sResName As String = ""
        If App.isRiskEnabled Then
            If App.ActiveProject.isImpact Then
                sResName = If(isAlts, "lbl_promt_alt_impact_", "lbl_promt_obj_impact_")
            Else
                sResName = If(isAlts, "lbl_promt_alt_likelihood_", "lbl_promt_obj_likelihood_")
            End If
        Else
            sResName = If(isAlts, "lbl_promt_alt_", "lbl_promt_obj_")
        End If
        Return sResName
    End Function
    ' D6630 ==

    ' D4166 ===
    Public Function GetWordingOptions(isAlts As Boolean) As String
        Dim sList As String = ""

        Dim sResName As String = getWordingResName(isAlts)  ' D6630

        For i As Integer = 0 To 10
            Dim sName As String = sResName + i.ToString
            Dim sVal As String = ResString(sName, True)
            If sVal <> sName Then
                sList += String.Format("<option value='{0}'>{0}</option>" + vbNewLine, SafeFormString(sVal))
            End If
        Next
        sList += String.Format("<option value=''>{0}</option>", SafeFormString(ResString("lbl_promt_custom")))
        Return sList
    End Function
    ' D4166 ==

    'A1315 ===
    Private Function CheckAllMTForAltsDirectOrRatings() As Boolean
        'Dim retVal As Boolean = True
        'Dim AltH As clsHierarchy = PM.AltsHierarchy(PM.ActiveAltsHierarchy)
        'If H.Nodes.Count > 1 Then
        '    Dim nodes As New List(Of clsNode)
        '    For Each node As ECCore.clsNode In H.Nodes
        '        If node.IsTerminalNode Then
        '            For Each p As clsNode In node.ParentNodes 
        '                If Not nodes.Contains(p) Then nodes.Add(p)
        '            Next
        '        End If
        '    Next

        '    For each node As clsNode In nodes
        '        If retVal Then
        '            Dim allRatings As Integer = 0
        '            Dim allDirect As Integer = 0
        '            Dim anyRating As Boolean = False 
        '            Dim anyDirect As Boolean = False
        '            For Each child As clsNode In node.Children
        '                If child.MeasureType = ECMeasureType.mtRatings Then
        '                    anyRating = True
        '                    allRatings += 1
        '                End If
        '                If child.MeasureType = ECMeasureType.mtDirect Then
        '                    anyDirect = True
        '                    allDirect += 1
        '                End If
        '            Next

        '            If (anyRating AndAlso allRatings <> node.Children.Count) OrElse (anyDirect AndAlso allDirect <> node.Children.Count) Then
        '                retVal = False
        '            End If
        '        End If
        '    Next
        'End If

        Dim H As clsHierarchy = PM.Hierarchy(PM.ActiveHierarchy)
        For Each covObj As clsNode In H.TerminalNodes
            If covObj.MeasureType <> ECMeasureType.mtDirect AndAlso covObj.MeasureType <> ECMeasureType.mtRatings Then
                Return False
            End If
        Next

        Return True
    End Function
    'A1315 ==

    ' D4376 ===
    Public Function GetCombinedName(ShowWithAction As Boolean) As String
        Dim sDefName As String = ResString("lblEvaluationLegendCombined")
        Dim sName As String = sDefName
        If _OPT_CUSTOM_COMBINED Then
            With App.ActiveProject.ProjectManager.Parameters
                Dim fIsCustom As Boolean = .ResultsCustomCombinedUID <> COMBINED_USER_ID OrElse .ResultsCustomCombinedName <> ""    ' D4384
                If fIsCustom Then
                    sName = String.Format("<span style='color:#009999'><b>{0}</b></span>", SafeFormString(If(.ResultsCustomCombinedName = "", sDefName, .ResultsCustomCombinedName)))    ' D4384 + A1458
                    If .ResultsCustomCombinedUID = COMBINED_USER_ID Then sName = String.Format("{0} (""{1}"")", sDefName, sName) ' D4384
                    Dim tAHPUser As clsUser = PM.GetUserByID(.ResultsCustomCombinedUID)
                    If tAHPUser IsNot Nothing Then
                        If sName = "" Then sName = If(tAHPUser.UserName <> "", tAHPUser.UserName, tAHPUser.UserEMail) ' D4382
                        If tAHPUser.UserEMail.ToLower <> sName.ToLower Then sName = String.Format("{0} ({1})", sName, tAHPUser.UserEMail)
                    End If
                End If
            End With
            If ShowWithAction AndAlso Not isReadOnly() Then sName = String.Format("<span class='custom_combined_name'>{0}</span></label>&nbsp;<a href='' onclick='editCustomCombined(); return false;'><i class='fa fa-user-circle' style='margin:0px 0px -4px 0px;'></i></a><label>", sName, ImagePath, SafeFormString(ResString("lblParamsSelectCombined")))   ' D6098
        End If
        Return sName
    End Function

    Private Sub CheckCombinedUser()
        PM.CheckCustomCombined()    ' D4382
        'With PM.Parameters
        '    Dim tAHPUser As clsUser = PM.GetUserByID(.ResultsCustomCombinedUID)
        '    If tAHPUser Is Nothing Then
        '        .ResultsCustomCombinedUID = COMBINED_USER_ID
        '        .ResultsCustomCombinedName = ""
        '    Else
        '        If .ResultsCustomCombinedName = "" Then .ResultsCustomCombinedName = If(tAHPUser.UserName <> "", tAHPUser.UserName, tAHPUser.UserEMail)
        '    End If
        'End With
    End Sub
    ' D4376 ==

    Protected Sub Ajax_Callback()
        Dim sAction As String = CheckVar(_PARAM_ACTION, "").Trim()
        Dim sResult As String = String.Format("['{0}']", CStr(If(String.IsNullOrEmpty(sAction), "", sAction))) 'A1315
        Dim sParam As String = ""
        Dim PP = PM.PipeParameters
        Dim fSaveProject = True ' D4162
        Select Case sAction
            Case "EvaluateObjectives"
                Dim boolResult = CheckVar("checked", PP.EvaluateObjectives)
                PP.EvaluateObjectives = boolResult
                sParam = boolResult.ToString
            Case "EvaluateAlternatives"
                Dim boolResult = CheckVar("checked", PP.EvaluateAlternatives)
                PP.EvaluateAlternatives = boolResult
                sParam = boolResult.ToString
            Case "ObjectivesEvalDirection"
                Dim intResult = CheckVar("val", PP.ObjectivesEvalDirection)
                PP.ObjectivesEvalDirection = CType(intResult, ObjectivesEvaluationDirection)
                sParam = intResult.ToString
            Case "DefaultNonCoveringObjectiveMeasurementType"
                Dim intResult = CheckVar("val", PP.DefaultNonCoveringObjectiveMeasurementType)
                PP.DefaultNonCoveringObjectiveMeasurementType = CType(intResult, ECMeasureType)
                PM.ActiveObjectives.DefaultMeasurementTypeForNonCoveringObjectives = PP.DefaultNonCoveringObjectiveMeasurementType
                sParam = intResult.ToString
            Case "DefaultCoveringObjectiveMeasurementType"
                Dim intResult = CheckVar("val", PP.DefaultCoveringObjectiveMeasurementType)
                PP.DefaultCoveringObjectiveMeasurementType = CType(intResult, ECMeasureType)
                PM.ActiveObjectives.DefaultMeasurementTypeForCoveringObjectives = PP.DefaultCoveringObjectiveMeasurementType
                sParam = intResult.ToString
            Case "FeedbackOn"
                Dim boolResult = CheckVar("checked", PP.FeedbackOn)
                PP.FeedbackOn = boolResult
                sParam = boolResult.ToString
            Case "ObjectivesPairwiseOneAtATime"
                Dim intResult = CheckVar("val", PP.ObjectivesPairwiseOneAtATime)
                PP.ObjectivesPairwiseOneAtATime = intResult
                sParam = intResult.ToString
            Case "AlternativesPairwiseOneAtATime"
                Dim intResult = CheckVar("val", PP.AlternativesPairwiseOneAtATime)
                PP.AlternativesPairwiseOneAtATime = intResult
                sParam = intResult.ToString
            Case "AlternativesEvalMode"
                Dim intResult = CheckVar("val", PP.AlternativesEvalMode)
                'A1315 ===
                Dim msg As String = ""
                If intResult = 2 AndAlso Not CheckAllMTForAltsDirectOrRatings() Then
                    intResult = 0
                    msg = String.Format(ResString_("msgPipeParameterNotAllowedForThisCluster"), ResString_("optEvalRSOneAltToObjs"), ResString_("optEvalRSOneAltAlts"))
                End If
                'A1315 ==            
                PP.AlternativesEvalMode = CType(intResult, AlternativesEvaluationMode)
                sParam = intResult.ToString
                sResult = String.Format("['{0}',{1},'{2}']", sAction, intResult, JS_SafeString(msg))
            Case "EvaluateDiagonals"
                Dim intResult = CheckVar("val", PP.EvaluateDiagonals)
                PP.EvaluateDiagonals = CType(intResult, DiagonalsEvaluation)
                sParam = intResult.ToString
            Case "EvaluateDiagonalsAlternatives"
                Dim intResult = CheckVar("val", PP.EvaluateDiagonalsAlternatives)
                PP.EvaluateDiagonalsAlternatives = CType(intResult, DiagonalsEvaluation)
                sParam = intResult.ToString
            Case "ForceAllDiagonals"
                Dim boolResult = CheckVar("checked", PP.ForceAllDiagonals)
                PP.ForceAllDiagonals = boolResult
                sParam = boolResult.ToString
            Case "ForceAllDiagonalsForAlternatives"
                Dim boolResult = CheckVar("checked", PP.ForceAllDiagonalsForAlternatives)
                PP.ForceAllDiagonalsForAlternatives = boolResult
                sParam = boolResult.ToString
            Case "ForceAllDiagonalsLimit"
                Dim intResult = CheckVar("val", PP.ForceAllDiagonalsLimit)
                PP.ForceAllDiagonalsLimit = intResult
                sParam = intResult.ToString
            Case "ForceAllDiagonalsLimitForAlternatives"
                Dim intResult = CheckVar("val", PP.ForceAllDiagonalsLimitForAlternatives)
                PP.ForceAllDiagonalsLimitForAlternatives = intResult
                sParam = intResult.ToString
            Case "ForceGraphical"
                Dim boolResult = CheckVar("checked", PP.ForceGraphical)
                PP.ForceGraphical = boolResult
                sParam = boolResult.ToString
            Case "ForceGraphicalForAlternatives"
                Dim boolResult = CheckVar("checked", PP.ForceGraphicalForAlternatives)
                PP.ForceGraphicalForAlternatives = boolResult
                sParam = boolResult.ToString
            Case "GraphicalPWMode"
                Dim intResult = CheckVar("val", PP.GraphicalPWMode)
                PP.GraphicalPWMode = CType(intResult, GraphicalPairwiseMode)
                sParam = intResult.ToString
            Case "GraphicalPWModeForAlternatives"
                Dim intResult = CheckVar("val", PP.GraphicalPWModeForAlternatives)
                PP.GraphicalPWModeForAlternatives = CType(intResult, GraphicalPairwiseMode)
                sParam = intResult.ToString
            Case "PairwiseType"
                Dim intResult = CheckVar("val", PP.PairwiseType)
                PP.PairwiseType = CType(intResult, PairwiseType)
                sParam = intResult.ToString
            Case "PairwiseTypeForAlternatives"
                Dim intResult = CheckVar("val", PP.PairwiseTypeForAlternatives)
                PP.PairwiseTypeForAlternatives = CType(intResult, PairwiseType)
                sParam = intResult.ToString
            Case "ModelEvalOrder"
                Dim intResult = CheckVar("val", PP.ModelEvalOrder)
                PP.ModelEvalOrder = CType(intResult, ModelEvaluationOrder)
                sParam = intResult.ToString
            Case "NameObjectives"
                sParam = CheckVar("val", PP.NameObjectives)
                PP.NameObjectives = sParam
            Case "NameAlternatives"
                sParam = CheckVar("val", PP.NameAlternatives)
                PP.NameAlternatives = sParam
            Case "JudgementPromt"
                sParam = CheckVar("val", PP.JudgementPromt)
                PP.JudgementPromt = sParam
            Case "JudgementAltsPromt"
                sParam = CheckVar("val", PP.JudgementAltsPromt)
                PP.JudgementAltsPromt = sParam
            Case "EvalTryParseValuesFromNames" 'A1340
                sParam = CheckVar("checked", PM.Parameters.EvalTryParseValuesFromNames).ToString 'A1340
                PM.Parameters.EvalTryParseValuesFromNames = CBool(sParam) 'A1340
                ' D4323 ===
            Case "ShowEventID"
                sParam = CheckVar("checked", PM.Parameters.NodeIndexIsVisible).ToString 'A1342
                PM.Parameters.NodeIndexIsVisible = CBool(sParam) 'A1342
            Case "IDColumnMode"
                sParam = CheckVar("val", PM.Parameters.NodeVisibleIndexMode).ToString
                PM.Parameters.NodeVisibleIndexMode = CType(sParam, IDColumnModes)
                sParam = PM.Parameters.NodeVisibleIndexMode.ToString
                ' D4323 ==

            Case "ShowProgressIndicator"
                Dim boolResult = CheckVar("checked", PP.ShowProgressIndicator)
                PP.ShowProgressIndicator = Not boolResult   ' D4165
                sParam = boolResult.ToString
            Case "AllowNavigation"
                Dim boolResult = CheckVar("checked", PP.AllowNavigation)
                PP.AllowNavigation = Not boolResult ' D4165
                sParam = boolResult.ToString
            Case "ShowNextUnassessed"
                Dim boolResult = CheckVar("checked", PP.ShowNextUnassessed)
                PP.ShowNextUnassessed = boolResult
                sParam = boolResult.ToString
            Case "AllowAutoadvance"
                'Dim boolResult = CheckVar("checked", PP.AllowAutoadvance)
                'PP.AllowAutoadvance = boolResult
                'sParam = boolResult.ToString
                ' D3963 ===
                With App.ActiveProject.ProjectManager.Parameters
                    Dim tResult = CheckVar("val", If(PP.AllowAutoadvance, 1, If(.EvalNoAskAutoAdvance, -1, 0)))
                    PP.AllowAutoadvance = tResult > 0
                    If .EvalNoAskAutoAdvance <> (tResult < 0) Then
                        .EvalNoAskAutoAdvance = (tResult < 0)
                        .Save()
                    End If
                    sParam = tResult.ToString
                End With
                ' D3963 ==
            Case "AllowMissingJudgments"
                Dim boolResult = CheckVar("checked", PP.AllowMissingJudgments)
                PP.AllowMissingJudgments = Not boolResult   ' D4165
                sParam = boolResult.ToString
                'Case "RedirectAtTheEnd"
                '    Dim boolResult = CheckVar("checked", PP.RedirectAtTheEnd)
                '    PP.RedirectAtTheEnd = boolResult
                '    sParam = boolResult.ToString
            Case "TerminalRedirectURL"
                sParam = CheckVar("val", PP.TerminalRedirectURL)
                PP.TerminalRedirectURL = sParam
            Case "LogOffAtTheEnd"
                Dim boolResult = CheckVar("checked", PP.LogOffAtTheEnd)
                PP.LogOffAtTheEnd = boolResult
                sParam = boolResult.ToString
                ' D4103 ===
                'Case "JoinPipes"
                '    Dim boolResult = CheckVar("checked", PM.Parameters.EvalJoinRiskionPipes)
                '    PM.Parameters.EvalJoinRiskionPipes = boolResult
                '    sParam = boolResult.ToString
                ' D4103 ==
                ' D4160 ===
                'Case "CloseWinAtEnd"
                '    Dim boolResult = CheckVar("checked", PM.Parameters.EvalCloseWindowAtFinish)    ' D4162
                '    PM.Parameters.EvalCloseWindowAtFinish = boolResult  ' D4162
                '    sParam = boolResult.ToString
                ' D4160 ==
                ' D4162 ===
            Case "ActionAtFinish"
                Dim tVal As Integer = CheckVar("val", -1)   ' D4165
                If tVal >= 0 Then
                    PM.Parameters.EvalCloseWindowAtFinish = tVal = 1
                    PP.RedirectAtTheEnd = tVal = 2
                    PM.Parameters.EvalOpenNextProjectAtFinish = tVal = 3
                    ' D4164 ===
                    If App.isRiskEnabled Then
                        PM.Parameters.EvalJoinRiskionPipes = tVal = 4
                    End If
                    ' D4164 ==
                End If
                ' D4162 ==
                ' D4706 ===
            Case "UseCaching"
                Dim boolResult = CheckVar("checked", PM.CalculationsManager.PrioritiesCacheManager.Enabled)
                PM.CalculationsManager.PrioritiesCacheManager.Enabled = boolResult
                sParam = boolResult.ToString
                sResult = String.Format("['UseCaching', {0}]", Bool2Num(PM.CalculationsManager.PrioritiesCacheManager.Enabled))    ' D4707
                ' D4706 ==

            Case "LocalResultsView"
                Dim intResult = CheckVar("val", PP.LocalResultsView)
                PP.LocalResultsView = CType(intResult, ResultsView)
                sParam = intResult.ToString
            Case "LocalResultsSortMode"
                Dim intResult = CheckVar("val", PP.LocalResultsSortMode)
                PP.LocalResultsSortMode = CType(intResult, ResultsSortMode)
                sParam = intResult.ToString
            Case "ShowExpectedValueLocal"
                Dim boolResult = CheckVar("checked", PP.ShowExpectedValueLocal)
                PP.ShowExpectedValueLocal = boolResult
                sParam = boolResult.ToString
                'A1154 ===
            Case "show_index"
                Dim tOriginalValue As Boolean = PM.Parameters.ResultsLocalShowIndex
                Dim boolResult = CheckVar("checked", tOriginalValue)
                If boolResult <> tOriginalValue Then
                    PM.Parameters.ResultsLocalShowIndex = boolResult
                End If
                sParam = boolResult.ToString
            Case "show_index_global"
                Dim tOriginalValue As Boolean = PM.Parameters.ResultsGlobalShowIndex
                Dim boolResult = CheckVar("checked", tOriginalValue)
                If boolResult <> tOriginalValue Then
                    PM.Parameters.ResultsGlobalShowIndex = boolResult
                End If
                sParam = boolResult.ToString
                'A1154 ==
                ' D7561 ===
            Case "show_local_bars"
                Dim tOriginalValue As Boolean = PM.Parameters.ResultsLocalShowBars
                Dim boolResult = CheckVar("checked", tOriginalValue)
                If boolResult <> tOriginalValue Then
                    PM.Parameters.ResultsLocalShowBars = boolResult
                End If
                sParam = boolResult.ToString
            Case "show_global_bars"
                Dim tOriginalValue As Boolean = PM.Parameters.ResultsGlobalShowBars
                Dim boolResult = CheckVar("checked", tOriginalValue)
                If boolResult <> tOriginalValue Then
                    PM.Parameters.ResultsGlobalShowBars = boolResult
                End If
                sParam = boolResult.ToString
                ' D7561 ==
                ' D7556 ===
            Case "HideLocalNormalization"
                Dim tOriginalValue As Boolean = PM.Parameters.EvalHideLocalNormalizationOptions
                Dim boolResult = Not CheckVar("checked", tOriginalValue)    ' D7580
                If boolResult <> tOriginalValue Then
                    PM.Parameters.EvalHideLocalNormalizationOptions = boolResult
                End If
                sParam = boolResult.ToString
            Case "HideGlobalNormalization"
                Dim tOriginalValue As Boolean = PM.Parameters.EvalHideGlobalNormalizationOptions
                Dim boolResult = Not CheckVar("checked", tOriginalValue)    ' D7580
                If boolResult <> tOriginalValue Then
                    PM.Parameters.EvalHideGlobalNormalizationOptions = boolResult
                End If
                sParam = boolResult.ToString
            ' D7556 ==
            Case "GlobalResultsView"
                Dim intResult = CheckVar("val", PP.GlobalResultsView)
                PP.GlobalResultsView = CType(intResult, ResultsView)
                sParam = intResult.ToString
            Case "GlobalResultsSortMode"
                Dim intResult = CheckVar("val", PP.GlobalResultsSortMode)
                PP.GlobalResultsSortMode = CType(intResult, ResultsSortMode)
                sParam = intResult.ToString
            Case "ShowExpectedValueGlobal"
                Dim boolResult = CheckVar("checked", PP.ShowExpectedValueGlobal)
                PP.ShowExpectedValueGlobal = boolResult
                sParam = boolResult.ToString
            Case "ShowWelcomeScreen"
                Dim boolResult = CheckVar("checked", PP.ShowWelcomeScreen)
                PP.ShowWelcomeScreen = boolResult
                sParam = boolResult.ToString
            Case "ShowThankYouScreen"
                Dim boolResult = CheckVar("checked", PP.ShowThankYouScreen)
                PP.ShowThankYouScreen = boolResult
                sParam = boolResult.ToString
                ' D3964 ===
            Case "ShowRewardThankYouScreen"
                Dim boolResult = CheckVar("checked", PM.Parameters.EvalShowRewardThankYou)
                PM.Parameters.EvalShowRewardThankYou = boolResult
                PM.Parameters.Save()
                sParam = boolResult.ToString
                ' D3964 ==
            Case "ShowConsistencyRatio"
                Dim boolResult = CheckVar("checked", PP.ShowConsistencyRatio)
                PP.ShowConsistencyRatio = boolResult
                sParam = boolResult.ToString
            Case "ShowInfoDocs"
                Dim boolResult = CheckVar("checked", PP.ShowInfoDocs)
                PP.ShowInfoDocs = boolResult
                sParam = boolResult.ToString
            Case "ShowInfoDocsMode"
                Dim intResult = CInt(PP.ShowInfoDocsMode)
                Dim boolResult = CheckVar("checked", intResult = 0)
                PP.ShowInfoDocsMode = If(boolResult, ShowInfoDocsMode.sidmFrame, ShowInfoDocsMode.sidmPopup)
                sParam = CInt(PP.ShowInfoDocsMode).ToString
                ' D4713 ===
            Case "HideInfodocCaptions"
                Dim boolResult = CheckVar("checked", PM.Parameters.EvalHideInfodocCaptions)
                PM.Parameters.EvalHideInfodocCaptions = boolResult
                PM.Parameters.Save()
                sParam = boolResult.ToString
                ' D4713 ==
                ' D6523 ===
            Case "ImagesZoom"
                Dim boolResult = CheckVar("checked", PM.Parameters.AutoFitInfoDocImages)
                PM.Parameters.AutoFitInfoDocImages = boolResult
                PM.Parameters.Save()
                sParam = boolResult.ToString
                ' D6523 ==
                ' D6962 ===
            Case "Text2Speech"
                Dim Mode = CheckVar("val", CInt(PM.Parameters.EvalTextToSpeech))
                PM.Parameters.EvalTextToSpeech = CType(Mode, ecText2Speech)
                PM.Parameters.Save()
                sParam = Mode.ToString
                ' D6962 ==
                ' D4714 ===
            Case "ShowRiskResults"
                Dim boolResult = CheckVar("checked", HasEmbeddedContent(PM.Parameters.EvalEmbeddedContent, EmbeddedContentType.RiskResults))
                PM.Parameters.EvalEmbeddedContent = SetEmbeddedContent(PM.Parameters.EvalEmbeddedContent, EmbeddedContentType.RiskResults, boolResult)
                PM.Parameters.Save()
                sParam = boolResult.ToString
                ' D4714 ==
                ' D6664 ===
            Case "ShowHeatMap"
                Dim boolResult = CheckVar("checked", HasEmbeddedContent(PM.Parameters.EvalEmbeddedContent, EmbeddedContentType.HeatMap))
                PM.Parameters.EvalEmbeddedContent = SetEmbeddedContent(PM.Parameters.EvalEmbeddedContent, EmbeddedContentType.HeatMap, boolResult)
                PM.Parameters.Save()
                sParam = boolResult.ToString
                ' D6664 ==
            Case "ShowFullObjectivePath"
                ' D4100 ===
                Dim intResult As Integer = CheckVar("val", CInt(PP.ShowFullObjectivePath))
                PP.ShowFullObjectivePath = CType(intResult, ecShowObjectivePath)
                sParam = PP.ShowFullObjectivePath.ToString
                ' D4100 ==
            Case "ShowComments"
                Dim boolResult = CheckVar("checked", PP.ShowComments)
                PP.ShowComments = boolResult
                sParam = boolResult.ToString
            Case "UseCISForIndividuals"
                Dim boolResult = CheckVar("checked", PP.UseCISForIndividuals)
                PP.UseCISForIndividuals = boolResult
                sParam = boolResult.ToString
            Case "DisableWarningsOnStepChange"
                Dim boolResult = CheckVar("checked", PP.DisableWarningsOnStepChange)
                PP.DisableWarningsOnStepChange = boolResult
                sParam = boolResult.ToString
                ' -D4118
                '    ' D3953 ===
                'Case "collapse_multipw_bars"
                '    Dim boolResult = CheckVar("checked", PM.Parameters.EvalCollapseMultiPWBars)
                '    PM.Parameters.EvalCollapseMultiPWBars = boolResult
                '    sParam = boolResult.ToString
                '    ' D3953 ==
            Case "ShowSensitivityAnalysis"
                Dim boolResult = CheckVar("checked", PP.ShowSensitivityAnalysis)
                PP.ShowSensitivityAnalysis = boolResult
                sParam = boolResult.ToString
            Case "ShowSensitivityAnalysisGradient"
                Dim boolResult = CheckVar("checked", PP.ShowSensitivityAnalysisGradient)
                PP.ShowSensitivityAnalysisGradient = boolResult
                sParam = boolResult.ToString
            Case "ShowSensitivityAnalysisPerformance"
                Dim boolResult = CheckVar("checked", PP.ShowSensitivityAnalysisPerformance)
                PP.ShowSensitivityAnalysisPerformance = boolResult
                sParam = boolResult.ToString
            Case "CalculateSAForCombined"
                Dim boolResult = CheckVar("val", PP.CalculateSAForCombined)
                sParam = boolResult.ToString
                PP.CalculateSAForCombined = boolResult
            Case "SynthesisMode"
                Dim intResult = CheckVar("val", PP.SynthesisMode)
                PP.SynthesisMode = CType(intResult, ECSynthesisMode)
                sParam = intResult.ToString
            Case "ShowWelcomeSurvey"
                Dim boolResult = CheckVar("checked", PP.ShowWelcomeSurvey)
                PP.ShowWelcomeSurvey = boolResult
                sParam = boolResult.ToString
            Case "ShowThankYouSurvey"
                Dim boolResult = CheckVar("checked", PP.ShowThankYouSurvey)
                PP.ShowThankYouSurvey = boolResult
                sParam = boolResult.ToString
                ' D3977 ===
            Case "WelcomeSurveyFirst"
                Dim boolResult = CheckVar("val", PM.Parameters.EvalWelcomeSurveyFirst)  ' D4780
                PM.Parameters.EvalWelcomeSurveyFirst = boolResult
                PM.Parameters.Save()
                sParam = boolResult.ToString
            Case "ThankYouSurveyLast"
                Dim boolResult = CheckVar("val", PM.Parameters.EvalThankYouSurveyLast)  ' D4780
                PM.Parameters.EvalThankYouSurveyLast = boolResult
                PM.Parameters.Save()
                sParam = boolResult.ToString
                ' D3977 ==
                ' D4162 ===
            Case "prj_list"
                sResult = String.Format("[-1,'{0}', '', '']", JS_SafeString(ResString("lblNavNoNextProject")))
                For Each tPrj As clsProject In App.ActiveProjectsList
                    Dim tWS As clsWorkspace = clsWorkspace.WorkspaceByUserIDAndProjectID(App.ActiveUser.UserID, tPrj.ID, App.Workspaces)
                    If (tPrj.IsRisk OrElse tPrj.ID <> App.ProjectID) AndAlso tPrj.ProjectStatus = ecProjectStatus.psActive AndAlso Not tPrj.isMarkedAsDeleted AndAlso _
                       App.CanUserModifyProject(App.ActiveUser.UserID, tPrj.ID, App.ActiveUserWorkgroup, tWS) Then  ' D4172
                        ' D4172 ===
                        If tPrj.IsRisk AndAlso (tPrj.PasscodeImpact = "" OrElse tPrj.PasscodeImpact.ToLower = tPrj.PasscodeLikelihood.ToLower) Then
                            tPrj.PasscodeImpact = App.ProjectUniquePasscode(tPrj.PasscodeImpact, -1)
                            App.DBProjectUpdate(tPrj, False, "Create Impact passcode")
                        End If
                        ' D4172 ==
                        sResult += String.Format("{0}[{1},'{2}','{3}','{4}']", If(sResult = "", "", ","), tPrj.ID, JS_SafeString(tPrj.ProjectName), JS_SafeString(tPrj.PasscodeLikelihood), JS_SafeString(tPrj.PasscodeImpact))
                    End If
                Next
                sResult = "[" + sResult + "]"
                fSaveProject = False
            Case "nextproject"
                Dim sVal = CheckVar("val", PM.Parameters.EvalNextProjectPasscodeAtFinish).Trim
                PM.Parameters.EvalNextProjectPasscodeAtFinish = sVal
                PM.Parameters.Save()
                ' D4164 ===
                Dim tPrj As clsProject = GetNextProject(App.ActiveProject)
                If tPrj IsNot Nothing AndAlso (Not tPrj.isOnline OrElse Not tPrj.isPublic) AndAlso App.CanUserModifyProject(App.ActiveUser.UserID, tPrj.ID, App.ActiveUserWorkgroup, App.DBWorkspaceByUserIDProjectID(App.ActiveUser.UserID, tPrj.ID), App.ActiveWorkgroup) Then
                    If App.ActiveWorkgroup.License.CheckParameterByID(ecLicenseParameter.MaxProjectsOnline, tPrj, False) Then
                        tPrj.isOnline = True
                        tPrj.isPublic = True
                        App.DBProjectUpdate(tPrj, False, "Set project on-line")
                    End If
                End If
                ' D4164 ==
                sParam = sVal

                ' D4376 ===
            Case "getusers"
                fSaveProject = False
                sResult = ""
                Dim UsersData As HashSet(Of Integer) = PM.StorageManager.Reader.DataExistsForUsersHashset(PM.ActiveHierarchy) 'PM.DataExistsForUsers()
                Dim UsersList As New List(Of clsUser)
                For Each tUID As Integer In UsersData
                    Dim tAHPUser As clsUser = PM.GetUserByID(tUID)
                    If tAHPUser IsNot Nothing Then UsersList.Add(tAHPUser)
                Next
                UsersList.Sort(New clsUserComparer(ecUserSort.usName, SortDirection.Ascending))
                For Each tAHPUser As clsUser In UsersList
                    sResult += String.Format("{0}[{1},'{2}','{3}']", If(sResult = "", "", ","), tAHPUser.UserID, JS_SafeString(tAHPUser.UserEMail), JS_SafeString(If(tAHPUser.UserName.ToLower = tAHPUser.UserEMail.ToLower, "", tAHPUser.UserName)))
                Next
                sResult = String.Format("[{0}]", sResult)

            Case "set_combined"
                PM.Parameters.ResultsCustomCombinedUID = CheckVar("uid", PM.Parameters.ResultsCustomCombinedUID)
                PM.Parameters.ResultsCustomCombinedName = CheckVar("name", PM.Parameters.ResultsCustomCombinedName).Trim
                CheckCombinedUser()
                App.ActiveProject.SaveProjectOptions("Set combined user", , , If(PM.Parameters.ResultsCustomCombinedUID = COMBINED_USER_ID, "COMBINED", PM.Parameters.ResultsCustomCombinedName))
                sResult = GetCombinedName(False)
                fSaveProject = False
                ' D4376 ==

                ' D6321 ===
            Case "copy_settings"
                If App.isRiskEnabled AndAlso Not isReadOnly() Then
                    Dim ObjWording As String = PM.PipeParameters.NameObjectives
                    Dim AltWording As String = PM.PipeParameters.NameAlternatives   ' D7212
                    Dim DestPS As ParameterSet
                    If App.ActiveProject.isImpact Then
                        DestPS = PM.PipeParameters.DefaultParameterSet
                        sParam = "Impact>Likelihood"
                    Else
                        DestPS = PM.PipeParameters.ImpactParameterSet
                        sParam = "Likelihood>Impact"
                    End If

                    fSaveProject = PM.PipeParameters.CopyParameterSet(PM.PipeParameters.CurrentParameterSet, DestPS)

                    If fSaveProject Then
                        PM.PipeParameters.NameObjectives = ObjWording
                        PM.PipeParameters.NameAlternatives = AltWording
                        Dim SrcID As Integer = PM.PipeParameters.CurrentParameterSet.ID
                        Dim DestID As Integer = DestPS.ID
                        For Each tPair As KeyValuePair(Of String, clsParameter) In PM.Parameters.Parameters.Parameters
                            For Each tParam As KeyValuePair(Of Integer, Object) In tPair.Value.Values
                                If tParam.Key = SrcID Then
                                    PM.Parameters.Parameters.SetParameter(tPair.Key, tParam.Value, DestID)
                                    Exit For
                                End If
                            Next
                        Next
                    End If
                End If
                sResult = String.Format("['{0}',{1}]", sAction, Bool2JS(fSaveProject))
                ' D6321 ==

            Case Else
                fSaveProject = False
                ' D4162 ==
        End Select

        If fSaveProject Then
            PM.PipeBuilder.PipeCreated = False  ' D3977
            App.ActiveProject.SaveProjectOptions("Update evaluation setting", , , String.Format("{0}:{1}", sAction, sParam))    ' D7577
        End If

        If sResult <> "" Then
            Response.Clear()
            Response.ContentType = "text/plain"
            Response.Write(sResult)
            Response.End()
        End If

    End Sub

    Protected Sub Page_Init(sender As Object, e As EventArgs) Handles Me.Init
        AlignHorizontalCenter = True
        AlignVerticalCenter = False
        Dim pgid As Integer = CheckVar("pgid", CurrentPageID)
        ' D4664 ===
        Select Case pgid
            Case _PGID_PROJECT_OPTION_DISPLAY, _PGID_PROJECT_OPTION_EVALUATE, _PGID_PROJECT_OPTION_NAVIGATE, _PGID_PROJECT_OPTION_SURVEY
                CurrentPageID = pgid
        End Select
        ' D4664 ==
        HasWelcomeSurvey = PM.StorageManager.Reader.IsWelcomeSurveyAvailable(App.ActiveProject.isImpact)    ' D3804
        HasThankYouSurvey = PM.StorageManager.Reader.IsThankYouSurveyAvailable(App.ActiveProject.isImpact)   ' D3804
        '_OPT_CUSTOM_COMBINED = ShowDraftPages()   ' D4376
        If isAJAX() Then Ajax_Callback()
        ' D4162 ===
        If Not isCallback AndAlso Not IsPostBack Then
            CheckCombinedUser() ' D4376
        End If
        ' D4162 ==
        ' D7566 ===
        Dim sMode As String = CheckVar("mode", "").Trim
        Dim ApplyMode As Boolean = False
        Select Case sMode.ToLower
            Case _OPT_MODE_AREA_VALIDATION2, _OPT_MODE_ALEXA_PROJECT
                ApplyMode = True
            Case "none", "empty", "clear", "delete", "remove"   ' allow to reset
                sMode = ""
                ApplyMode = True
        End Select
        If ApplyMode Then
            PM.Parameters.SpecialMode = sMode
            PRJ.SaveProjectOptions("Apply special mode", ,, "Mode: " + If(sMode = "", " - none -", sMode))
            Response.Redirect(PageURL(CurrentPageID), True)
        End If
        ' D7566 ==
    End Sub

End Class