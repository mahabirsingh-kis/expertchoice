Option Strict Off

Imports System.Drawing
Imports SpyronControls.Spyron.Core
Imports System.Web.Hosting
Imports ECCore.MiscFuncs
Imports Telerik.Web.UI

Namespace SpyronPageControls

    Partial Class Spyron_Surveys_ctrlSurveyPage
        Inherits UserControl

        ''' <summary>
        ''' Specify View mode of SurveyPage control
        ''' </summary>
        ''' <remarks></remarks>
        Enum ctrlSurveyPageMode
            cspmTest = 0
            cspmLaunched = 1
            cspmEdit = 2
        End Enum

        Public TempThemeURI As String = "" 'L0424

        Private fSurveyInfo As clsSurveyInfo
        Private fMode As ctrlSurveyPageMode = ctrlSurveyPageMode.cspmTest

        Private fRespondentEMail As String = ""
        Private fRespondentName As String = ""
        Private fReadOnlyInput As Boolean = False

        Private fProjectManager As clsProjectManager = Nothing 'L0040

        Public Property ProjectManager() As clsProjectManager 'L0040
            Get
                Return fProjectManager
            End Get
            Set(value As clsProjectManager) 'L0040
                fProjectManager = value
            End Set
        End Property

        Public Property ReadOnlyInput() As Boolean
            Get
                Return fReadOnlyInput
            End Get
            Set(value As Boolean)
                fReadOnlyInput = value
            End Set
        End Property

        ''' <summary>
        ''' Specify current user's E-mail
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property RespondentEMail() As String
            Get
                Return fRespondentEMail
            End Get
            Set(value As String)
                fRespondentEMail = value
            End Set
        End Property

        ''' <summary>
        ''' Specify current user's Name
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property RespondentName() As String
            Get
                Return fRespondentName
            End Get
            Set(value As String)
                fRespondentName = value
            End Set
        End Property

        Public Property SurveyInfo() As clsSurveyInfo
            Get
                Return fSurveyInfo
            End Get
            Set(value As clsSurveyInfo)
                fSurveyInfo = value
            End Set
        End Property

        Public ReadOnly Property Survey() As clsSurvey
            Get
                Return SurveyInfo.Survey(RespondentEMail) 'L0442
            End Get
        End Property

        ''L0020 ===
        '''' <summary>
        '''' Specify Spyron Survey ID
        '''' </summary>
        '''' <value></value>
        '''' <returns></returns>
        '''' <remarks></remarks>
        'Public Property SurveyID() As Integer
        '    Get
        '        Return fSurveyID
        '    End Get
        '    Set(ByVal value As Integer)
        '        fSurveyID = value
        '    End Set
        'End Property

        '' D0376 ===
        'Public Property StorageType() As SurveyStorageType
        '    Get
        '        Return fStorageType
        '    End Get
        '    Set(ByVal value As SurveyStorageType)
        '        fStorageType = value
        '    End Set
        'End Property

        'Public Property DBName() As String
        '    Get
        '        Return fDBName
        '    End Get
        '    Set(ByVal value As String)
        '        fDBName = value
        '    End Set
        'End Property
        '' D0376 ==

        'Public ReadOnly Property ProjectDBName() As String
        '    Get
        '        If fDP IsNot Nothing Then
        '            Return fDP.SQLProjectDBPrefix + SurveyID.ToString
        '        Else
        '            Return ""
        '        End If
        '    End Get
        'End Property

        ''L0020 ==
        '''' <summary>
        '''' Contains routines for database interactions
        '''' </summary>
        '''' <value></value>
        '''' <returns></returns>
        '''' <remarks></remarks>
        'Public Property SpyronDataProvider() As SpyronControls.Spyron.Data.clsSurveyDataProvider
        '    Get
        '        Return fDP
        '    End Get
        '    Set(ByVal value As SpyronControls.Spyron.Data.clsSurveyDataProvider)
        '        fDP = value
        '    End Set
        'End Property

        '
        ''' <summary>
        ''' Specify Survey Page view mode (Edit / Launched / Test)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Mode() As ctrlSurveyPageMode
            Get
                Return fMode
            End Get

            Set(value As ctrlSurveyPageMode)
                'depends on mode show or hide controls
                fMode = value

                If Mode = ctrlSurveyPageMode.cspmEdit Then
                    ToolbarPanel.Visible = True
                    PageEditArea.Visible = True
                    tbQuestionEdit.Visible = True
                    tbPageEdit.Visible = True
                    QuestionEditPanel.Visible = True
                    NavigationCell.Visible = True
                Else
                    Survey.SelectedQuestionGUID = Nothing ' D0385
                    Survey.ActivePageGUID = Nothing ' D0385
                    ToolbarPanel.Visible = False
                    PageEditArea.Visible = False
                    tbQuestionEdit.Visible = False
                    tbPageEdit.Visible = False
                    QuestionEditPanel.Visible = False
                End If

                ' Hide Navigation buttons for Launched mode (use Pipe navigation instead)
                If Mode = ctrlSurveyPageMode.cspmLaunched Then
                    NavigationCell.Visible = False '--sl temp
                End If
            End Set
        End Property


        ''' <summary>
        ''' Create table which contains necessary web controls for specified survey question
        ''' </summary>
        ''' <param name="QuestionGUID">Specify Question ID</param>
        ''' <returns>table which contains necessary web controls for specified survey QuestionID</returns>
        ''' <remarks></remarks>
        Public Function GetQuestionForm(QuestionGUID As Guid) As Table

            ' Create Container for web controls
            Dim ATable As New Table()
            Dim ATableRow As New TableRow()
            Dim ATableCell As New TableCell()

            ' Get Question by GUID
            Dim AQuestion As clsQuestion = Survey.QuestionByGUID(QuestionGUID)
            ATable.Width = Unit.Percentage(100)
            ATable.CssClass = "testClass"
            ' Change background for selected question
            If (Survey.SelectedQuestionGUID = QuestionGUID) And (Mode = ctrlSurveyPageMode.cspmEdit) Then 'L0026
                ATableRow.BackColor = Color.White
            End If

            ATable.Rows.Add(ATableRow)
            ATableCell.Width = Unit.Percentage(90)
            ATableRow.Cells.Add(ATableCell)

            ' Create label  for question text
            Dim ALabel As New Label()
            ALabel.ID = "lblQText" + AQuestion.AGUID.ToString 'L0020

            If Mode = ctrlSurveyPageMode.cspmEdit Then
                ' if Survey Page in Edit mode then add link for editing question 
                ALabel.Text = String.Format("<a href='SurveyEdit.aspx?id={0}&qid={1}{3}'>{2}</a>", SurveyInfo.ID, QuestionGUID, AQuestion.Text, TempThemeURI)
            Else
                ' else - just add question text
                ALabel.Text = AQuestion.Text
                '=sl080601
                If AQuestion.Type = QuestionType.qtImageCheck Then
                    ALabel.Text = SurveyInfo.SurveyDataProvider(RespondentEMail).File_GetContent(HostingEnvironment.ApplicationPhysicalPath + "Project\Evaluate\usa_map_selector.htm") 'L0442
                End If
                '==sl080601
            End If

            ' add number (index of question) befor question
            If (AQuestion.Visible) And (AQuestion.Type <> QuestionType.qtComment) And (AQuestion.Type <> QuestionType.qtImageCheck) Then
               If Not SurveyInfo.HideIndexNumbers then ALabel.Text = "<b>" + Survey.GetQuestionPageIndex(AQuestion.AGUID).ToString + ". </b>" + ALabel.Text 'L0478
            End If

            ATableCell.Controls.Add(ALabel)
            Dim ARespondent As clsRespondent = Survey.RespondentByEmail(RespondentEMail)

            'for each type of questions creating necessary controls
            Select Case AQuestion.Type
                Case QuestionType.qtComment
                    'ALabel.CssClass = "projectcomment"
                Case QuestionType.qtOpenLine
                    ALabel.Text = ALabel.Text + "&nbsp;"
                    Dim ATextBox As New TextBox
                    ATextBox.ID = "tbOpenTextBox" + AQuestion.AGUID.ToString + "s" 'L0020
                    ATextBox.TextMode = TextBoxMode.SingleLine

                    'Fill textbox with Respondent answer if it is necessary
                    If Not ARespondent Is Nothing And Mode = ctrlSurveyPageMode.cspmLaunched Then
                        Dim AAnswer As clsAnswer
                        AAnswer = ARespondent.AnswerByQuestionGUID(AQuestion.AGUID)
                        If Not AAnswer Is Nothing Then
                            If AAnswer.AnswerVariants.Count > 0 Then
                                ATextBox.Text = CType(AAnswer.AnswerVariants(0), clsVariant).VariantValue
                            End If
                        End If
                    End If
                    ATableCell.Controls.Add(ATextBox)

                Case QuestionType.qtNumber
                    ALabel.Text = ALabel.Text + "&nbsp;"
                    Dim ATextBox As New TextBox
                    ATextBox.ID = "tbNumberBox" + AQuestion.AGUID.ToString + "s"
                    ATextBox.TextMode = TextBoxMode.SingleLine

                    'Fill textbox with Respondent answer if it is necessary
                    If Not ARespondent Is Nothing And Mode = ctrlSurveyPageMode.cspmLaunched Then
                        Dim AAnswer As clsAnswer
                        AAnswer = ARespondent.AnswerByQuestionGUID(AQuestion.AGUID)
                        If Not AAnswer Is Nothing Then
                            If AAnswer.AnswerVariants.Count > 0 Then
                                ATextBox.Text = CType(AAnswer.AnswerVariants(0), clsVariant).VariantValue
                            End If
                        End If
                    End If
                    ATableCell.Controls.Add(ATextBox)

                Case QuestionType.qtNumberColumn
                    ALabel.Text = ALabel.Text + "&nbsp;"
                    Dim ATextBox As New TextBox
                    ATextBox.ID = "tbNumberBox" + AQuestion.AGUID.ToString + "s"
                    ATextBox.TextMode = TextBoxMode.MultiLine 

                    'Fill textbox with Respondent answer if it is necessary
                    If Not ARespondent Is Nothing And Mode = ctrlSurveyPageMode.cspmLaunched Then
                        Dim AAnswer As clsAnswer
                        AAnswer = ARespondent.AnswerByQuestionGUID(AQuestion.AGUID)
                        If Not AAnswer Is Nothing Then
                            If AAnswer.AnswerVariants.Count > 0 Then
                                ATextBox.Text = CType(AAnswer.AnswerVariants(0), clsVariant).VariantValue
                            End If
                        End If
                    End If
                    ATableCell.Controls.Add(ATextBox)

                Case QuestionType.qtOpenMemo
                    ALabel.Text = ALabel.Text + "<br /><br />"
                    Dim ATextBox As New TextBox
                    ATextBox.ID = "tbOpenTextBox" + AQuestion.AGUID.ToString + "s" 'L0020
                    ATextBox.TextMode = TextBoxMode.MultiLine
                    ATextBox.Width = Unit.Pixel(350)
                    ATextBox.Rows = 3

                    'Fill textbox with Respondent answer if it is necessary
                    If Not ARespondent Is Nothing And Mode = ctrlSurveyPageMode.cspmLaunched Then
                        Dim AAnswer As clsAnswer
                        AAnswer = ARespondent.AnswerByQuestionGUID(AQuestion.AGUID) 'L0020
                        If Not AAnswer Is Nothing Then
                            If AAnswer.AnswerVariants.Count > 0 Then
                                ATextBox.Text = CType(AAnswer.AnswerVariants(0), clsVariant).VariantValue
                            End If
                        End If
                    End If
                    ATableCell.Controls.Add(ATextBox)

                Case QuestionType.qtVariantsCheck, QuestionType.qtImageCheck
                    Dim isOther As Boolean = False
                    If AQuestion.Type <> QuestionType.qtImageCheck Then
                        ALabel.Text = ALabel.Text + "<br /><br />"
                    End If

                    Dim ACheckBox As CheckBox

                    Dim AAnswerGrid As New Table
                    Dim AAnswerRow As New TableRow
                    Dim AAnswerCell As New TableCell
                    Dim AAnswersPerCellCount As Integer = 0

                    If AQuestion.Variants.Count > 10 Then
                        ATableCell.Controls.Add(AAnswerGrid)
                        AAnswerGrid.Rows.Add(AAnswerRow)
                    End If

                    For Each answer As clsVariant In AQuestion.Variants
                        If answer.Type = VariantType.vtOtherLine Or answer.Type = VariantType.vtOtherMemo Then
                            isOther = True
                        Else
                            ACheckBox = New CheckBox
                            ACheckBox.ID = "cbVariants" + AQuestion.AGUID.ToString + "s" + answer.AGUID.ToString + "s" 'L0020
                            ACheckBox.Text = answer.Text + "<br />"
                            ACheckBox.InputAttributes.Add("value", answer.Text)
                            If AQuestion.Type = QuestionType.qtImageCheck Then
                                ACheckBox.InputAttributes.Add("style", "display:none;")
                                ACheckBox.Attributes.Add("style", "display:none;")
                            End If

                            'set checked property for each variant as Respondent answered if necessary
                            If Not ARespondent Is Nothing And Mode = ctrlSurveyPageMode.cspmLaunched Then
                                Dim AAnswer As clsAnswer
                                AAnswer = ARespondent.AnswerByQuestionGUID(AQuestion.AGUID)
                                If Not AAnswer Is Nothing Then
                                    If AAnswer.AnswerVariants.Contains(answer) Then
                                        ACheckBox.Checked = True
                                    End If
                                End If
                            End If
                            If AQuestion.Variants.Count > 10 Then
                                AAnswerCell.Controls.Add(ACheckBox)
                                AAnswersPerCellCount += 1
                                If AAnswersPerCellCount > 3 Then
                                    AAnswerRow.Cells.Add(AAnswerCell)
                                    AAnswersPerCellCount = 0
                                    AAnswerCell = New TableCell()
                                End If
                            Else
                                ATableCell.Controls.Add(ACheckBox)
                            End If


                        End If
                    Next

                    If AQuestion.Variants.Count > 10 Then
                        AAnswerRow.Cells.Add(AAnswerCell)
                    End If

                    If AQuestion.Type = QuestionType.qtImageCheck And Mode = ctrlSurveyPageMode.cspmLaunched And Survey.ActivePage.GetQuestionByGUID(AQuestion.AGUID) IsNot Nothing Then 'L0011 Not IsPostBack And 'L0012 + 'L0020 'L0478
                        ScriptManager.RegisterStartupScript(Me, GetType(String), "InitCB", "setTimeout('if ((isScript)) {Init()};', 1000);", True) ''statefs('')
                    End If

                    'add other textbox input if necessary
                    If isOther Then
                        Dim AOtherLabel As New Label
                        Dim ATextBox As New TextBox
                        AOtherLabel.Text = "Other:   "
                        ATableCell.Controls.Add(AOtherLabel)
                        ATextBox.ID = "tbOther" + AQuestion.AGUID.ToString + "s"
                        ATextBox.TextMode = TextBoxMode.SingleLine

                        ' set Respondent answer to Other field
                        If Not ARespondent Is Nothing And Mode = ctrlSurveyPageMode.cspmLaunched Then
                            Dim AAnswer As clsAnswer
                            AAnswer = ARespondent.AnswerByQuestionGUID(AQuestion.AGUID)
                            If Not AAnswer Is Nothing Then
                                For Each answer As clsVariant In AAnswer.AnswerVariants
                                    If (answer.Type = VariantType.vtOtherLine) Or _
                                       (answer.Type = VariantType.vtOtherMemo) Then
                                        ATextBox.Text = answer.VariantValue
                                    End If
                                Next
                            End If
                        End If

                        ATableCell.Controls.Add(ATextBox)
                    End If

                Case QuestionType.qtVariantsRadio
                    Dim isOther As Boolean = False
                    ALabel.Text = ALabel.Text + "<br /><br />"
                    Dim ARadioList As New RadioButtonList
                    ARadioList.ID = "rbVariants" + AQuestion.AGUID.ToString + "s"
                    For Each answer As clsVariant In AQuestion.Variants
                        If answer.Type = VariantType.vtOtherLine Or answer.Type = VariantType.vtOtherMemo Then
                            isOther = True
                        Else
                            Dim LI As New ListItem
                            LI.Text = answer.Text
                            LI.Value = answer.AGUID.ToString

                            'set Selected property for each variant as Respondent answered if necessary
                            If Not ARespondent Is Nothing And Mode = ctrlSurveyPageMode.cspmLaunched Then
                                Dim AAnswer As clsAnswer
                                AAnswer = ARespondent.AnswerByQuestionGUID(AQuestion.AGUID)
                                If Not AAnswer Is Nothing Then
                                    If AAnswer.AnswerVariants.Contains(answer) Then
                                        LI.Selected = True
                                    End If
                                End If
                            End If

                            ARadioList.Items.Add(LI)
                        End If
                    Next
                    ATableCell.Controls.Add(ARadioList)

                    'add other textbox input if necessary
                    If isOther Then
                        Dim AOtherLabel As New Label
                        Dim ATextBox As New TextBox
                        AOtherLabel.Text = "Other:   "
                        ATableCell.Controls.Add(AOtherLabel)
                        ATextBox.ID = "tbOther" + AQuestion.AGUID.ToString + "s"
                        ATextBox.TextMode = TextBoxMode.SingleLine

                        ' set Respondent answer to Other field
                        If Not ARespondent Is Nothing And Mode = ctrlSurveyPageMode.cspmLaunched Then
                            Dim AAnswer As clsAnswer
                            AAnswer = ARespondent.AnswerByQuestionGUID(AQuestion.AGUID)
                            If Not AAnswer Is Nothing Then
                                For Each answer As clsVariant In AAnswer.AnswerVariants
                                    If (answer.Type = VariantType.vtOtherLine) Or _
                                       (answer.Type = VariantType.vtOtherMemo) Then
                                        If answer.VariantValue <> "" Then ATextBox.Text = answer.VariantValue
                                    End If
                                Next
                            End If
                        End If

                        ATableCell.Controls.Add(ATextBox)
                    End If

                Case QuestionType.qtVariantsCombo
                    Dim isOther As Boolean = False
                    ALabel.Text = ALabel.Text + "&nbsp;"
                    Dim ADropDownList As New DropDownList
                    ADropDownList.ID = "ddVariants" + AQuestion.AGUID.ToString + "s"
                    For Each answer As clsVariant In AQuestion.Variants
                        Dim LI As New ListItem
                        LI.Text = answer.Text
                        LI.Value = answer.AGUID.ToString

                        'set Selected variant as Respondent answered if necessary
                        If Not ARespondent Is Nothing And Mode = ctrlSurveyPageMode.cspmLaunched Then
                            Dim AAnswer As clsAnswer
                            AAnswer = ARespondent.AnswerByQuestionGUID(AQuestion.AGUID)
                            If Not AAnswer Is Nothing Then
                                If AAnswer.AnswerVariants.Contains(answer) Then
                                    LI.Selected = True
                                End If
                            End If
                        End If
                        ADropDownList.Items.Add(LI)
                        'If answer.Type = VariantType.vtOtherLine Or answer.Type = VariantType.vtOtherMemo Then
                        '    isOther = True
                        'End If
                    Next
                    ATableCell.Controls.Add(ADropDownList)

                    'L0018 ===
                Case QuestionType.qtObjectivesSelect
                    ALabel.Text = ALabel.Text + "<br/>"
                    If ProjectManager IsNot Nothing Then
                        Dim ACheckBox As CheckBox

                        Dim AHierarchy As clsHierarchy
                        AHierarchy = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy)
                        For Each node As clsNode In AHierarchy.Nodes
                            ACheckBox = New CheckBox
                            ACheckBox.ID = "cbObjectives" + AQuestion.AGUID.ToString + "s" + node.NodeID.ToString + "s"
                            Dim strLevel As String = ""
                            For alevel As Integer = 1 To node.Level
                                strLevel += "&nbsp;&nbsp; " 'L0041
                            Next

                            'L0041 ===
                            Dim ALevelLabel As New Label
                            ALevelLabel.Text = strLevel
                            ATableCell.Controls.Add(ALevelLabel)
                            'L0041 ==

                            ACheckBox.Text = node.NodeName + "<br />" 'L0041
                            ACheckBox.InputAttributes.Add("value", node.NodeID.ToString)
                            ACheckBox.Checked = Not node.DisabledForUser(ProjectManager.User.UserID)
                            If node.ParentNode Is Nothing Then ACheckBox.Enabled = False 'L0040
                            ATableCell.Controls.Add(ACheckBox)
                        Next
                    End If

                Case QuestionType.qtAlternativesSelect
                    ALabel.Text = ALabel.Text + "<br/>"
                    If ProjectManager IsNot Nothing Then
                        Dim ACheckBox As CheckBox

                        Dim AHierarchy As clsHierarchy
                        AHierarchy = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy)
                        For Each node As clsNode In AHierarchy.Nodes
                            ACheckBox = New CheckBox
                            ACheckBox.ID = "cbAlternatives" + AQuestion.AGUID.ToString + "s" + node.NodeID.ToString + "s"
                            Dim strLevel As String = ""
                            ACheckBox.Text = node.NodeName + "<br />"
                            ACheckBox.InputAttributes.Add("value", node.NodeID.ToString)
                            ACheckBox.Checked = Not node.DisabledForUser(ProjectManager.User.UserID)
                            ATableCell.Controls.Add(ACheckBox)
                        Next
                    End If
                    'L0018 ==
            End Select
            Return ATable
        End Function

        Public Sub SaveChanges()
            Dim SelectedQuestionGUID As Guid = Survey.SelectedQuestionGUID
            Dim ActivePageGUID As Guid = Survey.ActivePageGUID 'L0020
            SurveyInfo.SaveSurvey(False) 'L0441
            Survey.ActivePageGUID = ActivePageGUID 'L0020
            Survey.SelectedQuestionGUID = SelectedQuestionGUID
        End Sub

        Public Sub InitControl()
            Dim AWizardStep As View
            Dim APageButton As Button
            If SurveyInfo IsNot Nothing Then
                If Survey Is Nothing Then
                    SurveyInfo.SurveyDataProvider(RespondentEMail).OpenSurvey(RespondentEMail) 'L0442
                Else
                    Dim SelectedQuestionGUID As Guid = Survey.SelectedQuestionGUID
                    Dim ActivePageGUID As Guid = Survey.ActivePageGUID
                    SurveyInfo.SurveyDataProvider(RespondentEMail).OpenSurvey(RespondentEMail) 'L0442
                    Survey.ActivePageGUID = ActivePageGUID
                    Survey.SelectedQuestionGUID = SelectedQuestionGUID
                End If

                For Each APage As clsSurveyPage In Survey.Pages
                    If SurveyWizard.FindControl("Page" + APage.AGUID.ToString) Is Nothing Then
                        AWizardStep = New View
                        AWizardStep.ID = "Page" + APage.AGUID.ToString
                        SurveyWizard.Views.Add(AWizardStep)
                        Dim APagerRow As TableRow
                        Dim APagerCell As TableCell
                        APagerRow = New TableRow
                        APagerCell = New TableCell
                        APagerRow.Cells.Add(APagerCell)
                        PagerTable.Rows.Add(APagerRow)
                        APageButton = New Button
                        APageButton.ID = "btnPage" + APage.AGUID.ToString
                        APageButton.Text = APage.Title
                        APageButton.Width = Unit.Pixel(150)
                        APageButton.CssClass = "button"
                        AddHandler APageButton.Click, AddressOf btnPager_Click
                        APagerCell.Controls.Add(APageButton)
                    Else
                        AWizardStep = SurveyWizard.FindControl("Page" + APage.AGUID.ToString)
                        APageButton = PageEditArea.FindControl("btnPage" + APage.AGUID.ToString)
                    End If

                    If APage.AGUID = Survey.ActivePageGUID Then
                        APageButton.Enabled = False
                    Else
                        APageButton.Enabled = True
                    End If
                    AWizardStep.Controls.Clear()
                    Dim ATable As New Table()
                    Dim ATableCell As TableCell
                    Dim ATableRow As TableRow
                    Dim ALabel As Label

                    ALabel = New Label()
                    ALabel.Width = Unit.Percentage(100)
                    ALabel.ID = "lblPageTitle" + APage.AGUID.ToString
                    ALabel.Text = APage.Title
                    ALabel.CssClass = "h4"

                    AWizardStep.Controls.Add(ALabel)
                    ATable.ID = "tblPage" + APage.AGUID.ToString
                    ATable.Width = Unit.Percentage(100)
                    AWizardStep.Controls.Add(ATable)
                    For Each AQuestion As clsQuestion In APage.Questions
                        If (Mode = ctrlSurveyPageMode.cspmEdit) Or (AQuestion.Visible) Then
                            ATableRow = New TableRow()
                            ATableRow.ID = "tblrQuestion" + AQuestion.AGUID.ToString
                            ATable.Rows.Add(ATableRow)
                            ATableCell = New TableCell()
                            ATableRow.Cells.Add(ATableCell)
                            ATableCell.Text = AQuestion.Text
                            ATableCell.Controls.Add(GetQuestionForm(AQuestion.AGUID))
                        End If
                    Next
                Next

                If Not Survey.ActivePage Is Nothing Then
                    SurveyWizard.ActiveViewIndex = Survey.Pages.IndexOf(Survey.ActivePage)
                End If

                If Not Survey.ActivePage Is Nothing Then

                    If Not Survey.SelectedQuestion Is Nothing Then
                        If Survey.ActivePage.GetQuestionByGUID(Survey.SelectedQuestion.AGUID) IsNot Nothing Then 'L0478
                            Survey.SelectedQuestionGUID = Nothing
                            Response.Redirect("SurveyEdit.aspx?id=" + SurveyInfo.ID.ToString + "&qid=-1" + TempThemeURI, True)
                        End If
                    End If
                    If Not Survey.SelectedQuestion Is Nothing And Not IsPostBack And Mode = ctrlSurveyPageMode.cspmEdit Then 'L0022
                        Dim AQuestion As clsQuestion
                        Dim isOther As Boolean = False
                        AQuestion = Survey.SelectedQuestion
                        eQuestionName.Text = AQuestion.Name
                        eQuestionText.Text = AQuestion.Text
                        eQuestionType.SelectedIndex = AQuestion.Type
                        eAnswers.Text = ""
                        For Each Answer As clsVariant In AQuestion.Variants
                            If Answer.Type = VariantType.vtOtherLine Then
                                isOther = True
                            Else
                                eAnswers.Text += Answer.Text + Chr(13) + Chr(10)
                            End If
                        Next
                        eOther.Checked = isOther
                        If AQuestion.Type > QuestionType.qtOpenMemo Then
                            eAnswers.Enabled = True
                            eOther.Enabled = True
                        End If
                        QuestionEditPanel.Visible = True
                    Else
                        If Not IsPostBack Then QuestionEditPanel.Visible = False
                        If SurveyWizard.ActiveViewIndex < 0 Then
                            SurveyWizard.ActiveViewIndex = 0
                            Survey.ActivePageGUID = CType(Survey.Pages(0), clsSurveyPage).AGUID
                        End If

                    End If
                Else
                    If SurveyWizard.ActiveViewIndex < 0 And SurveyWizard.Views.Count > 0 Then   ' D0350
                        SurveyWizard.ActiveViewIndex = 0
                        Survey.ActivePageGUID = CType(Survey.Pages(0), clsSurveyPage).AGUID
                    End If

                End If
            End If
        End Sub

        Protected Sub tbPageEdit_ButtonClick(sender As Object, e As RadToolBarEventArgs) Handles tbPageEdit.ButtonClick  ' D0548
            Select Case CType(e.Item, RadToolBarButton).CommandName  ' D0548
                Case "NewPage"
                    Dim ASurveyPage As New clsSurveyPage(Survey)
                    Survey.Pages.Add(ASurveyPage)

                    For Each key As String In Request.Form.AllKeys
                        If Not key Is Nothing Then
                            If key.Contains("tbNewPageTitle") Then
                                If Not Request.Form(key) Is Nothing Then
                                    ASurveyPage.Title = Request.Form(key).ToString
                                End If
                            End If
                        End If
                    Next

                Case "RenamePage"
                    Dim APage As clsSurveyPage
                    APage = Survey.ActivePage
                    If Not APage Is Nothing Then
                        For Each key As String In Request.Form.AllKeys
                            If Not key Is Nothing Then
                                If key.Contains("tbNewPageTitle") Then
                                    If Not Request.Form(key) Is Nothing Then
                                        APage.Title = Request.Form(key).ToString
                                    End If
                                End If
                            End If
                        Next
                    End If

                Case "RemovePage"
                    If Survey.Pages.Count > 1 Then
                        If Not Survey.ActivePage Is Nothing Then
                            Dim APage As clsSurveyPage
                            APage = Survey.ActivePage
                            Survey.Pages.Remove(APage)
                            SaveChanges()
                            InitControl()
                            Survey.ActivePageGUID = CType(Survey.Pages(0), clsSurveyPage).AGUID
                            Session(SurveyInfo.ID.ToString + "mode" + Mode.ToString) = Survey
                            Response.Redirect("SurveyEdit.aspx?id=" + SurveyInfo.ID.ToString + TempThemeURI, True)
                        End If
                    End If

                Case "PageUp"
                    If Not Survey.ActivePage Is Nothing Then
                        Dim APage As clsSurveyPage
                        APage = Survey.ActivePage
                        Survey.MovePageUp(APage) 'L0026 
                    End If

                Case "PageDown"
                    If Not Survey.ActivePage Is Nothing Then
                        Dim APage As clsSurveyPage
                        APage = Survey.ActivePage
                        Survey.MovePageDown(APage) 'L0026
                    End If
                    'If SurveyWizard.ActiveStepIndex < (SurveyWizard.WizardSteps.Count - 1) Then SurveyWizard.ActiveStepIndex = SurveyWizard.ActiveStepIndex + 1
            End Select
            SaveChanges()
            InitControl()
            Session(SurveyInfo.ID.ToString + "mode" + Mode.ToString) = Survey
        End Sub

        Protected Sub tbQuestionEdit_ButtonClick(sender As Object, e As RadToolBarEventArgs) Handles tbQuestionEdit.ButtonClick
            Dim APage As clsSurveyPage = Nothing
            Dim AQuestion As clsQuestion = Nothing
            Select Case CType(e.Item, RadToolBarButton).CommandName  ' D0548
                Case "NewQuestion"
                    If Not Survey.ActivePage Is Nothing Then
                        APage = Survey.ActivePage
                        If Not APage Is Nothing Then
                            AQuestion = New clsQuestion(APage)
                            AQuestion.Type = QuestionType.qtComment
                            AQuestion.Name = "New Question" + (APage.Questions.Count + 1).ToString
                            AQuestion.Text = "Question Text" + (APage.Questions.Count + 1).ToString
                            APage.Questions.Add(AQuestion)
                            SaveChanges()
                            InitControl()
                            Survey.SelectedQuestionGUID = AQuestion.AGUID 'L0026
                            Session(SurveyInfo.ID.ToString + "mode" + Mode.ToString) = Survey
                        End If
                    End If
                Case "DeleteQuestion"
                    If Not Survey.SelectedQuestion Is Nothing Then
                        AQuestion = Survey.SelectedQuestion
                        APage = Survey.ActivePage
                        APage.Questions.Remove(AQuestion)
                        Survey.SelectedQuestionGUID = Nothing
                        SaveChanges()
                        InitControl()
                        Session(SurveyInfo.ID.ToString + "mode" + Mode.ToString) = Survey
                        Response.Redirect("SurveyEdit.aspx?id=" + SurveyInfo.ID.ToString + TempThemeURI)
                    End If
                Case "QuestionUp"
                    If Not Survey.SelectedQuestion Is Nothing Then
                        AQuestion = Survey.SelectedQuestion
                        Survey.ActivePage.MoveQuestionUp(AQuestion) 'L0478
                        'AQuestion.Page.MoveQuestionUp(AQuestion) 'L0026 '- L0478
                        SaveChanges()
                        InitControl()
                        Session(SurveyInfo.ID.ToString + "mode" + Mode.ToString) = Survey
                    End If
                Case "QuestionDown"
                    If Not Survey.SelectedQuestion Is Nothing Then
                        AQuestion = Survey.SelectedQuestion
                        Survey.ActivePage.MoveQuestionDown(AQuestion) 'L0478
                        'AQuestion.Page.MoveQuestionDown(AQuestion) 'L0026 '- L0478
                        SaveChanges()
                        InitControl()
                        Session(SurveyInfo.ID.ToString + "mode" + Mode.ToString) = Survey
                    End If

            End Select
        End Sub

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            If SurveyInfo Is Nothing Then Exit Sub ' D0380
            If Not Session(SurveyInfo.ID.ToString + "mode" + Mode.ToString) Is Nothing Then
                SurveyInfo.SurveyDataProvider(RespondentEMail).Survey = Session(SurveyInfo.ID.ToString + "mode" + Mode.ToString) 'L0442
            End If
            If Not Request("qid") Is Nothing Then
                Dim QID As String
                QID = Request("qid")
                If QID = "-1" Then
                    Survey.SelectedQuestionGUID = Nothing
                Else
                    Survey.SelectedQuestionGUID = New Guid(QID)
                End If

                If Not Survey.SelectedQuestion Is Nothing Then
                    'L0478 ===
                    Dim NewActivePage As clsSurveyPage = Survey.GetPageByQuestionGUID(Survey.SelectedQuestion.AGUID)
                    If NewActivePage IsNot Nothing Then Survey.ActivePageGUID = NewActivePage.AGUID
                    'L0478 ==
                    'Survey.ActivePageGUID = Survey.SelectedQuestion.Page.AGUID '- L0478
                    Session(SurveyInfo.ID.ToString + "mode" + Mode.ToString) = Survey
                End If
                'Response.Redirect("SurveyEdit.aspx?id=" + Survey.ID.ToString, True)
            End If
            If Mode = ctrlSurveyPageMode.cspmEdit Then
                If eQuestionType.SelectedIndex < 3 Then
                    eAnswers.Enabled = False
                    eOther.Enabled = False
                Else
                    eAnswers.Enabled = True
                    eOther.Enabled = True
                End If
                If Not IsPostBack Then InitControl()
            End If

        End Sub

        Protected Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
            Dim AQuestion As clsQuestion
            AQuestion = Survey.SelectedQuestion
            For Each key As String In Request.Form.AllKeys
                If Not key Is Nothing Then
                    If key.Contains("eQuestionName") Then
                        AQuestion.Name = Request.Form(key).ToString
                    End If
                    If key.Contains("eQuestionText") Then
                        AQuestion.Text = Request.Form(key).ToString
                    End If
                    If key.Contains("eQuestionType") Then
                        AQuestion.Type = CInt(Request.Form(key).ToString)
                    End If
                    If key.Contains("eAnswers") Then
                        If AQuestion.AllowVariants Then 'L0018
                            Dim splitter(2) As Char
                            Dim AVariant As clsVariant
                            splitter.SetValue(Chr(13), 0)
                            splitter.SetValue(Chr(10), 1)

                            AQuestion.Variants.Clear()
                            ' SpyronDataProvider.RemoveAnswersForQuestion(Survey, AQuestion.ID)
                            'Need to clear here all Respondents answers for this Question

                            For Each answer As String In Request.Form(key).ToString.Split(splitter, StringSplitOptions.RemoveEmptyEntries)
                                AVariant = New clsVariant()
                                AVariant.Type = VariantType.vtText
                                AVariant.Text = answer
                                AVariant.ValueType = VariantValueType.vvtString
                                AVariant.VariantValue = answer
                                AQuestion.Variants.Add(AVariant)
                            Next
                            If eOther.Checked Then
                                AVariant = AQuestion.Variants(AQuestion.Variants.Count - 1)
                                '=SL08052701
                                If AVariant.Type <> VariantType.vtOtherLine Then
                                    AVariant = New clsVariant()
                                    AVariant.Type = VariantType.vtOtherLine
                                    AVariant.Text = "Other"
                                    AVariant.ValueType = VariantValueType.vvtString
                                    AVariant.VariantValue = ""
                                    AQuestion.Variants.Add(AVariant)
                                End If
                                '==SL08052701
                            End If
                        End If
                    End If

                End If
            Next
            SaveChanges()
            InitControl()
        End Sub

        Protected Sub btnPager_Click(sender As Object, e As EventArgs)
            If Not Session(SurveyInfo.ID.ToString + "mode" + Mode.ToString) Is Nothing Then
                SurveyInfo.SurveyDataProvider(RespondentEMail).Survey = Session(SurveyInfo.ID.ToString + "mode" + Mode.ToString) 'L0442
            End If
            Dim strPageID As String = CType(sender, Button).ID.Replace("btnPage", "")
            Dim APage As clsSurveyPage = Survey.PageByGUID(New Guid(strPageID))
            Survey.ActivePageGUID = APage.AGUID
            Session(SurveyInfo.ID.ToString + "mode" + Mode.ToString) = Survey
            InitControl()
            'SurveyWizard.ActiveViewIndex = Survey.Pages.IndexOf(APage)
            'Session("CurrentStepID") = SurveyWizard.ActiveViewIndex
        End Sub

        Protected Sub SavePageAnswers()
            Dim SelectedQuestionGUID As Guid
            Dim ActivePageGUID As Guid
            If Not Session(SurveyInfo.ID.ToString + "mode" + Mode.ToString) Is Nothing Then
                SurveyInfo.SurveyDataProvider(RespondentEMail).Survey = Session(SurveyInfo.ID.ToString + "mode" + Mode.ToString) 'L0442
            End If
            If Survey.RespondentByEmail(RespondentEMail) Is Nothing Then
                SelectedQuestionGUID = Survey.SelectedQuestionGUID
                ActivePageGUID = Survey.ActivePageGUID
                Dim ACurRespondent1 As clsRespondent = Survey.RespondentByEmail(RespondentEMail)
                If ACurRespondent1 Is Nothing Then
                    ACurRespondent1 = New clsRespondent()
                    ACurRespondent1.Email = RespondentEMail
                    ACurRespondent1.Name = RespondentName
                End If

                SurveyInfo.SurveyDataProvider(RespondentEMail).SaveStreamRespondentAnswers(ACurRespondent1) 'L0442
                Survey.ActivePageGUID = ActivePageGUID
                Survey.SelectedQuestionGUID = SelectedQuestionGUID
            End If


            Dim ARespondent As clsRespondent = Survey.RespondentByEmail(RespondentEMail)

            Dim QPrefix As String = ""
            'Dim AAnswer As clsAnswer
            ''If ARespondent.Answers.Count = 0 Then
            ''    AAnswer = New clsAnswer()
            ''    AAnswer.Question = Survey.ActivePage.Questions(0)
            ''    ARespondent.Answers.Add(AAnswer)
            ''End If
            'For Each key As String In Request.Form.AllKeys

            'Next
            'For Each APage As clsSurveyPage In Survey.Pages
            For Each AQuestion As clsQuestion In Survey.ActivePage.Questions
                Dim AAnswer As clsAnswer
                AAnswer = ARespondent.AnswerByQuestionGUID(AQuestion.AGUID)

                If AQuestion.Type <> QuestionType.qtComment Then
                    If AAnswer Is Nothing Then
                        AAnswer = New clsAnswer()
                        AAnswer.Question = AQuestion
                    End If

                    'L0018 ===
                    If AQuestion.Type = QuestionType.qtObjectivesSelect Then
                        For Each node As clsNode In ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).Nodes
                            If node.ParentNode IsNot Nothing Then node.DisabledForUser(ProjectManager.User.UserID) = True 'L0040
                        Next
                    End If

                    If AQuestion.Type = QuestionType.qtAlternativesSelect Then
                        For Each node As clsNode In ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).Nodes
                            node.DisabledForUser(ProjectManager.User.UserID) = True
                        Next
                    End If
                    'L0018 ==

                    AAnswer.AnswerVariants.Clear()

                    For Each key As String In Request.Form.AllKeys
                        If Not key Is Nothing Then
                            QPrefix = ""
                            Select Case AQuestion.Type
                                Case QuestionType.qtOpenLine
                                    QPrefix = "tbOpenTextBox" + AQuestion.AGUID.ToString + "s"
                                    If key.Contains(QPrefix) Then
                                        Dim K As String
                                        Dim AVariant As clsVariant
                                        K = Request.Form(key).ToString()
                                        If K <> "" Then
                                            AVariant = New clsVariant()
                                            ' AVariant.ID = 0
                                            AVariant.VariantValue = K
                                            AVariant.Type = VariantType.vtOtherLine
                                            AAnswer.AnswerVariants.Add(AVariant)
                                        End If
                                    End If
                                Case QuestionType.qtNumber, QuestionType.qtNumberColumn
                                    QPrefix = "tbNumberBox" + AQuestion.AGUID.ToString + "s"
                                    If key.Contains(QPrefix) Then
                                        Dim K As String
                                        Dim AVariant As clsVariant
                                        K = Request.Form(key).ToString()
                                        If K <> "" Then
                                            AVariant = New clsVariant()
                                            ' AVariant.ID = 0
                                            AVariant.VariantValue = K
                                            AVariant.Type = VariantType.vtOtherLine
                                            AAnswer.AnswerVariants.Add(AVariant)
                                        End If
                                    End If

                                Case QuestionType.qtOpenMemo
                                    QPrefix = "tbOpenTextBox" + AQuestion.AGUID.ToString + "s"
                                    If key.Contains(QPrefix) Then
                                        Dim K As String
                                        Dim AVariant As clsVariant
                                        K = Request.Form(key).ToString()
                                        If K <> "" Then
                                            AVariant = New clsVariant()
                                            'AVariant.ID = 0
                                            AVariant.VariantValue = K
                                            AVariant.Type = VariantType.vtOtherLine
                                            AAnswer.AnswerVariants.Add(AVariant)
                                        End If
                                    End If

                                Case QuestionType.qtVariantsCheck, QuestionType.qtImageCheck
                                    QPrefix = "cbVariants" + AQuestion.AGUID.ToString + "s"
                                    If key.Contains(QPrefix) Then
                                        For Each AVariant As clsVariant In AQuestion.Variants
                                            If key.Contains(QPrefix + AVariant.AGUID.ToString + "s") Then
                                                If (AVariant.Type <> VariantType.vtOtherLine) And _
                                                   (AVariant.Type <> VariantType.vtOtherMemo) Then
                                                    AAnswer.AnswerVariants.Add(AVariant)
                                                End If
                                            End If
                                        Next
                                    End If
                                    QPrefix = "tbOther" + AQuestion.AGUID.ToString + "s"
                                    If key.Contains(QPrefix) Then
                                        For Each AVariant As clsVariant In AQuestion.Variants
                                            If (AVariant.Type = VariantType.vtOtherLine) Or _
                                               (AVariant.Type = VariantType.vtOtherMemo) Then
                                                If Request.Form(key).ToString() <> "" Then
                                                    AVariant.VariantValue = Request.Form(key).ToString()
                                                    AAnswer.AnswerVariants.Add(AVariant)
                                                End If
                                            End If
                                        Next
                                    End If

                                    'L0018 ===
                                Case QuestionType.qtObjectivesSelect
                                    QPrefix = "cbObjectives" + AQuestion.AGUID.ToString + "s"
                                    If key.Contains(QPrefix) Then
                                        For Each node As clsNode In ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).Nodes
                                            If key.Contains(QPrefix + node.NodeID.ToString + "s") Then
                                                node.DisabledForUser(ProjectManager.User.UserID) = False
                                            End If
                                        Next
                                    End If

                                Case QuestionType.qtAlternativesSelect
                                    QPrefix = "cbAlternatives" + AQuestion.AGUID.ToString + "s"
                                    If key.Contains(QPrefix) Then
                                        For Each node As clsNode In ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).Nodes
                                            If key.Contains(QPrefix + node.NodeID.ToString + "s") Then
                                                node.DisabledForUser(ProjectManager.User.UserID) = False
                                            End If
                                        Next
                                    End If
                                    'L0018 ==

                                Case QuestionType.qtVariantsRadio
                                    QPrefix = "rbVariants" + AQuestion.AGUID.ToString + "s"
                                    If key.Contains(QPrefix) Then
                                        Dim K As String
                                        K = Request.Form(key).ToString()
                                        AAnswer.AnswerVariants.Add(AQuestion.VariantByGUID(New Guid(K)))
                                    End If
                                    QPrefix = "tbOther" + AQuestion.AGUID.ToString + "s"
                                    If key.Contains(QPrefix) Then
                                        For Each AVariant As clsVariant In AQuestion.Variants
                                            If (AVariant.Type = VariantType.vtOtherLine) Or _
                                               (AVariant.Type = VariantType.vtOtherMemo) Then
                                                If Request.Form(key).ToString() <> "" Then
                                                    AVariant.VariantValue = Request.Form(key).ToString()
                                                    AAnswer.AnswerVariants.Add(AVariant)
                                                End If
                                            End If
                                        Next
                                    End If

                                Case QuestionType.qtVariantsCombo
                                    QPrefix = "ddVariants" + AQuestion.AGUID.ToString + "s"
                                    If key.Contains(QPrefix) Then
                                        Dim K As String
                                        K = Request.Form(key).ToString()
                                        AAnswer.AnswerVariants.Add(AQuestion.VariantByGUID(New Guid(K)))
                                    End If
                                    'QPrefix = "tbOther" + AQuestion.ID.ToString + "s"
                                    'If key.Contains(QPrefix) Then
                                    '    For Each AVariant As clsVariant In AQuestion.Variants
                                    '        If (AVariant.Type = VariantType.vtOtherLine) Or _
                                    '           (AVariant.Type = VariantType.vtOtherMemo) Then
                                    '            If Request.Form(key).ToString() <> "" Then
                                    '                AVariant.VariantValue = Request.Form(key).ToString()
                                    '                AAnswer.AnswerVariants.Add(AVariant)
                                    '            End If
                                    '        End If
                                    '    Next
                                    'End If

                            End Select
                            'If key.Contains(QPrefix) Then
                            '    Dim K As String
                            '    K = Request.Form(key).ToString()
                            '    ARespondent.Answers.Add(AAnswer)
                            'End If
                        End If
                    Next

                    If Not ARespondent.Answers.Contains(AAnswer) Then
                        ARespondent.Answers.Add(AAnswer)
                    End If

                End If

                '=s080614 - Place Rated Check
                If (AQuestion.Type = QuestionType.qtImageCheck) And (ProjectManager IsNot Nothing) Then
                    Dim ListOfStates As New ArrayList
                    ListOfStates.Clear()
                    For Each AVariant As clsVariant In AAnswer.AnswerVariants
                        ListOfStates.Add(AVariant.Text)
                    Next
                    'SetEnabledNodesForPlacesRated(ProjectManager, ProjectManager.UserID, ListOfStates)
                End If
                '==s080614 - Place Rated Check

                'L0018 ===
                If (AQuestion.Type = QuestionType.qtAlternativesSelect Or AQuestion.Type = QuestionType.qtObjectivesSelect) And (ProjectManager IsNot Nothing) Then
                    ProjectManager.StorageManager.Writer.SaveUserDisabledNodes(ProjectManager.User)
                    ProjectManager.PipeBuilder.PipeCreated = False 'L0040
                    ProjectManager.PipeBuilder.CreatePipe() 'L0040
                End If
                'L0018 ==


            Next
            'Next

            SelectedQuestionGUID = Survey.SelectedQuestionGUID
            ActivePageGUID = Survey.ActivePageGUID
            Dim ACurRespondent As clsRespondent = Survey.RespondentByEmail(RespondentEMail)
            If ACurRespondent Is Nothing Then
                ACurRespondent = New clsRespondent()
                ACurRespondent.Email = RespondentEMail
                ACurRespondent.Name = RespondentName
            End If
            SurveyInfo.SurveyDataProvider(RespondentEMail).SaveStreamRespondentAnswers(ACurRespondent) 'L0442
            Survey.ActivePageGUID = ActivePageGUID
            Survey.SelectedQuestionGUID = SelectedQuestionGUID
            Session(SurveyInfo.ID.ToString + "mode" + Mode.ToString) = Survey
        End Sub


        Protected Sub btnNextSurveyPage_Click(sender As Object, e As EventArgs) Handles btnNextSurveyPage.Click
            MoveNextPage()
        End Sub


        Protected Sub btnPrevSurveyPage_Click(sender As Object, e As EventArgs) Handles btnPrevSurveyPage.Click
            MovePrevPage()
        End Sub

        Public Function MoveNextPage() As Boolean
            If Mode = ctrlSurveyPageMode.cspmLaunched Then
                If Not ReadOnlyInput Then SavePageAnswers()
            End If
            If SurveyWizard.ActiveViewIndex < (SurveyWizard.Views.Count - 1) Then
                SurveyWizard.ActiveViewIndex += 1
                Return True
            Else
                Session.Remove(SurveyInfo.ID.ToString + "mode" + Mode.ToString)
                Return False
            End If
        End Function

        Public Function MovePrevPage() As Boolean
            If Mode = ctrlSurveyPageMode.cspmLaunched Then
                If Not ReadOnlyInput Then SavePageAnswers()
            End If
            If SurveyWizard.ActiveViewIndex > 0 Then
                SurveyWizard.ActiveViewIndex -= 1
                Return True
            Else
                Session.Remove(SurveyInfo.ID.ToString + "mode" + Mode.ToString)
                Return False
            End If
        End Function

        Protected Sub SurveyWizard_ActiveViewChanged(sender As Object, e As EventArgs) Handles SurveyWizard.ActiveViewChanged
            If SurveyWizard.Views.Count > 0 Then
                Survey.ActivePageGUID = CType(Survey.Pages(SurveyWizard.ActiveViewIndex), clsSurveyPage).AGUID
                'Survey.SelectedQuestionID = -1
                'Session("CurrentStepID") = SurveyWizard.ActiveViewIndex
                'Session.Remove("EditingQuestionID")

                If Mode = ctrlSurveyPageMode.cspmEdit Then
                    Dim APage As clsSurveyPage
                    APage = Survey.ActivePage
                    If Not APage Is Nothing Then
                        If Not tbPageEdit.Items(1).FindControl("tbNewPageTitle") Is Nothing Then
                            CType(tbPageEdit.Items(1).FindControl("tbNewPageTitle"), TextBox).Text = APage.Title
                        End If
                    End If
                End If

                ' Response.Redirect("SurveyEdit.aspx?id=" + Survey.ID.ToString)
            Else
                InitControl()
            End If
            InitControl()

        End Sub

    End Class
End Namespace