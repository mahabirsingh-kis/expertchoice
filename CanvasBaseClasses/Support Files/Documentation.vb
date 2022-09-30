Imports System.IO
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Xml

Public Module DocumenationExtensions

    <Extension()>
    Function GetDocumentation(ByVal methodInfo As MethodInfo) As XmlElement
        Dim parametersString As String = ""

        For Each ParamInfo As ParameterInfo In methodInfo.GetParameters()

            If parametersString.Length > 0 Then
                parametersString += ","
            End If

            parametersString += ParamInfo.ParameterType.FullName
        Next

        If parametersString.Length > 0 Then
            Return XmlFromName(methodInfo.DeclaringType, "M"c, methodInfo.Name & "(" & parametersString & ")")
        Else
            Return XmlFromName(methodInfo.DeclaringType, "M"c, methodInfo.Name)
        End If
    End Function

    <Extension()>
    Function GetSummary(ByVal memberInfo As MethodInfo) As String
        Dim element As XmlElement = memberInfo.GetDocumentation()
        Dim summaryElm As XmlNode = element?.SelectSingleNode("summary")
        If summaryElm Is Nothing Then Return ""
        Return summaryElm.InnerText.Trim()
    End Function

    <Extension()>
    Function GetDocumentation(ByVal memberInfo As MemberInfo) As XmlElement
        Return XmlFromName(memberInfo.DeclaringType, memberInfo.MemberType.ToString()(0), memberInfo.Name)
    End Function

    <Extension()>
    Function GetSummary(ByVal memberInfo As MemberInfo) As String
        Dim element As XmlElement = memberInfo.GetDocumentation()
        Dim summaryElm As XmlNode = element?.SelectSingleNode("summary")
        If summaryElm Is Nothing Then Return ""
        Return summaryElm.InnerText.Trim()
    End Function

    <Extension()>
    Function GetDocumentation(ByVal type As Type) As XmlElement
        Return XmlFromName(type, "T"c, "")
    End Function

    <Extension()>
    Function GetSummary(ByVal type As Type) As String
        Dim element As XmlElement = type.GetDocumentation()
        Dim summaryElm As XmlNode = element?.SelectSingleNode("summary")
        If summaryElm Is Nothing Then Return ""
        Return summaryElm.InnerText.Trim()
    End Function

    '<Extension()>
    '''' <summary>
    '''' Get the xmlElement from generated XML documentation for the specific class
    '''' </summary>
    '''' <param name="classFullName">The FULL class name that can be produced as 'GetType([ClassName]).FullName'</param>
    '''' <returns></returns>
    'Function GetDocumentation(ByVal classFullName As String) As XmlElement
    '    Return XmlFromName(classFullName, "F"c, "")
    'End Function

    '<Extension()>
    'Function GetSummary(ByVal classFullName As String) As String
    '    Dim element As XmlElement = Type.GetDocumentation()
    '    Dim summaryElm As XmlNode = element?.SelectSingleNode("summary")
    '    If summaryElm Is Nothing Then Return ""
    '    Return summaryElm.InnerText.Trim()
    'End Function

    <Extension()>
    Private Function XmlFromName(ByVal type As Type, ByVal prefix As Char, ByVal name As String) As XmlElement
        Dim fullName As String

        If String.IsNullOrEmpty(name) Then
            fullName = prefix & ":" & type.FullName
        Else
            fullName = prefix & ":" & type.FullName & "." & name
        End If

        Dim xmlDoc As XmlDocument = XmlFromAssembly(type.Assembly)
        Dim matchedElement As XmlElement = Nothing  ' D7587
        If xmlDoc IsNot Nothing Then matchedElement = TryCast(xmlDoc("doc")("members").SelectSingleNode("member[@name='" & fullName & "']"), XmlElement)    ' D7587
        Return matchedElement
    End Function

    Private Cache As Dictionary(Of Assembly, XmlDocument) = New Dictionary(Of Assembly, XmlDocument)()
    Private FailCache As Dictionary(Of Assembly, Exception) = New Dictionary(Of Assembly, Exception)()

    <Extension()>
    Function XmlFromAssembly(ByVal assembly As Assembly) As XmlDocument
        If Not FailCache.ContainsKey(assembly) Then ' D7587
            Try
                If Not Cache.ContainsKey(assembly) Then
                    Cache(assembly) = XmlFromAssemblyNonCached(assembly)
                End If
                Return Cache(assembly)
            Catch exception As Exception
                FailCache(assembly) = exception
                'Throw exception    ' -D7587
            End Try
        End If
        Return Nothing  ' D7587
    End Function

    Private Function XmlFromAssemblyNonCached(ByVal assembly As Assembly) As XmlDocument
        Dim assemblyFilename As String = assembly.CodeBase
        Const prefix As String = "file:///"

        If assemblyFilename.StartsWith(prefix) Then
            Dim streamReader As StreamReader
            Try
                streamReader = New StreamReader(Path.ChangeExtension(assemblyFilename.Substring(prefix.Length), ".xml"))
            Catch exception As FileNotFoundException
                Throw New Exception("XML documentation not present (make sure it is turned on in project properties when building)", exception)
            End Try
            Dim xmlDocument As New XmlDocument()
            xmlDocument.Load(streamReader)
            Return xmlDocument
        Else
            Throw New Exception("Could not ascertain assembly filename", Nothing)
        End If
    End Function

End Module