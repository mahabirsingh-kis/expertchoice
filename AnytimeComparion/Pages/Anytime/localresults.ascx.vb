Imports Canvas
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Public Class localresults
    Inherits System.Web.UI.UserControl
    Public Shared intermediate_screen As Integer = -1

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load

    End Sub

    Public Shared Function GetDetails(ByVal AnytimeAction As clsAction) As LocalResultModel
        Dim LocalResult As LocalResultModel = New LocalResultModel()
        HttpContext.Current.Session("ObjData") = Nothing
        HttpContext.Current.Session("PairsData") = Nothing
        LocalResult.qh_help_id = ecEvaluationStepType.IntermediateResults
        LocalResult.AnytimeAction = AnytimeAction
        Dim localresultsdata = CType(AnytimeAction.ActionData, clsShowLocalResultsActionData)
        LocalResult.StepNode = localresultsdata.ParentNode
        LocalResult.CurrentStep = Convert.ToInt32(HttpContext.Current.Session("AT_CurrentStep"))

        Try
            LocalResult.StepTask = TeamTimeClass.GetPipeStepTask(AnytimeClass.Action(LocalResult.CurrentStep), Nothing, False, False, False)
        Catch
            LocalResult.StepTask = ""
        End Try

        LocalResult.ParentNodeID = localresultsdata.ParentNode.NodeID
        LocalResult.ParentNodeGUID = localresultsdata.ParentNode.NodeGuidID
        LocalResult.PipeParameters = AnytimeClass.CreateLocalResults(LocalResult.CurrentStep)

        Return LocalResult
    End Function

    Public Sub bindHtml(ByVal PipeParameters As Object, ByVal step_task As String)
        If PipeParameters IsNot Nothing Then
            Dim html As StringBuilder = New StringBuilder()
            Dim dta = JsonConvert.SerializeObject(PipeParameters)
            Dim dta1 = JObject.Parse(dta)

            html.Append("<div class='container-fluid'>")
            html.Append("<div class='row'>")
            Dim individual As Boolean = CType(dta1("canshowresults")("individual").ToString(), Boolean)
            Dim combined As Boolean = CType(dta1("canshowresults")("combined").ToString(), Boolean)
            Dim equalMessage As Boolean = CType(dta1("equalMessage").ToString(), Boolean)
            If (individual Or combined) And check_shown_results(individual, combined, dta1("results_data")(0)(4)) And Not equalMessage Then
                html.Append("<div id='IntermediateResults'>")
                If dta1("showTitle").ToString() = "True" Then
                    html.Append("<div class='col-12'>")
                    html.Append("<div class='page-title-box text-center'>")
                    html.Append($"<p class='font-size-18 prm-color text-center'>{step_task}</p>")
                    html.Append($"<h3 class='prm-color'>{dta1("resultsTitle")}</h3>")
                    html.Append("</div>")
                    html.Append("</div>")
                End If
                html.Append("<div class='col-lg-12' id='divTabResults'>")
                html.Append("<div class='border table-responsive'>")
                html.Append("<table class='table table-striped table-hover mb-0' id='tblLocalResults'>")
                html.Append("<thead>")
                html.Append("<tr>")
                html.Append("<th scope='col' class='prm-color'>No <i class='fas fa-sort ms-2'></i></th>")
                html.Append("<th scope='col' class='prm-color'>Name <i class='fas fa-sort ms-2'></i></th>")
                If (dta1("results_data")(0)(4).ToString() = "rvIndividual" Or dta1("results_data")(0)(4).ToString() = "rvBoth") And individual Then
                    html.Append("<th scope='col' class='prm-color'>Your Results <i class='fas fa-sort ms-2'></i></th>")
                End If
                If (dta1("results_data")(0)(4).ToString() = "rvGroup" Or dta1("results_data")(0)(4).ToString() = "rvBoth") And combined Then
                    html.Append("<th scope='col' class='prm-color'>Combined Results <i class='fas fa-sort ms-2'></i></th>")
                End If
                html.Append("<th scope='col' class='prm-color'>Bar Graph</th>")
                html.Append("</tr>")
                html.Append("</thead>")
                html.Append("<tbody>")
                For i = 0 To dta1("results_data").Count - 1
                    Dim result = dta1("results_data")(i)
                    html.Append("<tr>")
                    html.Append($"<td>{result(0)}</td>")
                    html.Append($"<td>{result(1)}</td>")
                    If (dta1("results_data")(0)(4).ToString() = "rvIndividual" Or dta1("results_data")(0)(4).ToString() = "rvBoth") And individual Then
                        html.Append($"<td>{Math.Round(Convert.ToDecimal(result(2)) * 100, 2)}%</td>")
                    End If
                    If (dta1("results_data")(0)(4).ToString() = "rvGroup" Or dta1("results_data")(0)(4).ToString() = "rvBoth") And combined Then
                        html.Append($"<td class='hl-color'>{Math.Round(Convert.ToDecimal(result(3)) * 100, 2)}%</td>")
                    End If
                    html.Append($"<td>")
                    html.Append("<div class='progress mb-1 bg-transparent' style='height: 10px;'>")
                    html.Append($"<div class='progress-bar bg_blue' role='progressbar' style='width: {Math.Round(Convert.ToDecimal(result(2)) * 100)}%;' aria-valuenow='{Math.Round(Convert.ToDecimal(result(2)) * 100)}' aria-valuemin='0' aria-valuemax='100'></div>")
                    html.Append("</div>")
                    html.Append("<div class='progress mb-1 bg-transparent' style='height: 10px;'>")
                    html.Append($"<div class='progress-bar bg_green' role='progressbar' style='width: {Math.Round(Convert.ToDecimal(result(3)) * 100)}%;' aria-valuenow='{Math.Round(Convert.ToDecimal(result(3)) * 100)}' aria-valuemin='0' aria-valuemax='100'></div>")
                    html.Append("</div>")
                    html.Append("</td>")
                    html.Append("</tr>")
                Next
                html.Append("</tbody>")
                html.Append("</table>")
                html.Append("</div>")
                If dta1("InconsistencyRatioStatus").ToString() = "True" And (dta1("JudgmentType").ToString() = "atAllPairwise" Or dta1("JudgmentType").ToString() = "atPairwise") Then
                    html.Append($"<h4 class='mb-4 mt-3 prm-color text-center'>Inconsistency Ratio: <span>{Math.Round(Convert.ToDecimal(dta1("InconsistencyRatio")), 2)}</span> </h4>")
                End If
                html.Append("<div class='text-center'>")
                html.Append("<button class='btn btn-clipboard' type='button' onclick=bindDynamicHtml('show_satisfactory')> Click here if these priorities or the Inconsistency are not satisfactory</button>")
                html.Append("</div>")
                html.Append("</div>")
                html.Append("</div>")
                html.Append("<div id='divDynamicResults'></div>")
            End If
            html.Append("</div>")
            html.Append("</div>")
            divContent.InnerHtml = html.ToString()
        End If
    End Sub

    Protected Function check_shown_results(ByVal individual As Boolean, ByVal combined As Boolean, ByVal results_data As String) As Boolean
        If results_data.ToString() = "rvIndividual" And Not individual Then
            Return False
        ElseIf results_data.ToString() = "rvGroup" And Not combined Then
            Return False
        ElseIf results_data.ToString() = "rvGroup" And Not individual And Not combined Then
            Return False
        Else
            Return True
        End If
    End Function

End Class