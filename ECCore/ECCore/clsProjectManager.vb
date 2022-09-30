Imports System.Collections
Imports System.IO
Imports System.Linq
Imports ECCore
Imports Canvas

Namespace ECCore

    Public Delegate Sub AddLogFunction(Log As String)

    <Serializable()> Partial Public Class clsProjectManager
#Region "Settings"
        Public Property IsRiskProject As Boolean
        Public Property FeedbackOn As Boolean = False
        Public Property OptimizationOn As Boolean = True
        Public Property StoreAllowedPermissions() As Boolean = False
        Public ReadOnly Property LoadOnDemand As Boolean = True
        Public Property DefaultGroupID As Integer = -1

#End Region

#Region "Data"
        Public Property Regions As New clsRegions

        Public Property CacheManager As New CacheManager

        Public Property Controls As New clsControls(Me)

        Public Property Attributes() As New clsAttributes(Me)

        Public Property InfoDocs() As New clsInfoDocs(Me)

        Public ReadOnly Property UsersRoles() As New clsProjectUsersRoles(Me)

        Public ReadOnly Property ControlsRoles() As New clsProjectUsersRoles(Me)

        Public Property AHPSFileManager() As New clsAHPSFileManager

        Public Property ExtraAHPTables() As MemoryStream = Nothing

        Public Property EvaluationGroups() As New clsEvaluationGroups(Me) ' Obsolete. We don't use this anymore. Need to support old project versions only.

        Public Property CombinedGroups() As New clsCombinedGroups(Me)

        Public Property CalculationsManager() As New clsCalculationsManager(Me)

        Public Property StorageManager() As New clsStorageManager(Me)

        Public Property MeasureScales() As New clsMeasureScales(Me)

        Public mXML As New clsXMLDataProvider 'C0430 - this is temporary, for debug purposes only

        Public Property AntiguaDashboard As New clsAntiguaPanel(Me, GUILocation.Board)
        Public Property AntiguaRecycleBin As New clsAntiguaPanel(Me, GUILocation.RecycleBin)
        Public Property AntiguaInfoDocs As New clsAntiguaInfoDocs(Me)

        Public Property AntiguaDashboardImpact As New clsAntiguaPanel(Me, GUILocation.Board)
        Public Property AntiguaRecycleBinImpact As New clsAntiguaPanel(Me, GUILocation.RecycleBin)
        Public Property AntiguaInfoDocsImpact As New clsAntiguaInfoDocs(Me)

        Public Property Comments As New UsersComments(Me)
        Public Property ProjectAnalyzer() As New clsProjectAnalyzer

        Public Property ProjectDataProvider() As New clsProjectDataProvider(Me)
        Public Property PipeBuilder() As clsPipeBuilder
        Public Property PipeParameters() As clsPipeParamaters
        Public ReadOnly Property Parameters As clsProjectParametersWithDefaults

        Public ReadOnly Property Reports As clsReportsCollection    ' D6502

#End Region

#Region "Resource Aligners"
        Private mRA As ResourceAligner = Nothing
        Public Property ResourceAligner As ResourceAligner
            Get
                If IsRiskProject Then Return ResourceAlignerRisk
                If mRA Is Nothing Then mRA = New ResourceAligner(Me)
                Return mRA
            End Get
            Set(value As ResourceAligner)
                If IsRiskProject Then
                    ResourceAlignerRisk = value
                Else
                    mRA = value
                End If
            End Set
        End Property

        Private mRARisk As ResourceAligner = Nothing
        Public Property ResourceAlignerRisk As ResourceAligner
            Get
                If mRARisk Is Nothing Then
                    mRARisk = New ResourceAligner(Me)
                    mRARisk.Solver.SolverLibrary = raSolverLibrary.raBaron
                    'mRARisk.Load()
                End If
                Return mRARisk
            End Get
            Set(value As ResourceAligner)
                mRARisk = value
            End Set
        End Property

        Public Sub InitRA()
            mRA = New ResourceAligner(Me)
            mRA.isLoading = True
            mRA.Scenarios.AddDefaultScenario()
        End Sub

        Public BaronSolverCallback As SolveBaronFunction = Nothing
        Public BaronSolverCallback2 As SolveBaronFunction2 = Nothing
#End Region

        Public AddLog As AddLogFunction = Nothing

        'Public Property RiskionProjectType As RiskionProjectType = RiskionProjectType.Risk ' -D4978 // see PipeParameters.ProjectType

        Public ReadOnly Property Edges As Edges

        Public ReadOnly Property RiskSimulations As RiskSimulations
        Public Property SourceGroups As New EventsGroups(Me)
        Public Property EventsGroups As New EventsGroups(Me)
        Public Property BayesianCalculationManager As New BayesianUpdating(Me)

        Public ReadOnly Property Pipe() As List(Of clsAction)
            Get
                Return PipeBuilder.Pipe
            End Get
        End Property

        Private mIsSynchronousSessionEvaluator As Boolean
        Public Property IsSynchronousSessionEvaluator() As Boolean
            Get
                Return mIsSynchronousSessionEvaluator
            End Get
            Set(ByVal value As Boolean)
                mIsSynchronousSessionEvaluator = value
                PipeBuilder.IsSynchronousSessionEvaluator = mIsSynchronousSessionEvaluator
            End Set
        End Property

        Public LastModifyTime As Nullable(Of DateTime)
        Public Property ProjectName() As String = "New Project"


        Public Property ProjectDescription As String
            Get
                Return InfoDocs.GetProjectDescription
            End Get
            Set(value As String)
                InfoDocs.SetProjectDescription(value)
            End Set
        End Property

        Public ReadOnly Property ProjectLocation() As String
            Get
                Return StorageManager.ProjectLocation
            End Get
        End Property

#Region "User management functions"
        Public Property UsersList As New List(Of clsUser)

        Protected mUserID As Integer
        Public Property UserID() As Integer
            Get
                Return mUserID
            End Get
            Set(ByVal value As Integer)
                'If UserExists(value) Or DataInstanceUserExists(value) Or CombinedGroupUserIDExists(value) Or (value = COMBINED_USER_ID) Then 'C0173 'C0552
                If UserExists(value) Then
                    mUserID = value
                    mUser = GetUserByID(value)
                Else
                    mUser = Nothing
                End If
            End Set
        End Property

        Protected mUser As clsUser
        Public Property User() As clsUser
            Get
                Return mUser
            End Get
            Set(ByVal value As clsUser)
                If mUser IsNot value Then SetUser(value)
            End Set
        End Property

        Protected Sub SetUser(ByVal AUser As ECTypes.clsUser)
            Dim U As clsUser = GetUserByEMail(AUser.UserEMail)

            If (U IsNot Nothing) And (U IsNot mUser) Then
                mUser = U
                mUserID = U.UserID
            Else
                Select Case StorageManager.StorageType
                    Case ECModelStorageType.mstCanvasStreamDatabase
                        Dim newUser As New clsUser
                        newUser.UserID = AUser.UserID
                        newUser.UserEMail = AUser.UserEMail
                        newUser.UserName = AUser.UserName
                        AddUser(AUser)

                        CombinedGroups.UpdateDynamicGroups()
                        StorageManager.Writer.SaveProject(True)
                End Select
            End If
        End Sub

        Public Function UserExists(ByVal UserID As Integer) As Boolean
            Return GetUserByID(UserID) IsNot Nothing
        End Function

        Public Overloads Function GetUserByID(ByVal id As Integer) As clsUser
            Return UsersList.FirstOrDefault(Function(u) (u.UserID = id))
        End Function

        Public Overloads Function GetUserByID(ByVal id As Guid) As clsUser
            Return UsersList.FirstOrDefault(Function(u) (u.UserGuidID.Equals(id)))
        End Function

        Public Function GetUserByEMail(ByVal email As String) As clsUser
            Return UsersList.FirstOrDefault(Function(u) (u.UserEMail.ToLower = email.ToLower))
        End Function

        Protected Function GetMaxUserID() As Integer
            Return UsersList.Select(Function(u) (u.UserID)).DefaultIfEmpty(-1).Max + 1
        End Function

        Public Overloads Function AddUser(ByVal user As clsUser) As Integer
            If user Is Nothing Then Return -1

            Dim i As Integer = 0
            For i = 0 To UsersList.Count - 1
                If UsersList.Item(i).UserID = user.UserID Then
                    UsersList.Item(i) = user
                    Return i
                End If
            Next

            If DefaultGroupID <> -1 Then
                Dim group As clsCombinedGroup = CombinedGroups.GetGroupByID(DefaultGroupID)
                If group IsNot Nothing Then
                    group.UsersList.Add(user)
                End If
            End If

            UsersList.Add(user)
            Return UsersList.Count - 1
        End Function

        Public Overloads Function AddUser(ByVal email As String, Optional ByVal Active As Boolean = True, Optional ByVal Name As String = "") As clsUser
            Dim U As clsUser = GetUserByEMail(email)
            If Not U Is Nothing Then
                Return U
            Else
                U = New clsUser
                U.UserEMail = email
                U.UserID = GetMaxUserID() + 1
                U.UserName = Name
                U.Active = Active
                U.LastJudgmentTime = Now

                If DefaultGroupID <> -1 Then
                    Dim group As clsCombinedGroup = CombinedGroups.GetGroupByID(DefaultGroupID)
                    If group IsNot Nothing Then
                        group.UsersList.Add(U)
                    End If
                End If

                UsersList.Add(U)
                'AddEmptyMissingJudgments(ActiveHierarchy, ActiveAltsHierarchy, U) 'C0120
                Return U
            End If
        End Function

        Public Overloads Sub DeleteUser(ByVal user As clsUser)
            If user IsNot Nothing Then DeleteUser(user.UserEMail, )
        End Sub

        Public Overloads Sub DeleteUser(ByVal email As String, Optional fDoSaveProject As Boolean = True)   ' D2295
            'todo AC base call for remove project, need to clean-up loaded into memory?
            Dim U As clsUser = GetUserByEMail(email)
            'A2101 ===
            For Each grp As clsCombinedGroup In CombinedGroups.GroupsList
                Dim user As clsUser = grp.GetUserByEmail(email)
                If user IsNot Nothing Then grp.UsersList.Remove(user)
            Next
            'A2101 ==
            If U IsNot Nothing Then StorageManager.Writer.DeleteUser(email, fDoSaveProject) 'C0028 + D2295
        End Sub
        Public Overloads Sub DeleteUsers(users As List(Of Integer))
            StorageManager.Writer.DeleteUserData(users)
            For Each id As Integer In users
                Dim u As clsUser = GetUserByID(id)
                If u IsNot Nothing Then
                    'A2101 ===
                    For Each grp As clsCombinedGroup In CombinedGroups.GroupsList
                        Dim user As clsUser = grp.GetUserByEmail(u.UserEMail)
                        If user IsNot Nothing Then grp.UsersList.Remove(user)
                    Next
                    'A2101 ==
                    UsersList.Remove(u)
                End If
            Next
            StorageManager.Writer.SaveModelStructure()  ' .SaveProject(True)
        End Sub

        Public Function GetActiveUsers() As List(Of clsUser)
            Return UsersList.Where(Function(u) (u.Active))
        End Function

        Shared Function GetCamouflagedUsers(tUsers As List(Of clsUser), fDoUserNamesCleanup As Boolean) As List(Of clsUser)
            Dim tResUsers As New List(Of clsUser)
            For i As Integer = 0 To tUsers.Count - 1
                Dim objMD5 As New System.Security.Cryptography.MD5CryptoServiceProvider
                Dim arrData() As Byte
                Dim arrHash() As Byte
                arrData = System.Text.Encoding.UTF8.GetBytes(tUsers(i).UserEMail)
                arrHash = objMD5.ComputeHash(arrData)
                objMD5 = Nothing
                Dim strOutput As New System.Text.StringBuilder(arrHash.Length)
                For j As Integer = 0 To arrHash.Length - 1
                    strOutput.Append(arrHash(j).ToString("X2"))
                Next
                Dim sHash As String = Strings.Left(strOutput.ToString().ToLower, 8)

                Dim tNewUser As clsUser = tUsers(i).Clone
                If fDoUserNamesCleanup Then tNewUser.UserName = String.Format("User {0}", sHash)
                tNewUser.UserEMail = String.Format("user_{0}{1}@ec.com", sHash, If(fDoUserNamesCleanup, "", "_name"))
                tResUsers.Add(tNewUser)
            Next
            Return tResUsers
        End Function
#End Region

#Region "Model hierarchies management functions"

        Private mProjectHierarchies As New List(Of clsHierarchy)
        Public ReadOnly Property Hierarchies() As List(Of clsHierarchy)
            Get
                Return mProjectHierarchies
            End Get
        End Property

        Private mProjectMeasureHierarchies As New List(Of clsHierarchy)
        Public ReadOnly Property MeasureHierarchies() As List(Of clsHierarchy)
            Get
                Return mProjectMeasureHierarchies
            End Get
        End Property

        Private Overloads Function GetHierarchyByID(ByVal ID As Integer) As clsHierarchy
            Dim res As clsHierarchy = Hierarchies.FirstOrDefault(Function(h) (h.HierarchyID = ID))
            If res Is Nothing Then res = MeasureHierarchies.FirstOrDefault(Function(h) (h.HierarchyID = ID))
            Return res
        End Function

        Public Overloads Function GetHierarchyByID(ByVal GuidID As Guid) As clsHierarchy
            Dim res As clsHierarchy = Hierarchies.FirstOrDefault(Function(h) (h.HierarchyGuidID.Equals(GuidID)))
            If res Is Nothing Then res = MeasureHierarchies.FirstOrDefault(Function(h) (h.HierarchyGuidID.Equals(GuidID)))
            Return res
        End Function

        Public Overloads Property Hierarchy(ByVal ID As Integer) As clsHierarchy 'C0261
            Get
                Return GetHierarchyByID(ID)
            End Get
            Set(ByVal value As clsHierarchy)
                For Each H As clsHierarchy In mProjectHierarchies
                    If H.HierarchyID = ID Then H = value
                Next
                ' D1800 ===
                For Each H As clsHierarchy In mProjectMeasureHierarchies
                    If H.HierarchyID = ID Then H = value
                Next
                ' D1800 ==
            End Set
        End Property

        Public Overloads Property Hierarchy(ByVal ID As Guid) As clsHierarchy 'C0261
            Get
                Return GetHierarchyByID(ID)
            End Get
            Set(ByVal value As clsHierarchy)
                For Each H As clsHierarchy In mProjectHierarchies
                    If H.HierarchyGuidID.Equals(ID) Then H = value
                Next
            End Set
        End Property

        Public ReadOnly Property ActiveObjectives() As clsHierarchy
            Get
                Return Hierarchy(ActiveHierarchy)
            End Get
        End Property


        Public Function GetNextHierarchyID() As Integer
            Return GetAllHierarchies().Select(Function(h) (h.HierarchyID)).DefaultIfEmpty(-1).Max() + 1
        End Function

        Public Function AddHierarchy() As clsHierarchy
            Dim hierarchy As New clsHierarchy(Me, ECHierarchyType.htModel)
            Dim id As Integer = GetNextHierarchyID()
            Hierarchies.Add(hierarchy)
            hierarchy.HierarchyID = id
            hierarchy.HierarchyName = "New Model Hierarchy " + id.ToString 'C0384

            Dim Goal As clsNode = hierarchy.AddNode(-1)
            Select Case id
                Case ECHierarchyID.hidLikelihood
                    Goal.NodeName = CStr(If(IsRiskProject, DEFAULT_NODENAME_GOAL_LIKELIHOOD, DEFAULT_NODENAME_GOAL))    ' D2110 + D2256
                Case ECHierarchyID.hidImpact
                    Goal.NodeName = DEFAULT_NODENAME_GOAL_IMPACT ' D2210 + D2256
                Case Else
                    Goal.NodeName = DEFAULT_NODENAME_GOAL   ' D2256
            End Select

            Return hierarchy
        End Function

        Public Sub RemoveHierarchy(ByVal HierarchyID As Integer)
            Hierarchies.RemoveAll(Function(h) (h.HierarchyID = HierarchyID))
        End Sub

        Public Sub VerifyObjectivesHierarchies()
            For Each H As clsHierarchy In Hierarchies
                If H.Nodes.Count = 0 Then H.AddNode(-1)
            Next
        End Sub

        Public Function IsValidHierarchyID(ByVal ID As Integer) As Boolean
            Dim res As Boolean = False
            For Each H As clsHierarchy In mProjectHierarchies
                If H.HierarchyID = ID Then
                    res = True
                End If
            Next
            ' D1800 ===
            If Not res Then
                For Each H As clsHierarchy In mProjectMeasureHierarchies
                    If H.HierarchyID = ID Then
                        res = True
                    End If
                Next
            End If
            ' D1800 ==
            Return res
        End Function

        Private mActiveHierarchy As Integer
        Public Property ActiveHierarchy() As Integer
            Get
                Return mActiveHierarchy
            End Get
            Set(ByVal value As Integer)
                If IsValidHierarchyID(value) AndAlso mActiveHierarchy <> value Then ' D1672
                    mActiveHierarchy = value

                    If IsValidAltsHierarchyID(ActiveAltsHierarchy) Then
                        'StorageManager.Reader.LoadAlternativeContribution(mActiveHierarchy, ActiveAltsHierarchy) 'C0028
                    End If
                End If
            End Set
        End Property

#End Region

#Region "Alternative hierarchies management functions"

        Private mProjectAltsHierarchies As New List(Of clsHierarchy)
        Public ReadOnly Property AltsHierarchies() As List(Of clsHierarchy)
            Get
                Return mProjectAltsHierarchies
            End Get
        End Property

        Private Overloads Function GetAltsHierarchyByID(ByVal ID As Integer) As clsHierarchy 'C0260
            For Each H As clsHierarchy In mProjectAltsHierarchies
                If H.HierarchyID = ID Then
                    Return H
                End If
            Next
            Return Nothing
        End Function

        Private Overloads Function GetAltsHierarchyByID(ByVal GuidID As Guid) As clsHierarchy 'C0260
            For Each H As clsHierarchy In mProjectAltsHierarchies
                If H.HierarchyGuidID = GuidID Then
                    Return H
                End If
            Next
            Return Nothing
        End Function

        Public Overloads Property AltsHierarchy(ByVal ID As Integer) As clsHierarchy 'C0261
            Get
                Return GetAltsHierarchyByID(ID)
            End Get
            Set(ByVal value As clsHierarchy)
                For Each H As clsHierarchy In mProjectAltsHierarchies
                    If H.HierarchyID = ID Then
                        H = value
                    End If
                Next
            End Set
        End Property

        Public ReadOnly Property ActiveAlternatives() As clsHierarchy
            Get
                Return AltsHierarchy(ActiveAltsHierarchy)
            End Get
        End Property

        Public Overloads Property AltsHierarchy(ByVal ID As Guid) As clsHierarchy 'C0261
            Get
                Return GetAltsHierarchyByID(ID)
            End Get
            Set(ByVal value As clsHierarchy)
                For Each H As clsHierarchy In mProjectAltsHierarchies
                    If H.HierarchyGuidID.Equals(ID) Then
                        H = value
                    End If
                Next
            End Set
        End Property

        Public Function AddAltsHierarchy() As clsHierarchy
            Dim hierarchy As New clsHierarchy(Me, ECHierarchyType.htAlternative)
            Dim index As Integer = GetNextHierarchyID()
            mProjectAltsHierarchies.Add(hierarchy)
            mProjectAltsHierarchies.Item(mProjectAltsHierarchies.Count - 1).HierarchyID = index 'C0384
            mProjectAltsHierarchies.Item(mProjectAltsHierarchies.Count - 1).HierarchyName = "New Alts Hierarchy " + index.ToString 'C0384

            Return mProjectAltsHierarchies.Item(mProjectAltsHierarchies.Count - 1) 'C0384
        End Function

        Public Function AddMeasureHierarchy() As clsHierarchy
            Dim hierarchy As New clsHierarchy(Me, ECHierarchyType.htMeasure)
            Dim index As Integer = GetNextHierarchyID()
            mProjectMeasureHierarchies.Add(hierarchy)
            mProjectMeasureHierarchies.Item(mProjectMeasureHierarchies.Count - 1).HierarchyID = index
            mProjectMeasureHierarchies.Item(mProjectMeasureHierarchies.Count - 1).HierarchyName = "New Measure Hierarchy " + index.ToString

            Return mProjectMeasureHierarchies.Item(mProjectMeasureHierarchies.Count - 1)
        End Function

        Public Sub RemoveAltsHierarchy(ByVal index As Integer)
            If IsValidAltsHierarchyID(index) Then
                mProjectAltsHierarchies.RemoveAt(index)
            End If
        End Sub

        Private mActiveAltsHierarchy As Integer
        Public Property ActiveAltsHierarchy() As Integer
            Get
                If AltsHierarchies.Count = 1 Then 'per AC's request ===
                    mActiveAltsHierarchy = AltsHierarchies(0).HierarchyID
                End If ' ==
                Return mActiveAltsHierarchy
            End Get
            Set(ByVal value As Integer)
                If IsValidAltsHierarchyID(value) Then
                    mActiveAltsHierarchy = value

                    If IsValidHierarchyID(ActiveHierarchy) Then
                        'StorageManager.Reader.LoadAlternativeContribution(ActiveHierarchy, mActiveAltsHierarchy) 'C0028
                    End If
                End If
            End Set
        End Property

        Public Function IsValidAltsHierarchyID(ByVal ID As Integer) As Boolean
            Dim res As Boolean = False
            For Each H As clsHierarchy In mProjectAltsHierarchies
                If H.HierarchyID = ID Then
                    res = True
                End If
            Next
            Return res
        End Function
#End Region

