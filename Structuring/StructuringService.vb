Imports System.Configuration.ConfigurationManager
Imports Canvas
Imports ECCore
Imports System.Data.Common
Imports ExpertChoice.Data
Imports ExpertChoice.Service

Namespace ExpertChoice.Structuring

    <Serializable> Public Class StructuringClient

        Private Const UseProjectLock As Boolean = True

        ' D0851 ===
        Private _LockTimeout As Integer = -1
        Public ReadOnly Property LockTimeout() As Integer
            Get
                If _LockTimeout < 0 Then
                    If AppSettings.AllKeys.Contains("LockTimeout") AndAlso Not String.IsNullOrEmpty(AppSettings.Item("LockTimeout")) Then
                        _LockTimeout = CInt(AppSettings.Item("LockTimeout")) * 60   ' D4953
                    End If
                    If _LockTimeout < 0 Then _LockTimeout = ExpertChoice.Data.Consts._DEF_LOCK_TIMEOUT
                End If
                Return _LockTimeout
            End Get
        End Property
        ' D0851 ==

        Public Property CurrentMeeting As clsMeeting = New clsMeeting() With {.Info = New MeetingInfo}
        Public Property CurrentSession As clsSession = New clsSession(New UserToken)

        Public Function Version() As String
            Return "2.0"
        End Function

        Private ChildVerticalOffset As Integer = 50
        Private ChildHorizontalOffset As Integer = 30

        Private SyncObj As New Object

        Sub New(tPM As clsProjectManager, tPrjId As Integer)
            CurrentMeeting.Info.ProjectManager = tPM
            CurrentMeeting.Info.ProjectID = tPrjId

            'Dim mMeetingLockInterval As Integer = 10000
            'mMeetingLockTimer = New System.Threading.Timer(New Threading.TimerCallback(AddressOf MeetingLockTimer), Nothing, mMeetingLockInterval, mMeetingLockInterval)
        End Sub

        '<NonSerialized> Private mMeetingLockTimer As System.Threading.Timer

        'Private Sub MeetingLockTimer(ByVal state As Object)
        '    If CurrentMeeting IsNot Nothing AndAlso CurrentMeeting.Info.ProjectManager IsNot Nothing Then
        '        Dim tMeetingState As Integer = CurrentMeeting.Info.ProjectManager.Parameters.CS_MeetingState
        '        If UseProjectLock Then ProjectLockInfoSet(mConnectionString, GenericDBAccess.ECGenericDatabaseAccess.DBProviderType.dbptSQLClient, CurrentMeeting.Info.ProjectID, CurrentMeeting.Info.ProjectManager.Parameters.CS_MeetingOwner, If(tMeetingState = MeetingState.Active OrElse tMeetingState = MeetingState.Paused, Data.ECLockStatus.lsLockForAntigua, Data.ECLockStatus.lsUnLocked), Now.AddSeconds(LockTimeout)) ' D0851 + D4953
        '    End If
        'End Sub

        <NonSerialized> Private rnd As New System.Random

        Private mConnectionString As String
        <NonSerialized> Private mDBHelper As ExpertChoice.Database.clsDatabaseHelper

#Region "Business Logic"

        Private ReadOnly Property DB() As ExpertChoice.Database.clsDatabaseHelper
            Get
                If mDBHelper Is Nothing Then
                    Dim CanvasMasterDBName = ExpertChoice.Service.WebConfigOption(ExpertChoice.Web.WebOptions._OPT_CANVASMASTERDB, "CoreDB", True)
                    mConnectionString = ExpertChoice.Data.clsDatabaseAdvanced.GetConnectionString(CanvasMasterDBName, GenericDBAccess.ECGenericDatabaseAccess.DBProviderType.dbptSQLClient)
                    Dim cd As ExpertChoice.Database.clsConnectionDefinition
                    cd = ExpertChoice.Database.getConnectionDefinition(CanvasMasterDBName, GenericDBAccess.ECGenericDatabaseAccess.DBProviderType.dbptSQLClient)
                    mDBHelper = New ExpertChoice.Database.clsDatabaseHelper(cd)
                    mDBHelper.Connect()
                End If
                Return mDBHelper
            End Get
        End Property

        Private Sub LoadAntiguaPanels(Meeting As MeetingInfo) 'A0609
            Meeting.ProjectManager.AntiguaDashboard.LoadPanel(ECModelStorageType.mstCanvasStreamDatabase, mConnectionString, GenericDBAccess.ECGenericDatabaseAccess.DBProviderType.dbptSQLClient, Meeting.ProjectID)
            Meeting.ProjectManager.AntiguaRecycleBin.LoadPanel(ECModelStorageType.mstCanvasStreamDatabase, mConnectionString, GenericDBAccess.ECGenericDatabaseAccess.DBProviderType.dbptSQLClient, Meeting.ProjectID)
            Meeting.ProjectManager.AntiguaInfoDocs.LoadAntiguaInfoDocs()
        End Sub

        Private Sub SetInfoDoc(ByVal VisualNodeGuid As Guid, ByVal InfoDoc As String) 'C0830
            Dim M = CurrentMeeting()
            If M IsNot Nothing Then
                Dim PM = M.Info.ProjectManager
                SyncLock SyncObj
                    PM.AntiguaInfoDocs.SetAntiguaInfoDoc(VisualNodeGuid, InfoDoc)
                End SyncLock
            End If
        End Sub

        Private Sub SaveDashboard()
            Dim M = CurrentMeeting()
            If M IsNot Nothing Then
                Dim PM = M.Info.ProjectManager
                SyncLock SyncObj
                    PM.AntiguaDashboard.SavePanel(ECModelStorageType.mstCanvasStreamDatabase, mConnectionString, GenericDBAccess.ECGenericDatabaseAccess.DBProviderType.dbptSQLClient, M.Info.ProjectID)
                End SyncLock
            End If
        End Sub

        Private Sub SaveRecycleBin()
            Dim M = CurrentMeeting()
            If M IsNot Nothing Then
                Dim PM = M.Info.ProjectManager
                SyncLock SyncObj
                    PM.AntiguaRecycleBin.SavePanel(ECModelStorageType.mstCanvasStreamDatabase, mConnectionString, GenericDBAccess.ECGenericDatabaseAccess.DBProviderType.dbptSQLClient, M.Info.ProjectID)
                End SyncLock
            End If
        End Sub

        Private Sub SaveStructure() 'C0638
            Dim M = CurrentMeeting()
            If M IsNot Nothing Then
                Dim PM = M.Info.ProjectManager
                SyncLock SyncObj
                    PM.StorageManager.Writer.SaveProject(True)
                End SyncLock
            End If
        End Sub

        'Private Sub SaveTreeView()
        '    SaveStructure() 'C0638
        'End Sub

        'Private Sub SaveAlternatives()
        '    SaveStructure() 'C0638
        'End Sub

        Private Sub DoSaveAll()
            SaveDashboard()
            SaveRecycleBin()
            SaveStructure() 'A1292
            'SaveTreeView()
            'SaveAlternatives() 'A0122
        End Sub

        Public Sub SaveAll() 'C0610            
            'save all to DB
            DoSaveAll()

            'create a restore point
            Dim M = CurrentMeeting()
            If M IsNot Nothing Then
                M.DoAutoSave(True)
            End If
        End Sub

        Private Function GetNewTokenID() As Integer
            Dim SQL As String
            Dim retVal As Integer
            SyncLock SyncObj
                Do
                    retVal = rnd.Next(100000000, 999999999)
                    SQL = "SELECT COUNT(*) FROM StructureTokens WHERE TokenID = " & retVal.ToString
                Loop While CInt(DB.ExecuteScalar(SQL)) > 0
            End SyncLock
            Return retVal
        End Function

        Private Function GetUserToken(ByVal TokenID As Integer) As UserToken
            Dim dr As DbDataReader = Nothing
            Dim u As New UserToken
            Dim Password As String = ""

            SyncLock SyncObj
                dr = DB.ExecuteReader("select * from StructureTokens where TokenID = " & TokenID)
                If (dr IsNot Nothing) AndAlso dr.Read Then
                    u.TokenID = CInt(dr("TokenID"))
                    u.MeetingID = CInt(dr("MeetingID"))
                    u.Email = dr("EMail").ToString.Trim
                    u.UserName = dr("Username").ToString.Trim
                    u.ClientType = CType(dr("ClientType"), ClientType)
                End If
                If dr IsNot Nothing AndAlso Not dr.IsClosed Then
                    dr.Close()
                End If
            End SyncLock
            Return u
        End Function

        Private Function GetDBPassword(ByVal MeetingID As Integer) As String
            Dim Password As String = ""
            Dim dr As DbDataReader = Nothing

            'DB.Connect()
            SyncLock SyncObj
                dr = DB.ExecuteReader("select Password from StructureMeetings where MeetingID = " & MeetingID)
                If dr.Read Then
                    Password = dr(0).ToString.Trim
                End If
                If dr IsNot Nothing AndAlso Not dr.IsClosed Then
                    dr.Close()
                End If
            End SyncLock

            Return Password
        End Function

#End Region

#Region "Owner Implementations"

        Public Function GetUserTokens(ByVal m As clsMeeting) As System.Collections.Generic.List(Of UserToken)
            If (m Is Nothing) Then
                Return Nothing
            End If

            Dim Users As New List(Of UserToken)

            SyncLock SyncObj
                For Each s As KeyValuePair(Of String, clsSession) In m.Sessions
                    If s.Value.UserToken.MeetingID = CurrentMeeting.Info.MeetingID Then
                        Users.Add(s.Value.UserToken)
                    End If
                Next
            End SyncLock

            Return Users
        End Function

        Public Sub SetMeetingState(ByRef args As AntiguaStateOperationEventArgs)
            Dim sess = CurrentSession()
            Dim m = CurrentMeeting()

            'If (m Is Nothing) OrElse (sess Is Nothing) OrElse (sess.UserToken.ClientType <> ClientType.Owner) Then
            If (m Is Nothing) OrElse (sess Is Nothing) Then
                Exit Sub
            End If

            SyncLock SyncObj
                m.Info.State = args.State
                m.Info.ProjectManager.Parameters.CS_MeetingState = args.State
                m.Info.ProjectManager.Parameters.Save()
                'If args.State = MeetingState.Stopped OrElse args.State = MeetingState.OwnerDisconnected Then
                '    If mMeetingLockTimer IsNot Nothing Then mMeetingLockTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite)  ' D5065
                'End If
                'releasing project lock
                If (args.State = MeetingState.InActive) Or (args.State = MeetingState.Stopped) Or (args.State = MeetingState.OwnerDisconnected) Then
                    If UseProjectLock Then ProjectLockInfoSet(mConnectionString, GenericDBAccess.ECGenericDatabaseAccess.DBProviderType.dbptSQLClient, m.Info.ProjectID, m.Info.ProjectManager.Parameters.CS_MeetingOwner, Data.ECLockStatus.lsUnLocked, Now.AddSeconds(LockTimeout))    ' D0851 + D4953
                    'If (args.State = MeetingState.Stopped) Then
                    'SaveDashboard()
                    'SaveRecycleBin()
                    'Else
                    'SaveAll()
                    'End If
                End If
            End SyncLock
        End Sub

        Public Sub BootUser(ByRef args As AntiguaDisconnectUserEventArgs)
            Dim sess = CurrentSession()
            Dim m = CurrentMeeting()

            If (sess Is Nothing) OrElse (m Is Nothing) OrElse (sess.UserToken.ClientType <> ClientType.Owner) Then
                Exit Sub
            End If

            'A0088 ===
            Dim keyToRemove As String = ""
            Dim currentMeetingID = sess.UserToken.MeetingID
            Dim TokenToRemove As UserToken = Nothing

            SyncLock SyncObj
                ' search for userToken that will be deleted
                For Each s In m.Sessions
                    If s.Value.UserToken.TokenID = args.TokenID Then
                        TokenToRemove = s.Value.UserToken
                        keyToRemove = s.Key
                        Exit For
                    End If
                Next
                ' notify all clients
                If TokenToRemove IsNot Nothing Then
                    Dim e1 As New AntiguaStateOperationEventArgs With {.CmdCode = Command.SetMeetingState, .State = MeetingState.Booted}
                    Dim e2 As New AntiguaClientArrivalEventArgs With {.CmdCode = Command.ClientArrival, .Token = TokenToRemove, .Entry = MeetingEntry.Exited}

                    For Each s As KeyValuePair(Of String, clsSession) In m.Sessions
                        If s.Value.UserToken.MeetingID = currentMeetingID Then
                            If s.Value.UserToken.TokenID = args.TokenID Then
                                s.Value.OperationResult(e1)
                            Else
                                If TokenToRemove IsNot Nothing Then _
                                s.Value.OperationResult(e2)
                            End If
                        End If
                    Next
                    If Not String.IsNullOrEmpty(keyToRemove) Then
                        m.Sessions.Remove(keyToRemove)
                    End If
                End If
            End SyncLock
        End Sub


        Public Sub SetLock(ByRef args As AntiguaStateOperationEventArgs)
            Dim sess = CurrentSession()
            Dim m = CurrentMeeting()

            If (sess Is Nothing) OrElse (m Is Nothing) OrElse (sess.UserToken.ClientType <> ClientType.Owner) Then
                Exit Sub
            End If

            SyncLock SyncObj
                m.Info.IsMeetingLocked = args.IsMeetingLocked
                'm.Info.BoardLocked = args.BoardLocked
                'm.Info.RecycleLocked = args.RecycleLocked
            End SyncLock
        End Sub

#End Region

