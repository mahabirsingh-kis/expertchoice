Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Xml
Imports Microsoft.OpenApi
Imports Microsoft.OpenApi.Models

Public Class OpenAPIWebAPI
    Inherits clsComparionCorePage

#Const _OPT_USE_MS_OPENAPI = True

    Private _FILE_CONFIG As String = _FILE_API + "openapi.cfg"
    Private _FILE_OPENAPI_TPL As String = _FILE_API + "openapi_template.yaml"
    Private _FILE_OPENAPI_OUTPUT_YAML As String = _FILE_API + _FILE_OPEN_API_YAML
    Private _FILE_OPENAPI_OUTPUT_JSON As String = _FILE_API + _FILE_OPEN_API_JSON

    Private _OPT_SHOW_OWN As Boolean = True
    'Private _OPT_SHOW_ENUM_AS_INT As Boolean = True

    Private _OPT_IGNORE_TYPES As String() = {"clsComparionCore", "clsComparionCorePage"}    ' D7588

#If Not _OPT_USE_MS_OPENAPI Then
    Private indent1 As String = "  "
    Private indent2 As String = "    "
    Private indent3 As String = "       "
    Private indent4 As String = "         "
    Private indent5 As String = "           "
    Private indent6 As String = "             "
#End If

    Public Sub New()
        MyBase.New(_PGID_WEBAPI)
    End Sub

    Private Function _Page() As mpWebAPI
        Return CType(Master, mpWebAPI)
    End Function

    'Private Sub ScanAPIFolders(sPath As String, ByRef Lst As Dictionary(Of String, Type))
    '    If String.IsNullOrEmpty(sPath) Then sPath = _FILE_API
    '    If Lst IsNot Nothing AndAlso MyComputer.FileSystem.DirectoryExists(sPath) Then
    '        Dim files As ReadOnlyCollection(Of String) = MyComputer.FileSystem.GetFiles(sPath, FileIO.SearchOption.SearchAllSubDirectories, "*.aspx")
    '        For Each file As String In files
    '            Dim sContent As String = File_GetContent(file)
    '            If Not String.IsNullOrEmpty(sContent) Then
    '                If sContent.Length > 1000 Then sContent = sContent.Substring(0, 1000)
    '                sContent = sContent.Trim
    '                Dim m As MatchCollection = System.Text.RegularExpressions.Regex.Matches(sContent, "(\s)inherits=([^\s])(\s)", RegexOptions.IgnoreCase)
    '            End If
    '        Next
    '    End If
    'End Sub

    Private Function getTypesForScan() As Dictionary(Of String, Type)
        Dim Res As New Dictionary(Of String, Type)
        Dim sLines As String() = File_GetContent(_FILE_CONFIG, "").Trim.Replace(vbCr, "").Split(CChar(vbLf))
        For Each sRow As String In sLines
            sRow = sRow.Trim
            If sRow.Length > 0 Then
                If sRow(0) <> "-" Then
                    Dim sIdx As Integer = sRow.IndexOf(" ")
                    If (sIdx < 0) Then sIdx = sRow.IndexOf(vbTab)
                    If sIdx > 0 Then
                        Dim sPath As String = sRow.Substring(0, sIdx).Trim
                        Try
                            Dim T As Type = Type.GetType(sRow.Substring(sIdx + 1).Trim)
                            If T IsNot Nothing Then Res.Add(sPath, T)
                        Catch ex As Exception
                        End Try
                    End If
                End If
            End If
        Next
        Return Res
    End Function

