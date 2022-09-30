Option Strict Off
'L0037
Imports System.Drawing
Imports SpyronControls.Spyron.Core

Partial Class EditQuestion
    Inherits clsComparionCorePage

    Public Const _COOKIE_EDITOR_MODE As String = "survey_rich_editor"   ' D1278

    Private fQuestionGUID As Guid
    Private ASurveyType As SurveyType
    Private AProjectID As Integer = -1

    Public Sub New()
        MyBase.New(_PGID_SURVEY_QUESTION_EDIT)  ' D0644
    End Sub

    Public Property AQuestionGUID() As Guid
        Get
            Return fQuestionGUID
        End Get
        Set(ByVal value As Guid)
            fQuestionGUID = value
        End Set
    End Property

    Public ReadOnly Property AQuestion() As clsQuestion
        Get
            If ASurvey IsNot Nothing Then
                Dim sQuestion As clsQuestion = ASurvey.QuestionByGUID(AQuestionGUID)
                If sQuestion Is Nothing Then
                    'Response.Redirect("~/404.aspx")
                    Response.Redirect(PageURL(_PGID_ERROR_404, GetTempThemeURI_(False)), True)   ' D1664 + D3897
                    Return Nothing
                Else
                    Return sQuestion
                End If
            Else
                'Response.Redirect("~/404.aspx")
                Response.Redirect(PageURL(_PGID_ERROR_404, GetTempThemeURI_(False)), True)   ' D1664 + D3897
                Return Nothing
            End If
        End Get
    End Property

    'Survey object stores in the session
    Public ReadOnly Property ASurvey() As clsSurvey
        Get
            Return ASurveyInfo.Survey("-") 'L0442
        End Get
    End Property

    Public ReadOnly Property ASurveyInfo() As clsSurveyInfo
        Get
            Return Session(SESSION_MAIN_SURVEY_INFO_EDIT + AProjectID.ToString + "-" + CInt(ASurveyType).ToString)
        End Get
    End Property

    ' D1277 ===
    Public Function GetInfodocURL() As String
        If ASurvey IsNot Nothing AndAlso AQuestion IsNot Nothing Then
            Return String.Format("{0}{1}.htm?r={2}", Infodoc_URL(ASurveyInfo.ProjectID, 0, reObjectType.SurveyQuestion, AQuestion.AGUID.ToString, -1), AQuestion.AGUID.ToString, GetRandomString(6, True, False).ToLower)    ' D2079 + D4421
        Else
            Return ""
        End If
    End Function

    Public Function EditorMode() As Boolean
        Return AQuestion IsNot Nothing AndAlso isMHT(AQuestion.Text)
    End Function

    Public Function GetSwitcher(ByVal isRich As Boolean, ByVal fActive As Boolean) As String
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

    Protected Sub Page_Init(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Init
        AlignHorizontalCenter = True
        AlignVerticalCenter = True
        'NavigationPageID = _PGID_SURVEY_LIST

        StorePageID = False ' D1278

        Dim strGUID As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("qid", ""))    'Anti-XSS

        If strGUID <> "" Then
            Try
                AQuestionGUID = New Guid(strGUID)
            Catch ex As Exception

            End Try
        End If

        Dim strST As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("st", ""))   'Anti-XSS
        If strST <> "" Then Integer.TryParse(strST, ASurveyType)
        Dim sProjectID As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("prjid", ""))   'Anti-XSS
        If sProjectID <> "" Then Integer.TryParse(sProjectID, AProjectID)

        'L0047 ===
        sqQuestionPreview.AlternativeAlternativesHierarchyText = ResString("Survey_strAltHierarchy")
        sqQuestionPreview.AlternativeObjectivesHierarchyText = ResString("Survey_strObjHierarchy")
        'L0047 ==

        'set initial values to page controls
        sqQuestionPreview.SurveyInfo = ASurveyInfo  ' D1258
        If AQuestion.Type = QuestionType.qtAlternativesSelect Then
            sqQuestionPreview.Hierarchy = App.ActiveProject.ProjectManager.AltsHierarchy(App.ActiveProject.ProjectManager.ActiveAltsHierarchy)
        End If
        If AQuestion.Type = QuestionType.qtObjectivesSelect Then
            sqQuestionPreview.Hierarchy = App.ActiveProject.ProjectManager.Hierarchy(App.ActiveProject.ProjectManager.ActiveHierarchy)
        End If
        sqQuestionPreview.Question = AQuestion

        tbQuestionText.Text = AQuestion.Text
        ' D1278 ===
        Dim sHTML As String = CutHTMLHeaders(Infodoc_Unpack(ASurveyInfo.ProjectID, 0, reObjectType.SurveyQuestion, AQuestion.AGUID.ToString, tbQuestionText.Text, True, True, -1, True))  ' D4421 + D4429 + D6727
        If isMHT(tbQuestionText.Text) Then
            tbQuestionText.Text = HTML2Text(sHTML) ' D2079 + D4421
        End If
        'If Not IsPostBack AndAlso Not isCallback Then
        '    Infodoc_Unpack(ASurveyInfo.ProjectID, 0, reObjectType.SurveyQuestion, AQuestion.AGUID.ToString, AQuestion.Text, True, True, -1)   ' D2079
        'End If
        ' D1278 ==
        tbQuestionText.Focus()
        If AQuestion.AllowVariants Then
            panVariantsEdit.Controls.Clear()
            Dim lblBR As Label

            Dim fContainsOther As Boolean = False 'L0050
            For Each AVariant As clsVariant In AQuestion.Variants
                Dim AVariantTextBox As New TextBox
                AVariantTextBox.ID = String.Format("tbVariant{0}", AVariant.AGUID)
                AVariantTextBox.TextMode = TextBoxMode.SingleLine
                AVariantTextBox.Text = AVariant.Text
                AVariantTextBox.Width = 345

                'L0342 ===
                If AVariant.Text.Trim() = "" Then
                    AVariantTextBox.ReadOnly = False
                    AVariantTextBox.Enabled = True
                Else
                    AVariantTextBox.ReadOnly = True
                    AVariantTextBox.Enabled = False
                End If
                'L0342 ==

                AVariantTextBox.ToolTip = AVariant.VariantValue 'LLL

                'L0050 ===
                If AVariant.Type = VariantType.vtOtherLine Or AVariant.Type = VariantType.vtOtherMemo Then
                    AVariantTextBox.BackColor = Color.LightYellow
                    AVariantTextBox.ToolTip = "Other Answer"
                    fContainsOther = True
                End If
                'L0050 ==

                panVariantsEdit.Controls.Add(AVariantTextBox)
                ''LLL ===
                'Dim lblVariantValue = New Label()
                'lblVariantValue.Text = "=" + AVariant.VariantValue
                'panVariantsEdit.Controls.Add(lblVariantValue)
                ''LLL ==

                'L0354 ===
                Dim AMoveImgBtn As New ImageButton()
                AMoveImgBtn.ToolTip = "Move Up"
                AMoveImgBtn.ID = "ibtnUp" + AVariant.AGUID.ToString
                AMoveImgBtn.ImageUrl = "Images/IconUp.gif"
                AMoveImgBtn.BackColor = Color.Transparent
                AMoveImgBtn.Attributes.Add("value", AVariant.AGUID.ToString)
                AMoveImgBtn.BorderWidth = 1
                AMoveImgBtn.BorderColor = Color.Transparent
                panVariantsEdit.Controls.Add(AMoveImgBtn)
                If AQuestion.Variants.IndexOf(AVariant) = 0 Then
                    AMoveImgBtn.Enabled = False
                Else
                    AddHandler AMoveImgBtn.Click, AddressOf MoveVariantUpClick
                End If

                AMoveImgBtn = New ImageButton()
                AMoveImgBtn.ToolTip = "Move Down"
                AMoveImgBtn.ID = "ibtnDown" + AVariant.AGUID.ToString
                AMoveImgBtn.ImageUrl = "Images/IconDown.gif"
                AMoveImgBtn.BackColor = Color.Transparent
                AMoveImgBtn.Attributes.Add("value", AVariant.AGUID.ToString)
                AMoveImgBtn.BorderWidth = 1
                AMoveImgBtn.BorderColor = Color.Transparent
                panVariantsEdit.Controls.Add(AMoveImgBtn)
                If AQuestion.Variants.IndexOf(AVariant) = AQuestion.Variants.Count - 1 Then
                    AMoveImgBtn.Enabled = False
                Else
                    AddHandler AMoveImgBtn.Click, AddressOf MoveVariantDownClick
                End If
                'L0354 ==

                Dim AImgBtn As New ImageButton()
                AImgBtn.ToolTip = "Delete Answer"
                AImgBtn.ID = "ibtnDelete" + AVariant.AGUID.ToString
                AImgBtn.ImageUrl = "Images/IconDelete.gif"
                AImgBtn.BackColor = Color.Transparent
                AImgBtn.Attributes.Add("value", AVariant.AGUID.ToString)
                AImgBtn.BorderWidth = 1
                AImgBtn.BorderColor = Color.Transparent
                AddHandler AImgBtn.Click, AddressOf AVariantButtonClick
                panVariantsEdit.Controls.Add(AImgBtn)

                lblBR = New Label()
                lblBR.Text = "<br/>"
                panVariantsEdit.Controls.Add(lblBR)
            Next
            'L0050 ===
            If Not fContainsOther Then
                Dim ALinkBtn As New LinkButton
                ALinkBtn.ID = "lbtnAddOtherVariant"
                ALinkBtn.Text = "Add ""Other"" Answer"
                AddHandler ALinkBtn.Click, AddressOf AOtherVariantButtonClick
                panVariantsEdit.Controls.Add(ALinkBtn)

                lblBR = New Label()
                lblBR.Text = "<br/>"
                panVariantsEdit.Controls.Add(lblBR)
            End If
            'L0050 ==
        Else
            tdVariants.Visible = False  ' D4440
            tdAddVar.Visible = False    ' D4440
        End If

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

    'L0050 ===
    Public Sub AOtherVariantButtonClick(ByVal sender As Object, ByVal e As EventArgs)
        Dim AVariant As clsVariant = New clsVariant()
        AVariant.Type = VariantType.vtOtherLine
        AVariant.Text = "Other: "
        AVariant.ValueType = VariantValueType.vvtString
        AVariant.VariantValue = ""
        AQuestion.Variants.Add(AVariant)
        'AQuestion.Variants.Remove(AQuestion.VariantByGUID(New Guid(Abtn.Attributes("value"))))
        btnApply_Click(sender, e)
    End Sub
    'L0050 ==

    'L0354 ===
    Public Sub MoveVariantUpClick(ByVal sender As Object, ByVal e As ImageClickEventArgs)
        Dim Abtn As ImageButton
        Abtn = CType(sender, ImageButton)
        Dim ATempVariant As New clsVariant()
        ATempVariant = AQuestion.VariantByGUID(New Guid(Abtn.Attributes("value")))
        Dim Idx As Integer = AQuestion.Variants.IndexOf(ATempVariant)
        If Idx > 0 Then
            AQuestion.Variants(Idx) = AQuestion.Variants(Idx - 1)
            AQuestion.Variants(Idx - 1) = ATempVariant
        End If
        btnApply_Click(sender, e)
    End Sub

    Public Sub MoveVariantDownClick(ByVal sender As Object, ByVal e As ImageClickEventArgs)
        Dim Abtn As ImageButton
        Abtn = CType(sender, ImageButton)
        Dim ATempVariant As New clsVariant()
        ATempVariant = AQuestion.VariantByGUID(New Guid(Abtn.Attributes("value")))
        Dim Idx As Integer = AQuestion.Variants.IndexOf(ATempVariant)
        If Idx < (AQuestion.Variants.Count - 1) Then
            AQuestion.Variants(Idx) = AQuestion.Variants(Idx + 1)
            AQuestion.Variants(Idx + 1) = ATempVariant
        End If
        btnApply_Click(sender, e)
    End Sub
    'L0354 ==

    Public Sub AVariantButtonClick(ByVal sender As Object, ByVal e As ImageClickEventArgs)
        Dim Abtn As ImageButton
        Abtn = CType(sender, ImageButton)
        AQuestion.Variants.Remove(AQuestion.VariantByGUID(New Guid(Abtn.Attributes("value"))))
        btnApply_Click(sender, e)
    End Sub

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        'if question type listbox is empty, then initialize it
        If lbQuestionType.Items.Count = 0 Then
            Dim AListItem As ListItem

            AListItem = New ListItem(ResString("Survey_qtComment"), QuestionType.qtComment)
            If AQuestion.Type <> QuestionType.qtComment Then
                AListItem.Enabled = False
            Else
                AListItem.Enabled = True
            End If
            lbQuestionType.Items.Add(AListItem)

            AListItem = New ListItem(ResString("Survey_qtOpenLine"), QuestionType.qtOpenLine)
            If AQuestion.Type <> QuestionType.qtOpenLine And AQuestion.Type <> QuestionType.qtOpenMemo Then
                AListItem.Enabled = False
            Else
                AListItem.Enabled = True
            End If
            lbQuestionType.Items.Add(AListItem)

            AListItem = New ListItem(ResString("Survey_qtOpenText"), QuestionType.qtOpenMemo)
            If AQuestion.Type <> QuestionType.qtOpenLine And AQuestion.Type <> QuestionType.qtOpenMemo Then
                AListItem.Enabled = False
            Else
                AListItem.Enabled = True
            End If
            lbQuestionType.Items.Add(AListItem)

            AListItem = New ListItem(ResString("Survey_qtRadioList"), QuestionType.qtVariantsRadio)
            If AQuestion.Type <> QuestionType.qtVariantsRadio And AQuestion.Type <> QuestionType.qtVariantsCombo Then
                AListItem.Enabled = False
            Else
                AListItem.Enabled = True
            End If
            lbQuestionType.Items.Add(AListItem)

            AListItem = New ListItem(ResString("Survey_qtCheckList"), QuestionType.qtVariantsCheck)
            If AQuestion.Type <> QuestionType.qtImageCheck And AQuestion.Type <> QuestionType.qtVariantsCheck Then
                AListItem.Enabled = False
            Else
                AListItem.Enabled = True
            End If
            lbQuestionType.Items.Add(AListItem)

            AListItem = New ListItem(ResString("Survey_qtDropDownList"), QuestionType.qtVariantsCombo)
            If AQuestion.Type <> QuestionType.qtVariantsCombo And AQuestion.Type <> QuestionType.qtVariantsRadio Then
                AListItem.Enabled = False
            Else
                AListItem.Enabled = True
            End If
            lbQuestionType.Items.Add(AListItem)

            AListItem = New ListItem(ResString("Survey_qtObjectivesList"), QuestionType.qtObjectivesSelect)
            If AQuestion.Type <> QuestionType.qtObjectivesSelect Then
                AListItem.Enabled = False
            Else
                AListItem.Enabled = True
            End If
            lbQuestionType.Items.Add(AListItem)

            AListItem = New ListItem(ResString("Survey_qtAlternativesList"), QuestionType.qtAlternativesSelect)
            If AQuestion.Type <> QuestionType.qtAlternativesSelect Then
                AListItem.Enabled = False
            Else
                AListItem.Enabled = True
            End If
            lbQuestionType.Items.Add(AListItem)

            AListItem = New ListItem(ResString("Survey_qtNumber"), QuestionType.qtNumber)
            If AQuestion.Type <> QuestionType.qtNumber Then
                AListItem.Enabled = False
            Else
                AListItem.Enabled = True
            End If
            lbQuestionType.Items.Add(AListItem)

            AListItem = New ListItem(ResString("Survey_qtNumberColumn"), QuestionType.qtNumberColumn)
            If AQuestion.Type <> QuestionType.qtNumberColumn Then
                AListItem.Enabled = False
            Else
                AListItem.Enabled = True
            End If
            lbQuestionType.Items.Add(AListItem)

            lbQuestionType.SelectedIndex = lbQuestionType.Items.IndexOf(lbQuestionType.Items.FindByValue(AQuestion.Type))
            'L0046 ===
            If AQuestion.Type <> QuestionType.qtComment Then
                cbRequired.Checked = Not AQuestion.AllowSkip
            End If
            'L0046 ==
        End If

        'L0047 ===
        If ddlMaxSelected.SelectedIndex = -1 and  ddlMaxSelected.SelectedIndex = -1 Then
            If ddlMaxSelected.Items.Count = 0 Or _
               ddlMinSelected.Items.Count = 0 Or _
               (AQuestion.Variants.Count <> ddlMinSelected.Items.Count And (AQuestion.Type = QuestionType.qtImageCheck Or AQuestion.Type = QuestionType.qtVariantsCheck)) Then
                ddlMaxSelected.Items.Clear()
                ddlMinSelected.Items.Clear()
                ddlMaxSelected.Items.Add(" - No Limit - ")
                ddlMinSelected.Items.Add(" - No Limit - ")
                If AQuestion.Type = QuestionType.qtImageCheck Or AQuestion.Type = QuestionType.qtVariantsCheck Then
                    ddlMaxSelected.Enabled = True
                    ddlMinSelected.Enabled = True
                    For i As Integer = 1 To AQuestion.Variants.Count
                        ddlMinSelected.Items.Add(i.ToString)
                        ddlMaxSelected.Items.Add(i.ToString)
                    Next
                Else
                    If AQuestion.Type = QuestionType.qtObjectivesSelect Or AQuestion.Type = QuestionType.qtAlternativesSelect Then
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
            End If
            ddlMaxSelected.SelectedIndex = AQuestion.MaxSelectedVariants
            ddlMinSelected.SelectedIndex = AQuestion.MinSelectedVariants
        End If
        'L0047 ==

        ' D4440 ===
        divSelCount.Visible = True
        tdOptions.Visible = True
        divLink.Visible = True
        ' D4440 ==

        If ddlAttributes.Items.Count = 0 Then
            ddlAttributes.Items.Clear()
            ddlAttributes.Items.Add(" - Not linked - ")
            Dim SelectedIndex = -1
            For Each attr In App.ActiveProject.ProjectManager.Attributes.GetUserAttributes
                ddlAttributes.Items.Add(attr.Name)
                If AQuestion.LinkedAttributeID.Equals(attr.ID) Then
                    SelectedIndex = App.ActiveProject.ProjectManager.Attributes.GetUserAttributes.IndexOf(attr) + 1
                End If
            Next
            ddlAttributes.SelectedIndex = SelectedIndex
        End If

        'L0046 ===
        If AQuestion.Type = QuestionType.qtComment OrElse AQuestion.Type = QuestionType.qtObjectivesSelect OrElse AQuestion.Type = QuestionType.qtAlternativesSelect Then
            cbRequired.Visible = False
        End If
        'L0046 ==
    End Sub

    Public Function ApplyChangesToQuestion() As Boolean 'L0341
        ' D4441 ===
        Dim sHTML As String = ""
        If CheckVar("EditorMode", 0) = 1 AndAlso isMHT(AQuestion.Text) Then
            sHTML = CutHTMLHeaders(Infodoc_Unpack(ASurveyInfo.ProjectID, 0, reObjectType.SurveyQuestion, AQuestion.AGUID.ToString, AQuestion.Text, True, True, -1, True))   ' D6727
            tbQuestionText.Text = HTML2Text(sHTML) ' D4428 + D4429
        End If
        If tbQuestionText.Text.Trim() <> "" OrElse Not isHTMLEmpty(sHTML) Then 'L0341
            ' D4441 ==
            'AQuestion.Text = tbQuestionText.Text
            If CheckVar("EditorMode", 0) = 0 Then
                AQuestion.Text = tbQuestionText.Text
            Else
                tbQuestionText.Text = HTML2Text(AQuestion.Text) ' D1278
            End If
        Else
            Return False
        End If
        AQuestion.Type = lbQuestionType.SelectedValue
        AQuestion.AllowSkip = Not cbRequired.Checked 'L0046
        AQuestion.SelectAllByDefault = cbSelectAllByDefault.Checked

        AQuestion.MaxSelectedVariants = ddlMaxSelected.SelectedIndex 'L0047
        AQuestion.MinSelectedVariants = ddlMinSelected.SelectedIndex 'L0047

        For Each AVariant As clsVariant In AQuestion.Variants
            Dim tbVariant As TextBox
            tbVariant = panVariantsEdit.FindControl(String.Format("tbVariant{0}", AVariant.AGUID))
            'L0050 ===
            If tbVariant IsNot Nothing Then
                If tbVariant.Text.Trim() <> "" Then 'L0341
                    AVariant.Text = tbVariant.Text
                Else
                    Return False
                End If
            End If
            'L0050 ==
        Next

        'add New Variants
        If AQuestion.AllowVariants Then
            For Each AString As String In tbAddVariants.Text.Split(CChar(vbLf))
                AString = AString.Replace(Chr(10), "")
                AString.TrimEnd(vbCr, vbLf)
                If AString.Trim() <> "" Then 'L0341
                    Dim AVariant As clsVariant = New clsVariant()
                    AVariant.Text = AString
                    AVariant.Type = VariantType.vtText
                    AVariant.ValueType = VariantValueType.vvtString
                    AVariant.VariantValue = AString
                    AQuestion.Variants.Add(AVariant)
                End If
            Next
        End If

        If ddlAttributes.SelectedIndex > 0 Then
            AQuestion.LinkedAttributeID = App.ActiveProject.ProjectManager.Attributes.GetUserAttributes(ddlAttributes.SelectedIndex - 1).ID
        Else
            AQuestion.LinkedAttributeID = Guid.Empty
        End If

        Return True 'L0341
    End Function

    Protected Sub btnApply_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnApply.Click
        'L0341 ===
        If ApplyChangesToQuestion() Then
            erEmptyFields.Visible = False
            ASurveyInfo.SaveSurvey(False)   ' D4421
            Response.Redirect(String.Format("EditQuestion.aspx?qid={0}&st={1}&prjid={2}", AQuestion.AGUID.ToString, CInt(ASurveyInfo.SurveyType), AProjectID) + GetTempThemeURI_(True), True) 'L0315 + D3897
        Else
            erEmptyFields.Visible = True
        End If
        'L0341 ==
    End Sub

    Protected Sub btnOK_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnOK.Click
        If ApplyChangesToQuestion() Then 'L0341
            Dim pid As Integer = ASurvey.Pages.IndexOf(ASurvey.ActivePage)
            erEmptyFields.Visible = False 'L0341
            ASurveyInfo.SaveSurvey(False) 'L0441
            Response.Redirect(URLParameter(PageURL(NavigationPageID), "guid", ASurveyInfo.AGuid) + "&pid=" + pid.ToString + "&prjid=" + ASurveyInfo.ProjectID.ToString + "&st=" + CInt(ASurveyInfo.SurveyType).ToString + GetTempThemeURI_(True), True) 'L0315 + D3897
        Else
            erEmptyFields.Visible = True 'L0341
        End If
    End Sub

    Protected Sub btnCancel_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnCancel.Click
        Dim pid As Integer = ASurvey.Pages.IndexOf(ASurvey.ActivePage)
        ASurveyInfo.SurveyDataProvider("-") = Nothing 'L0442
        Response.Redirect(URLParameter(PageURL(NavigationPageID), "guid", ASurveyInfo.AGuid) + "&pid=" + pid.ToString + "&prjid=" + ASurveyInfo.ProjectID.ToString + "&st=" + CInt(ASurveyInfo.SurveyType).ToString + GetTempThemeURI_(True), True) 'L0315 + D3897
    End Sub

    Protected Sub Page_PreRender(sender As Object, e As EventArgs) Handles Me.PreRender
        Dim TypeCnt As Integer = 0
        For Each tItem As ListItem In lbQuestionType.Items
            If tItem.Enabled Then TypeCnt += 1
        Next
        tdType.Visible = TypeCnt > 1
        If AQuestion IsNot Nothing Then
            Select Case AQuestion.Type
                Case QuestionType.qtComment, QuestionType.qtImageCheck
                    tdOptions.Visible = False
                Case QuestionType.qtVariantsCheck
                Case Else
                    divSelCount.Visible = False
            End Select
            If Not divSelCount.Visible AndAlso Not cbRequired.Visible AndAlso Not divLink.Visible Then tdOptions.Visible = False
        End If
    End Sub

    Private Sub EditQuestion_PreLoad(sender As Object, e As EventArgs) Handles Me.PreLoad
        If AQuestion.Type = QuestionType.qtAlternativesSelect And Not IsPostBack AndAlso Not isCallback Then
            cbSelectAllByDefault.Checked = AQuestion.SelectAllByDefault
            cbSelectAllByDefault.InputAttributes.Add("onclick", "$('.alt').prop('checked', this.checked); $('.altsselectall').prop('checked', this.checked);")
        Else
            cbSelectAllByDefault.Visible = False
        End If
    End Sub
End Class
