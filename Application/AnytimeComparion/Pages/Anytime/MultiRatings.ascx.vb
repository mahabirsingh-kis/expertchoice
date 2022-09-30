Imports System.Runtime.InteropServices

Public Class MultiRatings
    Inherits System.Web.UI.UserControl

    Dim screenCheck As ScreenCheck = New ScreenCheck()

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    End Sub

    Public Shared Function GetDetails(ByVal AnytimeAction As clsAction, ByVal App As clsComparionCore, <Out> ByRef pipeHelpUrl As String,
                                      <Out> ByRef qh_help_id As ecEvaluationStepType, <Out> ByRef NonPWType As String, <Out> ByRef StepNode As clsNode,
                                       <Out> ByRef showPriorityAndDirectValue As Boolean, <Out> ByRef precision As Integer, <Out> ByRef first_node_info As String,
                                      <Out> ByRef wrt_first_node_info As String, <Out> ByRef IsUndefined As Boolean, <Out> ByRef scaleDescriptions As List(Of ScaleDescriptions),
                                      <Out> ByRef StepTask As String, <Out> ByRef MultiNonPW_Data As List(Of clsRatingLine), <Out> ByRef ParentNodeName As String,
                                      <Out> ByRef parent_node_info As String, <Out> ByRef ParentNodeID As Integer, <Out> ByRef ParentNodeGUID As Guid,
                                      <Out> ByRef infodoc_params As String()) As Object
        Dim now_pw_all As clsNonPairwiseEvaluationActionData = CType(AnytimeAction.ActionData, clsNonPairwiseEvaluationActionData)

        Select Case now_pw_all.MeasurementType
            Case ECMeasureType.mtRatings
                pipeHelpUrl = TeamTimeClass.ResString("help_pipe_rating")
                qh_help_id = ecEvaluationStepType.MultiRatings
                NonPWType = "mtRatings"
                Dim Ratings As List(Of clsRating) = Nothing
                Dim MeasureScales As clsMeasureScales = App.ActiveProject.ProjectManager.MeasureScales
                Dim UndefIdx As Integer = -1

                If TypeOf AnytimeAction.ActionData Is clsAllChildrenEvaluationActionData Then
                    Dim MultiNonPWData As clsAllChildrenEvaluationActionData = CType(AnytimeAction.ActionData, clsAllChildrenEvaluationActionData)
                    StepNode = MultiNonPWData.ParentNode

                    If MeasureScales IsNot Nothing Then

                        If MeasureScales.RatingsScales IsNot Nothing Then
                            Dim RS As clsRatingScale = MeasureScales.GetRatingScaleByID(MultiNonPWData.ParentNode.RatingScaleID())

                            If RS IsNot Nothing Then
                                showPriorityAndDirectValue = App.ActiveProject.ProjectManager.Parameters.RatingsUseDirectValue(RS.GuidID)
                                Ratings = New List(Of clsRating)()

                                For Each tRating As clsRating In RS.RatingSet
                                    Dim tNewRating = New clsRating(tRating.ID, tRating.Name, tRating.Value, Nothing, tRating.Comment)
                                    tNewRating.GuidID = tRating.GuidID
                                    Ratings.Add(tNewRating)
                                Next
                            End If

                            precision = AnytimeClass.GetPrecisionForRatings(CType(RS, clsRatingScale))
                        End If
                    End If

                    If Ratings IsNot Nothing Then
                        Ratings.Add(New clsRating(-1, "Not Rated", 0, Nothing))
                        Ratings.Add(New clsRating(-2, "Direct Value", 0, Nothing))
                    End If

                    Dim Lst As List(Of clsRatingLine) = New List(Of clsRatingLine)()
                    Dim ID = 0

                    For Each tAlt As clsNode In MultiNonPWData.Children
                        Dim R As clsRatingMeasureData = CType(MultiNonPWData.GetJudgment(tAlt), clsRatingMeasureData)
                        Dim RID As Integer = -1
                        RID = If(R.Rating IsNot Nothing, R.Rating.ID, RID)
                        UndefIdx = If(UndefIdx = -1 AndAlso R.IsUndefined, ID, UndefIdx)
                        first_node_info = CStr(Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, If(tAlt.IsAlternative, reObjectType.Alternative, reObjectType.Node), tAlt.NodeID.ToString(), tAlt.InfoDoc, True, True, -1))
                        wrt_first_node_info = CStr(Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, reObjectType.AltWRTNode, tAlt.NodeID.ToString(), App.ActiveProject.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(tAlt.NodeGuidID, MultiNonPWData.ParentNode.NodeGuidID), True, True, MultiNonPWData.ParentNode.NodeID))
                        Dim DV As Single = -1
                        DV = If(R.Rating IsNot Nothing AndAlso R.Rating.RatingScaleID < 0 AndAlso R.Rating.ID < 0, R.Rating.Value, DV)
                        Lst.Add(New clsRatingLine(tAlt.NodeID, R.RatingScale.GuidID.ToString(), JS_SafeHTML(tAlt.NodeName), Ratings, RID, first_node_info, R.Comment, DV, "", "", wrt_first_node_info, "", "", ""))
                        ID += 1

                        If R.IsUndefined Then
                            IsUndefined = True
                        End If

                        Dim sd As ScaleDescriptions = New ScaleDescriptions()
                        sd.Name = R.RatingScale.Name
                        sd.Guid = R.RatingScale.GuidID.ToString()
                        sd.Description = Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, reObjectType.MeasureScale, R.RatingScale.GuidID.ToString(), R.RatingScale.Comment, True, True, -1)
                        scaleDescriptions.Add(sd)

                    Next

                    StepTask = ""

                    Try
                        StepTask = TeamTimeClass.GetPipeStepTask(AnytimeAction, Nothing, AnytimeClass.IsImpact AndAlso Not MultiNonPWData.ParentNode.IsTerminalNode)
                    Catch
                        StepTask = ""
                    End Try

                    MultiNonPW_Data = Lst
                    ParentNodeName = MultiNonPWData.ParentNode.NodeName
                    parent_node_info = CStr(Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, If(MultiNonPWData.ParentNode.IsAlternative, Consts.reObjectType.Alternative, Consts.reObjectType.Node), MultiNonPWData.ParentNode.NodeID.ToString(), MultiNonPWData.ParentNode.InfoDoc, True, True, -1))
                    ParentNodeID = MultiNonPWData.ParentNode.NodeID
                    ParentNodeGUID = MultiNonPWData.ParentNode.NodeGuidID
                    infodoc_params(0) = GeckoClass.GetInfodocParams(MultiNonPWData.ParentNode.NodeGuidID, Guid.Empty, True)
                    infodoc_params(1) = GeckoClass.GetInfodocParams(MultiNonPWData.ParentNode.NodeGuidID, Guid.Empty, True)
                    infodoc_params(2) = GeckoClass.GetInfodocParams(MultiNonPWData.ParentNode.NodeGuidID, Guid.Empty, True)
                End If

                If TypeOf AnytimeAction.ActionData Is clsAllCoveringObjectivesEvaluationActionData Then
                    Dim MultiNonPWData As clsAllCoveringObjectivesEvaluationActionData = CType(AnytimeAction.ActionData, clsAllCoveringObjectivesEvaluationActionData)
                    Dim Lst As List(Of clsRatingLine) = New List(Of clsRatingLine)()
                    Dim ID As Integer = 0

                    For Each tCovObj As clsNode In MultiNonPWData.CoveringObjectives

                        If MeasureScales IsNot Nothing Then

                            If MeasureScales.RatingsScales IsNot Nothing Then
                                Dim RS As clsRatingScale = MeasureScales.GetRatingScaleByID(tCovObj.RatingScaleID())

                                If RS IsNot Nothing Then
                                    showPriorityAndDirectValue = App.ActiveProject.ProjectManager.Parameters.RatingsUseDirectValue(RS.GuidID)
                                    Dim sd1 As ScaleDescriptions = New ScaleDescriptions()
                                    sd1.Name = RS.Name
                                    sd1.Guid = RS.GuidID.ToString()
                                    sd1.Description = Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, reObjectType.MeasureScale, RS.GuidID.ToString(), RS.Comment, True, True, -1)
                                    scaleDescriptions.Add(sd1)

                                    Ratings = New List(Of clsRating)()

                                    For Each tRating As clsRating In RS.RatingSet
                                        Dim tNewRating = New clsRating(tRating.ID, tRating.Name, tRating.Value, Nothing, tRating.Comment)
                                        tNewRating.GuidID = tRating.GuidID
                                        Ratings.Add(tNewRating)
                                    Next
                                End If

                                precision = AnytimeClass.GetPrecisionForRatings(CType(RS, clsRatingScale))
                            End If
                        End If

                        If Ratings IsNot Nothing Then
                            Ratings.Add(New clsRating(-1, "Not Rated", 0, Nothing))
                            Ratings.Add(New clsRating(-2, "Direct Value", 0, Nothing))
                        End If

                        If TypeOf MultiNonPWData.GetJudgment(tCovObj) Is clsRatingMeasureData Then
                            Dim R As clsRatingMeasureData = CType(MultiNonPWData.GetJudgment(tCovObj), clsRatingMeasureData)
                            Dim RID As Integer = If(R.Rating IsNot Nothing, R.Rating.ID, -1)
                            UndefIdx = If(R.IsUndefined AndAlso UndefIdx = -1, ID, UndefIdx)
                            first_node_info = CStr(Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, If(tCovObj.IsAlternative, Consts.reObjectType.Alternative, reObjectType.Node), tCovObj.NodeID.ToString(), tCovObj.InfoDoc, True, True, -1))
                            wrt_first_node_info = CStr(Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, reObjectType.AltWRTNode, MultiNonPWData.Alternative.NodeID.ToString(), App.ActiveProject.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(MultiNonPWData.Alternative.NodeGuidID, tCovObj.NodeGuidID), True, True, tCovObj.NodeID))
                            Dim DV As Single = -1
                            DV = If(R.Rating IsNot Nothing AndAlso R.Rating.RatingScaleID < 0 AndAlso R.Rating.ID < 0, R.Rating.Value, DV)
                            Lst.Add(New clsRatingLine(tCovObj.NodeID, R.RatingScale.GuidID.ToString(), JS_SafeHTML(tCovObj.NodeName), Ratings, RID, first_node_info, R.Comment, DV, "", "", wrt_first_node_info, "", "", ""))
                            ID += 1

                            If R.IsUndefined Then
                                IsUndefined = True
                            End If
                        End If
                    Next

                    StepTask = ""

                    Try
                        StepTask = TeamTimeClass.GetPipeStepTask(AnytimeAction, Nothing, AnytimeClass.IsImpact AndAlso Not MultiNonPWData.Alternative.IsTerminalNode)
                    Catch
                        StepTask = ""
                    End Try

                    MultiNonPW_Data = Lst
                    ParentNodeName = MultiNonPWData.Alternative.NodeName
                    parent_node_info = CStr(Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, If(MultiNonPWData.Alternative.IsAlternative, reObjectType.Alternative, reObjectType.Node), MultiNonPWData.Alternative.NodeID.ToString(), MultiNonPWData.Alternative.InfoDoc, True, True, -1))
                    ParentNodeID = MultiNonPWData.Alternative.NodeID
                    ParentNodeGUID = MultiNonPWData.Alternative.NodeGuidID
                    infodoc_params(0) = GeckoClass.GetInfodocParams(MultiNonPWData.Alternative.NodeGuidID, Guid.Empty, True)
                    infodoc_params(1) = GeckoClass.GetInfodocParams(MultiNonPWData.Alternative.NodeGuidID, Guid.Empty, True)
                    infodoc_params(2) = GeckoClass.GetInfodocParams(MultiNonPWData.Alternative.NodeGuidID, Guid.Empty, True)
                End If

            Case ECMeasureType.mtDirect
                pipeHelpUrl = TeamTimeClass.ResString("help_pipe_directEntry")
                qh_help_id = ecEvaluationStepType.MultiDirectInput
                NonPWType = "mtDirect"

                If TypeOf AnytimeAction.ActionData Is clsAllChildrenEvaluationActionData Then
                    Dim MultiDirectData1 As clsAllChildrenEvaluationActionData = CType(AnytimeAction.ActionData, clsAllChildrenEvaluationActionData)
                    StepNode = MultiDirectData1.ParentNode
                    StepTask = ""

                    Try
                        StepTask = TeamTimeClass.GetPipeStepTask(AnytimeAction, Nothing, AnytimeClass.IsImpact AndAlso Not MultiDirectData1.ParentNode.IsTerminalNode)
                    Catch
                        StepTask = ""
                    End Try

                    ParentNodeName = MultiDirectData1.ParentNode.NodeName
                    parent_node_info = CStr(Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, If(MultiDirectData1.ParentNode.IsAlternative, reObjectType.Alternative, reObjectType.Node), MultiDirectData1.ParentNode.NodeID.ToString(), MultiDirectData1.ParentNode.InfoDoc, True, True, -1))
                    Dim Lst As List(Of clsRatingLine) = New List(Of clsRatingLine)()
                    Dim ID = 0

                    For Each tAlt As clsNode In MultiDirectData1.Children
                        Dim DD As clsDirectMeasureData = CType(MultiDirectData1.GetJudgment(tAlt), clsDirectMeasureData)
                        first_node_info = CStr(Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, If(tAlt.IsAlternative, reObjectType.Alternative, reObjectType.Node), tAlt.NodeID.ToString(), tAlt.InfoDoc, True, True, -1))
                        wrt_first_node_info = CStr(Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, reObjectType.AltWRTNode, tAlt.NodeID.ToString(), App.ActiveProject.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(tAlt.NodeGuidID, MultiDirectData1.ParentNode.NodeGuidID), True, True, MultiDirectData1.ParentNode.NodeID))
                        Dim DV As Single = -1
                        DV = If(DD.IsUndefined, DV, DD.DirectData)
                        Lst.Add(New clsRatingLine(tAlt.NodeID, tAlt.NodeGuidID.ToString(), JS_SafeHTML(tAlt.NodeName), Nothing, -1, first_node_info, DD.Comment, DV, "", "", wrt_first_node_info, "", "", ""))
                        ID += 1

                        If DD.IsUndefined Then
                            IsUndefined = True
                        End If
                    Next

                    MultiNonPW_Data = Lst
                    ParentNodeID = MultiDirectData1.ParentNode.NodeID
                    ParentNodeGUID = MultiDirectData1.ParentNode.NodeGuidID
                    infodoc_params(0) = GeckoClass.GetInfodocParams(MultiDirectData1.ParentNode.NodeGuidID, Guid.Empty, True)
                    infodoc_params(1) = GeckoClass.GetInfodocParams(MultiDirectData1.ParentNode.NodeGuidID, Guid.Empty, True)
                    infodoc_params(2) = GeckoClass.GetInfodocParams(MultiDirectData1.ParentNode.NodeGuidID, Guid.Empty, True)
                End If

                If TypeOf AnytimeAction.ActionData Is clsAllCoveringObjectivesEvaluationActionData Then
                    Dim MultiNonPWData As clsAllCoveringObjectivesEvaluationActionData = CType(AnytimeAction.ActionData, clsAllCoveringObjectivesEvaluationActionData)
                    Dim Lst As List(Of clsRatingLine) = New List(Of clsRatingLine)()
                    Dim ID As Integer = 0

                    For Each tNode As clsNode In MultiNonPWData.CoveringObjectives
                        Dim DD As clsDirectMeasureData = CType(MultiNonPWData.GetJudgment(tNode), clsDirectMeasureData)
                        first_node_info = CStr(Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, If(tNode.IsAlternative, reObjectType.Alternative, reObjectType.Node), tNode.NodeID.ToString(), tNode.InfoDoc, True, True, -1))
                        wrt_first_node_info = CStr(Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, reObjectType.AltWRTNode, MultiNonPWData.Alternative.NodeID.ToString(), App.ActiveProject.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(MultiNonPWData.Alternative.NodeGuidID, tNode.NodeGuidID), True, True, tNode.NodeID))
                        Dim DV As Single = -1
                        DV = If(DD.IsUndefined, -1, DD.DirectData)
                        Lst.Add(New clsRatingLine(tNode.NodeID, tNode.NodeGuidID.ToString(), StringFuncs.JS_SafeHTML(tNode.NodeName), Nothing, -1, first_node_info, DD.Comment, DV, "", "", wrt_first_node_info, "", "", ""))
                        ID += 1

                        If DD.IsUndefined Then
                            IsUndefined = True
                        End If
                    Next

                    StepTask = ""

                    Try
                        StepTask = TeamTimeClass.GetPipeStepTask(AnytimeAction, Nothing, AnytimeClass.IsImpact AndAlso Not MultiNonPWData.Alternative.IsTerminalNode)
                    Catch
                        StepTask = ""
                    End Try

                    MultiNonPW_Data = Lst
                    ParentNodeName = MultiNonPWData.Alternative.NodeName
                    parent_node_info = CStr(Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, If(MultiNonPWData.Alternative.IsAlternative, Consts.reObjectType.Alternative, Consts.reObjectType.Node), MultiNonPWData.Alternative.NodeID.ToString(), MultiNonPWData.Alternative.InfoDoc, True, True, -1))
                    ParentNodeID = MultiNonPWData.Alternative.NodeID
                    ParentNodeGUID = MultiNonPWData.Alternative.NodeGuidID
                    infodoc_params(0) = GeckoClass.GetInfodocParams(MultiNonPWData.Alternative.NodeGuidID, Guid.Empty, True)
                    infodoc_params(1) = GeckoClass.GetInfodocParams(MultiNonPWData.Alternative.NodeGuidID, Guid.Empty, True)
                    infodoc_params(2) = GeckoClass.GetInfodocParams(MultiNonPWData.Alternative.NodeGuidID, Guid.Empty, True)
                End If
        End Select
    End Function

    Public Sub bindHtml(ByVal model As AnytimeOutputModel, <Out> ByVal hdnactive_multi_index As HiddenField)
        If model IsNot Nothing Then
            Dim html As StringBuilder = New StringBuilder()
            Dim IsHeaderNode As New HttpCookie("IsHeaderNode")
            IsHeaderNode.Values("IsHeaderNode") = "No"
            If (IsHeaderNode.Value.Contains("IsHeaderNode")) Then
                IsHeaderNode.Value = IsHeaderNode.Value.Substring(13)
            End If
            Response.Cookies.Add(IsHeaderNode)
            html.Append("<div class='' id=''>")
            html.Append("<div class='row' id='pipeContent'>")

            'Header / Title
            html.Append("<div class='col-12' >")
            html.Append("<div class='page-title-box text-center toggle_title'>")
            html.Append("<div class='collabse_btn' id='titleDiv'>")
            html.Append("<input type='checkbox' class='collabse_arrow' checked>")
            html.Append("<div class='arrow_icon'>")
            html.Append("<i class='fa fa-plus' aria-hidden='true'></i>")
            html.Append("<i class='fa fa-minus' aria-hidden='true'></i>")
            html.Append("</div>")
            html.Append($"<div class='txtStepTask'>{If(model.step_task Is Nothing Or model.step_task = "", "", model.step_task)}<asp:Label ID='lblTask' runat='server' /><span id='divT2S' style='font-size:20px; margin-left:1ex border: 0px !important; background: #f0f0ff08 !important; display:none'></span></div>")
            Dim model_text As String = HttpUtility.UrlEncode(model.cluster_phrase.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
            ''html.Append($"<a href='javascript:void(0);' onclick='openQueModal(1,decodeURIComponent(" + ChrW(&H22) + model_text + ChrW(&H22) + "))'>")
            html.Append($"<a{If(model.isPipeViewOnly, " class='d-none'", "")} href='javascript:void(0);' onclick=showHeaderPopup(decodeURIComponent({ChrW(&H22) + model_text + ChrW(&H22)}),'0','0','0','0',null,'question-node',decodeURIComponent({ChrW(&H22) + model_text + ChrW(&H22)}),null,null,'true')>")
            html.Append("<i Class='bx bxs-edit-alt chkEvaluator d-none'></i></a>")
            'html.Append("<a href='javascript:void(0);'><i class='bx bxs-edit-alt chkEvaluator'></i></a></p>")



            model_text = HttpUtility.UrlEncode(model.parent_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
            Dim model_text_info As String = HttpUtility.UrlEncode(model.parent_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
            html.Append("<div class='d-flex justify-content-center'>")
            html.Append("<div class='info_tooltip_main position-relative'>")
            html.Append($"<i class='fa fa-info-circle infocolorheader {If((model_text_info) = "" Or (model_text_info) = "NaN", "infoColor", "")} {If((model_text_info) <> "" And (model_text_info) <> "NaN", "", "chkEvaluator")}' aria-hidden='true' onclick=updateRightSideComment('','hdrtooltip','open',event)></i>")
            html.Append("<div class='tooltip_wrapper hideshowinfo' id='tooltip_wrapper' style='display: none;'>")
            html.Append("<div class='tooltip-header'>")
            html.Append($"<span>{model.parent_node}</span>")
            html.Append("<div class='action_icons'>")

            html.Append($"<a{If(model.isPipeViewOnly, " class='d-none'", "")} href='javascript:void(0);'><i class='bx bxs-edit-alt chkEvaluator d-none' onclick=showInfoPopup(decodeURIComponent({ChrW(&H22) + model_text + ChrW(&H22)}),'0','-1','0','-1',null,'parent-node',decodeURIComponent({ChrW(&H22) + model_text_info + ChrW(&H22)}),'header')></i></a>")
            html.Append("<a href='javascript:void(0);'><i class='fa fa-times' aria-hidden='true' onclick=updateRightSideComment('','hdrtooltip','close',event)></i></a>")
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
            '==========================

            'left item wise dropdown selection
            html.Append("<div class='col-lg-6'>")
            html.Append("<div class='progress_table_row'>")
            Dim count As Integer = 0, dataId As String = "", dataCount As Integer = 0
            If model.multi_non_pw_data IsNot Nothing Then
                For i = 0 To model.multi_non_pw_data.Count - 1
                    dataCount = dataCount + 1
                    Dim active_item = model.multi_non_pw_data.Where(Function(x) x.RatingID = -1).FirstOrDefault()
                    If active_item Is Nothing Then
                        html.Append($"<div id='div_row_wrapper_{i}' class='row_wrapper' onclick=addActiveClass(this,'','{i}');>")
                        '' html.Append($"<div id='div_row_wrapper_{i}' class='row_wrapper {If(i = 0, "active_table_row", "")}' onclick=addActiveClass(this,'','{i}');>")
                    Else
                        If count = 0 And dataId = "" And model.multi_non_pw_data(i).RatingID = -1 Then
                            html.Append($"<div id='div_row_wrapper_{i}' class='row_wrapper' onclick=addActiveClass(this,'','{i}');>")
                            'html.Append($"<div id='div_row_wrapper_{i}' class='row_wrapper {If(model.multi_non_pw_data(i).RatingID = -1, "active_table_row", "")}' onclick=addActiveClass(this,'','{i}');>")
                            dataId = i.ToString()
                            'hdnactive_multi_index.Value = i.ToString()
                            hdnactive_multi_index.Value = "0"
                        Else
                            html.Append($"<div id='div_row_wrapper_{i}' class='row_wrapper' onclick=addActiveClass(this,'','{i}');>")
                        End If
                        count = If(count = 0, If(model.multi_non_pw_data(i).RatingID = -1, 1, 0), 1)
                    End If
                    html.Append("<div class='tooltips_group'>")
                    If model.show_comments Then
                        If (i = model.multi_non_pw_data.Count - 1) Then
                            html.Append("<div class='info_tooltip_main position-relative scndLastColumn'>")
                        Else
                            html.Append("<div class='info_tooltip_main position-relative'>")
                        End If

                        html.Append($"<div class='comment_div' id='comment_div_{i}'>")
                        If model.multi_non_pw_data(i).Comment Is Nothing Or model.multi_non_pw_data(i).Comment = "" Then
                            html.Append($"<i class='far fa-comments emptyComment' id='comment_icon_{i}' aria-hidden='true' onclick=updateRightSideComment('{i}','comment','open',event)></i>")
                        Else
                            html.Append($"<i class='fa fa-comments filledComment' id='comment_icon_{i}' aria-hidden='true' onclick=updateRightSideComment('{i}','comment','open',event)></i>")
                        End If

                        html.Append($"<div class='tooltip_wrapper hideshowinfo' id='comment_div_box_{i}' style='display: none;'>")
                        html.Append("<div class='d-flex justify-content-between tooltip-header'>")
                        html.Append("<span>Add your comment</span>")
                        html.Append($"<div class='action_icons' onclick=updateRightSideComment('{i}','comment','close',event)>")
                        html.Append("<a href='javascript:void(0);'><i class='fa fa-times' aria-hidden='true'></i></a>")
                        html.Append("</div>")
                        html.Append("</div>")
                        'html.Append($"<textarea{If(model.isPipeViewOnly, " style='pointer-events:none;'", "")} class='form-control mb-3 mt-2 w-100' id='txtRightComment_{i}' rows='3'>{model.multi_non_pw_data(i).Comment}</textarea>")
                        'html.Append("<div class='comt_btn text-end'>")
                        'html.Append($"<button{If(model.isPipeViewOnly, " class='d-none'", "")} type='button' onclick=updateRightSideComment('{i}','comment','close',event)><i class='fa fa-check' aria-hidden='true'></i> OK</button>")
                        'html.Append("</div>")
                        html.Append("</div>")
                        html.Append("</div>")
                        html.Append("</div>")
                    End If

                    If (i = model.multi_non_pw_data.Count - 1) Then
                        If model.show_comments <> True Then
                            html.Append("<div class='info_tooltip_main position-relative scndLastColumn' style='display: none;'>")
                        Else
                            html.Append("<div class='info_tooltip_main position-relative scndLastColumn'>")
                        End If

                    Else
                        If model.show_comments <> True Then
                            html.Append("<div class='info_tooltip_main position-relative' style='display: none;'>")
                        Else
                            html.Append("<div class='info_tooltip_main position-relative'>")
                        End If
                    End If
                    'If model.is_infodoc_tooltip And model.showinfodocnode Then
                    model_text = HttpUtility.UrlEncode(model.multi_non_pw_data(i).Title.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
                    model_text_info = HttpUtility.UrlEncode(model.multi_non_pw_data(i).Infodoc.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
                    If model.showinfodocnode Then
                        html.Append($"<span class='spnInfo'><i class='fa fa-info-circle infocolor{i} {If((model_text_info) = "" Or (model_text_info) = "NaN", "infoColor", "")} {If((model_text_info) <> "" And (model_text_info) <> "NaN", "", "chkEvaluator")}' onclick=updateRightSideComment('{i}','tooltip','open',event) aria-hidden='true'></i></span>")
                    End If
                    html.Append($" <span>{model.multi_non_pw_data(i).Title}</span>")
                    html.Append($"<div class='tooltip_wrapper hideshowinfo right-tooltip' id='tooltip_wrapper_{i}' style='display: none;'>")
                    html.Append("<div class='tooltip-header'>")
                    html.Append($"<span>{model.multi_non_pw_data(i).Title}</span>")
                    html.Append($"<div class='action_icons'>")

                    html.Append($"<a{If(model.isPipeViewOnly, " class='d-none'", "")} href='javascript:void(0);'><i class='bx bxs-edit-alt chkEvaluator d-none' onclick=showInfoPopup(decodeURIComponent({ChrW(&H22) + model_text + ChrW(&H22)}),'{i}','2','0','1','{model.multi_non_pw_data(i).sGUID}','left-node',decodeURIComponent({ChrW(&H22) + model_text_info + ChrW(&H22)}),{i})></i></a>")
                    html.Append($"<a href='javascript:void(0);'><i class='fa fa-times' aria-hidden='true' onclick=updateRightSideComment('{i}','tooltip','close',event)></i></a>")
                    html.Append("</div>")
                    html.Append("</div>")
                    html.Append($"<p class='border-top font-size-12 pt-2 text-start mb-0'>{model.multi_non_pw_data(i).Infodoc}</p>")
                    html.Append("</div>")
                    html.Append("</div>")

                    model_text = HttpUtility.UrlEncode(model.multi_non_pw_data(i).Title.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
                    model_text_info = HttpUtility.UrlEncode(model.multi_non_pw_data(i).InfodocWRT.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
                    Dim wrt_header_text As String = HttpUtility.UrlEncode(model.parent_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))

                    'If model.is_infodoc_tooltip And model.showinfodocnode Then
                    'If model.showinfodocnode Then
                    '    If (i = model.multi_non_pw_data.Count - 1) Then
                    '        html.Append("<div class='info_tooltip_main position-relative scndLastColumn ms-auto'>")
                    '    Else
                    '        html.Append("<div class='info_tooltip_main position-relative ms-auto'>")
                    '    End If

                    '    html.Append($"<span class='w-bandage {If((model_text_info) <> "" And (model_text_info) <> "NaN", "", "winfoColor")} {If((model_text_info) <> "" And (model_text_info) <> "NaN", "", "chkEvaluator")}' onclick=updateRightSideComment('{i}','bandage','open',event) id='w_bandage_{i}'>wrt</span>")
                    '    html.Append($"<div class='tooltip_wrapper hideshowinfo right-tooltips' id='w_bandage_wrapper_{i}' style='display: none;'>")
                    '    html.Append("<div class='tooltip-header'>")
                    '    html.Append($"<span>{If(model.multi_non_pw_data(i).Title.Length > 14, model.multi_non_pw_data(i).Title.Substring(0, 15) + "...", model.multi_non_pw_data(i).Title)} WRT {If(model.parent_node.Length > 14, model.parent_node.Substring(0, 15) + "...", model.parent_node)}</span>")
                    '    html.Append($"<div class='action_icons'>")

                    '    html.Append($"<a{If(model.isPipeViewOnly, " class='d-none'", "")} href='javascript:void(0);'><i class='bx bxs-edit-alt chkEvaluator d-none' onclick=showInfoPopup(decodeURIComponent({ChrW(&H22) + model_text + ChrW(&H22)}),'{i + 1}','3','0','1',null,'wrt-left-node',decodeURIComponent({ChrW(&H22) + model_text_info + ChrW(&H22)}),decodeURIComponent({ChrW(&H22) + wrt_header_text + ChrW(&H22)}))></i></a>")
                    '    html.Append($"<a href='javascript:void(0);'><i class='fa fa-times' aria-hidden='true' onclick=updateRightSideComment('{i}','bandage','close',event)></i></a>")
                    '    html.Append("</div>")
                    '    html.Append("</div>")
                    '    'html.Append($"<p class='border-top font-size-12 pt-2 text-start mb-0'>{model.multi_non_pw_data(i).Infodoc}</p>")
                    '    html.Append($"<p class='border-top font-size-12 pt-2 text-start mb-0'>{model.multi_non_pw_data(i).InfodocWRT}</p>")
                    '    html.Append("</div>")
                    '    html.Append("</div>")
                    'End If
                    html.Append("</div>")

                    html.Append($"<div class='drop_progress'{If(model.isPipeViewOnly, " style='pointer-events:none;'", "")}>")
                    html.Append($"<div class='progres_dropdowns d-none d-md-block' id='ratings-dropdown{i}'>")
                    html.Append("<div class='progress'>")
                    html.Append($"<div id='progress_bar_{i}' class='progress-bar' role='progressbar2' style='width: 100%' aria-valuenow='100' aria-valuemin='0' aria-valuemax='100'></div>")
                    html.Append("</div>")
                    html.Append("<div class='outer-wrapper'>")
                    html.Append("<div class='inner-wrapper'>")
                    html.Append("<div class='explanation'>")
                    html.Append($"<div id='dropdown{i}' class='dropdown' onclick=toggleFunction('{i}',event)>")
                    html.Append($"<div id='dropdownHeader{i}' class='dropdown__header dropdown__header--hide'>")
                    html.Append($"<div id='dropdownHeaderValue{i}' class='dropdown__header--title'>Please Select Item...</div>")
                    html.Append("<div class='dropdown__header--icon'>")
                    html.Append($"<i id='chevronIcon{i}' class='chevronIcons fas fa-angle-down rotate-icon-home'></i>")
                    html.Append("</div>")
                    html.Append("</div>")

                    html.Append($"<div id='dropdownListBody{i}' class='dropdown__body dropdown__body--hide'>")
                    html.Append($"<ul id='dropdownList{i}' class='dropdown__body--list'>")
                    If model.multi_non_pw_data(i).Ratings IsNot Nothing Then
                        For j = 0 To model.multi_non_pw_data(i).Ratings.Count - 1
                            Dim text As String = HttpUtility.UrlEncode(model.multi_non_pw_data(i).Ratings(j).Name.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
                            If model.multi_non_pw_data(i).Ratings(j).Name.ToLower() = "direct value" Or model.multi_non_pw_data(i).Ratings(j).Name.ToLower() = "not rated" Then
                                html.Append($"<li onclick=getSelectText('{i}',decodeURIComponent({ChrW(&H22) + text + ChrW(&H22)}),'{Math.Round(model.multi_non_pw_data(i).Ratings(j).Value * 100, 2)}','{model.multi_non_pw_data(i).Ratings(j).GuidID}',event) data-value='{model.multi_non_pw_data(i).Ratings(j).GuidID}#{i}' class='dropdown__body--list-index'>{model.multi_non_pw_data(i).Ratings(j).Name}</li>")
                            Else
                                If model.showPriorityAndDirect Then
                                    html.Append($"<li onclick=getSelectText('{i}',decodeURIComponent({ChrW(&H22) + text + ChrW(&H22)}),'{Math.Round(model.multi_non_pw_data(i).Ratings(j).Value * 100, 2)}','{model.multi_non_pw_data(i).Ratings(j).GuidID}',event) data-value='{model.multi_non_pw_data(i).Ratings(j).GuidID}#{i}' class='dropdown__body--list-index'>{model.multi_non_pw_data(i).Ratings(j).Name} <span>{Math.Round(model.multi_non_pw_data(i).Ratings(j).Value * 100, 2)}%</span></li>")
                                Else
                                    html.Append($"<li onclick=getSelectText('{i}',decodeURIComponent({ChrW(&H22) + text + ChrW(&H22)}),'{Math.Round(model.multi_non_pw_data(i).Ratings(j).Value * 100, 2)}','{model.multi_non_pw_data(i).Ratings(j).GuidID}',event) data-value='{model.multi_non_pw_data(i).Ratings(j).GuidID}#{i}' class='dropdown__body--list-index'>{model.multi_non_pw_data(i).Ratings(j).Name}</li>")
                                End If
                            End If
                        Next
                    End If
                    html.Append("</ul>")
                    html.Append("</div>")
                    html.Append("</div>")
                    html.Append("</div>")
                    html.Append("</div>")
                    html.Append("</div>")
                    html.Append("</div>")
                    If model.showPriorityAndDirect Then
                        html.Append("<div class='inputs_value'>")
                        html.Append($"<input type='text' onkeypress='return isNumberKeyWithDecimal(this, event);' class='value_control TabMultiRate' title='Enter number 0 to 1' min='0' max='1' onkeyup=setValuesFromText('{i}','1') id='input_1_{i}'>")
                        html.Append("</div>")
                    End If

                    'If (model.multi_non_pw_data(i).RatingID = -1 And model.multi_non_pw_data(i).DirectData >= 0) Or model.multi_non_pw_data(i).RatingID >= 0 Then
                    html.Append($"<div class='close_icon' id='close_icon_{i}' onclick=ResetValue('{model.multi_non_pw_data(i).sGUID}','{i}',event)>")
                    html.Append("<i class='fa fa-times' aria-hidden='true'></i>")
                    html.Append("</div>")
                    'End If
                    html.Append("</div>")
                    html.Append("</div>")


                Next
            End If
            html.Append("</div>")
            html.Append("</div>")
            '=============================

            'right side radio button item wise
            'id='divRadioButtons'
            html.Append("<div class='col-lg-6 d-lg-block d-none'><div class='innerRadioButtons'>")
            'Dim item = From x In model.multi_non_pw_data Where x.RatingID = -1
            Dim item = If(dataId.Trim() = "", model.multi_non_pw_data.FirstOrDefault(), model.multi_non_pw_data(Convert.ToInt32(dataId)))
            If item Is Nothing Then
                item = model.multi_non_pw_data(If(dataId <> "", Convert.ToInt32(dataId), 0))
            End If
            dataId = If(dataId = "", "0", dataId)

            html.Append($"<div class='divtable_title' title='{item.Title}'>")
            html.Append($"<p> {item.Title}</p>")
            html.Append("</div>")
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
            If item.Ratings IsNot Nothing Then
                For j = 0 To item.Ratings.Count - 1
                    If model.showPriorityAndDirect Or item.Ratings(j).Name <> "Direct Value" Then
                        html.Append("<div class='form-check radio_progress customvalue'>")
                        html.Append($"<input{If(model.isPipeViewOnly, " style='pointer-events:none;'", "")} class='form-check-input intensity{j}' onchange=SetValuesFromRadio('{If(dataId = "", "0", dataId)}','{j}') onclick=SetValuesFromRadio('{If(dataId = "", "0", dataId)}','{j}') type='radio' name='{item.sGUID}' id='{item.Ratings(j).GuidID}' value='{item.Ratings(j).Value}' checked>")
                        html.Append($"<label{If(model.isPipeViewOnly, " style='pointer-events:none;'", "")} class='form-check-label' >")
                        If (item.Ratings(j).Comment = "") Then
                            html.Append($"<div class='radio_btnname'>{item.Ratings(j).Name}<span class='idesc intensitydesc{j}'></span></div>")
                        Else
                            html.Append($"<div class='radio_btnname'>{item.Ratings(j).Name}<span title='{item.Ratings(j).Comment}' class='idesc intensitydesc{j}'>{" - " + item.Ratings(j).Comment}<span></div>")
                        End If

                        'html.Append("</div>")
                        html.Append("</label>")
                        html.Append("<div class='tooltips_group1 w-auto'>")
                        If (j = model.multi_non_pw_data.Count - 1) Then
                            html.Append("<div class='info_tooltip_main position-relative scndLastColumn'>")
                        Else
                            html.Append("<div class='info_tooltip_main position-relative'>")
                        End If

                        html.Append("<div class='comment_div'>")
                        If j <> item.Ratings.Count - 1 Then
                            html.Append($"<i class='bx bxs-edit-alt chkEvaluator d-none' onclick='showNode(6{j})'></i>")
                        End If
                        html.Append($"<div class='info_tooltip right_tooltip hideshowinfo' id='parentnode6{j}'  style='display: none;'>")
                        html.Append("<div class='d-flex justify-content-between'>")
                        html.Append("<span>Edit intensity description</span>")
                        html.Append($"<div class='action_icons'><a href='javascript: void (0);' onclick='toggleBox(6{j})'><i class='fa fa-times' aria-hidden='true'></i></a></div>")
                        html.Append("</div>")
                        'html.Append($"<textarea{If(model.isPipeViewOnly, " style='pointer-events:none;'", "")} class='form-control mb-3 mt-2 w-100'id='txtleftComment_{j}' rows='3'>{item.Ratings(j).Comment}</textarea>")
                        'html.Append("<div class='comt_btn text-end'>")
                        'html.Append($"<button{If(model.isPipeViewOnly, " class='d-none'", "")} type='button' onclick=updateRightSideComment('{j}','showcomment','close',event)><i class='fa fa-check' aria-hidden='true'></i> OK</button>")

                        'html.Append("</div>")
                        html.Append("</div>")
                        html.Append("</div>")
                        html.Append("</div>")
                        html.Append("</div>")
                        If model.showPriorityAndDirect Then
                            If j <> item.Ratings.Count - 1 Then
                                html.Append($"<div class='radio_btn_progress' onclick=SetValuesFromRadio('{If(dataId = "", "0", dataId)}','{j}')>")
                                html.Append("<div class='progress'>")
                                html.Append($"<div class='progress-bar' onclick=SetValuesFromRadio('{If(dataId = "", "0", dataId)}','{j}') role='progressbar3' style='width: {item.Ratings(j).Value * 100}%' aria-valuenow='{item.Ratings(j).Value * 100}' aria-valuemin='0' aria-valuemax='100' onclick=SetValuesFromRadio('{If(dataId = "", "0", dataId)}','{j}')></div>")
                                html.Append("</div>")
                                Dim active_multi_index = model.multi_non_pw_data.Where(Function(x) x.RatingID = -1).Select(Function(y) y.ID).FirstOrDefault().ToString()
                                If j <> item.Ratings.Count - 1 Then
                                    If item.Ratings(j).Name.ToLower() = "direct value" Or item.Ratings(j).Name.ToLower() = "not rated" Then
                                        html.Append($"")
                                    Else
                                        html.Append($"{Math.Round(item.Ratings(j).Value * 100, 2)}%")
                                    End If
                                ElseIf j <> item.Ratings.Count - 2 Then
                                    html.Append($"")
                                End If
                            Else
                                ''  html += "<input type='text' class='custom-value TabNextMultRate' id='input_2_" + index + "' onkeyup=setValuesFromText('" + index + "','2')>";
                                html.Append($"<div class='radio_btn_progress btnprogress'{If(model.isPipeViewOnly, " style='pointer-events:none;'", "")}>")
                                html.Append($"<input type='text' onkeypress='return isNumberKeyWithDecimalMR(this, event);' class='custom-value' id='input_2_{dataId}' onkeyup=setValuesFromText('{dataId}','2')>")
                                html.Append($"<i class='fa fa-times custom-value-closs' aria-hidden='true' onclick=ResetValue('{item.sGUID}','{dataId}',event)></i>")
                            End If
                            html.Append("</div>")
                        End If

                        html.Append("</div>")
                    End If
                Next
            End If

            If model.showinfodocnode Then
                model_text = If(model.ScaleDescriptions.Count > 0, HttpUtility.UrlEncode(model.ScaleDescriptions(0).Name.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))), "")
                model_text_info = If(model.ScaleDescriptions.Count > 0, HttpUtility.UrlEncode(model.ScaleDescriptions(0).Description.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))), "")
                html.Append("<div class='bottom_info'>")
                html.Append("<div class='info_tooltip_main position-relative'>")
                html.Append($"<i class='fa fa-info-circle infocolorheader {If(model_text_info <> "" And model_text_info <> "NaN", "", " infoColor chkEvaluator")}' aria-hidden='true' onclick=updateRightSideComment('0','ScaleDescriptions','open',event)></i> <span class='{If(model_text_info <> "" And model_text_info <> "NaN", "", "chkEvaluator")}'>{If(model.ScaleDescriptions.Count > 0, model.ScaleDescriptions(0).Name, "")}</span>")
                html.Append("<div class='tooltip_wrapper hideshowinfo tooltip_show' id='ScaleDescriptions' style='display: none;'>")
                html.Append("<div class='tooltip-header'>")
                html.Append($"<span>{model.parent_node}</span>")
                html.Append("<div class='action_icons'>")

                html.Append($"<a href='javascript:void(0);'><i class='bx bxs-edit-alt chkEvaluator d-none' onclick='showInfoPopup(decodeURIComponent({ChrW(&H22) + model_text + ChrW(&H22)}),4,4,0,1,null,decodeURIComponent({ChrW(&H22) + "scale-node" + ChrW(&H22)}),decodeURIComponent({ChrW(&H22) + model_text_info + ChrW(&H22)}), decodeURIComponent({ChrW(&H22) + "scale" + ChrW(&H22)}))'></i></a>")
                'html.Append($"<a href='javascript:void(0);' class='edit_icon chkEvaluator' onclick=showInfoPopup(decodeURIComponent({ChrW(&H22) + model_text + ChrW(&H22)}),'4','4','0','1',null,'scale-node',decodeURIComponent({ChrW(&H22) + model_text_info + ChrW(&H22)}))>")
                html.Append("<a href='javascript:void(0);'><i class='fa fa-times' aria-hidden='true' onclick=updateRightSideComment('0','ScaleDescriptions','close',event)></i></a>")
                html.Append("</div>")
                html.Append("</div>")
                html.Append($"<p class='border-top font-size-12 pt-2 text-start mb-0'>{If(model.ScaleDescriptions.Count > 0, model.ScaleDescriptions(0).Description, "")}</p>")
                html.Append("</div>")
                html.Append("</div>")
                html.Append("</div>")
            End If
            '=============================
            html.Append("</div>")
            html.Append("</div>")
            html.Append("</div>")

            ' divContent.InnerHtml = html.ToString()
        End If





        If model IsNot Nothing Then
            Dim html As StringBuilder = New StringBuilder()
            Dim model_text As String = HttpUtility.UrlEncode(model.cluster_phrase.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
            Dim model_text_info As String = HttpUtility.UrlEncode(model.parent_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
            html.Append("<div  id=''>")
            html.Append("<div class='page_heading_section' style='padding-top:20px;'>")
            html.Append("<div class='container'>")
            html.Append("<div class='row'>")
            html.Append("<div class='col-md-6'>")

            html.Append("<div class='heading_content'><div class='page-title-box' id='titleDiv'>")
            html.Append("<div class='heading_icons'>")
            html.Append($"<a {If(model.isPipeViewOnly, " class='d-none'", " class='editsvg_icon'")}  href='javascript:void(0);' onclick=showHeaderPopup(decodeURIComponent({ChrW(&H22) + model_text + ChrW(&H22)}),'0','0','0','0',null,'question-node',decodeURIComponent({ChrW(&H22) + model_text + ChrW(&H22)}))><img class='chkEvaluatorone{If(model.isPM, "", " d-none")}' src='../../img/icon/edit-icon.svg'></a>")
            html.Append("<asp:Label ID='lblTask' runat='server' ><span id='divT2S' style='font-size:20px; margin-left:1ex border: 0px !important; background: #f0f0ff08 !important; display:none'></span></asp:Label>")
            html.Append("</div>")
            html.Append($"<div class='txtStepTask removep' id='MainHeaderInfodoc'>{model.step_task}<span class='threedoticon d-none' id='btnheadinfo'> <a href='#' onclick='showheaderpopup()' ><div class='snippet'><div class='stage'><div class='dot-falling'></div> </div></div></a></span></div>")

            Dim IsHideHeaderInfo As Boolean = False
            'If model.parent_node_info Is Nothing Or model.parent_node_info = "" Then
            '    html.Append($"<div class='heading_info' id='HeaderDivIcon'><a href ='javascript:void(0);'><img  id='img1' src='../../img/icon/empty_info.svg'><img id='img2' src='../../img/icon/info_data.svg' style='display: none;'><img id='img3' src='../../img/icon/info-close.svg' style='display: none;'></a></div>")
            '    IsHideHeaderInfo = True
            'Else
            '    html.Append($"<div class='heading_info' id='HeaderDivIcon'><a href ='javascript:void(0);'><img  id='img1' src='../../img/icon/empty_info.svg' style='display: none;'><img id='img2' src='../../img/icon/info_data.svg'><img id='img3' src='../../img/icon/info-close.svg' style='display: none;'></a></div>")
            'End If
            'html.Append("<div class='heading_info'><img src='../../img/icon/empty_info.svg' class='d-none'><img src='../../img/icon/info_data.svg'><img src='../../img/icon/info-close.svg' class='d-none'></div>")
            html.Append("</div>")
            html.Append("</div>")

            html.Append("</div>")

            html.Append($"<div class='col-md-6{If(model.parent_node_info.ToString().Trim() = "", " NoInfoData", "")}'>")
            html.Append($"<div class='info_content d-flex mt-lg-0 mt-2' id='Header_InfoDocs' {If(IsHideHeaderInfo, " style='display: none;'", "")}><div class='heading_icons heading_info me-0'><div  id='HeaderDivIcon'><a href='#'><img  id='img1' src='../../img/icon/empty_info.svg' style='display: none;'><img id='img2' src='../../img/icon/info_data.svg'><img id='img3' src='../../img/icon/info-close.svg' style='display: none;'></a></div><div class='infodoc_icons'><a class='editsvg_icon' href='javascript:void(0);' onclick=showInfoPopup(decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.parent_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}),'0','-1','0','-1',null,'parent-node',decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.parent_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}))><img class='chkEvaluatorone' src='../../img/icon/edit-icon.svg'></a><a href='#' onclick=Expandtxt(decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.parent_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}),'0','-1','0','-1',null,'parent-node',decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.parent_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)})) data-bs-toggle='modal' data-bs-target='#infodocPop'><img src='../../img/icon/full-screen-svgrepo-com.svg' id='ibtnFullscreen'></a></div></div>")
            html.Append("<div class='info_content_wrapper'>")
            html.Append($"<div class='info_text removep1'><p>{model.parent_node_info.ToString().Trim()}</p></div>")

            html.Append($"<div class='info_box_icons' style='display:none;'><a href='javascript:void(0);' onclick=showInfoPopup(decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.parent_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}),'0','-1','0','-1',null,'parent-node',decodeURIComponent({ChrW(&H22) + model_text_info + ChrW(&H22)}),'header') class='editsvg_icon'><img class='chkEvaluatorone{If(model.isPM, "", " d-none")}' src='../../img/icon/edit-icon.svg'></a>
            <a href='javascript:void(0);' data-bs-toggle='modal' data-bs-target='#infodocPop' onclick=onclick=Expandtxt(decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.parent_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}),'0','-1','0','-1',null,'parent-node',decodeURIComponent({ChrW(&H22) + model_text_info + ChrW(&H22)}),'header')><img src='../../img/icon/full-screen-svgrepo-com.svg'></a></div>")
            html.Append("</div>")
            html.Append("</div>")
            html.Append("</div>")

            html.Append("</div>")
            html.Append("</div>")
            html.Append("</div>")

            'html.Append("<div class='fixed_info'>")
            'html.Append("<div class='question_fix'><a href='javascript:void(0);' data-bs-toggle='modal' data-bs-target='#questinModal'>
            '    <img src='../../img/icon/question-mark-white.svg'></a><hr><a href='javascript:void(0);' class='edit_icon'>
            '    <img src='../../img/icon/edit-white.svg'></a>")
            'html.Append("</div>")
            'html.Append("</div>")

            html.Append("<div class='container'>")
            html.Append("<div class='row'>")
            html.Append("<div class='col-lg-6'>")
            html.Append("<div class='progress_table_row progress_main_container rating_scale_data'>")

            Dim count As Integer = 0, dataId As String = "", dataCount As Integer = 0
            'Dim ActRowID As Integer = 0
            'If Request.Cookies("ActiveRowID") IsNot Nothing Then
            '    Dim ActiveRowID As HttpCookie = Request.Cookies("GenActiveRowID")
            '    ActRowID = Convert.ToInt32(ActiveRowID.Value)
            'Else
            '    Dim ActiveRowID As New HttpCookie("GenActiveRowID")
            '    ActiveRowID.Value = "0"
            '    'ActiveRowID.Values("ActiveRowID") = "No"
            '    If (ActiveRowID.Value.Contains("ActiveRowID")) Then
            '        ActiveRowID.Value = ActiveRowID.Value.Substring(12)
            '    End If
            '    Response.Cookies.Add(ActiveRowID)
            'End If
            'Dim InfoDocSaveRowID As Integer = 0
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
            Dim NoInfoDocDataCls As String = ""
            Dim NoInfoDocRow As String = ""
            Dim nodataonesideinfodocrow As String = ""
            If model.multi_non_pw_data IsNot Nothing Then
                For i = 0 To model.multi_non_pw_data.Count - 1
                    NoInfoDocRow = ""
                    NoInfoDocDataCls = ""
                    nodataonesideinfodocrow = ""
                    If model.multi_non_pw_data(i).InfodocWRT.Trim() <> "" Or model.multi_non_pw_data(i).Infodoc.Trim() <> "" Then
                        If model.multi_non_pw_data(i).InfodocWRT.Trim() = "" Or model.multi_non_pw_data(i).Infodoc.Trim() = "" Then
                            nodataonesideinfodocrow = " nodataonesideinfodocrow"
                        End If
                    Else
                        NoInfoDocDataCls = " NoInfoData"
                        NoInfoDocRow = "NoInfDocDataRow "
                    End If
                    dataCount = dataCount + 1
                    Dim active_item = model.multi_non_pw_data.Where(Function(x) x.RatingID = -1).FirstOrDefault()
                    If active_item Is Nothing Then
                        'html.Append($"<div id='div_row_wrapper_{i}' class='row_wrapper{If(i = ActRowID, " active_table_row", "")}' onclick=addActiveClass(this,'','{i}');>")
                        html.Append($"<div id='div_row_wrapper_{i}' class='{NoInfoDocRow}row_wrapper {If(i = 0, " active_table_row", "")}{nodataonesideinfodocrow}' onclick=addActiveClass(this,'','{i}');>")
                    Else
                        If count = 0 And dataId = "" And model.multi_non_pw_data(i).RatingID = -1 Then
                            html.Append($"<div id='div_row_wrapper_{i}' class='{NoInfoDocRow}row_wrapper{If(model.multi_non_pw_data(i).RatingID = -1, " active_table_row", "")}{nodataonesideinfodocrow}' onclick=addActiveClass(this,'','{i}');>")
                            'html.Append($"<div id='div_row_wrapper_{i}' class='row_wrapper {If(model.multi_non_pw_data(i).RatingID = -1, "active_table_row", "")}' onclick=addActiveClass(this,'','{i}');>")
                            dataId = i.ToString()
                            'hdnactive_multi_index.Value = i.ToString()
                            hdnactive_multi_index.Value = "0"
                        Else
                            html.Append($"<div id='div_row_wrapper_{i}' class='{NoInfoDocRow}row_wrapper{nodataonesideinfodocrow}' onclick=addActiveClass(this,'','{i}');>")
                        End If
                        count = If(count = 0, If(model.multi_non_pw_data(i).RatingID = -1, 1, 0), 1)
                    End If


                    html.Append("<div class='tooltips_group'>")
                    If model.show_comments Or Not model.show_comments Then
                        If (i = model.multi_non_pw_data.Count - 1) Then
                            html.Append($"<div class='info_tooltip_main position-relative scndLastColumn {NoInfoDocDataCls}'>")
                        Else
                            html.Append($"<div class='info_tooltip_main position-relative{NoInfoDocDataCls}'>")
                        End If

                        html.Append($"<div class='comment_div' id='comment_div_{i}'><div class='information_icon'>")
                        If model.multi_non_pw_data(i).Comment Is Nothing Or model.multi_non_pw_data(i).Comment = "" Then
                            html.Append($"<a href ='javascript:void(0);'><img src='../../img/icon/info_data.svg' id='comment_icon_{i}' aria-hidden='true'></img></a>")
                            '' html.Append($"<img src='../../img/icon/info_down.svg' id='comment_icon_{i}' aria-hidden='true' onclick=updateRightSideComment('{i}','comment','open',event)></img>")
                        Else
                            html.Append($"<a href ='javascript:void(0);'><img src='../../img/icon/info_data.svg' id='comment_icon_{i}' aria-hidden='true'></img></a>")
                            '' html.Append($"<img src='../../img/icon/info_down.svg' id='comment_icon_{i}' aria-hidden='true' onclick=updateRightSideComment('{i}','comment','open',event)></img>")
                        End If
                        'html.Append($"<div class='tooltip_wrapper hideshowinfo' id='comment_div_box_{i}' style='display: none;'>")
                        'html.Append("<div class='d-flex justify-content-between tooltip-header'>")
                        'html.Append("<span>Add your comment</span>")
                        'html.Append($"<div class='action_icons' onclick=updateRightSideComment('{i}','comment','close',event)>")
                        'html.Append("<a href='javascript:void(0);'><i class='fa fa-times' aria-hidden='true'></i></a>")
                        'html.Append("</div></div>")
                        'html.Append($"<textarea{If(model.isPipeViewOnly, " style='pointer-events:none;'", "")} class='form-control mb-3 mt-2 w-100' id='txtRightComment_{i}' rows='3'>{model.multi_non_pw_data(i).Comment}</textarea>")
                        'html.Append("<div class='comt_btn text-end'>")
                        'html.Append($"<button{If(model.isPipeViewOnly, " class='d-none'", "")} type='button' onclick=updateRightSideComment('{i}','comment','close',event)><i class='fa fa-check' aria-hidden='true'></i> OK</button>")
                        'html.Append("</div>")
                        'html.Append("</div>")


                        html.Append("</div>")
                        html.Append("</div>")
                        html.Append("</div>")
                    End If

                    If (i = model.multi_non_pw_data.Count - 1) Then
                        If model.show_comments <> True Then
                            html.Append("<div class='info_tooltip_main position-relative scndLastColumn' style='display: none;'>")
                        Else
                            html.Append("<div class='info_tooltip_main position-relative scndLastColumn'>")
                        End If

                    Else
                        If model.show_comments <> True Then
                            html.Append("<div class='info_tooltip_main position-relative' style='display: none;'>")
                        Else
                            html.Append("<div class='info_tooltip_main position-relative'>")
                        End If

                    End If
                    'If model.is_infodoc_tooltip And model.showinfodocnode Then
                    model_text = HttpUtility.UrlEncode(model.multi_non_pw_data(i).Title.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
                    model_text_info = HttpUtility.UrlEncode(model.multi_non_pw_data(i).Infodoc.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
                    If model.show_comments Then
                        html.Append($"<div class='comment_icon'>")
                        If model.multi_non_pw_data(i).Comment Is Nothing Or model.multi_non_pw_data(i).Comment = "" Then
                            If Not (screenCheck.isMobileBrowserClient(Request)) Then
                                html.Append($"<a href ='javascript:void(0);' id='aUpdateRightSideCommentBlank{i}' onclick=updateRightSideComment('{i}','comment','open',event) aria-hidden='true'> <img src = '../../img/icon/Grey-plus-icon.svg' id='ImgComment_iconEmpty_{i}' title='' aria-hidden='true'> <img src = '../../img/icon/comment-svgrepo-com.svg' style='display:none;' id='ImgComment_icon_{i}' title='' aria-hidden='true'></a>")
                            Else
                                html.Append($"<a href ='javascript:void(0);' id='aUpdateRightSideCommentBlankM{i}' onclick=updateRightSideCommentMobile('{i}','comment','open',event) aria-hidden='true'> <img src = '../../img/icon/Grey-plus-icon.svg' id='ImgComment_iconEmpty_{i}' title='' aria-hidden='true'> <img src = '../../img/icon/comment-svgrepo-com.svg' style='display:none;' id='ImgComment_icon_{i}' title='' aria-hidden='true'></a>")
                            End If
                        Else
                            If Not (screenCheck.isMobileBrowserClient(Request)) Then
                                html.Append($"<a href ='javascript:void(0);' id='aUpdateRightSideComment{i}' onclick=updateRightSideComment('{i}','comment','open',event) aria-hidden='true'> <img src = '../../img/icon/Grey-plus-icon.svg' id='ImgComment_iconEmpty_{i}' style='display:none;' title='' aria-hidden='true'>  <img src = '../../img/icon/comment-svgrepo-com.svg' id='ImgComment_icon_{i}' title='' aria-hidden='true'></a>")
                            Else
                                html.Append($"<a href ='javascript:void(0);' id='aUpdateRightSideComment{i}' onclick=updateRightSideCommentMobile('{i}','comment','open',event) aria-hidden='true'> <img src = '../../img/icon/Grey-plus-icon.svg' id='ImgComment_iconEmpty_{i}' style='display:none;' title='' aria-hidden='true'>  <img src = '../../img/icon/comment-svgrepo-com.svg' id='ImgComment_icon_{i}' title='' aria-hidden='true'></a>")
                            End If
                        End If
                            'html.Append($"<a href ='javascript:void(0);'><img class='commentbox' src='../../img/icon/comment.svg' {If((model_text_info) = "" Or (model_text_info) = "NaN", "infoColor", "")} {If((model_text_info) <> "" And (model_text_info) <> "NaN", "", "chkEvaluator")}' onclick=updateRightSideComment('{i}','comment','open',event) aria-hidden='true'></a>")
                            html.Append($"<div class='tooltip_wrapper hideshowinfo' id='comment_div_box_{i}' style='display: none;'>")
                        html.Append("<div class='d-flex justify-content-between tooltip-header'>")
                        html.Append("<span>Add your comment</span>")
                        html.Append($"<div class='action_icons' onclick=HideMultiDerectCommentBox({i})>")
                        html.Append("<a href='javascript:void(0);'><i class='fa fa-times' aria-hidden='true'></i></a>")
                        html.Append("</div></div>")
                        html.Append($"<textarea{If(model.isPipeViewOnly, " style='pointer-events:none;'", "")} class='form-control mb-3 mt-2 w-100' placeholder='Add your comment' id='txtRightComment_{i}' rows='3'>{model.multi_non_pw_data(i).Comment}</textarea>")
                        html.Append("<div class='comt_btn text-end'>")
                        html.Append($"<button{If(model.isPipeViewOnly, " class='d-none'", "")} type='button' onclick=updateRightSideComment('{i}','comment','close',event)><i class='fa fa-check' aria-hidden='true'></i> OK</button>")
                        html.Append("</div>")
                        html.Append("</div>")
                        html.Append("</div>")
                    End If

                    html.Append("</div>")
                    html.Append($"<div class='brand_name'><span title='{model.multi_non_pw_data(i).Title}'>{model.multi_non_pw_data(i).Title}</span></div>")
                    html.Append($"<div class='tooltip_wrapper hideshowinfo right-tooltip' id='tooltip_wrapper_{i}' style='display: none;'>")
                    html.Append("<div class='tooltip-header'>")
                    html.Append($"<span>{model.multi_non_pw_data(i).Title}</span>")
                    html.Append($"<div class='action_icons'>")

                    html.Append($"<a{If(model.isPipeViewOnly, " class='d-none'", "")} href='javascript:void(0);'><i class='bx bxs-edit-alt chkEvaluator d-none' onclick=showInfoPopup(decodeURIComponent({ChrW(&H22) + model_text + ChrW(&H22)}),'{i + 1}','2','0','1',null,'left-node',decodeURIComponent({ChrW(&H22) + model_text_info + ChrW(&H22)}),{i})></i></a>")
                    html.Append($"<a href='javascript:void(0);'><i class='fa fa-times' aria-hidden='true' onclick=updateRightSideComment('{i}','tooltip','close',event)'></i></a>")
                    html.Append("</div>")


                    ''Ne uid

                    html.Append($"<div class='tooltip_wrapper hideshowinfo' id='comment_div_box_{i}' style='display: none;'>")
                    html.Append("<div class='d-flex justify-content-between tooltip-header'>")
                    html.Append("<span>Add your comment</span>")
                    html.Append($"<div class='action_icons' onclick=updateRightSideComment('{i}','comment','close',event)>")
                    html.Append("<a href='javascript:void(0);'><i class='fa fa-times' aria-hidden='true'></i></a>")
                    html.Append("</div>")
                    html.Append("</div>")
                    html.Append($"<textarea{If(model.isPipeViewOnly, " style='pointer-events:none;'", "")} class='form-control mb-3 mt-2 w-100' placeholder='Add your comment' id='txtRightComment_{i}' rows='3'>{model.multi_non_pw_data(i).Comment}</textarea>")
                    html.Append("<div class='comt_btn text-end'>")
                    html.Append($"<button{If(model.isPipeViewOnly, " class='d-none'", "")} type='button' onclick=updateRightSideComment('{i}','comment','close',event)><i class='fa fa-check' aria-hidden='true'></i> OK</button>")
                    html.Append("</div>")
                    html.Append("</div>")




                    html.Append("</div>")
                    html.Append($"<p class='border-top font-size-12 pt-2 text-start mb-0'>{model.multi_non_pw_data(i).Infodoc}</p>")
                    html.Append("</div>")
                    html.Append("</div>")

                    model_text = HttpUtility.UrlEncode(model.multi_non_pw_data(i).Title.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
                    model_text_info = HttpUtility.UrlEncode(model.multi_non_pw_data(i).InfodocWRT.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
                    Dim wrt_header_text As String = HttpUtility.UrlEncode(model.parent_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))

                    'If model.is_infodoc_tooltip And model.showinfodocnode Then
                    'If model.showinfodocnode Then
                    '    If (i = model.multi_non_pw_data.Count - 1) Then
                    '        html.Append("<div class='info_tooltip_main position-relative scndLastColumn ms-auto'>")
                    '    Else
                    '        html.Append("<div class='info_tooltip_main position-relative ms-auto'>")
                    '    End If

                    '    html.Append($"<span class='w-bandage {If((model_text_info) <> "" And (model_text_info) <> "NaN", "", "winfoColor")} {If((model_text_info) <> "" And (model_text_info) <> "NaN", "", "chkEvaluator")}' onclick=updateRightSideComment('{i}','bandage','open',event) id='w_bandage_{i}'>wrt</span>")
                    '    html.Append($"<div class='tooltip_wrapper hideshowinfo right-tooltips' id='w_bandage_wrapper_{i}' style='display: none;'>")
                    '    html.Append("<div class='tooltip-header'>")
                    '    html.Append($"<span>{If(model.multi_non_pw_data(i).Title.Length > 14, model.multi_non_pw_data(i).Title.Substring(0, 15) + "...", model.multi_non_pw_data(i).Title)} WRT {If(model.parent_node.Length > 14, model.parent_node.Substring(0, 15) + "...", model.parent_node)}</span>")
                    '    html.Append($"<div class='action_icons'>")

                    '    html.Append($"<a{If(model.isPipeViewOnly, " class='d-none'", "")} href='javascript:void(0);'><i class='bx bxs-edit-alt chkEvaluator d-none' onclick=showInfoPopup(decodeURIComponent({ChrW(&H22) + model_text + ChrW(&H22)}),'{i + 1}','3','0','1',null,'wrt-left-node',decodeURIComponent({ChrW(&H22) + model_text_info + ChrW(&H22)}),decodeURIComponent({ChrW(&H22) + wrt_header_text + ChrW(&H22)}))></i></a>")
                    '    html.Append($"<a href='javascript:void(0);'><i class='fa fa-times' aria-hidden='true' onclick=updateRightSideComment('{i}','bandage','close',event)></i></a>")
                    '    html.Append("</div>")
                    '    html.Append("</div>")
                    '    'html.Append($"<p class='border-top font-size-12 pt-2 text-start mb-0'>{model.multi_non_pw_data(i).Infodoc}</p>")
                    '    html.Append($"<p class='border-top font-size-12 pt-2 text-start mb-0'>{model.multi_non_pw_data(i).InfodocWRT}</p>")
                    '    html.Append("</div>")
                    '    html.Append("</div>")
                    'End If
                    'html.Append("</div>")

                    Dim DropDownListID As String = "UiDropdownList" & i

                    html.Append($"<div class='drop_progress'{If(model.isPipeViewOnly, " style='pointer-events:none;'", "")}>")
                    html.Append($"<div class='progres_dropdowns d-none d-md-block' id='ratings-dropdown{i}' onclick='ShowDropDownListDesktop({DropDownListID}D)'>")
                    html.Append("<div class='progress' style='height: 25px;'>")
                    html.Append($"<div id='progress_bar_{i}' class='progress-bar' role='progressbar' style='width: 100%' aria-valuenow='100' aria-valuemin='0' aria-valuemax='100'><span id='dropdownHeaderValue{i}'  ></span></div><small class='customdropdown_btn'></small>")
                    html.Append("</div>")


                    html.Append($"<div class='custom_dropdown custom_dropdown_dekstop w-100 d-block'>
                            <div class='options'>
                              <ul id='{DropDownListID}D'>")
                    If model.multi_non_pw_data(i).Ratings IsNot Nothing Then
                        For j = 0 To model.multi_non_pw_data(i).Ratings.Count - 1
                            Dim text As String = HttpUtility.UrlEncode(model.multi_non_pw_data(i).Ratings(j).Name.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
                            If model.multi_non_pw_data(i).Ratings(j).Name.ToLower() = "direct value" Or model.multi_non_pw_data(i).Ratings(j).Name.ToLower() = "not rated" Then
                                html.Append($"<li onclick=getSelectText('{i}',decodeURIComponent({ChrW(&H22) + text + ChrW(&H22)}),'{Math.Round(model.multi_non_pw_data(i).Ratings(j).Value * 100, 2)}','{model.multi_non_pw_data(i).Ratings(j).GuidID}',event) data-value='{model.multi_non_pw_data(i).Ratings(j).GuidID}#{i}'> <a href='javascript:void(0);'>
                    {model.multi_non_pw_data(i).Ratings(j).Name}<b></b> <span class='value'>{i}</span></a>   </li>")
                            Else
                                If model.showPriorityAndDirect Then
                                    html.Append($"<li onclick=getSelectText('{i}',decodeURIComponent({ChrW(&H22) + text + ChrW(&H22)}),'{Math.Round(model.multi_non_pw_data(i).Ratings(j).Value * 100, 2)}','{model.multi_non_pw_data(i).Ratings(j).GuidID}',event) data-value='{model.multi_non_pw_data(i).Ratings(j).GuidID}#{i}'><a href='javascript:void(0);'>
                    {model.multi_non_pw_data(i).Ratings(j).Name}<b>{Math.Round(model.multi_non_pw_data(i).Ratings(j).Value * 100, 2)}% </b> <span class='value'>{i}</span></a>   </li>")
                                Else
                                    html.Append($"<li onclick=getSelectText('{i}',decodeURIComponent({ChrW(&H22) + text + ChrW(&H22)}),'{Math.Round(model.multi_non_pw_data(i).Ratings(j).Value * 100, 2)}','{model.multi_non_pw_data(i).Ratings(j).GuidID}',event) data-value='{model.multi_non_pw_data(i).Ratings(j).GuidID}#{i}'><a href='javascript:void(0);'>
                    {model.multi_non_pw_data(i).Ratings(j).Name}<b></b> <span class='value'>{i}</span></a></li>")
                                End If
                            End If
                        Next
                    End If
                    html.Append("</ul>
                            </div>
                          </div>")
                    'html.Append("<div class='outer-wrapper'>")
                    'html.Append("<div class='inner-wrapper'>")
                    'html.Append("<div class='explanation'>")
                    'html.Append($"<div id='dropdown{i}' class='dropdown' onclick=toggleFunction('{i}',event)>")
                    'html.Append($"<div id='dropdownHeader{i}' class='dropdown__header dropdown__header--hide'>")
                    ''  html.Append($"<div id='dropdownHeaderValue{i}' class='dropdown__header--title'>Please Select Item...</div>")
                    'html.Append("<div class='dropdown__header--icon'>")
                    'html.Append($"<i id='chevronIcon{i}' class='chevronIcons fas fa-angle-down rotate-icon-home'></i>")
                    'html.Append("</div>")
                    'html.Append("</div>")

                    'html.Append($"<div id='dropdownListBody{i}' class='dropdown__body dropdown__body--hide'>")
                    'html.Append($"<ul id='dropdownList{i}' class='dropdown__body--list'>")
                    'If model.multi_non_pw_data(i).Ratings IsNot Nothing Then
                    '    For j = 0 To model.multi_non_pw_data(i).Ratings.Count - 1
                    '        Dim text As String = HttpUtility.UrlEncode(model.multi_non_pw_data(i).Ratings(j).Name.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
                    '        If model.multi_non_pw_data(i).Ratings(j).Name.ToLower() = "direct value" Or model.multi_non_pw_data(i).Ratings(j).Name.ToLower() = "not rated" Then
                    '            html.Append($"<li onclick=getSelectText('{i}',decodeURIComponent({ChrW(&H22) + text + ChrW(&H22)}),'{model.multi_non_pw_data(i).Ratings(j).Value * 100}','{model.multi_non_pw_data(i).Ratings(j).GuidID}',event) data-value='{model.multi_non_pw_data(i).Ratings(j).GuidID}#{i}' class='dropdown__body--list-index'>{model.multi_non_pw_data(i).Ratings(j).Name}</li>")
                    '        Else
                    '            If model.showPriorityAndDirect Then
                    '                html.Append($"<li onclick=getSelectText('{i}',decodeURIComponent({ChrW(&H22) + text + ChrW(&H22)}),'{model.multi_non_pw_data(i).Ratings(j).Value * 100}','{model.multi_non_pw_data(i).Ratings(j).GuidID}',event) data-value='{model.multi_non_pw_data(i).Ratings(j).GuidID}#{i}' class='dropdown__body--list-index'>{model.multi_non_pw_data(i).Ratings(j).Name} <span>{Math.Round(model.multi_non_pw_data(i).Ratings(j).Value * 100, 2)}%</span></li>")
                    '            Else
                    '                html.Append($"<li onclick=getSelectText('{i}',decodeURIComponent({ChrW(&H22) + text + ChrW(&H22)}),'{model.multi_non_pw_data(i).Ratings(j).Value * 100}','{model.multi_non_pw_data(i).Ratings(j).GuidID}',event) data-value='{model.multi_non_pw_data(i).Ratings(j).GuidID}#{i}' class='dropdown__body--list-index'>{model.multi_non_pw_data(i).Ratings(j).Name}</li>")
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
                              <a href='javascript:void(0);'><span id='dropdownHeaderValueM{i}'><span id='spndropdownHeaderValueM{i}' class='direct_value_data'>DataValu</span></span><small class='customdropdown_btn'></small></a>
                            </div>
                            <div class='options'>
                              <ul id='{DropDownListID}'>")
                    If model.multi_non_pw_data(i).Ratings IsNot Nothing Then
                        For j = 0 To model.multi_non_pw_data(i).Ratings.Count - 1
                            Dim text As String = HttpUtility.UrlEncode(model.multi_non_pw_data(i).Ratings(j).Name.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
                            If model.multi_non_pw_data(i).Ratings(j).Name.ToLower() = "direct value" Or model.multi_non_pw_data(i).Ratings(j).Name.ToLower() = "not rated" Then
                                html.Append($"<li onclick=getSelectText('{i}',decodeURIComponent({ChrW(&H22) + text + ChrW(&H22)}),'{Math.Round(model.multi_non_pw_data(i).Ratings(j).Value * 100, 2)}','{model.multi_non_pw_data(i).Ratings(j).GuidID}',event) data-value='{model.multi_non_pw_data(i).Ratings(j).GuidID}#{i}'> <a href='javascript:void(0);'>
                    {model.multi_non_pw_data(i).Ratings(j).Name}<b></b> <span class='value'>{i}</span></a>   </li>")
                            Else
                                If model.showPriorityAndDirect Then
                                    html.Append($"<li onclick=getSelectText('{i}',decodeURIComponent({ChrW(&H22) + text + ChrW(&H22)}),'{Math.Round(model.multi_non_pw_data(i).Ratings(j).Value * 100, 2)}','{model.multi_non_pw_data(i).Ratings(j).GuidID}',event) data-value='{model.multi_non_pw_data(i).Ratings(j).GuidID}#{i}'><a href='javascript:void(0);'>
                    {model.multi_non_pw_data(i).Ratings(j).Name}<b>{Math.Round(model.multi_non_pw_data(i).Ratings(j).Value * 100, 2)}% </b> <span class='value'>{i}</span></a>   </li>")
                                Else
                                    html.Append($"<li onclick=getSelectText('{i}',decodeURIComponent({ChrW(&H22) + text + ChrW(&H22)}),'{Math.Round(model.multi_non_pw_data(i).Ratings(j).Value * 100, 2)}','{model.multi_non_pw_data(i).Ratings(j).GuidID}',event) data-value='{model.multi_non_pw_data(i).Ratings(j).GuidID}#{i}'><a href='javascript:void(0);'>
                    {model.multi_non_pw_data(i).Ratings(j).Name}<b></b> <span class='value'>{i}</span></a></li>")
                                End If
                            End If
                        Next
                    End If
                    html.Append("</ul>
                            </div>
                          </div>")
                    If model.showPriorityAndDirect Then
                        html.Append("<div class='brand_value'>")
                        html.Append($"<input type='text' onkeypress='return isNumberKeyWithDecimal(this, event);' class='value_control TabMultiRate' title='Enter number 0 to 1' min='0' max='1' onkeyup=setValuesFromText('{i}','1') id='input_1_{i}'>")
                        html.Append("</div>")
                    End If

                    'If (model.multi_non_pw_data(i).RatingID = -1 And model.multi_non_pw_data(i).DirectData >= 0) Or model.multi_non_pw_data(i).RatingID >= 0 Then
                    html.Append($"<div class='close_icon mt-1' id='close_icon_{i}' onclick=ResetValue('{model.multi_non_pw_data(i).sGUID}','{i}',event)>")
                    html.Append("<img src='../../img/icon/erasar.svg' style='cursor: pointer;' class='imgClose' >")
                    html.Append("</div>")
                    'End If
                    html.Append("</div>")
                    Dim guid As String = model.multi_non_pw_data(i).sGUID.ToString()
                    Dim wrtInfodoc As String = HttpUtility.UrlEncode(model.multi_non_pw_data(i).InfodocWRT.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
                    Dim wrtAltenate As String = model.multi_non_pw_data(i).Title.ToString()
                    html.Append($"<div class='open_tooltip_info{NoInfoDocDataCls}'><hr> <div class='body_content'> <div class='split_content scale_infodoc multirating_infodoc'>")
                    If (model.multi_non_pw_data(i).Infodoc <> "") Then
                        html.Append($"<div class='normal_content'><div class='removeptagd removeptagd{i}'><p>{model.multi_non_pw_data(i).Infodoc.ToString()} </p>")
                    Else
                        html.Append($"<div class='normal_content NoInfoData'><div class='removeptagd removeptagd{i}'><h2 class='nodata_level'>No Data</h2>")
                    End If
                    html.Append($"</div></div>")
                    If (model.multi_non_pw_data(i).InfodocWRT <> "") Then
                        html.Append($"<div class='wrt_right_content'> <h3>WITH RESPECT TO <span>{model.parent_node}</span></h3><div class='removeptagw removeptagw{i}'><p>{model.multi_non_pw_data(i).InfodocWRT} </p>")
                    Else
                        html.Append($"<div class='wrt_right_content NoInfoData'> <h3>WITH RESPECT TO <span>{model.parent_node}</span></h3><div class='removeptagw removeptagw{i}'><h2 class='nodata_level'>No Data</h2>")
                    End If
                    html.Append($"</div></div>
                                <a class='fullscreen_link' onclick=SetExpendedPopupElement(decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.multi_non_pw_data(i).Infodoc.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}),'{i}','2','0','1','{guid}','left-node',decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.multi_non_pw_data(i).Title.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}),null,null,decodeURIComponent({ChrW(&H22) + wrtInfodoc + ChrW(&H22)}),'{i}','3','0','1','{guid}','wrt-left-node',decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(wrtAltenate) + ChrW(&H22)}),decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.parent_node.ToString()) + ChrW(&H22)}),{i}) href='javascript:void(0);' data-bs-toggle='modal' data-bs-target='#exampleModal'><img src='../../img/icon/full-screen-svgrepo-com.svg'></a>
                            </div>                           
                        </div>                
                    </div>")
                    html.Append("</div>")


                Next
            End If
            html.Append("</div>")

            html.Append("</div>")
            Dim item = If(dataId.Trim() = "", model.multi_non_pw_data.FirstOrDefault(), model.multi_non_pw_data(Convert.ToInt32(dataId)))
            ''New UI Right Side Contaner
            html.Append("<div class='col-lg-6 d-none d-lg-block' id='divRadioButtons'><div class='' id='MultingRatingHeader'>")
            'html.Append($"<div class='divtable_title'><p>{item.Title}</p></div>")

            html.Append($"<div class='divtable_title' title='{item.Title}'>")
            html.Append($"<p title='{item.Title}'>{item.Title}</p>")
            html.Append("</div>")
            html.Append("<div class='divtable_header'>")
            html.Append("<div class='product_name'><p>INTENSITY SCALE </p></div>")
            If model.showPriorityAndDirect Then
                html.Append("<div class='product_intensity'><p>PRIORITY</p></div>")
            End If
            html.Append("</div>")
            html.Append("<div class='form-check radio_progress side_comment'>")

            If item Is Nothing Then
                item = model.multi_non_pw_data(If(dataId <> "", Convert.ToInt32(dataId), 0))
            End If
            dataId = If(dataId = "", "0", dataId)
            If item.Ratings IsNot Nothing Then
                For j = 0 To item.Ratings.Count - 1
                    If model.showPriorityAndDirect Or item.Ratings(j).Name <> "Direct Value" Then
                        html.Append("<div class='form-check radio_progress customvalue'>")
                        html.Append($"<input{If(model.isPipeViewOnly, " style='pointer-events:none;'", "")} class='form-check-input intensity{j}' onchange=SetValuesFromRadio('{If(dataId = "", "0", dataId)}','{j}') onclick=SetValuesFromRadio('{If(dataId = "", "0", dataId)}','{j}') type='radio' name='{item.sGUID}' id='{item.Ratings(j).GuidID}' value='{item.Ratings(j).Value}' checked>")
                        html.Append($"<label{If(model.isPipeViewOnly, " style='pointer-events:none;'", "")} class='form-check-label' >")
                        If (item.Ratings(j).Comment = "") Then
                            html.Append($"<div class='radio_btnname' onclick=SetValuesFromRadio('{If(dataId = "", "0", dataId)}','{j}')>{item.Ratings(j).Name}<span class='idesc intensitydesc{j}'></span><a href='' class='ms-2 prm-color' onclick='showNode(6{j});return false'>
                      <img class='chkEvaluatorone{If(model.isPM, "", " d-none")}' src='../../img/icon/edit-icon.svg'>
                    </a>")
                        Else
                            html.Append($"<div class='radio_btnname' onclick=SetValuesFromRadio('{If(dataId = "", "0", dataId)}','{j}')>{item.Ratings(j).Name}<span title='{item.Ratings(j).Comment}' class='idesc intensitydesc{j}'>{" - " + item.Ratings(j).Comment}</span><a href='' class='ms-2 prm-color' onclick='showNode(6{j});return false'>
                      <img class='chkEvaluatorone{If(model.isPM, "", " d-none")}' src='../../img/icon/edit-icon.svg'>
                    </a>")
                        End If
                        ''comment Right Side
                        html.Append("<div class='info_tooltip_main comment_custom'>")
                        html.Append("<div class='comment_div'>")
                        'If j <> item.Ratings.Count - 1 Then
                        '    html.Append($"<i class='bx bxs-edit-alt chkEvaluator d-none' onclick='showNode(6{j})'></i>")
                        'End If
                        html.Append($"<div class='info_tooltip right_tooltip hideshowinfo' id='parentnode6{j}'  style='display: none;'>")
                        html.Append("<div class='d-flex justify-content-between'>")
                        html.Append("<span>Edit intensity description</span>")
                        html.Append($"<div class='action_icons'><a href='javascript: void (0);' onclick='hideNode(6{j})'><i class='fa fa-times' aria-hidden='true'></i></a></div>")
                        html.Append("</div>")

                        html.Append($"<textarea{If(model.isPipeViewOnly, " style='pointer-events:none;'", "")} class='form-control mb-3 mt-2 w-100'id='txtleftComment_{j}' rows='2'>{item.Ratings(j).Comment}</textarea>")
                        html.Append("<div class='comt_btn text-end'>")
                        html.Append($"<button{If(model.isPipeViewOnly, " class='d-none'", "")} type='button' onclick=updateRightSideComment('{j}','showcomment','close',event)><i class='fa fa-check' aria-hidden='true'></i> OK</button>")
                        html.Append("</div>")

                        html.Append("</div>")
                        html.Append("</div>")
                        html.Append("</div>")
                        html.Append("</div>")
                        'html.Append("</label>")
                        'html.Append("<div class='tooltips_group2 w-auto'>")
                        'If (j = model.multi_non_pw_data.Count - 1) Then
                        '    html.Append("<div class='info_tooltip_main position-relative scndLastColumn'>")
                        'Else
                        '    html.Append("<div class='info_tooltip_main position-relative'>")
                        'End If

                        'html.Append("<div class='comment_div'>")
                        'If j <> item.Ratings.Count - 1 Then
                        '    html.Append($"<i class='bx bxs-edit-alt chkEvaluator d-none' onclick='showNode(6{j})'></i>")
                        'End If
                        'html.Append($"<div class='tooltip_wrapper hideshowinfo parentnodemr' id='parentnode6{j}'  style='display: none;'>")
                        'html.Append("<div class='d-flex justify-content-between tooltip-header'>")
                        'html.Append("<span>Edit intensity description</span>")
                        'html.Append("</div>")

                        'html.Append($"<textarea{If(model.isPipeViewOnly, " style='pointer-events:none;'", "")} class='form-control mb-3 mt-2 w-100'id='txtleftComment_{j}' rows='3'>{item.Ratings(j).Comment}</textarea>")
                        'html.Append("<div class='comt_btn text-end'>")
                        'html.Append($"<button{If(model.isPipeViewOnly, " class='d-none'", "")} type='button' onclick=updateRightSideComment('{j}','showcomment','close',event)><i class='fa fa-check' aria-hidden='true'></i> OK</button>")
                        'html.Append("</div>")

                        'html.Append("</div>")
                        'html.Append("</div>")
                        'html.Append("</div>")
                        'html.Append("</div>")
                        If model.showPriorityAndDirect Then
                            Dim ida As Double
                            If j <> item.Ratings.Count - 1 Then
                                ida = Math.Round(item.Ratings(j).Value * 100, 2)
                                html.Append("<div class='radio_btn_progress'>")
                                html.Append("<div class='progress'>")
                                html.Append($"<div class='progress-bar' onclick=SetValuesFromRadio('{If(dataId = "", "0", dataId)}','{j}') role='progressbar' style='width: {item.Ratings(j).Value * 100}%' aria-valuenow='{item.Ratings(j).Value * 100}' aria-valuemin='0' aria-valuemax='100'></div>")
                                html.Append("</div>")
                                Dim active_multi_index = model.multi_non_pw_data.Where(Function(x) x.RatingID = -1).Select(Function(y) y.ID).FirstOrDefault().ToString()
                                If j <> item.Ratings.Count - 1 Then
                                    If item.Ratings(j).Name.ToLower() = "direct value" Or item.Ratings(j).Name.ToLower() = "not rated" Then
                                        html.Append($"")
                                    Else
                                        html.Append($"{Math.Round(item.Ratings(j).Value * 100, 2)}%")
                                    End If
                                ElseIf j <> item.Ratings.Count - 2 Then
                                    html.Append($"")
                                End If
                            Else
                                ''  html += "<input type='text' class='custom-value TabNextMultRate' id='input_2_" + index + "' onkeyup=setValuesFromText('" + index + "','2')>";
                                html.Append($"<div class='directvalue_btn_progress'{If(model.isPipeViewOnly, " style='pointer-events:none;'", "")}  onclick='CloseList()'>")
                                html.Append($"<input type='text' onkeypress='return isNumberKeyWithDecimalMR(this, event);' class='custom-value' id='input_2_{dataId}' onkeyup=setValuesFromText('{dataId}','2')>")
                                html.Append($"<img src='../../img/icon/erasar.svg' onclick=ResetValue('{item.sGUID}','{dataId}',event style='cursor: pointer;' class='imgClose' )></img>")
                            End If
                            html.Append("</div>")
                        End If
                        html.Append("</label>")
                        html.Append("</div>")
                    End If
                Next
            End If


            html.Append("</div>")
            html.Append("</div>")
            html.Append("</div>")
            html.Append("</div>")
            html.Append("</div>")
            html.Append("</div>")




            divContent.InnerHtml = html.ToString()
        End If
    End Sub

End Class