#If _OPT_USE_MS_OPENAPI Then
    Private Function GetSchema(t As Type, ByRef schemas As IDictionary(Of String, OpenApiSchema)) As OpenApiSchema
        Dim Res As New OpenApiSchema
        If t IsNot Nothing Then
            Dim sName As String = t.Name
            Select Case sName.ToLower
                Case "byte", "short", "int32", "uint32"
                    Res.Type = "integer"
                    Res.Format = "int32"
                Case "int64", "long"
                    Res.Type = "int64"
                    Res.Format = sName
                Case "double", "float"
                    Res.Type = "number"
                    Res.Format = sName.ToLower
                Case "string"
                    Res.Type = "string"
                Case "boolean"
                    Res.Type = "boolean"
                Case "date", "datetime", "time"
                    Res.Type = "string"
                    Res.Format = "date-time"
                Case "object"
                    Res.Type = "object"
                Case "httppostedfile"
                    Res.Type = "string"
                    Res.Format = "binary"
                Case Else
                    If schemas IsNot Nothing Then
                        If Not _OPT_IGNORE_TYPES.Contains(sName) Then   ' D7588

                            If sName.StartsWith("List") AndAlso t.GenericTypeArguments IsNot Nothing AndAlso t.GenericTypeArguments.Length > 0 Then
                                Res.Type = "array"
                                Res.Items = GetSchema(t.GenericTypeArguments(0), schemas)
                                Res.Description = String.Format("List({0})", t.GenericTypeArguments(0).Name)
                                'Res.Format = t.GenericTypeArguments(0).Name.ToLower
                                'Res.AdditionalProperties = GetSchema(t.GenericTypeArguments(0), schemas)
                            End If

                            If sName.StartsWith("NameValueCollection") OrElse sName.StartsWith("Dictionary") Then
                                Res.Type = "array"
                                Res.Items = New OpenApiSchema
                                Res.Description = sName
                            End If

                            If Not schemas.Keys.Contains(sName) AndAlso isCustomObject(t) Then
                                Dim DefObject As Object = If(t.IsValueType, Activator.CreateInstance(t), Nothing)
                                Dim NewSchema As New OpenApiSchema With {
                                .Description = t.GetSummary()
                            }
                                If t.IsEnum Then
                                    NewSchema.Type = "integer"
                                    NewSchema.Enum = New Any.OpenApiArray
                                Else
                                    NewSchema.Type = "object"
                                    NewSchema.Properties = New Dictionary(Of String, OpenApiSchema)
                                End If
                                For Each fld As Reflection.FieldInfo In t.GetFields
                                    If t.IsEnum Then
                                        If Not fld.Name.EndsWith("__") Then
                                            Dim val As Object = ([Enum].Parse(t, fld.Name))
                                            NewSchema.Enum.Add(New Any.OpenApiString(String.Format("{1} ({0})", CInt(val), fld.Name)))
                                        End If
                                    Else
                                        Dim s As OpenApiSchema = GetSchema(fld.FieldType, schemas)
                                        If s IsNot Nothing Then
                                            If DefObject IsNot Nothing Then
                                                Dim def_val As Object = fld.GetValue(DefObject)
                                                If def_val Is Nothing Then
                                                    def_val = "null"
                                                Else
                                                    If fld.FieldType.IsEnum Then def_val = CInt(def_val)
                                                    If def_val.ToString <> "" Then s.Default = New Any.OpenApiString(def_val.ToString)
                                                    's.Description = fld.FieldType.GetSummary() // crashing
                                                End If
                                            End If
                                            NewSchema.Properties.Add(fld.Name, s)
                                        End If
                                    End If
                                Next
                                schemas.Add(sName, NewSchema)
                            End If
                            If schemas.Keys.Contains(sName) Then
                                Res.Reference = New OpenApiReference With {
                                .Type = ReferenceType.Schema,
                                .Id = sName
                            }
                            Else
                                If Res.Type Is Nothing Then Res.Type = sName
                            End If
                        End If
                    End If ' D7588
            End Select
            'If Res.Description = "" Then Res.Description = t.GetSummary()  // crashing
        End If
        Return Res
    End Function

    Private Function GetXMLDescription(Section As XmlNode, Name As String, Types As Dictionary(Of String, Type), Optional isParam As Boolean = False, Optional ByRef [XMLNode] As XmlNode = Nothing) As String
        Dim sRes As String = ""
        If Section IsNot Nothing AndAlso Section.HasChildNodes AndAlso Not String.IsNullOrEmpty(Name) Then
            Dim node As XmlNode = Section.SelectSingleNode(String.Format(If(isParam, ".//param [@name=""{0}""]", ".//{0}"), Name))
            If node IsNot Nothing Then
                sRes = ""
                If node.ChildNodes IsNot Nothing AndAlso node.ChildNodes.Count > 0 Then
                    For Each child As XmlNode In node.ChildNodes
                        If child.Value <> "" AndAlso child.Name <> "remark" Then sRes += child.Value
                        If Not String.IsNullOrEmpty(child.Name) Then
                            Select Case child.Name.ToLower
                                Case "seealso", "see"
                                    If child.Attributes IsNot Nothing AndAlso child.Attributes.Count > 0 AndAlso child.Attributes(0).Name.ToLower = "cref" Then
                                        Dim sName As String = child.Attributes(0).Value
                                        Dim sPath As String = ""
                                        Dim isMethod As Boolean = sName.StartsWith("M:")
                                        Dim idx As Integer = sName.IndexOf(":")
                                        If idx > 0 Then sName = sName.Substring(idx + 1)
                                        idx = sName.LastIndexOf(".")
                                        If idx > 0 Then
                                            Dim sClass As String = sName.Substring(0, idx)
                                            sName = sName.Substring(idx + 1)
                                            If isMethod AndAlso Types IsNot Nothing Then
                                                For Each sKey As String In Types.Keys
                                                    If Types(sKey).ToString.ToLower = sClass.ToLower Then
                                                        sPath = String.Format("#/{0}/get_{1}__action_{2}", sKey.Replace("/", " "), sKey.Replace("/", "_"), sName)
                                                    End If
                                                Next
                                            End If
                                        End If
                                        sRes += If(sPath = "", sName, String.Format("[{1}]({0})", sPath, sName))
                                    End If
                            End Select
                        End If
                    Next
                Else
                    sRes = node.InnerText
                End If
                [XMLNode] = node
            End If
        End If
        Return sRes
    End Function

