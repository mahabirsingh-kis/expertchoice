Imports DevExpress.Web.Data
'L0067
Imports SpyronControls.Spyron.Core
Imports SpyronControls.Spyron.DataSources
Imports Telerik.Web.UI
'L0067
'L0067
'L0067


Partial Class SurveyEdit
    Inherits clsComparionCorePage

    'Private SurveyID As Integer = 0 ' D0379
    Private _Survey As clsSurveyInfo = Nothing  ' D0860
    Private ASurveyType As SurveyType
    Private AProjectID As Integer = -1

    Private IsReadOnly As Boolean = False   ' D2495

    Public Sub New()
        MyBase.New(_PGID_SURVEY_EDIT_PRE)
    End Sub

    'Survey object stores in the session
    Public ReadOnly Property ASurvey() As clsSurvey
        Get
            If ASurveyInfo IsNot Nothing Then
                Return ASurveyInfo.Survey("-") 'L0442
            Else
                Return Nothing
            End If
        End Get
    End Property

    Public Property SessionSurveyEdit As clsSurveyInfo
        Get
            'Return CType(Session(SESSION_MAIN_SURVEY_INFO_EDIT + AProjectID.ToString + "-" + CInt(ASurveyType).ToString), clsSurveyInfo)
            Return Nothing
        End Get
        Set(value As clsSurveyInfo)
            Session(SESSION_MAIN_SURVEY_INFO_EDIT + AProjectID.ToString + "-" + CInt(ASurveyType).ToString) = value
        End Set
    End Property

    Public Property SessionSurveyView As clsSurveyInfo
        Get
            'Return CType(Session(SESSION_MAIN_SURVEY_INFO_VIEW + AProjectID.ToString + "-" + CInt(ASurveyType).ToString), clsSurveyInfo)
            Return Nothing
        End Get
        Set(value As clsSurveyInfo)
            Session(SESSION_MAIN_SURVEY_INFO_VIEW + AProjectID.ToString + "-" + CInt(ASurveyType).ToString) = value
        End Set
    End Property

    'L0034 ===
    Public Property ASurveyInfo() As clsSurveyInfo
        Get
            ' D0860 ===
            If _Survey Is Nothing Then
                Dim sProjectID As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("prjid", "")) ' Anti-XSS
                ASurveyType = CType(CheckVar("st", ASurveyType), SurveyType)    ' D4734
                'Dim sGuid As String = Sanitizer.GetSafeHtmlFragment(CheckVar("guid", ""))  ' Anti-XSS

                Dim sType As String = CheckVar("type", "").ToLower
                Select Case sType
                    Case "pre"
                        ASurveyType = If(App.ActiveProject.isImpact, SurveyType.stImpactWelcomeSurvey, SurveyType.stWelcomeSurvey)
                    Case "post"
                        ASurveyType = If(App.ActiveProject.isImpact, SurveyType.stImpactThankyouSurvey, SurveyType.stThankyouSurvey)
                End Select

                If sProjectID <> "" Then
                    Integer.TryParse(sProjectID, AProjectID)
                Else
                    If App.HasActiveProject Then AProjectID = App.ProjectID ' D6054
                End If
                If SessionSurveyEdit IsNot Nothing Then
                    _Survey = SessionSurveyEdit
                Else
                    App.ActiveSurveysList = Nothing ' D1148
                End If
                If AProjectID > 0 AndAlso _Survey Is Nothing Then 'L0313
                    'If Not IsPostBack AndAlso Not IsCallback AndAlso isSLTheme(True) AndAlso CheckVar(_PARAM_ACTION, "").ToLower = "newsurvey" Then App.ActiveSurveysList = Nothing ' D0803 + D1333
                    _Survey = App.SurveysManager.GetSurveyInfoByProjectID(AProjectID, ASurveyType, Nothing)   ' D0379
                    If _Survey IsNot Nothing Then
                        If _Survey.Survey("-") Is Nothing Then
                            _Survey.InitNewSurvey()
                            _Survey.SaveSurvey(False)
                            App.ActiveProject.SaveProjectOptions("Create survey", True, True, CStr(IIf(ASurveyType = SurveyType.stImpactWelcomeSurvey OrElse ASurveyType = SurveyType.stWelcomeSurvey, "Welcome survey", "Thank you Survey")))    ' D3804
                        End If
                        If _Survey.Survey("-").ActivePage Is Nothing Then 'L0442
                            If _Survey.Survey("-").Pages.Count = 0 Then
                                _Survey.InitNewSurvey()
                                _Survey.SaveSurvey(False)
                            End If
                            If _Survey.Survey("-").Pages.Count > 0 Then 'L0447
                                _Survey.Survey("-").ActivePageGUID = CType(_Survey.Survey("-").Pages(0), clsSurveyPage).AGUID 'L0442
                            End If
                        End If
                    End If
                    SessionSurveyEdit = _Survey
                    SessionSurveyView = Nothing
                End If
            End If
            Return _Survey
        End Get
        Set(value As clsSurveyInfo)
            SessionSurveyEdit = value
            SessionSurveyView = Nothing
        End Set
    End Property
    'L0034 + D0860 ==

    ' D3805 ===
    Public Function GetTempThemeURI_(ShowAmpersand As Boolean) As String
        Dim sRes As String = GetTempThemeURI(ShowAmpersand)
        ' -D6054
        'If Not CheckVar("close", True) Then
        '    sRes += CStr(IIf(sRes = "" AndAlso Not ShowAmpersand, "?", "&")) + "close=0"
        'End If
        Return sRes
    End Function
    ' D3805 ==

    Protected Sub Page_Init(sender As Object, e As EventArgs) Handles Me.Init
        AlignHorizontalCenter = False
        AlignVerticalCenter = False
        Session.Remove("NewQuestion") 'L0036

        ' D1044 ===
        'If Not IsPostBack Then
        '    ' D1049 ===
        '    btnViewResults.Text = PageMenuItem(_PGID_SURVEY_RESULTS)    ' D1148
        '    btnViewResults.OnClientClick = String.Format("document.location.href='{0}'; return false;", PageURL(_PGID_SURVEY_RESULTS, "prjid=" + CheckVar("prjid", "") + "&st=" + CheckVar("st", "") + GetTempThemeURI(True))) ' D1148
        'End If
        ' D1044 ==

        'L0035 === reading strings from resource file
        ASPxPageControlSpyron.TabPages(0).Text = ResString("Survey_tabProperties")
        ASPxPageControlSpyron.TabPages(1).Text = ResString("Survey_tabPages")

        btnUpdateProperties.Text = ResString("Survey_btnSave")

        ' D0542 ===
        btnNewQuestion.Text = ResString("Survey_btnNewQuestion")
        btnNewPage.Text = ResString("Survey_btnNewPage")
        'btnRenamePage.ButtonText = ResString("Survey_btnRenamePage")
        btnPageUp.Text = ResString("Survey_btnPageUp")
        btnPageDown.Text = ResString("Survey_btnPageDown")
        btnRemovePage.Text = ResString("Survey_btnRemovePage")
        ' D0542 ==

        SurveyPage.OptionDeleteButtonText = ResString("Survey_btnDeleteQuestion")
        SurveyPage.OptionEditButtonText = ResString("Survey_btnEditQuestion")
        SurveyPage.OptionMoveUpButtonText = ResString("Survey_btnMoveUpQuestion")
        SurveyPage.OptionMoveDownButtonText = ResString("Survey_btnMoveDownQuestion")

        SurveyPage.AlternativeAlternativesHierarchyText = ResString("Survey_strAltHierarchy")
        SurveyPage.AlternativeObjectivesHierarchyText = ResString("Survey_strObjHierarchy")
        'L0035 ==

        'L0056 ===
        SurveyPage.ProjectManager = App.ActiveProject.ProjectManager
        SurveyPage.RequiredQuestionAlert = ResString("Survey_alertRequiredQuestion")
        SurveyPage.MinMaxQuestionAlert = ResString("Survey_alertMinMaxQuestion")
        SurveyPage.SelectItemsMsg = ResString("Survey_msgSelectItems")
        SurveyPage.MinimumItemsMsg = ResString("Survey_msgMinimumItems")
        SurveyPage.MaximumItemsMsg = ResString("Survey_msgMaximumItems")
        SurveyPage.AndMsg = ResString("Survey_msgAnd")
        'L0056 ==

        'If Not App.isSpyronMasterDBValid Or Not App.isSpyronProjectsDBValid Then FetchAccess(_PGID_SURVEY_LIST) ' D0375 + D0488

        Dim ActivePageID As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("pid", "")) ' Anti-XSS
        Dim pid As Integer = 0
        If ActivePageID <> "" Then
            If Integer.TryParse(ActivePageID, 0) Then
                pid = CInt(ActivePageID)
                If pid < ASurvey.Pages.Count And pid >= 0 Then
                    ASurvey.ActivePageGUID = CType(ASurvey.Pages(pid), clsSurveyPage).AGUID
                End If
            End If
        End If

        If App.HasActiveProject Then
            With App.ActiveProject.ProjectManager
                .Attributes.ReadAttributeValues(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID, -1)
            End With

            IsReadOnly = App.ActiveProject.ProjectStatus = ecProjectStatus.psArchived OrElse App.ActiveProject.ProjectStatus = ecProjectStatus.psTemplate   ' D2507
            If IsReadOnly Then
                Panel4.Enabled = False
                tbPageEdit.Enabled = False
                tbPageEdit.Visible = False
                Panel6.Enabled = False
            End If
        End If

        'L0034 ===
        SurveyPage.OptionDisplayQuestionEditButton = Not IsReadOnly ' D2495
        If ASurveyInfo IsNot Nothing Then
            SurveyPage.SurveyInfo = ASurveyInfo ' D1258
            If ASurvey IsNot Nothing Then SurveyPage.SurveyPage = ASurvey.ActivePage
            SurveyPage.ReadOnlyAnswers = IsReadOnly ' D2495

            tbTitle.Text = EcSanitizer.GetSafeHtmlFragment(ASurveyInfo.Title) ' D0379 + Anti-XSS
            tbComments.Text = EcSanitizer.GetSafeHtmlFragment(ASurveyInfo.Comments) ' D0379 + Anti-XSS
            cbShowNumbers.Checked = ASurveyInfo.HideIndexNumbers 

            Dim APageNavigatorContent As String = ""
            For Each APage As clsSurveyPage In ASurvey.Pages
                If APage.AGUID <> ASurvey.ActivePageGUID Then
                    APageNavigatorContent += String.Format("<a href=""SurveyEdit.aspx?pid={0}&prjid={1}&st={2}{3}"">{4}</a>&nbsp;", ASurvey.Pages.IndexOf(APage), ASurveyInfo.ProjectID, CInt(ASurveyInfo.SurveyType), GetTempThemeURI_(True), (ASurvey.Pages.IndexOf(APage) + 1).ToString) 'L0453 + D1147
                Else
                    APageNavigatorContent += String.Format("{0}&nbsp;", (ASurvey.Pages.IndexOf(APage) + 1).ToString)
                End If
            Next
            PagesNavigator.Text = APageNavigatorContent
            'L0034 ==

            'L0026 ===
            'if new survey was created then open tab with Properties
            If CheckVar(_PARAM_TAB, "") = "props" Then    ' D0825
                ASPxPageControlSpyron.ActiveTabIndex = 0
                tbTitle.Attributes.Add("onfocus", "this.select();") 'L0490
                tbTitle.Focus()
            End If
            'L0026 ==

            ' D6054 ===
            Select Case ASurveyInfo.SurveyType
                Case SurveyType.stImpactThankyouSurvey, SurveyType.stThankyouSurvey
                    CurrentPageID = _PGID_SURVEY_EDIT_POST
            End Select
            ' D6054 ==

        Else
            ' D1663 ===
            'App.ApplicationError.Init(ecErrorStatus.errMessage, CurrentPageID, "Can't find specified survey data")
            Response.Redirect(PageURL(_PGID_ERROR_404, GetTempThemeURI_(False)), True)
            'Response.Redirect("~/404.aspx")
            ' D1663 ==
        End If
    End Sub

    Protected Sub btnUpdateProperties_Click(sender As Object, e As EventArgs) Handles btnUpdateProperties.Click
        ' D0380 ===
        ASurveyInfo.Title = EcSanitizer.GetSafeHtmlFragment(tbTitle.Text) ' Anti-XSS
        ASurveyInfo.Comments = EcSanitizer.GetSafeHtmlFragment(tbComments.Text)   ' Anti-XSS
        ASurveyInfo.HideIndexNumbers = cbShowNumbers.Checked 
        'App.SurveysManager.UpdateSurvey(ASurveyInfo)
        ASurveyInfo.SaveSurvey(False) 'L0441
        ' D0380 ==

        tbTitle.Text = EcSanitizer.GetSafeHtmlFragment(ASurveyInfo.Title)    ' D0379 + Anti-XSS
        tbComments.Text = EcSanitizer.GetSafeHtmlFragment(ASurveyInfo.Comments)  ' D0379 + Anti-XSS
        cbShowNumbers.Checked = ASurveyInfo.HideIndexNumbers 
        'App.ActiveSurveysList = Nothing ' D0249
        Response.Redirect(String.Format("SurveyEdit.aspx?pid={0}&prjid={1}&st={2}", ASurvey.Pages.IndexOf(ASurvey.ActivePage), ASurveyInfo.ProjectID.ToString, CInt(ASurveyInfo.SurveyType)) + GetTempThemeURI_(True), True)
    End Sub

    Protected Sub dsRespondents_ObjectCreating(sender As Object, e As ObjectDataSourceEventArgs) Handles dsRespondents.ObjectCreating
        If Not ASurvey Is Nothing Then
            e.ObjectInstance = New clsRespondentsDS(ASurvey)
        End If
    End Sub

    Protected Sub dsRespondentGroups_Updated(sender As Object, e As ObjectDataSourceStatusEventArgs) Handles dsRespondentGroups.Updated, dsRespondentGroups.Inserted, dsRespondentGroups.Deleted
        ASurveyInfo.SaveSurvey(False) 'L0441
    End Sub

    'L0034 ===
    Protected Sub tbPageEdit_ButtonClick(sender As Object, e As RadToolBarEventArgs) Handles tbPageEdit.ButtonClick  ' D0542
        Select Case CType(e.Item, RadToolBarButton).CommandName
            Case "NewQuestion"
                Dim ANewQuestion As clsQuestion = New clsQuestion(ASurvey.ActivePage)
                Session(SESSION_NEW_QUESTION) = ANewQuestion
                Response.Redirect(String.Format("NewQuestion.aspx?st={0}{1}&prjid={2}", CInt(ASurveyInfo.SurveyType), GetTempThemeURI_(True), AProjectID), True) 'L0315

            Case "NewPage"
                Dim ASurveyPage As New clsSurveyPage(ASurvey)
                ASurveyPage.Title = String.Format(ResString("Survey_strNewPageTitle"), ASurvey.Pages.Count + 1) 'L0035
                ASurvey.Pages.Add(ASurveyPage)
                ASurvey.ActivePageGUID = ASurveyPage.AGUID 'L0035

            Case "RenamePage"
                Dim APage As clsSurveyPage
                APage = ASurvey.ActivePage
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
                If ASurvey.Pages.Count > 1 Then
                    If Not ASurvey.ActivePage Is Nothing Then
                        Dim APage As clsSurveyPage
                        APage = ASurvey.ActivePage
                        ASurvey.Pages.Remove(APage)
                        ASurvey.ActivePageGUID = CType(ASurvey.Pages(0), clsSurveyPage).AGUID
                    End If
                End If

            Case "PageUp"
                If Not ASurvey.ActivePage Is Nothing Then
                    Dim APage As clsSurveyPage
                    APage = ASurvey.ActivePage
                    ASurvey.MovePageUp(APage) 'L0026 
                End If

            Case "PageDown"
                If Not ASurvey.ActivePage Is Nothing Then
                    Dim APage As clsSurveyPage
                    APage = ASurvey.ActivePage
                    ASurvey.MovePageDown(APage) 'L0026
                End If

        End Select
        ASurveyInfo.SaveSurvey(False) 'L0441
        Response.Redirect(String.Format("SurveyEdit.aspx?pid={0}&prjid={1}&st={2}", ASurvey.Pages.IndexOf(ASurvey.ActivePage), ASurveyInfo.ProjectID.ToString, CInt(ASurveyInfo.SurveyType)) + GetTempThemeURI_(True), True) 'L0315
    End Sub
    'L0034 ==

    'L0061 ===
    Protected Sub SurveyPage_PageTitleButtonClick(NewTitle As String) Handles SurveyPage.PageTitleButtonClick
        ASurvey.ActivePage.Title = NewTitle
        ASurveyInfo.SaveSurvey(False) 'L0441
    End Sub
    'L0061 ==

    Protected Sub SurveyPage_QuestionButtonClick(AQuestion As clsQuestion, Action As String) Handles SurveyPage.QuestionButtonClick
        Select Case Action
            Case "Edit"
                Response.Redirect(String.Format("EditQuestion.aspx?qid={0}&st={1}&prjid={2}", AQuestion.AGUID.ToString, CInt(ASurveyInfo.SurveyType), AProjectID) + GetTempThemeURI_(True), True) 'L0037 'L0315
            Case "Delete"
                ASurvey.ActivePage.Questions.Remove(AQuestion)
            Case "Up"
                ASurvey.ActivePage.MoveQuestionUp(AQuestion)
            Case "Down"
                ASurvey.ActivePage.MoveQuestionDown(AQuestion)
        End Select
        ASurveyInfo.SaveSurvey(False) 'L0441
        Response.Redirect(String.Format("SurveyEdit.aspx?pid={0}&prjid={1}&st={2}", ASurvey.Pages.IndexOf(ASurvey.ActivePage), ASurveyInfo.ProjectID.ToString, CInt(ASurveyInfo.SurveyType)) + GetTempThemeURI_(True), True)
    End Sub

    'L0072 ===
    Protected Sub dsRespondents_Selecting(sender As Object, e As ObjectDataSourceSelectingEventArgs) Handles dsRespondents.Selecting
        'e.InputParameters("GroupID") = gvRespondentGroups.GetSelectedFieldValues("GroupID")(0).ToString
        If Session("RespondentGroupID") IsNot Nothing Then
            e.InputParameters("GroupID") = Session("RespondentGroupID")
        End If
    End Sub

    'L0039 ===
    Protected Sub ASPxGridViewRespondents_RowUpdated(sender As Object, e As ASPxDataUpdatedEventArgs)
        ASurveyInfo.SaveSurvey(False) 'L0441
    End Sub
    'L0039 ==
End Class
