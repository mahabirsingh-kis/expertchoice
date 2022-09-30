Option Strict Off

'L0028
Imports System.Drawing
Imports SpyronControls.Spyron.Core
'L0040 ===
Imports ECCore
Imports ECCore.MiscFuncs
'L0040 ==

Partial Class Spyron_Surveys_ctrlSurveyPageView
    Inherits UserControl

    Private fSurveyPage As clsSurveyPage = Nothing

    Private fOptionDisplayPageTitle As Boolean = True
    Private fOptionDisplayQuestionEditButton As Boolean = False
    Private fOptionEditButtonText As String = "Edit"
    Private fOptionDeleteButtonText As String = "Delete" 'L0034
    Private fOptionMoveUpButtonText As String = "Move Up" 'L0035
    Private fOptionMoveDownButtonText As String = "Move Down" 'L0035
    Private fstrObjectivesHierarchy As String = "" 'L0035
    Private fstrAlternativesHierarchy As String = "" 'L0035
    Private fSurveyInfo As clsSurveyInfo = Nothing 'L0043
    Private fReadOnlyAnswers As Boolean = False 'L0043
    'L0056 ===
    Private fstrRequiredQuestionAlert As String = "Please answer all the required questions marked with a red asterisk (*)"
    Private fstrMinMaxQuestionAlert As String = "Please, select necessary count of items."
    Private fstrSelectItemsMsg As String = "<div style='font-size:xx-small'><br/>(Select {0})<br/><br/></div>"
    Private fstrMinimumItemsMsg As String = "minimum {0} item(s)"
    Private fstrMaximumItemsMsg As String = "maximum {0} item(s)"
    Private fstrAndMsg As String = " and "
    'L0056 ==

    'L0040 ===
    Private fRespondentID As Integer = -1
    Private fRespondentName As String = "" 'L0043
    Private fRespondentEmail As String = "" 'L0043
    Private fProjectManager As clsProjectManager = Nothing 'L0043

    Public Property RespondentID As Integer
        Get
            Return fRespondentID
        End Get
        Set(value As Integer)
            fRespondentID = value
        End Set
    End Property

    'L0043 ===
    Public Property RespondentName() As String
        Get
            Return fRespondentName
        End Get
        Set(value As String)
            fRespondentName = value
        End Set
    End Property

    Public Property RespondentEMail() As String
        Get
            Return fRespondentEmail
        End Get
        Set(value As String)
            fRespondentEmail = value
        End Set
    End Property
    'L0043 ==

    Public Property ProjectManager() As clsProjectManager 'L0043
        Get
            Return fProjectManager
        End Get
        Set(value As clsProjectManager) 'L0043
            fProjectManager = value
        End Set
    End Property
    'L0040 ==

    'L0043 ===
    Public Property SurveyInfo() As clsSurveyInfo
        Get
            Return fSurveyInfo
        End Get
        Set(value As clsSurveyInfo)
            fSurveyInfo = value
        End Set
    End Property

    Public Property ReadOnlyAnswers() As Boolean
        Get
            Return fReadOnlyAnswers
        End Get
        Set(value As Boolean)
            fReadOnlyAnswers = value
        End Set
    End Property
    'L0043 ==

    Public Property SurveyPage() As clsSurveyPage
        Get
            Return fSurveyPage
        End Get
        Set(value As clsSurveyPage)
            fSurveyPage = value
            If value IsNot Nothing Then UpdateSurveyPageForm()
        End Set
    End Property

    Public Property OptionDisplayPageTitle() As Boolean
        Get
            Return fOptionDisplayPageTitle
        End Get
        Set(value As Boolean)
            fOptionDisplayPageTitle = value
        End Set
    End Property

    Public Property OptionDisplayQuestionEditButton() As Boolean
        Get
            Return fOptionDisplayQuestionEditButton
        End Get
        Set(value As Boolean)
            fOptionDisplayQuestionEditButton = value
        End Set
    End Property

    Public Property OptionEditButtonText() As String
        Get
            Return fOptionEditButtonText
        End Get
        Set(value As String)
            fOptionEditButtonText = value
        End Set
    End Property

    'L0034 ===
    Public Property OptionDeleteButtonText() As String
        Get
            Return fOptionDeleteButtonText
        End Get
        Set(value As String)
            fOptionDeleteButtonText = value
        End Set
    End Property
    'L0034 ==

    'L0035 ===
    Public Property OptionMoveUpButtonText() As String
        Get
            Return fOptionMoveUpButtonText
        End Get
        Set(value As String)
            fOptionMoveUpButtonText = value
        End Set
    End Property

    Public Property OptionMoveDownButtonText() As String
        Get
            Return fOptionMoveDownButtonText
        End Get
        Set(value As String)
            fOptionMoveDownButtonText = value
        End Set
    End Property

    Public Property AlternativeObjectivesHierarchyText() As String
        Get
            Return fstrObjectivesHierarchy
        End Get
        Set(value As String)
            fstrObjectivesHierarchy = value
        End Set
    End Property

    Public Property AlternativeAlternativesHierarchyText() As String
        Get
            Return fstrAlternativesHierarchy
        End Get
        Set(value As String)
            fstrAlternativesHierarchy = value
        End Set
    End Property
    'L0035 ==

    'L0056 ===
    Public Property RequiredQuestionAlert() As String
        Get
            Return fstrRequiredQuestionAlert
        End Get
        Set(value As String)
            fstrRequiredQuestionAlert = value
        End Set
    End Property

    Public Property MinMaxQuestionAlert() As String
        Get
            Return fstrMinMaxQuestionAlert
        End Get
        Set(value As String)
            fstrMinMaxQuestionAlert = value
        End Set
    End Property

    Public Property SelectItemsMsg() As String
        Get
            Return fstrSelectItemsMsg
        End Get
        Set(value As String)
            fstrSelectItemsMsg = value
        End Set
    End Property

    Public Property MinimumItemsMsg() As String
        Get
            Return fstrMinimumItemsMsg
        End Get
        Set(value As String)
            fstrMinimumItemsMsg = value
        End Set
    End Property

    Public Property MaximumItemsMsg() As String
        Get
            Return fstrMaximumItemsMsg
        End Get
        Set(value As String)
            fstrMaximumItemsMsg = value
        End Set
    End Property

    Public Property AndMsg() As String
        Get
            Return fstrAndMsg
        End Get
        Set(value As String)
            fstrAndMsg = value
        End Set
    End Property
    'L0056 ===

    Public Sub UpdateSurveyPageForm()
        'L0061 ===
        If OptionDisplayQuestionEditButton Then
            lblPageTitle.Visible = False
            HeaderPanel.Visible = True
            tbPageTitleEdit.Text = SurveyPage.Title
        Else
            If OptionDisplayPageTitle Then
                lblPageTitle.Text = SurveyPage.Title
            Else
                lblPageTitle.Text = ""
            End If
            pnlSurveyHeader.Visible = lblPageTitle.Text <> "" AndAlso OptionDisplayPageTitle    ' D3984
        End If
        'L0061 ==

        Dim ATableRow As TableRow
        Dim ATableCell As TableCell

        tblSurveyPageForm.Rows.Clear() 'L0036
        tblSurveyPageForm.CssClass = "tableClass"
        ProjectManager.Attributes.ReadAttributes(AttributesStorageType.astStreamsDatabase, ProjectManager.StorageManager.ProjectLocation, ProjectManager.StorageManager.ProviderType, ProjectManager.StorageManager.ModelID)

        For Each AQuestion As clsQuestion In SurveyPage.Questions
            ATableRow = New TableRow()
            tblSurveyPageForm.Rows.Add(ATableRow)

            ATableCell = New TableCell()
            ATableCell.CssClass = "testClass2"
            ATableRow.Cells.Add(ATableCell)
            If AQuestion.Type <> QuestionType.qtComment Then
                If Not SurveyInfo.HideIndexNumbers then ATableCell.Text = "<b>" + SurveyPage.Survey.GetQuestionPageIndex(AQuestion.AGUID).ToString + ".</b>" 'L0478
                'L0046 ===
                If Not AQuestion.AllowSkip Then
                    ATableCell.Text = "<b style=""color:Red"">*</b>" + ATableCell.Text
                End If
                'L0046 ==
            End If
            ATableCell.VerticalAlign = VerticalAlign.Top
            ATableCell.HorizontalAlign = HorizontalAlign.Right 'L0046

            ATableCell = New TableCell()
            ATableRow.Cells.Add(ATableCell)
            ATableCell.VerticalAlign = VerticalAlign.Top

            If Not OptionDisplayQuestionEditButton Then Page.ClientScript.RegisterOnSubmitStatement(GetType(String), "CheckFormSurvey", "if (typeof(NeedCheckStep)=='function' && !NeedCheckStep()) { return true }; ") 'L0440

            Dim ActrlSurveyQuestion As Spyron_Surveys_ctrlSurveyQuestion = Nothing 'L0071
            Dim c1 As UserControl = LoadControl("ctrlSurveyQuestion.ascx")
            'ActrlSurveyQuestion = CType(c1, ASP.spyron_surveys_ctrlsurveyquestion_ascx) 'L0071
            ActrlSurveyQuestion = c1

            ActrlSurveyQuestion.ID = "cQuestion" + AQuestion.AGUID.ToString
            ATableCell.Controls.Add(ActrlSurveyQuestion)

            'L0040 ===
            If ProjectManager IsNot Nothing Then
                If AQuestion.Type = QuestionType.qtObjectivesSelect Then
                    ActrlSurveyQuestion.Hierarchy = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy)
                End If

                If AQuestion.Type = QuestionType.qtAlternativesSelect Then
                    ActrlSurveyQuestion.Hierarchy = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy)
                End If
            End If
            'L0040 ==

            ActrlSurveyQuestion.AlternativeAlternativesHierarchyText = AlternativeAlternativesHierarchyText 'L0035
            ActrlSurveyQuestion.AlternativeObjectivesHierarchyText = AlternativeObjectivesHierarchyText 'L0035
            'L0056 ===
            ActrlSurveyQuestion.RequiredQuestionAlert = RequiredQuestionAlert
            ActrlSurveyQuestion.MinMaxQuestionAlert = MinMaxQuestionAlert
            ActrlSurveyQuestion.SelectItemsMsg = SelectItemsMsg
            ActrlSurveyQuestion.MinimumItemsMsg = MinimumItemsMsg
            ActrlSurveyQuestion.MaximumItemsMsg = MaximumItemsMsg
            ActrlSurveyQuestion.AndMsg = AndMsg
            ActrlSurveyQuestion.PageIndex = SurveyPage.Survey.GetQuestionPageIndex(AQuestion.AGUID)
            'L0056 ==

            'L0043 ===
            If SurveyInfo IsNot Nothing Then
                Dim Respondent As clsRespondent = SurveyInfo.Survey(RespondentEMail).RespondentByEmail(RespondentEMail) 'L0442
                If Respondent IsNot Nothing Then
                    ActrlSurveyQuestion.Answer = Respondent.AnswerByQuestionGUID(AQuestion.AGUID)
                    If AQuestion.Type = QuestionType.qtObjectivesSelect Or AQuestion.Type = QuestionType.qtAlternativesSelect Then
                        ActrlSurveyQuestion.UserID = ProjectManager.User.UserID
                    End If
                End If
            End If
            'L0043 ==
            If OptionDisplayQuestionEditButton Then ActrlSurveyQuestion.ViewMode = 1 'L0046
            ActrlSurveyQuestion.SurveyInfo = SurveyInfo ' D1278
            ActrlSurveyQuestion.Question = AQuestion

            If OptionDisplayQuestionEditButton Then
                ATableCell = New TableCell()
                ATableCell.VerticalAlign = VerticalAlign.Top 'L0034
                ATableRow.Cells.Add(ATableCell)

                'L0035 ===
                Dim AEditImgBtn As New ImageButton()
                AEditImgBtn.ToolTip = OptionEditButtonText
                AEditImgBtn.ID = "ibtnEdit" + AQuestion.AGUID.ToString
                AEditImgBtn.ImageUrl = "Images/IconEdit.gif"
                AEditImgBtn.BackColor = Color.Transparent
                AEditImgBtn.Attributes.Add("value", AQuestion.AGUID.ToString)
                AEditImgBtn.BorderWidth = 2
                AEditImgBtn.BorderColor = Color.Transparent
                AddHandler AEditImgBtn.Click, AddressOf AQuestionButtonClick
                ATableCell.Controls.Add(AEditImgBtn)

                Dim AUpImgBtn As New ImageButton()
                AUpImgBtn.ToolTip = OptionMoveUpButtonText
                AUpImgBtn.ID = "ibtnUp" + AQuestion.AGUID.ToString
                AUpImgBtn.ImageUrl = "Images/IconUp.gif"
                AUpImgBtn.BackColor = Color.Transparent
                AUpImgBtn.Attributes.Add("value", AQuestion.AGUID.ToString)
                AUpImgBtn.BorderWidth = 2
                AUpImgBtn.BorderColor = Color.Transparent
                AddHandler AUpImgBtn.Click, AddressOf AQuestionButtonClick
                ATableCell.Controls.Add(AUpImgBtn)

                Dim ADownImgBtn As New ImageButton()
                ADownImgBtn.ToolTip = OptionMoveDownButtonText
                ADownImgBtn.ID = "ibtnDown" + AQuestion.AGUID.ToString
                ADownImgBtn.ImageUrl = "Images/IconDown.gif"
                ADownImgBtn.BackColor = Color.Transparent
                ADownImgBtn.Attributes.Add("value", AQuestion.AGUID.ToString)
                ADownImgBtn.BorderWidth = 2
                ADownImgBtn.BorderColor = Color.Transparent
                AddHandler ADownImgBtn.Click, AddressOf AQuestionButtonClick
                ATableCell.Controls.Add(ADownImgBtn)

                Dim ADeleteImgBtn As New ImageButton()
                ADeleteImgBtn.ToolTip = OptionDeleteButtonText
                ADeleteImgBtn.ID = "ibtnDelete" + AQuestion.AGUID.ToString
                ADeleteImgBtn.ImageUrl = "Images/IconDelete.gif"
                ADeleteImgBtn.BackColor = Color.Transparent
                ADeleteImgBtn.Attributes.Add("value", AQuestion.AGUID.ToString)
                ADeleteImgBtn.BorderWidth = 2
                ADeleteImgBtn.BorderColor = Color.Transparent
                ADeleteImgBtn.OnClientClick = "return ConfirmDelete();"
                AddHandler ADeleteImgBtn.Click, AddressOf AQuestionButtonClick
                ATableCell.Controls.Add(ADeleteImgBtn)
                'L0035 ==
                If Not AQuestion.LinkedAttributeID.Equals(Guid.Empty) Then
                    Dim AInfoText As New Label()
                    For Each attr In ProjectManager.Attributes.GetUserAttributes
                        If attr.ID.Equals(AQuestion.LinkedAttributeID) Then
                            AInfoText.Text = String.Format("<br/>Linked to [{0}]", attr.Name)
                            AInfoText.Font.Italic = True
                            AInfoText.Font.Size = New FontUnit(8)
                            Exit For
                        End If
                    Next
                    ATableCell.Controls.Add(AInfoText)
                End If
            End If
        Next
    End Sub

    Public Event QuestionButtonClick(AQuestion As clsQuestion, Action As String) 'L0035
    Public Event PageTitleButtonClick(NewTitle As String) 'L0061

    'L0035 ===
    Public Sub AQuestionButtonClick(sender As Object, e As ImageClickEventArgs)
        Dim Abtn As ImageButton
        Abtn = CType(sender, ImageButton)
        For Each AQuestion As clsQuestion In SurveyPage.Questions
            If AQuestion.AGUID.ToString = Abtn.Attributes("value") Then
                If Abtn.ID.Contains("ibtnEdit") Then RaiseEvent QuestionButtonClick(AQuestion, "Edit")
                If Abtn.ID.Contains("ibtnDelete") Then RaiseEvent QuestionButtonClick(AQuestion, "Delete")
                If Abtn.ID.Contains("ibtnUp") Then RaiseEvent QuestionButtonClick(AQuestion, "Up")
                If Abtn.ID.Contains("ibtnDown") Then RaiseEvent QuestionButtonClick(AQuestion, "Down")
                UpPanel.Update()
                Exit For
            End If
        Next
    End Sub
    'L0035 ==

    Public Function SetUserAttribute(ID As Guid, UserID As Integer, ValueType As AttributeValueTypes, Value As Object) As Boolean
        With ProjectManager
            If .Attributes IsNot Nothing Then
                Dim res As Boolean
                Select Case ValueType
                    Case AttributeValueTypes.avtString
                        res = .Attributes.SetAttributeValue(ID, UserID, ValueType, CStr(Value), Guid.Empty, Guid.Empty)
                    Case AttributeValueTypes.avtLong
                        Dim v As Long
                        If Long.TryParse(CStr(Value), v) Then
                            res = .Attributes.SetAttributeValue(ID, UserID, ValueType, v, Guid.Empty, Guid.Empty)
                        End If
                    Case AttributeValueTypes.avtDouble
                        Dim v As Double ' D1858
                        If String2Double(CStr(Value), v) Then  ' D1858
                            res = .Attributes.SetAttributeValue(ID, UserID, ValueType, CDbl(v), Guid.Empty, Guid.Empty)
                        End If
                    Case AttributeValueTypes.avtBoolean
                        Dim v As Boolean
                        If Str2Bool(CStr(Value), v) Then
                            res = .Attributes.SetAttributeValue(ID, UserID, ValueType, v, Guid.Empty, Guid.Empty)
                        End If
                End Select
                .Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID, UserID)
                Return res
            End If
        End With
        Return False
    End Function


    'L0040 ===
    Public Sub ReadPageAnswers(ByRef NeedToRebuildPipe As Boolean)
        Dim AQuestionControl As Spyron_Surveys_ctrlSurveyQuestion
        'L0043 ==
        If SurveyPage IsNot Nothing AndAlso SurveyPage.Survey IsNot Nothing Then 'L0502
            Dim Respondent As clsRespondent = SurveyPage.Survey.RespondentByEmail(RespondentEMail)
            If Respondent Is Nothing Then
                Respondent = New clsRespondent
                Respondent.Email = RespondentEMail
                Respondent.Name = RespondentName
                SurveyInfo.SurveyDataProvider(RespondentEMail).SaveStreamRespondentAnswers(Respondent) 'L0442
                Respondent = SurveyPage.Survey.RespondentByEmail(RespondentEMail)
            End If
            'L0043 ==
            If Respondent IsNot Nothing Then
                For Each AQuestion As clsQuestion In SurveyPage.Questions
                    AQuestionControl = Nothing
                    AQuestionControl = tblSurveyPageForm.FindControl("cQuestion" + AQuestion.AGUID.ToString)
                    If AQuestionControl IsNot Nothing Then
                        If AQuestion.Type = QuestionType.qtAlternativesSelect Or _
                            AQuestion.Type = QuestionType.qtObjectivesSelect Or _
                            AQuestion.LinkedAttributeID <> Guid.Empty Then NeedToRebuildPipe = True
                        AQuestionControl.ReadAnswer()
                        If Not Respondent.Answers.Contains(AQuestionControl.Answer) And AQuestionControl.Answer IsNot Nothing Then
                            Respondent.Answers.Add(AQuestionControl.Answer)
                        End If

                        If AQuestionControl.Answer IsNot Nothing And Not AQuestion.LinkedAttributeID.Equals(Guid.Empty) Then
                            Dim AAttribute As clsAttribute = Nothing
                            SetUserAttribute(AQuestion.LinkedAttributeID, Respondent.ID, AttributeValueTypes.avtString, AQuestionControl.Answer.AnswerValuesString)
                            For Each atr In ProjectManager.Attributes.GetUserAttributes
                                If atr.ID.Equals(AQuestion.LinkedAttributeID) Then
                                    AAttribute = atr
                                    Exit For
                                End If
                            Next
                            If AAttribute IsNot Nothing Then
                                Dim AnswerString As String = ""
                                If AQuestionControl.Answer IsNot Nothing Then AnswerString = AQuestionControl.Answer.AnswerValuesString.Trim()
                                SetUserAttribute(AQuestion.LinkedAttributeID, Respondent.ID, AAttribute.ValueType, AnswerString)
                            End If
                        End If

                        '=s080614 - Place Rated Check
                        If (AQuestion.Type = QuestionType.qtImageCheck) And (ProjectManager IsNot Nothing) Then
                            Dim ListOfStates As New ArrayList
                            ListOfStates.Clear()
                            For Each AVariant As clsVariant In AQuestionControl.Answer.AnswerVariants
                                ListOfStates.Add(AVariant.Text)
                            Next
                            'SetEnabledNodesForPlacesRated(ProjectManager, ProjectManager.UserID, ListOfStates)
                        End If
                        '==s080614 - Place Rated Check

                        If (AQuestion.Type = QuestionType.qtAlternativesSelect Or AQuestion.Type = QuestionType.qtObjectivesSelect) And (ProjectManager IsNot Nothing) Then
                            ProjectManager.StorageManager.Writer.SaveUserDisabledNodes(ProjectManager.User)
                            ProjectManager.PipeBuilder.PipeCreated = False 'L0043
                            ProjectManager.PipeBuilder.CreatePipe() 'L0043
                        End If
                    End If
                Next
                If Not ReadOnlyAnswers Then
                    SurveyInfo.SurveyDataProvider(Respondent.Email).SaveStreamRespondentAnswers(Respondent) 'L0442
                    SurveyInfo.SurveyDataProvider(Respondent.Email) = Nothing 'L0442
                End If
            End If
        End If
    End Sub
    'L0040 ==

    'L0061 ===
    'Protected Sub btnRenamePage_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnRenamePage.Click
    '    btnRenamePage.Enabled = False
    'End Sub

    Protected Sub tbPageTitleEdit_TextChanged(sender As Object, e As EventArgs) Handles tbPageTitleEdit.TextChanged
        RaiseEvent PageTitleButtonClick(tbPageTitleEdit.Text)
        'btnRenamePage.Enabled = False
    End Sub
    'L0061 ==
End Class
