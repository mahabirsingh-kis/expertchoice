Imports Pages.external_classes

Public Class Pairwise
    Inherits System.Web.UI.UserControl

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    End Sub

    Public Shared Function GetDetails(ByVal AnytimeAction As clsAction, ByVal App As clsComparionCore) As PairwiseModel
        Dim PairwiseModel As PairwiseModel = New PairwiseModel()
        Dim forceError = CBool(HttpContext.Current.Session(Constants.Sess_ForceError))
        Dim PWData = CType(AnytimeAction.ActionData, clsPairwiseMeasureData)
        Dim ParentNode = CType(App.ActiveProject.HierarchyObjectives.GetNodeByID(PWData.ParentNodeID), clsNode)
        Dim FirstNode = CType(Nothing, clsNode)
        Dim SecondNode = CType(Nothing, clsNode)
        PairwiseModel.StepNode = ParentNode
        PairwiseModel.comment = PWData.Comment
        PairwiseModel.PairwiseType = App.ActiveProject.ProjectManager.PipeBuilder.GetPairwiseTypeForNode(ParentNode).ToString()

        If PairwiseModel.PairwiseType = "ptVerbal" Then
            PairwiseModel.pipeHelpUrl = TeamTimeClass.ResString("help_pipe_singlePairwiseVerbal")
            PairwiseModel.qh_help_id = Canvas.PipeParameters.ecEvaluationStepType.VerbalPW
            PairwiseModel.PipeWarning = If(HttpContext.Current.Request.Cookies(Constants.Cook_Extreme) Is Nothing, TeamTimeClass.ResString("msgPWExtreme"), "")
        Else
            PairwiseModel.pipeHelpUrl = TeamTimeClass.ResString("help_pipe_singlePairwiseGraphical")
            PairwiseModel.qh_help_id = Canvas.PipeParameters.ecEvaluationStepType.GraphicalPW
        End If

        PairwiseModel.question = ""
        PairwiseModel.wording = ""
        Dim ss As String = ""

        Dim NodeType = App.ActiveProject.HierarchyObjectives.TerminalNodes()
        Dim index As Integer = NodeType.FindIndex(Function(item) item.NodeName = ParentNode.NodeName)

        If index >= 0 Then
            PairwiseModel.question = "alternatives"
            PairwiseModel.wording = App.ActiveProject.PipeParameters.JudgementAltsPromt
        End If

        If ParentNode.IsTerminalNode Then
            PairwiseModel.question = App.ActiveProject.PipeParameters.NameAlternatives
            PairwiseModel.wording = App.ActiveProject.PipeParameters.JudgementAltsPromt
            FirstNode = App.ActiveProject.HierarchyAlternatives.GetNodeByID(PWData.FirstNodeID)
            SecondNode = App.ActiveProject.HierarchyAlternatives.GetNodeByID(PWData.SecondNodeID)
        Else
            PairwiseModel.question = App.ActiveProject.PipeParameters.NameObjectives
            PairwiseModel.wording = App.ActiveProject.PipeParameters.JudgementPromt
            FirstNode = App.ActiveProject.HierarchyObjectives.GetNodeByID(PWData.FirstNodeID)
            SecondNode = App.ActiveProject.HierarchyObjectives.GetNodeByID(PWData.SecondNodeID)
        End If

        PairwiseModel.StepTask = ""

        Try
            PairwiseModel.StepTask = TeamTimeClass.GetPipeStepTask(AnytimeAction, Nothing, AnytimeClass.IsImpact AndAlso Not FirstNode.IsTerminalNode AndAlso Not SecondNode.IsTerminalNode)
        Catch
            PairwiseModel.StepTask = ""
        End Try

        PairwiseModel.parent_node_info = CStr(Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, reObjectType.Node, ParentNode.NodeID.ToString(), ParentNode.InfoDoc, True, True, -1))
        PairwiseModel.first_node_info = CStr(Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, If(FirstNode.IsAlternative, reObjectType.Alternative, reObjectType.Node), FirstNode.NodeID.ToString(), FirstNode.InfoDoc, True, True, -1))
        PairwiseModel.second_node_info = CStr(Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, If(SecondNode.IsAlternative, reObjectType.Alternative, reObjectType.Node), SecondNode.NodeID.ToString(), SecondNode.InfoDoc, True, True, -1))
        PairwiseModel.wrt_first_node_info = CStr(Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, reObjectType.AltWRTNode, FirstNode.NodeID.ToString(), App.ActiveProject.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(FirstNode.NodeGuidID, ParentNode.NodeGuidID), True, True, ParentNode.NodeID))
        PairwiseModel.wrt_second_node_info = CStr(Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, reObjectType.AltWRTNode, SecondNode.NodeID.ToString(), App.ActiveProject.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(SecondNode.NodeGuidID, ParentNode.NodeGuidID), True, True, ParentNode.NodeID))
        PairwiseModel.FirstNodeName = FirstNode.NodeName
        PairwiseModel.SecondNodeName = SecondNode.NodeName
        PairwiseModel.ParentNodeName = ParentNode.NodeName
        PairwiseModel.PWValue = PWData.Value
        PairwiseModel.PWAdvantage = PWData.Advantage
        PairwiseModel.IsUndefined = PWData.IsUndefined
        PairwiseModel.ParentNodeGUID = ParentNode.NodeGuidID
        PairwiseModel.LeftNodeGUID = FirstNode.NodeGuidID
        PairwiseModel.RightNodeGUID = SecondNode.NodeGuidID
        PairwiseModel.infodoc_params = New String(4) {}
        PairwiseModel.infodoc_params(0) = GeckoClass.GetInfodocParams(ParentNode.NodeGuidID, Guid.Empty)
        PairwiseModel.infodoc_params(1) = GeckoClass.GetInfodocParams(FirstNode.NodeGuidID, SecondNode.NodeGuidID)
        PairwiseModel.infodoc_params(2) = GeckoClass.GetInfodocParams(SecondNode.NodeGuidID, FirstNode.NodeGuidID)
        PairwiseModel.infodoc_params(3) = GeckoClass.GetInfodocParams(FirstNode.NodeGuidID, ParentNode.NodeGuidID)
        PairwiseModel.infodoc_params(4) = GeckoClass.GetInfodocParams(SecondNode.NodeGuidID, ParentNode.NodeGuidID)
        Dim default_params As String = "c=-1&w=200&h=200"

        If PairwiseModel.infodoc_params(0) = "" Then
            PairwiseModel.infodoc_params(0) = default_params
        End If

        If PairwiseModel.infodoc_params(1) = "" Then
            PairwiseModel.infodoc_params(1) = GeckoClass.getCommonParams(PairwiseModel.infodoc_params(2))
        End If

        If PairwiseModel.infodoc_params(2) = "" Then
            PairwiseModel.infodoc_params(2) = GeckoClass.getCommonParams(PairwiseModel.infodoc_params(1))
        End If

        If PairwiseModel.infodoc_params(2) = "" OrElse PairwiseModel.infodoc_params(1) = "" Then
            GeckoClass.SetInfodocParams(FirstNode.NodeGuidID, SecondNode.NodeGuidID, default_params)
            GeckoClass.SetInfodocParams(SecondNode.NodeGuidID, FirstNode.NodeGuidID, default_params)
            PairwiseModel.infodoc_params(1) = default_params
            PairwiseModel.infodoc_params(2) = default_params
        End If

        If PairwiseModel.infodoc_params(3) = "" Then
            PairwiseModel.infodoc_params(3) = GeckoClass.getCommonParams(PairwiseModel.infodoc_params(4))
        End If

        If PairwiseModel.infodoc_params(4) = "" Then
            PairwiseModel.infodoc_params(4) = GeckoClass.getCommonParams(PairwiseModel.infodoc_params(3))
        End If

        If PairwiseModel.infodoc_params(3) = "" OrElse PairwiseModel.infodoc_params(4) = "" Then
            GeckoClass.SetInfodocParams(FirstNode.NodeGuidID, ParentNode.NodeGuidID, default_params)
            GeckoClass.SetInfodocParams(SecondNode.NodeGuidID, ParentNode.NodeGuidID, default_params)
            PairwiseModel.infodoc_params(3) = default_params
            PairwiseModel.infodoc_params(4) = default_params
        End If

        Dim debug_test = PairwiseModel.infodoc_params
        PairwiseModel.ParentNodeID = PWData.ParentNodeID

        Return PairwiseModel
    End Function

    Public Sub bindHtml(ByVal model As AnytimeOutputModel)
        If model IsNot Nothing Then
            Dim html As StringBuilder = New StringBuilder()
            Dim dta = JsonConvert.SerializeObject(model.PipeParameters)
            Dim dta1 = JObject.Parse(dta)
            Dim m As Match
            Dim imstyl As String = ""
            Dim onlyimg As String = ""
            html.Append("<div class='removeB divHeader'>")
            'html.Append("<div class='page_heading_section'>")
            html.Append("<div class='page_heading_section'>")
            html.Append("<div class='container'>")
            html.Append("<div class='row'>")
            html.Append("<div class='col-md-6'>")
            Dim model_text As String = HttpUtility.UrlEncode(model.cluster_phrase.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
            Dim screenCheck As ScreenCheck = New ScreenCheck()
            html.Append($"<div class='heading_content'><div class='page-title-box' id='titleDiv'><div class='heading_icons'>")
            If Not (screenCheck.isMobileBrowserClient(Request)) Then
                html.Append($"<a {If(model.isPipeViewOnly, " class='d-none'", " class='editsvg_icon'")}  href='javascript:void(0);' onclick=showHeaderPopup(decodeURIComponent({ChrW(&H22) + model_text + ChrW(&H22)}),'0','0','0','0',null,'question-node',decodeURIComponent({ChrW(&H22) + model_text + ChrW(&H22)}))><img class='chkEvaluatorone' src='../../img/icon/edit-icon.svg'></a>")
            End If
            html.Append("<asp:Label ID='lblTask' runat='server' ><span id='divT2S' style='font-size:20px; margin-left:1ex border: 0px !important; background: #f0f0ff08 !important; display:none'></span></asp:Label></div>")
            html.Append($"<div class='txtStepTask removep'  id='MainHeaderInfodoc'>{model.step_task}<span class='threedoticon d-none' id='btnheadinfo'> <a href='#' onclick='showheaderpopup()' ><div class='snippet'><div class='stage'><div class='dot-falling'></div> </div></div></a></span></div>")

            'html.Append("<div class='heading_info'><img src='../../img/icon/empty_info.svg' class='d-none'><img src='../../img/icon/info_data.svg'><img src='../../img/icon/info-close.svg' class='d-none'></div>")
            'If model.parent_node_info Is Nothing Or model.parent_node_info = "" Then
            '    html.Append($"<div class='heading_info' id='HeaderDivIcon'><a href ='javascript:void(0);'><img  id='img1' src='../../img/icon/empty_info.svg'><img id='img2' src='../../img/icon/info_data.svg' style='display: none;'><img id='img3' src='../../img/icon/info-close.svg' style='display: none;'></a></div>")
            '    IsHideHeaderInfo = True
            'Else
            '    html.Append($"<div class='heading_info' id='HeaderDivIcon'><a href ='javascript:void(0);'><img  id='img1' src='../../img/icon/empty_info.svg' style='display: none;'><img id='img2' src='../../img/icon/info_data.svg'><img id='img3' src='../../img/icon/info-close.svg' style='display: none;'></a></div>")
            'End If
            Dim NoInfoDocDataCls As String = ""
            If model.parent_node_info.ToString().Trim() = "" Then
                NoInfoDocDataCls = "NoInfoData"
            End If
            html.Append("</div>")
            'html.Append("</div>")
            html.Append("</div>")
            html.Append("</div>")
            html.Append($"<div Class='col-md-6 { NoInfoDocDataCls}'>")
            html.Append($"<div Class='info_content d-flex mt-lg-0 mt-2' id='Header_InfoDocs'><div class='heading_icons heading_info me-0'><div  id='HeaderDivIcon'><a href='#'><img  id='img1' src='../../img/icon/empty_info.svg' style='display: none;'><img id='img2' src='../../img/icon/info_data.svg'><img id='img3' src='../../img/icon/info-close.svg' style='display: none;'></a></div><div class='infodoc_icons'><a class='editsvg_icon' href='javascript:void(0);' onclick=showInfoPopup(decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.parent_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}),'0','-1','0','-1',null,'parent-node',decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.parent_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}))><img class='chkEvaluatorone' src='../../img/icon/edit-icon.svg'></a><a href='#' onclick=Expandtxt(decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.parent_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}),'0','-1','0','-1',null,'parent-node',decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.parent_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)})) data-bs-toggle='modal' data-bs-target='#infodocPop'><img src='../../img/icon/full-screen-svgrepo-com.svg' id='ibtnfullScreen'></a></div></div>")
            html.Append("<div Class='info_content_wrapper'>")
            html.Append("<div Class='info_text removep1'>")
            Dim parent_node_info = If(model.parent_node_info IsNot Nothing AndAlso model.parent_node_info <> "", Regex.Replace(model.parent_node_info, "<.b>", String.Empty), "")
            parent_node_info = Regex.Replace(parent_node_info, "<b>", String.Empty)
            parent_node_info = HttpUtility.UrlEncode(parent_node_info.Trim().Replace(ChrW(&H22), ChrW(&H27)))
            model_text = HttpUtility.UrlEncode(model.parent_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
            Dim model_text_info As String = ""
            model_text_info = HttpUtility.UrlEncode(model.parent_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))

            'Dim regext As New Text.RegularExpressions.Regex("<.*?>", RegexOptions.Singleline)
            'Dim plantext As String = regext.Replace(parent_node_info, String.Empty)
            html.Append($"{model.parent_node_info }")
            html.Append("</div>")

            html.Append("<div Class='info_box_icons d-none'>")
            html.Append($"<a href='javascript:void(0);' Class='editsvg_icon'><img class='chkEvaluatorone' src='../../img/icon/edit-icon.svg' onclick=showInfoPopup(decodeURIComponent({ChrW(&H22) + model_text + ChrW(&H22)}),null,'-1','0','-1',null,'parent-node',decodeURIComponent({ChrW(&H22) + parent_node_info + ChrW(&H22)}))></a>")
            html.Append($"<a href='javascript:void(0);' data-bs-toggle='modal' data-bs-target='#infodocPop' onclick=Expandtxt(decodeURIComponent({ChrW(&H22) + model_text + ChrW(&H22)}),null,'-1','0','-1',null,'parent-node',decodeURIComponent({ChrW(&H22) + parent_node_info + ChrW(&H22)}))><img src='../../img/icon/full-screen-svgrepo-com.svg'></a>")
            html.Append("</div>")
            html.Append("</div>")

            html.Append("</div>")
            html.Append("</div>")
            html.Append("</div>")
            html.Append("</div>")
            ''page_heading_section End
            html.Append("</div>")
            html.Append("<div class='container'>")
            html.Append("<div class='row'>")

            'Dim parent_node_info = If(model.parent_node_info IsNot Nothing AndAlso model.parent_node_info <> "", Regex.Replace(model.parent_node_info, "<.b>", String.Empty), "")
            'parent_node_info = Regex.Replace(parent_node_info, "<b>", String.Empty)
            'parent_node
            'html.Append($"<div id='divparentbox' class='col-md-12 {If(model.parent_node_info IsNot Nothing AndAlso model.parent_node_info <> "", "", "chkEvaluator")}'>")
            'html.Append("<div class='top_content_wrapper'>")
            'html.Append($"<input type='checkbox' class='toggle_checkbox me-2 checkboxes hide_infodoc_tooltip' data-val='Parent' id='checkboxesParent'>")
            'If model.parent_node_info Is Nothing Or model.parent_node_info = "" Then
            '    '''''''html.Append($"<i Class='fa fa-plus-square mb-0 mb-lg-4' id='checkBoxIcon6' aria-hidden='true'></i><span class='ms-3'>{model.parent_node}</span> <i class='fa fa-info-circle'></i>")
            '    html.Append($"<i Class='fa fa-plus-square mb-0 mb-lg-3 hide_infodoc_tooltip{If((model_text_info) <> "" And (model_text_info) <> "NaN", "", " chkEvaluator")}' id='checkBoxIconParent' aria-hidden='true'></i>")
            '    html.Append($"<i class='fa fa-info-circle chkEvaluator1 show_infodoc_tooltip hideinfoicon{If(model.is_infodoc_tooltip, " d-none", " d-none")}' onclick=updateRightSideComment('0','tooltip','open',event) aria-hidden='true'></i>")
            '    If Not model.hideInfoDocCaptions Then
            '        html.Append($"<span class='ms-3 wrt_content_toggle'>{model.parent_node}</span>")
            '    End If
            '    ''''''html.Append($"<span class='ms-3 wrt_content_toggle'>{model.parent_node}</span>")

            '    html.Append($"<div class='tooltip_wrapper' id='tooltip_wrapper_0' style='display: none;'>")
            '    html.Append("<div class='tooltip-header'>")
            '    html.Append($"<span>{model.parent_node}</span>")
            '    html.Append($"<div class='action_icons'>")

            '    html.Append($"<a href='javascript:void(0);'><i class='bx bxs-edit-alt chkEvaluator d-none' onclick=showInfoPopup(decodeURIComponent({ChrW(&H22) + model_text_info + ChrW(&H22)}),null,'-1','0','-1',null,'parent-node',decodeURIComponent({ChrW(&H22) + model_text + ChrW(&H22)}))></i></a>")
            '    html.Append($"<a href='javascript:void(0);'><i class='fa fa-times' aria-hidden='true' onclick=updateRightSideComment('0','tooltip','close',event)></i></a>")
            '    html.Append("</div>")
            '    html.Append("</div>")
            '    html.Append($"<p class='border-top font-size-12 pt-2 text-start mb-0'>{parent_node_info}</p>")
            '    html.Append("</div>")

            '    html.Append("<div class='border justify-content-between mb-3 px-3 pt-3 hide_infodoc_tooltip_div box-maxht parent-node-info-div columns tt-accordion-content tg-accordion-0 tg-accordion-sub-0' style='display:none;' id='checkout-shipping-address-Parent'>")
            'Else
            '    html.Append($"<i Class='fa fa-minus-square mb-0 mb-lg-3 hide_infodoc_tooltip' id='checkBoxIconParent' aria-hidden='true'></i>")
            '    html.Append($"<i class='fa fa-info-circle chkEvaluator1 show_infodoc_tooltip hideinfoicon{If(model.is_infodoc_tooltip, " d-none", " d-none")}' onclick=updateRightSideComment('0','tooltip','open',event) aria-hidden='true'></i>")
            '    If Not model.hideInfoDocCaptions Then
            '        html.Append($"<span class='ms-3 wrt_content_toggle'>{model.parent_node}</span>")
            '    End If

            '    html.Append($"<div class='tooltip_wrapper' id='tooltip_wrapper_0' style='display: none;'>")
            '    html.Append("<div class='tooltip-header'>")
            '    html.Append($"<span>{model.parent_node}</span>")
            '    html.Append($"<div class='action_icons'>")

            '    html.Append($"<a href='javascript:void(0);'><i class='bx bxs-edit-alt chkEvaluator d-none' onclick=showInfoPopup(decodeURIComponent({ChrW(&H22) + model_text_info + ChrW(&H22)}),null,'-1','0','-1',null,'parent-node',decodeURIComponent({ChrW(&H22) + model_text + ChrW(&H22)}))></i></a>")
            '    html.Append($"<a href='javascript:void(0);'><i class='fa fa-times' aria-hidden='true' onclick=updateRightSideComment('0','tooltip','close',event)></i></a>")
            '    html.Append("</div>")
            '    html.Append("</div>")
            '    html.Append($"<p class='border-top font-size-12 pt-2 text-start mb-0'>{parent_node_info}</p>")
            '    html.Append("</div>")

            '    html.Append("<div class='border justify-content-between mb-3 px-3 pt-3 hide_infodoc_tooltip_div box-maxht parent-node-info-div columns tt-accordion-content tg-accordion-0 tg-accordion-sub-0' style='display:flex;' id='checkout-shipping-address-Parent'>")
            'End If
            'If model.parent_node_info = "" Or model.parent_node_info Is Nothing Then
            '    html.Append($"<div class='divinfocheck'><p>{parent_node_info}</p></div>")
            'Else
            '    html.Append($"<div><p>{parent_node_info}</p> </div>")
            'End If

            'html.Append($"<div><a href='javascript:void(0);'> <img class='chkEvaluatorone' src='../../img/icon/edit-icon.svg' onclick=showInfoPopup(decodeURIComponent({ChrW(&H22) + model_text + ChrW(&H22)}),null,'-1','0','-1',null,'parent-node',decodeURIComponent({ChrW(&H22) + model_text_info + ChrW(&H22)}))>")
            '''''''''''''html.Append("<i class='bx bxs-edit chkEvaluator d-none' aria-hidden='true'></i>")
            'html.Append("</a></div>")
            'html.Append("</div>")
            'html.Append("</div>")
            'html.Append("</div>")
            m = Regex.Match(model.first_node_info.ToString().Trim(), "<img")
            If m.Success Then
                imstyl = " image_main_wrapper"
                onlyimg = " only_img"
            End If

            '==================Right Column==========================
            html.Append("<div class='col-md-12'>")
            html.Append("<div class='chart_wrapper active_row single_row pairwise_width removeB set_width_pair sliderBackground graphical_data verbal_grphical_comment'> ")
            Dim filter_gray As String = ""
            'first node
            model_text = HttpUtility.UrlEncode(model.first_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
            model_text_info = HttpUtility.UrlEncode(model.first_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
            'model_text_info = HttpUtility.UrlEncode(model.wrt_first_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
            Dim wrt_header_text As String = HttpUtility.UrlEncode(model.parent_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
            NoInfoDocDataCls = ""
            If (model.first_node_info.ToString().Trim() <> "" Or model.wrt_first_node_info.ToString().Trim() <> "") Then
                If (model.first_node_info.ToString().Trim() = "" Or model.wrt_first_node_info.ToString().Trim() = "") Then
                    html.Append($"<div class='value_wrapper left_info nodataonesideinfodocrow{imstyl}'><div class='info_data selected'>")
                Else
                    html.Append($"<div class='value_wrapper left_info{imstyl}'><div class='info_data selected'>")
                End If
            Else
                NoInfoDocDataCls = "NoInfoData"
                filter_gray = "filter_gray"
                html.Append($"<div class='value_wrapper left_info emptyInfodocbothside{imstyl}'><div class='info_data selected'>")
            End If
            html.Append($"<div class='info_header'><a class='{filter_gray} {NoInfoDocDataCls}'><img src='../../img/icon/info-close.svg' class='icon'></a><span class='title_tag text-uppercase' title='{model.first_node.ToString().Trim()}'> {model.first_node.ToString().Trim()}</span>
     <div class='header_edit_fw {NoInfoDocDataCls}'> <a href='#' data-bs-toggle='modal' data-bs-target='#exampleModal' onclick='SetExpendedPopupElement(decodeURIComponent(" + ChrW(&H22) + model_text_info + ChrW(&H22) + "),null,2,0,1,null,decodeURIComponent(" + ChrW(&H22) + "left-node" + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + model_text + ChrW(&H22) + "),null,null,decodeURIComponent(" + ChrW(&H22) + HttpUtility.UrlEncode(model.wrt_first_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22) + "),null,3,0,1,null,decodeURIComponent(" + ChrW(&H22) + "wrt-left-node1" + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + HttpUtility.UrlEncode(model.first_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + wrt_header_text + ChrW(&H22) + "))'><img src='../../img/icon/full-screen-svgrepo-com.svg '></a>")
            'html.Append($"<a href='#' onclick='showInfoPopup(decodeURIComponent(" + ChrW(&H22) + model_text + ChrW(&H22) + "),null,2,0,1,null,decodeURIComponent(" + ChrW(&H22) + "left-node" + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + model_text_info + ChrW(&H22) + "))' class='d-md-block d-none ms-2'><img class='chkEvaluatorone' src='../../img/icon/edit-icon.svg' class='icon'></a>")
            html.Append("</div></div>")
            If (model.first_node_info.ToString().Trim() <> "") Then
                html.Append($"<div Class='body_content removep3{onlyimg}'><span>{model.first_node_info.ToString()}</span></div>")
            Else
                html.Append("<div Class='body_content removep3 NoInfoData pwnodata'><div class='multiinfodata'><h2>No Data</h2></div></div>")
            End If
            If (model.wrt_first_node_info.ToString().Trim() <> "") Then
                html.Append($"<div class='wrt_content'><div class='body_content'><h2>WITH RESPECT TO <span>{model.parent_node}</span></h2><span>{model.wrt_first_node_info.ToString()}</span>")
            Else
                html.Append($"<div class='wrt_content NoInfoData'><div class='body_content'><h2>WITH RESPECT TO <span>{model.parent_node}</span></h2><div class='multiinfodata'><h3>No Data</h3> </div>")
            End If
            html.Append("</div></div>")


            'html.Append("<div class='value_wrapper left_info'>")
            'html.Append("<div Class='info_data selected'>")

            'html.Append("<div Class='info_header'>")

            'html.Append($"<span Class='title_tag text-uppercase'>{model.first_node}</span><a href ='javascript:void(0);' Class='d-md-block d-none'><img class='chkEvaluatorone' src='../../img/icon/edit-icon.svg' onclick='showInfoPopup(decodeURIComponent(" + ChrW(&H22) + model_text + ChrW(&H22) + "),null,2,0,1,null,decodeURIComponent(" + ChrW(&H22) + "left-node" + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + model_text_info + ChrW(&H22) + "))'></a></div>")


            'html.Append("<div Class='body_content removep3'>")
            'If model.first_node_info.ToString().Trim().Length < 120 Then
            '    html.Append($"<span>{model.first_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))}</span>")
            'Else
            '    html.Append($"<span>{model.first_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)).Substring(0, 120)} ...</span>")
            'End If
            ''html.Append($"<span>{model.first_node_info}</span>")
            'Dim wrt As String = model.first_node.ToString()
            'If (model.first_node_info.ToString().Trim() = "") Then
            '    html.Append("<div Class='no-data_content'>")
            '    html.Append("<h2> No Data</h2>")

            '    html.Append($"<a href='javascript:void(0);' class='chkEvaluatorone' data-bs-toggle='modal' data-bs-target='#exampleModal' onclick='SetExpendedPopupElement(decodeURIComponent(" + ChrW(&H22) + model_text_info + ChrW(&H22) + "),null,2,0,1,null,decodeURIComponent(" + ChrW(&H22) + "left-node" + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + model_text + ChrW(&H22) + "),null,null,decodeURIComponent(" + ChrW(&H22) + HttpUtility.UrlEncode(model.wrt_first_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22) + "),null,3,0,1,null,decodeURIComponent(" + ChrW(&H22) + "wrt-left-node1" + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + HttpUtility.UrlEncode(model.first_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + wrt_header_text + ChrW(&H22) + "))'> +<u> Add Data</u></a></div>")
            '    html.Append("</div>")
            'Else
            '    html.Append("</div>")
            'End If
            'html.Append("</div>")
            'html.Append("<div Class='info_datafooter'>")

            'html.Append($"<Button type='button' {If((model.wrt_first_node_info.ToString().Trim()) <> "", " class=''", " class='nowrt_data' ")}  data-bs-toggle='modal' data-bs-target='#exampleModal' onclick='SetExpendedPopupElement(decodeURIComponent(" + ChrW(&H22) + model_text_info + ChrW(&H22) + "),null,2,0,1,null,decodeURIComponent(" + ChrW(&H22) + "left-node" + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + model_text + ChrW(&H22) + "),null,null,decodeURIComponent(" + ChrW(&H22) + HttpUtility.UrlEncode(model.wrt_first_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22) + "),null,3,0,1,null,decodeURIComponent(" + ChrW(&H22) + "wrt-left-node1" + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + HttpUtility.UrlEncode(model.first_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + wrt_header_text + ChrW(&H22) + "))'>With respect To <img src='../../img/icon/button_arrow.svg'></button>")

            'html.Append($"<div class='modal fade' id='exampleModal' tabindex='-1' aria-labelledby='exampleModalLabel' aria-hidden='True'><div class='modal-dialog modal-fullscreen'><div class='modal-content'><div class='modal-header'><h3 class='modal-title'id='exampleModalLabel' style='color:white;'>INFODOCS</h3><button type='button' class='btn-close' data-bs-dismiss='modal' aria-label='Close'></button></div><div class='modal-body'><div class='row h-100'><div class='col-md-6 border-End'><div class='modal_info_header'><h3>{model.first_node}</h3><a href='#' ><img src='../../img/icon/edit-icon.svg' '></a></div>")
            'html.Append($"<div class='modal_info_content removep1'><p>{model.first_node_info}</p></div></div><div class='col-md-6'><div class='modal_info_header'><h3>{model.first_node + " WRT " + model.first_node}</h3><a href='#'><img src='../../img/icon/edit-icon.svg' ></a></div><div class='modal_info_content'><p>" + model.first_node_info + "</p></div></div></div></div></div></div></div>")

            'html.Append("</div>")
            html.Append("</div>")
            html.Append("<div class='w-100 d-none'>")
            html.Append("<div class='top_content_wrapper' >")
            html.Append("<div class=' blue_bgcolor p-3 text-center text-light clcheckboxes setDivMaxHeight5 align-self-start d-flex align-items-center position-relative resizepane' id='divFirstNode' style='height: auto;'>")
            'html.Append($"<input type='checkbox' class='toggle_checkbox me-2 checkchange hide_infodoc_tooltip' id='checkboxes7'> <i class='fa fa-plus-square{If(model.first_node_info IsNot Nothing AndAlso model.first_node_info <> "", "", " chkEvaluator")} hide_infodoc_tooltip' id='checkBoxIcon7' aria-hidden='true'></i>")
            html.Append($"<span class='ms-3 blue_bgcolor'>{model.first_node}</span>")

            html.Append($"<div class='tooltip_wrapper' id='tooltip_wrapper_1' style='display: none;'>")
            html.Append("<div class='tooltip-header'>")
            html.Append($"<span>{model.first_node}</span>")
            html.Append($"<div class='action_icons'>")

            html.Append($"<a href='javascript:void(0);'><i class='bx bxs-edit-alt chkEvaluator d-none' onclick='showInfoPopup(decodeURIComponent(" + ChrW(&H22) + model_text_info + ChrW(&H22) + "),null,2,0,1,null,decodeURIComponent(" + ChrW(&H22) + "left-node" + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + model_text + ChrW(&H22) + "))'></i></a>")
            html.Append($"<a href='javascript:void(0);'><i class='fa fa-times' aria-hidden='true' onclick=updateRightSideComment('1','tooltip','close',event)></i></a>")
            html.Append("</div>")
            html.Append("</div>")
            html.Append($"<p class='border-top font-size-12 pt-2 text-start mb-0'>{model.first_node_info}</p>")
            html.Append("</div>")

            html.Append("</div>")
            'html.Append($"<div class='toggle_area hide_infodoc_tooltip_div checkout-shipping-address5 {If((model.second_node_info) <> "" And (model.second_node_info) <> "NaN", "", "chkEvaluator")} resizepane' style='display:none;' id='checkout-shipping-address-SecondNode'>")

            html.Append($"<div class='my-3 position-relative{If((model.first_node_info) <> "" And (model.first_node_info) <> "NaN", "", " chkEvaluator")}' id='dvfirstnodelbl'><input type='checkbox' class='toggle_checkbox me-2 checkchange checkboxes5 checkboxes hide_infodoc_tooltip {If((model.first_node_info) <> "" And (model.first_node_info) <> "NaN", "", " chkEvaluator")}' data-val='FirstNode' id='checkboxesFirstNode'> <i class='hide_infodoc_tooltip checkBoxIcon5 mt-1 {If(model_text_info <> "" And model_text_info <> "NaN", "fa fa-minus-square ", "fa fa-plus-square chkEvaluator")}' id='checkBoxIconFirstNode'  aria-hidden='true'></i>")
            html.Append($"<i class='fa fa-info-circle chkEvaluator1 show_infodoc_tooltip hideinfoicon mt-1{If(model.is_infodoc_tooltip, " d-none", " d-none")}' onclick=updateRightSideComment('1','tooltip','open',event) aria-hidden='true'></i>")

            html.Append($"<span class='ms-2 wlr_heading'>{model.first_node}</span></div>")

            html.Append($"<div class='left-node-info-div tt-resizable-panel-1  toggle_area hide_infodoc_tooltip_div checkout-shipping-address5{If((model.first_node_info) <> "" And (model.first_node_info) <> "NaN", "", " chkEvaluator")} resizepane left-node-info-div columns tt-accordion-content tg-accordion-sub-1'{If((model.first_node_info) <> "" And (model.first_node_info) <> "NaN", "", " style='display:none;'")} id='checkout-shipping-address-FirstNode'>")
            html.Append($"<p>{model.first_node_info}</p>")

            'html.Append($"<a href='javascript:void(0);' class='edit_icon chkEvaluator' onclick=showInfoPopup(decodeURIComponent({ChrW(&H22) + model_text + ChrW(&H22)}),null,'2','0','1',null,'left-node',decodeURIComponent(" + ChrW(&H22) + model_text_info + ChrW(&H22) + "))>")
            html.Append($"<a href='javascript:void(0);' class='edit_icon chkEvaluator' onclick='showInfoPopup(decodeURIComponent(" + ChrW(&H22) + model_text_info + ChrW(&H22) + "),null,2,0,1,null,decodeURIComponent(" + ChrW(&H22) + "left-node" + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + model_text + ChrW(&H22) + "))'>")
            html.Append("<i class='bx bxs-edit chkEvaluator d-none' aria-hidden='true'></i>")
            html.Append("</a>")
            html.Append("</div>")
            html.Append("</div>")

            'wrt_first_node_info
            model_text = HttpUtility.UrlEncode(model.first_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
            model_text_info = HttpUtility.UrlEncode(model.wrt_first_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))

            html.Append($"<div class='top_content_wrapper mt-3  {If(model.wrt_first_node_info IsNot Nothing AndAlso model.wrt_first_node_info <> "", "", "chkEvaluator")}' id='dvfirst_node'>")
            html.Append($"<div class='wrapper_c_1 resizepane' id='idwrapper_c_1'><input type='checkbox' class='toggle_checkbox me-2 hide_infodoc_tooltip checkboxes checkboxes1' data-val='FirstNodeWRT' id='checkboxesFirstNodeWRT'> <i class='mb-0 mb-lg-4 checkBoxIcon1 hide_infodoc_tooltip {If(model_text_info <> "" And model_text_info <> "NaN", "fa fa-minus-square ", "fa fa-plus-square chkEvaluator")}' id='checkBoxIconFirstNodeWRT' aria-hidden='true'></i>")
            html.Append($"<i class='fa fa-info-circle chkEvaluator1 show_infodoc_tooltip hideinfoicon{If(model.is_infodoc_tooltip, " d-none", " d-none")}' onclick=updateRightSideComment('2','tooltip','open',event) aria-hidden='true'></i>")
            If Not model.hideInfoDocCaptions Then
                html.Append($"<span class='wlr_heading' title='{model.first_node} WRT {model.parent_node}'>&nbsp;")
                html.Append($" {If(model.first_node IsNot Nothing And model.first_node.Length > 14, model.first_node.Substring(0, 15) + "...", model.first_node)}")
                html.Append($" WRT {If(model.parent_node IsNot Nothing And model.parent_node.Length > 14, model.parent_node.Substring(0, 15) + "...", model.parent_node)}")
                html.Append("</span>")
            End If

            'html.Append($"<span class='wlr_heading' title='{model.first_node} WRT {model.parent_node}'>&nbsp;")
            'html.Append($" {If(model.first_node IsNot Nothing And model.first_node.Length > 14, model.first_node.Substring(0, 15) + "...", model.first_node)}")
            'html.Append($" WRT {If(model.parent_node IsNot Nothing And model.parent_node.Length > 14, model.parent_node.Substring(0, 15) + "...", model.parent_node)}")
            'html.Append("</span>")


            html.Append($"<div class='tooltip_wrapper' id='tooltip_wrapper_2' style='display: none;'>")
            html.Append("<div class='tooltip-header'>")
            html.Append($"<span>{model.first_node}</span>")
            html.Append($"<div class='action_icons'>")

            html.Append($"<a href='javascript:void(0);'><i class='bx bxs-edit-alt chkEvaluator d-none' onclick='showInfoPopup(decodeURIComponent(" + ChrW(&H22) + model_text_info + ChrW(&H22) + "),null,3,0,1,null,decodeURIComponent(" + ChrW(&H22) + "wrt-left-node1" + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + model_text + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + wrt_header_text + ChrW(&H22) + "))'></i></a>")
            html.Append($"<a href='javascript:void(0);'><i class='fa fa-times' aria-hidden='true' onclick=updateRightSideComment('2','tooltip','close',event)></i></a>")
            html.Append("</div>")
            html.Append("</div>")
            html.Append($"<p class='border-top font-size-12 pt-2 text-start mb-0'>{model.wrt_first_node_info}</p>")
            html.Append("</div></div>")


            html.Append($"<div class='wrt-left-node-info-div tt-resizable-panel-2 toggle_area small_content hide_infodoc_tooltip_div checkout-shipping-address1 columns tt-accordion-content tg-accordion-sub-3 resizepane'{If((model.wrt_first_node_info) <> "" And (model.wrt_first_node_info) <> "NaN", "", " style='display:block;'")} id='checkout-shipping-address-FirstNodeWRT'>")
            html.Append($"<p class='font-size-14 me-3'>{model.wrt_first_node_info}</p>")

            'html.Append($"<a href='javascript:void(0);' class='edit_icon chkEvaluator' onclick=showInfoPopup(decodeURIComponent({ChrW(&H22) + model_text + ChrW(&H22)}),null,'3','0','1',null,'wrt-left-node',decodeURIComponent({ChrW(&H22) + model_text_info + ChrW(&H22)}),decodeURIComponent({ChrW(&H22) + wrt_header_text + ChrW(&H22)}))>")
            html.Append($"<a href='javascript:void(0);' class='edit_icon chkEvaluator' onclick='showInfoPopup(decodeURIComponent(" + ChrW(&H22) + model_text_info + ChrW(&H22) + "),null,3,0,1,null,decodeURIComponent(" + ChrW(&H22) + "wrt-left-node1" + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + model_text + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + wrt_header_text + ChrW(&H22) + "))'>")
            html.Append("<i class='bx bxs-edit chkEvaluator d-none' aria-hidden='true'></i>")
            html.Append("</a>")
            html.Append("</div>")
            html.Append("</div>")
            html.Append("</div>")
            html.Append("</div>")
            'html.Append($"<div style='height:200px; width:200px; border:2px' id='abc'></div>")
            'html.Append($"<div class='tt-resizable-panel-2 toggle_area small_content hide_infodoc_tooltip_div checkout-shipping-address1 columns tt-accordion-content tg-accordion-sub-3'{If((model.wrt_first_node_info) <> "" And (model.wrt_first_node_info) <> "NaN", "", " style='display:none;'")} id='checkout-shipping-address-FirstNodeWRT'></div>")

            '=============================================

            'rating area
            If model.pairwise_type = "ptVerbal" Then
                Dim mValue As String = If(model.value >= 9, "Extreme", If(model.value >= 8, "Very Strong to Extreme", If(model.value >= 7, "Very Strong", If(model.value >= 6, "Strong to Very Strong", If(model.value >= 5, "Strong", If(model.value >= 4, "Moderate to Strong", If(model.value >= 3, "Moderate", If(model.value >= 2, "Equal to Moderate", If(model.value >= 1, "Equal", "")))))))))
                html.Append($"<div class='rating_area single_pairwise_resp p-0'>")
                html.Append("<div class='chart_area m-auto mb-4'>")
                'html.Append($"<div class='rating_result'{If(model.isPipeViewOnly, " style='pointer-events:none;'", "")}>")

                'html.Append("<div  class='left_result'>")
                'If (model.advantage = 1) Then
                '    html.Append($"<span id='divleft' class='spnleft'>{mValue}</span>")
                'Else
                '    html.Append("<span id='divleft' style='display: none;' class='spnleft'></span>")
                'End If
                'html.Append("</div>")

                'html.Append("<div class='equalizer'>")
                'If (model.advantage = 0 And mValue IsNot "") Then
                '    html.Append($"<span  id='divequal'  class='spnequal'>{mValue}</span>")
                'Else
                '    html.Append("<span  id='divequal'  style='display: none;' class='spnequal'></span>")
                'End If
                'html.Append("</div>")

                'html.Append("<div  class='right_result'>")
                'If (model.advantage = -1) Then
                '    html.Append($"<span id='divright'  class='spnright'>{mValue}</span>")
                'Else
                '    html.Append("<span id='divright' style='display: none;' class='spnright'></span>")
                'End If

                'html.Append("</div>")

                'html.Append("</div>")
                html.Append($"<div class='ratting_box'{If(model.isPipeViewOnly, " style='pointer-events:none;'", "")}>")
                Dim chartClass As String = ""
                chartClass = If(model.advantage = 1 And model.value >= 9, If(model.value = 9, " selected_left1 selcted_spot", " selected_left1"), "")
                html.Append($"<div id='div_rating_left_9' class='rating_EX rating_content rating_content_left{chartClass}' title='Extreme'  onclick=save_pairwise('9','1','left',this)>")
                html.Append("<span>EX</span><div class='arrow bounce'></div>")
                html.Append("</div>")
                chartClass = If(model.advantage = 1 And model.value >= 8, If(model.value = 8, " selected_left1 selcted_spot", " selected_left1"), "")
                html.Append($"<div id='div_rating_left_8' class='rating_EX_s rating_content rating_content_left{chartClass}' title='Very Strong to Extreme'  onclick=save_pairwise('8','1','left',this)><div class='arrow bounce'></div>")
                html.Append("</div>")
                chartClass = If(model.advantage = 1 And model.value >= 7, If(model.value = 7, " selected_left1 selcted_spot", " selected_left1"), "")
                html.Append($"<div id='div_rating_left_7' class='rating_VS rating_content rating_content_left{chartClass}' title='Very Strong'  onclick=save_pairwise('7','1','left',this)>")
                html.Append("<span>VS</span><div class='arrow bounce'></div>")
                html.Append("</div>")
                chartClass = If(model.advantage = 1 And model.value >= 6, If(model.value = 6, " selected_left1 selcted_spot", " selected_left1"), "")
                html.Append($"<div id='div_rating_left_6' class='rating_VS_s rating_content rating_content_left{chartClass}' title='Strong to Very Strong'   onclick=save_pairwise('6','1','left',this)><div class='arrow bounce'></div>")
                html.Append("</div>")
                chartClass = If(model.advantage = 1 And model.value >= 5, If(model.value = 5, " selected_left1 selcted_spot", " selected_left1"), "")
                html.Append($"<div id='div_rating_left_5' class='rating_S rating_content rating_content_left{chartClass}' title='Strong' onclick=save_pairwise('5','1','left',this)>")
                html.Append("<span>S</span><div class='arrow bounce'></div>")
                html.Append("</div>")
                chartClass = If(model.advantage = 1 And model.value >= 4, If(model.value = 4, " selected_left1 selcted_spot", " selected_left1"), "")
                html.Append($"<div id='div_rating_left_4' class='rating_S_s rating_content rating_content_left{chartClass}' title='Moderate to Strong' onclick=save_pairwise('4','1','left',this)><div class='arrow bounce'></div>")
                html.Append("</div>")
                chartClass = If(model.advantage = 1 And model.value >= 3, If(model.value = 3, " selected_left1 selcted_spot", " selected_left1"), "")
                html.Append($"<div id='div_rating_left_3' class='rating_M rating_content rating_content_left{chartClass}' title='Moderate' onclick=save_pairwise('3','1','left',this)>")
                html.Append("<span>M</span><div class='arrow bounce'></div>")
                html.Append("</div>")
                chartClass = If(model.advantage = 1 And model.value >= 2, If(model.value = 2, " selected_left1 selcted_spot", " selected_left1"), "")
                html.Append($"<div id='div_rating_left_2' class='rating_M_e rating_content rating_content_left{chartClass}' title='Equal to Moderate' onclick=save_pairwise('2','1','left',this)><div class='arrow bounce'></div>")
                html.Append("</div>")
                If model.advantage = 1 And model.value >= 1 Then
                    chartClass = "rating_content_left selected_left1"
                ElseIf model.advantage = -1 And model.value >= 1 Then
                    chartClass = "rating_content_right selected_right1"
                ElseIf model.advantage = 0 And model.value <= 0 Then
                    chartClass = ""
                Else
                    chartClass = "eqalizer_color selcted_spot"
                End If
                'chartClass = If(model.advantage = 1 And model.value >= 1, "rating_content_left selected_left1", "rating_content_right selected_right1")
                html.Append($"<div id='divRatingEQ' class='rating_e rating_content {chartClass}' title='Equal' onclick=save_pairwise('1','0','',this)>")
                html.Append("<span>EQ</span><div class='arrow bounce '></div>")
                html.Append("</div>")

                'If model.advantage = -1 And model.value >= 2 Then
                chartClass = If(model.advantage = -1 And model.value >= 2, If(model.value = 2, " selected_right1 selcted_spot", " selected_right1"), "")
                html.Append($"<div id='div_rating_right_2' class='rating_M_e rating_content rating_content_right{chartClass}' title='Equal to Moderate' onclick=save_pairwise('2','-1','right',this)><div class='arrow bounce arrowgreen'></div>")
                html.Append("</div>")
                chartClass = If(model.advantage = -1 And model.value >= 3, If(model.value = 3, " selected_right1 selcted_spot", " selected_right1"), "")
                html.Append($"<div id='div_rating_right_3' class='rating_M rating_content rating_content_right{chartClass}' title='Moderate' onclick=save_pairwise('3','-1','right',this)>")
                html.Append("<span>M</span><div class='arrow bounce arrowgreen'></div>")
                html.Append("</div>")
                chartClass = If(model.advantage = -1 And model.value >= 4, If(model.value = 4, " selected_right1 selcted_spot", " selected_right1"), "")
                html.Append($"<div id='div_rating_right_4' class='rating_S_s rating_content rating_content_right{chartClass}' title='Moderate to Strong' onclick=save_pairwise('4','-1','right',this)><div class='arrow bounce arrowgreen'></div>")
                html.Append("</div>")
                chartClass = If(model.advantage = -1 And model.value >= 5, If(model.value = 5, " selected_right1 selcted_spot", " selected_right1"), "")
                html.Append($"<div id='div_rating_right_5' class='rating_S rating_content rating_content_right{chartClass}' title='Strong' onclick=save_pairwise('5','-1','right',this)>")
                html.Append("<span>S</span><div class='arrow bounce arrowgreen'></div>")
                html.Append("</div>")
                chartClass = If(model.advantage = -1 And model.value >= 6, If(model.value = 6, " selected_right1 selcted_spot", " selected_right1"), "")
                html.Append($"<div id='div_rating_right_6' class='rating_VS_s rating_content rating_content_right{chartClass}' title='Strong to Very Strong'  onclick=save_pairwise('6','-1','right',this)><div class='arrow bounce arrowgreen'></div>")
                html.Append("</div>")
                chartClass = If(model.advantage = -1 And model.value >= 7, If(model.value = 7, " selected_right1 selcted_spot", " selected_right1"), "")
                html.Append($"<div id='div_rating_right_7' class='rating_VS rating_content rating_content_right{chartClass}' title='Very Strong' onclick=save_pairwise('7','-1','right',this)>")
                html.Append("<span>VS</span><div class='arrow bounce arrowgreen'></div>")
                html.Append("</div>")
                chartClass = If(model.advantage = -1 And model.value >= 8, If(model.value = 8, " selected_right1 selcted_spot", " selected_right1"), "")
                html.Append($"<div id='div_rating_right_8' class='rating_EX_s rating_content rating_content_right{chartClass}' title='Very Strong to Extreme' onclick=save_pairwise('8','-1','right',this)><div class='arrow bounce arrowgreen'></div>")
                html.Append("</div>")
                chartClass = If(model.advantage = -1 And model.value >= 9, If(model.value = 9, " selected_right1 selcted_spot", " selected_right1"), "")
                html.Append($"<div id='div_rating_right_9' class='rating_EX rating_content rating_content_right{chartClass}' title='Extreme' onclick=save_pairwise('9','-1','right',this)>")
                html.Append("<span>EX</span><div class='arrow bounce arrowgreen'></div>")
                html.Append("</div>")
                html.Append("</div>")
                html.Append("</div>")
                '=========================================

                'Comment box
                html.Append("<div class='pairwise_newdesign'>")

                'html.Append("<input type='checkbox'  class='comment_model'>")
                If model.show_comments Then
                    html.Append("<div class='comment_div' style='cursor:pointer;'>")
                Else
                    html.Append("<div class='comment_div' style='color: #333333;display: none;'>")
                End If
                If (model.comment Is Nothing Or model.comment = "") Then
                    If Not (screenCheck.isMobileBrowserClient(Request)) Then
                        html.Append("<div class='comment_icon' onclick='showNode(15)'><img src = '../../img/icon/Grey-plus-icon.svg' id='ImgComment_iconEmpty_15' title='' aria-hidden='true'>  <img src = '../../img/icon/comment-svgrepo-com.svg' style='display:none;' id='ImgComment_icon_15' title='' aria-hidden='true'><span style='color: #333333'  class='tooltip_item'> Add Comment</span></div>")
                    Else
                        html.Append("<div class='comment_icon' onclick='showNodeMobile(15)'><img src = '../../img/icon/Grey-plus-icon.svg' id='ImgComment_iconEmpty_15' title='' aria-hidden='true'>  <img src = '../../img/icon/comment-svgrepo-com.svg' style='display:none;' id='ImgComment_icon_15' title='' aria-hidden='true'><span style='color: #333333'  class='tooltip_item'> Add Comment</span></div>")
                    End If
                Else
                    If Not (screenCheck.isMobileBrowserClient(Request)) Then
                        html.Append($"<div class='comment_icon' onclick='showNode(15)'><img src = '../../img/icon/Grey-plus-icon.svg' id='ImgComment_iconEmpty_15' style='display:none;' title='' aria-hidden='true'>  <img src = '../../img/icon/comment-svgrepo-com.svg' id='ImgComment_icon_15' title='{model.comment.ToString()}' aria-hidden='true'><span style='color: #333333'  class='tooltip_item'> Add Comment</span></div>")
                    Else
                        html.Append($"<div class='comment_icon' onclick='showNodeMobile(15)'><img src = '../../img/icon/Grey-plus-icon.svg' id='ImgComment_iconEmpty_15' style='display:none;' title='' aria-hidden='true'>  <img src = '../../img/icon/comment-svgrepo-com.svg' id='ImgComment_icon_15' title='{model.comment.ToString()}' aria-hidden='true'><span style='color: #333333'  class='tooltip_item'> Add Comment</span></div>")
                    End If
                End If

                    'If (model.comment Is Nothing Or model.comment = "") Then
                    '    'html.Append("<i class='far fa-comments' id='single_comment_icon' onclick='showNode(15)' aria-hidden='true'></i>")
                    '    html.Append("<div class='comment_icon' onclick='showNode(15)'><img src='../../img/icon/commentadd.svg'><span style='color: #333333'> Add Comment</span></div>")
                    'Else
                    '    'html.Append("<i Class='fa fa-comments' id='single_comment_icon' onclick='showNode(15)' title='" + model.comment + "' aria-hidden='true'></i>")
                    '    html.Append("<div class='comment_icon' onclick='showNode(15)'><img src='../../img/icon/commentadd.svg'><span style='color: #333333'> Add Comment</span></div>")
                    'End If
                    'html.Append("<i class='fa fa-comments' aria-hidden='true'></i>")

                    html.Append("<div class='info_tooltip right_tooltip' id='parentnode15' style='display:none;'>")
                html.Append("<div class='d-flex justify-content-between'>")
                html.Append("<span>Add your comment</span>")
                html.Append("<div class='action_icons'>")
                html.Append("<a href='javascript:void(0);' onclick='hideNode(15)'><i class='fa fa-times' aria-hidden='true'></i></a>")
                html.Append("</div>")
                'html.Append($"<div class='action_icons' ><a href='javascript:void(0);'><img src='../../img/icon/erasar.svg onclick='hideNode(15)' aria-hidden='true' /></a></div>")
                html.Append("</div>")
                html.Append($"<textarea{If(model.isPipeViewOnly, " style='pointer-events:none;'", "")} class='form-control mb-3 mt-2 w-100' rows='3' id='single_comment' placeholder='Add your comment'>{model.comment}</textarea>")
                html.Append("<div class='comt_btn text-end'>")
                html.Append($"<button{If(model.isPipeViewOnly, " class='d-none'", "")} type='button' onclick='hideNode(15,15); comment_updated();'><i class='fa fa-check' aria-hidden='true'></i> OK</button>")
                html.Append("</div>")
                html.Append("</div>")
                html.Append("</div>")
                If model.show_comments Then
                    html.Append($"<div{If(model.isPipeViewOnly, " style='pointer-events:none;'", "")} class='close_icon ms-3' onclick='refreshRating();'>")
                Else
                    html.Append($"<div{If(model.isPipeViewOnly, " style='pointer-events:none;'", "")} class='close_icon ms-3 text-center w-100' onclick='refreshRating();'>")
                End If

                'html.Append("<i class='fa fa-times' aria-hidden='true'></i>")

                If (model.value > 0) Then
                    html.Append("<div  id='ClearJudgment'><img src='../../img/icon/erasar.svg' class='imgClose' style='filter: none; opacity: 1;'><span class='tooltip_item'>Clear Judgment</span></div>")
                Else
                    html.Append("<div id='ClearJudgment'><img src='../../img/icon/erasar.svg' class='imgClose' style='filter: grayscale(1); opacity: 0.5;'><span class='tooltip_item'>Clear Judgment</span></div>")
                End If
                html.Append("</div>")
                html.Append("</div>")
                html.Append("</div>")
                '=============================================
            ElseIf model.pairwise_type = "ptGraphical" Then
                html.Append("<div class='rating_area'>")
                html.Append("<div class='mb-2 mt-3 scale_wt'>")
                html.Append($"<div id='CurvechartInput' class='chart_data graphical_data mb-md-5 mb-4 mt-0 curvechart_input'{If(model.isPipeViewOnly, " style='pointer-events:none;'", "")}>")

                Dim fun As String = "onkeyup=graphicalall_key_up(this,'0','-1','up',-1,'true')"
                'Dim fun As String = "onkeyup=graphical_key_up(this,'0','-1','up',null,'true')"
                'fun += " onkeydown=graphical_key_up(this,'0','-1','press',null,'true')"
                'fun += " onmousedown=graphical_key_up(this,'0','-1','press',null,'true')"
                'fun += " onmouseup=graphical_key_up(this,'0','-1','up',null,'true')"
                'fun += " onblur=graphical_key_up(this,'0','-1','up',null,'true')"
                fun += " onkeypress='return isNumberKeyWithDecimal(this, event);'"
                html.Append($"<input type='text' id='noUiInput11' {fun}>")

                html.Append($"<div class='arrowicons'{If(model.isPipeViewOnly, " style='pointer-events:none;'", "")}>")
                html.Append("<a href='javascript:void(0);' onclick='swap_value();'><img src='../../img/icon/value_exchange.svg' /></a>")
                html.Append("</div>")

                fun = "onkeyup=graphicalall_key_up(this,'1','1','up',-1,'true')"
                'fun = "onkeyup=graphical_key_up(this,'1','1','up',null,'true')"
                'fun += " onkeydown=graphical_key_up(this,'1','1','press',null,'true')"
                'fun += " onmousedown=graphical_key_up(this,'1','1','press',null,'true')"
                'fun += " onmouseup=graphical_key_up(this,'1','1','up',null,'true')"
                'fun += " onblur=graphical_key_up(this,'1','1','up',null,'true')"
                fun += " onkeypress='return isNumberKeyWithDecimal(this, event);'"
                html.Append($"<input type='text' id='noUiInput21' {fun}>")
                html.Append("</div>")
                'html.Append("<div class='d-flex justify-content-center slide_valueComment '>")
                html.Append($"<div class='form-group'{If(model.isPipeViewOnly, " style='pointer-events:none;'", "")}>")

                html.Append("<div id='graphicalSlider'></div>")
                html.Append("</div>")

                html.Append("<div class='result_values'>")





                html.Append("<div class='pairwise_newdesign'>")
                'Comment box
                If model.show_comments Then
                    html.Append("<div style='cursor:pointer;'  class='comment_div px-2'>")
                    html.Append("<div class='comment_icon'>")
                    'html.Append("<input type='checkbox' class='comment_model'>")
                    If (model.comment Is Nothing Or model.comment = "") Then
                        'html.Append("<i class='far fa-comments' id='single_comment_icon' onclick='showNode(16)'  aria-hidden='true'></i>")
                        'html.Append("<a href='javascript:void(0)' onclick='showNode(16)'><img src='../../img/icon/commentadd.svg' id='single_comment_icon' aria-hidden='true'> <span style='color: #333333'>Add Comment</span></a>")
                        If Not (screenCheck.isMobileBrowserClient(Request)) Then
                            html.Append("<a href='javascript:void(0)' onclick='showNode(16)'><img src = '../../img/icon/Grey-plus-icon.svg' id='ImgComment_iconEmpty_16' title='' aria-hidden='true'>  <img src = '../../img/icon/comment-svgrepo-com.svg' id='ImgComment_icon_16' style='display:none;'title='' aria-hidden='true'> <span style='color: #333333'  class='tooltip_item'>Add Comment</span></a>")
                        Else
                            html.Append("<a href='javascript:void(0)' onclick='showNodeMobile(16)'><img src = '../../img/icon/Grey-plus-icon.svg' id='ImgComment_iconEmpty_16' title='' aria-hidden='true'>  <img src = '../../img/icon/comment-svgrepo-com.svg' id='ImgComment_icon_16' style='display:none;'title='' aria-hidden='true'> <span style='color: #333333'  class='tooltip_item'>Add Comment</span></a>")
                        End If
                    Else
                        'html.Append("<i Class='fa fa-comments' id='single_comment_icon' onclick='showNode(16)'  title='" + model.comment + "' aria-hidden='true'></i>")
                        If Not (screenCheck.isMobileBrowserClient(Request)) Then
                            html.Append($"<a href='javascript:void(0)' onclick='showNode(16)'><img src = '../../img/icon/Grey-plus-icon.svg' id='ImgComment_iconEmpty_16' style='display:none;' title='' aria-hidden='true'> <img src = '../../img/icon/comment-svgrepo-com.svg' id='ImgComment_icon_16' title='{model.comment.ToString()}' aria-hidden='true'> <span style='color: #333333'  class='tooltip_item'>Add Comment</span></a>")
                        Else
                            html.Append($"<a href='javascript:void(0)' onclick='showNodeMobile(16)'><img src = '../../img/icon/Grey-plus-icon.svg' id='ImgComment_iconEmpty_16' style='display:none;' title='' aria-hidden='true'> <img src = '../../img/icon/comment-svgrepo-com.svg' id='ImgComment_icon_16' title='{model.comment.ToString()}' aria-hidden='true'> <span style='color: #333333'  class='tooltip_item'>Add Comment</span></a>")
                        End If
                    End If
                        html.Append("<div class='info_tooltip right_tooltip'  id='parentnode16' style='display:none;'>")
                    html.Append("<div class='d-flex justify-content-between'>")
                    html.Append("<span>Add your comment</span>")
                    html.Append($"<div class='action_icons' ><a href='javascript:void(0);' onclick='hideNode(16)'><i class='fa fa-times' aria-hidden='true'></i></a></div>")
                    html.Append("</div>")
                    html.Append($"<textarea{If(model.isPipeViewOnly, " style='pointer-events:none;'", "")} class='form-control mb-3 mt-2 w-100' rows='3' id='single_comment' placeholder='Add your comment'>{model.comment}</textarea>")
                    html.Append("<div class='comt_btn text-end'>")
                    html.Append($"<button{If(model.isPipeViewOnly, " class='d-none'", "")} type='button' onclick='hideNode(16,16); comment_updated();'><i class='fa fa-check' aria-hidden='true'></i> OK</button>")
                    html.Append("</div>")
                    html.Append("</div>")
                    html.Append("</div>")
                    html.Append("</div>")
                Else
                    html.Append("<div style='cursor:pointer;'  class='comment_div px-2'>")
                    html.Append("<div class='comment_icon' style='display: none;'></div> </div>")
                End If
                html.Append($"<div{If(model.isPipeViewOnly, " style='pointer-events:none;'", "")} class='close_icon ms-2 ps-0 me-2' onclick=save_pairwise('-2147483648000','0');>")
                'html.Append("<i class='fa fa-times' aria-hidden='true'></i>")
                html.Append("<img src='../../img/icon/erasar.svg' class='imgClose'><span  class='tooltip_item'>Clear Judgment</span>")
                'html.Append("</div>")
                html.Append("</div> </div>")

                html.Append("</div>")

                html.Append("</div>")
                html.Append("</div>")
            End If
            NoInfoDocDataCls = ""
            filter_gray = ""
            'second node
            model_text = HttpUtility.UrlEncode(model.second_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
            model_text_info = HttpUtility.UrlEncode(model.second_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
            m = Regex.Match(model.second_node_info.ToString().Trim(), "<img")
            imstyl = ""
            onlyimg = ""
            If m.Success Then
                imstyl = " image_main_wrapper"
                onlyimg = " only_img"
            End If
            If (model.second_node_info.ToString().Trim() <> "" Or model.wrt_second_node_info.ToString().Trim() <> "") Then
                If (model.second_node_info.ToString().Trim() = "" Or model.wrt_second_node_info.ToString().Trim() = "") Then
                    html.Append($"<div class='value_wrapper right_info nodataonesideinfodocrow{imstyl}'><div class='info_data selected'>")
                Else
                    html.Append($"<div class='value_wrapper right_info{imstyl}'><div class='info_data selected'>")
                End If
            Else
                NoInfoDocDataCls = " NoInfoData"
                filter_gray = "filter_gray"
                html.Append($"<div class='value_wrapper right_info emptyInfodocbothside{imstyl}'><div class='info_data selected'>")
            End If
            'html.Append("<div class='value_wrapper'>")
            'html.Append("<div Class='info_data selected'>")

            html.Append($"<div id='ChartRightInfoDoc' Class='info_header'>")
            html.Append($"<a href='#' class='{filter_gray}{NoInfoDocDataCls}'><img src='../../img/icon/info-close.svg' class='icon'></a>")
            html.Append($"<span Class='title_tag text-uppercase' title='{model.second_node}'>{model.second_node}</span>")
            html.Append($"<div class='header_edit_fw {NoInfoDocDataCls }'><a href='#' data-bs-toggle='modal' data-bs-target='#exampleModal' onclick='SetExpendedPopupElement(decodeURIComponent(" + ChrW(&H22) + model_text + ChrW(&H22) + "),null,2,0,2,null,decodeURIComponent(" + ChrW(&H22) + "right-node" + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + model_text_info + ChrW(&H22) + "),null,null,decodeURIComponent(" + ChrW(&H22) + HttpUtility.UrlEncode(model.wrt_second_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22) + "),null,3,0,2,null,decodeURIComponent(" + ChrW(&H22) + "wrt-right-node1" + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + model_text + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + wrt_header_text + ChrW(&H22) + "))'><img src='../../img/icon/full-screen-svgrepo-com.svg '></a>")
            'html.Append($"<a href='javascript:void(0);' Class='d-md-block d-none ms-2'><img class='chkEvaluatorone' src='../../img/icon/edit-icon.svg'  onclick='showInfoPopup(decodeURIComponent(" + ChrW(&H22) + model_text + ChrW(&H22) + "),null,2,0,2,null,decodeURIComponent(" + ChrW(&H22) + "right-node" + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + model_text_info + ChrW(&H22) + "))'></a>")
            html.Append("</div></div>")
            'If (model.second_node_info.ToString().Trim() <> "" Or model.wrt_second_node_info.ToString().Trim() <> "") Then
            '' Right Side InfoDoc
            'If model.second_node_info.ToString().Trim().Length < 120 Then
            '    html.Append($"<span>{model.second_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))}</span>")
            'Else
            '    html.Append($"<span>{model.second_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)).Substring(0, 120)} ...</span>")
            'End If
            'html.Append($"<span>{model.second_node_info}</span>")

            'If (model.second_node_info.ToString().Trim() = "") Then
            '    html.Append("<div Class='no-data_content'>")
            '    html.Append("<h2> No Data</h2>")
            '    html.Append("<a href='javascript:void(0);' class='chkEvaluatorone' data-bs-toggle='modal' data-bs-target='#exampleModal' onclick='SetExpendedPopupElement(decodeURIComponent(" + ChrW(&H22) + model_text + ChrW(&H22) + "),null,2,0,2,null,decodeURIComponent(" + ChrW(&H22) + "right-node" + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + model_text_info + ChrW(&H22) + "),null,null,decodeURIComponent(" + ChrW(&H22) + HttpUtility.UrlEncode(model.wrt_second_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22) + "),null,3,0,2,null,decodeURIComponent(" + ChrW(&H22) + "wrt-right-node1" + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + model_text + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + wrt_header_text + ChrW(&H22) + "))'> +<u> Add Data</u></a></div>")
            'Else
            '    'html.Append("</div>")
            'End If
            'Dim infoAlttxt = model.second_node_info.ToString().Replace("</b>", "")
            'infoAlttxt = infoAlttxt.Replace("<b>", "")
            If (model.second_node_info.ToString().Trim() = "") Then
                html.Append("<div Class='body_content removep2 NoInfoData pwnodata'><div class='multiinfodata'><h2>No Data</h2></div></div>")
            Else
                html.Append($"<div Class='body_content removep2{onlyimg}'><span>{model.second_node_info.ToString()}</span></div>")
            End If
            If (model.wrt_second_node_info.ToString().Trim() = "") Then
                html.Append($"<div Class='wrt_content NoInfoData'><div class='body_content'><h2>WITH RESPECT TO <span>{model.parent_node.ToString()}</span></h2><div class='multiinfodata'><h3>No Data</h3> </div> </div></div>")
            Else
                html.Append($"<div Class='wrt_content'><div class='body_content'><h2>WITH RESPECT TO <span>{model.parent_node.ToString()}</span></h2><span>{model.wrt_second_node_info.ToString().Trim()}</span> </div></div>")
            End If
            'html.Append("<div Class='info_datafooter mt-4'>")
            '    html.Append("<div Class='info_datafooter'>")
            'html.Append($"<Button type='button' {If((model.wrt_second_node_info.ToString().Trim()) <> "", " class=''", " class='nowrt_data' ")}  data-bs-toggle='modal' data-bs-target='#exampleModal'  onclick='SetExpendedPopupElement(decodeURIComponent(" + ChrW(&H22) + model_text + ChrW(&H22) + "),null,2,0,2,null,decodeURIComponent(" + ChrW(&H22) + "right-node" + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + model_text_info + ChrW(&H22) + "),null,null,decodeURIComponent(" + ChrW(&H22) + HttpUtility.UrlEncode(model.wrt_second_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22) + "),null,3,0,2,null,decodeURIComponent(" + ChrW(&H22) + "wrt-right-node1" + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + model_text + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + wrt_header_text + ChrW(&H22) + "))' >With respect To <img src='../../img/icon/button_arrow.svg'></button>")

            'html.Append("</div>")
            'html.Append("</div>")
            'End If

            html.Append("<div class='w-100 d-none'>")
            html.Append("<div class='top_content_wrapper'>")
            html.Append("<div class=' green_bgcolor p-3 text-center text-light clcheckboxes setDivMaxHeight7 align-self-start d-flex  align-items-center position-relative resizepane' id='divSecondNode' style='height: auto !important; max-width: 100%;'>")
            'html.Append($"<input type='checkbox' class='toggle_checkbox me-2 checkchange hide_infodoc_tooltip' id='checkboxes5'> <i class='fa fa-plus-square{If(model.second_node_info IsNot Nothing AndAlso model.second_node_info <> "", "", " chkEvaluator")} hide_infodoc_tooltip' id='checkBoxIcon5' aria-hidden='true'></i>")
            html.Append($"<span class='ms-3 green_bgcolor'>{model.second_node}</span>")

            html.Append($"<div class='tooltip_wrapper' id='tooltip_wrapper_3' style='display: none;'>")
            html.Append("<div class='tooltip-header'>")
            html.Append($"<span>{model.second_node}</span>")
            html.Append($"<div class='action_icons'>")

            html.Append($"<a href='javascript:void(0);'><i class='bx bxs-edit-alt chkEvaluator d-none' onclick='showInfoPopup(decodeURIComponent(" + ChrW(&H22) + model_text_info + ChrW(&H22) + "),null,2,0,2,null,decodeURIComponent(" + ChrW(&H22) + "right-node" + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + model_text + ChrW(&H22) + "))'></i></a>")
            html.Append($"<a href='javascript:void(0);'><i class='fa fa-times' aria-hidden='true' onclick=updateRightSideComment('3','tooltip','close',event)></i></a>")
            html.Append("</div>")
            html.Append("</div>")
            html.Append($"<p class='border-top font-size-12 pt-2 text-start mb-0'>{model.second_node_info}</p>")
            html.Append("</div>")

            html.Append("</div>")

            html.Append($"<div class='my-3 position-relative{If((model.second_node_info) <> "" And (model.second_node_info) <> "NaN", "", " chkEvaluator")}'  id='dvsecondnodelbl'><input type='checkbox' class='toggle_checkbox me-2  checkchange hide_infodoc_tooltip checkboxes checkboxes6 {If((model_text_info) <> "" And (model_text_info) <> "NaN", "", "chkEvaluator")}' data-val='SecondNode' id='checkboxesSecondNode'> <i class='checkBoxIcon5 hide_infodoc_tooltip mt-1  {If(model_text_info <> "" And model_text_info <> "NaN", "fa fa-minus-square ", "fa fa-plus-square chkEvaluator")}' id='checkBoxIconSecondNode' aria-hidden='true'></i>")
            'html.Append($"<input type='checkbox' class='toggle_checkbox me-2  checkchange hide_infodoc_tooltip checkboxes checkboxes5' data-val='SecondNode' id='checkboxesSecondNode'> <i class='checkBoxIcon5 hide_infodoc_tooltip mt-1  {If(model_text_info <> "" And model_text_info <> "NaN", "fa fa-minus-square ", "fa fa-plus-square chkEvaluator")}' id='checkBoxIconSecondNode' aria-hidden='true'></i>")
            html.Append($"<i class='fa fa-info-circle chkEvaluator1 show_infodoc_tooltip hideinfoicon mt-1{If(model.is_infodoc_tooltip, " d-none", " d-none")}' onclick=updateRightSideComment('3','tooltip','open',event) aria-hidden='true'></i>")

            html.Append($"<span class='ms-2 wlr_heading'>{model.second_node}</span></div>")
            html.Append($"<div class='left-node-info-div tt-resizable-panel-1 toggle_area hide_infodoc_tooltip_div checkout-shipping-address5 {If((model.second_node_info) <> "" And (model.second_node_info) <> "NaN", "", "chkEvaluator")} resizepane right-node-info-div columns tt-accordion-content tg-accordion-sub-2'  style='max-width: 100%; {If((model.second_node_info) <> "" And (model.second_node_info) <> "NaN", "", "display:none;")}' id='checkout-shipping-address-SecondNode'>")
            html.Append($"<p>{model.second_node_info}</p>")

            'html.Append($"<a href='javascript:void(0);' class='edit_icon chkEvaluator' onclick=showInfoPopup(decodeURIComponent({ChrW(&H22) + model_text + ChrW(&H22)}),null,'2','0','2',null,'right-node',decodeURIComponent({ChrW(&H22) + model_text_info + ChrW(&H22)}))>")
            html.Append($"<a href='javascript:void(0);' class='edit_icon chkEvaluator' onclick='showInfoPopup(decodeURIComponent(" + ChrW(&H22) + model_text_info + ChrW(&H22) + "),null,2,0,2,null,decodeURIComponent(" + ChrW(&H22) + "right-node" + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + model_text + ChrW(&H22) + "))'>")
            html.Append("<i class='bx bxs-edit chkEvaluator d-none' aria-hidden='true'></i>")
            html.Append("</a>")
            html.Append("</div>")
            html.Append("</div>")

            'wrt_second_node_info
            model_text = HttpUtility.UrlEncode(model.second_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
            model_text_info = HttpUtility.UrlEncode(model.wrt_second_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
            html.Append($"<div class='top_content_wrapper mt-3{If(model.wrt_second_node_info IsNot Nothing AndAlso model.wrt_second_node_info.Trim() <> "", "", " chkEvaluator")}' id='dvsecond_node'>")
            html.Append($"<div class='wrapper_c_2 resizepane' id='idwrapper_c_2' style='max-width: 100%;'><input type='checkbox' class='toggle_checkbox me-2 hide_infodoc_tooltip checkboxes checkboxes2' data-val='SecondNodeWRT' id='checkboxesSecondNodeWRT'> <i class='mb-0 mb-lg-4 checkBoxIcon1 hide_infodoc_tooltip  {If(model_text_info <> "" And model_text_info <> "NaN", "fa fa-minus-square ", "fa fa-plus-square chkEvaluator")}' id='checkBoxIconSecondNodeWRT' aria-hidden='true'></i>")
            html.Append($"<i class='fa fa-info-circle chkEvaluator1 show_infodoc_tooltip hideinfoicon{If(model.is_infodoc_tooltip, " d-none", " d-none")}' onclick=updateRightSideComment('4','tooltip','open',event) aria-hidden='true'></i>")
            If Not model.hideInfoDocCaptions Then
                html.Append($"<span class='wlr_heading' title='{model.second_node} WRT {model.parent_node}'>&nbsp;")
                html.Append($" {If(model.second_node IsNot Nothing And model.second_node.Length > 14, model.second_node.Substring(0, 15) + "...", model.second_node)}")
                html.Append($" WRT {If(model.parent_node IsNot Nothing And model.parent_node.Length > 14, model.parent_node.Substring(0, 15) + "...", model.parent_node)}")
                html.Append("</span>")
            End If

            'html.Append($"<span class='wlr_heading' title='{model.second_node} WRT {model.parent_node}'>&nbsp;")
            'html.Append($" {If(model.second_node IsNot Nothing And model.second_node.Length > 14, model.second_node.Substring(0, 15) + "...", model.second_node)}")
            'html.Append($" WRT {If(model.parent_node IsNot Nothing And model.parent_node.Length > 14, model.parent_node.Substring(0, 15) + "...", model.parent_node)}")
            'html.Append("</span>")


            wrt_header_text = HttpUtility.UrlEncode(model.parent_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
            html.Append($"<div class='tooltip_wrapper' id='tooltip_wrapper_4' style='display: none;'>")
            html.Append("<div class='tooltip-header'>")
            html.Append($"<span title='{model.second_node} WRT {model.parent_node}'>&nbsp;")
            html.Append($" {If(model.second_node IsNot Nothing And model.second_node.Length > 14, model.second_node.Substring(0, 15) + "...", model.second_node)}")
            html.Append($" WRT {If(model.parent_node IsNot Nothing And model.parent_node.Length > 14, model.parent_node.Substring(0, 15) + "...", model.parent_node)}")
            html.Append("</span>")
            html.Append($"<div class='action_icons'>")

            html.Append($"<a href='javascript:void(0);'><i class='bx bxs-edit-alt chkEvaluator d-none' onclick='showInfoPopup(decodeURIComponent(" + ChrW(&H22) + model_text_info + ChrW(&H22) + "),null,3,0,1,null,decodeURIComponent(" + ChrW(&H22) + "wrt-left-node1" + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + model_text + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + wrt_header_text + ChrW(&H22) + "))'></i></a>")
            html.Append($"<a href='javascript:void(0);'><i class='fa fa-times' aria-hidden='true' onclick=updateRightSideComment('4','tooltip','close',event)></i></a>")
            html.Append("</div>")
            html.Append("</div>")
            html.Append($"<p class='border-top font-size-12 pt-2 text-start mb-0'>{model.wrt_first_node_info}</p>")
            html.Append("</div></div>")

            html.Append($"<div class='wrt-right-node-info-div tt-resizable-panel-2 toggle_area small_content hide_infodoc_tooltip_div checkout-shipping-address1 resizepane wrt-right-node-info-div columns tt-accordion-content tg-accordion-sub-4'  style='max-width: 100%; {If((model.wrt_second_node_info) <> "" And (model.wrt_second_node_info) <> "NaN", "", "display:none;")}' id='checkout-shipping-address-SecondNodeWRT'>  ")
            html.Append($"<p class='font-size-14 me-3'>{model.wrt_second_node_info}</p>")

            'html.Append($"<a href='javascript:void(0);' class='edit_icon chkEvaluator' onclick=showInfoPopup(decodeURIComponent({ChrW(&H22) + model_text + ChrW(&H22)}),null,'3','0','2',null,'wrt-right-node',decodeURIComponent({ChrW(&H22) + model_text_info + ChrW(&H22)}),decodeURIComponent({ChrW(&H22) + wrt_header_text + ChrW(&H22)}))>")
            html.Append($"<a href='javascript:void(0);' class='edit_icon chkEvaluator' onclick='showInfoPopup(decodeURIComponent(" + ChrW(&H22) + model_text_info + ChrW(&H22) + "),null,3,0,2,null,decodeURIComponent(" + ChrW(&H22) + "wrt-right-node1" + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + model_text + ChrW(&H22) + "),decodeURIComponent(" + ChrW(&H22) + wrt_header_text + ChrW(&H22) + "))'>")
            html.Append("<i class='bx bxs-edit chkEvaluator d-none' aria-hidden='true'></i>")
            html.Append("</a>")
            html.Append("</div>")
            html.Append("</div>")
            html.Append("</div>")
            html.Append("</div>")
            '=============================================

            html.Append("</div>")

            html.Append("</div>")
            html.Append("</div>")
            html.Append("</div>")
            html.Append("</div>")
            'html.Append("<div class='container-fluid'>")
            html.Append("<div class='row' id='removeB'>")
            'html.Append("<div class='container'>")
            'Title
            html.Append("<div class='col-12'>")
            '==============================


            html.Append("</div>")
            html.Append("</div>")

            divContent.InnerHtml = html.ToString()
        End If
    End Sub

End Class