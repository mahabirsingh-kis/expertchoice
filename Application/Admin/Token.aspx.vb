Partial Class Tokenpage
    Inherits clsComparionCorePage

    ' D1630 ===
    Private ReadOnly Property sTokenKey As String
        Get
            Return CStr(IIf(cbUserInstanceID.Checked, EcSanitizer.GetSafeHtmlFragment(tbInstanceID.Text), App.DatabaseID))  'Anti-XSS
        End Get
    End Property
    ' D1630 ==

    Protected Sub Page_Init(sender As Object, e As EventArgs) Handles Me.Init
        ScriptManager.RegisterStartupScript(Me, GetType(String), "Init", String.Format("setTimeout('theForm.{0}.focus();', 300);", tbSource.ClientID), True)    ' D3444
        cbUserInstanceID.Enabled = ShowDraftPages()   ' D3444
        btnEncode.Visible = ShowDraftPages()    ' D3444
        ' -D1630
        '' D0826 ===
        'cbUserInstanceID.InputAttributes.Remove("onclick")
        'cbUserInstanceID.InputAttributes.Add("onclick", String.Format("theForm.{0}.disabled=!this.checked; if (this.checked) theForm.{0}.focus();", tbInstanceID.ClientID))
        'If Not IsPostBack Then
        '    cbUserInstanceID.Checked = False
        '    tbInstanceID.Enabled = False
        'End If
        '' D0826 ==
    End Sub

    Public Sub New()
        MyBase.New(_PGID_ADMIN_SERVICEHASH)  ' D1630
    End Sub

    Protected Sub btnEncode_Click(sender As Object, e As EventArgs) Handles btnEncode.Click
        If tbSource.Text <> "" Then tbResult.Text = EncodeURL(EcSanitizer.GetSafeHtmlFragment(tbSource.Text), sTokenKey) ' D0826 + D1630 + Anti-XSS
    End Sub

    Protected Sub btnDecode_Click(sender As Object, e As EventArgs) Handles btnDecode.Click
        ' D1630 ===
        Dim sRes As New List(Of String)
        Dim sInput As String = EcSanitizer.GetSafeHtmlFragment(tbSource.Text.Trim())    'Anti-XSS
        If sTokenKey = "" Then sRes.Add("(!) Empty security key")
        If sInput = "" Then
            sRes.Add("(!) Empty input string")
        Else
            Dim sIdx As Integer = sInput.IndexOf("?")
            If sIdx > 0 Then
                sRes.Add("Try to parse string as URL with params")
                sInput = sInput.Substring(sIdx)
            End If

            Dim sInput_ As String = sInput.ToLower
            Dim HasParams As Boolean = sInput_.Contains("key=") OrElse sInput_.Contains("token=") OrElse sInput_.Contains("hash") OrElse sInput_.Contains("tinyurl")
            If HasParams Then
                sRes.Add("Probably, that string has URI params with encoded string")
                Try
                    Dim sParams As NameValueCollection = HttpUtility.ParseQueryString(sInput)
                    Dim sParam As String = EcSanitizer.GetSafeHtmlFragment(ParamByName(sParams, _PARAMS_KEY))   'Anti-XSS
                    If Not String.IsNullOrEmpty(sParam) Then
                        sRes.Add("Found token parameter")
                        sInput = sParam
                    End If
                    sParam = EcSanitizer.GetSafeHtmlFragment(ParamByName(sParams, _PARAMS_TINYURL)) 'Anti-XSS
                    If Not String.IsNullOrEmpty(sParam) Then
                        sRes.Add("Found hash parameter")
                        sInput = sParam
                    End If
                Catch ex As Exception
                    sRes.Add("(!) Error on parse string. Continue with string as is")
                End Try
            End If

            sRes.Add("Try decode as hash string…")
            Dim sResult As String = App.DecodeTinyURL(sInput)
            If sResult <> "" Then 
                sRes.Add("Found a hash code with text: " + sResult)
                sInput = sResult
            End If

            sRes.Add("Try decode as token string…")
            sResult = DecodeURL(sInput, sTokenKey)
            If String.IsNullOrEmpty(sResult) Then
                sRes.Add("(!) Can't decode URL. Input string or security key is wrong.")
            Else
                If sResult <> "" AndAlso EcSanitizer.GetSafeHtmlFragment(CheckVar("mode", "")).ToLower <> "god" Then sResult = Regex.Replace(sResult, "(.+)&password=([^&]*)&(.*)", "$1&password=*****&$3") Else sRes.Add("SuperGod mode enabled. You can see real passwords")  'Anti-XSS
                sRes.Add("Decoded string: " + sResult)
                If sResult.Contains("&") Then
                    sRes.Add("Decoded string looks as URL with params. Try to analyize…")
                    Try
                        Dim sParams As NameValueCollection = HttpUtility.ParseQueryString(sResult)

                        Dim sParam As String = EcSanitizer.GetSafeHtmlFragment(ParamByName(sParams, _PARAMS_EMAIL)) 'Anti-XSS
                        If sParam <> "" Then
                            Dim tUser As clsApplicationUser = App.DBUserByEmail(sParam)
                            If tUser Is Nothing Then
                                sRes.Add("User with e-mail '" + sParam + "' not found")
                            Else
                                If Not isSSO() Then sRes.Add(String.Format("User exists. ID: {3}, Email: '{0}', Name: '{1}', Has psw: {2}", tUser.UserEmail, tUser.UserName, tUser.HasPassword, tUser.UserID))   ' D6552
                            End If
                        End If

                        sParam = EcSanitizer.GetSafeHtmlFragment(ParamByName(sParams, _PARAMS_PASSCODE))    'Anti-XSS
                        If sParam <> "" Then
                            Dim tPrj As clsProject = App.DBProjectByPasscode(sParam)
                            If tPrj Is Nothing Then
                                sRes.Add("Project with access code '" + sParam + "' not found")
                            Else
                                Dim tWkg As clsWorkgroup = App.DBWorkgroupByID(tPrj.WorkgroupID, True, False)
                                Dim WkgID As String = "?"
                                If tWkg IsNot Nothing Then WkgID = String.Format("#{0}, ""{1}""", tWkg.ID, tWkg.Name)
                                sRes.Add(String.Format("Project exists. ID: {0}, Name: '{1}', On-line: {2}, Workgroup: {3}", tPrj.ID, tPrj.ProjectName, tPrj.isOnline AndAlso tPrj.isPublic, WkgID))
                                ' D5078 ===
                                Dim WkgGrp As String = EcSanitizer.GetSafeHtmlFragment(ParamByName(sParams, {"wkgrole"}))
                                If WkgGrp <> "" AndAlso tWkg IsNot Nothing Then
                                    Dim ID_ As Integer = -1
                                    If Integer.TryParse(WkgGrp, ID_) Then
                                        Dim GRP As clsRoleGroup = tWkg.RoleGroup(ID_, tWkg.RoleGroups)
                                        If GRP IsNot Nothing Then sRes.Add(String.Format("Attach to project as '{0}' (#{1})", GRP.Name, GRP.ID))
                                    End If
                                End If
                                ' D5078 ==
                                ' D1724 ===
                                sRes.Add("Link to that project under any account: " + ParseAllTemplates(_TEMPL_URL_APP + "?" + _PARAM_PASSCODE + "=" + sParam, Nothing, tPrj))
                                Dim tmpHID As Integer = tPrj.ProjectManager.ActiveHierarchy
                                Dim tUW As clsUserWorkgroup = clsUserWorkgroup.UserWorkgroupByUserIDAndWorkgroupID(App.ActiveUser.UserID, App.ActiveWorkgroup.ID, App.UserWorkgroups)   ' D4616
                                If App.isRiskEnabled AndAlso Not App.ActiveUser.CannotBeDeleted Then    ' D2767
                                    tPrj.ProjectManager.ActiveHierarchy = ECHierarchyID.hidLikelihood
                                    sRes.Add("Link to evaluate Likelihood project under your account: " + CreateLogonURL(App.ActiveUser, tPrj, "", ResString(_TEMPL_URL_APP, True), , tUW))    ' D1672 + D4616
                                    tPrj.ProjectManager.ActiveHierarchy = ECHierarchyID.hidImpact
                                    sRes.Add("Link to evaluate Impact project under your account: " + CreateLogonURL(App.ActiveUser, tPrj, "", ResString(_TEMPL_URL_APP, True), , tUW))   ' D1672 + D4616
                                Else
                                    If Not App.ActiveUser.CannotBeDeleted Then sRes.Add("Link to evaluate project under your account: " + CreateLogonURL(App.ActiveUser, tPrj, "", ResString(_TEMPL_URL_APP, True), , tUW)) ' D3364 + D4616
                                End If
                                tPrj.ProjectManager.ActiveHierarchy = tmpHID
                                ' D1724 ==
                            End If
                        End If

                    Catch ex As Exception
                        sRes.Add("(!) Can't parse string properly.")
                    End Try
                End If
            End If
        End If

        tbResult.Text = ""
        For i As Integer = 0 To sRes.Count - 1
            tbResult.Text += sRes(i) + vbCrLf
        Next
        ' D1630 ==
    End Sub

    Private Sub TokenPage_InitComplete(sender As Object, e As EventArgs) Handles Me.InitComplete
        If App.ActiveUser Is Nothing OrElse Not App.ActiveUser.CannotBeDeleted Then FetchAccess() ' D1630
    End Sub

End Class
