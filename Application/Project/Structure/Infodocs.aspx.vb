Partial Class InfodocsPage
    Inherits clsComparionCorePage

    Public Sub New()
        MyBase.New(_PGID_STRUCTURE_INFODOCS)
    End Sub
    ReadOnly Property PM As clsProjectManager
        Get
            Return App.ActiveProject.ProjectManager
        End Get
    End Property

    Public Function GetRows() As String
        Dim UserIDs As New List(Of Integer)
        UserIDs.Add(-1)
        Return datagrid_GetRows(PM, UserIDs, , , False)
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

    Public Function GetWRTInfodocs() As String
        Dim retVal As String = ""
        Dim i As Integer = 0
        Dim j As Integer = 0
        For Each obj As clsNode In PM.Hierarchy(PM.ActiveHierarchy).TerminalNodes   ' D3925
            i = 0
            For Each alt As clsNode In PM.AltsHierarchies(0).Nodes
                Dim aWRTInfodoc As String = PM.InfoDocs.GetNodeWRTInfoDoc(alt.NodeGuidID, obj.NodeGuidID)
                If aWRTInfodoc <> "" Then
                    retVal += CStr(IIf(retVal <> "", ",", "")) + String.Format("{{aid:{0}, oid:{1}}}", alt.NodeID, obj.NodeID)
                End If
                i += 1
            Next
            j += 1
        Next
        Return String.Format("[{0}]", retVal)
    End Function

    Public Function GetNodeInfodocs() As String
        Dim retVal As String = ""
        For Each obj As clsNode In PM.Hierarchy(PM.ActiveHierarchy).Nodes() ' D3925
            If obj.InfoDoc <> "" Then
                retVal += CStr(IIf(retVal <> "", ",", "")) + String.Format("{{nodeGuid:'{0}'}}", obj.NodeGuidID)
            End If
        Next
        For Each alt As clsNode In PM.AltsHierarchies(0).Nodes
            If alt.InfoDoc <> "" Then
                retVal += CStr(IIf(retVal <> "", ",", "")) + String.Format("{{nodeGuid:'{0}'}}", alt.NodeGuidID)
            End If
        Next
        Return String.Format("[{0}]", retVal)
    End Function

    'A1205 ===
    Protected Sub Ajax_Callback(data As String)
        Dim args As NameValueCollection = HttpUtility.ParseQueryString(data)
        Dim sAction As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "action")).ToLower ' Anti-XSS
        Dim tResult As String = CStr(IIf(String.IsNullOrEmpty(sAction), "", String.Format("['{0}','']", sAction)))

        Select Case sAction            
            Case "select_columns"
                Dim tColumnsIDs As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "column_ids"))
                SelectedColumns(App.ActiveProject) = tColumnsIDs
                tResult = String.Format("['{0}','']", "updated")
            Case "contributed_nodes"
                Dim EventID As Integer = CInt(EcSanitizer.GetSafeHtmlFragment(GetParam(args, "event_id")).ToLower)      ' Anti-XSS
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
        End Select

        If tResult <> "" Then            
            Response.Clear()
            Response.ContentType = "text/plain"
            Response.Write(tResult)
            Response.End()
        End If
    End Sub

    Protected Sub Page_InitComplete(sender As Object, e As EventArgs) Handles Me.InitComplete
        Ajax_Callback(Request.Form.ToString)
    End Sub
    'A1205 ==

End Class
