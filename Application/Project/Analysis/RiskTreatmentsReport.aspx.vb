Option Strict On

Imports System.Data
Imports Microsoft.Reporting.WebForms

Partial Class RiskTreatmentsReportPage
    Inherits clsComparionCorePage

    Public Enum RiskTreatmentsReportIDs As Integer
        rrAll = 0
        rrCauses = 1
        rrVulnerabilities = 2
        rrConseqences = 3
    End Enum

    Private ReportPageIDs() As Integer = {_PGID_RISK_TREATMENTS_REPORT_CAUSES, _PGID_RISK_TREATMENTS_REPORT_VULNERABILITIES, _PGID_RISK_TREATMENTS_REPORT_VULNERABILITIES}
    Private ReportTitles() As String = {"titleRiskTreatmentsReportCauses", "titleRiskTreatmentsReportVulnerabilities", "titleRiskTreatmentsReportConsequences"}
    Public ActiveReportID As RiskTreatmentsReportIDs = RiskTreatmentsReportIDs.rrCauses

    'datasets columns:
    Const IDX_ID As Integer = 0
    Const IDX_TREATMENT As Integer = 1
    Const IDX_TREATMENT_DESCRIPTION As Integer = 2
    Const IDX_NODE As Integer = 3
    Const IDX_EVENT As Integer = 4

    Public Sub New()
        MyBase.New(_PGID_RISK_TREATMENTS_REPORT_CAUSES)
    End Sub

    Protected Sub Page_PreLoad(sender As Object, e As EventArgs) Handles Me.PreLoad
        If Not IsPostBack AndAlso Not IsCallback Then
            pnlLoadingPanel.Caption = ResString("msgLoading")
            pnlLoadingPanel.Message = String.Format("<div style='margin:1em'><img src={1} alt='{0}' border=0/></div>&nbsp;&nbsp;{0}", ResString("lblPleaseWait"), ImagePath + "process.gif")
        End If
    End Sub

    Protected Sub Page_Init(sender As Object, e As EventArgs) Handles Me.Init
        ActiveReportID = CType(CInt(CheckVar("mode", 1)), RiskTreatmentsReportIDs)
        If ActiveReportID < 0 OrElse ActiveReportID > CInt(RiskTreatmentsReportIDs.rrConseqences) Then ActiveReportID = RiskTreatmentsReportIDs.rrCauses
        Dim sAction = EcSanitizer.GetSafeHtmlFragment(CheckVar(_PARAM_ACTION, "")).Trim.ToLower   ' Anti-XSS
        Select Case sAction
            Case "mode"
                'Response.Redirect(PageURL(CurrentPageID, GetTempThemeURI(False)) + String.Format("&mode={0}", CInt(ActiveReportID)), True)
        End Select
        If Not isCallback AndAlso Not IsPostBack Then
            CurrentPageID = ReportPageIDs(ActiveReportID - 1)
            GenerateReport(ActiveReportID)
        End If
    End Sub

    Public Function GetTreatments() As List(Of clsControl)
        Dim res As New List(Of clsControl)
        With App.ActiveProject.ProjectManager
            .Controls.ReadControls(ECModelStorageType.mstCanvasStreamDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID)

            For Each user As clsUser In .UsersList
                .StorageManager.Reader.LoadUserControlsPermissions(user.UserID)
            Next

            Dim LJT As DateTime
            .StorageManager.Reader.LoadUserJudgmentsControls(LJT)
            For Each control As clsControl In .Controls.Controls
                For Each assignment As clsControlAssignment In control.Assignments
                    assignment.Value = .Controls.GetCombinedEffectivenessValue(assignment.Judgments, control.ID, assignment.Value)
                Next
            Next

            For Each control As clsControl In .Controls.Controls
                res.Add(control)
            Next
        End With

        Return res
    End Function

    Private Sub GenerateReport(reportID As RiskTreatmentsReportIDs)
        Dim reportFileName As String = "RiskTreatmentsReportCauses.rdlc"
        Select Case reportID
            Case RiskTreatmentsReportIDs.rrCauses
                reportFileName = "RiskTreatmentsReportCauses.rdlc"
            Case RiskTreatmentsReportIDs.rrVulnerabilities, RiskTreatmentsReportIDs.rrConseqences
                reportFileName = "RiskTreatmentsReportVulnerabilitiesConsequences.rdlc"
            Case RiskTreatmentsReportIDs.rrAll
                ' not implemented
        End Select
        SetPageTitle(ResString(ReportTitles(CInt(reportID) - 1)))
        ReportViewerMain.Reset()
        ReportViewerMain.ProcessingMode = ProcessingMode.Local
        ReportViewerMain.LocalReport.ReportPath = Server.MapPath("~/Project/Analysis/Reports/" + reportFileName)
        ReportViewerMain.LocalReport.DataSources.Clear()
        GetReportData(reportID)
        ReportViewerMain.DataBind()
        ReportViewerMain.LocalReport.Refresh()
        ReportViewerMain.ShowExportControls = App.isExportAvailable
        ReportViewerMain.ShowPrintButton = App.isExportAvailable
    End Sub

    Private Sub GetReportData(reportID As RiskTreatmentsReportIDs)
        Dim ds As New DataSet
        Dim dt As DataTable
        Dim dr As DataRow

        dt = New DataTable() With {.TableName = "ds_treatments"}
        ds.Tables.Add(dt)

        For Each t As DataTable In ds.Tables            
            t.Columns.Add(New DataColumn("ID", GetType(String)))
            t.Columns.Add(New DataColumn("Treatment", GetType(String)))
            t.Columns.Add(New DataColumn("TreatmentDescription", GetType(String)))
            t.Columns.Add(New DataColumn("Node", GetType(String)))
            t.Columns.Add(New DataColumn("Event", GetType(String)))
        Next

        Dim allTreatments = GetTreatments()

        Select Case reportID
            Case RiskTreatmentsReportIDs.rrCauses
                Dim H As clsHierarchy = App.ActiveProject.ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood)
                Dim Treatments As IEnumerable(Of clsControl) = allTreatments.Where(Function(t) t.Type = ControlType.ctCause).OrderBy(Function(t) t.Name)
                For Each treatment As clsControl In Treatments
                    For Each assignment In treatment.Assignments
                        Dim tSource As clsNode = H.GetNodeByID(assignment.ObjectiveID)
                        If tSource IsNot Nothing Then
                            dr = dt.NewRow()
                            dr(IDX_TREATMENT) = treatment.Name
                            'dr(IDX_TREATMENT_DESCRIPTION) = treatment.InfoDoc
                            dr(IDX_TREATMENT_DESCRIPTION) = HTML2Text(GetControlInfodoc(App.ActiveProject, treatment, False))   ' D4345
                            dr(IDX_NODE) = tSource.NodeName
                            dt.Rows.Add(dr)
                        End If
                    Next                    
                Next
            Case RiskTreatmentsReportIDs.rrVulnerabilities, RiskTreatmentsReportIDs.rrConseqences
                Dim H As clsHierarchy = App.ActiveProject.ProjectManager.Hierarchy(CType(IIf(reportID = RiskTreatmentsReportIDs.rrVulnerabilities, ECHierarchyID.hidLikelihood, ECHierarchyID.hidImpact), ECHierarchyID))
                Dim AH As clsHierarchy = App.ActiveProject.ProjectManager.AltsHierarchy(App.ActiveProject.ProjectManager.ActiveAltsHierarchy)
                Dim tControlType As ControlType = CType(IIf(reportID = RiskTreatmentsReportIDs.rrVulnerabilities, ControlType.ctCauseToEvent, ControlType.ctConsequenceToEvent), ControlType)
                Dim Treatments As IEnumerable(Of clsControl) = allTreatments.Where(Function(t) t.Type = tControlType).OrderBy(Function(t) t.Name)
                For Each treatment As clsControl In Treatments
                    For Each assignment In treatment.Assignments
                        Dim tSource As clsNode = H.GetNodeByID(assignment.ObjectiveID)
                        Dim tEvent As clsNode = AH.GetNodeByID(assignment.EventID)
                        If tSource IsNot Nothing AndAlso tEvent IsNot Nothing Then
                            dr = dt.NewRow()
                            dr(IDX_TREATMENT) = treatment.Name
                            'dr(IDX_TREATMENT_DESCRIPTION) = treatment.InfoDoc
                            dr(IDX_TREATMENT_DESCRIPTION) = HTML2Text(GetControlInfodoc(App.ActiveProject, treatment, False))   ' D4345
                            dr(IDX_NODE) = tSource.NodeName
                            dr(IDX_EVENT) = tEvent.NodeName
                            dt.Rows.Add(dr)
                        End If
                    Next
                Next
        End Select

        ' assign data sources
        For Each t As DataTable In ds.Tables
            ReportViewerMain.LocalReport.DataSources.Add(New ReportDataSource(t.TableName, t))
        Next

        ' init parameters        
        Dim p0 As ReportParameter = Nothing
        Dim p1 As ReportParameter = Nothing
        Dim p2 As ReportParameter = Nothing
        Select Case reportID            
            Case RiskTreatmentsReportIDs.rrCauses
                p0 = New ReportParameter("col0title", ParseString("%%Risk%% %%Objective(l)%%"))
                p1 = New ReportParameter("col1title", ParseString("%%Objective(l)%% %%Control%%"))
                p2 = New ReportParameter("col2title", ParseString(""))
            Case RiskTreatmentsReportIDs.rrVulnerabilities
                p0 = New ReportParameter("col0title", ParseString("Vulnerability %%Control%%"))
                p1 = New ReportParameter("col1title", ParseString("%%Risk%% %%Objective(l)%%"))
                p2 = New ReportParameter("col2title", ParseString("%%Risk%% %%Event%%"))
            Case RiskTreatmentsReportIDs.rrConseqences
                p0 = New ReportParameter("col0title", ParseString("%%Objective(i)%%/Consequence %%Control%%"))
                p1 = New ReportParameter("col1title", ParseString("%%Objective(i)%%"))
                p2 = New ReportParameter("col2title", ParseString("%%Risk%% %%Event%%"))
            Case RiskTreatmentsReportIDs.rrAll
                ' not implemented
        End Select

        Dim params() As ReportParameter = New ReportParameter(2) {p0, p1, p2}
        ReportViewerMain.LocalReport.SetParameters(params)
    End Sub

    Protected Sub btnPrint_Click(sender As Object, e As EventArgs) Handles btnPrint.Click
        Dim rp As ReportPrintDocument = New ReportPrintDocument(ReportViewerMain.LocalReport)
        rp.Print()
    End Sub

End Class