Public Class CombinedGroupsWebAPI
    Inherits clsComparionCorePage

    Public Sub New()
        MyBase.New(_PGID_WEBAPI)
    End Sub

    Private Function _Page() As mpWebAPI
        Return CType(Master, mpWebAPI)
    End Function

    Private ReadOnly Property PRJ As clsProject
        Get
            Return App.ActiveProject
        End Get
    End Property

    Private ReadOnly Property PM As clsProjectManager
        Get
            If App.HasActiveProject Then Return App.ActiveProject.ProjectManager Else Return Nothing
        End Get
    End Property

    Public Function List() As jActionResult
        Return New jActionResult With {
            .Result = ecActionResult.arError,
            .Message = "Not implemented"
            }
    End Function

    Shared Function List_JSON(PM As clsProjectManager) As String
        Dim sList As String = ""
        If PM IsNot Nothing Then
            For Each group As clsCombinedGroup In PM.CombinedGroups.GroupsList
                If group.CombinedUserID <> -1 Then
                    sList += String.Format("{0}{{'id': {1}, 'title': '{2}'}}", If(sList = "", "", ", "), group.ID, JS_SafeString(group.Name))
                End If
            Next
        End If
        Return sList
    End Function

    Private Sub CombinedGroupsWebAPI_Load(sender As Object, e As EventArgs) Handles Me.Load
        FetchIfNoActiveProject()

        Select Case _Page.Action

            Case "list"
                _Page.ResponseData = List()

                'Case "list_json"
                '    _Page.ResponseData = List_JSON()

                ' example
                'Case "byid"
                '    Dim ID As Integer = -1
                '    Integer.TryParse(GetParam(_Page.Params, "id", True), ID)
                '    _Page.ResponseData = ByID(ID)

        End Select
    End Sub

End Class