Option Strict Off

'L0026
Imports SpyronControls.Spyron.Core

Partial Class NewQuestion
    Inherits clsComparionCorePage

    Public Const _COOKIE_EDITOR_MODE As String = "survey_rich_editor"   ' D1278
    Private ASurveyType As SurveyType
    Private AProjectID As Integer = -1

    Public Sub New()
        MyBase.New(_PGID_SURVEY_QUESTION_NEW)   ' D0644
    End Sub

    'Survey object stores in the session
    Public ReadOnly Property ASurvey() As clsSurvey
        Get
            If ASurveyInfo Is Nothing Then Return Nothing Else Return ASurveyInfo.Survey("-") 'L0442 + D1277
        End Get
    End Property

    'L0034 ===
    Public ReadOnly Property ASurveyInfo() As clsSurveyInfo
        Get
            Dim AResult As clsSurveyInfo = Session(SESSION_MAIN_SURVEY_INFO_EDIT + AProjectID.ToString + "-" + CInt(ASurveyType).ToString)
            If AResult Is Nothing Then
                Return New clsSurveyInfo()
            Else
                Return AResult
            End If
        End Get
    End Property
    'L0034 ==

    'New question object stores in the session
    Public Property AQuestion() As clsQuestion
        Get
            Dim fQuestion As clsQuestion = Session(SESSION_NEW_QUESTION)
            If fQuestion Is Nothing Then
                If ASurvey IsNot Nothing Then
                    Response.Redirect(String.Format("SurveyEdit.aspx?guid={0}&pid={1}&prjid={2}&st={3}", ASurveyInfo.AGuid, ASurvey.Pages.IndexOf(ASurvey.ActivePage), ASurveyInfo.ProjectID, CInt(ASurveyInfo.SurveyType)) + GetTempThemeURI_(True))   ' D3897
                Else
                    ' D1663 ===
                    Response.Redirect(PageURL(_PGID_ERROR_404, GetTempThemeURI_(False)), True)  ' D3897
                    ' D1663 ==
                End If
            End If
            Return Session(SESSION_NEW_QUESTION)
        End Get
        Set(value As clsQuestion)
            Session(SESSION_NEW_QUESTION) = value
        End Set
    End Property

    ' D1277 ===
    Public Function GetInfodocURL() As String
        If ASurvey IsNot Nothing AndAlso AQuestion IsNot Nothing Then
            Return String.Format("{0}{1}.htm?r={2}", Infodoc_URL(ASurveyInfo.ProjectID, 0, reObjectType.SurveyQuestion, AQuestion.AGUID.ToString, -1), AQuestion.AGUID.ToString, GetRandomString(6, True, False).ToLower)    ' D2079 + D4428
        Else
            Return ""
        End If
    End Function
    ' D1277 ==

    ' D1278 ===
    Public Function EditorMode() As Boolean
        Return (AQuestion IsNot Nothing AndAlso isMHT(AQuestion.Text)) OrElse (AQuestion.Text = "" AndAlso GetCookie(_COOKIE_EDITOR_MODE, "0") <> "0")
    End Function

    Public Function GetSwitcher(isRich As Boolean, fActive As Boolean) As String
        Dim sName As String = ResString(CStr(IIf(isRich, "lblRichText", "lblPlainText")))
        If fActive Then
            Return String.Format("<span style='margin-top:2px; padding:2px 6px 1px 6px; border-top:1px solid #cccccc; border-left:1px solid #cccccc; border-right:1px solid #cccccc; background: #f0f0f0;'>{0}</span>", sName)
        Else
            Return String.Format("&nbsp;<a href='' onclick='SwitchEditor({1}, 0); return false;' class='actions'>{0}</a>&nbsp;", sName, IIf(isRich, 1, 0))
        End If
    End Function
    ' D1278 ==

    ' D3897 ===
    Public Function GetTempThemeURI_(ShowAmpersand As Boolean) As String
        Dim sRes As String = GetTempThemeURI(ShowAmpersand)
        If Not CheckVar("close", True) Then
            sRes += CStr(IIf(sRes = "" AndAlso Not ShowAmpersand, "?", "&")) + "close=0"
        End If
        Return sRes
    End Function
    ' D3897 ==

    Protected Sub Page_Init(sender As Object, e As EventArgs) Handles Me.Init
        AlignHorizontalCenter = True
        AlignVerticalCenter = True
        'NavigationPageID = _PGID_SURVEY_LIST
        Dim strST As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("st", ""))   'Anti-XSS
        If strST <> "" Then Integer.TryParse(strST, ASurveyType)
        Dim sProjectID As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("prjid", ""))   'Anti-XSS
        If sProjectID <> "" Then Integer.TryParse(sProjectID, AProjectID)
        StorePageID = False ' D1278
        ' D6054 ===
        If ASurveyInfo IsNot Nothing Then
            Select Case ASurveyInfo.SurveyType
                Case SurveyType.stImpactThankyouSurvey, SurveyType.stThankyouSurvey
                    NavigationPageID = _PGID_SURVEY_EDIT_POST
                Case Else
                    NavigationPageID = _PGID_SURVEY_EDIT_PRE
            End Select
        End If
        ' D6054 ==
    End Sub

    Protected Sub NewQuestionWizard_ActiveStepChanged(sender As Object, e As EventArgs) Handles NewQuestionWizard.ActiveStepChanged
        If AQuestion.Type = QuestionType.qtAlternativesSelect Then
            cbSelectAll.Visible = True
            cbSelectAll.InputAttributes.Add("onclick", "$('.alt').prop('checked', this.checked); $('.altsselectall').prop('checked', this.checked);")
        Else
            cbSelectAll.Visible = False
        End If

        Select Case NewQuestionWizard.ActiveStepIndex

            'Update question type and text input controls
            Case NewQuestionWizard.WizardSteps.IndexOf(WSTextType)
                lbQuestionType.SelectedIndex = lbQuestionType.Items.IndexOf(lbQuestionType.Items.FindByValue(AQuestion.Type))

            Case NewQuestionWizard.WizardSteps.IndexOf(WSVariants)

                If CheckVar("EditorMode", 0) = 0 Then
                    AQuestion.Text = tbQuestionText.Text
                Else
                    If isMHT(AQuestion.Text) Then
                        tbQuestionText.Text = HTML2Text(Infodoc_Unpack(ASurveyInfo.ProjectID, 0, reObjectType.SurveyQuestion, AQuestion.AGUID.ToString, AQuestion.Text, True, True, -1, True))  ' D4429
                    Else
                        AQuestion.Text = HTML2Text(AQuestion.Text) ' D4428
                    End If
                End If

                'if question type is not selected yet, then move back to first step
                If lbQuestionType.SelectedItem Is Nothing Then
                    NewQuestionWizard.ActiveStepIndex = NewQuestionWizard.WizardSteps.IndexOf(WSTextType)
                Else
                    'else set question type and question text
                    AQuestion.Type = CInt(lbQuestionType.SelectedValue)

                    'update survey question control for previewing
                    sqQuestionPreview.SurveyInfo = ASurveyInfo  ' D1278
                    sqQuestionPreview.Question = AQuestion
                    'skip adding question variants depends on question type
                    If AQuestion.Type = QuestionType.qtAlternativesSelect Then
                        sqQuestionPreview.Hierarchy = App.ActiveProject.ProjectManager.AltsHierarchy(App.ActiveProject.ProjectManager.ActiveAltsHierarchy)
                    End If
                    If AQuestion.Type = QuestionType.qtObjectivesSelect Then
                        sqQuestionPreview.Hierarchy = App.ActiveProject.ProjectManager.Hierarchy(App.ActiveProject.ProjectManager.ActiveHierarchy)
                    End If
                    If Not AQuestion.AllowVariants() Then
                        NewQuestionWizard.ActiveStepIndex = NewQuestionWizard.WizardSteps.IndexOf(WSAdjustView)
                    End If
                End If

            Case NewQuestionWizard.WizardSteps.IndexOf(WSAdjustView)
                sqQuestionPreview.SurveyInfo = ASurveyInfo  ' D1278
                sqQuestionPreview.Question = AQuestion
                If AQuestion.Type = QuestionType.qtAlternativesSelect Then
                    sqQuestionPreview.Hierarchy = App.ActiveProject.ProjectManager.AltsHierarchy(App.ActiveProject.ProjectManager.ActiveAltsHierarchy)
                End If
                If AQuestion.Type = QuestionType.qtObjectivesSelect Then
                    sqQuestionPreview.Hierarchy = App.ActiveProject.ProjectManager.Hierarchy(App.ActiveProject.ProjectManager.ActiveHierarchy)
                End If
                'if question type allow Variants
                If AQuestion.AllowVariants Then
                    Dim ANewQuestion As clsQuestion
                    ANewQuestion = AQuestion
                    'clear existing variants
                    ANewQuestion.Variants.Clear()
                    'add Variants from tbVariants textbox

                    For Each AString As String In tbVariants.Text.Split(CChar(vbLf))
                        AString = AString.Replace(Chr(10), "")
                        AString.TrimEnd(vbCr, vbLf)
                        If AString <> "" Then
                            Dim AVariant As clsVariant = New clsVariant()
                            AVariant.Text = EcSanitizer.GetSafeHtmlFragment(AString)    'Anti-XSS
                            AVariant.Type = VariantType.vtText
                            AVariant.ValueType = VariantValueType.vvtString
                            AVariant.VariantValue = EcSanitizer.GetSafeHtmlFragment(AString)    'Anti-XSS
                            ANewQuestion.Variants.Add(AVariant)
                        End If
                    Next
                    If ANewQuestion.Variants.Count = 0 Then
                        NewQuestionWizard.ActiveStepIndex = NewQuestionWizard.WizardSteps.IndexOf(WSVariants)
                    End If

                    'L0046 ===
                    If ANewQuestion.Type = QuestionType.qtComment OrElse ANewQuestion.Type = QuestionType.qtAlternativesSelect OrElse ANewQuestion.Type = QuestionType.qtObjectivesSelect Then
                        cbRequired.Enabled = False
                        cbRequired.Checked = False
                    Else
                        cbRequired.Visible = True
                    End If
                    'L0046 ==

                    'add other variant if necessary
                    If cbAllowOther.Checked Then
                        Dim AVariant As clsVariant = New clsVariant()
                        AVariant.Type = VariantType.vtOtherLine
                        AVariant.Text = ResString("Survey_lblOther")
                        AVariant.ValueType = VariantValueType.vvtString
                        AVariant.VariantValue = ""
                        ANewQuestion.Variants.Add(AVariant)
                    End If

                    AQuestion = ANewQuestion
                    sqQuestionPreview.SurveyInfo = ASurveyInfo  ' D1278
                    sqQuestionPreview.Question = AQuestion
                    If AQuestion.Type = QuestionType.qtAlternativesSelect Then
                        sqQuestionPreview.Hierarchy = App.ActiveProject.ProjectManager.AltsHierarchy(App.ActiveProject.ProjectManager.ActiveAltsHierarchy)
                    End If
                    If AQuestion.Type = QuestionType.qtObjectivesSelect Then
                        sqQuestionPreview.Hierarchy = App.ActiveProject.ProjectManager.Hierarchy(App.ActiveProject.ProjectManager.ActiveHierarchy)
                    End If

                End If

                'L0047 ===
                ddlMaxSelected.Items.Clear()
                ddlMinSelected.Items.Clear()
                ddlMinSelected.Items.Add(" - No Limit - ")
                ddlMaxSelected.Items.Add(" - No Limit - ")
                If AQuestion.Type = QuestionType.qtImageCheck Or AQuestion.Type = QuestionType.qtVariantsCheck Then
                    ddlMaxSelected.Enabled = True
                    ddlMinSelected.Enabled = True
                    For i As Integer = 1 To AQuestion.Variants.Count
                        ddlMinSelected.Items.Add(i.ToString)
                        ddlMaxSelected.Items.Add(i.ToString)
                    Next
                Else
                    If AQuestion.Type = QuestionType.qtAlternativesSelect Or AQuestion.Type = QuestionType.qtObjectivesSelect Then
                        ddlMaxSelected.Enabled = True
                        ddlMinSelected.Enabled = True
                        Dim MaxItems As Integer = 0
                        If AQuestion.Type = QuestionType.qtAlternativesSelect Then
                            MaxItems = App.ActiveProject.ProjectManager.AltsHierarchy(App.ActiveProject.ProjectManager.ActiveAltsHierarchy).Nodes.Count
                        End If
                        If AQuestion.Type = QuestionType.qtObjectivesSelect Then
                            MaxItems = App.ActiveProject.ProjectManager.Hierarchy(App.ActiveProject.ProjectManager.ActiveHierarchy).Nodes.Count
                        End If
                        For i As Integer = 1 To MaxItems
                            ddlMinSelected.Items.Add(i.ToString)
                            ddlMaxSelected.Items.Add(i.ToString)
                        Next
                    Else
                        ddlMaxSelected.Enabled = False
                        ddlMinSelected.Enabled = False
                    End If
                End If
                ddlMaxSelected.SelectedIndex = AQuestion.MaxSelectedVariants
                ddlMinSelected.SelectedIndex = AQuestion.MinSelectedVariants
                'L0047 ==

                If NewQuestionWizard.ActiveStepIndex > 0 Then ScriptManager.RegisterStartupScript(Me, GetType(String), "IsChanged", "theForm.isChanged.value = 1; initSurveyForm();", True) ' D4431
        End Select
        ScriptManager.RegisterStartupScript(Me, GetType(String), "applyFix", "fixTextarea();", True)
    End Sub

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        If ASurvey IsNot Nothing AndAlso AQuestion IsNot Nothing Then
            'L0049 ===
            If tbQuestionText.Text = "" Then
                ' D1278 ===
                tbQuestionText.Text = AQuestion.Text    ' D1278

                ' D4441 ===
                Dim sHTML As String = ""
                If isMHT(tbQuestionText.Text) Then
                    sHTML = CutHTMLHeaders(Infodoc_Unpack(ASurveyInfo.ProjectID, 0, reObjectType.SurveyQuestion, AQuestion.AGUID.ToString, tbQuestionText.Text, True, True, -1, True))  ' D6727
                    tbQuestionText.Text = HTML2Text(sHTML) ' D4428 + D4429
                End If
                If tbQuestionText.Text.Trim() = "" AndAlso isHTMLEmpty(sHTML) Then 'L0341
                    ' D4441 ==
                    tbQuestionText.Text = "Question Text" ' String.Format("Question {0} Text", ASurvey.ActivePage.Questions.Count + 1)
                    AQuestion.Text = tbQuestionText.Text
                End If
                If Not IsPostBack AndAlso Not isCallback Then
                    Infodoc_Unpack(ASurveyInfo.ProjectID, 0, reObjectType.SurveyQuestion, AQuestion.AGUID.ToString, AQuestion.Text, True, True, -1, True)   ' D2079 + D4429
                End If
                ' D1278 ==
                tbQuestionText.Focus()
            End If
            'L0049 ==

            divOptions.Visible = True   ' D4440
            divSelCount.Visible = True  ' D4440

            'if question type listbox is empty, then initialize it
            If lbQuestionType.Items.Count = 0 Then
                Dim AListItem As ListItem
                AListItem = New ListItem(ResString("Survey_qtComment"), QuestionType.qtComment)
                lbQuestionType.Items.Add(AListItem)
                AListItem = New ListItem(ResString("Survey_qtOpenLine"), QuestionType.qtOpenLine)
                lbQuestionType.Items.Add(AListItem)
                AListItem = New ListItem(ResString("Survey_qtOpenText"), QuestionType.qtOpenMemo)
                lbQuestionType.Items.Add(AListItem)
                AListItem = New ListItem(ResString("Survey_qtRadioList"), QuestionType.qtVariantsRadio)
                lbQuestionType.Items.Add(AListItem)
                AListItem = New ListItem(ResString("Survey_qtCheckList"), QuestionType.qtVariantsCheck)
                lbQuestionType.Items.Add(AListItem)
                AListItem = New ListItem(ResString("Survey_qtDropDownList"), QuestionType.qtVariantsCombo)
                lbQuestionType.Items.Add(AListItem)
                AListItem = New ListItem(ResString("Survey_qtObjectivesList"), QuestionType.qtObjectivesSelect)
                lbQuestionType.Items.Add(AListItem)
                AListItem = New ListItem(ResString("Survey_qtAlternativesList"), QuestionType.qtAlternativesSelect)
                lbQuestionType.Items.Add(AListItem)
                AListItem = New ListItem(ResString("Survey_qtNumber"), QuestionType.qtNumber)
                lbQuestionType.Items.Add(AListItem)
                AListItem = New ListItem(ResString("Survey_qtNumberColumn"), QuestionType.qtNumberColumn)
                lbQuestionType.Items.Add(AListItem)
            End If
            'L0047 ===
            sqQuestionPreview.AlternativeAlternativesHierarchyText = ResString("Survey_strAltHierarchy")
            sqQuestionPreview.AlternativeObjectivesHierarchyText = ResString("Survey_strObjHierarchy")
            'L0047 ==
        End If
    End Sub

    'L0035 ===
    Protected Sub NewQuestionWizard_CancelButtonClick(sender As Object, e As EventArgs) Handles NewQuestionWizard.CancelButtonClick
        Session.Remove(SESSION_NEW_QUESTION) 'L0036
        Response.Redirect(String.Format("SurveyEdit.aspx?pid={0}&prjid={1}&st={2}", ASurvey.Pages.IndexOf(ASurvey.ActivePage), ASurveyInfo.ProjectID, CInt(ASurveyInfo.SurveyType)) + GetTempThemeURI_(True)) 'L0315 + D3897
    End Sub
    'L0035 ==

    Protected Sub NewQuestionWizard_FinishButtonClick(sender As Object, e As WizardNavigationEventArgs) Handles NewQuestionWizard.FinishButtonClick
        If AQuestion IsNot Nothing Then
            If ASurvey IsNot Nothing Then
                If ASurvey.ActivePage IsNot Nothing Then
                    'add created question to Survey
                    'L0036 ===
                    If ASurvey.QuestionByGUID(AQuestion.AGUID) IsNot Nothing Then
                        AQuestion.AGUID = New Guid()
                    End If
                    'L0036 ==
                    'L0046 ===
                    'set AllowSkip Property
                    If cbRequired.Checked Then
                        AQuestion.AllowSkip = False
                    Else
                        AQuestion.AllowSkip = True
                    End If
                    'L0046 ==

                    AQuestion.SelectAllByDefault = cbSelectAll.Checked

                    'L0047 ===
                    AQuestion.MaxSelectedVariants = ddlMaxSelected.SelectedIndex
                        AQuestion.MinSelectedVariants = ddlMinSelected.SelectedIndex
                        'L0047 ==

                        ASurvey.ActivePage.Questions.Add(AQuestion)
                        ASurveyInfo.SaveSurvey(False) 'L0441
                        Session(SESSION_MAIN_SURVEY_INFO_EDIT + AProjectID.ToString + "-" + CInt(ASurveyType).ToString) = ASurveyInfo
                        Session.Remove(SESSION_NEW_QUESTION) 'L0036
                        Response.Redirect(String.Format("SurveyEdit.aspx?guid={0}&pid={1}&prjid={2}&st={3}", ASurveyInfo.AGuid, ASurvey.Pages.IndexOf(ASurvey.ActivePage), ASurveyInfo.ProjectID, CInt(ASurveyInfo.SurveyType)) + GetTempThemeURI_(True)) 'L0315 + D3897
                    End If
                End If
        End If
    End Sub

    ' D4440 ===
    Protected Sub NewQuestionWizard_PreRender(sender As Object, e As EventArgs) Handles NewQuestionWizard.PreRender
        If AQuestion IsNot Nothing Then
            Select Case AQuestion.Type
                Case QuestionType.qtComment, QuestionType.qtImageCheck, QuestionType.qtObjectivesSelect, QuestionType.qtAlternativesSelect
                    divOptions.Visible = False
                Case QuestionType.qtVariantsCheck
                Case Else
                    divSelCount.Visible = False
            End Select
            If Not divSelCount.Visible AndAlso Not cbRequired.Visible Then divOptions.Visible = False
        End If
    End Sub

    ' D4440 ==

End Class
