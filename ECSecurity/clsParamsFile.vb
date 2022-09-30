Imports System.IO
Imports System.Security.Cryptography
Imports Org.BouncyCastle.Crypto.Digests

Namespace ECSecurity

#Const _USE_DOTNED_SHA512 = False   ' D4823

    <Serializable()> Public Class clsRestrictionParameter
        Private mID As ecLicenseParameter   ' D0913
        Private mValue As Long

        Public Property ID() As ecLicenseParameter  ' D0913
            Get
                Return mID
            End Get
            Set(ByVal value As ecLicenseParameter)  ' D0913
                mID = value
            End Set
        End Property

        Public Property Value() As Long
            Get
                Return mValue
            End Get
            Set(ByVal value As Long)
                mValue = value
            End Set
        End Property

        Public Sub New(ByVal ID As ecLicenseParameter, ByVal Value As Long) ' D0913
            mID = ID
            mValue = Value
        End Sub
    End Class

    <Serializable()> Public Class clsParamsFile
        'Private mParametersCount As Long   ' -D3046

        Private mParams As New ArrayList

        ' -D3046
        'Public ReadOnly Property ParametersCount() As Long
        '    Get
        '        Return mParametersCount
        '    End Get
        'End Property

        Public Overloads Function Read(ByVal Stream As MemoryStream, ByVal SerialNumber As String) As Boolean 'C0151
            Dim memStream As MemoryStream = Nothing

            Dim reader As BinaryReader = Nothing
            Dim count As Long

            Dim resValue As Boolean = True

            Try
                ' READING CONTENT OF PARAMETERS

                Dim paramsBytes As Byte()
                Dim paramsBytesCount As Integer

                ' creating empty buffer for parameters bytes
                paramsBytes = New Byte(CType(Stream.Length - 64 - 1, Integer)) {}

                ' reading parameters bytes to paramsBytes array
                paramsBytesCount = Stream.Read(paramsBytes, 0, Stream.Length - 64)

                ' DECRYPTING PARAMETER BYTES
                Dim decryptedBytes As Byte()
                decryptedBytes = Decrypt(paramsBytes, SerialNumber)
                memStream = New MemoryStream(decryptedBytes)
                reader = New BinaryReader(memStream)
                reader.BaseStream.Position = 0
                count = reader.ReadInt64 ' reading the number of parameters
                memStream.SetLength(8 + count * 8 * 2)

                '-------------

                ' VALIDATING HASH

                Dim hashBytes As Byte()
                Dim hashBytesCount As Integer

                ' creating empty buffer for 512 bytes for SHA512 hash value from the end of the file
                hashBytes = New Byte(CType(63, Integer)) {}
                ' reading hash bytes to hashBytes array
                hashBytesCount = Stream.Read(hashBytes, 0, 64)

                If hashBytesCount <> 64 Then
                    'If Not Stream Is Nothing Then
                    '    Stream.Close()
                    'End If

                    If Not memStream Is Nothing Then
                        memStream.Close()
                    End If
                    Return False
                End If

                Dim hash As Byte()
                memStream.Position = 0

                ' calculate hash
#If _USE_DOTNED_SHA512 Then
                Dim hashAlg As New SHA512Managed
                hash = hashAlg.ComputeHash(memStream)
#Else
                ' D4823 ===
                Dim sha As New SHA512Cng()
                hash = sha.ComputeHash(memStream)
                ' D4823 ==
