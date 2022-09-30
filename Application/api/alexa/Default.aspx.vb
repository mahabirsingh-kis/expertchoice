Imports System.IO
Imports ExpertChoice.Structuring
Imports StructuringPage

Partial Class AlexaWebAPI
    Inherits clsComparionCorePage

    Private Const _TPL_ALEXA_USER_SIGNUP_EMAIL = "{0}@choosenow.us" ' D7188
    Private Const _OPT_ALEXA_LICENSE_KEY = "ALEXA"                  ' D7188

    Public Sub New()
        MyBase.New(_PGID_WEBAPI)
    End Sub

    Private Function _Page() As mpWebAPI
        Return CType(Master, mpWebAPI)
    End Function

    ' D7584 ===
    Public Shared Function CheckAlexaProjectMode(App As clsComparionCore) As Boolean
        Dim fRes As Boolean = False
        If App IsNot Nothing Then
            If App.isAuthorized AndAlso App.isAlexaUser AndAlso App.HasActiveProject AndAlso App.ActiveProject.ProjectManager.Parameters.SpecialMode = "" Then
                App.ActiveProject.ProjectManager.Parameters.SpecialMode = _OPT_MODE_ALEXA_PROJECT
                App.ActiveProject.ProjectManager.Parameters.Save()
                fRes = True
            End If
        End If
        Return fRes
    End Function
    ' D7584 ==

    Private Sub DBAppendAction(args As AntiguaOperationEventArgs, ProjectID As Integer)
        If ProjectID >= 0 Then
            args.DT = DateTime.Now.Ticks
            Dim sData = String.Format("{0}{1}{2}{1}{3}", args.DT, vbTab, If(args.isAnonymousAction, 1, 0), args.GetJSON())
            App.DBTeamTimeDataWrite(ProjectID, COMBINED_USER_ID, ecExtraProperty.StructuringJsonData, sData, False)
        End If
    End Sub

    'Public Function connectToCS(ProjectID As String, UserID As String, UserName As String) As jActionResult
    '    Dim tResult As New jActionResult With {.Result = ecActionResult.arSuccess}

    '    Dim args As New AntiguaConnectOperationEventArgs
    '    args.CmdCode = Command.Connect
    '    args.CmdOwner = CInt(UserID)
    '    Dim fSaveParams As Boolean = False
    '    args.Token = New UserToken With {.Email = JS_SafeString(UserName + "@" + "alexa"), .UserName = JS_SafeString(UserName)}
    '    Dim connected_user As New csUser With {.id = CInt(UserID), .name = args.Token.UserName, .email = args.Token.Email }
    '    args.Tag = JsonConvert.SerializeObject(connected_user)
    '    args.isAnonymousAction = True

    '    DBAppendAction(args, CInt(ProjectID))

    '    Return tResult
    'End Function

    'Public Function disconnectFromCS(ProjectID As String, UserID As String) As jActionResult
    '    Dim tResult As New jActionResult With {.Result = ecActionResult.arSuccess}

    '    Dim args As New AntiguaDisconnectUserEventArgs
    '    args.CmdCode = Command.DisconnectUser
    '    args.CmdOwner = CInt(UserID)
    '    args.isAnonymousAction = True
    '    Dim disconnected_user As New csUser With {.id = CInt(UserID) }
    '    args.Tag = JsonConvert.SerializeObject(disconnected_user)

    '    DBAppendAction(args, CInt(ProjectID))

    '    Return tResult
    'End Function

    Public Function addAlternativeInCS(ProjectID As String, UserID As String, UserName As String, UserEmail As String, AlternativeName As String) As jActionResult
        Dim tResult As New jActionResult With {.Result = ecActionResult.arError}

        'Dim args As AntiguaAddNodeOperationEventArgs = New AntiguaAddNodeOperationEventArgs
        'args.CmdCode = Command.AddNode
        'args.CmdOwner = CInt(UserID)
        'args.isAnonymousAction = True
        'args.NodeID = Guid.NewGuid()
        'args.NodeTitle = AlternativeName
        'args.IsAlternative = True
        'args.ParentNodeID = Guid.Empty

        'If Not String.IsNullOrEmpty(args.NodeTitle) Then
        '    DBAppendAction(args, CInt(ProjectID))
        '    tResult.Result = ecActionResult.arSuccess
        'End If

        Dim NodeGuid As Guid = Guid.NewGuid
        Dim node As New clsVisualNode
        node.IsAlternative = True
        node.GuidID = NodeGuid
        node.Attributes = New clsVisualNodeAttributes
        node.Attributes.X = 0
        node.Attributes.Y = 0
        node.Attributes.Width = 200
        node.Attributes.Height = 50
        Dim itemTitle As String = AlternativeName
        node.Text = itemTitle
        node.Author = String.Format("{0} ({1})", UserName, UserEmail)
        node.LastModifiedBy = String.Format("{0} ({1})", UserName, UserEmail)
        node.Attributes.BackGroundColor = HexStringToInt(PM.Parameters.CS_DefaultAlternativeColor)
        node.Location = GUILocation.Board

        Dim args As New AntiguaNewNodeOperationEventArgs
        args.CmdCode = Command.CreateNewNode
        args.isAnonymousAction = True
        args.CmdOwner = CInt(UserID)
        args.MeetingOwner = PM.Parameters.CS_MeetingOwner
        args.Node = node

        DBAppendAction(args, CInt(ProjectID))

        Return tResult
    End Function

    ' D7188 ===
    Public Function RegisterNewUser(UserID As String, Optional UserName As String = "", Optional GroupName As String = "") As jActionResult   ' D7206
        Dim tResult As New jActionResult
        ' D7209 ===
        If String.IsNullOrEmpty(UserID) Then
            tResult.Result = ecActionResult.arError
            tResult.Message = "UserID is not specified"
            tResult.Data = ecAuthenticateError.aeNoUserFound.ToString    ' D7191 + D7198
        Else
            ' D7209 ==
            If String.IsNullOrEmpty(UserName) Then
                tResult.Result = ecActionResult.arError
                tResult.Message = "User Name is not specified"
                tResult.Data = ecAuthenticateError.aeNoUserFound.ToString    ' D7191 + D7198
            Else
                Dim sError As String = ""
                Dim tCode As Object = Nothing
                Dim AuthCode As ecAuthenticateError = ecAuthenticateError.aeUnknown ' D7247
                Dim DBUser As clsApplicationUser = GetUserByUserID(UserID, sError, AuthCode)
                tResult.Data = AuthCode.ToString    ' D7247
                If DBUser IsNot Nothing Then
                    tResult.Result = ecActionResult.arError
                    tResult.Message = "User is already registered"
                    tResult.Data = ecAuthenticateError.aeWrongCredentials.ToString
                    tResult.Tag = jUserShort.CreateFromBaseObject(DBUser)   ' D7209
                    Return tResult
                End If
                Dim tUser As clsApplicationUser = Nothing
                Dim sNameBase As String = SafeFileName(RemoveHTMLTags(UserName.Trim).Replace(" ", ""))  ' D7191
                If sNameBase.Length > 16 Then sNameBase = SubString(sNameBase, 15)  ' D7207 // AD: the max len limited as 32 symbols and @choosenow.us will be added
                Dim sExistedUser As String = ""
                Dim cnt As Integer = 10
                While tUser Is Nothing AndAlso cnt > 0
                    Dim sEmail As String = String.Format(_TPL_ALEXA_USER_SIGNUP_EMAIL, String.Format("{0}{1}", sNameBase, clsMeetingID.ReNew().ToString.Substring(0, 3)))  ' D7191 + D7209
                    tUser = App.UserWithSignup(sEmail, UserName,, ShortString(String.Format("Created from Alexa skill / {0}", UserID), 990, True), sExistedUser)    ' D7209
                    If sExistedUser <> "" Then tUser = Nothing
                    cnt -= 1
                End While
                If tUser IsNot Nothing AndAlso sExistedUser = "" Then
                    App.ActiveUser = tUser  ' D7205
                    ' D7207 + D7209 ===
                    Dim SQL As String = "INSERT INTO PrivateURLs (URL, Hash, UserID, ProjectID, Created) VALUES (?, ?, ?, ?, ?);"
                    Dim tParams As New List(Of Object)
                    tParams.Add(UserID)
                    tParams.Add(tUser.UserEmail)
                    tParams.Add(tUser.UserID)
                    tParams.Add(_DEF_ALEXA_PRJID_FLAG)
                    tParams.Add(Now)
                    App.Database.ExecuteSQL(SQL, tParams)
                    ' D7207 + D7209 ==
                    ' D7206 ===
                    If String.IsNullOrEmpty(GroupName) Then
                        tResult.Result = ecActionResult.arSuccess
                        tResult.Data = ecAuthenticateError.aeNoWorkgroupSelected.ToString
                        tResult.Tag = jSessionStatus.CreateFromBaseObject(App, Session) ' D7205 + D7209
                        ' D7199 ==
                    Else
                        Return CreateGroup(UserID, GroupName)
                    End If
                    ' D7206 ==
                Else
                    tResult.Result = ecActionResult.arError
                    tResult.Message = "Unable to signup and create unique UserEmail. Try another User Name."
                    tResult.Data = ecAuthenticateError.aeWrongCredentials.ToString    ' D7191 + D7198
                End If
            End If
        End If
        Return tResult
    End Function

    Public Function CreateGroup(UserID As String, GroupName As String) As jActionResult
        Dim tResult As New jActionResult

        If String.IsNullOrEmpty(GroupName) Then
            tResult.Result = ecActionResult.arError
            tResult.Message = "Group name is not specified"
            tResult.Data = ecAuthenticateError.aeWrongCredentials.ToString   ' D7191
        Else
            Dim AuthCode As ecAuthenticateError = ecAuthenticateError.aeUnknown ' D7247
            Dim tUser As clsApplicationUser = GetUserByUserID(UserID, tResult.Message, AuthCode)       ' D7191 + D7199 + D7247
            tResult.Data = AuthCode ' D7247
            If tUser IsNot Nothing Then
                tResult.Message = ""

                Dim tWorkgroup As New clsWorkgroup With {
                    .Comment = "Created from Alexa skill",
                    .Created = Now(),
                    .LastVisited = Now(),
                    .Name = GroupName,
                    .ECAMID = tUser.UserID,
                    .OwnerID = tUser.UserID}

                Dim tNewParams As New clsParamsFile
                With tNewParams
                    .SetParameter(ecLicenseParameter.CommercialUseEnabled, 0)
                    .SetParameter(ecLicenseParameter.CreatedAt, CLng(Date2ULong(Now)))
                    .SetParameter(ecLicenseParameter.ExpirationDate, CLng(Date2ULong(Now.AddYears(3))))
                    .SetParameter(ecLicenseParameter.RiskEnabled, 0)
                End With

                Dim sName As String = File_CreateTempName()
                If tNewParams.Write(sName, _OPT_ALEXA_LICENSE_KEY) Then
                    Dim LicenseData() As Byte = IO.File.ReadAllBytes(sName)
                    tWorkgroup.License.LicenseContent = New MemoryStream(LicenseData)
                    tWorkgroup.License.LicenseKey = _OPT_ALEXA_LICENSE_KEY
                End If
                File_Erase(sName)

                If tWorkgroup.License IsNot Nothing Then
                    If App.DBWorkgroupUpdate(tWorkgroup, True, "Create new Alexa skill workgroup") Then
                        App.CheckWorkgroup(tWorkgroup, False)
                        App.AttachWorkgroup(tUser.UserID, tWorkgroup, tWorkgroup.GetDefaultRoleGroupID(ecRoleLevel.rlApplicationLevel, ecRoleGroupType.gtWorkgroupManager))
                    End If
                    tResult.Result = ecActionResult.arSuccess
                    If App.isAuthorized Then App.ActiveWorkgroup = tWorkgroup   ' D7203
                    App.UserWorkgroups = Nothing    ' D7206
                    tResult.Tag = jSessionStatus.CreateFromBaseObject(App, Session)    ' D7203 + D7209
                    tResult.Data = ecAuthenticateError.aeNoErrors.ToString   ' D7206
                Else
                    tResult.Result = ecActionResult.arError
                    tResult.Message = If(tWorkgroup Is Nothing, "Unable to create workgroup", "Unable to create a valid workgroup license")
                    tResult.Data = ecAuthenticateError.aeWrongLicense.ToString    ' D7191
                End If
            Else
                tResult.Result = ecActionResult.arError
            End If
        End If
        Return tResult
    End Function

    Private Function GetUserByUserID(UserID As String, ByRef sError As String, ByRef CodeID As ecAuthenticateError) As clsApplicationUser   ' D7191 + D7199 + D7247
        Dim tUser As clsApplicationUser = Nothing
        If String.IsNullOrEmpty(UserID) Then
            sError = "UserID is not specified"
            CodeID = ecAuthenticateError.aeNoUserFound     ' D7191 + D7199 + D7247
        Else
            Dim sEmail As String = ""
            Dim SQL As String = "SELECT hash FROM PrivateURLs WHERE URL LIKE ?" ' D7250
            Dim tParams As New List(Of Object)
            tParams.Add(UserID)
            Dim dt As Object = App.Database.ExecuteScalarSQL(SQL, tParams)
            If dt IsNot Nothing AndAlso Not IsDBNull(dt) Then sEmail = CStr(dt)
            If sEmail <> "" Then tUser = App.DBUserByEmail(sEmail)
            If tUser IsNot Nothing AndAlso tUser.CannotBeDeleted Then
                tUser = Nothing
                sError = "Not allowed to login in this way"                 ' D7206
                CodeID = ecAuthenticateError.aeUseRegularLogon      ' D7191 + D7191 + D7206 + D7247
            Else
                If tUser IsNot Nothing Then
                    If tUser.Status = ecUserStatus.usDisabled Then
                        sError = "User account is disabled"
                        tUser = Nothing
                        CodeID = ecAuthenticateError.aeUserLocked      ' D7191 + D7191 + D7247
                    Else
                        CodeID = App.Logon(tUser.UserEmail, tUser.UserPassword, "", True, AllowBlankPsw, True)  ' D7205 + D7327
                        ' D7584 ===
                        Select Case CodeID
                            Case ecAuthenticateError.aeNoErrors, ecAuthenticateError.aeNoProjectsFound, ecAuthenticateError.aeNoWorkgroupSelected
                                App.isAlexaUser = True
                        End Select
                        ' D7584 ==
                    End If
                Else
                    sError = "User not found"
                    CodeID = ecAuthenticateError.aeNoUserFound    ' D7191 + D7191 + D7206 + D7247
                End If
                ' D7247 ===
                Select Case CodeID
                    Case ecAuthenticateError.aeNoErrors
                    Case ecAuthenticateError.aeUnknown
                    Case Else
                        If String.IsNullOrEmpty(sError) Then sError = App.GetMessageByAuthErrorCode(CodeID)
                End Select
                ' D7247 ==
            End If
        End If
        Return tUser
    End Function

    Public Function LoginByUserID(UserID As String) As jActionResult
        Dim tResult As New jActionResult
        Dim AuthCode As ecAuthenticateError = ecAuthenticateError.aeUnknown ' D7247
        Dim tUser As clsApplicationUser = GetUserByUserID(UserID, tResult.Message, AuthCode)    ' D7191 + D7199 + D7247
        If tUser Is Nothing Then
            tResult.Result = ecActionResult.arError
        Else
            ' D7247 ===
            Select Case AuthCode
                Case ecAuthenticateError.aeNoErrors, ecAuthenticateError.aeNoWorkgroupSelected, ecAuthenticateError.aeNoProjectsFound
                    tResult.Result = ecActionResult.arSuccess
                Case Else
                    tResult.Result = ecActionResult.arError
            End Select
            tResult.Tag = jSessionStatus.CreateFromBaseObject(App, Session) ' D7209
        End If
        tResult.Data = AuthCode.ToString
        ' D7247 ==
        Return tResult
    End Function

    Public Function LoginByPIN(UserID As String, PIN As Integer) As jActionResult
        Dim tResult As New jActionResult
        Dim AuthCode As ecAuthenticateError = ecAuthenticateError.aeUnknown ' D7247

        Dim tPINUser As clsApplicationUser = Nothing
        Dim tPrj As clsProject = Nothing
        Dim sExtra As String = ""
        App.GetUserByPin(ecPinCodeType.mfaAlexa, PIN, tPINUser, tPrj, sExtra)  ' D7501 + D7502

        If tPINUser IsNot Nothing Then
            If tPINUser.CannotBeDeleted Then
                tPINUser = Nothing
                tResult.Message = "Not allowed to login in this way"
                AuthCode = ecAuthenticateError.aeUseRegularLogon
            End If
            If tPINUser IsNot Nothing AndAlso tPINUser.Status = ecUserStatus.usDisabled Then
                tResult.Message = "User account is disabled"
                tPINUser = Nothing
                AuthCode = ecAuthenticateError.aeUserLocked
            End If
        End If

        If tPINUser IsNot Nothing Then
            Dim OldPrjID As Integer = If(tPrj Is Nothing, -1, tPrj.ID)
            Dim OldWkgID As Integer = If(tPrj Is Nothing, -1, tPrj.WorkgroupID)
            ' D7250 ===
            Dim SQL As String = "DELETE FROM PrivateURLs WHERE URL LIKE ? OR Hash LIKE ?"
            Dim tParams As New List(Of Object)
            tParams.Add(UserID)
            tParams.Add(tPINUser.UserEmail)
            Dim Deleted As Integer = App.Database.ExecuteSQL(SQL, tParams)
            SQL = "INSERT INTO PrivateURLs (URL, Hash, UserID, ProjectID, Created) VALUES (?, ?, ?, ?, ?);"
            tParams.Clear()
            tParams.Add(UserID)
            tParams.Add(tPINUser.UserEmail)
            tParams.Add(tPINUser.UserID)
            tParams.Add(_DEF_ALEXA_PRJID_FLAG)
            tParams.Add(Now)
            App.Database.ExecuteSQL(SQL, tParams)
            App.DBSaveLog(dbActionType.actModify, dbObjectType.einfUser, tPINUser.UserID, String.Format("Re-link AlexaID to account '{0}' by PIN code", tPINUser.UserEmail), "", tPINUser.UserID, OldWkgID)
            AuthCode = ecAuthenticateError.aeNoErrors
            ' D7250 ==
            If tResult.Message = "" AndAlso OldWkgID > 0 Then
                Dim tWkg As clsWorkgroup = App.DBWorkgroupByID(OldWkgID)
                If tWkg IsNot Nothing Then
                    App.AttachWorkgroup(tPINUser.UserID, tWkg, tWkg.GetDefaultRoleGroupID(ecRoleLevel.rlApplicationLevel, ecRoleGroupType.gtWorkgroupManager), "Attach user to workgroup by AlexaID")
                    If tPrj IsNot Nothing Then
                        App.AttachProject(tPINUser, tPrj, False, tWkg.GetDefaultRoleGroupID(ecRoleLevel.rlModelLevel, ecRoleGroupType.gtProjectManager), "Attach user to model by AlexaID")
                    End If
                End If
            End If
            If tResult.Message = "" Then Return LoginByUserID(UserID)
        Else
            tResult.Result = ecActionResult.arError
            If tResult.Message = "" Then tResult.Message = "PIN code is invalid or expired"
        End If
        tResult.Data = AuthCode.ToString
        ' D7247 ==
        Return tResult
    End Function

    Public Function UnregisterUser(UserID As String) As jActionResult
        Dim tResult As New jActionResult
        Dim AuthCode As ecAuthenticateError = ecAuthenticateError.aeUnknown ' D7247
        Dim tUser As clsApplicationUser = GetUserByUserID(UserID, tResult.Message, AuthCode)    ' D7247
        tResult.Data = AuthCode.ToString    ' D7247
        If tUser Is Nothing Then
            tResult.Result = ecActionResult.arError
        Else
            If App.isAuthorized AndAlso App.ActiveUser.UserID = tUser.UserID Then App.Logout()
            Dim SQL As String = "DELETE FROM PrivateURLs WHERE URL LIKE ? OR Hash LIKE ?"
            Dim tParams As New List(Of Object)
            tParams.Add(UserID)
            tParams.Add(tUser.UserEmail)
            Dim Deleted As Integer = App.Database.ExecuteSQL(SQL, tParams)
            App.DBSaveLog(dbActionType.actDelete, dbObjectType.einfUser, tUser.UserID, "Detach linked user by AlexaID", CStr(Deleted), tUser.UserID)
            tResult.Result = ecActionResult.arSuccess
            'If App.DBUserDelete(tUser) Then
            '    tResult.Result = ecActionResult.arSuccess
            'Else
            '    tResult.Result = ecActionResult.arError
            '    tResult.Message = "Error on delete user. Check logs."
            'End If
        End If
        Return tResult
    End Function
    ' D7188 ==

    Private Sub PipeParamsWebAPI_Load(sender As Object, e As EventArgs) Handles Me.Load
        Select Case _Page.Action

            'Case "connectToCS".ToLower
            '    _Page.ResponseData = connectToCS(GetParam(_Page.Params, "ProjectID".ToLower, True), GetParam(_Page.Params, "UserID".ToLower, True), GetParam(_Page.Params, "UserName".ToLower, True))

            'Case "disconnectFromCS".ToLower
            '    _Page.ResponseData = disconnectFromCS(GetParam(_Page.Params, "ProjectID".ToLower, True), GetParam(_Page.Params, "UserID".ToLower, True))

            'Case "addAlternativeInCS".ToLower
            '    _Page.ResponseData = addAlternativeInCS(GetParam(_Page.Params, "ProjectID".ToLower, True), GetParam(_Page.Params, "UserID".ToLower, True), GetParam(_Page.Params, "UserName".ToLower, True), GetParam(_Page.Params, "UserEmail".ToLower, True), GetParam(_Page.Params, "AlternativeName".ToLower, True))

                ' D7188 ===
            Case "registernewuser"
                _Page.ResponseData = RegisterNewUser(GetParam(_Page.Params, "UserId".ToLower, True), GetParam(_Page.Params, "UserName".ToLower, True), GetParam(_Page.Params, "GroupName".ToLower, True))   ' D7206 + D7209

            Case "creategroup"
                _Page.ResponseData = CreateGroup(GetParam(_Page.Params, "UserId".ToLower, True), GetParam(_Page.Params, "GroupName".ToLower, True))

            Case "loginbyuserid"
                _Page.ResponseData = LoginByUserID(GetParam(_Page.Params, "UserId".ToLower, True))
                ' D7188 ==

                ' D7249 ===
            Case "loginbypin"
                Dim pin As Integer = 0
                Integer.TryParse(GetParam(_Page.Params, "pin", True), pin)
                _Page.ResponseData = LoginByPIN(GetParam(_Page.Params, "UserId".ToLower, True), pin)
                ' D7249 ==

            Case "unregisteruser"
                _Page.ResponseData = UnregisterUser(GetParam(_Page.Params, "UserId".ToLower, True))


        End Select
    End Sub

End Class