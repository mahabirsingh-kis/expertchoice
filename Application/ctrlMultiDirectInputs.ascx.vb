Imports Telerik.Web.UI

Namespace ExpertChoice.Web.Controls

    Partial Public Class ctrlMultiDirectInputs

        Inherits ctrlEvaluationControlBase    ' D1012

        Private _Values As List(Of clsRatingLine)   ' D1012
        'Private _Caption As String = ""    ' -D2181

        ' D1012 ===
        Private _ParentNodeID As String = ""        ' D1141 + D2503
        Private _CaptionName As String = ""         ' D1007
        Private _CaptionInfodoc As String = ""      ' D0108
        Private _CaptionInfodocURL As String = ""   ' D1064
        Private _CaptionInfodocEditURL As String = ""   ' D1007
        Public _TooltipsInfodocList As String = ""  ' D1007
        Public _TooltipsWRTList As String = ""      ' D1007
        Public _ImagesList As String = ""           ' D1008
        Public _ImagesWRTList As String = ""        ' D1008
        Public _ImagesCommentsList As String = ""   ' D2719
        Private _ShowWRTInfodocs As Boolean = False ' D1012
        ' D1012 ==
        ' D1373 ===
        Private _CaptionInfodocCollapsed As Boolean = False
        Private _AlternativeInfodocCollapsed As Boolean = False
        Private _WRTInfodocCollapsed As Boolean = False
        Public _InfodocURLsList As String = ""
        Public _WRTInfodocURLsList As String = ""
        Public _InfodocTitlesList As String = ""
        Public _InfodocTitlesWRTList As String = ""
        Public _InfodocEditURLsList As String = ""
        Public _WRTInfodocEditURLsList As String = ""
        ' D1373 ==
        Private _CaptionSaveComment As String = "OK"
        Private _ImageCommentEmpty As String = "note_.gif"
        Private _ImageCommentExists As String = "note.gif"
        Public _precValue As Integer = 2        ' D2138
        Public _precCoeff As Integer = 100      ' D0684 + D2138
        Private _msgWrongNumber As String = "Wrong Number"
        Private _msgWrongNumberRange As String = "Number must be between 0 and 1"

        Public SliderIDs As String = ""     ' D2138
        Public InputIDs As String = ""      ' D0684

        Public SliderWidth As Integer = 100 ' D0684
        Public Const ShowSlider As Boolean = True   ' D0684

        Public frmInfodocGoal As ctrlFramedInfodoc = Nothing    ' D1901
        Public frmInfodocNode As ctrlFramedInfodoc = Nothing    ' D1901
        Public ReverseInfodocFrames As Boolean = False  ' D1901

        Public _FocusID As Integer = -1             ' D1373 + D1425

        Public Const _HIDDEN As String = ctrlFramedInfodoc._HIDDEN      ' D1373
        Public Const _VISIBLE As String = ctrlFramedInfodoc._VISIBLE    ' D1373

        Public sRootPath As String = ""     ' D1593

        ' D1141 ===
        Public Property ParentNodeID() As String    ' D2503
            Get
                Return _ParentNodeID
            End Get
            Set(value As String)  ' D2503
                _ParentNodeID = value
            End Set
        End Property
        ' D1141 ==

        ' -D2181
        'Public Property Caption() As String
        '    Get
        '        Return _Caption
        '    End Get
        '    Set(ByVal value As String)
        '        _Caption = value
        '    End Set
        'End Property

        ' D1373 ===
        Public Property CaptionName() As String
            Get
                Return _CaptionName
            End Get
            Set(value As String)
                _CaptionName = value
            End Set
        End Property
        ' D1373 ==

        Public Property CaptionInfodoc() As String
            Get
                Return _CaptionInfodoc
            End Get
            Set(value As String)
                _CaptionInfodoc = value
            End Set
        End Property

        ' D1012 ===
        Public Property CaptionInfodocEditURL() As String
            Get
                Return _CaptionInfodocEditURL
            End Get
            Set(value As String)
                _CaptionInfodocEditURL = value
            End Set
        End Property
        ' D1012 ==

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

        ' D1373 ===
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
        ' D1373 ==

        Public Property CaptionSaveComment() As String
            Get
                Return _CaptionSaveComment
            End Get
            Set(value As String)
                _CaptionSaveComment = value
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

        Public Property msgWrongNumberRange() As String
            Get
                Return _msgWrongNumberRange
            End Get
            Set(value As String)
                _msgWrongNumberRange = value
            End Set
        End Property

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

        ' D1012 ===
        Public Property Values() As List(Of clsRatingLine)
            Get
                Return _Values
            End Get
            Set(value As List(Of clsRatingLine))
                _Values = value
            End Set
        End Property
        ' D1012 ==

        Public Function GetDICount() As Integer
            If Values Is Nothing Then Return 0 Else Return Values.Count
        End Function

        Public Property ValuePrecision() As Integer
            Get
                Return _precValue
            End Get
            Set(value As Integer)
                _precValue = value
                If _precCoeff < 10 Then _precCoeff = 10 ' D0684
                _precCoeff = CInt(Math.Pow(10, value))  ' D0684
            End Set
        End Property

        ' D1012 ===
        Public Property ShowWRTInfodocs() As Boolean
            Get
                Return _ShowWRTInfodocs
            End Get
            Set(value As Boolean)
                _ShowWRTInfodocs = value
            End Set
        End Property
        ' D1012 ==

        ' D0684 ===
        Public ReadOnly Property SliderCoeff() As Integer
            Get
                Return _precCoeff
            End Get
        End Property
        ' D0684 ==

        Public Function GetDDValue(val As Single) As String
            If Not Single.IsNaN(val) Then
                If val < 0 Then Return "0"
                If val > 1 Then Return "1"
                Return JS_SafeNumber(Math.Round(val, _precValue))
            End If
            Return ""
        End Function

        Protected Sub Page_Load(sender As Object, e As EventArgs)
            lblCaption.Text = CaptionName
            CheckAndFlashWRTCaption(lblCaption.ClientID, ParentNodeID) ' D1141
            ' D1012 ===
            InitInfodocImage(imgCaptionInfodoc, tooltipGoal, CaptionInfodoc, CaptionInfodocEditURL, String.Format(lblInfodocTitleGoal, ShortString(CaptionName, MaxLenInfodocCaption)), imageInfodocEmpty, CaptionInfodocURL)   ' D1064 + D2181
            If Values IsNot Nothing Then
                rptDI.DataSource = Values
                rptDI.DataBind()
            End If
            ' D1012 ==
            If Not IsPostBack Then
                Page.ClientScript.RegisterOnSubmitStatement(GetType(String), "CheckForm", "; return CheckDIForm();")
                Page.ClientScript.RegisterStartupScript(GetType(String), "InitForm", "InitSliders();", True)    ' D2138
            End If
        End Sub

        Protected Sub rptDI_ItemDataBound(sender As Object, e As RepeaterItemEventArgs)
            If Not e.Item.DataItem Is Nothing Then
                Dim D As clsRatingLine = CType(e.Item.DataItem, clsRatingLine)

                ' D0684 ===
                Dim SliderID As String = ""

                Dim td As HtmlTableCell = CType(e.Item.FindControl("tdSlider"), HtmlTableCell)
                If td IsNot Nothing Then
                    td.Visible = ShowSlider
                    td.Width = CStr(SliderWidth + 12)  ' D2138
                End If

                SliderIDs += CStr(IIf(SliderIDs = "", "", ",")) + "'" + CStr(D.ID) + "'"    ' D2138
                InputIDs += CStr(IIf(InputIDs = "", "", ",")) + "'DI" + CStr(D.ID) + "'"    ' D0685
                ' D0684 ==

                ' D1373 ===
                _InfodocURLsList += CStr(IIf(_InfodocURLsList = "", "", ",")) + "'" + JS_SafeString(IIf(Not CanEditInfodocs AndAlso D.Infodoc = "", "", D.InfodocURL)) + "'"  ' D2412
                _WRTInfodocURLsList += CStr(IIf(_WRTInfodocURLsList = "", "", ",")) + "'" + JS_SafeString(IIf(Not CanEditInfodocs AndAlso D.InfodocWRT = "", "", D.InfodocWRTURL)) + "'"  ' D2412
                _InfodocEditURLsList += CStr(IIf(_InfodocEditURLsList = "", "", ",")) + """" + JS_SafeString(IIf(CanEditInfodocs, D.InfodocEditURL, String.Format("open_infodoc('{0}');", JS_SafeString(D.InfodocURL)))) + """"  ' D1571
                _WRTInfodocEditURLsList += CStr(IIf(_WRTInfodocEditURLsList = "", "", ",")) + """" + JS_SafeString(IIf(CanEditInfodocs, D.InfodocWRTEditURL, String.Format("open_infodoc('{0}');", JS_SafeString(D.InfodocWRTURL)))) + """"  ' D1571
                _InfodocTitlesList += CStr(IIf(_InfodocTitlesList = "", "", ",")) + "'" + JS_SafeString(String.Format(lblInfodocTitleNode, ShortString(D.Title, MaxLenInfodocCaption, False))) + "'"
                ' D1902 ===
                If ReverseInfodocFrames Then
                    _InfodocTitlesWRTList += CStr(IIf(_InfodocTitlesWRTList = "", "", ",")) + "'" + JS_SafeString(String.Format(lblInfodocTitleWRT, ShortString(CaptionName, MaxLenInfodocCaptionWRT - 2, False), ShortString(D.Title, MaxLenInfodocCaptionWRT - 2, False))) + "'"
                Else
                    _InfodocTitlesWRTList += CStr(IIf(_InfodocTitlesWRTList = "", "", ",")) + "'" + JS_SafeString(String.Format(lblInfodocTitleWRT, ShortString(D.Title, MaxLenInfodocCaptionWRT - 2, False), ShortString(CaptionName, MaxLenInfodocCaptionWRT - 2, False))) + "'"
                End If
                ' D1902 ==
                ' D1373 ==

                If Single.IsNaN(D.DirectData) AndAlso _FocusID < 0 Then _FocusID = e.Item.ItemIndex ' D1373

                ' D1012 ===
                If ShowWRTInfodocs Then
                    Dim tooltipWRT As RadToolTip = DirectCast(e.Item.FindControl("tooltipWRT"), RadToolTip)
                    Dim imgWRT As Image = DirectCast(e.Item.FindControl("imgWRT"), Image)

                    If imgWRT IsNot Nothing Then
                        _ImagesWRTList += CStr(IIf(_ImagesWRTList = "", "", ", ")) + "'" + imgWRT.ClientID + "'"
                        If tooltipWRT IsNot Nothing Then
                            _TooltipsWRTList += CStr(IIf(_TooltipsWRTList = "", "", ", ")) + "'" + tooltipWRT.ClientID + "'"
                            tooltipWRT.TargetControlID = imgWRT.ID
                        End If
                        If ShowWRTInfodocs Then InitInfodocImage(imgWRT, tooltipWRT, D.InfodocWRT, D.InfodocWRTEditURL, String.Format(lblInfodocTitleWRT, ShortString(D.Title, MaxLenInfodocCaption - 5), ShortString(CaptionName, MaxLenInfodocCaption - 1)), imageWRTInfodocEmpty, D.InfodocWRTURL) ' D1064 + D2420
                    End If
                End If

                Dim tooltipInfodoc As RadToolTip = DirectCast(e.Item.FindControl("tooltipInfodoc"), RadToolTip)
                Dim imgInfodoc As Image = DirectCast(e.Item.FindControl("imgInfodoc"), Image)

                If imgInfodoc IsNot Nothing Then
                    _ImagesList += CStr(IIf(_ImagesList = "", "", ", ")) + "'" + imgInfodoc.ClientID + "'"
                    If tooltipInfodoc IsNot Nothing Then
                        _TooltipsInfodocList += CStr(IIf(_TooltipsInfodocList = "", "", ", ")) + "'" + tooltipInfodoc.ClientID + "'"
                        tooltipInfodoc.TargetControlID = imgInfodoc.ID
                    End If
                    InitInfodocImage(imgInfodoc, tooltipInfodoc, D.Infodoc, D.InfodocEditURL, String.Format(lblInfodocTitleNode, ShortString(D.Title, MaxLenInfodocCaption)), imageInfodocEmpty, D.InfodocURL) ' D1064
                End If
                ' D1012 ==

                ' D0467 ===
                Dim imgComment As Image = DirectCast(e.Item.FindControl("imgComment"), Image)
                Dim tooltipComment As RadToolTip = DirectCast(e.Item.FindControl("ttEditComment"), RadToolTip)
                If imgComment IsNot Nothing AndAlso tooltipComment IsNot Nothing Then
                    imgComment.Visible = ShowComment
                    tooltipComment.Visible = ShowComment
                    If ShowComment Then
                        imgComment.ImageUrl = CStr(IIf(D.Comment = "", ImageCommentEmpty, ImageCommentExists))
                        imgComment.Attributes.Add("onclick", String.Format("setTimeout('var c = theForm.Comment{0}; if ((c)) c.focus();', 500);", D.ID))   ' D2089                        
                        imgComment.Attributes.Add("onmouseover", "ChangeCommentIcon(this,1);")  ' D2769 
                        imgComment.Attributes.Add("onmouseout", "ChangeCommentIcon(this,0);")   ' D2769
                        _ImagesCommentsList += CStr(IIf(_ImagesCommentsList = "", "", ",")) + String.Format("'{0}'", imgComment.ClientID) ' D2719
                        tooltipComment.TargetControlID = imgComment.ClientID
                        tooltipComment.IsClientID = True

                        Dim btnSave As Button = DirectCast(e.Item.FindControl("btnSave"), Button)
                        If Not btnSave Is Nothing Then
                            btnSave.Text = CaptionSaveComment
                            btnSave.OnClientClick = String.Format("ApplyComment('{0}', '{1}','{2}'); return false;", D.ID, JS_SafeString(tooltipComment.ClientID), imgComment.ClientID)
                        End If
                    End If
                End If
                ' D0467 ==
            End If
        End Sub

        ' D1373 ===
        Protected Sub Page_PreRender(sender As Object, e As EventArgs) Handles Me.PreRender
            If _FocusID < 0 Then _FocusID = 0
            If Not Page Is Nothing AndAlso Not IsPostBack AndAlso ShowFramedInfodocs AndAlso Values IsNot Nothing AndAlso Values.Count >= _FocusID Then ' D1428
                Dim tRow As clsRatingLine = Values(_FocusID)

                ' D1901 ===
                frmInfodocGoal = CType(Me.LoadControl("~/ctrlFramedInfodoc.ascx"), ctrlFramedInfodoc)
                frmInfodocGoal.ID = "frmInfodocGoal"
                frmInfodocGoal.OptionsID = frmInfodocGoal.ID
                frmInfodocNode = CType(LoadControl("~/ctrlFramedInfodoc.ascx"), ctrlFramedInfodoc)
                frmInfodocNode.ID = "frmInfodocNode"
                frmInfodocNode.OptionsID = frmInfodocNode.ID

                If ReverseInfodocFrames AndAlso CaptionInfodoc IsNot Nothing Then   ' D3250
                    If CaptionInfodoc IsNot Nothing Then tdLeft.Controls.Add(frmInfodocGoal) ' D3250
                    tdCenter.Controls.Add(frmInfodocNode)
                Else
                    tdLeft.Controls.Add(frmInfodocNode)
                    If CaptionInfodoc IsNot Nothing Then tdCenter.Controls.Add(frmInfodocGoal) ' D3250
                End If
                ' D1901 ==

                ' D1845 + D2994 ===
                InitRFrame(frmInfodocNode, String.Format(lblInfodocTitleNode, ShortString(tRow.Title, MaxLenInfodocCaption, True)), tRow.InfodocURL, tRow.InfodocEditURL, "alt", AlternativeInfodocCollapsed, isHTMLEmpty(tRow.Infodoc), 300, 90, "frmInfodocGoal,frmInfodocWRT", "md", If(CanEditInfodocs, True, Values.FirstOrDefault(Function(r) (Not isHTMLEmpty(r.Infodoc))) IsNot Nothing))  ' D4551
                If CaptionInfodoc IsNot Nothing Then   ' D3250
                    InitRFrame(frmInfodocGoal, String.Format(lblInfodocTitleGoal, ShortString(CaptionName, MaxLenInfodocCaption, True)), CaptionInfodocURL, CaptionInfodocEditURL, "node", CaptionInfodocCollapsed, isHTMLEmpty(CaptionInfodoc), 300, 90, "frmInfodocNode,frmInfodocWRT", "md", False)
                    If ShowWRTInfodocs Then InitRFrame(frmInfodocWRT, String.Format(lblInfodocTitleWRT, ShortString(tRow.Title, MaxLenInfodocCaptionWRT - 2, True), ShortString(CaptionName, MaxLenInfodocCaptionWRT - 2, True)), tRow.InfodocWRTURL, tRow.InfodocWRTEditURL, "wrt", WRTInfodocCollapsed, isHTMLEmpty(tRow.InfodocWRT), 300, 90, "frmInfodocNode,frmInfodocGoal", "md", If(CanEditInfodocs, True, Values.FirstOrDefault(Function(r) (Not isHTMLEmpty(r.InfodocWRT))) IsNot Nothing)) ' D4551
                Else
                    frmInfodocWRT.Visible = False   ' D3250
                End If
                ' D1845 + D2994 ==
            End If
            ScriptManager.RegisterStartupScript(Page, GetType(String), "InitFocus", String.Format("{0}setTimeout('SetFocus(" + _FocusID.ToString + ", 1, 1);',100); InitComments();", IIf(Request.Browser.Browser.ToLower.Contains("firefox"), "ResizeList(); ", "")), True)   ' D1844 + D3332 + D3342 + D3425
            'ScriptManager.RegisterStartupScript(Page, GetType(String), "InitFocus", String.Format("{0}alert(1);InitCursor();setTimeout('SetFocus(" + _FocusID.ToString + ", 1, 1); InitComments();", IIf(Request.Browser.Browser.ToLower.Contains("firefox"), "ResizeList(); ", "")), True)   ' D1844 + D3332 + D3342
        End Sub
        ' D1373 ==

    End Class

End Namespace