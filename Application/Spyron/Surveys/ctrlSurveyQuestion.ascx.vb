Option Strict Off

'L0027
Imports SpyronControls.Spyron.Core
Imports SpyronControls.Spyron.DataSources

Partial Class Spyron_Surveys_ctrlSurveyQuestion
    Inherits System.Web.UI.UserControl

    Private fQuestion As clsQuestion = Nothing
    Private fAnswer As clsAnswer = Nothing
    Private fBackColor As Drawing.Color = Drawing.Color.AliceBlue
    Private fAlternativeQuestionText As String = ""
    Private fHierarchy As clsHierarchy = Nothing
    Private fstrObjectivesHierarchy As String = "" 'L0035
    Private fstrAlternativesHierarchy As String = "" 'L0035
    Private fUserID As Integer = -1
    Private fstrRequiredQuestionAlert As String = "Please answer all the required questions marked with a red asterisk (*)" 'L0048
    Private fstrNumberRequired As String = "Please enter a valid number"
    Private fstrMinMaxQuestionAlert As String = "Please, select necessary count of items." 'L0048
    Private fstrSelectItemsMsg As String = "<div style='font-size:xx-small'><br/>(Select {0})<br/><br/></div>" 'L0056
    Private fstrMinimumItemsMsg As String = "minimum {0} item(s)" 'L0056
    Private fstrMaximumItemsMsg As String = "maximum {0} item(s)" 'L0056
    Private fstrAndMsg As String = " and " 'L0056
    Private fSurveyInfo As clsSurveyInfo = Nothing  ' D1278

    Private fViewMode As Integer = 0 'L0046
    Private fPageIndex As Integer = 0


    Public AltsCheckBoxesClientIDs As String = ""
    Public ObjsCheckBoxesClientIDs As String = ""

    Public sInitField As String = ""    ' D3325 + D3327
    Public sDoBlur As Boolean = False   ' D3325 + D3327


    ''' <summary>
    ''' Represent Question object
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Question() As clsQuestion
        Get
            Return fQuestion
        End Get
        Set(ByVal value As clsQuestion)
            fQuestion = value
            UpdateQuestionForm()
        End Set
    End Property

    ''' <summary>
    ''' Represent Answer object, using for Launch mode to show user's response
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Answer() As clsAnswer
        Get
            Return fAnswer
        End Get
        Set(ByVal value As clsAnswer)
            fAnswer = value
        End Set
    End Property

    ''' <summary>
    ''' Get or set Background color for Question control
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property BackColor() As Drawing.Color
        Get
            Return fBackColor
        End Get
        Set(ByVal value As Drawing.Color)
            fBackColor = value
        End Set
    End Property

    ''' <summary>
    ''' Set or get Alternative Question text, used for edit mode
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property AlternativeQuestionText() As String
        Get
            Return fAlternativeQuestionText
        End Get
        Set(ByVal value As String)
            fAlternativeQuestionText = value
        End Set
    End Property

    'L0035 ===
    Public Property AlternativeObjectivesHierarchyText() As String
        Get
            Return fstrObjectivesHierarchy
        End Get
        Set(ByVal value As String)
            fstrObjectivesHierarchy = value
        End Set
    End Property

    Public Property AlternativeAlternativesHierarchyText() As String
        Get
            Return fstrAlternativesHierarchy
        End Get
        Set(ByVal value As String)
            fstrAlternativesHierarchy = value
        End Set
    End Property
    'L0035 ==

    ''' <summary>
    ''' Set or get Hierarchy for Objectives/Alternatives select question types
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Hierarchy() As clsHierarchy
        Get
            Return fHierarchy
        End Get
        Set(ByVal value As clsHierarchy)
            fHierarchy = value
        End Set
    End Property

    Public Property UserID() As Integer
        Get
            Return fUserID
        End Get
        Set(ByVal value As Integer)
            fUserID = value
        End Set
    End Property

    'L0046 ===
    Public Property ViewMode() As Integer
        Get
            Return fViewMode
        End Get
        Set(ByVal value As Integer)
            fViewMode = value
        End Set
    End Property
    'L0046 ==

    'L0048 ===
    Public Property RequiredQuestionAlert() As String
        Get
            Return fstrRequiredQuestionAlert
        End Get
        Set(ByVal value As String)
            fstrRequiredQuestionAlert = value
        End Set
    End Property

    Public Property PageIndex As Integer
        Get
            Return fPageIndex
        End Get
        Set(value As Integer)
            fPageIndex = value
        End Set
    End Property

    Public Property MinMaxQuestionAlert() As String
        Get
            Return fstrMinMaxQuestionAlert
        End Get
        Set(ByVal value As String)
            fstrMinMaxQuestionAlert = value
        End Set
    End Property
    'L0048 ==

    'L0056 ===
    Public Property SelectItemsMsg() As String
        Get
            Return fstrSelectItemsMsg
        End Get
        Set(ByVal value As String)
            fstrSelectItemsMsg = value
        End Set
    End Property

    Public Property MinimumItemsMsg() As String
        Get
            Return fstrMinimumItemsMsg
        End Get
        Set(ByVal value As String)
            fstrMinimumItemsMsg = value
        End Set
    End Property

    Public Property MaximumItemsMsg() As String
        Get
            Return fstrMaximumItemsMsg
        End Get
        Set(ByVal value As String)
            fstrMaximumItemsMsg = value
        End Set
    End Property

    Public Property AndMsg() As String
        Get
            Return fstrAndMsg
        End Get
        Set(ByVal value As String)
            fstrAndMsg = value
        End Set
    End Property

    ' D1278 ===
    Public Property SurveyInfo() As clsSurveyInfo
        Get
            Return fSurveyInfo
        End Get
        Set(ByVal value As clsSurveyInfo)
            fSurveyInfo = value
        End Set
    End Property
    ' D1278 ==

    Public Function GetMinMaxSelectMsg() As String
        Dim AResult As String = ""
        If Question.MaxSelectedVariants > 0 Or Question.MinSelectedVariants > 0 Then
            Dim msg1 As String = ""
            If Question.MinSelectedVariants > 0 Then
                msg1 += String.Format(MinimumItemsMsg, Question.MinSelectedVariants.ToString)
                If Question.MaxSelectedVariants > 0 Then msg1 += AndMsg
            End If
            If Question.MaxSelectedVariants > 0 Then
                msg1 += String.Format(MaximumItemsMsg, Question.MaxSelectedVariants.ToString)
            End If
            AResult += String.Format(SelectItemsMsg, msg1)
        End If
        Return AResult
    End Function
    'L0056 ==

    Public Sub UpdateQuestionForm()
        Dim lblQuestionText As New Label
        lblQuestionText.CssClass = "questionTextClass"
        If lblQuestionText IsNot Nothing And Visible Then   ' D1278
            tcQuestionForm.Controls.Clear()
            tcQuestionForm.Controls.Add(lblQuestionText)

            Dim fMHT As Boolean = isMHT(Question.Text)  ' D2079

            sInitField = ""     ' D3325
            sDoBlur = False     ' D3325

            If AlternativeQuestionText = "" Then
                ' D2079 ===
                If Not fMHT Then
                    lblQuestionText.Text = SafeFormString(Question.Text).Replace(vbCrLf, "<br/>").Replace(vbCr, "<br/>").Replace(vbLf, "<br/>") ' D4427
                    If clsComparionCorePage.OPT_SURVEY_PARSE_LINKS Then lblQuestionText.Text = ParseTextHyperlinks(lblQuestionText.Text) ' D1039 + D4102
                End If
                ' D2079 ==
                If Question.Text.Trim() = "" And ViewMode = 1 Then erEmptyFields.Visible = True 'L0341
            Else
                lblQuestionText.Text = AlternativeQuestionText
            End If

            ' D1278 ===
            If fMHT And SurveyInfo IsNot Nothing Then   ' D2079
                lblQuestionText.Text = CutHTMLHeaders(Infodoc_Unpack(SurveyInfo.ProjectID, 0, reObjectType.SurveyQuestion, Question.AGUID.ToString, Question.Text, True, True, -1, True)).Trim  ' D2079 + D4429 + D6727
                If lblQuestionText.Text <> "" AndAlso Not isHTMLEmpty(lblQuestionText.Text) Then lblQuestionText.Text = String.Format("<div class='survey_question'>{0}</div>", lblQuestionText.Text)    ' D7038
            End If
            ' D1278 ==

            'ATableCell.Controls.Add(ALabel)
            'Dim ARespondent As clsRespondent = Survey.RespondentByEmail(RespondentEMail)

            'for each type of questions creating necessary controls
            Select Case Question.Type
                Case QuestionType.qtComment
                    'ALabel.CssClass = "projectcomment"
                Case QuestionType.qtOpenLine
                    lblQuestionText.Text += "&nbsp;"
                    Dim ATextBox As New TextBox
                    ATextBox.ID = "tbOpenTextBox" + Question.AGUID.ToString + "s"
                    ATextBox.TextMode = TextBoxMode.SingleLine

                    'Fill textbox with Respondent answer if it is necessary
                    If Answer IsNot Nothing Then
                        If Answer.AnswerVariants.Count > 0 Then
                            ATextBox.Text = CType(Answer.AnswerVariants(0), clsVariant).VariantValue
                        End If
                    End If
                    tcQuestionForm.Controls.Add(ATextBox)
                    If sInitField = "" Then sInitField = ATextBox.ClientID ' D3325

                    'L0046 ===
                    If Not Question.AllowSkip And ViewMode = 0 Then
                        Page.ClientScript.RegisterOnSubmitStatement(GetType(String), "CheckForm" + Question.AGUID.ToString, "; var o = $get(""" + ATextBox.ClientID + """); if (o.value == '') {dxDialog('" + RequiredQuestionAlert + "(#" + PageIndex.ToString + ")" + "', true, null); return false;};") 'L0337 'L0339
                    End If
                    'L0046 ==

                Case QuestionType.qtNumber
                    lblQuestionText.Text += "<div style='font-size:xx-small'><br/>(Enter a valid number)<br/><br/></div>"
                    Dim ATextBox As New TextBox
                    ATextBox.ID = "tbNumberBox" + Question.AGUID.ToString + "s"
                    ATextBox.TextMode = TextBoxMode.SingleLine

                    'Fill textbox with Respondent answer if it is necessary
                    If Answer IsNot Nothing Then
                        If Answer.AnswerVariants.Count > 0 Then
                            ATextBox.Text = CType(Answer.AnswerVariants(0), clsVariant).VariantValue
                        End If
                    End If
                    tcQuestionForm.Controls.Add(ATextBox)
                    If sInitField = "" Then sInitField = ATextBox.ClientID ' D3325

                    'L0046 ===
                    If Not Question.AllowSkip And ViewMode = 0 Then
                        Page.ClientScript.RegisterOnSubmitStatement(GetType(String), "CheckForm" + Question.AGUID.ToString, "; var o = $get(""" + ATextBox.ClientID + """); if (o.value == '') {dxDialog('" + RequiredQuestionAlert + " (#" + PageIndex.ToString + ")" + "', true, null); return false;};") 'L0337 'L0339
                    End If
                    'L0046 ===
                    If ViewMode = 0 Then
                        Dim skipCondition As String = ""
                        If Question.AllowSkip Then
                            skipCondition = "&& (o.value !== '')"
                        End If
                        Page.ClientScript.RegisterOnSubmitStatement(GetType(String), "CheckFormNumber" + Question.AGUID.ToString, "; var o = $get(""" + ATextBox.ClientID + """); if (((isNaN(parseFloat(o.value))) || (!isFinite(o.value)))" + skipCondition + ") {dxDialog('" + fstrNumberRequired + " (#" + PageIndex.ToString + ")" + "', true, null); o.focus(); o.select(); return false;};")
                    End If

                Case QuestionType.qtNumberColumn
                    lblQuestionText.Text += "<div style='font-size:xx-small'><br/>(Enter multiple numbers separated by a new line)<br/><br/></div>"
                    Dim ATextBox As New TextBox
                    ATextBox.ID = "tbNumberBox" + Question.AGUID.ToString + "s"
                    ATextBox.TextMode = TextBoxMode.MultiLine 
                    ATextBox.Rows = 6

                    'Fill textbox with Respondent answer if it is necessary
                    If Answer IsNot Nothing Then
                        If Answer.AnswerVariants.Count > 0 Then
                            ATextBox.Text = CType(Answer.AnswerVariants(0), clsVariant).VariantValue
                        End If
                    End If
                    tcQuestionForm.Controls.Add(ATextBox)
                    If sInitField = "" Then sInitField = ATextBox.ClientID ' D3325

                    'L0046 ===
                    If Not Question.AllowSkip And ViewMode = 0 Then
                        Page.ClientScript.RegisterOnSubmitStatement(GetType(String), "CheckForm" + Question.AGUID.ToString, "; var o = $get(""" + ATextBox.ClientID + """); if (o.value == '') {dxDialog('" + RequiredQuestionAlert + " (#" + PageIndex.ToString + ")" + "', true, null); return false;};") 'L0337 'L0339
                    End If
                    'L0046 ===
                    If ViewMode = 0 Then
                        Dim skipCondition As String = ""
                        If Question.AllowSkip Then
                            skipCondition = "&& (o.value !== '')"
                        End If
                        Page.ClientScript.RegisterOnSubmitStatement(GetType(String), "CheckFormNumber" + Question.AGUID.ToString, "; var o = $get(""" + ATextBox.ClientID + """); var txt = $.trim(o.value); arro = txt.split('\n'); var isallnumbers = true; for (var i = 0; i < arro.length; i++) { if ((isNaN(parseFloat(arro[i]))) || (!isFinite(arro[i]))) {isallnumbers = false};}; if (!isallnumbers" + skipCondition + ") {dxDialog('" + fstrNumberRequired + " (#" + PageIndex.ToString + ")" + "', true, null); o.focus(); o.select(); return false };")
                    End If

                Case QuestionType.qtOpenMemo
                    lblQuestionText.Text += "<br />"
                    Dim ATextBox As New TextBox
                    ATextBox.ID = "tbOpenTextBox" + Question.AGUID.ToString + "s"
                    ATextBox.TextMode = TextBoxMode.MultiLine
                    ATextBox.Width = Unit.Pixel(350) 'Option 01
                    ATextBox.Rows = 3 'Option 02

                    'Fill textbox with Respondent answer if it is necessary
                    If Answer IsNot Nothing Then
                        If Answer.AnswerVariants.Count > 0 Then
                            ATextBox.Text = CType(Answer.AnswerVariants(0), clsVariant).VariantValue
                        End If
                    End If
                    tcQuestionForm.Controls.Add(ATextBox)
                    If sInitField = "" Then sInitField = ATextBox.ClientID ' D3325

                    'L0046 ===
                    If Not Question.AllowSkip And ViewMode = 0 Then
                        Page.ClientScript.RegisterOnSubmitStatement(GetType(String), "CheckForm" + Question.AGUID.ToString, "; var o = $get(""" + ATextBox.ClientID + """); if (o.value == '') {alert('" + RequiredQuestionAlert + "'); o.focus(); return false;};") 'L0337 'L0339
                    End If
                    'L0046 ==

                Case QuestionType.qtVariantsCheck, QuestionType.qtImageCheck
                    Dim isOther As Boolean = False
                    If Question.Type <> QuestionType.qtImageCheck Then
                        lblQuestionText.Text += "<br />"
                        lblQuestionText.Text += GetMinMaxSelectMsg() 'L0056
                        ''L0048 ===
                        'If Question.MaxSelectedVariants > 0 Or Question.MinSelectedVariants > 0 Then
                        '    lblQuestionText.Text += "<div style='font-size:xx-small'><br/>(Select "
                        '    If Question.MinSelectedVariants > 0 Then
                        '        lblQuestionText.Text += "minimum " + Question.MinSelectedVariants.ToString + " item(s)"
                        '        If Question.MaxSelectedVariants > 0 Then lblQuestionText.Text += " and "
                        '    End If
                        '    If Question.MaxSelectedVariants > 0 Then
                        '        lblQuestionText.Text += "maximum " + Question.MaxSelectedVariants.ToString + " item(s)"
                        '    End If
                        '    lblQuestionText.Text += ")<br/><br/></div>"
                        'End If
                        ''L0048 ==
                    End If

                    Dim ACheckBox As CheckBox

                    Dim AAnswerGrid As New Table
                    Dim AAnswerRow As New TableRow
                    Dim AAnswerCell As New TableCell
                    Dim AAnswersPerCellCount As Integer = 0

                    'If Question.Variants.Count > 10 Then 'Option 03
                    '    tcQuestionForm.Controls.Add(AAnswerGrid)
                    '    AAnswerGrid.Rows.Add(AAnswerRow)
                    'End If

                    Dim CheckBoxesClientIDs As String = "" 'L0046
                    For Each AVariant As clsVariant In Question.Variants
                        If AVariant.Type = VariantType.vtOtherLine Or AVariant.Type = VariantType.vtOtherMemo Then
                            isOther = True
                        Else
                            ACheckBox = New CheckBox
                            ACheckBox.ID = "cbVariants" + Question.AGUID.ToString + "s" + AVariant.AGUID.ToString + "s"
                            ACheckBox.Text = AVariant.Text.Trim() + "<br />"
                            ACheckBox.InputAttributes.Add("value", AVariant.Text)

                            If AVariant.Text.Trim() = "" And ViewMode = 1 Then erEmptyFields.Visible = True 'L0341

                            If Question.Type = QuestionType.qtImageCheck Then
                                ACheckBox.InputAttributes.Add("style", "display:none;")
                                ACheckBox.Attributes.Add("style", "display:none;")
                            End If

                            'set checked property for each variant as Respondent answered if necessary
                            If Answer IsNot Nothing Then
                                If Answer.AnswerGUIDsString.Contains(AVariant.AGUID.ToString) Then 'L0043
                                    ACheckBox.Checked = True
                                End If
                            End If

                            'If Question.Variants.Count > 10 Then 'Option 03
                            '    AAnswerCell.Controls.Add(ACheckBox)
                            '    AAnswersPerCellCount += 1
                            '    If AAnswersPerCellCount > 3 Then 'Option 04
                            '        AAnswerRow.Cells.Add(AAnswerCell)
                            '        AAnswersPerCellCount = 0
                            '        AAnswerCell = New TableCell()
                            '    End If
                            'Else
                            tcQuestionForm.Controls.Add(ACheckBox)
                            'End If
                            'L0046 ===
                            CheckBoxesClientIDs += CStr(IIf(CheckBoxesClientIDs = "", "", ",")) + String.Format("'{0}'", ACheckBox.ClientID) ' D1132
                            'If Question.Variants.IndexOf(AVariant) = Question.Variants.Count - 1 Then
                            '    CheckBoxesClientIDs += "'" + ACheckBox.ClientID + "'"
                            'Else
                            '    CheckBoxesClientIDs += "'" + ACheckBox.ClientID + "',"
                            'End If

                            'var a = ['control1','control2'];
                            'L0046 ==
                        End If
                    Next

                    If Question.Variants.Count > 10 Then 'Option 03
                        AAnswerRow.Cells.Add(AAnswerCell)
                    End If
                    If Question.Type = QuestionType.qtImageCheck And AlternativeQuestionText <> "" Then 'L0011 Not IsPostBack And 'L0012 + 'L0020
                        ScriptManager.RegisterStartupScript(Me, GetType(String), "InitCB", "setTimeout('if ((isScript)) {Init()};', 1000);", True) ''statefs('')
                    End If

                    Dim sOtherCondition As String = "false"
                    'add other textbox input if necessary
                    If isOther Then
                        Dim AOtherLabel As New Label
                        Dim ATextBox As New TextBox

                        'L0050 ===
                        For Each AVariant As clsVariant In Question.Variants
                            If (AVariant.Type = VariantType.vtOtherLine) Or _
                                   (AVariant.Type = VariantType.vtOtherMemo) Then
                                AOtherLabel.Text = AVariant.Text
                            End If
                        Next
                        'L0050 ==

                        'AOtherLabel.Text = ... 'L0050
                        tcQuestionForm.Controls.Add(AOtherLabel)
                        ATextBox.ID = "tbOther" + Question.AGUID.ToString + "s"
                        ATextBox.TextMode = TextBoxMode.SingleLine

                        ' set Respondent answer to Other field
                        If Answer IsNot Nothing Then
                            For Each AVariant As clsVariant In Answer.AnswerVariants
                                If (AVariant.Type = VariantType.vtOtherLine) Or _
                                   (AVariant.Type = VariantType.vtOtherMemo) Then
                                    ATextBox.Text = AVariant.VariantValue
                                End If
                            Next
                        End If

                        tcQuestionForm.Controls.Add(ATextBox)
                        If sInitField = "" Then sInitField = ATextBox.ClientID ' D3325
                        sOtherCondition = "$get(""" + ATextBox.ClientID + """).value == ''"
                    End If

                    'L0046 ===
                    If Not Question.AllowSkip And ViewMode = 0 Then
                        Page.ClientScript.RegisterOnSubmitStatement(GetType(String), "CheckForm" + Question.AGUID.ToString, "; var ccount = 0; var a = [" + CheckBoxesClientIDs + "]; for (var j=0 ; j < a.length; j++){var o = $get(a[j]); if (o.checked) {ccount++;};}; if ((ccount == 0)&&(" + sOtherCondition + ")) {alert('" + RequiredQuestionAlert + "'); $get(a[0]).focus(); return false;};") 'L0048 'L0337 'L0339
                    End If
                    'L0046 ==

                    'L0048 ===
                    If ViewMode = 0 And (Question.MinSelectedVariants > 0 Or Question.MaxSelectedVariants > 0) Then
                        Dim QMax As Integer = Question.MaxSelectedVariants
                        Dim QMin As Integer = Question.MinSelectedVariants
                        If Question.MaxSelectedVariants = 0 Then QMax = Integer.MaxValue
                        Page.ClientScript.RegisterOnSubmitStatement(GetType(String), "CheckFormMinMax" + Question.AGUID.ToString, "; var ccount = 0; var a = [" + CheckBoxesClientIDs + "]; for (var j=0 ; j < a.length; j++){var o = $get(a[j]); if (o.checked) {ccount++;};}; if (" + sOtherCondition + ") {ccount++;}; if ((ccount < " + QMin.ToString + ") || (ccount > " + QMax.ToString + ")) {alert('" + MinMaxQuestionAlert + "'); return false;};")
                    End If
                    'L0048 ==

                Case QuestionType.qtVariantsRadio
                    Dim isOther As Boolean = False
                    'lblQuestionText.Text += "<br />"
                    Dim ARadioList As New RadioButtonList
                    ARadioList.CssClass = "text"
                    ARadioList.ID = "rbVariants" + Question.AGUID.ToString + "s"
                    For Each AVariant As clsVariant In Question.Variants
                        If AVariant.Type = VariantType.vtOtherLine Or AVariant.Type = VariantType.vtOtherMemo Then
                            isOther = True
                        Else
                            Dim LI As New ListItem
                            LI.Text = AVariant.Text.Trim() + "<br/>"
                            LI.Value = AVariant.AGUID.ToString
                            If AVariant.Text.Trim() = "" And ViewMode = 1 Then erEmptyFields.Visible = True 'L0341
                            'set Selected property for each variant as Respondent answered if necessary
                            If Answer IsNot Nothing Then
                                If Answer.AnswerGUIDsString.Contains(AVariant.AGUID.ToString) Then 'L0043
                                    LI.Selected = True
                                End If
                            End If
                            ARadioList.Items.Add(LI)
                        End If
                    Next

                    tcQuestionForm.Controls.Add(ARadioList)


                    'L0046 ===
                    If Not Question.AllowSkip And ViewMode = 0 Then
                        'L0337 ===
                        Dim RadioItemsClientIDs As String = ""
                        Dim vID As Integer = 0
                        For Each AVariant In Question.Variants
                            If Question.Variants.IndexOf(AVariant) = Question.Variants.Count - 1 Then
                                RadioItemsClientIDs += "'" + ARadioList.ClientID + "_" + vID.ToString + "'"
                            Else
                                RadioItemsClientIDs += "'" + ARadioList.ClientID + "_" + vID.ToString + "',"
                            End If
                            vID += 1
                        Next

                        Page.ClientScript.RegisterOnSubmitStatement(GetType(String), "CheckForm" + Question.AGUID.ToString, "; var ccount = 0; var a = [" + RadioItemsClientIDs + "]; for (var j=0 ; j < a.length; j++){var o = $get(a[j]); if (o.checked) {ccount++;};}; if (ccount == 0) {alert('" + RequiredQuestionAlert + "'); $get(a[0]).focus(); return false;};") 'L0048 'L0339
                        'L0337 ==
                    End If
                    'L0046 ==

                    'add other textbox input if necessary
                    If isOther Then
                        'L0049 ===
                        Dim variantOther As clsVariant = Nothing
                        For Each AVariant As clsVariant In Question.Variants
                            If (AVariant.Type = VariantType.vtOtherLine) Or _
                               (AVariant.Type = VariantType.vtOtherMemo) Then
                                variantOther = AVariant
                            End If
                        Next
                        'Dim AOtherLabel As New Label 'L0049
                        Dim LI As New ListItem
                        LI.Text = variantOther.Text 'L0050
                        LI.Value = variantOther.AGUID.ToString

                        Dim ATextBox As New TextBox
                        'AOtherLabel.Text = "Other:   " 'Option 05 'L0049
                        'tcQuestionForm.Controls.Add(AOtherLabel) 'L0049
                        'L0049 ==
                        ATextBox.ID = "tbOther" + Question.AGUID.ToString + "s"
                        ATextBox.TextMode = TextBoxMode.SingleLine
                        ' set Respondent answer to Other field

                        If Answer IsNot Nothing Then
                            'L0049 ===
                            If Answer.AnswerGUIDsString.Contains(variantOther.AGUID.ToString) Then
                                LI.Selected = True
                            End If
                            'L0049 ==
                            For Each AVariant As clsVariant In Answer.AnswerVariants
                                If (AVariant.Type = VariantType.vtOtherLine) Or _
                                   (AVariant.Type = VariantType.vtOtherMemo) Then
                                    If AVariant.VariantValue <> "" Then ATextBox.Text = AVariant.VariantValue
                                End If
                            Next
                        End If
                        ARadioList.Items.Add(LI) 'L0049
                        tcQuestionForm.Controls.Add(ATextBox)
                        ARadioList.Attributes.Add("onclick", "if (!$get(""" + ARadioList.ClientID + "_" + (ARadioList.Items.Count - 1).ToString + """).checked){$get(""" + ATextBox.ClientID + """).value = ''};") 'L0049
                        ATextBox.Attributes.Add("onfocus", "if (!(s_manual)) $get(""" + ARadioList.ClientID + "_" + (ARadioList.Items.Count - 1).ToString + """).checked = true;") 'L0049 + D3325
                        If sInitField = "" Then sInitField = ATextBox.ClientID ' D3325
                        sDoBlur = True  ' D3325
                        'Page.ClientScript.RegisterStartupScript(GetType(String), "OtherLI" + variantOther.AGUID.ToString, "; if ($get(""" + ARadioList.ClientID + """).value != '" + variantOther.AGUID.ToString + "'){$get(""" + ATextBox.ClientID + """).value = '11'};") 'L0049
                    End If

                    'L0062 ===
                    Dim AClearButton As New Button
                    AClearButton.ID = "cbtn" + Question.AGUID.ToString + "s"
                    AClearButton.Text = "Clear"
                    AClearButton.CssClass = "button"
                    AClearButton.Attributes.Add("style", "margin-left:1ex;")
                    'AClearButton.Attributes.Add("onClick", "for (i = 0; i < $get('" + ARadioList.ClientID + "').lenght; i++){alert($get('" + ARadioList.ClientID + "')[i].checked)};")
                    Dim AScript As String = ""
                    For i As Integer = 0 To ARadioList.Items.Count - 1
                        AScript += "$get('" + ARadioList.ClientID + "_" + i.ToString + "').checked = false;"
                    Next
                    'AClearButton.Attributes.Add("onClick", "alert($get('" + ARadioList.ClientID + "').id);")
                    AClearButton.Attributes.Add("onClick", AScript + "return false;")
                    tcQuestionForm.Controls.Add(AClearButton)
                    'L0062 ==

                Case QuestionType.qtVariantsCombo
                    Dim isOther As Boolean = False
                    lblQuestionText.Text += "&nbsp;"
                    Dim ADropDownList As New DropDownList
                    ADropDownList.ID = "ddVariants" + Question.AGUID.ToString + "s"
                    Dim LI As ListItem
                    LI = New ListItem
                    LI.Text = "-Select One-"
                    LI.Value = ""
                    ADropDownList.Items.Add(LI)
                    For Each AVariant As clsVariant In Question.Variants
                        If AVariant.Type = VariantType.vtOtherLine Or AVariant.Type = VariantType.vtOtherMemo Then
                            isOther = True
                        Else
                            LI = New ListItem
                            LI.Text = AVariant.Text
                            LI.Value = AVariant.AGUID.ToString
                            If AVariant.Text.Trim() = "" And ViewMode = 1 Then erEmptyFields.Visible = True 'L0341
                            'set Selected variant as Respondent answered if necessary
                            If Answer IsNot Nothing Then
                                If Answer.AnswerGUIDsString.Contains(AVariant.AGUID.ToString) Then 'L0043
                                    LI.Selected = True
                                End If
                            End If
                            ADropDownList.Items.Add(LI)
                        End If
                    Next
                    tcQuestionForm.Controls.Add(ADropDownList)
                    'If Answer Is Nothing Then ADropDownList.SelectedIndex = -1'L0339
                    'L0062 ===
                    Dim AClearButton As New Button
                    AClearButton.ID = "cbtn" + Question.AGUID.ToString + "s"
                    AClearButton.Text = "Clear"
                    AClearButton.CssClass = "button"
                    AClearButton.Attributes.Add("style", "margin-left:1ex")
                    tcQuestionForm.Controls.Add(AClearButton)
                    'L0062 ==

                    Dim sOtherCondition As String = "false"

                    Dim ATextBox As New TextBox
                    'add other textbox input if necessary
                    If isOther Then
                        Dim AOtherLabel As New Label

                        For Each AVariant As clsVariant In Question.Variants
                            If (AVariant.Type = VariantType.vtOtherLine) Or _
                                   (AVariant.Type = VariantType.vtOtherMemo) Then
                                AOtherLabel.Text = "<br/>" + AVariant.Text
                            End If
                        Next
                        tcQuestionForm.Controls.Add(AOtherLabel)
                        ATextBox.ID = "tbOther" + Question.AGUID.ToString + "s"
                        ATextBox.TextMode = TextBoxMode.SingleLine

                        ' set Respondent answer to Other field
                        If Answer IsNot Nothing Then
                            For Each AVariant As clsVariant In Answer.AnswerVariants
                                If (AVariant.Type = VariantType.vtOtherLine) Or _
                                   (AVariant.Type = VariantType.vtOtherMemo) Then
                                    ATextBox.Text = AVariant.VariantValue
                                End If
                            Next
                        End If

                        tcQuestionForm.Controls.Add(ATextBox)
                        sOtherCondition = "$get('" + ATextBox.ClientID + "').value == ''"
                        ATextBox.Attributes.Add("onfocus", "if (!(s_manual)) $get(""" + ADropDownList.ClientID + """).value = '';")   ' D3325
                        If sInitField = "" Then sInitField = ATextBox.ClientID ' D3325
                        sDoBlur = True ' D3325
                        ADropDownList.Attributes.Add("onfocus", "$get('" + ATextBox.ClientID + "').value = '';")
                        AClearButton.Attributes.Add("onClick", "$get('" + ADropDownList.ClientID + "').value = ''; $get('" + ATextBox.ClientID + "').value = ''; return false;")
                    Else
                        AClearButton.Attributes.Add("onClick", "$get('" + ADropDownList.ClientID + "').value = ''; return false;")
                    End If

                    If Not Question.AllowSkip And ViewMode = 0 Then
                        If isOther Then
                            Page.ClientScript.RegisterOnSubmitStatement(GetType(String), "CheckForm" + Question.AGUID.ToString, "; var o = $get(""" + ADropDownList.ClientID + """); var tb = $get(""" + ATextBox.ClientID + """); if (o.value == '' && tb.value == '') {alert('" + RequiredQuestionAlert + "'); return false;};") 'L0048 'L0337 'L0339
                        Else
                            Page.ClientScript.RegisterOnSubmitStatement(GetType(String), "CheckForm" + Question.AGUID.ToString, "; var o = $get(""" + ADropDownList.ClientID + """); if (o.value == '') {alert('" + RequiredQuestionAlert + "'); return false;};") 'L0048 'L0337 'L0339
                        End If
                    End If

                Case QuestionType.qtObjectivesSelect
                    lblQuestionText.Text += "<br/>"
                    lblQuestionText.Text += GetMinMaxSelectMsg()

                    If Hierarchy IsNot Nothing Then
                        Dim CheckBoxesClientIDs As String = "" 'L0048
                        Dim ACheckBox As CheckBox
                        Dim AllAllowed As Boolean = True
                        For Each node As clsNode In Hierarchy.Nodes
                            If node.DisabledForUser(UserID) Then
                                AllAllowed = False
                                Exit For
                            End If
                        Next

                        ACheckBox = New CheckBox()
                        ACheckBox.ID = "cbAllObjectives"
                        ACheckBox.Text = "<b style='color:blue;'>All</b>" + "<br/>"
                        ACheckBox.InputAttributes.Add("onclick", "SetSelection(this, 'obj')")
                        If AllAllowed Then ACheckBox.Checked = True
                        tcQuestionForm.Controls.Add(ACheckBox)

                        For Each node As clsNode In Hierarchy.Nodes
                            ACheckBox = New CheckBox
                            ACheckBox.ID = "cbObjectives" + Question.AGUID.ToString + "s" + node.NodeID.ToString + "s"
                            ACheckBox.Style.Add(HtmlTextWriterStyle.MarginLeft, (node.Level * 10).ToString + "px")
                            ACheckBox.Text = node.NodeName + "<br />" 'L0043
                            ACheckBox.InputAttributes.Add("value", node.NodeID.ToString)
                            ACheckBox.InputAttributes.Add("class", "obj")
                            ACheckBox.Checked = Not node.DisabledForUser(UserID)

                            If node.ParentNode Is Nothing Then ACheckBox.Enabled = False 'L0043
                            tcQuestionForm.Controls.Add(ACheckBox)

                            'L0048 ===
                            If Hierarchy.Nodes.IndexOf(node) = Hierarchy.Nodes.Count - 1 Then
                                CheckBoxesClientIDs += "'" + ACheckBox.ClientID + "'"
                            Else
                                CheckBoxesClientIDs += "'" + ACheckBox.ClientID + "',"
                            End If
                            'L0048 ==
                        Next
                        ObjsCheckBoxesClientIDs = CheckBoxesClientIDs
                        'L0048 ===
                        If ViewMode = 0 And (Question.MinSelectedVariants > 0 Or Question.MaxSelectedVariants > 0) Then
                            Dim QMax As Integer = Question.MaxSelectedVariants
                            Dim QMin As Integer = Question.MinSelectedVariants
                            If Question.MaxSelectedVariants = 0 Then QMax = Integer.MaxValue
                            Page.ClientScript.RegisterOnSubmitStatement(GetType(String), "CheckFormMinMax" + Question.AGUID.ToString, "; var ccount = 0; var a = [" + CheckBoxesClientIDs + "]; for (var j=0 ; j < a.length; j++){var o = $get(a[j]); if (o.checked) {ccount++;};}; if ((ccount < " + QMin.ToString + ") || (ccount > " + QMax.ToString + ")) {alert('" + MinMaxQuestionAlert + "'); return false;};")

                            '</script>
                        End If
                        'L0048 ==
                    Else
                        lblQuestionText.Text += "<br/> " + AlternativeObjectivesHierarchyText
                    End If

                Case QuestionType.qtAlternativesSelect
                    lblQuestionText.Text += "<br/>"
                    lblQuestionText.Text += GetMinMaxSelectMsg() 'L0056
                    'L0048 ===

                    'L0048 ==
                    If Hierarchy IsNot Nothing Then
                        Dim CheckBoxesClientIDs As String = "" 'L0048
                        Dim ACheckBox As CheckBox
                        Dim AllAllowed As Boolean = True
                        For Each node As clsNode In Hierarchy.Nodes
                            If node.DisabledForUser(UserID) Then
                                AllAllowed = False
                                Exit For
                            End If
                        Next

                        ACheckBox = New CheckBox()
                        ACheckBox.ID = "cbAllAlternatives"
                        ACheckBox.Text = "<b style='color:blue'>All</b>" + "<br/>"
                        ACheckBox.InputAttributes.Add("onclick", "SetSelection(this, 'alt')")
                        ACheckBox.InputAttributes.Add("class", "altsselectall")

                        If Answer Is Nothing Then
                            ACheckBox.Checked = Question.SelectAllByDefault
                        Else
                            ACheckBox.Checked = AllAllowed
                        End If
                        tcQuestionForm.Controls.Add(ACheckBox)

                        For Each node As clsNode In Hierarchy.Nodes
                            ACheckBox = New CheckBox
                            ACheckBox.ID = "cbAlternatives" + Question.AGUID.ToString + "s" + node.NodeID.ToString + "s"
                            Dim strLevel As String = ""
                            ACheckBox.Text = node.NodeName + "<br />"
                            ACheckBox.InputAttributes.Add("value", node.NodeID.ToString)

                            If Answer Is Nothing Then
                                ACheckBox.Checked = Question.SelectAllByDefault
                            Else
                                ACheckBox.Checked = Not node.DisabledForUser(Hierarchy.ProjectManager.User.UserID)
                            End If

                            ACheckBox.Style.Add(HtmlTextWriterStyle.MarginLeft, "10px")
                            ACheckBox.InputAttributes.Add("class", "alt")
                            tcQuestionForm.Controls.Add(ACheckBox)
                            'L0048 ===
                            If Hierarchy.Nodes.IndexOf(node) = Hierarchy.Nodes.Count - 1 Then
                                CheckBoxesClientIDs += "'" + ACheckBox.ClientID + "'"
                            Else
                                CheckBoxesClientIDs += "'" + ACheckBox.ClientID + "',"
                            End If
                            'L0048 ==
                        Next

                        AltsCheckBoxesClientIDs = CheckBoxesClientIDs
                        'L0048 ===
                        If ViewMode = 0 And (Question.MinSelectedVariants > 0 Or Question.MaxSelectedVariants > 0) Then
                            Dim QMax As Integer = Question.MaxSelectedVariants
                            Dim QMin As Integer = Question.MinSelectedVariants
                            If Question.MaxSelectedVariants = 0 Then QMax = Integer.MaxValue
                            Page.ClientScript.RegisterOnSubmitStatement(GetType(String), "CheckFormMinMax" + Question.AGUID.ToString, "; var ccount = 0; var a = [" + CheckBoxesClientIDs + "]; for (var j=0 ; j < a.length; j++){var o = $get(a[j]); if (o.checked) {ccount++;};}; if ((ccount < " + QMin.ToString + ") || (ccount > " + QMax.ToString + ")) {alert('" + MinMaxQuestionAlert + "'); return false;};")
                        End If
                        'L0048 ==
                    Else
                        lblQuestionText.Text += "<br/> " + AlternativeAlternativesHierarchyText
                    End If
            End Select
        End If
    End Sub

    'L0040 ===
    Public Sub ReadAnswer()
        If Question.Type <> QuestionType.qtComment Then
            If Answer Is Nothing Then
                Answer = New clsAnswer()
                Answer.Question = Question
            End If
            Answer.AnswerDate = Now() 'L0355
            If Question.Type = QuestionType.qtObjectivesSelect Then
                For Each node As clsNode In Hierarchy.Nodes
                    If node.ParentNode IsNot Nothing Then node.DisabledForUser(Hierarchy.ProjectManager.User.UserID) = True 'L0043
                Next
            End If

            If Question.Type = QuestionType.qtAlternativesSelect Then
                For Each node As clsNode In Hierarchy.Nodes
                    node.DisabledForUser(Hierarchy.ProjectManager.User.UserID) = True
                Next
            End If

            Answer.AnswerVariants.Clear()

            For Each key As String In Request.Form.AllKeys
                If Not key Is Nothing Then
                    Dim QPrefix As String = ""
                    Select Case Question.Type
                        Case QuestionType.qtOpenLine, QuestionType.qtOpenMemo
                            QPrefix = "tbOpenTextBox" + Question.AGUID.ToString + "s"
                            If key.Contains(QPrefix) Then
                                Dim K As String
                                Dim AVariant As clsVariant
                                K = Request.Form(key).ToString()
                                If K <> "" Then
                                    AVariant = New clsVariant()
                                    AVariant.VariantValue = K
                                    AVariant.Type = VariantType.vtOtherLine
                                    Answer.AnswerVariants.Add(AVariant)
                                End If
                            End If
                        Case QuestionType.qtNumber, QuestionType.qtNumberColumn
                            QPrefix = "tbNumberBox" + Question.AGUID.ToString + "s"
                            If key.Contains(QPrefix) Then
                                Dim K As String
                                Dim AVariant As clsVariant
                                K = Request.Form(key).ToString()
                                If K <> "" Then
                                    AVariant = New clsVariant()
                                    AVariant.VariantValue = K
                                    AVariant.Type = VariantType.vtOtherLine
                                    Answer.AnswerVariants.Add(AVariant)
                                End If
                            End If
                        Case QuestionType.qtVariantsCheck, QuestionType.qtImageCheck
                            QPrefix = "cbVariants" + Question.AGUID.ToString + "s"
                            If key.Contains(QPrefix) Then
                                For Each AVariant As clsVariant In Question.Variants
                                    If key.Contains(QPrefix + AVariant.AGUID.ToString + "s") Then
                                        If (AVariant.Type <> VariantType.vtOtherLine) And
                                           (AVariant.Type <> VariantType.vtOtherMemo) Then
                                            Answer.AnswerVariants.Add(AVariant)
                                        End If
                                    End If
                                Next
                            End If

                            QPrefix = "tbOther" + Question.AGUID.ToString + "s"
                            If key.Contains(QPrefix) Then
                                For Each AVariant As clsVariant In Question.Variants
                                    If (AVariant.Type = VariantType.vtOtherLine) Or
                                       (AVariant.Type = VariantType.vtOtherMemo) Then
                                        'If Request.Form(key).ToString() <> "" Then
                                        AVariant.VariantValue = Request.Form(key).ToString()
                                        Answer.AnswerVariants.Add(AVariant)
                                        'End If
                                    End If
                                Next
                            End If

                        Case QuestionType.qtObjectivesSelect
                            QPrefix = "cbObjectives" + Question.AGUID.ToString + "s"
                            If key.Contains(QPrefix) Then
                                For Each node As clsNode In Hierarchy.Nodes
                                    If key.Contains(QPrefix + node.NodeID.ToString + "s") Then
                                        node.DisabledForUser(Hierarchy.ProjectManager.User.UserID) = False
                                    End If
                                Next
                            End If

                        Case QuestionType.qtAlternativesSelect
                            QPrefix = "cbAlternatives" + Question.AGUID.ToString + "s"
                            If key.Contains(QPrefix) Then
                                For Each node As clsNode In Hierarchy.Nodes
                                    If key.Contains(QPrefix + node.NodeID.ToString + "s") Then
                                        node.DisabledForUser(Hierarchy.ProjectManager.User.UserID) = False
                                        Dim AVariant As clsVariant = New clsVariant()
                                        AVariant.VariantValue = node.NodeGuidID.ToString()
                                        AVariant.ValueType = VariantValueType.vvtString
                                        AVariant.Type = VariantType.vtSimple
                                        AVariant.Text = node.NodeName
                                        AVariant.AGUID = node.NodeGuidID
                                        Answer.AnswerVariants.Add(AVariant)
                                    End If
                                Next
                            End If

                        Case QuestionType.qtVariantsRadio
                            QPrefix = "rbVariants" + Question.AGUID.ToString + "s"
                            If key.Contains(QPrefix) Then
                                Dim K As String
                                K = Request.Form(key).ToString()
                                'L0049 ===
                                Dim AVariant As clsVariant = Question.VariantByGUID(New Guid(K))
                                'If AVariant.Type <> VariantType.vtOtherLine And _
                                '    AVariant.Type <> VariantType.vtOtherMemo Then
                                Answer.AnswerVariants.Add(AVariant)
                                'End If
                                'L0049 ==
                            End If

                            QPrefix = "tbOther" + Question.AGUID.ToString + "s"
                            If key.Contains(QPrefix) Then
                                'For Each AVariant As clsVariant In Question.Variants
                                '    If (AVariant.Type = VariantType.vtOtherLine) Or _
                                '       (AVariant.Type = VariantType.vtOtherMemo) Then
                                '        If Request.Form(key).ToString() <> "" Then
                                '            AVariant.VariantValue = Request.Form(key).ToString()
                                '            Answer.AnswerVariants.Add(AVariant)
                                '        End If
                                '    End If
                                'Next
                                For Each AVariant As clsVariant In Answer.AnswerVariants
                                    If (AVariant.Type = VariantType.vtOtherLine) Or
                                       (AVariant.Type = VariantType.vtOtherMemo) Then
                                        'If Request.Form(key).ToString() <> "" Then
                                        AVariant.VariantValue = Request.Form(key).ToString()
                                        'Answer.AnswerVariants.Add(AVariant)
                                        'End If
                                    End If
                                Next
                            End If

                        Case QuestionType.qtVariantsCombo
                            QPrefix = "ddVariants" + Question.AGUID.ToString + "s"
                            If key.Contains(QPrefix) Then
                                Dim K As String
                                K = Request.Form(key).ToString()
                                If K <> "" Then
                                    Answer.AnswerVariants.Add(Question.VariantByGUID(New Guid(K)))
                                End If
                            End If

                            QPrefix = "tbOther" + Question.AGUID.ToString + "s"
                            If key.Contains(QPrefix) Then
                                For Each AVariant As clsVariant In Question.Variants
                                    If (AVariant.Type = VariantType.vtOtherLine) Or
                                       (AVariant.Type = VariantType.vtOtherMemo) Then
                                        AVariant.VariantValue = Request.Form(key).ToString()
                                        Answer.AnswerVariants.Add(AVariant)
                                    End If
                                Next
                            End If
                    End Select
                End If
            Next
            If Answer.AnswerVariants.Count = 0 Then Answer = Nothing 'L0441
        End If

        '=s080614 - Place Rated Check
        'If (Question.Type = QuestionType.qtImageCheck) And (ProjectManager IsNot Nothing) Then
        '    Dim ListOfStates As New ArrayList
        '    ListOfStates.Clear()
        '    For Each AVariant As clsVariant In Answer.AnswerVariants
        '        ListOfStates.Add(AVariant.Text)
        '    Next
        '    SetEnabledNodesForPlacesRated(ProjectManager, ProjectManager.UserID, ListOfStates)
        'End If
        '==s080614 - Place Rated Check

        'If (Question.Type = QuestionType.qtAlternativesSelect Or Question.Type = QuestionType.qtObjectivesSelect) And (ProjectManager IsNot Nothing) Then
        '    ProjectManager.StorageManager.Writer.SaveUserDisabledNodes(ProjectManager.User.UserID)
        'End If
    End Sub
    'L0040 ==

End Class