#End If

                ' Comparing hash at the end of the file and calculated hash byte by byte
                For i As Integer = 0 To hashBytesCount - 1
                    If hash(i) <> hashBytes(i) Then
                        'If Not Stream Is Nothing Then
                        '    Stream.Close()
                        'End If

                        If Not memStream Is Nothing Then
                            memStream.Close()
                        End If
                        Return False
                    End If
                Next

                '-------------

                Dim ParamID As ecLicenseParameter   ' D0913

                memStream.Position = 0
                reader.ReadInt64()

                For i As Long = 1 To count
                    ParamID = CType(reader.ReadInt64, ecLicenseParameter)   ' D0913

                    Select Case ParamID
                        Case ecLicenseParameter.MaxProjectCreatorsInWorkgroup, ecLicenseParameter.MaxPMsInProject,
                            ecLicenseParameter.MaxProjectsTotal,
                            ecLicenseParameter.ExpirationDate, ecLicenseParameter.MaxWorkgroupsTotal,
                            ecLicenseParameter.TeamTimeEnabled, ecLicenseParameter.MaxProjectsOnline,
                            ecLicenseParameter.SpyronEnabled, ecLicenseParameter.ResourceAlignerEnabled,
                            ecLicenseParameter.ExportEnabled, ecLicenseParameter.CommercialUseEnabled,
                            ecLicenseParameter.MaxLifetimeProjects, ecLicenseParameter.MaxObjectives,
                            ecLicenseParameter.MaxLevelsBelowGoal, ecLicenseParameter.MaxAlternatives,
                            ecLicenseParameter.MaxUsersInProject,
                            ecLicenseParameter.MaxUsersInWorkgroup, ecLicenseParameter.RiskEnabled,
                            ecLicenseParameter.RiskTreatments, ecLicenseParameter.RiskTreatmentsOptimization,
                            ecLicenseParameter.AllowUseGurobi, ecLicenseParameter.InstanceID,
                            ecLicenseParameter.isSelfHost, ecLicenseParameter.CreatedAt   ' D0917 + D1483 + D2056 + D3585 + D3922 + D3965

                            SetParameter(ParamID, reader.ReadInt64)
                        Case Else
                            reader.ReadInt64()
                    End Select
                Next
            Catch ex As Exception
                resValue = False
            Finally
                If Not reader Is Nothing Then
                    reader.Close()
                End If

                'If Not Stream Is Nothing Then
                '    Stream.Close()
                'End If

                If Not memStream Is Nothing Then
                    memStream.Close()
                End If
            End Try

            Return resValue
        End Function

        ' D3046 ===
        Public ReadOnly Property Parameters As ArrayList
            Get
                Return mParams
            End Get
        End Property
        ' D3046 ==

        Public Overloads Function Read(ByVal FilePath As String, ByVal SerialNumber As String) As Boolean
            If Not File.Exists(FilePath) Then
                Return False
            End If

            Dim fs As FileStream = Nothing
            Dim memStream As MemoryStream = Nothing

            Dim reader As BinaryReader = Nothing
            Dim count As Long

            Dim resValue As Boolean = True

            Try
                fs = New IO.FileStream(FilePath, FileMode.Open, FileAccess.Read)

                ' READING CONTENT OF PARAMETERS

                Dim paramsBytes As Byte()
                Dim paramsBytesCount As Integer

                ' creating empty buffer for parameters bytes
                paramsBytes = New Byte(CType(fs.Length - 64 - 1, Integer)) {}

                ' reading parameters bytes to paramsBytes array
                paramsBytesCount = fs.Read(paramsBytes, 0, fs.Length - 64)


                ' DECRYPTING PARAMETER BYTES
                Dim decryptedBytes As Byte()
                decryptedBytes = Decrypt(paramsBytes, SerialNumber)
                memStream = New MemoryStream(decryptedBytes)
                reader = New BinaryReader(memStream)
                reader.BaseStream.Position = 0
                count = reader.ReadInt64 ' reading the number of parameters
                memStream.SetLength(8 + count * 8 * 2)

                '-------------

                ' VALIDATING HASH

                Dim hashBytes As Byte()
                Dim hashBytesCount As Integer

                ' creating empty buffer for 512 bytes for SHA512 hash value from the end of the file
                hashBytes = New Byte(CType(63, Integer)) {}
                ' reading hash bytes to hashBytes array
                hashBytesCount = fs.Read(hashBytes, 0, 64)

                If hashBytesCount <> 64 Then
                    If Not fs Is Nothing Then
                        fs.Close()
                    End If

                    If Not memStream Is Nothing Then
                        memStream.Close()
                    End If
                    Return False
                End If

                Dim hash As Byte()
                memStream.Position = 0

                ' calculate hash
