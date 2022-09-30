Option Strict Off

Imports SpyronControls.Spyron.Core
Imports Telerik.Web.UI
Imports System.IO

Partial Class RichEditorPage
    Inherits clsComparionCorePage

    Public _ObjectType As reObjectType = reObjectType.Unspecified     ' D0107 + D3697
    Private _Project As clsProject = Nothing    ' D1146
    Private _Survey As clsSurveyInfo = Nothing          ' D1277
    Private _SurveyQuestion As clsQuestion = Nothing    ' D1277
    Private tStep As Integer = -1   ' D3699
    Private QuickHelpContent As String = ""             ' D3728
    Public QuickHelpAutoShow As Boolean = False         ' D3741
    Public sPathText As String = ""         ' D4105
    Public sTreeText As String = ""         ' D4105

    'Public MessageOnLoad As String = ""         ' D5071
    Private MaxCaptionName As Integer = 120      ' D1102

    Public Sub New()
        MyBase.New(_PGID_RICHEDITOR)
    End Sub

    Private Property CurrentProject() As clsProject
        Get
            Return _Project
        End Get
        Set(value As clsProject)
            _Project = value
        End Set
    End Property

    ' D1277 ===
    Private Property CurrentSurvey() As clsSurveyInfo
        Get
            If _Survey Is Nothing AndAlso CurrentProject IsNot Nothing Then ' D6967
                _Survey = CType(Session(SESSION_MAIN_SURVEY_INFO_EDIT + CurrentProject.ID.ToString + "-" + CInt(CheckVar("st", -1)).ToString), clsSurveyInfo)
                If _Survey Is Nothing Then _Survey = App.SurveysManager.GetSurveyInfoByProjectID(CurrentProject.ID, CInt(CheckVar("st", -1)), Nothing)
            End If
            Return _Survey
        End Get
        Set(value As clsSurveyInfo)
            _Survey = value
            Session(SESSION_MAIN_SURVEY_INFO_EDIT + CurrentProject.ID.ToString + "-" + CInt(CheckVar("st", -1)).ToString) = value
        End Set
    End Property

    Private Property CurrentQuestion() As clsQuestion
        Get
            If _SurveyQuestion Is Nothing Then
                If CheckVar("new", False) AndAlso Session(SESSION_NEW_QUESTION) IsNot Nothing Then _SurveyQuestion = CType(Session(SESSION_NEW_QUESTION), clsQuestion) ' D1278
                If _SurveyQuestion Is Nothing Then
                    ' D1278 ===
                    Dim sQ As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("question", ""))    'Anti-XSS
                    If CurrentSurvey IsNot Nothing AndAlso sQ <> "" Then
                        _SurveyQuestion = CurrentSurvey.Survey("-").QuestionByGUID(New Guid(sQ))
                    End If
                    ' D1278 ==
                End If
            End If
            Return _SurveyQuestion
        End Get
        Set(value As clsQuestion)
            _SurveyQuestion = value
            Session(SESSION_NEW_QUESTION) = value
        End Set
    End Property
    ' D1277 ==

    ' D3696 ===
    Public Function CallbackFunction() As String
        Return CStr(IIf(_ObjectType = reObjectType.QuickHelp, "onRichEditorRefreshQH", "onRichEditorRefresh"))
    End Function
    ' D3696 ==

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        StorePageID = False
        ' D1146 ===
        If Not App.isAuthorized Then
            Dim sMID As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("meetingid", "")) 'Anti-XSS
            If sMID <> "" Then
                Dim PrjID As Object = App.Database.ExecuteScalarSQL(String.Format("SELECT ProjectID FROM StructureMeetings WHERE MeetingID={0}", sMID))
                Dim tPrjID As Integer = -1
                If PrjID IsNot Nothing AndAlso Not IsDBNull(PrjID) AndAlso Integer.TryParse(CStr(PrjID), tPrjID) Then CurrentProject = App.DBProjectByID(tPrjID)
            End If
        End If
        ' D9402 ===
        If CurrentProject Is Nothing AndAlso App.isAntiguaAuthorized Then
            CurrentProject = AnonAntiguaProject()
            If CurrentProject Is Nothing Then FetchAccess()
        End If
        ' D9402 ==
        Dim otID As Integer = 0
        If Integer.TryParse(CheckVar("type", otID), otID) Then _ObjectType = CType(otID, reObjectType)
        If CurrentProject Is Nothing AndAlso App.HasActiveProject Then CurrentProject = App.ActiveProject
        If CurrentProject Is Nothing AndAlso _ObjectType <> reObjectType.SurveyQuestion Then FetchAccess() ' D1277
        ' D3279 ===
        If CurrentProject IsNot Nothing Then
            Dim sPasscode As String = EcSanitizer.GetSafeHtmlFragment(CheckVar(_PARAM_PASSCODE, "")).Trim.ToLower   'Anti-XSS
            If CurrentProject.IsRisk AndAlso CurrentProject.PasscodeImpact.ToLower = sPasscode Then
                CurrentProject.ProjectManager.ActiveHierarchy = ECHierarchyID.hidImpact ' D2898
                CurrentProject.ProjectManager.PipeParameters.CurrentParameterSet = CurrentProject.ProjectManager.PipeParameters.ImpactParameterSet
            End If
            If CurrentProject.PasscodeLikelihood.ToLower = sPasscode Then
                CurrentProject.ProjectManager.ActiveHierarchy = ECHierarchyID.hidLikelihood
                CurrentProject.ProjectManager.PipeParameters.CurrentParameterSet = CurrentProject.ProjectManager.PipeParameters.DefaultParameterSet
            End If
        End If
        ' D3279 ==
        ' D1146 ==
        ' D0107 ===
        If Not IsPostBack AndAlso Not isCallback Then
            ClientScript.RegisterStartupScript(GetType(String), "DummyTemplates", GetDummyTemplates(), True) ' D4936
            'CType(Master, clsComparionCoreMasterPopupPage).ShowButtonsLine = True  ' D0107 + D0493
            btnSave.Text = ResString("btnOK")
            btnApply.Text = ResString("btnApply")   ' D0348
            btnClose.Text = ResString("btnCancel")
            ' D1953 ===
            If (_ObjectType = reObjectType.Alternative OrElse _ObjectType = reObjectType.Node OrElse _ObjectType = reObjectType.AntiguaInfodoc) AndAlso ((CheckVar("reset", False) OrElse CheckVar("refresh", False))) Then
                Dim HID As ECHierarchyID = CurrentProject.ProjectManager.ActiveHierarchy
                CurrentProject.ResetProject(True) ' D1109 + D1146
                CurrentProject.ProjectManager.ActiveHierarchy = HID
            End If
            ' D1953 ==
            LoadData()

            ' D4050 ===
            'FileUpload.Attributes.Add("style", "display:none;")
            FileUpload.Attributes.Add("onchange", "if (this.value!='') { theForm.btnUpload.disabled=0; theForm.btnUpload.focus(); }")
            ' D4050 ==

            ' D1457 ===
            If (_ObjectType = reObjectType.ExtraInfo OrElse _ObjectType = reObjectType.PipeMessage) AndAlso TinyMCEEditor.Enabled Then
                Dim sParam As String = EcSanitizer.GetSafeHtmlFragment(CheckVar(_PARAM_ID, "")) 'Anti-XSS
                If sParam.ToLower = _PARAM_INVITATION_EVAL OrElse sParam.ToLower = _PARAM_INVITATION_TT OrElse _ObjectType = reObjectType.PipeMessage OrElse sParam.ToLower = _PARAM_INVITATION_CUSTOM.ToLower Then  ' D3964 + D4650
                    lnkHints.Text = String.Format("<b>{0}</b>", ResString("lblEmailVariables"))
                    Dim sTooltip As String = String.Format("<ul type=square style='margin:0px 1em 1ex 0px; text-align: left;'>{0}", lnkHints.Text)
                    Dim tCustom As String() = {_TEMPL_APPNAME, _TEMPL_PRJNAME, _TEMPL_PRJPASSCODE, _TEMPL_MEETING_ID, _TEMPL_URL_APP,
                                               _TEMPL_URL_LOGIN, _TEMPL_URL_EVALUATE, _TEMPL_URL_EVALUATE_TT, _TEMPL_URL_MEETINGID,
                                               _TEMPL_URL_EVALUATE_ANONYM, _TEMPL_URL_EVALUATE_SIGNUP, _TEMPL_URL_RESETPSW, _TEMPL_URL_EVALUATE_SIGNUP_EMAILONLY,
                                               _TEMPL_URL_EVALUATE_SIGNUP_EMAILPSW, _TEMPL_URL_EVALUATE_SIGNUP_NAMEONLY, _TEMPL_SERVICE_EMAIL}
                    ' D4065 ===
                    Dim Lst As String() = _TEMPL_LIST_ALL(App.isRiskEnabled)
                    For i As Integer = 0 To Lst.Length - 1   ' D2467
                        If (Lst(i) <> _TEMPL_MEETING_ID OrElse _MEETING_ID_AVAILABLE) AndAlso (tCustom.Contains(Lst(i)) OrElse sParam.ToLower <> _PARAM_INVITATION_CUSTOM.ToLower) Then    ' D0395 + D4650
                            If Lst(i) = _TEMPL_URL_APP OrElse Lst(i) = _TEMPL_URL_MEETINGID OrElse Not Lst(i).StartsWith("%%url_") OrElse sParam.ToLower = _PARAM_INVITATION_EVAL.ToLower OrElse sParam.ToLower = _PARAM_INVITATION_TT.ToLower Then ' D4936
                                If Not isSSO_Only() OrElse Not _TEMPL_LIST_HIDE_URLS.Contains(Lst(i)) Then    ' D6552 + D7444
                                    If _TEMPL_LIST_RES(App.isRiskEnabled).Count > i Then sTooltip += String.Format("<li><a href='' onclick='return InsertTemplate(""{2}"")' class='actions'>{0}</a>: {1}</li>", Lst(i), ResString(_TEMPL_LIST_RES(App.isRiskEnabled)(i)), JS_SafeString(Lst(i)))    ' D0221 + D2467 + A1561
                                End If
                            End If
                        End If
                    Next
                    sTooltip += String.Format("</ul><div style='margin:1ex' class='text small gray'>{0}</div>", ResString("msgClick2Insert"))    ' D7444
                    ' D4065 ==
                    dxToolTipHints.InnerHtml = sTooltip
                    tdHints.Visible = True
                End If
            End If
            ' D1457 ==

            ' D3696 + D3699 ===
            If _ObjectType = reObjectType.QuickHelp AndAlso TinyMCEEditor.Enabled Then
                Dim tEvalType As ecEvaluationStepType = CheckVar("qh", CInt(ecEvaluationStepType.Other))
                ' D3717 ===
                tdQHOptions.InnerHtml = ""
                'Dim sApply As String = CheckVar("apply", "").Trim
                Dim sLst As String = "" ' D3731
                Dim tCnt As Integer = 0 ' D3731
                ' D4082 ===
                Dim tCurStep As Integer = CheckVar("step", -1)

                Dim HasQHCluster As Boolean = False
                If tCurStep > 0 Then
                    ' D4108 ===
                    Dim QHApplySteps As New List(Of Integer)
                    Dim ApplyNodes As List(Of clsNode) = CurrentProject.ProjectManager.PipeBuilder.GetPipeStepClusters(tCurStep, HasQHCluster, QHApplySteps) ' D4116
                    tCnt = ApplyNodes.Count

                    If HasQHCluster OrElse ApplyNodes.Count > 0 Then
                        Dim tCurNode As clsNode = CurrentProject.ProjectManager.PipeBuilder.GetPipeActionNode(CurrentProject.ProjectManager.Pipe(tCurStep - 1))  ' D4113 + D4116
                        For i As Integer = 0 To ApplyNodes.Count - 1
                            Dim tStep As Integer = QHApplySteps(i)
                            Dim tParentNode As clsNode = ApplyNodes(i)
                            Dim fActive = (tCurStep = tStep) OrElse (tParentNode Is tCurNode)  ' D4113
                            Dim sName As String = SafeFormString(ShortString(tParentNode.NodeName, 40, True))
                            If fActive Then sName = String.Format("<b>{0}</b>", sName)
                            Dim fChecked As Boolean = fActive
                            Dim AutoShow As Boolean = False ' D4082
                            Dim sContent As String = CurrentProject.PipeParameters.PipeMessages.GetEvaluationQuickHelpText(CurrentProject.ProjectManager, tStep, True, AutoShow)    ' D3741 + D4079 + D4082
                            If sContent <> "" AndAlso QuickHelpContent = sContent Then fChecked = True ' D4116
                            sLst += String.Format("<div><nobr><label><input type='checkbox' id='qh_apply_{1}'{2}{4} value='{3}'>#{3}. {0}</label></nobr></div>", sName, i, IIf(fActive, " disabled", ""), tStep, IIf(fChecked, " checked", ""))     ' D4092
                        Next
                        If sLst <> "" Then sLst += String.Format("<input type='hidden' id='qh_apply_cnt' value='{0}'>", tCnt)
                    End If
                    ' D4108 ==
                End If
                ' D6662 ===
                Select Case tEvalType
                    Case ecEvaluationStepType.RiskResults, ecEvaluationStepType.HeatMap
                    Case Else
                        ' D6662 ==
                        ' D3731 ===
                        btnApply.Text = ResString("lblQHApplyToCluster")
                        If tCnt = 0 Then
                            btnApply.Enabled = False
                        Else
                            If tCnt = 1 Then
                                btnApply.Enabled = HasQHCluster ' D4113
                                btnApply.OnClientClick = String.Format("SaveChanges(true, '&applyto={0}'); return false;", tCurStep)    ' D4082
                            Else
                                ' D3728 ===
                                divList.InnerHtml = String.Format("<div id='divList' style='margin:1ex 1ex 1ex 1em; {2}; overflow-x:hidden;{0}'>{1}</div>", IIf(tCnt > 6, " height:8em;", ""), sLst, IIf(tCnt > 6, "overflow-y:scroll", ""))
                                btnApply.Text = ResString("lblQHApplyTo")
                                btnApply.OnClientClick = "onApplyTo(); return false;"
                                ' D3728 ==
                            End If
                        End If
                        ' D4082 ==
                        btnApply.Width = Unit.Pixel(150)
                        btnSave.Width = Unit.Pixel(150)
                        btnSave.Text = ResString("btnQHSaveStep")
                        ' D3731 ==
                End Select

                tdQHOptions.InnerHtml += String.Format("<span style='float:right'><label><input type='checkbox' name='cbQHAutoShow' value='1' {0}>{1}</label>&nbsp;</span>", IIf(QuickHelpAutoShow, "checked", ""), ResString("lblQHAutoShowOption"))  ' D3741
                Dim sDefault As String = File_GetContent(GetIncFile(String.Format(_FILE_TEMPL_QUCIK_HELP, tEvalType.ToString + IIf(App.isRiskEnabled, IIf(CurrentProject.isImpact, "_Impact", "_Likelihood"), ""))), "").Trim   ' D3778
                If sDefault = "" AndAlso App.isRiskEnabled Then sDefault = File_GetContent(GetIncFile(String.Format(_FILE_TEMPL_QUCIK_HELP, tEvalType.ToString)), "").Trim ' D3778
                If sDefault <> "" Then
                    sDefault = PrepareTask(sDefault)
                    'If CheckVar("apply", False) Then tdQHOptions.InnerHtml += String.Format("<span style='float:right'><nobr><label><input type='checkbox' name='cbCluster' id='cbCluster' value='1'{1}>{0}</label></nobr></span>", ResString("lblQHApplyCluster"), IIf(tStep > 0, "", " checked"))
                    tdQHOptions.InnerHtml += String.Format("<nobr>[ <a href='' onclick='useDefault(); return false;' class='dashed'>{0}</a> ]</nobr><input type='hidden' id='txtDefault' value='{1}'/>", ResString("lblQHGetDefault"), SafeFormString(sDefault))
                End If
                trQHOptions.Visible = tdQHOptions.InnerHtml <> ""
                ' D3717 ==
            End If
            ' D3696 + D3699 ==

            cbAutoSave.InputAttributes.Add("onclick", "SwitchAutoSave();")
            cbAutoSave.Checked = GetCookie("RE_AutoSave", "1") = "1"
            cbAutoSave.Text = ResString("lblRichEditorAutoSave")
            cbAutoSave.Visible = _ObjectType <> reObjectType.QuickHelp  ' D3720
            'cbAutoSave.InputAttributes.Add("onkeypress", "SwitchAutoSave();")
            'cbAutoSave.InputAttributes.Add("onchange", "SwitchAutoSave();")
        End If
        TinyMCEEditor.Focus()
        ' D0107 ==

        ' D4050 ===
        If CheckVar("doUpload", "") <> "" AndAlso FileUpload.HasFile Then
            Dim fUploaded As Boolean = False
            Dim sUploadedFileName As String = File_CreateTempName()
            FileUpload.SaveAs(sUploadedFileName)
            Dim tmpMHT As String = File_GetContent(sUploadedFileName, "")

            If tmpMHT <> "" Then
                Dim sBaseURL As String = String.Format("{0}://{1}", Request.Url.Scheme, Request.Url.Host)
                Dim ID As Integer = 0
                If CurrentProject IsNot Nothing Then ID = CurrentProject.ID
                Dim sBasePath As String = Infodoc_Path(ID, 0, reObjectType.Unspecified, "Upload", -1)
                Dim sContent As String = tmpMHT
                Dim isValid As Boolean = False
                If isMHT(tmpMHT) Then
                    Dim Lines() As String = tmpMHT.Substring(0, CInt(IIf(tmpMHT.Length > 1000, 1000, tmpMHT.Length))).Replace(vbCr, "").Split(vbLf)   ' D4056
                    For Each sLine In Lines
                        If sLine.StartsWith("X-ECC:") Then
                            Dim idx = tmpMHT.IndexOf(sLine)
                            If idx > 0 Then
                                Dim sMHT As String = tmpMHT.Substring(idx + sLine.Length + 2)
                                Dim sMD5 As String = GetMD5(sMHT)
                                If sLine.Contains(sMD5) Then
                                    isValid = True
                                    Exit For
                                End If
                            End If
                        End If
                    Next
                    If isValid Then sContent = Infodoc_Unpack(ID, 0, reObjectType.Unspecified, "Upload", tmpMHT, True, False, -1)
                Else
                    Infodoc_Prepare(ID, 0, reObjectType.Unspecified, "Upload")
                    isValid = True
                End If
                If isValid Then
                    Dim sTitle As String = ""
                    fUploaded = SaveData(sContent, tmpMHT, sTitle, sBasePath)
                End If
                File_Erase(sUploadedFileName)
                File_DeleteFolder(sBasePath)
                If fUploaded Then
                    'Response.Redirect(Request.Url.OriginalString, True)
                    Response.Redirect(RemoveXssFromUrl(Request.RawUrl), True)  ' D6766
                Else
                    ClientScript.RegisterStartupScript(GetType(String), "msg", "msgWrongFile();", True)
                End If
            End If
        End If
        ' D4050 ==

        ' D3473 ===
        If CheckVar("ajax", False) Then
            Select Case CheckVar("action", "").Trim.ToLower
                Case "save"
                    ' D4049 ===
                    Dim sMHT As String = ""
                    Dim sTitle As String = ""
                    If SaveData(HttpUtility.UrlDecode(CheckVar("text", "")), sMHT, sTitle, "") Then ' D4066
                        RawResponseStart()
                        Response.ContentType = "text/plain"
                        'Response.AddHeader("Content-Length", CStr(sResult))
                        Dim sReply As String = ResString("msgSaved")
                        If CheckVar("download", False) Then
                            If String.IsNullOrEmpty(sMHT) Then
                                sReply = String.Format("<span class='error'>{0}</span>", ResString("msgEmptyDownload"))
                            Else
                                Dim sMD5 As String = GetMD5(sMHT)
                                Dim sFileName As String = File_CreateTempName()
                                Dim fileData As FileInfo = MyComputer.FileSystem.GetFileInfo(sFileName)
                                MyComputer.FileSystem.WriteAllText(sFileName, sMHT, False)
                                Dim sURL As String = RemoveXssFromUrl(Request.Url.AbsoluteUri) ' D6766
                                'Dim sTitle As String = HTML2Text(lblCaption.Text).Trim
                                'If sTitle.ToLower.StartsWith("edit") Then sTitle = sTitle.Substring(5).Trim
                                If CurrentProject IsNot Nothing Then sTitle = String.Format("{1} - {0}", ShortString(sTitle, 50, False), ShortString(CurrentProject.ProjectName, 35)).Replace("...", "_") ' D4085
                                sTitle = GetProjectFileName(sTitle.Trim, "Infodoc", "", ".mht")
                                sURL = sURL.Replace("ajax=" + CheckVar("ajax", ""), "").Replace("action=save", "action=download").Replace("&&", "&")
                                sReply = String.Format("{0}&f={1}&t={2}", sURL, HttpUtility.UrlEncode(fileData.Name), HttpUtility.UrlEncode(sTitle))
                            End If
                        End If
                        Response.Write(sReply)
                        ' D4049 ==
                        Response.End()
                        'RawResponseEnd()
                    End If
                    ' -D3741
                    'Case "autoshow"
                    '    Dim sVal As String = CheckVar("autoshow", "")
                    '    If sVal <> "" Then
                    '        App.QuickHelp_AutoShow = sVal <> "0"
                    '        RawResponseStart()
                    '        Response.ContentType = "text/plain"
                    '        Response.Write("Option saved")
                    '        Response.End()
                    '    End If

            End Select
        End If
        If CheckVar("action", "").ToLower = "download" Then
            Dim sName As String = CheckVar("f", "")
            Dim sTitle As String = CheckVar("t", "")
            If sName <> "" Then
                Dim sFilename = Path.GetTempPath() + sName
                If MyComputer.FileSystem.FileExists(sFilename) Then
                    If sTitle = "" Then
                        If CurrentProject IsNot Nothing Then sTitle = GetProjectFileName(ShortString(CurrentProject.ProjectName, 35, True) + " (Infodoc)", "Infodoc", "", ".mht") Else sTitle = "Infodoc.mht" ' D4085
                    End If
                    DownloadFile(sFilename, "multipart/related", sTitle, dbObjectType.einfFile, App.ProjectID)  ' D6593
                    'RawResponseStart()
                    'Response.AppendHeader("Content-Disposition", String.Format("attachment; filename=""{0}""", HttpUtility.UrlEncode(SafeFileName(sTitle))))    ' D6591
                    ''Response.ContentType = "application/octet-stream"
                    'Response.ContentType = "multipart/related"
                    'Dim fileLen As Integer = MyComputer.FileSystem.GetFileInfo(sFilename).Length
                    'Response.AddHeader("Content-Length", CStr(fileLen))
                    'Response.BinaryWrite(MyComputer.FileSystem.ReadAllBytes(sFilename))
                    'App.DBSaveLog(dbActionType.actDownload, dbObjectType.einfProject, App.ProjectID, sTitle, String.Format("Filename: {0}; Size: {1}", Path.GetFileName(sFilename), fileLen))
                    'File_Erase(sFilename)
                    'If Response.IsClientConnected Then RawResponseEnd()
                End If
            End If
        End If

        'ImageUpload.Visible = False
        'ImageUploadBtn.Visible = False
        ' D3473 ==
    End Sub

    ' D0998 ===
    Public Function GetInfodocURL() As String
        ' D2250 ===
        If _ObjectType = reObjectType.MeasureScale Then
            Return PageURL(_PGID_EVALUATE_INFODOC, String.Format("type={0}&{1}={2}&guid={4}&scale={3}", CInt(_ObjectType), _PARAM_ID, EcSanitizer.GetSafeHtmlFragment(CheckVar(_PARAM_ID, "")), EcSanitizer.GetSafeHtmlFragment(CheckVar("scale", CInt(ECMeasureType.mtRatings).ToString)), EcSanitizer.GetSafeHtmlFragment(CheckVar("guid", "")))) ' D2250 + D3543 + Anti-XSS
        End If
        ' D2250 ==
        '' D7011 ===
        'If _ObjectType = reObjectType.DashboardInfodoc Then
        '    Return PageURL(_PGID_EVALUATE_INFODOC, String.Format("type={0}&dash={1}&id={2}&r=", CInt(_ObjectType), EcSanitizer.GetSafeHtmlFragment(CheckVar("dash", "")), EcSanitizer.GetSafeHtmlFragment(CheckVar("id", "")), GetRandomString(6, True, False)))
        'End If
        '' D7011 ==
        If Not IsInfoDoc() Then Return ""
        ' D1003 ===
        Dim sID As String = EcSanitizer.GetSafeHtmlFragment(CheckVar(_PARAM_ID, ""))    'Anti-XSS
        Dim sGUID As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("guid", "")) 'Anti-XSS
        Dim sPID As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("pid", ""))   'Anti-XSS
        Dim sPGUID As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("pguid", ""))   'Anti-XSS
        Return PageURL(_PGID_EVALUATE_INFODOC, String.Format("type={0}&{1}{2}", CInt(_ObjectType), IIf(sGUID <> "", "guid=" + sGUID.ToString, "id=" + sID.ToString), IIf(sPGUID <> "", "&pguid=" + sPGUID.ToString, IIf(sPID <> "", "&pid=" + sPID.ToString, "")))) ' D1301
        ' D1003 ==
    End Function
    ' D0998 ==

    ' D0107 ===
    Private Sub LoadData()
        Dim fLoaded As Boolean = False
        Dim sCaption As String = ""
        Dim sContent As String = ""
        Dim ObjType As reObjectType = reObjectType.Unspecified  ' D0132
        Dim ObjID As String = ""    ' D0132
        Dim PidID As Integer = -1   ' D1003
        Dim HID As Integer = 0      ' D1669
        Dim tStepNode As clsNode = Nothing  ' D4105

        If CurrentProject IsNot Nothing Or _ObjectType = reObjectType.SurveyQuestion Then    ' D1128 + D1146 + D1277
            Select Case _ObjectType

                Case reObjectType.Node, reObjectType.Alternative
                    tStepNode = GetNode(False)   ' D1003
                    If Not tStepNode Is Nothing Then
                        sCaption = String.Format(ResString("lblRichEditNode"), SafeFormString(ShortString(tStepNode.NodeName, MaxCaptionName, True)), ResString(IIf(IsInfoDoc, "lblStructureDescription", "lblStructureAdditionalInfo"))) ' D0105 + D1102
                        sContent = IIf(IsInfoDoc, Infodoc_Unpack(CurrentProject.ID, CurrentProject.ProjectManager.ActiveHierarchy, _ObjectType, tStepNode.NodeID.ToString, tStepNode.InfoDoc, True, True, -1), tStepNode.Comment) ' D0131 + D1146 + D1669
                        ObjType = _ObjectType   ' D0132
                        ObjID = tStepNode.NodeID.ToString  ' D0132
                        fLoaded = True
                    End If

                    ' D1003 ===
                Case reObjectType.AltWRTNode
                    Dim tNode As clsNode = GetNode(False)
                    Dim tParentNode As clsNode = GetNode(True)
                    If Not tNode Is Nothing AndAlso tParentNode IsNot Nothing Then
                        sCaption = String.Format(ResString(CStr(IIf(CurrentProject.IsRisk AndAlso Not CurrentProject.isImpact, "lblRichEditWRTLikelihood", "lblRichEditWRT"))), SafeFormString(ShortString(tNode.NodeName, MaxCaptionName, True)), SafeFormString(tParentNode.NodeName))    ' D1102 + Â3031
                        Dim sInfodoc As String = CurrentProject.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(tNode.NodeGuidID, tParentNode.NodeGuidID)
                        sContent = CStr(IIf(IsInfoDoc, Infodoc_Unpack(CurrentProject.ID, CurrentProject.ProjectManager.ActiveHierarchy, _ObjectType, tNode.NodeID.ToString, sInfodoc, True, True, tParentNode.NodeID), tNode.Comment))   ' D1146 + D1669
                        ObjType = _ObjectType
                        ObjID = tNode.NodeID.ToString
                        tStepNode = tNode   ' D4105
                        If Not tParentNode.IsAlternative Then tStepNode = tParentNode ' D4105
                        PidID = tParentNode.NodeID
                        fLoaded = True
                    End If
                    ' D1003 ==

                    ' D0113 ===
                Case reObjectType.PipeMessage
                    ' D4293 ===
                    Dim tType As PipeMessageKind = PipeMessageKind.pmkText
                    If CheckVar("risk", False) Then tType = PipeMessageKind.pmlTextRiskControls
                    Dim sPostfix As String = IIf(tType = PipeMessageKind.pmlTextRiskControls, "_risk", "")
                    ' D4293 ==
                    Select Case CheckVar(_PARAM_ID, "").ToLower

                        Case "welcome"
                            sCaption = ResString("lblWelcome")  ' D0120
                            sContent = CurrentProject.PipeParameters.PipeMessages.GetWelcomeText(tType, CurrentProject.ProjectManager.ActiveHierarchy, CurrentProject.ProjectManager.ActiveAltsHierarchy) 'C0139 + D4293
                            If sContent = "" Then
                                sContent = ParseURLAndPathTemplates(File_GetContent(GetWelcomeThankYouIncFile(False, CurrentProject.isImpact, CurrentProject.PipeParameters.ProjectType = ProjectType.ptOpportunities, tType = PipeMessageKind.pmlTextRiskControls)))
                                sContent = ParseAllTemplates(sContent, App.ActiveUser, CurrentProject)  ' D4462
                            Else
                                sContent = Infodoc_Unpack(CurrentProject.ID, CurrentProject.ProjectManager.ActiveHierarchy, reObjectType.PipeMessage, "welcome" + sPostfix, sContent, True, True, -1) ' D0120 + D0131 + D0800 + D1146 + D1550 + D1669 + D2325 + D3326 + D4293
                            End If
                            ObjType = reObjectType.PipeMessage  ' D0132
                            ObjID = "welcome" + sPostfix  ' D0132 + D4293
                            fLoaded = True
                        Case "thankyou"
                            ' D3964 ===
                            Dim isReward As Boolean = CheckVar("mode", "") = "reward"
                            sCaption = ResString(CStr(IIf(isReward, "lblRewardThankYou", "lblThankYou"))) ' D0120
                            sContent = CurrentProject.PipeParameters.PipeMessages.GetThankYouText(IIf(isReward, PipeMessageKind.pmkReward, tType), CurrentProject.ProjectManager.ActiveHierarchy, CurrentProject.ProjectManager.ActiveAltsHierarchy) 'C0139 + D4293
                            ' D3972 ===
                            If sContent = "" Then
                                If isReward Then
                                    sContent = ParseURLAndPathTemplates(File_GetContent(GetIncFile(_FILE_TEMPL_THANKYOU_REWARD)))
                                Else
                                    sContent = ParseURLAndPathTemplates(File_GetContent(GetWelcomeThankYouIncFile(True, CurrentProject.isImpact, CurrentProject.PipeParameters.ProjectType = ProjectType.ptOpportunities, tType = PipeMessageKind.pmlTextRiskControls))) ' D4293
                                End If
                                sContent = ParseAllTemplates(sContent, App.ActiveUser, CurrentProject)  ' D4462
                            Else
                                sContent = Infodoc_Unpack(CurrentProject.ID, CurrentProject.ProjectManager.ActiveHierarchy, reObjectType.PipeMessage, CStr("thankyou" + IIf(isReward, "_reward", "") + sPostfix), sContent, True, True, -1) ' D0120 + D0131 + D0800 + D1146 + D1550 + D1669 + D2325 + D3326 + D4293
                            End If
                            ' D3964 + D3972 ==
                            ObjType = reObjectType.PipeMessage  ' D0132
                            ObjID = IIf(isReward, "thankyou_reward", "thankyou") + sPostfix   ' D0132 + D3985 + D4293
                            fLoaded = True
                    End Select
                    sCaption = String.Format(ResString("lblRichEditMessage"), sCaption) ' D0120
                    ' D0113 ==

                    ' D2064 ===
                Case reObjectType.Description
                    sContent = CurrentProject.ProjectManager.ProjectDescription    ' D2083
                    sContent = Infodoc_Unpack(CurrentProject.ID, CurrentProject.ProjectManager.ActiveHierarchy, reObjectType.Description, "", sContent, True, True, -1) ' D2083
                    If sContent = "" Then sContent = SafeFormString(CurrentProject.Comment).Replace(vbLf, "<br>")     ' D2086 + D6317
                    ObjType = reObjectType.Description
                    ObjID = ""
                    sCaption = String.Format(ResString("lblRichEditDescription"), sCaption) ' D0120
                    fLoaded = True
                    ' D2064 ==

                    ' D0299 ===
                Case reObjectType.ExtraInfo
                    Dim sParam As String = EcSanitizer.GetSafeHtmlFragment(CheckVar(_PARAM_ID, "")).Trim.ToLower  'Anti-XSS
                    If sParam <> "" Then
                        ' D0509 ===
                        Select Case sParam
                            Case _PARAM_INVITATION_TT.ToLower   ' D1358
                                sContent = CurrentProject.PipeParameters.TeamTimeInvitationBody2 ' D1358 + D3371
                                If sContent = "" Then sContent = ParseURLAndPathTemplates(App.ResString("bodySynchronousInvitations")).Replace(vbCrLf, "<br>") ' D0312 + D1146 + D1357 + D1369 + D1550
                                sCaption = ResString("lblInvitationTeamTimeEdit")    ' D1378
                            Case _PARAM_INVITATION_EVAL.ToLower ' D1358
                                sContent = CurrentProject.PipeParameters.EvaluateInvitationBody2    ' D3371
                                If sContent = "" Then sContent = ParseURLAndPathTemplates(App.ResString(CStr(IIf(App.isRiskEnabled, "bodyDecisionInvitationsRisk", "bodyDecisionInvitations")))).Replace(vbCrLf, "<br>") ' D0312 + D1146 + D1357 + D1550 + D2467
                                sCaption = ResString("lblInvitationAnytimeEdit")    ' D1378
                                ' D4647 ===
                            Case _PARAM_INVITATION_CUSTOM.ToLower
                                sContent = CurrentProject.ProjectManager.Parameters.InvitationCustomText
                                sCaption = ResString("lblInvitationCustomText")
                                ' D4647 ==
                        End Select
                        ' D0509 ==
                        If sContent <> "" OrElse sParam = _PARAM_INVITATION_CUSTOM.ToLower Then ' D4647
                            If isMHT(sContent) Then sContent = Infodoc_Unpack(CurrentProject.ID, CurrentProject.ProjectManager.ActiveHierarchy, _ObjectType, sParam, sContent, True, True, -1) ' D1357 + D1669
                            fLoaded = True
                            ObjType = reObjectType.ExtraInfo
                            ObjID = sParam
                        End If
                    End If
                    ' D0299 ==

                    ' D1083 ===
                Case reObjectType.AntiguaInfodoc
                    Dim tNode As clsVisualNode = GetVirtualNode()
                    If tNode IsNot Nothing Then
                        sCaption = String.Format(ResString("lblAntiguaInfodoc"), SafeFormString(ShortString(tNode.Text, 35, True)))
                        sContent = CurrentProject.ProjectManager.AntiguaInfoDocs.GetAntiguaInfoDoc(tNode.GuidID)
                        If sContent Is Nothing Then sContent = ""
                        sContent = Infodoc_Unpack(CurrentProject.ID, CurrentProject.ProjectManager.ActiveHierarchy, _ObjectType, tNode.GuidID.ToString, sContent, True, True, -1)  ' D1109 + D1146 + D1669
                        ObjType = _ObjectType
                        ObjID = tNode.GuidID.ToString
                        fLoaded = True
                    End If
                    ' D1083 ==

                    ' D1277 ===
                Case reObjectType.SurveyQuestion
                    Dim fNew As Boolean = CheckVar("new", True)
                    Dim sFormField As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("fld", "")) 'Anti-XSS
                    Dim tSurvey As clsSurveyInfo = CurrentSurvey()
                    Dim tQuestion As clsQuestion = CurrentQuestion()
                    If tSurvey IsNot Nothing AndAlso tQuestion IsNot Nothing Then
                        sContent = Infodoc_Unpack(CurrentSurvey.ProjectID, 0, _ObjectType, CurrentQuestion.AGUID.ToString, CurrentQuestion.Text, True, True, -1, True)  ' D1278 + D4431
                        ' D4421 ===
                        Dim sTitle As String = ShortString(tSurvey.Title, 35, True)
                        If sTitle = "" Then sTitle = ResString(IIf(tSurvey.SurveyType = SurveyType.stImpactWelcomeSurvey OrElse tSurvey.SurveyType = SurveyType.stWelcomeSurvey, "lblWelcome", "lblThankYou"))
                        sCaption = String.Format(ResString(IIf(fNew, "lblSurveyQuestionNew", "lblSurveyQuestionEdit")), SafeFormString(sTitle))
                        ' D4421 ==
                        ObjType = _ObjectType
                        ObjID = tQuestion.AGUID.ToString
                        fLoaded = True
                        If sFormField <> "" Then ClientScript.RegisterStartupScript(GetType(String), "InitText", "setTimeout(""InitContent('" + sFormField + "');"", 300);", True)
                    End If
                    ' D1277 ==

                    ' D2249 ===
                Case reObjectType.MeasureScale
                    Dim MType As ECMeasureType = ECMeasureType.mtNone
                    Dim scale As clsMeasurementScale = GetCurrentScale(MType)
                    If scale IsNot Nothing Then
                        sContent = Infodoc_Unpack(CurrentProject.ID, CurrentProject.ProjectManager.ActiveHierarchy, _ObjectType, scale.GuidID.ToString, scale.Comment, True, True, -1)  ' D2266
                        sCaption = String.Format(ResString(IIf(String.IsNullOrEmpty(scale.Comment), "lblScaleDescNew", "lblScaleDescEdit")), SafeFormString(ShortString(scale.Name, 35, True)))
                        ObjType = _ObjectType
                        ObjID = scale.GuidID.ToString
                        fLoaded = True
                    End If
                    ' D2249 ==

                    ' D3693 ===
                Case reObjectType.QuickHelp
                    Dim tEvalType As ecEvaluationStepType = CheckVar("qh", CInt(ecEvaluationStepType.Other))
                    tStep = CheckVar("step", 0)  ' D3699
                    sCaption = ResString(String.Format("lblQuickHelp_{0}", tEvalType), True)
                    If Not sCaption.StartsWith("lblQuickHelp") AndAlso tEvalType <> ecEvaluationStepType.Other Then
                        ' D3695 ===
                        sCaption = String.Format(ResString("lblRichEditQuickHelp"), sCaption)
                        Dim tNode As clsNode = GetNode(False)
                        ObjID = GetQuickHelpObjectID(tEvalType, tNode)
                        ' D6662 ===
                        Select Case tEvalType
                            Case ecEvaluationStepType.RiskResults, ecEvaluationStepType.HeatMap
                                Dim _ID As Integer = CheckVar("id", -1)
                                sContent = CurrentProject.PipeParameters.PipeMessages.GetEvaluationQuickHelpForCustom(CurrentProject.ProjectManager, tEvalType, _ID, QuickHelpAutoShow)
                            Case Else
                                ' D6662 ==
                                If tNode IsNot Nothing Then sCaption += String.Format(" ('{0}')", SafeFormString(ShortString(tNode.NodeName, 45, True))) ' D4082
                                Dim isCluster As Boolean = False
                                sContent = CurrentProject.PipeParameters.PipeMessages.GetEvaluationQuickHelpText(CurrentProject.ProjectManager, tStep, isCluster, QuickHelpAutoShow)  ' D3699 + D3741 + D4079 + D4081
                                sCaption += String.Format("<div class='text small gray'>({0})</div>", IIf(isCluster, "Cluster value", "Step value"))    ' D3730 + D4081
                        End Select
                        QuickHelpContent = sContent ' D3728
                        If sContent <> "" Then sContent = Infodoc_Unpack(CurrentProject.ID, CurrentProject.ProjectManager.ActiveHierarchy, reObjectType.QuickHelp, ObjID, sContent, True, True, -1)
                        ' D3695 ==
                        ObjType = reObjectType.QuickHelp
                        fLoaded = True
                    End If
                    ' D3693 ==

                    ' D4283 ===
                Case reObjectType.Attribute
                    Dim tGUID As Guid = Str2GUID(CheckVar("guid", ""))
                    If Not tGUID.Equals(Guid.Empty) Then
                        Dim tAttr As clsAttribute = CurrentProject.ProjectManager.Attributes.GetAttributeByID(tGUID)
                        If tAttr IsNot Nothing Then
                            sCaption = String.Format(ResString("lblRichEditAttr"), SafeFormString(ShortString(tAttr.Name, MaxCaptionName, True)))
                            ObjType = _ObjectType
                            sContent = CurrentProject.ProjectManager.InfoDocs.GetCustomInfoDoc(tAttr.ID, Guid.Empty)
                            If Not String.IsNullOrEmpty(sContent) AndAlso isMHT(sContent) Then sContent = Infodoc_Unpack(CurrentProject.ID, CurrentProject.ProjectManager.ActiveHierarchy, _ObjectType, tAttr.ID.ToString, sContent, True, True, -1)
                            ObjID = tAttr.ID.ToString
                            fLoaded = True
                        End If
                    End If

                Case reObjectType.AttributeValue
                    Dim tGUID As Guid = Str2GUID(CheckVar("guid", ""))
                    If Not tGUID.Equals(Guid.Empty) Then
                        Dim tAttr As clsAttributeEnumeration = CurrentProject.ProjectManager.Attributes.GetEnumByID(tGUID)
                        If tAttr IsNot Nothing Then
                            sCaption = String.Format(ResString("lblRichEditAttrVal"), SafeFormString(ShortString(tAttr.Name, MaxCaptionName, True)))
                            ObjType = _ObjectType
                            sContent = CurrentProject.ProjectManager.InfoDocs.GetCustomInfoDoc(tAttr.ID, Guid.Empty)
                            If Not String.IsNullOrEmpty(sContent) AndAlso isMHT(sContent) Then sContent = Infodoc_Unpack(CurrentProject.ID, CurrentProject.ProjectManager.ActiveHierarchy, _ObjectType, tAttr.ID.ToString, sContent, True, True, -1)
                            ObjID = tAttr.ID.ToString
                            fLoaded = True
                        End If
                    End If

                Case reObjectType.ResultGroup
                    Dim tID As Integer = CheckVar("id", Integer.MinValue)
                    If tID <> Integer.MinValue Then
                        Dim tGrp As clsCombinedGroup = CurrentProject.ProjectManager.CombinedGroups.GetGroupByCombinedID(tID)
                        If tGrp IsNot Nothing Then
                            sCaption = String.Format(ResString("lblRichEditResGroup"), SafeFormString(ShortString(tGrp.Name, MaxCaptionName, True)))
                            ObjType = _ObjectType
                            Dim tGUID As New Guid(tID, CShort(reObjectType.ResultGroup), 0, 0, 0, 0, 0, 0, 0, 0, 0)
                            sContent = CurrentProject.ProjectManager.InfoDocs.GetCustomInfoDoc(tGUID, Guid.Empty)
                            If Not String.IsNullOrEmpty(sContent) AndAlso isMHT(sContent) Then sContent = Infodoc_Unpack(CurrentProject.ID, CurrentProject.ProjectManager.ActiveHierarchy, _ObjectType, tGrp.ID.ToString, sContent, True, True, -1)
                            ObjID = tGrp.ID.ToString
                            fLoaded = True
                        End If
                    End If
                    ' D4283 ==

                    ' D4344 ===
                Case reObjectType.Control
                    Dim tGUID As Guid = Str2GUID(CheckVar("guid", ""))
                    If Not tGUID.Equals(Guid.Empty) Then
                        Dim tCtrl As clsControl = CurrentProject.ProjectManager.Controls.GetControlByID(tGUID)
                        If tCtrl IsNot Nothing Then
                            sCaption = String.Format(ResString("lblRichEditControl"), SafeFormString(ShortString(tCtrl.Name, MaxCaptionName, True)))
                            ObjType = _ObjectType
                            sContent = GetControlInfodoc(CurrentProject, tCtrl, False) ' D4345
                            ObjID = tCtrl.ID.ToString
                            fLoaded = True
                        End If
                    End If
                    ' D4344 ==

                    ' D7011 ===
                Case reObjectType.DashboardInfodoc
                    Dim tDash As Integer = CheckVar("dash", Integer.MinValue)
                    If tDash <> Integer.MinValue AndAlso CurrentProject.ProjectManager.Reports.Reports.ContainsKey(tDash) Then
                        Dim D As clsReport = CurrentProject.ProjectManager.Reports.Reports(tDash)
                        Dim tID As Integer = CheckVar("id", Integer.MinValue)
                        If D.Items.ContainsKey(tID) Then
                            Dim Item = D.Items(tID)
                            sCaption = String.Format(ResString("lblRichEditDashboardInfodoc"), SafeFormString(ShortString(Item.Name, MaxCaptionName, True)))
                            ObjType = _ObjectType
                            Dim tGUID As New Guid(tID, CShort(reObjectType.DashboardInfodoc), CShort(tDash), 0, 0, 0, 0, 0, 0, 0, 0)
                            sContent = CurrentProject.ProjectManager.InfoDocs.GetCustomInfoDoc(tGUID, Guid.Empty)
                            If Not String.IsNullOrEmpty(sContent) AndAlso isMHT(sContent) Then sContent = Infodoc_Unpack(CurrentProject.ID, CurrentProject.ProjectManager.ActiveHierarchy, _ObjectType, tID.ToString, sContent, True, True, tDash)
                            If sContent = "" Then sContent = ResString("templDashboardInformationEmpty")    ' D7391
                            ObjID = tID.ToString
                            fLoaded = True
                        End If
                    End If
                    ' D7011 ==
            End Select
        End If

        ' D4176 ===
        If fLoaded AndAlso _ObjectType <> reObjectType.AntiguaInfodoc AndAlso App.isAuthorized AndAlso CurrentProject IsNot Nothing Then
            If Not App.CanUserModifyProject(App.ActiveUser.UserID, CurrentProject.ID, App.ActiveUserWorkgroup, App.DBWorkspaceByUserIDProjectID(App.ActiveUser.UserID, CurrentProject.ID), App.ActiveWorkgroup) Then
                fLoaded = False
                lblError.Text = ResString("msgCanUSerPMAction")
            End If
        End If
        ' D4176 ==

        TinyMCEEditor.Visible = fLoaded
        lblError.Visible = Not fLoaded
        If fLoaded Then
            ' D0131 ===
            ' D0132 ===
            If ObjType <> reObjectType.Unspecified AndAlso CurrentSurvey IsNot Nothing Then ' D1109 + D2083 + D6967
                ' D1277 ===
                Dim tID As Integer
                If ObjType = reObjectType.SurveyQuestion Then tID = CurrentSurvey().ProjectID Else tID = CurrentProject.ID ' D2079
                If CurrentProject IsNot Nothing Then HID = CurrentProject.ProjectManager.ActiveHierarchy ' D1669
                Infodoc_Prepare(tID, HID, ObjType, ObjID, Nothing, PidID)   ' D1004 + D1146
                InfodocPath.Value = Infodoc_URL(tID, HID, ObjType, ObjID, PidID) + _FILE_MHT_MEDIADIR   ' D5071
                ' D0132 + D1277 ==
                ' D4049 ===
                Dim main As New EditorToolGroup
                Dim customDownload As New EditorTool()
                customDownload.Name = "Download"
                customDownload.ShowText = False
                customDownload.ShowIcon = True
                customDownload.Text = ResString("lblRichEditorDownload")    ' D4298
                main.Tools.Add(customDownload)
                Dim customUpload As New EditorTool()
                customUpload.Name = "Upload"
                customUpload.ShowText = False
                customUpload.ShowIcon = True
                customUpload.Text = ResString("lblRichEditorUpload")    ' D4298
                main.Tools.Add(customUpload)
                ' D4049 ==

                ' D4105 ===
                If tStepNode IsNot Nothing AndAlso Not tStepNode.IsAlternative AndAlso tStepNode.ParentNode IsNot Nothing Then  ' D4343
                    Dim tpl As New EditorToolGroup
                    Dim customHierarchy As New EditorTool()
                    customHierarchy.Name = "Tree"
                    customHierarchy.ShowText = False
                    customHierarchy.ShowIcon = True
                    'customHierarchy.Attributes.Add("data", SafeFormString(GetNodePathString(tStepNode, ecNodePathFormat.FullHierarchy)))
                    sTreeText = JS_SafeString(GetNodePathString(tStepNode, ecNodePathFormat.FullHierarchy))
                    customHierarchy.Text = String.Format(ResString("lblRichEditorInsertHierarchy"), SafeFormString(tStepNode.NodeName))  ' D4298
                    tpl.Tools.Add(customHierarchy)
                    Dim customPath As New EditorTool()
                    customPath.Name = "Path"
                    customPath.ShowText = False
                    customPath.ShowIcon = True
                    'customPath.Attributes.Add("data", SafeFormString(GetNodePathString(tStepNode, ecNodePathFormat.FullPath)))
                    sPathText = JS_SafeString(GetNodePathString(tStepNode, ecNodePathFormat.FullPath))
                    customPath.Text = String.Format(ResString("lblRichEditorInsertPath"), SafeFormString(tStepNode.NodeName))  ' D4298
                    tpl.Tools.Add(customPath)
                End If
                ' D4105 ==
            End If
            ' D7444 ===
            If isSSO_Only() Then
                'Dim sStartURL As String = ApplicationURL(False, False) + _URL_ROOT
                'Dim sPasscodeURL As String = If(CurrentProject Is Nothing, sStartURL, URLWithParams(sStartURL, _PARAM_PASSCODE + "="))
                For Each sTpl As String In _TEMPL_LIST_HIDE_URLS
                    If sContent.ToLower.Contains(sTpl.ToLower) Then sContent = ParseTemplate(sContent, sTpl, _TEMPL_URL_APP + "?" + _PARAM_PASSCODE + "=" + _TEMPL_PRJPASSCODE)
                Next
            End If
            ' D7444 ==
            ' D0131 ==
            ' D3283 ===
            If sContent = "" OrElse (sContent.ToLower.IndexOf("</html>", StringComparison.InvariantCultureIgnoreCase) < 1 AndAlso sContent.ToLower.IndexOf("</title>", StringComparison.InvariantCultureIgnoreCase) < 1) Then  ' D4298 + D4371
                Dim sTitle = ""
                If CurrentProject IsNot Nothing Then sTitle = CurrentProject.ProjectName
                If sContent <> "" AndAlso _OPT_INFODOC_PARSE_PLAIN_LINKS Then sContent = ParseTextHyperlinks(sContent) ' D7453
                sContent = String.Format(_TEMPL_EMPTY_INFODOC, sContent, SafeFormString(sTitle), ThemePath)    ' D4371 + D6504, SafeFormString(ApplicationName), SafeFormString(sCaption))
                ' D1278 ==
            End If
            ' D3283 ==
            If sContent <> "" AndAlso sContent.ToLower.IndexOf("</html>", StringComparison.InvariantCultureIgnoreCase) < 1 Then sContent = String.Format("<HTML>{0}</HTML>", sContent) ' D4371

            TinyMCEEditor.Text = sContent

            TinyMCEEditor.Enabled = ObjType = reObjectType.SurveyQuestion OrElse ObjType = reObjectType.ExtraInfo OrElse CanEditActiveProject OrElse _ObjectType = reObjectType.AntiguaInfodoc OrElse (CurrentProject IsNot Nothing AndAlso ((CurrentProject.LockInfo IsNot Nothing AndAlso CurrentProject.LockInfo.LockStatus = ECLockStatus.lsLockForAntigua) OrElse (CurrentProject.LockInfo IsNot Nothing AndAlso CurrentProject.LockInfo.LockStatus = ECLockStatus.lsLockForTeamTime AndAlso CurrentProject.LockInfo.LockerUserID = App.ActiveUser.UserID) OrElse (CurrentProject.ProjectStatus = ecProjectStatus.psMasterProject OrElse CurrentProject.ProjectStatus = ecProjectStatus.psTemplate)))     ' D0135 + D0535 + D1083 + D1109 + D1277 + D1378 + D2492 + D2493 + D4407 + D4547
            btnSave.Enabled = TinyMCEEditor.Enabled  ' D0135 + D1083
            btnApply.Enabled = btnSave.Enabled  ' D1378
            lblCaption.Text = sCaption
        Else
            btnSave.Enabled = False     ' D0113
            btnApply.Enabled = False    ' D1004
        End If
    End Sub

    Private Function SaveData(ByVal sText As String, ByRef sMHT As String, ByRef sTitle As String, sBasePath As String) As Boolean
        Dim fUpdated As Boolean = False
        Dim sContent As String = sText  ' D3491
        Dim sBaseURL As String = String.Format("{0}://{1}", Request.Url.Scheme, Request.Url.Host)
        ' D0801 ===
        sContent = sContent.Trim

        'Dim sContentEmpty As String = HTML2Text(HTML2TextWithSafeTags(sContent.ToLower.Replace("<img ", "image<"), "")).Trim    ' D3711
        'If sContentEmpty = "" Then sContent = ""
        If isHTMLEmpty(sContent) Then sContent = "" ' D3956

        ' D0801 ==

        Select Case _ObjectType

            Case reObjectType.Alternative, reObjectType.Node
                Dim tNode As clsNode = GetNode(False)   ' D1003
                If Not tNode Is Nothing Then
                    If sBasePath = "" Then sBasePath = Infodoc_Path(CurrentProject.ID, CurrentProject.ProjectManager.ActiveHierarchy, _ObjectType, tNode.NodeID.ToString, -1) ' D0657 + D1003 + D1146 + D1669 + D4050
                    If IsInfoDoc() Then
                        tNode.InfoDoc = Infodoc_Pack(sContent, sBaseURL, sBasePath)
                        sMHT = tNode.InfoDoc    ' D4049
                    Else
                        tNode.Comment = sContent ' D0131 + D0611 + D0613 + D0657
                        sMHT = sContent     ' D4049
                    End If
                    sTitle = GetNodeTypeAndName(tNode)  ' D4049
                    fUpdated = CurrentProject.ProjectManager.StorageManager.Writer.SaveInfoDocs() 'C0276 + D1146
                    ' D1899 ===
                    If fUpdated AndAlso _ObjectType = reObjectType.Alternative Then
                        _ObjectType = reObjectType.AntiguaInfodoc
                        Dim tAntiguaNode As clsVisualNode = GetVirtualNode()
                        If tAntiguaNode IsNot Nothing Then
                            CurrentProject.ProjectManager.AntiguaInfoDocs.SetAntiguaInfoDoc(tAntiguaNode.GuidID, tNode.InfoDoc)
                        End If
                        _ObjectType = reObjectType.Alternative
                    End If
                    If fUpdated AndAlso App.isAuthorized Then App.SaveProjectLogEvent(CurrentProject, "Edit infodoc", False, GetNodeTypeAndName(tNode)) ' D3572 + D3731 + D3912 + D4037
                    ' D1899 ==
                End If

                ' D1003 ===
            Case reObjectType.AltWRTNode
                Dim tNode As clsNode = GetNode(False)
                Dim tParentNode As clsNode = GetNode(True)
                If Not tNode Is Nothing AndAlso tParentNode IsNot Nothing Then
                    If sBasePath = "" Then sBasePath = Infodoc_Path(CurrentProject.ID, CurrentProject.ProjectManager.ActiveHierarchy, _ObjectType, tNode.NodeID.ToString, tParentNode.NodeID) ' D1146 + D1669 + D4050
                    Dim sInfoDoc = Infodoc_Pack(sContent, sBaseURL, sBasePath)
                    sMHT = sInfoDoc ' D4049
                    sTitle = String.Format("{0} WRT {1}", GetNodeTypeAndName(tNode), GetNodeTypeAndName(tParentNode))   ' D4049
                    CurrentProject.ProjectManager.InfoDocs.SetNodeWRTInfoDoc(tNode.NodeGuidID, tParentNode.NodeGuidID, sInfoDoc)
                    fUpdated = CurrentProject.ProjectManager.StorageManager.Writer.SaveInfoDocs
                    If fUpdated AndAlso App.isAuthorized Then App.SaveProjectLogEvent(CurrentProject, "Edit infodoc", False, String.Format("'{0}' WRT '{1}'", tNode.NodeName, tParentNode.NodeName)) ' D3572 + D3731 + D4037
                End If
                ' D1003 ==

                ' D0113 ===
            Case reObjectType.PipeMessage
                ' D4293 ===
                Dim tType As PipeMessageKind = PipeMessageKind.pmkText
                If CheckVar("risk", False) Then tType = PipeMessageKind.pmlTextRiskControls
                Dim sPostfix As String = IIf(tType = PipeMessageKind.pmlTextRiskControls, "_risk", "")
                ' D4293 ==
                Dim isReward As Boolean = CheckVar("mode", "") = "reward" ' D3964
                If sBasePath = "" Then sBasePath = Infodoc_Path(CurrentProject.ID, CurrentProject.ProjectManager.ActiveHierarchy, _ObjectType, CheckVar(_PARAM_ID, "") + IIf(isReward, "_reward", "") + sPostfix, -1) ' D0657 + D1003 + D1146 + D1669 + D3964 + D4050 + D4293
                sMHT = Infodoc_Pack(sContent, sBaseURL, sBasePath)  ' D4049
                sTitle = String.Format("{0} page{1}", CheckVar(_PARAM_ID, ""), IIf(isReward, " (reward)", ""))  ' D4049
                Select Case CheckVar(_PARAM_ID, "").ToLower
                    Case "welcome"
                        CurrentProject.PipeParameters.PipeMessages.SetWelcomeText(tType, CurrentProject.ProjectManager.ActiveHierarchy, CurrentProject.ProjectManager.ActiveAltsHierarchy, sMHT)    ' D0131 'C0139 + D0611 + D0613 + D0657 + D4293
                        fUpdated = True
                    Case "thankyou"
                        CurrentProject.PipeParameters.PipeMessages.SetThankYouText(IIf(isReward, PipeMessageKind.pmkReward, tType), CurrentProject.ProjectManager.ActiveHierarchy, CurrentProject.ProjectManager.ActiveAltsHierarchy, sMHT)   ' D0131 'C0139 + D0611 + D0613 + D0657 + D3964 + D4293
                        fUpdated = True
                End Select
                If fUpdated Then
                    CurrentProject.PipeParameters.PipeMessages.Save(PipeStorageType.pstStreamsDatabase, CurrentProject.ConnectionString, CurrentProject.ProviderType, CurrentProject.ID) ' D0329 + D0369 + D0376 'C0420 + D0483 + D1146
                    If fUpdated AndAlso App.isAuthorized Then App.SaveProjectLogEvent(CurrentProject, "Edit infodoc", False, CheckVar(_PARAM_ID, "")) ' D3769 + D4037
                End If
                ' D0113 ==

                ' D2064 ===
            Case reObjectType.Description
                If sBasePath = "" Then sBasePath = Infodoc_Path(CurrentProject.ID, CurrentProject.ProjectManager.ActiveHierarchy, _ObjectType, "", -1) ' D4050
                Dim sComment As String = Infodoc_Pack(sContent, sBaseURL, sBasePath)
                sMHT = sComment ' D4049
                sTitle = String.Format("Project description", CurrentProject.ProjectName) ' D4049
                CurrentProject.ProjectManager.ProjectDescription = sComment ' D2083
                CurrentProject.ProjectManager.StorageManager.Writer.SaveInfoDocs()  ' D2083
                If App.isAuthorized Then App.SaveProjectLogEvent(CurrentProject, "Update project description", False, "") ' D3572 + D4037
                CurrentProject.Comment = HTML2Text(sContent)
                App.DBProjectUpdate(CurrentProject, False, "Edit project description")
                fUpdated = True

                ' D2064 ==

                ' D0299 ===
            Case reObjectType.ExtraInfo
                Dim sParam As String = EcSanitizer.GetSafeHtmlFragment(CheckVar(_PARAM_ID, "")).Trim.ToLower  'Anti-XSS
                If sParam <> "" Then
                    If sBasePath = "" Then sBasePath = Infodoc_Path(CurrentProject.ID, CurrentProject.ProjectManager.ActiveHierarchy, _ObjectType, sParam, -1) ' D1357 + D1669 + D4050
                    sContent = Infodoc_Pack(sContent, sBaseURL, sBasePath)  ' D1357
                    sMHT = sContent ' D4049
                    sTitle = String.Format("Invitation info ({0})", sParam) ' D4049
                    ' D0509 ===
                    Select Case sParam
                        Case _PARAM_INVITATION_TT.ToLower   ' D1358
                            CurrentProject.PipeParameters.TeamTimeInvitationBody2 = sContent ' D1358 + D3371
                            fUpdated = True ' D1357
                        Case _PARAM_INVITATION_EVAL.ToLower ' D1358
                            CurrentProject.PipeParameters.EvaluateInvitationBody2 = sContent     ' D3371
                            fUpdated = True ' D1357
                            ' D4647 ===
                        Case _PARAM_INVITATION_CUSTOM.ToLower
                            CurrentProject.ProjectManager.Parameters.InvitationCustomText = sContent
                            fUpdated = True
                            ' D4647 ==
                    End Select
                    ' D0509 ==
                    If fUpdated Then CurrentProject.SaveProjectOptions("Update invitation text", , , sParam) ' D1357 + D3731 + D3758
                End If
                ' D0299 ==

                ' D1083 ===
            Case reObjectType.AntiguaInfodoc
                Dim tNode As clsVisualNode = GetVirtualNode()
                If tNode IsNot Nothing Then
                    If sBasePath = "" Then sBasePath = Infodoc_Path(CurrentProject.ID, CurrentProject.ProjectManager.ActiveHierarchy, _ObjectType, tNode.GuidID.ToString, -1) ' D0657 + D1003 + D1146 + D1669 + D4050
                    sContent = Infodoc_Pack(sContent, sBaseURL, sBasePath)  ' D1109
                    sMHT = sContent ' D4049
                    sTitle = String.Format("CS infodoc ({0})", tNode.Text)   ' D4049
                    fUpdated = CurrentProject.ProjectManager.AntiguaInfoDocs.SetAntiguaInfoDoc(tNode.GuidID, sContent) ' D1146
                    ' D1899 ===
                    If fUpdated Then
                        _ObjectType = reObjectType.Alternative
                        Dim tAltNode As clsNode = GetNode(False)
                        If tAltNode IsNot Nothing Then
                            tAltNode.InfoDoc = sContent
                            CurrentProject.ProjectManager.StorageManager.Writer.SaveInfoDocs()
                            If App.isAuthorized Then App.SaveProjectLogEvent(CurrentProject, "Edit infodoc", False, GetNodeTypeAndName(tAltNode) + " (CS)") ' D3572 + D3731 + D4037
                        End If
                        _ObjectType = reObjectType.AntiguaInfodoc
                    End If
                    ' D1899 ==
                End If
                ' D1083 ==

                ' D1278 ===
            Case reObjectType.SurveyQuestion
                If CurrentQuestion IsNot Nothing AndAlso CurrentSurvey IsNot Nothing Then
                    If sBasePath = "" Then sBasePath = Infodoc_Path(CurrentSurvey.ProjectID, CurrentProject.ProjectManager.ActiveHierarchy, _ObjectType, CurrentQuestion.AGUID.ToString, -1) ' D2079 + D4050
                    CurrentQuestion.Text = Infodoc_Pack(sContent, sBaseURL, sBasePath)
                    sMHT = CurrentQuestion.Text ' D4049
                    sTitle = String.Format("Survey question ({0} - {1})", CurrentSurvey.Title, CurrentQuestion.Name)    ' D4049
                    Infodoc_Unpack(CurrentSurvey.ProjectID, 0, reObjectType.SurveyQuestion, CurrentQuestion.AGUID.ToString, CurrentQuestion.Text, True, True, -1, True)  ' D2079 + D4431
                    If CheckVar("new", False) Then Session(SESSION_NEW_QUESTION) = CurrentQuestion ' D4428
                    fUpdated = True
                End If
                ' D1278 ==

                ' D2249 ===
            Case reObjectType.MeasureScale
                Dim MType As ECMeasureType = ECMeasureType.mtNone
                Dim scale As clsMeasurementScale = GetCurrentScale(MType)
                If scale IsNot Nothing Then
                    If sBasePath = "" Then sBasePath = Infodoc_Path(CurrentProject.ID, CurrentProject.ProjectManager.ActiveHierarchy, _ObjectType, scale.GuidID.ToString, -1) ' D4050
                    scale.Comment = Infodoc_Pack(sContent, sBaseURL, sBasePath)
                    sMHT = scale.Comment    ' D4049
                    sTitle = String.Format("Scale description ({0})", scale.Name)  ' D4049
                    fUpdated = CurrentProject.SaveStructure("Edit infodoc", False, True, String.Format("Edit Scale '{0}' description", scale.Name)) ' D4311
                End If
                ' D2249 ==

                ' D3695 ===
            Case reObjectType.QuickHelp
                Dim tEvalType As ecEvaluationStepType = CheckVar("qh", CInt(ecEvaluationStepType.Other))
                Dim tStep As Integer = CheckVar("step", 0)  ' D3699
                Dim tNode As clsNode = GetNode(False)
                Dim ObjID As String = GetQuickHelpObjectID(tEvalType, tNode)
                Dim sObjName As String = "" ' D4081
                If tNode IsNot Nothing Then sObjName = GetNodeTypeAndName(tNode) ' D4081 + D4082
                If sBasePath = "" Then sBasePath = Infodoc_Path(CurrentProject.ID, CurrentProject.ProjectManager.ActiveHierarchy, _ObjectType, ObjID, -1) ' D4050
                ' D3720 ===
                Dim fAutoShow As Boolean = CheckVar("autoshow", QuickHelpAutoShow)  ' D3741
                Dim sInfodoc As String = Infodoc_Pack(sContent, sBaseURL, sBasePath)
                Dim sApplyTo As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("applyto", ""))   'Anti-XSS
                sMHT = sInfodoc  ' D4049
                sTitle = "Quick help"
                If tNode IsNot Nothing Then sTitle = String.Format("{0} ({1}/{2})", sTitle, GetNodeTypeAndName(tNode), tEvalType) Else sTitle = String.Format("{0} ({1})", sTitle, tEvalType) ' D4049 + D4085
                If sApplyTo <> "" Then
                    ' D4082 ===
                    Dim sSteps As String() = sApplyTo.Split(",")
                    For Each sID As String In sSteps
                        Dim tmpStep As Integer
                        If Integer.TryParse(sID, tmpStep) Then
                            If tmpStep > 0 AndAlso tStep <= CurrentProject.ProjectManager.Pipe.Count Then
                                CurrentProject.PipeParameters.PipeMessages.SetEvaluationQuickHelpText(CurrentProject.ProjectManager, tmpStep, True, fAutoShow, sInfodoc)
                            End If
                        End If
                    Next
                    ' D4082 ==
                Else
                    Select Case tEvalType
                        Case ecEvaluationStepType.RiskResults, ecEvaluationStepType.HeatMap
                            CurrentProject.PipeParameters.PipeMessages.SetEvaluationQuickHelpForCustom(CurrentProject.ProjectManager, tEvalType, CheckVar("ID", -1), fAutoShow, sInfodoc)
                        Case Else
                            CurrentProject.PipeParameters.PipeMessages.SetEvaluationQuickHelpText(CurrentProject.ProjectManager, tStep, False, fAutoShow, sInfodoc)  ' D3699 + D3741 + D4079 + D4081
                    End Select
                End If
                ' D3720 ==
                fUpdated = CurrentProject.PipeParameters.PipeMessages.Save(PipeStorageType.pstStreamsDatabase, CurrentProject.ConnectionString, CurrentProject.ProviderType, CurrentProject.ID)
                If fUpdated AndAlso App.isAuthorized AndAlso App.HasActiveProject Then   ' D3992
                    CheckShow_QuickHelp(sInfodoc)   ' D3738
                    'App.DBSaveLog(dbActionType.actModify, dbObjectType.einfProject, CurrentProject.ID, sTitle, "")
                    App.SaveProjectLogEvent(CurrentProject, "Edit quick help", False, String.Format("{0}{1}", tEvalType.ToString, IIf(sObjName = "", "", " " + sObjName)))   ' D3572 + D3734 + D4081
                End If
                ' D3695 ==

                ' D4283 ===
            Case reObjectType.Attribute
                Dim tGUID As Guid = Str2GUID(CheckVar("guid", ""))
                If Not tGUID.Equals(Guid.Empty) Then
                    Dim tAttr As clsAttribute = CurrentProject.ProjectManager.Attributes.GetAttributeByID(tGUID)
                    If tAttr IsNot Nothing Then
                        sBasePath = Infodoc_Path(CurrentProject.ID, CurrentProject.ProjectManager.ActiveHierarchy, _ObjectType, tAttr.ID.ToString, -1)
                        sContent = Infodoc_Pack(sContent, sBaseURL, sBasePath)
                        sMHT = sContent
                        CurrentProject.ProjectManager.InfoDocs.SetCustomInfoDoc(sContent, tAttr.ID, Guid.Empty)
                        fUpdated = CurrentProject.ProjectManager.StorageManager.Writer.SaveInfoDocs()
                    End If
                End If

            Case reObjectType.AttributeValue
                Dim tGUID As Guid = Str2GUID(CheckVar("guid", ""))
                If Not tGUID.Equals(Guid.Empty) Then
                    Dim tAttr As clsAttributeEnumeration = CurrentProject.ProjectManager.Attributes.GetEnumByID(tGUID)
                    If tAttr IsNot Nothing Then
                        sBasePath = Infodoc_Path(CurrentProject.ID, CurrentProject.ProjectManager.ActiveHierarchy, _ObjectType, tAttr.ID.ToString, -1)
                        sContent = Infodoc_Pack(sContent, sBaseURL, sBasePath)
                        sMHT = sContent
                        CurrentProject.ProjectManager.InfoDocs.SetCustomInfoDoc(sContent, tAttr.ID, Guid.Empty)
                        fUpdated = CurrentProject.ProjectManager.StorageManager.Writer.SaveInfoDocs()
                    End If
                End If

            Case reObjectType.ResultGroup
                Dim tID As Integer = CheckVar("id", Integer.MinValue)
                If tID <> Integer.MinValue Then
                    Dim tGrp As clsCombinedGroup = CurrentProject.ProjectManager.CombinedGroups.GetGroupByCombinedID(tID)
                    If tGrp IsNot Nothing Then
                        sBasePath = Infodoc_Path(CurrentProject.ID, CurrentProject.ProjectManager.ActiveHierarchy, _ObjectType, tGrp.ID.ToString, -1)
                        sContent = Infodoc_Pack(sContent, sBaseURL, sBasePath)
                        sMHT = sContent
                        Dim tGUID As New Guid(tID, CShort(reObjectType.ResultGroup), 0, 0, 0, 0, 0, 0, 0, 0, 0)
                        CurrentProject.ProjectManager.InfoDocs.SetCustomInfoDoc(sContent, tGUID, Guid.Empty)
                        fUpdated = CurrentProject.ProjectManager.StorageManager.Writer.SaveInfoDocs()
                    End If
                End If
                ' D4283 ==

                ' D4344 ===
            Case reObjectType.Control
                Dim tGUID As Guid = Str2GUID(CheckVar("guid", ""))
                If Not tGUID.Equals(Guid.Empty) Then
                    Dim tCtrl As clsControl = CurrentProject.ProjectManager.Controls.GetControlByID(tGUID)
                    If tCtrl IsNot Nothing Then
                        fUpdated = SetControlInfodoc(CurrentProject, tCtrl, sContent, sBaseURL, ParseString("%%Risk%% %%Controls%%: edit description"), sMHT)   ' D4345
                    End If
                End If
                ' D4344 ==

                ' D7011 ===
            Case reObjectType.DashboardInfodoc
                Dim tDash As Integer = CheckVar("dash", Integer.MinValue)
                If tDash <> Integer.MinValue AndAlso CurrentProject.ProjectManager.Reports.Reports.ContainsKey(tDash) Then
                    Dim D As clsReport = CurrentProject.ProjectManager.Reports.Reports(tDash)
                    Dim tID As Integer = CheckVar("id", Integer.MinValue)
                    If D.Items.ContainsKey(tID) Then
                        Dim Item = D.Items(tID)
                        Dim tGUID As New Guid(tID, CShort(reObjectType.DashboardInfodoc), CShort(tDash), 0, 0, 0, 0, 0, 0, 0, 0)
                        If HTML2Text(sContent).Trim = ResString("templDashboardInformationEmpty") Then sContent = ""    ' D7391
                        sBasePath = Infodoc_Path(CurrentProject.ID, CurrentProject.ProjectManager.ActiveHierarchy, _ObjectType, tID.ToString, tDash)
                        sContent = Infodoc_Pack(sContent, sBaseURL, sBasePath)
                        sMHT = sContent
                        CurrentProject.ProjectManager.InfoDocs.SetCustomInfoDoc(sContent, tGUID, Guid.Empty)
                        fUpdated = CurrentProject.ProjectManager.StorageManager.Writer.SaveInfoDocs()
                    End If
                End If
                ' D7011 ==

        End Select

        If fUpdated AndAlso CurrentProject IsNot Nothing Then
            CurrentProject.ProjectManager.LastModifyTime = Now ' D1292
            App.DBUpdateDateTime(clsComparionCore._TABLE_PROJECTS, clsComparionCore._FLD_PROJECTS_LASTMODIFY, CurrentProject.ID)    ' D2453
        End If

        Return fUpdated
    End Function
    ' D0107 ==

    Private Function GetNode(fIsParentNode As Boolean) As clsNode   ' D1003
        Dim tNode As clsNode = Nothing

        ' D0701 ===
        Dim sNodeGUID As String = EcSanitizer.GetSafeHtmlFragment(CheckVar(CStr(IIf(fIsParentNode, "pguid", "guid")), ""))  ' D1003 + Anti-XSS
        Dim tmpObjectType As reObjectType = IIf(fIsParentNode, reObjectType.Node, _ObjectType)  ' D1003
        If sNodeGUID <> "" Then
            Try ' D1299
                Dim tGUID As Guid = Str2GUID(sNodeGUID)    ' D6704
                Select Case tmpObjectType   ' D1003
                    Case reObjectType.Alternative, reObjectType.AltWRTNode, reObjectType.QuickHelp    ' D1004 + D3695
                        tNode = CurrentProject.HierarchyAlternatives.GetNodeByID(tGUID) ' D4228
                        If tNode Is Nothing Then tNode = CurrentProject.HierarchyObjectives.GetNodeByID(tGUID) ' D2412 + D4228
                    Case reObjectType.Node
                        ' D4228 ===
                        tNode = CurrentProject.HierarchyObjectives.GetNodeByID(tGUID)
                        If tNode Is Nothing AndAlso App.isRiskEnabled Then
                            tNode = CurrentProject.ProjectManager.Hierarchy(CInt(IIf(CurrentProject.isImpact, ECHierarchyID.hidLikelihood, ECHierarchyID.hidImpact))).GetNodeByID(tGUID)
                        End If
                        ' D4228 ==
                End Select
            Catch ex As Exception   ' D1299
            End Try
        End If

        If tNode Is Nothing Then
            ' D0701 ==
            Dim NodeID As Integer = -1
            Dim sID As String = CheckVar(IIf(fIsParentNode, "pid", _PARAM_ID), "")   ' D4228
            If Not String.IsNullOrEmpty(sID) AndAlso sID.Length < 10 AndAlso Integer.TryParse(sID, NodeID) Then ' D1003 + D4228
                Select Case tmpObjectType ' D0107 + D1003
                    Case reObjectType.Alternative, reObjectType.AltWRTNode   ' D1004
                        If Not HasPermission(_PGID_STRUCTURE_ALTERNATIVES, CurrentProject) Then Return Nothing ' D0483 + D1146
                        tNode = CurrentProject.HierarchyAlternatives.GetNodeByID(NodeID)
                        If tNode Is Nothing Then tNode = GetNodeByID(CurrentProject.HierarchyObjectives.Nodes, NodeID) ' D3473
                    Case reObjectType.Node
                        If Not HasPermission(_PGID_STRUCTURE_HIERARCHY, CurrentProject) Then Return Nothing ' D0483 + D1146
                        tNode = CurrentProject.HierarchyObjectives.GetNodeByID(NodeID)
                End Select
            End If
        End If
        Return tNode
    End Function

    ' D1083 ===
    Private Function GetVirtualNode() As clsVisualNode
        'If Not IsPostBack AndAlso Not IsCallback Then
        With CurrentProject ' D1146
            '.ResetProject(True)
            .ProjectManager.AntiguaDashboard.LoadPanel(clsProject.StorageType, .ConnectionString, .ProviderType, .ID)
            .ProjectManager.AntiguaInfoDocs.LoadAntiguaInfoDocs()
        End With
        'End If
        Dim tNode As clsVisualNode = Nothing
        Dim sNodeGUID As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("guid", "")) 'Anti-XSS
        If sNodeGUID <> "" Then
            Try ' D1299 
                Dim tGUID As Guid = Str2GUID(sNodeGUID)    ' D6704
                tNode = CurrentProject.ProjectManager.AntiguaDashboard.GetNodeByGuid(tGUID)
            Catch ex As Exception   ' D1299
            End Try
        End If
        Return tNode
    End Function
    ' D1083 ==

    ' D2249 ===
    Private Function GetCurrentScale(ByRef MType As ECMeasureType) As clsMeasurementScale
        Dim Scale As clsMeasurementScale = Nothing
        Dim sScale As String = EcSanitizer.GetSafeHtmlFragment(CheckVar(_PARAM_ID, "")) 'Anti-XSS
        If sScale.Length < 8 Then sScale = EcSanitizer.GetSafeHtmlFragment(CheckVar("guid", "")) ' D3543 + Anti-XSS
        If sScale <> "" Then    ' D2251
            Try
                Dim tScale As Guid = New Guid(sScale)
                Scale = CurrentProject.ProjectManager.MeasureScales.GetScaleByID(tScale) 'A1594
                '-A1594 Dim sType As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("scale", CInt(ECMeasureType.mtRatings).ToString)) ' D3543 + Anti-XSS
                '-A1594 If Integer.TryParse(sType, MType) AndAlso tScale <> Guid.Empty Then
                '-A1594     Select Case CType(MType, ECMeasureType)
                '-A1594         Case ECMeasureType.mtRatings
                '-A1594             Dim tRS As clsRatingScale = CurrentProject.ProjectManager.MeasureScales.GetRatingScaleByID(tScale)
                '-A1594             If tRS IsNot Nothing Then Scale = tRS
                '-A1594     End Select
                '-A1594 End If
            Catch ex As Exception
            End Try
        End If
        Return Scale
    End Function
    ' D2249 ==

    ' D0105 ===
    Private Function IsInfoDoc() As Boolean
        Return CheckVar("field", "").ToLower = "infodoc"    ' D0107
    End Function
    ' D0105 ==

    'Public Sub ImageUploadBtn_Click(sender As Object, e As EventArgs)
    '    MessageOnLoad = "Unknown file or unsupported. Use .PNG, .GIF, or .JPG file images."
    '    If ImageUpload.HasFile Then
    '        ' D5071 ===
    '        Dim FileName As String = ImageUpload.PostedFile.FileName
    '        Select Case System.IO.Path.GetExtension(FileName).ToLower()
    '            Case ".png", ".gif", ".jpg", ".jpeg"
    '                MessageOnLoad = ""
    '                Dim sImgName As String = RemoveXssFromUrl(System.IO.Path.GetFileName(FileName)) ' D6767
    '                If (ImageUpload.PostedFile.ContentLength <= _OPT_IMG_MAXSIZE) Then
    '                    ImageUpload.SaveAs(String.Format("{0}/{1}", Server.MapPath(InfodocPath.Value), sImgName))

    '                    Dim sContent As String = RemoveXssFromText(TinyMCEEditor.Text.ToString())   ' D6766
    '                    Dim sImageTag As String = String.Format("<img src = '{0}/{1}'/>", InfodocPath.Value, sImgName)
    '                    If sContent.Contains("</body>") Then
    '                        Dim index = sContent.IndexOf("</body>")
    '                        sContent = sContent.Insert(index, sImageTag)
    '                    Else
    '                        sContent += sImageTag
    '                    End If
    '                    TinyMCEEditor.Text = sContent
    '                Else
    '                    MessageOnLoad = "File is too big. Limit is " + SizeString(_OPT_IMG_MAXSIZE)
    '                End If
    '        End Select
    '        ' D5071 ==
    '    End If
    'End Sub

    ' D4936 ===
    Public Function GetDummyTemplates() As String
        Dim sResult = ""
        Dim Lst As String() = _TEMPL_LIST_ALL(App.isRiskEnabled)
        For i As Integer = 0 To Lst.Length - 1
            Dim tp As String = Lst(i)
            If tp.StartsWith("%%url_") AndAlso tp <> _TEMPL_URL_APP AndAlso tp <> _TEMPL_URL_MEETINGID AndAlso (Not isSSO() OrElse tp <> _TEMPL_URL_RESETPSW) Then  ' D6552
                sResult += String.Format("{0}""{1}"":""{2}""", If(sResult = "", "", ", "), JS_SafeString(tp.Replace("%%", "")), "[url will be here]")
            End If
        Next
        If sResult <> "" Then sResult = String.Format("if ((_templates)) {{ var _tmp = $.extend({{}}, _templates, {{{0}}}); _templates = _tmp; }}" + vbCrLf, sResult)
        Return sResult
    End Function
    ' D4936 ==

End Class
