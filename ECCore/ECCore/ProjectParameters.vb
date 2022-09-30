Imports ECCore
Imports Canvas
Imports System.Linq
Imports System.IO
Imports System.Data.Common

Namespace ECCore

    <Serializable> Public Class clsParameter
        Public Property Name As String ' UNIQUE KEY // AD: not case sensitive
        Public Property Type As System.Type
        Public Property Description As String = ""
        Public Property Values As Dictionary(Of Integer, Object) ' D3786

        Public Sub New(Name As String, Type As System.Type, Optional Description As String = "")    ' D3786
            Me.Name = Name ' D4786
            Me.Type = Type
            Me.Values = New Dictionary(Of Integer, Object)  ' D3786
            If Description Is Nothing Then Description = "" ' D3786
            Me.Description = Description
        End Sub
    End Class

    <Serializable> Public Class clsProjectParameters
        Public Property Parameters As Dictionary(Of String, clsParameter)

        Private mProjectManager As clsProjectManager
        Public ReadOnly Property ProjectManager As clsProjectManager
            Get
                Return mProjectManager
            End Get
        End Property

        ' D3786 ===
        Public ReadOnly Property ParameterSetID As Integer
            Get
                If ProjectManager IsNot Nothing Then Return ProjectManager.PipeParameters.CurrentParameterSet.ID Else Return -1
            End Get
        End Property

        Public Function AddParameter(Name As String, Type As System.Type, Optional Description As String = "") As clsParameter
            Dim tParam As clsParameter = GetParameterByName(Name)
            If tParam Is Nothing Then
                tParam = New clsParameter(Name, Type, Description)
                Parameters.Add(Name, tParam)
            End If
            Return tParam
            ' D3786 ==
        End Function

        Public Function GetParameterByName(Name As String) As clsParameter
            ' D3786 ===
            Name = Name.ToLower
            For Each sKey As String In Parameters.Keys
                If sKey.ToLower = Name Then Return Parameters(sKey)
            Next
            Return Nothing
            ' D3786 ==
        End Function

        Public Function DeleteParameter(Name As String) As Boolean
            Dim parameter As clsParameter = GetParameterByName(Name) ' D3786
            If parameter IsNot Nothing Then
                Parameters.Remove(parameter.Name)   ' D3786
                Return True
            Else
                Return False
            End If
        End Function

        ' D3786 ===
        Public Function GetParameter(Name As String, ParamSetID As Integer) As Object
            Dim tParam As clsParameter = GetParameterByName(Name)
            If tParam IsNot Nothing Then
                If tParam.Values.ContainsKey(ParamSetID) Then
                    Return tParam.Values(ParamSetID)
                End If
            End If
            Return Nothing
        End Function

        Private Function GetParameterValue(ParameterName As String) As Object
            Dim tVal As Object = GetParameter(ParameterName, ParameterSetID)
            If tVal Is Nothing AndAlso ParameterSetID <> -1 Then tVal = GetParameter(ParameterName, -1)
            Return tVal
        End Function

        Public Function GetParameterValue(ParameterName As String, DefaultValue As Integer) As Integer
            Dim value As Object = GetParameterValue(ParameterName)
            If value Is Nothing Then Return DefaultValue Else Return CInt(value)
        End Function

        Public Function GetParameterValue(ParameterName As String, DefaultValue As Double) As Double
            Dim value As Object = GetParameterValue(ParameterName)
            If value Is Nothing Then Return DefaultValue Else Return CDbl(value)
        End Function

        Public Function GetParameterValue(ParameterName As String, DefaultValue As String) As String
            Dim value As Object = GetParameterValue(ParameterName)
            If value Is Nothing Then Return DefaultValue Else Return CStr(value)
        End Function

        Public Function GetParameterValue(ParameterName As String, DefaultValue As Boolean) As Boolean
            Dim value As Object = GetParameterValue(ParameterName)
            If value Is Nothing Then Return DefaultValue Else Return CBool(value)
        End Function

        Public Function SetParameterValue(ParameterName As String, Value As Boolean, Optional ParamSetID As Integer = -1, Optional Description As String = "")    ' D3786
            Dim parameter As clsParameter = GetParameterByName(ParameterName)
            If parameter Is Nothing Then AddParameter(ParameterName, GetType(Boolean), Description)
            Return SetParameter(ParameterName, Value, ParamSetID)
        End Function

        Public Function SetParameterValue(ParameterName As String, Value As Integer, Optional ParamSetID As Integer = -1, Optional Description As String = "")    ' D3786
            Dim parameter As clsParameter = GetParameterByName(ParameterName)
            If parameter Is Nothing Then AddParameter(ParameterName, GetType(Integer), Description)
            Return SetParameter(ParameterName, Value, ParamSetID)
        End Function

        Public Function SetParameterValue(ParameterName As String, Value As Double, Optional ParamSetID As Integer = -1, Optional Description As String = "")    ' D3786
            Dim parameter As clsParameter = GetParameterByName(ParameterName)
            If parameter Is Nothing Then AddParameter(ParameterName, GetType(Double), Description)
            Return SetParameter(ParameterName, Value, ParamSetID)
        End Function

        Public Function SetParameterValue(ParameterName As String, Value As String, Optional ParamSetID As Integer = -1, Optional Description As String = "")    ' D3786
            Dim parameter As clsParameter = GetParameterByName(ParameterName)
            If parameter Is Nothing Then AddParameter(ParameterName, GetType(String), Description)
            Return SetParameter(ParameterName, Value, ParamSetID)
        End Function

        Public Function SetParameter(ParameterName As String, Value As Object, Optional ParamSetID As Integer = -1) As Boolean
            Dim fChanged As Boolean = False
            Dim tParam As clsParameter = GetParameterByName(ParameterName)    ' D3786
            If tParam IsNot Nothing AndAlso Value IsNot Nothing AndAlso tParam.Type Is Value.GetType Then
                If tParam.Values.ContainsKey(ParamSetID) Then
                    If tParam.Values(ParamSetID) <> Value Then
                        tParam.Values(ParamSetID) = Value
                        fChanged = True
                    End If
                Else
                    tParam.Values.Add(ParamSetID, Value)
                    fChanged = True
                End If
            End If
            Return fChanged
        End Function

        Private Function GetParametersStream() As MemoryStream
            Dim MS As New MemoryStream
            Dim BW As New BinaryWriter(MS)

            BW.Write(Parameters.Count)
            For Each tParam As clsParameter In Parameters.Values
                BW.Write(tParam.Name)
                BW.Write(tParam.Description)
                BW.Write(tParam.Type.ToString)
                BW.Write(tParam.Values.Count)
                For Each tVal As KeyValuePair(Of Integer, Object) In tParam.Values
                    BW.Write(tVal.Key)
                    Select Case tParam.Type
                        Case GetType(Integer)
                            BW.Write(CInt(tVal.Value))
                        Case GetType(Double)
                            BW.Write(CDbl(tVal.Value))
                        Case GetType(Boolean)
                            BW.Write(CBool(tVal.Value))
                        Case GetType(String)
                            BW.Write(CStr(tVal.Value))
                    End Select
                Next
            Next
            BW.Close()
            Return MS
        End Function

        Private Function ParseStream(Stream As MemoryStream) As Boolean
            Stream.Seek(0, SeekOrigin.Begin)
            Dim BR As New BinaryReader(Stream)

            Parameters.Clear()

            Dim count As Integer = BR.ReadInt32
            For i As Integer = 1 To count
                Dim name As String = BR.ReadString
                Dim description As String = BR.ReadString
                Dim typeName As String = BR.ReadString
                Dim type As Type = Type.GetType(typeName)
                Dim tParam As clsParameter = AddParameter(name, type, description)

                Dim ValsCount As Integer = BR.ReadInt32
                For j As Integer = 1 To ValsCount
                    Dim SetID As Integer = BR.ReadInt32
                    Dim value As Object = Nothing
                    Select Case type
                        Case GetType(Integer)
                            value = BR.ReadInt32
                        Case GetType(Double)
                            value = BR.ReadDouble
                        Case GetType(Boolean)
                            value = BR.ReadBoolean
                        Case GetType(String)
                            value = BR.ReadString
                    End Select
                    If tParam IsNot Nothing AndAlso value IsNot Nothing Then tParam.Values.Add(SetID, value)
                Next
            Next

            BR.Close()
            Return True
        End Function
        ' D3786 ==

        Private Function Load_CanvasStreamsDatabase(ByVal Location As String, ByVal ProviderType As DBProviderType, ByVal ModelID As Integer) As Boolean
            Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()

                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection

                oCommand.CommandText = "SELECT * FROM ModelStructure WHERE ProjectID=? AND StructureType=?"
                oCommand.Parameters.Clear()
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "StructureType", CInt(StructureType.stProjectParameters)))

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
                    ParseStream(MS)
                Else
                    dbReader.Close()
                    dbConnection.Close()
                    Return False
                End If
            End Using

            Return True
        End Function

        Public Overloads Function Load(ByVal StorageType As ECModelStorageType, ByVal Location As String, ByVal ProviderType As DBProviderType, ByVal ModelID As Integer) As Boolean
            Select Case StorageType
                Case ECModelStorageType.mstCanvasStreamDatabase
                    Return Load_CanvasStreamsDatabase(Location, ProviderType, ModelID)
            End Select
            Return False
        End Function

        Public Overloads Function Load() As Boolean
            With ProjectManager.StorageManager
                If .StorageType = ECModelStorageType.mstCanvasStreamDatabase Then
                    Return Load(.StorageType, .ProjectLocation, .ProviderType, .ModelID)
                End If
            End With
            Return False    ' D4364
        End Function

        Private Function Save_CanvasStreamsDatabase(ByVal Location As String, ByVal ProviderType As DBProviderType, ByVal ModelID As Integer) As Boolean
            Dim MS As MemoryStream = GetParametersStream()

            Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()

                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection

                ' D3786 ===
                oCommand.CommandText = "DELETE FROM ModelStructure WHERE ProjectID=? AND StructureType=?"
                oCommand.Parameters.Clear()
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "StructureType", CInt(StructureType.stProjectParameters)))
                Dim affected As Integer = DBExecuteNonQuery(ProviderType, oCommand)
                ' D3786 ==

                oCommand.CommandText = "INSERT INTO ModelStructure (ProjectID, StructureType, StreamSize, Stream) VALUES (?, ?, ?, ?)"
                oCommand.Parameters.Clear()
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "StructureType", CInt(StructureType.stProjectParameters)))
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "StreamSize", MS.ToArray.Length))
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "Stream", MS.ToArray))
                affected = DBExecuteNonQuery(ProviderType, oCommand)
            End Using

            Return True
        End Function

        Public Function Save() As Boolean
            With ProjectManager.StorageManager
                Select Case .StorageType
                    Case ECModelStorageType.mstCanvasStreamDatabase
                        Return Save_CanvasStreamsDatabase(.ProjectLocation, .ProviderType, .ModelID)
                End Select
            End With
            Return False    ' D4364
        End Function

        Public Function Save(ByVal StorageType As ECModelStorageType, ByVal Location As String, ByVal ProviderType As DBProviderType, ByVal ModelID As Integer) As Boolean
            Select Case StorageType
                Case ECModelStorageType.mstCanvasStreamDatabase
                    Return Save_CanvasStreamsDatabase(Location, ProviderType, ModelID)
            End Select
            Return False
        End Function

        Public Sub New(ProjectManager As clsProjectManager)
            mProjectManager = ProjectManager
            Parameters = New Dictionary(Of String, clsParameter)
        End Sub

    End Class

    ' D3786 ===
    <Serializable> Public Class clsProjectParametersWithDefaults

        Private Const RESULTS_LOCAL_SHOW_IDX As String = "ResultsLocalShowIndex"
        Private Const RESULTS_GLOBAL_SHOW_IDX As String = "ResultsGlobalShowIndex"
        Private Const RESULTS_CUSTOM_COMBINED_UID As String = "ResultsCombinedUID"      ' D4376
        Private Const RESULTS_CUSTOM_COMBINED_NAME As String = "ResultsCombinedName"    ' D4376
        Private Const RESULTS_LOCAL_SHOW_BARS As String = "ResultsLocalShowBars"        ' D4561
        Private Const RESULTS_GLOBAL_SHOW_BARS As String = "ResultsGlobalShowBars"      ' D4561
        'Public Const RA_TIMEPERIODS_HAS_DATA As String = "RATimeperiodsHasData"
        Private Const RA_TIMEPERIODS_DISTRIBUTE_MODE As String = "RATimeperiodDistributeMode"
        Private Const RA_ACTIVE_SCENARIO_ID As String = "RAScenarioID"  ' D3859
        ' D3877 ===
        Private Const RA_SOLVER_ID As String = "RASolverID"
        Private Const RA_SOLVER_XA_STRATEGY As String = "RASolverXAStrategy"
        Private Const RA_SOLVER_XA_VARIATION As String = "RASolverXAVariation"
        Private Const RA_SOLVER_XA_TIMEOUT As String = "RASolverXATimeout"
        Private Const RA_SOLVER_XA_TIMEOUT_UNCHANGED As String = "RASolverXATimeoutUnchanged"
        ' D3877 ==
        Private Const RA_SOLVER_PRIORITIES As String = "RASolverPriorities"             ' D4364
        Private Const RA_FUNDING_POOLS_EXHAUSTED As String = "RAFPoolsExhausted"        ' D4365
        Private Const RA_FUNDING_POOLS_ORDER As String = "RAFPoolsOrder"                ' D4367
        Private Const RA_FUNDING_POOLS_USE_PRTY As String = "RAFPoolsUsePrty"           ' D7098

        'Private Const RA_SHOW_TIMEPERIODS As String = "RAShowTimeperiods"              ' D3905 - D3976
        Private Const TT_HIDE_OFFLINE_USERS As String = "TTHideOfflineUsers"            ' D3929
        Private Const TT_EVAL_PRG_USER As String = "TTEvalPrgUser"                        ' D4554

        Private Const EVAL_COLLPASE_MULTIPW_BARS As String = "EvalCollapseMultiPWBars"  ' D3953
        Private Const EVAL_NO_ASK_AUTOADVANCE As String = "EvalNoAskAutoAdvance"        ' D3963
        Private Const EVAL_REWARD_THANKYOU As String = "EvalRewardThankyou"             ' D3963
        Private Const EVAL_WELCOME_SURVEY_FIRST As String = "EvalWelcomeSurveyFirst"    ' D3977
        Private Const EVAL_THANKYOU_SURVEY_LAST As String = "EvalThankYouSurveyLast"    ' D3977
        Private Const EVAL_JOIN_RISKION_PIPES As String = "EvalJoinRiskionPipes"        ' D4103
        Private Const EVAL_NAV_CLOSE_WIN_AT_FINISH As String = "EvalCloseWinAtFinish"   ' D4160
        Private Const EVAL_NAV_NEXT_PASSCODE_AT_FINISH As String = "EvalNextPasscodeAtFinish"   ' D4160
        Private Const EVAL_NAV_OPEN_NEXT_PRJ As String = "EvalOpenNextProjectAtFinish"          ' D4162
        Private Const EVAL_TRY_PARSE_VALUES_FROM_NAMES As String = "EvalParseValueFromName_{0}" ' A1340
        Private Const EVAL_HIDE_INFODOC_CAPTIONS As String = "EvalHideInfodocCaptions"  ' D4713
        Private Const EVAL_EMBEDDED_CONTENT As String = "EvalEmbeddedContent"           ' D4714
        Private Const EVAL_T2S As String = "EvalT2S"                                    ' D6962
        Private Const EVAL_HIDE_LOCAL_NORMALIZATION As String = "EvalHideLocalNormalization"    ' D7556
        Private Const EVAL_HIDE_GLOBAL_NORMALIZATION As String = "EvalHideGlobalNormalization"  ' D7556

        Private Const EVAL_WORDING_TEMPLATES As String = "WordingTemplates"             ' D4413

        Private Const ALTERNATIVES_SAVED_FILTERS As String = "AlternativessSavedFilter" ' A1363

        Private Const INVITATION_CUSTOM_TEXT As String = "InvitateCustomText"           ' D4647
        Private Const INVITATION_CUSTOM_TITLE As String = "InvitateCustomTitle"         ' D4647

        Private Const NODE_VISIBLE_INDEX_MODE As String = "NodeIndexMode"               ' D4180
        Private Const NODE_INDEX_VISIBILITY As String = "NodeIndexVisible"              ' D4323
        Private Const OBJECTIVE_INDEX_VISIBILITY As String = "ObjIndexVisible"
        'Private Const NODE_INDEX_PLAIN_VIEW As String = "NodeIndexPlainView"            ' D4323

        Private Const RISK_CONTROLS_SHOW_INFODOC As String = "ControlsShowInfodoc"      ' D4387
        Private Const RISK_CONTROLS_PAGINATION As String = "RiskControlsPagination"     ' A1385
        Private Const RISK_CONTROLS_USE_SIMULATED_VALUES As String = "SimulatedIO"      ' A1386
        Private Const RISK_SHOW_TOTAL_RISK As String = "ShowTotalRisk"

        Private Const RISK_REGIONS_RISK_LOW As String = "RiskRegionsLow"                ' A1414
        Private Const RISK_REGIONS_RISK_HIGH As String = "RiskRegionsHigh"              ' A1414
        Private Const RISK_SHOW_CONTROLS_INDICES As String = "RiskShowIndices"          ' A1471
        Private Const RISK_SHOW_CONTROLS_RISK_REDUCTION As String = "RiskShowSAReduction"
        Private Const RISK_SENSITIVITY_PARAMETER As String = "RisksensitivityParam"     ' A1522
        Private Const RISK_SHOW_LIKELIHOODS_GIVEN_SOURCES As String = "ShowGivenLkh"
        Private Const SYNTHESIZE_WRT_NODE_PARENT_GUID As String = "SynthesizeWRTParentNodeGuid"

        'Private Const RISK_REGIONS_RISK_LOW_COLOR As String = "RiskRegionsLowColor"     ' A1414
        'Private Const RISK_REGIONS_RISK_MED_COLOR As String = "RiskRegionsMedColor"     ' A1414
        'Private Const RISK_REGIONS_RISK_HIGH_COLOR As String = "RiskRegionsHighColor"   ' A1414
        Private Const RISK_REGIONS_RISK_LOW_COLOR As String = "RiskRegionsLowColorNew"
        Private Const RISK_REGIONS_RISK_MED_COLOR As String = "RiskRegionsMedColorNew"
        Private Const RISK_REGIONS_RISK_HIGH_COLOR As String = "RiskRegionsHighColorNew"

        Public Const OPT_PASSCODE_RISK_CONTROLS_PIPE_PREFIX As String = "$Risk$"        ' D4162
        Public Const NodeIndexShowWithFormatting As Boolean = True                      ' D4323

        Private Const PROJECT_DESCRIPTION_HEIGHT As String = "ProjectDescriptionHeight" ' A1522

        Private Const RATINGS_USE_DIRECT_VALUES As String = "RatingsUseDirectValues_{0}"    ' D4169

        Private Const DASHBOARD_CHART_OPTIONS As String = "Dashboard_Chart_Options"
        'Private Const DASHBOARD_SIMPLE_VIEW_MODE As String = "Dashboard_Simple_Mode"

        Private Const SYNTHESIS_COLOR_PALETTE_ID As String = "Synthesis_Color_Palette_id" 'A1445
        Private Const SYNTHESIS_NORMALIZATION_MODE As String = "SYNTHESIS_NORMALIZATION"
        Private Const SYNTHESIS_SHOW_EVENTS_TYPE As String = "SYNTHESIS_SHOW_EVENTS_TYPE"
        Private Const SYNTHESIS_OBJECTIVES_VISIBILITY As String = "Synthesis_Objectives_Visibility" 'A1445
        Private Const SYNTHESIS_OBJECTIVES_PRIORITIES_VISIBILITY As String = "Synthesis_Objectives_Priorities_Visibility"
        Private Const SYNTHESIS_GRIDS_TAB As String = "SYNTHESIS_GRIDS_TAB" 'A1732
        'Private Const SYNTHESIS_OBJECTIVES_SPLITTER_SIZE As String = "Synthesis_ObjectivesSplitterSize" 'A1482
        Private Const SYNTHESIS_LEGENDS_VISIBLE As String = "Synthesis_LegendsVisible"
        Private Const SYNTHESIS_WRT_NODE_ID As String = "WRTNodeID"     ' D6876
        Private Const HIERARCHY_VERTICAL_SPLITTER_SIZE As String = "Hierarchy_VerticalSplitterSize" 'A1508
        Public Const DEFAULT_HIERARCHY_RECTANGLE_HEIGHT As Integer = 50
        Public Const DEFAULT_HIERARCHY_RECTANGLE_MIN_WIDTH As Integer = 75
        Public Const DEFAULT_HIERARCHY_RECTANGLE_MAX_WIDTH As Integer = 200
        Private Const HIERARCHY_WAS_SHOWN_TO_PM As String = "Hierarchy_WasShown"
        Private Const HIERARCHY_RECTANGLE_HEIGHT As String = "Hierarchy_RectHeight"
        Private Const HIERARCHY_RECTANGLE_MIN_WIDTH As String = "Hierarchy_RectMinWidth"
        Private Const HIERARCHY_RECTANGLE_MAX_WIDTH As String = "Hierarchy_RectMaxWidth"
        Private Const HIERARCHY_MULTISELECT_ENABLED As String = "Hierarchy_MultiselectEnabled"
        Private Const HIERARCHY_SHOW_ALTERNATIVES_PRIORITIES = "Hierarchy_ShowAltsPriorities"

        Private Const AUTO_FIT_INFODOC_IMAGES As String = "AutoFitInfoDocImages"
        Private Const SHOW_FRAMED_INFODOCS_MOBILE As String = "ShowFramedInfodocsMobile"

        Private Const ASA_PAGE_SIZE As String = "Asa_Page_Size"
        Private Const ASA_PAGE_NUM As String = "Asa_Page_Num"
        Private Const ASA_SORT_MODE As String = "Asa_Sort_Mode"
        Private Const DSA_ACTIVE_SORTING As String = "DSA_ActiveSorting"
        Private Const SA_SORT_MODE As String = "SA_Sort_Mode"   ' D6928
        ' -D6928
        'Private Const SA_ALTS_SORT_MODE As String = "Sensitivies_Alts-_Sort_Mode"
        'Private Const PSA_ALTS_SORT_MODE As String = "PSA_Alts-_Sort_Mode"

        Private Const STRUCTURE_DISPLAY_PRIORITIES_MODE As String = "Structure_display_priorities_mode"

        Private Const EFFICIENT_FRONTIER_MIN_BENEFIT_INCREASE As String = "EfficientFrontier_MinBenefitIncrease"
        Private Const EFFICIENT_FRONTIER_X_AXIS_INTERVALS As String = "EfficientFrontier_XAxisIntervals" 'A1463
        Private Const EFFICIENT_FRONTIER_USE_UNLIM_BUDGET_CC As String = "EfficientFrontier_UseUnlimBudgetCC" 'A1501
        Private Const EFFICIENT_FRONTIER_PLOT_POINT_BY_POINT As String = "EfficientFrontier_PlottingMode" 'A1602
        Private Const EFFICIENT_FRONTIER_PLOT_IS_INCREASING As String = "EfficientFrontier_IsIncreasing" 'A1602
        Private Const EFFICIENT_FRONTIER_PLOT_PARAMETER As String = "EfficientFrontier_PlotParameter" 'A1608
        Private Const EFFICIENT_FRONTIER_CALCULATE_LEC_VALUES As String = "EfficientFrontier_LECValues" 'A1617

        Private Const RISK_BAYESIAN_MODE As String = "Risk_BayesianMode" 'A1465
        Private Const RISK_PLOT_SERIES_SHOWN As String = "RiskPlot_Series" 'A1482
        Private Const RISK_PLOT_MANUAL_ZOOM_PARAMS As String = "RiskPlot_ManualZoomParams" 'A1509 - axis X min and max, axis Y min and max
        Private Const RISK_CONTROLS_ACTUAL_SELECTION_MODE As String = "RiskPlot_ControlsSelectionMode" 'A1483
        'A1607 ===
        Private Const CS_TREE_MODE As String = "CS_TreeMode"
        Private Const CS_BOARD_MODE As String = "CS_BoardMode"
        Private Const CS_MEETING_SYNCH_MODE As String = "CS_MeetingSynchMode"
        'Private Const CS_MEETING_WHITEBOARD_MODE As String = "CS_MeetingWhiteboardMode"
        Private Const CS_MEETING_WHITEBOARD_DRAWING_DATA As String = "CS_MeetingWhiteboardDrawingData"
        Private Const CS_DRAWING_LIFE_TIME As String = "CS_DrawingLifeTime"
        Private Const CS_MEETING_STATE As String = "CS_MeetingState"
        Private Const CS_MEETING_OWNER As String = "CS_MeetingOwner"
        Private Const CS_DEFAULT_ALT_TITLE As String = "CS_DefaultAltTitle"
        Private Const CS_DEFAULT_OBJ_TITLE As String = "CS_DefaultObjTitle"
        Private Const CS_DEFAULT_SOURCE_TITLE As String = "CS_DefaultSourceTitle"
        Private Const CS_DEFAULT_ALT_COLOR As String = "CS_DefaultAltColor"
        Private Const CS_DEFAULT_SOURCE_COLOR As String = "CS_DefaultSourceColor"
        Private Const CS_DEFAULT_OBJ_COLOR As String = "CS_DefaultObjColor"
        Private Const CS_USER_LIST As String = "CS_UserList"
        Private Const CS_COLOR_CODING_BY_USER As String = "CS_UserColorCoding"
        Private Const CS_MEETING_LOCKED_BY_PM As String = "CS_MeetingLocked"
        Private Const CS_ITEM_WIDTH As String = "CS_ItemWidth"
        Private Const CS_ITEM_HEIGHT As String = "CS_ItemHeight"
        'A1607 ==

        Private Const RISK_REGISTER_TIMESTAMP As String = "RiskRegister_Timestamp" 'A1499

        Private Const LEC_SHOW_INTERSECTIONS As String = "LEC_ShowIntersections" 'A1609
        Private Const LEC_SHOW_FREQUENCY_CHARTS As String = "LEC_ShowFreqCharts" 'A1609

        Private Const RISKION_CALCULATIONS_MODE As String = "RiskionCalculationsMode"
        Private Const RISKION_SHOW_RISK_REDUCTION_OPTIONS As String = "RiskShowReductionOptiopns"

        Private Const RISKION_LEC_SHOW_INTERSECTIONS_OPTIONS As String = "RiskLECShowIntersectionOptiopns"

        Private Const PROJECTS_REPORTS As String = "ProjectReports" ' D6502

        Private Const PROP_TIMEFRAME As String = "TimeFrame"        ' D69568
        Private Const PROP_ASSUMPTIONS As String = "Assumptions"    ' D69568

        Private Const PROJECT_SPECIAL_MODE As String = "SpecialMode"    ' D7566

        Public Property ResultsLocalShowIndex As Boolean
            Get
                Return Parameters.GetParameterValue(RESULTS_LOCAL_SHOW_IDX, True)
            End Get
            Set(value As Boolean)
                Parameters.SetParameterValue(RESULTS_LOCAL_SHOW_IDX, value, Parameters.ParameterSetID)
            End Set
        End Property

        Public Property ResultsGlobalShowIndex As Boolean
            Get
                Return Parameters.GetParameterValue(RESULTS_GLOBAL_SHOW_IDX, True)
            End Get
            Set(value As Boolean)
                Parameters.SetParameterValue(RESULTS_GLOBAL_SHOW_IDX, value, Parameters.ParameterSetID)
            End Set
        End Property

        ' D4376 ===
        Public Property ResultsCustomCombinedUID As Integer
            Get
                Return Parameters.GetParameterValue(RESULTS_CUSTOM_COMBINED_UID, COMBINED_USER_ID)
            End Get
            Set(value As Integer)
                Parameters.SetParameterValue(RESULTS_CUSTOM_COMBINED_UID, value, Parameters.ParameterSetID)
            End Set
        End Property

        Public Property ResultsCustomCombinedName As String
            Get
                Return Parameters.GetParameterValue(RESULTS_CUSTOM_COMBINED_NAME, "")
            End Get
            Set(value As String)
                Parameters.SetParameterValue(RESULTS_CUSTOM_COMBINED_NAME, value.Trim, Parameters.ParameterSetID)
            End Set
        End Property
        ' D4376 ==

        ' D7561 ===
        Public Property ResultsLocalShowBars As Boolean
            Get
                Return Parameters.GetParameterValue(RESULTS_LOCAL_SHOW_BARS, True)
            End Get
            Set(value As Boolean)
                Parameters.SetParameterValue(RESULTS_LOCAL_SHOW_BARS, value, Parameters.ParameterSetID)
            End Set
        End Property

        Public Property ResultsGlobalShowBars As Boolean
            Get
                Return Parameters.GetParameterValue(RESULTS_GLOBAL_SHOW_BARS, True)
            End Get
            Set(value As Boolean)
                Parameters.SetParameterValue(RESULTS_GLOBAL_SHOW_BARS, value, Parameters.ParameterSetID)
            End Set
        End Property
        ' D7561 ==

        ' D4647 ===
        Public Property InvitationCustomText As String
            Get
                Return Parameters.GetParameterValue(INVITATION_CUSTOM_TEXT, "")
            End Get
            Set(value As String)
                Parameters.SetParameterValue(INVITATION_CUSTOM_TEXT, value.Trim, Parameters.ParameterSetID)
            End Set
        End Property

        Public Property InvitationCustomTitle As String
            Get
                Return Parameters.GetParameterValue(INVITATION_CUSTOM_TITLE, "")
            End Get
            Set(value As String)
                Parameters.SetParameterValue(INVITATION_CUSTOM_TITLE, value.Trim, Parameters.ParameterSetID)
            End Set
        End Property
        ' D4647 ==

        ' D4413 ===
        Public Property WordingTemplatesData As String
            Get
                Return Parameters.GetParameterValue(EVAL_WORDING_TEMPLATES, "")
            End Get
            Set(value As String)
                Parameters.SetParameterValue(EVAL_WORDING_TEMPLATES, value.Trim, -1)
            End Set
        End Property
        ' D4413 ==

        ' -D3943
        'Public Property TimeperiodsHasData(tScenarioID As Integer) As Boolean
        '    Get
        '        Return Parameters.GetParameterValue(String.Format("{0}{1}", RA_TIMEPERIODS_HAS_DATA, If(tScenarioID > 0, tScenarioID, "")), False) ' D3841
        '    End Get
        '    Set(value As Boolean)
        '        Parameters.SetParameterValue(String.Format("{0}{1}", RA_TIMEPERIODS_HAS_DATA, If(tScenarioID > 0, tScenarioID, "")), value, Parameters.ParameterSetID) ' D3841
        '    End Set
        'End Property

        Public Property TimeperiodsDistributeMode As Integer
            Get
                Return Parameters.GetParameterValue(RA_TIMEPERIODS_DISTRIBUTE_MODE, 1)
            End Get
            Set(value As Integer)
                Parameters.SetParameterValue(RA_TIMEPERIODS_DISTRIBUTE_MODE, value, Parameters.ParameterSetID)
            End Set
        End Property

        ' D3859 ===
        Public Property RAActiveScenarioID As Integer
            Get
                Return Parameters.GetParameterValue(RA_ACTIVE_SCENARIO_ID, 0)
            End Get
            Set(value As Integer)
                Parameters.SetParameterValue(RA_ACTIVE_SCENARIO_ID, value, Parameters.ParameterSetID)
            End Set
        End Property
        ' D3859 ==

        ' D3859 ===
        Public Property RASolver As raSolverLibrary
            Get
                Return CType(Parameters.GetParameterValue(RA_SOLVER_ID, CInt(raSolverLibrary.raGurobi)), raSolverLibrary)
            End Get
            Set(value As raSolverLibrary)
                Parameters.SetParameterValue(RA_SOLVER_ID, CInt(value), Parameters.ParameterSetID)
            End Set
        End Property

        Public Property RASolver_XAStrategy As Integer
            Get
                Return Parameters.GetParameterValue(RA_SOLVER_XA_STRATEGY, 1)
            End Get
            Set(value As Integer)
                Parameters.SetParameterValue(RA_SOLVER_XA_STRATEGY, value, Parameters.ParameterSetID)
            End Set
        End Property

        Public Property RASolver_XAVariation As String
            Get
                Return Parameters.GetParameterValue(RA_SOLVER_XA_VARIATION, "")
            End Get
            Set(value As String)
                Parameters.SetParameterValue(RA_SOLVER_XA_VARIATION, value, Parameters.ParameterSetID)
            End Set
        End Property

        Public Property RASolver_XATimeout As Integer
            Get
                Return Parameters.GetParameterValue(RA_SOLVER_XA_TIMEOUT, 120)
            End Get
            Set(value As Integer)
                Parameters.SetParameterValue(RA_SOLVER_XA_TIMEOUT, value, Parameters.ParameterSetID)
            End Set
        End Property

        Public Property RASolver_XATimeoutUnchanged As Integer
            Get
                Return Parameters.GetParameterValue(RA_SOLVER_XA_TIMEOUT_UNCHANGED, 30)
            End Get
            Set(value As Integer)
                Parameters.SetParameterValue(RA_SOLVER_XA_TIMEOUT_UNCHANGED, value, Parameters.ParameterSetID)
            End Set
        End Property
        ' D3877 ==

        ' D4364 ===
        Public Property RASolverPriorities As String
            Get
                Return Parameters.GetParameterValue(RA_SOLVER_PRIORITIES, "")
            End Get
            Set(value As String)
                Parameters.SetParameterValue(RA_SOLVER_PRIORITIES, value, Parameters.ParameterSetID)
            End Set
        End Property
        ' D4364 ==

        ' D4365 ===
        Public Property RAFundingPoolsExhausted() As Boolean
            Get
                Return Parameters.GetParameterValue(RA_FUNDING_POOLS_EXHAUSTED, False)
            End Get
            Set(value As Boolean)
                Parameters.SetParameterValue(RA_FUNDING_POOLS_EXHAUSTED, value, Parameters.ParameterSetID)
            End Set
        End Property
        ' D4365  ==

        ' D4367 ===
        Public Property RAFundingPoolsOrder As String
            Get
                Return Parameters.GetParameterValue(RA_FUNDING_POOLS_ORDER, "")
            End Get
            Set(value As String)
                Parameters.SetParameterValue(RA_FUNDING_POOLS_ORDER, value, Parameters.ParameterSetID)
            End Set
        End Property
        ' D4367 ==

        ' D7098 ===
        Public Property RAFundingPoolsUserPrty As Boolean
            Get
                Return Parameters.GetParameterValue(RA_FUNDING_POOLS_USE_PRTY, False)
            End Get
            Set(value As Boolean)
                Parameters.SetParameterValue(RA_FUNDING_POOLS_USE_PRTY, value, Parameters.ParameterSetID)
            End Set
        End Property
        ' D7098 ==

        ' -D3976
        '' D3905 ===
        'Public Property RAShowTimeperiods() As Boolean
        '    Get
        '        Return Parameters.GetParameterValue(RA_SHOW_TIMEPERIODS, False)
        '    End Get
        '    Set(value As Boolean)
        '        Parameters.SetParameterValue(RA_SHOW_TIMEPERIODS, value, Parameters.ParameterSetID)
        '    End Set
        'End Property
        '' D3905 ==

        ' D3929 ===
        Public Property TTHideOfflineUsers As Boolean
            Get
                Return Parameters.GetParameterValue(TT_HIDE_OFFLINE_USERS, False)
            End Get
            Set(value As Boolean)
                Parameters.SetParameterValue(TT_HIDE_OFFLINE_USERS, value, Parameters.ParameterSetID)
            End Set
        End Property
        ' D3929 ==

        ' D4554 ===
        Public Property TTEvaluationProgressUser As String
            Get
                Return Parameters.GetParameterValue(TT_EVAL_PRG_USER, "")
            End Get
            Set(value As String)
                Parameters.SetParameterValue(TT_EVAL_PRG_USER, value, Parameters.ParameterSetID)
            End Set
        End Property
        ' D4554 ==

        ' D3953 ===
        Public Property EvalCollapseMultiPWBars As Boolean
            Get
                Return Parameters.GetParameterValue(EVAL_COLLPASE_MULTIPW_BARS, True)
            End Get
            Set(value As Boolean)
                Parameters.SetParameterValue(EVAL_COLLPASE_MULTIPW_BARS, value, Parameters.ParameterSetID)
            End Set
        End Property
        ' D3953 ==

        ' D4713 ===
        Public Property EvalHideInfodocCaptions As Boolean
            Get
                Return Parameters.GetParameterValue(EVAL_HIDE_INFODOC_CAPTIONS, False)
            End Get
            Set(value As Boolean)
                Parameters.SetParameterValue(EVAL_HIDE_INFODOC_CAPTIONS, value, Parameters.ParameterSetID)
            End Set
        End Property
        ' D4713 ==

        ' D7556 ===
        Public Property EvalHideLocalNormalizationOptions As Boolean
            Get
                Return Parameters.GetParameterValue(EVAL_HIDE_LOCAL_NORMALIZATION, False)
            End Get
            Set(value As Boolean)
                Parameters.SetParameterValue(EVAL_HIDE_LOCAL_NORMALIZATION, value, Parameters.ParameterSetID)
            End Set
        End Property

        Public Property EvalHideGlobalNormalizationOptions As Boolean
            Get
                Return Parameters.GetParameterValue(EVAL_HIDE_GLOBAL_NORMALIZATION, False)
            End Get
            Set(value As Boolean)
                Parameters.SetParameterValue(EVAL_HIDE_GLOBAL_NORMALIZATION, value, Parameters.ParameterSetID)
            End Set
        End Property
        ' D7556 ==

        ' D4714 ===
        Public Property EvalEmbeddedContent As String
            Get
                Return Parameters.GetParameterValue(EVAL_EMBEDDED_CONTENT, "")
            End Get
            Set(value As String)
                Parameters.SetParameterValue(EVAL_EMBEDDED_CONTENT, value, Parameters.ParameterSetID)
            End Set
        End Property
        ' D4714 ==

        ' D3963 ===
        Public Property EvalNoAskAutoAdvance As Boolean
            Get
                Return Parameters.GetParameterValue(EVAL_NO_ASK_AUTOADVANCE, False)
            End Get
            Set(value As Boolean)
                Parameters.SetParameterValue(EVAL_NO_ASK_AUTOADVANCE, value, Parameters.ParameterSetID)
            End Set
        End Property

        Public Property EvalShowRewardThankYou As Boolean
            Get
                Return Parameters.GetParameterValue(EVAL_REWARD_THANKYOU, False)
            End Get
            Set(value As Boolean)
                Parameters.SetParameterValue(EVAL_REWARD_THANKYOU, value, Parameters.ParameterSetID)
            End Set
        End Property
        ' D3963 ==

        ' D3977 ===
        Public Property EvalWelcomeSurveyFirst As Boolean
            Get
                Return Parameters.GetParameterValue(EVAL_WELCOME_SURVEY_FIRST, False)
            End Get
            Set(value As Boolean)
                Parameters.SetParameterValue(EVAL_WELCOME_SURVEY_FIRST, value, Parameters.ParameterSetID)
            End Set
        End Property

        Public Property EvalThankYouSurveyLast As Boolean
            Get
                Return Parameters.GetParameterValue(EVAL_THANKYOU_SURVEY_LAST, False)
            End Get
            Set(value As Boolean)
                Parameters.SetParameterValue(EVAL_THANKYOU_SURVEY_LAST, value, Parameters.ParameterSetID)
            End Set
        End Property
        ' D3977 ==

        ' D6962 ===
        Public Property EvalTextToSpeech As ecText2Speech
            Get
                Return CType(Parameters.GetParameterValue(EVAL_T2S, CInt(ecText2Speech.Regular)), ecText2Speech)
            End Get
            Set(value As ecText2Speech)
                Parameters.SetParameterValue(EVAL_T2S, CInt(value), Parameters.ParameterSetID)
            End Set
        End Property
        ' D6962 ==

        ' D4169 ===
        Public Property RatingsUseDirectValue(tScaleID As Guid) As Boolean
            Get
                Return Parameters.GetParameterValue(String.Format(RATINGS_USE_DIRECT_VALUES, tScaleID.ToString), True)
            End Get
            Set(value As Boolean)
                Parameters.SetParameterValue(String.Format(RATINGS_USE_DIRECT_VALUES, tScaleID.ToString), value, Parameters.ParameterSetID)
            End Set
        End Property        
        ' D3905 ==
                
        'A1522 ===
        Public Property Project_DescriptionHeight() As Double
            Get
                Return Parameters.GetParameterValue(PROJECT_DESCRIPTION_HEIGHT, 200)
            End Get
            Set(value As Double)
                Parameters.SetParameterValue(PROJECT_DESCRIPTION_HEIGHT, value, Parameters.ParameterSetID)
            End Set
        End Property
        'A1522 ==

        ' D4103 ===
        ' Please note: single option for Riskion and not depend on hierarchy
        Public Property EvalJoinRiskionPipes As Boolean
            Get
                Dim tVal As Object = Parameters.GetParameter(EVAL_JOIN_RISKION_PIPES, -1)
                If tVal Is Nothing Then Return True Else Return CBool(tVal)
            End Get
            Set(value As Boolean)
                Parameters.SetParameterValue(EVAL_JOIN_RISKION_PIPES, value, -1)
            End Set
        End Property
        ' D4103 ==

        ' D4160 ===
        Public Property EvalCloseWindowAtFinish As Boolean  ' D4162
            Get
                Return Parameters.GetParameterValue(EVAL_NAV_CLOSE_WIN_AT_FINISH, False) AndAlso (Not Parameters.ProjectManager.IsRiskProject OrElse Not EvalJoinRiskionPipes)    ' D4164
            End Get
            Set(value As Boolean)
                Parameters.SetParameterValue(EVAL_NAV_CLOSE_WIN_AT_FINISH, value, Parameters.ParameterSetID)
            End Set
        End Property

        Public Property EvalNextProjectPasscodeAtFinish As String   ' D4162
            Get
                Return Parameters.GetParameterValue(EVAL_NAV_NEXT_PASSCODE_AT_FINISH, "")
            End Get
            Set(value As String)
                Parameters.SetParameterValue(EVAL_NAV_NEXT_PASSCODE_AT_FINISH, value, Parameters.ParameterSetID)
            End Set
        End Property
        ' D4160 ==

        ' D4162 ===
        Public Property EvalOpenNextProjectAtFinish As Boolean
            Get
                Return Parameters.GetParameterValue(EVAL_NAV_OPEN_NEXT_PRJ, False) AndAlso (Not Parameters.ProjectManager.IsRiskProject OrElse Not EvalJoinRiskionPipes)    ' D4164
            End Get
            Set(value As Boolean)
                Parameters.SetParameterValue(EVAL_NAV_OPEN_NEXT_PRJ, value, Parameters.ParameterSetID)
            End Set
        End Property
        ' D4162 ==

        Public Property AutoFitInfoDocImages As Boolean
            Get
                Return Parameters.GetParameterValue(AUTO_FIT_INFODOC_IMAGES, False)
            End Get
            Set(value As Boolean)
                Parameters.SetParameterValue(AUTO_FIT_INFODOC_IMAGES, value, Parameters.ParameterSetID)
            End Set
        End Property


        Public Property ShowFramedInfodocsMobile As Boolean
            Get
                Return Parameters.GetParameterValue(SHOW_FRAMED_INFODOCS_MOBILE, False)
            End Get
            Set(value As Boolean)
                Parameters.SetParameterValue(SHOW_FRAMED_INFODOCS_MOBILE, value, Parameters.ParameterSetID)
            End Set
        End Property

        'A1340 ===
        Public Property EvalTryParseValuesFromNames As Boolean
            Get
                Return Parameters.GetParameterValue(EVAL_TRY_PARSE_VALUES_FROM_NAMES, False)
            End Get
            Set(value As Boolean)
                Parameters.SetParameterValue(EVAL_TRY_PARSE_VALUES_FROM_NAMES, value, Parameters.ParameterSetID)
            End Set
        End Property 'A1340 ==

        'A1363 ===
        Public Property AlternativeFilters As String
            Get
                Return Parameters.GetParameterValue(ALTERNATIVES_SAVED_FILTERS, "")
            End Get
            Set(value As String)
                Parameters.SetParameterValue(ALTERNATIVES_SAVED_FILTERS, value, Parameters.ParameterSetID)
            End Set
        End Property 'A1363 ==

        ' D4180 ===
        Public Property NodeVisibleIndexMode As IDColumnModes
            Get
                If Parameters.ProjectManager IsNot Nothing AndAlso Not Parameters.ProjectManager.IsRiskProject Then Return IDColumnModes.IndexID
                Dim tVal As IDColumnModes = IDColumnModes.UniqueID
                Select Case Parameters.GetParameterValue(NODE_VISIBLE_INDEX_MODE, CInt(tVal))
                    ' -D4323
                    'Case CInt(IDColumnModes.None)
                    '    tVal = IDColumnModes.None
                    Case CInt(IDColumnModes.IndexID)
                        tVal = IDColumnModes.IndexID
                    Case CInt(IDColumnModes.Rank)
                        tVal = IDColumnModes.Rank
                End Select
                Return tVal
            End Get
            Set(value As IDColumnModes)
                Parameters.SetParameterValue(NODE_VISIBLE_INDEX_MODE, CInt(value), -1)
            End Set
        End Property
        ' D4180 ==

        ' D4323 ===
        Public Property NodeIndexIsVisible As Boolean 'A1342
            Get
                Return Parameters.GetParameterValue(NODE_INDEX_VISIBILITY, True)
            End Get
            Set(value As Boolean)
                Parameters.SetParameterValue(NODE_INDEX_VISIBILITY, value, -1)
            End Set
        End Property

        'Public Property NodeIndexPlainView As Boolean
        '    Get
        '        Return Parameters.GetParameterValue(NODE_INDEX_PLAIN_VIEW, False)
        '    End Get
        '    Set(value As Boolean)
        '        Parameters.SetParameterValue(NODE_INDEX_PLAIN_VIEW, Parameters.ParameterSetID)
        '    End Set
        'End Property
        ' D4323 ==

        Public Property ObjectiveIndexIsVisible As Boolean 'A2107
            Get
                Return Parameters.GetParameterValue(OBJECTIVE_INDEX_VISIBILITY, False)
            End Get
            Set(value As Boolean)
                Parameters.SetParameterValue(OBJECTIVE_INDEX_VISIBILITY, value, -1)
            End Set
        End Property

        ' D4387 ===
        Public Property Riskion_Control_ShowInfodocs As Boolean
            Get
                Return Parameters.GetParameterValue(RISK_CONTROLS_SHOW_INFODOC, False)
            End Get
            Set(value As Boolean)
                Parameters.SetParameterValue(RISK_CONTROLS_SHOW_INFODOC, value, Parameters.ParameterSetID)
            End Set
        End Property
        ' D4387 ==

        Public Property Riskion_ControlsSelectedUser As Integer
            Get
                Return Parameters.GetParameterValue("Riskion_ControlsSelectedUser", Integer.MinValue)
            End Get
            Set(value As Integer)
                Parameters.SetParameterValue("Riskion_ControlsSelectedUser", value, Parameters.ParameterSetID)
            End Set
        End Property

        'A1385 ===
        Public Property Riskion_Control_Pagination As Integer ' -1 = disabled
            Get
                Return Parameters.GetParameterValue(RISK_CONTROLS_PAGINATION, 100)
            End Get
            Set(value As Integer)
                Parameters.SetParameterValue(RISK_CONTROLS_PAGINATION, value, Parameters.ParameterSetID)
            End Set
        End Property
        'A1385 ==

        'A1386 ===
        Public Property Riskion_Use_Simulated_Values As Integer ' 0 - Computed, 1 - Simulated Input, 2 - Simulated Output, 3 - Simulated Input and Output
            Get
                If Not Parameters.ProjectManager.IsRiskProject Then Return 0
                Dim retVal As Integer = CInt(Parameters.GetParameterValue(RISK_CONTROLS_USE_SIMULATED_VALUES, 2))
                Return If(retVal > 0, 2, 0)
            End Get
            Set(value As Integer)
                value = If(value > 0, 2, 0)
                Parameters.SetParameterValue(RISK_CONTROLS_USE_SIMULATED_VALUES, value, Parameters.ParameterSetID)
            End Set
        End Property
        'A1386 ==

        'A1471 ===
        Public Property Riskion_Show_Control_Indices As Boolean
            Get
                Return Parameters.GetParameterValue(RISK_SHOW_CONTROLS_INDICES, True)
            End Get
            Set(value As Boolean)
                Parameters.SetParameterValue(RISK_SHOW_CONTROLS_INDICES, value, Parameters.ParameterSetID)
            End Set
        End Property
        'A1471 ==

        Public Property Riskion_ShowControlsRiskReduction As Boolean
            Get
                Return Parameters.GetParameterValue(RISK_SHOW_CONTROLS_RISK_REDUCTION, False)
            End Get
            Set(value As Boolean)
                Parameters.SetParameterValue(RISK_SHOW_CONTROLS_RISK_REDUCTION, value, -1)
            End Set
        End Property

        Public Property Riskion_Show_Total_Risk As Boolean
            Get
                Return CInt(Parameters.GetParameterValue(RISK_SHOW_TOTAL_RISK, False))
            End Get
            Set(value As Boolean)
                Parameters.SetParameterValue(RISK_SHOW_TOTAL_RISK, value, -1)
            End Set
        End Property

        'A1522 ===
        Public Property RiskSensitivityParameter As Integer ' 0 - Priority (Impact), 1 - Risk
            Get
                Return Parameters.GetParameterValue(RISK_SENSITIVITY_PARAMETER, 1)
            End Get
            Set(value As Integer)
                Parameters.SetParameterValue(RISK_SENSITIVITY_PARAMETER, value, Parameters.ParameterSetID)
            End Set
        End Property
        'A1522 ==        

        Public Property ShowLikelihoodsGivenSources As Boolean 'CalculationManager.ShowDueToPriorities
            Get
                Return Parameters.GetParameterValue(RISK_SHOW_LIKELIHOODS_GIVEN_SOURCES, True)
            End Get
            Set(value As Boolean)
                Parameters.SetParameterValue(RISK_SHOW_LIKELIHOODS_GIVEN_SOURCES, value, Parameters.ParameterSetID)
            End Set
        End Property

        Public Property WRTNodeParentGUID As String 
            Get
                Return Parameters.GetParameterValue(SYNTHESIZE_WRT_NODE_PARENT_GUID, "")
            End Get
            Set(value As String)
                Parameters.SetParameterValue(SYNTHESIZE_WRT_NODE_PARENT_GUID, value, Parameters.ParameterSetID)
            End Set
        End Property

        'A1414 ===
        Public ReadOnly Property DefaultRh As Double
            Get
                Return 0.15
            End Get
        End Property

        Public ReadOnly Property DefaultRl As Double
            Get
                Return 0.02
            End Get
        End Property

        Public ReadOnly Property DefaultRedBrush As String
            Get
                If Parameters.ProjectManager.PipeParameters.ProjectType = ProjectType.ptOpportunities Then Return "#e2ffe0" '"#a0ff6a" '"#ffffff" 'Green
                Return "#fa8282"   ' "#C78989" '"#ffe7e7" '"#fc4c4c" '"#FF7D7B" '"#ff6347" 'Red
            End Get
        End Property

        Public ReadOnly Property DefaultWhiteBrush As String
            Get
                Return "#eef52f"    ' "#feffdb" '"#fddb6d" '"#FFEDCE" '"#ffff00" 'Yellow
            End Get
        End Property

        Public ReadOnly Property DefaultGreenBrush As String
            Get
                If Parameters.ProjectManager.PipeParameters.ProjectType = ProjectType.ptOpportunities Then Return "#C78989" '"#FF7D7B" 'Red
                Return "#9efc97"    ' "#e2ffe0" '"#57910c" '"#a0ff6a" '"#E1ECC0" '"#00ff7f" 'Green
            End Get
        End Property

        Public Property Riskion_Regions_Rl As Double
            Get
                Dim retVal As Double = Parameters.GetParameterValue(RISK_REGIONS_RISK_LOW, CDbl(Integer.MinValue))
                If retVal = CDbl(Integer.MinValue) Then
                    retVal = DefaultRl
                End If
                Return retVal
            End Get
            Set(value As Double)
                Parameters.SetParameterValue(RISK_REGIONS_RISK_LOW, value, Parameters.ParameterSetID)
            End Set
        End Property

        Public Property Riskion_Regions_Rh As Double
            Get
                Dim retVal As Double = Parameters.GetParameterValue(RISK_REGIONS_RISK_HIGH, CDbl(Integer.MinValue))
                If retVal = CDbl(Integer.MinValue) Then
                    retVal = DefaultRh
                End If
                Return retVal
            End Get
            Set(value As Double)
                Parameters.SetParameterValue(RISK_REGIONS_RISK_HIGH, value, Parameters.ParameterSetID)
            End Set
        End Property

        Public Property Riskion_Regions_Rl_Color As String
            Get
                Dim retVal As String = Parameters.GetParameterValue(RISK_REGIONS_RISK_LOW_COLOR, "")
                If retVal = "" Then
                    retVal = DefaultGreenBrush
                End If
                Return retVal
            End Get
            Set(value As String)
                Parameters.SetParameterValue(RISK_REGIONS_RISK_LOW_COLOR, value, Parameters.ParameterSetID)
            End Set
        End Property

        Public Property Riskion_Regions_Rm_Color As String
            Get
                Dim retVal As String = Parameters.GetParameterValue(RISK_REGIONS_RISK_MED_COLOR, "")
                If retVal = "" Then
                    retVal = DefaultWhiteBrush
                End If
                Return retVal
            End Get
            Set(value As String)
                Parameters.SetParameterValue(RISK_REGIONS_RISK_MED_COLOR, value, Parameters.ParameterSetID)
            End Set
        End Property

        Public Property Riskion_Regions_Rh_Color As String
            Get
                Dim retVal As String = Parameters.GetParameterValue(RISK_REGIONS_RISK_HIGH_COLOR, "")
                If retVal = "" Then
                    retVal = DefaultRedBrush
                End If
                Return retVal
            End Get
            Set(value As String)
                Parameters.SetParameterValue(RISK_REGIONS_RISK_HIGH_COLOR, value, Parameters.ParameterSetID)
            End Set
        End Property
        'A1414 ==

        Public Property Dashboard_ChartOptions As String
            Get
                Return Parameters.GetParameterValue(DASHBOARD_CHART_OPTIONS, "{}")
            End Get
            Set(value As String)
                Parameters.SetParameterValue(DASHBOARD_CHART_OPTIONS, value, Parameters.ParameterSetID)
            End Set
        End Property

        Public Property SynthesisColorPaletteId As Integer
            Get
                Return Parameters.GetParameterValue(SYNTHESIS_COLOR_PALETTE_ID, 1)
            End Get
            Set(value As Integer)
                Parameters.SetParameterValue(SYNTHESIS_COLOR_PALETTE_ID, value, Parameters.ParameterSetID)
            End Set
        End Property

        Public Property Normalization As LocalNormalizationType
            Get
                If Parameters.ProjectManager IsNot Nothing AndAlso Parameters.ProjectManager.IsRiskProject Then Return LocalNormalizationType.ntUnnormalized
                Dim retVal As Integer = Parameters.GetParameterValue(SYNTHESIS_NORMALIZATION_MODE, CInt(LocalNormalizationType.ntNormalizedForAll))
                If Parameters.ProjectManager IsNot Nothing AndAlso Parameters.ProjectManager.PipeParameters.SynthesisMode = ECSynthesisMode.smDistributive AndAlso retVal = LocalNormalizationType.ntUnnormalized Then retVal = LocalNormalizationType.ntNormalizedForAll
                If retVal = CInt(LocalNormalizationType.ntNormalizedSum100) Then retVal = CInt(LocalNormalizationType.ntNormalizedForAll)
                Return CType(retVal, LocalNormalizationType)
            End Get
            Set(value As LocalNormalizationType)
                Parameters.SetParameterValue(SYNTHESIS_NORMALIZATION_MODE, CInt(value), Parameters.ParameterSetID)
            End Set
        End Property

        ' D6928 ===
        Public Property SensitivitySorting As SASortMode
            Get
                Dim retVal As Integer = Parameters.GetParameterValue(SA_SORT_MODE, CInt(SASortMode.AltsByPrty))
                Return CType(retVal, SASortMode)
            End Get
            Set(value As SASortMode)
                Parameters.SetParameterValue(SA_SORT_MODE, CInt(value), Parameters.ParameterSetID)
            End Set
        End Property
        ' D6928 ==

        Public Property RiskionShowEventsType As Integer
            Get
                Return Parameters.GetParameterValue(SYNTHESIS_SHOW_EVENTS_TYPE, 2)
            End Get
            Set(value As Integer)
                Parameters.SetParameterValue(SYNTHESIS_SHOW_EVENTS_TYPE, value)
            End Set
        End Property

        'A1445 ===
        Public Property Synthesis_ObjectivesVisibility As Boolean
            Get
                Return Parameters.GetParameterValue(SYNTHESIS_OBJECTIVES_VISIBILITY, True)
            End Get
            Set(value As Boolean)
                Parameters.SetParameterValue(SYNTHESIS_OBJECTIVES_VISIBILITY, value, Parameters.ParameterSetID)
            End Set
        End Property 'A1445 ==


        Public Property Synthesis_ObjectivesPrioritiesVisibility As Integer ' < 2 - invisible, 2 - show local priorities, 3 - show global priorities, 4 - show both priorities
            Get
                Return Parameters.GetParameterValue(SYNTHESIS_OBJECTIVES_PRIORITIES_VISIBILITY, 0)
            End Get
            Set(value As Integer)
                Parameters.SetParameterValue(SYNTHESIS_OBJECTIVES_PRIORITIES_VISIBILITY, value, Parameters.ParameterSetID)
            End Set
        End Property

        Public Property Synthesis_ObjectivesPrioritiesMode As Integer ' 0 - Local, 1 - Global
            Get
                Return Parameters.GetParameterValue("Synthesis_ObjectivesPrioritiesMode", 0)
            End Get
            Set(value As Integer)
                Parameters.SetParameterValue("Synthesis_ObjectivesPrioritiesMode", value, -1)
            End Set
        End Property

        'A1732 ===
        Public Property Synthesis_GridsTab As Integer
            Get
                Return Parameters.GetParameterValue(SYNTHESIS_GRIDS_TAB, 0)
            End Get
            Set(value As Integer)
                Parameters.SetParameterValue(SYNTHESIS_GRIDS_TAB, value, Parameters.ParameterSetID)
            End Set
        End Property 'A1732 ==

        'A1482 ===
        'Public Property Synthesis_ObjectivesSplitterSize As Double
        '    Get
        '        Return Parameters.GetParameterValue(SYNTHESIS_OBJECTIVES_SPLITTER_SIZE, 200)
        '    End Get
        '    Set(value As Double)
        '        Parameters.SetParameterValue(SYNTHESIS_OBJECTIVES_SPLITTER_SIZE, value, Parameters.ParameterSetID)
        '    End Set
        'End Property 'A1482 ==

        Public Property Synthesis_LegendsVisible As Boolean
            Get
                Return Parameters.GetParameterValue(SYNTHESIS_LEGENDS_VISIBLE, False)
            End Get
            Set(value As Boolean)
                Parameters.SetParameterValue(SYNTHESIS_LEGENDS_VISIBLE, value, Parameters.ParameterSetID)
            End Set
        End Property 'A1445 ==

        Private _Synthesis_WRTNodeID() As Integer = {Integer.MinValue, Integer.MinValue, Integer.MinValue}
        ' D6876 ===
        Public Property Synthesis_WRTNodeID As Integer
            Get
                Dim ID As Integer = _Synthesis_WRTNodeID(Parameters.ProjectManager.ActiveHierarchy) 'Parameters.GetParameterValue(SYNTHESIS_WRT_NODE_ID + Parameters.ProjectManager.ActiveHierarchy.ToString, -1)
                With Parameters.ProjectManager.ActiveObjectives
                    If .GetNodeByID(ID) Is Nothing AndAlso .Nodes.Count > 0 Then ID = .Nodes(0).NodeID
                End With
                Return ID
            End Get
            Set(value As Integer)
                _Synthesis_WRTNodeID(Parameters.ProjectManager.ActiveHierarchy) = value
                'Parameters.SetParameterValue(SYNTHESIS_WRT_NODE_ID + Parameters.ProjectManager.ActiveHierarchy.ToString, value, Parameters.ParameterSetID)
            End Set
        End Property

        'Public Property GridWRTNodeID As Integer
        '    Get
        '        Return Synthesis_WRTNodeID
        '    End Get
        '    Set(value As Integer)
        '        Synthesis_WRTNodeID = value
        '    End Set
        'End Property

        'Public Property SAWRTNodeID As Integer
        '    Get
        '        Return Synthesis_WRTNodeID
        '    End Get
        '    Set(value As Integer)
        '        Synthesis_WRTNodeID = value
        '    End Set
        'End Property
        ' D6876 ==

        'A1508 ===
        Public Property Hierarchy_VerticalSplitterSize As Double
            Get
                Return Parameters.GetParameterValue(HIERARCHY_VERTICAL_SPLITTER_SIZE, 200)
            End Get
            Set(value As Double)
                Parameters.SetParameterValue(HIERARCHY_VERTICAL_SPLITTER_SIZE, value, Parameters.ParameterSetID)
            End Set
        End Property 'A1508 ==

        Public Property Hierarchy_WasShownToPM As String
            Get
                Return Parameters.GetParameterValue(HIERARCHY_WAS_SHOWN_TO_PM, "") ' undefined for existing projects
            End Get
            Set(value As String)
                Parameters.SetParameterValue(HIERARCHY_WAS_SHOWN_TO_PM, value, Parameters.ParameterSetID)
            End Set
        End Property

        Public Property Hierarchy_RectangleHeight As Integer
            Get
                Return Parameters.GetParameterValue(HIERARCHY_RECTANGLE_HEIGHT, DEFAULT_HIERARCHY_RECTANGLE_HEIGHT)
            End Get
            Set(value As Integer)
                Parameters.SetParameterValue(HIERARCHY_RECTANGLE_HEIGHT, value, Parameters.ParameterSetID)
            End Set
        End Property

        Public Property Hierarchy_RectangleMinWidth As Integer
            Get
                Return Parameters.GetParameterValue(HIERARCHY_RECTANGLE_MIN_WIDTH, DEFAULT_HIERARCHY_RECTANGLE_MIN_WIDTH)
            End Get
            Set(value As Integer)
                Parameters.SetParameterValue(HIERARCHY_RECTANGLE_MIN_WIDTH, value, Parameters.ParameterSetID)
            End Set
        End Property

        Public Property Hierarchy_RectangleMaxWidth As Integer
            Get
                Return Parameters.GetParameterValue(HIERARCHY_RECTANGLE_MAX_WIDTH, DEFAULT_HIERARCHY_RECTANGLE_MAX_WIDTH)
            End Get
            Set(value As Integer)
                Parameters.SetParameterValue(HIERARCHY_RECTANGLE_MAX_WIDTH, value, Parameters.ParameterSetID)
            End Set
        End Property

        Public Property Hierarchy_MultiselectEnabled As Boolean
            Get
                Return Parameters.GetParameterValue(HIERARCHY_MULTISELECT_ENABLED, False)
            End Get
            Set(value As Boolean)
                Parameters.SetParameterValue(HIERARCHY_MULTISELECT_ENABLED, value, Parameters.ParameterSetID)
            End Set
        End Property

        Public Property Hierarchy_ShowAlternativesPriorities As Boolean
            Get
                Return Parameters.GetParameterValue(HIERARCHY_SHOW_ALTERNATIVES_PRIORITIES, False)
            End Get
            Set(value As Boolean)
                Parameters.SetParameterValue(HIERARCHY_SHOW_ALTERNATIVES_PRIORITIES, value, Parameters.ParameterSetID)
            End Set
        End Property

        Public Property AsaPageSize As Integer
            Get
                Return Parameters.GetParameterValue(ASA_PAGE_SIZE, 15)
            End Get
            Set(value As Integer)
                Parameters.SetParameterValue(ASA_PAGE_SIZE, value, Parameters.ParameterSetID)
            End Set
        End Property

        Public Property AsaPageNum As Integer
            Get
                Return Parameters.GetParameterValue(ASA_PAGE_NUM, 1)
            End Get
            Set(value As Integer)
                Parameters.SetParameterValue(ASA_PAGE_NUM, value, Parameters.ParameterSetID)
            End Set
        End Property

        'A1466 ===
        Public Property AsaSortMode As Integer '0 - Hierarchy (default), 1 - Priority, 2 - Name
            Get
                Return Parameters.GetParameterValue(ASA_SORT_MODE, 0)
            End Get
            Set(value As Integer)
                Parameters.SetParameterValue(ASA_SORT_MODE, value, Parameters.ParameterSetID)
            End Set
        End Property 'A1466 ==

        Public Property DsaActiveSorting As Boolean
            Get
                Return Parameters.GetParameterValue(DSA_ACTIVE_SORTING, False)
            End Get
            Set(value As Boolean)
                Parameters.SetParameterValue(DSA_ACTIVE_SORTING, value, Parameters.ParameterSetID)
            End Set
        End Property

        'A1473 ===

        ' -D6928
        'Public Property SenitivitiesAltsSortMode As Integer '0 - Default, 1 - Priority, 2 - Name
        '    Get
        '        Return Parameters.GetParameterValue(SA_ALTS_SORT_MODE, 0) ' not sorted by default per EF's request
        '    End Get
        '    Set(value As Integer)
        '        Parameters.SetParameterValue(SA_ALTS_SORT_MODE, value, Parameters.ParameterSetID)
        '    End Set
        'End Property 'A1466 ==

        ' -D6928
        'Public Property PerformanceAltsSortMode As Integer '0 - Default, 1 - Priority, 2 - Name
        '    Get
        '        Return Parameters.GetParameterValue(PSA_ALTS_SORT_MODE, 1) ' sorted by priority per EF's request
        '    End Get
        '    Set(value As Integer)
        '        Parameters.SetParameterValue(PSA_ALTS_SORT_MODE, value, Parameters.ParameterSetID)
        '    End Set
        'End Property

        'Public Property Dashboard_SimpleViewMode As Boolean
        '    Get
        '        Return Parameters.GetParameterValue(DASHBOARD_SIMPLE_VIEW_MODE, False)
        '    End Get
        '    Set(value As Boolean)
        '        Parameters.SetParameterValue(DASHBOARD_SIMPLE_VIEW_MODE, value, Parameters.ParameterSetID)
        '    End Set
        'End Property

        'A1496 ===
        Public Property Structure_DisplayPrioritiesMode As Integer '0 - None (default), 1 - Local, 2 - Global, 3 - Both Local And Global
            Get
                If Parameters.ProjectManager.IsRiskProject Then Return 0
                Return Parameters.GetParameterValue(STRUCTURE_DISPLAY_PRIORITIES_MODE, 0)
            End Get
            Set(value As Integer)
                Parameters.SetParameterValue(STRUCTURE_DISPLAY_PRIORITIES_MODE, value, Parameters.ParameterSetID)
            End Set
        End Property 'A1466 ==

        Public Const EfficientFrontierMinBenefitIncreaseDefaultValue As Double = 1
        Public Property EfficientFrontierMinBenefitIncrease As Double
            Get
                Return Parameters.GetParameterValue(EFFICIENT_FRONTIER_MIN_BENEFIT_INCREASE, EfficientFrontierMinBenefitIncreaseDefaultValue)
            End Get
            Set(value As Double)
                Parameters.SetParameterValue(EFFICIENT_FRONTIER_MIN_BENEFIT_INCREASE, value, Parameters.ParameterSetID)
            End Set
        End Property

        'A1463 ===
        Public Property EfficientFrontierXAxisIntervals As String
            Get
                Return Parameters.GetParameterValue(EFFICIENT_FRONTIER_X_AXIS_INTERVALS, "")
            End Get
            Set(value As String)
                Parameters.SetParameterValue(EFFICIENT_FRONTIER_X_AXIS_INTERVALS, value, Parameters.ParameterSetID)
            End Set
        End Property 'A1463 ==

        'A1501 ===
        Public Property EfficientFrontierUseUnlimitedBudgetForCC As Boolean
            Get
                Return Parameters.GetParameterValue(EFFICIENT_FRONTIER_USE_UNLIM_BUDGET_CC, False)
            End Get
            Set(value As Boolean)
                Parameters.SetParameterValue(EFFICIENT_FRONTIER_USE_UNLIM_BUDGET_CC, value, Parameters.ParameterSetID)
            End Set
        End Property 'A1501 ==

        'A1602 ===
        Public Property EfficientFrontierPlotPointByPoint As Boolean?
            Get
                Dim sVal As String = Parameters.GetParameterValue(EFFICIENT_FRONTIER_PLOT_POINT_BY_POINT, "")
                If sVal = "" Then
                    Return Nothing
                Else
                    Return sVal = "1"
                End If
            End Get
            Set(value As Boolean?)
                Parameters.SetParameterValue(EFFICIENT_FRONTIER_PLOT_POINT_BY_POINT, If(value.Value, "1", "0") , Parameters.ParameterSetID)
            End Set
        End Property

        Public Property EfficientFrontierIsIncreasing As Boolean
            Get
                Return Parameters.GetParameterValue(EFFICIENT_FRONTIER_PLOT_IS_INCREASING, True)
            End Get
            Set(value As Boolean)
                Parameters.SetParameterValue(EFFICIENT_FRONTIER_PLOT_IS_INCREASING, value, Parameters.ParameterSetID)
            End Set
        End Property
        'A1602 ==

        'A1608 ===
        Public Property EfficientFrontierPlotParameter As String ' "priority", "leverage", "delta_leverage", "savings", "delta_savings"
            Get
                Return Parameters.GetParameterValue(EFFICIENT_FRONTIER_PLOT_PARAMETER, "priority")
            End Get
            Set(value As String)
                Parameters.SetParameterValue(EFFICIENT_FRONTIER_PLOT_PARAMETER, value, Parameters.ParameterSetID)
            End Set
        End Property
        'A1608 ==

        'A1617 ===
        Public Property EfficientFrontierCalculateLECValues As Boolean
            Get
                Return Parameters.GetParameterValue(EFFICIENT_FRONTIER_CALCULATE_LEC_VALUES, True)
            End Get
            Set(value As Boolean)
                Parameters.SetParameterValue(EFFICIENT_FRONTIER_CALCULATE_LEC_VALUES, value, Parameters.ParameterSetID)
            End Set
        End Property
        'A1617 ==

        'A1465 ===
        Public Property Riskion_BayesianMode As Integer
            Get
                Return Parameters.GetParameterValue(RISK_BAYESIAN_MODE, 0)
            End Get
            Set(value As Integer)
                Parameters.SetParameterValue(RISK_BAYESIAN_MODE, value, Parameters.ParameterSetID)
            End Set
        End Property 'A1465 ==

        'A1481 ===
        Public Property Riskion_RiskPlotMode() As Integer '-1 - none, 0 - w/o controls, 1 - with controls, 2 - split, 3 - both
            Get
                Return Parameters.GetParameterValue(RISK_PLOT_SERIES_SHOWN, 0)
            End Get
            Set(value As Integer)
                Parameters.SetParameterValue(RISK_PLOT_SERIES_SHOWN, value, Parameters.ParameterSetID)
            End Set
        End Property 'A1481 ==
        
        ''A1483 ===
        'Public Property Riskion_ControlsSelectionMode As Integer '-1 undefined, 0 - Manually selected, 1 - Optimized based on computed input, 2 - Optimized based on simulated input
        '    Get
        '        Return Parameters.GetParameterValue(RISK_CONTROLS_ACTUAL_SELECTION_MODE, 0)
        '    End Get
        '    Set(value As Integer)
        '        Parameters.SetParameterValue(RISK_CONTROLS_ACTUAL_SELECTION_MODE, value, Parameters.ParameterSetID)
        '    End Set
        'End Property 'A1483 ==

         'A1509 ===
        Public Property Riskion_RiskPlotManualZoomParams As String
            Get
                Return Parameters.GetParameterValue(RISK_PLOT_MANUAL_ZOOM_PARAMS, "")
            End Get
            Set(value As String)
                Parameters.SetParameterValue(RISK_PLOT_MANUAL_ZOOM_PARAMS, value, Parameters.ParameterSetID)
            End Set
        End Property 'A1481 ==

        'A1483 ===
        Public Property Riskion_ControlsActualSelectionMode As Integer '0 - Manually selected, 1 - Optimized
            Get
                Return Parameters.GetParameterValue(RISK_CONTROLS_ACTUAL_SELECTION_MODE, 0)
            End Get
            Set(value As Integer)
                Parameters.SetParameterValue(RISK_CONTROLS_ACTUAL_SELECTION_MODE, value, Parameters.ParameterSetID)
            End Set
        End Property 'A1483 ==

        Public Property MyRiskReward_ShowEventsMode As string 'all, risks, rewards
            Get
                Return Parameters.GetParameterValue("MyRiskReward_ShowEventsMode", "all")
            End Get
            Set(value As string)
                Parameters.SetParameterValue("MyRiskReward_ShowEventsMode", value, -1)
            End Set
        End Property

        Public Property MyRiskReward_ShowDescriptions As Boolean
            Get
                Return Parameters.GetParameterValue("MyRiskReward_ShowDescriptions", True)
            End Get
            Set(value As Boolean)
                Parameters.SetParameterValue("MyRiskReward_ShowDescriptions", value, -1)
            End Set
        End Property

        'A1607 ===
        Public Property CS_TreeMode As Integer
            Get
                Return Parameters.GetParameterValue(CS_TREE_MODE, 0)
            End Get
            Set(value As Integer)
                Parameters.SetParameterValue(CS_TREE_MODE, value, Parameters.ParameterSetID)
            End Set
        End Property

        Public Property CS_BoardMode As Integer
            Get
                Return Parameters.GetParameterValue(CS_BOARD_MODE, 0)
            End Get
            Set(value As Integer)
                Parameters.SetParameterValue(CS_BOARD_MODE, value, Parameters.ParameterSetID)
            End Set
        End Property

        Public Property CS_MeetingSynchMode As Integer
            Get
                Return Parameters.GetParameterValue(CS_MEETING_SYNCH_MODE, 0)
            End Get
            Set(value As Integer)
                Parameters.SetParameterValue(CS_MEETING_SYNCH_MODE, value, Parameters.ParameterSetID)
            End Set
        End Property

        'Public Property CS_MeetingWhiteboardMode As String
        '    Get
        '        Return Parameters.GetParameterValue(CS_MEETING_WHITEBOARD_MODE, "normal")
        '    End Get
        '    Set(value As String)
        '        Parameters.SetParameterValue(CS_MEETING_WHITEBOARD_MODE, value, Parameters.ParameterSetID)
        '    End Set
        'End Property

        Public Property CS_MeetingWhiteboardDrawingData As String
            Get
                If CS_DrawingLifeTime < Integer.MaxValue Then Return ""
                Return Parameters.GetParameterValue(CS_MEETING_WHITEBOARD_DRAWING_DATA, "")
            End Get
            Set(value As String)
                Parameters.SetParameterValue(CS_MEETING_WHITEBOARD_DRAWING_DATA, value, Parameters.ParameterSetID)
            End Set
        End Property

        Public Property CS_DrawingLifeTime As Integer
            Get
                Return Parameters.GetParameterValue(CS_DRAWING_LIFE_TIME, 15000)
            End Get
            Set(value As Integer)
                Parameters.SetParameterValue(CS_DRAWING_LIFE_TIME, value, Parameters.ParameterSetID)
            End Set
        End Property

        Public Property CS_MeetingState As Integer
            Get
                Return Parameters.GetParameterValue(CS_MEETING_STATE, 0)
            End Get
            Set(value As Integer)
                Parameters.SetParameterValue(CS_MEETING_STATE, value, Parameters.ParameterSetID)
            End Set
        End Property

        Public Property CS_MeetingOwner As Integer
            Get
                Return Parameters.GetParameterValue(CS_MEETING_OWNER, -1)
            End Get
            Set(value As Integer)
                Parameters.SetParameterValue(CS_MEETING_OWNER, value, Parameters.ParameterSetID)
            End Set
        End Property

        Public Property CS_UserList As String
            Get
                Return Parameters.GetParameterValue(CS_USER_LIST, "[]")
            End Get
            Set(value As String)
                Parameters.SetParameterValue(CS_USER_LIST, value, Parameters.ParameterSetID)
            End Set
        End Property

        Public Property CS_DefaultAlternativeTitle As String
            Get
                Return Parameters.GetParameterValue(CS_DEFAULT_ALT_TITLE, "")
            End Get
            Set(value As String)
                Parameters.SetParameterValue(CS_DEFAULT_ALT_TITLE, value, Parameters.ParameterSetID)
            End Set
        End Property

        Public Property CS_DefaultSourceTitle As String
            Get
                Return Parameters.GetParameterValue(CS_DEFAULT_SOURCE_TITLE, "")
            End Get
            Set(value As String)
                Parameters.SetParameterValue(CS_DEFAULT_SOURCE_TITLE, value, Parameters.ParameterSetID)
            End Set
        End Property

        Public Property CS_DefaultObjectiveTitle As String
            Get
                Return Parameters.GetParameterValue(CS_DEFAULT_OBJ_TITLE, "")
            End Get
            Set(value As String)
                Parameters.SetParameterValue(CS_DEFAULT_OBJ_TITLE, value, Parameters.ParameterSetID)
            End Set
        End Property

        Public Property CS_DefaultAlternativeColor As String
            Get
                Return Parameters.GetParameterValue(CS_DEFAULT_ALT_COLOR, "#f0faeb")
            End Get
            Set(value As String)
                Parameters.SetParameterValue(CS_DEFAULT_ALT_COLOR, value, Parameters.ParameterSetID)
            End Set
        End Property

        Public Property CS_DefaultSourceColor As String
            Get
                Return Parameters.GetParameterValue(CS_DEFAULT_SOURCE_COLOR, "#f9fafc")
            End Get
            Set(value As String)
                Parameters.SetParameterValue(CS_DEFAULT_SOURCE_COLOR, value, Parameters.ParameterSetID)
            End Set
        End Property

        Public Property CS_DefaultObjectiveColor As String
            Get
                Return Parameters.GetParameterValue(CS_DEFAULT_OBJ_COLOR, "#f2f0fc")
            End Get
            Set(value As String)
                Parameters.SetParameterValue(CS_DEFAULT_OBJ_COLOR, value, Parameters.ParameterSetID)
            End Set
        End Property

        Public Property CS_ColorCodingByUser As Boolean
            Get
                Return Parameters.GetParameterValue(CS_COLOR_CODING_BY_USER, True)
            End Get
            Set(value As Boolean)
                Parameters.SetParameterValue(CS_COLOR_CODING_BY_USER, value, Parameters.ParameterSetID)
            End Set
        End Property

        Public Property CS_MeetingLockedByPM As Boolean
            Get
                Return Parameters.GetParameterValue(CS_MEETING_LOCKED_BY_PM, False)
            End Get
            Set(value As Boolean)
                Parameters.SetParameterValue(CS_MEETING_LOCKED_BY_PM, value, Parameters.ParameterSetID)
            End Set
        End Property        

        Public Property CS_ItemWidth As Integer
            Get
                Return Parameters.GetParameterValue(CS_ITEM_WIDTH, 200)
            End Get
            Set(value As Integer)
                Parameters.SetParameterValue(CS_ITEM_WIDTH, value, Parameters.ParameterSetID)
            End Set
        End Property

        Public Property CS_ItemHeight As Integer
            Get
                Return Parameters.GetParameterValue(CS_ITEM_HEIGHT, 50)
            End Get
            Set(value As Integer)
                Parameters.SetParameterValue(CS_ITEM_HEIGHT, value, Parameters.ParameterSetID)
            End Set
        End Property
        'A1607 ==

        'A1499 ===
        Public Property Riskion_Register_Timestamp As Boolean
            Get
                Return Parameters.GetParameterValue(RISK_REGISTER_TIMESTAMP, False)
            End Get
            Set(value As Boolean)
                Parameters.SetParameterValue(RISK_REGISTER_TIMESTAMP, value, Parameters.ParameterSetID)
            End Set
        End Property 'A1499 ==

        'A1609 ===
        Public Property LEC_ShowIntersections As Boolean
            Get
                Return Parameters.GetParameterValue(LEC_SHOW_INTERSECTIONS, False)
            End Get
            Set(value As Boolean)
                Parameters.SetParameterValue(LEC_SHOW_INTERSECTIONS, value, Parameters.ParameterSetID)
            End Set
        End Property

        Public Property LEC_ShowFrequencyCharts As Boolean
            Get
                Return Parameters.GetParameterValue(LEC_SHOW_FREQUENCY_CHARTS, False)
            End Get
            Set(value As Boolean)
                Parameters.SetParameterValue(LEC_SHOW_FREQUENCY_CHARTS, value, Parameters.ParameterSetID)
            End Set
        End Property 'A1499 ==
        
        Public Property LEC_LogarithmicScale As Boolean
            Get
                Return Parameters.GetParameterValue("LEC_LogScale", False)
            End Get
            Set(value As Boolean)
                Parameters.SetParameterValue("LEC_LogScale", value, Parameters.ParameterSetID)
            End Set
        End Property

        Public Property LEC_ShowWithoutControls As Boolean
            Get
                Return Parameters.GetParameterValue("LEC_WOControls", True)
            End Get
            Set(value As Boolean)
                Parameters.SetParameterValue("LEC_WOControls", value, Parameters.ParameterSetID)
            End Set
        End Property

        Public Property LEC_ShowWithControls As Boolean
            Get
                Return Parameters.GetParameterValue("LEC_WControls", True)
            End Get
            Set(value As Boolean)
                Parameters.SetParameterValue("LEC_WControls", value, Parameters.ParameterSetID)
            End Set
        End Property

        Public Property RiskionCalculationsMode As LikelihoodsCalculationMode
            Get
                Return LikelihoodsCalculationMode.Regular
                Return Parameters.GetParameterValue(RISKION_CALCULATIONS_MODE, CInt(LikelihoodsCalculationMode.Regular))
            End Get
            Set(value As LikelihoodsCalculationMode)
                Parameters.SetParameterValue(RISKION_CALCULATIONS_MODE, CInt(value), Parameters.ParameterSetID)
            End Set
        End Property

        Public Property RiskionShowRiskReductionOptions As Boolean
            Get 
                Return Parameters.GetParameterValue(RISKION_SHOW_RISK_REDUCTION_OPTIONS, False)
            End Get
            Set(value As Boolean)
                Parameters.SetParameterValue(RISKION_SHOW_RISK_REDUCTION_OPTIONS, value)
            End Set
        End Property            

        Public Property LEC_ShowIntersectionsOptions As String
            Get
                Return Parameters.GetParameterValue(RISKION_LEC_SHOW_INTERSECTIONS_OPTIONS, "{red_line: true, green_line: true, red_line_intersection: false, green_line_intersection: false, red_line_intersection_wc: false, green_line_intersection_wc: false}")
            End Get
            Set(value As String)
                Parameters.SetParameterValue(RISKION_LEC_SHOW_INTERSECTIONS_OPTIONS, value)
            End Set
        End Property

        ' D6502 ===
        Public Property ProjectReports As String
            Get
                Return Parameters.GetParameterValue(PROJECTS_REPORTS, "")
            End Get
            Set(value As String)
                Parameters.SetParameterValue(PROJECTS_REPORTS, value.Trim, -1)
            End Set
        End Property
        ' D6502 ==

        ' D6968 ===
        Public Property TimeFrame As String
            Get
                Return Parameters.GetParameterValue(PROP_TIMEFRAME, "")
            End Get
            Set(value As String)
                Parameters.SetParameterValue(PROP_TIMEFRAME, value.Trim, -1)
            End Set
        End Property

        Public Property Assumptions As String
            Get
                Return Parameters.GetParameterValue(PROP_ASSUMPTIONS, "")
            End Get
            Set(value As String)
                Parameters.SetParameterValue(PROP_ASSUMPTIONS, value.Trim, -1)
            End Set
        End Property
        ' D6968 ==

        ' D7566 ===
        Public Property SpecialMode As String
            Get
                Return Parameters.GetParameterValue(PROJECT_SPECIAL_MODE, "")
            End Get
            Set(value As String)
                Parameters.SetParameterValue(PROJECT_SPECIAL_MODE, value.Trim, -1)
            End Set
        End Property
        ' D7566 ==

        Dim mParams As clsProjectParameters = Nothing
        Public ReadOnly Property Parameters As clsProjectParameters
            Get
                Return mParams
            End Get
        End Property

        ' (!) ========= Using attributes =========

        ' D6850 ===
        Public Property DecimalDigits As Integer
            Get
                Dim retVal As Integer = 2
                retVal = CInt(Parameters.ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_SYNTHESIS_DECIMALS_ID, UNDEFINED_USER_ID))
                If retVal < 0 Then retVal = 0
                If retVal > 5 Then retVal = 5
                Return retVal
            End Get
            Set(value As Integer)
                With Parameters.ProjectManager                    
                    .Attributes.SetAttributeValue(ATTRIBUTE_SYNTHESIS_DECIMALS_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtLong, value, Guid.Empty, Guid.Empty)
                    .Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID, UNDEFINED_USER_ID)
                End With
            End Set
        End Property
        ' D6850 ==

        Public Function Save() As Boolean
            Parameters?.ProjectManager?.Reports?.Save(, False)    ' D6521
            Return Parameters.Save() ' D4364
        End Function

        Public Function Load() As Boolean
            Dim fRes As Boolean = Parameters.Load() ' D4364
            If fRes AndAlso Parameters IsNot Nothing Then Parameters.ProjectManager?.Reports?.Load()    ' D6521
            Return fRes ' D6521
        End Function

        Public Sub New(ProjectManager As clsProjectManager)
            mParams = New clsProjectParameters(ProjectManager)
        End Sub

    End Class
    ' D3786 ==

End Namespace
