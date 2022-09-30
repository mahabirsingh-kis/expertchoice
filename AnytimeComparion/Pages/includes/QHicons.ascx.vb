Public Class QHicons
    Inherits System.Web.UI.UserControl

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    End Sub
    Public Sub BindHtml(ByRef output As AnytimeOutputModel)
        Dim html As StringBuilder = New StringBuilder()
        If (output.owner <> "0") Then
            html.Append("<div class='right qhelp-edit-wrap'>")
            html.Append("<a href='#' data-reveal-id='tt-qh-modal'><span class='icon-tt-edit'></span></a></div>")
        End If
        If (output.owner <> "0" And Not String.IsNullOrWhiteSpace(output.qh_info)) Then
            html.Append("<div class='right qhelp-question-wrap'>")
            html.Append(" <a href='#' data-reveal-id='tt-view-qh-modal'><span class='icon-tt-question-circle qh-icons qh-qm-icon qhelp-icon'")
            html.Append(output.qh_info)
            html.Append("</span></a> </div>")
        End If
        content.InnerHtml = html.ToString()
    End Sub
End Class