#Region "Hierarchies general functions"
        Public Function GetAllHierarchies() As List(Of clsHierarchy)
            Return Hierarchies.Concat(AltsHierarchies).Concat(MeasureHierarchies).ToList
        End Function

        Public Overloads Function GetAnyHierarchyByID(ByVal HierarchyID As Integer) As clsHierarchy
            Dim allHierarchies As List(Of clsHierarchy) = GetAllHierarchies()
            Return allHierarchies.FirstOrDefault(Function(h) (h.HierarchyID = HierarchyID))
        End Function

        Public Overloads Function GetAnyHierarchyByID(ByVal HierarchyGuidID As Guid) As clsHierarchy
            Dim allHierarchies As List(Of clsHierarchy) = GetAllHierarchies()
            Return allHierarchies.FirstOrDefault(Function(h) (h.HierarchyGuidID.Equals(HierarchyGuidID)))
        End Function

        Public Sub CreateHierarchyNodesLinks(ByVal HierarchyID As Integer) 'TODO: (not high priority) Make it friend
            For Each node As clsNode In Hierarchy(HierarchyID).Nodes
                node.ParentNode = Hierarchy(HierarchyID).GetNodeByID(node.ParentNodeID)
            Next
        End Sub

        Private Sub SetLevelValue(ByVal Hierarchy As clsHierarchy, ByVal node As clsNode, ByVal value As Integer)
            node.Level = value
            For Each nd As clsNode In node.Children
                SetLevelValue(Hierarchy, nd, value + 1)
            Next
        End Sub

        Public Sub CreateHierarchyLevelValues(ByVal Hierarchy As clsHierarchy) 'TODO: (not high priority) Make it friend
            For Each node As clsNode In Hierarchy.Nodes
                If node.ParentNode Is Nothing Then SetLevelValue(Hierarchy, node, 0)
            Next
        End Sub

        Private Sub SetLevelValueCH(ByVal node As clsNode, ByVal value As Integer)
            node.Level = value
            For Each nd As clsNode In node.Children
                SetLevelValueCH(nd, value + 1)
            Next
        End Sub

        Public Sub CreateHierarchyLevelValuesCH(ByVal Hierarchy As clsHierarchy)
            If Hierarchy IsNot Nothing Then
                For Each node As clsNode In Hierarchy.Nodes
                    If node.ParentNodes Is Nothing OrElse node.ParentNodes.Count = 0 Then
                        SetLevelValueCH(node, 0)
                    End If
                Next
            End If
        End Sub

        Public Sub MoveAlternatiesToHierarchy()
            For Each node As clsNode In Hierarchy(ActiveHierarchy).TerminalNodes
                For Each alt As clsNode In AltsHierarchy(ActiveAltsHierarchy).TerminalNodes 'C0385
                    Dim newNode As clsNode = Hierarchy(ActiveHierarchy).AddNode(node.ParentNodeID)
                    newNode.NodeName = alt.NodeName
                Next
            Next

            Dim AltRootNodes As List(Of clsNode) = AltsHierarchy(ActiveAltsHierarchy).GetLevelNodes(0) 'C0384
            For Each alt As clsNode In AltRootNodes
                AltsHierarchy(ActiveAltsHierarchy).DeleteNode(alt)
            Next
        End Sub

        Public Function AddImpactHierarchy() As clsHierarchy
            Dim ImpactHierarchy As clsHierarchy = AddHierarchy()
            ImpactHierarchy.HierarchyName = "Impact Hierarchy"
            ImpactHierarchy.HierarchyID = ECHierarchyID.hidImpact
            Return ImpactHierarchy
        End Function

        Public Function CloneMeasureHierarchy(SourceScaleID As Guid, DestScaleID As Guid, Optional CopyJudgments As Boolean = False) As clsHierarchy
            Dim sourceH As clsHierarchy = GetAnyHierarchyByID(SourceScaleID)
            If sourceH Is Nothing Then Return Nothing

            Dim destScale As clsMeasurementScale = MeasureScales.GetScaleByID(DestScaleID)
            Dim destH As clsHierarchy = AddMeasureHierarchy()
            destH.HierarchyName = destScale.Name
            destH.HierarchyGuidID = destScale.GuidID

            For Each node As clsNode In sourceH.Nodes
                Dim newNode As New clsNode(destH)
                newNode.NodeID = node.NodeID
                newNode.NodeGuidID = node.NodeGuidID
                newNode.NodeName = node.NodeName
                newNode.Comment = node.Comment
                If destH.Nodes.Count > 0 Then
                    newNode.ParentNodeID = destH.Nodes(0).NodeID
                    newNode.ParentNode = destH.Nodes(0)
                Else
                    newNode.ParentNodeID = -1
                    newNode.ParentNode = Nothing
                End If

                destH.Nodes.Add(newNode)
            Next

            destH.ResetNodesDictionaries()

            If CopyJudgments Then
                For Each J As clsPairwiseMeasureData In sourceH.Nodes(0).Judgments.JudgmentsFromAllUsers
                    Dim newPWData As New clsPairwiseMeasureData(J.FirstNodeID, J.SecondNodeID, J.Advantage, J.Value, J.ParentNodeID, J.UserID, J.IsUndefined, J.Comment)
                    destH.Nodes(0).Judgments.AddMeasureData(newPWData, True)
                Next
            End If

            Return destH
        End Function

        Public Function HasCompleteHierarchy() As Boolean
            For Each H As clsHierarchy In Hierarchies
                If H.HasCompleteHierarchy Then Return True
            Next
            Return False
        End Function

        Public Function HasCollaborativeStructuringData(ProjectID) As Boolean
            If Not AntiguaDashboard.IsPanelLoaded Then
                AntiguaDashboard.LoadPanel(ECModelStorageType.mstCanvasStreamDatabase, StorageManager.ProjectLocation, GenericDBAccess.ECGenericDatabaseAccess.DBProviderType.dbptSQLClient, ProjectID)
            End If
            Return AntiguaDashboard.Nodes IsNot Nothing AndAlso AntiguaDashboard.Nodes.Count > 0
        End Function

        Public Function GetContributedCoveringObjectives(alt As clsNode, H As clsHierarchy) As List(Of clsNode)
            Dim res As New List(Of clsNode)
            For Each node As clsNode In H.TerminalNodes
                Dim HasContributions As Boolean = node.GetNodesBelow(UNDEFINED_USER_ID).Contains(alt)
                If HasContributions Then res.Add(node)
            Next
            Return res
        End Function

        Public Function UpdateContributions(AltID As Guid, CovObjIDs As List(Of Guid), HierarchyID As ECHierarchyID, Optional ByVal fSave As Boolean = True) As Boolean
            Dim Objectives As ECCore.clsHierarchy = Hierarchy(HierarchyID)
            Dim CoveringObjectives As List(Of ECCore.clsNode) = Objectives.TerminalNodes
            Dim Alternatives As ECCore.clsHierarchy = AltsHierarchy(ActiveAltsHierarchy)

            Dim Alt As ECCore.clsNode = Alternatives.GetNodeByID(AltID)
            Dim ContributionChanged As Boolean = False

            If Alt IsNot Nothing Then
                For Each CovObj As ECCore.clsNode In CoveringObjectives
                    If CovObj IsNot Nothing Then
                        ' setting the value
                        Dim value As Boolean = CovObjIDs.Contains(CovObj.NodeGuidID)
                        If value Then
                            If Objectives.AltsDefaultContribution = ECAltsDefaultContribution.adcFull AndAlso Objectives.IsUsingDefaultFullContribution Then
                                For Each node As ECCore.clsNode In Objectives.TerminalNodes
                                    'If node.NodeID <> CovObj.NodeID Then
                                    For Each A As ECCore.clsNode In Alternatives.TerminalNodes
                                        node.ChildrenAlts.Add(A.NodeID)
                                    Next
                                    'End If
                                Next
                                'CovObj.ChildrenAlts.Add(Alt.NodeID)
                                ContributionChanged = True
                            Else
                                If Not CovObj.ChildrenAlts.Contains(Alt.NodeID) Then
                                    CovObj.ChildrenAlts.Add(Alt.NodeID)
                                    ContributionChanged = True
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
                Next

                If ContributionChanged Then
                    Dim isNone As Boolean = True
                    Dim isFull As Boolean = True
                    Dim altsCount As Integer = Alternatives.TerminalNodes.Count
                    For Each CovObj As clsNode In Objectives.TerminalNodes
                        If CovObj.ChildrenAlts.Count <> 0 Then
                            isNone = False
                        End If
                        If CovObj.ChildrenAlts.Count < altsCount Then
                            isFull = False
                        End If
                    Next

                    If (Objectives.HierarchyID <> ECHierarchyID.hidImpact) AndAlso (isFull OrElse (isNone And Objectives.AltsDefaultContribution = ECAltsDefaultContribution.adcFull)) Then
                        For Each CovObj As clsNode In Objectives.TerminalNodes
                            CovObj.ChildrenAlts.Clear()
                        Next

                        Select Case HierarchyID
                            Case ECHierarchyID.hidLikelihood
                                PipeParameters.AltsDefaultContribution = CType(If(isFull, ECAltsDefaultContribution.adcFull, ECAltsDefaultContribution.adcNone), ECAltsDefaultContribution)
                            Case ECHierarchyID.hidImpact
                                PipeParameters.AltsDefaultContributionImpact = CType(If(isFull, ECAltsDefaultContribution.adcFull, ECAltsDefaultContribution.adcNone), ECAltsDefaultContribution)
                        End Select
                        Objectives.AltsDefaultContribution = CType(If(isFull, ECAltsDefaultContribution.adcFull, ECAltsDefaultContribution.adcNone), ECAltsDefaultContribution)
                    Else
                        Select Case HierarchyID
                            Case ECHierarchyID.hidLikelihood
                                PipeParameters.AltsDefaultContribution = ECAltsDefaultContribution.adcNone
                            Case ECHierarchyID.hidImpact
                                PipeParameters.AltsDefaultContributionImpact = ECAltsDefaultContribution.adcNone
                        End Select
                        Objectives.AltsDefaultContribution = ECAltsDefaultContribution.adcNone
                    End If
                    ClearCalculatedValues()
                    If fSave Then SavePipeParameters(PipeStorageType.pstStreamsDatabase, StorageManager.ModelID)
                End If
                If fSave Then StorageManager.Writer.SaveModelStructure()
            End If
            Objectives.mIsUsingDefaultContribution = Nothing
            Return ContributionChanged
        End Function

        Public Function UpdateContributions(ByVal CovObjIDs As List(Of Guid), ByVal AltIDs As List(Of Guid), ByVal Value As Boolean, Optional HierarchyID As Integer = -1) As Boolean

            Dim Values As New List(Of Boolean)
            For Each obj As Guid In CovObjIDs
                Values.Add(Value)
            Next

            Dim retVal As Boolean = UpdateContributions(CovObjIDs, AltIDs, Values, HierarchyID)
            Return retVal
        End Function

        Public Function UpdateContributions(ByVal CovObjIDs As List(Of Guid), ByVal AltIDs As List(Of Guid), ByVal Values As List(Of Boolean), Optional HierarchyID As Integer = -1, Optional ByVal EventsContributions As List(Of Boolean) = Nothing) As Boolean
            Dim retVal As Boolean = False
            If CovObjIDs IsNot Nothing AndAlso CovObjIDs.Count > 0 Then 'AndAlso AltIDs IsNot Nothing AndAlso AltIDs.Count > 0 Then
                Dim CovObj As ECCore.clsNode
                Dim Alt As ECCore.clsNode

                If HierarchyID = -1 Then HierarchyID = ActiveHierarchy

                Dim Objectives As ECCore.clsHierarchy = Hierarchy(HierarchyID)
                Dim Alternatives As ECCore.clsHierarchy = AltsHierarchy(ActiveAltsHierarchy)

                Dim ContributionChanged As Boolean = False 'C0801

                For Each covobjID As Guid In CovObjIDs
                    CovObj = Objectives.GetNodeByID(covobjID)
                    If CovObj IsNot Nothing Then
                        For Each altID As Guid In AltIDs
                            Alt = Alternatives.GetNodeByID(altID)
                            If Alt IsNot Nothing Then
                                ' setting the value
                                Dim value As Boolean = False
                                If EventsContributions Is Nothing Then value = Values(CovObjIDs.IndexOf(covobjID)) Else value = EventsContributions(AltIDs.IndexOf(altID))
                                If value Then
                                    If Objectives.AltsDefaultContribution = ECAltsDefaultContribution.adcFull AndAlso Objectives.IsUsingDefaultFullContribution Then
                                        For Each node As ECCore.clsNode In Objectives.TerminalNodes
                                            If node.NodeID <> CovObj.NodeID Then
                                                For Each A As ECCore.clsNode In Alternatives.TerminalNodes
                                                    node.ChildrenAlts.Add(A.NodeID)
                                                Next
                                            End If
                                        Next
                                        CovObj.ChildrenAlts.Add(Alt.NodeID)
                                        ContributionChanged = True
                                    Else
                                        If Not CovObj.ChildrenAlts.Contains(Alt.NodeID) Then
                                            CovObj.ChildrenAlts.Add(Alt.NodeID)
                                            ContributionChanged = True 'C0801
                                        End If
                                    End If
                                Else
                                    If CovObj.ChildrenAlts.Count = 0 AndAlso Objectives.AltsDefaultContribution = ECAltsDefaultContribution.adcFull AndAlso Objectives.IsUsingDefaultFullContribution Then
                                        If AltIDs.Count = 1 Then
                                            For Each A As ECCore.clsNode In Alternatives.TerminalNodes
                                                If A.NodeID <> Alt.NodeID Then
                                                    CovObj.ChildrenAlts.Add(A.NodeID)
                                                    ContributionChanged = True
                                                End If
                                            Next
                                        Else
                                            Objectives.AltsDefaultContribution = ECAltsDefaultContribution.adcNone
                                            ContributionChanged = True
                                        End If
                                        For Each node As ECCore.clsNode In Objectives.TerminalNodes
                                            If CovObj.NodeID <> node.NodeID Then
                                                For Each A As ECCore.clsNode In Alternatives.TerminalNodes
                                                    node.ChildrenAlts.Add(A.NodeID)
                                                Next
                                            Else

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
                        Next
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

                    If Objectives.HierarchyID <> ECHierarchyID.hidImpact Then
                        If isFull OrElse (isNone And Objectives.AltsDefaultContribution = ECAltsDefaultContribution.adcFull) Then
                            For Each CovObj In Objectives.TerminalNodes
                                CovObj.ChildrenAlts.Clear()
                            Next

                            Select Case Objectives.HierarchyID
                                Case ECHierarchyID.hidLikelihood
                                    PipeParameters.AltsDefaultContribution = CType(IIf(isFull, ECAltsDefaultContribution.adcFull, ECAltsDefaultContribution.adcNone), ECAltsDefaultContribution)
                                Case ECHierarchyID.hidImpact
                                    PipeParameters.AltsDefaultContributionImpact = CType(IIf(isFull, ECAltsDefaultContribution.adcFull, ECAltsDefaultContribution.adcNone), ECAltsDefaultContribution)
                            End Select
                            Objectives.AltsDefaultContribution = CType(IIf(isFull, ECAltsDefaultContribution.adcFull, ECAltsDefaultContribution.adcNone), ECAltsDefaultContribution)
                        Else
                            Select Case Objectives.HierarchyID
                                Case ECHierarchyID.hidLikelihood
                                    PipeParameters.AltsDefaultContribution = ECAltsDefaultContribution.adcNone
                                Case ECHierarchyID.hidImpact
                                    PipeParameters.AltsDefaultContributionImpact = ECAltsDefaultContribution.adcNone
                            End Select
                            Objectives.AltsDefaultContribution = ECAltsDefaultContribution.adcNone
                        End If
                    End If
                    ClearCalculatedValues()
                End If
                'C0801==

                StorageManager.Writer.SaveModelStructure()

                Objectives.mIsUsingDefaultContribution = Nothing

                PipeParameters.Write(PipeStorageType.pstStreamsDatabase, StorageManager.Location, StorageManager.ProviderType, StorageManager.ModelID)
                Dim sAction As String = "Set" 'A1170
                If Values IsNot Nothing AndAlso Not Values(0) Then sAction = "Reset" 'A1170
                If EventsContributions IsNot Nothing AndAlso Not EventsContributions(0) Then sAction = "Reset" 'A1170
                Dim sComment As String = String.Format("{0} {1} WRT {2}", sAction, IIf(AltIDs.Count > 1, IIf(AltIDs.Count = Alternatives.TerminalNodes.Count, " all alts", String.Format("{0} alt(s)", AltIDs.Count)), If(AltIDs.Count > 0, Alternatives.GetNodeByID(AltIDs(0)).NodeName, "")), IIf(CovObjIDs.Count > 1, IIf(CovObjIDs.Count = Objectives.TerminalNodes.Count, "all cov objs", String.Format("{0} obj(s)", CovObjIDs.Count)), Objectives.GetNodeByID(CovObjIDs(0)).NodeName))    ' D3757 + A1170                

                retVal = True
            End If
            Return retVal
        End Function

#End Region

#Region "Cleanup functions"
        Public Sub ClearCalculatedValues(Optional ByVal Node As clsNode = Nothing)
            Dim nodes As New List(Of clsNode)
            If Node Is Nothing Then
                nodes = Hierarchy(ActiveHierarchy).Nodes
            Else
                nodes.Add(Node)
            End If

            For Each nd As clsNode In nodes
                nd.Judgments.Weights.ClearUserWeights()
                nd.Judgments.ClearCombinedJudgments()
            Next
        End Sub
        Public Sub CleanUpModel(ByVal fDoObjCleanup As Boolean, fDoAltCleanup As Boolean, ByVal fDoUserEmailsCleanup As Boolean, ByVal fDoUserNamesCleanup As Boolean, Optional sObjName As String = "Objective", Optional sAltName As String = "Alternative", Optional sObjImpactName As String = "Objective")   ' D1182 + D1186 + D3432
            If fDoObjCleanup Then  ' D1182 + D3432
                ActiveHierarchy = ECHierarchyID.hidLikelihood   ' D3432
                If Hierarchy(ActiveHierarchy).Nodes.Count > 0 Then
                    For i As Integer = 0 To Hierarchy(ActiveHierarchy).Nodes.Count - 1
                        If i = 0 Then
                            Hierarchy(ActiveHierarchy).Nodes(0).NodeName = "Goal"
                        Else
                            Hierarchy(ActiveHierarchy).Nodes(i).NodeName = sObjName + Hierarchy(ActiveHierarchy).Nodes(i).NodeID.ToString   ' D3432
                        End If
                        If Hierarchy(ActiveHierarchy).Nodes(i).InfoDoc <> "" Then
                            Hierarchy(ActiveHierarchy).Nodes(i).InfoDoc = "InfoDoc for " + Hierarchy(ActiveHierarchy).Nodes(i).NodeName
                        End If
                    Next
                End If
                PipeParameters.CurrentParameterSet = PipeParameters.DefaultParameterSet ' D3432
                PipeParameters.PipeMessages.SetWelcomeText(PipeMessageKind.pmkText, ActiveHierarchy, ActiveAltsHierarchy, "")
                PipeParameters.PipeMessages.SetThankYouText(PipeMessageKind.pmkText, ActiveHierarchy, ActiveAltsHierarchy, "")

                ' D3432 ===
                If IsRiskProject Then
                    ActiveHierarchy = ECHierarchyID.hidImpact
                    If Hierarchy(ActiveHierarchy).Nodes.Count > 0 Then
                        For i As Integer = 0 To Hierarchy(ActiveHierarchy).Nodes.Count - 1
                            If i = 0 Then
                                Hierarchy(ActiveHierarchy).Nodes(0).NodeName = "Goal"
                            Else
                                Hierarchy(ActiveHierarchy).Nodes(i).NodeName = sObjImpactName + Hierarchy(ActiveHierarchy).Nodes(i).NodeID.ToString   ' D3432
                            End If
                            If Hierarchy(ActiveHierarchy).Nodes(i).InfoDoc <> "" Then
                                Hierarchy(ActiveHierarchy).Nodes(i).InfoDoc = "InfoDoc for " + Hierarchy(ActiveHierarchy).Nodes(i).NodeName
                            End If
                        Next
                    End If
                    ' D3432 ===
                    If IsRiskProject Then
                        PipeParameters.CurrentParameterSet = PipeParameters.ImpactParameterSet
                        PipeParameters.PipeMessages.SetWelcomeText(PipeMessageKind.pmkText, ActiveHierarchy, ActiveAltsHierarchy, "")
                        PipeParameters.PipeMessages.SetThankYouText(PipeMessageKind.pmkText, ActiveHierarchy, ActiveAltsHierarchy, "")
                        PipeParameters.CurrentParameterSet = PipeParameters.DefaultParameterSet
                    End If
                    ' D3432 ==
                    ActiveHierarchy = ECHierarchyID.hidLikelihood   ' D3432
                End If
                ' D3432 ==

            End If

            If fDoAltCleanup Then  ' D1182 + D3432
                If AltsHierarchy(ActiveAltsHierarchy).Nodes.Count > 0 Then
                    For i As Integer = 0 To AltsHierarchy(ActiveAltsHierarchy).Nodes.Count - 1
                        AltsHierarchy(ActiveAltsHierarchy).Nodes(i).NodeName = sAltName + " " + (i + 1).ToString    ' D3432
                        If AltsHierarchy(ActiveAltsHierarchy).Nodes(i).InfoDoc <> "" Then
                            AltsHierarchy(ActiveAltsHierarchy).Nodes(i).InfoDoc = "InfoDoc for " + AltsHierarchy(ActiveAltsHierarchy).Nodes(i).NodeName
                        End If
                    Next
                End If
            End If

            If fDoUserEmailsCleanup Then  ' D1182 + D1186
                ' D3657 ===
                Dim tNewUsers As List(Of clsUser) = GetCamouflagedUsers(UsersList, fDoUserNamesCleanup)
                For i As Integer = 0 To UsersList.Count - 1
                    If UsersList(i).UserID >= 0 Then
                        UsersList(i).UserName = tNewUsers(i).UserName
                        UsersList(i).UserEMail = tNewUsers(i).UserEMail
                    End If
                    ' D3657 ==
                Next
            End If

        End Sub
        Public Sub CleanUpUserDataFromMemory(HierarchyID As Integer, ByVal UserID As Integer, Optional ByVal LeaveUserWeights As Boolean = False, Optional ForceDelete As Boolean = False)
            Dim isCombined As Boolean = IsCombinedUserID(UserID)

            Dim H As clsHierarchy = GetAnyHierarchyByID(HierarchyID)
            If H Is Nothing Then Exit Sub

            Dim DoDelete As Boolean = True
            Dim u As clsUser = GetUserByID(UserID)
            If Not isCombined Then
                If (u IsNot Nothing) AndAlso (u.IncludedInSynchronous Or (u.UserID = UserID)) Then
                    DoDelete = False
                End If
            Else
                H.SetCombinedLoaded(UserID, False)
            End If

            If DoDelete Or ForceDelete Then
                For Each node As clsNode In H.Nodes
                    If node.Judgments IsNot Nothing Then
                        node.Judgments.DeleteJudgmentsFromUser(UserID)
                        If Not LeaveUserWeights Then
                            node.Judgments.Weights.ClearUserWeights(UserID)
                            If isCombined Then
                                node.Judgments.ClearCombinedJudgments(CombinedGroups.GetCombinedGroupByUserID(UserID))
                            End If
                        End If
                    End If

                    If node.PWOutcomesJudgments IsNot Nothing Then
                        node.PWOutcomesJudgments.DeleteJudgmentsFromUser(UserID)
                    End If
                Next

                If H.HierarchyID = ECHierarchyID.hidLikelihood Then
                    For Each alt As clsNode In ActiveAlternatives.Nodes
                        If Not LeaveUserWeights Then
                            If alt.DirectJudgmentsForNoCause IsNot Nothing Then
                                alt.DirectJudgmentsForNoCause.DeleteJudgmentsFromUser(UserID)
                                alt.DirectJudgmentsForNoCause.Weights.ClearUserWeights(UserID)
                                If isCombined Then
                                    alt.DirectJudgmentsForNoCause.ClearCombinedJudgments(CombinedGroups.GetCombinedGroupByUserID(UserID))
                                End If
                            End If
                            alt.PWOutcomesJudgments.DeleteJudgmentsFromUser(UserID)
                        End If
                    Next
                End If

                If Not isCombined AndAlso u IsNot Nothing Then
                    u.LastJudgmentTime = VERY_OLD_DATE
                End If
            End If
        End Sub

        Public Sub CleanUpControlsJudgmentsFromMemory(ByVal UserID As Integer, Optional HierarchyID As Integer = -1)
            For Each control As clsControl In Controls.Controls
                If HierarchyID = -1 OrElse ((HierarchyID = ECHierarchyID.hidLikelihood) AndAlso (control.Type = ControlType.ctCause OrElse control.Type = ControlType.ctCauseToEvent)) OrElse ((HierarchyID = ECHierarchyID.hidImpact) AndAlso (control.Type = ControlType.ctConsequence OrElse (control.Type = ControlType.ctConsequenceToEvent))) Then
                    For Each assignment As clsControlAssignment In control.Assignments
                        assignment.Judgments.DeleteJudgmentsFromUser(UserID)
                    Next
                End If
            Next
        End Sub

        Public Sub CleanUpUserPermissionsFromMemory(ByVal UserID As Integer) 'C0782
            'C0902===
            'For Each node As clsNode In Hierarchy(ActiveHierarchy).Nodes
            '    node.UserPermissions(UserID).SpecialPermissions = Nothing
            'Next

            'C0902==
        End Sub