#If _USE_DOTNED_SHA512 Then
                Dim hashAlg As New SHA512Managed
                hash = hashAlg.ComputeHash(memStream)
#Else
                ' D4823 ===
                Dim sha As New SHA512Cng()
                hash = sha.ComputeHash(memStream)
                ' D4823 ==
#End If

                ' Comparing hash at the end of the file and calculated hash byte by byte
                For i As Integer = 0 To hashBytesCount - 1
                    If hash(i) <> hashBytes(i) Then
                        If Not fs Is Nothing Then
                            fs.Close()
                        End If

                        If Not memStream Is Nothing Then
                            memStream.Close()
                        End If
                        Return False
                    End If
                Next

                '-------------

                Dim ParamID As ecLicenseParameter   ' D0913

                memStream.Position = 0
                reader.ReadInt64()

                For i As Long = 1 To count
                    ParamID = CType(reader.ReadInt64, ecLicenseParameter)   ' D0913

                    Select Case ParamID
                        Case ecLicenseParameter.MaxProjectCreatorsInWorkgroup, ecLicenseParameter.MaxPMsInProject,
                            ecLicenseParameter.MaxProjectsTotal,
                            ecLicenseParameter.ExpirationDate, ecLicenseParameter.MaxWorkgroupsTotal,
                            ecLicenseParameter.TeamTimeEnabled, ecLicenseParameter.MaxProjectsOnline,
                            ecLicenseParameter.SpyronEnabled, ecLicenseParameter.ResourceAlignerEnabled,
                            ecLicenseParameter.ExportEnabled, ecLicenseParameter.CommercialUseEnabled,
                            ecLicenseParameter.MaxLifetimeProjects, ecLicenseParameter.MaxObjectives,
                            ecLicenseParameter.MaxLevelsBelowGoal, ecLicenseParameter.MaxAlternatives,
                            ecLicenseParameter.MaxUsersInProject,
                            ecLicenseParameter.MaxUsersInWorkgroup, ecLicenseParameter.RiskEnabled,
                            ecLicenseParameter.RiskTreatments, ecLicenseParameter.RiskTreatmentsOptimization,
                            ecLicenseParameter.AllowUseGurobi, ecLicenseParameter.InstanceID,
                            ecLicenseParameter.isSelfHost, ecLicenseParameter.CreatedAt ' D0913 + D0917 + D1483 + D0256 + D3585 + D3922 + D3946 + D3965

                            SetParameter(ParamID, reader.ReadInt64)

                        Case Else
                            reader.ReadInt64()
                    End Select
                Next
            Catch ex As Exception
                resValue = False
            Finally
                If Not reader Is Nothing Then
                    reader.Close()
                End If

                If Not fs Is Nothing Then
                    fs.Close()
                End If

                If Not memStream Is Nothing Then
                    memStream.Close()
                End If
            End Try

            Return resValue
        End Function

        Public Overloads Function Write(ByVal Stream As MemoryStream, ByVal SerialNumber As String) As Boolean 'C0151
            Dim writer As BinaryWriter = Nothing

            Dim resValue As Boolean = True
            Dim hash As Byte()

            Try
                writer = New BinaryWriter(Stream)

                ' write the number of parameters
                writer.Write(CLng(mParams.Count))

                For Each param As clsRestrictionParameter In mParams
                    writer.Write(CLng(param.ID))
                    writer.Write(CLng(param.Value))
                Next

                ' calculate hash for the file
                Stream.Seek(0, SeekOrigin.Begin)

#If _USE_DOTNED_SHA512 Then
                Dim hashAlg As New SHA512Managed
                hash = hashAlg.ComputeHash(Stream)
#Else
                ' D4823 ===
                Dim sha As New SHA512Cng()
                hash = sha.ComputeHash(Stream)
                ' D4823 ==
