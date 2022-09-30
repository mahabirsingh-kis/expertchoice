Imports System.Diagnostics
Imports TTAHttp

Partial Class LaunchTeamTimeClient
    Inherits clsComparionCorePage

    Public Sub New()
        MyBase.New(_PGID_TEAMTIME)
    End Sub

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load

        ' D2713 ===
        If CheckVar("action", "") = "xpinstall" Then
            RawResponseStart()
            'Response.AppendHeader("Content-Disposition", "inline; filename=""ClickOnce.xpi""")
            Response.ContentType = "application/x-xpinstall"
            'Response.AppendHeader("Content-Length", "23350")
            Response.TransmitFile(Server.MapPath("..\..\TTA\ClickOnce.xpi"))
            RawResponseEnd()
        End If
        ' D2713 ==

        Dim br As String = Request.Browser.Type.ToLower ' D2634
        'Dim supporterBrowser As Boolean = isIE(Request) OrElse (br.Contains("firefox") AndAlso Request.Browser.ClrVersion.ToString >= "3.5")    ' D2634 + D2635 + D2659 + D2713
        Dim supporterBrowser As Boolean = isIE(Request) ' D2634 + D2635 + D2659 + D2710 + D3807

        If supporterBrowser OrElse CheckVar("action", "") = "force" Then    ' D2634
            Dim url As String = ""
            Try

                Dim QueryString As String = URLDecode(Request.QueryString.ToString.Replace("&action=force", ""))    ' D2635

                Dim InResponseParams As New QueryEncrypt(QueryString, "com.expertchoice.core")

                If InResponseParams("FromCore") IsNot Session("TTLaunchHash") Then
                    Throw New Exception("Invalid launch URL")
                End If

                Dim AdminProxy As New KeypadPrivillegedClient

                'Clear the DB of tokens and add a master token for this meetingID
                Dim meetingID As String = clsMeetingID.AsString(App.ActiveProject.MeetingID)

                AdminProxy.ClearTokens(meetingID)

                Dim ResponseParams As New QueryEncrypt("com.expertchoice.teamtime")
                ResponseParams.Add("meetingid", meetingID)
                ResponseParams.Add("email", App.ActiveUser.UserEmail)
                ResponseParams.Add("password", App.ActiveUser.UserPassword)
                Dim AuthToken As String = AdminProxy.AddToken(meetingID, ClientType.Regular)
                ResponseParams.Add("authtoken", AuthToken)
                Dim dummyClient As New KeypadClient

                ResponseParams.Add("webservice", dummyClient.Endpoint.Address.ToString)
                Try
                    dummyClient.Close()
                Catch ex As Exception
                End Try

                AdminProxy.Close()

                url = "~/TTA/TeamTimeAssistant.application" & "?" & ResponseParams.ToString
                url = Page.Request.Url.GetLeftPart(UriPartial.Authority) & ResolveUrl(url)

                If Debugger.IsAttached Then
                    'txtLaunch.Text = ResponseParams.ToString
                    txtLaunch.Text = App.ActiveUser.UserEmail & "|" & meetingID & "|" & AuthToken
                    txtLaunch.Visible = True
                    txtLaunch.Focus()
                Else
                    ClientScript.RegisterStartupScript(Me.GetType, "CloseWin", "setTimeout('self.close()', 3000);", True) ' D2568 + D2710
                    Response.Redirect(url)
                End If

            Catch ex As Exception
                lblLaunch.CssClass = "text" ' D2346
                lblLaunch.Text = String.Format("<h4 class='error'>{0}</h4><div class='text' style='font-weight:normal; text-align:left'>RTE (Saved in the Logs): <b>{1}</b><p>Stack trace: {2}</div><p align='center'><br><input type=button value='Close' class='button' onclick='window.close(); return false;'></p>", ResString("msgTTARunFailed"), ex.Message, ex.StackTrace)  ' D2635 + D2710
                App.DBSaveLog(dbActionType.actShowRTE, dbObjectType.einfRTE, App.ProjectID, "RTE on start TTA", String.Format("RTE: {0}" + vbCrLf + "Stack trace: {1} // {2}", ex.Message, ex.StackTrace, br))  ' D2659
                ClientScript.RegisterStartupScript(Me.GetType, "ResizeWin", "window.resizeTo(640,500);", True)  ' D2710
            End Try
        Else
            'lblLaunch.Visible = False  ' D2634
            txtLaunch.Visible = False
            'lblBrowserWarning.Visible = True
            ClientScript.RegisterStartupScript(GetType(String), "CheckExtension", "CheckMeta4ClickOnce(); CheckClickOnce(); setTimeout('CheckClickOnceExt();', 750);", True)   ' D2634 + D2713
        End If

    End Sub

End Class
