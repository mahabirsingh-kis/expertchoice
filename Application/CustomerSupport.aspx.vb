Imports FogBugz

Partial Class CustomerSupportFormPage
    Inherits clsComparionCorePage

    Private isForm As Boolean = True

    Public Sub New()
        MyBase.New(_PGID_CUSTOMER_SUPPORT)
    End Sub

    Public ReadOnly Property UserName() As String
        Get
            If App.isAuthorized Then Return App.ActiveUser.UserName
            Return ""
        End Get
    End Property

    Public ReadOnly Property UserEmail() As String
        Get
            If App.isAuthorized Then Return App.ActiveUser.UserEmail
            Return GetCookie(_COOKIE_EMAIL, "")
        End Get
    End Property

    ' D0883 ===
    Private ReadOnly Property Inbox() As String
        Get
            Return WebConfigOption(_OPT_FOGBUGZ_INBOX, "", True).Trim
        End Get
    End Property
    ' D0883 ==

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        StorePageID = False ' D0493
        CType(Master, clsComparionCoreMasterPopupPage).ShowButtonsLine = True   ' D0493

        btnSend.Text = ResString("btnSend")
        btnClose.Text = ResString("btnClose")

        If Not IsPostBack AndAlso isForm Then
            'Form.DefaultButton = btnSend.ID
            btnSend.Enabled = Inbox <> ""     ' D0883
            'ClientScript.RegisterStartupScript(GetType(String), "InitForm", "theForm.name.focus();", True)
            ClientScript.RegisterStartupScript(GetType(String), "InitForm", "InitForm();", True)    ' D1683
            ClientScript.RegisterOnSubmitStatement(GetType(String), "SendForm", "setTimeout('theForm.disabled=1;', 500);")  ' D0884
        End If

    End Sub

    ' D0883 ===
    Protected Sub btnSend_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnSend.Click
        Dim sMsg As String = vbCrLf + "{0}: {1}" + vbCrLf + "{2}: {3}" + vbCrLf + "{4}: {5}" + vbCrLf + "{6}: {7}" + vbCrLf + "{8}:" + vbCrLf + "{9}" + vbCrLf + vbCrLf + "{10}"   ' D1377
        If App.ApplicationError IsNot Nothing Then App.ApplicationError.PageID = CurrentPageID ' D1377
        Dim sExtra As String = CStr(IIf(Request("Status") <> "", vbCrLf + vbCrLf + GetApplicationStatus(), "Client: " + Request.UserHostAddress.ToString + vbCrLf + "Core: " + Request.Url.AbsoluteUri + " / " + GetVersion(App.GetCoreVersion()))) ' D1264 + D1377
        Dim sEmail As String = Request("email").Trim    ' D0885
        sMsg = String.Format(sMsg, ResString("lblCSName"), Request("name"), ResString("lblCSEmail"), sEmail, ResString("lblCSPhone"), Request("phone"), ResString("lblCSSubject"), Request("subject"), ResString("lblCSDescription"), Request("description"), sExtra)   ' D0885
        Dim sError As String = ""
        Dim sSender As String = WebConfigOption(_OPT_FOGBUGZ_INBOX)    ' D0886
        If isValidEmail(sEmail) Then sSender = sEmail
        If sSender = "" Then sSender = SystemEmail ' D0886 + D1152
        Dim sSubject = Request("subject").Trim()
        If sSubject = "" Then sSubject = ResString("subjCustomerSupport") ' D4217

        Dim retval As String = ""
        Try
            Dim bug As New BugReport(WebConfigOption(_OPT_FOGBUGZ_URL), "Feedback Request")
            bug.Project = "Feedback"
            bug.Area = "Misc"
            bug.Description = sSubject  ' D4217
            bug.ExtraInformation = sMsg
            bug.ForceNewBug = False
            bug.Email = sEmail
            bug.DefaultMsg = "ok"
            retval = bug.Submit
        Catch ex As Exception   ' D1424
            App.DBSaveLog(dbActionType.actSendRTE, dbObjectType.einfOther, App.ProjectID, "Error on send customer feedback", ex.Message)    ' D1424
            retval = ""
        End Try

        If retVal = "ok" Then
            lblReport.Text = ResString("msgCSThankYou")
        Else
            lblReport.Text = String.Format("<div class='error'>{0}: {1}</div>", ResString("lblError"), "Could not send email. Crash report was sent.")  ' D1424
        End If
        isForm = False

    End Sub

    Protected Sub Page_PreRender(ByVal sender As Object, ByVal e As EventArgs) Handles Me.PreRender
        pnlForm.Visible = isForm
        btnSend.Visible = isForm
        pnlMessage.Visible = Not isForm
    End Sub
    ' D0883 ==

End Class