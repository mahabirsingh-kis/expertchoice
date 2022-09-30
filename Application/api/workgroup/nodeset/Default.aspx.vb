Partial Class NodeSetWebAPI
    Inherits clsComparionCorePage

    Public Sub New()
        MyBase.New(_PGID_WEBAPI)
    End Sub

    Private Function _Page() As mpWebAPI
        Return CType(Master, mpWebAPI)
    End Function

    Public Function List(HID As ecNodeSetHierarchy) As jActionResult
        Dim sMsg As String = ""
        Dim tLst As List(Of clsNodeSet) = App.NodeSets_GetList(HID, True, sMsg)
        Dim Items As New List(Of jNodeSet)
        If tLst IsNot Nothing AndAlso sMsg = "" Then
            For Each tSet As clsNodeSet In tLst
                Dim tItem As jNodeSet = jNodeSet.CreateFromBaseObject(tSet)
                Items.Add(tItem)
            Next
        End If
        Return New jActionResult With {
            .Result = If(String.IsNullOrEmpty(sMsg), ecActionResult.arSuccess, ecActionResult.arError),
            .ObjectID = HID,
            .Message = sMsg,
            .Data = Items
            }
    End Function

    Public Function Restore_Defaults(HID As ecNodeSetHierarchy) As jActionResult
        Dim sMsg As String = ""
        App.NodeSets_RestoreDefaults(HID, True, sMsg)
        Return New jActionResult With {
            .Result = If(String.IsNullOrEmpty(sMsg), ecActionResult.arSuccess, ecActionResult.arError),
            .ObjectID = HID,
            .Message = sMsg
            }
    End Function

    Private Function GetHID() As ecNodeSetHierarchy
        Dim HID As Integer = CInt(ecNodeSetHierarchy.hidObjectives)
        Integer.TryParse(GetParam(_Page.Params, "hid", True), HID)
        Select Case HID
            Case ecNodeSetHierarchy.hidObjectives, ecNodeSetHierarchy.hidAlternatives, ecNodeSetHierarchy.hidLikelihood, ecNodeSetHierarchy.hidImpact, ecNodeSetHierarchy.hidControls, ecNodeSetHierarchy.hidEvents
            Case Else
                HID = CInt(ecNodeSetHierarchy.hidObjectives)
        End Select
        Return CType(HID, ecNodeSetHierarchy)
    End Function

    Private Sub NodeSetWebAPI_Load(sender As Object, e As EventArgs) Handles Me.Load
        FetchIfNotAuthorized()
        Select Case _Page.Action

            Case "restore_defaults"
                _Page.ResponseData = Restore_Defaults(GetHID())

            Case "list"
                _Page.ResponseData = List(GetHID())

        End Select
    End Sub

End Class