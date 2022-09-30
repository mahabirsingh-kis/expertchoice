Partial Class HelpAuthWebAPI
    Inherits clsComparionCorePage

    'Public Const _KO_URL_Auth As String = "https://app.knowledgeowl.com/api/head/remotelogin.json"
    Public Const _KO_URL_Token As String = "https://app.knowledgeowl.com/oauth2/token"              ' D6234
    Public Const _KO_URL_Help As String = "https://comparion.knowledgeowl.com/"                     ' D7172
    Public Const _KO_URL_Help_Riskion As String = "https://riskion.knowledgeowl.com/"               ' D7172
    Public Const _KO_APIKey As String = "5d24808fad121c5243d01e3c"
    Public Const _KO_PrjID As String = "5c775aa76e121c7c09b9dd85-5c775b388e121c822d196668"
    Public Const _KO_PrjID_Riskion As String = "5c775aa76e121c7c09b9dd85-60175454ec161c6062579688"  ' D7172
    Public Const _KO_BaseID As String = "5c775b388e121c822d196668"
    Public Const _KO_ClientID As String = "5c775b388e121c822d196668"                                ' D6234
    Public Const _KO_ClientSecret As String = "9dbaffdc6cd182cab7a696b5116fe8eadbfc7d669dda5fdf"    ' D6234

    Const _SESS_KO_HELP_TOKEN As String = "KO_Token"

    Shared _KO_Token As String = Nothing
    Shared _KO_Error As String = ""

    Public Sub New()
        MyBase.New(_PGID_WEBAPI)
    End Sub

    Private Function _Page() As mpWebAPI
        Return CType(Master, mpWebAPI)
    End Function

    Shared Function GetHelpPath(isRiskion As Boolean) As String
        Return If(isRiskion, HelpAuthWebAPI._KO_URL_Help_Riskion, HelpAuthWebAPI._KO_URL_Help)
    End Function

    Shared Function KO_GetToken(App As clsComparionCore, Session As HttpSessionState) As String
        If App IsNot Nothing AndAlso Session IsNot Nothing AndAlso App.isAuthorized AndAlso _KO_Token Is Nothing Then

            If Session(_SESS_KO_HELP_TOKEN) IsNot Nothing Then
                _KO_Token = CStr(Session(_SESS_KO_HELP_TOKEN))
            End If

            If String.IsNullOrEmpty(_KO_Token) Then
                _KO_Token = ""
                _KO_Error = ""

                Dim sEmail As String = App.ActiveUser.UserEmail
                Dim sName As String = If(App.ActiveUser.UserName = "", App.ActiveUser.UserEmail, App.ActiveUser.UserName)
                Dim sGroups As String = String.Format("{0},{1}", If(App.isRiskEnabled, "riskion", "comparion"), If(App.CanUserCreateNewProject, "pm", "eval"))

                Dim strResponse As String = Nothing
                Dim client As New WebClient()
                client.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(String.Format("{0}:{1}", _KO_ClientID, _KO_ClientSecret))))

                Dim postData = New NameValueCollection()
                postData.Add("grant_type", "client_credentials")
                'postData.Add("reader[ssoid]", sEmail)       ' D6273
                'postData.Add("reader[username]", sName)     ' D6273
                'postData.Add("reader[Groups]", sGroups)     ' D6273

                Dim sURL As String = String.Format("{0}", _KO_URL_Token)

                Dim tResponse As Byte() = {}
                Try
                    tResponse = client.UploadValues(sURL, "POST", postData)
                Catch ex As Exception
                    _KO_Error = ex.Message
                    If ex IsNot Nothing Then
                        Dim R As WebResponse = CType(ex, WebException).Response
                        If R IsNot Nothing Then
                            With New IO.StreamReader(R.GetResponseStream)
                                _KO_Error += vbNewLine + " " + .ReadToEnd()
                            End With
                        End If
                    End If
                End Try
                If tResponse IsNot Nothing Then strResponse = Encoding.ASCII.GetString(tResponse)

                If Not String.IsNullOrEmpty(strResponse) Then
                    Dim resp As AuthTokenResponseObj = JsonConvert.DeserializeObject(Of AuthTokenResponseObj)(strResponse)
                    If Not String.IsNullOrEmpty(resp.access_token) Then _KO_Token = resp.access_token
                End If
            End If

            Session(_SESS_KO_HELP_TOKEN) = _KO_Token
        End If

        If String.IsNullOrEmpty(_KO_Token) AndAlso String.IsNullOrEmpty(_KO_Error) AndAlso Not App.isAuthorized Then _KO_Error = "User is not authorized"

        Return If(_KO_Token Is Nothing, "", _KO_Token)
    End Function

    Public Function GetToken() As String
        Return KO_GetToken(App, Session)
    End Function

    ' -D6234
    'Public Function GetAuthURL() As String
    '    Dim sURL As String = ""
    '    Dim sToken As String = GetToken()
    '    If Not String.IsNullOrEmpty(sToken) Then
    '        sURL = String.Format("{0}remote-auth?n={1}", _KO_URL_Help, sToken)
    '    End If
    '    Return sURL
    'End Function

    Private Sub KOHelpWebAPI_Load(sender As Object, e As EventArgs) Handles Me.Load
        Select Case _Page.Action
            'Case ""
            '    Dim sURL As String = GetAuthURL()
            '    If Not String.IsNullOrEmpty(sURL) Then
            '        Response.Redirect(sURL, True)
            '    End If
            '    _Page.ResponseData = If(String.IsNullOrEmpty(_KO_Error), "Unable to get token", _KO_Error)
            Case "", "gettoken"
                _Page.ResponseData = New jActionResult() With {
                    .Data = GetToken(),
                    .Message = _KO_Error,
                    .Result = If(String.IsNullOrEmpty(_KO_Error) AndAlso Not String.IsNullOrEmpty(CStr(.Data)), ecActionResult.arSuccess, ecActionResult.arError)
                }
                'Case "getauthurl"
                '    _Page.ResponseData = GetAuthURL()
        End Select
    End Sub

    Private Class AuthTokenResponseObj
        Public Property access_token As String
        Public Property expires_in As Object
        Public Property token_type As String
        Public Property scope As Object
    End Class

End Class