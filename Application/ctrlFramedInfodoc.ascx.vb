Imports System.Globalization

Namespace ExpertChoice.Web.Controls

    Partial Public Class ctrlFramedInfodoc

        Inherits UserControl

        Public Shared HideFrameCaption As Boolean = False  ' D4713

        Public Const _OPTION_STATUS_SUFFIX As String = "_status"    ' D1211 + D1213
        Public Const _OPTION_WIDTH_SUFFIX As String = "_w"
        Public Const _OPTION_HEIGHT_SUFFIX As String = "_h"
        Public Const _OPTION_SAVE_SIZE_SUFFIX As String = "_save"   ' D1964

        Public Const _VISIBLE As String = "_frame_vis"
        Public Const _HIDDEN As String = "_frame_hid"

        Private _FrameURL As String = ""        ' D1213
        Private _FrameID As String = ""         ' D1213

        Private _CaptionHidden As String = "Show"
        Private _CaptionVisible As String = "Hide"
        Private _CaptionEdit As String = "Edit"
        Private _OptionsID As String = ""
        Private _RelatedIDs As String = ""  ' D1211
        Private _CookieID As String = ""    ' D1211

        Private _isCollapsed As Boolean = False

        Private _Option_StoreStatus As Boolean = True
        Private _Option_StoreSize As Boolean = True

        Private _ImageEditURL As String = ""
        Private _ImagePlusURL As String = ""
        Private _ImageMinusURL As String = ""

        ' D1211 ===
        Private _Height As Integer = 150
        Private _Width As Integer = 300
        Private _MinHeight As Integer = 32
        Private _MinWidth As Integer = 32
        Private _MaxHeight As Integer = 850
        Private _MaxWidth As Integer = 1250

        Private _onClientOnExpand As String = ""
        Private _onClientOnCollapse As String = ""
        ' D1211 ==
        Private _onClientOnEditClick As String = ""

        Public Property CaptionHidden() As String
            Get
                Return _CaptionHidden
            End Get
            Set(value As String)
                _CaptionHidden = value
            End Set
        End Property

        Public Property CaptionVisible() As String
            Get
                Return _CaptionVisible
            End Get
            Set(value As String)
                _CaptionVisible = value
            End Set
        End Property

        Public Property CaptionEdit() As String
            Get
                Return _CaptionEdit
            End Get
            Set(value As String)
                _CaptionEdit = value
            End Set
        End Property

        ' D1213 ===
        Public Property FrameURL() As String
            Get
                Return _FrameURL
            End Get
            Set(value As String)
                _FrameURL = value
            End Set
        End Property

        Public Property FrameID() As String
            Get
                Return _FrameID
            End Get
            Set(value As String)
                _FrameID = value
            End Set
        End Property
        ' D1213 ==

        Public Property OptionsID() As String
            Get
                If _OptionsID = "" Then Return ID Else Return _OptionsID
            End Get
            Set(value As String)
                _OptionsID = value
            End Set
        End Property

        '' D1212 ===
        'Public ReadOnly Property ContainerClientID() As String
        '    Get
        '        If lblInfodocPanel IsNot Nothing Then Return lblInfodocPanel.ClientID Else Return ""
        '    End Get
        'End Property
        '' D1212 ==

        ' D1211 ===
        Public Property RelatedIDs() As String
            Get
                Return _RelatedIDs
            End Get
            Set(value As String)
                _RelatedIDs = value
            End Set
        End Property

        Public Property CookieID() As String
            Get
                Return _CookieID
            End Get
            Set(value As String)
                _CookieID = value
            End Set
        End Property
        ' D1211 ==

        '' D1864 ===
        'Public Property AllowSaveSize As Boolean
        '    Get
        '        Return _AllowSaveSize
        '    End Get
        '    Set(value As Boolean)
        '        _AllowSaveSize = value
        '    End Set
        'End Property

        'Public Property SaveSizeMessage As String
        '    Get
        '        Return _SaveSizeMessage
        '    End Get
        '    Set(value As String)
        '        _SaveSizeMessage = value
        '    End Set
        'End Property
        '' D1864 ==

        Public Property ImagePlusURL() As String
            Get
                Return _ImagePlusURL
            End Get
            Set(value As String)
                _ImagePlusURL = value
            End Set
        End Property

        Public Property ImageMinusURL() As String
            Get
                Return _ImageMinusURL
            End Get
            Set(value As String)
                _ImageMinusURL = value
            End Set
        End Property

        Public Property ImageEditURL() As String
            Get
                Return _ImageEditURL
            End Get
            Set(value As String)
                _ImageEditURL = value
            End Set
        End Property

        Public Property onClientOnEditClick() As String
            Get
                Return _onClientOnEditClick
            End Get
            Set(value As String)
                _onClientOnEditClick = value
            End Set
        End Property

        ' D1211 ===
        Public Property onClientOnExpand() As String
            Get
                Return _onClientOnExpand
            End Get
            Set(value As String)
                _onClientOnExpand = value
            End Set
        End Property

        Public Property onClientOnCollpase() As String
            Get
                Return _onClientOnCollapse
            End Get
            Set(value As String)
                _onClientOnCollapse = value
            End Set
        End Property

        Public Property Width() As Integer
            Get
                Return _Width
            End Get
            Set(value As Integer)
                _Width = value
            End Set
        End Property

        Public Property Height() As Integer
            Get
                Return _Height
            End Get
            Set(value As Integer)
                _Height = value
            End Set
        End Property

        Public Property MinWidth() As Integer
            Get
                Return _MinWidth
            End Get
            Set(value As Integer)
                _MinWidth = value
            End Set
        End Property

        Public Property MinHeight() As Integer
            Get
                Return _MinHeight
            End Get
            Set(value As Integer)
                _MinHeight = value
            End Set
        End Property

        Public Property MaxWidth() As Integer
            Get
                Return _MaxWidth
            End Get
            Set(value As Integer)
                _MaxWidth = value
            End Set
        End Property

        Public Property MaxHeight() As Integer
            Get
                Return _MaxHeight
            End Get
            Set(value As Integer)
                _MaxHeight = value
            End Set
        End Property
        ' D1211 ==

        Public Property Options_StoreStatus() As Boolean
            Get
                Return _Option_StoreStatus
            End Get
            Set(value As Boolean)
                _Option_StoreStatus = value
            End Set
        End Property

        Public Property Options_StoreSize() As Boolean
            Get
                Return _Option_StoreSize
            End Get
            Set(value As Boolean)
                _Option_StoreSize = value
            End Set
        End Property

        Public Property isCollapsed() As Boolean
            Get
                Return _isCollapsed
            End Get
            Set(value As Boolean)
                _isCollapsed = value
            End Set
        End Property

        Private Function GetCookie(sName As String, mDefVal As Integer) As Integer
            If (Request.Cookies(sName) Is Nothing) Then Return mDefVal
            ' D1591 ===
            Dim tVal As Integer
            If Not Integer.TryParse(Request.Cookies(sName).Value, tVal) Then tVal = mDefVal
            Return tVal
            ' D1591 ==
        End Function

        Public ReadOnly Property CookieIsCollpased(Optional ByVal sCookieID As String = "") As Boolean  ' D1211
            Get
                Return GetCookie(CStr(IIf(sCookieID = "", OptionsID, sCookieID)) + _OPTION_STATUS_SUFFIX, 1) <> 1 ' D1211
            End Get
        End Property

        Public ReadOnly Property CookieWidth(Optional ByVal sCookieID As String = "") As Integer
            Get
                Return GetCookie(CStr(IIf(sCookieID = "", OptionsID, sCookieID)) + _OPTION_WIDTH_SUFFIX, _Width)
            End Get
        End Property

        Public ReadOnly Property CookieHeight(Optional ByVal sCookieID As String = "") As Integer
            Get
                Return GetCookie(CStr(IIf(sCookieID = "", OptionsID, sCookieID)) + _OPTION_HEIGHT_SUFFIX, _Height)
            End Get
        End Property

        Public Function CreateHeaderRow(isCollapsed As Boolean) As String ' D1063
            Dim sVis As String = OptionsID + _VISIBLE
            Dim sHid As String = OptionsID + _HIDDEN
            ' D1211 ===
            If Not String.IsNullOrEmpty(RelatedIDs) Then
                Dim IDs As String() = RelatedIDs.Split(CType(",", Char()))
                For Each sID As String In IDs
                    sVis += """, """ + sID + _VISIBLE
                    sHid += """, """ + sID + _HIDDEN
                Next
            End If
            Dim sCookieID As String = CookieID
            If String.IsNullOrEmpty(sCookieID) Then sCookieID = OptionsID
            ' D1211 ==
            Return String.Format("<nobr><a href='' onclick='SwitchRFrame([""{4}""], [""{5}""], ""{6}"", {7}); {9}; return false;'><img src='{2}' border=0 title='Click to expand or collapse description' style='margin-right:3px; border:0px; text-decoration:none;'></a><span id='lbl_{10}'{11}>{0}</span><img src='{8}' border=0 title='{3}' alt='{3}' style='margin-left:6px' onclick=""{1}"" class='aslink' id='lnk_{10}'></nobr>", If(HideFrameCaption, "", If(isCollapsed, CaptionHidden, CaptionVisible)), onClientOnEditClick, If(isCollapsed, ImagePlusURL, ImageMinusURL), JS_SafeString(CaptionEdit), If(isCollapsed, sHid, sVis), If(isCollapsed, sVis, sHid), sCookieID + _OPTION_STATUS_SUFFIX, If(isCollapsed, 1, 0), ImageEditURL, If(isCollapsed, onClientOnExpand, onClientOnCollpase), FrameID + CStr(If(isCollapsed, _HIDDEN, _VISIBLE)), If(HideFrameCaption, " style='display:none'", "")) ' D1211 + D1372 + D4713
        End Function

        ' D1211 ===
        ' D1591 ===
        Public Function Script_Init(sRootPath As String, AllowSaveSize As Boolean, SaveSizeMessage As String) As String  ' D1593 + D1964
            Dim sSaveSizeCode As String = ""    ' D1964
            'If AllowSaveSize Then sSaveSizeCode = "setTimeout('SaveSizeFramedInfodoc(""' + c + '"", ""' + id + ((d) ? ',' + d : '') + '=' + cw + ':' + ch + '"");', 300);" ' D1964
            'If AllowSaveSize Then sSaveSizeCode = "setTimeout('SaveSizeFramedInfodoc(""' + c + '"", ""' + id + ((d) ? ',' + d : '') + '=' + cw + ':' + ch + '"");', 300);" ' D1964
            If AllowSaveSize Then sSaveSizeCode = "setTimeout('SaveSizeFramedInfodoc(""' + c + '"", ""' + cw + ':' + ch + '"");', 350);" ' D1964
            Return "<script  language='JavaScript' type='text/javascript'>" + vbCrLf +
                   "$(document).ready(function () {" + vbCrLf +
                   "    $('div.resizable').resizable({" + vbCrLf +
                   "        maxHeight: " + MaxHeight.ToString + "," + vbCrLf +
                   "        maxWidth: " + MaxWidth.ToString + "," + vbCrLf +
                   "        minHeight: " + MinHeight.ToString + "," + vbCrLf +
                   "        minWidth: " + MinWidth.ToString + "," + vbCrLf +
                   "        helper: 'ui-resizable-helper'," + vbCrLf +
                   "        autoHide: true," + vbCrLf +
                   "        animate: false," + vbCrLf +
                   "        containment: 'document'," + vbCrLf +
                   "        ghost: false," + vbCrLf +
                   "        start: function (event, ui) { SetFrameContentVis(ui, 0); }, " + vbCrLf +
                   "        stop: function (event, ui) { SetFrameContentVis(ui, 1); SetSizeByUI(ui); }" + vbCrLf +
                   "    });" + vbCrLf +
                   "  });" + vbCrLf + vbCrLf +
                   "  function SetFrameContentVis(ui, vis) {" + vbCrLf +
                   "    if ((ui.element) && (ui.element.length==1) && (ui.element[0])) {;" + vbCrLf +
                   "        ui.element[0].style.background = (vis ? '': '#f0f0f0');" + vbCrLf +
                   "        var f = ui.element[0].firstChild;" + vbCrLf +
                   "        if ((f)) f.style.display = (vis ? 'block' : 'none');" + vbCrLf +
                   "        if ((f) && vis) { f.style.width = ui.element.width() + 'px'; f.style.height = ui.element.height() + 'px'; }" + vbCrLf +
                   "    }" + vbCrLf +
                   "  }" + vbCrLf + vbCrLf +
                   "  function SetFrameSize(id, w, h) {" + vbCrLf +
                   "    if (id=='') return false;" + vbCrLf +
                   "    var n = id.split(',');" + vbCrLf +
                   "    for (var i=0; i<n.length; i++) {" + vbCrLf +
                   "        var o = $('#' + n[i]);" + vbCrLf +
                   "        if ((o) && n[i]!='') {" + vbCrLf +
                   "            o.css('width', w).css('height', h);" + vbCrLf +
                   "            var f = (o.length==1 ? o[0].firstChild : null);" + vbCrLf +
                   "            if ((f)) { f.style.width = w + 'px'; f.style.height = h + 'px'; }" + vbCrLf +
                   "        }" + vbCrLf +
                   "    }" + vbCrLf +
                   "  }" + vbCrLf + vbCrLf +
                   "  function SetFrameSizeByFrame(id) {" + vbCrLf +
                   "    var o = $('#' + id);" + vbCrLf +
                   "    if ((o)) SetFrameSize(o.attr('also'), o.outerWidth(), o.outerHeight());" + vbCrLf +
                   "  }" + vbCrLf + vbCrLf +
                   "  function SetSizeByUI(ui) {" + vbCrLf +
                   "    var d = ui.element.attr('also');" + vbCrLf +
                   "    var id = ui.element.attr('id');" + vbCrLf +
                   "    var c = ui.element.attr('cookie');" + vbCrLf +
                   "    if (c!='') {" + vbCrLf +
                   "      cw = ui.element.width();" + vbCrLf +
                   "      ch = ui.element.height();" + vbCrLf +
                   "      document.cookie = c + '" + _OPTION_WIDTH_SUFFIX + "=' + cw + ';';" + vbCrLf +
                   "      document.cookie = c + '" + _OPTION_HEIGHT_SUFFIX + "=' + ch + ';';" + vbCrLf +
                   sSaveSizeCode +
                   "    }" + vbCrLf +
                   "    if ((id != '')) {" + vbCrLf +
                   "      setTimeout(""SetFrameSizeByFrame('"" + id + ""');"", 70);" + vbCrLf +
                   "    }" + vbCrLf +
                   "    else {" + vbCrLf +
                   "      if ((d)) SetFrameSize(d, ui.size.width - 8, ui.size.height - 8);" + vbCrLf +
                   "    }" + vbCrLf +
                   "  }" + vbCrLf + vbCrLf +
                   "    function SwitchRFrame(div_hide, div_show, cookie_name, cookie_value) {" + vbCrLf +
                   "       for (var i=0; i<div_hide.length; i++) { var h = $('#'+div_hide[i]);  if ((h)) h.hide(); }" + vbCrLf +
                   "       for (var j=0; j<div_show.length; j++) { var s = $('#'+div_show[j]);  if ((s)) s.show(); }" + vbCrLf +
                   "       if (cookie_name!='') document.cookie = cookie_name +'=' + cookie_value +';" + CStr(IIf(_KeepFramedInfodocsForAMonth, " expires = " + DateTime.UtcNow.AddMonths(1).ToString("U", CultureInfo.InvariantCulture) + ";", "")) + " path=/';" + vbCrLf +
                   "       setTimeout('$(window).resize();', 150);" + vbCrLf +
                   "       return false;" + vbCrLf +
                   "    }" + vbCrLf + vbCrLf +
                   "    function SwitchRFrameSimple(id, show) {" + vbCrLf +
                   "       var v = (show ? '" + _VISIBLE + "' : '" + _HIDDEN + "'); var h = (show ? '" + _HIDDEN + "' : '" + _VISIBLE + "');" + vbCrLf +
                   "       SwitchRFrame([id + h], [id + v], id+'" + _OPTION_STATUS_SUFFIX + "', show);" + vbCrLf +
                   "    }" + vbCrLf + vbCrLf +
                   "    function SwitchRFrameNoCookie(id, show) {" + vbCrLf +
                   "       var v = (show ? '" + _VISIBLE + "' : '" + _HIDDEN + "'); var h = (show ? '" + _HIDDEN + "' : '" + _VISIBLE + "');" + vbCrLf +
                   "       SwitchRFrame([id + h], [id + v], '', 0);" + vbCrLf +
                   "    }" + vbCrLf + vbCrLf +
                   "    function SaveSizeFramedInfodoc(c, d) {" + vbCrLf +
                   "       if (confirm('" + JS_SafeString(SaveSizeMessage) + "')) { document.cookie = c + '" + _OPTION_SAVE_SIZE_SUFFIX + "=' + d + ';'; if (typeof(isChanged)=='function') isChanged(); }" + vbCrLf +
                   "    }" + vbCrLf +
                   "</script>" 'D1964
        End Function
        ' D1591 ==

        Protected Sub Page_PreRender(sender As Object, e As EventArgs) Handles Me.PreRender  ' D1375
            ' D1212 ===
            If (Options_StoreSize) Then
                Width = CookieWidth(CookieID)
                Height = CookieHeight(CookieID)
            Else
                Width = _Width
                Height = _Height
            End If
            ' D1212 ==
        End Sub
        ' D1211 ==

        ' D1864 ===
        Public Shared Function String2List(sString As String) As NameValueCollection
            Dim sList As New NameValueCollection
            If Not String.IsNullOrEmpty(sString) Then
                Dim sTmp As String() = sString.Split(CType(";", Char()))
                For Each sPair As String In sTmp
                    Dim idx = sPair.IndexOf("=")
                    If idx > 0 Then
                        sList.Add(sPair.Substring(0, idx), sPair.Substring(idx + 1))
                    End If
                Next
            End If
            Return sList
        End Function

        Public Shared Function List2String(tList As NameValueCollection) As String
            Dim sRes As String = ""
            If tList IsNot Nothing Then
                Dim W, H As Integer
                For Each sName As String In tList.AllKeys
                    If ParseSizes(tList(sName), W, H) AndAlso W > 30 AndAlso H > 30 AndAlso W < 1200 AndAlso H < 800 Then sRes += If(sRes = "", "", ";") + String.Format("{0}={1}", sName.ToLower, tList(sName))
                Next
            End If
            Return sres
        End Function

        Public Shared Function ParseSizes(sSizes As String, ByRef tWidth As Integer, ByRef tHeight As Integer) As Boolean
            If Not String.IsNullOrEmpty(sSizes) Then
                Dim idx = sSizes.IndexOf(":")
                Return (idx > 0 AndAlso Integer.TryParse(sSizes.Substring(0, idx), tWidth) AndAlso Integer.TryParse(sSizes.Substring(idx + 1), tHeight))
            End If
            Return False
        End Function

        ' D1864 ==

    End Class

End Namespace