Namespace ExpertChoice.Web.Controls

    Partial Public Class ctrlRating

        Inherits ctrlEvaluationControlBase    ' D1003

        'Private _RatingData As clsRatingsAction = Nothing   ' D0038 'Cv2
        Private _RatingData As clsOneAtATimeEvaluationActionData = Nothing   ' D0038 'Cv2 'C0464
        Private _NotRatedCaption As String = "Not rated"    ' D0039
        Private _ObjectiveName As String = ""   ' D1084
        'Private _Caption As String = ""         ' D1003 -D2181
        Private _CaptionInfodoc As String          ' D0151
        Private _CaptionInfodocURL As String = ""       ' D0689
        Private _CaptionInfodocCollapsed As Boolean = False   ' D0689
        Private _CaptionInfodocEditURL As String = ""   ' D0689
        Private _AlternativeName As String = ""         ' D1003
        Private _AlternativeInfodoc As String = ""      ' D0241
        Private _AlternativeInfodocURL As String = ""   ' D0241
        Private _AlternativeInfodocCollapsed As Boolean = False ' D0689
        Private _AlternativeInfodocEditURL As String = ""       ' D1003
        ' D1003 ===
        Private _WRTInfodoc As String = ""
        Private _WRTInfodocURL As String = ""
        Private _WRTInfodocCollapsed As Boolean = False
        Private _WRTInfodocEditURL As String = ""
        ' D1003 ==
        ' D2250 ===
        Private _ScaleInfodoc As String = ""
        Private _ScaleInfodocURL As String = ""
        Private _ScaleInfodocCollapsed As Boolean = False
        Private _ScaleInfodocEditURL As String = ""
        ' D2250 ==
        Public _ImageBlank As String = ""       ' D0689
        Public _Width As String = "80%"         ' D0689
        Public Intensities As String = ""       ' D2281

        Private _msgDirectRatingValue As String = "Wrong value" ' D1748
        Private _lblDirectValue As String = "Direct value"      ' D1748

        Public Precision As Integer = 2     ' D2120 + D3706

        Public lblInfodocTitleScale As String = "Scale description"    ' D2250
        Public lblIntensityDescriptionInput As String = "Edit intensity description"    ' D2281

        Public ParentNodeID As String = ""  ' D2505
        Public MeasurementScale As clsMeasurementScale = Nothing    ' D2510

        Public sRootPath As String = ""     ' D1593

        Public ShowDirectValue As Boolean = True    ' D4170
        Public ShowPrtyValues As Boolean = True     ' D4170

        'Cv2===
        Public Property Data() As clsOneAtATimeEvaluationActionData 'C0464
            Get
                Return _RatingData
            End Get
            Set(value As clsOneAtATimeEvaluationActionData) 'C0464
                _RatingData = value
            End Set
        End Property
        'Cv2==

        ' D1084 ===
        Public Property ObjectiveName() As String
            Get
                Return _ObjectiveName
            End Get
            Set(value As String)
                _ObjectiveName = value
            End Set
        End Property
        ' D1084 ==

        ' -D2181
        'Public Property Caption() As String
        '    Get
        '        Return _Caption         ' D1003
        '    End Get
        '    Set(ByVal value As String)
        '        _Caption = value        ' D1003
        '    End Set
        'End Property

        ' D0107 ===
        Public Property CaptionInfodoc() As String
            Get
                Return _CaptionInfodoc    ' D0108 + D1003
            End Get
            Set(value As String)
                _CaptionInfodoc = value     ' D1003
            End Set
        End Property

        ' D0689 ===
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

        ' D1003 ===
        Public Property CaptionInfodocEditURL() As String
            Get
                Return _CaptionInfodocEditURL
            End Get
            Set(value As String)
                _CaptionInfodocEditURL = value
            End Set
        End Property

        Public Property AlternativeName() As String  ' D0689
            Get
                Return _AlternativeName
            End Get
            Set(value As String)
                _AlternativeName = value
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
        ' D1003 ==

        Public Property AlternativeInfodoc() As String  ' D0689
            Get
                Return _AlternativeInfodoc
            End Get
            Set(value As String)
                _AlternativeInfodoc = value
            End Set
        End Property

        Public Property AlternativeInfodocURL() As String   ' D0689
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
        ' D0689 ==

        ' D1003 ===
        Public Property AlternativeInfodocEditURL() As String
            Get
                Return _AlternativeInfodocEditURL
            End Get
            Set(value As String)
                _AlternativeInfodocEditURL = value
            End Set
        End Property
        ' D1003 ==

        ' D2250 ===
        Public Property ScaleInfodoc() As String
            Get
                Return _ScaleInfodoc
            End Get
            Set(value As String)
                _ScaleInfodoc = value
            End Set
        End Property

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
        ' D2250 ==

        Public Property NotRatedCaption() As String
            Get
                Return _NotRatedCaption
            End Get
            Set(value As String)
                _NotRatedCaption = value
            End Set
        End Property

        ' D1748 ===
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
        ' D1748 ==

        ' D0689 ===
        Public Property ImageBlank() As String
            Get
                Return _ImageBlank
            End Get
            Set(value As String)
                _ImageBlank = value
            End Set
        End Property

        Public Property Width() As String
            Get
                Return _Width
            End Get
            Set(value As String)
                _Width = value
            End Set
        End Property
        ' D0689 ==

        Public Function GetValue() As Integer   ' D1748
            If Data Is Nothing Then Return -1
            ' D1748 ===
            Dim tJud As clsRatingMeasureData = CType(CType(Data, clsOneAtATimeEvaluationActionData).Judgment, clsRatingMeasureData)
            If tJud.IsUndefined Then
                Return -1
            Else
                Return tJud.Rating.ID
                'If tJud.Rating.RatingScaleID < 0 Then Return tJud.SingleValue Else Return tJud.Rating.ID
            End If
            ' D1748 ==
        End Function

        Private Sub AddRating(ID As Integer, Name As String, sComment As String, Optional fSafeText As Boolean = True, Optional fIsChecked As Boolean = False)  ' D1748 + D2247
            Dim tItem As New ListItem
            'sComment = ShortString(sComment, 200, True) '   D2470
            If fSafeText Then Name = JS_SafeHTML(Name) ' D1748
            tItem.Text = Name   ' D0042 + D1748
            If CanEditInfodocs AndAlso ID >= 0 Then Intensities += String.Format("<input type='hidden' name='Intensity{0}' value='{1}'/>", ID, sComment) ' D2281
            ' D2247 ===
            If sComment <> "" OrElse (CanEditInfodocs AndAlso ID >= 0) Then ' D2281
                If fSafeText Then sComment = JS_SafeHTML(sComment)
                If ShowFramedInfodocs Then
                    Dim sDesc As String = ""
                    If CanEditInfodocs Then sDesc = String.Format("&nbsp;&nbsp;<a href='' onclick='EditIntensityDesc({0}, {3}); return false;'><img id='imgDesc{0}' src='{1}edit_tiny.gif' width=10 height=10 border=0 title='{2}'/></a>", ID, ImagePath, lblIntensityDescriptionInput, IIf(rbListRatings.Items.Count > 4, 1, 0)) ' D3008
                    sDesc = String.Format("<span class='text small' style='color:#336666'><span id='IntRow{1}' style='max-width:250px;' title='{0}'>{3}</span>{2}</span>", SafeFormString(sComment), ID, sDesc, IIf(sComment <> "", " &nbsp;&middot;&nbsp;" + SafeFormString(ShortString(sComment, 100, True)), "")) ' D2470 + D3009
                    tItem.Text += sDesc
                Else
                    ' D2281 ===
                    Dim sDesc As String = String.Format("<img src='{0}info12{3}.png' width=12 height=12 border=0 id='Int{2}'/>", ImagePath, JS_SafeString(sComment), ID, IIf(sComment <> "", "", "_dis"))    ' D2470 + D3008
                    If CanEditInfodocs Then sDesc = String.Format("<a href='' onclick='EditIntensityDesc({1}, {2}); return false;'>{0}</a>", sDesc, ID, IIf(rbListRatings.Items.Count > 4, 1, 0))
                    tItem.Text += "&nbsp;" + sDesc
                    ' D2281 ==
                End If
            End If
            ' D2247 ==
            tItem.Value = ID.ToString
            tItem.Selected = (ID = GetValue() OrElse fIsChecked)    ' D1748
            tItem.Attributes.Add("onclick", String.Format("SetRating({0});", ID))
            'tItem.Attributes.Add("onkeypress", String.Format("SetRating({0})", ID))    ' -D1748
            rbListRatings.Items.Add(tItem)
        End Sub

        Protected Sub Page_Load(sender As Object, e As EventArgs)
            lblCaption.Text = ObjectiveName ' D2181
            trCaption.Visible = Not ShowFramedInfodocs  ' D2181
            If Not Data Is Nothing Then
                CheckAndFlashWRTCaption(lblCaption.ClientID, ParentNodeID) ' D1139 + D2503 + D2505
                rbListRatings.Items.Clear()
                'For Each tRating As clsRating In Data.RatingsSet'Cv2
                AddRating(-1, NotRatedCaption, "", True)    ' D1748 + D2247
                If MeasurementScale IsNot Nothing Then ' D2505
                    For Each tRating As clsRating In CType(MeasurementScale, clsRatingScale).RatingSet 'Cv2 'C0464
                        ' D4170 ===
                        Dim sName As String = tRating.Name
                        If ShowPrtyValues Then sName = String.Format("{0} ({1})", tRating.Name, Double2String(100 * tRating.Value, Precision, True))
                        AddRating(tRating.ID, sName, tRating.Comment, True)   ' D0258 + D1748 + D2120 + D2247 + D3229
                        ' D4170 ==
                    Next
                End If
                ' D1748 ===
                Dim sDirectValue As String = ""
                Dim tJud As clsRatingMeasureData = CType(CType(Data, clsOneAtATimeEvaluationActionData).Judgment, clsRatingMeasureData)
                If Not tJud.IsUndefined AndAlso tJud.Rating IsNot Nothing AndAlso tJud.Rating.RatingScaleID < 0 Then
                    ' D1788 ===
                    sDirectValue = tJud.SingleValue.ToString("F8")    ' D1745 + D1747
                    While sDirectValue.Length > 3 AndAlso sDirectValue(sDirectValue.Length - 1) = "0"
                        sDirectValue = sDirectValue.Substring(0, sDirectValue.Length - 1)
                    End While
                    ' D1788 ==
                End If
                If ShowDirectValue Then AddRating(-2, String.Format("</label><span onclick='theForm.DirectRating.focus();'>{0}: </span><input type='text' class='input' style='width:100px' name='DirectRating' value='{1}' onfocus='var f = $get(""{2}_{3}""); if ((f) && !f.checked) {{ f.checked=true; this.focus(); }}' onblur_='onKeyUpDirect(true);' onkeyup='onKeyUpDirect(false);' onmousedown='focusOnClick(this);'><label>", lblDirectValue, sDirectValue, rbListRatings.ClientID, rbListRatings.Items.Count), "", False, sDirectValue <> "") ' D1748 + D2007 + D2247 + D4170
                ' D1748 ==

                ' D1006 + D2994 ===
                InitInfodocImage(imgCaptionInfodoc, tooltipGoal, CaptionInfodoc, CaptionInfodocEditURL, String.Format(lblInfodocTitleGoal, ShortString(ObjectiveName, MaxLenInfodocCaption)), imageInfodocEmpty, CaptionInfodocURL)  ' D1064 + D2181
                InitInfodocImage(imgAltInfodoc, tooltipAltInfodoc, AlternativeInfodoc, AlternativeInfodocEditURL, String.Format(lblInfodocTitleNode, ShortString(AlternativeName, MaxLenInfodocCaption)), imageInfodocEmpty, AlternativeInfodocURL)    ' D1003 + D1004 + D1064
                If WRTInfodocEditURL <> "" OrElse WRTInfodocURL <> "" Then InitInfodocImage(imgWRTInfodoc, tooltipWRTInfodoc, WRTInfodoc, WRTInfodocEditURL, String.Format(lblInfodocTitleWRT, ShortString(AlternativeName, MaxLenInfodocCaption - 5), ShortString(ObjectiveName, MaxLenInfodocCaption - 5)), imageWRTInfodocEmpty, WRTInfodocURL) Else imgWRTInfodoc.Visible = False ' D1064 + D2181 + D4346
                ' D2994 ==
                If MeasurementScale IsNot Nothing Then InitInfodocImage(imgScaleInfodoc, tooltipScaleInfodoc, ScaleInfodoc, ScaleInfodocEditURL, String.Format(lblInfodocTitleScale, ShortString(MeasurementScale.Name, MaxLenInfodocCaption)), imageInfodocEmpty, ScaleInfodocURL) ' D1064 + D2505
                imgScaleInfodoc.Attributes("align") = "left"   ' D2250
                ' D1006 ==

                ' D1214 ===
                If ShowFramedInfodocs Then
                    ' D2994 ===
                    InitRFrame(frmInfodocGoal, String.Format(lblInfodocTitleGoal, ShortString(ObjectiveName, MaxLenInfodocCaption, True)), CaptionInfodocURL, CaptionInfodocEditURL, "node", CaptionInfodocCollapsed, isHTMLEmpty(CaptionInfodoc), 300, 90, "", "r_node") ' D2181
                    InitRFrame(frmInfodocNode, String.Format(lblInfodocTitleNode, ShortString(AlternativeName, MaxLenInfodocCaption, True)), AlternativeInfodocURL, AlternativeInfodocEditURL, "alt", AlternativeInfodocCollapsed, isHTMLEmpty(AlternativeInfodoc), 300, 90, "", "r_alt")
                    If WRTInfodocEditURL <> "" OrElse WRTInfodocURL <> "" Then InitRFrame(frmInfodocWRT, String.Format(lblInfodocTitleWRT, ShortString(AlternativeName, MaxLenInfodocCaptionWRT, True), ShortString(ObjectiveName, MaxLenInfodocCaptionWRT, True)), WRTInfodocURL, WRTInfodocEditURL, "wrt", WRTInfodocCollapsed, isHTMLEmpty(WRTInfodoc), 300, 90, "", "r_wrt") Else frmInfodocWRT.Visible = False ' D2181 + D4346
                    ' D2994 ==
                    If MeasurementScale IsNot Nothing Then InitRFrame(frmInfodocScale, String.Format(lblInfodocTitleScale, ShortString(MeasurementScale.Name, MaxLenInfodocCaptionWRT, True)), ScaleInfodocURL, ScaleInfodocEditURL, "scale", ScaleInfodocCollapsed, isHTMLEmpty(ScaleInfodoc), 300, 90, "", "scale") ' D2249 + D2505
                Else
                    If CanEditInfodocs Then ScriptManager.RegisterStartupScript(Me, GetType(String), "IntiDesc", "setTimeout('SetDesc();', 300);", True) ' D3008 + D3425 + D4160
                End If
                ' D1214 ==

                If imgAltInfodoc.Visible Then imgAltInfodoc.Attributes.Add("style", "margin-left:3px")

                If Not IsPostBack Then
                    ScriptManager.RegisterStartupScript(Me, GetType(String), "IntiLst", "setTimeout('onResizeRatings();', 150);", True)    ' D3470
                    ScriptManager.RegisterOnSubmitStatement(Page, GetType(String), "Check", "return CheckForm();") ' D1313
                End If
            End If
        End Sub

    End Class

End Namespace