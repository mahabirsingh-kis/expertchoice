Imports Canvas
Imports ECCore
Imports ExpertChoice.Data
Imports ExpertChoice.Service
Imports ExpertChoice.Web
Imports ECWeb = ExpertChoice.Web



Public Class TeamTimeClass
    Private Shared _Action As clsAction
    Public Shared sCallbackSave As String = ""
    Public Shared _TeamTimeRefreshTimeoutLoaded As Boolean = False
    Public Shared _TeamTimeRefreshTimeout As Integer = WebOptions.SynchronousRefresh()
    Public Shared ClusterPhrase As String = ""
    Public Shared ClusterPhraseIsCustom As Boolean = False
    Public Shared TaskNodeGUID As String = ""
    Private Shared sOldWRTPath As String = Nothing
    Private Shared SESS_WRT_PATH As String = ""
    Private Shared _TeamTimeUsersList As List(Of ECTypes.clsUser) = Nothing
    Public Shared TaskTemplates As Dictionary(Of String, String) = New Dictionary(Of String, String)()
    Private Shared _isTeamTime As Boolean = False
    Friend Const _SESS_VARIANCE As String = "tt_variance"
    Friend Const _SESS_PW_SIDE As String = "tt_pw_side"
    Friend Const _SESS_HIDE_COMBINED As String = "tt_no_combined"
    Friend Const _COOKIE_USERSPAGE As String = "tt_pg"
    Friend Const _COOKIE_USERS_PAGES_USE As String = "tt_up"
    Friend Const _COOKIE_USERS_PAGES_SIZE As String = "tt_ps"
    Friend Const _COOKIE_TT_NORM As String = "tt_res_norm"
    Friend Const _OPT_SHOW_ONLINE_ONLY_PROJECT As Boolean = True
    Const SessUserDisplay As String = "UserDisplayName"
    Const SessHidePie As String = "SessHidePie"
    Private Const _TeamTimeSessionDataTimeoutCoeff As Integer = 10
    Public Shared _TeamTimePipeStep As Integer = -1
    Public Shared _TeamTimeActive As Boolean = True
    Friend Const ShowStepsCount As Integer = 11
    Public Shared globalGUID As String = ""
    Public Shared language As clsLanguageResource
    Private Shared _TeamTime_Pipe As clsTeamTimePipe = Nothing
    Public Const _SESS_TT_Pipe As String = "Sess_TT_Pipe"
    Private Const _SESS_TT_Step As String = "_SESS_TT_Step"
    Private Const _SESS_STEPS_LIST As String = "tt_steps_list"
    Private Const _SESS_TT_USERSLIST As String = "tt_users_list"
    Private Shared _wrtNodeID As Integer = -1
    Private Shared _ForceSaveStepInfo As Boolean = False

    Public Enum SynchronousEvaluationMode
        semNone = 0
        semOnline = 1
        semVotingBox = 2
        semByFacilitatorOnly = 3
    End Enum

    Public Shared Function ResString(ByVal sResourceName As String, ByVal Optional fAsIsIfMissed As Boolean = False, ByVal Optional fParseTemplates As Boolean = True, ByVal Optional fCapitalized As Boolean = False) As String
        Dim context = HttpContext.Current
        Dim app = CType(context.Session("App"), clsComparionCore)
        Dim sRes = app.ResString(sResourceName, fAsIsIfMissed)

        If sResourceName.ToLower() <> _TEMPL_APPNAME.ToLower() AndAlso sResourceName.ToLower() <> _TEMPL_APPNAME_PLAIN.ToLower() Then
            If fParseTemplates AndAlso sRes.IndexOf("%") >= 0 Then sRes = ParseAllTemplates(sRes, app.ActiveUser, app.ActiveProject)
        End If

        If fCapitalized Then
            sRes = sRes.Substring(0, 1).ToUpper() + sRes.Substring(1).ToLower()
        End If

        Return sRes
    End Function

    Public Shared Function ParseAllTemplates(ByVal sMessage As String, ByVal tUser As clsApplicationUser, ByVal tProject As clsProject, ByVal Optional fUseJSSafeString As Boolean = False, ByVal Optional fReplaceOnlyCommon As Boolean = False) As String
        Dim context As HttpContext = HttpContext.Current
        Dim App = CType(context.Session("App"), clsComparionCore)
        If sMessage.IndexOf("%") < 0 Then Return sMessage
        Dim sRes As String = ParseTemplate(sMessage, _TEMPL_APPNAME, ApplicationName(), fUseJSSafeString)
        sRes = ParseTemplate(sRes, _TEMPL_APPNAME_PLAIN, ApplicationName(False), fUseJSSafeString)
        sRes = ParseTemplate(sRes, _TEMPL_APPNAME_TEAMTIME, App.ResString("titleApplicationTeamTime"), fUseJSSafeString)
        sRes = ParseTemplate(sRes, _TEMPL_APPNAME_SURVEY, App.ResString("titleApplicationSurvey"), fUseJSSafeString)
        sRes = ParseTemplate(sRes, _TEMPL_PROJECTS, App.ResString("templ_Projects"), fUseJSSafeString)
        sRes = ParseTemplate(sRes, _TEMPL_PROJECT, App.ResString("templ_Project"), fUseJSSafeString)
        sRes = ParseTemplate(sRes, _TEMPL_MODELS, App.ResString("templ_Models"), fUseJSSafeString)
        sRes = ParseTemplate(sRes, _TEMPL_MODEL, App.ResString("templ_Model"), fUseJSSafeString)
        sRes = ParseTemplate(sRes, _TEMPL_SERVICE_EMAIL, SystemEmail, fUseJSSafeString)
        Dim sUserName As String = ""
        Dim sUserEmail As String = ""
        Dim sUserPsw As String = "*********"

        If (tUser IsNot Nothing) Then
            sUserEmail = tUser.UserEmail
            sUserName = tUser.UserName
        End If

        Dim sOwnerName As String = ""
        Dim sOwnerEmail As String = ""

        If (App.ActiveUser IsNot Nothing) Then
            sOwnerName = App.ActiveUser.UserName
            sOwnerEmail = App.ActiveUser.UserEmail
        End If

        sRes = ParseURLAndPathTemplates(sRes, fUseJSSafeString)
        Dim sPrjName As String = ""
        Dim sPrjPasscode As String = ""
        Dim sMeetingID As String = ""
        Dim sRiskModel As String = ""
        Dim sRiskModels As String = ""

        If (tProject IsNot Nothing) Then
            sPrjName = StringFuncs.SafeFormString(tProject.ProjectName)
            sPrjPasscode = StringFuncs.SafeFormString(tProject.Passcode)
            sMeetingID = clsMeetingID.AsString(tProject.MeetingID())

            If Not String.IsNullOrEmpty(sUserEmail) AndAlso tProject.isValidDBVersion Then
                Dim tPrjuser As ECTypes.clsUser = tProject.ProjectManager.GetUserByEMail(sUserEmail)
                If tPrjuser IsNot Nothing AndAlso Not String.IsNullOrEmpty(tPrjuser.UserName) Then sUserName = tPrjuser.UserName

                If App.ActiveUser IsNot Nothing Then
                    tPrjuser = tProject.ProjectManager.GetUserByEMail(App.ActiveUser.UserEmail)
                    If tPrjuser IsNot Nothing AndAlso Not String.IsNullOrEmpty(tPrjuser.UserName) Then sOwnerName = tPrjuser.UserName
                End If

                If App.isRiskEnabled Then
                    sRiskModel = App.ResString(Convert.ToString((If(tProject.ProjectManager.ActiveHierarchy = Convert.ToInt32(ECTypes.ECHierarchyID.hidImpact), "lblImpact", "lblLikelihood"))))
                    sRiskModels = App.ResString(Convert.ToString((If(tProject.ProjectManager.ActiveHierarchy = Convert.ToInt32(ECTypes.ECHierarchyID.hidImpact), "lblImpacts", "lblLikelihoods"))))
                Else
                    sRiskModel = App.ResString("lblProject")
                    sRiskModels = App.ResString("lblProjects")
                End If
            End If
        End If

        If String.IsNullOrEmpty(sUserPsw) Then sUserPsw = App.ResString("lblDummyBlankPassword")

        If Not fReplaceOnlyCommon Then
            sRes = ParseTemplate(sRes, _TEMPL_USEREMAIL, sUserEmail, fUseJSSafeString)
            sRes = ParseTemplate(sRes, _TEMPL_USERNAME, sUserName, fUseJSSafeString)
            sRes = ParseTemplate(sRes, _TEMPL_USER_FIRSTNAME, StringFuncs.GetUserFirstName(sUserName), fUseJSSafeString)
            sRes = ParseTemplate(sRes, _TEMPL_USERPSW, sUserPsw, fUseJSSafeString)
        End If

        Dim sWkgName As String = ""
        If App.ActiveWorkgroup IsNot Nothing Then sWkgName = App.ActiveWorkgroup.Name
        sRes = ParseTemplate(sRes, _TEMPL_OWNERNAME, sOwnerName, fUseJSSafeString)
        sRes = ParseTemplate(sRes, _TEMPL_OWNEREMAIL, sOwnerEmail, fUseJSSafeString)
        sRes = ParseTemplate(sRes, _TEMPL_WORKGROUP, sWkgName, fUseJSSafeString)
        sRes = ParseTemplate(sRes, _TEMPL_PRJNAME, sPrjName, fUseJSSafeString)
        sRes = ParseTemplate(sRes, _TEMPL_PRJPASSCODE, sPrjPasscode, fUseJSSafeString)
        sRes = ParseTemplate(sRes, _TEMPL_RISKMODEL, sRiskModel, fUseJSSafeString)
        sRes = ParseTemplate(sRes, _TEMPL_RISKMODELS, sRiskModels, fUseJSSafeString)
        sRes = ParseTemplate(sRes, _TEMPL_MEETING_ID, sMeetingID, fUseJSSafeString)
        Dim sStartURL As String = ApplicationURL(False, False) & Consts._URL_ROOT
        Dim sEvalAnytimeURL As String = ApplicationURL(True, False)
        Dim sEvalTTURL As String = ApplicationURL(True, True)
        sRes = ParseTemplate(sRes, _TEMPL_URL_APP, sStartURL, fUseJSSafeString)
        sRes = ParseTemplate(sRes, _TEMPL_URL_APP_TT, sEvalTTURL, fUseJSSafeString)

        If Not fReplaceOnlyCommon Then
            If sRes.ToLower().Contains(_TEMPL_URL_LOGIN.ToLower()) Then sRes = ParseTemplate(sRes, _TEMPL_URL_LOGIN, CreateLogonURL(tUser, Nothing, "", sStartURL), fUseJSSafeString)
            If sRes.ToLower().Contains(_TEMPL_URL_EVALUATE.ToLower()) Then sRes = ParseTemplate(sRes, _TEMPL_URL_EVALUATE, CreateLogonURL(tUser, tProject, "pipe=yes", sEvalAnytimeURL), fUseJSSafeString)
            If sRes.ToLower().Contains(_TEMPL_URL_EVALUATE_TT.ToLower()) Then sRes = ParseTemplate(sRes, _TEMPL_URL_EVALUATE_TT, CreateLogonURL(tUser, tProject, "TTOnly=1", sEvalTTURL), fUseJSSafeString)
            If App.isRiskEnabled AndAlso sRes.ToLower().Contains(_TEMPL_URL_EVALUATE_LIKELIHOOD.ToLower()) Then sRes = ParseTemplate(sRes, _TEMPL_URL_EVALUATE_LIKELIHOOD, CreateLogonURL(tUser, tProject, "pipe=yes", sEvalAnytimeURL, tProject.PasscodeLikelihood), fUseJSSafeString)
            If App.isRiskEnabled AndAlso sRes.ToLower().Contains(_TEMPL_URL_EVALUATE_IMPACT.ToLower()) Then sRes = ParseTemplate(sRes, _TEMPL_URL_EVALUATE_IMPACT, CreateLogonURL(tUser, tProject, "pipe=yes", sEvalAnytimeURL, tProject.PasscodeImpact), fUseJSSafeString)
            If App.isRiskEnabled AndAlso sRes.ToLower().Contains(_TEMPL_URL_EVALUATE_CONTROLS.ToLower()) Then sRes = ParseTemplate(sRes, _TEMPL_URL_EVALUATE_CONTROLS, CreateLogonURL(tUser, tProject, "pipe=yes&mode=riskcontrols", sEvalAnytimeURL, tProject.PasscodeLikelihood), fUseJSSafeString)
            If sRes.ToLower().Contains(_TEMPL_URL_MEETINGID.ToLower()) Then sRes = ParseTemplate(sRes, _TEMPL_URL_MEETINGID, String.Format("{0}?{1}={2}", sEvalTTURL, _PARAM_MEETING_ID, sMeetingID), fUseJSSafeString)
            If sRes.ToLower().Contains(_TEMPL_URL_EVALUATE_ANONYM.ToLower()) Then sRes = ParseTemplate(sRes, _TEMPL_URL_EVALUATE_ANONYM, CreateEvaluateSignupURL(tProject, True, "", "", sStartURL), fUseJSSafeString)
            If sRes.ToLower().Contains(_TEMPL_URL_EVALUATE_SIGNUP.ToLower()) Then sRes = ParseTemplate(sRes, _TEMPL_URL_EVALUATE_SIGNUP, CreateEvaluateSignupURL(tProject, False, "", "", sStartURL), fUseJSSafeString)
            If sRes.ToLower().Contains(_TEMPL_URL_RESETPSW.ToLower()) AndAlso tUser IsNot Nothing Then sRes = ParseTemplate(sRes, _TEMPL_URL_RESETPSW, CreateResetPswURL(tUser, "", sStartURL), fUseJSSafeString)
            If sRes.ToLower().Contains(_TEMPL_URL_EVALUATE_SIGNUP_EMAILONLY.ToLower()) Then sRes = ParseTemplate(sRes, _TEMPL_URL_EVALUATE_SIGNUP_EMAILONLY, CreateEvaluateSignupURL(tProject, False, "e", "", sStartURL), fUseJSSafeString)
            If sRes.ToLower().Contains(_TEMPL_URL_EVALUATE_SIGNUP_EMAILPSW.ToLower()) Then sRes = ParseTemplate(sRes, _TEMPL_URL_EVALUATE_SIGNUP_EMAILPSW, CreateEvaluateSignupURL(tProject, False, "ep", "", sStartURL), fUseJSSafeString)
            If sRes.ToLower().Contains(_TEMPL_URL_EVALUATE_SIGNUP_NAMEONLY.ToLower()) Then sRes = ParseTemplate(sRes, _TEMPL_URL_EVALUATE_SIGNUP_NAMEONLY, CreateEvaluateSignupURL(tProject, False, "n", "", sStartURL), fUseJSSafeString)
        End If

        If sRes.ToLower().Contains(_TEMPL_IP.ToLower()) Then sRes = ParseTemplate(sRes, _TEMPL_IP, context.Request.UserHostAddress, fUseJSSafeString)
        If sRes.ToLower().Contains(_TEMPL_PSWLOCK_TIMEOUT.ToLower()) Then sRes = ParseTemplate(sRes, _TEMPL_PSWLOCK_TIMEOUT, Consts._DEF_PASSWORD_ATTEMPTS_LOCK_TIMEOUT.ToString(), fUseJSSafeString)
        If sRes.ToLower().Contains(_TEMPL_PSWLOCK_ATTEMPTS.ToLower()) Then sRes = ParseTemplate(sRes, _TEMPL_PSWLOCK_ATTEMPTS, Consts._DEF_PASSWORD_ATTEMPTS.ToString(), fUseJSSafeString)

        If sRes.Contains("%%") AndAlso App.ActiveWorkgroup IsNot Nothing AndAlso App.ActiveWorkgroup.WordingTemplates IsNot Nothing Then
            Dim lres As String = sRes.ToLower()

            If App.HasActiveProject() AndAlso App.ActiveProject IsNot Nothing AndAlso App.ActiveProject.ProjectManager.PipeParameters.ProjectType = CanvasTypes.ProjectType.ptOpportunities AndAlso (lres.Contains(Consts._TPL_RISK_VULNERABILITIES) OrElse lres.Contains(Consts._TPL_RISK_VULNERABILITY) OrElse lres.Contains(Consts._TPL_RISK_EVENT) OrElse lres.Contains(Consts._TPL_RISK_EVENTS) OrElse lres.Contains(Consts._TPL_RISK_CONTROL) OrElse lres.Contains(Consts._TPL_RISK_CONTROLS) OrElse lres.Contains(Consts._TPL_RISK_RISK) OrElse lres.Contains(Consts._TPL_RISK_RISKS)) Then
                sRes = ParseTemplate(sRes, Consts._TPL_RISK_VULNERABILITY, App.ResString("templ_vulnerability_for_opportunity"), fUseJSSafeString)
                sRes = ParseTemplate(sRes, Consts._TPL_RISK_VULNERABILITIES, App.ResString("templ_vulnerabilities_for_opportunity"), fUseJSSafeString)
                sRes = ParseTemplate(sRes, Consts._TPL_RISK_EVENT, App.ResString("templ_event_for_opportunity"), fUseJSSafeString)
                sRes = ParseTemplate(sRes, Consts._TPL_RISK_EVENTS, App.ResString("templ_events_for_opportunity"), fUseJSSafeString)
                sRes = ParseTemplate(sRes, Consts._TPL_RISK_CONTROL, App.ResString("templ_control_for_opportunity"), fUseJSSafeString)
                sRes = ParseTemplate(sRes, Consts._TPL_RISK_CONTROLS, App.ResString("templ_controls_for_opportunity"), fUseJSSafeString)
                sRes = ParseTemplate(sRes, Consts._TPL_RISK_RISK, App.ResString("templ_risk_for_opportunity"), fUseJSSafeString)
                sRes = ParseTemplate(sRes, Consts._TPL_RISK_RISKS, App.ResString("templ_risks_for_opportunity"), fUseJSSafeString)
            End If

            For Each sName As String In App.ActiveWorkgroup.WordingTemplates.Keys
                Dim value = ""
                App.ActiveWorkgroup.WordingTemplates.TryGetValue(sName, value)
                If value.Contains("!") Then value = value.Replace("%%", " ")

                If App.ActiveWorkgroup.WordingTemplates(sName).Contains("tpl_comparion_alternative") Then
                    value = ResString("lblWordingTemplatesAlternative")
                End If

                If App.ActiveWorkgroup.WordingTemplates(sName).Contains("tpl_comparion_alternatives") Then
                    value = ResString("lblWordingTemplatesAlternatives")
                End If

                If App.ActiveWorkgroup.WordingTemplates(sName).Contains("tpl_comparion_objective") Then
                    value = ResString("lblWordingTemplatesObjective")
                End If

                If App.ActiveWorkgroup.WordingTemplates(sName).Contains("tpl_comparion_objectives") Then
                    value = ResString("lblWordingTemplatesObjectives")
                End If

                If sName.Contains("%%") Then

                    If sRes.ToLower().Contains(sName.ToLower()) Then
                        sRes = ParseTemplate(sRes, sName, value, fUseJSSafeString)
                    End If
                End If
            Next
        End If

        Dim _TPL_ALT_OBJ As String() = {_TEMPL_ALTERNATIVES, _TEMPL_ALTERNATIVE, _TEMPL_OBJECTIVES, _TEMPL_OBJECTIVE}

        If sRes.Contains("%%") Then

            For Each sTempl As String In _TPL_ALT_OBJ
                If sRes.ToLower().Contains(sTempl.ToLower()) Then Return PrepareTask(sRes)
            Next
        End If

        Return sRes
    End Function

    Public Shared Function ParseTemplate(ByVal sMessage As String, ByVal sTemplateName As String, ByVal sTemplateValue As String, ByVal Optional fUseJSSafeString As Boolean = False) As String
        Return ParseTemplateCommon(sMessage, sTemplateName, sTemplateValue, fUseJSSafeString)
    End Function

    Public Shared Function ApplicationURL(ByVal isEvaluationSite As Boolean, ByVal fIsTeamTime As Boolean) As String
        Dim context As HttpContext = HttpContext.Current
        Dim App = CType(context.Session("App"), clsComparionCore)
        Dim sURL As String = context.Request.Url.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped)
        If isEvaluationSite AndAlso Not String.IsNullOrEmpty(App.Options.EvalSiteURL) AndAlso (Not App.Options.EvalURL4TeamTime OrElse fIsTeamTime) Then sURL = App.Options.EvalSiteURL
        Return sURL
    End Function

    Public Shared Function CreateLogonURL(ByVal tUser As clsApplicationUser, ByVal tProject As clsProject, ByVal sOtherParams As String, ByVal sPagePath As String, ByVal Optional sPasscode As String = Nothing) As String
        Dim context As HttpContext = HttpContext.Current
        Dim App = CType(context.Session("App"), clsComparionCore)
        Dim sURL As String = ""

        If (tUser IsNot Nothing) Then
            sURL = String.Format("{0}={1}&{2}={3}", _PARAM_EMAIL, HttpUtility.UrlEncode(tUser.UserEmail), _PARAM_PASSWORD, HttpUtility.UrlEncode(tUser.UserPassword))

            If (tProject IsNot Nothing) Then
                sURL += String.Format("&{0}={1}", _PARAM_PASSCODE, HttpUtility.UrlEncode(Convert.ToString((If(String.IsNullOrEmpty(sPasscode), tProject.Passcode, sPasscode)))))
            End If
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

    Public Shared Function ParseTemplateCommon(ByVal sMessage As String, ByVal sTemplateName As String, ByVal sTemplateValue As String, ByVal Optional fUseJSSafeString As Boolean = False) As String
        If fUseJSSafeString Then sTemplateValue = StringFuncs.JS_SafeString(sTemplateValue)
        sMessage = sMessage.Replace("%%" & StringFuncs.Capitalize(sTemplateName.Trim(Convert.ToChar("%"))) & "%%", StringFuncs.Capitalize(sTemplateValue))
        Return sMessage.Replace(sTemplateName, sTemplateValue)
    End Function

    Public Shared Function PrepareTask(ByVal sTask As String, ByVal Optional tExtraParam As Object = Nothing, ByVal Optional fHasSubNodes As Boolean = False, ByVal Optional tUsedParams As Dictionary(Of String, String) = Nothing) As String
        Dim context As HttpContext = HttpContext.Current
        Dim App = CType(context.Session("App"), clsComparionCore)
        Return PrepareTaskCommon(App, sTask, tExtraParam, fHasSubNodes)
    End Function

    Public Shared Function PrepareTaskCommon(ByVal tApp As clsComparionCore, ByVal sTask As String, ByVal Optional tExtraParam As Object = Nothing, ByVal Optional fHasSubNodes As Boolean = False, ByVal Optional tUsedParams As Dictionary(Of String, String) = Nothing) As String
        Dim sObjectives As String = ""
        Dim sObjective As String = ""
        Dim sAlternatives As String = ""
        Dim sAlternative As String = ""
        Dim sPromtObj As String = ""
        Dim sPromtAlt As String = ""

        If sTask IsNot Nothing AndAlso tApp IsNot Nothing Then
            Dim fIsImpact As Boolean = False

            If tApp.ActiveProject IsNot Nothing AndAlso tApp.ActiveProject.isValidDBVersion Then
                Dim _with1 = tApp.ActiveProject.ProjectManager.PipeParameters
                Dim OldParams As PipeParameters.ParameterSet = _with1.CurrentParameterSet
                Dim sHid As String = ""

                If tApp.ActiveProject.ProjectManager.ActiveHierarchy = Convert.ToInt32(ECTypes.ECHierarchyID.hidImpact) Then
                    sHid = "_Impact"
                    If Not Object.ReferenceEquals(_with1.CurrentParameterSet, _with1.ImpactParameterSet) Then _with1.CurrentParameterSet = _with1.ImpactParameterSet
                    fIsImpact = True
                End If

                If tApp.ActiveProject.ProjectManager.ActiveHierarchy = Convert.ToInt32(ECTypes.ECHierarchyID.hidLikelihood) Then
                    sHid = "_Likelihood"
                    If Not Object.ReferenceEquals(_with1.CurrentParameterSet, _with1.DefaultParameterSet) Then _with1.CurrentParameterSet = _with1.DefaultParameterSet
                End If

                sAlternatives = _with1.NameAlternatives
                sObjectives = _with1.NameObjectives

                If Not String.IsNullOrEmpty(_with1.JudgementPromt) Then
                    sPromtObj = _with1.JudgementPromt
                Else
                    Dim sTempl As String = "lbl_promt_obj{1}_{0}"
                    Dim flag As Boolean = False
                    sPromtObj = tApp.CurrentLanguage.GetString(String.Format(sTempl, (If(_with1.JudgementPromtID < 0, 0, _with1.JudgementPromtID)), sHid), "", flag)
                    If String.IsNullOrEmpty(sPromtObj) Then sPromtObj = tApp.ResString(String.Format(sTempl, 0, sHid))
                End If

                If Not String.IsNullOrEmpty(_with1.JudgementAltsPromt) Then
                    sPromtAlt = _with1.JudgementAltsPromt
                Else
                    Dim sTempl As String = "lbl_promt_alt{1}_{0}"
                    Dim flag As Boolean = False
                    sPromtAlt = tApp.CurrentLanguage.GetString(String.Format(sTempl, (If(_with1.JudgementAltsPromtID < 0, 0, _with1.JudgementAltsPromtID)), sHid), "", flag)
                    If String.IsNullOrEmpty(sPromtAlt) Then sPromtAlt = tApp.ResString(String.Format(sTempl, 0, sHid))
                End If

                _with1.CurrentParameterSet = OldParams
            End If

            If tApp.isRiskEnabled AndAlso tApp.ActiveWorkgroup IsNot Nothing AndAlso tApp.ActiveWorkgroup.WordingTemplates IsNot Nothing Then
                If String.IsNullOrEmpty(sAlternatives) AndAlso tApp.ActiveWorkgroup.WordingTemplates.ContainsKey(Consts._TPL_RISK_EVENTS) Then sAlternatives = tApp.ActiveWorkgroup.WordingTemplates(Consts._TPL_RISK_EVENTS)
                If String.IsNullOrEmpty(sAlternative) AndAlso tApp.ActiveWorkgroup.WordingTemplates.ContainsKey(Consts._TPL_RISK_EVENT) Then sAlternative = tApp.ActiveWorkgroup.WordingTemplates(Consts._TPL_RISK_EVENT)
                Dim sObjName As String = Convert.ToString((If(fIsImpact, Consts._TPL_RISK_CONSEQUENCES, Consts._TPL_RISK_SOURCES)))
                If String.IsNullOrEmpty(sObjectives) AndAlso tApp.ActiveWorkgroup.WordingTemplates.ContainsKey(sObjName) Then sObjectives = tApp.ActiveWorkgroup.WordingTemplates(sObjName)
                sObjName = Convert.ToString((If(fIsImpact, Consts._TPL_RISK_CONSEQUENCE, Consts._TPL_RISK_SOURCE)))
                If String.IsNullOrEmpty(sObjective) AndAlso tApp.ActiveWorkgroup.WordingTemplates.ContainsKey(sObjName) Then sObjective = tApp.ActiveWorkgroup.WordingTemplates(sObjName)
            End If

            If String.IsNullOrEmpty(sAlternatives) Then sAlternatives = tApp.ResString(Convert.ToString((If(tApp.isRiskEnabled, "templ_AlternativesRisk", "templ_Alternatives"))))
            If String.IsNullOrEmpty(sAlternative) Then sAlternative = tApp.ResString(Convert.ToString((If(tApp.isRiskEnabled, "templ_AlternativeRisk", "templ_Alternative"))))
            If String.IsNullOrEmpty(sObjectives) OrElse fHasSubNodes Then sObjectives = tApp.ResString(Convert.ToString((If(tApp.isRiskEnabled, (If(fIsImpact, "templ_Objectives_Impact", "templ_ObjectivesRisk")), "templ_Objectives"))))
            If String.IsNullOrEmpty(sObjective) OrElse fHasSubNodes Then sObjective = tApp.ResString(Convert.ToString((If(tApp.isRiskEnabled, (If(fIsImpact, "templ_Objective_Impact", "templ_ObjectiveRisk")), "templ_Objective"))))

            If tExtraParam IsNot Nothing AndAlso (TypeOf (tExtraParam) Is clsStepFunction OrElse TypeOf (tExtraParam) Is clsRatingScale) Then

                If TypeOf (tExtraParam) Is clsStepFunction Then
                    sObjective = tApp.ResString("templ_Interval")
                    sObjectives = tApp.ResString("templ_Intervals")
                Else
                    sObjective = tApp.ResString("templ_Intensity")
                    sObjectives = tApp.ResString("templ_Intensities")
                End If

                sAlternative = sObjective
                sAlternatives = sObjectives
            End If

            Dim tParams As Dictionary(Of String, String) = New Dictionary(Of String, String)()

            If tUsedParams IsNot Nothing Then
                tParams = tUsedParams
            End If

            tParams.Add(_TEMPL_ALTERNATIVES, StringFuncs.JS_SafeHTML(sAlternatives))
            tParams.Add(_TEMPL_ALTERNATIVE, StringFuncs.JS_SafeHTML(sAlternative))
            tParams.Add(_TEMPL_OBJECTIVES, StringFuncs.JS_SafeHTML(sObjectives))
            tParams.Add(_TEMPL_OBJECTIVE, StringFuncs.JS_SafeHTML(sObjective))

            If sTask.ToLower().Contains("%%promt_") Then
                tParams.Add(_TEMPL_PROMT_OBJ, sPromtObj)
                tParams.Add(_TEMPL_PROMT_ALT, sPromtAlt)
                tParams.Add(_TEMPL_PROMT_ALT_WORD, clsComparionCorePage.GetPromptWordCommon(tApp, True))
                tParams.Add(_TEMPL_PROMT_OBJ_WORD, clsComparionCorePage.GetPromptWordCommon(tApp, False))
            End If

            If sTask.Contains("wording%%") Then
                tParams.Add(_TEMPL_RATE_WORDING, clsComparionCorePage.GetRatingAltsWordingCommon(tApp, True))
                tParams.Add(_TEMPL_RATE_OBJ_WORDING, clsComparionCorePage.GetRatingObjWordingCommon(tApp, True))
                tParams.Add(_TEMPL_EST_WORDING, clsComparionCorePage.GetRatingAltsWordingCommon(tApp, False))
                tParams.Add(_TEMPL_EST_OBJ_WORDING, clsComparionCorePage.GetRatingObjWordingCommon(tApp, False))
            End If

            sTask = StringFuncs.ParseStringTemplates(sTask, tParams)

            If sTask.Contains("%%") AndAlso tApp.isRiskEnabled AndAlso tApp.ActiveWorkgroup IsNot Nothing AndAlso tApp.ActiveWorkgroup.WordingTemplates IsNot Nothing Then

                For Each sName As String In tApp.ActiveWorkgroup.WordingTemplates.Keys

                    If sTask.ToLower().Contains(sName.ToLower()) Then
                        sTask = ParseTemplateCommon(sTask, sName, tApp.ActiveWorkgroup.WordingTemplates(sName), False)
                    End If
                Next
            End If
        End If

        Return sTask
    End Function

    Public Shared Function CreateEvaluateSignupURL(ByVal tProject As clsProject, ByVal fIsAnonymous As Boolean, ByVal sSignupMode As String, ByVal sOtherParams As String, ByVal sPagePath As String) As String
        Dim context As HttpContext = HttpContext.Current
        Dim App = CType(context.Session("App"), clsComparionCore)
        Dim sURL As String = ""
        If tProject Is Nothing Then Return sURL
        sURL += String.Format("&{0}=1&{1}={2}&{3}={4}&{5}={6}", _PARAMS_SIGNUP(0), _PARAMS_ANONYMOUS_SIGNUP(0), (If(fIsAnonymous, "1", "0")), _PARAM_PASSCODE, HttpUtility.UrlEncode(App.ActiveProject.Passcode), _PARAMS_SIGNUP_MODE(0), sSignupMode)

        If Not String.IsNullOrEmpty(sOtherParams) Then
            If Not String.IsNullOrEmpty(sURL) Then sURL += "&"
            sURL += sOtherParams
        End If

        sURL = CryptService.EncodeURL(sURL, App.DatabaseID)

        If App.Options.UseTinyURL Then
            Dim PID As Integer = -1
            If tProject IsNot Nothing Then PID = tProject.ID
            sURL = String.Format("{0}?{2}={1}", sPagePath, App.CreateTinyURL(sURL, PID, -1), _PARAMS_TINYURL(0))
        Else
            sURL = String.Format("{0}?{2}={1}", sPagePath, sURL, _PARAMS_KEY(0))
        End If

        Return sURL
    End Function

    Public Shared Function ApplicationName(ByVal Optional NameWithMark As Boolean = True) As String
        Dim context As HttpContext = HttpContext.Current
        Dim App = CType(context.Session("App"), clsComparionCore)
        Return App.ResString(Convert.ToString((If(App.isRiskEnabled, "titleApplicationNameRisk", "titleApplicationName"))) + Convert.ToString((If(NameWithMark, "", "Plain"))))
    End Function

    Public Shared Function ParseURLAndPathTemplates(ByVal sMessage As String, ByVal Optional fUseJSSafeString As Boolean = False) As String
        Dim sRes As String = sMessage
        sRes = ParseTemplate(sRes, _TEMPL_ROOT_URI, Consts._URL_ROOT, fUseJSSafeString)
        sRes = ParseTemplate(sRes, _TEMPL_ROOT_PATH, Consts._FILE_ROOT, fUseJSSafeString)
        sRes = ParseTemplate(sRes, _TEMPL_ROOT_IMAGES, ThemePath & Consts._FILE_IMAGES, fUseJSSafeString)
        Return sRes
    End Function

    Public Shared ReadOnly Property ThemePath As String
        Get
            Dim page As Page = CType(HttpContext.Current.Session("thispage"), Page)
            Return String.Format("{0}{1}/", _URL_THEMES, (If(String.IsNullOrEmpty(page.Theme), PagesList._THEME_EC09, page.Theme)))
        End Get
    End Property

    Public Shared Function CreateResetPswURL(ByVal tUser As clsApplicationUser, ByVal sOtherParams As String, ByVal sPagePath As String) As String
        Dim context As HttpContext = HttpContext.Current
        Dim App = CType(context.Session("App"), clsComparionCore)
        Dim sURL As String = ""

        If tUser IsNot Nothing AndAlso Not tUser.CannotBeDeleted Then
            App.DBTinyURLDelete(-1, -2, tUser.UserID)
            sURL = String.Format("{0}=resetpsw&ue={1}&up={2}&t={3}", _PARAM_ACTION, tUser.UserEmail, tUser.UserPassword, DateTime.Now.Ticks)
            sURL = CryptService.EncodeURL(sURL, App.DatabaseID)
            sURL = String.Format("{0}Password.aspx?{2}={1}", sPagePath, App.CreateTinyURL(sURL, -2, tUser.UserID), _PARAMS_TINYURL(0))
        End If

        Return sURL
    End Function

    Public Shared Property isTeamTime As Boolean
        Get
            Dim teamtime As Boolean = False
            Dim context As HttpContext = HttpContext.Current

            If context IsNot Nothing AndAlso context.Session IsNot Nothing Then
                Dim App = CType(context.Session("App"), clsComparionCore)

                If App.ActiveProject IsNot Nothing Then
                    teamtime = (App.ActiveProject.isTeamTimeLikelihood OrElse App.ActiveProject.isTeamTimeImpact)
                End If
            End If

            Return teamtime
        End Get
        Set(ByVal value As Boolean)
            _isTeamTime = value
        End Set
    End Property

    Public Shared ReadOnly Property isTeamTimeOwner As Boolean
        Get
            Dim teamtimeOwner As Boolean = False
            Dim context As HttpContext = HttpContext.Current

            If context IsNot Nothing AndAlso context.Session IsNot Nothing Then
                Dim App = CType(context.Session("App"), clsComparionCore)

                If isTeamTime Then

                    'If App.ActiveProject.get_MeetingStatus(App.ActiveUser) = ECTeamTimeStatus.tsTeamTimeSessionOwner Then
                    If App.ActiveProject.MeetingStatus(App.ActiveUser) = ECTeamTimeStatus.tsTeamTimeSessionOwner Then
                        teamtimeOwner = True
                    End If
                Else

                    If Consts._LOGIN_WITH_MEETINGID_TO_INACTIVE_MEETING AndAlso (App.Options.isLoggedInWithMeetingID OrElse App.Options.OnlyTeamTimeEvaluation) Then
                        teamtimeOwner = False
                    End If
                End If
            End If

            Return teamtimeOwner
        End Get
    End Property

    Public Shared Property TeamTime As clsTeamTimePipe
        Get
            Dim context As HttpContext = HttpContext.Current

            If context IsNot Nothing AndAlso context.Session IsNot Nothing Then
                Dim App = CType(context.Session("App"), clsComparionCore)

                If isTeamTimeOwner Then

                    If _TeamTime_Pipe IsNot Nothing Then

                        If _TeamTime_Pipe.ProjectManager.StorageManager.ModelID <> App.ProjectID Then
                            _TeamTime_Pipe = Nothing
                        End If
                    End If

                    If context.Session(_SESS_TT_Pipe) Is Nothing Then
                        _TeamTime_Pipe = Nothing

                        If context.Session(_SESS_TT_Pipe) IsNot Nothing Then
                            _TeamTime_Pipe = CType(context.Session(_SESS_TT_Pipe), clsTeamTimePipe)

                            If (_TeamTime_Pipe.ProjectManager.StorageManager.ModelID <> App.ProjectID) OrElse (_TeamTime_Pipe.ProjectManager.ActiveHierarchy <> App.ActiveProject.ProjectManager.ActiveHierarchy) Then
                                _TeamTime_Pipe = Nothing
                            End If
                        End If

                        If context.Session(_SESS_TT_Pipe) Is Nothing Then
                            _TeamTime_Pipe = New clsTeamTimePipe(App.ActiveProject.ProjectManager, App.ActiveProject.ProjectManager.User)
                            _TeamTime_Pipe.Override_ResultsMode = True
                            _TeamTime_Pipe.ResultsViewMode = Canvas.CanvasTypes.ResultsView.rvBoth

                            If context.Session(_SESS_TT_USERSLIST) Is Nothing Then
                                _TeamTime_Pipe.VerifyUsers(TeamTimeUsersList)
                            Else
                                _TeamTime_Pipe.VerifyUsers(CType(context.Session(_SESS_TT_USERSLIST), List(Of ECTypes.clsUser)))
                            End If

                            _TeamTime_Pipe.CreatePipe()
                            _ForceSaveStepInfo = True
                            context.Session.Remove(_SESS_TT_Pipe)
                            context.Session.Add(_SESS_TT_Pipe, _TeamTime_Pipe)
                            context.Session.Remove(_SESS_STEPS_LIST)
                        End If
                    End If

                    _TeamTime_Pipe = CType(context.Session(_SESS_TT_Pipe), clsTeamTimePipe)

                    If _TeamTime_Pipe.ProjectManager.ActiveHierarchy < 0 Then
                        _TeamTime_Pipe = New clsTeamTimePipe(App.ActiveProject.ProjectManager, App.ActiveProject.ProjectManager.User)
                        _TeamTime_Pipe.Override_ResultsMode = True
                        _TeamTime_Pipe.ResultsViewMode = Canvas.CanvasTypes.ResultsView.rvBoth
                        _TeamTime_Pipe.VerifyUsers(TeamTimeUsersList)
                        _TeamTime_Pipe.CreatePipe()
                        _ForceSaveStepInfo = True
                        context.Session.Remove(_SESS_TT_Pipe)
                        context.Session.Add(_SESS_TT_Pipe, _TeamTime_Pipe)
                        context.Session.Remove(_SESS_STEPS_LIST)
                    End If
                Else

                    If _TeamTime_Pipe IsNot Nothing Then

                        If _TeamTime_Pipe.ProjectManager.StorageManager.ModelID <> App.ProjectID Then
                            _TeamTime_Pipe = New clsTeamTimePipe(App.ActiveProject.ProjectManager, App.ActiveProject.ProjectManager.GetUserByID(App.ActiveProject.MeetingOwnerID))
                            _TeamTime_Pipe.Override_ResultsMode = True
                            _TeamTime_Pipe.ResultsViewMode = Canvas.CanvasTypes.ResultsView.rvBoth
                            _TeamTime_Pipe.VerifyUsers(TeamTimeUsersList)
                            _TeamTime_Pipe.CreatePipe()
                            _ForceSaveStepInfo = True
                            context.Session.Remove(_SESS_TT_Pipe)
                            context.Session.Add(_SESS_TT_Pipe, _TeamTime_Pipe)
                            context.Session.Remove(_SESS_STEPS_LIST)
                        End If
                    End If
                End If
            End If

            Return _TeamTime_Pipe
        End Get
        Set(ByVal value As clsTeamTimePipe)
            Dim context As HttpContext = HttpContext.Current

            If context IsNot Nothing AndAlso context.Session IsNot Nothing Then
                _TeamTime_Pipe = value
                context.Session(_SESS_TT_Pipe) = _TeamTime_Pipe
                context.Session.Remove(_SESS_STEPS_LIST)
            End If
        End Set
    End Property

    Public Shared Property TeamTimeUsersList As List(Of ECTypes.clsUser)
        Get
            Dim context As HttpContext = HttpContext.Current
            Dim App = CType(context.Session("App"), clsComparionCore)
            Dim userddd = App.ActiveUser.UserEmail
            Dim userlist = New List(Of ECTypes.clsUser)()

            If isTeamTimeOwner Then
                If context.Session(_SESS_TT_USERSLIST) IsNot Nothing Then userlist = CType(context.Session(_SESS_TT_USERSLIST), List(Of ECTypes.clsUser))
            Else
                userlist = _TeamTimeUsersList
                If context.Session(_SESS_TT_USERSLIST) Is Nothing Then userlist = Nothing
            End If

            If _TeamTimeUsersList Is Nothing OrElse userlist IsNot _TeamTimeUsersList Then
                userlist = New List(Of ECTypes.clsUser)()
                Dim tWSList As List(Of clsWorkspace) = App.DBWorkspacesByProjectID(App.ProjectID)
                context.Session(_SESS_TT_USERSLIST) = CType(userlist, List(Of ECTypes.clsUser))

                For Each tAppUser As clsApplicationUser In UsersList
                    Dim tWS As clsWorkspace = clsWorkspace.WorkspaceByUserIDAndProjectID(tAppUser.UserID, App.ProjectID, tWSList)
                    'If tWS IsNot Nothing AndAlso clsOnlineUserSession.OnlineSessionByUserID(tWS.UserID, SessionsList) IsNot Nothing Then tWS.set_Status(App.ActiveProject.isImpact, ecWorkspaceStatus.wsSynhronousActive)
                    'If tWS IsNot Nothing AndAlso clsOnlineUserSession.OnlineSessionByUserID(tWS.UserID, SessionsList) IsNot Nothing Then tWS.Status(App.ActiveProject.isImpact, ecWorkspaceStatus.wsSynhronousActive)
                    context.Session(_SESS_TT_USERSLIST) = CType(userlist, List(Of ECTypes.clsUser))

                    'If tWS IsNot Nothing AndAlso tWS.get_isInTeamTime(App.ActiveProject.isImpact) Then
                    If tWS IsNot Nothing AndAlso tWS.isInTeamTime(App.ActiveProject.isImpact) Then
                        context.Session(_SESS_TT_USERSLIST) = CType(userlist, List(Of ECTypes.clsUser))
                        Dim fCanParticipate As Boolean = True
                        'If Not TeamTimePipeParams.TeamTimeDisplayUsersWithViewOnlyAccess AndAlso tWS.get_TeamTimeStatus(App.ActiveProject.isImpact) = ecWorkspaceStatus.wsSynhronousReadOnly Then fCanParticipate = False
                        If Not TeamTimePipeParams.TeamTimeDisplayUsersWithViewOnlyAccess AndAlso tWS.TeamTimeStatus(App.ActiveProject.isImpact) = ecWorkspaceStatus.wsSynhronousReadOnly Then fCanParticipate = False
                        If TeamTimePipeParams.TeamTimeHideProjectOwner AndAlso App.ActiveProject.MeetingOwnerID = tAppUser.UserID Then fCanParticipate = False

                        If fCanParticipate Then
                            Dim tPrjUser As ECTypes.clsUser = TeamTime.ProjectManager.GetUserByEMail(tAppUser.UserEmail)
                            Dim fUpdate As Boolean = False

                            If tPrjUser Is Nothing Then
                                tPrjUser = TeamTime.ProjectManager.AddUser(tAppUser.UserEmail, True, tAppUser.UserName)
                                tPrjUser.IncludedInSynchronous = True
                                tPrjUser.SyncEvaluationMode = CType(SynchronousEvaluationMode.semOnline, ECTypes.SynchronousEvaluationMode)
                                fUpdate = True
                            Else

                                If Not tPrjUser.IncludedInSynchronous Or tPrjUser.SyncEvaluationMode = CType(SynchronousEvaluationMode.semNone, ECTypes.SynchronousEvaluationMode) Then
                                    tPrjUser.IncludedInSynchronous = True
                                    If tPrjUser.SyncEvaluationMode = CType(SynchronousEvaluationMode.semNone, ECTypes.SynchronousEvaluationMode) Then tPrjUser.SyncEvaluationMode = CType(SynchronousEvaluationMode.semOnline, ECTypes.SynchronousEvaluationMode)
                                    fUpdate = True
                                End If
                            End If

                            context.Session(_SESS_TT_USERSLIST) = CType(userlist, List(Of ECTypes.clsUser))
                            If fUpdate Then TeamTime.ProjectManager.StorageManager.Writer.SaveModelStructure()
                            Dim tAddUser As ECTypes.clsUser = tPrjUser.Clone()
                            tAddUser.UserName = tPrjUser.UserName
                            tAddUser.Active = clsOnlineUserSession.OnlineSessionByUserID(tAppUser.UserID, SessionsList) IsNot Nothing

                            If Not (Not TeamTimePipeParams.TeamTimeDisplayUsersWithViewOnlyAccess AndAlso tPrjUser IsNot Nothing AndAlso tPrjUser.SyncEvaluationMode = CType(SynchronousEvaluationMode.semByFacilitatorOnly, ECTypes.SynchronousEvaluationMode)) Then
                                userlist.Add(tAddUser)
                            End If

                            context.Session(_SESS_TT_USERSLIST) = CType(userlist, List(Of ECTypes.clsUser))
                        End If
                    End If
                Next

                Dim Fld As ecUserSort = ecUserSort.usEmail

                Select Case TeamTimePipeParams.TeamTimeUsersSorting
                    Case CanvasTypes.TTUsersSorting.ttusName
                        Fld = ecUserSort.usName
                    Case CanvasTypes.TTUsersSorting.ttusKeypadID
                        Fld = ecUserSort.usKeyPad
                    Case Else
                        Fld = ecUserSort.usEmail
                End Select

                Dim cmp As clsUserComparer = New clsUserComparer(Fld, SortDirection.Ascending)
                userlist.Sort(cmp)
            End If

            context.Session(_SESS_TT_USERSLIST) = CType(userlist, List(Of ECTypes.clsUser))
            _TeamTimeUsersList = userlist
            Return CType(context.Session(_SESS_TT_USERSLIST), List(Of ECTypes.clsUser))
        End Get
        Set(ByVal value As List(Of ECTypes.clsUser))
            _TeamTimeUsersList = value
        End Set
    End Property

    Public Shared ReadOnly Property SessionsList As List(Of clsOnlineUserSession)
        Get
            Dim context As HttpContext = HttpContext.Current
            Dim sessions_List As List(Of clsOnlineUserSession) = New List(Of clsOnlineUserSession)()

            If context IsNot Nothing AndAlso context.Session IsNot Nothing Then
                Dim App = CType(context.Session("App"), clsComparionCore)
                Dim AllOnline As List(Of clsOnlineUserSession) = App.DBOnlineSessions()

                For Each onlineUser As clsOnlineUserSession In AllOnline

                    If clsApplicationUser.UserByUserID(onlineUser.UserID, UsersList) IsNot Nothing AndAlso (Not _OPT_SHOW_ONLINE_ONLY_PROJECT OrElse onlineUser.ProjectID = App.ProjectID) Then
                        sessions_List.Add(onlineUser)
                    End If
                Next
            End If

            Return sessions_List
        End Get
    End Property

    Public Shared ReadOnly Property UsersList As List(Of clsApplicationUser)
        Get
            Dim context As HttpContext = HttpContext.Current
            Dim users_List As List(Of clsApplicationUser) = New List(Of clsApplicationUser)()

            If context IsNot Nothing AndAlso context.Session IsNot Nothing Then
                Dim App = CType(context.Session("App"), clsComparionCore)
                users_List = App.DBUsersByProjectID(App.ProjectID)
            End If

            Return users_List
        End Get
    End Property

    Public Shared ReadOnly Property TeamTimePipeParams As Canvas.PipeParameters.clsPipeParamaters
        Get
            Dim context As HttpContext = HttpContext.Current

            If context IsNot Nothing AndAlso context.Session IsNot Nothing Then
                Dim App = CType(context.Session("App"), clsComparionCore)

                If isTeamTimeOwner Then
                    Return TeamTime.PipeParameters
                Else
                    Return App.ActiveProject.PipeParameters
                End If
            Else
                Return Nothing
            End If
        End Get
    End Property

    Public Shared Function LanguagesScanFolder(ByVal sPath As String) As List(Of clsLanguageResource)
        Dim context As HttpContext = HttpContext.Current
        Dim App = CType(context.Session("App"), clsComparionCore)
        Dim Languages As List(Of clsLanguageResource) = New List(Of clsLanguageResource)()
        Dim files As String() = System.IO.Directory.GetFiles(context.Server.MapPath("~/App_GlobalResources/"), "*.resx")
        Dim AllFiles = New System.Collections.ObjectModel.ReadOnlyCollection(Of String)(files)
        _Login.debugging = ""

        For Each sFileName As String In AllFiles

            If System.IO.Path.GetExtension(sFileName).ToLower() = Consts._FILE_RESOURCE_EXT And Not System.IO.Path.GetFileName(sFileName).StartsWith("~") Then
                _Login.debugging += (sFileName & " <br>")
                Dim Lng As clsLanguageResource = New clsLanguageResource()
                Lng.ResxFilename = sFileName

                If Lng.isLoaded Then
                    Languages.Add(Lng)
                Else
                End If
            End If
        Next

        _Login.debugging += ("Done <br>")
        Return Languages
    End Function

    Public Shared Function get_SessVar(ByVal sName As String) As String
        Dim context As HttpContext = HttpContext.Current
        Dim App = CType(context.Session("App"), clsComparionCore)

        If context.Session(sName) Is Nothing Then
            Return Nothing
        Else
            Return Convert.ToString(context.Session(sName))
        End If
    End Function

    Public Shared Sub set_SessVar(ByVal sName As String, ByVal value As String)
        Dim context As HttpContext = HttpContext.Current
        Dim App = CType(context.Session("App"), clsComparionCore)

        If value Is Nothing And (context.Session(sName) IsNot Nothing) Then
            context.Session.Remove(sName)
        Else

            If context.Session(sName) Is Nothing Then
                context.Session.Add(sName, value)
            Else
                context.Session(sName) = value
            End If
        End If
    End Sub

    Public Shared Function GetPipeStepTask(ByVal Action As clsAction, ByVal tExtraParam As Object, ByVal Optional fHasSubnodes As Boolean = False, ByVal Optional fIgnoreClusterPhrase As Boolean = False, ByVal Optional fCanBePathInteractive As Boolean = True, ByVal Optional fParseNodeNames As Boolean = True) As String
        Dim context As HttpContext = HttpContext.Current
        Dim App = CType(context.Session("App"), clsComparionCore)
        Dim sRes As String = ""
        Dim Params As Dictionary(Of String, String) = New Dictionary(Of String, String)()
        Dim fGetResString As Boolean = True
        Dim tClusterNode As clsNode = Nothing

        If App.HasActiveProject() AndAlso Action IsNot Nothing AndAlso Action.ActionData IsNot Nothing Then
            Dim Hierarchy As clsHierarchy = App.ActiveProject.HierarchyObjectives
            Dim isImpact As Boolean = App.ActiveProject.ProjectManager.ActiveHierarchy = Convert.ToInt16(ECTypes.ECHierarchyID.hidImpact)
            Dim IsRiskWithControls As Boolean = False

            ' D2503

            Select Case Action.ActionType
                Case ActionType.atPairwise, ActionType.atPairwiseOutcomes
                    Dim lData As clsPairwiseMeasureData = CType(Action.ActionData, clsPairwiseMeasureData)
                    Dim parentNode As clsNode = Nothing

                    Select Case Action.ActionType
                        Case ActionType.atPairwise
                            parentNode = App.ActiveProject.HierarchyObjectives.GetNodeByID(lData.ParentNodeID)
                            Hierarchy = CType(If(parentNode.IsTerminalNode, App.ActiveProject.HierarchyAlternatives, App.ActiveProject.HierarchyObjectives), clsHierarchy)
                        Case ActionType.atPairwiseOutcomes
                            parentNode = Action.ParentNode
                    End Select

                    If App.isRiskEnabled Then

                        If parentNode.RiskNodeType = ECTypes.RiskNodeType.ntCategory Then
                            sRes = Convert.ToString((If(parentNode.IsTerminalNode, "task_Pairwise_Alternatives_Category", "task_Pairwise_Objectives_Category")))
                        Else

                            If parentNode.IsTerminalNode Then
                                sRes = Convert.ToString((If(parentNode.Level = 0, "task_Pairwise_AlternativesNoObj", (If(isImpact, "task_Pairwise_Alternatives", "task_Pairwise_AlternativesLikelihood")))))
                            Else

                                If parentNode.ParentNode() Is Nothing Then
                                    sRes = Convert.ToString((If(tExtraParam IsNot Nothing, "task_Pairwise_ObjectivesIntensities", "task_Pairwise_ObjectivesGoal")))
                                Else
                                    sRes = Convert.ToString((If(isImpact, "task_Pairwise_Objectives", "task_Pairwise_ObjectivesLikelihood")))
                                End If
                            End If
                        End If
                    Else
                        sRes = Convert.ToString((If(parentNode.IsTerminalNode, "task_Pairwise_Alternatives", "task_Pairwise_Objectives")))
                    End If

                    Dim tNodeLeft As clsNode = New clsNode()
                    Dim tNodeRight As clsNode = New clsNode()
                    Dim fIsPWOutcomes As Boolean = Action.ActionType = ActionType.atPairwiseOutcomes

                    ' D2351
                    If fIsPWOutcomes AndAlso parentNode IsNot Nothing Then
                        Dim tRS As clsRatingScale = CType(Action.PWONode.MeasurementScale, clsRatingScale)
                        sRes = Convert.ToString((If(parentNode.IsAlternative, (If(Action.PWONode.ParentNode() Is Nothing, "task_PairwiseOutcomesAltGoal", "task_PairwiseOutcomesAlt")), If(Action.PWONode Is Nothing, "task_PairwiseOutcomesGoal", "task_PairwiseOutcomes"))))
                        ' D2318 + D2351 + D2410 + D2438
                        If parentNode.Level > 1 Then sRes = "task_PairwiseOutcomesLevels"
                        If tRS.IsPWofPercentages Then sRes = Convert.ToString((If(parentNode.ParentNode() Is Nothing, "task_PairwiseOfPercentagesGoal", "task_PairwiseOfPercentages")))
                        If tRS.IsExpectedValues Then sRes = Convert.ToString((If(parentNode.ParentNode() Is Nothing, "task_PairwiseExpectedValuesGoal", "task_PairwiseExpectedValues")))
                        App.ActiveProject.ProjectManager.PipeBuilder.GetPWNodes(Action, lData, tNodeLeft, tNodeRight)
                        ' D2830
                        Params.Add(ECWeb.Options._TEMPL_JUSTNODE, GetWRTNodeNameWithPath(Action.PWONode, fCanBePathInteractive))
                    Else

                        If Hierarchy IsNot Nothing Then
                            tNodeLeft = Hierarchy.GetNodeByID(lData.FirstNodeID)
                            tNodeRight = Hierarchy.GetNodeByID(lData.SecondNodeID)
                        End If
                    End If

                    If parentNode IsNot Nothing Then Params.Add(ECWeb.Options._TEMPL_NODENAME, GetWRTNodeNameWithPath(parentNode, fCanBePathInteractive))
                    ' D2830
                    If tNodeLeft IsNot Nothing Then Params.Add(ECWeb.Options._TEMPL_NODE_A, StringFuncs.JS_SafeHTML(tNodeLeft.NodeName))
                    If tNodeRight IsNot Nothing Then Params.Add(ECWeb.Options._TEMPL_NODE_B, StringFuncs.JS_SafeHTML(tNodeRight.NodeName))
                    ' D2364
                    tClusterNode = parentNode
                Case ActionType.atNonPWOneAtATime
                    Dim data As clsOneAtATimeEvaluationActionData = CType(Action.ActionData, clsOneAtATimeEvaluationActionData)

                    If data IsNot Nothing Then

                        ' D2503 ===
                        If IsRiskWithControls Then

                            If data IsNot Nothing AndAlso data.Assignment IsNot Nothing AndAlso data.Control IsNot Nothing Then
                                Params.Add(ECWeb.Options._TEMPL_NODENAME, data.Control.Name)
                                Dim tNode As clsNode = Nothing
                                Dim WRT As clsNode = Nothing

                                Select Case data.Control.Type
                                    Case ControlType.ctCause

                                        If Not Guid.Equals(data.Assignment.ObjectiveID, Guid.Empty) Then
                                            tNode = App.ActiveProject.ProjectManager.Hierarchy(Convert.ToInt16(ECTypes.ECHierarchyID.hidLikelihood)).GetNodeByID(data.Assignment.ObjectiveID)
                                            If tNode IsNot Nothing Then Params.Add(ECWeb.Options._TEMPL_NODE_A, StringFuncs.JS_SafeHTML(tNode.NodeName))
                                            WRT = tNode
                                            tNode = App.ActiveProject.ProjectManager.AltsHierarchy(App.ActiveProject.ProjectManager.ActiveAltsHierarchy).GetNodeByID(data.Assignment.EventID)
                                            If tNode IsNot Nothing Then Params.Add(ECWeb.Options._TEMPL_JUSTNODE, StringFuncs.JS_SafeHTML(tNode.NodeName))
                                        End If

                                    Case ControlType.ctCauseToEvent

                                        If Not Guid.Equals(data.Assignment.ObjectiveID, Guid.Empty) Then
                                            tNode = App.ActiveProject.ProjectManager.Hierarchy(Convert.ToInt16(ECTypes.ECHierarchyID.hidLikelihood)).GetNodeByID(data.Assignment.ObjectiveID)
                                            If tNode IsNot Nothing Then Params.Add(ECWeb.Options._TEMPL_NODE_A, StringFuncs.JS_SafeHTML(tNode.NodeName))
                                            WRT = tNode
                                            tNode = App.ActiveProject.ProjectManager.AltsHierarchy(App.ActiveProject.ProjectManager.ActiveAltsHierarchy).GetNodeByID(data.Assignment.EventID)
                                            If tNode IsNot Nothing Then Params.Add(ECWeb.Options._TEMPL_JUSTNODE, StringFuncs.JS_SafeHTML(tNode.NodeName))
                                        End If

                                    Case ControlType.ctConsequenceToEvent

                                        If Not Guid.Equals(data.Assignment.ObjectiveID, Guid.Empty) Then
                                            tNode = App.ActiveProject.ProjectManager.Hierarchy(Convert.ToInt16(ECTypes.ECHierarchyID.hidImpact)).GetNodeByID(data.Assignment.ObjectiveID)
                                            If tNode IsNot Nothing Then Params.Add(ECWeb.Options._TEMPL_NODE_B, StringFuncs.JS_SafeHTML(tNode.NodeName))
                                            WRT = tNode
                                            tNode = App.ActiveProject.ProjectManager.AltsHierarchy(App.ActiveProject.ProjectManager.ActiveAltsHierarchy).GetNodeByID(data.Assignment.EventID)
                                            If tNode IsNot Nothing Then Params.Add(ECWeb.Options._TEMPL_JUSTNODE, StringFuncs.JS_SafeHTML(tNode.NodeName))
                                        End If
                                End Select

                                tClusterNode = WRT
                                ' D2703

                                Dim sName As String = ""

                                Select Case data.Control.Type
                                    Case ControlType.ctCause
                                        sName = App.ResString("lblControlCause")
                                    Case ControlType.ctCauseToEvent
                                        ' D2654
                                        sName = App.ResString(Convert.ToString(If(WRT IsNot Nothing AndAlso WRT.Level = 0, "lblControlCauseToEventGoal", "lblControlCauseToEvent")))
                                    Case ControlType.ctConsequenceToEvent
                                        sName = App.ResString("lblControlConsequence")
                                End Select

                                sRes = String.Format(App.ResString("taskControlNonPWOneAtATime"), sName)
                                fGetResString = False
                            End If
                        Else

                            ' D2503 ==
                            Select Case data.MeasurementType
                                Case ECMeasureType.mtRatings
                                    ' D2508 ===
                                    ' D2589 ===
                                    Dim isAlt As Boolean = data.Node IsNot Nothing AndAlso (data.Node.IsAlternative OrElse data.Node.IsTerminalNode)
                                    ' D2530 + D2587
                                    Dim tData As clsNonPairwiseMeasureData = CType(data.Judgment, clsNonPairwiseMeasureData)
                                    Dim tNode As clsNode = Nothing

                                    ' D2530
                                    If isAlt Then
                                        tNode = App.ActiveProject.HierarchyAlternatives.GetNodeByID(CType(data.Judgment, clsNonPairwiseMeasureData).NodeID)
                                    Else
                                        tNode = App.ActiveProject.HierarchyObjectives.GetNodeByID(CType(data.Judgment, clsNonPairwiseMeasureData).NodeID)

                                        If tNode Is Nothing Then
                                            tNode = App.ActiveProject.HierarchyAlternatives.GetNodeByID(CType(data.Judgment, clsNonPairwiseMeasureData).NodeID)
                                            isAlt = True
                                        End If
                                    End If

                                    If tNode IsNot Nothing Then Params.Add(ECWeb.Options._TEMPL_NODE_A, StringFuncs.JS_SafeHTML(tNode.NodeName))
                                    ' D2589 ==
                                    ' D1508 ==
                                    Params.Add(ECWeb.Options._TEMPL_NODE_B, GetWRTNodeNameWithPath(data.Node, fCanBePathInteractive))
                                    Params.Add(ECWeb.Options._TEMPL_NODENAME, GetWRTNodeNameWithPath(data.Node, fCanBePathInteractive))
                                    Params.Add(ECWeb.Options._TEMPL_EVALCOUNT, StringFuncs.JS_SafeHTML(data.Node.Children.Count))


                                    ' D0558 + D2830
                                    If App.isRiskEnabled Then

                                        If isImpact Then
                                            sRes = Convert.ToString(If(data.Node.Level > 0, "lblEvaluationRatingImpact", "lblEvaluationRatingImpactNoLevels"))
                                        Else
                                            sRes = Convert.ToString(If(data.Node.Level > 0, "lblEvaluationRatingRisk", "lblEvaluationRatingNoLevelsRisk"))
                                            ' D2407
                                        End If
                                    Else
                                        sRes = Convert.ToString(If(data.Node.Level > 0, "lblEvaluationRating", "lblEvaluationRatingNoLevels"))
                                        ' D2589
                                    End If

                                    ' D2527 + D2530
                                    If data.Node IsNot Nothing AndAlso Not isAlt Then sRes += "Obj"
                                Case ECMeasureType.mtStep
                                    Dim tStep As clsStepMeasureData = CType(data.Judgment, clsStepMeasureData)
                                    Dim tParentNode As clsNode = CType(data.Node.Hierarchy.GetNodeByID(tStep.ParentNodeID), clsNode)
                                    Dim tAlt As clsNode = DirectCast(Nothing, clsNode)

                                    If tParentNode.IsTerminalNode Then
                                        tAlt = data.Node.Hierarchy.ProjectManager.AltsHierarchy(data.Node.Hierarchy.ProjectManager.ActiveAltsHierarchy).GetNodeByID(tStep.NodeID)
                                    Else
                                        tAlt = data.Node.Hierarchy.GetNodeByID(tStep.NodeID)
                                    End If

                                    Params.Add(ECWeb.Options._TEMPL_NODE_A, GetWRTNodeNameWithPath(tParentNode, fCanBePathInteractive))
                                    ' D2830

                                    Params.Add(ECWeb.Options._TEMPL_NODENAME, GetWRTNodeNameWithPath(tAlt, fCanBePathInteractive))
                                    Params.Add(ECWeb.Options._TEMPL_EVALCOUNT, StringFuncs.JS_SafeHTML(tAlt.Children.Count))

                                    ' D2408 ===
                                    If App.isRiskEnabled Then

                                        If isImpact Then
                                            sRes = Convert.ToString((If(tParentNode.ParentNode() Is Nothing, "lblEvaluationStepGoalImpact", "lblEvaluationStepImpact")))
                                        Else
                                            sRes = Convert.ToString((If(tParentNode.ParentNode() Is Nothing, "lblEvaluationStepGoalRisk", "lblEvaluationStepRisk")))
                                        End If
                                    Else
                                        sRes = "lblEvaluationStep"
                                    End If

                                                                    ' D2408 ==

                                Case ECMeasureType.mtDirect
                                    ' D2361 + D2379 + D2830
                                    ' D2540 ===
                                    Dim tDirect As clsDirectMeasureData = CType(data.Judgment, clsDirectMeasureData)
                                    Dim tH As clsHierarchy = DirectCast(Nothing, clsHierarchy)

                                    If data.Node.IsTerminalNode Then
                                        tH = App.ActiveProject.HierarchyAlternatives
                                    Else
                                        tH = App.ActiveProject.HierarchyObjectives
                                    End If

                                    Dim DirectChild As clsNode = tH.GetNodeByID(tDirect.NodeID)
                                    Params.Add(ECWeb.Options._TEMPL_NODE_A, GetWRTNodeNameWithPath(data.Node, fCanBePathInteractive))

                                    ' D2830
                                    ' D2540 ==
                                    If App.isRiskEnabled Then

                                        If isImpact Then
                                            ' D2398
                                            sRes = Convert.ToString(If(data.Node.Level = 0, "task_DirectDataImpactNoObj", "task_DirectDataImpact"))
                                        Else
                                            sRes = Convert.ToString(If(data.Node.Level = 0, "task_DirectDataRiskNoObj", "task_DirectDataRisk"))
                                        End If

                                        ' D2540
                                        Params.Add(ECWeb.Options._TEMPL_NODETYPE, Convert.ToString(If(data.Node.IsTerminalNode, ECWeb.Options._TEMPL_ALTERNATIVE, ECWeb.Options._TEMPL_OBJECTIVE)))
                                    Else
                                        sRes = "task_DirectData"
                                    End If

                                    Params.Add(ECWeb.Options._TEMPL_NODENAME, GetWRTNodeNameWithPath(tH.GetNodeByID(tDirect.NodeID), fCanBePathInteractive))
                                    Params.Add(ECWeb.Options._TEMPL_EVALCOUNT, StringFuncs.JS_SafeHTML(data.Node.Children.Count))
                                Case ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtCustomUtilityCurve, ECMeasureType.mtRegularUtilityCurve
                                    Dim tParentNode2 As clsNode = CType(data.Node.Hierarchy.GetNodeByID(CType(data.Judgment, clsUtilityCurveMeasureData).ParentNodeID), clsNode)
                                    Params.Add(ECWeb.Options._TEMPL_NODE_A, GetWRTNodeNameWithPath(tParentNode2, fCanBePathInteractive))
                                    ' D2830
                                    Dim tAlt2 As clsNode = DirectCast(Nothing, clsNode)

                                    If tParentNode2.IsTerminalNode Then
                                        tAlt2 = data.Node.Hierarchy.ProjectManager.AltsHierarchy(data.Node.Hierarchy.ProjectManager.ActiveAltsHierarchy).GetNodeByID(CType(data.Judgment, clsUtilityCurveMeasureData).NodeID)
                                    Else
                                        tAlt2 = data.Node.Hierarchy.GetNodeByID(CType(data.Judgment, clsUtilityCurveMeasureData).NodeID)
                                    End If

                                    Params.Add(ECWeb.Options._TEMPL_NODENAME, GetWRTNodeNameWithPath(tAlt2, fCanBePathInteractive))

                                    Select Case data.MeasurementType
                                        Case ECMeasureType.mtAdvancedUtilityCurve
                                            sRes = "task_AdvancedUtilityCurve"
                                        Case Else

                                            ' D2405 ===
                                            If App.isRiskEnabled Then

                                                If isImpact Then
                                                    sRes = Convert.ToString((If(tParentNode2.ParentNode() Is Nothing, "lblEvaluationUCGoalImpact", "lblEvaluationUCImpact")))
                                                Else
                                                    sRes = Convert.ToString((If(tParentNode2.ParentNode() Is Nothing, "lblEvaluationUCGoalRisk", "lblEvaluationUCRisk")))
                                                End If
                                            Else
                                                sRes = "lblEvaluationUC"
                                            End If
                                            ' D2405 ==
                                    End Select
                            End Select

                            tClusterNode = data.Node
                            ' D2364
                        End If
                    End If

                Case ActionType.atNonPWAllChildren
                    Dim data2 As clsAllChildrenEvaluationActionData = CType(Action.ActionData, clsAllChildrenEvaluationActionData)

                    If data2 IsNot Nothing AndAlso data2.ParentNode IsNot Nothing Then

                        Select Case data2.MeasurementType
                            Case ECMeasureType.mtRatings

                                If App.isRiskEnabled Then

                                    If isImpact Then
                                        sRes = Convert.ToString((If(data2.ParentNode.IsAlternative, "task_MultiRatings_AllCovObjImpact", (If(data2.ParentNode.ParentNode() Is Nothing, "task_MultiRatings_AllAltsGoalImpact", "task_MultiRatings_AllAltsImpact")))))
                                    Else
                                        sRes = Convert.ToString((If(data2.ParentNode Is Nothing OrElse data2.ParentNode.ParentNode() Is Nothing, "lblEvaluationMultiDirectDataLikelihood", If(data2.ParentNode.IsTerminalNode, "task_MultiRatings_AllAltsRisk", If(data2.ParentNode.RiskNodeType = ECTypes.RiskNodeType.ntCategory, "task_MultiRatings_AllObjRisk_Cat", "task_MultiRatings_AllObjRisk")))))
                                        ' D2318 + D2319 + D2964
                                    End If

                                    If Hierarchy IsNot Nothing AndAlso Hierarchy.Nodes.Count = 1 Then sRes = "task_MultiRatings_AllAlts_NoObj"
                                Else
                                    sRes = "task_MultiRatings_AllAlts"
                                End If

                            Case ECMeasureType.mtDirect

                                If App.isRiskEnabled Then

                                    If data2.ParentNode.IsTerminalNode Then

                                        ' D2354 ===
                                        If isImpact Then
                                            ' D2399
                                            sRes = Convert.ToString(If(data2.ParentNode.Level = 0, "lblEvaluationMultiDirectDataAltsGoalRisk", "lblEvaluationMultiDirectDataAltsRisk"))
                                        Else
                                            sRes = Convert.ToString(If(data2.ParentNode.Level = 0, "lblEvaluationMultiDirectDataAltsGoalLikelihood", "lblEvaluationMultiDirectDataAltsLikelihood"))
                                            ' D2354 ==
                                        End If
                                    Else

                                        If Not isImpact Then
                                            sRes = Convert.ToString(If(data2.ParentNode.Level > 0, "lblEvaluationMultiDirectDataLevelsLikelihood", "lblEvaluationMultiDirectDataLikelihood"))
                                        Else
                                            sRes = Convert.ToString((If(data2.ParentNode.ParentNode() Is Nothing, "lblEvaluationMultiDirectDataGoalRisk", "lblEvaluationMultiDirectDataRiskObj")))
                                        End If
                                    End If
                                Else

                                    If data2.ParentNode.IsTerminalNode Then
                                        sRes = "lblEvaluationMultiDirectDataAlts"
                                    Else
                                        sRes = "lblEvaluationMultiDirectData"
                                    End If
                                End If
                        End Select

                        Params.Add(ECWeb.Options._TEMPL_NODENAME, GetWRTNodeNameWithPath(data2.ParentNode, fCanBePathInteractive AndAlso Not (data2.ParentNode.RiskNodeType = ECTypes.RiskNodeType.ntCategory)))


                        ' D2830 + D2964 
                        Params.Add(ECWeb.Options._TEMPL_EVALCOUNT, Convert.ToString(data2.Children.Count))
                        tClusterNode = data2.ParentNode
                        ' D2364
                    End If

                Case ActionType.atNonPWAllCovObjs
                    Dim data3 As clsAllCoveringObjectivesEvaluationActionData = CType(Action.ActionData, clsAllCoveringObjectivesEvaluationActionData)

                    'Cv2 'C0464

                    If data3 IsNot Nothing Then

                        Select Case data3.MeasurementType
                            Case ECMeasureType.mtRatings

                                If App.isRiskEnabled Then

                                    If isImpact Then
                                        sRes = Convert.ToString((If(App.ActiveProject.HierarchyObjectives.GetMaxLevel() < 1, "task_MultiRatings_AllCovObjGoalImpact", "task_MultiRatings_AllCovObjImpact")))
                                    Else
                                        sRes = Convert.ToString((If(App.ActiveProject.HierarchyObjectives.GetMaxLevel() < 1, "task_MultiRatings_AllCovObjGoal", "task_MultiRatings_AllCovObj")))
                                        'If Hierarchy IsNot Nothing AndAlso Hierarchy.Nodes.Count = 1 Then sRes = "task_MultiRatings_AllAlts_NoObj" ' - D2429
                                    End If
                                Else
                                    sRes = "task_MultiRatings_AllCovObj"
                                End If

                            Case ECMeasureType.mtDirect

                                If App.isRiskEnabled Then

                                    ' D2360 ===
                                    If data3.Alternative.IsTerminalNode Then

                                        If Hierarchy.Nodes.Count <= 1 Then
                                            sRes = "lblEvaluationMultiDirectAltWRTCovRiskNoObj"
                                        Else
                                            sRes = "lblEvaluationMultiDirectAltWRTCovRisk"
                                        End If
                                    Else

                                        If Hierarchy.Nodes.Count <= 1 Then
                                            sRes = "lblEvaluationMultiDirectDataRiskNoObj"
                                        Else
                                            sRes = "lblEvaluationMultiDirectDataRisk"
                                        End If
                                        ' D2360 ==
                                    End If
                                Else
                                    sRes = Convert.ToString(If(data3.Alternative.IsTerminalNode, "lblEvaluationMultiDirectAltWRTCov", "lblEvaluationMultiDirectData"))
                                End If
                        End Select

                        Params.Add(ECWeb.Options._TEMPL_NODENAME, GetWRTNodeNameWithPath(data3.Alternative, fCanBePathInteractive))
                        ' D2830
                        Params.Add(ECWeb.Options._TEMPL_EVALCOUNT, Convert.ToString(data3.CoveringObjectives.Count))
                        tClusterNode = data3.Alternative
                        ' D2364
                    End If


                    ' D3250 ===
                Case ActionType.atAllEventsWithNoSource
                    Dim data4 As clsAllEventsWithNoSourceEvaluationActionData = CType(Action.ActionData, clsAllEventsWithNoSourceEvaluationActionData)

                    If data4 IsNot Nothing Then

                        Select Case data4.MeasurementType
                            Case ECMeasureType.mtRatings, ECMeasureType.mtDirect
                                sRes = "lblEvaluationNoSources"
                                tClusterNode = Nothing
                        End Select
                    End If

                                    ' D3250 ==

                Case ActionType.atAllPairwise, ActionType.atAllPairwiseOutcomes

                    If TypeOf Action.ActionData Is clsAllPairwiseEvaluationActionData Then
                        Dim Data5 As clsAllPairwiseEvaluationActionData = CType(Action.ActionData, clsAllPairwiseEvaluationActionData)
                        Dim fIsPWOutcomes2 As Boolean = Action.ActionType = ActionType.atAllPairwiseOutcomes
                        Params.Add(ECWeb.Options._TEMPL_NODENAME, GetWRTNodeNameWithPath(Data5.ParentNode, fCanBePathInteractive))
                        ' D2830
                        Params.Add(ECWeb.Options._TEMPL_EVALCOUNT, Convert.ToString(Data5.Judgments.Count))

                        If fIsPWOutcomes2 Then
                            If Action.ParentNode IsNot Nothing Then Params.Add(ECWeb.Options._TEMPL_JUSTNODE, StringFuncs.JS_SafeHTML(Action.ParentNode.NodeName))

                            If Action.ParentNode IsNot Nothing AndAlso Action.ParentNode.IsAlternative Then
                                ' D2351
                                sRes = Convert.ToString((If(Data5.ParentNode IsNot Nothing AndAlso Data5.ParentNode.ParentNode() Is Nothing, "task_MultiPairwise_Alternatives_PW{0}_Goal", "task_MultiPairwise_Alternatives_PW{0}")))
                            Else
                                sRes = Convert.ToString((If(Data5.ParentNode IsNot Nothing AndAlso Data5.ParentNode.ParentNode() Is Nothing, "task_MultiPairwise_Hierarchy_PW{0}", "task_MultiPairwise_Objectives_PW{0}")))
                            End If

                            Dim tRS As clsRatingScale = CType(Action.PWONode.MeasurementScale, clsRatingScale)
                            Dim sName As String = "Outcomes"
                            If tRS.IsPWofPercentages Then sName = "Percentages"
                            If tRS.IsExpectedValues Then sName = "ExpectedValues"
                            sRes = String.Format(sRes, sName)
                        Else

                            If Data5.ParentNode IsNot Nothing AndAlso Data5.ParentNode.IsTerminalNode Then
                                sRes = Convert.ToString((If(Data5.ParentNode.ParentNode() Is Nothing, "task_MultiPairwise_AlternativesGoal", If(App.isRiskEnabled AndAlso Not isImpact, "task_MultiPairwise_AlternativesLikelihood", "task_MultiPairwise_Alternatives"))))
                            Else
                                sRes = Convert.ToString((If(tExtraParam IsNot Nothing, If(TypeOf tExtraParam Is clsStepFunction, "task_MultiPairwise_Objectives_Intensities_SF", "task_MultiPairwise_Objectives_Intensities"), (If(Data5.ParentNode.ParentNode() Is Nothing, "task_MultiPairwise_ObjectivesGoal", If(App.isRiskEnabled AndAlso Not isImpact, "task_MultiPairwise_ObjectivesLikelihood", "task_MultiPairwise_Objectives"))))))
                            End If
                        End If

                        tClusterNode = Data5.ParentNode
                    End If

                Case ActionType.atShowLocalResults
                    Dim Data6 As clsShowLocalResultsActionData = CType(Action.ActionData, clsShowLocalResultsActionData)
                    Params.Add(ECWeb.Options._TEMPL_NODENAME, GetWRTNodeNameWithPath(Data6.ParentNode, fCanBePathInteractive))
                    Dim Test = Data6.ParentNode.NodeGuidID
                    Dim fIsPWOutcomes3 As Boolean = Data6.PWOutcomesNode IsNot Nothing AndAlso Data6.ParentNode.MeasureType() = ECMeasureType.mtPWOutcomes
                    If fIsPWOutcomes3 Then Params.Add(ECWeb.Options._TEMPL_JUSTNODE, GetWRTNodeNameWithPath(Data6.PWOutcomesNode, fCanBePathInteractive))

                    ' D2830
                    If tExtraParam IsNot Nothing Then
                        sRes = "task_LocalResultsObjectivesIntensities"
                    Else

                        If App.isRiskEnabled Then

                            If Data6.ParentNode.IsTerminalNode Then
                                sRes = Convert.ToString(If(Not isImpact, "task_LocalResultsAlternativesRisk", "task_LocalResultsAlternativesImpact"))
                            Else

                                ' D2723 ==
                                If Not isImpact AndAlso Data6.ParentNode.RiskNodeType = ECTypes.RiskNodeType.ntCategory Then
                                    sRes = "task_LocalResultsObjectives_Category"
                                Else

                                    ' D2723 ==
                                    If Not isImpact Then
                                        sRes = Convert.ToString((If(Data6.ParentNode.ParentNode() Is Nothing, "task_LocalResultsObjectivesGoal", "task_LocalResultsObjectivesRisk")))
                                    Else
                                        sRes = Convert.ToString((If(Data6.ParentNode.ParentNode() Is Nothing, "task_LocalResultsObjectivesGoalImpact", "task_LocalResultsObjectivesImpact")))
                                    End If
                                End If
                            End If
                        Else
                            sRes = Convert.ToString(If(Data6.ParentNode.IsTerminalNode, "task_LocalResultsAlternatives", "task_LocalResultsObjectives"))
                        End If
                    End If

                    ' D2364
                    tClusterNode = Data6.ParentNode
                Case ActionType.atShowGlobalResults

                    If App.isRiskEnabled Then
                        sRes = Convert.ToString(If(isImpact, "task_GlobalResultsImpact", "task_GlobalResultsRisk"))
                    Else
                        sRes = "task_GlobalResults"
                    End If

                    tClusterNode = Hierarchy.Nodes(0)
                    ' D2364
                    ' D2364 + D2830
                    Params.Add(ECWeb.Options._TEMPL_NODENAME, GetWRTNodeNameWithPath(tClusterNode, fCanBePathInteractive))
                Case ActionType.atSensitivityAnalysis
                    Dim Data7 As clsSensitivityAnalysisActionData = CType(Action.ActionData, clsSensitivityAnalysisActionData)

                    Select Case Data7.SAType
                        Case SAType.satDynamic
                            ' D2527
                            sRes = Convert.ToString(If(App.isRiskEnabled, If(Not isImpact, "task_DynamicSARisk", "task_DynamicSAImpact"), "task_DynamicSA"))
                        Case SAType.satGradient
                            sRes = Convert.ToString(If(App.isRiskEnabled, If(Not isImpact, "task_GradientSARisk", "task_GradientSAImpact"), "task_GradientSA"))
                        Case SAType.satPerformance
                            ' D2527
                            sRes = Convert.ToString(If(App.isRiskEnabled, If(Not isImpact, "task_PerformanceSARisk", "task_PerformanceSAImpact"), "task_PerformanceSA"))
                    End Select

                    ' D2364
                    tClusterNode = Action.ParentNode
            End Select
        End If

        ClusterPhrase = ""
        ' D2364 ===
        Dim sDefTask As String = ""

        If fGetResString AndAlso Not String.IsNullOrEmpty(sRes) Then
            sDefTask = ResString(sRes, False, False)
        Else
            sDefTask = sRes
        End If

        ' D2503 + D2596

        ' D2751 ===
        Dim isMulti As Boolean = False
        Dim tAGuid As Guid = Guid.Empty

        Select Case Action.ActionType
            Case ActionType.atAllPairwise, ActionType.atAllPairwiseOutcomes, ActionType.atNonPWAllChildren, ActionType.atNonPWAllCovObjs
                isMulti = True
            Case ActionType.atShowLocalResults
                isMulti = True

                If tClusterNode IsNot Nothing Then
                    tAGuid = tClusterNode.NodeGuidID
                End If

            Case ActionType.atShowGlobalResults, ActionType.atSensitivityAnalysis

                If tClusterNode IsNot Nothing Then
                    tAGuid = tClusterNode.NodeGuidID
                End If
        End Select


        ' D2751 ==
        'var sss = App.ActiveProject.PipeParameters.JudgementPromt;
        'var sssd = App.ActiveProject.PipeParameters.JudgementPromtMulti;
        'var sssa = App.ActiveProject.PipeParameters.JudgementAltsPromt;
        'var ssss = App.ActiveProject.PipeParameters.JudgementAltsPromtMulti;
        'var sdsds = App.ActiveProject.PipeParameters.NameAlternatives;
        'var sdsds3 = App.ActiveProject.PipeParameters.ProjectPurpose;
        'var sdsds33 = App.ActiveProject.PipeParameters.TeamTimeInvitationSubject;
        'var sdsds35 = App.ActiveProject.PipeParameters.PipeMessages;
        'var sdsds38 = App.ActiveProject.PipeParameters.ProjectPurpose;
        'var sdsdss = tClusterNode.NodeGuidID;

        If tClusterNode IsNot Nothing AndAlso Not fIgnoreClusterPhrase Then ClusterPhrase = App.ActiveProject.ProjectManager.PipeBuilder.GetClusterPhraseForNode(tClusterNode, isMulti, tAGuid, False)
        ' D2692 + D2751
        If String.IsNullOrEmpty(ClusterPhrase) OrElse String.IsNullOrEmpty(StringFuncs.HTML2Text(ClusterPhrase)) Then ClusterPhrase = sDefTask
        ' D2729
        ClusterPhraseIsCustom = ClusterPhrase.Trim().ToLower() <> sDefTask.Trim().ToLower()
        If tClusterNode IsNot Nothing Then TaskNodeGUID = tClusterNode.NodeGuidID.ToString()
        TaskTemplates = Params


        ' D2929 ===
        ' D2964
        If tClusterNode Is Nothing OrElse tClusterNode IsNot Nothing AndAlso Not (tClusterNode.RiskNodeType = ECTypes.RiskNodeType.ntCategory) Then
            'If fCanBePathInteractive Then  ' D2972
            Dim sNodesTempls As String() = {ECWeb.Options._TEMPL_JUSTNODE, ECWeb.Options._TEMPL_NODENAME, ECWeb.Options._TEMPL_NODE_A, ECWeb.Options._TEMPL_NODE_B}
            Dim sNodeParams As Dictionary(Of String, String) = New Dictionary(Of String, String)()

            For Each sTpl As String In sNodesTempls

                If ClusterPhrase.Contains(sTpl) AndAlso Not ClusterPhrase.ToLower().Contains("'wrt_name'>" & sTpl.ToLower()) Then
                    sNodeParams.Add(sTpl, String.Format("<span class='node_name'>{0}</span>", sTpl))
                End If
            Next

            If sNodeParams.Count > 0 Then ClusterPhrase = StringFuncs.ParseStringTemplates(ClusterPhrase, sNodeParams)
            'End If
        End If

        ' D2929 ==

        Dim sTask As String = ""

        If fParseNodeNames AndAlso Params IsNot Nothing Then
            sTask = PrepareTask(StringFuncs.ParseStringTemplates(ClusterPhrase, Params), tExtraParam, fHasSubnodes, Params)

            For Each sKey As String In Params.Keys
                If Not TaskTemplates.ContainsKey(sKey) Then TaskTemplates.Add(sKey, Params(sKey))
            Next
        Else
            sTask = PrepareTask(ClusterPhrase, tExtraParam, fHasSubnodes)
        End If

        context.Session("sRes") = sRes
        context.Session("ClusterPhrase") = ClusterPhrase
        Return sTask
    End Function

    Public Shared Function GetWRTNodeNameWithPath(ByVal tNode As clsNode, ByVal CanBeInteractive As Boolean) As String
        Dim context As HttpContext = HttpContext.Current
        Dim App = CType(context.Session("App"), clsComparionCore)
        Dim sName As String = ""

        If tNode IsNot Nothing Then
            sName = StringFuncs.JS_SafeHTML(tNode.NodeName)
            Dim sDivider As String = StringFuncs.JS_SafeHTML(App.ResString("lblObjectivePathDivider"))
            Dim sPath As String = ""

            While tNode.ParentNode() IsNot Nothing

                If tNode.ParentNode().ParentNode() IsNot Nothing Then
                    sPath = StringFuncs.JS_SafeHTML(tNode.ParentNode().NodeName) & sDivider & sPath
                End If

                tNode = tNode.ParentNode()
            End While

            Dim objectivePath As PipeParameters.ecShowObjectivePath = App.ActiveProject.PipeParameters.ShowFullObjectivePath

            If objectivePath = PipeParameters.ecShowObjectivePath.CollapsePath AndAlso CanBeInteractive AndAlso Not String.IsNullOrEmpty(sPath) Then
                If sOldWRTPath Is Nothing Then sOldWRTPath = get_SessVar(SESS_WRT_PATH)
                'Dim fCanSee As Boolean = App.ActiveProject.PipeParameters.ShowFullObjectivePath OrElse Not String.Equals(sPath, sOldWRTPath)
                Dim fCanSee As Boolean = Not String.Equals(sPath, sOldWRTPath)
                sName = String.Format("<span id='wrtCollapsePath' style='cursor:pointer;' onmouseover =""this.title='{3}';"" onclick='ToggleWRTPath();' class='wrt_link'><span id='wrt_path' class='wrt_path'{2}>{0}</span>{1}</span>", sPath, sName, If(fCanSee, "", " style='display:none;'"), StringFuncs.JS_SafeString((sPath & sName).Replace("""", "&#39;")))
            ElseIf objectivePath = PipeParameters.ecShowObjectivePath.AlwaysShowFull Then
                sName = sPath & sName
            End If
        End If

        Return sName
    End Function

End Class