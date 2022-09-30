Public Class SensitivitiesAnalysis
    Inherits System.Web.UI.UserControl

    Public msgSeeingCombined As String = "combined"
    Public msgSeeingIndividual As String = "individual"
    Public msgSeeingUser As String = "user"
    Public ProjectManager As clsProjectManager = Nothing
    Public Opt_ShowMaxAltsCount As Integer = -1
    Private _Current_UserID As Integer = Integer.MinValue
    Private _SA_UserID As Integer = Integer.MinValue
    Private _NodesList As Dictionary(Of Integer, String) = Nothing
    Public AltColors As String() = {"#344d94", "#cb3034", "#9d27a8", "#e3e112", "#00687d", "#407000", "#f24961", "#663d2e", "#9600fa", "#ffbde6", "#00c49f", "#7280c4", "#009180", "#e33000", "#80bdff", "#a10040", "#0affe3", "#00523c", "#919100", "#5c00f7", "#a15f00", "#cce6ff", "#00465c", "#adff69", "#f24ba0", "#0dff87", "#ff8c47", "#349400", "#b3b3a1", "#a10067", "#ba544a", "#edc2d1", "#00e8c3", "#3f0073", "#5ec1f7", "#6e00b8", "#f5f5c4", "#e33000", "#52ba00", "#ff943b", "#0079db", "#f0e6c0", "#ffb517", "#cf0076", "#e8cfc9"}
    Public ObjPriorities As Dictionary(Of Integer, Double) = New Dictionary(Of Integer, Double)()
    Public AltValues As Dictionary(Of Integer, Double) = New Dictionary(Of Integer, Double)()
    Public AltValuesInOne As Dictionary(Of Integer, Dictionary(Of Integer, Double)) = New Dictionary(Of Integer, Dictionary(Of Integer, Double))()
    Public AltValuesInZero As Dictionary(Of Integer, Dictionary(Of Integer, Double)) = New Dictionary(Of Integer, Dictionary(Of Integer, Double))()

    Public Const ACTION_DSA_UPDATE_VALUES As String = "dsa_update_values"
    Public Const ACTION_DSA_RESET As String = "dsa_reset"
    Public msgNoEvaluationData As String = "no data for {0}"
    Public msgNoGroupData As String = "no group data"
    Public msgHint As String = "drag bars"
    Public lblNormalization As String = "Normaization: "
    Public lblSeeing As String = ""
    Public lblMessage As String = ""
    Public lblSelectNode As String = "Select: "
    Public lblRefreshCaption As String = "Refresh"
    Public lblKeepSortedAlts As String = "Freeze order of alternatives (?)"
    Public lblShowLines As String = "Show lines"
    Public lblLineUp As String = "Align Labels"
    Public lblShowLegend As String = "Show Legend"
    Public pnlLoadingID As String = ""
    Public NormalizationsList As Dictionary(Of AlternativeNormalizationOptions, String) = New Dictionary(Of AlternativeNormalizationOptions, String)()
    Public Opt_ShowYouAreSeeing As Boolean = True
    Public Opt_isMobile As Boolean = False
    Private _SA_Data As clsSensitivityAnalysisActionData = Nothing
    Private _NormalizationMode As AlternativeNormalizationOptions = AlternativeNormalizationOptions.anoPercentOfMax
    Public Const _SESS_SA_NORMALIZATION As String = "SANormMode"
    Public Const _SESS_SA_WRT_NODE As String = "SAWrtNode"
    Public Const _OPT_IGNORE_CATEGORIES As Boolean = True

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    End Sub

    'get details for sensitive analysis
    Public Shared Function GetDetails(ByVal ActionData As Object, ByVal App As clsComparionCore) As SensitivityAnalysisModel
        Dim model As SensitivityAnalysisModel = New SensitivityAnalysisModel()

        model.StepTask = ""
        Dim sensitivities As clsSensitivityAnalysisActionData = CType(ActionData, clsSensitivityAnalysisActionData)
        Dim sensitivitiesAnalysis As SensitivitiesAnalysis = New SensitivitiesAnalysis()
        sensitivitiesAnalysis.clearData()
        sensitivitiesAnalysis.CurrentUserID = App.ActiveProject.ProjectManager.UserID

        If App.Options.BackDoor = _BACKDOOR_PLACESRATED Then
            sensitivitiesAnalysis.Opt_ShowMaxAltsCount = 10
            sensitivitiesAnalysis.SAUserID = App.ActiveProject.ProjectManager.UserID
        Else
            sensitivitiesAnalysis.SAUserID = (If(App.ActiveProject.PipeParameters.CalculateSAForCombined, ECTypes.COMBINED_USER_ID, App.ActiveProject.ProjectManager.UserID))
        End If

        model.saType = sensitivities.SAType.ToString()

        Select Case sensitivities.SAType
            Case SAType.satDynamic
                model.StepTask = TeamTimeClass.ResString("lblEvaluationDynamicSA")
                model.qh_help_id = Canvas.PipeParameters.ecEvaluationStepType.DynamicSA
            Case SAType.satGradient
                model.StepTask = TeamTimeClass.ResString("lblEvaluationGradientSA")
                model.qh_help_id = Canvas.PipeParameters.ecEvaluationStepType.GradientSA
            Case SAType.satPerformance
                model.StepTask = TeamTimeClass.ResString("lblEvaluationPerformanceSA")
                model.qh_help_id = Canvas.PipeParameters.ecEvaluationStepType.PerformanceSA
        End Select

        Dim sSeeing As String = ""
        Dim SAUserID = sensitivitiesAnalysis.SAUserID
        Dim msgSeeingCombined = sensitivitiesAnalysis.msgSeeingCombined
        Dim CurrentUserID = sensitivitiesAnalysis.CurrentUserID
        Dim msgSeeingIndividual = sensitivitiesAnalysis.msgSeeingIndividual
        Dim ProjectManager = sensitivitiesAnalysis.ProjectManager
        Dim msgSeeingUser = sensitivitiesAnalysis.msgSeeingUser

        If sensitivitiesAnalysis.SAUserID = ECTypes.COMBINED_USER_ID Then
            sSeeing = msgSeeingCombined
        ElseIf SAUserID = CurrentUserID Then
            sSeeing = msgSeeingIndividual
        Else

            If SAUserID <> Integer.MinValue AndAlso ProjectManager.User IsNot Nothing Then
                Dim sUserEmail As String = ""
                Dim tUser As ECTypes.clsUser = ProjectManager.GetUserByID(SAUserID)
                If (tUser IsNot Nothing) Then sUserEmail = tUser.UserEMail
                sSeeing = String.Format(msgSeeingUser, sUserEmail)
            End If
        End If

        model.StepTask += If(sSeeing <> "", " " & sSeeing, "")

        Return model
    End Function

    'cleaer object and property data
    Public Sub clearData()
        ObjPriorities.Clear()
        AltValues.Clear()
        ProjectManager = Nothing
        AltValuesInZero.Clear()
        AltValuesInOne.Clear()
        SAUserID = Integer.MinValue
        CurrentUserID = Integer.MinValue
        _NodesList = Nothing
    End Sub

    Public Property CurrentUserID As Integer
        Get
            If _Current_UserID = Integer.MinValue AndAlso ProjectManager IsNot Nothing Then
                _Current_UserID = ProjectManager.UserID
            End If
            Return _Current_UserID
        End Get
        Set(ByVal value As Integer)
            _Current_UserID = value
        End Set
    End Property

    Public Property SAUserID As Integer
        Get
            If _SA_UserID = Integer.MinValue AndAlso ProjectManager IsNot Nothing Then
                _SA_UserID = ProjectManager.UserID
            End If
            Return _SA_UserID
        End Get
        Set(ByVal value As Integer)
            _SA_UserID = value
        End Set
    End Property

    Public Sub SetSaUserId(ByVal app As clsComparionCore)
        If app.Options.BackDoor = ExpertChoice.Web.Options._BACKDOOR_PLACESRATED Then
            Opt_ShowMaxAltsCount = 10
            SAUserID = app.ActiveProject.ProjectManager.UserID
        Else
            SAUserID = (If(app.ActiveProject.PipeParameters.CalculateSAForCombined, ECTypes.COMBINED_USER_ID, app.ActiveProject.ProjectManager.UserID))
        End If
    End Sub

    Public Property CurrentNode As clsNode
        Get
            Dim context As HttpContext = HttpContext.Current
            Dim NodeID As Integer = -1

            If context.Session(_SESS_SA_WRT_NODE + ModelID()) IsNot Nothing Then
                NodeID = Convert.ToInt32(context.Session(_SESS_SA_WRT_NODE + ModelID()))
            End If

            Dim tNode As clsNode = Nothing

            If ProjectManager IsNot Nothing Then
                tNode = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).GetNodeByID(NodeID)
                If tNode Is Nothing AndAlso ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).Nodes.Count > 0 Then tNode = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).Nodes(0)
            End If

            Return tNode
        End Get
        Set(ByVal value As clsNode)

            If ProjectManager IsNot Nothing AndAlso value IsNot Nothing Then
                HttpContext.Current.Session.Remove(_SESS_SA_WRT_NODE + ModelID())
                HttpContext.Current.Session.Add(_SESS_SA_WRT_NODE + ModelID(), value.NodeID)
            End If
        End Set
    End Property

    Friend Function ModelID() As String
        If ProjectManager Is Nothing Then
            Return ""
        Else
            Return String.Format("_{0}", ProjectManager.StorageManager.ModelID)
        End If
    End Function

    Friend Function GetSAData() As String
        Return String.Format("[{0}, {1}]", GetSAObjectives(), GetSAAlternatives())
    End Function

    Public Function GetSAObjectives() As String
        Dim retVal As String = ""
        Dim App = CType(HttpContext.Current.Session("App"), clsComparionCore)
        ProjectManager = App.ActiveProject.ProjectManager
        Dim H As clsHierarchy = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy)
        Dim altH As clsHierarchy = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy)
        initSAData()

        If ObjPriorities.Count > 0 And altH IsNot Nothing And H IsNot Nothing Then
            Dim wrtNode As clsNode = CurrentNode

            If wrtNode IsNot Nothing Then
                Dim i = 0

                For Each obj As clsNode In wrtNode.GetNodesBelow(SAUserID)
                    Dim AttrGuid As Guid = ATTRIBUTE_DEFAULT_BRUSH_COLOR_OBJECTIVE_ID

                    If ProjectManager.ActiveHierarchy = CInt(ECTypes.ECHierarchyID.hidImpact) Then
                        AttrGuid = ECCore.ATTRIBUTE_DEFAULT_BRUSH_COLOR_IMPACT_ID
                    End If

                    Dim sNodeColor As String = ""
                    Dim tNodeColor As Long = Convert.ToInt64(ProjectManager.Attributes.GetAttributeValue(AttrGuid, obj.NodeGuidID))
                    sNodeColor = If(tNodeColor > 0, LongToBrush(tNodeColor), GetPaletteColor(CurrentPaletteID(ProjectManager), obj.NodeID))
                    Dim gradientMaxValues As String = ""
                    Dim gradientMinValues As String = ""

                    For Each altID As Integer In AltValuesInOne(obj.NodeID).Keys
                        gradientMaxValues += Convert.ToString(If(Not String.IsNullOrEmpty(gradientMaxValues), ",", "")) & $"{{altID:{altID},val:{JS_SafeNumber(AltValuesInOne(obj.NodeID)(altID))}}}"
                    Next

                    For Each altID As Integer In AltValuesInZero(obj.NodeID).Keys
                        gradientMinValues += Convert.ToString(If(Not String.IsNullOrEmpty(gradientMinValues), ",", "")) & $"{{altID:{altID},val:{JS_SafeNumber(AltValuesInZero(obj.NodeID)(altID))}}}"
                    Next

                    retVal += Convert.ToString((If(Not String.IsNullOrEmpty(retVal), ",", ""))) & $"{{id:{obj.NodeID},idx:{i},name:'{JS_SafeString(obj.NodeName)}',value:{JS_SafeNumber(ObjPriorities(obj.NodeID))},initValue:{JS_SafeNumber(ObjPriorities(obj.NodeID))},gradientMaxValues:[{gradientMaxValues}],gradientMinValues:[{gradientMinValues}],gradientInitMinValues:[{gradientMinValues}],color:'{sNodeColor}'}}"
                    i += 1
                Next
            End If
        End If

        retVal = $"[{retVal}]"
        Return retVal
    End Function

    Public Function GetSAAlternatives() As String
        Dim retVal As String = ""
        Dim altH As ECCore.clsHierarchy = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy)
        initSAData()

        If AltValues.Count > 0 And altH IsNot Nothing Then
            Dim i As Integer = 0

            For Each alt As clsNode In altH.TerminalNodes()
                Dim altColor = Convert.ToInt64(ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_DEFAULT_BRUSH_COLOR_ID, alt.NodeGuidID))
                Dim sAltColor = If(altColor > 0, LongToBrush(altColor), GetPaletteColor(CurrentPaletteID(ProjectManager), alt.NodeID, True))
                retVal += Convert.ToString(If(Not String.IsNullOrEmpty(retVal), ",", "")) & String.Format("{{id:{0},idx:{1},name:'{2}',value:{3},initValue:{4},color:'{5}',visible:1}}", alt.NodeID, i, JS_SafeString(alt.NodeName), JS_SafeNumber(AltValues(alt.NodeID)), JS_SafeNumber(AltValues(alt.NodeID)), sAltColor)
                i += 1
            Next
        End If

        retVal = String.Format("[{0}]", retVal)
        Return retVal
    End Function

    Friend Sub initSAData()
        If ObjPriorities.Count = 0 Then
            Dim H As ECCore.clsHierarchy = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy)

            If H IsNot Nothing Then
                Dim wrtNode As ECCore.clsNode = CurrentNode

                If wrtNode IsNot Nothing Then
                    ObjPriorities.Clear()
                    AltValues.Clear()
                    AltValuesInOne.Clear()
                    ProjectManager.CalculationsManager.InitializeSAGradient(wrtNode.NodeID, False, SAUserID, ObjPriorities, AltValues, AltValuesInOne, 0)
                    AltValuesInZero = ProjectManager.CalculationsManager.GetGradientData(wrtNode.NodeID, False, SAUserID, ObjPriorities, 0)
                End If
            End If
        End If
    End Sub

    Public Property NormalizationMode As AlternativeNormalizationOptions
        Get
            Dim context As HttpContext = HttpContext.Current

            If context.Session(_SESS_SA_NORMALIZATION + ModelID()) IsNot Nothing Then
                _NormalizationMode = CType(context.Session(_SESS_SA_NORMALIZATION + ModelID()), AlternativeNormalizationOptions)
            End If

            Return _NormalizationMode
        End Get
        Set(ByVal value As AlternativeNormalizationOptions)
            _NormalizationMode = value
            HttpContext.Current.Session.Remove(_SESS_SA_NORMALIZATION + ModelID())
            HttpContext.Current.Session.Add(_SESS_SA_NORMALIZATION + ModelID(), _NormalizationMode)
        End Set
    End Property

    Friend Sub updateAltValuesinZero(ByVal NewObjPriorities As Dictionary(Of Integer, Double))
        Dim App = CType(HttpContext.Current.Session("App"), clsComparionCore)
        ProjectManager = App.ActiveProject.ProjectManager
        Dim H As ECCore.clsHierarchy = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy)

        If H IsNot Nothing Then
            Dim wrtNode As ECCore.clsNode = CurrentNode

            If wrtNode IsNot Nothing Then
                AltValuesInZero = ProjectManager.CalculationsManager.GetGradientData(wrtNode.NodeID, False, SAUserID, NewObjPriorities, 0)
            End If
        End If
    End Sub

    Public Function initSA() As initSAModel
        Dim initSAModel As initSAModel = New initSAModel()
        Dim App = CType(HttpContext.Current.Session("App"), clsComparionCore)
        Dim WorkSpace = CType(App.ActiveWorkspace, clsWorkspace)
        Dim CurrentStep = WorkSpace.ProjectStep(App.ActiveProject.isImpact)
        Dim AnytimeAction = CType(AnytimeClass.GetAction(CurrentStep), clsAction)
        CurrentUserID = App.ActiveProject.ProjectManager.UserID
        SetSaUserId(App)

        If AnytimeAction.ActionType = ActionType.atSensitivityAnalysis Then
            Dim sensitivities As clsSensitivityAnalysisActionData = CType(AnytimeAction.ActionData, clsSensitivityAnalysisActionData)
            _SA_Data = sensitivities
            lblKeepSortedAlts = TeamTimeClass.ResString("lblSAKeppSorted")
            lblRefreshCaption = TeamTimeClass.ResString("btnReset")
            lblShowLines = TeamTimeClass.ResString("lblSAShowLines")
            lblLineUp = TeamTimeClass.ResString("lblSALineUp")
            lblShowLegend = TeamTimeClass.ResString("lblSAShowLegend")
            lblSelectNode = TeamTimeClass.PrepareTask(TeamTimeClass.ResString("lblSASelectNode"))
            msgHint = TeamTimeClass.PrepareTask(TeamTimeClass.GetPipeStepTask(AnytimeClass.Action(CurrentStep), Nothing))
            msgSeeingCombined = TeamTimeClass.ResString("lblSASeeingCombined")
            msgSeeingIndividual = TeamTimeClass.ResString("lblSASeeingIndividual")
            msgSeeingUser = TeamTimeClass.ResString("lblSASeeingForUser")
            InitComponent()
        End If

        initSAModel.lblSeeing = lblSeeing
        initSAModel.lblMessage = lblMessage
        initSAModel.GetSAObjectives = GetSAObjectives()
        initSAModel.GetSAAlternatives = GetSAAlternatives()
        initSAModel.GetDecimalsValue = GetDecimalsValue()
        initSAModel.GetOptions = GetOptions()
        initSAModel.GetNodesList = GetNodesList()
        initSAModel.GetGSASubobjectives = GetGSASubobjectives()
        initSAModel.msgHint = msgHint
        initSAModel.ACTION_DSA_UPDATE_VALUES = ACTION_DSA_UPDATE_VALUES
        initSAModel.ACTION_DSA_RESET = ACTION_DSA_RESET
        initSAModel.GetSATypeString = GetSATypeString()
        initSAModel.lblRefreshCaption = lblRefreshCaption
        initSAModel.Opt_ShowYouAreSeeing = Opt_ShowYouAreSeeing
        initSAModel.pnlLoadingID = pnlLoadingID
        initSAModel.sensitivities = _SA_Data
        'Dim oSerializer = New System.Web.Script.Serialization.JavaScriptSerializer()
        'Return oSerializer.Serialize(output)
        Return initSAModel
    End Function

    Public Sub InitComponent()
        Dim sMessage As String = ""
        Dim App = CType(HttpContext.Current.Session("App"), clsComparionCore)
        ProjectManager = App.ActiveProject.ProjectManager

        If ProjectManager IsNot Nothing Then

            If NodesList.Count > 0 Then
                Dim fIsCombined As Boolean = ECTypes.IsCombinedUserID(SAUserID)
                Dim fCanShowIndividual As Boolean = False
                Dim fCanShowGlobal As Boolean = False
                Dim PrjAnalyzer As clsJudgmentsAnalyzer = New clsJudgmentsAnalyzer(ProjectManager.PipeParameters.SynthesisMode, ProjectManager)

                If Not fIsCombined Then
                    fCanShowIndividual = PrjAnalyzer.CanShowIndividualResults(SAUserID, CurrentNode)
                Else
                    fCanShowGlobal = PrjAnalyzer.CanShowGroupResults(CurrentNode)
                End If

                PrjAnalyzer = Nothing

                If (fIsCombined AndAlso Not fCanShowGlobal) Or (Not fIsCombined AndAlso Not fCanShowIndividual) Then
                    sMessage = msgNoGroupData
                Else

                    If CurrentNode IsNot Nothing Then

                        If Not fIsCombined AndAlso Not fCanShowIndividual Then
                            sMessage = String.Format(msgNoEvaluationData, CurrentNode.NodeName)
                        End If

                        If Opt_ShowYouAreSeeing Then
                            Dim sSeeing As String = ""

                            If SAUserID = ECTypes.COMBINED_USER_ID Then
                                sSeeing = msgSeeingCombined
                            ElseIf SAUserID = CurrentUserID Then
                                sSeeing = msgSeeingIndividual
                            Else

                                If SAUserID <> Integer.MinValue AndAlso ProjectManager.User IsNot Nothing Then
                                    Dim sUserEmail As String = ""
                                    Dim tUser As ECTypes.clsUser = ProjectManager.GetUserByID(SAUserID)
                                    If (tUser IsNot Nothing) Then sUserEmail = tUser.UserEMail
                                    sSeeing = String.Format(msgSeeingUser, sUserEmail)
                                End If
                            End If

                            If Not String.IsNullOrEmpty(sSeeing) Then
                                lblSeeing = sSeeing
                            Else
                                Opt_ShowYouAreSeeing = False
                            End If
                        End If
                    End If
                End If
            Else
                sMessage = msgNoGroupData
            End If

            lblMessage = sMessage
        End If
    End Sub

    Public ReadOnly Property NodesList As Dictionary(Of Integer, String)
        Get

            If _NodesList Is Nothing Then
                _NodesList = New Dictionary(Of Integer, String)()
                Dim tRoot As clsNode = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).Nodes(0)

                Select Case SAType
                    Case Else
                        GetOverTerminalNodes(_NodesList, tRoot, "")
                End Select
            End If

            Return _NodesList
        End Get
    End Property

    Public ReadOnly Property SAType As SAType
        Get

            If Data Is Nothing Then
                Return SAType.satNone
            Else
                Return Data.SAType
            End If
        End Get
    End Property

    Public Property Data As clsSensitivityAnalysisActionData
        Get
            Return _SA_Data
        End Get
        Set(ByVal value As clsSensitivityAnalysisActionData)
            _SA_Data = value
        End Set
    End Property

    Public Function GetDecimalsValue() As Integer
        Return Convert.ToInt32(ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_SYNTHESIS_DECIMALS_ID, UNDEFINED_USER_ID))
    End Function

    Public Function GetOptions() As String
        Dim sRes As String = ""

        If SAType = Canvas.SAType.satDynamic Then
            sRes += String.Format("&nbsp;<label><input type=checkbox class='checkbox' name='cbKeepSorted' value='1' onclick='onKeepSorted(this.checked);' {0}>{1}</label>", "", lblKeepSortedAlts)
        End If

        If SAType = Canvas.SAType.satPerformance Then
            sRes += String.Format("<br /><label class='large-6 small-6 columns'><input type=checkbox class='checkbox' name='cbShowLines' value='1' onclick='onShowLines(this.checked);' {0}>{1}</label>", "checked", lblShowLines)
            sRes += String.Format("<label class='large-6 small-6 columns'><input type=checkbox class='checkbox' name='cbLineUp' value='1' onclick='onLineUp(this.checked);' {0}>{1}</label>", "checked", lblLineUp)
        End If

        If SAType = Canvas.SAType.satGradient Then
            sRes += String.Format("&nbsp;<label><input type=checkbox class='checkbox' name='cbShowLegend' value='1' onclick='onShowLegend(this.checked);' {0}>{1}</label>", "checked", lblShowLegend)
        End If

        Return sRes
    End Function

    Public Function GetNodesList() As String
        Dim sList As String = ""

        If NodesList IsNot Nothing AndAlso NodesList.Count > 1 Then

            For Each ID As Integer In NodesList.Keys
                Dim sActive As String = ""
                If CurrentNode IsNot Nothing AndAlso CurrentNode.NodeID = ID Then sActive = " selected"
                sList += String.Format("<option value='{0}'{1}>{2}</option>", ID, sActive, NodesList(ID))
            Next

            sList = String.Format("{0} <select id='nodes' onchange='onChangeNode(this.value);' style='width:210px'>{1}</option>", lblSelectNode, sList)
        End If

        If String.IsNullOrEmpty(sList) Then sList = ""
        Return sList
    End Function

    Public Function GetGSASubobjectives() As List(Of Object)
        Dim retVal = New List(Of Object)()
        Dim wrtNode As ECCore.clsNode = CurrentNode

        If wrtNode IsNot Nothing Then
            Dim i As Integer = 0

            For Each obj As ECCore.clsNode In wrtNode.GetNodesBelow(SAUserID)
                Dim sActive As String = ""
                Dim list = New List(Of Object)()
                list.Add(i)
                list.Add(sActive)
                list.Add(obj.NodeName)
                retVal.Add(list)
                i += 1
            Next
        End If

        Return retVal
    End Function

    Public Function GetSATypeString() As String
        Select Case SAType
            Case Canvas.SAType.satDynamic
                Return "DSA"
            Case Canvas.SAType.satGradient
                Return "GSA"
            Case Canvas.SAType.satPerformance
                Return "PSA"
        End Select

        Return "None"
    End Function

    Private Sub GetOverTerminalNodes(ByRef tNodesList As Dictionary(Of Integer, String), ByVal pNode As clsNode, ByVal margin As String)
        If pNode IsNot Nothing AndAlso Not pNode.IsTerminalNode AndAlso Not pNode.DisabledForUser(CurrentUserID) Then
            tNodesList.Add(pNode.NodeID, ShortString((If(String.IsNullOrEmpty(margin), "", margin.Replace(" ", "&nbsp;") & "• ")) & SafeFormString(pNode.NodeName), 50 - margin.Length))

            For Each cNode As clsNode In pNode.GetNodesBelow(SAUserID)
                GetOverTerminalNodes(tNodesList, cNode, margin & " ")
            Next
        End If
    End Sub

    Protected Sub Page_Init(ByVal sender As Object, ByVal e As EventArgs)
        Anytime.Ajax_Callback(Request.Form.ToString())
    End Sub

    Public Sub bindHtml(ByVal model As AnytimeOutputModel)
        If model IsNot Nothing Then
            Dim html As StringBuilder = New StringBuilder()

            'heder part
            html.Append("<div class='page_heading_section'>")
            html.Append("<div class='container'>")
            html.Append("<div class='row'>")

            html.Append("<div Class='col-md-6'>")
            html.Append($"<div Class='heading_content'><p><span>{model.step_task}</span></p></div>")
            html.Append("</div>")

            'If Not model.initSA.sensitivities.SAType = 1 Then
            html.Append("<div class='col-md-6 filter_btns'><div class='d-flex justify-content-start justify-content-lg-end align-items-center'>")

                If Not model.initSA.GetOptions = "" Then
                    html.Append("<div class='customrCheck'>")
                    html.Append($"{model.initSA.GetOptions}")
                    html.Append("</div>")
                End If
            'html.Append("<div Class='filter_btn btn_open'>")
            'html.Append("<div class='filter_icon'><img src='../../img/icon/filter.svg'></div>")
            'html.Append("<div class='filter_dropdown'><div class='filter_head'>")
            'html.Append("<span>Select Offshore Patrol Cutter(OPC)</span><img src='../../img/icon/filter.svg' class='filter_closer'></div>")
            'html.Append("<ul Class='filter_list'></ul>")
            'html.Append("</div></div>")
            html.Append("<button id='btnRefresh' onclick='onReset(); return false;'  class='reset-btn'><img src='../../img/icon/reset.svg'></button>")
            If model.initSA.sensitivities.SAType = 1 Then
                html.Append("<div class='normalization_dropdown ms-3'><label>Normalization :</label><select id='ddlsensitive' onchange='setSANotmalization(ddlsensitive)' class='form-select'>")
                html.Append("<option value='normAll'>Normalized</Option>")
                html.Append("<option value='unnormalized'>Unnormalized</Option>")
                html.Append("</select></div>")
            End If
            If model.initSA.GetSATypeString = "GSA" Then
                html.Append("<div class=' normalization_dropdown ms-3'>")
                If model.initSA.GetGSASubobjectives IsNot Nothing AndAlso model.initSA.GetGSASubobjectives.Count > 0 Then
                    html.Append("<select id='selSubobjectives' class='sensitivity-select select' onchange=onChangeSubobjective()>")
                    html.Append("</select>")
                End If
                html.Append("</div>")
            End If
            html.Append("</div></div>")
            'End If



            html.Append("</div>")
            html.Append("</div>")
            html.Append("</div>")


            html.Append("<div class='large-4 columns collapsesensitivity-label-wrap text-center hide-for-medium-down' id='sensitivcheckbox'>")
            html.Append("</div>")




            html.Append("<div class='container mt-3'><div class='row'><div id='divSA'>")
            html.Append("<canvas id='DSACanvas' class='whole' style='margin: 0px; padding: 0px;'>Your browser doesn't support HTML5</canvas>")
            html.Append("</div></div></div>")


            divContent.InnerHtml = html.ToString()
        End If
    End Sub

End Class