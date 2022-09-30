Option Strict On
Partial Class GetInfodocPage
    Inherits clsComparionCorePage

    Private _ObjectType As reObjectType = reObjectType.Unspecified
    Private _Prj As clsProject = Nothing    ' D0787

    Public Sub New()
        MyBase.New(_PGID_EVALUATE_INFODOC)  ' D0787
    End Sub

    ' D0787 ===
    Private ReadOnly Property Project() As clsProject
        Get
            If _Prj Is Nothing Then
                Dim prjID As Integer = CheckVar(_PARAM_PROJECT, -1)
                If prjID <= 0 OrElse prjID = App.ProjectID Then prjID = App.ProjectID ' D2532
                _Prj = clsProject.ProjectByID(prjID, App.ActiveProjectsList)
                If _Prj Is Nothing Then _Prj = App.DBProjectByID(prjID)
                If _Prj Is Nothing AndAlso App.isAntiguaAuthorized Then _Prj = AnonAntiguaProject()  ' D9402
                ' D2532 ===
                If _Prj IsNot Nothing AndAlso App.isRiskEnabled Then
                    Dim sPasscode As String = EcSanitizer.GetSafeHtmlFragment(CheckVar(_PARAM_PASSCODE, "")).Trim.ToLower ' Anti-XSS
                    If _Prj.PasscodeImpact.ToLower = sPasscode Then
                        _Prj.ProjectManager.ActiveHierarchy = ECHierarchyID.hidImpact ' D2898
                        _Prj.ProjectManager.PipeParameters.CurrentParameterSet = _Prj.ProjectManager.PipeParameters.ImpactParameterSet  ' D3279
                    End If
                    If _Prj.PasscodeLikelihood.ToLower = sPasscode Then
                        _Prj.ProjectManager.ActiveHierarchy = ECHierarchyID.hidLikelihood
                        _Prj.ProjectManager.PipeParameters.CurrentParameterSet = _Prj.ProjectManager.PipeParameters.DefaultParameterSet ' D3279
                    End If
                End If
                ' D2532 ==
            End If
            Return _Prj
        End Get
    End Property
    ' D0787 ==

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        StorePageID = False ' D1004
        If Not App.isAuthorized AndAlso Not App.isAntiguaAuthorized Then FetchAccess()  ' D9402
        If Project Is Nothing Then FetchAccess(_PGID_PROJECTSLIST)  ' D9402
        '' D1301 ===
        'Dim sCheck As String = CheckVar("check", "")
        'If sCheck <> "" Then
        '    Dim sPath As String = Server.MapPath(sCheck)
        '    If MyComputer.FileSystem.FileExists(sPath) Then Response.Redirect(sCheck, True)
        'End If
        '' D1301 ==

        ' D0787 ====
        Dim sContent As String = ""
        Dim otID As Integer = 0
        If Integer.TryParse(CheckVar("type", otID.ToString), otID) Then _ObjectType = CType(otID, reObjectType)

        ' D3820 ===
        Dim sEvalURL As String = App.Options.EvalSiteURL
        Dim fIgnoreEvalSite As Boolean = CheckVar("ignoreval", False)
        If fIgnoreEvalSite Then App.Options.EvalSiteURL = ""
        ' D3820 ==

        Dim sEmpty As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("empty", ""))    ' D3699 + Anti-XSS

        If Project Is Nothing AndAlso _ObjectType <> reObjectType.SurveyQuestion Then   ' D1277
            sContent = "Wrong parameters"
        Else
            ' D0787 ==

            Dim sName As String = ""

            Select Case _ObjectType

                Case reObjectType.Node, reObjectType.Alternative
                    Dim tNode As clsNode = GetNode(False)   ' D1004
                    'If _ObjectType = reObjectType.Node Then sName = "Objective #{0}" Else sName = "Alternative #{0}" ' D0780 -D1063
                    If Not tNode Is Nothing Then
                        sName = String.Format("«{0}»", tNode.NodeName)  ' D1063
                        If Not tNode Is Nothing Then sContent = Infodoc_Unpack(Project.ID, Project.ProjectManager.ActiveHierarchy, _ObjectType, tNode.NodeID.ToString, tNode.InfoDoc, True, True, -1) ' D0787 + D1669
                    End If

                    ' D1004 ===
                Case reObjectType.AltWRTNode
                    Dim tNode As clsNode = GetNode(False)
                    Dim tParentNode As clsNode = GetNode(True)
                    If Not tNode Is Nothing AndAlso tParentNode IsNot Nothing Then
                        Dim sInfodoc As String = Project.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(tNode.NodeGuidID, tParentNode.NodeGuidID)    ' D1128
                        sContent = Infodoc_Unpack(Project.ID, Project.ProjectManager.ActiveHierarchy, _ObjectType, tNode.NodeID.ToString, sInfodoc, True, True, tParentNode.NodeID) ' D1669
                        sName = String.Format(ResString(CStr(IIf(Project.IsRisk AndAlso Not Project.isImpact, "lblShowDescriptionWRTLikelihood", "lblShowDescriptionWRT"))), tNode.NodeName, tParentNode.NodeName) ' D1063 + D3031 + D9402
                    End If
                    ' D1004 ==

                Case reObjectType.PipeMessage
                    ' D4293 ===
                    Dim tType As PipeMessageKind = PipeMessageKind.pmkText
                    If CheckVar("risk", False) Then tType = PipeMessageKind.pmlTextRiskControls
                    Dim sPostfix As String = If(tType = PipeMessageKind.pmlTextRiskControls, "_risk", "")
                    ' D4293 ==
                    Select Case CheckVar(_PARAM_ID, "").ToLower
                        Case "welcome"
                            sContent = Project.PipeParameters.PipeMessages.GetWelcomeText(tType, Project.ProjectManager.ActiveHierarchy, Project.ProjectManager.ActiveAltsHierarchy) 'C0139 + D0787 + D4293
                            If sContent = "" Then sContent = ParseURLAndPathTemplates(File_GetContent(GetWelcomeThankYouIncFile(False, Project.isImpact, Project.PipeParameters.ProjectType = ProjectType.ptOpportunities, tType = PipeMessageKind.pmlTextRiskControls))) Else sContent = Infodoc_Unpack(Project.ID, Project.ProjectManager.ActiveHierarchy, reObjectType.PipeMessage, "welcome" + sPostfix, sContent, True, True, -1) ' D0787 + D0800 + D1128 + D1550 + D1669 + D2325 + D3326 + D4293
                            sName = "«Welcome» screen"  ' D0780
                        Case "thankyou"
                            sContent = Project.PipeParameters.PipeMessages.GetThankYouText(tType, Project.ProjectManager.ActiveHierarchy, Project.ProjectManager.ActiveAltsHierarchy) 'C0139 + D0787
                            If sContent = "" Then sContent = ParseURLAndPathTemplates(File_GetContent(GetWelcomeThankYouIncFile(True, Project.isImpact, Project.PipeParameters.ProjectType = ProjectType.ptOpportunities, tType = PipeMessageKind.pmlTextRiskControls))) Else sContent = Infodoc_Unpack(Project.ID, Project.ProjectManager.ActiveHierarchy, reObjectType.PipeMessage, "thankyou" + sPostfix, sContent, True, True, -1) ' D0787 + D0800 + D1128 + D1550 + D1669 + D2325 + D3326 + D4293
                            sName = "«Thank you» screen"  ' D0780
                    End Select

                    ' D2064 ===
                Case reObjectType.Description
                    If App.CanUserModifyProject(App.ActiveUser.UserID, Project.ID, App.ActiveUserWorkgroup, clsWorkspace.WorkspaceByUserIDAndProjectID(App.ActiveUser.UserID, Project.ID, App.Workspaces)) OrElse App.CanUserDoProjectAction(ecActionType.at_mlViewModel, App.ActiveUser.UserID, Project.ID, App.ActiveUserWorkgroup, App.ActiveWorkgroup) Then
                        sContent = Project.ProjectManager.ProjectDescription    ' D2083
                        If sContent <> "" Then sContent = Infodoc_Unpack(Project.ID, Project.ProjectManager.ActiveHierarchy, reObjectType.Description, "", sContent, True, True, -1) ' D2083
                        If sContent = "" Then sContent = SafeFormString(Project.Comment).Replace(vbLf, "<br>")    ' D6317
                    End If
                    ' D2064 ==

                    ' D1358 ===

                Case reObjectType.ExtraInfo
                    Dim sParam As String = EcSanitizer.GetSafeHtmlFragment(CheckVar(_PARAM_ID, "")).Trim.ToLower    ' Anti-XSS
                    If sParam <> "" Then
                        Select Case sParam
                            Case _PARAM_INVITATION_TT.ToLower
                                sContent = Project.PipeParameters.TeamTimeInvitationBody2    ' D3371
                                'If sContent = "" Then sContent = ParseAllTemplates(App.ResString("bodySynchronousInvitations"), App.ActiveUser, Project, False, True).Replace(vbCrLf, "<br>") ' D1369
                                If sContent = "" Then sContent = ParseURLAndPathTemplates(App.ResString("bodySynchronousInvitations")).Replace(vbCrLf, "<br>") ' D1369 + D1550
                                sName = ResString("lblInvitationTeamTimeEdit")  ' D1550
                            Case _PARAM_INVITATION_EVAL.ToLower
                                sContent = Project.PipeParameters.EvaluateInvitationBody2   ' D3371
                                'If sContent = "" Then sContent = ParseAllTemplates(App.ResString("bodyDecisionInvitations"), App.ActiveUser, Project, False, True).Replace(vbCrLf, "<br>")
                                If sContent = "" Then sContent = ParseURLAndPathTemplates(App.ResString(CStr(IIf(App.isRiskEnabled, "bodyDecisionInvitationsRisk", "bodyDecisionInvitations")))).Replace(vbCrLf, "<br>") ' D1550 + D2467
                                ' D4647 ===
                                sName = ResString("lblInvitationAnytimeEdit")   ' D1550
                            Case _PARAM_INVITATION_CUSTOM.ToLower
                                sContent = ParseAllTemplates(Project.ProjectManager.Parameters.InvitationCustomText, Nothing, Project)  ' D4650
                                sName = ResString("lblInvitationCustomText")
                                If sEmpty = "" Then sEmpty = ResString("msgNoCustomSingup") ' D4873
                                ' D4647 ==
                        End Select
                        If sContent <> "" AndAlso isMHT(sContent) Then sContent = Infodoc_Unpack(Project.ID, Project.ProjectManager.ActiveHierarchy, _ObjectType, sParam, sContent, True, True, -1) ' D1669
                    End If
                    ' D1358 ==

                ' D9402 ===
                Case reObjectType.AntiguaInfodoc
                    Dim tNode As clsVisualNode = GetVirtualNode()
                    If tNode IsNot Nothing Then
                        sContent = Project.ProjectManager.AntiguaInfoDocs.GetAntiguaInfoDoc(tNode.GuidID)
                        If sContent Is Nothing Then sContent = ""
                        sContent = Infodoc_Unpack(Project.ID, Project.ProjectManager.ActiveHierarchy, _ObjectType, tNode.GuidID.ToString, sContent, True, True, -1)
                    End If
                ' D9402 ==

                    ' D1277 ===
                Case reObjectType.SurveyQuestion
                    'TODO: unpack survey infodoc
                    ' D1277 ==

                    ' D2250 ===
                Case reObjectType.MeasureScale
                    Dim MType As ECMeasureType = ECMeasureType.mtNone
                    Dim Scale As clsMeasurementScale = GetCurrentScale(MType)
                    If Scale IsNot Nothing Then
                        sContent = Scale.Comment
                        If sContent <> "" AndAlso isMHT(sContent) Then sContent = Infodoc_Unpack(Project.ID, Project.ProjectManager.ActiveHierarchy, _ObjectType, Scale.GuidID.ToString, Scale.Comment, True, True, -1) ' D2252
                    End If
                    ' D2250 ==

                    ' D3695 ===
                Case reObjectType.QuickHelp
                    Dim tEvalType As ecEvaluationStepType = CType(CheckVar("qh", CInt(ecEvaluationStepType.Other)), ecEvaluationStepType)
                    If tEvalType <> ecEvaluationStepType.Other Then
                        Dim tNode As clsNode = GetNode(False)
                        Dim ObjID As String = GetQuickHelpObjectID(tEvalType, tNode)
                        Dim tStepID As Integer = CheckVar("step", 0)   ' D3699
                        Dim fShow As Boolean = False    ' D3741
                        Dim isCluster As Boolean = False    ' D4082
                        ' D6662 ===
                        Select Case tEvalType
                            Case ecEvaluationStepType.RiskResults, ecEvaluationStepType.HeatMap
                                sContent = Project.PipeParameters.PipeMessages.GetEvaluationQuickHelpForCustom(Project.ProjectManager, tEvalType, CheckVar("id", -1), fShow)
                            Case Else
                                sContent = Project.PipeParameters.PipeMessages.GetEvaluationQuickHelpText(Project.ProjectManager, tStepID, isCluster, fShow)  ' D3699 + D3741 + D4079 + D4081 + D4082
                        End Select
                        ' D6662 ==
                        If sContent <> "" Then sContent = Infodoc_Unpack(Project.ID, Project.ProjectManager.ActiveHierarchy, reObjectType.QuickHelp, ObjID, sContent, True, True, -1)
                        sEmpty = ResString("msgInfodocEmpty")   ' D3699
                        ' D3738 ===
                        If sContent <> "" Then
                            CheckShow_QuickHelp(sContent)
                            'sContent = PrepareTask(sContent)    ' D3778
                            If CheckVar("autoshow", fShow) Then ' D3741
                                Dim tIdx As Integer = sContent.ToLower.LastIndexOf("</body>")
                                If tIdx <= 0 Then tIdx = sContent.Length
                                Dim sCode As String = String.Format("<br/><form method='post' name='theForm' style='margin-top:1em;'>" +
                                                                    "<div style='position:absolute; display: inline-block; bottom: 0; right: 0;'>" +
                                                                    "<div id='divQHAutoShow' class='qh_option' style='border:1px solid #f5f5f5; border-radius: 3px; background:#ffffff; padding:4px; opacity: 0.85 !important; filter: alpha(opacity=85) !important; box-shadow: 3px 3px 3px 2px #e0e0e0; font-size: 12px; font-family: Arial, Helvetica, Verdana, sans-serif;'>" +
                                                                    "<label><input type='checkbox' name='cbQHAutoShow' value='1' {0} onclick='document.cookie = ""{2}="" + (this.checked ? 0 : 1) + "";path=/;"";'>{1}</label>" +
                                                                    "</div></div></form>",
                                                                    IIf(GetCookie(_COOKIE_QH_AUTOSHOW + App.ProjectID.ToString, "") <> "0", "", "checked"), ResString("lblQHAutoShow"), _COOKIE_QH_AUTOSHOW + App.ProjectID.ToString) ' D3740 + D3741 + D6708
                                'sCode += "<script type='text/javascript'> var addEvent = window.addEventListener ? function (elem, type, method) { elem.addEventListener(type, method, false); } : function (elem, type, method) { elem.attachEvent('on' + type, method); }; var opt = document.getElementById('initQHOption'); if((opt)) { addEvent(document.body, 'mouseover', function () { opt.style.display = ''; }); addEvent(document.body, 'mouseout', function () { opt.style.display = 'none'; }); opt.style.display = 'none'; } </script>"    ' D6672
                                sContent = sContent.Insert(tIdx, sCode)
                            End If
                        End If
                        ' D3738 ==
                        'If sContent = "" Then sContent = ParseURLAndPathTemplates(File_GetContent(GetIncFile(String.Format(_FILE_TEMPL_QUCIK_HELP, tEvalType.ToString)), "")) Else sContent = Infodoc_Unpack(Project.ID, Project.ProjectManager.ActiveHierarchy, reObjectType.QuickHelp, tEvalType.ToString, sContent, True, True, -1)
                    End If
                    ' D3695 ==

                    ' D4283 ===
                Case reObjectType.Attribute
                    Dim tGUID As Guid = Str2GUID(CheckVar("guid", ""))
                    If Not tGUID.Equals(Guid.Empty) Then
                        Dim tAttr As clsAttribute = Project.ProjectManager.Attributes.GetAttributeByID(tGUID)
                        If tAttr IsNot Nothing Then
                            sName = String.Format(ResString("lblInfodocAttr"), SafeFormString(tAttr.Name))
                            sContent = Project.ProjectManager.InfoDocs.GetCustomInfoDoc(tAttr.ID, Guid.Empty)
                            If Not String.IsNullOrEmpty(sContent) AndAlso isMHT(sContent) Then sContent = Infodoc_Unpack(Project.ID, Project.ProjectManager.ActiveHierarchy, _ObjectType, tAttr.ID.ToString, sContent, True, True, -1)
                        End If
                    End If

                Case reObjectType.AttributeValue
                    Dim tGUID As Guid = Str2GUID(CheckVar("guid", ""))
                    If Not tGUID.Equals(Guid.Empty) Then
                        Dim tAttr As clsAttributeEnumeration = Project.ProjectManager.Attributes.GetEnumByID(tGUID)
                        If tAttr IsNot Nothing Then
                            sName = String.Format(ResString("lblInfodocAttrVal"), SafeFormString(tAttr.Name))
                            sContent = Project.ProjectManager.InfoDocs.GetCustomInfoDoc(tAttr.ID, Guid.Empty)
                            If Not String.IsNullOrEmpty(sContent) AndAlso isMHT(sContent) Then sContent = Infodoc_Unpack(Project.ID, Project.ProjectManager.ActiveHierarchy, _ObjectType, tAttr.ID.ToString, sContent, True, True, -1)
                        End If
                    End If

                Case reObjectType.ResultGroup
                    Dim tID As Integer = CheckVar("id", Integer.MinValue)
                    If tID <> Integer.MinValue Then
                        Dim tGrp As clsCombinedGroup = Project.ProjectManager.CombinedGroups.GetGroupByCombinedID(tID)
                        If tGrp IsNot Nothing Then
                            sName = String.Format(ResString("lblInfodocResGroup"), SafeFormString(tGrp.Name))
                            Dim tGUID As New Guid(tID, CShort(reObjectType.ResultGroup), 0, 0, 0, 0, 0, 0, 0, 0, 0)
                            sContent = Project.ProjectManager.InfoDocs.GetCustomInfoDoc(tGUID, Guid.Empty)
                            If Not String.IsNullOrEmpty(sContent) AndAlso isMHT(sContent) Then sContent = Infodoc_Unpack(Project.ID, Project.ProjectManager.ActiveHierarchy, _ObjectType, tGrp.ID.ToString, sContent, True, True, -1)
                        End If
                    End If
                    ' D4283 ==

                    ' D4344 ===
                Case reObjectType.Control
                    Dim tGUID As Guid = Str2GUID(CheckVar("guid", ""))
                    If Not tGUID.Equals(Guid.Empty) Then
                        Dim tCtrl As clsControl = Project.ProjectManager.Controls.GetControlByID(tGUID)
                        If tCtrl IsNot Nothing Then
                            sContent = GetControlInfodoc(Project, tCtrl, False)    ' D4345
                            ' D4371 ===
                            Dim idx As Integer = sContent.ToLower.IndexOf("<body")
                            If idx >= 0 Then
                                idx = sContent.IndexOf(">", idx)
                                If idx > 0 Then
                                    Dim sExtra As String = String.Format("<h3 style='padding:4px; text-align:center; background:#f0f8ff; margin:2px;'>{0}</h3>", tCtrl.Name)
                                    sContent = sContent.Insert(idx + 1, sExtra)
                                End If
                            End If
                            ' D4371 ==
                        End If
                    End If
                    ' D4344 ==

                    ' D7011 ===
                Case reObjectType.DashboardInfodoc
                    Dim tDash As Integer = CheckVar("dash", Integer.MinValue)
                    If tDash <> Integer.MinValue AndAlso Project.ProjectManager.Reports.Reports.ContainsKey(tDash) Then
                        Dim D As clsReport = Project.ProjectManager.Reports.Reports(tDash)
                        Dim tID As Integer = CheckVar("id", Integer.MinValue)
                        If D.Items.ContainsKey(tID) Then
                            Dim Item = D.Items(tID)
                            sName = SafeFormString(Item.Name)
                            Dim tGUID As New Guid(tID, CShort(reObjectType.DashboardInfodoc), CShort(tDash), 0, 0, 0, 0, 0, 0, 0, 0)
                            sContent = Project.ProjectManager.InfoDocs.GetCustomInfoDoc(tGUID, Guid.Empty)
                            If Not String.IsNullOrEmpty(sContent) AndAlso isMHT(sContent) Then sContent = Infodoc_Unpack(Project.ID, Project.ProjectManager.ActiveHierarchy, _ObjectType, tID.ToString, sContent, True, True, tDash)
                            If sContent = "" Then sContent = ResString("templDashboardInformationEmpty")    ' D7391
                        End If
                    End If
                    ' D7011 ==

            End Select

            ' D1298 ===
            If sContent = "" Then
                sContent = sEmpty   ' D3699
                If sContent <> "" Then sContent = String.Format("<table width=100% height=99% border=0><tr><td valign=middle align=center style='font-size:9pt; color:#999999'>{0}</td></tr></table>", sContent)
            End If
            ' D1298 ==

            '' D2835 + D2841 ===
            'Dim sHint As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("title", "")).Trim ' Anti-XSS
            'If sHint <> "" Then
            '    sHint = sHint.Replace("'", "&#39;")
            '    If sContent.ToLower.IndexOf("</body>") > 0 Then
            '        sContent = sContent.Replace("<body", "<body title='" + sHint + "'")
            '    End If
            'End If
            '' D2835 + D2841 ==

            Dim isHTML As Boolean = sContent.IndexOf("</html>", StringComparison.InvariantCultureIgnoreCase) > 0    ' D4183
            'If sContent <> "" And sContent.IndexOf("</html>", StringComparison.InvariantCultureIgnoreCase) < 1 Then
            If sContent = "" OrElse Not isHTML Then ' D4183
                ' D1278 ===
                Dim sTitle = ""
                If Project IsNot Nothing Then sTitle = Project.ProjectName
                sTitle = String.Format("{0} [{1}] — {2}", sName, sTitle, ApplicationName)
                If sContent <> "" AndAlso _OPT_INFODOC_PARSE_PLAIN_LINKS Then sContent = ParseTextHyperlinks(sContent) ' D7453
                sContent = String.Format(_TEMPL_EMPTY_INFODOC, sContent, sTitle, ThemePath)  ' D0780 + D0787 + D1063 + D2841 + D4371 + D6504
                ' D1278 ==
            End If

            ' D6043 ===
            If App.isAuthorized Then
                Dim UID As Integer = App.ActiveUser.UserID
                Dim tUser As clsApplicationUser = Nothing
                If Integer.TryParse(CheckVar("user_id", ""), UID) AndAlso UID <> App.ActiveUser.UserID Then tUser = App.DBUserByID(UID)
                If tUser Is Nothing Then tUser = App.ActiveUser

                If CheckVar("preview", False) OrElse (tUser IsNot Nothing AndAlso tUser.CannotBeDeleted) Then
                    ' D6043 ==
                    ' D7444 ===
                    If isSSO_Only() Then
                        Dim sStartURL As String = ApplicationURL(False, False) + _URL_ROOT
                        Dim sPasscodeURL As String = If(Project Is Nothing, sStartURL, URLWithParams(sStartURL, _PARAM_PASSCODE + "="))
                        For Each sTpl As String In _TEMPL_LIST_HIDE_URLS
                            If sContent.ToLower.Contains(sTpl.ToLower) Then sContent = ParseTemplate(sContent, sTpl, sPasscodeURL + If(Project Is Nothing, "", If(sTpl = _TEMPL_URL_EVALUATE_IMPACT, Project.PasscodeImpact, Project.PasscodeLikelihood)))
                        Next
                    Else
                        ' D1713 ===
                        'sContent = sContent.Replace("<body", "<body class='infodoc_preview' ") ' D1555
                        Dim sReplace As String = ResString("lblPreviewURLTemplate")
                        For Each sTpl As String In _TEMPL_LIST_HIDE_URLS ' D7432
                            sContent = ParseTemplate(sContent, sTpl, sReplace)
                        Next
                        ' D1713 ==
                    End If
                    ' D7444 ==
                End If
                If CheckVar("parse", True) AndAlso App.isAuthorized Then
                    ' D1859 ===

                    If Not isHTML Then
                        For Each sTpl As String In _TEMPL_LIST_URLS
                            If Not isSSO() OrElse sTpl <> _TEMPL_URL_RESETPSW Then  ' D6552
                                sContent = sContent.Replace(sTpl, String.Format("<a href='{0}'>{0}</a>", sTpl))
                            End If
                        Next
                    End If
                    sContent = ParseAllTemplates(sContent, tUser, Project) ' D1554 + D1555 + D9402
                    ' D1859 ==
                End If
            End If

            'sContent = ParseVideoLinks(sContent, _ObjectType = reObjectType.QuickHelp, ApplicationURL(False, False), String.Format("<script src='https://www.youtube.com/iframe_api'></script><script> function onYouTubeIframeAPIReady() {{ var p = new YT.Player('iframe_youtube'); if ((p)) {{ p.addEventListener('onReady', function (event) {{ p.playVideo(); }}}} </script>", _URL_ROOT, Now.Ticks Mod 100000000))    ' D3697 + D4557 + D7135 + D7141 + D7552
            sContent = ParseVideoLinks(sContent, _ObjectType = reObjectType.QuickHelp, ApplicationURL(False, False))    ' D3697 + D4557 + D7135 + D7141 + D7552 + D7555

        End If

        If fIgnoreEvalSite Then App.Options.EvalSiteURL = sEvalURL ' D3820

        ' D0806 ===
        RawResponseStart()
        Response.ContentType = "text/html"
        ' D0806 ==
        Response.Write(sContent)
        RawResponseEnd()
    End Sub

    Private Function GetNode(fIsParentNode As Boolean) As clsNode   ' D1003
        Dim tNode As clsNode = Nothing

        If Project IsNot Nothing AndAlso App.isAuthorized Then   ' D1128

            Dim tObj As clsHierarchy = Project.HierarchyObjectives      ' D2292
            Dim TAlt As clsHierarchy = Project.HierarchyAlternatives    ' D2292

            ' D0701 ===
            Dim sNodeGUID As String = EcSanitizer.GetSafeHtmlFragment(CheckVar(CStr(IIf(fIsParentNode, "pguid", "guid")), ""))  ' D1003 + Anti-XSS
            Dim tmpObjectType As reObjectType = If(fIsParentNode, reObjectType.Node, _ObjectType)  ' D1003

            ' D2292 ===
            If tmpObjectType = reObjectType.Node Then
                Dim sRID As String = SessVar(SESSION_SCALE_ID)  ' D1965
                If sRID Is Nothing Then sRID = ""
                Dim sGUID As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("rid", sRID)).Trim.ToLower ' Anti-XSS
                If sGUID <> "" Then
                    For Each tRating As clsRatingScale In Project.ProjectManager.MeasureScales.RatingsScales
                        ' D4687 ===
                        If tRating.GuidID.ToString.ToLower = sGUID Then
                            Dim tGUID As Guid = Str2GUID(sNodeGUID)   ' D6704
                            Dim tRSH As clsHierarchy = Project.ProjectManager.GetHierarchyByID(tRating.GuidID)
                            If tRSH.GetNodeByID(tGUID) IsNot Nothing Then tObj = tRSH
                        End If
                        ' D4687 ==
                    Next
                End If
            End If
            ' D2292 ==

            If sNodeGUID <> "" AndAlso tObj IsNot Nothing Then ' D2292
                Try ' D1299
                    Dim tGUID As Guid = Str2GUID(sNodeGUID) ' D6704
                    Select Case tmpObjectType   ' D1003
                        Case reObjectType.Alternative, reObjectType.AltWRTNode, reObjectType.QuickHelp    ' D1004 + D3695
                            tNode = TAlt.GetNodeByID(tGUID)    ' D2292 + D4228
                            If tNode Is Nothing Then tNode = tObj.GetNodeByID(tGUID) ' D2412 + D4228
                        Case reObjectType.Node
                            ' D4228 ===
                            tNode = tObj.GetNodeByID(tGUID)    ' D2292
                            If tNode Is Nothing AndAlso App.isRiskEnabled Then
                                tNode = Project.ProjectManager.Hierarchy(CInt(IIf(Project.isImpact, ECHierarchyID.hidLikelihood, ECHierarchyID.hidImpact))).GetNodeByID(tGUID)
                            End If
                            ' D4228 ==
                    End Select
                Catch ex As Exception   ' D1299
                End Try
            End If

            If tNode Is Nothing AndAlso tObj IsNot Nothing Then    ' D2292
                ' D0701 ==
                Dim NodeID As Integer = 0
                If Integer.TryParse(CheckVar(If(fIsParentNode, "pid", _PARAM_ID), "0"), NodeID) Then ' D1003
                    Select Case tmpObjectType ' D0107 + D1003
                        Case reObjectType.Alternative, reObjectType.AltWRTNode  ' D1004
                            tNode = TAlt.GetNodeByID(NodeID)    ' D1128 + D2292
                            If tNode Is Nothing Then tNode = GetNodeByID(Project.HierarchyObjectives.Nodes, NodeID) ' D3473
                        Case reObjectType.Node
                            tNode = tObj.GetNodeByID(NodeID)    ' D1128 + D2292
                    End Select
                End If
            End If
        End If

        Return tNode
    End Function

    ' D9402 ===
    Private Function GetVirtualNode() As clsVisualNode
        Dim tNode As clsVisualNode = Nothing
        Dim sNodeGUID As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("guid", "")) 'Anti-XSS
        With Project
            .ProjectManager.AntiguaDashboard.LoadPanel(clsProject.StorageType, .ConnectionString, .ProviderType, .ID)
            .ProjectManager.AntiguaInfoDocs.LoadAntiguaInfoDocs()
            If sNodeGUID <> "" Then
                Try
                    Dim tGUID As Guid = Str2GUID(sNodeGUID) ' D6704
                    tNode = .ProjectManager.AntiguaDashboard.GetNodeByGuid(tGUID)
                Catch ex As Exception
                End Try
            End If
        End With
        Return tNode
    End Function
    ' D9402 ==

    ' D2250 ===
    Private Function GetCurrentScale(ByRef MType As ECMeasureType) As clsMeasurementScale
        Dim Scale As clsMeasurementScale = Nothing
        Dim sScale As String = EcSanitizer.GetSafeHtmlFragment(CheckVar(_PARAM_ID, ""))   ' Anti-XSS
        If sScale.Length < 8 Then sScale = CheckVar("guid", "") ' D3543
        If sScale > "" Then
            Dim tScale As Guid = New Guid(sScale)
            Scale = Project.ProjectManager.MeasureScales.GetScaleByID(tScale) 'A1594
            '-A1594 Dim sType As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("scale", CInt(ECMeasureType.mtRatings).ToString)) ' D3543 + Anti-XSS
            '-A1594 Dim tType As Integer = -1
            '-A1594 ' D4705 ===
            '-A1594 If Integer.TryParse(sType, tType) AndAlso tScale <> Guid.Empty Then
            '-A1594     MType = CType(tType, ECMeasureType)
            '-A1594     Select Case MType
            '-A1594         ' D4705 ==
            '-A1594         Case ECMeasureType.mtRatings
            '-A1594             Dim tRS As clsRatingScale = Project.ProjectManager.MeasureScales.GetRatingScaleByID(tScale)
            '-A1594             If tRS IsNot Nothing Then Scale = tRS
            '-A1594     End Select
            '-A1594 End If
        End If
        Return Scale
    End Function
    ' D2250 ==

End Class
