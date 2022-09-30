Imports ExpertChoice.Web.Controls

Partial Public Class UtilityCurve
    Inherits System.Web.UI.UserControl

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    End Sub

    Public Shared Function GetDetails(ByVal AnytimeAction As clsAction, ByVal App As clsComparionCore) As UtilityCurveModel
        Dim ucModel As UtilityCurveModel = New UtilityCurveModel()
        Dim single_non_pw = CType(AnytimeAction.ActionData, clsOneAtATimeEvaluationActionData)
        Dim ObjHierarchy = CType(App.ActiveProject.ProjectManager.Hierarchy(App.ActiveProject.ProjectManager.ActiveHierarchy), clsHierarchy)
        Dim AltsHierarchy = CType(App.ActiveProject.ProjectManager.AltsHierarchy(App.ActiveProject.ProjectManager.ActiveAltsHierarchy), clsHierarchy)

        If single_non_pw.Node IsNot Nothing AndAlso single_non_pw.Judgment IsNot Nothing Then
            Dim measuretype = CType(AnytimeAction.ActionData, clsNonPairwiseEvaluationActionData)

            Select Case (CType(AnytimeAction.ActionData, clsNonPairwiseEvaluationActionData)).MeasurementType
                Case ECMeasureType.mtRatings
                    ucModel.pipeHelpUrl = TeamTimeClass.ResString("help_pipe_rating")
                    ucModel.qh_help_id = Canvas.PipeParameters.ecEvaluationStepType.Ratings
                    ucModel.NonPWType = "mtRatings"
                    Dim r_data As clsOneAtATimeEvaluationActionData = CType(AnytimeAction.ActionData, clsOneAtATimeEvaluationActionData)
                    Dim r_judgment As clsNonPairwiseMeasureData = CType(r_data.Judgment, clsNonPairwiseMeasureData)
                    Dim tJud As clsRatingMeasureData = CType(r_data.Judgment, clsRatingMeasureData)
                    ucModel.NonPWValue = If(tJud.IsUndefined, "-1", tJud.Rating.Value.ToString())

                    If tJud.IsUndefined Then
                        ucModel.NonPWValue = "-1"
                    Else
                        ucModel.NonPWValue = tJud.Rating.Value.ToString()
                        ucModel.is_direct = If(tJud.Rating.ID = -1, True, False)
                    End If

                    ucModel.comment = tJud.Comment
                    Dim r_ParentNode = r_data.Node
                    Dim r_ChildNode As clsNode = Nothing

                    If r_data.Node.IsAlternative Then
                        r_ParentNode = ObjHierarchy.Nodes(0)
                        r_ChildNode = r_data.Node
                    Else
                        r_ParentNode = r_data.Node

                        If r_ParentNode IsNot Nothing AndAlso r_ParentNode.IsTerminalNode Then
                            r_ChildNode = AltsHierarchy.GetNodeByID(r_judgment.NodeID)
                        Else
                            r_ChildNode = ObjHierarchy.GetNodeByID(r_judgment.NodeID)
                        End If
                    End If

                    ucModel.StepNode = r_ParentNode
                    ucModel.StepTask = ""

                    Try
                        ucModel.StepTask = TeamTimeClass.GetPipeStepTask(AnytimeAction, Nothing, AnytimeClass.IsImpact AndAlso Not r_ParentNode.IsTerminalNode)
                    Catch
                        ucModel.StepTask = ""
                    End Try

                    ucModel.ParentNodeName = r_ParentNode.NodeName
                    ucModel.ChildNodeName = r_ChildNode.NodeName
                    ucModel.ParentNodeGUID = r_ParentNode.NodeGuidID
                    ucModel.LeftNodeGUID = r_ChildNode.NodeGuidID
                    ucModel.infodoc_params(0) = GeckoClass.GetInfodocParams(r_ParentNode.NodeGuidID, Guid.Empty)
                    ucModel.infodoc_params(1) = GeckoClass.GetInfodocParams(r_ChildNode.NodeGuidID, Guid.Empty)
                    ucModel.infodoc_params(3) = GeckoClass.GetInfodocParams(r_ChildNode.NodeGuidID, r_ParentNode.NodeGuidID)
                    Dim MScale As clsRatingScale = CType(r_data.MeasurementScale, clsRatingScale)

                    If MScale IsNot Nothing Then
                        ucModel.showPriorityAndDirectValue = App.ActiveProject.ProjectManager.Parameters.RatingsUseDirectValue(MScale.GuidID)
                        ucModel.precision = AnytimeClass.GetPrecisionForRatings(CType(MScale, clsRatingScale))
                        Dim sd As ScaleDescriptions = New ScaleDescriptions()
                        sd.Name = MScale.Name
                        sd.Guid = MScale.GuidID.ToString()
                        sd.Description = InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, reObjectType.MeasureScale, MScale.GuidID.ToString(), MScale.Comment, True, True, -1)

                        ucModel.scaleDescriptions.Add(sd)
                    End If

                    For Each intensity As clsRating In MScale.RatingSet
                        ucModel.intensities.Add(New String() {intensity.Value.ToString(), intensity.Name.ToString(), intensity.ID.ToString(), intensity.Priority.ToString(), intensity.Comment})
                    Next

                    ucModel.intensities.Add(New String() {"-1", "Not Rated", "-1", "0", ""})
                    ucModel.intensities.Add(New String() {"0", "Direct Value", "-2", "0", ""})
                    ucModel.first_node_info = CStr(InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, If(r_ChildNode.IsAlternative, Consts.reObjectType.Alternative, Consts.reObjectType.Node), r_ChildNode.NodeID.ToString(), r_ChildNode.InfoDoc, True, True, -1))
                    ucModel.parent_node_info = CStr(InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, If(r_ParentNode.IsAlternative, Consts.reObjectType.Alternative, Consts.reObjectType.Node), r_ParentNode.NodeID.ToString(), r_ParentNode.InfoDoc, True, True, -1))
                    ucModel.wrt_first_node_info = CStr(InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.AltWRTNode, r_ChildNode.NodeID.ToString(), App.ActiveProject.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(r_ChildNode.NodeGuidID, r_ParentNode.NodeGuidID), True, True, r_ParentNode.NodeID))

                    If tJud.IsUndefined Then
                        ucModel.IsUndefined = True
                    End If

                    ucModel.ParentNodeID = r_data.Node.NodeID
                Case ECMeasureType.mtDirect
                    ucModel.qh_help_id = ecEvaluationStepType.DirectInput
                    ucModel.NonPWType = "mtDirect"
                    Dim d_tAlt As clsNode = Nothing
                    Dim d_tParentNode As clsNode = Nothing
                    Dim d_data As clsOneAtATimeEvaluationActionData = CType(AnytimeAction.ActionData, clsOneAtATimeEvaluationActionData)
                    Dim d_judgment As clsNonPairwiseMeasureData = CType(d_data.Judgment, clsNonPairwiseMeasureData)
                    Dim d_measure_data As clsDirectMeasureData = CType(d_data.Judgment, clsDirectMeasureData)
                    ucModel.comment = d_measure_data.Comment

                    If d_measure_data IsNot Nothing Then

                        If Not d_measure_data.IsUndefined Then

                            If Convert.ToDecimal(d_measure_data.ObjectValue.ToString()) < 0 Then
                                ucModel.NonPWValue = "0"
                            ElseIf Convert.ToDecimal(d_measure_data.ObjectValue.ToString()) > 1 Then
                                ucModel.NonPWValue = "1"
                            Else
                                ucModel.precision = 4
                                ucModel.NonPWValue = StringFuncs.JS_SafeNumber(d_measure_data.ObjectValue)
                            End If
                        End If
                    End If

                    If AnytimeAction.ActionData IsNot Nothing AndAlso d_data.Judgment IsNot Nothing Then
                        Dim DirectData = CType(d_data.Judgment, clsDirectMeasureData)
                        d_tParentNode = d_data.Node
                        ucModel.StepNode = d_tParentNode

                        If d_tParentNode.IsTerminalNode Then
                            d_tAlt = AltsHierarchy.GetNodeByID(DirectData.NodeID)
                        Else
                            d_tAlt = ObjHierarchy.GetNodeByID(DirectData.NodeID)
                        End If

                        ucModel.StepTask = ""

                        Try
                            ucModel.StepTask = TeamTimeClass.GetPipeStepTask(AnytimeAction, Nothing, AnytimeClass.IsImpact AndAlso Not d_tParentNode.IsTerminalNode)
                        Catch
                            ucModel.StepTask = ""
                        End Try

                        ucModel.ParentNodeName = d_tParentNode.NodeName
                        ucModel.ChildNodeName = d_tAlt.NodeName
                        ucModel.ParentNodeGUID = d_tParentNode.NodeGuidID
                        ucModel.LeftNodeGUID = d_tAlt.NodeGuidID
                        ucModel.infodoc_params(0) = GeckoClass.GetInfodocParams(d_tParentNode.NodeGuidID, Guid.Empty)
                        ucModel.infodoc_params(1) = GeckoClass.GetInfodocParams(d_tAlt.NodeGuidID, Guid.Empty)
                        ucModel.infodoc_params(3) = GeckoClass.GetInfodocParams(d_tAlt.NodeGuidID, d_tParentNode.NodeGuidID)
                        ucModel.first_node_info = CStr(InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, If(d_tAlt.IsAlternative, Consts.reObjectType.Alternative, Consts.reObjectType.Node), d_tAlt.NodeID.ToString(), d_tAlt.InfoDoc, True, True, -1))
                        ucModel.parent_node_info = CStr(InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, If(d_tAlt.IsAlternative, Consts.reObjectType.Alternative, Consts.reObjectType.Node), d_tParentNode.NodeID.ToString(), d_tParentNode.InfoDoc, True, True, -1))
                        ucModel.wrt_first_node_info = CStr(InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.AltWRTNode, d_tAlt.NodeID.ToString(), App.ActiveProject.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(d_tAlt.NodeGuidID, d_tParentNode.NodeGuidID), True, True, d_tParentNode.NodeID))
                    End If

                    If d_measure_data.IsUndefined Then
                        ucModel.IsUndefined = True
                    End If

                    ucModel.ParentNodeID = d_data.Node.NodeID
                Case ECMeasureType.mtStep
                    ucModel.pipeHelpUrl = TeamTimeClass.ResString("help_pipe_stepFunction")
                    ucModel.qh_help_id = Canvas.PipeParameters.ecEvaluationStepType.StepFunction
                    Dim StepActionData As clsOneAtATimeEvaluationActionData = CType(AnytimeAction.ActionData, clsOneAtATimeEvaluationActionData)
                    ucModel.NonPWType = StepActionData.MeasurementType.ToString()
                    Dim StepData = CType(StepActionData.Judgment, clsStepMeasureData)
                    StepData.StepFunction.SortByInterval()
                    ucModel.comment = StepData.Comment
                    Dim intervalx = New String(StepData.StepFunction.Intervals.Count - 1) {}
                    Dim intervaly = New String(StepData.StepFunction.Intervals.Count - 1) {}

                    For Each period As clsStepInterval In StepData.StepFunction.Intervals
                        Dim ind = StepData.StepFunction.Intervals.IndexOf(period)
                        intervalx(ind) = period.Low.ToString()
                        intervaly(ind) = (period.Value).ToString()
                    Next

                    Try
                        ucModel.step_intervals.Add(intervalx)
                        ucModel.step_intervals.Add(intervaly)
                    Catch ex As Exception
                        Dim msg = ex.Message
                    End Try

                    Dim ChildNode As clsNode

                    If single_non_pw.Node.IsTerminalNode Then
                        ChildNode = AltsHierarchy.GetNodeByID(StepData.NodeID)
                    Else
                        ChildNode = ObjHierarchy.GetNodeByID(StepData.NodeID)
                    End If

                    Dim StepParentNode = single_non_pw.Node
                    ucModel.StepNode = StepParentNode
                    ucModel.first_node_info = CStr(InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, If(ChildNode.IsAlternative, Consts.reObjectType.Alternative, Consts.reObjectType.Node), ChildNode.NodeID.ToString(), ChildNode.InfoDoc, True, True, -1))
                    ucModel.parent_node_info = CStr(InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, If(StepParentNode.IsAlternative, Consts.reObjectType.Alternative, Consts.reObjectType.Node), StepParentNode.NodeID.ToString(), StepParentNode.InfoDoc, True, True, -1))
                    ucModel.wrt_first_node_info = CStr(InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.AltWRTNode, ChildNode.NodeID.ToString(), App.ActiveProject.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(ChildNode.NodeGuidID, StepParentNode.NodeGuidID), True, True, StepParentNode.NodeID))
                    ucModel.ParentNodeName = StepParentNode.NodeName
                    ucModel.ChildNodeName = ChildNode.NodeName
                    ucModel.ParentNodeGUID = StepParentNode.NodeGuidID
                    ucModel.LeftNodeGUID = ChildNode.NodeGuidID
                    If ucModel.infodoc_params IsNot Nothing Then
                        ucModel.infodoc_params(0) = GeckoClass.GetInfodocParams(StepParentNode.NodeGuidID, Guid.Empty)
                        ucModel.infodoc_params(1) = GeckoClass.GetInfodocParams(ChildNode.NodeGuidID, Guid.Empty)
                        ucModel.infodoc_params(3) = GeckoClass.GetInfodocParams(ChildNode.NodeGuidID, StepParentNode.NodeGuidID)
                    End If
                    ucModel.StepTask = ""

                    Try
                        ucModel.StepTask = TeamTimeClass.GetPipeStepTask(AnytimeAction, Nothing, AnytimeClass.IsImpact AndAlso Not StepParentNode.IsTerminalNode)
                    Catch
                        ucModel.StepTask = ""
                    End Try

                    Dim step_value = New Double()

                    If Not Double.IsNaN(Convert.ToDouble(StepData.ObjectValue)) Then
                        step_value = Convert.ToDouble(StepData.ObjectValue)
                    Else
                        step_value = ECCore.TeamTimeFuncs.TeamTimeFuncs.UndefinedValue
                    End If

                    ucModel.CurrentValue = Convert.ToDouble(step_value)
                    ucModel.piecewise = StepData.StepFunction.IsPiecewiseLinear
                    ucModel.ParentNodeID = StepActionData.Node.NodeID
                    Dim SF = CType(StepActionData.MeasurementScale, clsStepFunction)
                    Dim Low As Single = (CType(SF.Intervals(0), clsStepInterval)).Low
                    Dim High As Single = (CType(SF.Intervals(SF.Intervals.Count - 1), clsStepInterval)).Low
                    Dim XMinValue = Low - (High - Low) / 10
                    Dim XMaxValue = High + (High - Low) / 10
                    ucModel.PipeParameters = New With {
                        Key .min = XMinValue,
                        Key .max = XMaxValue
                    }
                    ucModel.IsUndefined = StepData.IsUndefined
                Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                    ucModel.pipeHelpUrl = TeamTimeClass.ResString("help_pipe_utilityCurve")
                    ucModel.qh_help_id = ecEvaluationStepType.UtilityCurve
                    Dim tParentNode As clsNode = Nothing
                    Dim tAlt As clsNode = Nothing
                    Dim tData As clsOneAtATimeEvaluationActionData = CType(AnytimeAction.ActionData, clsOneAtATimeEvaluationActionData)
                    ucModel.NonPWType = tData.MeasurementType.ToString()
                    Dim tData_Judgment = CType(tData.Judgment, clsUtilityCurveMeasureData)
                    Dim RUC = CType(tData.MeasurementScale, clsRegularUtilityCurve)
                    Dim XMinValue = RUC.Low
                    Dim XMaxValue = RUC.High
                    Dim Curvature = StringFuncs.JS_SafeString(RUC.Curvature)
                    Dim Decreasing = (Not RUC.IsIncreasing).ToString().ToLower()
                    Dim XValue As Double = 0

                    Dim Description As String = InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, reObjectType.MeasureScale, RUC.GuidID.ToString(), RUC.Comment, True, True, -1)

                    Dim scaleDescriptions As List(Of ScaleDescriptions) = New List(Of ScaleDescriptions)()

                    scaleDescriptions.Add(New ScaleDescriptions() With {.Description = Description, .Guid = RUC.GuidID.ToString()})
                    ucModel.scaleDescriptions = scaleDescriptions
                    If tData.Judgment IsNot Nothing Then
                        Dim RUCData = tData_Judgment

                        If RUCData.IsUndefined Then
                            XValue = -2147483648000
                        Else
                            XValue = RUCData.Data
                        End If
                    End If

                    Dim UCData As UCDataModel = New UCDataModel()

                    UCData.RUC = RUC
                    UCData.XMinValue = CInt(XMinValue)
                    UCData.XMaxValue = CInt(XMaxValue)
                    UCData.Curvature = CDbl(Curvature)
                    UCData.XValue = CDbl(XValue)
                    UCData.Decreasing = CBool(Decreasing)

                    ucModel.UCData = UCData

                    'ucModel.UCData = New With {
                    '    Key .RUC = RUC,
                    '    Key .XMinValue = XMinValue,
                    '    Key .XMaxValue = XMaxValue,
                    '    Key .Curvature = Curvature,
                    '    Key .XValue = XValue,
                    '    Key .Decreasing = Decreasing
                    '}
                    tParentNode = tData.Node
                    ucModel.StepNode = tParentNode

                    If tParentNode.IsTerminalNode Then
                        tAlt = AltsHierarchy.GetNodeByID(tData_Judgment.NodeID)
                    Else
                        tAlt = ObjHierarchy.GetNodeByID(tData_Judgment.NodeID)
                    End If

                    ucModel.first_node_info = CStr(InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, If(tAlt.IsAlternative, Consts.reObjectType.Alternative, Consts.reObjectType.Node), tAlt.NodeID.ToString(), tAlt.InfoDoc, True, True, -1))
                    ucModel.parent_node_info = CStr(InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, If(tParentNode.IsAlternative, Consts.reObjectType.Alternative, Consts.reObjectType.Node), tParentNode.NodeID.ToString(), tParentNode.InfoDoc, True, True, -1))
                    ucModel.wrt_first_node_info = CStr(InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, Consts.reObjectType.AltWRTNode, tAlt.NodeID.ToString(), App.ActiveProject.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(tAlt.NodeGuidID, tParentNode.NodeGuidID), True, True, tParentNode.NodeID))
                    ucModel.ParentNodeName = tParentNode.NodeName
                    ucModel.ChildNodeName = tAlt.NodeName
                    ucModel.ParentNodeGUID = tParentNode.NodeGuidID
                    ucModel.LeftNodeGUID = tAlt.NodeGuidID
                    ucModel.infodoc_params = New String(3) {}
                    ucModel.infodoc_params(0) = GeckoClass.GetInfodocParams(tParentNode.NodeGuidID, Guid.Empty)
                    ucModel.infodoc_params(1) = GeckoClass.GetInfodocParams(tAlt.NodeGuidID, Guid.Empty)
                    ucModel.infodoc_params(3) = GeckoClass.GetInfodocParams(tAlt.NodeGuidID, tParentNode.NodeGuidID)
                    ucModel.StepTask = ""
                    'ucModel.scaleDescriptions = DBNull
                    Try
                        ucModel.StepTask = TeamTimeClass.GetPipeStepTask(AnytimeAction, Nothing, AnytimeClass.IsImpact AndAlso Not tParentNode.IsTerminalNode)
                    Catch
                        ucModel.StepTask = ""
                    End Try

                    ucModel.comment = tData_Judgment.Comment
                    ucModel.ParentNodeID = tData.Node.NodeID
                    ucModel.IsUndefined = tData_Judgment.IsUndefined
                    Exit Select
            End Select
        End If

        Return ucModel
    End Function

    Private _Data As Object

    'Private _Header As String = ""      ' D1123 - D2181

    ' D1011 ===
    Private _CaptionName As String = ""
    Private _CaptionInfodoc As String = ""
    Private _CaptionInfodocCollapsed As Boolean = False
    Private _CaptionInfodocURL As String = ""
    Private _CaptionInfodocEditURL As String = ""
    Private _AlternativeName As String = ""
    Private _AlternativeInfodoc As String = ""
    Private _AlternativeInfodocURL As String = ""
    Private _AlternativeInfodocCollapsed As Boolean = False
    Private _AlternativeInfodocEditURL As String = ""
    Private _WRTInfodoc As String = ""
    Private _WRTInfodocURL As String = ""
    Private _WRTInfodocCollapsed As Boolean = False
    Private _WRTInfodocEditURL As String = ""
    Private _YAxisCaption As String = "Priority"
    ' D4933 ===
    Public Property ScaleInfodoc As String = ""
    Public Property ScaleInfodocURL As String = ""
    Public Property ScaleInfodocCollapsed As Boolean = False
    Public Property ScaleInfodocEditURL As String = ""
    Public Property lblInfodocTitleScale As String = ""
    ' D4933 ==
    Private _msgWrongNumber As String = "Wrong Number"
    ' D1011 ==

    Public ParentNodeID As String = ""  ' D2505

    Public sRootPath As String = ""     ' D1593

    Public CustomCodeOnInit As String = ""  ' D2665

    Public Sub New()
        _Data = Nothing
        If Page Is Nothing Then AddHandler Load, AddressOf InitComponent Else AddHandler Page.Load, AddressOf InitComponent
    End Sub

    Public Property Data() As Object
        Get
            Return _Data
        End Get
        Set(value As Object)
            _Data = value
        End Set
    End Property

    Private _StepsX As String = ""
    Public Property StepsX As String
        Get
            Return _StepsX
        End Get
        Set(value As String)
            _StepsX = value
        End Set
    End Property

    Private _StepsY As String = ""
    Public Property StepsY As String
        Get
            Return _StepsY
        End Get
        Set(value As String)
            _StepsY = value
        End Set
    End Property

    Private _XValue As Single = 0
    Public Property XValue As Single
        Get
            Return _XValue
        End Get
        Set(value As Single)
            _XValue = value
        End Set
    End Property

    Public ReadOnly Property XValueString As String
        Get
            Dim sValue As String = ""
            If ActionData IsNot Nothing Then
                Select Case ActionData.MeasurementType
                    Case ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtRegularUtilityCurve
                        Dim RUCData = CType(ActionData.Judgment, clsUtilityCurveMeasureData)
                        If Not RUCData.IsUndefined Then sValue = JS_SafeNumber(XValue)
                    Case ECMeasureType.mtStep
                        Dim SFData = CType(ActionData.Judgment, clsStepMeasureData)
                        If Not SFData.IsUndefined Then sValue = JS_SafeNumber(XValue)
                End Select
            End If
            Return sValue
        End Get
    End Property

    Private _XMinValue As Single = 0
    Public Property XMinValue As Single
        Get
            Return _XMinValue
        End Get
        Set(value As Single)
            _XMinValue = value
        End Set
    End Property

    Private _XMaxValue As Single = 100
    Public Property XMaxValue As Single
        Get
            Return _XMaxValue
        End Get
        Set(value As Single)
            _XMaxValue = value
        End Set
    End Property

    Public ReadOnly Property XMin As String
        Get
            Return JS_SafeNumber(XMinValue)
        End Get
    End Property

    Public ReadOnly Property XMax As String
        Get
            Return JS_SafeNumber(XMaxValue)
        End Get
    End Property

    Public Property YAxisCaption As String
        Get
            Return _YAxisCaption
        End Get
        Set(value As String)
            _YAxisCaption = value
        End Set
    End Property

    Private _UCType As String = "RUC"
    Public Property UCType As String
        Get
            Return _UCType
        End Get
        Set(value As String)
            _UCType = value
        End Set
    End Property

    Private _PWLString As String = "0"
    Public Property PWLString As String
        Get
            Return _PWLString
        End Get
        Set(value As String)
            _PWLString = value
        End Set
    End Property

    Private _Decreasing As String = "false"
    Public Property Decreasing As String
        Get
            Return _Decreasing
        End Get
        Set(value As String)
            _Decreasing = value
        End Set
    End Property

    Private _Curvature As String = "1"
    Public Property Curvature As String
        Get
            Return _Curvature
        End Get
        Set(value As String)
            _Curvature = value
        End Set
    End Property

    Public Property msgWrongNumber() As String
        Get
            Return _msgWrongNumber
        End Get
        Set(value As String)
            _msgWrongNumber = value
        End Set
    End Property

    Private ReadOnly Property ActionData() As clsOneAtATimeEvaluationActionData 'C0464
        Get
            If Not Data Is Nothing Then Return CType(Data, clsOneAtATimeEvaluationActionData) Else Return Nothing 'C0464
        End Get
    End Property

    Private ReadOnly Property Judgment() As clsNonPairwiseMeasureData   ' D1699
        Get
            Dim Action As clsOneAtATimeEvaluationActionData = ActionData 'C0464
            If Not Action Is Nothing Then
                ' D1699 ===
                Select Case ActionData.MeasurementType
                    Case ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtRegularUtilityCurve
                        Return CType(ActionData.Judgment, clsUtilityCurveMeasureData)
                    Case ECMeasureType.mtStep
                        Return CType(ActionData.Judgment, clsStepMeasureData)
                End Select
                ' D1699 ==
            End If
            Return Nothing
        End Get
    End Property

    ' -D2181
    '' D1123 ===
    'Public Property Header() As String
    '    Get
    '        Return _Header
    '    End Get
    '    Set(ByVal value As String)
    '        _Header = value
    '    End Set
    'End Property
    '' D1123 ==

    ' D1011 ===
    Public Property CaptionName() As String
        Get
            Return _CaptionName
        End Get
        Set(value As String)
            _CaptionName = value
        End Set
    End Property

    Public Property CaptionInfodoc() As String
        Get
            Return _CaptionInfodoc
        End Get
        Set(value As String)
            _CaptionInfodoc = value
        End Set
    End Property

    Public Property CaptionInfodocURL() As String
        Get
            Return _CaptionInfodocURL
        End Get
        Set(value As String)
            _CaptionInfodocURL = value
        End Set
    End Property

    Public Property CaptionInfodocCollapsed() As Boolean
        Get
            Return _CaptionInfodocCollapsed
        End Get
        Set(value As Boolean)
            _CaptionInfodocCollapsed = value
        End Set
    End Property

    Public Property CaptionInfodocEditURL() As String
        Get
            Return _CaptionInfodocEditURL
        End Get
        Set(value As String)
            _CaptionInfodocEditURL = value
        End Set
    End Property

    Public Property AlternativeName() As String
        Get
            Return _AlternativeName
        End Get
        Set(value As String)
            _AlternativeName = value
        End Set
    End Property

    Public Property AlternativeInfodoc() As String
        Get
            Return _AlternativeInfodoc
        End Get
        Set(value As String)
            _AlternativeInfodoc = value
        End Set
    End Property

    Public Property AlternativeInfodocURL() As String
        Get
            Return _AlternativeInfodocURL
        End Get
        Set(value As String)
            _AlternativeInfodocURL = value
        End Set
    End Property

    Public Property AlternativeInfodocCollapsed() As Boolean
        Get
            Return _AlternativeInfodocCollapsed
        End Get
        Set(value As Boolean)
            _AlternativeInfodocCollapsed = value
        End Set
    End Property

    Public Property AlternativeInfodocEditURL() As String
        Get
            Return _AlternativeInfodocEditURL
        End Get
        Set(value As String)
            _AlternativeInfodocEditURL = value
        End Set
    End Property

    Public Property WRTInfodoc() As String
        Get
            Return _WRTInfodoc
        End Get
        Set(value As String)
            _WRTInfodoc = value
        End Set
    End Property

    Public Property WRTInfodocURL() As String
        Get
            Return _WRTInfodocURL
        End Get
        Set(value As String)
            _WRTInfodocURL = value
        End Set
    End Property

    Public Property WRTInfodocCollapsed() As Boolean
        Get
            Return _WRTInfodocCollapsed
        End Get
        Set(value As Boolean)
            _WRTInfodocCollapsed = value
        End Set
    End Property

    Public Property WRTInfodocEditURL() As String
        Get
            Return _WRTInfodocEditURL
        End Get
        Set(value As String)
            _WRTInfodocEditURL = value
        End Set
    End Property
    ' D1011 ==
    Protected Sub InitComponent(sender As Object, e As EventArgs)

    End Sub

    Protected Sub Page_PreRender(sender As Object, e As EventArgs) Handles Me.PreRender
        'If ActionData.MeasurementScale IsNot Nothing Then   ' D3472
        '    lblMessage.Visible = False  ' D3472
        Select Case ActionData.MeasurementType
            Case ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtRegularUtilityCurve
                Dim RUC = CType(ActionData.MeasurementScale, clsRegularUtilityCurve)
                XMinValue = RUC.Low
                XMaxValue = RUC.High
                Curvature = JS_SafeNumber(RUC.Curvature)
                Decreasing = (Not RUC.IsIncreasing).ToString.ToLower
                UCType = "RUC"

                If ActionData.Judgment IsNot Nothing Then
                    Dim RUCData = CType(ActionData.Judgment, clsUtilityCurveMeasureData)
                    If RUCData.IsUndefined Then
                        XValue = XMinValue - 1
                    Else
                        XValue = CSng(Math.Round(RUCData.Data, 2))
                    End If
                End If
                'pnlRUC.Visible = True
            Case ECMeasureType.mtStep
                Dim SF = CType(ActionData.MeasurementScale, clsStepFunction)
                'SF.SortByInterval()
                Dim tmp As Single = SF.GetValue(0)  ' D6333
                Dim Low As Single = CType(SF.Intervals(0), clsStepInterval).Low
                Dim High As Single = CType(SF.Intervals(SF.Intervals.Count - 1), clsStepInterval).Low
                'XMinValue = Low - (High - Low) / 10
                'XMaxValue = High + (High - Low) / 10
                XMinValue = Low
                XMaxValue = High
                If SF.IsPiecewiseLinear Then
                    PWLString = "1"
                Else
                    PWLString = "0"
                End If
                For Each interval As clsStepInterval In SF.Intervals
                    If SF.Intervals.IndexOf(interval) < SF.Intervals.Count - 1 Then
                        If SF.IsPiecewiseLinear Then
                            StepsX += String.Format("{0}, ", JS_SafeNumber(interval.Low))
                            StepsY += String.Format("{0}, ", JS_SafeNumber(interval.Value))
                        Else
                            StepsX += String.Format("{0}, {1}, ", JS_SafeNumber(interval.Low), JS_SafeNumber(interval.High))
                            StepsY += String.Format("{0}, {1}, ", JS_SafeNumber(interval.Value), JS_SafeNumber(interval.Value))
                        End If
                    Else
                        StepsX += String.Format("{0}, {1}", JS_SafeNumber(interval.Low), JS_SafeNumber(interval.Low))
                        StepsY += String.Format("{0}, {1}", JS_SafeNumber(interval.Value), JS_SafeNumber(interval.Value))
                    End If
                Next
                UCType = "Step"

                If ActionData.Judgment IsNot Nothing Then
                    Dim SFData = CType(ActionData.Judgment, clsStepMeasureData)
                    If SFData.IsUndefined Then
                        'XValue = XMinValue - 1
                        XValue = UNDEFINED_SINGLE_VALUE ' D3861
                    Else
                        XValue = CSng(Math.Round(SFData.Value, 2))
                    End If
                End If
                'pnlRUC.Visible = True
        End Select
        ' D3472 ===
        'Else
        '    pnlRUC.Visible = False
        '    lblMessage.Text = "<div class='text error' style='border:1px solid #f0f0f0; padding:4em; text-align:center; font-weight:bold;'>Invalid measurement scale</div>"
        '    lblMessage.Visible = True
        'End If
        ' D3472 ==
    End Sub
End Class