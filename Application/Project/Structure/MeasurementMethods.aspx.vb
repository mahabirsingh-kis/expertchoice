Imports System.Web.Script.Serialization

Partial Class MeasurementMethodsPage
    Inherits clsComparionCorePage

    Public Enum MeasurementMethodsPageTypes
        mmAll = 0
        mmObj = 1
        mmAlt = 2
    End Enum

    Public Const OPT_ALLOW_STRUCTURAL_ADJUST As Boolean = False

    ' actions
    Public Const ACTION_VIEW As String = "view" ' All / Alts / Objs
    Public Const ACTION_MEASURE_TYPE As String = "mt"
    Public Const ACTION_MEASURE_SCALE As String = "ms"
    Public Const ACTION_NUM_COMPARISONS As String = "num_comparisons"
    Public Const ACTION_PAIRWISE_TYPE As String = "pairwise_type"
    Public Const ACTION_DISPLAY_OPTION As String = "display_option"
    Public Const ACTION_FORCE_GRAPHICAL As String = "force_graphical"
    Public Const ACTION_SET_MEASURE_TYPE_FOR_ALL As String = "mt_for_all"
    Public Const ACTION_SET_NODE_PROPERTY As String = "set_node_prop"
    Public Const ACTION_GET_STAT_DATA As String = "get_stat_data"
    Public Const ACTION_SAVE_STAT_DATA As String = "save_stat_data"

    Public Const PagePrefix As String = "Measurement Methods (HMTL)"
    Public NoSourcesGuid As Guid = New Guid("{1D3AF700-27D3-4A3B-825C-703A28364E67}")
    Public NoSourcesID As Integer = Integer.MaxValue

    Private Const ChangeMeasurementScaleNameWhenPasting As Boolean = False

    ' D3731 ===
    Private Function GetScaleTypeName(tScalteType As ECMeasureType) As String
        Return ResString(String.Format("lbl_{0}", tScalteType.ToString))
    End Function
    ' D3731 ==

    Public Sub New()
        MyBase.New(_PGID_STRUCTURE_MEASUREMENT_METHODS)
    End Sub

    Protected Sub Page_PreLoad(sender As Object, e As EventArgs) Handles Me.PreLoad
        Dim iVM As Integer = CInt(MTMode)

        Dim sAction = EcSanitizer.GetSafeHtmlFragment(CheckVar(_PARAM_ACTION, "")).Trim.ToLower   ' Anti-XSS
        Select Case sAction
            Case ACTION_VIEW ' MTMode = all/obj/alt
                Dim VM As Integer = CheckVar("value", iVM)
                If VM <> iVM Then MTMode = CType(VM, MeasurementMethodsPageTypes)
        End Select

        Dim sView = EcSanitizer.GetSafeHtmlFragment(CheckVar("view", "")).Trim.ToLower    ' Anti-XSS
        Select Case sView
            Case "all"
                If iVM <> MeasurementMethodsPageTypes.mmAll Then MTMode = MeasurementMethodsPageTypes.mmAll
                CurrentPageID = _PGID_STRUCTURE_MEASUREMENT_METHODS
            Case "obj"
                If iVM <> MeasurementMethodsPageTypes.mmObj Then MTMode = MeasurementMethodsPageTypes.mmObj
                CurrentPageID = _PGID_STRUCTURE_MEASUREMENT_METHODS_OBJS
            Case "alt"
                If iVM <> MeasurementMethodsPageTypes.mmAlt Then MTMode = MeasurementMethodsPageTypes.mmAlt
                CurrentPageID = _PGID_STRUCTURE_MEASUREMENT_METHODS_ALTS
        End Select

        Ajax_Callback(RemoveXssFromUrl(Request.Form.ToString)) ' D6767
    End Sub

    Public Property MTMode As MeasurementMethodsPageTypes
        Get
            Return CType(CInt(PM.Attributes.GetAttributeValue(ATTRIBUTE_MEASUREMENT_METHODS_MODE_ID, UNDEFINED_USER_ID)), MeasurementMethodsPageTypes)
        End Get
        Set(value As MeasurementMethodsPageTypes)
            WriteSetting(PRJ, ATTRIBUTE_MEASUREMENT_METHODS_MODE_ID, AttributeValueTypes.avtLong, CInt(value))
        End Set
    End Property

    Public Sub SaveNodeStatisticalData(NodeID As Guid, Data As List(Of clsStatisticalDataItem))
        Dim serializer As New JavaScriptSerializer
        Dim sData As String = serializer.Serialize(Data)
        WriteSetting(PRJ, ATTRIBUTE_RISK_NODE_STATICTICAL_DATA_ID, AttributeValueTypes.avtString, sData, NodeID, PagePrefix + ": Edit statistical data", "NodeId: " + NodeID.ToString)
    End Sub

    Public Function GetNodeStatData(node As clsNode) As String
        Dim retVal As String = ""
        Dim tData As List(Of clsStatisticalDataItem) = node.StatisticalData
        If tData IsNot Nothing AndAlso tData.Count > 0 Then 
            For Each item As clsStatisticalDataItem In tData
                retVal += CStr(IIf(retVal = "", "", ",")) + String.Format("['{0}',{1}]", JS_SafeString(item.TimePeriodName), JS_SafeNumber(item.Data))
            Next
        End If
        Return string.Format("[{0}]", retVal)
    End Function

    Public Function GetMeasureScalesData() As String
        Dim retVal As String = ""

        If App.isRiskEnabled Then
            ' Riskion Scales
            GetMeasureScalesDataByType(retVal, ECMeasureType.mtRatings)
            GetMeasureScalesDataByType(retVal, ECMeasureType.mtStep)
            GetMeasureScalesDataByType(retVal, ECMeasureType.mtRegularUtilityCurve)
            GetMeasureScalesDataByType(retVal, ECMeasureType.mtRatings, RatingScaleType.rsOutcomes)
        Else
            ' Comparion Scales
            GetMeasureScalesDataByType(retVal, ECMeasureType.mtRatings)
            GetMeasureScalesDataByType(retVal, ECMeasureType.mtRegularUtilityCurve)
            GetMeasureScalesDataByType(retVal, ECMeasureType.mtStep)
        End If

        Return String.Format("[{0}]", retVal)
    End Function

    Private Function IsScaleVisible(scaleType As ScaleType) As Boolean
        'Return Not PM.IsRiskProject OrElse (PM.ActiveHierarchy = ECHierarchyID.hidLikelihood AndAlso scaleType = ScaleType.stLikelihood) OrElse (PM.ActiveHierarchy = ECHierarchyID.hidImpact AndAlso scaleType = ScaleType.stImpact)
        Return scaleType = ScaleType.stShared OrElse (PM.ActiveHierarchy = ECHierarchyID.hidLikelihood AndAlso scaleType = ScaleType.stLikelihood) OrElse (PM.ActiveHierarchy = ECHierarchyID.hidImpact AndAlso scaleType = ScaleType.stImpact)
    End Function

    Private Sub GetMeasureScalesDataByType(ByRef retVal As String, sType As ECMeasureType, Optional sRatingScaleType As RatingScaleType = RatingScaleType.rsRegular)
        For Each Scale As clsMeasurementScale In PM.MeasureScales.AllScales
            Dim MT As Integer = CInt(ECMeasureType.mtNone)
            Dim scaleType As ScaleType = PM.MeasureScales.GetScaleType(Scale)
            If TypeOf (Scale) Is clsRatingScale Then MT = CInt(ECMeasureType.mtRatings)
            If TypeOf (Scale) Is clsStepFunction Then MT = CInt(ECMeasureType.mtStep)
            If TypeOf (Scale) Is clsRegularUtilityCurve Then MT = CInt(ECMeasureType.mtRegularUtilityCurve)
            Dim RST As RatingScaleType = PM.MeasureScales.GetRatingScaleType(Scale)
            If MT = sType AndAlso RST = sRatingScaleType AndAlso IsScaleVisible(scaleType) Then
                MT = CInt(ECMeasureType.mtPWOutcomes)
                retVal += CStr(IIf(retVal = "", "", ",")) + GetMeasureScaleData(Scale, ECMeasureType.mtPWOutcomes, sRatingScaleType)
            End If
        Next
    End Sub

    Private Function GetMeasureScaleData(Scale As clsMeasurementScale, sType As ECMeasureType, Optional sRatingScaleType As RatingScaleType = RatingScaleType.rsRegular) As String
        If Scale Is Nothing Then Return "[]"

        Dim MT As Integer = CInt(ECMeasureType.mtNone)
        Dim scaleType As ScaleType = PM.MeasureScales.GetScaleType(Scale)
        If TypeOf (Scale) Is clsRatingScale Then MT = CInt(ECMeasureType.mtRatings)
        If TypeOf (Scale) Is clsStepFunction Then MT = CInt(ECMeasureType.mtStep)
        If TypeOf (Scale) Is clsRegularUtilityCurve Then MT = CInt(ECMeasureType.mtRegularUtilityCurve)
        Dim RST As RatingScaleType = PM.MeasureScales.GetRatingScaleType(Scale)
        Dim sIntensities As String = ""
        Dim sMT As String = JS_SafeString("None")
        Select Case MT
            Case ECMeasureType.mtRatings
                sMT = JS_SafeString("Ratings")
                Select Case RST
                    Case RatingScaleType.rsExpectedValues
                        sMT = JS_SafeString("Expected Values")
                    Case RatingScaleType.rsOutcomes
                        MT = ECMeasureType.mtPWOutcomes
                        sMT = JS_SafeString("Pairwise Of Probabilities")
                    Case RatingScaleType.rsPWOfPercentages
                        MT = ECMeasureType.mtPWAnalogous
                        sMT = JS_SafeString("Pairwise Of Percentages")
                End Select
                Dim rs As clsRatingScale = CType(Scale, clsRatingScale)
                For Each intensity As clsRating In rs.RatingSet
                    sIntensities += CStr(IIf(sIntensities = "", "", ",")) + String.Format("['{0}','{1}',{2},'{3}']", intensity.GuidID, JS_SafeString(intensity.Name), JS_SafeNumber(intensity.Value), JS_SafeString(intensity.Comment))
                Next
            Case ECMeasureType.mtStep
                sMT = JS_SafeString("Step Function")
                Dim sf As clsStepFunction = CType(Scale, clsStepFunction)
                For Each intv As clsStepInterval In sf.Intervals
                    sIntensities += CStr(IIf(sIntensities = "", "", ",")) + String.Format("['{0}','{1}',{2},{3},{4},'{5}']", intv.ID, JS_SafeString(intv.Name), JS_SafeNumber(intv.Low), JS_SafeNumber(intv.High), JS_SafeNumber(intv.Value), JS_SafeString(intv.Comment))
                Next
            Case ECMeasureType.mtRegularUtilityCurve
                sMT = JS_SafeString("Utility Curve")
                Dim uc As clsRegularUtilityCurve = CType(Scale, clsRegularUtilityCurve)
                sIntensities = CStr(IIf(sIntensities = "", "", ",")) + String.Format("{0},{1},{2},{3},{4}", JS_SafeNumber(uc.Low), JS_SafeNumber(uc.High), JS_SafeNumber(uc.Curvature), Bool2JS(uc.IsIncreasing), Bool2JS(uc.IsLinear))
        End Select
        sIntensities = String.Format("[{0}]", sIntensities)

        Dim tApplications As Integer = 0

        For Each node As clsNode In PM.ActiveObjectives.Nodes
            'If node.MeasureType = sType AndAlso (node.MeasurementScaleID = Scale.ID OrElse node.FeedbackMeasurementScaleID = Scale.ID) Then tApplications += 1
            If (node.MeasureType = sType AndAlso node.MeasurementScaleID = Scale.ID) AndAlso (Not PM.IsRiskProject OrElse (Not node.RiskNodeType = RiskNodeType.ntCategory AndAlso Not node.AllChildrenCategories())) Then tApplications += 1
        Next

        If PM.IsRiskProject Then
            Dim noSources As List(Of clsNode) = PM.ActiveObjectives.GetUncontributedAlternatives
            If noSources.Count > 0 Then
                For Each node As clsNode In noSources
                    If node.MeasureType = sType AndAlso node.MeasurementScaleID = Scale.ID Then tApplications += 1
                Next
            End If
        End If

        'Return String.Format("['{0}',{1},'{2}','{3}','{4}',{5},{6},{7},'{8}',{9},{10}]", Scale.GuidID, MT, sMT, JS_SafeString(Scale.Name), JS_SafeString(Scale.Comment), CInt(RST), IIf(Scale.IsDefault, 1, 0), tApplications.ToString, "", sIntensities, If(MT = ECMeasureType.mtStep, Bool2JS(CType(Scale, clsStepFunction).IsPiecewiseLinear), If(PM.Parameters.RatingsUseDirectValue(Scale.GuidID), "1", "0")))
        Return String.Format("['{0}',{1},'{2}','{3}','{4}',{5},{6},{7},'{8}',{9},{10}]", Scale.GuidID, MT, sMT, JS_SafeString(Scale.Name), "", CInt(RST), IIf(Scale.IsDefault, 1, 0), tApplications.ToString, "", sIntensities, If(MT = ECMeasureType.mtStep, Bool2JS(CType(Scale, clsStepFunction).IsPiecewiseLinear), If(PM.Parameters.RatingsUseDirectValue(Scale.GuidID), "1", "0")))
    End Function

    Private Function IsScaleInUse(ScaleID As Integer) As String
        Dim sUses As String = ""
        For i As Integer = 0 To PM.ActiveObjectives.Nodes.Count - 1
            If PM.ActiveObjectives.Nodes(i).MeasurementScaleID = ScaleID Then
                If sUses.Length < 150 Then sUses += String.Format("{0}'{1}'", IIf(sUses = "", "", ", "), PM.ActiveObjectives.Nodes(i).NodeName) Else Return sUses + ", ..."
            End If
        Next
        Return sUses
    End Function

    Public Function GetDiagonalsEvaluationModeForNode(NodeID As Guid) As DiagonalsEvaluation
        Dim retVal As DiagonalsEvaluation = DiagonalsEvaluation.deAll
        retVal = CType(PM.Attributes.GetAttributeValue(ATTRIBUTE_EVALUATE_DIAGONALS_MODE_ID, NodeID), DiagonalsEvaluation)
        Return retVal
    End Function

    Private Function isLastChild(node As clsNode, Optional ParentNode As clsNode = Nothing) As Boolean
        Dim Parent As clsNode = If (ParentNode Is Nothing, node.ParentNode, ParentNode)
        Return Parent IsNot Nothing AndAlso Parent.Children.IndexOf(node) = Parent.Children.Count - 1
    End Function

    Private Function GetNodeConnectingLines(origItem As clsNode, Optional ParentNode As clsNode = Nothing) As String
        Dim retVal As String = ""

        Dim item As clsNode = origItem
        Dim parent As clsNode = If(ParentNode Is Nothing, If(origItem.Tag isnot nothing AndAlso TypeOf(origItem.Tag) Is clsNode, Ctype(origItem.Tag, clsNode), origItem.ParentNode), ParentNode)

        While parent IsNot Nothing
            Dim isLast As Boolean = isLastChild(item, Parent)
            If isLast Then
                If item Is origItem Then
                    retVal += "L"
                Else
                    retVal += "_"
                End If
            Else
                If item Is origItem Then
                    retVal += "+"
                Else
                    retVal += "|"
                End If
            End If
            item = parent
            parent = parent.ParentNode
        End While

        Return retVal
    End Function

    Public Function GetNodeData(node As clsNode, ParentNode As clsNode) As String
        If node IsNot Nothing Then
            Dim mType As Integer = CInt(node.MeasureType)
            If node.AllChildrenCategories Then mType = -1
            Dim msGuid As String = ""
            Dim forceGraphicalOption As Boolean = If(PM.Attributes.IsValueSet(ATTRIBUTE_PAIRWISE_FORCE_GRAPHICAL_ID, node.NodeGuidID), CBool(PM.Attributes.GetAttributeValue(ATTRIBUTE_PAIRWISE_FORCE_GRAPHICAL_ID, node.NodeGuidID)), If(node.IsTerminalNode, PM.PipeBuilder.PipeParameters.ForceGraphicalForAlternatives, PM.PipeBuilder.PipeParameters.ForceGraphical))
            Dim forceGraphicalOptionEnabled As Boolean = True
            If node.MeasurementScale IsNot Nothing AndAlso node.MeasureType <> ECMeasureType.mtDirect AndAlso node.MeasureType <> ECMeasureType.mtPairwise AndAlso mType >= 0 Then msGuid = node.MeasurementScale.GuidID.ToString
            Dim nodeNumComparisons As DiagonalsEvaluation = GetDiagonalsEvaluationModeForNode(node.NodeGuidID)
            Dim nodeElCount As String = node.GetElementsCountString()
            Dim nodeJudgCountTooltip As String = ""
            Dim nodeJudgCount As Integer = 0
            Dim nodeJudgCountString As String = node.GetJudgmentsCountString(nodeNumComparisons, nodeJudgCount, nodeJudgCountTooltip)
            Dim nodeDisplayOption As Integer = 0
            If node.MeasureType = ECMeasureType.mtPairwise OrElse node.MeasureType = ECMeasureType.mtPWOutcomes OrElse node.MeasureType = ECMeasureType.mtPWAnalogous Then
                'pairwise
                nodeDisplayOption = CInt(PM.Attributes.GetAttributeValue(ATTRIBUTE_DISPLAY_OPTION_PAIRWISE_ID, node.NodeGuidID))
            Else
                'ratings
                nodeDisplayOption = CInt(PM.Attributes.GetAttributeValue(ATTRIBUTE_DISPLAY_OPTION_RATINGS_ID, node.NodeGuidID))
            End If

            Dim nodeKnownLikelihood As String = ""
            If PRJ.IsRisk AndAlso node.MeasureType = ECMeasureType.mtPWAnalogous Then
                Dim kl As List(Of KnownLikelihoodDataContract) = node.GetKnownLikelihoods()
                For Each item As KnownLikelihoodDataContract In kl
                    If item.Value > 0 Then
                        nodeKnownLikelihood = String.Format("{0} : {1}", item.NodeName, Math.Round(item.Value, 4))
                    End If
                Next
            End If

            Dim nodePairwiseType As PairwiseType = PairwiseType.ptVerbal
            If PM.Attributes.IsValueSet(ATTRIBUTE_PAIRWISE_TYPE_ID, node.NodeGuidID) Then
                nodePairwiseType = CType(PM.Attributes.GetAttributeValue(ATTRIBUTE_PAIRWISE_TYPE_ID, node.NodeGuidID), PairwiseType)
                forceGraphicalOptionEnabled = False
            Else
                If node.IsTerminalNode Then nodePairwiseType = PM.PipeParameters.PairwiseTypeForAlternatives Else nodePairwiseType = PM.PipeParameters.PairwiseType
            End If

            ' connecting lines
            Dim connecting_lines As String = GetNodeConnectingLines(node, ParentNode)

            ' expansion state
            Dim isExpanded As Integer = CInt(IIf(node.Children.Count > 0, 1, -1))

            Dim PipeStep As Integer = PM.PipeBuilder.GetFirstEvalPipeStepForNode(node, -1)

            Dim sStructAdjust As String = CStr(IIf(CBool(PM.Attributes.GetAttributeValue(ATTRIBUTE_IS_STRUCTURAL_ADJUST_NODE_ID, node.NodeGuidID)), 1, 0))
            Dim hasGrandChildren As Boolean = node.Children IsNot Nothing AndAlso node.Children.Count > 0 AndAlso node.Children.Where(Function(child) child.Children IsNot Nothing AndAlso child.Children.Count > 0).Count > 0
            If Not hasGrandChildren Then sStructAdjust = "-1"

            Return String.Format("{{""id"":{0},""pid"":{1},""exp"":{2},""lines"":""{3}"",""name"":""{4}"",""mt"":{5},""ms"":""{6}"",""action"":""{7}"",""el_count"":""{8}"",""judg_count"":{9},""s_judg_count"":""{10}"",""h_judg_count"":""{11}"",""num_comp"":{12},""disp_opt"":{13},""pw_type"":{14},""force_g"":{15},""is_term"":{16},""pipe_step"":{17},""structural"":{18},""node_type"":{19},""synth_type"":{20},""has_stat"":{21}, ""guid"":""{22}"",""fg_enabled"":{23},""s_kw"":""{24}"",""is_ns"":{25}}}", If(node.IsAlternative, AltID(node.NodeID), node.NodeID), If(ParentNode Is Nothing, "-1", ParentNode.NodeID.ToString), isExpanded, connecting_lines, JS_SafeString(node.NodeName), mType, msGuid, "action", nodeElCount, nodeJudgCount, "&nbsp;" + nodeJudgCountString, ParseString(nodeJudgCountTooltip), CInt(nodeNumComparisons), nodeDisplayOption, CInt(nodePairwiseType), CInt(IIf(forceGraphicalOption, 1, 0)), CInt(IIf(node.IsTerminalNode, 1, 0)), PipeStep, sStructAdjust, CInt(IIf(App.isRiskEnabled, node.RiskNodeType, -1)), CInt(PM.Attributes.GetAttributeValue(ATTRIBUTE_NODE_SYNTHESIS_TYPE_ID, node.NodeGuidID)), CInt(IIf(App.isRiskEnabled, CInt(IIf(node.StatisticalData IsNot Nothing AndAlso node.StatisticalData.Count > 0, 1, 0)), -1)), node.NodeGuidID.ToString, Bool2JS(forceGraphicalOptionEnabled), JS_SafeString(nodeKnownLikelihood), Bool2JS(node.IsAlternative))
        End If
        Return ""
    End Function

    Public Function GetNodesData() As String
        Dim retVal As String = ""

        Dim H As clsHierarchy = PM.ActiveObjectives
        If H IsNot Nothing Then
            H.SortNodesByLevels()
            PM.CreateHierarchyLevelValuesCH(H)
            PM.PipeBuilder.PipeCreated = False
            PM.PipeBuilder.CreatePipe()
            For Each t As Tuple(Of Integer, Integer, clsNode) In H.NodesInLinearOrder()
                t.Item3.Tag = Nothing
                retVal += CStr(IIf(retVal = "", "", ",")) + GetNodeData(t.Item3, H.GetNodeByID(t.Item2))
            Next
        End If

        If PM.IsRiskProject AndAlso PM.ActiveHierarchy = ECHierarchyID.hidLikelihood Then
            Dim NoSourcesNode As clsNode = New clsNode(PM.ActiveObjectives) With {.NodeName = ResString("lblTreeNodeNoSources"), .NodeGuidID = NoSourcesGuid, .NodeID = NoSourcesID}
            retVal += CStr(IIf(retVal = "", "", ",")) + GetNodeData(NoSourcesNode, Nothing)
            For Each alt As clsNode In PM.ActiveObjectives.GetUncontributedAlternatives
                alt.Tag = NoSourcesNode
                NoSourcesNode.Children.Add(alt)
            Next
            For Each alt As clsNode In PM.ActiveObjectives.GetUncontributedAlternatives
                retVal += CStr(IIf(retVal = "", "", ",")) + GetNodeData(alt, NoSourcesNode)
            Next
        End If

        Return String.Format("[{0}]", retVal)
    End Function

    Private Sub CopyNodeAttribute(AttributeID As Guid, SourceNodeID As Guid, TargetNodeID As Guid)
        Dim attr As clsAttribute = PM.Attributes.GetAttributeByID(AttributeID)
        If attr IsNot Nothing Then
            With PM
                If .Attributes.IsValueSet(AttributeID, SourceNodeID) Then
                    If attr.ValueType = AttributeValueTypes.avtLong
                        Dim SourceValue As Long = CLng(.Attributes.GetAttributeValue(AttributeID, SourceNodeID))
                        .Attributes.SetAttributeValue(AttributeID, UNDEFINED_USER_ID, attr.ValueType, SourceValue, TargetNodeID, Guid.Empty)
                    End If
                    If attr.ValueType = AttributeValueTypes.avtBoolean
                        Dim SourceValue As Boolean = CBool(.Attributes.GetAttributeValue(AttributeID, SourceNodeID))
                        .Attributes.SetAttributeValue(AttributeID, UNDEFINED_USER_ID, attr.ValueType, SourceValue, TargetNodeID, Guid.Empty)
                    End If
                    If attr.ValueType = AttributeValueTypes.avtDouble
                        Dim SourceValue As Double = CDbl(.Attributes.GetAttributeValue(AttributeID, SourceNodeID))
                        .Attributes.SetAttributeValue(AttributeID, UNDEFINED_USER_ID, attr.ValueType, SourceValue, TargetNodeID, Guid.Empty)
                    End If
                    If attr.ValueType = AttributeValueTypes.avtString
                        Dim SourceValue As String = CStr(.Attributes.GetAttributeValue(AttributeID, SourceNodeID))
                        .Attributes.SetAttributeValue(AttributeID, UNDEFINED_USER_ID, attr.ValueType, SourceValue, TargetNodeID, Guid.Empty)
                    End If
                Else
                    .Attributes.SetAttributeValue(AttributeID, UNDEFINED_USER_ID, AttributeValueTypes.avtLong, Nothing, TargetNodeID, Guid.Empty)
                End If
            End With
        End If
    End Sub

    Public Sub SetNodeMeasurementScale(NodeIDs As List(Of Guid), MeasurementType As ECMeasureType, ScaleID As Guid, SourceNodeID As Guid, DoCopyPairwiseSettings As Boolean, Optional fSaveChanges As Boolean = True, Optional isEventWithNoSource As Boolean = False)
        Dim tRes As Guid = Nothing

        Dim SourceKnownLikelihoods As List(Of KnownLikelihoodDataContract) = Nothing
        If MeasurementType = ECMeasureType.mtPWAnalogous Then
            SourceKnownLikelihoods = GetKnownLikelihoods(SourceNodeID)
        End If
        Dim tknownLikelihoodsChanged As Boolean = False

        With PM.StorageManager
            PM.Attributes.ReadAttributeValues(AttributesStorageType.astStreamsDatabase, .ProjectLocation, .ProviderType, .ModelID, UNDEFINED_USER_ID)
        End With
        Dim mtChaged As Boolean = False
        For Each nodeID As Guid In NodeIDs
            Dim node As clsNode = If(Not isEventWithNoSource, PM.ActiveObjectives.GetNodeByID(NodeId), PM.ActiveAlternatives.GetNodeByID(NodeId))

            If node IsNot Nothing Then
                node.MeasureType = MeasurementType
                mtChaged = True

                If node.MeasureType = ECMeasureType.mtRatings Then
                    If node.MeasurementScale IsNot Nothing Then
                        'ScaleID = node.MeasurementScale.GuidID
                    End If
                End If

                If SourceNodeID <> Guid.Empty Then
                    CopyNodeAttribute(ATTRIBUTE_PAIRWISE_FORCE_GRAPHICAL_ID, SourceNodeID, nodeID)
                    CopyNodeAttribute(ATTRIBUTE_PAIRWISE_TYPE_ID, SourceNodeID, nodeID)
                    CopyNodeAttribute(ATTRIBUTE_EVALUATE_DIAGONALS_MODE_ID, SourceNodeID, nodeID)
                    CopyNodeAttribute(ATTRIBUTE_DISPLAY_OPTION_PAIRWISE_ID, SourceNodeID, nodeID)
                    CopyNodeAttribute(ATTRIBUTE_DISPLAY_OPTION_RATINGS_ID, SourceNodeID, nodeID)
                End If

                node.ResetClusterPhrase()

                Select Case MeasurementType
                    Case ECMeasureType.mtRatings, ECMeasureType.mtPWOutcomes
                        Dim RatingScale As clsRatingScale = PM.ActiveObjectives.ProjectManager.MeasureScales.GetRatingScaleByID(ScaleID)
                        If RatingScale IsNot Nothing Then
                            node.RatingScaleID = RatingScale.ID
                        End If
                    Case ECMeasureType.mtStep
                        Dim StepFunction As clsStepFunction = PM.ActiveObjectives.ProjectManager.MeasureScales.GetStepFunctionByID(ScaleID)
                        If StepFunction IsNot Nothing Then
                            node.StepFunctionID = StepFunction.ID
                        End If
                    Case ECMeasureType.mtRegularUtilityCurve
                        Dim RUC As clsRegularUtilityCurve = PM.ActiveObjectives.ProjectManager.MeasureScales.GetRegularUtilityCurveByID(ScaleID)
                        If RUC IsNot Nothing Then
                            node.RegularUtilityCurveID = RUC.ID
                        End If
                    Case ECMeasureType.mtAdvancedUtilityCurve
                        Dim AUC As clsAdvancedUtilityCurve = PM.ActiveObjectives.ProjectManager.MeasureScales.GetAdvancedUtilityCurveByID(ScaleID)
                        If AUC IsNot Nothing Then
                            node.AdvancedUtilityCurveID = AUC.ID
                        End If
                    Case ECMeasureType.mtPWAnalogous
                        If Not SourceNodeID.Equals(Guid.Empty) Then
                            With PM
                                Dim SourceNode As clsNode = .Hierarchy(.ActiveHierarchy).GetNodeByID(SourceNodeID)
                                Dim alts As List(Of clsNode) = node.GetContributedAlternatives()

                                If alts IsNot Nothing Then
                                    Dim tContributionExists As Boolean = False
                                    Dim tAltWithKnownLikelihood As clsNode = Nothing
                                    Dim tknownLikelihood As KnownLikelihoodDataContract = Nothing
                                    For Each alt In alts
                                        For Each value In SourceKnownLikelihoods
                                            If value.GuidID.Equals(alt.NodeGuidID) AndAlso value.Value > 0 Then
                                                tContributionExists = True
                                                tAltWithKnownLikelihood = alt
                                                tknownLikelihood = value
                                            End If
                                        Next
                                    Next

                                    If tContributionExists Then
                                        .Attributes.SetAttributeValue(ATTRIBUTE_KNOWN_VALUE_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtDouble, tknownLikelihood.Value, tAltWithKnownLikelihood.NodeGuidID, node.NodeGuidID)
                                        tknownLikelihoodsChanged = True
                                        Dim TargetKnownLikelihoods = GetKnownLikelihoods(node.NodeGuidID)
                                        For Each alt In alts
                                            If alt.NodeGuidID <> tAltWithKnownLikelihood.NodeGuidID Then
                                                For Each lkhd In TargetKnownLikelihoods
                                                    If lkhd.GuidID.Equals(alt.NodeGuidID) AndAlso lkhd.Value > 0 Then
                                                        .Attributes.SetAttributeValue(ATTRIBUTE_KNOWN_VALUE_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtDouble, UNDEFINED_ATTRIBUTE_DEFAULT_DOUBLE_VALUE, alt.NodeGuidID, node.NodeGuidID)
                                                    End If
                                                Next
                                            End If
                                        Next
                                    End If

                                End If
                            End With
                        End If
                End Select
            End If
            If node.MeasurementScale IsNot Nothing Then tRes = node.MeasurementScale.GuidID
        Next

        If tknownLikelihoodsChanged OrElse mtChaged Then
            With PM
                If fSaveChanges Then
                    .Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID, UNDEFINED_USER_ID)
                End If
            End With
        End If

        If fSaveChanges Then
            PM.StorageManager.Writer.SaveModelStructure()
            PRJ.SaveStructure("Set measurement scale", True, , GetScaleTypeName(MeasurementType)) ' D3731
        End If
    End Sub

    Public Sub SetMeasurementTypeForNodes(Nodes As List(Of clsNode), MeasurementType As ECMeasureType)
        Dim tRes As Boolean = False

        With PM.StorageManager
            PM.Attributes.ReadAttributeValues(AttributesStorageType.astStreamsDatabase, .ProjectLocation, .ProviderType, .ModelID, UNDEFINED_USER_ID)
        End With
        Dim mtChanged As Boolean = False
        For Each node In Nodes
            If node.MeasureType <> MeasurementType Then
                node.MeasureType = MeasurementType
                mtChanged = True
                node.ResetClusterPhrase()
            End If
        Next

        If mtChanged Then
            With PM
                .Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID, UNDEFINED_USER_ID)
            End With
        End If

        PRJ.SaveStructure("Set measurement scale", True, True, PrepareTask(String.Format("Set '{0}' for all %%objectives%%", GetScaleTypeName(MeasurementType))))    ' D3731
    End Sub

    Public ReadOnly Property DefaultMeasurementTypeAltString(mode As MeasurementMethodsPageTypes) As String
        Get
            Dim retValAlt As String = ""
            Dim fmt As String = ResString("lblDefaultMTValues")
            If mode = MeasurementMethodsPageTypes.mmAll Then fmt = fmt.Replace(":", " (" + ParseString("%%Alternatives%%").First + "):")
            Select Case PM.PipeBuilder.PipeParameters.DefaultCoveringObjectiveMeasurementType
                Case ECMeasureType.mtPairwise
                    retValAlt = String.Format(fmt, ResString("optPairwise"))
                Case ECMeasureType.mtRatings
                    retValAlt = String.Format(fmt, ResString("optRatingScale"))
            End Select
            If mode = MeasurementMethodsPageTypes.mmObj Then Return ""
            Return "<br/>" + retValAlt
        End Get
    End Property

    Public ReadOnly Property DefaultNumberOfComparisonsString(mode As MeasurementMethodsPageTypes) As String
        Get
            Dim retValObj As String = ""
            Dim fmt As String = ResString("lblDefaultMTValues")
            If mode = MeasurementMethodsPageTypes.mmAll Then fmt = fmt.Replace(":", " (" + ParseString("%%Objectives%%").First + "):")
            Select Case PM.PipeBuilder.PipeParameters.EvaluateDiagonals
                Case DiagonalsEvaluation.deAll
                    retValObj = String.Format(fmt, ResString("optEvalDiagAll"))
                Case DiagonalsEvaluation.deFirst
                    retValObj = String.Format(fmt, ResString("optEvalDiagFirst"))
                Case DiagonalsEvaluation.deFirstAndSecond
                    retValObj = String.Format(fmt, ResString("optEvalDiagFirstSecond"))
            End Select

            Dim retValAlt As String = ""
            fmt = ResString("lblDefaultMTValues")
            If mode = MeasurementMethodsPageTypes.mmAll Then fmt = fmt.Replace(":", " (" + ParseString("%%Alternatives%%").First + "):")
            Select Case PM.PipeBuilder.PipeParameters.EvaluateDiagonalsAlternatives
                Case DiagonalsEvaluation.deAll
                    retValAlt = String.Format(fmt, ResString("optEvalDiagAll"))
                Case DiagonalsEvaluation.deFirst
                    retValAlt = String.Format(fmt, ResString("optEvalDiagFirst"))
                Case DiagonalsEvaluation.deFirstAndSecond
                    retValAlt = String.Format(fmt, ResString("optEvalDiagFirstSecond"))
            End Select
            If mode = MeasurementMethodsPageTypes.mmAll Then Return retValObj + "<br/>" + retValAlt
            If mode = MeasurementMethodsPageTypes.mmObj Then Return retValObj
            Return retValAlt
        End Get
    End Property

    Public ReadOnly Property DefaultPairwiseTypeString(mode As MeasurementMethodsPageTypes) As String
        Get
            Dim retValObj As String = ""
            Dim fmt As String = ResString("lblDefaultMTValues")
            If mode = MeasurementMethodsPageTypes.mmAll Then fmt = fmt.Replace(":", " (" + ParseString("%%Objectives%%").First + "):")
            Select Case PM.PipeBuilder.PipeParameters.PairwiseType
                Case PairwiseType.ptGraphical
                    retValObj = String.Format(fmt, ResString("optEvalPWGraphical"))
                Case PairwiseType.ptVerbal
                    retValObj = String.Format(fmt, ResString("optEvalPWVerbal"))
            End Select

            Dim retValAlt As String = ""
            fmt = ResString("lblDefaultMTValues")
            If mode = MeasurementMethodsPageTypes.mmAll Then fmt = fmt.Replace(":", " (" + ParseString("%%Alternatives%%").First + "):")
            Select Case PM.PipeBuilder.PipeParameters.PairwiseTypeForAlternatives
                Case PairwiseType.ptGraphical
                    retValAlt = String.Format(fmt, ResString("optEvalPWGraphical"))
                Case PairwiseType.ptVerbal
                    retValAlt = String.Format(fmt, ResString("optEvalPWVerbal"))
            End Select
            If mode = MeasurementMethodsPageTypes.mmAll Then Return retValObj + "<br/>" + retValAlt
            If mode = MeasurementMethodsPageTypes.mmObj Then Return retValObj
            Return retValAlt
        End Get
    End Property

    Public ReadOnly Property DefaultDisplayString(mode As MeasurementMethodsPageTypes) As String
        Get
            Dim retValObj As String = ""
            Dim fmt As String = ResString("lblDefaultMTValues")
            If mode = MeasurementMethodsPageTypes.mmAll Then fmt = fmt.Replace(":", " (" + ParseString("%%Objectives%%").First + "):")
            If PM.PipeBuilder.PipeParameters.ObjectivesPairwiseOneAtATime Then
                retValObj = String.Format(fmt, ResString("optEvalPairwiseOneShort"))
            Else
                retValObj = String.Format(fmt, ResString("optEvalPairwiseAllShort"))
            End If

            Dim retValAlt As String = ""
            fmt = ResString("lblDefaultMTValues")
            If mode = MeasurementMethodsPageTypes.mmAll Then fmt = fmt.Replace(":", " (" + ParseString("%%Alternatives%%").First + "):")
            If PM.PipeBuilder.PipeParameters.AlternativesPairwiseOneAtATime Then
                retValAlt = String.Format(fmt, ResString("optEvalPairwiseOneShort"))
            Else
                retValAlt = String.Format(fmt, ResString("optEvalPairwiseAllShort"))
            End If
            If mode = MeasurementMethodsPageTypes.mmAll Then Return retValObj + "<br/>" + retValAlt
            If mode = MeasurementMethodsPageTypes.mmObj Then Return retValObj
            Return retValAlt
        End Get
    End Property

    Public ReadOnly Property DefaultForceGraphicalString(mode As MeasurementMethodsPageTypes) As String
        Get
            Dim retValObj As String = ""
            Dim fmt As String = ResString("lblDefaultMTValues")
            If mode = MeasurementMethodsPageTypes.mmAll Then fmt = fmt.Replace(":", " (" + ParseString("%%Objectives%%").First + "):")
            If PM.PipeBuilder.PipeParameters.ForceGraphical Then
                retValObj = String.Format(fmt, ResString("lblYes"))
            Else
                retValObj = String.Format(fmt, ResString("lblNo"))
            End If

            Dim retValAlt As String = ""
            fmt = ResString("lblDefaultMTValues")
            If mode = MeasurementMethodsPageTypes.mmAll Then fmt = fmt.Replace(":", " (" + ParseString("%%Alternatives%%").First + "):")
            If PM.PipeBuilder.PipeParameters.ForceGraphicalForAlternatives Then
                retValAlt = String.Format(fmt, ResString("lblYes"))
            Else
                retValAlt = String.Format(fmt, ResString("lblNo"))
            End If
            If mode = MeasurementMethodsPageTypes.mmAll Then Return retValObj + "<br/>" + retValAlt
            If mode = MeasurementMethodsPageTypes.mmObj Then Return retValObj
            Return retValAlt
        End Get
    End Property

    Public Function GetKnownLikelihoods(ByVal ParentNodeID As Guid) As List(Of KnownLikelihoodDataContract)
        Dim res As List(Of KnownLikelihoodDataContract) = Nothing
        With PM
            Dim ParentNode As clsNode = .Hierarchy(.ActiveHierarchy).GetNodeByID(ParentNodeID)
            If ParentNode IsNot Nothing Then
                res = ParentNode.GetKnownLikelihoods()
            End If
        End With
        Return res
    End Function

    Private Function IsScaleInUse(ScaleID As Guid) As String
        Dim sUses As String = ""
        Dim NodesList = PM.Hierarchy(ECHierarchyID.hidLikelihood).Nodes
        NodesList.AddRange(PM.Hierarchy(ECHierarchyID.hidLikelihood).GetUncontributedAlternatives)
        If PM.Hierarchy(ECHierarchyID.hidImpact) IsNot Nothing Then 
            NodesList.AddRange(PM.Hierarchy(ECHierarchyID.hidImpact).Nodes)
        End If
        For Each Node In NodesList
            If Node.MeasurementScale IsNot Nothing AndAlso Node.MeasurementScale.GuidID = ScaleID Then 
                If sUses.Length < 150 Then sUses += String.Format("{0}'{1}'", IIf(sUses = "", "", ", "), Node.NodeName) Else Return sUses + ", ..."
            End If
        Next
        Return sUses
    End Function

    Private Function AltID(NodeId As Integer) As Integer
        Return Integer.MaxValue - NodeId - 1
    End Function

    Protected Sub Ajax_Callback(data As String)
        Dim args As NameValueCollection = HttpUtility.ParseQueryString(data)
        Dim sAction As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "action", True)).ToLower ' Anti-XSS
        Dim tResult As String = CStr(IIf(String.IsNullOrEmpty(sAction), "", "['err_msg','Server error']"))

        Select Case sAction
            Case ACTION_VIEW ' MTMode = all/obj/alt
                Dim iVM As Integer = CInt(MTMode)
                Dim VM As Integer = CInt(GetParam(args, "value", True).Trim)
                If VM <> iVM Then MTMode = CType(VM, MeasurementMethodsPageTypes)
                tResult = String.Format("['{0}',{1}]", sAction, CInt(MTMode))
            Case ACTION_MEASURE_TYPE
                Dim NodeId As Integer = CInt(GetParam(args, "node", True).Trim())
                Dim ParentId As Integer = CInt(GetParam(args, "pid", True).Trim())
                Dim MT As ECMeasureType = CType(CInt(GetParam(args, "mt", True).Trim()), ECMeasureType)
                Dim isEventWithNoSource As Boolean = Str2Bool(GetParam(args, "is_ns", True).Trim())
                Dim node As clsNode = If(Not isEventWithNoSource, PM.ActiveObjectives.GetNodeByID(NodeId), PM.ActiveAlternatives.GetNodeByID(AltID(NodeId)))
                If node IsNot Nothing Then
                    node.MeasureType = MT
                    node.ResetClusterPhrase()
                    With PM
                        .Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID, UNDEFINED_USER_ID)
                    End With
                    PRJ.SaveStructure("Set measurement scale", True, True, GetScaleTypeName(MT))    ' D3731
                    tResult = String.Format("['{0}',{1}]", sAction, GetNodeData(node, node.ParentNode)) 'TODO: DA_CH
                End If
            Case ACTION_MEASURE_SCALE
                Dim NodeId As Integer = CInt(GetParam(args, "node", True).Trim())
                Dim ParentId As Integer = CInt(GetParam(args, "pid", True).Trim())
                Dim MSGUID As Guid = New Guid(GetParam(args, "ms", True))
                Dim isEventWithNoSource As Boolean = Str2Bool(GetParam(args, "is_ns", True).Trim())
                Dim node As clsNode = If(Not isEventWithNoSource, PM.ActiveObjectives.GetNodeByID(NodeId), PM.ActiveAlternatives.GetNodeByID(AltID(NodeId)))
                If node IsNot Nothing Then
                    Dim nodeIDs As New List(Of Guid)
                    nodeIDs.Add(node.NodeGuidID)
                    SetNodeMeasurementScale(nodeIDs, node.MeasureType, MSGUID, Guid.Empty, False, , isEventWithNoSource)
                    tResult = String.Format("['{0}',{1}]", sAction, GetNodeData(node, PM.ActiveObjectives.GetNodeByID(ParentId))) 'TODO: DA_CH
                End If
            Case ACTION_NUM_COMPARISONS, ACTION_PAIRWISE_TYPE, ACTION_DISPLAY_OPTION, ACTION_FORCE_GRAPHICAL
                Dim NodeId As Integer = CInt(GetParam(args, "node", True))
                Dim ParentId As Integer = CInt(GetParam(args, "pid", True))
                Dim sValue As String = CStr(GetParam(args, "value", True))
                Dim H As clsHierarchy = PM.ActiveObjectives
                Dim isEventWithNoSource As Boolean = Str2Bool(GetParam(args, "is_ns", True).Trim())
                Dim node As clsNode = If(Not isEventWithNoSource, PM.ActiveObjectives.GetNodeByID(NodeId), PM.ActiveAlternatives.GetNodeByID(AltID(NodeId)))
                If node IsNot Nothing Then
                    Dim AttributeID As Guid
                    Dim Value As Object = Nothing
                    Select Case sAction
                        Case ACTION_NUM_COMPARISONS
                            AttributeID = ATTRIBUTE_EVALUATE_DIAGONALS_MODE_ID
                            If Not String.IsNullOrEmpty(sValue) Then Value = CInt(sValue)
                            PM.PipeBuilder.PipeCreated = False
                        Case ACTION_PAIRWISE_TYPE
                            AttributeID = ATTRIBUTE_PAIRWISE_TYPE_ID
                            If Not String.IsNullOrEmpty(sValue) Then Value = CInt(sValue)
                        Case ACTION_DISPLAY_OPTION
                            AttributeID = ATTRIBUTE_DISPLAY_OPTION_PAIRWISE_ID
                            If Not String.IsNullOrEmpty(sValue) Then Value = CInt(sValue)
                        Case ACTION_FORCE_GRAPHICAL
                            AttributeID = ATTRIBUTE_PAIRWISE_FORCE_GRAPHICAL_ID
                            If Not String.IsNullOrEmpty(sValue) Then Value = Str2Bool(sValue)
                    End Select
                    Dim attr As clsAttribute = PM.Attributes.GetAttributeByID(AttributeID)
                    If attr IsNot Nothing Then
                        With PM
                            .Attributes.SetAttributeValue(AttributeID, UNDEFINED_USER_ID, CType(IIf(AttributeID.Equals(ATTRIBUTE_PAIRWISE_FORCE_GRAPHICAL_ID), AttributeValueTypes.avtBoolean, AttributeValueTypes.avtLong), AttributeValueTypes), Value, node.NodeGuidID, Guid.Empty)
                            .Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID, UNDEFINED_USER_ID)
                            PM.PipeBuilder.PipeCreated = False  ' D7060
                        End With
                    End If
                    tResult = String.Format("['{0}',{1}]", sAction, GetNodeData(node, PM.ActiveObjectives.GetNodeByID(ParentId))) 'TODO: DA_CH
                End If
            Case ACTION_SET_MEASURE_TYPE_FOR_ALL
                Dim MT As ECMeasureType = CType(CInt(GetParam(args, "mt", True).Trim()), ECMeasureType)
                Dim H As clsHierarchy = PM.ActiveObjectives
                Dim Nodes As New List(Of clsNode)
                For Each node As clsNode In H.Nodes
                    Select Case MTMode
                        Case MeasurementMethodsPageTypes.mmAll
                            Nodes.Add(node)
                        Case MeasurementMethodsPageTypes.mmAlt
                            If node.IsTerminalNode Then Nodes.Add(node)
                        Case MeasurementMethodsPageTypes.mmObj
                            If Not node.IsTerminalNode Then Nodes.Add(node)
                    End Select
                Next
                SetMeasurementTypeForNodes(Nodes, MT)
                tResult = String.Format("['{0}',{1}]", sAction, GetNodesData())
            Case "copy_mt_to"
                Dim NodeID As Integer = CInt(EcSanitizer.GetSafeHtmlFragment(GetParam(args, "from", True))) ' Anti-XSS
                Dim sToNodeIDs As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "to", True)).ToLower ' Anti-XSS
                Dim tToNodesIDs As List(Of Integer) = Param2IntList(sToNodeIDs)
                Dim isEventWithNoSource As Boolean = Str2Bool(GetParam(args, "is_ns", True).Trim())
                Dim H As clsHierarchy = If(isEventWithNoSource, PM.ActiveAlternatives, PM.ActiveObjectives)
                Dim SourceNode As clsNode = H.GetNodeByID(If(isEventWithNoSource, AltID(NodeId), NodeID))
                If SourceNode IsNot Nothing Then
                    Dim TargetNodes As New List(Of Guid)
                    For Each node As clsNode In H.Nodes
                        If tToNodesIDs.Contains(If(isEventWithNoSource, AltID(node.NodeID), node.NodeID)) AndAlso (isEventWithNoSource OrElse node.IsTerminalNode = SourceNode.IsTerminalNode)
                            If Not TargetNodes.Contains(node.NodeGuidID) Then TargetNodes.Add(node.NodeGuidID)
                        End If                        
                    Next
                    If TargetNodes.Count > 0 Then
                        SetNodeMeasurementScale(TargetNodes, SourceNode.MeasureType, If(SourceNode.MeasurementScale IsNot Nothing, SourceNode.MeasurementScale.GuidID, Guid.Empty), SourceNode.NodeGuidID, True, False, isEventWithNoSource)
                        PM.Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, PM.StorageManager.ProjectLocation, PM.StorageManager.ProviderType, PM.StorageManager.ModelID, UNDEFINED_USER_ID)
                        PM.StorageManager.Writer.SaveModelStructure()
                        PRJ.SaveStructure("Set measurement scale", True, , GetScaleTypeName(SourceNode.MeasureType))
                        tResult = String.Format("['{0}',{1}]", sAction, GetNodesData())
                    End If
                End If
            Case ACTION_SET_NODE_PROPERTY
                'action=set_node_prop&prop=" + op + "&val=" + toggle_val);
                Dim NodeID As Integer = CInt(GetParam(args, "node_id", True))
                Dim ParentId As Integer = CInt(GetParam(args, "pid", True))
                Dim sProp As String = GetParam(args, "prop", True)
                Dim sVal As String = GetParam(args, "val", True)
                Dim H As clsHierarchy = PM.ActiveObjectives
                Dim fChanged As Boolean = False
                Dim node As clsNode = H.GetNodeByID(NodeID)
                If node IsNot Nothing Then
                    Select Case sProp
                        Case "s"
                            fChanged = PM.Attributes.SetAttributeValue(ATTRIBUTE_IS_STRUCTURAL_ADJUST_NODE_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtBoolean, Str2Bool(sVal), node.NodeGuidID, Nothing)
                        Case "m"
                            fChanged = PM.Attributes.SetAttributeValue(ATTRIBUTE_NODE_SYNTHESIS_TYPE_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtLong, CInt(sVal), node.NodeGuidID, Nothing)
                        Case "c"
                            Dim bVal = Str2Bool(sVal)
                            node.RiskNodeType = If(bVal, RiskNodeType.ntCategory, RiskNodeType.ntUncertainty)
                            fChanged = True
                    End Select
                End If
                If fChanged Then
                    With PM
                        .Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID, UNDEFINED_USER_ID)
                        PM.PipeBuilder.PipeCreated = False  ' D7060
                    End With
                End If
                tResult = String.Format("['{0}',{1}]", sAction, GetNodeData(node, PM.ActiveObjectives.GetNodeByID(ParentId)))
            Case ACTION_GET_STAT_DATA
                Dim NodeID As Integer = CInt(EcSanitizer.GetSafeHtmlFragment(GetParam(args, "node_id", True))) ' Anti-XSS
                Dim H As clsHierarchy = PM.ActiveObjectives
                Dim sData As String = "[]"
                If H IsNot Nothing Then
                    Dim node As clsNode = H.GetNodeByID(NodeID)
                    If node IsNot Nothing Then
                        sData = GetNodeStatData(node)
                    End If
                End If
                tResult = String.Format("['{0}',{1}]", sAction, sData)
            Case ACTION_SAVE_STAT_DATA
                Dim NodeID As Integer = CInt(EcSanitizer.GetSafeHtmlFragment(GetParam(args, "node_id", True))) ' Anti-XSS
                Dim ItemsCount As Integer = CInt(EcSanitizer.GetSafeHtmlFragment(GetParam(args, "count", True))) ' Anti-XSS
                Dim H As clsHierarchy = PM.ActiveObjectives
                Dim sData As String = "[]"
                If H IsNot Nothing Then
                    Dim node As clsNode = H.GetNodeByID(NodeID)
                    If node IsNot Nothing Then
                        Dim tStatData As List(Of clsStatisticalDataItem) = New List(Of clsStatisticalDataItem)
                        For i As Integer = 0 To ItemsCount - 1
                            Dim sPeriod As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "p" + i.ToString, True)) ' Anti-XSS
                            Dim sValue As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "v" + i.ToString, True)) ' Anti-XSS
                            Dim tValue As Double 
                            If String2Double(sValue, tValue) Then 
                                tStatData.Add(New clsStatisticalDataItem With {.TimePeriodName = sPeriod, .Data = tValue})
                            End If
                        Next
                        
                        If tStatData. Count = 0 Then tStatData = Nothing
                        SaveNodeStatisticalData(node.NodeGuidID, tStatData)

                        sData = GetNodeStatData(node)
                    End If
                End If
                tResult = String.Format("['{0}',{1}]", sAction, sData)
            Case "delete_scale"
                Dim ms As Guid = New Guid(CStr(EcSanitizer.GetSafeHtmlFragment(GetParam(args, "ms", True)))) ' Anti-XSS
                Dim tScale As ECCore.clsMeasurementScale = PM.MeasureScales.GetScaleByID(ms)
                Dim sName As String = If(tScale IsNot Nothing, tScale.Name, "")
                Dim sUses As String = IsScaleInUse(ms)
                If sUses = "" Then 
                    PM.MeasureScales.DeleteScaleByID(ms)
                    PM.StorageManager.Writer.SaveModelStructure()
                    PM.PipeBuilder.PipeCreated = False  ' D7060
                    App.SaveProjectLogEvent(PRJ, "Delete measurement scale", True, sName)
                    tResult = String.Format("['{0}',{1},'']", sAction, GetMeasureScalesData())
                Else
                    tResult = String.Format("['err_msg','{0}']", JS_SafeString(String.Format(ResString("msgScaleInUse"), sUses)))
                End If
            Case "refresh_scale"
                Dim ms As Guid = New Guid(CStr(EcSanitizer.GetSafeHtmlFragment(GetParam(args, "ms", True)))) ' Anti-XSS
                Dim mt As ECMeasureType = CType(CInt(EcSanitizer.GetSafeHtmlFragment(GetParam(args, "mt", True))), ECMeasureType) ' Anti-XSS
                Dim tScale As ECCore.clsMeasurementScale = PM.MeasureScales.GetScaleByID(ms)
                tResult = String.Format("['{0}',{1}]", sAction, GetMeasureScaleData(tScale, mt, PM.MeasureScales.GetRatingScaleType(tScale)))
            Case "new_scale"
                Dim NewScale As clsMeasurementScale = Nothing
                Dim mt As ECMeasureType = CType(CInt(EcSanitizer.GetSafeHtmlFragment(GetParam(args, "mt", True))), ECMeasureType) ' Anti-XSS
                Dim sName As String = CStr(EcSanitizer.GetSafeHtmlFragment(GetParam(args, "name", True))).Trim ' Anti-XSS
                Select Case mt
                    Case ECMeasureType.mtRatings, ECMeasureType.mtPWOutcomes
                        NewScale = PM.MeasureScales.AddRatingScale()
                        If mt = ECMeasureType.mtPWOutcomes Then CType(NewScale, clsRatingScale).IsOutcomes = True
                    Case ECMeasureType.mtStep
                        NewScale  = PM.MeasureScales.AddStepFunction()
                    Case ECMeasureType.mtRegularUtilityCurve
                        NewScale  = PM.MeasureScales.AddRegularUtilityCurve()
                End Select
                If NewScale IsNot Nothing AndAlso sName <> "" Then 
                    NewScale.Name = sName
                End If
                If NewScale IsNot Nothing AndAlso PM.IsRiskProject Then 
                    NewScale.Type = If(PM.ActiveHierarchy = ECHierarchyID.hidLikelihood, ScaleType.stLikelihood, ScaleType.stImpact)
                End If
                PM.StorageManager.Writer.SaveModelStructure()
                App.SaveProjectLogEvent(PRJ, "New measurement scale", True, If(NewScale Is Nothing, "", NewScale.Name)) 
                tResult = String.Format("['{0}',{1},'{2}']", sAction, GetMeasureScaleData(NewScale, mt, PM.MeasureScales.GetRatingScaleType(NewScale)), If(NewScale Is Nothing, "", NewScale.GuidID.ToString))
            Case "scale_name"
                Dim ms As Guid = New Guid(CStr(EcSanitizer.GetSafeHtmlFragment(GetParam(args, "ms", True)))) ' Anti-XSS
                Dim mt As ECMeasureType = CType(CInt(EcSanitizer.GetSafeHtmlFragment(GetParam(args, "mt", True))), ECMeasureType) ' Anti-XSS
                Dim sName As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "name", True)).Trim ' Anti-XSS

                Dim tScale As clsMeasurementScale = PM.MeasureScales.GetScaleByID(ms)
                If tScale IsNot Nothing Then 
                    tScale.Name = sName
                    PM.StorageManager.Writer.SaveModelStructure()
                    App.SaveProjectLogEvent(PRJ, "Edit measurement scale", True, tScale.Name) 
                End If

                tResult = String.Format("['{0}',{1}]", sAction, GetMeasureScaleData(tScale, mt, PM.MeasureScales.GetRatingScaleType(tScale)))
            Case "clone_scale"
                Dim NewScaleID As Guid = Guid.Empty

                Dim ms As Guid = New Guid(CStr(EcSanitizer.GetSafeHtmlFragment(GetParam(args, "ms", True)))) ' Anti-XSS
                Dim mt As ECMeasureType = CType(CInt(EcSanitizer.GetSafeHtmlFragment(GetParam(args, "mt", True))), ECMeasureType) ' Anti-XSS
                Dim CopyJudgments As Boolean = True
                Dim RatingScale As ECCore.clsRatingScale = Nothing
                Dim RUC As ECCore.clsRegularUtilityCurve = Nothing
                Dim SF As ECCore.clsStepFunction = Nothing
                Dim sScaleData As String = ""
                PM.StorageManager.Reader.LoadUserData(PM.User)

                Select Case mt
                    Case ECMeasureType.mtRatings, ECMeasureType.mtPWOutcomes
                        RatingScale = PM.MeasureScales.CloneRatingScale(CType(PM.MeasureScales.GetScaleByID(ms), ECCore.clsRatingScale), CopyJudgments)
                        RatingScale.IsDefault = False
                        Dim destH As ECCore.clsHierarchy = PM.CloneMeasureHierarchy(ms, RatingScale.GuidID, CopyJudgments)
                        If destH IsNot Nothing Then
                            PM.StorageManager.Writer.SaveUserJudgments(PM.User)
                        End If
                        NewScaleID = RatingScale.GuidID
                        sScaleData = GetMeasureScaleData(RatingScale, mt, PM.MeasureScales.GetRatingScaleType(RatingScale))
                    Case ECMeasureType.mtRegularUtilityCurve
                        RUC = PM.MeasureScales.CloneRegularUtilityCurve(CType(PM.MeasureScales.GetScaleByID(ms), ECCore.clsRegularUtilityCurve))
                        RUC.IsDefault = False
                        NewScaleID = RUC.GuidID
                        sScaleData = GetMeasureScaleData(RUC, mt, PM.MeasureScales.GetRatingScaleType(RUC))
                    Case ECMeasureType.mtStep
                        SF = PM.MeasureScales.CloneStepFunction(CType(PM.MeasureScales.GetScaleByID(ms), ECCore.clsStepFunction), CopyJudgments)
                        SF.IsDefault = False
                        Dim destH As ECCore.clsHierarchy = PM.CloneMeasureHierarchy(ms, SF.GuidID, CopyJudgments)
                        If destH IsNot Nothing Then
                            PM.StorageManager.Writer.SaveUserJudgments(PM.User)
                        End If
                        NewScaleID = SF.GuidID
                        sScaleData = GetMeasureScaleData(SF, mt, PM.MeasureScales.GetRatingScaleType(SF))
                End Select

                PM.StorageManager.Writer.SaveModelStructure()
                PM.PipeBuilder.PipeCreated = False  ' D7060
                App.SaveProjectLogEvent(PRJ, "Clone measurement scale", True, PM.MeasureScales.GetScaleByID(ms).Name)
                tResult = String.Format("['{0}',{1},'{2}']", sAction, sScaleData, NewScaleID)
            Case "edit_int"
                Dim ms As Guid = New Guid(CStr(GetParam(args, "ms", True)))
                Dim sName As String = GetParam(args, "name", True)
                Dim sValue As String = GetParam(args, "val", True)
                Dim sRatID As String = GetParam(args, "id", True)
                Dim tValue As Double = 0
                String2Double(sValue, tValue)
                Dim sDescr As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "descr", True)) ' Anti-XSS

                Dim rs As clsRatingScale = CType(PM.MeasureScales.GetScaleByID(ms), clsRatingScale)
                If rs IsNot Nothing Then 
                    Dim rsInt As clsRating
                    If (sRatID <> "") Then
                        rsInt = rs.GetRatingByID(New Guid(sRatID))
                    Else
                        rsInt = rs.AddIntensity()
                    End If
                    rsInt.Name = sName
                    If tValue < 0 Then tValue = 0
                    If tValue > 1 Then tValue = 1
                    rsInt.Value = CSng(tValue)
                    rsInt.Comment = sDescr
                    rs.RatingSet.Sort(New clsIntensityComparer(SortDirection.Descending))
                    PM.StorageManager.Writer.SaveModelStructure()
                    PM.PipeBuilder.PipeCreated = False  ' D7060
                    App.SaveProjectLogEvent(PRJ, "Edit measurement scale", True, rs.Name)
                End If

                tResult = String.Format("['{0}',{1}]", sAction, GetMeasureScaleData(rs, ECMeasureType.mtRatings, PM.MeasureScales.GetRatingScaleType(rs)))
            Case "edit_sf_int"
                Dim ms As Guid = New Guid(CStr(EcSanitizer.GetSafeHtmlFragment(GetParam(args, "ms", True)))) ' Anti-XSS
                Dim sName As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "name", True)) ' Anti-XSS
                Dim sLow As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "low", True)) ' Anti-XSS
                Dim sValue As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "val", True)) ' Anti-XSS
                Dim sRatID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "id", True)) ' Anti-XSS
                Dim tValue As Double = 0
                Dim tLow As Double = 0
                String2Double(sValue, tValue)
                String2Double(sLow, tLow)                

                Dim sf As clsStepFunction = CType(PM.MeasureScales.GetScaleByID(ms), clsStepFunction)
                If sf IsNot Nothing Then
                    Dim sfInt As clsStepInterval
                    If (sRatID <> "") Then
                        sfInt = sf.GetIntervalByComment(sRatID)
                        if sfInt.Comment = "" Then sfInt.Comment = (Guid.NewGuid()).ToString
                    Else
                        sfInt = sf.AddInterval()
                        sfInt.Comment = Guid.NewGuid().ToString
                    End If
                    sfInt.Name = sName
                    If tValue < 0 Then tValue = 0
                    If tValue > 1 Then tValue = 1
                    sfInt.Value = CSng(tValue)
                    sfInt.Low = CSng(tLow)
                    sf.Intervals.Sort(New clsIntervalsComparer(SortDirection.Descending, clsIntervalsComparer.IntervalField.Value))
                    PM.StorageManager.Writer.SaveModelStructure()
                    PM.PipeBuilder.PipeCreated = False  ' D7060
                    App.SaveProjectLogEvent(PRJ, "Edit measurement scale", True, sf.Name)
                End If

                tResult = String.Format("['{0}',{1}]", sAction, GetMeasureScaleData(sf, ECMeasureType.mtStep, PM.MeasureScales.GetRatingScaleType(sf)))
            Case "del_int"
                Dim ms As Guid = New Guid(CStr(EcSanitizer.GetSafeHtmlFragment(GetParam(args, "ms", True)))) ' Anti-XSS
                Dim id As Guid = New Guid(CStr(EcSanitizer.GetSafeHtmlFragment(GetParam(args, "id", True)))) ' Anti-XSS                
                Dim rs As clsRatingScale = CType(PM.MeasureScales.GetScaleByID(ms), clsRatingScale)
                If rs IsNot Nothing Then
                    Dim intensityToRemove As clsRating = Nothing
                    For Each intensity As clsRating In rs.RatingSet
                        If intensity.GuidID = id Then intensityToRemove = intensity 
                    Next
                    If intensityToRemove IsNot Nothing Then
                        rs.RatingSet.Remove(intensityToRemove)
                        PM.StorageManager.Writer.SaveModelStructure()
                        PM.PipeBuilder.PipeCreated = False  ' D7060
                        App.SaveProjectLogEvent(PRJ, "Edit measurement scale", True, rs.Name)
                    End If
                End If

                tResult = String.Format("['{0}',{1}]", sAction, GetMeasureScaleData(rs, ECMeasureType.mtRatings, PM.MeasureScales.GetRatingScaleType(rs)))
            Case "del_sf_int"
                Dim ms As Guid = New Guid(CStr(EcSanitizer.GetSafeHtmlFragment(GetParam(args, "ms", True)))) ' Anti-XSS
                Dim id As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "id", True)) ' Anti-XSS
                Dim sf As clsStepFunction = CType(PM.MeasureScales.GetScaleByID(ms), clsStepFunction)
                If sf IsNot Nothing Then
                    Dim intensityToRemove As clsStepInterval = Nothing
                    For Each intensity As clsStepInterval  In sf.Intervals
                        If intensity.Comment = id Then intensityToRemove = intensity 
                    Next
                    If intensityToRemove IsNot Nothing Then
                        sf.Intervals.Remove(intensityToRemove)
                        PM.StorageManager.Writer.SaveModelStructure()
                        PM.PipeBuilder.PipeCreated = False  ' D7060
                        App.SaveProjectLogEvent(PRJ, "Edit measurement scale", True, sf.Name)
                    End If
                End If

                tResult = String.Format("['{0}',{1}]", sAction, GetMeasureScaleData(sf, ECMeasureType.mtStep, PM.MeasureScales.GetRatingScaleType(sf)))
            Case "scale_default"
                Dim ms As Guid = New Guid(CStr(EcSanitizer.GetSafeHtmlFragment(GetParam(args, "ms", True)))) ' Anti-XSS
                Dim mt As ECMeasureType = CType(CInt(EcSanitizer.GetSafeHtmlFragment(GetParam(args, "mt", True))), ECMeasureType) ' Anti-XSS
                Dim tScale As clsMeasurementScale = PM.MeasureScales.GetScaleByID(ms)
                
                Dim chk As Boolean = Param2Bool(args, "chk")
                PM.MeasureScales.SetScaleDefault(ms, chk)

                PM.StorageManager.Writer.SaveModelStructure()
                PM.PipeBuilder.PipeCreated = False  ' D7060
                App.SaveProjectLogEvent(PRJ, String.Format("Update default measurement scale"), False, "")

                tResult = String.Format("['{0}',{1}]", sAction, GetMeasureScaleData(tScale, mt, PM.MeasureScales.GetRatingScaleType(tScale)))
            Case "scale_hide"
                Dim ms As Guid = New Guid(CStr(EcSanitizer.GetSafeHtmlFragment(GetParam(args, "ms", True)))) ' Anti-XSS
                Dim chk As Boolean = Param2Bool(args, "chk")
                PM.Parameters.RatingsUseDirectValue(ms) = Not chk
                PM.Parameters.Save()
                Dim tScale As clsMeasurementScale = PM.MeasureScales.GetScaleByID(ms)
                tResult = String.Format("['{0}',{1}]", sAction, GetMeasureScaleData(tScale, ECMeasureType.mtRatings, PM.MeasureScales.GetRatingScaleType(tScale)))
            Case "scale_sf_linear"
                Dim ms As Guid = New Guid(CStr(EcSanitizer.GetSafeHtmlFragment(GetParam(args, "ms", True)))) ' Anti-XSS
                Dim chk As Boolean = Param2Bool(args, "chk")
                Dim sf As clsStepFunction = Ctype(PM.MeasureScales.GetScaleByID(ms), clsStepFunction)
                If sf IsNot Nothing Then
                    sf.IsPiecewiseLinear = chk
                    PM.StorageManager.Writer.SaveModelStructure()
                    PM.PipeBuilder.PipeCreated = False  ' D7060
                    App.SaveProjectLogEvent(PRJ, String.Format("Edit measurement scale"), False, "")
                End If

                tResult = String.Format("['{0}',{1}]", sAction, GetMeasureScaleData(sf, ECMeasureType.mtStep, PM.MeasureScales.GetRatingScaleType(sf)))
            Case "uc_param"
                Dim ms As Guid = New Guid(CStr(EcSanitizer.GetSafeHtmlFragment(GetParam(args, "ms", True)))) ' Anti-XSS
                Dim param As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "param", True)) ' Anti-XSS
                Dim sValue As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "value", True)) ' Anti-XSS
                Dim tValue As Double = 0
                Dim uc As clsRegularUtilityCurve = Ctype(PM.MeasureScales.GetScaleByID(ms), clsRegularUtilityCurve)
                If uc IsNot Nothing Then
                    Select case param
                        Case "curv"
                            If String2Double(sValue, tValue) Then
                                uc.Curvature = CSng(tValue)
                                uc.IsLinear = uc.Curvature = 0
                            End If
                        Case "low"
                            If String2Double(sValue, tValue) Then uc.Low = CSng(tValue)
                        Case "high"
                            If String2Double(sValue, tValue) Then uc.High = CSng(tValue)
                        Case "inc"
                            uc.IsIncreasing = Param2Bool(args, "value")
                        Case "lin"
                            uc.IsLinear = Param2Bool(args, "value")
                            If uc.IsLinear Then uc.Curvature = 0
                    End Select
                    PM.StorageManager.Writer.SaveModelStructure()
                    PM.PipeBuilder.PipeCreated = False  ' D7060
                    App.SaveProjectLogEvent(PRJ, String.Format("Edit measurement scale"), False, "")
                End If                
                tResult = String.Format("['{0}',{1}]", sAction, GetMeasureScaleData(uc, ECMeasureType.mtRegularUtilityCurve, PM.MeasureScales.GetRatingScaleType(uc)))
            Case "assess_priorities_options"
                Dim retVal As String = "<div>"
                retVal += "<strong><span>" + ResString("optEvalDiagonals") + "</span></strong><br/>"
                retVal += "<span>" + ResString("optEvalDiagonalsHint") + "</span><br/>"
                retVal += "<label><input type='radio' name='grpPairs' id='optEvalDiagAll'value='0' />" + ResString("optEvalDiagAll") + "</label><br/>"
                retVal += "<span>" + ResString("optEvalDiagAllHint") + "</span><br/>"
                retVal += "<label><input type='radio' name='grpPairs' checked='checked' id='optEvalDiagFirstSecond' value='2' />" + ResString("optEvalDiagFirstSecond") + "</label><br/>"
                retVal += "<span>" + ResString("optEvalDiagFirstSecondHint") + "</span><br/>"
                retVal += "<label><input type='radio' name='grpPairs' id='optEvalDiagFirst' value='1' />" + ResString("optEvalDiagFirst") + "</label><br/>"
                retVal += "<span>" + ResString("optEvalDiagFirstHint") + "</span><br/>"
                retVal += "<br/>"

                retVal += "<strong><span>" + ResString("optEvalPairwiseType") + "</span></strong><br/>"
                retVal += "<label><input type='radio' name='grpPW' id='optEvalPWGraphical' value='2' />" + ResString("optEvalPWGraphical") + "</label><br/>"
                retVal += "<label><input type='radio' name='grpPW' id='optEvalPWVerbal' value='1' checked='checked' />" + ResString("optEvalPWVerbal") + "</label><br/>"
                'retVal += "<label><input type='checkbox' id='optEvalPWForceGraphical' checked='checked' />" + ResString("optEvalPWForceGraphical") + "</label><br/>"
                retVal += "<br/>"

                retVal += "<strong><span>" + "Display options:" + "</span></strong><br/>"
                retVal += "<label><input type='checkbox' id='optDispInconsist' checked='checked' />" + ResString("optDispInconsist") + "</label><br/>"
                retVal += "<label><input type='checkbox' id='optDispMultiPairwise' checked='checked' />" + ResString("optDispMultiPairwise") + "</label><br/>"

                retVal += "</div>"
                tResult = String.Format("['{0}','{1}']", sAction, JS_SafeString(retVal))
            Case "assess_priorities_zeros"
                Dim ms As Guid = New Guid(CStr(EcSanitizer.GetSafeHtmlFragment(GetParam(args, "ms", True)))) ' Anti-XSS
                Dim list As New List(Of KeyValuePair(Of String, String))
                If (TypeOf PM.MeasureScales.GetScaleByID(ms) Is clsRatingScale) Then 
                    Dim rs As clsRatingScale = CType(PM.MeasureScales.GetScaleByID(ms), clsRatingScale)
                    For Each intensity As clsRating In rs.RatingSet                        
                        list.Add(New KeyValuePair(Of String, String)(intensity.GuidID.ToString, intensity.Name))
                    Next
                End If
                If (TypeOf PM.MeasureScales.GetScaleByID(ms) Is clsStepFunction) Then 
                    Dim sf As clsStepFunction = CType(PM.MeasureScales.GetScaleByID(ms), clsStepFunction)
                    For Each intensity As clsStepInterval In sf.Intervals
                        Dim g As Guid   ' D7063
                        list.Add(New KeyValuePair(Of String, String)(If(Guid.TryParse(intensity.Comment, g), intensity.Comment, intensity.ID.ToString), intensity.Name))    ' D7063
                    Next
                End If
                Dim retVal As String = "<div>"
                retVal += "<strong><span>" + String.Format(ResString("msgSelectIntensityToExclude"), "priority") + "</span></strong><br/>"
                For Each item As KeyValuePair(Of String, String) In list
                    retVal += "<label><input type='checkbox' class='cb_zeros' data-id='" + item.key + "' />" + item.Value + "</label><br/>"
                Next
                retVal += "</div>"
                tResult = String.Format("['{0}','{1}']", sAction, JS_SafeString(retVal))
            Case "assess_priorities_certain"
                tResult = String.Format("['{0}',[]]", sAction)
                Dim ms As Guid = New Guid(CStr(EcSanitizer.GetSafeHtmlFragment(GetParam(args, "ms", True)))) ' Anti-XSS
                If (TypeOf PM.MeasureScales.GetScaleByID(ms) Is clsRatingScale) Then 
                    Dim rs As clsRatingScale = CType(PM.MeasureScales.GetScaleByID(ms), clsRatingScale)
                    Dim intensity = rs.AddIntensity()
                    intensity.Name = ResString("sCertainIntensityName")
                    intensity.Value = 1
                    tResult = String.Format("['{0}',{1}]", sAction, GetMeasureScaleData(rs, ECMeasureType.mtRatings, PM.MeasureScales.GetRatingScaleType(rs)))
                End If
                If (TypeOf PM.MeasureScales.GetScaleByID(ms) Is clsStepFunction) Then 
                    Dim sf As clsStepFunction = CType(PM.MeasureScales.GetScaleByID(ms), clsStepFunction)
                    Dim intv = sf.AddInterval()
                    intv.Name = ResString("sCertainIntensityName")
                    intv.Value = 1
                    intv.Low = 0
                    intv.High = 1
                    tResult = String.Format("['{0}',{1}]", sAction, GetMeasureScaleData(sf, ECMeasureType.mtStep, PM.MeasureScales.GetRatingScaleType(sf)))
                End If
            Case "paste_scale"
                Dim ms As Guid = New Guid(CStr(EcSanitizer.GetSafeHtmlFragment(GetParam(args, "ms", True)))) ' Anti-XSS
                Dim mt As ECMeasureType = CType(CInt(EcSanitizer.GetSafeHtmlFragment(GetParam(args, "mt", True))), ECMeasureType) ' Anti-XSS
                Dim sText As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "data", True)) ' Anti-XSS
                Dim tScale As clsMeasurementScale = Nothing
                Select Case mt
                    Case ECMeasureType.mtRatings, ECMeasureType.mtPWOutcomes
                        Dim rs As clsRatingScale = CType(PM.MeasureScales.GetScaleByID(ms), clsRatingScale)
                        DoPasteRS(rs, mt, sText)
                        tScale = rs
                    Case ECMeasureType.mtStep
                        Dim sf As clsStepFunction = CType(PM.MeasureScales.GetScaleByID(ms), clsStepFunction)
                        DoPasteSF(sf, sText)
                        tScale = sf
                    Case ECMeasureType.mtRegularUtilityCurve
                        Dim uc As clsRegularUtilityCurve = Ctype(PM.MeasureScales.GetScaleByID(ms), clsRegularUtilityCurve)
                        DoPasteUC(uc, sText)
                        tScale = uc
                End Select
                PM.StorageManager.Writer.SaveModelStructure()
                PM.PipeBuilder.PipeCreated = False  ' D7060
                App.SaveProjectLogEvent(PRJ, String.Format("Paste measurement scale from clipboard"), False, "")
                tResult = String.Format("['{0}',{1}]", sAction, GetMeasureScaleData(tScale, mt, PM.MeasureScales.GetRatingScaleType(tScale)))
            Case "get_known_likelihoods"
                Dim ParentNodeID As Integer = CInt(EcSanitizer.GetSafeHtmlFragment(GetParam(args, "parent_node_id", True)))                
                tResult = String.Format("['{0}',[{1}]]", sAction, GetKnownLikelihoodsString(ParentNodeID))
            Case "set_known_likelihood"
                Dim ParentNodeID As Integer = CInt(EcSanitizer.GetSafeHtmlFragment(GetParam(args, "parent_node_id", True)))
                Dim NodeGuid As Guid = New Guid(EcSanitizer.GetSafeHtmlFragment(GetParam(args, "node_id", True)))
                Dim sValue As String = GetParam(args, "value", True)
                Dim Value As Double = 0
                Dim ParentNode As clsNode = PM.ActiveObjectives.GetNodeByID(ParentNodeID)
                If String2Double(sValue, Value) Then
                    SetKnownLikelihood(ParentNode.NodeGuidID, NodeGuid, Value)
                End If
                Dim NodeID As Integer = If(ParentNode.IsTerminalNode, PM.ActiveAlternatives.GetNodeByID(NodeGuid).NodeID, PM.ActiveObjectives.GetNodeByID(NodeGuid).NodeID)
                PM.PipeBuilder.PipeCreated = False  ' D7060
                tResult = String.Format("['{0}',[{1}],{2}]", sAction, GetKnownLikelihoodsString(ParentNodeID), GetNodeData(ParentNode, ParentNode.ParentNode)) 'TODO: DA_CH
        End Select

        If tResult <> "" Then
            Response.Clear()
            Response.ContentType = "text/plain"
            Response.Write(tResult)
            Response.End()
        End If
    End Sub

