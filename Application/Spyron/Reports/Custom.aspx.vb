Option Strict Off

Imports DevExpress.Web.ASPxGridView
Imports System.Data
Imports System.Drawing
Imports DevExpress.Web.ASPxClasses
Imports DevExpress.Web.ASPxGridView.Export
Imports DevExpress.Web.ASPxPivotGrid
Imports SpyronControls.Spyron.Core
Imports SpyronControls.Spyron.DataSources
Imports ExpertChoice.Service.InfodocService
Imports Telerik.Web.UI

Partial Class CustomResultsPage
    Inherits clsComparionCorePage

    Public CanOpenPipe As Boolean = True    ' D3670

    Public ShowResponsive As Boolean = True ' D6500

    'L0476 ===
    Public ReadOnly Property UseSession() As Boolean
        Get
            Return GetCookie("UseSession", "True") = "True" ' D4663
        End Get
    End Property
    'L0476 ==

    Public Sub New()
        MyBase.New(_PGID_REPORT_CUSTOM)
    End Sub

    ' D3264 ===
    Private ReadOnly Property PrjID As String
        Get
            Return "Prj" + App.ProjectID.ToString + "_"
        End Get
    End Property
    ' D3264 ==

    'L0475 ===
    Private _DS0 As DataSet = Nothing
    Public Property DS0() As DataSet
        Get
            If UseSession Then
                Return Session(PrjID + "DS0")
            Else
                Return _DS0
            End If
        End Get
        Set(value As DataSet)
            If UseSession Then
                Session(PrjID + "DS0") = value
            Else
                _DS0 = value
            End If
        End Set
    End Property

    Private _DS1 As DataSet = Nothing
    Public Property DS1() As DataSet
        Get
            If UseSession Then
                Return Session(PrjID + "DS1")
            Else
                Return _DS1
            End If
        End Get
        Set(value As DataSet)
            If UseSession Then
                Session(PrjID + "DS1") = value
            Else
                _DS1 = value
            End If
        End Set
    End Property

    Private _DS3 As DataSet = Nothing
    Public Property DS3() As DataSet
        Get
            If UseSession Then
                Return Session(PrjID + "DS3")
            Else
                Return _DS3
            End If
        End Get
        Set(value As DataSet)
            If UseSession Then
                Session(PrjID + "DS3") = value
            Else
                _DS3 = value
            End If
        End Set
    End Property

    Private _DS4 As DataSet = Nothing
    Public Property DS4() As DataSet
        Get
            If UseSession Then
                Return Session(PrjID + "DS4")
            Else
                Return _DS4
            End If
        End Get
        Set(value As DataSet)
            If UseSession Then
                Session(PrjID + "DS4") = value
            Else
                _DS4 = value
            End If
        End Set
    End Property
    'L0475 ==
    'L0493 ===
    Private _DS5 As DataSet = Nothing
    Public Property DS5() As DataSet
        Get
            If UseSession Then
                Return Session(PrjID + "DS5")
            Else
                Return _DS5
            End If
        End Get
        Set(value As DataSet)
            If UseSession Then
                Session(PrjID + "DS5") = value
            Else
                _DS5 = value
            End If
        End Set
    End Property
    'L0493 ==

    Private Sub ClearSessionDS()
        DS0 = Nothing
        DS1 = Nothing
        DS3 = Nothing
        DS4 = Nothing
        DS5 = Nothing
        DS6 = Nothing
    End Sub

    'L0503 ===
    Private _DS6 As DataSet = Nothing
    Public Property DS6() As DataSet
        Get
            If UseSession Then
                Return Session(PrjID + "DS6")
            Else
                Return _DS6
            End If
        End Get
        Set(value As DataSet)
            If UseSession Then
                Session(PrjID + "DS6") = value
            Else
                _DS6 = value
            End If
        End Set
    End Property
    'L0503 ==

    Protected Sub Page_Init(sender As Object, e As EventArgs) Handles Me.Init
        ' D0707 ===
        pnlReportOptions.Visible = True
        Dim sTab As String = CheckVar(_PARAM_TAB, "").ToLower    ' D4785
        Select Case sTab
            Case "tab3" 'L0347
                ASPxPageControlReports.ActiveTabIndex = 2
                CurrentPageID = _PGID_REPORT_CUSTOM_OBJ_PRIORITY    ' D4785
            Case "tab4" 'L0347
                ASPxPageControlReports.ActiveTabIndex = 3
                CurrentPageID = _PGID_REPORT_CUSTOM_EVAL_PRG    ' D4785
            Case "tab5", "tab7" 'L0493
                ASPxPageControlReports.ActiveTabIndex = 4
                CurrentPageID = If(sTab = "tab7", _PGID_SYNTHESIZE_INCONSIST, _PGID_REPORT_CUSTOM_INCONSIST)    ' D4785
            Case "tab6", "tab8"
                ASPxPageControlReports.ActiveTabIndex = 5
                CurrentPageID = If(sTab = "tab8", _PGID_SYNTHESIZE_SURVEY, _PGID_REPORT_CUSTOM_SURVEY)    ' D4785
            Case "judgments"
                CurrentPageID = _PGID_REPORT_CUSTOM_JUDGMENTS    ' D4785
                ASPxPageControlReports.ActiveTabIndex = 1
            Case "csv"
                ASPxPageControlReports.ActiveTabIndex = 6
                CurrentPageID = _PGID_REPORT_CSV        ' D6299
                pnlReportOptions.Visible = False
            Case Else
                CurrentPageID = _PGID_REPORT_CUSTOM_PRIORITY    ' D4785
                ASPxPageControlReports.ActiveTabIndex = 0
        End Select
        ASPxPageControlReports.TabPages(0).Text = PageMenuItem(_PGID_REPORT_CUSTOM_PRIORITY)
        ASPxPageControlReports.TabPages(1).Text = PageMenuItem(_PGID_REPORT_CUSTOM_JUDGMENTS)
        'If isSLTheme() Then  ' D0729 + D0766 -D4874
        ASPxPageControlReports.ShowTabs = False
            ASPxPageControlReports.ContentStyle.Border.BorderStyle = BorderStyle.None
        'End If ' -D4874
        ' D0707 ==

        ' D1551 ===
        For i = 0 To ASPxPageControlReports.TabPages.Count - 1
            ASPxPageControlReports.TabPages(i).Enabled = (i = ASPxPageControlReports.ActiveTabIndex)
            ASPxPageControlReports.TabPages(i).Visible = ASPxPageControlReports.TabPages(i).Enabled
        Next
        ' D1551 ==

        'L0431 ===
        'btnGenerateJudgements.Text = ResString("btnGenerateReport")
        btnGeneratePrioritesOverview.Text = ResString("btnGenerateReport")
        btnGeneratePWJudgements.Text = ResString("btnGenerateReport")
        btnEvalProgress.Text = ResString("btnGenerateReport")
        btnObjPriorities.Text = ResString("btnGenerateReport")
        btnInconsistency.Text = ResString("btnGenerateReport") 'L0493
        lblError.Text = ResString("msgEmptyReport")
        'L0431 ==

        '' D2826 ===
        ClientScript.RegisterOnSubmitStatement(GetType(String), "Loader", "showLoadingPanel(); setTimeout(function() { hideLoadingPanel(); }, 3000);")   ' D2878
        'pnlLoading.Caption = ResString("msgLoading")
        'pnlLoading.Message = String.Format("<div style='margin:1em'><img src={1} alt='{0}' border=0/></div>&nbsp;&nbsp;{0}", ResString("lblPleaseWait"), ImagePath + "process.gif")
        '' D2826 ==

        Dim Participants As New List(Of clsUser)
        ddlGroups.Items.Clear()
        For Each Group As clsCombinedGroup In App.ActiveProject.ProjectManager.CombinedGroups.GroupsList
            ddlGroups.Items.Add("[" + Group.Name + "]")
        Next

        For Each User As clsUser In App.ActiveProject.ProjectManager.UsersList
            Dim AListItem As New ListItem(User.UserName, User.UserEMail)
            ddlGroups.Items.Add(AListItem)
        Next

        ' D3670 ===
        If Not App.ActiveProject.isOnline OrElse Not App.ActiveProject.isPublic Then
            Dim tPrj = App.ActiveProject
            If tPrj.ProjectStatus = ecProjectStatus.psActive Then
                Dim fCanBeOnline As Boolean = App.ActiveProject.isOnline OrElse App.ActiveWorkgroup.License.CheckParameterByID(ecLicenseParameter.MaxProjectsOnline, Nothing, False)
                If Not tPrj.isMarkedAsDeleted AndAlso Not tPrj.isTeamTimeImpact AndAlso Not tPrj.isTeamTimeLikelihood AndAlso
                   App.CanUserModifyProject(App.ActiveUser.UserID, tPrj.ID, App.ActiveUserWorkgroup, App.ActiveWorkspace, App.ActiveWorkgroup) AndAlso fCanBeOnline Then
                Else
                    CanOpenPipe = False
                End If
            Else
                CanOpenPipe = False
            End If
        End If
        ShowResponsive = CanOpenPipe AndAlso App.isEvalURL_EvalSite ' D6500

        If isAjax() Then

            Dim sResult As String = ""

            Select Case CheckVar("action", "").ToLower
                Case "link", "open" ' D6364
                    Dim sEmail As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("email", ""))   'Anti-XSS
                    Dim sNodeGUID As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("guid", "")) 'Anti-XSS
                    If sEmail <> "" AndAlso CanOpenPipe Then
                        Dim sEvalURL As String = App.Options.EvalSiteURL    ' D3889
                        If Not ShowResponsive Then App.Options.EvalSiteURL = ""    ' D3889 + D6500
                        'If CheckVar("show_indiv", False) AndAlso App.ActiveProject.PipeParameters.LocalResultsView = ResultsView.rvNone Then
                        '    App.ActiveProject.PipeParameters.LocalResultsView = ResultsView.rvIndividual
                        '    App.ActiveProject.SaveProjectOptions("Update evaluation setting", , , "View Intermediate results: Individual (on call from Consesnsus View)")
                        'End If
                        ' D6227 ===
                        Dim isOpen As Boolean = CheckVar("action", "").ToLower = "open"
                        If isOpen AndAlso Not ShowResponsive Then App.Options.EvalURLPostfix = "&ignoreoffline=yes&ignorepsw=yes"  ' D6364
                        Dim sPath As String = ParseAllTemplates(_TEMPL_URL_EVALUATE, App.DBUserByEmail(sEmail), App.ActiveProject) ' D6364
                        'If sPath <> "" Then PrepareProjectForOpenPipe(App.ActiveProject)    ' D6363 -D6364
                        If isOpen Then App.Options.EvalURLPostfix = ""  ' D6364
                        If Not ShowResponsive AndAlso sEmail.ToLower = App.ActiveUser.UserEmail.ToLower Then sPath = PageURL(_PGID_EVALUATION, "incons")    ' D6500
                        If sPath <> "" Then sResult = String.Format("{0}{1}&mode=searchresults&node={2}&show_indiv={3}{4}", sPath, If(ShowResponsive, If(isOpen, "", "&" + _PARAM_READONLY + "=1"), "&id=" + HttpUtility.UrlEncode(sEmail)), HttpUtility.UrlEncode(sNodeGUID), Bool2Num(App.ActiveProject.PipeParameters.LocalResultsView = ResultsView.rvNone OrElse App.ActiveProject.PipeParameters.LocalResultsView = ResultsView.rvGroup), If(ShowResponsive, "", "&ignoreval=1"))   ' D4009 + D6362 + D6500
                        ' D6227 ==
                        App.Options.EvalSiteURL = sEvalURL  ' D3889
                    End If
            End Select

            Response.Clear()
            Response.ContentType = "text/plain"
            Response.Write(sResult)
            Response.End()

        End If
        ' D3670 ==

    End Sub

    Private PM As clsProjectManager ' = App.ActiveProject.ProjectManager

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        AlignHorizontalCenter = False
        AlignVerticalCenter = False
        PM = App.ActiveProject.ProjectManager
        PM.ProjectDataProvider.FullAlternativePath = False
        If Session(PrjID + "DSADHOC") Is Nothing Then
            Session(PrjID + "DSADHOC") = App.ActiveProject.Passcode
        End If
    End Sub

    'L0251 ===
    Protected Sub tbExport_ButtonClick(sender As Object, e As RadToolBarEventArgs) Handles tbExport.ButtonClick
        Dim FileName As String = "PairwiseJudgements"
        App.DBSaveLog(dbActionType.actReportStart, dbObjectType.einfProjectReport, App.ProjectID, "Ad-Hoc " + FileName, String.Format("{0}, {1}", CType(e.Item, RadToolBarButton).CommandName, ddlGroups.SelectedValue)) ' D2237
        Select Case CType(e.Item, RadToolBarButton).CommandName
            Case "ExporttoRTF"
                ASPxGridViewExporter1.WriteRtfToResponse(FileName, True)
            Case "ExporttoPDF"
                ASPxGridViewExporter1.WritePdfToResponse(FileName, True)
            Case "ExporttoXLS"
                ASPxGridViewExporter1.WriteXlsToResponse(FileName, True)
            Case "Generate"
                'L0359 ===
                DS1 = Nothing 'L0507
                'L0359 ==
                'L0345 ===
                ASPxGridView1.Visible = True
                ASPxGridView1_Load(sender, e)
                'L0345 ==
        End Select
    End Sub
    'L0251 ==

    'L0345 ===
    Protected Sub tbExport3_ButtonClick(sender As Object, e As RadToolBarEventArgs) Handles tbExport3.ButtonClick
        Dim FileName As String = "UsersObjectivesPriorities"
        App.DBSaveLog(dbActionType.actReportStart, dbObjectType.einfProjectReport, App.ProjectID, "Ad-Hoc " + FileName, String.Format("{0}, {1}", CType(e.Item, RadToolBarButton).CommandName, ddlGroups.SelectedValue)) ' D2237
        Select Case CType(e.Item, RadToolBarButton).CommandName
            Case "ExporttoRTF"
                ASPxGridViewExporter3.WriteRtfToResponse(FileName, True)
            Case "ExporttoPDF"
                ASPxGridViewExporter3.WritePdfToResponse(FileName, True)
            Case "ExporttoXLS"
                ASPxGridViewExporter3.WriteXlsToResponse(FileName, True)
            Case "Generate"
                'L0359 ===
                DS3 = Nothing 'L0507
                'L0359 ==
                ASPxGridView3.Visible = True
                ASPxGridView3_Load(sender, e)
        End Select
    End Sub

    'L0431 ===
    Protected Sub ASPxGridView1_HtmlDataCellPrepared(sender As Object, e As ASPxGridViewTableDataCellEventArgs) Handles ASPxGridView1.HtmlDataCellPrepared
        If e.DataColumn.FieldName = "Value" Then
            Dim strValue As String = CStr(e.CellValue)
            Dim sngValue As Single
            If Single.TryParse(strValue, sngValue) Then
                e.Cell.Text = sngValue.ToString("G2").Replace("-", "1/") 'L0468
                If e.CellValue.ToString.Contains("-") Then
                    e.Cell.ForeColor = Color.Red 'L0435
                Else
                    e.Cell.ForeColor = Color.Blue 'L0435
                End If
            Else
                e.Cell.Text = strValue
            End If
        End If
        If e.DataColumn.FieldName = "NodeID" Then
            e.Cell.Text = PM.Hierarchy(PM.ActiveHierarchy).GetNodeByID(CInt(e.CellValue)).NodeName  ' D3925
        End If
    End Sub
    'L0431 ==

    Protected Sub ASPxGridView1_Load(sender As Object, e As EventArgs) Handles ASPxGridView1.Load
        If DS1 IsNot Nothing And Session(PrjID + "DSADHOC") = App.ActiveProject.Passcode Then 'L0433
            ASPxGridView1.Visible = True
        End If
        If ASPxGridView1.Visible Then
            If DS1 Is Nothing Or Session(PrjID + "DSADHOC") <> App.ActiveProject.Passcode Then
                'Dim GID = CType(App.ActiveProject.ProjectManager.CombinedGroups.GroupsList(ddlGroups.SelectedIndex), clsCombinedGroup).CombinedUserID
                Dim UID As Integer
                If ddlGroups.SelectedIndex >= App.ActiveProject.ProjectManager.CombinedGroups.GroupsList.Count Then
                    UID = App.ActiveProject.ProjectManager.UsersList(ddlGroups.SelectedIndex - App.ActiveProject.ProjectManager.CombinedGroups.GroupsList.Count).UserID
                Else
                    UID = CType(App.ActiveProject.ProjectManager.CombinedGroups.GroupsList(ddlGroups.SelectedIndex), clsCombinedGroup).CombinedUserID
                End If
                DS1 = PM.ProjectDataProvider.ReportDataSet(CanvasReportType.crtAllJudgmentsInOne, , UID)
            End If
            btnPDF.Enabled = App.isExportAvailable 'L0379
            btnRTF.Enabled = App.isExportAvailable 'L0379
            btnXLS.Enabled = App.isExportAvailable 'L0379
            btnGeneratePWJudgements.Text = "Refresh" 'L0507
            ASPxGridView1.DataSource = DS1
            ASPxGridView1.DataMember = "AllJudgments"
            ASPxGridView1.DataBind()

            'L0431 'L0465 ===
            ASPxGridView1.Columns(0).Caption = ResString("tblUserName1")
            ASPxGridView1.Columns(1).Caption = ResString("tblUserEmail")
            ASPxGridView1.Columns(2).Caption = ResString("tblWRT")
            ASPxGridView1.Columns(3).Caption = ResString("tblElement1")
            ASPxGridView1.Columns(4).Caption = ResString("tblElement2")
            ASPxGridView1.Columns(5).Caption = ResString("tblValue")
            ASPxGridView1.Columns(6).Caption = ResString("tblComment")
            ASPxGridView1.Columns(7).Caption = ResString("tblMeasurementType")
            ASPxGridView1.Columns(8).Visible = False
            ASPxGridView1.Columns(5).CellStyle.HorizontalAlign = HorizontalAlign.Right
            'ASPxGridView1.Columns(3).CellStyle.HorizontalAlign = HorizontalAlign.Left
            'L0431 'L0465 ==
        End If
    End Sub

    Protected Sub ASPxGridView3_Load(sender As Object, e As EventArgs) Handles ASPxGridView3.Load
        If DS3 IsNot Nothing And Session(PrjID + "DSADHOC") = App.ActiveProject.Passcode Then 'L0433
            ASPxGridView3.Visible = True
        End If
        If ASPxGridView3.Visible Then
            If DS3 Is Nothing Or Session(PrjID + "DSADHOC") <> App.ActiveProject.Passcode Then
                'Dim GID = CType(App.ActiveProject.ProjectManager.CombinedGroups.GroupsList(ddlGroups.SelectedIndex), clsCombinedGroup).CombinedUserID
                Dim UID As Integer
                If ddlGroups.SelectedIndex >= App.ActiveProject.ProjectManager.CombinedGroups.GroupsList.Count Then
                    UID = App.ActiveProject.ProjectManager.UsersList(ddlGroups.SelectedIndex - App.ActiveProject.ProjectManager.CombinedGroups.GroupsList.Count).UserID
                Else
                    UID = CType(App.ActiveProject.ProjectManager.CombinedGroups.GroupsList(ddlGroups.SelectedIndex), clsCombinedGroup).CombinedUserID
                End If
                DS3 = PM.ProjectDataProvider.ReportDataSet(CanvasReportType.crtUsersObjectivesPriorities, , UID)
            End If
            btnPDF3.Enabled = App.isExportAvailable 'L0379
            btnRTF3.Enabled = App.isExportAvailable 'L0379
            btnXLS3.Enabled = App.isExportAvailable 'L0379
            btnObjPriorities.Text = "Refresh" 'L0507
            ASPxGridView3.DataSource = DS3
            ASPxGridView3.DataMember = "UsersObjectivesPriorities"
            ASPxGridView3.DataBind()

            'L0431 'L0465===
            ASPxGridView3.Columns(0).Visible = False
            ASPxGridView3.Columns(1).Caption = ResString("tblUserEmail")
            ASPxGridView3.Columns(2).Caption = ResString("tblUserName1")
            ASPxGridView3.Columns(3).Caption = ResString("tblObjective")
            ASPxGridView3.Columns(4).Caption = ResString("tblPath")
            ASPxGridView3.Columns(5).Caption = ResString("tblLocalPriority")
            CType(ASPxGridView3.Columns(5), GridViewDataTextColumn).PropertiesTextEdit.DisplayFormatString = "p2"
            ASPxGridView3.Columns(6).Caption = ResString("tblGlobalPriority")
            CType(ASPxGridView3.Columns(6), GridViewDataTextColumn).PropertiesTextEdit.DisplayFormatString = "p2"
            ASPxGridView3.Columns(7).Caption = ResString("tblInconsistency")
            CType(ASPxGridView3.Columns(7), GridViewDataTextColumn).PropertiesTextEdit.DisplayFormatString = "n2" 'L0468
            'L0431 'L0465==
        End If
    End Sub

    'L0360 ===
    'L0400 ===
    'L0401 ===
    Protected Sub ASPxPivotGrid1_Load(sender As Object, e As EventArgs) Handles ASPxPivotGrid1.Load
        If DS0 IsNot Nothing And Session(PrjID + "DSADHOC") = App.ActiveProject.Passcode Then 'L0433
            ASPxPivotGrid1.Visible = True
        End If
        If ASPxPivotGrid1.Visible Then
            btnPDF0.Enabled = App.isExportAvailable
            btnRTF0.Enabled = App.isExportAvailable
            btnXLS0.Enabled = App.isExportAvailable
            btnGeneratePrioritesOverview.Text = "Refresh" 'L0507
            If DS0 Is Nothing Or Session(PrjID + "DSADHOC") <> App.ActiveProject.Passcode Then
                Dim ADataSet As DataSet
                'Dim GID = CType(App.ActiveProject.ProjectManager.CombinedGroups.GroupsList(ddlGroups.SelectedIndex), clsCombinedGroup).CombinedUserID
                If ddlGroups.SelectedIndex >= App.ActiveProject.ProjectManager.CombinedGroups.GroupsList.Count Then
                    Dim AUser As clsUser = App.ActiveProject.ProjectManager.UsersList(ddlGroups.SelectedIndex - App.ActiveProject.ProjectManager.CombinedGroups.GroupsList.Count)
                    ADataSet = PM.ProjectDataProvider.ReportDataSet(CanvasReportType.crtPivotAlternativesPriorities, AUser)
                Else
                    Dim AGroupType As GroupReportType = GroupReportType.grtGroupOnly
                    If chkAllUsers.Checked Then
                        AGroupType = GroupReportType.grtBoth
                    Else
                        AGroupType = GroupReportType.grtGroupOnly
                    End If
                    Dim aGroup As clsCombinedGroup = CType(App.ActiveProject.ProjectManager.CombinedGroups.GroupsList(ddlGroups.SelectedIndex), clsCombinedGroup)
                    ADataSet = PM.ProjectDataProvider.ReportDataSet(CanvasReportType.crtPivotAlternativesPriorities, aGroup, AGroupType)
                End If

                DS0 = ADataSet
                If ADataSet.Tables("PivotAlternativesPriorities").Rows.Count = 0 Then
                    ASPxPivotGrid1.Visible = False
                    lblError.Visible = True
                    Session.Remove("DS0")
                Else
                    lblError.Visible = False
                    ASPxPivotGrid1.DataSource = DS0
                    ASPxPivotGrid1.DataMember = "PivotAlternativesPriorities"
                    ASPxPivotGrid1.DataBind()
                End If
                'L0402 ===
            Else
                lblError.Visible = False
                ASPxPivotGrid1.DataSource = DS0
                ASPxPivotGrid1.DataMember = "PivotAlternativesPriorities"
                ASPxPivotGrid1.DataBind()
                'L0402 ==
            End If
            ASPxPivotGrid1.Fields(0).Caption = ResString("tblAlternative")
            ASPxPivotGrid1.Fields(1).Caption = ResString("tblUser")
            ASPxPivotGrid1.Fields(2).Caption = ResString("tblPriority")
        End If
    End Sub

    Protected Sub ASPxPivotGrid1_CustomCellDisplayText(sender As Object, e As PivotCellDisplayTextEventArgs) Handles ASPxPivotGrid1.CustomCellDisplayText
        If e.Value IsNot Nothing Then
            If e.Value.ToString = "-1" Then e.DisplayText = "N/A"
        End If
    End Sub
    'L0400 ==
    'L0360 ==
    'L0345 ==

    'L0347 ===
    Protected Sub ASPxGridView4_Load(sender As Object, e As EventArgs) Handles ASPxGridView4.Load
        If DS4 IsNot Nothing And Session(PrjID + "DSADHOC") = App.ActiveProject.Passcode Then 'L0433
            ASPxGridView4.Visible = True
        End If
        If ASPxGridView4.Visible Then
            'If DS4 Is Nothing Or Session("DSADHOC") <> App.ActiveProject.Passcode Then
            'Dim GID = CType(App.ActiveProject.ProjectManager.CombinedGroups.GroupsList(ddlGroups.SelectedIndex), clsCombinedGroup).CombinedUserID
            'PM.ProjectDataProvider.GroupUserIDs = New List(Of Integer)
            'PM.ProjectDataProvider.GroupUserIDs.Add(GID)
            Dim UID As Integer
            If ddlGroups.SelectedIndex >= App.ActiveProject.ProjectManager.CombinedGroups.GroupsList.Count Then
                UID = App.ActiveProject.ProjectManager.UsersList(ddlGroups.SelectedIndex - App.ActiveProject.ProjectManager.CombinedGroups.GroupsList.Count).UserID
            Else
                UID = CType(App.ActiveProject.ProjectManager.CombinedGroups.GroupsList(ddlGroups.SelectedIndex), clsCombinedGroup).CombinedUserID
            End If
            DS4 = PM.ProjectDataProvider.ReportDataSet(CanvasReportType.crtEvaluationProgress, , UID)
            'End If
            btnPDF4.Enabled = App.isExportAvailable 'L0379
            btnRTF4.Enabled = App.isExportAvailable 'L0379
            btnXLS4.Enabled = App.isExportAvailable 'L0379
            btnEvalProgress.Text = "Refresh" 'L0507 
            ASPxGridView4.DataSource = DS4
            ASPxGridView4.DataMember = "EvaluationProgress"
            ASPxGridView4.DataBind()

            'L0431 ===
            ASPxGridView4.Columns(0).Caption = ResString("tblN")
            ASPxGridView4.Columns(1).Caption = ResString("tblUserName1")
            ASPxGridView4.Columns(2).Caption = ResString("tblUserEmail")
            ASPxGridView4.Columns(3).Caption = ResString("tblJudgmentsMade")
            ASPxGridView4.Columns(4).Caption = ResString("tblJudgementsTotal")
            ASPxGridView4.Columns(5).Caption = ResString("tblPercentage")
            'L0431 ==
        End If
    End Sub

    Protected Sub tbExport4_ButtonClick(sender As Object, e As RadToolBarEventArgs) Handles tbExport4.ButtonClick
        Dim FileName As String = "EvaluationProgress"
        App.DBSaveLog(dbActionType.actReportStart, dbObjectType.einfProjectReport, App.ProjectID, "Ad-Hoc " + FileName, String.Format("{0}, {1}", CType(e.Item, RadToolBarButton).CommandName, ddlGroups.SelectedValue)) ' D2237
        Select Case CType(e.Item, RadToolBarButton).CommandName
            Case "ExporttoRTF"
                ASPxGridViewExporter4.WriteRtfToResponse(FileName, True)
            Case "ExporttoPDF"
                ASPxGridViewExporter4.WritePdfToResponse(FileName, True)
            Case "ExporttoXLS"
                ASPxGridViewExporter4.WriteXlsToResponse(FileName, True)
            Case "Generate"
                'L0359 ===
                DS4 = Nothing 'L0507
                'L0359 ==
                ASPxGridView4.Visible = True
                ASPxGridView4_Load(sender, e)
        End Select
    End Sub
    'l0347 ==

    'L0360 ===
    'L0400 ===
    Protected Sub tbExport0_ButtonClick(sender As Object, e As RadToolBarEventArgs) Handles tbExport0.ButtonClick
        Dim FileName As String = "PrioritiesOverview"
        Dim filePath As String = File_CreateTempName()
        Dim sContentType As String = "application/octet-stream"
        Dim isExport As Boolean = False

        App.DBSaveLog(dbActionType.actReportStart, dbObjectType.einfProjectReport, App.ProjectID, "Ad-Hoc " + FileName, String.Format("{0}, {1}", CType(e.Item, RadToolBarButton).CommandName, ddlGroups.SelectedValue)) ' D2237
        Select Case CType(e.Item, RadToolBarButton).CommandName
            Case "ExporttoRTF"
                ASPxPivotGridExporter1.ExportToRtf(filePath)
                sContentType = "application/msword"
                FileName = "rtf"
                isExport = True
            Case "ExporttoPDF"
                ASPxPivotGridExporter1.ExportToPdf(filePath)
                sContentType = "application/pdf"
                FileName = "pdf"
                isExport = True
            Case "ExporttoXLS"
                ASPxPivotGridExporter1.ExportToXls(filePath)
                sContentType = "application/vnd.ms-excel"
                FileName = "xls"
                isExport = True
            Case "Generate"
                DS0 = Nothing 'L0507
                ASPxPivotGrid1.Visible = True
                ASPxPivotGrid1_Load(sender, e)
        End Select
        If isExport Then
            FileName = GetProjectFileName("PrioritiesOverview", App.ActiveProject.Passcode, "report", FileName)
            DownloadFile(filePath, sContentType, FileName, dbObjectType.einfProjectReport, App.ProjectID)   ' D6593
            'RawResponseStart()
            'Response.AppendHeader("Content-Disposition", String.Format("attachment; filename=""{0}""", HttpUtility.UrlEncode(SafeFileName(FileName))))	' D3478 + D6591
            'Response.ContentType = sContentType
            'Dim fileLen As Long = MyComputer.FileSystem.GetFileInfo(filePath).Length
            'Response.AddHeader("Content-Length", CStr(fileLen))
            'DebugInfo(String.Format("Start transferring for {0} bytes", fileLen))
            'Response.BinaryWrite(MyComputer.FileSystem.ReadAllBytes(filePath))
            'App.DBSaveLog(dbActionType.actDownload, dbObjectType.einfProjectReport, App.ProjectID, "Export custom report", String.Format("Filename: {0}; Size: {1}", FileName, fileLen))    ' D2236
            'File_Erase(filePath)
        End If
        'If isExport Then RawResponseEnd() ' D2237
    End Sub
    'L0400 ==
    'L0360 ==

    'L0493 ===
    Protected Sub tbExport5_ButtonClick(sender As Object, e As RadToolBarEventArgs) Handles tbExport5.ButtonClick
        Dim FileName As String = "Inconsistencies"
        App.DBSaveLog(dbActionType.actReportStart, dbObjectType.einfProjectReport, App.ProjectID, "Ad-Hoc " + FileName, String.Format("{0}, {1}", CType(e.Item, RadToolBarButton).CommandName, ddlGroups.SelectedValue)) ' D2237
        Select Case CType(e.Item, RadToolBarButton).CommandName
            Case "ExporttoRTF"
                ASPxGridViewExporter5.WriteRtfToResponse(FileName, True)
            Case "ExporttoPDF"
                ASPxGridViewExporter5.WritePdfToResponse(FileName, True)
            Case "ExporttoXLS"
                ASPxGridViewExporter5.WriteXlsToResponse(FileName, True)
            Case "Generate"
                'L0359 ===
                DS5 = Nothing 'L0507
                'L0359 ==
                ASPxGridView5.Visible = True
                ASPxGridView5_Load(sender, e)
        End Select
    End Sub

    Protected Sub ASPxGridView5_Load(sender As Object, e As EventArgs) Handles ASPxGridView5.Load
        If DS5 IsNot Nothing And Session(PrjID + "DSADHOC") = App.ActiveProject.Passcode Then 'L0433
            ASPxGridView5.Visible = True
        End If
        If ASPxGridView5.Visible Then
            If DS5 Is Nothing Or Session(PrjID + "DSADHOC") <> App.ActiveProject.Passcode Then
                'Dim GID = CType(App.ActiveProject.ProjectManager.CombinedGroups.GroupsList(ddlGroups.SelectedIndex), clsCombinedGroup).CombinedUserID
                Dim UID As Integer
                If ddlGroups.SelectedIndex >= App.ActiveProject.ProjectManager.CombinedGroups.GroupsList.Count Then
                    UID = App.ActiveProject.ProjectManager.UsersList(ddlGroups.SelectedIndex - App.ActiveProject.ProjectManager.CombinedGroups.GroupsList.Count).UserID
                Else
                    UID = CType(App.ActiveProject.ProjectManager.CombinedGroups.GroupsList(ddlGroups.SelectedIndex), clsCombinedGroup).CombinedUserID
                End If
                DS5 = PM.ProjectDataProvider.ReportDataSet(CanvasReportType.crtInconsistencies, , UID)
            End If

            btnPDF5.Enabled = App.isExportAvailable 'L0379
            btnRTF5.Enabled = App.isExportAvailable 'L0379
            btnXLS5.Enabled = App.isExportAvailable 'L0379
            btnInconsistency.Text = "Refresh" 'L0507
            ASPxGridView5.DataSource = DS5
            ASPxGridView5.DataMember = "Inconsistencies"
            ASPxGridView5.DataBind()

            ASPxGridView5.Columns(0).Caption = ResString("tblUserName")     ' D7625
            ASPxGridView5.Columns(1).Caption = ResString("tblUserEmail")    ' D7625
            ASPxGridView5.Columns(2).Caption = ResString("tblObjective")
            ASPxGridView5.Columns(3).Caption = ResString("tblPath")
            ASPxGridView5.Columns(4).Caption = ResString("tblInconsistency")
            'CType(ASPxGridView5.Columns(4), GridViewDataTextColumn).PropertiesTextEdit.DisplayFormatString = "n4"
            ASPxGridView5.Columns(5).Caption = ResString("tblNumberOfChildren")
            ASPxGridView5.Columns(6).Caption = ResString("tblAction")
        End If
    End Sub

    Protected Sub ddlGroups_Load(sender As Object, e As EventArgs) Handles ddlGroups.Load
        If Not IsPostBack And Session(PrjID + "GroupUserID" + App.ActiveProject.ID.ToString) IsNot Nothing Then
            ddlGroups.SelectedIndex = CInt(Session(PrjID + "GroupUserID" + App.ActiveProject.ID.ToString))
        End If
    End Sub
    'L0493 ==

    Protected Sub ddlGroups_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlGroups.SelectedIndexChanged
        ClearSessionDS()
        Session(PrjID + "GroupUserID" + App.ActiveProject.ID.ToString) = ddlGroups.SelectedIndex
        Dim sTBClientID As String = ""  ' D1551
        Select Case ASPxPageControlReports.ActiveTabIndex
            Case 0
                ASPxPivotGrid1.Visible = True
                ASPxPivotGrid1_Load(sender, e)
                sTBClientID = tbExport0.ClientID    ' D1551
            Case 1
                ASPxGridView1.Visible = True
                ASPxGridView1_Load(sender, e)
                'ASPxGridView2.Visible = True
                'ASPxGridView2_Load(sender, e)
                sTBClientID = tbExport.ClientID     ' D1551
            Case 2
                ASPxGridView3.Visible = True
                ASPxGridView3_Load(sender, e)
                sTBClientID = tbExport3.ClientID    ' D1551
            Case 3
                ASPxGridView4.Visible = True
                ASPxGridView4_Load(sender, e)
                sTBClientID = tbExport4.ClientID    ' D1551
            Case 4
                ASPxGridView5.Visible = True
                ASPxGridView5_Load(sender, e)
                sTBClientID = tbExport5.ClientID    ' D1551
            Case 5
                If PanelWelcomeSurvey.Visible Then
                    _SurveyWelcome = Nothing
                    gvResultOverview.DataBind()
                    gvStatisticResults.DataBind()
                End If
                If PanelThankyouSurvey.Visible Then
                    _SurveyThankyou = Nothing
                    gvResultOverview2.DataBind()
                    gvStatisticResults2.DataBind()
                End If

        End Select
        If sTBClientID <> "" Then ScriptManager.RegisterStartupScript(Me, GetType(String), "FlashButton", String.Format("setTimeout(""BtnFlash('{0}', 1);"", 250); setTimeout(""BtnFlash('{0}', 1);"", 750);", JS_SafeString(sTBClientID)), True) ' D1551
    End Sub

    'Survey results ===
    Private _SurveyWelcome As clsSurveyInfo = Nothing
    Private _SurveyThankyou As clsSurveyInfo = Nothing

    Public ReadOnly Property AWelcomeSurvey() As clsSurvey
        Get
            If App.ActiveProject.isImpact Then
                Return ASurveyInfo(SurveyType.stImpactWelcomeSurvey).Survey("")
            Else
                Return ASurveyInfo(SurveyType.stWelcomeSurvey).Survey("")
            End If
        End Get
    End Property

    Public ReadOnly Property AThankyouSurvey() As clsSurvey
        Get
            If App.ActiveProject.isImpact Then
                Return ASurveyInfo(SurveyType.stImpactThankyouSurvey).Survey("")
            Else
                Return ASurveyInfo(SurveyType.stThankyouSurvey).Survey("")
            End If
        End Get
    End Property

    Public Function ASurveyInfo(ASurveyType As SurveyType) As clsSurveyInfo
        Dim fSurveyInfo As clsSurveyInfo = Nothing
        If ((ASurveyType = SurveyType.stWelcomeSurvey Or ASurveyType = SurveyType.stImpactWelcomeSurvey) AndAlso _SurveyWelcome Is Nothing) Or _
           ((ASurveyType = SurveyType.stThankyouSurvey Or ASurveyType = SurveyType.stImpactThankyouSurvey) AndAlso _SurveyThankyou Is Nothing) Then 'L0313
            Dim AUsersList As New Dictionary(Of String, clsComparionUser)
            If ddlGroups.SelectedIndex >= App.ActiveProject.ProjectManager.CombinedGroups.GroupsList.Count Then
                Dim AUser As clsUser = App.ActiveProject.ProjectManager.UsersList(ddlGroups.SelectedIndex - App.ActiveProject.ProjectManager.CombinedGroups.GroupsList.Count)
                AUsersList.Add(AUser.UserEMail, New clsComparionUser() With {.ID = AUser.UserID, .UserName = AUser.UserName})
            Else
                Dim AGroup = CType(App.ActiveProject.ProjectManager.CombinedGroups.GroupsList(ddlGroups.SelectedIndex), clsCombinedGroup)
                For Each AUser In AGroup.UsersList
                    AUsersList.Add(AUser.UserEMail, New clsComparionUser() With {.ID = AUser.UserID, .UserName = AUser.UserName})
                Next
            End If

            'For Each AUser In App.ActiveProject.ProjectManager.UsersList
            '    AUsersList.Add(AUser.UserEMail, AUser.UserID)
            'Next
            App.SurveysManager.ActiveUserEmail = ""
            fSurveyInfo = App.SurveysManager.GetSurveyInfoByProjectID(App.ProjectID, ASurveyType, AUsersList)   ' D0379
            If ASurveyType = SurveyType.stWelcomeSurvey Or ASurveyType = SurveyType.stImpactWelcomeSurvey Then
                _SurveyWelcome = fSurveyInfo
            End If
            If ASurveyType = SurveyType.stThankyouSurvey Or ASurveyType = SurveyType.stImpactThankyouSurvey Then
                _SurveyThankyou = fSurveyInfo
            End If
        Else
            If ASurveyType = SurveyType.stWelcomeSurvey Or ASurveyType = SurveyType.stImpactWelcomeSurvey Then
                fSurveyInfo = _SurveyWelcome
            End If
            If ASurveyType = SurveyType.stThankyouSurvey Or ASurveyType = SurveyType.stImpactThankyouSurvey Then
                fSurveyInfo = _SurveyThankyou
            End If
        End If
        Return fSurveyInfo
    End Function

    Protected Sub dsRespondentAnswers_ObjectCreating(sender As Object, e As ObjectDataSourceEventArgs) Handles dsRespondentAnswers.ObjectCreating
        If Not AWelcomeSurvey Is Nothing Then
            e.ObjectInstance = New clsRespondentAnswersDS(AWelcomeSurvey)
        End If
    End Sub

    Protected Sub dsRespondentAnswers2_ObjectCreating(sender As Object, e As ObjectDataSourceEventArgs) Handles dsRespondentAnswers2.ObjectCreating
        If Not AThankyouSurvey Is Nothing Then
            e.ObjectInstance = New clsRespondentAnswersDS(AThankyouSurvey)
        End If
    End Sub

    Protected Sub gvResultOverview_DataBinding(sender As Object, e As EventArgs) Handles gvResultOverview.DataBound, gvResultOverview2.DataBound 'L0058
        Dim i As Integer = 3
        Dim ASurvey As clsSurvey = Nothing
        Dim AGridView As ASPxGridView = Nothing
        Dim ASurveyType As SurveyType
        AGridView = sender
        If sender Is gvResultOverview Then
            ASurvey = AWelcomeSurvey
            If App.ActiveProject.isImpact Then
                ASurveyType = SurveyType.stImpactWelcomeSurvey
            Else
                ASurveyType = SurveyType.stWelcomeSurvey
            End If
        End If
        If sender Is gvResultOverview2 Then
            ASurvey = AThankyouSurvey
            If App.ActiveProject.isImpact Then
                ASurveyType = SurveyType.stImpactThankyouSurvey
            Else
                ASurveyType = SurveyType.stThankyouSurvey
            End If
        End If

        If AGridView.Columns.Count * 150 < 2000 Then
            AGridView.Width = AGridView.Columns.Count * 150 'L0334
        End If

        If ASurvey IsNot Nothing Then
            For Each APage As clsSurveyPage In ASurvey.Pages
                AGridView.Columns(i).HeaderStyle.BackColor = Color.AliceBlue
                AGridView.Columns(i).Caption = APage.Title 'L0448
                i += 1 'L0444
                For Each AQuestion As clsQuestion In APage.Questions
                    If AQuestion.Type <> QuestionType.qtComment And AQuestion.Type <> QuestionType.qtAlternativesSelect And AQuestion.Type <> QuestionType.qtObjectivesSelect Then 'L0067
                        If isMHT(AQuestion.Text) Then
                            AGridView.Columns(i).Caption = HTML2Text(Infodoc_Unpack(ASurveyInfo(ASurveyType).ProjectID, 0, reObjectType.SurveyQuestion, AQuestion.AGUID.ToString, AQuestion.Text, True, True, -1))    ' D2079
                        Else
                            If clsComparionCorePage.OPT_SURVEY_PARSE_LINKS Then AGridView.Columns(i).Caption = ParseTextHyperlinks(AQuestion.Text) Else AGridView.Columns(i).Caption = AQuestion.Text 'L0058 + D4102
                        End If

                        AGridView.Columns(i).HeaderStyle.Wrap = DefaultBoolean.True 'L0334
                        AGridView.Columns(i).HeaderStyle.VerticalAlign = VerticalAlign.Top 'L0334
                        i += 1
                    End If
                Next
            Next
        End If
    End Sub
    'L0042 ==

    'L0069 ===
    Protected Sub tbExport6_ButtonClick(sender As Object, e As RadToolBarEventArgs) Handles tbExport6.ButtonClick, tbExport8.ButtonClick  ' D0542
        Dim FileName As String = ""
        Dim GridExporter As ASPxGridViewExporter = Nothing
        If sender Is tbExport6 Then
            FileName = ASurveyInfo(IIf(App.ActiveProject.isImpact, SurveyType.stImpactWelcomeSurvey, SurveyType.stWelcomeSurvey)).Title + "_Results_Overview"
            GridExporter = ASPxGridViewExporterResultOverview
        End If
        If sender Is tbExport8 Then
            FileName = ASurveyInfo(IIf(App.ActiveProject.isImpact, SurveyType.stImpactThankyouSurvey, SurveyType.stThankyouSurvey)).Title + "_Results_Overview"
            GridExporter = ASPxGridViewExporterResultOverview2
        End If

        App.DBSaveLog(dbActionType.actReportStart, dbObjectType.einfProjectReport, App.ProjectID, "Ad-Hoc " + FileName, String.Format("{0}, {1}", CType(e.Item, RadToolBarButton).CommandName, ddlGroups.SelectedValue)) ' D2237
        Select Case CType(e.Item, RadToolBarButton).CommandName  ' D0542
            Case "ExporttoRTF"
                GridExporter.WriteRtfToResponse(FileName, True)
            Case "ExporttoPDF"
                GridExporter.WritePdfToResponse(FileName, True)
            Case "ExporttoXLS"
                GridExporter.WriteXlsToResponse(FileName, True)
        End Select
    End Sub
    'L0069 ==

    'L0071 ===
    Protected Sub dsStatisticResults_ObjectCreating(sender As Object, e As ObjectDataSourceEventArgs) Handles dsStatisticResults.ObjectCreating
        If Not AWelcomeSurvey Is Nothing Then
            e.ObjectInstance = New clsStatisticResultsDS(AWelcomeSurvey)
        End If
    End Sub
    Protected Sub dsStatisticResults2_ObjectCreating(sender As Object, e As ObjectDataSourceEventArgs) Handles dsStatisticResults2.ObjectCreating
        If Not AThankyouSurvey Is Nothing Then
            e.ObjectInstance = New clsStatisticResultsDS(AThankyouSurvey)
        End If
    End Sub

    Protected Sub gvStatisticResults_CustomColumnDisplayText(sender As Object, e As ASPxGridViewColumnDisplayTextEventArgs) Handles gvStatisticResults.CustomColumnDisplayText
        If e.Column.FieldName = "Question" Then
            If isMHT(e.Value) Then
                Dim AGUID As String = gvStatisticResults.GetDataRow(e.VisibleRowIndex).Item("QGUID")
                e.DisplayText = CutHTMLHeaders(HTML2Text(Infodoc_Unpack(ASurveyInfo(IIf(App.ActiveProject.isImpact, SurveyType.stImpactWelcomeSurvey, SurveyType.stWelcomeSurvey)).ProjectID, 0, reObjectType.SurveyQuestion, AGUID, CStr(e.Value), True, True, -1)))  ' D2079 + D6727
            Else
                If clsComparionCorePage.OPT_SURVEY_PARSE_LINKS Then e.DisplayText = ParseTextHyperlinks(e.Value) ' D4102
            End If
        End If
    End Sub

    Protected Sub gvStatisticResults2_CustomColumnDisplayText(sender As Object, e As ASPxGridViewColumnDisplayTextEventArgs) Handles gvStatisticResults2.CustomColumnDisplayText
        If e.Column.FieldName = "Question" Then
            If isMHT(CStr(e.Value)) Then
                Dim AGUID As String = gvStatisticResults2.GetDataRow(e.VisibleRowIndex).Item("QGUID")
                e.DisplayText = CutHTMLHeaders(HTML2Text(Infodoc_Unpack(ASurveyInfo(IIf(App.ActiveProject.isImpact, SurveyType.stImpactThankyouSurvey, SurveyType.stThankyouSurvey)).ProjectID, 0, reObjectType.SurveyQuestion, AGUID, CStr(e.Value), True, True, -1))) ' D2079 + D6727
            Else
                If clsComparionCorePage.OPT_SURVEY_PARSE_LINKS Then e.DisplayText = ParseTextHyperlinks(e.Value) ' D4102
            End If
        End If
    End Sub

    Protected Sub gvStatisticResults_DataBound(sender As Object, e As EventArgs) Handles gvStatisticResults.DataBound, gvStatisticResults2.DataBound
        App.DBSaveLog(dbActionType.actReportStart, dbObjectType.einfProjectReport, App.ProjectID, "Survey Results", ddlGroups.SelectedValue)  ' D2237
        Dim AGridView As ASPxGridView = sender
        AGridView.ExpandAll()
        AGridView.Width = 800
    End Sub

    Protected Sub tbExport7_ButtonClick(sender As Object, e As RadToolBarEventArgs) Handles tbExport7.ButtonClick, tbExport9.ButtonClick
        Dim FileName As String = ""
        Dim GridExporter As ASPxGridViewExporter = Nothing
        If sender Is tbExport7 Then
            FileName = ASurveyInfo(IIf(App.ActiveProject.isImpact, SurveyType.stImpactWelcomeSurvey, SurveyType.stWelcomeSurvey)).Title + "_Statistic_Results"
            GridExporter = ASPxGridViewExporterStatisticResults
        End If
        If sender Is tbExport9 Then
            FileName = ASurveyInfo(IIf(App.ActiveProject.isImpact, SurveyType.stImpactThankyouSurvey, SurveyType.stThankyouSurvey)).Title + "_Statistic_Results"
            GridExporter = ASPxGridViewExporterStatisticResults2
        End If
        App.DBSaveLog(dbActionType.actReportStart, dbObjectType.einfProjectReport, App.ProjectID, "Ad-Hoc " + FileName, String.Format("{0}, {1}", CType(e.Item, RadToolBarButton).CommandName, ddlGroups.SelectedValue)) ' D2237
        Select Case CType(e.Item, RadToolBarButton).CommandName  ' D0542
            Case "ExporttoRTF"
                GridExporter.WriteRtfToResponse(FileName, True)
            Case "ExporttoPDF"
                GridExporter.WritePdfToResponse(FileName, True)
            Case "ExporttoXLS"
                GridExporter.WriteXlsToResponse(FileName, True)
        End Select
    End Sub
    'L0071 ==
    'SUrvey results ==

    Protected Sub PanelWelcomeSurvey_Load(sender As Object, e As EventArgs) Handles PanelWelcomeSurvey.Load, PanelThankyouSurvey.Load
        If AWelcomeSurvey IsNot Nothing Then
            PanelWelcomeSurvey.Visible = True
            lblWSurveyTitle.Text = ASurveyInfo(IIf(App.ActiveProject.isImpact, SurveyType.stImpactWelcomeSurvey, SurveyType.stWelcomeSurvey)).Title
        Else
            lblNoWelcomeSurvey.Text = "No Welcome Survey in this project"
        End If
        If AThankyouSurvey IsNot Nothing Then
            PanelThankyouSurvey.Visible = True
            lblTSurveyTitle.Text = ASurveyInfo(IIf(App.ActiveProject.isImpact, SurveyType.stImpactThankyouSurvey, SurveyType.stThankyouSurvey)).Title
        Else
            lblNoThankyouSurvey.Text = "No Thank you Survey in this project"
        End If
    End Sub

    ' D4102 ===
    Protected Sub gvResultOverview_HtmlDataCellPrepared(sender As Object, e As ASPxGridViewTableDataCellEventArgs) Handles gvResultOverview.HtmlDataCellPrepared, gvResultOverview2.HtmlDataCellPrepared
        If clsComparionCorePage.OPT_SURVEY_PARSE_LINKS AndAlso TypeOf (e.CellValue) Is String AndAlso Not String.IsNullOrEmpty(e.CellValue) Then
            Dim sVal As String = ParseTextHyperlinks(e.CellValue)
            If sVal <> e.CellValue Then e.Cell.Text = ParseTextHyperlinks(SafeFormString(e.CellValue))
        End If
    End Sub
    ' D4102 ==

    ' D2236 ===
    Protected Sub ASPxGridView_PreRender(sender As Object, e As EventArgs) Handles ASPxGridView1.PreRender, ASPxGridView3.PreRender, ASPxGridView4.PreRender, ASPxGridView5.PreRender, ASPxPivotGrid1.PreRender, gvResultOverview.PreRender, gvStatisticResults.PreRender
        App.DBSaveLog(dbActionType.actReportEnd, dbObjectType.einfProjectReport, App.ProjectID, "", "")
    End Sub
    ' D2236 ==

    ' D4872 ===
    Private Sub CustomResultsPage_Error(sender As Object, e As EventArgs) Handles Me.[Error]
        Response.Redirect(PageURL(_PGID_SERVICEPAGE, "?action=msg&type=no_report"), True)   ' D7170
    End Sub
    ' D4872 ==

End Class
