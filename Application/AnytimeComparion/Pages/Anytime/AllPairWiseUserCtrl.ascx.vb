Imports System.Runtime.InteropServices
Imports System.Web.Services

Public Class AllPairWiseUserCtrl
    Inherits System.Web.UI.UserControl

    Dim screenCheck As ScreenCheck = New ScreenCheck()

    Public Sub New()

    End Sub

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    End Sub

    Public Sub BindHtml(ByRef output As AnytimeOutputModel)
        Dim m As Match
        Dim IsMobile = False
        Dim is_AT_owner = True
        Dim active_multi_index = 0
        Dim main_gradient_checkbox = False
        Dim text_limit = 0
        ''...
        Dim last = False
        Dim screen_sizes As New Dictionary(Of String, Integer)
        screen_sizes.Add("option", 0)
        Dim bars_left()() As String = New String(7)() {}
        bars_left(0) = {"9", "Extreme", "nine", "EX"}
        bars_left(1) = {"8", "Very Strong to Extreme", "eight", " "}
        bars_left(2) = {"7", "Very Strong", "seven", "VS"}
        bars_left(3) = {"6", "Strong to Very Strong", "six", " "}
        bars_left(4) = {"5", "Strong", "five", "S"}
        bars_left(5) = {"4", "Moderate to Strong", "four", " "}
        bars_left(6) = {"3", "Moderate", "three", "M"}
        bars_left(7) = {"2", "Equal to Moderate", "two", " "}

        Dim bars_right()() As String = New String(7)() {}
        bars_right(0) = {"2", "Equal to Moderate", "two", " "}
        bars_right(1) = {"3", "Moderate", "three", "M"}
        bars_right(2) = {"4", "Moderate to Strong", "four", " "}
        bars_right(3) = {"5", "Strong", "five", "S"}
        bars_right(4) = {"6", "Strong to Very Strong", "six", " "}
        bars_right(5) = {"7", "Very Strong", "seven", "VS"}
        bars_right(6) = {"8", "Very Strong to Extreme", "eight", " "}
        bars_right(7) = {"9", "Extreme", "nine", "EX"}
        Dim left_input As Decimal() = New Decimal() {}
        Dim right_input As Decimal() = New Decimal() {}

        'Dim hdnMultiPwData As HiddenField = CType(Page.Form.FindControl("hdnMultiPwData"), HiddenField)
        'hdnMultiPwData.Value = JsonConvert.SerializeObject(output.multi_pw_data)

        Dim hdnLeftBar As HiddenField = CType(Page.Form.FindControl("hdnLeftBar"), HiddenField)
        hdnLeftBar.Value = JsonConvert.SerializeObject(bars_left)

        Dim hdnRightBar As HiddenField = CType(Page.Form.FindControl("hdnRightBar"), HiddenField)
        hdnRightBar.Value = JsonConvert.SerializeObject(bars_right)

        Dim html As StringBuilder = New StringBuilder()

        question.Visible = True
        Dim steptask As String = HttpUtility.UrlEncode(output.cluster_phrase.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
        Dim parentnode As String = HttpUtility.UrlEncode(output.parent_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
        Dim parentnodeinfo As String = HttpUtility.UrlEncode(output.parent_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
        'html.Append("Vendor / Source Selection.'</strong> Review your results below to ensure they make sense to you. If not, you may navigate back to the previous judgments to edit them.")
        html.Append("<div class='page_heading_section'>")
        html.Append("<div class='container'>")

        html.Append("<div class='row'>")
        html.Append("<div class='col-md-6'>")

        Dim NoInfoDocDataCls As String = ""
        Dim NoInfoDocRowData As String = ""
        Dim filter_gray As String = ""
        If output.parent_node_info.Trim() = "" Then
            NoInfoDocDataCls = " NoInfoData"
        End If
        'html.Append("<div class='heading_content' id='titleDiv'>")
        'html.Append("<div class='txtStepTask'>")
        'html.Append("<p>" + output.step_task + "</p>")
        'html.Append("</div>")
        'html.Append("<div class='heading_info'><img src='../../img/icon/empty_info.svg' class='d-none'><img src='../../img/icon/info_data.svg' ><img src='../../img/icon/info-close.svg' class='d-none'></div>")

        'html.Append("<div class='col-md-12'><div class='font-size-16 text-center prm-color d-flex  justify-content-center'><p class='font-size-18 prm-color text-center'><div class='txtStepTask'><p>" + output.step_task + "</p></div><br />")
        'html.Append($"<div class='editStepTask ms-3'><a{If(output.isPipeViewOnly, " class='d-none'", "")} href='javascript:void(0);' style='cursor:pointer;' onclick='showHeaderPopup(decodeURIComponent(" + ChrW(&H22) + steptask + ChrW(&H22) + ")" + ",0,0,2,1,null)'><i Class='bx bxs-edit-alt chkEvaluator d-none'></i></a></div></p> </div>")
        'html.Append("</div>")
        Dim CommentDivStyle, ClearJudgmentclose_iconDiv As String
        If output.show_comments Then
            CommentDivStyle = ""
            ClearJudgmentclose_iconDiv = ""
        Else
            CommentDivStyle = " style='display: none;' "
            ClearJudgmentclose_iconDiv = " text-center"
        End If
        Dim model_text As String = HttpUtility.UrlEncode(output.cluster_phrase.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
        html.Append($"<div class='heading_content'><div class='page-title-box' id='titleDiv'><div class='heading_icons'>")
        If Not (screenCheck.isMobileBrowserClient(Request)) Then
            html.Append($"<a {If(output.isPipeViewOnly, " class='d-none'", " class='editsvg_icon'")}  href='javascript:void(0);' onclick=showHeaderPopup(decodeURIComponent({ChrW(&H22) + model_text + ChrW(&H22)}),'0','0','0','0',null,'question-node',decodeURIComponent({ChrW(&H22) + model_text + ChrW(&H22)}))><img class='chkEvaluatorone' src='../../img/icon/edit-icon.svg'></a>")
        End If
        html.Append("<asp:Label ID='lblTask' runat='server' ><span id='divT2S' style='font-size:20px; margin-left:1ex border: 0px !important; background: #f0f0ff08 !important; display:none'></span></asp:Label></div>")
        html.Append($"<div Class='txtStepTask removep' id='MainHeaderInfodoc'>{output.step_task}<span class='threedoticon d-none' id='btnheadinfo'> <a href='#' onclick='showheaderpopup()' ><div class='snippet'><div class='stage'><div class='dot-falling'></div> </div></div></a></span></div>")

        ' html.Append("<asp:Label ID='lblTask' runat='server' ><span id='divT2S' style='font-size:20px; margin-left:1ex border: 0px !important; background: #f0f0ff08 !important; display:none'></span></asp:Label>")
        'html.Append("<div class='heading_info'><img src='../../img/icon/empty_info.svg' class='d-none'><img src='../../img/icon/info_data.svg'><img src='../../img/icon/info-close.svg' class='d-none'></div>")

        Dim IsHideHeaderInfo As Boolean = False
        'If output.parent_node_info Is Nothing Or output.parent_node_info = "" Then
        '    html.Append($"<div class='heading_info' id='HeaderDivIcon'><a href ='javascript:void(0);'><img  id='img1' src='../../img/icon/empty_info.svg'><img id='img2' src='../../img/icon/info_data.svg' style='display: none;'><img id='img3' src='../../img/icon/info-close.svg' style='display: none;'></a></div>")
        '    IsHideHeaderInfo = True
        'Else
        '    html.Append($"<div class='heading_info' id='HeaderDivIcon'><a href ='javascript:void(0);'><img  id='img1' src='../../img/icon/empty_info.svg' style='display: none;'><img id='img2' src='../../img/icon/info_data.svg'><img id='img3' src='../../img/icon/info-close.svg' style='display: none;'></a></div>")
        'End If

        html.Append("</div>")
        html.Append("</div>")

        'html.Append("<asp:Label ID='lblTask' runat='server' /><span id='divT2S' style='font-size:20px; margin-left:1ex border: 0px !important; background: #f0f0ff08 !important; display:none'></span>")
        'html.Append("</div>")
        html.Append("</div>")
        html.Append($"<div class='col-md-6{NoInfoDocDataCls}'>")
        'If output.is_infodoc_tooltip Or output.showinfodocnode Then
        'html.Append($"<p'>{output.parent_node_info}</p><p></p></p>")
        html.Append($"<div class='info_content d-flex mt-lg-0 mt-2' id='Header_InfoDocs' {If(IsHideHeaderInfo, " style='display: none;'", "")}><div class='heading_icons heading_info me-0'><div  id='HeaderDivIcon'><a href='#'><img  id='img1' src='../../img/icon/empty_info.svg' style='display: none;'><img id='img2' src='../../img/icon/info_data.svg'><img id='img3' src='../../img/icon/info-close.svg' style='display: none;'></a></div><div class='infodoc_icons'><a class='editsvg_icon' href='javascript:void(0);' onclick=showInfoPopup(decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(output.parent_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}),'0','-1','0','-1',null,'parent-node',decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(output.parent_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}))><img class='chkEvaluatorone' src='../../img/icon/edit-icon.svg'></a><a href='#' onclick=Expandtxt(decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(output.parent_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}),'0','-1','0','-1',null,'parent-node',decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(output.parent_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)})) data-bs-toggle='modal' data-bs-target='#infodocPop'><img src='../../img/icon/full-screen-svgrepo-com.svg' id='ibtnfullScreen'></a></div></div><div class='info_content_wrapper'><div class='info_text'>{output.parent_node_info}</div><div class='info_box_icons d-none'>")
        html.Append($"<a Class='editsvg_icon' href='javascript:void(0);' onclick=showInfoPopup(decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(output.parent_node) + ChrW(&H22)}),'0','-1','0','-1',null,'parent-node',decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(output.parent_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}))><img class='chkEvaluatorone' src='../../img/icon/edit-icon.svg'></a>")
        If Not (screenCheck.isMobileBrowserClient(Request)) Then
            html.Append($"<a href='javascript:void(0);' data-bs-toggle='modal' data-bs-target='#infodocPop'><img src='../../img/icon/full-screen-svgrepo-com.svg' onclick=Expandtxt(decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(output.parent_node) + ChrW(&H22)}),'0','-1','0','-1',null,'parent-node',decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(output.parent_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}))></a>")
        End If
        html.Append("</div></div></div>")
        'End If
        html.Append("</div>")
        'html.Append("<div class='info_box_icons'><a class='editsvg_icon' href='javascript:void(0);' onclick='showInfoPopup(decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(output.parent_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}),'0','-1','0','-1',null,'parent-node',decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(output.parent_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}))'><img src='../../img/icon/edit-icon.svg'></a><a href='javascript:void(0);' data-bs-toggle='modal' data-bs-target='#exampleModal'><img src='../../img/icon/full-screen-svgrepo-com.svg'></a></div>")
        html.Append("</div>")
        'html.Append($"<div class='modal fade' id='exampleModaltest' tabindex='-1' aria-labelledby='exampleModalLabel' aria-hidden='True'><div class='modal-dialog modal-fullscreen'><div class='modal-content'><div class='modal-header'><h3 class='modal-title'id='exampleModalLabel' style='color:white;'>INFODOCS</h3><button type='button' class='btn-close' data-bs-dismiss='modal' aria-label='Close'></button></div><div class='modal-body'><div class='row h-100'><div class='border-End'><div class='modal_info_header'><h3>{output.parent_node}</h3><a href='javascript:void(0);' class='chkEvaluatorone'><img src='../../img/icon/edit-icon.svg' onclick='showInfoPopup(decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(output.parent_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}),'0','-1','0','-1',null,'parent-node',decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(output.parent_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}))'></a></div><div class='modal_info_content'><p>{output.parent_node_info}</p></div></div></div></div></div></div></div>")
        'html.Append("</div>")

        If output.is_infodoc_tooltip Or output.showinfodocnode Then
            'html.Append($"<div class='info_center centered_tooltip' id='InfoDocsParentDiv'><p class='font-size-14 text-center {If((parentnodeinfo) <> "" And (parentnodeinfo) <> "NaN", "", "chkEvaluator")}'><i onclick='showNode(101)' class='fa fa-info-circle {If((parentnodeinfo) <> "" And (parentnodeinfo) <> "NaN", "", "iinfoColor")}  definfovalue {If((parentnode) = "" Or (parentnode) = "NaN", "", "headerinfoColor")}'></i> {output.parent_node}</p>")
            html.Append($"<div class='info_tooltip hideshowinfo parentnode101 {If(output.parent_node_info = "", " divchkEvaluator", "")}' style='display:none;' id='parentnode101'><div class='tooltop_head'><span>" + output.parent_node + "</span><div class='action_icons'>")
            html.Append($"<a{If(output.isPipeViewOnly, " class='d-none'", "")} href='javascript:void(0);'><i class='fas fa-edit chkEvaluator d-none' onclick='showInfoPopup(decodeURIComponent(" + ChrW(&H22) + parentnodeinfo + ChrW(&H22) + ")" + ",0,-1,2,-1,0, " + ChrW(&H22) + "parent-node1" + ChrW(&H22) + ", decodeURIComponent(" + ChrW(&H22) + parentnode + ChrW(&H22) + ")" + ")' aria-hidden='true'></i></a>")
            html.Append("<a href='javascript:void(0);'><i class='fa fa-times' onclick='hideNode(101)' aria-hidden='true'></i></a></div></div><p>" + output.parent_node_info.ToString() + "</p></div></div>")
        End If
        'If output.pairwise_type = "ptVerbal" Then
        '    html.Append("<div class='pairwise_legends d-lg-none'><span><strong>EQ</strong>ual</span><span><strong>M</strong>oderate</span><span><strong>S</strong>trong</span><span><strong>V</strong>ery <strong>S</strong>trong</span><span><strong>EX</strong>treme</span></div>")
        'End If
        html.Append("<div class='d-none'> <a href='#' onclick='ShowMainWrapperStyle()' Class='btn-secondary'>test</a></div>")
        html.Append("</div>")
        html.Append("</div>")

        html.Append("<div class='container'>")
        html.Append("<div class='col-md-12' id='divAllPairwise'>")
        html.Append($"<div id='InfoDocsParentDiv_1'></div>")
        'If output.pairwise_type = "ptVerbal" And IsMobile Then
        '    html.Append("<div class='original-legend text-center' style='font-size: .750rem;'><b>EQ</b>ual<b>&nbsp;&nbsp;&nbsp;M</b>oderate  &nbsp;&nbsp;&nbsp;<b>S</b>trong &nbsp;&nbsp;&nbsp;<b>V</b>ery<b>S</b>trong  &nbsp;&nbsp;&nbsp;<b>EX</b>treme</div>")
        'End If

        If Request.Cookies("GenActiveRowID") Is Nothing Then
            Dim ActiveRowID As New HttpCookie("GenActiveRowID")
            ActiveRowID.Value = "-1"
            If (ActiveRowID.Value.Contains("GenActiveRowID")) Then
                ActiveRowID.Value = ActiveRowID.Value.Substring(12)
            End If
            Response.Cookies.Add(ActiveRowID)
        End If
        For index = 0 To If(output.multi_pw_data Is Nothing, 0, If(output.multi_pw_data.Count > 50, 10, output.multi_pw_data.Count - 1))
            filter_gray = ""
            'For index = 0 To output.multi_pw_data.Count - 1
            'If ActRowID = index Then
            '    html.Append($"<div class='chart_wrapper graphical_chart active_row' data-val='{index}' onclick='activeRow({index}, true);' id='index_" + index.ToString() + "'>")
            'Else
            NoInfoDocDataCls = ""
            html.Append($"<div class='chart_wrapper graphical_data graphical_chart unselected_row verbal_grphical_comment{NoInfoDocRowData}' data-val='{index}' onclick='activeRow({index}, true);' id='index_" + index.ToString() + "'>")
            'End If
            ' If Not IsMobile Then
            Dim left As String = HttpUtility.UrlEncode(output.multi_pw_data(index).InfodocLeft.ToString().Replace("<b>", "").Replace("</b>", "").Trim().Replace(ChrW(&H22), ChrW(&H27)))
            Dim leftWRT As String = HttpUtility.UrlEncode(output.multi_pw_data(index).InfodocLeftWRT.ToString().Replace("<b>", "").Replace("</b>", "").Trim().Replace(ChrW(&H22), ChrW(&H27)))
            m = Regex.Match(output.multi_pw_data(index).InfodocLeft.ToString().Trim(), "<img")
            Dim imstyl As String = ""
            If m.Success Then
                imstyl = " image_main_wrapper"
            End If
            If output.multi_pw_data(index).InfodocLeft.ToString().Trim() <> "" Or output.multi_pw_data(index).InfodocLeftWRT.ToString().Trim() <> "" Then
                If output.multi_pw_data(index).InfodocLeft.ToString().Trim() = "" Or output.multi_pw_data(index).InfodocLeftWRT.ToString().Trim() = "" Then
                    html.Append($"<div class='value_wrapper left_info nodataonesideinfodocrow{imstyl}'><div class='info_data selected'>")
                Else
                    html.Append($"<div class='value_wrapper left_info{imstyl}'><div class='info_data selected'>")
                End If

            Else
                NoInfoDocDataCls = "NoInfoData"
                filter_gray = " filter_gray"
                html.Append($"<div class='value_wrapper left_info emptyInfodocbothside{imstyl}'><div class='info_data selected'>")
            End If
            html.Append("<div Class='info_header'>")
            html.Append($"<a class='{NoInfoDocDataCls}{filter_gray}'><img src='../../img/icon/info-close.svg' class='icon'></a>")
            html.Append($"<span Class='title_tag text-uppercase bottom' title='{output.multi_pw_data(index).LeftNode}'>{output.multi_pw_data(index).LeftNode}</span>")
            html.Append($"<div class='header_edit_fw {NoInfoDocDataCls}'>")
            html.Append("<a href='#' data-bs-toggle='modal' data-bs-target='#exampleModal' onclick='SetExpendedPopupElement(decodeURIComponent(" + ChrW(&H22) + left + ChrW(&H22) + ")" + "," + output.multi_pw_data(index).NodeID_Left.ToString() + ",2,2,1,null," + ChrW(&H22) + "left-text-node" + ChrW(&H22) + "," + ChrW(&H22) + output.multi_pw_data(index).LeftNode + ChrW(&H22) + "," + index.ToString() + ", decodeURIComponent(" + ChrW(&H22) + parentnode + ChrW(&H22) + ")," + "decodeURIComponent(" + ChrW(&H22) + leftWRT + ChrW(&H22) + ")," + output.multi_pw_data(index).NodeID_Left.ToString() + ",3,2,1,null," + ChrW(&H22) + "wrt-text-node" + ChrW(&H22) + "," + ChrW(&H22) + output.multi_pw_data(index).LeftNode + ChrW(&H22) + "," + index.ToString() + ",decodeURIComponent(" + ChrW(&H22) + parentnode + ChrW(&H22) + "))'><img src='../../img/icon/full-screen-svgrepo-com.svg '></a>")
            'If Not (screenCheck.isMobileBrowserClient(Request)) Then
            '    html.Append("<a href='#' class='d-md-block d-none ms-2'><img src='../../img/icon/edit-icon.svg' Class='chkEvaluatorone' onclick='showInfoPopup(decodeURIComponent(" + ChrW(&H22) + left + ChrW(&H22) + ")" + "," + output.multi_pw_data(index).NodeID_Left.ToString() + ",2,2,1,null," + ChrW(&H22) + "left-text-node" + ChrW(&H22) + "," + ChrW(&H22) + output.multi_pw_data(index).LeftNode + ChrW(&H22) + "," + index.ToString() + ", decodeURIComponent(" + ChrW(&H22) + parentnode + ChrW(&H22) + ")" + ")' aria-hidden='true'></a>")

            'End If
            html.Append("</div>")
            'html.Append("<a href='javascript:void(0);' class='chkEvaluatorone'><img src='../../img/icon/edit-icon.svg' Class='icon' onclick='showInfoPopup(decodeURIComponent(" + ChrW(&H22) + left + ChrW(&H22) + ")" + "," + output.multi_pw_data(index).NodeID_Left.ToString() + ",2,2,1,null," + ChrW(&H22) + "left-text-node" + ChrW(&H22) + "," + ChrW(&H22) + output.multi_pw_data(index).LeftNode + ChrW(&H22) + "," + index.ToString() + ", decodeURIComponent(" + ChrW(&H22) + parentnode + ChrW(&H22) + ")" + ")' aria-hidden='true'></a>")

            html.Append("<a href='javascript:void(0);'  data-bs-toggle='modal' data-bs-target='#exampleModal' Class='d-md-none'><img src='../../img/icon/info-close.svg' Class='icon Bhawan' onclick='SetExpendedPopupElement(decodeURIComponent(" + ChrW(&H22) + left + ChrW(&H22) + ")" + "," + output.multi_pw_data(index).NodeID_Left.ToString() + ",2,2,1,null," + ChrW(&H22) + "left-text-node" + ChrW(&H22) + "," + ChrW(&H22) + output.multi_pw_data(index).LeftNode + ChrW(&H22) + "," + index.ToString() + ", decodeURIComponent(" + ChrW(&H22) + parentnode + ChrW(&H22) + ")," + "decodeURIComponent(" + ChrW(&H22) + leftWRT + ChrW(&H22) + ")," + output.multi_pw_data(index).NodeID_Left.ToString() + ",3,2,1,null," + ChrW(&H22) + "wrt-text-node" + ChrW(&H22) + "," + ChrW(&H22) + output.multi_pw_data(index).LeftNode + ChrW(&H22) + "," + index.ToString() + ",decodeURIComponent(" + ChrW(&H22) + parentnode + ChrW(&H22) + "))'></a>")
            html.Append("</div>")
            'If output.multi_pw_data(index).InfodocLeft.ToString().Trim() <> "" Or output.multi_pw_data(index).InfodocLeftWRT.ToString().Trim() <> "" Then
            If output.multi_pw_data(index).InfodocLeft.ToString().Trim() = "" Then
                html.Append("<div Class='body_content NoInfoData pwnodata'><div class='multiinfodata'><h2>No Data</h2></div>")
            Else
                html.Append($"<div Class='body_content'><span>{output.multi_pw_data(index).InfodocLeft.ToString().Replace("<b>", "").Replace("</b>", "").Trim()}</span>")
            End If
            html.Append("</div>")
            'html.Append("<div Class='info_datafooter'>")
            If output.multi_pw_data(index).InfodocLeftWRT.ToString().Trim() = "" Then
                html.Append($"<div Class='wrt_content NoInfoData'><div class='body_content'><h2>WITH RESPECT TO <span>{output.parent_node.ToString().Trim()}</span></h2><div Class='multiinfodata'><h2>No Data</h2></div>")
            Else
                html.Append($"<div Class='wrt_content'><div class='body_content'><h2>WITH RESPECT TO <span>{output.parent_node.ToString().Trim()}</span></h2><span>{output.multi_pw_data(index).InfodocLeftWRT.ToString().Replace("<b>", "").Replace("</b>", "").Trim()}</span>")
            End If
            html.Append("</div></div>")
            'End If

            Dim right As String = HttpUtility.UrlEncode(output.multi_pw_data(index).InfodocRight.ToString().Replace("<b>", "").Replace("</b>", "").Trim().Replace(ChrW(&H22), ChrW(&H27)))
            Dim rightWRT As String = HttpUtility.UrlEncode(output.multi_pw_data(index).InfodocRightWRT.ToString().Replace("<b>", "").Replace("</b>", "").Trim().Replace(ChrW(&H22), ChrW(&H27)))


            html.Append($"<div class='info_tooltip hideshowinfo parentnode102{index} {If(output.multi_pw_data(index).InfodocLeft = "", " divchkEvaluator", "")}' style='display:none;' id='il_" + index.ToString() + "'><div class='tooltop_head'><span>" + output.multi_pw_data(index).LeftNode + "</span><div class='action_icons'>")

            html.Append($"<a{If(output.isPipeViewOnly, " class='d-none'", "")} href='javascript:void(0);'><i class='fas fa-edit chkEvaluator d-none' onclick='showInfoPopup(decodeURIComponent(" + ChrW(&H22) + left + ChrW(&H22) + ")" + "," + output.multi_pw_data(index).NodeID_Left.ToString() + ",2,2,1,null," + ChrW(&H22) + "left-text-node" + ChrW(&H22) + "," + ChrW(&H22) + output.multi_pw_data(index).LeftNode + ChrW(&H22) + "," + index.ToString() + ", decodeURIComponent(" + ChrW(&H22) + parentnode + ChrW(&H22) + ")" + ")' aria-hidden='true'></i></a>")

            html.Append("<a href='javascript:void(0);'><i class='fa fa-times' onclick='hideWrapper(il_" + index.ToString() + ")' aria-hidden='true'></i></a></div></div><p>" + output.multi_pw_data(index).InfodocLeft.ToString().Replace("<b>", "").Replace("</b>", "").Trim().Replace(ChrW(&H22), ChrW(&H27)) + "</p></div></div>")

            'html.Append("<div Class='box_title' style='cursor: pointer;' onclick='activeRow(" + index.ToString() + ");'><p>" + output.multi_pw_data(index).LeftNode + "</p></div><div class='info_div'>")


            'html.Append($"<div onclick='showWrapper(ilr_{index.ToString()})'><span class='wrt_text {If((leftWRT) <> "" And (leftWRT) <> "NaN", "", "wwinfoColor")} {If((leftWRT) <> "" And (leftWRT) <> "NaN", "", "chkEvaluator")}' > wrt </span></div>")
            'html.Append($"<div class='info_tooltip hideshowinfo right_tooltip{If(output.multi_pw_data(index).InfodocLeftWRT = "", " divchkEvaluator", "")}' style='display:none;' id='ilr_" + index.ToString() + "'><div class='tooltop_head'><span>" + output.multi_pw_data(index).LeftNode + " WRT " + output.parent_node + "</span><div class='action_icons'>")
            'html.Append($"<a{If(output.isPipeViewOnly, " class='d-none'", "")} href='javascript:void(0);'><i class='fas fa-edit chkEvaluator d-none' onclick='showInfoPopup(decodeURIComponent(" + ChrW(&H22) + leftWRT + ChrW(&H22) + ")" + "," + output.multi_pw_data(index).NodeID_Left.ToString() + ",3,2,1,null," + ChrW(&H22) + "wrt-text-node" + ChrW(&H22) + "," + ChrW(&H22) + output.multi_pw_data(index).LeftNode + ChrW(&H22) + "," + index.ToString() + ", decodeURIComponent(" + ChrW(&H22) + parentnode + ChrW(&H22) + ")" + ")' aria-hidden='true'></i></a>")
            'html.Append("<a href='javascript:void(0);'><i class='fa fa-times' onclick='hideWrapper(ilr_" + index.ToString() + ")' aria-hidden='true'></i></a></div></div><p>" + output.multi_pw_data(index).InfodocLeftWRT.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)) + "</p></div>")




            html.Append("</div>")
            'End If
            If output.pairwise_type = "ptVerbal" Then
                html.Append("<div class='rating_area'>")
                'html.Append($"<div class='comment_div' {CommentDivStyle} readonly id='comdiv{index}'>")
                'html.Append("<div style='cursor:pointer;' onclick='ShowCommentBox(" + index.ToString() + ")'>")
                'If (output.multi_pw_data(index).Comment Is Nothing Or output.multi_pw_data(index).Comment = "") Then
                '    'html.Append("<i class='far fa-comments' aria-hidden='true'></i></div>")
                '    html.Append($"<img src = '../../img/icon/Grey-plus-icon.svg' id='multi_comment_iconEmpty_{index}' title='' aria-hidden='true'>  <img src = '../../img/icon/comment-svgrepo-com.svg' style='display:none;' id='multi_comment_icon_{index}' title='' aria-hidden='true'><span style='color: #333333'>Add Comment</span></div>")
                'Else
                '    'html.Append("<i Class='fa fa-comments' title='" + output.multi_pw_data(index).Comment + "' aria-hidden='true'></i></div>")
                '    html.Append($"<img src = '../../img/icon/Grey-plus-icon.svg' style='display:none;'id='multi_comment_iconEmpty_{index}' title='' aria-hidden='true'><img src = '../../img/icon/comment-svgrepo-com.svg'  id='multi_comment_icon_{index}' title='' aria-hidden='true'><span style='color: #333333'>Add Comment</span></div>")
                'End If
                'If (index = 0) Then
                '    html.Append($"<div class='info_tooltip right_tooltip hideshowinfo'  id='toggleComment_{index}' style='display:none;' ><div class='d-flex justify-content-between'><span style='color: #333333'>Add your comment</span>")
                'Else
                '    html.Append($"<div class='info_tooltip right_tooltip hideshowinfo'  id='toggleComment_{index}' style='display:none;' ><div class='d-flex justify-content-between'><span style='color: #333333'>Add your comment</span>")
                'End If
                ''html.Append($"<div class='info_tooltip'  style='display:none;' id='divComment_{index}'>abc</div>")onmouseover='showComment({index})'
                'html.Append($"<div class='action_icons' ><a href='javascript:void(0);' onclick='toggleBox({index})'><i class='fa fa-times' aria-hidden='true'></i></a></div></div>")
                'html.Append($"<textarea{If(output.isPipeViewOnly, " style='pointer-events:none;'", "")} id='comment_{index}' placeholder='Add your comment' class='form-control mb-3 mt-2 w-100' rows='3'>" + output.multi_pw_data(index).Comment + "</textarea><div class='comt_btn text-end'>")
                'html.Append("<button type='button' class='py-1 px-2' onclick='save_multi_pairwise(" + ChrW(&H22) + "comment" + ChrW(&H22) + "," + index.ToString() + ",0,1," + index.ToString() + "," + output.current_step.ToString() + ")'> <i class='fa fa-check' aria-hidden='true'></i> OK</button>")
                'html.Append("</div></div>")
                'html.Append("</div>")
                ''End If

                html.Append($"<div class='chart_area'{If(output.isPipeViewOnly, " style='pointer-events:none;'", "")}>")
                Dim value As Int32 = 0
                Try
                    value = Convert.ToInt32(Math.Abs(Math.Floor(output.multi_pw_data(index).Value)))
                Catch ex As Exception
                    value = 0
                End Try
                If (value = 1) Then
                    Dim abc = 1
                End If
                If active_multi_index <> index Or output.collapse_bars Then
                    html.Append("<div class='rating_result'>")
                    'If (output.multi_pw_data(index).Advantage = 1 And value >= 2) Then
                    '    html.Append($"<div style='display:block;' class='left_result left_result_{index}'><span id='left_result_{index}'>" + bars_right(value - 2)(1) + "</span></div>")
                    'Else
                    '    html.Append($"<div style='display:none;' class='left_result left_result_{index}'><span id='left_result_{index}'></span></div>")
                    'End If

                    Dim innerIndex = (bars_left.Length) - value
                    'If (output.multi_pw_data(index).Advantage = -1) Then
                    '    If (innerIndex < 0) Then
                    '        innerIndex = -1 * innerIndex
                    '    End If
                    '    Try
                    '        html.Append($"<div style='display:block;'  class='right_result right_result_{index}'><span id='right_result_{index}'>" + bars_left(Convert.ToInt32(innerIndex + 1))(1) + "</span></div>")
                    '    Catch ex As Exception
                    '        html.Append($"<div style='display:block;'  class='right_result right_result_{index}'><span id='right_result_{index}'>" + If(bars_left.Length >= innerIndex, bars_left(Convert.ToInt32(innerIndex))(1), bars_left(bars_left.Length - 1)(1)) + "</span></div>")
                    '    End Try
                    'Else
                    '    html.Append($"<div style='display:none;'  class='right_result right_result_{index}'><span id='right_result_{index}'></span></div>")
                    'End If
                    'If (value = 1 And output.multi_pw_data(index).Advantage = 0) Then
                    '    html.Append($"<div class='equal equal_{index}'><span>Equal</span></div>")
                    'Else
                    '    html.Append($"<div style='display:none;' class='equal equal_{index}'><span>Equal</span></div>")
                    'End If

                    html.Append("</div>")
                End If

                html.Append($"<div class='ratting_box' id='rating_box_{index}'>")
                Dim currentRating = ""
                Dim currentIndex = 0

                For leftindex = 0 To bars_left.Length - 1
                    If bars_left(leftindex)(3) <> " " Then
                        currentRating = bars_left(leftindex)(3)
                        currentIndex = Convert.ToInt32(bars_left(leftindex)(0))
                        'data.Value >= bar[0]
                        '
                        html.Append("<div id='show_" + index.ToString() + "_" + (bars_left.Length - leftindex).ToString() + "' title='" + bars_left(leftindex)(1) + "' onclick='save_multi_pairwise(" + ChrW(&H22) + "left" + ChrW(&H22) + "," + leftindex.ToString() + "," + (bars_left.Length - leftindex).ToString() + ",1," + index.ToString() + "," + output.current_step.ToString() + ")' class='rating_" + bars_left(leftindex)(3) + " rating_content rating_content_left " + If(output.multi_pw_data(index).Advantage = 1 And value >= currentIndex, " selected_left", "") + If(output.multi_pw_data(index).Advantage = 1 And value = currentIndex, " selcted_spot", "") + "'>")
                        If (output.multi_pw_data(index).Advantage = 1 And value = currentIndex) Then
                            html.Append($"<div class='multiverbal_spotname left_verbal left_name_{index} clsname_{index}' id='spotL" + index.ToString() + "_" + (bars_left.Length - leftindex).ToString() + "' >" + bars_right(value - 2)(1) + "</div> <div class='arrow bounce'></div>")

                        Else
                            html.Append($"<div class='multiverbal_spotname left_verbal left_name_{index} hide clsname_{index}' id='spotL" + index.ToString() + "_" + (bars_left.Length - leftindex).ToString() + "' >" + bars_right(bars_left.Length - (leftindex + 1))(1) + "</div> <div class='arrow bounce'></div>")
                        End If
                        html.Append("<span>" + bars_left(leftindex)(3) + "</span></div>")
                    Else
                        currentIndex = Convert.ToInt32(bars_left(leftindex)(0))
                        html.Append("<div id='show_" + index.ToString() + "_" + (bars_left.Length - leftindex).ToString() + "' title='" + bars_left(leftindex)(1) + "' onclick='save_multi_pairwise(" + ChrW(&H22) + "left" + ChrW(&H22) + "," + leftindex.ToString() + "," + (bars_left.Length - leftindex).ToString() + ",1," + index.ToString() + "," + output.current_step.ToString() + ")' ")
                        html.Append(" Class='rating_" + currentRating + "_s rating_content rating_content_left " + If(output.multi_pw_data(index).Advantage = 1 And value >= currentIndex, " selected_left", "") + If(output.multi_pw_data(index).Advantage = 1 And value = currentIndex, " selcted_spot", "") + "'>")
                        If (output.multi_pw_data(index).Advantage = 1 And value = currentIndex) Then
                            html.Append($"<div class='multiverbal_spotname left_verbal left_name_{index} clsname_{index}' id='spotL" + index.ToString() + "_" + (bars_left.Length - leftindex).ToString() + "' >" + bars_right(value - 2)(1) + "</div><div class='arrow bounce'></div>")
                        Else
                            html.Append($"<div class='multiverbal_spotname left_verbal left_name_{index} hide clsname_{index}' id='spotL" + index.ToString() + "_" + (bars_left.Length - leftindex).ToString() + "'>" + bars_right(bars_left.Length - (leftindex + 1))(1) + "</div><div class='arrow bounce'></div>")
                        End If
                        html.Append("</div>")
                    End If
                Next
                If value >= 1 And output.multi_pw_data(index).Advantage = 1 Then
                    html.Append($"<div id='divRatingEQ_e_{index}' onclick='save_multi_pairwise(" + ChrW(&H22) + "equal" + ChrW(&H22) + "," + index.ToString() + ",1,0," + index.ToString() + "," + output.current_step.ToString() + ")' class='rating_e rating_content rating_content_left selected_left '>")
                    html.Append($"<div Class='multiverbal_spotname equal_{index} hide clsname_{index}'>Equal</div><span>EQ</span><div class='arrow bounce'></div></div>")
                ElseIf value >= 1 And output.multi_pw_data(index).Advantage = -1 Then
                    html.Append($"<div id='divRatingEQ_e_{index}' onclick='save_multi_pairwise(" + ChrW(&H22) + "equal" + ChrW(&H22) + "," + index.ToString() + ",1,0," + index.ToString() + "," + output.current_step.ToString() + ")' class='rating_e rating_content rating_content_right selected_right '>")
                    html.Append($"<div Class='multiverbal_spotname equal_{index} hide clsname_{index}'>Equal</div><span>EQ</span><div class='arrow bounce'></div></div>")
                ElseIf value = 1 And output.multi_pw_data(index).Advantage = 0 Then
                    html.Append($"<div id='divRatingEQ_e_{index}' onclick='save_multi_pairwise(" + ChrW(&H22) + "equal" + ChrW(&H22) + "," + index.ToString() + ",1,0," + index.ToString() + "," + output.current_step.ToString() + ")' class='rating_e eqalizer_color rating_content rating_content_right selcted_spot'>")
                    html.Append($"<div Class='multiverbal_spotname equal_{index} clsname_{index}'>Equal</div><span>EQ</span><div class='arrow bounce'></div></div>")
                Else
                    html.Append($"<div id='divRatingEQ_e_{index}' onclick='save_multi_pairwise(" + ChrW(&H22) + "equal" + ChrW(&H22) + "," + index.ToString() + ",1,0," + index.ToString() + "," + output.current_step.ToString() + ")' class='rating_e rating_content rating_content_right rating_content_left'>")
                    html.Append($"<div Class='multiverbal_spotname equal_{index} hide clsname_{index}'>Equal</div><span>EQ</span><div class='arrow bounce'></div></div>")
                End If
                For rightindex = 0 To bars_right.Length - 1
                    If bars_right(rightindex)(3) <> " " Then
                        currentIndex = Convert.ToInt32(bars_right(rightindex)(0))
                        html.Append("<div id='showr_" + index.ToString() + "_" + rightindex.ToString() + "' title='" + bars_right(rightindex)(1) + "' onclick='save_multi_pairwise(" + ChrW(&H22) + "right" + ChrW(&H22) + "," + rightindex.ToString() + "," + rightindex.ToString() + ",-1," + index.ToString() + "," + output.current_step.ToString() + ")' ")
                        html.Append("Class='rating_" + bars_right(rightindex)(3) + " rating_content rating_content_right " + If(output.multi_pw_data(index).Advantage = -1 And value >= currentIndex, "selected_right", "") + If(output.multi_pw_data(index).Advantage = -1 And value = currentIndex, " selcted_spot", "") + "'><span>" + bars_right(rightindex)(3) + "</span>")
                        If (output.multi_pw_data(index).Advantage = -1 And value = currentIndex) Then
                            html.Append($"<div class='multiverbal_spotname right_name_{index} clsname_{index}' id='spotR" + index.ToString() + "_" + (rightindex).ToString() + "'>" + bars_right(value - 2)(1) + "</div><div class='arrow bounce arrowgreen'></div>")
                        Else
                            html.Append($"<div class='multiverbal_spotname right_name_{index} hide clsname_{index}' id='spotR" + index.ToString() + "_" + (rightindex).ToString() + "'>" + bars_left(bars_right.Length - rightindex - 1)(1) + "</div><div class='arrow bounce arrowgreen'></div>")
                        End If
                        html.Append("</div>")
                    Else
                        currentRating = bars_right(rightindex + 1)(3)
                        currentIndex = Convert.ToInt32(bars_right(rightindex)(0))
                        html.Append("<div id='showr_" + index.ToString() + "_" + rightindex.ToString() + "' title='" + bars_right(rightindex)(1) + "' onclick='save_multi_pairwise(" + ChrW(&H22) + "right" + ChrW(&H22) + "," + rightindex.ToString() + "," + rightindex.ToString() + ",-1," + index.ToString() + "," + output.current_step.ToString() + ")' class='rating_" + currentRating + "_s rating_content rating_content_right " + If(output.multi_pw_data(index).Advantage = -1 And value >= currentIndex, "selected_right", "") + If(output.multi_pw_data(index).Advantage = -1 And value = currentIndex, " selcted_spot", "") + "'>")
                        If (output.multi_pw_data(index).Advantage = -1 And value = currentIndex) Then
                            html.Append($"<div class='multiverbal_spotname right_name_{index} clsname_{index}' id='spotR" + index.ToString() + "_" + (rightindex).ToString() + "'>" + bars_right(value - 2)(1) + "</div><div class='arrow bounce arrowgreen'></div>")
                        Else
                            html.Append($"<div class='multiverbal_spotname right_name_{index} hide clsname_{index}' id='spotR" + index.ToString() + "_" + (rightindex).ToString() + "'>" + bars_left(bars_right.Length - rightindex - 1)(1) + "</div><div class='arrow bounce arrowgreen'></div>")
                        End If
                        html.Append("</div>")
                    End If
                Next
                html.Append("</div></div>")
                html.Append("<div class='pairwise_newdesign'>")
                html.Append($"<div class='comment_div' {CommentDivStyle} readonly id='comdiv{index}'>")
                If Not (screenCheck.isMobileBrowserClient(Request)) Then
                    html.Append("<div style='cursor:pointer;' onclick='ShowCommentBox(" + index.ToString() + ")'>")
                Else
                    html.Append("<div style='cursor:pointer;' onclick='ShowCommentBoxMobile(" + index.ToString() + ")'>")
                End If
                If (output.multi_pw_data(index).Comment Is Nothing Or output.multi_pw_data(index).Comment = "") Then
                        'html.Append("<i class='far fa-comments' aria-hidden='true'></i></div>")
                        If Not (screenCheck.isMobileBrowserClient(Request)) Then
                            html.Append($"<img src = '../../img/icon/Grey-plus-icon.svg' id='multi_comment_iconEmpty_{index}' title='' aria-hidden='true'>  <img src = '../../img/icon/comment-svgrepo-com.svg' style='display:none;' id='multi_comment_icon_{index}' title='' aria-hidden='true'><span style='color: #333333'  class='tooltip_item'>Add Comment</span></div>")
                        Else
                            html.Append($"<img src = '../../img/icon/Grey-plus-icon.svg' id='multi_comment_iconEmpty_mobile_{index}' title='' aria-hidden='true'>  <img src = '../../img/icon/comment-svgrepo-com.svg' style='display:none;' id='multi_comment_icon_{index}' title='' aria-hidden='true'><span style='color: #333333'  class='tooltip_item'>Add Comment</span></div>")
                        End If
                    Else
                        If Not (screenCheck.isMobileBrowserClient(Request)) Then
                            html.Append($"<img src = '../../img/icon/Grey-plus-icon.svg' style='display:none;'id='multi_comment_iconEmpty_{index}' title='' aria-hidden='true'><img src = '../../img/icon/comment-svgrepo-com.svg'  id='multi_comment_icon_{index}' title='' aria-hidden='true'><span style='color: #333333'  class='tooltip_item'>Add Comment</span></div>")
                        Else
                            html.Append($"<img src = '../../img/icon/Grey-plus-icon.svg' style='display:none;'id='multi_comment_iconEmpty_mobile_{index}' title='' aria-hidden='true'><img src = '../../img/icon/comment-svgrepo-com.svg'  id='multi_comment_icon_{index}' title='' aria-hidden='true'><span style='color: #333333'  class='tooltip_item'>Add Comment</span></div>")
                        End If
                    End If
                    If (index = 0) Then
                        html.Append($"<div class='info_tooltip right_tooltip hideshowinfo'  id='toggleComment_{index}' style='display:none;' ><div class='d-flex justify-content-between'><span style='color: #333333'>Add your comment</span>")
                    Else
                        html.Append($"<div class='info_tooltip right_tooltip hideshowinfo'  id='toggleComment_{index}' style='display:none;' ><div class='d-flex justify-content-between'><span style='color: #333333'>Add your comment</span>")
                    End If
                    'html.Append($"<div class='info_tooltip'  style='display:none;' id='divComment_{index}'>abc</div>")onmouseover='showComment({index})'
                    html.Append($"<div class='action_icons' ><a href='javascript:void(0);' onclick='toggleBox({index})'><i class='fa fa-times' aria-hidden='true'></i></a></div></div>")
                    html.Append($"<textarea{If(output.isPipeViewOnly, " style='pointer-events:none;'", "")} id='comment_{index}' placeholder='Add your comment' class='form-control mb-3 mt-2 w-100' rows='3'>" + output.multi_pw_data(index).Comment + "</textarea><div class='comt_btn text-end'>")
                    html.Append("<button type='button' class='py-1 px-2' onclick='save_multi_pairwise(" + ChrW(&H22) + "comment" + ChrW(&H22) + "," + index.ToString() + ",0,1," + index.ToString() + "," + output.current_step.ToString() + ")'> <i class='fa fa-check' aria-hidden='true'></i> OK</button>")
                    html.Append("</div></div>")
                    html.Append("</div>")
                    'End If
                    html.Append($"<div{If(output.isPipeViewOnly, " style='pointer-events:none;'", "")} class='close_icon{ClearJudgmentclose_iconDiv}' style='cursor:pointer;' onclick='save_multi_pairwise(" + ChrW(&H22) + "close" + ChrW(&H22) + "," + index.ToString() + "," + index.ToString() + ",-1," + index.ToString() + "," + output.current_step.ToString() + ")' ><img src='../../img/icon/erasar.svg' class='imgClose'><span  class='tooltip_item'>Clear Judgment</span></div>")
                    html.Append("</div>")
                    'html.Append($"<div{If(output.isPipeViewOnly, " style='pointer-events:none;'", "")} class='close_icon{ClearJudgmentclose_iconDiv}' style='cursor:pointer;' onclick='save_multi_pairwise(" + ChrW(&H22) + "close" + ChrW(&H22) + "," + index.ToString() + "," + index.ToString() + ",-1," + index.ToString() + "," + output.current_step.ToString() + ")' ><span><img src='../../img/icon/erasar.svg'></span><span>Clear Judgment</span></div>")
                    'html.Append("</div>")
                    html.Append("</div>")

                End If

                Dim participant_slider = output.multi_pw_data

            If output.pairwise_type = "ptGraphical" Then
                html.Append($"<div class='rating_area'>")
                'html.Append("<div class='scale_wt'>")
                html.Append("<div class='chart_data graphical_data mt-3 mb-0'>")
                'html.Append("<div Class='comment_div px-2' >")
                'html.Append($"<div Class='comment_icon'{CommentDivStyle}>")
                'If (output.multi_pw_data(index).Comment Is Nothing Or output.multi_pw_data(index).Comment = "") Then
                '    'html.Append($"<i class='far fa-comments' id='multi_comment_icon_{index}' onclick='showNode(3{index},{index})'  aria-hidden='true'></i>")
                '    html.Append($"<a href='javascript:void(0)' onclick='showNode(3{index},{index})'><img src = '../../img/icon/Grey-plus-icon.svg' id='multi_comment_iconEmpty_{index}' title='' aria-hidden='true'>  <img src = '../../img/icon/comment-svgrepo-com.svg' style='display:none;' id='multi_comment_icon_{index}' title='' aria-hidden='true'><span style='color: #333333'> Add Comment</span></a>")
                'Else
                '    'html.Append($"<i Class='fa fa-comments' id='multi_comment_icon_{index}' onclick='showNode(3{index},{index})'  title='" + output.multi_pw_data(index).Comment + "' aria-hidden='true'></i>")
                '    html.Append($"<a href='javascript:void(0)' onclick='showNode(3{index},{index})'><img src = '../../img/icon/Grey-plus-icon.svg' style='display:none;' id='multi_comment_iconEmpty_{index}' title='' aria-hidden='true'><img src = '../../img/icon/comment-svgrepo-com.svg' id='multi_comment_icon_{index}' title='" + output.multi_pw_data(index).Comment + "'  aria-hidden='true'><span style='color: #333333'> Add Comment</span></a>")
                'End If
                ''html.Append($"<img src = '../../img/icon/commentadd.svg' id='multi_comment_icon_{index}' onclick='showNode(3{index},{index})'  aria-hidden='true'><span> Add Comment</span>")
                'If (index = 0) Then
                '    html.Append($"<div class='info_tooltip right_tooltip tooltip_wrapper hideshowinfo'  id='parentnode3{index}' style='display:none;'>")
                'Else
                '    html.Append($"<div class='info_tooltip right_tooltip tooltip_wrapper hideshowinfo'  id='parentnode3{index}' style='display:none;'>")
                'End If

                ''Dim right As String = HttpUtility.UrlEncode(output.multi_pw_data(index).InfodocRight.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
                ''Dim rightWRT As String = HttpUtility.UrlEncode(output.multi_pw_data(index).InfodocRightWRT.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))

                'html.Append("<div class='d-flex justify-content-between'>")
                'html.Append("<span>Add your comment</span>")
                'html.Append($"<div class='action_icons'><a href='javascript:void(0);' onclick='hideNode(3{index})'><i class='fa fa-times' ></i></a></div>")
                'html.Append("</div>")
                'html.Append($"<textarea{If(output.isPipeViewOnly, " style='pointer-events:none;'", "")} class='form-control mb-3 mt-2 w-100' rows='3' id='multi_comment_{index}' placeholder='Add your comment'>{output.multi_pw_data(index).Comment}</textarea>")
                'html.Append("<div class='comt_btn text-end'>")
                'html.Append($"<button type='button' class='py-1 px-2' onclick='hideNode(3{index},{index})'> <i class='fa fa-check' aria-hidden='true'></i> OK</button>")
                ''html.Append($"<button class='py-1 px-2' onclick='save_multi_pairwise(" + ChrW(&H22) + "comment" + ChrW(&H22) + "," + index.ToString() + ",0,1," + index.ToString() + "," + output.current_step.ToString() + ")'> <i class='fa fa-check' aria-hidden='true'></i> OK</button>")
                'html.Append("</div>")
                'html.Append("</div>")
                ''html.Append("</div>")

                'html.Append("</div>")
                'html.Append("<div Class='info_tooltip right_tooltip' style='transform: scaleY(0);'>")
                'html.Append("<div Class='d-flex justify-content-between'>")
                'html.Append("<span> Add your comment</span>")
                'html.Append("<div Class='action_icons'>")
                'html.Append("<a href = '#' >")
                'html.Append("<i class='fa fa-times font-size-16 text-danger' aria-hidden='true'></i>")
                'html.Append("</a>")
                'html.Append("</div>")
                'html.Append("</div>")
                'html.Append("<textarea Class='form-control mb-3 mt-2 w-100' rows='3'></textarea>")
                'html.Append("<div Class='comt_btn text-end'>")
                'html.Append("<Button> <i Class='fa fa-check' aria-hidden='true'></i> OK</button>")
                'html.Append("</div>")
                'html.Append("</div>")
                'html.Append("</div>")

                html.Append("<div Class='curvechart_input'>")
                Dim fun As String = $"onkeyup=graphicalall_key_up(this,'0','-1','up',{index},'false')"
                'fun += "onclick=slideScroll()"
                'Dim fun As String = "onkeyup=graphical_key_up(this,'0','-1','up',null,'false')"
                'fun += " onkeydown=graphical_key_up(this,'0','-1','press',null,'false')"
                'fun += " onmousedown=graphical_key_up(this,'0','-1','press',null,'false')"
                'fun += " onmouseup=graphical_key_up(this,'0','-1','up',null,'false')"
                'fun += " onblur=graphical_key_up(this,'0','-1','up',null,'false')"
                'fun += " onkeypress='return isNumberKeyWithDecimal(this, event);'"


                'onkeypress='checkEnter(this)'
                If output.multi_pw_data(index).Advantage < 0 Then
                    html.Append($"<input type='text' value='{Math.Round(Convert.ToDecimal(output.multi_pw_data(index).Value), 2)}' id='input1{index}' class='value_control checkEnter' {fun}>")
                Else
                    html.Append($"<input type='text' value='1' id='input1{index}' class='value_control checkEnter' {fun}>")
                End If
                html.Append("<div class='arrowicons'>")
                html.Append($"<a href='javascript:void(0);' onclick=swap_value('{index}'); id='reverse_icon_{index}'>")
                html.Append("<img src='../../img/icon/value_exchange.svg'>")
                html.Append("</a>")
                html.Append("</div>")
                If output.multi_pw_data(index).Advantage > 0 Then
                    html.Append($"<input type='text'  value='{Math.Round(Convert.ToDecimal(output.multi_pw_data(index).Value), 2)}' id='input2{index}' class='value_control checkEnter' {fun}>")
                Else
                    html.Append($"<input type='text' value='1' id='input2{index}' class='value_control checkEnter' {fun}>")
                End If
                'html.Append("<input type = 'number' >")

                'html.Append("<input type = 'number' >")
                html.Append("</div>")
                html.Append("</div>")
                html.Append($"<div class='form-group w-100 mt-3 mb-5 mb-md-4'{If(output.isPipeViewOnly, " style='pointer-events:none;'", "")}>")

                html.Append($"<div id='gslider{index}'></div>")
                html.Append("</div>")



                html.Append("<div class='pairwise_newdesign'>")
                html.Append("<div Class='comment_div' >")
                html.Append($"<div Class='comment_icon'{CommentDivStyle}>")
                If (output.multi_pw_data(index).Comment Is Nothing Or output.multi_pw_data(index).Comment = "") Then
                    'html.Append($"<i class='far fa-comments' id='multi_comment_icon_{index}' onclick='showNode(3{index},{index})'  aria-hidden='true'></i>")
                    If Not (screenCheck.isMobileBrowserClient(Request)) Then
                        html.Append($"<a href='javascript:void(0)' onclick='showNode(3{index},{index})'><img src = '../../img/icon/Grey-plus-icon.svg' id='multi_comment_iconEmpty_{index}' title='' aria-hidden='true'>  <img src = '../../img/icon/comment-svgrepo-com.svg' style='display:none;' id='multi_comment_icon_{index}' title='' aria-hidden='true'><span style='color: #333333'  class='tooltip_item'> Add Comment</span></a>")
                    Else
                        html.Append($"<a href='javascript:void(0)' onclick='showNodeMobile(3{index},{index})'><img src = '../../img/icon/Grey-plus-icon.svg' id='multi_comment_iconEmpty_{index}' title='' aria-hidden='true'>  <img src = '../../img/icon/comment-svgrepo-com.svg' style='display:none;' id='multi_comment_icon_{index}' title='' aria-hidden='true'><span style='color: #333333'  class='tooltip_item'> Add Comment</span></a>")
                    End If
                Else
                    If Not (screenCheck.isMobileBrowserClient(Request)) Then
                        'html.Append($"<i Class='fa fa-comments' id='multi_comment_icon_{index}' onclick='showNode(3{index},{index})'  title='" + output.multi_pw_data(index).Comment + "' aria-hidden='true'></i>")
                        html.Append($"<a href='javascript:void(0)' onclick='showNode(3{index},{index})'><img src = '../../img/icon/Grey-plus-icon.svg' style='display:none;' id='multi_comment_iconEmpty_{index}' title='' aria-hidden='true'><img src = '../../img/icon/comment-svgrepo-com.svg' id='multi_comment_icon_{index}' title='" + output.multi_pw_data(index).Comment + "'  aria-hidden='true'><span style='color: #333333'  class='tooltip_item'> Add Comment</span></a>")
                    Else
                        html.Append($"<a href='javascript:void(0)' onclick='showNodeMobile(3{index},{index})'><img src = '../../img/icon/Grey-plus-icon.svg' style='display:none;' id='multi_comment_iconEmpty_{index}' title='' aria-hidden='true'><img src = '../../img/icon/comment-svgrepo-com.svg' id='multi_comment_icon_{index}' title='" + output.multi_pw_data(index).Comment + "'  aria-hidden='true'><span style='color: #333333'  class='tooltip_item'> Add Comment</span></a>")
                    End If
                End If
                    'html.Append($"<img src = '../../img/icon/commentadd.svg' id='multi_comment_icon_{index}' onclick='showNode(3{index},{index})'  aria-hidden='true'><span> Add Comment</span>")
                    If (index = 0) Then
                    html.Append($"<div class='info_tooltip right_tooltip tooltip_wrapper hideshowinfo'  id='parentnode3{index}' style='display:none;'>")
                Else
                    html.Append($"<div class='info_tooltip right_tooltip tooltip_wrapper hideshowinfo'  id='parentnode3{index}' style='display:none;'>")
                End If

                'Dim right As String = HttpUtility.UrlEncode(output.multi_pw_data(index).InfodocRight.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
                'Dim rightWRT As String = HttpUtility.UrlEncode(output.multi_pw_data(index).InfodocRightWRT.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))

                html.Append("<div class='d-flex justify-content-between'>")
                html.Append("<span>Add your comment</span>")
                html.Append($"<div class='action_icons'><a href='javascript:void(0);' onclick='hideNode(3{index})'><i class='fa fa-times' ></i></a></div>")
                html.Append("</div>")
                html.Append($"<textarea{If(output.isPipeViewOnly, " style='pointer-events:none;'", "")} class='form-control mb-3 mt-2 w-100' rows='3' id='multi_comment_{index}' placeholder='Add your comment'>{output.multi_pw_data(index).Comment}</textarea>")
                html.Append("<div class='comt_btn text-end'>")
                html.Append($"<button type='button' id='btnUpdateComment' class='py-1 px-2' onclick='hideNode(3{index},{index})'> <i class='fa fa-check' aria-hidden='true'></i> OK</button>")
                'html.Append($"<button class='py-1 px-2' onclick='save_multi_pairwise(" + ChrW(&H22) + "comment" + ChrW(&H22) + "," + index.ToString() + ",0,1," + index.ToString() + "," + output.current_step.ToString() + ")'> <i class='fa fa-check' aria-hidden='true'></i> OK</button>")
                html.Append("</div>")
                html.Append("</div>")
                'html.Append("</div>")

                html.Append("</div>")
                html.Append("<div Class='info_tooltip right_tooltip' style='transform: scaleY(0);'>")
                html.Append("<div Class='d-flex justify-content-between'>")
                html.Append("<span> Add your comment</span>")
                html.Append("<div Class='action_icons'>")
                html.Append("<a href = '#' >")
                html.Append("<i class='fa fa-times font-size-16 text-danger' aria-hidden='true'></i>")
                html.Append("</a>")
                html.Append("</div>")
                html.Append("</div>")
                html.Append("<textarea Class='form-control mb-3 mt-2 w-100' rows='3'></textarea>")
                html.Append("<div Class='comt_btn text-end'>")
                html.Append("<Button> <i Class='fa fa-check' aria-hidden='true'></i> OK</button>")
                html.Append("</div>")
                html.Append("</div>")
                html.Append("</div>")
                fun = $"onclick='add_multivalues(-2147483648000, 0, {index}, event); set_slider_blank({index}); clearInputOnMultiPWGraphical({index})'"
                html.Append($"<div Class='close_icon' {fun}>")
                html.Append("<img src = '../../img/icon/erasar.svg' class='imgClose'>")
                html.Append("<span  class='tooltip_item'>Clear Judgment</span>")
                html.Append("</div>")
                html.Append("</div>")

                html.Append("<div class='result_values d-none'>")

                'Comment box
                If output.show_comments Then
                    html.Append("<div style='cursor:pointer;'  class='comment_div'>")
                    'html.Append("<input type='checkbox' class='comment_model'>")
                    If (output.multi_pw_data(index).Comment Is Nothing Or output.multi_pw_data(index).Comment = "") Then
                        html.Append($"<i class='far fa-comments' id='multi_comment_icon_{index}' onclick='showNode(3{index},{index})'  aria-hidden='true'></i>")
                    Else
                        html.Append($"<i Class='fa fa-comments' id='multi_comment_icon_{index}' onclick='showNode(3{index},{index})'  title='" + output.multi_pw_data(index).Comment + "' aria-hidden='true'></i>")
                    End If
                    If (index = 0) Then
                        html.Append($"<div class='info_tooltip right_tooltip tooltip_wrapper hideshowinfo'  id='parentnode3{index}' style='display:none;'>")
                    Else
                        html.Append($"<div class='info_tooltip right_tooltip tooltip_wrapper hideshowinfo'  id='parentnode3{index}' style='display:none;'>")
                    End If
                    html.Append("<div class='d-flex justify-content-between'>")
                    html.Append("<span>Add your comment</span>")
                    html.Append($"<div class='action_icons' ><a href='javascript:void(0);' onclick='toggleBox()'><i class='fa fa-times' onclick='hideNode(3{index},{index})' aria-hidden='true'></i></a></div>")
                    html.Append("</div>")
                    html.Append($"<textarea{If(output.isPipeViewOnly, " style='pointer-events:none;'", "")} class='form-control mb-3 mt-2 w-100' rows='3' id='multi_comment_{index}' placeholder='Add your comment'>{output.multi_pw_data(index).Comment}</textarea>")
                    html.Append("<div class='comt_btn text-end'>")
                    html.Append($"<button type='button' class='py-1 px-2' onclick='hideNode(3{index},{index})'> <i class='fa fa-check' aria-hidden='true'></i> OK</button>")
                    'html.Append($"<button class='py-1 px-2' onclick='save_multi_pairwise(" + ChrW(&H22) + "comment" + ChrW(&H22) + "," + index.ToString() + ",0,1," + index.ToString() + "," + output.current_step.ToString() + ")'> <i class='fa fa-check' aria-hidden='true'></i> OK</button>")
                    html.Append("</div>")
                    html.Append("</div>")
                    html.Append("</div>")
                End If
                html.Append($"<div class='reverse_icon'{If(output.isPipeViewOnly, " style='pointer-events:none;'", "")}>")
                html.Append($"<a href='javascript:void(0);' onclick=swap_value('{index}'); id='reverse_icon_{index}'><img src='../../img/icon/value_exchange.svg' /></a>")
                html.Append("</div>")
                html.Append($"<div class='inputs_value'{If(output.isPipeViewOnly, " style='pointer-events:none;'", "")}>")

                'Dim fun As String = $"onkeyup=graphicalall_key_up(this,'0','-1','up',{index},'false')"
                'fun += "onclick=slideScroll()"
                'Dim fun As String = "onkeyup=graphical_key_up(this,'0','-1','up',null,'false')"
                'fun += " onkeydown=graphical_key_up(this,'0','-1','press',null,'false')"
                'fun += " onmousedown=graphical_key_up(this,'0','-1','press',null,'false')"
                'fun += " onmouseup=graphical_key_up(this,'0','-1','up',null,'false')"
                'fun += " onblur=graphical_key_up(this,'0','-1','up',null,'false')"
                'fun += " onkeypress='return isNumberKeyWithDecimal(this, event);'"
                'onkeypress='checkEnter(this)'
                If output.multi_pw_data(index).Advantage < 0 Then
                    html.Append($"<input type='text' value='{Math.Round(Convert.ToDecimal(output.multi_pw_data(index).Value), 2)}' id='input2{index}' class='value_control checkEnter' {fun}>")
                Else
                    html.Append($"<input type='text' value='1' id='input2{index}' class='value_control checkEnter' {fun}>")
                End If
                If output.multi_pw_data(index).Advantage > 0 Then
                    html.Append($"<input type='text'  value='{Math.Round(Convert.ToDecimal(output.multi_pw_data(index).Value), 2)}' id='input1{index}' class='value_control checkEnter' {fun}>")
                Else
                    html.Append($"<input type='text' value='1' id='input1{index}' class='value_control checkEnter' {fun}>")
                End If

                html.Append("</div>")
                html.Append("<div class='d-flex justify-content-center slide_valueComment '>")


                html.Append($"<div{If(output.isPipeViewOnly, " style='pointer-events:none;'", "")} class='close_icon ms-3 me-3' {fun}>")
                html.Append($"<i class='fa fa-times' aria-hidden='true' id='close_icon_{index}'></i>")
                html.Append("</div>")

                html.Append("</div>")
                html.Append("</div>")
                'html.Append("</div>")

                html.Append("</div>")
            End If

            If Not IsMobile Then
                NoInfoDocDataCls = ""
                filter_gray = ""
                m = Regex.Match(output.multi_pw_data(index).InfodocRight, "<img")
                If m.Success Then
                    imstyl = " image_main_wrapper"
                End If
                If output.multi_pw_data(index).InfodocRight <> "" Or output.multi_pw_data(index).InfodocRightWRT.ToString().Trim() <> "" Then
                    If output.multi_pw_data(index).InfodocRight = "" Or output.multi_pw_data(index).InfodocRightWRT.ToString().Trim() = "" Then
                        html.Append($"<div class='value_wrapper right_info nodataonesideinfodocrow{imstyl}'><div class='info_data selected'>")
                    Else
                        html.Append($"<div class='value_wrapper right_info{imstyl}'><div class='info_data selected'>")
                    End If
                Else
                    NoInfoDocDataCls = "NoInfoData"
                    filter_gray = " filter_gray"
                    html.Append($"<div class='value_wrapper right_info emptyInfodocbothside{imstyl}'><div class='info_data selected'>")
                End If
                'If (output.multi_pw_data(index).InfodocRight <> "") Then
                '    html.Append($"<div class='value_wrapper right_info'><div class='info_data selected'>")
                'Else
                '    html.Append($"<div class='value_wrapper right_info no_data'><div class='info_data selected'>")
                'End If
                html.Append("<div Class='info_header'>")
                Dim rightIndex As String = HttpUtility.UrlEncode(output.multi_pw_data(index).InfodocRight.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
                Dim rightWRTIndex As String = HttpUtility.UrlEncode(output.multi_pw_data(index).InfodocRightWRT.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
                html.Append($"<a class='{NoInfoDocDataCls }{filter_gray}'><img src='../../img/icon/info-close.svg' class='icon'></a>")
                html.Append($"<span Class='title_tag text-uppercase' title='{output.multi_pw_data(index).RightNode}'>{output.multi_pw_data(index).RightNode}</span>")
                html.Append($"<div class='header_edit_fw {NoInfoDocDataCls}'>")
                html.Append($"<a href='#' data-bs-toggle='modal' data-bs-target='#exampleModal' onclick='SetExpendedPopupElement(decodeURIComponent(" + ChrW(&H22) + output.multi_pw_data(index).RightNode + ChrW(&H22) + ")" + "," + output.multi_pw_data(index).NodeID_Right.ToString() + ",2,2,1,null," + ChrW(&H22) + "right-node" + ChrW(&H22) + ",decodeURIComponent(" + ChrW(&H22) + rightIndex + ChrW(&H22) + ")," + index.ToString() + ", decodeURIComponent(" + ChrW(&H22) + parentnode + ChrW(&H22) + ")," +
            "decodeURIComponent(" + ChrW(&H22) + rightWRT + ChrW(&H22) + ")," + output.multi_pw_data(index).NodeID_Right.ToString() + ",3,2,1,null," + ChrW(&H22) + "wrt-right-node" + ChrW(&H22) + "," + ChrW(&H22) + output.multi_pw_data(index).RightNode + ChrW(&H22) + "," + index.ToString() + ",decodeURIComponent(" + ChrW(&H22) + parentnode + ChrW(&H22) + "))'> <img src='../../img/icon/full-screen-svgrepo-com.svg '> </a>")

                'If Not (screenCheck.isMobileBrowserClient(Request)) Then
                '    html.Append("<a href='#' class='d-md-block d-none ms-2'><img src='../../img/icon/edit-icon.svg' class='chkEvaluatorone' onclick='showInfoPopup(decodeURIComponent(" + ChrW(&H22) + output.multi_pw_data(index).RightNode + ChrW(&H22) + ")," + output.multi_pw_data(index).NodeID_Right.ToString() + ",2,2,1,null," + ChrW(&H22) + "right-node" + ChrW(&H22) + ",decodeURIComponent(" + ChrW(&H22) + rightIndex + ChrW(&H22) + ")," + index.ToString() + ", decodeURIComponent(" + ChrW(&H22) + parentnode + ChrW(&H22) + ")" + ")'  aria-hidden='true'></a>")
                'End If
                html.Append("<a href = 'javascripr:void(0);'  data-bs-toggle='modal' data-bs-target='#exampleModal' Class='d-md-none' onclick='SetExpendedPopupElement(decodeURIComponent(" + ChrW(&H22) + output.multi_pw_data(index).RightNode + ChrW(&H22) + ")" + "," + output.multi_pw_data(index).NodeID_Right.ToString() + ",2,2,1,null," + ChrW(&H22) + "right-node" + ChrW(&H22) + ",decodeURIComponent(" + ChrW(&H22) + rightIndex + ChrW(&H22) + ")," + index.ToString() + ", decodeURIComponent(" + ChrW(&H22) + parentnode + ChrW(&H22) + ")," +
                "decodeURIComponent(" + ChrW(&H22) + rightWRT + ChrW(&H22) + ")," + output.multi_pw_data(index).NodeID_Right.ToString() + ",3,2,1,null," + ChrW(&H22) + "wrt-right-node" + ChrW(&H22) + "," + ChrW(&H22) + output.multi_pw_data(index).RightNode + ChrW(&H22) + "," + index.ToString() + ",decodeURIComponent(" + ChrW(&H22) + parentnode + ChrW(&H22) + "))'><img src='../../img/icon/info-close.svg' Class='icon'></a>")
                html.Append("</div>")
                html.Append("</div>")
                'If output.multi_pw_data(index).InfodocRight <> "" Or output.multi_pw_data(index).InfodocRightWRT.ToString().Trim() <> "" Then
                If output.multi_pw_data(index).InfodocRight.ToString().Trim() <> "" Then
                    html.Append($"<div Class='body_content'><span>{output.multi_pw_data(index).InfodocRight.ToString().Replace("<b>", "").Replace("</b>", "").Trim().Replace(ChrW(&H22), ChrW(&H27))}</span>")
                Else
                    html.Append($"<div Class='body_content NoInfoData pwnodata'><div Class='multiinfodata'><h2>No Data</h2> </div>")
                End If
                html.Append("</div>")
                html.Append($"")
                If output.multi_pw_data(index).InfodocRightWRT.ToString().Trim() <> "" Then
                    html.Append($"<div class='wrt_content'><div class='body_content'><h2>WITH RESPECT TO <span>{output.parent_node.ToString().Trim()}</span></h2><span>{output.multi_pw_data(index).InfodocRightWRT.ToString().Replace("<b>", "").Replace("</b>", "").Trim()}</span>")
                Else
                    html.Append($"<div class='wrt_content NoInfoData'><div class='body_content'><h2>WITH RESPECT TO <span>{output.parent_node.ToString().Trim()}</span></h2><div Class='multiinfodata'><h2>No Data</h2> </div>")
                End If
                html.Append("</div></div>")
                'End If
                'html.Append("<div Class='info_datafooter'>")
                'html.Append($"<button type='button' data-bs-toggle='modal' data-bs-target='#exampleModalWRTRight_{index}'>with respect to <img src='../../img/icon/button_arrow.svg'></button>")
                '    html.Append($"<button type='button' id='btnWRTRight_{index}' {If((rightWRT.ToString().Trim()) <> "", " class=''", " class='nowrt_data' ")} data-bs-toggle='modal' data-bs-target='#exampleModal'  onclick='SetExpendedPopupElement(decodeURIComponent(" + ChrW(&H22) + output.multi_pw_data(index).RightNode + ChrW(&H22) + ")" + "," + output.multi_pw_data(index).NodeID_Right.ToString() + ",2,2,1,null," + ChrW(&H22) + "right-node" + ChrW(&H22) + ",decodeURIComponent(" + ChrW(&H22) + rightIndex + ChrW(&H22) + ")," + index.ToString() + ", decodeURIComponent(" + ChrW(&H22) + parentnode + ChrW(&H22) + ")," +
                '"decodeURIComponent(" + ChrW(&H22) + rightWRT + ChrW(&H22) + ")," + output.multi_pw_data(index).NodeID_Right.ToString() + ",3,2,1,null," + ChrW(&H22) + "wrt-right-node" + ChrW(&H22) + "," + ChrW(&H22) + output.multi_pw_data(index).RightNode + ChrW(&H22) + "," + index.ToString() + ",decodeURIComponent(" + ChrW(&H22) + parentnode + ChrW(&H22) + "))'>with respect to <img src='../../img/icon/button_arrow.svg'></button>")
                'html.Append("</div>")


                'Dim rightWRTClick As String = "showInfoPopup(decodeURIComponent(" + ChrW(&H22) + rightWRT + ChrW(&H22) + ")" + "," + output.multi_pw_data(index).NodeID_Right.ToString() + ",3,2,1,null," + ChrW(&H22) + "wrt-right-node" + ChrW(&H22) + "," + ChrW(&H22) + output.multi_pw_data(index).RightNode + ChrW(&H22) + "," + index.ToString() + ", decodeURIComponent(" + ChrW(&H22) + parentnode + ChrW(&H22) + ")" + ")"

                'html.Append($"<div class='modal fade' id='exampleModalWRTRight_{index}' tabindex='-1' aria-labelledby='exampleModalLabel' aria-hidden='True'><div class='modal-dialog modal-fullscreen'><div class='modal-content'><div class='modal-header'><h3 class='modal-title'id='exampleModalLabel' style='color:white;'>INFODOCS</h3><button type='button' class='btn-close' data-bs-dismiss='modal' aria-label='Close'></button></div><div class='modal-body'><div class='row h-100'><div class='col-md-6 border-End'><div class='modal_info_header'><h3>{output.multi_pw_data(index).RightNode}</h3><a href='javascript:void(0);' class='chkEvaluatorone'><img src='../../img/icon/edit-icon.svg' onclick='showInfoPopup(decodeURIComponent(" + ChrW(&H22) + rightIndex + ChrW(&H22) + ")" + "," + output.multi_pw_data(index).NodeID_Right.ToString() + ",2,2,1,null," + ChrW(&H22) + "right-node" + ChrW(&H22) + ", decodeURIComponent(" + ChrW(&H22) + rightIndex + ChrW(&H22) + ")," + index.ToString() + ", decodeURIComponent(" + ChrW(&H22) + parentnode + ChrW(&H22) + ")" + ")'></a></div>")
                'html.Append($"<div class='modal_info_content'><p>{output.multi_pw_data(index).InfodocRight}</p></div></div></div></div></div></div></div>")


                'html.Append($"<div onclick ='showWrapper(ir_{index.ToString()})'><i class='fa fa-info-circle  {If((right) <> "" And (right) <> "NaN", "", "iinfoColor")} {If((right) <> "" And (right) <> "NaN", "", "chkEvaluator")} definfovalue' aria-hidden='true'></i></div>")
                'html.Append($"<div class='info_tooltip hideshowinfo parentnode103{index} {If(output.multi_pw_data(index).InfodocRight = "", " divchkEvaluator", "")}' style='display:none;' id='ir_" + index.ToString() + "'><div class='tooltop_head'><span>" + output.multi_pw_data(index).RightNode + "</span><div class='action_icons'><a" + If(output.isPipeViewOnly, " class='d-none'", "") + " href='javascript:void(0);'><i class='fas fa-edit chkEvaluator d-none' onclick='showInfoPopup(decodeURIComponent(" + ChrW(&H22) + right + ChrW(&H22) + ")" + "," + output.multi_pw_data(index).NodeID_Right.ToString() + ",2,2,1,null," + ChrW(&H22) + "right-node" + ChrW(&H22) + "," + ChrW(&H22) + output.multi_pw_data(index).RightNode + ChrW(&H22) + "," + index.ToString() + ", decodeURIComponent(" + ChrW(&H22) + parentnode + ChrW(&H22) + ")" + ")' aria-hidden='true'></i></a>")
                'html.Append("<a href='javascript:void(0);'><i class='fa fa-times' onclick='hideWrapper(ir_" + index.ToString() + ")' aria-hidden='true'></i></a></div></div><p>" + output.multi_pw_data(index).InfodocRight.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)) + "</p></div></div><div class='box_title'  style='cursor: pointer;' onclick='activeRow(" + index.ToString() + ");'><p>" + output.multi_pw_data(index).RightNode + "</p></div><div class='info_div'>")

                'html.Append($"<div onclick='showWrapper(irr_{index.ToString()})'><span class='wrt_text {If((rightWRT) <> "" And (rightWRT) <> "NaN", "", "wwinfoColor")} {If((rightWRT) <> "" And (rightWRT) <> "NaN", "", "chkEvaluator")}' > wrt </span></div>")
                'html.Append($"<div class='info_tooltip hideshowinfo {If(output.multi_pw_data(index).InfodocRightWRT = "", " divchkEvaluator", "")} right_tooltip' style='display:none;' id='irr_" + index.ToString() + "'><div class='tooltop_head'><span>" + output.multi_pw_data(index).RightNode + " WRT " + output.parent_node + "</span><div class='action_icons'>")
                'html.Append($"<a{If(output.isPipeViewOnly, " class='d-none'", "")} href='javascript:void(0);'><i class='fas fa-edit chkEvaluator d-none' onclick='showInfoPopup(decodeURIComponent(" + ChrW(&H22) + rightWRT + ChrW(&H22) + ")" + "," + output.multi_pw_data(index).NodeID_Right.ToString() + ",3,2,1,null," + ChrW(&H22) + "wrt-right-node" + ChrW(&H22) + "," + ChrW(&H22) + output.multi_pw_data(index).RightNode + ChrW(&H22) + "," + index.ToString() + ", decodeURIComponent(" + ChrW(&H22) + parentnode + ChrW(&H22) + ")" + ")' aria-hidden='true'></i></a><a href='javascript:void(0);'>")
                'html.Append("<i class='fa fa-times' onclick='hideWrapper(irr_" + index.ToString() + ")' aria-hidden='true'></i></a></div></div><p>" + output.multi_pw_data(index).InfodocRightWRT.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)) + "</p></div></div>")
                html.Append("</div></div>")
            End If

            html.Append("</div>")
            active_multi_index = index

        Next
        Dim gptoVerbal = True
        If output.pairwise_type = "ptVerbal" And gptoVerbal Then
            'html.Append("<span class='columns text-center icon-tt-info-circle action '>" + TeamTimeClass.ResString("msgGPWJudgment") + "</span>")
        End If
        'html.Append("</div>")
        html.Append("</div>")
        html.Append("</div>")
        dvContent.InnerHtml = html.ToString()
        'ScriptManager.RegisterClientScriptBlock(Page, GetType(String), "", $"initT2S()", True)
    End Sub

    Public Shared Function BindRestHtml(ByRef output As AnytimeOutputModel, ByVal fromval As Int16, ByVal toval As Int16) As String
        Dim IsMobile = False
        Dim active_multi_index = 0

        Dim screen_sizes As New Dictionary(Of String, Integer)
        screen_sizes.Add("option", 0)
        Dim bars_left()() As String = New String(7)() {}
        bars_left(0) = {"9", "Extreme", "nine", "EX"}
        bars_left(1) = {"8", "Very Strong to Extreme", "eight", " "}
        bars_left(2) = {"7", "Very Strong", "seven", "VS"}
        bars_left(3) = {"6", "Strong to Very Strong", "six", " "}
        bars_left(4) = {"5", "Strong", "five", "S"}
        bars_left(5) = {"4", "Moderate to Strong", "four", " "}
        bars_left(6) = {"3", "Moderate", "three", "M"}
        bars_left(7) = {"2", "Equal to Moderate", "two", " "}

        Dim bars_right()() As String = New String(7)() {}
        bars_right(0) = {"2", "Equal to Moderate", "two", " "}
        bars_right(1) = {"3", "Moderate", "three", "M"}
        bars_right(2) = {"4", "Moderate to Strong", "four", " "}
        bars_right(3) = {"5", "Strong", "five", "S"}
        bars_right(4) = {"6", "Strong to Very Strong", "six", " "}
        bars_right(5) = {"7", "Very Strong", "seven", "VS"}
        bars_right(6) = {"8", "Very Strong to Extreme", "eight", " "}
        bars_right(7) = {"9", "Extreme", "nine", "EX"}
        Dim left_input As Decimal() = New Decimal() {}
        Dim right_input As Decimal() = New Decimal() {}

        Dim html As StringBuilder = New StringBuilder()

        'For index = 201 To output.multi_pw_data.Count - 1
        For index = fromval To toval - 1
            html.Append($"<div class='chart_wrapper graphical_chart verbal_grphical_comment' data-val='{index}' onclick='activeRow({index}, true);' id='index_" + index.ToString() + "'>")
            ' If Not IsMobile Then
            Dim left As String = HttpUtility.UrlEncode(output.multi_pw_data(index).InfodocLeft.ToString().Replace("<b>", "").Replace("</b>", "").Trim().Replace(ChrW(&H22), ChrW(&H27)))
            Dim leftWRT As String = HttpUtility.UrlEncode(output.multi_pw_data(index).InfodocLeftWRT.ToString().Replace("<b>", "").Replace("</b>", "").Trim().Replace(ChrW(&H22), ChrW(&H27)))
            html.Append("<div class='value_wrapper blue_bgcolor'><div class='info_div'>")
            Dim parentnode As String = HttpUtility.UrlEncode(output.parent_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))

            html.Append($"<div onclick='showWrapper(il_{index.ToString()})'><i class='fa fa-info-circle {If((left) <> "" And (left) <> "NaN", "", "iinfoColor")} {If((left) <> "" And (left) <> "NaN", "", "chkEvaluator")} definfovalue ' aria-hidden='true'></i></div>")
            html.Append($"<div class='info_tooltip hideshowinfo parentnode102{index} {If(output.multi_pw_data(index).InfodocLeft = "", " divchkEvaluator", "")}' style='display:none;' id='il_" + index.ToString() + "'><div class='tooltop_head'><span>" + output.multi_pw_data(index).LeftNode + "</span><div class='action_icons'>")
            html.Append($"<a{If(output.isPipeViewOnly, " class='d-none'", "")} href='javascript:void(0);'><i class='fas fa-edit chkEvaluator d-none' onclick='showInfoPopup(decodeURIComponent(" + ChrW(&H22) + left + ChrW(&H22) + ")" + "," + output.multi_pw_data(index).NodeID_Left.ToString() + ",2,2,1,null," + ChrW(&H22) + "left-text-node" + ChrW(&H22) + "," + ChrW(&H22) + output.multi_pw_data(index).LeftNode + ChrW(&H22) + "," + index.ToString() + ", decodeURIComponent(" + ChrW(&H22) + parentnode + ChrW(&H22) + ")" + ")' aria-hidden='true'></i></a>")
            html.Append("<a href='javascript:void(0);'><i class='fa fa-times' onclick='hideWrapper(il_" + index.ToString() + ")' aria-hidden='true'></i></a></div></div><p>" + output.multi_pw_data(index).InfodocLeft.ToString().Replace("<b>", "").Replace("</b>", "").Trim().Replace(ChrW(&H22), ChrW(&H27)) + "</p></div></div>")

            html.Append("<div Class='box_title' style='cursor: pointer;' onclick='activeRow(" + index.ToString() + ");'><p>" + output.multi_pw_data(index).LeftNode + "</p></div><div class='info_div'>")

            html.Append($"<div onclick='showWrapper(ilr_{index.ToString()})'><span class='wrt_text {If((leftWRT) <> "" And (leftWRT) <> "NaN", "", "wwinfoColor")} {If((leftWRT) <> "" And (leftWRT) <> "NaN", "", "chkEvaluator")}' > wrt </span></div>")
            html.Append($"<div class='info_tooltip hideshowinfo right_tooltip{If(output.multi_pw_data(index).InfodocLeftWRT = "", " divchkEvaluator", "")}' style='display:none;' id='ilr_" + index.ToString() + "'><div class='tooltop_head'><span>" + output.multi_pw_data(index).LeftNode + " WRT " + output.parent_node + "</span><div class='action_icons'>")

            html.Append($"<a{If(output.isPipeViewOnly, " class='d-none'", "")} href='javascript:void(0);'><i class='fas fa-edit chkEvaluator d-none' onclick='showInfoPopup(decodeURIComponent(" + ChrW(&H22) + leftWRT + ChrW(&H22) + ")" + "," + output.multi_pw_data(index).NodeID_Left.ToString() + ",3,2,1,null," + ChrW(&H22) + "wrt-text-node" + ChrW(&H22) + "," + ChrW(&H22) + output.multi_pw_data(index).LeftNode + ChrW(&H22) + "," + index.ToString() + ", decodeURIComponent(" + ChrW(&H22) + parentnode + ChrW(&H22) + ")" + ")' aria-hidden='true'></i></a>")
            html.Append("<a href='javascript:void(0);'><i class='fa fa-times' onclick='hideWrapper(ilr_" + index.ToString() + ")' aria-hidden='true'></i></a></div></div><p>" + output.multi_pw_data(index).InfodocLeftWRT.ToString().Replace("<b>", "").Replace("</b>", "").Trim().Replace(ChrW(&H22), ChrW(&H27)) + "</p></div></div></div>")
            'End If

            If output.pairwise_type = "ptVerbal" Then
                html.Append("<div class='rating_area'>")

                'If output.show_comments Then
                html.Append($"<div class='comment_div' readonly id='comdiv{index}'>")
                html.Append("<div style='cursor:pointer;' onclick='toggleBox(" + index.ToString() + ")'>")
                If (output.multi_pw_data(index).Comment Is Nothing Or output.multi_pw_data(index).Comment = "") Then
                    html.Append("<i class='far fa-comments' aria-hidden='true'></i></div>")
                Else
                    html.Append("<i Class='fa fa-comments' title='" + output.multi_pw_data(index).Comment + "' aria-hidden='true'></i></div>")
                End If
                If (index = 0) Then
                    html.Append($"<div class='info_tooltip right_tooltip tooltip_wrapper hideshowinfo'  id='toggleComment_{index}' style='display:none;' ><div class='d-flex justify-content-between'><span>Add your comment</span>")
                Else
                    html.Append($"<div class='info_tooltip right_tooltip tooltip_wrapper hideshowinfo'  id='toggleComment_{index}' style='display:none;' ><div class='d-flex justify-content-between'><span>Add your comment</span>")
                End If
                'html.Append($"<div class='info_tooltip'  style='display:none;' id='divComment_{index}'>abc</div>")onmouseover='showComment({index})'
                html.Append($"<div class='action_icons' ><a href='javascript:void(0);' onclick='toggleBox({index})'><i class='fa fa-times' aria-hidden='true'></i></a></div></div>")
                html.Append($"<textarea{If(output.isPipeViewOnly, " style='pointer-events:none;'", "")} id='comment_{index}' class='form-control mb-3 mt-2 w-100' rows='3'>" + output.multi_pw_data(index).Comment + "</textarea><div class='comt_btn text-end'>")
                html.Append("<button type='button' class='py-1 px-2' onclick='save_multi_pairwise(" + ChrW(&H22) + "comment" + ChrW(&H22) + "," + index.ToString() + ",0,1," + index.ToString() + "," + output.current_step.ToString() + ")'> <i class='fa fa-check' aria-hidden='true'></i> OK</button>")
                html.Append("</div></div>")
                html.Append("</div>")
                'End If

                html.Append($"<div class='chart_area'{If(output.isPipeViewOnly, " style='pointer-events:none;'", "")}>")
                Dim value As Int32 = 0
                Try
                    value = Convert.ToInt32(Math.Abs(Math.Floor(output.multi_pw_data(index).Value)))
                Catch ex As Exception
                    value = 0
                End Try
                If (value = 1) Then
                    Dim abc = 1
                End If
                If active_multi_index <> index Or output.collapse_bars Then
                    html.Append("<div class='rating_result'>")
                    'If (output.multi_pw_data(index).Advantage = 1 And value >= 2) Then
                    '    html.Append($"<div style='display:block;' class='left_result left_result_{index}'><span id='left_result_{index}'>" + bars_right(value - 2)(1) + "</span></div>")
                    'Else
                    '    html.Append($"<div style='display:none;' class='left_result left_result_{index}'><span id='left_result_{index}'></span></div>")
                    'End If

                    Dim innerIndex = (bars_left.Length) - value
                    'If (output.multi_pw_data(index).Advantage = -1) Then
                    '    If (innerIndex < 0) Then
                    '        innerIndex = -1 * innerIndex
                    '    End If
                    '    Try
                    '        html.Append($"<div style='display:block;' class='right_result right_result_{index}'><span id='right_result_{index}'>" + bars_left(Convert.ToInt32(innerIndex + 1))(1) + "</span></div>")
                    '    Catch ex As Exception

                    '        html.Append($"<div style='display:block;' class='right_result right_result_{index}'><span id='right_result_{index}'>" + bars_left(Convert.ToInt32(innerIndex))(1) + "</span></div>")
                    '    End Try
                    'Else
                    '    html.Append($"<div style='display:none;' class='right_result right_result_{index}'><span id='right_result_{index}'></span></div>")
                    'End If
                    'If (value = 1 And output.multi_pw_data(index).Advantage = 0) Then
                    '    html.Append($"<div class='equal equal_{index}'><span>Equal</span></div>")
                    'Else
                    '    html.Append($"<div style='display:none;' class='equal equal_{index}'><span>Equal</span></div>")
                    'End If

                    'html.Append("</div>")
                End If

                html.Append($"<div class='ratting_box' id='rating_box_{index}'>")
                Dim currentRating = ""
                Dim currentIndex = 0

                For leftindex = 0 To bars_left.Length - 1
                    If bars_left(leftindex)(3) <> " " Then
                        currentRating = bars_left(leftindex)(3)
                        currentIndex = Convert.ToInt32(bars_left(leftindex)(0))
                        'data.Value >= bar[0]
                        '
                        html.Append("<div id='show_" + index.ToString() + "_" + (bars_left.Length - leftindex).ToString() + "' title='" + bars_left(leftindex)(1) + "' onclick='save_multi_pairwise(" + ChrW(&H22) + "left" + ChrW(&H22) + "," + leftindex.ToString() + "," + (bars_left.Length - leftindex).ToString() + ",1," + index.ToString() + "," + output.current_step.ToString() + ")' class='rating_" + bars_left(leftindex)(3) + " rating_content rating_content_left " + If(output.multi_pw_data(index).Advantage = 1 And value >= currentIndex, "selcted_spot selected_left", "") + "'><span>" + bars_left(leftindex)(3) + "</span></div>")
                    Else
                        currentIndex = Convert.ToInt32(bars_left(leftindex)(0))
                        html.Append("<div id='show_" + index.ToString() + "_" + (bars_left.Length - leftindex).ToString() + "' title='" + bars_left(leftindex)(1) + "' onclick='save_multi_pairwise(" + ChrW(&H22) + "left" + ChrW(&H22) + "," + leftindex.ToString() + "," + (bars_left.Length - leftindex).ToString() + ",1," + index.ToString() + "," + output.current_step.ToString() + ")' ")
                        html.Append(" Class='rating_" + currentRating + "_s rating_content rating_content_left " + If(output.multi_pw_data(index).Advantage = 1 And value >= currentIndex, "selcted_spot selected_left", "") + "'></div>")
                    End If
                Next
                If value >= 1 And output.multi_pw_data(index).Advantage = 1 Then
                    html.Append($"<div id='divRatingEQ_e_{index}' onclick='save_multi_pairwise(" + ChrW(&H22) + "equal" + ChrW(&H22) + "," + index.ToString() + ",1,0," + index.ToString() + "," + output.current_step.ToString() + ")' class='rating_e rating_content rating_content_left selected_left'><span>EQ</span></div>")
                ElseIf value >= 1 And output.multi_pw_data(index).Advantage = -1 Then
                    html.Append($"<div id='divRatingEQ_e_{index}' onclick='save_multi_pairwise(" + ChrW(&H22) + "equal" + ChrW(&H22) + "," + index.ToString() + ",1,0," + index.ToString() + "," + output.current_step.ToString() + ")' class='rating_e rating_content rating_content_right selected_right'><span>EQ</span></div>")
                ElseIf value = 1 And output.multi_pw_data(index).Advantage = 0 Then
                    html.Append($"<div id='divRatingEQ_e_{index}' onclick='save_multi_pairwise(" + ChrW(&H22) + "equal" + ChrW(&H22) + "," + index.ToString() + ",1,0," + index.ToString() + "," + output.current_step.ToString() + ")' class='rating_e eqalizer_color rating_content rating_content_right'><span>EQ</span></div>")
                Else
                    html.Append($"<div id='divRatingEQ_e_{index}' onclick='save_multi_pairwise(" + ChrW(&H22) + "equal" + ChrW(&H22) + "," + index.ToString() + ",1,0," + index.ToString() + "," + output.current_step.ToString() + ")' class='rating_e rating_content rating_content_right rating_content_left'><span>EQ</span></div>")
                End If
                For rightindex = 0 To bars_right.Length - 1
                    If bars_right(rightindex)(3) <> " " Then
                        currentIndex = Convert.ToInt32(bars_right(rightindex)(0))
                        html.Append("<div id='showr_" + index.ToString() + "_" + rightindex.ToString() + "' title='" + bars_right(rightindex)(1) + "' onclick='save_multi_pairwise(" + ChrW(&H22) + "right" + ChrW(&H22) + "," + rightindex.ToString() + "," + rightindex.ToString() + ",-1," + index.ToString() + "," + output.current_step.ToString() + ")' ")
                        html.Append("Class='rating_" + bars_right(rightindex)(3) + " rating_content rating_content_right " + If(output.multi_pw_data(index).Advantage = -1 And value >= currentIndex, "selected_right", "") + If(output.multi_pw_data(index).Advantage = -1 And value = currentIndex, " selcted_spot", "") + "'><span>" + bars_right(rightindex)(3) + "</span>")
                        If (output.multi_pw_data(index).Advantage = -1 And value = currentIndex) Then
                            html.Append($"<div class='multiverbal_spotname right_name_{index}'>" + bars_right(value - 2)(1) + "</div>")
                        End If
                        html.Append("</div>")
                    Else
                        currentRating = bars_right(rightindex + 1)(3)
                        currentIndex = Convert.ToInt32(bars_right(rightindex)(0))
                        html.Append("<div id='showr_" + index.ToString() + "_" + rightindex.ToString() + "' title='" + bars_right(rightindex)(1) + "' onclick='save_multi_pairwise(" + ChrW(&H22) + "right" + ChrW(&H22) + "," + rightindex.ToString() + "," + rightindex.ToString() + ",-1," + index.ToString() + "," + output.current_step.ToString() + ")' class='rating_" + currentRating + "_s rating_content rating_content_right " + If(output.multi_pw_data(index).Advantage = -1 And value >= currentIndex, "selected_right", "") + If(output.multi_pw_data(index).Advantage = -1 And value = currentIndex, " selcted_spot", "") + "'>")
                        If (output.multi_pw_data(index).Advantage = -1 And value = currentIndex) Then
                            html.Append($"<div class='multiverbal_spotname right_name_{index}'>" + bars_right(value - 2)(1) + "</div>")
                        End If
                        html.Append("</div>")
                    End If
                Next
                html.Append("</div></div>")

                html.Append($"<div{If(output.isPipeViewOnly, " style='pointer-events:none;'", "")} class='close_icon' style='cursor:pointer;' onclick='save_multi_pairwise(" + ChrW(&H22) + "close" + ChrW(&H22) + "," + index.ToString() + "," + index.ToString() + ",-1," + index.ToString() + "," + output.current_step.ToString() + ")' ><i class='fa fa-times' aria-hidden='true'></i></div>")

                html.Append("</div>")

            End If

            Dim participant_slider = output.multi_pw_data

            If output.pairwise_type = "ptGraphical" Then
                html.Append($"<div class='rating_area graphRating'>")
                html.Append("<div class='scale_wt'>")

                html.Append($"<div class='form-group'{If(output.isPipeViewOnly, " style='pointer-events:none;'", "")}>")

                html.Append($"<div id='gslider{index}'></div>")
                html.Append("</div>")

                html.Append("<div class='result_values'>")
                html.Append($"<div class='reverse_icon'{If(output.isPipeViewOnly, " style='pointer-events:none;'", "")}>")
                html.Append($"<a href='javascript:void(0);' onclick=swap_value('{index}'); id='reverse_icon_{index}'><img src='../../assets/images/reverce_icon.png' /></a>")
                html.Append("</div>")
                html.Append($"<div class='inputs_value'{If(output.isPipeViewOnly, " style='pointer-events:none;'", "")}>")

                Dim fun As String = $"onkeyup=graphicalall_key_up(this,'0','-1','up',{index},'false')"
                'fun += "onclick=slideScroll()"
                'Dim fun As String = "onkeyup=graphical_key_up(this,'0','-1','up',null,'false')"
                'fun += " onkeydown=graphical_key_up(this,'0','-1','press',null,'false')"
                'fun += " onmousedown=graphical_key_up(this,'0','-1','press',null,'false')"
                'fun += " onmouseup=graphical_key_up(this,'0','-1','up',null,'false')"
                'fun += " onblur=graphical_key_up(this,'0','-1','up',null,'false')"
                'fun += " onkeypress='return isNumberKeyWithDecimal(this, event);'"
                If output.multi_pw_data(index).Advantage > 0 Then
                    html.Append($"<input type='text'  value='{Math.Round(Convert.ToDecimal(output.multi_pw_data(index).Value), 2)}' id='input1{index}' class='value_control checkEnter' {fun}>")
                Else
                    html.Append($"<input type='text' value='1' id='input1{index}' class='value_control checkEnter' {fun}>")
                End If

                'onkeypress='checkEnter(this)'
                If output.multi_pw_data(index).Advantage < 0 Then
                    html.Append($"<input type='text' value='{Math.Round(Convert.ToDecimal(output.multi_pw_data(index).Value), 2)}' id='input2{index}' class='value_control checkEnter' {fun}>")
                Else
                    html.Append($"<input type='text' value='1' id='input2{index}' class='value_control checkEnter' {fun}>")
                End If
                html.Append("</div>")
                html.Append("<div class='d-flex justify-content-center slide_valueComment '>")

                fun = $"onclick='add_multivalues(-2147483648000, 0, {index}, event); set_slider_blank({index})'"
                html.Append($"<div{If(output.isPipeViewOnly, " style='pointer-events:none;'", "")} class='close_icon ms-3 me-3' {fun}>")
                html.Append($"<i class='fa fa-times' aria-hidden='true' id='close_icon_{index}'></i>")
                html.Append("</div>")
                'Comment box
                If output.show_comments Then
                    html.Append("<div style='cursor:pointer;'  class='comment_div'>")
                    'html.Append("<input type='checkbox' class='comment_model'>")
                    If (output.multi_pw_data(index).Comment Is Nothing Or output.multi_pw_data(index).Comment = "") Then
                        html.Append($"<i class='far fa-comments' id='multi_comment_icon_{index}' onclick='showNode(3{index},{index})'  aria-hidden='true'></i>")
                    Else
                        html.Append($"<i Class='fa fa-comments' id='multi_comment_icon_{index}' onclick='showNode(3{index},{index})'  title='" + output.multi_pw_data(index).Comment + "' aria-hidden='true'></i>")
                    End If
                    If (index = 0) Then
                        html.Append($"<div class='info_tooltip tooltip_wrapper hideshowinfo'  id='parentnode3{index}' style='display:none;'>")
                    Else
                        html.Append($"<div class='info_tooltip right_tooltip tooltip_wrapper hideshowinfo'  id='parentnode3{index}' style='display:none;'>")
                    End If
                    html.Append("<div class='d-flex justify-content-between'>")
                    html.Append("<span>Add your comment</span>")
                    html.Append($"<div class='action_icons' ><a href='javascript:void(0);' onclick='toggleBox()'><i class='fa fa-times' onclick='hideNode(3{index},{index})' aria-hidden='true'></i></a></div>")
                    html.Append("</div>")
                    html.Append($"<textarea{If(output.isPipeViewOnly, " style='pointer-events:none;'", "")} class='form-control mb-3 mt-2 w-100' rows='3' id='multi_comment_{index}' placeholder='Add your comment'>{output.multi_pw_data(index).Comment}</textarea>")
                    html.Append("<div class='comt_btn text-end'>")
                    html.Append($"<button type='button' class='py-1 px-2' onclick='hideNode(3{index},{index})'> <i class='fa fa-check' aria-hidden='true'></i> OK</button>")
                    'html.Append($"<button class='py-1 px-2' onclick='save_multi_pairwise(" + ChrW(&H22) + "comment" + ChrW(&H22) + "," + index.ToString() + ",0,1," + index.ToString() + "," + output.current_step.ToString() + ")'> <i class='fa fa-check' aria-hidden='true'></i> OK</button>")
                    html.Append("</div>")
                    html.Append("</div>")
                    html.Append("</div>")
                End If

                html.Append("</div>")
                html.Append("</div>")
                html.Append("</div>")

                html.Append("</div>")
            End If

            If Not IsMobile Then
                Dim right As String = HttpUtility.UrlEncode(output.multi_pw_data(index).InfodocRight.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
                Dim rightWRT As String = HttpUtility.UrlEncode(output.multi_pw_data(index).InfodocRightWRT.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
                html.Append($"<div class='value_wrapper green_bgcolor'><div class='info_div'><div onclick='showWrapper(ir_{index.ToString()})'><i class='fa fa-info-circle  {If((right) <> "" And (right) <> "NaN", "", "iinfoColor")} {If((right) <> "" And (right) <> "NaN", "", "chkEvaluator")} definfovalue' aria-hidden='true'></i></div>")
                html.Append($"<div class='info_tooltip hideshowinfo parentnode103{index} {If(output.multi_pw_data(index).InfodocRight = "", " divchkEvaluator", "")}' style='display:none;' id='ir_" + index.ToString() + "'><div class='tooltop_head'><span>" + output.multi_pw_data(index).RightNode + "</span><div class='action_icons'><a" + If(output.isPipeViewOnly, " class='d-none'", "") + " href='javascript:void(0);'><i class='fas fa-edit chkEvaluator d-none' onclick='showInfoPopup(decodeURIComponent(" + ChrW(&H22) + right + ChrW(&H22) + ")" + "," + output.multi_pw_data(index).NodeID_Right.ToString() + ",2,2,1,null," + ChrW(&H22) + "right-node" + ChrW(&H22) + "," + ChrW(&H22) + output.multi_pw_data(index).RightNode + ChrW(&H22) + "," + index.ToString() + ", decodeURIComponent(" + ChrW(&H22) + parentnode + ChrW(&H22) + ")" + ")' aria-hidden='true'></i></a>")
                html.Append("<a href='javascript:void(0);'><i class='fa fa-times' onclick='hideWrapper(ir_" + index.ToString() + ")' aria-hidden='true'></i></a></div></div><p>" + output.multi_pw_data(index).InfodocRight.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)) + "</p></div></div><div class='box_title'  style='cursor: pointer;' onclick='activeRow(" + index.ToString() + ");'><p>" + output.multi_pw_data(index).RightNode + "</p></div><div class='info_div'>")

                html.Append($"<div onclick='showWrapper(irr_{index.ToString()})'><span class='wrt_text {If((rightWRT) <> "" And (rightWRT) <> "NaN", "", "wwinfoColor")} {If((rightWRT) <> "" And (rightWRT) <> "NaN", "", "chkEvaluator")}' > wrt </span></div>")
                html.Append($"<div class='info_tooltip hideshowinfo {If(output.multi_pw_data(index).InfodocRightWRT = "", " divchkEvaluator", "")} right_tooltip' style='display:none;' id='irr_" + index.ToString() + "'><div class='tooltop_head'><span>" + output.multi_pw_data(index).RightNode + " WRT " + output.parent_node + "</span><div class='action_icons'>")
                html.Append($"<a{If(output.isPipeViewOnly, " class='d-none'", "")} href='javascript:void(0);'><i class='fas fa-edit chkEvaluator d-none' onclick='showInfoPopup(decodeURIComponent(" + ChrW(&H22) + rightWRT + ChrW(&H22) + ")" + "," + output.multi_pw_data(index).NodeID_Right.ToString() + ",3,2,1,null," + ChrW(&H22) + "wrt-right-node" + ChrW(&H22) + "," + ChrW(&H22) + output.multi_pw_data(index).RightNode + ChrW(&H22) + "," + index.ToString() + ", decodeURIComponent(" + ChrW(&H22) + parentnode + ChrW(&H22) + ")" + ")' aria-hidden='true'></i></a><a href='javascript:void(0);'>")
                html.Append("<i class='fa fa-times' onclick='hideWrapper(irr_" + index.ToString() + ")' aria-hidden='true'></i></a></div></div><p>" + output.multi_pw_data(index).InfodocRightWRT.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)) + "</p></div></div></div>")
            End If

            html.Append("</div>")
            active_multi_index = index

        Next

        Return html.ToString()
    End Function

    Public Shared Function GetAllPairWiseData(ByRef AnytimeAction As clsAction, ByRef App As clsComparionCore, ByRef StepNode As clsNode, <Out> ByRef multi_GUIDs As List(Of String()),
    <Out> ByRef PairwiseType As String, <Out> ByRef MultiPW_Data As List(Of clsPairwiseLine), <Out> ByRef ParentNodeName As String, <Out> ByRef ParentNodeGUID As Guid, <Out> ByRef StepTask As String,
    <Out> ByRef parent_node_info As String, <Out> ByRef pipeHelpUrl As String, <Out> ByRef ParentNodeID As Integer, <Out> ByRef infodoc_params As String(), <Out> ByRef qh_help_id As ecEvaluationStepType) As Object
        Dim IsUndefined As Boolean = False
        Dim fIsPWOutcomes As Boolean = AnytimeAction.ActionType = ActionType.atAllPairwiseOutcomes
        Dim UndefIDx As Integer = -1
        If TypeOf AnytimeAction.ActionData Is clsAllPairwiseEvaluationActionData Then
            Dim AllPwData As clsAllPairwiseEvaluationActionData = CType(AnytimeAction.ActionData, clsAllPairwiseEvaluationActionData)
            Dim fAlts As Boolean = AllPwData.ParentNode.IsTerminalNode
            'Dim StepTask As String = ""
            Try
                StepTask = TeamTimeClass.GetPipeStepTask(AnytimeAction, Nothing, AnytimeClass.IsImpact AndAlso Not fAlts)
            Catch
                StepTask = ""
            End Try
            Dim PWType = App.ActiveProject.ProjectManager.PipeBuilder.GetPairwiseTypeForNode(AllPwData.ParentNode)
            qh_help_id = If(PWType = CanvasTypes.PairwiseType.ptVerbal, ecEvaluationStepType.VerbalPW, ecEvaluationStepType.GraphicalPW)
            StepNode = AllPwData.ParentNode
            Dim L As List(Of ECTypes.KnownLikelihoodDataContract) = Nothing

            If App.isRiskEnabled AndAlso AllPwData.ParentNode.MeasureType() = ECCore.ECMeasureType.mtPWAnalogous Then
                L = AllPwData.ParentNode.GetKnownLikelihoods()
            End If
            Dim RS As clsRatingScale = Nothing
            If fIsPWOutcomes AndAlso AnytimeAction.ParentNode IsNot Nothing Then
                If AnytimeAction.ParentNode.IsAlternative Then
                    RS = CType(AnytimeAction.PWONode.MeasurementScale, clsRatingScale)
                Else
                    If (Not (AnytimeAction.ParentNode.ParentNode Is Nothing)) Then
                        RS = CType(AnytimeAction.ParentNode.ParentNode().MeasurementScale, clsRatingScale)
                    End If
                End If

            End If
            Dim Lst As List(Of clsPairwiseLine) = New List(Of clsPairwiseLine)()
            Dim ID = 0
            For Each tJud As clsPairwiseMeasureData In AllPwData.Judgments
                Dim tLeftNode As clsNode = Nothing
                Dim tRightNode As clsNode = Nothing
                If fIsPWOutcomes Then
                    App.ActiveProject.ProjectManager.PipeBuilder.GetPWNodes(AnytimeAction, tJud, tLeftNode, tRightNode)
                Else

                    If fAlts Then
                        tLeftNode = App.ActiveProject.HierarchyAlternatives.GetNodeByID(tJud.FirstNodeID)
                        tRightNode = App.ActiveProject.HierarchyAlternatives.GetNodeByID(tJud.SecondNodeID)
                    Else
                        tLeftNode = App.ActiveProject.HierarchyObjectives.GetNodeByID(tJud.FirstNodeID)
                        tRightNode = App.ActiveProject.HierarchyObjectives.GetNodeByID(tJud.SecondNodeID)
                    End If
                End If
                Dim KnownLikelihoodA As Double = -1
                Dim KnownLikelihoodB As Double = -1

                If L IsNot Nothing Then

                    For Each tLikelihood As ECTypes.KnownLikelihoodDataContract In L

                        If tLikelihood.Value >= 0 Then

                            If tLikelihood.ID = tLeftNode.NodeID Then
                                KnownLikelihoodA = tLikelihood.Value
                            End If

                            If tLikelihood.ID = tRightNode.NodeID Then
                                KnownLikelihoodB = tLikelihood.Value
                            End If
                        End If
                    Next
                End If
                If tLeftNode IsNot Nothing AndAlso tRightNode IsNot Nothing Then
                    Dim PW = New clsPairwiseLine(ID, tLeftNode.NodeID, tRightNode.NodeID, tLeftNode.NodeName, tRightNode.NodeName, tJud.IsUndefined, tJud.Advantage, tJud.Value, tJud.Comment, KnownLikelihoodA, KnownLikelihoodB)
                    Dim guids As String() = New String(2) {}
                    guids(0) = AllPwData.ParentNode.NodeGuidID.ToString()
                    guids(1) = tLeftNode.NodeGuidID.ToString()
                    guids(2) = tRightNode.NodeGuidID.ToString()
                    multi_GUIDs.Add(guids)

                    If fIsPWOutcomes AndAlso RS IsNot Nothing Then
                        Dim tRating As clsRating = RS.GetRatingByID(tLeftNode.NodeGuidID)

                        If tRating IsNot Nothing Then
                            PW.LeftNodeComment = tRating.Comment
                        End If

                        tRating = RS.GetRatingByID(tRightNode.NodeGuidID)

                        If tRating IsNot Nothing Then
                            PW.RightNodeComment = tRating.Comment
                        End If
                    End If
                    PW.InfodocLeft = Convert.ToString(InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, If(tLeftNode.IsAlternative, Consts.reObjectType.Alternative, Consts.reObjectType.Node), tLeftNode.NodeID.ToString(), tLeftNode.InfoDoc, True, True, -1))
                    PW.InfodocLeftWRT = Convert.ToString(InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.AltWRTNode, tLeftNode.NodeID.ToString(), App.ActiveProject.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(tLeftNode.NodeGuidID, AllPwData.ParentNode.NodeGuidID), True, True, AllPwData.ParentNode.NodeID))
                    PW.InfodocRight = Convert.ToString(InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, If(tRightNode.IsAlternative, Consts.reObjectType.Alternative, Consts.reObjectType.Node), tRightNode.NodeID.ToString(), tRightNode.InfoDoc, True, True, -1))
                    PW.InfodocRightWRT = Convert.ToString(InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.AltWRTNode, tRightNode.NodeID.ToString(), App.ActiveProject.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(tRightNode.NodeGuidID, AllPwData.ParentNode.NodeGuidID), True, True, AllPwData.ParentNode.NodeID))

                    If UndefIDx = -1 AndAlso PW.isUndefined Then
                        UndefIDx = ID
                    End If
                    Lst.Add(PW)
                    ID += 1

                    If tJud.IsUndefined Then
                        IsUndefined = True
                    End If
                End If
            Next
            PairwiseType = PWType.ToString()
            MultiPW_Data = Lst
            ParentNodeName = AllPwData.ParentNode.NodeName
            ParentNodeGUID = AllPwData.ParentNode.NodeGuidID
            parent_node_info = Convert.ToString(InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.Node, AllPwData.ParentNode.NodeID.ToString(), AllPwData.ParentNode.InfoDoc, True, True, -1))
            pipeHelpUrl = TeamTimeClass.ResString(CType(PWType = 1, String)) 'If(CanvasTypes.PairwiseType.ptVerbal, "help_pipe_multiPairwiseVerbal", "help_pipe_multiPairwiseGraphical"))
            ParentNodeID = AllPwData.ParentNode.NodeID
            infodoc_params = New String(4) {}
            infodoc_params(0) = GeckoClass.GetInfodocParams(AllPwData.ParentNode.NodeGuidID, Guid.Empty, True)
            infodoc_params(1) = GeckoClass.GetInfodocParams(AllPwData.ParentNode.NodeGuidID, Guid.Empty, True)
            infodoc_params(2) = GeckoClass.GetInfodocParams(AllPwData.ParentNode.NodeGuidID, Guid.Empty, True)
            infodoc_params(3) = GeckoClass.GetInfodocParams(AllPwData.ParentNode.NodeGuidID, AllPwData.ParentNode.NodeGuidID, True)
            infodoc_params(4) = GeckoClass.GetInfodocParams(AllPwData.ParentNode.NodeGuidID, AllPwData.ParentNode.NodeGuidID, True)
        End If
    End Function


End Class