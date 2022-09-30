Public Class QuestionHeader
    Inherits System.Web.UI.UserControl

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    End Sub

    Public Sub BindHtml(ByRef output As AnytimeOutputModel)
        Info_QHicons.BindHtml(output)
        Dim multiIndex = 1
        Dim isOwner As Double = 0
        Dim is_anytime = True
        Dim App = CType(Session("App"), clsComparionCore)
        If output.owner = App.ActiveUser.UserName.ToString() Then
            isOwner = 1
        End If
        Dim html As StringBuilder = New StringBuilder()
        If is_anytime Then
            'html.Append("<div class='question-header large-12 columns anytime-question-header")
            'If (output.page_type <> "atAllPairwise") Or (output.page_type = "atAllPairwise" And output.pairwise_type = "ptGraphical") Or (output.page_type = "atAllPairwise" And output.pairwise_type = "ptVerbal") Then
            '    'And If(output.multi_non_pw_data Is Nothing, 0, output.multi_non_pw_data.Count >= 7)
            '    html.Append(" tt-sticky-element'>")
            'Else
            '    html.Append("'>")
            'End If
            'html.Append("<div class='tg-legend'><div class='small-12 columns tt-question-title text-center qh-icons-wrap'>")
            'If output.page_type <> "atShowLocalResults" And output.page_type <> "atShowGlobalResults" Then
            '    html.Append("<div class='row qhelp-questions common-location'>")
            '    html.Append(dvFirstContent.Visible)
            '    html.Append("</div>")
            'End If
            html.Append("<div class='col-12'><div class='page-title-box text-center toggle_title'><div class='collabse_btn'>")
            html.Append("<input type='checkbox' class='collabse_arrow' checked=''><div class='arrow_icon'><i class='fa fa-plus' aria-hidden='true'></i>")
            html.Append("<i class='fa fa-minus' aria-hidden='True'></i></div><p class='font-size-18 prm-color text-center toggle_text'>")
            If output.step_task <> "" Then
                html.Append(output.step_task)
            Else
                html.Append("<span> which of the two")
                html.Append("<span>" + output.question + "</span> ")
                html.Append("<span>" + output.wording + "</span></span> ")
            End If
            'html.Append("You have completed prioritizing your objectives with respect to <strong>'Vendor / Source Selection.'</strong>")
            'html.Append("Review your results below to ensure they make sense to you. If not, you may navigate back to the previous judgments to edit them. <a href='#'><i class='bx bxs-edit-alt'></i></a>")
            html.Append("</p></div></div></div>")
            'html.Append("<h3 class='no-margin'> ")
            'If String.IsNullOrWhiteSpace(output.step_task) Then
            '    html.Append("<span class='pipe-question question-node-info-text'>")
            '    html.Append(output.step_task)
            '    html.Append("</span>")
            'Else
            '    html.Append("<span class='pipe-question'> which of the two")
            '    html.Append("<span class='question'>" + output.question + "</span> ")
            '    html.Append("<span class='wording'>" + output.wording + "</span></span> ")
            'End If
            'html.Append("</h3>")
            'html.Append("</div> </div>")
        End If

        Dim collapsed_info_docs = output.multi_collapse_default

        If Not output.is_infodoc_tooltip And (output.page_type = "atAllPairwise" Or output.page_type = "atAllPairwiseOutcomes") Then

            html.Append($"<div class='other-info-doc columns " + If(output.multi_non_pw_data Is Nothing, "", If(output.multi_non_pw_data.Count > 7, "duplicate-parent-infodoc", "")) + "  hide large-8 large-centered'>")
            If isOwner = 0 And String.IsNullOrWhiteSpace(output.parent_node_info) Then
                Context.Session("output") = output
                html.Append(" <a onclick='set_collapse_cookies('parent-node'," + Context.Session("output").ToString() + ")'; update_infodoc_params('parent-node', " + output.ParentNodeGUID.ToString() + ", '')")
                html.Append("class='tt-toggler-0' id='0' data-toggler='tg-accordion'>")
                If (output.showinfodocnode) Then
                    html.Append("<span class='icon icon-desktop icon-tt-minus-square' data-node='0'></span>")
                Else
                    html.Append("<span class='icon icon-desktop icon-tt-plus-square' data-node='0'></span>")
                End If
                html.Append("</a>")
            End If

            html.Append($"<span id='0' class='text parent-lbl parent-node hide-when-editing-0 text editable-trigger143 et-0 small'>" + If(output.hideInfoDocCaptions, "", output.parent_node) + "  </span>")
            If isOwner <> 1 Or Not output.showinfodocnode Then
            Else
                html.Append($" <a data-index=" + multiIndex.ToString() + " data-node-type='- 1'  data-location='-1' data-node='parent-node' data-node-description=" + output.parent_node + " class='edit-info-doc-btn edit-pencil ep ep-0 ep-Sub-0' title='' href='#' >")
                html.Append(" <span class='icon-tt-pencil'> </span> </a>")
            End If
            html.Append("</div>")
            If Not output.showinfodocnode Or (String.IsNullOrWhiteSpace(output.parent_node_info) And isOwner = 0) Then
            Else
                html.Append("<div class='parent-node-info-div  tt-accordion-content tg-accordion-0  tg-accordion-sub-0'>")
                html.Append(" <div data-index='0' class='tt-panel-temp tt-panel tt-resizable-panel tt-resizable-panel-0 small-centered columns'>")
                html.Append("<div class='parent-node-info-text'>" + output.parent_node_info)
                html.Append("</div></div></div>")
            End If

            html.Append("</div>")
        End If
        Dim isMobile = False
        If isMobile And output.framed_info_docs And Not output.is_infodoc_tooltip And (output.page_type = "atAllPairwise" Or output.page_type = "atAllPairwiseOutcomes") Then
            html.Append(" <div class='row editable-content mobile-frame'>")
            If isOwner = 0 And String.IsNullOrWhiteSpace(output.multi_pw_data(multiIndex).InfodocLeft) Then
                html.Append("<div class='columns small-6'>&nbsp;</div>")
            Else

            End If
        End If
        dvContent.InnerHtml = html.ToString()
    End Sub


End Class