Imports ExpertChoice.Data

Public Class QuestionHeader
    Inherits System.Web.UI.UserControl

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    End Sub
    Public Sub BindHtml(ByRef output As AnytimeOutputModel)
        Info_QHicons.BindHtml(output)
        Dim multiIndex = 1
        Dim isOwner As Double = 0
        Dim is_screen_reduced = False
        Dim is_anytime = True
        Dim App = CType(Session("App"), clsComparionCore)
        If output.owner = App.ActiveUser.UserName.ToString() Then
            isOwner = 1
        End If
        Dim html As StringBuilder = New StringBuilder()

        If is_anytime Then
            html.Append("<div class='question-header large-12 columns anytime-question-header")
            If (output.page_type <> "atAllPairwise") Or (output.page_type = "atAllPairwise" And output.pairwise_type = "ptGraphical") Or (output.page_type = "atAllPairwise" And output.pairwise_type = "ptVerbal" And If(output.multi_non_pw_data Is Nothing, 0, output.multi_non_pw_data.Count >= 7)) Then
                html.Append(" tt-sticky-element'>")
            Else
                html.Append("'>")
            End If
            html.Append("<div class='tg-legend'><div class='small-12 columns tt-question-title text-center qh-icons-wrap'>")
            If output.page_type <> "atShowLocalResults" And output.page_type <> "atShowGlobalResults" Then
                html.Append("<div class='row qhelp-questions common-location'>")
                html.Append(dvFirstContent.Visible)
                html.Append("</div>")
            End If
            html.Append("<h3 class='no-margin'> ")
            If String.IsNullOrWhiteSpace(output.step_task) Then
                html.Append("<span class='pipe-question question-node-info-text'>")
                html.Append(output.step_task)
                html.Append("</span>")
            Else
                html.Append("<span class='pipe-question'> which of the two")
                html.Append("<span class='question'>" + output.question + "</span> ")
                html.Append("<span class='wording'>" + output.wording + "</span></span> ")
            End If
            html.Append("</h3> </div> </div>")
        End If



        Dim collapsed_info_docs = output.multi_collapse_default

        If Not output.is_infodoc_tooltip And (output.page_type = "atAllPairwise" Or output.page_type = "atAllPairwiseOutcomes") Then

            html.Append("<div class='other-info-doc columns " + If(output.multi_non_pw_data Is Nothing, "", If(output.multi_non_pw_data.Count > 7, "duplicate-parent-infodoc", "")) + "  hide large-8 large-centered'>")
            If isOwner = 0 And String.IsNullOrWhiteSpace(output.parent_node_info) Then
                html.Append("<div class='text-center'>")

                html.Append(" <a onclick='set_collapse_cookies('parent-node'); update_infodoc_params('parent-node', output.ParentNodeGUID, '')")
                html.Append("class='tt-toggler-0' id='0' data-toggler='tg-accordion'>")
                If (output.showinfodocnode) Then
                    html.Append("<span class='icon icon-desktop icon-tt-minus-square' data-node='0'></span>")
                Else
                    html.Append("<span class='icon icon-desktop icon-tt-plus-square' data-node='0'></span>")
                End If
                html.Append("</a>")
            End If

            html.Append("<span id='0' class='text parent-lbl parent-node hide-when-editing-0 text editable-trigger143 et-0 small'>" + If(output.hideInfoDocCaptions, "", output.parent_node) + "  </span>")

            If isOwner <> 1 Or Not output.showinfodocnode Then
            Else
                html.Append("<a data-index='" + multiIndex.ToString() + "' data-node-type='- 1'  data-location='-1' data-node='parent-node' data-node-description=" + output.parent_node + " class='edit-info-doc-btn edit-pencil ep ep-0 ep-Sub-0' title='' href='#' >")
                html.Append(" <span class='icon-tt-pencil'> </span> </a>")
            End If
            html.Append("</div>")
            If Not output.showinfodocnode Or (String.IsNullOrWhiteSpace(output.parent_node_info) And isOwner = 0) Then
            Else
                html.Append("<div class='parent-node-info-div  tt-accordion-content tg-accordion-0  tg-accordion-sub-0'>")
                html.Append(" <div data-index='0' class='tt-panel-temp tt-panel tt-resizable-panel tt-resizable-panel-0 small-centered columns'>")
                html.Append("<div class='parent-node-info-text'" + output.parent_node_info + ">")
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
                html.Append("<div class='columns small-6'><div class='text-center'>")
                html.Append("<a class='tt-toggler-1 tt-toggler' id='1'  data-toggler='tg-accordion-sub' onclick='set_collapse_cookies('left-node'); update_infodoc_params('left-node', output.ParentNodeGUID, '')'")
                If (output.showinfodocnode) Then
                    html.Append("<span class='icon icon-desktop icon-tt-minus-square' data-node='1'></span>")
                Else
                    html.Append("<span class='icon icon-desktop icon-tt-plus-square' data-node='1'></span>")
                End If
                html.Append("</a>")
                html.Append($" <span id='1' class='left-node hide-when-editing-1 text editable-trigger143 et-1 small' style='font-size:  12px;'> " + If(output.hideInfoDocCaptions, "", output.multi_pw_data(multiIndex).LeftNode) + " </span>")
                html.Append("</div>")
                html.Append(" <div class='row collapse editable-input-wrap eiw-1'> <div class='small-12 columns'>")
                html.Append($" <input value=" + output.multi_pw_data(multiIndex).LeftNode + "  name='' placeholder='Add something here' type='text' class='text editable-input ei-1' /> ")
                html.Append("</div><div class='small-12 columns'> <a id='1' href='#' class='button tiny tt-button-primary cancel-wrt-btn'>Cancel</a>")
                html.Append(" <a  id='1' href='#'  class='button tiny tt-button-primary save-wrt-btn'>Save</a> </div> </div>")
                If (Not output.showinfodocnode Or collapsed_info_docs(1) Or (String.IsNullOrWhiteSpace(output.multi_pw_data(multiIndex).InfodocLeft) And isOwner = 0)) Then
                Else
                    html.Append("<div class='left-node-info-div tt-accordion-content tg-accordion-1  tg-accordion-sub-1'>")
                    html.Append("<div data-index='1' class='tt-panel-temp tt-panel tt-resizable-panel tt-resizable-panel-1 small-centered columns box-1' data-box-index='1' style='max-height: 90px;'>  ")
                    If (isOwner <> 1 Or Not output.showinfodocnode Or collapsed_info_docs(1)) Then
                    Else
                        html.Append($" <a data-index='" + multiIndex + "'")
                        html.Append($" data-node-type='2' data-location='1' data-node-id='" + output.multi_pw_data(multiIndex).NodeID_Left + "'  data-node='left-node'   data-node-description='" + output.multi_pw_data(multiIndex).LeftNode + "' class='edit-info-doc-btn edit-pencil ep ep-sub-1' ")
                        html.Append(" title='' href='#'> <span class='icon-tt-pencil'></span> </a> ")
                        html.Append($"<div  class='left-node-info-text" + If(is_screen_reduced, "zoom-out", "") + "'>" + output.multi_pw_data(multiIndex).InfodocLeft + "</div>")
                    End If
                    html.Append(" </div> </div>")
                End If
                html.Append(" </div>")
            End If
            If (String.IsNullOrWhiteSpace(output.multi_pw_data(multiIndex).InfodocRight) And isOwner = 0) Then
            Else
                html.Append("<div class='small-6 columns'><div class='text-center'>")
                html.Append($"<a onclick='set_collapse_cookies('right-node'); update_infodoc_params('right-node', output.ParentNodeGUID, '')' class=' tt-toggler-2 " + If((isOwner = 1 Or (isOwner = 0 And Not String.IsNullOrWhiteSpace(output.multi_pw_data(multiIndex).InfodocRight))), "tt-toggler", "") + "' id='2' data-toggler='tg-accordion-Sub'>")
                html.Append($"<span class='" + If((output.showinfodocnode And Not collapsed_info_docs(2)), "icon-tt-minus-square", "") + " " + If((Not output.showinfodocnode Or collapsed_info_docs(2)), "icon-tt-plus-square", "") + " icon icon-desktop' data-node='2' ></span> </a>")
                html.Append(" <span id='2' class='right-node hide-when-editing-2 text editable-trigger143 et-2 small' style='font-size: 12px;'> " + output.multi_pw_data(multiIndex).RightNode + "   </span></div>")
                html.Append(" <div class='row collapse editable-input-wrap eiw-2'> <div class='small-12 columns'>")
                html.Append($"<input  value= '" + output.multi_pw_data(multiIndex).RightNode + "'  name='' placeholder='Add something here' type='text' class='text editable-input ei-2' />         </div>")
                html.Append(" <div class='small-12 columns'> <a id='2' href='#' class='button tiny tt-button-primary cancel-wrt-btn '>Cancel</a>  <a   id='2' href='#'  class='button tiny tt-button-primary save-wrt-btn'>Save</a></div></div>")
                html.Append("<div  class='right-node-info-div  tt-accordion-content tg-accordion-2  tg-accordion-sub-2" + If((Not output.showinfodocnode Or collapsed_info_docs(2) Or (String.IsNullOrWhiteSpace(output.multi_pw_data(multiIndex).InfodocRight) And isOwner = 0)), "hide", "") + "'")
                html.Append("<div data-index='1' class='tt-panel-temp tt-panel tt-resizable-panel tt-resizable-panel-1 small-centered columns box-2' data-box-index='2'  style='max-height: 90px;'>")
                html.Append($" <a data-index='" + multiIndex + "' data-node-type='2'  data-location='2'  data-node='right-node'  data-node-id='" + output.multi_pw_data(multiIndex).NodeID_Right + "'  data-node-description='" + output.multi_pw_data(multiIndex).RightNode + "' class='edit-info-doc-btn edit-pencil ep ep-sub-2" + If((isOwner <> 1 Or Not output.showinfodocnode Or collapsed_info_docs(2)), "hide", "") + "' title=''  href='#'>")
                html.Append($"<span class='icon-tt-pencil" + If(isOwner <> 1, "hide", "") + "'></span> </a> <div  class='right-node-info-text " + If(is_screen_reduced, "zoom-out", "") + "'>" + output.multi_pw_data(multiIndex).InfodocRight + "  </div>")
                html.Append(" </div>  </div>")
            End If
            html.Append("</div>")
        End If
        html.Append("</div></div></div>")
        If (output.pairwise_type = "ptVerbal" And (output.page_type = "atAllPairwise" Or output.page_type = "atAllPairwiseOutcomes")) Then
            html.Append("<div  class='legend columns " + If(output.multi_pw_data.Count > 7, "duplicate-parent-infodoc", "") + " hide'>")
            html.Append(" <div class='text-center panel' style='font-size:  .750rem;padding: 1px;margin-bottom: 3px;'> <b>EQ</b>ual<b>&nbsp;&nbsp;&nbsp;M</b>oderate")
            html.Append("&nbsp;&nbsp;&nbsp;<b>S</b>trong  &nbsp;&nbsp;&nbsp;<b>V</b>ery<b>S</b>trong &nbsp;&nbsp;&nbsp;<b>EX</b>treme  </div> </div>")
        End If
        html.Append(" </div>")
        Dim is_teamtime = False
        Dim information_nodes = ""
        html.Append(" <div class='tt-sticky-element teamtime-question question-header'>  <div class='tg-legend'><div class='hide-for-medium-down'>")
        html.Append("<div class='tt-toggle-comments-wrap toggleComments tc-btn-left' style='padding:  0px;'><span style=''>show comments</span></div> </div>")
        html.Append("<div class='small-12 columns tt-question-title text-center small-centered'>")
        html.Append($"<h3><span class='pipe-question question-node-info-text " + If(information_nodes <> "", "", "hide") + "' ng-class=''>" + information_nodes + "</span></h3>")
        html.Append("</div></div></div>")
        dvContent.InnerHtml = html.ToString()
    End Sub

End Class