#End Region
        Public Sub MoveNodesInfoDocsToAdvanced()
            For Each H As clsHierarchy In GetAllHierarchies()
                For Each node As clsNode In H.Nodes
                    If node.InfoDoc <> "" Then InfoDocs.SetNodeInfoDoc(node.NodeGuidID, node.InfoDoc)
                Next
            Next
        End Sub

        Public Function IsUsingDefaultWeightForAllUsers() As Boolean
            Return UsersList.FirstOrDefault(Function(u) (u.Weight <> DEFAULT_USER_WEIGHT)) Is Nothing
        End Function

        Public Function GetPreviousNodeLocationForAntigua(ByVal VisualNodeGuid As Guid) As GUILocation 'C0602 'C0638
            Dim vNode As clsVisualNode

            vNode = AntiguaDashboard.GetNodeByGuid(VisualNodeGuid)
            If vNode IsNot Nothing Then
                Return GUILocation.Board
            End If

            vNode = AntiguaRecycleBin.GetNodeByGuid(VisualNodeGuid)
            If vNode IsNot Nothing Then
                Return GUILocation.RecycleBin
            End If

            For Each node As clsNode In Hierarchy(ActiveHierarchy).Nodes
                If node.NodeGuidID = VisualNodeGuid Then
                    Return GUILocation.Treeview
                End If
            Next

            For Each alt As clsNode In AltsHierarchy(ActiveAltsHierarchy).Nodes
                If alt.NodeGuidID = VisualNodeGuid Then
                    Return GUILocation.Alternatives
                End If
            Next

            Return GUILocation.None
        End Function

        Public Sub CheckCustomCombined()
            If Parameters.ResultsCustomCombinedUID >= 0 Then
                Dim tAHPUser As clsUser = GetUserByID(Parameters.ResultsCustomCombinedUID)
                If tAHPUser Is Nothing Then
                    Parameters.ResultsCustomCombinedUID = COMBINED_USER_ID
                    Parameters.ResultsCustomCombinedName = ""
                End If
            End If
        End Sub

        'Public Function FillRiskModel(RiskModelID As Integer) As Boolean
        '    Dim PM As New clsProjectManager(,,)
        '    With StorageManager
        '        PM.LoadProject(.ProjectLocation, .ProviderType, .StorageType, RiskModelID)
        '        Dim AH As clsHierarchy = PM.AltsHierarchy(PM.ActiveAltsHierarchy)
        '        AH.Nodes.Clear()
        '        For Each Alt As clsNode In AltsHierarchy(ActiveAltsHierarchy).TerminalNodes
        '            Dim riskAlt As New clsNode(AH)
        '            riskAlt.NodeID = Alt.NodeID
        '            riskAlt.NodeGuidID = Alt.NodeGuidID
        '            riskAlt.NodeName = Alt.NodeName
        '        Next
        '        .Writer.SaveProject()
        '    End With

        '    PM = Nothing
        '    Return True
        'End Function

        Public Function ImportRisksFromModel(RiskModelID As Integer, fImportAsRisk As Boolean, ByRef ErrorMessage As String) As Boolean ' D3759
            Dim PM As New clsProjectManager(,,)
            Dim AH As clsHierarchy = AltsHierarchy(ActiveAltsHierarchy)
            With StorageManager
                PM.LoadProject(.ProjectLocation, .ProviderType, .StorageType, RiskModelID)
                PM.SetUser(PM.UsersList(0))
                PM.CalculationsManager.Calculate(New clsCalculationTarget(CalculationTargetType.cttCombinedGroup, PM.CombinedGroups.GetDefaultCombinedGroup), PM.Hierarchy(PM.ActiveHierarchy).Nodes(0))
                For Each riskAlt As clsNode In PM.AltsHierarchy(PM.ActiveAltsHierarchy).TerminalNodes
                    For Each a As RAAlternative In ResourceAligner.Scenarios.ActiveScenario.AlternativesFull
                        If a.ID.ToLower = riskAlt.NodeGuidID.ToString.ToLower Then
                            ' D3758 ===
                            Dim tVal As Double = riskAlt.UnnormalizedPriority
                            If Not fImportAsRisk Then tVal = 1 - tVal
                            a.RiskOriginal = tVal
                            ' D3758 ==
                            If ResourceAligner.Scenarios.ActiveScenario.ID = 0 Then
                                ResourceAligner.SetAlternativeRisk(a.ID, a.RiskOriginal, False)
                            End If
                        End If
                    Next
                Next
                If ResourceAligner.Scenarios.ActiveScenario.ID = 0 Then
                    Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, .ProjectLocation, .ProviderType, .ModelID, UNDEFINED_USER_ID)
                End If
            End With
            ResourceAligner.Save()
            PM = Nothing
            Return True
        End Function

        Public Function GetTeamTimeStepForConsensusView(ByVal CovObjID As Integer, ByVal ChildID As Integer, ByVal TeamTimeOwnerID As Integer) As Integer
            Dim PB As New clsPipeBuilder(Me, Me.PipeParameters, Me.Parameters)  ' D3977
            PB.IsSynchronousSession = True
            PB.IsSynchronousSessionEvaluator = False
            PB.SynchronousOwner = GetUserByID(TeamTimeOwnerID)

            PB.CreatePipe(True)

            Dim action As clsAction
            For i As Integer = 0 To PB.Pipe.Count - 1
                action = CType(PB.Pipe(i), clsAction)
                Select Case action.ActionType
                    Case ActionType.atNonPWOneAtATime
                        Dim J As clsNonPairwiseMeasureData = CType(CType(action.ActionData, clsOneAtATimeEvaluationActionData).Judgment, clsNonPairwiseMeasureData)
                        If J.ParentNodeID = CovObjID And J.NodeID = ChildID Then
                            Return i
                        End If
                    Case ActionType.atPairwise ' return the first pairwise step in the pipe for this cluster
                        Dim J As clsPairwiseMeasureData = CType(action.ActionData, clsPairwiseMeasureData)
                        If (J.ParentNodeID = CovObjID) And (J.FirstNodeID = ChildID Or J.SecondNodeID = ChildID) Then
                            Return i
                        End If
                End Select
            Next
            PB = Nothing
            Return -1
        End Function

