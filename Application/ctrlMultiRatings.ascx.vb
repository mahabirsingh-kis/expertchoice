Imports System.Drawing
Imports Telerik.Web.UI

Namespace ExpertChoice.Web.Controls

    Partial Public Class ctrlMultiRatings

        Inherits ctrlEvaluationControlBase    ' D1007

        Private _Ratings As List(Of clsRatingLine)  ' D1008
        Private _ParentNodeID As String = ""        ' D1139 + 2503
        'Private _Caption As String = ""            ' -D2181
        Private _CaptionName As String = ""         ' D1007
        Private _CaptionInfodoc As String = ""      ' D0108
        Private _CaptionInfodocURL As String = ""   ' D1064
        Private _CaptionInfodocEditURL As String = ""   ' D1007
        ' D1372 ===
        Private _CaptionInfodocCollapsed As Boolean = False
        Private _AlternativeInfodocCollapsed As Boolean = False
        Private _WRTInfodocCollapsed As Boolean = False
        Public _ImageBlank As String = ""
        ' D1372 ==
        Private _ComboWidth As Integer = 12         ' D0112
        Public _RatingsList As String = ""          ' D0116
        Public _LabelsList As String = ""           ' D2791
        Public _ComboBoxesList As String = ""       ' D0116
        Public _TooltipsInfodocList As String = ""  ' D1007
        Public _TooltipsWRTList As String = ""      ' D1007
        Public _ImagesList As String = ""           ' D1008
        Public _ImagesWRTList As String = ""        ' D1008
        Public _ImagesCommentsList As String = ""   ' D2719
        Public _InfodocURLsList As String = ""      ' D1096
        Public _WRTInfodocURLsList As String = ""   ' D1096
        Public _InfodocTitlesList As String = ""    ' D1372
        Public Property RatingsTitle As String = "" ' D6661
        Public Property FlashRatingsTitle As Boolean = True ' D6680
        Public _InfodocTitlesWRTList As String = "" ' D1372
        Public _InfodocEditURLsList As String = ""  ' D1372
        Public _WRTInfodocEditURLsList As String = ""   ' D1372
        ' D2251 ===
        Private _ScaleInfodocURL As String = ""
        Private _ScaleInfodocCollapsed As Boolean = False
        Private _ScaleInfodocEditURL As String = ""
        ' D2251 ==
        Private _ScaleMaxCaptionWidth As Integer = 50   ' D3742

        Public Const ComboMaxWidth As Integer = 35  ' D0260
        Public Const FrameWidth As Integer = 200    ' D1096 + D1508

        Private ShowPathsForRows As Boolean = False ' D4106


        ' D0647 ===
        Private _CaptionSaveComment As String = "OK"
        Private _ImageCommentEmpty As String = "note_.gif"
        Private _ImageCommentExists As String = "note.gif"
        ' D0647 ==
        'Private _msgMissingRatings As String = ""   ' D1034 -D2158
        Private _tblIntensity As String = "Intensity"   ' D1035
        Private _tblPriority As String = "Priority"     ' D1035
        Private _msgLoading As String = "Loading…"      ' D1035
        Private _msgDirectRatingValue As String = "Wrong value" ' D1747
        Private _lblDirectValue As String = "Direct value"      ' D1747

        Public lblInfodocTitleScale As String = "Scale description"    ' D2250
        Public lblIntensityDescriptionInput As String = "Edit intensity description"    ' D2282

        Public _FocusID As Integer = -1                 ' D1029 + D1425
        Public ReverseInfodocFrames As Boolean = False  ' D1754

        Public Const _HIDDEN As String = ctrlFramedInfodoc._HIDDEN      ' D1372
        Public Const _VISIBLE As String = ctrlFramedInfodoc._VISIBLE    ' D1372

        Public sRootPath As String = ""     ' D1593

        Public Precision As Integer = 2     ' D2120 + D3706

        Private ScalesTotal As List(Of Guid) = Nothing          ' D2282 + D3070
        Public ScalesComment As String = ""                     ' D2282

        Public frmInfodocGoal As ctrlFramedInfodoc = Nothing    ' D1754
        Public frmInfodocNode As ctrlFramedInfodoc = Nothing    ' D1754

        Public Const _OPT_SHOW_COMBOBOXES As Boolean = False    ' D2791
        Private Const SESS_WORDING As String = "RW"             ' D6680

        Public Const BAR_WIDTH As Integer = 50  ' D3265

        Private Const DoAnimateOnLoad As Boolean = True         ' D3354
        Public Bars2Animate As String = ""                      ' D3354

        Public _ShowIndex As Boolean = False             ' D4013 + D4105

        Public ShowDirectValue As Boolean = True    ' D4170
        Public ShowPrtyValues As Boolean = True     ' D4170

        'Public USD_OptionVisible As Boolean = False     ' D3741
        'Public USD_ShowValues As Boolean = False        ' D3740
        'Public USD_Cost As Double = 0                   ' D3740
        'Public USD_OptionName As String = "Show USD"    ' D3740

        ' D0112 ===
        Public Property ComboWidth() As Integer ' D3472
            Get
                Return _ComboWidth
            End Get
            Set(value As Integer)
                _ComboWidth = value
            End Set
        End Property
        ' D0112 ==

        ' D1007 ===
        Public Property CaptionName() As String
            Get
                Return _CaptionName
            End Get
            Set(value As String)
                _CaptionName = value
            End Set
        End Property
        ' D1007 ==

        ' D1139 ===
        Public Property ParentNodeID() As String
            Get
                Return _ParentNodeID
            End Get
            Set(value As String)
                _ParentNodeID = value
            End Set
        End Property
        ' D1139 ==

        ' D4013 ===
        Public Property ShowIndex As Boolean
            Get
                Return _ShowIndex
            End Get
            Set(value As Boolean)
                _ShowIndex = value
            End Set
        End Property
        ' D4013 ==

        ' -D2181
        'Public Property Caption() As String
        '    Get
        '        Return _Caption
        '    End Get
        '    Set(ByVal value As String)
        '        _Caption = value
        '    End Set
        'End Property

        ' D0108 ===
        Public Property CaptionInfodoc() As String
            Get
                Return _CaptionInfodoc
            End Get
            Set(value As String)
                _CaptionInfodoc = value
            End Set
        End Property
        ' D0108 ==

        ' D1064 ===
        Public Property CaptionInfodocURL() As String
            Get
                Return _CaptionInfodocURL
            End Get
            Set(value As String)
                _CaptionInfodocURL = value
            End Set
        End Property
        ' D1064 ==

        ' D1007 ===
        Public Property CaptionInfodocEditURL() As String
            Get
                Return _CaptionInfodocEditURL
            End Get
            Set(value As String)
                _CaptionInfodocEditURL = value
            End Set
        End Property
        ' D1007 ==

        ' D1372 ===
        Public Property CaptionInfodocCollapsed() As Boolean
            Get
                Return _CaptionInfodocCollapsed
            End Get
            Set(value As Boolean)
                _CaptionInfodocCollapsed = value
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

        Public Property AlternativeInfodocCollapsed() As Boolean
            Get
                Return _AlternativeInfodocCollapsed
            End Get
            Set(value As Boolean)
                _AlternativeInfodocCollapsed = value
            End Set
        End Property
        ' D1372 ==

        ' D2251 ===
        Public Property ScaleInfodocURL() As String
            Get
                Return _ScaleInfodocURL
            End Get
            Set(value As String)
                _ScaleInfodocURL = value
            End Set
        End Property

        Public Property ScaleInfodocCollapsed() As Boolean
            Get
                Return _ScaleInfodocCollapsed
            End Get
            Set(value As Boolean)
                _ScaleInfodocCollapsed = value
            End Set
        End Property

        Public Property ScaleInfodocEditURL() As String
            Get
                Return _ScaleInfodocEditURL
            End Get
            Set(value As String)
                _ScaleInfodocEditURL = value
            End Set
        End Property
        ' D2251 ==

        ' D0647 ===
        Public Property CaptionSaveComment() As String
            Get
                Return _CaptionSaveComment
            End Get
            Set(value As String)
                _CaptionSaveComment = value
            End Set
        End Property

        ' D1372 ===
        Public Property ImageBlank() As String
            Get
                Return _ImageBlank
            End Get
            Set(value As String)
                _ImageBlank = value
            End Set
        End Property
        ' D1372 ==

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
        ' D0647 ==

        ' D1008 ===
        Public Property Ratings() As List(Of clsRatingLine)
            Get
                Return _Ratings
            End Get
            Set(value As List(Of clsRatingLine))
                _Ratings = value
            End Set
        End Property
        ' D1008 ==

        ' -D2158
        '' D1034 ===
        'Public Property msgMissingRatings() As String
        '    Get
        '        Return _msgMissingRatings
        '    End Get
        '    Set(ByVal value As String)
        '        _msgMissingRatings = value
        '    End Set
        'End Property
        '' D1034 ==

        ' D1747 ===
        Public Property msgDirectRatingValue() As String
            Get
                Return _msgDirectRatingValue
            End Get
            Set(value As String)
                _msgDirectRatingValue = value
            End Set
        End Property

        Public Property lblDirectValue() As String
            Get
                Return _lblDirectValue
            End Get
            Set(value As String)
                _lblDirectValue = value
            End Set
        End Property
        ' D1747 ==

        ' D1035 ===
        Public Property tblIntensity() As String
            Get
                Return _tblIntensity
            End Get
            Set(value As String)
                _tblIntensity = value
            End Set
        End Property

        Public Property tblPriority() As String
            Get
                Return _tblPriority
            End Get
            Set(value As String)
                _tblPriority = value
            End Set
        End Property

        Public Property msgLoading() As String
            Get
                Return _msgLoading
            End Get
            Set(value As String)
                _msgLoading = value
            End Set
        End Property
        ' D1035 ==

        Public Function GetRatingsCount() As Integer
            If Ratings IsNot Nothing Then Return Ratings.Count Else Return 0 ' D1008
        End Function

        Public Sub ScanRatingsForMaxLen(Ratings As List(Of clsRating)) ' D1008
            For Each tRating As clsRating In Ratings
                If CInt(0.8 * (tRating.Name.Length + 5)) > ComboWidth Then ComboWidth = CInt(0.8 * (tRating.Name.Length + 5)) ' D0260 + D1029
                If ComboWidth > ComboMaxWidth Then ComboWidth = ComboMaxWidth ' D0260
            Next
        End Sub

        ' D3472 ===
        Public Function GetRatingsColWidth() As Integer
            Dim sWidth As Integer = ComboWidth * 11 + BAR_WIDTH + (3 + Precision) * 8 + 30
            If sWidth < 250 Then sWidth = 250
            If sWidth > 450 Then sWidth = 450
            Return sWidth
        End Function
        ' D3472 ==

        Protected Sub Page_Load(sender As Object, e As EventArgs)
            If CaptionName Is Nothing Then lblCaption.Visible = False Else lblCaption.Text = CaptionName ' D2181 + D3250
            trCaption.Visible = Not ShowFramedInfodocs  ' D2181
            CheckAndFlashWRTCaption(lblCaption.ClientID, ParentNodeID) ' D1139
            If CaptionInfodoc IsNot Nothing AndAlso CaptionName IsNot Nothing Then InitInfodocImage(imgCaptionInfodoc, tooltipGoal, CaptionInfodoc, CaptionInfodocEditURL, String.Format(lblInfodocTitleGoal, ShortString(CaptionName, MaxLenInfodocCaption)), imageInfodocEmpty, CaptionInfodocURL) ' D1007 + D1064 + D2181 + D3250
            ' D1008 ===
            If Ratings IsNot Nothing Then
                CheckRatingForDuplicates()  ' D4106
                '-D4180 due to using new clsNode.Index property
                ' D4013 ===
                'If ShowIndex Then
                '    Dim MaxLen As Integer = 0
                '    For Each R As clsRatingLine In Ratings
                '        If R.Idx.Length > MaxLen Then MaxLen = R.Idx.Length
                '    Next
                '    If MaxLen > 1 Then
                '        For Each R As clsRatingLine In Ratings
                '            R.Idx = padWithZeros(R.Idx, MaxLen)
                '        Next
                '    End If
                'End If
                ' D4013 ==
                rptAlts.DataSource = Ratings
                rptAlts.DataBind()
            End If
            ' D1034 ===
            If Not Page Is Nothing AndAlso Not IsPostBack Then
                Page.ClientScript.RegisterOnSubmitStatement(GetType(String), "CheckForm", "return CheckFormOnSubmit();")
            End If
            ' D1034 ==
        End Sub

        Protected Sub rptAlts_ItemDataBound(sender As Object, e As RepeaterItemEventArgs)
            If Not e.Item.DataItem Is Nothing Then

                If Not e.Item.FindControl("ddListRatings") Is Nothing Then
                    Dim ddList As RadComboBox = CType(e.Item.FindControl("ddListRatings"), RadComboBox)   ' D0535
                    ddList.ID = "ddRatings" + e.Item.ItemIndex.ToString ' D1029
                    AddHandler ddList.ItemDataBound, AddressOf ddListRatings_ItemDataBound  ' D1029

                    Dim D As clsRatingLine = CType(e.Item.DataItem, clsRatingLine)
                    If D.Ratings Is Nothing Then D.Ratings = New List(Of clsRating) ' D6688
                    ddList.DataSource = D.Ratings
                    ddList.DataBind()
                    ddList.SelectedValue = CStr(D.RatingID)
                    If D.DirectData >= 0 AndAlso D.DirectData <= 1 AndAlso D.RatingID < 0 Then
                        ddList.SelectedIndex = -1
                        If D.DirectData >= 0 AndAlso D.DirectData <= 1 AndAlso D.RatingID < 0 Then
                            ddList.SelectedIndex = -1
                            ' D1788 + D3706 ===
                            'Dim sVal As String = D.DirectData.ToString("F8")    ' D1745 + D1747
                            'While sVal.Length > 3 AndAlso sVal(sVal.Length - 1) = "0"
                            '    sVal = sVal.Substring(0, sVal.Length - 1)
                            'End While
                            Dim tDVal As Double = D.DirectData
                            String2Double(JS_SafeNumber(D.DirectData), tDVal)
                            ddList.Text = tDVal.ToString()  ' Double2String(D.DirectData, Precision)   ' D3082 
                            If ddList.Text.Length > Precision + 4 Then ddList.Text = ddList.Text.Substring(0, Precision + 4)
                            ' D1788 + D3706 ==
                        End If
                    End If
                    ddList.Attributes.Add("data", String.Format("['{0}',{1}]", D.sGUID, IIf(D.RatingsComment = "", 0, 1)))    ' D2251 + D2420 + D3354

                    ' D2282 ===
                    If ScalesTotal Is Nothing Then ScalesTotal = New List(Of Guid) ' D3070
                    For Each tRating As clsRating In D.Ratings
                        If tRating.ID >= 0 AndAlso Not ScalesTotal.Contains(tRating.GuidID) Then    ' D3070
                            ScalesTotal.Add(tRating.GuidID)    ' D3070
                            ScalesComment += String.Format("<input type=hidden name='Intensity_{0}' value='{1}'/>", GetMD5(tRating.GuidID.ToString), SafeFormString(tRating.Comment))    ' D2470 + D3009 + D3068 + D3070
                        End If
                    Next
                    ' D2282 ==

                    _InfodocURLsList += CStr(IIf(_InfodocURLsList = "", "", ",")) + "'" + JS_SafeString(IIf(Not CanEditInfodocs AndAlso D.Infodoc = "", "", D.InfodocURL)) + "'"  ' D1096 + D2412
                    _WRTInfodocURLsList += CStr(IIf(_WRTInfodocURLsList = "", "", ",")) + "'" + JS_SafeString(IIf(Not CanEditInfodocs AndAlso D.InfodocWRT = "", "", D.InfodocWRTURL)) + "'"  ' D1096 + D2412
                    ' D1372 ===
                    _InfodocEditURLsList += CStr(IIf(_InfodocEditURLsList = "", "", ",")) + """" + JS_SafeString(IIf(CanEditInfodocs, D.InfodocEditURL, String.Format("open_infodoc('{0}');", JS_SafeString(D.InfodocURL)))) + """"   ' D1571
                    _WRTInfodocEditURLsList += CStr(IIf(_WRTInfodocEditURLsList = "", "", ",")) + """" + JS_SafeString(IIf(CanEditInfodocs, D.InfodocWRTEditURL, String.Format("open_infodoc('{0}');", JS_SafeString(D.InfodocWRTURL)))) + """"   ' D1571
                    _InfodocTitlesList += CStr(IIf(_InfodocTitlesList = "", "", ",")) + "'" + JS_SafeString(String.Format(lblInfodocTitleNode, ShortString(D.Title, MaxLenInfodocCaption, False))) + "'"
                    _InfodocTitlesWRTList += CStr(IIf(_InfodocTitlesWRTList = "", "", ",")) + "'" + JS_SafeString(String.Format(lblInfodocTitleWRT, ShortString(D.Title, MaxLenInfodocCaptionWRT, False), ShortString(CaptionName, MaxLenInfodocCaptionWRT, False))) + "'"
                    ' D1372 ==

                    If D.RatingID < 0 AndAlso D.DirectData < 0 AndAlso _FocusID < 0 Then _FocusID = e.Item.ItemIndex ' D1029 + D1750

                    'ddList.OnClientSelectedIndexChanged = String.Format("SetRating({0}, this.value)", D.ID) 
                    ' D0116 ===
                    ddList.OnClientSelectedIndexChanged = String.Format("onChangedRating")
                    If _RatingsList <> "" Then
                        _RatingsList += ", "
                        _ComboBoxesList += ", "
                    End If
                    _RatingsList += String.Format("{0}", D.ID)
                    _ComboBoxesList += String.Format("'{0}'", ddList.ClientID)
                    ' D0116 ==

                    ' D2791 ===
                    If Not _OPT_SHOW_COMBOBOXES Then
                        ddList.Attributes.Add("style", "display:none")
                        Dim lbl As Label = CType(e.Item.FindControl("lblRatingValue"), Label)
                        If lbl IsNot Nothing Then
                            lbl.Visible = True
                            lbl.Text = ShortString(ddList.Text, 85, True, 40)   ' D4968
                            If lbl.Text <> ddList.Text Then lbl.ToolTip = ddList.Text ' D4968
                            lbl.Attributes.Add("style", "padding:1px 3px;")
                            _LabelsList += CStr(IIf(_LabelsList = "", "", ",")) + String.Format("'{0}'", lbl.ClientID)
                        End If
                    End If
                    ' D2791 ==

                    ' D1580 ===
                    Dim Bar As Label = CType(e.Item.FindControl("Bar"), Label)
                    Bar.Text = "&nbsp;" ' D2769
                    Dim Val As Double = -1
                    If D.RatingID >= 0 Then
                        For i As Integer = 0 To D.Ratings.Count - 1
                            If D.Ratings(i).ID = D.RatingID Then Val = D.Ratings(i).Value
                        Next
                    Else
                        If D.DirectData >= 0 AndAlso D.DirectData <= 1 Then Val = D.DirectData ' D1745
                    End If
                    Dim sBarID As String = "bar_" + GetRandomString(4, True, False)  ' D3354
                    If Val >= 0 Then Bar.Text = String.Format("<table border=0 cellspacing=0 cellpadding=0 width='100%'><tr valign=middle><td style='width:" + BAR_WIDTH.ToString + "px; padding-right:4px; background:!important; border:0px;'>{0}</td><td align=right class='text small' style='background:!important; border:0px;'><nobr>{1}</nobr></td></tr></table><input type='hidden' name='bar_old_{2}' value='{3}'>", HTMLCreateGraphBar(CSng(If(DoAnimateOnLoad AndAlso Val > 0.01, 0.02, Val)), 1, BAR_WIDTH, 8, "fill1", ImageBlank, Double2String(Val * 100, Precision, True), False, sBarID), Double2String(Val * 100, Precision, True), D.ID, JS_SafeNumber(Val)) ' D1598 + D2120 + D2122 + D3082 + D3265 + D3354 + D4391
                    ' D1580 ==
                    If DoAnimateOnLoad AndAlso Val > 0.01 Then Bars2Animate += String.Format("{0}['{1}',{2}]", IIf(Bars2Animate = "", "", ","), sBarID, Math.Round(Val * BAR_WIDTH)) ' D3354 + D4391

                    ' D0108 + D1007 ===
                    Dim tooltipWRT As RadToolTip = DirectCast(e.Item.FindControl("tooltipWRT"), RadToolTip)
                    Dim imgWRT As WebControls.Image = DirectCast(e.Item.FindControl("imgWRT"), WebControls.Image)

                    If imgWRT IsNot Nothing Then
                        _ImagesWRTList += CStr(IIf(_ImagesWRTList = "", "", ", ")) + "'" + imgWRT.ClientID + "'"  ' D1008
                        If tooltipWRT IsNot Nothing Then
                            _TooltipsWRTList += CStr(IIf(_TooltipsWRTList = "", "", ", ")) + "'" + tooltipWRT.ClientID + "'"
                            tooltipWRT.TargetControlID = imgWRT.ID
                        End If
                        imgWRT.Attributes.Add("style", "float:right; margin-left:1ex;")
                        InitInfodocImage(imgWRT, If(ShowFramedInfodocs, Nothing, tooltipWRT), D.InfodocWRT, D.InfodocWRTEditURL, String.Format(lblInfodocTitleWRT, ShortString(D.Title, MaxLenInfodocCaption - 5), ShortString(CaptionName, MaxLenInfodocCaption - 1)), imageWRTInfodocEmpty, D.InfodocWRTURL)    ' D1008 + D1064 + D1096 + D1372 + D2420
                    End If

                    Dim tooltipInfodoc As RadToolTip = DirectCast(e.Item.FindControl("tooltipInfodoc"), RadToolTip)
                    Dim imgInfodoc As WebControls.Image = DirectCast(e.Item.FindControl("imgInfodoc"), WebControls.Image)

                    If imgInfodoc IsNot Nothing Then
                        _ImagesList += CStr(IIf(_ImagesList = "", "", ", ")) + "'" + imgInfodoc.ClientID + "'"  ' D1008
                        If tooltipInfodoc IsNot Nothing Then
                            'imgInfodoc.Attributes.Add("style", "float:left; padding-right:4px;")    ' D4106
                            imgInfodoc.Attributes.Add("style", "padding-left:4px;")    ' D4106
                            _TooltipsInfodocList += CStr(IIf(_TooltipsInfodocList = "", "", ", ")) + "'" + tooltipInfodoc.ClientID + "'"
                            tooltipInfodoc.TargetControlID = imgInfodoc.ID
                        End If
                        InitInfodocImage(imgInfodoc, If(ShowFramedInfodocs, Nothing, tooltipInfodoc), D.Infodoc, D.InfodocEditURL, String.Format(lblInfodocTitleNode, ShortString(D.Title, MaxLenInfodocCaption)), imageInfodocEmpty, D.InfodocURL)    ' D1008 + D1064 + D1096 + D1372
                    End If
                    ' D1007 ==
                    ' D0108 ==

                    ' D0467 ===
                    Dim imgComment As WebControls.Image = DirectCast(e.Item.FindControl("imgComment"), WebControls.Image)
                    Dim tooltipComment As RadToolTip = DirectCast(e.Item.FindControl("ttEditComment"), RadToolTip)
                    If imgComment IsNot Nothing AndAlso tooltipComment IsNot Nothing Then
                        imgComment.Visible = ShowComment
                        tooltipComment.Visible = ShowComment
                        If ShowComment Then
                            'imgComment.Attributes.Add("style", "float:left; padding-right:4px;")    ' D4106
                            imgComment.ImageUrl = CStr(IIf(D.Comment = "", ImageCommentEmpty, ImageCommentExists))
                            imgComment.Attributes.Add("onclick", String.Format("SetFocus({0}, 1, 1, 0);", D.ID)) ' D1784
                            imgComment.Attributes.Add("onmouseover", "ChangeCommentIcon(this,1);")  ' D2769
                            imgComment.Attributes.Add("onmouseout", "ChangeCommentIcon(this,0);")   ' D2769
                            'imgComment.Attributes.Add("onload", String.Format("setTimeout(""$get('{1}').alt='{0}';"", 500);", JS_SafeString(D.Comment), imgComment.ClientID))  ' D2719
                            _ImagesCommentsList += CStr(IIf(_ImagesCommentsList = "", "", ",")) + String.Format("'{0}'", imgComment.ClientID) ' D2719
                            tooltipComment.TargetControlID = imgComment.ClientID
                            tooltipComment.IsClientID = True

                            Dim btnSave As Button = DirectCast(e.Item.FindControl("btnSave"), Button)
                            If Not btnSave Is Nothing Then
                                btnSave.Text = CaptionSaveComment
                                btnSave.OnClientClick = String.Format("ApplyComment('{0}', '{1}','{2}'); return false;", D.ID, JS_SafeString(tooltipComment.ClientID), imgComment.ClientID)
                            End If
                            ' D6761 ===
                            Dim btnCancel As Button = DirectCast(e.Item.FindControl("btnCancel"), Button)
                            If Not btnCancel Is Nothing Then
                                btnCancel.OnClientClick = String.Format("var t = $find('{0}'); if ((t)) t.hide(); return false;", JS_SafeString(tooltipComment.ClientID))
                            End If
                            ' D6761 ==
                        End If
                    End If
                    ' D0467 ==
                End If
            End If
        End Sub

        ' D0122 ===
        Protected Sub ddListRatings_PreRender(sender As Object, e As EventArgs)
            ' D1747 ===
            If TypeOf (sender) Is RadComboBox AndAlso Not IsPostBack Then
                Dim Combo As RadComboBox = CType(sender, RadComboBox)
                Combo.Width = Unit.Pixel(If(ShowFramedInfodocs, FrameWidth - 60, 8 * ComboWidth)) ' D1096 + D1580
            End If
            ' D1747 ==
        End Sub
        ' D0112 ==

        ' D1029 ===
        Protected Sub Page_PreRender(sender As Object, e As EventArgs) Handles Me.PreRender
            If Ratings IsNot Nothing Then

                If Session(SESS_WORDING) IsNot Nothing AndAlso CStr(Session(SESS_WORDING)) = RatingsTitle Then FlashRatingsTitle = False    ' D6680
                Session(SESS_WORDING) = RatingsTitle    ' D6680

                frmInfodocGoal = CType(LoadControl("~/ctrlFramedInfodoc.ascx"), ctrlFramedInfodoc)
                frmInfodocGoal.OptionsID = "frmInfodocGoal"   ' D1815
                frmInfodocNode = CType(LoadControl("~/ctrlFramedInfodoc.ascx"), ctrlFramedInfodoc)
                frmInfodocNode.OptionsID = "frmInfodocNode"   ' D1815

                If ReverseInfodocFrames AndAlso CaptionInfodoc IsNot Nothing Then   ' D3250
                    tdInfodocLeft.Controls.Add(frmInfodocNode)
                    If CaptionInfodoc IsNot Nothing Then tdInfodocCenter.Controls.Add(frmInfodocGoal) ' D3250
                Else
                    If CaptionInfodoc IsNot Nothing Then tdInfodocLeft.Controls.Add(frmInfodocGoal) ' D3250
                    tdInfodocCenter.Controls.Add(frmInfodocNode)
                End If

                ' D2252 ===
                If Not IsPostBack AndAlso Not ShowFramedInfodocs AndAlso ScaleInfodocURL <> "" AndAlso CanEditInfodocs Then ' D4170
                    tooltipScaleDesc.Visible = True
                    tooltipScaleDesc.Title = String.Format("<div class='text' style='margin-right:2em; margin-bottom:3px; border-bottom:1px solid #cccccc;'><b>{0}</b></div>", lblInfodocTitleScale)
                Else
                    tooltipScaleDesc.Visible = False    ' D4170
                End If
                ' D2252 ==

                ' D1372 ===
                If Not Page Is Nothing AndAlso Not IsPostBack AndAlso ShowFramedInfodocs AndAlso Ratings.Count > 0 Then
                    If _FocusID < 0 Then _FocusID = 0 ' D1751
                    Dim tRow As clsRatingLine = Ratings(_FocusID)
                    ' D1845 + D2994 ===
                    InitRFrame(frmInfodocNode, String.Format(lblInfodocTitleNode, ShortString(tRow.Title, MaxLenInfodocCaption, True)), tRow.InfodocURL, tRow.InfodocEditURL, "alt", AlternativeInfodocCollapsed, isHTMLEmpty(tRow.Infodoc), 300, 90, "frmInfodocGoal,frmInfodocWRT", "mr", If(CanEditInfodocs, True, Ratings.FirstOrDefault(Function(r) (Not isHTMLEmpty(r.Infodoc))) IsNot Nothing))  ' D4551
                    If CaptionInfodoc IsNot Nothing Then    ' D3250
                        InitRFrame(frmInfodocGoal, String.Format(lblInfodocTitleGoal, ShortString(CaptionName, MaxLenInfodocCaption, True)), CaptionInfodocURL, CaptionInfodocEditURL, "node", CaptionInfodocCollapsed, isHTMLEmpty(CaptionInfodoc), 300, 90, "frmInfodocNode,frmInfodocWRT", "mr", False)
                        InitRFrame(frmInfodocWRT, String.Format(lblInfodocTitleWRT, ShortString(tRow.Title, MaxLenInfodocCaptionWRT, True), ShortString(CaptionName, MaxLenInfodocCaptionWRT, True)), tRow.InfodocWRTURL, tRow.InfodocWRTEditURL, "wrt", WRTInfodocCollapsed, isHTMLEmpty(tRow.InfodocWRT), 300, 90, "frmInfodocNode,frmInfodocGoal", "mr", If(CanEditInfodocs, True, Ratings.FirstOrDefault(Function(r) (Not isHTMLEmpty(r.InfodocWRT))) IsNot Nothing))  ' D4551
                    Else
                        frmInfodocWRT.Visible = False    ' D3250
                    End If
                    ' D1845 + D2994 ==
                    ' D2252 ===
                    InitRFrame(frmInfodocScale, String.Format(lblInfodocTitleScale, ShortString(tRow.Title, MaxLenInfodocCaption, True)), ScaleInfodocURL.Replace("%%guid%%", tRow.sGUID), ScaleInfodocEditURL.Replace("%%guid%%", tRow.sGUID), "scale", ScaleInfodocCollapsed, False, 300, 90, "", "scale", False)
                    frmInfodocScale.Visible = True
                    ' D2252 ==
                End If
                ' D1372 ==
                ' D1743 ===
                If Ratings.Count = 0 Then
                    frmInfodocGoal.Visible = False
                    frmInfodocNode.Visible = False
                    frmInfodocWRT.Visible = False
                    ScriptManager.RegisterStartupScript(Page, GetType(String), "InitBlank", "if (($get('ratings'))) $get('ratings').innerHTML = 'No data for evaluate';", True) ' D1035
                End If
                ' D1743 ==
                ScriptManager.RegisterStartupScript(Page, GetType(String), "InitFocus", "setTimeout('onResizeRatings(); InitCursor(" + _FocusID.ToString + ");', 150);", True) ' D1035 + D1776 + D1779 + D1844 + D3342 + D3470 + D4109
            End If
        End Sub

        Protected Sub ddListRatings_ItemDataBound(sender As Object, e As RadComboBoxItemEventArgs)
            If e.Item.DataItem IsNot Nothing Then
                If CDbl(e.Item.Value) < 0 Then e.Item.BackColor = Color.LightGoldenrodYellow ' D1598
                Dim r As clsRating = CType(e.Item.DataItem, clsRating)
                e.Item.Attributes.Add("data", String.Format("['{0}','{1}','{2}','{3}']", JS_SafeString(r.Name), JS_SafeNumber(IIf(r.ID >= 0, Double2String(r.Value, 3 * Precision), -1)), JS_SafeString(IIf(r.ID >= 0, Double2String(100 * r.Value, Precision, True), "&nbsp;")), JS_SafeString(GetMD5(r.GuidID.ToString)))) ' D2120 + D2248 + D2282 + D3068 + D3070 + D3082 + D3354
            End If
        End Sub
        ' D1029 ==

        ' D4106 ===
        Private Sub CheckRatingForDuplicates()
            ShowPathsForRows = False
            For i As Integer = 0 To Ratings.Count - 2
                Dim a As clsNode = Ratings(i).OriginalNode
                If a IsNot Nothing Then
                    For j As Integer = i + 1 To Ratings.Count - 1
                        Dim b As clsNode = Ratings(j).OriginalNode
                        If b IsNot Nothing AndAlso a.NodeGuidID.Equals(b.NodeGuidID) Then
                            ShowPathsForRows = True
                            Exit For
                        End If
                    Next
                    If ShowPathsForRows Then Exit For
                End If
            Next
        End Sub
        ' D4106 ==

        ' D4109 ===
        Public Function GetNodePath(tData As Object) As String
            Dim tRow As clsRatingLine = CType(tData, clsRatingLine)
            Dim sPath As String = StringService.GetNodePathString(tRow.OriginalNode, ecNodePathFormat.SpanWithTitleLimitedLevels)
            If sPath <> "" Then sPath = String.Format("<div class='text small' style='color:#006688; margin-bottom:2px;' id='spanPath{1}' path=""{2}"">{0}</div>", sPath, tRow.ID, StringService.GetNodePathString(tRow.OriginalNode, ecNodePathFormat.HierarchyWithoutCurrent))
            If sPath = "" AndAlso tRow.ScenarioName <> "" Then sPath = String.Format("<div class='text small' style='color:#006688; margin-bottom:2px;' id='spanPath{1}'>{0}</div>", tRow.ScenarioName, tRow.ID) ' D6822
            Return sPath
        End Function
        ' D4109 ==

        ' D4135 ===
        Public Function GetPathMaxHeight() As Integer
            Dim tCnt As Integer = 1
            If Ratings IsNot Nothing AndAlso Ratings.Count > 0 AndAlso Ratings(0).OriginalNode IsNot Nothing Then
                tCnt = Ratings(0).OriginalNode.Hierarchy.GetMaxLevel() - 2
            End If
            If tCnt < 1 Then tCnt = 1
            Return tCnt
        End Function
        ' D4135 ==

        ' D6683 ===
        Public Function GetRatingsTitleForFlash() As String
            Dim sRes As String = RatingsTitle.Trim
            Dim sp = sRes.LastIndexOf(" ")
            If sp > 0 Then sRes = String.Format("{0}<span id='divFlash' class='attention' style='padding:1px'>{1}</span>", sRes.Substring(0, sp + 1), sRes.Substring(sp + 1))
            Return sRes
        End Function
        ' D6683 ==

    End Class

End Namespace