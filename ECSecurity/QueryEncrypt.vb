Imports System
Imports System.Collections.Generic
Imports System.Text
Imports System.Security.Cryptography
Imports System.IO
Imports System.Configuration

Namespace ECSecurity

    Public Class QueryEncrypt
        Inherits System.Collections.Specialized.NameValueCollection

        Public SplitChar As String = "¾"

        Private mExpireTime As DateTime = Nothing
        Private mHashCode As String

        Public Property ExpireTime() As DateTime
            Get
                Return mExpireTime
            End Get
            Set(ByVal value As DateTime)
                mExpireTime = value
            End Set
        End Property

        Public Overrides Function ToString() As String

            Dim retVal As String = ""

            For Each s As String In Me
                retVal += s & "=" & Me(s) & SplitChar
            Next
            If retVal.EndsWith(SplitChar) Then retVal = retVal.Remove(retVal.Length - 1, 1)

            If mExpireTime <> Nothing Then
                retVal += SplitChar & "queryexpires=" & mExpireTime.Ticks
            End If

            Try
                retVal = EncryptString(retVal, mHashCode)
            Catch ex As Exception
                retVal = ""
            End Try

            Return System.Web.HttpUtility.UrlEncode(retVal)
        End Function

        Public Sub New(ByVal HashCode As String)
            MyBase.New()
            mHashCode = HashCode
        End Sub

        Public Sub New(ByVal EncryptedString As String, ByVal HashCode As String)

            MyBase.New()

            mHashCode = HashCode
            Dim open As String = ""
            Dim expired As Boolean = False
            Try
                open = DecryptString(System.Web.HttpUtility.UrlDecode(EncryptedString), mHashCode)
                Dim arr() As String = open.Split(SplitChar)
                For Each s As String In arr
                    If s.Split("=")(0) = "queryexpires" Then
                        If Now.Ticks - s.Split("=")(1) > 0 Then
                            expired = True
                            Exit For
                        End If
                    Else
                        Me.Add(s.Split("=")(0), s.Split("=")(1))
                    End If
                Next
            Catch ex As Exception

            End Try

            If expired Then Throw New Exception("Querystring has expired")

        End Sub

        Private Shared Function EncryptString(ByVal Message As String, ByVal Passphrase As String) As String
            Dim Results As Byte()
            Dim UTF8 As New System.Text.UTF8Encoding()

            ' Step 1. We hash the pass phrase using MD5 
            ' We use the MD5 hash generator as the result is a 128 bit byte array 
            ' which is a valid length for the TripleDES encoder we use below 

            Dim HashProvider As New MD5CryptoServiceProvider()
            Dim TDESKey As Byte() = HashProvider.ComputeHash(UTF8.GetBytes(Passphrase))

            ' Step 2. Create a new TripleDESCryptoServiceProvider object 
            Dim TDESAlgorithm As New TripleDESCryptoServiceProvider()

            ' Step 3. Setup the encoder 
            TDESAlgorithm.Key = TDESKey
            TDESAlgorithm.Mode = CipherMode.ECB
            TDESAlgorithm.Padding = PaddingMode.PKCS7

            ' Step 4. Convert the input string to a byte[] 
            Dim DataToEncrypt As Byte() = UTF8.GetBytes(Message)

            ' Step 5. Attempt to encrypt the string 
            Try
                Dim Encryptor As ICryptoTransform = TDESAlgorithm.CreateEncryptor()
                Results = Encryptor.TransformFinalBlock(DataToEncrypt, 0, DataToEncrypt.Length)
            Finally
                ' Clear the TripleDes and Hash provider services of any sensitive information 
                TDESAlgorithm.Clear()
                HashProvider.Clear()
            End Try

            ' Step 6. Return the encrypted string as a base64 encoded string 
            Return Convert.ToBase64String(Results)
        End Function

        Private Shared Function DecryptString(ByVal Message As String, ByVal Passphrase As String) As String
            Dim Results As Byte()
            Dim UTF8 As New System.Text.UTF8Encoding()

            ' Step 1. We hash the pass phrase using MD5 
            ' We use the MD5 hash generator as the result is a 128 bit byte array 
            ' which is a valid length for the TripleDES encoder we use below 

            Dim HashProvider As New MD5CryptoServiceProvider()
            Dim TDESKey As Byte() = HashProvider.ComputeHash(UTF8.GetBytes(Passphrase))

            ' Step 2. Create a new TripleDESCryptoServiceProvider object 
            Dim TDESAlgorithm As New TripleDESCryptoServiceProvider()

            ' Step 3. Setup the decoder 
            TDESAlgorithm.Key = TDESKey
            TDESAlgorithm.Mode = CipherMode.ECB
            TDESAlgorithm.Padding = PaddingMode.PKCS7

            ' Step 4. Convert the input string to a byte[] 
            Dim DataToDecrypt As Byte() = Convert.FromBase64String(Message)

            ' Step 5. Attempt to decrypt the string 
            Try
                Dim Decryptor As ICryptoTransform = TDESAlgorithm.CreateDecryptor()
                Results = Decryptor.TransformFinalBlock(DataToDecrypt, 0, DataToDecrypt.Length)
            Finally
                ' Clear the TripleDes and Hash provider services of any sensitive information 
                TDESAlgorithm.Clear()
                HashProvider.Clear()
            End Try

            ' Step 6. Return the decrypted string in UTF8 format 
            Return UTF8.GetString(Results)
        End Function

    End Class

End Namespace


