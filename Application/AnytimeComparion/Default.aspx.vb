Imports System.Runtime.InteropServices
Imports System.Web.Services
Imports Pages.external_classes
Imports ECService = ExpertChoice.Service

Public Class _Default
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim App = CType(Session("App"), clsComparionCore)

        If Request.QueryString("pageError") Is Nothing Then
            Dim project = App.DBProjectByPasscode(If(Session("passcode") IsNot Nothing, Session("passcode").ToString(), ""))
            If project IsNot Nothing Then
                Dim customTitle As String = "", customMessage As String = ""
                LoadCustomSignUpPageContent(project, customTitle, customMessage)
                Session("customTitle") = customTitle
                Session("customMessage") = customMessage
                Session("ProjectName") = project.ProjectName
            End If
        ElseIf Request.QueryString.HasKeys Then
            If Request.QueryString("pageError") IsNot Nothing And (Session("pageError") IsNot Nothing AndAlso Session("pageError").ToString().Trim() <> "") Then
                If Request.QueryString("pageError").ToString() = "inviteNoAccess" Then
                    Session("pageError") = "The access code for specified model is disabled. Please contact the project manager to get the access."
                ElseIf Request.QueryString("pageError").ToString() = "aeProjectLocked" Then
                    'Session("pageError") = $"{If(Session("ProjectName") IsNot Nothing AndAlso Session("ProjectName").ToString().Trim() <> "", Session("ProjectName").ToString(), "")}"
                ElseIf Request.QueryString("pageError").ToString() = "modelClosed" Then
                    Session("pageError") = "Model has been closed, You may now close this window/tab."
                End If
            ElseIf Request.QueryString("pageError") IsNot Nothing AndAlso Request.QueryString("pageError").ToString() = "modelClosed" Then
                Session("pageError") = "Model has been closed, You may now close this window/tab."
            End If
        End If
    End Sub

    Private Sub LoadCustomSignUpPageContent(ByVal project As clsProject, <Out> ByRef Optional customTitle As String = "", <Out> ByRef Optional customMessage As String = "")
        If project Is Nothing Then Return
        'customTitle.InnerHtml = $"Sign Up Below<br/> {project.ProjectName}"
        'customTitle = $"Sign Up Below<br/><hr/> {project.ProjectName}"
        Dim signUpContent = ""
        Dim signUpTitle = String.Format(TeamTimeClass.ResString("msgSignUpEvaluate"), project.ProjectName)

        If project.ProjectManager.Parameters.InvitationCustomTitle <> "" Then
            signUpTitle = StringFuncs.SafeFormString(TeamTimeClass.ParseAllTemplates(project.ProjectManager.Parameters.InvitationCustomTitle, Nothing, project))
        End If

        If project.ProjectManager.Parameters.InvitationCustomText <> "" Then

            If project.ProjectManager.Parameters.InvitationCustomText.Trim().ToString() <> "" Then
                Dim htmlString = project.ProjectManager.Parameters.InvitationCustomText.Trim()

                If InfodocService.isMHT(htmlString) Then
                    htmlString = InfodocService.Infodoc_Unpack(project.ID, project.ProjectManager.ActiveHierarchy, Consts.reObjectType.ExtraInfo, _PARAM_INVITATION_CUSTOM, htmlString, True, True, -1)
                End If

                signUpContent = StringFuncs.HTML2TextWithSafeTags(htmlString, (StringFuncs._DEF_SAFE_TAGS & "IMG;TABLE;TR;TH;TD;")).Trim()

                If signUpContent <> "" Then
                    signUpContent = $"{TeamTimeClass.ParseAllTemplates(signUpContent, Nothing, project)}"
                End If
            End If
        End If

        customTitle = If(project.ProjectManager.Parameters.InvitationCustomTitle <> "", signUpTitle, $"")
        'customTitle = signUpContent
        'customTitle.InnerHtml = If(project.ProjectManager.Parameters.InvitationCustomTitle <> "", signUpTitle, $"Sign up or log in below.<br/> {project.ProjectName}")
        'customContent.InnerHtml = signUpContent
        customMessage = signUpContent
    End Sub

    <WebMethod(EnableSession:=True)>
    Public Shared Function IsExistingUser(ByVal email As String) As String
        System.Threading.Thread.Sleep(1000)
        Dim responseText = "false"

        If Not String.IsNullOrWhiteSpace(email) Then

            Try
                Dim app = CType(HttpContext.Current.Session("App"), clsComparionCore)
                Dim emailUser = app.DBUserByEmail(email)
                responseText = If(emailUser Is Nothing, "false", "true")
            Catch ex As Exception
                responseText = $"Error: {ex.Message}"
            End Try
        End If

        Return responseText
    End Function


    <WebMethod(EnableSession:=True)>
    Public Shared Function GeneralLinkSignUp(ByVal email As String, ByVal name As String, ByVal password As String, ByVal sPhone As String, ByVal signup_mode As Boolean) As Object
        Dim context = HttpContext.Current
        Dim app = CType(context.Session("App"), clsComparionCore)
        Dim passCode = CStr(context.Session(Constants.Sess_SignUp_Passcode))
        Dim nully = ""
        Dim isUserExist = app.DBUserByEmail(email) IsNot Nothing
        Dim isPass = False
        Dim message = ""

        If isUserExist AndAlso email <> "" Then
            message = $"{TeamTimeClass.ResString("errUseRegisteredForm")}"
        Else
            Dim project = app.DBProjectByPasscode(passCode)
            message = AnytimeClass.GetProjectInAccessibleMessage(project, email, False)

            If String.IsNullOrEmpty(message) Then
                Dim user = app.UserWithSignup(email, name, password, "", nully, False)
                Dim authRes = app.Logon(email, password, passCode, False, True, False)

                If user Is Nothing OrElse (authRes = ecAuthenticateError.aeNoUserFound AndAlso email = "") Then
                    authRes = SiteMaster.forceSignUponAnonymous(project, name)

                    Select Case authRes
                        Case ecAuthenticateError.aePasscodeNotAllowed
                            message = String.Format(TeamTimeClass.ResString("msgDisabledPasscode"), project.ProjectName)
                        Case ecAuthenticateError.aeNoErrors
                            isPass = True
                        Case ecAuthenticateError.aeProjectLocked
                            message = String.Format(TeamTimeClass.ResString("msgEvaluationLocked"), project.ProjectName)
                        Case ecAuthenticateError.aeProjectReadOnly
                            message = "Sorry, but decision " & project.ProjectName & " is read-only and not available for collect input judgments. Please contact your Project Organizer to request permission to access this project."
                        Case Else
                            message = TeamTimeClass.ParseAllTemplates(app.GetMessageByAuthErrorCode(authRes), app.ActiveUser, Nothing)
                    End Select
                End If

                If authRes = ecAuthenticateError.aeNoErrors OrElse app.ActiveUser IsNot Nothing Then
                    context.Session("User") = app.ActiveUser
                    context.Session("Project") = app.ActiveProject
                    AddSignUpUserPhoneNumber(app, sPhone)
                    '_Default.StartAnytime(app.ActiveProject.ID)
                    Login.StartAnytime(app.ActiveProject.ID)
                    isPass = True
                    ChangeLastHasToUserSpecific(app)
                End If
            End If
        End If

        Dim oSerializer = New System.Web.Script.Serialization.JavaScriptSerializer()
        Dim output = New Object()
        output = New With {
        Key .message = message,
        Key .pass = isPass
    }
        Return oSerializer.Serialize(output)
    End Function

    Private Shared Sub AddSignUpUserPhoneNumber(ByVal app As clsComparionCore, ByVal phoneNumber As String)
        If app.HasActiveProject() AndAlso (app.ActiveProject.isValidDBVersion OrElse app.ActiveProject.isDBVersionCanBeUpdated) AndAlso phoneNumber <> "" Then
            Dim pm = app.ActiveProject.ProjectManager

            If pm.Attributes.GetAttributeByID(Attributes.ATTRIBUTE_USER_PHONE_ID) Is Nothing Then
                pm.Attributes.AddAttribute(Attributes.ATTRIBUTE_USER_PHONE_ID, TeamTimeClass.ResString("lblUserPhone"), Attributes.AttributeTypes.atUser, Attributes.AttributeValueTypes.avtString, "", False, Nothing, "lblUserPhone")
                pm.Attributes.WriteAttributes(Attributes.AttributesStorageType.astStreamsDatabase, pm.StorageManager.ProjectLocation, pm.StorageManager.ProviderType, pm.StorageManager.ModelID)
            End If

            Dim currentUser = pm.GetUserByEMail(app.ActiveUser.UserEmail)

            If currentUser Is Nothing Then
                currentUser = pm.AddUser(app.ActiveUser.UserEmail, True, app.ActiveUser.UserName)
            End If

            pm.Attributes.SetAttributeValue(Attributes.ATTRIBUTE_USER_PHONE_ID, currentUser.UserID, Attributes.AttributeValueTypes.avtString, phoneNumber, Guid.Empty, Guid.Empty)
            pm.Attributes.WriteAttributeValues(Attributes.AttributesStorageType.astStreamsDatabase, pm.StorageManager.ProjectLocation, pm.StorageManager.ProviderType, pm.StorageManager.ModelID, currentUser.UserID)
        End If
    End Sub

    Private Shared Sub ChangeLastHasToUserSpecific(ByVal App As clsComparionCore)
        Dim userSpecificHash As String = GeckoClass.CreateLogonURL(App.ActiveUser, App.ActiveProject, False, "", "")
        userSpecificHash = userSpecificHash.Replace("?hash=", "")
        Dim hashCookie As HttpCookie = New HttpCookie("LastHash", userSpecificHash) With {
            .HttpOnly = True,
            .Expires = DateTime.Now.AddDays(1)
        }
        HttpContext.Current.Response.Cookies.Add(hashCookie)
    End Sub

    <WebMethod(EnableSession:=True)>
    Public Shared Function GeneralLinkLogin(ByVal email As String, ByVal password As String) As Object
        HttpContext.Current.Session("NewUser") = False
        Dim app = CType(HttpContext.Current.Session("App"), clsComparionCore)
        Dim passCode = CStr(HttpContext.Current.Session(Constants.Sess_SignUp_Passcode))
        System.Threading.Thread.Sleep(1000)
        Dim authRes = app.Logon(email, password, passCode, False, True, False)
        Dim isPass = False
        Dim message = ""
        Dim output = New Object()
        Dim oSerializer = New System.Web.Script.Serialization.JavaScriptSerializer()

        If authRes = ecAuthenticateError.aeNoErrors Then
            HttpContext.Current.Session("User") = app.ActiveUser
            HttpContext.Current.Session("Project") = app.ActiveProject
            Login.StartAnytime(app.ActiveProject.ID)
            isPass = True
            ChangeLastHasToUserSpecific(app)
        Else
            message = ProcessLoginAuthentication(app, authRes, email)
        End If

        output = New With {
        Key .message = message,
        Key .pass = isPass
    }
        Return oSerializer.Serialize(output)
    End Function

    Private Shared Function ProcessLoginAuthentication(ByVal app As clsComparionCore, ByVal authRes As ecAuthenticateError, ByVal email As String) As String
        Dim message = TeamTimeClass.ParseAllTemplates(app.GetMessageByAuthErrorCode(authRes), app.ActiveUser, Nothing)

        If authRes = ecAuthenticateError.aeWrongPassword Then

            If Single.Parse(app.CanvasMasterDBVersion) >= 0.9996 AndAlso app.Database.Connect() AndAlso email <> "" Then

                Try
                    Dim user = app.DBUserByEmail(email)

                    If user IsNot Nothing Then
                        Dim newStatus = GetWrongPasswordAttemptsFromLog(app, user)
                        Dim oldStatus = user.PasswordStatus
                        user.PasswordStatus = newStatus

                        If user.PasswordStatus <> oldStatus Then
                            Dim sqlParams = New List(Of Object) From {
                                email
                            }
                            app.Database.ExecuteSQL($"UPDATE {clsComparionCore._TABLE_USERS} SET PasswordStatus = {user.PasswordStatus} WHERE {clsComparionCore._FLD_USERS_EMAIL}=?", sqlParams)

                            If user.PasswordStatus = Consts._DEF_PASSWORD_ATTEMPTS Then
                                CheckUserPasswordStatusAndSendEmail(app, user, Consts._DEF_PASSWORD_ATTEMPTS)
                            End If

                            System.Threading.Thread.Sleep(1000)
                        End If

                        If user.PasswordStatus >= Consts._DEF_PASSWORD_ATTEMPTS Then
                            app.DBUpdateDateTime(clsComparionCore._TABLE_USERS, clsComparionCore._FLD_USERS_LASTVISITED, user.UserID)
                            authRes = ecAuthenticateError.aeUserLockedByWrongPsw
                            message = TeamTimeClass.ParseAllTemplates(app.GetMessageByAuthErrorCode(authRes), app.ActiveUser, Nothing)
                        End If

                        app.DBSaveLogonEvent(email, authRes, ecAuthenticateWay.awRegular, HttpContext.Current.Request, message)
                    End If

                Catch ex As Exception
                End Try
            End If
        End If

        If authRes = ecAuthenticateError.aeUserLockedByWrongPsw Then
            message = String.Format(message, "Password Help?")
            message += "<br> " & String.Format(TeamTimeClass.ResString("msgAuthContact"), WebOptions.SystemEmail).Replace("class='error'", "style='text-decoration: underline;'")
        ElseIf authRes <> ecAuthenticateError.aeWrongPassword Then
            Dim passCode = CStr(HttpContext.Current.Session(Constants.Sess_SignUp_Passcode))

            If Not String.IsNullOrEmpty(passCode) Then
                Dim project = app.DBProjectByPasscode(passCode)
                Dim newMessage = AnytimeClass.GetProjectInAccessibleMessage(project, email, False)
                message = If(String.IsNullOrEmpty(newMessage), message, newMessage)
            End If
        End If

        Return message
    End Function

    Private Shared Function GetWrongPasswordAttemptsFromLog(ByVal app As clsComparionCore, ByVal user As clsApplicationUser) As Integer
        Dim errorCount = 0

        If Consts._DEF_PASSWORD_ATTEMPTS_AUTOUNLOCK Then
            Dim actionIds = $"{CInt(dbActionType.actLogon)},{CInt(dbActionType.actTokenizedURLLogon)},{CInt(dbActionType.actCredentialsLogon)},{CInt(dbActionType.actUnLock)}"
            Dim sqlCommand = $"SELECT * FROM Logs WHERE ActionID IN ({actionIds}) AND TypeID={CInt(dbObjectType.einfUser)} AND (ObjectID={user.UserID} OR COMMENT LIKE ?) AND DT>? ORDER BY DT"
            Dim sqlParams = New List(Of Object)()
            sqlParams.Add($"{user.UserEmail} %")
            sqlParams.Add(DateTime.Now.AddMinutes(-Consts._DEF_PASSWORD_ATTEMPTS_PERIOD))
            Dim tries = app.Database.SelectBySQL(sqlCommand, sqlParams)

            For Each row In tries
                'If row("Result") = DBNull.Value Then Continue For
                If row("Result") Is Nothing OrElse row("Result").ToString() = "" Then Continue For

                If Convert.ToInt32(row("ActionID")) = CInt(dbActionType.actUnLock) Then
                    errorCount = 0
                Else
                    Dim result = row("Result").ToString().ToLower()

                    If result.StartsWith(ecAuthenticateError.aeWrongPassword.ToString().ToLower()) OrElse result.StartsWith(ecAuthenticateError.aeUserLockedByWrongPsw.ToString().ToLower()) Then

                        If result.StartsWith(ecAuthenticateError.aeUserLockedByWrongPsw.ToString().ToLower()) AndAlso errorCount < Consts._DEF_PASSWORD_ATTEMPTS Then
                            errorCount = Consts._DEF_PASSWORD_ATTEMPTS
                        Else
                            errorCount += 1
                        End If
                    Else
                        errorCount = 0
                    End If
                End If
            Next
        Else
            errorCount = user.PasswordStatus
        End If

        errorCount = If(errorCount < 0, 0, errorCount)
        errorCount += 1
        errorCount = If(errorCount > Consts._DEF_PASSWORD_ATTEMPTS, Consts._DEF_PASSWORD_ATTEMPTS, errorCount)
        Return errorCount
    End Function

    Private Shared Function CheckUserPasswordStatusAndSendEmail(ByVal app As clsComparionCore, ByVal user As clsApplicationUser, ByVal maxPasswordAttempts As Integer) As Boolean
        Dim result = False

        If user IsNot Nothing AndAlso user.PasswordStatus = maxPasswordAttempts Then
            Dim errorString = ""
            app.DBSaveLog(dbActionType.actLock, dbObjectType.einfUser, user.UserID, "Lock user due to max login attempts" & (If(Consts._DEF_PASSWORD_ATTEMPTS_AUTOUNLOCK, " in a period", "")), ExpertChoice.Service.Common.GetClientIP(HttpContext.Current.Request))
            Dim subject = TeamTimeClass.ParseAllTemplates(TeamTimeClass.ResString("subjLockedPswAttempts", False, False), user, Nothing)
            Dim body = TeamTimeClass.ParseAllTemplates(TeamTimeClass.ResString("bodyLockedPswAttempts", False, False), user, Nothing)
            result = ECService.Common.SendMail(WebOptions.SystemEmail, user.UserEmail, subject, body, errorString, "", False, WebOptions.SMTPSSL())
            app.DBSaveLog(dbActionType.actSendEmail, dbObjectType.einfUser, user.UserID, "Email about locking account due to wrong psw", errorString)
        End If

        Return result
    End Function

End Class