#Region "Judgments functions"
        Public Sub AddEmptyMissingJudgments(ByVal HierarchyID As Integer, ByVal AltsHierarchyID As Integer, Optional ByVal AUser As clsUser = Nothing, Optional ByVal NodeID As Integer = -1, Optional CheckRoles As Boolean = True) 'C0756
            If Not IsValidHierarchyID(HierarchyID) Or Not IsValidAltsHierarchyID(AltsHierarchyID) Then
                Exit Sub
            End If

            If Hierarchy(HierarchyID).Nodes.Count = 0 Then Exit Sub


            Dim i As Integer
            Dim j As Integer

            Dim UList As New List(Of clsUser) 'C0388
            If AUser IsNot Nothing Then
                UList.Add(AUser)
            End If
            Dim users As List(Of clsUser) = If(AUser IsNot Nothing, UList, UsersList) 'C0388

            Dim nodesBelow As List(Of clsNode) 'C0450

            'Debug.Print("Add empty missing judgments started (clsProjectManager): " + Now.ToString)

            Dim forceAdd As Boolean = True

            For Each user As clsUser In users
                For Each node As clsNode In Hierarchy(HierarchyID).Nodes
                    If (NodeID = -1) OrElse ((NodeID <> -1) AndAlso (NodeID = node.NodeID)) Then 'C0081
                        If CheckRoles Then
                            nodesBelow = node.GetNodesBelow(user.UserID) 'C0450
                        Else
                            nodesBelow = node.GetNodesBelow(UNDEFINED_USER_ID)
                        End If

                        Select Case node.MeasureType
                            Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous
                                Dim DiagonalsEvaluationMode As DiagonalsEvaluation

                                DiagonalsEvaluationMode = Attributes.GetAttributeValue(ATTRIBUTE_EVALUATE_DIAGONALS_MODE_ID, node.NodeGuidID)

                                If node.IsTerminalNode Then
                                    If PipeParameters.ForceAllDiagonalsForAlternatives And (nodesBelow.Count < PipeParameters.ForceAllDiagonalsLimitForAlternatives) Then
                                        DiagonalsEvaluationMode = DiagonalsEvaluation.deAll
                                    End If
                                Else
                                    If PipeParameters.ForceAllDiagonals And (nodesBelow.Count < PipeParameters.ForceAllDiagonalsLimit) Then
                                        DiagonalsEvaluationMode = DiagonalsEvaluation.deAll
                                    End If
                                End If

                                For i = 0 To nodesBelow.Count - 2
                                    For j = i + 1 To nodesBelow.Count - 1
                                        If DiagonalsEvaluationMode = DiagonalsEvaluation.deAll Or
                                        ((DiagonalsEvaluationMode = DiagonalsEvaluation.deFirst) And (j = i + 1)) Or
                                        ((DiagonalsEvaluationMode = DiagonalsEvaluation.deFirstAndSecond) And ((j = i + 1) Or (j = i + 2))) Then 'C0756

                                            Dim pwData As clsPairwiseMeasureData = CType(node.Judgments, clsPairwiseJudgments).PairwiseJudgment(nodesBelow(i).NodeID, nodesBelow(j).NodeID, user.UserID)
                                            If pwData Is Nothing Then
                                                node.Judgments.AddMeasureData(New clsPairwiseMeasureData(nodesBelow(i).NodeID, nodesBelow(j).NodeID, 0, 0, node.NodeID, user.UserID, True), forceAdd)
                                            Else
                                                pwData.AggregatedValue = 1
                                                pwData.AggregatedValues.Clear()
                                                pwData.AggregatedValues2.Clear()
                                            End If
                                        End If
                                    Next
                                Next
                            Case ECMeasureType.mtPWOutcomes
                                Dim outcomes As List(Of clsRating) = CType(node.MeasurementScale, clsRatingScale).RatingSet

                                Dim DiagonalsEvaluationMode As DiagonalsEvaluation
                                'DiagonalsEvaluationMode = GetRatingScaleDiagonalsEvaluation(CType(node.MeasurementScale, clsRatingScale))
                                If node.Hierarchy.HierarchyType = ECHierarchyType.htMeasure Then
                                    DiagonalsEvaluationMode = MeasureScales.GetRatingScaleDiagonalsEvaluation(CType(node.MeasurementScale, clsRatingScale))
                                Else
                                    DiagonalsEvaluationMode = Attributes.GetAttributeValue(ATTRIBUTE_EVALUATE_DIAGONALS_MODE_ID, node.NodeGuidID)
                                End If

                                If node.IsTerminalNode Then
                                    If PipeParameters.ForceAllDiagonalsForAlternatives And (outcomes.Count < PipeParameters.ForceAllDiagonalsLimitForAlternatives) Then
                                        DiagonalsEvaluationMode = DiagonalsEvaluation.deAll
                                    End If
                                Else
                                    If PipeParameters.ForceAllDiagonals And (outcomes.Count < PipeParameters.ForceAllDiagonalsLimit) Then
                                        DiagonalsEvaluationMode = DiagonalsEvaluation.deAll
                                    End If
                                End If

                                For Each child As clsNode In nodesBelow
                                    For i = 0 To outcomes.Count - 2
                                        For j = i + 1 To outcomes.Count - 1
                                            If DiagonalsEvaluationMode = DiagonalsEvaluation.deAll Or
                                            ((DiagonalsEvaluationMode = DiagonalsEvaluation.deFirst) And (j = i + 1)) Or
                                            ((DiagonalsEvaluationMode = DiagonalsEvaluation.deFirstAndSecond) And ((j = i + 1) Or (j = i + 2))) Then

                                                Dim pwData As clsPairwiseMeasureData = child.PWOutcomesJudgments.PairwiseJudgment(CType(outcomes.Item(i), clsRating).ID, CType(outcomes.Item(j), clsRating).ID, user.UserID, child.NodeID, node.NodeID)
                                                If pwData Is Nothing Then
                                                    Dim data As New clsPairwiseMeasureData(CType(outcomes.Item(i), clsRating).ID, CType(outcomes.Item(j), clsRating).ID, 0, 0, child.NodeID, user.UserID, True)
                                                    data.OutcomesNodeID = node.NodeID
                                                    'child.Hierarchy = node.Hierarchy 

                                                    child.PWOutcomesJudgments.AddMeasureData(data, forceAdd)
                                                Else
                                                    pwData.AggregatedValue = 1
                                                    pwData.AggregatedValues.Clear()
                                                    pwData.AggregatedValues2.Clear()
                                                End If
                                            End If
                                        Next
                                    Next
                                Next
                            Case Else
                                Dim altID As Integer
                                For Each alt As clsNode In nodesBelow
                                    altID = alt.NodeID
                                    Dim nonpwData As clsNonPairwiseMeasureData = CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(altID, node.NodeID, user.UserID)
                                    If nonpwData Is Nothing Then
                                        Select Case node.MeasureType
                                            Case ECMeasureType.mtRatings
                                                node.Judgments.AddMeasureData(New clsRatingMeasureData(altID, node.NodeID, user.UserID, Nothing, node.MeasurementScale, True), forceAdd) 'C0260
                                            Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve 'C0026
                                                If IsCombinedUserID(user.UserID) Then
                                                    node.Judgments.AddMeasureData(New clsUtilityCurveMeasureData(altID, node.NodeID, user.UserID, Single.NaN, node.MeasurementScale, True,, True), forceAdd)
                                                Else
                                                    node.Judgments.AddMeasureData(New clsUtilityCurveMeasureData(altID, node.NodeID, user.UserID, Single.NaN, node.MeasurementScale, True), forceAdd)
                                                End If
                                            Case ECMeasureType.mtStep
                                                If IsCombinedUserID(user.UserID) Then
                                                    node.Judgments.AddMeasureData(New clsStepMeasureData(altID, node.NodeID, user.UserID, Nothing, node.MeasurementScale, True,, True), forceAdd)
                                                Else
                                                    node.Judgments.AddMeasureData(New clsStepMeasureData(altID, node.NodeID, user.UserID, Nothing, node.MeasurementScale, True), forceAdd)
                                                End If
                                            Case ECMeasureType.mtDirect
                                                node.Judgments.AddMeasureData(New clsDirectMeasureData(altID, node.NodeID, user.UserID, Single.NaN, True), forceAdd) 'C0260
                                        End Select
                                    Else
                                        nonpwData.AggregatedValue = 0
                                        nonpwData.AggregatedValues.Clear()
                                        nonpwData.AggregatedValues2.Clear()
                                    End If
                                Next
                        End Select
                    End If
                Next

                If Hierarchy(HierarchyID).HierarchyID = ECHierarchyID.hidLikelihood Then
                    Dim parentNodeID As Integer = Hierarchy(HierarchyID).Nodes(0).NodeID
                    For Each alt As clsNode In Hierarchy(HierarchyID).GetUncontributedAlternatives
                        If alt.MeasureType = ECMeasureType.mtPWOutcomes Then
                            Dim outcomes As List(Of clsRating) = CType(alt.MeasurementScale, clsRatingScale).RatingSet

                            Dim DiagonalsEvaluationMode As DiagonalsEvaluation = Attributes.GetAttributeValue(ATTRIBUTE_EVALUATE_DIAGONALS_MODE_ID, alt.NodeGuidID)

                            If PipeParameters.ForceAllDiagonalsForAlternatives And (outcomes.Count < PipeParameters.ForceAllDiagonalsLimitForAlternatives) Then
                                DiagonalsEvaluationMode = DiagonalsEvaluation.deAll
                            End If

                            For i = 0 To outcomes.Count - 2
                                For j = i + 1 To outcomes.Count - 1
                                    If DiagonalsEvaluationMode = DiagonalsEvaluation.deAll Or
                                    ((DiagonalsEvaluationMode = DiagonalsEvaluation.deFirst) And (j = i + 1)) Or
                                    ((DiagonalsEvaluationMode = DiagonalsEvaluation.deFirstAndSecond) And ((j = i + 1) Or (j = i + 2))) Then

                                        Dim pwData As clsPairwiseMeasureData = alt.PWOutcomesJudgments.PairwiseJudgment(CType(outcomes.Item(i), clsRating).ID, CType(outcomes.Item(j), clsRating).ID, user.UserID, alt.NodeID, alt.NodeID)
                                        If pwData Is Nothing Then
                                            Dim data As New clsPairwiseMeasureData(CType(outcomes.Item(i), clsRating).ID, CType(outcomes.Item(j), clsRating).ID, 0, 0, alt.NodeID, user.UserID, True)
                                            data.OutcomesNodeID = alt.NodeID
                                            alt.PWOutcomesJudgments.AddMeasureData(data, forceAdd)
                                        Else
                                            pwData.AggregatedValue = 1
                                            pwData.AggregatedValues.Clear()
                                            pwData.AggregatedValues2.Clear()
                                        End If
                                    End If
                                Next
                            Next
                        Else
                            Dim nonpwJ As clsNonPairwiseMeasureData = alt.DirectJudgmentsForNoCause.GetJudgement(alt.NodeID, parentNodeID, user.UserID)
                            If nonpwJ Is Nothing Then
                                Select Case alt.MeasureType
                                    Case ECMeasureType.mtRatings
                                        nonpwJ = New clsRatingMeasureData(alt.NodeID, parentNodeID, user.UserID, Nothing, alt.MeasurementScale, forceAdd)
                                    Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                        nonpwJ = New clsUtilityCurveMeasureData(alt.NodeID, parentNodeID, user.UserID, Single.NaN, alt.MeasurementScale, forceAdd)
                                    Case ECMeasureType.mtStep
                                        nonpwJ = New clsStepMeasureData(alt.NodeID, parentNodeID, user.UserID, Nothing, alt.MeasurementScale, forceAdd)
                                    Case ECMeasureType.mtDirect
                                        nonpwJ = New clsDirectMeasureData(alt.NodeID, parentNodeID, user.UserID, Single.NaN, forceAdd)
                                End Select
                                If nonpwJ IsNot Nothing Then
                                    alt.DirectJudgmentsForNoCause.AddMeasureData(nonpwJ, forceAdd)
                                End If
                            Else
                                nonpwJ.AggregatedValue = 0
                                nonpwJ.AggregatedValues.Clear()
                                nonpwJ.AggregatedValues2.Clear()
                            End If
                        End If
                    Next

                End If

                If IsRiskProject AndAlso Edges IsNot Nothing AndAlso Edges.Edges IsNot Nothing Then ' D6800
                    For Each alt As clsNode In ActiveAlternatives.TerminalNodes
                        If Edges.Edges.ContainsKey(alt.NodeGuidID) Then
                            For Each edge As Edge In Edges.Edges(alt.NodeGuidID)
                                Dim ToEvent As clsNode = edge.ToNode

                                Dim nonpwJ As clsNonPairwiseMeasureData = alt.EventsJudgments.GetJudgement(ToEvent.NodeID, alt.NodeID, user.UserID)
                                If nonpwJ Is Nothing Then
                                    Select Case edge.MeasurementType
                                        Case ECMeasureType.mtRatings
                                            nonpwJ = New clsRatingMeasureData(ToEvent.NodeID, alt.NodeID, user.UserID, Nothing, edge.MeasurementScale, True)
                                        Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                            nonpwJ = New clsUtilityCurveMeasureData(ToEvent.NodeID, alt.NodeID, user.UserID, Single.NaN, edge.MeasurementScale, True)
                                        Case ECMeasureType.mtStep
                                            nonpwJ = New clsStepMeasureData(ToEvent.NodeID, alt.NodeID, user.UserID, Nothing, edge.MeasurementScale, True)
                                        Case ECMeasureType.mtDirect
                                            nonpwJ = New clsDirectMeasureData(ToEvent.NodeID, alt.NodeID, user.UserID, Single.NaN, True)
                                    End Select
                                    If nonpwJ IsNot Nothing Then
                                        alt.EventsJudgments.AddMeasureData(nonpwJ, False)
                                    End If
                                Else
                                    nonpwJ.AggregatedValue = 0
                                    nonpwJ.AggregatedValues.Clear()
                                    nonpwJ.AggregatedValues2.Clear()
                                End If
                            Next
                        End If
                    Next
                End If

                For Each alt As clsNode In AltsHierarchy(ActiveAltsHierarchy).TerminalNodes
                    Dim covObjs As List(Of clsNode) = GetContributedCoveringObjectives(alt, Hierarchy(HierarchyID))
                    For Each CovObj As clsNode In covObjs
                        Dim nonpwJ As clsNonPairwiseMeasureData = CType(alt.FeedbackJudgments, clsNonPairwiseJudgments).GetJudgement(CovObj.NodeID, alt.NodeID, user.UserID)
                        If nonpwJ Is Nothing Then
                            Select Case alt.FeedbackMeasureType
                                Case ECMeasureType.mtRatings
                                    'nonpwJ = New clsRatingMeasureData(alt.NodeID, parentNodeID, user.UserID, Nothing, alt.MeasurementScale, True)
                                Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                    'nonpwJ = New clsUtilityCurveMeasureData(alt.NodeID, parentNodeID, user.UserID, Single.NaN, alt.MeasurementScale, True)
                                Case ECMeasureType.mtStep
                                    'nonpwJ = New clsStepMeasureData(alt.NodeID, parentNodeID, user.UserID, Nothing, alt.MeasurementScale, True)
                                Case ECMeasureType.mtDirect
                                    nonpwJ = New clsDirectMeasureData(CovObj.NodeID, alt.NodeID, user.UserID, Single.NaN, True)
                            End Select
                            If nonpwJ IsNot Nothing Then
                                alt.FeedbackJudgments.AddMeasureData(nonpwJ, forceAdd)
                            End If
                        Else
                            nonpwJ.AggregatedValue = 0
                            nonpwJ.AggregatedValues.Clear()
                            nonpwJ.AggregatedValues2.Clear()
                        End If
                    Next
                Next
            Next
            'Debug.Print("Add empty missing judgments ended (clsProjectManager): " + Now.ToString)
        End Sub

        Public Function DeleteUserJudgments(ByVal AUser As clsUser, Optional JudgmentsType As ECJudgmentsType = ECJudgmentsType.jtObjectivesAndAlternatives, Optional Hid As Integer = -1) As Boolean 'A1341
            If AUser Is Nothing Then
                Return False
            End If

            If JudgmentsType = ECJudgmentsType.jtControls Then 'A1341
                CleanUpControlsJudgmentsFromMemory(AUser.UserID, Hid)
                StorageManager.Writer.SaveUserJudgmentsControls(AUser)
                Return True
            Else
                For Each H As clsHierarchy In Hierarchies
                    If Hid = -1 OrElse H.HierarchyID = Hid Then 'A1341
                        For Each node As clsNode In H.Nodes
                            'If JudgmentsType = ECJudgmentsType.jtObjectivesAndAlternatives OrElse (JudgmentsType = ECJudgmentsType.jtAlternatives AndAlso node.IsTerminalNode) OrElse (JudgmentsType = ECJudgmentsType.jtObjectives AndAlso Not node.IsTerminalNode) Then 'A1341 'AS/16986===
                            'node.Judgments.DeleteJudgmentsFromUser(AUser.UserID)
                            'node.PWOutcomesJudgments.DeleteJudgmentsFromUser(AUser.UserID)
                            'End If 'AS/16986==

                            If JudgmentsType = ECJudgmentsType.jtObjectivesAndAlternatives Then 'AS/16986===
                                node.Judgments.DeleteJudgmentsFromUser(AUser.UserID)
                                node.PWOutcomesJudgments.DeleteJudgmentsFromUser(AUser.UserID)
                            ElseIf JudgmentsType = ECJudgmentsType.jtObjectives Then
                                If Not node.IsTerminalNode Then
                                    DeleteJudgmentsForNode(node.NodeGuidID, AUser.UserID)
                                End If
                            ElseIf JudgmentsType = ECJudgmentsType.jtAlternatives Then
                                If node.IsTerminalNode Then
                                    DeleteJudgmentsForNode(node.NodeGuidID, AUser.UserID)
                                End If
                            End If 'AS/16986==
                        Next
                    End If 'A1341
                Next

                'If JudgmentsType = ECJudgmentsType.jtObjectivesAndAlternatives Or JudgmentsType = ECJudgmentsType.jtAlternatives Then 'AS/16986===
                '    For Each alt As clsNode In AltsHierarchy(ActiveAltsHierarchy).TerminalNodes
                '        alt.Judgments.DeleteJudgmentsFromUser(AUser.UserID)
                '        alt.PWOutcomesJudgments.DeleteJudgmentsFromUser(AUser.UserID)
                '        alt.DirectJudgmentsForNoCause.DeleteJudgmentsFromUser(AUser.UserID)
                '    Next
                'End If 'AS/16986==

                Return StorageManager.Writer.SaveUserJudgments(AUser)
            End If
        End Function

        Public Function DeleteJudgmentsForNode(nid As Guid, Optional userID As Integer = Integer.MinValue, Optional bDeleteForDescendants As Boolean = False) As Boolean 'AS/11332a + A1206 'AS/11593 added parameter userID 'AS/11332 added parameter bDeleteForDescendants 

            For Each H As clsHierarchy In mProjectHierarchies.Union(mProjectAltsHierarchies) 'A1409
                Dim node As clsNode = H.GetNodeByID(nid) 'A1206
                If node IsNot Nothing Then 'A1206
                    If userID = Integer.MinValue Then 'AS/11593 enclosed and added Else part
                        For Each user As clsUser In UsersList 'delete judgments for all users
                            StorageManager.Reader.LoadUserJudgments(user)
                            If bDeleteForDescendants AndAlso Not node.IsAlternative Then 'AS/11332 enclosed + A1409
                                For Each child As clsNode In node.GetNodesBelow(user.UserID) ''AS/11332b===
                                    child.Judgments.DeleteJudgmentsFromUser(user.UserID)
                                Next ''AS/11332b==
                            End If 'AS/11332

                            'A1409 ===
                            If node.IsAlternative Then
                                'TODO: AC
                                'node.DeleteAlternativeJudgmentsFromUser(user.UserID)
                                'StorageManager.Writer.SaveUserJudgments(user)
                                'A1409 ==
                            Else
                                node.Judgments.DeleteJudgmentsFromUser(user.UserID)
                                StorageManager.Writer.SaveUserJudgments(user)
                            End If
                            If user IsNot Me.User Then
                                CleanUpUserDataFromMemory(H.HierarchyID, user.UserID)
                            End If
                        Next
                    Else 'delete judgments only for selected user 'AS/11593===
                        Dim user As clsUser = GetUserByID(userID)
                        StorageManager.Reader.LoadUserJudgments(user)
                        If bDeleteForDescendants Then 'AS/11332 enclosed
                            For Each child As clsNode In node.GetNodesBelow(userID)
                                child.Judgments.DeleteJudgmentsFromUser(userID)
                            Next
                        End If 'AS/11332

                        node.Judgments.DeleteJudgmentsFromUser(userID)
                        StorageManager.Writer.SaveUserJudgments(user)
                        If user IsNot Me.User Then
                            CleanUpUserDataFromMemory(H.HierarchyID, userID)
                        End If
                    End If 'AS/11593==
                End If
            Next
            CType(Me, clsProjectManager).PipeBuilder.PipeCreated = False ''AS/11332b need to rebuild the pipe after deleting judgments

        End Function

        Public Sub DeleteJudgmentsForAlternative(aid As Integer, userID As Integer) 'AS/11642f 'AS/11642g converted to sub

            Dim destUser As clsUser = GetUserByID(userID)
            StorageManager.Reader.LoadUserJudgments(destUser)

            For Each node As clsNode In Hierarchy(ActiveHierarchy).TerminalNodes
                If Not IsPWMeasurementType(node.MeasureType) Then

                    'For Each srcJ As clsCustomMeasureData In node.Judgments.JudgmentsFromUser(userID)
                    Dim count As Integer = node.Judgments.JudgmentsFromUser(userID).Count - 1
                    For i As Integer = count To 0 Step -1
                        Dim altJ As clsCustomMeasureData = node.Judgments.JudgmentsFromUser(userID).Item(i)
                        Select Case node.MeasureType
                            Case ECMeasureType.mtRatings
                                Dim ratJ As clsRatingMeasureData = CType(altJ, clsRatingMeasureData)
                                If ratJ.NodeID = aid Then
                                    node.Judgments.JudgmentsFromUser(userID).Remove(ratJ)
                                End If
                            Case ECMeasureType.mtStep
                                Dim stepJ As clsStepMeasureData = CType(altJ, clsStepMeasureData)
                                If stepJ.NodeID = aid Then
                                    node.Judgments.JudgmentsFromUser(userID).Remove(stepJ)
                                End If
                            Case ECMeasureType.mtDirect
                                Dim dirJ As clsDirectMeasureData = CType(altJ, clsDirectMeasureData)
                                If dirJ.NodeID = aid Then
                                    node.Judgments.JudgmentsFromUser(userID).Remove(dirJ)
                                End If
                            Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                Dim ucJ As clsUtilityCurveMeasureData = CType(altJ, clsUtilityCurveMeasureData)
                                If ucJ.NodeID = aid Then
                                    node.Judgments.JudgmentsFromUser(userID).Remove(ucJ)
                                End If
                        End Select
                    Next
                End If
            Next

        End Sub

        Public Function DeleteJudgmentsWitNode(nid As Guid, Optional userID As Integer = Integer.MinValue, Optional bDeleteForDescendants As Boolean = False) As Boolean
            For Each H As clsHierarchy In Hierarchies.Union(AltsHierarchies)
                Dim node As clsNode = H.GetNodeByID(nid)
                If node IsNot Nothing Then
                    If userID = Integer.MinValue Then
                        For Each user As clsUser In UsersList 'delete judgments for all users
                            StorageManager.Reader.LoadUserJudgments(user)
                            If bDeleteForDescendants AndAlso Not node.IsAlternative Then 'AS/11332 enclosed + A1409
                                For Each child As clsNode In node.GetNodesBelow(user.UserID) ''AS/11332b===
                                    child.Judgments.DeleteJudgmentsFromUser(user.UserID)
                                Next ''AS/11332b==
                            End If 'AS/11332

                            'A1409 ===
                            If node.IsAlternative Then
                                'TODO: AC
                                'node.DeleteAlternativeJudgmentsFromUser(user.UserID)
                                'StorageManager.Writer.SaveUserJudgments(user)
                                'A1409 ==
                            Else
                                node.Judgments.DeleteJudgmentsFromUser(user.UserID)
                                StorageManager.Writer.SaveUserJudgments(user)
                            End If
                            If user IsNot Me.User Then
                                CleanUpUserDataFromMemory(H.HierarchyID, user.UserID)
                            End If
                        Next
                    Else
                        'delete judgments only for selected user 'AS/11593===
                        Dim user As clsUser = GetUserByID(userID)
                        StorageManager.Reader.LoadUserJudgments(user)
                        If bDeleteForDescendants Then 'AS/11332 enclosed
                            For Each child As clsNode In node.GetNodesBelow(userID)
                                child.Judgments.DeleteJudgmentsFromUser(userID)
                            Next
                        End If 'AS/11332

                        node.Judgments.DeleteJudgmentsFromUser(userID)
                        StorageManager.Writer.SaveUserJudgments(user)
                        If user IsNot Me.User Then
                            CleanUpUserDataFromMemory(H.HierarchyID, userID)
                        End If
                    End If 'AS/11593==
                End If
            Next
            CType(Me, clsProjectManager).PipeBuilder.PipeCreated = False ''AS/11332bneed to rebuild the pipe after deleting judgments

        End Function

        Public Function CopyJudgmentsFromUserToUserForNode(nodeID As Integer, fromUserID As Integer, toUserID As Integer, Optional FullRewrite As Boolean = True, Optional PairwiseOnly As Boolean = False, Optional ActiveHierarchyOnly As Boolean = True) As Boolean 'AS/11594

            Dim srcUser As clsUser = GetUserByID(fromUserID)
            Dim destUser As clsUser = GetUserByID(toUserID)

            If srcUser Is Nothing Or destUser Is Nothing Then Return False

            StorageManager.Reader.LoadUserJudgments(srcUser)

            If PairwiseOnly Then FullRewrite = False

            'If Not FullRewrite Then 'AS/11594===
            '    StorageManager.Reader.LoadUserJudgments(destUser)
            'Else
            '    CleanUpUserDataFromMemory(destUser.UserID, False, True)
            'End If 'AS/11593==
            StorageManager.Reader.LoadUserJudgments(destUser) 'AS/11594

            For Each H As clsHierarchy In Hierarchies
                If Not ActiveHierarchyOnly Or ActiveHierarchyOnly And H.HierarchyID = ActiveHierarchy Then
                    For Each node As clsNode In H.Nodes
                        If node.NodeID = nodeID Then 'AS/11594
                            If Not PairwiseOnly Or (PairwiseOnly And (node.MeasureType = ECMeasureType.mtPairwise Or node.MeasureType = ECMeasureType.mtPWAnalogous Or node.MeasureType = ECMeasureType.mtPWOutcomes)) Then
                                node.Judgments.Weights.ClearUserWeights(destUser.UserID)

                                Dim srcJudgments As List(Of clsCustomMeasureData) = node.Judgments.JudgmentsFromUser(srcUser.UserID)
                                For Each srcJ As clsCustomMeasureData In srcJudgments
                                    If Not srcJ.IsUndefined Then
                                        Dim destJ As clsCustomMeasureData = Nothing
                                        Select Case node.MeasureType
                                            Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous
                                                Dim srcPW As clsPairwiseMeasureData = CType(srcJ, clsPairwiseMeasureData)
                                                destJ = New clsPairwiseMeasureData(srcPW.FirstNodeID, srcPW.SecondNodeID, srcPW.Advantage, srcPW.Value, srcPW.ParentNodeID, destUser.UserID, srcPW.IsUndefined, srcPW.Comment)
                                            Case ECMeasureType.mtRatings
                                                Dim srcR As clsRatingMeasureData = CType(srcJ, clsRatingMeasureData)
                                                destJ = New clsRatingMeasureData(srcR.NodeID, srcR.ParentNodeID, destUser.UserID, srcR.Rating, srcR.RatingScale, srcR.IsUndefined, srcR.Comment)
                                            Case ECMeasureType.mtStep
                                                Dim srcSF As clsStepMeasureData = CType(srcJ, clsStepMeasureData)
                                                destJ = New clsStepMeasureData(srcSF.NodeID, srcSF.ParentNodeID, destUser.UserID, srcSF.Value, srcSF.StepFunction, srcSF.IsUndefined, srcSF.Comment)
                                            Case ECMeasureType.mtDirect
                                                Dim srcDirect As clsDirectMeasureData = CType(srcJ, clsDirectMeasureData)
                                                destJ = New clsDirectMeasureData(srcDirect.NodeID, srcDirect.ParentNodeID, destUser.UserID, srcDirect.DirectData, srcDirect.IsUndefined, srcDirect.Comment)
                                            Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                                Dim srcUC As clsUtilityCurveMeasureData = CType(srcJ, clsUtilityCurveMeasureData)
                                                destJ = New clsUtilityCurveMeasureData(srcUC.NodeID, srcUC.ParentNodeID, destUser.UserID, srcUC.Data, srcUC.UtilityCurve, srcUC.IsUndefined, srcUC.Comment)
                                        End Select
                                        If destJ IsNot Nothing Then
                                            node.Judgments.AddMeasureData(destJ, FullRewrite)
                                        End If
                                    End If
                                Next

                                node.PWOutcomesJudgments.Weights.ClearUserWeights(destUser.UserID)
                                Dim pwoJudgments As List(Of clsCustomMeasureData) = node.PWOutcomesJudgments.JudgmentsFromUser(srcUser.UserID)
                                For Each srcJ As clsCustomMeasureData In pwoJudgments
                                    If Not srcJ.IsUndefined Then
                                        Dim srcPW As clsPairwiseMeasureData = CType(srcJ, clsPairwiseMeasureData)
                                        Dim destJ As New clsPairwiseMeasureData(srcPW.FirstNodeID, srcPW.SecondNodeID, srcPW.Advantage, srcPW.Value, srcPW.ParentNodeID, destUser.UserID, srcPW.IsUndefined, srcPW.Comment)
                                        destJ.OutcomesNodeID = srcPW.OutcomesNodeID
                                        node.PWOutcomesJudgments.AddMeasureData(destJ, FullRewrite)
                                    End If
                                Next
                            End If
                        End If
                    Next
                End If
            Next

            For Each alt As clsNode In AltsHierarchy(ActiveAltsHierarchy).Nodes
                alt.PWOutcomesJudgments.Weights.ClearUserWeights(destUser.UserID)
                Dim pwoJudgments As List(Of clsCustomMeasureData) = alt.PWOutcomesJudgments.JudgmentsFromUser(srcUser.UserID)
                For Each srcJ As clsCustomMeasureData In pwoJudgments
                    If Not srcJ.IsUndefined Then
                        Dim srcPW As clsPairwiseMeasureData = CType(srcJ, clsPairwiseMeasureData)
                        Dim destJ As New clsPairwiseMeasureData(srcPW.FirstNodeID, srcPW.SecondNodeID, srcPW.Advantage, srcPW.Value, srcPW.ParentNodeID, destUser.UserID, srcPW.IsUndefined, srcPW.Comment)
                        destJ.OutcomesNodeID = srcPW.OutcomesNodeID
                        alt.PWOutcomesJudgments.AddMeasureData(destJ, FullRewrite)
                    End If
                Next
            Next

            StorageManager.Writer.SaveUserJudgments(destUser, Now)
            destUser.LastJudgmentTime = VERY_OLD_DATE
            StorageManager.Reader.LoadUserJudgments(destUser)

            Return True

        End Function

        Public Function CopyJudgmentsFromClusterToCluster(fromNodeID As Guid, toNodeId As Guid, ByRef sMsg As String, Optional CopyForUserID As Integer = UNDEFINED_INTEGER_VALUE) As Boolean 'AS/11593 'AS/11593d made the 3rd paramater optional + A1282
            sMsg = "" 'A1282

            For Each H As clsHierarchy In Hierarchies
                Dim fromNode As clsNode = H.GetNodeByID(fromNodeID)
                Dim toNode As clsNode = H.GetNodeByID(toNodeId)
                If fromNode IsNot Nothing And toNode IsNot Nothing Then
                    'DeleteJudgmentsForNode(toNode.NodeGuidID, UserID) 'AS/11593f moved down

                    'make sure both clusters are PW and have the same number of elements
                    'LARER -- adjust the If to do check for all levels if decide to implement for all levels
                    'If fromNode.MeasureType <> ECMeasureType.mtPairwise And toNode.MeasureType <> ECMeasureType.mtPairwise Then 'AS/11593f===
                    '    'prompt user and exit
                    '    sMsg = "Measure type of at least one of the selected %%objectives%% is not pairwise. Cannot complete the operation." 'AS/11593e + A1282
                    '    Return False 'AS/11593f==
                    If fromNode.MeasureType <> ECMeasureType.mtPairwise Then 'AS/11593f===
                        'prompt user and exit
                        sMsg = "Measure type of the source %%objective%% is not pairwise. Cannot complete the operation." 'AS/11593e + A1282
                        Return False
                    ElseIf toNode.MeasureType <> ECMeasureType.mtPairwise Then
                        'prompt user and exit
                        sMsg = "Measure type of the destination %%objective%% is not pairwise. Cannot complete the operation." 'AS/11593e + A1282
                        Return False 'AS/11593f==
                    ElseIf fromNode.Children.Count <> toNode.Children.Count Then
                        'prompt user and exit
                        sMsg = "The selected nodes have different number of sub-nodes. Cannot complete the operation." 'AS/11593e + A1282
                        Return False
                    End If

                    If CopyForUserID = UNDEFINED_INTEGER_VALUE Then 'AS/11593d===
                        CopyForUserID = Me.User.UserID
                    End If 'AS/11593d==
                    DeleteJudgmentsForNode(toNode.NodeGuidID, CopyForUserID) 'AS/11593f moved down from above

                    Dim measureType As ECMeasureType = fromNode.MeasureType
                    For Each user As clsUser In UsersList
                        If user.UserID = CopyForUserID Then 'AS/11593d
                            StorageManager.Reader.LoadUserJudgments(user)

                            Dim fromJudgments As List(Of clsCustomMeasureData) = fromNode.Judgments.JudgmentsFromUser(user.UserID)
                            Dim toJudgments As List(Of clsCustomMeasureData) = toNode.Judgments.JudgmentsFromUser(user.UserID) 'AS/11593
                            Dim count As Integer = 0 'AS/11593

                            For Each fromJ As clsCustomMeasureData In fromJudgments
                                If Not fromJ.IsUndefined Then
                                    'Dim toJ As clsCustomMeasureData = Nothing 'AS/11593
                                    Dim toJ As clsCustomMeasureData = toJudgments(count) 'AS/11593

                                    Select Case toNode.MeasureType
                                        Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous
                                            Dim fromPW As clsPairwiseMeasureData = CType(fromJ, clsPairwiseMeasureData)
                                            Dim toPW As clsPairwiseMeasureData = CType(toJ, clsPairwiseMeasureData)
                                            'toJ = New clsPairwiseMeasureData(fromPW.FirstNodeID, fromPW.SecondNodeID, fromPW.Advantage, fromPW.Value, fromPW.ParentNodeID, user.UserID, fromPW.IsUndefined, fromPW.Comment) 'AS/11593
                                            toJ = New clsPairwiseMeasureData(toPW.FirstNodeID, toPW.SecondNodeID, fromPW.Advantage, fromPW.Value, toPW.ParentNodeID, user.UserID, fromPW.IsUndefined, fromPW.Comment) 'AS/11593
                                    End Select
                                    If toJ IsNot Nothing Then
                                        toNode.Judgments.AddMeasureData(toJ)
                                    End If
                                End If
                                count = count + 1 'AS/11593
                            Next
                            StorageManager.Writer.SaveUserJudgments(user, Now)
                            user.LastJudgmentTime = VERY_OLD_DATE
                        End If
                    Next
                End If
            Next

            Return True

        End Function

        Public Function CopyJudgmentsFromClusterToClusterEnhanced(fromNodeID As Guid, toNodeId As Guid, ByRef sMsg As String,
                                                                  Optional CopyMode As CopyJudgmentsMode = CopyJudgmentsMode.Replace,
                                                                      Optional CopyToUsers As List(Of clsUser) = Nothing) As Boolean 'AS/11593g
            '    sMsg = "" 'A1282

            '    For Each H As clsHierarchy In Hierarchies
            '        Dim fromNode As clsNode = H.GetNodeByID(fromNodeID)
            '        Dim toNode As clsNode = H.GetNodeByID(toNodeId)
            '        If fromNode IsNot Nothing And toNode IsNot Nothing Then
            '            If fromNode.MeasureType <> ECMeasureType.mtPairwise Then 'AS/11593f===
            '                'prompt user and exit
            '                sMsg = "Measure type of the source %%objective%% is not pairwise. Cannot complete the operation." 'AS/11593e + A1282
            '                Return False
            '            ElseIf toNode.MeasureType <> ECMeasureType.mtPairwise Then
            '                'prompt user and exit
            '                sMsg = "Measure type of the destination %%objective%% is not pairwise. Cannot complete the operation." 'AS/11593e + A1282
            '                Return False 'AS/11593f==
            '            ElseIf fromNode.Children.Count <> toNode.Children.Count Then
            '                'prompt user and exit
            '                sMsg = "The selected nodes have different number of sub-nodes. Cannot complete the operation." 'AS/11593e + A1282
            '                Return False
            '            End If

            '            Dim CopyToWhatUsers As CopyJudgmentsToWhatUser = CopyJudgmentsToWhatUser.LoggedInUser
            '            Dim CopyToUserID As Integer = UNDEFINED_INTEGER_VALUE 'copy to multiple selected or all users
            '            If CopyToUsers Is Nothing Then 'copy to logged in user
            '                CopyToUsers = New List(Of clsUser)
            '                CopyToUsers.Add(Me.User)
            '                CopyToUserID = Me.User.UserID
            '            ElseIf CopyToUsers.Count = 1 Then
            '                CopyToWhatUsers = CopyJudgmentsToWhatUser.SelectedSingleUser
            '                CopyToUserID = CopyToUsers(0).UserID 'copy to a single selected user
            '            ElseIf CopyToUsers.Count > 1 And CopyToUsers.Count < UsersList.Count Then
            '                CopyToWhatUsers = CopyJudgmentsToWhatUser.SelectedUsers
            '            Else
            '                CopyToWhatUsers = CopyJudgmentsToWhatUser.AllUsers
            '            End If

            '            Dim measureType As ECMeasureType = fromNode.MeasureType

            '            Select Case CopyMode
            '                Case CopyJudgmentsMode.Replace
            '                    DeleteJudgmentsForNode(toNode.NodeGuidID, CopyToUserID)

            '                    For Each user As clsUser In CopyToUsers
            '                        StorageManager.Reader.LoadUserJudgments(user)

            '                        Dim fromJudgments As List(Of clsCustomMeasureData) = fromNode.Judgments.JudgmentsFromUser(user.UserID)
            '                        For Each fromJ As clsCustomMeasureData In fromJudgments
            '                            If Not fromJ.IsUndefined Then
            '                                Dim toJ As clsCustomMeasureData = Nothing
            '                                Select Case toNode.MeasureType
            '                                    Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous
            '                                        Dim fromPW As clsPairwiseMeasureData = CType(fromJ, clsPairwiseMeasureData)
            '                                        toJ = New clsPairwiseMeasureData(fromPW.FirstNodeID, fromPW.SecondNodeID, fromPW.Advantage, fromPW.Value, fromPW.ParentNodeID, user.UserID, fromPW.IsUndefined, fromPW.Comment)
            '                                End Select
            '                                If toJ IsNot Nothing Then
            '                                    toNode.Judgments.AddMeasureData(toJ)
            '                                End If
            '                            End If
            '                        Next
            '                        StorageManager.Writer.SaveUserJudgments(user, Now)
            '                        user.LastJudgmentTime = VERY_OLD_DATE
            '                    Next

            '                Case CopyJudgmentsMode.AddMissing
            '                    For Each user As clsUser In CopyToUsers
            '                        StorageManager.Reader.LoadUserJudgments(user)

            '                        Dim CopyFromNodes As List(Of clsNode) = fromNode.GetNodesBelow(user.UserID)
            '                        Dim CopyToNodes As List(Of clsNode) = toNode.GetNodesBelow(user.UserID)
            '                        For Each nodeTo As clsNode In CopyToNodes
            '                            DeleteJudgmentsForNode(nodeTo.NodeGuidID, user.UserID)

            '                        Next

            '                    Next
            '                Case CopyJudgmentsMode.UpdateAndAddMissing

            '            End Select

            '        End If
            '    Next

            '    Return True

        End Function

        Public Function CopyJudgmentsFromNodeToNodes(fromNodeID As Guid, toNodeIds As List(Of Guid), ByRef sMsg As String, srcUserGuid As Guid,
                                                    Optional destUsersList As List(Of clsUser) = Nothing,
                                                    Optional CopyMode As CopyJudgmentsMode = CopyJudgmentsMode.Replace,
                                                    Optional PairwiseOnly As Boolean = False,
                                                    Optional ActiveHierarchyOnly As Boolean = True) As Boolean 'AS/16216a (CopyUserJudgments2 as a prototype)

            Dim srcUser As clsUser = GetUserByID(srcUserGuid)
            If srcUser Is Nothing Then srcUser = Me.User
            Dim destUser As clsUser = Nothing

            StorageManager.Reader.LoadUserJudgments(srcUser)

            If PairwiseOnly Then CopyMode = CopyJudgmentsMode.UpdateAndAddMissing

            For Each H As clsHierarchy In Hierarchies
                If Not ActiveHierarchyOnly Or ActiveHierarchyOnly And H.HierarchyID = ActiveHierarchy Then
                    Dim srcNode As clsNode = H.GetNodeByID(fromNodeID) 'AS/16216c 'AS/16216f
                    Dim SrcJudgments As List(Of clsCustomMeasureData) = srcNode.Judgments.JudgmentsFromUser(srcUser.UserID) 'AS/16216c 'AS/16216f
                    Dim copySuccess As Boolean = True 'AS/16216e

                    For Each destUser In destUsersList
                        If CopyMode = CopyJudgmentsMode.AddMissing Or CopyMode = CopyJudgmentsMode.UpdateAndAddMissing Then 'AS/16216f=== 'AS/16216g put back
                            StorageManager.Reader.LoadUserJudgments(destUser)
                        Else
                            'CleanUpUserDataFromMemory(destUser.UserID, False, True) 'AS/16216g
                            'DeleteJudgmentsForNode(node.NodeGuidID, destUser.UserID) 'AS/16216g
                        End If
                        For Each toNodeID As Guid In toNodeIds
                            'Dim toNode As clsNode = AltsHierarchy(ActiveAltsHierarchy).GetNodeByID(toNodeID)
                            For Each node As clsNode In H.Nodes
                                If node.NodeGuidID = toNodeID Then
                                    If Not PairwiseOnly Or (PairwiseOnly And (node.MeasureType = ECMeasureType.mtPairwise Or node.MeasureType = ECMeasureType.mtPWAnalogous Or node.MeasureType = ECMeasureType.mtPWOutcomes)) Then

                                        If CopyMode <> CopyJudgmentsMode.AddMissing AndAlso CopyMode <> CopyJudgmentsMode.UpdateAndAddMissing Then
                                            DeleteJudgmentsForNode(node.NodeGuidID, destUser.UserID)
                                        End If
                                        node.Judgments.Weights.ClearUserWeights(destUser.UserID) 'AS/16216f== 'AS/16216g== put back

                                        'Dim SrcJudgments As List(Of clsCustomMeasureData) = node.Judgments.JudgmentsFromUser(srcUser.UserID) 'AS/16216c
                                        'Dim srcNode As clsNode = H.GetNodeByID(fromNodeID) 'AS/16216c 'AS/16216f moved up
                                        'Dim SrcJudgments As List(Of clsCustomMeasureData) = srcNode.Judgments.JudgmentsFromUser(srcUser.UserID) 'AS/16216c 'AS/16216f moved up

                                        For i As Integer = 0 To SrcJudgments.Count - 1 'AS/16059a
                                            Dim doAdd As Boolean = True
                                            Dim srcJ As clsCustomMeasureData = SrcJudgments(i) 'AS/16059a
                                            If Not srcJ.IsUndefined Then
                                                Dim destJ As clsCustomMeasureData = Nothing
                                                Select Case node.MeasureType
                                                    Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous
                                                        If srcNode.MeasureType = node.MeasureType Then 'AS/16216i
                                                            Dim srcPW As clsPairwiseMeasureData = CType(srcJ, clsPairwiseMeasureData)
                                                            If CopyMode = CopyJudgmentsMode.AddMissing AndAlso CType(node.Judgments, clsPairwiseJudgments).PairwiseJudgment(srcPW.FirstNodeID, srcPW.SecondNodeID, destUser.UserID) IsNot Nothing Then
                                                                If CType(node.Judgments, clsPairwiseJudgments).PairwiseJudgment(srcPW.FirstNodeID, srcPW.SecondNodeID, destUser.UserID) Is Nothing Or CType(node.Judgments, clsPairwiseJudgments).PairwiseJudgment(srcPW.FirstNodeID, srcPW.SecondNodeID, destUser.UserID).Value = 0 Then 'AS/11725===
                                                                    destJ = New clsPairwiseMeasureData(srcPW.FirstNodeID, srcPW.SecondNodeID, srcPW.Advantage, srcPW.Value, srcPW.ParentNodeID, destUser.UserID, srcPW.IsUndefined, srcPW.Comment)
                                                                Else
                                                                    doAdd = False
                                                                End If 'AS/11725==
                                                            Else
                                                                destJ = New clsPairwiseMeasureData(srcPW.FirstNodeID, srcPW.SecondNodeID, srcPW.Advantage, srcPW.Value, srcPW.ParentNodeID, destUser.UserID, srcPW.IsUndefined, srcPW.Comment)
                                                            End If
                                                        Else 'AS/16216i===
                                                            copySuccess = False
                                                        End If'AS/16216i==

                                                    Case ECMeasureType.mtRatings
                                                        If srcNode.MeasureType = ECMeasureType.mtRatings Then 'AS/16216e enclosed
                                                            Dim srcR As clsRatingMeasureData = CType(srcJ, clsRatingMeasureData)
                                                            If CopyMode = CopyJudgmentsMode.AddMissing AndAlso CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(srcR.NodeID, srcR.ParentNodeID, srcR.UserID) IsNot Nothing Then
                                                                If CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(srcR.NodeID, srcR.ParentNodeID, destUser.UserID) Is Nothing Or CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(srcR.NodeID, srcR.ParentNodeID, destUser.UserID).IsUndefined Then 'AS/11725=== 'AS/11725a replaced .SingleValue = 0 with .IsUndefined
                                                                    destJ = New clsRatingMeasureData(srcR.NodeID, srcR.ParentNodeID, destUser.UserID, srcR.Rating, srcR.RatingScale, srcR.IsUndefined, srcR.Comment)
                                                                Else
                                                                    doAdd = False
                                                                End If 'AS/11725==
                                                            Else
                                                                destJ = New clsRatingMeasureData(srcR.NodeID, srcR.ParentNodeID, destUser.UserID, srcR.Rating, srcR.RatingScale, srcR.IsUndefined, srcR.Comment)
                                                            End If
                                                        ElseIf srcNode.MeasureType = ECMeasureType.mtDirect Then 'AS/16216e===
                                                            Dim srcDirect As clsDirectMeasureData = CType(srcJ, clsDirectMeasureData)
                                                            Dim R As New clsRating 'AS/16302
                                                            R.ID = -1
                                                            R.Name = "Direct Entry from " & srcNode.NodeName
                                                            R.Value = Single.Parse(srcDirect.SingleValue.ToString)
                                                            destJ = New clsRatingMeasureData(srcDirect.NodeID, srcDirect.ParentNodeID, destUser.UserID, R, Nothing)
                                                        Else 'AS/16216i===
                                                            copySuccess = False
                                                        End If'AS/16216i==

                                                    Case ECMeasureType.mtDirect
                                                        If srcNode.MeasureType = node.MeasureType Then 'AS/16216i
                                                            Dim srcDirect As clsDirectMeasureData = CType(srcJ, clsDirectMeasureData)
                                                            If CopyMode = CopyJudgmentsMode.AddMissing AndAlso CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(srcDirect.NodeID, srcDirect.ParentNodeID, srcDirect.UserID) IsNot Nothing Then
                                                                If CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(srcDirect.NodeID, srcDirect.ParentNodeID, destUser.UserID) Is Nothing Or CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(srcDirect.NodeID, srcDirect.ParentNodeID, destUser.UserID).IsUndefined Then 'AS/11725=== 'AS/11725a replaced .SingleValue = 0 with .IsUndefined
                                                                    destJ = New clsDirectMeasureData(srcDirect.NodeID, srcDirect.ParentNodeID, destUser.UserID, srcDirect.DirectData, srcDirect.IsUndefined, srcDirect.Comment)
                                                                Else
                                                                    doAdd = False
                                                                End If 'AS/11725==
                                                            Else
                                                                destJ = New clsDirectMeasureData(srcDirect.NodeID, srcDirect.ParentNodeID, destUser.UserID, srcDirect.DirectData, srcDirect.IsUndefined, srcDirect.Comment)
                                                            End If
                                                        Else 'AS/16216i===
                                                            copySuccess = False
                                                        End If'AS/16216i==

                                                    Case ECMeasureType.mtStep
                                                        If srcNode.MeasureType = node.MeasureType Then 'AS/16216i
                                                            Dim srcSF As clsStepMeasureData = CType(srcJ, clsStepMeasureData)
                                                            If CopyMode = CopyJudgmentsMode.AddMissing AndAlso CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(srcSF.NodeID, srcSF.ParentNodeID, srcSF.UserID) IsNot Nothing Then
                                                                If CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(srcSF.NodeID, srcSF.ParentNodeID, destUser.UserID) Is Nothing Or CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(srcSF.NodeID, srcSF.ParentNodeID, destUser.UserID).IsUndefined Then 'AS/11725=== 'AS/11725a replaced .SingleValue = 0 with .IsUndefined
                                                                    destJ = New clsStepMeasureData(srcSF.NodeID, srcSF.ParentNodeID, destUser.UserID, srcSF.Value, srcSF.StepFunction, srcSF.IsUndefined, srcSF.Comment)
                                                                Else
                                                                    doAdd = False
                                                                End If 'AS/11725==
                                                            Else
                                                                destJ = New clsStepMeasureData(srcSF.NodeID, srcSF.ParentNodeID, destUser.UserID, srcSF.Value, srcSF.StepFunction, srcSF.IsUndefined, srcSF.Comment)
                                                            End If
                                                        Else 'AS/16216i===
                                                            copySuccess = False
                                                        End If'AS/16216i==

                                                    Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                                        If srcNode.MeasureType = node.MeasureType Then 'AS/16216i
                                                            Dim srcUC As clsUtilityCurveMeasureData = CType(srcJ, clsUtilityCurveMeasureData)
                                                            If CopyMode = CopyJudgmentsMode.AddMissing AndAlso CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(srcUC.NodeID, srcUC.ParentNodeID, srcUC.UserID) IsNot Nothing Then
                                                                If CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(srcUC.NodeID, srcUC.ParentNodeID, destUser.UserID) Is Nothing Or CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(srcUC.NodeID, srcUC.ParentNodeID, destUser.UserID).IsUndefined Then 'AS/11725=== 'AS/11725a replaced .SingleValue = 0 with .IsUndefined
                                                                    destJ = New clsUtilityCurveMeasureData(srcUC.NodeID, srcUC.ParentNodeID, destUser.UserID, srcUC.Data, srcUC.UtilityCurve, srcUC.IsUndefined, srcUC.Comment)
                                                                Else
                                                                    doAdd = False
                                                                End If 'AS/11725==
                                                            Else
                                                                destJ = New clsUtilityCurveMeasureData(srcUC.NodeID, srcUC.ParentNodeID, destUser.UserID, srcUC.Data, srcUC.UtilityCurve, srcUC.IsUndefined, srcUC.Comment)
                                                            End If
                                                        Else 'AS/16216i===
                                                            copySuccess = False
                                                        End If 'AS/16216i==

                                                End Select
                                                If doAdd AndAlso destJ IsNot Nothing Then
                                                    'node.Judgments.AddMeasureData(destJ, CopyMode = CopyJudgmentsMode.Replace) 'AS/16059b
                                                    node.Judgments.AddMeasureData(destJ) 'AS/16059b
                                                    copySuccess = True 'AS/16216g

                                                End If
                                            End If
                                        Next

                                        node.PWOutcomesJudgments.Weights.ClearUserWeights(destUser.UserID)
                                        Dim pwoJudgments As List(Of clsCustomMeasureData) = node.PWOutcomesJudgments.JudgmentsFromUser(srcUser.UserID)
                                        For Each srcJ As clsCustomMeasureData In pwoJudgments
                                            If Not srcJ.IsUndefined Then
                                                Dim srcPW As clsPairwiseMeasureData = CType(srcJ, clsPairwiseMeasureData)
                                                If CopyMode <> CopyJudgmentsMode.AddMissing OrElse CopyMode = CopyJudgmentsMode.AddMissing AndAlso CType(node.PWOutcomesJudgments, clsPairwiseJudgments).PairwiseJudgment(srcPW.FirstNodeID, srcPW.SecondNodeID, destUser.UserID, srcPW.ParentNodeID, srcPW.OutcomesNodeID) Is Nothing Then
                                                    Dim destJ As New clsPairwiseMeasureData(srcPW.FirstNodeID, srcPW.SecondNodeID, srcPW.Advantage, srcPW.Value, srcPW.ParentNodeID, destUser.UserID, srcPW.IsUndefined, srcPW.Comment)
                                                    destJ = New clsPairwiseMeasureData(srcPW.FirstNodeID, srcPW.SecondNodeID, srcPW.Advantage, srcPW.Value, srcPW.ParentNodeID, destUser.UserID, srcPW.IsUndefined, srcPW.Comment)
                                                    destJ.OutcomesNodeID = srcPW.OutcomesNodeID
                                                    'node.PWOutcomesJudgments.AddMeasureData(destJ, CopyMode = CopyJudgmentsMode.Replace) 'AS/16059b
                                                    node.PWOutcomesJudgments.AddMeasureData(destJ) 'AS/16059b
                                                End If
                                            End If
                                        Next


                                        'Next
                                    End If
                                End If
                            Next
                            If Not copySuccess Then 'AS/16216e===
                                Return False
                            End If 'AS/16216e==
                        Next
                        StorageManager.Writer.SaveUserJudgments(destUser, Now) 'AS/16216b===
                        destUser.LastJudgmentTime = VERY_OLD_DATE
                        'StorageManager.Reader.LoadUserJudgments(destUser) 'AS/16216b==
                    Next
                End If
            Next

            'For Each alt As clsNode In AltsHierarchy(ActiveAltsHierarchy).Nodes 'AS/16216b===
            '    alt.PWOutcomesJudgments.Weights.ClearUserWeights(destUser.UserID)
            '    Dim pwoJudgments As List(Of clsCustomMeasureData) = alt.PWOutcomesJudgments.JudgmentsFromUser(srcUser.UserID)
            '    For Each srcJ As clsCustomMeasureData In pwoJudgments
            '        If Not srcJ.IsUndefined Then
            '            Dim srcPW As clsPairwiseMeasureData = CType(srcJ, clsPairwiseMeasureData)
            '            If CopyMode <> CopyJudgmentsMode.AddMissing OrElse CopyMode = CopyJudgmentsMode.AddMissing AndAlso CType(alt.PWOutcomesJudgments, clsPairwiseJudgments).PairwiseJudgment(srcPW.FirstNodeID, srcPW.SecondNodeID, destUser.UserID, srcPW.ParentNodeID, srcPW.OutcomesNodeID) Is Nothing Then
            '                Dim destJ As New clsPairwiseMeasureData(srcPW.FirstNodeID, srcPW.SecondNodeID, srcPW.Advantage, srcPW.Value, srcPW.ParentNodeID, destUser.UserID, srcPW.IsUndefined, srcPW.Comment)
            '                destJ.OutcomesNodeID = srcPW.OutcomesNodeID
            '                'alt.PWOutcomesJudgments.AddMeasureData(destJ, CopyMode = CopyJudgmentsMode.Replace) 'AS/16059b
            '                alt.PWOutcomesJudgments.AddMeasureData(destJ) 'AS/16059b
            '            End If
            '        End If
            '    Next
            'Next

            'StorageManager.Writer.SaveUserJudgments(destUser, Now)
            'destUser.LastJudgmentTime = VERY_OLD_DATE
            'StorageManager.Reader.LoadUserJudgments(destUser) 'AS/16216b==
            StorageManager.Reader.LoadUserJudgments(User) 'AS/16216b

            Return True
        End Function

        Public Function CopyUserJudgments(ByVal User1 As clsUser, ByVal User2 As clsUser) As Boolean 'C0133
            If User1 Is Nothing Or User2 Is Nothing Then
                Return False
            End If

            StorageManager.Writer.DeleteUserJudgments(User2)

            StorageManager.Reader.LoadUserJudgments(User1)

            For Each node As clsNode In Hierarchy(ActiveHierarchy).Nodes
                node.Judgments.DeleteJudgmentsFromUser(User2.UserID) 'C0269
                For Each J As clsCustomMeasureData In node.Judgments.JudgmentsFromUser(User1.UserID)
                    If Not J.IsUndefined Then
                        Select Case node.MeasureType
                            Case ECMeasureType.mtPairwise
                                Dim pwMeasureData As clsPairwiseMeasureData = CType(J, clsPairwiseMeasureData)
                                Dim newpwMD As clsPairwiseMeasureData = New clsPairwiseMeasureData(pwMeasureData.FirstNodeID, pwMeasureData.SecondNodeID, pwMeasureData.Advantage, pwMeasureData.Value, pwMeasureData.ParentNodeID, User2.UserID, False, pwMeasureData.Comment)
                                node.Judgments.AddMeasureData(newpwMD, True) 'C0269
                            Case ECMeasureType.mtRatings
                                Dim rData As clsRatingMeasureData = CType(J, clsRatingMeasureData)
                                Dim newrData As clsRatingMeasureData = New clsRatingMeasureData(rData.NodeID, rData.ParentNodeID, User2.UserID, rData.Rating, node.MeasurementScale, False, rData.Comment)
                                node.Judgments.AddMeasureData(newrData, True) 'C0269
                            Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                Dim ucData As clsUtilityCurveMeasureData = CType(J, clsUtilityCurveMeasureData)
                                Dim newucData As clsUtilityCurveMeasureData = New clsUtilityCurveMeasureData(ucData.NodeID, ucData.ParentNodeID, User2.UserID, ucData.Data, node.MeasurementScale, False, ucData.Comment)
                                node.Judgments.AddMeasureData(newucData, True) 'C0269
                            Case ECMeasureType.mtStep
                                Dim stepData As clsStepMeasureData = CType(J, clsStepMeasureData)
                                Dim newstepData As clsStepMeasureData = New clsStepMeasureData(stepData.NodeID, stepData.ParentNodeID, User2.UserID, stepData.Value, node.MeasurementScale, False, stepData.Comment) 'C0156
                                node.Judgments.AddMeasureData(newstepData, True) 'C0269
                            Case ECMeasureType.mtDirect
                                Dim directData As clsDirectMeasureData = CType(J, clsDirectMeasureData)
                                Dim newdirectData As clsDirectMeasureData = New clsDirectMeasureData(directData.NodeID, directData.ParentNodeID, User2.UserID, directData.DirectData, False, directData.Comment) 'C0181
                                node.Judgments.AddMeasureData(newdirectData, True) 'C0269
                        End Select
                    End If
                Next
            Next

            'C0450===
            ' in streams approach we don't have to save each judgment, because all judgments for user are being saved once
            ' so, we just create a dummy measure data object and just put there correct UserID, and then call the function
            If (StorageManager.StorageType = ECModelStorageType.mstCanvasStreamDatabase) Then
                StorageManager.Writer.SaveUserJudgments(User2)
            End If
            'C0450==

            Return True
        End Function


        'Public Function CopyJudgmentsFromAlternativeToAlternative(fromAltID As Guid, toAltID As Guid, ByRef sMsg As String, Optional FullRewrite As Boolean = True, Optional PairwiseOnly As Boolean = False, Optional ActiveHierarchyOnly As Boolean = True) As Boolean 'AS/11642d + A1282 'AS/11642e
        Public Function CopyJudgmentsFromAlternativeToAlternatives(fromAltID As Guid, toAltIDs As List(Of Guid), ByRef sMsg As String, srcUserGuid As Guid,
                                            Optional destUsersList As List(Of clsUser) = Nothing,
                                            Optional CopyMode As CopyJudgmentsMode = CopyJudgmentsMode.Replace,
                                            Optional PairwiseOnly As Boolean = False,
                                            Optional ActiveHierarchyOnly As Boolean = True) As Boolean 'AS/11642e

            Dim srcUser As clsUser = GetUserByID(srcUserGuid) 'AS/11642e===
            If srcUser Is Nothing Then srcUser = Me.User
            Dim destUser As clsUser = Nothing

            StorageManager.Reader.LoadUserJudgments(srcUser) 'AS/11642e==

            sMsg = "" 'A1282
            'take all judgments for one alternative from selected participant and copy them to one or more other alternatives to the selected participants.

            Dim fromAlt As clsNode = AltsHierarchy(ActiveAltsHierarchy).GetNodeByID(fromAltID)            

            If fromAlt Is Nothing Or toAltIDs.Count = 0 Then Return False

            ''make sure the alternatives have identical settings for everything 'AS/11642b=== 'AS/11642e=== commented out 'AS/11642f deleted the piece entirely

            For Each destUser In destUsersList

                If srcUser Is Nothing Or destUser Is Nothing Then Return False

                StorageManager.Reader.LoadUserJudgments(srcUser) 'AS/11642f moved down 'AS/11642g put back

                If CopyMode = CopyJudgmentsMode.AddMissing Or CopyMode = CopyJudgmentsMode.UpdateAndAddMissing Then
                    StorageManager.Reader.LoadUserJudgments(destUser)
                Else
                    ' replace
                    'For Each H As clsHierarchy In Hierarchies 'AS/11642f===
                    '    If Not ActiveHierarchyOnly Or ActiveHierarchyOnly And H.HierarchyID = ActiveHierarchy Then
                    '        CleanUpUserDataFromMemory(H.HierarchyID, destUser.UserID, False, True)
                    '    End If
                    'Next 'AS/11642f===
                    For each toAltID As Guid In toAltIDs 
                        Dim toAlt As clsNode = AltsHierarchy(ActiveAltsHierarchy).GetNodeByID(toAltID)
                        DeleteJudgmentsForAlternative(toAlt.NodeID, destUser.UserID) 'AS/11642f
                    Next
                End If
                'StorageManager.Reader.LoadUserJudgments(srcUser) 'AS/11642f moved down

                For each toAltID As Guid In toAltIDs 
                    Dim toAlt As clsNode = AltsHierarchy(ActiveAltsHierarchy).GetNodeByID(toAltID)
                    For Each node As clsNode In Hierarchy(ActiveHierarchy).TerminalNodes
                        If Not IsPWMeasurementType(node.MeasureType) Then
                            For Each srcJ As clsCustomMeasureData In node.Judgments.JudgmentsFromUser(srcUser.UserID)
                                Dim doAdd As Boolean = True
                                If srcJ IsNot Nothing AndAlso Not srcJ.IsUndefined Then
                                    Dim destJ As clsCustomMeasureData = Nothing
                                    Select Case node.MeasureType
                                        Case ECMeasureType.mtRatings
                                            Dim srcR As clsRatingMeasureData = CType(srcJ, clsRatingMeasureData)
                                            Dim destR As clsRatingMeasureData = CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(srcR.NodeID, srcR.ParentNodeID, destUser.UserID)
                                            If srcR.NodeID = fromAlt.NodeID Then 'AS/11642g
                                                Select Case CopyMode
                                                    Case CopyJudgmentsMode.Replace, CopyJudgmentsMode.UpdateAndAddMissing
                                                        destJ = New clsRatingMeasureData(toAlt.NodeID, node.NodeID, destUser.UserID, srcR.Rating, srcR.RatingScale, srcR.IsUndefined, srcR.Comment)
                                                    Case CopyJudgmentsMode.AddMissing
                                                        If destR Is Nothing OrElse destR.IsUndefined Then
                                                            destJ = New clsRatingMeasureData(toAlt.NodeID, node.NodeID, destUser.UserID, srcR.Rating, srcR.RatingScale, srcR.IsUndefined, srcR.Comment) 'AS/11642g
                                                        End If
                                                End Select
                                                'Debug.Print("FROM: " & fromAlt.NodeName & ",  " & node.NodeName & ",  " & srcUser.UserName) 'AS/11642g
                                                'Debug.Print("TO:   " & toAlt.NodeName & ",  " & node.NodeName & ",  " & destUser.UserName & ",  " & srcR.Rating.Name & ",  " & srcR.RatingScale.Name & ",  " & srcR.IsUndefined.ToString) 'AS/11642g
                                            End If'AS/11642g

                                        Case ECMeasureType.mtStep
                                            Dim srcSF As clsStepMeasureData = CType(srcJ, clsStepMeasureData)
                                            Dim destSF As clsStepMeasureData = CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(srcSF.NodeID, srcSF.ParentNodeID, destUser.UserID)
                                            If srcSF.NodeID = fromAlt.NodeID Then 'AS/11642g
                                                Select Case CopyMode
                                                    Case CopyJudgmentsMode.Replace, CopyJudgmentsMode.UpdateAndAddMissing
                                                        destJ = New clsStepMeasureData(toAlt.NodeID, node.NodeID, destUser.UserID, srcSF.Value, srcSF.StepFunction, srcSF.IsUndefined, srcSF.Comment)
                                                    Case CopyJudgmentsMode.AddMissing
                                                        If destSF Is Nothing OrElse destSF.IsUndefined Then
                                                            destJ = New clsStepMeasureData(toAlt.NodeID, node.NodeID, destUser.UserID, srcSF.Value, srcSF.StepFunction, srcSF.IsUndefined, srcSF.Comment) 'AS/11642g
                                                        End If
                                                End Select
                                            End If'AS/11642g

                                        Case ECMeasureType.mtDirect
                                            Dim srcDirect As clsDirectMeasureData = CType(srcJ, clsDirectMeasureData)
                                            Dim destDirect As clsDirectMeasureData = CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(srcDirect.NodeID, srcDirect.ParentNodeID, destUser.UserID)
                                            If srcDirect.NodeID = fromAlt.NodeID Then 'AS/11642g
                                                Select Case CopyMode
                                                    Case CopyJudgmentsMode.Replace, CopyJudgmentsMode.UpdateAndAddMissing
                                                        destJ = New clsDirectMeasureData(toAlt.NodeID, node.NodeID, destUser.UserID, srcDirect.DirectData, srcDirect.IsUndefined, srcDirect.Comment)
                                                    Case CopyJudgmentsMode.AddMissing
                                                        If destDirect Is Nothing OrElse destDirect.IsUndefined Then
                                                            destJ = New clsDirectMeasureData(toAlt.NodeID, node.NodeID, destUser.UserID, srcDirect.DirectData, srcDirect.IsUndefined, srcDirect.Comment) 'AS/11642g
                                                        End If
                                                End Select
                                            End If 'AS/11642g

                                        Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                            Dim srcUC As clsUtilityCurveMeasureData = CType(srcJ, clsUtilityCurveMeasureData)
                                            Dim destUC As clsUtilityCurveMeasureData = CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(srcUC.NodeID, srcUC.ParentNodeID, destUser.UserID)
                                            If srcUC.NodeID = fromAlt.NodeID Then 'AS/11642g
                                                Select Case CopyMode
                                                    Case CopyJudgmentsMode.Replace, CopyJudgmentsMode.UpdateAndAddMissing
                                                        destJ = New clsUtilityCurveMeasureData(toAlt.NodeID, node.NodeID, destUser.UserID, srcUC.Data, srcUC.UtilityCurve, srcUC.IsUndefined, srcUC.Comment)
                                                    Case CopyJudgmentsMode.AddMissing
                                                        If destUC Is Nothing OrElse destUC.IsUndefined Then
                                                            destJ = New clsUtilityCurveMeasureData(toAlt.NodeID, node.NodeID, destUser.UserID, srcUC.Data, srcUC.UtilityCurve, srcUC.IsUndefined, srcUC.Comment) 'AS/11642g
                                                        End If
                                                End Select
                                            End If 'AS/11642g

                                    End Select
                                    If destJ IsNot Nothing Then
                                        node.Judgments.AddMeasureData(destJ, False)
                                    End If
                                End If
                            Next
                        End If
                    Next
                Next
                'Next 'AS/11642f 
                StorageManager.Writer.SaveUserJudgments(destUser, Now)
            Next 'AS/11642f

            'If destUser IsNot User Then 'AS/11642f===
            '    For Each H As clsHierarchy In Hierarchies
            '        If Not ActiveHierarchyOnly Or ActiveHierarchyOnly And H.HierarchyID = ActiveHierarchy Then
            '            CleanUpUserDataFromMemory(H.HierarchyID, destUser.UserID, False, True)
            '        End If
            '    Next
            'Else
            '    PipeBuilder.PipeCreated = False
            'End If 'AS/11642f==

            Return True
        End Function

        Public Function CopyUserJudgments(SourceUserEmail As String, DestUserEmail As String, Optional FullRewrite As Boolean = True, Optional PairwiseOnly As Boolean = False, Optional ActiveHierarchyOnly As Boolean = True) As Boolean
            Dim SrcUser As clsUser = GetUserByEMail(SourceUserEmail)
            Dim DestUser As clsUser = GetUserByEMail(DestUserEmail)

            If SrcUser Is Nothing Or DestUser Is Nothing Then Return False

            StorageManager.Reader.LoadUserJudgments(SrcUser)

            If PairwiseOnly Then FullRewrite = False

            If Not FullRewrite Then
                StorageManager.Reader.LoadUserJudgments(DestUser)
            Else
                CleanUpUserDataFromMemory(DestUser.UserID, False, True)
            End If

            For Each H As clsHierarchy In Hierarchies
                If Not ActiveHierarchyOnly Or ActiveHierarchyOnly And H.HierarchyID = ActiveHierarchy Then
                    For Each node As clsNode In H.Nodes
                        If Not PairwiseOnly Or (PairwiseOnly And (node.MeasureType = ECMeasureType.mtPairwise Or node.MeasureType = ECMeasureType.mtPWAnalogous Or node.MeasureType = ECMeasureType.mtPWOutcomes)) Then
                            node.Judgments.Weights.ClearUserWeights(DestUser.UserID)

                            Dim SrcJudgments As List(Of clsCustomMeasureData) = node.Judgments.JudgmentsFromUser(SrcUser.UserID)
                            For Each srcJ As clsCustomMeasureData In SrcJudgments
                                If Not srcJ.IsUndefined Then
                                    Dim destJ As clsCustomMeasureData = Nothing
                                    Select Case node.MeasureType
                                        Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous
                                            Dim srcPW As clsPairwiseMeasureData = CType(srcJ, clsPairwiseMeasureData)
                                            destJ = New clsPairwiseMeasureData(srcPW.FirstNodeID, srcPW.SecondNodeID, srcPW.Advantage, srcPW.Value, srcPW.ParentNodeID, DestUser.UserID, srcPW.IsUndefined, srcPW.Comment)
                                        Case ECMeasureType.mtRatings
                                            Dim srcR As clsRatingMeasureData = CType(srcJ, clsRatingMeasureData)
                                            destJ = New clsRatingMeasureData(srcR.NodeID, srcR.ParentNodeID, DestUser.UserID, srcR.Rating, srcR.RatingScale, srcR.IsUndefined, srcR.Comment)
                                        Case ECMeasureType.mtStep
                                            Dim srcSF As clsStepMeasureData = CType(srcJ, clsStepMeasureData)
                                            destJ = New clsStepMeasureData(srcSF.NodeID, srcSF.ParentNodeID, DestUser.UserID, srcSF.Value, srcSF.StepFunction, srcSF.IsUndefined, srcSF.Comment)
                                        Case ECMeasureType.mtDirect
                                            Dim srcDirect As clsDirectMeasureData = CType(srcJ, clsDirectMeasureData)
                                            destJ = New clsDirectMeasureData(srcDirect.NodeID, srcDirect.ParentNodeID, DestUser.UserID, srcDirect.DirectData, srcDirect.IsUndefined, srcDirect.Comment)
                                        Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                            Dim srcUC As clsUtilityCurveMeasureData = CType(srcJ, clsUtilityCurveMeasureData)
                                            destJ = New clsUtilityCurveMeasureData(srcUC.NodeID, srcUC.ParentNodeID, DestUser.UserID, srcUC.Data, srcUC.UtilityCurve, srcUC.IsUndefined, srcUC.Comment)
                                    End Select
                                    If destJ IsNot Nothing Then
                                        node.Judgments.AddMeasureData(destJ, FullRewrite)
                                    End If
                                End If
                            Next

                            node.PWOutcomesJudgments.Weights.ClearUserWeights(DestUser.UserID)
                            Dim pwoJudgments As List(Of clsCustomMeasureData) = node.PWOutcomesJudgments.JudgmentsFromUser(SrcUser.UserID)
                            For Each srcJ As clsCustomMeasureData In pwoJudgments
                                If Not srcJ.IsUndefined Then
                                    Dim srcPW As clsPairwiseMeasureData = CType(srcJ, clsPairwiseMeasureData)
                                    Dim destJ As New clsPairwiseMeasureData(srcPW.FirstNodeID, srcPW.SecondNodeID, srcPW.Advantage, srcPW.Value, srcPW.ParentNodeID, DestUser.UserID, srcPW.IsUndefined, srcPW.Comment)
                                    destJ.OutcomesNodeID = srcPW.OutcomesNodeID
                                    node.PWOutcomesJudgments.AddMeasureData(destJ, FullRewrite)
                                End If
                            Next
                        End If
                    Next
                End If
            Next

            For Each alt As clsNode In AltsHierarchy(ActiveAltsHierarchy).Nodes
                alt.PWOutcomesJudgments.Weights.ClearUserWeights(DestUser.UserID)
                Dim pwoJudgments As List(Of clsCustomMeasureData) = alt.PWOutcomesJudgments.JudgmentsFromUser(SrcUser.UserID)
                For Each srcJ As clsCustomMeasureData In pwoJudgments
                    If Not srcJ.IsUndefined Then
                        Dim srcPW As clsPairwiseMeasureData = CType(srcJ, clsPairwiseMeasureData)
                        Dim destJ As New clsPairwiseMeasureData(srcPW.FirstNodeID, srcPW.SecondNodeID, srcPW.Advantage, srcPW.Value, srcPW.ParentNodeID, DestUser.UserID, srcPW.IsUndefined, srcPW.Comment)
                        destJ.OutcomesNodeID = srcPW.OutcomesNodeID
                        alt.PWOutcomesJudgments.AddMeasureData(destJ, FullRewrite)
                    End If
                Next
            Next

            StorageManager.Writer.SaveUserJudgments(DestUser, Now)
            DestUser.LastJudgmentTime = VERY_OLD_DATE
            StorageManager.Reader.LoadUserJudgments(DestUser)

            Return True
        End Function

        Public Function CopyUserJudgments2(SourceUserEmail As String, DestUserEmail As String, Optional CopyMode As CopyJudgmentsMode = CopyJudgmentsMode.Replace, Optional PairwiseOnly As Boolean = False, Optional ActiveHierarchyOnly As Boolean = True) As Boolean 'A1226 - renamed the function because of constant "Overload resolution failed" error
            Dim SrcUser As clsUser = GetUserByEMail(SourceUserEmail)
            Dim DestUser As clsUser = GetUserByEMail(DestUserEmail)

            If SrcUser Is Nothing Or DestUser Is Nothing Then Return False

            StorageManager.Reader.LoadUserJudgments(SrcUser)

            If CopyMode = CopyJudgmentsMode.AddMissing Or CopyMode = CopyJudgmentsMode.UpdateAndAddMissing Then
                StorageManager.Reader.LoadUserJudgments(DestUser)
            Else
                ' replace
                For Each H As clsHierarchy In Hierarchies
                    If Not ActiveHierarchyOnly Or ActiveHierarchyOnly And H.HierarchyID = ActiveHierarchy Then
                        CleanUpUserDataFromMemory(H.HierarchyID, DestUser.UserID, False, True)
                    End If
                Next
            End If

            For Each H As clsHierarchy In Hierarchies
                If Not ActiveHierarchyOnly Or ActiveHierarchyOnly And H.HierarchyID = ActiveHierarchy Then

                    For Each node As clsNode In H.Nodes
                        If Not PairwiseOnly Or (PairwiseOnly AndAlso (node.MeasureType = ECMeasureType.mtPairwise OrElse node.MeasureType = ECMeasureType.mtPWAnalogous OrElse node.MeasureType = ECMeasureType.mtPWOutcomes)) Then
                            node.Judgments.Weights.ClearUserWeights(DestUser.UserID)
                            For Each srcJ As clsCustomMeasureData In node.Judgments.JudgmentsFromUser(SrcUser.UserID)
                                Dim doAdd As Boolean = True
                                If Not srcJ.IsUndefined Then
                                    Dim destJ As clsCustomMeasureData = Nothing
                                    Select Case node.MeasureType
                                        Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous
                                            Dim srcPW As clsPairwiseMeasureData = CType(srcJ, clsPairwiseMeasureData)
                                            Dim destPW As clsPairwiseMeasureData = CType(node.Judgments, clsPairwiseJudgments).PairwiseJudgment(srcPW.FirstNodeID, srcPW.SecondNodeID, DestUser.UserID)

                                            Select Case CopyMode
                                                Case CopyJudgmentsMode.Replace, CopyJudgmentsMode.UpdateAndAddMissing
                                                    destJ = New clsPairwiseMeasureData(srcPW.FirstNodeID, srcPW.SecondNodeID, srcPW.Advantage, srcPW.Value, srcPW.ParentNodeID, DestUser.UserID, srcPW.IsUndefined, srcPW.Comment)
                                                Case CopyJudgmentsMode.AddMissing
                                                    If destPW Is Nothing OrElse destPW.IsUndefined Then
                                                        destJ = New clsPairwiseMeasureData(srcPW.FirstNodeID, srcPW.SecondNodeID, srcPW.Advantage, srcPW.Value, srcPW.ParentNodeID, DestUser.UserID, srcPW.IsUndefined, srcPW.Comment)
                                                    End If
                                            End Select
                                        Case ECMeasureType.mtRatings
                                            Dim srcR As clsRatingMeasureData = CType(srcJ, clsRatingMeasureData)
                                            Dim destR As clsRatingMeasureData = CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(srcR.NodeID, srcR.ParentNodeID, DestUser.UserID)
                                            Select Case CopyMode
                                                Case CopyJudgmentsMode.Replace, CopyJudgmentsMode.UpdateAndAddMissing
                                                    destJ = New clsRatingMeasureData(srcR.NodeID, srcR.ParentNodeID, DestUser.UserID, srcR.Rating, srcR.RatingScale, srcR.IsUndefined, srcR.Comment)
                                                Case CopyJudgmentsMode.AddMissing
                                                    If destR Is Nothing OrElse destR.IsUndefined Then
                                                        destJ = New clsRatingMeasureData(srcR.NodeID, srcR.ParentNodeID, DestUser.UserID, srcR.Rating, srcR.RatingScale, srcR.IsUndefined, srcR.Comment)
                                                    End If
                                            End Select
                                        Case ECMeasureType.mtStep
                                            Dim srcSF As clsStepMeasureData = CType(srcJ, clsStepMeasureData)
                                            Dim destSF As clsStepMeasureData = CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(srcSF.NodeID, srcSF.ParentNodeID, DestUser.UserID)
                                            Select Case CopyMode
                                                Case CopyJudgmentsMode.Replace, CopyJudgmentsMode.UpdateAndAddMissing
                                                    destJ = New clsStepMeasureData(srcSF.NodeID, srcSF.ParentNodeID, DestUser.UserID, srcSF.Value, srcSF.StepFunction, srcSF.IsUndefined, srcSF.Comment)
                                                Case CopyJudgmentsMode.AddMissing
                                                    If destSF Is Nothing OrElse destSF.IsUndefined Then
                                                        destJ = New clsStepMeasureData(srcSF.NodeID, srcSF.ParentNodeID, DestUser.UserID, srcSF.Value, srcSF.StepFunction, srcSF.IsUndefined, srcSF.Comment)
                                                    End If
                                            End Select
                                        Case ECMeasureType.mtDirect
                                            Dim srcDirect As clsDirectMeasureData = CType(srcJ, clsDirectMeasureData)
                                            Dim destDirect As clsDirectMeasureData = CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(srcDirect.NodeID, srcDirect.ParentNodeID, DestUser.UserID)
                                            Select Case CopyMode
                                                Case CopyJudgmentsMode.Replace, CopyJudgmentsMode.UpdateAndAddMissing
                                                    destJ = New clsDirectMeasureData(srcDirect.NodeID, srcDirect.ParentNodeID, DestUser.UserID, srcDirect.DirectData, srcDirect.IsUndefined, srcDirect.Comment)
                                                Case CopyJudgmentsMode.AddMissing
                                                    If destDirect Is Nothing OrElse destDirect.IsUndefined Then
                                                        destJ = New clsDirectMeasureData(srcDirect.NodeID, srcDirect.ParentNodeID, DestUser.UserID, srcDirect.DirectData, srcDirect.IsUndefined, srcDirect.Comment)
                                                    End If
                                            End Select
                                        Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                            Dim srcUC As clsUtilityCurveMeasureData = CType(srcJ, clsUtilityCurveMeasureData)
                                            Dim destUC As clsUtilityCurveMeasureData = CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(srcUC.NodeID, srcUC.ParentNodeID, DestUser.UserID)
                                            Select Case CopyMode
                                                Case CopyJudgmentsMode.Replace, CopyJudgmentsMode.UpdateAndAddMissing
                                                    destJ = New clsUtilityCurveMeasureData(srcUC.NodeID, srcUC.ParentNodeID, DestUser.UserID, srcUC.Data, srcUC.UtilityCurve, srcUC.IsUndefined, srcUC.Comment)
                                                Case CopyJudgmentsMode.AddMissing
                                                    If destUC Is Nothing OrElse destUC.IsUndefined Then
                                                        destJ = New clsUtilityCurveMeasureData(srcUC.NodeID, srcUC.ParentNodeID, DestUser.UserID, srcUC.Data, srcUC.UtilityCurve, srcUC.IsUndefined, srcUC.Comment)
                                                    End If
                                            End Select
                                    End Select
                                    If destJ IsNot Nothing Then
                                        node.Judgments.AddMeasureData(destJ, False)
                                    End If
                                End If
                            Next

                            If IsRiskProject AndAlso node.MeasureType = ECMeasureType.mtPWOutcomes Then
                                node.PWOutcomesJudgments.Weights.ClearUserWeights(DestUser.UserID)
                                For Each srcPW As clsPairwiseMeasureData In node.PWOutcomesJudgments.JudgmentsFromUser(SrcUser.UserID)
                                    If Not srcPW.IsUndefined Then
                                        Dim destJ As clsPairwiseMeasureData = Nothing
                                        Dim destPW As clsPairwiseMeasureData = CType(node.PWOutcomesJudgments, clsPairwiseJudgments).PairwiseJudgment(srcPW.FirstNodeID, srcPW.SecondNodeID, DestUser.UserID)

                                        Select Case CopyMode
                                            Case CopyJudgmentsMode.Replace, CopyJudgmentsMode.UpdateAndAddMissing
                                                destJ = New clsPairwiseMeasureData(srcPW.FirstNodeID, srcPW.SecondNodeID, srcPW.Advantage, srcPW.Value, srcPW.ParentNodeID, DestUser.UserID, srcPW.IsUndefined, srcPW.Comment)
                                                destJ.OutcomesNodeID = srcPW.OutcomesNodeID
                                            Case CopyJudgmentsMode.AddMissing
                                                If destPW Is Nothing OrElse destPW.IsUndefined Then
                                                    destJ = New clsPairwiseMeasureData(srcPW.FirstNodeID, srcPW.SecondNodeID, srcPW.Advantage, srcPW.Value, srcPW.ParentNodeID, DestUser.UserID, srcPW.IsUndefined, srcPW.Comment)
                                                    destJ.OutcomesNodeID = srcPW.OutcomesNodeID
                                                End If
                                        End Select
                                        If destJ IsNot Nothing Then
                                            node.PWOutcomesJudgments.AddMeasureData(destJ, False)
                                        End If
                                    End If
                                Next
                            End If
                        End If
                    Next
                End If
            Next

            If IsRiskProject Then
                For Each alt As clsNode In ActiveAlternatives.Nodes
                    If alt.MeasureType = ECMeasureType.mtPWOutcomes Then
                        alt.PWOutcomesJudgments.Weights.ClearUserWeights(DestUser.UserID)
                        For Each srcPW As clsPairwiseMeasureData In alt.PWOutcomesJudgments.JudgmentsFromUser(SrcUser.UserID)
                            If Not srcPW.IsUndefined Then
                                Dim destJ As clsPairwiseMeasureData = Nothing
                                Dim destPW As clsPairwiseMeasureData = CType(alt.PWOutcomesJudgments, clsPairwiseJudgments).PairwiseJudgment(srcPW.FirstNodeID, srcPW.SecondNodeID, DestUser.UserID)

                                Select Case CopyMode
                                    Case CopyJudgmentsMode.Replace, CopyJudgmentsMode.UpdateAndAddMissing
                                        destJ = New clsPairwiseMeasureData(srcPW.FirstNodeID, srcPW.SecondNodeID, srcPW.Advantage, srcPW.Value, srcPW.ParentNodeID, DestUser.UserID, srcPW.IsUndefined, srcPW.Comment)
                                        destJ.OutcomesNodeID = srcPW.OutcomesNodeID
                                    Case CopyJudgmentsMode.AddMissing
                                        If destPW Is Nothing OrElse destPW.IsUndefined Then
                                            destJ = New clsPairwiseMeasureData(srcPW.FirstNodeID, srcPW.SecondNodeID, srcPW.Advantage, srcPW.Value, srcPW.ParentNodeID, DestUser.UserID, srcPW.IsUndefined, srcPW.Comment)
                                            destJ.OutcomesNodeID = srcPW.OutcomesNodeID
                                        End If
                                End Select
                                If destJ IsNot Nothing Then
                                    alt.PWOutcomesJudgments.AddMeasureData(destJ, False)
                                End If
                            End If
                        Next
                    Else
                        alt.DirectJudgmentsForNoCause.Weights.ClearUserWeights(DestUser.UserID)
                        For Each srcNonPW As clsNonPairwiseMeasureData In alt.DirectJudgmentsForNoCause.JudgmentsFromUser(SrcUser.UserID)
                            If Not srcNonPW.IsUndefined Then
                                Dim destJ As clsNonPairwiseMeasureData = Nothing

                                Select Case alt.MeasureType
                                    Case ECMeasureType.mtRatings
                                        Dim srcR As clsRatingMeasureData = CType(srcNonPW, clsRatingMeasureData)
                                        Dim destR As clsRatingMeasureData = CType(alt.DirectJudgmentsForNoCause, clsNonPairwiseJudgments).GetJudgement(srcR.NodeID, srcR.ParentNodeID, DestUser.UserID)
                                        Select Case CopyMode
                                            Case CopyJudgmentsMode.Replace, CopyJudgmentsMode.UpdateAndAddMissing
                                                destJ = New clsRatingMeasureData(srcR.NodeID, srcR.ParentNodeID, DestUser.UserID, srcR.Rating, srcR.RatingScale, srcR.IsUndefined, srcR.Comment)
                                            Case CopyJudgmentsMode.AddMissing
                                                If destR Is Nothing OrElse destR.IsUndefined Then
                                                    destJ = New clsRatingMeasureData(srcR.NodeID, srcR.ParentNodeID, DestUser.UserID, srcR.Rating, srcR.RatingScale, srcR.IsUndefined, srcR.Comment)
                                                End If
                                        End Select
                                    Case ECMeasureType.mtStep
                                        Dim srcSF As clsStepMeasureData = CType(srcNonPW, clsStepMeasureData)
                                        Dim destSF As clsStepMeasureData = CType(alt.DirectJudgmentsForNoCause, clsNonPairwiseJudgments).GetJudgement(srcSF.NodeID, srcSF.ParentNodeID, DestUser.UserID)
                                        Select Case CopyMode
                                            Case CopyJudgmentsMode.Replace, CopyJudgmentsMode.UpdateAndAddMissing
                                                destJ = New clsStepMeasureData(srcSF.NodeID, srcSF.ParentNodeID, DestUser.UserID, srcSF.Value, srcSF.StepFunction, srcSF.IsUndefined, srcSF.Comment)
                                            Case CopyJudgmentsMode.AddMissing
                                                If destSF Is Nothing OrElse destSF.IsUndefined Then
                                                    destJ = New clsStepMeasureData(srcSF.NodeID, srcSF.ParentNodeID, DestUser.UserID, srcSF.Value, srcSF.StepFunction, srcSF.IsUndefined, srcSF.Comment)
                                                End If
                                        End Select
                                    Case ECMeasureType.mtDirect
                                        Dim srcDirect As clsDirectMeasureData = CType(srcNonPW, clsDirectMeasureData)
                                        Dim destDirect As clsDirectMeasureData = CType(alt.DirectJudgmentsForNoCause, clsNonPairwiseJudgments).GetJudgement(srcDirect.NodeID, srcDirect.ParentNodeID, DestUser.UserID)
                                        Select Case CopyMode
                                            Case CopyJudgmentsMode.Replace, CopyJudgmentsMode.UpdateAndAddMissing
                                                destJ = New clsDirectMeasureData(srcDirect.NodeID, srcDirect.ParentNodeID, DestUser.UserID, srcDirect.DirectData, srcDirect.IsUndefined, srcDirect.Comment)
                                            Case CopyJudgmentsMode.AddMissing
                                                If destDirect Is Nothing OrElse destDirect.IsUndefined Then
                                                    destJ = New clsDirectMeasureData(srcDirect.NodeID, srcDirect.ParentNodeID, DestUser.UserID, srcDirect.DirectData, srcDirect.IsUndefined, srcDirect.Comment)
                                                End If
                                        End Select
                                    Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                        Dim srcUC As clsUtilityCurveMeasureData = CType(srcNonPW, clsUtilityCurveMeasureData)
                                        Dim destUC As clsUtilityCurveMeasureData = CType(alt.DirectJudgmentsForNoCause, clsNonPairwiseJudgments).GetJudgement(srcUC.NodeID, srcUC.ParentNodeID, DestUser.UserID)
                                        Select Case CopyMode
                                            Case CopyJudgmentsMode.Replace, CopyJudgmentsMode.UpdateAndAddMissing
                                                destJ = New clsUtilityCurveMeasureData(srcUC.NodeID, srcUC.ParentNodeID, DestUser.UserID, srcUC.Data, srcUC.UtilityCurve, srcUC.IsUndefined, srcUC.Comment)
                                            Case CopyJudgmentsMode.AddMissing
                                                If destUC Is Nothing OrElse destUC.IsUndefined Then
                                                    destJ = New clsUtilityCurveMeasureData(srcUC.NodeID, srcUC.ParentNodeID, DestUser.UserID, srcUC.Data, srcUC.UtilityCurve, srcUC.IsUndefined, srcUC.Comment)
                                                End If
                                        End Select
                                End Select

                                If destJ IsNot Nothing Then
                                    alt.DirectJudgmentsForNoCause.AddMeasureData(destJ, False)
                                End If
                            End If
                        Next
                    End If

                Next
            End If

            StorageManager.Writer.SaveUserJudgments(DestUser, Now)
            If DestUser IsNot User Then
                For Each H As clsHierarchy In Hierarchies
                    If Not ActiveHierarchyOnly Or ActiveHierarchyOnly And H.HierarchyID = ActiveHierarchy Then
                        CleanUpUserDataFromMemory(H.HierarchyID, DestUser.UserID, False, True)
                    End If
                Next
            Else
                PipeBuilder.PipeCreated = False
            End If

            Return True
        End Function

        Public Function CopyUserJudgmentsForControls(SourceUserEmail As String, DestUserEmail As String, Optional CopyMode As CopyJudgmentsMode = CopyJudgmentsMode.Replace, Optional PairwiseOnly As Boolean = False, Optional ActiveHierarchyOnly As Boolean = False) As Boolean 'A1444
            Dim SrcUser As clsUser = GetUserByEMail(SourceUserEmail)
            Dim DestUser As clsUser = GetUserByEMail(DestUserEmail)

            If SrcUser Is Nothing Or DestUser Is Nothing Then Return False

            Dim HierarchyID As Integer = If(ActiveHierarchyOnly, ActiveHierarchy, -1)

            Dim dt As DateTime
            StorageManager.Reader.LoadUserJudgmentsControls(dt, SrcUser)

            If CopyMode = CopyJudgmentsMode.AddMissing Or CopyMode = CopyJudgmentsMode.UpdateAndAddMissing Then
                StorageManager.Reader.LoadUserJudgmentsControls(dt, DestUser)
            Else
                ' replace
                CleanUpControlsJudgmentsFromMemory(DestUser.UserID, HierarchyID)
            End If

            For Each control As clsControl In Controls.Controls
                If HierarchyID = -1 OrElse ((HierarchyID = ECHierarchyID.hidLikelihood) AndAlso (control.Type = ControlType.ctCause OrElse control.Type = ControlType.ctCauseToEvent)) OrElse ((HierarchyID = ECHierarchyID.hidImpact) AndAlso (control.Type = ControlType.ctConsequence OrElse (control.Type = ControlType.ctConsequenceToEvent))) Then
                    For Each assignment As clsControlAssignment In control.Assignments
                        For Each srcJ As clsCustomMeasureData In assignment.Judgments.JudgmentsFromUser(SrcUser.UserID)
                            Dim doAdd As Boolean = True
                            If Not srcJ.IsUndefined Then
                                Dim destJ As clsCustomMeasureData = Nothing
                                Select Case assignment.MeasurementType
                                    Case ECMeasureType.mtRatings
                                        Dim srcR As clsRatingMeasureData = CType(srcJ, clsRatingMeasureData)
                                        Dim destR As clsRatingMeasureData = CType(assignment.Judgments, clsNonPairwiseJudgments).GetJudgement(srcR.NodeID, srcR.ParentNodeID, DestUser.UserID)
                                        Select Case CopyMode
                                            Case CopyJudgmentsMode.Replace, CopyJudgmentsMode.UpdateAndAddMissing
                                                destJ = New clsRatingMeasureData(srcR.NodeID, srcR.ParentNodeID, DestUser.UserID, srcR.Rating, srcR.RatingScale, srcR.IsUndefined, srcR.Comment)
                                            Case CopyJudgmentsMode.AddMissing
                                                If destR Is Nothing OrElse destR.IsUndefined Then
                                                    destJ = New clsRatingMeasureData(srcR.NodeID, srcR.ParentNodeID, DestUser.UserID, srcR.Rating, srcR.RatingScale, srcR.IsUndefined, srcR.Comment)
                                                End If
                                        End Select
                                    Case ECMeasureType.mtStep
                                        Dim srcSF As clsStepMeasureData = CType(srcJ, clsStepMeasureData)
                                        Dim destSF As clsStepMeasureData = CType(assignment.Judgments, clsNonPairwiseJudgments).GetJudgement(srcSF.NodeID, srcSF.ParentNodeID, DestUser.UserID)
                                        Select Case CopyMode
                                            Case CopyJudgmentsMode.Replace, CopyJudgmentsMode.UpdateAndAddMissing
                                                destJ = New clsStepMeasureData(srcSF.NodeID, srcSF.ParentNodeID, DestUser.UserID, srcSF.Value, srcSF.StepFunction, srcSF.IsUndefined, srcSF.Comment)
                                            Case CopyJudgmentsMode.AddMissing
                                                If destSF Is Nothing OrElse destSF.IsUndefined Then
                                                    destJ = New clsStepMeasureData(srcSF.NodeID, srcSF.ParentNodeID, DestUser.UserID, srcSF.Value, srcSF.StepFunction, srcSF.IsUndefined, srcSF.Comment)
                                                End If
                                        End Select
                                    Case ECMeasureType.mtDirect
                                        Dim srcDirect As clsDirectMeasureData = CType(srcJ, clsDirectMeasureData)
                                        Dim destDirect As clsDirectMeasureData = CType(assignment.Judgments, clsNonPairwiseJudgments).GetJudgement(srcDirect.NodeID, srcDirect.ParentNodeID, DestUser.UserID)
                                        Select Case CopyMode
                                            Case CopyJudgmentsMode.Replace, CopyJudgmentsMode.UpdateAndAddMissing
                                                destJ = New clsDirectMeasureData(srcDirect.NodeID, srcDirect.ParentNodeID, DestUser.UserID, srcDirect.DirectData, srcDirect.IsUndefined, srcDirect.Comment)
                                            Case CopyJudgmentsMode.AddMissing
                                                If destDirect Is Nothing OrElse destDirect.IsUndefined Then
                                                    destJ = New clsDirectMeasureData(srcDirect.NodeID, srcDirect.ParentNodeID, DestUser.UserID, srcDirect.DirectData, srcDirect.IsUndefined, srcDirect.Comment)
                                                End If
                                        End Select
                                    Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                        Dim srcUC As clsUtilityCurveMeasureData = CType(srcJ, clsUtilityCurveMeasureData)
                                        Dim destUC As clsUtilityCurveMeasureData = CType(assignment.Judgments, clsNonPairwiseJudgments).GetJudgement(srcUC.NodeID, srcUC.ParentNodeID, DestUser.UserID)
                                        Select Case CopyMode
                                            Case CopyJudgmentsMode.Replace, CopyJudgmentsMode.UpdateAndAddMissing
                                                destJ = New clsUtilityCurveMeasureData(srcUC.NodeID, srcUC.ParentNodeID, DestUser.UserID, srcUC.Data, srcUC.UtilityCurve, srcUC.IsUndefined, srcUC.Comment)
                                            Case CopyJudgmentsMode.AddMissing
                                                If destUC Is Nothing OrElse destUC.IsUndefined Then
                                                    destJ = New clsUtilityCurveMeasureData(srcUC.NodeID, srcUC.ParentNodeID, DestUser.UserID, srcUC.Data, srcUC.UtilityCurve, srcUC.IsUndefined, srcUC.Comment)
                                                End If
                                        End Select
                                End Select
                                If destJ IsNot Nothing Then
                                    assignment.Judgments.AddMeasureData(destJ, False)
                                End If
                            End If
                        Next
                    Next
                End If
            Next

            StorageManager.Writer.SaveUserJudgmentsControls(DestUser, Now)
            If DestUser IsNot User Then
                CleanUpControlsJudgmentsFromMemory(DestUser.UserID, HierarchyID)
            Else
                PipeBuilder.PipeCreated = False
            End If

            Return True
        End Function
