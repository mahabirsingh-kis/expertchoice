Imports System.Runtime.InteropServices

Public Class NonPairWiseAllCovObjsUserCtrl
    Inherits System.Web.UI.UserControl

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    End Sub

    Public Shared Function GetNonPairWiseAllCovObjData(ByRef AnytimeAction As clsAction, ByRef App As clsComparionCore, ByRef StepNode As clsNode, <Out> ByRef IsUndefined As Boolean) As Object

        Dim pipeHelpUrl As String = ""
        Dim qh_help_id = New PipeParameters.ecEvaluationStepType()
        Dim NonPWType = ""
        Dim showPriorityAndDirectValue As Boolean = True
        Dim precision As Integer = 0
        Dim first_node_info, parent_node_info, wrt_first_node_info As String
        first_node_info = ""
        parent_node_info = ""
        wrt_first_node_info = ""
        'Dim IsUndefined = False
        Dim scaleDescriptions As List(Of Object) = New List(Of Object)()
        Dim StepTask = ""
        Dim MultiNonPW_Data = New List(Of clsRatingLine)()
        Dim ParentNodeName, ChildNodeName As String
        ParentNodeName = ""
        ChildNodeName = ""
        Dim ParentNodeID As Integer = -1
        Dim ParentNodeGUID As Guid = New Guid()
        'Dim infodoc_params As String() = New String(4) {}
        Dim now_pw_all As clsNonPairwiseEvaluationActionData = CType(AnytimeAction.ActionData, clsNonPairwiseEvaluationActionData)

        Select Case now_pw_all.MeasurementType
            Case ECMeasureType.mtRatings
                pipeHelpUrl = TeamTimeClass.ResString("help_pipe_rating")
                qh_help_id = Canvas.PipeParameters.ecEvaluationStepType.MultiRatings
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
                        first_node_info = CStr(InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, If(tAlt.IsAlternative, Consts.reObjectType.Alternative, Consts.reObjectType.Node), tAlt.NodeID.ToString(), tAlt.InfoDoc, True, True, -1))
                        wrt_first_node_info = CStr(InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.AltWRTNode, tAlt.NodeID.ToString(), App.ActiveProject.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(tAlt.NodeGuidID, MultiNonPWData.ParentNode.NodeGuidID), True, True, MultiNonPWData.ParentNode.NodeID))
                        Dim DV As Single = -1
                        DV = If(R.Rating IsNot Nothing AndAlso R.Rating.RatingScaleID < 0 AndAlso R.Rating.ID < 0, R.Rating.Value, DV)
                        Lst.Add(New clsRatingLine(tAlt.NodeID, R.RatingScale.GuidID.ToString(), StringFuncs.JS_SafeHTML(tAlt.NodeName), Ratings, RID, first_node_info, R.Comment, DV, "", "", wrt_first_node_info, "", "", ""))
                        ID += 1

                        If R.IsUndefined Then
                            IsUndefined = True
                        End If

                        scaleDescriptions.Add(New With {
                            .Name = R.RatingScale.Name,
                            .Guid = R.RatingScale.GuidID.ToString(),
                            .Description = InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.MeasureScale, R.RatingScale.GuidID.ToString(), R.RatingScale.Comment, True, True, -1)
                        })
                    Next

                    StepTask = ""

                    Try
                        StepTask = TeamTimeClass.GetPipeStepTask(AnytimeAction, Nothing, AnytimeClass.IsImpact AndAlso Not MultiNonPWData.ParentNode.IsTerminalNode)
                    Catch
                        StepTask = ""
                    End Try

                    MultiNonPW_Data = Lst
                    ParentNodeName = MultiNonPWData.ParentNode.NodeName
                    parent_node_info = CStr(InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, If(MultiNonPWData.ParentNode.IsAlternative, Consts.reObjectType.Alternative, Consts.reObjectType.Node), MultiNonPWData.ParentNode.NodeID.ToString(), MultiNonPWData.ParentNode.InfoDoc, True, True, -1))
                    ParentNodeID = MultiNonPWData.ParentNode.NodeID
                    ParentNodeGUID = MultiNonPWData.ParentNode.NodeGuidID
                    SetInfoDoc_Params(MultiNonPWData.ParentNode.NodeGuidID)
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
                                    scaleDescriptions.Add(New With {
                                        .Name = RS.Name,
                                        .Guid = RS.GuidID.ToString(),
                                        .Description = InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.MeasureScale, RS.GuidID.ToString(), RS.Comment, True, True, -1)
                                    })
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
                            first_node_info = CStr(InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, If(tCovObj.IsAlternative, Consts.reObjectType.Alternative, Consts.reObjectType.Node), tCovObj.NodeID.ToString(), tCovObj.InfoDoc, True, True, -1))
                            wrt_first_node_info = CStr(InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.AltWRTNode, MultiNonPWData.Alternative.NodeID.ToString(), App.ActiveProject.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(MultiNonPWData.Alternative.NodeGuidID, tCovObj.NodeGuidID), True, True, tCovObj.NodeID))
                            Dim DV As Single = -1
                            DV = If(R.Rating IsNot Nothing AndAlso R.Rating.RatingScaleID < 0 AndAlso R.Rating.ID < 0, R.Rating.Value, DV)
                            Lst.Add(New clsRatingLine(tCovObj.NodeID, R.RatingScale.GuidID.ToString(), StringFuncs.JS_SafeHTML(tCovObj.NodeName), Ratings, RID, first_node_info, R.Comment, DV, "", "", wrt_first_node_info, "", "", ""))
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
                    parent_node_info = CStr(InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, If(MultiNonPWData.Alternative.IsAlternative, Consts.reObjectType.Alternative, Consts.reObjectType.Node), MultiNonPWData.Alternative.NodeID.ToString(), MultiNonPWData.Alternative.InfoDoc, True, True, -1))
                    ParentNodeID = MultiNonPWData.Alternative.NodeID
                    ParentNodeGUID = MultiNonPWData.Alternative.NodeGuidID
                    SetInfoDoc_Params(MultiNonPWData.Alternative.NodeGuidID)
                End If

            Case ECMeasureType.mtDirect
                pipeHelpUrl = TeamTimeClass.ResString("help_pipe_directEntry")
                qh_help_id = Canvas.PipeParameters.ecEvaluationStepType.MultiDirectInput
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
                    parent_node_info = CStr(InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, If(MultiDirectData1.ParentNode.IsAlternative, Consts.reObjectType.Alternative, Consts.reObjectType.Node), MultiDirectData1.ParentNode.NodeID.ToString(), MultiDirectData1.ParentNode.InfoDoc, True, True, -1))
                    Dim Lst As List(Of clsRatingLine) = New List(Of clsRatingLine)()
                    Dim ID = 0

                    For Each tAlt As clsNode In MultiDirectData1.Children
                        Dim DD As clsDirectMeasureData = CType(MultiDirectData1.GetJudgment(tAlt), clsDirectMeasureData)
                        first_node_info = CStr(InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, If(tAlt.IsAlternative, Consts.reObjectType.Alternative, Consts.reObjectType.Node), tAlt.NodeID.ToString(), tAlt.InfoDoc, True, True, -1))
                        wrt_first_node_info = CStr(InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.AltWRTNode, tAlt.NodeID.ToString(), App.ActiveProject.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(tAlt.NodeGuidID, MultiDirectData1.ParentNode.NodeGuidID), True, True, MultiDirectData1.ParentNode.NodeID))
                        Dim DV As Single = -1
                        DV = If(DD.IsUndefined, DV, DD.DirectData)
                        Lst.Add(New clsRatingLine(tAlt.NodeID, tAlt.NodeGuidID.ToString(), StringFuncs.JS_SafeHTML(tAlt.NodeName), Nothing, -1, first_node_info, DD.Comment, DV, "", "", wrt_first_node_info, "", "", ""))
                        ID += 1

                        If DD.IsUndefined Then
                            IsUndefined = True
                        End If
                    Next

                    MultiNonPW_Data = Lst
                    ParentNodeID = MultiDirectData1.ParentNode.NodeID
                    ParentNodeGUID = MultiDirectData1.ParentNode.NodeGuidID
                    SetInfoDoc_Params(MultiDirectData1.ParentNode.NodeGuidID)
                End If

                If TypeOf AnytimeAction.ActionData Is clsAllCoveringObjectivesEvaluationActionData Then
                    Dim MultiNonPWData As clsAllCoveringObjectivesEvaluationActionData = CType(AnytimeAction.ActionData, clsAllCoveringObjectivesEvaluationActionData)
                    Dim Lst As List(Of clsRatingLine) = New List(Of clsRatingLine)()
                    Dim ID As Integer = 0

                    For Each tNode As clsNode In MultiNonPWData.CoveringObjectives
                        Dim DD As clsDirectMeasureData = CType(MultiNonPWData.GetJudgment(tNode), clsDirectMeasureData)
                        first_node_info = CStr(InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, If(tNode.IsAlternative, Consts.reObjectType.Alternative, Consts.reObjectType.Node), tNode.NodeID.ToString(), tNode.InfoDoc, True, True, -1))
                        wrt_first_node_info = CStr(InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.AltWRTNode, MultiNonPWData.Alternative.NodeID.ToString(), App.ActiveProject.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(MultiNonPWData.Alternative.NodeGuidID, tNode.NodeGuidID), True, True, tNode.NodeID))
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
                    parent_node_info = CStr(InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, If(MultiNonPWData.Alternative.IsAlternative, Consts.reObjectType.Alternative, Consts.reObjectType.Node), MultiNonPWData.Alternative.NodeID.ToString(), MultiNonPWData.Alternative.InfoDoc, True, True, -1))
                    ParentNodeID = MultiNonPWData.Alternative.NodeID
                    ParentNodeGUID = MultiNonPWData.Alternative.NodeGuidID
                    SetInfoDoc_Params(MultiNonPWData.Alternative.NodeGuidID)
                End If
        End Select

    End Function

    Public Shared Function SetInfoDoc_Params(ByVal NodeID As Guid) As Object
        Dim infodoc_params As String() = New String(4) {}
        infodoc_params(0) = GeckoClass.GetInfodocParams(NodeID, Guid.Empty, True)
        infodoc_params(1) = GeckoClass.GetInfodocParams(NodeID, Guid.Empty, True)
        infodoc_params(2) = GeckoClass.GetInfodocParams(NodeID, Guid.Empty, True)
    End Function

End Class