#Else
    Private Function getTypeName(t As Type) As String
        Dim sRes As String = t.Name.ToLower
        Select Case t.Name
            Case "Int32", "UInt32", "Int64"
                sRes = "integer"
            Case "Double", "Float"
                sRes = "number"
        End Select
        Return sRes
    End Function

    Private Function SafeStr(Str As String) As String
        If String.IsNullOrEmpty(Str) Then Return "" Else Return Str.Replace("'", "\'")
    End Function

#End If

    Private Function isCustomObject(t As Type) As Boolean
        If t IsNot Nothing Then
            Dim sName As String = t.Name
            Return (t.IsEnum OrElse (t.IsClass AndAlso (sName.StartsWith("j") OrElse sName.StartsWith("cls") OrElse sName.StartsWith("ec"))))
        End If
        Return False
    End Function

#If Not _OPT_USE_MS_OPENAPI Then
    Private Sub addSchema(schema As Type, ByRef Lst As List(Of Type), ByRef schemasList As String)
        If schema IsNot Nothing AndAlso Lst IsNot Nothing AndAlso schemasList IsNot Nothing AndAlso Not Lst.Contains(schema) Then
            If isCustomObject(schema) Then
                Dim sRes As String = String.Format(indent2 + "{0}:" + vbCrLf + indent3 + "type: {1}" + vbCrLf + indent3 + "{2}:" + vbCrLf, schema.Name, If(schema.IsEnum, "integer", "object"), If(schema.IsEnum, "enum", "properties"))
                For Each fld As Reflection.FieldInfo In schema.GetFields
                    If schema.IsEnum Then
                        If Not fld.Name.EndsWith("__") Then
                            Dim val As Object = ([Enum].Parse(schema, fld.Name))
                            'sRes += String.Format(If(_OPT_SHOW_ENUM_AS_INT, "#" + indent4 + "{1}:" + vbCrLf, "") + indent4 + "- '{0}'" + vbCrLf, If(_OPT_SHOW_ENUM_AS_INT, CInt(val), val), fld.Name)
                            sRes += String.Format(indent4 + "- '{1} ({0})'" + vbCrLf, CInt(val), fld.Name)
                        End If
                    Else
                        Dim sType As String = ""
                        Dim sName As String = fld.FieldType.Name
                        If isCustomObject(fld.FieldType) Then
                            sType = String.Format("object" + vbCrLf + indent5 + "$ref: '#/components/schemas/{0}'", sName)
                            addSchema(fld.FieldType, Lst, schemasList)
                        Else
                            sType = getTypeName(fld.FieldType)
                        End If
                        sRes += String.Format(indent4 + "{0}:" + vbCrLf + indent5 + "type: {1}" + vbCrLf, fld.Name, sType)
                    End If
                Next
                schemasList += sRes
            End If
            Lst.Add(schema)
        End If
    End Sub
