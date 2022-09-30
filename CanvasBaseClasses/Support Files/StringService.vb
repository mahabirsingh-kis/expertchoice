Imports System.Text.RegularExpressions
Imports System.Text
Imports System.Linq
Imports Canvas.CanvasTypes
Imports ExpertChoice.Data

Namespace ExpertChoice.Service

    Public Module StringService

        ' D4105 ===
        Public Enum ecNodePathFormat
            SpanWithTitleLimitedLevels = 1
            SpanWithTitleFillPath = 2
            LimitedLevels = 3
            FullPath = 4
            FullHierarchy = 5
            HierarchyWithoutCurrent = 6 ' D4109
            PathWithoutCurrent = 7      ' D4109
        End Enum
        ' D4105 ==

        'Public _VERSION_YEAR As String = "2009"     ' D0457 'MF0561
        Public _ACCESSKEY_PREFIX As String = "~"    ' D0246

        Public _Revision As Integer = -1            ' D0461 + D0473

        ''' <summary>
        ''' Get current version of this application.
        ''' </summary>
        ''' <returns>String in form "MajorVer.MinorVer.Build".</returns>
        ''' <remarks>Version retrieved from "AssemblyInfo.vb" file</remarks>
        Public Function GetVersion(ByVal ver As System.Version, Optional Format As VersionFormat = VersionFormat.Normal) As String
            Dim sVersion As String = ""
            Select Case Format
                Case VersionFormat.Normal
                    sVersion = String.Format("{0}.{1}.{2}", ver.Major, ver.Minor, CStr(ver.Build).PadLeft(3, "0"c))
                    If ver.Revision > 0 Then
                        sVersion += String.Format(".{0}", ver.Revision) ' D0367
                    End If
                Case VersionFormat.Short
                    sVersion = String.Format("{0}.{1}.{2}", ver.Major, ver.Minor, ver.Build)
            End Select
            Return sVersion
        End Function

        Public Enum VersionFormat
            Normal = 0
            [Short] = 1
        End Enum

        ' D0367 ===
        'Private Function GetSVNRevision(ByVal sFolder As String) As Integer
        '    Dim tRevision As Integer = -1
        '    Dim sFileName As String = sFolder + ".svn\entries"
        '    If My.Computer.FileSystem.FileExists(sFileName) Then
        '        Dim sContent As String = File_GetContent(sFileName)
        '        If sContent <> "" Then
        '            Dim Lines() As String = sContent.Replace(vbCr, "").Split(CChar(vbLf))
        '            If Lines.Length > 10 Then
        '                If Lines(10) <> "" Then Integer.TryParse(Lines(10).Trim, tRevision)
        '                DebugInfo("Read revision from SVN data: " + tRevision.ToString, _TRACE_INFO)
        '            End If
        '        End If
        '    End If
        '    Return tRevision
        'End Function

        ' D0948 ===
        Private Function GetHgRevision(ByVal sFolder As String) As Integer
            Dim tRevision As Integer = -1
            Dim sFileName As String = sFolder + ".hg\cache\tags"    ' D1401
            If My.Computer.FileSystem.FileExists(sFileName) Then
                Dim sContent As String = File_GetContent(sFileName)
                If sContent <> "" Then
                    Dim Lines() As String = sContent.Replace(vbCr, "").Split(CChar(vbLf))
                    If Lines.Length > 0 Then
                        Dim sVal As String = Lines(0)
                        Dim Idx As Integer = sVal.IndexOf(" ")
                        If Idx > 0 Then
                            sVal = sVal.Substring(0, Idx).Trim
                            If Integer.TryParse(sVal, tRevision) Then
                                DebugInfo("Read revision from Hg tags data: " + tRevision.ToString, _TRACE_INFO)
                            Else
                                tRevision = -1
                            End If
                        End If
                    End If
                End If
            End If
            Return tRevision
        End Function
        ' D0948 ==

        Public Function RevisionNumber(ByVal sApplicationFolder As String) As Integer
            If _Revision < 0 Then
                _Revision = 0

                If sApplicationFolder <> "" AndAlso Not sApplicationFolder.EndsWith("\") Then sApplicationFolder += "\" ' D0880

                Dim sRevFile As String = sApplicationFolder + "revision.txt"
                If My.Computer.FileSystem.FileExists(sRevFile) Then
                    ' D7545 + D7585 ===
                    Dim sRevContent As String = File_GetContent(sRevFile).Replace(vbCr, "").Replace("+", "").Trim
                    Dim sLines As String() = sRevContent.Split(CChar(vbLf))
                    If sLines.Count > 0 Then sRevContent = sLines(0)
                    sRevContent = sRevContent.Trim(CChar(vbLf)).Trim
                    ' D7585 ==
                    If sRevContent <> "" AndAlso Integer.TryParse(sRevContent, _Revision) Then
                        DebugInfo("Read revision from revision.txt: " + _Revision.ToString, _TRACE_INFO)
                    End If
                    ' D7545 ==
                End If

                ' D4234 ===
                If My.Computer.FileSystem.FileExists(sApplicationFolder + "repo.txt") Then
                    Dim sRepo As String = ""
                    sRepo = File_GetContent(sApplicationFolder + "repo.txt").Replace(vbCrLf + vbCrLf, vbCrLf).Replace(vbLf, "")
                    If sRepo <> "" Then
                        Dim idx As Integer = sRepo.ToLower.IndexOf("build:")
                        If idx >= 0 Then
                            Dim sRev As String = sRepo.Substring(idx + 6).Trim
                            idx = sRev.IndexOf(vbCr)
                            If idx > 0 Then sRev = sRev.Substring(0, idx).Trim
                            idx = sRev.LastIndexOf(".")
                            If idx > 0 Then sRev = sRev.Substring(idx + 1)
                            If Not Integer.TryParse(sRev, _Revision) Then _Revision = 0
                        End If
                    End If
                End If
                ' D4234 ==

                ' -D4234 ===
                '' D0948 ===
                ''Dim tAppRevision As Integer = GetSVNRevision(sApplicationFolder)
                ''If tAppRevision > 0 AndAlso tAppRevision > _Revision Then _Revision = tAppRevision

                ''tAppRevision = GetSVNRevision(sApplicationFolder )
                'Dim tAppRevision As Integer = GetHgRevision(sApplicationFolder + "..\")
                '' D0948 ==
                'If tAppRevision > 0 AndAlso tAppRevision > _Revision Then _Revision = tAppRevision
                ' - D4234 ==

            End If
            If _Revision < 0 Then _Revision = 0 ' D3816
            Return _Revision
        End Function
        ' D0367 ==

        ' D0246 ===
        Public Function ParseAccessKey(ByVal sCaption As String, ByRef sAccessKey As String) As String
            Dim sAK As String = ""
            Dim startPos As Integer = 0
            Do Until startPos >= sCaption.Length
                Dim umlPosition As Integer = sCaption.IndexOf(_ACCESSKEY_PREFIX, startPos)
                If umlPosition >= 0 And umlPosition < sCaption.Length - 1 Then
                    sAK = sCaption.Substring(umlPosition + 1, 1)
                    If sAK = _ACCESSKEY_PREFIX Then
                        startPos = umlPosition + 2
                    Else
                        If Not sAccessKey Is Nothing Then sAccessKey = sAK.ToUpper
                        sCaption = String.Format("{0}<u>{1}</u>{2}", sCaption.Substring(0, umlPosition), sAK, sCaption.Substring(umlPosition + 2))
                        Exit Do
                    End If
                Else
                    startPos = sCaption.Length + 1
                End If
            Loop
            sCaption = sCaption.Replace(_ACCESSKEY_PREFIX + _ACCESSKEY_PREFIX, _ACCESSKEY_PREFIX)
            Return sCaption
        End Function
        ' D0246 ==

        ' D0426 ===
        Public Function FileSize(ByVal sBytes As Long) As String
            Dim sText As String = "b"
            Dim sSize As String = CStr(sBytes)
            If sBytes > 999 * 1024 Then
                sSize = (sBytes / (1024 * 1024)).ToString("F1")
                sText = "Mb"
            ElseIf sBytes > 9999 Then
                sSize = CStr(sBytes \ 1024)
                sText = "Kb"
            End If
            Return String.Format("{0}{1}", sSize, sText)
        End Function
        ' D0426 ==

        ' D4013 ==
        Public Function padNumWithZeros(Value As Integer, MaxLength As Integer) As String
            Return String.Format("{0:" + "000000000000".Substring(0, CInt(IIf(MaxLength > 12, 12, IIf(MaxLength < 1, 1, MaxLength)))) + "}", Value)
        End Function

        Public Function padWithZeros(Value As String, MaxLength As Integer) As String
            If Value IsNot Nothing Then
                While Value.Length < MaxLength
                    Value = "0" + Value
                End While
            End If
            Return Value
        End Function
        ' D4013 ==

        'L0461 ===
        Public Function ParseStackTrace(ByVal InitStackTrace As String) As String
            Dim Lines As String()
            Dim Result As String = ""
            Lines = InitStackTrace.Split(CChar(Environment.NewLine))
            For Each line In Lines
                If line.Contains(" in") Then
                    line = line.Substring(0, line.IndexOf(" in")).Trim()
                End If
                Result += line
            Next
            Return Result
        End Function
        'L0461 ==

        Public Function GetCaseHeader(ByVal ex As System.Exception) As String
            Dim sKind As String = "Service RTE: "
            Dim StackTraceHash As String = ParseStackTrace(ex.StackTrace)
            StackTraceHash = Strings.Left(GetMD5(StackTraceHash), 7)
            Dim retVal As String = Left(String.Format("{0}'{1}'", sKind, ex.Message), 117)
            Return String.Format("{0} [{1}]", retVal, StackTraceHash)
        End Function

        ' D4106 ===
        Public Function GetNodePathString(tNode As ECCore.clsNode, tFormat As ecNodePathFormat) As String    ' D4105
            Dim sResult As String = ""
            If tNode IsNot Nothing AndAlso Not tNode.IsAlternative AndAlso tNode.ParentNode IsNot Nothing Then  ' D4109
                Dim tMaxLevels As Integer = 3
                Dim tHints As New List(Of String)
                Dim tLevels As Integer = 0
                Dim sPath As String = ""
                Dim sPathFull As String = ""
                ' D4109 ===
                Dim sNode As String = String.Format("<i>{0}</i>", SafeFormString(tNode.NodeName))
                If tFormat <> ecNodePathFormat.PathWithoutCurrent Then sPathFull = sNode
                If tFormat <> ecNodePathFormat.HierarchyWithoutCurrent Then tHints.Add(sNode)
                ' D4109 ==
                While tNode.ParentNode IsNot Nothing AndAlso tNode.ParentNode.Level > 0 ' D4109
                    tHints.Insert(0, SafeFormString(tNode.ParentNode.NodeName))
                    If tLevels < tMaxLevels + 1 Then
                        If sPath.Length + CInt(IIf(tNode.ParentNode.NodeName.Length > 65, 65, tNode.ParentNode.NodeName.Length)) > 135 Then tLevels = tMaxLevels
                        sPath = String.Format("<nobr>{0}{1}</nobr>{2}", IIf(tLevels >= tMaxLevels, "&hellip;", SafeFormString(ShortString(tNode.ParentNode.NodeName, 65, True))), IIf(sPath = "", "", " &gt; "), sPath)
                    End If
                    sPathFull = String.Format("{0}{1}{2}", SafeFormString(tNode.ParentNode.NodeName), IIf(sPathFull = "", "", " &gt; "), sPathFull)
                    tNode = tNode.ParentNode
                    tLevels += 1
                End While
                If tHints.Count > 0 Then
                    Dim sHint As String = ""
                    Dim sIndent As String = ""
                    For Each sName As String In tHints
                        sHint += String.Format("<div><nobr>{0}&#149;&nbsp;{1}</nobr></div>", sIndent, sName)
                        sIndent += "&nbsp;&nbsp;&nbsp;"
                    Next
                    Select Case tFormat
                        Case ecNodePathFormat.FullHierarchy, ecNodePathFormat.HierarchyWithoutCurrent   ' D4109
                            sResult = sHint
                        Case ecNodePathFormat.FullPath, ecNodePathFormat.PathWithoutCurrent ' D4109
                            sResult = sPathFull
                        Case ecNodePathFormat.LimitedLevels
                            sResult = sPath
                        Case ecNodePathFormat.SpanWithTitleFillPath
                            sResult = String.Format("<span title=""{1}"">{0}</span>", sPathFull, sHint.Replace("""", "&quot;")) ' D4109
                        Case ecNodePathFormat.SpanWithTitleLimitedLevels
                            sResult = String.Format("<span title=""{1}"">{0}</span>", sPath, sHint.Replace("""", "&quot;"))     ' D4109
                    End Select
                End If
            End If
            Return sResult
        End Function
        ' D4106 ==

        ' D5025 ===
        Public Function GetMethodsList(ByVal type As Type, Optional ShowOnlyOwn As Boolean = True) As String
            Dim sList As New List(Of String)    ' D5032
            If type IsNot Nothing Then  ' D5026
                For Each method In type.GetMethods()
                    If method.IsPublic AndAlso (Not ShowOnlyOwn OrElse method.DeclaringType.Name = type.Name) Then
                        Dim parameters = method.GetParameters()
                        Dim parameterDescriptions = String.Join(", ", method.GetParameters().[Select](Function(x) String.Format("{1}{2} [{0}]", x.ParameterType.Name, x.Name, If(x.IsOptional, "?", ""))).ToArray())
                        ' D5026 ===
                        Dim retType = method.ReturnType.Name
                        If retType = "List`1" AndAlso method.ReturnType.GenericTypeArguments IsNot Nothing AndAlso method.ReturnType.GenericTypeArguments.Length > 0 Then
                            retType = String.Format("List({0})", method.ReturnType.GenericTypeArguments(0).Name)
                        End If
                        Dim sSummary As String = method.GetSummary()
                        If sSummary <> "" Then sSummary = " - " + sSummary
                        sList.Add(String.Format("{1} ({2}) [{0}] {3}", retType, method.Name.TrimStart(CChar("_")), parameterDescriptions, sSummary))   ' D5032
                        ' D5026 ==
                    End If
                Next
            End If
            sList.Sort()    ' D5032
            Return String.Join(vbNewLine, sList)    ' D5032
        End Function
        ' D5025 ==

        ' D5032 ===
        Public Function Str2ProjectStatus(sValue As String) As ecProjectStatus
            Dim Status As ecProjectStatus = ecProjectStatus.psActive
            Select Case sValue
                Case ecProjectStatus.psArchived.ToString, CInt(ecProjectStatus.psArchived).ToString
                    Status = ecProjectStatus.psArchived
                Case ecProjectStatus.psTemplate.ToString, CInt(ecProjectStatus.psTemplate).ToString
                    Status = ecProjectStatus.psTemplate
                Case ecProjectStatus.psMasterProject.ToString, CInt(ecProjectStatus.psMasterProject).ToString
                    Status = ecProjectStatus.psMasterProject
            End Select
            Return Status
        End Function

        Public Function Str2ProjectType(sValue As String) As ProjectType
            Dim T As ProjectType = ProjectType.ptRegular
            Select Case sValue
                Case ProjectType.ptMyRiskReward.ToString, CInt(ProjectType.ptMyRiskReward).ToString ' D6798
                    T = ProjectType.ptMyRiskReward    ' D6798
                Case ProjectType.ptMixed.ToString, CInt(ProjectType.ptMixed).ToString
                    T = ProjectType.ptMixed
                Case ProjectType.ptOpportunities.ToString, CInt(ProjectType.ptOpportunities).ToString
                    T = ProjectType.ptOpportunities
                Case ProjectType.ptRiskAssociated.ToString, CInt(ProjectType.ptRiskAssociated).ToString
                    T = ProjectType.ptRiskAssociated
            End Select
            Return T
        End Function
        ' D5032 ==

        ' D7121 ===
        ''' <summary>
        ''' Get the distance (num of editable symbols) between the two strings
        ''' </summary>
        ''' <param name="value1"></param>
        ''' <param name="value2"></param>
        ''' <returns></returns>
        Public Function Levenshtein_Distance(ByVal value1 As String, ByVal value2 As String) As Integer
            If value2.Length = 0 Then Return value1.Length

            Dim costs As Integer() = New Integer(value2.Length - 1) {}

            Dim k As Integer = 0
            While k < costs.Length
                costs(k) = Threading.Interlocked.Increment(k)
            End While

            For i As Integer = 0 To value1.Length - 1
                Dim cost As Integer = i
                Dim previousCost As Integer = i
                Dim value1Char As Char = value1(i)

                For j As Integer = 0 To value2.Length - 1
                    Dim currentCost As Integer = cost
                    cost = costs(j)

                    If value1Char <> value2(j) Then
                        If previousCost < currentCost Then
                            currentCost = previousCost
                        End If
                        If cost < currentCost Then
                            currentCost = cost
                        End If
                        currentCost += 1
                    End If
                    costs(j) = currentCost
                    previousCost = currentCost
                Next
            Next

            Return costs(costs.Length - 1)
        End Function
        ' D7121 ==

    End Module

End Namespace
