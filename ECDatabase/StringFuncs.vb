Imports System.Text.RegularExpressions
Imports System.Text
Imports System.Globalization
Imports Org.BouncyCastle.Crypto.Digests

Namespace ExpertChoice.Service

#Const _USE_DOTNET_MD5 = False  ' D4822

    Public Module StringFuncs

        Public Const COST_FORMAT As String = "N2" 'A0907 + always use with UserCulture as second parameter
        Public Const _DEF_SAFE_TAGS As String = ";B;BR;BR/;SPAN;DIV;DIV/;P;P/;I;EM;U;A;FONT;BLOCKQUOTE;CENTER;DL;DT;UL;LI;OL;HR;NOBR;PRE;SMALL;SUB;SUP;STRONG;" ' D4650

        Public _OPT_MaxDecimalPlaces As Integer = 14
        Public _OPT_JSMaxDecimalPlaces As Integer = 10   ' D6504
        Public _OPT_EPS As Double = Math.Pow(10, -_OPT_MaxDecimalPlaces - 1)

        Public _OPT_PARSE_YOUTUBE_LINKS As Boolean = True       ' D6714
        Public _OPT_PARSE_SCREENCAST_EMBED As Boolean = True    ' D6714

        Public UserLocale As CultureInfo = New CultureInfo("en-US", True) 'CultureInfo.CurrentCulture   ' D3198 + A1376 + D4507

        ' D3199 ===
        Public Function CostString(tCost As Double, Optional DecimalDigits As Integer = 2, Optional CurrencySymbol As Boolean = False) As String 'A1226 + A1265
            Dim sRes As String = "" ' D4016
            If tCost >= (Integer.MinValue >> 1) Then   ' D4106
                sRes = tCost.ToString(COST_FORMAT, UserLocale)
                If sRes.EndsWith(UserLocale.NumberFormat.CurrencyDecimalSeparator + "00") Then sRes = sRes.Substring(0, sRes.Length - 3)
                'A1226 ===
                If sRes.Contains(UserLocale.NumberFormat.CurrencyDecimalSeparator) Then
                    Dim comma_index As Integer = sRes.IndexOf(UserLocale.NumberFormat.CurrencyDecimalSeparator)
                    If DecimalDigits <= 0 Then
                        sRes = sRes.Substring(0, comma_index)
                    End If
                    If DecimalDigits = 1 Then
                        sRes = sRes.Substring(0, comma_index + 2)
                    End If
                End If
                'A1226 ==
            End If
            If CurrencySymbol Then
                ' D4181 ===
                Dim sTpl As String = "{0}{1}"
                Select Case UserLocale.NumberFormat.CurrencyPositivePattern
                    Case 1
                        sTpl = "{1}{0}"
                    Case 2
                        sTpl = "{0} {1}"
                    Case 3
                        sTpl = "{1} {0}"
                End Select
                sRes = String.Format(sTpl, CStr(IIf(sRes = "", "", UserLocale.NumberFormat.CurrencySymbol)), sRes) 'A1376
                'sRes = String.Format(sTpl, CStr(IIf(sRes = "", "", UserLocale.NumberFormat.CurrencySymbol)), sRes) 'A1376
                ' D4181 ==
            End If
            Return sRes
        End Function
        ' D3199 ==

        Public Function DeltaString(Total As Double, Value As Double, DecimalDigits As Integer, Optional isCost As Boolean = False, Optional DeltaSymbol As String = "&#8710;:") As String
            Dim tDeltaValue As Double = (Total - Value) * CDbl(IIf(isCost, 1, 100))
            Dim sDeltaValue As String = ""
            If isCost Then
                sDeltaValue = CostString(tDeltaValue, DecimalDigits, True)
            Else
                sDeltaValue = tDeltaValue.ToString("F2") + "%"
            End If
            Return String.Format(" ({0} {1})", DeltaSymbol, sDeltaValue)
            'Return String.Format(" (unfunded: {0})", sDeltaValue)
        End Function

        ' D0990 ===
        Public Function SubString(ByVal sStr As String, ByVal MaxLen As Integer) As String
            If sStr IsNot Nothing AndAlso sStr.Length > MaxLen Then sStr = sStr.Substring(0, MaxLen)
            Return sStr
        End Function
        ' D0990 ==

        ''' <summary>
        ''' Truncate short string for max specified chars length (Enhanced version)
        ''' </summary>
        ''' <param name="sStr">String for truncate, Should be not NULL.</param>
        ''' <param name="MaxLen">The maximum characters in resulted string</param>
        ''' <param name="asPlainText">Use "…" char insted html-styled "&#133;" after truncated string</param>
        ''' <returns>Truncated string with hellip when string is over specified max chars length</returns>
        ''' <remarks>This version will try to truncate nearest word space with trunc punctuation and spaces after 85% of specified length</remarks>
        Public Function ShortString(ByVal sStr As String, ByVal MaxLen As Integer, Optional ByVal asPlainText As Boolean = False, Optional CheckSpacesForBreak As Integer = 100) As String   ' D3908
            If sStr IsNot Nothing Then sStr = sStr.Trim ' D3255 + D3258
            If sStr IsNot Nothing AndAlso sStr.Length > MaxLen Then
                ' D0179 ===
                Dim CutIdx As Integer = MaxLen - 1
                Dim sp As Integer = CutIdx
                Dim fCanSlice As Boolean = False
                While sp > 0.85 * MaxLen
                    Select Case CStr(sStr(sp - 1))
                        Case " ", ".", ",", ";", ":", "/", "\"
                            fCanSlice = True
                        Case Else
                            If fCanSlice Then Exit While
                    End Select
                    sp -= 1
                End While
                If fCanSlice Then CutIdx = sp
                sStr = sStr.Substring(0, CutIdx).Trim + CStr(IIf(asPlainText, "…", "&#133;")) ' D0160
                ' D0179 ==
            End If
            ' D3908 ===
            If Not String.IsNullOrEmpty(sStr) AndAlso CheckSpacesForBreak > 1 AndAlso CheckSpacesForBreak < sStr.Length Then
                Dim idx As Integer = 0
                Dim idx_prev As Integer = 0
                While idx < sStr.Length
                    idx = sStr.IndexOf(" ", idx + 1)
                    If idx < 0 Then idx = sStr.Length
                    If idx > idx_prev Then
                        While (idx - idx_prev) > CheckSpacesForBreak
                            sStr = sStr.Insert(idx_prev + CheckSpacesForBreak, " ")
                            idx += 1
                            idx_prev += CheckSpacesForBreak + 1
                        End While
                    End If
                    idx_prev = idx
                End While
            End If
            ' D3908 ==
            Return sStr
        End Function

        ' D0360 ===
        Public Function TrimEndZeroes(ByVal sValue As String) As String
            While sValue > "" AndAlso (sValue.Contains(".") Or sValue.Contains(",")) AndAlso sValue.EndsWith("0")
                sValue = sValue.Substring(0, sValue.Length - 1)
            End While
            If sValue.EndsWith(".") Or sValue.EndsWith(",") Then sValue = sValue.Substring(0, sValue.Length - 1)
            Return sValue
        End Function
        ' D0360 ==

        ' D0041 ===
        ''' <summary>
        ''' Convert date value to ULong
        ''' </summary>
        ''' <param name="DateValue"></param>
        ''' <returns></returns>
        ''' <remarks>Internal function .ToBinary() getting wrong value, as usual.</remarks>
        Public Function Date2ULong(ByVal DateValue As Date) As ULong
            Dim sDT As String = DateValue.ToString
            Return CULng(CDate(sDT).ToBinary)
        End Function
        ' D0041 ==

        ' D3510 ===
        Public Function Date2ULong(DateTime As Nullable(Of DateTime)) As ULong
            If DateTime.HasValue Then Return Date2ULong(DateTime.Value) Else Return ULong.MinValue
        End Function
        ' D3510 ==

        ' D0137 ===
        ''' <summary>
        ''' Simple function for get string for Nullable date value
        ''' </summary>
        ''' <param name="DT"></param>
        ''' <param name="sEmptyValue"></param>
        ''' <returns></returns>
        ''' <remarks>Return sEmptyValue when data is not defined</remarks>
        Public Function Date2String(ByVal DT As Nullable(Of DateTime), Optional ByVal sEmptyValue As String = "") As String ' D0138
            If DT.HasValue Then Return DT.Value.ToShortDateString Else Return sEmptyValue
        End Function
        ' D0137 ==

        ' D3576 ===
        Public Function DateTime2String(ByVal DT As Nullable(Of DateTime), fShowSeconds As Boolean, Optional ByVal sEmptyValue As String = "") As String
            If DT.HasValue Then Return DT.Value.ToShortDateString + " " + DT.Value.ToString(String.Format("HH:mm{0}", IIf(fShowSeconds, ":ss", ""))) Else Return sEmptyValue
        End Function
        ' D3576 ==

        ' D3271 ===
        Public Function SizeString(tSize As Long) As String
            Dim sSize As String = ""
            If tSize < 1000 Then
                sSize = String.Format("{0}b", tSize)
            Else
                If tSize < 1024000 Then
                    sSize = String.Format("{0}Kb", (tSize / 1024).ToString("F1"))
                Else
                    sSize = String.Format("{0}Mb", (tSize / 1048576).ToString("F1"))
                End If
            End If
            Return sSize
        End Function
        ' D3271 ==

        ' D0193 ===
        Public Function BinaryStr2DateTime(ByVal sBinaryStr As String) As Nullable(Of DateTime)
            Dim Res As Nullable(Of DateTime) = Nothing
            If String.IsNullOrEmpty(sBinaryStr) OrElse sBinaryStr = "0" Then Return Res ' D3511
            Dim tst As Long
            If Long.TryParse(sBinaryStr, tst) Then
                Res = DateTime.FromBinary(CLng(sBinaryStr))
            End If
            Return Res
        End Function
        ' D0193 ==

        'A1507 ===
        Public Function Str2Bool(ByVal sValue As String) As Boolean
            If sValue Is Nothing Then Return False
            sValue = sValue.ToLower
            Return sValue = "1" OrElse sValue = "true" OrElse sValue = "yes"
        End Function
        'A1507 ==

        ' D0214 ===
        Public Function Str2Bool(ByVal sValue As String, ByRef fValue As Boolean) As Boolean
            If sValue = "1" Or sValue.ToLower = "true" Or sValue.ToLower = "yes" Then
                fValue = True
                Return True
            End If
            If sValue = "0" Or sValue.ToLower = "false" Or sValue.ToLower = "no" Then
                fValue = False
                Return True
            End If
            Return False
        End Function

        ' D0144 + D3198 ===
        Public Function String2Double(ByVal sValue As String, ByRef tValue As Double) As Boolean  ' D0147 + D1858
            sValue = sValue.Trim
            If sValue.StartsWith(".") Then sValue = "0" + sValue
            If sValue.StartsWith("-.") Then sValue = "-0" + sValue.Substring(1)
            Dim fRes As Boolean = False
            If UserLocale.DisplayName <> CultureInfo.CurrentCulture.DisplayName Then fRes = String2DoubleAsCulture(sValue, tValue, UserLocale)
            If Not fRes Then
                fRes = String2DoubleAsCulture(sValue, tValue, CultureInfo.CurrentCulture)
                If Not fRes Then
                    fRes = String2DoubleAsCulture(sValue, tValue, CultureInfo.InvariantCulture, "$")
                    If Not fRes Then fRes = String2DoubleAsCulture(sValue, tValue, CultureInfo.CreateSpecificCulture("en-US"))
                End If
            End If
            Return fRes
        End Function
        ' D0144 ==

        Public Function String2DoubleAsCulture(ByVal sValue As String, ByRef tValue As Double, tCulture As CultureInfo, Optional sCustCurrency As String = Nothing) As Boolean
            Dim sep As Char = tCulture.NumberFormat.CurrencyDecimalSeparator(0)
            Dim grp As Char = tCulture.NumberFormat.CurrencyGroupSeparator(0)
            Dim perc As String = tCulture.NumberFormat.PercentSymbol
            If String.IsNullOrEmpty(sCustCurrency) Then sCustCurrency = tCulture.NumberFormat.CurrencySymbol
            Dim sVal As String = sValue.Replace(sCustCurrency, "").Replace(grp, "").Replace(perc, "").Replace(".", sep)
            Return Double.TryParse(sValue, Globalization.NumberStyles.Any, tCulture, tValue)
        End Function
        ' D3198 ==

        ' D4283 ===
        Public Function Str2GUID(sVal As String) As Guid
            Dim tRes As Guid
            If sVal <> "" Then
                Try
                    tRes = New Guid(sVal)
                Catch ex As Exception
                End Try
            End If
            Return tRes
        End Function
        ' D4283 ==

        ' D0079 ===
        ''' <summary>
        ''' Get string with random symbols
        ''' </summary>
        ''' <param name="mLength"></param>
        ''' <param name="fOnlyAlphaNumeric"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetRandomString(ByVal mLength As Integer, ByVal fOnlyAlphaNumeric As Boolean, ByVal fUseUpperCase As Boolean) As String ' D0104 + D0286
            Dim sPsw As String = ""
            Dim randomNumGen As System.Security.Cryptography.RandomNumberGenerator = System.Security.Cryptography.RNGCryptoServiceProvider.Create()
            Dim randomBytes(mLength) As Byte
            randomNumGen.GetBytes(randomBytes)
            ' D0104 ===
            If fOnlyAlphaNumeric Then
                For i As Integer = 0 To randomBytes.Length - 1
                    Dim c As Integer = randomBytes(i) Mod 36
                    Dim AC As Integer = CInt(IIf(fUseUpperCase And CBool(c And i And 1), Asc("A"), Asc("a"))) ' D286
                    If c < 10 Then sPsw += Chr(c + Asc("0")) Else sPsw += Chr(c - 10 + AC) ' D0286
                Next
            Else
                sPsw = System.Convert.ToBase64String(randomBytes, Base64FormattingOptions.None)
            End If
            ' D0104 ==
            sPsw = sPsw.Substring(0, mLength).Trim  ' D1712
            Return sPsw
        End Function

        ' D1770 ===
        Public Function Capitalize(sString As String) As String
            If Not String.IsNullOrEmpty(sString) Then ' D2109
                ' D4148 ===
                Dim idx As Integer = 0
                While idx < sString.Length AndAlso sString(idx) = "%"
                    idx += 1
                End While
                If idx < sString.Length Then
                    Dim sUpper As String = sString(idx).ToString.ToUpper
                    sString = sString.Remove(idx, 1).Insert(idx, sUpper)
                End If
            End If
            Return sString
            ' D4148 ==
        End Function
        ' D1770 ==

        ''' <summary>
        ''' Parse string with template(s) "%%...%%"
        ''' </summary>
        ''' <param name="sString">String with templates %%</param>
        ''' <param name="sParams">List of templates (Key, Value)</param>
        ''' <returns>Parsed strings. All params, enclosed in "%%...%%", which not found in sParams were leave as is. </returns>
        ''' <remarks>Use code like this: Dim tParams As New Generic.Dictionary(Of String, String) and tParams.Add(sKey, sValue) for create list of params. </remarks>
        Public Function ParseStringTemplates(ByVal sString As String, ByVal sParams As Generic.Dictionary(Of String, String)) As String
            If sParams IsNot Nothing Then   ' D2094
                For Each tPair As Generic.KeyValuePair(Of String, String) In sParams
                    sString = sString.Replace(tPair.Key, tPair.Value)   ' D0220
                    sString = sString.Replace("%%" + Capitalize(tPair.Key.Trim(CChar("%"))) + "%%", Capitalize(tPair.Value))   ' D1770
                Next
            End If
            Return sString
        End Function
        ' D0120 ==

        ' D6648 ===
        Public Function GetEnumIDsAsString(tEnum As Type) As String
            Dim sList As String = ""
            For Each e As Integer In [Enum].GetValues(tEnum)
                sList += String.Format("{0}{1}", If(sList = "", "", ","), e)
            Next
            Return sList
        End Function
        ' D6648 ==

        ''' <summary>
        ''' Function for check e-mail on validity.
        ''' </summary>
        ''' <param name="sEmail">String with email address.</param>
        ''' <returns>True, when string is correct e-mail address.</returns>
        ''' <remarks>Used simple RegExp "^([^@]+)@([^@]+)\.([^@]+)$". Empty string is invalid address.</remarks>
        Public Function isValidEmail(ByVal sEmail As String) As Boolean
            Dim emailRegex As New System.Text.RegularExpressions.Regex("^([^@]+)@([^@]+)\.([^@]+)$")
            Dim emailMatch As System.Text.RegularExpressions.Match = emailRegex.Match(sEmail)
            Return (sEmail <> "" AndAlso sEmail.Trim = sEmail AndAlso emailMatch.Success)
        End Function

        ' D2279 ===
        Public Function GetUserFirstName(sName As String) As String
            Dim sFirst As String = ""
            If Not String.IsNullOrEmpty(sName) Then
                sFirst = sName.Trim
                Dim Idx As Integer = sFirst.IndexOf(" ")
                If Idx > 0 Then sFirst = sFirst.Substring(0, Idx).Trim
            End If
            Return sFirst
        End Function
        ' D2279 ==

        ' D5039 ===
        Function RemoveBadTags(sText As String) As String
            If Not String.IsNullOrEmpty(sText) Then
                Dim BTags As String() = {"applet", "embed", "frameset", "iframe", "head", "noframes", "noscript", "object", "script", "style", "title"}
                For Each sTag As String In BTags
                    Dim S As Integer = sText.IndexOf("<" + sTag, StringComparison.OrdinalIgnoreCase)
                    If S >= 0 Then
                        Dim E1 As Integer = sText.IndexOf(">", S + 1)
                        Dim E2 As Integer = sText.IndexOf("/" + sTag, StringComparison.OrdinalIgnoreCase)
                        If E2 > 0 Then E2 += sTag.Length + 1
                        If E1 > S OrElse E2 > S Then sText = sText.Remove(S, If(E1 > E2, E1, E2) - S + 1)
                    End If
                    If sText = "" Then Exit For
                Next
            End If
            Return sText
        End Function
        ' D5039 ==

        ' D2094 ===
        ' Based on an old code from http://www.codeproject.com/Articles/639/Removing-HTML-from-the-text-in-ASP
        Function HTML2TextWithSafeTags(strText As String, Optional AllowedTags As String = _DEF_SAFE_TAGS) As String  ' D2116

            'Dim TAGLIST As String = ";!--;!DOCTYPE;A;ACRONYM;ADDRESS;APPLET;AREA;B;BASE;BASEFONT;" & _
            '      "BGSOUND;BIG;BLOCKQUOTE;BODY;BR;BUTTON;CAPTION;CENTER;CITE;CODE;" & _
            '      "COL;COLGROUP;COMMENT;DD;DEL;DFN;DIR;DIV;DL;DT;EM;EMBED;FIELDSET;" & _
            '      "FONT;FORM;FRAME;FRAMESET;HEAD;H1;H2;H3;H4;H5;H6;HR;HTML;I;IFRAME;IMG;" & _
            '      "INPUT;INS;ISINDEX;KBD;LABEL;LAYER;LAGEND;LI;LINK;LISTING;MAP;MARQUEE;" & _
            '      "MENU;META;NOBR;NOFRAMES;NOSCRIPT;OBJECT;OL;OPTION;P;PARAM;PLAINTEXT;" & _
            '      "PRE;Q;S;SAMP;SCRIPT;SELECT;SMALL;SPAN;STRIKE;STRONG;STYLE;SUB;SUP;" & _
            '      "TABLE;TBODY;TD;TEXTAREA;TFOOT;TH;THEAD;TITLE;TR;TT;U;UL;VAR;WBR;XMP;"

            Const BLOCKTAGLIST As String = ";APPLET;EMBED;FRAMESET;IFRAME;HEAD;NOFRAMES;NOSCRIPT;OBJECT;SCRIPT;STYLE;TITLE;"

            Dim nPos1, nPos2, nPos3 As Integer
            Dim strResult As String = ""
            Dim strTagName As String
            Dim bRemove, bSearchForBlock As Boolean

            Do While strText <> "" AndAlso strText.Contains("<!--")
                nPos1 = InStr(strText, "<!--")
                nPos2 = InStr(strText, "-->")
                If nPos2 > 1 AndAlso nPos2 > nPos1 Then
                    strText = strText.Remove(nPos1 - 1, nPos2 - nPos1 + 3)
                Else
                    Exit Do
                End If
            Loop

            nPos1 = InStr(strText, "<")
            Do While nPos1 > 0
                nPos2 = InStr(nPos1 + 1, strText, ">")
                If nPos2 > 0 Then
                    strTagName = Mid(strText, nPos1 + 1, nPos2 - nPos1 - 1)
                    strTagName = Replace(Replace(strTagName, vbCr, " "), vbLf, " ")

                    nPos3 = InStr(strTagName, " ")
                    If nPos3 > 0 Then
                        strTagName = Left(strTagName, nPos3 - 1)
                    End If

                    If Left(strTagName, 1) = "/" Then
                        strTagName = Mid(strTagName, 2)
                        bSearchForBlock = False
                    Else
                        bSearchForBlock = True
                    End If

                    'If InStr(1, TAGLIST, ";" & strTagName & ";", vbTextCompare) > 0 Then
                    If InStr(1, AllowedTags, ";" & strTagName & ";", vbTextCompare) = 0 Then
                        bRemove = True
                        If bSearchForBlock Then
                            If InStr(1, BLOCKTAGLIST, ";" & strTagName & ";", vbTextCompare) > 0 Then
                                nPos2 = Len(strText)
                                nPos3 = InStr(nPos1 + 1, strText, "</" & strTagName, vbTextCompare)
                                If nPos3 > 0 Then
                                    nPos3 = InStr(nPos3 + 1, strText, ">")
                                End If

                                If nPos3 > 0 Then
                                    nPos2 = nPos3
                                End If
                            End If
                        End If
                    Else
                        bRemove = False
                    End If

                    If bRemove Then
                        strResult = strResult & Left(strText, nPos1 - 1)
                        strText = Mid(strText, nPos2 + 1)
                    Else
                        strResult = strResult & Left(strText, nPos1)
                        strText = Mid(strText, nPos1 + 1)
                    End If
                Else
                    strResult = strResult & strText
                    strText = ""
                End If

                nPos1 = InStr(strText, "<")
            Loop
            strResult = strResult & strText

            Return strResult
        End Function
        ' D2094 ==

        ' This function converts HTML code to plain text
        Public Function HTML2Text(ByVal HTMLCode As String) As String
            If Not String.IsNullOrEmpty(HTMLCode) Then ' D4346
                HTMLCode = HTMLCode.Trim    ' D3986
                If HTMLCode = "" Then Return "" ' D3986

                ' Remove new lines since they are not visible in HTML
                HTMLCode = HTMLCode.Replace(vbNewLine, " ").Replace(vbCr, " ").Replace(vbLf, " ")    ' D4371

                ' Remove tab spaces
                HTMLCode = HTMLCode.Replace(vbTab, " ")  ' D4371

                ' Remove multiple white spaces from HTML
                HTMLCode = Regex.Replace(HTMLCode, "\\s+", "  ").Trim   ' D4371

                ' Remove HEAD tag
                HTMLCode = Regex.Replace(HTMLCode, "<head.*?</head>", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)

                ' Remove Title tag
                HTMLCode = Regex.Replace(HTMLCode, "<title.*?</title>", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)     'D4371

                ' Remove any JavaScript
                HTMLCode = Regex.Replace(HTMLCode, "<script.*?</script>", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)

                ' Check if there are line breaks (<br>) or paragraph (<p>)
                ' D4371 ===
                Dim Breaks() As String = {"<br>", "<br/>", "<br ", "<p>", "<p ", "<hr>", "<hr/>", "<hr ", "<div>", "<div ", "<table", "<tr>", "<tr ", "<h1", "<h2", "<h3", "<h4", "<h5", "<h6", "<li>", "<li "}
                For Each sBr As String In Breaks
                    HTMLCode = HTMLCode.Replace(sBr.ToLower, vbNewLine + sBr.ToLower).Replace(sBr.ToUpper, vbNewLine + sBr.ToUpper)
                Next
                ' D4371 ==

                ' Remove all HTML tags and return plain text
                HTMLCode = System.Text.RegularExpressions.Regex.Replace(HTMLCode, "<[^>]*>", "").Trim

                HTMLCode = HTMLCode.Replace(" " + vbNewLine, vbNewLine).Trim(CChar(vbNewLine))  ' D4371

                ' D2776 ===
                ' Replace special characters like &, <, >, " etc.
                Dim sbHTML As StringBuilder = New StringBuilder(HTMLCode)

                ' Note: There are many more special characters, these are just most common.
                Dim OldWords() As String = {"&nbsp;", "&amp;", "&quot;", "&lt;", "&gt;", "&reg;", "&copy;", "&bull;", "&#8482;"}
                Dim NewWords() As String = {" ", "&", """", "<", ">", "®", "©", "•", "™"}
                For i As Integer = 0 To OldWords.Length - 1
                    sbHTML.Replace(OldWords(i), NewWords(i))
                Next i

                '' Check if there are line breaks (<br>) or paragraph (<p>)
                'sbHTML.Replace("<br>", "\n<br>")
                'sbHTML.Replace("<br ", "\n<br ")
                'sbHTML.Replace("<p ", "\n<p ")

                HTMLCode = sbHTML.ToString
            End If

            Return HTMLCode
            ' D2776 ==
        End Function


        ' D6727 ===
        Public Function CutHTMLHeaders(sHTML As String) As String
            If Not String.IsNullOrEmpty(sHTML) Then
                Dim idx As Integer = sHTML.IndexOf("<body", 0, StringComparison.InvariantCultureIgnoreCase)
                If idx >= 0 Then
                    idx = sHTML.IndexOf(">", idx)
                    If (idx > 0) Then
                        sHTML = sHTML.Substring(idx + 1)
                        idx = sHTML.IndexOf("</body>", 0, StringComparison.InvariantCultureIgnoreCase)
                        If (idx > 0) Then sHTML = sHTML.Substring(0, idx)
                    End If
                End If
            End If
            Return sHTML
        End Function
        ' D6727 ==

        ' D3956 ===
        Public Function isHTMLEmpty(sText As String) As Boolean
            If String.IsNullOrEmpty(sText) Then Return True Else Return HTML2Text(HTML2TextWithSafeTags(sText.ToLower.Replace("<img ", "image<").ToLower.Replace("<iframe ", "iframe<"), "")).Trim = "" ' D3986 + D6682
        End Function
        ' D3956 ==

        'A1361 ===
        Public Function htmlUnescape(s As String) As String
            Return s.Replace("&amp;", "&").Replace("&quot;", """").Replace("&#39;", "'").Replace("&lt;", "<").Replace("&gt;", ">")
        End Function
        'A1361 ==

        ' D0896 + D2183 ===
        Public Function GetMD5(ByVal strData As Byte()) As String
#If _USE_DOTNET_MD5 Then    ' D4822
            Dim objMD5 As New System.Security.Cryptography.MD5CryptoServiceProvider
            Dim arrHash() As Byte
            arrHash = objMD5.ComputeHash(strData)
             D2183 ==
#Else
            ' D4819 ===
            Dim objMD5 As New MD5Digest()
            objMD5.BlockUpdate(strData, 0, strData.Length)
            Dim arrHash As Byte() = New Byte(objMD5.GetDigestSize() - 1) {}
            objMD5.DoFinal(arrHash, 0)
            ' D4819 ==
#End If
            objMD5 = Nothing

            Dim strOutput As New System.Text.StringBuilder(arrHash.Length)
            For i As Integer = 0 To arrHash.Length - 1
                strOutput.Append(arrHash(i).ToString("X2"))
            Next

            Return strOutput.ToString().ToLower
        End Function
        ' D0896 ==

        ' D2183 ===
        Public Function GetMD5(ByVal strData As String) As String
            If String.IsNullOrEmpty(strData) Then Return "" Else Return GetMD5(System.Text.Encoding.UTF8.GetBytes(strData)) ' D5063
        End Function
        ' D2183 ==

        Public Function Double2String(Value As Double, Optional MinDecimalPlaces As Integer = 2, Optional tPercentSign As Boolean = False, Optional tIndention As Boolean = False, Optional PeriodAsDecimalsSeparator As Boolean = False) As String
            If Double.IsNaN(Value) Then Value = 0

            Dim dp As Integer = MinDecimalPlaces
            If Value <> 0 Then
                While dp <= _OPT_MaxDecimalPlaces AndAlso Math.Abs(Math.Round(Value, dp)) <= _OPT_EPS
                    dp += 1
                End While
            End If

            If dp > _OPT_MaxDecimalPlaces Then dp = MinDecimalPlaces

            Dim sValue As String = If(PeriodAsDecimalsSeparator, Value.ToString("F" + dp.ToString, System.Globalization.CultureInfo.InvariantCulture), Value.ToString("F" + dp.ToString)) + CStr(IIf(tPercentSign, "%", "")) ' D3170
            If tIndention Then
                If sValue.IndexOf(".") = 1 Then sValue = "    " + sValue
                If sValue.IndexOf(".") = 2 Then sValue = "  " + sValue
                If sValue.IndexOf(",") = 1 Then sValue = "    " + sValue
                If sValue.IndexOf(",") = 2 Then sValue = "  " + sValue
            End If
            Return sValue
        End Function

        '' D2756 ===
        'Public Function DetectHyperlinks(sText As String, fTargetBlank As Boolean) As String
        '    If Not String.IsNullOrEmpty(sText) Then
        '        Dim pattern As String = "((https?|ftp|gopher|telnet|file|notes|ms-help):((//)|(\\\\))+[\w\d:#@%/;$()~_?\+-=\\\.&]*)"
        '        Dim R As New Regex(pattern)
        '        Dim tMatch As New MatchEvaluator(Function(m As Match) [String].Format("<a href=""{0}""{1}>{0}</a>", m.ToString(), IIf(fTargetBlank, " target=_blank", "")))
        '        sText = R.Replace(sText, tMatch)
        '    End If
        '    Return sText
        'End Function
        '' D2756 ==

        ''' <summary>
        ''' Replace common html-chars to html-encoded strings
        ''' </summary>
        ''' <param name="sText">String, used as field value in web-form</param>
        ''' <returns>String with replaced chars</returns>
        '''<remarks>Replaced: &amp;, tags &amp;lt; &amp;gt; ', ".  </remarks>
        Public Function SafeFormString(ByVal sText As String) As String
            Dim sTmp As String = sText
            If sTmp IsNot Nothing Then sTmp = sTmp.Replace("&", "&#38;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("'", "&#39;").Replace("""", "&quot;") ' D3731
            If sTmp Is Nothing Then sTmp = "" ' D3731
            Return sTmp
        End Function

        ''' <summary>
        ''' Replace common html-chars to html-encoded strings
        ''' </summary>
        ''' <param name="sText">Object, should be available for convert to string</param>
        ''' <returns>String with replaced chars</returns>
        ''' <remarks>Replaced: &amp;, tags &amp;lt; &amp;gt;. </remarks>
        Public Function JS_SafeHTML(ByVal sText As Object) As String
            If sText Is Nothing OrElse IsDBNull(sText) Then Return ""
            Dim sTmp As String = CStr(sText)    ' D4626
            If Not String.IsNullOrEmpty(sTmp) Then  ' D4626
                sTmp = Replace(sTmp, "&", "&amp;")
                sTmp = Replace(sTmp, "<", "&lt;")
                sTmp = Replace(sTmp, ">", "&gt;")
            Else
                sTmp = ""
            End If
            Return sTmp
        End Function

        ''' <summary>
        ''' Replace common html-chars to javascript-safe strings
        ''' </summary>
        ''' <param name="sText">Object, should be available for convert to string</param>
        ''' <returns>String with replaced chars</returns>
        ''' <remarks>Replaced: \, \n, \r, ', ". </remarks>
        Public Function JS_SafeString(ByVal sText As Object, Optional vbCrLfExtra As Boolean = False) As String 'A1553
            If sText Is Nothing OrElse IsDBNull(sText) Then Return ""
            Dim sTmp As String = CStr(sText)    ' D4626
            If Not String.IsNullOrEmpty(sTmp) Then  ' D4626
                sTmp = Replace(sTmp, "\", "\\")
                sTmp = Replace(sTmp, "'", "\u0027")
                sTmp = Replace(sTmp, """", "\""")
                sTmp = Replace(sTmp, vbCrLf, If(vbCrLfExtra, "\r\n", "\n")) 'A1553
                sTmp = Replace(sTmp, vbCr, "\r")
                sTmp = Replace(sTmp, vbLf, "\n")
                sTmp = Replace(sTmp, vbTab, "\t")   ' D7029
            Else
                sTmp = ""
            End If
            Return sTmp
        End Function

        ''' <summary>
        ''' Function for preparing db-source numbers (integer and float) for transfer JScript code. This function replace "," in float definition to "." symbol. 
        ''' </summary>
        ''' <param name="iNumber">Parameter should be available for convert to string</param>
        ''' <returns>String with javascript-safe number</returns>
        ''' <remarks>Parse Null or empty strings as "0". Float numbers with exponent will be showed as regular rounded number without E.</remarks>
        Public Function JS_SafeNumber(ByVal iNumber As Object, Optional MinDecimalPlaces As Integer = 2) As String
            Dim iTmp As String
            ' D0185 ===
            Dim sNumber As String = CStr(iNumber)
            If TypeOf (iNumber) Is Single OrElse TypeOf (iNumber) Is Double Then
                Dim iNum As Double = CDbl(iNumber)
                If Math.Abs(iNum) > 100000000.0 OrElse sNumber.IndexOf("E", StringComparison.CurrentCultureIgnoreCase) > 0 Then
                    'sNumber = iNum.ToString("F20").TrimEnd(CChar("0"))
                    sNumber = Double2String(iNum, MinDecimalPlaces)   ' D3082
                    ' D6504 ===
                Else
                    Dim dotIdx As Integer = sNumber.IndexOf(".")
                    If dotIdx < 0 Then dotIdx = sNumber.IndexOf(",")
                    If dotIdx > 0 AndAlso (sNumber.Length - dotIdx > _OPT_JSMaxDecimalPlaces) Then sNumber = iNum.ToString("F" + _OPT_JSMaxDecimalPlaces.ToString)
                    ' D6504 ==
                End If
            End If
            iTmp = Replace(sNumber, ",", ".").TrimEnd(CChar("."))
            ' D0185 ==
            If (iTmp = "") Then iTmp = "0"
            Return iTmp
        End Function

        Public Function Bool2JS(bValue As Boolean) As String 'A1041
            Return If(bValue, "true", "false")
        End Function

        ' D3726 ===
        Public Function Bool2YesNo(bValue As Boolean) As String
            Return If(bValue, "yes", "no")
        End Function
        ' D3726 ==

        ' D3646 ===
        Public Function Bool2Num(bValue As Boolean) As Integer
            Return If(bValue, 1, 0)
        End Function
        ' D3646 ==

        ' D0543 ===
        Public Function RemoveHTMLTags(ByVal sString As String) As String
            Return Regex.Replace(sString, "<[^>]*?>", String.Empty, RegexOptions.IgnoreCase)
        End Function
        ' D0543 ==

        ' D0600 ===
        Public Function BlankBaseHyperlinks(ByVal sText As String) As String
            'Dim sText As String = "hello <a href='?test'>test</a> and <a href=""&sadas'href'"">And</a> & here &lt;a href='12'&gt;12&lt;/a&gt; link and the end"
            sText = sText.Replace("target=_blank", "").Replace("target=_top", "").Replace("target=_parent", "").Replace("target=_self", "") ' for remove all wrong targets, which could be inserted at the wrong place; new one will be inserted with space before
            sText = sText.Replace("target='infodoc' rel='noopener'", "")    ' D4669

            Dim sResult As String = sText

            Dim r As New Regex("<a\s+(?<1>[^>]*)href\s*=\s*(?<1>[^>]*)", RegexOptions.IgnoreCase Or RegexOptions.Compiled)

            ' D3676 ===
            Dim Lst As New List(Of Integer)
            Dim m As Match = r.Match(sText)
            While m.Success
                If sText.Substring(m.Groups(0).Index, m.Groups(0).Length).ToLower.IndexOf(" target") < 0 Then Lst.Add(m.Groups(0).Index)
                m = m.NextMatch()
            End While

            For i As Integer = Lst.Count - 1 To 0 Step -1
                'If sResult.Length > Lst(i) + 8 Then sResult = sResult.Insert(Lst(i) + 2, " target=_blank")
                If sResult.Length > Lst(i) + 8 Then sResult = sResult.Insert(Lst(i) + 2, " target='infodoc' rel='noopener'") ' D4669
            Next
            ' D3676 ==

            Return sResult
        End Function
        ' D0600 ==

        ' D3697 ===
        Public Function ParseVideoLinks(sText As String, fAllowAutoPlay As Boolean, Optional Domain As String = "", Optional JS_YoutubeScript As String = "") As String   ' D4557 + D7135 + D7141
            Dim sResult As String = sText

            If _OPT_PARSE_YOUTUBE_LINKS AndAlso (sText.ToLower.Contains("youtube.") OrElse sText.ToLower.Contains("youtu.be")) Then ' D6714

                '' youtube params: http://sergeychunkevich.com/dlya-web-mastera/youtube-parametry/
                'Dim sReplace As String = "<iframe width='640' height='360' src='https://www.youtube.com/embed/{0}?rel=0&modestbranding=1{1}' frameborder='0' allowfullscreen></iframe>"  ' D3699
                Dim sReplace As String = "<div style='position:relative; padding-bottom:56.25%; height:0; overflow:hidden;'><iframe src='//www.youtube.com/embed/{0}{2}' {3} allow='{1}accelerometer; encrypted-media; gyroscope; picture-in-picture' allowfullscreen='allowfullscreen' mozallowfullscreen='mozallowfullscreen' msallowfullscreen='msallowfullscreen' oallowfullscreen='oallowfullscreen' webkitallowfullscreen='webkitallowfullscreen' frameborder='0' style='border:0px; position:absolute; top:0; left:0; width:100%; height:100%;'></iframe></div>"    ' D6672 + D7134

                Dim sSearch As String = "(?:https?://)?(?:www\.)?(?:youtu\.be/|youtube\.com(?:/embed/|/v/|/watch\?v=))([\w\-]{8,25})(?:[&\w-=%]*)?"

                ''[^"\'](?:https?://)?  # Optional scheme. Either http or https; We want the http thing NOT to be prefixed by a quote -> not embeded yet.
                ''(?:www\.)?        # Optional www subdomain
                ''(?:               # Group host alternatives
                ''  youtu\.be/      # Either youtu.be,
                ''| youtube\.com    # or youtube.com
                ''  (?:             # Group path alternatives
                ''    /embed/       # Either /embed/
                ''  | /v/           # or /v/
                ''  | /watch\?v=    # or /watch\?v=
                ''  )               # End path alternatives.
                '')                 # End host alternatives.
                ''([\w\-]{8,25})    # $1 Allow 8-25 for YouTube id (just in case).
                ''(?:               # Group unwanted &feature extension
                ''    [&\w-=%]*     # Either &feature=related or any other key/value pairs
                '')

                Dim offset As Integer = 0
                Dim isFirstVideo As Boolean = True  ' D7141

                Dim r As New Regex(sSearch, RegexOptions.IgnoreCase Or RegexOptions.Compiled)
                Dim m As Match = r.Match(sText)
                While m.Success
                    Dim Idx As Integer = m.Groups(0).Index
                    Dim sOrig As String = m.Groups(0).Value
                    Dim sID As String = m.Groups(1).Value

                    Dim isLink As Boolean = False
                    If Idx > 10 Then
                        Dim sPrev As String = sText.Substring(Idx - 8, 8).ToLower
                        isLink = sPrev.Contains("=") AndAlso (sPrev.Contains("href") OrElse sPrev.Contains("src"))
                    End If

                    If sID <> "" AndAlso Not isLink Then
                        ' D3699 ===
                        Dim sExtra As String = ""
                        Dim sParam As String = String.Format("rel=0&enablejsapi=1&origin={0}", Domain)   ' D7134 + D7135 + D7141 // &enablejsapi=1&origin={0}
                        If fAllowAutoPlay AndAlso offset = 0 AndAlso (HTML2Text(sText).Length - sOrig.Length) < 500 AndAlso isFirstVideo Then    ' D7141 // AndAlso String.IsNullOrEmpty(JS_YoutubeScript)
                            sExtra = "autoplay; "  ' D6672 sExtra = "&autoplay=1" ' autoplay first video and when not so much text near to video frame
                            'sParam += If(sParam = "", "", "&") + "mute=1&autoplay=1"  ' D7134 + D7135 + D7555 // autoplay is not working // no need to mute
                        End If
                        If sParam <> "" AndAlso Not sParam.StartsWith("?") Then sParam = "?" + sParam   ' D7135
                        Dim sNew As String = String.Format(sReplace, sID, sExtra, sParam, If(isFirstVideo, " id='iframe_youtube'", ""))   ' D7134
                        ' D3699 ==

                        ' D7141 ===
                        If isFirstVideo AndAlso Not String.IsNullOrEmpty(JS_YoutubeScript) Then sNew += JS_YoutubeScript
                        isFirstVideo = False
                        ' D7141 ==

                        sResult = sResult.Remove(Idx + offset, sOrig.Length)
                        sResult = sResult.Insert(Idx + offset, sNew)
                        offset += (sNew.Length - sOrig.Length)
                    End If

                    m = m.NextMatch()
                End While
            End If

            ' D6675 ===
            If _OPT_PARSE_SCREENCAST_EMBED AndAlso sResult.ToLower.Contains("screencast.com") Then  ' D6714
                Dim sReplace As String = "<div style='position:relative; padding-bottom:{1}%; height:0; overflow:hidden; border:1px solid #f0f0f0; box-shadow: 4px 4px 6px rgba(150,150,150,0.4); background: url(/Images/preload.gif) no-repeat center center; background-size: 60px;'><iframe scrolling='no' type='text/html' src='https://www.screencast.com/{0}' webkitallowfullscreen='' mozallowfullscreen='' allowfullscreen='' frameborder='0' style='position:absolute; top:0; left:0; width:100%; height:100%;'></iframe></div>"
                Dim offset As Integer = 0
                Dim idx As Integer
                Do
                    idx = sResult.IndexOf("screencast.com/", offset, StringComparison.InvariantCultureIgnoreCase)
                    If idx > 0 Then
                        Dim frame As Integer = sResult.LastIndexOf("<iframe ", idx, StringComparison.InvariantCultureIgnoreCase)
                        Dim iframe As Integer = sResult.IndexOf("</iframe>", idx, StringComparison.InvariantCultureIgnoreCase)
                        If frame >= offset AndAlso iframe > 0 Then
                            Dim sQuote1 As Integer = sResult.IndexOf("'", idx)
                            Dim sQuote2 As Integer = sResult.IndexOf("""", idx)
                            If sQuote2 > 0 AndAlso (sQuote2 < sQuote1 OrElse sQuote1 < 0) Then sQuote1 = sQuote2
                            If sQuote1 > idx Then
                                Dim tRate As Double = 56.7
                                Dim widx As Integer = sResult.IndexOf("width=", frame, StringComparison.InvariantCultureIgnoreCase)
                                Dim hidx As Integer = sResult.IndexOf("height=", frame, StringComparison.InvariantCultureIgnoreCase)
                                Dim tmp As Integer
                                Dim w As Double = 0
                                Dim h As Double = 0
                                If (widx > 0) Then
                                    tmp = sResult.IndexOf(" ", widx)
                                    If tmp > 0 Then String2Double(sResult.Substring(widx + 7, tmp - widx - 8), w)
                                End If
                                If (hidx > 0) Then
                                    tmp = sResult.IndexOf(" ", hidx)
                                    If tmp > 0 Then String2Double(sResult.Substring(hidx + 8, tmp - hidx - 9), h)
                                End If
                                If w > 0 AndAlso h > 0 Then tRate = h / w
                                Dim sPath As String = sResult.Substring(idx + 15, sQuote1 - idx - 15)
                                If sPath.Length > 0 Then
                                    Dim sEmbed As String = String.Format(sReplace, sPath, Math.Round(100 * tRate))
                                    sResult = sResult.Substring(0, frame) + sEmbed + sResult.Substring(iframe + 9)
                                    offset = frame + sEmbed.Length
                                End If
                            End If
                        Else
                            offset = idx + 15   ' D6794
                        End If
                    End If
                Loop While idx > 0
            End If
            ' D6675 ==

            Return sResult
        End Function
        ' D3697 ==

        ' D1039 ===
        Public Function ParseTextHyperlinks(ByVal sText As String) As String
            Dim strPattern As String
            Dim strReplace As String
            Dim strResult As String = sText
            If sText.ToLower.IndexOf(" href") < 0 Then

                'strPattern = "[^/>]+(?<url>www\.(?:[\w-]+\.)+[\w-]+(?:/[\w-./?%&~=]*[^.])?)"
                ''strReplace = "<a href=""http://${url}!"" target=_blank>${url}!</a>"
                'strReplace = " http://${url}"
                'strResult = Regex.Replace(" " + strResult, strPattern, strReplace).Trim ' D4102

                strPattern = "(?<prot>(https|http|ftp|mms)://)(?<url>(?:[\w-]+\.)+[\w-]+(?:/[\w-./?%&~=\#]*[^.\s\t\r\n\<])?)"   ' D4371
                strReplace = "<a href=""${prot}${url}"" target=_blank>${url}</a>"
                strResult = Regex.Replace(strResult, strPattern, strReplace)

            End If
            Return strResult
        End Function
        ' D1039 ==

    End Module

End Namespace