#Region "JSON"

        Public Function IntColorToString(ByVal intColor As Integer, isAlternative As Boolean) As String
            Dim m = CurrentMeeting()
            If (intColor > 0 And intColor < Int32.MaxValue) Or (intColor < 0 And intColor > Int32.MinValue) Then
                Dim hex As String = intColor.ToString("X8")
                Return "#" + hex.Substring(2, 2) + hex.Substring(4, 2) + hex.Substring(6, 2)
            Else
                Return If(isAlternative, m.Info.ProjectManager.Parameters.CS_DefaultAlternativeColor, m.Info.ProjectManager.Parameters.CS_DefaultObjectiveColor)
            End If
        End Function

        Public Function GetWhiteboardNodeJSON(v As clsVisualNode, DefaultWidth As Integer, DefaultHeight As Integer) As String
            Dim w = If(v.Attributes.Width = 0, DefaultWidth, v.Attributes.Width)
            Dim h = If(v.Attributes.Height = 0, DefaultHeight, v.Attributes.Height)
            If v.IsAlternative AndAlso v.Location <> GUILocation.Alternatives Then v.Location = GUILocation.Alternatives
            Dim sAttrs As String = String.Format("{{""x"":{0},""y"":{1},""h"":{2},""w"":{3},""color"":""{4}""}}", JS_SafeNumber(v.Attributes.X), JS_SafeNumber(v.Attributes.Y), JS_SafeString(h), JS_SafeString(w), IntColorToString(v.Attributes.BackGroundColor, v.IsAlternative))
            Dim sPros As String = ""
            If v.ProsList IsNot Nothing Then
                For Each tPro In v.ProsList
                    sPros += If(sPros = "", "", ",") + GetWhiteboardNodeJSON(tPro, DefaultWidth, DefaultHeight)
                Next
            End If
            Dim sCons As String = ""
            If v.ConsList IsNot Nothing Then
                For Each tCon In v.ConsList
                    sCons += If(sCons = "", "", ",") + GetWhiteboardNodeJSON(tCon, DefaultWidth, DefaultHeight)
                Next
            End If
            Return String.Format("{{""nodeguid"":""{0}"",""name"":""{1}"",""hasinfodoc"":{2},""isalt"":{3},""attributes"":{4},""author"":""{5}"",""lastmodifiedby"":""{6}"",""is_pc_open"":{7},""pc_moved"":{8},""pros"":[{9}],""cons"":[{10}],""location"":{11}}}", v.GuidID.ToString, JS_SafeString(v.Text), Bool2Num(v.HasInfoDoc), Bool2JS(v.IsAlternative), sAttrs, JS_SafeString(SafeFormString(v.Author)), JS_SafeString(SafeFormString(v.LastModifiedBy)), Bool2Num(v.IsProsConsPaneVisible), Bool2JS(v.WasAlreadyMovedToHierarchy), sPros, sCons, CInt(v.Location))
        End Function

        Public Function GetWhiteboardNodesJSON(Optional ReloadPanels As Boolean = False) As String
            Dim m = CurrentMeeting()

            If ReloadPanels Then LoadAntiguaPanels(m.info)
            
            Dim retVal As String = ""
            For Each v As clsVisualNode In m.Info.ProjectManager.AntiguaDashboard.Nodes
                If v.IsAlternative AndAlso v.Location <> GUILocation.Alternatives Then v.Location = GUILocation.Alternatives
                If v.Location = GUILocation.Board OrElse v.Location = GUILocation.BoardImpact OrElse v.Location = GUILocation.Alternatives Then 
                    retVal += If(retVal <> "", ",", "") + GetWhiteboardNodeJSON(v, m.Info.ProjectManager.Parameters.CS_ItemWidth, m.Info.ProjectManager.Parameters.CS_ItemHeight)
                End If            
            Next

            Return retVal
        End Function

        Public Function GetNodeJSON(node As clsNode) As String
            Dim retVal As String = ""
            Dim parentGuids As String = ""
            If node.ParentNode IsNot Nothing Then parentGuids = String.Format("""{0}""", node.ParentNode.NodeGuidID.ToString)
            If node.ParentNodesGuids IsNot Nothing Then
                For Each id As Guid In node.ParentNodesGuids
                    Dim sId As String = id.ToString
                    If parentGuids.IndexOf(sId) < 0 Then
                        parentGuids += If(parentGuids = "", "", ",") + String.Format("""{0}""", sId)
                    End If
                Next
            End If
            parentGuids = String.Format("[{0}]", parentGuids)
            retVal = String.Format("{{""nodeid"":{0},""nodeguid"":""{1}"",""name"":""{2}"",""parentguids"":{3},""level"":{4},""hasinfodoc"":{5},""isterminal"":{6},""iscategory"":{7},""enabled"":{8},""expanded"":{9},""parentId"":{10},""isalt"":{11},""hid"":{12}}}", node.NodeID, node.NodeGuidID.ToString, JS_SafeString(node.NodeName), parentGuids, node.Level, Bool2Num(node.InfoDoc.Length > 0), Bool2Num(node.IsTerminalNode), Bool2Num(node.RiskNodeType = RiskNodeType.ntCategory), Bool2Num(node.Enabled), 1, If(node.ParentNode Is Nothing, "null", node.ParentNodeID.ToString), Bool2JS(node.IsAlternative), node.Hierarchy.HierarchyID)
            If node.Children IsNot Nothing AndAlso node.Children.Count > 0 Then 
                For Each child As clsNode In node.Children
                    retVal += If(retVal <> "", ",", "") + GetNodeJSON(child)
                Next
            End If
            Return retVal
        End Function

        Public Function GetObjectivesJSON(hid As ECHierarchyID) As String
            Dim m = CurrentMeeting()
            Dim retVal As String = ""
            If m.Info.ProjectManager.Hierarchy(hid) IsNot Nothing Then
                m.Info.ProjectManager.CreateHierarchyLevelValuesCH(m.Info.ProjectManager.Hierarchy(hid))
                Dim goal As clsNode = m.Info.ProjectManager.Hierarchy(hid).Nodes(0)
                retVal = GetNodeJSON(goal)
            End If
            Return retVal
        End Function

        Public Function GetAlternativesJSON() As String
            Dim m = CurrentMeeting()
            Dim retVal As String = ""        
            For Each alt As clsNode In m.Info.ProjectManager.ActiveAlternatives.TerminalNodes
                retVal += If(retVal <> "", ",", "") + GetNodeJSON(alt)
            Next
            Return retVal
        End Function

#End Region

