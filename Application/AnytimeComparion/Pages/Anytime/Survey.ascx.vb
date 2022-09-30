Public Class Survey
    Inherits System.Web.UI.UserControl

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim App = CType(Session("App"), clsComparionCore)

        If App.ActiveUser Is Nothing Then
            Response.Redirect("~/login.aspx")
        End If
    End Sub

    Public Sub bindHtml(ByVal PipeParameters As Object, ByVal isPipeViewOnly As Boolean)
        If PipeParameters IsNot Nothing Then
            Dim html As StringBuilder = New StringBuilder()
            Dim dta = JsonConvert.SerializeObject(PipeParameters)
            Dim dta1 = JObject.Parse(dta)

            'Dim hdnSurveyAnswers As HiddenField = CType(Page.Form.FindControl("hdnSurveyAnswers"), HiddenField)
            'hdnSurveyAnswers.Value = dta1("SurveyAnswers").ToString()

            'Dim hdnSurveyPageQuestions As HiddenField = CType(Page.Form.FindControl("hdnSurveyPageQuestions"), HiddenField)
            'hdnSurveyPageQuestions.Value = dta1("SurveyPage")("Questions").ToString()

            html.Append("<div class='quiz-section'>")
            html.Append($"<div class='container'>{If(Convert.ToString(dta1("SurveyPage")("Title")).ToString().Trim() <> "", "<div class='heading col-md-8 offset-md-2'>" + Convert.ToString(dta1("SurveyPage")("Title")) + "</div>", "")}</div>")
            html.Append("<div class='quiz-wapper'>")
            html.Append($"<div class='container'{If(isPipeViewOnly, " style='pointer-events:none;'", "")}>")
            html.Append("<div class='col-md-8 offset-md-2 single-quiz'><div class='row'><div class='d-flex mb-2'><i class='text-danger me-1'>*</i> <span>(Required)</span></div>")

            For i = 0 To dta1("SurveyPage")("Questions").Count - 1
                Dim qType = dta1("SurveyPage")("Questions")(i)("Type").ToString()
                If qType = "1" Or qType = "2" Or qType = "14" Or qType = "15" Then
                    html.Append($"<div class='col-lg-12'><div class='question_body'>")
                Else
                    html.Append($"<div class='col-lg-12'><div class='question_body{If(qType = "0", " image_qestion", "")}'>")
                End If
                'If qType <> "12" And qType <> "13" And qType <> "14" Then
                html.Append($"<div class='d-flex align-items-start mb-2'>")
                If dta1("QuestionNumbering").Count > 0 Then
                    If qType <> "0" Then
                        html.Append($"<h5 class=''>{If(dta1("QuestionNumbering").Count > 0, dta1("QuestionNumbering")(i).ToString() + ".", "")} &nbsp;</h5>")
                    Else
                        html.Append($"<h5 class=''></h5>")
                    End If
                End If
                Dim title As String = dta1("SurveyPage")("Questions")(i)("Text").ToString()
                title = title.Replace(vbLf, "<br>")

                If i = 0 Then
                    Dim firstIndex As Integer = title.IndexOf("<p>") + 3
                    Dim m As Match = Regex.Match(title, "<p>")
                    If m.Success Then
                        'Dim firstIndex As Integer = title.IndexOf("<p>") + 3
                        Dim sString = title.Substring(firstIndex)
                        Dim lastIndex As Integer = sString.LastIndexOf("</p>")
                        Dim fstTitle = If(lastIndex > 0, sString.Substring(0, lastIndex), sString)
                        html.Append($"<h6 class='hText'> {fstTitle}")
                    Else
                        html.Append($"<h6 class='hText'> {title}")
                    End If
                Else
                    html.Append($"<h6 class='hText'> {title}")

                End If

                If dta1("SurveyPage")("Questions")(i)("AllowSkip").ToString() = "False" Then
                    html.Append(" <i class='required-text text-danger ms-1'> *</i>")
                End If
                html.Append("</h6>")
                html.Append("</div>")
                If (dta1("SurveyPage")("Questions").Count > 0) Then
                    If ((Convert.ToInt16(dta1("SurveyPage")("Questions")(i)("MinSelectedVariants")) > 0) Or Convert.ToInt16(dta1("SurveyPage")("Questions")(i)("MaxSelectedVariants")) > 0) Then
                        html.Append($"<div class='d-flex align-items-start mb-2'>")
                        html.Append($"<label>(select {If(Convert.ToInt16(dta1("SurveyPage")("Questions")(i)("MinSelectedVariants")) > 0, " minimum " + dta1("SurveyPage")("Questions")(i)("MinSelectedVariants").ToString() + " item(s)", "")}")
                        html.Append($"{If(Convert.ToInt16(dta1("SurveyPage")("Questions")(i)("MinSelectedVariants")) > 0 And Convert.ToInt16(dta1("SurveyPage")("Questions")(i)("MaxSelectedVariants")) > 0, " and ", "")}")
                        html.Append($"{If(Convert.ToInt16(dta1("SurveyPage")("Questions")(i)("MaxSelectedVariants")) > 0, " maximum " + dta1("SurveyPage")("Questions")(i)("MaxSelectedVariants").ToString() + " item(s)", "")})")
                        html.Append("</label>")
                        html.Append("</div>")
                    End If
                End If

                Dim SurveyAnswers = dta1("SurveyAnswers")(i)

                If qType <> "1" And qType <> "2" And qType <> "14" And qType <> "15" Then
                    'If dta1("SurveyPage")("Questions")(i)("Variants").Count > 0 Then
                    Dim Variants = dta1("SurveyPage")("Questions")(i)("Variants")
                    If qType = "12" Then
                        Variants = dta1("objectivelist")
                    ElseIf qType = "13" Then
                        Variants = dta1("alternativelist")
                    End If

                    If dta1("SurveyPage")("Questions")(i)("Type").ToString() = "3" Then
                        html.Append("<div class='formgrp'>")
                        For k = 0 To Variants.Count - 1
                            Dim AGUID_name = $"{dta1("SurveyPage")("Questions")(i)("AGUID")}"
                            Dim onClick = $"onclick=otherDisable('{AGUID_name}','notOther')"
                            If Variants(k)("Type").ToString() = "2" Then
                                onClick = $"onclick=otherDisable('{AGUID_name}','other')"
                            End If

                            html.Append("<div class='form-group '>")
                            'If SurveyAnswers(0).ToString() = Variants(k)("Text").ToString() Or (SurveyAnswers(0).ToString() <> Variants(k)("Text").ToString() And Variants(k)("Type").ToString() = "2") Then
                            '    html.Append($"<input class='custom-radio' type='radio' name='{AGUID_name}' id='{Variants(k)("AGUID")}' checked {If(Variants(Variants.Count - 1)("Type").ToString() = "2", onClick, "")} />")
                            'Else
                            '    html.Append($"<input class='custom-radio' type='radio' onchange=change_respondentAnswer('{i}','{Variants(k)("Text").ToString().Replace(" ", "##")}','3') name='{AGUID_name}' id='{Variants(k)("AGUID")}' {If(Variants(Variants.Count - 1)("Type").ToString() = "2", onClick, "")} />")
                            'End If
                            html.Append($"<input class='custom-radio' type='radio' onchange=change_respondentAnswer('{i}','{Variants(k)("Text").ToString().Replace(" ", "##")}','3') name='{AGUID_name}' id='{Variants(k)("AGUID")}' {If(SurveyAnswers(0).ToString() = Variants(k)("Text").ToString() Or (SurveyAnswers(0).ToString().Contains(":") And Variants(k)("Type").ToString() = "2"), "checked='checked'", "")} {If(Variants(Variants.Count - 1)("Type").ToString() = "2", onClick, "")} />")
                            html.Append($"<label for='{Variants(k)("AGUID")}' {onClick}>{Variants(k)("Text")}</label>")
                            If Variants(k)("Type").ToString() = "2" Then
                                html.Append($"<input type='text' name='{AGUID_name}' onkeyup=change_otherAnswer('3','{i}',this,'{Variants(k)("Text").ToString().Replace(" ", "##")}') class='form-control w-' value='{If(SurveyAnswers(0).ToString().Contains(":"), SurveyAnswers(0).ToString().Replace(CChar("::"), ""), "")}' {If(SurveyAnswers(0).ToString() = Variants(k)("Text").ToString() Or (SurveyAnswers(0).ToString().Contains(":") And Variants(k)("Type").ToString() = "2"), "", "disabled")} />")
                            End If
                            If k = Variants.Count - 1 Then
                                html.Append($"<button type='button' class='clear-btn btn' onclick=clearInputs('radio','3','{dta1("SurveyPage")("Questions")(i)("AGUID")}','{i}')>Clear</button>")
                            End If
                            html.Append("</div>")
                        Next
                        html.Append("</div>")
                    End If

                    If dta1("SurveyPage")("Questions")(i)("Type").ToString() = "4" Then
                        html.Append("<div class='formgrpchk'>")
                        For k = 0 To Variants.Count - 1

                            html.Append("<div class='form-group '>")
                            If Variants(k)("Type").ToString() = "0" Then
                                'Dim model_text As String = HttpUtility.UrlEncode(dta1("SurveyPage")("Questions")(i).ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
                                ',decodeURIComponent({ChrW(&H22) + model_text + ChrW(&H22)})
                                html.Append($"<input class='' onchange=change_respondentAnswer('{i}',this,'4','true','{Variants(k)("Text").ToString().Replace(" ", "##")}') type='checkbox' name='{dta1("SurveyPage")("Questions")(i)("AGUID")}' id='{Variants(k)("AGUID")}'")
                                If SurveyAnswers(0).ToString().Split(CChar(";")).Contains(Variants(k)("Text").ToString()) Then
                                    html.Append(" checked='checked' ")
                                End If
                                html.Append($"/><label for='{Variants(k)("AGUID")}'>{Variants(k)("Text")}</label>")
                            End If
                            If Variants(k)("Type").ToString() = "2" Then
                                html.Append($"<label for='{Variants(k)("AGUID")}'>{Variants(k)("Text")}</label>")
                                Try
                                    html.Append($"<input type='text' onkeyup=change_otherAnswer('4','{i}',this,'{Variants(k)("Text").ToString().Replace(" ", "##")}') class='form-control other-input ms-4' name='{dta1("SurveyPage")("Questions")(i)("AGUID")}' value='{SurveyAnswers(0).ToString().Replace(":", "").Split(CChar(";"))(SurveyAnswers(0).ToString().Replace(":", "").Split(CChar(";")).Count - 1)}'/>")
                                Catch
                                    html.Append($"<input type='text' onkeyup=change_otherAnswer('4','{i}',this,'{Variants(k)("Text").ToString().Replace(" ", "##")}') class='form-control other-input ms-4' name='{dta1("SurveyPage")("Questions")(i)("AGUID")}' value='{SurveyAnswers(0).ToString().Replace(":", "")}'/>")
                                End Try
                            End If
                            html.Append("</div>")
                        Next
                        html.Append("</div>")

                    End If

                    If dta1("SurveyPage")("Questions")(i)("Type").ToString() = "5" Then
                        html.Append("<div class='form-group '>")
                        html.Append($"<select class='form-select' onchange=change_respondentAnswer('{i}',this,'5') id='{dta1("SurveyPage")("Questions")(i)("AGUID")}' name='{dta1("SurveyPage")("Questions")(i)("AGUID")}'>")
                        html.Append("<option value=''>--Select One--</option>")
                        For k = 0 To Variants.Count - 1
                            If Variants(k)("Type").ToString() = "0" Then
                                html.Append($"<option value='{Variants(k)("AGUID")}' {If(SurveyAnswers(0).ToString() = Variants(k)("Text").ToString(), "selected", "")}>{Variants(k)("Text")}</option>")
                            End If
                        Next
                        html.Append("</select>")

                        If Variants(Variants.Count - 1)("Type").ToString() <> "2" Then
                            html.Append($"<button type='button' onclick=clearInputs('select','5','{dta1("SurveyPage")("Questions")(i)("AGUID")}','{i}') class='clear-btn btn'>Clear</button>")
                        End If
                        html.Append("</div>")

                        If Variants(Variants.Count - 1)("Type").ToString() = "2" Then
                            html.Append("<div class='form-group '>")
                            html.Append($"<label for='{Variants(Variants.Count - 1)("AGUID")}'>{Variants(Variants.Count - 1)("Text")}</label>")
                            html.Append($"<input type='text' onkeyup=change_respondentAnswer('{i}',this,'2') class='form-control other-input ms-4' name='{dta1("SurveyPage")("Questions")(i)("AGUID")}' value='{SurveyAnswers(0).ToString().Replace(":", "")}'/>")
                            html.Append($"<button type='button' onclick=clearInputs('select','5','{dta1("SurveyPage")("Questions")(i)("AGUID")}','{i}') class='clear-btn btn'>Clear</button>")
                            html.Append("</div>")
                        End If
                    End If

                    'Objectives Checklist
                    If dta1("SurveyPage")("Questions")(i)("Type").ToString() = "12" Then
                        For k = 0 To Variants.Count - 1
                            html.Append($"<div class='form-group  {If(Variants(k)("level").ToString() = "0", "", If(Variants(k)("level").ToString() = "1", "ms-3", If(Variants(k)("level").ToString() = "2", "ms-5", If(Variants(k)("level").ToString() = "3", "ms-2 ms-5", ""))))}'>")
                            'Dim model_text As String = HttpUtility.UrlEncode(dta1("SurveyPage")("Questions")(i).ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
                            ',decodeURIComponent({ChrW(&H22) + model_text + ChrW(&H22)})
                            If k = 0 Then
                                html.Append($"<input class='clsType12' onchange=change_AllrespondentAnswer('{i}','{Variants(k)("NodeName").ToString().Replace(" ", "##")}','12','false','{k}') type='checkbox' name='{dta1("SurveyPage")("Questions")(i)("AGUID")}' id='qType_12_{k}'")
                            Else
                                html.Append($"<input class='clsType12' onchange=change_AllrespondentAnswer('{i}','{Variants(k)("NodeName").ToString().Replace(" ", "##")}','12','false','{k}') type='checkbox' name='{dta1("SurveyPage")("Questions")(i)("AGUID")}' id='qType_12_{k}'")
                            End If
                            'If SurveyAnswers(0).ToString() <> "" And CBool(SurveyAnswers(0).ToString().Split(CChar(";"))(k)) Then
                            If Variants(k)("isDisabled").ToString().ToLower() = "true" Then
                                html.Append(" checked='checked' ")
                            End If
                            html.Append($"{If(k = 1, "disabled", "")}/><label for='qType_12_{k}'>{Variants(k)("NodeName")}</label>")
                            html.Append("</div>")
                        Next
                    End If

                    'Alternatives Check List
                    If dta1("SurveyPage")("Questions")(i)("Type").ToString() = "13" Then
                        For k = 0 To Variants.Count - 1
                            html.Append($"<div class='form-group {If(k <> 0, " ms-3", "")}'>")
                            'Dim model_text As String = HttpUtility.UrlEncode(dta1("SurveyPage")("Questions")(i).ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
                            ',decodeURIComponent({ChrW(&H22) + model_text + ChrW(&H22)})
                            html.Append($"<input class='clsType13' onchange=change_AllrespondentAnswer('{i}','{Variants(k)("NodeName").ToString().Replace(" ", "##")}','13','false','{k}') type='checkbox' name='{dta1("SurveyPage")("Questions")(i)("AGUID")}' id='qType_13_{k}'")
                            'If SurveyAnswers(0).ToString() <> "" AndAlso SurveyAnswers(0).ToString().ToLower() = Variants(k)("isDisabled").ToString().ToLower() Then
                            If Variants(k)("isDisabled").ToString().ToLower() = "true" Then
                                html.Append(" checked='checked' ")
                                'ElseIf SurveyAnswers(0).ToString() <> "" And CBool(SurveyAnswers(0).ToString().Split(CChar(";"))(k)) Then
                                '    html.Append(" checked='checked' ")
                            End If
                            html.Append($"/><label for='qType_13_{k}'>{Variants(k)("NodeName")}</label>")
                            html.Append("</div>")
                        Next
                    End If
                    'End If
                ElseIf qType = "1" Or qType = "2" Or qType = "14" Or qType = "15" Then
                    If dta1("SurveyPage")("Questions")(i)("Type").ToString() = "1" Then
                        html.Append("<div class='form-group '>")
                        html.Append($"<input type='text' onkeyup=change_respondentAnswer('{i}',this,'1') class='form-control' name='{dta1("SurveyPage")("Questions")(i)("AGUID")}' value='{SurveyAnswers(0)}' />")
                        html.Append("</div>")
                    End If

                    If dta1("SurveyPage")("Questions")(i)("Type").ToString() = "2" Then
                        html.Append("<div class='form-group '>")
                        html.Append($"<textarea row='1' class='form-control ' onkeyup=change_respondentAnswer('{i}',this,'2') id='{dta1("SurveyPage")("Questions")(i)("AGUID")}' value='{SurveyAnswers(0)}'>{SurveyAnswers(0)}</textarea>")
                        html.Append("</div>")
                    End If

                    If dta1("SurveyPage")("Questions")(i)("Type").ToString() = "15" Then
                        html.Append("<label>(Enter multiple numbers separated by a new line)</label><div class='form-group '>")
                        html.Append($"<textarea row='1' class='form-control' onkeyup=change_respondentAnswer('{i}',this,'15') name='{dta1("SurveyPage")("Questions")(i)("AGUID")}' value='{SurveyAnswers(0)}'>{SurveyAnswers(0)}</textarea>")
                        html.Append("")
                        html.Append("</div>")
                    End If

                    'Number
                    If dta1("SurveyPage")("Questions")(i)("Type").ToString() = "14" Then
                        html.Append("<label>(Enter a valid number)</label><div class='form-group'>")
                        'html.Append($"<input type='text' onkeyup=change_respondentAnswer('{i}',this,'14') onpaste=onlyNumbers(event) class='form-control other-input ms-4' id='{dta1("SurveyPage")("Questions")(i)("AGUID")}' value='{SurveyAnswers(0)}'")
                        html.Append($"<input type='text' onkeyup=change_respondentAnswer('{i}',this,'14') class='form-control other-input ms-4' id='{dta1("SurveyPage")("Questions")(i)("AGUID")}' value='{SurveyAnswers(0)}'")
                        html.Append("/>")
                        html.Append("</div>")
                    End If
                End If
                'html.Append("<div><hr></div>")
                'End If
                html.Append("</div></div>")


            Next
            html.Append("</div></div></div>")
            html.Append("</div>")
            html.Append("</div>")
            html.Append("</div>")
            divContent.InnerHtml = html.ToString()
        End If
    End Sub

End Class