#End If

    Private Sub GenerateOpenAPI(ByRef sError As String)

        Dim sTpl = File_GetContent(_FILE_OPENAPI_TPL, sError)
        If String.IsNullOrEmpty(sError) AndAlso Not String.IsNullOrEmpty(sTpl) Then ' D7587

            Dim sSchemas As String = ""
            'Dim sParameters As String = ""
            'Dim sResponses As String = ""
            Dim sTags As String = ""
            Dim sPaths As String = ""

            Dim ListAll As Dictionary(Of String, Type) = getTypesForScan()
            Dim usedSchemas As New List(Of Type)

#If Not _OPT_USE_MS_OPENAPI Then

            For Each sPath As String In ListAll.Keys
                Dim t As Type = ListAll(sPath)
                'If Types.Count > 0 Then
                'End If
                Dim sTag As String = sPath.Replace("/", " ")
                sTags += String.Format(indent2 + "- '{0}'" + vbCrLf, SafeStr(sTag))

                For Each method In t.GetMethods()
                    If method.IsPublic AndAlso (Not _OPT_SHOW_OWN OrElse method.DeclaringType.Name = t.Name) Then
                        sPaths += String.Format(indent1 + "'/{0}/?action={1}':" + vbCrLf + indent2 + "get:" + vbCrLf, sPath, method.Name.TrimStart(CChar("_")))

                        Dim sSummary As String = method.GetSummary()
                        If sSummary <> "" Then sSummary = String.Format(indent3 + "summary: '{0}'" + vbCrLf, SafeStr(sSummary))

                        Dim parameters As Reflection.ParameterInfo() = method.GetParameters()
                        Dim sParams As String = ""
                        For Each x As Reflection.ParameterInfo In method.GetParameters()
                            sParams += String.Format(indent4 + "- name: {1}" + vbCrLf + indent5 + "in: query" + vbCrLf + indent5 + "description: {3}" + vbCrLf + indent5 + "required: {2}" + vbCrLf + indent5 + "schema:" + vbCrLf + indent6 + "type: {4}" + vbCrLf + indent6 + "format: {0}" + vbCrLf, x.ParameterType.Name, x.Name, Bool2JS(Not x.IsOptional), "", getTypeName(x.ParameterType))
                            addSchema(x.ParameterType, usedSchemas, sSchemas)
                        Next
                        If sParams <> "" Then sParams = indent3 + "parameters:" + vbCrLf + sParams

                        Dim retType As String = method.ReturnType.Name
                        If retType = "List`1" AndAlso method.ReturnType.GenericTypeArguments IsNot Nothing AndAlso method.ReturnType.GenericTypeArguments.Length > 0 Then
                            retType = String.Format("List({0})", method.ReturnType.GenericTypeArguments(0).Name)
                        End If
                        Dim sRet As String = ""
                        If retType <> "" AndAlso retType.StartsWith("j") Then
                            sRet = indent6 + "application/json:" + vbCrLf + indent6 + "  schema:" + vbCrLf + indent6 + String.Format("    $ref: '#/components/schemas/{0}'", retType) + vbCrLf
                            addSchema(method.ReturnType, usedSchemas, sSchemas)
                        Else
                            sRet = indent6 + "text/plain:" + vbCrLf + indent6 + "  schema:" + vbCrLf + indent6 + "    type: string" + vbCrLf
                        End If

                        sPaths += String.Format(indent3 + "tags:" + vbCrLf + indent4 + "- '{3}'" + vbCrLf + "{0}{1}" + indent3 + "responses:" + vbCrLf + indent4 + "'200':" + vbCrLf + indent5 + "content:" + vbCrLf + "{2}" + vbCrLf, sSummary, sParams, sRet, SafeStr(sTag))
                    End If
                Next
            Next
            If sPaths <> "" Then sPaths = vbCrLf + sPaths
            If sSchemas <> "" Then sSchemas = vbCrLf + sSchemas
            If sTags <> "" Then sTags = vbCrLf + sTags