#Region "Client Implementations"

        'C0638===
        Public Sub PutNewNodeToDashboard(ByRef args As AntiguaNewNodeOperationEventArgs)
            Dim m = CurrentMeeting()

            If (m Is Nothing) Then Exit Sub

            Dim Success As Boolean = True

            Dim tNode As clsVisualNode
            SyncLock SyncObj
                tNode = m.Info.ProjectManager.AntiguaDashboard.AddNode(args.Node)
                args.Tag = GetWhiteboardNodeJSON(tNode, m.Info.ProjectManager.Parameters.CS_ItemWidth, m.Info.ProjectManager.Parameters.CS_ItemHeight)
            End SyncLock

            If Success Then
                If Not m.Info.SaveOnDemand Then SaveDashboard()
            Else
                CurrentSession.OperationResult(New AntiguaOperationEventArgs With {.CmdCode = Command.NoSuccess, .CmdOwner = args.CmdOwner})
            End If
        End Sub
        'C0638==

        Public Sub Connect(ByRef args As AntiguaConnectOperationEventArgs) 'A0095 + D0826
            If args Is Nothing Then Exit Sub

            Dim mUser As UserToken = GetUserToken(args.Token.TokenID)

            If mUser IsNot Nothing Then
                'A0095 ===
                If mUser.ClientType = ClientType.Owner Then
                    Dim res As Boolean = False
                    Dim credsData As New Data.clsCredentials
                    If Data.clsCredentials.TryParseHash(args.Credentials, args.InstanceID, credsData) Then
                        If (credsData.UserEmail = mUser.Email) Then
                            res = True
                        End If
                    End If

                    If Not res Then
                        args.ConnectError = ConnectErrorCode.WrongCredentials
                        Exit Sub
                    End If
                End If
                'A0095 ==

                mUser.UserGuid = args.Token.UserGuid

                'If meeting does not exist, then create new meeting object
                If (mUser.MeetingID > 0) Then
                    Dim m As MeetingInfo = CurrentMeeting.Info
                    If m IsNot Nothing Then
                        m.TreeMode = args.TreeMode
                        m.BoardMode = args.BoardMode
                        Dim Meeting = New clsMeeting() With {.Info = m}

                        Meeting.Info.TreeMode = args.TreeMode
                        Meeting.Info.BoardMode = args.BoardMode
                        Meeting.Info.CSMode = args.CSMode

                        'Meeting.Sessions.Add(newSession.SessionID, newSession)
                        'ReturnConnectInfo(newSession, Meeting, args)
                    End If
                Else
                    args.ConnectError = ConnectErrorCode.WrongMeetingID
                    'newSession.OperationResult(args)
                End If
            End If
        End Sub

        Public Function CreateUserToken(ByVal MeetingID As Integer, ByVal MeetingPassword As String, ByVal Email As String, ByVal UserName As String, ByVal UserGuid As Guid) As UserToken
            Dim newUser As UserToken = Nothing
            Dim m As MeetingInfo = Nothing
            Dim DBPassword As String = GetDBPassword(MeetingID)

            If MeetingPassword = DBPassword Then
                newUser = New UserToken
                newUser.TokenID = GetNewTokenID()
                newUser.MeetingID = MeetingID
                newUser.Email = Email
                newUser.UserName = Uri.UnescapeDataString(UserName) 'A0236
                newUser.ClientType = ClientType.Regular
                newUser.UserGuid = CType(IIf(Not UserGuid.Equals(Guid.Empty), UserGuid, Guid.NewGuid), Guid)

                If newUser.TokenID = -1 Then
                    newUser = Nothing
                Else
                    Dim SQL As String
                    SyncLock SyncObj
                        Try
                            DB.BeginTransaction()
                            SQL = String.Format("INSERT INTO StructureTokens VALUES ({0}, {1}, '{2}', '{3}', {4})", newUser.TokenID, newUser.MeetingID, newUser.Email, newUser.UserName, CInt(newUser.ClientType))
                            DB.ExecuteNonQuery(SQL, CommandType.Text, Database.ConnectionState.KeepOpen)

                            DB.CommitTransaction()
                        Catch ex As Exception
                            DB.RollbackTransaction()
                            newUser = Nothing
                        End Try
                    End SyncLock
                End If
            End If

            Return newUser
        End Function

        Public Sub SetMeetingMode(ByRef args As AntiguaStateOperationEventArgs)
            Dim sess = CurrentSession()
            Dim m = CurrentMeeting()

            If (sess IsNot Nothing) AndAlso (sess.UserToken.ClientType = ClientType.Owner) Then
                SyncLock SyncObj
                    If args.TreeMode = MeetingMode.Impacts Then m.Info.ProjectManager.ActiveHierarchy = EchierarchyID.hidImpact else m.Info.ProjectManager.ActiveHierarchy = EchierarchyID.hidLikelihood
                    m.Info.TreeMode = Ctype(args.TreeMode, MeetingMode)
                    m.Info.BoardMode = Ctype(args.BoardMode, MeetingMode)
                    m.Info.CSMode = args.CSMode
                End SyncLock
            End If
        End Sub

        Public Sub SetActiveHierarchyID(ByRef args As AntiguaStateOperationEventArgs, Optional skipOwner As Boolean = False)
            Dim sess = CurrentSession()
            Dim m = CurrentMeeting()

            If (sess IsNot Nothing) AndAlso (sess.UserToken.ClientType = ClientType.Owner) Then
                SyncLock SyncObj
                    m.Info.ActiveHierarchyID = args.ActiveHierarchyID
                    If m.Info.ProjectManager IsNot Nothing Then
                        m.Info.ProjectManager.ActiveHierarchy = m.Info.ActiveHierarchyID
                        'm.Info.ProjectManager.StorageManager.Writer.SaveProject(False)
                        Select Case m.Info.ActiveHierarchyID
                            Case ECHierarchyID.hidImpact
                                m.Info.ProjectManager.PipeParameters.CurrentParameterSet = m.Info.ProjectManager.PipeParameters.GetParameterSetByID(PipeParameters.PARAMETER_SET_IMPACT)
                                LoadAntiguaPanels(m.Info) 'A0609
                            Case Else
                                m.Info.ProjectManager.PipeParameters.CurrentParameterSet = m.Info.ProjectManager.PipeParameters.GetParameterSetByID(PipeParameters.PARAMETER_SET_DEFAULT)
                                LoadAntiguaPanels(m.Info) 'A0609
                        End Select
                    End If
                End SyncLock
            End If
        End Sub

        Private Function clsNodeToVisualNode(ByVal node As clsNode) As clsVisualNode
            Dim vNode As clsVisualNode = Nothing
            If node IsNot Nothing Then
                vNode = New clsVisualNode
                vNode.GuidID = node.NodeGuidID
                vNode.Text = node.NodeName
                'C0640===
                If node.ParentNode IsNot Nothing Then
                    vNode.ParentGuidID = node.ParentNode.NodeGuidID
                Else
                    vNode.ParentGuidID = Guid.Empty
                End If
                'C0640==
                vNode.Attributes = New clsVisualNodeAttributes
                vNode.Attributes.X = 0
                vNode.Attributes.Y = 0
                vNode.Attributes.Height = 60
                vNode.Attributes.Width = 140
                vNode.Attributes.BackGroundColor = 0 '-52 '-1120086

                vNode.InfoDoc = node.InfoDoc 'A0346
                vNode.HasInfoDoc = node.InfoDoc <> "" 'A0346
                vNode.IsAlternative = node.IsAlternative 'A0414                
            End If
            Return vNode
        End Function

        Private Function clsVisualNodeToNode(ByVal vNode As clsVisualNode) As clsNode
            Dim Node As clsNode = Nothing
            If vNode IsNot Nothing Then
                Node = New clsNode
                Node.NodeGuidID = vNode.GuidID
                Node.NodeName = vNode.Text
                'TODO: add here more fields from clsVisualNode
            End If
            Return Node
        End Function

        Private Sub AddObjectiveToList(ByVal node As clsNode, ByRef ListOfNodes As List(Of clsVisualNode), AltH As clsHierarchy) 'C0643
            ListOfNodes.Add(clsNodeToVisualNode(node))
            If AltH IsNot Nothing AndAlso node.IsTerminalNode Then
                For Each alt In node.ChildrenAlts
                    ListOfNodes.Last.ChildrenAlts.Add(AltH.GetNodeByID(alt).NodeGuidID)
                Next
            End If
            For Each child As clsNode In node.Children
                AddObjectiveToList(child, ListOfNodes, AltH)
            Next
        End Sub

        Public Function GetNodes(ByVal Location As Canvas.GUILocation) As System.Collections.Generic.List(Of Canvas.clsVisualNode)
            Dim m = CurrentMeeting()
            If (m Is Nothing) Then
                Return Nothing
            End If

            Dim Nodes As New List(Of clsVisualNode)

            SyncLock SyncObj
                Select Case Location
                    Case GUILocation.Board
                        For Each node In m.Info.ProjectManager.AntiguaDashboard.Nodes
                            If node.Location <> GUILocation.Board AndAlso node.Location <> GUILocation.BoardImpact Then node.Location = GUILocation.Board
                        Next
                        Nodes = m.Info.ProjectManager.AntiguaDashboard.Nodes
                    Case GUILocation.Treeview
                        If m.Info.ProjectManager.Hierarchy(m.Info.ProjectManager.ActiveHierarchy).Nodes.Count = 0 Then
                            Dim node As clsNode = m.Info.ProjectManager.Hierarchy(m.Info.ProjectManager.ActiveHierarchy).AddNode(-1)
                            'Dim node As clsNode = AddCSNode(m.Info.ProjectManager, m.Info.ProjectManager.Hierarchy(m.Info.ProjectManager.ActiveHierarchy), -1) 'A0958
                            node.NodeName = "Goal"
                        End If
                        Dim AltH As clsHierarchy = Nothing

                        If m.Info.ProjectManager.AltsHierarchies.Count > 0 Then
                            AltH = m.Info.ProjectManager.AltsHierarchy(m.Info.ProjectManager.ActiveAltsHierarchy)
                        End If

                        'C0643===
                        AddObjectiveToList(m.Info.ProjectManager.Hierarchy(m.Info.ProjectManager.ActiveHierarchy).Nodes(0), Nodes, AltH)
                        'C0643==
                    Case GUILocation.RecycleBin
                        Nodes = m.Info.ProjectManager.AntiguaRecycleBin.Nodes
                    Case GUILocation.Alternatives 'C0640
                        If m.Info.ProjectManager.AltsHierarchies.Count > 0 Then 'A0160 RTE here
                            Dim AH = m.Info.ProjectManager.AltsHierarchy(m.Info.ProjectManager.ActiveAltsHierarchy)
                            If AH IsNot Nothing Then
                                For Each alt In AH.TerminalNodes
                                    Dim vNode = clsNodeToVisualNode(alt)
                                    vNode.IsAlternative = True
                                    Nodes.Add(vNode)
                                Next
                            End If
                        End If
                End Select
            End SyncLock

            Return Nodes
        End Function

        Public Function IsPasswordRequired(ByVal MeetingID As Integer) As Boolean
            Return GetDBPassword(MeetingID).Length > 0
        End Function

        Public Sub MoveNodeInHierarchy(ByRef args As AntiguaReorderOperationEventArgs)
            Dim m = CurrentMeeting()
            Dim sess = CurrentSession()

            If (m Is Nothing) OrElse sess Is Nothing Then
                Exit Sub
            End If

            Dim Success As Boolean = False

            SyncLock SyncObj
                Dim H As clsHierarchy = m.Info.ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood)
                If H IsNot Nothing Then
                    Dim srcNode As clsNode = H.GetNodeByID(args.SourceNodeGuid)
                    Dim destNode As clsNode = H.GetNodeByID(args.DestNodeGuid)

                    If srcNode Is Nothing OrElse destNode Is Nothing Then
                        H = m.Info.ProjectManager.Hierarchy(ECHierarchyID.hidImpact)
                        If H IsNot Nothing Then
                            srcNode = H.GetNodeByID(args.SourceNodeGuid)
                            destNode = H.GetNodeByID(args.DestNodeGuid)
                        End If
                    End If
                    
                    If (srcNode IsNot Nothing) AndAlso (destNode IsNot Nothing) AndAlso H IsNot Nothing Then
                        H.MoveNode(srcNode, destNode, args.Action)
                        Success = True
                    End If

                    m.Info.ProjectManager.CreateHierarchyLevelValuesCH(H)
                    args.Tag = String.Format("[{0}]", GetObjectivesJSON(CType(H.HierarchyID, ECHierarchyID)))
                End If
            End SyncLock

            If Success Then
                If Not m.Info.SaveOnDemand Then SaveStructure() 'A1292
            Else
                sess.OperationResult(New AntiguaOperationEventArgs With {.CmdCode = Command.NoSuccess, .CmdOwner = args.CmdOwner})
            End If
        End Sub

        Private Sub CopyInObjectives(ByRef args As AntiguaCopyOperationEventArgs)
            Dim m = CurrentMeeting()
            Dim sess = CurrentSession()

            If (m Is Nothing) OrElse sess Is Nothing Then
                Exit Sub
            End If

            Dim Success As Boolean = False

            SyncLock SyncObj
                Dim H As clsHierarchy = m.Info.ProjectManager.Hierarchy(m.Info.ProjectManager.ActiveHierarchy)
                If H IsNot Nothing AndAlso H.HierarchyType = ECHierarchyType.htModel Then

                    Dim srcNode As clsNode = H.GetNodeByID(args.SourceNodeGuid)
                    Dim destNode As clsNode = H.GetNodeByID(args.DestNodeGuid)

                    If (srcNode IsNot Nothing) AndAlso (destNode IsNot Nothing) Then
                        Dim res As List(Of ECCore.clsNode) = H.CopyNode(srcNode, destNode, args.Action, False)
                        args.ResList = New List(Of clsVisualNode)
                        If res IsNot Nothing AndAlso res.Count > 0 Then
                            For Each node In res
                                args.ResList.Add(clsNodeToVisualNode(node))
                            Next
                        End If
                        Success = True
                    End If
                End If
            End SyncLock

            If Success Then
                If Not m.Info.SaveOnDemand Then SaveStructure() 'A1292
            Else
                sess.OperationResult(New AntiguaOperationEventArgs With {.CmdCode = Command.NoSuccess, .CmdOwner = args.CmdOwner})
            End If
        End Sub

        Private Sub PutChildrenOnBoard(ByVal hNode As clsNode, ByRef X As Integer, ByRef Y As Integer, W As Integer, ByRef H As Integer, ByVal m As clsMeeting, ByVal IsRootBoardNode As Boolean, Location As GUILocation)
            'restore colors (not saved to database)
            Dim vNode As clsVisualNode = Nothing
            If hNode.Tag IsNot Nothing AndAlso TypeOf hNode.Tag Is clsVisualNode Then
                vNode = CType(hNode.Tag, clsVisualNode)
            End If

            If vNode Is Nothing Then
                Dim node = clsNodeToVisualNode(hNode)
                vNode = node
            End If

            vNode.Attributes.X = X
            vNode.Attributes.Y = Y
            vNode.Attributes.Width = W
            vNode.Attributes.Height = H
            vNode.Location = Location

            If IsRootBoardNode Then vNode.ParentGuidID = Guid.Empty
            If m.Info.ActiveHierarchyID = ECHierarchyID.hidImpact Then vNode.Location = GUILocation.BoardImpact

            m.Info.ProjectManager.AntiguaDashboard.Nodes.Add(vNode)
            CopyInfodocFromNodeToVisualNode(hNode, hNode.NodeGuidID, m.Info.ProjectManager)

            If (hNode.Children IsNot Nothing) AndAlso (hNode.Children.Count > 0) Then
                Y += ChildVerticalOffset
                For Each child In hNode.Children
                    PutChildrenOnBoard(child, X + ChildHorizontalOffset, Y, W, H, m, False, Location)
                    Y += ChildVerticalOffset
                Next
            End If
        End Sub

        Private Sub MoveNodeFromTreeToBoard(ByRef args As AntiguaMoveToBoardEventArgs)
            Dim m = CurrentMeeting()
            Dim sess = CurrentSession()

            If m Is Nothing OrElse sess Is Nothing Then
                Exit Sub
            End If

            Dim Success As Boolean = False

            SyncLock SyncObj
                Dim H As clsHierarchy = m.Info.ProjectManager.Hierarchy(m.Info.ProjectManager.ActiveHierarchy)
                If H IsNot Nothing Then
                    Dim hNode As clsNode
                    hNode = H.GetNodeByID(args.NodeGuid)
                    If hNode IsNot Nothing Then
                        PutChildrenOnBoard(hNode, args.Position.X, args.Position.Y, args.Size.X, args.Size.Y, m, True, args.Location)

                        H.DeleteNode(hNode)
                        Success = True

                        args.Tag = String.Format("[[{0}],[{1}]]", GetWhiteboardNodesJSON(), GetObjectivesJSON(CType(H.HierarchyID, ECHierarchyID)))                                    
                    End If
                End If
            End SyncLock

            If Success Then
                If Not m.Info.SaveOnDemand Then
                    SaveStructure() 'A1292
                    SaveDashboard()
                End If
            Else
                sess.OperationResult(New AntiguaOperationEventArgs With {.CmdCode = Command.NoSuccess, .CmdOwner = args.CmdOwner})
            End If
        End Sub

        Private Sub MoveNodesFromDashboardToHierarchy(ByRef args As AntiguaMoveToHierarchyOperationEventArgs)
            Dim m = CurrentMeeting()
            Dim sess = CurrentSession()

            If m Is Nothing OrElse sess Is Nothing Then
                Exit Sub
            End If

            Dim Success As Boolean = False

            SyncLock SyncObj
                Dim H As clsHierarchy = m.Info.ProjectManager.Hierarchy(m.Info.ProjectManager.ActiveHierarchy)
                If H IsNot Nothing Then

                    Dim destNode As clsNode = H.GetNodeByID(args.DestNodeGuid)
                    If destNode IsNot Nothing Then
                        For Each nodeGuid As Guid In args.NodesGuids
                            Dim vNode As clsVisualNode = m.Info.ProjectManager.AntiguaDashboard.GetNodeByGuid(nodeGuid)
                            If vNode IsNot Nothing Then
                                ' put new node under Goal
                                MoveSingleNodeFromBoardToTree(H, destNode, vNode, args.Action, m.Info.ProjectManager)
                            End If
                        Next
                        Success = True

                        m.Info.ProjectManager.CreateHierarchyLevelValuesCH(m.Info.ProjectManager.ActiveObjectives)
                        args.Tag = String.Format("[{0}]", GetObjectivesJSON(CType(H.HierarchyID, ECHierarchyID)))
                    End If
                End If
            End SyncLock

            If Success Then

                If Not m.Info.SaveOnDemand Then
                    SaveStructure() 'A1292
                    SaveDashboard()
                End If
            Else
                sess.OperationResult(New AntiguaOperationEventArgs With {.CmdCode = Command.NoSuccess, .CmdOwner = args.CmdOwner})
            End If
        End Sub

        Private Sub MoveSingleNodeFromBoardToTree(H As clsHierarchy, destNode As clsNode, vNode As clsVisualNode, tAction As NodeMoveAction, pm As clsProjectManager)
            Dim srcNode As clsNode = H.AddNode(H.Nodes(0).NodeID, , vNode.GuidID) 'C0645
            'Dim srcNode As clsNode = AddCSNode(pm, H, H.Nodes(0).NodeID) 'C0645 + A0958
            H.MoveNode(srcNode, destNode, tAction)

            'srcNode.NodeGuidID = vNode.GuidID
            If Not String.IsNullOrEmpty(vNode.Text) Then
                srcNode.NodeName = vNode.Text
            End If
            srcNode.Tag = vNode
            CopyInfodocFromVisualNodeToNode(vNode, srcNode, pm)
            pm.AntiguaDashboard.RemoveNodeByGuid(vNode.GuidID)
            Dim i As Integer = 0
            While i < pm.AntiguaDashboard.Nodes.Count
                Dim node As clsVisualNode = pm.AntiguaDashboard.Nodes(i)
                If node IsNot Nothing AndAlso node.ParentGuidID.Equals(vNode.GuidID) Then
                    MoveSingleNodeFromBoardToTree(H, srcNode, node, NodeMoveAction.nmaAsChildOfNode, pm)
                    i = 0
                Else
                    i += 1
                End If
            End While
        End Sub

        'A0958 ===
        Private Function AddCSNode(PM As clsProjectManager, H As ECCore.clsHierarchy, NodeID As Integer) As ECCore.clsNode
            Dim tmpAltsDefaultContribution As ECTypes.ECAltsDefaultContribution = PM.PipeParameters.AltsDefaultContribution
            Dim tmpAltsDefaultContributionImpact As ECTypes.ECAltsDefaultContribution = PM.PipeParameters.AltsDefaultContributionImpact
            Dim tmpAltsDefaultContributionHierarchy As ECTypes.ECAltsDefaultContribution = H.AltsDefaultContribution
            If H.HierarchyType = ECHierarchyType.htModel Then
                H.AltsDefaultContribution = ECAltsDefaultContribution.adcNone
                PM.PipeParameters.AltsDefaultContribution = ECAltsDefaultContribution.adcNone
                PM.PipeParameters.AltsDefaultContributionImpact = ECAltsDefaultContribution.adcNone
            End If
            Dim retVal As clsNode = H.AddNode(NodeID)
            If PM.PipeParameters.AltsDefaultContribution <> tmpAltsDefaultContribution OrElse PM.PipeParameters.AltsDefaultContributionImpact <> tmpAltsDefaultContributionImpact Then
                PM.SavePipeParameters(PipeStorageType.pstStreamsDatabase, PM.StorageManager.ModelID)
            End If
            Return retVal
        End Function
        'A0958 ==

        Private Sub AddNode(ByRef args As AntiguaAddNodeOperationEventArgs)
            Dim sess = CurrentSession()
            Dim m = CurrentMeeting()
            If (m IsNot Nothing) AndAlso (sess IsNot Nothing) Then

                Dim Success As Boolean = False

                If args.IsAlternative Then
                    'add to alternatives
                    Dim AltH As clsHierarchy = Nothing

                    If m.Info.ProjectManager.AltsHierarchies.Count > 0 Then
                        AltH = m.Info.ProjectManager.AltsHierarchy(m.Info.ProjectManager.ActiveAltsHierarchy)
                    End If

                    If AltH IsNot Nothing Then
                        SyncLock SyncObj
                            Dim node As clsNode = AltH.AddNode(-1, , New Guid(args.NodeID.ToString))
                            'Dim node As clsNode = AddCSNode(m.Info.ProjectManager, AltH, -1) 'A0958
                            node.NodeName = args.NodeTitle
                            'node.NodeGuidID = New Guid(args.NodeID.ToString)
                            If args.ParentNodeID <> Guid.Empty Then
                                Dim destNode As clsNode = m.Info.ProjectManager.ActiveAlternatives.GetNodeByID(args.ParentNodeID)
                                If destNode IsNot Nothing Then 
                                    m.Info.ProjectManager.ActiveAlternatives.MoveNode(node, destNode, NodeMoveAction.nmaAfterNode)
                                End If
                            End If
                            args.Tag = GetNodeJSON(node)
                            Success = True
                        End SyncLock
                    End If
                Else
                    'add to objectives
                    Dim HId As Integer = m.Info.ProjectManager.ActiveHierarchy

                    Dim H As clsHierarchy = m.Info.ProjectManager.Hierarchy(HId)
                    If H IsNot Nothing Then

                        Dim destNode As clsNode = H.GetNodeByID(args.ParentNodeID)
                        If destNode IsNot Nothing Then
                            ' put new node under Goal
                            SyncLock SyncObj
                                Dim srcNode As clsNode = H.AddNode(H.Nodes(0).NodeID, , args.NodeID)
                                'Dim srcNode As clsNode = AddCSNode(m.Info.ProjectManager, H, H.Nodes(0).NodeID) 'A0958
                                H.MoveNode(srcNode, destNode, NodeMoveAction.nmaAsChildOfNode)
                                'srcNode.NodeGuidID = args.NodeID
                                srcNode.NodeName = args.NodeTitle
                                args.Tag = GetNodeJSON(srcNode)
                                Success = True
                            End SyncLock
                        End If
                    End If
                End If

                If Success Then
                    If Not m.Info.SaveOnDemand Then SaveStructure() 'A1292                        
                Else
                    sess.OperationResult(New AntiguaOperationEventArgs With {.CmdCode = Command.NoSuccess, .CmdOwner = args.CmdOwner})
                End If
            End If
        End Sub

        Private Sub AddRiskObjectives(args As AntiguaAddRiskObjectivesOperationEventArgs)
            Dim sess = CurrentSession()
            Dim m = CurrentMeeting()
            If (m IsNot Nothing) AndAlso (sess IsNot Nothing) Then

                Dim Success As Boolean = False

                Dim HId As Integer = m.Info.ProjectManager.ActiveHierarchy

                HId = args.HierarchyID
                Dim ParentNodeID = m.Info.ProjectManager.Hierarchy(HId).Nodes(0).NodeGuidID

                Dim tNames = args.NodesNames.Split(CChar(Environment.NewLine))
                Dim tAnyNameExists As Boolean = False
                If tNames IsNot Nothing AndAlso tNames.Count > 0 Then
                    For Each s In tNames
                        If Not String.IsNullOrEmpty(s) AndAlso s.Trim <> "" Then tAnyNameExists = True
                    Next
                End If

                Dim H As clsHierarchy = m.Info.ProjectManager.Hierarchy(HId)
                If H IsNot Nothing AndAlso tAnyNameExists Then
                    Dim destNode As clsNode = H.GetNodeByID(ParentNodeID)
                    If destNode IsNot Nothing Then
                        ' put new node under Goal
                        args.NewNodes = New Dictionary(Of Guid, String)
                        SyncLock SyncObj
                            For Each newNodeName As String In tNames
                                If Not String.IsNullOrEmpty(newNodeName) AndAlso Not newNodeName.Trim = "" Then
                                    newNodeName = newNodeName.Trim
                                    Dim srcNode As clsNode = H.AddNode(H.Nodes(0).NodeID)
                                    'Dim srcNode As clsNode = AddCSNode(m.Info.ProjectManager, H, H.Nodes(0).NodeID) 'A0958
                                    'H.MoveNode(srcNode, destNode, NodeMoveAction.nmaAsChildOfNode)

                                    'srcNode.NodeGuidID = Guid.NewGuid
                                    srcNode.NodeName = newNodeName
                                    args.NewNodes.Add(srcNode.NodeGuidID, srcNode.NodeName)
                                    Success = True
                                End If
                            Next
                        End SyncLock
                    End If
                End If

                If Success Then
                    If Not m.Info.SaveOnDemand Then SaveStructure()
                End If
            End If
        End Sub

        Private Sub RestoreFromRecycle(ByRef args As AntiguaMoveToBoardEventArgs)
            Dim m = CurrentMeeting()
            Dim sess = CurrentSession()

            If m Is Nothing OrElse sess Is Nothing Then
                Exit Sub
            End If

            Dim Success As Boolean = False

            SyncLock SyncObj
                Dim vNode As clsVisualNode = m.Info.ProjectManager.AntiguaRecycleBin.GetNodeByGuid(args.NodeGuid)
                If vNode IsNot Nothing Then
                    RestoreSingleNodeFromRecycle(vNode, m.Info.ProjectManager, args.Position, True)
                    Success = True
                End If
            End SyncLock

            If Success Then
                If Not m.Info.SaveOnDemand Then
                    SaveRecycleBin()
                    SaveDashboard()
                End If
            Else
                sess.OperationResult(New AntiguaOperationEventArgs With {.CmdCode = Command.NoSuccess, .CmdOwner = args.CmdOwner})
            End If
        End Sub

        Private Sub RestoreSingleNodeFromRecycle(vNode As clsVisualNode, pm As clsProjectManager, Position As System.Drawing.Point, IsRootNode As Boolean)
            vNode.Attributes.X = Position.X
            vNode.Attributes.Y = Position.Y
            If IsRootNode Then vNode.ParentGuidID = Guid.Empty
            pm.AntiguaDashboard.AddNode(vNode)
            pm.AntiguaRecycleBin.RemoveNodeByGuid(vNode.GuidID)

            Dim i As Integer = 0
            While i < pm.AntiguaRecycleBin.Nodes.Count
                Dim node As clsVisualNode = pm.AntiguaRecycleBin.Nodes(i)
                If node IsNot Nothing AndAlso node.ParentGuidID.Equals(vNode.GuidID) Then
                    RestoreSingleNodeFromRecycle(node, pm, Position, False)
                    i = 0
                Else
                    i += 1
                End If
            End While
        End Sub

        Private Sub RenameItem(ByRef args As AntiguaPropertiesOperationEventArgs)
            Dim m = CurrentMeeting()
            Dim sess = CurrentSession()

            If m Is Nothing Then
                Exit Sub
            End If

            Dim Success As Boolean = False

            Dim BoardNode As clsVisualNode
            Dim TreeNode As clsNode
            Dim AltNode As clsNode = Nothing

            SyncLock SyncObj
                BoardNode = m.Info.ProjectManager.AntiguaDashboard.GetNodeByGuid(args.NodeGuid)
                If BoardNode IsNot Nothing Then
                    BoardNode.Text = args.Title
                    'A0219 ===
                    Dim LastModifiedBy As String = ""
                    For Each client In m.Sessions
                        If client.Value.UserToken.TokenID = args.CmdOwner Then
                            LastModifiedBy = String.Format("{0} ({1})", client.Value.UserToken.UserName, client.Value.UserToken.Email)
                            Exit For
                        End If
                    Next
                    If LastModifiedBy <> "" Then
                        BoardNode.LastModifiedBy = LastModifiedBy
                    End If
                    'A0219 ==
                    'BoardNode.Attributes.Width = args.NewSize.X
                    'BoardNode.Attributes.Height = args.NewSize.Y
                    Success = True
                End If

                TreeNode = m.Info.ProjectManager.Hierarchy(m.Info.ProjectManager.ActiveHierarchy).GetNodeByID(args.NodeGuid)
                If TreeNode IsNot Nothing Then
                    TreeNode.NodeName = args.Title
                    Success = True
                End If
                If m.Info.ProjectManager.AltsHierarchies.Count > 0 Then 'A0160 RTE here
                    AltNode = m.Info.ProjectManager.AltsHierarchy(m.Info.ProjectManager.ActiveAltsHierarchy).GetNodeByID(args.NodeGuid)
                    If AltNode IsNot Nothing Then
                        AltNode.NodeName = args.Title
                        Success = True
                    End If
                End If
            End SyncLock

            If Success Then
                If Not m.Info.SaveOnDemand Then
                    If BoardNode IsNot Nothing Then SaveDashboard()
                    If TreeNode IsNot Nothing OrElse AltNode IsNot Nothing Then SaveStructure() 'A1292
                End If
            Else
                sess.OperationResult(New AntiguaOperationEventArgs With {.CmdCode = Command.NoSuccess, .CmdOwner = args.CmdOwner})
            End If
        End Sub

        Private Sub ResizeItem(ByRef args As AntiguaPropertiesOperationEventArgs)
            Dim m = CurrentMeeting(), sess = CurrentSession(), Success As Boolean = False, BoardNode As clsVisualNode, AltNode As clsNode = Nothing
            If m Is Nothing Then Exit Sub           

            SyncLock SyncObj
                BoardNode = m.Info.ProjectManager.AntiguaDashboard.GetNodeByGuid(args.NodeGuid)
                If BoardNode IsNot Nothing Then
                    BoardNode.Attributes.Width = args.Width
                    BoardNode.Attributes.Height = args.Height
                    Success = True
                End If
            End SyncLock

            If Success Then
                If Not m.Info.SaveOnDemand Then
                    If BoardNode IsNot Nothing Then SaveDashboard()
                End If
            Else
                sess.OperationResult(New AntiguaOperationEventArgs With {.CmdCode = Command.NoSuccess, .CmdOwner = args.CmdOwner})
            End If
        End Sub

        Private Sub ResizeItems(ByRef args As AntiguaSettingEventArgs)
            Dim m = CurrentMeeting(), sess = CurrentSession(), Success As Boolean = False, BoardNode As clsVisualNode, AltNode As clsNode = Nothing
            If m Is Nothing Then Exit Sub           

            SyncLock SyncObj
                Dim w As Integer = CInt(args.Value)
                Dim h As Integer = CInt(args.Tag)
                For Each BoardNode In m.Info.ProjectManager.AntiguaDashboard.Nodes
                    BoardNode.Attributes.Width = w
                    BoardNode.Attributes.Height = h
                    Success = True
                Next
            End SyncLock

            If Success Then
                If Not m.Info.SaveOnDemand Then
                    SaveDashboard()
                End If
            Else
                sess.OperationResult(New AntiguaOperationEventArgs With {.CmdCode = Command.NoSuccess, .CmdOwner = args.CmdOwner})
            End If
        End Sub

        Private Sub MoveNodeOnBoard(ByRef args As AntiguaMoveToBoardEventArgs)
            Dim m = CurrentMeeting()
            Dim sess = CurrentSession()

            If (m Is Nothing) OrElse (sess Is Nothing) Then
                Exit Sub
            End If

            Dim Success As Boolean = False
            Dim Node As clsVisualNode

            SyncLock SyncObj
                Node = m.Info.ProjectManager.AntiguaDashboard.GetNodeByGuid(args.NodeGuid)

                If Node IsNot Nothing Then
                    Node.Attributes.X = args.Position.X
                    Node.Attributes.Y = args.Position.Y
                    Success = True
                End If
            End SyncLock

            If Success Then
                If Not m.Info.SaveOnDemand Then SaveDashboard()
            Else
                sess.OperationResult(New AntiguaOperationEventArgs With {.CmdCode = Command.NoSuccess, .CmdOwner = args.CmdOwner})
            End If
        End Sub

        Private Sub DeleteNodes(ByRef args As AntiguaDeleteOperationEventArgs)
            Dim m = CurrentMeeting()
            Dim sess = CurrentSession()

            If (m Is Nothing) OrElse (sess Is Nothing) Then
                Exit Sub
            End If

            Dim Success As Boolean = False

            SyncLock SyncObj

                For Each gId In args.NodesGuids
                    Dim vNode As clsVisualNode
                    Dim Node As clsNode

                    If args.IsWhiteboardItems Then
                        vNode = m.Info.ProjectManager.AntiguaDashboard.GetNodeByGuid(gId)
                        If vNode IsNot Nothing Then
                            m.Info.ProjectManager.AntiguaDashboard.RemoveNodeByGuid(gId)
                            Success = True
                        End If
                        Dim H = m.Info.ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood)
                        Node = H.GetNodeByID(gId)
                        If Node Is Nothing Then 
                            H = m.Info.ProjectManager.Hierarchy(ECHierarchyID.hidImpact)
                            if H IsNot Nothing Then Node = H.GetNodeByID(gId)
                        End If
                    Else
                        Dim H As clsHierarchy = m.Info.ProjectManager.ActiveAlternatives
                        Node = H.GetNodeByID(gId)
                        If Node Is Nothing Then 
                            H = m.Info.ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood)
                            Node = H.GetNodeByID(gId)
                        End If
                        If Node Is Nothing Then 
                            H = m.Info.ProjectManager.Hierarchy(ECHierarchyID.hidImpact)
                            If H IsNot Nothing Then Node = H.GetNodeByID(gId)
                        End If
                        If Node IsNot Nothing Then
                            CopyInfodocFromNodeToVisualNode(Node, Node.NodeGuidID, m.Info.ProjectManager)

                            H.DeleteNode(Node)
                            Success = True
                        End If
                    End If                    
                Next
            End SyncLock

            If Success Then
                If Not m.Info.SaveOnDemand Then
                    DoSaveAll()
                End If
            Else
                sess.OperationResult(New AntiguaOperationEventArgs With {.CmdCode = Command.NoSuccess, .CmdOwner = args.CmdOwner})
            End If
        End Sub

        Private Sub AddToRecycleBin(ByVal Node As clsNode, pm As clsProjectManager, IsRootNode As Boolean)
            If Node IsNot Nothing Then
                If pm.AntiguaRecycleBin.GetNodeByGuid(Node.NodeGuidID) Is Nothing Then
                    Dim vNode As clsVisualNode = Nothing
                    If Node.Tag IsNot Nothing AndAlso TypeOf Node.Tag Is clsVisualNode Then vNode = CType(Node.Tag, clsVisualNode) Else vNode = clsNodeToVisualNode(Node)
                    If IsRootNode Then vNode.ParentGuidID = Guid.Empty
                    pm.AntiguaRecycleBin.Nodes.Add(vNode)
                End If
                CopyInfodocFromNodeToVisualNode(Node, Node.NodeGuidID, pm)
                If Node.Children.Count > 0 Then
                    For Each child In Node.Children
                        AddToRecycleBin(child, pm, False)
                    Next
                End If
            End If
        End Sub

        Private Sub AddToRecycleBinFromDashboard(NodeGuidID As Guid, ByVal vNode As clsVisualNode, pm As clsProjectManager, IsRootNode As Boolean)
            If vNode IsNot Nothing Then
                If Not pm.AntiguaRecycleBin.Nodes.Contains(vNode) Then
                    If IsRootNode Then vNode.ParentGuidID = Guid.Empty
                    pm.AntiguaRecycleBin.AddNode(vNode)
                End If
                pm.AntiguaDashboard.RemoveNodeByGuid(NodeGuidID)
                Dim i As Integer = 0
                While i < pm.AntiguaDashboard.Nodes.Count
                    Dim tNode = pm.AntiguaDashboard.Nodes(i)
                    If tNode IsNot Nothing AndAlso tNode.ParentGuidID.Equals(NodeGuidID) Then
                        AddToRecycleBinFromDashboard(tNode.GuidID, tNode, pm, False)
                        i = 0
                    Else
                        i += 1
                    End If
                End While
            End If
        End Sub

        Private Sub GrantPermissions(ByRef args As AntiguaGrantPermissionsEventArgs)
            Dim sess = CurrentSession()
            Dim m = CurrentMeeting()

            If (sess Is Nothing) OrElse (m Is Nothing) OrElse (sess.UserToken.ClientType <> ClientType.Owner) Then
                Exit Sub
            End If

            'A0088 ===
            Dim sessionToGrantPermission As clsSession = Nothing
            Dim currentMeetingID = sess.UserToken.MeetingID
            Dim TokenToGrantPermission As UserToken = Nothing
            Dim Success As Boolean = False

            SyncLock SyncObj
                ' search for userToken that will be deleted
                For Each s In m.Sessions
                    If s.Value.UserToken.TokenID = args.TokenID Then
                        TokenToGrantPermission = s.Value.UserToken
                        sessionToGrantPermission = s.Value

                        m.Info.OwnerEmail = TokenToGrantPermission.Email
                        m.Info.OwnerName = TokenToGrantPermission.UserName

                        TokenToGrantPermission.ClientType = ClientType.Owner
                        sess.UserToken.ClientType = ClientType.Regular

                        Success = True
                        Exit For
                    End If
                Next

                If Success AndAlso TokenToGrantPermission IsNot Nothing Then
                    sessionToGrantPermission.OperationResult(args)
                Else
                    sess.OperationResult(New AntiguaOperationEventArgs With {.CmdCode = Command.NoSuccess, .CmdOwner = args.CmdOwner})
                End If
            End SyncLock
        End Sub

        Private Sub InfoDocChanged(ByRef args As AntiguaInfoDocChangedEventArgs)
            Dim m = CurrentMeeting()
            Dim sess = CurrentSession()

            Dim Success As Boolean = False

            If m Is Nothing OrElse sess Is Nothing Then
                Exit Sub
            End If

            Dim Node As clsNode
            Dim vNode As clsVisualNode

            vNode = m.Info.ProjectManager.AntiguaDashboard.GetNodeByGuid(args.NodeGuid)
            If vNode IsNot Nothing Then
                vNode.HasInfoDoc = args.HasInfodoc
                Success = True
            Else
                Dim H As clsHierarchy = m.Info.ProjectManager.Hierarchy(m.Info.ProjectManager.ActiveHierarchy)
                Node = H.GetNodeByID(args.NodeGuid)
                If Node IsNot Nothing Then
                    'Node.HasInfodoc = args.HasInfodoc ?
                    Success = True
                Else
                    If m.Info.ProjectManager.AltsHierarchies.Count > 0 Then 'A0160
                        Dim altsH As clsHierarchy = m.Info.ProjectManager.AltsHierarchy(m.Info.ProjectManager.ActiveAltsHierarchy)

                        Node = altsH.GetNodeByID(args.NodeGuid)
                        If Node IsNot Nothing Then
                            'Node.HasInfodoc = args.HasInfodoc ?
                            Success = True
                        End If
                    End If

                End If
            End If

            If Success Then
                ' D4426 ===
                If m IsNot Nothing Then
                    Dim PM = m.Info.ProjectManager
                    SyncLock SyncObj
                        PM.StorageManager.Reader.LoadInfoDocs()
                        PM.AntiguaInfoDocs.LoadAntiguaInfoDocs()
                    End SyncLock
                End If
                ' D4426 ==
            Else
                sess.OperationResult(New AntiguaOperationEventArgs With {.CmdCode = Command.NoSuccess, .CmdOwner = args.CmdOwner})
            End If
        End Sub

