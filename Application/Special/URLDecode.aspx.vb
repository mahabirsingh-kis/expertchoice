
Partial Class URLDecodePage
    Inherits clsComparionCorePage

    Protected Sub Page_Init(sender As Object, e As EventArgs) Handles Me.Init
        If Not App.isAuthorized AndAlso Not App.CanUserDoAction(ecActionType.at_slManageAnyWorkgroup, App.ActiveUserWorkgroup) Then FetchAccess()
    End Sub

    Public Sub New()
        MyBase.New(_PGID_TEST)
    End Sub

    Protected Sub Page_PreRenderComplete1(sender As Object, e As EventArgs) Handles Me.PreRenderComplete
        Page.Title = "URL Decoder — " + ApplicationName
    End Sub

    Protected Sub btnEncode_Click(sender As Object, e As EventArgs) Handles btnEncode.Click
        Dim sResult As String = ""
        Dim val As String = EcSanitizer.GetSafeHtmlFragment(tbURL.Text).Trim  ' Anti-XSS
        If val <> "" Then
            Dim sParamsList As NameValueCollection = HttpUtility.ParseQueryString(val)
            Dim QIdx As Integer = val.IndexOf("?")
            If QIdx > 0 Then
                val = val.Substring(QIdx + 1)
                sParamsList = HttpUtility.ParseQueryString(val)
            End If
            Dim sHash As String = EcSanitizer.GetSafeHtmlFragment(ParamByName(sParamsList, _PARAMS_TINYURL))  ' Anti-XSS
            Dim sToken As String = EcSanitizer.GetSafeHtmlFragment(ParamByName(sParamsList, _PARAMS_KEY)) ' Anti-XSS
            If String.IsNullOrEmpty(sToken) AndAlso sHash = "" Then sHash = val
            If Not String.IsNullOrEmpty(sHash) Then sToken = App.DecodeTinyURL(sHash)
            If String.IsNullOrEmpty(sToken) Then sToken = val
            If Not String.IsNullOrEmpty(sToken) Then sResult = DecodeURL(sToken, App.DatabaseID)
            If String.IsNullOrEmpty(sResult) Then
                sResult = "<div class='error'><b>Sorry, but can't parse that string…</b></div>"
            Else
                sParamsList = HttpUtility.ParseQueryString(sResult)
                Dim sExtra As String = ""
                Dim sEmail As String = EcSanitizer.GetSafeHtmlFragment(ParamByName(sParamsList, _PARAMS_EMAIL))   ' Anti-XSS
                Dim sPsw As String = EcSanitizer.GetSafeHtmlFragment(ParamByName(sParamsList, _PARAMS_PASSWORD))  ' Anti-XSS
                If sPsw <> "" Then
                    sResult = sResult.Replace(sPsw, "******")
                    sPsw = "<i>[hidden]</i>"
                End If
                Dim sPasscode As String = EcSanitizer.GetSafeHtmlFragment(ParamByName(sParamsList, _PARAMS_PASSCODE)) ' Anti-XSS
                If sPasscode <> "" Then
                    Dim tPrj As clsProject = App.DBProjectByPasscode(sPasscode)
                    If tPrj IsNot Nothing Then
                        sExtra = String.Format("Project: #{0}, {1}", tPrj.ID, ShortString(tPrj.ProjectName, 50))
                        Dim tWkg As clsWorkgroup = App.DBWorkgroupByID(tPrj.WorkgroupID, False, False)
                        If tWkg IsNot Nothing Then sExtra = String.Format("{0}<br>Workgroup: {1}", sExtra, tWkg.Name)
                    End If
                End If
                Dim sUserName As String = EcSanitizer.GetSafeHtmlFragment(ParamByName(sParamsList, _PARAMS_USERNAME)) ' Anti-XSS
                sResult = String.Format("<b>String decoded</b>:<br>{4}<br><br>E-mail: {0}<br>Password: {1}<br>Passcode: {2}<br>User name: {3}<br><br>{5}", sEmail, sPsw, sPasscode, sUserName, sResult, sExtra)
            End If
            lblResult.InnerHtml = sResult
        End If
        lblResult.InnerHtml = sResult
    End Sub

End Class
