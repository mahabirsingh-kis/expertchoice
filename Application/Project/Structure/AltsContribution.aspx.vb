Option Strict Off

Partial Class AltsContributionPage
    Inherits clsComparionCorePage

    Public Sub New()
        MyBase.New(_PGID_STRUCTURE_ALTSCONTRIB)
    End Sub
    ReadOnly Property PM As clsProjectManager
        Get
            Return App.ActiveProject.ProjectManager
        End Get
    End Property

    Public ReadOnly Property IDColumnMode As String
        Get
            Dim tSess As Object = SessVar("IDColumnMode" + App.ActiveProject.ID.ToString)
            If String.IsNullOrEmpty(tSess) OrElse CStr(tSess) = "0" Then Return "id" Else Return "index"
        End Get
    End Property
    Public Function GetRows() As String
        Dim UserIDs As New List(Of Integer)
        UserIDs.Add(-1)
        Return datagrid_GetRows(PM, UserIDs)
    End Function

    Public Function GetColumns() As String
        Dim UserIDs As New List(Of Integer)
        UserIDs.Add(-1)
        Return datagrid_GetColumns(PM, UserIDs, True)
    End Function

    Public Function GetHierarchyColumns() As String
        Return datagrid_GetHierarchyColumns(PM)
    End Function

    'A1201 ===
    Public Function GetAttributes() As String
        Return datagrid_GetAttributes(PM, App)
    End Function
    'A1201 ==

    Protected Sub Ajax_Callback(data As String)
        Dim args As NameValueCollection = HttpUtility.ParseQueryString(data)
        Dim sAction As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "action")).ToLower ' Anti-XSS
        Dim tResult As String = CStr(IIf(String.IsNullOrEmpty(sAction), "", String.Format("['{0}','']", sAction)))
        Dim sSnapshotMessage As String = "Set contribution(s)" 'A1202
        Dim sSnapshotComment As String = "" 'A1202

        Select Case sAction
            Case "setcol"
                Dim sColID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "col_id")).ToLower  ' Anti-XSS
                Dim sVal As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "val")).ToLower   ' Anti-XSS
                Dim ColID As Integer = CInt(sColID)
                Dim Val As Boolean = CBool(sVal)
                Dim obj = PM.Hierarchy(PM.ActiveHierarchy).GetNodeByID(ColID)
                Dim TerminalNodes As New List(Of Guid)
                If obj IsNot Nothing Then
                    If obj.IsTerminalNode Then
                        TerminalNodes.Add(obj.NodeGuidID)
                    Else
                        For Each respectiveNode As clsNode In PM.Hierarchy(PM.ActiveHierarchy).GetRespectiveTerminalNodes(obj)
                            TerminalNodes.Add(respectiveNode.NodeGuidID)
                        Next
                    End If
                End If

                'For Each node As clsNode In TerminalNodes
                '    node.ChildrenAlts.Clear()
                '    If Val Then
                '        For Each alt In PM.AltsHierarchy(PM.ActiveAltsHierarchy).Nodes
                '            node.ChildrenAlts.Add(alt.NodeID)
                '        Next
                '    End If
                'Next
                Dim altIDs As New List(Of Guid)
                For Each alt As clsNode In PM.AltsHierarchy(PM.ActiveAltsHierarchy).Nodes
                    altIDs.Add(alt.NodeGuidID)
                Next

                If PM.UpdateContributions(TerminalNodes, altIDs, Val) Then
                    sSnapshotComment = String.Format("Set {0} contributions for {1} terminal nodes and {2} alternatives", Bool2JS(Val), TerminalNodes.Count, altIDs.Count)
                    tResult = String.Format("['{0}','']", "ok")
                End If

            Case "setrow"
                Dim sRowID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "row_id")).ToLower  ' Anti-XSS
                Dim sVal As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "val")).ToLower   ' Anti-XSS
                Dim RowID As Integer = CInt(sRowID)
                Dim Val As Boolean = CBool(sVal)

                Dim Alt As clsNode = PM.AltsHierarchy(PM.ActiveAltsHierarchy).GetNodeByID(RowID)
                If Alt IsNot Nothing Then
                    Dim CovObjIDs As New List(Of Guid)
                    If Val Then
                        For Each Node As clsNode In PM.Hierarchy(PM.ActiveHierarchy).TerminalNodes
                            CovObjIDs.Add(Node.NodeGuidID)
                        Next
                    End If

                    PM.UpdateContributions(Alt.NodeGuidID, CovObjIDs, PM.ActiveHierarchy, False)
                End If
                sSnapshotComment = String.Format("Set all {0} for altID: {1}", Bool2JS(Val), RowID)
                tResult = String.Format("['{0}','']", "ok")
            Case "setcell"
                Dim sRowID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "row_id")).ToLower  ' Anti-XSS
                Dim sColID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "col_id")).ToLower  ' Anti-XSS
                Dim sVal As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "val")).ToLower   ' Anti-XSS

                Dim Val As Boolean = CBool(sVal)
                Dim Alt As clsNode = PM.ActiveAlternatives.GetNodeByID(New Guid(sRowID))
                Dim CovObj As clsNode = PM.ActiveObjectives.GetNodeByID(New Guid(sColID))

                PM.UpdateContributions(New List(Of Guid) From {CovObj.NodeGuidID}, New List(Of Guid) From {Alt.NodeGuidID}, New List(Of Boolean) From {Val})

                sSnapshotComment = String.Format("Set {0} for altID: {1} vs. {2}", Bool2JS(Val), New Guid(sRowID), CovObj.NodeName)
                tResult = String.Format("['{0}','']", "ok")
            Case "setall"
                Dim sVal As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "val")).ToLower   ' Anti-XSS
                Dim Val As Boolean = CBool(sVal)
                Dim CovObjIDs As New List(Of Guid)

                If Val Then
                    For Each obj In PM.Hierarchy(PM.ActiveHierarchy).TerminalNodes
                        CovObjIDs.Add(obj.NodeGuidID)
                    Next
                End If

                For Each alt In PM.AltsHierarchy(PM.ActiveAltsHierarchy).Nodes    ' D3925
                    PM.UpdateContributions(alt.NodeGuidID, CovObjIDs, PM.ActiveHierarchy, False)
                Next
                sSnapshotComment = String.Format("Set ALL {0}", Bool2JS(Val))
                tResult = String.Format("['{0}','']", "ok")
            Case "setrange" 'A1200 ===
                Dim nodeIDs As String() = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "nodeids").ToLower).Split(CChar(";"))
                Dim chk As Boolean = CBool(EcSanitizer.GetSafeHtmlFragment(GetParam(args, "val").ToLower))
                Dim AltIDs As New List(Of Integer)
                For Each node As String In nodeIDs
                    Dim nodeAr = node.Split(CChar(","))
                    Dim altID = CInt(nodeAr(0))
                    If Not AltIDs.Contains(altID) Then
                        AltIDs.Add(altID)
                    End If
                Next

                For Each altID As Integer In AltIDs
                    Dim Alt As clsNode = PM.AltsHierarchy(PM.ActiveAltsHierarchy).GetNodeByID(altID)
                    If Alt IsNot Nothing Then
                        Dim CovObjIDs As New List(Of Guid)
                        For Each Node As clsNode In PM.Hierarchy(PM.ActiveHierarchy).TerminalNodes
                            If Node.GetContributedAlternatives.Contains(Alt) Then
                                CovObjIDs.Add(Node.NodeGuidID)
                            End If
                        Next

                        For Each node As String In nodeIDs
                            Dim nodeAr = node.Split(CChar(","))
                            If altID = CInt(nodeAr(0)) Then
                                Dim colIndex = CInt(nodeAr(1))
                                Dim obj = PM.Hierarchy(PM.ActiveHierarchy).GetNodeByID(colIndex)
                                If chk AndAlso Not CovObjIDs.Contains(obj.NodeGuidID) Then
                                    CovObjIDs.Add(obj.NodeGuidID)
                                End If
                                If Not chk AndAlso CovObjIDs.Contains(obj.NodeGuidID) Then
                                    CovObjIDs.Remove(obj.NodeGuidID)
                                End If
                            End If
                        Next
                        PM.UpdateContributions(Alt.NodeGuidID, CovObjIDs, PM.ActiveHierarchy, False)
                    End If
                Next
                sSnapshotComment = String.Format("Set range {0}", Bool2JS(chk))
                tResult = String.Format("['{0}','']", "ok")
                'A1200 ==
                'A1201 ===
            Case "select_columns"
                Dim tColumnsIDs As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "column_ids"))
                SelectedColumns(App.ActiveProject) = tColumnsIDs
                'tResult = "'reload'"
                tResult = String.Format("['{0}','']", "updated")
                'A1201 ==
            Case "contributed_nodes"
                Dim EventID As String = CInt(EcSanitizer.GetSafeHtmlFragment(GetParam(args, "event_id")).ToLower) ' Anti-XSS
                Dim alt = PM.AltsHierarchy(PM.ActiveAltsHierarchy).Nodes(EventID)
                Dim retVal As String = ""
                Dim H As clsHierarchy = Nothing
                If App.isRiskEnabled Then
                    If PM.ActiveHierarchy = ECHierarchyID.hidLikelihood Then H = PM.Hierarchy(ECHierarchyID.hidImpact)
                    If PM.ActiveHierarchy = ECHierarchyID.hidImpact Then H = PM.Hierarchy(ECHierarchyID.hidLikelihood)
                Else
                    H = PM.Hierarchy(PM.ActiveHierarchy)
                End If
                For Each obj In H.TerminalNodes
                    If obj.GetContributedAlternatives.Contains(alt) Then
                        retVal += CStr(IIf(retVal = "", "", ",")) + String.Format("[{0},'{1}']", obj.NodeID, JS_SafeString(obj.NodeName))
                    End If
                Next
                tResult = String.Format("['{0}',[{1}]]", "contributed_nodes", retVal)
            Case "setresource"
                Dim sProjectID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "project_id")).ToLower  ' Anti-XSS
                Dim sResID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "res_id")).ToLower  ' Anti-XSS
                Dim sPeriodID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "period_id")).ToLower    ' Anti-XSS
                Dim sVal As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "val")).ToLower   ' Anti-XSS
                tResult = String.Format("['{0}','']", "ok")
            Case "delete_event"
                Dim sAltID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "event_id")) ' Anti-XSS                
                Dim AltID As Integer = CInt(sAltID)
                Dim AltH As clsHierarchy = PM.AltsHierarchy(PM.ActiveAltsHierarchy)
                Dim Alt As clsNode = AltH.GetNodeByID(AltID)
                If Alt IsNot Nothing Then
                    sSnapshotMessage = "Delete " + ParseString("%%alternative%%")
                    sSnapshotComment = String.Format("Delete ""{0}""", Alt.NodeName)
                    AltH.DeleteNode(Alt, False)
                    tResult = String.Format("['{0}','']", "ok")
                End If
            Case "update_event_name"
                Dim sAltID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "event_id"))  ' Anti-XSS                
                Dim AltID As Integer = CInt(sAltID)
                Dim sAltName As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "val"))  ' Anti-XSS                
                Dim AltH As clsHierarchy = PM.AltsHierarchy(PM.ActiveAltsHierarchy)
                Dim Alt As clsNode = AltH.GetNodeByID(AltID)
                If Alt IsNot Nothing Then
                    sSnapshotMessage = "Update " + ParseString("%%alternative%%") + " name"
                    sSnapshotComment = String.Format("Update ""{0}"" name", Alt.NodeName)
                    Alt.NodeName = sAltName
                    tResult = String.Format("['{0}','']", "ok")
                End If
        End Select

        If tResult <> "" Then
            If tResult.StartsWith("['ok',") Then
                App.ActiveProject.SaveStructure(sSnapshotMessage, True, True, sSnapshotComment) 'A1202 'If tResult = "'ok'" Then PM.StorageManager.Writer.SaveProject(True,)
            End If
            Response.Clear()
            Response.ContentType = "text/plain"
            Response.Write(tResult)
            Response.End()
        End If
    End Sub

    Protected Sub Page_InitComplete(sender As Object, e As EventArgs) Handles Me.InitComplete
        Ajax_Callback(Request.Form.ToString)
    End Sub
    
    Public Function ShowContributions() As Boolean
        If PM.ActiveHierarchy = ECHierarchyID.hidImpact And PM.ActiveObjectives.Nodes.Count = 1 Then
            Return False
        End If
        Return True
    End Function

    Private Sub Page_Init(sender As Object, e As EventArgs) Handles Me.Init
        Dim tHid As integer = CheckVar("hid", PM.ActiveHierarchy)
        If tHid <> PM.ActiveHierarchy Then
            PM.ActiveHierarchy = tHid
            App.ActiveProject.ProjectManager.PipeParameters.CurrentParameterSet = If(tHid = ECHierarchyID.hidLikelihood, App.ActiveProject.ProjectManager.PipeParameters.DefaultParameterSet, App.ActiveProject.ProjectManager.PipeParameters.ImpactParameterSet)
        End If

        'Dim tPgid As Integer = CheckVar("pgid", 0)
        'Select Case tPgid
        '    Case _PGID_STRUCTURE_SOURCES_CONTRIBUTIONS, _PGID_STRUCTURE_OBJECTIVES_CONTRIBUTIONS
        '        NavigationPageID = tPgid
        'End Select
    End Sub

End Class