#End Region

#Region "Evaluation progress"
        Public Function GetDefaultTotalJudgmentCount(ByVal HierarchyID As Integer) As Integer 'C0414
            Return ProjectAnalyzer.GetDefaultTotalJudgmentsCount(HierarchyID)
        End Function

        Public Function GetMadeJudgmentCount(ByVal HierarchyID As Integer, ByVal UserID As Integer, ByRef LastJudgmentTime As DateTime) As Integer 'C0766
            Dim aUser As clsUser = GetUserByID(UserID)
            If aUser Is Nothing Then Return 0

            Dim madeCount As Integer = StorageManager.Reader.GetMadeJudgementsCount(aUser, aUser.LastJudgmentTime, HierarchyID)
            LastJudgmentTime = aUser.LastJudgmentTime
            Return madeCount
        End Function

        Public Function GetTotalJudgmentCount(ByVal HierarchyID As Integer, ByVal UserID As Integer, Optional LoadPermissions As Boolean = True, Optional DefaultTotalCount As Integer = -1) As Integer
            Debug.Print("GetTotalJudgmentsCount for user: " + UserID.ToString)

            Dim PermissionsUserID As Integer
            Dim CheckOwnerPermissions As Boolean = PipeBuilder.IsSynchronousSession And IsSynchronousSessionEvaluator And (PipeBuilder.SynchronousOwner IsNot Nothing)

            If CheckOwnerPermissions Then
                PermissionsUserID = PipeBuilder.SynchronousOwner.UserID
            Else
                PermissionsUserID = UserID
            End If

            If User IsNot Nothing AndAlso User.UserID <> UserID Then
                Dim U As clsUser = GetUserByID(UserID)
                If U IsNot Nothing Then
                    If LoadPermissions AndAlso (PermissionsUserID <> UserID And PermissionsUserID <> User.UserID) Then
                        U = GetUserByID(PermissionsUserID)
                        StorageManager.Reader.LoadUserPermissions(U)
                    End If

                    Dim total As Integer = ProjectAnalyzer.GetTotalJudgmentsCount(UserID, HierarchyID, PermissionsUserID, False)
                    If LoadPermissions Then CleanUpUserDataFromMemory(HierarchyID, UserID)
                    Return total
                Else
                    If DefaultTotalCount = -1 Then
                        Return ProjectAnalyzer.GetDefaultTotalJudgmentsCount(ActiveHierarchy)
                    Else
                        Return DefaultTotalCount
                    End If

                End If
            Else
                Return ProjectAnalyzer.GetTotalJudgmentsCount(UserID, HierarchyID, PermissionsUserID, False)
            End If
        End Function