#End If

            Dim Templates As New Dictionary(Of String, String)
            Templates.Add("%%schemas%%", sSchemas)
            'Templates.Add("%%parameters%%", sParameters)
            'Templates.Add("%%responses%%", sResponses)
            Templates.Add("%%tags%%", sTags)
            Templates.Add("%%paths%%", sPaths)
            Templates.Add("%%version%%", Now.ToString("yyMMdd"))

            Dim sYAML As String = ParseString(sTpl)
            sYAML = ParseStringTemplates(sYAML, Templates)

#If _OPT_USE_MS_OPENAPI Then

            Dim oaReader As New Microsoft.OpenApi.Readers.OpenApiStringReader(New Readers.OpenApiReaderSettings With {
            .ReferenceResolution = Readers.ReferenceResolutionSetting.DoNotResolveReferences})
            Dim oaDiag As New Readers.OpenApiDiagnostic
            Dim oaDoc As OpenApiDocument = oaReader.Read(sYAML, oaDiag)

            'If doc IsNot Nothing AndAlso (oaDiag.Errors Is Nothing OrElse oaDiag.Errors.Count = 0) Then
            If oaDoc IsNot Nothing Then

                Dim xmlDoc As New XmlDocument()
                Dim xmlFName As String = _FILE_ROOT + "bin\Application.xml"
                If MyComputer.FileSystem.FileExists(xmlFName) Then
                    xmlDoc.Load(xmlFName)
                Else
                    sError = "XML documentation not present"    ' D7587
                    Exit Sub ' D7587
                End If

                If oaDoc.Paths Is Nothing Then oaDoc.Paths = New OpenApiPaths
                If oaDoc.Components Is Nothing Then oaDoc.Components = New OpenApiComponents
                If oaDoc.Components.Schemas Is Nothing Then oaDoc.Components.Schemas = New Dictionary(Of String, OpenApiSchema)
                If oaDoc.Tags Is Nothing Then oaDoc.Tags = New List(Of OpenApiTag)

                For Each sPath As String In ListAll.Keys
                    Dim t As Type = ListAll(sPath)
                    Dim sTag As String = sPath.Replace("/", " ")
                    oaDoc.Tags.Add(New OpenApiTag With {.Name = sTag, .Description = t.GetSummary()})

                    For Each method In t.GetMethods()
                        If method.IsPublic AndAlso (Not _OPT_SHOW_OWN OrElse method.DeclaringType.Name = t.Name) Then

                            Dim XMLPath As String = String.Format("M:{0}.{1}", t.Name, method.Name)
                            Dim xmlSection As XmlNode = xmlDoc.SelectSingleNode("//member[starts-with(@name, '" + XMLPath + "')]")

                            Dim oaPath As New OpenApiPathItem
                            Dim retNode As XmlNode = Nothing
                            Dim sExample As String = ""

                            Dim oaOper As New OpenApiOperation With {.Parameters = New List(Of OpenApiParameter), .Tags = New List(Of OpenApiTag) From {New OpenApiTag With {.Name = sTag}}, .Summary = method.GetSummary()}
                            sExample = GetXMLDescription(xmlSection, "remark", ListAll)
                            If Not String.IsNullOrEmpty(sExample) Then oaOper.Description = sExample
                            sExample = GetXMLDescription(xmlSection, "example", ListAll)
                            If Not String.IsNullOrEmpty(sExample) Then
                                If oaOper.RequestBody Is Nothing Then oaOper.RequestBody = New OpenApiRequestBody With {
                                    .Content = New Dictionary(Of String, OpenApiMediaType)
                                    }
                                oaOper.RequestBody.Content.Add("request", New OpenApiMediaType With {
                                    .Example = New Any.OpenApiString(sExample)
                                    })
                            End If

                            For Each x As Reflection.ParameterInfo In method.GetParameters()
                                Dim desc As String = GetXMLDescription(xmlSection, x.Name, ListAll, True, retNode)
                                If (x.IsOptional AndAlso x.HasDefaultValue AndAlso x.DefaultValue IsNot Nothing AndAlso x.HasDefaultValue.ToString <> "") Then desc += String.Format("{1}Default value is ""{0}""", x.DefaultValue.ToString, If(desc = "", "", ". "))
                                oaOper.Parameters.Add(New OpenApiParameter With {
                                    .Name = x.Name,
                                    .In = ParameterLocation.Query,
                                    .Required = Not x.IsOptional,
                                    .Schema = GetSchema(If(x.ParameterType.IsByRef, x.ParameterType.GetElementType, x.ParameterType), oaDoc.Components.Schemas),    ' D7221
                                    .Description = desc
                                })
                            Next

                            Dim oaResp As New OpenApiResponse With {
                                .Content = New Dictionary(Of String, OpenApiMediaType),
                                .Description = GetXMLDescription(xmlSection, "returns", ListAll, False, retNode)
                            }


                            Dim ret As New OpenApiMediaType With {
                                .Schema = GetSchema(method.ReturnType, oaDoc.Components.Schemas)
                            }
                            sExample = GetXMLDescription(retNode, "example", ListAll)
                            If Not String.IsNullOrEmpty(sExample) Then
                                ret.Example = New Any.OpenApiString(GetXMLDescription(retNode, "example", ListAll), False)
                            End If

                            oaResp.Content.Add(If(method.ReturnType.Name.StartsWith("j"), "application/json", "text/plain"), ret)

                            oaOper.Responses = New OpenApiResponses
                            oaOper.Responses.Add("200", oaResp)

                            oaPath.AddOperation(OperationType.Get, oaOper)
                            oaDoc.Paths.Add(String.Format("/{0}/?action={1}", sPath, method.Name.TrimStart(CChar("_"))), oaPath)
                        End If
                    Next
                Next

                If MyComputer.FileSystem.FileExists(_FILE_OPENAPI_OUTPUT_YAML) Then File_Erase(_FILE_OPENAPI_OUTPUT_YAML)
                Dim str As FileStream = File.Open(_FILE_OPENAPI_OUTPUT_YAML, FileMode.Create)
                Using str
                    Dim sw As New StreamWriter(str)
                    Using sw
                        Dim w As New Writers.OpenApiYamlWriter(sw)
                        oaDoc.SerializeAsV3(w)
                    End Using
                    str.Close()
                End Using

                If MyComputer.FileSystem.FileExists(_FILE_OPENAPI_OUTPUT_JSON) Then File_Erase(_FILE_OPENAPI_OUTPUT_JSON)
                str = File.Open(_FILE_OPENAPI_OUTPUT_JSON, FileMode.Create)
                Using str
                    Dim sw As New StreamWriter(str)
                    Using sw
                        Dim w As New Writers.OpenApiJsonWriter(sw)
                        oaDoc.SerializeAsV3(w)
                    End Using
                    str.Close()
                End Using

            Else
                sError = "Unable to parse template: " + vbCrLf
                For Each tErr As OpenApiError In oaDiag.Errors
                    sError += tErr.Message + vbCrLf
                Next

            End If

#Else
            File_Erase(_FILE_OPENAPI_OUTPUT)
            MyComputer.FileSystem.WriteAllText(_FILE_OPENAPI_OUTPUT, sYAML, False)
#End If
        Else
            sError = "OpenAPI template is empty or missing" ' D7587
        End If
    End Sub

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim sError As String = ""
        GenerateOpenAPI(sError)
        _Page.ResponseData = New jActionResult() With {
            .Message = sError,
            .Result = If(String.IsNullOrEmpty(sError), ecActionResult.arSuccess, ecActionResult.arError)
        }
    End Sub

End Class