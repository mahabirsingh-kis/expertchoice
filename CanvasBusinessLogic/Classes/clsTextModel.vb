Imports ECCore.ECTypes
Imports ECCore
Imports ExpertChoice.Service

Namespace ExpertChoice.Data

    Public Class clsTextModel

#Region "Constants and declarations"

        Public Shared _TR_PREFIX As String = "#"    ' D2133
        Public Shared _TR_COMMENT As String = "'"   ' D2133
        ' D2146 ===
        Public Shared _TR_HEADER() As String = {_TR_PREFIX(0) + "MODEL", _TR_PREFIX(0) + "MODEL:"}
        Public Shared _TR_HIERARCHY() As String = {_TR_PREFIX(0) + "OBJECTIVES", _TR_PREFIX(0) + "OBJECTIVES:", _TR_PREFIX(0) + "OBJS", _TR_PREFIX(0) + "OBJS:"}
        Public Shared _TR_LIKELIHOOD() As String = {_TR_PREFIX(0) + "LIKELIHOOD", _TR_PREFIX(0) + "LIKELIHOOD:"}   ' D2425 + D2452
        Public Shared _TR_IMPACT() As String = {_TR_PREFIX(0) + "IMPACT", _TR_PREFIX(0) + "IMAPCT:"}        ' D2425
        Public Shared _TR_CONTROLS() As String = {_TR_PREFIX(0) + "CONTROLS", _TR_PREFIX(0) + "CONTROLS:"}    ' D5041
        Public Shared _TR_ALTERNATIVES() As String = {_TR_PREFIX(0) + "ALTERNATIVES", _TR_PREFIX(0) + "ALTERNATIVES:", _TR_PREFIX(0) + "ALTS", _TR_PREFIX(0) + "ALTS:", _TR_PREFIX(0) + "EVENTS", _TR_PREFIX(0) + "EVENTS:"}    ' D2454
        Public Shared _TR_EVENTS() As String = {_TR_PREFIX(0) + "EVENTS", _TR_PREFIX(0) + "EVENTS:"}    ' D5041
        Public Shared _TR_PARTICIPANTS() As String = {_TR_PREFIX(0) + "PARTICIPANTS", _TR_PREFIX(0) + "PARTICIPANTS:", _TR_PREFIX(0) + "USERS", _TR_PREFIX(0) + "USERS:"}
        ' D2146 ==

        Public Shared _TR_SPLITTER As String = vbCrLf
        Public Shared _TR_DELIMITER As String = vbTab

        Public Shared Option_DetectHypelinks As Boolean = True  ' D2756

        ' D2299 ===
        Delegate Function ProcessStringLineFunction(ByVal sLine As String, LineIdx As Integer, ByRef tData As List(Of String)) As Boolean
        Delegate Function ProcessNodeLineFunction(ByVal sLine As String, LineIdx As Integer, ByRef tData As List(Of clsNode)) As Boolean
        Delegate Function ProcessUserLineFunction(ByVal sLine As String, LineIdx As Integer, ByRef tData As List(Of clsUser)) As Boolean
        ' D2299 ==

#End Region

#Region "Service functions"

        Public Shared Function PrepareContent(sContent As String) As String()
            Dim sLines As String() = {}
            If sContent IsNot Nothing Then
                sLines = sContent.Trim.Split(CChar(_TR_SPLITTER))
            End If
            Return sLines
        End Function

        Private Shared Function IgnoreEmptyLines(sLines As String(), Optional tStartIdx As Integer = 0) As Integer
            If sLines IsNot Nothing Then
                If tStartIdx < 0 Then tStartIdx = 0
                If sLines.Length > 0 Then
                    If tStartIdx >= sLines.Length Then tStartIdx = -1
                    While tStartIdx >= 0 AndAlso tStartIdx < sLines.Length AndAlso sLines(tStartIdx).Trim = ""
                        tStartIdx += 1
                    End While
                    If tStartIdx >= sLines.Length Then tStartIdx = -1
                End If
            End If
            Return tStartIdx
        End Function

        Private Shared Function GetSectionIdx(sLines As String(), sSectionID() As String) As Integer
            Dim Idx As Integer = -1
            If sLines IsNot Nothing AndAlso sLines.Length > 0 Then
                For i As Integer = 0 To sLines.Length - 1
                    If Array.IndexOf(sSectionID, sLines(i).Trim.ToUpper) >= 0 Then Return i
                Next
            End If
            Return Idx
        End Function

