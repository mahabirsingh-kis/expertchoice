Public Class MultiDirect
    Inherits System.Web.UI.UserControl

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    End Sub

    Public Sub bindHtml(ByVal model As AnytimeOutputModel)
        If model IsNot Nothing Then
            Dim html As StringBuilder = New StringBuilder()
            If Request.Cookies("GenActiveRowID") Is Nothing Then
                Dim GenActiveRowID As New HttpCookie("GenActiveRowID")
                GenActiveRowID.Value = "-1"
                If (GenActiveRowID.Value.Contains("GenActiveRowID")) Then
                    GenActiveRowID.Value = GenActiveRowID.Value.Substring(15)
                End If
                Response.Cookies.Add(GenActiveRowID)
                'Else
                '    Dim GenActiveRowID As HttpCookie = Request.Cookies("GenActiveRowID")
                '    InfoDocSaveRowID = Convert.ToInt32(GenActiveRowID.Value)
            End If
            Dim model_text As String = HttpUtility.UrlEncode(model.cluster_phrase.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
            Dim model_text_info As String = HttpUtility.UrlEncode(model.parent_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
            html.Append("<div class='page_heading_section'>")
            html.Append("<div class='container'>")
            html.Append("<div class='row'>")
            'html.Append("<div class='col-md-6'>")

            html.Append("<div class='col-md-6'>")
            Dim screenCheck As ScreenCheck = New ScreenCheck()
            html.Append("<div class='heading_content'><div class='page-title-box' id='titleDiv'><div class='heading_icons'>")
            If Not (screenCheck.isMobileBrowserClient(Request)) Then
                html.Append($"<a {If(model.isPipeViewOnly, " class='d-none'", " class='editsvg_icon'")}  href='javascript:void(0);' onclick=showHeaderPopup(decodeURIComponent({ChrW(&H22) + model_text + ChrW(&H22)}),'0','0','0','0',null,'question-node',decodeURIComponent({ChrW(&H22) + model_text + ChrW(&H22)}))><img class='chkEvaluatorone' src='../../img/icon/edit-icon.svg'></a>")
            End If
            html.Append("<asp:Label ID='lblTask' runat='server' ><span id='divT2S' style='font-size:20px; margin-left:1ex border: 0px !important; background: #f0f0ff08 !important; display:none'></span></asp:Label></div>")
            html.Append($"<div Class='txtStepTask removep' id='MainHeaderInfodoc'>{model.step_task}<span class='threedoticon d-none' id='btnheadinfo'> <a href='#' onclick='showheaderpopup()' ><div class='snippet'><div class='stage'><div class='dot-falling'></div> </div></div></a></span></div>")
            'html.Append("<asp:Label ID='lblTask' runat='server' ><span id='divT2S' style='font-size:20px; margin-left:1ex border: 0px !important; background: #f0f0ff08 !important; display:none'></span></asp:Label>")
            'html.Append($"<div class='heading_info' id='HeaderDivIcon'><img  id='img1' src='../../img/icon/empty_info.svg' class='d-none'><img id='img2' src='../../img/icon/info_data.svg'><img id='img3' src='../../img/icon/info-close.svg' class='d-none'></div>")
            Dim IsHideHeaderInfo As Boolean = False
            'If model.parent_node_info Is Nothing Or model.parent_node_info = "" Then
            '    html.Append($"<div class='heading_info' id='HeaderDivIcon'><a href='#'><img  id='img1' src='../../img/icon/empty_info.svg'><img id='img2' src='../../img/icon/info_data.svg' style='display: none;'><img id='img3' src='../../img/icon/info-close.svg' style='display: none;'></a></div>")
            '    IsHideHeaderInfo = True
            'Else
            '    html.Append($"<div class='heading_info' id='HeaderDivIcon'><a href='#'><img  id='img1' src='../../img/icon/empty_info.svg' style='display: none;'><img id='img2' src='../../img/icon/info_data.svg'><img id='img3' src='../../img/icon/info-close.svg' style='display: none;'></a></div>")
            'End If

            html.Append("</div>")
            html.Append("</div>")
            html.Append("</div>")
            Dim NoInfoDocDataCls As String = ""
            If model.parent_node_info.ToString().Trim() = "" Then
                NoInfoDocDataCls = " NoInfoData"
            End If
            html.Append($"<div class='col-md-6{NoInfoDocDataCls}'>")
            html.Append($"<div class='info_content d-flex mt-lg-0 mt-2' id='Header_InfoDocs' {If(IsHideHeaderInfo, " style='display: none;'", "")}><div class='heading_icons heading_info me-0'><div  id='HeaderDivIcon'><a href='#'><img  id='img1' src='../../img/icon/empty_info.svg' style='display: none;'><img id='img2' src='../../img/icon/info_data.svg'><img id='img3' src='../../img/icon/info-close.svg' style='display: none;'></a></div><div class='infodoc_icons'><a class='editsvg_icon' href='javascript:void(0);' onclick=showInfoPopup(decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.parent_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}),'0','-1','0','-1',null,'parent-node',decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.parent_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}))><img class='chkEvaluatorone' src='../../img/icon/edit-icon.svg'></a><a href='#' onclick=Expandtxt(decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.parent_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}),'0','-1','0','-1',null,'parent-node',decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.parent_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)})) data-bs-toggle='modal' data-bs-target='#infodocPop'><img src='../../img/icon/full-screen-svgrepo-com.svg' id='imgFullScreen'></a></div></div><div class='info_content_wrapper'><div class='info_text removep2'>{model.parent_node_info}</div></div></div>")

            'html.Append($"<div class='info_content d-flex mt-lg-0 mt-2' id='Header_InfoDocs' {If(IsHideHeaderInfo, " style='display: none;'", "")}><div class='heading_icons heading_info me-0'><div  id='HeaderDivIcon'><a href='#'><img  id='img1' src='../../img/icon/empty_info.svg' style='display: none;'><img id='img2' src='../../img/icon/info_data.svg'><img id='img3' src='../../img/icon/info-close.svg' style='display: none;'></a></div><div class='infodoc_icons'><a class='editsvg_icon' href='javascript:void(0);' onclick=showInfoPopup(decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.parent_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}),'0','-1','0','-1',null,'parent-node',decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.parent_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}))><img class='chkEvaluatorone' src='../../img/icon/edit-icon.svg'></a><a href='#' onclick=Expandtxt(decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.parent_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}),'0','-1','0','-1',null,'parent-node',decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.parent_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)})) data-bs-toggle='modal' data-bs-target='#infodocPop'><img src='../../img/icon/full-screen-svgrepo-com.svg' id='imgFullScreen'></a></div></div><div class='info_content_wrapper'><div class='info_text removep2'>{model.parent_node_info}</div><div class='info_box_icons' style='display:none'><a class='editsvg_icon' href='javascript:void(0);' onclick=showInfoPopup(decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.parent_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}),'0','-1','0','-1',null,'parent-node',decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.parent_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}))><img class='chkEvaluatorone' src='../../img/icon/edit-icon.svg'></a><a href='#' onclick=Expandtxt(decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.parent_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}),'0','-1','0','-1',null,'parent-node',decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.parent_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)})) data-bs-toggle='modal' data-bs-target='#infodocPop'><img src='../../img/icon/full-screen-svgrepo-com.svg' id='imgFullScreen'></a></div></div></div>")

            html.Append("</div></div>")
                html.Append("</div>")
                html.Append("</div>")
            'html.Append($"<div class='modal fade' id='exampleModalInfo' tabindex='-1' aria-labelledby='exampleModalLabel' aria-hidden='True'><div class='modal-dialog modal-fullscreen'><div class='modal-content'><div class='modal-header'><h3 class='modal-title'id='exampleModalLabel' style='color:white;'>INFODOCS</h3><button type='button' class='btn-close' data-bs-dismiss='modal' aria-label='Close'></button></div><div class='modal-body'><div class='row h-100'><div class='border-End'><div class='modal_info_header'><h3>{model.parent_node}</h3><a href='javascript:void(0);' ><img class='chkEvaluatorone' src='../../img/icon/edit-icon.svg' onclick=showInfoPopup(decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.parent_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}),'0','-1','0','-1',null,'parent-node',decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.parent_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}))></a></div><div class='modal_info_content'><p>{model.parent_node_info}</p></div></div></div></div></div></div></div>")
            'Header title

            html.Append("<div class='container'>")
                html.Append("<div class='row'>")
                html.Append("<div class='col-12' >")
                html.Append("<div class='page-title-box text-center toggle_title d-none'>")
                html.Append("<div class='collabse_btn' id='titleDiv'>")
                html.Append("<input type='checkbox' class='collabse_arrow' checked>")
                html.Append("<div class='arrow_icon'>")
                html.Append("<i class='fa fa-plus' aria-hidden='true'></i>")
                html.Append("<i class='fa fa-minus' aria-hidden='true'></i>")
                html.Append("</div>")
                'html.Append("<div>")
                html.Append($"<div class='txtStepTask'><p class='font-size-16 prm-color text-center'>{model.step_task} ")
                html.Append("<asp:Label ID='lblTask1' runat='server' /><span id='divT2S1' style='font-size:20px; margin-left:1ex border: 0px !important; background: #f0f0ff08 !important; display:none'></span>")


                html.Append($"<a{If(model.isPipeViewOnly, " class='d-none'", "")} href='javascript:void(0);' onclick=showHeaderPopup(decodeURIComponent({ChrW(&H22) + model_text + ChrW(&H22)}),'0','0','0','0',null,'question-node',decodeURIComponent({ChrW(&H22) + model_text + ChrW(&H22)}))>")
                html.Append("<i Class='bx bxs-edit-alt chkEvaluator d-none'></i></a></p></div>")

                html.Append("<div class='d-flex justify-content-center'>")
                html.Append("<div class='info_tooltip_main position-relative'>")
                html.Append($"<i class='fa fa-info-circle {If((model_text_info) = "" Or (model_text_info) = "NaN", "", "infocolorheader")} {If((model_text_info) <> "" And (model_text_info) <> "NaN", "", "infoColor")} {If((model_text_info) <> "" And (model_text_info) <> "NaN", "", "chkEvaluator")}' aria-hidden='true' onclick=updateRightSideComment('','hdrtooltip','open',event)></i>")
                html.Append("<div class='tooltip_wrapper hideshowinfo' id='tooltip_wrapper' style='display: none;'>")
                html.Append("<div class='tooltip-header'>")
                html.Append($"<span>{model.parent_node}</span>")
                html.Append("<div class='action_icons'>")
                Dim parentnode = HttpUtility.UrlEncode(model.parent_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
                html.Append($"<a{If(model.isPipeViewOnly, " class='d-none'", "")} href='javascript:void(0);'><i class='bx bxs-edit-alt chkEvaluator d-none' onclick=showInfoPopup(decodeURIComponent({ChrW(&H22) + model_text + ChrW(&H22)}),'0','-1','0','-1',null,'parent-node',decodeURIComponent({ChrW(&H22) + model_text_info + ChrW(&H22)}))></i></a>")
                'html.Append($"<a href='javascript:void(0);'><i class='bx bxs-edit-alt chkEvaluator'></i></a>")
                html.Append("<a href='javascript:void(0);'><i class='fa fa-times' aria-hidden='true'' onclick=updateRightSideComment('','hdrtooltip','close',event)></i></a>")
                html.Append("</div>")
                html.Append("</div>")
                html.Append($"<p class='border-top font-size-12 pt-2 text-start mb-0'>{model.parent_node_info}</p>")
                html.Append("</div>")
                html.Append("</div>")
                html.Append($"<p class='prm-color ms-2 {If((model_text_info) <> "" And (model_text_info) <> "NaN", "", "chkEvaluator")}'>{model.parent_node}</p>")
                html.Append("</div>")

                html.Append("</div>")

                html.Append("</div>")
                html.Append("</div>")

                'body data
                html.Append("<div class='col-lg-12'>")
                html.Append($"<div class='progress_table_row progress_main_container'>")
                If model.multi_non_pw_data IsNot Nothing Then
                'Dim active_multi_index As Integer = 0, has_undefined As Boolean = False
                'For i = 0 To model.multi_non_pw_data.Count - 1
                '    If model.multi_non_pw_data(i).RatingID = -1 Then
                '        active_multi_index = i
                '        has_undefined = True
                '        Exit For
                '    End If
                'Next
                Dim NoInfoDocRow As String = ""
                Dim NoDataOneSideInfoDoc As String = ""
                For i = 0 To model.multi_non_pw_data.Count - 1
                    NoInfoDocDataCls = ""
                    NoInfoDocRow = ""
                    NoDataOneSideInfoDoc = ""
                    'If model.multi_non_pw_data(i).InfodocWRT.Trim() = "" And model.multi_non_pw_data(i).Infodoc.Trim() = "" Then
                    '    NoInfoDocDataCls = " NoInfoData"
                    '    NoInfoDocRow = " NoInfDocDataRow"
                    'End If
                    Dim guid As String = model.multi_non_pw_data(i).sGUID.ToString()
                    Dim model_text_info_wrt = HttpUtility.UrlEncode(model.multi_non_pw_data(i).InfodocWRT.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
                    html.Append($"<div class='direct_entry_comment row_wrapper {If(i = 0, "active_table_row", "")} {NoInfoDocRow}' id='multi_direct_slider_{i}' onclick=ActiveDirect('{i}')>")
                    html.Append("<div class='tooltips_group'>")

                    'tooltip section
                    model_text = HttpUtility.UrlEncode(model.multi_non_pw_data(i).Title.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
                    model_text_info = HttpUtility.UrlEncode(model.multi_non_pw_data(i).Infodoc.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
                    If model.multi_non_pw_data(i).Infodoc.Trim() <> "" Or model.multi_non_pw_data(i).InfodocWRT.Trim() <> "" Then
                        If model.multi_non_pw_data(i).Infodoc.Trim() = "" Or model.multi_non_pw_data(i).InfodocWRT.Trim() = "" Then
                            NoDataOneSideInfoDoc = "nodataonesideinfodocrow"
                        End If
                        html.Append($"<div class='info_tooltip_main position-relative'>")
                        html.Append($"<span class='spnInfo'><a href ='javascript:void(0);'><img class='{If((model_text_info) <> "" And (model_text_info) <> "NaN", "", "infoColor")}' aria-hidden='true' src='../../img/icon/info-close.svg' /></a></span>")
                    Else
                        'html.Append($"<span class='spnInfo empty_spnInfo'><a href ='javascript:void(0);'><img class='{If((model_text_info) <> "" And (model_text_info) <> "NaN", "", "infoColor")} {If((model_text_info) <> "" And (model_text_info) <> "NaN", "", "chkEvaluator")}' data-bs-toggle='modal' data-bs-target='#exampleModal' aria-hidden='true' src='../../img/icon/empty_info.svg' onclick=SetExpendedPopupElement(decodeURIComponent({ChrW(&H22) + model_text_info + ChrW(&H22)}),'{i + 1}','2','0','1','{guid}','left-node',decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.multi_non_pw_data(i).Title.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}),null,null,decodeURIComponent({ChrW(&H22) + model_text_info_wrt + ChrW(&H22)}),'{i + 1}','3','0','1','{guid}','wrt-left-node',decodeURIComponent({ChrW(&H22) + model_text + ChrW(&H22)}),decodeURIComponent({ChrW(&H22) + parentnode + ChrW(&H22)})) /></a></span>")
                        NoInfoDocDataCls = " NoInfoData"
                        NoInfoDocRow = " NoInfDocDataRow"
                        html.Append($"<div class='info_tooltip_main position-relative{NoInfoDocDataCls }'>")
                        html.Append($"<span class='spnInfo empty_spnInfo'><a href ='javascript:void(0);'><img class='{If((model_text_info) <> "" And (model_text_info) <> "NaN", "", "infoColor")}' aria-hidden='true' src='../../img/icon/info-close.svg' style='filter: grayscale(1)' /></a></span>")
                    End If

                    html.Append($"<div class='tooltip_wrapper hideshowinfo' id='tooltip_wrapper_{i}' style='display: none;'>")
                    html.Append("<div class='tooltip-header'>")
                    html.Append($"<span>{model.multi_non_pw_data(i).Title}</span>")
                    html.Append($"<div class='action_icons'>")

                    html.Append($"<a{If(model.isPipeViewOnly, " class='d-none'", "")} href='javascript:void(0);'><i class='bx bxs-edit-alt chkEvaluator d-none'></i></a>")
                    html.Append($"<a href='javascript:void(0);'><i class='fa fa-times' aria-hidden='true'></i></a>")
                    html.Append("</div>")
                    html.Append("</div>")
                    html.Append($"<p class='border-top font-size-12 pt-2 text-start mb-0'>{model.multi_non_pw_data(i).Infodoc}</p>")
                    html.Append("</div>")
                    html.Append("</div>")
                    'End If
                    html.Append("<div class='info_tooltip_main position-relative'>")
                    html.Append("<div class='comment_div'>")
                    'comment section
                    If model.show_comments Then
                        If model.multi_non_pw_data(i).Comment Is Nothing Or model.multi_non_pw_data(i).Comment = "" Then
                            If Not (screenCheck.isMobileBrowserClient(Request)) Then
                                html.Append($"<a href ='javascript:void(0);' id='comment_icon_{i}' onclick=updateRightSideComment('{i}','comment','open',event) aria-hidden='true'> <img src = '../../img/icon/Grey-plus-icon.svg' id='ImgComment_iconEmpty_{i}' title='' aria-hidden='true'>  <img src = '../../img/icon/comment-svgrepo-com.svg' style='display:none;' id='ImgComment_icon_{i}' title='' aria-hidden='true'></a>")
                            Else
                                html.Append($"<a href ='javascript:void(0);' id='comment_icon_{i}' onclick=updateRightSideCommentMobile('{i}','comment','open',event) aria-hidden='true'> <img src = '../../img/icon/Grey-plus-icon.svg' id='ImgComment_iconEmpty_{i}' title='' aria-hidden='true'>  <img src = '../../img/icon/comment-svgrepo-com.svg' style='display:none;' id='ImgComment_icon_{i}' title='' aria-hidden='true'></a>")
                            End If
                        Else
                            If Not (screenCheck.isMobileBrowserClient(Request)) Then
                                html.Append($"<a href ='javascript:void(0);' id='comment_icon_{i}' aria-hidden='true' onclick=updateRightSideComment('{i}','comment','open',event)><img src = '../../img/icon/Grey-plus-icon.svg' id='ImgComment_iconEmpty_{i}' title='' style='display:none;' aria-hidden='true'>  <img src = '../../img/icon/comment-svgrepo-com.svg'  id='ImgComment_icon_{i}' title='{model.multi_non_pw_data(i).Comment }' aria-hidden='true'></a>")
                            Else
                                html.Append($"<a href ='javascript:void(0);' id='comment_icon_{i}' aria-hidden='true' onclick=updateRightSideCommentMobile('{i}','comment','open',event)><img src = '../../img/icon/Grey-plus-icon.svg' id='ImgComment_iconEmpty_{i}' title='' style='display:none;' aria-hidden='true'>  <img src = '../../img/icon/comment-svgrepo-com.svg'  id='ImgComment_icon_{i}' title='{model.multi_non_pw_data(i).Comment }' aria-hidden='true'></a>")
                            End If
                        End If
                        If (i >= model.multi_non_pw_data.Count - 2) Then
                            html.Append($"<div class='hideshowinfo tooltip_wrapper tooltip_top info_tooltip right_tooltip' id='comment_div_box_{i}' style='display: none;'>")
                        Else
                            html.Append($"<div class='hideshowinfo tooltip_wrapper info_tooltip right_tooltip' id='comment_div_box_{i}' style='display: none;'>")
                        End If
                        html.Append("<div class='d-flex justify-content-between'>")
                        html.Append("<span>Add your comment</span>")
                        html.Append($"<div class='action_icons'><a href='javascript:void(0);' onclick='HideMultiDerectCommentBox({i})'><i class='fa fa-times' ></i></a></div>")
                        html.Append("</div>")
                        html.Append($"<textarea{If(model.isPipeViewOnly, " style='pointer-events:none;'", "")} class='form-control mb-3 mt-2 w-100' placeholder='Add your comment' id='txtRightComment_{i}' rows='3'>{model.multi_non_pw_data(i).Comment}</textarea>")
                        html.Append("<div class='comt_btn text-end'>")
                        html.Append($"<button type='button' onclick=updateRightSideComment('{i}','comment','close',event)><i class='fa fa-check' aria-hidden='true'></i> OK</button>")
                        html.Append("</div>")
                        html.Append("</div>")
                    End If
                    html.Append("</div>")
                    html.Append("</div>")


                    html.Append("<div class='brand_name'>")
                    html.Append($"<span title='{model.multi_non_pw_data(i).Title}'>{model.multi_non_pw_data(i).Title}</span>")
                    html.Append("</div>")
                    html.Append("</div>")

                    model_text = HttpUtility.UrlEncode(model.multi_non_pw_data(i).Title.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))

                    Dim wrt_header_text As String = HttpUtility.UrlEncode(model.parent_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))

                    html.Append("<div class='drop_progress'>")
                    html.Append("<div class='info_tooltip_main position-relative wrtFix'>")
                    'If model.showinfodocnode And model.is_infodoc_tooltip Then
                    'html.Append($"<span class='spnInfo'><span class='w-bandage {If((model_text_info) <> "" And (model_text_info) <> "NaN", "", "winfoColor")} {If((model_text_info) <> "" And (model_text_info) <> "NaN", "", "chkEvaluator")}' onclick=updateRightSideComment('{i}','bandage','open',event) id='w_bandage_{i}'>wrt</span></span>")
                    'End If
                    html.Append($"<div class='brand_value'{If(model.isPipeViewOnly, " style='pointer-events:none;'", "")}>")

                    Dim fun As String = $"onkeyup=SetMultiDirectSlider('{i}')"
                    fun += $" onkeydown=SetMultiDirectSlider('{i}')"
                    fun += $" onmousedown=SetMultiDirectSlider('{i}')"
                    fun += $" onmouseup=SetMultiDirectSlider('{i}')"
                    fun += $" onblur=SetMultiDirectSlider('{i}')"
                    fun += " onkeypress='return isNumberKeyWithDecimal(this, event);'"
                    html.Append($"<input type='text'  {fun} class='value_control directinput' title='Enter number 0 to 1' min='0' max='1' id='at_direct_input{i}'>")
                    html.Append("</div>")

                    'End If
                    html.Append("</div>")
                    html.Append($"<div class='progres_dropdown mt-2'{If(model.isPipeViewOnly, " style='pointer-events:none;'", "")}>")
                    html.Append("<div class='form-group'>")
                    html.Append($"<div id='at_direct_slider{i}' data-myval='{i}' class='multiDirectSlider'></div>")
                    html.Append("</div>")
                    html.Append("</div>")

                    html.Append($"<div class='close_icon mt-1' id='close_icon_{i}' onclick=ResetMultiDirectSlider('{i}'){If(model.isPipeViewOnly, " style='pointer-events:none;'", "")}>")
                    html.Append("<img src='../../img/icon/erasar.svg' style='cursor: pointer;' class='imgClose' >")
                    html.Append("</div>")
                    html.Append("</div>")
                    'If model.multi_non_pw_data(i).Infodoc <> "" Then
                    html.Append($"<div class='open_tooltip_info{NoInfoDocDataCls}'><hr><div class='body_content'><div class='split_content {NoDataOneSideInfoDoc}'>")
                    If (model.multi_non_pw_data(i).Infodoc <> "") Then
                        'html.Append($"<div class='normal_content'><h3>INFO</h3><p>{model.multi_non_pw_data(i).Infodoc.ToString()} </p>")
                        html.Append($"<div class='normal_content'><p>{model.multi_non_pw_data(i).Infodoc.ToString()} </p>")
                    Else
                        html.Append("<div class='normal_content NoInfoData'><h2 class='nodata_level'>No Data</h2>")
                        'html.Append("<div class='normal_content NoInfoData'><h3>INFO</h3><h2 class='nodata_level'>No Data</h2>")
                    End If
                    html.Append("</div>")

                    If (model.multi_non_pw_data(i).InfodocWRT <> "") Then
                        html.Append($"<div class='wrt_right_content'><h3>WITH RESPECT TO <span>{model.parent_node}</span></h3>")
                        html.Append($"<p>{model.multi_non_pw_data(i).InfodocWRT} </p>")
                    Else
                        html.Append($"<div class='wrt_right_content NoInfoData'><h3>WITH RESPECT TO <span>{model.parent_node}</span></h3>")
                        html.Append("<h2 class='nodata_level'>No Data</h2>")
                    End If
                    html.Append($"</div>")
                    html.Append($"<a class='fullscreen_link' href='#' data-bs-toggle='modal' data-bs-target='#exampleModal' onclick=SetExpendedPopupElement(decodeURIComponent({ChrW(&H22) + model_text_info + ChrW(&H22)}),'{i + 1}','2','0','1','{guid}','left-node',decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.multi_non_pw_data(i).Title.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}),decodeURIComponent({ChrW(&H22) + model_text_info + ChrW(&H22)}),null,decodeURIComponent({ChrW(&H22) + model_text_info_wrt + ChrW(&H22)}),'{i + 1}','3','0','1','{guid}','wrt-left-node',decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.multi_non_pw_data(i).Title.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}),decodeURIComponent({ChrW(&H22) + parentnode + ChrW(&H22)}))><img src='../../img/icon/full-screen-svgrepo-com.svg '></a>")

                    'html.Append($"<a class='fullscreen_link' onclick=' href='#' data-bs-toggle='modal' data-bs-target='#exampleModalWRT_{i}'><img src='../../img/icon/full-screen-svgrepo-com.svg'></a>")
                    html.Append($"</div></div></div>")

                    'html.Append($"<div class='modal fade' id='exampleModalWRT_{i}' tabindex='-1' aria-labelledby='exampleModalLabel' aria-hidden='True'><div class='modal-dialog modal-fullscreen'><div class='modal-content'><div class='modal-header'><h3 class='modal-title'id='exampleModalLabel' style='color:white;'>INFODOCS</h3><button type='button' class='btn-close' data-bs-dismiss='modal' aria-label='Close'></button></div><div class='modal-body'><div class='row h-100'><div class='col-md-6 border-End'><div class='modal_info_header'><h3>{model.multi_non_pw_data(i).Title}</h3><a href='javascript:void(0);'><img class='chkEvaluatorone' src='../../img/icon/edit-icon.svg' onclick=showInfoPopup(decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.multi_non_pw_data(i).Title.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}),'{i + 1}','2','0','1','{guid}','left-node',decodeURIComponent({ChrW(&H22) + model_text_info + ChrW(&H22)}))></a></div><div class='modal_info_content'><p>{model.multi_non_pw_data(i).Infodoc}</p></div></div><div class='col-md-6'><div class='modal_info_header'><h3>With respect to {model.parent_node}</h3><a href='javascript:void(0);'><img class='chkEvaluatorone' src='../../img/icon/edit-icon.svg'onclick=showInfoPopup(decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.multi_non_pw_data(i).Title.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}),'{i + 1}','3','0','1','{guid}','wrt-left-node',decodeURIComponent({ChrW(&H22) + model_text_info_wrt + ChrW(&H22)}),decodeURIComponent({ChrW(&H22) + parentnode + ChrW(&H22)})) ></a></div><div class='modal_info_content'><p>{model.multi_non_pw_data(i).InfodocWRT}</p></div></div></div></div></div>")

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
                    'Else
                    '    html.Append($"</div></div><div class='info_datafooter d-block '><button type='button' {If((model.multi_non_pw_data(i).InfodocWRT.ToString().Trim()) <> "", " class=''", " class='nowrt_data' ")}' data-bs-toggle='modal' data-bs-target='#exampleModalWRT_{i}'>with respect to <img src='../../img/icon/button_arrow.svg'></button></div></div>")
                    '    html.Append($"<div class='modal fade' id='exampleModalWRT_{i}' tabindex='-1' aria-labelledby='exampleModalLabel' aria-hidden='True'><div class='modal-dialog modal-fullscreen'><div class='modal-content'><div class='modal-header'><h3 class='modal-title'id='exampleModalLabel' style='color:white;'>INFODOCS</h3><button type='button' class='btn-close' data-bs-dismiss='modal' aria-label='Close'></button></div><div class='modal-body'><div class='row h-100'><div class='col-md-6 border-End'><div class='modal_info_header'><h3>{model.multi_non_pw_data(i).Title}</h3><a href='javascript:void(0);'><img class='chkEvaluatorone' src='../../img/icon/edit-icon.svg' onclick=showInfoPopup(decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.multi_non_pw_data(i).Title.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}),'{i + 1}','2','0','1','{guid}','left-node',decodeURIComponent({ChrW(&H22) + model_text_info + ChrW(&H22)}))></a></div><div class='modal_info_content'><p>{model.multi_non_pw_data(i).Infodoc}</p></div></div><div class='col-md-6'><div class='modal_info_header'><h3>With respect to {model.parent_node}</h3><a href='javascript:void(0);'><img class='chkEvaluatorone' src='../../img/icon/edit-icon.svg'onclick=showInfoPopup(decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.multi_non_pw_data(i).Title.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}),'{i + 1}','3','0','1','{guid}','wrt-left-node',decodeURIComponent({ChrW(&H22) + model_text_info_wrt + ChrW(&H22)}),decodeURIComponent({ChrW(&H22) + parentnode + ChrW(&H22)})) ></a></div><div class='modal_info_content'><p>{model.multi_non_pw_data(i).InfodocWRT}</p></div></div></div></div></div></div></div>")
                    'End If
                    ' End If

                    html.Append("</div>")
                Next
            End If
                html.Append("</div>")
                html.Append("</div>")
                html.Append("</div>")
                '=====================

                html.Append("</div>")
                html.Append("</div>")

                divContent.InnerHtml = html.ToString()
            End If
    End Sub

End Class