Imports Canvas.PipeParameters
Imports ExpertChoice.Data
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Public Class Survey
    Inherits System.Web.UI.UserControl

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        Dim App = CType(Session("App"), clsComparionCore)

        If App.ActiveUser Is Nothing Then
            Response.Redirect("~/login.aspx")
        End If
    End Sub

    Public Sub bindHtml(ByVal PipeParameters As Object)
        If PipeParameters IsNot Nothing Then
            Dim html As StringBuilder = New StringBuilder()
            Dim dta = JsonConvert.SerializeObject(PipeParameters)
            Dim dta1 = JObject.Parse(dta)
            html.Append("<div class='quiz-section'>")
            html.Append($"<div class='heading'>{Convert.ToString(dta1("SurveyPage")("Title"))}</div>")
            html.Append("<div class='quiz-wapper'>")
            html.Append("<div class='container'>")
            html.Append("<div class='single-quiz'>")
            For i = 0 To dta1("SurveyPage")("Questions").Count - 1
                html.Append($"<div class='d-flex mb-3'>")
                html.Append($"<b class='me-2'>{dta1("QuestionNumbering")(i)}</b><p> {dta1("SurveyPage")("Questions")(i)("Text")}")
                If dta1("SurveyPage")("Questions")(i)("AllowSkip").ToString() = "False" Then
                    html.Append(" <i class='required-text'>Required</i>")
                End If
                html.Append("</p>")
                html.Append("</div>")

                Dim qType = dta1("SurveyPage")("Questions")(i)("Type").ToString()
                Dim SurveyAnswers = dta1("SurveyAnswers")(i)
                If qType <> "1" And qType <> "2" And qType <> "14" And qType <> "15" Then
                    If dta1("SurveyPage")("Questions")(i)("Variants").Count > 0 Then
                        Dim Variants = dta1("SurveyPage")("Questions")(i)("Variants")
                        If dta1("SurveyPage")("Questions")(i)("Type").ToString() = "3" Then
                            For k = 0 To Variants.Count - 1
                                Dim AGUID_name = $"{dta1("SurveyPage")("Questions")(i)("AGUID")}"
                                Dim onClick = $"onclick=otherDisable('{AGUID_name}','notOther')"
                                If Variants(k)("Type").ToString() = "2" Then
                                    onClick = $"onclick=otherDisable('{AGUID_name}','other')"
                                End If
                                html.Append("<div class='form-group'>")
                                html.Append($"<input class='custom-radio' type='radio' name='{AGUID_name}' id='{Variants(k)("AGUID")}' {If(SurveyAnswers(0).ToString() = Variants(k)("Text").ToString(), "checked='checked'", "")} {If(Variants(Variants.Count - 1)("Type").ToString() = "2", onClick, "")} />")
                                html.Append($"<label for='{Variants(k)("AGUID")}' {onClick}>{Variants(k)("Text")}</label>")
                                If Variants(k)("Type").ToString() = "2" Then
                                    html.Append($"<input type='text' name='{AGUID_name}' class='form-conrol other-input ms-4' {If(SurveyAnswers(0).ToString() <> Variants(k)("Text").ToString(), "disabled", "")} />")
                                End If
                                If k = Variants.Count - 1 Then
                                    html.Append($"<button type='button' class='clear-btn' onclick=clearInputs('radio','3','{dta1("SurveyPage")("Questions")(i)("AGUID")}')>Clear</button>")
                                End If
                                html.Append("</div>")
                            Next
                        End If

                        If dta1("SurveyPage")("Questions")(i)("Type").ToString() = "4" Then
                            For k = 0 To Variants.Count - 1
                                html.Append("<div class='form-group'>")
                                html.Append($"<input class='form-control' type='checkbox' name='{dta1("SurveyPage")("Questions")(i)("AGUID")}' id='{Variants(k)("AGUID")}'")
                                If SurveyAnswers(0).ToString() = Variants(k)("Text").ToString() Then
                                    html.Append(" checked='checked' ")
                                End If
                                html.Append($"/><label for='{Variants(k)("AGUID")}'>{Variants(k)("Text")}</label>")
                                html.Append("</div>")
                            Next
                        End If

                        If dta1("SurveyPage")("Questions")(i)("Type").ToString() = "5" Then
                            html.Append("<div class='form-group'>")
                            html.Append($"<select class='form-select' id='{dta1("SurveyPage")("Questions")(i)("AGUID")}' name='{dta1("SurveyPage")("Questions")(i)("AGUID")}'>")
                            html.Append("<option value=''>--Select One--</option>")
                            For k = 0 To Variants.Count - 1
                                If Variants(k)("Type").ToString() = "0" Then
                                    html.Append($"<option value='{Variants(k)("AGUID")}' {If(SurveyAnswers(0).ToString() = Variants(k)("Text").ToString(), "selected", "")}>{Variants(k)("Text")}</option>")
                                End If
                            Next
                            html.Append("</select>")

                            If Variants(Variants.Count - 1)("Type").ToString() <> "2" Then
                                html.Append($"<button type='button' onclick=clearInputs('select','5','{dta1("SurveyPage")("Questions")(i)("AGUID")}') class='clear-btn'>Clear</button>")
                            End If
                            html.Append("</div>")

                            If Variants(Variants.Count - 1)("Type").ToString() = "2" Then
                                html.Append("<div class='form-group'>")
                                html.Append($"<label for='{Variants(Variants.Count - 1)("AGUID")}'>{Variants(Variants.Count - 1)("Text")}</label>")
                                html.Append($"<input type='text' class='form-control other-input ms-4' name='{dta1("SurveyPage")("Questions")(i)("AGUID")}' value='{SurveyAnswers(0).ToString().Replace(":", "")}'/>")
                                html.Append($"<button type='button' onclick=clearInputs('select','5','{dta1("SurveyPage")("Questions")(i)("AGUID")}') class='clear-btn'>Clear</button>")
                                html.Append("</div>")
                            End If
                        End If

                        If dta1("SurveyPage")("Questions")(i)("Type").ToString() = "12" Then
                            For k = 0 To Variants.Count - 1
                                html.Append("<div class='form-group'>")
                                html.Append($"<input class='form-control' type='checkbox' name='{dta1("SurveyPage")("Questions")(i)("AGUID")}' id='{Variants(k)("AGUID")}'")
                                If SurveyAnswers(0).ToString() = Variants(k)("Text").ToString() Then
                                    html.Append(" checked='checked' ")
                                End If
                                html.Append($"/><label for='{Variants(k)("AGUID")}'>{Variants(k)("Text")}</label>")
                                html.Append("</div>")
                            Next
                        End If

                        If dta1("SurveyPage")("Questions")(i)("Type").ToString() = "13" Then
                            For k = 0 To Variants.Count - 1
                                html.Append("<div class='form-group'>")
                                html.Append($"<input class='form-control' type='checkbox' name='{dta1("SurveyPage")("Questions")(i)("AGUID")}' id='{Variants(k)("AGUID")}'")
                                If SurveyAnswers(0).ToString() = Variants(k)("Text").ToString() Then
                                    html.Append(" checked='checked' ")
                                End If
                                html.Append($"/><label for='{Variants(k)("AGUID")}'>{Variants(k)("Text")}</label>")
                                html.Append("</div>")
                            Next
                        End If
                    End If
                ElseIf qType = "1" Or qType = "2" Or qType = "14" Or qType = "15" Then
                    If dta1("SurveyPage")("Questions")(i)("Type").ToString() = "1" Then
                        html.Append("<div class='form-group'>")
                        html.Append($"<input type='text' class='form-control other-input ms-4' name='{dta1("SurveyPage")("Questions")(i)("AGUID")}' value='{SurveyAnswers(0)}' />")
                        html.Append("</div>")
                    End If

                    If dta1("SurveyPage")("Questions")(i)("Type").ToString() = "2" Then
                        html.Append("<div class='form-group'>")
                        html.Append($"<textarea class='form-control custom-textarea' id='{dta1("SurveyPage")("Questions")(i)("AGUID")}' value='{SurveyAnswers(0)}'>{SurveyAnswers(0)}</textarea>")
                        html.Append("</div>")
                    End If

                    If dta1("SurveyPage")("Questions")(i)("Type").ToString() = "15" Then
                        html.Append("<div class='form-group'>")
                        html.Append($"<textarea class='form-control custom-textarea' name='{dta1("SurveyPage")("Questions")(i)("AGUID")}' value='{SurveyAnswers(0)}'>{SurveyAnswers(0)}</textarea>")
                        html.Append("<label>(Enter multiple numbers separated by a new line)</label>")
                        html.Append("</div>")
                    End If

                    If dta1("SurveyPage")("Questions")(i)("Type").ToString() = "14" Then
                        html.Append("<div class='form-group'>")
                        html.Append($"<input type='text' class='form-control other-input ms-4' id='{dta1("SurveyPage")("Questions")(i)("AGUID")}' value='{SurveyAnswers(0)}'")
                        html.Append("/><label>(Enter a valid number)</label>")
                        html.Append("</div>")
                    End If
                End If
            Next
            html.Append("</div>")
            html.Append("</div>")
            html.Append("</div>")
            html.Append("</div>")
            divContent.InnerHtml = html.ToString()
        End If
    End Sub

End Class