Imports System.IO
Imports System.Runtime.InteropServices
Imports Canvas
Imports ECCore
Imports ExpertChoice.Data
Imports ExpertChoice.Service

Public Class AllPairWiseUserCtrl
    Inherits System.Web.UI.UserControl

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    End Sub
    Public Sub BindHtml(ByVal output As AnytimeOutputModel)
        Dim IsMobile = False
        Dim is_AT_owner = True
        Dim active_multi_index = 0
        Dim main_gradient_checkbox=False
        Dim text_limit = 0
        Dim bars_left As String() = New String(8) {}
        Dim str = New String() {"9", "Extreme", "nine", "EX"}
        bars_left(0) = str.ToString()
        str = New String() {"8", "Very Strong to Extreme", "eight", " "}
        bars_left(1) = str.ToString()
        str = New String() {"7", "Very Strong", "seven", "VS"}
        bars_left(2) = str.ToString()
        str = New String() {"6", "Strong to Very Strong", "six", " "}
        bars_left(3) = str.ToString()
        str = New String() {"5", "Strong", "five", "S"}
        bars_left(4) = str.ToString()
        str = New String() {"4", "Moderate to Strong", "four", " "}
        bars_left(5) = str.ToString()
        str = New String() {"3", "Moderate", "three", "M"}
        bars_left(6) = str.ToString()
        str = New String() {"2", "Equal to Moderate", "two", " "}
        bars_left(7) = str.ToString()
        Dim bars_right As String() = New String(8) {}
        str = New String() {"2", "Equal to Moderate", "two", " "}
        bars_left(0) = str.ToString()
        str = New String() {"3", "Moderate", "three", "M"}
        bars_left(1) = str.ToString()
        str = New String() {"4", "Moderate to Strong", "four", " "}
        bars_left(2) = str.ToString()
        str = New String() {"5", "Strong", "five", "S"}
        bars_left(3) = str.ToString()
        str = New String() {"6", "Strong to Very Strong", "six", " "}
        bars_left(4) = str.ToString()
        str = New String() {"7", "Very Strong", "seven", "VS"}
        bars_left(5) = str.ToString()
        str = New String() {"8", "Very Strong to Extreme", "eight", " "}
        bars_left(6) = str.ToString()
        str = New String() {"9", "Extreme", "nine", "EX"}
        bars_left(7) = str.ToString()

        QuestionHeader.BindHtml(output)
        FramedInfodDocs.BindHtml(output)
        Dim html As StringBuilder = New StringBuilder()
        html.Append(question.Visible)
        html.Append("<div class='tt-judgements-item large-12 columns' ><div class='columns tt-j-content'><div class='row collapse tt-j-result ds-div-trigger'>  ")
        html.Append("<div class='columns'><div class='row collapse'>")
        If output.pairwise_type = "ptVerbal" And IsMobile Then
            html.Append("<div class='original-legend text-center' style='font-size: .750rem;'><b>EQ</b>ual<b>&nbsp;&nbsp;&nbsp;M</b>oderate  &nbsp;&nbsp;&nbsp;<b>S</b>trong &nbsp;&nbsp;&nbsp;<b>V</b>ery<b>S</b>trong  &nbsp;&nbsp;&nbsp;<b>EX</b>treme</div>")
        End If
        html.Append(" <div class='tt-j-others-result columns'><div class='tt-multi-pairwise-wrap multi-verbal large-10 large-centered columns multi-graphical " + If(If(output.multi_pw_data Is Nothing, 0, output.multi_pw_data.Count <= 7), "fix-multi-pw", "") + "'>")
        html.Append("<div class='multi-loop-wrap hide'>")
        For index = 1 To If(output.multi_pw_data Is Nothing, 0, output.multi_pw_data.Count)
            html.Append("<div class='multi-loop-wrap hide'><div id='multi-row-" + index + "' onclick='set_multi_index(" + index + ")' class='fade-fifty' : active_multi_index!=" + index + ",'top-row' :active_multi_index==0, 'selected' : active_multi_index==" + index + ", 'multi-verbal large-12 columns' : active_multi_index != " + index + ",  'columns tt-j-content multi-loop' :  active_multi_index == " + index + ", 'tt-verbal-bars-wrap' : " + output.pairwise_type = "ptVerbal" + " multi-rows multi-row-" + index + "'>")
            If IsMobile = False Then
                html.Append("<div class='small-6 columns text-left show-for-medium-down multi-mobile left-question'>")
                html.Append("checkGPtoVerbal( output.multi_pw_data )")
                html.Append("<div class='small-2 columns text-center'>")
                If (is_AT_owner Or (Not is_AT_owner And Not String.IsNullOrWhiteSpace(output.multi_pw_data(index).InfodocLeft))) And output.showinfodocnode And Not output.framed_info_docs Then
                    html.Append("<a data-dropdown='gdrop" + index + "_1' aria-controls='gdrop" + index + "_1' aria-expanded='false' data-options='align:top;'")
                    html.Append(" class='left-node-" + output.multi_pw_data(index).NodeID_Left + "-" + index + "-tooltip'>")
                    html.Append("<span class='" + If(String.IsNullOrWhiteSpace(output.multi_pw_data(index).InfodocLeft), "disabled", "not-disabled") + " icon icon-tt-info-circle'></span></a>")
                End If
                If (Not is_AT_owner And String.IsNullOrWhiteSpace(output.multi_pw_data(index).InfodocLeft)) Or Not output.showinfodocnode Then
                    html.Append("<a> <span>&nbsp;</span> </a>")
                End If
                html.Append("</div>")
                html.Append(" <div class='small-8 text-center  columns'><span class='mobile multi-left-node limited-lft-" + index + " 'hide': active_multi_index = " + index + "' >")
                If output.multi_pw_data(index).LeftNode.Length > text_limit Then
                    html.Append("<a class='show-for-small-only' href='#'> " + output.multi_pw_data(index).LeftNode.Substring(0, text_limit - 2) + "...</a>")
                End If
                If output.multi_pw_data(index).LeftNode.Length <= text_limit Then
                    html.Append(" <a class='show-for-small-only' id='l-" + index + "' href='#'>" + output.multi_pw_data(index).LeftNode + "</a>")
                End If
                html.Append("<a class='show-for-medium-up' id='l-" + index + "'  href='#'>" + output.multi_pw_data(index).LeftNode + "</a> </span>")
                html.Append("<span class='multi-left-node full-lft-" + index + " 'hide': active_multi_index != " + index + "'><a id='l-" + index + "'>" + output.multi_pw_data(index).LeftNode + " </a></span> </div>")
                html.Append("<div class='small-2 columns infodoc-icon-right text-center' >")

                If (is_AT_owner Or (Not is_AT_owner And Not String.IsNullOrWhiteSpace(output.multi_pw_data(index).InfodocLeftWRT))) And output.showinfodocnode Then
                    html.Append("<a data-dropdown='gdrop" + index + "_2' aria-controls='gdrop" + index + "_2' aria-expanded='false' class='wrt-left-node-" + output.multi_pw_data(index).NodeID_Left + "-+" + index + "-tooltip'>  <span class='icon icon-tt-w-font" + If(String.IsNullOrWhiteSpace(output.multi_pw_data(index).InfodocLeftWRT), "disabled", "not-disabled") + "'></span></a>  ")
                End If
                If (Not is_AT_owner And String.IsNullOrWhiteSpace(output.multi_pw_data(index).InfodocLeftWRT)) Then
                    html.Append("<a><span>&nbsp;</span></a>")
                End If
                html.Append("</div>")
                html.Append("<div class='small-6 columns text-right show-for-medium-down multi-mobile right-question'> <div class='small-2 columns text-center'>")
                If (is_AT_owner Or (Not is_AT_owner And Not String.IsNullOrWhiteSpace(output.multi_pw_data(index).InfodocRight))) And output.showinfodocnode And Not output.framed_info_docs Then
                    html.Append("<a data-dropdown='gdrop" + index + "_3' aria-controls='gdrop" + index + "_3' aria-expanded='false' data-options='align:top;'")
                    html.Append(" class='right-node-" + output.multi_pw_data(index).NodeID_Right + "-" + index + "-tooltip'>")
                    html.Append("<span class='" + If(String.IsNullOrWhiteSpace(output.multi_pw_data(index).InfodocRight), "disabled", "not-disabled") + " icon icon-tt-info-circle'></span></a>")
                End If
                If (Not is_AT_owner And String.IsNullOrWhiteSpace(output.multi_pw_data(index).InfodocRight)) Or Not output.showinfodocnode Then
                    html.Append("<a> <span>&nbsp;</span> </a>")
                End If
                html.Append("</div>")
                html.Append(" <div class='small-8 text-center  columns'><span class='mobile multi-right-node limited-lft-" + index + " 'hide': active_multi_index = " + index + "' >")
                If output.multi_pw_data(index).RightNode.Length > text_limit Then
                    html.Append("<a class='show-for-small-only' href='#'> " + output.multi_pw_data(index).RightNode.Substring(0, text_limit - 2) + "...</a>")
                End If
                If output.multi_pw_data(index).RightNode.Length <= text_limit Then
                    html.Append(" <a class='show-for-small-only' id='l-" + index + "' href='#'>" + output.multi_pw_data(index).RightNode + "</a>")
                End If
                html.Append("<a class='show-for-medium-up' id='l-" + index + "'  href='#'>" + output.multi_pw_data(index).RightNode + "</a> </span>")
                html.Append("<span class='multi-right-node full-rgt-" + index + " 'hide': active_multi_index != " + index + "'><a id='l-" + index + "'>" + output.multi_pw_data(index).RightNode + " </a></span> </div>")
                html.Append(" <div class='small-2 left text-center columns' >")
                If (is_AT_owner Or (Not is_AT_owner And Not String.IsNullOrWhiteSpace(output.multi_pw_data(index).InfodocRightWRT))) And output.showinfodocnode Then
                    html.Append("<a data-dropdown='gdrop" + index + "_4' aria-controls='gdrop" + index + "_4' aria-expanded='false' class='wrt-left-node-" + output.multi_pw_data(index).NodeID_Right + "-+" + index + "-tooltip'>  <span class='icon icon-tt-w-font" + If(String.IsNullOrWhiteSpace(output.multi_pw_data(index).InfodocRightWRT), "disabled", "not-disabled") + "'></span></a>  ")
                End If
                If (Not is_AT_owner And String.IsNullOrWhiteSpace(output.multi_pw_data(index).InfodocRightWRT)) Then
                    html.Append("<a><span>&nbsp;</span></a>")
                End If
                html.Append(" </div></div>")
            End If
            If active_multi_index = index And output.pairwise_type = "ptVerbal" And IsMobile Then
                html.Append("<div class='small-12 medium-9 medium-centered columns show-for-medium-down text-center multi-pw-verbal-labels-wrap anytime'>")
                If active_multi_index = index Or (active_multi_index = 0 And index = 0) And Not output.collapse_bars Then
                    html.Append("<div class='columns selected-item text-center selected-item-multi-pw show-for-medium-down' style=''> display_selected_bar(" + output.multi_pw_data(index).Value + "," + output.multi_pw_data(index).Advantage + " </div>")
                End If
                html.Append("<div class='tt-mobile-wrap columns'> <div class='tt-j-content'>")

                If active_multi_index = index And Not output.collapse_bars Then
                    html.Append("<div class='tt-equalizer-mobile multi-vb-bars-wrap'>  <ul id='pm-bars' class='vb-bars-small-medium-screen'><li  ")
                    For index_ = 1 To bars_left.Length
                        html.Append("<li data-index='" + bars_left(index_) + "'  data-bar-index='" + index_ + "' onclick='add_multivalues(" + bars_left(0) + " , 1 , pair_index, $event)' id='" + index_ + 1 + "' title='" + bars_left(1) + "' class='" + bars_left(2) + "" + If(main_gradient_checkbox, "", "no-gradient") + " pm-bars lft lft-eq lft-" + index_ + 1 + "" + If(main_gradient_checkbox, "", "no-gradient") + "  tg-gradients ")
                        html.Append(If(output.advantage = 1, If(output.value >= bars_left(0), "active-selected", ""), "") + "' data-pos='lft' ")
                        Dim strval = (output.advantage = 1 And bars_left(0) > output.value And bars_left(0) - output.value < 1 And (output.value Mod 1 > 0))
                        html.Append("style= 'background' : (" + If(strval.ToString(), "linear-gradient(270deg, #0058a3 '+ (" + bars_left(0) - output.value + "+) '%, #cccccc 50%)'", "") + ">")
                        If bars_left(3) <> "M" Then
                            html.Append(" <span class='bar-label'>" + bars_left(3) + "</span>")
                        End If
                        If bars_left(3) = "M" Then
                            html.Append("<span class='bar-label' style='top: -7px; position:relative'>" + bars_left(3) + "</span>")
                        End If
                        html.Append(" </li>")
                    Next
                    html.Append("<li  data-bar-index='0' onclick='add_multivalues(1, 0 ,pair_index, $event)' data-index='" + index + "' id='9' title='Equal' data-pos='mid'")
                    html.Append("class='pm-bars zero mid lft-9 lft-eq rgt-eq rgt-9 mid-9 " + If(main_gradient_checkbox, "", "no-gradient") + " " + If(output.advantage = 1, "lft-selected", "") + " " + If(output.advantage = -1, "rgt-selected", "") + " " + If(output.advantage = 0 And output.value = 1, "active-selected", "") + "  tg-gradients'>")
                    html.Append("<span class='bar-label' style='display: block; height: 100%; font-size: 7px; margin-top: -1px;'>EQ</span> </li>")
                    For index_ = 1 To bars_right.Length
                        html.Append("<li data-index='" + index_ + "'  data-bar-index='" + index_ + "' onclick='add_multivalues(" + bars_right(0) + ", -1 , pair_index, $event))' id='" + bars_right.Length - index_ + "' title='" + bars_right(1) + "' class='" + bars_right(2) + "" + If(main_gradient_checkbox, "", "no-gradient") + " pm-bars lft lft-eq lft-" + bars_right.Length - index_ + "" + If(main_gradient_checkbox, "", "no-gradient") + "  tg-gradients ")
                        html.Append(If(output.advantage = 1, If(output.value >= bars_right(0), "active-selected", ""), "") + "' data-pos='lft' ")
                        Dim strval = (output.advantage = 1 And bars_right(0) > output.value And bars_right(0) - output.value < 1 And (output.value Mod 1 > 0))
                        html.Append("style= 'background' : (" + If(strval.ToString(), "linear-gradient(270deg, #0058a3 '+ (" + bars_right(0) - output.value + "+) '%, #cccccc 50%)'", "") + ">")
                        If bars_right(3) <> "M" Then
                            html.Append(" <span class='bar-label'>" + bars_right(3) + "</span>")
                        End If
                        If bars_right(3) = "M" Then
                            html.Append("<span class='bar-label' style='top: -7px; position:relative'>" + bars_right(3) + "</span>")
                        End If
                        html.Append(" </li>")
                    Next
                    html.Append(" </ul></div>")
                End If
                html.Append("</div></div></div>")
            End If
            html.Append("</div>")

            If Not IsMobile Then
                html.Append("<div class='medium-3 columns text-left hide-for-medium-down pw-verbal-desktop left-alternative " + If(active_multi_index = index Or (active_multi_index = 0 And index = 0), "top-row-left", "") + "'")

            End If

        Next

        dvContent.InnerHtml = html.ToString()
    End Sub
    Public Shared Function GetAllPairWiseData(ByRef AnytimeAction As clsAction, ByRef App As clsComparionCore, ByRef StepNode As clsNode, <Out> ByRef multi_GUIDs As List(Of String()),
                                              <Out> ByRef PairwiseType As String, <Out> ByRef MultiPW_Data As List(Of clsPairwiseLine), <Out> ByRef ParentNodeName As String, <Out> ByRef ParentNodeGUID As Guid,
                                              <Out> ByRef parent_node_info As String, <Out> ByRef pipeHelpUrl As String, <Out> ByRef ParentNodeID As Integer, <Out> ByRef infodoc_params As String())
        Dim IsUndefined As Boolean = False
        Dim fIsPWOutcomes As Boolean = AnytimeAction.ActionType = ActionType.atAllPairwiseOutcomes
        Dim UndefIDx As Integer = -1
        If TypeOf AnytimeAction.ActionData Is clsAllPairwiseEvaluationActionData Then
            Dim AllPwData As clsAllPairwiseEvaluationActionData = CType(AnytimeAction.ActionData, clsAllPairwiseEvaluationActionData)
            Dim fAlts As Boolean = AllPwData.ParentNode.IsTerminalNode
            Dim StepTask As String = ""
            Try
                StepTask = TeamTimeClass.GetPipeStepTask(AnytimeAction, Nothing, AnytimeClass.IsImpact AndAlso Not fAlts)
            Catch
                StepTask = ""
            End Try
            Dim PWType = App.ActiveProject.ProjectManager.PipeBuilder.GetPairwiseTypeForNode(AllPwData.ParentNode)
            Dim qh_help_id = PWType = If(CanvasTypes.PairwiseType.ptVerbal, Canvas.PipeParameters.ecEvaluationStepType.VerbalPW, Canvas.PipeParameters.ecEvaluationStepType.GraphicalPW)
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
            pipeHelpUrl = TeamTimeClass.ResString(PWType = 1) 'If(CanvasTypes.PairwiseType.ptVerbal, "help_pipe_multiPairwiseVerbal", "help_pipe_multiPairwiseGraphical"))
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