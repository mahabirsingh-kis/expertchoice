Imports Telerik.Web.UI

Namespace ExpertChoice.Web.Controls

    Partial Public Class ctrlMultiPairwise

        Inherits ctrlEvaluationControlBase

        Public Const pwUndefinedValue As Long = TeamTimeFuncs.UndefinedValue    ' D1909

        Private _Data As List(Of clsPairwiseLine)
        Private _Caption As String = ""
        Private _ParentNodeID As String = ""    ' D2503
        Private _CaptionName As String = ""
        Private _CaptionInfodoc As String = ""
        Private _CaptionInfodocURL As String = ""
        Private _CaptionInfodocEditURL As String = ""
        Private _isCaptionInfodocCollapsed As Boolean = False   ' D1165
        ' D4337 ===
        Public SecondNodeName As String = ""
        Public SecondNodeInfodoc As String = ""
        Public SecondNodeInfodocURL As String = ""
        Public SecondNodeInfodocEditURL As String = ""
        Public isSecondNodeMode As Boolean = False
        ' D4337 ==
        ' D1374 ===
        Private _InfodocsCollapsed As Boolean = False
        Public _LeftInfodocURLsList As String = ""
        Private _HasLeftInfodoc As Boolean = False  ' D4331
        Public _LeftWRTInfodocURLsList As String = ""
        Public _LeftInfodocTitlesList As String = ""
        Public _LeftInfodocTitlesWRTList As String = ""
        Public _LeftInfodocEditURLsList As String = ""
        Public _LeftWRTInfodocEditURLsList As String = ""
        Private _HasLeftWRTInfodoc As Boolean = False  ' D4331
        Public _RightInfodocURLsList As String = ""
        Private _HasRightInfodoc As Boolean = False  ' D4331
        Public _RightWRTInfodocURLsList As String = ""
        Public _RightInfodocTitlesList As String = ""
        Public _RightInfodocTitlesWRTList As String = ""
        Public _RightInfodocEditURLsList As String = ""
        Public _RightWRTInfodocEditURLsList As String = ""
        Private _HasRightWRTInfodoc As Boolean = False  ' D4331
        ' D1374 ==
        Public _TooltipsInfodocList As String = ""
        Public _TooltipsWRTList As String = ""
        Public _IDsList As String = ""       ' D1165 + D2016
        Public _ImagesList As String = ""
        Public _ImagesWRTList As String = ""
        Public _ImagesCommentsList As String = ""   ' D2725
        Private _ShowWRTInfodocs As Boolean = False
        Private _CaptionSaveComment As String = "OK"
        Private _CaptionCloseComment As String = "Close"    ' D1161
        Private _ImageCommentEmpty As String = "note_.gif"
        Private _ImageCommentExists As String = "note.gif"
        Private _PWType As PairwiseType = PairwiseType.ptVerbal     ' D1165
        Private _GPWMode As GraphicalPairwiseMode = TeamTimeFuncs.GPW_Mode_Default  ' D2148
        Public GPWModeStrict As Boolean = TeamTimeFuncs.GPW_Mode_Strict    ' D2148
        Public Const gpwPrecision As Integer = 4    ' D2738

        ' D1158 ===
        Private _BlankImage As String = "blank.gif"

        Public _BarWidth As Integer = 150
        Public _BarHeight As Integer = 9
        Public _BarStyle As String = "pw_edit"

        Public VerbalHints() As String = {"Equal", "Moderately", "Strongly", "Very Strongly", "Extremely", ""}  ' D1367
        Public VerbalHintBetween As String = "Between {0} and {1}"
        Public VerbalShortHints() As String = {"Eq", "M", "S", "VS", "E"}
        Public LegendReverseOrder As Boolean = False    ' D7566
        ' D1158 ==

        Public KnownLikelihoodTitle As String = "Known Likelihood: "  ' D2738

        ' D1165 ===
        Private _msgWrongNumber As String = "Wrong number"
        Private _msgWrongNumberPart As String = "Wrong number"
        ' D1165 ==

        Public Const MapName As String = "pwmap"    ' D1163
        Private _PWImageMap As String = ""          ' D1163

        ' D1374
        Public _FocusID As Integer = -1             ' D1425

        Public ShowOnlyFocusedRow As Boolean = False    ' D2677

        Public ShowFloatLegend As Boolean = False   ' D4320

        Public Const _HIDDEN As String = ctrlFramedInfodoc._HIDDEN
        Public Const _VISIBLE As String = ctrlFramedInfodoc._VISIBLE
        ' D1374 ==

        Public sRootPath As String = ""     ' D1593

        Public Property ParentNodeID() As String    ' D2503
            Get
                Return _ParentNodeID
            End Get
            Set(value As String)      ' D2503
                _ParentNodeID = value
            End Set
        End Property

        Public Property Caption() As String
            Get
                Return _Caption
            End Get
            Set(value As String)
                _Caption = value
            End Set
        End Property

        ' D1374 ===
        Public Property CaptionName() As String
            Get
                Return _CaptionName
            End Get
            Set(value As String)
                _CaptionName = value
            End Set
        End Property
        ' D1374 ==

        Public Property CaptionInfodoc() As String
            Get
                Return _CaptionInfodoc
            End Get
            Set(value As String)
                _CaptionInfodoc = value
            End Set
        End Property

        ' D2091 ===
        Public Property GPWMode As GraphicalPairwiseMode
            Get
                Return _GPWMode
            End Get
            Set(value As GraphicalPairwiseMode)
                _GPWMode = value
            End Set
        End Property
        ' D2091 ==

        ' D1165 ===
        Public Property PWType() As PairwiseType
            Get
                Return _PWType
            End Get
            Set(value As PairwiseType)
                _PWType = value
            End Set
        End Property

        Public Property isInfodocCaptionCollapsed() As Boolean
            Get
                Return _isCaptionInfodocCollapsed
            End Get
            Set(value As Boolean)
                _isCaptionInfodocCollapsed = value
            End Set
        End Property
        ' D1165 ==

        Public Property CaptionInfodocEditURL() As String
            Get
                Return _CaptionInfodocEditURL
            End Get
            Set(value As String)
                _CaptionInfodocEditURL = value
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

        Public Property CaptionSaveComment() As String
            Get
                Return _CaptionSaveComment
            End Get
            Set(value As String)
                _CaptionSaveComment = value
            End Set
        End Property

        ' D1161 ===
        Public Property CaptionCloseComment() As String
            Get
                Return _CaptionCloseComment
            End Get
            Set(value As String)
                _CaptionCloseComment = value
            End Set
        End Property
        ' D1161 ==

        ' D2048 ===
        Public WriteOnly Property Message As String
            Set(value As String)
                lblMessage.Text = value
            End Set
        End Property
        ' D2048 ==

        ' D1374 ===
        Public Property InfodocsCollapsed() As Boolean
            Get
                Return _InfodocsCollapsed
            End Get
            Set(value As Boolean)
                _InfodocsCollapsed = value
            End Set
        End Property
        ' D1374 ==

        Public Property ImageCommentExists() As String
            Get
                Return _ImageCommentExists
            End Get
            Set(value As String)
                _ImageCommentExists = value
            End Set
        End Property

        Public Property ImageCommentEmpty() As String
            Get
                Return _ImageCommentEmpty
            End Get
            Set(value As String)
                _ImageCommentEmpty = value
            End Set
        End Property

        Public Property Data() As List(Of clsPairwiseLine)
            Get
                Return _Data
            End Get
            Set(value As List(Of clsPairwiseLine))
                _Data = value
            End Set
        End Property

        Public Property ShowWRTInfodocs() As Boolean
            Get
                Return _ShowWRTInfodocs
            End Get
            Set(value As Boolean)
                _ShowWRTInfodocs = value
            End Set
        End Property

        ' D1165 ===
        Public Property msgWrongNumber() As String
            Get
                Return String.Format(_msgWrongNumber, GetSliderMax)     ' D2091
            End Get
            Set(value As String)
                _msgWrongNumber = value
            End Set
        End Property

        Public Property msgWrongNumberPart() As String
            Get
                Return _msgWrongNumberPart
            End Get
            Set(value As String)
                _msgWrongNumberPart = value
            End Set
        End Property
        ' D1165 ==

        ' D1158 ===
        Public Function GetHints() As String
            Dim sHints As String = ""
            For i As Integer = -8 To 8
                Dim Hint As String = VerbalHints(Math.Abs(i \ 2))
                If i Mod 2 <> 0 Then
                    Dim Hint2 As String = VerbalHints(Math.Abs(i \ 2) + 1)
                    Hint = CStr(IIf(i > 0, String.Format(VerbalHintBetween, Hint, Hint2), String.Format(VerbalHintBetween, Hint2, Hint)))
                End If
                If Not sHints = "" Then sHints += ", "
                sHints += String.Format("'{0}'", JS_SafeString(Hint))
            Next
            Return sHints
        End Function

        Public Property BlankImage() As String
            Get
                Return _BlankImage
            End Get
            Set(value As String)
                _BlankImage = value
            End Set
        End Property

        ' D1163 ===
        Public Property PWImageMap() As String
            Get
                Return _PWImageMap
            End Get
            Set(value As String)
                _PWImageMap = value
            End Set
        End Property
        ' D1163 ==

        ' D2048 ===
        Public Function IsWarning(Data As clsPairwiseLine) As String
            If PWType = PairwiseType.ptVerbal AndAlso Not Data.IsUndefined AndAlso ((Data.Value Mod 1) <> 0 OrElse Data.Value > 9) Then
                Return "1"
            Else
                Return "0"
            End If
        End Function
        ' D2048 ==

        ' D2091 ===
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
        ' D2091 ==

        ' D2031 ===
        Public Function GetInitValue(Data As clsPairwiseLine) As Double
            If Data Is Nothing OrElse Data.isUndefined Then
                Return pwUndefinedValue ' D2023
            Else
                Return Data.Value
            End If
        End Function
        ' D2031 ==

        Public Function GetValue(Data As clsPairwiseLine) As Double

            If Data Is Nothing OrElse Data.isUndefined Then
                Return pwUndefinedValue ' D2023
            Else
                ' D4970 ===
                If Data.Value = 0 OrElse Data.Advantage = 0 Then
                    Data.Value = 1
                    Data.Advantage = 0
                End If
                ' D4970 ==
                ' D1165 ===
                If PWType = PairwiseType.ptGraphical Then ' D0142
                    Return Data.Value   ' D2016
                Else
                    ' D2048 ===
                    Dim Val As Single = CSng(Math.Round(Data.Value))
                    If Val > 9 Then Val = 9
                    ' D1165 ==
                    Return CLng(IIf(Val = 1, 0, Data.Advantage * (Val - 1)))  ' D1435
                    ' D2048 ==
                End If
            End If
        End Function
        ' D1158 ==

        Protected Sub Page_Load(sender As Object, e As EventArgs)
            lblCaption.Text = Caption
            CheckAndFlashWRTCaption(lblCaption.ClientID, ParentNodeID)
            InitInfodocImage(imgCaptionInfodoc, tooltipGoal, CaptionInfodoc, CaptionInfodocEditURL, String.Format(lblInfodocTitleGoal, ShortString(CaptionName, MaxLenInfodocCaption)), imageInfodocEmpty, CaptionInfodocURL)
            If Data IsNot Nothing Then
                rptPW.DataSource = Data
                rptPW.DataBind()
                ' D1165 ===
                If PWType = PairwiseType.ptGraphical Then
                    Page.ClientScript.RegisterStartupScript(GetType(String), "InitSliders", "InitSliders();", True) ' D1166 + D2016
                    ScriptManager.RegisterOnSubmitStatement(Me, GetType(String), "CheckGPWForm", "return CheckGPWForm();")      'D1166
                Else
                    ScriptManager.RegisterOnSubmitStatement(Me, GetType(String), "CheckPWForm", "return CheckUndefined();")     'D1313
                    lblMessage.Attributes.Add("style", "display:none")
                    Page.ClientScript.RegisterStartupScript(GetType(String), "InitList", If(ShowFloatLegend, "showFloatLegend();", ""), True) ' D1166 + D2016 + D4192 + D4320 + D4373
                End If
                ' D1165 ==
                If Not divLegend.Visible AndAlso PWType = PairwiseType.ptVerbal Then
                    'divLegend.InnerHtml = "<table border=0 cellspacing=1 cellpadding=1 class='tbl' id='tblLegend' style='max-width:230px;'>"
                    divLegend.InnerHtml = "<table border=0 cellspacing=1 cellpadding=1 class='tbl' id='tblLegend'>"
                    Dim AltRow As Boolean = True
                    For i As Integer = VerbalShortHints.Length - 1 To 0 Step -1
                        Dim idx = If(LegendReverseOrder, VerbalShortHints.Length - i - 1, i)    ' D7566
                        AltRow = Not AltRow
                        divLegend.InnerHtml += String.Format("<tr class='tbl_row{0} tbl_margins text'><td align=center>{1}</td><td align=left>{2}</td></tr>", IIf(AltRow, "", "_alt"), VerbalShortHints(idx), VerbalHints(If(idx = VerbalShortHints.Length - 1, VerbalHints.Length - 1, idx))) ' D7566
                    Next
                    divLegend.InnerHtml += "</table>"
                    divLegend.Visible = True
                End If
            End If
        End Sub

        Protected Sub rptPW_ItemDataBound(sender As Object, e As RepeaterItemEventArgs)
            If Not e.Item.DataItem Is Nothing Then
                Dim PW As clsPairwiseLine = CType(e.Item.DataItem, clsPairwiseLine)

                ' D1165 ===
                ' D1186 ===
                If PWType = PairwiseType.ptGraphical Then
                    _IDsList += CStr(IIf(_IDsList = "", "", ",")) + String.Format("'{0}'", PW.ID)
                    Dim td As HtmlTableCell = CType(e.Item.FindControl("tdContentGPW"), HtmlTableCell)
                    ' D1186 ==
                    If td IsNot Nothing Then
                        td.Visible = True   ' D1186
                        '    Dim RadSliderGPW As RadSlider = CType(e.Item.FindControl("RadSliderGPW"), RadSlider)
                        '    If RadSliderGPW IsNot Nothing Then
                        '        _RadSlidersList += IIf(_RadSlidersList = "", "", ",") + "'" + RadSliderGPW.ClientID + "'"
                        '        RadSliderGPW.MinimumValue = gpwMinValue
                        '        RadSliderGPW.MaximumValue = gpwMaxValue
                        '        RadSliderGPW.LargeChange = CInt(Math.Round(gpwMaxValue / 9.9))
                        '        RadSliderGPW.SmallChange = CInt(Math.Round(gpwMaxValue / 999))

                        '        Dim Prg As Single = IIf(PW.Value = 1, 50, IIf(PW.Advantage > 0, 100 / (PW.Value + 1), (100 * PW.Value) / (PW.Value + 1)))
                        '        If PW.isUndefined Then Prg = 50
                        '        RadSliderGPW.Value = CInt(IIf(PW.isUndefined, gpwEqualValue, Math.Round(gpwValueCoeff * Prg)))
                        '        RadSliderGPW.Attributes.Add("onmouseup", String.Format("setTimeout('CheckSlider({0});', 50);", PW.ID))
                        '        RadSliderGPW.Visible = True ' D1186
                        '    End If
                        '    'btnUndefined.OnClientClick = String.Format("isChanged(); UpdateRatio({0}); SetGPWValue({0}); return false;", pwUndefinedValue) ' D0256
                    End If
                Else
                    ' D1186 ===
                    Dim td As HtmlTableCell = CType(e.Item.FindControl("tdContentVPW"), HtmlTableCell)
                    If td IsNot Nothing Then
                        td.InnerHtml = String.Format("<div style='height:16px'><img src='{2}' border='0' usemap='#{3}' id='imgScale{0}' width=272 height=17/><a href='' onclick='SwitchHelpTooltip(this.children[0].id, {4}); return false;'><img src='{2}' border=0 id='imgScaleHelp{0}' title='' width=16/></a></div><span id='PW{0}'><script language='JavaScript'> document.write(GetPairwise('{0}', {1})); </script></span>", PW.ID, JS_SafeNumber(GetValue(PW)), ImagePath + "blank.gif", MapName, IIf(e.Item.ItemIndex > 2, 11, 31)) ' D1436 + D4333 + D4373 + D6254
                        td.Visible = True   ' D1186
                    End If
                    ' D1186 ==
                End If
                ' D1165 ==

                ' D1163 ===
                Dim tdComment As HtmlTableCell = CType(e.Item.FindControl("tdComment"), HtmlTableCell)
                If tdComment IsNot Nothing Then tdComment.Visible = ShowComment
                ' D1163 ==

                If _FocusID < 0 AndAlso PW.isUndefined Then _FocusID = PW.ID ' D1379

                If Not isSecondNodeMode Then    ' D4337
                    ' D1374 ===
                    _LeftInfodocURLsList += CStr(IIf(_LeftInfodocURLsList = "", "", ",")) + "'" + JS_SafeString(IIf(PW.InfodocLeft = "" AndAlso Not CanEditInfodocs, "", PW.InfodocLeftURL)) + "'"    ' D2975
                    _LeftWRTInfodocURLsList += CStr(IIf(_LeftWRTInfodocURLsList = "", "", ",")) + "'" + JS_SafeString(IIf(PW.InfodocLeftWRT = "" AndAlso Not CanEditInfodocs, "", PW.InfodocLeftWRTURL)) + "'"    ' D2975
                    _LeftInfodocEditURLsList += CStr(IIf(_LeftInfodocEditURLsList = "", "", ",")) + """" + JS_SafeString(IIf(CanEditInfodocs, PW.InfodocLeftEditURL, String.Format("open_infodoc('{0}');", JS_SafeString(PW.InfodocLeftURL)))) + """" ' D1571
                    _LeftWRTInfodocEditURLsList += CStr(IIf(_LeftWRTInfodocEditURLsList = "", "", ",")) + """" + JS_SafeString(IIf(CanEditInfodocs, PW.InfodocLeftWRTEditURL, String.Format("open_infodoc('{0}');", JS_SafeString(PW.InfodocLeftWRTURL)))) + """" ' D1571
                    _LeftInfodocTitlesList += CStr(IIf(_LeftInfodocTitlesList = "", "", ",")) + "'" + JS_SafeString(String.Format(lblInfodocTitleNode, ShortString(PW.LeftNode, MaxLenInfodocCaption, False))) + "'"
                    _LeftInfodocTitlesWRTList += CStr(IIf(_LeftInfodocTitlesWRTList = "", "", ",")) + "'" + JS_SafeString(String.Format(lblInfodocTitleWRT, ShortString(PW.LeftNode, MaxLenInfodocCaptionWRT - 2, False), ShortString(CaptionName, MaxLenInfodocCaptionWRT - 2, False))) + "'"

                    _RightInfodocURLsList += CStr(IIf(_RightInfodocURLsList = "", "", ",")) + "'" + JS_SafeString(IIf(PW.InfodocRight = "" AndAlso Not CanEditInfodocs, "", PW.InfodocRightURL)) + "'"    ' D2975
                    _RightWRTInfodocURLsList += CStr(IIf(_RightWRTInfodocURLsList = "", "", ",")) + "'" + JS_SafeString(IIf(PW.InfodocRightWRT = "" AndAlso Not CanEditInfodocs, "", PW.InfodocRightWRTURL)) + "'"    ' D2975
                    _RightInfodocEditURLsList += CStr(IIf(_RightInfodocEditURLsList = "", "", ",")) + """" + JS_SafeString(IIf(CanEditInfodocs, PW.InfodocRightEditURL, String.Format("open_infodoc('{0}');", JS_SafeString(PW.InfodocRightURL)))) + """"    ' D1571
                    _RightWRTInfodocEditURLsList += CStr(IIf(_RightWRTInfodocEditURLsList = "", "", ",")) + """" + JS_SafeString(IIf(CanEditInfodocs, PW.InfodocRightWRTEditURL, String.Format("open_infodoc('{0}');", JS_SafeString(PW.InfodocRightWRTURL)))) + """" ' D1571
                    _RightInfodocTitlesList += CStr(IIf(_RightInfodocTitlesList = "", "", ",")) + "'" + JS_SafeString(String.Format(lblInfodocTitleNode, ShortString(PW.RightNode, MaxLenInfodocCaption, False))) + "'"
                    _RightInfodocTitlesWRTList += CStr(IIf(_RightInfodocTitlesWRTList = "", "", ",")) + "'" + JS_SafeString(String.Format(lblInfodocTitleWRT, ShortString(PW.RightNode, MaxLenInfodocCaptionWRT - 2, False), ShortString(CaptionName, MaxLenInfodocCaptionWRT - 2, False))) + "'"
                    ' D1374 ==

                    ' D1161 ===
                    InitInfodoc(False, e.Item, "tooltipLeftInfodoc", "imgLeftInfodoc", PW.InfodocLeft, PW.InfodocLeftEditURL, PW.InfodocLeftURL, String.Format(lblInfodocTitleNode, ShortString(PW.LeftNode, MaxLenInfodocCaption)))
                    InitInfodoc(False, e.Item, "tooltipRightInfodoc", "imgRightInfodoc", PW.InfodocRight, PW.InfodocRightEditURL, PW.InfodocRightURL, String.Format(lblInfodocTitleNode, ShortString(PW.RightNode, MaxLenInfodocCaption)))

                    If ShowWRTInfodocs Then
                        InitInfodoc(True, e.Item, "tooltipLeftWRT", "imgLeftWRT", PW.InfodocLeftWRT, PW.InfodocLeftWRTEditURL, PW.InfodocLeftWRTURL, String.Format(lblInfodocTitleWRT, ShortString(PW.LeftNode, MaxLenInfodocCaptionWRT), ShortString(Caption, MaxLenInfodocCaptionWRT)))
                        InitInfodoc(True, e.Item, "tooltipRightWRT", "imgRightWRT", PW.InfodocRightWRT, PW.InfodocRightWRTEditURL, PW.InfodocRightWRTURL, String.Format(lblInfodocTitleWRT, ShortString(PW.RightNode, MaxLenInfodocCaptionWRT), ShortString(Caption, MaxLenInfodocCaptionWRT)))
                    End If
                    ' D1161 ==

                    ' D4331 ===
                    If PW.InfodocLeft <> "" Then _HasLeftInfodoc = True
                    If PW.InfodocLeftWRT <> "" Then _HasLeftWRTInfodoc = True
                    If PW.InfodocRight <> "" Then _HasRightInfodoc = True
                    If PW.InfodocRightWRT <> "" Then _HasRightWRTInfodoc = True
                End If
                ' D4331 + D4337 ==

                Dim imgComment As Image = DirectCast(e.Item.FindControl("imgComment"), Image)
                Dim tooltipComment As RadToolTip = DirectCast(e.Item.FindControl("ttEditComment"), RadToolTip)
                If imgComment IsNot Nothing AndAlso tooltipComment IsNot Nothing Then
                    imgComment.Visible = ShowComment
                    tooltipComment.Visible = ShowComment
                    If ShowComment Then
                        imgComment.ImageUrl = CStr(IIf(PW.Comment = "", ImageCommentEmpty, ImageCommentExists))
                        imgComment.Attributes.Add("onclick", String.Format("setTimeout('var c = theForm.Comment{0}; if ((c)) c.focus();', 500);", PW.ID))   ' D2089
                        imgComment.Attributes.Add("onmouseover", "ChangeCommentIcon(this,1);")  ' D2769
                        imgComment.Attributes.Add("onmouseout", "ChangeCommentIcon(this,0);")   ' D2769
                        _ImagesCommentsList += CStr(IIf(_ImagesCommentsList = "", "", ",")) + String.Format("'{0}'", imgComment.ClientID) ' D2725
                        tooltipComment.TargetControlID = imgComment.ClientID
                        tooltipComment.IsClientID = True

                        Dim btnSave As Button = DirectCast(e.Item.FindControl("btnSave"), Button)
                        If Not btnSave Is Nothing Then
                            btnSave.Text = CaptionSaveComment
                            btnSave.OnClientClick = String.Format("ApplyComment('{0}', '{1}','{2}'); return false;", e.Item.ItemIndex, JS_SafeString(tooltipComment.ClientID), imgComment.ClientID)
                        End If

                        ' D1161 ===
                        Dim btnClose As Button = DirectCast(e.Item.FindControl("btnClose"), Button)
                        If Not btnClose Is Nothing Then
                            btnClose.Text = CaptionCloseComment
                            btnClose.OnClientClick = String.Format("SwitchTooltip('{0}'); return false;", JS_SafeString(tooltipComment.ClientID))
                        End If
                        ' D1161 ==
                    End If
                End If
            End If
        End Sub

        ' D1161 ===
        Private Sub InitInfodoc(fIsWRT As Boolean, Item As RepeaterItem, TooltipID As String, ImageID As String, Infodoc As String, InfodocEditURL As String, InfodocURL As String, sTitle As String)
            Dim tooltipInfodoc As RadToolTip = DirectCast(Item.FindControl(TooltipID), RadToolTip)
            Dim imgInfodoc As Image = DirectCast(Item.FindControl(ImageID), Image)

            If imgInfodoc IsNot Nothing Then
                If fIsWRT Then
                    _ImagesWRTList += CStr(IIf(_ImagesWRTList = "", "", ", ")) + "'" + imgInfodoc.ClientID + "'"
                Else
                    _ImagesList += CStr(IIf(_ImagesList = "", "", ", ")) + "'" + imgInfodoc.ClientID + "'"
                End If
                If tooltipInfodoc IsNot Nothing Then
                    If fIsWRT Then
                        _TooltipsWRTList += CStr(IIf(_TooltipsWRTList = "", "", ", ")) + "'" + tooltipInfodoc.ClientID + "'"
                    Else
                        _TooltipsInfodocList += CStr(IIf(_TooltipsInfodocList = "", "", ", ")) + "'" + tooltipInfodoc.ClientID + "'"
                    End If
                    tooltipInfodoc.TargetControlID = imgInfodoc.ID
                End If
                InitInfodocImage(imgInfodoc, tooltipInfodoc, Infodoc, InfodocEditURL, sTitle, CStr(IIf(fIsWRT, imageWRTInfodocEmpty, imageInfodocEmpty)), InfodocURL) ' D3593
            End If
        End Sub
        ' D1161 ==

        ' D2738 ===
        Public Function GetKnownLikelihood(L As Double) As String
            Dim sLikelihood As String = ""
            If L >= 0 Then
                sLikelihood = String.Format("<div class='text small' style='margin-top:4px; color:#ffff99;'>{0} {1}</div>", KnownLikelihoodTitle, L.ToString("F" + gpwPrecision.ToString))
            End If
            Return sLikelihood
        End Function
        ' D2738 ==

        ' D2989 ===
        Public Function GetNodeComment(sVal As String) As String
            Dim sComment As String = ""
            If Not String.IsNullOrEmpty(sVal) Then
                sComment = String.Format("<div class='text small' style='margin:2px; color:#eeeeee;'>{0}</div>", sVal)
            End If
            Return sComment
        End Function
        ' D2989 ==

        ' D1374 ===
        Protected Sub Page_PreRender(sender As Object, e As EventArgs) Handles Me.PreRender
            If _FocusID < 0 Then _FocusID = 0
            If Not Page Is Nothing AndAlso Not IsPostBack AndAlso ShowFramedInfodocs AndAlso Data IsNot Nothing AndAlso Data.Count >= _FocusID Then
                Dim tRow As clsPairwiseLine = Data(_FocusID)
                ' D1415 ===
                Dim fHasGoal As Boolean = CaptionInfodoc <> "" OrElse CanEditInfodocs
                ' D1845 + D2994 + D4331 + D4337 ===
                InitRFrame(frmInfodocGoal, String.Format(lblInfodocTitleGoal, String.Format(If(isSecondNodeMode, "<b>{0}</b>", "<span class='wrt_name_infodoc'>{0}</span>"), ShortString(CaptionName, MaxLenInfodocCaption - 10, True))), CaptionInfodocURL, CaptionInfodocEditURL, "node", isInfodocCaptionCollapsed, isHTMLEmpty(CaptionInfodoc), 250, 90, "frmInfodocNodeLeft,frmInfodocNodeRight", "mp", False OrElse fHasGoal) ' D1450
                If isSecondNodeMode Then
                    frmInfodocNodeLeft.Visible = False
                    frmInfodocWRTLeft.Visible = False
                    frmInfodocWRTRight.Visible = False
                    If lblInfodocTitleNode <> "" AndAlso SecondNodeInfodocURL <> "" Then InitRFrame(frmInfodocNodeRight, String.Format(lblInfodocTitleNode, ShortString(SecondNodeName, MaxLenInfodocCaption, True)), SecondNodeInfodocURL, SecondNodeInfodocEditURL, "noderight", _isCaptionInfodocCollapsed, isHTMLEmpty(SecondNodeInfodoc), If(fHasGoal, 250, 350), 90, "frmInfodocGoal", "mp", False) Else frmInfodocNodeRight.Visible = False ' D4425
                Else
                    If CanEditInfodocs OrElse _HasLeftInfodoc Then InitRFrame(frmInfodocNodeLeft, String.Format(lblInfodocTitleNode, ShortString(tRow.LeftNode, MaxLenInfodocCaption, True)), tRow.InfodocLeftURL, tRow.InfodocLeftEditURL, "nodeleft", isInfodocCaptionCollapsed, isHTMLEmpty(tRow.InfodocLeft), If(fHasGoal, 250, 350), 90, "frmInfodocNodeRight,frmInfodocGoal", "mp", _HasLeftInfodoc) Else frmInfodocNodeLeft.Visible = False ' D4112
                    If CanEditInfodocs OrElse _HasRightInfodoc Then InitRFrame(frmInfodocNodeRight, String.Format(lblInfodocTitleNode, ShortString(tRow.RightNode, MaxLenInfodocCaption, True)), tRow.InfodocRightURL, tRow.InfodocRightEditURL, "noderight", isInfodocCaptionCollapsed, isHTMLEmpty(tRow.InfodocRight), If(fHasGoal, 250, 350), 90, "frmInfodocNodeLeft,frmInfodocGoal", "mp", _HasRightInfodoc) Else frmInfodocNodeRight.Visible = False ' D4112
                    ' D1845 ==
                    If ShowWRTInfodocs Then
                        InitRFrame(frmInfodocWRTLeft, String.Format(lblInfodocTitleWRT, ShortString(tRow.LeftNode, MaxLenInfodocCaptionWRT - 2, True), ShortString(CaptionName, MaxLenInfodocCaptionWRT - 2, True)), tRow.InfodocLeftWRTURL, tRow.InfodocLeftWRTEditURL, "wrtleft", InfodocsCollapsed, isHTMLEmpty(tRow.InfodocLeftWRT), CInt(IIf(fHasGoal, 250, 350)), 90, "frmInfodocWRTRight", "mpwrt", _HasLeftWRTInfodoc)  ' D1845
                        InitRFrame(frmInfodocWRTRight, String.Format(lblInfodocTitleWRT, ShortString(tRow.RightNode, MaxLenInfodocCaptionWRT - 2, True), ShortString(CaptionName, MaxLenInfodocCaptionWRT - 2, True)), tRow.InfodocRightWRTURL, tRow.InfodocRightWRTEditURL, "wrtright", InfodocsCollapsed, isHTMLEmpty(tRow.InfodocRightWRT), CInt(IIf(fHasGoal, 250, 350)), 90, "frmInfodocWRTLeft", "mpwrt", _HasRightWRTInfodoc) ' D1845
                        ' D1451 + D2994 + D4331 ==
                    End If
                End If
                ' D4337 ==
            End If
            'ScriptManager.RegisterStartupScript(Page, GetType(String), "InitFocus", String.Format("{0}{1}setTimeout('InitComments(); SetFocus({2},1,1); if ((theForm.gpw_slider{2}_b)) {{ theForm.gpw_slider{2}_b.focus(); setTimeout(""theForm.gpw_slider{2}_b.blur();"", 10); }}', 250);", IIf(Request.Browser.Browser.ToLower.Contains("firefox"), "ResizeList(); ", ""), IIf(ShowOnlyFocusedRow AndAlso _FocusID >= 0, " HideAllExceptOne(" + _FocusID.ToString + ");", ""), _FocusID), True) ' D2677 + D2725 + D3237
            ScriptManager.RegisterStartupScript(Page, GetType(String), "InitFocus", String.Format("{0}{1}setTimeout('InitComments(); SetFocus({2},1,false); if ((theForm.gpw_slider{2}_b)) {{ theForm.gpw_slider{2}_b.focus(); setTimeout(""theForm.gpw_slider{2}_b.blur();"", 10); }}', 250);", "ResizeList(); ", IIf(ShowOnlyFocusedRow AndAlso _FocusID >= 0, " HideAllExceptOne(" + _FocusID.ToString + ");", ""), _FocusID), True) ' D2677 + D2725 + D3237 + D3896
        End Sub
        ' D1374 ==

    End Class

End Namespace