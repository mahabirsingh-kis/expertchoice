Imports System.ComponentModel
Imports System.IO
Imports ECCore
Imports GenericDBAccess
Imports System.Data.Common
Imports ECCore.MiscFuncs
Imports Canvas

Namespace ECCore
    <Serializable()> Public Class StorageReaderAHP
        Inherits clsStorageReader

        Protected mLoadTime As DateTime

        Public Overrides Function LoadProject() As Boolean
            If ProjectManager Is Nothing Then Return False

            Dim res As Boolean = LoadProjectFromAHP()
            If res Then
                If ProjectManager.IsRiskProject AndAlso Not ProjectManager.IsValidHierarchyID(ECHierarchyID.hidImpact) Then ' D2740
                    ProjectManager.AddImpactHierarchy()
                End If

                ProjectManager.VerifyObjectivesHierarchies()
                Dim NeedToFixRatings As Boolean = ProjectManager.MeasureScales.FixRatingScales
                If NeedToFixRatings Then
                    ProjectManager.StorageManager.Writer.SaveProject(True)
                End If
            End If
            Return res
        End Function


        Private Function LoadProjectFromAHP() As Boolean
            If (ProjectManager Is Nothing) OrElse Not CheckDBConnection(ProviderType, Location) Then Return False

            mLoadTime = Now

            'FixJudgmentsTable(ConnectionString)

            Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location) 'C0235
                Dim dbReader As DbDataReader
                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection 'C0235

                dbConnection.Open()

                Dim node As clsNode
                Dim alt As clsNode

                'C0775===
                ' Step 1: Load objectives
                ' Step 2: Load alternatives
                ' Step 3: Load alts contributions
                ' Step 4: Load info docs
                ' Step 5: Load users
                ' Step 6: Load measurement types
                ' Step 7 (usersCount): Load user judgments
                ' Step 8: Load user permissions - objectives
                ' Step 9: Load user permissions - alternatives
                'C0775==

                ' load nodes

                oCommand.CommandText = "SELECT * FROM NodeDefs ORDER BY sOrder" 'C0235
                dbReader = DBExecuteReader(ProviderType, oCommand)

                ProjectManager.Hierarchies.Clear()
                ProjectManager.AltsHierarchies.Clear()

                Dim H As clsHierarchy = Nothing

                If Not dbReader Is Nothing Then
                    H = ProjectManager.AddHierarchy()
                    H.Nodes.Clear()
                    H.ResetNodesDictionaries()

                    While dbReader.Read
                        node = New clsNode(H)

                        node.NodeID = CInt(CStr(dbReader("NID")).Substring(1))

                        If Not TypeOf (dbReader("Parent")) Is DBNull Then
                            If dbReader("Parent") <> "" Then
                                node.ParentNodeID = CInt(CStr(dbReader("Parent")).Substring(1))
                            End If
                        End If
                        node.NodeName = dbReader("NodeName")

                        'C0304===
                        node.AHPNodeData = New clsAHPNodeData

                        If Not (TypeOf (dbReader("EnforceMode")) Is DBNull) Then
                            node.AHPNodeData.EnforceMode = dbReader("EnforceMode")
                        End If

                        If Not (TypeOf (dbReader("StructuralAdjust")) Is DBNull) Then
                            node.AHPNodeData.StructuralAdjust = dbReader("StructuralAdjust")
                        End If

                        If Not (TypeOf (dbReader("topp")) Is DBNull) Then
                            node.AHPNodeData.topp = dbReader("topp")
                        End If

                        If Not (TypeOf (dbReader("Leftt")) Is DBNull) Then
                            node.AHPNodeData.Leftt = dbReader("Leftt")
                        End If

                        If Not (TypeOf (dbReader("ProtectedJudgments")) Is DBNull) Then
                            node.AHPNodeData.ProtectedJudgments = dbReader("ProtectedJudgments")
                        End If
                        'C0304==

                        H.Nodes.Add(node)
                    End While
                End If

                H.ResetNodesDictionaries()

                dbReader.Close()

                'define structure
                ProjectManager.CreateHierarchyNodesLinks(H.HierarchyID)
                ProjectManager.CreateHierarchyLevelValues(H)

                ' load active alternatives IDs
                Dim mActiveAlts As New ArrayList

                'oCommand = New odbc.odbcCommand("SELECT * FROM AltsActive", dbConnection) 'Toodbc 'C0235
                oCommand.CommandText = "SELECT * FROM AltsActive" 'C0235
                dbReader = DBExecuteReader(ProviderType, oCommand)

                If Not dbReader Is Nothing Then
                    While dbReader.Read
                        mActiveAlts.Add(CInt(CStr(dbReader("AID")).Substring(1)))
                    End While
                End If

                'oCommand = Nothing 'C0235
                dbReader.Close()

                ' load alternatives
                oCommand.CommandText = "SELECT * FROM AltsGlobal ORDER BY sOrder" 'C0615

                dbReader = DBExecuteReader(ProviderType, oCommand)
                Dim periodFieldExists As Boolean = False
                Dim basePeriodAIDFieldExists As Boolean = False

                Dim fieldChecked As Boolean = False
                Dim A As clsHierarchy = Nothing

                ProjectManager.Attributes.ReadAttributes(AttributesStorageType.astDatabase, Location, ProviderType, -1)

                'Dim mid As Integer 'C0065
                If Not dbReader Is Nothing Then
                    A = ProjectManager.AddAltsHierarchy()
                    A.ResetNodesDictionaries()

                    While dbReader.Read

                        'C0429===
                        If Not fieldChecked Then
                            For j As Integer = 0 To dbReader.FieldCount - 1
                                If dbReader.GetName(j).ToLower = "period" Then
                                    periodFieldExists = True
                                End If
                                If dbReader.GetName(j).ToLower = "baseperiodaid" Then
                                    basePeriodAIDFieldExists = True
                                End If
                            Next
                            fieldChecked = True
                        End If
                        'C0429==

                        node = New clsNode(A)
                        node.NodeID = CInt(CStr(dbReader("AID")).Substring(1))
                        node.NodeName = dbReader("AltName")
                        node.SOrder = CInt(dbReader("SOrder"))
                        ProjectManager.Attributes.SetAttributeValue(ATTRIBUTE_RA_ALT_SORT_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtLong, node.SOrder, node.NodeGuidID, Guid.Empty)

                        'C0065===
                        'If Integer.TryParse(CStr(dbReader("MID")), mid) Then
                        '    node.AHPTag = mid
                        'End If
                        node.AHPTag = CStr(dbReader("MID"))
                        'C0065==

                        'C0304===
                        node.AHPAltData = New clsAHPAltData

                        If Not (TypeOf (dbReader("Infeasible")) Is DBNull) Then
                            node.AHPAltData.Infeasible = dbReader("Infeasible")
                        End If

                        'C0306===
                        'If Not (TypeOf (dbReader("IID")) Is DBNull) Then
                        '    node.AHPAltData.IID = dbReader("IID")
                        'End If
                        'C0306==

                        If Not (TypeOf (dbReader("MID")) Is DBNull) Then
                            node.AHPAltData.MID = dbReader("MID")
                        End If

                        If Not (TypeOf (dbReader("Level")) Is DBNull) Then
                            node.AHPAltData.Level = dbReader("Level")
                        End If

                        If Not (TypeOf (dbReader("IsLeaf")) Is DBNull) Then
                            node.AHPAltData.IsLeaf = dbReader("IsLeaf")
                        End If

                        If Not (TypeOf (dbReader("Selected")) Is DBNull) Then
                            node.AHPAltData.Selected = dbReader("Selected")
                        End If

                        'C0429===
                        If periodFieldExists Then
                            If Not (TypeOf (dbReader("Period")) Is DBNull) Then
                                node.AHPAltData.Period = dbReader("Period")
                            End If
                        End If

                        If basePeriodAIDFieldExists Then
                            If Not (TypeOf (dbReader("BasePeriodAID")) Is DBNull) Then
                                node.AHPAltData.BasePeriodAID = dbReader("BasePeriodAID")
                            End If
                        End If

                        If node.NodeID <> 0 Then
                            A.Nodes.Add(node) 'C0585
                        End If
                        'C0069==
                    End While
                End If

                A.ResetNodesDictionaries()

                dbReader.Close()

                mActiveAlts = Nothing
                oCommand.CommandText = "SELECT * FROM AltsData WHERE WRT=?" 'C0235
                oCommand.Parameters.Clear()
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "WRT", "N0")) 'C0235 'C0237
                dbReader = DBExecuteReader(ProviderType, oCommand)

                If dbReader.HasRows Then
                    While dbReader.Read
                        alt = A.GetNodeByID(CInt(CStr(dbReader("AID")).Substring(1)))
                        If alt IsNot Nothing Then
                            If Not TypeOf (dbReader("DATA")) Is DBNull Then
                                'alt.Tag = CSng(dbReader("DATA")) 'C0185
                                'alt.Tag = CSng(FixStringWithSingleValue(CStr(dbReader("DATA")))) 'C0185 'C0626

                                ' for now keep the old way of saving costs in ECCore
                                alt.Tag = CStr(dbReader("DATA")) 'C0626

                                ' but also use a new one
                                Dim dRes As Double
                                If Double.TryParse(FixStringWithSingleValue(CStr(dbReader("DATA"))), dRes) Then
                                    ProjectManager.Attributes.SetAttributeValue(ATTRIBUTE_COST_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtDouble, dRes, alt.NodeGuidID, Guid.Empty)
                                Else
                                    'Debug.Print("Wrong double value in AHP file: " + FixStringWithSingleValue(CStr(dbReader("DATA"))))
                                End If
                            End If
                        End If
                    End While
                End If
                dbReader.Close()

                If TableExists(Location, ProviderType, "AltsContributeTo") Then 'C0236
                    'oCommand = New odbc.odbcCommand("SELECT * FROM AltsContributeTo", dbConnection) 'C0235
                    oCommand.CommandText = "SELECT * FROM AltsContributeTo" 'C0235
                    dbReader = DBExecuteReader(ProviderType, oCommand)

                    If dbReader.HasRows Then
                        While dbReader.Read
                            node = H.GetNodeByID(CInt(CStr(dbReader("WRT")).Substring(1)))
                            alt = A.GetNodeByID(CInt(CStr(dbReader("AID")).Substring(1)))
                            If Not node Is Nothing And Not alt Is Nothing Then
                                node.ChildrenAlts.Add(alt.NodeID)
                            End If
                        End While
                    Else
                        For Each node In H.Nodes
                            For Each alt In A.TerminalNodes
                                node.ChildrenAlts.Add(alt.NodeID)
                            Next
                        Next
                    End If
                    dbReader.Close()
                Else
                    For Each node In H.Nodes
                        For Each alt In A.TerminalNodes
                            node.ChildrenAlts.Add(alt.NodeID)
                        Next
                    Next
                End If

                If ProjectManager.Hierarchies.Count > 0 Then
                    ProjectManager.ActiveHierarchy = H.HierarchyID
                End If

                If ProjectManager.AltsHierarchies.Count > 0 Then
                    ProjectManager.ActiveAltsHierarchy = A.HierarchyID
                End If

                ' check full contribution
                Dim altsCount As Integer = A.TerminalNodes.Count
                Dim isFull As Boolean = True
                For Each node In H.TerminalNodes
                    If node.ChildrenAlts.Count <> altsCount Then
                        isFull = False
                        Exit For
                    End If
                Next

                If isFull Then
                    For Each node In H.TerminalNodes
                        node.ChildrenAlts.Clear()
                    Next
                    H.AltsDefaultContribution = ECAltsDefaultContribution.adcFull
                End If

                ' load infodocs

                oCommand.CommandText = "SELECT * FROM Documents" 'C0235
                dbReader = DBExecuteReader(ProviderType, oCommand)

                While dbReader.Read
                    If Not TypeOf (dbReader("INFODOC")) Is DBNull Then
                        If CStr(dbReader("NID")).Substring(0, 1).ToLower = "n" Then
                            If Not H Is Nothing Then
                                node = H.GetNodeByID(CInt(CStr(dbReader("NID")).Substring(1)))
                                If Not node Is Nothing Then
                                    node.InfoDoc = CStr(dbReader("INFODOC"))
                                End If
                            End If
                        Else
                            If Not A Is Nothing Then
                                node = A.GetNodeByID(CInt(CStr(dbReader("NID")).Substring(1)))
                                If Not node Is Nothing Then
                                    node.InfoDoc = CStr(dbReader("INFODOC"))
                                End If
                            End If
                        End If
                    End If
                End While

                dbReader.Close()

                ' load users
                Dim user As clsUser

                ' EC11 style of defining users

                'Dim qString As String = "SELECT * FROM People ORDER BY PID"
                Dim qString As String
                qString = "SELECT * FROM People ORDER BY PID"

                oCommand.CommandText = qString 'C0235
                dbReader = DBExecuteReader(ProviderType, oCommand)

                Dim i As Integer = 0
                If Not dbReader Is Nothing Then
                    ProjectManager.UsersList.Clear()
                    While dbReader.Read
                        If dbReader("PID") <> 1 Then ' if not combined
                            user = New clsUser
                            user.UserID = dbReader("PID") 'PID
                            user.Active = dbReader("Participating")
                            i = i + 1
                            If Not (TypeOf (dbReader("PersonName")) Is DBNull) Then
                                user.UserName = dbReader("PersonName") 'PersonName
                            End If
                            If Not (TypeOf (dbReader("Email")) Is DBNull) Then
                                user.UserEMail = dbReader("Email") 'email
                            End If

                            'C0304===
                            user.AHPUserData = New clsAHPUserData

                            If FieldExists(dbReader, "Weight") Then 'AS/076 enclosed
                                If Not (TypeOf (dbReader("Weight")) Is DBNull) Then
                                    'user.AHPUserData.Weight = dbReader("Weight") 'C0851
                                    user.Weight = CSng(dbReader("Weight")) 'C0851
                                End If
                            End If 'AS/076

                            If Not (TypeOf (dbReader("Organization")) Is DBNull) Then
                                user.AHPUserData.Organization = dbReader("Organization")
                            End If

                            If Not (TypeOf (dbReader("Keypad")) Is DBNull) Then
                                user.AHPUserData.Keypad = dbReader("Keypad")
                                user.VotingBoxID = CInt(dbReader("Keypad")) 'C0737
                            End If

                            If Not (TypeOf (dbReader("Wave")) Is DBNull) Then
                                user.AHPUserData.Wave = dbReader("Wave")
                            End If

                            If Not (TypeOf (dbReader("Location")) Is DBNull) Then
                                user.AHPUserData.Location = dbReader("Location")
                            End If

                            If Not (TypeOf (dbReader("Eval")) Is DBNull) Then
                                user.AHPUserData.Eval = dbReader("Eval")
                            End If

                            If Not (TypeOf (dbReader("EvalCluster")) Is DBNull) Then
                                user.AHPUserData.EvalCluster = dbReader("EvalCluster")
                            End If

                            If Not (TypeOf (dbReader("RoleWritingType")) Is DBNull) Then
                                user.AHPUserData.RoleWritingType = dbReader("RoleWritingType")
                            End If

                            If Not (TypeOf (dbReader("RoleViewingType")) Is DBNull) Then
                                user.AHPUserData.RoleViewingType = dbReader("RoleViewingType")
                            End If
                            'C0304==

                            user.LastJudgmentTime = mLoadTime
                            ProjectManager.AddUser(user)
                        End If
                    End While
                End If

                'oCommand = Nothing 'C0235
                dbReader.Close()

                ' load Measure Types

                oCommand.CommandText = "SELECT * FROM llCritFormulas" 'C0235
                dbReader = DBExecuteReader(ProviderType, oCommand)

                Dim InfoDocFieldExists As Boolean = False
                For j As Integer = 0 To dbReader.FieldCount - 1
                    If dbReader.GetName(j).ToLower = "infodoc" Then
                        InfoDocFieldExists = True
                    End If
                Next

                Dim AdvancedUCLoaded As Boolean = False 'C0066

                Dim ucIncreasing As Boolean = True 'C0070

                If Not dbReader Is Nothing Then
                    While dbReader.Read
                        node = H.GetNodeByID(CInt(CStr(dbReader("WRT")).Substring(1)))
                        If Not node Is Nothing Then
                            'If node.IsTerminalNode Then 'C0382 - because in MaxOutModels there might be direct non-terminal nodes
                            Select Case CInt(dbReader("FTYPE"))
                                Case 9 ' Pairwise
                                    'node.MeasureType = ECMeasureType.mtPairwise 'C0073
                                    node.MeasureType(False) = ECMeasureType.mtPairwise 'C0073
                                Case 4 ' Ratings
                                    'node.MeasureType = ECMeasureType.mtRatings 'C0073
                                    node.MeasureType(False) = ECMeasureType.mtRatings 'C0073

                                    If InfoDocFieldExists Then
                                        If Not TypeOf (dbReader("InfoDoc")) Is DBNull Then
                                            node.TempRatingScaleInfoDoc = CStr(dbReader("InfoDoc"))
                                        End If
                                    End If
                                Case 1, 2
                                    node.MeasureType(False) = ECMeasureType.mtRegularUtilityCurve
                                    ucIncreasing = CInt(dbReader("FTYPE")) = 1

                                    If Not TypeOf (dbReader("Description")) Is DBNull Then
                                        Dim desc As String = dbReader("Description")
                                        If desc <> "" Then
                                            Dim descType As String = desc(0)
                                            Dim v As Single

                                            If (descType.ToUpper = "F") Or (descType.ToUpper = "C") Or (descType.ToUpper = "M") Then
                                                node.Tag = New clsMaxOutData
                                            End If

                                            Select Case descType.ToUpper
                                                Case "F"
                                                    If Integer.TryParse(desc.Substring(1), v) Then
                                                        v /= 100
                                                        CType(node.Tag, clsMaxOutData).HasFixedLocalPriority = True
                                                        CType(node.Tag, clsMaxOutData).FixedLocalPriority = v
                                                    End If
                                                Case "C"
                                                    CType(node.Tag, clsMaxOutData).HasCalculatedLocalPriority = True
                                                Case "M"
                                                    If Integer.TryParse(desc.Substring(1), v) Then
                                                        v /= 100
                                                        CType(node.Tag, clsMaxOutData).HasMaxPriorityValue = True
                                                        CType(node.Tag, clsMaxOutData).MaxPriorityValue = v
                                                    End If
                                            End Select
                                        End If
                                    End If
                                'C0382==

                                Case 3 'C0066
                                    'If TableExists(Location, "Nodes") Then 'C0236
                                    If TableExists(Location, ProviderType, "Nodes") Then 'C0236
                                        Using aucConnection As DbConnection = GetDBConnection(ProviderType, Location) 'C0235

                                            'C0235===
                                            'Dim aucReader As odbc.odbcDataReader
                                            'Dim aucCommand As odbc.odbcCommand
                                            Dim aucReader As DbDataReader
                                            Dim aucCommand As DbCommand = GetDBCommand(ProviderType)
                                            'C0235==

                                            aucConnection.Open()
                                            aucCommand.Connection = aucConnection 'C0317

                                            'aucCommand = New odbc.odbcCommand("SELECT * FROM Nodes WHERE HierarchyID=? AND NodeID=?", aucConnection) 'C0235
                                            aucCommand.CommandText = "SELECT * FROM Nodes WHERE HierarchyID=? AND NodeID=?" 'C0235
                                            aucCommand.Parameters.Clear()
                                            'C0235===
                                            'aucCommand.Parameters.AddWithValue("HierarchyID", H.HierarchyID)
                                            'aucCommand.Parameters.AddWithValue("NodeID", node.NodeID)
                                            aucCommand.Parameters.Add(GetDBParameter(ProviderType, "HierarchyID", H.HierarchyID)) 'C0237
                                            aucCommand.Parameters.Add(GetDBParameter(ProviderType, "NodeID", node.NodeID)) 'C0237
                                            'C0235==

                                            aucReader = DBExecuteReader(ProviderType, aucCommand)

                                            Dim MT As ECMeasureType = ECMeasureType.mtNone
                                            Dim MSid As Integer
                                            If aucReader.HasRows Then
                                                MT = CInt(aucReader("MeasureType"))
                                                MSid = CInt(aucReader("MeasurementScale"))
                                            End If
                                            aucReader.Close()

                                            If MT = ECMeasureType.mtAdvancedUtilityCurve Then
                                                'node.MeasureType = ECMeasureType.mtAdvancedUtilityCurve 'C0073
                                                node.MeasureType(False) = ECMeasureType.mtAdvancedUtilityCurve 'C0073

                                                If Not AdvancedUCLoaded Then
                                                    Dim AUC As clsAdvancedUtilityCurve
                                                    ProjectManager.MeasureScales.AdvancedUtilityCurves.Clear()

                                                    'aucCommand = New odbc.odbcCommand("SELECT * FROM AdvancedUtilityCurves", aucConnection) 'C0235
                                                    aucCommand.CommandText = "SELECT * FROM AdvancedUtilityCurves" 'C0235
                                                    aucReader = DBExecuteReader(ProviderType, aucCommand)

                                                    If Not aucReader Is Nothing Then
                                                        While aucReader.Read
                                                            AUC = New clsAdvancedUtilityCurve(aucReader("id"))
                                                            AUC.Name = aucReader("Name")
                                                            AUC.Comment = aucReader("Comment")
                                                            ProjectManager.MeasureScales.AdvancedUtilityCurves.Add(AUC)
                                                            AUC.Points.Clear()
                                                        End While
                                                    End If
                                                    'aucCommand = Nothing 'C0235
                                                    aucReader.Close()

                                                    ' Load points for Advanced Utility Curves
                                                    'aucCommand = New odbc.odbcCommand("SELECT * FROM UCPoints", aucConnection) 'C0235
                                                    aucCommand.CommandText = "SELECT * FROM UCPoints" 'C0235
                                                    aucReader = DBExecuteReader(ProviderType, aucCommand)

                                                    If Not aucReader Is Nothing Then
                                                        While aucReader.Read
                                                            AUC = ProjectManager.MeasureScales.GetAdvancedUtilityCurveByID(aucReader("UCID"))
                                                            If AUC IsNot Nothing Then
                                                                AUC.AddUCPoint(aucReader("XValue"), aucReader("YValue"))
                                                            End If
                                                        End While
                                                    End If
                                                    'aucCommand = Nothing 'C0235
                                                    aucReader.Close()

                                                    AdvancedUCLoaded = True
                                                End If

                                                'node.AdvancedUtilityCurveID = MSid 'C0073
                                                node.AdvancedUtilityCurveID(False) = MSid 'C0073

                                                '================
                                                'aucCommand = New odbc.odbcCommand("SELECT * FROM NonPairwiseData WHERE ParentNodeID=" + node.NodeID.ToString, aucConnection) 'C0235
                                                aucCommand.CommandText = "SELECT * FROM NonPairwiseData WHERE ParentNodeID=" + node.NodeID.ToString 'C0235
                                                aucReader = DBExecuteReader(ProviderType, aucCommand)

                                                Dim nonpwData As clsNonPairwiseMeasureData
                                                Dim ucDataValue As Single

                                                Dim userid As Integer

                                                Dim H1 As clsHierarchy
                                                Dim H2 As clsHierarchy
                                                Dim H3 As clsHierarchy

                                                Dim isAlt As Boolean
                                                Dim ParentNode As clsNode
                                                Dim tmpNode As clsNode

                                                If Not aucReader Is Nothing Then
                                                    While aucReader.Read
                                                        H1 = ProjectManager.Hierarchy(aucReader("HierarchyID"))
                                                        isAlt = CInt(aucReader("AltsHierarchyID")) <> -1
                                                        H2 = If(isAlt, ProjectManager.AltsHierarchy(CInt(aucReader("AltsHierarchyID"))), H1)

                                                        If (Not H1 Is Nothing) And (Not H2 Is Nothing) Then
                                                            ParentNode = H1.GetNodeByID(aucReader("ParentNodeID"))
                                                            tmpNode = H2.GetNodeByID(aucReader("NodeID"))
                                                            userid = CInt(aucReader("UserID"))
                                                            If (Not ParentNode Is Nothing) And (Not tmpNode Is Nothing) And ProjectManager.UserExists(userid) Then
                                                                nonpwData = Nothing
                                                                Select Case ParentNode.MeasureType
                                                                    Case ECMeasureType.mtAdvancedUtilityCurve
                                                                        If Not TypeOf (aucReader("Data")) Is DBNull Then
                                                                            ucDataValue = CSng(aucReader("Data"))
                                                                            nonpwData = New clsUtilityCurveMeasureData(tmpNode.NodeID, ParentNode.NodeID, userid, ucDataValue, ParentNode.MeasurementScale, Single.IsNaN(ucDataValue), aucReader("Comment"))
                                                                        End If
                                                                End Select

                                                                If nonpwData IsNot Nothing Then
                                                                    If Not TypeOf (aucReader("ModifyTime")) Is DBNull Then
                                                                        nonpwData.ModifyDate = aucReader("ModifyTime")
                                                                    Else
                                                                        nonpwData.ModifyDate = VERY_OLD_DATE
                                                                    End If

                                                                    H3 = If(ParentNode.IsTerminalNode,
                                                                    ParentNode.Hierarchy.ProjectManager.AltsHierarchy(ParentNode.Hierarchy.ProjectManager.ActiveAltsHierarchy),
                                                                    ParentNode.Hierarchy)

                                                                    'If ParentNode.GetNodesBelow.Clone.IndexOf(H3.GetNodeByID(nonpwData.NodeID)) <> -1 Then 'C0385
                                                                    'If ParentNode.GetNodesBelow.IndexOf(H3.GetNodeByID(nonpwData.NodeID)) <> -1 Then 'C0385 'C0450
                                                                    If ParentNode.GetNodesBelow(UNDEFINED_USER_ID).Contains(H3.GetNodeByID(nonpwData.NodeID)) Then 'C0450
                                                                        'ParentNode.Judgments.AddMeasureData(nonpwData) 'C0327
                                                                        ParentNode.Judgments.AddMeasureData(nonpwData, True) 'C0327
                                                                    End If
                                                                End If
                                                            End If
                                                        End If
                                                    End While
                                                End If

                                                aucReader.Close()
                                                aucCommand = Nothing
                                                '=============
                                            Else
                                                'node.MeasureType = ECMeasureType.mtDirect 'C0073
                                                node.MeasureType(False) = ECMeasureType.mtDirect 'C0073
                                            End If

                                            aucCommand = Nothing
                                        End Using
                                    Else
                                        'node.MeasureType = ECMeasureType.mtDirect 'C0073
                                        node.MeasureType(False) = ECMeasureType.mtDirect 'C0073
                                    End If
                                Case 5 'C0156
                                    node.MeasureType(False) = ECMeasureType.mtStep
                                    'C0300===
                                    Dim stepFunc As clsStepFunction = ProjectManager.MeasureScales.AddStepFunction
                                    stepFunc.Name = "Step Function for " + node.NodeName
                                    stepFunc.IsPiecewiseLinear = CBool(dbReader("PWLINEAR"))
                                    node.StepFunctionID(False) = stepFunc.ID
                                    'C0300==
                            End Select

                            If node.MeasureType = ECMeasureType.mtRegularUtilityCurve Then
                                Dim UC As clsRegularUtilityCurve

                                Dim ucLow As Single
                                Dim ucHigh As Single
                                Dim ucCurvature As Single

                                Dim str As String

                                If Not TypeOf (dbReader("LOW")) Is DBNull Then
                                    str = CStr(dbReader("LOW"))
                                    str = str.Replace(".", ",")

                                    'ucLow = Single.Parse(FixStringWithSingleValue(CStr(dbReader("LOW"))))
                                    ucLow = Single.Parse(FixStringWithSingleValue(str))
                                End If

                                If Not TypeOf (dbReader("HIGH")) Is DBNull Then
                                    str = CStr(dbReader("HIGH"))
                                    str = str.Replace(".", ",")
                                    'ucHigh = CSng(str)
                                    'ucHigh = Single.Parse(FixStringWithSingleValue(CStr(dbReader("HIGH"))))
                                    ucHigh = Single.Parse(FixStringWithSingleValue(str))
                                End If

                                If Not TypeOf (dbReader("CURVATURE")) Is DBNull Then
                                    str = CStr(dbReader("CURVATURE"))
                                    If str = "" Then
                                        ucCurvature = 0
                                    Else
                                        str = str.Replace(".", ",")

                                        'ucCurvature = CSng(str)
                                        'ucCurvature = Single.Parse(FixStringWithSingleValue(CStr(dbReader("CURVATURE"))))
                                        ucCurvature = Single.Parse(FixStringWithSingleValue(str))
                                    End If
                                Else
                                    ucCurvature = 0
                                End If

                                UC = ProjectManager.MeasureScales.AddRegularUtilityCurve
                                UC.Low = ucLow
                                UC.High = ucHigh
                                UC.Curvature = ucCurvature
                                UC.IsLinear = ucCurvature = 0

                                'UC.IsIncreasing = UC.GetValue(UC.Low) <= UC.GetValue(UC.High) 'C0068
                                UC.IsIncreasing = ucIncreasing 'C0070

                                'node.RegularUtilityCurveID = UC.ID 'C0073
                                node.RegularUtilityCurveID(False) = UC.ID 'C0073
                            End If
                            'Else 'C0382
                            'node.MeasureType = ECMeasureType.mtPairwise 'C0073
                            'node.MeasureType(False) = ECMeasureType.mtPairwise 'C0073 'C0382
                            'End If 'C0382
                        End If
                    End While

                    dbReader.Close()
                End If


                'Debug.Print("Start wrong nodes")
                Dim wrongNodes As New List(Of clsNode)
                For Each node In H.Nodes
                    If Not node.IsTerminalNode And node.MeasureType <> ECMeasureType.mtPairwise Then
                        wrongNodes.Add(node)
                        'Debug.Print(node.NodeID.ToString + " " + node.NodeName + " " + node.MeasureType.ToString)
                    End If
                Next
                'Debug.Print("End wrong nodes")

                'C0968===
                ' Case 1071 - forcing pairwise for non-covering objectives that are not pw or direct
                For Each node In H.Nodes
                    If Not node.IsTerminalNode Then
                        If node.MeasureType <> ECMeasureType.mtPairwise And node.MeasureType <> ECMeasureType.mtDirect Then
                            node.MeasureType = ECMeasureType.mtPairwise
                            'Debug.Print("Fixed node (forced pw): " + node.NodeName)
                        End If
                    End If
                Next
                'C0968==

                For Each node In H.Nodes
                    If Not node.IsTerminalNode And node.MeasureType = ECMeasureType.mtDirect Then
                        oCommand.CommandText = "SELECT COUNT(*) FROM AltsData WHERE WRT=?"
                        oCommand.Parameters.Clear()
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "WRT", "N" + node.NodeID.ToString))
                        Dim obj As Object = DBExecuteScalar(ProviderType, oCommand)
                        Dim count As Integer = If(obj Is Nothing, 0, CType(obj, Integer))

                        If count = 0 Then
                            oCommand.CommandText = "SELECT COUNT(*) FROM Judgments WHERE WRT=?"
                            oCommand.Parameters.Clear()
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "WRT", "N" + node.NodeID.ToString))
                            obj = DBExecuteScalar(ProviderType, oCommand)
                            count = If(obj Is Nothing, 0, CType(obj, Integer))

                            If count <> 0 Then
                                node.MeasureType = ECMeasureType.mtPairwise
                            End If
                        End If
                    End If
                Next


                ' load intensities
                'oCommand = New OleDb.OleDbCommand("SELECT * FROM Intensities", dbConnection)'Toodbc
                'oCommand = New odbc.odbcCommand("SELECT * FROM Intensities", dbConnection) 'Toodbc 'C0235
                oCommand.CommandText = "SELECT * FROM Intensities" 'C0235
                oCommand.Parameters.Clear()
                dbReader = DBExecuteReader(ProviderType, oCommand)

                Dim intensity As clsECIntensity
                Dim intensityList As New ArrayList

                Dim CommentFieldExists As Boolean = False
                For j As Integer = 0 To dbReader.FieldCount - 1
                    If dbReader.GetName(j).ToLower = "comment" Then
                        CommentFieldExists = True
                    End If
                Next

                If Not dbReader Is Nothing Then
                    While dbReader.Read
                        intensity = New clsECIntensity
                        intensity.ID = CInt(CStr(dbReader("IID")).Substring(1))
                        intensity.Name = CStr(dbReader("IntensityName"))
                        If CommentFieldExists Then
                            If Not TypeOf (dbReader("Comment")) Is DBNull Then
                                intensity.Comment = CStr(dbReader("Comment"))
                            End If
                        End If
                        intensityList.Add(intensity)
                    End While

                    dbReader.Close()
                End If

                ' load rating set
                Dim rating As clsRating
                Dim IName As String
                Dim IComment As String

                'For Each node In H.TerminalNodes.Clone
                'For Each node In H.TerminalNodes.Clone 'C0156 'C0385
                For Each node In H.TerminalNodes 'C0385
                    Select Case node.MeasureType
                        Case ECMeasureType.mtRatings
                            'oCommand = New odbc.odbcCommand("SELECT * FROM llCritIntensities WHERE WRT='N" + node.NodeID.ToString + "'", dbConnection) 'Toodbc 'C0235
                            oCommand.CommandText = "SELECT * FROM llCritIntensities WHERE WRT='N" + node.NodeID.ToString + "'" 'C0235
                            dbReader = DBExecuteReader(ProviderType, oCommand)

                            If Not dbReader Is Nothing Then
                                Dim rScale As clsRatingScale = ProjectManager.MeasureScales.AddRatingScale
                                rScale.Name = "Scale for " + node.NodeName
                                While dbReader.Read
                                    rating = rScale.AddIntensity()
                                    rating.ID = CInt(CStr(dbReader("IID")).Substring(1))

                                    IName = ""
                                    IComment = ""
                                    For Each intens As clsECIntensity In intensityList
                                        If intens.ID = rating.ID Then
                                            IName = intens.Name
                                            IComment = intens.Comment
                                        End If
                                    Next

                                    'rating.Name = dbReader("IID") + " - " + IName
                                    rating.Name = IName
                                    rating.Comment = IComment
                                    rating.Value = CSng(dbReader("Lpriority"))

                                End While

                                'node.RatingScaleID = rScale.ID 'C0073
                                node.RatingScaleID(False) = rScale.ID 'C0073

                                dbReader.Close()

                                ' sort ratings
                                rScale.Sort(True)
                            End If
                        'oCommand = Nothing 'C0235
                        Case ECMeasureType.mtStep 'C0156
                            'oCommand = New odbc.odbcCommand("SELECT * FROM llCritIntensities WHERE WRT='N" + node.NodeID.ToString + "'", dbConnection) 'C0235
                            oCommand.CommandText = "SELECT * FROM llCritIntensities WHERE WRT='N" + node.NodeID.ToString + "'" 'C0235
                            dbReader = DBExecuteReader(ProviderType, oCommand)

                            Dim interval As clsStepInterval
                            If Not dbReader Is Nothing Then
                                'C0300===
                                'Dim stepFunc As clsStepFunction = ProjectManager.MeasureScales.AddStepFunction
                                'stepFunc.Name = "Step Function for " + node.NodeName
                                Dim stepFunc As clsStepFunction = CType(node.MeasurementScale, clsStepFunction)
                                'C0300==
                                While dbReader.Read
                                    interval = stepFunc.AddInterval
                                    interval.ID = CInt(CStr(dbReader("IID")).Substring(1))

                                    IName = ""
                                    IComment = ""
                                    For Each intens As clsECIntensity In intensityList
                                        If intens.ID = interval.ID Then
                                            IName = intens.Name
                                            IComment = intens.Comment
                                        End If
                                    Next

                                    interval.Name = IName
                                    interval.Comment = IComment
                                    interval.Low = CSng(dbReader("StepGE"))
                                    interval.Value = CSng(dbReader("Lpriority"))

                                End While

                                'node.StepFunctionID(False) = stepFunc.ID 'C0300

                                dbReader.Close()

                                ' sort ratings
                                'stepFunc.Sort(True) 'C0158
                                stepFunc.SortByInterval(True) 'C0158

                                'fix step functions
                                Dim low As Single = CType(stepFunc.Intervals(0), clsStepInterval).Low

                                'If low <> 0 Then 'C0158
                                'If low <> NEGATIVE_INFINITY Then 'C0158
                                '    interval = stepFunc.AddInterval
                                '    'interval.Low = 0 'C0158
                                '    interval.Low = NEGATIVE_INFINITY 'C0158
                                '    interval.High = low
                                '    interval.Value = 0

                                '    'stepFunc.Sort(True) 'C0158
                                '    stepFunc.SortByInterval(True) 'C0158
                                'End If

                                For i = 0 To stepFunc.Intervals.Count - 2
                                    CType(stepFunc.Intervals(i), clsStepInterval).High = CType(stepFunc.Intervals(i + 1), clsStepInterval).Low
                                Next
                                'CType(stepFunc.Intervals(stepFunc.Intervals.Count - 1), clsStepInterval).High = 1 'C0158
                                CType(stepFunc.Intervals(stepFunc.Intervals.Count - 1), clsStepInterval).High = POSITIVE_INFINITY 'C0158

                            End If
                            'oCommand = Nothing 'C0235
                    End Select
                Next

                For Each node In H.Nodes
                    If node.MeasureType = ECMeasureType.mtRatings Then
                        If node.MeasurementScale IsNot Nothing Then
                            CType(node.MeasurementScale, clsRatingScale).Comment = node.TempRatingScaleInfoDoc
                        End If
                    End If
                Next

                LoadGroups() 'ASGroups
                LoadGroupsRoles() 'ASGroups

                'If ProjectManager.LoadOnDemand Then
                If False Then
                    If ProjectManager.User IsNot Nothing Then 'C0772
                        LoadUserJudgments(ProjectManager.User)
                        LoadUserPermissions(ProjectManager.User)
                    End If
                Else
                    LoadUserJudgments(Nothing, False) 'C0120
                    LoadUserPermissions(Nothing)
                End If
                'C0081==

                For Each node In wrongNodes
                    If node.MeasureType = ECMeasureType.mtPairwise Then
                        Dim uJudgments As New Dictionary(Of Integer, List(Of clsDirectMeasureData))
                        For Each U As clsUser In ProjectManager.UsersList
                            If node.Judgments.JudgmentsFromUser(U.UserID).Count > 0 Then
                                Dim dJudgments As New List(Of clsDirectMeasureData)
                                uJudgments.Add(U.UserID, dJudgments)
                                node.Judgments.CalculateWeights(New clsCalculationTarget(CalculationTargetType.cttUser, U))
                                For Each child As clsNode In node.Children
                                    Dim dData As New clsDirectMeasureData(child.NodeID, node.NodeID, U.UserID, child.LocalPriority(U.UserID))
                                    dJudgments.Add(dData)
                                Next
                            End If
                        Next
                        node.MeasureType = ECMeasureType.mtDirect
                        For Each dJudgments As List(Of clsDirectMeasureData) In uJudgments.Values
                            For Each dJ As clsDirectMeasureData In dJudgments
                                node.Judgments.AddMeasureData(dJ, True)
                            Next
                        Next
                    End If
                Next

                If H IsNot Nothing Then
                    For Each node In H.Nodes
                        node.Tag = Nothing
                    Next
                End If
                'C0382==

                ProjectManager.ExtraAHPTables = LoadExtraTables() 'C0342
                LoadAdvancedInfoDocs() 'C0948

                ' Load RA
                ProjectManager.InitRA()

                LoadRAScenarios()
                LoadMustsMustnots()
                LoadConstraints()
                LoadBudgetLimits()
                LoadRAGroups()
                LoadRADependencies()
                LoadRASettings()
                LoadRARisks()
                LoadRACosts()
                LoadRAFundingPools()
                ProjectManager.ResourceAligner.isLoading = False ' D3240

                oCommand = Nothing 'C0235
            End Using
            Return True
        End Function

        Public Function LoadExtraTables() As Stream
            If Not CheckDBConnection(ProviderType, Location) Then Return Nothing

            Dim MS As New MemoryStream
            Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()

                Dim dbReader As DbDataReader
                Dim oCommand As DbCommand = GetDBCommand(ProviderType)

                oCommand.Connection = dbConnection

                Dim DS As New DataSet
                Dim DT As DataTable
                For Each tName As String In _AHP_EXTRATABLES_LIST
                    If TableExists(Location, ProviderType, tName) Then
                        oCommand.CommandText = "SELECT * FROM " + tName
                        dbReader = DBExecuteReader(ProviderType, oCommand)
                        DT = New DataTable(tName)
                        DT.Load(dbReader)

                        DS.Tables.Add(DT)
                    End If
                Next

                DS.WriteXml(MS, XmlWriteMode.WriteSchema)

            End Using
            Return MS
        End Function

        Private Function LoadAdvancedInfoDocs() As Boolean 'C0948
            If (ProjectManager Is Nothing) OrElse Not CheckDBConnection(ProviderType, Location) Then Return False

            If Not TableExists(Location, ProviderType, "AdvancedInfoDocs") Then Return False

            Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()

                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection

                oCommand.CommandText = "SELECT * FROM AdvancedInfoDocs"

                Dim dbReader As DbDataReader
                dbReader = DBExecuteReader(ProviderType, oCommand)

                If dbReader IsNot Nothing Then
                    While dbReader.Read
                        Dim docType As InfoDocType = CType(dbReader("DocType"), InfoDocType)
                        Dim targetID As Integer = CType(dbReader("TargetID"), Integer)
                        Dim targetOwnerID As Integer = CType(dbReader("TargetOwnerID"), Integer)
                        Dim additionalID As Integer = CType(dbReader("AdditionalID"), Integer)
                        Dim additionalOwnerID As Integer = CType(dbReader("AdditionalOwnerID"), Integer)
                        Dim infoDoc As String = CType(dbReader("InfoDoc"), String)

                        Dim targetNode As clsNode = Nothing
                        If targetOwnerID <> -1 Then
                            targetNode = ProjectManager.GetAnyHierarchyByID(targetOwnerID).GetNodeByID(targetID)
                        End If

                        Dim additionalNode As clsNode = Nothing
                        If additionalOwnerID <> -1 Then
                            additionalNode = ProjectManager.GetAnyHierarchyByID(additionalOwnerID).GetNodeByID(additionalID)
                        End If

                        Dim targetGuid As Guid
                        If targetNode IsNot Nothing Then
                            targetGuid = targetNode.NodeGuidID
                        Else
                            targetGuid = Guid.Empty
                        End If

                        Dim additionalGuid As Guid
                        If additionalNode IsNot Nothing Then
                            additionalGuid = additionalNode.NodeGuidID
                        Else
                            additionalGuid = Guid.Empty
                        End If

                        Dim NewInfoDoc As New clsInfoDoc(docType, targetGuid, additionalGuid, infoDoc)
                        ProjectManager.InfoDocs.InfoDocs.Add(NewInfoDoc)

                        If NewInfoDoc.DocumentType = InfoDocType.idtNode And targetNode IsNot Nothing Then
                            targetNode.InfoDoc = infoDoc
                        End If
                    End While
                End If

                dbReader.Close()
            End Using
            Return True
        End Function

        Private Function LoadUserPermissions(Optional ByVal AUser As ECCore.ECTypes.clsUser = Nothing) As Boolean 'ASGroups
            If (ProjectManager Is Nothing) OrElse Not CheckDBConnection(ProviderType, Location) Then
                Return False
            End If

            Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                Dim dbReader As DbDataReader
                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection

                dbConnection.Open()

                Dim nid As Integer
                Dim aid As Integer
                Dim pid As Integer

                Dim node As clsNode = Nothing
                Dim alt As clsNode = Nothing

                If TableExists(Location, ProviderType, "RoleEvaluateNodes") Then
                    If AUser IsNot Nothing Then
                        oCommand.CommandText = "SELECT * FROM RoleEvaluateNodes WHERE PID=" + AUser.UserID.ToString
                    Else
                        oCommand.CommandText = "SELECT * FROM RoleEvaluateNodes"
                    End If
                    dbReader = DBExecuteReader(ProviderType, oCommand)

                    Dim fieldsChecked As Boolean = False
                    Dim RoleStateFieldExists As Boolean = False
                    Dim RoleTypeFieldExists As Boolean = False


                    While dbReader.Read
                        If Not fieldsChecked Then
                            For j As Integer = 0 To dbReader.FieldCount - 1
                                If dbReader.GetName(j).ToLower = "rolestate" Then
                                    RoleStateFieldExists = True
                                End If
                                If dbReader.GetName(j).ToLower = "roletype" Then
                                    RoleTypeFieldExists = True
                                End If
                            Next
                            fieldsChecked = True
                        End If

                        If Not TypeOf (dbReader("NID")) Is DBNull Then
                            nid = CInt(CStr(dbReader("NID")).Substring(1))
                            node = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).GetNodeByID(nid)
                        End If
                        If Not node Is Nothing Then
                            pid = CInt(dbReader("PID"))
                            If ProjectManager.UserExists(pid) Then
                                If Not RoleStateFieldExists Or Not RoleTypeFieldExists Then
                                    ' if new roles fields do not exist, then load old way
                                    ProjectManager.UsersRoles.SetObjectivesRoles(pid, node.NodeGuidID, RolesValueType.rvtRestricted)
                                Else
                                    ' load new way
                                    Dim roleState As Integer
                                    If Not TypeOf (dbReader("RoleState")) Is DBNull Then
                                        roleState = CInt(dbReader("RoleState"))

                                        Select Case roleState
                                            Case 1 ' allowed
                                                ProjectManager.UsersRoles.SetObjectivesRoles(pid, node.NodeGuidID, RolesValueType.rvtAllowed)
                                            Case 2 ' restricted
                                                ProjectManager.UsersRoles.SetObjectivesRoles(pid, node.NodeGuidID, RolesValueType.rvtRestricted)
                                        End Select
                                    End If

                                    Dim roleType As Integer
                                    If Not TypeOf (dbReader("RoleType")) Is DBNull Then
                                        roleType = CInt(dbReader("RoleType"))
                                    End If

                                End If
                            End If
                        End If
                    End While
                    dbReader.Close()
                End If

                If TableExists(Location, ProviderType, "RoleEvaluateJD") Then
                    If AUser IsNot Nothing Then
                        oCommand.CommandText = "SELECT * FROM RoleEvaluateJD WHERE PID=" + AUser.UserID.ToString
                    Else
                        oCommand.CommandText = "SELECT * FROM RoleEvaluateJD"
                    End If
                    dbReader = DBExecuteReader(ProviderType, oCommand)

                    Dim fieldsChecked As Boolean = False
                    Dim RoleStateFieldExists As Boolean = False
                    Dim RoleTypeFieldExists As Boolean = False

                    While dbReader.Read
                        If Not fieldsChecked Then
                            For j As Integer = 0 To dbReader.FieldCount - 1
                                If dbReader.GetName(j).ToLower = "rolestate" Then
                                    RoleStateFieldExists = True
                                End If
                                If dbReader.GetName(j).ToLower = "roletype" Then
                                    RoleTypeFieldExists = True
                                End If
                            Next
                            fieldsChecked = True
                        End If

                        If Not TypeOf (dbReader("PID")) Is DBNull Then
                            pid = CInt(dbReader("PID"))
                        End If

                        node = Nothing
                        alt = Nothing

                        If Not TypeOf (dbReader("WRT")) Is DBNull Then
                            If CStr(dbReader("WRT")) <> "" Then
                                nid = CInt(CStr(dbReader("WRT")).Substring(1))
                                node = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).GetNodeByID(nid)
                            End If
                        End If

                        If Not TypeOf (dbReader("EID")) Is DBNull Then
                            If CStr(dbReader("EID")) <> "" Then
                                aid = CInt(CStr(dbReader("EID")).Substring(1))
                                alt = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(aid)
                            End If
                        End If

                        If (Not node Is Nothing) And (Not alt Is Nothing) And ProjectManager.UserExists(pid) Then
                            If Not RoleStateFieldExists Or Not RoleTypeFieldExists Then
                                ' load old way
                                ProjectManager.UsersRoles.SetAlternativesRoles(pid, node.NodeGuidID, alt.NodeGuidID, RolesValueType.rvtRestricted) 'C0901 'C1061
                            Else
                                ' load new way
                                Dim roleState As Integer
                                If Not TypeOf (dbReader("RoleState")) Is DBNull Then
                                    roleState = CInt(dbReader("RoleState"))
                                    Select Case roleState
                                        Case 1 ' allowed
                                            ProjectManager.UsersRoles.SetAlternativesRoles(pid, node.NodeGuidID, alt.NodeGuidID, RolesValueType.rvtAllowed)
                                        Case 2 ' restricted
                                            ProjectManager.UsersRoles.SetAlternativesRoles(pid, node.NodeGuidID, alt.NodeGuidID, RolesValueType.rvtRestricted)
                                    End Select
                                End If

                                Dim roleType As Integer
                                If Not TypeOf (dbReader("RoleType")) Is DBNull Then
                                    roleType = CInt(dbReader("RoleType"))
                                End If

                            End If
                        End If
                    End While
                    dbReader.Close()
                End If
            End Using
            Return True
        End Function

        Private Function LoadGroups() As Boolean 'ASGroups
            If (ProjectManager Is Nothing) OrElse Not CheckDBConnection(ProviderType, Location) Then Return False

            Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                Dim dbReader As DbDataReader
                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection

                dbConnection.Open()

                Dim GroupID As Integer = -1
                Dim GroupName As String = ""

                If TableExists(Location, ProviderType, "PeopleGroups") Then
                    oCommand.CommandText = "SELECT * FROM PeopleGroups"
                    dbReader = DBExecuteReader(ProviderType, oCommand)

                    ProjectManager.CombinedGroups.GroupsList.Clear()

                    While dbReader.Read
                        If Not TypeOf (dbReader("PGroupID")) Is DBNull Then
                            GroupID = CInt(dbReader("PGroupID"))
                        End If
                        If Not TypeOf (dbReader("PGroupName")) Is DBNull Then
                            GroupName = CStr(dbReader("PGroupName"))
                        End If

                        Dim CG As clsCombinedGroup = ProjectManager.CombinedGroups.GetGroupByID(GroupID)
                        If CG Is Nothing Then
                            CG = ProjectManager.CombinedGroups.AddCombinedGroup(GroupName)
                            CG.ID = GroupID
                            If GroupID = 1 Then
                                CG.CombinedUserID = COMBINED_USER_ID
                            End If
                        End If
                    End While
                    dbReader.Close()
                End If

                If TableExists(Location, ProviderType, "PeopleGroupsMembers") Then
                    oCommand.CommandText = "SELECT * FROM PeopleGroupsMembers"
                    dbReader = DBExecuteReader(ProviderType, oCommand)

                    Dim UserID As Integer

                    While dbReader.Read
                        If Not TypeOf (dbReader("PGroupID")) Is DBNull Then
                            GroupID = CInt(dbReader("PGroupID"))
                        End If
                        If Not TypeOf (dbReader("PID")) Is DBNull Then
                            UserID = CInt(dbReader("PID"))
                        End If

                        Dim CG As clsCombinedGroup = ProjectManager.CombinedGroups.GetGroupByID(GroupID)
                        Dim user As clsUser = ProjectManager.GetUserByID(UserID)
                        If CG IsNot Nothing And user IsNot Nothing Then
                            If Not CG.ContainsUser(user.UserID) Then
                                CG.UsersList.Add(user)
                            End If
                        End If
                    End While
                    dbReader.Close()
                End If

            End Using
            Return True
        End Function

        Public Function LoadGroupsRoles() As Boolean 'ASGroups
            If (ProjectManager Is Nothing) OrElse Not CheckDBConnection(ProviderType, Location) Then Return False

            Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                Dim dbReader As DbDataReader
                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection

                dbConnection.Open()

                Dim GroupID As Integer
                Dim WRT As String
                Dim EID As String
                Dim RoleState As Integer
                Dim RoleType As Integer

                If TableExists(Location, ProviderType, "RolesForGroups") Then
                    oCommand.CommandText = "SELECT * FROM RolesForGroups"
                    dbReader = DBExecuteReader(ProviderType, oCommand)

                    While dbReader.Read
                        GroupID = CInt(dbReader("GroupID"))
                        WRT = CStr(dbReader("WRT"))
                        EID = CStr(dbReader("EID"))
                        RoleState = CInt(dbReader("RoleState"))
                        RoleType = CInt(dbReader("RoleType"))

                        Dim CG As clsCombinedGroup = ProjectManager.CombinedGroups.GetGroupByID(GroupID)
                        If CG IsNot Nothing Then
                            Dim node As clsNode = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).GetNodeByID(CInt(WRT.Substring(1)))
                            If node IsNot Nothing Then
                                If node.IsTerminalNode And (EID <> "") Then
                                    Dim alt As clsNode = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(CInt(EID.Substring(1)))
                                    If alt IsNot Nothing Then
                                        Select Case RoleState
                                            Case 0
                                                If CG.CombinedUserID = COMBINED_USER_ID Then
                                                    ProjectManager.UsersRoles.SetAlternativesRoles(CG.CombinedUserID, node.NodeGuidID, alt.NodeGuidID, RolesValueType.rvtUndefined)
                                                End If
                                            Case 1
                                                ProjectManager.UsersRoles.SetAlternativesRoles(CG.CombinedUserID, node.NodeGuidID, alt.NodeGuidID, RolesValueType.rvtAllowed)
                                            Case 2
                                                ProjectManager.UsersRoles.SetAlternativesRoles(CG.CombinedUserID, node.NodeGuidID, alt.NodeGuidID, RolesValueType.rvtRestricted)
                                        End Select
                                    End If
                                Else
                                    Select Case RoleState
                                        Case 0
                                            If CG.CombinedUserID = COMBINED_USER_ID Then
                                                ProjectManager.UsersRoles.SetObjectivesRoles(CG.CombinedUserID, node.NodeGuidID, RolesValueType.rvtUndefined)
                                            End If
                                        Case 1
                                            ProjectManager.UsersRoles.SetObjectivesRoles(CG.CombinedUserID, node.NodeGuidID, RolesValueType.rvtAllowed)
                                        Case 2
                                            ProjectManager.UsersRoles.SetObjectivesRoles(CG.CombinedUserID, node.NodeGuidID, RolesValueType.rvtRestricted)
                                    End Select
                                End If
                            End If
                        End If
                    End While
                    dbReader.Close()
                End If
            End Using

            Return True
        End Function

        Private Function LoadConstraints() As Boolean
            If (ProjectManager Is Nothing) OrElse Not CheckDBConnection(ProviderType, Location) Then Return False

            Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                Dim dbReader As DbDataReader
                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection

                dbConnection.Open()

                Dim AID As Integer
                Dim CName As String
                Dim value As Double
                Dim alt As clsNode

                Dim RA As Canvas.ResourceAligner = ProjectManager.ResourceAligner

                If TableExists(Location, ProviderType, "RAconstraints") Then
                    oCommand.CommandText = "SELECT * FROM RAconstraints ORDER BY SOrder" 'AS/2-9-16b added Order By
                    dbReader = DBExecuteReader(ProviderType, oCommand)

                    Dim constraint As RAConstraint
                    Dim aidStr As String
                    Dim psid As Integer
                    Dim CCID As String 'AS/11-4-15

                    While dbReader.Read

                        psid = CInt(dbReader("PSID"))
                        aidStr = CStr(dbReader("AID"))
                        If Not TypeOf (dbReader("value1")) Is DBNull Then
                            If Double.TryParse(dbReader("value1"), value) Then
                                CName = dbReader("RAConstraint").ToString
                                CCID = dbReader("CCID").ToString 'AS/11-4-15 'AS/11-20-15a here and below replaced CStr function with .ToString

                                If RA.Scenarios.Scenarios.ContainsKey(psid) Then
                                    Dim scenario As RAScenario = RA.Scenarios.Scenarios(psid)
                                    'constraint = scenario.Constraints.GetConstraintByName(CName) 'AS/11-4-15
                                    'If constraint Is Nothing Then constraint = scenario.Constraints.AddConstraint(CName) 'AS/11-4-15
                                    constraint = scenario.Constraints.GetConstraintByCCID(CCID) 'AS/11-4-15===
                                    If constraint Is Nothing Then
                                        constraint = scenario.Constraints.AddConstraint(CCID, CName)
                                        constraint.ECD_SOrder = dbReader("SOrder").ToString 'AS/11-18-15===
                                        constraint.ECD_AID = dbReader("AID").ToString
                                        constraint.ECD_AssociatedUDcolKey = dbReader("AssociatedUDcolKey").ToString
                                        If dbReader.GetSchemaTable.Columns.Contains("AssociatedCVID") Then constraint.ECD_AssociatedCVID = dbReader("AssociatedCVID").ToString ' D3648 
                                        If dbReader.GetSchemaTable.Columns.Contains("ECC_LinkedAttributeID") Then   ' D3648
                                            If dbReader("ECC_LinkedAttributeID").ToString <> "" Then 'AS/11-19-15 enclosed
                                                constraint.LinkedAttributeID = Guid.Parse(dbReader("ECC_LinkedAttributeID").ToString)
                                            End If 'AS/11-19-15
                                            If dbReader("ECC_LinkedEnumID").ToString <> "" Then 'AS/11-19-15 enclosed
                                                constraint.LinkedEnumID = Guid.Parse(dbReader("ECC_LinkedEnumID").ToString) 'AS/11-18-15==
                                            End If
                                        End If
                                    End If 'AS/11-4-15==

                                    If aidStr.ToLower = "min" Then
                                        constraint.MinValue = value
                                    Else
                                        If aidStr.ToLower = "max" Then
                                            constraint.MaxValue = value
                                        Else
                                            Dim s As String = aidStr.Substring(1)
                                            If Integer.TryParse(s, AID) Then
                                                alt = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(AID)
                                                If alt IsNot Nothing Then
                                                    If constraint IsNot Nothing Then
                                                        scenario.Constraints.SetConstraintValue(constraint.ID, alt.NodeGuidID.ToString, value)
                                                    End If
                                                End If
                                            End If
                                        End If
                                    End If
                                End If
                            End If
                        Else 'AS/9-2-15===
                            CName = dbReader("RAConstraint").ToString
                            CCID = dbReader("CCID").ToString 'AS/11-4-15

                            If RA.Scenarios.Scenarios.ContainsKey(psid) Then
                                Dim scenario As RAScenario = RA.Scenarios.Scenarios(psid)
                                'constraint = scenario.Constraints.GetConstraintByName(CName) 'AS/11-4-15
                                'If constraint Is Nothing Then constraint = scenario.Constraints.AddConstraint(CName) 'AS/11-4-15
                                constraint = scenario.Constraints.GetConstraintByCCID(CCID) 'AS/11-4-15
                                If constraint Is Nothing Then
                                    constraint = scenario.Constraints.AddConstraint(CCID, CName) 'AS/11-4-15
                                    constraint.ECD_SOrder = dbReader("SOrder").ToString 'AS/11-18-15===
                                    constraint.ECD_AID = dbReader("AID").ToString
                                    constraint.ECD_AssociatedUDcolKey = dbReader("AssociatedUDcolKey").ToString
                                    If dbReader.GetSchemaTable.Columns.Contains("AssociatedCVID") Then constraint.ECD_AssociatedCVID = dbReader("AssociatedCVID").ToString ' D3648
                                    If dbReader.GetSchemaTable.Columns.Contains("ECC_LinkedAttributeID") Then   ' D3648
                                        If dbReader("ECC_LinkedAttributeID").ToString <> "" Then 'AS/11-19-15 enclosed
                                            constraint.LinkedAttributeID = Guid.Parse(dbReader("ECC_LinkedAttributeID").ToString)
                                        End If 'AS/11-19-15
                                        If dbReader("ECC_LinkedEnumID").ToString <> "" Then 'AS/11-19-15 enclosed
                                            constraint.LinkedEnumID = Guid.Parse(dbReader("ECC_LinkedEnumID").ToString) 'AS/11-18-15==
                                        End If
                                    End If
                                End If
                            End If
                        End If 'AS/9-2-15==

                    End While
                    dbReader.Close()
                End If
            End Using

            Return True
        End Function

        Private Function LoadBudgetLimits() As Boolean
            If (ProjectManager Is Nothing) OrElse Not CheckDBConnection(ProviderType, Location) Then Return False

            Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                Dim dbReader As DbDataReader
                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection

                dbConnection.Open()

                Dim PSid As Integer
                Dim limit As String
                Dim limitValue As Double

                If TableExists(Location, ProviderType, "RABudgetLimits") Then
                    oCommand.CommandText = "SELECT * FROM RABudgetLimits"
                    dbReader = DBExecuteReader(ProviderType, oCommand)

                    While dbReader.Read
                        PSid = CInt(dbReader("PSid"))
                        limit = CStr(dbReader("Limit"))
                        limitValue = Double.Parse(limit, Globalization.NumberStyles.Currency)
                        If ProjectManager.ResourceAligner.Scenarios.Scenarios.ContainsKey(PSid) Then
                            Dim scenario As RAScenario = ProjectManager.ResourceAligner.Scenarios.Scenarios(PSid)
                            scenario.Budget = limitValue
                        End If
                    End While
                    dbReader.Close()
                End If
            End Using

            Return True
        End Function

        Private Function LoadMustsMustnots() As Boolean
            If (ProjectManager Is Nothing) OrElse Not CheckDBConnection(ProviderType, Location) Then Return False

            Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                Dim dbReader As DbDataReader
                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection

                dbConnection.Open()

                Dim AID As Integer
                Dim value As Double
                Dim alt As clsNode
                Dim psID As Integer

                Dim RA As Canvas.ResourceAligner = ProjectManager.ResourceAligner

                If TableExists(Location, ProviderType, "MustsMustNots") Then
                    oCommand.CommandText = "SELECT * FROM MustsMustNots"
                    dbReader = DBExecuteReader(ProviderType, oCommand)

                    While dbReader.Read
                        psID = CInt(dbReader("PSID"))
                        If RA.Scenarios.Scenarios.ContainsKey(psID) Then
                            Dim scenario As RAScenario = RA.Scenarios.Scenarios(psID)
                            AID = dbReader("row")
                            alt = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(AID)
                            If alt IsNot Nothing Then
                                Dim raAlt As Canvas.RAAlternative = Nothing
                                For Each rAlt As Canvas.RAAlternative In scenario.AlternativesFull
                                    If rAlt.ID.ToLower = alt.NodeGuidID.ToString.ToLower Then
                                        raAlt = rAlt
                                    End If
                                Next
                                If raAlt IsNot Nothing Then
                                    If Not TypeOf (dbReader("PartialMinPct")) Is DBNull Then
                                        If Double.TryParse(FixStringWithSingleValue(dbReader("PartialMinPct")), value) Then
                                            raAlt.IsPartial = True
                                            raAlt.MinPercent = value
                                        End If
                                    End If
                                    If (TypeOf (dbReader("Must")) Is DBNull OrElse dbReader("Must") = 0) And (TypeOf (dbReader("MustNot")) Is DBNull OrElse dbReader("MustNot") = 0) Then
                                        If Not raAlt.IsPartial Then 'AS/10-26-15 enclosed
                                            raAlt.MustNot = True
                                        End If 'AS/10-26-15
                                    End If
                                    If (Not TypeOf (dbReader("Must")) Is DBNull AndAlso dbReader("Must") = 1) And (Not TypeOf (dbReader("MustNot")) Is DBNull AndAlso dbReader("MustNot") = 1) Then
                                        raAlt.Must = True
                                    ElseIf (Not TypeOf (dbReader("Must")) Is DBNull AndAlso dbReader("Must") = 1) And (Not TypeOf (dbReader("MustNot")) Is DBNull AndAlso dbReader("MustNot") = 0) Then 'AS/6-29-16===
                                        raAlt.Must = True
                                    ElseIf (Not TypeOf (dbReader("Must")) Is DBNull AndAlso dbReader("Must") = 1) And (TypeOf (dbReader("MustNot")) Is DBNull) Then
                                        raAlt.Must = True 'AS/6-29-16==
                                    End If
                                End If
                            End If
                        End If
                    End While
                    dbReader.Close()
                End If
            End Using

            Return True
        End Function

        Private Function LoadRAGroups() As Boolean
            If (ProjectManager Is Nothing) OrElse Not CheckDBConnection(ProviderType, Location) Then Return False

            Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                Dim dbReader As DbDataReader
                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection

                dbConnection.Open()

                Dim GID As Integer
                Dim AID As Integer
                Dim group As RAGroup
                Dim PSID As Integer

                Dim RA As Canvas.ResourceAligner = ProjectManager.ResourceAligner

                If TableExists(Location, ProviderType, "Groups") Then
                    oCommand.CommandText = "SELECT * FROM Groups"
                    dbReader = DBExecuteReader(ProviderType, oCommand)

                    While dbReader.Read
                        PSID = CInt(dbReader("PSID"))
                        If RA.Scenarios.Scenarios.ContainsKey(PSID) Then
                            Dim scenario As RAScenario = RA.Scenarios.Scenarios(PSID)
                            Dim s As String = CStr(dbReader("GID")).Substring(1)
                            If Integer.TryParse(s, GID) Then
                                group = scenario.Groups.AddGroup(, dbReader("Groupname"))
                                If group IsNot Nothing Then
                                    group.IntID = GID
                                    group.Condition = CType(CInt(dbReader("Type")) - 1, RAGroupCondition)
                                End If
                            End If
                        End If
                    End While
                    dbReader.Close()
                End If

                If TableExists(Location, ProviderType, "GroupMembers") Then
                    oCommand.CommandText = "SELECT * FROM GroupMembers"
                    dbReader = DBExecuteReader(ProviderType, oCommand)

                    While dbReader.Read
                        PSID = CInt(dbReader("PSID"))
                        If RA.Scenarios.Scenarios.ContainsKey(PSID) Then
                            Dim scenario As RAScenario = RA.Scenarios.Scenarios(PSID)
                            Dim s As String = CStr(dbReader("GID")).Substring(1)
                            If Integer.TryParse(s, GID) Then
                                group = scenario.Groups.GetGroupByIntID(GID)
                                If group IsNot Nothing Then
                                    s = CStr(dbReader("AID")).Substring(1)
                                    Dim alt As clsNode = Nothing
                                    If Integer.TryParse(s, AID) Then
                                        alt = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(AID)
                                    End If
                                    If alt IsNot Nothing Then
                                        Dim raAlt As Canvas.RAAlternative = Nothing
                                        For Each rAlt As Canvas.RAAlternative In scenario.AlternativesFull
                                            If rAlt.ID.ToLower = alt.NodeGuidID.ToString.ToLower Then
                                                raAlt = rAlt
                                            End If
                                        Next
                                        If raAlt IsNot Nothing Then
                                            group.Alternatives.Add(raAlt.ID, raAlt)
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    End While
                    dbReader.Close()
                End If
            End Using

            Return True
        End Function

        Private Function LoadRADependencies() As Boolean
            If (ProjectManager Is Nothing) OrElse Not CheckDBConnection(ProviderType, Location) Then Return False

            Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                Dim dbReader As DbDataReader
                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection

                dbConnection.Open()

                Dim RA As Canvas.ResourceAligner = ProjectManager.ResourceAligner

                If TableExists(Location, ProviderType, "Dependencies") Then
                    oCommand.CommandText = "SELECT * FROM Dependencies"
                    dbReader = DBExecuteReader(ProviderType, oCommand)

                    While dbReader.Read
                        Dim psid As Integer = CInt(dbReader("PSID"))
                        If RA.Scenarios.Scenarios.ContainsKey(psid) Then
                            Dim scenario As RAScenario = RA.Scenarios.Scenarios(psid)
                            Dim row As Integer = dbReader("row")
                            Dim col As Integer = dbReader("RAcolumn")
                            Dim value As String = dbReader("dependency")

                            Dim alt1 As clsNode = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(row)
                            Dim alt2 As clsNode = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(col)
                            If alt1 IsNot Nothing And alt2 IsNot Nothing Then
                                Dim raAlt1 As Canvas.RAAlternative = Nothing
                                Dim raAlt2 As Canvas.RAAlternative = Nothing
                                For Each rAlt As Canvas.RAAlternative In scenario.AlternativesFull
                                    If rAlt.ID.ToLower = alt1.NodeGuidID.ToString.ToLower Then
                                        raAlt1 = rAlt
                                    End If
                                    If rAlt.ID.ToLower = alt2.NodeGuidID.ToString.ToLower Then
                                        raAlt2 = rAlt
                                    End If
                                Next
                                If raAlt1 IsNot Nothing AndAlso raAlt2 IsNot Nothing Then
                                    Select Case value.ToLower
                                        Case "m"
                                            scenario.Dependencies.SetDependency(raAlt1.ID, raAlt2.ID, Canvas.RADependencyType.dtMutuallyDependent)
                                        Case "d"
                                            scenario.Dependencies.SetDependency(raAlt1.ID, raAlt2.ID, Canvas.RADependencyType.dtDependsOn)
                                        Case "x"
                                            scenario.Dependencies.SetDependency(raAlt1.ID, raAlt2.ID, Canvas.RADependencyType.dtMutuallyExclusive)
                                    End Select
                                End If
                            End If
                        End If
                    End While
                    dbReader.Close()
                End If
            End Using

            Return True
        End Function

        Private Function LoadRASettings() As Boolean 'RA_AS2
            If (ProjectManager Is Nothing) OrElse Not CheckDBConnection(ProviderType, Location) Then Return False

            Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                Dim dbReader As DbDataReader
                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection

                dbConnection.Open()

                Dim Name As String
                Dim Value As String

                If TableExists(Location, ProviderType, "RAproperties") Then
                    oCommand.CommandText = "SELECT * FROM RAproperties"
                    dbReader = DBExecuteReader(ProviderType, oCommand)

                    Dim RA As Canvas.ResourceAligner = ProjectManager.ResourceAligner
                    Dim SOrder As Integer 'AS/4-8-16===
                    While dbReader.Read
                        If CStr(dbReader("PropertyName")).ToLower = "sortraalternativesorder" Then
                            SOrder = If(CInt(dbReader("PValue")) Mod 2 = 0, -1, 0) 'odd - ascending, even - descending
                        End If
                    End While
                    dbReader.Close() 'close and reopen to start reading again from the beginning
                    dbReader = DBExecuteReader(ProviderType, oCommand) 'AS/4-8-16==

                    While dbReader.Read
                        Dim psid As Integer = CInt(dbReader("PSID"))
                        If RA.Scenarios.Scenarios.ContainsKey(psid) Then
                            Dim scenario As RAScenario = RA.Scenarios.Scenarios(psid)
                            Name = CStr(dbReader("PropertyName"))
                            Value = CStr(dbReader("PValue"))

                            Select Case Name.ToLower
                                Case "ignoreconstraints"
                                    scenario.Settings.CustomConstraints = If(Value = "0", True, False)
                                Case "ignoredependencies"
                                    scenario.Settings.Dependencies = If(Value = "0", True, False)
                                Case "ignorefundingpools"
                                    scenario.Settings.FundingPools = If(Value = "0", True, False)
                                Case "ignoregroups"
                                    scenario.Settings.Groups = If(Value = "0", True, False)
                                Case "ignoremustnots"
                                    scenario.Settings.MustNots = If(Value = "0", True, False)
                                Case "ignoremusts"
                                    scenario.Settings.Musts = If(Value = "0", True, False)
                                Case "ignorerisks"
                                    scenario.Settings.Risks = If(Value = "0", True, False)
                                Case "basecaseshow"
                                    scenario.Settings.UseBaseCase = If(Value = "1", True, False)
                                Case "bcincludeconstraints"
                                    scenario.Settings.BaseCaseForConstraints = If(Value = "1", True, False)
                                Case "bcincludedependencies"
                                    scenario.Settings.BaseCaseForDependencies = If(Value = "1", True, False)
                                Case "bcincludefundingpools"
                                    scenario.Settings.BaseCaseForFundingPools = If(Value = "1", True, False)
                                Case "bcincludegroups"
                                    scenario.Settings.BaseCaseForGroups = If(Value = "1", True, False)
                                Case "bcincludemustnots"
                                    scenario.Settings.BaseCaseForMustNots = If(Value = "1", True, False)
                                Case "bcincludemusts"
                                    scenario.Settings.BaseCaseForMusts = If(Value = "1", True, False)
                                Case "sortraalternativesby" 'AS/1-26-16===
                                    'Dim colECC As Integer = getECCcolumnByECDcol(CInt(Value))
                                    Dim colECC As RAGlobalSettings.raColumnID = getECCcolumnByECDcol(CInt(Value))
                                    RA.Scenarios.GlobalSettings.SortBy = If(SOrder < 0, -colECC, colECC) '-2 'AS/4-8-16==
                                    'Case "SortRAconstraintsBy"
                                    '    RA.Scenarios.GlobalSettings.SortBy = Value 'AS/1-26-16==
                            End Select
                        End If
                    End While
                    dbReader.Close()
                End If
            End Using

            Return True
        End Function

        Private Function LoadRAScenarios() As Boolean
            If (ProjectManager Is Nothing) OrElse Not CheckDBConnection(ProviderType, Location) Then Return False

            Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                Dim dbReader As DbDataReader
                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection

                dbConnection.Open()

                Dim ID As Integer
                Dim Name As String
                Dim Description As String

                If TableExists(Location, ProviderType, "RAPortfolioScenarios") Then
                    oCommand.CommandText = "SELECT * FROM RAPortfolioScenarios"
                    dbReader = DBExecuteReader(ProviderType, oCommand)

                    Dim RA As Canvas.ResourceAligner = ProjectManager.ResourceAligner

                    While dbReader.Read
                        ID = CInt(dbReader("PSID"))
                        If Not TypeOf (dbReader("ScenarioName")) Is DBNull Then
                            Name = CStr(dbReader("ScenarioName"))
                        Else
                            Name = "Scenario" + ID.ToString
                        End If

                        If Not TypeOf (dbReader("Description")) Is DBNull Then
                            Description = CStr(dbReader("Description"))
                        Else
                            Description = ""
                        End If

                        Dim s As RAScenario
                        If ID = 0 Then
                            s = RA.Scenarios.Scenarios(ID)
                        Else
                            s = RA.Scenarios.AddScenario(, False)
                        End If
                        s.ID = ID
                        s.Name = Name
                    End While
                    RA.Scenarios.CheckAndSortScenarios()
                    dbReader.Close()
                End If
            End Using

            Return True
        End Function

        Private Function LoadRARisks() As Boolean
            If (ProjectManager Is Nothing) OrElse Not CheckDBConnection(ProviderType, Location) Then Return False

            Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                Dim dbReader As DbDataReader
                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection

                dbConnection.Open()

                If TableExists(Location, ProviderType, "RArisks") Then
                    oCommand.CommandText = "SELECT * FROM RArisks"
                    dbReader = DBExecuteReader(ProviderType, oCommand)

                    Dim RA As Canvas.ResourceAligner = ProjectManager.ResourceAligner
                    Dim A As clsHierarchy = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy)
                    Dim PSID As Integer

                    If dbReader.HasRows Then
                        While dbReader.Read
                            Dim alt As clsNode = A.GetNodeByID(CInt(CStr(dbReader("AID")).Substring(1)))
                            If alt IsNot Nothing Then
                                PSID = CInt(dbReader("PSID"))
                                If Not TypeOf (dbReader("Risk")) Is DBNull Then
                                    Dim dRes As Double
                                    If Double.TryParse(CStr(dbReader("Risk")), dRes) Then
                                        Dim Scenario As RAScenario
                                        If RA.Scenarios.Scenarios.ContainsKey(PSID) Then
                                            Scenario = RA.Scenarios.Scenarios(PSID)
                                            For Each raAlt As RAAlternative In Scenario.AlternativesFull
                                                If raAlt.ID.ToLower = alt.NodeGuidID.ToString.ToLower Then
                                                    raAlt.RiskOriginal = dRes
                                                End If
                                            Next
                                        End If
                                        If PSID = 0 Then
                                            ProjectManager.Attributes.SetAttributeValue(ATTRIBUTE_RISK_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtDouble, dRes, alt.NodeGuidID, Guid.Empty)
                                        End If
                                    End If
                                End If
                            End If
                        End While
                    End If
                    dbReader.Close()
                End If
            End Using

            Return True
        End Function

        Private Function LoadRACosts() As Boolean
            If (ProjectManager Is Nothing) OrElse Not CheckDBConnection(ProviderType, Location) Then Return False

            Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                Dim dbReader As DbDataReader
                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection

                dbConnection.Open()

                If TableExists(Location, ProviderType, "RAbenefits") Then
                    oCommand.CommandText = "SELECT * FROM RAbenefits"
                    dbReader = DBExecuteReader(ProviderType, oCommand)

                    Dim RA As Canvas.ResourceAligner = ProjectManager.ResourceAligner
                    Dim A As clsHierarchy = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy)
                    Dim PSID As Integer

                    If FieldExists(dbReader, "Costs") Then
                        If dbReader.HasRows Then
                            While dbReader.Read
                                Dim alt As clsNode = A.GetNodeByID(CInt(CStr(dbReader("AID")).Substring(1)))
                                If alt IsNot Nothing Then
                                    PSID = CInt(dbReader("PSID"))
                                    If Not TypeOf (dbReader("Costs")) Is DBNull Then
                                        Dim dRes As Double
                                        If Double.TryParse(FixStringWithSingleValue(CStr(dbReader("Costs"))), dRes) Then
                                            Dim Scenario As RAScenario
                                            If RA.Scenarios.Scenarios.ContainsKey(PSID) Then
                                                Scenario = RA.Scenarios.Scenarios(PSID)
                                                For Each raAlt As RAAlternative In Scenario.AlternativesFull
                                                    If raAlt.ID.ToLower = alt.NodeGuidID.ToString.ToLower Then
                                                        raAlt.Cost = dRes
                                                    End If
                                                Next
                                            End If
                                            'If PSID = 0 Then
                                            '    ProjectManager.Attributes.SetAttributeValue(ATTRIBUTE_RISK_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtDouble, dRes, alt.NodeGuidID, Guid.Empty)
                                            'End If
                                        End If
                                    End If
                                End If
                            End While

                        End If
                    Else 'AS/9-18-15=== if table RAbenefits is empty, pick up the costs from AltsData
                        dbReader.Close()
                        oCommand.CommandText = "SELECT * FROM AltsData WHERE WRT = 'N0' AND PID = 1"
                        dbReader = DBExecuteReader(ProviderType, oCommand)

                        If Not dbReader.HasRows Then
                            dbReader.Close()
                            oCommand.CommandText = "SELECT * FROM AltsData WHERE WRT = 'N0' AND PID = 0"
                            dbReader = DBExecuteReader(ProviderType, oCommand)
                        End If

                        If dbReader.HasRows Then
                            While dbReader.Read
                                Dim alt As clsNode = A.GetNodeByID(CInt(CStr(dbReader("AID")).Substring(1)))
                                If alt IsNot Nothing Then
                                    PSID = 0
                                    If Not TypeOf (dbReader("DATA")) Is DBNull Then
                                        Dim dRes As Double
                                        If Double.TryParse(FixStringWithSingleValue(CStr(dbReader("DATA"))), dRes) Then
                                            Dim Scenario As RAScenario
                                            If RA.Scenarios.Scenarios.ContainsKey(PSID) Then
                                                Scenario = RA.Scenarios.Scenarios(PSID)
                                                For Each raAlt As RAAlternative In Scenario.AlternativesFull
                                                    If raAlt.ID.ToLower = alt.NodeGuidID.ToString.ToLower Then
                                                        raAlt.Cost = dRes
                                                    End If
                                                Next
                                            End If

                                        End If
                                    End If
                                End If
                            End While
                        End If 'AS/9-18-15==
                    End If

                    dbReader.Close()

                Else 'AS/9-17-15=== if table RAbenefits not exits, pick up the costs from AltsData
                    oCommand.CommandText = "SELECT * FROM AltsData WHERE WRT = 'N0' AND PID = 1"
                    dbReader = DBExecuteReader(ProviderType, oCommand)

                    Dim RA As Canvas.ResourceAligner = ProjectManager.ResourceAligner
                    Dim A As clsHierarchy = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy)
                    Dim PSID As Integer

                    If Not dbReader.HasRows Then 'AS/9-18-15===
                        dbReader.Close()
                        oCommand.CommandText = "SELECT * FROM AltsData WHERE WRT = 'N0' AND PID = 0"
                        dbReader = DBExecuteReader(ProviderType, oCommand)
                    End If 'AS/9-18-15==

                    If dbReader.HasRows Then
                        While dbReader.Read
                            Dim alt As clsNode = A.GetNodeByID(CInt(CStr(dbReader("AID")).Substring(1)))
                            If alt IsNot Nothing Then
                                PSID = 0
                                If Not TypeOf (dbReader("DATA")) Is DBNull Then
                                    Dim dRes As Double
                                    If Double.TryParse(FixStringWithSingleValue(CStr(dbReader("DATA"))), dRes) Then
                                        Dim Scenario As RAScenario
                                        If RA.Scenarios.Scenarios.ContainsKey(PSID) Then
                                            Scenario = RA.Scenarios.Scenarios(PSID)
                                            For Each raAlt As RAAlternative In Scenario.AlternativesFull
                                                If raAlt.ID.ToLower = alt.NodeGuidID.ToString.ToLower Then
                                                    raAlt.Cost = dRes
                                                End If
                                            Next
                                        End If

                                    End If
                                End If
                            End If
                        End While
                    End If

                    dbReader.Close() 'AS/9-17-15==
                End If
            End Using

            Return True
        End Function

        Private Function LoadRAFundingPools() As Boolean
            If (ProjectManager Is Nothing) OrElse Not CheckDBConnection(ProviderType, Location) Then Return False

            Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                Dim dbReader As DbDataReader
                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection

                dbConnection.Open()

                Dim RA As Canvas.ResourceAligner = ProjectManager.ResourceAligner
                Dim A As clsHierarchy = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy)

                If TableExists(Location, ProviderType, "FundingPools") Then
                    Dim PSID As Integer
                    ' read funding pools definitions first
                    oCommand.CommandText = "SELECT * FROM FundingPools WHERE AID=?"
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "AID", "POOL"))
                    dbReader = DBExecuteReader(ProviderType, oCommand)

                    If dbReader.HasRows Then
                        While dbReader.Read
                            PSID = CInt(dbReader("PSID"))
                            If Not TypeOf (dbReader("Limit")) Is DBNull Then
                                Dim dRes As Double
                                If Double.TryParse(FixStringWithSingleValue(CStr(dbReader("Limit"))), dRes) Then
                                    Dim Scenario As RAScenario
                                    If RA.Scenarios.Scenarios.ContainsKey(PSID) Then
                                        Scenario = RA.Scenarios.Scenarios(PSID)
                                        Dim pool As RAFundingPool = Scenario.FundingPools.AddPool(CStr(dbReader("Poolname")).Trim)
                                        pool.PoolLimit = dRes
                                    End If
                                End If
                            End If
                        End While
                    End If
                    dbReader.Close()

                    ' read funding pools data
                    oCommand.CommandText = "SELECT * FROM FundingPools WHERE AID<>?"
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "AID", "POOL"))
                    dbReader = DBExecuteReader(ProviderType, oCommand)
                    If dbReader.HasRows Then
                        While dbReader.Read
                            Dim alt As clsNode = A.GetNodeByID(CInt(CStr(dbReader("AID")).Substring(1)))
                            If alt IsNot Nothing Then
                                PSID = CInt(dbReader("PSID"))
                                If Not TypeOf (dbReader("Limit")) Is DBNull Then
                                    Dim dRes As Double
                                    If Double.TryParse(FixStringWithSingleValue(CStr(dbReader("Limit"))), dRes) Then
                                        Dim Scenario As RAScenario
                                        If RA.Scenarios.Scenarios.ContainsKey(PSID) Then
                                            Scenario = RA.Scenarios.Scenarios(PSID)
                                            Dim FP As RAFundingPool = Scenario.FundingPools.GetPoolByName(CStr(dbReader("Poolname")).Trim)
                                            If FP IsNot Nothing Then
                                                FP.SetAlternativeValue(alt.NodeGuidID.ToString, dRes)
                                            End If
                                        End If
                                    End If
                                End If
                            End If
                        End While
                    End If
                End If
            End Using

            Return True
        End Function

        Private Function FieldExists(dbReader As DbDataReader, fieldName As String) As Boolean
            For i As Integer = 0 To dbReader.FieldCount - 1
                If dbReader.GetName(i).ToLower = fieldName.ToLower Then Return True
            Next
            Return False
        End Function

        Private Function getECCcolumnByECDcol(colECD As Integer) As RAGlobalSettings.raColumnID 'AS/4-8-16
            Dim colECC As RAGlobalSettings.raColumnID
            Select Case colECD
                Case 0 'AID
                    colECC = RAGlobalSettings.raColumnID.ID ' 1
                Case 1 'ColIndex 
                    colECC = RA_OPT_DEF_SORTING ' 1
                Case 2 'colAltName 
                    colECC = RAGlobalSettings.raColumnID.Name ' 2
                Case 3 'colSelected (Funded)
                    colECC = RAGlobalSettings.raColumnID.isFunded ' 3
                Case 4 'ColBenefits 
                    colECC = RAGlobalSettings.raColumnID.Benefit ' 4
                Case 5 'colCosts
                    colECC = RAGlobalSettings.raColumnID.Cost ' 8
                Case 6 'colPartial 
                    colECC = RAGlobalSettings.raColumnID.isPartial ' 9
                Case 7 'colPartialPct 
                    colECC = RAGlobalSettings.raColumnID.isPartial ' 9
                Case 8 'ColMusts 
                    colECC = RAGlobalSettings.raColumnID.Musts ' 11
                Case 9 'ColMustNots 
                    colECC = RAGlobalSettings.raColumnID.MustNot ' 12
                Case Else
                    colECC = RA_OPT_DEF_SORTING '1
            End Select
            Return colECC
        End Function

        Public Function LoadUserJudgments(Optional ByVal AUser As ECCore.ECTypes.clsUser = Nothing, Optional ByVal CreateEmptyMissingJudgments As Boolean = True, Optional ByVal NodeID As Integer = -1) As Boolean 'C0081
            If (ProjectManager Is Nothing) OrElse Not CheckDBConnection(ProviderType, Location) Then
                Return False
            End If

            Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location) 'C0235
                dbConnection.Open()

                Dim dbReader As DbDataReader
                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection 'C0235

                Dim UList As New List(Of clsUser) 'C0388
                If Not AUser Is Nothing Then
                    UList.Add(AUser)
                End If
                Dim users As List(Of clsUser) = If(Not AUser Is Nothing, UList, ProjectManager.UsersList) 'C0388


                Dim adv As Integer
                Dim value As Single
                Dim comment As String

                Dim child1 As clsNode
                Dim child2 As clsNode

                Dim node As clsNode
                Dim alt As clsNode

                Dim H As clsHierarchy = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy)
                Dim A As clsHierarchy = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy)

                For Each user As clsUser In users
                    oCommand.CommandText = "SELECT * FROM Judgments WHERE PID = " & user.UserID.ToString 'C0235
                    dbReader = DBExecuteReader(ProviderType, oCommand)

                    If Not dbReader Is Nothing Then
                        While dbReader.Read
                            value = CSng(dbReader("J"))
                            If value <> 0 Then
                                If value > 1 Then
                                    adv = 1
                                Else
                                    value = 1 / value
                                    adv = -1
                                End If
                            Else
                                'if VALUE = 0 ??!!  missing judgment?
                                'value = 1
                            End If

                            If Not TypeOf (dbReader("Note")) Is DBNull Then
                                comment = dbReader("Note")
                            Else
                                comment = ""
                            End If

                            node = H.GetNodeByID(CInt(CStr(dbReader("WRT")).Substring(1)))


                            Dim pwData As clsPairwiseMeasureData

                            If Not node Is Nothing Then
                                Dim nodesBelow As List(Of clsNode) = node.GetNodesBelow(UNDEFINED_USER_ID)
                                If Not node.IsTerminalNode Then
                                    If (CStr(dbReader("N1"))(0) = "N") And (CStr(dbReader("N2"))(0) = "N") Then
                                        ' if pairwise
                                        child1 = H.GetNodeByID(CInt(CStr(dbReader("N1")).Substring(1)))
                                        child2 = H.GetNodeByID(CInt(CStr(dbReader("N2")).Substring(1)))

                                        If (Not child1 Is Nothing) And (Not child2 Is Nothing) Then
                                            If (child1.ParentNode Is node) And (child2.ParentNode Is node) Then
                                                If node.GetChildIndexByID(child1.NodeID, nodesBelow) > node.GetChildIndexByID(child2.NodeID, nodesBelow) Then 'C0128
                                                    pwData = New clsPairwiseMeasureData(child2.NodeID,
                                                    child1.NodeID, If(adv = 1, -1, 1), value, node.NodeID, user.UserID, value = 0, comment)
                                                Else
                                                    pwData = New clsPairwiseMeasureData(child1.NodeID,
                                                    child2.NodeID, adv, value, node.NodeID, user.UserID, value = 0, comment)
                                                End If
                                                pwData.ModifyDate = mLoadTime
                                                If node.MeasureType = ECMeasureType.mtDirect Then
                                                    node.MeasureType = ECMeasureType.mtPairwise
                                                End If
                                                node.Judgments.AddMeasureData(pwData, True) 'C0327
                                            End If
                                        End If
                                    Else

                                    End If
                                Else
                                    If node.MeasureType = ECMeasureType.mtPairwise Then
                                        If (CStr(dbReader("N1"))(0) = "A") And (CStr(dbReader("N2"))(0) = "A") Then
                                            ' if pairwise
                                            child1 = A.GetNodeByID(CInt(CStr(dbReader("N1")).Substring(1)))
                                            child2 = A.GetNodeByID(CInt(CStr(dbReader("N2")).Substring(1)))
                                            If (Not child1 Is Nothing) And (Not child2 Is Nothing) Then
                                                If node.GetChildIndexByID(child1.NodeID, nodesBelow) > node.GetChildIndexByID(child2.NodeID, nodesBelow) Then
                                                    pwData = New clsPairwiseMeasureData(child2.NodeID,
                                                    child1.NodeID, If(adv = 1, -1, 1), value, node.NodeID, user.UserID, value = 0, comment)
                                                Else
                                                    pwData = New clsPairwiseMeasureData(child1.NodeID,
                                                            child2.NodeID, adv, value, node.NodeID, user.UserID, value = 0, comment)
                                                End If
                                                pwData.ModifyDate = mLoadTime
                                                node.Judgments.AddMeasureData(pwData, True) 'C0327
                                            End If
                                        Else
                                        End If
                                    End If
                                End If
                            End If
                        End While
                        dbReader.Close()
                    End If

                    ' Load AltsData

                    oCommand.CommandText = "SELECT * FROM AltsData WHERE PID=" + user.UserID.ToString 'C0235
                    dbReader = DBExecuteReader(ProviderType, oCommand)

                    Dim pid As Integer
                    Dim data As String

                    Dim RD As clsRatingMeasureData

                    If Not dbReader Is Nothing Then
                        While dbReader.Read
                            If Not TypeOf (dbReader("DATA")) Is DBNull AndAlso CStr(dbReader("DATA")).ToLower <> CStr("#VALUE!").ToLower AndAlso CStr(dbReader("DATA")).ToLower <> CStr("N/A").ToLower Then ' special check for case 6271 'AS/11-23-15 added a special check for 8414
                                ''Debug.Print(dbReader("PID").ToString & "  " & dbReader("AID").ToString & "  " & dbReader("WRT").ToString & "  " & dbReader("DATA").ToString)
                                pid = CInt(dbReader("PID"))
                                If pid <> 1 Then
                                    node = H.GetNodeByID(CInt(CStr(dbReader("WRT")).Substring(1)))

                                    'C0382===
                                    alt = Nothing

                                    If node IsNot Nothing Then 'C0393
                                        If node.IsTerminalNode Then
                                            alt = A.GetNodeByID(CInt(CStr(dbReader("AID")).Substring(1)))
                                        Else
                                            If node.MeasureType = ECMeasureType.mtDirect Then
                                                alt = H.GetNodeByID(CInt(CStr(dbReader("AID")).Substring(1)))
                                            End If
                                        End If
                                    End If

                                    If Not TypeOf (dbReader("Note")) Is DBNull Then
                                        comment = dbReader("Note")
                                    Else
                                        comment = ""
                                    End If

                                    If (Not node Is Nothing) And (Not alt Is Nothing) Then
                                        Select Case node.MeasureType
                                            Case ECMeasureType.mtRatings
                                                data = CStr(dbReader("DATA"))(0)
                                                If data.ToUpper = "I" Then
                                                    RD = New clsRatingMeasureData(alt.NodeID, node.NodeID, pid, CType(node.MeasurementScale, clsRatingScale).GetRatingByID(CInt(CStr(dbReader("DATA")).Substring(1))),
                                                    ProjectManager.MeasureScales.GetRatingScaleByName("Scale for " + node.NodeName)) 'C0969
                                                    RD.Comment = comment
                                                    RD.ModifyDate = mLoadTime
                                                    node.Judgments.AddMeasureData(RD, True) 'C0327
                                                Else
                                                    Dim R As New clsRating
                                                    R.ID = -1
                                                    R.Name = "Direct Entry from EC11.5"
                                                    R.Value = Single.Parse(FixStringWithSingleValue(CStr(dbReader("DATA"))))

                                                    RD = New clsRatingMeasureData(alt.NodeID, node.NodeID, pid, R, Nothing)

                                                    RD.ModifyDate = mLoadTime
                                                    node.Judgments.AddMeasureData(RD, True) 'C0327
                                                End If
                                            Case ECMeasureType.mtRegularUtilityCurve
                                                'Dim ucdata As New clsUtilityCurveMeasureData(alt.NodeID, node.NodeID, pid, Single.Parse(FixStringWithSingleValue(CStr(dbReader("DATA")))), node.Hierarchy.ProjectManager.MeasureScales.GetRegularUtilityCurveByID(node.MeasurementScaleID), False, comment) 'AS/6-5-15
                                                Dim sData As String 'AS/6-5-15===
                                                sData = CStr(dbReader("DATA"))
                                                If InStr(sData, "(") > 0 And InStr(sData, ")") > 0 Then
                                                    sData = Replace(sData, "(", "-")
                                                    sData = Replace(sData, ")", "")
                                                End If
                                                Dim ucdata As New clsUtilityCurveMeasureData(alt.NodeID, node.NodeID, pid, Single.Parse(FixStringWithSingleValue(sData)), node.Hierarchy.ProjectManager.MeasureScales.GetRegularUtilityCurveByID(node.MeasurementScaleID), False, comment) 'AS/6-5-15==
                                                ucdata.ModifyDate = mLoadTime
                                                node.Judgments.AddMeasureData(ucdata, True) 'C0327
                                            Case ECMeasureType.mtStep 'C0156
                                                Dim stepData As New clsStepMeasureData(alt.NodeID, node.NodeID, pid, Single.Parse(FixStringWithSingleValue(CStr(dbReader("DATA")))), node.Hierarchy.ProjectManager.MeasureScales.GetStepFunctionByID(node.MeasurementScaleID), False, comment)
                                                stepData.ModifyDate = mLoadTime
                                                node.Judgments.AddMeasureData(stepData, True) 'C0327
                                            Case ECMeasureType.mtDirect 'C0156
                                                Dim directDataValue As Single = Single.Parse(FixStringWithSingleValue(CStr(dbReader("DATA")))) 'C0382
                                                Dim dData As New clsDirectMeasureData(alt.NodeID, node.NodeID, pid, directDataValue, False, comment) 'C0382
                                                dData.ModifyDate = mLoadTime
                                                node.Judgments.AddMeasureData(dData, True) 'C0237

                                                ' if this is a MaxOut model
                                                If Not node.IsTerminalNode Then
                                                    Dim fixedValue As Single

                                                    ' go through all children and add direct data for fixed local priority nodes
                                                    For Each child As clsNode In node.Children
                                                        If child.Tag IsNot Nothing Then
                                                            Dim MaxOutData As clsMaxOutData = CType(child.Tag, clsMaxOutData)
                                                            If MaxOutData.HasFixedLocalPriority Then
                                                                fixedValue = MaxOutData.FixedLocalPriority
                                                                Dim FixedDirectData As New clsDirectMeasureData(child.NodeID, node.NodeID, pid, fixedValue, False, comment)
                                                                FixedDirectData.ModifyDate = mLoadTime
                                                                node.Judgments.AddMeasureData(FixedDirectData, True)
                                                            End If
                                                        End If
                                                    Next

                                                    ' go through all children and add direct data for calculated local priority nodes
                                                    For Each child As clsNode In node.Children
                                                        If child.Tag IsNot Nothing Then
                                                            Dim MaxOutData As clsMaxOutData = CType(child.Tag, clsMaxOutData)
                                                            If MaxOutData.HasCalculatedLocalPriority Then
                                                                Dim CalculatedValue As Single = 1 - fixedValue - directDataValue
                                                                Dim CalculatedDirectData As New clsDirectMeasureData(child.NodeID, node.NodeID, pid, CalculatedValue, False, comment) 'C0383
                                                                CalculatedDirectData.ModifyDate = mLoadTime
                                                                node.Judgments.AddMeasureData(CalculatedDirectData, True)
                                                            End If
                                                        End If
                                                    Next
                                                End If
                                        End Select
                                    End If
                                End If
                            End If
                        End While
                        dbReader.Close()
                    End If

                    ' TODO: load judgments for specific hierarchies
                    For Each MH As clsHierarchy In ProjectManager.Hierarchies
                        For Each MAH As clsHierarchy In ProjectManager.AltsHierarchies
                            If CreateEmptyMissingJudgments Then
                                ProjectManager.AddEmptyMissingJudgments(MH.HierarchyID, MAH.HierarchyID, user, NodeID) 'C0120
                            End If
                        Next
                    Next
                Next

                oCommand = Nothing
            End Using

            Return True
        End Function
    End Class
End Namespace
