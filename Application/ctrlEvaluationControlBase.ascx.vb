Imports Telerik.Web.UI

Namespace ExpertChoice.Web.Controls

    Partial Public Class ctrlEvaluationControlBase

        Inherits UserControl

        Public MaxLenInfodocCaption As Integer = 45         ' D1058 + D1084
        Public MaxLenInfodocCaptionWRT As Integer = 20      ' D1084 + D1372 + D1375
        Public FrameLoadingStyle As String = "width:99%; height:99%; background: url({0}devex_loading.gif) no-repeat center center;"

        Public SizesList As New NameValueCollection         ' D1964

        Public imageInfodoc As String = "info15.png"                ' D2250
        Public imageInfodocEmpty As String = "info15_dis.png"       ' D2250
        Public imageWRTInfodoc As String = "readme.gif"             ' D1004
        Public imageWRTInfodocEmpty As String = "readme_dis.gif"    ' D1004

        Private _AllowSaveSize As Boolean = False     ' D1864
        Private _SaveSizeMessage As String = ""       ' D1864

        Private _imgPath As String = ""

        Private _Comment As String = ""
        Private _ShowComment As Boolean = False
        Private _lblComment As String = "Comment:"
        Private _CommentCollapsed As Boolean = True ' D1069

        Private _lblEraseJudgment As String = "Erase"

        Public _changeFlagID As String = ""
        Public _onChangeAction As String = ""

        Private _lblInfodocTitleGoal As String = "Description for {0}"
        Private _lblInfodocTitleNode As String = "Description for {0}"
        Private _lblInfodocTitleWRT As String = "Description for {0} WRT {1}"

        Private _CanEditInfodocs As Boolean = False
        Private _imgEditInfodoc As String = ""
        ' D1063 ===
        Private _imgEditTitle As String = ""
        Private _imgViewInfodoc As String = ""
        Private _imgViewTitle As String = ""
        ' D1063 ==

        Private _FramedInfodocs As Boolean = False

        Public Property ImagePath() As String
            Get
                Return _imgPath
            End Get
            Set(value As String)
                _imgPath = value
            End Set
        End Property

        Public Property CanEditInfodocs() As Boolean
            Get
                Return _CanEditInfodocs
            End Get
            Set(value As Boolean)
                _CanEditInfodocs = value
            End Set
        End Property

        Public Property ImageEditInfodoc() As String
            Get
                Return _imgEditInfodoc
            End Get
            Set(value As String)
                _imgEditInfodoc = value
            End Set
        End Property

        ' D1063 ===
        Public Property ImageEditTitle() As String
            Get
                Return _imgEditTitle
            End Get
            Set(value As String)
                _imgEditTitle = value
            End Set
        End Property

        Public Property ImageViewInfodoc() As String
            Get
                Return _imgViewInfodoc
            End Get
            Set(value As String)
                _imgViewInfodoc = value
            End Set
        End Property

        Public Property ImageViewTitle() As String
            Get
                Return _imgViewTitle
            End Get
            Set(value As String)
                _imgViewTitle = value
            End Set
        End Property
        ' D1063 ==

        Public Property ShowFramedInfodocs() As Boolean
            Get
                Return _FramedInfodocs
            End Get
            Set(value As Boolean)
                _FramedInfodocs = value
            End Set
        End Property

        Public Property lblInfodocTitleGoal() As String
            Get
                Return _lblInfodocTitleGoal
            End Get
            Set(value As String)
                _lblInfodocTitleGoal = value
            End Set
        End Property

        Public Property lblInfodocTitleNode() As String
            Get
                Return _lblInfodocTitleNode
            End Get
            Set(value As String)
                _lblInfodocTitleNode = value
            End Set
        End Property

        Public Property lblInfodocTitleWRT() As String
            Get
                Return _lblInfodocTitleWRT
            End Get
            Set(value As String)
                _lblInfodocTitleWRT = value
            End Set
        End Property

        Public Property Comment() As String
            Get
                Return _Comment
            End Get
            Set(value As String)
                _Comment = value
            End Set
        End Property

        Public Property ShowComment() As Boolean
            Get
                Return _ShowComment
            End Get
            Set(value As Boolean)
                _ShowComment = value
            End Set
        End Property

        ' D1069 ===
        Public Property isCommentCollapsed() As Boolean
            Get
                Return _CommentCollapsed
            End Get
            Set(value As Boolean)
                _CommentCollapsed = value
            End Set
        End Property
        ' D1069 ==

        Public Property ChangeFlagID() As String
            Get
                Return _changeFlagID
            End Get
            Set(value As String)
                _changeFlagID = value
            End Set
        End Property

        Public Property lblEraseJudgment() As String
            Get
                Return _lblEraseJudgment
            End Get
            Set(value As String)
                _lblEraseJudgment = value
            End Set
        End Property

        Public Property lblCommentTitle() As String
            Get
                Return _lblComment
            End Get
            Set(value As String)
                _lblComment = value
            End Set
        End Property

        Public Property onChangeAction() As String
            Get
                Return _onChangeAction
            End Get
            Set(value As String)
                _onChangeAction = value
            End Set
        End Property

        ' D1864 ===
        Public Property AllowSaveSize As Boolean
            Get
                Return _AllowSaveSize AndAlso _CanEditInfodocs
            End Get
            Set(value As Boolean)
                _AllowSaveSize = value
            End Set
        End Property

        Public Property SaveSizeMessage As String
            Get
                Return _SaveSizeMessage
            End Get
            Set(value As String)
                _SaveSizeMessage = value
            End Set
        End Property
        ' D1864 ==


        Public Function CreateLinkImage(sEditURL As String, sImage As String, sTitle As String) As String ' D1063
            Dim sRes As String = ""
            'If CanEditInfodocs AndAlso sEditURL <> "" Then ' -D1063
            sRes = String.Format("<a href='' onclick=""{0}; return false;""><img src='{1}' border=0 title='{2}' alt='{2}' align=right style='float:right; margin-left:3px'></a>", sEditURL, sImage, JS_SafeString(sTitle)) ' D1063
            'End If ' -D1063
            Return sRes
        End Function

        ' D1004 ===
        Public Function CreateFramedInfodoc(sFrameTitle As String, sInfodocURL As String, sInfodocEditURL As String, sFrameID As String, fIsCollapsed As Boolean, fIsEmptyInfodoc As Boolean, Optional ByVal sFrameWidth As String = "255px", Optional ByVal sFrameHeight As String = "150px", Optional ByVal sRelatedFrameID As String = "", Optional ByVal sFixedCookieName As String = "") As String  ' D1064 + D1069 + D1084
            If sInfodocURL <> "" Or (CanEditInfodocs AndAlso sInfodocEditURL <> "") Then
                If sFixedCookieName = "" Then sFixedCookieName = sFrameID + "_status" ' D1069
                Return String.Format("<div style='margin-bottom:2px'>{0}{1}</div>", IIf(CanEditInfodocs AndAlso sInfodocEditURL <> "", CreateLinkImage(sInfodocEditURL, ImagePath + ImageEditInfodoc, ImageEditTitle), CreateLinkImage(String.Format("open_infodoc('{0}');", JS_SafeString(sInfodocURL)), ImagePath + ImageViewInfodoc, ImageViewTitle)), HTMLCollpasedContent(sFrameID + "_content", sFrameID + "_collapsed", String.Format("<br><iframe id='{0}' style='border:1px solid #e0e0e0;' width='{1}' height='{2}' frameborder='0' allowtransparency='true' src='{3}' class='frm_loading' onload='InfodocFrameLoaded(this);'></iframe>", sFrameID, sFrameWidth, sFrameHeight, sInfodocURL), "", fIsCollapsed Or fIsEmptyInfodoc, sFixedCookieName, ImagePath, SafeFormString(sFrameTitle), fIsEmptyInfodoc, CStr(IIf(sRelatedFrameID = "", "", sRelatedFrameID + "_content")), CStr(IIf(sRelatedFrameID = "", "", sRelatedFrameID + "_collapsed"))))   ' D1063 + D1064 + D1069 + D1090
            Else
                Return "&nbsp;"
            End If
        End Function
        ' D1004 ==

        ' D1213 ===
        Public Sub InitRFrame(ByRef tFrame As ctrlFramedInfodoc, sFrameTitle As String, sInfodocURL As String, sInfodocEditURL As String, sFrameID As String, fIsCollapsed As Boolean, fIsEmptyInfodoc As Boolean, Optional ByVal sFrameWidth As Integer = 255, Optional ByVal sFrameHeight As Integer = 150, Optional ByVal sRelatedFrameIDs As String = "", Optional ByVal sFixedCookieName As String = "", Optional fAlwaysShowFrame As Boolean = False)   ' D1372 + D1845
            If tFrame IsNot Nothing Then
                If (fAlwaysShowFrame OrElse CanEditInfodocs) OrElse Not fIsEmptyInfodoc OrElse (CanEditInfodocs AndAlso sInfodocEditURL <> "") Then  ' D1448 + D1845 + D4319
                    'sFrameTitle = SafeFormString(sFrameTitle)
                    tFrame.ImagePlusURL = ImagePath + CStr(IIf(fIsEmptyInfodoc, "plus.gif", "plus_green.gif"))
                    tFrame.ImageMinusURL = ImagePath + CStr(IIf(fIsEmptyInfodoc, "minus.gif", "minus_green.gif"))
                    tFrame.ImageEditURL = ImagePath + CStr(IIf(CanEditInfodocs AndAlso sInfodocEditURL <> "", ImageEditInfodoc, ImageViewInfodoc))
                    tFrame.FrameURL = sInfodocURL
                    tFrame.FrameID = sFrameID
                    tFrame.CookieID = sFixedCookieName
                    tFrame.RelatedIDs = sRelatedFrameIDs    ' D1372
                    tFrame.onClientOnEditClick = CStr(IIf(CanEditInfodocs AndAlso sInfodocEditURL <> "", sInfodocEditURL, String.Format("open_infodoc('{0}');", JS_SafeString(sInfodocURL))))
                    tFrame.CaptionEdit = CStr(IIf(CanEditInfodocs, ImageEditTitle, ImageViewTitle))
                    tFrame.CaptionHidden = sFrameTitle
                    tFrame.CaptionVisible = sFrameTitle
                    tFrame.Width = sFrameWidth      ' D1421
                    tFrame.Height = sFrameHeight    ' D1421
                    If SizesList IsNot Nothing AndAlso SizesList(sFixedCookieName) IsNot Nothing Then
                        Dim W As Integer
                        Dim H As Integer
                        If ctrlFramedInfodoc.ParseSizes(SizesList(sFixedCookieName), W, H) Then
                            tFrame.Width = W
                            tFrame.Height = H
                        End If
                    End If
                    tFrame.isCollapsed = fIsCollapsed   ' D1591
                Else
                    tFrame.Visible = False
                End If
            End If
        End Sub
        ' D1213 ==

        Public Sub InitInfodocImage(ByRef tImage As Image, tTooltip As RadToolTip, sInfodoc As String, sEditInfodoc As String, sInfodocTitle As String, sImageEmptyInfodoc As String, sInfodocPath As String) ' D1004 + D1064
            tImage.Visible = Not ShowFramedInfodocs AndAlso (sInfodoc <> "" OrElse (CanEditInfodocs AndAlso sEditInfodoc <> ""))    ' D2411
            If Not tImage.Visible Then Exit Sub
            If tTooltip IsNot Nothing Then
                tTooltip.Visible = tImage.Visible
                If sInfodoc <> "" Then tTooltip.Text = sInfodoc
                If sInfodocTitle <> "" Then tTooltip.Title = String.Format("<div class='text' style='margin-right:2em; margin-bottom:3px; border-bottom:1px solid #cccccc;'><b>{0}</b></div>", sInfodocTitle) Else tTooltip.Text = "<br>" + tTooltip.Text
            End If
            If CanEditInfodocs Then
                If sInfodoc = "" Then tImage.ImageUrl = ImagePath + sImageEmptyInfodoc ' D1004
                If sEditInfodoc <> "" Then
                    tImage.CssClass = "aslink"
                    tImage.Attributes.Add("onclick", sEditInfodoc)
                End If
            Else
                If sInfodoc <> "" AndAlso sInfodocPath <> "" Then
                    tImage.CssClass = "aslink"
                    tImage.Attributes.Add("onclick", String.Format("open_infodoc('{0}');", JS_SafeString(sInfodocPath)))
                End If
            End If
        End Sub

        ' D1065 ===
        Public Function CreateCommentArea(sName As String, sOnChange As String) As String
            'Dim sTitle As String = String.Format("<b>{0}:</b>", lblCommentTitle)
            Dim sComment As String = String.Format("<br><textarea name='{2}' onchange='{0}' onkeypress='{0}' cols='50' rows='5' class='input' style='width:99%'>{1}</textarea>", sOnChange, Comment, sName)
            Return String.Format("<div class='text' style='margin:4px 0px 0px 0px; text-align:left'>{0}</div>", HTMLCollpasedContent("comment_content", "comment_collapsed", sComment, "&nbsp;", isCommentCollapsed, "comment_status", ImagePath, lblCommentTitle, Comment = "", "", "", String.Format("setTimeout(""if ((theForm.{0})) theForm.{0}.focus();"", 150);", sName), ""))    ' D1069
        End Function
        ' D1065 ==

        ' D1139 + D2503 ===
        Public Sub CheckAndFlashWRTCaption(sLabelClientID As String, sNodeParentID As String)
            Dim fNoFlash As Boolean = Session("WRTObj") IsNot Nothing AndAlso CStr(Session("WRTObj")) = sNodeParentID
            If Not fNoFlash OrElse fNoFlash AndAlso sLabelClientID <> "" Then
                Page.ClientScript.RegisterStartupScript(GetType(String), "Flash", String.Format("setTimeout(""DoFlashWRT('{0}');"", 1000);", sLabelClientID), True) ' D2830
                Session("WRTObj") = sNodeParentID
                ' D2503 ==
            End If
        End Sub
        ' D1139 ==

    End Class

End Namespace