#End Region

#Region "Read and parse sections"

        Private Shared Function ParseString(sLine As String, LineIdx As Integer, ByRef tData As List(Of String)) As Boolean  ' D2299
            If tData IsNot Nothing AndAlso Not String.IsNullOrEmpty(sLine) Then
                tData.Add(sLine)
                Return True
            End If
            Return False
        End Function

        Private Shared Function ParseUserLine(sLine As String, LineIdx As Integer, ByRef tData As List(Of clsUser)) As Boolean    ' D2299
            If tData IsNot Nothing AndAlso Not String.IsNullOrEmpty(sLine) Then
                sLine = sLine.Trim
                Dim idx As Integer = sLine.IndexOf(_TR_DELIMITER)
                Dim idx_space As Integer = sLine.IndexOf(" ")   ' D6486
                If idx_space > 0 AndAlso (idx_space < idx OrElse idx < 0) Then idx = idx_space  ' D6486
                sLine = sLine.Replace(_TR_DELIMITER, " ")
                Dim sEmail As String = ""
                Dim sName As String = ""
                If idx > 0 Then
                    sEmail = sLine.Substring(0, idx).Trim
                    sName = sLine.Substring(idx + 1).Trim
                Else
                    sEmail = sLine.Trim
                End If
                If sEmail <> "" Then
                    Dim tUser As New clsUser
                    tUser.UserID = LineIdx
                    tUser.UserEMail = sEmail
                    tUser.UserName = sName
                    tData.Add(tUser)
                    Return True
                End If
            End If
            Return False
        End Function

        Private Shared Function ParseObjective(sLine As String, LineIdx As Integer, ByRef tData As List(Of clsNode)) As Boolean   ' D2299
            If tData IsNot Nothing AndAlso Not String.IsNullOrEmpty(sLine) Then
                sLine = sLine.Trim
                Dim idx As Integer = sLine.IndexOf(_TR_DELIMITER)
                sLine = sLine.Replace(_TR_DELIMITER, " ")
                Dim sName As String = ""
                Dim sDesc As String = ""
                If idx > 0 Then
                    sName = sLine.Substring(0, idx).Trim
                    sDesc = sLine.Substring(idx + 1).Trim
                Else
                    sName = sLine.Trim
                End If
                If sName <> "" Then
                    Dim tNode As New clsNode
                    tNode.NodeID = LineIdx
                    tNode.NodeName = sName
                    tNode.InfoDoc = sDesc
                    tData.Add(tNode)
                    Return True
                End If
            End If
            Return False
        End Function

        Public Shared Function ReadUsers(sContent As String, ByRef sError As String) As List(Of clsUser)
            Return ReadUsers(PrepareContent(sContent), sError)
        End Function

        Public Shared Function ReadUsers(sLines As String(), ByRef sError As String) As List(Of clsUser)
            Dim tUsers As New List(Of clsUser)
            Return CType(ReadSectionUsers(sLines, _TR_PARTICIPANTS, AddressOf ParseUserLine, tUsers), List(Of clsUser))
        End Function

        Public Shared Function ReadObjectives(sContent As String, ByRef sError As String) As List(Of clsNode)
            Return ReadObjectives(PrepareContent(sContent), sError)
        End Function

        Public Shared Function ReadObjectives(sLines As String(), ByRef sError As String) As List(Of clsNode)
            Dim tObjectives As New List(Of clsNode)
            ReadSectionNodes(sLines, _TR_HIERARCHY, AddressOf ParseObjective, tObjectives)  ' D5053
            Return tObjectives  ' D5053
        End Function

        Public Shared Function ReadAlternatives(sContent As String, ByRef sError As String) As List(Of clsNode)
            Return ReadAlternatives(PrepareContent(sContent), sError)
        End Function

        Public Shared Function ReadAlternatives(sLines As String(), ByRef sError As String) As List(Of clsNode)
            Dim tAlternatives As New List(Of clsNode)
            ' D5053 ===
            ReadSectionNodes(sLines, _TR_ALTERNATIVES, AddressOf ParseObjective, tAlternatives)
            For Each tAlt As clsNode In tAlternatives
                tAlt.IsAlternative = True
            Next
            Return tAlternatives
            ' D5053 ==
        End Function

        Private Shared Function ReadNodesSection(sLines As String(), sHeaders As String(), ByRef sError As String) As ArrayList  ' D2426
            Dim sSection As New List(Of String) ' D2256
            ' D2136 ===
            sSection = CType(ReadSection(sLines, sHeaders, AddressOf ParseString, sSection), List(Of String))   ' D2426
            If sSection IsNot Nothing Then
                For i As Integer = 0 To sSection.Count - 1
                    Dim sStr As String = CStr(sSection(i))
                    Dim c As Integer = 0
                    While c < 16 AndAlso c < sStr.Length AndAlso (sStr(c) = vbTab OrElse sStr(c) = " ")
                        c += 1
                    End While
                    If c > 0 Then sSection(i) = "                ".Substring(0, c) + sStr.Substring(c)
                Next
            End If
            Dim tRes As New ArrayList
            For Each sLine As String In sSection
                tRes.Add(sLine)
            Next
            Return tRes
            ' D2136 ==
        End Function

