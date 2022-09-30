Imports System.Reflection
Imports Microsoft.Security.Application

Public Class EcSanitizer
    Private Const CrMarker As String = "|cr|"
    Private Const LfMarker As String = "|lf|"
    Private Const CrLfMarker As String = "|crLf|"
    Private Const TabMarker As String = "|tab|"
    Private Const VerticalTabMarker As String = "|vTab|"
    Private Const NewLineMarker As String = "|newLine|"
    Private Const SpaceMarker As String = "|space|"
    Private Const AndMarker As String = "|and|"

    'Language specific character constants. Need to use 3 character language code and then the character
    '----- Russian Characters Starts -----
    Private Const LanRusM As String = "|LanRusM|"
    '----- Russian Characters Ends -----

    ''' <summary>
    ''' Sanitizes input HTML fragment for safe display on browser without removing some useful characters
    ''' </summary>
    ''' <param name="input"></param>
    Public Shared Function GetSafeHtmlFragment(input As String) As String
        input = If(String.IsNullOrEmpty(input), string.Empty, input)

        ' Replacing some useful characters so that Sanitizer doesn't remove those
        input = ReplaceUsefulCharactersBeforeSanitization(input)

        ' Sanitizes input HTML fragment for safe display on browser
        input = Sanitizer.GetSafeHtmlFragment(input)

        ' Replacing back useful characters. MUST NOT Trim
        input = ReplaceUsefulCharactersBackAfterSanitization(input)

        Return input
    End Function

    ''' <summary>
    ''' Sanitizes input HTML document for safe display on browser without removing some useful characters if isIncoming is true
    ''' </summary>
    ''' <param name="input"></param>
    Public Shared Function GetSafeHtml(input As String) As String
        input = If(String.IsNullOrEmpty(input), string.Empty, input)

        ' Replacing some useful characters so that Sanitizer doesn't remove those
        input = ReplaceUsefulCharactersBeforeSanitization(input)

        ' Sanitizes input HTML document for safe display on browser
        input = Sanitizer.GetSafeHtml(input)

        ' Replacing back useful characters. MUST NOT Trim
        input = ReplaceUsefulCharactersBackAfterSanitization(input)

        Return input
    End Function

    Public Shared Function GetSafeUrl(input as String) as String
        input = If(String.IsNullOrEmpty(input), string.Empty, input)

        ' Encodes URL to prevent XSS attack
        input = Microsoft.Security.Application.Encoder.UrlEncode(input)

        Return input
    End Function

    Public Shared Sub SetSafeNameValueCollection(ByRef valueCollection As NameValueCollection)
        Dim readableInfo As PropertyInfo = valueCollection.[GetType]().GetProperty("IsReadOnly", BindingFlags.NonPublic Or BindingFlags.Instance)
        readableInfo.SetValue(valueCollection, False, Nothing)

        For i As Integer = 0 To valueCollection.Count - 1
            If (Not (String.IsNullOrEmpty(valueCollection.Keys(i)) OrElse String.IsNullOrEmpty(valueCollection(valueCollection.Keys(i))))) Then
                If (valueCollection(valueCollection.Keys(i)).ToLower().Contains("<script>") OrElse valueCollection(valueCollection.Keys(i)).ToLower().Contains("</script>")) Then
                    valueCollection(valueCollection.Keys(i)) = GetSafeHtmlFragment(valueCollection(valueCollection.Keys(i)))
                End if

                If valueCollection.Keys(i).Equals("__CALLBACKPARAM", StringComparison.CurrentCultureIgnoreCase) Then
                    valueCollection(valueCollection.Keys(i)) = RemoveXssFromParameter(valueCollection(valueCollection.Keys(i)), False)
                End If
            End If
        Next

        readableInfo.SetValue(valueCollection, True, Nothing)
    End Sub

    Private Shared Function ReplaceUsefulCharactersBeforeSanitization(input As String) As String
        'General
        input = input.Replace(vbCrLf, CrLfMarker)
        input = input.Replace(vbCr, CrMarker)
        input = input.Replace(vbLf, LfMarker)
        input = input.Replace(vbTab, TabMarker)
        input = input.Replace(vbVerticalTab, VerticalTabMarker)
        input = input.Replace(vbNewLine, NewLineMarker)
        input = input.Replace("&", AndMarker)
        input = input.Replace(" ", SpaceMarker)

        'Language specific
        input = input.Replace("м", LanRusM)

        Return input
    End Function

    Private Shared Function ReplaceUsefulCharactersBackAfterSanitization(input As String) As String
        'Language specific
        input = input.Replace(LanRusM, "м")

        'General
        input = input.Replace(SpaceMarker, " ")
        input = input.Replace(AndMarker, "&")
        input = input.Replace(NewLineMarker, vbNewLine)
        input = input.Replace(VerticalTabMarker, vbVerticalTab)
        input = input.Replace(TabMarker, vbTab)
        input = input.Replace(LfMarker, vbLf)
        input = input.Replace(CrMarker, vbCr)
        input = input.Replace(CrLfMarker, vbCrLf)

        Return input
    End Function
End Class

