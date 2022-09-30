Imports ECCore.AHPConverterHelperFunctions
Imports System.Data.Common
Imports System.Linq

Namespace ECCore
    <Serializable()> Public Class StorageWriterAHP
        Inherits clsStorageWriter

        Public Property GoalDefaultInfoDoc() As String
        Public WriteOnlyAllowedJudgmentsToAHP As Boolean = False

        Protected mIntensityCounter As Integer = 1
        Protected mHasOnlyOneDirectJudgmentForNonCovObj As Boolean

        <NonSerialized()> Protected mDBConnection As DbConnection
        <NonSerialized()> Protected mDBTransaction As DbTransaction
        <NonSerialized()> Protected mDBCommand As DbCommand

        Protected Function CheckConnection() As Boolean
            If mDBConnection IsNot Nothing AndAlso ((mDBConnection.State = ConnectionState.Open) Or (mDBConnection.State = ConnectionState.Executing)) Then
                Return True
            Else
                Return CheckDBConnection(ProviderType, Location)
            End If
        End Function

        Protected Function SavingToDatabaseInProgress() As Boolean
            Return mDBConnection IsNot Nothing AndAlso ((mDBConnection.State = ConnectionState.Open) Or (mDBConnection.State = ConnectionState.Executing))
        End Function

#Region "Functions needed to convert to ahp"
        Protected Function GetNextIntensityID() As Integer
            Return mIntensityCounter + 1
        End Function

#End Region

