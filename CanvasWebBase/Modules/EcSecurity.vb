Imports System.Linq
Imports System.Text.RegularExpressions
Imports Newtonsoft.Json.Linq

Namespace ExpertChoice.Web
    Public Module XSS   ' AD: rename from ECSecurity since the conflict naming with ECSecurity namespace    ' D6689
        ''' <summary>
        ''' Returns a new LDAP string after removing vulnerable characters
        ''' </summary>
        ''' <param name="vulnerableString"></param>
        ''' <returns></returns>
        Public Function RemoveXssFromLdap(ByVal vulnerableString As String) As String
            Dim decodedString As String = HttpUtility.UrlDecode(vulnerableString)
            Dim isEncoded As Boolean = CBool(IIf(vulnerableString.Equals(decodedString, StringComparison.CurrentCultureIgnoreCase), False, True))

            If isEncoded Then
                vulnerableString = decodedString
            End If

            vulnerableString = RemoveVulnerableCharacters(vulnerableString, XssType.Ldap)

            If isEncoded Then
                vulnerableString = HttpUtility.UrlEncode(vulnerableString)
            End If

            Return vulnerableString
        End Function

        ''' <summary>
        ''' Returns a new string after removing vulnerable characters
        ''' </summary>
        ''' <param name="vulnerableString"></param>
        ''' <returns></returns>
        Public Function RemoveXssFromText(ByVal vulnerableString As String) As String
            Dim decodedString As String = HttpUtility.HtmlDecode(vulnerableString)
            Dim isEncoded As Boolean = CBool(IIf(vulnerableString.Equals(decodedString, StringComparison.CurrentCultureIgnoreCase), False, True))

            If isEncoded Then
                vulnerableString = decodedString
            End If

            vulnerableString = RemoveVulnerableCharacters(vulnerableString, XssType.Text)

            If isEncoded Then
                vulnerableString = HttpUtility.HtmlEncode(vulnerableString)
            End If

            Return vulnerableString
        End Function

        ''' <summary>
        ''' Returns a new URL string after removing vulnerable parameters or replacing vulnerable characters
        ''' </summary>
        ''' <param name="url"></param>
        ''' <returns></returns>
        Public Function RemoveXssFromUrl(ByVal url As String, ByVal Optional replace As Boolean = False) As String
            If Not String.IsNullOrEmpty(url) Then
                Dim decodedUrl As String = HttpUtility.UrlDecode(url)
                Dim isUrlEncoded As Boolean = CBool(IIf(url.Equals(decodedUrl, StringComparison.CurrentCultureIgnoreCase), False, True))

                If isUrlEncoded Then
                    url = decodedUrl
                End If

                Dim splitString As String = CStr(IIf(url.Contains(HttpUtility.UrlEncode("?")), HttpUtility.UrlEncode("?"), "?"))
                Dim splittedUrl As String() = url.Split(New String() {splitString}, StringSplitOptions.None)  ' separating path and parameters
                url = splittedUrl(0)  ' assigning path to url

                ' for path
                If url.Length > 0 Then
                    url = RemoveVulnerableCharacters(url, XssType.Url)
                End If

                ' for parameters
                If splittedUrl.Length > 1 Then
                    If replace Then
                        url += "?" + RemoveVulnerableCharacters(splittedUrl(1), XssType.Url)
                    Else
                        Dim vList As List(Of String) = GetVulnerableStringsForUrl()   ' gets vulnerable characters or strings of url

                        For i As Integer = 1 To splittedUrl.Length - 1
                            splitString = CStr(IIf(splittedUrl(i).Contains(HttpUtility.UrlEncode("&")), HttpUtility.UrlEncode("&"), "&"))
                            Dim params As String() = splittedUrl(i).Split(New String() {splitString}, StringSplitOptions.None)    ' splitting parameters

                            For Each iParam As String In params
                                Dim isFound As Boolean = vList.Any(Function(v) iParam.Contains(v))    ' checking if any vulnerable characters in parameter

                                If Not isFound Then
                                    url += CStr(IIf(url.Length = 0, url, IIf(url.Contains("?"), "&", "?"))) + iParam  ' adding parameter when no vulnerable characters found
                                End If
                            Next
                        Next
                    End If
                End If

                If isUrlEncoded Then
                    url = HttpUtility.UrlEncode(url).Replace(HttpUtility.UrlEncode("/"), "/").Replace(HttpUtility.UrlEncode("?"), "?").Replace(HttpUtility.UrlEncode("="), "=")
                End If

            End If

            Return url
        End Function

        Public Function RemoveXssFromParameter(ByVal vulnerableString As String, ByVal Optional isUrlParameter As Boolean = True) As String
            If Not String.IsNullOrEmpty(vulnerableString) Then  ' D6992

                Dim decodedString As String = HttpUtility.UrlDecode(vulnerableString)
                Dim isEncoded As Boolean = CBool(IIf(vulnerableString.Equals(decodedString, StringComparison.CurrentCultureIgnoreCase), False, True))
                Dim type As XssType = CType(IIf(isUrlParameter, XssType.Url, XssType.Parameter), XssType)

                If isEncoded Then
                    vulnerableString = decodedString
                End If

                vulnerableString = RemoveVulnerableCharacters(vulnerableString, type)

                If isEncoded Then
                    vulnerableString = HttpUtility.UrlEncode(vulnerableString)
                End If
            End If

            Return vulnerableString
        End Function

        ''' <summary>
        ''' Returns a new string after replacing vulnerable characters
        ''' </summary>
        ''' <param name="vulnerableString"></param>
        ''' <param name="type"></param>
        ''' <returns></returns>
        Private Function RemoveVulnerableCharacters(ByVal vulnerableString As String, ByVal type As XssType) As String
            Dim regexPattern As String = String.Empty

            Select Case type
                Case XssType.Ldap
                    regexPattern = "\*|[(]|[)]|=|;|,|!|&|\|"
                    vulnerableString = Regex.Replace(vulnerableString, regexPattern, String.Empty, RegexOptions.IgnoreCase)
                Case XssType.Parameter
                    regexPattern = "<[^<>]*?>|</?|>|alert[(].*[)]|\.\.+"

                    If ((vulnerableString.StartsWith("{") AndAlso vulnerableString.EndsWith("}")) OrElse (vulnerableString.StartsWith("[") AndAlso vulnerableString.EndsWith("]"))) Then
                        Try
                            'Newtonsoft.Json.Linq.JContainer.Parse(vulnerableString)
                            Dim jToken As JToken = JToken.Parse(vulnerableString)
                        Catch ex As Exception
                            regexPattern += "|{+|}+|('+|""+)o?|:+"
                        End Try
                    End If

                    If (Regex.IsMatch(vulnerableString, "(\|\w+);(\w+\|)?")) Then
                        'check for telerik control grid pagination parameter "GB|20;12|PAGERONCLICK3|PN1;"
                        regexPattern += "|;;+"
                    Else
                        regexPattern += "|;+"
                    End If
                    vulnerableString = Regex.Replace(vulnerableString, regexPattern, String.Empty, RegexOptions.IgnoreCase)
                Case XssType.Text
                    regexPattern = "<[^<>]*?>|</?|>|('+|""+)o?|alert[(].*[)]"
                    vulnerableString = Regex.Replace(vulnerableString, regexPattern, String.Empty, RegexOptions.IgnoreCase)
                Case XssType.Url
                    Dim currentUrl As Uri
                    Dim schemeAuthority As String = String.Empty

                    If vulnerableString.Contains(Uri.SchemeDelimiter) Then
                        Try
                            currentUrl = New Uri(vulnerableString)  ' vulnerableString is always decoded url string
                            schemeAuthority = currentUrl.OriginalString.Replace(HttpUtility.UrlDecode(currentUrl.PathAndQuery), "") ' decoding PathAndQuery as it's got encoded
                            vulnerableString = vulnerableString.Replace(schemeAuthority, "") ' removing scheme and authority so that ":" dosen't get replaced
                        Catch ex As Exception

                        End Try
                    End If

                    regexPattern = "<[^<>]*?>|</?|>|('+|""+)o?|alert[(].*[)]|;+|{+|}+|\.\.+|:+"
                    vulnerableString = schemeAuthority + Regex.Replace(vulnerableString, regexPattern, String.Empty, RegexOptions.IgnoreCase)
            End Select

            Return vulnerableString
        End Function

        ''' <summary>
        ''' Returns list of vulnerable characters and strings for URL
        ''' </summary>
        ''' <returns></returns>
        Private Function GetVulnerableStringsForUrl() As List(Of String)
            Dim vLis As List(Of String) = New List(Of String)()

            ' Never add "(" and ")" as these are valid url characters
            ' replacing single quote with first character of HTML events which starts with "o" (as IE does)
            vLis.Add("'O")
            'vLis.Add(CStr(IIf(type = XssType.Text, HttpUtility.HtmlEncode("'O"), HttpUtility.UrlEncode("'O"))))
            vLis.Add("'o")
            'vLis.Add(CStr(IIf(type = XssType.Text, HttpUtility.HtmlEncode("'o"), HttpUtility.UrlEncode("'o"))))

            ' replacing double quote with first character of HTML events which starts with "o" (as IE does)
            vLis.Add("""O")
            'vLis.Add(CStr(IIf(type = XssType.Text, HttpUtility.HtmlEncode("""O"), HttpUtility.UrlEncode("""O"))))
            vLis.Add("""o")
            'vLis.Add(CStr(IIf(type = XssType.Text, HttpUtility.HtmlEncode("""o"), HttpUtility.UrlEncode("""o"))))

            vLis.Add("'")
            'vLis.Add(CStr(IIf(type = XssType.Text, HttpUtility.HtmlEncode("'"), HttpUtility.UrlEncode("'"))))
            vLis.Add("""")
            'vLis.Add(CStr(IIf(type = XssType.Text, HttpUtility.HtmlEncode(""""), HttpUtility.UrlEncode(""""))))

            vLis.Add("<script>")
            'vLis.Add(CStr(IIf(type = XssType.Text, HttpUtility.HtmlEncode("<script>"), HttpUtility.UrlEncode("<script>"))))
            vLis.Add("</script>")
            'vLis.Add(CStr(IIf(type = XssType.Text, HttpUtility.HtmlEncode("</script>"), HttpUtility.UrlEncode("</script>"))))

            vLis.Add("</")
            'vLis.Add(CStr(IIf(type = XssType.Text, HttpUtility.HtmlEncode("</"), HttpUtility.UrlEncode("</"))))
            vLis.Add("<")
            'vLis.Add(CStr(IIf(type = XssType.Text, HttpUtility.HtmlEncode("<"), HttpUtility.UrlEncode("<"))))
            vLis.Add(">")
            'vLis.Add(CStr(IIf(type = XssType.Text, HttpUtility.HtmlEncode(">"), HttpUtility.UrlEncode(">"))))

            vLis.Add(";")
            'vLis.Add(HttpUtility.UrlEncode(";"))
            vLis.Add("{")
            'vLis.Add(HttpUtility.UrlEncode("{"))
            vLis.Add("}")
            'vLis.Add(HttpUtility.UrlEncode("}"))

            Return vLis
        End Function

        Private Enum XssType
            Ldap = 1
            Parameter = 2
            Text = 3
            Url = 4
        End Enum
    End Module
End Namespace