#End If

                ' encrypt the stream
                Dim PlainTextBytes As Byte() = New Byte(Stream.Length - 1) {}
                Dim CipherBytes As Byte()

                Stream.Seek(0, SeekOrigin.Begin)
                Stream.Read(PlainTextBytes, 0, Stream.Length)
                CipherBytes = Encrypt(PlainTextBytes, SerialNumber)

                ' write encrypted bytes (override existing plain text bytes with encrypted)
                Stream.Seek(0, SeekOrigin.Begin)
                writer.Write(CipherBytes)

                ' put the hash to the end of the file
                writer.Write(hash)

            Catch ex As Exception
                resValue = False
            Finally
                If Not writer Is Nothing Then
                    writer.Close()
                End If

            End Try

            Return resValue
        End Function

        Public Overloads Function Write(ByVal FilePath As String, ByVal SerialNumber As String) As Boolean
            Dim fs As IO.FileStream = Nothing
            Dim writer As BinaryWriter = Nothing

            Dim resValue As Boolean = True
            Dim hash As Byte()

            Try
                fs = New IO.FileStream(FilePath, FileMode.Create, FileAccess.ReadWrite)
                writer = New BinaryWriter(fs)

                ' write the number of parameters
                writer.Write(CLng(mParams.Count))

                For Each param As clsRestrictionParameter In mParams
                    writer.Write(CLng(param.ID))
                    writer.Write(CLng(param.Value))
                Next

                ' calculate hash for the file
                fs.Seek(0, SeekOrigin.Begin)
#If _USE_DOTNED_SHA512 Then
                Dim hashAlg As New SHA512Managed
                hash = hashAlg.ComputeHash(fs)
#Else
                ' D4823 ===
                Dim sha As New SHA512Cng()
                hash = sha.ComputeHash(fs)
                ' D4823 ==
