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

    Public Sub bindHtml(ByVal PipeParameters As Object, ByVal step_task As String, ByVal current_step As Integer, ByVal question As String,
                        ByVal nameAlternatives As String, ByVal cluster_phrase As String, ByVal isPipeViewOnly As Boolean)
        If PipeParameters IsNot Nothing And PipeParameters.ToString() <> "False" Then
            Dim html As StringBuilder = New StringBuilder()
            Dim dta = JsonConvert.SerializeObject(PipeParameters)
            Dim dta1 = JObject.Parse(dta)
            Dim individual As Boolean = CType(dta1("canshowresult")("individual").ToString(), Boolean)
            Dim combined As Boolean = CType(dta1("canshowresult")("combined").ToString(), Boolean)
            Dim showNormalization As Boolean = CType(dta1("showNormalization").ToString(), Boolean)
            Dim ShowBars As Boolean = CType(dta1("ShowBars").ToString(), Boolean)

            Dim maxCombinedResult As Decimal = 0, loopCount As Integer = 0
            Dim hdnCombinedResult As Decimal = 0, hdnindividualResult As Decimal = 0
            Dim clsdnone As String = ""
            If ShowBars <> True Then
                clsdnone = " d-none"
            End If

            If dta1("results_data") IsNot Nothing Then
                For i = 0 To dta1("results_data").Count - 1
                    If loopCount = 0 Then
                        If dta1("results_data")(0)(4).ToString() = "rvBoth" Then
                            hdnCombinedResult = If(dta1("results_data")(i)(3).ToString() <> "" And dta1("results_data")(i)(3).ToString() <> "NaN", Convert.ToDecimal(dta1("results_data")(i)(3)), 0)
                            hdnindividualResult = If(dta1("results_data")(i)(2).ToString() <> "" And dta1("results_data")(i)(2).ToString() <> "NaN", Convert.ToDecimal(dta1("results_data")(i)(2)), 0)
                            maxCombinedResult = If(hdnCombinedResult > hdnindividualResult, hdnCombinedResult, hdnindividualResult)
                        ElseIf (dta1("results_data")(0)(4).ToString() = "rvGroup") And combined Then
                            maxCombinedResult = If(dta1("results_data")(i)(3).ToString() <> "" And dta1("results_data")(i)(3).ToString() <> "NaN", Convert.ToDecimal(dta1("results_data")(i)(3)), 0)
                        Else
                            maxCombinedResult = If(dta1("results_data")(i)(2).ToString() <> "" And dta1("results_data")(i)(2).ToString() <> "NaN", Convert.ToDecimal(dta1("results_data")(i)(2)), 0)
                        End If
                    Else
                        If dta1("results_data")(0)(4).ToString() = "rvBoth" Then
                            If dta1("results_data")(i)(3).ToString() <> "" AndAlso dta1("results_data")(i)(3).ToString() <> "NaN" AndAlso maxCombinedResult < Convert.ToDecimal(dta1("results_data")(i)(3)) Then
                                maxCombinedResult = Convert.ToDecimal(dta1("results_data")(i)(3))
                            End If
                            If dta1("results_data")(i)(2).ToString() <> "" AndAlso dta1("results_data")(i)(2).ToString() <> "NaN" AndAlso maxCombinedResult < Decimal.Parse((dta1("results_data")(i)(2).ToString()), System.Globalization.NumberStyles.Any) Then
                                maxCombinedResult = Convert.ToDecimal(dta1("results_data")(i)(2))
                            End If
                        ElseIf (dta1("results_data")(0)(4).ToString() = "rvGroup" Or dta1("results_data")(0)(4).ToString() = "rvBoth") And combined Then
                            If dta1("results_data")(i)(3).ToString() <> "" AndAlso dta1("results_data")(i)(3).ToString() <> "NaN" AndAlso maxCombinedResult < Convert.ToDecimal(dta1("results_data")(i)(3)) Then
                                maxCombinedResult = Convert.ToDecimal(dta1("results_data")(i)(3))
                            End If
                        Else
                            If dta1("results_data")(i)(2).ToString() <> "" AndAlso dta1("results_data")(i)(2).ToString() <> "NaN" AndAlso maxCombinedResult < Decimal.Parse((dta1("results_data")(i)(2).ToString()), System.Globalization.NumberStyles.Any) Then
                                maxCombinedResult = Convert.ToDecimal(dta1("results_data")(i)(2))
                            End If
                        End If
                    End If
                    loopCount = loopCount + 1
                Next
            End If

            'html.Append("<div class='main_wrapper'>")
            Dim screenCheck As ScreenCheck = New ScreenCheck()
            Dim model_text As String = HttpUtility.UrlEncode(cluster_phrase.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))

            html.Append("<div class='page_heading_section'>")
            html.Append("<div class='container'>")
            html.Append("<div class='row'>")
            html.Append("<div class='col-md-6'>")
            html.Append("<div class='heading_content'>")
            html.Append("<div class='page-title-box' id='titleDiv'><div class='heading_icons'>")
            If Not (screenCheck.isMobileBrowserClient(Request)) Then
                html.Append($"<a{If(isPipeViewOnly, " class='d-none'", "")} href='#'> <img class='chkEvaluatorone' src = '../../img/icon/edit-icon.svg' onclick=showHeaderPopup(decodeURIComponent({ChrW(&H22) + model_text + ChrW(&H22)}),'0','0','0','0',null,'question-node',decodeURIComponent({ChrW(&H22) + model_text + ChrW(&H22)}))></a>")
            End If
            html.Append("<asp:Label ID='lblTask' runat='server' ><span id='divT2S' style='font-size:20px; margin-left:1ex border: 0px !important; background: #f0f0ff08 !important; display:none'></span></asp:Label></div>")
            html.Append($"<div class='txtStepTask' id='MainHeaderInfodoc'>{step_task}<span class='threedoticon d-none' id='btnheadinfo'> <a href='#' onclick='showheaderpopup()' ><div class='snippet'><div class='stage'><div class='dot-falling'></div> </div></div></a></span></div>")
            html.Append("<asp:Label ID='lblTask' runat='server'><span class='123' id='divT2S' style=' margin-left:1ex border: 0px !important; background: #f0f0ff08 !important; display:none'></span></asp:label>")
            'html.Append("<div class='heading_info'>")
            'html.Append("<img src='../../img/icon/empty_info.svg' class='d-none'>")
            'html.Append("<img src='../../img/icon/info_data.svg'>")
            'html.Append("<img src='../../img/icon/info-close.svg' class='d-none'>")

            'html.Append("</div>")
            html.Append("</div>")


            html.Append("</div>")
            html.Append("</div>")

            'html.Append("<div class='col-md-6'>")
            'html.Append("<div class='info_content'>")
            'html.Append("<div class='info_content_wrapper'>")
            'html.Append("<div class='info_text'>")
            'html.Append("<p>Lorem ipsum dolor sit amet, consectetur adipi scing elit, sed do eius mod elitelit tempor incididunt ut labore et dolore magna aliqua. Quis ipsum suspen dfef tyu tempor incididunt ut labore et dolore magna aliqua. Quis ipsum suspen dfef tyu ultrices gravida. Risus commodo viverra maecenas accumsan lacus</p>")
            'html.Append("</div>")
            'html.Append("<div class='info_box_icons'>")
            'html.Append("<a href='javascript:void(0);' class='editsvg_icon'><img class='chkEvaluatorone' src='../../img/icon/edit-icon.svg'></a>")
            'html.Append("<a href='javascript:void(0);' data-bs-toggle='modal' data-bs-target='#exampleModal'><img src='../../img/icon/full-screen-svgrepo-com.svg'></a>")
            'html.Append("</div>")

            'html.Append("</div>")
            'html.Append("</div>")
            'html.Append("</div>")

            html.Append("</div>")
            html.Append("</div>")
            html.Append("</div>")


            html.Append("<div class='container'>")
            html.Append("<div class='row'>")
            If showNormalization Then
                html.Append("<div Class='col-lg-12'>")
                html.Append("<div Class='row'>")
                html.Append("<div Class='col-lg-12 text-end d-lg-block d-none normalization_dropdown'>")
                html.Append("<Label class='mb-0'>Normalization : </label>")
                html.Append("<Select id='ddlGlobalNormalization' onchange=onSortNormalizationChange('ddlGlobalNormalization') Class='form-select mb-3' aria-label='Default select example'>")
                html.Append("<option value='1'>Normalized</Option>")
                html.Append("<option value='2'>% of Maximum</Option>")
                html.Append("<option value='3'>Unnormalized</Option>")
                html.Append("</select>")
                html.Append("</div>")
                html.Append("</div>")
                html.Append("</div>")
            End If
            html.Append("<div class='objectives_main d-none d-lg-block'>")
            html.Append("<div class=''>")
            html.Append("<div class='row'>")
            html.Append("<div class='col-lg-4'>")
            html.Append("<div class='objective_section'>")
            html.Append($"<h2 class='objective_heading'>{If(question IsNot Nothing, Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(question.ToLower()), "")}</h2>")

            'html.Append("<div class='accordion' id='accordionExample'>")
            'html.Append("<div class='accordion-item'>")
            'html.Append("<h2 class='accordion-header px-2' id='headingOne'>")
            'html.Append("<button class='accordion-button' type='button' data-bs-toggle='collapse' data-bs-target='#collapseOne' aria-expanded='true' aria-controls='collapseOne'>Goal: Optimize IT Portfolio To Improve Percentages</button>")
            'html.Append("</h2>")
            'html.Append("<div id='collapseOne' class='accordion-collapse collapse show' aria-labelledby='headingOne' data-bs-parent='#accordionExample'>")
            'html.Append("<div class='accordion-body'>")
            'html.Append("<div class='accordion-item'>")
            'html.Append("<h2 class='accordion-header' id='heading2'>")
            'html.Append("<button class='accordion-button d-flex justify-content-between' type='button' data-bs-toggle='collapse' data-bs-target='#collapse2' aria-expanded='true' aria-controls='collapse2'>Leverage Knowledge <span class='text-black'>10.29%</span></button> </h2>")
            html.Append("<div class='' id='divHierarchy'>")

            html.Append("</div>")

            'html.Append("</div>")
            'html.Append("</div>")
            'html.Append("</div>")
            'html.Append("</div>")
            'html.Append("</div>")

            html.Append("</div>")
            html.Append("</div>")
            html.Append("<div class='col-lg-8'>")
            html.Append("<div class='resultTable'>")
            'html.Append("<div class='alter_heading text-center position-relative'><span class='arrow_down'></span><h2>Alternatives</h2><span class='arrow_down'></span></div>")
            html.Append("<div class='table-responsive d-none d-lg-block global_resulttable'>")
            html.Append("<table class='table  table-hover mb-0 progress_table d-none' id='tblGlobalResults'>")
            html.Append("<thead><tr>")
            If CBool(dta1("showIndex")) Then
                html.Append("<th scope='col' class='text_blue' onclick=sort_Results('nodeID','true')>No <i class='fas fa-sort ms-2'></i></th>")
            End If
            html.Append($"<th scope='col' class='text_blue' onclick=sort_Results('nodeName','true')>{If(nameAlternatives IsNot Nothing, Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(nameAlternatives.ToLower()), "")} <i class='fas fa-sort ms-2'></i></th>")
            If (dta1("results_data")(0)(4).ToString() = "rvIndividual" Or dta1("results_data")(0)(4).ToString() = "rvBoth") And individual Then
                html.Append("<th scope='col' class='text_green text-center' onclick=sort_Results('yourResults','true')>Your Results <i class='fas fa-sort ms-2'></i></th>")
            End If
            If (dta1("results_data")(0)(4).ToString() = "rvGroup" Or dta1("results_data")(0)(4).ToString() = "rvBoth") And combined Then
                html.Append($"<th scope='col' class='text_blue text-center' onclick=sort_Results('combine','true')>{If(dta1("columnValueCombined") IsNot Nothing, dta1("columnValueCombined"), "")} <i class='fas fa-sort ms-2'></i></th>")
            End If
            If ShowBars Then
                html.Append("<th scope='col' class='text_blue'>Bar Graph</th>")
            End If
            html.Append("</tr></thead>")
            html.Append("<tbody>")
            For i = 0 To dta1("results_data").Count - 1
                Dim result = dta1("results_data")(i)
                html.Append("<tr>")
                If CBool(dta1("showIndex")) Then
                    html.Append($"<td scope='row'>{result(0)}</td>")
                End If
                html.Append($"<td>{result(1)}</td>")
                If (dta1("results_data")(0)(4).ToString() = "rvIndividual" Or dta1("results_data")(0)(4).ToString() = "rvBoth") And individual Then
                    html.Append($"<td class='text_green text-center text_bold'>{If(result(2).ToString() <> "" And result(2).ToString() <> "NaN", Math.Round(Convert.ToDecimal(result(2)) * 100, 2), 0.00)}%</td>")
                End If
                If (dta1("results_data")(0)(4).ToString() = "rvGroup" Or dta1("results_data")(0)(4).ToString() = "rvBoth") And combined Then
                    html.Append($"<td  class='text_blue text-center text_bold'>{If(result(3).ToString() <> "" And result(3).ToString() <> "NaN", Math.Round(Convert.ToDecimal(result(3)) * 100, 2), 0.00)}%</td>")
                End If

                If ShowBars Then
                    html.Append("<td >")
                    If (dta1("results_data")(0)(4).ToString() = "rvIndividual" Or dta1("results_data")(0)(4).ToString() = "rvBoth") And individual Then
                        html.Append("<div class='progress ' style='height: 10px;'>")
                        If result(2).ToString() = "" Or result(2).ToString() = "NaN" Then
                            html.Append($"<div class='progress-bar bg_green' role='progressbar' style='width: 0%;' aria-valuenow='0' aria-valuemin='0' aria-valuemax='{Math.Round(maxCombinedResult * 100, 2)}'></div>")
                        Else
                            'html.Append($"<div class='progress-bar bg_blue' role='progressbar' style='width: {If(result(2).ToString() <> "" And result(2).ToString() <> "NaN" And maxCombinedResult > 0, Math.Round(Convert.ToDecimal(result(2)) * Math.Round(maxCombinedResult * 100, 2), 2), 0.00)}%;' aria-valuenow='{If(result(2).ToString() <> "" And result(2).ToString() <> "NaN", Math.Round(Convert.ToDecimal(result(2)) * 100, 2), 0.00)}' aria-valuemin='0' aria-valuemax='100'></div>")
                            html.Append($"<div class='progress-bar bg_green' role='progressbar' style='width: {If(result(2).ToString() <> "" And result(2).ToString() <> "0" And maxCombinedResult > 0 And result(2).ToString() <> "NaN", Math.Round(Convert.ToDecimal(result(2)) * 100 / Math.Round(maxCombinedResult * 100, 2) * 100, 2), 0.00)}%;' aria-valuenow='{If(result(2).ToString() <> "" And result(2).ToString() <> "NaN", Math.Round(Decimal.Parse((result(2).ToString()), System.Globalization.NumberStyles.Any) * 100, 2), 0.00)}' aria-valuemin='0' aria-valuemax='{Math.Round(maxCombinedResult * 100, 2)}'></div>")
                        End If
                        html.Append("</div>")
                    End If

                    If (dta1("results_data")(0)(4).ToString() = "rvGroup" Or dta1("results_data")(0)(4).ToString() = "rvBoth") And combined Then
                        html.Append("<div class='progress' style='height: 10px;'>")
                        If result(3).ToString() = "" Or result(3).ToString() = "NaN" Then
                            html.Append($"<div class='progress-bar bg_blue' role='progressbar' style='width: 0%;' aria-valuenow='0' aria-valuemin='0' aria-valuemax='{Math.Round(maxCombinedResult * 100, 2)}'></div>")
                        Else
                            'html.Append($"<div class='progress-bar bg_green' role='progressbar' style='width: {If(result(3).ToString() <> "" And result(3).ToString() <> "NaN" And maxCombinedResult > 0, Math.Round(Convert.ToDecimal(result(3)) * 100 / Math.Round(maxCombinedResult * 100, 2) * 100, 2), 0.00)}%;' aria-valuenow='{If(result(3).ToString() <> "" And result(3).ToString() <> "NaN", Math.Round(Convert.ToDecimal(result(3)) * 100, 2), 0.00)}' aria-valuemin='0' aria-valuemax='100'></div>")
                            html.Append($"<div class='progress-bar bg_blue' role='progressbar' style='width: {If(result(3).ToString() <> "" AndAlso result(3).ToString() <> "NaN" AndAlso Convert.ToDecimal(result(3)) > 0 AndAlso maxCombinedResult > 0, Math.Round(Convert.ToDecimal(result(3)) * 100 / Math.Round(maxCombinedResult * 100, 2) * 100, 2), 0.00)}%;' aria-valuenow='{If(result(3).ToString() <> "" And result(3).ToString() <> "NaN", Math.Round(Convert.ToDecimal(result(3)) * 100, 2), 0.00)}' aria-valuemin='0' aria-valuemax='{Math.Round(maxCombinedResult * 100, 2)}'></div>")
                        End If
                        html.Append("</div>")
                    End If
                    html.Append("</td>")
                End If
                html.Append("</tr>")
            Next
            html.Append("</tbody>")
            html.Append("</table>")

            html.Append("</div>")
            html.Append("</div>")
            html.Append("</div>")
            html.Append("</div>")
            html.Append("</div>")




            html.Append("</div>")


            'html.Append("<div class='container-fluid px-0'>")
            'html.Append("<div class='col-12'>")
            'html.Append("<div class='row'>")

            'If showNormalization Then
            '    html.Append("<div class='col-lg-6 d-lg-none'><div class='sorting_togle'><div class='table_headingwrt'><h5 id='headText'></h5></div><input type='checkbox' class='sorting_arrow'>")
            '    html.Append("<span><i class='fa fa-angle-down' aria-hidden='true'></i></span><div class='sort_bubble'>")
            '    html.Append("<div class='row'>")
            '    html.Append("<div class='col-6'><label>Normalization</label><select id='mddlGlobalNormalization' onchange=onSortNormalizationChange('mddlGlobalNormalization') class='form-select'>")
            '    html.Append("<option value='1'>Normalized</Option>")
            '    html.Append("<option value='2'>% of Maximum</Option>")
            '    html.Append("<option value='3'>Unnormalized</Option>")
            '    html.Append("</select></div>")
            '    html.Append("<div class='col-6'><label>Sort By:</label><select id='sortNormalization' onchange=onSortNormalizationChange('sortNormalization') class='form-select'>")
            '    html.Append("<option value='nodeID'>Index</Option>")
            '    html.Append("<option value='nodeName'>Name</Option>")
            '    html.Append("<option value='yourResults'>Your Results</Option>")
            '    html.Append("</select></div>")
            '    html.Append("</div></div></div></div>")
            'End If

            'html.Append("<div class='col-md-6 mob_table' style='display:none;'><input type='checkbox' class='toggle_tree'>")
            'html.Append("<span><i class='fa fa-bars' aria-hidden='true'></i></span>")
            'html.Append("<div id='mdivHierarchy' style='display:none;' class='tree_hideshow'></div>")
            'html.Append("</div>")

            'If dta1("results_data").Count > 0 AndAlso (individual Or combined) AndAlso check_shown_results(individual, combined, dta1("results_data")(0)(4).ToString()) Then
            '    'html.Append("<div class='col-lg-8 d-lg-block d-none'>")
            '    'html.Append("<div class='border table-responsive table_Scroll'>")

            '    'html.Append("</div>")
            '    'html.Append("</div>")


            '    html.Append("<div class='mob_progress_table d-lg-none'>")

            '    'If (dta1("results_data")(0)(4).ToString() = "rvIndividual" Or dta1("results_data")(0)(4).ToString() = "rvBoth") And individual Then
            '    '    html.Append("<table class='table mb-0' id='mtblGlobalResults'><thead><tr><th><div class='table_header'><span style='width:100%;background:#211d71;'>Your Results</span>")
            '    '    html.Append("</div></th></tr></thead>")
            '    If (dta1("results_data")(0)(4).ToString() = "rvBoth") Then
            '        html.Append("<table class='table mb-0' id='mtblGlobalResults'><thead><tr><th><div class='table_header'  style='color: #92c645 !IMPORTANT;'><span>Your Results</span><span>Combined Results</span>")

            '        html.Append("</div></th></tr></thead>")
            '    ElseIf (dta1("results_data")(0)(4).ToString() = "rvGroup") And combined Then
            '        html.Append("<table class='table mb-0' id='mtblGlobalResults'><thead><tr><th><div class='table_header'><span style='width:100%;background:#92c645;'>Combined Results</span>")

            '        html.Append("</div></th></tr></thead>")
            '    ElseIf (dta1("results_data")(0)(4).ToString() = "rvIndividual") And individual Then
            '        html.Append("<table class='table mb-0' id='mtblGlobalResults'><thead><tr><th><div class='table_header'><span style='width:100%;background:#211d71;'>Your Results</span>")
            '        html.Append("</div></th></tr></thead>")
            '    End If

            '    html.Append("<tbody class='mtbody'>")
            '    For j = 0 To dta1("results_data").Count - 1
            '        Dim mresult = dta1("results_data")(j)
            '        html.Append("<tr> <td><div class='table_item'><p>")
            '        If CBool(dta1("showIndex")) Then
            '            html.Append($"<span>{mresult(0)}</span>")
            '        End If

            '        html.Append($"{mresult(1)}</p><div class='progress_result'>")

            '        If (dta1("results_data")(0)(4).ToString() = "rvIndividual" Or dta1("results_data")(0)(4).ToString() = "rvBoth") And individual And ShowBars Then
            '            html.Append("<div class='progress' style='height: 20px;'>")
            '            If mresult(2).ToString() = "" Or mresult(2).ToString() = "NaN" Then
            '                html.Append($"<div class='progress-bar bg_blue' role='progressbar' style='width: 25%;' aria-valuenow='25' aria-valuemin='0' aria-valuemax='{Math.Round(maxCombinedResult * 100, 2)}'></div>")
            '            Else
            '                html.Append($"<div class='progress-bar bg_blue' role='progressbar' style='width: {If(mresult(2).ToString() <> "" And mresult(2).ToString() <> "0" And maxCombinedResult > 0 And mresult(2).ToString() <> "NaN", Math.Round(Convert.ToDecimal(mresult(2)) * 100 / Math.Round(maxCombinedResult * 100, 2) * 100, 2), 0.00)}%;' aria-valuenow='{If(mresult(2).ToString() <> "" And mresult(2).ToString() <> "NaN", Math.Round(Decimal.Parse((mresult(2).ToString()), System.Globalization.NumberStyles.Any) * 100, 2), 0.00)}' aria-valuemin='0' aria-valuemax='{Math.Round(maxCombinedResult * 100, 2)}'></div>")
            '            End If
            '            html.Append("</div>")
            '        End If


            '        If (dta1("results_data")(0)(4).ToString() = "rvIndividual" Or dta1("results_data")(0)(4).ToString() = "rvBoth") And individual Then
            '            html.Append($"<p>{If(mresult(2).ToString() <> "" And mresult(2).ToString() <> "NaN", Math.Round(Convert.ToDecimal(mresult(2)) * 100, 2), 0.00)}%</p>")
            '        End If

            '        html.Append("</div><div class='progress_result'>")
            '        'html.Append("<div class='progress bg-transparent' style='height: 20px;'>")
            '        'html.Append("<div class='progress-bar bg_green' role='progressbar' style='width: 35%;' aria-valuenow='35' aria-valuemin='0' ")
            '        'html.Append("aria-valuemax='100'></div></div>")

            '        If (dta1("results_data")(0)(4).ToString() = "rvGroup" Or dta1("results_data")(0)(4).ToString() = "rvBoth") And combined And ShowBars Then
            '            html.Append("<div class='progress' style='height: 20px;'>")
            '            If mresult(3).ToString() = "" Or mresult(3).ToString() = "NaN" Then
            '                html.Append($"<div class='progress-bar bg_green' role='progressbar' style='width: 35%;' aria-valuenow='35' aria-valuemin='0' aria-valuemax='{Math.Round(maxCombinedResult * 100, 2)}'></div>")
            '            Else
            '                html.Append($"<div class='progress-bar bg_green' role='progressbar' style='width: {If(mresult(3).ToString() <> "" And mresult(3).ToString() <> "NaN", Math.Round(Convert.ToDecimal(mresult(3)) * 100 / Math.Round(maxCombinedResult * 100, 2) * 100, 2), 0.00)}%;' aria-valuenow='{If(mresult(3).ToString() <> "" And mresult(3).ToString() <> "NaN", Math.Round(Convert.ToDecimal(mresult(3)) * 100, 2), 0.00)}' aria-valuemin='0' aria-valuemax='{Math.Round(maxCombinedResult * 100, 2)}'></div>")
            '            End If
            '            html.Append("</div>")
            '        End If

            '        If (dta1("results_data")(0)(4).ToString() = "rvGroup" Or dta1("results_data")(0)(4).ToString() = "rvBoth") And combined Then
            '            html.Append($"<p>{If(mresult(3).ToString() <> "" And mresult(3).ToString() <> "NaN", Math.Round(Convert.ToDecimal(mresult(3)) * 100, 2), 0.00)}%</p>")
            '        End If
            '        html.Append("</div></div></td></tr>")
            '    Next

            '    html.Append("</tbody>")
            '    html.Append("</table></div>")
            'Else
            '    html.Append("<div class='col-lg-8 d-lg-block text-muted text-xl-center text10'>")
            '    html.Append($"<h3>{If(dta1("messagenote") IsNot Nothing AndAlso dta1("messagenote").Count > 0, dta1("messagenote")(0), "")}</h3>")
            '    html.Append("<br/>")
            '    html.Append("<span>If a participant does not have a role for one or more evaluations then there may not be enough information to show his/her individual results. 
            '    Ask the project manager to set the Combined Input Source (CIS) option so that evaluations of others having roles for those evaluations which you do not, 
            '    will be used to generate and display results.</span>")
            '    html.Append("</div>")
            'End If
            'html.Append("</div>")
            'html.Append("</div>")
            'html.Append("</div>")
            'html.Append("</div>")
            html.Append("</div>")
            divContent.InnerHtml = html.ToString()

            ''Mobile View
            html = New StringBuilder()
            html.Append("<div class='objectives_main d-block d-lg-none'>")
            html.Append("<div class='objectives_tab'>")


            html.Append($"<ul class='nav nav-tabs justify-content-center' id='myTab' role='tablist'>
                <li class='nav-item' role='presentation'>
                  <button class='nav-link' id='objectives-tab' data-bs-toggle='tab' data-bs-target='#objectives' type='button' role='tab' aria-controls='objectives' aria-selected='false'>{If(question IsNot Nothing, Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(question.ToLower()), "")}</button>
                </li>
                <li class='nav-item' role='presentation'>
                  <button class='nav-link active' id='alternatives-tab' data-bs-toggle='tab' data-bs-target='#alternatives' type='button' role='tab' aria-controls='alternatives' aria-selected='true'>{If(nameAlternatives IsNot Nothing, Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(nameAlternatives.ToLower()), "")}</button>
                </li>
              </ul>")
            html.Append("<div class='tab-content' id='myTabContent'>
                <div class='tab-pane fade' id='objectives' role='tabpanel' aria-labelledby='objectives-tab'>
                    <div class='objective_section'>
                        <div class='accordion' id='accordionExample'>
                            <div class='accordion-item'>
                                <h2 class='accordion-header px-2' id='headingOne'>
                                    <button class='accordion-button' type='button' data-bs-toggle='collapse' data-bs-target='#collapseOne' aria-expanded='true' aria-controls='collapseOne'>Goal: Optimize IT Portfolio To Improve Percentages</button>
                                </h2>
                                <div id='collapseOne' class='accordion-collapse collapse show' aria-labelledby='headingOne' data-bs-parent='#accordionExample' style=''>
                                    <div class='accordion-body'>                                        
                                        <div class='accordion-item'>
                                            <h2 class='accordion-header' id='heading4'>
                                                <button class='accordion-button d-flex justify-content-between collapsed' type='button' data-bs-toggle='collapse' data-bs-target='#collapse4' aria-expanded='false' aria-controls='collapse4'>Maintain Serviceability<span class='text-black'>33.29%</span></button>
                                            </h2>
                                            <div id='collapse4' class='accordion-collapse collapse show' aria-labelledby='heading4' data-bs-parent='#accordion4'>
                                                <div class='accordion-body p-0'>
                                                  <ul>
                                                      <li>Improve Service Efficiencies <span>0.00%</span> </li>
                                                      <li>Leverage Purchasing Power <span>0.00%</span> </li>
                                                      <li>Imporve Time to Market <span>0.00%</span> </li>
                                                      <li>Manage Resources <span>0.00%</span> </li>
                                                  </ul>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>")
            If dta1("results_data").Count > 0 AndAlso (individual Or combined) AndAlso check_shown_results(individual, combined, dta1("results_data")(0)(4).ToString()) Then
                html.Append("<div class='tab-pane fade active show' id='alternatives' role='tabpanel' aria-labelledby='alternatives-tab'>
                    <div class='resultTable resultTable_mobile'>
                        <div class='resultTable_header'>
                            <div class='row m-0'>")
                If (dta1("results_data")(0)(4).ToString() = "rvIndividual" Or dta1("results_data")(0)(4).ToString() = "rvBoth") Then
                    html.Append("<div class='col-6'>
                                    <h6 class=' text_green text-center'>Your Results</h6>
                                </div>")
                End If
                If (dta1("results_data")(0)(4).ToString() = "rvGroup" Or dta1("results_data")(0)(4).ToString() = "rvBoth") Then
                    html.Append($"<div class='col-6'>
                                    <h6 class=' text_blue text-center'>{If(dta1("columnValueCombined") IsNot Nothing, dta1("columnValueCombined"), "")} </h6>
                                </div>")
                End If
                html.Append("</div>
                        </div>")
                html.Append("<div id='mtblGlobalResults'>")

                For j = 0 To dta1("results_data").Count - 1
                    Dim mresult = dta1("results_data")(j)
                    html.Append("<div class='row m-0 py-3 border-bottom align-items-end'>
                <div class='col-12'> <div class='resultTable_column ps-3'>
                <p class='mb-2'>")
                    If CBool(dta1("showIndex")) Then
                        html.Append($"<span class='number'>{mresult(0)}</span>")
                    End If
                    html.Append($"{mresult(1)}</p></div></div>")
                    If (dta1("results_data")(0)(4).ToString() = "rvIndividual" Or dta1("results_data")(0)(4).ToString() = "rvBoth") Then
                        If (dta1("results_data")(0)(4).ToString() = "rvBoth") Then
                            html.Append($"<div class='col-6'><div class='resultTable_column ps-3'><div class='mobile_progress d-flex align-items-center'><div class='progress{clsdnone}'>")
                        Else
                            html.Append($"<div class='col-12'><div class='resultTable_column ps-3'><div class='mobile_progress d-flex align-items-center'><div class='progress{clsdnone}'>")
                        End If
                        If (dta1("results_data")(0)(4).ToString() = "rvIndividual" Or dta1("results_data")(0)(4).ToString() = "rvBoth") And individual And ShowBars Then

                            If mresult(2).ToString() = "" Or mresult(2).ToString() = "NaN" Then
                                html.Append($"<div class='progress-bar bg_green' role='progressbar' style='width:0%;' aria-valuenow='25' aria-valuemin='0' aria-valuemax='{Math.Round(maxCombinedResult * 100, 2)}'></div>")
                            Else
                                html.Append($"<div class='progress-bar bg_green' role='progressbar' style='width:{If(mresult(2).ToString() <> "" And mresult(2).ToString() <> "0" And maxCombinedResult > 0 And mresult(2).ToString() <> "NaN", Math.Round(Convert.ToDecimal(mresult(2)) * 100 / Math.Round(maxCombinedResult * 100, 2) * 100, 2), 0.00)}%;' aria-valuenow='{If(mresult(2).ToString() <> "" And mresult(2).ToString() <> "NaN", Math.Round(Decimal.Parse((mresult(2).ToString()), System.Globalization.NumberStyles.Any) * 100, 2), 0.00)}' aria-valuemin='0' aria-valuemax='{Math.Round(maxCombinedResult * 100, 2)}'></div>")
                            End If
                        End If
                        html.Append("</div>")
                        If (dta1("results_data")(0)(4).ToString() = "rvIndividual" Or dta1("results_data")(0)(4).ToString() = "rvBoth") And individual Then

                            html.Append($"<h6 class='text_green mb-0 ms-3'>{If(mresult(2).ToString() <> "" And mresult(2).ToString() <> "NaN", Math.Round(Convert.ToDecimal(mresult(2)) * 100, 2), 0.00)}%</h6>")
                        End If
                        html.Append("</div></div></div>")
                    End If
                    If (dta1("results_data")(0)(4).ToString() = "rvGroup" Or dta1("results_data")(0)(4).ToString() = "rvBoth") Then
                        If dta1("results_data")(0)(4).ToString() = "rvBoth" Then
                            html.Append($"<div class='col-6'><div class='resultTable_column pe-3'><div class='mobile_progress mobile_progress_right d-flex align-items-center'><div class='progress{clsdnone}'>")
                        Else
                            html.Append($"<div class='col-12'><div class='resultTable_column pe-3'><div class='mobile_progress mobile_progress_right d-flex align-items-center'><div class='progress{clsdnone}'>")
                        End If
                        If (dta1("results_data")(0)(4).ToString() = "rvGroup" Or dta1("results_data")(0)(4).ToString() = "rvBoth") And combined And ShowBars Then
                            If mresult(3).ToString() = "" Or mresult(3).ToString() = "NaN" Then
                                html.Append($"<div class='progress-bar' role='progressbar' style='width:0%;' aria-valuenow='35' aria-valuemin='0' aria-valuemax='{Math.Round(maxCombinedResult * 100, 2)}'></div>")
                            Else

                                html.Append($"<div class='progress-bar' role='progressbar' style='width:{If(mresult(3).ToString() <> "" AndAlso mresult(3).ToString() <> "NaN" AndAlso Convert.ToDecimal(mresult(3)) > 0 AndAlso maxCombinedResult > 0, Math.Round(Convert.ToDecimal(mresult(3)) * 100 / Math.Round(maxCombinedResult * 100, 2) * 100, 2), 0.00)}%;' aria-valuenow='{If(mresult(3).ToString() <> "" And mresult(3).ToString() <> "NaN", Math.Round(Convert.ToDecimal(mresult(3)) * 100, 2), 0.00)}' aria-valuemin='0' aria-valuemax='{Math.Round(maxCombinedResult * 100, 2)}'></div>")
                            End If

                        End If
                        html.Append("</div>")
                        If (dta1("results_data")(0)(4).ToString() = "rvGroup" Or dta1("results_data")(0)(4).ToString() = "rvBoth") And combined Then
                            html.Append($"<h6 class='text_blue mb-0 ms-3'>{If(mresult(3).ToString() <> "" And mresult(3).ToString() <> "NaN", Math.Round(Convert.ToDecimal(mresult(3)) * 100, 2), 0.00)}%</h6>")
                        End If
                        html.Append("</div></div></div>")
                    End If
                    html.Append("</div>")
                Next
                html.Append("</div>")
            Else
                html.Append("<div class='col-lg-8 d-lg-block text-muted text-xl-center text10'>")
                html.Append($"<h3>{If(dta1("messagenote") IsNot Nothing AndAlso dta1("messagenote").Count > 0, dta1("messagenote")(0), "")}</h3>")
                html.Append("<br/>")
                html.Append("<span>If a participant does not have a role for one or more evaluations then there may not be enough information to show his/her individual results. 
                Ask the project manager to set the Combined Input Source (CIS) option so that evaluations of others having roles for those evaluations which you do not, 
                will be used to generate and display results.</span>")
                html.Append("</div>")
            End If
            html.Append("</div></div></div></div>")
            '    html.Append("<div class='container'><div Class='nomalized_select d-flex align-items-center justify-content-end my-4'>
            '<Label Class='me-3'><b>Normalization</b></label>
            '<Select id='mddlGlobalNormalization' Class='form-select' onchange=onSortNormalizationChange('mddlGlobalNormalization') >
            '   <option value='1'>Normalized</Option>
            '    <option value='2'>% of Maximum</Option>
            '    <option value='3'>Unnormalized</Option>
            '</select> </div></div> ")
            html.Append("<div class='container'><div class='col-lg-6 d-lg-none'><div class='sorting_togle min-ht'><div class='table_headingwrt'><h5 id='headText'></h5></div><div class='sort_bubble'><div class='row'>")
            If showNormalization Then
                html.Append("<div class='col-6'><label>Normalization:</label><select id='mddlGlobalNormalization' onchange=onSortNormalizationChange('mddlGlobalNormalization') class='form-select'><option value='1'>Normalized</option><option value='2'>% of Maximum</option><option value='3'>Unnormalized</option></select></div>")
            End If
            html.Append("<div class='col-6'><label>Sort By:</label><select id='sortNormalization' onchange=onSortNormalizationChange('sortNormalization') class='form-select'>")
            If CBool(dta1("showIndex")) Then
                html.Append("<option value='nodeID'>Index</option>")
            End If
            html.Append("<option value='nodeName'>Name</option>")
            If (dta1("results_data")(0)(4).ToString() = "rvIndividual" Or dta1("results_data")(0)(4).ToString() = "rvBoth") And individual Then
                html.Append("<option value='yourResults'>Your Results</option>")
            End If
            If (dta1("results_data")(0)(4).ToString() = "rvGroup" Or dta1("results_data")(0)(4).ToString() = "rvBoth") And combined Then
                html.Append($"<option value='combine'>{dta1("columnValueCombined")}</option>")
            End If
            html.Append("</select>")
            html.Append("</div></div></div></div></div></div>")
            divCMobile.InnerHtml = html.ToString()
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