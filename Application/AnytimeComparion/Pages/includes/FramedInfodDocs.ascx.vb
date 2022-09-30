Public Class FramedInfodDocs
    Inherits System.Web.UI.UserControl

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    End Sub

    Public Sub BindHtml(ByRef output As AnytimeOutputModel)
        Dim isOwner As Double = 0
        Dim isMobile = False
        Dim active_multi_index = 0
        Dim App = CType(Session("App"), clsComparionCore)
        If output.owner = App.ActiveUser.UserName.ToString() Then
            isOwner = 1
        End If
        If output.is_infodoc_tooltip And output.showinfodocnode Then
            speditable.Visible = True
        End If
        If String.IsNullOrWhiteSpace(output.parent_node_info) And isOwner = 0 Then
            speditable.Attributes.Add("class", "hide")
        End If
        parent_tooltip_trigger.Attributes.Add("data-node-description", output.parent_node)

        parent_tooltip_trigger.Attributes.Add("data-readonly", (If(isOwner = 0, 1, 0)).ToString())
        sp_0.InnerText = output.parent_node
        txtparentnode.Value = output.parent_node
        If (Not output.is_infodoc_tooltip Or (isOwner = 0 And output.framed_info_docs)) And (output.page_type = "atAllPairwise" Or output.page_type = "atAllPairwiseOutcomes") And output.showinfodocnode Then
            span2editable.Visible = True
        End If
        dveditconetnt.Attributes.Add("class", "row editable-content " + If(isMobile And output.framed_info_docs, "editable-content-height", ""))
        If String.IsNullOrWhiteSpace(output.multi_pw_data(active_multi_index).InfodocLeft) And isOwner = 0 Then
            dvblank1.Visible = True
        End If
        If isOwner = 0 And String.IsNullOrWhiteSpace(output.parent_node_info) Then
            dvBlank2.Visible = True
        End If
        If isMobile = False Then
            dvWindowscontecnt.Visible = True
        End If
        dvWindowscontecnt.Attributes.Add("class", "columns" + If((String.IsNullOrWhiteSpace(output.multi_pw_data(active_multi_index).InfodocLeft) And isOwner = 0), "hide", "") + " " + If(isOwner = 1 Or (isOwner = 0 And Not String.IsNullOrWhiteSpace(output.parent_node_info)), "large-3", "") + " " + If(isOwner = 0 And String.IsNullOrWhiteSpace(output.parent_node_info), "large-4", ""))
        a_1.Attributes.Add("class", (If(isOwner = 1 Or (isOwner = 0 And Not String.IsNullOrWhiteSpace(output.multi_pw_data(active_multi_index).InfodocLeft)), "tt-toggler", "")))
        spdesktop.Attributes.Add("class", If((output.showinfodocnode) And Not output.multi_collapse_default(1), "icon-tt-minus-square", "") + " " + If(Not output.showinfodocnode Or output.multi_collapse_default(1), "icon-tt-plus-square", "") + " icon icon-desktop")
        sp_1.InnerText = If(output.hideInfoDocCaptions, "", output.multi_pw_data(active_multi_index).LeftNode)
        If Not (isMobile Or output.is_infodoc_tooltip) And isOwner = 0 And Not String.IsNullOrWhiteSpace(output.multi_pw_data(active_multi_index).InfodocLeft) Then
            a_magnifyedit.Visible = True
        End If
        a_magnifyedit.Attributes.Add("data-index", active_multi_index.ToString())
        a_magnifyedit.Attributes.Add("data-node-id", output.multi_pw_data(active_multi_index).NodeID_Left.ToString())
        a_magnifyedit.Attributes.Add("data-node-description", output.multi_pw_data(active_multi_index).LeftNode)
        txtleftnode.Value = output.multi_pw_data(active_multi_index).LeftNode
        dvconetcnt2.Attributes.Add("class", "left-node-info-div tt-accordion-content tg-accordion-1  tg-accordion-sub-1 " + If(Not output.showinfodocnode Or output.multi_collapse_default(1) Or (String.IsNullOrWhiteSpace(output.multi_pw_data(active_multi_index).InfodocLeft) And isOwner = 0), "hide", ""))
        a_leftnode.Attributes.Add("data-index", active_multi_index.ToString())
        a_leftnode.Attributes.Add("data-node-id", output.multi_pw_data(active_multi_index).NodeID_Left.ToString())
        a_leftnode.Attributes.Add("data-node-description", output.multi_pw_data(active_multi_index).LeftNode)
        sp_hide.Attributes.Add("class", "icon-tt-pencil " + If(isOwner <> 1, "hide", ""))
        dvinfodoc.InnerHtml = output.multi_pw_data(active_multi_index).InfodocLeft
        Dim is_screen_reduced = False
        dvinfodoc.Attributes.Add("class", "left-node-info-text " + If(is_screen_reduced, "zoom-out", ""))
        dvParentNode.Attributes.Add("class", "columns original-info-doc " + If((String.IsNullOrWhiteSpace(output.parent_node_info) And isOwner = 0), "hide", "") + " " + If(isOwner = 1 Or (isOwner = 0 And Not String.IsNullOrWhiteSpace(output.parent_node_info)), "large-6", "") + " " + If(isOwner = 0 And String.IsNullOrWhiteSpace(output.parent_node_info), "large-2", ""))
        a_0.Attributes.Add("class", "tt-toggler-0 " + If(isOwner = 1 Or (isOwner = 0 And Not String.IsNullOrWhiteSpace(output.parent_node_info)), "tt-toggler", ""))
        sp_infodoc.Attributes.Add("class", "icon icon-desktop " + If((output.showinfodocnode) And Not output.multi_collapse_default(0), "icon-tt-minus-square", "") + " " + If(Not output.showinfodocnode Or output.multi_collapse_default(0), "icon-tt-plus-square", ""))
        sp_p0.InnerText = If(output.hideInfoDocCaptions, "", output.parent_node)
        If Not (isMobile Or output.is_infodoc_tooltip) And isOwner = 0 And Not String.IsNullOrWhiteSpace(output.parent_node_info) Then
            a_pnode.Visible = True
        End If
        a_pnode.Attributes.Add("data-index", active_multi_index.ToString())
        a_pnode.Attributes.Add("data-node-description", output.parent_node)
        txtpNode.Value = output.parent_node
        dvinfodoc_1.Attributes("class") += If(Not output.showinfodocnode Or output.multi_collapse_default(0) Or (String.IsNullOrWhiteSpace(output.parent_node_info) And isOwner = 0), "hide", "") + " " + If(isMobile, "infodoc-height-mobile", "")
        ainfodoc.Attributes.Add("data-index", active_multi_index.ToString())
        ainfodoc.Attributes.Add("data-node-description", output.parent_node)
        ainfodoc.Attributes("class") += If(isOwner = 1 Or Not output.showinfodocnode Or output.multi_collapse_default(0), "hide", "")
        spicon.Attributes("class") += If(isOwner <> 1, "hide", "")
        dvText.InnerHtml = output.parent_node_info
        dvText.Attributes("class") += If(is_screen_reduced, "zoom-out", "")
        If Not isMobile Then
            dvNoMmbile.Visible = True
        End If
        dvNoMmbile.Attributes("class") += If((String.IsNullOrWhiteSpace(output.multi_pw_data(active_multi_index).InfodocRight) And isOwner = 0), "hide", "") + " " + If(isOwner = 1 Or (isOwner = 0 And Not String.IsNullOrWhiteSpace(output.parent_node_info)), "large-3", "") + " " + If(isOwner = 0 And String.IsNullOrWhiteSpace(output.parent_node_info), "large-4", "")
        a_2.Attributes("class") += If(isOwner = 1 Or (isOwner = 0 And Not String.IsNullOrWhiteSpace(output.multi_pw_data(active_multi_index).InfodocRight)), "tt-toggler", "")
        sp_icon_1.Attributes("class") += If((output.showinfodocnode) And Not output.multi_collapse_default(2), "icon-tt-minus-square", "") + " " + If(Not output.showinfodocnode Or output.multi_collapse_default(2), "icon-tt-plus-square", "")
        sp_2.InnerText = output.multi_pw_data(active_multi_index).RightNode
        If Not (isMobile Or output.is_infodoc_tooltip) And isOwner = 0 And Not String.IsNullOrWhiteSpace(output.multi_pw_data(active_multi_index).InfodocRight) Then
            a_maginify2.Visible = True
        End If
        a_maginify2.Attributes.Add("data-index", active_multi_index.ToString())
        a_maginify2.Attributes.Add("data-node-id", output.multi_pw_data(active_multi_index).NodeID_Right.ToString())
        a_maginify2.Attributes.Add("data-node-description", output.multi_pw_data(active_multi_index).RightNode)
        a_maginify2.Attributes.Add("data-node-title", output.multi_pw_data(active_multi_index).RightNode)
        txtpnode1.Value = output.multi_pw_data(active_multi_index).RightNode
        dvcontent1.Attributes("class") += If(Not output.showinfodocnode Or output.multi_collapse_default(2) Or (String.IsNullOrWhiteSpace(output.multi_pw_data(active_multi_index).InfodocRight) And isOwner = 0), "hide", "")
        aNode1.Attributes.Add("data-index", active_multi_index.ToString())
        aNode1.Attributes.Add("data-node-id", output.multi_pw_data(active_multi_index).NodeID_Right.ToString())
        aNode1.Attributes.Add("data-node-description", output.multi_pw_data(active_multi_index).RightNode)
        aNode1.Attributes.Add("data-node-title", output.multi_pw_data(active_multi_index).RightNode)
        aNode1.Attributes("class") += If(isOwner <> 1 Or Not output.showinfodocnode Or output.multi_collapse_default(2), "hide", "")
        sp_icon2.Attributes("class") += If(isOwner <> 1, "hide", "")
        dvDocright.Attributes("class") += If(is_screen_reduced, "zoom-out", "")
        dvDocright.InnerHtml = output.multi_pw_data(active_multi_index).InfodocRight
        If (isOwner = 0 And String.IsNullOrWhiteSpace(output.parent_node_info)) Then
            dvBlank3.Visible = True
        End If
        If isOwner = 0 And String.IsNullOrWhiteSpace(output.parent_node_info) Then
            dvBlank4.Visible = True
        End If
        dvleft3.Attributes("class") += If((String.IsNullOrWhiteSpace(output.multi_pw_data(active_multi_index).InfodocLeftWRT) And isOwner = 0) Or isMobile, "hide", "")
        dvleft3.Attributes("class") += If(isOwner = 1 Or (isOwner = 0 And Not String.IsNullOrWhiteSpace(output.parent_node_info)), "large-3", "")
        dvleft3.Attributes("class") += If(isOwner = 0 And String.IsNullOrWhiteSpace(output.parent_node_info), "large-4", "")
        a_3.Attributes("class") += If(isOwner = 1 Or (isOwner = 0 And Not String.IsNullOrWhiteSpace(output.multi_pw_data(active_multi_index).InfodocLeftWRT)), "tt-toggler", "")
        a_3.Attributes.Add("onClick", "set_collapse_cookies('wrt-left-node'); update_infodoc_params('wrt-left-node', " + output.ParentNodeGUID.ToString() + ", " + output.ParentNodeGUID.ToString() + ")")
        spicon3.Attributes("class") += If((output.showinfodocnode) And Not output.multi_collapse_default(3), "icon-tt-minus-square", "") + " " + If(Not output.showinfodocnode Or output.multi_collapse_default(3), "icon-tt-plus-square", "")
        sp_3.InnerText = If(output.hideInfoDocCaptions, "", output.multi_pw_data(active_multi_index).InfodocLeftWRT)
        If Not (isMobile Or output.is_infodoc_tooltip) And isOwner = 0 And Not String.IsNullOrWhiteSpace(output.multi_pw_data(active_multi_index).InfodocLeftWRT) Then
            aleftnode.Visible = True
            aleftnode.Attributes.Add("data-index", active_multi_index.ToString())
            aleftnode.Attributes.Add("data-node-id", output.multi_pw_data(active_multi_index).NodeID_Left.ToString())
            aleftnode.Attributes.Add("data-node-description", output.parent_node)
            aleftnode.Attributes.Add("data-node-title", output.multi_pw_data(active_multi_index).LeftNode)
        End If
        Dim collapsed_info_docs = output.multi_collapse_default
        dvleftnode1.Attributes("class") += If(Not output.showinfodocnode Or collapsed_info_docs(3) Or (String.IsNullOrWhiteSpace(output.multi_pw_data(active_multi_index).InfodocLeftWRT) And isOwner = 0), "hide", "")
        anodeleft1.Attributes.Add("data-index", active_multi_index.ToString())
        anodeleft1.Attributes.Add("data-node-id", output.multi_pw_data(active_multi_index).NodeID_Left.ToString())
        anodeleft1.Attributes.Add("data-node-description", output.parent_node)
        anodeleft1.Attributes.Add("data-node-title", output.multi_pw_data(active_multi_index).LeftNode)
        sp_icon3.Attributes("class") += If(isOwner <> 1, "hide", "")
        dvdocleft.Attributes("class") += If(is_screen_reduced, "zoom-out", "")
        dvdocleft.InnerHtml = output.multi_pw_data(active_multi_index).InfodocLeftWRT
        dvnode3.Attributes("class") += If(isOwner = 1 Or (isOwner = 0 And Not String.IsNullOrWhiteSpace(output.parent_node_info)), "large-6", "")
        dvnode3.Attributes("class") += If(isOwner = 0 And String.IsNullOrWhiteSpace(output.parent_node_info), "large-2", "")
        dvnode4.Attributes("class") += If((String.IsNullOrWhiteSpace(output.multi_pw_data(active_multi_index).InfodocRightWRT) And isOwner = 0) Or isMobile, "hide", "")
        dvnode4.Attributes("class") += If(isOwner = 1 Or (isOwner = 0 And Not String.IsNullOrWhiteSpace(output.parent_node_info)), "large-3", "")
        dvnode4.Attributes("class") += If(isOwner = 0 And String.IsNullOrWhiteSpace(output.parent_node_info), "large-4", "")
        a_4.Attributes.Add("onClick", "set_collapse_cookies('wrt-right-node'); update_infodoc_params('wrt-right-node', " + output.ParentNodeGUID.ToString() + ", " + output.ParentNodeGUID.ToString() + ")")
        a_4.Attributes("class") += If(isOwner = 1 Or (isOwner = 0 And Not String.IsNullOrWhiteSpace(output.multi_pw_data(active_multi_index).InfodocRightWRT)), "tt-toggler", "")
        spicon4.Attributes("class") += If((output.showinfodocnode) And Not collapsed_info_docs(4), "icon-tt-minus-square", "")
        spicon4.Attributes("class") += If(Not output.showinfodocnode Or collapsed_info_docs(4), "icon-tt-plus-square", "")
        sp_4.InnerHtml = If(output.hideInfoDocCaptions, "", output.multi_pw_data(active_multi_index).InfodocRightWRT)
        If Not (isMobile Or output.is_infodoc_tooltip) And isOwner = 0 And Not String.IsNullOrWhiteSpace(output.multi_pw_data(active_multi_index).InfodocRightWRT) Then
            anodeleft2.Visible = True
            anodeleft2.Attributes.Add("data-index", active_multi_index.ToString())
            anodeleft2.Attributes.Add("data-node-id", output.multi_pw_data(active_multi_index).NodeID_Right.ToString())
            anodeleft2.Attributes.Add("data-node-description", output.parent_node)
            anodeleft2.Attributes.Add("data-node-title", output.multi_pw_data(active_multi_index).RightNode)
        End If
        dvnode5.Attributes("class") += If(Not output.showinfodocnode Or collapsed_info_docs(4) Or (String.IsNullOrWhiteSpace(output.multi_pw_data(active_multi_index).InfodocRightWRT) And isOwner = 0), "hide", "")
        anode5left.Attributes.Add("data-index", active_multi_index.ToString())
        anode5left.Attributes.Add("data-node-id", output.multi_pw_data(active_multi_index).NodeID_Right.ToString())
        anode5left.Attributes.Add("data-node-description", output.parent_node)
        anode5left.Attributes.Add("data-node-title", output.multi_pw_data(active_multi_index).RightNode)
        sp_5.Attributes("class") += If(isOwner <> 1, "hide", "")
        dvdocright2.Attributes("class") += If(is_screen_reduced, "zoom-out", "")
        dvdocright2.InnerHtml = output.multi_pw_data(active_multi_index).InfodocRightWRT
        If isOwner = 0 And String.IsNullOrWhiteSpace(output.parent_node_info) Then
            dvBlank5.Visible = True
        End If








    End Sub

End Class