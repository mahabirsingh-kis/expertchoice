Imports Microsoft.VisualBasic
Imports System.Xml

<Serializable()> Public Class clsResourceParameter
    Private mName As String = ""
    Private mValue As String = ""
    Private mComment As String = "" ' D0798

    Public Property Name() As String
        Get
            Return mName
        End Get
        Set(ByVal value As String)
            mName = value
        End Set
    End Property

    Public Property Value() As String
        Get
            Return mValue
        End Get
        Set(ByVal value As String)
            mValue = value
        End Set
    End Property

    ' D0798 ===
    Public Property Comment() As String
        Get
            Return mComment
        End Get
        Set(ByVal value As String)
            mComment = value
        End Set
    End Property
    ' D0798 ==

    Public Sub New(ByVal Name As String, ByVal Value As String, Optional ByVal sComment As String = Nothing)    ' D0798
        mName = Name
        mValue = Value
        If sComment IsNot Nothing Then mComment = sComment ' D0798
    End Sub

End Class

<Serializable()> Public Class clsResourceReader
    Dim mParams As Dictionary(Of String, clsResourceParameter)

    Public Function ReadResourceFile(ByVal FilePath As String) As Boolean
        mParams = Nothing
        If Not My.Computer.FileSystem.FileExists(FilePath) Then
            Return False
        End If

        mParams = New Dictionary(Of String, clsResourceParameter)

        Dim reader As XmlTextReader = Nothing

        Dim dataName As String
        Dim dataValue As String
        Dim dataComment As String   ' D0798

        Try
            reader = New XmlTextReader(FilePath)
            reader.WhitespaceHandling = WhitespaceHandling.None
            While reader.Read()
                If (reader.NodeType = XmlNodeType.Element) And (reader.Name = "data") Then
                    dataName = ""
                    dataValue = ""
                    dataComment = ""   ' D0798

                    dataName = reader.GetAttribute("name")

                    If reader.Read() Then
                        If (reader.NodeType = XmlNodeType.Element) And (reader.Name = "value") Then
                            dataValue = reader.ReadElementContentAsString()
                        End If
                        ' D0798 ===
                        If (reader.NodeType = XmlNodeType.Element) And (reader.Name = "comment") Then
                            dataComment = reader.ReadElementContentAsString()
                        End If
                        ' D0798 ==
                    End If

                    mParams.Add(dataName.ToLower, New clsResourceParameter(dataName, dataValue, dataComment))    ' D0798
                End If
            End While
        Finally
            If Not (reader Is Nothing) Then
                reader.Close()
            End If
        End Try

        Return True
    End Function

    Public Sub ClearParameterList()
        mParams = Nothing
    End Sub

    Public ReadOnly Property Parameters() As Dictionary(Of String, clsResourceParameter)
        Get
            Return mParams
        End Get
    End Property

    Public Function ParameterExists(ByVal Name As String) As Boolean
        Return mParams.ContainsKey(Name.ToLower)
        'If mParams Is Nothing Or Name Is Nothing Then
        '    Return False
        'End If

        'For Each param As clsResourceParameter In mParams
        '    If param.Name.ToLower = Name.ToLower Then
        '        Return True
        '    End If
        'Next
        'Return False
    End Function

    'Public Function ValueByName(ByVal Name As String) As String
    '    If mParams Is Nothing Or Name Is Nothing Then
    '        Return ""
    '    End If

    '    For Each param As clsResourceParameter In mParams
    '        If param.Name.ToLower = Name.ToLower Then
    '            Return param.Value
    '        End If
    '    Next

    '    Return ""
    'End Function

    Public Function ParameterByName(ByVal Name As String) As clsResourceParameter
        Return If(mParams.ContainsKey(Name.ToLower), mParams(Name.ToLower), Nothing)
        'If mParams Is Nothing Or Name Is Nothing Then
        '    Return Nothing
        'End If
        '
        'For Each param As clsResourceParameter In mParams
        '    If param.Name.ToLower = Name.ToLower Then
        '        Return param
        '    End If
        'Next
        '
        'Return Nothing
    End Function

    Public Sub AddParameter(ByVal Param As clsResourceParameter)
        If mParams Is Nothing Then
            mParams = New Dictionary(Of String, clsResourceParameter)
        End If

        mParams.Add(param.Name.ToLower, Param)
    End Sub

End Class