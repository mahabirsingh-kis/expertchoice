Imports ExpertChoice.Structuring

Partial Class StructuringPage
    Inherits clsComparionCorePage

    Public ReadOnly Property UserEmail As String
        Get
            Return If(App.isAuthorized, App.ActiveUser.UserEmail, App.Antigua_UserEmail)    ' D4920
        End Get
    End Property

    Public ReadOnly Property UserName As String
        Get
            Return If(App.isAuthorized, App.ActiveUser.UserName, App.Antigua_UserName)     ' D4920
        End Get
    End Property

    Public CSMode As StructuringMode = StructuringMode.Collaborative
    Public UserGuid As Guid = Guid.NewGuid
    Public Credentials As String = ""

    Public Property IsDebugModeOn As Boolean = False

    Private _PRJ As clsProject = Nothing
    Public ReadOnly Property PRJ As clsProject
        Get
            If _PRJ Is Nothing Then
                If App.isAuthorized AndAlso App.HasActiveProject Then
                    ' D4921 ===
                    With App
                        _PRJ = .ActiveProject
                        .Antigua_MeetingID = .ActiveProject.MeetingID
                        .Antigua_UserEmail = .ActiveUser.UserEmail
                        .Antigua_UserName = .ActiveUser.UserName
                        .Antigua_UserID = .ActiveUser.UserID
                    End With
                    ' D4921 ==
                Else
                    ' D4920 + D4996 + D9402 ===
                    _PRJ = AnonAntiguaProject()
                    Session(_SESS_PRJ_ANON_NAME) = _PRJ
                    ' D4920 + D4996 + D9402 ==
                End If
            End If
            Return _PRJ
        End Get
    End Property

    Public ReadOnly Property PM As clsProjectManager
        Get
            Return If(PRJ IsNot Nothing, PRJ.ProjectManager, Nothing)
        End Get
    End Property

    Private _CmdOwner As Integer = UNDEFINED_INTEGER_VALUE
    Public ReadOnly Property CmdOwner As Integer
        Get
            If _CmdOwner = UNDEFINED_INTEGER_VALUE Then 
                _CmdOwner = If(App.ActiveUser IsNot Nothing, App.ActiveUser.UserID, -HashByString(UserEmail)) ' anonymous users have negative IDs
            End If
            Return _CmdOwner
        End Get
    End Property

    Public ReadOnly Property WhiteboardWidth As Integer = 1600
    Public ReadOnly Property WhiteboardHeight As Integer = 1600

    Private ReadOnly Property _SESS_SERVICE_CONNECTION As String
        Get
            Return String.Format("StructuringService_{0}", If(PRJ IsNot Nothing, PRJ.ID, 0))
        End Get
    End Property

    Private _ServiceConnection As StructuringClient = Nothing
    Public ReadOnly Property ServiceConnection As StructuringClient
        Get
            If _ServiceConnection Is Nothing Then
                Dim tSess As Object = Session(_SESS_SERVICE_CONNECTION)
                If tSess IsNot Nothing Then
                    _ServiceConnection = CType(tSess, StructuringClient)
                Else
                    _ServiceConnection = New StructuringClient(PM, PRJ.ID)
                    ConnectService()
                    Session(_SESS_SERVICE_CONNECTION) = _ServiceConnection
                End If
            End If
            Return _ServiceConnection
        End Get
    End Property

    Private Sub Page_Unload(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Unload
        If _ServiceConnection IsNot Nothing Then Session(_SESS_SERVICE_CONNECTION) = _ServiceConnection
        If _PRJ IsNot Nothing AndAlso Not (App.isAuthorized AndAlso App.HasActiveProject) Then
            Session(_SESS_PRJ_ANON_NAME) = _PRJ
        End If
    End Sub

    ''' <summary>
    ''' Create StructuringService session object only, no real initialization, called only once for session
    ''' </summary>
    Public Sub ConnectService()
        Dim args As New AntiguaConnectOperationEventArgs
        args.CmdCode = Command.Connect
        args.CmdOwner = CmdOwner
        args.MeetingOwner = PM.Parameters.CS_MeetingOwner
        Credentials = GetCredentials()
        args.Credentials = Credentials
        args.InstanceID = App.DatabaseID
        args.TreeMode = CType(PM.Parameters.CS_TreeMode, MeetingMode)
        If args.TreeMode = MeetingMode.Impacts Then PM.ActiveHierarchy = EchierarchyID.hidImpact else PM.ActiveHierarchy = EchierarchyID.hidLikelihood
        args.BoardMode = CType(PM.Parameters.CS_BoardMode, MeetingMode)
        args.Token = New UserToken With {.TokenID = args.CmdOwner, .UserGuid = UserGuid, .Email = UserEmail, .UserName = UserName}
        _ServiceConnection.CurrentSession.UserToken = args.Token
        Dim UToken As UserToken = _ServiceConnection.CreateUserToken(CInt(PRJ.MeetingID), "", UserEmail, UserName, UserGuid)    ' D4920
        _ServiceConnection.Connect(args)
    End Sub

    Public Sub SetMeetingState(Value As MeetingState)
        Dim args As New AntiguaStateOperationEventArgs
        args.CmdCode = Command.SetMeetingState
        args.CmdOwner = CmdOwner
        args.State = Value
        args.MeetingOwner = If(PM IsNot Nothing, PM.Parameters.CS_MeetingOwner, -1)
        If App.isAuthorized Then
            PM.Parameters.CS_MeetingState = CInt(args.State)
            If args.State = MeetingState.Active OrElse args.State = MeetingState.Paused Then
                PM.Parameters.CS_MeetingOwner = App.ActiveUser.UserID
                args.MeetingOwner = PM.Parameters.CS_MeetingOwner
            End If
            If args.State = MeetingState.Stopped Then
                Dim sComment As String = ""
                Dim sMessage As String = ResString("lblSnapshotAfterCS")
                App.SnapshotSaveProject(ecSnapShotType.Auto, sMessage, App.ProjectID, True, sComment)
            End If
            If args.State = MeetingState.OwnerDisconnected OrElse args.State = MeetingState.Stopped Then
                PM.Parameters.CS_MeetingOwner = -1
                App.Antigua_ResetCredentials()  ' D6485
            End If
            If args.State = MeetingState.Active Then
                _ServiceConnection = Nothing ' in order to reconnect
            End If
            PM.Parameters.Save()        
        End If
        ServiceConnection.Operation(args)
        DBAppendAction(args)
    End Sub

    Public Function GetCredentials() As String
        If App.isAuthorized Then
            If App.HasActiveProject AndAlso App.CanUserModifyProject(App.ActiveUser.UserID, App.ProjectID, App.ActiveUserWorkgroup, App.ActiveWorkspace) Then
                Return New clsCredentials(App.ActiveUser.UserEmail, PRJ.Passcode, App.DatabaseID, -1, App.Options.SessionID).HashString
            End If
        End If
        Return ""
    End Function

    Public ReadOnly Property InvitationText() As String
        Get
            If Not App.isAuthorized Then Return ""
            Dim retVal As String = ResString("antiguatmplPlainText")       
            retVal = retVal.Replace("[Meeting URL]", MeetingURL).Replace("[Meeting Owner]", App.ActiveUser.DisplayName).Replace("[Meeting ID]", clsMeetingID.AsString(App.Antigua_MeetingID, clsMeetingID.ecMeetingIDType.Antigua))
            Return retVal           
        End Get
    End Property

    Public ReadOnly Property MeetingURL() As String
        Get
            Return If(App.isAuthorized, ApplicationURL(False, False) + PageURL(_PGID_START, String.Format("{0}={1}", _PARAM_MEETING_ID, clsMeetingID.AsString(App.Antigua_MeetingID, clsMeetingID.ecMeetingIDType.Antigua))), "")
        End Get
    End Property

    Private Sub SaveSetting(ID As Guid, valueType As AttributeValueTypes, value As Object)
        With PM
            .Attributes.SetAttributeValue(ID, UNDEFINED_USER_ID, valueType, value, Guid.Empty, Guid.Empty)
            .Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID, UNDEFINED_USER_ID)
        End With
    End Sub

    Public Sub New()
        MyBase.New(_PGID_ANTIGUA_MEETING)
    End Sub

    Public Readonly Property ActiveProjectDescription As String 
        Get
            Return If(PRJ IsNot Nothing, String.Format(ParseString(" for %%model%% {0}"), SafeFormString(ShortString(PRJ.ProjectName, 60, True))), "")
        End Get
    End Property     

    Public class csUser
        Public Property id As Integer
        Public Property email As String
        Public Property name As String
        Public Property color As String
        Public Property can_be_pm As Boolean
        Public Property can_change_pm As Boolean
        Public Property is_pm As Boolean
    End Class

    Public class csObj
        Public Property id As Integer
        Public Property guid As String
        Public Property text As String
        Public Property pid As Integer
        Public Property selected As Boolean
        Public Property expanded As Boolean = True
    End Class

    Public Function GetUsersList() As String
        Dim retVal As String = PM.Parameters.CS_UserList
        Try
            Dim list As List(Of csUser) = JsonConvert.DeserializeObject(Of List(Of csUser))(retVal)
        Catch ex As Exception
            retVal = "[]"
        End Try
        Return If(retVal = "", "[]", retVal)
    End Function

    Private Function GetCurrentCSHelpUrl() As String
        Dim retVal As String = ""
        If CSMode = StructuringMode.Collaborative Then
            Select Case PM.Parameters.CS_BoardMode
                Case MeetingMode.Alternatives
                    If PM.IsRiskProject Then
                        retVal = ResString("antiguaBraistorming_helpRisk")
                    Else
                        retVal = ResString("antiguaBraistorming_help")
                    End If
                    If PM.Parameters.CS_TreeMode = MeetingMode.Sources Then 
                        If PM.IsRiskProject Then
                            retVal = If(PM.ActiveHierarchy = ECHierarchyID.hidLikelihood, ResString("antiguaProsAndCons_helpLikelihood"), ResString("antiguaProsAndCons_helpImpact"))
                        Else
                            retVal = ResString("antiguaProsAndCons_help")
                        End If
                    End If
                Case MeetingMode.Sources
                    If PM.IsRiskProject Then
                        retVal = ResString("antiguaStructuring_helpLikelihood")
                    Else
                        retVal = ResString("antiguaStructuring_help")
                    End If
                    If PM.Parameters.CS_TreeMode = MeetingMode.Alternatives Then 
                        retVal = ResString("antiguaSourcesContribution_help")
                    End If
                Case MeetingMode.Impacts 
                    If PM.IsRiskProject Then
                        retVal = ResString("antiguaStructuring_helpImpact")
                    Else
                        retVal = ResString("antiguaStructuring_help")
                    End If
                    If PM.Parameters.CS_TreeMode = MeetingMode.Alternatives Then 
                        retVal = ResString("antiguaConsequencesContribution_help")
                    End If                   
            End Select
        Else
            Select Case PM.Parameters.CS_BoardMode
                Case MeetingMode.Alternatives
                    If PM.IsRiskProject Then
                        retVal = ResString("antiguaIndividualBraistorming_helpRisk")
                    Else
                        retVal = ResString("antiguaIndividualBraistorming_help")
                    End If                    
                Case MeetingMode.Sources
                    If PM.IsRiskProject Then
                        retVal = ResString("antiguaIndividualStructuring_helpLikelihood")
                    Else
                        retVal = ResString("antiguaIndividualStructuring_help")
                    End If
                    If PM.Parameters.CS_TreeMode = MeetingMode.Alternatives Then 
                        If PM.IsRiskProject Then
                            retVal = ResString("antiguaIndividualProsAndCons_helpLikelihood")
                        Else
                            retVal = ResString("antiguaIndividualProsAndCons_help")
                        End If
                    End If
                    If PM.Parameters.CS_TreeMode = MeetingMode.Alternatives Then 
                        retVal = ResString("antiguaIndividualSourcesContribution_help")
                    End If
                Case MeetingMode.Impacts
                    If PM.IsRiskProject Then
                        retVal = ResString("antiguaIndividualStructuring_helpImpact")
                    Else
                        retVal = ResString("antiguaIndividualStructuring_help")
                    End If
                    If PM.Parameters.CS_TreeMode = MeetingMode.Alternatives Then 
                        If PM.IsRiskProject Then
                            retVal = ResString("antiguaIndividualProsAndCons_helpImpact")
                        Else
                            retVal = ResString("antiguaIndividualProsAndCons_help")
                        End If
                    End If
                    If PM.Parameters.CS_TreeMode = MeetingMode.Alternatives Then 
                        retVal = ResString("antiguaIndividualConsequencesContribution_help")
                    End If
            End Select
        End If
        Return retVal
    End Function

    Protected Sub Page_InitComplete(sender As Object, e As System.EventArgs) Handles Me.InitComplete
        If PRJ Is Nothing Then FetchAccess()
        ' D4996 ===
        IsDebugModeOn = CheckVar("debug", False)
        If _PRJ IsNot Nothing AndAlso PM IsNot Nothing Then 
            ServiceConnection.CurrentMeeting.Info.ProjectManager = PM
        End If        
        If Not IsPostBack AndAlso Not isCallback AndAlso Not isAJAX Then
            If Not App.isAuthorized OrElse PM.Parameters.CS_MeetingOwner <> App.ActiveUser.UserID Then
                _PRJ.ResetProject(True)
                Session.Remove(_SESS_SERVICE_CONNECTION)
            End If
            PM.AntiguaDashboard.LoadPanel(ECModelStorageType.mstCanvasStreamDatabase, PM.StorageManager.ProjectLocation, PM.StorageManager.ProviderType, PRJ.ID)
            PM.AntiguaRecycleBin.LoadPanel(ECModelStorageType.mstCanvasStreamDatabase, PM.StorageManager.ProjectLocation, PM.StorageManager.ProviderType, PRJ.ID)
            PM.AntiguaInfoDocs.LoadAntiguaInfoDocs()
        End If
        If isAJAX Then Ajax_Callback(Request.Form.ToString)
        ' D4996 ==
    End Sub

    Private Sub AddUser(connected_user As csUser)
        If connected_user IsNot Nothing Then
            Try
                Dim users As List(Of csUser) = JsonConvert.DeserializeObject(Of List(Of csUser))(PM.Parameters.CS_UserList)
                users.RemoveAll(Function(u) u.id = connected_user.id)
                users.Add(connected_user)
                PM.Parameters.CS_UserList = JsonConvert.SerializeObject(users)
                PM.Parameters.Save()
            Catch ex As Exception
            End Try
        End If
    End Sub

    Public Sub RemoveUser(disconnected_user As csUser)
        If disconnected_user IsNot Nothing Then
            Try
                Dim users As List(Of csUser) = JsonConvert.DeserializeObject(Of List(Of csUser))(PM.Parameters.CS_UserList)
                Dim i As Integer = 0
                While i < users.Count 
                    If users(i).id = disconnected_user.id Then 
                        users.RemoveAt(i)
                    Else
                        i += 1
                    End If
                End While
                PM.Parameters.CS_UserList = JsonConvert.SerializeObject(users)
                PM.Parameters.Save()
            Catch ex As Exception

            End Try
        End If
    End Sub

    Protected Sub Ajax_Callback(data As String)
        Dim httpargs As NameValueCollection = HttpUtility.ParseQueryString(data)
        Dim sAction As String = GetParam(httpargs, "action").ToLower ' Anti-XSS
        Dim tResult As String = ""

        'Dim Cmd As Command = Command.NoSuccess
        Dim tCmd As Integer = Command.NoSuccess

        Select Case sAction
            Case "do_anonymous_actions" ' executing the actions performed by regular participants via meeting owner's instance
                Dim sData As String = GetParam(httpargs, "data")
                Dim sAllArgs As String() = sData.Split(CChar(vbTab))

                For Each sArgs As String In sAllArgs
                    If Not String.IsNullOrEmpty(sArgs) Then
                        Dim args As AntiguaOperationEventArgs = AntiguaOperationEventArgs.ReadJSON(sArgs)
                        args.isAnonymousAction = False
                        If ServiceConnection.Operation(args) Then
                            Try
                                If args.CmdCode = Command.Connect Then 
                                    AddUser(JsonConvert.DeserializeObject(Of csUser)(CStr(args.Tag)))
                                End If
                                If args.CmdCode = Command.DisconnectUser Then 
                                    RemoveUser(JsonConvert.DeserializeObject(Of csUser)(CStr(args.Tag)))
                                End If
                                DBAppendAction(args)
                            Catch ex As Exception

                            End Try
                        End If
                    End If
                Next

            Case "do"
                Dim sData As String = GetParam(httpargs, "data")
                Dim isAnonymousAction As Boolean = CmdOwner <> PM.Parameters.CS_MeetingOwner

                'Cmd = CType(GetParam(httpargs, "cs_action"), Command)
                tCmd = Param2Int(httpargs, "cs_action")  ' D7598

                Select Case tCmd    ' D7598
                    Case Command.Connect
                        If PM IsNot Nothing Then    ' D4920 + D4921
                            Dim args As New AntiguaConnectOperationEventArgs
                            args.CmdCode = Command.Connect
                            args.CmdOwner = CmdOwner
                            Dim fSaveParams As Boolean = False
                            Dim tAppUser As clsApplicationUser = App.DBUserByEmail(UserEmail)
                            If tAppUser IsNot Nothing AndAlso App.isAuthorized AndAlso App.HasActiveProject AndAlso App.CanUserModifyProject(App.ActiveUser.UserID, App.ProjectID, App.ActiveUserWorkgroup, App.ActiveWorkspace, App.ActiveWorkgroup) AndAlso PM.Parameters.CS_MeetingOwner <> App.ActiveUser.UserID Then  ' D4921
                                PM.Parameters.CS_MeetingOwner = App.ActiveUser.UserID
                                isAnonymousAction = False
                                fSaveParams = True
                                args.CanChangePM = False
                                args.IsPM = True
                            Else
                                If tAppUser Is Nothing Then
                                    args.CanChangePM = False
                                    args.IsPM = False
                                Else
                                    ' D6973 ===
                                    Dim tUW As clsUserWorkgroup = Nothing
                                    If App.ActiveWorkgroup IsNot Nothing Then
                                        tUW = App.DBUserWorkgroupByUserIDWorkgroupID(tAppUser.UserID, App.ActiveWorkgroup.ID)
                                    Else
                                        If PRJ IsNot Nothing Then tUW = App.DBUserWorkgroupByUserIDWorkgroupID(tAppUser.UserID, PRJ.WorkgroupID)
                                    End If
                                    ' D6973 ==
                                    Dim tWS = App.DBWorkspaceByUserIDProjectID(tAppUser.UserID, PRJ.ID)
                                    ' D6983 ===
                                    args.IsPM = tWS IsNot Nothing AndAlso tAppUser IsNot Nothing AndAlso App.ActiveWorkgroup IsNot Nothing AndAlso App.CanUserModifyProject(tAppUser.UserID, PRJ.ID, tUW, tWS)
                                    args.CanBePM = args.IsPM OrElse (App.ActiveWorkgroup IsNot Nothing AndAlso App.CanUserBePM(App.ActiveWorkgroup, tAppUser.UserID, PRJ, False, True, tUW, tWS))
                                    args.CanChangePM = tWS Is Nothing OrElse (App.ActiveWorkgroup IsNot Nothing AndAlso Not App.CanUserDoAction(ecActionType.at_alManageAnyModel, tUW, App.ActiveWorkgroup)) 'AndAlso args.CanBePM
                                    ' D6983 ===
                                End If
                            End If
                            args.MeetingOwner = PM.Parameters.CS_MeetingOwner
                            args.Token = New UserToken With {.Email = JS_SafeString(UserEmail), .UserName = JS_SafeString(UserName)}
                            Try
                                Dim connected_user As csUser = JsonConvert.DeserializeObject(Of csUser)(GetParam(httpargs, "connected_user"))
                                args.Tag = JsonConvert.SerializeObject(connected_user)
                                args.isAnonymousAction = isAnonymousAction

                                If PM.Parameters.CS_MeetingOwner = CmdOwner Then
                                    AddUser(connected_user)
                                End If

                                If ServiceConnection.Operation(args) Then
                                    DBAppendAction(args)
                                End If

                                If PM.Parameters.CS_MeetingOwner = CmdOwner AndAlso App.isAuthorized AndAlso PM.Parameters.CS_MeetingState <> MeetingState.Active AndAlso PM.Parameters.CS_MeetingState <> MeetingState.Paused Then
                                    SetMeetingState(MeetingState.Active)
                                End If

                            Catch ex As Exception

                            End Try
                        End If

                    Case Command.DisconnectUser
                        Dim args As New AntiguaDisconnectUserEventArgs
                        args.CmdCode = Command.DisconnectUser
                        args.CmdOwner = CmdOwner
                        args.isAnonymousAction = isAnonymousAction
                        Try
                            Dim disconnected_user As csUser = JsonConvert.DeserializeObject(Of csUser)(GetParam(httpargs, "user_disconnected"))
                            args.Tag = JsonConvert.SerializeObject(disconnected_user)
                            If Not isAnonymousAction AndAlso PM IsNot Nothing Then
                                'args.MeetingOwner = PM.Parameters.CS_MeetingOwner
                                If App.isAuthorized Then
                                    PM.Parameters.CS_MeetingOwner = -1
                                End If
                                RemoveUser(disconnected_user)
                                If App.isAuthorized Then
                                    SetMeetingState(MeetingState.OwnerDisconnected)
                                End If
                            End If
                            DBAppendAction(args)
                        Catch ex As Exception

                        End Try
                    Case Command.RefreshUsersList
                        Dim args As New AntiguaConnectOperationEventArgs
                        args.CmdCode = Command.RefreshUsersList
                        args.CmdOwner = CmdOwner
                        args.Tag = "{}"
                        Dim fSaveParams As Boolean = False
                        If HasParam(httpargs, "offline_users") Then
                            Dim offlineUserIDs As List(Of Integer) = Param2IntList(GetParam(httpargs, "offline_users"))
                            If offlineUserIDs.Count > 0 Then
                                Try
                                    Dim users As List(Of csUser) = JsonConvert.DeserializeObject(Of List(Of csUser))(PM.Parameters.CS_UserList)
                                    Dim i As Integer = 0
                                    While i < users.Count
                                        If offlineUserIDs.Contains(users(i).id) Then
                                            users.RemoveAt(i)
                                        Else
                                            i += 1
                                        End If
                                    End While
                                    PM.Parameters.CS_UserList = JsonConvert.SerializeObject(users)
                                    PM.Parameters.Save()
                                Catch ex As Exception

                                End Try
                            End If
                            args.Tag = PM.Parameters.CS_UserList
                            fSaveParams = True
                        End If
                        DBAppendAction(args)

                    'Case Command.DisconnectUserWhenMeetingStopped
                    '    Dim args As New AntiguaConnectOperationEventArgs
                    '    args.CmdCode = Command.DisconnectUserWhenMeetingStopped
                    '    args.CmdOwner = CmdOwner                        
                    '    DBAppendAction(args)

                    Case Command.SetMeetingState
                        Dim tState = Param2Int(httpargs, "value")   ' D7598
                        If tState >= 0 Then SetMeetingState(CType(tState, MeetingState))    ' D7598
                        'SetMeetingState(CType(GetParam(httpargs, "value"), MeetingState))

                    Case Command.SetMeetingMode
                        Dim args As New AntiguaStateOperationEventArgs
                        args.CmdCode = Command.SetMeetingMode
                        args.CmdOwner = CmdOwner
                        args.MeetingOwner = PM.Parameters.CS_MeetingOwner
                        Dim tTreeMode As Integer, tBoardMode As Integer
                        If Integer.TryParse(GetParam(httpargs, "tree_mode"), tTreeMode) AndAlso Integer.TryParse(GetParam(httpargs, "board_mode"), tBoardMode) Then
                            args.TreeMode = CType(tTreeMode, MeetingMode)
                            args.BoardMode = CType(tBoardMode, MeetingMode)
                            args.CSMode = CSMode
                            If App.isAuthorized Then
                                If args.TreeMode = MeetingMode.Impacts Then PM.ActiveHierarchy = ECHierarchyID.hidImpact Else PM.ActiveHierarchy = ECHierarchyID.hidLikelihood
                                If PM.Parameters.CS_TreeMode <> args.TreeMode Then PM.Parameters.CS_TreeMode = args.TreeMode
                                If PM.Parameters.CS_BoardMode <> args.BoardMode Then PM.Parameters.CS_BoardMode = args.BoardMode
                                PM.Parameters.Save()
                            End If
                            DBAppendAction(args)
                        End If

                    Case Command.CreateNewNode
                        Dim NodeGuid As Guid = Guid.NewGuid
                        Dim X As Double
                        String2Double(GetParam(httpargs, "x"), X)
                        Dim Y As Double
                        String2Double(GetParam(httpargs, "y"), Y)
                        Dim sTempId As String = GetParam(httpargs, "temp_id")
                        ' D7598 ===
                        Dim tWidth As Integer = Param2Int(httpargs, "w")
                        Dim tHeight As Integer = Param2Int(httpargs, "h")
                        Dim tMode As Integer = Param2Int(httpargs, "board_mode")
                        Dim tBoardMode As MeetingMode = MeetingMode.Alternatives
                        If tMode >= 0 Then tBoardMode = CType(tMode, MeetingMode)
                        ' D7598 ==
                        Dim node As New clsVisualNode
                        node.IsAlternative = Str2Bool(GetParam(httpargs, "is_alt"))
                        node.GuidID = NodeGuid
                        node.Attributes = New clsVisualNodeAttributes
                        node.Attributes.X = X
                        node.Attributes.Y = Y
                        node.Attributes.Width = tWidth
                        node.Attributes.Height = tHeight
                        Dim itemTitle As String = If(node.IsAlternative, PM.Parameters.CS_DefaultAlternativeTitle, PM.Parameters.CS_DefaultObjectiveTitle)
                        If httpargs("title") IsNot Nothing Then
                            itemTitle = GetParam(httpargs, "title")
                        End If
                        node.Text = itemTitle
                        node.Author = String.Format("{0} ({1})", UserName, UserEmail)
                        node.LastModifiedBy = String.Format("{0} ({1})", UserName, UserEmail)
                        Dim hexColor As String = If(node.IsAlternative, PM.Parameters.CS_DefaultAlternativeColor, If(tBoardMode = MeetingMode.Impacts, PM.Parameters.CS_DefaultObjectiveColor, PM.Parameters.CS_DefaultSourceColor))
                        If httpargs("color") IsNot Nothing Then
                            hexColor = GetParam(httpargs, "color")
                        End If
                        node.Attributes.BackGroundColor = HexStringToInt(hexColor)
                        node.Location = GUILocation.Board
                        If tBoardMode = MeetingMode.Impacts Then node.Location = GUILocation.BoardImpact
                        Dim args As New AntiguaNewNodeOperationEventArgs
                        args.CmdCode = Command.CreateNewNode
                        args.isAnonymousAction = isAnonymousAction
                        args.CmdOwner = CmdOwner
                        args.MeetingOwner = PM.Parameters.CS_MeetingOwner
                        args.Node = node
                        args.TmpID = sTempId
                        If ServiceConnection.Operation(args) Then
                            DBAppendAction(args)
                        End If

                    Case Command.MoveNodesOnBoard
                        Dim X As Integer = Param2Int(httpargs, "x") ' D7598
                        Dim Y As Integer = Param2Int(httpargs, "y") ' D7598
                        Dim sNodeGuid As String = GetParam(httpargs, "guid")
                        Dim args As New AntiguaMoveToBoardEventArgs
                        args.CmdCode = Command.MoveNodesOnBoard
                        args.isAnonymousAction = isAnonymousAction
                        args.CmdOwner = CmdOwner
                        args.MeetingOwner = PM.Parameters.CS_MeetingOwner
                        args.Position = New Drawing.Point(X, Y)
                        If Guid.TryParse(sNodeGuid, args.NodeGuid) Then
                            If ServiceConnection.Operation(args) Then
                                DBAppendAction(args)
                            End If
                        End If

                    Case Command.DeleteNodes
                        Dim sNodeGuids As String = GetParam(httpargs, "guids")
                        Dim ListToRemove As List(Of Guid) = Param2GuidList(sNodeGuids)
                        Dim IsWhiteboardItems As Boolean = Str2Bool(GetParam(httpargs, "whiteboard_items"))

                        If ListToRemove IsNot Nothing AndAlso ListToRemove.Count > 0 Then
                            Dim args As New AntiguaDeleteOperationEventArgs
                            args.CmdCode = Command.DeleteNodes
                            args.isAnonymousAction = isAnonymousAction
                            args.CmdOwner = CmdOwner
                            args.MeetingOwner = PM.Parameters.CS_MeetingOwner
                            args.NodesGuids = ListToRemove
                            args.IsWhiteboardItems = IsWhiteboardItems
                            If ServiceConnection.Operation(args) Then
                                DBAppendAction(args)
                            End If
                        End If

                    Case Command.RenameItem
                        Dim sNodeGuid As String = GetParam(httpargs, "guid")
                        Dim sText As String = GetParam(httpargs, "name")
                        Dim args As New AntiguaPropertiesOperationEventArgs
                        args.CmdCode = Command.RenameItem
                        args.isAnonymousAction = isAnonymousAction
                        args.CmdOwner = CmdOwner
                        args.MeetingOwner = PM.Parameters.CS_MeetingOwner
                        args.Title = sText.Trim
                        If Guid.TryParse(sNodeGuid, args.NodeGuid) Then
                            If ServiceConnection.Operation(args) Then
                                DBAppendAction(args)
                            End If
                        End If

                    Case Command.ResizeItem
                        Dim sItemGuid As String = GetParam(httpargs, "item_guid")
                        Dim tWidth As Integer = Param2Int(httpargs, "width")    ' D7598
                        Dim tHeight As Integer = Param2Int(httpargs, "height")  ' D7598
                        Dim args As New AntiguaPropertiesOperationEventArgs
                        args.CmdCode = Command.ResizeItem
                        args.isAnonymousAction = isAnonymousAction
                        args.CmdOwner = CmdOwner
                        args.MeetingOwner = PM.Parameters.CS_MeetingOwner
                        args.Width = tWidth
                        args.Height = tHeight
                        If Guid.TryParse(sItemGuid, args.NodeGuid) Then
                            If ServiceConnection.Operation(args) Then
                                DBAppendAction(args)
                            End If
                        End If

                    Case Command.SetNodesColor
                        Dim sNodeGuid As String = GetParam(httpargs, "guid")
                        Dim sColor As String = GetParam(httpargs, "color")
                        Dim args As New AntiguaPropertiesOperationEventArgs
                        args.CmdCode = Command.SetNodesColor
                        args.isAnonymousAction = isAnonymousAction
                        args.CmdOwner = CmdOwner
                        args.MeetingOwner = PM.Parameters.CS_MeetingOwner
                        args.sColor = sColor.Trim
                        Dim tGuid As Guid
                        If Guid.TryParse(sNodeGuid, tGuid) Then
                            args.NodeGuid = tGuid
                            If ServiceConnection.Operation(args) Then
                                DBAppendAction(args)
                            End If
                        End If

                    Case Command.SendToAlternatives, Command.SendToObjectives ' Move node
                        Dim sNodeGuid As String = GetParam(httpargs, "guid")
                        Dim args As New AntiguaReorderOperationEventArgs
                        args.CmdCode = CType(tCmd, Command)
                        args.isAnonymousAction = isAnonymousAction
                        args.CmdOwner = CmdOwner
                        args.MeetingOwner = PM.Parameters.CS_MeetingOwner
                        If Guid.TryParse(sNodeGuid, args.SourceNodeGuid) Then
                            args.DestNodeGuid = args.SourceNodeGuid
                            If ServiceConnection.Operation(args) Then
                                DBAppendAction(args)
                            End If
                        End If

                    Case Command.AddNode
                        Dim args As AntiguaAddNodeOperationEventArgs = New AntiguaAddNodeOperationEventArgs
                        args.CmdCode = CType(tCmd, Command)
                        args.CmdOwner = CmdOwner
                        args.isAnonymousAction = isAnonymousAction
                        args.MeetingOwner = PM.Parameters.CS_MeetingOwner
                        args.NodeID = Guid.NewGuid()
                        args.NodeTitle = GetParam(httpargs, "name").Trim()
                        args.IsAlternative = Str2Bool(GetParam(httpargs, "is_alt").Trim())
                        args.ParentNodeID = If(args.IsAlternative, Guid.Empty, PM.ActiveObjectives.Nodes(0).NodeGuidID)
                        Dim ParentNodeGuid As Guid = Guid.Empty
                        Dim sPGuid As String = GetParam(httpargs, "parent_node_guid").Trim()
                        If sPGuid <> "" AndAlso Guid.TryParse(sPGuid, ParentNodeGuid) Then
                            args.ParentNodeID = ParentNodeGuid
                        End If

                        If Not String.IsNullOrEmpty(args.NodeTitle) AndAlso ServiceConnection.Operation(args) Then
                            DBAppendAction(args)
                        End If

                    Case Command.MoveFromBoardToTree ' Move objective from board to tree
                        Dim sTargetGuid As String = GetParam(httpargs, "target_guid")
                        Dim sNodeGuid As String = GetParam(httpargs, "source_guid")
                        Dim tNodeGuid As Guid
                        Dim tTargetGuid As Guid
                        Guid.TryParse(sTargetGuid, tTargetGuid)
                        If Guid.TryParse(sNodeGuid, tNodeGuid) Then
                            Dim tDropPosition As Integer = Param2Int(httpargs, "drop_action")   ' D7598
                            Dim args As New AntiguaMoveToHierarchyOperationEventArgs
                            args.CmdCode = CType(tCmd, Command)
                            args.isAnonymousAction = isAnonymousAction
                            args.CmdOwner = CmdOwner
                            args.MeetingOwner = PM.Parameters.CS_MeetingOwner
                            args.NodesGuids = New List(Of Guid)
                            args.NodesGuids.Add(tNodeGuid)
                            args.DestNodeGuid = If(String.IsNullOrEmpty(sTargetGuid), PM.ActiveObjectives.Nodes(0).NodeGuidID, tTargetGuid)
                            args.Position = tDropPosition
                            args.Action = If(tDropPosition < 0, NodeMoveAction.nmaAsChildOfNode, CType(tDropPosition, NodeMoveAction))
                            If ServiceConnection.Operation(args) Then
                                DBAppendAction(args)
                            End If
                        End If

                    Case Command.MoveAltToList ' Copy from whiteboard to alts
                        Dim sTargetGuid As String = GetParam(httpargs, "target_guid")
                        Dim sNodeGuid As String = GetParam(httpargs, "source_guid")
                        Dim tNodeGuid As Guid
                        If Guid.TryParse(sNodeGuid, tNodeGuid) Then
                            Dim tDropPosition As Integer = Param2Int(httpargs, "pos")   ' D7598
                            Dim args As New AntiguaMoveToHierarchyOperationEventArgs
                            args.CmdCode = CType(tCmd, Command)
                            args.isAnonymousAction = isAnonymousAction
                            args.CmdOwner = CmdOwner
                            args.MeetingOwner = PM.Parameters.CS_MeetingOwner
                            args.NodesGuids = New List(Of Guid)
                            args.NodesGuids.Add(tNodeGuid)
                            Dim tTargetGuid As Guid
                            Guid.TryParse(sTargetGuid, tTargetGuid)
                            args.DestNodeGuid = If(String.IsNullOrEmpty(sTargetGuid), Guid.Empty, tTargetGuid)
                            args.Position = tDropPosition
                            If ServiceConnection.Operation(args) Then
                                DBAppendAction(args)
                            End If
                        End If

                    Case Command.CopyAltToBoard, Command.MoveFromTreeToBoard
                        Dim sNodeGuid As String = GetParam(httpargs, "guid")
                        Dim tNodeGuid As Guid
                        If Guid.TryParse(sNodeGuid, tNodeGuid) Then
                            Dim args As New AntiguaMoveToBoardEventArgs
                            args.CmdCode = CType(tCmd, Command)
                            args.isAnonymousAction = isAnonymousAction
                            args.CmdOwner = CmdOwner
                            args.MeetingOwner = PM.Parameters.CS_MeetingOwner
                            args.NodeGuid = tNodeGuid
                            Select Case PM.Parameters.CS_BoardMode
                                Case MeetingMode.Alternatives
                                    args.Location = GUILocation.Alternatives
                                Case MeetingMode.Sources
                                    args.Location = GUILocation.Board
                                Case MeetingMode.Impacts
                                    args.Location = GUILocation.BoardImpact
                            End Select
                            ' D7598 ===
                            Dim X As Integer = Param2Int(httpargs, "x")
                            Dim Y As Integer = Param2Int(httpargs, "y")
                            Dim W As Integer = Param2Int(httpargs, "w")
                            Dim H As Integer = Param2Int(httpargs, "h")
                            ' D7598 ==
                            args.Position = New Drawing.Point With {.X = CInt(X), .Y = CInt(Y)}
                            args.Size = New Drawing.Point With {.X = CInt(W), .Y = CInt(H)}
                            If ServiceConnection.Operation(args) Then
                                DBAppendAction(args)
                            End If
                        End If

                    Case Command.ReorderInAlts, Command.ReorderInTree
                        Dim tSourceNodeGuid As Guid
                        Dim tTargetNodeGuid As Guid
                        If Guid.TryParse(GetParam(httpargs, "source_guid"), tSourceNodeGuid) AndAlso Guid.TryParse(GetParam(httpargs, "target_guid"), tTargetNodeGuid) Then
                            Dim tDropPosition As Integer = Param2Int(httpargs, "pos")   ' D7598

                            Dim args As New AntiguaReorderOperationEventArgs
                            args.CmdCode = CType(tCmd, Command) 'Command.ReorderInAlts or Command.ReorderInTree
                            args.isAnonymousAction = isAnonymousAction
                            args.Action = CType(tDropPosition, NodeMoveAction)
                            args.DestNodeGuid = tTargetNodeGuid
                            args.SourceNodeGuid = tSourceNodeGuid
                            args.CmdOwner = CmdOwner
                            args.MeetingOwner = PM.Parameters.CS_MeetingOwner
                            If ServiceConnection.Operation(args) Then
                                DBAppendAction(args)
                            End If
                        End If

                    Case Command.ChatMessage
                        Dim args As New AntiguaChatOperationEventArgs
                        args.AtTime = DateTime.Now
                        args.CmdCode = Command.ChatMessage
                        args.CmdOwner = CmdOwner
                        args.MeetingOwner = PM.Parameters.CS_MeetingOwner
                        args.UserName = GetParam(httpargs, "username")
                        args.Text = GetParam(httpargs, "msg")
                        args.TimeStamp = GetParam(httpargs, "ts")
                        args.isAnonymousAction = isAnonymousAction
                        DBAppendAction(args)

                    Case Command.Setting
                        Dim sSettingName As String = GetParam(httpargs, "name")
                        Dim sSettingValue As String = GetParam(httpargs, "value")
                        Dim args As New AntiguaSettingEventArgs
                        args.CmdCode = CType(tCmd, Command)
                        args.CmdOwner = CmdOwner
                        args.MeetingOwner = PM.Parameters.CS_MeetingOwner
                        args.Name = sSettingName
                        args.Value = sSettingValue
                        args.isAnonymousAction = isAnonymousAction

                        If Not isAnonymousAction AndAlso App.isAuthorized Then
                            If sSettingName = "color_coding" Then
                                PM.Parameters.CS_ColorCodingByUser = Str2Bool(sSettingValue)
                                PM.Parameters.Save()
                            End If

                            If sSettingName = "drawing_stay" Then
                                PM.Parameters.CS_DrawingLifeTime = CInt(sSettingValue)
                                PM.Parameters.CS_MeetingWhiteboardDrawingData = ""
                                PM.Parameters.Save()
                            End If

                            If sSettingName = "synch_mode" Then
                                PM.Parameters.CS_MeetingSynchMode = CInt(sSettingValue)
                                PM.Parameters.Save()
                            End If

                            If sSettingName = "default_alt_title" Then
                                PM.Parameters.CS_DefaultAlternativeTitle = sSettingValue
                                PM.Parameters.Save()
                            End If

                            If sSettingName = "default_obj_title" Then
                                PM.Parameters.CS_DefaultObjectiveTitle = sSettingValue
                                PM.Parameters.Save()
                            End If

                            If sSettingName = "default_source_title" Then
                                PM.Parameters.CS_DefaultSourceTitle = sSettingValue
                                PM.Parameters.Save()
                            End If

                            If sSettingName = "default_alt_color" Then
                                PM.Parameters.CS_DefaultAlternativeColor = sSettingValue
                                PM.Parameters.Save()
                            End If

                            If sSettingName = "default_obj_color" Then
                                PM.Parameters.CS_DefaultObjectiveColor = sSettingValue
                                PM.Parameters.Save()
                            End If

                            If sSettingName = "default_source_color" Then
                                PM.Parameters.CS_DefaultSourceColor = sSettingValue
                                PM.Parameters.Save()
                            End If

                            If sSettingName = "item_size" Then
                                Dim tRes As Double
                                Dim fChanged As Boolean = False
                                args.Value = ""
                                args.Tag = ""
                                If String2Double(GetParam(httpargs, "width"), tRes) AndAlso tRes <> PM.Parameters.CS_ItemWidth Then
                                    PM.Parameters.CS_ItemWidth = CInt(tRes)
                                    args.Value = tRes.ToString
                                    fChanged = True
                                End If
                                If String2Double(GetParam(httpargs, "height"), tRes) AndAlso tRes <> PM.Parameters.CS_ItemHeight Then
                                    PM.Parameters.CS_ItemHeight = CInt(tRes)
                                    args.Tag = tRes.ToString
                                    fChanged = True
                                End If
                                If fChanged Then
                                    PM.Parameters.Save()
                                End If
                                args.Value = PM.Parameters.CS_ItemWidth.ToString
                                args.Tag = PM.Parameters.CS_ItemHeight.ToString
                            End If

                            'If sSettingName = "save_user_list" Then 
                            '    PM.Parameters.CS_UserList = sSettingValue
                            '    PM.Parameters.Save()
                            'End If

                            If sSettingName = "make_pm" Then
                                Dim value As Boolean = Str2Bool(sSettingValue)
                                Dim Email As String = GetParam(httpargs, "email")
                                ' D6651 ===
                                Dim tAppUser As clsApplicationUser = App.DBUserByEmail(Email)
                                Dim tCanChangePM As Boolean = True
                                Dim success As Boolean = False
                                If tAppUser IsNot Nothing AndAlso App.ActiveWorkgroup IsNot Nothing Then    ' D6983
                                    Dim tUW = App.DBUserWorkgroupByUserIDWorkgroupID(tAppUser.UserID, App.ActiveWorkgroup.ID)
                                    Dim tWS = App.DBWorkspaceByUserIDProjectID(tAppUser.UserID, PRJ.ID)
                                    Dim grpID As Integer = App.ActiveWorkgroup.GetDefaultRoleGroupID(ecRoleLevel.rlModelLevel, If(value, ecRoleGroupType.gtProjectManager, ecRoleGroupType.gtEvaluator))

                                    If tUW Is Nothing AndAlso value Then tUW = App.AttachWorkgroupByProject(tAppUser.UserID, PRJ, ecRoleGroupType.gtProjectOrganizer, True)
                                    If tUW IsNot Nothing AndAlso value AndAlso Not App.CanUserBePM(App.ActiveWorkgroup, tAppUser.UserID, PRJ, False, True, tUW, tWS) Then
                                        Dim POGrpID As Integer = App.ActiveWorkgroup.GetDefaultRoleGroupID(ecRoleLevel.rlApplicationLevel, ecRoleGroupType.gtProjectOrganizer)
                                        If POGrpID > 0 AndAlso tUW.RoleGroupID <> POGrpID Then
                                            tUW.RoleGroupID = POGrpID
                                            App.DBUserWorkgroupUpdate(tUW, False, "Set user as " + ResString(String.Format("lbl_{0}", ecRoleGroupType.gtProjectOrganizer)))
                                        End If
                                    End If

                                    If Not value OrElse App.CanUserBePM(App.ActiveWorkgroup, tAppUser.UserID, PRJ, False, False, tUW, tWS) Then
                                        If tWS Is Nothing AndAlso value Then tWS = App.AttachProject(tAppUser, PRJ, False, grpID)

                                        If tWS IsNot Nothing AndAlso grpID > 0 AndAlso tWS.GroupID <> grpID Then
                                            tWS.GroupID = grpID
                                            App.DBWorkspaceUpdate(tWS, False, "Set user as " + ResString(String.Format("lbl_{0}", If(value, ecRoleGroupType.gtProjectManager, ecRoleGroupType.gtEvaluator))))
                                        End If
                                    End If
                                    ' D6651 ==

                                    success = tUW IsNot Nothing AndAlso tWS IsNot Nothing AndAlso tWS.GroupID = grpID
                                    tCanChangePM = tWS Is Nothing OrElse Not App.CanUserDoAction(ecActionType.at_alManageAnyModel, tUW, App.ActiveWorkgroup)
                                End If
                                args.Name = ""
                                tResult = New jActionResult With {.Result = If(success, ecActionResult.arSuccess, ecActionResult.arError), .Data = Email, .Tag = tCanChangePM}.ToJSON
                            End If
                        End If

                        If sSettingName = "whiteboard_draw_arr" AndAlso PM.Parameters.CS_DrawingLifeTime = Integer.MaxValue Then
                            Dim curData = PM.Parameters.CS_MeetingWhiteboardDrawingData
                            PM.Parameters.CS_MeetingWhiteboardDrawingData = curData + If(curData = "", "", ",") + sSettingValue.Substring(1, sSettingValue.Length - 2)
                            PM.Parameters.Save()
                        End If

                        If sSettingName = "whiteboard_drawing_clear_my" Then
                            PM.Parameters.CS_MeetingWhiteboardDrawingData = sSettingValue.Substring(1, sSettingValue.Length - 2)
                            PM.Parameters.Save()
                        End If

                        If sSettingName = "whiteboard_drawing_clear_all" Then
                            PM.Parameters.CS_MeetingWhiteboardDrawingData = ""
                            PM.Parameters.Save()
                        End If

                        If args.Name <> "" Then
                            If ServiceConnection.Operation(args) Then
                                DBAppendAction(args)
                            End If
                        End If

                    Case Command.SwitchProsCons
                        Dim tMode As String = CStr(GetParam(httpargs, "mode")).ToLower
                        Dim args As New AntiguaSwitchProsConsEventArgs
                        Dim tShow As Boolean = Str2Bool(GetParam(httpargs, "is_open"))
                        args.Show = tShow
                        If tMode <> "all" Then
                            Dim sNodeGuid As String = GetParam(httpargs, "guid")
                            Guid.TryParse(sNodeGuid, args.NodeGuid)
                        End If
                        args.CmdCode = Command.SwitchProsCons
                        args.isAnonymousAction = isAnonymousAction
                        args.CmdOwner = CmdOwner
                        args.MeetingOwner = PM.Parameters.CS_MeetingOwner
                        args.Mode = tMode
                        If ServiceConnection.Operation(args) Then
                            DBAppendAction(args)
                        End If

                    Case Command.CreateNewItemToProsCons
                        Dim sNodeGuid As String = GetParam(httpargs, "guid")
                        Dim tNodeGuid As Guid
                        If Guid.TryParse(sNodeGuid, tNodeGuid) Then
                            Dim args As New AntiguaProsConsEventArgs
                            args.isAnonymousAction = isAnonymousAction
                            args.NewItemTitle = GetParam(httpargs, "name")
                            args.IsPro = Str2Bool(GetParam(httpargs, "is_pro"))
                            args.NodeGuid = tNodeGuid
                            args.CmdCode = Command.CreateNewItemToProsCons
                            args.CmdOwner = CmdOwner
                            args.MeetingOwner = PM.Parameters.CS_MeetingOwner
                            args.Tag = GetParam(httpargs, "lastmodifiedby")
                            If ServiceConnection.Operation(args) Then
                                DBAppendAction(args)
                            End If
                        End If

                    Case Command.DeleteProOrCon
                        Dim sNodeGuid As String = GetParam(httpargs, "node_guid")
                        Dim tNodeGuid As Guid
                        If Guid.TryParse(sNodeGuid, tNodeGuid) Then
                            Dim args As New AntiguaProsConsEventArgs
                            args.CmdCode = Command.DeleteProOrCon
                            args.isAnonymousAction = isAnonymousAction
                            args.IsPro = Str2Bool(GetParam(httpargs, "is_pro"))
                            args.Source = New List(Of Guid)
                            args.DoForAll = Str2Bool(GetParam(httpargs, "all"))
                            If Not args.DoForAll Then
                                Dim sItemGuid As String = GetParam(httpargs, "item_guid")
                                Dim tItemGuid As Guid
                                If Guid.TryParse(sItemGuid, tItemGuid) Then
                                    args.Source.Add(tItemGuid)
                                End If
                            End If
                            args.NodeGuid = tNodeGuid
                            args.CmdOwner = CmdOwner
                            args.MeetingOwner = PM.Parameters.CS_MeetingOwner

                            If (args.Source.Count > 0 OrElse args.DoForAll) AndAlso ServiceConnection.Operation(args) Then
                                DBAppendAction(args)
                            End If
                        End If

                    Case Command.RenameProsConsItem
                        Dim sNodeGuid As String = GetParam(httpargs, "node_guid")
                        Dim tNodeGuid As Guid
                        If Guid.TryParse(sNodeGuid, tNodeGuid) Then
                            Dim args As New AntiguaProsConsEventArgs
                            args.CmdCode = Command.RenameProsConsItem
                            args.isAnonymousAction = isAnonymousAction
                            args.IsPro = Str2Bool(GetParam(httpargs, "is_pro"))
                            args.Source = New List(Of Guid)
                            Dim sItemGuid As String = GetParam(httpargs, "item_guid")
                            Dim tItemGuid As Guid
                            If Guid.TryParse(sItemGuid, tItemGuid) Then
                                args.Source.Add(tItemGuid)
                            End If
                            args.NewItemTitle = GetParam(httpargs, "name")
                            args.NodeGuid = tNodeGuid
                            args.CmdOwner = CmdOwner
                            args.MeetingOwner = PM.Parameters.CS_MeetingOwner

                            If args.Source.Count > 0 AndAlso ServiceConnection.Operation(args) Then
                                DBAppendAction(args)
                            End If
                        End If

                    Case Command.CopyFromProsConsToTree
                        Dim sTargetGuid As String = GetParam(httpargs, "target_guid")
                        Dim sNodeGuid As String = GetParam(httpargs, "source_guid")
                        Dim tNodeGuid As Guid
                        Dim sAltGuid As String = GetParam(httpargs, "alt_guid")
                        Dim tAltGuid As Guid
                        If Guid.TryParse(sNodeGuid, tNodeGuid) AndAlso Guid.TryParse(sAltGuid, tAltGuid) Then
                            Dim tDropPosition As Integer = Param2Int(httpargs, "pos")   ' D7598
                            Dim args As New AntiguaProsConsEventArgs
                            args.CmdCode = CType(tCmd, Command)
                            args.isAnonymousAction = isAnonymousAction
                            args.CmdOwner = CmdOwner
                            args.MeetingOwner = PM.Parameters.CS_MeetingOwner
                            args.Source = New List(Of Guid)
                            args.Source.Add(tNodeGuid)
                            args.NodeGuid = tAltGuid
                            args.NewItemTitle = GetParam(httpargs, "name")
                            args.Target = New List(Of Guid)
                            Dim tTargetGuid As Guid
                            Guid.TryParse(sTargetGuid, tTargetGuid)
                            args.Target.Add(If(String.IsNullOrEmpty(sTargetGuid), PM.ActiveObjectives.Nodes(0).NodeGuidID, tTargetGuid))
                            args.Position = New Drawing.Point(tDropPosition, 0)
                            args.Action = If(tDropPosition < 0, NodeMoveAction.nmaAsChildOfNode, CType(tDropPosition, NodeMoveAction))

                            If ServiceConnection.Operation(args) Then
                                DBAppendAction(args)
                            End If
                        End If

                    Case Command.SetMeetingLock
                        Dim args As New AntiguaStateOperationEventArgs
                        args.CmdCode = Command.SetMeetingLock
                        args.CmdOwner = CmdOwner
                        args.MeetingOwner = PM.Parameters.CS_MeetingOwner
                        args.IsMeetingLocked = Str2Bool(GetParam(httpargs, "is_locked"))
                        PM.Parameters.CS_MeetingLockedByPM = args.IsMeetingLocked
                        PM.Parameters.Save()
                        If ServiceConnection.Operation(args) Then
                            DBAppendAction(args)
                        End If

                    Case Command.CopyAllAltsToBoard
                        Dim args As New AntiguaCopyToBoardEventArgs
                        args.CmdCode = Command.CopyAllAltsToBoard
                        args.isAnonymousAction = isAnonymousAction
                        ' D7598 ===
                        Dim tMode As Integer = Param2Int(httpargs, "mode")
                        If tMode >= 0 Then args.CopyMode = CType(tMode, AntiguaCopyToBoardEventArgs.CopyModes)
                        ' D7598 ==
                        'args.CopyMode = CType(GetParam(httpargs, "mode"), AntiguaCopyToBoardEventArgs.CopyModes)
                        args.CmdOwner = CmdOwner
                        args.MeetingOwner = PM.Parameters.CS_MeetingOwner
                        args.Position = New Drawing.Point With {.X = 20, .Y = 120}
                        Dim W As Integer = Param2Int(httpargs, "w") ' D7598
                        Dim H As Integer = Param2Int(httpargs, "h") ' D7598
                        args.Size = New Drawing.Point With {.X = CInt(W), .Y = CInt(H)}
                        If ServiceConnection.Operation(args) Then
                            DBAppendAction(args)
                        End If

                    Case Command.EditGoal
                        Dim args As New AntiguaPropertiesOperationEventArgs
                        args.CmdCode = Command.EditGoal
                        args.isAnonymousAction = isAnonymousAction
                        args.Title = GetParam(httpargs, "title")
                        args.CmdOwner = CmdOwner
                        args.MeetingOwner = PM.Parameters.CS_MeetingOwner
                        If ServiceConnection.Operation(args) Then
                            DBAppendAction(args)
                        End If

                End Select

                If String.IsNullOrEmpty(tResult) Then   ' D6651
                    Dim fRes As jActionResult = New jActionResult With {.Data = "", .Result = ecActionResult.arSuccess}
                    tResult = fRes.ToJSON
                End If

            Case "get_alt_contributions"
                Dim sAltGuid As String = GetParam(httpargs, "guid")
                Dim tAltGuid As Guid
                If Guid.TryParse(sAltGuid, tAltGuid) Then
                    Dim tAlt As clsNode = PM.AltsHierarchy(PM.ActiveAltsHierarchy).GetNodeByID(tAltGuid)

                    Dim tObjectivesData As New List(Of csObj)
                    For Each obj As Tuple(Of Integer, Integer, clsNode) In PM.ActiveObjectives.NodesInLinearOrder
                        Dim node As clsNode = obj.Item3
                        tObjectivesData.Add(New csObj With {.id = node.NodeID, .guid = node.NodeGuidID.ToString, .text = node.NodeName, .pid = If(node.ParentNode Is Nothing, -1, node.ParentNodeID), .selected = node.GetContributedAlternatives().Contains(tAlt) })
                    Next
                    Dim fRes As jActionResult = New jActionResult With {.Data = tObjectivesData, .Tag = sAltGuid, .Result = ecActionResult.arSuccess, .Message = sAction}
                    tResult = fRes.ToJSON
                End If

            Case "set_alt_contributions"
                Dim sAltGuid As String = GetParam(httpargs, "guid")
                Dim tAltGuid As Guid = New Guid(sAltGuid)
                Dim sObjs As String = GetParam(httpargs, "ids")
                Dim tObjs As String() = sObjs.Split(CChar(","))
                Dim iObjs As List(Of Guid) = New List(Of Guid)
                For Each s As String In tObjs
                    ' D6999 ===
                    Dim tmpS As Guid
                    If Not String.IsNullOrWhiteSpace(s) AndAlso Guid.TryParse(s, tmpS) Then
                        iObjs.Add(tmpS)
                        ' D6999 ==
                    End If
                Next
                PM.UpdateContributions(tAltGuid, iObjs, Ctype(PM.ActiveHierarchy, ECHierarchyID), True)
                Dim fRes As jActionResult = New jActionResult With {.Result = ecActionResult.arSuccess, .Message = sAction}
                tResult = fRes.ToJSON

            Case "get_obj_contributions"
                Dim sObjGuid As String = GetParam(httpargs, "guid")
                Dim tObjGuid As Guid = New Guid(sObjGuid)
                Dim tNode As clsNode = PM.Hierarchy(PM.ActiveHierarchy).GetNodeByID(tObjGuid)

                Dim tAlternativesData As New List(Of csObj)
                Dim tExistingContributedAlts As List(Of clsNode) = tNode.GetContributedAlternatives()
                For Each alt As clsNode In PM.AltsHierarchy(PM.ActiveAltsHierarchy).TerminalNodes
                    tAlternativesData.Add(New csObj With {.id = alt.NodeID, .guid = alt.NodeGuidID.ToString, .text = alt.NodeName, .pid = -1, .selected = tExistingContributedAlts.Contains(alt) })
                Next
                Dim fRes As jActionResult = New jActionResult With {.Data = tAlternativesData, .Tag = sObjGuid, .Result = ecActionResult.arSuccess, .Message = sAction}
                tResult = fRes.ToJSON

            Case "set_obj_contributions"
                Dim sObjGuid As String = GetParam(httpargs, "guid")
                Dim tObjGuid As Guid = New Guid(sObjGuid)
                Dim sAlts As String = GetParam(httpargs, "ids")
                Dim tAlts As String() = sAlts.Split(CChar(","))
                Dim iAlts As List(Of Guid) = New List(Of Guid)
                For Each s As String In tAlts
                    If Not String.IsNullOrWhiteSpace(s) Then 
                        iAlts.Add(New Guid(s))
                    End If
                Next
                Dim tNode As clsNode = PM.ActiveObjectives.GetNodeByID(tObjGuid)
                tNode.ChildrenAlts.Clear()
                Dim CovObjIDs As New List(Of Guid)
                CovObjIDs.Add(tObjGuid)
                If PM.UpdateContributions(CovObjIDs, iAlts, True, CType(PM.ActiveHierarchy, ECHierarchyID)) Then
                    PRJ.MakeSnapshot("CS: Set contributions", "For: " + tNode.NodeName)
                End If
                Dim fRes As jActionResult = New jActionResult With {.Result = ecActionResult.arSuccess, .Message = sAction}
                tResult = fRes.ToJSON
            Case "refresh_full"
                Dim fRes As jActionResult = New jActionResult With {.Data = "", .Result = ecActionResult.arSuccess}
                tResult = fRes.ToJSON

            Case "get_help_url"
                tResult = GetCurrentCSHelpUrl()

        End Select

        ' D4953 + D5001 ===
        If App IsNot Nothing AndAlso App.isAuthorized AndAlso App.ActiveUser IsNot Nothing AndAlso (PM.Parameters.CS_MeetingOwner = App.ActiveUser.UserID OrElse tCmd = Command.SetMeetingState OrElse tCmd = Command.DisconnectUser) AndAlso PRJ.LockInfo IsNot Nothing Then
            App.DBProjectLockInfoWrite(If(PM.Parameters.CS_MeetingState = MeetingState.Active OrElse PM.Parameters.CS_MeetingState = MeetingState.Paused, ECLockStatus.lsLockForAntigua, ECLockStatus.lsUnLocked), PRJ.LockInfo, App.ActiveUser, Now.AddSeconds(StructuringServicePage.LongPollMaxTime * 10)) ' locking for 30 minutes
        End If
        ' D4953 + D5001 ==

        Response.Clear()
        Response.ContentType = "text/plain"
        Response.Write(tResult)
        Response.End()
    End Sub

    Private Sub DBAppendAction(args As AntiguaOperationEventArgs)
        If PRJ IsNot Nothing AndAlso PRJ.ID >= 0 Then
            args.DT = DateTime.Now.Ticks
            Dim sData = String.Format("{0}{1}{2}{1}{3}", args.DT, vbTab, If(args.isAnonymousAction, 1, 0), args.GetJSON())
            App.DBTeamTimeDataWrite(PRJ.ID, COMBINED_USER_ID, ecExtraProperty.StructuringJsonData, sData, False)
        End If
    End Sub

    Private Sub StructuringPage_PreRenderComplete(sender As Object, e As EventArgs) Handles Me.PreRenderComplete
        Session(_SESS_PRJ_ANON_NAME) = _PRJ ' D4996
    End Sub

End Class