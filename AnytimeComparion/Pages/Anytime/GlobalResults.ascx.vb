Imports Canvas
Imports ExpertChoice.Data

Public Class GlobalResults
    Inherits System.Web.UI.UserControl

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    End Sub

    Public Shared Function GetDetails(ByVal stepId As Integer, ByVal CurrentStep As Integer, ByVal App As clsComparionCore) As GlobalResultModel
        Dim globalresultModel As GlobalResultModel = New GlobalResultModel()
        globalresultModel.question = App.ActiveProject.PipeParameters.NameObjectives
        globalresultModel.qh_help_id = ecEvaluationStepType.OverallResults
        Try
            globalresultModel.StepTask = TeamTimeClass.GetPipeStepTask(AnytimeClass.Action(stepId), Nothing, False, False, False)
        Catch
            globalresultModel.StepTask = ""
        End Try

        Dim globalresultsdata = CType(AnytimeClass.GetAction(stepId).ActionData, clsShowGlobalResultsActionData)
        globalresultModel.PipeParameters = AnytimeClass.CreateGlobalResults(CInt(CurrentStep))
        globalresultModel.ParentNodeID = globalresultsdata.WRTNode.NodeID

        Return globalresultModel
    End Function

End Class