#End Region

#Region "Collaborative Identification of Alternatives"

        Public Sub AddAlternatives(ByRef args As AntiguaMoveToHierarchyOperationEventArgs)
            Dim sess = CurrentSession()
            Dim m = CurrentMeeting()
            If (m IsNot Nothing) AndAlso (sess IsNot Nothing) Then

                Dim Success As Boolean = False

                Dim AltH As clsHierarchy = Nothing

                If m.Info.ProjectManager.AltsHierarchies.Count > 0 Then 'A0160
                    AltH = m.Info.ProjectManager.AltsHierarchy(m.Info.ProjectManager.ActiveAltsHierarchy)
                End If

                If AltH Is Nothing Then Exit Sub 'A0160 RTE here

                SyncLock SyncObj
                    Dim i As Integer = 0
                    For Each altGuid In args.NodesGuids
                        'add to alternatives
                        Dim alt = m.Info.ProjectManager.AntiguaDashboard.GetNodeByGuid(altGuid)
                        If alt IsNot Nothing AndAlso AltH.GetNodeByID(altGuid) Is Nothing Then
                            Dim node As clsNode = AltH.AddNode(-1, , altGuid)
                            'Dim node As clsNode = AddCSNode(m.Info.ProjectManager, AltH, -1) 'A0958
                            node.NodeName = alt.Text
                            'node.NodeGuidID = altGuid

                            If args.Position > -1 Then
                                ' moving node from last position to first
                                AltH.Nodes.Remove(node)
                                AltH.Nodes.Insert(args.Position + i, node)
                                i += 1
                            End If

                            'remove from dashboard
                            'm.Info.ProjectManager.AntiguaDashboard.RemoveNodeByGuid(alt.GuidID) 'C0638

                            CopyInfodocFromVisualNodeToNode(alt, node, m.Info.ProjectManager)

                            alt.WasAlreadyMovedToHierarchy = True 'A0400
                            Success = True

                            args.Tag = GetNodeJSON(node) 'todo do for multiple alternatives
                        End If
                    Next
                End SyncLock

                If Success Then
                    If Not m.Info.SaveOnDemand Then
                        SaveDashboard()
                        SaveStructure() 'A1292
                    End If
                Else
                    sess.OperationResult(New AntiguaOperationEventArgs With {.CmdCode = Command.NoSuccess, .CmdOwner = args.CmdOwner})
                End If
            End If
        End Sub

        Public Sub CopyAlternativeToBoard(ByRef args As AntiguaMoveToBoardEventArgs)
            Dim sess = CurrentSession()
            Dim m = CurrentMeeting()
            If (m IsNot Nothing) AndAlso (sess IsNot Nothing) Then

                Dim Success As Boolean = False

                SyncLock SyncObj
                    'remove from alternatives
                    Dim AltH As clsHierarchy = Nothing
                    If m.Info.ProjectManager.AltsHierarchies.Count > 0 Then 'A0160
                        AltH = m.Info.ProjectManager.AltsHierarchy(m.Info.ProjectManager.ActiveAltsHierarchy)

                        Dim node As clsNode = AltH.GetNodeByID(args.NodeGuid)
                        If node IsNot Nothing AndAlso m.Info.ProjectManager.AntiguaDashboard.GetNodeByGuid(args.NodeGuid) Is Nothing AndAlso m.Info.ProjectManager.AntiguaDashboardImpact.GetNodeByGuid(args.NodeGuid) Is Nothing Then
                            node.IsAlternative = True 'A0414
                            'put on Board
                            Dim alt As clsVisualNode = clsNodeToVisualNode(node)

                            alt.Attributes.X = args.Position.X
                            alt.Attributes.Y = args.Position.Y
                            alt.Attributes.Width = args.Size.X
                            alt.Attributes.Height = args.Size.Y

                            alt.IsAlternative = True
                            alt.GuidID = args.NodeGuid

                            m.Info.ProjectManager.AntiguaDashboard.AddNode(alt) 'C0638
                            CopyInfodocFromNodeToVisualNode(node, alt.GuidID, m.Info.ProjectManager)

                            args.Tag = GetWhiteboardNodeJSON(alt, m.Info.ProjectManager.Parameters.CS_ItemWidth, m.Info.ProjectManager.Parameters.CS_ItemHeight)
                                                        
                            Success = True
                        End If
                    End If
                End SyncLock

                If Success Then
                    If Not m.Info.SaveOnDemand Then
                        SaveDashboard()
                        SaveStructure() 'A1292
                    End If
                Else
                    sess.OperationResult(New AntiguaOperationEventArgs With {.CmdCode = Command.NoSuccess, .CmdOwner = args.CmdOwner})
                End If
            End If
        End Sub

        Const Xmax As Integer = 1300
        Const Ymax As Integer = 1300
        Const XStep As Integer = 250
        Const YStep As Integer = 80

        Private Sub ArrangeAlternativeOnBoard(ByVal alt As clsVisualNode, ByVal x0 As Integer, ByVal y0 As Integer, ByRef X As Integer, ByRef Y As Integer, ByRef W As Integer, ByRef H As Integer, ByVal copyMode As AntiguaCopyToBoardEventArgs.CopyModes)
            alt.Location = GUILocation.Alternatives
            alt.Attributes.X = X
            alt.Attributes.Y = Y
            alt.Attributes.Width = W
            alt.Attributes.Height = H
            Select Case copyMode
                Case AntiguaCopyToBoardEventArgs.CopyModes.cmList
                    Y += YStep
                    If Y >= Ymax Then
                        Y = y0
                        X += XStep
                    End If
                Case AntiguaCopyToBoardEventArgs.CopyModes.cmTile
                    X += XStep
                    If X >= Xmax Then
                        Y += YStep
                        X = x0
                    End If
            End Select
        End Sub

        Public Sub CopyAllAlternativesToBoard(ByRef args As AntiguaCopyToBoardEventArgs)
            Dim sess = CurrentSession()
            Dim m = CurrentMeeting()
            If (m IsNot Nothing) AndAlso (sess IsNot Nothing) Then

                Dim Success As Boolean = False
                args.ArrangedNodesCoords = New Dictionary(Of Guid, Drawing.Point)

                SyncLock SyncObj
                    Dim AltH As clsHierarchy = Nothing
                    If m.Info.ProjectManager.AltsHierarchies.Count > 0 Then 'A0160
                        AltH = m.Info.ProjectManager.AltsHierarchy(m.Info.ProjectManager.ActiveAltsHierarchy)

                        If args.Position.X > Xmax OrElse args.Position.X < 0 Then args.Position = New System.Drawing.Point(20, args.Position.Y)
                        If args.Position.Y > Ymax OrElse args.Position.Y < 0 Then args.Position = New System.Drawing.Point(args.Position.X, 20)

                        Dim X As Integer = args.Position.X
                        Dim Y As Integer = args.Position.Y

                        For Each Node As clsNode In AltH.Nodes
                            Dim NodeGuid As Guid = Node.NodeGuidID
                            Dim boardNode As clsVisualNode = m.Info.ProjectManager.AntiguaDashboard.GetNodeByGuid(NodeGuid)
                            Dim recycleNode As clsVisualNode = m.Info.ProjectManager.AntiguaRecycleBin.GetNodeByGuid(NodeGuid)
                            If boardNode Is Nothing AndAlso recycleNode Is Nothing Then
                                Dim alt As clsVisualNode = clsNodeToVisualNode(Node)
                                ArrangeAlternativeOnBoard(alt, args.Position.X, args.Position.Y, X, Y, args.Size.X, args.Size.Y, args.CopyMode)
                                alt.IsAlternative = True
                                alt.GuidID = NodeGuid
                                m.Info.ProjectManager.AntiguaDashboard.AddNode(alt)
                                alt.WasAlreadyMovedToHierarchy = True
                                CopyInfodocFromNodeToVisualNode(Node, alt.GuidID, m.Info.ProjectManager)
                                If Not args.ArrangedNodesCoords.ContainsKey(alt.GuidID) Then args.ArrangedNodesCoords.Add(alt.GuidID, New Drawing.Point(CInt(alt.Attributes.X), CInt(alt.Attributes.Y)))
                                Success = True
                            Else
                                If recycleNode Is Nothing AndAlso boardNode IsNot Nothing Then
                                    ArrangeAlternativeOnBoard(boardNode, args.Position.X, args.Position.Y, X, Y, args.Size.X, args.Size.Y, args.CopyMode)
                                    CopyInfodocFromNodeToVisualNode(Node, boardNode.GuidID, m.Info.ProjectManager) 'A0654
                                    If Not args.ArrangedNodesCoords.ContainsKey(boardNode.GuidID) Then args.ArrangedNodesCoords.Add(boardNode.GuidID, New Drawing.Point(CInt(boardNode.Attributes.X), CInt(boardNode.Attributes.Y)))
                                    boardNode.WasAlreadyMovedToHierarchy = True
                                    Success = True
                                Else
                                    If boardNode Is Nothing AndAlso recycleNode IsNot Nothing Then
                                        ArrangeAlternativeOnBoard(recycleNode, args.Position.X, args.Position.Y, X, Y, args.Size.X, args.Size.Y, args.CopyMode)
                                        recycleNode.IsAlternative = True
                                        recycleNode.WasAlreadyMovedToHierarchy = True
                                        m.Info.ProjectManager.AntiguaDashboard.AddNode(recycleNode)
                                        m.Info.ProjectManager.AntiguaRecycleBin.RemoveNodeByGuid(recycleNode.GuidID)
                                        If Not args.ArrangedNodesCoords.ContainsKey(recycleNode.GuidID) Then args.ArrangedNodesCoords.Add(recycleNode.GuidID, New Drawing.Point(CInt(recycleNode.Attributes.X), CInt(recycleNode.Attributes.Y)))
                                        CopyInfodocFromNodeToVisualNode(Node, recycleNode.GuidID, m.Info.ProjectManager) 'A0654
                                        Success = True
                                    End If
                                End If
                            End If
                        Next

                        args.Tag = String.Format("[{0}]", GetWhiteboardNodesJSON())
                    End If
                End SyncLock

                If Success Then
                    If Not m.Info.SaveOnDemand Then
                        SaveDashboard()
                        SaveRecycleBin()
                        SaveStructure() 'A1292
                    End If
                Else
                    sess.OperationResult(New AntiguaOperationEventArgs With {.CmdCode = Command.NoSuccess, .CmdOwner = args.CmdOwner})
                End If
            End If
        End Sub

        Public Sub ReorderAlternatives(ByRef args As AntiguaReorderOperationEventArgs)
            'C0641===
            If args.Action = NodeMoveAction.nmaAsChildOfNode Then
                args.Action = NodeMoveAction.nmaAfterNode
            End If
            'C0641==
            Dim sess = CurrentSession()
            Dim m = CurrentMeeting()

            Dim Success As Boolean = False

            If (m IsNot Nothing) AndAlso (sess IsNot Nothing) Then
                SyncLock SyncObj

                    Dim AltH As clsHierarchy = Nothing
                    If m.Info.ProjectManager.AltsHierarchies.Count > 0 Then
                        AltH = m.Info.ProjectManager.AltsHierarchy(m.Info.ProjectManager.ActiveAltsHierarchy)
                        If AltH IsNot Nothing Then
                            Dim destNode As clsNode = AltH.GetNodeByID(args.DestNodeGuid)
                            Dim srcNode As clsNode = AltH.GetNodeByID(args.SourceNodeGuid)
                            If destNode IsNot Nothing AndAlso srcNode IsNot Nothing Then
                                AltH.MoveNode(srcNode, destNode, args.Action)

                                Success = True
                            End If
                        End If
                    End If

                End SyncLock

                If Success Then
                    If Not m.Info.SaveOnDemand Then SaveStructure() 'A1292
                Else
                    sess.OperationResult(New AntiguaOperationEventArgs With {.CmdCode = Command.NoSuccess, .CmdOwner = args.CmdOwner})
                End If
            End If
        End Sub

