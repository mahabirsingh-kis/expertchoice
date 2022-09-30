Public Class LocalResults
    Inherits System.Web.UI.UserControl

    Dim screenCheck As ScreenCheck = New ScreenCheck()
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

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

    Public Sub bindHtml(ByVal PipeParameters As Object, ByVal step_task As String, ByVal parentnodeID As Integer, ByVal current_step As Integer,
                        ByVal isPipeViewOnly As Boolean, ByVal cluster_phrase As String)
        If PipeParameters IsNot Nothing And PipeParameters.ToString() <> "False" Then
            Dim html As StringBuilder = New StringBuilder()
            Dim dta = JsonConvert.SerializeObject(PipeParameters)
            Dim dta1 = JObject.Parse(dta)
            Dim DisplayNone As String = " d-none"
            If dta1("MeasurementType").ToString() = "ptVerbal" Then
                DisplayNone = ""
            End If
            Dim nameAlternatives As String = dta1("CurrentNode")(1).ToString().Trim()
            'Dim hdnStepPairsData As HiddenField = CType(Page.Form.FindControl("hdnStepPairsData"), HiddenField)
            'hdnStepPairsData.Value = dta1("StepPairsData").ToString()
            'Dim hdnObjectivesData As HiddenField = CType(Page.Form.FindControl("hdnObjectivesData"), HiddenField)
            'hdnObjectivesData.Value = dta1("ObjectivesData").ToString()

            html.Append("<div class=''>")
            'html.Append("<div class='row'>")
            Dim individual As Boolean = CType(dta1("canshowresults")("individual").ToString(), Boolean)
            Dim combined As Boolean = CType(dta1("canshowresults")("combined").ToString(), Boolean)
            Dim equalMessage As Boolean = CType(dta1("equalMessage").ToString(), Boolean)
            Dim showIndex As Boolean = CType(dta1("showIndex").ToString(), Boolean)
            Dim showNormalization As Boolean = CType(dta1("showNormalization").ToString(), Boolean)
            Dim ShowBars As Boolean = CType(dta1("ShowBars").ToString(), Boolean)
            Dim clsdnone As String = ""
            If ShowBars <> True Then
                clsdnone = " d-none"
            End If

            Dim maxCombinedResult As Decimal = 0, loopCount As Integer = 0
            Dim hdnCombinedResult As Decimal = 0, hdnindividualResult As Decimal = 0

            If dta1("results_data") IsNot Nothing Then
                If Request.Cookies("TotalResultRows") IsNot Nothing Then
                    Dim TotalResultRows As HttpCookie = Request.Cookies("TotalResultRows")
                    TotalResultRows.Value = Val(dta1("results_data").Count - 1).ToString()
                Else
                    Dim TotalResultRows As New HttpCookie("TotalResultRows")
                    TotalResultRows.Value = Val(dta1("results_data").Count - 1).ToString()
                    If (TotalResultRows.Value.Contains("TotalResultRows")) Then
                        TotalResultRows.Value = TotalResultRows.Value.Substring(16)
                    End If
                    Response.Cookies.Add(TotalResultRows)
                End If
                For i = 0 To dta1("results_data").Count - 1
                    If loopCount = 0 Then
                        If (dta1("results_data")(0)(4).ToString() = "rvBoth") And combined Then
                            hdnCombinedResult = If(dta1("results_data")(i)(3).ToString() <> "" And dta1("results_data")(i)(3).ToString() <> "NaN", Convert.ToDecimal(dta1("results_data")(i)(3)), 0)
                            hdnindividualResult = If(dta1("results_data")(i)(2).ToString() <> "" And dta1("results_data")(i)(2).ToString() <> "NaN", Convert.ToDecimal(dta1("results_data")(i)(2)), 0)
                            maxCombinedResult = If(hdnCombinedResult > hdnindividualResult, hdnCombinedResult, hdnindividualResult)
                        ElseIf (dta1("results_data")(0)(4).ToString() = "rvGroup") And combined Then
                            maxCombinedResult = If(dta1("results_data")(i)(3).ToString() <> "" And dta1("results_data")(i)(3).ToString() <> "NaN", Convert.ToDecimal(dta1("results_data")(i)(3)), 0)
                        Else
                            maxCombinedResult = If(dta1("results_data")(i)(2).ToString() <> "" And dta1("results_data")(i)(2).ToString() <> "NaN", Convert.ToDecimal(dta1("results_data")(i)(2)), 0)
                        End If
                    Else
                        If (dta1("results_data")(0)(4).ToString() = "rvBoth") And combined Then
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

            'If (individual Or combined) And check_shown_results(individual, combined, dta1("results_data")(0)(4).ToString()) And Not equalMessage Then
            If (individual Or combined) And check_shown_results(individual, combined, dta1("results_data")(0)(4).ToString()) Then
                html.Append("<div class=''>")
                html.Append("<div id='divIntermediateResults' class='divHeader1'>")
                html.Append("<div style='display: block;' class='page_heading_section'>")
                If dta1("showTitle").ToString() = "True" Then
                    html.Append("<div class='container'>")
                    html.Append("<div class='row'>")
                    html.Append("<div class='col-md-6' >")
                    html.Append("<div class='page-title-box  toggle_title' id='titleDiv1'>")
                    html.Append("<div class='collabse_btn' >")
                    html.Append("<input type='checkbox' class='collabse_arrow d-none' checked>")
                    html.Append("<div Class='arrow_icon d-none'>")
                    html.Append("<i Class='fa fa-plus' aria-hidden='true'></i><i Class='fa fa-minus' aria-hidden='true'></i>")
                    html.Append("</div>")
                    Dim stTask As String = RemoveHTML(step_task)
                    'html.Append($"<div class='d-flex justify-content-center mb-2 flex-wrap font-size-16'></div>")

                    Dim model_text As String = HttpUtility.UrlEncode(cluster_phrase.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
                    'html.Append($"<a{If(isPipeViewOnly, " class='d-none'", "")} href='javascript:void(0);' onclick=showHeaderPopup(decodeURIComponent({ChrW(&H22) + model_text + ChrW(&H22)}),'0','0','0','0',null,'question-node',decodeURIComponent({ChrW(&H22) + model_text + ChrW(&H22)}))>")
                    'html.Append("<i Class='bx bxs-edit-alt chkEvaluator d-none'></i></a>")
                    'html.Append("</div>")

                    'html.Append("<div class='heading_content'><div class='page-title-box' id='titleDiv'>")
                    'html.Append($"<a {If(isPipeViewOnly, " class='d-none'", "")} href='#' onclick=showHeaderPopup(decodeURIComponent({ChrW(&H22) + model_text + ChrW(&H22)}),'0','0','0','0',null,'question-node',decodeURIComponent({ChrW(&H22) + model_text + ChrW(&H22)}))><img src='../../img/icon/edit-icon.svg'></a>")
                    'html.Append($"<div class='txtStepTask'>{step_task}</div>")
                    'html.Append("<asp:Label ID='lblTask' runat='server' /><span id='divT2S' style='font-size:20px; margin-left:1ex border: 0px !important; background: #f0f0ff08 !important; display:none'></span>")
                    'html.Append("</div>")
                    'html.Append($"<a {If(isPipeViewOnly, " class='d-none'", " class='chkEvaluatorone'")} href='javascript:void(0);' onclick=showHeaderPopup(decodeURIComponent({ChrW(&H22) + model_text + ChrW(&H22)}),'0','0','0','0',null,'question-node',decodeURIComponent({ChrW(&H22) + model_text + ChrW(&H22)}))><img src='../../img/icon/edit-icon.svg'></a>")
                    'html.Append("<div class='heading_info'><img src='../../img/icon/empty_info.svg' class='d-none'><img src='../../img/icon/info_data.svg'><img src='../../img/icon/info-close.svg' class='d-none'></div>")
                    'html.Append("</div>")

                    Dim screenCheck As ScreenCheck = New ScreenCheck()
                    html.Append("<div class='heading_content'><div class='page-title-box' id='titleDiv'><div class='heading_icons'>")
                    If Not (screenCheck.isMobileBrowserClient(Request)) Then
                        html.Append($"<a {If(isPipeViewOnly, " class='d-none'", " class='editsvg_icon'")}  href='javascript:void(0);' onclick=showHeaderPopup(decodeURIComponent({ChrW(&H22) + model_text + ChrW(&H22)}),'0','0','0','0',null,'question-node',decodeURIComponent({ChrW(&H22) + model_text + ChrW(&H22)}))><img class='chkEvaluatorone' src='../../img/icon/edit-icon.svg'></a>")
                    End If
                    html.Append("<asp:Label ID='lblTask' runat='server' ><span id='divT2S' style='font-size:20px; margin-left:1ex border: 0px !important; background: #f0f0ff08 !important; display:none'></span></asp:Label></div>")
                    html.Append($"<div class='txtStepTask removep' id='MainHeaderInfodoc'>{step_task.Replace("<p>", "").Replace("</p>", "")}<span class='threedoticon d-none' id='btnheadinfo'> <a href='#' onclick='showheaderpopup()' ><div class='snippet'><div class='stage'><div class='dot-falling'></div> </div></div></a></span></div>")
                    'html.Append("<asp:Label ID='lblTask' runat='server' ><span id='divT2S' style='font-size:20px; margin-left:1ex border: 0px !important; background: #f0f0ff08 !important; display:none'></span></asp:Label>")
                    'html.Append("<div class='heading_info'><img src='../../img/icon/empty_info.svg' class='d-none'><img src='../../img/icon/info_data.svg'><img src='../../img/icon/info-close.svg' class='d-none'></div>")
                    Dim headertxt = dta1("resultsTitle").ToString()
                    Dim IsHideHeaderInfo As Boolean = False
                    'If headertxt Is Nothing Or headertxt = "" Then
                    '    html.Append($"<div class='heading_info' id='HeaderDivIcon'><a href='#'><img  id='img1' src='../../img/icon/empty_info.svg'><img id='img2' src='../../img/icon/info_data.svg' style='display: none;'><img id='img3' src='../../img/icon/info-close.svg' style='display: none;'></a></div>")
                    '    IsHideHeaderInfo = True
                    'Else
                    '    html.Append($"<div class='heading_info' id='HeaderDivIcon'><a href='#'><img  id='img1' src='../../img/icon/empty_info.svg' style='display: none;'><img id='img2' src='../../img/icon/info_data.svg'><img id='img3' src='../../img/icon/info-close.svg' style='display: none;'></a></div>")
                    'End If
                    html.Append("</div>")
                    html.Append("</div>")

                    html.Append("</div>")
                    html.Append("</div>")
                    html.Append("</div>")

                    html.Append("<div class='col-md-6 d-none'>")
                    html.Append($"<div class='info_content d-flex mt-lg-0 mt-2' id='Header_InfoDocs'><div class='heading_icons heading_info me-0'><div  id='HeaderDivIcon'><a href='#'><img  id='img1' src='../../img/icon/empty_info.svg' style='display: none;'><img id='img2' src='../../img/icon/info_data.svg'><img id='img3' src='../../img/icon/info-close.svg' style='display: none;'></a></div><div class='infodoc_icons'><a class='editsvg_icon' href='javascript:void(0);' onclick=showInfoPopup(decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(headertxt.Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}),'0','-1','0','-1',null,'parent-node',decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(headertxt.Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}))><img class='chkEvaluatorone' src='../../img/icon/edit-icon.svg'></a><a href='#' onclick=Expandtxt(decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(headertxt.Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}),'0','-1','0','-1',null,'parent-node',decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(headertxt.Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)})) data-bs-toggle='modal' data-bs-target='#infodocPop'><img src='../../img/icon/full-screen-svgrepo-com.svg' id='imgFullScreen'></a></div></div>")
                    html.Append("<div class='info_content_wrapper'>")
                    html.Append("<div class='info_text'>")

                    'html.Append($"<h3 class='pb-0 prm-color'>{dta1("resultsTitle")}</h3><p></p></p>")

                    html.Append($"<p'>{headertxt}</p>")
                    headertxt = HttpUtility.UrlEncode(headertxt.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
                    html.Append("</div>")

                    'Editor Right Side Button

                    'html.Append($"<div {If(isPipeViewOnly, " class='d-none'", " class='info_box_icons'")}><a href='javascript:void(0);'><img class='chkEvaluatorone' src='../../img/icon/edit-icon.svg'></a><a href='javascript:void(0);'><img src='../../img/icon/full-screen-svgrepo-com.svg'></a></div>")
                    Dim infotxt = If(dta1("CurrentNode") IsNot Nothing, HttpUtility.UrlEncode(dta1("CurrentNode")(0).ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))), "")
                    html.Append($"<div {If(isPipeViewOnly, " class='d-none'", " class='info_box_icons d-none'")}><a href='javascript:void(0);' class='chkEvaluatorone' onclick=showHeaderPopup(decodeURIComponent({ChrW(&H22) + headertxt + ChrW(&H22)}),null,'-1','0','-1',null,'parent-node',decodeURIComponent({ChrW(&H22) + infotxt + ChrW(&H22)}))><img class='chkEvaluatorone' src='../../img/icon/edit-icon.svg'></a><a href='javascript:void(0);' data-bs-toggle='modal' data-bs-target='#infodocPop' onclick=Expandtxt(decodeURIComponent({ChrW(&H22) + headertxt + ChrW(&H22)}),'0','0','0','0',null,'question-node',decodeURIComponent({ChrW(&H22) + headertxt + ChrW(&H22)}))><img src='../../img/icon/full-screen-svgrepo-com.svg'></a></div>")

                    html.Append($"<div {If(isPipeViewOnly, " class='d-none'", " class='info_box_icons d-none'")}><a href='javascript:void(0);' class='chkEvaluatorone' onclick=showHeaderPopup(decodeURIComponent({ChrW(&H22) + headertxt + ChrW(&H22)}),null,'-1','0','-1',null,'parent-node',decodeURIComponent({ChrW(&H22) + headertxt + ChrW(&H22)}))><img class='chkEvaluatorone' src='../../img/icon/edit-icon.svg'></a><a href='javascript:void(0);' data-bs-toggle='modal' data-bs-target='#infodocPop' onclick=Expandtxt(decodeURIComponent({ChrW(&H22) + headertxt + ChrW(&H22)}),'0','0','0','0',null,'question-node',decodeURIComponent({ChrW(&H22) + headertxt + ChrW(&H22)}))><img src='../../img/icon/full-screen-svgrepo-com.svg'></a></div>")

                    'html.Append($"<a href='javascript:void(0);' data-bs-toggle='modal' data-bs-target='#infodocPop' onclick='Expandtxt(decodeURIComponent({ChrW(&H22) + model_text + ChrW(&H22)}),'0','0','0','0',null,'question-node',decodeURIComponent({ChrW(&H22) + model_text + ChrW(&H22)}))' ><img src='../../img/icon/full-screen-svgrepo-com.svg'></a>")
                    html.Append("</div>")
                    html.Append("</div>")
                    html.Append("</div>")
                    html.Append("</div>")
                End If

                html.Append("</div>")
                html.Append("</div>")

                html.Append("<div class='container'>")
                html.Append("<div class='row mt-3'>")

                Dim infotxtsum = If(dta1("CurrentNode") IsNot Nothing, dta1("CurrentNode")(0).ToString().Trim(), "")
                Dim headertxtSub As String = dta1("resultsTitle").ToString()
                If Not CBool(dta1("CurrentNode")(2)) AndAlso showNormalization Then
                    html.Append($"<div class='col-lg-6 offset-lg-1'><div class='heading_content mb-2'><div class='me-2 removep2 table_heading'><p title='{dta1("resultsTitle").ToString()}'>{dta1("resultsTitle").ToString()}</p></div> <div Class='heading_info'><a{If(isPipeViewOnly, " class='d-none'", "")} href='#'><img class='chkEvaluatorone w-75' src = '../../img/icon/edit-icon.svg' onclick=showHeaderPopup(decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(headertxtSub.Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}),'0','-1','0','-1',null,'parent-node',decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(infotxtsum.Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}),'Subheading')> </a> </div></div></div>")
                    html.Append("<div class='col-lg-4 d-lg-block d-none'>")
                    html.Append("<div Class='align-items-center d-flex justify-content-end mb-3 normalization_dropdown'>")
                    html.Append("<Label class='mb-0'>Normalization : </label>")
                    html.Append("<Select id='ddlNormalization' onchange=onSortNormalizationChange('ddlNormalization') Class='form-select' aria-label='Default select example'>")
                    html.Append("<option value='1'>Normalized</Option>")
                    html.Append("<option value='2'>% of Maximum</Option>")
                    html.Append("<option value='3'>Unnormalized</Option>")
                    html.Append("</select>")
                    html.Append("</div>")
                    html.Append("</div>")
                Else

                    html.Append($"<div class='col-lg-10 offset-lg-1'><div class='heading_content mb-2'><div class='me-2 removep2 table_heading'><p title='{dta1("resultsTitle").ToString()}'>{dta1("resultsTitle").ToString()}</p></div> <div Class='heading_info'><a{If(isPipeViewOnly, " class='d-none'", "")} href='#'><img class='chkEvaluatorone w-75' src = '../../img/icon/edit-icon.svg' onclick=showHeaderPopup(decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(headertxtSub.Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}),'0','-1','0','-1',null,'parent-node',decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(infotxtsum.Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}),'Subheading')> </a> </div></div></div>")
                End If
                html.Append("<div class='col-lg-10 offset-lg-1'>")

                'for mobile view
                html.Append("<div class='d-block d-lg-none'><div class='resultTable_mobile' id='divLocalResults'>")

                html.Append("<div class='border-bottom pb-2 pt-3 m-0 row local_result_head'>")
                If (dta1("results_data")(0)(4).ToString() = "rvIndividual" Or dta1("results_data")(0)(4).ToString() = "rvBoth") And individual Then
                    html.Append("<div class='col-6'><h6 class='text_green text-center mobile_progress '>Your Results</h6></div>")
                End If
                If (dta1("results_data")(0)(4).ToString() = "rvGroup" Or dta1("results_data")(0)(4).ToString() = "rvBoth") And combined Then
                    html.Append($"<div class='col-6'><h6 class='text_blue text-center'>{dta1("columnValueCombined")} </h6></div>")
                End If
                html.Append("</div>")

                For i = 0 To dta1("results_data").Count - 1
                    Dim result = dta1("results_data")(i)
                    html.Append("<div class='row m-0 py-3 border-bottom align-items-end'>")
                    html.Append("<div class='col-12'>")
                    html.Append($"<div class='resultTable_column'><p class='mb-2'>")
                    If showIndex Then
                        html.Append($"<span class='number'>{result(0)}</span>")
                    End If
                    html.Append($"{result(1)}</p></div></div>")
                    If (dta1("results_data")(0)(4).ToString() = "rvIndividual" Or dta1("results_data")(0)(4).ToString() = "rvBoth") And individual Then
                        If (dta1("results_data")(0)(4).ToString() = "rvBoth") Then
                            html.Append("<div class='col-6'>")
                        Else
                            html.Append("<div class='col-12'>")
                        End If

                        html.Append($"<div class='resultTable_column'><div class='mobile_progress d-flex align-items-center'><div class='progress w-100{clsdnone}'>")
                        If result(2).ToString() = "" Or result(2).ToString() = "NaN" Then
                            html.Append($"<div class='progress-bar bg_green' role='progressbar' style='width: 0%;' aria-valuenow='100' aria-valuemin='0' aria-valuemax='{Math.Round(maxCombinedResult * 100, 2)}'></div>")
                        Else
                            html.Append($"<div class='progress-bar bg_green' role='progressbar' style='width: {If(result(2).ToString() <> "" And result(2).ToString() <> "0" And result(2).ToString() <> "NaN" And maxCombinedResult > 0, Math.Round(Decimal.Parse((result(2).ToString()), System.Globalization.NumberStyles.Any) * 100 / Math.Round(maxCombinedResult * 100, 2) * 100, 2), 00.00)}%;' aria-valuenow='{If(result(2).ToString() <> "" And result(2).ToString() <> "NaN", Math.Round(Decimal.Parse((result(2).ToString()), System.Globalization.NumberStyles.Any) * 100, 2), 0.00)}' aria-valuemin='0' aria-valuemax='{Math.Round(maxCombinedResult * 100, 2)}'></div>")
                        End If
                        html.Append("</div>")
                        If (dta1("results_data")(0)(4).ToString() = "rvIndividual" Or dta1("results_data")(0)(4).ToString() = "rvBoth") And individual Then
                            html.Append($"<h6 class='text_green mb-0 ms-3'>{If(result(2).ToString() <> "" And result(2).ToString() <> "NaN", Math.Round(Decimal.Parse((result(2).ToString()), System.Globalization.NumberStyles.Any) * 100, 2), 0.00)}%</h6>")
                        End If

                        html.Append("</div></div>")
                        html.Append("</div>")
                    End If
                    If (dta1("results_data")(0)(4).ToString() = "rvGroup" Or dta1("results_data")(0)(4).ToString() = "rvBoth") And individual Then
                        If (dta1("results_data")(0)(4).ToString() = "rvBoth") Then
                            html.Append("<div Class='col-6'>")
                        Else
                            html.Append("<div Class='col-12'>")
                        End If
                        html.Append($"<div class='resultTable_column'><div class='d-flex align-items-center'><div class='progress w-100{clsdnone}'>")
                        If (dta1("results_data")(0)(4).ToString() = "rvGroup" Or dta1("results_data")(0)(4).ToString() = "rvBoth") And combined And ShowBars Then
                            html.Append($"<div class='progress-bar bg_blue' role='progressbar' style='width: {If(result(3).ToString() <> "" And result(3).ToString() <> "0" And result(3).ToString() <> "NaN", Math.Round(Convert.ToDecimal(result(3)) * 100 / Math.Round(maxCombinedResult * 100, 2) * 100, 2), 0.00)}%;' aria-valuenow='{If(result(3).ToString() <> "" And result(3).ToString() <> "0" And result(3).ToString() <> "NaN", Math.Round(Convert.ToDecimal(result(3)) * 100, 2), 0.00)}' aria-valuemin='0' aria-valuemax='{Math.Round(maxCombinedResult * 100, 2)}'></div>")
                        End If
                        html.Append("</div>")
                        If (dta1("results_data")(0)(4).ToString() = "rvGroup" Or dta1("results_data")(0)(4).ToString() = "rvBoth") And combined Then
                            html.Append($"<h6 class='text_blue mb-0 ms-3'>{If(result(3).ToString() <> "" And result(3).ToString() <> "NaN", Math.Round(Decimal.Parse((result(3).ToString()), System.Globalization.NumberStyles.Any) * 100, 2), 0.00)}%</h6>")
                        End If

                        html.Append("</div></div>")
                        html.Append("</div>")
                    End If
                    html.Append("</div>")
                Next
                html.Append("</div></div>")
                html.Append("<div class='col-lg-6 d-lg-none'><div class='sorting_togle min-ht'><div class='table_headingwrt'><h5 id='headText'></h5></div>")
                html.Append("<div class='sort_bubble'>")
                html.Append("<div class='row'>")
                If showNormalization Then
                    If dta1("CurrentNode")(1).ToString().ToUpper() <> "objectives".ToUpper() Then
                        html.Append("<div class='col-6'><label>Normalization:</label><select id='mlddlNormalization' onchange=onSortNormalizationChange('mlddlNormalization') class='form-select'>")
                        html.Append("<option value='1'>Normalized</Option>")
                        html.Append("<option value='2'>% of Maximum</Option>")
                        html.Append("<option value='3'>Unnormalized</Option>")
                        html.Append("</select>")
                        html.Append("</div>")
                    End If
                End If
                html.Append("<div class='col-6'><label>Sort By:</label><select id='mlsortNormalization' onchange=onSortNormalizationChange('mlsortNormalization') class='form-select'>")
                If showIndex Then
                    html.Append("<option value='nodeID'>Index</Option>")
                End If
                html.Append("<option value='nodeName'>Name</Option>")
                If (dta1("results_data")(0)(4).ToString() = "rvIndividual" Or dta1("results_data")(0)(4).ToString() = "rvBoth") And individual Then
                    html.Append("<option value='yourResults'>Your Results</Option>")
                End If
                If (dta1("results_data")(0)(4).ToString() = "rvGroup" Or dta1("results_data")(0)(4).ToString() = "rvBoth") And combined Then
                    html.Append($"<option value='combine'>{dta1("columnValueCombined")}</Option>")
                End If
                html.Append("</select></div>")
                html.Append("</div></div></div></div>")
                'for desktop view
                'html.Append("<div class=''")v
                html.Append("<div class='resultTable'>")
                If (dta1("JudgmentType").ToString() = "atAllPairwise" Or dta1("JudgmentType").ToString() = "atPairwise") Then
                    If dta1("InconsistencyRatioStatus").ToString() = "True" Then
                        html.Append("<div class='table-responsive d-none d-lg-block global_resulttable clsLocalResult a'>")
                    Else
                        html.Append("<div class='table-responsive d-none d-lg-block global_resulttable clsLocalResult b'>")
                    End If
                ElseIf Not CBool(dta1("CurrentNode")(2)) Then
                    html.Append("<div class='table-responsive d-none d-lg-block global_resulttable tblStructure'>")
                Else
                    html.Append("<div class='table-responsive d-none d-lg-block global_resulttable tblNonStructure'>")
                End If
                If individual Or combined Then
                    html.Append("<table class='table' id='tblLocalResults'>")
                    html.Append("<thead>")
                    html.Append("<tr>")
                    If showIndex Then
                        'html.Append("<th scope='col' class='prm-color' onclick=sort_Results('nodeID')>No <i class='fas fa-sort ms-2'></i></th>")
                        html.Append("<th scope='col' class='text_blue ' onclick=sort_Results('nodeID')>No <i class='fas fa-sort ms-2'></i></th>")
                    End If
                    html.Append($"<th scope='col' class='text_blue Capitalize-first-letter-ofeach-word' onclick=sort_Results('nodeName')>{If(nameAlternatives IsNot Nothing, Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(nameAlternatives.ToLower()), "")} <i class='fas fa-sort ms-2'></i></th>")
                    If (dta1("results_data")(0)(4).ToString() = "rvIndividual" Or dta1("results_data")(0)(4).ToString() = "rvBoth") And individual Then
                        html.Append("<th scope='col' class='text_green text-center' onclick=sort_Results('yourResults')>Your Results <i class='fas fa-sort ms-2'></i></th>")
                    End If
                    If (dta1("results_data")(0)(4).ToString() = "rvGroup" Or dta1("results_data")(0)(4).ToString() = "rvBoth") And combined Then
                        'html.Append("<th scope='col' class='prm-color'>Combined Results <i class='fas fa-sort ms-2'></i></th>")
                        html.Append($"<th scope='col' class='text_blue text-center' onclick=sort_Results('combine')>{dta1("columnValueCombined")} <i class='fas fa-sort ms-2'></i></th>")
                    End If
                    If ShowBars Then
                        html.Append("<th scope='col' class='text_blue'>Bar Graph</th>")
                    End If
                    html.Append("</tr>")
                    html.Append("</thead>")
                    html.Append("<tbody>")
                    For i = 0 To dta1("results_data").Count - 1
                        Dim result = dta1("results_data")(i)
                        html.Append("<tr>")
                        If showIndex Then
                            html.Append($"<td>{result(0)}</td>")
                        End If
                        html.Append($"<td >{result(1)}</td>")
                        If (dta1("results_data")(0)(4).ToString() = "rvIndividual" Or dta1("results_data")(0)(4).ToString() = "rvBoth") And individual Then
                            html.Append($"<td class='br-color text-center text_green' >{If(result(2).ToString() <> "" And result(2).ToString() <> "NaN", Math.Round(Decimal.Parse((result(2).ToString()), System.Globalization.NumberStyles.Any) * 100, 2), 0.00)}%</td>")
                        End If
                        If (dta1("results_data")(0)(4).ToString() = "rvGroup" Or dta1("results_data")(0)(4).ToString() = "rvBoth") And combined Then
                            html.Append($"<td class='hl-color text-center text_blue'>{If(result(3).ToString() <> "" And result(3).ToString() <> "NaN", Math.Round(Convert.ToDecimal(result(3)) * 100, 2), 0.00)}%</td>")
                        End If
                        If ShowBars Then
                            html.Append($"<td>")
                            If (dta1("results_data")(0)(4).ToString() = "rvIndividual" Or dta1("results_data")(0)(4).ToString() = "rvBoth") And individual Then
                                html.Append($"<div class='progress' data-toggle='tooltip' style='height: 10px; margin-bottom: 4px; '  title='{If(result(2).ToString() <> "" And result(2).ToString() <> "NaN", Math.Round(Decimal.Parse((result(2).ToString()), System.Globalization.NumberStyles.Any) * 100, 2), 0.00)}%'>")
                                html.Append($"<div class='progress-bar bg_green' role='progressbar' style='width: {If(result(2).ToString() <> "" And result(2).ToString() <> "0" And maxCombinedResult > 0 And result(2).ToString() <> "NaN", Math.Round(Decimal.Parse((result(2).ToString()), System.Globalization.NumberStyles.Any) * 100 / Math.Round(maxCombinedResult * 100, 2) * 100, 2), 0.00)}%;' aria-valuenow='{If(result(2).ToString() <> "" And result(2).ToString() <> "NaN", Math.Round(Decimal.Parse((result(2).ToString()), System.Globalization.NumberStyles.Any) * 100, 2), 0.00)}' aria-valuemin='0' aria-valuemax='{Math.Round(maxCombinedResult * 100, 2)}' data-toggle='tooltip'  title='{If(result(2).ToString() <> "" And result(2).ToString() <> "NaN", Math.Round(Decimal.Parse((result(2).ToString()), System.Globalization.NumberStyles.Any) * 100, 2), 0.00)}%' ></div>")
                                html.Append("</div>")
                            End If
                            If (dta1("results_data")(0)(4).ToString() = "rvGroup" Or dta1("results_data")(0)(4).ToString() = "rvBoth") And combined Then
                                html.Append($"<div class='progress' style='height: 10px;'  data-toggle='tooltip' title='{If(result(3).ToString() <> "" And result(3).ToString() <> "NaN", Math.Round(Convert.ToDecimal(result(3)) * 100, 2), 0.00)}%' >")
                                html.Append($"<div class='progress-bar bg_blue ' role='progressbar' style='width: {If(result(3).ToString() <> "" And result(3).ToString() <> "NaN", Math.Round(Decimal.Parse((result(3).ToString()), System.Globalization.NumberStyles.Any) * 100 / Math.Round(maxCombinedResult * 100, 2) * 100, 2), 0.00)}%;' aria-valuenow='{If(result(3).ToString() <> "" And result(3).ToString() <> "NaN", Math.Round(Decimal.Parse((result(3).ToString()), System.Globalization.NumberStyles.Any) * 100, 2), 0.00)}' aria-valuemin='0' aria-valuemax='{Math.Round(maxCombinedResult * 100, 2)}'  data-toggle='tooltip' title='{If(result(3).ToString() <> "" And result(3).ToString() <> "NaN", Math.Round(Decimal.Parse((result(3).ToString()), System.Globalization.NumberStyles.Any) * 100, 2), 0.00)}%'></div>")
                                html.Append("</div>")
                            End If
                        End If

                        html.Append("</td>")
                        html.Append("</tr>")
                    Next
                    html.Append("</tbody>")
                    html.Append("</table>")
                End If
                html.Append("</div> ")
                html.Append("</div>")
                html.Append("</div>")
                If dta1("InconsistencyRatioStatus").ToString() = "True" And (dta1("JudgmentType").ToString() = "atAllPairwise" Or dta1("JudgmentType").ToString() = "atPairwise") Then
                    html.Append($"<h4 class='mt-3 text-blue text-center p-0'>Inconsistency Ratio: <span class='spnInconsistencyRatio'>{Math.Round(Convert.ToDecimal(dta1("InconsistencyRatio")), 2)}</span> </h4>")
                End If
                html.Append("<div class='text-center mt-3'>")
                If dta1("JudgmentType").ToString() = "atPairwise" Or dta1("JudgmentType").ToString() = "atAllPairwise" Then
                    If CBool(dta1("InconsistencyRatioStatus")) Then
                        html.Append("<button class='bg_green btn btn-clipboard mb-0 text-light' type='button' onclick=bindDynamicHtml('show_satisfactory')> Click here if these priorities or the Inconsistency are not satisfactory</button>")
                    Else
                        html.Append("<button class='bg_green btn btn-clipboard mb-0 text-light' type='button' onclick=bindDynamicHtml('redo_judgement');> Click here if you would like to redo a Judgment for one pair of elements</button>")
                    End If
                End If
                'html.Append("</div>")
                html.Append("</div>")
                html.Append("</div>")
                'html.Append("</div>")
                html.Append("</div>")
                html.Append("</div>")

                'show_satisfactory
                html.Append("<div id='divshow_satisfactory' class='divHeader2 d-none'>")
                If dta1("showTitle").ToString() = "True" Then
                    html.Append("<div class='col-12'>")
                    html.Append("<div Class='text-center' style='display: none' id='titleDiv2'>")
                    html.Append($"<p class='font-size-16 prm-color text-center'>{step_task}</p>")
                    html.Append($"<h3 class='prm-color'>{dta1("resultsTitle")}</h3>")
                    html.Append("</div>")

                    html.Append("</div>")
                    'html.Append("</div>")
                End If
                html.Append("<div class='col-lg-6 offset-lg-3 mt-4'>")
                html.Append("<div class='text-center buttons_div px-2'>")
                ''Task 253
                html.Append("<button type='button' class='bg_green btn btn-clipboard mb-0 text-light' onclick=bindDynamicHtml('Judgment_table')>Click here to investigate comparison matrix and inconsistencies</button>")
                html.Append("<p> </p>")
                html.Append("<button type='button' class='bg_green btn btn-clipboard mb-0 text-light' onclick='reviewJudgment()'> Click here to review all judgments</button>")
                html.Append("<p class='pt-3 d-none'> if you think the priorities are not reasonable then:</p>")
                html.Append("<button type='button' class='bg_green btn btn-clipboard mb-0 text-light d-none' onclick=bindDynamicHtml('redo_judgement')> Click here if you would like to redo a judgment for one pair of elements</button>")
                html.Append("<p> </p>")
                html.Append("<button type='button' class='btn foot_btn p-2' onclick='bindDynamicHtml()'>Cancel</button>")
                html.Append("</div>")
                html.Append("</div>")
                html.Append("</div>")

                'redo a judgment for one pair of elements
                html.Append($"<div id='divredo_judgement' class='divHeader3 d-none' style='padding-top:0'>")
                'html.Append("<div style='display: block;' class='page_heading_section'>")
                If dta1("showTitle").ToString() = "True" Then
                    html.Append("<div class='container'><div class='row'><div class='col-12'>")
                    html.Append("<div Class='page-title-box text-center' id='titleDiv3'>")
                    html.Append($"<p class='font-size-18 prm-color text-center'>{step_task}</p>")
                    html.Append($"<h3 class='prm-color'>{dta1("resultsTitle")}</h3>")
                    html.Append("</div>")

                    html.Append("</div>")
                    'html.Append("</div>")
                End If

                'for mobile view
                html.Append("<div class='d-block d-lg-none'><div class='resultTable_mobile' id='divredoJudgement'>")

                html.Append("<div class='border-bottom pb-2 pt-3 m-0 row local_result_head'>")
                If (dta1("results_data")(0)(4).ToString() = "rvIndividual" Or dta1("results_data")(0)(4).ToString() = "rvBoth") And individual Then
                    html.Append("<div class='col-6'><h6 class='text_green text-center mobile_progress '>Your Results</h6></div>")
                End If
                If (dta1("results_data")(0)(4).ToString() = "rvGroup" Or dta1("results_data")(0)(4).ToString() = "rvBoth") And combined Then
                    html.Append($"<div class='col-6'><h6 class='text_blue text-center'>{dta1("columnValueCombined")} </h6></div>")
                End If
                html.Append("</div>")

                For i = 0 To dta1("results_data").Count - 1
                    Dim result = dta1("results_data")(i)
                    html.Append("<div class='row m-0 py-3 border-bottom align-items-end'>")

                    html.Append("<div class='col-12'>")
                    html.Append($"<div class='resultTable_column'><p class='mb-2'>")
                    html.Append($"<input name='{result(5)}' Class='checkbox-custom me-3 mt-1 redoCheckbox' onchange=LocalselectCheckbox('mcheckbox_items{result(0)}','{result(5)}') type='checkbox' id='mcheckbox_items{result(0)}' value='{result(5)}'>")
                    html.Append($"<span class='number'>{result(0)}</span>{result(1)}</p></div>")

                    html.Append("</div>")

                    If (dta1("results_data")(0)(4).ToString() = "rvIndividual" Or dta1("results_data")(0)(4).ToString() = "rvBoth") And individual Then
                        If (dta1("results_data")(0)(4).ToString() = "rvBoth") Then
                            html.Append("<div class='col-6'>")
                        Else
                            html.Append("<div class='col-12'>")
                        End If

                        html.Append($"<div class='resultTable_column'><div class='mobile_progress d-flex align-items-center'><div class='progress w-100{clsdnone}'>")
                        If result(2).ToString() = "" Or result(2).ToString() = "NaN" Then
                            html.Append($"<div class='progress-bar bg_green' role='progressbar' style='width: 0%;' aria-valuenow='100' aria-valuemin='0' aria-valuemax='{Math.Round(maxCombinedResult * 100, 2)}'></div>")
                        Else
                            html.Append($"<div class='progress-bar bg_green' role='progressbar' style='width: {If(result(2).ToString() <> "" And result(2).ToString() <> "0" And result(2).ToString() <> "NaN" And maxCombinedResult > 0, Math.Round(Decimal.Parse((result(2).ToString()), System.Globalization.NumberStyles.Any) * 100 / Math.Round(maxCombinedResult * 100, 2) * 100, 2), 00.00)}%;' aria-valuenow='{If(result(2).ToString() <> "" And result(2).ToString() <> "NaN", Math.Round(Decimal.Parse((result(2).ToString()), System.Globalization.NumberStyles.Any) * 100, 2), 0.00)}' aria-valuemin='0' aria-valuemax='{Math.Round(maxCombinedResult * 100, 2)}'></div>")
                        End If
                        html.Append("</div>")
                        If (dta1("results_data")(0)(4).ToString() = "rvIndividual" Or dta1("results_data")(0)(4).ToString() = "rvBoth") And individual Then
                            html.Append($"<h6 class='text_green mb-0 ms-3'>{If(result(2).ToString() <> "" And result(2).ToString() <> "NaN", Math.Round(Decimal.Parse((result(2).ToString()), System.Globalization.NumberStyles.Any) * 100, 2), 0.00)}%</h6>")
                        End If

                        html.Append("</div></div>")
                        html.Append("</div>")
                    End If
                    If (dta1("results_data")(0)(4).ToString() = "rvGroup" Or dta1("results_data")(0)(4).ToString() = "rvBoth") And individual Then
                        If (dta1("results_data")(0)(4).ToString() = "rvBoth") Then
                            html.Append("<div Class='col-6'>")
                        Else
                            html.Append("<div Class='col-12'>")
                        End If
                        html.Append($"<div class='resultTable_column'> <div class='d-flex align-items-center'> <div class='progress w-100{clsdnone}'>")
                        If (dta1("results_data")(0)(4).ToString() = "rvGroup" Or dta1("results_data")(0)(4).ToString() = "rvBoth") And combined And ShowBars Then
                            html.Append($"<div class='progress-bar bg_blue' role='progressbar' style='width: {If(result(3).ToString() <> "" And result(3).ToString() <> "0" And result(3).ToString() <> "NaN", Math.Round(Convert.ToDecimal(result(3)) * 100 / Math.Round(maxCombinedResult * 100, 2) * 100, 2), 0.00)}%;' aria-valuenow='{If(result(3).ToString() <> "" And result(3).ToString() <> "0" And result(3).ToString() <> "NaN", Math.Round(Convert.ToDecimal(result(3)) * 100, 2), 0.00)}' aria-valuemin='0' aria-valuemax='{Math.Round(maxCombinedResult * 100, 2)}'></div>")
                        End If
                        html.Append("</div>")
                        If (dta1("results_data")(0)(4).ToString() = "rvGroup" Or dta1("results_data")(0)(4).ToString() = "rvBoth") And combined Then
                            html.Append($"<h6 class='text_blue mb-0 ms-3'>{If(result(3).ToString() <> "" And result(3).ToString() <> "NaN", Math.Round(Decimal.Parse((result(3).ToString()), System.Globalization.NumberStyles.Any) * 100, 2), 0.00)}%</h6>")
                        End If

                        html.Append("</div></div>")
                        html.Append("</div>")
                    End If

                    html.Append("</div>")
                Next
                html.Append("</div></div>")

                'for desktop view
                html.Append("<div class='col-md-10 offset-md-1'>")
                html.Append("<div Class='page-title-box text-center resultTable'>")
                html.Append("<div class='table-responsive d-none d-lg-block table_Scroll global_resulttable clsLocalResult b'>")
                If individual Or combined Then
                    html.Append("<table class='table table-hover mb-0 progress_table text-center'>")
                    html.Append("<thead>")
                    html.Append("<tr>")
                    html.Append("<th></th>")
                    If showIndex Then
                        html.Append("<th scope='col' class='text_blue text-start'>No <i class='fas fa-sort ms-2'></i></th>")
                    End If
                    html.Append("<th scope='col' class='text_blue text-start'>Name <i class='fas fa-sort ms-2'></i></th>")
                    If (dta1("results_data")(0)(4).ToString() = "rvIndividual" Or dta1("results_data")(0)(4).ToString() = "rvBoth") And individual Then
                        html.Append("<th scope='col' class='text_green text-center'>Your Results <i class='fas fa-sort ms-2'></i></th>")
                    End If
                    If (dta1("results_data")(0)(4).ToString() = "rvGroup" Or dta1("results_data")(0)(4).ToString() = "rvBoth") And combined Then
                        'html.Append("<th scope='col' class='prm-color'>Combined Results <i class='fas fa-sort ms-2'></i></th>")
                        html.Append($"<th scope='col' class='text_blue text-center'>{dta1("columnValueCombined")} <i class='fas fa-sort ms-2'></i></th>")
                    End If
                    If ShowBars Then
                        html.Append("<th scope='col' class='text_blue text-start'>Bar Graph</th>")
                    End If
                    html.Append("</tr>")
                    html.Append("</thead>")
                    html.Append("<tbody>")
                    For i = 0 To dta1("results_data").Count - 1
                        Dim result = dta1("results_data")(i)
                        html.Append("<tr>")
                        html.Append("<td class='text-start'>")
                        html.Append("<div Class='custom-checkbox'>")
                        html.Append($"<input name='{result(5).ToString()}' Class='checkbox-custom redoCheckbox' onchange=LocalselectCheckbox('checkbox_items{result(0)}','{result(5).ToString()}') id='checkbox_items{result(0).ToString()}' value='{result(5).ToString()}' type='checkbox'>")
                        html.Append($"<Label Class='checkbox-custom-label' for='checkbox_items{result(0)}'> </label>")
                        html.Append("</div>")
                        html.Append("</td>")
                        If showIndex Then
                            html.Append($"<td class='text-start'>{result(0)}</td>")
                        End If
                        html.Append($"<td class='text-start'>{result(1)}</td>")
                        If (dta1("results_data")(0)(4).ToString() = "rvIndividual" Or dta1("results_data")(0)(4).ToString() = "rvBoth") And individual Then
                            html.Append($"<td class='br-color text-center text_green'>{If(result(2).ToString() <> "" And result(2).ToString() <> "NaN", Math.Round(Decimal.Parse((result(2).ToString()), System.Globalization.NumberStyles.Any) * 100, 2), 0.00)}%</td>")
                        End If
                        If (dta1("results_data")(0)(4).ToString() = "rvGroup" Or dta1("results_data")(0)(4).ToString() = "rvBoth") And combined Then
                            html.Append($"<td class='hl-color text-center text_blue'>{If(result(3).ToString() <> "" And result(3).ToString() <> "NaN", Math.Round(Convert.ToDecimal(result(3)) * 100, 2), 0.00)}%</td>")
                        End If
                        If ShowBars Then
                            html.Append($"<td class='text-start'>")
                            html.Append("<div class='progress' style='height: 10px;'>")
                            html.Append($"<div class='progress-bar bg_blue' role='progressbar' style='width: {If(result(2).ToString() <> "" And result(2).ToString() <> "0" And maxCombinedResult > 0 And result(2).ToString() <> "NaN", Math.Round(Decimal.Parse((result(2).ToString()), System.Globalization.NumberStyles.Any) * 100 / Math.Round(maxCombinedResult * 100, 2) * 100, 2), 0.00)}%;' aria-valuenow='{If(result(2).ToString() <> "" And result(2).ToString() <> "NaN", Math.Round(Decimal.Parse((result(2).ToString()), System.Globalization.NumberStyles.Any) * 100, 2), 0.00)}' aria-valuemin='0' aria-valuemax='{Math.Round(maxCombinedResult * 100, 2)}'></div>")
                            html.Append("</div>")
                            html.Append("<div class='progress' style='height: 10px;'>")
                            html.Append($"<div class='progress-bar bg_green' role='progressbar' style='width: {If(result(3).ToString() <> "" And result(3).ToString() <> "0" And result(3).ToString() <> "NaN", Math.Round(Convert.ToDecimal(result(3)) * 100 / Math.Round(maxCombinedResult * 100, 2) * 100, 2), 0.00)}%;' aria-valuenow='{If(result(3).ToString() <> "" And result(3).ToString() <> "0" And result(3).ToString() <> "NaN", Math.Round(Convert.ToDecimal(result(3)) * 100, 2), 0.00)}' aria-valuemin='0' aria-valuemax='{Math.Round(maxCombinedResult * 100, 2)}'></div>")
                            html.Append("</div>")
                            html.Append("</td>")
                        End If
                        html.Append("</tr>")
                    Next
                    html.Append("</tbody>")
                    html.Append("</table>")
                End If
                html.Append("</div>")
                If dta1("InconsistencyRatioStatus").ToString() = "True" And (dta1("JudgmentType").ToString() = "atAllPairwise" Or dta1("JudgmentType").ToString() = "atPairwise") Then
                    html.Append($"<h4 class='my-2 prm-color text-center'>Inconsistency Ratio: <span class='spnInconsistencyRatio'>{Math.Round(Convert.ToDecimal(dta1("InconsistencyRatio")), 2)}</span> </h4>")
                End If
                html.Append("<div Class='mt-3 row'>")
                html.Append("<div Class='col-lg-8'>")
                html.Append("<textarea rows='2' Class='form-control mb-3' readonly>Select a pair of elements (by clicking the checkbox on left) for which you think: One has too high a priority, and the other has too low of a priority.</textarea>")
                html.Append("</div>")
                html.Append("<div Class='col-lg-4 text-center text-md-end'>")
                html.Append($"<button type='button' class='bg_green btn text-white mx-2 mb-2 mb-md-0' id='btnReevaluate' onclick=reEvaluate('{parentnodeID}','{current_step}') disabled> Re-evaluate</button>")
                html.Append("<button type='button' class='bg_green btn btn-clipboard mb-0 text-light' onclick=bindDynamicHtml('cancel')> Cancel</button>&nbsp;")
                html.Append("</div>")
                html.Append("</div>")
                html.Append("</div>")
                html.Append("</div>")
                html.Append("</div>")
                html.Append("</div>")
                html.Append("</div>")
                html.Append("</div>")

                'Judgment Table
                html.Append("<div id='divJudgment_table' class='d-none'>")
                'html.Append("<div class='col-12'>")
                html.Append("<div class='page_heading_section' id='titleDiv4'><div class='container'><div class='row align-items-center'><div class='col-md-2'><div class='heading_content'>")
                html.Append("<p class='heading-content-title'><span>Judgment Table</span></p>")
                html.Append("</div></div>")
                html.Append("<div class='col-md-10'><div class='info_content judgement-content-info'><div class='info_content_wrapper'>")
                html.Append("<div class='info_text'><p>")
                html.Append("If elements in the table are sorted from high to low priority, than judgments should generally be increase in any row from left to right, and in any column from bottom to top. Althrough exceptions to this pattern are valid, thay may indicate a judgment that should be examined for accuracy.")
                html.Append("</p></div>")
                html.Append("</div></div></div>")
                html.Append("</div></div></div>")

                If (screenCheck.isMobileBrowserClient(Request)) Then

                    html.Append("<div class='judgment_table_header'>")
                    html.Append("<div Class='th_icon'><a class='judgeHeader activeHeader judgeHeader1' onclick='saveOBPriority(true, 1)'><i class='fa fa-sort-amount-desc' aria-hidden='true'></i></a></div>")
                    html.Append("<div class='th_icon'><a class='judgeHeader judgeHeader2' onclick='saveOBPriority(false, 2)'><i class='fa fa-list-ol' aria-hidden='true'></i></a></div>")
                    html.Append("<div class='th_icon'><a class='judgeHeader judgeHeader3' onclick='copyJudgmentTable()'><i class='fa fa-files-o' aria-hidden='true'></i></a></div>")
                    html.Append("<div class='th_icon'><a class='judgeHeader judgeHeader4' onclick='open_pasteModal(4)'><i class='fa fa-clipboard' aria-hidden='true'></i></a></div>")
                    html.Append("<div class='th_icon'><a class='judgeHeader judgeHeader5' onclick=MatrixOperation('invertThis')><i class='fa fa-repeat' aria-hidden='true'></i></a></div>")
                    html.Append("<div class='th_icon'><a class='judgeHeader judgeHeader6' ><i class='fa fa-eye' onclick='showEyeDropDown()' aria-hidden='true'></i></a></div>")

                    html.Append("<div class='th_icon'><a class='judgeHeader judgeHeader7' onclick=MatrixOperation('invertCurrent')><i class='fa fa-minus' aria-hidden='true'></i></a></div>")
                    html.Append("<div class='th_icon'><a class='judgeHeader judgeHeader8' onclick=MatrixOperation('invert')><i class='fas fa-exchange-alt'></i></a></div>")
                    html.Append("<div id='idropdown' style='display:none;' class='eyedropdown'><ul><li class='cleckbox-list'><label><input id='chkRank' type='checkbox'/> Rank of consistency<label></li><li class='cleckbox-list'><label><input name='chkBestFit' Class='checkbox-custom' id='chkBestFit' value='BestFit' type='checkbox'> Best fit<label></li><li class='cleckbox-list'><label><input id='sbpriority' onclick='msortbypriority(this)'  type='checkbox'/> Order by priorty<label></li><li class='cleckbox-list'><label><input name='inconsistency' id='reviewjudgment' checked type='radio' />Re-evaluate a pair<label></li><li class='cleckbox-list'><label><input name='inconsistency' id='changejudgment' type='radio'/> Make changes on this screen<label></li></ul></div>")
                    html.Append("</div>")
                End If
                html.Append("<div class='container'><div class='row'>")
                html.Append("<div class='col-md-12 col-lg-12'><div class='row position-relative'><div class='col-md-12 main-content-data-wrapper'>")
                html.Append("<div class='main-header-wrapper'>")
                html.Append("<div class='main-header-info'>")
                html.Append("<div class='judgement-options-info'>")
                html.Append("<div class='row'>")


                html.Append("<div class='col-md-5 col-lg-5'>")
                html.Append("<div class='row  align-items-center'>")
                html.Append("<div class='col-md-8 col-lg-7'>")


                html.Append("<div class='check-sort-options-info'><div class='check-options-info'>")
                html.Append("<div class='form-group'><input name='chkRank' Class='checkbox' id='chkRank' value='Rank' type='checkbox'>")
                html.Append("<label for='chkRank' class='checkbox_label'>Rank by Inconsistency</label></div>")
                html.Append("<div class='form-group'><input name='chkBestFit' Class='checkbox' id='chkBestFit' value='BestFit' type='checkbox'>")
                html.Append("<label for='chkBestFit' class='checkbox_label'>Best fit</label></div>")
                html.Append("</div></div>")
                html.Append("</div>")
                'html.Append("</div>")

                html.Append("<div class='col-md-4 col-lg-5'><div class='select-options-info'><select onchange='SortByPriorityAndOrder(this)' class='form-select'>")
                html.Append("<option value='False'>Sort By Original order</option><option value='True'>Sort By Priority</option>")
                html.Append("</select></div></div>")

                html.Append("</div></div>")


                html.Append("<div class='col-md-7 col-lg-7'><div class='row justify-content-end align-items-center'><div class='col-md-12 col-lg-12'><div class='legends-check-wrapper'>")
                '    html.Append("<div class='legends-colors-wrapper'>
                '                                        <div class='legends-colors-info'>
                '                                            <div class='select-legends-info'>
                '                                                <div class='legend-text-info'>Pairwise Verbal Scale</div>
                '                                                <div class='dropdown-icon-info legends-info'><img src='../../img/icon/dropdown-arrow-icon.svg' alt='dropdown-icon'></div>
                '                                            </div>                                                        
                '                                        </div>
                '                                        <div class='show-legend-wrapper' style='display: none;'>                                                        
                '                                            <div class='show-legend-info'>
                '                                                <div class='row'>
                '                                                    <div class='col-md-12'>
                '					<div class='sub-legend-info'>                                                                        
                '                                                            <ul>
                '                                                                <li class='main-text-info'>1. Equal</li>
                '                                                                <li class='sub-text-info'>2. Equal to Moderate</li>
                '                                                                <li class='main-text-info'>3. Moderate</li>
                '                                                                <li class='sub-text-info'>4. Moderate to Strong</li>
                '                                                                <li class='main-text-info'>5. Strong</li>
                '                                                                <li class='sub-text-info'>6. Strong to  Very strong</li>
                '                                                                <li class='main-text-info'>7. Very strong</li>
                '                                                                <li class='sub-text-info'>8.  Very strong to Extreme</li>
                '                                                                <li class='main-text-info'>9. Extreme</li>
                '                                                            </ul>
                '                                                        </div>

                '                                                    </div>

                '                                                </div>
                '                                            </div> 
                '                                        </div>
                '                                    </div>")
                '    html.Append("<div class='legends-description-wrapper'>
                '	<div class='legends-toogle-info'>
                '		<img src='../../img/icon/legends.svg' alt='legends'>
                '		Legend
                '		<img src='../../img/icon/down-arrow-bordered-icon.svg' alt='down-arrow'>
                '	</div>
                '	<div class='legends-description-info' style='display: none;'>
                '		<ul>                                               
                '			<li class='rank-legend-info'> <span class='box-info rank-boxcolor-info'></span> Rank</li>
                '			<li class='inverted-legend-info'> <span class='box-info inverted-boxcolor-info'></span> Inverted</li>
                '			<li class='bestfit-legend-info'> <span class='box-info bestfit-boxcolor-info'></span> Best Fit</li>
                '			<li class='revaluate-legend-info'> <span class='box-info revaluate-boxcolor-info'></span> Revaluate</li>
                '		</ul>
                '	</div>
                '</div>")

                html.Append("<div Class='radio-options-info'>")
                html.Append("<div class='form-check'>")
                html.Append("<input type='radio' class='form-check-input' id='reviewjudgment' name='inconsistency' value='reevaluate a pair' checked />")
                html.Append("<label class='form-check-label' for='reviewjudgment'>Select Pair To Re-evaluate </label>")
                html.Append("</div>")
                html.Append("<div class='form-check'>")
                html.Append("<input type='radio' class='form-check-input' id='changejudgment' name='inconsistency' value='Make changes on this screen' />")
                html.Append("<label class='form-check-label' for='changejudgment'>Enter Changes In Judgment Cells</label>")
                html.Append("</div></div>")
                html.Append("<div Class='legends-pin-open-wrapper'><a href='#' class='openpinbtn' >	<div class='legends-toogle-info'><img src='../../img/icon/legends.svg' alt='legends'>Legend")
                'html.Append("<img src ='../../img/icon/pin-blue-icon.svg' class='pin-blue-icon-info' alt='pin-blue-icon'></div></a> </div>")
                html.Append("</div></a> </div></div></div></div></div>")

                html.Append("</div></div>")
                html.Append("</div></div>")

                html.Append("<div class='main-content-wrapper'> <div Class='main-content-info'><div class='judgement-table-info'>")
                html.Append($"<div class='table-responsive' {If(isPipeViewOnly, " style='pointer-events:none';", "")} id='divJugement'></div>")
                If dta1("InconsistencyRatioStatus").ToString() = "True" And (dta1("JudgmentType").ToString() = "atAllPairwise" Or dta1("JudgmentType").ToString() = "atPairwise") Then
                    html.Append($"<div class='judgement-result-wrapper'><div class='judgement-result-info text-center'><h4 class='my-1 prm-color text-center'>Inconsistency Ratio: <span class='spnInconsistencyRatio'>{Math.Round(Convert.ToDecimal(dta1("InconsistencyRatio")), 2)}</span> </h4></div></div>")
                End If
                html.Append("</div></div>")

                html.Append("</div></div>")
                'html.Append("<li class='revaluate-legend-info'> <span class='box-info revaluate-boxcolor-info'></span> Revaluate</li>")
                html.Append($"<div class='col-md-2 show-legend-and-scale-wrapper' style='display: none;'>
						<div id='legendandscalesidebar' class='legend-and-scale-sidebar'>														
							<div class='show-legend-wrapper'> 
								<div class='judgement-cells-help-info'>
									<div class='legend-text-info'>Cell info</div>
									<img src='../../img/Judgment-cell-info.svg' alt='help-data' class='dumny'>
								</div>  
                                <div class='legends-description-info mt-3'>
										<ul>                                               
											<li class='inverted-legend-info'> <span class='box-info inverted-boxcolor-info'></span> Inverted</li>
										</ul>
									</div>
							                                                  
								<div class='show-legend-info{DisplayNone}'>								
											<div class='sub-legend-info'>
												<div class='legend-text-info'>Pairwise Verbal Scale</div>                                                                                                                                                
												<ul>
													<li class='main-text-info'>1. Equal</li>
													<li class='sub-text-info'>2. Equal to Moderate</li>
													<li class='main-text-info'>3. Moderate</li>
													<li class='sub-text-info'>4. Moderate to Strong</li>
													<li class='main-text-info'>5. Strong</li>
													<li class='sub-text-info'>6. Strong to  Very strong</li>
													<li class='main-text-info'>7. Very strong</li>
													<li class='sub-text-info'>8.  Very strong to Extreme</li>
													<li class='main-text-info'>9. Extreme</li>
												</ul>
											</div>									
										
								</div>
								
							</div>
						</div>
					</div>")

                html.Append("</div></div>")

                html.Append("<div class='col-md-12 col-lg-12'>")
                html.Append("<div class='main-content-wrapper'>")

                html.Append("</div></div>")

                html.Append("<div class='col-md-12 col-lg-12'><div class='main-footer-wrapper'><div class='main-footer-info'><div class='judgement-footer-cta-wrapper'><div class='judgement-footer-cta-info'>")
                html.Append("<div class='judgement-cta-info'>")
                html.Append("<a href='javascript:void(0);' title='Copy' class='cta-info'> <img src='../../img/icon/copy-grey-icon.svg' onclick='copyJudgmentTable()' alt='copy-icon'></a>")

                html.Append("<a href='javascript:void(0);' title='Paste' class='cta-info'> <img src='../../img/icon/paste-icon.svg' onclick=open_pasteModal() alt='edit-icon'></a>")
                html.Append($"<a href='javascript:void(0);' title='Restore Judgment' id='btnRestoreJudgment' class='cta-info' {If(CBool(dta1("JudgmentsSaved")), "", " disabled")}> <img src='../../img/icon/refresh-grey-icon.svg' onclick=MatrixOperation('invertThis') alt='refresh-icon'></a>")
                html.Append("<button type='button' id='btnPriorities' Class='cta-info txt-cta-info' onclick='bindDynamicHtml()'> Priorities</button>")
                html.Append("<button type='button' id='btnInvertJudgment' onclick=MatrixOperation('invertCurrent') Class='cta-info txt-cta-info disabled'> Invert this Judgment</button>")
                html.Append("<button type='button' onclick=MatrixOperation('invert') Class='cta-info txt-cta-info'> Invert All Judgments</button>")
                html.Append("</div>")
                html.Append("</div></div></div></div></div>")

                html.Append("</div>")
                'html.Append("</div>")




                html.Append("</div>")
                html.Append("</div>")
                html.Append("</div>")
                html.Append("</div>")
            End If
            html.Append("</div>")
            'html.Append($"<textarea id='txtarClipBoardData' class='d-none'>{dta1("ClipBoardData")}</textarea>")
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

    Public Function RemoveHTML(HTMLstring As String) As String

        If Not HTMLstring.Contains("<") Then Return HTMLstring

        Dim DoRec As Boolean = False
        Dim textOut As String = ""
        Dim shtml = HTMLstring

        Dim SkipMe As Boolean = False
        Dim SkipMeTag As String = ""
        textOut &= shtml.Substring(0, shtml.IndexOf("<"))

        For l = 1 To HTMLstring.Length
            Dim tmp As String = Mid(HTMLstring, l, 1)

            ' Enable skip-me mode (for large blocks of non-readable code)
            If tmp = "<" And Mid(HTMLstring, l + 1, 6) = "script" Then SkipMe = True : SkipMeTag = "script" : DoRec = False
            If tmp = "<" And Mid(HTMLstring, l + 1, 5) = "style" Then SkipMe = True : SkipMeTag = "style" : DoRec = False

            ' If we're already in skip-me mode, then figure out iff it's time to exit it.
            If SkipMe = True Then
                If tmp = "<" And Mid(HTMLstring, l + 1, Len(SkipMeTag) + 1) = "/" + SkipMeTag Then
                    SkipMe = False
                    tmp = ""
                    l = l + Len(SkipMeTag) + 1
                    DoRec = False
                End If
            End If

            ' If we arent in skip-me mode, move on to handle parsing of the HTML content (pulling text out from in between tags)
            If SkipMe = False Then
                If tmp = ">" Then DoRec = True : textOut &= " " : tmp = ""
                If tmp = "<" Then DoRec = False : tmp = ""

                If DoRec = True Then
                    textOut &= tmp
                End If
            End If

        Next

        Return textOut
    End Function

End Class