#Region "Saving functions"
        Private Function SaveAltsCosts() As Boolean
            If ProjectManager Is Nothing Then Return False
            If Not SavingToDatabaseInProgress() AndAlso Not CheckDBConnection(ProviderType, Location) Then Return False 'C0812

            Dim dbConnection As DbConnection
            If SavingToDatabaseInProgress() Then
                dbConnection = mDBConnection
            Else
                dbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()
            End If

            Dim oCommand As DbCommand = GetDBCommand(ProviderType) 'C0235
            Dim affected As Integer

            oCommand.Connection = dbConnection

            'C0921===
            Dim Transaction As DbTransaction = dbConnection.BeginTransaction
            oCommand.Transaction = Transaction
            'C0921==

            For Each alt As clsNode In ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).TerminalNodes 'C0385
                Dim res As Object = ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_COST_ID, alt.NodeGuidID)
                Dim Cost As Double
                If res IsNot Nothing Then
                    Cost = CDbl(res)
                Else
                    Cost = UNDEFINED_ATTRIBUTE_DEFAULT_DOUBLE_VALUE
                End If

                'If alt.Tag IsNot Nothing Then
                If Cost <> UNDEFINED_ATTRIBUTE_DEFAULT_DOUBLE_VALUE Then
                    Try
                        oCommand.CommandText = "INSERT INTO AltsData (PID, AID, WRT, DATA) VALUES (?, ?, ?, ?)"
                        oCommand.Parameters.Clear()
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "PID", 0)) 'C0237
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "AID", "A" + If(IsAltsZeroBased(ProjectManager), alt.NodeID + 1, alt.NodeID).ToString)) 'C0237
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "WRT", "N0")) 'C0237
                        'oCommand.Parameters.Add(GetDBParameter(ProviderType, "DATA", CStr(alt.Tag))) 'C0626
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "DATA", Cost.ToString)) 'C0626
                        affected = DBExecuteNonQuery(ProviderType, oCommand)
                    Catch ex As Exception
                        Transaction.Rollback() 'C0921
                        oCommand = Nothing
                        If Not SavingToDatabaseInProgress() Then 'C0812
                            dbConnection.Close()
                        End If
                        Return False
                    End Try
                End If
            Next

            Transaction.Commit() 'C0921



            oCommand = Nothing
            If Not SavingToDatabaseInProgress() Then 'C0812
                dbConnection.Close()
            End If
            Return True
        End Function

        Private Function SaveMeasurementTypes() As Boolean 'C0300
            If (ProjectManager Is Nothing) OrElse Not CheckDBConnection(ProviderType, Location) Then
                Return False
            End If

            'C0812===
            Dim dbConnection As DbConnection
            If SavingToDatabaseInProgress() Then
                dbConnection = mDBConnection
            Else
                dbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()
            End If
            'C0812==

            Dim oCommand As DbCommand = GetDBCommand(ProviderType)
            Dim affected As Integer

            oCommand.Connection = dbConnection

            'C0921===
            Dim Transaction As DbTransaction = dbConnection.BeginTransaction
            oCommand.Transaction = Transaction
            'C0921==

            ' =============================
            ' Fill llCritFormulas table

            ' Delete everything from llCritFormulas table
            oCommand.CommandText = "DELETE FROM llCritFormulas"
            affected = DBExecuteNonQuery(ProviderType, oCommand)

            Dim nid As Integer
            Dim ftype As Integer

            'For Each node As clsNode In ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).TerminalNodes.Clone 'C0385
            'For Each node As clsNode In ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).TerminalNodes 'C0385 'C0784
            For Each node As clsNode In ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).Nodes 'C0784
                Try
                    nid = If(IsNodesZeroBased(ProjectManager), node.NodeID + 1, node.NodeID)

                    Dim lowStr As String = ""
                    Dim highStr As String = ""
                    Dim curvStr As String = ""
                    Dim pwLinear As Boolean = False 'C0300
                    Dim InfoDoc As String = ""

                    Select Case node.MeasureType
                        Case ECMeasureType.mtPairwise
                            ftype = 9
                        Case ECMeasureType.mtRatings
                            ftype = 4
                            InfoDoc = CType(node.MeasurementScale, clsRatingScale).Comment
                        Case ECMeasureType.mtAdvancedUtilityCurve
                            ftype = 3
                        Case ECMeasureType.mtRegularUtilityCurve
                            If CType(node.MeasurementScale, clsRegularUtilityCurve).IsIncreasing Then
                                ftype = 1
                            Else
                                ftype = 2
                            End If
                            lowStr = CType(node.MeasurementScale, clsRegularUtilityCurve).Low.ToString
                            highStr = CType(node.MeasurementScale, clsRegularUtilityCurve).High.ToString
                            curvStr = CType(node.MeasurementScale, clsRegularUtilityCurve).Curvature.ToString
                        'curvStr = curvStr.Replace(".", ",") 'C0320'C0323
                        Case ECMeasureType.mtStep
                            ftype = 5
                            pwLinear = CType(node.MeasurementScale, clsStepFunction).IsPiecewiseLinear 'C0300
                        Case ECMeasureType.mtDirect
                            ftype = 3
                    End Select

                    oCommand.CommandText = "INSERT INTO llCritFormulas (WRT, FTYPE, LOW, HIGH, CURVATURE, GE, LE, PWLINEAR, InfoDoc) " +
                                        "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)" 'C0300

                    oCommand.Parameters.Clear()

                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "WRT", "N" + nid.ToString))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "FTYPE", ftype.ToString))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "LOW", lowStr))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "HIGH", highStr))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "CURVATURE", curvStr))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "GE", ""))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "LE", ""))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "PWLINEAR", pwLinear))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "InfoDoc", InfoDoc))

                    affected = DBExecuteNonQuery(ProviderType, oCommand)
                Catch ex As Exception
                    Transaction.Rollback() 'C0921
                    oCommand = Nothing
                    dbConnection.Close()
                    Return False
                End Try
            Next

            Transaction.Commit() 'C0921

            oCommand = Nothing
            If Not SavingToDatabaseInProgress() Then 'C0812
                dbConnection.Close()
            End If
            Return True
        End Function


        Private Function SaveAlternatives() As Boolean 'C0304
            If (ProjectManager Is Nothing) OrElse Not CheckDBConnection(ProviderType, Location) Then
                Return False
            End If

            'C0812===
            Dim dbConnection As DbConnection
            If SavingToDatabaseInProgress() Then
                dbConnection = mDBConnection
            Else
                dbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()
            End If
            'C0812==

            Dim oCommand As DbCommand = GetDBCommand(ProviderType)
            Dim affected As Integer

            oCommand.Connection = dbConnection

            'C0921===
            Dim Transaction As DbTransaction = dbConnection.BeginTransaction
            oCommand.Transaction = Transaction
            'C0921==

            ' =============================
            ' Fill AltsGlobal table

            ' Delete everything from AltsGlobal table
            oCommand.CommandText = "DELETE FROM AltsGlobal"
            affected = DBExecuteNonQuery(ProviderType, oCommand)

            ' Add IDEAL alternative
            Dim i As Integer = 1

            'oCommand.CommandText = "INSERT INTO AltsGlobal (SOrder, AID, MID, AltName, [Level], IsLeaf, Infeasible, Selected) VALUES (?, ?, ?, ?, ?, ?, ?, ?)" 'C0429
            oCommand.CommandText = "INSERT INTO AltsGlobal (SOrder, AID, MID, AltName, [Level], IsLeaf, Infeasible, Selected, Period) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)" 'C0429
            oCommand.Parameters.Clear()
            oCommand.Parameters.Add(GetDBParameter(ProviderType, "SOrder", i))
            oCommand.Parameters.Add(GetDBParameter(ProviderType, "AID", "A0"))
            oCommand.Parameters.Add(GetDBParameter(ProviderType, "MID", ""))
            oCommand.Parameters.Add(GetDBParameter(ProviderType, "AltName", "IDEAL"))
            oCommand.Parameters.Add(GetDBParameter(ProviderType, "Level", 0))
            oCommand.Parameters.Add(GetDBParameter(ProviderType, "IsLeaf", True))
            oCommand.Parameters.Add(GetDBParameter(ProviderType, "Infeasible", False))
            oCommand.Parameters.Add(GetDBParameter(ProviderType, "Selected", False))
            oCommand.Parameters.Add(GetDBParameter(ProviderType, "Period", 0)) 'C0429
            affected = DBExecuteNonQuery(ProviderType, oCommand)

            i += 1

            ' Add alternatives
            'For Each alt As clsNode In ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).TerminalNodes.Clone 'C0385
            For Each alt As clsNode In ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).TerminalNodes 'C0385
                Try
                    'oCommand.CommandText = "INSERT INTO AltsGlobal (SOrder, AID, AltName, Infeasible, IID, MID, [Level], IsLeaf, Selected, Period, BasePeriodAID) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)"
                    'oCommand.CommandText = "INSERT INTO AltsGlobal (SOrder, AID, AltName, Infeasible, MID, [Level], IsLeaf, Selected) VALUES (?, ?, ?, ?, ?, ?, ?, ?)" 'C0429
                    oCommand.CommandText = "INSERT INTO AltsGlobal (SOrder, AID, AltName, Infeasible, MID, [Level], IsLeaf, Selected, Period, BasePeriodAID) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)" 'C0429
                    oCommand.Parameters.Clear()
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "SOrder", i))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "AID", "A" + If(IsAltsZeroBased(ProjectManager), alt.NodeID + 1, alt.NodeID).ToString))
                    'oCommand.Parameters.Add(GetDBParameter(ProviderType, "AltName", alt.NodeName)) 'C0727
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "AltName", alt.NodeName.Substring(0, If(alt.NodeName.Length > 100, 100, alt.NodeName.Length)))) 'C0727

                    If (alt.AHPAltData IsNot Nothing) Then
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "Infeasible", alt.AHPAltData.Infeasible))
                    Else
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "Infeasible", False))
                    End If

                    'If alt.AHPAltData IsNot Nothing Then
                    '    oCommand.Parameters.Add(GetDBParameter(ProviderType, "IID", alt.AHPAltData.IID))
                    'Else
                    '    oCommand.Parameters.Add(GetDBParameter(ProviderType, "IID", ""))
                    'End If

                    'If alt.AHPAltData IsNot Nothing Then 'C0306
                    If (alt.AHPAltData IsNot Nothing) AndAlso (alt.AHPAltData.MID <> UNDEFINED_STRING_VALUE) Then 'C0306
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "MID", alt.AHPAltData.MID))
                    Else
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "MID", ""))
                    End If

                    'If alt.AHPAltData IsNot Nothing Then 'C0306
                    If (alt.AHPAltData IsNot Nothing) AndAlso (alt.AHPAltData.Level <> UNDEFINED_INTEGER_VALUE) Then 'C0306
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "Level", alt.AHPAltData.Level))
                    Else
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "Level", 0))
                    End If

                    If alt.AHPAltData IsNot Nothing Then
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "IsLeaf", alt.AHPAltData.IsLeaf))
                    Else
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "IsLeaf", True))
                    End If

                    If alt.AHPAltData IsNot Nothing Then
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "Selected", alt.AHPAltData.Selected))
                    Else
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "Selected", True))
                    End If

                    'C0984===
                    'C0429===
                    'If alt.AHPAltData IsNot Nothing Then
                    '    oCommand.Parameters.Add(GetDBParameter(ProviderType, "Period", alt.AHPAltData.Period))
                    'Else
                    '    oCommand.Parameters.Add(GetDBParameter(ProviderType, "Period", 0))
                    'End If

                    'If alt.AHPAltData IsNot Nothing Then
                    '    oCommand.Parameters.Add(GetDBParameter(ProviderType, "BasePeriodAID", alt.AHPAltData.BasePeriodAID))
                    'Else
                    '    oCommand.Parameters.Add(GetDBParameter(ProviderType, "BasePeriodAID", ""))
                    'End If
                    ''C0429==
                    'C0984==

                    'C0984===
                    If (alt.AHPAltData IsNot Nothing) AndAlso (alt.AHPAltData.Period <> UNDEFINED_INTEGER_VALUE) Then 'C0306
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "Period", alt.AHPAltData.Period))
                    Else
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "Period", 0))
                    End If

                    'If alt.AHPAltData IsNot Nothing Then 'C0306
                    If (alt.AHPAltData IsNot Nothing) AndAlso (alt.AHPAltData.BasePeriodAID <> UNDEFINED_STRING_VALUE) Then 'C0306
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "BasePeriodAID", alt.AHPAltData.BasePeriodAID))
                    Else
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "BasePeriodAID", ""))
                    End If
                    'C0984==

                    'C0306===
                    ''If alt.AHPAltData IsNot Nothing Then 'C0306
                    'If (alt.AHPAltData IsNot Nothing) AndAlso (alt.AHPAltData.Period <> UNDEFINED_INTEGER_VALUE) Then 'C0306
                    '    oCommand.Parameters.Add(GetDBParameter(ProviderType, "Period", alt.AHPAltData.Period))
                    'Else
                    '    oCommand.Parameters.Add(GetDBParameter(ProviderType, "Period", DBNull.Value))
                    'End If

                    ''If alt.AHPAltData IsNot Nothing Then 'C0306
                    'If (alt.AHPAltData IsNot Nothing) AndAlso (alt.AHPAltData.BasePeriodAID <> UNDEFINED_STRING_VALUE) Then 'C0306
                    '    oCommand.Parameters.Add(GetDBParameter(ProviderType, "BasePeriodAID", alt.AHPAltData.BasePeriodAID))
                    'Else
                    '    oCommand.Parameters.Add(GetDBParameter(ProviderType, "BasePeriodAID", DBNull.Value))
                    'End If
                    'C0306==

                    affected = DBExecuteNonQuery(ProviderType, oCommand)
                Catch ex As Exception
                    Transaction.Rollback() 'C0921
                    oCommand = Nothing
                    dbConnection.Close()
                    Return False
                End Try

                i += 1
            Next

            Transaction.Commit() 'C0921

            Transaction = dbConnection.BeginTransaction 'C0921
            oCommand.Transaction = Transaction 'C0921
            ' =============================
            ' Fill AltsActive table

            ' Add active alternatives IDs
            'For Each alt As clsNode In ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).TerminalNodes.Clone 'C0385
            For Each alt As clsNode In ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).TerminalNodes 'C0385
                Try
                    oCommand.CommandText = "INSERT INTO AltsActive (AID) VALUES (?)"
                    oCommand.Parameters.Clear()
                    'oCommand.Parameters.AddWithValue("AID", "A" + If(projectmanager.IsAltsZeroBased, alt.NodeID + 1, alt.NodeID).ToString) 'C0235
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "AID", "A" + If(IsAltsZeroBased(ProjectManager), alt.NodeID + 1, alt.NodeID).ToString)) 'C0235 'C0237

                    affected = DBExecuteNonQuery(ProviderType, oCommand)
                Catch ex As Exception
                    Transaction.Rollback() 'C0921
                    oCommand = Nothing
                    dbConnection.Close()
                    Return False
                End Try
            Next

            Transaction.Commit() 'C0921

            oCommand = Nothing
            If Not SavingToDatabaseInProgress() Then 'C0812
                dbConnection.Close()
            End If

            Return True
        End Function

        Private Function SaveNodes() As Boolean 'C0304
            If (ProjectManager Is Nothing) OrElse Not CheckDBConnection(ProviderType, Location) Then
                Return False
            End If

            'C0812===
            Dim dbConnection As DbConnection
            If SavingToDatabaseInProgress() Then
                dbConnection = mDBConnection
            Else
                dbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()
            End If
            'C0812==

            Dim oCommand As DbCommand = GetDBCommand(ProviderType)
            Dim affected As Integer

            oCommand.Connection = dbConnection

            'C0921===
            Dim Transaction As DbTransaction = dbConnection.BeginTransaction
            oCommand.Transaction = Transaction
            'C0921==

            ' =============================
            ' Fill NodeDefs table

            ' Delete everything from NodeDefs table
            oCommand.CommandText = "DELETE FROM NodeDefs"
            affected = DBExecuteNonQuery(ProviderType, oCommand)

            Dim nid As Integer
            Dim ParentNid As Integer
            Dim i As Integer = 1

            For Each node As clsNode In ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).Nodes
                Try
                    nid = If(IsNodesZeroBased(ProjectManager), node.NodeID + 1, node.NodeID)
                    If node.ParentNode IsNot Nothing Then
                        ParentNid = If(IsNodesZeroBased(ProjectManager), node.ParentNodeID + 1, node.ParentNodeID)
                    End If

                    oCommand.CommandText = "INSERT INTO NodeDefs (SOrder, NID, NodeName, Parent, StructuralAdjust, ProtectedJudgments, topp, Leftt, EnforceMode) " +
                                        "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)"
                    oCommand.Parameters.Clear()
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "SOrder", i))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "NID", "N" + nid.ToString))
                    'oCommand.Parameters.Add(GetDBParameter(ProviderType, "NodeName", node.NodeName)) 'C0727
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "NodeName", node.NodeName.Substring(0, If(node.NodeName.Length > 255, 255, node.NodeName.Length)))) 'C0727
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "Parent", If(node.ParentNode Is Nothing, "", "N" + ParentNid.ToString)))

                    If node.AHPNodeData IsNot Nothing Then 'C0306
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "StructuralAdjust", node.AHPNodeData.StructuralAdjust))
                    Else
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "StructuralAdjust", DBNull.Value))
                    End If

                    If node.AHPNodeData IsNot Nothing Then
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProtectedJudgments", node.AHPNodeData.ProtectedJudgments))
                    Else
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProtectedJudgments", DBNull.Value))
                    End If

                    'If node.AHPNodeData IsNot Nothing Then 'C0306
                    If (node.AHPNodeData IsNot Nothing) AndAlso (node.AHPNodeData.topp <> UNDEFINED_INTEGER_VALUE) Then 'C0306
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "topp", node.AHPNodeData.topp))
                    Else
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "topp", DBNull.Value))
                    End If

                    'If node.AHPNodeData IsNot Nothing Then 'C0306
                    If (node.AHPNodeData IsNot Nothing) AndAlso (node.AHPNodeData.Leftt <> UNDEFINED_INTEGER_VALUE) Then 'C0306
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "Leftt", node.AHPNodeData.Leftt))
                    Else
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "Leftt", DBNull.Value))
                    End If

                    If node.AHPNodeData IsNot Nothing Then 'C0306
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "EnforceMode", node.AHPNodeData.EnforceMode))
                    Else
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "EnforceMode", DBNull.Value))
                    End If

                    affected = DBExecuteNonQuery(ProviderType, oCommand)
                Catch ex As Exception
                    Transaction.Rollback() 'C0921
                    oCommand = Nothing
                    dbConnection.Close()
                    Return False
                End Try
                i += 1
            Next

            Transaction.Commit() 'C0921

            oCommand = Nothing
            If Not SavingToDatabaseInProgress() Then 'C0812
                dbConnection.Close()
            End If
            Return True
        End Function

        Private Function SaveUsersInfo() As Boolean 'C0304
            If (ProjectManager Is Nothing) OrElse Not CheckDBConnection(ProviderType, Location) Then
                Return False
            End If

            'C0812===
            Dim dbConnection As DbConnection
            If SavingToDatabaseInProgress() Then
                dbConnection = mDBConnection
            Else
                dbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()
            End If
            'C0812==

            'Dim oCommand As New odbc.odbcCommand 'C0235
            Dim oCommand As DbCommand = GetDBCommand(ProviderType) 'C0235
            Dim affected As Integer

            oCommand.Connection = dbConnection


            'C0921===
            Dim Transaction As DbTransaction = dbConnection.BeginTransaction
            oCommand.Transaction = Transaction
            'C0921==

            ' =============================
            ' Fill People table

            ' Delete everything from AltsGlobal table
            oCommand.CommandText = "DELETE FROM People"
            affected = DBExecuteNonQuery(ProviderType, oCommand)

            Dim uid As Integer

            'C0552===
            'If ProjectManager.UsersList.Count > 1 Then
            '    ProjectManager.AddCombinedUser()
            'End If
            'C0552==

            'Dim bIsUsingDefaultWeight As Boolean = IsUsingDefaultWeightForAllUsers() 'C0940 'C0950
            Dim bIsUsingDefaultWeight As Boolean = ProjectManager.IsUsingDefaultWeightForAllUsers() 'C0950

            Dim U As clsUser
            For i As Integer = 0 To ProjectManager.UsersList.Count - 1
                U = ProjectManager.UsersList(i)

                'If U.UserID <> COMBINED_USER_ID Then 'C0555
                If Not IsCombinedUserID(U.UserID) Then 'C0555
                    ' synchronize userid
                    'C0558===
                    'If ProjectManager.UserExists(1) Then
                    '    If U.UserID > 0 Then
                    '        uid = U.UserID + 1
                    '    Else
                    '        uid = U.UserID
                    '    End If
                    'Else
                    '    uid = U.UserID
                    'End If
                    'C0558==
                    uid = GetAHPUserID(ProjectManager, U) 'C0558

                    If i = 1 Then
                        Try
                            oCommand.CommandText = "INSERT INTO People (PID, Email, Location, PersonName, Combined, Participating, RoleWritingType, RoleViewingType) " +
                                                "VALUES (?, ?, ?, ?, ?, ?, ?, ?)"
                            'oCommand.CommandText = "INSERT INTO People (PID, Email, Location, PersonName, Combined, Participating, Eval, Weight, Keypad, Wave, Password, Organization, ProgressStatus, EvalCluster, LastChanged, RoleWritingType, RoleViewingType) " + _
                            '                        "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)"
                            oCommand.Parameters.Clear()
                            'C0235===
                            'oCommand.Parameters.AddWithValue("PID", 1)
                            'oCommand.Parameters.AddWithValue("Email", "")
                            'oCommand.Parameters.AddWithValue("Location", "") 'TODO: figure out this field
                            'oCommand.Parameters.AddWithValue("PersonName", "Combined")
                            'oCommand.Parameters.AddWithValue("Combined", True)
                            'oCommand.Parameters.AddWithValue("Participating", False)
                            'oCommand.Parameters.AddWithValue("RoleWritingType", 2)
                            'oCommand.Parameters.AddWithValue("RoleViewingType", 2)
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "PID", 1)) 'C0237
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "Email", "")) 'C0237
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "Location", "")) 'TODO: figure out this field 'C0237
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "PersonName", "Combined")) 'C0237
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "Combined", True)) 'C0237
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "Participating", False)) 'C0237
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "RoleWritingType", 2)) 'C0237
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "RoleViewingType", 2)) 'C0237
                            'C0235==
                            affected = DBExecuteNonQuery(ProviderType, oCommand)
                        Catch ex As Exception
                            Transaction.Rollback() 'C0921
                            oCommand = Nothing
                            dbConnection.Close()
                            Return False
                        End Try
                    End If

                    Try
                        'oCommand.CommandText = "INSERT INTO People (PID, Email, PersonName, Combined, Participating, Weight, Organization, Keypad, Wave, Location, Eval, EvalCluster, RoleWritingType, RoleViewingType) " + _
                        '                        "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)"
                        'oCommand.CommandText = "INSERT INTO People (PID, Email, Location, PersonName, Combined, Participating, Eval, Weight, Keypad, Wave, Password, Organization, ProgressStatus, EvalCluster, LastChanged, RoleWritingType, RoleViewingType) " + _
                        '                        "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)"
                        'oCommand.CommandText = "INSERT INTO People (PID, Email, Location, PersonName, Combined, Participating, RoleWritingType, RoleViewingType, [Group]) " + _
                        '                        "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)" 'ASGroups - added Group field in query

                        oCommand.CommandText = "INSERT INTO People (PID, Email, PersonName, Combined, Participating, Weight, Organization, Keypad, Wave, Location, Eval, EvalCluster, RoleWritingType, RoleViewingType, [Group]) " +
                                            "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)"

                        oCommand.Parameters.Clear()

                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "PID", uid))
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "Email", If(U.UserEMail.Length < 255, U.UserEMail, U.UserEMail.Substring(0, Math.Min(U.UserEMail.Length, 255)))))
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "PersonName", If(U.UserName.Length < 255, U.UserName, U.UserName.Substring(0, Math.Min(U.UserName.Length, 255)))))
                        'oCommand.Parameters.Add(GetDBParameter(ProviderType, "Combined", U.UserID = COMBINED_USER_ID)) 'C0555
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "Combined", IsCombinedUserID(U.UserID))) 'C0555
                        'TODO: COMBINED GROUPS
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "Participating", U.Active))

                        'If U.AHPUserData IsNot Nothing Then 'C0306

                        'C0851===
                        'If (U.AHPUserData IsNot Nothing) AndAlso (U.AHPUserData.Weight <> UNDEFINED_SINGLE_VALUE) Then 'C0306
                        '    oCommand.Parameters.Add(GetDBParameter(ProviderType, "Weight", U.AHPUserData.Weight))
                        'Else
                        '    oCommand.Parameters.Add(GetDBParameter(ProviderType, "Weight", DBNull.Value))
                        'End If
                        'C0851==

                        'oCommand.Parameters.Add(GetDBParameter(ProviderType, "Weight", U.Weight.ToString)) 'C0940
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "Weight", If(bIsUsingDefaultWeight, DBNull.Value, U.Weight.ToString))) 'C0940

                        'If U.AHPUserData IsNot Nothing Then 'C0306
                        If (U.AHPUserData IsNot Nothing) AndAlso (U.AHPUserData.Organization <> UNDEFINED_STRING_VALUE) Then 'C0306
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "Organization", If(U.AHPUserData.Organization.Length < 255, U.AHPUserData.Organization, U.AHPUserData.Organization.Substring(0, Math.Min(U.AHPUserData.Organization.Length, 255))))) 'C0237
                        Else
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "Organization", DBNull.Value)) 'C0237
                        End If

                        'If U.AHPUserData IsNot Nothing Then 'C0306

                        'C0737===
                        'If (U.AHPUserData IsNot Nothing) AndAlso (U.AHPUserData.Keypad <> UNDEFINED_INTEGER_VALUE) Then 'C0306
                        '    oCommand.Parameters.Add(GetDBParameter(ProviderType, "Keypad", U.AHPUserData.Keypad)) 'C0237
                        'Else
                        '    oCommand.Parameters.Add(GetDBParameter(ProviderType, "Keypad", DBNull.Value)) 'C0237
                        'End If

                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "Keypad", U.VotingBoxID)) 'C0737
                        'C0737==

                        'If U.AHPUserData IsNot Nothing Then 'C0306
                        If (U.AHPUserData IsNot Nothing) AndAlso (U.AHPUserData.Wave <> UNDEFINED_INTEGER_VALUE) Then 'C0306
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "Wave", U.AHPUserData.Wave)) 'C0237
                        Else
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "Wave", DBNull.Value)) 'C0237
                        End If

                        'If U.AHPUserData IsNot Nothing Then 'C0306
                        If (U.AHPUserData IsNot Nothing) AndAlso (U.AHPUserData.Location <> UNDEFINED_STRING_VALUE) Then 'C0306
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "Location", If(U.AHPUserData.Location.Length < 255, U.AHPUserData.Location, U.AHPUserData.Location.Substring(0, Math.Min(U.AHPUserData.Location.Length, 255)))))
                        Else
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "Location", DBNull.Value))
                        End If

                        'If U.AHPUserData IsNot Nothing Then 'C0306
                        If (U.AHPUserData IsNot Nothing) AndAlso (U.AHPUserData.Eval <> UNDEFINED_STRING_VALUE) Then 'C0306
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "Eval", U.AHPUserData.Eval))
                        Else
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "Eval", DBNull.Value))
                        End If

                        'If U.AHPUserData IsNot Nothing Then 'C0306
                        If (U.AHPUserData IsNot Nothing) AndAlso (U.AHPUserData.EvalCluster <> UNDEFINED_STRING_VALUE) Then 'C0306
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "EvalCluster", U.AHPUserData.EvalCluster))
                        Else
                            'oCommand.Parameters.Add(GetDBParameter(ProviderType, "EvalCluster", U.AHPUserData.EvalCluster)) 'C0306
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "EvalCluster", DBNull.Value)) 'C0306
                        End If

                        'If U.AHPUserData IsNot Nothing Then 'C0306
                        If (U.AHPUserData IsNot Nothing) AndAlso (U.AHPUserData.RoleWritingType <> UNDEFINED_INTEGER_VALUE) Then 'C0306
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "RoleWritingType", U.AHPUserData.RoleWritingType))
                        Else
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "RoleWritingType", DBNull.Value))
                        End If

                        'If U.AHPUserData IsNot Nothing Then 'C0306
                        If (U.AHPUserData IsNot Nothing) AndAlso (U.AHPUserData.RoleViewingType <> UNDEFINED_INTEGER_VALUE) Then 'C0306
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "RoleViewingType", U.AHPUserData.RoleViewingType))
                        Else
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "RoleViewingType", DBNull.Value))
                        End If

                        'ASGroups===
                        Dim GroupsString As String = "" ' here we will store the list of groups this users belongs to, will be stored in Group field
                        For Each CG As clsCombinedGroup In ProjectManager.CombinedGroups.GroupsList
                            If CG.ContainsUser(U.UserID) Then
                                GroupsString += If(GroupsString = "", "", ",") + CG.ID.ToString
                            End If
                        Next
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "Group", If(GroupsString.Length < 255, GroupsString, GroupsString.Substring(0, Math.Min(GroupsString.Length, 255)))))
                        'ASGroups==

                        affected = DBExecuteNonQuery(ProviderType, oCommand)
                    Catch ex As Exception
                        Transaction.Rollback() 'C0921
                        oCommand = Nothing
                        dbConnection.Close()
                        Return False
                    End Try
                End If
            Next

            Transaction.Commit() 'C0921

            oCommand = Nothing
            If Not SavingToDatabaseInProgress() Then 'C0812
                dbConnection.Close()
            End If
            Return True
        End Function


        Private Function SaveExtraAHPTables() 'C0342
            If ProjectManager Is Nothing Then Return False 'C0812
            If Not SavingToDatabaseInProgress() AndAlso Not CheckDBConnection(ProviderType, Location) Then Return False 'C0812

            'C0812===
            'If (ProjectManager Is Nothing) orelse Not CheckDBConnection(ProviderType, Location) Then
            '    Return False
            'End If
            'C0812==

            'Dim dbConnection As New odbc.odbcConnection(Location) 'C0235

            'C0812===
            'Dim dbConnection As DbConnection = GetDBConnection(ProviderType, Location) 'C0235
            'dbConnection.Open()
            'C0812==

            'C0812===
            Dim dbConnection As DbConnection
            If SavingToDatabaseInProgress() Then
                dbConnection = mDBConnection
            Else
                dbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()
            End If
            'C0812==

            Dim DS As New DataSet
            DS.ReadXml(ProjectManager.ExtraAHPTables, XmlReadMode.ReadSchema)

            'C0426===
            Dim DynamicTablesList As New List(Of String)
            For Each tableName As String In _AHP__DYNAMIC_EXTRATABLES_LIST
                DynamicTablesList.Add(tableName.ToLower)
            Next
            'C0426==

            Dim affected As Integer 'C0426

            For Each table As DataTable In DS.Tables
                If TableExists(dbConnection.ConnectionString, ProviderType, table.TableName) Then 'C0398
                    Try 'C0426 - added Try...Catch
                        Dim DA As DbDataAdapter = GetDBDataAdapter(ProviderType, "SELECT * FROM " + table.TableName, dbConnection)
                        'DA.FillSchema(table, SchemaType.Source)
                        Dim cb As Data.Common.DbCommandBuilder = GetDBCommandBuilder(ProviderType, DA)
                        cb.QuotePrefix = "["
                        cb.QuoteSuffix = "]"
                        DA.InsertCommand = cb.GetInsertCommand
                        'DA.InsertCommand.CommandText = DA.InsertCommand.CommandText.Replace("Connection", "[Connection]")
                        affected = DA.Update(DS, table.TableName)
                        'DS.AcceptChanges()

                        DynamicTablesList.Remove(table.TableName.ToLower) 'C0426
                    Catch ex As Exception
                        'Debug.Print("Error in table + " + table.TableName + ": " + ex.Message)
                    End Try
                End If
            Next
            DS.AcceptChanges()

            'C0426===
            'Dim oCommand As DbCommand = dbConnection.CreateCommand
            'For Each tableName As String In DynamicTablesList
            '    Try
            '        oCommand.CommandText = "DROP TABLE " + tableName
            '        affected = DBExecuteNonQuery(ProviderType, oCommand)
            '    Catch ex As Exception
            '        'Debug.Print("Failed to drop table " + tableName + " with Error: " + ex.Message)
            '    End Try
            'Next
            'C0426==

            'If dbConnection IsNot mDBConnection Then 'C0812
            If Not SavingToDatabaseInProgress() Then 'C0812
                dbConnection.Close()
            End If

            Return True
        End Function

        Private Sub SaveSimpleProperty(ByVal oCommand As DbCommand, ByVal PropertyName As String, ByVal DefaultPropertyValue As String) 'C0825
            oCommand.CommandText = "SELECT COUNT(*) FROM MProperties WHERE PropertyName=?"
            oCommand.Parameters.Clear()
            oCommand.Parameters.Add(GetDBParameter(ProviderType, "PropertyName", PropertyName))
            Dim obj As Object = DBExecuteScalar(ProviderType, oCommand)
            Dim count As Integer = If(obj Is Nothing, 0, CType(obj, Integer))

            If count < 1 Then
                oCommand.CommandText = "INSERT INTO MProperties (PropertyName, PValue) " +
                                            "VALUES (?, ?)"
                oCommand.Parameters.Clear()
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "PropertyName", PropertyName))
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "PValue", DefaultPropertyValue))
                Dim affected As Integer = DBExecuteNonQuery(ProviderType, oCommand)
            End If
        End Sub

        Private Sub SaveCalculatedProperty(ByVal oCommand As DbCommand, ByVal PropertyName As String, ByVal DefaultPropertyValue As String) 'C0825
            oCommand.CommandText = "UPDATE MProperties SET PValue=? WHERE PropertyName=?"
            oCommand.Parameters.Clear()
            oCommand.Parameters.Add(GetDBParameter(ProviderType, "PValue", DefaultPropertyValue))
            oCommand.Parameters.Add(GetDBParameter(ProviderType, "PropertyName", PropertyName))
            Dim affected As Integer = DBExecuteNonQuery(ProviderType, oCommand)
            If affected = 0 Then
                oCommand.CommandText = "INSERT INTO MProperties (PValue,PropertyName) " +
                                            "VALUES (?, ?)"
                affected = DBExecuteNonQuery(ProviderType, oCommand)
            End If
        End Sub

        Private Function SaveMProperties() As Boolean
            If ProjectManager Is Nothing Then Return False 'C0812
            If Not SavingToDatabaseInProgress() AndAlso Not CheckDBConnection(ProviderType, Location) Then Return False 'C0812

            Dim dbConnection As DbConnection
            If SavingToDatabaseInProgress() Then
                dbConnection = mDBConnection
            Else
                dbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()
            End If

            Dim oCommand As DbCommand = GetDBCommand(ProviderType)

            oCommand.Connection = dbConnection

            'C0921===
            Dim Transaction As DbTransaction = dbConnection.BeginTransaction
            oCommand.Transaction = Transaction
            'C0921==

            ' =============================
            ' Fill MProperties table

            Try
                SaveCalculatedProperty(oCommand, "HideIdealAlternative", If(ProjectManager.CalculationsManager.ShowIdealAlternative And ProjectManager.CalculationsManager.IncludeIdealAlternative, "0", "-1"))
                SaveCalculatedProperty(oCommand, "IdealAlt", If(ProjectManager.CalculationsManager.IncludeIdealAlternative, "-1", "0"))
                SaveCalculatedProperty(oCommand, "NextAltID", GetNextAltID(ProjectManager))
                SaveCalculatedProperty(oCommand, "NextIntensityID", GetNextIntensityID)
                SaveCalculatedProperty(oCommand, "NextNodeID", GetNextNodeID(ProjectManager))
                SaveCalculatedProperty(oCommand, "NextPersonID", GetNextUserID(ProjectManager))
                SaveCalculatedProperty(oCommand, "SynIdeal", If(ProjectManager.CalculationsManager.SynthesisMode = ECSynthesisMode.smIdeal, "-1", "0"))
                SaveCalculatedProperty(oCommand, "RecalculationNeeded", True.ToString)
                'SaveCalculatedProperty(oCommand, "Version", "3.18") 'AS/11-20-15
                'SaveCalculatedProperty(oCommand, "Version", AHP_DB_LATEST_VERSION) 'AS/11-20-15 'AS/6-15-16
                SaveCalculatedProperty(oCommand, "Version", AHP_DB_DEFAULT_VERSION) 'AS/11-20-15 'AS/6-15-16
                SaveCalculatedProperty(oCommand, "LastDownloadedAsAHP", DateTime.Now.ToString) 'AS/11-16-15
                SaveCalculatedProperty(oCommand, "LastUploadedToECC", "") 'AS/11-16-15
                SaveCalculatedProperty(oCommand, "ECC_AHPversion", AHP_DB_LATEST_VERSION) 'AS/6-28-16

                '==================

                SaveSimpleProperty(oCommand, "AllowGuests", "-1")
                SaveSimpleProperty(oCommand, "apVD", "0")
                SaveSimpleProperty(oCommand, "apVP", "0")
                SaveSimpleProperty(oCommand, "CopyToPartcipantOption", "") 'C0930
                SaveSimpleProperty(oCommand, "CurrentPSID", "0")
                SaveSimpleProperty(oCommand, "DecisionBottomUp", "0")
                SaveSimpleProperty(oCommand, "DecisionPhase", "0")
                SaveSimpleProperty(oCommand, "DecisionStep", "0")
                SaveSimpleProperty(oCommand, "DecisionType", "1")
                SaveSimpleProperty(oCommand, "GuestCopyOption", "3")
                SaveSimpleProperty(oCommand, "NewUserPassword", "")
                SaveSimpleProperty(oCommand, "NextCVID", "1")
                SaveSimpleProperty(oCommand, "NextProConID", "1")
                SaveSimpleProperty(oCommand, "NextPSID", "1")
                SaveSimpleProperty(oCommand, "NextUDColID", "1")
                SaveSimpleProperty(oCommand, "OpenOption", "0")
                SaveSimpleProperty(oCommand, "PortalDeployed", "")
                SaveSimpleProperty(oCommand, "PortalSettings", "0")
                SaveSimpleProperty(oCommand, "posUDCol", "1")
                SaveSimpleProperty(oCommand, "qPeopleActive", "SELECT * FROM(People) where Participating <> 0 ORDER BY PID;")
                SaveSimpleProperty(oCommand, "ShowCategories", "False")
                SaveSimpleProperty(oCommand, "ShowMapKey", "False")
                SaveSimpleProperty(oCommand, "ShowRisks", "False")
                SaveSimpleProperty(oCommand, "StoreAllowedRoles", "False")
                SaveSimpleProperty(oCommand, "ZipFileName", "")
                SaveSimpleProperty(oCommand, "ZipModel", "True")
                SaveSimpleProperty(oCommand, "RiskModelLocation", "")
                SaveSimpleProperty(oCommand, "ApplyDiscount", "True")
                SaveSimpleProperty(oCommand, "EnableTimePeriods", "False")
                SaveSimpleProperty(oCommand, "posAlts", "3")
                SaveSimpleProperty(oCommand, "posTotals", "4")
                SaveSimpleProperty(oCommand, "posCosts", "5")
                SaveSimpleProperty(oCommand, "posUCCol", "5")
                SaveSimpleProperty(oCommand, "LaunchRAforComparion", "0")
                SaveSimpleProperty(oCommand, "ShowTotals", "True")
                SaveSimpleProperty(oCommand, "ShowCosts", "0")
                SaveSimpleProperty(oCommand, "ShowUDcolumns", "0")
                'SaveSimpleProperty(oCommand, "Version", "3.10") 'AS changed db version from 3.09 to 3.10
                SaveSimpleProperty(oCommand, "NextPGID", GetNextGroupID(ProjectManager)) 'ASGroups
                SaveSimpleProperty(oCommand, "HasOnlyOneDirectForNonCovObj", mHasOnlyOneDirectJudgmentForNonCovObj.ToString)

            Catch ex As Exception
                Transaction.Rollback() 'C0921
                oCommand = Nothing
                If Not SavingToDatabaseInProgress() Then 'C0812
                    dbConnection.Close()
                End If
                Return False
            End Try

            Transaction.Commit() 'C0921

            oCommand = Nothing
            If Not SavingToDatabaseInProgress() Then 'C0812
                dbConnection.Close()
            End If
            Return True
        End Function

        Private Function SaveUserPermissions() As Boolean 'ASGroups
            If (ProjectManager Is Nothing) OrElse Not CheckDBConnection(ProviderType, Location) Then
                Return False
            End If

            Dim dbConnection As DbConnection
            If SavingToDatabaseInProgress() Then
                dbConnection = mDBConnection
            Else
                dbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()
            End If

            Dim oCommand As DbCommand = GetDBCommand(ProviderType) 'C0235
            Dim affected As Integer

            oCommand.Connection = dbConnection

            Dim transaction As DbTransaction 'C0235

            ' =============================
            ' Fill RoleEvaluateNodes and RoleEvaluateJD tables

            ' Delete everything from RoleEvaluateNodes and RoleEvaluateJD tables
            oCommand.CommandText = "DELETE FROM RoleEvaluateNodes"
            affected = DBExecuteNonQuery(ProviderType, oCommand)

            oCommand.CommandText = "DELETE FROM RoleEvaluateJD"
            affected = DBExecuteNonQuery(ProviderType, oCommand)

            Dim nid As Integer
            Dim userID As Integer

            For Each U As clsUser In ProjectManager.UsersList
                transaction = dbConnection.BeginTransaction()
                oCommand.Transaction = transaction

                If Not IsCombinedUserID(U.UserID) Then
                    userID = GetAHPUserID(ProjectManager, U)

                    ' writing objectives roles
                    Dim AllowedObjectives As List(Of Guid) = ProjectManager.UsersRoles.GetAllowedObjectives(U.UserID)
                    Dim RestrictedObjectives As List(Of Guid) = ProjectManager.UsersRoles.GetRestrictedObjectives(U.UserID)

                    If AllowedObjectives IsNot Nothing Then
                        For Each ObjGuid As Guid In AllowedObjectives
                            Dim node As clsNode = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).GetNodeByID(ObjGuid)
                            If node IsNot Nothing Then
                                nid = If(IsNodesZeroBased(ProjectManager), node.NodeID + 1, node.NodeID)
                                Try
                                    oCommand.CommandText = "INSERT INTO RoleEvaluateNodes (PID, NID, RoleState, RoleType) " +
                                                        "VALUES (?, ?, ?, ?)"
                                    oCommand.Parameters.Clear()
                                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "PID", userID))
                                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "NID", "N" + nid.ToString))
                                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "RoleState", 1))
                                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "RoleType", 1))
                                    affected = DBExecuteNonQuery(ProviderType, oCommand)
                                Catch ex As Exception
                                    transaction.Rollback()
                                    oCommand = Nothing
                                    dbConnection.Close()
                                    Return False
                                End Try
                            End If
                        Next
                    End If

                    If RestrictedObjectives IsNot Nothing Then
                        For Each ObjGuid As Guid In RestrictedObjectives
                            Dim node As clsNode = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).GetNodeByID(ObjGuid)
                            If node IsNot Nothing Then
                                nid = If(IsNodesZeroBased(ProjectManager), node.NodeID + 1, node.NodeID)
                                Try
                                    oCommand.CommandText = "INSERT INTO RoleEvaluateNodes (PID, NID, RoleState, RoleType) " +
                                                        "VALUES (?, ?, ?, ?)"
                                    oCommand.Parameters.Clear()
                                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "PID", userID))
                                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "NID", "N" + nid.ToString))
                                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "RoleState", If(node.IsTerminalNode, 1, 2)))
                                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "RoleType", 1))
                                    affected = DBExecuteNonQuery(ProviderType, oCommand)
                                Catch ex As Exception
                                    transaction.Rollback()
                                    oCommand = Nothing
                                    dbConnection.Close()
                                    Return False
                                End Try
                            End If
                        Next
                    End If

                    ' writing alternatives roles

                    For Each node As clsNode In ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).TerminalNodes
                        If node.IsAllowed(U.UserID) Then
                            nid = If(IsNodesZeroBased(ProjectManager), node.NodeID + 1, node.NodeID)

                            Dim AllowedAlternatives As HashSet(Of Guid) = ProjectManager.UsersRoles.GetAllowedAlternatives(U.UserID, node.NodeGuidID)
                            Dim RestrictedAlternatives As HashSet(Of Guid) = ProjectManager.UsersRoles.GetRestrictedAlternatives(U.UserID, node.NodeGuidID)

                            If AllowedAlternatives IsNot Nothing Then
                                For Each AltGuid As Guid In AllowedAlternatives
                                    Dim alt As clsNode = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(AltGuid)
                                    If alt IsNot Nothing Then
                                        Dim aid As Integer = If(IsAltsZeroBased(ProjectManager), alt.NodeID + 1, alt.NodeID)
                                        Try
                                            oCommand.CommandText = "INSERT INTO RoleEvaluateJD (PID, WRT, EID, RoleState, RoleType) " +
                                                                "VALUES (?, ?, ?, ?, ?)"
                                            oCommand.Parameters.Clear()
                                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "PID", userID))
                                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "WRT", "N" + nid.ToString))
                                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "EID", "A" + aid.ToString))
                                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "RoleState", 1))
                                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "RoleType", 1))
                                            affected = DBExecuteNonQuery(ProviderType, oCommand)
                                        Catch ex As Exception
                                            transaction.Rollback()
                                            oCommand = Nothing
                                            dbConnection.Close()
                                            Return False
                                        End Try
                                    End If
                                Next
                            End If

                            If RestrictedAlternatives IsNot Nothing Then
                                For Each AltGuid As Guid In RestrictedAlternatives
                                    Dim alt As clsNode = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(AltGuid)
                                    If alt IsNot Nothing Then
                                        Dim aid As Integer = If(IsAltsZeroBased(ProjectManager), alt.NodeID + 1, alt.NodeID)
                                        Try
                                            oCommand.CommandText = "INSERT INTO RoleEvaluateJD (PID, WRT, EID, RoleState, RoleType) " +
                                                                "VALUES (?, ?, ?, ?, ?)"
                                            oCommand.Parameters.Clear()
                                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "PID", userID))
                                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "WRT", "N" + nid.ToString))
                                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "EID", "A" + aid.ToString))
                                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "RoleState", 2))
                                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "RoleType", 1))
                                            affected = DBExecuteNonQuery(ProviderType, oCommand)
                                        Catch ex As Exception
                                            transaction.Rollback()
                                            oCommand = Nothing
                                            dbConnection.Close()
                                            Return False
                                        End Try
                                    End If
                                Next
                            End If
                        End If
                    Next
                End If
                transaction.Commit()
            Next

            oCommand = Nothing
            If Not SavingToDatabaseInProgress() Then
                dbConnection.Close()
            End If
            Return True
        End Function

        Private Function SaveGroups() As Boolean 'ASGroups
            If (ProjectManager Is Nothing) OrElse Not CheckDBConnection(ProviderType, Location) Then
                Return False
            End If

            Dim dbConnection As DbConnection
            If SavingToDatabaseInProgress() Then
                dbConnection = mDBConnection
            Else
                dbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()
            End If

            Dim oCommand As DbCommand = GetDBCommand(ProviderType) 'C0235
            Dim affected As Integer

            oCommand.Connection = dbConnection

            Dim transaction As DbTransaction 'C0235

            ' =============================
            ' Fill PeopleGroups and RolesForGroups tables

            ' Delete everything from PeopleGroups and RolesForGroups tables
            oCommand.CommandText = "DELETE FROM PeopleGroups"
            affected = DBExecuteNonQuery(ProviderType, oCommand)

            oCommand.CommandText = "DELETE FROM PeopleGroupsMembers"
            affected = DBExecuteNonQuery(ProviderType, oCommand)

            oCommand.CommandText = "DELETE FROM RolesForGroups"
            affected = DBExecuteNonQuery(ProviderType, oCommand)


            'New List<C>().Cast<A>().ToList();
            'ProjectManager.CombinedGroups.GroupsList.Sort(New clsCombinedUserIDDecreasingComparer)
            Dim cGroups As List(Of clsCombinedGroup) = ProjectManager.CombinedGroups.GroupsList.Cast(Of clsCombinedGroup).ToList
            cGroups.Sort(New clsCombinedUserIDDecreasingComparer)
            For i As Integer = 0 To cGroups.Count - 1
                CType(ProjectManager.CombinedGroups.GroupsList(i), clsCombinedGroup).ID = i
            Next

            ' fix GroupIDs for AHP so they start from 1, not from zero
            Dim IsZeroBased = IsGroupsZeroBased(ProjectManager)
            If IsZeroBased Then
                For Each CG As clsCombinedGroup In ProjectManager.CombinedGroups.GroupsList
                    CG.ID += 1
                Next
            End If

            ' filling PeopleGroups table
            For Each CG As clsCombinedGroup In ProjectManager.CombinedGroups.GroupsList
                transaction = dbConnection.BeginTransaction()
                oCommand.Transaction = transaction

                Try
                    oCommand.CommandText = "INSERT INTO PeopleGroups (PGroupID, PGroupName) " +
                                        "VALUES (?, ?)"
                    oCommand.Parameters.Clear()
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "PGroupID", CG.ID.ToString))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "PGroupName", CG.Name))
                    affected = DBExecuteNonQuery(ProviderType, oCommand)
                Catch ex As Exception
                    If IsZeroBased Then
                        For Each G As clsCombinedGroup In ProjectManager.CombinedGroups.GroupsList
                            G.ID -= 1
                        Next
                    End If

                    transaction.Rollback()
                    oCommand = Nothing
                    dbConnection.Close()
                    Return False
                End Try


                For Each user As clsUser In CG.UsersList
                    Dim userid As Integer = GetAHPUserID(ProjectManager, user)
                    Try
                        oCommand.CommandText = "INSERT INTO PeopleGroupsMembers (PGroupID, PID) " +
                                            "VALUES (?, ?)"
                        oCommand.Parameters.Clear()
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "PGroupID", CG.ID.ToString))
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "PID", userid))
                        affected = DBExecuteNonQuery(ProviderType, oCommand)
                    Catch ex As Exception
                        If IsZeroBased Then
                            For Each G As clsCombinedGroup In ProjectManager.CombinedGroups.GroupsList
                                G.ID -= 1
                            Next
                        End If

                        transaction.Rollback()
                        oCommand = Nothing
                        dbConnection.Close()
                        Return False
                    End Try
                Next
                transaction.Commit()
            Next

            For Each CG As clsCombinedGroup In ProjectManager.CombinedGroups.GroupsList
                transaction = dbConnection.BeginTransaction()
                oCommand.Transaction = transaction

                ' saving objectives roles for groups
                Dim AllowedObjectives As List(Of Guid) = ProjectManager.UsersRoles.GetAllowedObjectives(CG.CombinedUserID)
                Dim RestrictedObjectives As List(Of Guid) = ProjectManager.UsersRoles.GetRestrictedObjectives(CG.CombinedUserID)

                If AllowedObjectives IsNot Nothing Then
                    For Each ObjGuid As Guid In AllowedObjectives
                        Dim node As clsNode = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).GetNodeByID(ObjGuid)
                        If node IsNot Nothing Then
                            Dim nid As Integer = If(IsNodesZeroBased(ProjectManager), node.NodeID + 1, node.NodeID)
                            Try
                                oCommand.CommandText = "INSERT INTO RolesForGroups (GroupID, WRT, EID, RoleState, RoleType) " +
                                                    "VALUES (?, ?, ?, ?, ?)"
                                oCommand.Parameters.Clear()
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "GroupID", CG.ID))
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "WRT", "N" + nid.ToString))
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "EID", ""))
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "RoleState", 1))
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "RoleType", 1))
                                affected = DBExecuteNonQuery(ProviderType, oCommand)
                            Catch ex As Exception
                                If IsZeroBased Then
                                    For Each G As clsCombinedGroup In ProjectManager.CombinedGroups.GroupsList
                                        G.ID -= 1
                                    Next
                                End If

                                transaction.Rollback()
                                oCommand = Nothing
                                dbConnection.Close()
                                Return False
                            End Try
                        End If

                    Next
                End If

                If RestrictedObjectives IsNot Nothing Then
                    For Each ObjGuid As Guid In RestrictedObjectives
                        Dim node As clsNode = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).GetNodeByID(ObjGuid)
                        If node IsNot Nothing Then
                            Dim nid As Integer = If(IsNodesZeroBased(ProjectManager), node.NodeID + 1, node.NodeID)
                            Try
                                oCommand.CommandText = "INSERT INTO RolesForGroups (GroupID, WRT, EID, RoleState, RoleType) " +
                                                    "VALUES (?, ?, ?, ?, ?)"
                                oCommand.Parameters.Clear()
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "GroupID", CG.ID))
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "WRT", "N" + nid.ToString))
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "EID", ""))
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "RoleState", If(node.IsTerminalNode, 1, 2)))
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "RoleType", 1))
                                affected = DBExecuteNonQuery(ProviderType, oCommand)
                            Catch ex As Exception
                                If IsZeroBased Then
                                    For Each G As clsCombinedGroup In ProjectManager.CombinedGroups.GroupsList
                                        G.ID -= 1
                                    Next
                                End If

                                transaction.Rollback()
                                oCommand = Nothing
                                dbConnection.Close()
                                Return False
                            End Try
                        End If
                    Next
                End If

                ' save undefined roles only for default group
                If CG.CombinedUserID = COMBINED_USER_ID Then
                    Dim UndefinedObjectives As List(Of Guid) = ProjectManager.UsersRoles.GetUndefinedObjectivesForDefaultGroup()
                    If UndefinedObjectives IsNot Nothing Then
                        For Each ObjGuid As Guid In UndefinedObjectives
                            Dim node As clsNode = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).GetNodeByID(ObjGuid)
                            If node IsNot Nothing Then
                                Dim nid As Integer = If(IsNodesZeroBased(ProjectManager), node.NodeID + 1, node.NodeID)
                                Try
                                    oCommand.CommandText = "INSERT INTO RolesForGroups (GroupID, WRT, EID, RoleState, RoleType) " +
                                                        "VALUES (?, ?, ?, ?, ?)"
                                    oCommand.Parameters.Clear()
                                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "GroupID", CG.ID))
                                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "WRT", "N" + nid.ToString))
                                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "EID", ""))
                                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "RoleState", 0))
                                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "RoleType", 1))
                                    affected = DBExecuteNonQuery(ProviderType, oCommand)
                                Catch ex As Exception
                                    If IsZeroBased Then
                                        For Each G As clsCombinedGroup In ProjectManager.CombinedGroups.GroupsList
                                            G.ID -= 1
                                        Next
                                    End If

                                    transaction.Rollback()
                                    oCommand = Nothing
                                    dbConnection.Close()
                                    Return False
                                End Try
                            End If

                        Next
                    End If
                End If

                ' saving alternatives roles for groups
                For Each node As clsNode In ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).TerminalNodes
                    Dim nid As Integer = If(IsNodesZeroBased(ProjectManager), node.NodeID + 1, node.NodeID)
                    Dim AllowedAlternatives As HashSet(Of Guid) = ProjectManager.UsersRoles.GetAllowedAlternatives(CG.CombinedUserID, node.NodeGuidID)

                    If AllowedAlternatives IsNot Nothing Then
                        For Each AltGuid As Guid In AllowedAlternatives
                            Dim alt As clsNode = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(AltGuid)
                            If alt IsNot Nothing Then
                                Dim aid As Integer = If(IsAltsZeroBased(ProjectManager), alt.NodeID + 1, alt.NodeID)
                                Try
                                    oCommand.CommandText = "INSERT INTO RolesForGroups (GroupID, WRT, EID, RoleState, RoleType) " +
                                                        "VALUES (?, ?, ?, ?, ?)"
                                    oCommand.Parameters.Clear()
                                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "GroupID", CG.ID))
                                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "WRT", "N" + nid.ToString))
                                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "EID", "A" + aid.ToString))
                                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "RoleState", 1))
                                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "RoleType", 1))
                                    affected = DBExecuteNonQuery(ProviderType, oCommand)
                                Catch ex As Exception
                                    If IsZeroBased Then
                                        For Each G As clsCombinedGroup In ProjectManager.CombinedGroups.GroupsList
                                            G.ID -= 1
                                        Next
                                    End If

                                    transaction.Rollback()
                                    oCommand = Nothing
                                    dbConnection.Close()
                                    Return False
                                End Try
                            End If
                        Next
                    End If

                    Dim RestrictedAlternatives As HashSet(Of Guid) = ProjectManager.UsersRoles.GetRestrictedAlternatives(CG.CombinedUserID, node.NodeGuidID)
                    If RestrictedAlternatives IsNot Nothing Then
                        For Each AltGuid As Guid In RestrictedAlternatives
                            Dim alt As clsNode = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(AltGuid)
                            If alt IsNot Nothing Then
                                Dim aid As Integer = If(IsAltsZeroBased(ProjectManager), alt.NodeID + 1, alt.NodeID)
                                Try
                                    oCommand.CommandText = "INSERT INTO RolesForGroups (GroupID, WRT, EID, RoleState, RoleType) " +
                                                        "VALUES (?, ?, ?, ?, ?)"
                                    oCommand.Parameters.Clear()
                                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "GroupID", CG.ID))
                                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "WRT", "N" + nid.ToString))
                                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "EID", "A" + aid.ToString))
                                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "RoleState", 2))
                                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "RoleType", 1))
                                    affected = DBExecuteNonQuery(ProviderType, oCommand)
                                Catch ex As Exception
                                    If IsZeroBased Then
                                        For Each G As clsCombinedGroup In ProjectManager.CombinedGroups.GroupsList
                                            G.ID -= 1
                                        Next
                                    End If

                                    transaction.Rollback()
                                    oCommand = Nothing
                                    dbConnection.Close()
                                    Return False
                                End Try
                            End If
                        Next
                    End If

                    ' write undefined alts only for default group
                    If CG.CombinedUserID = COMBINED_USER_ID Then
                        Dim UndefinedAlternatives As HashSet(Of Guid) = ProjectManager.UsersRoles.GetUndefinedAlternativesForDefaultGroup(node.NodeGuidID)
                        If UndefinedAlternatives IsNot Nothing Then
                            For Each AltGuid As Guid In UndefinedAlternatives
                                Dim alt As clsNode = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(AltGuid)
                                If alt IsNot Nothing Then
                                    Dim aid As Integer = If(IsAltsZeroBased(ProjectManager), alt.NodeID + 1, alt.NodeID)
                                    Try
                                        oCommand.CommandText = "INSERT INTO RolesForGroups (GroupID, WRT, EID, RoleState, RoleType) " +
                                                            "VALUES (?, ?, ?, ?, ?)"
                                        oCommand.Parameters.Clear()
                                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "GroupID", CG.ID))
                                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "WRT", "N" + nid.ToString))
                                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "EID", "A" + aid.ToString))
                                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "RoleState", 0))
                                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "RoleType", 1))
                                        affected = DBExecuteNonQuery(ProviderType, oCommand)
                                    Catch ex As Exception
                                        If IsZeroBased Then
                                            For Each G As clsCombinedGroup In ProjectManager.CombinedGroups.GroupsList
                                                G.ID -= 1
                                            Next
                                        End If

                                        transaction.Rollback()
                                        oCommand = Nothing
                                        dbConnection.Close()
                                        Return False
                                    End Try
                                End If
                            Next
                        End If
                    End If
                Next
                transaction.Commit()
            Next

            If IsZeroBased Then
                For Each CG As clsCombinedGroup In ProjectManager.CombinedGroups.GroupsList
                    CG.ID -= 1
                Next
            End If


            oCommand = Nothing
            If Not SavingToDatabaseInProgress() Then
                dbConnection.Close()
            End If
            Return True
        End Function

        Private Function SaveRAConstraints() As Boolean
            If (ProjectManager Is Nothing) OrElse Not CheckDBConnection(ProviderType, Location) Then
                Return False
            End If

            Dim dbConnection As DbConnection
            If SavingToDatabaseInProgress() Then
                dbConnection = mDBConnection
            Else
                dbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()
            End If

            Dim oCommand As DbCommand = GetDBCommand(ProviderType)
            Dim affected As Integer

            oCommand.Connection = dbConnection

            oCommand.CommandText = "DELETE FROM RAconstraints"
            affected = DBExecuteNonQuery(ProviderType, oCommand)

            Dim aid As Integer
            Dim i As Integer = 1
            'Dim dummyCCID As Long 'AS/10-23-15 AS/11-18-15a
            For Each scenario As RAScenario In ProjectManager.ResourceAligner.Scenarios.Scenarios.Values
                For Each constraint As RAConstraint In scenario.Constraints.Constraints.Values
                    For Each cData As KeyValuePair(Of String, Double) In constraint.AlternativesData
                        'dummyCCID = dummyCCID - 1 AS/11-18-15a


                        Dim alt As clsNode = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(New Guid(cData.Key))
                        If alt IsNot Nothing Then
                            Try
                                aid = If(IsAltsZeroBased(ProjectManager), alt.NodeID + 1, alt.NodeID)

                                oCommand.CommandText = "INSERT INTO RAconstraints (SOrder, AID, value1, PSid, RAconstraint, CCID, AssociatedUDcolKey, AssociatedCVID, ECC_LinkedAttributeID, ECC_LinkedEnumID, ECC_ID) " +
                                                    "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)" 'AS/8-11-15  'AS/11-10-15 added Associated... and ECC_<...> fields
                                oCommand.Parameters.Clear()
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "SOrder", i))
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "AID", "A" + aid.ToString))
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "value1", cData.Value))
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "PSid", scenario.ID))
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "RAconstraint", constraint.Name))
                                'oCommand.Parameters.Add(GetDBParameter(ProviderType, "CCID", dummyCCID)) 'AS/8-11-15 'AS/10-23-15 replaced "-1" with dummyCCID AS/11-18-15a
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "CCID", constraint.ID)) 'AS/8-11-15 'AS/10-23-15 replaced "-1" with dummyCCID AS/11-18-15a
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "AssociatedUDcolKey", constraint.ECD_AssociatedUDcolKey)) 'AS/11-10-15===
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "AssociatedCVID", constraint.ECD_AssociatedCVID))
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "ECC_LinkedAttributeID", constraint.LinkedAttributeID.ToString))
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "ECC_LinkedEnumID", constraint.LinkedEnumID.ToString))
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "ECC_ID", constraint.ID)) 'AS/11-10-15==

                                affected = DBExecuteNonQuery(ProviderType, oCommand)
                            Catch ex As Exception
                                oCommand = Nothing
                                dbConnection.Close()
                                Return False
                            End Try
                            i += 1
                        End If
                    Next

                    If constraint.AlternativesData.Count = 0 Then 'AS/8-21-15===
                        Try
                            oCommand.CommandText = "INSERT INTO RAconstraints (SOrder, AID, value1, PSid, RAconstraint, CCID, AssociatedUDcolKey, AssociatedCVID, ECC_LinkedAttributeID, ECC_LinkedEnumID, ECC_ID) " +
                                                    "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)" 'AS/8-11-15  'AS/11-10-15 added Associated... and ECC_<...> fields
                            oCommand.Parameters.Clear()
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "SOrder", i))
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "AID", ""))
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "value1", DBNull.Value))
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "PSid", scenario.ID))
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "RAconstraint", constraint.Name))
                            'oCommand.Parameters.Add(GetDBParameter(ProviderType, "CCID", dummyCCID)) 'AS/10-23-15 replaced "-1" with dummyCCID AS/11-18-15a
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "CCID", constraint.ID)) 'AS/10-23-15 replaced "-1" with dummyCCID AS/11-18-15a
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "AssociatedUDcolKey", constraint.ECD_AssociatedUDcolKey)) 'AS/11-10-15===
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "AssociatedCVID", constraint.ECD_AssociatedCVID))
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "ECC_LinkedAttributeID", constraint.LinkedAttributeID.ToString))
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "ECC_LinkedEnumID", constraint.LinkedEnumID.ToString))
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "ECC_ID", constraint.ID)) 'AS/11-10-15==

                            affected = DBExecuteNonQuery(ProviderType, oCommand)
                        Catch ex As Exception
                            oCommand = Nothing
                            dbConnection.Close()
                            Return False
                        End Try

                        i += 1 'AS/2-10-16
                    End If 'AS/8-21-15==

                    If constraint.MinValueSet Then
                        Try
                            oCommand.CommandText = "INSERT INTO RAconstraints (SOrder, AID, value1, PSid, RAconstraint, CCID, AssociatedUDcolKey, AssociatedCVID, ECC_LinkedAttributeID, ECC_LinkedEnumID, ECC_ID) " +
                                                    "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)" 'AS/8-11-15  'AS/11-10-15 added Associated... and ECC_<...> fields
                            oCommand.Parameters.Clear()
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "SOrder", i))
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "AID", "Min"))
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "value1", constraint.MinValue))
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "PSid", scenario.ID))
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "RAconstraint", constraint.Name))
                            'oCommand.Parameters.Add(GetDBParameter(ProviderType, "CCID", dummyCCID)) 'AS/8-11-15 'AS/10-23-15 replaced "-1" with dummyCCID AS/11-18-15a
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "CCID", constraint.ID)) 'AS/8-11-15 'AS/10-23-15 replaced "-1" with dummyCCID AS/11-18-15a
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "AssociatedUDcolKey", constraint.ECD_AssociatedUDcolKey)) 'AS/11-10-15===
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "AssociatedCVID", constraint.ECD_AssociatedCVID))
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "ECC_LinkedAttributeID", constraint.LinkedAttributeID.ToString))
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "ECC_LinkedEnumID", constraint.LinkedEnumID.ToString))
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "ECC_ID", constraint.ID)) 'AS/11-10-15==

                            affected = DBExecuteNonQuery(ProviderType, oCommand)
                        Catch ex As Exception
                            oCommand = Nothing
                            dbConnection.Close()
                            Return False
                        End Try

                        i += 1
                    End If

                    If constraint.MaxValueSet Then
                        Try
                            oCommand.CommandText = "INSERT INTO RAconstraints (SOrder, AID, value1, PSid, RAconstraint, CCID, AssociatedUDcolKey, AssociatedCVID, ECC_LinkedAttributeID, ECC_LinkedEnumID, ECC_ID) " +
                                                    "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)" 'AS/8-11-15  'AS/11-10-15 added Associated... and ECC_<...> fields
                            oCommand.Parameters.Clear()
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "SOrder", i))
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "AID", "Max"))
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "value1", constraint.MaxValue))
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "PSid", scenario.ID))
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "RAconstraint", constraint.Name))
                            'oCommand.Parameters.Add(GetDBParameter(ProviderType, "CCID", dummyCCID)) 'AS/8-11-15 'AS/10-23-15 replaced "-1" with dummyCCID AS/11-18-15a
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "CCID", constraint.ID)) 'AS/8-11-15 'AS/10-23-15 replaced "-1" with dummyCCID AS/11-18-15a
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "AssociatedUDcolKey", constraint.ECD_AssociatedUDcolKey)) 'AS/11-10-15===
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "AssociatedCVID", constraint.ECD_AssociatedCVID))
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "ECC_LinkedAttributeID", constraint.LinkedAttributeID.ToString))
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "ECC_LinkedEnumID", constraint.LinkedEnumID.ToString))
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "ECC_ID", constraint.ID)) 'AS/11-10-15==

                            affected = DBExecuteNonQuery(ProviderType, oCommand)
                        Catch ex As Exception
                            oCommand = Nothing
                            dbConnection.Close()
                            Return False
                        End Try
                        i += 1
                    End If
                Next
            Next


            oCommand = Nothing
            If Not SavingToDatabaseInProgress() Then
                dbConnection.Close()
            End If
            Return True
        End Function

        Private Function SaveRABudgetLimits() As Boolean
            If (ProjectManager Is Nothing) OrElse Not CheckDBConnection(ProviderType, Location) Then
                Return False
            End If

            Dim dbConnection As DbConnection
            If SavingToDatabaseInProgress() Then
                dbConnection = mDBConnection
            Else
                dbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()
            End If

            Dim oCommand As DbCommand = GetDBCommand(ProviderType)
            Dim affected As Integer
            oCommand.Connection = dbConnection

            oCommand.CommandText = "DELETE FROM RABudgetLimits"
            affected = DBExecuteNonQuery(ProviderType, oCommand)

            For Each scenario As RAScenario In ProjectManager.ResourceAligner.Scenarios.Scenarios.Values
                oCommand.CommandText = "INSERT INTO RABudgetLimits (PSid, Limit) VALUES (?, ?)"
                oCommand.Parameters.Clear()
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "PSid", scenario.ID))
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "Limit", scenario.Budget))
                affected = DBExecuteNonQuery(ProviderType, oCommand)
            Next

            oCommand = Nothing
            If Not SavingToDatabaseInProgress() Then
                dbConnection.Close()
            End If
            Return True
        End Function

        Private Function SaveRAPortfolioScenarios() As Boolean 'AS/6-22-15
            If (ProjectManager Is Nothing) OrElse Not CheckDBConnection(ProviderType, Location) Then
                Return False
            End If

            Dim dbConnection As DbConnection
            If SavingToDatabaseInProgress() Then
                dbConnection = mDBConnection
            Else
                dbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()
            End If

            Dim oCommand As DbCommand = GetDBCommand(ProviderType)
            Dim affected As Integer
            oCommand.Connection = dbConnection

            oCommand.CommandText = "DELETE FROM RAPortfolioScenarios"
            affected = DBExecuteNonQuery(ProviderType, oCommand)

            For Each scenario As RAScenario In ProjectManager.ResourceAligner.Scenarios.Scenarios.Values
                Try
                    oCommand.CommandText = "INSERT INTO RAPortfolioScenarios (PSID, ScenarioName, Description) VALUES (?, ?, ?)"
                    oCommand.Parameters.Clear()
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "PSID", scenario.ID))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "ScenarioName", scenario.Name))
                    If scenario.Description Is Nothing Then scenario.Description = ""
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "Description", scenario.Description))
                    affected = DBExecuteNonQuery(ProviderType, oCommand)

                Catch ex As Exception
                    oCommand = Nothing
                    dbConnection.Close()
                    Return False
                End Try
            Next

            oCommand = Nothing
            If Not SavingToDatabaseInProgress() Then
                dbConnection.Close()
            End If
            Return True
        End Function

        Private Function SaveRAGroups() As Boolean
            If (ProjectManager Is Nothing) OrElse Not CheckDBConnection(ProviderType, Location) Then
                Return False
            End If

            Dim dbConnection As DbConnection
            If SavingToDatabaseInProgress() Then
                dbConnection = mDBConnection
            Else
                dbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()
            End If

            Dim oCommand As DbCommand = GetDBCommand(ProviderType)
            Dim affected As Integer

            oCommand.Connection = dbConnection

            oCommand.CommandText = "DELETE FROM Groups"
            affected = DBExecuteNonQuery(ProviderType, oCommand)

            oCommand.CommandText = "DELETE FROM GroupMembers"
            affected = DBExecuteNonQuery(ProviderType, oCommand)

            Dim i As Integer = 1
            Dim gID As Integer = 1

            For Each scenario As RAScenario In ProjectManager.ResourceAligner.Scenarios.Scenarios.Values
                For Each group As RAGroup In scenario.Groups.Groups.Values
                    Try
                        oCommand.CommandText = "INSERT INTO Groups (SOrder, GID, Groupname, [Type], PSid) " +
                                            "VALUES (?, ?, ?, ?, ?)"
                        oCommand.Parameters.Clear()
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "SOrder", gID))
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "GID", "G" + gID.ToString))
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "Groupname", group.Name))
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "Type", CInt(group.Condition) + 1))
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "PSid", scenario.ID))

                        affected = DBExecuteNonQuery(ProviderType, oCommand)
                    Catch ex As Exception
                        oCommand = Nothing
                        dbConnection.Close()
                        Return False
                    End Try

                    For Each raAlt As Canvas.RAAlternative In group.Alternatives.Values
                        Dim alt As clsNode = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(New Guid(raAlt.ID))
                        If alt IsNot Nothing Then
                            Dim aid As Integer = If(IsAltsZeroBased(ProjectManager), alt.NodeID + 1, alt.NodeID)
                            Try
                                oCommand.CommandText = "INSERT INTO GroupMembers (SOrder, GID, AID, PSid) " +
                                                    "VALUES (?, ?, ?, ?)"
                                oCommand.Parameters.Clear()
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "SOrder", i))
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "GID", "G" + gID.ToString))
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "AID", "A" + aid.ToString))
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "PSid", scenario.ID))

                                affected = DBExecuteNonQuery(ProviderType, oCommand)
                            Catch ex As Exception
                                oCommand = Nothing
                                dbConnection.Close()
                                Return False
                            End Try
                            i += 1
                        End If
                    Next

                    gID += 1
                Next
            Next

            oCommand = Nothing
            If Not SavingToDatabaseInProgress() Then
                dbConnection.Close()
            End If
            Return True
        End Function

        Private Function SaveRADependencies() As Boolean
            If (ProjectManager Is Nothing) OrElse Not CheckDBConnection(ProviderType, Location) Then
                Return False
            End If

            Dim dbConnection As DbConnection
            If SavingToDatabaseInProgress() Then
                dbConnection = mDBConnection
            Else
                dbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()
            End If

            Dim oCommand As DbCommand = GetDBCommand(ProviderType)
            Dim affected As Integer

            oCommand.Connection = dbConnection

            oCommand.CommandText = "DELETE FROM Dependencies"
            affected = DBExecuteNonQuery(ProviderType, oCommand)

            Dim aid1 As Integer
            Dim aid2 As Integer

            For Each scenario As RAScenario In ProjectManager.ResourceAligner.Scenarios.Scenarios.Values
                For Each dependency As Canvas.RADependency In scenario.Dependencies.Dependencies
                    Dim alt1 As clsNode = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(New Guid(dependency.FirstAlternativeID))
                    Dim alt2 As clsNode = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(New Guid(dependency.SecondAlternativeID))
                    If alt1 IsNot Nothing And alt2 IsNot Nothing Then
                        Try
                            aid1 = If(IsAltsZeroBased(ProjectManager), alt1.NodeID + 1, alt1.NodeID)
                            aid2 = If(IsAltsZeroBased(ProjectManager), alt2.NodeID + 1, alt2.NodeID)

                            Dim dType As String = ""
                            Select Case dependency.Value
                                Case Canvas.RADependencyType.dtDependsOn
                                    dType = "D"
                                Case Canvas.RADependencyType.dtMutuallyExclusive
                                    dType = "X"
                                Case Canvas.RADependencyType.dtMutuallyDependent
                                    dType = "M"
                            End Select

                            oCommand.CommandText = "INSERT INTO Dependencies (row, dependency, RAcolumn, PSid) " +
                                                "VALUES (?, ?, ?, ?)"
                            oCommand.Parameters.Clear()
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "row", aid1))
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "dependency", dType))
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "RAcolumn", aid2))
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "PSid", scenario.ID))

                            affected = DBExecuteNonQuery(ProviderType, oCommand)
                        Catch ex As Exception
                            oCommand = Nothing
                            dbConnection.Close()
                            Return False
                        End Try
                    End If
                Next
            Next

            oCommand = Nothing
            If Not SavingToDatabaseInProgress() Then
                dbConnection.Close()
            End If
            Return True
        End Function

        Private Function SaveRAMustsMustnots() As Boolean
            If (ProjectManager Is Nothing) OrElse Not CheckDBConnection(ProviderType, Location) Then
                Return False
            End If

            Dim dbConnection As DbConnection
            If SavingToDatabaseInProgress() Then
                dbConnection = mDBConnection
            Else
                dbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()
            End If

            Dim oCommand As DbCommand = GetDBCommand(ProviderType)
            Dim affected As Integer

            oCommand.Connection = dbConnection

            oCommand.CommandText = "DELETE FROM MustsMustNots"
            affected = DBExecuteNonQuery(ProviderType, oCommand)

            Dim aid As Integer

            For Each scenario As RAScenario In ProjectManager.ResourceAligner.Scenarios.Scenarios.Values
                For Each raAlt As Canvas.RAAlternative In scenario.AlternativesFull
                    If raAlt.Must Or raAlt.MustNot Or raAlt.IsPartial Then
                        Dim alt As clsNode = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(New Guid(raAlt.ID))
                        If alt IsNot Nothing Then
                            Try
                                aid = If(IsAltsZeroBased(ProjectManager), alt.NodeID + 1, alt.NodeID)

                                oCommand.CommandText = "INSERT INTO MustsMustNots (row, Must, MustNot, PartialMinPct, PSid, AID) " +
                                                    "VALUES (?, ?, ?, ?, ?, ?)"
                                oCommand.Parameters.Clear()
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "row", aid))
                                'oCommand.Parameters.Add(GetDBParameter(ProviderType, "Must", If(raAlt.Must, 1, DBNull.Value))) 'AS/8-18-15
                                'oCommand.Parameters.Add(GetDBParameter(ProviderType, "MustNot", If(raAlt.Must, 1, DBNull.Value))) 'AS/8-18-15
                                If raAlt.Must Then 'AS/8-18-15==
                                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "Must", 1))
                                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "MustNot", 1))
                                ElseIf raAlt.MustNot Then
                                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "Must", 0))
                                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "MustNot", 0))
                                Else
                                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "Must", DBNull.Value))
                                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "MustNot", DBNull.Value))
                                End If 'AS/8-18-15==

                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "PartialMinPct", If(raAlt.IsPartial, raAlt.MinPercent, "").ToString))
                                'oCommand.Parameters.Add(GetDBParameter(ProviderType, "PSid", 0)) 'AS/8-18-15b
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "PSid", scenario.ID)) 'AS/8-18-15b
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "AID", "A" + aid.ToString))

                                affected = DBExecuteNonQuery(ProviderType, oCommand)
                            Catch ex As Exception
                                oCommand = Nothing
                                dbConnection.Close()
                                Return False
                            End Try
                        End If
                    End If
                Next
            Next

            oCommand = Nothing
            If Not SavingToDatabaseInProgress() Then
                dbConnection.Close()
            End If
            Return True
        End Function

        Private Sub SaveRAProperty(ByVal oCommand As DbCommand, PSID As Integer, ByVal PropertyName As String, ByVal DefaultPropertyValue As String) 'C0825
            oCommand.CommandText = "DELETE FROM RAproperties WHERE PSID=? AND PropertyName=?"
            oCommand.Parameters.Clear()
            oCommand.Parameters.Add(GetDBParameter(ProviderType, "PSID", PSID))
            oCommand.Parameters.Add(GetDBParameter(ProviderType, "PropertyName", PropertyName))
            Dim affected As Integer = DBExecuteNonQuery(ProviderType, oCommand)


            oCommand.CommandText = "INSERT INTO RAproperties (PSID, PropertyName, PValue) " +
                                                "VALUES (?, ?, ?)"
            oCommand.Parameters.Clear()
            oCommand.Parameters.Add(GetDBParameter(ProviderType, "PSID", PSID))
            oCommand.Parameters.Add(GetDBParameter(ProviderType, "PropertyName", PropertyName))
            oCommand.Parameters.Add(GetDBParameter(ProviderType, "PValue", DefaultPropertyValue))
            affected = DBExecuteNonQuery(ProviderType, oCommand)
        End Sub

        Private Function SaveRASettings() As Boolean
            If (ProjectManager Is Nothing) OrElse Not CheckDBConnection(ProviderType, Location) Then
                Return False
            End If

            Dim dbConnection As DbConnection
            If SavingToDatabaseInProgress() Then
                dbConnection = mDBConnection
            Else
                dbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()
            End If

            Dim RA As Canvas.ResourceAligner = ProjectManager.ResourceAligner

            Dim oCommand As DbCommand = GetDBCommand(ProviderType)
            oCommand.Connection = dbConnection

            For Each scenario As RAScenario In ProjectManager.ResourceAligner.Scenarios.Scenarios.Values
                Try
                    'RA.Scenarios.ActiveScenario.Settings.
                    SaveRAProperty(oCommand, scenario.ID, "IgnoreConstraints", If(scenario.Settings.CustomConstraints, "0", "1"))
                    SaveRAProperty(oCommand, scenario.ID, "IgnoreDependencies", If(scenario.Settings.Dependencies, "0", "1"))
                    SaveRAProperty(oCommand, scenario.ID, "IgnoreFundingPools", If(scenario.Settings.FundingPools, "0", "1"))
                    SaveRAProperty(oCommand, scenario.ID, "IgnoreGroups", If(scenario.Settings.Groups, "0", "1"))
                    SaveRAProperty(oCommand, scenario.ID, "IgnoreMustNots", If(scenario.Settings.MustNots, "0", "1"))
                    SaveRAProperty(oCommand, scenario.ID, "IgnoreMusts", If(scenario.Settings.Musts, "0", "1"))
                    SaveRAProperty(oCommand, scenario.ID, "IgnoreRisks", If(scenario.Settings.Risks, "0", "1"))

                    SaveRAProperty(oCommand, scenario.ID, "BaseCaseShow", If(scenario.Settings.UseBaseCase, "1", "0"))
                    SaveRAProperty(oCommand, scenario.ID, "BCIncludeGroups", If(scenario.Settings.BaseCaseForGroups, "1", "0"))
                    SaveRAProperty(oCommand, scenario.ID, "BCIncludeFundingPools", If(scenario.Settings.BaseCaseForFundingPools, "1", "0"))
                    SaveRAProperty(oCommand, scenario.ID, "BCIncludeConstraints", If(scenario.Settings.BaseCaseForConstraints, "1", "0"))
                    SaveRAProperty(oCommand, scenario.ID, "BCIncludeDependencies", If(scenario.Settings.BaseCaseForDependencies, "1", "0"))
                    SaveRAProperty(oCommand, scenario.ID, "BCIncludeMustNots", If(scenario.Settings.BaseCaseForMustNots, "1", "0"))
                    SaveRAProperty(oCommand, scenario.ID, "BCIncludeMusts", If(scenario.Settings.BaseCaseForMusts, "1", "0"))
                    SaveRAProperty(oCommand, scenario.ID, "SortRAalternativesBy", RA.Scenarios.GlobalSettings.SortBy) 'AS/1-26-16
                Catch ex As Exception
                    oCommand = Nothing
                    dbConnection.Close()
                    Return False
                End Try
            Next

            oCommand = Nothing
            If Not SavingToDatabaseInProgress() Then
                dbConnection.Close()
            End If

            Return True
        End Function

        Private Function SaveRACosts() As Boolean
            If (ProjectManager Is Nothing) OrElse Not CheckDBConnection(ProviderType, Location) Then
                Return False
            End If

            Dim dbConnection As DbConnection
            If SavingToDatabaseInProgress() Then
                dbConnection = mDBConnection
            Else
                dbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()
            End If

            Dim RA As Canvas.ResourceAligner = ProjectManager.ResourceAligner

            Dim oCommand As DbCommand = GetDBCommand(ProviderType)
            oCommand.Connection = dbConnection

            oCommand.CommandText = "DELETE FROM RAbenefits"
            Dim affected As Integer = DBExecuteNonQuery(ProviderType, oCommand)

            For Each scenario As RAScenario In ProjectManager.ResourceAligner.Scenarios.Scenarios.Values
                Dim transaction As DbTransaction = dbConnection.BeginTransaction()
                oCommand.Transaction = transaction

                Try
                    For Each raAlt As Canvas.RAAlternative In scenario.AlternativesFull
                        Dim alt As clsNode = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(New Guid(raAlt.ID))
                        If alt IsNot Nothing Then
                            Dim aid As Integer = If(IsAltsZeroBased(ProjectManager), alt.NodeID + 1, alt.NodeID)

                            oCommand.CommandText = "INSERT INTO RAbenefits (PSID, AID, Benefit, Costs) " +
                                                "VALUES (?, ?, ?, ?)"
                            oCommand.Parameters.Clear()
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "PSID", scenario.ID))
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "AID", "A" + aid.ToString))
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "Benefit", raAlt.BenefitOriginal))
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "Costs", raAlt.Cost.ToString))
                            affected = DBExecuteNonQuery(ProviderType, oCommand)
                        End If
                    Next
                    transaction.Commit()
                Catch ex As Exception
                    transaction.Rollback()
                    oCommand = Nothing
                    dbConnection.Close()
                    Return False
                End Try
            Next

            oCommand = Nothing
            If Not SavingToDatabaseInProgress() Then
                dbConnection.Close()
            End If

            Return True
        End Function

        Private Function SaveRAFundingPools() As Boolean
            If (ProjectManager Is Nothing) OrElse Not CheckDBConnection(ProviderType, Location) Then
                Return False
            End If

            Dim dbConnection As DbConnection
            If SavingToDatabaseInProgress() Then
                dbConnection = mDBConnection
            Else
                dbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()
            End If

            Dim RA As Canvas.ResourceAligner = ProjectManager.ResourceAligner

            Dim oCommand As DbCommand = GetDBCommand(ProviderType)
            oCommand.Connection = dbConnection

            oCommand.CommandText = "DELETE FROM FundingPools"
            Dim affected As Integer = DBExecuteNonQuery(ProviderType, oCommand)

            Dim sOrder As Integer = 1

            For Each scenario As RAScenario In ProjectManager.ResourceAligner.Scenarios.Scenarios.Values
                Try
                    For Each FP As Canvas.RAFundingPool In scenario.FundingPools.Pools.Values
                        oCommand.CommandText = "INSERT INTO FundingPools (SOrder, PSID, Poolname, AID, Limit) " +
                                                "VALUES (?, ?, ?, ?, ?)"
                        oCommand.Parameters.Clear()
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "Sorder", sOrder))
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "PSID", scenario.ID))
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "Poolname", FP.Name))
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "AID", "POOL"))
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "Limit", FP.PoolLimit.ToString))
                        affected = DBExecuteNonQuery(ProviderType, oCommand)
                        sOrder += 1
                    Next
                    For Each FP As Canvas.RAFundingPool In scenario.FundingPools.Pools.Values
                        For Each kvp As KeyValuePair(Of String, Double) In FP.Values
                            Dim alt As clsNode = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(New Guid(kvp.Key))
                            If alt IsNot Nothing Then
                                Dim aid As Integer = If(IsAltsZeroBased(ProjectManager), alt.NodeID + 1, alt.NodeID)

                                oCommand.CommandText = "INSERT INTO FundingPools (SOrder, PSID, Poolname, AID, Limit) " +
                                                        "VALUES (?, ?, ?, ?, ?)"
                                oCommand.Parameters.Clear()
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "Sorder", sOrder))
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "PSID", scenario.ID))
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "Poolname", FP.Name))
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "AID", "A" + aid.ToString))
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "Limit", kvp.Value.ToString))
                                affected = DBExecuteNonQuery(ProviderType, oCommand)
                                sOrder += 1
                            End If
                        Next
                    Next
                Catch ex As Exception
                    oCommand = Nothing
                    dbConnection.Close()
                    Return False
                End Try
            Next

            oCommand = Nothing
            If Not SavingToDatabaseInProgress() Then
                dbConnection.Close()
            End If

            Return True
        End Function

        Private Function SaveStepFunctions() As Boolean 'C0156
            If ProjectManager Is Nothing Then Return False 'C0812
            If Not SavingToDatabaseInProgress() AndAlso Not CheckDBConnection(ProviderType, Location) Then Return False 'C0812

            'C0812===
            'If (ProjectManager Is Nothing) orelse Not CheckDBConnection(ProviderType, Location) Then
            '    Return False
            'End If
            'C0812==

            'Dim dbConnection As New odbc.odbcConnection(Location) 'C0235

            'C0812===
            'Dim dbConnection As DbConnection = GetDBConnection(ProviderType, Location) 'C0235
            'dbConnection.Open()
            'C0812==

            'C0812===
            Dim dbConnection As DbConnection
            If SavingToDatabaseInProgress() Then
                dbConnection = mDBConnection
            Else
                dbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()
            End If
            'C0812==

            'Dim oCommand As New odbc.odbcCommand 'C0235
            Dim oCommand As DbCommand = GetDBCommand(ProviderType) 'C0235
            Dim affected As Integer

            oCommand.Connection = dbConnection

            'C0921===
            Dim Transaction As DbTransaction = dbConnection.BeginTransaction
            oCommand.Transaction = Transaction
            'C0921==

            ' =============================
            ' Fill Intensities and llCritIntensities tables

            Dim nid As Integer

            'Dim i As Integer = 1 'C0156

            Dim base As Integer

            Dim tmpStepFunction As clsStepFunction
            Dim tmpIntensity As clsStepInterval

            For Each SF As clsStepFunction In ProjectManager.MeasureScales.StepFunctions
                Dim InUse As Boolean = False
                For Each node As clsNode In ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).Nodes
                    If (node.MeasureType = ECMeasureType.mtStep) And (node.StepFunctionID = SF.ID) Then
                        InUse = True
                    End If
                Next

                If InUse Then
                    'SF.Sort() 'C0158
                    SF.SortByInterval() 'C0158
                    base = mIntensityCounter

                    tmpStepFunction = New clsStepFunction(-2) 'C0012

                    For Each interval As clsStepInterval In SF.Intervals
                        If (interval.Low <> NEGATIVE_INFINITY) Then 'C0158
                            'C0012===
                            tmpIntensity = tmpStepFunction.AddInterval
                            tmpIntensity.ID = mIntensityCounter
                            tmpIntensity.Name = interval.Name
                            tmpIntensity.Value = interval.Value
                            'C0012==

                            Try
                                oCommand.CommandText = "INSERT INTO Intensities (IID, IntensityName, Comment) " +
                                                        "VALUES (?, ?, ?)"
                                oCommand.Parameters.Clear()
                                'C0235===
                                'oCommand.Parameters.AddWithValue("IID", "I" + mIntensityCounter.ToString)
                                'oCommand.Parameters.AddWithValue("IntensityName", interval.Name)
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "IID", "I" + mIntensityCounter.ToString)) 'C0237
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "IntensityName", interval.Name)) 'C0237
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "Comment", interval.Comment))
                                'C0235==
                                affected = DBExecuteNonQuery(ProviderType, oCommand)
                            Catch ex As Exception
                                Transaction.Rollback() 'C0921
                                oCommand = Nothing
                                If Not SavingToDatabaseInProgress() Then 'C0812
                                    dbConnection.Close()
                                End If

                                Return False
                            End Try
                            mIntensityCounter += 1
                        End If
                    Next

                    For Each node As clsNode In ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).Nodes
                        If (node.MeasureType = ECMeasureType.mtStep) And (node.StepFunctionID = SF.ID) Then
                            node.Tag = tmpStepFunction

                            nid = If(IsNodesZeroBased(ProjectManager), node.NodeID + 1, node.NodeID)
                            For j As Integer = 0 To SF.Intervals.Count - 1
                                If CType(SF.Intervals(j), clsStepInterval).Low <> NEGATIVE_INFINITY Then 'C0158
                                    'For j As Integer = 1 To SF.Intervals.Count - 1
                                    Try
                                        oCommand.CommandText = "INSERT INTO llCritIntensities (WRT, IID, StepGE, Lpriority, Dormant) " +
                                                                "VALUES (?, ?, ?, ?, ?)"
                                        oCommand.Parameters.Clear()
                                        'C0235===
                                        'oCommand.Parameters.AddWithValue("WRT", "N" + nid.ToString)
                                        'oCommand.Parameters.AddWithValue("IID", "I" + (base + j).ToString)
                                        'oCommand.Parameters.AddWithValue("StepGE", CType(SF.Intervals(j), clsStepInterval).Low)
                                        'oCommand.Parameters.AddWithValue("Lpriority", CType(SF.Intervals(j), clsStepInterval).Value)
                                        'oCommand.Parameters.AddWithValue("Dormant", False)
                                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "WRT", "N" + nid.ToString)) 'C0237
                                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "IID", "I" + (base + j).ToString)) 'C0237
                                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "StepGE", CType(SF.Intervals(j), clsStepInterval).Low)) 'C0237
                                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "LPriority", CType(SF.Intervals(j), clsStepInterval).Value)) 'C0237
                                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "Dormant", False)) 'C0237
                                        'C0235==
                                        affected = DBExecuteNonQuery(ProviderType, oCommand)
                                    Catch ex As Exception
                                        Transaction.Rollback() 'C0921
                                        oCommand = Nothing
                                        If Not SavingToDatabaseInProgress() Then 'C0812
                                            dbConnection.Close()
                                        End If
                                        Return False
                                    End Try
                                End If
                            Next
                        End If
                    Next
                End If
            Next

            Transaction.Commit() 'C0921

            oCommand = Nothing
            If Not SavingToDatabaseInProgress() Then 'C0812
                dbConnection.Close()
            End If
            Return True
        End Function

        Private Function SaveRatingScales() As Boolean
            If ProjectManager Is Nothing Then Return False 'C0812
            If Not SavingToDatabaseInProgress() AndAlso Not CheckDBConnection(ProviderType, Location) Then Return False 'C0812

            'C0812===
            'If (ProjectManager Is Nothing) orelse Not CheckDBConnection(ProviderType, Location) Then
            '    Return False
            'End If
            'C0812==

            'Dim dbConnection As New odbc.odbcConnection(Location) 'C0235
            'C0812===
            'Dim dbConnection As DbConnection = GetDBConnection(ProviderType, Location) 'C0235
            'dbConnection.Open()
            'C0812==

            'C0812===
            Dim dbConnection As DbConnection
            If SavingToDatabaseInProgress() Then
                dbConnection = mDBConnection
            Else
                dbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()
            End If
            'C0812==

            'Dim oCommand As New odbc.odbcCommand 'C0235
            Dim oCommand As DbCommand = GetDBCommand(ProviderType) 'C0235
            Dim affected As Integer

            oCommand.Connection = dbConnection

            'C0921===
            Dim Transaction As DbTransaction = dbConnection.BeginTransaction
            oCommand.Transaction = Transaction
            'C0921==

            ' =============================
            ' Fill Intensities and llCritIntensities tables

            ' Delete everything from Intensities and llCritIntensities table
            oCommand.CommandText = "DELETE FROM Intensities"
            affected = DBExecuteNonQuery(ProviderType, oCommand)

            oCommand.CommandText = "DELETE FROM llCritIntensities"
            affected = DBExecuteNonQuery(ProviderType, oCommand)

            Dim nid As Integer

            'Dim i As Integer = 1 'C0156

            Dim base As Integer

            Dim tmpRatingScale As clsRatingScale 'C0012
            Dim tmpIntensity As clsRating 'C0012

            For Each RS As clsRatingScale In ProjectManager.MeasureScales.RatingsScales
                RS.Sort()
                base = mIntensityCounter

                tmpRatingScale = New clsRatingScale(-2) 'C0012

                For Each rating As clsRating In RS.RatingSet
                    'C0012===
                    tmpIntensity = tmpRatingScale.AddIntensity()
                    tmpIntensity.ID = mIntensityCounter
                    tmpIntensity.Name = rating.Name
                    tmpIntensity.Value = rating.Value
                    'C0012==

                    Try
                        oCommand.CommandText = "INSERT INTO Intensities (IID, IntensityName, Comment) " +
                                            "VALUES (?, ?, ?)"
                        oCommand.Parameters.Clear()
                        'C0235===
                        'oCommand.Parameters.AddWithValue("IID", "I" + mIntensityCounter.ToString)
                        'oCommand.Parameters.AddWithValue("IntensityName", rating.Name)
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "IID", "I" + mIntensityCounter.ToString)) 'C0237
                        'oCommand.Parameters.Add(GetDBParameter(ProviderType, "IntensityName", rating.Name)) 'C0237 'C0931
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "IntensityName", rating.Name.Substring(0, If(rating.Name.Length > 100, 100, rating.Name.Length)))) 'C0931
                        'C0235==
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "Comment", rating.Comment.Substring(0, If(rating.Comment.Length > 255, 255, rating.Comment.Length))))

                        affected = DBExecuteNonQuery(ProviderType, oCommand)
                    Catch ex As Exception
                        Transaction.Rollback() 'C0921
                        oCommand = Nothing
                        If Not SavingToDatabaseInProgress() Then 'C0812
                            dbConnection.Close()
                        End If
                        Return False
                    End Try
                    mIntensityCounter += 1
                Next

                Transaction.Commit() 'C0921
                Transaction = dbConnection.BeginTransaction 'C0921
                oCommand.Transaction = Transaction

                For Each node As clsNode In ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).Nodes
                    If (node.MeasureType = ECMeasureType.mtRatings) And (node.RatingScaleID = RS.ID) Then
                        node.Tag = tmpRatingScale 'C0012

                        nid = If(IsNodesZeroBased(ProjectManager), node.NodeID + 1, node.NodeID)
                        For j As Integer = 0 To RS.RatingSet.Count - 1
                            Try
                                oCommand.CommandText = "INSERT INTO llCritIntensities (WRT, IID, StepGE, Lpriority, Dormant) " +
                                                    "VALUES (?, ?, ?, ?, ?)"
                                oCommand.Parameters.Clear()
                                'C0235===
                                'oCommand.Parameters.AddWithValue("WRT", "N" + nid.ToString)
                                'oCommand.Parameters.AddWithValue("IID", "I" + (base + j).ToString)
                                'oCommand.Parameters.AddWithValue("StepGE", 0)
                                'oCommand.Parameters.AddWithValue("Lpriority", CType(RS.RatingSet(j), clsRating).Value)
                                'oCommand.Parameters.AddWithValue("Dormant", False)
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "WRT", "N" + nid.ToString)) 'C0237
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "IID", "I" + (base + j).ToString)) 'C0237
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "StepGE", 0)) 'C0237
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "LPriority", CType(RS.RatingSet(j), clsRating).Value)) 'C0237
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "Dormant", False)) 'C0237
                                'C0235==
                                affected = DBExecuteNonQuery(ProviderType, oCommand)
                            Catch ex As Exception
                                Transaction.Rollback() 'C0921
                                oCommand = Nothing
                                If Not SavingToDatabaseInProgress() Then 'C0812
                                    dbConnection.Close()
                                End If
                                Return False
                            End Try
                        Next
                    End If
                Next
            Next

            Transaction.Commit() 'C0921

            oCommand = Nothing
            If Not SavingToDatabaseInProgress() Then 'C0812
                dbConnection.Close()
            End If
            Return True
        End Function

        Private Function SavePWAltsValues(ByVal AUserID As Integer, ByVal node As ECCore.clsNode) As Boolean
            If (ProjectManager Is Nothing) OrElse Not CheckDBConnection(ProviderType, Location) Or (node Is Nothing) Then
                Return False
            End If

            If Not ProjectManager.UserExists(AUserID) Then
                Return False
            End If

            'Dim dbConnection As New odbc.odbcConnection(Location) 'C0235
            Dim dbConnection As DbConnection = GetDBConnection(ProviderType, Location) 'C0235
            dbConnection.Open()

            'Dim oCommand As New odbc.odbcCommand 'C0235
            Dim oCommand As DbCommand = GetDBCommand(ProviderType) 'C0235

            If (node.MeasureType <> ECMeasureType.mtPairwise) Then
                Return False
            End If

            'Dim nodesBelow As ArrayList = node.GetNodesBelow.Clone 'C0385
            'Dim nodesBelow As List(Of clsNode) = node.GetNodesBelow 'C0385 'C0450
            Dim nodesBelow As List(Of clsNode) = node.GetNodesBelow(AUserID) 'C0450

            If nodesBelow.Count = 0 Then
                Return False
            End If

            oCommand.Connection = dbConnection

            Dim affected As Integer

            Dim UserID As Integer '= If(AUserID = COMBINED_USER_ID, 1, AUserID) ' in case = -1 write to combine

            'If AUserID = COMBINED_USER_ID Then 'C0555
            If IsCombinedUserID(AUserID) Then 'C0555
                UserID = 1
            Else
                UserID = GetAHPUserID(ProjectManager, ProjectManager.GetUserByID(AUserID)) 'C0558
            End If

            Dim nid As Integer
            Dim aid As Integer

            For i As Integer = 0 To nodesBelow.Count - 1

                nid = If(IsNodesZeroBased(ProjectManager), node.NodeID + 1, node.NodeID)
                aid = If(IsAltsZeroBased(ProjectManager), CType(nodesBelow(i), clsNode).NodeID + 1, CType(nodesBelow(i), clsNode).NodeID)

                'oCommand.CommandText = GetUpdateAltsValuesSQLString(UserID.ToString, "N" + nid.ToString, "A" + aid.ToString, node.Judgments.Weights(i).ToString(Culture))
                oCommand.CommandText = "UPDATE AltsValues SET ALTVALUE=? WHERE PID=? AND WRT=? AND AID=?"
                oCommand.Parameters.Clear()
                'oCommand.Parameters.AddWithValue("ALTVALUE", node.Judgments.Weights(i)) 'C0128
                'oCommand.Parameters.AddWithValue("ALTVALUE", node.Judgments.Weights(CType(nodesBelow(i), clsNode).NodeID)) 'C0128 'C0159

                'oCommand.Parameters.AddWithValue("ALTVALUE", node.Judgments.Weights.GetUserWeights(AUserID).GetWeightValueByNodeID(CType(nodesBelow(i), clsNode).NodeID)) 'C0159 'C0177

                'C0235===
                'oCommand.Parameters.AddWithValue("ALTVALUE", node.Judgments.Weights.GetUserWeights(AUserID, ProjectManager.CalculationsManager.SynthesisMode).GetWeightValueByNodeID(CType(nodesBelow(i), clsNode).NodeID)) 'C0177
                'oCommand.Parameters.AddWithValue("PID", UserID)
                'oCommand.Parameters.AddWithValue("WRT", "N" + nid.ToString)
                'oCommand.Parameters.AddWithValue("AID", "A" + aid.ToString)
                'oCommand.Parameters.Add(GetDBParameter(ProviderType, "ALTVALUE", node.Judgments.Weights.GetUserWeights(AUserID, ProjectManager.CalculationsManager.SynthesisMode).GetWeightValueByNodeID(CType(nodesBelow(i), clsNode).NodeID))) 'C0177 'C0237 'C0338
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "ALTVALUE", node.Judgments.Weights.GetUserWeights(AUserID, ProjectManager.CalculationsManager.SynthesisMode, ProjectManager.CalculationsManager.IncludeIdealAlternative).GetWeightValueByNodeID(CType(nodesBelow(i), clsNode).NodeID))) 'C0338
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "PID", UserID)) 'C0237
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "WRT", "N" + nid.ToString)) 'C0237
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "AID", "A" + aid.ToString)) 'C0237
                'C0235==

                affected = DBExecuteNonQuery(ProviderType, oCommand)

                If affected = 0 Then
                    'oCommand.CommandText = GetInsertAltsValuesSQLString(UserID.ToString, "N" + nid.ToString, "A" + aid.ToString, node.Judgments.Weights(i).ToString(Culture))
                    oCommand.CommandText = "INSERT INTO AltsValues (PID, WRT, AID, ALTVALUE) " +
                    "VALUES (?, ?, ?, ?)"
                    oCommand.Parameters.Clear()
                    'C0235===
                    'oCommand.Parameters.AddWithValue("PID", UserID)
                    'oCommand.Parameters.AddWithValue("WRT", "N" + nid.ToString)
                    'oCommand.Parameters.AddWithValue("AID", "A" + aid.ToString)
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "PID", UserID)) 'C0237
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "WRT", "N" + nid.ToString)) 'C0237
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "AID", "A" + aid.ToString)) 'C0237
                    'C0235==
                    'oCommand.Parameters.AddWithValue("ALTVALUE", node.Judgments.Weights(i)) 'C0128
                    'oCommand.Parameters.AddWithValue("ALTVALUE", node.Judgments.Weights(CType(nodesBelow(i), clsNode).NodeID)) 'C0128 'C0159

                    'oCommand.Parameters.AddWithValue("ALTVALUE", node.Judgments.Weights.GetUserWeights(AUserID).GetWeightValueByNodeID(CType(nodesBelow(i), clsNode).NodeID)) 'C0159 'C0177
                    'oCommand.Parameters.AddWithValue("ALTVALUE", node.Judgments.Weights.GetUserWeights(AUserID, ProjectManager.CalculationsManager.SynthesisMode).GetWeightValueByNodeID(CType(nodesBelow(i), clsNode).NodeID)) 'C0177 'C0235
                    'oCommand.Parameters.Add(GetDBParameter(ProviderType, "ALTVALUE", node.Judgments.Weights.GetUserWeights(AUserID, ProjectManager.CalculationsManager.SynthesisMode).GetWeightValueByNodeID(CType(nodesBelow(i), clsNode).NodeID))) 'C0235 'C0237 'C0338
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "ALTVALUE", node.Judgments.Weights.GetUserWeights(AUserID, ProjectManager.CalculationsManager.SynthesisMode, ProjectManager.CalculationsManager.IncludeIdealAlternative).GetWeightValueByNodeID(CType(nodesBelow(i), clsNode).NodeID))) 'C0338

                    affected = DBExecuteNonQuery(ProviderType, oCommand)
                End If
            Next

            oCommand = Nothing
            dbConnection.Close()
            Return True
        End Function

        Private Function SavePWData(ByVal node As ECCore.clsNode, ByVal PWData As ECCore.clsPairwiseMeasureData) As Boolean
            If (ProjectManager Is Nothing) Or (node Is Nothing) Or (PWData Is Nothing) Then Return False 'C0812
            If PWData.IsUndefined Then Return True

            If WriteOnlyAllowedJudgmentsToAHP Then
                Dim nodesBelow As List(Of clsNode) = node.GetNodesBelow(PWData.UserID)
                Dim isAllowed1 As Boolean = False
                Dim isAllowed2 As Boolean = False
                For Each child As clsNode In nodesBelow
                    If child.NodeID = PWData.FirstNodeID Then
                        isAllowed1 = True
                    End If
                    If child.NodeID = PWData.SecondNodeID Then
                        isAllowed2 = True
                    End If
                Next
                If Not isAllowed1 Or Not isAllowed2 Then
                    Return True
                End If
            End If

            If Not SavingToDatabaseInProgress() AndAlso Not CheckDBConnection(ProviderType, Location) Then Return False 'C0812

            Dim dbConnection As DbConnection
            If SavingToDatabaseInProgress() Then
                dbConnection = mDBConnection
            Else
                dbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()
            End If

            Dim oCommand As DbCommand
            If mDBCommand Is Nothing Then
                oCommand = GetDBCommand(ProviderType)
            Else
                oCommand = mDBCommand
            End If
            'C0921==

            'Dim node As clsNode = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).GetNodeByID(pd.ParentNodeID)

            ' synchronize userid

            Dim userID As Integer

            If IsCombinedUserID(PWData.UserID) Then 'C0555
                userID = 1
            Else
                userID = GetAHPUserID(ProjectManager, ProjectManager.GetUserByID(PWData.UserID)) 'C0558
            End If

            Dim parentNID As Integer
            Dim firstNID As Integer
            Dim secondNID As Integer

            parentNID = If(IsNodesZeroBased(ProjectManager), PWData.ParentNodeID + 1, PWData.ParentNodeID)
            If node.IsTerminalNode Then
                firstNID = If(IsAltsZeroBased(ProjectManager), PWData.FirstNodeID + 1, PWData.FirstNodeID)
                secondNID = If(IsAltsZeroBased(ProjectManager), PWData.SecondNodeID + 1, PWData.SecondNodeID)
            Else
                firstNID = If(IsNodesZeroBased(ProjectManager), PWData.FirstNodeID + 1, PWData.FirstNodeID)
                secondNID = If(IsNodesZeroBased(ProjectManager), PWData.SecondNodeID + 1, PWData.SecondNodeID)
            End If

            Dim N1 As String = If(node.IsTerminalNode, "A" + firstNID.ToString, "N" + firstNID.ToString)
            Dim N2 As String = If(node.IsTerminalNode, "A" + secondNID.ToString, "N" + secondNID.ToString)

            Dim value As Single
            value = If(PWData.Advantage = 1, PWData.Value, 1 / PWData.Value)

            If N1 > N2 Then
                Dim tmpStr As String = N1
                N1 = N2
                N2 = tmpStr

                value = 1 / value
            End If

            Dim affected As Integer

            oCommand.Connection = dbConnection

            If PWData.IsUndefined Then
                Try
                    oCommand.CommandText = "DELETE FROM Judgments WHERE PID=? AND WRT=? AND N1=? AND N2=?"
                    oCommand.Parameters.Clear()
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "PID", userID)) 'C0237
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "WRT", "N" + parentNID.ToString)) 'C0237
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "N1", N1)) 'C0237
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "N2", N2)) 'C0237
                    'C0235==
                    affected = DBExecuteNonQuery(ProviderType, oCommand)
                Catch ex As Exception
                    oCommand = Nothing
                    If Not SavingToDatabaseInProgress() Then 'C0812
                        dbConnection.Close()
                    End If

                    'Debug.Print("Exception when saving pairwise judgment to ahp file (PID=" + userID.ToString + ", WRT=N" + parentNID.ToString + ", N1=" + N1 + ", N2=" + N2)
                    Return False
                End Try

                oCommand = Nothing 'C0059
                If Not SavingToDatabaseInProgress() Then 'C0812
                    dbConnection.Close() 'C0059
                End If
                Return True
            End If

            ' IF NOTHING UPDATED THEN INSERT NEW RECORDS TO THE TABLE
            If affected = 0 Then
                Try
                    oCommand.CommandText = "INSERT INTO Judgments (PID, WRT, N1, N2, J, [Note]) " +
                        "VALUES (?, ?, ?, ?, ?, ?)"
                    oCommand.Parameters.Clear()
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "PID", userID)) 'C0237
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "WRT", "N" + parentNID.ToString)) 'C0237
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "N1", N1)) 'C0237
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "N2", N2)) 'C0237
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "J", value)) 'C0237

                    If ProviderType = DBProviderType.dbptODBC Then
                        Dim tNote As Odbc.OdbcParameter = oCommand.CreateParameter
                        tNote.OdbcType = Odbc.OdbcType.Text
                        tNote.ParameterName = "Note"
                        tNote.Value = PWData.Comment
                        'oCommand.Parameters.Add(GetDBParameter(ProviderType, tNote)) 'C0237
                        oCommand.Parameters.Add(tNote) 'C0237
                    Else
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "Note", PWData.Comment)) 'C0237
                    End If
                    'C0235==

                    affected = DBExecuteNonQuery(ProviderType, oCommand)
                Catch ex1 As Exception
                    oCommand = Nothing

                    'Debug.Print(" Save PWData error: " + ex1.Message)
                    If dbConnection Is Nothing Then
                        'Debug.Print(" dbConnection is Nothing")
                    Else
                        'Debug.Print(" dbConnectionState: " + dbConnection.State.ToString)
                    End If
                    If Not SavingToDatabaseInProgress() Then 'C0812
                        dbConnection.Close()
                    End If

                    'Debug.Print("Exception when saving pairwise judgment to ahp file (PID=" + userID.ToString + ", WRT=N" + parentNID.ToString + ", N1=" + N1 + ", N2=" + N2)

                    Return False
                End Try
                'Debug.Print("Saved pairwise judgment to ahp file (PID=" + userID.ToString + ", WRT=N" + parentNID.ToString + ", N1=" + N1 + ", N2=" + N2)
            End If

            If mDBCommand Is Nothing Then 'C0926
                oCommand = Nothing 'C0056
            End If

            If Not SavingToDatabaseInProgress() Then 'C0812
                dbConnection.Close()
            End If
            Return True
        End Function

        Public Overrides Function SaveProject(Optional ByVal StructureOnly As Boolean = False) As Boolean
            If (ProjectManager Is Nothing) OrElse Not CheckDBConnection(ProviderType, Location) Then Return False

            'C0772===
            ' Progress:
            ' Step 1: SaveAlternatives()
            ' Step 2: SaveAltsContribution()
            ' Step 3: SaveInfoDocs()
            ' Step 4: SaveMeasurementTypes()
            ' Step 5: SaveRatingScales()
            ' Step 6: SaveStepFunctions()
            ' Step 7: SaveMProperties()
            ' Step 8: SaveNodes()
            ' Step 9: SaveUsersInfo()
            ' Step 10 (usersCount): SaveUserPermissions
            ' Step 11: SaveAdvancedUtilityCurves() 
            ' Step 12 (nodesCount): Judgments per node: count = CType(ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy), clsHierarchy).Nodes.Count
            ' Step 13: SaveAltsCosts()
            ' Step 14: SaveExtraAHPTables()
            ' Total Steps = 12 + usersCount + nodesCount

            'Debug.Print("Start saving as AHP: " + Now.ToString)

            'Dim dbConnection As New odbc.odbcConnection(Location) 'C0235

            Dim dbConnection As DbConnection = GetDBConnection(ProviderType, Location) 'C0235
            dbConnection.Open()


            mDBConnection = dbConnection 'C0812

            'Dim oCommand As New odbc.odbcCommand 'C0235
            Dim oCommand As DbCommand = GetDBCommand(ProviderType) 'C0235
            oCommand.Connection = dbConnection

            'Dim transaction As odbc.odbcTransaction 'C0235
            'Dim transaction As DbTransaction 'C0235

            Dim affected As Integer

            '==========================

            'TODO: Fill AltsGlobal ------------------- easy - done - SaveAlternativesToDB
            'TODO: Fill AltsActive ------------------- easy - done - SaveAlternativesToDB
            'TODO: Fill AltsContributeTo ------------- easy - done - SaveAltsContributionToDB
            'TODO: Fill Documents -------------------- easy - done - SaveDocumentsToDB
            'TODO: Fill Intensities ------------------ hard - done - SaveRatingScalesToDB
            'TODO: Fill llCritFormulas --------------- hard - done - SaveMeasurementTypesToDB
            'TODO: Fill llCritIntensities ------------ hard - done - SaveRatingScalesToDB
            'TODO: Fill MProperties ------------------ hard
            'TODO: Fill NodeDefs --------------------- hard - done - SaveNodesToDB
            'TODO: Fill People ----------------------- easy - done - SaveUsersToDB
            'TODO: Fill PeopleClusterAccessDenied ---- hard - ? (I think it is not used at alls)
            'TODO: Fill RoleEvaluateJD --------------- hard - done - SaveRolesToDB
            'TODO: Fill RoleEvaluateNodes ------------ hard - done - SaveRolesToDB


            SaveAlternatives()
            SaveAltsContribution()
            SaveInfoDocs()
            SaveMeasurementTypes()

            mIntensityCounter = 1 'C0156
            SaveRatingScales()
            SaveStepFunctions() 'C0156
            SaveExtraAHPTables() 'C0342

            SaveNodes()
            SaveUsersInfo()
            SaveGroups() 'ASGroups

            If Not StructureOnly Then
                SaveUserPermissions()
            End If

            'SaveAdvancedUtilityCurves() 'C0772

            '==========================


            'Dim userID As Integer
            Dim oldUserID As Integer
            oldUserID = ProjectManager.UserID

            Dim TempList As List(Of clsCustomMeasureData) = Nothing

            'Debug.Print("Start save judgments: " + Now)


            Dim Transaction As DbTransaction 'C0921

            If Not StructureOnly Then 'C0772
                ' WRITE PAIRWISE DATA TO JUDGMENTS TABLE
                For Each node As clsNode In CType(ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy), clsHierarchy).Nodes
                    'C0921===
                    Transaction = dbConnection.BeginTransaction
                    oCommand.Transaction = Transaction
                    mDBCommand = oCommand
                    'C0921==

                    TempList = node.Judgments.JudgmentsFromAllUsers
                    Select Case node.MeasureType
                        Case ECMeasureType.mtPairwise
                            For Each pd As clsPairwiseMeasureData In TempList
                                'SavePairwiseJudgmentToDBUsingCulture(dbConnection, pd, curculture) 'C0008
                                SavePWData(node, pd) 'C0008
                            Next
                        Case Else
                            For Each nonPWData As clsNonPairwiseMeasureData In TempList
                                SaveNonPairwiseData(node, nonPWData, True)
                            Next
                            'C0156==
                    End Select

                    Transaction.Commit() 'C0921
                    mDBCommand = Nothing
                Next
                'transaction.Commit()
            End If
            '==========================

            SaveAltsCosts() 'C0174

            'SaveAltsRisks()

            'C0921===
            Transaction = dbConnection.BeginTransaction
            oCommand.Transaction = Transaction
            'C0921==

            'C0784===
            ' saving direct input judgments for non-covering objectives
            For Each node As clsNode In ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).Nodes
                If Not node.IsTerminalNode And node.MeasureType = ECMeasureType.mtDirect Then
                    For Each nonpwData As clsDirectMeasureData In node.Judgments.JudgmentsFromAllUsers
                        oCommand.CommandText = "INSERT INTO NonPairwiseData (UserID, HierarchyID, ParentNodeID, NodeID, ScaleItemID, Data, Comment, ModifyTime, UsersNumber) " +
                                        "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)"

                        oCommand.Parameters.Clear()
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "UserID", nonpwData.UserID))
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "HierarchyID", ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).HierarchyID))
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "ParentNodeID", If(IsNodesZeroBased(ProjectManager), nonpwData.ParentNodeID + 1, nonpwData.ParentNodeID)))
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "NodeID", If(IsNodesZeroBased(ProjectManager), nonpwData.NodeID + 1, nonpwData.NodeID)))
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "ScaleItemID", -1))
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "Data", CSng(nonpwData.ObjectValue)))

                        If ProviderType = DBProviderType.dbptODBC Then
                            Dim tComment As Odbc.OdbcParameter = oCommand.CreateParameter
                            tComment.OdbcType = Odbc.OdbcType.Text
                            tComment.ParameterName = "Comment"
                            tComment.Value = nonpwData.Comment
                            oCommand.Parameters.Add(tComment)
                        Else
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "Comment", nonpwData.Comment))
                        End If

                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "ModifyTime", nonpwData.ModifyDate.ToString))
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "UsersNumber", -1))

                        affected = DBExecuteNonQuery(ProviderType, oCommand)
                    Next
                End If
            Next

            Transaction.Commit() 'C0921

            'C0785===
            Transaction = dbConnection.BeginTransaction 'C0921
            oCommand.Transaction = Transaction 'C0921

            mDBCommand = oCommand 'C0926

            ' Saving ratios to Judgments table
            mHasOnlyOneDirectJudgmentForNonCovObj = False
            For Each node As clsNode In ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).Nodes
                If Not node.IsTerminalNode And node.MeasureType = ECMeasureType.mtDirect Then
                    For Each user As clsUser In ProjectManager.UsersList
                        Dim children As List(Of clsNode) = node.GetNodesBelow(user.UserID)
                        If children.Count > 1 Then
                            For i As Integer = 0 To children.Count - 2
                                Dim node1 As clsNode = children(i)
                                Dim node2 As clsNode = children(i + 1)

                                If CType(node.Judgments, clsNonPairwiseJudgments).JudgmentsFromUser(user.UserID).Count = 1 Then
                                    mHasOnlyOneDirectJudgmentForNonCovObj = True
                                End If

                                Dim J1 As clsDirectMeasureData = CType(CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(node1.NodeID, node.NodeID, user.UserID), clsDirectMeasureData)
                                Dim J2 As clsDirectMeasureData = CType(CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(node2.NodeID, node.NodeID, user.UserID), clsDirectMeasureData)

                                If J1 IsNot Nothing And J2 IsNot Nothing Then
                                    If Not J1.IsUndefined And Not J2.IsUndefined Then
                                        If J1.SingleValue <> 0 And J2.SingleValue <> 0 Then
                                            Dim ratio As Single = J1.SingleValue / J2.SingleValue
                                            Dim pwData As New clsPairwiseMeasureData(node1.NodeID, node2.NodeID, If(ratio >= 1, 1, -1), If(ratio >= 1, ratio, 1 / ratio), node.NodeID, user.UserID)
                                            SavePWData(node, pwData)
                                        End If
                                    End If
                                End If
                            Next
                        End If
                    Next
                End If
            Next

            Transaction.Commit() 'C0921

            mDBCommand = Nothing

            SaveMProperties()

            'Debug.Print("Start NodeInfo: " + Now)

            'Dim nid As Integer

            oCommand = Nothing

            SaveRABudgetLimits()
            SaveRAPortfolioScenarios() 'AS/6-22-15
            SaveRAConstraints()
            SaveRAGroups()
            SaveRADependencies()
            SaveRAMustsMustnots()
            SaveRASettings()
            SaveAltsRisks()
            SaveRAFundingPools()
            SaveRACosts()

            dbConnection.Close()
            dbConnection = Nothing

            mDBConnection = Nothing 'C0812

            'Debug.Print("Finished saving as AHP: " + Now.ToString)

            Return True
        End Function

        Private Function SaveNonPairwiseData(ByVal node As ECCore.clsNode, ByVal NonPWData As ECCore.clsNonPairwiseMeasureData, Optional ByVal UseNodeTagAsRatingScale As Boolean = False) As Boolean
            If (ProjectManager Is Nothing) Or (node Is Nothing) Or (NonPWData Is Nothing) Then Return False 'C0812
            If NonPWData.IsUndefined Then Return True

            If WriteOnlyAllowedJudgmentsToAHP Then
                Dim nodesBelow As List(Of clsNode) = node.GetNodesBelow(NonPWData.UserID)
                Dim isAllowed As Boolean = False
                For Each child As clsNode In nodesBelow
                    If child.NodeID = NonPWData.NodeID Then
                        isAllowed = True
                        Exit For
                    End If
                Next
                If Not isAllowed Then
                    Return True
                End If
            End If

            If Not SavingToDatabaseInProgress() AndAlso Not CheckDBConnection(ProviderType, Location) Then Return False 'C0812

            Dim dbConnection As DbConnection
            If SavingToDatabaseInProgress() Then
                dbConnection = mDBConnection
            Else
                dbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()
            End If

            Dim oCommand As DbCommand
            If mDBCommand Is Nothing Then
                oCommand = GetDBCommand(ProviderType)
            Else
                oCommand = mDBCommand
            End If
            'C0921==

            Dim affected As Integer

            oCommand.Connection = dbConnection

            Dim UserID As Integer '= If(RD.UserID = COMBINED_USER_ID, 1, RD.UserID) ' in case = -1 write to combine

            ' synchronize userid
            If IsCombinedUserID(NonPWData.UserID) Then 'C0555
                'TODO: CHECK SUCH SITUATIONS (AHP DOESN'T HAVE MULTIPLE COMBINED GROUPS)
                UserID = 1
            Else
                UserID = GetAHPUserID(ProjectManager, ProjectManager.GetUserByID(NonPWData.UserID)) 'C0558
            End If

            Dim nid As Integer = If(IsNodesZeroBased(ProjectManager), NonPWData.ParentNodeID + 1, NonPWData.ParentNodeID)
            Dim aid As Integer = If(IsAltsZeroBased(ProjectManager), NonPWData.NodeID + 1, NonPWData.NodeID)


            'C0012===
            'Dim node As clsNode = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).GetNodeByID(rd.ParentNodeID)
            If node Is Nothing Then
                oCommand = Nothing 'C0059
                dbConnection.Close() 'C0059

                Return False
            End If

            Dim rd As clsRatingMeasureData = Nothing
            Dim IntensityID As Integer

            If TypeOf NonPWData Is clsRatingMeasureData Then
                rd = NonPWData
                IntensityID = rd.Rating.ID

                'If UseNodeTagAsRatingScale Then 'C0303
                If (IntensityID <> -1) And UseNodeTagAsRatingScale Then 'C0303
                    CType(node.MeasurementScale, clsRatingScale).Sort()
                    CType(node.Tag, clsRatingScale).Sort()

                    Dim rating As clsRating
                    For i As Integer = 0 To CType(node.MeasurementScale, clsRatingScale).RatingSet.Count - 1
                        rating = CType(node.MeasurementScale, clsRatingScale).RatingSet(i)
                        If rating.ID = rd.Rating.ID Then
                            IntensityID = CType(CType(node.Tag, clsRatingScale).RatingSet(i), clsRating).ID
                        End If
                    Next
                End If
            End If

            Try
                oCommand.CommandText = "INSERT INTO AltsData (PID, WRT, AID, DATA, [Note]) " +
                " VALUES (?, ?, ?, ?, ?)"
                oCommand.Parameters.Clear()
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "PID", UserID)) 'C0237
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "WRT", "N" + nid.ToString)) 'C0237
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "AID", "A" + aid.ToString)) 'C0237
                If (TypeOf NonPWData Is clsRatingMeasureData) Then
                    If (CType(NonPWData, clsRatingMeasureData).Rating IsNot Nothing) Then
                        If (CType(NonPWData, clsRatingMeasureData).Rating.ID <> -1) Then
                            'oCommand.Parameters.Add(GetDBParameter(ProviderType, "DATA", If(rd.UserID <> COMBINED_USER_ID, "I" + IntensityID.ToString, rd.Rating.Value))) 'C0235 'C0237 'C0555
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "DATA", If(Not IsCombinedUserID(rd.UserID), "I" + IntensityID.ToString, rd.Rating.Value))) 'C0555
                        Else
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "DATA", rd.Rating.Value))
                        End If
                    End If
                Else
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "DATA", CSng(NonPWData.ObjectValue))) 'C0235 'C0237
                End If

                If ProviderType = DBProviderType.dbptODBC Then
                    Dim tNote As Odbc.OdbcParameter = oCommand.CreateParameter
                    tNote.OdbcType = Odbc.OdbcType.Text
                    tNote.ParameterName = "Note"
                    tNote.Value = NonPWData.Comment
                    oCommand.Parameters.Add(tNote) 'C0237
                Else
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "Note", NonPWData.Comment)) 'C0237
                End If

                affected = DBExecuteNonQuery(ProviderType, oCommand)
            Catch ex2 As Exception
                oCommand = Nothing
                If Not SavingToDatabaseInProgress() Then 'C0812
                    dbConnection.Close() 'C0056
                End If
                'Debug.Print("Exception when saving non-pairwise judgment to ahp file (PID=" + UserID.ToString + ", WRT=N" + nid.ToString + ", AID=" + aid.ToString)

                Return False
            End Try

            Try
                ' otherwise try to write to the database using US culture
                oCommand.CommandText = "INSERT INTO AltsValues (PID, WRT, AID, ALTVALUE) " +
                "VALUES (?, ?, ?, ?)"
                oCommand.Parameters.Clear()
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "PID", UserID)) 'C0237
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "WRT", "N" + nid.ToString)) 'C0237
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "AID", "A" + aid.ToString)) 'C0237
                'C0235==
                If TypeOf NonPWData Is clsRatingMeasureData Then
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "ALTVALUE", rd.Rating.Value)) 'C0235 'C0237
                Else
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "ALTVALUE", CSng(NonPWData.SingleValue))) 'C0235 'C0237
                End If

                affected = DBExecuteNonQuery(ProviderType, oCommand)
            Catch ex1 As Exception
                oCommand = Nothing
                If Not SavingToDatabaseInProgress() Then 'C0812
                    dbConnection.Close() 'C0056
                End If
                'Debug.Print("Exception when saving non-pairwise judgment to ahp file (PID=" + UserID.ToString + ", WRT=N" + nid.ToString + ", AID=" + aid.ToString)

                Return False
            End Try
            'End If 'C0777

            oCommand = Nothing 'C0056
            If Not SavingToDatabaseInProgress() Then 'C0812
                dbConnection.Close() 'C0056
            End If
            Return True
        End Function

        Private Function SaveInfoDocs() As Boolean
            If ProjectManager Is Nothing Then Return False 'C0812
            If Not SavingToDatabaseInProgress() AndAlso Not CheckDBConnection(ProviderType, Location) Then Return False 'C0812

            'C0812===
            'If (ProjectManager Is Nothing) orelse Not CheckDBConnection(ProviderType, Location) Then
            '    Return False
            'End If
            'C0812==

            'Dim dbConnection As New odbc.odbcConnection(Location) 'C0235

            'C0812===
            'Dim dbConnection As DbConnection = GetDBConnection(ProviderType, Location) 'C0235
            'dbConnection.Open()
            'C0812==

            'C0812===
            Dim dbConnection As DbConnection
            If SavingToDatabaseInProgress() Then
                dbConnection = mDBConnection
            Else
                dbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()
            End If
            'C0812==

            'Dim oCommand As New odbc.odbcCommand 'C0235
            Dim oCommand As DbCommand = GetDBCommand(ProviderType) 'C0235
            Dim affected As Integer

            oCommand.Connection = dbConnection

            'C0921===
            Dim Transaction As DbTransaction = dbConnection.BeginTransaction
            oCommand.Transaction = Transaction
            'C0921==

            ' =============================
            ' Fill Documents table

            ' Delete everything from AltsGlobal table
            oCommand.CommandText = "DELETE FROM Documents"
            affected = DBExecuteNonQuery(ProviderType, oCommand)

            Dim nid As Integer
            Dim infodoc As String 'C0008

            For Each H As clsHierarchy In ProjectManager.GetAllHierarchies
                For Each node As clsNode In H.Nodes
                    If (H.HierarchyType = ECHierarchyType.htModel) Or ((H.HierarchyType = ECHierarchyType.htAlternative) And node.IsTerminalNode) Then
                        If (H.HierarchyType = ECHierarchyType.htModel) Then
                            nid = If(IsNodesZeroBased(ProjectManager), node.NodeID + 1, node.NodeID)
                            'C0008===
                            If (node.ParentNode Is Nothing) And (node.InfoDoc = "") Then
                                infodoc = GoalDefaultInfoDoc
                            Else
                                infodoc = node.InfoDoc
                            End If
                            'C0008==
                        Else
                            nid = If(IsAltsZeroBased(ProjectManager), node.NodeID + 1, node.NodeID)
                            infodoc = node.InfoDoc 'C0008
                        End If

                        Try
                            'If node.InfoDoc <> "" Then 'C0020
                            If infodoc <> "" Then 'C0020
                                oCommand.CommandText = "INSERT INTO Documents (NID, INFODOC, MODIFIED) VALUES (?, ?, ?)"
                                oCommand.Parameters.Clear()
                                'oCommand.Parameters.AddWithValue("NID", If(H.HierarchyType = ECHierarchyType.htModel, "N" + nid.ToString, "A" + nid.ToString)) 'C0235
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "NID", If(H.HierarchyType = ECHierarchyType.htModel, "N" + nid.ToString, "A" + nid.ToString))) 'C0235 'C0237
                                'oCommand.Parameters.AddWithValue("INFODOC", node.InfoDoc) 'C0008
                                ' D0164 === 
                                'oCommand.Parameters.AddWithValue("INFODOC", infodoc) 'C0008 -D0164

                                'C0235===
                                'Dim tInfoDoc As odbc.odbcParameter = oCommand.CreateParameter
                                'tInfoDoc.odbcType = odbc.odbcType.Text
                                'tInfoDoc.ParameterName = "INFODOC"
                                'tInfoDoc.Value = infodoc
                                'oCommand.Parameters.Add(tInfoDoc)
                                'C0235==

                                'C0235===
                                If ProviderType = DBProviderType.dbptODBC Then
                                    Dim tInfoDoc As Odbc.OdbcParameter = oCommand.CreateParameter
                                    tInfoDoc.OdbcType = Odbc.OdbcType.Text
                                    tInfoDoc.ParameterName = "INFODOC"
                                    tInfoDoc.Value = infodoc
                                    'oCommand.Parameters.Add(GetDBParameter(ProviderType, tInfoDoc)) 'C0237
                                    oCommand.Parameters.Add(tInfoDoc) 'C0237
                                Else
                                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "INFODOC", infodoc)) 'C0237
                                End If
                                'C0235==

                                ' D0164 ==
                                'oCommand.Parameters.AddWithValue("MODIFIED", 0) 'TODO: figure out this field 'C0235
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "MODIFIED", 0)) 'TODO: figure out this field 'C0235 'C0237

                                affected = DBExecuteNonQuery(ProviderType, oCommand)
                            End If
                        Catch ex As Exception
                            Transaction.Rollback() 'C0921
                            oCommand = Nothing
                            If Not SavingToDatabaseInProgress() Then 'C0812
                                dbConnection.Close()
                            End If
                            Return False
                        End Try
                    End If
                Next
            Next

            Transaction.Commit() 'C0921

            SaveAdvancedInfoDocs() 'C0948

            oCommand = Nothing
            If Not SavingToDatabaseInProgress() Then 'C0812
                dbConnection.Close()
            End If
            Return True
        End Function

        Private Function SaveAltsContribution() As Boolean
            If ProjectManager Is Nothing Then Return False 'C0812
            If Not SavingToDatabaseInProgress() AndAlso Not CheckDBConnection(ProviderType, Location) Then Return False 'C0812

            'C0812===
            'If (ProjectManager Is Nothing) orelse Not CheckDBConnection(ProviderType, Location) Then
            '    Return False
            'End If
            'C0812==

            'Dim dbConnection As New odbc.odbcConnection(Location) 'C0235

            'C0812===
            'Dim dbConnection As DbConnection = GetDBConnection(ProviderType, Location) 'C0235
            'dbConnection.Open()
            'C0812==

            'C0812===
            Dim dbConnection As DbConnection
            If SavingToDatabaseInProgress() Then
                dbConnection = mDBConnection
            Else
                dbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()
            End If
            'C0812==

            'Dim oCommand As New odbc.odbcCommand 'C0235
            Dim oCommand As DbCommand = GetDBCommand(ProviderType) 'C0235
            Dim affected As Integer

            oCommand.Connection = dbConnection

            'C0921===
            Dim Transaction As DbTransaction = dbConnection.BeginTransaction
            oCommand.Transaction = Transaction
            'C0921==

            ' =============================
            ' Fill AltsContributeTo table

            ' Delete everything from AltsGlobal table
            oCommand.CommandText = "DELETE FROM AltsContributeTo"
            affected = DBExecuteNonQuery(ProviderType, oCommand)

            Dim nid As Integer
            Dim aid As Integer

            ' insert ideal alternative
            'For Each node As clsNode In ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).TerminalNodes.Clone 'C0385
            For Each node As clsNode In ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).TerminalNodes 'C0385
                Try
                    nid = If(IsNodesZeroBased(ProjectManager), node.NodeID + 1, node.NodeID)

                    oCommand.CommandText = "INSERT INTO AltsContributeTo (AID, WRT) " +
                                        "VALUES (?, ?)"
                    oCommand.Parameters.Clear()
                    'C0235===
                    'oCommand.Parameters.AddWithValue("AID", "A0")
                    'oCommand.Parameters.AddWithValue("WRT", "N" + nid.ToString)
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "AID", "A0")) 'C0237
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "WRT", "N" + nid.ToString)) 'C0237
                    'C0235==
                    affected = DBExecuteNonQuery(ProviderType, oCommand)
                Catch ex As Exception
                    Transaction.Rollback() 'C0921
                    oCommand = Nothing
                    If Not SavingToDatabaseInProgress() Then 'C0812
                        dbConnection.Close()
                    End If
                    Return False
                End Try
            Next

            ' insert alternatives
            'For Each alt As clsNode In ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).TerminalNodes.Clone 'C0385
            For Each alt As clsNode In ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).TerminalNodes 'C0385
                'For Each node As clsNode In ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).TerminalNodes.Clone 'C0385
                For Each node As clsNode In ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).TerminalNodes 'C0385
                    'If node.GetNodesBelow.IndexOf(alt) <> -1 Then 'C0012 'C0450
                    If node.GetNodesBelow(UNDEFINED_USER_ID).Contains(alt) Then 'C0450
                        Try
                            nid = If(IsNodesZeroBased(ProjectManager), node.NodeID + 1, node.NodeID)
                            aid = If(IsAltsZeroBased(ProjectManager), alt.NodeID + 1, alt.NodeID)

                            oCommand.CommandText = "INSERT INTO AltsContributeTo (AID, WRT) " +
                                                "VALUES (?, ?)"
                            oCommand.Parameters.Clear()
                            'C0235===
                            'oCommand.Parameters.AddWithValue("AID", "A" + aid.ToString)
                            'oCommand.Parameters.AddWithValue("WRT", "N" + nid.ToString)
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "AID", "A" + aid.ToString)) 'C0237
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "WRT", "N" + nid.ToString)) 'C0237
                            'C0235==

                            affected = DBExecuteNonQuery(ProviderType, oCommand)
                        Catch ex As Exception
                            Transaction.Rollback() 'C0921
                            oCommand = Nothing
                            If Not SavingToDatabaseInProgress() Then 'C0812
                                dbConnection.Close()
                            End If
                            Return False
                        End Try
                    End If
                Next
            Next

            Transaction.Commit() 'C0921

            oCommand = Nothing
            If Not SavingToDatabaseInProgress() Then 'C0812
                dbConnection.Close()
            End If
            Return True
        End Function

        Private Function SaveAltsRisks() As Boolean
            If ProjectManager Is Nothing Then Return False
            If Not SavingToDatabaseInProgress() AndAlso Not CheckDBConnection(ProviderType, Location) Then Return False

            Dim dbConnection As DbConnection
            If SavingToDatabaseInProgress() Then
                dbConnection = mDBConnection
            Else
                dbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()
            End If

            Dim oCommand As DbCommand = GetDBCommand(ProviderType)
            Dim affected As Integer

            oCommand.Connection = dbConnection

            oCommand.CommandText = "DELETE FROM RArisks"
            oCommand.Parameters.Clear()
            affected = DBExecuteNonQuery(ProviderType, oCommand)

            Dim Transaction As DbTransaction = dbConnection.BeginTransaction
            oCommand.Transaction = Transaction

            For Each scenario As RAScenario In ProjectManager.ResourceAligner.Scenarios.Scenarios.Values
                For Each alt As Canvas.RAAlternative In scenario.AlternativesFull
                    Dim altNode As clsNode = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(New Guid(alt.ID))
                    If altNode IsNot Nothing Then
                        Dim Risk As Double = alt.RiskOriginal
                        If Risk <> UNDEFINED_ATTRIBUTE_DEFAULT_DOUBLE_VALUE Then
                            Try
                                oCommand.CommandText = "INSERT INTO RArisks (PSID, AID, Risk) VALUES (?, ?, ?)"
                                oCommand.Parameters.Clear()
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "PSID", scenario.ID))
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "AID", "A" + If(IsAltsZeroBased(ProjectManager), altNode.NodeID + 1, altNode.NodeID).ToString))
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "Risk", Risk.ToString))
                                affected = DBExecuteNonQuery(ProviderType, oCommand)
                            Catch ex As Exception
                                Transaction.Rollback()
                                oCommand = Nothing
                                If Not SavingToDatabaseInProgress() Then
                                    dbConnection.Close()
                                End If
                                Return False
                            End Try
                        End If
                    End If
                Next
            Next

            Transaction.Commit()

            oCommand = Nothing
            If Not SavingToDatabaseInProgress() Then
                dbConnection.Close()
            End If
            Return True
        End Function

        Protected Function SaveAdvancedInfoDocs() As Boolean 'C0951
            If ProjectManager Is Nothing Then Return False
            Dim IsAHP As Boolean = True

            'C0953===
            If Not TableExists(Location, ProviderType, "AdvancedInfoDocs") Then
                Return False
            End If
            'C0953==

            If Not SavingToDatabaseInProgress() AndAlso Not CheckDBConnection(ProviderType, Location) Then Return False

            Dim dbConnection As DbConnection
            If SavingToDatabaseInProgress() Then
                dbConnection = mDBConnection
            Else
                dbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()
            End If

            Try
                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection

                Dim targetID As Integer
                Dim parentID As Integer

                Dim targetOwnerID As Integer
                Dim parentOwnerID As Integer

                Dim TargetNode As clsNode
                Dim ParentNode As clsNode

                For Each infoDoc As clsInfoDoc In ProjectManager.InfoDocs.InfoDocs
                    Select Case infoDoc.DocumentType
                        Case InfoDocType.idtNodeWRTParent
                            TargetNode = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).GetNodeByID(infoDoc.TargetID)
                            If TargetNode IsNot Nothing Then
                                targetID = If(IsAHP AndAlso IsNodesZeroBased(ProjectManager), TargetNode.NodeID + 1, TargetNode.NodeID)
                                targetOwnerID = ProjectManager.ActiveHierarchy
                            Else
                                TargetNode = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(infoDoc.TargetID)
                                If TargetNode IsNot Nothing Then
                                    targetID = If(IsAHP AndAlso IsAltsZeroBased(ProjectManager), TargetNode.NodeID + 1, TargetNode.NodeID)
                                    targetOwnerID = ProjectManager.ActiveAltsHierarchy
                                End If
                            End If

                            ParentNode = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).GetNodeByID(infoDoc.AdditionalID)
                            If ParentNode IsNot Nothing Then
                                parentID = If(IsAHP AndAlso IsNodesZeroBased(ProjectManager), ParentNode.NodeID + 1, ParentNode.NodeID)
                                parentOwnerID = ProjectManager.ActiveHierarchy
                            Else
                                ParentNode = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(infoDoc.AdditionalID)
                                If ParentNode IsNot Nothing Then
                                    parentID = If(IsAHP AndAlso IsAltsZeroBased(ProjectManager), ParentNode.NodeID + 1, ParentNode.NodeID)
                                    parentOwnerID = ProjectManager.ActiveAltsHierarchy
                                End If
                            End If

                            If TargetNode IsNot Nothing And ParentNode IsNot Nothing Then
                                oCommand.CommandText = "INSERT INTO AdvancedInfoDocs (DocType, TargetID, TargetOwnerID, AdditionalID, AdditionalOwnerID, InfoDoc) VALUES (?, ?, ?, ?, ?, ?)"
                                oCommand.Parameters.Clear()
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "DocType", CType(infoDoc.DocumentType, Integer)))
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "TargetID", targetID))
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "TargetOwnerID", targetOwnerID))
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "AdditionalID", parentID))
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "AdditionalOwnerID", parentOwnerID))
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "InfoDoc", infoDoc.InfoDoc))

                                Dim affected As Integer = DBExecuteNonQuery(ProviderType, oCommand)
                            End If
                        Case InfoDocType.idtNode
                            TargetNode = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).GetNodeByID(infoDoc.TargetID)
                            If TargetNode IsNot Nothing Then
                                targetID = If(IsAHP AndAlso IsNodesZeroBased(ProjectManager), TargetNode.NodeID + 1, TargetNode.NodeID)
                                targetOwnerID = ProjectManager.ActiveHierarchy
                            Else
                                TargetNode = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(infoDoc.TargetID)
                                If TargetNode IsNot Nothing Then
                                    targetID = If(IsAHP AndAlso IsAltsZeroBased(ProjectManager), TargetNode.NodeID + 1, TargetNode.NodeID)
                                    targetOwnerID = ProjectManager.ActiveAltsHierarchy
                                End If
                            End If

                            parentID = -1
                            parentOwnerID = targetOwnerID

                            If TargetNode IsNot Nothing Then
                                oCommand.CommandText = "INSERT INTO AdvancedInfoDocs (DocType, TargetID, TargetOwnerID, AdditionalID, AdditionalOwnerID, InfoDoc) VALUES (?, ?, ?, ?, ?, ?)"
                                oCommand.Parameters.Clear()
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "DocType", CType(infoDoc.DocumentType, Integer)))
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "TargetID", targetID))
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "TargetOwnerID", targetOwnerID))
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "AdditionalID", parentID))
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "AdditionalOwnerID", parentOwnerID))
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "InfoDoc", infoDoc.InfoDoc))

                                Dim affected As Integer = DBExecuteNonQuery(ProviderType, oCommand)
                            End If
                    End Select
                Next

                'For Each H As clsHierarchy In ProjectManager.GetAllHierarchies
                '    For Each node As clsNode In H.Nodes
                '        If node.InfoDoc <> "" Then
                '            targetID = If(IsAHP AndAlso projectmanager.IsNodesZeroBased(), node.NodeID + 1, node.NodeID)
                '            targetOwnerID = H.HierarchyID

                '            oCommand.CommandText = "INSERT INTO AdvancedInfoDocs (DocType, TargetID, TargetOwnerID, AdditionalID, AdditionalOwnerID, InfoDoc) VALUES (?, ?, ?, ?, ?, ?)"
                '            oCommand.Parameters.Clear()
                '            oCommand.Parameters.Add(GetDBParameter(ProviderType, "DocType", CType(InfoDocType.idtNode, Integer)))
                '            oCommand.Parameters.Add(GetDBParameter(ProviderType, "TargetID", targetID))
                '            oCommand.Parameters.Add(GetDBParameter(ProviderType, "TargetOwnerID", targetOwnerID))
                '            oCommand.Parameters.Add(GetDBParameter(ProviderType, "AdditionalID", -1))
                '            oCommand.Parameters.Add(GetDBParameter(ProviderType, "AdditionalOwnerID", targetOwnerID))
                '            oCommand.Parameters.Add(GetDBParameter(ProviderType, "InfoDoc", node.InfoDoc))

                '            Dim affected As Integer = DBExecuteNonQuery(ProviderType, oCommand)
                '        End If
                '    Next
                'Next
                oCommand = Nothing
            Finally
                If Not SavingToDatabaseInProgress() Then
                    dbConnection.Close()
                End If
            End Try

            Return True
        End Function
#End Region
    End Class
End Namespace