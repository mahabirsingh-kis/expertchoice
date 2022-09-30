
Namespace ExpertChoice.Web.Controls

    Partial Public Class ctrlPairwiseBar

        Inherits ctrlEvaluationControlBase

        ' D1837 ===
        Public Const pwUndefinedValue As Long = TeamTimeFuncs.UndefinedValue    ' D1909
        Public Const gpwPrecision As Integer = 4    ' D1848
        ' D0143 ===
        Public Const gpwValueCoeff As Single = 1000
        Private Const _gpwEqualValue As Integer = CInt(99 * gpwValueCoeff)
        ' D1837 ==
        Public Const gpwBarWidth As Integer = 250
        ' D0143 ==
        Public Const MapName As String = "pwmap"    ' D0216
        Private _PWImageMap As String = ""          ' D0216

        Public GPWModeStrict As Boolean = TeamTimeFuncs.GPW_Mode_Strict    ' D2148

        Private _PWType As PairwiseType = PairwiseType.ptVerbal ' D0138
        Private _GPWMode As GraphicalPairwiseMode = TeamTimeFuncs.GPW_Mode_Default    ' D2088 + D2148
        Private _data As clsPairwiseMeasureData = Nothing
        Private _imgScale As Image   ' D0090
        Private _imgScalePath As String = ""    ' D0171
        Private _PWExtremeWarning As Boolean = False        ' D1322
        Private _msgPWExtreme As String = "Extreme value"   ' D1322
        Private _msgWrongNumber As String = "Wrong number"              ' D1111;
        Private _msgWrongNumberPart As String = "Wrong number"          ' D1116;
        ' D0995 ===
        Private _Caption As String = "" ' D4112
        Private _GoalName As String = ""
        Private _LeftNodeName As String = ""
        Private _RightNodeName As String = ""
        Private _LeftNodeComment As String = "" ' D2996
        Private _RightNodeComment As String = "" ' D2996
        Private _GoalInfodoc As String = ""
        Private _LeftNodeInfodoc As String = ""
        Private _RightNodeInfodoc As String = ""
        ' D0995 ==
        Private _LeftNodeWRTInfodoc As String = ""              ' D0996 + D1001
        Private _RightNodeWRTInfodoc As String = ""             ' D0996 + D1001
        ' D0204 ===
        Private _FramedInfodocs As Boolean = False
        Private _GoalNodeInfodocURL As String = ""              ' D0688
        Private _GoalNodeInfodocEditURL As String = ""          ' D0995
        Private _LeftNodeInfodocURL As String = ""
        Private _LeftNodeInfodocEditURL As String = ""          ' D0995
        Private _RightNodeInfodocURL As String = ""
        Private _RightNodeInfodocEditURL As String = ""         ' D0995
        ' D0996 ===
        Private _LeftNodeWRTInfodocURL As String = ""
        Private _LeftNodeWRTInfodocEditURL As String = ""
        Private _RightNodeWRTInfodocURL As String = ""
        Private _RightNodeWRTInfodocEditURL As String = ""
        ' D0996 ==
        Private _isInfodocGoalCollapsed As Boolean = False      ' D0688
        Private _isInfodocLeftCollapsed As Boolean = False      ' D0688
        Private _isInfodocRightCollapsed As Boolean = False     ' D0688
        Private _isInfodocLeftWRTCollapsed As Boolean = False   ' D0996
        Private _isInfodocRightWRTCollapsed As Boolean = False  ' D0996
        ' D0204 ==
        Private _ShowWRTInfodocs As Boolean = False             ' D1011
        Public _Width As String = "100%"   ' D0256

        Public KnownLikelihoodA As Double = -1      ' D2737
        Public KnownLikelihoodB As Double = -1      ' D2737
        Public KnownLikelihoodTitle As String = "Known Likelihood: "  ' D2737

        Public sRootPath As String = ""     ' D1593

        Public VerbalHints() As String = {"Equal", "Moderately", "Strongly", "Very Strongly", "Extremely", "Extremely"} ' D1318
        Public VerbalHintBetween As String = "Between {0} and {1}"   ' D0031

        Public Property PWType() As PairwiseType    ' D0138
            Get
                Return _PWType
            End Get
            Set(ByVal value As PairwiseType)
                _PWType = value
            End Set
        End Property

        ' D2088 ===
        Public Property GPWMode As GraphicalPairwiseMode
            Get
                Return _GPWMode
            End Get
            Set(value As GraphicalPairwiseMode)
                _GPWMode = value
            End Set
        End Property
        ' D2088 ==

        ' D0216 ===
        Public Property PWImageMap() As String
            Get
                Return _PWImageMap
            End Get
            Set(ByVal value As String)
                _PWImageMap = value
            End Set
        End Property
        ' D0216 ==

        ' D0171 ===
        Public Property ImgScalePath() As String
            Get
                Return _imgScalePath
            End Get
            Set(ByVal value As String)
                _imgScalePath = value
            End Set
        End Property
        ' D0171 ==

        Public Property LeftNodeName() As String
            Get
                Return _LeftNodeName    ' D0995
            End Get
            Set(ByVal value As String)
                _LeftNodeName = value   ' D0995
            End Set
        End Property

        Public Property RightNodeName() As String
            Get
                Return _RightNodeName   ' D0995
            End Get
            Set(ByVal value As String)
                _RightNodeName = value
            End Set
        End Property

        ' D2996 ===
        Public Property LeftNodeComment() As String
            Get
                Return _LeftNodeComment
            End Get
            Set(ByVal value As String)
                _LeftNodeComment = value
            End Set
        End Property

        Public Property RightNodeComment() As String
            Get
                Return _RightNodeComment
            End Get
            Set(ByVal value As String)
                _RightNodeComment = value
            End Set
        End Property
        ' D2996 ==

        ' D0256 ===
        Public Property Width() As String
            Get
                Return _Width
            End Get
            Set(ByVal value As String)
                _Width = value
            End Set
        End Property
        ' D0256 ==

        ' D2048 ===
        Public WriteOnly Property Message As String
            Set(value As String)
                lblMessage.Text = value
                'lblMessage.Visible = value <> ""   ' -D4614
            End Set
        End Property
        ' D2048 ==

        ' D1111 ===
        Public Property msgWrongNumber() As String
            Get
                Return String.Format(_msgWrongNumber, GetSliderMax)     ' D2090
            End Get
            Set(ByVal value As String)
                _msgWrongNumber = value
            End Set
        End Property
        ' D1111 ===

        ' D1116 ===
        Public Property msgWrongNumberPart() As String
            Get
                Return _msgWrongNumberPart
            End Get
            Set(ByVal value As String)
                _msgWrongNumberPart = value
            End Set
        End Property
        ' D1116 ===

        ' D1322 ===
        Public Property msgPWExtreme() As String
            Get
                Return _msgPWExtreme
            End Get
            Set(ByVal value As String)
                _msgPWExtreme = value
            End Set
        End Property

        Public Property ShowPWExtremeWarning() As Boolean
            Get
                Return _PWExtremeWarning
            End Get
            Set(ByVal value As Boolean)
                _PWExtremeWarning = value
            End Set
        End Property
        ' D1322 ==

        ' D4112 ===
        Public Property Caption() As String
            Get
                Return _Caption
            End Get
            Set(ByVal value As String)
                _Caption = value
            End Set
        End Property
        ' D4112 ==

        ' D0023 ===
        Public Property GoalName() As String    ' D0995
            Get
                Return _GoalName    ' D0995
            End Get
            Set(ByVal value As String)
                _GoalName = value   ' D0995
            End Set
        End Property

        ' D0106 ===
        Public Property GoalInfodoc() As String ' D0995
            Get
                Return _GoalInfodoc     ' D0108 + D0995
            End Get
            Set(ByVal value As String)
                _GoalInfodoc = value    ' D0995
            End Set
        End Property

        Public Property LeftNodeInfodoc() As String
            Get
                Return _LeftNodeInfodoc     ' D0995
            End Get
            Set(ByVal value As String)
                _LeftNodeInfodoc = value    ' D0995
            End Set
        End Property

        Public Property RightNodeInfodoc() As String
            Get
                Return _RightNodeInfodoc    ' D0995
            End Get
            Set(ByVal value As String)
                _RightNodeInfodoc = value   ' D0995
            End Set
        End Property

        ' D1011 ===
        Public Property ShowWRTInfodocs() As Boolean
            Get
                Return _ShowWRTInfodocs
            End Get
            Set(ByVal value As Boolean)
                _ShowWRTInfodocs = value
            End Set
        End Property
        ' D1011 ==

        ' D0996 + D1001 ===
        Public Property LeftNodeWRTInfodoc() As String
            Get
                Return _LeftNodeWRTInfodoc
            End Get
            Set(ByVal value As String)
                _LeftNodeWRTInfodoc = value
            End Set
        End Property

        Public Property RightNodeWRTInfodoc() As String
            Get
                Return _RightNodeWRTInfodoc
            End Get
            Set(ByVal value As String)
                _RightNodeWRTInfodoc = value
            End Set
        End Property
        ' D0996 + D1001 ==

        ' D0688 ===
        Public Property GoalNodeInfodoc_URL() As String
            Get
                Return _GoalNodeInfodocURL
            End Get
            Set(ByVal value As String)
                _GoalNodeInfodocURL = value
            End Set
        End Property
        ' D0688 ==

        ' D0995 ===
        Public Property GoalNodeInfodoc_EditURL() As String
            Get
                Return _GoalNodeInfodocEditURL
            End Get
            Set(ByVal value As String)
                _GoalNodeInfodocEditURL = value
            End Set
        End Property
        ' D0995 ==

        ' D0204 ===
        Public Property LeftNodeInfodoc_URL() As String
            Get
                Return _LeftNodeInfodocURL
            End Get
            Set(ByVal value As String)
                _LeftNodeInfodocURL = value
            End Set
        End Property

        ' D0995 ===
        Public Property LeftNodeInfodoc_EditURL() As String
            Get
                Return _LeftNodeInfodocEditURL
            End Get
            Set(ByVal value As String)
                _LeftNodeInfodocEditURL = value
            End Set
        End Property
        ' D0995 ==

        Public Property RightNodeInfodoc_URL() As String
            Get
                Return _RightNodeInfodocURL
            End Get
            Set(ByVal value As String)
                _RightNodeInfodocURL = value
            End Set
        End Property

        ' D0995 ===
        Public Property RightNodeInfodoc_EditURL() As String
            Get
                Return _RightNodeInfodocEditURL
            End Get
            Set(ByVal value As String)
                _RightNodeInfodocEditURL = value
            End Set
        End Property
        ' D0995 ==

        ' D0996 ===
        Public Property LeftNodeWRTInfodoc_URL() As String
            Get
                Return _LeftNodeWRTInfodocURL
            End Get
            Set(ByVal value As String)
                _LeftNodeWRTInfodocURL = value
            End Set
        End Property

        Public Property LeftNodeWRTInfodoc_EditURL() As String
            Get
                Return _LeftNodeWRTInfodocEditURL
            End Get
            Set(ByVal value As String)
                _LeftNodeWRTInfodocEditURL = value
            End Set
        End Property

        Public Property RightNodeWRTInfodoc_URL() As String
            Get
                Return _RightNodeWRTInfodocURL
            End Get
            Set(ByVal value As String)
                _RightNodeWRTInfodocURL = value
            End Set
        End Property

        Public Property RightNodeWRTInfodoc_EditURL() As String
            Get
                Return _RightNodeWRTInfodocEditURL
            End Get
            Set(ByVal value As String)
                _RightNodeWRTInfodocEditURL = value
            End Set
        End Property
        ' D0996 ==
        ' D0204 ==

        Public Property isInfodocGoalCollapsed() As Boolean
            Get
                Return _isInfodocGoalCollapsed
            End Get
            Set(ByVal value As Boolean)
                _isInfodocGoalCollapsed = value
            End Set
        End Property

        Public Property isInfodocLeftCollapsed() As Boolean
            Get
                Return _isInfodocLeftCollapsed
            End Get
            Set(ByVal value As Boolean)
                _isInfodocLeftCollapsed = value
            End Set
        End Property

        Public Property isInfodocRightCollapsed() As Boolean
            Get
                Return _isInfodocRightCollapsed
            End Get
            Set(ByVal value As Boolean)
                _isInfodocRightCollapsed = value
            End Set
        End Property
        ' D0688 ==

        ' D0996 ===
        Public Property isInfodocLeftWRTCollapsed() As Boolean
            Get
                Return _isInfodocLeftWRTCollapsed
            End Get
            Set(ByVal value As Boolean)
                _isInfodocLeftWRTCollapsed = value
            End Set
        End Property

        Public Property isInfodocRightWRTCollapsed() As Boolean
            Get
                Return _isInfodocRightWRTCollapsed
            End Get
            Set(ByVal value As Boolean)
                _isInfodocRightWRTCollapsed = value
            End Set
        End Property
        ' D0996 ==

        Public Property Data() As clsPairwiseMeasureData
            Get
                Return _data
            End Get
            Set(ByVal value As clsPairwiseMeasureData)
                _data = value
                'btnUndefined.Visible = False 
            End Set
        End Property

        Public Function GetPWValue() As Object
            If Data Is Nothing Then Return CSng(IIf(PWType = PairwiseType.ptGraphical, GpwEqualValue, pwUndefinedValue))
            Return IIf(Data.IsUndefined, pwUndefinedValue, Data.Value)
        End Function

        ' D2090 ===
        Public ReadOnly Property GetSliderMax As Integer
            Get
                If GPWMode = GraphicalPairwiseMode.gpwmLessThan9 Then Return 9 Else Return 99
            End Get
        End Property

        Public ReadOnly Property GetSliderMaxReal As Integer
            Get
                Select Case GPWMode
                    Case GraphicalPairwiseMode.gpwmLessThan9
                        Return 9
                    Case GraphicalPairwiseMode.gpwmLessThan99
                        Return 99
                    Case Else
                        Return 999
                End Select
            End Get
        End Property

        Public Shared ReadOnly Property GpwEqualValue As Integer
            Get
                Return _gpwEqualValue
            End Get
        End Property
        ' D2090 ==

        ' D3010 ===
        Public Function GetPWBars() As String
            Dim sBars As String = ""
            If PWType <> PairwiseType.ptGraphical Then
                ' create bars line
                Dim _Hint As String

                For i As Integer = -8 To 8

                    If PWType = PairwiseType.ptNumerical Then   ' D0138
                        _Hint = CStr(IIf(i = 0, "1", Math.Abs(i) + 1))
                    Else
                        _Hint = VerbalHints(Math.Abs(i) \ 2)
                        If Math.Abs(i) = 8 Then _Hint = VerbalHints(5) ' D1318
                        ' D0031 ===
                        If i Mod 2 <> 0 Then
                            Dim _Hint2 As String = VerbalHints(1 + Math.Abs(i) \ 2)
                            ' D0250 ===
                            _Hint = String.Format(VerbalHintBetween, _Hint, _Hint2)
                            ' D0250 ==
                        End If
                        ' D0031 ==
                    End If

                    sBars += String.Format("<td style='width:{0}px'><div style='width:{0}px;text-align:{4}; background:#e0e0e0 url({3}bars/point.gif);' class='pw_bar' title='{2}' onclick='{6}'><img src='{3}bars/point.gif' id='ID{5}' name='pw{5}' width='{0}' height='{1}' border='0' style='display:none'/></div></td>", 14, 1, _Hint, ImagePath, IIf(i < 0, "right", "left"), i, String.Format("SetPW({0},{1});", Math.Abs(i) + 1, IIf(i = 0, 0, IIf(i < 0, 1, -1))))
                    'sBars += String.Format("<td style='width:{0}px'><div style='width:{0}px; height:{1}px; text-align:{4}; background:#e0e0e0 url({3}bars/point.gif);' class='pw_bar' title='{2}' onclick='{6}'><img src='{3}bars/point.gif' id='ID{5}' name='pw{5}' width='{0}' height='{1}' border='0' /></div></td>", 14, 10 * (Math.Abs(i) + 1), _Hint, ImagePath, IIf(i < 0, "right", "left"), i, String.Format("SetPW({0},{1});", Math.Abs(i) + 1, IIf(i = 0, 0, IIf(i < 0, 1, -1))))
                Next

                sBars = "<div align='center' style='text-align:center; padding:0px; margin:0px auto;'><table border=0 cellspacing=0 cellpadding=0 style='margin: auto auto 0px auto;'><tr valign=bottom>" + sBars + "</tr></table></div>"
            End If
            Return sBars
        End Function
        ' D3010 ==

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
            ' D0138 ===
            If PWType = PairwiseType.ptGraphical Then

                pnlGPW.Visible = True

                btnUndefined.OnClientClick = String.Format("SetGPW('{0}',0); return false;", pwUndefinedValue) ' D0256 + D1838 + D1846

                ScriptManager.RegisterOnSubmitStatement(Me, GetType(String), "CheckGPWForm", "return CheckGPWForm();")    'D1110
            Else
                pnlGPW.Visible = False

                btnUndefined.OnClientClick = String.Format("SetPW('{0}',-1); return false;", pwUndefinedValue) ' D0256 + D1846

                ' D0138 ==
                ' load scale for numeric or verbal
                _imgScale = New Image
                _imgScale.SkinID = CStr(IIf(PWType = PairwiseType.ptVerbal, "PWVerbalScaleImage", "PWNumericScaleImage"))   ' D0138
                ' D0171 ===
                If PWType = PairwiseType.ptVerbal Then
                    mapVerbalPW.Enabled = True
                    mapVerbalPW.Text = PWImageMap   ' D0216
                    _imgScale.Attributes.Add("usemap", String.Format("#{0}", MapName)) ' D0097
                End If
                ' D0171 ==
                phScale.Controls.Add(_imgScale)

                ScriptManager.RegisterOnSubmitStatement(Me, GetType(String), "CheckUndef", "return CheckPWForm();")    'D1110
            End If

            If Not Data Is Nothing Then
                ' D4970 ===
                If Not Data.IsUndefined AndAlso (Data.Value = 0 OrElse Data.Advantage = 0) Then
                    Data.Value = 1
                    Data.Advantage = 0
                End If
                ' D4970 ==
                ' D0142 ===
                If PWType = PairwiseType.ptGraphical Then
                    btnUndefined.Visible = False    ' D1887
                    Page.ClientScript.RegisterStartupScript(GetType(String), "InitGPW", String.Format("InitGPW({0},{1});", JS_SafeNumber(Data.Value), JS_SafeNumber(Data.Advantage)), True) ' D0256 + D1838 + D1847 + D1887
                Else
                    ' D0171 ===
                    If Not _imgScale Is Nothing Then
                        _imgScale.ImageUrl = ImgScalePath
                    End If
                    ' D0171 ==
                    If Not Data.IsUndefined Then Page.ClientScript.RegisterStartupScript(GetType(String), "InitPW", String.Format("UpdatePW('{0}',{1});", JS_SafeNumber(GetPWValue()), JS_SafeNumber(Data.Advantage)), True) ' D1846
                    Page.ClientScript.RegisterStartupScript(GetType(String), "InitPWBars", "InitPW();", True)   ' D3356
                End If
                ' D0142 ==
                CheckAndFlashWRTCaption(lblCaption.ClientID, Data.ParentNodeID.ToString) ' D1139 + D2503
            End If

            ' D0995 ===
            lblNode1.Text = LeftNodeName
            lblNode2.Text = RightNodeName
            lblCaption.Text = String.Format("<span style='padding:1px 3px'>{0}</span>", Caption)    ' D1138 + D4112
            lblCaption.Visible = Not ShowFramedInfodocs AndAlso (CanEditInfodocs OrElse GoalInfodoc <> "")      ' D4747
            ' D0998 + D1001 + D1002 + D1004 + D2994 ===
            InitInfodocImage(imgCaptionInfodoc, tooltipGoal, GoalInfodoc, GoalNodeInfodoc_EditURL, String.Format(lblInfodocTitleGoal, ShortString(GoalName, MaxLenInfodocCaption)), imageInfodocEmpty, GoalNodeInfodoc_URL) ' D1064
            InitInfodocImage(ImageLeft, tooltipLeft, LeftNodeInfodoc, LeftNodeInfodoc_EditURL, String.Format(lblInfodocTitleNode, ShortString(LeftNodeName, MaxLenInfodocCaption)), imageInfodocEmpty, LeftNodeInfodoc_URL) ' D1064
            If ShowWRTInfodocs Then InitInfodocImage(ImageLeftWRT, tooltipLeftWRT, LeftNodeWRTInfodoc, LeftNodeWRTInfodoc_EditURL, String.Format(lblInfodocTitleWRT, ShortString(LeftNodeName, MaxLenInfodocCaption - 5), ShortString(GoalName, MaxLenInfodocCaption - 5)), imageWRTInfodocEmpty, LeftNodeWRTInfodoc_URL) ' D1011 + D1064
            InitInfodocImage(ImageRight, tooltipRight, RightNodeInfodoc, RightNodeInfodoc_EditURL, String.Format(lblInfodocTitleNode, ShortString(RightNodeName, MaxLenInfodocCaption)), imageInfodocEmpty, RightNodeInfodoc_URL)   ' D1064
            If ShowWRTInfodocs Then InitInfodocImage(ImageRightWRT, tooltipRightWRT, RightNodeWRTInfodoc, RightNodeWRTInfodoc_EditURL, String.Format(lblInfodocTitleWRT, ShortString(RightNodeName, MaxLenInfodocCaption - 5), ShortString(GoalName, MaxLenInfodocCaption - 5)), imageWRTInfodocEmpty, RightNodeWRTInfodoc_URL) ' D1011 + D1064
            ' D0998 + D1001 + D1002 + D1004 + D2994 ==
            pnlComment.Visible = ShowComment
            'lblComment.Text = lblCommentTitle       ' D1002 -D1065
            btnUndefined.Text = lblEraseJudgment    ' D1002
            ' D0995 ==

            ' D1213 + D2994 ===
            If ShowFramedInfodocs Then
                InitRFrame(frmInfodocGoal, String.Format(lblInfodocTitleNode, ShortString(GoalName, MaxLenInfodocCaption, True)), GoalNodeInfodoc_URL, GoalNodeInfodoc_EditURL, "goal", isInfodocGoalCollapsed, isHTMLEmpty(GoalInfodoc), 650, 120, "", "pw_goal") ' D1421
                InitRFrame(frmInfodocLeft, String.Format(lblInfodocTitleNode, ShortString(LeftNodeName, MaxLenInfodocCaption, True)), LeftNodeInfodoc_URL, LeftNodeInfodoc_EditURL, "left", isInfodocLeftCollapsed, isHTMLEmpty(LeftNodeInfodoc), 255, 110, "frmInfodocRight", "pw_node")
                InitRFrame(frmInfodocWRTLeft, String.Format(lblInfodocTitleWRT, ShortString(LeftNodeName, MaxLenInfodocCaptionWRT, True), ShortString(GoalName, MaxLenInfodocCaptionWRT, True)), LeftNodeWRTInfodoc_URL, LeftNodeWRTInfodoc_EditURL, "left_wrt", isInfodocLeftWRTCollapsed, isHTMLEmpty(LeftNodeWRTInfodoc), 255, 110, "frmInfodocWRTRight", "pw_wrt")
                InitRFrame(frmInfodocRight, String.Format(lblInfodocTitleNode, ShortString(RightNodeName, MaxLenInfodocCaption, True)), RightNodeInfodoc_URL, RightNodeInfodoc_EditURL, "right", isInfodocRightCollapsed, isHTMLEmpty(RightNodeInfodoc), 255, 110, "frmInfodocLeft", "pw_node")
                InitRFrame(frmInfodocWRTRight, String.Format(lblInfodocTitleWRT, ShortString(RightNodeName, MaxLenInfodocCaptionWRT, True), ShortString(GoalName, MaxLenInfodocCaptionWRT, True)), RightNodeWRTInfodoc_URL, RightNodeWRTInfodoc_EditURL, "right_wrt", isInfodocRightWRTCollapsed, isHTMLEmpty(RightNodeWRTInfodoc), 255, 110, "frmInfodocWRTLeft", "pw_wrt")
            End If
            ' D1213 + D2994 ==
        End Sub

        ' D2737 ===
        Public Function GetKnownLikelihood(L As Double) As String
            Dim sLikelihood As String = ""
            If L >= 0 Then
                sLikelihood = String.Format("<div class='text' style='margin-top:1ex; color:#ffff99;'>{0} {1}</div>", KnownLikelihoodTitle, L.ToString("F" + gpwPrecision.ToString))
            End If
            Return sLikelihood
        End Function
        ' D2737 ==

        ' D2996 ===
        Public Function GetNodeComment(sVal As String) As String
            Dim sComment As String = ""
            If Not String.IsNullOrEmpty(sVal) Then
                sComment = String.Format("<div class='text small' style='font-weight:normal; margin-top:2px; color:#f5deb3;'>{0}</div>", sVal)
            End If
            Return sComment
        End Function
        ' D2996 ==

        Public Function CreateBar(ByVal img As String, ByVal pattern As String, ByVal sAlign As String) As String    ' D0142
            Return String.Format("<div class='ratio_bar' style='width:{4}px; text-align:{3};'><img src='{0}bars/{2}' width='{5}' height='10' name='{1}'></div>", ImagePath, img, pattern, sAlign, gpwBarWidth, CInt(gpwBarWidth / 2))    ' D0142 + D0143
        End Function

    End Class

End Namespace