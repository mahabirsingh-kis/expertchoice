Imports System.Web.Services

Public Class ctUtilityCurve
    Inherits System.Web.UI.UserControl

    Dim screenCheck As ScreenCheck = New ScreenCheck()

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    End Sub

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

    Private sValue As String = ""
    Public Property XValueString As String
        Get
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
        Set(value As String)
            sValue = value
        End Set
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

    Private _UCType As String = ""
    Public Property UCType As String
        Get
            Dim Action As clsOneAtATimeEvaluationActionData = ActionData 'C0464
            If Not Action Is Nothing Then
                ' D1699 ===
                Select Case ActionData.MeasurementType
                    Case ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtRegularUtilityCurve
                        _UCType = "RUC"
                    Case ECMeasureType.mtStep
                        _UCType = "Step"
                End Select
                ' D1699 ==
            End If
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

    Shared AnyAction As clsAction = New clsAction()
    Public Shared Function GetDetails(ByVal AnytimeAction As clsAction, ByVal App As clsComparionCore) As UtilityCurveModel
        Dim ucModel As UtilityCurveModel = New UtilityCurveModel()
        AnyAction = AnytimeAction
        Dim single_non_pw = CType(AnytimeAction.ActionData, clsOneAtATimeEvaluationActionData)
        Dim ObjHierarchy = CType(App.ActiveProject.ProjectManager.Hierarchy(App.ActiveProject.ProjectManager.ActiveHierarchy), clsHierarchy)
        Dim AltsHierarchy = CType(App.ActiveProject.ProjectManager.AltsHierarchy(App.ActiveProject.ProjectManager.ActiveAltsHierarchy), clsHierarchy)

        Dim infodoc_params As String() = New String(4) {}

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
                    infodoc_params(0) = GeckoClass.GetInfodocParams(r_ParentNode.NodeGuidID, Guid.Empty)
                    infodoc_params(1) = GeckoClass.GetInfodocParams(r_ChildNode.NodeGuidID, Guid.Empty)
                    infodoc_params(3) = GeckoClass.GetInfodocParams(r_ChildNode.NodeGuidID, r_ParentNode.NodeGuidID)
                    Dim MScale As clsRatingScale = CType(r_data.MeasurementScale, clsRatingScale)

                    If MScale IsNot Nothing Then
                        ucModel.showPriorityAndDirectValue = App.ActiveProject.ProjectManager.Parameters.RatingsUseDirectValue(MScale.GuidID)
                        ucModel.precision = AnytimeClass.GetPrecisionForRatings(CType(MScale, clsRatingScale))
                        Dim sdList As List(Of ScaleDescriptions) = New List(Of ScaleDescriptions)
                        Dim sd As ScaleDescriptions = New ScaleDescriptions()
                        sd.Name = MScale.Name
                        sd.Guid = MScale.GuidID.ToString()
                        sd.Description = InfodocService.Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, reObjectType.MeasureScale, MScale.GuidID.ToString(), MScale.Comment, True, True, -1)

                        sdList.Add(sd)
                        ucModel.scaleDescriptions = sdList
                    End If

                    Dim intense As List(Of String()) = New List(Of String())
                    For Each intensity As clsRating In MScale.RatingSet
                        intense.Add(New String() {intensity.Value.ToString(), intensity.Name.ToString(), intensity.ID.ToString(), intensity.Priority.ToString(), intensity.Comment})
                    Next
                    intense.Add(New String() {"-1", "Not Rated", "-1", "0", ""})
                    intense.Add(New String() {"0", "Direct Value", "-2", "0", ""})

                    ucModel.intensities = intense

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
                        infodoc_params(0) = GeckoClass.GetInfodocParams(d_tParentNode.NodeGuidID, Guid.Empty)
                        infodoc_params(1) = GeckoClass.GetInfodocParams(d_tAlt.NodeGuidID, Guid.Empty)
                        infodoc_params(3) = GeckoClass.GetInfodocParams(d_tAlt.NodeGuidID, d_tParentNode.NodeGuidID)
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
                    ucModel.step_intervals = New List(Of String())()
                    ucModel.step_intervals.Add(intervalx)
                    ucModel.step_intervals.Add(intervaly)
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

                    infodoc_params(0) = GeckoClass.GetInfodocParams(StepParentNode.NodeGuidID, Guid.Empty)
                    infodoc_params(1) = GeckoClass.GetInfodocParams(ChildNode.NodeGuidID, Guid.Empty)
                    infodoc_params(3) = GeckoClass.GetInfodocParams(ChildNode.NodeGuidID, StepParentNode.NodeGuidID)
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
                    infodoc_params = New String(3) {}
                    infodoc_params(0) = GeckoClass.GetInfodocParams(tParentNode.NodeGuidID, Guid.Empty)
                    infodoc_params(1) = GeckoClass.GetInfodocParams(tAlt.NodeGuidID, Guid.Empty)
                    infodoc_params(3) = GeckoClass.GetInfodocParams(tAlt.NodeGuidID, tParentNode.NodeGuidID)
                    ucModel.StepTask = ""

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

        ucModel.infodoc_params = infodoc_params
        Return ucModel
    End Function

    Public Sub bindHtml(ByVal model As AnytimeOutputModel)
        If model IsNot Nothing Then
            If model.UCData IsNot Nothing Then
                'piecewise
                XMinValue = model.UCData.XMinValue
                XMaxValue = model.UCData.XMaxValue
                'UCType = model.UCData.RUC.
                Decreasing = model.UCData.Decreasing.ToString().ToLower()
                _XValue = Convert.ToSingle(model.UCData.XValue)
                '_XValue = If(model.UCData.XValue < 0, 0, Convert.ToSingle(model.UCData.XValue))
                If _XValue >= -2147483648000 Then
                    Session("XValue") = XValue
                Else
                    Session("XValue") = ""
                End If

                Curvature = model.UCData.Curvature.ToString()

                'UCType = model.UCData.
            Else
                Dim tData As clsOneAtATimeEvaluationActionData = CType(AnyAction.ActionData, clsOneAtATimeEvaluationActionData)
                Dim SF = CType(tData.MeasurementScale, clsStepFunction) 'tData.MeasurementScale 
                'Dim SF = CType(ActionData.MeasurementScale, clsRatingScale)
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

                If tData.Judgment IsNot Nothing Then
                    Dim SFData = CType(tData.Judgment, clsStepMeasureData)
                    If SFData.IsUndefined Then
                        'XValue = XMinValue - 1
                        XValue = 0 ' D3861
                        Session("XValue") = ""
                    Else
                        XValue = CSng(Math.Round(SFData.Value, 2))
                        Session("XValue") = XValue
                    End If
                End If
            End If
            XValueString = XValue.ToString()

            If (XValueString = "0" Or XValueString = "") Then
                imgClose.Style.Add("filter", "grayscale(1)")
                imgClose.Style.Add("opacity", "0.5")
                imgCloseOne.Style.Add("filter", "grayscale(1)")
                imgCloseOne.Style.Add("opacity", "0.5")
            Else
                imgClose.Style.Add("filter", "none")
                imgClose.Style.Add("opacity", "1")
                imgCloseOne.Style.Add("filter", "none")
                imgCloseOne.Style.Add("opacity", "1")
            End If
            Dim html As StringBuilder = New StringBuilder()
            Dim dta = JsonConvert.SerializeObject(model.PipeParameters)
            Dim firstnodeinfo As String = HttpUtility.UrlEncode(model.first_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
            Dim parentnodeinfo As String = HttpUtility.UrlEncode(model.parent_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
            Dim wrtfirstnodeinfo As String = HttpUtility.UrlEncode(model.wrt_first_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
            Dim wrttext As String = GetWrtText("Left", model.child_node.ToString(), model.parent_node.ToString())
            Dim stepTask As String = HttpUtility.UrlEncode(model.step_task.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
            'Dim scaleDescription As String = HttpUtility.UrlEncode(model.ScaleDescriptions(0).Description.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
            Dim NoInfoDocDataCls As String = ""
            If model.parent_node_info.ToString().Trim() = "" Then
                NoInfoDocDataCls = "NoInfoData"
            End If
            'html.Append($"<div class='col-md-4 offset-md-2'><div class='info_center colorfix {If((model.first_node_info.ToString()) <> "" And (model.first_node_info.ToString()) <> "NaN", "", "chkEvaluator")}'><p class='font-size-14'><i onclick ='showNode(1)' class='fa fa-info-circle {If((model.first_node_info.ToString()) = "" Or (model.first_node_info.ToString()) = "NaN", "infoColor", "")} {If((model.first_node_info.ToString()) <> "" And (model.first_node_info.ToString()) <> "NaN", "", "chkEvaluator")}'></i> {model.first_node}{model.child_node} </p>")

            'html.Append($"<div class='info_tooltip hideshowinfo' style='display:none;' id='parentnode1'><div class='tooltop_head'><span>{model.first_node}{model.child_node}</span><div class='action_icons'>")
            'html.Append($"<a{If(model.isPipeViewOnly, " class='d-none'", "")} href ='#'><i class='bx bxs-edit-alt chkEvaluator d-none' onclick='showHeaderPopup(decodeURIComponent(" + ChrW(&H22) + firstnodeinfo + ChrW(&H22) + ")" + ",0,2,8,1,0,null, decodeURIComponent(" + ChrW(&H22) + firstnodeinfo + ChrW(&H22) + ")" + ")' aria-hidden='true'></i></a>")
            'html.Append("<a href='#'><i class='fa fa-times' onclick='hideNode(1)' aria-hidden='true'></i></a></div></div><p>" + model.first_node_info.ToString() + "</p></div></div></div>")

            'html.Append($"<div class='col-md-3'><div class='info_center colorfix {If((model.parent_node_info.ToString()) <> "" And (model.parent_node_info.ToString()) <> "NaN", "", "chkEvaluator")}'><p class='font-size-14'><i onclick ='showNode(2)' class='fa fa-info-circle {If((model.parent_node_info.ToString()) = "" Or (model.parent_node_info.ToString()) = "NaN", "infoColor", "")} {If((model.parent_node_info.ToString()) <> "" And (model.parent_node_info.ToString()) <> "NaN", "", "chkEvaluator")}'></i> {model.parent_node} </p>")
            'html.Append($"<div class='info_tooltip hideshowinfo' style='display:none;' id='parentnode2'><div class='tooltop_head '><span>" + model.parent_node + "</span><div class='action_icons'>")
            'html.Append($"<a{If(model.isPipeViewOnly, " class='d-none'", "")} href ='#'><i class='bx bxs-edit-alt chkEvaluator d-none' onclick='showInfoPopup(decodeURIComponent(" + ChrW(&H22) + parentnodeinfo + ChrW(&H22) + ")" + ",0,-1,8,-1,0, " + ChrW(&H22) + "parent-node1" + ChrW(&H22) + ", decodeURIComponent(" + ChrW(&H22) + model.parent_node + ChrW(&H22) + ")" + ")' aria-hidden='true'></i></a>")
            'html.Append("<a href='#'><i class='fa fa-times' onclick='hideNode(2)' aria-hidden='true'></i></a></div></div><p>" + model.parent_node_info.ToString() + "</p></div></div></div>")


            'html.Append($"<div class='col-md-3'><div class='info_center colorfix {If((model.wrt_first_node_info.ToString()) <> "" And (model.wrt_first_node_info.ToString()) <> "NaN", "", "chkEvaluator")}'><p class='font-size-14'><span onclick ='showNode(3)' class='wrt_text  {If((model.wrt_first_node_info.ToString()) <> "" And (model.wrt_first_node_info.ToString()) <> "NaN", "", "winfoColor")}  {If((model.wrt_first_node_info.ToString().Trim()) <> "" And (model.wrt_first_node_info.ToString()) <> "NaN", "", "chkEvaluator")}' > wrt </span>" + wrttext + "</p>")
            'html.Append($"<div class='info_tooltip hideshowinfo' style='display:none;' id='parentnode3'><div class='tooltop_head'><span>" + wrttext + "</span><div class='action_icons'>")
            'html.Append($"<a{If(model.isPipeViewOnly, " class='d-none'", "")} href ='#'><i class='bx bxs-edit-alt chkEvaluator d-none' onclick='showInfoPopup(decodeURIComponent(" + ChrW(&H22) + wrtfirstnodeinfo + ChrW(&H22) + ")" + ",0,3,8,1,0, " + ChrW(&H22) + "parent-node1" + ChrW(&H22) + ", decodeURIComponent(" + ChrW(&H22) + wrttext + ChrW(&H22) + ")" + ")' aria-hidden='true'></i></a>")
            'html.Append("<a href='#'><i class='fa fa-times' onclick='hideNode(3)' aria-hidden='true'></i></a></div></div><p>" + model.wrt_first_node_info.ToString() + "</p></div></div></div>")

            infoDiv.InnerHtml = html.ToString()


            Dim divHtml As StringBuilder = New StringBuilder()


            'divHtml.Append("<div style='cursor:pointer;' class='mx-2 info_poptop'>")
            'If (model.comment Is Nothing Or model.comment = "") Then
            '    divHtml.Append("<i id='utilityCurveComment' class='far fa-comments' onclick='showNode(5)'  aria-hidden='true'></i>")
            'Else
            '    divHtml.Append("<i id='utilityCurveComment' Class='fa fa-comments' title='" + model.comment + "' onclick='showNode(5)'  aria-hidden='true'></i>")
            'End If

            'divHtml.Append($"<div  class='info_tooltip right_tooltip hideshowinfo'  id='parentnode5' style='display:none;' ><div class='d-flex justify-content-between'><span id='commentSpan'>Add your comment</span>")
            'divHtml.Append($"<div class='action_icons' ><a href='#' onclick='toggleBox()'><i class='fa fa-times' onclick='hideNode(5)' aria-hidden='true'></i></a></div></div>")
            'divHtml.Append($"<textarea{If(model.isPipeViewOnly, " style='pointer-events:none;'", "")} id='commentBox1' class='form-control mb-3 mt-2 w-100' rows='3'>" + model.comment + "</textarea><div class='comt_btn text-end'>")
            'divHtml.Append($"<button{If(model.isPipeViewOnly, " class='d-none'", "")} type='button' class='py-1 px-2' onclick='save_utility_curve(8,null,1," + ChrW(&H22) + UCType + ChrW(&H22) + ")'> <i class='fa fa-check' aria-hidden='true'></i> OK</button>")
            'divHtml.Append("</div></div></div>")
            'infoScale.InnerHtml = divHtml.ToString()
            'infoScale1.InnerHtml = divHtml.ToString()

            'divHtml = New StringBuilder()
            Dim NoInfoDocRowData As String = ""
            NoInfoDocDataCls = ""
            If model.first_node_info.ToString().Trim() = "" And model.wrt_first_node_info.ToString().Trim() = "" Then
                NoInfoDocRowData = "NoInfDocDataRow"
                NoInfoDocDataCls = "NoInfoData"
            End If
            If Not (screenCheck.isMobileBrowserClient(Request)) Then
                divHtml.Append($"<div Class='active_table_row row_wrapper progress_table_row'>
<div class='tooltips_group w-100 order-0'><div class='info_tooltip_main position-relative {NoInfoDocRowData} {NoInfoDocDataCls}'><div class='comment_div' id='comment_div_0'><div class='information_icon'><a href='javascript:void(0);'><img src='../../img/icon/info_data.svg' id='comment_icon_0' aria-hidden='true' style='filter: grayscale(1);'></a></div></div></div><div class='info_tooltip_main position-relative'><div class='comment_icon'><a href='javascript:void(0);' onclick='updateRightSideComment('0','comment','open',event)' aria-hidden='true'> <img src='../../img/icon/Grey-plus-icon.svg' id='ImgComment_iconEmpty_5' title='' aria-hidden='true' onclick=ShowUtiliCurveCommentTxt()> <img src='../../img/icon/comment-svgrepo-com.svg' style='display:none;' id='ImgComment_icon_5' title='' aria-hidden='true' onclick=ShowUtiliCurveCommentTxt()></a><div class='tooltip_wrapper hideshowinfo' id='comment_div_box_0' style='display: none;'><div class='d-flex justify-content-between tooltip-header' onclick='updateRightSideComment('0','comment','open',event)'><span>Add your comment</span><div class='action_icons' onclick='HideMultiDerectCommentBox(0)'><a href='javascript:void(0);'><i class='fa fa-times' aria-hidden='true'></i></a></div></div><textarea class='form-control mb-3 mt-2 w-100' placeholder='Add your comment' id='txtRightComment_0' rows='3'></textarea><div class='comt_btn text-end'><button type='button' onclick='save_utility_curve(8,null,1," + ChrW(&H22) + UCType + ChrW(&H22) + ")'><i class='fa fa-check' aria-hidden='true'></i> OK</button></div></div></div></div><div class='brand_name'>")
            Else
                divHtml.Append($"<div Class='active_table_row row_wrapper progress_table_row'>
<div class='tooltips_group w-100 order-0'><div class='info_tooltip_main position-relative {NoInfoDocRowData} {NoInfoDocDataCls}'><div class='comment_div' id='comment_div_0'><div class='information_icon'><a href='javascript:void(0);'><img src='../../img/icon/info_data.svg' id='comment_icon_0' aria-hidden='true' style='filter: grayscale(1);'></a></div></div></div><div class='info_tooltip_main position-relative'><div class='comment_icon'><a href='javascript:void(0);' onclick='updateRightSideCommentMobile(0,'comment','open',event)' aria-hidden='true'> <img src='../../img/icon/Grey-plus-icon.svg' id='ImgComment_iconEmpty_5' title='' aria-hidden='true' onclick=ShowUtiliCurveCommentTxtMobile()> <img src='../../img/icon/comment-svgrepo-com.svg' style='display:none;' id='ImgComment_icon_5' title='' aria-hidden='true' onclick=ShowUtiliCurveCommentTxtMobile()></a><div class='tooltip_wrapper hideshowinfo' id='comment_div_box_0' style='display: none;'><div class='d-flex justify-content-between tooltip-header' onclick='updateRightSideComment('0','comment','open',event)'><span>Add your comment</span><div class='action_icons' onclick='HideMultiDerectCommentBox(0)'><a href='javascript:void(0);'><i class='fa fa-times' aria-hidden='true'></i></a></div></div><textarea class='form-control mb-3 mt-2 w-100' placeholder='Add your comment' id='txtRightComment_0' rows='3'></textarea><div class='comt_btn text-end'><button type='button' onclick='save_utility_curve(8,null,1," + ChrW(&H22) + UCType + ChrW(&H22) + ")'><i class='fa fa-check' aria-hidden='true'></i> OK</button></div></div></div></div><div class='brand_name'>")
            End If
            If Not (screenCheck.isMobileBrowserClient(Request)) Then
                divHtml.Append($"<span>{model.child_node}</span></div><div class='tooltip_wrapper hideshowinfo right-tooltip' id='tooltip_wrapper_0' style='display: none;'><div class='tooltip-header'><span>{model.child_node}</span><div class='action_icons'><a href='javascript:void(0);'><i class='bx bxs-edit-alt chkEvaluator d-none' onclick='showInfoPopup(decodeURIComponent(&quot;alternative%201&quot;),'1','2','0','1',null,'left-node',decodeURIComponent(&quot;&quot;),0)'></i></a><a href='javascript:void(0);'><i class='fa fa-times' aria-hidden='true' onclick='updateRightSideComment('0','tooltip','close',event)''></i></a></div><div class='tooltip_wrapper hideshowinfo' id='comment_div_box_0' style='display: none;'><div class='d-flex justify-content-between tooltip-header'><span>Add your comment</span><div class='action_icons' onclick='updateRightSideComment('0','comment','close',event)'><a href='javascript:void(0);'><i class='fa fa-times' aria-hidden='true'></i></a></div></div><textarea class='form-control mb-3 mt-2 w-100' placeholder='Add your comment' id='txtRightComment_0' rows='3'></textarea><div class='comt_btn text-end'><button type='button' onclick='updateRightSideComment('0','comment','close',event)'><i class='fa fa-check' aria-hidden='true'></i> OK</button></div></div></div><p class='border-top font-size-12 pt-2 text-start mb-0'></p></div></div> <hr class='w-100 {NoInfoDocRowData} {NoInfoDocDataCls}'>")
            Else
                divHtml.Append($"<span>{model.child_node}</span></div><div class='tooltip_wrapper hideshowinfo right-tooltip' id='tooltip_wrapper_0' style='display: none;'><div class='tooltip-header'><span>{model.child_node}</span><div class='action_icons'><a href='javascript:void(0);'><i class='bx bxs-edit-alt chkEvaluator d-none' onclick='showInfoPopup(decodeURIComponent(&quot;alternative%201&quot;),'1','2','0','1',null,'left-node',decodeURIComponent(&quot;&quot;),0)'></i></a><a href='javascript:void(0);'><i class='fa fa-times' aria-hidden='true' onclick='updateRightSideCommentMobile('0','tooltip','close',event)'></i></a></div><div class='tooltip_wrapper hideshowinfo' id='comment_div_box_0' style='display: none;'><div class='d-flex justify-content-between tooltip-header'><span>Add your comment</span><div class='action_icons' onclick='updateRightSideComment('0','comment','close',event)'><a href='javascript:void(0);'><i class='fa fa-times' aria-hidden='true'></i></a></div></div><textarea class='form-control mb-3 mt-2 w-100' placeholder='Add your comment' id='txtRightComment_0' rows='3'></textarea><div class='comt_btn text-end'><button type='button' onclick='updateRightSideComment('0','comment','close',event)'><i class='fa fa-check' aria-hidden='true'></i> OK</button></div></div></div><p class='border-top font-size-12 pt-2 text-start mb-0'></p></div></div> <hr class='w-100 {NoInfoDocRowData} {NoInfoDocDataCls}'>")
            End If
            'divHtml.Append($"<h2 Class='accordion-header position-relative'>")
            '    divHtml.Append($"<a href ='#' onclick='showInfoPopup(decodeURIComponent(" + ChrW(&H22) + firstnodeinfo + ChrW(&H22) + ")" + ",0,2,8,1,0,null, decodeURIComponent(" + ChrW(&H22) + model.child_node + ChrW(&H22) + ")" + ")' class='accordian_edit_icon  d-none d-md-block'> <img class='chkEvaluatorone' src='../../img/icon/edit-icon.svg'> </a>")
            '    divHtml.Append($" <div class='accordion-button' type='button' data-bs-toggle='collapse' data-bs-target='#faq-content-1' aria-expanded='true'>")
            '    divHtml.Append($" <div class=' me-4'>")
            'divHtml.Append($"<a id='lnkUtilityInfoDoc'><img src='../../img/icon/info_data.svg'></a>")
            'divHtml.Append($"</div>{model.first_node}{model.child_node} </div></h2>")
            divHtml.Append($"<div class='w-100 {NoInfoDocRowData} {NoInfoDocDataCls}'>")
                divHtml.Append($"<div class='accordion-body'><div class=''>")
                divHtml.Append($"<div class='open_tooltip_info'>")
                divHtml.Append("<div class='body_content'><div class='split_content scale_infodoc split_content scale_infodoc multirating_infodoc'>")
                If (model.first_node_info <> "") Then
                    divHtml.Append("<div Class='normal_content'><div class='removeptagd infodata_maxht'>")
                    divHtml.Append($"<p>{model.first_node_info.ToString()} </p>")
                Else
                    divHtml.Append("<div Class='normal_content NoInfoData'><div class='removeptagd infodata_maxht'>")
                    divHtml.Append("<h2 class='nodata_level'>No Data</h2>")
                End If
                divHtml.Append("</div></div>")
                If (model.wrt_first_node_info <> "") Then
                    divHtml.Append($"<div Class='wrt_right_content'><h3>WITH RESPECT TO <span>{model.parent_node}</span></h3><div class='removeptagw'>")
                    divHtml.Append($"<p>{model.wrt_first_node_info.ToString()} </p>")
                Else
                    divHtml.Append($"<div Class='wrt_right_content NoInfoData'><h3>WITH RESPECT TO <span>{model.parent_node}</span></h3><div class='removeptagw'>")
                    divHtml.Append("<h2 class='nodata_level'>No Data</h2>")
                End If
                divHtml.Append("</div></div>")
                divHtml.Append($"<a class='fullscreen_link' onclick='SetExpendedPopupElement(decodeURIComponent(" + ChrW(&H22) + firstnodeinfo + ChrW(&H22) + ")" + ",0,2,8,1,0,null, decodeURIComponent(" + ChrW(&H22) + model.child_node + ChrW(&H22) + "),true,null, " + "decodeURIComponent(" + ChrW(&H22) + wrttext + ChrW(&H22) + ")" + ",0,3,8,1,0, " + ChrW(&H22) + "parent-node1" + ChrW(&H22) + ", decodeURIComponent(" + ChrW(&H22) + wrtfirstnodeinfo + ChrW(&H22) + ")" + ");return false' href='#' data-bs-toggle='modal' data-bs-target='#exampleModal'><img src='../../img/icon/full-screen-svgrepo-com.svg'></a>")
                divHtml.Append("</div></div>")
                'divHtml.Append($"<div class='body_content removep1 info_text'><span>{ model.first_node_info.ToString().Trim()} </span></div><div class='info_datafooter d-block'>  <button  onclick='SetExpendedPopupElement(decodeURIComponent(" + ChrW(&H22) + firstnodeinfo + ChrW(&H22) + ")" + ",0,2,8,1,0,null, decodeURIComponent(" + ChrW(&H22) + model.child_node + ChrW(&H22) + "),true,null, " + "decodeURIComponent(" + ChrW(&H22) + wrttext + ChrW(&H22) + ")" + ",0,3,8,1,0, " + ChrW(&H22) + "parent-node1" + ChrW(&H22) + ", decodeURIComponent(" + ChrW(&H22) + wrtfirstnodeinfo + ChrW(&H22) + ")" + ");return false'  data-bs-toggle='modal' data-bs-target='#exampleModal'>with respect To <img src='../../img/icon/button_arrow.svg'></button></div>")
                divHtml.Append("</div></div></div></div> </div>")
                divHtml.Append($"<div Class='accordion-item d-none' {If(model.show_comments, " ", " style='display: none;'")} > <h2 Class='accordion-header'> <Button onclick ='ShowUtiliCurveCommentTxt()' Class='accordion-button collapsed' type='button' data-bs-toggle='collapse' data-bs-target='#faq-content-4'>")
                divHtml.Append($"<div Class='comment_icon me-4 d-none'>")
                If model.comment Is Nothing Or model.comment = "" Then
                    divHtml.Append($"<img src = '../../img/icon/Grey-plus-icon.svg' id='ImgComment_iconEmpty_5' title='' aria-hidden='true' onclick='updateRightSideComment('0','showcomment','open',event)'>  <img src = '../../img/icon/comment-svgrepo-com.svg' style='display:none;' id='ImgComment_icon_5' title='' aria-hidden='true'>")
                Else
                    divHtml.Append($"<img src = '../../img/icon/Grey-plus-icon.svg' id='ImgComment_iconEmpty_5' title='' style='display:none;' aria-hidden='true'>  <img src = '../../img/icon/comment-svgrepo-com.svg'  id='ImgComment_icon_5' title='{model.comment}' aria-hidden='true' onclick='updateRightSideComment('0','showcomment','open',event)'>")
                End If
                divHtml.Append("</div> Comments</button></h2>")
                divHtml.Append($"<div id ='faq-content-4' class='accordion-collapse collapse' data-bs-parent='#faqlist'>  <div class='accordion-body'><div class='toggle_info'><div class='workment_comment_tooltip'>")
                divHtml.Append($"<div class='d-flex justify-content-between'> <span>Add your comment</span><div class='action_icons'>")
                divHtml.Append($"</div></div> <textarea class='form-control mb-3 mt-2 w-100' rows='3' id='commentBox' placeholder='Add your comment'> { model.comment}</textarea>")

                divHtml.Append($"<div class='comt_btn text-end'> <button type='button' data-bs-toggle='collapse' data-bs-target='#faq-content-4' aria-expanded='true' onclick='save_utility_curve(8,null,1," + ChrW(&H22) + UCType + ChrW(&H22) + ")'><i class='fa fa-check' aria-hidden='true'></i> OK</button> </div> </div> </div></div> </div> </div>")
                faqlist.InnerHtml = divHtml.ToString()

                divHtml = New StringBuilder()

                'divHtml.Append($"<p><span>{model.parent_node}</span></p>")
                'divHtml.Append($"<div class='heading_info'> <img src='../../img/icon/empty_info.svg' class='d-none'><img src='../../img/icon/info_data.svg'><img src='../../img/icon/info-close.svg' class='d-none'></div>")
                Dim IsHideHeaderInfo As Boolean = False
                'If model.parent_node_info Is Nothing Or model.parent_node_info = "" Then
                '    divHtml.Append($"<div class='heading_info' id='HeaderDivIcon'><a href ='javascript:void(0);'><img  id='img1' src='../../img/icon/empty_info.svg'><img id='img2' src='../../img/icon/info_data.svg' style='display: none;'><img id='img3' src='../../img/icon/info-close.svg' style='display: none;'></a></div>")
                '    IsHideHeaderInfo = True
                'Else
                '    divHtml.Append($"<div class='heading_info' id='HeaderDivIcon'><a href ='javascript:void(0);'><img  id='img1' src='../../img/icon/empty_info.svg' style='display: none;'><img id='img2' src='../../img/icon/info_data.svg'><img id='img3' src='../../img/icon/info-close.svg' style='display: none;'></a></div>")
                'End If
                Dim model_text1 As String = HttpUtility.UrlEncode(model.cluster_phrase.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27)))
                divHtml.Append($"<div class='page-title-box '> <div Class='heading_icons utilityInfo'><a{If(model.isPipeViewOnly, " class='d-none'", "")} class='editsvg_icon' href='#'><img class='chkEvaluatorone' src = '../../img/icon/edit-icon.svg' onclick='showHeaderPopup(decodeURIComponent(" + ChrW(&H22) + model_text1 + ChrW(&H22) + ")" + ",0,0,8,0,0,null, decodeURIComponent(" + ChrW(&H22) + parentnodeinfo + ChrW(&H22) + ")" + ")'> </a> <asp:Label ID='lblTask' runat='server' ><span id='divT2S' style='font-size:20px; margin-left:1ex border: 0px !important; background: #f0f0ff08 !important; display:none'></span></label></div>")
            divHtml.Append($"<div Class='txtStepTask mb-0  removep2'  id='MainHeaderInfodoc'>{model.step_task}<span class='threedoticon d-none' id='btnheadinfo'> <a href='#' onclick='showheaderpopup()' ><div class='snippet'><div class='stage'><div class='dot-falling'></div> </div></div></a></span></div></div>")
            divutc.InnerHtml = divHtml.ToString()

                'UtDiv.InnerHtml = divHtml.ToString()
                divHtml = New StringBuilder()
                divHtml.Append($"<div class='info_content d-flex mt-lg-0 mt-2' id='Header_InfoDocs'{If(IsHideHeaderInfo, " style='display: none;'", "")}><div class='heading_icons dcsdcsd heading_info me-0'><div  id='HeaderDivIcon'><a href='#'><img  id='img1' src='../../img/icon/empty_info.svg' style='display: none;'><img id='img2' src='../../img/icon/info_data.svg'><img id='img3' src='../../img/icon/info-close.svg' style='display: none;'></a></div><div class='infodoc_icons'><a class='editsvg_icon' href='javascript:void(0);' onclick=showInfoPopup(decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.parent_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}),'0','-1','0','-1',null,'parent-node',decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.parent_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}))><img class='chkEvaluatorone' src='../../img/icon/edit-icon.svg'></a><a href='#' onclick=Expandtxt(decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.parent_node.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)}),'0','-1','0','-1',null,'parent-node',decodeURIComponent({ChrW(&H22) + HttpUtility.UrlEncode(model.parent_node_info.ToString().Trim().Replace(ChrW(&H22), ChrW(&H27))) + ChrW(&H22)})) data-bs-toggle='modal' data-bs-target='#infodocPop'><img src='../../img/icon/full-screen-svgrepo-com.svg' id='ibtnFullscreen'></a></div></div><div class='info_content_wrapper'> <div class='info_text removep'>{model.parent_node_info.ToString()}</div>")
                divHtml.Append($"<div class='info_box_icons d-none'>")
                divHtml.Append($"<a{If(model.isPipeViewOnly, " class='d-none'", "")} href ='#' class='editsvg_icon'><img class='chkEvaluatorone' src='../../img/icon/edit-icon.svg' onclick='showInfoPopup(decodeURIComponent(" + ChrW(&H22) + parentnodeinfo + ChrW(&H22) + ")" + ",0,-1,8,-1,0, " + ChrW(&H22) + "parent-node1" + ChrW(&H22) + ", decodeURIComponent(" + ChrW(&H22) + model.parent_node + ChrW(&H22) + ")" + ")'></a>")
                divHtml.Append($"<a href='#' data-bs-toggle='modal' data-bs-target='#infodocPop' onclick='Expandtxt(decodeURIComponent(" + ChrW(&H22) + model.parent_node + ChrW(&H22) + ")" + ",0,-1,8,-1,0, " + ChrW(&H22) + "parent-node1" + ChrW(&H22) + ", decodeURIComponent(" + ChrW(&H22) + parentnodeinfo + ChrW(&H22) + ")" + ")' ><img src='../../img/icon/full-screen-svgrepo-com.svg'></a>")
                'divHtml.Append($"<a href='#' data-bs-toggle='modal' data-bs-target='#exampleModal' onclick='SetExpendedPopupElement(decodeURIComponent(" + ChrW(&H22) + parentnodeinfo + ChrW(&H22) + ")" + ",0,-1,8,-1,0, " + ChrW(&H22) + "parent-node1" + ChrW(&H22) + ", decodeURIComponent(" + ChrW(&H22) + model.parent_node + ChrW(&H22) + ")" + ")'><img src='../../img/icon/full-screen-svgrepo-com.svg'></a>")
                divHtml.Append($"</div></div></div>")
                HeaderInfoRigh.InnerHtml = divHtml.ToString()

                'divHtml = New StringBuilder()
                'divHtml.Append($"<div><p>{model.parent_node_info.ToString()}</p></div>")
                'stepTaskDiv.InnerHtml = divHtml.ToString()

                'divHtml = New StringBuilder()
                'divHtml.Append($"<a{If(model.isPipeViewOnly, " class='d-none'", "")} href ='#' class='editsvg_icon'><img class='vwEval' src='../../img/icon/edit-icon.svg' onclick='showInfoPopup(decodeURIComponent(" + ChrW(&H22) + parentnodeinfo + ChrW(&H22) + ")" + ",0,-1,8,-1,0, " + ChrW(&H22) + "parent-node1" + ChrW(&H22) + ", decodeURIComponent(" + ChrW(&H22) + model.parent_node + ChrW(&H22) + ")" + ")'></a>")
                'divHtml.Append($"<a href='#' data-bs-toggle='modal' data-bs-target='#infodocPop' onclick='Expandtxt(decodeURIComponent(" + ChrW(&H22) + model.parent_node + ChrW(&H22) + ")" + ",0,-1,8,-1,0, " + ChrW(&H22) + "parent-node1" + ChrW(&H22) + ", decodeURIComponent(" + ChrW(&H22) + parentnodeinfo + ChrW(&H22) + ")" + ")' ><img src='../../img/icon/full-screen-svgrepo-com.svg'></a>")
                'BtnDiv.InnerHtml = divHtml.ToString()
            End If
    End Sub
    Private Sub WtiteHtml()

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