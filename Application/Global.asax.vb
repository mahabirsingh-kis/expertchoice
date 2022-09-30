Imports Options = ExpertChoice.Web.Options
Public Class Global_asax
    Inherits HttpApplication

    Sub Application_Start(sender As Object, e As EventArgs)
    End Sub

    Protected Sub Application_PostAuthorizeRequest()
    End Sub

    Private Sub Global_asax_BeginRequest(sender As Object, e As EventArgs) Handles Me.BeginRequest
        If Request.HttpMethod = "POST" AndAlso Request.ContentLength < 30000000 Then
            ' Sanitizing Form request values for Anti-XSS
            EcSanitizer.SetSafeNameValueCollection(Request.Form)
        End If
    End Sub

    Private _requestCount As Integer = 0
    Sub Application_EndRequest(ByVal sender As Object, ByVal e As EventArgs)
        For Each iKey As String In Response.Cookies.AllKeys
            If (iKey.ToUpper().Equals("ASP.NET_SESSIONID")) Then
                Response.Cookies(iKey).Secure = Request.IsSecureConnection
            End If
        Next

        Try
            _requestCount = _requestCount + 1
            If (_requestCount Mod 20 = 0) Then
                _requestCount = 0
                GC.Collect()
                'GC.WaitForPendingFinalizers()
            End If
        Catch ex As Exception
        End Try
    End Sub

    Protected Sub Session_Start(ByVal sender As Object, ByVal e As EventArgs)
        Dim App As clsComparionCore = New clsComparionCore()

        If App.Options Is Nothing Then
            App.Options = New clsComparionCoreOptions()
        End If

        Dim context As HttpContext = HttpContext.Current

        If context IsNot Nothing AndAlso context.Session IsNot Nothing Then
            context.Session("App") = App
            System.Diagnostics.Trace.Write(vbLf & "App is initialized!")
        Else
            System.Diagnostics.Trace.Write(vbLf & "Context is null!")
        End If

        App.Options.CanvasMasterDBName = ConfigurationManager.AppSettings.[Get]("SQLMasterDB")
        App.Options.CanvasProjectsDBName = App.Options.CanvasMasterDBName
        ExpertChoice.Database.ConnectionStringSection.GlobalDefaultProvider = ConfigurationManager.AppSettings.[Get]("SQLDefaultProvider")
        System.Diagnostics.Trace.Write(App.CanvasMasterDBVersion)
        If Not App.isCanvasMasterDBValid OrElse Not App.isCanvasProjectsDBValid Then System.Diagnostics.Trace.Write(String.Format("Error: Database can't be found! Details: {0}", App.Database.LastError))
        '_Default.LoadLanguange()
        WebOptions.LoadComparionCoreOptions(App)
        Dim sessionUID As String = "UID"
        Dim uId As String = CStr(context.Session(sessionUID))

        If String.IsNullOrEmpty(uId) Then
            uId = If(context.Request.Cookies(Options._COOKIE_UID) Is Nothing, "", context.Request.Cookies(Options._COOKIE_UID).Value)
        End If

        If String.IsNullOrEmpty(uId) Then
            uId = ExpertChoice.Service.StringFuncs.GetRandomString(6, True, False)
            context.Session.Add(sessionUID, uId)
            Dim uIdCookie As HttpCookie = New HttpCookie(Options._COOKIE_UID, uId) With {
                .HttpOnly = True,
                .Expires = DateTime.Now.AddDays(1)
            }

            If FormsAuthentication.RequireSSL AndAlso context.Request.IsSecureConnection Then
                uIdCookie.Secure = True
            End If

            context.Response.Cookies.Add(uIdCookie)
        Else
        End If

        If context.Request IsNot Nothing Then
        End If

        App.Options.SessionID = $"{uId}#{context.Session.SessionID.Substring(0, 4)}"
    End Sub

End Class