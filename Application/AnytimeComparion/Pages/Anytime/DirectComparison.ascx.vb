Imports System.Web.Services

Public Class DirectComparison
    Inherits System.Web.UI.UserControl

    Dim screenCheck As ScreenCheck = New ScreenCheck()

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    End Sub

    Public Sub bindHtml(ByVal model As AnytimeOutputModel)
        If model IsNot Nothing Then
            Dim html As StringBuilder = New StringBuilder()
            'Dim parentnodeinfo As String = HttpUtility.UrlEncode(If(model.parent_node_info <> "" AndAlso model.parent_node_info.Trim() <> "", model.parent_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)), ""))
            Dim firstnodeinfo As String = HttpUtility.UrlEncode(model.first_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
            Dim parentnodeinfo As String = HttpUtility.UrlEncode(model.parent_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
            Dim wrtfirstnodeinfo As String = HttpUtility.UrlEncode(model.wrt_first_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
            Dim wrttext As String = GetWrtText("Left", model.child_node.ToString(), model.parent_node.ToString())
            Dim stepTask As String = HttpUtility.UrlEncode(model.step_task.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
            Dim model_text As String = HttpUtility.UrlEncode(model.cluster_phrase.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
            Dim model_text_info As String = HttpUtility.UrlEncode(model.parent_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))

            'div 1
            html.Append("<div class='page_heading_section'>")
            'div container
            html.Append("<div class='container'>")
            'Header title div
            html.Append("<div class='row'>")
            'co-6 div
            html.Append("<div class='col-md-6'>")
            'heading div
            html.Append($"<div class='heading_content'><div class='page-title-box' id='titleDiv'><div class='heading_icons'>")

            'If Not ScreenCheck.isMobileBrowserClient(Request) Then
            html.Append($"<a {If(model.isPipeViewOnly, " class='d-none'", " class='editsvg_icon'")}  href='javascript:void(0);' onclick=showHeaderPopup(decodeURIComponent({ChrW(&H22) + model_text + ChrW(&H22)}),'0','0','0','0',null,'question-node',decodeURIComponent({ChrW(&H22) + model_text + ChrW(&H22)}))><img class='chkEvaluatorone' src='../../img/icon/edit-icon.svg'></a>")
            'End If
            html.Append("<asp:Label ID='lblTask' runat='server' ><span id='divT2S' style='font-size:20px; margin-left:1ex border: 0px !important; background: #f0f0ff08 !important; display:none'></span></asp:Label></div>")
            html.Append($"<div Class='txtStepTask removep' id='MainHeaderInfodoc'>{model.step_task}<span class='threedoticon d-none' id='btnheadinfo'> <a href='#' onclick='showheaderpopup()' ><div class='snippet'><div class='stage'><div class='dot-falling'></div> </div></div></a></span></div>")

            ' html.Append("<asp:Label ID='lblTask' runat='server' ><span id='divT2S' style='font-size:20px; margin-left:1ex border: 0px !important; background: #f0f0ff08 !important; display:none'></span></asp:Label>")
            'html.Append("<div class='heading_info'><img src='../../img/icon/empty_info.svg' class='d-none'><img src='../../img/icon/info_data.svg'><img src='../../img/icon/info-close.svg' class='d-none'></div>")

            Dim IsHideHeaderInfo As Boolean = False
            'If model.parent_node_info Is Nothing Or model.parent_node_info = "" Then
            '    html.Append($"<div class='heading_info' id='HeaderDivIcon'><a href='#'><img  id='img1' src='../../img/icon/empty_info.svg'><img id='img2' src='../../img/icon/info_data.svg' style='display: none;'><img id='img3' src='../../img/icon/info-close.svg' style='display: none;'></a></div>")
            '    IsHideHeaderInfo = True
            'Else
            '    html.Append($"<div class='heading_info' id='HeaderDivIcon'><a href='#'><img  id='img1' src='../../img/icon/empty_info.svg' style='display: none;'><img id='img2' src='../../img/icon/info_data.svg'><img id='img3' src='../../img/icon/info-close.svg' style='display: none;'></a></div>")
            'End If

            html.Append("</div>")
            html.Append("</div>")
            'col-6 div end
            html.Append("</div>")
            Dim NoInfoDocDataCls As String = ""
            If model.parent_node_info.ToString().Trim() = "" Then
                NoInfoDocDataCls = "NoInfoData"
            End If
            'co-6 div
            html.Append($"<div class='col-md-6 {NoInfoDocDataCls}'>")
            If model.is_infodoc_tooltip Or model.showinfodocnode Then
                'html.Append($"<p'>{model.parent_node_info}</p><p></p></p>")
                html.Append($"<div class='info_content d-flex mt-lg-0 mt-2' id='Header_InfoDocs' {If(IsHideHeaderInfo, " style='display: none;'", "")}><div class='heading_icons heading_info me-0'><div  id='HeaderDivIcon'><a href='#'><img  id='img1' src='../../img/icon/empty_info.svg' style='display: none;'><img id='img2' src='../../img/icon/info_data.svg'><img id='img3' src='../../img/icon/info-close.svg' style='display: none;'></a></div><div class='infodoc_icons'><a class='editsvg_icon' href='javascript:void(0);' onclick=showInfoPopup(decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.parent_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}),'0','-1','0','-1',null,'parent-node',decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.parent_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}))><img class='chkEvaluatorone' src='../../img/icon/edit-icon.svg'></a><a href='#' onclick=Expandtxt(decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.parent_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}),'0','-1','0','-1',null,'parent-node',decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.parent_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)})) data-bs-toggle='modal' data-bs-target='#infodocPop'><img src='../../img/icon/full-screen-svgrepo-com.svg' id='ibtnFullscreen'></a></div></div><div class='info_content_wrapper'><div class='info_text'>{model.parent_node_info}</div><div class='info_box_icons d-none'>")
                html.Append($"<a Class='editsvg_icon' href='javascript:void(0);' onclick=showInfoPopup(decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.parent_node) + ChrW(&H22)}),'0','-1','0','-1',null,'parent-node',decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.parent_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}))><img class='chkEvaluatorone' src='../../img/icon/edit-icon.svg'></a>")
                'If Not (ScreenCheck.isMobileBrowserClient(Request)) Then
                html.Append($"<a href='javascript:void(0);' data-bs-toggle='modal' data-bs-target='#infodocPop'><img src='../../img/icon/full-screen-svgrepo-com.svg' onclick=Expandtxt(decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.parent_node) + ChrW(&H22)}),'0','-1','0','-1',null,'parent-node',decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.parent_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}))></a>")
                'End If
                html.Append("</div></div></div>")
            End If
            html.Append("</div>")
            html.Append("</div>")
            html.Append("</div>")
            'container end
            html.Append("</div>")
            Dim NoInfDocDataRow As String = ""
            NoInfoDocDataCls = ""
            Dim noinfdocdataoneside As String = ""
            If model.first_node_info.ToString().Trim() = "" And model.wrt_first_node_info.ToString().Trim() = "" Then
                NoInfDocDataRow = "NoInfDocDataRow"
                NoInfoDocDataCls = "NoInfoData"
                noinfdocdataoneside = "nodataonesideinfodocrow"
            Else
                If model.first_node_info.ToString().Trim() = "" Or model.wrt_first_node_info.ToString().Trim() = "" Then
                    noinfdocdataoneside = "nodataonesideinfodocrow"
                End If
            End If
            html.Append("<div class='container'>")
            'Header title div
            html.Append("<div class='row'>")
            html.Append("<div class='col-md-12'><div class='progress_table_row progress_main_container'>")
            html.Append("<div class='row_wrapper active_table_row direct_entry_comment'>")
            html.Append("<div class='tooltips_group'>")
            html.Append($"<div class='info_tooltip_main position-relative {NoInfoDocDataCls }'>")
            'first node
            model_text = HttpUtility.UrlEncode(model.first_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27) + model.child_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))))
            model_text_info = HttpUtility.UrlEncode(model.first_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
            'model_text_info = HttpUtility.UrlEncode(model.wrt_first_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
            Dim wrt_header_text As String = HttpUtility.UrlEncode(model.parent_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
            If model.first_node_info <> "" Or model.wrt_first_node_info <> "" Then
                html.Append($"<span class='spnInfo'><a href ='javascript:void(0);'><img class='{If((model_text_info) <> "" And (model_text_info) <> "NaN", "", "infoColor")} {If((model_text_info) <> "" And (model_text_info) <> "NaN", "", "chkEvaluator")}' aria-hidden='true' src='../../img/icon/info-close.svg' /></a></span>")
            Else
                'html.Append($"<span class='spnInfo empty_spnInfo'><a href ='javascript:void(0);'><img class='{If((model_text_info) <> "" And (model_text_info) <> "NaN", "", "infoColor")} {If((model_text_info) <> "" And (model_text_info) <> "NaN", "", "chkEvaluator")}' data-bs-toggle='modal' data-bs-target='#exampleModal' aria-hidden='true' src='../../img/icon/empty_info.svg' onclick='SetExpendedPopupElement(decodeURIComponent(" + ChrW(&H22) + model_text_info + ChrW(&H22) + "),null,2,0,1,null,decodeURIComponent(" + ChrW(&H22) + "left-node" + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + HttpUtility.UrlEncode(model.first_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)) + model.child_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22) + "),null,null,decodeURIComponent(" + ChrW(&H22) + HttpUtility.UrlEncode(model.wrt_first_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22) + "),null,3,0,1,null,decodeURIComponent(" + ChrW(&H22) + "wrt-left-node1" + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + HttpUtility.UrlEncode(model.first_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)) + model.child_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + wrt_header_text + ChrW(&H22) + "))' class='fullscreen_link' /></a></span>")
                html.Append($"<span class='spnInfo empty_spnInfo filter_gray'><a href ='javascript:void(0);'><img class='{If((model_text_info) <> "" And (model_text_info) <> "NaN", "", "infoColor")} {If((model_text_info) <> "" And (model_text_info) <> "NaN", "", "chkEvaluator")}' aria-hidden='true' src='../../img/icon/info-close.svg' style='grayscale(1)' /></a></span>")
            End If

            html.Append("<div class='tooltip_wrapper hideshowinfo' id='tooltip_wrapper' style='display: none;'>")
            html.Append("<div class='tooltip-header'>")
            html.Append($"<span>{model.first_node}{model.child_node}</span>")

            html.Append($"<div class='action_icons'>")

            html.Append($"<a{If(model.isPipeViewOnly, " class='d-none'", "")} href='javascript:void(0);'><i class='bx bxs-edit-alt chkEvaluator d-none'></i></a>")
            html.Append($"<a href='javascript:void(0);'><i class='fa fa-times' aria-hidden='true'></i></a>")
            html.Append("</div>")
            html.Append("</div>")
            html.Append($"<p class='border-top font-size-12 pt-2 text-start mb-0'>{model.first_node_info}</p>")
            html.Append("</div>")
            html.Append("</div>")
            html.Append("<div class='info_tooltip_main position-relative'>")
            html.Append("<div class='comment_div'>")
            'comment section
            If model.show_comments Then
                If model.comment Is Nothing Or model.comment = "" Then
                    If Not (screenCheck.isMobileBrowserClient(Request)) Then
                        html.Append($"<a href='javascript:void(0)' onclick='showNode(16)'><img src = '../../img/icon/Grey-plus-icon.svg' id='ImgComment_iconEmpty_16' title='' aria-hidden='true'> <img src = '../../img/icon/comment-svgrepo-com.svg' style='display:none;' id='ImgComment_icon_16' title='' aria-hidden='true'></a>")
                    Else
                        html.Append($"<a href='javascript:void(0)' onclick='showNodeMobile(16)'><img src = '../../img/icon/Grey-plus-icon.svg' id='ImgComment_iconEmpty_16' title='' aria-hidden='true'> <img src = '../../img/icon/comment-svgrepo-com.svg' style='display:none;' id='ImgComment_icon_16' title='' aria-hidden='true'></a>")
                    End If
                    'html.Append($"<a href='javascript:void(0)' onclick='showNode(16)'><img class='emptyComment' id='single_comment_icon' aria-hidden='true' src='../../img/icon/comment.svg' /></a>")
                Else
                    If Not (screenCheck.isMobileBrowserClient(Request)) Then
                        html.Append($"<a href='javascript:void(0)' onclick='showNode(16)'><img src = '../../img/icon/Grey-plus-icon.svg' id='ImgComment_iconEmpty_16' title=''  style='display:none;'  aria-hidden='true'>  <img src = '../../img/icon/comment-svgrepo-com.svg' id='ImgComment_icon_16' title='{model.comment}' aria-hidden='true'></a>")
                    Else
                        html.Append($"<a href='javascript:void(0)' onclick='showNodeMobile(16)'><img src = '../../img/icon/Grey-plus-icon.svg' id='ImgComment_iconEmpty_16' title=''  style='display:none;'  aria-hidden='true'>  <img src = '../../img/icon/comment-svgrepo-com.svg' id='ImgComment_icon_16' title='{model.comment}' aria-hidden='true'></a>")
                    End If
                    'html.Append($"<a href='javascript:void(0)' onclick='showNode(16)'><img class='fa fa-comments filledComment' id='single_comment_icon' aria-hidden='true' src='../../img/icon/comment.svg' /></a>")
                End If
                    html.Append("<div class='info_tooltip right_tooltip'  id='parentnode16' style='display:none;'>")
                    html.Append("<div class='d-flex justify-content-between'>")
                    html.Append("<span>add your comment</span>")
                    html.Append($"<div class='action_icons' ><a href='javascript:void(0);' onclick='hideNode(16)'><i class='fa fa-times' onclick='hideNode(16)' aria-hidden='true'></i></a></div>")
                    html.Append("</div>")
                    html.Append($"<textarea{If(model.isPipeViewOnly, " style='pointer-events:none;'", "")} class='form-control mb-3 mt-2 w-100' rows='3' id='single_comment' placeholder='Add your comment'>{model.comment}</textarea>")
                    html.Append("<div class='comt_btn text-end'>")
                    html.Append($"<button{If(model.isPipeViewOnly, " class='d-none'", "")} type='button' onclick='SaveDirectComment(16)'><i class='fa fa-check' aria-hidden='true'></i> ok</button>")
                    html.Append("</div>")
                    html.Append("</div>")
                    'html.Append("</div>")
                End If
                html.Append("</div>")
                html.Append("</div>")
                html.Append("<div class='brand_name'>")
                html.Append($"<span title='{model.first_node}{model.child_node}'>{model.first_node}{model.child_node}</span>")
                html.Append("</div>")
                html.Append("</div>")

                html.Append("<div class='drop_progress'>")
                html.Append("<div class='info_tooltip_main position-relative wrtFix'>")

                html.Append($"<div class='brand_value'{If(model.isPipeViewOnly, " style='pointer-events:none;'", "")}>")

                Dim fun As String = $"onkeyup=SetsingleDirectSlider()"
                fun += $" onkeydown=SetsingleDirectSlider()"
                fun += $" onmousedown=SetsingleDirectSlider()"
                fun += $" onmouseup=SetsingleDirectSlider()"
                fun += $" onblur=SetsingleDirectSlider()"
                fun += " onkeypress='return isNumberKeyWithDecimal(this, event);'"
                html.Append($"<input type='text'  {fun} class='value_control directinput' title='Enter number 0 to 1' min='0' max='1' id='at_direct_input'>")
                html.Append("</div>")

                'End If
                html.Append("</div>")
                html.Append($"<div class='progres_dropdown mt-2'{If(model.isPipeViewOnly, " style='pointer-events:none;'", "")}>")
                html.Append("<div class='form-group'>")
                html.Append($"<div id='at_direct_slider' data-myval='' class='singleDirectSlider'></div>")
                html.Append("</div>")
                html.Append("</div>")

                html.Append($"<div class='close_icon mt-1' id='close_icon' onclick=ResetSingleDirectSlider(){If(model.isPipeViewOnly, " style='pointer-events:none;'", "")}>")
                html.Append("<img src='../../img/icon/erasar.svg' style='cursor: pointer;' class='imgClose' >")
                html.Append("</div>")

                html.Append("</div>")
                html.Append($"<div class='open_tooltip_info {NoInfoDocDataCls} {NoInfDocDataRow}'><hr><div class='body_content'><div class='split_content {noinfdocdataoneside}'>")
                If (model.first_node_info <> "") Then
                    html.Append($"<div class='normal_content'>")
                    html.Append($"<p>{model.first_node_info} </p>")
                Else
                    html.Append($"<div class='normal_content NoInfoData'>")
                    html.Append("<h2 class='nodata_level'>No Data</h2>")
                End If
                html.Append("</div>")
                If (model.wrt_first_node_info <> "") Then
                    html.Append($"<div class='wrt_right_content'><h3>WITH RESPECT TO <span>{model.parent_node}</span></h3>")
                    html.Append($"<p>{model.wrt_first_node_info} </p>")
                Else
                    html.Append($"<div class='wrt_right_content NoInfoData'><h3>WITH RESPECT TO <span>{model.parent_node}</span></h3>")
                    html.Append("<h2 class='nodata_level'>No Data</h2>")
                End If
                html.Append($"</div>")


                html.Append($"<a href='#' data-bs-toggle='modal' data-bs-target='#exampleModal' onclick='SetExpendedPopupElement(decodeURIComponent(" + ChrW(&H22) + model_text_info + ChrW(&H22) + "),null,2,0,1,null,decodeURIComponent(" + ChrW(&H22) + "left-node" + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + HttpUtility.UrlEncode(model.first_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)) + model.child_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22) + "),null,null,decodeURIComponent(" + ChrW(&H22) + HttpUtility.UrlEncode(model.wrt_first_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22) + "),null,3,0,1,null,decodeURIComponent(" + ChrW(&H22) + "wrt-left-node1" + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + HttpUtility.UrlEncode(model.first_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)) + model.child_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + wrt_header_text + ChrW(&H22) + "))' class='fullscreen_link'><img src='../../img/icon/full-screen-svgrepo-com.svg '></a></div></div></div>")

                'html.Append($"<div class='modal fade' id='exampleModalWRT' tabindex='-1' aria-labelledby='exampleModalLabel' aria-hidden='True'><div class='modal-dialog modal-fullscreen'><div class='modal-content'><div class='modal-header'><h3 class='modal-title'id='exampleModalLabel' style='color:white;'>INFODOCS</h3><button type='button' class='btn-close' data-bs-dismiss='modal' aria-label='Close'></button></div><div class='modal-body'><div class='row h-100'><div class='col-md-6 border-End'><div class='modal_info_header'><h3>{model.first_node}{model.child_node}</h3><a href='javascript:void(0);'><img class='chkEvaluatorone' src='../../img/icon/edit-icon.svg' onclick=showInfoPopup(decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode((model.first_node + model.child_node).ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}),'','2','0','1','null','left-node',decodeURIComponent({ChrW(&H22) + model_text_info + ChrW(&H22)}))></a></div><div class='modal_info_content'><p>{model.first_node_info}</p></div></div><div class='col-md-6'><div class='modal_info_header'><h3>With respect to {model.parent_node}</h3><a href='javascript:void(0);'><img class='chkEvaluatorone' src='../../img/icon/edit-icon.svg'onclick=showInfoPopup(decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode((model.first_node + model.child_node).ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}),'','3','0','1','null','wrt-left-node',decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.wrt_first_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}),decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.parent_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)})) ></a></div><div class='modal_info_content'><p>{model.wrt_first_node_info}</p></div></div></div></div></div></div></div>")
                '$<p>model.multi_non_pw_data(i).InfodocWRT + "</p>"
                'html.Append($"<p>{model.multi_non_pw_data(i).Infodoc.ToString()}</p>")
                'If (model.multi_non_pw_data(i).InfodocWRT <> "") Then
                '    html.Append($"</div></div><div class='info_datafooter d-block '><button type='button' {If((model.multi_non_pw_data(i).InfodocWRT.ToString().Trim()) <> "", " class=''", " class='nowrt_data' ")} data-bs-toggle='modal' data-bs-target='#exampleModalWRT_{i}'>with respect to <img src='../../imgSetExpendedPopupElement/icon/button_arrow.svg'></button>")
                '    html.Append($"<div class='tooltip_wrapper hideshowinfo' id='w_bandage_wrapper_{i}' style='display: none;'>")
                '    html.Append("<div class='tooltip-header'>")
                '    html.Append($"<span>{If(model.multi_non_pw_data(i).Title.Length > 14, model.multi_non_pw_data(i).Title.Substring(0, 15) + "...", model.multi_non_pw_data(i).Title)} WRT {If(model.parent_node.Length > 14, model.parent_node.Substring(0, 15) + "...", model.parent_node)}</span>")
                '    html.Append($"<div class='action_icons'>")
                '    html.Append($"<a{If(model.isPipeViewOnly, " class='d-none'", "")} href='javascript:void(0);'><i class='bx bxs-edit-alt chkEvaluator d-none' onclick=showInfoPopup(decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.multi_non_pw_data(i).Title.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}),'{i + 1}','0','1','{guid}','wrt-left-node',decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.multi_non_pw_data(i).Infodoc.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}))></i></a>")
                '    html.Append($"<a href='javascript:void(0);'><i class='fa fa-times' aria-hidden='true' onclick=updateRightSideComment('{i}','bandage','close',event)></i></a>")
                '    html.Append("</div>")
                '    html.Append("</div>")
                '    html.Append($"<p class='border-top font-size-12 pt-2 text-start mb-0'>{model.multi_non_pw_data(i).InfodocWRT}</p>")
                '    html.Append("</div>")
                '    html.Append("</div>")
                '    html.Append("</div>")
                '    html.Append($"<div class='modal fade' id='exampleModalWRT_{i}' tabindex='-1' aria-labelledby='exampleModalLabel' aria-hidden='True'><div class='modal-dialog modal-fullscreen'><div class='modal-content'><div class='modal-header'><h3 class='modal-title'id='exampleModalLabel' style='color:white;'>INFODOCS</h3><button type='button' class='btn-close' data-bs-dismiss='modal' aria-label='Close'></button></div><div class='modal-body'><div class='row h-100'><div class='col-md-6 border-End'><div class='modal_info_header'><h3>{model.multi_non_pw_data(i).Title}</h3><a href='javascript:void(0);'><img class='chkEvaluatorone' src='../../img/icon/edit-icon.svg' onclick=showInfoPopup(decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.multi_non_pw_data(i).Title.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}),'{i + 1}','2','0','1','{guid}','left-node',decodeURIComponent({ChrW(&H22) + model_text_info + ChrW(&H22)}))></a></div><div class='modal_info_content'><p>{model.multi_non_pw_data(i).Infodoc}</p></div></div><div class='col-md-6'><div class='modal_info_header'><h3>With respect to {model.parent_node}</h3><a href='javascript:void(0);'><img class='chkEvaluatorone' src='../../img/icon/edit-icon.svg'onclick=showInfoPopup(decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.multi_non_pw_data(i).Title.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}),'{i + 1}','3','0','1','{guid}','wrt-left-node',decodeURIComponent({ChrW(&H22) + model_text_info_wrt + ChrW(&H22)}),decodeURIComponent({ChrW(&H22) + parentnode + ChrW(&H22)})) ></a></div><div class='modal_info_content'><p>{model.multi_non_pw_data(i).InfodocWRT}</p></div></div></div></div></div></div></div>")
                'Else
                '    html.Append($"</div></div><div class='info_datafooter d-block '><button type='button' {If((model.multi_non_pw_data(i).InfodocWRT.ToString().Trim()) <> "", " class=''", " class='nowrt_data' ")}' data-bs-toggle='modal' data-bs-target='#exampleModalWRT_{i}'>with respect to <img src='../../img/icon/button_arrow.svg'></button></div></div>")
                '    html.Append($"<div class='modal fade' id='exampleModalWRT_{i}' tabindex='-1' aria-labelledby='exampleModalLabel' aria-hidden='True'><div class='modal-dialog modal-fullscreen'><div class='modal-content'><div class='modal-header'><h3 class='modal-title'id='exampleModalLabel' style='color:white;'>INFODOCS</h3><button type='button' class='btn-close' data-bs-dismiss='modal' aria-label='Close'></button></div><div class='modal-body'><div class='row h-100'><div class='col-md-6 border-End'><div class='modal_info_header'><h3>{model.multi_non_pw_data(i).Title}</h3><a href='javascript:void(0);'><img class='chkEvaluatorone' src='../../img/icon/edit-icon.svg' onclick=showInfoPopup(decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.multi_non_pw_data(i).Title.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}),'{i + 1}','2','0','1','{guid}','left-node',decodeURIComponent({ChrW(&H22) + model_text_info + ChrW(&H22)}))></a></div><div class='modal_info_content'><p>{model.multi_non_pw_data(i).Infodoc}</p></div></div><div class='col-md-6'><div class='modal_info_header'><h3>With respect to {model.parent_node}</h3><a href='javascript:void(0);'><img class='chkEvaluatorone' src='../../img/icon/edit-icon.svg'onclick=showInfoPopup(decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.multi_non_pw_data(i).Title.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}),'{i + 1}','3','0','1','{guid}','wrt-left-node',decodeURIComponent({ChrW(&H22) + model_text_info_wrt + ChrW(&H22)}),decodeURIComponent({ChrW(&H22) + parentnode + ChrW(&H22)})) ></a></div><div class='modal_info_content'><p>{model.multi_non_pw_data(i).InfodocWRT}</p></div></div></div></div></div></div></div>")
                'End If


                ' End If

                html.Append("</div>")

                html.Append("</div>")

                html.Append("</div>")
                html.Append("</div> </div>")

                '       html.Append("<div class='chart_wrapper active_row single_row pairwise_width removeB set_width_pair sliderBackground'> ")

                '       'first node
                '       model_text = HttpUtility.UrlEncode(model.first_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27) + model.child_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))))
                '       model_text_info = HttpUtility.UrlEncode(model.first_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
                '       'model_text_info = HttpUtility.UrlEncode(model.wrt_first_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
                '       'Dim wrt_header_text As String = HttpUtility.UrlEncode(model.parent_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))

                '       If (model.first_node_info.ToString().Trim() = "" And model.wrt_first_node_info.ToString().Trim() = "") Then
                '           html.Append("<div class='value_wrapper left_info empty_info '><div class='info_data selected'>")
                '       Else
                '           html.Append("<div class='value_wrapper left_info '><div class='info_data selected'>")
                '       End If
                '       html.Append($"<div class='info_header'><a><img src='../../img/icon/info-close.svg' class='icon'></a><span class='title_tag text-uppercase'> {model.first_node}{model.child_node}</span>
                '<div class='header_edit_fw'> <a href='#' data-bs-toggle='modal' data-bs-target='#exampleModal' onclick='SetExpendedPopupElement(decodeURIComponent(" + ChrW(&H22) + model_text_info + ChrW(&H22) + "),null,2,0,1,null,decodeURIComponent(" + ChrW(&H22) + "left-node" + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + HttpUtility.UrlEncode(model.first_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)) + model.child_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22) + "),null,null,decodeURIComponent(" + ChrW(&H22) + HttpUtility.UrlEncode(model.wrt_first_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22) + "),null,3,0,1,null,decodeURIComponent(" + ChrW(&H22) + "wrt-left-node1" + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + HttpUtility.UrlEncode(model.first_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)) + model.child_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + wrt_header_text + ChrW(&H22) + "))'><img src='../../img/icon/full-screen-svgrepo-com.svg '></a>  <a href='#' 
                'onclick='showInfoPopup(decodeURIComponent(" + ChrW(&H22) + model_text + ChrW(&H22) + "),null,2,0,1,null,decodeURIComponent(" + ChrW(&H22) + "left-node" + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + model_text_info + ChrW(&H22) + "))' class='d-md-block d-none ms-2'><img src='../../img/icon/edit-icon.svg' class='icon'></a> </div> </div>")

                '       'If (model.first_node_info.ToString().Trim() <> "" Or model.wrt_first_node_info.ToString().Trim() <> "") Then
                '       html.Append("<div Class='body_content removep3'>")
                '       If (model.first_node_info.ToString().Trim() <> "") Then
                '           html.Append($"<span> {model.first_node_info.ToString()} </span>")
                '       Else
                '           html.Append("<div class=''><h2>No Data</h2></div>")
                '           End If
                '           html.Append($"</div> <div class='wrt_content'><div class='body_content'><h2>WITH RESPECT TO <span>{model.parent_node}</span></h2>")
                '           If (model.wrt_first_node_info.ToString().Trim() <> "") Then
                '           html.Append($"<span>{model.wrt_first_node_info}</span>")
                '       Else
                '               html.Append($"<div class=''><h3>No Data</h3> </div>")
                '           End If
                '           html.Append(" </div></div>")
                '           'End If


                '           'html.Append("<div class='value_wrapper left_info'>")
                '           'html.Append("<div Class='info_data selected'>")

                '           'html.Append("<div Class='info_header'>")

                '           'html.Append($"<span Class='title_tag text-uppercase'>{model.first_node}</span><a href ='javascript:void(0);' Class='d-md-block d-none'><img class='chkEvaluatorone' src='../../img/icon/edit-icon.svg' onclick='showInfoPopup(decodeURIComponent(" + ChrW(&H22) + model_text + ChrW(&H22) + "),null,2,0,1,null,decodeURIComponent(" + ChrW(&H22) + "left-node" + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + model_text_info + ChrW(&H22) + "))'></a></div>")


                '           'html.Append("<div Class='body_content removep3'>")
                '           'If model.first_node_info.ToString().Trim().Length < 120 Then
                '           '    html.Append($"<span>{model.first_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))}</span>")
                '           'Else
                '           '    html.Append($"<span>{model.first_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)).Substring(0, 120)} ...</span>")
                '           'End If
                '           ''html.Append($"<span>{model.first_node_info}</span>")
                '           'Dim wrt As String = model.first_node.ToString()
                '           'If (model.first_node_info.ToString().Trim() = "") Then
                '           '    html.Append("<div Class='no-data_content'>")
                '           '    html.Append("<h2> No Data</h2>")

                '           '    html.Append($"<a href='javascript:void(0);' class='chkEvaluatorone' data-bs-toggle='modal' data-bs-target='#exampleModal' onclick='SetExpendedPopupElement(decodeURIComponent(" + ChrW(&H22) + model_text_info + ChrW(&H22) + "),null,2,0,1,null,decodeURIComponent(" + ChrW(&H22) + "left-node" + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + model_text + ChrW(&H22) + "),null,null,decodeURIComponent(" + ChrW(&H22) + HttpUtility.UrlEncode(model.wrt_first_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22) + "),null,3,0,1,null,decodeURIComponent(" + ChrW(&H22) + "wrt-left-node1" + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + HttpUtility.UrlEncode(model.first_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + wrt_header_text + ChrW(&H22) + "))'> +<u> Add Data</u></a></div>")
                '           '    html.Append("</div>")
                '           'Else
                '           '    html.Append("</div>")
                '           'End If
                '           'html.Append("</div>")
                '           'html.Append("<div Class='info_datafooter'>")

                '           'html.Append($"<Button type='button' {If((model.wrt_first_node_info.ToString().Trim()) <> "", " class=''", " class='nowrt_data' ")}  data-bs-toggle='modal' data-bs-target='#exampleModal' onclick='SetExpendedPopupElement(decodeURIComponent(" + ChrW(&H22) + model_text_info + ChrW(&H22) + "),null,2,0,1,null,decodeURIComponent(" + ChrW(&H22) + "left-node" + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + model_text + ChrW(&H22) + "),null,null,decodeURIComponent(" + ChrW(&H22) + HttpUtility.UrlEncode(model.wrt_first_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22) + "),null,3,0,1,null,decodeURIComponent(" + ChrW(&H22) + "wrt-left-node1" + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + HttpUtility.UrlEncode(model.first_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + wrt_header_text + ChrW(&H22) + "))'>With respect To <img src='../../img/icon/button_arrow.svg'></button>")

                '           'html.Append($"<div class='modal fade' id='exampleModal' tabindex='-1' aria-labelledby='exampleModalLabel' aria-hidden='True'><div class='modal-dialog modal-fullscreen'><div class='modal-content'><div class='modal-header'><h3 class='modal-title'id='exampleModalLabel' style='color:white;'>INFODOCS</h3><button type='button' class='btn-close' data-bs-dismiss='modal' aria-label='Close'></button></div><div class='modal-body'><div class='row h-100'><div class='col-md-6 border-End'><div class='modal_info_header'><h3>{model.first_node}</h3><a href='#' ><img src='../../img/icon/edit-icon.svg' '></a></div>")
                '           'html.Append($"<div class='modal_info_content removep1'><p>{model.first_node_info}</p></div></div><div class='col-md-6'><div class='modal_info_header'><h3>{model.first_node + " WRT " + model.first_node}</h3><a href='#'><img src='../../img/icon/edit-icon.svg' ></a></div><div class='modal_info_content'><p>" + model.first_node_info + "</p></div></div></div></div></div></div></div>")

                '           'html.Append("</div>")
                '           html.Append("</div>")
                '       html.Append("<div class='w-100 d-none'>")
                '       html.Append("<div class='top_content_wrapper' >")
                '       html.Append("<div class=' blue_bgcolor p-3 text-center text-light clcheckboxes setDivMaxHeight5 align-self-start d-flex align-items-center position-relative resizepane' id='divFirstNode' style='height: auto;'>")
                '       'html.Append($"<input type='checkbox' class='toggle_checkbox me-2 checkchange hide_infodoc_tooltip' id='checkboxes7'> <i class='fa fa-plus-square{If(model.first_node_info IsNot Nothing AndAlso model.first_node_info <> "", "", " chkEvaluator")} hide_infodoc_tooltip' id='checkBoxIcon7' aria-hidden='true'></i>")
                '       html.Append($"<span class='ms-3 blue_bgcolor'>{model.first_node}</span>")

                '       html.Append($"<div class='tooltip_wrapper' id='tooltip_wrapper_1' style='display: none;'>")
                '       html.Append("<div class='tooltip-header'>")
                '       html.Append($"<span>{model.first_node}</span>")
                '       html.Append($"<div class='action_icons'>")

                '       html.Append($"<a href='javascript:void(0);'><i class='bx bxs-edit-alt chkEvaluator d-none' onclick='showInfoPopup(decodeURIComponent(" + ChrW(&H22) + model_text_info + ChrW(&H22) + "),null,2,0,1,null,decodeURIComponent(" + ChrW(&H22) + "left-node" + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + model_text + ChrW(&H22) + "))'></i></a>")
                '       html.Append($"<a href='javascript:void(0);'><i class='fa fa-times' aria-hidden='true' onclick=updateRightSideComment('1','tooltip','close',event)></i></a>")
                '       html.Append("</div>")
                '       html.Append("</div>")
                '       html.Append($"<p class='border-top font-size-12 pt-2 text-start mb-0'>{model.first_node_info}</p>")
                '       html.Append("</div>")

                '       html.Append("</div>")
                '       'html.Append($"<div class='toggle_area hide_infodoc_tooltip_div checkout-shipping-address5 {If((model.second_node_info) <> "" And (model.second_node_info) <> "NaN", "", "chkEvaluator")} resizepane' style='display:none;' id='checkout-shipping-address-SecondNode'>")

                '       html.Append($"<div class='my-3 position-relative{If((model.first_node_info) <> "" And (model.first_node_info) <> "NaN", "", " chkEvaluator")}' id='dvfirstnodelbl'><input type='checkbox' class='toggle_checkbox me-2 checkchange checkboxes5 checkboxes hide_infodoc_tooltip {If((model.first_node_info) <> "" And (model.first_node_info) <> "NaN", "", " chkEvaluator")}' data-val='FirstNode' id='checkboxesFirstNode'> <i class='hide_infodoc_tooltip checkBoxIcon5 mt-1 {If(model_text_info <> "" And model_text_info <> "NaN", "fa fa-minus-square ", "fa fa-plus-square chkEvaluator")}' id='checkBoxIconFirstNode'  aria-hidden='true'></i>")
                '       html.Append($"<i class='fa fa-info-circle chkEvaluator1 show_infodoc_tooltip hideinfoicon mt-1{If(model.is_infodoc_tooltip, " d-none", " d-none")}' onclick=updateRightSideComment('1','tooltip','open',event) aria-hidden='true'></i>")

                '       html.Append($"<span class='ms-2 wlr_heading'>{model.first_node}</span></div>")

                '       html.Append($"<div class='left-node-info-div tt-resizable-panel-1  toggle_area hide_infodoc_tooltip_div checkout-shipping-address5{If((model.first_node_info) <> "" And (model.first_node_info) <> "NaN", "", " chkEvaluator")} resizepane left-node-info-div columns tt-accordion-content tg-accordion-sub-1'{If((model.first_node_info) <> "" And (model.first_node_info) <> "NaN", "", " style='display:none;'")} id='checkout-shipping-address-FirstNode'>")
                '       html.Append($"<p>{model.first_node_info}</p>")

                '       'html.Append($"<a href='javascript:void(0);' class='edit_icon chkEvaluator' onclick=showInfoPopup(decodeURIComponent({ChrW(&H22) + model_text + ChrW(&H22)}),null,'2','0','1',null,'left-node',decodeURIComponent(" + ChrW(&H22) + model_text_info + ChrW(&H22) + "))>")
                '       html.Append($"<a href='javascript:void(0);' class='edit_icon chkEvaluator' onclick='showInfoPopup(decodeURIComponent(" + ChrW(&H22) + model_text_info + ChrW(&H22) + "),null,2,0,1,null,decodeURIComponent(" + ChrW(&H22) + "left-node" + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + model_text + ChrW(&H22) + "))'>")
                '       html.Append("<i class='bx bxs-edit chkEvaluator d-none' aria-hidden='true'></i>")
                '       html.Append("</a>")
                '       html.Append("</div>")
                '       html.Append("</div>")

                '       'wrt_first_node_info
                '       model_text = HttpUtility.UrlEncode(model.first_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
                '       model_text_info = HttpUtility.UrlEncode(model.wrt_first_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))

                '       html.Append($"<div class='top_content_wrapper mt-3  {If(model.wrt_first_node_info IsNot Nothing AndAlso model.wrt_first_node_info <> "", "", "chkEvaluator")}' id='dvfirst_node'>")
                '       html.Append($"<div class='wrapper_c_1 resizepane' id='idwrapper_c_1'><input type='checkbox' class='toggle_checkbox me-2 hide_infodoc_tooltip checkboxes checkboxes1' data-val='FirstNodeWRT' id='checkboxesFirstNodeWRT'> <i class='mb-0 mb-lg-4 checkBoxIcon1 hide_infodoc_tooltip {If(model_text_info <> "" And model_text_info <> "NaN", "fa fa-minus-square ", "fa fa-plus-square chkEvaluator")}' id='checkBoxIconFirstNodeWRT' aria-hidden='true'></i>")
                '       html.Append($"<i class='fa fa-info-circle chkEvaluator1 show_infodoc_tooltip hideinfoicon{If(model.is_infodoc_tooltip, " d-none", " d-none")}' onclick=updateRightSideComment('2','tooltip','open',event) aria-hidden='true'></i>")
                '       If Not model.hideInfoDocCaptions Then
                '           html.Append($"<span class='wlr_heading' title='{model.first_node} WRT {model.parent_node}'>&nbsp;")
                '           html.Append($" {If(model.first_node IsNot Nothing And model.first_node.Length > 14, model.first_node.Substring(0, 15) + "...", model.first_node)}")
                '           html.Append($" WRT {If(model.parent_node IsNot Nothing And model.parent_node.Length > 14, model.parent_node.Substring(0, 15) + "...", model.parent_node)}")
                '           html.Append("</span>")
                '       End If

                '       'html.Append($"<span class='wlr_heading' title='{model.first_node} WRT {model.parent_node}'>&nbsp;")
                '       'html.Append($" {If(model.first_node IsNot Nothing And model.first_node.Length > 14, model.first_node.Substring(0, 15) + "...", model.first_node)}")
                '       'html.Append($" WRT {If(model.parent_node IsNot Nothing And model.parent_node.Length > 14, model.parent_node.Substring(0, 15) + "...", model.parent_node)}")
                '       'html.Append("</span>")


                '       html.Append($"<div class='tooltip_wrapper' id='tooltip_wrapper_2' style='display: none;'>")
                '       html.Append("<div class='tooltip-header'>")
                '       html.Append($"<span>{model.first_node}</span>")
                '       html.Append($"<div class='action_icons'>")

                '       html.Append($"<a href='javascript:void(0);'><i class='bx bxs-edit-alt chkEvaluator d-none' onclick='showInfoPopup(decodeURIComponent(" + ChrW(&H22) + model_text_info + ChrW(&H22) + "),null,3,0,1,null,decodeURIComponent(" + ChrW(&H22) + "wrt-left-node1" + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + model_text + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + wrt_header_text + ChrW(&H22) + "))'></i></a>")
                '       html.Append($"<a href='javascript:void(0);'><i class='fa fa-times' aria-hidden='true' onclick=updateRightSideComment('2','tooltip','close',event)></i></a>")
                '       html.Append("</div>")
                '       html.Append("</div>")
                '       html.Append($"<p class='border-top font-size-12 pt-2 text-start mb-0'>{model.wrt_first_node_info}</p>")
                '       html.Append("</div></div>")


                '       html.Append($"<div class='wrt-left-node-info-div tt-resizable-panel-2 toggle_area small_content hide_infodoc_tooltip_div checkout-shipping-address1 columns tt-accordion-content tg-accordion-sub-3 resizepane'{If((model.wrt_first_node_info) <> "" And (model.wrt_first_node_info) <> "NaN", "", " style='display:block;'")} id='checkout-shipping-address-FirstNodeWRT'>")
                '       html.Append($"<p class='font-size-14 me-3'>{model.wrt_first_node_info}</p>")

                '       'html.Append($"<a href='javascript:void(0);' class='edit_icon chkEvaluator' onclick=showInfoPopup(decodeURIComponent({ChrW(&H22) + model_text + ChrW(&H22)}),null,'3','0','1',null,'wrt-left-node',decodeURIComponent({ChrW(&H22) + model_text_info + ChrW(&H22)}),decodeURIComponent({ChrW(&H22) + wrt_header_text + ChrW(&H22)}))>")
                '       html.Append($"<a href='javascript:void(0);' class='edit_icon chkEvaluator' onclick='showInfoPopup(decodeURIComponent(" + ChrW(&H22) + model_text_info + ChrW(&H22) + "),null,3,0,1,null,decodeURIComponent(" + ChrW(&H22) + "wrt-left-node1" + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + model_text + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + wrt_header_text + ChrW(&H22) + "))'>")
                '       html.Append("<i class='bx bxs-edit chkEvaluator d-none' aria-hidden='true'></i>")
                '       html.Append("</a>")
                '       html.Append("</div>")
                '       html.Append("</div>")
                '       html.Append("</div>")
                '       html.Append("</div>")
                '       'html.Append($"<div style='height:200px; width:200px; border:2px' id='abc'></div>")
                '       'html.Append($"<div class='tt-resizable-panel-2 toggle_area small_content hide_infodoc_tooltip_div checkout-shipping-address1 columns tt-accordion-content tg-accordion-sub-3'{If((model.wrt_first_node_info) <> "" And (model.wrt_first_node_info) <> "NaN", "", " style='display:none;'")} id='checkout-shipping-address-FirstNodeWRT'></div>")

                '       '=============================================

                '       'rating area
                '       'If model.pairwise_type = "ptVerbal" Then
                '       '    Dim mValue As String = If(model.value >= 9, "Extreme", If(model.value >= 8, "Very Strong to Extreme", If(model.value >= 7, "Very Strong", If(model.value >= 6, "Strong to Very Strong", If(model.value >= 5, "Strong", If(model.value >= 4, "Moderate to Strong", If(model.value >= 3, "Moderate", If(model.value >= 2, "Equal to Moderate", If(model.value >= 1, "Equal", "")))))))))
                '       '    html.Append($"<div class='rating_area single_pairwise_resp p-0'>")
                '       '    html.Append("<div class='chart_area m-auto'>")
                '       '    html.Append($"<div class='rating_result'{If(model.isPipeViewOnly, " style='pointer-events:none;'", "")}>")

                '       '    html.Append("<div  class='left_result'>")
                '       '    If (model.advantage = 1) Then
                '       '        html.Append($"<span id='divleft' class='spnleft'>{mValue}</span>")
                '       '    Else
                '       '        html.Append("<span id='divleft' style='display: none;' class='spnleft'></span>")
                '       '    End If
                '       '    html.Append("</div>")

                '       '    html.Append("<div class='equalizer'>")
                '       '    If (model.advantage = 0 And mValue IsNot "") Then
                '       '        html.Append($"<span  id='divequal'  class='spnequal'>{mValue}</span>")
                '       '    Else
                '       '        html.Append("<span  id='divequal'  style='display: none;' class='spnequal'></span>")
                '       '    End If
                '       '    html.Append("</div>")

                '       '    html.Append("<div  class='right_result'>")
                '       '    If (model.advantage = -1) Then
                '       '        html.Append($"<span id='divright'  class='spnright'>{mValue}</span>")
                '       '    Else
                '       '        html.Append("<span id='divright' style='display: none;' class='spnright'></span>")
                '       '    End If

                '       '    html.Append("</div>")

                '       '    html.Append("</div>")
                '       '    html.Append($"<div class='ratting_box'{If(model.isPipeViewOnly, " style='pointer-events:none;'", "")}>")
                '       '    Dim chartClass As String = ""
                '       '    chartClass = If(model.advantage = 1 And model.value >= 9, " selected_left1", "")
                '       '    html.Append($"<div id='div_rating_left_9' class='rating_EX rating_content rating_content_left{chartClass}' title='Extreme'  onclick=save_pairwise('9','1','left',this)>")
                '       '    html.Append("<span>EX</span>")
                '       '    html.Append("</div>")
                '       '    chartClass = If(model.advantage = 1 And model.value >= 8, " selected_left1", "")
                '       '    html.Append($"<div id='div_rating_left_8' class='rating_EX_s rating_content rating_content_left{chartClass}' title='Very Strong to Extreme'  onclick=save_pairwise('8','1','left',this)>")
                '       '    html.Append("</div>")
                '       '    chartClass = If(model.advantage = 1 And model.value >= 7, " selected_left1", "")
                '       '    html.Append($"<div id='div_rating_left_7' class='rating_VS rating_content rating_content_left{chartClass}' title='Very Strong'  onclick=save_pairwise('7','1','left',this)>")
                '       '    html.Append("<span>VS</span>")
                '       '    html.Append("</div>")
                '       '    chartClass = If(model.advantage = 1 And model.value >= 6, " selected_left1", "")
                '       '    html.Append($"<div id='div_rating_left_6' class='rating_VS_s rating_content rating_content_left{chartClass}' title='Strong to Very Strong'   onclick=save_pairwise('6','1','left',this)>")
                '       '    html.Append("</div>")
                '       '    chartClass = If(model.advantage = 1 And model.value >= 5, " selected_left1", "")
                '       '    html.Append($"<div id='div_rating_left_5' class='rating_S rating_content rating_content_left{chartClass}' title='Strong' onclick=save_pairwise('5','1','left',this)>")
                '       '    html.Append("<span>S</span>")
                '       '    html.Append("</div>")
                '       '    chartClass = If(model.advantage = 1 And model.value >= 4, " selected_left1", "")
                '       '    html.Append($"<div id='div_rating_left_4' class='rating_S_s rating_content rating_content_left{chartClass}' title='Moderate to Strong' onclick=save_pairwise('4','1','left',this)>")
                '       '    html.Append("</div>")
                '       '    chartClass = If(model.advantage = 1 And model.value >= 3, " selected_left1", "")
                '       '    html.Append($"<div id='div_rating_left_3' class='rating_M rating_content rating_content_left{chartClass}' title='Moderate' onclick=save_pairwise('3','1','left',this)>")
                '       '    html.Append("<span>M</span>")
                '       '    html.Append("</div>")
                '       '    chartClass = If(model.advantage = 1 And model.value >= 2, " selected_left1", "")
                '       '    html.Append($"<div id='div_rating_left_2' class='rating_M_e rating_content rating_content_left{chartClass}' title='Equal to Moderate' onclick=save_pairwise('2','1','left',this)>")
                '       '    html.Append("</div>")
                '       '    If model.advantage = 1 And model.value >= 1 Then
                '       '        chartClass = "rating_content_left selected_left1"
                '       '    ElseIf model.advantage = -1 And model.value >= 1 Then
                '       '        chartClass = "rating_content_right selected_right1"
                '       '    ElseIf model.advantage = 0 And model.value <= 0 Then
                '       '        chartClass = ""
                '       '    Else
                '       '        chartClass = "eqalizer_color"
                '       '    End If
                '       '    'chartClass = If(model.advantage = 1 And model.value >= 1, "rating_content_left selected_left1", "rating_content_right selected_right1")
                '       '    html.Append($"<div id='divRatingEQ' class='rating_e rating_content {chartClass}' title='Equal' onclick=save_pairwise('1','0','',this)>")
                '       '    html.Append("<span>EQ</span>")
                '       '    html.Append("</div>")
                '       '    chartClass = If(model.advantage = -1 And model.value >= 2, " selected_right1", "")
                '       '    html.Append($"<div id='div_rating_right_2' class='rating_M_e rating_content rating_content_right{chartClass}' title='Equal to Moderate' onclick=save_pairwise('2','-1','right',this)>")
                '       '    html.Append("</div>")
                '       '    chartClass = If(model.advantage = -1 And model.value >= 3, " selected_right1", "")
                '       '    html.Append($"<div id='div_rating_right_3' class='rating_M rating_content rating_content_right{chartClass}' title='Moderate' onclick=save_pairwise('3','-1','right',this)>")
                '       '    html.Append("<span>M</span>")
                '       '    html.Append("</div>")
                '       '    chartClass = If(model.advantage = -1 And model.value >= 4, " selected_right1", "")
                '       '    html.Append($"<div id='div_rating_right_4' class='rating_S_s rating_content rating_content_right{chartClass}' title='Moderate to Strong' onclick=save_pairwise('4','-1','right',this)>")
                '       '    html.Append("</div>")
                '       '    chartClass = If(model.advantage = -1 And model.value >= 5, " selected_right1", "")
                '       '    html.Append($"<div id='div_rating_right_5' class='rating_S rating_content rating_content_right{chartClass}' title='Strong' onclick=save_pairwise('5','-1','right',this)>")
                '       '    html.Append("<span>S</span>")
                '       '    html.Append("</div>")
                '       '    chartClass = If(model.advantage = -1 And model.value >= 6, " selected_right1", "")
                '       '    html.Append($"<div id='div_rating_right_6' class='rating_VS_s rating_content rating_content_right{chartClass}' title='Strong to Very Strong'  onclick=save_pairwise('6','-1','right',this)>")
                '       '    html.Append("</div>")
                '       '    chartClass = If(model.advantage = -1 And model.value >= 7, " selected_right1", "")
                '       '    html.Append($"<div id='div_rating_right_7' class='rating_VS rating_content rating_content_right{chartClass}' title='Very Strong' onclick=save_pairwise('7','-1','right',this)>")
                '       '    html.Append("<span>VS</span>")
                '       '    html.Append("</div>")
                '       '    chartClass = If(model.advantage = -1 And model.value >= 8, " selected_right1", "")
                '       '    html.Append($"<div id='div_rating_right_8' class='rating_EX_s rating_content rating_content_right{chartClass}' title='Very Strong to Extreme' onclick=save_pairwise('8','-1','right',this)>")
                '       '    html.Append("</div>")
                '       '    chartClass = If(model.advantage = -1 And model.value >= 9, " selected_right1", "")
                '       '    html.Append($"<div id='div_rating_right_9' class='rating_EX rating_content rating_content_right{chartClass}' title='Extreme' onclick=save_pairwise('9','-1','right',this)>")
                '       '    html.Append("<span>EX</span>")
                '       '    html.Append("</div>")
                '       '    html.Append("</div>")
                '       '    html.Append("</div>")
                '       '    '=========================================

                '       '    'Comment box
                '       '    html.Append("<div class='order-2 d-flex mt-3'>")

                '       '    'html.Append("<input type='checkbox'  class='comment_model'>")
                '       '    If model.show_comments Then
                '       '        html.Append("<div class='comment_div' style='cursor:pointer;'>")
                '       '    Else
                '       '        html.Append("<div class='comment_div' style='color: #333333;display: none;'>")
                '       '    End If
                '       '    html.Append("<div class='comment_icon' onclick='showNode(15)'><img src='../../img/icon/commentadd.svg'><span style='color: #333333'> Add Comment</span></div>")
                '       '    'If (model.comment Is Nothing Or model.comment = "") Then
                '       '    '    'html.Append("<i class='far fa-comments' id='single_comment_icon' onclick='showNode(15)' aria-hidden='true'></i>")
                '       '    '    html.Append("<div class='comment_icon' onclick='showNode(15)'><img src='../../img/icon/commentadd.svg'><span style='color: #333333'> Add Comment</span></div>")
                '       '    'Else
                '       '    '    'html.Append("<i Class='fa fa-comments' id='single_comment_icon' onclick='showNode(15)' title='" + model.comment + "' aria-hidden='true'></i>")
                '       '    '    html.Append("<div class='comment_icon' onclick='showNode(15)'><img src='../../img/icon/commentadd.svg'><span style='color: #333333'> Add Comment</span></div>")
                '       '    'End If
                '       '    'html.Append("<i class='fa fa-comments' aria-hidden='true'></i>")

                '       '    html.Append("<div class='info_tooltip right_tooltip' id='parentnode15' style='display:none;'>")
                '       '    html.Append("<div class='d-flex justify-content-between'>")
                '       '    html.Append("<span>Add your comment</span>")
                '       '    html.Append("<div class='action_icons'>")
                '       '    html.Append("<a href='javascript:void(0);' onclick='hideNode(15)'><i class='fa fa-times' aria-hidden='true'></i></a>")
                '       '    html.Append("</div>")
                '       '    'html.Append($"<div class='action_icons' ><a href='javascript:void(0);'><img src='../../img/icon/erasar.svg onclick='hideNode(15)' aria-hidden='true' /></a></div>")
                '       '    html.Append("</div>")
                '       '    html.Append($"<textarea{If(model.isPipeViewOnly, " style='pointer-events:none;'", "")} class='form-control mb-3 mt-2 w-100' rows='3' id='single_comment' placeholder='Add your comment'>{model.comment}</textarea>")
                '       '    html.Append("<div class='comt_btn text-end'>")
                '       '    html.Append($"<button{If(model.isPipeViewOnly, " class='d-none'", "")} type='button' onclick='showNode(15); comment_updated();'><i class='fa fa-check' aria-hidden='true'></i> OK</button>")
                '       '    html.Append("</div>")
                '       '    html.Append("</div>")
                '       '    html.Append("</div>")
                '       '    If model.show_comments Then
                '       '        html.Append($"<div{If(model.isPipeViewOnly, " style='pointer-events:none;'", "")} class='close_icon ms-3' onclick='refreshRating();'>")
                '       '    Else
                '       '        html.Append($"<div{If(model.isPipeViewOnly, " style='pointer-events:none;'", "")} class='close_icon ms-3 text-center w-100' onclick='refreshRating();'>")
                '       '    End If

                '       '    'html.Append("<i class='fa fa-times' aria-hidden='true'></i>")
                '       '    html.Append("<div class=''  id='ClearJudgment'><span><img src='../../img/icon/erasar.svg'></span><span>Clear Judgment</span></div>")
                '       '    html.Append("</div>")
                '       '    html.Append("</div>")
                '       '    html.Append("</div>")
                '       '    '=============================================
                '       'ElseIf model.pairwise_type = "" Then

                '       '    html.Append("<div class='rating_area'>")
                '       '    html.Append("<div class='scale_wt mt-5'>")

                '       '    html.Append($"<div class='form-group'{If(model.isPipeViewOnly, " style='pointer-events:none;'", "")}>")

                '       '    html.Append("<div id='at_direct_slider' class='singleDirectSlider'></div>")
                '       '    html.Append("</div>")

                '       '    html.Append("<div class='result_values'>")

                '       '    'Comment box
                '       '    If model.show_comments Then
                '       '        html.Append("<div style='cursor:pointer;'  class='comment_div px-2'>")
                '       '        html.Append("<div class='comment_icon'>")
                '       '        'html.Append("<input type='checkbox' class='comment_model'>")
                '       '        If (model.comment Is Nothing Or model.comment = "") Then
                '       '            'html.Append("<i class='far fa-comments' id='single_comment_icon' onclick='showNode(16)'  aria-hidden='true'></i>")
                '       '            html.Append("<a href='javascript:void(0)' onclick='showNode(16)'><img src='../../img/icon/commentadd.svg' id='single_comment_icon' aria-hidden='true'> <span style='color: #333333'>Add Comment</span></a>")
                '       '        Else
                '       '            'html.Append("<i Class='fa fa-comments' id='single_comment_icon' onclick='showNode(16)'  title='" + model.comment + "' aria-hidden='true'></i>")
                '       '            html.Append("<a href='javascript:void(0)' onclick='showNode(16)'><img src='../../img/icon/commentadd.svg' id='single_comment_icon' aria-hidden='true'> <span style='color: #333333'>Add Comment</span></a>")
                '       '        End If
                '       '        html.Append("<div class='info_tooltip right_tooltip'  id='parentnode16' style='display:none;'>")
                '       '        html.Append("<div class='d-flex justify-content-between'>")
                '       '        html.Append("<span>Add your comment</span>")
                '       '        html.Append($"<div class='action_icons' ><a href='javascript:void(0);' onclick='hideNode(16)'><i class='fa fa-times' onclick='hideNode(16)' aria-hidden='true'></i></a></div>")
                '       '        html.Append("</div>")
                '       '        html.Append($"<textarea{If(model.isPipeViewOnly, " style='pointer-events:none;'", "")} class='form-control mb-3 mt-2 w-100' rows='3' id='single_comment' placeholder='Add your comment'>{model.comment}</textarea>")
                '       '        html.Append("<div class='comt_btn text-end'>")
                '       '        html.Append($"<button{If(model.isPipeViewOnly, " class='d-none'", "")} type='button' onclick='showNode(16); comment_updated();'><i class='fa fa-check' aria-hidden='true'></i> OK</button>")
                '       '        html.Append("</div>")
                '       '        html.Append("</div>")
                '       '        html.Append("</div>")
                '       '        html.Append("</div>")
                '       '    Else
                '       '        html.Append("<div style='cursor:pointer;'  class='comment_div px-2'>")
                '       '        html.Append("<div class='comment_icon' style='display: none;'></div> </div>")
                '       '    End If
                '       '    html.Append("<div class='info_tooltip_main position-relative wrtFix'>")
                '       '    html.Append("<div class='brand_value'>")
                '       '    fun = $"onkeyup=SetsingleDirectSlider()"
                '       '    fun += $" onkeydown=SetsingleDirectSlider()"
                '       '    fun += $" onmousedown=SetsingleDirectSlider()"
                '       '    fun += $" onmouseup=SetsingleDirectSlider()"
                '       '    fun += $" onblur=SetsingleDirectSlider()"
                '       '    fun += " onkeypress='return isNumberKeyWithDecimal(this, event);'"
                '       '    html.Append($"<input type='text' {fun} class='value_control' title='Enter number 0 to 1' min='0' max='1' id='at_direct_input' val='{If(model.non_pw_value IsNot Nothing, model.non_pw_value, "")}'>")
                '       '    html.Append("</div>")
                '       '    html.Append("</div>")

                '       '    html.Append($"<div id='CurvechartInput' class='curvechart_input'{If(model.isPipeViewOnly, " style='pointer-events:none;'", "")}>")

                '       '    'Dim fun As String = "onkeyup=graphicalall_key_up(this,'0','-1','up',-1,'true')"
                '       '    'Dim fun As String = "onkeyup=graphical_key_up(this,'0','-1','up',null,'true')"
                '       '    'fun += " onkeydown=graphical_key_up(this,'0','-1','press',null,'true')"
                '       '    'fun += " onmousedown=graphical_key_up(this,'0','-1','press',null,'true')"
                '       '    'fun += " onmouseup=graphical_key_up(this,'0','-1','up',null,'true')"
                '       '    'fun += " onblur=graphical_key_up(this,'0','-1','up',null,'true')"
                '       '    'fun += " onkeypress='return isNumberKeyWithDecimal(this, event);'"
                '       '    'html.Append($"<input type='text' id='noUiInput11' onkeypress='return isNumberKeyWithDecimal(this, event);'>")

                '       '    'html.Append($"<div class='arrowicons'{If(model.isPipeViewOnly, " style='pointer-events:none;'", "")}>")
                '       '    'html.Append("<a href='javascript:void(0);' onclick='swap_value();'><img src='../../img/icon/value_exchange.svg' /></a>")
                '       '    'html.Append("</div>")

                '       '    'fun = "onkeyup=graphicalall_key_up(this,'1','1','up',-1,'true')"
                '       '    'fun = "onkeyup=graphical_key_up(this,'1','1','up',null,'true')"
                '       '    'fun += " onkeydown=graphical_key_up(this,'1','1','press',null,'true')"
                '       '    'fun += " onmousedown=graphical_key_up(this,'1','1','press',null,'true')"
                '       '    'fun += " onmouseup=graphical_key_up(this,'1','1','up',null,'true')"
                '       '    'fun += " onblur=graphical_key_up(this,'1','1','up',null,'true')"
                '       '    'fun += " onkeypress='return isNumberKeyWithDecimal(this, event);'"
                '       '    'html.Append($"<input type='text' id='noUiInput21' onkeypress='return isNumberKeyWithDecimal(this, event);'>")
                '       '    html.Append("</div>")
                '       '    'html.Append("<div class='d-flex justify-content-center slide_valueComment '>")
                '       '    html.Append($"<div{If(model.isPipeViewOnly, " style='pointer-events:none;'", "")} class='close_icon ms-2 ps-0 me-2' onclick=ResetSingleDirectSlider();>")
                '       '    'html.Append("<i class='fa fa-times' aria-hidden='true'></i>")
                '       '    html.Append("<span><img src='../../img/icon/erasar.svg'></span><span>Clear Judgment</span>")
                '       '    'html.Append("</div>")

                '       '    html.Append("</div>")
                '       '    html.Append("</div>")
                '       '    html.Append("</div>")

                '       '    html.Append("</div>")
                '       ''End If

                '       'second node
                '       model_text = HttpUtility.UrlEncode(model.second_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
                '       model_text_info = HttpUtility.UrlEncode(model.second_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))

                '       If (model.second_node_info.ToString().Trim() = "" And model.wrt_second_node_info.ToString().Trim() = "") Then
                '           'html.Append("<div class='value_wrapper right_info empty_info'><div class='info_data selected'>")
                '       Else
                '           'html.Append("<div class='value_wrapper right_info'><div class='info_data selected'>")
                '       End If
                '       'html.Append("<div class='value_wrapper'>")
                '       'html.Append("<div Class='info_data selected'>")

                '       'html.Append("<div id='ChartRightInfoDoc' Class='info_header'>")
                '       ''html.Append("<a href='#' ><img src='../../img/icon/info-close.svg' class='icon'></a>")
                '       'html.Append($"<span Class='title_tag text-uppercase'>{model.second_node}</span>")
                '       'html.Append($"<div class='header_edit_fw'><a href='#' data-bs-toggle='modal' data-bs-target='#exampleModal' onclick='SetExpendedPopupElement(decodeURIComponent(" + ChrW(&H22) + model_text + ChrW(&H22) + "),null,2,0,2,null,decodeURIComponent(" + ChrW(&H22) + "right-node" + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + model_text_info + ChrW(&H22) + "),null,null,decodeURIComponent(" + ChrW(&H22) + HttpUtility.UrlEncode(model.wrt_second_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22) + "),null,3,0,2,null,decodeURIComponent(" + ChrW(&H22) + "wrt-right-node1" + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + model_text + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + wrt_header_text + ChrW(&H22) + "))'><img src='../../img/icon/full-screen-svgrepo-com.svg '></a>")

                '       'html.Append($"<a href='javascript:void(0);' Class='d-md-block d-none ms-2'><img class='chkEvaluatorone' src='../../img/icon/edit-icon.svg'  onclick='showInfoPopup(decodeURIComponent(" + ChrW(&H22) + model_text + ChrW(&H22) + "),null,2,0,2,null,decodeURIComponent(" + ChrW(&H22) + "right-node" + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + model_text_info + ChrW(&H22) + "))'></a></div></div>")

                '       ''If (model.second_node_info.ToString().Trim() <> "" Or model.wrt_second_node_info.ToString().Trim() <> "") Then



                '       ''    'onclick='showInfoPopup(decodeURIComponent(" + ChrW(&H22) + rightIndex + ChrW(&H22) + ")" + "," + output.multi_pw_data(Index).NodeID_Right.ToString() + ",2,2,1,null," + ChrW(&H22) + "right-node" + ChrW(&H22) + ", decodeURIComponent(" + ChrW(&H22) + rightIndex + ChrW(&H22) + ")," + Index.ToString() + ", decodeURIComponent(" + ChrW(&H22) + parentnode + ChrW(&H22) + ")" + ")

                '       ''    'Dim rightWRTClick As String = "showInfoPopup(decodeURIComponent(" + ChrW(&H22) + output + ChrW(&H22) + ")" + "," + output.multi_pw_data(Index).NodeID_Right.ToString() + ",3,2,1,null," + ChrW(&H22) + "wrt-right-node" + ChrW(&H22) + "," + ChrW(&H22) + output.multi_pw_data(Index).RightNode + ChrW(&H22) + "," + Index.ToString() + ", decodeURIComponent(" + ChrW(&H22) + parentnode + ChrW(&H22) + ")" + ")"

                '       ''    'onclick='showInfoPopup(decodeURIComponent(" + ChrW(&H22) + output.multi_pw_data(Index).RightNode + ChrW(&H22) + ")" + "," + output.multi_pw_data(Index).NodeID_Right.ToString() + ",3,2,1,null," + ChrW(&H22) + "wrt-right-node" + ChrW(&H22) + ",decodeURIComponent(" + ChrW(&H22) + rightWRTIndex + ChrW(&H22) + ")," + Index.ToString() + ", decodeURIComponent(" + ChrW(&H22) + parentnode + ChrW(&H22) + ")" + ")'





                '       ''    'html.Append($"<div class='modal fade' id='exampleModalWRTRight' tabindex='-1' aria-labelledby='exampleModalLabel' aria-hidden='True'><div class='modal-dialog modal-fullscreen'><div class='modal-content'><div class='modal-header'><h3 class='modal-title'id='exampleModalLabel' style='color:white;'>INFODOCS</h3><button type='button' class='btn-close' data-bs-dismiss='modal' aria-label='Close'></button></div><div class='modal-body'><div class='row h-100'><div class='col-md-6 border-End'><div class='modal_info_header'><h3>{model.second_node}</h3><a href='#' ><img src='../../img/icon/edit-icon.svg' '></a></div>")
                '       ''    'html.Append($"<div class='modal_info_content removep2'><p>{model.second_node_info}</p></div></div><div class='col-md-6'><div class='modal_info_header'><h3>{model.second_node + " WRT " + model.second_node}</h3><a href='#'><img src='../../img/icon/edit-icon.svg' ></a></div><div class='modal_info_content'><p>" + model.wrt_second_node_info + "</p></div></div></div></div></div></div></div>")

                '       ''    '' Right Side InfoDoc
                '       ''    html.Append("<div Class='body_content removep2'>")
                '       ''    'If model.second_node_info.ToString().Trim().Length < 120 Then
                '       ''    '    html.Append($"<span>{model.second_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))}</span>")
                '       ''    'Else
                '       ''    '    html.Append($"<span>{model.second_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)).Substring(0, 120)} ...</span>")
                '       ''    'End If
                '       ''    'html.Append($"<span>{model.second_node_info}</span>")

                '       ''    'If (model.second_node_info.ToString().Trim() = "") Then
                '       ''    '    html.Append("<div Class='no-data_content'>")
                '       ''    '    html.Append("<h2> No Data</h2>")
                '       ''    '    html.Append("<a href='javascript:void(0);' class='chkEvaluatorone' data-bs-toggle='modal' data-bs-target='#exampleModal' onclick='SetExpendedPopupElement(decodeURIComponent(" + ChrW(&H22) + model_text + ChrW(&H22) + "),null,2,0,2,null,decodeURIComponent(" + ChrW(&H22) + "right-node" + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + model_text_info + ChrW(&H22) + "),null,null,decodeURIComponent(" + ChrW(&H22) + HttpUtility.UrlEncode(model.wrt_second_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22) + "),null,3,0,2,null,decodeURIComponent(" + ChrW(&H22) + "wrt-right-node1" + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + model_text + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + wrt_header_text + ChrW(&H22) + "))'> +<u> Add Data</u></a></div>")
                '       ''    'Else
                '       ''    '    'html.Append("</div>")
                '       ''    'End If
                '       ''    Dim infoAlttxt = model.second_node_info.ToString().Replace("</b>", "")
                '       ''    infoAlttxt = infoAlttxt.Replace("<b>", "")
                '       ''    If (model.second_node_info.ToString().Trim() = "") Then
                '       ''        html.Append("<div class=''><h2>No Data</h2></div>")
                '       ''    Else
                '       ''        html.Append($"<span>{model.second_node_info.ToString()}</span>")
                '       ''    End If

                '       ''    html.Append("</div>")


                '       ''    If (model.wrt_second_node_info.ToString().Trim() = "") Then
                '       ''        html.Append($" <div Class='wrt_content'><div class='body_content'><h2>WITH RESPECT TO <span>{model.parent_node.ToString()}</span></h2><div class=''><h3>No Data</h3> </div> </div></div>")
                '       ''    Else
                '       ''        html.Append($"<div Class='wrt_content'><div class='body_content'><h2>WITH RESPECT TO <span>{model.parent_node.ToString()}</span></h2><span>{model.wrt_second_node_info.ToString().Trim()}</span> </div></div>")
                '       ''    End If


                '       ''    'html.Append("<div Class='info_datafooter mt-4'>")
                '       ''    '    html.Append("<div Class='info_datafooter'>")
                '       ''    'html.Append($"<Button type='button' {If((model.wrt_second_node_info.ToString().Trim()) <> "", " class=''", " class='nowrt_data' ")}  data-bs-toggle='modal' data-bs-target='#exampleModal'  onclick='SetExpendedPopupElement(decodeURIComponent(" + ChrW(&H22) + model_text + ChrW(&H22) + "),null,2,0,2,null,decodeURIComponent(" + ChrW(&H22) + "right-node" + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + model_text_info + ChrW(&H22) + "),null,null,decodeURIComponent(" + ChrW(&H22) + HttpUtility.UrlEncode(model.wrt_second_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22) + "),null,3,0,2,null,decodeURIComponent(" + ChrW(&H22) + "wrt-right-node1" + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + model_text + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + wrt_header_text + ChrW(&H22) + "))' >With respect To <img src='../../img/icon/button_arrow.svg'></button>")

                '       ''    'html.Append("</div>")
                '       ''    html.Append("</div>")
                '       ''    'End If

                '       'html.Append("<div class='w-100 d-none'>")
                '       'html.Append("<div class='top_content_wrapper'>")
                '       'html.Append("<div class=' green_bgcolor p-3 text-center text-light clcheckboxes setDivMaxHeight7 align-self-start d-flex  align-items-center position-relative resizepane' id='divSecondNode' style='height: auto !important; max-width: 100%;'>")
                '       ''html.Append($"<input type='checkbox' class='toggle_checkbox me-2 checkchange hide_infodoc_tooltip' id='checkboxes5'> <i class='fa fa-plus-square{If(model.second_node_info IsNot Nothing AndAlso model.second_node_info <> "", "", " chkEvaluator")} hide_infodoc_tooltip' id='checkBoxIcon5' aria-hidden='true'></i>")
                '       'html.Append($"<span class='ms-3 green_bgcolor'>{model.second_node}</span>")

                '       'html.Append($"<div class='tooltip_wrapper' id='tooltip_wrapper_3' style='display: none;'>")
                '       'html.Append("<div class='tooltip-header'>")
                '       'html.Append($"<span>{model.second_node}</span>")
                '       'html.Append($"<div class='action_icons'>")

                '       'html.Append($"<a href='javascript:void(0);'><i class='bx bxs-edit-alt chkEvaluator d-none' onclick='showInfoPopup(decodeURIComponent(" + ChrW(&H22) + model_text_info + ChrW(&H22) + "),null,2,0,2,null,decodeURIComponent(" + ChrW(&H22) + "right-node" + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + model_text + ChrW(&H22) + "))'></i></a>")
                '       'html.Append($"<a href='javascript:void(0);'><i class='fa fa-times' aria-hidden='true' onclick=updateRightSideComment('3','tooltip','close',event)></i></a>")
                '       'html.Append("</div>")
                '       'html.Append("</div>")
                '       'html.Append($"<p class='border-top font-size-12 pt-2 text-start mb-0'>{model.second_node_info}</p>")
                '       'html.Append("</div>")

                '       'html.Append("</div>")

                '       'html.Append($"<div class='my-3 position-relative{If((model.second_node_info) <> "" And (model.second_node_info) <> "NaN", "", " chkEvaluator")}'  id='dvsecondnodelbl'><input type='checkbox' class='toggle_checkbox me-2  checkchange hide_infodoc_tooltip checkboxes checkboxes6 {If((model_text_info) <> "" And (model_text_info) <> "NaN", "", "chkEvaluator")}' data-val='SecondNode' id='checkboxesSecondNode'> <i class='checkBoxIcon5 hide_infodoc_tooltip mt-1  {If(model_text_info <> "" And model_text_info <> "NaN", "fa fa-minus-square ", "fa fa-plus-square chkEvaluator")}' id='checkBoxIconSecondNode' aria-hidden='true'></i>")
                '       ''html.Append($"<input type='checkbox' class='toggle_checkbox me-2  checkchange hide_infodoc_tooltip checkboxes checkboxes5' data-val='SecondNode' id='checkboxesSecondNode'> <i class='checkBoxIcon5 hide_infodoc_tooltip mt-1  {If(model_text_info <> "" And model_text_info <> "NaN", "fa fa-minus-square ", "fa fa-plus-square chkEvaluator")}' id='checkBoxIconSecondNode' aria-hidden='true'></i>")
                '       'html.Append($"<i class='fa fa-info-circle chkEvaluator1 show_infodoc_tooltip hideinfoicon mt-1{If(model.is_infodoc_tooltip, " d-none", " d-none")}' onclick=updateRightSideComment('3','tooltip','open',event) aria-hidden='true'></i>")

                '       'html.Append($"<span class='ms-2 wlr_heading'>{model.second_node}</span></div>")
                '       'html.Append($"<div class='left-node-info-div tt-resizable-panel-1 toggle_area hide_infodoc_tooltip_div checkout-shipping-address5 {If((model.second_node_info) <> "" And (model.second_node_info) <> "NaN", "", "chkEvaluator")} resizepane right-node-info-div columns tt-accordion-content tg-accordion-sub-2'  style='max-width: 100%; {If((model.second_node_info) <> "" And (model.second_node_info) <> "NaN", "", "display:none;")}' id='checkout-shipping-address-SecondNode'>")
                '       'html.Append($"<p>{model.second_node_info}</p>")

                '       ''html.Append($"<a href='javascript:void(0);' class='edit_icon chkEvaluator' onclick=showInfoPopup(decodeURIComponent({ChrW(&H22) + model_text + ChrW(&H22)}),null,'2','0','2',null,'right-node',decodeURIComponent({ChrW(&H22) + model_text_info + ChrW(&H22)}))>")
                '       'html.Append($"<a href='javascript:void(0);' class='edit_icon chkEvaluator' onclick='showInfoPopup(decodeURIComponent(" + ChrW(&H22) + model_text_info + ChrW(&H22) + "),null,2,0,2,null,decodeURIComponent(" + ChrW(&H22) + "right-node" + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + model_text + ChrW(&H22) + "))'>")
                '       'html.Append("<i class='bx bxs-edit chkEvaluator d-none' aria-hidden='true'></i>")
                '       'html.Append("</a>")
                '       'html.Append("</div>")
                '       'html.Append("</div>")

                '       'wrt_second_node_info
                '       model_text = HttpUtility.UrlEncode(model.second_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
                '       model_text_info = HttpUtility.UrlEncode(model.wrt_second_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
                '       'html.Append($"<div class='top_content_wrapper mt-3{If(model.wrt_second_node_info IsNot Nothing AndAlso model.wrt_second_node_info.Trim() <> "", "", " chkEvaluator")}' id='dvsecond_node'>")
                '       'html.Append($"<div class='wrapper_c_2 resizepane' id='idwrapper_c_2' style='max-width: 100%;'><input type='checkbox' class='toggle_checkbox me-2 hide_infodoc_tooltip checkboxes checkboxes2' data-val='SecondNodeWRT' id='checkboxesSecondNodeWRT'> <i class='mb-0 mb-lg-4 checkBoxIcon1 hide_infodoc_tooltip  {If(model_text_info <> "" And model_text_info <> "NaN", "fa fa-minus-square ", "fa fa-plus-square chkEvaluator")}' id='checkBoxIconSecondNodeWRT' aria-hidden='true'></i>")
                '       'html.Append($"<i class='fa fa-info-circle chkEvaluator1 show_infodoc_tooltip hideinfoicon{If(model.is_infodoc_tooltip, " d-none", " d-none")}' onclick=updateRightSideComment('4','tooltip','open',event) aria-hidden='true'></i>")
                '       'If Not model.hideInfoDocCaptions Then
                '       '    html.Append($"<span class='wlr_heading' title='{model.second_node} WRT {model.parent_node}'>&nbsp;")
                '       '    html.Append($" {If(model.second_node IsNot Nothing And model.second_node.Length > 14, model.second_node.Substring(0, 15) + "...", model.second_node)}")
                '       '    html.Append($" WRT {If(model.parent_node IsNot Nothing And model.parent_node.Length > 14, model.parent_node.Substring(0, 15) + "...", model.parent_node)}")
                '       '    html.Append("</span>")
                '       'End If

                '       ''html.Append($"<span class='wlr_heading' title='{model.second_node} WRT {model.parent_node}'>&nbsp;")
                '       ''html.Append($" {If(model.second_node IsNot Nothing And model.second_node.Length > 14, model.second_node.Substring(0, 15) + "...", model.second_node)}")
                '       ''html.Append($" WRT {If(model.parent_node IsNot Nothing And model.parent_node.Length > 14, model.parent_node.Substring(0, 15) + "...", model.parent_node)}")
                '       ''html.Append("</span>")


                '       'wrt_header_text = HttpUtility.UrlEncode(model.parent_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
                '       'html.Append($"<div class='tooltip_wrapper' id='tooltip_wrapper_4' style='display: none;'>")
                '       'html.Append("<div class='tooltip-header'>")
                '       'html.Append($"<span title='{model.second_node} WRT {model.parent_node}'>&nbsp;")
                '       'html.Append($" {If(model.second_node IsNot Nothing And model.second_node.Length > 14, model.second_node.Substring(0, 15) + "...", model.second_node)}")
                '       'html.Append($" WRT {If(model.parent_node IsNot Nothing And model.parent_node.Length > 14, model.parent_node.Substring(0, 15) + "...", model.parent_node)}")
                '       'html.Append("</span>")
                '       'html.Append($"<div class='action_icons'>")

                '       'html.Append($"<a href='javascript:void(0);'><i class='bx bxs-edit-alt chkEvaluator d-none' onclick='showInfoPopup(decodeURIComponent(" + ChrW(&H22) + model_text_info + ChrW(&H22) + "),null,3,0,1,null,decodeURIComponent(" + ChrW(&H22) + "wrt-left-node1" + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + model_text + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + wrt_header_text + ChrW(&H22) + "))'></i></a>")
                '       'html.Append($"<a href='javascript:void(0);'><i class='fa fa-times' aria-hidden='true' onclick=updateRightSideComment('4','tooltip','close',event)></i></a>")
                '       'html.Append("</div>")
                '       'html.Append("</div>")
                '       'html.Append($"<p class='border-top font-size-12 pt-2 text-start mb-0'>{model.wrt_first_node_info}</p>")
                '       'html.Append("</div></div>")

                '       'html.Append($"<div class='wrt-right-node-info-div tt-resizable-panel-2 toggle_area small_content hide_infodoc_tooltip_div checkout-shipping-address1 resizepane wrt-right-node-info-div columns tt-accordion-content tg-accordion-sub-4'  style='max-width: 100%; {If((model.wrt_second_node_info) <> "" And (model.wrt_second_node_info) <> "NaN", "", "display:none;")}' id='checkout-shipping-address-SecondNodeWRT'>  ")
                '       'html.Append($"<p class='font-size-14 me-3'>{model.wrt_second_node_info}</p>")

                '       ''html.Append($"<a href='javascript:void(0);' class='edit_icon chkEvaluator' onclick=showInfoPopup(decodeURIComponent({ChrW(&H22) + model_text + ChrW(&H22)}),null,'3','0','2',null,'wrt-right-node',decodeURIComponent({ChrW(&H22) + model_text_info + ChrW(&H22)}),decodeURIComponent({ChrW(&H22) + wrt_header_text + ChrW(&H22)}))>")
                '       'html.Append($"<a href='javascript:void(0);' class='edit_icon chkEvaluator' onclick='showInfoPopup(decodeURIComponent(" + ChrW(&H22) + model_text_info + ChrW(&H22) + "),null,3,0,2,null,decodeURIComponent(" + ChrW(&H22) + "wrt-right-node1" + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + model_text + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + wrt_header_text + ChrW(&H22) + "))'>")
                '       'html.Append("<i class='bx bxs-edit chkEvaluator d-none' aria-hidden='true'></i>")
                '       'html.Append("</a>")
                '       'html.Append("</div>")
                '       'html.Append("</div>")
                '       'html.Append("</div>")
                '       'html.Append("</div>")
                '       '=============================================

                '       html.Append("</div>")

                '       html.Append("</div>")
                '       html.Append("</div>")
                '       html.Append("</div>")
                '       html.Append("</div>")
                '       'html.Append("<div class='container-fluid'>")
                '       'html.Append("<div class='row' id='removeB'>")
                '       ''html.Append("<div class='container'>")
                '       ''Title
                '       'html.Append("<div class='col-12'>")
                '       ''==============================


                '       'html.Append("</div>")
                '       'html.Append("<div class='topheader_row col-md-12' id='titleDiv'>")
                '       'html.Append("<div id='stepTaskDiv' class='collabse_btn'>")
                '       ''html.Append($"<span class='d-flex font-size-16 justify-content-center text-center flex-wrap headerText'>{model.step_task}</span>")
                '       'html.Append($"<a{If(model.isPipeViewOnly, " class='d-none'", "")} href='javascript:void(0);' class='ms-3'>")
                '       'html.Append("<i class='bx bxs-edit-alt chkEvaluator d-none' onclick='showHeaderPopup(decodeURIComponent(" + ChrW(&H22) + stepTask + ChrW(&H22) + "),0,0,0,0,null,decodeURIComponent(" + ChrW(&H22) + "question-node" + ChrW(&H22) + "), decodeURIComponent(" + ChrW(&H22) + stepTask + ChrW(&H22) + ")" + ")' aria-hidden='true'></i>")
                '       'html.Append("</a>")
                '       'html.Append("</div>")
                '       'html.Append("</div></div>")

                '       'nodes like: first, wrt, and parent start
                '       'html.Append("<div class='row my-3' id='infoDiv'>")
                '       ''first node start
                '       'html.Append("<div class='col-md-4 offset-md-2'>")
                '       'html.Append($"<div class='info_center colorfix {If(model.first_node_info.ToString() <> "" And model.first_node_info.ToString() <> "NaN", "", "chkEvaluator")}'>")
                '       'html.Append($"<p class='font-size-14'>")
                '       ''If model.showinfodocnode Then
                '       ''    html.Append($"<i onclick ='showNode(1)' class='fa fa-info-circle{If(model.first_node_info.ToString() = "" Or model.first_node_info.ToString() = "NaN", " infoColor", "")}{If(model.first_node_info.ToString() <> "" And model.first_node_info.ToString() <> "NaN", "", " chkEvaluator")}'></i>")
                '       ''    'html.Append($"<i onclick ='showNode(1)' class='fa fa-plus-square{If(model.is_infodoc_tooltip, " d-none", " d-none")}{If(model.first_node_info.ToString() = "" Or model.first_node_info.ToString() = "NaN", " infoColor", "")}{If(model.first_node_info.ToString() <> "" And model.first_node_info.ToString() <> "NaN", "", " chkEvaluator")}'></i>")
                '       '''End If
                '       ''html.Append($" {model.first_node}{model.child_node} </p>")
                '       'html.Append("<div class='info_tooltip hideshowinfo' style='display:none;' id='parentnode1'>")
                '       'html.Append("<div class='tooltop_head'>")
                '       'html.Append($"<span>{model.first_node}{model.child_node}</span>")
                '       'html.Append("<div class='action_icons'>")
                '       'html.Append($"<a{If(model.isPipeViewOnly, " class='d-none'", "")} href='javascript:void(0);'>")
                '       'html.Append("<i class='bx bxs-edit-alt chkEvaluator d-none' onclick='showInfoPopup(decodeURIComponent(" + ChrW(&H22) + firstnodeinfo + ChrW(&H22) + ")" + ",0,2,8,1,0,null, decodeURIComponent(" + ChrW(&H22) + firstnodeinfo + ChrW(&H22) + ")" + ")' aria-hidden='true'></i>")
                '       'html.Append("</a>")
                '       'html.Append("<a href='javascript:void(0);'>")
                '       'html.Append("<i class='fa fa-times' onclick='hideNode(1)' aria-hidden='true'></i>")
                '       'html.Append("</a>")
                '       'html.Append("</div>")
                '       'html.Append("</div>")
                '       'html.Append($"<p>{model.first_node_info}</p>")
                '       'html.Append("</div>")
                '       'html.Append("</div>")
                '       'html.Append("</div>")
                '       ''first node end

                '       ''Parent node start
                '       'html.Append("<div class='col-md-3'>")
                '       'html.Append($"<div class='info_center colorfix {If(model.parent_node_info.ToString() <> "" And model.parent_node_info.ToString() <> "NaN", "", "chkEvaluator")}'>")
                '       'html.Append("<p class='font-size-14'>")
                '       ''html.Append($"<i onclick ='showNode(2)' class='fa fa-info-circle {If(model.parent_node_info.ToString() = "" Or model.parent_node_info.ToString() = "NaN", "infoColor", "")} {If(model.parent_node_info.ToString() <> "" And model.parent_node_info.ToString() <> "NaN", "", "chkEvaluator")}'></i>")
                '       ''html.Append($"<i onclick ='showNode(2)' class='fa fa-plus-square{If(model.is_infodoc_tooltip, " d-none", " d-none")}{If(model.first_node_info.ToString() = "" Or model.first_node_info.ToString() = "NaN", " infoColor", "")}{If(model.first_node_info.ToString() <> "" And model.first_node_info.ToString() <> "NaN", "", " chkEvaluator")}'></i>")
                '       ''html.Append($" {model.parent_node} </p>")
                '       'html.Append("<div class='info_tooltip hideshowinfo' style='display:none;' id='parentnode2'>")
                '       'html.Append("<div class='tooltop_head'>")
                '       'html.Append($"<span>{model.parent_node}</span>")
                '       'html.Append("<div class='action_icons'>")
                '       'html.Append($"<a{If(model.isPipeViewOnly, " class='d-none'", "")} href ='javascript:void(0);'>")
                '       'html.Append("<i class='bx bxs-edit-alt chkEvaluator d-none' onclick='showInfoPopup(decodeURIComponent(" + ChrW(&H22) + parentnodeinfo + ChrW(&H22) + ")" + ",0,-1,8,-1,0, " + ChrW(&H22) + "parent-node1" + ChrW(&H22) + ", decodeURIComponent(" + ChrW(&H22) + model.parent_node + ChrW(&H22) + ")" + ")' aria-hidden='true'></i>")
                '       'html.Append("</a>")
                '       'html.Append("<a href ='javascript:void(0);'>")
                '       'html.Append("<i class='fa fa-times' onclick='hideNode(2)' aria-hidden='true'></i>")
                '       'html.Append("</a>")
                '       'html.Append("</div>")
                '       'html.Append("</div>")
                '       'html.Append($"<p>{model.parent_node_info}</p>")
                '       'html.Append("</div>")
                '       'html.Append("</div>")
                '       'html.Append("</div>")
                '       ''Parent node end

                '       ''first wrt node start
                '       'html.Append("<div class='col-md-3'>")
                '       'html.Append($"<div class='info_center colorfix {If(model.wrt_first_node_info.ToString() <> "" And model.wrt_first_node_info.ToString() <> "NaN", "", "chkEvaluator")}'>")
                '       'html.Append("<p class='font-size-14'>")
                '       ''html.Append($"<span onclick='showNode(3)' class='wrt_text{If(model.wrt_first_node_info.ToString() <> "" And model.wrt_first_node_info.ToString() <> "NaN", "", " winfoColor")}{If(model.wrt_first_node_info.ToString() <> "" And model.wrt_first_node_info.ToString() <> "NaN", "", " chkEvaluator")}'> wrt </span>")
                '       ''html.Append($"{wrttext}</p>")
                '       'html.Append("<div class='info_tooltip hideshowinfo' style='display:none;' id='parentnode3'>")
                '       'html.Append("<div class='tooltop_head'>")
                '       'html.Append($"<span>{wrttext}</span>")
                '       'html.Append("<div class='action_icons'>")
                '       'html.Append($"<a{If(model.isPipeViewOnly, " class='d-none'", "")} href ='javascript:void(0);'>")
                '       'html.Append("<i class='bx bxs-edit-alt chkEvaluator d-none' onclick='showInfoPopup(decodeURIComponent(" + ChrW(&H22) + wrtfirstnodeinfo + ChrW(&H22) + ")" + ",0,3,8,1,0, " + ChrW(&H22) + "parent-node1" + ChrW(&H22) + ", decodeURIComponent(" + ChrW(&H22) + wrttext + ChrW(&H22) + ")" + ")' aria-hidden='true'></i>")
                '       'html.Append("</a>")
                '       'html.Append("<a href ='javascript:void(0);'>")
                '       'html.Append("<i class='fa fa-times' onclick='hideNode(3)' aria-hidden='true'></i>")
                '       'html.Append("</a>")
                '       'html.Append("</div>")
                '       'html.Append("</div>")
                '       'html.Append($"<p>{model.wrt_first_node_info}</p>")
                '       'html.Append("</div>")
                '       'html.Append("</div>")
                '       'html.Append("</div>")
                '       ''first wrt node end
                '       'html.Append("</div>")
                '       'nodes like: first, wrt, and parent end

                '       'body data
                '       'html.Append("<div class='col-lg-12'>")
                '       'html.Append("<div class='progress_table_row progress_main_container'>")
                '       'html.Append($"<div class='row_wrapper active_table_row' id='single_direct_slider_0'>")

                '       'html.Append("<div class='tooltips_group'>")
                '       'If model.show_comments Then
                '       '    html.Append("<div class='info_tooltip_main position-relative'>")

                '       '    html.Append($"<div class='comment_div' id='comment_div_0'>")
                '       '    If model.comment Is Nothing Or model.comment = "" Then
                '       '        html.Append($"<i class='far fa-comments emptyComment' id='comment_icon_0' aria-hidden='true' onclick=updateRightSideComment('0','DirectComment','open',event)></i>")
                '       '    Else
                '       '        html.Append($"<i class='fa fa-comments filledComment' id='comment_icon_0' aria-hidden='true' onclick=updateRightSideComment('0','DirectComment','open',event)></i>")
                '       '    End If

                '       '    html.Append($"<div class='tooltip_wrapper hideshowinfo' id='comment_div_box_0' style='display: none;'>")
                '       '    html.Append("<div class='d-flex justify-content-between tooltip-header'>")
                '       '    html.Append("<span>Add your comment</span>")
                '       '    html.Append($"<div class='action_icons' onclick=updateRightSideComment('0','DirectComment','close',event)>")
                '       '    html.Append("<a href='javascript:void(0);'><i class='fa fa-times' aria-hidden='true'></i></a>")
                '       '    html.Append("</div>")
                '       '    html.Append("</div>")
                '       '    html.Append($"<textarea{If(model.isPipeViewOnly, " style='pointer-events:none;'", "")} class='form-control mb-3 mt-2 w-100' id='txtRightComment_0' rows='3'>{model.comment}</textarea>")
                '       '    html.Append("<div class='comt_btn text-end'>")
                '       '    html.Append($"<button{If(model.isPipeViewOnly, " class='d-none'", "")} type='button' onclick=updateRightSideComment('0','DirectComment','close',event)><i class='fa fa-check' aria-hidden='true'></i> OK</button>")
                '       '    html.Append("</div>")
                '       '    html.Append("</div>")
                '       '    html.Append("</div>")
                '       '    html.Append("</div>")
                '       'End If
                '       'html.Append("</div>")
                '       ''If model.non_pw_value IsNot Nothing Then

                '       'html.Append($"<div class='drop_progress'{If(model.isPipeViewOnly, " style='pointer-events:none;'", "")}>")
                '       'html.Append("<div class='info_tooltip_main position-relative wrtFix'>")
                '       'html.Append("<div class='brand_value'>")
                '       'Dim fun As String = $"onkeyup=SetsingleDirectSlider()"
                '       'fun += $" onkeydown=SetsingleDirectSlider()"
                '       'fun += $" onmousedown=SetsingleDirectSlider()"
                '       'fun += $" onmouseup=SetsingleDirectSlider()"
                '       'fun += $" onblur=SetsingleDirectSlider()"
                '       'fun += " onkeypress='return isNumberKeyWithDecimal(this, event);'"
                '       'html.Append($"<input type='text' {fun} class='value_control' title='Enter number 0 to 1' min='0' max='1' id='at_direct_input' val='{If(model.non_pw_value IsNot Nothing, model.non_pw_value, "")}'>")
                '       'html.Append("</div>")
                '       'html.Append("</div>")

                '       'html.Append("<div class='progres_dropdown mt-2'>")
                '       'html.Append("<div class='form-group'>")
                '       'html.Append($"<div id='at_direct_slider' data-myval='1' class='singleDirectSlider'></div>")
                '       'html.Append("</div>")
                '       'html.Append("</div>")

                '       'html.Append($"<div class='close_icon' id='close_icon' onclick=ResetSingleDirectSlider()>")
                '       'html.Append("<i class='fa fa-times d-none' aria-hidden='true'></i>")
                '       'html.Append("</div>")
                '       'html.Append("</div>")

                '       ''End If
                '       'html.Append("</div>")
                '       'html.Append("</div>")
                '       'html.Append("</div>")

                '=============================

                html.Append("</div>")

                'html.Append("</div>")
                divContent.InnerHtml = html.ToString()
            End If
    End Sub

    Public Function GetWrtText(nodeType As String, childNode As String, parentNode As String) As String
        Dim leftText = ""
        Dim rightText = ""
        Dim wrtText = "WRT"

        If childNode.Length > 14 And wrtText.Length > 2 Then
            leftText = childNode.Substring(0, 14) + "..."
        Else
            leftText = childNode
        End If
        If parentNode.Length > 14 And wrtText.Length > 2 Then
            rightText = parentNode.Substring(0, 14) + "..."
        Else
            rightText = parentNode
        End If
        Return (leftText + " " + wrtText.Trim() + " " + rightText).Trim()
    End Function

End Class