#End Region

#Region "Shared Common Read functions"

        Public Shared Function ReadSection(sLines As String(), sSectionID() As String, tParseValueFunc As ProcessStringLineFunction, tData As List(Of String)) As List(Of String)
            If sLines IsNot Nothing Then
                Dim idx = GetSectionIdx(sLines, sSectionID)
                If idx >= 0 Then    ' D2150
                    Dim fCanRead As Boolean = True
                    While idx < sLines.Length - 1 And fCanRead
                        idx += 1
                        Dim sRow As String = sLines(idx).Replace(vbCr, "").Replace(vbLf, "")    ' D2133
                        If sRow.Trim = "" OrElse sRow.StartsWith(_TR_PREFIX) Then   ' D2133
                            fCanRead = False
                        Else
                            If Not sRow.StartsWith(_TR_COMMENT) Then    ' D2133
                                tParseValueFunc(sRow, idx, tData)
                            End If
                        End If
                    End While
                End If
            End If
            Return tData
        End Function

        Public Shared Function ReadSectionUsers(sLines As String(), sSectionID() As String, tParseValueFunc As ProcessUserLineFunction, tData As List(Of clsUser)) As List(Of clsUser)
            If sLines IsNot Nothing Then
                Dim idx = GetSectionIdx(sLines, sSectionID)
                If idx >= 0 Then    ' D2150
                    Dim fCanRead As Boolean = True
                    While idx < sLines.Length - 1 And fCanRead
                        idx += 1
                        Dim sRow As String = sLines(idx).Replace(vbCr, "").Replace(vbLf, "")    ' D2133
                        If sRow.Trim = "" OrElse sRow.StartsWith(_TR_PREFIX) Then   ' D2133
                            fCanRead = False
                        Else
                            If Not sRow.StartsWith(_TR_COMMENT) Then    ' D2133
                                tParseValueFunc(sRow, idx, tData)
                            End If
                        End If
                    End While
                End If
            End If
            Return tData
        End Function

        Public Shared Sub ReadSectionNodes(sLines As String(), sSectionID() As String, tParseValueFunc As ProcessNodeLineFunction, ByRef tData As List(Of clsNode)) ' D2299 + D5053
            If sLines IsNot Nothing Then
                Dim idx = GetSectionIdx(sLines, sSectionID)
                If idx >= 0 Then    ' D2150
                    Dim fCanRead As Boolean = True
                    While idx < sLines.Length - 1 AndAlso fCanRead
                        idx += 1
                        Dim sRow As String = sLines(idx).Replace(vbCr, "").Replace(vbLf, "")    ' D2133
                        If sRow.Trim = "" OrElse sRow.StartsWith(_TR_PREFIX) Then   ' D2133
                            fCanRead = False
                        Else
                            If Not sRow.StartsWith(_TR_COMMENT) Then    ' D2133
                                tParseValueFunc(sRow, idx, tData)
                            End If
                        End If
                    End While
                End If
            End If
        End Sub

        ' D5053 ===
        Friend Shared Function ReadAlternativesToHierarchy(sLines As String(), tHeads() As String, ByRef tHierarchy As clsHierarchy) As String
            Dim sError As String = ""
            Dim AltsList As ArrayList = ReadNodesSection(sLines, tHeads, sError)
            Dim hAlts As New clsHierarchyStringsParser(tHierarchy, Nothing, False)
            Try
                Dim tNewNodesIDs As New List(Of Guid) 'A1506
                hAlts.Parse(AltsList, tNewNodesIDs, True) 'A1506
            Catch ex As Exception
                sError = "Parse issue"
            End Try
            Return sError
        End Function
        ' D5053 ==

        ' D2426 ===
        Friend Shared Function ReadNodesToHierarchy(sLines As String(), tHeads() As String, ByRef tHierarchy As clsHierarchy, ByRef NodesReadCount As Integer) As String  ' D2452 + D5053
            Dim sError As String = ""
            NodesReadCount = -1
            Dim ObjsList As ArrayList = ReadNodesSection(sLines, tHeads, sError)    ' D2452
            If sError = "" Then

                NodesReadCount = ObjsList.Count
                If ObjsList.Count > 0 Then  ' D2146
                    Dim tGoal As clsNode = Nothing
                    If tHierarchy.Nodes.Count > 0 AndAlso ObjsList.Count > 0 Then
                        Dim tNodes As New List(Of clsNode)
                        ParseObjective(CStr(ObjsList(0)), 0, tNodes) ' D2299
                        If tNodes.Count > 0 Then
                            Dim tNode As clsNode = CType(tNodes(0), clsNode)
                            tHierarchy.Nodes(0).NodeName = tNode.NodeName
                            tHierarchy.Nodes(0).InfoDoc = tNode.InfoDoc
                            tGoal = tHierarchy.Nodes(0)
                            'If tGoal.NodeID = 0 Then tGoal.NodeID = 1   ' D5053 ' force to start NodeID from 1 (0 is default)
                            ObjsList.RemoveAt(0)
                        End If
                    Else
                        If tHierarchy IsNot Nothing AndAlso tHierarchy.Nodes.Count > 0 Then tHierarchy.DeleteNode(tHierarchy.Nodes(0), False) ' D5041 +  D5053
                    End If

                    Dim hObjs As New clsHierarchyStringsParser(tHierarchy, tGoal, True)
                    Try
                        Dim tNewNodesIDs As New List(Of Guid) 'A1506
                        hObjs.Parse(ObjsList, tNewNodesIDs, True) 'A1506
                    Catch ex As Exception
                        sError = "Parse issue"
                    End Try
                End If

            End If
            Return sError
        End Function
        ' D2426 ==

        Public Shared Function ReadModel(ByRef tProject As clsProject, sContent As String, fIsRiskModel As Boolean, ByRef sError As String) As Boolean  ' D2426
            Dim fLoaded As Boolean = False
            If tProject IsNot Nothing AndAlso Not String.IsNullOrEmpty(sContent) Then
                tProject.ProjectManager.PipeParameters.ProjectName = tProject.ProjectName
                If isValidContent(sContent, sError) Then
                    If Option_DetectHypelinks Then sContent = ParseTextHyperlinks(sContent) ' D2756 + D4102
                    Dim sLines As String() = PrepareContent(sContent)

                    ' D2804 ===
                    Dim sComment As New List(Of String)
                    ReadSection(sLines, _TR_HEADER, AddressOf ParseString, sComment)
                    If sComment IsNot Nothing AndAlso sComment.Count > 0 Then
                        Dim tmpComment As String = ""
                        For Each s As String In sComment
                            tmpComment += s
                        Next
                        tProject.Comment = tmpComment
                    End If
                    ' D2804 ==

                    Dim NodesCnt As Integer = -1

                    ' D2426 + D5053 ===
                    If fIsRiskModel Then
                        ' D2452 ===
                        If Not tProject.ProjectManager.IsValidHierarchyID(ECHierarchyID.hidImpact) Then tProject.ProjectManager.AddImpactHierarchy()
                        Dim tHasRisk As Boolean = False
                        sError = ReadNodesToHierarchy(sLines, _TR_IMPACT, tProject.ProjectManager.Hierarchy(ECHierarchyID.hidImpact), NodesCnt)
                        If sError = "" AndAlso NodesCnt > 0 Then tHasRisk = True
                        If sError = "" Then sError = ReadNodesToHierarchy(sLines, _TR_LIKELIHOOD, tProject.ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood), NodesCnt)
                        If sError = "" AndAlso Not tHasRisk AndAlso NodesCnt = 0 Then
                            sError = ReadNodesToHierarchy(sLines, _TR_HIERARCHY, tProject.ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood), NodesCnt)
                        End If
                        ' D2452 ==
                    Else
                        sError = ReadNodesToHierarchy(sLines, _TR_HIERARCHY, tProject.HierarchyObjectives, NodesCnt)  ' D2452
                    End If
                    ' D2426 + D0503 ==

                    If sError <> "" Then
                        sError = "Error on read " + CStr(IIf(fIsRiskModel, "likelihood", "objectives")) + ": " + sError ' D2426
                    Else

                        'Dim AltsList As ArrayList = ReadNodesSection(sLines, _TR_ALTERNATIVES, sError)  ' D2426
                        'If sError = "" Then

                        '    Dim hAlts As New clsHierarchyStringsParser(tProject.HierarchyAlternatives, Nothing, False)
                        '    Try
                        '        Dim tNewNodesIDs As New List(Of Guid) 'A1506
                        '        hAlts.Parse(AltsList, tNewNodesIDs, True) 'A1506
                        '    Catch ex As Exception
                        '        sError = "Parse issue"
                        '    End Try
                        'End If

                        sError = ReadAlternativesToHierarchy(sLines, _TR_ALTERNATIVES, tProject.HierarchyAlternatives)  ' D5053

                        If sError <> "" Then
                            sError = "Error on read alternatives: " + sError
                        Else
                            Dim tUsers As List(Of clsUser) = ReadUsers(sLines, sError)
                            If sError = "" Then
                                For Each tUser As clsUser In tUsers
                                    tProject.ProjectManager.AddUser(tUser)  ' D2136
                                Next
                                fLoaded = True
                            Else
                                sError = "Error on read users list: " + sError
                            End If

                        End If
                    End If

                Else
                    sError = "Error on read project"
                End If
            End If
            Return fLoaded
        End Function

        Public Shared Function isValidContent(sContent As String, ByRef sError As String) As Boolean
            Dim fIsValid As Boolean = False
            sError = ""

            If Not String.IsNullOrEmpty(sContent) Then
                Dim sLines As String() = PrepareContent(sContent)
                If GetSectionIdx(sLines, _TR_HEADER) >= 0 OrElse GetSectionIdx(sLines, _TR_HIERARCHY) >= 0 OrElse GetSectionIdx(sLines, _TR_ALTERNATIVES) >= 0 OrElse GetSectionIdx(sLines, _TR_PARTICIPANTS) >= 0 Then fIsValid = True ' D2146 + D2153
                If Not fIsValid Then sError = String.Format("Wrong format: should contain the model section(s) with '{0}' descriptor.", _TR_PREFIX) ' D2146
            Else
                sError = "Empty content"
            End If

            Return fIsValid
        End Function

