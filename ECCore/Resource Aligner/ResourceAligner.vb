Option Strict On    ' D2882

Imports ECCore
Imports Canvas
Imports System.Collections.Generic
Imports System.Linq
Imports System.IO
Imports System.Runtime.Serialization.Formatters.Binary
Imports System.Data.Common
Imports System.Web.Script.Serialization
Imports System.Collections.Specialized

Namespace Canvas
    <Serializable()> Public Class ResourceAligner    ' D2837
        Public ProjectManager As clsProjectManager
        Public Scenarios As RAScenarios
        Public Solver As RASolver
        Public isLoading As Boolean = False     ' D3240
        Public Property isLoaded As Boolean = False
        'Private Property ScenarioComparisonSettings As RASettings    'A0892 ' AD: deprecated and stay there for backward compat. Use .GlobalSettings instead.

        Public CombinedGroupUserID As Integer

        Public Logs As String = ""      ' D4516

        'Public Property RiskOptimizer As RiskOptimizer
        Public ReadOnly Property RiskOptimizer As RASolver
            Get
                Return Solver
            End Get
        End Property

        Public Sub Solve()
            Solver.Solve(raSolverExport.raNone)    ' D3236
        End Sub

        '''' <summary>
        '''' Loads Resource Aligner data from streams database for current model
        '''' </summary>
        '''' <returns>Returns True is succeeded, False otherwise</returns>
        '''' <remarks></remarks>
        'Public Function Load() As Boolean
        '    ' D2849 ===
        '    Dim fResult As Boolean = False
        '    isLoading = True    ' D3240
        '    With ProjectManager.StorageManager

        '        If .StorageType = ECModelStorageType.mstCanvasStreamDatabase Then
        '            ' D3651 ===
        '            Dim sError As String = ""
        '            If .CanvasDBVersion.MinorVersion <= 30 Then
        '                fResult = Load_CanvasStreamsDatabaseOld(.ProjectLocation, .ProviderType, .ModelID, sError)
        '            End If
        '            'If fResult AndAlso String.IsNullOrEmpty(sError) Then
        '            '    Save_CanvasStreamsDatabase(.ProjectLocation, .ProviderType, .ModelID)
        '            'End If
        '            If Not fResult AndAlso String.IsNullOrEmpty(sError) Then fResult = Load_CanvasStreamsDatabaseNew(.ProjectLocation, .ProviderType, .ModelID)
        '            ' D3651 ==

        '            ProjectManager.LoadPipeParameters(PipeStorageType.pstStreamsDatabase, .ModelID)

        '            Scenarios.AddDefaultScenario()

        '            isLoading = False

        '            Scenarios.GlobalSettings.ResourceAligner = Me   ' D3249 // for override wrong PM/StorageManager after deserialization
        '            '-A1224 Scenarios.CheckAlternatives()
        '            '-A1224 'Scenarios.UpdateScenarioBenefits()
        '            '-A1224 Scenarios.UpdateSortOrder()
        '            Scenarios.CheckModel(False) 'A1324
        '            If Scenarios.Scenarios.Count > 0 Then
        '                'Scenarios.ActiveScenarioID = CType(Scenarios.Scenarios.ElementAt(0).Value, RAScenario).ID ' D3213 -D4325
        '                Scenarios.ActiveScenarioID = ProjectManager.Parameters.RAActiveScenarioID   ' D3859
        '                Scenarios.SyncLinkedConstraints()   ' D3340

        '                ' D3826 ===
        '                'Dim fNeedUpdate As Boolean = False
        '                For Each tScenID As Integer In Scenarios.Scenarios.Keys
        '                    Dim tScen As RAScenario = Scenarios.Scenarios(tScenID)
        '                    If RA_OPT_FORCE_CC_USE_IN_TIMEPERIODS Then
        '                        For Each tCID As Integer In tScen.Constraints.Constraints.Keys
        '                            Dim tConstr As RAConstraint = tScen.Constraints.Constraints(tCID)
        '                            If Not tConstr.IsLinkedToResource Then
        '                                tConstr.IsLinkedToResource = True
        '                                'fNeedUpdate = True
        '                            End If
        '                        Next
        '                    End If
        '                Next
        '                ' D3826 ==

        '                If RA_OPT_USE_SOLVER_PRIORITIES Then LoadSolverPriorities() ' D4364
        '                LoadFundingPoolsOrder() ' D4367

        '            End If
        '        End If
        '    End With

        '    ' D3877 ===
        '    If Solver IsNot Nothing AndAlso ProjectManager IsNot Nothing Then
        '        With ProjectManager.Parameters
        '            Solver.SolverLibrary = .RASolver
        '            Solver.XAStrategy = .RASolver_XAStrategy
        '            Solver.XAVariation = .RASolver_XAVariation
        '            Solver.XATimeOutGlobal = .RASolver_XATimeout
        '            Solver.XATimeoutUnchanged = .RASolver_XATimeoutUnchanged
        '        End With
        '    End If
        '    ' D3877 ==

        '    isLoading = False   ' D3240

        '    Return fResult
        '    ' D2849 ==
        'End Function

        Private Sub FixScenarios()
            If Scenarios.GlobalSettings Is Nothing Then Scenarios.GlobalSettings = New RAGlobalSettings(Me)
            For Each scenario As RAScenario In Scenarios.Scenarios.Values
                If scenario.SolverPriorities Is Nothing Then scenario.SolverPriorities = New RASolverPriorities(scenario)
                If scenario.Settings Is Nothing Then scenario.Settings = New RASettings
                If scenario.Groups Is Nothing Then scenario.Groups = New RAGroups
                If scenario.EventGroups Is Nothing Then scenario.EventGroups = New RAGroups
                If scenario.Constraints Is Nothing Then scenario.Constraints = New RAConstraints(scenario)
                If scenario.Dependencies Is Nothing Then scenario.Dependencies = New RADependencies
                If scenario.TimePeriodsDependencies Is Nothing Then scenario.TimePeriodsDependencies = New RADependencies
                If scenario.TimePeriods Is Nothing Then scenario.TimePeriods = New RATimePeriods(scenario)
                If scenario.FundingPools Is Nothing Then scenario.FundingPools = New RAFundingPools
                If scenario.AlternativesFull Is Nothing Then scenario.AlternativesFull = scenario.Alternatives
            Next
        End Sub

        Public Function Load() As Boolean
            Return Load(ECModelStorageType.mstCanvasStreamDatabase, ProjectManager.StorageManager.Location, ProjectManager.StorageManager.ProviderType, ProjectManager.StorageManager.ModelID)
        End Function

        Public Function Load(ByVal StorageType As ECModelStorageType, ByVal Location As String, ByVal ProviderType As DBProviderType, ByVal ModelID As Integer) As Boolean
            Select Case StorageType
                Case ECModelStorageType.mstCanvasStreamDatabase
                    If isLoaded Then Return True

                    isLoading = True

                    Dim sError As String = ""
                    Dim res As Boolean = False
                    If ProjectManager.StorageManager.CanvasDBVersion.MinorVersion <= 30 AndAlso HasOldRAData(ProviderType, Location, ModelID) Then
                        res = Load_CanvasStreamsDatabaseOld(Location, ProviderType, ModelID, sError)
                        If res Then
                            FixScenarios()
                            Save(StorageType, Location, ProviderType, ModelID)
                        End If
                    End If

                    MiscFuncs.PrintDebugInfo("LoadRA started - ")

                    If Not res AndAlso String.IsNullOrEmpty(sError) Then res = Load_CanvasStreamsDatabaseNew(Location, ProviderType, ModelID)
                    Scenarios.AddDefaultScenario()

                    isLoading = False

                    Scenarios.GlobalSettings.ResourceAligner = Me   ' D3249 // for override wrong PM/StorageManager after deserialization

                    Scenarios.CheckModel(False) 'A1324
                    If Scenarios.Scenarios.Count > 0 Then
                        If Scenarios.Scenarios.ContainsKey(ProjectManager.Parameters.RAActiveScenarioID) Then   ' D6064
                            Scenarios.ActiveScenarioID = ProjectManager.Parameters.RAActiveScenarioID   ' D3859
                        Else
                            Scenarios.ActiveScenarioID = CType(Scenarios.Scenarios.ElementAt(0).Value, RAScenario).ID   ' D3213
                        End If
                        Scenarios.SyncLinkedConstraintsValues()   ' D3340
                        Scenarios.SyncLinkedConstraintsToResources()    ' D4913

                        If RA_OPT_USE_SOLVER_PRIORITIES Then LoadSolverPriorities() ' D4364
                        LoadFundingPoolsOrder() ' D4367
                    End If

                    ' D3877 ===
                    If Solver IsNot Nothing AndAlso ProjectManager IsNot Nothing Then
                        With ProjectManager.Parameters
                            Solver.SolverLibrary = .RASolver
                            Solver.XAStrategy = .RASolver_XAStrategy
                            Solver.XAVariation = .RASolver_XAVariation
                            Solver.XATimeOutGlobal = .RASolver_XATimeout
                            Solver.XATimeoutUnchanged = .RASolver_XATimeoutUnchanged
                            Solver.UseFundingPoolsPriorities = .RAFundingPoolsUserPrty  ' D7098
                        End With
                    End If
                    ' D3877 ==

                    MiscFuncs.PrintDebugInfo("LoadRA done - " + Now.ToString)
                    isLoaded = True
                    Return res
                Case ECModelStorageType.mstAHPDatabase
                    Return Load_AHPDatabase(Location, ProviderType)
            End Select
            Return False
        End Function

        Private Function Load_CanvasStreamsDatabaseOld(ByVal Location As String, ByVal ProviderType As DBProviderType, ByVal ModelID As Integer, ByRef sError As String) As Boolean ' D3651
            Dim fResult As Boolean = False  ' D3651
            Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)

                dbConnection.Open()

                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection

                oCommand.CommandText = "SELECT * FROM ModelStructure WHERE ProjectID=? AND StructureType=?"
                oCommand.Parameters.Clear()
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "StructureType", CInt(StructureType.stResourceAligner)))

                Dim dbReader As DbDataReader
                dbReader = DBExecuteReader(ProviderType, oCommand)

                Dim MS As New MemoryStream

                If dbReader.HasRows Then
                    Try
                        dbReader.Read()

                        Dim bufferSize As Integer = CInt(dbReader("StreamSize"))    ' The size of the BLOB buffer.
                        Dim outbyte(bufferSize - 1) As Byte  ' The BLOB byte() buffer to be filled by GetBytes.
                        Dim retval As Long                   ' The bytes returned from GetBytes.
                        Dim startIndex As Long = 0           ' The starting position in the BLOB output.

                        retval = dbReader.GetBytes(CInt(dbReader.GetOrdinal("Stream")), startIndex, outbyte, 0, bufferSize)

                        Dim bw As BinaryWriter = New BinaryWriter(MS)

                        ' Continue reading and writing while there are bytes beyond the size of the buffer.
                        Do While retval = bufferSize
                            bw.Write(outbyte)
                            bw.Flush()
                            ' Reposition the start index to the end of the last buffer and fill the buffer.
                            startIndex += bufferSize
                            retval = dbReader.GetBytes(CInt(dbReader.GetOrdinal("Stream")), startIndex, outbyte, 0, bufferSize)
                        Loop

                        ' Write the remaining buffer.
                        bw.Write(outbyte, 0, CInt(retval))
                        bw.Flush()


                        dbReader.Close()
                        dbConnection.Close()

                        MS.Position = 0
                        Scenarios = Deserialize(MS)
                        Scenarios.ResourceAligner = Me
                        Scenarios.ActiveScenarioID = Scenarios.AddDefaultScenario

                        ' D3651 ===
                        fResult = True

                    Catch ex As Exception
                        sError = "old_ra"
                    End Try
                    ' D3651 ==
                Else
                    dbReader.Close()
                    dbConnection.Close()
                End If

            End Using

            If Not fResult AndAlso Not String.IsNullOrEmpty(sError) AndAlso Solver IsNot Nothing Then Solver.LastError = sError ' D3651

            Return fResult  ' D3651
        End Function

        Private Function Load_CanvasStreamsDatabaseNew(ByVal Location As String, ByVal ProviderType As DBProviderType, ByVal ModelID As Integer) As Boolean
            Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()

                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection

                oCommand.CommandText = "SELECT * FROM ModelStructure WHERE ProjectID=? AND StructureType=?"
                oCommand.Parameters.Clear()
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "StructureType", CInt(StructureType.stResourceAlignerNew)))

                Dim dbReader As DbDataReader
                dbReader = DBExecuteReader(ProviderType, oCommand)

                Dim MS As New MemoryStream

                If dbReader.HasRows Then
                    dbReader.Read()

                    Dim bufferSize As Integer = CInt(dbReader("StreamSize"))    ' The size of the BLOB buffer.
                    Dim outbyte(bufferSize - 1) As Byte  ' The BLOB byte() buffer to be filled by GetBytes.
                    Dim retval As Long                   ' The bytes returned from GetBytes.
                    Dim startIndex As Long = 0           ' The starting position in the BLOB output.

                    retval = dbReader.GetBytes(CInt(dbReader.GetOrdinal("Stream")), startIndex, outbyte, 0, bufferSize)

                    Dim bw As BinaryWriter = New BinaryWriter(MS)

                    ' Continue reading and writing while there are bytes beyond the size of the buffer.
                    Do While retval = bufferSize
                        bw.Write(outbyte)
                        bw.Flush()
                        ' Reposition the start index to the end of the last buffer and fill the buffer.
                        startIndex += bufferSize
                        retval = dbReader.GetBytes(CInt(dbReader.GetOrdinal("Stream")), startIndex, outbyte, 0, bufferSize)
                    Loop

                    ' Write the remaining buffer.
                    bw.Write(outbyte, 0, CInt(retval))
                    bw.Flush()

                    dbReader.Close()
                    dbConnection.Close()

                    MS.Position = 0
                    ParseRAStream(MS)
                    Scenarios.ResourceAligner = Me
                Else
                    dbReader.Close()
                    dbConnection.Close()
                    Return False
                End If
            End Using

            Return True
        End Function

        Public Function HasOldRAData(ProviderType As DBProviderType, Location As String, ModelID As Integer) As Boolean
            Dim count As Integer = 0
            Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()

                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection

                oCommand.CommandText = "SELECT COUNT(*) FROM ModelStructure WHERE ProjectID=? AND StructureType=?"
                oCommand.Parameters.Clear()
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "StructureType", StructureType.stResourceAligner))
                Dim obj As Object = DBExecuteScalar(ProviderType, oCommand)
                count = CInt(If(obj Is Nothing, 0, CType(obj, Integer)))

                oCommand = Nothing
            End Using
            Return count <> 0
        End Function

        Private Function Load_AHPDatabase(ByVal dbConnectionString As String, ByVal ProviderType As DBProviderType) As Boolean
            Return True
        End Function

        ''' <summary>
        ''' Saves Resource Aligner data to streams database for current model
        ''' </summary>
        ''' <returns>Returns True is succeeded, False otherwise</returns>
        ''' <remarks></remarks>
        Public Function Save() As Boolean
            If isLoading Then Return False
            With ProjectManager.StorageManager
                Select Case .StorageType
                    Case ECModelStorageType.mstCanvasStreamDatabase
                        Return Save_CanvasStreamsDatabase(.ProjectLocation, .ProviderType, .ModelID)
                End Select
            End With
        End Function

        Public Function Save(ByVal StorageType As ECModelStorageType, ByVal Location As String, ByVal ProviderType As DBProviderType, ByVal ModelID As Integer) As Boolean
            Select Case StorageType
                Case ECModelStorageType.mstCanvasStreamDatabase
                    Return Save_CanvasStreamsDatabase(Location, ProviderType, ModelID)
                Case ECModelStorageType.mstAHPDatabase
                    Return Save_AHPDatabase(Location, ProviderType)
            End Select
            Return False
        End Function

        Private Function GetRAStream_1_1_30() As MemoryStream
            Dim MS As New MemoryStream
            Dim BW As New BinaryWriter(MS)

            BW.Write(Scenarios.Scenarios.Count)
            BW.Write(Scenarios.ActiveScenarioID)

            For Each Scenario As RAScenario In Scenarios.Scenarios.Values
                BW.Write(Scenario.ID)
                BW.Write(Scenario.Name)
                BW.Write(Scenario.Description)
                BW.Write(Scenario.Index)
                BW.Write(Scenario.IsCheckedCS)
                BW.Write(Scenario.IsCheckedIB)
                BW.Write(Scenario.Budget)

                ' settings
                BW.Write(Scenario.Settings.BaseCaseForConstraints)
                BW.Write(Scenario.Settings.BaseCaseForDependencies)
                BW.Write(Scenario.Settings.BaseCaseForFundingPools)
                BW.Write(Scenario.Settings.BaseCaseForGroups)
                BW.Write(Scenario.Settings.BaseCaseForMustNots)
                BW.Write(Scenario.Settings.BaseCaseForMusts)
                BW.Write(Scenario.Settings.CustomConstraints)
                BW.Write(Scenario.Settings.Dependencies)
                BW.Write(Scenario.Settings.FundingPools)
                BW.Write(Scenario.Settings.Groups)
                BW.Write(Scenario.Settings.MustNots)
                BW.Write(Scenario.Settings.Musts)
                BW.Write(Scenario.Settings.Risks)
                BW.Write(Scenario.Settings.UseBaseCase)
                BW.Write(Scenario.Settings.UseBaseCaseOptions)
                BW.Write(Scenario.Settings.UseIgnoreOptions)

                ' alternatives
                BW.Write(Scenario.Alternatives.Count)
                For Each raAlt As RAAlternative In Scenario.Alternatives
                    BW.Write(raAlt.ID)
                    BW.Write(raAlt.Name)
                    BW.Write(raAlt.BenefitOriginal)
                    BW.Write(raAlt.Cost)
                    BW.Write(raAlt.IsPartial)
                    BW.Write(raAlt.MinPercent)
                    BW.Write(raAlt.Must)
                    BW.Write(raAlt.MustNot)
                    BW.Write(raAlt.RiskOriginal)
                    BW.Write(raAlt.SBPriority)
                    BW.Write(raAlt.SBTotal)
                    BW.Write(raAlt.SortOrder)
                Next

                ' groups
                BW.Write(Scenario.Groups.Groups.Values.Count)
                For Each group As RAGroup In Scenario.Groups.Groups.Values
                    BW.Write(group.ID)
                    BW.Write(group.IntID)
                    BW.Write(group.Name)
                    BW.Write(group.Enabled)
                    BW.Write(CInt(group.Condition))

                    BW.Write(group.Alternatives.Count)
                    For Each raAlt As RAAlternative In group.Alternatives.Values
                        BW.Write(raAlt.ID)
                    Next
                Next

                ' dependencies
                BW.Write(Scenario.Dependencies.Dependencies.Count)
                For Each dependency As RADependency In Scenario.Dependencies.Dependencies
                    BW.Write(dependency.FirstAlternativeID)
                    BW.Write(dependency.SecondAlternativeID)
                    BW.Write(CInt(dependency.Value))
                Next

                ' custom constraints
                BW.Write(Scenario.Constraints.Constraints.Count)
                For Each CC As RAConstraint In Scenario.Constraints.Constraints.Values
                    BW.Write(CC.ID)
                    BW.Write(CC.Name)
                    BW.Write(CC.Enabled)
                    BW.Write(CC.IsReadOnly)
                    BW.Write(CC.MaxValue)
                    BW.Write(CC.MinValue)
                    BW.Write(CC.LinkedAttributeID.ToByteArray)
                    BW.Write(CC.LinkedEnumID.ToByteArray)

                    BW.Write(CC.AlternativesData.Count)
                    For Each kvp As KeyValuePair(Of String, Double) In CC.AlternativesData
                        BW.Write(kvp.Key)
                        BW.Write(kvp.Value)
                    Next
                Next

                ' funding pools
                BW.Write(Scenario.FundingPools.Pools.Count)
                For Each FP As RAFundingPool In Scenario.FundingPools.Pools.Values
                    BW.Write(FP.ID)
                    BW.Write(FP.Name)
                    BW.Write(FP.Enabled)
                    BW.Write(FP.PoolLimit)

                    BW.Write(FP.Values.Count)
                    For Each kvp As KeyValuePair(Of String, Double) In FP.Values
                        BW.Write(kvp.Key)
                        BW.Write(kvp.Value)
                    Next
                Next
            Next

            ' global settings
            BW.Write(Scenarios.GlobalSettings.isAutoSolve)
            BW.Write(Scenarios.GlobalSettings.Precision)
            BW.Write(Scenarios.GlobalSettings.ShowFrozenHeaders)
            BW.Write(CInt(Scenarios.GlobalSettings.SortBy))

            BW.Close()

            Return MS
        End Function

        Private Function GetRAStream_1_1_31() As MemoryStream
            Dim MS As New MemoryStream
            Dim BW As New BinaryWriter(MS)

            BW.Write(Scenarios.Scenarios.Count)
            BW.Write(Scenarios.ActiveScenarioID)

            For Each Scenario As RAScenario In Scenarios.Scenarios.Values
                BW.Write(Scenario.ID)
                BW.Write(Scenario.Name)
                BW.Write(Scenario.Description)
                BW.Write(Scenario.Index)
                BW.Write(Scenario.IsCheckedCS)
                BW.Write(Scenario.IsCheckedIB)
                BW.Write(Scenario.Budget)

                ' settings
                BW.Write(Scenario.Settings.BaseCaseForConstraints)
                BW.Write(Scenario.Settings.BaseCaseForDependencies)
                BW.Write(Scenario.Settings.BaseCaseForFundingPools)
                BW.Write(Scenario.Settings.BaseCaseForGroups)
                BW.Write(Scenario.Settings.BaseCaseForMustNots)
                BW.Write(Scenario.Settings.BaseCaseForMusts)
                BW.Write(Scenario.Settings.CustomConstraints)
                BW.Write(Scenario.Settings.Dependencies)
                BW.Write(Scenario.Settings.FundingPools)
                BW.Write(Scenario.Settings.Groups)
                BW.Write(Scenario.Settings.MustNots)
                BW.Write(Scenario.Settings.Musts)
                BW.Write(Scenario.Settings.Risks)
                BW.Write(Scenario.Settings.UseBaseCase)
                BW.Write(Scenario.Settings.UseBaseCaseOptions)
                BW.Write(Scenario.Settings.UseIgnoreOptions)

                ' alternatives
                BW.Write(Scenario.Alternatives.Count)
                For Each raAlt As RAAlternative In Scenario.Alternatives
                    BW.Write(raAlt.ID)
                    BW.Write(raAlt.Name)
                    BW.Write(raAlt.BenefitOriginal)
                    BW.Write(raAlt.Cost)
                    BW.Write(raAlt.IsPartial)
                    BW.Write(raAlt.MinPercent)
                    BW.Write(raAlt.Must)
                    BW.Write(raAlt.MustNot)
                    BW.Write(raAlt.RiskOriginal)
                    BW.Write(raAlt.SBPriority)
                    BW.Write(raAlt.SBTotal)
                    BW.Write(raAlt.SortOrder)
                Next

                ' groups
                BW.Write(Scenario.Groups.Groups.Values.Count)
                For Each group As RAGroup In Scenario.Groups.Groups.Values
                    BW.Write(group.ID)
                    BW.Write(group.IntID)
                    BW.Write(group.Name)
                    BW.Write(group.Enabled)
                    BW.Write(CInt(group.Condition))

                    BW.Write(group.Alternatives.Count)
                    For Each raAlt As RAAlternative In group.Alternatives.Values
                        BW.Write(raAlt.ID)
                    Next
                Next

                ' dependencies
                BW.Write(Scenario.Dependencies.Dependencies.Count)
                For Each dependency As RADependency In Scenario.Dependencies.Dependencies
                    BW.Write(dependency.FirstAlternativeID)
                    BW.Write(dependency.SecondAlternativeID)
                    BW.Write(CInt(dependency.Value))
                Next

                ' custom constraints
                BW.Write(Scenario.Constraints.Constraints.Count)
                For Each CC As RAConstraint In Scenario.Constraints.Constraints.Values
                    BW.Write(CC.ID)
                    BW.Write(CC.Name)
                    BW.Write(CC.Enabled)
                    BW.Write(CC.IsReadOnly)
                    BW.Write(CC.MaxValue)
                    BW.Write(CC.MinValue)
                    BW.Write(CC.LinkedAttributeID.ToByteArray)
                    BW.Write(CC.LinkedEnumID.ToByteArray)

                    ' new to 1.1.31
                    BW.Write(CC.ECD_AID)
                    BW.Write(CC.ECD_AssociatedCVID)
                    BW.Write(CC.ECD_AssociatedUDcolKey)
                    BW.Write(CC.ECD_CCID)
                    BW.Write(CC.ECD_SOrder)

                    BW.Write(CC.AlternativesData.Count)
                    For Each kvp As KeyValuePair(Of String, Double) In CC.AlternativesData
                        BW.Write(kvp.Key)
                        BW.Write(kvp.Value)
                    Next
                Next

                ' funding pools
                BW.Write(Scenario.FundingPools.Pools.Count)
                For Each FP As RAFundingPool In Scenario.FundingPools.Pools.Values
                    BW.Write(FP.ID)
                    BW.Write(FP.Name)
                    BW.Write(FP.Enabled)
                    BW.Write(FP.PoolLimit)

                    BW.Write(FP.Values.Count)
                    For Each kvp As KeyValuePair(Of String, Double) In FP.Values
                        BW.Write(kvp.Key)
                        BW.Write(kvp.Value)
                    Next
                Next
            Next

            ' global settings
            BW.Write(Scenarios.GlobalSettings.isAutoSolve)
            BW.Write(Scenarios.GlobalSettings.Precision)
            BW.Write(Scenarios.GlobalSettings.ShowFrozenHeaders)
            BW.Write(CInt(Scenarios.GlobalSettings.SortBy))

            BW.Close()

            Return MS
        End Function

        Private Function GetRAStream_1_1_35() As MemoryStream
            Dim MS As New MemoryStream
            Dim BW As New BinaryWriter(MS)

            BW.Write(Scenarios.Scenarios.Count)
            BW.Write(Scenarios.ActiveScenarioID)

            For Each Scenario As RAScenario In Scenarios.Scenarios.Values
                BW.Write(Scenario.ID)
                BW.Write(Scenario.Name)
                BW.Write(Scenario.Description)
                BW.Write(Scenario.Index)
                BW.Write(Scenario.IsCheckedCS)
                BW.Write(Scenario.IsCheckedIB)
                BW.Write(Scenario.Budget)

                ' settings
                BW.Write(Scenario.Settings.BaseCaseForConstraints)
                BW.Write(Scenario.Settings.BaseCaseForDependencies)
                BW.Write(Scenario.Settings.BaseCaseForFundingPools)
                BW.Write(Scenario.Settings.BaseCaseForGroups)
                BW.Write(Scenario.Settings.BaseCaseForMustNots)
                BW.Write(Scenario.Settings.BaseCaseForMusts)
                BW.Write(Scenario.Settings.CustomConstraints)
                BW.Write(Scenario.Settings.Dependencies)
                BW.Write(Scenario.Settings.FundingPools)
                BW.Write(Scenario.Settings.Groups)
                BW.Write(Scenario.Settings.MustNots)
                BW.Write(Scenario.Settings.Musts)
                BW.Write(Scenario.Settings.Risks)
                BW.Write(Scenario.Settings.UseBaseCase)
                BW.Write(Scenario.Settings.UseBaseCaseOptions)
                BW.Write(Scenario.Settings.UseIgnoreOptions)

                ' alternatives
                BW.Write(Scenario.Alternatives.Count)
                For Each raAlt As RAAlternative In Scenario.Alternatives
                    BW.Write(raAlt.ID)
                    BW.Write(raAlt.Name)
                    BW.Write(raAlt.BenefitOriginal)
                    BW.Write(raAlt.Cost)
                    BW.Write(raAlt.IsPartial)
                    BW.Write(raAlt.MinPercent)
                    BW.Write(raAlt.Must)
                    BW.Write(raAlt.MustNot)
                    BW.Write(raAlt.RiskOriginal)
                    BW.Write(raAlt.SBPriority)
                    BW.Write(raAlt.SBTotal)
                    BW.Write(raAlt.SortOrder)
                Next

                ' groups
                BW.Write(Scenario.Groups.Groups.Values.Count)
                For Each group As RAGroup In Scenario.Groups.Groups.Values
                    BW.Write(group.ID)
                    BW.Write(group.IntID)
                    BW.Write(group.Name)
                    BW.Write(group.Enabled)
                    BW.Write(CInt(group.Condition))

                    BW.Write(group.Alternatives.Count)
                    For Each raAlt As RAAlternative In group.Alternatives.Values
                        BW.Write(raAlt.ID)
                    Next
                Next

                ' dependencies
                BW.Write(Scenario.Dependencies.Dependencies.Count)
                For Each dependency As RADependency In Scenario.Dependencies.Dependencies
                    BW.Write(dependency.FirstAlternativeID)
                    BW.Write(dependency.SecondAlternativeID)
                    BW.Write(CInt(dependency.Value))
                Next

                ' custom constraints
                BW.Write(Scenario.Constraints.Constraints.Count)
                For Each CC As RAConstraint In Scenario.Constraints.Constraints.Values
                    BW.Write(CC.ID)
                    BW.Write(CC.Name)
                    BW.Write(CC.Enabled)
                    BW.Write(CC.IsReadOnly)
                    BW.Write(CC.MaxValue)
                    BW.Write(CC.MinValue)
                    BW.Write(CC.LinkedAttributeID.ToByteArray)
                    BW.Write(CC.LinkedEnumID.ToByteArray)

                    ' new to 1.1.31
                    BW.Write(CC.ECD_AID)
                    BW.Write(CC.ECD_AssociatedCVID)
                    BW.Write(CC.ECD_AssociatedUDcolKey)
                    BW.Write(CC.ECD_CCID)
                    BW.Write(CC.ECD_SOrder)

                    BW.Write(CC.AlternativesData.Count)
                    For Each kvp As KeyValuePair(Of String, Double) In CC.AlternativesData
                        BW.Write(kvp.Key)
                        BW.Write(kvp.Value)
                    Next
                Next

                ' funding pools
                BW.Write(Scenario.FundingPools.Pools.Count)
                For Each FP As RAFundingPool In Scenario.FundingPools.Pools.Values
                    BW.Write(FP.ID)
                    BW.Write(FP.Name)
                    BW.Write(FP.Enabled)
                    BW.Write(FP.PoolLimit)

                    BW.Write(FP.Values.Count)
                    For Each kvp As KeyValuePair(Of String, Double) In FP.Values
                        BW.Write(kvp.Key)
                        BW.Write(kvp.Value)
                    Next
                Next

                ' Time periods
                Dim TimePeriodsStream As MemoryStream = Scenario.TimePeriods.CreateStream
                BW.Write(TimePeriodsStream.Length)
                BW.Write(TimePeriodsStream.ToArray)
            Next

            ' global settings
            BW.Write(Scenarios.GlobalSettings.isAutoSolve)
            BW.Write(Scenarios.GlobalSettings.Precision)
            BW.Write(Scenarios.GlobalSettings.ShowFrozenHeaders)
            BW.Write(CInt(Scenarios.GlobalSettings.SortBy))

            BW.Close()

            Return MS
        End Function

        Private Function GetRAStream_1_1_36() As MemoryStream
            Dim MS As New MemoryStream
            Dim BW As New BinaryWriter(MS)

            BW.Write(Scenarios.Scenarios.Count)
            BW.Write(Scenarios.ActiveScenarioID)

            For Each Scenario As RAScenario In Scenarios.Scenarios.Values
                BW.Write(Scenario.ID)
                BW.Write(Scenario.Name)
                BW.Write(Scenario.Description)
                BW.Write(Scenario.Index)
                BW.Write(Scenario.IsCheckedCS)
                BW.Write(Scenario.IsCheckedIB)
                BW.Write(Scenario.Budget)

                ' settings
                BW.Write(Scenario.Settings.BaseCaseForConstraints)
                BW.Write(Scenario.Settings.BaseCaseForDependencies)
                BW.Write(Scenario.Settings.BaseCaseForFundingPools)
                BW.Write(Scenario.Settings.BaseCaseForGroups)
                BW.Write(Scenario.Settings.BaseCaseForMustNots)
                BW.Write(Scenario.Settings.BaseCaseForMusts)
                BW.Write(Scenario.Settings.CustomConstraints)
                BW.Write(Scenario.Settings.Dependencies)
                BW.Write(Scenario.Settings.FundingPools)
                BW.Write(Scenario.Settings.Groups)
                BW.Write(Scenario.Settings.MustNots)
                BW.Write(Scenario.Settings.Musts)
                BW.Write(Scenario.Settings.Risks)
                BW.Write(Scenario.Settings.UseBaseCase)
                BW.Write(Scenario.Settings.UseBaseCaseOptions)
                BW.Write(Scenario.Settings.UseIgnoreOptions)

                ' alternatives
                BW.Write(Scenario.AlternativesFull.Count)
                For Each raAlt As RAAlternative In Scenario.AlternativesFull
                    BW.Write(raAlt.ID)
                    BW.Write(raAlt.Name)
                    BW.Write(raAlt.BenefitOriginal)
                    BW.Write(raAlt.Cost)
                    BW.Write(raAlt.IsPartial)
                    BW.Write(raAlt.MinPercent)
                    BW.Write(raAlt.Must)
                    BW.Write(raAlt.MustNot)
                    BW.Write(raAlt.RiskOriginal)
                    BW.Write(raAlt.SBPriority)
                    BW.Write(raAlt.SBTotal)
                    BW.Write(raAlt.SortOrder)
                    BW.Write(raAlt.Enabled)
                Next

                ' groups
                BW.Write(Scenario.Groups.Groups.Values.Count)
                For Each group As RAGroup In Scenario.Groups.Groups.Values
                    BW.Write(group.ID)
                    BW.Write(group.IntID)
                    BW.Write(group.Name)
                    BW.Write(group.Enabled)
                    BW.Write(CInt(group.Condition))

                    BW.Write(group.Alternatives.Count)
                    For Each raAlt As RAAlternative In group.Alternatives.Values
                        BW.Write(raAlt.ID)
                    Next
                Next

                ' dependencies
                BW.Write(Scenario.Dependencies.Dependencies.Count)
                For Each dependency As RADependency In Scenario.Dependencies.Dependencies
                    BW.Write(dependency.FirstAlternativeID)
                    BW.Write(dependency.SecondAlternativeID)
                    BW.Write(CInt(dependency.Value))
                Next

                ' custom constraints
                BW.Write(Scenario.Constraints.Constraints.Count)
                For Each CC As RAConstraint In Scenario.Constraints.Constraints.Values
                    BW.Write(CC.ID)
                    BW.Write(CC.Name)
                    BW.Write(CC.Enabled)
                    BW.Write(CC.IsReadOnly)
                    BW.Write(CC.MaxValue)
                    BW.Write(CC.MinValue)
                    BW.Write(CC.LinkedAttributeID.ToByteArray)
                    BW.Write(CC.LinkedEnumID.ToByteArray)

                    ' new to 1.1.31
                    BW.Write(CC.ECD_AID)
                    BW.Write(CC.ECD_AssociatedCVID)
                    BW.Write(CC.ECD_AssociatedUDcolKey)
                    BW.Write(CC.ECD_CCID)
                    BW.Write(CC.ECD_SOrder)

                    BW.Write(CC.AlternativesData.Count)
                    For Each kvp As KeyValuePair(Of String, Double) In CC.AlternativesData
                        BW.Write(kvp.Key)
                        BW.Write(kvp.Value)
                    Next
                Next

                ' funding pools
                BW.Write(Scenario.FundingPools.Pools.Count)
                For Each FP As RAFundingPool In Scenario.FundingPools.Pools.Values
                    BW.Write(FP.ID)
                    BW.Write(FP.Name)
                    BW.Write(FP.Enabled)
                    BW.Write(FP.PoolLimit)

                    BW.Write(FP.Values.Count)
                    For Each kvp As KeyValuePair(Of String, Double) In FP.Values
                        BW.Write(kvp.Key)
                        BW.Write(kvp.Value)
                    Next
                Next

                ' Time periods
                Dim TimePeriodsStream As MemoryStream = Scenario.TimePeriods.CreateStream
                BW.Write(TimePeriodsStream.Length)
                BW.Write(TimePeriodsStream.ToArray)
            Next

            ' global settings
            BW.Write(Scenarios.GlobalSettings.isAutoSolve)
            BW.Write(Scenarios.GlobalSettings.Precision)
            BW.Write(Scenarios.GlobalSettings.ShowFrozenHeaders)
            BW.Write(CInt(Scenarios.GlobalSettings.SortBy))

            BW.Close()

            Return MS
        End Function

        Private Function GetRAStream_1_1_37() As MemoryStream
            Dim MS As New MemoryStream
            Dim BW As New BinaryWriter(MS)

            BW.Write(Scenarios.Scenarios.Count)
            BW.Write(Scenarios.ActiveScenarioID)

            For Each Scenario As RAScenario In Scenarios.Scenarios.Values
                BW.Write(Scenario.ID)
                BW.Write(Scenario.Name)
                BW.Write(Scenario.Description)
                BW.Write(Scenario.Index)
                BW.Write(Scenario.IsCheckedCS)
                BW.Write(Scenario.IsCheckedIB)
                BW.Write(Scenario.Budget)

                ' settings
                BW.Write(Scenario.Settings.BaseCaseForConstraints)
                BW.Write(Scenario.Settings.BaseCaseForDependencies)
                BW.Write(Scenario.Settings.BaseCaseForFundingPools)
                BW.Write(Scenario.Settings.BaseCaseForGroups)
                BW.Write(Scenario.Settings.BaseCaseForMustNots)
                BW.Write(Scenario.Settings.BaseCaseForMusts)
                BW.Write(Scenario.Settings.CustomConstraints)
                BW.Write(Scenario.Settings.Dependencies)
                BW.Write(Scenario.Settings.FundingPools)
                BW.Write(Scenario.Settings.Groups)
                BW.Write(Scenario.Settings.MustNots)
                BW.Write(Scenario.Settings.Musts)
                BW.Write(Scenario.Settings.Risks)
                BW.Write(Scenario.Settings.UseBaseCase)
                BW.Write(Scenario.Settings.UseBaseCaseOptions)
                BW.Write(Scenario.Settings.UseIgnoreOptions)

                ' alternatives
                BW.Write(Scenario.AlternativesFull.Count)
                For Each raAlt As RAAlternative In Scenario.AlternativesFull
                    BW.Write(raAlt.ID)
                    BW.Write(raAlt.Name)
                    BW.Write(raAlt.BenefitOriginal)
                    BW.Write(raAlt.Cost)
                    BW.Write(raAlt.IsPartial)
                    BW.Write(raAlt.MinPercent)
                    BW.Write(raAlt.Must)
                    BW.Write(raAlt.MustNot)
                    BW.Write(raAlt.RiskOriginal)
                    BW.Write(raAlt.SBPriority)
                    BW.Write(raAlt.SBTotal)
                    BW.Write(raAlt.SortOrder)
                    BW.Write(raAlt.Enabled)
                Next

                ' groups
                BW.Write(Scenario.Groups.Groups.Values.Count)
                For Each group As RAGroup In Scenario.Groups.Groups.Values
                    BW.Write(group.ID)
                    BW.Write(group.IntID)
                    BW.Write(group.Name)
                    BW.Write(group.Enabled)
                    BW.Write(CInt(group.Condition))

                    BW.Write(group.Alternatives.Count)
                    For Each raAlt As RAAlternative In group.Alternatives.Values
                        BW.Write(raAlt.ID)
                    Next
                Next

                ' dependencies
                BW.Write(Scenario.Dependencies.Dependencies.Count)
                For Each dependency As RADependency In Scenario.Dependencies.Dependencies
                    BW.Write(dependency.FirstAlternativeID)
                    BW.Write(dependency.SecondAlternativeID)
                    BW.Write(CInt(dependency.Value))
                Next

                ' custom constraints
                BW.Write(Scenario.Constraints.Constraints.Count)
                For Each CC As RAConstraint In Scenario.Constraints.Constraints.Values
                    BW.Write(CC.ID)
                    BW.Write(CC.Name)
                    BW.Write(CC.Enabled)
                    BW.Write(CC.IsReadOnly)
                    BW.Write(CC.MaxValue)
                    BW.Write(CC.MinValue)
                    BW.Write(CC.LinkedAttributeID.ToByteArray)
                    BW.Write(CC.LinkedEnumID.ToByteArray)

                    ' new to 1.1.31
                    BW.Write(CC.ECD_AID)
                    BW.Write(CC.ECD_AssociatedCVID)
                    BW.Write(CC.ECD_AssociatedUDcolKey)
                    BW.Write(CC.ECD_CCID)
                    BW.Write(CC.ECD_SOrder)

                    BW.Write(CC.AlternativesData.Count)
                    For Each kvp As KeyValuePair(Of String, Double) In CC.AlternativesData
                        BW.Write(kvp.Key)
                        BW.Write(kvp.Value)
                    Next
                Next

                ' funding pools
                BW.Write(Scenario.FundingPools.Pools.Count)
                For Each FP As RAFundingPool In Scenario.FundingPools.Pools.Values
                    BW.Write(FP.ID)
                    BW.Write(FP.Name)
                    BW.Write(FP.Enabled)
                    BW.Write(FP.PoolLimit)

                    BW.Write(FP.Values.Count)
                    For Each kvp As KeyValuePair(Of String, Double) In FP.Values
                        BW.Write(kvp.Key)
                        BW.Write(kvp.Value)
                    Next
                Next

                ' Time periods
                Dim TimePeriodsStream As MemoryStream = Scenario.TimePeriods.CreateStream_v_37
                BW.Write(TimePeriodsStream.Length)
                BW.Write(TimePeriodsStream.ToArray)
            Next

            ' global settings
            BW.Write(Scenarios.GlobalSettings.isAutoSolve)
            BW.Write(Scenarios.GlobalSettings.Precision)
            BW.Write(Scenarios.GlobalSettings.ShowFrozenHeaders)
            BW.Write(CInt(Scenarios.GlobalSettings.SortBy))

            BW.Close()

            Return MS
        End Function

        Private Function GetRAStream_1_1_38() As MemoryStream
            Dim MS As New MemoryStream
            Dim BW As New BinaryWriter(MS)

            BW.Write(Scenarios.Scenarios.Count)
            BW.Write(Scenarios.ActiveScenarioID)

            For Each Scenario As RAScenario In Scenarios.Scenarios.Values
                BW.Write(Scenario.ID)
                BW.Write(Scenario.Name)
                BW.Write(Scenario.Description)
                BW.Write(Scenario.Index)
                BW.Write(Scenario.IsCheckedCS)
                BW.Write(Scenario.IsCheckedIB)
                BW.Write(Scenario.Budget)

                ' settings
                BW.Write(Scenario.Settings.BaseCaseForConstraints)
                BW.Write(Scenario.Settings.BaseCaseForDependencies)
                BW.Write(Scenario.Settings.BaseCaseForFundingPools)
                BW.Write(Scenario.Settings.BaseCaseForGroups)
                BW.Write(Scenario.Settings.BaseCaseForMustNots)
                BW.Write(Scenario.Settings.BaseCaseForMusts)
                BW.Write(Scenario.Settings.CustomConstraints)
                BW.Write(Scenario.Settings.Dependencies)
                BW.Write(Scenario.Settings.FundingPools)
                BW.Write(Scenario.Settings.Groups)
                BW.Write(Scenario.Settings.MustNots)
                BW.Write(Scenario.Settings.Musts)
                BW.Write(Scenario.Settings.Risks)
                BW.Write(Scenario.Settings.UseBaseCase)
                BW.Write(Scenario.Settings.UseBaseCaseOptions)
                BW.Write(Scenario.Settings.UseIgnoreOptions)
                BW.Write(Scenario.Settings.TimePeriods)

                ' alternatives
                BW.Write(Scenario.AlternativesFull.Count)
                For Each raAlt As RAAlternative In Scenario.AlternativesFull
                    BW.Write(raAlt.ID)
                    BW.Write(raAlt.Name)
                    BW.Write(raAlt.BenefitOriginal)
                    BW.Write(raAlt.Cost)
                    BW.Write(raAlt.IsPartial)
                    BW.Write(raAlt.MinPercent)
                    BW.Write(raAlt.Must)
                    BW.Write(raAlt.MustNot)
                    BW.Write(raAlt.RiskOriginal)
                    BW.Write(raAlt.SBPriority)
                    BW.Write(raAlt.SBTotal)
                    BW.Write(raAlt.SortOrder)
                    BW.Write(raAlt.Enabled)
                Next

                ' groups
                BW.Write(Scenario.Groups.Groups.Values.Count)
                For Each group As RAGroup In Scenario.Groups.Groups.Values
                    BW.Write(group.ID)
                    BW.Write(group.IntID)
                    BW.Write(group.Name)
                    BW.Write(group.Enabled)
                    BW.Write(CInt(group.Condition))

                    BW.Write(group.Alternatives.Count)
                    For Each raAlt As RAAlternative In group.Alternatives.Values
                        BW.Write(raAlt.ID)
                    Next
                Next

                ' dependencies
                BW.Write(Scenario.Dependencies.Dependencies.Count)
                For Each dependency As RADependency In Scenario.Dependencies.Dependencies
                    BW.Write(dependency.FirstAlternativeID)
                    BW.Write(dependency.SecondAlternativeID)
                    BW.Write(CInt(dependency.Value))
                Next

                ' custom constraints
                BW.Write(Scenario.Constraints.Constraints.Count)
                For Each CC As RAConstraint In Scenario.Constraints.Constraints.Values
                    BW.Write(CC.ID)
                    BW.Write(CC.Name)
                    BW.Write(CC.Enabled)
                    BW.Write(CC.IsReadOnly)
                    BW.Write(CC.MaxValue)
                    BW.Write(CC.MinValue)
                    BW.Write(CC.LinkedAttributeID.ToByteArray)
                    BW.Write(CC.LinkedEnumID.ToByteArray)

                    ' new to 1.1.31
                    BW.Write(CC.ECD_AID)
                    BW.Write(CC.ECD_AssociatedCVID)
                    BW.Write(CC.ECD_AssociatedUDcolKey)
                    BW.Write(CC.ECD_CCID)
                    BW.Write(CC.ECD_SOrder)

                    BW.Write(CC.AlternativesData.Count)
                    For Each kvp As KeyValuePair(Of String, Double) In CC.AlternativesData
                        BW.Write(kvp.Key)
                        BW.Write(kvp.Value)
                    Next
                Next

                ' funding pools
                BW.Write(Scenario.FundingPools.Pools.Count)
                For Each FP As RAFundingPool In Scenario.FundingPools.Pools.Values
                    BW.Write(FP.ID)
                    BW.Write(FP.Name)
                    BW.Write(FP.Enabled)
                    BW.Write(FP.PoolLimit)

                    BW.Write(FP.Values.Count)
                    For Each kvp As KeyValuePair(Of String, Double) In FP.Values
                        BW.Write(kvp.Key)
                        BW.Write(kvp.Value)
                    Next
                Next

                ' Time periods
                Dim TimePeriodsStream As MemoryStream = Scenario.TimePeriods.CreateStream_v_38
                BW.Write(TimePeriodsStream.Length)
                BW.Write(TimePeriodsStream.ToArray)
            Next

            ' global settings
            BW.Write(Scenarios.GlobalSettings.isAutoSolve)
            BW.Write(Scenarios.GlobalSettings.Precision)
            BW.Write(Scenarios.GlobalSettings.ShowFrozenHeaders)
            BW.Write(CInt(Scenarios.GlobalSettings.SortBy))

            BW.Close()

            Return MS
        End Function

        Private Function GetRAStream_1_1_39() As MemoryStream
            Dim MS As New MemoryStream
            Dim BW As New BinaryWriter(MS)

            BW.Write(Scenarios.Scenarios.Count)
            BW.Write(Scenarios.ActiveScenarioID)

            ' v1.1.39
            BW.Write(CombinedGroupUserID)

            For Each Scenario As RAScenario In Scenarios.Scenarios.Values
                BW.Write(Scenario.ID)
                BW.Write(Scenario.Name)
                BW.Write(Scenario.Description)
                BW.Write(Scenario.Index)
                BW.Write(Scenario.IsCheckedCS)
                BW.Write(Scenario.IsCheckedIB)
                BW.Write(Scenario.Budget)

                ' v.1.1.39
                BW.Write(Scenario.CombinedGroupUserID)

                ' settings
                BW.Write(Scenario.Settings.BaseCaseForConstraints)
                BW.Write(Scenario.Settings.BaseCaseForDependencies)
                BW.Write(Scenario.Settings.BaseCaseForFundingPools)
                BW.Write(Scenario.Settings.BaseCaseForGroups)
                BW.Write(Scenario.Settings.BaseCaseForMustNots)
                BW.Write(Scenario.Settings.BaseCaseForMusts)
                BW.Write(Scenario.Settings.CustomConstraints)
                BW.Write(Scenario.Settings.Dependencies)
                BW.Write(Scenario.Settings.FundingPools)
                BW.Write(Scenario.Settings.Groups)
                BW.Write(Scenario.Settings.MustNots)
                BW.Write(Scenario.Settings.Musts)
                BW.Write(Scenario.Settings.Risks)
                BW.Write(Scenario.Settings.UseBaseCase)
                BW.Write(Scenario.Settings.UseBaseCaseOptions)
                BW.Write(Scenario.Settings.UseIgnoreOptions)
                BW.Write(Scenario.Settings.TimePeriods)

                ' alternatives
                BW.Write(Scenario.AlternativesFull.Count)
                For Each raAlt As RAAlternative In Scenario.AlternativesFull
                    BW.Write(raAlt.ID)
                    BW.Write(raAlt.Name)
                    BW.Write(raAlt.BenefitOriginal)
                    BW.Write(raAlt.Cost)
                    BW.Write(raAlt.IsPartial)
                    BW.Write(raAlt.MinPercent)
                    BW.Write(raAlt.Must)
                    BW.Write(raAlt.MustNot)
                    BW.Write(raAlt.RiskOriginal)
                    BW.Write(raAlt.SBPriority)
                    BW.Write(raAlt.SBTotal)
                    BW.Write(raAlt.SortOrder)
                    BW.Write(raAlt.Enabled)
                Next

                ' groups
                BW.Write(Scenario.Groups.Groups.Values.Count)
                For Each group As RAGroup In Scenario.Groups.Groups.Values
                    BW.Write(group.ID)
                    BW.Write(group.IntID)
                    BW.Write(group.Name)
                    BW.Write(group.Enabled)
                    BW.Write(CInt(group.Condition))

                    BW.Write(group.Alternatives.Count)
                    For Each raAlt As RAAlternative In group.Alternatives.Values
                        BW.Write(raAlt.ID)
                    Next
                Next

                ' dependencies
                BW.Write(Scenario.Dependencies.Dependencies.Count + Scenario.TimePeriodsDependencies.Dependencies.Count)
                For Each dependency As RADependency In Scenario.Dependencies.Dependencies
                    BW.Write(dependency.FirstAlternativeID)
                    BW.Write(dependency.SecondAlternativeID)
                    BW.Write(CInt(dependency.Value))
                Next
                For Each dependency As RADependency In Scenario.TimePeriodsDependencies.Dependencies
                    BW.Write(dependency.FirstAlternativeID)
                    BW.Write(dependency.SecondAlternativeID)
                    BW.Write(CInt(dependency.Value))
                Next

                ' custom constraints
                BW.Write(Scenario.Constraints.Constraints.Count)
                For Each CC As RAConstraint In Scenario.Constraints.Constraints.Values
                    BW.Write(CC.ID)
                    BW.Write(CC.Name)
                    BW.Write(CC.Enabled)
                    BW.Write(CC.IsReadOnly)
                    BW.Write(CC.MaxValue)
                    BW.Write(CC.MinValue)
                    BW.Write(CC.LinkedAttributeID.ToByteArray)
                    BW.Write(CC.LinkedEnumID.ToByteArray)

                    ' new to 1.1.31
                    BW.Write(CC.ECD_AID)
                    BW.Write(CC.ECD_AssociatedCVID)
                    BW.Write(CC.ECD_AssociatedUDcolKey)
                    BW.Write(CC.ECD_CCID)
                    BW.Write(CC.ECD_SOrder)

                    BW.Write(CC.AlternativesData.Count)
                    For Each kvp As KeyValuePair(Of String, Double) In CC.AlternativesData
                        BW.Write(kvp.Key)
                        BW.Write(kvp.Value)
                    Next

                    ' v1.1.39
                    BW.Write(CC.IsLinkedToResource)
                Next

                ' funding pools
                BW.Write(Scenario.FundingPools.Pools.Count)
                For Each FP As RAFundingPool In Scenario.FundingPools.Pools.Values
                    BW.Write(FP.ID)
                    BW.Write(FP.Name)
                    BW.Write(FP.Enabled)
                    BW.Write(FP.PoolLimit)

                    BW.Write(FP.Values.Count)
                    For Each kvp As KeyValuePair(Of String, Double) In FP.Values
                        BW.Write(kvp.Key)
                        BW.Write(kvp.Value)
                    Next
                Next

                ' Time periods
                Dim TimePeriodsStream As MemoryStream = Scenario.TimePeriods.CreateStream_v_39
                BW.Write(TimePeriodsStream.Length)
                BW.Write(TimePeriodsStream.ToArray)
            Next

            ' global settings
            BW.Write(Scenarios.GlobalSettings.isAutoSolve)
            BW.Write(Scenarios.GlobalSettings.Precision)
            BW.Write(Scenarios.GlobalSettings.ShowFrozenHeaders)
            BW.Write(CInt(Scenarios.GlobalSettings.SortBy))

            BW.Close()

            Return MS
        End Function

        Private Function GetRAStream_1_1_40() As MemoryStream
            Dim MS As New MemoryStream
            Dim BW As New BinaryWriter(MS)

            BW.Write(Scenarios.Scenarios.Count)
            BW.Write(Scenarios.ActiveScenarioID)

            ' v1.1.39
            BW.Write(CombinedGroupUserID)

            For Each Scenario As RAScenario In Scenarios.Scenarios.Values
                BW.Write(Scenario.ID)
                BW.Write(Scenario.Name)
                BW.Write(Scenario.Description)
                BW.Write(Scenario.Index)
                BW.Write(Scenario.IsCheckedCS)
                BW.Write(Scenario.IsCheckedIB)
                BW.Write(Scenario.Budget)

                ' v.1.1.39
                BW.Write(Scenario.CombinedGroupUserID)

                ' settings
                BW.Write(Scenario.Settings.BaseCaseForConstraints)
                BW.Write(Scenario.Settings.BaseCaseForDependencies)
                BW.Write(Scenario.Settings.BaseCaseForFundingPools)
                BW.Write(Scenario.Settings.BaseCaseForGroups)
                BW.Write(Scenario.Settings.BaseCaseForMustNots)
                BW.Write(Scenario.Settings.BaseCaseForMusts)
                BW.Write(Scenario.Settings.CustomConstraints)
                BW.Write(Scenario.Settings.Dependencies)
                BW.Write(Scenario.Settings.FundingPools)
                BW.Write(Scenario.Settings.Groups)
                BW.Write(Scenario.Settings.MustNots)
                BW.Write(Scenario.Settings.Musts)
                BW.Write(Scenario.Settings.Risks)
                BW.Write(Scenario.Settings.UseBaseCase)
                BW.Write(Scenario.Settings.UseBaseCaseOptions)
                BW.Write(Scenario.Settings.UseIgnoreOptions)
                BW.Write(Scenario.Settings.TimePeriods)

                ' alternatives
                BW.Write(Scenario.AlternativesFull.Count)
                For Each raAlt As RAAlternative In Scenario.AlternativesFull
                    BW.Write(raAlt.ID)
                    BW.Write(raAlt.Name)
                    BW.Write(raAlt.BenefitOriginal)
                    BW.Write(raAlt.Cost)
                    BW.Write(raAlt.IsPartial)
                    BW.Write(raAlt.MinPercent)
                    BW.Write(raAlt.Must)
                    BW.Write(raAlt.MustNot)
                    BW.Write(raAlt.RiskOriginal)
                    BW.Write(raAlt.SBPriority)
                    BW.Write(raAlt.SBTotal)
                    BW.Write(raAlt.SortOrder)
                    BW.Write(raAlt.Enabled)
                Next

                ' groups
                BW.Write(Scenario.Groups.Groups.Values.Count)
                For Each group As RAGroup In Scenario.Groups.Groups.Values
                    BW.Write(group.ID)
                    BW.Write(group.IntID)
                    BW.Write(group.Name)
                    BW.Write(group.Enabled)
                    BW.Write(CInt(group.Condition))

                    BW.Write(group.Alternatives.Count)
                    For Each raAlt As RAAlternative In group.Alternatives.Values
                        BW.Write(raAlt.ID)
                    Next
                Next

                ' dependencies
                BW.Write(Scenario.Dependencies.Dependencies.Count + Scenario.TimePeriodsDependencies.Dependencies.Count)
                Dim dList As New List(Of RADependency)
                For Each d As RADependency In Scenario.Dependencies.Dependencies
                    dList.Add(d)
                Next
                For Each d As RADependency In Scenario.TimePeriodsDependencies.Dependencies
                    dList.Add(d)
                Next

                For Each dependency As RADependency In dList
                    BW.Write(dependency.FirstAlternativeID)
                    BW.Write(dependency.SecondAlternativeID)
                    BW.Write(CInt(dependency.Value))

                    ' new to 1.1.40
                    BW.Write(CInt(dependency.LagCondition))
                    BW.Write(dependency.Lag)
                    BW.Write(dependency.LagUpperBound)
                Next

                ' custom constraints
                BW.Write(Scenario.Constraints.Constraints.Count)
                For Each CC As RAConstraint In Scenario.Constraints.Constraints.Values
                    BW.Write(CC.ID)
                    BW.Write(CC.Name)
                    BW.Write(CC.Enabled)
                    BW.Write(CC.IsReadOnly)
                    BW.Write(CC.MaxValue)
                    BW.Write(CC.MinValue)
                    BW.Write(CC.LinkedAttributeID.ToByteArray)
                    BW.Write(CC.LinkedEnumID.ToByteArray)

                    ' new to 1.1.31
                    BW.Write(CC.ECD_AID)
                    BW.Write(CC.ECD_AssociatedCVID)
                    BW.Write(CC.ECD_AssociatedUDcolKey)
                    BW.Write(CC.ECD_CCID)
                    BW.Write(CC.ECD_SOrder)

                    BW.Write(CC.AlternativesData.Count)
                    For Each kvp As KeyValuePair(Of String, Double) In CC.AlternativesData
                        BW.Write(kvp.Key)
                        BW.Write(kvp.Value)
                    Next

                    ' v1.1.39
                    BW.Write(CC.IsLinkedToResource)
                Next

                ' funding pools
                BW.Write(Scenario.FundingPools.Pools.Count)
                For Each FP As RAFundingPool In Scenario.FundingPools.Pools.Values
                    BW.Write(FP.ID)
                    BW.Write(FP.Name)
                    BW.Write(FP.Enabled)
                    BW.Write(FP.PoolLimit)

                    BW.Write(FP.Values.Count)
                    For Each kvp As KeyValuePair(Of String, Double) In FP.Values
                        BW.Write(kvp.Key)
                        BW.Write(kvp.Value)
                    Next
                Next

                ' Time periods
                Dim TimePeriodsStream As MemoryStream = Scenario.TimePeriods.CreateStream_v_40
                BW.Write(TimePeriodsStream.Length)
                BW.Write(TimePeriodsStream.ToArray)
            Next

            ' global settings
            BW.Write(Scenarios.GlobalSettings.isAutoSolve)
            BW.Write(Scenarios.GlobalSettings.Precision)
            BW.Write(Scenarios.GlobalSettings.ShowFrozenHeaders)
            BW.Write(CInt(Scenarios.GlobalSettings.SortBy))

            BW.Close()

            Return MS
        End Function

        Private Function GetRAStream_1_1_43() As MemoryStream
            Dim MS As New MemoryStream
            Dim BW As New BinaryWriter(MS)

            BW.Write(Scenarios.Scenarios.Count)
            BW.Write(Scenarios.ActiveScenarioID)

            ' v1.1.39
            BW.Write(CombinedGroupUserID)

            For Each Scenario As RAScenario In Scenarios.Scenarios.Values
                BW.Write(Scenario.ID)
                BW.Write(Scenario.Name)
                BW.Write(Scenario.Description)
                BW.Write(Scenario.Index)
                BW.Write(Scenario.IsCheckedCS)
                BW.Write(Scenario.IsCheckedIB)
                BW.Write(Scenario.Budget)

                ' v.1.1.39
                BW.Write(Scenario.CombinedGroupUserID)

                ' settings
                BW.Write(Scenario.Settings.BaseCaseForConstraints)
                BW.Write(Scenario.Settings.BaseCaseForDependencies)
                BW.Write(Scenario.Settings.BaseCaseForFundingPools)
                BW.Write(Scenario.Settings.BaseCaseForGroups)
                BW.Write(Scenario.Settings.BaseCaseForMustNots)
                BW.Write(Scenario.Settings.BaseCaseForMusts)
                BW.Write(Scenario.Settings.CustomConstraints)
                BW.Write(Scenario.Settings.Dependencies)
                BW.Write(Scenario.Settings.FundingPools)
                BW.Write(Scenario.Settings.Groups)
                BW.Write(Scenario.Settings.MustNots)
                BW.Write(Scenario.Settings.Musts)
                BW.Write(Scenario.Settings.Risks)
                BW.Write(Scenario.Settings.UseBaseCase)
                BW.Write(Scenario.Settings.UseBaseCaseOptions)
                BW.Write(Scenario.Settings.UseIgnoreOptions)
                BW.Write(Scenario.Settings.TimePeriods)

                ' alternatives
                BW.Write(Scenario.AlternativesFull.Count)
                For Each raAlt As RAAlternative In Scenario.AlternativesFull
                    BW.Write(raAlt.ID)
                    BW.Write(raAlt.Name)
                    BW.Write(raAlt.BenefitOriginal)
                    BW.Write(raAlt.Cost)
                    BW.Write(raAlt.IsPartial)
                    BW.Write(raAlt.MinPercent)
                    BW.Write(raAlt.Must)
                    BW.Write(raAlt.MustNot)
                    BW.Write(raAlt.RiskOriginal)
                    BW.Write(raAlt.SBPriority)
                    BW.Write(raAlt.SBTotal)
                    BW.Write(raAlt.SortOrder)
                    BW.Write(raAlt.Enabled)
                Next

                ' groups
                BW.Write(Scenario.Groups.Groups.Values.Count)
                For Each group As RAGroup In Scenario.Groups.Groups.Values
                    BW.Write(group.ID)
                    BW.Write(group.IntID)
                    BW.Write(group.Name)
                    BW.Write(group.Enabled)
                    BW.Write(CInt(group.Condition))

                    BW.Write(group.Alternatives.Count)
                    For Each raAlt As RAAlternative In group.Alternatives.Values
                        BW.Write(raAlt.ID)
                    Next
                Next

                BW.Write(Scenario.EventGroups.Groups.Values.Count)
                For Each group As RAGroup In Scenario.EventGroups.Groups.Values
                    BW.Write(group.ID)
                    BW.Write(group.IntID)
                    BW.Write(group.Name)
                    BW.Write(group.Enabled)
                    BW.Write(CInt(group.Condition))

                    BW.Write(group.Alternatives.Count)
                    For Each raAlt As RAAlternative In group.Alternatives.Values
                        BW.Write(raAlt.ID)
                    Next
                Next

                ' dependencies
                BW.Write(Scenario.Dependencies.Dependencies.Count + Scenario.TimePeriodsDependencies.Dependencies.Count)
                Dim dList As New List(Of RADependency)
                For Each d As RADependency In Scenario.Dependencies.Dependencies
                    dList.Add(d)
                Next
                For Each d As RADependency In Scenario.TimePeriodsDependencies.Dependencies
                    dList.Add(d)
                Next

                For Each dependency As RADependency In dList
                    BW.Write(dependency.FirstAlternativeID)
                    BW.Write(dependency.SecondAlternativeID)
                    BW.Write(CInt(dependency.Value))

                    ' new to 1.1.40
                    BW.Write(CInt(dependency.LagCondition))
                    BW.Write(dependency.Lag)
                    BW.Write(dependency.LagUpperBound)
                Next

                ' custom constraints
                BW.Write(Scenario.Constraints.Constraints.Count)
                For Each CC As RAConstraint In Scenario.Constraints.Constraints.Values
                    BW.Write(CC.ID)
                    BW.Write(CC.Name)
                    BW.Write(CC.Enabled)
                    BW.Write(CC.IsReadOnly)
                    BW.Write(CC.MaxValue)
                    BW.Write(CC.MinValue)
                    BW.Write(CC.LinkedAttributeID.ToByteArray)
                    BW.Write(CC.LinkedEnumID.ToByteArray)

                    ' new to 1.1.31
                    BW.Write(CC.ECD_AID)
                    BW.Write(CC.ECD_AssociatedCVID)
                    BW.Write(CC.ECD_AssociatedUDcolKey)
                    BW.Write(CC.ECD_CCID)
                    BW.Write(CC.ECD_SOrder)

                    BW.Write(CC.AlternativesData.Count)
                    For Each kvp As KeyValuePair(Of String, Double) In CC.AlternativesData
                        BW.Write(kvp.Key)
                        BW.Write(kvp.Value)
                    Next

                    ' v1.1.39
                    BW.Write(CC.IsLinkedToResource)
                Next

                ' funding pools
                BW.Write(Scenario.FundingPools.Pools.Count)
                For Each FP As RAFundingPool In Scenario.FundingPools.Pools.Values
                    BW.Write(FP.ID)
                    BW.Write(FP.Name)
                    BW.Write(FP.Enabled)
                    BW.Write(FP.PoolLimit)

                    BW.Write(FP.Values.Count)
                    For Each kvp As KeyValuePair(Of String, Double) In FP.Values
                        BW.Write(kvp.Key)
                        BW.Write(kvp.Value)
                    Next
                Next

                ' Time periods
                Dim TimePeriodsStream As MemoryStream = Scenario.TimePeriods.CreateStream_v_40
                BW.Write(TimePeriodsStream.Length)
                BW.Write(TimePeriodsStream.ToArray)
            Next

            ' global settings
            BW.Write(Scenarios.GlobalSettings.isAutoSolve)
            BW.Write(Scenarios.GlobalSettings.Precision)
            BW.Write(Scenarios.GlobalSettings.ShowFrozenHeaders)
            BW.Write(CInt(Scenarios.GlobalSettings.SortBy))

            BW.Close()

            Return MS
        End Function

        Private Function GetRAStream_1_1_45() As MemoryStream
            Dim MS As New MemoryStream
            Dim BW As New BinaryWriter(MS)

            ' D4909 ===
            'BW.Write(Scenarios.Scenarios.Count)
            Dim Cnt As Integer = Scenarios.Scenarios.Sum(Function(S) If(S.Value.IsInfeasibilityAnalysis, 0, 1))
            BW.Write(Cnt)
            ' D4909 ==
            BW.Write(Scenarios.ActiveScenarioID)

            ' v1.1.39
            BW.Write(CombinedGroupUserID)

            For Each Scenario As RAScenario In Scenarios.Scenarios.Values
                If Not Scenario.IsInfeasibilityAnalysis Then ' D4909
                    BW.Write(Scenario.ID)
                    BW.Write(Scenario.Name)
                    BW.Write(Scenario.Description)
                    BW.Write(Scenario.Index)
                    BW.Write(Scenario.IsCheckedCS)
                    BW.Write(Scenario.IsCheckedIB)
                    BW.Write(Scenario.Budget)

                    ' v.1.1.39
                    BW.Write(Scenario.CombinedGroupUserID)

                    ' settings
                    BW.Write(Scenario.Settings.BaseCaseForConstraints)
                    BW.Write(Scenario.Settings.BaseCaseForDependencies)
                    BW.Write(Scenario.Settings.BaseCaseForFundingPools)
                    BW.Write(Scenario.Settings.BaseCaseForGroups)
                    BW.Write(Scenario.Settings.BaseCaseForMustNots)
                    BW.Write(Scenario.Settings.BaseCaseForMusts)
                    BW.Write(Scenario.Settings.CustomConstraints)
                    BW.Write(Scenario.Settings.Dependencies)
                    BW.Write(Scenario.Settings.FundingPools)
                    BW.Write(Scenario.Settings.Groups)
                    BW.Write(Scenario.Settings.MustNots)
                    BW.Write(Scenario.Settings.Musts)
                    BW.Write(Scenario.Settings.Risks)
                    BW.Write(Scenario.Settings.UseBaseCase)
                    BW.Write(Scenario.Settings.UseBaseCaseOptions)
                    BW.Write(Scenario.Settings.UseIgnoreOptions)
                    BW.Write(Scenario.Settings.TimePeriods)

                    ' new to 1.1.45
                    BW.Write(Scenario.Settings.ResourcesMin)
                    BW.Write(Scenario.Settings.ResourcesMax)

                    ' alternatives
                    BW.Write(Scenario.AlternativesFull.Count)
                    For Each raAlt As RAAlternative In Scenario.AlternativesFull
                        BW.Write(raAlt.ID)
                        BW.Write(raAlt.Name)
                        BW.Write(raAlt.BenefitOriginal)
                        BW.Write(raAlt.Cost)
                        BW.Write(raAlt.IsPartial)
                        BW.Write(raAlt.MinPercent)
                        BW.Write(raAlt.Must)
                        BW.Write(raAlt.MustNot)
                        BW.Write(raAlt.RiskOriginal)
                        BW.Write(raAlt.SBPriority)
                        BW.Write(raAlt.SBTotal)
                        BW.Write(raAlt.SortOrder)
                        BW.Write(raAlt.Enabled)
                    Next

                    ' groups
                    BW.Write(Scenario.Groups.Groups.Values.Count)
                    For Each group As RAGroup In Scenario.Groups.Groups.Values
                        BW.Write(group.ID)
                        BW.Write(group.IntID)
                        BW.Write(group.Name)
                        BW.Write(group.Enabled)
                        BW.Write(CInt(group.Condition))

                        BW.Write(group.Alternatives.Count)
                        For Each raAlt As RAAlternative In group.Alternatives.Values
                            BW.Write(raAlt.ID)
                        Next
                    Next

                    BW.Write(Scenario.EventGroups.Groups.Values.Count)
                    For Each group As RAGroup In Scenario.EventGroups.Groups.Values
                        BW.Write(group.ID)
                        BW.Write(group.IntID)
                        BW.Write(group.Name)
                        BW.Write(group.Enabled)
                        BW.Write(CInt(group.Condition))

                        BW.Write(group.Alternatives.Count)
                        For Each raAlt As RAAlternative In group.Alternatives.Values
                            BW.Write(raAlt.ID)
                        Next
                    Next

                    ' dependencies
                    BW.Write(Scenario.Dependencies.Dependencies.Count + Scenario.TimePeriodsDependencies.Dependencies.Count)
                    Dim dList As New List(Of RADependency)
                    For Each d As RADependency In Scenario.Dependencies.Dependencies
                        dList.Add(d)
                    Next
                    For Each d As RADependency In Scenario.TimePeriodsDependencies.Dependencies
                        dList.Add(d)
                    Next

                    For Each dependency As RADependency In dList
                        BW.Write(dependency.FirstAlternativeID)
                        BW.Write(dependency.SecondAlternativeID)
                        BW.Write(CInt(dependency.Value))

                        ' new to 1.1.40
                        BW.Write(CInt(dependency.LagCondition))
                        BW.Write(dependency.Lag)
                        BW.Write(dependency.LagUpperBound)
                    Next

                    ' custom constraints
                    BW.Write(Scenario.Constraints.Constraints.Count)
                    For Each CC As RAConstraint In Scenario.Constraints.Constraints.Values
                        BW.Write(CC.ID)
                        BW.Write(CC.Name)
                        BW.Write(CC.Enabled)
                        BW.Write(CC.IsReadOnly)
                        BW.Write(CC.MaxValue)
                        BW.Write(CC.MinValue)
                        BW.Write(CC.LinkedAttributeID.ToByteArray)
                        BW.Write(CC.LinkedEnumID.ToByteArray)

                        ' new to 1.1.31
                        BW.Write(CC.ECD_AID)
                        BW.Write(CC.ECD_AssociatedCVID)
                        BW.Write(CC.ECD_AssociatedUDcolKey)
                        BW.Write(CC.ECD_CCID)
                        BW.Write(CC.ECD_SOrder)

                        BW.Write(CC.AlternativesData.Count)
                        For Each kvp As KeyValuePair(Of String, Double) In CC.AlternativesData
                            BW.Write(kvp.Key)
                            BW.Write(kvp.Value)
                        Next

                        ' v1.1.39
                        BW.Write(CC.IsLinkedToResource)

                        ' new to 1.1.45
                        BW.Write(CC.LinkSourceID.ToByteArray)
                        BW.Write(CC.LinkSourceMode)
                    Next

                    ' funding pools
                    BW.Write(Scenario.FundingPools.Pools.Count)
                    For Each FP As RAFundingPool In Scenario.FundingPools.Pools.Values
                        BW.Write(FP.ID)
                        BW.Write(FP.Name)
                        BW.Write(FP.Enabled)
                        BW.Write(FP.PoolLimit)

                        BW.Write(FP.Values.Count)
                        For Each kvp As KeyValuePair(Of String, Double) In FP.Values
                            BW.Write(kvp.Key)
                            BW.Write(kvp.Value)
                        Next
                    Next

                    ' Time periods
                    Dim TimePeriodsStream As MemoryStream = Scenario.TimePeriods.CreateStream_v_40
                    BW.Write(TimePeriodsStream.Length)
                    BW.Write(TimePeriodsStream.ToArray)
                End If
            Next

            ' global settings
            BW.Write(Scenarios.GlobalSettings.isAutoSolve)
            BW.Write(Scenarios.GlobalSettings.Precision)
            BW.Write(Scenarios.GlobalSettings.ShowFrozenHeaders)
            BW.Write(CInt(Scenarios.GlobalSettings.SortBy))

            BW.Close()

            Return MS
        End Function

        Private Function GetRAStream_1_1_46() As MemoryStream
            Dim MS As New MemoryStream
            Dim BW As New BinaryWriter(MS)

            ' D4909 ===
            'BW.Write(Scenarios.Scenarios.Count)
            Dim Cnt As Integer = Scenarios.Scenarios.Sum(Function(S) If(S.Value.IsInfeasibilityAnalysis, 0, 1))
            BW.Write(Cnt)
            ' D4909 ==
            BW.Write(Scenarios.ActiveScenarioID)

            ' v1.1.39
            BW.Write(CombinedGroupUserID)

            For Each Scenario As RAScenario In Scenarios.Scenarios.Values
                If Not Scenario.IsInfeasibilityAnalysis Then ' D4909
                    BW.Write(Scenario.ID)
                    BW.Write(Scenario.Name)
                    BW.Write(Scenario.Description)
                    BW.Write(Scenario.Index)
                    BW.Write(Scenario.IsCheckedCS)
                    BW.Write(Scenario.IsCheckedIB)
                    BW.Write(Scenario.Budget)

                    ' v.1.1.39
                    BW.Write(Scenario.CombinedGroupUserID)

                    ' settings
                    BW.Write(Scenario.Settings.BaseCaseForConstraints)
                    BW.Write(Scenario.Settings.BaseCaseForDependencies)
                    BW.Write(Scenario.Settings.BaseCaseForFundingPools)
                    BW.Write(Scenario.Settings.BaseCaseForGroups)
                    BW.Write(Scenario.Settings.BaseCaseForMustNots)
                    BW.Write(Scenario.Settings.BaseCaseForMusts)
                    BW.Write(Scenario.Settings.CustomConstraints)
                    BW.Write(Scenario.Settings.Dependencies)
                    BW.Write(Scenario.Settings.FundingPools)
                    BW.Write(Scenario.Settings.Groups)
                    BW.Write(Scenario.Settings.MustNots)
                    BW.Write(Scenario.Settings.Musts)
                    BW.Write(Scenario.Settings.Risks)
                    BW.Write(Scenario.Settings.UseBaseCase)
                    BW.Write(Scenario.Settings.UseBaseCaseOptions)
                    BW.Write(Scenario.Settings.UseIgnoreOptions)
                    BW.Write(Scenario.Settings.TimePeriods)

                    ' new to 1.1.45
                    BW.Write(Scenario.Settings.ResourcesMin)
                    BW.Write(Scenario.Settings.ResourcesMax)

                    ' new to 1.1.46
                    BW.Write(Scenario.Settings.CostTolerance)

                    ' alternatives
                    BW.Write(Scenario.AlternativesFull.Count)
                    For Each raAlt As RAAlternative In Scenario.AlternativesFull
                        BW.Write(raAlt.ID)
                        BW.Write(raAlt.Name)
                        BW.Write(raAlt.BenefitOriginal)
                        BW.Write(raAlt.Cost)
                        BW.Write(raAlt.IsPartial)
                        BW.Write(raAlt.MinPercent)
                        BW.Write(raAlt.Must)
                        BW.Write(raAlt.MustNot)
                        BW.Write(raAlt.RiskOriginal)
                        BW.Write(raAlt.SBPriority)
                        BW.Write(raAlt.SBTotal)
                        BW.Write(raAlt.SortOrder)
                        BW.Write(raAlt.Enabled)
                    Next

                    ' groups
                    BW.Write(Scenario.Groups.Groups.Values.Count)
                    For Each group As RAGroup In Scenario.Groups.Groups.Values
                        BW.Write(group.ID)
                        BW.Write(group.IntID)
                        BW.Write(group.Name)
                        BW.Write(group.Enabled)
                        BW.Write(CInt(group.Condition))

                        BW.Write(group.Alternatives.Count)
                        For Each raAlt As RAAlternative In group.Alternatives.Values
                            BW.Write(raAlt.ID)
                        Next
                    Next

                    BW.Write(Scenario.EventGroups.Groups.Values.Count)
                    For Each group As RAGroup In Scenario.EventGroups.Groups.Values
                        BW.Write(group.ID)
                        BW.Write(group.IntID)
                        BW.Write(group.Name)
                        BW.Write(group.Enabled)
                        BW.Write(CInt(group.Condition))

                        BW.Write(group.Alternatives.Count)
                        For Each raAlt As RAAlternative In group.Alternatives.Values
                            BW.Write(raAlt.ID)
                        Next
                    Next

                    ' dependencies
                    BW.Write(Scenario.Dependencies.Dependencies.Count + Scenario.TimePeriodsDependencies.Dependencies.Count)
                    Dim dList As New List(Of RADependency)
                    For Each d As RADependency In Scenario.Dependencies.Dependencies
                        dList.Add(d)
                    Next
                    For Each d As RADependency In Scenario.TimePeriodsDependencies.Dependencies
                        dList.Add(d)
                    Next

                    For Each dependency As RADependency In dList
                        BW.Write(dependency.FirstAlternativeID)
                        BW.Write(dependency.SecondAlternativeID)
                        BW.Write(CInt(dependency.Value))

                        ' new to 1.1.40
                        BW.Write(CInt(dependency.LagCondition))
                        BW.Write(dependency.Lag)
                        BW.Write(dependency.LagUpperBound)
                    Next

                    ' custom constraints
                    BW.Write(Scenario.Constraints.Constraints.Count)
                    For Each CC As RAConstraint In Scenario.Constraints.Constraints.Values
                        BW.Write(CC.ID)
                        BW.Write(CC.Name)
                        BW.Write(CC.Enabled)
                        BW.Write(CC.IsReadOnly)
                        BW.Write(CC.MaxValue)
                        BW.Write(CC.MinValue)
                        BW.Write(CC.LinkedAttributeID.ToByteArray)
                        BW.Write(CC.LinkedEnumID.ToByteArray)

                        ' new to 1.1.31
                        BW.Write(CC.ECD_AID)
                        BW.Write(CC.ECD_AssociatedCVID)
                        BW.Write(CC.ECD_AssociatedUDcolKey)
                        BW.Write(CC.ECD_CCID)
                        BW.Write(CC.ECD_SOrder)

                        BW.Write(CC.AlternativesData.Count)
                        For Each kvp As KeyValuePair(Of String, Double) In CC.AlternativesData
                            BW.Write(kvp.Key)
                            BW.Write(kvp.Value)
                        Next

                        ' v1.1.39
                        BW.Write(CC.IsLinkedToResource)

                        ' new to 1.1.45
                        BW.Write(CC.LinkSourceID.ToByteArray)
                        BW.Write(CC.LinkSourceMode)
                    Next

                    ' funding pools
                    BW.Write(Scenario.FundingPools.Pools.Count)
                    Scenario.FundingPools.ResourceID = RA_Cost_GUID
                    For Each FP As RAFundingPool In Scenario.FundingPools.Pools.Values
                        BW.Write(FP.ID)
                        BW.Write(FP.Name)
                        BW.Write(FP.Enabled)
                        BW.Write(FP.PoolLimit)

                        BW.Write(FP.Values.Count)
                        For Each kvp As KeyValuePair(Of String, Double) In FP.Values
                            BW.Write(kvp.Key)
                            BW.Write(kvp.Value)
                        Next
                    Next

                    ' Time periods
                    Dim TimePeriodsStream As MemoryStream = Scenario.TimePeriods.CreateStream_v_40
                    BW.Write(TimePeriodsStream.Length)
                    BW.Write(TimePeriodsStream.ToArray)
                End If
            Next

            ' global settings
            BW.Write(Scenarios.GlobalSettings.isAutoSolve)
            BW.Write(Scenarios.GlobalSettings.Precision)
            BW.Write(Scenarios.GlobalSettings.ShowFrozenHeaders)
            BW.Write(CInt(Scenarios.GlobalSettings.SortBy))

            ' write resource pools
            BW.Write(UNDEFINED_INTEGER_VALUE)
            BW.Write(Cnt)
            For Each Scenario As RAScenario In Scenarios.Scenarios.Values
                If Not Scenario.IsInfeasibilityAnalysis Then
                    BW.Write(Scenario.ID)
                    BW.Write(Scenario.ResourcePools.Count)
                    For Each FPools As RAFundingPools In Scenario.ResourcePools.Values
                        BW.Write(FPools.ResourceID.ToByteArray)
                        BW.Write(FPools.Pools.Count)
                        For Each FP As RAFundingPool In FPools.Pools.Values
                            BW.Write(FP.ID)
                            BW.Write(FP.Name)
                            BW.Write(FP.Enabled)
                            BW.Write(FP.PoolLimit)

                            BW.Write(FP.Values.Count)
                            For Each kvp As KeyValuePair(Of String, Double) In FP.Values
                                BW.Write(kvp.Key)
                                BW.Write(kvp.Value)
                            Next

                            BW.Write(FP.Priorities.Count)
                            For Each kvp As KeyValuePair(Of String, Double) In FP.Priorities
                                BW.Write(kvp.Key)
                                BW.Write(kvp.Value)
                            Next
                        Next
                    Next
                End If
            Next



            '' new funding pools
            'BW.Write(Cnt)

            'For Each Scenario As RAScenario In Scenarios.Scenarios.Values
            '    If Not Scenario.IsInfeasibilityAnalysis Then
            '        BW.Write(Scenario.ID)
            '        BW.Write(Scenario.FundingPools.Pools.Count)
            '        For Each FP As RAFundingPool In Scenario.FundingPools.Pools.Values
            '            BW.Write(FP.ID)
            '            BW.Write(FP.Name)
            '            BW.Write(FP.Enabled)
            '            BW.Write(FP.PoolLimit)

            '            BW.Write(FP.Values.Count)
            '            For Each kvp As KeyValuePair(Of String, Double) In FP.Values
            '                BW.Write(kvp.Key)
            '                BW.Write(kvp.Value)
            '            Next

            '            BW.Write(FP.Priorities.Count)
            '            For Each kvp As KeyValuePair(Of String, Double) In FP.Priorities
            '                BW.Write(kvp.Key)
            '                BW.Write(kvp.Value)
            '            Next
            '        Next
            '    End If
            'Next

            BW.Close()

            Return MS
        End Function

        Private Function GetRAStream() As MemoryStream
            If ProjectManager.StorageManager.CanvasDBVersion.MinorVersion <= 30 Then
                Return GetRAStream_1_1_30()
            End If

            Select Case ProjectManager.StorageManager.CanvasDBVersion.MinorVersion
                Case 31 To 34
                    Return GetRAStream_1_1_31()
                Case 35
                    Return GetRAStream_1_1_35()
                Case 36
                    Return GetRAStream_1_1_36()
                Case 37
                    Return GetRAStream_1_1_37()
                Case 38
                    Return GetRAStream_1_1_38()
                Case 39
                    Return GetRAStream_1_1_39()
                Case 40 To 42
                    Return GetRAStream_1_1_40()
                Case 43 To 44
                    Return GetRAStream_1_1_43()
                Case 45
                    Return GetRAStream_1_1_45()
                Case 46 To _DB_VERSION_MINOR
                    Return GetRAStream_1_1_46()
                Case Else
                    Return Nothing
            End Select
        End Function

        Private Function ParseRAStream_1_1_30(ByVal Stream As MemoryStream) As Boolean
            Stream.Seek(0, SeekOrigin.Begin)

            Dim BR As New BinaryReader(Stream)

            Scenarios.Clear()

            Dim ScenariosCount As Integer = BR.ReadInt32
            Dim ActiveScenarioID As Integer = BR.ReadInt32

            Dim fHasIDs As Boolean = True   ' D3652

            For i As Integer = 1 To ScenariosCount
                Dim scenario As New RAScenario(Scenarios)

                scenario.ID = BR.ReadInt32
                scenario.Name = BR.ReadString
                scenario.Description = BR.ReadString
                scenario.Index = BR.ReadInt32
                scenario.IsCheckedCS = BR.ReadBoolean
                scenario.IsCheckedIB = BR.ReadBoolean
                scenario.Budget = BR.ReadDouble

                ' settings
                scenario.Settings.BaseCaseForConstraints = BR.ReadBoolean
                scenario.Settings.BaseCaseForDependencies = BR.ReadBoolean
                scenario.Settings.BaseCaseForFundingPools = BR.ReadBoolean
                scenario.Settings.BaseCaseForGroups = BR.ReadBoolean
                scenario.Settings.BaseCaseForMustNots = BR.ReadBoolean
                scenario.Settings.BaseCaseForMusts = BR.ReadBoolean

                scenario.Settings.CustomConstraints = BR.ReadBoolean
                scenario.Settings.Dependencies = BR.ReadBoolean
                scenario.Settings.FundingPools = BR.ReadBoolean
                scenario.Settings.Groups = BR.ReadBoolean
                scenario.Settings.MustNots = BR.ReadBoolean
                scenario.Settings.Musts = BR.ReadBoolean
                scenario.Settings.Risks = BR.ReadBoolean

                scenario.Settings.UseBaseCase = BR.ReadBoolean
                scenario.Settings.UseBaseCaseOptions = BR.ReadBoolean
                scenario.Settings.UseIgnoreOptions = BR.ReadBoolean


                ' alternatives
                Dim altsCount As Integer = BR.ReadInt32
                For j As Integer = 1 To altsCount
                    Dim raAlt As New RAAlternative
                    raAlt.ID = BR.ReadString
                    raAlt.Name = BR.ReadString
                    raAlt.BenefitOriginal = BR.ReadDouble
                    raAlt.Cost = BR.ReadDouble
                    raAlt.IsPartial = BR.ReadBoolean
                    raAlt.MinPercent = BR.ReadDouble
                    raAlt.Must = BR.ReadBoolean
                    raAlt.MustNot = BR.ReadBoolean
                    raAlt.RiskOriginal = BR.ReadDouble
                    raAlt.SBPriority = BR.ReadDouble
                    raAlt.SBTotal = BR.ReadDouble
                    raAlt.SortOrder = BR.ReadInt32

                    scenario.AlternativesFull.Add(raAlt)
                Next

                ' groups
                Dim groupsCount As Integer = BR.ReadInt32
                For j As Integer = 1 To groupsCount
                    Dim group As New RAGroup
                    group.ID = BR.ReadString
                    group.IntID = BR.ReadInt32
                    group.Name = BR.ReadString
                    group.Enabled = BR.ReadBoolean
                    group.Condition = CType(BR.ReadInt32, RAGroupCondition)

                    Dim aCount As Integer = BR.ReadInt32
                    For k As Integer = 1 To aCount
                        Dim altID As String = BR.ReadString
                        Dim alt As RAAlternative = scenario.Alternatives.Find(Function(a) (a.ID.ToLower = altID.ToLower))
                        If alt IsNot Nothing Then
                            group.Alternatives.Add(alt.ID, alt)
                        End If
                    Next

                    scenario.Groups.Groups.Add(group.ID, group)
                Next

                ' dependencies
                Dim dCount As Integer = BR.ReadInt32
                For j As Integer = 1 To dCount
                    Dim FirstAlternativeID As String = BR.ReadString
                    Dim SecondAlternativeID As String = BR.ReadString
                    Dim Value As RADependencyType = CType(BR.ReadInt32, RADependencyType)
                    Dim dependency As New RADependency(FirstAlternativeID, SecondAlternativeID, Value)

                    scenario.Dependencies.Dependencies.Add(dependency)
                Next

                ' custom constraints
                Dim ccCount As Integer = BR.ReadInt32
                For j As Integer = 1 To ccCount
                    Dim CC As New RAConstraint
                    CC.ID = BR.ReadInt32
                    CC.Name = BR.ReadString
                    CC.Enabled = BR.ReadBoolean
                    CC.IsReadOnly = BR.ReadBoolean
                    CC.MaxValue = BR.ReadDouble
                    CC.MinValue = BR.ReadDouble
                    CC.LinkedAttributeID = New Guid(BR.ReadBytes(16))
                    CC.LinkedEnumID = New Guid(BR.ReadBytes(16))

                    Dim adCount As Integer = BR.ReadInt32
                    For k As Integer = 1 To adCount
                        Dim aID As String = BR.ReadString
                        Dim aValue As Double = BR.ReadDouble
                        CC.AlternativesData.Add(aID, aValue)
                    Next

                    scenario.Constraints.Constraints.Add(CC.ID, CC)
                Next

                ' funding pools
                Dim fpCount As Integer = BR.ReadInt32
                For j As Integer = 1 To fpCount
                    Dim FP As New RAFundingPool
                    FP.ID = BR.ReadInt32
                    FP.Name = BR.ReadString
                    FP.Enabled = BR.ReadBoolean
                    FP.PoolLimit = BR.ReadDouble

                    Dim vCount As Integer = BR.ReadInt32
                    For k As Integer = 1 To vCount
                        Dim aID As String = BR.ReadString
                        Dim aValue As Double = BR.ReadDouble
                        FP.Values.Add(aID, aValue)
                    Next

                    scenario.FundingPools.Pools.Add(FP.ID, FP)
                Next

                ' D3652 ===
                If fHasIDs AndAlso scenario.ID = 0 AndAlso Scenarios.Scenarios.Keys.Contains(scenario.ID) Then
                    fHasIDs = False
                End If
                If Not fHasIDs AndAlso scenario.ID = 0 Then
                    scenario.ID = i - 1
                    If scenario.Index = 0 Then scenario.Index = i
                End If
                ' D3652 ==

                Scenarios.Scenarios.Add(scenario.ID, scenario)
            Next

            ' global settings
            Scenarios.GlobalSettings.isAutoSolve = BR.ReadBoolean
            Scenarios.GlobalSettings.Precision = BR.ReadInt32
            Scenarios.GlobalSettings.ShowFrozenHeaders = BR.ReadBoolean
            Scenarios.GlobalSettings.SortBy = CType(BR.ReadInt32, RAGlobalSettings.raColumnID)

            Scenarios.ActiveScenarioID = ActiveScenarioID

            BR.Close()

            Return True
        End Function

        Private Function ParseRAStream_1_1_31(ByVal Stream As MemoryStream) As Boolean
            Stream.Seek(0, SeekOrigin.Begin)

            Dim BR As New BinaryReader(Stream)

            Scenarios.Clear()

            Dim ScenariosCount As Integer = BR.ReadInt32
            Dim ActiveScenarioID As Integer = BR.ReadInt32

            Dim bFoundCosts As Boolean 'AS/4-6-16
            Dim costs() As Double 'AS/4-6-16 keep costs for the default scenario 

            For i As Integer = 1 To ScenariosCount
                Dim scenario As New RAScenario(Scenarios)

                scenario.ID = BR.ReadInt32
                scenario.Name = BR.ReadString
                scenario.Description = BR.ReadString
                scenario.Index = BR.ReadInt32
                scenario.IsCheckedCS = BR.ReadBoolean
                scenario.IsCheckedIB = BR.ReadBoolean
                scenario.Budget = BR.ReadDouble

                ' settings
                scenario.Settings.BaseCaseForConstraints = BR.ReadBoolean
                scenario.Settings.BaseCaseForDependencies = BR.ReadBoolean
                scenario.Settings.BaseCaseForFundingPools = BR.ReadBoolean
                scenario.Settings.BaseCaseForGroups = BR.ReadBoolean
                scenario.Settings.BaseCaseForMustNots = BR.ReadBoolean
                scenario.Settings.BaseCaseForMusts = BR.ReadBoolean

                scenario.Settings.CustomConstraints = BR.ReadBoolean
                scenario.Settings.Dependencies = BR.ReadBoolean
                scenario.Settings.FundingPools = BR.ReadBoolean
                scenario.Settings.Groups = BR.ReadBoolean
                scenario.Settings.MustNots = BR.ReadBoolean
                scenario.Settings.Musts = BR.ReadBoolean
                scenario.Settings.Risks = BR.ReadBoolean

                scenario.Settings.UseBaseCase = BR.ReadBoolean
                scenario.Settings.UseBaseCaseOptions = BR.ReadBoolean
                scenario.Settings.UseIgnoreOptions = BR.ReadBoolean


                ' alternatives
                Dim altsCount As Integer = BR.ReadInt32
                bFoundCosts = False 'AS/4-6-16
                For j As Integer = 1 To altsCount
                    Dim raAlt As New RAAlternative
                    raAlt.ID = BR.ReadString
                    raAlt.Name = BR.ReadString
                    raAlt.BenefitOriginal = BR.ReadDouble
                    raAlt.Cost = BR.ReadDouble
                    If i = 1 Then 'defulat scenario 'AS/4-6-16===
                        ReDim Preserve costs(j)
                        costs(j) = raAlt.Cost
                    Else
                        If raAlt.Cost <> 0 Then
                            bFoundCosts = True
                        End If
                    End If 'AS/4-6-16==
                    raAlt.IsPartial = BR.ReadBoolean
                    raAlt.MinPercent = BR.ReadDouble
                    raAlt.Must = BR.ReadBoolean
                    raAlt.MustNot = BR.ReadBoolean
                    raAlt.RiskOriginal = BR.ReadDouble
                    raAlt.SBPriority = BR.ReadDouble
                    raAlt.SBTotal = BR.ReadDouble
                    raAlt.SortOrder = BR.ReadInt32

                    scenario.AlternativesFull.Add(raAlt)
                Next

                If i > 1 Then 'do for scenarios other than default
                    If Not bFoundCosts Then 'AS/4-6-16===
                        For j As Integer = 1 To altsCount
                            scenario.Alternatives(j - 1).Cost = costs(j)
                        Next
                    End If
                End If 'AS/4-6-16==

                ' groups
                Dim groupsCount As Integer = BR.ReadInt32
                For j As Integer = 1 To groupsCount
                    Dim group As New RAGroup
                    group.ID = BR.ReadString
                    group.IntID = BR.ReadInt32
                    group.Name = BR.ReadString
                    group.Enabled = BR.ReadBoolean
                    group.Condition = CType(BR.ReadInt32, RAGroupCondition)

                    Dim aCount As Integer = BR.ReadInt32
                    For k As Integer = 1 To aCount
                        Dim altID As String = BR.ReadString
                        Dim alt As RAAlternative = scenario.Alternatives.Find(Function(a) (a.ID.ToLower = altID.ToLower))
                        If alt IsNot Nothing Then
                            group.Alternatives.Add(alt.ID, alt)
                        End If
                    Next

                    scenario.Groups.Groups.Add(group.ID, group)
                Next

                ' dependencies
                Dim dCount As Integer = BR.ReadInt32
                For j As Integer = 1 To dCount
                    Dim FirstAlternativeID As String = BR.ReadString
                    Dim SecondAlternativeID As String = BR.ReadString
                    Dim Value As RADependencyType = CType(BR.ReadInt32, RADependencyType)
                    Dim dependency As New RADependency(FirstAlternativeID, SecondAlternativeID, Value)

                    scenario.Dependencies.Dependencies.Add(dependency)
                Next

                ' custom constraints
                Dim ccCount As Integer = BR.ReadInt32
                For j As Integer = 1 To ccCount
                    Dim CC As New RAConstraint
                    CC.ID = BR.ReadInt32
                    CC.Name = BR.ReadString
                    CC.Enabled = BR.ReadBoolean
                    CC.IsReadOnly = BR.ReadBoolean
                    CC.MaxValue = BR.ReadDouble
                    CC.MinValue = BR.ReadDouble
                    CC.LinkedAttributeID = New Guid(BR.ReadBytes(16))
                    CC.LinkedEnumID = New Guid(BR.ReadBytes(16))

                    ' new to 1.1.31
                    CC.ECD_AID = BR.ReadString
                    CC.ECD_AssociatedCVID = BR.ReadString
                    CC.ECD_AssociatedUDcolKey = BR.ReadString
                    CC.ECD_CCID = BR.ReadString
                    CC.ECD_SOrder = BR.ReadInt32

                    Dim adCount As Integer = BR.ReadInt32
                    For k As Integer = 1 To adCount
                        Dim aID As String = BR.ReadString
                        Dim aValue As Double = BR.ReadDouble
                        CC.AlternativesData.Add(aID, aValue)
                    Next

                    scenario.Constraints.Constraints.Add(CC.ID, CC)
                Next

                ' funding pools
                Dim fpCount As Integer = BR.ReadInt32
                For j As Integer = 1 To fpCount
                    Dim FP As New RAFundingPool
                    FP.ID = BR.ReadInt32
                    FP.Name = BR.ReadString
                    FP.Enabled = BR.ReadBoolean
                    FP.PoolLimit = BR.ReadDouble

                    Dim vCount As Integer = BR.ReadInt32
                    For k As Integer = 1 To vCount
                        Dim aID As String = BR.ReadString
                        Dim aValue As Double = BR.ReadDouble
                        FP.Values.Add(aID, aValue)
                    Next

                    scenario.FundingPools.Pools.Add(FP.ID, FP)
                Next

                Scenarios.Scenarios.Add(scenario.ID, scenario)
            Next

            ' global settings
            Scenarios.GlobalSettings.isAutoSolve = BR.ReadBoolean
            Scenarios.GlobalSettings.Precision = BR.ReadInt32
            Scenarios.GlobalSettings.ShowFrozenHeaders = BR.ReadBoolean
            Scenarios.GlobalSettings.SortBy = CType(BR.ReadInt32, RAGlobalSettings.raColumnID)

            Scenarios.ActiveScenarioID = ActiveScenarioID

            BR.Close()

            Return True
        End Function

        Private Function ParseRAStream_1_1_35(ByVal Stream As MemoryStream) As Boolean
            Stream.Seek(0, SeekOrigin.Begin)

            Dim BR As New BinaryReader(Stream)

            Scenarios.Clear()

            Dim ScenariosCount As Integer = BR.ReadInt32
            Dim ActiveScenarioID As Integer = BR.ReadInt32

            Dim bFoundCosts As Boolean 'AS/4-6-16
            Dim costs() As Double 'AS/4-6-16 keep costs for the default scenario 

            For i As Integer = 1 To ScenariosCount
                Dim scenario As New RAScenario(Scenarios)

                scenario.ID = BR.ReadInt32
                scenario.Name = BR.ReadString
                scenario.Description = BR.ReadString
                scenario.Index = BR.ReadInt32
                scenario.IsCheckedCS = BR.ReadBoolean
                scenario.IsCheckedIB = BR.ReadBoolean
                scenario.Budget = BR.ReadDouble

                ' settings
                scenario.Settings.BaseCaseForConstraints = BR.ReadBoolean
                scenario.Settings.BaseCaseForDependencies = BR.ReadBoolean
                scenario.Settings.BaseCaseForFundingPools = BR.ReadBoolean
                scenario.Settings.BaseCaseForGroups = BR.ReadBoolean
                scenario.Settings.BaseCaseForMustNots = BR.ReadBoolean
                scenario.Settings.BaseCaseForMusts = BR.ReadBoolean

                scenario.Settings.CustomConstraints = BR.ReadBoolean
                scenario.Settings.Dependencies = BR.ReadBoolean
                scenario.Settings.FundingPools = BR.ReadBoolean
                scenario.Settings.Groups = BR.ReadBoolean
                scenario.Settings.MustNots = BR.ReadBoolean
                scenario.Settings.Musts = BR.ReadBoolean
                scenario.Settings.Risks = BR.ReadBoolean

                scenario.Settings.UseBaseCase = BR.ReadBoolean
                scenario.Settings.UseBaseCaseOptions = BR.ReadBoolean
                scenario.Settings.UseIgnoreOptions = BR.ReadBoolean


                ' alternatives
                Dim altsCount As Integer = BR.ReadInt32
                bFoundCosts = False 'AS/4-6-16
                For j As Integer = 1 To altsCount
                    Dim raAlt As New RAAlternative
                    raAlt.ID = BR.ReadString
                    raAlt.Name = BR.ReadString
                    raAlt.BenefitOriginal = BR.ReadDouble
                    raAlt.Cost = BR.ReadDouble
                    If i = 1 Then 'defulat scenario 'AS/4-6-16===
                        ReDim Preserve costs(j)
                        costs(j) = raAlt.Cost
                    Else
                        If raAlt.Cost <> 0 Then
                            bFoundCosts = True
                        End If
                    End If 'AS/4-6-16==
                    raAlt.IsPartial = BR.ReadBoolean
                    raAlt.MinPercent = BR.ReadDouble
                    raAlt.Must = BR.ReadBoolean
                    raAlt.MustNot = BR.ReadBoolean
                    raAlt.RiskOriginal = BR.ReadDouble
                    raAlt.SBPriority = BR.ReadDouble
                    raAlt.SBTotal = BR.ReadDouble
                    raAlt.SortOrder = BR.ReadInt32

                    scenario.AlternativesFull.Add(raAlt)
                Next

                If i > 1 Then 'do for scenarios other than default
                    If Not bFoundCosts Then 'AS/4-6-16===
                        For j As Integer = 1 To altsCount
                            scenario.Alternatives(j - 1).Cost = costs(j)
                        Next
                    End If
                End If 'AS/4-6-16==

                ' groups
                Dim groupsCount As Integer = BR.ReadInt32
                For j As Integer = 1 To groupsCount
                    Dim group As New RAGroup
                    group.ID = BR.ReadString
                    group.IntID = BR.ReadInt32
                    group.Name = BR.ReadString
                    group.Enabled = BR.ReadBoolean
                    group.Condition = CType(BR.ReadInt32, RAGroupCondition)

                    Dim aCount As Integer = BR.ReadInt32
                    For k As Integer = 1 To aCount
                        Dim altID As String = BR.ReadString
                        Dim alt As RAAlternative = scenario.Alternatives.Find(Function(a) (a.ID.ToLower = altID.ToLower))
                        If alt IsNot Nothing Then
                            group.Alternatives.Add(alt.ID, alt)
                        End If
                    Next

                    scenario.Groups.Groups.Add(group.ID, group)
                Next

                ' dependencies
                Dim dCount As Integer = BR.ReadInt32
                For j As Integer = 1 To dCount
                    Dim FirstAlternativeID As String = BR.ReadString
                    Dim SecondAlternativeID As String = BR.ReadString
                    Dim Value As RADependencyType = CType(BR.ReadInt32, RADependencyType)
                    Dim dependency As New RADependency(FirstAlternativeID, SecondAlternativeID, Value)

                    scenario.Dependencies.Dependencies.Add(dependency)
                Next

                ' custom constraints
                Dim ccCount As Integer = BR.ReadInt32
                For j As Integer = 1 To ccCount
                    Dim CC As New RAConstraint
                    CC.ID = BR.ReadInt32
                    CC.Name = BR.ReadString
                    CC.Enabled = BR.ReadBoolean
                    CC.IsReadOnly = BR.ReadBoolean
                    CC.MaxValue = BR.ReadDouble
                    CC.MinValue = BR.ReadDouble
                    CC.LinkedAttributeID = New Guid(BR.ReadBytes(16))
                    CC.LinkedEnumID = New Guid(BR.ReadBytes(16))

                    ' new to 1.1.31
                    CC.ECD_AID = BR.ReadString
                    CC.ECD_AssociatedCVID = BR.ReadString
                    CC.ECD_AssociatedUDcolKey = BR.ReadString
                    CC.ECD_CCID = BR.ReadString
                    CC.ECD_SOrder = BR.ReadInt32

                    Dim adCount As Integer = BR.ReadInt32
                    For k As Integer = 1 To adCount
                        Dim aID As String = BR.ReadString
                        Dim aValue As Double = BR.ReadDouble
                        CC.AlternativesData.Add(aID, aValue)
                    Next

                    scenario.Constraints.Constraints.Add(CC.ID, CC)
                Next

                ' funding pools
                Dim fpCount As Integer = BR.ReadInt32
                For j As Integer = 1 To fpCount
                    Dim FP As New RAFundingPool
                    FP.ID = BR.ReadInt32
                    FP.Name = BR.ReadString
                    FP.Enabled = BR.ReadBoolean
                    FP.PoolLimit = BR.ReadDouble

                    Dim vCount As Integer = BR.ReadInt32
                    For k As Integer = 1 To vCount
                        Dim aID As String = BR.ReadString
                        Dim aValue As Double = BR.ReadDouble
                        FP.Values.Add(aID, aValue)
                    Next

                    scenario.FundingPools.Pools.Add(FP.ID, FP)
                Next

                Dim TimePeriodsSize As Long = BR.ReadInt64
                Dim TimePeriodsBytes() As Byte = BR.ReadBytes(CInt(TimePeriodsSize))
                Dim TimePeriodsStream As New MemoryStream(TimePeriodsBytes)
                scenario.TimePeriods.ParseStream(TimePeriodsStream)

                Scenarios.Scenarios.Add(scenario.ID, scenario)
            Next

            ' global settings
            Scenarios.GlobalSettings.isAutoSolve = BR.ReadBoolean
            Scenarios.GlobalSettings.Precision = BR.ReadInt32
            Scenarios.GlobalSettings.ShowFrozenHeaders = BR.ReadBoolean
            Scenarios.GlobalSettings.SortBy = CType(BR.ReadInt32, RAGlobalSettings.raColumnID)

            Scenarios.ActiveScenarioID = ActiveScenarioID

            BR.Close()

            Return True
        End Function

        Private Function ParseRAStream_1_1_36(ByVal Stream As MemoryStream) As Boolean
            Stream.Seek(0, SeekOrigin.Begin)

            Dim BR As New BinaryReader(Stream)

            Scenarios.Clear()

            Dim ScenariosCount As Integer = BR.ReadInt32
            Dim ActiveScenarioID As Integer = BR.ReadInt32

            Dim bFoundCosts As Boolean 'AS/4-6-16
            Dim costs() As Double 'AS/4-6-16 keep costs for the default scenario 

            For i As Integer = 1 To ScenariosCount
                Dim scenario As New RAScenario(Scenarios)

                scenario.ID = BR.ReadInt32
                scenario.Name = BR.ReadString
                scenario.Description = BR.ReadString
                scenario.Index = BR.ReadInt32
                scenario.IsCheckedCS = BR.ReadBoolean
                scenario.IsCheckedIB = BR.ReadBoolean
                scenario.Budget = BR.ReadDouble

                ' settings
                scenario.Settings.BaseCaseForConstraints = BR.ReadBoolean
                scenario.Settings.BaseCaseForDependencies = BR.ReadBoolean
                scenario.Settings.BaseCaseForFundingPools = BR.ReadBoolean
                scenario.Settings.BaseCaseForGroups = BR.ReadBoolean
                scenario.Settings.BaseCaseForMustNots = BR.ReadBoolean
                scenario.Settings.BaseCaseForMusts = BR.ReadBoolean

                scenario.Settings.CustomConstraints = BR.ReadBoolean
                scenario.Settings.Dependencies = BR.ReadBoolean
                scenario.Settings.FundingPools = BR.ReadBoolean
                scenario.Settings.Groups = BR.ReadBoolean
                scenario.Settings.MustNots = BR.ReadBoolean
                scenario.Settings.Musts = BR.ReadBoolean
                scenario.Settings.Risks = BR.ReadBoolean

                scenario.Settings.UseBaseCase = BR.ReadBoolean
                scenario.Settings.UseBaseCaseOptions = BR.ReadBoolean
                scenario.Settings.UseIgnoreOptions = BR.ReadBoolean


                ' alternatives
                Dim altsCount As Integer = BR.ReadInt32
                bFoundCosts = False 'AS/4-6-16
                For j As Integer = 1 To altsCount
                    Dim raAlt As New RAAlternative
                    raAlt.ID = BR.ReadString
                    raAlt.Name = BR.ReadString
                    raAlt.BenefitOriginal = BR.ReadDouble
                    raAlt.Cost = BR.ReadDouble
                    If i = 1 Then 'defulat scenario 'AS/4-6-16===
                        ReDim Preserve costs(j)
                        costs(j) = raAlt.Cost
                    Else
                        If raAlt.Cost <> 0 Then
                            bFoundCosts = True
                        End If
                    End If 'AS/4-6-16==
                    raAlt.IsPartial = BR.ReadBoolean
                    raAlt.MinPercent = BR.ReadDouble
                    raAlt.Must = BR.ReadBoolean
                    raAlt.MustNot = BR.ReadBoolean
                    raAlt.RiskOriginal = BR.ReadDouble
                    raAlt.SBPriority = BR.ReadDouble
                    raAlt.SBTotal = BR.ReadDouble
                    raAlt.SortOrder = BR.ReadInt32
                    raAlt.Enabled = BR.ReadBoolean

                    scenario.AlternativesFull.Add(raAlt)
                Next

                If i > 1 Then 'do for scenarios other than default
                    If Not bFoundCosts Then 'AS/4-6-16===
                        For j As Integer = 1 To altsCount
                            scenario.Alternatives(j - 1).Cost = costs(j)
                        Next
                    End If
                End If 'AS/4-6-16==

                ' groups
                Dim groupsCount As Integer = BR.ReadInt32
                For j As Integer = 1 To groupsCount
                    Dim group As New RAGroup
                    group.ID = BR.ReadString
                    group.IntID = BR.ReadInt32
                    group.Name = BR.ReadString
                    group.Enabled = BR.ReadBoolean
                    group.Condition = CType(BR.ReadInt32, RAGroupCondition)

                    Dim aCount As Integer = BR.ReadInt32
                    For k As Integer = 1 To aCount
                        Dim altID As String = BR.ReadString
                        Dim alt As RAAlternative = scenario.Alternatives.Find(Function(a) (a.ID.ToLower = altID.ToLower))
                        If alt IsNot Nothing Then
                            group.Alternatives.Add(alt.ID, alt)
                        End If
                    Next

                    scenario.Groups.Groups.Add(group.ID, group)
                Next

                ' dependencies
                Dim dCount As Integer = BR.ReadInt32
                For j As Integer = 1 To dCount
                    Dim FirstAlternativeID As String = BR.ReadString
                    Dim SecondAlternativeID As String = BR.ReadString
                    Dim Value As RADependencyType = CType(BR.ReadInt32, RADependencyType)
                    Dim dependency As New RADependency(FirstAlternativeID, SecondAlternativeID, Value)

                    scenario.Dependencies.Dependencies.Add(dependency)
                Next

                ' custom constraints
                Dim ccCount As Integer = BR.ReadInt32
                For j As Integer = 1 To ccCount
                    Dim CC As New RAConstraint
                    CC.ID = BR.ReadInt32
                    CC.Name = BR.ReadString
                    CC.Enabled = BR.ReadBoolean
                    CC.IsReadOnly = BR.ReadBoolean
                    CC.MaxValue = BR.ReadDouble
                    CC.MinValue = BR.ReadDouble
                    CC.LinkedAttributeID = New Guid(BR.ReadBytes(16))
                    CC.LinkedEnumID = New Guid(BR.ReadBytes(16))

                    ' new to 1.1.31
                    CC.ECD_AID = BR.ReadString
                    CC.ECD_AssociatedCVID = BR.ReadString
                    CC.ECD_AssociatedUDcolKey = BR.ReadString
                    CC.ECD_CCID = BR.ReadString
                    CC.ECD_SOrder = BR.ReadInt32

                    Dim adCount As Integer = BR.ReadInt32
                    For k As Integer = 1 To adCount
                        Dim aID As String = BR.ReadString
                        Dim aValue As Double = BR.ReadDouble
                        CC.AlternativesData.Add(aID, aValue)
                    Next

                    scenario.Constraints.Constraints.Add(CC.ID, CC)
                Next

                ' funding pools
                Dim fpCount As Integer = BR.ReadInt32
                For j As Integer = 1 To fpCount
                    Dim FP As New RAFundingPool
                    FP.ID = BR.ReadInt32
                    FP.Name = BR.ReadString
                    FP.Enabled = BR.ReadBoolean
                    FP.PoolLimit = BR.ReadDouble

                    Dim vCount As Integer = BR.ReadInt32
                    For k As Integer = 1 To vCount
                        Dim aID As String = BR.ReadString
                        Dim aValue As Double = BR.ReadDouble
                        FP.Values.Add(aID, aValue)
                    Next

                    scenario.FundingPools.Pools.Add(FP.ID, FP)
                Next

                Dim TimePeriodsSize As Long = BR.ReadInt64
                Dim TimePeriodsBytes() As Byte = BR.ReadBytes(CInt(TimePeriodsSize))
                Dim TimePeriodsStream As New MemoryStream(TimePeriodsBytes)
                scenario.TimePeriods.ParseStream(TimePeriodsStream)

                Scenarios.Scenarios.Add(scenario.ID, scenario)
            Next

            ' global settings
            Scenarios.GlobalSettings.isAutoSolve = BR.ReadBoolean
            Scenarios.GlobalSettings.Precision = BR.ReadInt32
            Scenarios.GlobalSettings.ShowFrozenHeaders = BR.ReadBoolean
            Scenarios.GlobalSettings.SortBy = CType(BR.ReadInt32, RAGlobalSettings.raColumnID)

            Scenarios.ActiveScenarioID = ActiveScenarioID

            BR.Close()

            Return True
        End Function

        Private Function ParseRAStream_1_1_37(ByVal Stream As MemoryStream) As Boolean
            Stream.Seek(0, SeekOrigin.Begin)

            Dim BR As New BinaryReader(Stream)

            Scenarios.Clear()

            Dim ScenariosCount As Integer = BR.ReadInt32
            Dim ActiveScenarioID As Integer = BR.ReadInt32

            Dim bFoundCosts As Boolean 'AS/4-6-16
            Dim costs() As Double 'AS/4-6-16 keep costs for the default scenario 

            For i As Integer = 1 To ScenariosCount
                Dim scenario As New RAScenario(Scenarios)

                scenario.ID = BR.ReadInt32
                scenario.Name = BR.ReadString
                scenario.Description = BR.ReadString
                scenario.Index = BR.ReadInt32
                scenario.IsCheckedCS = BR.ReadBoolean
                scenario.IsCheckedIB = BR.ReadBoolean
                scenario.Budget = BR.ReadDouble

                ' settings
                scenario.Settings.BaseCaseForConstraints = BR.ReadBoolean
                scenario.Settings.BaseCaseForDependencies = BR.ReadBoolean
                scenario.Settings.BaseCaseForFundingPools = BR.ReadBoolean
                scenario.Settings.BaseCaseForGroups = BR.ReadBoolean
                scenario.Settings.BaseCaseForMustNots = BR.ReadBoolean
                scenario.Settings.BaseCaseForMusts = BR.ReadBoolean

                scenario.Settings.CustomConstraints = BR.ReadBoolean
                scenario.Settings.Dependencies = BR.ReadBoolean
                scenario.Settings.FundingPools = BR.ReadBoolean
                scenario.Settings.Groups = BR.ReadBoolean
                scenario.Settings.MustNots = BR.ReadBoolean
                scenario.Settings.Musts = BR.ReadBoolean
                scenario.Settings.Risks = BR.ReadBoolean

                scenario.Settings.UseBaseCase = BR.ReadBoolean
                scenario.Settings.UseBaseCaseOptions = BR.ReadBoolean
                scenario.Settings.UseIgnoreOptions = BR.ReadBoolean


                ' alternatives
                Dim altsCount As Integer = BR.ReadInt32
                bFoundCosts = False 'AS/4-6-16
                For j As Integer = 1 To altsCount
                    Dim raAlt As New RAAlternative
                    raAlt.ID = BR.ReadString
                    raAlt.Name = BR.ReadString
                    raAlt.BenefitOriginal = BR.ReadDouble
                    raAlt.Cost = BR.ReadDouble
                    If i = 1 Then 'defulat scenario 'AS/4-6-16===
                        ReDim Preserve costs(j)
                        costs(j) = raAlt.Cost
                    Else
                        If raAlt.Cost <> 0 Then
                            bFoundCosts = True
                        End If
                    End If 'AS/4-6-16==
                    raAlt.IsPartial = BR.ReadBoolean
                    raAlt.MinPercent = BR.ReadDouble
                    raAlt.Must = BR.ReadBoolean
                    raAlt.MustNot = BR.ReadBoolean
                    raAlt.RiskOriginal = BR.ReadDouble
                    raAlt.SBPriority = BR.ReadDouble
                    raAlt.SBTotal = BR.ReadDouble
                    raAlt.SortOrder = BR.ReadInt32
                    raAlt.Enabled = BR.ReadBoolean

                    scenario.AlternativesFull.Add(raAlt)
                Next

                If i > 1 Then 'do for scenarios other than default
                    If Not bFoundCosts Then 'AS/4-6-16===
                        For j As Integer = 1 To altsCount
                            scenario.Alternatives(j - 1).Cost = costs(j)
                        Next
                    End If
                End If 'AS/4-6-16==

                ' groups
                Dim groupsCount As Integer = BR.ReadInt32
                For j As Integer = 1 To groupsCount
                    Dim group As New RAGroup
                    group.ID = BR.ReadString
                    group.IntID = BR.ReadInt32
                    group.Name = BR.ReadString
                    group.Enabled = BR.ReadBoolean
                    group.Condition = CType(BR.ReadInt32, RAGroupCondition)

                    Dim aCount As Integer = BR.ReadInt32
                    For k As Integer = 1 To aCount
                        Dim altID As String = BR.ReadString
                        Dim alt As RAAlternative = scenario.Alternatives.Find(Function(a) (a.ID.ToLower = altID.ToLower))
                        If alt IsNot Nothing Then
                            group.Alternatives.Add(alt.ID, alt)
                        End If
                    Next

                    scenario.Groups.Groups.Add(group.ID, group)
                Next

                ' dependencies
                Dim dCount As Integer = BR.ReadInt32
                For j As Integer = 1 To dCount
                    Dim FirstAlternativeID As String = BR.ReadString
                    Dim SecondAlternativeID As String = BR.ReadString
                    Dim Value As RADependencyType = CType(BR.ReadInt32, RADependencyType)
                    Dim dependency As New RADependency(FirstAlternativeID, SecondAlternativeID, Value)

                    scenario.Dependencies.Dependencies.Add(dependency)
                Next

                ' custom constraints
                Dim ccCount As Integer = BR.ReadInt32
                For j As Integer = 1 To ccCount
                    Dim CC As New RAConstraint
                    CC.ID = BR.ReadInt32
                    CC.Name = BR.ReadString
                    CC.Enabled = BR.ReadBoolean
                    CC.IsReadOnly = BR.ReadBoolean
                    CC.MaxValue = BR.ReadDouble
                    CC.MinValue = BR.ReadDouble
                    CC.LinkedAttributeID = New Guid(BR.ReadBytes(16))
                    CC.LinkedEnumID = New Guid(BR.ReadBytes(16))

                    ' new to 1.1.31
                    CC.ECD_AID = BR.ReadString
                    CC.ECD_AssociatedCVID = BR.ReadString
                    CC.ECD_AssociatedUDcolKey = BR.ReadString
                    CC.ECD_CCID = BR.ReadString
                    CC.ECD_SOrder = BR.ReadInt32

                    Dim adCount As Integer = BR.ReadInt32
                    For k As Integer = 1 To adCount
                        Dim aID As String = BR.ReadString
                        Dim aValue As Double = BR.ReadDouble
                        CC.AlternativesData.Add(aID, aValue)
                    Next

                    scenario.Constraints.Constraints.Add(CC.ID, CC)
                Next

                ' funding pools
                Dim fpCount As Integer = BR.ReadInt32
                For j As Integer = 1 To fpCount
                    Dim FP As New RAFundingPool
                    FP.ID = BR.ReadInt32
                    FP.Name = BR.ReadString
                    FP.Enabled = BR.ReadBoolean
                    FP.PoolLimit = BR.ReadDouble

                    Dim vCount As Integer = BR.ReadInt32
                    For k As Integer = 1 To vCount
                        Dim aID As String = BR.ReadString
                        Dim aValue As Double = BR.ReadDouble
                        FP.Values.Add(aID, aValue)
                    Next

                    scenario.FundingPools.Pools.Add(FP.ID, FP)
                Next

                Dim TimePeriodsSize As Long = BR.ReadInt64
                Dim TimePeriodsBytes() As Byte = BR.ReadBytes(CInt(TimePeriodsSize))
                Dim TimePeriodsStream As New MemoryStream(TimePeriodsBytes)
                scenario.TimePeriods.ParseStream_v_37(TimePeriodsStream)

                Scenarios.Scenarios.Add(scenario.ID, scenario)
            Next

            ' global settings
            Scenarios.GlobalSettings.isAutoSolve = BR.ReadBoolean
            Scenarios.GlobalSettings.Precision = BR.ReadInt32
            Scenarios.GlobalSettings.ShowFrozenHeaders = BR.ReadBoolean
            Scenarios.GlobalSettings.SortBy = CType(BR.ReadInt32, RAGlobalSettings.raColumnID)

            Scenarios.ActiveScenarioID = ActiveScenarioID

            BR.Close()

            Return True
        End Function

        Private Function ParseRAStream_1_1_38(ByVal Stream As MemoryStream) As Boolean
            Stream.Seek(0, SeekOrigin.Begin)

            Dim BR As New BinaryReader(Stream)

            Scenarios.Clear()

            Dim ScenariosCount As Integer = BR.ReadInt32
            Dim ActiveScenarioID As Integer = BR.ReadInt32

            Dim bFoundCosts As Boolean 'AS/4-6-16
            Dim costs() As Double 'AS/4-6-16 keep costs for the default scenario 

            For i As Integer = 1 To ScenariosCount
                Dim scenario As New RAScenario(Scenarios)

                scenario.ID = BR.ReadInt32
                scenario.Name = BR.ReadString
                scenario.Description = BR.ReadString
                scenario.Index = BR.ReadInt32
                scenario.IsCheckedCS = BR.ReadBoolean
                scenario.IsCheckedIB = BR.ReadBoolean
                scenario.Budget = BR.ReadDouble

                ' settings
                scenario.Settings.BaseCaseForConstraints = BR.ReadBoolean
                scenario.Settings.BaseCaseForDependencies = BR.ReadBoolean
                scenario.Settings.BaseCaseForFundingPools = BR.ReadBoolean
                scenario.Settings.BaseCaseForGroups = BR.ReadBoolean
                scenario.Settings.BaseCaseForMustNots = BR.ReadBoolean
                scenario.Settings.BaseCaseForMusts = BR.ReadBoolean

                scenario.Settings.CustomConstraints = BR.ReadBoolean
                scenario.Settings.Dependencies = BR.ReadBoolean
                scenario.Settings.FundingPools = BR.ReadBoolean
                scenario.Settings.Groups = BR.ReadBoolean
                scenario.Settings.MustNots = BR.ReadBoolean
                scenario.Settings.Musts = BR.ReadBoolean
                scenario.Settings.Risks = BR.ReadBoolean

                scenario.Settings.UseBaseCase = BR.ReadBoolean
                scenario.Settings.UseBaseCaseOptions = BR.ReadBoolean
                scenario.Settings.UseIgnoreOptions = BR.ReadBoolean

                scenario.Settings.TimePeriods = BR.ReadBoolean

                ' alternatives
                Dim altsCount As Integer = BR.ReadInt32
                bFoundCosts = False 'AS/4-6-16
                For j As Integer = 1 To altsCount
                    Dim raAlt As New RAAlternative
                    raAlt.ID = BR.ReadString
                    raAlt.Name = BR.ReadString
                    raAlt.BenefitOriginal = BR.ReadDouble
                    raAlt.Cost = BR.ReadDouble
                    If i = 1 Then 'defulat scenario 'AS/4-6-16===
                        ReDim Preserve costs(j)
                        costs(j) = raAlt.Cost
                    Else
                        If raAlt.Cost <> 0 Then
                            bFoundCosts = True
                        End If
                    End If 'AS/4-6-16==
                    raAlt.IsPartial = BR.ReadBoolean
                    raAlt.MinPercent = BR.ReadDouble
                    raAlt.Must = BR.ReadBoolean
                    raAlt.MustNot = BR.ReadBoolean
                    raAlt.RiskOriginal = BR.ReadDouble
                    raAlt.SBPriority = BR.ReadDouble
                    raAlt.SBTotal = BR.ReadDouble
                    raAlt.SortOrder = BR.ReadInt32
                    raAlt.Enabled = BR.ReadBoolean

                    scenario.AlternativesFull.Add(raAlt)
                Next

                If i > 1 Then 'do for scenarios other than default
                    If Not bFoundCosts Then 'AS/4-6-16===
                        For j As Integer = 1 To altsCount
                            scenario.Alternatives(j - 1).Cost = costs(j)
                        Next
                    End If
                End If 'AS/4-6-16==

                ' groups
                Dim groupsCount As Integer = BR.ReadInt32
                For j As Integer = 1 To groupsCount
                    Dim group As New RAGroup
                    group.ID = BR.ReadString
                    group.IntID = BR.ReadInt32
                    group.Name = BR.ReadString
                    group.Enabled = BR.ReadBoolean
                    group.Condition = CType(BR.ReadInt32, RAGroupCondition)

                    Dim aCount As Integer = BR.ReadInt32
                    For k As Integer = 1 To aCount
                        Dim altID As String = BR.ReadString
                        Dim alt As RAAlternative = scenario.Alternatives.Find(Function(a) (a.ID.ToLower = altID.ToLower))
                        If alt IsNot Nothing Then
                            group.Alternatives.Add(alt.ID, alt)
                        End If
                    Next

                    scenario.Groups.Groups.Add(group.ID, group)
                Next

                ' dependencies
                Dim dCount As Integer = BR.ReadInt32
                For j As Integer = 1 To dCount
                    Dim FirstAlternativeID As String = BR.ReadString
                    Dim SecondAlternativeID As String = BR.ReadString
                    Dim Value As RADependencyType = CType(BR.ReadInt32, RADependencyType)
                    Dim dependency As New RADependency(FirstAlternativeID, SecondAlternativeID, Value)

                    scenario.Dependencies.Dependencies.Add(dependency)
                Next

                ' custom constraints
                Dim ccCount As Integer = BR.ReadInt32
                For j As Integer = 1 To ccCount
                    Dim CC As New RAConstraint
                    CC.ID = BR.ReadInt32
                    CC.Name = BR.ReadString
                    CC.Enabled = BR.ReadBoolean
                    CC.IsReadOnly = BR.ReadBoolean
                    CC.MaxValue = BR.ReadDouble
                    CC.MinValue = BR.ReadDouble
                    CC.LinkedAttributeID = New Guid(BR.ReadBytes(16))
                    CC.LinkedEnumID = New Guid(BR.ReadBytes(16))

                    ' new to 1.1.31
                    CC.ECD_AID = BR.ReadString
                    CC.ECD_AssociatedCVID = BR.ReadString
                    CC.ECD_AssociatedUDcolKey = BR.ReadString
                    CC.ECD_CCID = BR.ReadString
                    CC.ECD_SOrder = BR.ReadInt32

                    Dim adCount As Integer = BR.ReadInt32
                    For k As Integer = 1 To adCount
                        Dim aID As String = BR.ReadString
                        Dim aValue As Double = BR.ReadDouble
                        CC.AlternativesData.Add(aID, aValue)
                    Next

                    scenario.Constraints.Constraints.Add(CC.ID, CC)
                Next

                ' funding pools
                Dim fpCount As Integer = BR.ReadInt32
                For j As Integer = 1 To fpCount
                    Dim FP As New RAFundingPool
                    FP.ID = BR.ReadInt32
                    FP.Name = BR.ReadString
                    FP.Enabled = BR.ReadBoolean
                    FP.PoolLimit = BR.ReadDouble

                    Dim vCount As Integer = BR.ReadInt32
                    For k As Integer = 1 To vCount
                        Dim aID As String = BR.ReadString
                        Dim aValue As Double = BR.ReadDouble
                        FP.Values.Add(aID, aValue)
                    Next

                    scenario.FundingPools.Pools.Add(FP.ID, FP)
                Next

                Dim TimePeriodsSize As Long = BR.ReadInt64
                Dim TimePeriodsBytes() As Byte = BR.ReadBytes(CInt(TimePeriodsSize))
                Dim TimePeriodsStream As New MemoryStream(TimePeriodsBytes)
                scenario.TimePeriods.ParseStream_v_38(TimePeriodsStream)

                Scenarios.Scenarios.Add(scenario.ID, scenario)
            Next

            ' global settings
            Scenarios.GlobalSettings.isAutoSolve = BR.ReadBoolean
            Scenarios.GlobalSettings.Precision = BR.ReadInt32
            Scenarios.GlobalSettings.ShowFrozenHeaders = BR.ReadBoolean
            Scenarios.GlobalSettings.SortBy = CType(BR.ReadInt32, RAGlobalSettings.raColumnID)

            Scenarios.ActiveScenarioID = ActiveScenarioID

            BR.Close()

            Return True
        End Function

        Private Function ParseRAStream_1_1_39(ByVal Stream As MemoryStream) As Boolean
            Stream.Seek(0, SeekOrigin.Begin)

            Dim BR As New BinaryReader(Stream)

            Scenarios.Clear()

            Dim ScenariosCount As Integer = BR.ReadInt32
            Dim ActiveScenarioID As Integer = BR.ReadInt32

            ' v1.1.39
            CombinedGroupUserID = BR.ReadInt32
            If CombinedGroupUserID >= 0 Then
                Dim CG As clsCombinedGroup = CType(ProjectManager.CombinedGroups.GetGroupByID(CombinedGroupUserID), clsCombinedGroup)
                If CG IsNot Nothing Then CombinedGroupUserID = CG.CombinedUserID Else CombinedGroupUserID = COMBINED_USER_ID
            End If

            Dim bFoundCosts As Boolean 'AS/4-6-16
            Dim costs() As Double 'AS/4-6-16 keep costs for the default scenario 

            For i As Integer = 1 To ScenariosCount
                Dim scenario As New RAScenario(Scenarios)

                scenario.ID = BR.ReadInt32
                scenario.Name = BR.ReadString
                scenario.Description = BR.ReadString
                scenario.Index = BR.ReadInt32
                scenario.IsCheckedCS = BR.ReadBoolean
                scenario.IsCheckedIB = BR.ReadBoolean
                scenario.Budget = BR.ReadDouble

                ' v1.1.39
                scenario.CombinedGroupUserID = BR.ReadInt32
                If scenario.CombinedGroupUserID >= 0 Then
                    Dim CG As clsCombinedGroup = CType(ProjectManager.CombinedGroups.GetGroupByID(scenario.CombinedGroupUserID), clsCombinedGroup)
                    If CG IsNot Nothing Then scenario.CombinedGroupUserID = CG.CombinedUserID Else scenario.CombinedGroupUserID = COMBINED_USER_ID
                End If

                ' settings
                scenario.Settings.BaseCaseForConstraints = BR.ReadBoolean
                scenario.Settings.BaseCaseForDependencies = BR.ReadBoolean
                scenario.Settings.BaseCaseForFundingPools = BR.ReadBoolean
                scenario.Settings.BaseCaseForGroups = BR.ReadBoolean
                scenario.Settings.BaseCaseForMustNots = BR.ReadBoolean
                scenario.Settings.BaseCaseForMusts = BR.ReadBoolean

                scenario.Settings.CustomConstraints = BR.ReadBoolean
                scenario.Settings.Dependencies = BR.ReadBoolean
                scenario.Settings.FundingPools = BR.ReadBoolean
                scenario.Settings.Groups = BR.ReadBoolean
                scenario.Settings.MustNots = BR.ReadBoolean
                scenario.Settings.Musts = BR.ReadBoolean
                scenario.Settings.Risks = BR.ReadBoolean

                scenario.Settings.UseBaseCase = BR.ReadBoolean
                scenario.Settings.UseBaseCaseOptions = BR.ReadBoolean
                scenario.Settings.UseIgnoreOptions = BR.ReadBoolean

                scenario.Settings.TimePeriods = BR.ReadBoolean

                ' alternatives
                Dim altsCount As Integer = BR.ReadInt32
                bFoundCosts = False 'AS/4-6-16
                For j As Integer = 1 To altsCount
                    Dim raAlt As New RAAlternative
                    raAlt.ID = BR.ReadString
                    raAlt.Name = BR.ReadString
                    raAlt.BenefitOriginal = BR.ReadDouble
                    raAlt.Cost = BR.ReadDouble
                    If i = 1 Then 'defulat scenario 'AS/4-6-16===
                        ReDim Preserve costs(j)
                        costs(j) = raAlt.Cost
                    Else
                        If raAlt.Cost <> 0 Then
                            bFoundCosts = True
                        End If
                    End If 'AS/4-6-16==
                    raAlt.IsPartial = BR.ReadBoolean
                    raAlt.MinPercent = BR.ReadDouble
                    raAlt.Must = BR.ReadBoolean
                    raAlt.MustNot = BR.ReadBoolean
                    raAlt.RiskOriginal = BR.ReadDouble
                    raAlt.SBPriority = BR.ReadDouble
                    raAlt.SBTotal = BR.ReadDouble
                    raAlt.SortOrder = BR.ReadInt32
                    raAlt.Enabled = BR.ReadBoolean

                    scenario.AlternativesFull.Add(raAlt)
                Next

                If i > 1 Then 'do for scenarios other than default
                    If Not bFoundCosts Then 'AS/4-6-16===
                        For j As Integer = 1 To altsCount
                            scenario.AlternativesFull(j - 1).Cost = costs(j)    ' D3881
                        Next
                    End If
                End If 'AS/4-6-16==

                ' groups
                Dim groupsCount As Integer = BR.ReadInt32
                For j As Integer = 1 To groupsCount
                    Dim group As New RAGroup
                    group.ID = BR.ReadString
                    group.IntID = BR.ReadInt32
                    group.Name = BR.ReadString
                    group.Enabled = BR.ReadBoolean
                    group.Condition = CType(BR.ReadInt32, RAGroupCondition)

                    Dim aCount As Integer = BR.ReadInt32
                    For k As Integer = 1 To aCount
                        Dim altID As String = BR.ReadString
                        Dim alt As RAAlternative = scenario.AlternativesFull.Find(Function(a) (a.ID.ToLower = altID.ToLower))   ' D3881
                        If alt IsNot Nothing Then
                            group.Alternatives.Add(alt.ID, alt)
                        End If
                    Next

                    scenario.Groups.Groups.Add(group.ID, group)
                Next

                ' dependencies
                Dim dCount As Integer = BR.ReadInt32
                For j As Integer = 1 To dCount
                    Dim FirstAlternativeID As String = BR.ReadString
                    Dim SecondAlternativeID As String = BR.ReadString
                    Dim Value As RADependencyType = CType(BR.ReadInt32, RADependencyType)
                    Dim dependency As New RADependency(FirstAlternativeID, SecondAlternativeID, Value)
                    If Value = RADependencyType.dtConcurrent Or Value = RADependencyType.dtSuccessive Then
                        scenario.TimePeriodsDependencies.Dependencies.Add(dependency)
                    Else
                        scenario.Dependencies.Dependencies.Add(dependency)
                    End If
                Next

                ' custom constraints
                Dim ccCount As Integer = BR.ReadInt32
                For j As Integer = 1 To ccCount
                    Dim CC As New RAConstraint
                    CC.ID = BR.ReadInt32
                    CC.Name = BR.ReadString
                    CC.Enabled = BR.ReadBoolean
                    CC.IsReadOnly = BR.ReadBoolean
                    CC.MaxValue = BR.ReadDouble
                    CC.MinValue = BR.ReadDouble
                    CC.LinkedAttributeID = New Guid(BR.ReadBytes(16))
                    CC.LinkedEnumID = New Guid(BR.ReadBytes(16))

                    ' new to 1.1.31
                    CC.ECD_AID = BR.ReadString
                    CC.ECD_AssociatedCVID = BR.ReadString
                    CC.ECD_AssociatedUDcolKey = BR.ReadString
                    CC.ECD_CCID = BR.ReadString
                    CC.ECD_SOrder = BR.ReadInt32

                    Dim adCount As Integer = BR.ReadInt32
                    For k As Integer = 1 To adCount
                        Dim aID As String = BR.ReadString
                        Dim aValue As Double = BR.ReadDouble
                        CC.AlternativesData.Add(aID, aValue)
                    Next

                    CC.IsLinkedToResource = BR.ReadBoolean

                    scenario.Constraints.Constraints.Add(CC.ID, CC)
                Next

                ' funding pools
                Dim fpCount As Integer = BR.ReadInt32
                For j As Integer = 1 To fpCount
                    Dim FP As New RAFundingPool
                    FP.ID = BR.ReadInt32
                    FP.Name = BR.ReadString
                    FP.Enabled = BR.ReadBoolean
                    FP.PoolLimit = BR.ReadDouble

                    Dim vCount As Integer = BR.ReadInt32
                    For k As Integer = 1 To vCount
                        Dim aID As String = BR.ReadString
                        Dim aValue As Double = BR.ReadDouble
                        FP.Values.Add(aID, aValue)
                    Next

                    scenario.FundingPools.Pools.Add(FP.ID, FP)
                Next

                Dim TimePeriodsSize As Long = BR.ReadInt64
                Dim TimePeriodsBytes() As Byte = BR.ReadBytes(CInt(TimePeriodsSize))
                Dim TimePeriodsStream As New MemoryStream(TimePeriodsBytes)
                scenario.TimePeriods.ParseStream_v_39(TimePeriodsStream)

                Scenarios.Scenarios.Add(scenario.ID, scenario)
            Next

            ' global settings
            Scenarios.GlobalSettings.isAutoSolve = BR.ReadBoolean
            Scenarios.GlobalSettings.Precision = BR.ReadInt32
            Scenarios.GlobalSettings.ShowFrozenHeaders = BR.ReadBoolean
            Scenarios.GlobalSettings.SortBy = CType(BR.ReadInt32, RAGlobalSettings.raColumnID)

            Scenarios.ActiveScenarioID = ActiveScenarioID

            BR.Close()

            Return True
        End Function

        Private Function ParseRAStream_1_1_40(ByVal Stream As MemoryStream) As Boolean
            Stream.Seek(0, SeekOrigin.Begin)

            Dim BR As New BinaryReader(Stream)

            Scenarios.Clear()

            Dim ScenariosCount As Integer = BR.ReadInt32
            Dim ActiveScenarioID As Integer = BR.ReadInt32

            ' v1.1.39
            CombinedGroupUserID = BR.ReadInt32
            If CombinedGroupUserID >= 0 Then
                Dim CG As clsCombinedGroup = CType(ProjectManager.CombinedGroups.GetGroupByID(CombinedGroupUserID), clsCombinedGroup)
                If CG IsNot Nothing Then CombinedGroupUserID = CG.CombinedUserID Else CombinedGroupUserID = COMBINED_USER_ID
            End If

            Dim bFoundCosts As Boolean 'AS/4-6-16
            Dim costs() As Double 'AS/4-6-16 keep costs for the default scenario 

            For i As Integer = 1 To ScenariosCount
                Dim scenario As New RAScenario(Scenarios)

                scenario.ID = BR.ReadInt32
                scenario.Name = BR.ReadString
                scenario.Description = BR.ReadString
                scenario.Index = BR.ReadInt32
                scenario.IsCheckedCS = BR.ReadBoolean
                scenario.IsCheckedIB = BR.ReadBoolean
                scenario.Budget = BR.ReadDouble

                ' v1.1.39
                scenario.CombinedGroupUserID = BR.ReadInt32
                If scenario.CombinedGroupUserID >= 0 Then
                    Dim CG As clsCombinedGroup = CType(ProjectManager.CombinedGroups.GetGroupByID(scenario.CombinedGroupUserID), clsCombinedGroup)
                    If CG IsNot Nothing Then scenario.CombinedGroupUserID = CG.CombinedUserID Else scenario.CombinedGroupUserID = COMBINED_USER_ID
                End If

                ' settings
                scenario.Settings.BaseCaseForConstraints = BR.ReadBoolean
                scenario.Settings.BaseCaseForDependencies = BR.ReadBoolean
                scenario.Settings.BaseCaseForFundingPools = BR.ReadBoolean
                scenario.Settings.BaseCaseForGroups = BR.ReadBoolean
                scenario.Settings.BaseCaseForMustNots = BR.ReadBoolean
                scenario.Settings.BaseCaseForMusts = BR.ReadBoolean

                scenario.Settings.CustomConstraints = BR.ReadBoolean
                scenario.Settings.Dependencies = BR.ReadBoolean
                scenario.Settings.FundingPools = BR.ReadBoolean
                scenario.Settings.Groups = BR.ReadBoolean
                scenario.Settings.MustNots = BR.ReadBoolean
                scenario.Settings.Musts = BR.ReadBoolean
                scenario.Settings.Risks = BR.ReadBoolean

                scenario.Settings.UseBaseCase = BR.ReadBoolean
                scenario.Settings.UseBaseCaseOptions = BR.ReadBoolean
                scenario.Settings.UseIgnoreOptions = BR.ReadBoolean

                scenario.Settings.TimePeriods = BR.ReadBoolean

                ' alternatives
                Dim altsCount As Integer = BR.ReadInt32
                bFoundCosts = False 'AS/4-6-16
                For j As Integer = 1 To altsCount
                    Dim raAlt As New RAAlternative
                    raAlt.ID = BR.ReadString
                    raAlt.Name = BR.ReadString
                    raAlt.BenefitOriginal = BR.ReadDouble
                    raAlt.Cost = BR.ReadDouble
                    If i = 1 Then 'defulat scenario 'AS/4-6-16===
                        ReDim Preserve costs(j)
                        costs(j) = raAlt.Cost
                    Else
                        If raAlt.Cost <> 0 Then
                            bFoundCosts = True
                        End If
                    End If 'AS/4-6-16==
                    raAlt.IsPartial = BR.ReadBoolean
                    raAlt.MinPercent = BR.ReadDouble
                    raAlt.Must = BR.ReadBoolean
                    raAlt.MustNot = BR.ReadBoolean
                    raAlt.RiskOriginal = BR.ReadDouble
                    raAlt.SBPriority = BR.ReadDouble
                    raAlt.SBTotal = BR.ReadDouble
                    raAlt.SortOrder = BR.ReadInt32
                    raAlt.Enabled = BR.ReadBoolean

                    scenario.AlternativesFull.Add(raAlt)
                Next

                If i > 1 Then 'do for scenarios other than default
                    If Not bFoundCosts Then 'AS/4-6-16===
                        For j As Integer = 1 To altsCount
                            scenario.AlternativesFull(j - 1).Cost = costs(j)    ' D3881
                        Next
                    End If
                End If 'AS/4-6-16==

                ' groups
                Dim groupsCount As Integer = BR.ReadInt32
                For j As Integer = 1 To groupsCount
                    Dim group As New RAGroup
                    group.ID = BR.ReadString
                    group.IntID = BR.ReadInt32
                    group.Name = BR.ReadString
                    group.Enabled = BR.ReadBoolean
                    group.Condition = CType(BR.ReadInt32, RAGroupCondition)

                    Dim aCount As Integer = BR.ReadInt32
                    For k As Integer = 1 To aCount
                        Dim altID As String = BR.ReadString
                        Dim alt As RAAlternative = scenario.AlternativesFull.Find(Function(a) (a.ID.ToLower = altID.ToLower))   ' D3881
                        If alt IsNot Nothing Then
                            group.Alternatives.Add(alt.ID, alt)
                        End If
                    Next

                    scenario.Groups.Groups.Add(group.ID, group)
                Next

                ' dependencies
                Dim dCount As Integer = BR.ReadInt32
                For j As Integer = 1 To dCount
                    Dim FirstAlternativeID As String = BR.ReadString
                    Dim SecondAlternativeID As String = BR.ReadString
                    Dim Value As RADependencyType = CType(BR.ReadInt32, RADependencyType)

                    'new to 1.1.40
                    Dim LagCondition As LagCondition = CType(BR.ReadInt32, LagCondition)
                    Dim Lag As Integer = BR.ReadInt32
                    Dim LagUpperBound As Integer = BR.ReadInt32

                    Dim dependency As New RADependency(FirstAlternativeID, SecondAlternativeID, Value)

                    ' new to 1.1.40
                    dependency.LagCondition = LagCondition
                    dependency.Lag = Lag
                    dependency.LagUpperBound = LagUpperBound

                    If Value = RADependencyType.dtConcurrent Or Value = RADependencyType.dtSuccessive Or Value = RADependencyType.dtLag Then
                        scenario.TimePeriodsDependencies.Dependencies.Add(dependency)
                    Else
                        scenario.Dependencies.Dependencies.Add(dependency)
                    End If
                Next

                ' custom constraints
                Dim ccCount As Integer = BR.ReadInt32
                For j As Integer = 1 To ccCount
                    Dim CC As New RAConstraint
                    CC.ID = BR.ReadInt32
                    CC.Name = BR.ReadString
                    CC.Enabled = BR.ReadBoolean
                    CC.IsReadOnly = BR.ReadBoolean
                    CC.MaxValue = BR.ReadDouble
                    CC.MinValue = BR.ReadDouble
                    CC.LinkedAttributeID = New Guid(BR.ReadBytes(16))
                    CC.LinkedEnumID = New Guid(BR.ReadBytes(16))

                    ' new to 1.1.31
                    CC.ECD_AID = BR.ReadString
                    CC.ECD_AssociatedCVID = BR.ReadString
                    CC.ECD_AssociatedUDcolKey = BR.ReadString
                    CC.ECD_CCID = BR.ReadString
                    CC.ECD_SOrder = BR.ReadInt32

                    Dim adCount As Integer = BR.ReadInt32
                    For k As Integer = 1 To adCount
                        Dim aID As String = BR.ReadString
                        Dim aValue As Double = BR.ReadDouble
                        CC.AlternativesData.Add(aID, aValue)
                    Next

                    CC.IsLinkedToResource = BR.ReadBoolean

                    scenario.Constraints.Constraints.Add(CC.ID, CC)
                Next

                ' funding pools
                Dim fpCount As Integer = BR.ReadInt32
                For j As Integer = 1 To fpCount
                    Dim FP As New RAFundingPool
                    FP.ID = BR.ReadInt32
                    FP.Name = BR.ReadString
                    FP.Enabled = BR.ReadBoolean
                    FP.PoolLimit = BR.ReadDouble

                    Dim vCount As Integer = BR.ReadInt32
                    For k As Integer = 1 To vCount
                        Dim aID As String = BR.ReadString
                        Dim aValue As Double = BR.ReadDouble
                        FP.Values.Add(aID, aValue)
                    Next

                    scenario.FundingPools.Pools.Add(FP.ID, FP)
                Next

                Dim TimePeriodsSize As Long = BR.ReadInt64
                Dim TimePeriodsBytes() As Byte = BR.ReadBytes(CInt(TimePeriodsSize))
                Dim TimePeriodsStream As New MemoryStream(TimePeriodsBytes)
                scenario.TimePeriods.ParseStream_v_40(TimePeriodsStream)

                Scenarios.Scenarios.Add(scenario.ID, scenario)
            Next

            ' global settings
            Scenarios.GlobalSettings.isAutoSolve = BR.ReadBoolean
            Scenarios.GlobalSettings.Precision = BR.ReadInt32
            Scenarios.GlobalSettings.ShowFrozenHeaders = BR.ReadBoolean
            Scenarios.GlobalSettings.SortBy = CType(BR.ReadInt32, RAGlobalSettings.raColumnID)

            Scenarios.ActiveScenarioID = ActiveScenarioID

            BR.Close()

            Return True
        End Function

        Private Function ParseRAStream_1_1_43(ByVal Stream As MemoryStream) As Boolean
            Stream.Seek(0, SeekOrigin.Begin)

            Dim BR As New BinaryReader(Stream)

            Scenarios.Clear()

            Dim ScenariosCount As Integer = BR.ReadInt32
            Dim ActiveScenarioID As Integer = BR.ReadInt32

            ' v1.1.39
            CombinedGroupUserID = BR.ReadInt32
            If CombinedGroupUserID >= 0 Then
                Dim CG As clsCombinedGroup = CType(ProjectManager.CombinedGroups.GetGroupByID(CombinedGroupUserID), clsCombinedGroup)
                If CG IsNot Nothing Then CombinedGroupUserID = CG.CombinedUserID Else CombinedGroupUserID = COMBINED_USER_ID
            End If

            Dim bFoundCosts As Boolean 'AS/4-6-16
            Dim costs() As Double 'AS/4-6-16 keep costs for the default scenario 

            For i As Integer = 1 To ScenariosCount
                Dim scenario As New RAScenario(Scenarios)

                scenario.ID = BR.ReadInt32
                scenario.Name = BR.ReadString
                scenario.Description = BR.ReadString
                scenario.Index = BR.ReadInt32
                scenario.IsCheckedCS = BR.ReadBoolean
                scenario.IsCheckedIB = BR.ReadBoolean
                scenario.Budget = BR.ReadDouble

                ' v1.1.39
                scenario.CombinedGroupUserID = BR.ReadInt32
                If scenario.CombinedGroupUserID >= 0 Then
                    Dim CG As clsCombinedGroup = CType(ProjectManager.CombinedGroups.GetGroupByID(scenario.CombinedGroupUserID), clsCombinedGroup)
                    If CG IsNot Nothing Then scenario.CombinedGroupUserID = CG.CombinedUserID Else scenario.CombinedGroupUserID = COMBINED_USER_ID
                End If

                ' settings
                scenario.Settings.BaseCaseForConstraints = BR.ReadBoolean
                scenario.Settings.BaseCaseForDependencies = BR.ReadBoolean
                scenario.Settings.BaseCaseForFundingPools = BR.ReadBoolean
                scenario.Settings.BaseCaseForGroups = BR.ReadBoolean
                scenario.Settings.BaseCaseForMustNots = BR.ReadBoolean
                scenario.Settings.BaseCaseForMusts = BR.ReadBoolean

                scenario.Settings.CustomConstraints = BR.ReadBoolean
                scenario.Settings.Dependencies = BR.ReadBoolean
                scenario.Settings.FundingPools = BR.ReadBoolean
                scenario.Settings.Groups = BR.ReadBoolean
                scenario.Settings.MustNots = BR.ReadBoolean
                scenario.Settings.Musts = BR.ReadBoolean
                scenario.Settings.Risks = BR.ReadBoolean

                scenario.Settings.UseBaseCase = BR.ReadBoolean
                scenario.Settings.UseBaseCaseOptions = BR.ReadBoolean
                scenario.Settings.UseIgnoreOptions = BR.ReadBoolean

                scenario.Settings.TimePeriods = BR.ReadBoolean

                ' alternatives
                Dim altsCount As Integer = BR.ReadInt32
                bFoundCosts = False 'AS/4-6-16
                For j As Integer = 1 To altsCount
                    Dim raAlt As New RAAlternative
                    raAlt.ID = BR.ReadString
                    raAlt.Name = BR.ReadString
                    raAlt.BenefitOriginal = BR.ReadDouble
                    raAlt.Cost = BR.ReadDouble
                    If i = 1 Then 'defulat scenario 'AS/4-6-16===
                        ReDim Preserve costs(j)
                        costs(j) = raAlt.Cost
                    Else
                        If raAlt.Cost <> 0 Then
                            bFoundCosts = True
                        End If
                    End If 'AS/4-6-16==
                    raAlt.IsPartial = BR.ReadBoolean
                    raAlt.MinPercent = BR.ReadDouble
                    raAlt.Must = BR.ReadBoolean
                    raAlt.MustNot = BR.ReadBoolean
                    raAlt.RiskOriginal = BR.ReadDouble
                    raAlt.SBPriority = BR.ReadDouble
                    raAlt.SBTotal = BR.ReadDouble
                    raAlt.SortOrder = BR.ReadInt32
                    raAlt.Enabled = BR.ReadBoolean

                    scenario.AlternativesFull.Add(raAlt)
                Next

                If i > 1 Then 'do for scenarios other than default
                    If Not bFoundCosts Then 'AS/4-6-16===
                        For j As Integer = 1 To altsCount
                            scenario.AlternativesFull(j - 1).Cost = costs(j)    ' D3881
                        Next
                    End If
                End If 'AS/4-6-16==

                Dim groupsCount As Integer = BR.ReadInt32
                For j As Integer = 1 To groupsCount
                    Dim group As New RAGroup
                    group.ID = BR.ReadString
                    group.IntID = BR.ReadInt32
                    group.Name = BR.ReadString
                    group.Enabled = BR.ReadBoolean
                    group.Condition = CType(BR.ReadInt32, RAGroupCondition)

                    Dim aCount As Integer = BR.ReadInt32
                    For k As Integer = 1 To aCount
                        Dim altID As String = BR.ReadString
                        Dim alt As RAAlternative = scenario.AlternativesFull.Find(Function(a) (a.ID.ToLower = altID.ToLower))   ' D3881
                        If alt IsNot Nothing Then
                            group.Alternatives.Add(alt.ID, alt)
                        End If
                    Next

                    scenario.Groups.Groups.Add(group.ID, group)
                Next

                ' new to 1.1.43 - event groups
                groupsCount = BR.ReadInt32
                For j As Integer = 1 To groupsCount
                    Dim group As New RAGroup
                    group.ID = BR.ReadString
                    group.IntID = BR.ReadInt32
                    group.Name = BR.ReadString
                    group.Enabled = BR.ReadBoolean
                    group.Condition = CType(BR.ReadInt32, RAGroupCondition)

                    Dim aCount As Integer = BR.ReadInt32
                    For k As Integer = 1 To aCount
                        Dim altID As String = BR.ReadString
                        '-A1411 Dim alt As RAAlternative = scenario.AlternativesFull.Find(Function(a) (a.ID.ToLower = altID.ToLower))
                        'If alt IsNot Nothing Then
                        '    group.Alternatives.Add(alt.ID, alt)
                        'End If
                        'A1411 ===
                        Dim alt As RAAlternative = New RAAlternative With {.ID = altID}
                        group.Alternatives.Add(alt.ID, alt)
                        'A1411 ==
                    Next

                    scenario.EventGroups.Groups.Add(group.ID, group)
                Next

                ' dependencies
                Dim dCount As Integer = BR.ReadInt32
                For j As Integer = 1 To dCount
                    Dim FirstAlternativeID As String = BR.ReadString
                    Dim SecondAlternativeID As String = BR.ReadString
                    Dim Value As RADependencyType = CType(BR.ReadInt32, RADependencyType)

                    'new to 1.1.40
                    Dim LagCondition As LagCondition = CType(BR.ReadInt32, LagCondition)
                    Dim Lag As Integer = BR.ReadInt32
                    Dim LagUpperBound As Integer = BR.ReadInt32

                    Dim dependency As New RADependency(FirstAlternativeID, SecondAlternativeID, Value)

                    ' new to 1.1.40
                    dependency.LagCondition = LagCondition
                    dependency.Lag = Lag
                    dependency.LagUpperBound = LagUpperBound

                    If Value = RADependencyType.dtConcurrent Or Value = RADependencyType.dtSuccessive Or Value = RADependencyType.dtLag Then
                        scenario.TimePeriodsDependencies.Dependencies.Add(dependency)
                    Else
                        scenario.Dependencies.Dependencies.Add(dependency)
                    End If
                Next

                ' custom constraints
                Dim ccCount As Integer = BR.ReadInt32
                For j As Integer = 1 To ccCount
                    Dim CC As New RAConstraint
                    CC.ID = BR.ReadInt32
                    CC.Name = BR.ReadString
                    CC.Enabled = BR.ReadBoolean
                    CC.IsReadOnly = BR.ReadBoolean
                    CC.MaxValue = BR.ReadDouble
                    CC.MinValue = BR.ReadDouble
                    CC.LinkedAttributeID = New Guid(BR.ReadBytes(16))
                    CC.LinkedEnumID = New Guid(BR.ReadBytes(16))

                    ' new to 1.1.31
                    CC.ECD_AID = BR.ReadString
                    CC.ECD_AssociatedCVID = BR.ReadString
                    CC.ECD_AssociatedUDcolKey = BR.ReadString
                    CC.ECD_CCID = BR.ReadString
                    CC.ECD_SOrder = BR.ReadInt32

                    Dim adCount As Integer = BR.ReadInt32
                    For k As Integer = 1 To adCount
                        Dim aID As String = BR.ReadString
                        Dim aValue As Double = BR.ReadDouble
                        CC.AlternativesData.Add(aID, aValue)
                    Next

                    CC.IsLinkedToResource = BR.ReadBoolean

                    scenario.Constraints.Constraints.Add(CC.ID, CC)
                Next

                ' funding pools
                Dim fpCount As Integer = BR.ReadInt32
                For j As Integer = 1 To fpCount
                    Dim FP As New RAFundingPool
                    FP.ID = BR.ReadInt32
                    FP.Name = BR.ReadString
                    FP.Enabled = BR.ReadBoolean
                    FP.PoolLimit = BR.ReadDouble

                    Dim vCount As Integer = BR.ReadInt32
                    For k As Integer = 1 To vCount
                        Dim aID As String = BR.ReadString
                        Dim aValue As Double = BR.ReadDouble
                        FP.Values.Add(aID, aValue)
                    Next

                    scenario.FundingPools.Pools.Add(FP.ID, FP)
                Next

                Dim TimePeriodsSize As Long = BR.ReadInt64
                Dim TimePeriodsBytes() As Byte = BR.ReadBytes(CInt(TimePeriodsSize))
                Dim TimePeriodsStream As New MemoryStream(TimePeriodsBytes)
                scenario.TimePeriods.ParseStream_v_40(TimePeriodsStream)

                Scenarios.Scenarios.Add(scenario.ID, scenario)
            Next

            ' global settings
            Scenarios.GlobalSettings.isAutoSolve = BR.ReadBoolean
            Scenarios.GlobalSettings.Precision = BR.ReadInt32
            Scenarios.GlobalSettings.ShowFrozenHeaders = BR.ReadBoolean
            Scenarios.GlobalSettings.SortBy = CType(BR.ReadInt32, RAGlobalSettings.raColumnID)

            Scenarios.ActiveScenarioID = ActiveScenarioID

            BR.Close()

            Return True
        End Function

        Private Function ParseRAStream_1_1_45(ByVal Stream As MemoryStream) As Boolean
            Stream.Seek(0, SeekOrigin.Begin)

            Dim BR As New BinaryReader(Stream)

            Scenarios.Clear()

            Dim ScenariosCount As Integer = BR.ReadInt32
            Dim ActiveScenarioID As Integer = BR.ReadInt32

            ' v1.1.39
            CombinedGroupUserID = BR.ReadInt32
            If CombinedGroupUserID >= 0 Then
                Dim CG As clsCombinedGroup = CType(ProjectManager.CombinedGroups.GetGroupByID(CombinedGroupUserID), clsCombinedGroup)
                If CG IsNot Nothing Then CombinedGroupUserID = CG.CombinedUserID Else CombinedGroupUserID = COMBINED_USER_ID
            End If

            Dim bFoundCosts As Boolean 'AS/4-6-16
            Dim costs() As Double 'AS/4-6-16 keep costs for the default scenario 

            For i As Integer = 1 To ScenariosCount
                Dim scenario As New RAScenario(Scenarios)

                scenario.ID = BR.ReadInt32
                scenario.Name = BR.ReadString
                scenario.Description = BR.ReadString
                scenario.Index = BR.ReadInt32
                scenario.IsCheckedCS = BR.ReadBoolean
                scenario.IsCheckedIB = BR.ReadBoolean
                scenario.Budget = BR.ReadDouble

                ' v1.1.39
                scenario.CombinedGroupUserID = BR.ReadInt32
                If scenario.CombinedGroupUserID >= 0 Then
                    Dim CG As clsCombinedGroup = CType(ProjectManager.CombinedGroups.GetGroupByID(scenario.CombinedGroupUserID), clsCombinedGroup)
                    If CG IsNot Nothing Then scenario.CombinedGroupUserID = CG.CombinedUserID Else scenario.CombinedGroupUserID = COMBINED_USER_ID
                End If

                ' settings
                scenario.Settings.BaseCaseForConstraints = BR.ReadBoolean
                scenario.Settings.BaseCaseForDependencies = BR.ReadBoolean
                scenario.Settings.BaseCaseForFundingPools = BR.ReadBoolean
                scenario.Settings.BaseCaseForGroups = BR.ReadBoolean
                scenario.Settings.BaseCaseForMustNots = BR.ReadBoolean
                scenario.Settings.BaseCaseForMusts = BR.ReadBoolean

                scenario.Settings.CustomConstraints = BR.ReadBoolean
                scenario.Settings.Dependencies = BR.ReadBoolean
                scenario.Settings.FundingPools = BR.ReadBoolean
                scenario.Settings.Groups = BR.ReadBoolean
                scenario.Settings.MustNots = BR.ReadBoolean
                scenario.Settings.Musts = BR.ReadBoolean
                scenario.Settings.Risks = BR.ReadBoolean

                scenario.Settings.UseBaseCase = BR.ReadBoolean
                scenario.Settings.UseBaseCaseOptions = BR.ReadBoolean
                scenario.Settings.UseIgnoreOptions = BR.ReadBoolean

                scenario.Settings.TimePeriods = BR.ReadBoolean

                ' new to 1.1.45
                scenario.Settings.ResourcesMin = BR.ReadBoolean
                scenario.Settings.ResourcesMax = BR.ReadBoolean

                ' alternatives
                Dim altsCount As Integer = BR.ReadInt32
                bFoundCosts = False 'AS/4-6-16
                For j As Integer = 1 To altsCount
                    Dim raAlt As New RAAlternative
                    raAlt.ID = BR.ReadString
                    raAlt.Name = BR.ReadString
                    raAlt.BenefitOriginal = BR.ReadDouble
                    raAlt.Cost = BR.ReadDouble
                    If i = 1 Then 'defulat scenario 'AS/4-6-16===
                        ReDim Preserve costs(j)
                        costs(j) = raAlt.Cost
                    Else
                        If raAlt.Cost <> 0 Then
                            bFoundCosts = True
                        End If
                    End If 'AS/4-6-16==
                    raAlt.IsPartial = BR.ReadBoolean
                    raAlt.MinPercent = BR.ReadDouble
                    raAlt.Must = BR.ReadBoolean
                    raAlt.MustNot = BR.ReadBoolean
                    raAlt.RiskOriginal = BR.ReadDouble
                    raAlt.SBPriority = BR.ReadDouble
                    raAlt.SBTotal = BR.ReadDouble
                    raAlt.SortOrder = BR.ReadInt32
                    raAlt.Enabled = BR.ReadBoolean

                    scenario.AlternativesFull.Add(raAlt)
                Next

                If i > 1 Then 'do for scenarios other than default
                    If Not bFoundCosts Then 'AS/4-6-16===
                        For j As Integer = 1 To altsCount
                            scenario.AlternativesFull(j - 1).Cost = costs(j)    ' D3881
                        Next
                    End If
                End If 'AS/4-6-16==

                Dim groupsCount As Integer = BR.ReadInt32
                For j As Integer = 1 To groupsCount
                    Dim group As New RAGroup
                    group.ID = BR.ReadString
                    group.IntID = BR.ReadInt32
                    group.Name = BR.ReadString
                    group.Enabled = BR.ReadBoolean
                    group.Condition = CType(BR.ReadInt32, RAGroupCondition)

                    Dim aCount As Integer = BR.ReadInt32
                    For k As Integer = 1 To aCount
                        Dim altID As String = BR.ReadString
                        Dim alt As RAAlternative = scenario.AlternativesFull.Find(Function(a) (a.ID.ToLower = altID.ToLower))   ' D3881
                        If alt IsNot Nothing Then
                            group.Alternatives.Add(alt.ID, alt)
                        End If
                    Next

                    scenario.Groups.Groups.Add(group.ID, group)
                Next

                ' new to 1.1.43 - event groups
                groupsCount = BR.ReadInt32
                For j As Integer = 1 To groupsCount
                    Dim group As New RAGroup
                    group.ID = BR.ReadString
                    group.IntID = BR.ReadInt32
                    group.Name = BR.ReadString
                    group.Enabled = BR.ReadBoolean
                    group.Condition = CType(BR.ReadInt32, RAGroupCondition)

                    Dim aCount As Integer = BR.ReadInt32
                    For k As Integer = 1 To aCount
                        Dim altID As String = BR.ReadString
                        '-A1411 Dim alt As RAAlternative = scenario.AlternativesFull.Find(Function(a) (a.ID.ToLower = altID.ToLower))
                        'If alt IsNot Nothing Then
                        '    group.Alternatives.Add(alt.ID, alt)
                        'End If
                        'A1411 ===
                        Dim alt As RAAlternative = New RAAlternative With {.ID = altID}
                        group.Alternatives.Add(alt.ID, alt)
                        'A1411 ==
                    Next

                    scenario.EventGroups.Groups.Add(group.ID, group)
                Next

                ' dependencies
                Dim dCount As Integer = BR.ReadInt32
                For j As Integer = 1 To dCount
                    Dim FirstAlternativeID As String = BR.ReadString
                    Dim SecondAlternativeID As String = BR.ReadString
                    Dim Value As RADependencyType = CType(BR.ReadInt32, RADependencyType)

                    'new to 1.1.40
                    Dim LagCondition As LagCondition = CType(BR.ReadInt32, LagCondition)
                    Dim Lag As Integer = BR.ReadInt32
                    Dim LagUpperBound As Integer = BR.ReadInt32

                    Dim dependency As New RADependency(FirstAlternativeID, SecondAlternativeID, Value)

                    ' new to 1.1.40
                    dependency.LagCondition = LagCondition
                    dependency.Lag = Lag
                    dependency.LagUpperBound = LagUpperBound

                    If Value = RADependencyType.dtConcurrent Or Value = RADependencyType.dtSuccessive Or Value = RADependencyType.dtLag Then
                        scenario.TimePeriodsDependencies.Dependencies.Add(dependency)
                    Else
                        scenario.Dependencies.Dependencies.Add(dependency)
                    End If
                Next

                ' custom constraints
                Dim ccCount As Integer = BR.ReadInt32
                For j As Integer = 1 To ccCount
                    Dim CC As New RAConstraint
                    CC.ID = BR.ReadInt32
                    CC.Name = BR.ReadString
                    CC.Enabled = BR.ReadBoolean
                    CC.IsReadOnly = BR.ReadBoolean
                    CC.MaxValue = BR.ReadDouble
                    CC.MinValue = BR.ReadDouble
                    CC.LinkedAttributeID = New Guid(BR.ReadBytes(16))
                    CC.LinkedEnumID = New Guid(BR.ReadBytes(16))

                    ' new to 1.1.31
                    CC.ECD_AID = BR.ReadString
                    CC.ECD_AssociatedCVID = BR.ReadString
                    CC.ECD_AssociatedUDcolKey = BR.ReadString
                    CC.ECD_CCID = BR.ReadString
                    CC.ECD_SOrder = BR.ReadInt32

                    Dim adCount As Integer = BR.ReadInt32
                    For k As Integer = 1 To adCount
                        Dim aID As String = BR.ReadString
                        Dim aValue As Double = BR.ReadDouble
                        CC.AlternativesData.Add(aID, aValue)
                    Next

                    CC.IsLinkedToResource = BR.ReadBoolean

                    ' new to 1.1.45
                    CC.LinkSourceID = New Guid(BR.ReadBytes(16))
                    CC.LinkSourceMode = BR.ReadInt32

                    scenario.Constraints.Constraints.Add(CC.ID, CC)
                Next

                ' funding pools
                Dim fpCount As Integer = BR.ReadInt32
                For j As Integer = 1 To fpCount
                    Dim FP As New RAFundingPool
                    FP.ID = BR.ReadInt32
                    FP.Name = BR.ReadString
                    FP.Enabled = BR.ReadBoolean
                    FP.PoolLimit = BR.ReadDouble

                    Dim vCount As Integer = BR.ReadInt32
                    For k As Integer = 1 To vCount
                        Dim aID As String = BR.ReadString
                        Dim aValue As Double = BR.ReadDouble
                        FP.Values.Add(aID, aValue)
                    Next

                    scenario.FundingPools.Pools.Add(FP.ID, FP)
                Next

                Dim TimePeriodsSize As Long = BR.ReadInt64
                Dim TimePeriodsBytes() As Byte = BR.ReadBytes(CInt(TimePeriodsSize))
                Dim TimePeriodsStream As New MemoryStream(TimePeriodsBytes)
                scenario.TimePeriods.ParseStream_v_40(TimePeriodsStream)

                Scenarios.Scenarios.Add(scenario.ID, scenario)
            Next

            ' global settings
            Scenarios.GlobalSettings.isAutoSolve = BR.ReadBoolean
            Scenarios.GlobalSettings.Precision = BR.ReadInt32
            Scenarios.GlobalSettings.ShowFrozenHeaders = BR.ReadBoolean
            Scenarios.GlobalSettings.SortBy = CType(BR.ReadInt32, RAGlobalSettings.raColumnID)

            Scenarios.ActiveScenarioID = ActiveScenarioID

            BR.Close()

            Return True
        End Function

        Private Function ParseRAStream_1_1_46(ByVal Stream As MemoryStream) As Boolean
            Stream.Seek(0, SeekOrigin.Begin)

            Dim BR As New BinaryReader(Stream)

            Scenarios.Clear()

            Dim ScenariosCount As Integer = BR.ReadInt32
            Dim ActiveScenarioID As Integer = BR.ReadInt32

            ' v1.1.39
            CombinedGroupUserID = BR.ReadInt32
            If CombinedGroupUserID >= 0 Then
                Dim CG As clsCombinedGroup = CType(ProjectManager.CombinedGroups.GetGroupByID(CombinedGroupUserID), clsCombinedGroup)
                If CG IsNot Nothing Then CombinedGroupUserID = CG.CombinedUserID Else CombinedGroupUserID = COMBINED_USER_ID
            End If

            Dim bFoundCosts As Boolean 'AS/4-6-16
            Dim costs() As Double 'AS/4-6-16 keep costs for the default scenario 

            For i As Integer = 1 To ScenariosCount
                Dim scenario As New RAScenario(Scenarios)

                scenario.ID = BR.ReadInt32
                scenario.Name = BR.ReadString
                scenario.Description = BR.ReadString
                scenario.Index = BR.ReadInt32
                scenario.IsCheckedCS = BR.ReadBoolean
                scenario.IsCheckedIB = BR.ReadBoolean
                scenario.Budget = BR.ReadDouble

                ' v1.1.39
                scenario.CombinedGroupUserID = BR.ReadInt32
                If scenario.CombinedGroupUserID >= 0 Then
                    Dim CG As clsCombinedGroup = CType(ProjectManager.CombinedGroups.GetGroupByID(scenario.CombinedGroupUserID), clsCombinedGroup)
                    If CG IsNot Nothing Then scenario.CombinedGroupUserID = CG.CombinedUserID Else scenario.CombinedGroupUserID = COMBINED_USER_ID
                End If

                ' settings
                scenario.Settings.BaseCaseForConstraints = BR.ReadBoolean
                scenario.Settings.BaseCaseForDependencies = BR.ReadBoolean
                scenario.Settings.BaseCaseForFundingPools = BR.ReadBoolean
                scenario.Settings.BaseCaseForGroups = BR.ReadBoolean
                scenario.Settings.BaseCaseForMustNots = BR.ReadBoolean
                scenario.Settings.BaseCaseForMusts = BR.ReadBoolean

                scenario.Settings.CustomConstraints = BR.ReadBoolean
                scenario.Settings.Dependencies = BR.ReadBoolean
                scenario.Settings.FundingPools = BR.ReadBoolean
                scenario.Settings.Groups = BR.ReadBoolean
                scenario.Settings.MustNots = BR.ReadBoolean
                scenario.Settings.Musts = BR.ReadBoolean
                scenario.Settings.Risks = BR.ReadBoolean

                scenario.Settings.UseBaseCase = BR.ReadBoolean
                scenario.Settings.UseBaseCaseOptions = BR.ReadBoolean
                scenario.Settings.UseIgnoreOptions = BR.ReadBoolean

                scenario.Settings.TimePeriods = BR.ReadBoolean

                ' new to 1.1.45
                scenario.Settings.ResourcesMin = BR.ReadBoolean
                scenario.Settings.ResourcesMax = BR.ReadBoolean

                ' new to 1.1.46
                scenario.Settings.CostTolerance = BR.ReadBoolean

                ' alternatives
                Dim altsCount As Integer = BR.ReadInt32
                bFoundCosts = False 'AS/4-6-16
                For j As Integer = 1 To altsCount
                    Dim raAlt As New RAAlternative
                    raAlt.ID = BR.ReadString
                    raAlt.Name = BR.ReadString
                    raAlt.BenefitOriginal = BR.ReadDouble
                    raAlt.Cost = BR.ReadDouble
                    If i = 1 Then 'defulat scenario 'AS/4-6-16===
                        ReDim Preserve costs(j)
                        costs(j) = raAlt.Cost
                    Else
                        If raAlt.Cost <> 0 Then
                            bFoundCosts = True
                        End If
                    End If 'AS/4-6-16==
                    raAlt.IsPartial = BR.ReadBoolean
                    raAlt.MinPercent = BR.ReadDouble
                    raAlt.Must = BR.ReadBoolean
                    raAlt.MustNot = BR.ReadBoolean
                    raAlt.RiskOriginal = BR.ReadDouble
                    raAlt.SBPriority = BR.ReadDouble
                    raAlt.SBTotal = BR.ReadDouble
                    raAlt.SortOrder = BR.ReadInt32
                    raAlt.Enabled = BR.ReadBoolean

                    scenario.AlternativesFull.Add(raAlt)
                Next

                If i > 1 Then 'do for scenarios other than default
                    If Not bFoundCosts Then 'AS/4-6-16===
                        For j As Integer = 1 To altsCount
                            scenario.AlternativesFull(j - 1).Cost = costs(j)    ' D3881
                        Next
                    End If
                End If 'AS/4-6-16==

                Dim groupsCount As Integer = BR.ReadInt32
                For j As Integer = 1 To groupsCount
                    Dim group As New RAGroup
                    group.ID = BR.ReadString
                    group.IntID = BR.ReadInt32
                    group.Name = BR.ReadString
                    group.Enabled = BR.ReadBoolean
                    group.Condition = CType(BR.ReadInt32, RAGroupCondition)

                    Dim aCount As Integer = BR.ReadInt32
                    For k As Integer = 1 To aCount
                        Dim altID As String = BR.ReadString
                        Dim alt As RAAlternative = scenario.AlternativesFull.Find(Function(a) (a.ID.ToLower = altID.ToLower))   ' D3881
                        If alt IsNot Nothing Then
                            group.Alternatives.Add(alt.ID, alt)
                        End If
                    Next

                    scenario.Groups.Groups.Add(group.ID, group)
                Next

                ' new to 1.1.43 - event groups
                groupsCount = BR.ReadInt32
                For j As Integer = 1 To groupsCount
                    Dim group As New RAGroup
                    group.ID = BR.ReadString
                    group.IntID = BR.ReadInt32
                    group.Name = BR.ReadString
                    group.Enabled = BR.ReadBoolean
                    group.Condition = CType(BR.ReadInt32, RAGroupCondition)

                    Dim aCount As Integer = BR.ReadInt32
                    For k As Integer = 1 To aCount
                        Dim altID As String = BR.ReadString
                        '-A1411 Dim alt As RAAlternative = scenario.AlternativesFull.Find(Function(a) (a.ID.ToLower = altID.ToLower))
                        'If alt IsNot Nothing Then
                        '    group.Alternatives.Add(alt.ID, alt)
                        'End If
                        'A1411 ===
                        Dim alt As RAAlternative = New RAAlternative With {.ID = altID}
                        group.Alternatives.Add(alt.ID, alt)
                        'A1411 ==
                    Next

                    scenario.EventGroups.Groups.Add(group.ID, group)
                Next

                ' dependencies
                Dim dCount As Integer = BR.ReadInt32
                For j As Integer = 1 To dCount
                    Dim FirstAlternativeID As String = BR.ReadString
                    Dim SecondAlternativeID As String = BR.ReadString
                    Dim Value As RADependencyType = CType(BR.ReadInt32, RADependencyType)

                    'new to 1.1.40
                    Dim LagCondition As LagCondition = CType(BR.ReadInt32, LagCondition)
                    Dim Lag As Integer = BR.ReadInt32
                    Dim LagUpperBound As Integer = BR.ReadInt32

                    Dim dependency As New RADependency(FirstAlternativeID, SecondAlternativeID, Value)

                    ' new to 1.1.40
                    dependency.LagCondition = LagCondition
                    dependency.Lag = Lag
                    dependency.LagUpperBound = LagUpperBound

                    If (scenario.AlternativesFull.FirstOrDefault(Function(x) x.ID = FirstAlternativeID) IsNot Nothing OrElse scenario.Groups.GetGroupByID(FirstAlternativeID) IsNot Nothing) AndAlso (scenario.AlternativesFull.FirstOrDefault(Function(x) x.ID = SecondAlternativeID) IsNot Nothing  OrElse scenario.Groups.GetGroupByID(SecondAlternativeID) IsNot Nothing) Then
                        If Value = RADependencyType.dtConcurrent Or Value = RADependencyType.dtSuccessive Or Value = RADependencyType.dtLag Then
                            scenario.TimePeriodsDependencies.Dependencies.Add(dependency)
                        Else
                            scenario.Dependencies.Dependencies.Add(dependency)
                        End If
                    End If
                Next

                ' custom constraints
                Dim ccCount As Integer = BR.ReadInt32
                For j As Integer = 1 To ccCount
                    Dim CC As New RAConstraint
                    CC.ID = BR.ReadInt32
                    CC.Name = BR.ReadString
                    CC.Enabled = BR.ReadBoolean
                    CC.IsReadOnly = BR.ReadBoolean
                    CC.MaxValue = BR.ReadDouble
                    CC.MinValue = BR.ReadDouble
                    CC.LinkedAttributeID = New Guid(BR.ReadBytes(16))
                    CC.LinkedEnumID = New Guid(BR.ReadBytes(16))

                    ' new to 1.1.31
                    CC.ECD_AID = BR.ReadString
                    CC.ECD_AssociatedCVID = BR.ReadString
                    CC.ECD_AssociatedUDcolKey = BR.ReadString
                    CC.ECD_CCID = BR.ReadString
                    CC.ECD_SOrder = BR.ReadInt32

                    Dim adCount As Integer = BR.ReadInt32
                    For k As Integer = 1 To adCount
                        Dim aID As String = BR.ReadString
                        Dim aValue As Double = BR.ReadDouble
                        CC.AlternativesData.Add(aID, aValue)
                    Next

                    CC.IsLinkedToResource = BR.ReadBoolean

                    ' new to 1.1.45
                    CC.LinkSourceID = New Guid(BR.ReadBytes(16))
                    CC.LinkSourceMode = BR.ReadInt32

                    scenario.Constraints.Constraints.Add(CC.ID, CC)
                Next

                ' funding pools
                Dim fpCount As Integer = BR.ReadInt32
                For j As Integer = 1 To fpCount
                    Dim FP As New RAFundingPool
                    FP.ID = BR.ReadInt32
                    FP.Name = BR.ReadString
                    FP.Enabled = BR.ReadBoolean
                    FP.PoolLimit = BR.ReadDouble

                    Dim vCount As Integer = BR.ReadInt32
                    For k As Integer = 1 To vCount
                        Dim aID As String = BR.ReadString
                        Dim aValue As Double = BR.ReadDouble
                        FP.Values.Add(aID, aValue)
                    Next

                    scenario.FundingPools.Pools.Add(FP.ID, FP)
                Next

                scenario.FundingPools.ResourceID = RA_Cost_GUID
                scenario.ResourcePools.Clear()
                scenario.ResourcePools.Add(RA_Cost_GUID, scenario.FundingPools)

                Dim TimePeriodsSize As Long = BR.ReadInt64
                Dim TimePeriodsBytes() As Byte = BR.ReadBytes(CInt(TimePeriodsSize))
                Dim TimePeriodsStream As New MemoryStream(TimePeriodsBytes)
                scenario.TimePeriods.ParseStream_v_40(TimePeriodsStream)

                Scenarios.Scenarios.Add(scenario.ID, scenario)
            Next

            ' global settings
            Scenarios.GlobalSettings.isAutoSolve = BR.ReadBoolean
            Scenarios.GlobalSettings.Precision = BR.ReadInt32
            Scenarios.GlobalSettings.ShowFrozenHeaders = BR.ReadBoolean
            Scenarios.GlobalSettings.SortBy = CType(BR.ReadInt32, RAGlobalSettings.raColumnID)

            Scenarios.ActiveScenarioID = ActiveScenarioID

            If BR.BaseStream.Position < BR.BaseStream.Length - 1 Then
                Dim sCount As Integer = BR.ReadInt32
                If sCount <> UNDEFINED_INTEGER_VALUE Then
                    For i As Integer = 1 To sCount
                        Dim sID As Integer = BR.ReadInt32
                        Dim scenario As RAScenario = Scenarios.GetScenarioById(sID)
                        ' funding pools with priorities
                        scenario.FundingPools.Pools.Clear()
                        Dim fpCount As Integer = BR.ReadInt32
                        For j As Integer = 1 To fpCount
                            Dim FP As New RAFundingPool
                            FP.ID = BR.ReadInt32
                            FP.Name = BR.ReadString
                            FP.Enabled = BR.ReadBoolean
                            FP.PoolLimit = BR.ReadDouble

                            Dim vCount As Integer = BR.ReadInt32
                            For k As Integer = 1 To vCount
                                Dim aID As String = BR.ReadString
                                Dim aValue As Double = BR.ReadDouble
                                FP.Values.Add(aID, aValue)
                            Next

                            vCount = BR.ReadInt32
                            For k As Integer = 1 To vCount
                                Dim aID As String = BR.ReadString
                                Dim aValue As Double = BR.ReadDouble
                                FP.Priorities.Add(aID, aValue)
                            Next

                            scenario.FundingPools.Pools.Add(FP.ID, FP)
                        Next
                    Next
                Else
                    Dim ScCount As Integer = BR.ReadInt32
                    For i As Integer = 1 To ScCount
                        Dim sID As Integer = BR.ReadInt32
                        Dim scenario As RAScenario = Scenarios.GetScenarioById(sID)
                        scenario.ResourcePools.Clear()
                        Dim rpCount As Integer = BR.ReadInt32
                        For j As Integer = 1 To rpCount
                            Dim resourceID As Guid = New Guid(BR.ReadBytes(16))
                            Dim FPools As New RAFundingPools
                            FPools.ResourceID = resourceID
                            Dim fpCount As Integer = BR.ReadInt32
                            For j1 As Integer = 1 To fpCount
                                Dim FP As New RAFundingPool
                                FP.ID = BR.ReadInt32
                                FP.Name = BR.ReadString
                                FP.Enabled = BR.ReadBoolean
                                FP.PoolLimit = BR.ReadDouble

                                Dim vCount As Integer = BR.ReadInt32
                                For k1 As Integer = 1 To vCount
                                    Dim aID As String = BR.ReadString
                                    Dim aValue As Double = BR.ReadDouble
                                    FP.Values.Add(aID, aValue)
                                Next

                                vCount = BR.ReadInt32
                                For k As Integer = 1 To vCount
                                    Dim aID As String = BR.ReadString
                                    Dim aValue As Double = BR.ReadDouble
                                    FP.Priorities.Add(aID, aValue)
                                Next

                                FPools.Pools.Add(FP.ID, FP)
                            Next
                            scenario.ResourcePools.Add(resourceID, FPools)
                        Next
                    Next

                End If
            End If


            BR.Close()

            Return True
        End Function

        Public Function ParseRAStream(ByVal Stream As MemoryStream) As Boolean
            If ProjectManager.StorageManager.CanvasDBVersion.MinorVersion <= 30 Then
                Return ParseRAStream_1_1_30(Stream)
            End If

            Select Case ProjectManager.StorageManager.CanvasDBVersion.MinorVersion
                Case 31 To 34
                    Return ParseRAStream_1_1_31(Stream)
                Case 35
                    Return ParseRAStream_1_1_35(Stream)
                Case 36
                    Return ParseRAStream_1_1_36(Stream)
                Case 37
                    Return ParseRAStream_1_1_37(Stream)
                Case 38
                    Return ParseRAStream_1_1_38(Stream)
                Case 39
                    Return ParseRAStream_1_1_39(Stream)
                Case 40 To 42
                    Return ParseRAStream_1_1_40(Stream)
                Case 43 To 44
                    Return ParseRAStream_1_1_43(Stream)
                Case 45
                    Return ParseRAStream_1_1_45(Stream)
                Case 46 To _DB_VERSION_MINOR
                    Return ParseRAStream_1_1_46(Stream)
                Case Else
                    Return False
            End Select
        End Function

        Private Function Save_CanvasStreamsDatabase(ByVal Location As String, ByVal ProviderType As DBProviderType, ByVal ModelID As Integer) As Boolean
            ' D4367 ===
            If RA_OPT_USE_SOLVER_PRIORITIES Then SaveSolverPriorities(False) ' D4364
            SaveFundingPoolsOrder(False)
            'ProjectManager.Parameters.Save()
            ' D4367 ==

            'Dim MS As MemoryStream = CType(Serialize(), MemoryStream)   ' D2882
            Dim MS As MemoryStream = GetRAStream()

            Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()

                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection

                oCommand.CommandText = "DELETE FROM ModelStructure WHERE ProjectID=? AND (StructureType=? or StructureType=?)"
                oCommand.Parameters.Clear()
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "StructureType1", CInt(StructureType.stResourceAligner)))
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "StructureType2", CInt(StructureType.stResourceAlignerNew)))

                Dim affected As Integer = DBExecuteNonQuery(ProviderType, oCommand)

                oCommand.CommandText = "INSERT INTO ModelStructure (ProjectID, StructureType, StreamSize, Stream) VALUES (?, ?, ?, ?)"
                oCommand.Parameters.Clear()
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "StructureType", StructureType.stResourceAlignerNew))
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "StreamSize", MS.ToArray.Length))
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "Stream", MS.ToArray))
                affected = DBExecuteNonQuery(ProviderType, oCommand)
            End Using

            Return True
        End Function

        Private Function Save_AHPDatabase(ByVal dbConnectionString As String, ByVal ProviderType As DBProviderType) As Boolean
            Return True
        End Function

        Public Function Serialize() As Stream
            Dim MS As New MemoryStream
            Dim formatter As BinaryFormatter = New BinaryFormatter()

            Dim tRealRA As ResourceAligner = Scenarios.ResourceAligner
            Scenarios.ResourceAligner = Nothing
            Scenarios.GlobalSettings.ResourceAligner = Nothing

            formatter.Serialize(MS, Scenarios)

            Scenarios.ResourceAligner = tRealRA
            Scenarios.GlobalSettings.ResourceAligner = Scenarios.ResourceAligner

            Return MS
        End Function

        Public Function Deserialize(Stream As MemoryStream) As RAScenarios
            Dim formatter As BinaryFormatter = New BinaryFormatter()
            Dim s As RAScenarios = CType(formatter.Deserialize(Stream), RAScenarios)

            For Each scenario As RAScenario In s.Scenarios.Values
                If (scenario.AlternativesFull Is Nothing) OrElse (scenario.AlternativesFull.Count = 0 And scenario.Alternatives.Count > 0) Then
                    scenario.AlternativesFull = New List(Of RAAlternative)
                    For Each alt As RAAlternative In scenario.Alternatives
                        scenario.AlternativesFull.Add(alt)
                    Next
                End If
            Next

            Return s
        End Function

        Public Sub Clear()
            Solver.ResetSolver()
            Scenarios.Clear()
        End Sub

        Public Sub UpdateBenefits()
            For Each alt As RAAlternative In Scenarios.ActiveScenario.Alternatives
                alt.Benefit = CDbl(If(Scenarios.ActiveScenario.Settings.Risks, alt.BenefitOriginal * (1 - alt.RiskOriginal), alt.BenefitOriginal)) ' D2882
            Next
        End Sub

        Public Sub SetAlternativeCost(AlternativeID As String, Value As Double, Optional PerformSave As Boolean = True) ' D3766
            If Scenarios.ActiveScenario Is Nothing Then Return
            Dim alt As RAAlternative = GetAlternativeById(Scenarios.ActiveScenario, AlternativeID)
            If alt IsNot Nothing Then
                ' D6464 + D6719 ===
                For Each RAC As KeyValuePair(Of Integer, RAConstraint) In Scenarios.ActiveScenario.Constraints.Constraints
                    If RAC.Value.isDollarValue Then
                        Dim tVal As Double = Scenarios.ActiveScenario.Constraints.GetConstraintValue(RAC.Key, alt.ID)
                        If tVal <> UNDEFINED_INTEGER_VALUE AndAlso tVal = alt.Cost Then Scenarios.ActiveScenario.Constraints.SetConstraintValue(RAC.Key, alt.ID, Value)
                    End If
                Next
                alt.Cost = Value
                ' D6718 ==
            End If
            ' D6464 ==
            'If Scenarios.ActiveScenario IsNot Nothing AndAlso Scenarios.ActiveScenario.ID = 0 Then
            '    With ProjectManager
            '        .Attributes.SetAttributeValue(ATTRIBUTE_COST_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtDouble, Value, New Guid(AlternativeID), Guid.Empty)
            '        If PerformSave Then ' D3766
            '            .Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID, UNDEFINED_USER_ID)
            '        End If
            '    End With
            'End If
        End Sub

        Public Sub SetAlternativeRisk(AlternativeID As String, Value As Double, Optional PerformSave As Boolean = True)
            If Scenarios.ActiveScenario Is Nothing Then Return
            Dim alt As RAAlternative = GetAlternativeById(Scenarios.ActiveScenario, AlternativeID)
            If alt IsNot Nothing Then
                alt.RiskOriginal = Value
                If Scenarios.ActiveScenario.Settings.Risks Then
                    alt.Benefit = alt.BenefitOriginal * (1 - alt.RiskOriginal)
                End If
            End If
            'If Scenarios.ActiveScenario IsNot Nothing AndAlso Scenarios.ActiveScenario.ID = 0 Then
            '    With ProjectManager
            '        .Attributes.SetAttributeValue(ATTRIBUTE_RISK_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtDouble, Value, New Guid(AlternativeID), Guid.Empty)
            '        If PerformSave Then
            '            .Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID, UNDEFINED_USER_ID)
            '        End If
            '    End With
            'End If
        End Sub

        ' D4359 ===
        Public Function LoadSolverPriorities() As Boolean
            Dim fResult As Boolean = False
            Dim sAllData As String = ProjectManager.Parameters.RASolverPriorities
            If sAllData <> "" Then
                Dim sLines As String() = sAllData.Split(CChar(vbNewLine))
                If sLines.Count > 1 Then
                    If sLines(0).StartsWith(RA_OPT_SOLVER_PRIORITIES_DATA_VER_PREFIX) Then
                        Dim DataVersion As Integer = 999
                        If Integer.TryParse(sLines(0).Substring(RA_OPT_SOLVER_PRIORITIES_DATA_VER_PREFIX.Length), DataVersion) Then
                            For i As Integer = 1 To sLines.Count - 1    ' since first row is Ver.
                                Dim sData As String = sLines(i)
                                Dim Index As Integer = sData.IndexOf(vbTab)
                                If Index > 0 Then
                                    Dim sID As String = sData.Substring(0, Index)
                                    Dim sPrty As String = sData.Substring(Index + 1)
                                    Dim ID As Integer
                                    If Integer.TryParse(sID, ID) AndAlso sPrty <> "" Then
                                        Dim tScen As RAScenario = Scenarios.GetScenarioById(ID)
                                        If tScen IsNot Nothing Then
                                            Dim ser As New JavaScriptSerializer()
                                            Dim Prty As List(Of RASolverPriority) = ser.Deserialize(Of List(Of RASolverPriority))(sPrty)
                                            If Prty IsNot Nothing Then tScen.SolverPriorities.Priorities = Prty
                                        End If
                                    End If
                                End If
                            Next
                            fResult = True
                        End If
                    End If
                End If
            End If
            For Each tScen As RAScenario In Scenarios.Scenarios.Values
                tScen.SolverPriorities.CheckAndSort()
            Next
            Return fResult
        End Function

        Public Function SaveSolverPriorities(SaveToDB As Boolean) As Boolean   ' D4367
            Dim fResult As Boolean = False
            ' D4364 ===
            If ProjectManager IsNot Nothing Then
                Dim sAllData As String = String.Format("{0}{1}{2}", RA_OPT_SOLVER_PRIORITIES_DATA_VER_PREFIX, RA_OPT_SOLVER_PRIORITIES_DATA_VERSION, vbNewLine)
                For Each tScen As RAScenario In Scenarios.Scenarios.Values
                    If tScen.SolverPriorities IsNot Nothing Then
                        Dim ser As New JavaScriptSerializer()
                        Dim sPrty As String = ser.Serialize(tScen.SolverPriorities.Priorities)
                        sAllData += String.Format("{0}{1}{2}{3}", tScen.ID.ToString, vbTab, sPrty, vbNewLine)
                    End If
                Next
                If ProjectManager.Parameters.RASolverPriorities <> sAllData Then
                    ProjectManager.Parameters.RASolverPriorities = sAllData
                    If SaveToDB Then fResult = ProjectManager.Parameters.Save() Else fResult = True ' D4367
                Else
                    fResult = True
                End If
            End If
            ' D4364 ==
            Return fResult
        End Function
        ' D4359 ==

        ' D4367 ===
        Public Function LoadFundingPoolsOrder() As Boolean
            Dim fResult As Boolean = False
            Dim sAllData As String = ProjectManager.Parameters.RAFundingPoolsOrder
            Dim tOrders As New Dictionary(Of Integer, String)
            If sAllData <> "" Then
                Dim sLines As String() = sAllData.Split(CChar(vbNewLine))
                For Each sData As String In sLines
                    Dim idx As Integer = sData.IndexOf(vbTab)
                    If idx > 0 Then
                        Dim ID As Integer
                        Dim sID As String = sData.Substring(0, idx)
                        Dim sOrder As String = sData.Substring(idx + 1)
                        If sOrder <> "" AndAlso Integer.TryParse(sID, ID) Then
                            Dim tScen As RAScenario = Scenarios.GetScenarioById(ID)
                            If tScen IsNot Nothing Then tOrders.Add(tScen.ID, sOrder)
                        End If
                    End If
                Next
            End If
            For Each tScen As RAScenario In Scenarios.Scenarios.Values
                Dim sOrder As String = ""
                If tOrders.ContainsKey(tScen.ID) Then sOrder = tOrders(tScen.ID)
                tScen.FundingPools.SetPoolsOrderByString(sOrder)
            Next
            Return fResult
        End Function

        Public Function SaveFundingPoolsOrder(SaveToDB As Boolean) As Boolean
            Dim fResult As Boolean = False
            If ProjectManager IsNot Nothing Then
                Dim sAllData As String = ""
                For Each tScen As RAScenario In Scenarios.Scenarios.Values
                    If tScen.FundingPools IsNot Nothing Then
                        sAllData += String.Format("{0}{1}{2}{3}", tScen.ID, vbTab, tScen.FundingPools.GetPoolsOrderAsString, vbNewLine)
                    End If
                Next
                If ProjectManager.Parameters.RAFundingPoolsOrder <> sAllData Then
                    ProjectManager.Parameters.RAFundingPoolsOrder = sAllData
                    If SaveToDB Then fResult = ProjectManager.Parameters.Save() Else fResult = True
                Else
                    fResult = True
                End If
            End If
            Return fResult
        End Function
        ' D4367 ==

        Public Function GetAlternativeById(scenario As RAScenario, altId As String) As RAAlternative 'A0914
            If scenario IsNot Nothing AndAlso scenario.AlternativesFull IsNot Nothing Then
                For Each alt As RAAlternative In scenario.AlternativesFull
                    If alt.ID.Equals(altId) Then Return alt
                Next
            End If
            Return Nothing
        End Function

        Public ReadOnly Property IsRisk As Boolean
            Get
                'Return Solver.SolverLibrary = raSolverLibrary.raBaron
                Return ProjectManager.IsRiskProject
            End Get
        End Property

        Public Sub New(PM As clsProjectManager)
            ProjectManager = PM
            Scenarios = New RAScenarios(Me)
            Solver = New RASolver(Me)
            CombinedGroupUserID = -1
            'RiskOptimizer = New RiskOptimizer(ProjectManager)
        End Sub
    End Class

End Namespace