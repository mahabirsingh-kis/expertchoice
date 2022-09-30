Namespace ExpertChoice.Web.Controls

    Partial Public Class ctrlUtilityCurve

        Inherits ctrlEvaluationControlBase    ' D1011

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
            If Not Data Is Nothing And Not lblCaption Is Nothing Then
                If ActionData.MeasurementType = ECMeasureType.mtRegularUtilityCurve Or ActionData.MeasurementType = ECMeasureType.mtStep Then pnlRUC.Visible = True ' D0173

                ' D1011 ===
                pnlComment.Visible = ShowComment
                lblCaption.Text = String.Format("&nbsp;{0}&nbsp;", CaptionName)     ' D1123 + D2181 + D4213
                lblCaption.Visible = (ParentNodeID <> "" OrElse CaptionName <> "")  ' D4213
                If ParentNodeID <> "" OrElse CaptionName <> "" Then CheckAndFlashWRTCaption(lblCaption.ClientID, ParentNodeID) ' D1141 + D2503 + D2505 + D2776

                ' D1009 ===
                If CaptionInfodocEditURL <> "" OrElse CaptionInfodocURL <> "" Then InitInfodocImage(imgCaptionInfodoc, tooltipGoal, CaptionInfodoc, CaptionInfodocEditURL, String.Format(lblInfodocTitleGoal, ShortString(CaptionName, MaxLenInfodocCaption)), imageInfodocEmpty, CaptionInfodocURL)  ' D1064 + D6713
                If AlternativeInfodocEditURL <> "" OrElse AlternativeInfodocURL <> "" Then InitInfodocImage(imgAltInfodoc, tooltipAltInfodoc, AlternativeInfodoc, AlternativeInfodocEditURL, String.Format(lblInfodocTitleNode, ShortString(AlternativeName, MaxLenInfodocCaption)), imageInfodocEmpty, AlternativeInfodocURL)    ' D1064  + D6713
                If WRTInfodocEditURL <> "" OrElse WRTInfodocURL <> "" Then InitInfodocImage(imgWRTInfodoc, tooltipWRTInfodoc, WRTInfodoc, WRTInfodocEditURL, String.Format(lblInfodocTitleWRT, ShortString(AlternativeName, MaxLenInfodocCaption - 5), ShortString(CaptionName, MaxLenInfodocCaption - 5)), imageWRTInfodocEmpty, WRTInfodocURL) Else imgWRTInfodoc.Visible = False ' D1064 + D4346
                If ScaleInfodocEditURL <> "" OrElse ScaleInfodocURL <> "" Then InitInfodocImage(imgScaleInfodoc, tooltipScaleInfodoc, ScaleInfodoc, ScaleInfodocEditURL, lblInfodocTitleScale, imageInfodocEmpty, ScaleInfodocURL) Else imgScaleInfodoc.Visible = False ' D4933

                If imgAltInfodoc.Visible Then imgAltInfodoc.Attributes.Add("style", "margin-left:3px")
                If imgScaleInfodoc.Visible Then imgScaleInfodoc.Attributes.Add("style", "float:left")    ' D4933
                ' D1011 ==

                ' D1214 ===
                Dim sCode As String = "InitFlash();"    ' D1295
                If ShowFramedInfodocs Then
                    If CaptionInfodocEditURL <> "" OrElse CaptionInfodocURL <> "" Then InitRFrame(frmInfodocGoal, String.Format(lblInfodocTitleGoal, ShortString(CaptionName, MaxLenInfodocCaption, True)), CaptionInfodocURL, CaptionInfodocEditURL, "node", CaptionInfodocCollapsed, isHTMLEmpty(CaptionInfodoc), 300, 90, "", "uc_node") Else frmInfodocGoal.Visible = False ' D6713
                    If AlternativeInfodocEditURL <> "" OrElse AlternativeInfodocURL <> "" Then InitRFrame(frmInfodocNode, String.Format(lblInfodocTitleNode, ShortString(AlternativeName, MaxLenInfodocCaption, True)), AlternativeInfodocURL, AlternativeInfodocEditURL, "alt", AlternativeInfodocCollapsed, isHTMLEmpty(AlternativeInfodoc), 300, 90, "", "uc_alt") Else frmInfodocNode.Visible = False    ' D6713
                    If WRTInfodocEditURL <> "" OrElse WRTInfodocURL <> "" Then InitRFrame(frmInfodocWRT, String.Format(lblInfodocTitleWRT, ShortString(AlternativeName, MaxLenInfodocCaptionWRT, True), ShortString(CaptionName, MaxLenInfodocCaptionWRT, True)), WRTInfodocURL, WRTInfodocEditURL, "wrt", WRTInfodocCollapsed, isHTMLEmpty(WRTInfodoc), 300, 90, "", "uc_wrt") Else frmInfodocWRT.Visible = False ' D4346
                    If ScaleInfodocEditURL <> "" OrElse ScaleInfodocURL <> "" Then InitRFrame(frmInfodocScale, lblInfodocTitleScale, ScaleInfodocURL, ScaleInfodocEditURL, "scale", ScaleInfodocCollapsed, isHTMLEmpty(ScaleInfodoc), 300, 90, "", "uc_scale") Else frmInfodocScale.Visible = False ' D4933
                End If
                ' D1214 ==
                If sCode <> "" Then Page.ClientScript.RegisterStartupScript(GetType(String), "InitFrames", String.Format("setTimeout(""{0}"", 500);", sCode), True) ' D1295

            End If
        End Sub

        Protected Sub Page_PreRender(sender As Object, e As EventArgs) Handles Me.PreRender
            If ActionData.MeasurementScale IsNot Nothing Then   ' D3472
                lblMessage.Visible = False  ' D3472
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
                        pnlRUC.Visible = True
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
                        pnlRUC.Visible = True
                End Select
                ' D3472 ===
            Else
                pnlRUC.Visible = False
                lblMessage.Text = "<div class='text error' style='border:1px solid #f0f0f0; padding:4em; text-align:center; font-weight:bold;'>Invalid measurement scale</div>"
                lblMessage.Visible = True
            End If
            ' D3472 ==
        End Sub
    End Class

End Namespace
