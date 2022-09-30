Imports Canvas
Imports ECCore
Imports ExpertChoice.Data
Imports ExpertChoice.Service

Public Class NonPairWiseOneAtATimeUserCtrl
    Inherits System.Web.UI.UserControl

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    End Sub

    Public Shared Function GetNonPairWiseOneAtATimeData(ByRef AnytimeAction As clsAction, ByRef App As clsComparionCore, ByRef StepNode As clsNode)

        Dim single_non_pw = CType(AnytimeAction.ActionData, clsOneAtATimeEvaluationActionData)
        Dim ObjHierarchy = CType(App.ActiveProject.ProjectManager.Hierarchy(App.ActiveProject.ProjectManager.ActiveHierarchy), clsHierarchy)
        Dim AltsHierarchy = CType(App.ActiveProject.ProjectManager.AltsHierarchy(App.ActiveProject.ProjectManager.ActiveAltsHierarchy), clsHierarchy)
        Dim pipeHelpUrl As String = ""
        Dim qh_help_id = New PipeParameters.ecEvaluationStepType()
        Dim NonPWType = ""
        Dim NonPWValue = ""
        Dim is_direct = False
        Dim Comment As String = ""
        Dim StepTask = ""
        Dim ParentNodeName, ChildNodeName As String
        ParentNodeName = ""
        ChildNodeName = ""
        Dim ParentNodeGUID As Guid = New Guid()
        Dim LeftNodeGUID As Guid = New Guid()
        Dim showPriorityAndDirectValue As Boolean = True
        Dim precision As Integer = 0
        Dim scaleDescriptions As List(Of Object) = New List(Of Object)()
        Dim intensities = New List(Of String())()
        Dim first_node_info, parent_node_info, wrt_first_node_info As String
        first_node_info = ""
        parent_node_info = ""
        wrt_first_node_info = ""
        Dim IsUndefined = False
        Dim ParentNodeID As Integer = -1
        Dim step_intervals = New List(Of String())()
        Dim CurrentValue As Double = -1
        Dim piecewise As Boolean = False
        Dim PipeParameters = New Object()
        Dim UCData As Object = Nothing

        If single_non_pw.Node IsNot Nothing AndAlso single_non_pw.Judgment IsNot Nothing Then
            Dim measuretype = CType(AnytimeAction.ActionData, clsNonPairwiseEvaluationActionData)

            Select Case (measuretype).MeasurementType
                Case ECMeasureType.mtRatings
                    pipeHelpUrl = TeamTimeClass.ResString("help_pipe_rating")
                    qh_help_id = Canvas.PipeParameters.ecEvaluationStepType.Ratings
                    NonPWType = "mtRatings"
                    Dim r_judgment As clsNonPairwiseMeasureData = CType(single_non_pw.Judgment, clsNonPairwiseMeasureData)
                    Dim tJud As clsRatingMeasureData = CType(single_non_pw.Judgment, clsRatingMeasureData)
                    NonPWValue = If(tJud.IsUndefined, "-1", tJud.Rating.Value.ToString())

                    If tJud.IsUndefined Then
                        NonPWValue = "-1"
                    Else
                        NonPWValue = tJud.Rating.Value.ToString()
                        is_direct = If(tJud.Rating.ID = -1, True, False)
                    End If

                    Comment = tJud.Comment
                    Dim r_ParentNode = single_non_pw.Node
                    Dim r_ChildNode As clsNode = Nothing

                    If single_non_pw.Node.IsAlternative Then
                        r_ParentNode = ObjHierarchy.Nodes(0)
                        r_ChildNode = single_non_pw.Node
                    Else
                        r_ParentNode = single_non_pw.Node

                        If r_ParentNode IsNot Nothing AndAlso r_ParentNode.IsTerminalNode Then
                            r_ChildNode = AltsHierarchy.GetNodeByID(r_judgment.NodeID)
                        Else
                            r_ChildNode = ObjHierarchy.GetNodeByID(r_judgment.NodeID)
                        End If
                    End If

                    StepNode = r_ParentNode
                    StepTask = ""

                    Try
                        StepTask = TeamTimeClass.GetPipeStepTask(AnytimeAction, Nothing, AnytimeClass.IsImpact AndAlso Not r_ParentNode.IsTerminalNode)
                    Catch
                        StepTask = ""
                    End Try

                    ParentNodeName = r_ParentNode.NodeName
                    ChildNodeName = r_ChildNode.NodeName
                    ParentNodeGUID = r_ParentNode.NodeGuidID
                    LeftNodeGUID = r_ChildNode.NodeGuidID
                    SetInfoDoc_Params(r_ParentNode.NodeGuidID, r_ChildNode.NodeGuidID)
                    Dim MScale As clsRatingScale = CType(single_non_pw.MeasurementScale, clsRatingScale)

                    If MScale IsNot Nothing Then
                        showPriorityAndDirectValue = App.ActiveProject.ProjectManager.Parameters.RatingsUseDirectValue(MScale.GuidID)
                        precision = AnytimeClass.GetPrecisionForRatings(CType(MScale, clsRatingScale))
                        scaleDescriptions.Add(New With {
                            .Name = MScale.Name,
                            .Guid = MScale.GuidID.ToString(),
                            .Description = InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.MeasureScale, MScale.GuidID.ToString(), MScale.Comment, True, True, -1)
                        })
                    End If

                    For Each intensity As clsRating In MScale.RatingSet
                        intensities.Add(New String() {intensity.Value.ToString(), intensity.Name.ToString(), intensity.ID.ToString(), intensity.Priority.ToString(), intensity.Comment})
                    Next

                    intensities.Add(New String() {"-1", "Not Rated", "-1", "0", ""})
                    intensities.Add(New String() {"0", "Direct Value", "-2", "0", ""})
                    first_node_info = CStr(InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, If(r_ChildNode.IsAlternative, Consts.reObjectType.Alternative, Consts.reObjectType.Node), r_ChildNode.NodeID.ToString(), r_ChildNode.InfoDoc, True, True, -1))
                    parent_node_info = CStr(InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, If(r_ParentNode.IsAlternative, Consts.reObjectType.Alternative, Consts.reObjectType.Node), r_ParentNode.NodeID.ToString(), r_ParentNode.InfoDoc, True, True, -1))
                    wrt_first_node_info = CStr(InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.AltWRTNode, r_ChildNode.NodeID.ToString(), App.ActiveProject.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(r_ChildNode.NodeGuidID, r_ParentNode.NodeGuidID), True, True, r_ParentNode.NodeID))

                    If tJud.IsUndefined Then
                        IsUndefined = True
                    End If

                    ParentNodeID = single_non_pw.Node.NodeID
                Case ECMeasureType.mtDirect
                    qh_help_id = Canvas.PipeParameters.ecEvaluationStepType.DirectInput
                    NonPWType = "mtDirect"
                    Dim d_tAlt As clsNode = Nothing
                    Dim d_tParentNode As clsNode = Nothing
                    Dim d_judgment As clsNonPairwiseMeasureData = CType(single_non_pw.Judgment, clsNonPairwiseMeasureData)
                    Dim d_measure_data As clsDirectMeasureData = CType(single_non_pw.Judgment, clsDirectMeasureData)
                    Comment = d_measure_data.Comment

                    If d_measure_data IsNot Nothing Then

                        If Not d_measure_data.IsUndefined Then

                            If Convert.ToDecimal(d_measure_data.ObjectValue.ToString()) < 0 Then
                                NonPWValue = "0"
                            ElseIf Convert.ToDecimal(d_measure_data.ObjectValue.ToString()) > 1 Then
                                NonPWValue = "1"
                            Else
                                precision = 4
                                NonPWValue = StringFuncs.JS_SafeNumber(d_measure_data.ObjectValue)
                            End If
                        End If
                    End If

                    If AnytimeAction.ActionData IsNot Nothing AndAlso single_non_pw.Judgment IsNot Nothing Then
                        Dim DirectData = CType(single_non_pw.Judgment, clsDirectMeasureData)
                        d_tParentNode = single_non_pw.Node
                        StepNode = d_tParentNode

                        If d_tParentNode.IsTerminalNode Then
                            d_tAlt = AltsHierarchy.GetNodeByID(DirectData.NodeID)
                        Else
                            d_tAlt = ObjHierarchy.GetNodeByID(DirectData.NodeID)
                        End If

                        StepTask = ""

                        Try
                            StepTask = TeamTimeClass.GetPipeStepTask(AnytimeAction, Nothing, AnytimeClass.IsImpact AndAlso Not d_tParentNode.IsTerminalNode)
                        Catch
                            StepTask = ""
                        End Try

                        ParentNodeName = d_tParentNode.NodeName
                        ChildNodeName = d_tAlt.NodeName
                        ParentNodeGUID = d_tParentNode.NodeGuidID
                        LeftNodeGUID = d_tAlt.NodeGuidID
                        SetInfoDoc_Params(d_tParentNode.NodeGuidID, d_tAlt.NodeGuidID)
                        first_node_info = CStr(InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, If(d_tAlt.IsAlternative, Consts.reObjectType.Alternative, Consts.reObjectType.Node), d_tAlt.NodeID.ToString(), d_tAlt.InfoDoc, True, True, -1))
                        parent_node_info = CStr(InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, If(d_tAlt.IsAlternative, Consts.reObjectType.Alternative, Consts.reObjectType.Node), d_tParentNode.NodeID.ToString(), d_tParentNode.InfoDoc, True, True, -1))
                        wrt_first_node_info = CStr(InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.AltWRTNode, d_tAlt.NodeID.ToString(), App.ActiveProject.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(d_tAlt.NodeGuidID, d_tParentNode.NodeGuidID), True, True, d_tParentNode.NodeID))
                    End If

                    If d_measure_data.IsUndefined Then
                        IsUndefined = True
                    End If

                    ParentNodeID = single_non_pw.Node.NodeID
                Case ECMeasureType.mtStep
                    pipeHelpUrl = TeamTimeClass.ResString("help_pipe_stepFunction")
                    qh_help_id = Canvas.PipeParameters.ecEvaluationStepType.StepFunction
                    NonPWType = single_non_pw.MeasurementType.ToString()
                    Dim StepData = CType(single_non_pw.Judgment, clsStepMeasureData)
                    StepData.StepFunction.SortByInterval()
                    Comment = StepData.Comment
                    Dim intervalx = New String(StepData.StepFunction.Intervals.Count - 1) {}
                    Dim intervaly = New String(StepData.StepFunction.Intervals.Count - 1) {}

                    For Each period As clsStepInterval In StepData.StepFunction.Intervals
                        Dim ind = StepData.StepFunction.Intervals.IndexOf(period)
                        intervalx(ind) = period.Low.ToString()
                        intervaly(ind) = (period.Value).ToString()
                    Next

                    step_intervals.Add(intervalx)
                    step_intervals.Add(intervaly)
                    Dim ChildNode As clsNode

                    If single_non_pw.Node.IsTerminalNode Then
                        ChildNode = AltsHierarchy.GetNodeByID(StepData.NodeID)
                    Else
                        ChildNode = ObjHierarchy.GetNodeByID(StepData.NodeID)
                    End If

                    Dim StepParentNode = single_non_pw.Node
                    StepNode = StepParentNode
                    first_node_info = CStr(InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, If(ChildNode.IsAlternative, Consts.reObjectType.Alternative, Consts.reObjectType.Node), ChildNode.NodeID.ToString(), ChildNode.InfoDoc, True, True, -1))
                    parent_node_info = CStr(InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, If(StepParentNode.IsAlternative, Consts.reObjectType.Alternative, Consts.reObjectType.Node), StepParentNode.NodeID.ToString(), StepParentNode.InfoDoc, True, True, -1))
                    wrt_first_node_info = CStr(InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.AltWRTNode, ChildNode.NodeID.ToString(), App.ActiveProject.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(ChildNode.NodeGuidID, StepParentNode.NodeGuidID), True, True, StepParentNode.NodeID))
                    ParentNodeName = StepParentNode.NodeName
                    ChildNodeName = ChildNode.NodeName
                    ParentNodeGUID = StepParentNode.NodeGuidID
                    LeftNodeGUID = ChildNode.NodeGuidID
                    SetInfoDoc_Params(StepParentNode.NodeGuidID, ChildNode.NodeGuidID)
                    StepTask = ""

                    Try
                        StepTask = TeamTimeClass.GetPipeStepTask(AnytimeAction, Nothing, AnytimeClass.IsImpact AndAlso Not StepParentNode.IsTerminalNode)
                    Catch
                        StepTask = ""
                    End Try

                    Dim step_value = New Double()

                    If Not Double.IsNaN(Convert.ToDouble(StepData.ObjectValue)) Then
                        step_value = Convert.ToDouble(StepData.ObjectValue)
                    Else
                        step_value = ECCore.TeamTimeFuncs.TeamTimeFuncs.UndefinedValue
                    End If

                    CurrentValue = Convert.ToDouble(step_value)
                    piecewise = StepData.StepFunction.IsPiecewiseLinear
                    ParentNodeID = single_non_pw.Node.NodeID
                    Dim SF = CType(single_non_pw.MeasurementScale, clsStepFunction)
                    Dim Low As Single = (CType(SF.Intervals(0), clsStepInterval)).Low
                    Dim High As Single = (CType(SF.Intervals(SF.Intervals.Count - 1), clsStepInterval)).Low
                    Dim XMinValue = Low - (High - Low) / 10
                    Dim XMaxValue = High + (High - Low) / 10
                    PipeParameters = New With {
                        .min = XMinValue,
                        .max = XMaxValue
                    }
                    IsUndefined = StepData.IsUndefined
                Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                    pipeHelpUrl = TeamTimeClass.ResString("help_pipe_utilityCurve")
                    qh_help_id = Canvas.PipeParameters.ecEvaluationStepType.UtilityCurve
                    Dim tParentNode As clsNode = Nothing
                    Dim tAlt As clsNode = Nothing
                    NonPWType = single_non_pw.MeasurementType.ToString()
                    Dim tData_Judgment = CType(single_non_pw.Judgment, clsUtilityCurveMeasureData)
                    Dim RUC = CType(single_non_pw.MeasurementScale, clsRegularUtilityCurve)
                    Dim XMinValue = RUC.Low
                    Dim XMaxValue = RUC.High
                    Dim Curvature = StringFuncs.JS_SafeString(RUC.Curvature)
                    Dim Decreasing = (Not RUC.IsIncreasing).ToString().ToLower()
                    Dim XValue As Double = 0

                    If single_non_pw.Judgment IsNot Nothing Then
                        Dim RUCData = tData_Judgment

                        If RUCData.IsUndefined Then
                            XValue = -2147483648000
                        Else
                            XValue = RUCData.Data
                        End If
                    End If

                    UCData = New With {
                        .RUC = RUC,
                        .XMinValue = XMinValue,
                        .XMaxValue = XMaxValue,
                        .Curvature = Curvature,
                        .XValue = XValue,
                        .Decreasing = Decreasing
                    }
                    tParentNode = single_non_pw.Node
                    StepNode = tParentNode

                    If tParentNode.IsTerminalNode Then
                        tAlt = AltsHierarchy.GetNodeByID(tData_Judgment.NodeID)
                    Else
                        tAlt = ObjHierarchy.GetNodeByID(tData_Judgment.NodeID)
                    End If

                    first_node_info = CStr(InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, If(tAlt.IsAlternative, Consts.reObjectType.Alternative, Consts.reObjectType.Node), tAlt.NodeID.ToString(), tAlt.InfoDoc, True, True, -1))
                    parent_node_info = CStr(InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, If(tParentNode.IsAlternative, Consts.reObjectType.Alternative, Consts.reObjectType.Node), tParentNode.NodeID.ToString(), tParentNode.InfoDoc, True, True, -1))
                    wrt_first_node_info = CStr(InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.AltWRTNode, tAlt.NodeID.ToString(), App.ActiveProject.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(tAlt.NodeGuidID, tParentNode.NodeGuidID), True, True, tParentNode.NodeID))
                    ParentNodeName = tParentNode.NodeName
                    ChildNodeName = tAlt.NodeName
                    ParentNodeGUID = tParentNode.NodeGuidID
                    LeftNodeGUID = tAlt.NodeGuidID
                    SetInfoDoc_Params(tParentNode.NodeGuidID, tAlt.NodeGuidID)
                    StepTask = ""

                    Try
                        StepTask = TeamTimeClass.GetPipeStepTask(AnytimeAction, Nothing, AnytimeClass.IsImpact AndAlso Not tParentNode.IsTerminalNode)
                    Catch
                        StepTask = ""
                    End Try

                    Comment = tData_Judgment.Comment
                    ParentNodeID = single_non_pw.Node.NodeID
                    IsUndefined = tData_Judgment.IsUndefined
                    Exit Select
            End Select
        End If
    End Function

    Public Shared Function SetInfoDoc_Params(ByVal ParentNodeID As Guid, ByVal ChildNodeID As Guid)
        Dim infodoc_params As String() = New String(4) {}
        infodoc_params(0) = GeckoClass.GetInfodocParams(ParentNodeID, Guid.Empty)
        infodoc_params(1) = GeckoClass.GetInfodocParams(ChildNodeID, Guid.Empty)
        infodoc_params(3) = GeckoClass.GetInfodocParams(ChildNodeID, ParentNodeID)
    End Function





End Class