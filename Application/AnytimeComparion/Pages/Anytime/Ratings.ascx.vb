Imports System.Web.Services

Public Class Ratings
    Inherits System.Web.UI.UserControl

    Dim screenCheck As ScreenCheck = New ScreenCheck()

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    End Sub

    Public Sub bindHtml(ByVal model As AnytimeOutputModel)
        If model IsNot Nothing Then
            Dim html As StringBuilder = New StringBuilder()
            'Dim parentnodeinfo As String = HttpUtility.UrlEncode(If(model.parent_node_info <> "" AndAlso model.parent_node_info.Trim() <> "", model.parent_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)), ""))
            Dim firstnodeinfo As String = If(model.first_node_info IsNot Nothing, HttpUtility.UrlEncode(model.first_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))), "")
            Dim parentnodeinfo As String = If(model.parent_node_info IsNot Nothing, HttpUtility.UrlEncode(model.parent_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))), "")
            Dim wrtfirstnodeinfo As String = If(model.wrt_first_node_info IsNot Nothing, HttpUtility.UrlEncode(model.wrt_first_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))), "")
            Dim wrttext As String = GetWrtText("Left", model.child_node, model.parent_node)
            Dim stepTask As String = If(model.step_task IsNot Nothing, HttpUtility.UrlEncode(model.step_task.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))), "")

            'Dim steptask As String = HttpUtility.UrlEncode(model.cluster_phrase.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
            Dim parentnode As String = If(model.parent_node IsNot Nothing, HttpUtility.UrlEncode(model.parent_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))), "")
            'Dim parentnodeinfo As String = HttpUtility.UrlEncode(output.parent_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))

            'Header title div
            html.Append("<div class='row'>")
            html.Append("<div class='page_heading_section' style='padding-top:20px;'>")
            html.Append("<div class='container'>")

            html.Append("<div class='row'>")
            html.Append("<div class='col-md-6'>")

            'html.Append("<div class='heading_content' id='titleDiv'>")
            'html.Append("<div class='txtStepTask'>")
            'html.Append("<p>" + output.step_task + "</p>")
            'html.Append("</div>")
            'html.Append("<div class='heading_info'><img src='../../img/icon/empty_info.svg' class='d-none'><img src='../../img/icon/info_data.svg' ><img src='../../img/icon/info-close.svg' class='d-none'></div>")

            'html.Append("<div class='col-md-12'><div class='font-size-16 text-center prm-color d-flex  justify-content-center'><p class='font-size-18 prm-color text-center'><div class='txtStepTask'><p>" + output.step_task + "</p></div><br />")
            'html.Append($"<div class='editStepTask ms-3'><a{If(output.isPipeViewOnly, " class='d-none'", "")} href='javascript:void(0);' style='cursor:pointer;' onclick='showHeaderPopup(decodeURIComponent(" + ChrW(&H22) + steptask + ChrW(&H22) + ")" + ",0,0,2,1,null)'><i Class='bx bxs-edit-alt chkEvaluator d-none'></i></a></div></p> </div>")
            'html.Append("</div>")
            Dim header_Model_text As String = If(model.cluster_phrase IsNot Nothing, HttpUtility.UrlEncode(model.cluster_phrase.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))), "")
            html.Append("<div class='heading_content'><div class='page-title-box' id='titleDiv'>")
            html.Append($"<div Class='heading_icons'> <a {If(model.isPipeViewOnly, " class='d-none'", " class='editsvg_icon'")}  href='javascript:void(0);' onclick=showHeaderPopup(decodeURIComponent({ChrW(&H22) + header_Model_text + ChrW(&H22)}),'0','0','0','0',null,'question-node',decodeURIComponent({ChrW(&H22) + header_Model_text + ChrW(&H22)}))><img class='chkEvaluatorone' src='../../img/icon/edit-icon.svg'></a>")
            html.Append("<asp:Label ID='lblTask' runat='server' ><span id='divT2S' style='font-size:20px; margin-left:1ex border: 0px !important; background: #f0f0ff08 !important; display:none'></span></asp:Label>")
            html.Append("</div>")
            html.Append($"<div class='txtStepTask removep'  id='MainHeaderInfodoc'>{model.step_task}<span class='threedoticon d-none' id='btnheadinfo'> <a href='#' onclick='showheaderpopup()' ><div class='snippet'><div class='stage'><div class='dot-falling'></div> </div></div></a></span></div>")

            'html.Append("<div class='heading_info'><img src='../../img/icon/empty_info.svg' class='d-none'><img src='../../img/icon/info_data.svg'><img src='../../img/icon/info-close.svg' class='d-none'></div>")
            Dim IsHideHeaderInfo As Boolean = False
            'If model.parent_node_info Is Nothing Or model.parent_node_info = "" Then
            '    html.Append($"<div class='heading_info' id='HeaderDivIcon'><a href='#' ><img  id='img1' src='../../img/icon/empty_info.svg'><img id='img2' src='../../img/icon/info_data.svg' style='display: none;'><img id='img3' src='../../img/icon/info-close.svg' style='display: none;'></a></div>")
            '    IsHideHeaderInfo = True
            'Else
            '    html.Append($"<div class='heading_info' id='HeaderDivIcon'><a href='#'><img  id='img1' src='../../img/icon/empty_info.svg' style='display: none;'><img id='img2' src='../../img/icon/info_data.svg'><img id='img3' src='../../img/icon/info-close.svg' style='display: none;'></a></div>")
            'End If

            html.Append("</div>")
            html.Append("</div>")
            Dim NoInfoDocDataCls As String = ""
            If model.parent_node_info.ToString().Trim() = "" Then
                NoInfoDocDataCls = "NoInfoData"
            End If
            'html.Append("<asp:Label ID='lblTask' runat='server' /><span id='divT2S' style='font-size:20px; margin-left:1ex border: 0px !important; background: #f0f0ff08 !important; display:none'></span>")
            'html.Append("</div>")
            html.Append("</div>")
            html.Append($"<div class='col-md-6 {NoInfoDocDataCls}'>")
            If model.is_infodoc_tooltip Or model.showinfodocnode Then
                Dim headerText As String = $"{model.first_node}{model.child_node}"
                'html.Append($"<p'>{output.parent_node_info}</p><p></p></p>")
                html.Append($"<div class='info_content d-flex mt-lg-0 mt-2' id='Header_InfoDocs' {If(IsHideHeaderInfo, " style='display: none;'", "")}><div class='heading_icons heading_info me-0'><div  id='HeaderDivIcon'><a href='#'><img  id='img1' src='../../img/icon/empty_info.svg' style='display: none;'><img id='img2' src='../../img/icon/info_data.svg'><img id='img3' src='../../img/icon/info-close.svg' style='display: none;'></a></div><div class='infodoc_icons'><a class='editsvg_icon' href='javascript:void(0);' onclick=showInfoPopup(decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.parent_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}),'0','-1','0','-1',null,'parent-node',decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.parent_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}))><img class='chkEvaluatorone' src='../../img/icon/edit-icon.svg'></a><a href='#' onclick=Expandtxt(decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.parent_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}),'0','-1','0','-1',null,'parent-node',decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.parent_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)})) data-bs-toggle='modal' data-bs-target='#infodocPop'><img src='../../img/icon/full-screen-svgrepo-com.svg' id='ibtnFullscreen'></a></div></div><div class='info_content_wrapper'><div class='info_text'>{model.parent_node_info}</div><div class='info_box_icons d-none'><a class='editsvg_icon chkEvaluatorone' href='javascript:void(0);' onclick='showInfoPopup(decodeURIComponent(" + ChrW(&H22) + parentnodeinfo + ChrW(&H22) + ")" + ",0,-1,8,-1,0, " + ChrW(&H22) + "parent-node1" + ChrW(&H22) + ", decodeURIComponent(" + ChrW(&H22) + model.parent_node + ChrW(&H22) + ")" + ")'>")
                'html.Append($"<div class='info_content'><div class='info_content_wrapper'><div class='info_text'>{model.first_node_info}</div><div class='info_box_icons'><a class='editsvg_icon chkEvaluatorone' href='javascript:void(0);' onclick='showInfoPopup(decodeURIComponent(" + ChrW(&H22) + firstnodeinfo + ChrW(&H22) + ")" + ",0,2,8,1,0,null, decodeURIComponent(" + ChrW(&H22) + model.child_node + ChrW(&H22) + ")" + ")'>")
                html.Append($"<img class='chkEvaluatorone' src='../../img/icon/edit-icon.svg'></a><a href='javascript:void(0);' data-bs-toggle='modal' data-bs-target='#infodocPop'><img src='../../img/icon/full-screen-svgrepo-com.svg' onclick='Expandtxt(decodeURIComponent(" + ChrW(&H22) + model.parent_node + ChrW(&H22) + ")" + ",0,-1,8,-1,0, " + ChrW(&H22) + "parent-node1" + ChrW(&H22) + ", decodeURIComponent(" + ChrW(&H22) + parentnodeinfo + ChrW(&H22) + ")," + ")'></a></div></div></div>")
            End If
            html.Append("</div>")
            'html.Append("<div class='info_box_icons'><a class='editsvg_icon' href='javascript:void(0);' onclick='showInfoPopup(decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(output.parent_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}),'0','-1','0','-1',null,'parent-node',decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(output.parent_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}))'><img src='../../img/icon/edit-icon.svg'></a><a href='javascript:void(0);' data-bs-toggle='modal' data-bs-target='#exampleModal'><img src='../../img/icon/full-screen-svgrepo-com.svg'></a></div>")
            html.Append("</div>")
            'html.Append($"<div class='modal fade' id='exampleModaltest' tabindex='-1' aria-labelledby='exampleModalLabel' aria-hidden='True'><div class='modal-dialog modal-fullscreen'><div class='modal-content'><div class='modal-header'><h3 class='modal-title'id='exampleModalLabel' style='color:white;'>INFODOCS</h3><button type='button' class='btn-close' data-bs-dismiss='modal' aria-label='Close'></button></div><div class='modal-body'><div class='row h-100'><div class='border-End'><div class='modal_info_header'><h3>{output.parent_node}</h3><a href='javascript:void(0);' class='chkEvaluatorone'><img src='../../img/icon/edit-icon.svg' onclick='showInfoPopup(decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(output.parent_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}),'0','-1','0','-1',null,'parent-node',decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(output.parent_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}))'></a></div><div class='modal_info_content'><p>{output.parent_node_info}</p></div></div></div></div></div></div></div>")
            html.Append("</div>")

            If model.is_infodoc_tooltip Or model.showinfodocnode Then
                'html.Append($"<div class='info_center centered_tooltip' id='InfoDocsParentDiv'><p class='font-size-14 text-center {If((parentnodeinfo) <> "" And (parentnodeinfo) <> "NaN", "", "chkEvaluator")}'><i onclick='showNode(101)' class='fa fa-info-circle {If((parentnodeinfo) <> "" And (parentnodeinfo) <> "NaN", "", "iinfoColor")}  definfovalue {If((parentnode) = "" Or (parentnode) = "NaN", "", "headerinfoColor")}'></i> {output.parent_node}</p>")
                html.Append($"<div class='info_tooltip hideshowinfo parentnode101 {If(model.parent_node_info = "", " divchkEvaluator", "")}' style='display:none;' id='parentnode101'><div class='tooltop_head'><span>" + model.parent_node + "</span><div class='action_icons'>")
                html.Append($"<a{If(model.isPipeViewOnly, " class='d-none'", "")} href='javascript:void(0);'><i class='fas fa-edit chkEvaluator d-none' onclick='showInfoPopup(decodeURIComponent(" + ChrW(&H22) + parentnodeinfo + ChrW(&H22) + ")" + ",0,-1,2,-1,0, " + ChrW(&H22) + "parent-node1" + ChrW(&H22) + ", decodeURIComponent(" + ChrW(&H22) + parentnode + ChrW(&H22) + ")" + ")' aria-hidden='true'></i></a>")
                html.Append("<a href='javascript:void(0);'><i class='fa fa-times' onclick='hideNode(101)' aria-hidden='true'></i></a></div></div><p>" + model.parent_node_info.ToString() + "</p></div></div>")
            End If
            If model.pairwise_type = "ptVerbal" Then
                html.Append("<div class='pairwise_legends d-lg-none'><span><strong>EQ</strong>ual</span><span><strong>M</strong>oderate</span><span><strong>S</strong>trong</span><span><strong>V</strong>ery <strong>S</strong>trong</span><span><strong>EX</strong>treme</span></div>")
            End If
            html.Append("</div>")

            'html.Append("<div class='topheader_row col-md-12' id='titleDiv'>")
            'html.Append("<div id='stepTaskDiv' class='collabse_btn'>")
            'html.Append($"<span class=' font-16 headerText'>{model.step_task}</span>")
            'html.Append($"<a{If(model.isPipeViewOnly, " class='d-none'", "")} href='javascript:void(0);' class='ms-3'>")
            'html.Append("<i class='bx bxs-edit-alt chkEvaluator d-none' onclick='showHeaderPopup(decodeURIComponent(" + ChrW(&H22) + stepTask + ChrW(&H22) + "),0,0,0,0,null,decodeURIComponent(" + ChrW(&H22) + "question-node" + ChrW(&H22) + "), decodeURIComponent(" + ChrW(&H22) + stepTask + ChrW(&H22) + ")" + ")' aria-hidden='true'></i>")
            'html.Append("</a>")
            'html.Append("</div>")
            html.Append("</div></div>")

            'nodes like: first, wrt, and parent start
            ''html.Append("<div class='row my-3' id='infoDiv' style='display:none;'>")
            html.Append("<div id='infoDiv' style='display:none;'>")
            'first node start
            html.Append("<div class='col-md-4 offset-md-2'>")
            html.Append($"<div class='info_center colorfix {If(model.first_node_info.ToString() <> "" And model.first_node_info.ToString() <> "NaN", "", "chkEvaluator")}'>")
            'html.Append($"<p class='font-size-14'>")
            If model.showinfodocnode Then
                'html.Append($"<i onclick ='showNode(1)' class='fa fa-info-circle{If(model.first_node_info.ToString() = "" Or model.first_node_info.ToString() = "NaN", " infoColor", "")}{If(model.first_node_info.ToString() <> "" And model.first_node_info.ToString() <> "NaN", "", " chkEvaluator")}'></i>")
                'html.Append($"<i onclick ='showNode(1)' class='fa fa-plus-square{If(model.is_infodoc_tooltip, " d-none", " d-none")}{If(model.first_node_info.ToString() = "" Or model.first_node_info.ToString() = "NaN", " infoColor", "")}{If(model.first_node_info.ToString() <> "" And model.first_node_info.ToString() <> "NaN", "", " chkEvaluator")}'></i>")
            End If
            'html.Append($" {model.first_node}{model.child_node} </p>")
            'html.Append("<div class='info_tooltip hideshowinfo' style='display:none;' id='parentnode1'>")
            'html.Append("<div class='tooltop_head tooltip-header'>")
            'html.Append($"<span>{model.first_node}{model.child_node}</span>")
            'html.Append("<div class='action_icons'>")
            'html.Append($"<a{If(model.isPipeViewOnly, " class='d-none'", "")} href='javascript:void(0);'>")
            'html.Append("<i class='bx bxs-edit-alt chkEvaluator d-none' onclick='showInfoPopup(decodeURIComponent(" + ChrW(&H22) + firstnodeinfo + ChrW(&H22) + ")" + ",0,2,8,1,0,null, decodeURIComponent(" + ChrW(&H22) + firstnodeinfo + ChrW(&H22) + ")" + ")' aria-hidden='true'></i>")
            'html.Append("</a>")
            'html.Append("<a href='javascript:void(0);'>")
            'html.Append("<i class='fa fa-times' onclick='hideNode(1)' aria-hidden='true'></i>")
            'html.Append("</a>")
            'html.Append("</div>")
            'html.Append("</div>")
            'html.Append($"<p>{model.first_node_info}</p>")
            'html.Append("</div>")
            html.Append("</div>")
            html.Append("</div>")
            'first node end

            'Parent node start
            html.Append("<div class='col-md-3'>")
            html.Append($"<div class='info_center colorfix {If(model.parent_node_info.ToString() <> "" And model.parent_node_info.ToString() <> "NaN", "", "chkEvaluator")}'>")
            html.Append("<p class='font-size-14'>")
            'html.Append($"<i onclick ='showNode(2)' class='fa fa-info-circle {If(model.parent_node_info.ToString() = "" Or model.parent_node_info.ToString() = "NaN", "infoColor", "")} {If(model.parent_node_info.ToString() <> "" And model.parent_node_info.ToString() <> "NaN", "", "chkEvaluator")}'></i>")
            'html.Append($"<i onclick ='showNode(2)' class='fa fa-plus-square{If(model.is_infodoc_tooltip, " d-none", " d-none")}{If(model.first_node_info.ToString() = "" Or model.first_node_info.ToString() = "NaN", " infoColor", "")}{If(model.first_node_info.ToString() <> "" And model.first_node_info.ToString() <> "NaN", "", " chkEvaluator")}'></i>")
            'html.Append($" {model.parent_node} </p>")
            html.Append("<div class='info_tooltip hideshowinfo' style='display:none;' id='parentnode2'>")
            html.Append("<div class='tooltop_head tooltip-header'>")
            'html.Append($"<span>{model.parent_node}</span>")
            html.Append("<div class='action_icons'>")
            html.Append($"<a{If(model.isPipeViewOnly, " class='d-none'", "")} href ='javascript:void(0);'>")
            html.Append("<i class='bx bxs-edit-alt chkEvaluator d-none' onclick='showInfoPopup(decodeURIComponent(" + ChrW(&H22) + parentnodeinfo + ChrW(&H22) + ")" + ",0,-1,8,-1,0, " + ChrW(&H22) + "parent-node1" + ChrW(&H22) + ", decodeURIComponent(" + ChrW(&H22) + model.parent_node + ChrW(&H22) + ")" + ")' aria-hidden='true'></i>")
            html.Append("</a>")
            html.Append("<a href ='javascript:void(0);'>")
            html.Append("<i class='fa fa-times' onclick='hideNode(2)' aria-hidden='true'></i>")
            html.Append("</a>")
            html.Append("</div>")
            html.Append("</div>")
            html.Append($"<p>{model.parent_node_info}</p>")
            html.Append("</div>")
            html.Append("</div>")
            html.Append("</div>")
            'Parent node end8

            'first wrt node start
            'html.Append("<div class='col-md-3'>")
            'html.Append($"<div class='info_center colorfix {If(model.wrt_first_node_info.ToString() <> "" And model.wrt_first_node_info.ToString() <> "NaN", "", "chkEvaluator")}'>")
            'html.Append("<p class='font-size-14'>")
            'html.Append($"<span onclick='showNode(3)' class='wrt_text{If(model.wrt_first_node_info.ToString() <> "" And model.wrt_first_node_info.ToString() <> "NaN", "", " winfoColor")}{If(model.wrt_first_node_info.ToString() <> "" And model.wrt_first_node_info.ToString() <> "NaN", "", " chkEvaluator")}'> wrt </span>")
            'html.Append($"{wrttext}</p>")
            'html.Append("<div class='info_tooltip hideshowinfo' style='display:none;' id='parentnode3'>")
            'html.Append("<div class='tooltop_head tooltip-header'>")
            'html.Append($"<span>{wrttext}</span>")
            'html.Append("<div class='action_icons'>")
            'html.Append($"<a{If(model.isPipeViewOnly, " class='d-none'", "")} href ='javascript:void(0);'>")
            'html.Append("<i class='bx bxs-edit-alt chkEvaluator d-none' onclick='showInfoPopup(decodeURIComponent(" + ChrW(&H22) + wrtfirstnodeinfo + ChrW(&H22) + ")" + ",0,3,8,1,0, " + ChrW(&H22) + "parent-node1" + ChrW(&H22) + ", decodeURIComponent(" + ChrW(&H22) + wrttext + ChrW(&H22) + ")" + ")' aria-hidden='true'></i>")
            'html.Append("</a>")
            'html.Append("<a href ='javascript:void(0);'>")
            'html.Append("<i class='fa fa-times' onclick='hideNode(3)' aria-hidden='true'></i>")
            'html.Append("</a>")
            'html.Append("</div>")
            'html.Append("</div>")
            'html.Append($"<p>{model.wrt_first_node_info}</p>")
            'html.Append("</div>")
            'html.Append("</div>")
            'html.Append("</div>")
            'first wrt node end
            html.Append("</div>")
            'nodes like: first, wrt, and parent end
            html.Append("<div class='container'>")
            html.Append("<div class='row'>")
            'left item wise dropdown selection start
            html.Append("<div class='col-lg-6'>")
            html.Append("<div class='progress_table_row rating_scale_data'>")
            Dim NoInfDocDataRow As String = ""
            NoInfoDocDataCls = ""
            If model.first_node_info.ToString().Trim() = "" And model.wrt_first_node_info.ToString().Trim() = "" Then
                NoInfDocDataRow = "NoInfDocDataRow"
                NoInfoDocDataCls = "NoInfoData"
            End If
            If model.first_node_info.ToString().Trim() = "" Or model.wrt_first_node_info.ToString().Trim() = "" Then
                html.Append($"<div id='div_row_wrapper_0' class='row_wrapper direct_entry_comment active_table_row shot_data'>")
            Else
                html.Append($"<div id='div_row_wrapper_0' class='row_wrapper direct_entry_comment active_table_row'>")
            End If
            html.Append("<div class='tooltips_group'>")
            html.Append($"<div Class='info_tooltip_main position-relative {NoInfoDocDataCls}'>")
            html.Append("<div Class='comment_div'>")
            html.Append("<div Class='information_icon'>")
            html.Append("<a href ='javascript:void(0);'><img src = '../../img/icon/info_data.svg' ></a>")
            html.Append("</div>")

            html.Append("</div>")
            html.Append("</div>")
            If model.show_comments Then
                html.Append("<div class='info_tooltip_main position-relative'>")

                html.Append($"<div class='comment_div' id='comment_div_0'>")
                If model.comment Is Nothing Or model.comment = "" Then
                    If Not (screenCheck.isMobileBrowserClient(Request)) Then
                        html.Append($"<a href ='javascript:void(0);' id='comment_icon_0' onclick=updateRightSideComment('0','Ratingcomment','open',event) ><img src = '../../img/icon/Grey-plus-icon.svg' id='ImgComment_iconEmpty_0' title='' aria-hidden='true'>  <img src = '../../img/icon/comment-svgrepo-com.svg' style='display:none;' id='ImgComment_icon_0' title='' aria-hidden='true'></a>")
                    Else
                        html.Append($"<a href ='javascript:void(0);' id='comment_icon_0' onclick=updateRightSideCommentMobile('0','Ratingcomment','open',event) ><img src = '../../img/icon/Grey-plus-icon.svg' id='ImgComment_iconEmpty_0' title='' aria-hidden='true'>  <img src = '../../img/icon/comment-svgrepo-com.svg' style='display:none;' id='ImgComment_icon_0' title='' aria-hidden='true'></a>")
                    End If
                Else
                    If Not (screenCheck.isMobileBrowserClient(Request)) Then
                        html.Append($"<a href ='javascript:void(0);' id='comment_icon_0' onclick=updateRightSideComment('0','Ratingcomment','open',event)><img src = '../../img/icon/Grey-plus-icon.svg' style='display:none;' id='ImgComment_iconEmpty_0' title='' aria-hidden='true'>  <img src = '../../img/icon/comment-svgrepo-com.svg' id='ImgComment_icon_0' title='' aria-hidden='true'></a>")
                    Else
                        html.Append($"<a href ='javascript:void(0);' id='comment_icon_0' onclick=updateRightSideCommentMobile('0','Ratingcomment','open',event)><img src = '../../img/icon/Grey-plus-icon.svg' style='display:none;' id='ImgComment_iconEmpty_0' title='' aria-hidden='true'>  <img src = '../../img/icon/comment-svgrepo-com.svg' id='ImgComment_icon_0' title='' aria-hidden='true'></a>")
                    End If
                End If
                    'html.Append($"<div class='brand_name'><span>{model.child_node}</span></div>")
                    html.Append("<div class='tooltip_wrapper hideshowinfo info_tooltip right_tooltip' id='comment_div_box_0' style='display: none;'>")
                    html.Append("<div class='d-flex justify-content-between tooltip-header'>")
                    html.Append("<span>Add your comment</span>")
                    html.Append($"<div class='action_icons' onclick=HideMultiDerectCommentBox(0)>")
                    html.Append("<a href='javascript:void(0);'><i class='fa fa-times' aria-hidden='true'></i></a>")
                    html.Append("</div>")
                    html.Append("</div>")
                    html.Append($"<textarea{If(model.isPipeViewOnly, " style='pointer-events:none;'", "")} class='form-control mb-3 mt-2 w-100' placeholder='Add your comment' id='txtRightComment_0' rows='3'>{model.comment}</textarea>")
                    html.Append("<div class='comt_btn text-end'>")
                    html.Append($"<button{If(model.isPipeViewOnly, " class='d-none'", "")} type='button' onclick=updateRightSideComment('0','Ratingcomment','close',event)><i class='fa fa-check' aria-hidden='true'></i> OK</button>")
                    html.Append("</div>")
                    html.Append("</div>")
                    html.Append("</div>")
                    html.Append("</div>")
                End If

            html.Append($"<div class='brand_name'><span class='text-decoration-none fw-500 fz-16' title='{model.child_node}'>{model.child_node}</span></div>")

                html.Append("</div>")

                Dim DropDownListID As String = "UiDropdownList"

                html.Append($"<div class='drop_progress '{If(model.isPipeViewOnly, " style='pointer-events:none;'", "")}>")
                html.Append($"<div class='progres_dropdowns  d-none d-md-block' id='ratings-dropdown0' onclick='ShowDropDownListDesktop({DropDownListID}D)'>")
                html.Append("<div class='progress' style='height: 25px;'>")
            html.Append($"<div id='progress_bar_0' class='progress-bar' role='progressbar' aria-valuenow='100' aria-valuemin='0' aria-valuemax='100'><span id='dropdownHeaderValue0'  ><span id='spndropdownHeaderValueM0' class='direct_value_data'>DataValue</span></span></span><small class='customdropdown_btn'></small></div>")

            html.Append("</div>")

                html.Append($"<div class='custom_dropdown custom_dropdown_dekstop w-100 d-block'>
                            <div class='options'>
                              <ul id='{DropDownListID}D'>")
                If model.intensities IsNot Nothing Then
                    For j = 0 To model.intensities.Count - 1
                        Dim text As String = HttpUtility.UrlEncode(model.name.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
                        If model.intensities(j)(1).ToLower() = "direct value" Or model.intensities(j)(1).ToLower() = "not rated" Then
                            html.Append($"<li onclick='save_ratings({model.intensities(j)(2)})' data-value='{model.intensities(j)(0)}#0'> <a href='#'>{model.intensities(j)(1)}</a></li>")
                        Else
                            If model.showPriorityAndDirect Then
                                html.Append($"<li onclick='save_ratings({model.intensities(j)(2)})' data-value='{model.intensities(j)(0)}#0'> <a href='#'>{model.intensities(j)(1)} <span>{Math.Round(Convert.ToDecimal(model.intensities(j)(0)) * 100, 2)}%</span></a></li>")
                            Else
                                html.Append($"<li onclick='save_ratings({model.intensities(j)(2)})' data-value='{model.intensities(j)(0)}#0'> <a href='#'>{model.intensities(j)(1)} </a></li>")
                            End If
                        End If
                    Next
                End If
                html.Append("</ul>
                            </div>
                          </div>")
                'html.Append("<div class='progress'>")
                'html.Append($"<div id='progress_bar_0' class='progress-bar' role='progressbar6' style='width: 100%' aria-valuenow='100' aria-valuemin='0' aria-valuemax='100'></div>")
                'html.Append("</div>")
                'html.Append("<div class='outer-wrapper'>")
                'html.Append("<div class='inner-wrapper'>")
                'html.Append("<div class='explanation'>")
                'html.Append($"<div id='dropdown0' class='dropdown' onclick=toggleFunction('0',event)>")
                'html.Append($"<div id='dropdownHeader0' class='dropdown__header dropdown__header--hide'>")
                'html.Append($"<div id='dropdownHeaderValue0' class='dropdown__header--title'>Please Select Item...</div>")
                'html.Append("<div class='dropdown__header--icon'>")
                'html.Append($"<i id='chevronIcon0' class='chevronIcons fas fa-angle-down rotate-icon-home'></i>")
                'html.Append("</div>")
                'html.Append("</div>")

                'html.Append($"<div id='dropdownListBody0' class='dropdown__body dropdown__body--hide'>")
                'html.Append($"<ul id='dropdownList0' class='dropdown__body--list'>")
                'If model.intensities IsNot Nothing Then
                '    For j = 0 To model.intensities.Count - 1
                '        Dim text As String = HttpUtility.UrlEncode(model.intensities(j)(1).ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
                '        If model.intensities(j)(1).ToLower() = "direct value" Or model.intensities(j)(1).ToLower() = "not rated" Then
                '            html.Append($"<li onclick='save_ratings({model.intensities(j)(2)})' data-value='{model.intensities(j)(0)}#0' class='dropdown__body--list-index'>{model.intensities(j)(1)}</li>")
                '        Else
                '            If model.showPriorityAndDirect Then
                '                html.Append($"<li onclick='save_ratings({model.intensities(j)(2)})' data-value='{model.intensities(j)(0)}#0' class='dropdown__body--list-index'>{model.intensities(j)(1)} <span>{Math.Round(Convert.ToDecimal(model.intensities(j)(0)) * 100, 2)}%</span></li>")
                '            Else
                '                html.Append($"<li onclick='save_ratings({model.intensities(j)(2)})' data-value='{model.intensities(j)(0)}#0' class='dropdown__body--list-index'>{model.intensities(j)(1)} </li>")
                '            End If

                '        End If
                '    Next
                'End If
                'html.Append("</ul>")
                'html.Append("</div>")
                'html.Append("</div>")
                'html.Append("</div>")
                'html.Append("</div>")
                'html.Append("</div>")
                html.Append("</div>")
                html.Append($"<div class='custom_dropdown d-block d-md-none' onclick='ShowDropDownList({DropDownListID})'>
                            <div class='selected'>
                              <a href='#'><span id='dropdownHeaderValueM0'><span id='spndropdownHeaderValueM0' class='direct_value_data'>DataValu</span></span><small class='customdropdown_btn'></small></a>
                            </div>
                            <div class='options'>
                              <ul id='{DropDownListID}'>")
                If model.intensities IsNot Nothing Then
                    For j = 0 To model.intensities.Count - 1
                        Dim text As String = HttpUtility.UrlEncode(model.name.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
                        If model.intensities(j)(1).ToLower() = "direct value" Or model.intensities(j)(1).ToLower() = "not rated" Then
                            html.Append($"<li onclick='save_ratings({model.intensities(j)(2)})' data-value='{model.intensities(j)(0)}#0'> <a href='#'>{model.intensities(j)(1)}</a></li>")
                        Else
                            If model.showPriorityAndDirect Then
                                html.Append($"<li onclick='save_ratings({model.intensities(j)(2)})' data-value='{model.intensities(j)(0)}#0'> <a href='#'>{model.intensities(j)(1)} <span>{Math.Round(Convert.ToDecimal(model.intensities(j)(0)) * 100, 2)}%</span></a></li>")
                            Else
                                html.Append($"<li onclick='save_ratings({model.intensities(j)(2)})' data-value='{model.intensities(j)(0)}#0'> <a href='#'>{model.intensities(j)(1)} </a></li>")
                            End If
                        End If
                    Next
                End If
                html.Append("</ul>
                            </div>
                          </div>")
                If model.showPriorityAndDirect Then
                    html.Append("<div class='brand_value'>")
                    html.Append($"<input type='text' onkeypress='return isNumberKeyWithDecimal(this, event);' class='value_control' title='Enter number 0 to 1' min='0' max='1'  id='direct_value_input'>")
                    html.Append("</div>")
                End If

                'If (model.multi_non_pw_data(i).RatingID = -1 And model.multi_non_pw_data(i).DirectData >= 0) Or model.multi_non_pw_data(i).RatingID >= 0 Then
                html.Append($"<div class='close_icon mt-1' id='close_icon_0' onclick='ResetRatingValue()'>")
                html.Append("<div class='close_icon mt-1'><img src='../../img/icon/erasar.svg' style='cursor: pointer;' class='imgClose'  /></div>")
                'html.Append("<i class='fa fa-times' aria-hidden='true'></i>")
                html.Append("</div>")
                'End If
                html.Append("</div>")

                html.Append($"<div Class='open_tooltip_info {NoInfDocDataRow} {NoInfoDocDataCls}'>")
                html.Append("<hr>")
                html.Append($"<div class='body_content'><div class='split_content scale_infodoc multirating_infodoc'>")
                If (model.first_node_info <> "") Then
                    html.Append($" <div class='normal_content'><span>{model.first_node_info}</span>")
                Else
                    html.Append(" <div class='normal_content NoInfoData'><h2 class='nodata_level'>No Data</h2>")
                End If
                'html.Append($"<span>{model.parent_node_info}</span>")

                html.Append("</div>")
                'html.Append("</div>")
                'html.Append("</div>")
                If (model.wrt_first_node_info <> "") Then
                    html.Append($"<div class='wrt_right_content'> <h3>WITH RESPECT TO <span>{model.parent_node}</span></h3><div class='removeptagw removeptagw'>")
                    html.Append($"<p>{model.wrt_first_node_info} </p>")
                Else
                    html.Append($"<div class='wrt_right_content NoInfoData'> <h3>WITH RESPECT TO <span>{model.parent_node}</span></h3><div class='removeptagw removeptagw'>")
                    html.Append("<h2 class='nodata_level'>No Data</h2>")
                End If
                'html.Append("<div Class='info_datafooter'>")
                'html.Append($"<Button type='button' {If((wrtfirstnodeinfo.ToString().Trim()) <> "", " class=''", " class='nowrt_data' ")} data-bs-toggle='modal' data-bs-target='#exampleModal' onclick='SetExpendedPopupElement(decodeURIComponent(" + ChrW(&H22) + parentnodeinfo + ChrW(&H22) + ")" + ",0,-1,8,-1,0, " + ChrW(&H22) + "parent-node1" + ChrW(&H22) + ", decodeURIComponent(" + ChrW(&H22) + model.parent_node + ChrW(&H22) + "),null,null," + "decodeURIComponent(" + ChrW(&H22) + wrttext + ChrW(&H22) + ")" + ",0,3,8,1,0, " + ChrW(&H22) + "parent-node1" + ChrW(&H22) + ", decodeURIComponent(" + ChrW(&H22) + wrtfirstnodeinfo + ChrW(&H22) + ")" + ")'
                '>With respect To <img src='../../img/icon/button_arrow.svg'></button>")

                'html.Append($"<Button type='button' {If((wrtfirstnodeinfo.ToString().Trim()) <> "", " class=''", " class='nowrt_data' ")} data-bs-toggle='modal' data-bs-target='#exampleModal' onclick='SetExpendedPopupElement(decodeURIComponent(" + ChrW(&H22) + model.child_node + ChrW(&H22) + ")" + ",0,2,8,1,0,null, decodeURIComponent(" + ChrW(&H22) + firstnodeinfo + ChrW(&H22) + "),false,null," + "decodeURIComponent(" + ChrW(&H22) + wrttext + ChrW(&H22) + ")" + ",0,3,8,1,0, " + ChrW(&H22) + "parent-node1" + ChrW(&H22) + ", decodeURIComponent(" + ChrW(&H22) + wrtfirstnodeinfo + ChrW(&H22) + ")" + ")'
                '>With respect To <img src='../../img/icon/button_arrow.svg'></button>")
                html.Append("</div>")
                'html.Append($"</div>
                '                    <a class='fullscreen_link' onclick=SetExpendedPopupElement(decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.first_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}),'null','2','0','1','null','left-node',decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.child_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}),null,null,decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.wrt_first_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}),'null','3','0','1','null','wrt-left-node',decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.parent_node_info) + ChrW(&H22)}),decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.parent_node.ToString()) + ChrW(&H22)})) href='#' data-bs-toggle='modal' data-bs-target='#exampleModal'><img src='../../img/icon/full-screen-svgrepo-com.svg'></a>
                '                </div>                           
                '            </div>                
                '        </div>")
                html.Append($"</div>
                                <a class='fullscreen_link' onclick=SetExpendedPopupElement(decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.first_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}),'null','2','0','1','null','left-node',decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.child_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}),null,null,decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.wrt_first_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}),'null','3','0','1','null','wrt-left-node',decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.child_node.ToString()) + ChrW(&H22)}),decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.parent_node.ToString()) + ChrW(&H22)})) href='#' data-bs-toggle='modal' data-bs-target='#exampleModal'><img src='../../img/icon/full-screen-svgrepo-com.svg'></a>
                            </div>                           
                        </div>                
                    </div>")
                html.Append("</div>")

                html.Append("</div>")
                html.Append("</div>")
                'left item dropdown end

            'right side radio button item wise
            html.Append("<div class='col-lg-6 d-lg-block d-none ' id='divRadioButtons'><div class='innerRadioButtons'>")
            html.Append("<div class='divtable_title'>")
            html.Append($"<p title='{model.child_node}'>{model.child_node}</p>")
            html.Append("</div>")
            'Dim item = From x In model.multi_non_pw_data Where x.RatingID = -1
            Dim item = model.multi_non_pw_data.FirstOrDefault()

                'html.Append("<div class='divtable_title'>")
                'html.Append($"<p> {item.Title}</p>")
                'html.Append("</div>")
                html.Append("<div class='divtable_header'>")
                html.Append("<div class='product_name'>")
                html.Append("<p>Intensity Name</p>")
                html.Append("</div>")
                If model.showPriorityAndDirect Then
                    html.Append("<div class='product_intensity'>")
                    html.Append("<p>Priority</p>")
                    html.Append("</div>")
                End If
                html.Append("</div>")
                'If model.intensities IsNot Nothing Then
                '    For j = 0 To model.intensities.Count - 1
                '        If model.showPriorityAndDirect Or model.intensities(j)(1) <> "Direct Value" Then
                '            html.Append("<div class='form-check radio_progress customvalue'>")
                '            html.Append($"<input class='form-check-input intensity{j}' onchange=SetValuesFromRadio('0','{j}') onclick=SetValuesFromRadio('0','{j}') type='radio' name='{item.sGUID}' id='{item.Ratings(j).GuidID}' value='{item.Ratings(j).Value}' checked>")
                '            html.Append($"<label class='form-check-label' for='{item.Ratings(j).GuidID}'>")
                '            If (item.Ratings(j).Comment = "") Then
                '                html.Append($"<div class='radio_btnname'>{item.Ratings(j).Name}<span class='idesc intensitydesc{j}'></span></div>")

                '            Else
                '                html.Append($"<div class='radio_btnname'>{item.Ratings(j).Name}<span title='{item.Ratings(j).Comment}' class='idesc intensitydesc{j}'>{" - " + item.Ratings(j).Comment}<span></div>")
                '            End If

                '            'html.Append("</div>")
                '            html.Append("</label>")
                '            html.Append("<div class='tooltips_group w-auto'>")
                '            If (j = model.multi_non_pw_data.Count - 1) Then
                '                html.Append("<div class='info_tooltip_main position-relative scndLastColumn'>")
                '            Else
                '                html.Append("<div class='info_tooltip_main position-relative'>")
                '            End If

                '            html.Append("<div class='comment_div'>")
                '            If j <> item.Ratings.Count - 1 Then
                '                html.Append($"<i class='bx bxs-edit-alt chkEvaluator d-none' onclick='showNode(6{j})'></i>")
                '            End If
                '            html.Append($"<div class='tooltip_wrapper hideshowinfo parentnodemr' id='parentnode6{j}'  style='display: none;'>")
                '            html.Append("<div class='d-flex justify-content-between tooltip-header'>")
                '            html.Append("<span>Edit intensity description</span>")
                '            html.Append("</div>")
                '            html.Append($"<textarea class='form-control mb-3 mt-2 w-100'id='txtleftComment_{j}' rows='3'>{item.Ratings(j).Comment}</textarea>")
                '            html.Append("<div class='comt_btn text-end'>")
                '            html.Append($"<button type='button' onclick=updateRightSideComment('{j}','showcomment','close',event)><i class='fa fa-check' aria-hidden='true'></i> OK</button>")

                '            html.Append("</div>")
                '            html.Append("</div>")
                '            html.Append("</div>")
                '            html.Append("</div>")
                '            html.Append("</div>")
                '            If model.showPriorityAndDirect Then

                '                If j <> item.Ratings.Count - 1 Then
                '                    html.Append("<div class='radio_btn_progress'>")
                '                    html.Append("<div class='progress'>")
                '                    html.Append($"<div class='progress-bar' role='progressbar' style='width: {item.Ratings(j).Value * 100}%' aria-valuenow='{item.Ratings(j).Value * 100}' aria-valuemin='0' aria-valuemax='100'></div>")
                '                    html.Append("</div>")
                '                    Dim active_multi_index = model.multi_non_pw_data.Where(Function(x) x.RatingID = -1).Select(Function(y) y.ID).FirstOrDefault().ToString()
                '                    If j <> item.Ratings.Count - 1 Then
                '                        If item.Ratings(j).Name.ToLower() = "direct value" Or item.Ratings(j).Name.ToLower() = "not rated" Then
                '                            html.Append($"")
                '                        Else
                '                            html.Append($"{Math.Round(item.Ratings(j).Value * 100, 2)}%")
                '                        End If
                '                    ElseIf j <> item.Ratings.Count - 2 Then
                '                        html.Append($"")
                '                    End If
                '                Else
                '                    html.Append("<div class='radio_btn_progress btnprogress'>")
                '                    html.Append($"<input type='text' onkeypress='return isNumberKeyWithDecimal(this, event);' class='custom-value' id='input_2_0' onkeyup=setValuesFromText('0','2')>")
                '                    html.Append($"<i class='fa fa-times custom-value-closs' aria-hidden='true' onclick=ResetValue('{item.sGUID}','0',event)></i>")
                '                End If
                '                html.Append("</div>")
                '            End If

                '            html.Append("</div>")
                '        End If
                '    Next
                'End If

                If model.intensities IsNot Nothing Then
                    html.Append("<div class='form-check radio_progress side_comment'>")
                    For j = 0 To model.intensities.Count - 1
                        If model.showPriorityAndDirect Or model.intensities(j)(1) <> "Direct Value" Then
                            html.Append("<div class='form-check radio_progress customvalue'>")
                            html.Append($"<input{If(model.isPipeViewOnly, " style='pointer-events:none;'", "")} class='form-check-input intensity{j}' onchange='save_ratings({model.intensities(j)(2)})' onclick='save_ratings({model.intensities(j)(2)})' type='radio' name='intensity_{model.intensities(j)(2)}' id='intensity_{model.intensities(j)(2)}' value='{model.intensities(j)(0)}'>")
                            html.Append($"<label{If(model.isPipeViewOnly, " style='pointer-events:none;'", "")} class='form-check-label'>") ' for='intensity_{model.intensities(j)(2)}'
                            If (model.intensities(j)(4) = "") Then
                                html.Append($"<div class='radio_btnname'><div onclick='save_ratings({model.intensities(j)(2)})'>{model.intensities(j)(1)}<span class='idesc intensitydesc{j}'></span></div><a href='javascript:void(0);' class='ms-2 prm-color' onclick='showNode(6{j},{model.intensities(j)(2)});return false'> <img class='chkEvaluatorone' src='../../img/icon/edit-icon.svg'></a>")
                            Else
                                html.Append($"<div class='radio_btnname'><div onclick='save_ratings({model.intensities(j)(2)})'>{model.intensities(j)(1)}<span title='{model.intensities(j)(4)}' class='idesc intensitydesc{j}'>{" - " + model.intensities(j)(4)}</span></div>
                            <a href='javascript:void(0);' class='ms-2 prm-color' onclick='showNode(6{j},{model.intensities(j)(2)} );return false'> <img class='chkEvaluatorone' src='../../img/icon/edit-icon.svg'></a>")
                            End If
                            'If (j = model.multi_non_pw_data.Count - 1) Then
                            '    html.Append("<div class='info_tooltip_main position-relative scndLastColumn'>")
                            'Else
                            '    html.Append("<div class='info_tooltip_main position-relative'>")
                            'End If

                            'html.Append("<div class='info_tooltip_main comment_custom'>")
                            html.Append("<div class='comment_div'>")
                            'If j <> model.intensities.Count - 1 Then
                            '    html.Append($"<i class='bx bxs-edit-alt chkEvaluator d-none' onclick='showNode(6{j})'></i>")
                            'End If
                            html.Append($"<div class='info_tooltip right_tooltip hideshowinfo' id='parentnode6{j}'  style='display: none;'>")
                            html.Append("<div class='d-flex justify-content-between'>")
                            html.Append($"<span>Edit intensity description</span><div class='action_icons'><a href='javascript:void(0);' onclick='hideNode(6{j})'><i class='fa fa-times' aria-hidden='true'></i></a></div>")
                            html.Append("</div>")
                            html.Append($"<textarea{If(model.isPipeViewOnly, " style='pointer-events:none;'", "")} class='form-control mb-3 mt-2 w-100'id='txtleftComment_{j}' rows='2'>{model.intensities(j)(4)}</textarea>")
                            html.Append("<div class='comt_btn text-end'>")
                            html.Append($"<button type='button' onclick=updateRightSideComment('{j}','Ratingshowcomment','close',event)><i class='fa fa-check' aria-hidden='true'></i> OK</button>")

                            'html.Append("</div>")
                            html.Append("</div>")
                            html.Append("</div>")
                            html.Append("</div>")
                            html.Append("</div>")
                            If model.showPriorityAndDirect Then

                                If j <> model.intensities.Count - 1 Then
                                    html.Append($"<div class='radio_btn_progress'{If(model.isPipeViewOnly, " style='pointer-events:none;'", "")}>")
                                    html.Append("<div class='progress'>")
                                    html.Append($"<div class='progress-bar' role='progressbar7' onclick='save_ratings({model.intensities(j)(2)})' style='width: {Convert.ToDouble(model.intensities(j)(0)) * 100}%' aria-valuenow='{Convert.ToDouble(model.intensities(j)(0)) * 100}' aria-valuemin='0' aria-valuemax='100'></div>")
                                    html.Append("</div>")
                                    Dim active_multi_index = model.multi_non_pw_data.Where(Function(x) x.RatingID = -1).Select(Function(y) y.ID).FirstOrDefault().ToString()
                                    If j <> model.intensities.Count - 1 Then
                                        If model.intensities(j)(1).ToLower() = "direct value" Or model.intensities(j)(1).ToLower() = "not rated" Then
                                            html.Append($"")
                                        Else
                                            html.Append($"{Math.Round(Convert.ToDouble(model.intensities(j)(0)) * 100, 2)}%")
                                        End If
                                    ElseIf j <> model.intensities.Count - 2 Then
                                        html.Append($"")
                                    End If
                                Else
                                    html.Append($"<div class='radio_btn_progress directvalue_btn_progress  btnprogress'{If(model.isPipeViewOnly, " style='pointer-events:none;'", "")}>")
                                    html.Append($"<input type='text' onkeypress='return isNumberKeyWithDecimal(this, event);' class='custom-value' id='InputSingleRatingDirect' >")
                                    html.Append($"<div class='close_icon'><img src='../../img/icon/erasar.svg' onclick='ResetRatingValue()' style='cursor: pointer;' class='imgClose'  /></div>")
                                End If
                                html.Append("</div>")
                            End If
                            html.Append("</label>")

                            'html.Append("<div class='tooltips_group w-auto'>")
                            'If (j = model.multi_non_pw_data.Count - 1) Then
                            '    html.Append("<div class='info_tooltip_main position-relative scndLastColumn'>")
                            'Else
                            '    html.Append("<div class='info_tooltip_main position-relative'>")
                            'End If

                            'html.Append("<div class='comment_div'>")
                            'If j <> model.intensities.Count - 1 Then
                            '    html.Append($"<i class='bx bxs-edit-alt chkEvaluator d-none' onclick='showNode(6{j})'></i>")
                            'End If
                            'html.Append($"<div class='hideshowinfo info_tooltip parentnodemr right_tooltip tooltip_wrapper' id='parentnode6{j}'  style='display: none;'>")
                            'html.Append("<div class='d-flex justify-content-between'>")
                            'html.Append("<span>Edit intensity description</span>")
                            'html.Append("</div>")
                            'html.Append($"<textarea{If(model.isPipeViewOnly, " style='pointer-events:none;'", "")} class='form-control mb-3 mt-2 w-100'id='txtleftComment_{j}' rows='3'>{model.intensities(j)(4)}</textarea>")
                            'html.Append("<div class='comt_btn text-end'>")
                            'html.Append($"<button type='button' onclick=updateRightSideComment('{j}','Ratingshowcomment','close',event)><i class='fa fa-check' aria-hidden='true'></i> OK</button>")

                            'html.Append("</div>")
                            'html.Append("</div>")
                            'html.Append("</div>")
                            'html.Append("</div>")
                            'html.Append("</div>")


                            html.Append("</div>")
                        End If
                    Next
                    html.Append("</div>")
                End If

                'If model.showinfodocnode Then
                '    Dim model_text As String = If(model.ScaleDescriptions.Count > 0, HttpUtility.UrlEncode(model.ScaleDescriptions(0).Name.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))), "")
                '    Dim model_text_info As String = If(model.ScaleDescriptions.Count > 0, HttpUtility.UrlEncode(model.ScaleDescriptions(0).Description.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))), "")
                '    html.Append("<div class='bottom_info'>")
                '    html.Append("<div class='info_tooltip_main position-relative'>")
                '    html.Append($"<i class='fa fa-info-circle infocolorheader {If(model_text_info <> "" And model_text_info <> "NaN", "", " infoColor chkEvaluator")}' aria-hidden='true' onclick=updateRightSideComment('0','ScaleDescriptions','open',event)></i> <span class='{If(model_text_info <> "" And model_text_info <> "NaN", "", "chkEvaluator")}'>{If(model.ScaleDescriptions.Count > 0, model.ScaleDescriptions(0).Name, "")}</span>")
                '    html.Append("<div class='tooltip_wrapper hideshowinfo tooltip_show' id='ScaleDescriptions' style='display: none;'>")
                '    html.Append("<div class='tooltip-header'>")
                '    html.Append($"<span>{model.parent_node}</span>")
                '    html.Append("<div class='action_icons'>")

                '    html.Append($"<a href='javascript:void(0);'><i class='bx bxs-edit-alt chkEvaluator d-none' onclick='showInfoPopup(decodeURIComponent({ChrW(&H22) + model_text + ChrW(&H22)}),4,4,0,1,null,decodeURIComponent({ChrW(&H22) + "scale-node" + ChrW(&H22)}),decodeURIComponent({ChrW(&H22) + model_text_info + ChrW(&H22)}), decodeURIComponent({ChrW(&H22) + "scale" + ChrW(&H22)}))'></i></a>")
                '    'html.Append($"<a href='javascript:void(0);' class='edit_icon chkEvaluator' onclick=showInfoPopup(decodeURIComponent({ChrW(&H22) + model_text + ChrW(&H22)}),'4','4','0','1',null,'scale-node',decodeURIComponent({ChrW(&H22) + model_text_info + ChrW(&H22)}))>")
                '    html.Append("<a href='javascript:void(0);'><i class='fa fa-times' aria-hidden='true' onclick=updateRightSideComment('0','ScaleDescriptions','close',event)></i></a>")
                '    html.Append("</div>")
                '    html.Append("</div>")
                '    html.Append($"<p class='border-top font-size-12 pt-2 text-start mb-0'>{If(model.ScaleDescriptions.Count > 0, model.ScaleDescriptions(0).Description, "")}</p>")
                '    html.Append("</div>")
                '    html.Append("</div>")
                '    html.Append("</div>")
                'End If
                '=============================


                html.Append("</div>")


                divContent.InnerHtml = html.ToString()
            End If
    End Sub

    Public Function GetWrtText(nodeType As String, childNode As String, parentNode As String) As String
        Dim leftText = ""
        Dim rightText = ""
        Dim wrtText = "WRT"
        childNode = If(childNode IsNot Nothing, childNode, "")
        parentNode = If(parentNode IsNot Nothing, parentNode, "")

        If childNode IsNot Nothing AndAlso childNode.Length > 14 And wrtText.Length > 2 Then
            leftText = childNode.Substring(0, 14) + "..."
        Else
            leftText = childNode
        End If
        If parentNode IsNot Nothing AndAlso parentNode.Length > 14 And wrtText.Length > 2 Then
            rightText = parentNode.Substring(0, 14) + "..."
        Else
            rightText = parentNode
        End If
        Return (leftText + " " + wrtText.Trim() + " " + rightText).Trim()
    End Function
    'Public Function BindReHtml() As String
    '    Dim model As AnytimeOutputModel = New AnytimeOutputModel()
    '    bindHtml(model)
    '    Return ""
    'End Function
End Class