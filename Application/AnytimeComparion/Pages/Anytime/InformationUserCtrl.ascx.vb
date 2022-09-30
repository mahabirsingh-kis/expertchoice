Imports System.Runtime.InteropServices
Imports System.Web.Services

Public Class InformationUserCtrl
    Inherits System.Web.UI.UserControl

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim App = CType(Session("App"), clsComparionCore)

        If App.ActiveUser Is Nothing Then
            Response.Redirect("~/login.aspx")
        End If
    End Sub

    Public Sub BindHtml(ByVal informationMessage As String, ByRef NonPWType As String)
        'Dim html As StringBuilder = New StringBuilder()
        'html.Append(informationMessage)
        If NonPWType IsNot Nothing Then
            dvInformation.Attributes("class") = ""
            dvInformation.Attributes("class") += If(NonPWType = "welcome", " welcomewrapper", "thunkupage welcomewrapper")
        End If
        Dim htmlstr As New StringBuilder
        If NonPWType = "welcome" Then
            htmlstr.Append("<div id='DivEditorIcon' class='heading_content justify-content-between'><div class='heading_info'><a href='javascript:void(0);' onclick='ShowWelcomPopup()'> <img class='chkEvaluatorone' src='../../img/icon/edit-icon.svg'></a></div></div>")
        End If
        htmlstr.Append(informationMessage)
        dvInformation.InnerHtml = htmlstr.ToString()
    End Sub
    Public Shared Function GetInformationPageData(ByRef StepNode As clsNode, ByRef AnytimeAction As clsAction, ByRef App As clsComparionCore, ByVal CurrentStep As Integer, <Out> ByRef pipeHelpUrl As String, <Out> ByRef qh_help_id As ecEvaluationStepType, <Out> ByRef InformationMessage As String, <Out> ByRef NonPWType As String, ByVal isUnassessed As Boolean) As Object
        qh_help_id = New PipeParameters.ecEvaluationStepType()
        InformationMessage = ""

        StepNode = Nothing
        Dim data As clsInformationPageActionData = CType(AnytimeAction.ActionData, clsInformationPageActionData)
        Dim isReward = False
        If data.Description.ToLower() = "welcome" Then
            NonPWType = "welcome"
            pipeHelpUrl = TeamTimeClass.ResString("help_pipe_welcome")
            qh_help_id = Canvas.PipeParameters.ecEvaluationStepType.Welcome
            InformationMessage = App.ActiveProject.PipeParameters.PipeMessages.GetWelcomeText(Canvas.PipeParameters.PipeMessageKind.pmkText, App.ActiveProject.ProjectManager.ActiveHierarchy, App.ActiveProject.ProjectManager.ActiveAltsHierarchy)
            If InformationMessage = "" Then
                InformationMessage = TeamTimeClass.ParseAllTemplates(FileService.File_GetContent(GeckoClass.GetWelcomeThankYouIncFile(False, App.ActiveProject.isImpact, App.ActiveProject.PipeParameters.ProjectType = CanvasTypes.ProjectType.ptOpportunities)), App.ActiveUser, App.ActiveProject)
            Else
                InformationMessage = InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, ExpertChoice.Data.Consts.reObjectType.PipeMessage, "welcome", InformationMessage, False, True, -1)
            End If
        Else
            NonPWType = "thankyou"
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
        If data.Description.ToLower() = "welcome" Then
            InformationMessage = InformationMessage.Replace("../../App_Themes/ec09/images/help/help.gif", "../../img/icon/helpCircle_blue.svg")
            InformationMessage = InformationMessage.Replace("'Next'", "<button class='welcome_next'disabled><span class='d-none d-lg-inline-block me-1'>Next </span><img src='../../img/icon/Next.svg' class='icon'></button>")
            InformationMessage = InformationMessage.Replace("near the top right of the screen", "on the right of the screen")
        Else
            If isUnassessed Then
                Dim str As String
                Dim strArr() As String
                str = InformationMessage.ToString().Replace("<h2>", "b")
                str = str.Replace("</h2>", "b")

                strArr = str.Split(Convert.ToChar("b"))
                If strArr.Length > 0 Then
                    If strArr(1) = "You are done!" Then
                        InformationMessage = "<img src='../../img/icon/thumup.svg'> " + InformationMessage
                    Else
                        InformationMessage = "<img src='../../img/icon/alert-icon-circle.svg'> " + InformationMessage
                    End If
                Else
                    InformationMessage = "<img src='../../img/icon/alert-icon-circle.svg'> " + InformationMessage
                End If
            Else
                If isReward Then
                    InformationMessage = "<img src='../../img/icon/thumup.svg'> " + InformationMessage
                End If
            End If
        End If
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