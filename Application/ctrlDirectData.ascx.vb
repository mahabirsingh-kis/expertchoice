Namespace ExpertChoice.Web.Controls

    Partial Public Class ctrlDirectData

        Inherits ctrlEvaluationControlBase    ' D1010

        Private _Data As Object
        Private _precValue As Integer = 2
        Public _precCoeff As Integer = 100      ' D3023

        'Public _changeFlagID As String = ""
        'Private _onChangeAction As String = ""  ' D0255
        Private _Infodoc As String = ""
        'Private _comment As String = ""
        'Private _commentCaption As String = "Comment:"
        'Private _Header As String = ""      ' D1115 -D2181
        ' D1010 ===
        Private _Caption As String = ""
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
        ' D1010 ==
        Private _msgWrongNumber As String = "Wrong Number"
        Private _msgWrongNumberRange As String = "Number must be between 0 and 1"
        Private _msgPromt As String = "Please enter value between 0 and 1: "
        Private _btnNext As String = ""    ' D0786

        Public sRootPath As String = ""     ' D1593

        Public SliderWidth As Integer = 100 ' D3023

        Public Sub New()
            _Data = Nothing
            If Page Is Nothing Then AddHandler Load, AddressOf InitComponent Else AddHandler Page.Load, AddressOf InitComponent
        End Sub

        Public Property Data() As Object
            Get
                Return _Data
            End Get
            Set(ByVal value As Object)
                _Data = value
            End Set
        End Property

        '' D0255 ===
        'Public Property onChangeAction() As String
        '    Get
        '        Return _onChangeAction
        '    End Get
        '    Set(ByVal value As String)
        '        _onChangeAction = value
        '    End Set
        'End Property
        '' D0255 ==

        ' D0786 ===
        Public Property NextButtonClientID() As String
            Get
                Return _btnNext
            End Get
            Set(ByVal value As String)
                _btnNext = value
            End Set
        End Property
        ' D0786 ==

        'Public Property ChangeFlagID() As String
        '    Get
        '        Return _changeFlagID
        '    End Get
        '    Set(ByVal value As String)
        '        _changeFlagID = value
        '    End Set
        'End Property

        Public Property ValuePrecision() As Integer ' D0123
            Get
                Return _precValue
            End Get
            Set(ByVal value As Integer)
                _precValue = value
            End Set
        End Property

        'Public Property Comment() As String
        '    Get
        '        Return _comment
        '    End Get
        '    Set(ByVal value As String)
        '        _comment = value
        '    End Set
        'End Property

        'Public Property ShowComment() As Boolean
        '    Get
        '        Return pnlComment.Visible
        '    End Get
        '    Set(ByVal value As Boolean)
        '        pnlComment.Visible = value
        '    End Set
        'End Property

        'Public Property CommentCaption() As String
        '    Get
        '        Return _commentCaption
        '    End Get
        '    Set(ByVal value As String)
        '        _commentCaption = value
        '    End Set
        'End Property

        Public Property msgWrongNumber() As String
            Get
                Return _msgWrongNumber
            End Get
            Set(ByVal value As String)
                _msgWrongNumber = value
            End Set
        End Property

        Public Property msgWrongNumberRange() As String
            Get
                Return _msgWrongNumberRange
            End Get
            Set(ByVal value As String)
                _msgWrongNumberRange = value
            End Set
        End Property

        Public Property Promt() As String
            Get
                Return _msgPromt
            End Get
            Set(ByVal value As String)
                _msgPromt = value
            End Set
        End Property

        ' -D2181
        '' D1115 ===
        'Public Property Header() As String
        '    Get
        '        Return _Header
        '    End Get
        '    Set(ByVal value As String)
        '        _Header = value
        '    End Set
        'End Property
        '' D1115 ==

        ' D1010 ===
        Public Property CaptionName() As String
            Get
                Return _Caption
            End Get
            Set(ByVal value As String)
                _Caption = value
            End Set
        End Property

        Public Property CaptionInfodoc() As String
            Get
                Return _CaptionInfodoc
            End Get
            Set(ByVal value As String)
                _CaptionInfodoc = value
            End Set
        End Property

        Public Property CaptionInfodocURL() As String
            Get
                Return _CaptionInfodocURL
            End Get
            Set(ByVal value As String)
                _CaptionInfodocURL = value
            End Set
        End Property

        Public Property CaptionInfodocCollapsed() As Boolean
            Get
                Return _CaptionInfodocCollapsed
            End Get
            Set(ByVal value As Boolean)
                _CaptionInfodocCollapsed = value
            End Set
        End Property

        Public Property CaptionInfodocEditURL() As String
            Get
                Return _CaptionInfodocEditURL
            End Get
            Set(ByVal value As String)
                _CaptionInfodocEditURL = value
            End Set
        End Property

        Public Property AlternativeName() As String
            Get
                Return _AlternativeName
            End Get
            Set(ByVal value As String)
                _AlternativeName = value
            End Set
        End Property

        Public Property AlternativeInfodoc() As String
            Get
                Return _AlternativeInfodoc
            End Get
            Set(ByVal value As String)
                _AlternativeInfodoc = value
            End Set
        End Property

        Public Property AlternativeInfodocURL() As String
            Get
                Return _AlternativeInfodocURL
            End Get
            Set(ByVal value As String)
                _AlternativeInfodocURL = value
            End Set
        End Property

        Public Property AlternativeInfodocCollapsed() As Boolean
            Get
                Return _AlternativeInfodocCollapsed
            End Get
            Set(ByVal value As Boolean)
                _AlternativeInfodocCollapsed = value
            End Set
        End Property

        Public Property AlternativeInfodocEditURL() As String
            Get
                Return _AlternativeInfodocEditURL
            End Get
            Set(ByVal value As String)
                _AlternativeInfodocEditURL = value
            End Set
        End Property

        Public Property WRTInfodoc() As String
            Get
                Return _WRTInfodoc
            End Get
            Set(ByVal value As String)
                _WRTInfodoc = value
            End Set
        End Property

        Public Property WRTInfodocURL() As String
            Get
                Return _WRTInfodocURL
            End Get
            Set(ByVal value As String)
                _WRTInfodocURL = value
            End Set
        End Property

        Public Property WRTInfodocCollapsed() As Boolean
            Get
                Return _WRTInfodocCollapsed
            End Get
            Set(ByVal value As Boolean)
                _WRTInfodocCollapsed = value
            End Set
        End Property

        Public Property WRTInfodocEditURL() As String
            Get
                Return _WRTInfodocEditURL
            End Get
            Set(ByVal value As String)
                _WRTInfodocEditURL = value
            End Set
        End Property
        ' D1010 ==

        Private ReadOnly Property ActionData() As clsOneAtATimeEvaluationActionData 'C0464
            Get
                If Not Data Is Nothing Then Return CType(Data, clsOneAtATimeEvaluationActionData) Else Return Nothing 'C0464
            End Get
        End Property

        Private ReadOnly Property Judgment() As clsDirectMeasureData
            Get
                Dim Action As clsOneAtATimeEvaluationActionData = ActionData 'C0464
                If Not Action Is Nothing Then Return CType(ActionData.Judgment, clsDirectMeasureData) Else Return Nothing
            End Get
        End Property

        Public ReadOnly Property ValueStr() As String
            Get
                Dim Val As clsDirectMeasureData = Judgment
                If Not Val Is Nothing Then
                    If Not Val.IsUndefined Then
                        If CDbl(Val.ObjectValue) < 0 Then Return "0" ' D0167
                        If CDbl(Val.ObjectValue) > 1 Then Return "1" ' D0167
                        Return JS_SafeNumber(Math.Round(CDbl(Val.ObjectValue), _precValue))
                    End If
                End If
                Return ""
            End Get
        End Property

        Public Property Infodoc() As String
            Get
                Return _Infodoc
            End Get
            Set(ByVal value As String)
                _Infodoc = value
            End Set
        End Property

        Protected Sub InitComponent(ByVal sender As Object, ByVal e As EventArgs)
            If Not Data Is Nothing And Not lblCaption Is Nothing Then
                pnlComment.Visible = ShowComment
                lblPrompt.Text = Promt
                ' D1010 ===
                'lblComment.Text = lblCommentTitle  ' -D1065
                lblCaption.Text = CaptionName    ' D1011 + D1115 + D2181
                ' D2503 ===
                Dim sNodeID As String = ""
                If ActionData.Node IsNot Nothing Then sNodeID = ActionData.Node.NodeID.ToString
                If sNodeID = "" AndAlso ActionData.Assignment IsNot Nothing Then sNodeID = ActionData.Assignment.ObjectiveID.ToString.Replace("-", "") ' D3068

                CheckAndFlashWRTCaption(lblCaption.ClientID, sNodeID) ' D1141
                ' D2503 ==

                ' D2994 ===
                InitInfodocImage(imgCaptionInfodoc, tooltipGoal, CaptionInfodoc, CaptionInfodocEditURL, String.Format(lblInfodocTitleGoal, ShortString(CaptionName, MaxLenInfodocCaption)), imageInfodocEmpty, CaptionInfodocURL)  ' D1064
                If AlternativeName <> "" AndAlso AlternativeInfodocURL <> "" Then InitInfodocImage(imgAltInfodoc, tooltipAltInfodoc, AlternativeInfodoc, AlternativeInfodocEditURL, String.Format(lblInfodocTitleNode, ShortString(AlternativeName, MaxLenInfodocCaption)), imageInfodocEmpty, AlternativeInfodocURL)    ' D1064  + D6713
                If WRTInfodocURL <> "" OrElse WRTInfodocEditURL <> "" Then InitInfodocImage(imgWRTInfodoc, tooltipWRTInfodoc, WRTInfodoc, WRTInfodocEditURL, String.Format(lblInfodocTitleWRT, ShortString(AlternativeName, MaxLenInfodocCaption - 5), ShortString(CaptionName, MaxLenInfodocCaption - 5)), imageWRTInfodocEmpty, WRTInfodocURL) Else imgWRTInfodoc.Visible = False ' D1064 + D4346
                ' D2994 ==

                If imgAltInfodoc.Visible Then imgAltInfodoc.Attributes.Add("style", "margin-left:3px")
                ' D1010 ==

                ' D1214 ===
                Dim sCode As String = ""
                If ShowFramedInfodocs Then
                    ' D2994 ===
                    InitRFrame(frmInfodocGoal, String.Format(lblInfodocTitleGoal, ShortString(CaptionName, MaxLenInfodocCaption, True)), CaptionInfodocURL, CaptionInfodocEditURL, "node", CaptionInfodocCollapsed, CaptionInfodoc = "", 300, 90, "", "di_node")
                    If AlternativeName <> "" AndAlso AlternativeInfodocURL <> "" Then InitRFrame(frmInfodocNode, String.Format(lblInfodocTitleNode, ShortString(AlternativeName, MaxLenInfodocCaption, True)), AlternativeInfodocURL, AlternativeInfodocEditURL, "alt", AlternativeInfodocCollapsed, AlternativeInfodoc = "", 300, 90, "", "di_alt") Else frmInfodocNode.Visible = False   ' D6713
                    If WRTInfodocURL <> "" OrElse WRTInfodocEditURL <> "" Then InitRFrame(frmInfodocWRT, String.Format(lblInfodocTitleWRT, ShortString(AlternativeName, MaxLenInfodocCaptionWRT, True), ShortString(CaptionName, MaxLenInfodocCaptionWRT, True)), WRTInfodocURL, WRTInfodocEditURL, "wrt", WRTInfodocCollapsed, WRTInfodoc = "", 300, 90, "", "di_wrt") Else frmInfodocWRT.Visible = False ' D4346
                    ' D2994 ==
                End If
                If Not Page Is Nothing AndAlso Not IsPostBack Then
                    Page.ClientScript.RegisterStartupScript(GetType(String), "InitDD", sCode + "if (theForm.DDValue) setTimeout('theForm.DDValue.focus();InitSlider();', 500);", True)    ' D0317 + D1214 + D1784
                    Page.ClientScript.RegisterOnSubmitStatement(GetType(String), "CheckForm", "return CheckFormOnSubmit();")
                End If
                ' D0176 ==
            End If
        End Sub

    End Class

End Namespace
