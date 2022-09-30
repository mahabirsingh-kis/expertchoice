Imports ExpertChoice.Data
Imports System.Text.RegularExpressions
Imports System.Globalization

Namespace ExpertChoice.Service

    Public Module HTMLService

        Public Const _KeepFramedInfodocsForAMonth As Boolean = True    ' D1063 + D3675

        ' D0027 ===
        ''' <summary>
        ''' Create HTML tags with GraphBar
        ''' </summary>
        ''' <param name="Value">Current value of bar</param>
        ''' <param name="MaxValue"></param>
        ''' <param name="BWidth">Bar width in pixels</param>
        ''' <param name="BHeight">Bar height in pixels</param>
        ''' <param name="sStyle">Used style for bar</param>
        ''' <param name="BlankImagePath">Link to image with "blank" gif</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function HTMLCreateGraphBar(ByVal Value As Single, ByVal MaxValue As Single, ByVal BWidth As Integer, ByVal BHeight As Integer, ByVal sStyle As String, ByVal BlankImagePath As String, Optional ByVal sGraphHint As String = "", Optional fShowValueOverride As Boolean = False, Optional sImgID As String = "") As String    ' D0136 + D0323 + D2188 + D3354
            If MaxValue < 0 Then Return "&nbsp;"
            ' D0028 ===
            If MaxValue = 0 Then Value = 0 Else Value = CSng(Value / (MaxValue + 0.00000001))

            Dim sBar As String = "<div class='progress' style='height:{0}px;width:{1}px;padding:1px'>{2}</div>"
            Dim sFill As String = "<span class='{3}' style='float:left;'><img src='{4}' width={1} height={0} border=0 id='{6}' title='{5}'></span>"  ' D0323 + D2196
            If fShowValueOverride AndAlso sGraphHint <> "" Then sFill = String.Format("<span class='bar_value' style='float:right; margin:{2}px 0px 0px 0px; padding:0px;'>{1}</span>", BWidth - 4, sGraphHint, (BHeight - 12) \ 2) + sFill ' D2188 + D2196 + D3354

            Dim FillWidth As Integer = -1
            If Value >= 0 And Value <= 100 Then FillWidth = CInt(Math.Round((BWidth) * Value)) ' D0147
            If FillWidth < 0 Then FillWidth = 0
            If FillWidth > BWidth Then FillWidth = BWidth

            'sFill = String.Format(sFill, BHeight, FillWidth, 100 * Value, CStr(IIf(FillWidth = 0, "", sStyle)), BlankImagePath, IIf(sGraphHint = "", String.Format("{0}%", 100 * Value), sGraphHint), sImgID) ' D0136 + D0323 + D3354 + D4830
            sFill = String.Format(sFill, BHeight, FillWidth, 100 * Value, sStyle, BlankImagePath, IIf(sGraphHint = "", String.Format("{0}%", 100 * Value), sGraphHint), sImgID) ' D0136 + D0323 + D3354 + D4830

            Return String.Format(sBar, BHeight, BWidth, sFill)
            ' D0028 ==
        End Function
        ' D0027 ==

        ' D0426 ===
        Public Function HTMLFileLink(ByVal sLInk As String, ByVal sRealFilename As String, ByVal sTitle As String) As String
            If My.Computer.FileSystem.FileExists(sRealFilename) Then
                Dim fi As System.IO.FileInfo = My.Computer.FileSystem.GetFileInfo(sRealFilename)
                Return String.Format("<a href='{1}'>{0}</a> ({2})", sTitle, Uri.EscapeUriString(sLInk), FileSize(fi.Length))
            Else
                Return String.Format("<span class='gray'>{0}</span>", sTitle)
            End If
        End Function
        ' D0426 ==

        ' D0091 ===
        ''' <summary>
        ''' Create Link with e-mail address.
        ''' </summary>
        ''' <param name="sText">Link text</param>
        ''' <param name="sEmail">Could be empty</param>
        ''' <returns>Link like mailto:address </returns>
        ''' <remarks></remarks>
        Public Function HTMLEmailLink(ByVal sText As String, ByVal sEmail As String, Optional ByVal sLinkOtherText As String = "") As String    ' D0195
            If sEmail <> "" Then sText = String.Format("<a href='mailto:{0}'{2}>{1}</a>", sEmail, IIf(sText = "", sEmail, sText), CStr(IIf(sLinkOtherText <> "", " " + sLinkOtherText, ""))) ' D0136 + D0194 + D0232
            Return sText
        End Function

        Public Function HTMLTextLink(ByVal sURL As String, ByVal sText As String, Optional ByVal fAvailable As Boolean = True, Optional ByVal sOtherLinkParam As String = "", Optional ByVal sInactiveStyle As String = "inactive") As String
            Dim sLink As String = ""
            If fAvailable And sURL <> "" Then
                If sOtherLinkParam <> "" Then sOtherLinkParam = " " + sOtherLinkParam
                sLink = String.Format("<a href='{0}'{1}>{2}</a>", sURL, sOtherLinkParam, sText)
            Else
                sLink = CStr(IIf(sInactiveStyle <> "", String.Format("<span class='{0}'>{1}</span>", sInactiveStyle, sText), sText))
            End If
            Return sLink
        End Function

        Public Function HTMLUserLinkEmail(ByVal tUser As clsApplicationUser, Optional ByVal sLinkOtherText As String = "") As String  ' D0223 + D0459
            If tUser Is Nothing Then Return "" Else Return HTMLEmailLink(tUser.UserName, tUser.UserEmail, sLinkOtherText)
        End Function
        ' D0091 ==

        ' D0688 ===
        Public Function HTMLCollapsedObject(ByVal sNameContent As String, ByVal sNameCollapsed As String, ByVal isContent As Boolean, ByVal isCollapsed As Boolean, ByVal sCookieName As String, ByVal sImagePath As String, Optional ByVal sSwitcherText As String = "", Optional ByVal isEmptyContent As Boolean = False, Optional ByVal sRelatedContent As String = "", Optional ByVal sRelatedCollapsed As String = "", Optional ByVal sCodeOnClick As String = "") As String ' D0997 + D1064 + D1065
            'Const _postfix_content As String = "content"
            'Const _postfix_collapsed As String = "collapsed"
            If Not String.IsNullOrEmpty(sRelatedContent) Then sRelatedContent = """,""" + sRelatedContent ' D1064
            If Not String.IsNullOrEmpty(sRelatedCollapsed) Then sRelatedCollapsed = """,""" + sRelatedCollapsed ' D1064
            Return String.Format("<span id='{2}'{3}><a href='' onclick='{11}; return SwitchBlock([""{0}{9}""],[""{1}{10}""],""{6}"",""{7}"");' class='actions dashed'><img src='" + sImagePath + "{4}' width=9 height=9 alt='' title='' border=0 style='margin:3px 3px 0px 0px; float:left; border:0px;'>{8}</a>{5}</span>", IIf(isContent, sNameContent, sNameCollapsed), IIf(isContent, sNameCollapsed, sNameContent), IIf(isContent, sNameContent, sNameCollapsed), IIf((isCollapsed And isContent) Or (Not isCollapsed And Not isContent), " style='display:none'", ""), IIf(isContent, IIf(isEmptyContent, "minus.gif", "minus_green.gif"), IIf(isEmptyContent, "plus.gif", "plus_green.gif")), "{0}", sCookieName, IIf(isContent, "0", "1"), sSwitcherText, sRelatedContent, sRelatedCollapsed, sCodeOnClick)  ' D0997 + D1064 + D1065
        End Function

        Public Function HTMLCollpasedContent(ByVal sNameContent As String, ByVal sNameCollapsed As String, ByVal sContentText As String, ByVal sCollapsedText As String, ByVal isCollapsed As Boolean, ByVal sCookieName As String, ByVal sImagePath As String, Optional ByVal sSwitcherText As String = "", Optional ByVal isEmptyContent As Boolean = False, Optional ByVal sRelatedContent As String = "", Optional ByVal sRelatedCollapsed As String = "", Optional ByVal sScriptOnExpand As String = "", Optional ByVal sScriptOnCollapse As String = "") As String    ' D0997 + D1064 + D1065
            Return String.Format(HTMLCollapsedObject(sNameContent, sNameCollapsed, True, isCollapsed, sCookieName, sImagePath, sSwitcherText, isEmptyContent, sRelatedContent, sRelatedCollapsed, sScriptOnCollapse), sContentText) + _
                   String.Format(HTMLCollapsedObject(sNameContent, sNameCollapsed, False, isCollapsed, sCookieName, sImagePath, sSwitcherText, isEmptyContent, sRelatedCollapsed, sRelatedContent, sScriptOnExpand), sCollapsedText)     ' D0997 + D1064 + D1065
        End Function

        Public Function JS_CollapsedContent() As String
            Return "    function SwitchBlock(div_hide, div_show, cookie_name, cookie_value) {" + vbCrLf + _
                   "       for (var i=0; i<div_hide.length; i++) { var h = $get(div_hide[i]);  if ((h)) h.style.display = 'none'; }" + vbCrLf + _
                   "       for (var j=0; j<div_show.length; j++) { var s = $get(div_show[j]);  if ((s)) s.style.display = 'block'; }" + vbCrLf + _
                   "       if (cookie_name!='') document.cookie = cookie_name +'=' + cookie_value +';" + CStr(IIf(_KeepFramedInfodocsForAMonth, " expires = " + DateTime.UtcNow.AddMonths(1).ToString("U", CultureInfo.InvariantCulture) + ";", "")) + " path=/';" + vbCrLf + _
                   "       return false;" + vbCrLf + _
                   "    }"      ' D1063
        End Function
        ' D0688 ==

    End Module

End Namespace
