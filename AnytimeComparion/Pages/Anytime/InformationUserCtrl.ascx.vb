Imports System.Runtime.InteropServices
Imports System.Web.Services
Imports Canvas
Imports ECCore
Imports ExpertChoice.Data
Imports ExpertChoice.Service

Public Class InformationUserCtrl
    Inherits System.Web.UI.UserControl

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim App = CType(Session("App"), clsComparionCore)

        If App.ActiveUser Is Nothing Then
            Response.Redirect("~/login.aspx")
        End If
    End Sub

    Public Sub BindHtml(ByVal informationMessage As String)
        Dim html As StringBuilder = New StringBuilder()
        html.Append(informationMessage)
        dvInformation.InnerHtml = html.ToString()
    End Sub
    Public Shared Function GetInformationPageData(ByRef StepNode As clsNode, ByRef AnytimeAction As clsAction, ByRef App As clsComparionCore, ByVal CurrentStep As Integer, <Out> ByRef pipeHelpUrl As String, <Out> ByRef qh_help_id As ecEvaluationStepType, <Out> ByRef InformationMessage As String)
        qh_help_id = New PipeParameters.ecEvaluationStepType()
        InformationMessage = ""

        StepNode = Nothing
        Dim data As clsInformationPageActionData = CType(AnytimeAction.ActionData, clsInformationPageActionData)
        Dim isReward = False
        If data.Description.ToLower() = "welcome" Then
            pipeHelpUrl = TeamTimeClass.ResString("help_pipe_welcome")
            qh_help_id = Canvas.PipeParameters.ecEvaluationStepType.Welcome
            InformationMessage = App.ActiveProject.PipeParameters.PipeMessages.GetWelcomeText(Canvas.PipeParameters.PipeMessageKind.pmkText, App.ActiveProject.ProjectManager.ActiveHierarchy, App.ActiveProject.ProjectManager.ActiveAltsHierarchy)
            If InformationMessage = "" Then
                InformationMessage = TeamTimeClass.ParseAllTemplates(FileService.File_GetContent(GeckoClass.GetWelcomeThankYouIncFile(False, App.ActiveProject.isImpact, App.ActiveProject.PipeParameters.ProjectType = CanvasTypes.ProjectType.ptOpportunities)), App.ActiveUser, App.ActiveProject)
            Else
                InformationMessage = InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, ExpertChoice.Data.Consts.reObjectType.PipeMessage, "welcome", InformationMessage, False, True, -1)
            End If
        Else
            qh_help_id = Canvas.PipeParameters.ecEvaluationStepType.ThankYou
            If (App.ActiveProject.ProjectManager.Parameters.EvalShowRewardThankYou And GetNextUnassessed(CurrentStep) Is Nothing) Then
                isReward = True
            End If
            InformationMessage = App.ActiveProject.PipeParameters.PipeMessages.GetThankYouText(If(isReward, Canvas.PipeParameters.PipeMessageKind.pmkReward, Canvas.PipeParameters.PipeMessageKind.pmkText), App.ActiveProject.ProjectManager.ActiveHierarchy, App.ActiveProject.ProjectManager.ActiveAltsHierarchy)

            If InformationMessage <> "" Then
                InformationMessage = InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, ExpertChoice.Data.Consts.reObjectType.PipeMessage, "thankyou" & (If(isReward, "_reward", "")), InformationMessage, False, True, -1)
            Else
                If isReward Then
                    InformationMessage = TeamTimeClass.ParseAllTemplates(FileService.File_GetContent(GeckoClass.GetIncFile(Consts._FILE_TEMPL_THANKYOU_REWARD)), App.ActiveUser, App.ActiveProject)
                Else
                    InformationMessage = TeamTimeClass.ParseAllTemplates(FileService.File_GetContent(GeckoClass.GetWelcomeThankYouIncFile(True, App.ActiveProject.isImpact, App.ActiveProject.PipeParameters.ProjectType = CanvasTypes.ProjectType.ptOpportunities)), App.ActiveUser, App.ActiveProject)
                End If
            End If
        End If
        InformationMessage = TeamTimeClass.ParseAllTemplates(InformationMessage, App.ActiveUser, App.ActiveProject)
    End Function

    <WebMethod(EnableSession:=True)>
    Private Shared Function GetNextUnassessed(ByVal StartingStep As Integer) As Integer()
        Dim context As HttpContext = HttpContext.Current
        Dim App = CType(context.Session("App"), clsComparionCore)
        Dim returnValue As Integer() = New Integer(1) {}
        If StartingStep < 0 Then StartingStep = 0
        Dim unassessed_step As Integer = StartingStep
        Dim unassessed_count = 0

        For i As Integer = 1 To App.ActiveProject.Pipe.Count - 1

            If AnytimeClass.IsUndefined(AnytimeClass.GetAction(i)) Then
                unassessed_count += 1
                If unassessed_count > 2 Then Exit For
            End If
        Next

        For i As Integer = unassessed_step + 1 To App.ActiveProject.Pipe.Count - 1

            If AnytimeClass.IsUndefined(AnytimeClass.GetAction(i)) Then
                unassessed_step = i
                returnValue(0) = unassessed_step
                returnValue(1) = unassessed_count
                Return returnValue
            End If
        Next

        For i As Integer = 1 To App.ActiveProject.Pipe.Count - 1

            If AnytimeClass.IsUndefined(AnytimeClass.GetAction(i)) Then
                unassessed_step = i
                returnValue(0) = unassessed_step
                returnValue(1) = unassessed_count
                Return returnValue
            End If
        Next

        Return Nothing
    End Function

End Class