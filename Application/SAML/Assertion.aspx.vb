Partial Class SAMLAssertPage
    Inherits clsComparionCorePage

    Dim isDebug As Boolean = False  ' D6552
    Public _OPT_SAML_ATTR_EMAIL As String = "emailAddress"  ' D6451 + D7424
    Public _OPT_SAML_ATTR_PASSCODE As String = "passcode"   ' D7412
    Public _OPT_SAML_ATTR_WKG As String = "usr-workgroup"   ' D7412
    Public _OPT_SAML_ATTR_ROLE As String = "department"     ' D7412
    Public _OPT_SAML_ATTR_FNAME As String = "firstName"     ' D7424
    Public _OPT_SAML_ATTR_LNAME As String = "lastName"      ' D7424

    Public Sub New()
        MyBase.New(_PGID_SSO_ASSERT)    ' D6550
    End Sub

    ' D7439 ===
    Private Function GetOrigParams(targetURL As String) As String
        Dim sOrigParams = CheckVar("orig_params", "").Trim
        If sOrigParams = "" AndAlso Not String.IsNullOrEmpty(targetURL) AndAlso targetURL.ToLower.Contains("orig_params=") Then
            Dim idx As Integer = targetURL.ToLower.IndexOf("orig_params=")
            sOrigParams = targetURL.Substring(idx + 12)
            If sOrigParams.Contains("&") Then sOrigParams = sOrigParams.Substring(0, sOrigParams.IndexOf("&"))
        End If
        Dim sParams As String = ""
        If sOrigParams <> "" Then
            Try
                sParams = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(sOrigParams))
            Catch ex As Exception
            End Try
        End If
        Return sParams
    End Function
    ' D7439 ==

    Protected Sub Page_Init(sender As Object, e As EventArgs) Handles Me.Init
        ShowNavigation = False
        StorePageID = False

        isDebug = CheckVar("debug", Str2Bool(GetCookie(_COOKIE_SSO_DEBUG, "0")))   ' D6552 + D7424

        Dim sRes As String = ""
        Dim tRes As ecAuthenticateError = ecAuthenticateError.aeUnknown

        Dim isInResponseTo As Boolean = False
        Dim partnerIdP As String = Nothing
        Dim authnContext As String = Nothing
        Dim userName As String = Nothing
        Dim attributes As IDictionary(Of String, String) = Nothing
        Dim targetUrl As String = Nothing
        Dim tUser As clsApplicationUser = Nothing   ' D7413

        If isSSO() Then ' D7413
            Try
                ComponentSpace.SAML2.SAMLServiceProvider.ReceiveSSO(Request, isInResponseTo, partnerIdP, authnContext, userName, attributes, targetUrl)
            Catch ex As Exception
                If isDebug Then
                    AddLog(String.Format("<div class='error'><b>SAML error</b>: {0}</div>", ex.Message))
                Else
                    If isInResponseTo Then sRes = String.Format("Invalid SAML response: {0}", ex.Message)    ' D7413 + D7439
                End If
            End Try

            If Not String.IsNullOrEmpty(targetUrl) AndAlso targetUrl.ToLower.Contains("debug=") Then
                isDebug = True    ' D6552 + D7402
            End If
            If userName Is Nothing Then userName = ""   ' D7420
            ' D7424 ===
            If attributes IsNot Nothing AndAlso userName = "" Then
                If attributes.ContainsKey(_OPT_SAML_ATTR_FNAME) Then userName = attributes(_OPT_SAML_ATTR_FNAME)
                If attributes.ContainsKey(_OPT_SAML_ATTR_LNAME) Then userName = (userName + " " + attributes(_OPT_SAML_ATTR_LNAME)).Trim
            End If
            ' D7424 ==

            Dim sPasscode As String = ""    ' D6532
            ' D6451 ===
            Dim sEmail As String = userName
            If attributes IsNot Nothing Then
                If attributes.ContainsKey(_OPT_SAML_ATTR_EMAIL) Then sEmail = attributes(_OPT_SAML_ATTR_EMAIL)
                If attributes.ContainsKey(_OPT_SAML_ATTR_PASSCODE) Then sPasscode = attributes(_OPT_SAML_ATTR_PASSCODE)
            End If
            ' D6451 ==
            If sEmail Is Nothing Then sEmail = ""   ' D7420

            If isDebug Then
                AddLog(String.Format("isInResponseTo: {0}", isInResponseTo))
                AddLog(String.Format("partnerIdP: {0}", partnerIdP))
                AddLog(String.Format("authnContext: {0}", authnContext))
                AddLog(String.Format("userEmail: {0}", sEmail))
                AddLog(String.Format("userName: {0}", userName))
                If Not String.IsNullOrEmpty(sPasscode) Then AddLog(String.Format("passcode: {0}", sPasscode))

                Dim atr As String = ""
                If attributes IsNot Nothing Then
                    For Each tP As KeyValuePair(Of String, String) In attributes
                        atr += String.Format("{2}[{0}: {1}]", tP.Key, tP.Value, If(atr = "", "", ", "))
                    Next
                End If
                AddLog("Attributes: " + atr)
            End If

            ' D7413 ===
            If Not String.IsNullOrEmpty(targetUrl) Then
                Dim tempURI As Uri = Nothing
                If Not Uri.TryCreate(targetUrl, UriKind.RelativeOrAbsolute, tempURI) Then targetUrl = Nothing
            End If
            If targetUrl Is Nothing Then targetUrl = ""    ' D6550 + D7420 + D7439
            ' D7413 ==

            If Not String.IsNullOrEmpty(sEmail) Then
                tUser = App.DBUserByEmail(sEmail)
                If tUser IsNot Nothing Then
                    If isDebug Then AddLog(String.Format(" User exists: {0} [#{1}, {2}]", tUser.UserEmail, tUser.UserID, tUser.UserName))
                Else
                    tUser = App.UserWithSignup(sEmail, userName, GetRandomString(_DEF_PASSWORD_LENGTH + 4, False, False), "Created from SSO", , False)  ' D6532 + D6550
                    If isDebug Then ' D7424
                        If tUser IsNot Nothing Then
                            AddLog(String.Format("User account has been created: {0} (#{1})", tUser.UserEmail, tUser.UserID))
                        Else
                            AddLog(String.Format("Unable to create user account '{0}'", tUser.UserEmail))
                        End If
                    End If
                End If
                If tUser IsNot Nothing Then
                    ' Check user workgroups and attach to specified (default) when no available wkg yet
                    Dim tUWList As List(Of clsUserWorkgroup) = App.DBUserWorkgroupsByUserID(tUser.UserID)
                    If tUWList.Count = 0 AndAlso String.IsNullOrEmpty(sPasscode) Then
                        Dim WkgList As List(Of clsWorkgroup) = App.DBWorkgroupsByStatus(ecWorkgroupStatus.wsEnabled, False, False)
                        Dim sWkgName As String = ""
                        If attributes IsNot Nothing Then
                            If attributes.ContainsKey(_OPT_SAML_ATTR_WKG) Then sWkgName = attributes(_OPT_SAML_ATTR_WKG)
                        End If
                        If String.IsNullOrEmpty(sWkgName) Then
                            sWkgName = WebConfigOption(_OPT_SSO_DEF_WKG)
                        End If
                        If String.IsNullOrEmpty(sWkgName) AndAlso WkgList.Count = 1 Then
                            sWkgName = WkgList(0).Name
                        End If

                        ' Search wkg by name
                        Dim tWkg As clsWorkgroup = Nothing
                        If Not String.IsNullOrEmpty(sWkgName) Then
                            Dim sCheckWkgName As String = sWkgName.Trim.ToLower
                            For Each tmpWkg As clsWorkgroup In WkgList
                                If tmpWkg.Name.Trim.ToLower.Equals(sCheckWkgName) Then
                                    tWkg = tmpWkg
                                    Exit For
                                End If
                            Next
                        End If

                        If tWkg Is Nothing Then
                            AddLog(ResString("msgSSOCantFindWorkgroup"))    ' D7440
                            'sRes = If(String.IsNullOrEmpty(sWkgName), ResString("msgSSODefWorkgroupEmpty"), String.Format(ResString("msgSSOCantFindWorkgroup"), SafeFormString(sWkgName)))
                            sRes = If(String.IsNullOrEmpty(sWkgName), ResString("msgSSONewUserWithoutModels"), String.Format(ResString("msgSSOCantFindWorkgroup"), SafeFormString(sWkgName)))   ' D7419
                        Else
                            tWkg = App.DBWorkgroupByID(tWkg.ID, True, True)
                            Dim tRole As ecRoleGroupType = ecRoleGroupType.gtEvaluator
                            Dim sRole As String = ""
                            If attributes IsNot Nothing Then
                                If attributes.ContainsKey(_OPT_SAML_ATTR_ROLE) Then sRole = attributes(_OPT_SAML_ATTR_ROLE)
                            End If
                            If String.IsNullOrEmpty(sRole) Then
                                sRole = WebConfigOption(_OPT_SSO_DEF_ROLE)
                            End If
                            Select Case sRole.Trim.ToLower.Replace(" ", "").Replace("_", "")
                                Case "wm", "workgroupmanager", "wkgmanager"
                                    tRole = ecRoleGroupType.gtWorkgroupManager
                                Case "po", "prjorganizer", "projectorganizer", "pm", "prjmanager", "projectmanager"
                                    tRole = ecRoleGroupType.gtProjectOrganizer
                                Case Else
                                    tRole = ecRoleGroupType.gtUser  ' D7420
                            End Select
                            Dim tRoleID = tWkg.GetDefaultRoleGroupID(ecRoleLevel.rlApplicationLevel, tRole)
                            If tRoleID < 0 Then
                                AddLog(ResString("msgSSOCantFindRoleGroup"))    ' D7440
                                'sRes = String.Format(ResString("msgSSOCantFindRoleGroup"))
                                sRes = String.Format(ResString("msgSSONewUserWithoutModels"))   ' D7419
                            Else
                                tUWList.Add(App.AttachWorkgroup(tUser.UserID, tWkg, tRoleID, String.Format("Attach user to workgroup '{0}' as '{1}' on SSO call", tWkg.Name, ResString(String.Format("lblb_{0}", tRole.ToString)))))
                            End If
                        End If
                    End If

                    If String.IsNullOrEmpty(sRes) Then
                        ' D7412 ==
                        ' D7420 ===
                        Dim tParams As New NameValueCollection
                        Dim sParams As String = GetOrigParams(targetUrl)    ' D7439
                        If sParams <> "" Then tParams = HttpUtility.ParseQueryString(sParams)   ' D7439
                        tParams(_PARAM_SSO_PREFIX + _PARAM_EMAIL) = tUser.UserEmail
                        tParams(_PARAM_SSO_PREFIX + _PARAM_PASSWORD) = tUser.UserPassword
                        If sPasscode <> "" Then tParams(_PARAM_SSO_PREFIX + _PARAM_PASSCODE) = sPasscode
                        ' D7439 ===
                        ' D7449 ===
                        Dim sMeetingID As String = ParamByName(Request.Params, _PARAMS_MEETING_ID)
                        If sMeetingID = "" Then sMeetingID = ParamByName(tParams, _PARAMS_MEETING_ID)
                        ' D7449 ==
                        Dim sAuthURL As String = ""
                        tRes = Authenticate(tParams, sRes, sAuthURL, If(String.IsNullOrEmpty(sMeetingID), ecAuthenticateWay.awSSO, ecAuthenticateWay.awJoinMeeting), True)  ' D7449
                        AddLog("Authentication result: " + tRes.ToString)   ' D7440
                        If tRes = ecAuthenticateError.aeNoErrors AndAlso Not String.IsNullOrEmpty(sAuthURL) Then
                            targetUrl = sAuthURL
                            AddLog("Destination URL: " + sAuthURL) ' D7440
                        End If
                        ' D7420 + D7439 ==

                        ' D7459 ===
                        If tRes = ecAuthenticateError.aeUseRegularLogon AndAlso sMeetingID <> "" Then
                            Dim tMeeting As Long
                            If sPasscode = "" AndAlso clsMeetingID.TryParse(sMeetingID, tMeeting) Then
                                Dim tPrj As clsProject = App.DBProjectByMeetingID(tMeeting)
                                If tPrj IsNot Nothing Then
                                    sPasscode = clsMeetingID.AsString(If(clsMeetingID.AsString(tPrj.MeetingIDImpact) = sMeetingID, tPrj.MeetingIDImpact, tPrj.MeetingIDLikelihood))
                                    tParams(_PARAM_SSO_PREFIX + _PARAM_PASSWORD) = tUser.UserPassword
                                End If
                            End If
                            tParams.Add("pipe", "1")
                            tRes = Authenticate(tParams, sRes, sAuthURL, ecAuthenticateWay.awSSO, True)
                        End If
                        ' D7459 ==

                        'App.DBSaveLogonEvent(tUser.UserEmail, tRes, ecAuthenticateWay.awSSO, Request)   ' D6532
                        If App.isAuthorized Then
                            App.SSO_User = True ' D6552
                            ' D7419 ===
                            If App.ActiveUser IsNot Nothing AndAlso (App.ActiveUser.UserName = "" OrElse App.ActiveUser.UserName.Equals(App.ActiveUser.UserEmail, StringComparison.CurrentCultureIgnoreCase)) AndAlso Not String.IsNullOrEmpty(userName) Then
                                App.ActiveUser.UserName = userName
                                App.DBUserUpdate(App.ActiveUser, False, "Import user name from SSO credentials")
                                AddLog("Update user name")  ' D7440
                            End If
                            ' D7419 ==
                        Else
                            If tRes <> ecAuthenticateError.aeNoErrors Then
                                ' D7424 ===
                                Select Case tRes
                                    Case ecAuthenticateError.aeNoUserFound, ecAuthenticateError.aeWrongPassword
                                        sRes = ResString("msgWrongEmailSSO")
                                    Case Else
                                        sRes = App.GetMessageByAuthErrorCode(tRes, sPasscode)
                                End Select
                                ' D7424 ==
                            End If
                        End If
                    End If
                    If Not String.IsNullOrEmpty(sRes) Then sRes = ParseString(String.Format(sRes, sEmail))   ' D7420 + D7440
                End If
            End If
            'Else
            '    sRes = "SSO mode is disabled"    ' D7413
        End If

        ' D7439 ===
        If String.IsNullOrEmpty(targetUrl) Then
            targetUrl = URLWithParams(_URL_ROOT + If(_URL_ROOT.EndsWith("/"), "", "/"), GetOrigParams(""))  ' D7440
        End If
        ' D7439 ==

        If isDebug Then
            If sRes <> "" Then AddLog(String.Format("<b><br/>{0}</b>", sRes))
            If Not String.IsNullOrEmpty(targetUrl) Then AddLog(String.Format("<br>Target: <a href='{0}'>{0}</a>", targetUrl))
        Else
            If (tRes <> ecAuthenticateError.aeNoErrors AndAlso tRes <> ecAuthenticateError.aeUnknown) OrElse Not String.IsNullOrEmpty(sRes) Then    ' D7440
                If Not String.IsNullOrEmpty(sRes) Then App.ApplicationError.Init(ecErrorStatus.errMessage, CurrentPageID, sRes, tRes)   ' D7439
                If tUser IsNot Nothing Then App.ApplicationError.CustomData = tUser.UserEmail
                'targetUrl = PageURL(_PGID_START)   ' D7439
            End If
            Response.Redirect(targetUrl, True)  ' D7412
        End If

    End Sub

    Private Sub AddLog(sText As String)
        lblMessage.Text += sText + "<br>"
    End Sub

End Class