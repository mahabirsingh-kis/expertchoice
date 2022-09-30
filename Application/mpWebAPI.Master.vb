Public Class mpWebAPI
    Inherits clsMasterPageBase

    ' D5026 ===
    Private _Params As NameValueCollection = Nothing
    Private _Action As String = Nothing

    Public ResponseData As Object = Nothing

    Public ReadOnly Property Params As NameValueCollection
        Get
            If _Params Is Nothing Then _Params = _Page.ParseJSONParams(_Page.CheckVar("params", ""))    ' D6575
            Return _Params
        End Get
    End Property

    Public ReadOnly Property Action As String
        Get
            If _Action Is Nothing Then
                If Params IsNot Nothing AndAlso Not String.IsNullOrEmpty(_PARAM_ACTION) AndAlso Params(_PARAM_ACTION) IsNot Nothing Then
                    _Action = Params(_PARAM_ACTION).ToLower
                Else
                    If Request IsNot Nothing AndAlso Request(_PARAM_ACTION) IsNot Nothing Then _Action = CStr(Request(_PARAM_ACTION)).ToLower
                End If
            End If
            Return _Action
        End Get
    End Property

    Private Sub mpWebAPI_PreRender(sender As Object, e As EventArgs) Handles Me.PreRender
        If ResponseData IsNot Nothing Then
            ' D6994 ===
            If TypeOf ResponseData Is jActionResult AndAlso Not String.IsNullOrEmpty(CType(ResponseData, jActionResult).Message) Then
                CType(ResponseData, jActionResult).Message = _Page.ParseString(CType(ResponseData, jActionResult).Message)
            End If
            ' D6994 ==
            _Page.SendResponseJSON(ResponseData)
        Else
            If Not String.IsNullOrEmpty(Action) Then _Page.FetchNoMethod(False, String.Format("METHOD_NOT_FOUND_({0})", RemoveHTMLTags(Action)))
        End If
        _Page.ResponseError(HttpStatusCode.NotImplemented, "NOTHING_TO_REPLY")
    End Sub
    ' D5026 ==

End Class