#End Region

#Region "Misc Routines"

        Protected Overrides Sub Finalize()
            MyBase.Finalize()
            'If mMeetingLockTimer IsNot Nothing Then mMeetingLockTimer.Dispose()
        End Sub

        Public Function HexStringToInt(ByVal s As String) As Integer
            s = s.Trim
            If s <> "" AndAlso s(0) = "#" Then s = s.Substring(1)

            Dim i As Integer = 0
            Int32.TryParse(s, System.Globalization.NumberStyles.HexNumber, Globalization.CultureInfo.CurrentCulture, i)
            Return i
        End Function

        Public Sub ChangeNodesColor(ByRef args As AntiguaPropertiesOperationEventArgs)
            Dim m = CurrentMeeting()
            Dim sess = CurrentSession()

            If (m IsNot Nothing) AndAlso (sess IsNot Nothing) Then
                SyncLock SyncObj
                    'A0219 ===
                    Dim LastModifiedBy As String = ""
                    For Each client In m.Sessions
                        If client.Value.UserToken.TokenID = args.CmdOwner Then
                            LastModifiedBy = String.Format("{0} ({1})", client.Value.UserToken.UserName, client.Value.UserToken.Email)
                            Exit For
                        End If
                    Next
                    'A0219 ==
                    Dim aNode = m.Info.ProjectManager.AntiguaDashboard.GetNodeByGuid(args.NodeGuid)
                    If aNode IsNot Nothing Then
                        aNode.Attributes.BackGroundColor = HexStringToInt(args.sColor)
                        If LastModifiedBy <> "" Then aNode.LastModifiedBy = LastModifiedBy
                    End If
                End SyncLock

                If Not m.Info.SaveOnDemand Then
                    SaveDashboard()
                End If

            End If
        End Sub

        ''' <summary>
        ''' DoRevert is called when Meeting Owner selects a restore point to revert
        ''' </summary>
        ''' <param name="args"></param>
        ''' <remarks></remarks>
        Public Sub DoRevert(ByRef args As AntiguaSaveOperationEventArgs)
            Dim m = CurrentMeeting()
            Dim sess = CurrentSession()
            Dim Success As Boolean = False

            If (m IsNot Nothing) AndAlso (sess IsNot Nothing) Then
                'do revert to the restore point
                If (args.RevertID >= 0) And (args.RevertID < m.RestorePoints.Count) Then
                    SyncLock SyncObj
                        Dim restPoint = m.RestorePoints(args.RevertID)
                        m.Info.ProjectManager.AntiguaDashboard.Nodes.Clear()
                        m.Info.ProjectManager.AntiguaRecycleBin.Nodes.Clear()
                        m.Info.ProjectManager.Hierarchy(m.Info.ProjectManager.ActiveHierarchy).Nodes.Clear()
                        m.Info.ProjectManager.AltsHierarchy(m.Info.ProjectManager.ActiveAltsHierarchy).Nodes.Clear()

                        m.Info.ProjectManager.AntiguaDashboard.Nodes = clsMeeting.CloneVNodesList(restPoint.DashBoardNodes)
                        m.Info.ProjectManager.AntiguaRecycleBin.Nodes = clsMeeting.CloneVNodesList(restPoint.Recycle)
                        m.Info.ProjectManager.Hierarchy(m.Info.ProjectManager.ActiveHierarchy).Nodes = clsMeeting.CloneNodesList(restPoint.Hierarchy)
                        m.Info.ProjectManager.AltsHierarchy(m.Info.ProjectManager.ActiveAltsHierarchy).Nodes = clsMeeting.CloneNodesList(restPoint.AltsHierarchy)
                    End SyncLock
                    Success = True
                End If

                ' notify clients
                If Success Then
                    If Not m.Info.SaveOnDemand Then
                        DoSaveAll()
                    End If
                Else
                    sess.OperationResult(New AntiguaOperationEventArgs With {.CmdCode = Command.NoSuccess, .CmdOwner = args.CmdOwner})
                End If
            End If

        End Sub

        ' D0589 + D4953 ===
        Public Function ProjectLockInfoSet(ByVal sConnectionString As String, ByVal tProviderType As GenericDBAccess.ECGenericDatabaseAccess.GenericDB.DBProviderType, ByVal tProjectID As Integer, ByRef sLockUserID As Integer, ByVal tLockStatus As ECLockStatus, ByVal tExpiration As Nullable(Of DateTime)) As Boolean
            Dim fResult As Boolean = False
            Using tDatabase As New clsDatabaseAdvanced(sConnectionString, tProviderType)    ' D2235
                If tProjectID > 0 AndAlso sLockUserID > 0 AndAlso tDatabase.Connect Then
                    If tLockStatus <> ECLockStatus.lsUnLocked Then
                        If Not tExpiration.HasValue Then tExpiration = Now.AddSeconds(_DEF_LOCK_TIMEOUT)
                        If tExpiration < Now Then tLockStatus = ECLockStatus.lsUnLocked
                    Else
                        If Not tExpiration.HasValue Then tExpiration = Now
                    End If
                    Dim tParams As New List(Of Object)
                    tParams.Clear()
                    tParams.Add(tExpiration.Value)
                    fResult = tDatabase.ExecuteSQL(String.Format("UPDATE Projects SET LockStatus={0}, LockedByUserID={1}, LockExpiration=? WHERE ID={2}", CInt(tLockStatus), sLockUserID, tProjectID), tParams) > 0
                    'DebugInfo(String.Format("Set project expiration {0}/{2}: {3} (prj #{1})", tLockStatus, tProjectID, tExpiration, fResult))
                    tDatabase.Close()
                Else
                    'DebugInfo("Can't open DB connection", _TRACE_WARNING)
                End If
            End Using
            Return fResult
        End Function
        ' D0589 + D4953 ==

#End Region

#Region "Pros Cons and Comments"

        Private Sub SwitchProsCons(ByRef args As AntiguaSwitchProsConsEventArgs)
            Dim m = CurrentMeeting()
            Dim sess = CurrentSession()

            If m Is Nothing OrElse sess Is Nothing Then
                Exit Sub
            End If

            Dim Success As Boolean = False

            Dim BoardNode As clsVisualNode

            SyncLock SyncObj
                If args.Mode = "all" Then
                    For Each tNode In m.Info.ProjectManager.AntiguaDashboard.Nodes
                        If tNode.IsAlternative Then tNode.IsProsConsPaneVisible = args.Show
                    Next
                End If
                If args.Mode = "single" Then
                    BoardNode = m.Info.ProjectManager.AntiguaDashboard.GetNodeByGuid(args.NodeGuid)
                    If BoardNode.IsAlternative Then BoardNode.IsProsConsPaneVisible = args.Show
                End If
                If args.Mode = "oneatatime" Then
                    For Each tNode In m.Info.ProjectManager.AntiguaDashboard.Nodes
                        If tNode.IsAlternative Then tNode.IsProsConsPaneVisible = False
                    Next
                    BoardNode = m.Info.ProjectManager.AntiguaDashboard.GetNodeByGuid(args.NodeGuid)
                    If BoardNode.IsAlternative Then BoardNode.IsProsConsPaneVisible = True
                End If
                Success = True
            End SyncLock

            If Success Then
                If Not m.Info.SaveOnDemand Then
                    SaveDashboard()
                End If
            Else
                sess.OperationResult(New AntiguaOperationEventArgs With {.CmdCode = Command.NoSuccess, .CmdOwner = args.CmdOwner})
            End If
        End Sub

        Private Function GetProsConsItem(ByVal Node As clsVisualNode, ByVal id As Guid) As clsVisualNode
            If Node IsNot Nothing Then
                For Each item In Node.ConsList
                    If item.GuidID.Equals(id) Then
                        Return item
                    End If
                Next

                For Each item In Node.ProsList
                    If item.GuidID.Equals(id) Then
                        Return item
                    End If
                Next
            End If
            Return Nothing
        End Function

        Private Sub DeleteProsConsItem(ByVal Node As clsVisualNode, ByVal id As Guid)
            Dim item = GetProsConsItem(Node, id)

            If item IsNot Nothing Then
                If Node.ProsList.Contains(item) Then Node.ProsList.Remove(item)
                If Node.ConsList.Contains(item) Then Node.ConsList.Remove(item)
            End If
        End Sub

        Private Sub ProsConsOperation(ByRef args As AntiguaProsConsEventArgs)
            Dim m = CurrentMeeting()
            Dim sess = CurrentSession()

            If (m Is Nothing) OrElse (sess Is Nothing) Then
                Exit Sub
            End If

            Dim Success As Boolean = False

            Dim NeedSaveDashboard As Boolean = False
            Dim NeedSaveRecycle As Boolean = False
            Dim NeedSaveTreeView As Boolean = False

            SyncLock SyncObj

                Select Case args.CmdCode
                    'Case Command.MoveFromBoardToProsConsList
                    '    Dim vNode As clsVisualNode = m.Info.ProjectManager.AntiguaDashboard.GetNodeByGuid(args.NodeGuid)
                    '    Dim source As clsVisualNode = m.Info.ProjectManager.AntiguaDashboard.GetNodeByGuid(args.Source(0))
                    '    If vNode IsNot Nothing Then
                    '        If vNode.ProsList Is Nothing Then vNode.ProsList = New List(Of clsVisualNode)
                    '        If vNode.ConsList Is Nothing Then vNode.ConsList = New List(Of clsVisualNode)

                    '        If args.IsPro Then vNode.ProsList.Add(source) Else vNode.ConsList.Add(source)
                    '        m.Info.ProjectManager.AntiguaDashboard.RemoveNodeByGuid(source.GuidID)
                    '        NeedSaveDashboard = True
                    '        Success = True
                    '    End If
                    'Case Command.MoveFromProsConsToBoard
                    '    Dim vNode As clsVisualNode = m.Info.ProjectManager.AntiguaDashboard.GetNodeByGuid(args.NodeGuid)
                    '    Dim source As clsVisualNode = GetProsConsItem(vNode, args.Source(0))

                    '    If vNode IsNot Nothing AndAlso source IsNot Nothing Then
                    '        DeleteProsConsItem(vNode, args.Source(0))
                    '        source.Attributes.X = args.Position.X
                    '        source.Attributes.Y = args.Position.Y
                    '        m.Info.ProjectManager.AntiguaDashboard.AddNode(source)
                    '        NeedSaveDashboard = True
                    '        Success = True
                    '    End If
                    Case Command.CopyFromProsConsToTree
                        Dim vNode As clsVisualNode = m.Info.ProjectManager.AntiguaDashboard.GetNodeByGuid(args.NodeGuid)
                        Dim H As clsHierarchy = m.Info.ProjectManager.Hierarchy(m.Info.ProjectManager.ActiveHierarchy)

                        If H IsNot Nothing Then

                            args.Result = New List(Of Guid)

                            Dim destNode As clsNode = H.GetNodeByID(args.Target(0))
                            If destNode IsNot Nothing Then
                                For Each nodeGuid As Guid In args.Source
                                    Dim pcNode As clsVisualNode = GetProsConsItem(vNode, nodeGuid)
                                    If pcNode IsNot Nothing Then
                                        Dim NewItem As clsVisualNode = pcNode.Clone()
                                        NewItem.GuidID = Guid.NewGuid
                                        NewItem.Text = args.NewItemTitle

                                        Dim srcNode As clsNode = H.AddNode(H.Nodes(0).NodeID, , NewItem.GuidID)
                                        'Dim srcNode As clsNode = AddCSNode(m.Info.ProjectManager, H, H.Nodes(0).NodeID) 'A0958
                                        H.MoveNode(srcNode, destNode, args.Action)

                                        'srcNode.NodeGuidID = NewItem.GuidID
                                        args.Result.Add(NewItem.GuidID)

                                        If Not String.IsNullOrEmpty(NewItem.Text) Then
                                            srcNode.NodeName = NewItem.Text
                                        End If
                                        srcNode.Tag = NewItem

                                        pcNode.WasAlreadyMovedToHierarchy = True

                                        '-A1125 'A0960 ===
                                        ''remove all contributions
                                        'RemoveContributionForNode(srcNode.NodeGuidID, m.Info.ProjectManager.ActiveHierarchy)

                                        ''set up a contribution for the current alternative and new node                                        
                                        'Dim altNode As ECCore.clsNode = m.Info.ProjectManager.AltsHierarchy(m.Info.ProjectManager.ActiveAltsHierarchy).GetNodeByID(args.NodeGuid)
                                        'If altNode IsNot Nothing AndAlso srcNode.IsTerminalNode Then
                                        '    Dim e As New AntiguaMoveToHierarchyOperationEventArgs
                                        '    Dim NodeGUIDs As New List(Of Guid)
                                        '    NodeGUIDs.Add(srcNode.NodeGuidID)
                                        '    SetContribution(NodeGUIDs, altNode.NodeGuidID, True, H.HierarchyID)
                                        'End If
                                        '-A1125 'A0960 ==

                                        m.Info.ProjectManager.CreateHierarchyLevelValuesCH(H)
                                        args.Tag = String.Format("[{0}]", GetObjectivesJSON(CType(H.HierarchyID, ECHierarchyID)))
                                    End If
                                Next
                                NeedSaveDashboard = True
                                NeedSaveTreeView = True
                                Success = True
                            End If
                        End If
                    Case Command.DeleteProOrCon
                        Dim vNode As clsVisualNode = m.Info.ProjectManager.AntiguaDashboard.GetNodeByGuid(args.NodeGuid)

                        If vNode IsNot Nothing Then
                            If args.DoForAll Then 
                                While If(args.IsPro, vNode.ProsList, vNode.ConsList).Count > 0
                                    Dim item As clsVisualNode = If(args.IsPro, vNode.ProsList, vNode.ConsList).Item(0)
                                    DeleteProsConsItem(vNode, item.GuidID)
                                End While
                            Else
                                For Each id In args.Source
                                    Dim source As clsVisualNode
                                    source = GetProsConsItem(vNode, id)
                                    If source IsNot Nothing Then
                                        DeleteProsConsItem(vNode, id)
                                    End If
                                Next
                            End If
                            NeedSaveDashboard = True
                            Success = True
                        End If
                    Case Command.MarkProsConsItem
                        Dim vNode As clsVisualNode = m.Info.ProjectManager.AntiguaDashboard.GetNodeByGuid(args.NodeGuid)

                        If vNode IsNot Nothing Then
                            For Each id In args.Source
                                Dim source As clsVisualNode
                                source = GetProsConsItem(vNode, id)
                                If source IsNot Nothing Then
                                    source.WasAlreadyMovedToHierarchy = args.Mark
                                End If
                            Next
                            NeedSaveDashboard = True
                            Success = True
                        End If

                    Case Command.CreateNewItemToProsCons
                        Dim vNode As clsVisualNode = m.Info.ProjectManager.AntiguaDashboard.GetNodeByGuid(args.NodeGuid)

                        If vNode IsNot Nothing Then
                            If vNode.ProsList Is Nothing Then vNode.ProsList = New List(Of clsVisualNode)
                            If vNode.ConsList Is Nothing Then vNode.ConsList = New List(Of clsVisualNode)
                            
                            Dim names As String() = args.NewItemTitle.Split(Chr(10)) 'CType(VBLf, Char()))
                            Dim prosConsJSON As String = ""
                            For Each name As String In names
                                name = name.Trim
                                If Not String.IsNullOrEmpty(name) Then 
                                    Dim newItem As New clsVisualNode() With {.GuidID = Guid.NewGuid(), .Text = name, .Author = CStr(args.Tag), .LastModifiedBy = CStr(args.Tag)}
                                    If args.IsPro Then vNode.ProsList.Add(newItem) Else vNode.ConsList.Add(newItem)
                                    NeedSaveDashboard = True
                                    prosConsJSON += If(prosConsJSON = "", "", ",") + GetWhiteboardNodeJSON(newItem, m.Info.ProjectManager.Parameters.CS_ItemWidth, m.Info.ProjectManager.Parameters.CS_ItemHeight)
                                    Success = True
                                End If
                            Next

                            args.Tag = "[" + prosConsJSON + "]"
                        End If

                    Case Command.CopyFromProsToCons
                        Dim vNode As clsVisualNode = m.Info.ProjectManager.AntiguaDashboard.GetNodeByGuid(args.NodeGuid)
                        Dim targetNode As clsVisualNode = m.Info.ProjectManager.AntiguaDashboard.GetNodeByGuid(args.Target(0))

                        If vNode IsNot Nothing AndAlso targetNode IsNot Nothing Then
                            If vNode.ProsList Is Nothing Then vNode.ProsList = New List(Of clsVisualNode)
                            If vNode.ConsList Is Nothing Then vNode.ConsList = New List(Of clsVisualNode)
                            If targetNode.ProsList Is Nothing Then targetNode.ProsList = New List(Of clsVisualNode)
                            If targetNode.ConsList Is Nothing Then targetNode.ConsList = New List(Of clsVisualNode)

                            Dim source = GetProsConsItem(vNode, args.Source(0))
                            Dim NewItem As clsVisualNode = source.Clone()
                            NewItem.GuidID = Guid.NewGuid()

                            args.Result = New List(Of Guid)
                            args.Result.Add(NewItem.GuidID)

                            If args.IsPro Then targetNode.ProsList.Add(NewItem) Else targetNode.ConsList.Add(NewItem)

                            NeedSaveDashboard = True
                            Success = True
                        End If
                    Case Command.RenameProsConsItem
                        Dim vNode As clsVisualNode = m.Info.ProjectManager.AntiguaDashboard.GetNodeByGuid(args.NodeGuid)
                        Dim Usr = GetUserToken(args.CmdOwner)

                        If vNode IsNot Nothing Then
                            Dim source = GetProsConsItem(vNode, args.Source(0))
                            If source IsNot Nothing Then
                                source.Text = args.NewItemTitle
                                source.LastModifiedBy = String.Format("{0} ({1})", Usr.UserName, Usr.Email)
                                Success = True
                                NeedSaveDashboard = True
                            End If
                        End If
                End Select
            End SyncLock

            If Success Then
                If Not m.Info.SaveOnDemand Then
                    If NeedSaveDashboard Then SaveDashboard()
                    If NeedSaveRecycle Then SaveRecycleBin()
                    If NeedSaveTreeView Then SaveStructure()
                End If
            Else
                sess.OperationResult(New AntiguaOperationEventArgs With {.CmdCode = Command.NoSuccess, .CmdOwner = args.CmdOwner})
            End If
        End Sub

        Private Sub CommentOperation(ByRef args As AntiguaCommentEventArgs)
            Dim m = CurrentMeeting()
            Dim sess = CurrentSession()

            If (m Is Nothing) OrElse (sess Is Nothing) Then
                Exit Sub
            End If

            Dim Success As Boolean = False

            Dim NeedSaveDashboard As Boolean = False

            SyncLock SyncObj

                Select Case args.CmdCode
                    Case Command.AddComment
                        Dim vNode As clsVisualNode = m.Info.ProjectManager.AntiguaDashboard.GetNodeByGuid(args.NodeGuid)

                        If vNode IsNot Nothing Then
                            If vNode.Comments Is Nothing Then vNode.Comments = New Dictionary(Of Guid, String)

                            If Not vNode.Comments.ContainsKey(args.CommentGuid) Then
                                vNode.Comments.Add(args.CommentGuid, args.CommentString)
                                NeedSaveDashboard = True
                                Success = True
                            End If
                        End If
                    Case Command.EditComment
                        Dim vNode As clsVisualNode = m.Info.ProjectManager.AntiguaDashboard.GetNodeByGuid(args.NodeGuid)

                        If vNode IsNot Nothing Then
                            If vNode.Comments Is Nothing Then vNode.Comments = New Dictionary(Of Guid, String)

                            If vNode.Comments.ContainsKey(args.CommentGuid) Then
                                vNode.Comments(args.CommentGuid) = args.CommentString
                                NeedSaveDashboard = True
                                Success = True
                            End If
                        End If
                    Case Command.DeleteComment
                        Dim vNode As clsVisualNode = m.Info.ProjectManager.AntiguaDashboard.GetNodeByGuid(args.NodeGuid)

                        If vNode IsNot Nothing Then
                            If vNode.Comments Is Nothing Then vNode.Comments = New Dictionary(Of Guid, String)

                            If vNode.Comments.ContainsKey(args.CommentGuid) Then
                                vNode.Comments.Remove(args.CommentGuid)
                                NeedSaveDashboard = True
                                Success = True
                            End If
                        End If
                    Case Command.SwitchComments
                        m.Info.AllowComments = args.AllowComments
                        Success = True
                End Select
            End SyncLock

            If Success Then
                If Not m.Info.SaveOnDemand Then
                    If NeedSaveDashboard Then SaveDashboard()
                End If
            Else
                sess.OperationResult(New AntiguaOperationEventArgs With {.CmdCode = Command.NoSuccess, .CmdOwner = args.CmdOwner})
            End If
        End Sub

#End Region

#Region "Sub-objectives"

        Private Sub DropAsSubObjective(ByRef args As AntiguaReorderOperationEventArgs)
            Dim m = CurrentMeeting()
            Dim sess = CurrentSession()

            If (m Is Nothing) OrElse (sess Is Nothing) Then
                Exit Sub
            End If

            Dim Success As Boolean = False

            SyncLock SyncObj
                Dim vNode = m.Info.ProjectManager.AntiguaDashboard.GetNodeByGuid(args.SourceNodeGuid)
                If vNode IsNot Nothing Then
                    vNode.ParentGuidID = args.DestNodeGuid
                    Success = True
                End If
            End SyncLock

            If Success Then
                If Not m.Info.SaveOnDemand Then
                    SaveDashboard()
                End If
            Else
                sess.OperationResult(New AntiguaOperationEventArgs With {.CmdCode = Command.NoSuccess, .CmdOwner = args.CmdOwner})
            End If
        End Sub

        Private Sub DetachSubObjective(ByRef args As AntiguaReorderOperationEventArgs)
            Dim m = CurrentMeeting()
            Dim sess = CurrentSession()

            If (m Is Nothing) OrElse (sess Is Nothing) Then
                Exit Sub
            End If

            Dim Success As Boolean = False

            SyncLock SyncObj
                Dim targNode = m.Info.ProjectManager.AntiguaDashboard.GetNodeByGuid(args.DestNodeGuid)
                Dim vNode = m.Info.ProjectManager.AntiguaDashboard.GetNodeByGuid(args.SourceNodeGuid)
                If vNode IsNot Nothing Then
                    vNode.ParentGuidID = Guid.Empty
                    vNode.Attributes.X = targNode.Attributes.X + 165
                    vNode.Attributes.Y = targNode.Attributes.Y + 25
                    Success = True
                End If
            End SyncLock

            If Success Then
                If Not m.Info.SaveOnDemand Then
                    SaveDashboard()
                End If
            Else
                sess.OperationResult(New AntiguaOperationEventArgs With {.CmdCode = Command.NoSuccess, .CmdOwner = args.CmdOwner})
            End If
        End Sub

#End Region

#Region "Causes and Consequences"

        Public Function GetContributionsForAlternative(ByVal AltID As Guid) As AntiguaContributionsForAlternativeEventArgs
            Dim m = CurrentMeeting()
            Dim sess = CurrentSession()

            If m Is Nothing OrElse sess Is Nothing Then
                Return Nothing
            End If

            Dim args As New AntiguaContributionsForAlternativeEventArgs With {.AltGuid = AltID}

            SyncLock SyncObj
                Dim H1 As clsHierarchy = m.Info.ProjectManager.Hierarchy(ECTypes.ECHierarchyID.hidLikelihood)
                If H1 IsNot Nothing Then

                    For i As Integer = 1 To H1.Nodes.Count - 1
                        Dim node = H1.Nodes(i)
                        If node.IsTerminalNode Then
                            Dim ca = node.GetContributedAlternatives()
                            If ca IsNot Nothing AndAlso ca.Count > 0 Then
                                For Each alt In ca
                                    If alt.NodeGuidID = AltID Then
                                        args.ContributedCauses.Add(node.NodeGuidID, node.NodeName)
                                        Exit For
                                    End If
                                Next
                            End If
                        End If
                    Next
                End If

                Dim H2 As clsHierarchy = m.Info.ProjectManager.Hierarchy(ECTypes.ECHierarchyID.hidImpact)
                If H2 IsNot Nothing Then

                    For i As Integer = 1 To H2.Nodes.Count - 1
                        Dim node = H2.Nodes(i)
                        If node.IsTerminalNode Then
                            Dim ca = node.GetContributedAlternatives()
                            If ca IsNot Nothing AndAlso ca.Count > 0 Then
                                For Each alt In ca
                                    If alt.NodeGuidID = AltID Then
                                        args.ContributedConsequences.Add(node.NodeGuidID, node.NodeName)
                                        Exit For
                                    End If
                                Next
                            End If
                        End If
                    Next
                End If

            End SyncLock

            Return args
        End Function

        Public Function GetNonContributionsForAlternative(ByVal AltID As Guid, ByVal HierarchyID As ECHierarchyID) As Dictionary(Of Guid, String)
            Dim m = CurrentMeeting()
            Dim sess = CurrentSession()

            If m Is Nothing OrElse sess Is Nothing Then
                Return Nothing
            End If

            Dim args As New Dictionary(Of Guid, String)

            SyncLock SyncObj
                Dim H As clsHierarchy = m.Info.ProjectManager.Hierarchy(HierarchyID)
                If H IsNot Nothing Then

                    For i As Integer = 1 To H.Nodes.Count - 1
                        Dim node = H.Nodes(i)
                        If node.IsTerminalNode Then
                            Dim ca = node.GetContributedAlternatives()

                            Dim IsContributed As Boolean = False
                            If ca IsNot Nothing AndAlso ca.Count > 0 Then
                                For Each alt In ca
                                    If alt.NodeGuidID = AltID Then
                                        IsContributed = True
                                        Exit For
                                    End If
                                Next
                            End If
                            If Not IsContributed Then args.Add(node.NodeGuidID, node.NodeName)
                        End If
                    Next
                End If

            End SyncLock

            Return args
        End Function

        Public Function GetContributionsForObjective(ByVal ObjID As Guid, ByVal HierarchyID As ECHierarchyID) As List(Of AntiguaContributionDataItem)
            Dim m = CurrentMeeting()
            Dim sess = CurrentSession()

            If m Is Nothing OrElse sess Is Nothing Then
                Return Nothing
            End If

            Dim args As New List(Of AntiguaContributionDataItem)

            SyncLock SyncObj
                Dim H As clsHierarchy = m.Info.ProjectManager.Hierarchy(HierarchyID)
                If H IsNot Nothing Then

                    Dim node = H.GetNodeByID(ObjID)
                    If node IsNot Nothing AndAlso node.IsTerminalNode Then
                        Dim ca = node.GetContributedAlternatives()

                        If ca IsNot Nothing AndAlso ca.Count > 0 Then
                            For Each alt In ca
                                args.Add(New AntiguaContributionDataItem With {.ID = alt.NodeGuidID, .Name = alt.NodeName, .BooleanValue = True})
                            Next
                        End If

                    End If
                End If

            End SyncLock

            Return args
        End Function

        Public Function SetContributionAll(ByVal Value As Boolean) As Boolean
            Dim m = CurrentMeeting()
            Dim Objectives As ECCore.clsHierarchy = m.Info.ProjectManager.Hierarchy(m.Info.ProjectManager.ActiveHierarchy)
            Dim Alternatives As ECCore.clsHierarchy = m.Info.ProjectManager.AltsHierarchy(m.Info.ProjectManager.ActiveAltsHierarchy)

            For Each CovObj As ECCore.clsNode In Objectives.TerminalNodes
                CovObj.ChildrenAlts.Clear()
                If Value Then
                    For Each Alt As ECCore.clsNode In Alternatives.TerminalNodes
                        CovObj.ChildrenAlts.Add(Alt.NodeID)
                    Next
                End If
            Next
            m.Info.ProjectManager.StorageManager.Writer.SaveModelStructure()
            Return True
        End Function


        Public Function SetContribution(ByVal NodeGUIDs As List(Of Guid), ByVal AltGUID As Guid, IsContributed As Boolean, HierarchyID As Integer, Optional NotifyClients As Boolean = False) As Boolean
            Dim m = CurrentMeeting()
            Dim sess = CurrentSession()

            If m Is Nothing OrElse sess Is Nothing Then
                Return Nothing
            End If

            SyncLock SyncObj
                If NodeGUIDs IsNot Nothing AndAlso NodeGUIDs.Count > 0 AndAlso Not AltGUID.Equals(Guid.Empty) Then
                    Dim CovObj As ECCore.clsNode
                    Dim Alt As ECCore.clsNode

                    If HierarchyID = -1 Then HierarchyID = m.Info.ProjectManager.ActiveHierarchy

                    Dim Objectives As ECCore.clsHierarchy = m.Info.ProjectManager.Hierarchy(HierarchyID)
                    Dim Alternatives As ECCore.clsHierarchy = m.Info.ProjectManager.AltsHierarchy(m.Info.ProjectManager.ActiveAltsHierarchy)

                    Dim ContributionChanged As Boolean = False 'C0801

                    For Each covobjID As Guid In NodeGUIDs
                        CovObj = Objectives.GetNodeByID(covobjID)
                        If CovObj IsNot Nothing Then
                            Alt = Alternatives.GetNodeByID(AltGUID)
                            If Alt IsNot Nothing Then
                                ' setting the value                                    
                                If IsContributed Then
                                    If Objectives.AltsDefaultContribution = ECAltsDefaultContribution.adcFull AndAlso Objectives.IsUsingDefaultFullContribution Then
                                        'For Each node As ECCore.clsNode In Objectives.TerminalNodes
                                        '    If node.NodeID <> CovObj.NodeID Then
                                        '        For Each A As ECCore.clsNode In Alternatives.TerminalNodes
                                        '            node.ChildrenAlts.Add(A.NodeID)
                                        '        Next
                                        '    End If
                                        'Next
                                        'CovObj.ChildrenAlts.Add(Alt.NodeID)
                                        'ContributionChanged = True
                                    Else
                                        If Not CovObj.ChildrenAlts.Contains(Alt.NodeID) Then
                                            CovObj.ChildrenAlts.Add(Alt.NodeID)
                                            ContributionChanged = True 'C0801
                                        End If
                                    End If
                                Else
                                    If CovObj.ChildrenAlts.Count = 0 AndAlso Objectives.AltsDefaultContribution = ECAltsDefaultContribution.adcFull AndAlso Objectives.IsUsingDefaultFullContribution Then
                                        For Each A As ECCore.clsNode In Alternatives.TerminalNodes
                                            If A.NodeID <> Alt.NodeID Then
                                                CovObj.ChildrenAlts.Add(A.NodeID)
                                                ContributionChanged = True
                                            End If
                                        Next
                                        For Each node As ECCore.clsNode In Objectives.TerminalNodes
                                            If CovObj.NodeID <> node.NodeID Then
                                                For Each A As ECCore.clsNode In Alternatives.TerminalNodes
                                                    node.ChildrenAlts.Add(A.NodeID)
                                                Next
                                            End If
                                        Next
                                    Else
                                        If CovObj.ChildrenAlts.Contains(Alt.NodeID) Then
                                            CovObj.ChildrenAlts.Remove(Alt.NodeID)
                                            ContributionChanged = True
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    Next

                    'C0801===
                    If ContributionChanged Then
                        Dim isNone As Boolean = True
                        Dim isFull As Boolean = True
                        Dim altsCount As Integer = Alternatives.TerminalNodes.Count
                        For Each CovObj In Objectives.TerminalNodes
                            If CovObj.ChildrenAlts.Count <> 0 Then
                                isNone = False
                            End If
                            If CovObj.ChildrenAlts.Count < altsCount Then
                                isFull = False
                            End If
                        Next

                        If isFull OrElse (isNone And Objectives.AltsDefaultContribution = ECAltsDefaultContribution.adcFull) Then
                            For Each CovObj In Objectives.TerminalNodes
                                CovObj.ChildrenAlts.Clear()
                            Next

                            Select Case Objectives.HierarchyID
                                Case ECHierarchyID.hidLikelihood
                                    m.Info.ProjectManager.PipeParameters.AltsDefaultContribution = CType(IIf(isFull, ECAltsDefaultContribution.adcFull, ECAltsDefaultContribution.adcNone), ECAltsDefaultContribution)
                                Case ECHierarchyID.hidImpact
                                    m.Info.ProjectManager.PipeParameters.AltsDefaultContributionImpact = CType(IIf(isFull, ECAltsDefaultContribution.adcFull, ECAltsDefaultContribution.adcNone), ECAltsDefaultContribution)
                            End Select
                            Objectives.AltsDefaultContribution = CType(IIf(isFull, ECAltsDefaultContribution.adcFull, ECAltsDefaultContribution.adcNone), ECAltsDefaultContribution)
                        Else
                            Select Case Objectives.HierarchyID
                                Case ECHierarchyID.hidLikelihood
                                    m.Info.ProjectManager.PipeParameters.AltsDefaultContribution = ECAltsDefaultContribution.adcNone
                                Case ECHierarchyID.hidImpact
                                    m.Info.ProjectManager.PipeParameters.AltsDefaultContributionImpact = ECAltsDefaultContribution.adcNone
                            End Select
                            Objectives.AltsDefaultContribution = ECAltsDefaultContribution.adcNone
                        End If
                        ClearCalculatedValues(m.Info.ProjectManager)
                        m.Info.ProjectManager.SavePipeParameters(PipeStorageType.pstStreamsDatabase, m.Info.ProjectManager.StorageManager.ModelID)
                    End If
                    'C0801==

                    m.Info.ProjectManager.StorageManager.Writer.SaveModelStructure()

                    If NotifyClients AndAlso ContributionChanged Then
                        SyncLock SyncObj
                            Dim args As New AntiguaSwitchProsConsEventArgs
                            args.NodeGuid = AltGUID
                            args.CmdCode = Command.GetContributionsForNode
                        End SyncLock
                        'Else
                        '    sess.OperationResult(New AntiguaOperationEventArgs With {.CmdCode = Command.NoSuccess})
                    End If
                    Return True
                Else
                    Return False
                End If
            End SyncLock

            Return False
        End Function

        Public Function RemoveContributionForNode(ByVal covobjID As Guid, HierarchyID As Integer) As Boolean
            Dim m = CurrentMeeting()
            Dim sess = CurrentSession()

            If m Is Nothing OrElse sess Is Nothing Then
                Return False
            End If

            Dim CovObj As ECCore.clsNode

            If HierarchyID = -1 Then HierarchyID = m.Info.ProjectManager.ActiveHierarchy

            Dim Objectives As ECCore.clsHierarchy = m.Info.ProjectManager.Hierarchy(HierarchyID)
            Dim Alternatives As ECCore.clsHierarchy = m.Info.ProjectManager.AltsHierarchy(m.Info.ProjectManager.ActiveAltsHierarchy)

            Dim ContributionChanged As Boolean = False 'C0801

            CovObj = Objectives.GetNodeByID(covobjID)
            If CovObj IsNot Nothing Then
                For Each Alt As ECCore.clsNode In Alternatives.TerminalNodes
                    If Alt IsNot Nothing Then
                        ' remove the value                                    
                        If CovObj.ChildrenAlts.Count = 0 AndAlso Objectives.AltsDefaultContribution = ECAltsDefaultContribution.adcFull AndAlso Objectives.IsUsingDefaultFullContribution Then
                            For Each A As ECCore.clsNode In Alternatives.TerminalNodes
                                If A.NodeID <> Alt.NodeID Then
                                    CovObj.ChildrenAlts.Add(A.NodeID)
                                    ContributionChanged = True
                                End If
                            Next
                            For Each node As ECCore.clsNode In Objectives.TerminalNodes
                                If CovObj.NodeID <> node.NodeID Then
                                    For Each A As ECCore.clsNode In Alternatives.TerminalNodes
                                        node.ChildrenAlts.Add(A.NodeID)
                                    Next
                                End If
                            Next
                        Else
                            If CovObj.ChildrenAlts.Contains(Alt.NodeID) Then
                                CovObj.ChildrenAlts.Remove(Alt.NodeID)
                                ContributionChanged = True
                            End If
                        End If
                    End If
                Next
            End If

            If ContributionChanged Then
                Dim isNone As Boolean = True
                Dim isFull As Boolean = True
                Dim altsCount As Integer = Alternatives.TerminalNodes.Count
                For Each CovObj In Objectives.TerminalNodes
                    If CovObj.ChildrenAlts.Count <> 0 Then
                        isNone = False
                    End If
                    If CovObj.ChildrenAlts.Count < altsCount Then
                        isFull = False
                    End If
                Next

                If isFull OrElse (isNone And Objectives.AltsDefaultContribution = ECAltsDefaultContribution.adcFull) Then
                    For Each CovObj In Objectives.TerminalNodes
                        CovObj.ChildrenAlts.Clear()
                    Next

                    Select Case Objectives.HierarchyID
                        Case ECHierarchyID.hidLikelihood
                            m.Info.ProjectManager.PipeParameters.AltsDefaultContribution = CType(IIf(isFull, ECAltsDefaultContribution.adcFull, ECAltsDefaultContribution.adcNone), ECAltsDefaultContribution)
                        Case ECHierarchyID.hidImpact
                            m.Info.ProjectManager.PipeParameters.AltsDefaultContributionImpact = CType(IIf(isFull, ECAltsDefaultContribution.adcFull, ECAltsDefaultContribution.adcNone), ECAltsDefaultContribution)
                    End Select
                    Objectives.AltsDefaultContribution = CType(IIf(isFull, ECAltsDefaultContribution.adcFull, ECAltsDefaultContribution.adcNone), ECAltsDefaultContribution)
                Else
                    Select Case Objectives.HierarchyID
                        Case ECHierarchyID.hidLikelihood
                            m.Info.ProjectManager.PipeParameters.AltsDefaultContribution = ECAltsDefaultContribution.adcNone
                        Case ECHierarchyID.hidImpact
                            m.Info.ProjectManager.PipeParameters.AltsDefaultContributionImpact = ECAltsDefaultContribution.adcNone
                    End Select
                    Objectives.AltsDefaultContribution = ECAltsDefaultContribution.adcNone
                End If
                ClearCalculatedValues(m.Info.ProjectManager)
                m.Info.ProjectManager.SavePipeParameters(PipeStorageType.pstStreamsDatabase, m.Info.ProjectManager.StorageManager.ModelID)
            End If

            m.Info.ProjectManager.StorageManager.Writer.SaveModelStructure()
        End Function

        Private Sub SetContribution(ByRef args As AntiguaMoveToHierarchyOperationEventArgs, IsContributed As Boolean)
            Dim m = CurrentMeeting()
            Dim sess = CurrentSession()

            If m Is Nothing OrElse sess Is Nothing Then
                Exit Sub
            End If

            Dim NodeGuids As New List(Of Guid)
            NodeGuids.Add(args.DestNodeGuid)
            If SetContribution(NodeGuids, args.NodesGuids(0), IsContributed, args.HierarchyID) Then

            Else
                sess.OperationResult(New AntiguaOperationEventArgs With {.CmdCode = Command.NoSuccess, .CmdOwner = args.CmdOwner})
            End If
        End Sub

        Public Sub ClearCalculatedValues(ByVal tProjectManager As clsProjectManager, Optional ByVal Node As clsNode = Nothing) 'C0186
            '
            ' AC, put your code here
            ''
            'C0186===

            Dim nodes As New List(Of clsNode) 'C0384
            If Node Is Nothing Then
                nodes = tProjectManager.Hierarchy(tProjectManager.ActiveHierarchy).Nodes
            Else
                nodes.Add(Node)
            End If

            For Each nd As clsNode In nodes
                nd.Judgments.Weights.ClearUserWeights()
                nd.Judgments.ClearCombinedJudgments()
            Next
            'C0186==

        End Sub

#End Region

        Public Function Operation(ByVal param As AntiguaOperationEventArgs) As Boolean
            Dim StartTime As DateTime
            Dim EndTime As DateTime
            Dim m = CurrentMeeting()

            Dim retVal As Boolean = True

            'If param.CmdOwner <> m.Info.ProjectManager.Parameters.CS_MeetingOwner Then Return True
            'If param.CmdOwner <> param.MeetingOwner Then Return True
            'param.isAnonymousAction = param.CmdOwner <> param.MeetingOwner
            If param.isAnonymousAction Then Return True

            If m IsNot Nothing AndAlso m.IsLoggingEnabled Then
                StartTime = DateTime.Now
            End If

            ''todo Optimize
            'If {Command.SetActiveHierarchyID, Command.MoveFromTreeToBoard, Command.MoveFromBoardToTree, Command.MoveAltToList, Command.ConvertCoveringObjsCopy, Command.ConvertCoveringObjsMove, Command.ReorderInTree, Command.ReorderInAlts, Command.Refresh, Command.RenameItem, Command.CopyInObjectives, Command.CopyFromProsConsToTree, Command.AddNode, Command.AddRiskObjectives, Command.DropAsSubObjective, Command.DetachSubObjective, Command.SetContribution, Command.RemoveContribution, Command.SetMeetingState, Command.SetMeetingMode, Command.Setting}.Contains(param.CmdCode) Then
            '    m.Info.ProjectManager.StorageManager.Reader.LoadProject()
            'End If

            ''todo Optimize
            'If {Command.SetActiveHierarchyID, Command.CreateNewNode, Command.DeleteNodes, Command.MoveNodesOnBoard, Command.MoveFromTreeToBoard, Command.MoveFromBoardToTree, Command.CopyAltToBoard, Command.CopyAllAltsToBoard, Command.MoveAltToList, Command.ConvertCoveringObjsCopy, Command.ConvertCoveringObjsMove, Command.SetNodesColor, Command.Refresh, Command.RenameItem, Command.CreateNewItemToProsCons, Command.CopyFromProsConsToTree, Command.DeleteProOrCon, Command.CopyFromProsToCons, Command.RenameProsConsItem, Command.MarkProsConsItem, Command.AddComment, Command.EditComment, Command.DeleteComment, Command.SwitchComments, Command.SendToObjectives, Command.SendToSources, Command.SendToConsequences, Command.SendToAlternatives}.Contains(param.CmdCode) Then
            '    LoadAntiguaPanels(m.Info)
            'End If

            Select Case param.CmdCode
                Case Command.Connect
                    Dim args As AntiguaConnectOperationEventArgs = CType(param, AntiguaConnectOperationEventArgs)
                    Connect(args)    ' D0826
                'Case Command.ChatMessage
                '    Dim args As AntiguaChatOperationEventArgs = CType(param, AntiguaChatOperationEventArgs)
                '    SendChatMessage(args)
                Case Command.AutoSaved
                    SaveAll()
                Case Command.DoRevert
                    Dim args As AntiguaSaveOperationEventArgs = CType(param, AntiguaSaveOperationEventArgs)
                    DoRevert(args)
                Case Command.SetMeetingState
                    Dim args As AntiguaStateOperationEventArgs = CType(param, AntiguaStateOperationEventArgs)
                    SetMeetingState(args)
                Case Command.SetMeetingMode
                    Dim args As AntiguaStateOperationEventArgs = CType(param, AntiguaStateOperationEventArgs)
                    SetMeetingMode(args)
                Case Command.SetActiveHierarchyID
                    Dim args As AntiguaStateOperationEventArgs = CType(param, AntiguaStateOperationEventArgs)
                    SetActiveHierarchyID(args)
                Case Command.SetNodesColor
                    Dim args As AntiguaPropertiesOperationEventArgs = CType(param, AntiguaPropertiesOperationEventArgs)
                    ChangeNodesColor(args)
                'Case Command.SetTreeLock, Command.SetBoardLock
                Case Command.SetMeetingLock
                    Dim args As AntiguaStateOperationEventArgs = CType(param, AntiguaStateOperationEventArgs)
                    SetLock(args)
                Case Command.CreateNewNode
                    Dim args As AntiguaNewNodeOperationEventArgs = CType(param, AntiguaNewNodeOperationEventArgs)
                    PutNewNodeToDashboard(args)
                Case Command.AddNode
                    Dim args As AntiguaAddNodeOperationEventArgs = CType(param, AntiguaAddNodeOperationEventArgs)
                    AddNode(args)
                Case Command.AddRiskObjectives
                    Dim args As AntiguaAddRiskObjectivesOperationEventArgs = CType(param, AntiguaAddRiskObjectivesOperationEventArgs)
                    AddRiskObjectives(args)
                Case Command.DisconnectUser
                    Dim args As AntiguaDisconnectUserEventArgs = CType(param, AntiguaDisconnectUserEventArgs)
                    BootUser(args)
                Case Command.MoveNodesOnBoard
                    Dim args As AntiguaMoveToBoardEventArgs = CType(param, AntiguaMoveToBoardEventArgs)
                    MoveNodeOnBoard(args)
                Case Command.RenameItem
                    Dim args As AntiguaPropertiesOperationEventArgs = CType(param, AntiguaPropertiesOperationEventArgs)
                    RenameItem(args)
                Case Command.ResizeItem
                    Dim args As AntiguaPropertiesOperationEventArgs = CType(param, AntiguaPropertiesOperationEventArgs)
                    ResizeItem(args)
                    'A0131
                Case Command.DeleteNodes
                    Dim args As AntiguaDeleteOperationEventArgs = CType(param, AntiguaDeleteOperationEventArgs)
                    DeleteNodes(args)
                Case Command.ReorderInAlts
                    Dim args As AntiguaReorderOperationEventArgs = CType(param, AntiguaReorderOperationEventArgs)
                    ReorderAlternatives(args)
                Case Command.ReorderInTree
                    Dim args As AntiguaReorderOperationEventArgs = CType(param, AntiguaReorderOperationEventArgs)
                    MoveNodeInHierarchy(args)
                Case Command.MoveFromTreeToBoard
                    Dim args As AntiguaMoveToBoardEventArgs = CType(param, AntiguaMoveToBoardEventArgs)
                    MoveNodeFromTreeToBoard(args)
                Case Command.CopyAltToBoard
                    Dim args As AntiguaMoveToBoardEventArgs = CType(param, AntiguaMoveToBoardEventArgs)
                    CopyAlternativeToBoard(args)
                Case Command.CopyAllAltsToBoard
                    Dim args As AntiguaCopyToBoardEventArgs = CType(param, AntiguaCopyToBoardEventArgs)
                    CopyAllAlternativesToBoard(args)
                Case Command.MoveAltToList
                    Dim args As AntiguaMoveToHierarchyOperationEventArgs = CType(param, AntiguaMoveToHierarchyOperationEventArgs)
                    AddAlternatives(args)
                Case Command.MoveFromBoardToTree
                    Dim args As AntiguaMoveToHierarchyOperationEventArgs = CType(param, AntiguaMoveToHierarchyOperationEventArgs)
                    MoveNodesFromDashboardToHierarchy(args)
                Case Command.RestoreFromRecycle
                    Dim args As AntiguaMoveToBoardEventArgs = CType(param, AntiguaMoveToBoardEventArgs)
                    RestoreFromRecycle(args)
                Case Command.SwitchProsCons
                    Dim args As AntiguaSwitchProsConsEventArgs = CType(param, AntiguaSwitchProsConsEventArgs)
                    SwitchProsCons(args)
                Case Command.GrantPermissions
                    Dim args As AntiguaGrantPermissionsEventArgs = CType(param, AntiguaGrantPermissionsEventArgs)
                    GrantPermissions(args)
                Case Command.CopyInObjectives
                    Dim args As AntiguaCopyOperationEventArgs = CType(param, AntiguaCopyOperationEventArgs)
                    CopyInObjectives(args)
                Case Command.InfoDocChanged
                    Dim args As AntiguaInfoDocChangedEventArgs = CType(param, AntiguaInfoDocChangedEventArgs)
                    InfoDocChanged(args)
                Case Command.CreateNewItemToProsCons, Command.DeleteProOrCon, Command.MarkProsConsItem, Command.CopyFromProsConsToTree, Command.CopyFromProsToCons, Command.RenameProsConsItem
                    Dim args As AntiguaProsConsEventArgs = CType(param, AntiguaProsConsEventArgs)
                    ProsConsOperation(args)
                Case Command.AddComment, Command.DeleteComment, Command.EditComment, Command.SwitchComments
                    Dim args As AntiguaCommentEventArgs = CType(param, AntiguaCommentEventArgs)
                    CommentOperation(args)
                Case Command.ConvertCoveringObjsCopy, Command.ConvertCoveringObjsMove
                    Dim args As AntiguaConvertObjectivesEventArgs = CType(param, AntiguaConvertObjectivesEventArgs)
                    args.doCopy = param.CmdCode = Command.ConvertCoveringObjsCopy
                    ConvertCoveringObjectivesOfSelectedNode(args)
                Case Command.DropAsSubObjective
                    Dim args As AntiguaReorderOperationEventArgs = CType(param, AntiguaReorderOperationEventArgs)
                    DropAsSubObjective(args)
                Case Command.DetachSubObjective
                    Dim args As AntiguaReorderOperationEventArgs = CType(param, AntiguaReorderOperationEventArgs)
                    DetachSubObjective(args)
                Case Command.SendToAlternatives, Command.SendToObjectives, Global.Command.SendToSources, Global.Command.SendToConsequences
                    Dim args As AntiguaReorderOperationEventArgs = CType(param, AntiguaReorderOperationEventArgs)
                    SendNodeToDashboard(args)
                Case Command.SetContribution
                    Dim args As AntiguaMoveToHierarchyOperationEventArgs = CType(param, AntiguaMoveToHierarchyOperationEventArgs)
                    If m.Info.ProjectManager.AltsHierarchy(m.Info.ProjectManager.ActiveAltsHierarchy).GetNodeByID(args.NodesGuids(0)) Is Nothing Then
                        Dim args1 As New AntiguaMoveToHierarchyOperationEventArgs
                        args1.CmdCode = Command.MoveAltToList
                        args1.CmdOwner = args.CmdOwner
                        args1.Position = -1
                        args1.NodesGuids = args.NodesGuids
                        args1.DestNodeGuid = args.NodesGuids(0)
                        AddAlternatives(args1)
                    End If
                    SetContribution(args, True)
                Case Command.RemoveContribution
                    Dim args As AntiguaMoveToHierarchyOperationEventArgs = CType(param, AntiguaMoveToHierarchyOperationEventArgs)
                    SetContribution(args, False)
                Case Command.Setting
                    Dim args As AntiguaSettingEventArgs = CType(param, AntiguaSettingEventArgs)
                    ' do nothing here
                    If args.Name = "item_size" Then 
                        ResizeItems(args)
                    End If
                Case Command.EditGoal
                    Dim args As AntiguaPropertiesOperationEventArgs = CType(param, AntiguaPropertiesOperationEventArgs)
                    m.Info.ProjectManager.ActiveObjectives.Nodes(0).NodeName = args.Title
                    SaveStructure()
            End Select

            If m IsNot Nothing AndAlso m.IsLoggingEnabled Then
                EndTime = DateTime.Now
                Dim username As String = UserNameByToken(m, param.CmdOwner)

                Dim ts As TimeSpan = EndTime.Subtract(StartTime)
                Dim cmdCode As String = param.CmdCode.ToString

                While cmdCode.Length < 25
                    cmdCode += " "
                End While

                If username.Length > 20 Then username = username.Substring(0, 19)

                While username.Length < 20
                    username += " "
                End While

                SyncLock SyncObj
                    For Each session In m.Sessions
                        If session.Value.UserToken.ClientType = ClientType.Owner Then
                            session.Value.LogMessage(username + " " + cmdCode + " " + ts.TotalSeconds.ToString.PadRight(10) + " " + ts.TotalMilliseconds.ToString)
                        End If
                    Next
                End SyncLock

            End If

            Return retVal
        End Function

        Private Function UserNameByToken(ByVal meeting As clsMeeting, ByVal token As Integer) As String
            For Each user In meeting.Sessions
                If user.Value.UserToken.TokenID = token Then
                    Return user.Value.UserToken.UserName
                End If
            Next

            Return ""
        End Function

        Private Sub CopyInfodocFromVisualNodeToNode(ByVal sourceNode As clsVisualNode, ByVal targetNode As clsNode, ByVal ProjectManager As clsProjectManager)
            ProjectManager.AntiguaInfoDocs.LoadAntiguaInfoDocs()
            Dim sContent As String = ProjectManager.AntiguaInfoDocs.GetAntiguaInfoDoc(sourceNode.GuidID)
            If Not String.IsNullOrEmpty(sContent) AndAlso Not sContent = "" Then
                targetNode.InfoDoc = sContent
                ProjectManager.InfoDocs.SetNodeInfoDoc(targetNode.NodeGuidID, sContent)
                ProjectManager.StorageManager.Writer.SaveInfoDocs()
            End If
        End Sub

        Private Sub CopyInfodocFromNodeToNode(ByVal sourceNode As clsNode, ByVal targetNode As clsNode, ByVal ProjectManager As clsProjectManager)
            Dim sContent As String = ProjectManager.InfoDocs.GetNodeInfoDoc(sourceNode.NodeGuidID)
            If Not String.IsNullOrEmpty(sContent) AndAlso Not sContent = "" Then
                targetNode.InfoDoc = sContent
                ProjectManager.InfoDocs.SetNodeInfoDoc(targetNode.NodeGuidID, sContent)
                ProjectManager.StorageManager.Writer.SaveInfoDocs()
            End If
        End Sub

        Private Sub CopyInfodocFromNodeToVisualNode(ByVal sourceNode As clsNode, ByVal targetNodeGuid As Guid, ByVal ProjectManager As clsProjectManager)
            ProjectManager.StorageManager.Reader.LoadInfoDocs()
            Dim sContent As String = sourceNode.InfoDoc
            If Not String.IsNullOrEmpty(sContent) AndAlso Not sContent = "" Then
                ProjectManager.AntiguaInfoDocs.SetAntiguaInfoDoc(targetNodeGuid, sContent)
            End If
        End Sub

        Private Sub ConvertCoveringObjectivesOfSelectedNode(ByRef args As AntiguaConvertObjectivesEventArgs)
            Dim sess = CurrentSession()
            Dim m = CurrentMeeting()
            If (m IsNot Nothing) AndAlso (sess IsNot Nothing) Then

                Dim Success As Boolean = False

                Dim AltH As clsHierarchy = Nothing

                If m.Info.ProjectManager.AltsHierarchies.Count > 0 Then
                    AltH = m.Info.ProjectManager.AltsHierarchy(m.Info.ProjectManager.ActiveAltsHierarchy)
                End If

                If AltH Is Nothing Then Exit Sub
                Dim H As clsHierarchy = m.Info.ProjectManager.Hierarchy(m.Info.ProjectManager.ActiveHierarchy)
                If H IsNot Nothing AndAlso H.HierarchyType = ECHierarchyType.htModel Then

                    SyncLock SyncObj

                        Dim srcNode As clsNode = H.GetNodeByID(args.NodeGuid)
                        Dim nodes As List(Of clsNode) = New List(Of clsNode)
                        ExpandChildNodes(srcNode.GetVisibleNodesBelow(m.Info.ProjectManager.UserID), nodes)
                        If srcNode.NodeID <> 0 Then nodes.Insert(0, srcNode)
                        args.NewAlternativesGuids = New Dictionary(Of Guid, Guid)
                        For Each node In nodes
                            If node.IsTerminalNode Then
                                Dim alt As clsNode = AltH.AddNode(-1)
                                'Dim alt As clsNode = AddCSNode(m.Info.ProjectManager, AltH, -1) 'A0958
                                alt.NodeName = node.NodeName
                                'alt.NodeGuidID = Guid.NewGuid

                                CopyInfodocFromNodeToNode(node, alt, m.Info.ProjectManager)
                                If Not args.NewAlternativesGuids.ContainsKey(node.NodeGuidID) Then args.NewAlternativesGuids.Add(node.NodeGuidID, alt.NodeGuidID)
                                If Not args.doCopy Then H.DeleteNode(node)

                                Success = True
                            End If
                        Next
                    End SyncLock
                End If

                If Success Then
                    If Not m.Info.SaveOnDemand Then
                        SaveStructure()
                    End If
                Else
                    sess.OperationResult(New AntiguaOperationEventArgs With {.CmdCode = Command.NoSuccess, .CmdOwner = args.CmdOwner})
                End If
            End If
        End Sub

        Private Sub ExpandChildNodes(ByVal SourceNodes As List(Of clsNode), ByRef TargetNodes As List(Of clsNode))
            If SourceNodes IsNot Nothing Then
                For Each node In SourceNodes
                    TargetNodes.Add(node)
                    ExpandChildNodes(node.Children, TargetNodes)
                Next
            End If
        End Sub

        Private Sub SendNodeToDashboard(args As AntiguaReorderOperationEventArgs)
            Dim m = CurrentMeeting()
            Dim sess = CurrentSession()
            If (m Is Nothing) OrElse (sess Is Nothing) Then Exit Sub

            Dim Success As Boolean = False
            Dim Node As clsVisualNode

            SyncLock SyncObj
                Node = m.Info.ProjectManager.AntiguaDashboard.GetNodeByGuid(args.SourceNodeGuid)

                If Node IsNot Nothing Then
                    m.Info.ProjectManager.AntiguaInfoDocs.LoadAntiguaInfoDocs()
                    Dim sInfodoc As String = m.Info.ProjectManager.AntiguaInfoDocs.GetAntiguaInfoDoc(Node.GuidID)

                    Node.GuidID = args.DestNodeGuid
                    Node.WasAlreadyMovedToHierarchy = False
                    Node.IsAlternative = args.CmdCode = Global.Command.SendToAlternatives
                    Select Case args.CmdCode
                        Case Command.SendToAlternatives
                            Node.Location = GUILocation.Alternatives
                        Case Command.SendToObjectives, Command.SendToSources
                            Node.Location = GUILocation.Board
                        Case Command.SendToConsequences
                            Node.Location = GUILocation.BoardImpact
                    End Select
                    Node.ParentGuidID = Guid.Empty
                    Node.ProsList.Clear()
                    Node.ConsList.Clear()

                    If Not String.IsNullOrEmpty(sInfodoc) Then
                        Node.InfoDoc = sInfodoc
                        m.Info.ProjectManager.AntiguaInfoDocs.SetAntiguaInfoDoc(Node.GuidID, sInfodoc)
                    End If
                    Success = True
                End If
            End SyncLock

            If Success Then
                If Not m.Info.SaveOnDemand Then
                    SaveDashboard()
                End If
            Else
                sess.OperationResult(New AntiguaOperationEventArgs With {.CmdCode = Command.NoSuccess, .CmdOwner = args.CmdOwner})
            End If
        End Sub

        Public Sub Disconnect()
            If DB IsNot Nothing AndAlso Not DB.DbConnection IsNot Nothing AndAlso DB.DbConnection.State = ConnectionState.Open Then DB.DbConnection.Close() ' D2227
        End Sub

    End Class

End Namespace