Option Strict Off

Imports System.Drawing
Imports DevExpress.Web.ASPxTabControl
Imports DevExpress.XtraPrinting.Drawing
Imports ExpertChoice.Web.Controls
Imports DevExpress.XtraReports.UI

Partial Class ReportView
    Inherits clsComparionCorePage

    ' D0075 ===
    Private TabNames() As String = {"structure", "objectives", "alternatives", "objandalts", "judgments", "judgments_alts", "objpriorities", "overall", "objandaltpriorities", "overallresults"} 'L0188 'L0390 'A0784 + D3703 + A1100 + D4892 + D7043
    Private TabPages() As Integer = { _
                                    _PGID_REPORT_STRUCTURE, _
                                    _PGID_REPORT_OBJECTIVES, _
                                    _PGID_REPORT_ALTERNATIVES, _
                                    _PGID_REPORT_OBJANDALTS, _
                                    _PGID_REPORT_JUDGMENTS, _
                                    _PGID_REPORT_JUDGMENTS_ALTS, _
                                    _PGID_REPORT_OBJPRIORITIES, _
                                    _PGID_REPORT_OVERALLRESULTS, _
                                    _PGID_REPORT_OBJANDALTPRIORITIES, _
                                    _PGID_REPORT_USERSOVERALLRESULTS _
                                    } 'L0188 'L0390 + D3703
    ' D0075 ==

    'L0101 ===
    Private _RepOptions As String = "0111110000" 'L0194 'L0210

    Public Property RepOptions() As String
        Get
            If Session(PrjID + "RepOptions") <> "" Then _RepOptions = Session(PrjID + "RepOptions")
            Return _RepOptions
        End Get
        Set(value As String)
            _RepOptions = value
            Session(PrjID + "RepOptions") = value
        End Set
    End Property
    'L0101 ==

    Public Sub New()
        MyBase.New(_PGID_REPORTS)   ' D0047 +  D0075
    End Sub

    ' D3264 ===
    Private ReadOnly Property PrjID As String
        Get
            Return "Prj" + App.ProjectID.ToString + "_"
        End Get
    End Property
    ' D3264 ==

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        ' D0075 ===
        AlignHorizontalCenter = True
        AlignVerticalCenter = False

        'NavigationPageID = _PGID_REPORTS

        ' D0395 ===
        For i As Integer = 0 To TabPages.Length - 1
            ASPxPageControlReports.TabPages(i).Text = PageMenuItem(TabPages(i))
            ASPxPageControlReports.TabPages(i).Enabled = HasPermission(TabPages(i), App.ActiveProject)  ' D0488
        Next

        Dim sTab As String = EcSanitizer.GetSafeHtmlFragment(CheckVar(_PARAM_TAB, TabNames(0))).ToLower   ' Anti-XSS
        Dim idx As Integer = Array.IndexOf(TabNames, sTab)
        If idx < 0 Then idx = 0

        CurrentPageID = TabPages(idx)
        ASPxPageControlReports.ActiveTabIndex = idx
        ' D0395 ==
        ' D0667===
        'If isSLTheme() Then  ' D0729 + D0766 -D4874
        ASPxPageControlReports.ShowTabs = False
            ASPxPageControlReports.ContentStyle.Border.BorderStyle = BorderStyle.None
            ' D3702 ===
            For i As Integer = 0 To TabPages.Length - 1
                ASPxPageControlReports.TabPages(i).Visible = i = idx
            Next
        ' D3702 ==
        'End If ' -D4874
        ' D0667 ==
        'pnlLoadingNextStep.Caption = ResString("msgLoading")    ' D3702
        'pnlLoadingNextStep.Message = String.Format("<div style='margin:1em'><img src={1} alt='{0}' border=0/></div>&nbsp;&nbsp;{0}", ResString("lblPleaseWait"), ImagePath + "process.gif")  ' D3702
    End Sub

    ' D0395 ===
    Protected Sub ASPxPageControlReports_ActiveTabChanged(source As Object, e As TabControlEventArgs) Handles ASPxPageControlReports.ActiveTabChanged
        Response.Redirect(PageURL(TabPages(ASPxPageControlReports.ActiveTabIndex)), True)
    End Sub

    Private Sub SetRepOption(OptionIndex As Integer, OptionValue As Boolean)
        Dim AChars = RepOptions.ToCharArray()
        If OptionValue Then
            AChars(OptionIndex) = CChar("1")
        Else
            AChars(OptionIndex) = CChar("0")
        End If
        RepOptions = AChars
    End Sub

    Protected Sub ASPxPageControlReports_Load(sender As Object, e As EventArgs) Handles ASPxPageControlReports.Load
        App.DBSaveLog(dbActionType.actReportStart, dbObjectType.einfProjectReport, App.ProjectID, "Predefined Report: " + PageTitle(CurrentPageID), "") ' D2237
        Dim Report As XtraReport = Nothing
        Dim ReportName As String = ""

        Select Case CurrentPageID
            Case _PGID_REPORT_OBJANDALTS '4
                SetRepOption(0, cbFullPath3.Checked)
            Case _PGID_REPORT_OBJECTIVES '2
                SetRepOption(0, cbFullPath2.Checked)
                SetRepOption(4, cbDescription2.Checked)
            Case _PGID_REPORT_ALTERNATIVES '3
                SetRepOption(4, CheckBox2.Checked)
            Case _PGID_REPORT_OBJPRIORITIES '6
                SetRepOption(0, cbFullPath5.Checked)
                SetRepOption(5, cbBars1.Checked)
            Case _PGID_REPORT_OBJANDALTPRIORITIES '8
                SetRepOption(0, cbFullPath6.Checked)
                SetRepOption(1, cbLocal6.Checked)
                SetRepOption(2, cbGlobal6.Checked)
            Case _PGID_REPORT_OVERALLRESULTS '7
                SetRepOption(5, cbBars2.Checked)
        End Select

        If RepOptions(4) = "1" Then
            'L0120 ===
            'If SessVar("RTFParsed" + App.ActiveProject.Passcode) = "" Then 'L0122
                ParseInfodocsMHT2RTF(App.ActiveProject, ReportCommentType.rctTag) 'L0110 'L0119
                SessVar("RTFParsed" + App.ActiveProject.Passcode) = "Yes" 'L0122
            'End If
            'L0120 ==
        Else
            SessVar("RTFParsed" + App.ActiveProject.Passcode) = "" 'L0335
        End If


        Dim DefaultCommentType = App.ActiveProject.ProjectManager.ProjectDataProvider.CommentType 'L0120
        App.ActiveProject.ProjectManager.ProjectDataProvider.CommentType = ReportCommentType.rctTag 'L0120
        Dim R As ctrlReport = Nothing 'L0119
        Dim sTitle As String = PageMenuItem(CurrentPageID)  ' D3134
        Select Case CurrentPageID
            Case _PGID_REPORT_STRUCTURE
                Report = New xrHierarchy(App.ActiveProject.ProjectManager, sTitle, PrepareTask("%%Objectives%%"), PrepareTask("%%Alternatives%%")) 'L0032 + D2463
                ReportName = "xrHierarchy"
                R = CtrlReport1  'L0119
            Case _PGID_REPORT_OBJANDALTS
                Report = New xrHierarchyObjectivesAndAlternatives(App.ActiveProject.ProjectManager, sTitle, RepOptions) 'L0032 'L0101 + D2463
                ReportName = "xrHierarchyObjectivesAndAlternatives"
                R = CtrlReport3  'L0119
            Case _PGID_REPORT_OBJECTIVES
                Report = New xrHierarchyObjectives(App.ActiveProject.ProjectManager, sTitle, RepOptions) 'L0032 'L0101
                ReportName = "xrHierarchyObjectives"
                R = CtrlReport2  'L0119
                'L0193 ==
            Case _PGID_REPORT_ALTERNATIVES
                Report = New xrHierarchyAlternatives(App.ActiveProject.ProjectManager, sTitle, RepOptions)
                ReportName = "xrHierarchyAlternatives"
                R = CtrlReport8
                'L0193 ===
            Case _PGID_REPORT_JUDGMENTS
                Report = New xrJudgementsOverview(App.ActiveProject.ProjectManager, sTitle, CanvasReportType.crtJudgmentsObjs) 'L0032 + D3703
                ReportName = "xrJudgmentsOverview"
                R = CtrlReport4  'L0119
                ' D3703 ===
            Case _PGID_REPORT_JUDGMENTS_ALTS
                Report = New xrJudgementsOverview(App.ActiveProject.ProjectManager, sTitle, CanvasReportType.crtJudgmentsAlts)    ' D3703
                ReportName = "xrJudgmentsOverview"
                R = CtrlReport10
                ' D3703 ==
            Case _PGID_REPORT_OBJPRIORITIES
                ' D2591 ===
                Dim sGlobalPrty As String = ResString("lblReportGlobalPrty")
                If App.isRiskEnabled Then sGlobalPrty = ResString(CStr(IIf(App.ActiveProject.isImpact, "lblReportGlobalPrtyImpact", "lblReportGlobalPrtyLikelihood")))
                Dim sLocalPrty As String = ResString("lblReportLocalPrty")
                If App.isRiskEnabled Then sLocalPrty = ResString(CStr(IIf(App.ActiveProject.isImpact, "lblReportLocalPrtyImpact", "lblReportLocalPrtyLikelihood")))
                Report = New xrObjectivePriorities(App.ActiveProject.ProjectManager, sTitle, RepOptions, PrepareTask("%%Objectives%%"), sLocalPrty, sGlobalPrty) 'L0032 'L0101 + D2475
                ' D2591 ==
                ReportName = "xrObjectivePriorities"
                R = CtrlReport5  'L0119
            Case _PGID_REPORT_OBJANDALTPRIORITIES
                ' D2590 ===
                Dim sPrty As String = ResString("lblPriority")
                If App.isRiskEnabled Then sPrty = ResString(CStr(IIf(App.ActiveProject.isImpact, "lblPriorityImpact", "lblPriorityLikelihood")))
                Report = New xrObjectiveAndAltsPriorities(App.ActiveProject.ProjectManager, sTitle, RepOptions, PrepareTask("%%Objective%%"), PrepareTask("%%Alternative%%"), sPrty) 'L0032 'L0101 + D2463
                ' D2590 ==
                ReportName = "xrObjectiveAndAltsPriorities"
                R = CtrlReport7  'L0119 'L0188
            Case _PGID_REPORT_OVERALLRESULTS
                ' D2590 ===
                Dim sGlobalPrty As String = ResString("lblReportGlobalPrty")
                If App.isRiskEnabled Then sGlobalPrty = ResString(CStr(IIf(App.ActiveProject.isImpact, "lblReportGlobalPrtyImpact", "lblReportGlobalPrtyLikelihood")))
                Report = New xrAltsOverallPriorities(App.ActiveProject.ProjectManager, sTitle, RepOptions, PrepareTask("%%Alternatives%%"), sGlobalPrty) 'L0032 'L0119 + D2463 + D2475
                ' D2590 ==
                ReportName = "xrAltsOverallPriorities"
                R = CtrlReport6  'L0119 'L0188
            Case _PGID_REPORT_USERSOVERALLRESULTS 'L0390
                Report = New xrSLOverallResults(App.ActiveProject.ProjectManager, sTitle, PrepareTask("%%Alternative%%"))   ' D2463
                ReportName = "xrSLOverallResults"
                R = CtrlReport9
        End Select

        'If Not Report Is Nothing AndAlso ReportName <> "" Then
        '    Dim R As ctrlReport = TryCast(ASPxPageControlReports.ActiveTabPage.Controls(0), ctrlReport) 'L0119
        If (R IsNot Nothing) AndAlso (Report IsNot Nothing) AndAlso (ReportName <> "") Then 'L0119

            'L0188 ==

            Dim AWatermark = Image.FromFile(Me.Server.MapPath("Watermark.png"))
            Report.Watermark.ImageViewMode = ImageViewMode.Stretch
            Report.Watermark.ShowBehind = True
            Report.Watermark.Image = AWatermark
            'L0188 ===
            R.ReportViewer.Report = Report
            R.ReportViewer.ReportName = ReportName
            R.ExportEnabled = App.isExportAvailable 'L0379
            R.ReportTitle = sTitle  ' D3134
            R.ReportViewer.DataBind()
        End If
        'End If
        App.ActiveProject.ProjectManager.ProjectDataProvider.CommentType = DefaultCommentType 'L0120
        'App.ActiveProject.ResetProject(True) 'L0110 'L0120
    End Sub
    ' D0395 ==
    'L0335 ===
    Public Sub RestoreCheckboxesStates()
        ExecuteCheckChangedHandler = False
        ' D2463 ===
        cbFullPath2.Text = ResString("optReportShowObjPath")
        cbDescription2.Text = ResString("optReportShowObjDesc")
        CheckBox2.Text = ResString("optReportShowAltDesc")
        cbFullPath3.Text = ResString("optReportShowObjPath")
        cbFullPath5.Text = ResString("optReportShowObjPath")
        cbBars1.Text = ResString("optReportShowHistograms")
        cbBars2.Text = ResString("optReportShowHistograms")
        cbFullPath6.Text = ResString("optReportShowObjPath")
        cbLocal6.Text = ResString("optReportShowLocalPrty")
        If App.isRiskEnabled Then cbLocal6.Text = ResString(CStr(IIf(App.ActiveProject.isImpact, "optReportShowLocalPrtyImpact", "optReportShowLocalPrtyLikelihood"))) ' D2590
        cbGlobal6.Text = ResString("optReportShowGlobalPrty")
        If App.isRiskEnabled Then cbGlobal6.Text = ResString(CStr(IIf(App.ActiveProject.isImpact, "optReportShowGlobalPrtyImpact", "optReportShowGlobalPrtyLikelihood"))) ' D2590
        ' D2463 ==
        If RepOptions(0) = "1" Then
            cbFullPath2.Checked = True
            cbFullPath3.Checked = True
            cbFullPath5.Checked = True
            cbFullPath6.Checked = True
        Else
            cbFullPath2.Checked = False
            cbFullPath3.Checked = False
            cbFullPath5.Checked = False
            cbFullPath6.Checked = False
        End If
        If RepOptions(1) = "1" Then
            cbLocal6.Checked = True
        Else
            cbLocal6.Checked = False
        End If
        If RepOptions(2) = "1" Then
            cbGlobal6.Checked = True
        Else
            cbGlobal6.Checked = False
        End If
        If RepOptions(4) = "1" Then
            cbDescription2.Checked = True
            CheckBox2.Checked = True
        Else
            cbDescription2.Checked = False
            CheckBox2.Checked = False
        End If
        If RepOptions(5) = "1" Then
            cbBars1.Checked = True
            cbBars2.Checked = True
        Else
            cbBars1.Checked = False
            cbBars2.Checked = False
        End If
        ExecuteCheckChangedHandler = True
    End Sub

    Dim ExecuteCheckChangedHandler As Boolean = True
    Dim FirstTimeLoad As Boolean = True
    'L0101 ===
    Protected Sub cbFullPath_CheckedChanged(sender As Object, e As EventArgs) Handles cbFullPath2.CheckedChanged, cbFullPath3.CheckedChanged, cbFullPath5.CheckedChanged, cbFullPath6.CheckedChanged 'L0119
        If ExecuteCheckChangedHandler Then
            Dim AChars = RepOptions.ToCharArray()
            If CType(sender, CheckBox).Checked Then
                AChars(0) = CChar("1")
            Else
                AChars(0) = CChar("0")
            End If
            RepOptions = AChars
            ASPxPageControlReports_Load(sender, e)
            'Response.Redirect(Request.Url.AbsoluteUri, True)
        End If
    End Sub

    Protected Sub cbFullPath_Load(sender As Object, e As EventArgs) Handles cbFullPath2.Init, cbFullPath3.Init, cbFullPath5.Init, cbFullPath6.Init 'L0119
        RestoreCheckboxesStates()
    End Sub
    'L0101 ==
    'L0107 ===
    Protected Sub cbLocalPrty_CheckedChanged(sender As Object, e As EventArgs) Handles cbLocal6.CheckedChanged 'L0119 'L0194
        Dim AChars = RepOptions.ToCharArray()
        If CType(sender, CheckBox).Checked Then
            AChars(1) = CChar("1")
        Else
            AChars(1) = CChar("0")
        End If
        RepOptions = AChars
        ASPxPageControlReports_Load(sender, e)
        'Response.Redirect(Request.Url.AbsoluteUri, True)
    End Sub

    Protected Sub cbLocalPrty_Load(sender As Object, e As EventArgs) Handles cbLocal6.Init 'L0119 'L0194
        RestoreCheckboxesStates()
    End Sub

    Protected Sub cbGlobalPrty_CheckedChanged(sender As Object, e As EventArgs) Handles cbGlobal6.CheckedChanged 'L0119 'L0194
        Dim AChars = RepOptions.ToCharArray()
        If CType(sender, CheckBox).Checked Then
            AChars(2) = CChar("1")
        Else
            AChars(2) = CChar("0")
        End If
        RepOptions = AChars
        ASPxPageControlReports_Load(sender, e)
        'Response.Redirect(Request.Url.AbsoluteUri, True)
    End Sub

    Protected Sub cbGlobalPrty_Load(sender As Object, e As EventArgs) Handles cbGlobal6.Init 'L0119 'L0194
        RestoreCheckboxesStates()
    End Sub

    'Protected Sub cbAltPrty_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles cbAltPrty6.CheckedChanged 'L0119
    '    Dim AChars = RepOptions.ToCharArray()
    '    If CType(sender, CheckBox).Checked Then
    '        AChars(3) = "1"
    '    Else
    '        AChars(3) = "0"
    '    End If
    '    RepOptions = AChars
    '    Response.Redirect(Request.Url.AbsoluteUri, True)
    'End Sub 'L0192

    'Protected Sub cbAltPrty_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles cbAltPrty6.Load 'L0119
    '    If Not IsPostBack Then 'L0119
    '        If RepOptions(3) = "1" Then
    '            CType(sender, CheckBox).Checked = True
    '        End If
    '    End If
    'End Sub 'L0192
    'L0107 ==
    'L0110 ===
    Protected Sub cbDescription_CheckedChanged(sender As Object, e As EventArgs) Handles cbDescription2.CheckedChanged, CheckBox2.CheckedChanged 'L0119 'L0193 cbDescription3.CheckedChanged, cbDescription5.CheckedChanged, cbDescription6.CheckedChanged, cbDescription7.CheckedChanged, 'L0194
        Dim AChars = RepOptions.ToCharArray()
        If CType(sender, CheckBox).Checked Then
            AChars(4) = CChar("1")
        Else
            AChars(4) = CChar("0")
        End If
        RepOptions = AChars
        ASPxPageControlReports_Load(sender, e)
        'Response.Redirect(Request.Url.AbsoluteUri, True)
    End Sub

    Protected Sub cbDescription_Load(sender As Object, e As EventArgs) Handles cbDescription2.Init, CheckBox2.Init 'L0119 'L0193 cbDescription3.Load, cbDescription5.Load, cbDescription6.Load, cbDescription7.Load, 'L0194
        RestoreCheckboxesStates()
    End Sub
    'L0110 ==
    'L0210 ===
    Protected Sub cbBars1_CheckedChanged(sender As Object, e As EventArgs) Handles cbBars1.CheckedChanged, cbBars2.CheckedChanged
        Dim AChars = RepOptions.ToCharArray()
        If CType(sender, CheckBox).Checked Then
            AChars(5) = CChar("1")
        Else
            AChars(5) = CChar("0")
        End If
        RepOptions = AChars
        ASPxPageControlReports_Load(sender, e)
        'Response.Redirect(Request.Url.AbsoluteUri, True)
    End Sub

    Protected Sub cbBars1_Load(sender As Object, e As EventArgs) Handles cbBars1.Init, cbBars2.Init
        RestoreCheckboxesStates()
    End Sub
    'L0210 ==
    'L0335 ==

    ' D2237 ===
    Protected Sub ASPxPageControlReports_PreRender(sender As Object, e As EventArgs) Handles ASPxPageControlReports.PreRender
        App.DBSaveLog(dbActionType.actReportEnd, dbObjectType.einfProjectReport, App.ProjectID, "Predefined Report: " + PageTitle(CurrentPageID), "")
    End Sub
    ' D2237 ==

    ' D4872 ===
    Private Sub ReportView_Error(sender As Object, e As EventArgs) Handles Me.[Error]
        Response.Redirect(PageURL(_PGID_SERVICEPAGE, "?action=msg&type=no_report"), True)   ' D7170
    End Sub
    ' D4872 ==

End Class