#Region "Paste From Clipboard Routines"

    Dim sItemsSeparator As String = "\t"
    Dim sCaretReturn As String = "\r\n"

    Dim templScaleName As String = "SCALE NAME:"

    Dim templUCIncreasing As String = "Increasing:"
    Dim templUCLinear As String = "Linear:"
    Dim templUCCurvature As String = "Curvature:"
    Dim templUCLow As String = "Low:"
    Dim templUCHigh As String = "High:"

    Dim templSFLinear As String = "Piecewise Linear:"

    Dim colName As String = "Name"
    Dim colValue As String = "Value"
    Dim colDescr As String = "Description"
    Dim colLow As String = "Low"

    Public Function ParseClipboardText(sText As String, Optional fParseCommas As Boolean = False) As String
        sText = sText.Replace("\""", """").Replace("\'", "'").Replace(vbNewLine, vbCr).Replace(vbCrLf, vbCr).Replace(vbLf, vbCr)
        If fParseCommas Then sText = sText.Replace(",", vbCr).Replace(";", vbCr)
        sText = sText.Replace(" " + vbCr, vbCr).Replace(vbCr + " ", vbCr)
        Return sText
    End Function

    Public Function ParseClipboardAsLines(sText As String, Optional fParseCommas As Boolean = False) As String()
        Return ParseClipboardText(sText, fParseCommas).Split(CChar(vbCr))
    End Function

    Private Sub DoPasteRS(RS As clsRatingScale, MeasureTypeID As ECMeasureType, ByVal ClipBoardText As String)
        If ClipBoardText Is Nothing OrElse ClipBoardText.Trim.Length = 0 Then Exit Sub

        Dim Intensities As New ArrayList
        Intensities.AddRange(RS.RatingSet)
        RS.RatingSet.Clear()

        Dim sList As String() = ParseClipboardAsLines(ClipBoardText)
        If sList.Count > 0 Then
            For Each item As String In sList
                Dim isHeaderRow As Boolean = item.Trim.Replace(vbTab, "").Replace(" ", "").ToLower = (colName + colValue + colDescr).ToLower
                Dim isScaleNameRow As Boolean = item.ToLower.Trim.StartsWith(templScaleName.ToLower)
                If ChangeMeasurementScaleNameWhenPasting AndAlso isScaleNameRow Then
                    RS.Name = item.Remove(0, templScaleName.Length).Replace(vbTab, "").Trim
                Else
                    If isHeaderRow OrElse isScaleNameRow Then Continue For
                    ' D2246 ===
                    'item = item.Trim()
                    ' try to spikt with tabs
                    Dim params As Char() = {CChar(vbTab)}
                    Dim tParts() As String = item.Trim.Split(params, StringSplitOptions.RemoveEmptyEntries)
                    ' if no tabs
                    If tParts.Count = 1 AndAlso Not MeasureTypeID = ECMeasureType.mtPWOutcomes Then
                        ' try to split with spaces and use last one part as number (no comments at all there)
                        tParts = item.Split()
                        If tParts.Count > 1 Then
                            Dim tmp() As String = {"", ""}
                            tmp(1) = tParts.Last
                            tParts(tParts.Count - 1) = ""
                            tmp(0) = String.Join(" ", tParts)
                            tParts = tmp
                        Else
                            Dim s As String() = {String.Join(" ", tParts), "0".ToString}
                            tParts = s
                        End If
                    End If

                    If tParts.Count = 2 AndAlso MeasureTypeID = ECMeasureType.mtPWOutcomes Then
                        Dim s As String() = {"intensity name", tParts(0), tParts(1)}
                        tParts = s
                    End If

                    If tParts.Count = 1 Then
                        If MeasureTypeID <> ECMeasureType.mtPWOutcomes Then
                            Dim s As String() = {tParts(0), "0"}
                            tParts = s
                        Else
                            Dim s As String() = {"intensity name", tParts(0)}
                            tParts = s
                        End If
                    End If

                    ' if have name, value, optional comment
                    If tParts.Count > 1 Then
                        Dim sNamePart As String = CStr(IIf(MeasureTypeID <> ECMeasureType.mtPWOutcomes OrElse tParts.Count > 2, tParts(0).Trim, "intensity name")) 'A0881
                        Dim sValuePart As String = tParts(1).Trim
                        'If MeasureTypeID = ECMeasureType.mtPWOutcomes AndAlso tParts.Count = 2 Then sValuePart = tParts(1) 'A0881
                        Dim sComment As String = ""
                        If tParts.Count > 2 Then sComment = tParts(2).Trim
                        'If MeasureTypeID = ECMeasureType.mtPWOutcomes AndAlso tParts.Count = 2 Then sComment = tParts(1) 'A0881
                        ' D2246 ==

                        If Not (sValuePart.Length > 0) Then
                            sValuePart = "0"
                        End If

                        If Not (sNamePart.Length > 0) Then
                            sNamePart = sValuePart
                            sValuePart = "0"
                        End If

                        Dim tDblValue As Double = 0
                        If Not String2Double(sValuePart, tDblValue) Then
                            sNamePart += " " + sValuePart
                        End If

                        If tDblValue < 0 Then tDblValue = 0
                        If tDblValue > 1 Then tDblValue = 1
                        Dim rsi As New clsRating()
                        rsi.GuidID = Guid.NewGuid()
                        rsi.Name = sNamePart
                        rsi.Value = CSng(tDblValue)
                        rsi.Comment = sComment

                        Dim exIntensity As clsRating = Nothing
                        For each intensity as clsRating In Intensities
                            If intensity.Name.Trim = sNamePart.Trim Then 
                                exIntensity = intensity
                            End If
                        Next

                        If exIntensity IsNot Nothing Then 
                            rsi.GuidID = exIntensity.GuidID
                        End If

                        RS.RatingSet.Add(rsi)
                    End If
                End If
            Next
        End If
    End Sub

    Private Sub DoPasteSF(sf As clsStepFunction, ByVal ClipBoardText As String)
        If ClipBoardText Is Nothing OrElse ClipBoardText.Trim.Length = 0 Then Exit Sub

        Dim sList As String()
        Dim params As Char() = {CChar(vbLf)}
        sList = ClipBoardText.Trim.Split(params, StringSplitOptions.RemoveEmptyEntries)  ' D4324
        If sList.Count > 0 Then
            Dim intervals As New ArrayList
            intervals.AddRange(sf.Intervals)
            sf.Intervals.Clear()

            For Each item As String In sList
                Dim isHeader As Boolean = item.Trim.Replace(vbTab, "").Replace(" ", "").ToLower = (colName + colLow + colValue).ToLower
                Dim isScaleNameRow As Boolean = item.ToLower.Trim.StartsWith(templScaleName.ToLower)
                If item.ToLower.Trim.StartsWith(templSFLinear.ToLower) OrElse isHeader Then
                    If Not isHeader Then
                        sf.IsPiecewiseLinear = GetBoolParam(item, templSFLinear)
                    End If
                Else
                    If ChangeMeasurementScaleNameWhenPasting AndAlso item.Trim.StartsWith(templScaleName) Then
                        Dim sName = item.Remove(0, templScaleName.Length).Replace(vbTab, "").Trim
                        sf.Name = sName
                    Else
                        If isHeader OrElse isScaleNameRow Then Continue For
                        ' D2246 ===
                        Dim params1 As Char() = {CChar(vbTab)}
                        Dim tParts() As String = item.Trim.Split(params1, StringSplitOptions.RemoveEmptyEntries)
                        ' if no tabs
                        If tParts.Count = 1 Then
                            tParts = item.Split()
                            If tParts.Count > 2 Then
                                Dim tmp() As String = {"", "", ""}
                                tmp(2) = tParts(tParts.Count - 1)
                                tParts(tParts.Count - 1) = ""
                                tmp(1) = tParts(tParts.Count - 2)
                                tParts(tParts.Count - 2) = ""
                                tmp(0) = String.Join(" ", tParts)
                                tParts = tmp
                            Else
                                Dim s As String() = {String.Join(" ", tParts), "0", "0"}
                                tParts = s
                            End If
                        End If

                        If tParts.Count = 1 Then
                            Dim s As String() = {tParts(0), "0", "0"}
                            tParts = s
                        Else
                            If tParts.Count = 2 Then
                                Dim s As String() = {tParts(0), tParts(1), "0"}
                                tParts = s
                            End If
                        End If

                        If tParts.Count > 2 Then
                            Dim sNamePart As String = tParts(0).Trim
                            Dim sLowPart As String = tParts(1).Trim
                            Dim sPrtyPart As String = tParts(2).Trim

                            If Not (sLowPart.Length > 0) Then sLowPart = "0"
                            If Not (sPrtyPart.Length > 0) Then sPrtyPart = "0"

                            If Not (sNamePart.Length > 0) Then
                                sNamePart = sLowPart
                                sLowPart = "0"
                                sPrtyPart = "0"
                            End If

                            Dim tLowValue As Double = 0
                            If Not String2Double(sLowPart, tLowValue) Then
                                sNamePart += " " + sLowPart
                            End If

                            Dim tPrtyValue As Double = 0
                            If Not String2Double(sPrtyPart, tPrtyValue) Then
                                sNamePart += " " + sPrtyPart
                            End If

                            If tPrtyValue < 0 Then tPrtyValue = 0
                            If tPrtyValue > 1 Then tPrtyValue = 1

                            Dim sfi As New clsStepInterval
                            sfi.Name = sNamePart
                            sfi.Low = CSng(tLowValue)
                            sfi.High = 0
                            sfi.Value = CSng(tPrtyValue)
                            sfi.Comment = ""

                            Dim exInterval As clsStepInterval = Nothing
                            For each Interval as clsStepInterval In intervals
                                If Interval.Name.Trim = sNamePart.Trim Then 
                                    exInterval = Interval
                                End If
                            Next

                            If exInterval IsNot Nothing Then 
                                sfi.Comment = exInterval.Comment
                            End If

                            If sfi.Comment = "" Then sfi.Comment = (Guid.NewGuid()).ToString

                            sf.Intervals.Add(sfi)
                        End If
                    End If
                End If
            Next
        End If
    End Sub

    Private Function GetBoolParam(item As String, templ As String) As Boolean
        If item.ToLower.Trim.StartsWith(templ.ToLower) Then
            Dim sBool As String = item.Trim.Remove(0, templ.Length).Replace(vbTab, "").Trim
            Return Str2Bool(sBool)
        End If
        Return False
    End Function

    Private Function GetStrParam(item As String, templ As String) As String
        If item.ToLower.Trim.StartsWith(templ.ToLower) Then
            Dim s As String = item.Trim.Remove(0, templ.Length).Replace(vbTab, "").Trim
            Return s
        End If
        Return ""
    End Function

    Private Function GetDblParam(item As String, templ As String) As Double
        Dim d As Double = 0
        If item.ToLower.Trim.StartsWith(templ.ToLower) Then
            String2Double(item.Trim.Remove(0, templ.Length).Replace(vbTab, "").Trim, d)
        End If
        Return d
    End Function

    Private Sub DoPasteUC(uc As clsRegularUtilityCurve, ByVal ClipBoardText As String)
        If ClipBoardText Is Nothing OrElse ClipBoardText.Trim.Length = 0 Then Exit Sub

        Dim sList As String()
        Dim params As Char() = {CChar(vbLf)}
        sList = ClipBoardText.Trim.Split(params, StringSplitOptions.RemoveEmptyEntries)  ' D4324
        If sList.Count > 0 Then            
            For Each item As String In sList
                If ChangeMeasurementScaleNameWhenPasting AndAlso item.ToLower.Trim.StartsWith(templScaleName.ToLower) Then uc.Name = GetStrParam(item, templScaleName)

                If item.ToLower.Trim.StartsWith(templUCIncreasing.ToLower) Then uc.IsIncreasing = GetBoolParam(item, templUCIncreasing)
                If item.ToLower.Trim.StartsWith(templUCCurvature.ToLower) Then uc.Curvature = CSng(GetDblParam(item, templUCCurvature))
                If item.ToLower.Trim.StartsWith(templUCLinear.ToLower) Then uc.IsLinear = GetBoolParam(item, templUCLinear)
                If item.ToLower.Trim.StartsWith(templUCLow.ToLower) Then uc.Low = CSng(GetDblParam(item, templUCLow))
                If item.ToLower.Trim.StartsWith(templUCHigh.ToLower) Then uc.High = CSng(GetDblParam(item, templUCHigh))
            Next
        End If
    End Sub

#End Region

    Public Function GetKnownLikelihoodsString(NodeID As Integer) As String
        Dim kl As List(Of KnownLikelihoodDataContract) = PM.ActiveObjectives.GetNodeByID(NodeID).GetKnownLikelihoods()
        Dim sKnownItems As String = ""
        For Each item In kl
            sKnownItems += If(sKnownItems = "", "", ",") + String.Format("{{""ID"":""{0}"",""name"":""{1}"",""value"":{2}}}", item.GuidID, JS_SafeString(item.NodeName), If(item.Value > 0, item.Value, 0))
        Next
        Return sKnownItems
    End Function

    Public Sub SetKnownLikelihood(ParentNodeID As Guid, NodeID As Guid, Value As Double)
        Dim ParentNode As ECCore.clsNode = PM.ActiveObjectives.GetNodeByID(ParentNodeID)
        Dim nodes As List(Of ECCore.clsNode)
        If Not ParentNode.IsTerminalNode Then
            nodes = ParentNode.GetNodesBelow(UNDEFINED_USER_ID)
        Else
            nodes = PM.ActiveAlternatives.TerminalNodes
        End If

        If nodes IsNot Nothing Then
            For Each nd As ECCore.clsNode In nodes
                PM.Attributes.SetAttributeValue(ATTRIBUTE_KNOWN_VALUE_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtDouble, If(nd.NodeGuidID.Equals(NodeID), Value, Nothing), nd.NodeGuidID, ParentNodeID)
            Next
        End If

        With PM
            .Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID, UNDEFINED_USER_ID)
        End With
    End Sub

End Class