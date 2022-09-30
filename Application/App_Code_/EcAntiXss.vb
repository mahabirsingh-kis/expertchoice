Imports AntiXss= Microsoft.Security.Application

Public Class EcAntiXss
    ''' <summary>
    ''' Encodes input string for use in HTML
    ''' </summary>
    ''' <param name="input"></param>
    ''' <returns></returns>
    Public Shared Function HtmlEncode(input As String) As String
        Return HtmlEncode(input, False) ' Do not change parameter values
    End Function

    ''' <summary>
    ''' Encodes input string for use in HTML
    ''' </summary>
    ''' <param name="input"></param>
    ''' <param name="useNamedEntities"></param>
    ''' <returns></returns>
    Public Shared Function HtmlEncode(input As String, useNamedEntities As Boolean) As String
        input = AntiXss.Encoder.HtmlEncode(input, useNamedEntities)
        Return If(String.IsNullOrEmpty(input), string.Empty, input)
    End Function

    ''' <summary>
    ''' Encodes the specified string for using Cascading Style Sheet (CSS) attributes. The return value from this function is exptected to be used in building an attribute string
    ''' </summary>
    ''' <param name="input"></param>
    ''' <returns></returns>
    Public Shared Function CssEncode(input As String) As String
        input = AntiXss.Encoder.CssEncode(input)
        Return If(String.IsNullOrEmpty(input), string.Empty, input)
    End Function

    ''' <summary>
    ''' Encodes input string for use in an HTML attribute
    ''' </summary>
    ''' <param name="input"></param>
    ''' <returns></returns>
    Public Shared Function HtmlAttributeEncode(input As String) As String
        input = AntiXss.Encoder.HtmlAttributeEncode(input)
        Return If(String.IsNullOrEmpty(input), string.Empty, input)
    End Function

    ''' <summary>
    ''' Encodes input string for use in application/x-www-form-urlencode form submissions
    ''' </summary>
    ''' <param name="input"></param>
    ''' <returns></returns>
    Public Shared Function HtmlFormUrlEncode(input As String) As String
        Return HtmlFormUrlEncode(input, Encoding.UTF8)  ' Do not change parameter values
    End Function

    ''' <summary>
    ''' Encodes input string for use in application/x-www-form-urlencode form submissions
    ''' </summary>
    ''' <param name="input"></param>
    ''' <param name="inputEncoding"></param>
    ''' <returns></returns>
    Public Shared Function HtmlFormUrlEncode(input As String, inputEncoding As Encoding) As String
        If(IsNothing(inputEncoding))
            inputEncoding = Encoding.UTF8   ' Do not change default encoding
        End If

        input = AntiXss.Encoder.HtmlFormUrlEncode(input, inputEncoding)
        Return If(String.IsNullOrEmpty(input), string.Empty, input)
    End Function

    ''' <summary>
    ''' Encodes input string for use in application/x-www-form-urlencode form submissions
    ''' </summary>
    ''' <param name="input"></param>
    ''' <param name="codePage"></param>
    ''' <returns></returns>
    Public Shared Function HtmlFormUrlEncode(input As String, codePage As Integer) As String
        input = AntiXss.Encoder.HtmlFormUrlEncode(input, Encoding.GetEncoding(codePage))' Do not change parameter values
        Return If(String.IsNullOrEmpty(input), string.Empty, input)
    End Function

    ''' <summary>
    ''' Encodes input string for use in JavaScript
    ''' </summary>
    ''' <param name="input"></param>
    ''' <returns></returns>
    Public Shared Function JavaScriptEncode(input As String) As String
        Return JavaScriptEncode(input, False)
    End Function

    ''' <summary>
    ''' Encodes input string for use in JavaScript
    ''' </summary>
    ''' <param name="input"></param>
    ''' <param name="emitQuotes"></param>
    ''' <returns></returns>
    Public Shared Function JavaScriptEncode(input As String, emitQuotes As Boolean) As String
        input = AntiXss.Encoder.JavaScriptEncode(input, emitQuotes)
        Return If(String.IsNullOrEmpty(input), string.Empty, input)
    End Function

    ''' <summary>
    ''' Encodes input string for use as a value in Lightweight Directory Access Protocol (LDAP) DNs
    ''' </summary>
    ''' <param name="input"></param>
    ''' <returns></returns>
    Public Shared Function LdapDistinguishedNameEncode(input As String) As String
        Return LdapDistinguishedNameEncode(input, True, True)   ' Do not change parameter values
    End Function

    ''' <summary>
    ''' Encodes input string for use as a value in Lightweight Directory Access Protocol (LDAP) DNs
    ''' </summary>
    ''' <param name="input"></param>
    ''' <param name="useInitialCharacterRules"></param>
    ''' <param name="useFinalCharacterRule"></param>
    ''' <returns></returns>
    Public Shared Function LdapDistinguishedNameEncode(input As String, useInitialCharacterRules As Boolean, useFinalCharacterRule As Boolean) As String
        input = AntiXss.Encoder.LdapDistinguishedNameEncode(input, useInitialCharacterRules, useFinalCharacterRule)
        Return If(String.IsNullOrEmpty(input), string.Empty, input)
    End Function

    ''' <summary>
    ''' Encodes input string for use as a value in Lightweight Directory Access Protocol (LDAP) filter queries
    ''' </summary>
    ''' <param name="input"></param>
    ''' <returns></returns>
    Public Shared Function LdapFilterEncode(input As String) As String
        input = AntiXss.Encoder.LdapFilterEncode(input)
        Return If(String.IsNullOrEmpty(input), string.Empty, input)
    End Function

    ''' <summary>
    ''' Encodes input string for use in Universal Resource Locators (URLs)
    ''' </summary>
    ''' <param name="input"></param>
    ''' <returns></returns>
    Public Shared Function UrlEncode(input As String) As String
        Return UrlEncode(input, Encoding.UTF8)  ' Do not change parameter values
    End Function

    ''' <summary>
    ''' Encodes input string for use in Universal Resource Locators (URLs)
    ''' </summary>
    ''' <param name="input"></param>
    ''' <param name="inputEncoding"></param>
    ''' <returns></returns>
    Public Shared Function UrlEncode(input As String, inputEncoding As Encoding) As String
        If(IsNothing(inputEncoding))
            inputEncoding = Encoding.UTF8   ' Do not change default encoding
        End If

        input = AntiXss.Encoder.UrlEncode(input, inputEncoding)
        Return If(String.IsNullOrEmpty(input), string.Empty, input)
    End Function

    ''' <summary>
    ''' Encodes input string for use in Universal Resource Locators (URLs)
    ''' </summary>
    ''' <param name="input"></param>
    ''' <param name="codePage"></param>
    ''' <returns></returns>
    Public Shared Function UrlEncode(input As String, codePage As Integer) As String
        input = AntiXss.Encoder.UrlEncode(input, Encoding.GetEncoding(codePage))   ' Do not change parameter values
        Return If(String.IsNullOrEmpty(input), string.Empty, input)
    End Function

    ''' <summary>
    ''' URL - encodes the path section of a URL string and returns the encoding string
    ''' </summary>
    ''' <param name="input"></param>
    ''' <returns></returns>
    Public Shared Function UrlPathEncode(input As String) As String
        input = AntiXss.Encoder.UrlPathEncode(input)
        Return If(String.IsNullOrEmpty(input), string.Empty, input)
    End Function

    ''' <summary>
    ''' Encodes input string for use in Visual Basic Script
    ''' </summary>
    ''' <param name="input"></param>
    ''' <returns></returns>
    Public Shared Function VisualBasicScriptEncode(input As String) As String
        input = AntiXss.Encoder.VisualBasicScriptEncode(input)
        Return If(String.IsNullOrEmpty(input), string.Empty, input)
    End Function

    ''' <summary>
    ''' Encodes input string for use in XML attributes
    ''' </summary>
    ''' <param name="input"></param>
    ''' <returns></returns>
    Public Shared Function XmlAttributeEncode(input As String) As String
        input = AntiXss.Encoder.XmlAttributeEncode(input)
        Return If(String.IsNullOrEmpty(input), string.Empty, input)
    End Function

    ''' <summary>
    ''' Encode input string for use in XML
    ''' </summary>
    ''' <param name="input"></param>
    ''' <returns></returns>
    Public Shared Function XmlEncode(input As String) As String
        input = AntiXss.Encoder.XmlEncode(input)
        Return If(String.IsNullOrEmpty(input), string.Empty, input)
    End Function
End Class
