Imports System.IO
Imports System.Security.Cryptography
Imports Org.BouncyCastle.Crypto
Imports Org.BouncyCastle.Crypto.Engines
Imports Org.BouncyCastle.Crypto.Modes
Imports Org.BouncyCastle.Crypto.Paddings
Imports Org.BouncyCastle.Crypto.Parameters


Namespace ECSecurity

    Public Module CryptFuncs

        '#Const _USE_DOTNET_ENCODING = True ' D4822

        '#If _USE_DOTNET_ENCODING Then

        '        Public Const _OPT_USE_DOTNET_ENCODING = True

        '#Else

        Public FIPS_MODE As Boolean = False ' D6689

        Private Function GetCipherData(ByVal cipher As PaddedBufferedBlockCipher, ByVal data As Byte()) As Byte()
            Dim minSize As Integer = cipher.GetOutputSize(data.Length)
            Dim outBuf As Byte() = New Byte(minSize - 1) {}
            Dim length1 As Integer = cipher.ProcessBytes(data, 0, data.Length, outBuf, 0)
            Dim length2 As Integer = cipher.DoFinal(outBuf, length1)
            Dim actualLength As Integer = length1 + length2
            Dim result As Byte() = New Byte(actualLength - 1) {}
            System.Array.Copy(outBuf, 0, result, 0, result.Length)
            Return result
        End Function

        '#End If
        Public Function Encrypt(ByVal PlainTextBytes As Byte(), ByVal Key As Byte(), ByVal IV As Byte()) As Byte()
            Dim encryptedData As Byte()

            '#If _USE_DOTNET_ENCODING Then
            If Not FIPS_MODE Then   ' .Net

                Dim ms As New MemoryStream
                Dim alg As New RijndaelManaged

                alg.Key = Key
                alg.IV = IV

                Dim cs As New CryptoStream(ms, alg.CreateEncryptor, CryptoStreamMode.Write)
                cs.Write(PlainTextBytes, 0, PlainTextBytes.Length)
                'cs.Flush()
                cs.Close()

                encryptedData = ms.ToArray

            Else
                '#Else

                Dim aes As PaddedBufferedBlockCipher = New PaddedBufferedBlockCipher(New CbcBlockCipher(New AesEngine()))
                Dim ivAndKey As ICipherParameters = New ParametersWithIV(New KeyParameter(Key), IV)
                aes.Init(True, ivAndKey)
                encryptedData = GetCipherData(aes, PlainTextBytes)

            End If
            '#End If

            Return encryptedData
        End Function

        Public Function Encrypt(ByVal PlainTextBytes As Byte(), ByVal Password As String) As Byte()
            Dim pdb As PasswordDeriveBytes = New PasswordDeriveBytes(Password, New Byte() {&H49, &H76, &H61, &H6E, &H20, &H4D, &H65, &H64, &H76, &H65, &H64, &H65, &H76})
            Return Encrypt(PlainTextBytes, pdb.GetBytes(32), pdb.GetBytes(16))
        End Function

        Public Function Decrypt(ByVal CipherData As Byte(), ByVal Key As Byte(), ByVal IV As Byte()) As Byte()
            Dim decryptedData As Byte()

            '#If _USE_DOTNET_ENCODING Then
            If Not FIPS_MODE Then   ' .Net
                Dim ms As New MemoryStream
                Dim alg As New RijndaelManaged
                alg.Key = Key
                alg.IV = IV

                Dim cs As New CryptoStream(ms, alg.CreateDecryptor, CryptoStreamMode.Write)
                cs.Write(CipherData, 0, CipherData.Length)
                cs.Close()

                decryptedData = ms.ToArray

            Else
                '#Else

                Dim aes As PaddedBufferedBlockCipher = New PaddedBufferedBlockCipher(New CbcBlockCipher(New AesEngine()))
                Dim ivAndKey As ICipherParameters = New ParametersWithIV(New KeyParameter(Key), IV)
                aes.Init(False, ivAndKey)
                decryptedData = GetCipherData(aes, CipherData)

            End If
            '#End If

            Return decryptedData
        End Function

        Public Function Decrypt(ByVal CipherData As Byte(), ByVal Password As String) As Byte()
            Dim pdb As PasswordDeriveBytes = New PasswordDeriveBytes(Password, New Byte() {&H49, &H76, &H61, &H6E, &H20, &H4D, &H65, &H64, &H76, &H65, &H64, &H65, &H76})
            Return Decrypt(CipherData, pdb.GetBytes(32), pdb.GetBytes(16))
        End Function

    End Module

End Namespace