#End If

                ' encrypt the stream
                Dim PlainTextBytes As Byte() = New Byte(fs.Length - 1) {}
                Dim CipherBytes As Byte()

                fs.Seek(0, SeekOrigin.Begin)
                fs.Read(PlainTextBytes, 0, fs.Length)
                CipherBytes = Encrypt(PlainTextBytes, SerialNumber)

                ' write encrypted bytes (override existing plain text bytes with encrypted)
                fs.Seek(0, SeekOrigin.Begin)
                writer.Write(CipherBytes)

                ' put the hash to the end of the file
                writer.Write(hash)

            Catch ex As Exception
                resValue = False
            Finally
                If Not writer Is Nothing Then
                    writer.Close()
                End If

                If Not fs Is Nothing Then
                    fs.Close()
                End If

            End Try

            Return resValue
        End Function

        ' D0909 ===
        Public Function IsParameterExists(ByVal ID As ecLicenseParameter) As Boolean    ' D3046
            For Each param As clsRestrictionParameter In mParams
                If param.ID = ID Then Return True
            Next
            Return False
        End Function
        ' D0909 ==

        Public Function GetParameter(ByVal tID As ecLicenseParameter) As clsRestrictionParameter
            For Each param As clsRestrictionParameter In mParams
                If param.ID = tID Then
                    Return param
                End If
            Next
            Return Nothing
        End Function

        Public Function SetParameter(ByVal tID As ecLicenseParameter, ByVal Value As Long) As Boolean   ' D0913
            For Each param As clsRestrictionParameter In mParams
                If param.ID = tID Then
                    param.Value = Value
                    Return True
                End If
            Next
            Return False
        End Function

        Public Sub New()
            Dim param As clsRestrictionParameter

            param = New clsRestrictionParameter(ecLicenseParameter.MaxProjectCreatorsInWorkgroup, UNLIMITED_VALUE)   ' D0913
            mParams.Add(param)

            param = New clsRestrictionParameter(ecLicenseParameter.MaxPMsInProject, UNLIMITED_VALUE)   ' D0913
            mParams.Add(param)

            ' -D2548
            'param = New clsRestrictionParameter(ecLicenseParameter.MaxEvaluatorsInModel, UNLIMITED_VALUE)   ' D0913
            'mParams.Add(param)

            'param = New clsRestrictionParameter(ecLicenseParameter.MaxModelsPerOwner, UNLIMITED_VALUE)   ' D0913
            'mParams.Add(param)

            param = New clsRestrictionParameter(ecLicenseParameter.MaxProjectsTotal, UNLIMITED_VALUE)   ' D0913
            mParams.Add(param)

            ' -D2548
            'param = New clsRestrictionParameter(ecLicenseParameter.MaxConcurrentEvaluatorsInModel, UNLIMITED_VALUE)   ' D0913
            'mParams.Add(param)

            param = New clsRestrictionParameter(ecLicenseParameter.ExpirationDate, UNLIMITED_DATE)   ' D0913
            mParams.Add(param)

            'C0153===
            param = New clsRestrictionParameter(ecLicenseParameter.MaxWorkgroupsTotal, UNLIMITED_VALUE)   ' D0913 + D3046
            mParams.Add(param)

            param = New clsRestrictionParameter(ecLicenseParameter.TeamTimeEnabled, 1)   ' D0913
            mParams.Add(param)
            'C0153==

            'C0155===
            param = New clsRestrictionParameter(ecLicenseParameter.MaxProjectsOnline, UNLIMITED_VALUE)   '  D0417 + D0913
            mParams.Add(param)
            'C0155==

            ' D0285 ===
            param = New clsRestrictionParameter(ecLicenseParameter.SpyronEnabled, 1)   ' D0913
            mParams.Add(param)
            ' D0285 ==

            'C0712===
            param = New clsRestrictionParameter(ecLicenseParameter.ResourceAlignerEnabled, 0)   ' D0913
            mParams.Add(param)
            'C0712==

            'D0909 ===
            param = New clsRestrictionParameter(ecLicenseParameter.ExportEnabled, 1)   ' D0913
            mParams.Add(param)

            param = New clsRestrictionParameter(ecLicenseParameter.MaxLifetimeProjects, UNLIMITED_VALUE)   ' D0913
            mParams.Add(param)

            param = New clsRestrictionParameter(ecLicenseParameter.CommercialUseEnabled, 1)   ' D0913 + D0917
            mParams.Add(param)

            param = New clsRestrictionParameter(ecLicenseParameter.MaxObjectives, UNLIMITED_VALUE)   ' D0913
            mParams.Add(param)

            param = New clsRestrictionParameter(ecLicenseParameter.MaxLevelsBelowGoal, UNLIMITED_VALUE)   ' D0913
            mParams.Add(param)

            param = New clsRestrictionParameter(ecLicenseParameter.MaxAlternatives, UNLIMITED_VALUE)   ' D0913
            mParams.Add(param)

            ' -D2548
            'param = New clsRestrictionParameter(ecLicenseParameter.MaxViewOnlyUsers, UNLIMITED_VALUE)   ' D0913
            'mParams.Add(param)
            'D0909 ==

            ' D1483 ===
            param = New clsRestrictionParameter(ecLicenseParameter.MaxUsersInWorkgroup, UNLIMITED_VALUE)
            mParams.Add(param)

            param = New clsRestrictionParameter(ecLicenseParameter.MaxUsersInProject, UNLIMITED_VALUE)
            mParams.Add(param)
            ' D1483 ==

            param = New clsRestrictionParameter(ecLicenseParameter.RiskEnabled, 0)     ' D2056 
            mParams.Add(param)  ' D2056

            ' D3585 ===
            param = New clsRestrictionParameter(ecLicenseParameter.RiskTreatments, 1)
            mParams.Add(param)

            param = New clsRestrictionParameter(ecLicenseParameter.RiskTreatmentsOptimization, 1)
            mParams.Add(param)
            ' D3585 ==

            param = New clsRestrictionParameter(ecLicenseParameter.AllowUseGurobi, 0)   ' D3922
            mParams.Add(param)  ' D3922

            param = New clsRestrictionParameter(ecLicenseParameter.InstanceID, 0)   ' D3946
            mParams.Add(param)  ' D3946

            ' D3965 ===
            param = New clsRestrictionParameter(ecLicenseParameter.isSelfHost, 0)
            mParams.Add(param)

            param = New clsRestrictionParameter(ecLicenseParameter.CreatedAt, 0)
            mParams.Add(param)
            ' D3965 ==

        End Sub

    End Class

End Namespace