#End Region

#Region "Shared Export functions"

        ' D2150 ===
        Private Shared Function GetHierarchyText(tProjectID As Integer, NodesList As List(Of clsNode), sIndent As String) As String
            Dim sRes As String = ""
            If NodesList IsNot Nothing AndAlso NodesList.Count > 0 Then
                For Each tNode As clsNode In NodesList
                    Dim sComment = ""
                    If tNode.InfoDoc <> "" Then
                        If isMHT(tNode.InfoDoc) Then
                            sComment = Infodoc_Unpack(tProjectID, 0, CType(IIf(tNode.IsAlternative, reObjectType.Alternative, reObjectType.Node), reObjectType), CStr(tNode.NodeID), tNode.InfoDoc, True, True, -1)
                            sComment = HTML2Text(sComment)
                        Else
                            sComment = tNode.InfoDoc
                        End If
                        sComment = _TR_DELIMITER + sComment.Replace(vbCrLf, " ").Replace(vbCr, " ").Replace(vbLf, " ").Trim
                    End If
                    sRes += String.Format("{0}{1}{2}{3}", sIndent, tNode.NodeName, sComment, _TR_SPLITTER)
                    If Not tNode.IsAlternative AndAlso Not tNode.IsTerminalNode Then
                        Dim UID As Integer = COMBINED_USER_ID
                        If tNode.Hierarchy.ProjectManager.User IsNot Nothing Then UID = tNode.Hierarchy.ProjectManager.User.UserID
                        Dim sChilds As List(Of clsNode) = tNode.GetNodesBelow(UID)
                        If sChilds IsNot Nothing AndAlso sChilds.Count > 0 Then sRes += GetHierarchyText(tProjectID, sChilds, sIndent + "  ")
                    End If
                Next
            End If
            Return sRes
        End Function

        Public Shared Function GetModelStructure(tProject As clsProject, fIsRiskModel As Boolean, Optional IsJSExport As Boolean = False) As String
            Dim sModel As String = ""
            If tProject IsNot Nothing AndAlso tProject.isValidDBVersion Then
                Dim sCommon As String = _TR_HEADER(0) + _TR_SPLITTER
                sCommon = String.Format("{0}{2}Project: {3}{1}", sCommon, _TR_SPLITTER, _TR_COMMENT, tProject.ProjectName)
                If tProject.Comment <> "" Then sCommon = String.Format("{0}{2}Description: {3}{1}", sCommon, _TR_SPLITTER, _TR_COMMENT, tProject.Comment)

                ' D2425 ===
                Dim sObjs As String = CStr(IIf(fIsRiskModel, _TR_LIKELIHOOD(0), _TR_HIERARCHY(0))) + _TR_SPLITTER + GetHierarchyText(tProject.ID, tProject.ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).GetLevelNodes(0), "")  ' D2452

                If fIsRiskModel AndAlso tProject.ProjectManager.IsValidHierarchyID(ECHierarchyID.hidImpact) Then
                    sObjs += _TR_SPLITTER + _TR_SPLITTER + _TR_IMPACT(0) + _TR_SPLITTER + GetHierarchyText(tProject.ID, tProject.ProjectManager.Hierarchy(ECHierarchyID.hidImpact).GetLevelNodes(0), "")
                End If
                ' D2425 ==

                Dim sAlts As String = CStr(IIf(fIsRiskModel, _TR_ALTERNATIVES(_TR_ALTERNATIVES.Length - 2), _TR_ALTERNATIVES(0))) + _TR_SPLITTER + GetHierarchyText(tProject.ID, tProject.HierarchyAlternatives.Nodes, "")

                Dim sUsers As String = _TR_PARTICIPANTS(0) + _TR_SPLITTER
                For Each tUser As clsUser In tProject.ProjectManager.UsersList
                    sUsers += tUser.UserEMail + CStr(IIf(tUser.UserName <> "", _TR_DELIMITER + tUser.UserName, "")) + _TR_SPLITTER
                Next

                sModel = String.Format("{1}{0}{2}{0}{3}{0}{4}", _TR_SPLITTER + _TR_SPLITTER, sCommon, sObjs, sAlts, sUsers)
            End If
            If IsJSExport Then sModel = sModel.Replace(_TR_SPLITTER, vbCr + vbLf)
            Return sModel
        End Function
        ' D2150 ==

#End Region

    End Class

End Namespace
