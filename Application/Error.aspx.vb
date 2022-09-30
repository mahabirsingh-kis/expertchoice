Partial Class Error500Page
    Inherits clsComparionCorePage

    Public CanSubmit As Boolean = False
    Public ReturnURL As String = ""

    Public Sub New()
        MyBase.New(_PGID_ERROR_500)
    End Sub

    Protected Sub Page_Init(sender As Object, e As EventArgs) Handles Me.Init
        If isAJAX() AndAlso CheckVar("submit", False) Then
            Dim sReply As String = SubmitFeedback()
            If sReply = "" Then sReply = String.Format("<h5 class='error' style='margin:2em 1ex'>{0}</h4>", ResString("msgCantPerformAction"))
            RawResponseStart()
            Response.Write(sReply)
            RawResponseEnd()
        Else
            AlignHorizontalCenter = True
            AlignVerticalCenter = True
            ShowNavigation = False
            ShowErrorDetails()
            'Dim fCanAuto As Boolean = WebConfigOption(_OPT_FOGBUGZ_AUTOSUBMIT, If(Not Request.IsLocal AndAlso App.ApplicationError.Status = ecErrorStatus.errRTE, 1, 0).ToString).ToLower = True.ToString.ToLower
            Dim fCanAuto As Boolean = Str2Bool(WebConfigOption(_OPT_FOGBUGZ_AUTOSUBMIT, Bool2Num(Not Request.IsLocal AndAlso App.ApplicationError.Status = ecErrorStatus.errRTE).ToString))   ' D6637
            If CanSubmit AndAlso fCanAuto Then  ' D6637
                SubmitFeedback()
            End If
            If ReturnURL = "" Then ReturnURL = PageURL(_DEF_PGID_ONPROJECTS) ' D0885
        End If
    End Sub

    Private Sub ShowErrorDetails()
        ' D0175 ===
        If Not Request("test") Is Nothing And App.ApplicationError.Status = ecErrorStatus.errNone Then
            App.ApplicationError.Init(ecErrorStatus.errTest, CurrentPageID, "Feedback Test message", Nothing, Request.Url.AbsolutePath) ' D0459
        End If
        ' D0175 ==

        lblSourceCode.Visible = False
        lblPostText.Visible = False

        Dim sError As String = ""
        Dim sDetails As String = "" ' D0184

        If Not Request.UrlReferrer Is Nothing Then ReturnURL = Request.UrlReferrer.OriginalString ' D0468

        Select Case App.ApplicationError.Status

            Case ecErrorStatus.errDatabase
                Response.Redirect(PageURL(_PGID_START), True)

            Case ecErrorStatus.errPageNotFound
                Response.Redirect(PageURL(_PGID_ERROR_404), True)

            Case ecErrorStatus.errAccessDenied
                Response.Redirect(PageURL(_PGID_ERROR_403), True)

            Case ecErrorStatus.errMessage
                If App.ApplicationError.Message <> "" Then sError = App.ApplicationError.Message

            Case ecErrorStatus.errRTE, ecErrorStatus.errTest
                sError = GetRTEHeader()    ' D0119 + D0247
                If Request.IsLocal Then
                    ' D0119 ===
                    If Not App.ApplicationError.Details Is Nothing Then
                        lblSourceCode.Text = String.Format(ResString("errDetails"), RemoveXssFromUrl(App.ApplicationError.PageURL, True), RemoveXssFromUrl(ReturnURL, True), String.Format("<div class='code' style='overflow-y:auto'>{0}</div>", App.ApplicationError.Details.StackTrace.Replace(vbCrLf, "<br>")))  ' D0468 + Anti-XSS
                        lblSourceCode.Visible = True
                        sDetails = App.ApplicationError.Details.StackTrace   ' D0184
                    End If
                    FeedbackExtra.Value = GetApplicationStatus().Replace(vbCrLf, "<br>")
                    lblPostText.Text = String.Format("&#187; <b><a href='' class='dashed' onclick='return ShowDetails()'>{0}</a></b>", ResString("lnkViewStatusDump"))
                    lblPostText.Visible = True
                    ' D0119 ==
                End If
        End Select

        ' D0184 ===
        If sError <> "" Then
            Try
                App.DBSaveLog(If(App.ApplicationError.Status = ecErrorStatus.errRTE, dbActionType.actShowRTE, dbActionType.actShowMessage), If(App.ApplicationError.Status = ecErrorStatus.errRTE, dbObjectType.einfRTE, dbObjectType.einfMessage), -1, sError, String.Format("Details: {0};", IIf(String.IsNullOrEmpty(sDetails), "none", sDetails)))    ' D0496
            Catch ex As Exception
                DebugInfo(ex.Message, _TRACE_RTE)   ' D0330
            End Try
        End If
        ' D0184 ==

        CanSubmit = sError <> "" AndAlso WebConfigOption(_OPT_FOGBUGZ_URL) <> ""
        If sError = "" Then sError = ResString("errNoDetails")

        lblError.Text = sError
        '  D0118 ==
    End Sub

    Private Function SubmitFeedback() As String
        Dim sResult As String = ""
        Dim sURL As String = WebConfigOption(_OPT_FOGBUGZ_URL)
        ' D0175 ===
        If sURL <> "" And App.ApplicationError.Status <> ecErrorStatus.errNone Then

            ' D0175 ==
            Dim sEmail As String = GetCookie(_COOKIE_EMAIL)
            If Not App.ActiveUser Is Nothing Then sEmail = App.ActiveUser.UserEmail

            Dim client As New WebClient()

            'D2784 ===
            Dim postData = New NameValueCollection()
            postData.Add("ScoutUserName", WebConfigOption(_OPT_FOGBUGZ_USERNAME))
            postData.Add("ScoutProject", WebConfigOption(_OPT_FOGBUGZ_PROJECT))
            postData.Add("ScoutArea", WebConfigOption(_OPT_FOGBUGZ_AREA))
            postData.Add("ScoutDefaultMessage", ResString("msgFogBugzDefaultMessage"))
            postData.Add("Description", GetRTEHeader)   ' D4175
            postData.Add("Extra", vbCrLf + GetApplicationStatus.Replace("&", "%26"))
            If sEmail <> "" Then postData.Add("Email", sEmail)

            ' D2762 ===
            Dim Response As Byte() = {}
            Try
                Response = client.UploadValues(sURL, "POST", postData)
                ' D2784 ==
            Catch ex As Exception
                Try
                    App.DBSaveLog(dbActionType.actSendRTE, dbObjectType.einfRTE, -1, "Unable to send RTE: " + ex.Message, sURL + vbCrLf + ex.StackTrace)
                Catch exLogs As Exception
                    DebugInfo(ex.Message, _TRACE_RTE)   ' D0330
                End Try
            End Try
            ' D2762 ==
            sResult = Encoding.ASCII.GetString(Response)
            If sResult <> "" Then
                Dim isError As Boolean = sResult.IndexOf("<Error>") > 0
                lblPostText.Visible = False
                sResult = String.Format("<h5{1} style='margin:2em 1ex'>{0}</h4>", sResult, IIf(isError, " class='error'", ""))
                lblSourceCode.Text = sResult
                lblSourceCode.Visible = True
                ' D0184 ===
                Try
                    App.DBSaveLog(dbActionType.actSendRTE, dbObjectType.einfRTE, -1, GetRTEHeader, HttpUtility.HtmlDecode(sResult))  ' D0496 + D4175
                Catch ex As Exception
                    DebugInfo(ex.Message, _TRACE_RTE)   ' D0330
                End Try
                ' D0184 ==
            End If
            CanSubmit = False
        End If
        Return sResult
    End Function

End Class
