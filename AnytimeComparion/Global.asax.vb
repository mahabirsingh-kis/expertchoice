Imports System.Web.Optimization
Imports ExpertChoice.Data
Imports ExpertChoice.Web

Public Class Global_asax
    Inherits HttpApplication

    Public Shared ServerError As String

    Sub Application_Start(sender As Object, e As EventArgs)
        ' Fires when the application is started
        RouteConfig.RegisterRoutes(RouteTable.Routes)
        BundleConfig.RegisterBundles(BundleTable.Bundles)
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
        _Login.LoadLanguange()
        WebOptions.LoadComparionCoreOptions(App)
        Dim sessionUID As String = "UID"
        Dim uId As String = CStr(context.Session(sessionUID))

        If String.IsNullOrEmpty(uId) Then
            uId = If(context.Request.Cookies(_COOKIE_UID) Is Nothing, "", context.Request.Cookies(_COOKIE_UID).Value)
        End If

        If String.IsNullOrEmpty(uId) Then
            uId = ExpertChoice.Service.StringFuncs.GetRandomString(6, True, False)
            context.Session.Add(sessionUID, uId)
            Dim uIdCookie As HttpCookie = New HttpCookie(_COOKIE_UID, uId) With {
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