#End Region

        Public Overloads Function LoadProject(ByVal ConnectionString As String, ByVal ProviderType As DBProviderType, Optional ByVal StorageType As ECModelStorageType = ECModelStorageType.mstCanvasDatabase, Optional ByVal ModelID As Integer = -1) As Boolean
            StorageManager.StorageType = StorageType
            StorageManager.ModelID = ModelID
            StorageManager.ProjectLocation = ConnectionString
            StorageManager.ProviderType = ProviderType

            Dim res As Boolean = False
            If StorageType = ECModelStorageType.mstCanvasStreamDatabase Then
                res = StorageManager.Reader.LoadProject()
            Else
                res = StorageManager.LoadModelStream_AHPSFile()
            End If
            If res Then PipeBuilder = New clsPipeBuilder(Me, PipeParameters, Parameters)
            Return res
        End Function

        Public Overridable Sub LoadPipeParameters(Optional ByVal StorageType As PipeStorageType = PipeStorageType.pstDatabase, Optional ByVal ModelID As Integer = -1)
            PipeParameters.Read(StorageType, ProjectLocation, StorageManager.ProviderType, ModelID) 'C0269

            FeedbackOn = PipeParameters.FeedbackOn

            CalculationsManager.IncludeIdealAlternative = PipeParameters.IncludeIdealAlternative
            CalculationsManager.ShowIdealAlternative = PipeParameters.ShowIdealAlternative
            CalculationsManager.SynthesisMode = PipeParameters.SynthesisMode

            CalculationsManager.CombinedMode = PipeParameters.CombinedMode 'C0945

            CalculationsManager.UseCombinedForRestrictedNodes = PipeParameters.UseCISForIndividuals 'C0824
            CalculationsManager.UseUserWeights = PipeParameters.UseWeights

            DefaultGroupID = PipeParameters.DefaultGroupID

            Dim PS As ParameterSet = PipeParameters.CurrentParameterSet
            For Each H As clsHierarchy In Hierarchies
                Select Case H.HierarchyID
                    Case ECHierarchyID.hidLikelihood
                        PipeParameters.CurrentParameterSet = PipeParameters.DefaultParameterSet
                        If (PipeParameters.AltsDefaultContribution = ECAltsDefaultContribution.adcFull) Then
                            If H.IsUsingDefaultFullContribution Then
                                H.AltsDefaultContribution = ECAltsDefaultContribution.adcFull
                            Else
                                PipeParameters.AltsDefaultContribution = ECAltsDefaultContribution.adcNone
                                H.AltsDefaultContribution = ECAltsDefaultContribution.adcNone
                            End If
                        Else
                            H.AltsDefaultContribution = ECAltsDefaultContribution.adcNone
                        End If
                    Case ECHierarchyID.hidImpact
                        PipeParameters.CurrentParameterSet = PipeParameters.ImpactParameterSet
                        If (PipeParameters.AltsDefaultContributionImpact = ECAltsDefaultContribution.adcFull) Then
                            If H.IsUsingDefaultFullContribution Then
                                H.AltsDefaultContribution = ECAltsDefaultContribution.adcFull
                            Else
                                PipeParameters.AltsDefaultContributionImpact = ECAltsDefaultContribution.adcNone
                                H.AltsDefaultContribution = ECAltsDefaultContribution.adcNone
                            End If
                        Else
                            H.AltsDefaultContribution = ECAltsDefaultContribution.adcNone
                        End If
                End Select
                H.DefaultMeasurementTypeForCoveringObjectives = PipeParameters.DefaultCoveringObjectiveMeasurementType 'C0753
            Next
            PipeParameters.CurrentParameterSet = PS

        End Sub

        Public Overridable Sub SavePipeParameters(Optional ByVal StorageType As PipeStorageType = PipeStorageType.pstDatabase, Optional ByVal ModelID As Integer = -1)
            PipeParameters.Write(StorageType, ProjectLocation, StorageManager.ProviderType, ModelID)

            CalculationsManager.IncludeIdealAlternative = PipeParameters.IncludeIdealAlternative
            CalculationsManager.ShowIdealAlternative = PipeParameters.ShowIdealAlternative
            CalculationsManager.SynthesisMode = PipeParameters.SynthesisMode

            CalculationsManager.UseCombinedForRestrictedNodes = PipeParameters.UseCISForIndividuals
            CalculationsManager.UseUserWeights = PipeParameters.UseWeights
        End Sub

        Public ReadOnly Property DollarValueOfEnterprise As Double 'A1430
            Get
                Dim retVal As Double = 0
                If Hierarchy(ECHierarchyID.hidImpact) IsNot Nothing Then
                    retVal = Hierarchy(ECHierarchyID.hidImpact).Nodes(0).DollarValue
                End If
                Return If(retVal <> UNDEFINED_INTEGER_VALUE, retVal, 0)
            End Get
        End Property

        Private Function GetRoleStatisticFromList(ObjectiveID As Guid, AlternativeID As Guid, ListOfRoles As List(Of RolesStatistics)) As RolesStatistics
            Return ListOfRoles.Find(Function(rs) (rs.ObjectiveID.Equals(ObjectiveID) And rs.AlternativeID.Equals(AlternativeID)))
        End Function

        Function GetRolesStatistics(Optional SendType As RolesToSendType = RolesToSendType.rstAlternativesOnly) As List(Of RolesStatistics)
            Dim res As New List(Of RolesStatistics)
            Dim RS As RolesStatistics
            Dim H As clsHierarchy = Hierarchy(ActiveHierarchy)
            Dim AH As clsHierarchy = AltsHierarchy(ActiveAltsHierarchy)

            If SendType = RolesToSendType.rstObjectivesOnly Or SendType = RolesToSendType.rstAll Then
                For Each node As clsNode In H.Nodes
                    RS = New RolesStatistics
                    RS.ObjectiveID = node.NodeGuidID
                    RS.AlternativeID = Guid.Empty
                    RS.AllowedCount = 0
                    RS.RestrictedCount = 0
                    RS.EvaluatedCount = 0
                    res.Add(RS)
                Next
            End If

            If SendType = RolesToSendType.rstAlternativesOnly Or SendType = RolesToSendType.rstAll Then
                For Each node As clsNode In H.TerminalNodes
                    For Each alt As clsNode In node.GetNodesBelow(UNDEFINED_USER_ID)
                        RS = New RolesStatistics
                        RS.ObjectiveID = node.NodeGuidID
                        RS.AlternativeID = alt.NodeGuidID
                        RS.AllowedCount = 0
                        RS.RestrictedCount = 0
                        res.Add(RS)
                    Next
                Next

                If IsRiskProject And H.HierarchyID = ECHierarchyID.hidLikelihood And H.Nodes.Count > 1 Then
                    For Each alt As clsNode In H.GetUncontributedAlternatives()
                        RS = New RolesStatistics
                        RS.ObjectiveID = H.Nodes(0).NodeGuidID
                        RS.AlternativeID = alt.NodeGuidID
                        RS.AllowedCount = 0
                        RS.RestrictedCount = 0
                        RS.EvaluatedCount = 0
                        res.Add(RS)
                    Next
                End If
            End If

            For Each user As clsUser In UsersList
                'If user IsNot Me.User Then StorageManager.Reader.LoadUserData(user)
                StorageManager.Reader.LoadUserData(user)

                If SendType = RolesToSendType.rstObjectivesOnly Or SendType = RolesToSendType.rstAll Then
                    For Each node As ECCore.clsNode In H.Nodes
                        RS = GetRoleStatisticFromList(node.NodeGuidID, Guid.Empty, res)
                        If RS IsNot Nothing Then
                            Dim isAllowed As Boolean = UsersRoles.IsAllowedObjective(node.NodeGuidID, user.UserID)
                            RS.AllowedCount += CInt(IIf(isAllowed, 1, 0))
                            RS.RestrictedCount += CInt(IIf(isAllowed, 0, 1))

                            If isAllowed Then
                                Dim NodesList As List(Of clsNode) = node.GetNodesBelow(user.UserID)
                                If Not IsPWMeasurementType(node.MeasureType) AndAlso node.MeasureType <> ECMeasureType.mtPWOutcomes Then
                                    Dim evaluatedAll As Boolean = True
                                    For Each child As clsNode In NodesList
                                        If child.IsAlternative AndAlso UsersRoles.IsAllowedAlternative(node.NodeGuidID, child.NodeGuidID, user.UserID) Then
                                            Dim j As clsNonPairwiseMeasureData = CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(child.NodeID, node.NodeID, user.UserID)
                                            If j Is Nothing OrElse j.IsUndefined Then
                                                evaluatedAll = False
                                                Exit For
                                            End If
                                        End If
                                    Next
                                    RS.EvaluatedCount += If(evaluatedAll, 1, 0)
                                Else
                                    Dim DiagonalsEvaluationMode As DiagonalsEvaluation = PipeParameters.EvaluateDiagonals
                                    If Attributes.IsValueSet(ATTRIBUTE_EVALUATE_DIAGONALS_MODE_ID, node.NodeGuidID) Then
                                        DiagonalsEvaluationMode = Attributes.GetAttributeValue(ATTRIBUTE_EVALUATE_DIAGONALS_MODE_ID, node.NodeGuidID)
                                    Else
                                        If node.IsTerminalNode Then
                                            If PipeParameters.ForceAllDiagonalsForAlternatives And (NodesList.Count < PipeParameters.ForceAllDiagonalsLimitForAlternatives) Then
                                                DiagonalsEvaluationMode = DiagonalsEvaluation.deAll
                                            End If
                                        Else
                                            If PipeParameters.ForceAllDiagonals And (NodesList.Count < PipeParameters.ForceAllDiagonalsLimit) Then
                                                DiagonalsEvaluationMode = DiagonalsEvaluation.deAll
                                            End If
                                        End If
                                    End If

                                    Dim evaluatedAll As Boolean = True
                                    Dim size As Integer
                                    Dim M As Double(,) = CType(node.Judgments, clsPairwiseJudgments).GetPairwiseMatrix(user.UserID, size)
                                    Select Case DiagonalsEvaluationMode
                                        Case DiagonalsEvaluation.deAll
                                            For i As Integer = 0 To size - 2
                                                For j As Integer = i + 1 To size - 1
                                                    If M(i, j) = 0 Then
                                                        evaluatedAll = False
                                                        Exit For
                                                    End If
                                                Next
                                            Next
                                        Case DiagonalsEvaluation.deFirst
                                            For i As Integer = 0 To size - 2
                                                If M(i, i + 1) = 0 Then
                                                    evaluatedAll = False
                                                    Exit For
                                                End If
                                            Next
                                        Case DiagonalsEvaluation.deFirstAndSecond
                                            For i As Integer = 0 To size - 2
                                                If M(i, i + 1) = 0 Then
                                                    evaluatedAll = False
                                                    Exit For
                                                End If
                                            Next
                                            For i As Integer = 0 To size - 2
                                                If M(i, i + 1) = 0 Then
                                                    evaluatedAll = False
                                                    Exit For
                                                End If
                                            Next
                                    End Select

                                    RS.EvaluatedCount += If(evaluatedAll, 1, 0)
                                End If
                            End If
                        End If
                    Next
                End If

                If SendType = RolesToSendType.rstAlternativesOnly Or SendType = RolesToSendType.rstAll Then
                    For Each node As clsNode In H.TerminalNodes
                        For Each alt As clsNode In node.GetNodesBelow(UNDEFINED_USER_ID)
                            RS = GetRoleStatisticFromList(node.NodeGuidID, alt.NodeGuidID, res)
                            If RS IsNot Nothing Then
                                Dim isAllowed As Boolean = UsersRoles.IsAllowedAlternative(node.NodeGuidID, alt.NodeGuidID, user.UserID)
                                RS.AllowedCount += CInt(IIf(isAllowed, 1, 0))
                                RS.RestrictedCount += CInt(IIf(isAllowed, 0, 1))

                                If Not IsPWMeasurementType(node.MeasureType) AndAlso node.MeasureType <> ECMeasureType.mtPWOutcomes Then
                                    Dim j As clsNonPairwiseMeasureData = CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(alt.NodeID, node.NodeID, user.UserID)
                                    If j IsNot Nothing AndAlso Not j.IsUndefined AndAlso isAllowed Then RS.EvaluatedCount += 1
                                Else
                                    RS.EvaluatedCount = -1
                                End If
                            End If
                        Next
                    Next

                    If IsRiskProject And H.HierarchyID = ECHierarchyID.hidLikelihood And H.Nodes.Count > 1 Then
                        For Each alt As clsNode In H.GetUncontributedAlternatives
                            RS = GetRoleStatisticFromList(H.Nodes(0).NodeGuidID, alt.NodeGuidID, res)
                            If RS IsNot Nothing Then
                                Dim isAllowed As Boolean = UsersRoles.IsAllowedAlternative(H.Nodes(0).NodeGuidID, alt.NodeGuidID, user.UserID)
                                RS.AllowedCount += CInt(IIf(isAllowed, 1, 0))
                                RS.RestrictedCount += CInt(IIf(isAllowed, 0, 1))
                                If Not IsPWMeasurementType(alt.MeasureType) AndAlso alt.MeasureType <> ECMeasureType.mtPWOutcomes Then
                                    Dim j As clsNonPairwiseMeasureData = CType(alt.DirectJudgmentsForNoCause, clsNonPairwiseJudgments).GetJudgement(alt.NodeID, H.Nodes(0).NodeID, user.UserID)
                                    If j IsNot Nothing AndAlso Not j.IsUndefined AndAlso isAllowed Then RS.EvaluatedCount += 1
                                Else
                                    RS.EvaluatedCount = -1
                                End If
                            End If
                        Next
                    End If
                End If

                If user IsNot Me.User Then
                    CleanUpUserPermissionsFromMemory(user.UserID)
                    CleanUpUserDataFromMemory(H.HierarchyID, user.UserID)
                End If
            Next
            Return res
        End Function

        Public Function CopyHierarchy(SourceHierarchyID As Integer, DestinationHierarchyID As Integer, Optional CopyUserData As Boolean = True) As clsHierarchy
            'Dim sH As clsHierarchy = Hierarchies.FirstOrDefault(Function(h) (h.HierarchyID = SourceHierarchyID))
            Dim sH As clsHierarchy = GetHierarchyByID(SourceHierarchyID)
            Dim dH As clsHierarchy = GetHierarchyByID(DestinationHierarchyID)

            If dH Is Nothing Then
                Select Case sH.HierarchyType
                    Case ECHierarchyType.htModel
                        dH = AddHierarchy()
                    Case ECHierarchyType.htMeasure
                        dH = AddMeasureHierarchy()
                End Select
            End If

            dH.Nodes.Clear()

            Dim idLookup As New Dictionary(Of Guid, Guid)

            For Each node As clsNode In sH.Nodes
                Dim newNode As New clsNode
                newNode.Hierarchy = dH
                newNode.NodeID = node.NodeID
                newNode.ParentNodeID = node.ParentNodeID
                newNode.NodeGuidID = Guid.NewGuid
                newNode.NodeName = node.NodeName
                newNode.MeasureType = node.MeasureType
                newNode.InfoDoc = node.InfoDoc
                newNode.Level = node.Level
                newNode.MeasureMode = node.MeasureMode
                newNode.RatingScaleID = node.RatingScaleID
                newNode.RegularUtilityCurveID = node.RegularUtilityCurveID
                newNode.SOrder = node.SOrder
                newNode.StepFunctionID = node.StepFunctionID
                newNode.AHPAltData = Nothing
                newNode.AHPNodeData = Nothing
                newNode.AHPTag = Nothing

                dH.Nodes.Add(newNode)
                idLookup.Add(node.NodeGuidID, newNode.NodeGuidID)

                If IsPWMeasurementType(newNode.MeasureType) Then
                    newNode.Judgments = New clsPairwiseJudgments(newNode, Me)
                Else
                    newNode.Judgments = New clsNonPairwiseJudgments(newNode, Me)
                End If
            Next

            dH.ResetNodesDictionaries()

            For Each node As clsNode In sH.Nodes
                Dim dNode As clsNode = dH.GetNodeByID(node.NodeID)
                dNode.Children = New List(Of clsNode)
                For Each child As clsNode In node.Children
                    dNode.Children.Add(dH.GetNodeByID(child.NodeID))
                Next
                dNode.ChildrenAlts = New HashSet(Of Integer)
                For Each childAlt As Integer In node.ChildrenAlts
                    dNode.ChildrenAlts.Add(childAlt)
                Next
            Next

            'CreateHierarchyNodesLinks(dH.HierarchyID)
            CreateHierarchyLevelValues(dH)

            For Each node As clsNode In dH.Nodes
                'node.ParentNode(True, False) = dH.GetNodeByID(node.ParentNodeID)
                node.ParentNodesGuids.Clear()
                Dim sNode As clsNode = sH.GetNodeByID(node.NodeID)
                For Each id As Guid In sNode.ParentNodesGuids
                    node.ParentNodesGuids.Add(idLookup(id))
                    node.ParentNodes.Add(dH.GetNodeByID(node.ParentNodesGuids(node.ParentNodesGuids.Count - 1)))
                Next
            Next

            If CopyUserData Then
                For Each group As clsCombinedGroup In CombinedGroups.GroupsList
                    Dim gRoles As clsUserRoles = UsersRoles.GetUserRolesByID(group.CombinedUserID)
                    If gRoles IsNot Nothing Then
                        Dim c As Integer
                        c = gRoles.ObjectivesRoles.Allowed.Count
                        For i As Integer = 0 To c - 1
                            If idLookup.ContainsKey(gRoles.ObjectivesRoles.Allowed(i)) Then
                                gRoles.ObjectivesRoles.Allowed.Add(idLookup(gRoles.ObjectivesRoles.Allowed(i)))
                            End If
                        Next
                        c = gRoles.ObjectivesRoles.Restricted.Count
                        For i As Integer = 0 To c - 1
                            If idLookup.ContainsKey(gRoles.ObjectivesRoles.Restricted(i)) Then
                                gRoles.ObjectivesRoles.Restricted.Add(idLookup(gRoles.ObjectivesRoles.Restricted(i)))
                            End If
                        Next

                        c = gRoles.ObjectivesRoles.Undefined.Count
                        For i As Integer = 0 To c - 1
                            If idLookup.ContainsKey(gRoles.ObjectivesRoles.Undefined(i)) Then
                                gRoles.ObjectivesRoles.Undefined.Add(idLookup(gRoles.ObjectivesRoles.Undefined(i)))
                            End If
                        Next

                        Dim addedRoles As New Dictionary(Of Guid, clsAlternativesRoles)

                        For Each objID As Guid In gRoles.AlternativesRoles.Keys
                            If idLookup.ContainsKey(objID) Then
                                Dim altRoles As New clsAlternativesRoles
                                altRoles.CoveringObjectiveID = idLookup(objID)
                                addedRoles.Add(idLookup(objID), altRoles)
                                For Each altID As Guid In gRoles.AlternativesRoles(objID).AllowedAlternativesList
                                    altRoles.AllowedAlternativesList.Add(altID)
                                Next
                                For Each altID As Guid In gRoles.AlternativesRoles(objID).RestrictedAlternativesList
                                    altRoles.RestrictedAlternativesList.Add(altID)
                                Next
                                For Each altID As Guid In gRoles.AlternativesRoles(objID).UndefinedAlternativesList
                                    altRoles.UndefinedAlternativesList.Add(altID)
                                Next
                            End If
                        Next

                        For Each kvp As KeyValuePair(Of Guid, clsAlternativesRoles) In addedRoles
                            gRoles.AlternativesRoles.Add(kvp.Key, kvp.Value)
                        Next

                        StorageManager.Writer.SaveGroupPermissions(group)
                    End If
                Next

                For Each u As clsUser In UsersList
                    StorageManager.Reader.LoadUserData(u)

                    For Each node As clsNode In sH.Nodes
                        Dim SrcJudgments As List(Of clsCustomMeasureData) = node.Judgments.JudgmentsFromUser(u.UserID)
                        For Each srcJ As clsCustomMeasureData In SrcJudgments
                            If Not srcJ.IsUndefined Then
                                Dim destJ As clsCustomMeasureData = Nothing
                                Select Case node.MeasureType
                                    Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous
                                        Dim srcPW As clsPairwiseMeasureData = CType(srcJ, clsPairwiseMeasureData)
                                        destJ = New clsPairwiseMeasureData(srcPW.FirstNodeID, srcPW.SecondNodeID, srcPW.Advantage, srcPW.Value, srcPW.ParentNodeID, u.UserID, srcPW.IsUndefined, srcPW.Comment)
                                    Case ECMeasureType.mtRatings
                                        Dim srcR As clsRatingMeasureData = CType(srcJ, clsRatingMeasureData)
                                        destJ = New clsRatingMeasureData(srcR.NodeID, srcR.ParentNodeID, u.UserID, srcR.Rating, srcR.RatingScale, srcR.IsUndefined, srcR.Comment)
                                    Case ECMeasureType.mtStep
                                        Dim srcSF As clsStepMeasureData = CType(srcJ, clsStepMeasureData)
                                        destJ = New clsStepMeasureData(srcSF.NodeID, srcSF.ParentNodeID, u.UserID, srcSF.Value, srcSF.StepFunction, srcSF.IsUndefined, srcSF.Comment)
                                    Case ECMeasureType.mtDirect
                                        Dim srcDirect As clsDirectMeasureData = CType(srcJ, clsDirectMeasureData)
                                        destJ = New clsDirectMeasureData(srcDirect.NodeID, srcDirect.ParentNodeID, u.UserID, srcDirect.DirectData, srcDirect.IsUndefined, srcDirect.Comment)
                                    Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                        Dim srcUC As clsUtilityCurveMeasureData = CType(srcJ, clsUtilityCurveMeasureData)
                                        destJ = New clsUtilityCurveMeasureData(srcUC.NodeID, srcUC.ParentNodeID, u.UserID, srcUC.Data, srcUC.UtilityCurve, srcUC.IsUndefined, srcUC.Comment)
                                End Select
                                If destJ IsNot Nothing Then
                                    dH.GetNodeByID(node.NodeID).Judgments.AddMeasureData(destJ, False)
                                End If
                            End If
                        Next

                    Next

                    Dim roles As clsUserRoles = UsersRoles.GetUserRolesByID(u.UserID)
                    If roles IsNot Nothing Then
                        Dim c As Integer
                        c = roles.ObjectivesRoles.Allowed.Count
                        For i As Integer = 0 To c - 1
                            If idLookup.ContainsKey(roles.ObjectivesRoles.Allowed(i)) Then
                                roles.ObjectivesRoles.Allowed.Add(idLookup(roles.ObjectivesRoles.Allowed(i)))
                            End If
                        Next
                        c = roles.ObjectivesRoles.Restricted.Count
                        For i As Integer = 0 To c - 1
                            If idLookup.ContainsKey(roles.ObjectivesRoles.Restricted(i)) Then
                                roles.ObjectivesRoles.Restricted.Add(idLookup(roles.ObjectivesRoles.Restricted(i)))
                            End If
                        Next

                        c = roles.ObjectivesRoles.Undefined.Count
                        For i As Integer = 0 To c - 1
                            If idLookup.ContainsKey(roles.ObjectivesRoles.Undefined(i)) Then
                                roles.ObjectivesRoles.Undefined.Add(idLookup(roles.ObjectivesRoles.Undefined(i)))
                            End If
                        Next

                        Dim addedRoles As New Dictionary(Of Guid, clsAlternativesRoles)

                        For Each objID As Guid In roles.AlternativesRoles.Keys
                            If idLookup.ContainsKey(objID) Then
                                Dim altRoles As New clsAlternativesRoles
                                altRoles.CoveringObjectiveID = idLookup(objID)
                                addedRoles.Add(idLookup(objID), altRoles)
                                For Each altID As Guid In roles.AlternativesRoles(objID).AllowedAlternativesList
                                    altRoles.AllowedAlternativesList.Add(altID)
                                Next
                                For Each altID As Guid In roles.AlternativesRoles(objID).RestrictedAlternativesList
                                    altRoles.RestrictedAlternativesList.Add(altID)
                                Next
                                For Each altID As Guid In roles.AlternativesRoles(objID).UndefinedAlternativesList
                                    altRoles.UndefinedAlternativesList.Add(altID)
                                Next
                            End If
                        Next

                        For Each kvp As KeyValuePair(Of Guid, clsAlternativesRoles) In addedRoles
                            roles.AlternativesRoles.Add(kvp.Key, kvp.Value)
                        Next

                        StorageManager.Writer.SaveUserPermissions(u)
                    End If

                    StorageManager.Writer.SaveUserJudgments(u.UserID)

                    If u IsNot User Then
                        CleanUpUserDataFromMemory(sH.HierarchyID, u.UserID)
                        CleanUpUserDataFromMemory(dH.HierarchyID, u.UserID)
                    End If
                Next
            End If

            Return dH
        End Function

        Public Sub New(Optional ByVal LoadOnDemand As Boolean = True, Optional ByVal IsSynchronousSessionEvaluator As Boolean = False, Optional isRisk As Boolean = False)
            IsRiskProject = isRisk

            If IsRiskProject Then
                MeasureScales.AddDefaultRatingScaleForLikelihood()
                MeasureScales.AddDefaultRatingScaleForImpact()
                MeasureScales.AddDefaultVulnerabilityRatingScale()
                MeasureScales.AddDefaultControlsRatingScale()

                MeasureScales.AddDefaultRegularUtilityCurveForLikelihood()
                MeasureScales.AddDefaultRegularUtilityCurveForImpact()
                MeasureScales.AddDefaultRegularUtilityCurveForControls()
                MeasureScales.AddDefaultRegularUtilityCurveForVulnerability()

                MeasureScales.AddDefaultStepFunctionForLikelihood()
                MeasureScales.AddDefaultStepFunctionForImpact()
                MeasureScales.AddDefaultStepFunctionForControls()
                MeasureScales.AddDefaultStepFunctionForVulnerabilities()
            Else
                MeasureScales.AddDefaultRatingScale()
                MeasureScales.AddDefaultStepFunction()
                MeasureScales.AddDefaultRegularUtilityCurve()
            End If

            MeasureScales.AddDefaultOutcomesScale()
            MeasureScales.AddDefaultPWofPercentagesScale()
            MeasureScales.AddDefaultExpectedValuesScale()
            MeasureScales.AddDefaultAdvancedUtilityCurve()

            AddHierarchy()
            AddAltsHierarchy()

            mActiveHierarchy = 0
            mActiveAltsHierarchy = 1

            If IsRiskProject Then AddImpactHierarchy() ' D2740

            LastModifyTime = Now 'C0611

            PipeParameters = New clsPipeParamaters
            Parameters = New clsProjectParametersWithDefaults(Me)
            Reports = New clsReportsCollection(Me)  ' D6502
            PipeBuilder = New clsPipeBuilder(Me, PipeParameters, Parameters)

            mIsSynchronousSessionEvaluator = IsSynchronousSessionEvaluator
            PipeBuilder.IsSynchronousSessionEvaluator = IsSynchronousSessionEvaluator
            ProjectAnalyzer.ProjectManager = Me
            ProjectAnalyzer.PipeParameters = PipeParameters

            RiskSimulations = New RiskSimulations(Me)

            Edges = New Edges(Me)
        End Sub

    End Class

End Namespace