Imports ECSecurity.ECSecurity

Namespace ExpertChoice.Service

    Public Module CryptService

        ''' <summary>
        ''' Common password for encrypt/decrypt strings, like a URI params
        ''' </summary>
        ''' <remarks></remarks>
        Friend Const _DEF_CRYPT_PASSWORD As String = "ExpertChoiceSecurity"

        Friend Const _DEF_ENCRYPTED_PREFIX As String = "¶"                      ' D0273
        'Public Const _DEF_ENCRYPTED_BLANK_PSW As String = _DEF_ENCRYPTED_PREFIX + "pSxoydFD62y57YMkwG5KEQ--"    ' D0838

        Public Function EncodeURL(ByVal sURL As String, ByVal sCryptPsw As String) As String ' D0826
            Try
                Dim DecodedBytes As Byte() = System.Text.Encoding.UTF8.GetBytes(sURL)
                If String.IsNullOrEmpty(sCryptPsw) Then sCryptPsw = _DEF_CRYPT_PASSWORD ' D0826
                Dim EncodedBytes As Byte() = Encrypt(DecodedBytes, sCryptPsw)   ' D0826
                Dim sEncodedStr As String = Convert.ToBase64String(EncodedBytes)
                Return sEncodedStr.Replace("+", "_").Replace("/", "!").Replace("=", "-")
            Catch ex As Exception
                DebugInfo(ex.Message, _TRACE_RTE)   ' D0330
                Return ""
            End Try
        End Function

        Public Function DecodeURL(ByVal sURL As String, ByVal sCryptPsw As String) As String    ' D0826
            Try
                Dim sEncodedStr As String = sURL.Replace("_", "+").Replace("!", "/").Replace("-", "=")
                Dim EncodedBytes As Byte() = Convert.FromBase64String(sEncodedStr)
                If String.IsNullOrEmpty(sCryptPsw) Then sCryptPsw = _DEF_CRYPT_PASSWORD ' D0826
                ' D0826 ===
                Dim DecodedBytes As Byte() = {}
                Try
                    DecodedBytes = Decrypt(EncodedBytes, sCryptPsw)
                    Return System.Text.Encoding.UTF8.GetString(DecodedBytes)
                Catch ex As Exception
                    If sCryptPsw <> _DEF_CRYPT_PASSWORD Then
                        Return DecodeURL(sURL, _DEF_CRYPT_PASSWORD)
                    End If
                End Try
                ' D0826 ==
            Catch ex As Exception
                DebugInfo(ex.Message, _TRACE_RTE)   ' D0330
            End Try
            Return ""   ' D0826
        End Function
        ' D0214 ==

        ' D0273 ===
        Public Function isEncodedString(ByVal sText As String) As Boolean
            Return sText.StartsWith(_DEF_ENCRYPTED_PREFIX)
        End Function

        Public Function EncodeString(ByVal sText As String, ByVal sCryptPsw As String, Optional ByVal fUserEncodeSign As Boolean = True) As String  ' D0826
            Try
                Dim sEncodedStrting As String = EncodeURL(sText, sCryptPsw) ' D0826
                'Dim EncodedBytes As Byte() = Encrypt(System.Text.Encoding.UTF8.GetBytes(sText), _DEF_CRYPT_PASSWORD)
                'Dim sEncodedStrting As String = System.Text.Encoding.UTF8.GetString(EncodedBytes)
                If fUserEncodeSign Then sEncodedStrting = _DEF_ENCRYPTED_PREFIX + sEncodedStrting
                Return sEncodedStrting
            Catch ex As Exception
                DebugInfo(ex.Message, _TRACE_RTE)   ' D0330
                Return sText
            End Try
        End Function

        Public Function DecodeString(ByVal sText As String, ByVal sCryptPsw As String, Optional ByVal fCheckEncodeSign As Boolean = True) As String   ' D0826
            Try
                Dim fIsEncoded As Boolean = isEncodedString(sText)
                If fCheckEncodeSign And Not fIsEncoded Then Return sText
                If fIsEncoded Then sText = sText.Substring(_DEF_ENCRYPTED_PREFIX.Length)
                Return DecodeURL(sText, sCryptPsw)  ' D0826
                'Dim DecodedBytes As Byte() = Decrypt(System.Text.Encoding.UTF8.GetBytes(sText), _DEF_CRYPT_PASSWORD)
                'Return System.Text.Encoding.UTF8.GetString(DecodedBytes)
            Catch ex As Exception
                DebugInfo(ex.Message, _TRACE_RTE)   ' D0330
                Return sText
            End Try
        End Function
        ' D0273 ==

        Public _DEF_ENCODED_BLANK_PSW As String = EncodeString("", Nothing, True)   ' D0838

    End Module

End Namespace
