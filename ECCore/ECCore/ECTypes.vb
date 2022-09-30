Imports ECCore
Imports System.Data.Common 'C0236
Imports System.IO 'C0304
Imports System.Runtime.Serialization

Namespace ECCore
    Public Module ECTypes
        Public Const COMBINED_USER_ID As Integer = -1 'C0552

        Public Const IDEAL_ALTERNATIVE_ID As Integer = -2
        Public Const IDEAL_ALTERNATIVE_NAME As String = "IDEAL"

        Public VERY_OLD_DATE As DateTime = New DateTime(1900, 1, 1, 1, 1, 1)
        Public UNDEFINED_DATE As DateTime = New DateTime(1, 1, 1, 0, 0, 0) 'C0369

        Public Const DEFAULT_RATING_SCALE_NAME As String = "Default Rating Scale"
        Public Const DEFAULT_RATING_SCALE_NAME_FOR_LIKELIHOOD As String = "Default Likelihood Scale"
        Public Const DEFAULT_RATING_SCALE_NAME_FOR_IMPACT As String = "Default Impact Scale"
        Public Const DEFAULT_RATING_SCALE_NAME_VULNERABILITY As String = "Default Event Vulnerability Scale"
        'Public Const DEFAULT_RATING_SCALE_NAME_CONTROLS As String = "Default Control Scale"
        Public Const DEFAULT_RATING_SCALE_NAME_CONTROLS As String = "Default Treatment Scale" 'A0825
        'Public Const DEFAULT_OUTCOMES_SCALE_NAME As String = "Default Outcomes Scale"
        Public Const DEFAULT_OUTCOMES_SCALE_NAME As String = "Default Probabilities Scale" 'A0809
        Public Const DEFAULT_PWOP_SCALE_NAME As String = "Default Pairwise of Percentages Scale" 'A0780
        Public Const DEFAULT_EXPECTED_VALUES_SCALE_NAME As String = "Default Expected Values Scale" 'A0780

        Public Const DEFAULT_REGULAR_UTILITY_CURVE_NAME As String = "Default Regular Utility Curve"
        Public Const DEFAULT_REGULAR_UTILITY_CURVE_FOR_LIKELIHOOD_NAME As String = "Default Utility Curve For Likelihood"
        Public Const DEFAULT_REGULAR_UTILITY_CURVE_FOR_IMPACT_NAME As String = "Default Utility Curve For Impact"
        Public Const DEFAULT_REGULAR_UTILITY_CURVE_FOR_CONTROLS_NAME As String = "Default Utility Curve For Controls"
        Public Const DEFAULT_REGULAR_UTILITY_CURVE_FOR_VULNERABILITY_NAME As String = "Default Utility Curve For Vulnerabilities"

        Public Const DEFAULT_ADVANCED_UTILITY_CURVE_NAME As String = "Default Advanced Utility Curve" 'C0706

        Public Const DEFAULT_STEP_FUNCTION_NAME As String = "Default Step Function" 'C0706
        Public Const DEFAULT_STEP_FUNCTION_FOR_LIKELIHOOD_NAME As String = "Default Step Function For Likelihood"
        Public Const DEFAULT_STEP_FUNCTION_FOR_IMPACT_NAME As String = "Default Step Function For Impact" 'C0706
        Public Const DEFAULT_STEP_FUNCTION_FOR_CONTROLS_NAME As String = "Default Step Function For Controls" 'C0706
        Public Const DEFAULT_STEP_FUNCTION_FOR_VULNERABILITY_NAME As String = "Default Step Function For Vulnerability" 'C0706

        ' D2394 ===
        Public Const DEFAULT_RATING_SCALE_DESC As String = ""
        Public Const DEFAULT_RATING_SCALE_DESC_FOR_LIKELIHOOD As String = "Default Ratings Scale for Sources and Vulnerabilities." + vbCrLf + "Participants can enter likelihoods between given intensities"
        Public Const DEFAULT_RATING_SCALE_DESC_FOR_IMPACT As String = ""
        Public Const DEFAULT_RATING_SCALE_DESC_VULNERABILITY As String = ""
        Public Const DEFAULT_RATING_SCALE_DESC_CONTROLS As String = ""
        Public Const DEFAULT_OUTCOMES_SCALE_DESC As String = ""
        Public Const DEFAULT_PWOP_SCALE_DESC As String = ""
        Public Const DEFAULT_EXPECTED_VALUES_SCALE_DESC As String = ""
        Public Const DEFAULT_REGULAR_UTILITY_CURVE_DESC As String = ""
        Public Const DEFAULT_ADVANCED_UTILITY_CURVE_DESC As String = ""
        Public Const DEFAULT_STEP_FUNCTION_DESC As String = ""
        ' D2394 ==

        ' D2256 ===
        'Public DEFAULT_RATINGSCALE_NAMES() As String = {"None", "Very low", "Low", "Low to moderately", "Moderately", "Moderately to considerably", "Considerably", "Highly likely", "Definitely"}
        'Public DEFAULT_RATINGSCALE_VALUES() As Single = {0, 0.04, 0.259, 0.36, 0.467, 0.556, 0.69, 0.862, 1}

        Public DEFAULT_RATING_SCALE_NAMES() As String = {"None", "A Tad", "Moderate", "Moderate to Good", "Good", "Good to Very Good", "Very Good", "Excellent", "Outstanding"}
        Public DEFAULT_RATING_SCALE_COMMENTS() As String = {"", "", "", "", "", "", "", "", ""} ' D2394
        Public DEFAULT_RATING_SCALE_VALUES() As Single = {0.0, 0.04, 0.259, 0.36, 0.467, 0.556, 0.69, 0.862, 1}

        Public DEFAULT_RATING_SCALE_LIKELIHOOD_NAMES() As String = {"Almost never", "Once a decade", "Once a year", "Rarely", "Occasionally", "Not uncommon", "50/50", "Very likely", "Certain"}    ' D2391
        Public DEFAULT_RATING_SCALE_LIKELIHOOD_COMMENTS() As String = {"One in ten thousand", "", "", "", "", "", "", "", ""}    ' D2394
        Public DEFAULT_RATING_SCALE_LIKELIHHOD_VALUES() As Single = {0.0001, 0.0003, 0.0027, 0.01, 0.05, 0.3, 0.5, 0.9, 1.0}    ' D2391

        Public DEFAULT_RATING_SCALE_IMPACT_NAMES() As String = {"None", "Just a tad", "Low", "Low to moderate", "Moderate", "Moderate to considerable", "Considerable", "Significant", "Extreme"}
        Public DEFAULT_RATING_SCALE_IMPACT_COMMENTS() As String = {"", "", "", "", "", "", "", "", ""}  ' D2394
        Public DEFAULT_RATING_SCALE_IMPACT_VALUES() As Single = {0.0, 0.04, 0.259, 0.36, 0.467, 0.556, 0.69, 0.862, 1}

        Public DEFAULT_RATING_SCALE_VULNERABILTIES_NAMES() As String = {"None", "Just a tad", "Low", "Low to moderate", "Moderate", "Moderate to considerable", "Considerable", "Significant", "Extreme"}
        Public DEFAULT_RATING_SCALE_VULNERABILTIES_COMMENTS() As String = {"", "", "", "", "", "", "", "", ""}  ' D2394
        Public DEFAULT_RATING_SCALE_VULNERABILTIES_VALUES() As Single = {0.0, 0.04, 0.259, 0.36, 0.467, 0.556, 0.69, 0.862, 1}

        Public DEFAULT_RATING_SCALE_CONTROLS_NAMES() As String = {"Not effective", "Tad effective", "Slightly effective", "Moderately Effective", "Half Effective", "Effective", "Very Effective", "Extremely Effective", "Completely Effective"}
        Public DEFAULT_RATING_SCALE_CONTROLS_COMMENTS() As String = {"", "", "", "", "", "", "", "", ""}
        Public DEFAULT_RATING_SCALE_CONTROLS_VALUES() As Single = {0.0, 0.01, 0.05, 0.3, 0.5, 0.75, 0.9, 0.95, 1}

        Public DEFAULT_OUTCOMES_NAMES() As String = {"0%", "10%", "25%", "50%", "75%", "90%", "100%"}
        Public DEFAULT_OUTCOMES_COMMENTS() As String = {"", "", "", "", "", "", ""} ' D2394
        Public DEFAULT_OUTCOMES_VALUES() As Single = {0.0, 0.1, 0.25, 0.5, 0.75, 0.9, 1.0}

        Public DEFAULT_PWOP_NAMES() As String = {"0%", "10%", "25%", "50%", "75%", "90%", "100%"}
        Public DEFAULT_PWOP_COMMENTS() As String = {"", "", "", "", "", "", ""} ' D2394
        Public DEFAULT_PWOP_VALUES() As Single = {0.0, 0.1, 0.25, 0.5, 0.75, 0.9, 1.0}

        Public DEFAULT_EXPECTED_VALUES_NAMES() As String = {"0%", "10%", "25%", "50%", "75%", "90%", "100%"} 'A0780
        Public DEFAULT_EXPECTED_VALUES_COMMENTS() As String = {"", "", "", "", "", "", ""}  ' D2394
        Public DEFAULT_EXPECTED_VALUES_VALUES() As Single = {0.0, 0.1, 0.25, 0.5, 0.75, 0.9, 1.0} 'A0780

        Public DEFAULT_NODENAME_GOAL As String = "Goal"
        Public DEFAULT_NODENAME_GOAL_LIKELIHOOD As String = "Sources"
        Public DEFAULT_NODENAME_GOAL_IMPACT As String = "Objectives/Assets/Consequences"
        ' D2256 ==

        Public Const NODE_NAME_MAX_LENGTH As Integer = 1000
        Public Const HIERARCHY_NAME_MAX_LENGTH As Integer = 1000
        Public Const MEASUREMENT_SCALE_NAME_LENGTH As Integer = 1000
        Public Const RATING_INTENSITY_NAME_MAX_LENGTH As Integer = 1000
        Public Const STEP_INTERVAL_NAME_MAX_LENGTH As Integer = 1000

        ' D0145 ===
        Public Const _DB_VERSION_CANVAS As Integer = 1
        Public Const _DB_VERSION_MAJOR As Integer = 1
        Public Const _DB_VERSION_MINOR As Integer = 46

        Private Const _DB_UNDEF_VERSION_CANVAS As Integer = 1
        Private Const _DB_UNDEF_VERSION_MAJOR As Integer = 1
        Private Const _DB_UNDEF_VERSION_MINOR As Integer = 0
        ' D0145 ==

        Public Const AHP_DB_DEFAULT_VERSION As Single = 3.03 'C0068
        'Public Const AHP_DB_LATEST_VERSION As Single = 3.18 'AS/11-10-15  
        Public Const AHP_DB_LATEST_VERSION As Single = 3.24 'AS/11-10-15 'AS/6-10-16 updated from 3.21 to 3.24

        Public Const UNDEFINED_DATA_INSTANCE_ID = Integer.MaxValue 'C0113

        Public Const GROUP_USER_ID As Integer = Integer.MaxValue 'C0148

        Public Const COMBINED_GROUPS_USERS_START_ID As Integer = -1000 'C0159 - starting from ID=-1000 there'll be fictive combined users
        Public Const COMBINED_GROUP_ALL_USERS_ID As Integer = Integer.MinValue + 1 'C0672

        Public ReadOnly ATTRIBUTE_GROUP_BY_INCONSISTENCY_ID As Guid = New Guid("{AE76264A-7FDE-4B04-B1AB-67276186181E}") 'A1157

        'Public Const DEFAULT_COMBINED_GROUP_NAME = "Default Combined Group" 'C0672 'C0721
        Public Const DEFAULT_COMBINED_GROUP_NAME = "All Participants" 'C0721
        Public Const ALL_USERS_COMBINED_GROUP_NAME = "All Users" 'C0672

        Public Const NEGATIVE_INFINITY As Single = Single.MinValue 'C0158
        Public Const POSITIVE_INFINITY As Single = Single.MaxValue 'C0158

        Public Const UNDEFINED_INTEGER_VALUE As Integer = Integer.MinValue 'C0306
        Public Const UNDEFINED_SINGLE_VALUE As Single = Single.MinValue 'C0306
        Public Const UNDEFINED_STRING_VALUE As String = "UNDEFINED_STRING_VALUE" 'C0306

        Public Const UNDEFINED_USER_ID As Integer = Integer.MinValue 'C0450
        Public Const DUMMY_PERMISSIONS_USER_ID As Integer = Integer.MinValue + 2

        Public _AHP_EXTRATABLES_LIST() As String = {"Categories", "CategoryValues", "DataMapping", "Dependencies", "GroupMembers", "Groups", "FundingPools", "MustsMustNots", "RAbenefits", "RABudgetLimits", "RAconstraints", "RAPortfolioScenarios", "RAproperties", "RArisks", "UDColsData", "UDColsDefs", "MProperties"} 'C0426 'C0825 (added MProperties)
        'Public _AHP__DYNAMIC_EXTRATABLES_LIST() As String = {"RArisks", "RABudgetLimits", "RAconstraints", "RAPortfolioScenarios", "FundingPools", "Groups", "GroupMembers", "MustsMustNots", "Dependencies", "DependenciesPeriods", "RAproperties", "RAbenefits"} 'C0426
        Public _AHP__DYNAMIC_EXTRATABLES_LIST() As String = {"RABudgetLimits", "RAconstraints", "RAPortfolioScenarios", "FundingPools", "Groups", "GroupMembers", "MustsMustNots", "Dependencies", "DependenciesPeriods", "RAproperties", "RAbenefits"} 'C0426

        Public Const DEFAULT_USER_WEIGHT As Single = 0.5 'C0745

        Public Const USE_DATA_INSTANCES As Boolean = False 'C0754
        Public Const USE_COMBINED_FOR_RESTRICTED As Boolean = True 'C0754

        Public Const RA_CostID As String = "c5488b18-ace3-4711-835a-28b0bbb1cc87"   ' D3918
        Public RA_Cost_GUID As New Guid(RA_CostID)     ' D3918

        Public Const EFFICIENT_FRONTIER_DEFAULT_NUMBER_OF_STEPS As Integer = 20

        'Public Enum DBProviderType 'C0235 'C0247 - moved this declaration to GenericDBAccess
        '    dbptSQLClient = 0
        '    dbptOLEDB = 1
        '    dbptOracle = 2
        '    dbptODBC = 3
        'End Enum

        'Sub CallbackWorkerProgressText(ByVal ProgressValue As Integer, ByVal ProgressText As String)
        Public Delegate Sub CallbackWorkerProgressTextFunction(ByVal ProgressValue As Integer, ByVal ProgressText As String)

        Public Enum DiagonalsEvaluation
            deAll = 0
            deFirst = 1
            deFirstAndSecond = 2
        End Enum

        Public Enum CopyJudgmentsMode
            Replace = 0
            UpdateAndAddMissing = 1
            AddMissing = 2
        End Enum

        <DataContract(), Flags()>
        Public Enum RolesToSendType
            <EnumMember()> rstAll = 0
            <EnumMember()> rstObjectivesOnly = 1
            <EnumMember()> rstAlternativesOnly = 2
        End Enum

        <DataContract()>
        Public Class RolesStatistics
            <DataMember()> Public Property ObjectiveID() As Guid
            <DataMember()> Public Property AlternativeID() As Guid
            <DataMember()> Public Property AllowedCount() As Integer
            <DataMember()> Public Property RestrictedCount() As Integer
            <DataMember()> Public Property EvaluatedCount() As Integer
        End Class

        <DataContract(), Flags()> _
        Public Enum NodeMoveAction 'C0621
            'nmaAsChildOfNode = 0
            'nmaBeforeNode = 1
            'nmaAfterNode = 2
            <EnumMember()> nmaBeforeNode = 1       'A0680
            <EnumMember()> nmaAsChildOfNode = 2    'A0680
            <EnumMember()> nmaAfterNode = 4        'A0680
        End Enum

        <DataContract()>
        Public Enum RiskNodeType 'A0829
            <EnumMember()> ntUncertainty = 0
            <EnumMember()> ntCategory = 1
            <EnumMember()> ntOBSOLETE = 2
            <EnumMember()> ntReadOnly = 3
        End Enum

        <DataContract()> _
        Public Class KnownLikelihoodDataContract
            <DataMember()> Public Property ID() As Integer
            <DataMember()> Public Property GuidID() As Guid
            <DataMember()> Public Property NodeName() As String
            <DataMember()> Public Property Value() As Double
            <DataMember()> Public Property NewValue() As Double
        End Class

        Public Class clsStatisticalDataItem 'A1296 - for Bayesian Updating feature
            Public Property TimePeriodName As String = ""
            Public Property Data() As Double = UNDEFINED_INTEGER_VALUE
        End Class

        Public Enum RolesValueType 'C1060
            rvtUndefined = 0
            rvtRestricted = 1
            rvtAllowed = 2
        End Enum

        Public Enum ShowResultsModes As Integer 'A1378
            rmComputed = 0
            rmSimulated = 1
            'rmBoth = 2
        End Enum

        <Serializable()> Public Enum LocalNormalizationType
            ntNormalizedForAll = 0
            ntNormalizedMul100 = 1
            ntNormalizedSum100 = 2
            ntUnnormalized = 3
        End Enum

        Public Enum CalculationTargetType 'C0551
            cttUser = 0
            cttDataInstance = 1
            cttCombinedGroup = 2
        End Enum

        Public Enum CombinedCalculationsMode 'C0945
            cmAIJ = 0
            cmAIPTotals = 2
        End Enum

        ' D4180 ===
        Public Enum IDColumnModes
            'None = -1  ' -D4323 due to add separate option
            UniqueID = 0
            IndexID = 1
            Rank = 2
        End Enum
        ' D4180 ==

        'A1343 ===
        Public Enum ECWRTStates
            wsGoal = 0
            wsSelectedNode = 1
        End Enum
        'A1343 ==

        <Serializable()> Public Class clsCalculationTarget 'C0551 ' D0462
            Public Property TargetType() As CalculationTargetType
            Public Property Target() As Object

            Public Function GetUserID() As Integer
                Select Case TargetType
                    Case CalculationTargetType.cttUser
                        Return CType(Target, clsUser).UserID
                    Case CalculationTargetType.cttCombinedGroup
                        Return CType(Target, clsCombinedGroup).CombinedUserID
                    Case CalculationTargetType.cttDataInstance
                        Return CType(Target, clsDataInstance).User.UserID
                End Select
            End Function

            Public Sub New()
            End Sub

            Public Sub New(ByVal TargetType As CalculationTargetType, ByVal Target As Object)
                Me.TargetType = TargetType
                Me.Target = Target
            End Sub
        End Class

        Public Enum ECUserType 'C0223
            utNormal = 0
            utCombined = 1
            utDataInstance = 2
        End Enum

        Public Enum ECGroupType 'C0152
            gtEvaluation = 0
            gtCombined = 1
        End Enum

        Public Enum AHPDataType 'C304
            dtObjective = 0
            dtAlternative = 1
            dtUser = 2
        End Enum

        <Serializable()> Public Class UserEvaluationProgressData
            Public Property ID As Integer
            Public Property Email() As String
            Public Property EvaluatedCount() As Integer
            Public Property TotalCount() As Integer
            Public Property LastJudgmentTime() As Nullable(Of DateTime)
        End Class


        <Serializable()> Public Class clsMaxOutData 'C0382
            Public HasFixedLocalPriority As Boolean
            Public FixedLocalPriority As Single
            Public HasMaxPriorityValue As Boolean
            Public MaxPriorityValue As Single
            Public HasCalculatedLocalPriority As Boolean
        End Class

        <Serializable()> Public Class clsDataInstance
            Private mID As Integer
            'Private mGuidID As Guid 'C0259 'C0261
            Private mName As String
            Private mComment As String
            Private mUser As clsUser
            Private mEvaluatorUser As clsUser 'C0646

            Public Property ID() As Integer
                Get
                    Return mID
                End Get
                Set(ByVal value As Integer)
                    mID = value
                End Set
            End Property

            'C0261===
            'Public Property GuidID() As Guid 'C0259
            '    Get
            '        Return mGuidID
            '    End Get
            '    Set(ByVal value As Guid)
            '        mGuidID = value
            '    End Set
            'End Property
            'C0261==

            Public Property Name() As String
                Get
                    Return mName
                End Get
                Set(ByVal value As String)
                    mName = value
                End Set
            End Property

            Public Property Comment() As String
                Get
                    If mComment Is Nothing Then
                        mComment = ""
                    End If

                    Return mComment
                End Get
                Set(ByVal value As String)
                    mComment = value
                End Set
            End Property

            Public Property User() As clsUser
                Get
                    Return mUser
                End Get
                Set(ByVal value As clsUser)
                    mUser = value
                End Set
            End Property

            Public Property EvaluatorUser() As clsUser 'C0646
                Get
                    Return mEvaluatorUser
                End Get
                Set(ByVal value As clsUser)
                    mEvaluatorUser = value
                End Set
            End Property

            Public Sub New() 'C0260
                'mGuidID = Guid.NewGuid 'C0261
            End Sub
        End Class

        Public Enum SynchronousEvaluationMode 'C0086
            semNone = 0
            semOnline = 1
            semVotingBox = 2
            semByFacilitatorOnly = 3
        End Enum

        Public Enum ECMeasureMode
            mmImportance = 0
            mmPreference = 1
            mmLikelihood = 2
        End Enum

        Public Enum ECHierarchyType
            htModel = 0
            htAlternative = 1
            htMeasure = 2
        End Enum

        Public Enum ECJudgmentsType 'A1341
            jtAlternatives = 0
            jtObjectives = 1
            jtObjectivesAndAlternatives = 2
            jtControls = 3
        End Enum

        Public Enum ECHierarchyID
            hidLikelihood = 0
            hidImpact = 2
        End Enum

        Public Enum ECSynthesisMode
            smDistributive = 0
            smIdeal = 1
            smNone = -1 'C0955
        End Enum

        <Serializable()> Public Class ECSAData 'Sensitivity Analysis Data - to hold components
            Public NodeID As Integer ' ID of node which is a child of a node for which we are doing sensitivity analysis
            Public Value As Single ' component value
        End Class

        Public Enum ECModelStorageType
            mstXMLFile = 0
            mstAHPFile = 1
            mstAHPDatabase = 2
            mstXMLReader = 3
            mstCanvasDatabase = 4
            mstCanvasStreamDatabase = 5 'C0259
            mstBinaryStream = 6 'C0259
            mstAHPSStream = 7 'C0378
            mstAHPSFile = 8 'C0770
            mstTextFile = 9 'D2132
        End Enum

        <Serializable()> Public Class ECCanvasDatabaseVersion 'C0023
            Public Property CanvasVersion() As Integer
            Public Property MajorVersion() As Integer
            Public Property MinorVersion() As Integer
            Public Property SeparatorChar As Char = "."

            Public Sub New()
            End Sub

            Public Sub New(CanvasVersion As Integer, MajorVersion As Integer, MinorVersion As Integer)
                Me.CanvasVersion = CanvasVersion
                Me.MajorVersion = MajorVersion
                Me.MinorVersion = MinorVersion
            End Sub

            Public Function GetVersionString() As String
                Return CanvasVersion.ToString + SeparatorChar +
                        MajorVersion.ToString + SeparatorChar +
                        MinorVersion.ToString
            End Function

            Public Function ParseString(ByVal VersionString As String) As Boolean
                If VersionString = "" Then Return False

                If VersionString.Length < 5 Then Return False

                Dim i As Integer
                Dim j As Integer
                Dim s As String

                i = 0
                j = VersionString.IndexOf(SeparatorChar, i)
                s = VersionString.Substring(i, j - i)
                Dim vCanvas As Integer
                If Not Integer.TryParse(s, vCanvas) Then
                    vCanvas = -1
                End If

                i = j + 1
                j = VersionString.IndexOf(SeparatorChar, i)
                s = VersionString.Substring(i, j - i)
                Dim vMajor As Integer
                If Not Integer.TryParse(s, vMajor) Then
                    vMajor = -1
                End If

                s = VersionString.Substring(j + 1)
                Dim vMinor As Integer
                If Not Integer.TryParse(s, vMinor) Then
                    vMinor = -1
                End If

                If (vCanvas = -1) Or (vMajor = -1) Or (vMinor = -1) Then
                    Return False
                Else
                    CanvasVersion = vCanvas
                    MajorVersion = vMajor
                    MinorVersion = vMinor
                    Return True
                End If
            End Function
        End Class

        Public Function GetCurrentDBVersion() As ECCanvasDatabaseVersion
            Return New ECCanvasDatabaseVersion(_DB_VERSION_CANVAS, _DB_VERSION_MAJOR, _DB_VERSION_MINOR)
        End Function

        Private Function GetUndefinedDBVersion() As ECCanvasDatabaseVersion
            Return New ECCanvasDatabaseVersion(_DB_UNDEF_VERSION_CANVAS, _DB_UNDEF_VERSION_MAJOR, _DB_UNDEF_VERSION_MINOR)
        End Function

        Public Function GetDBVersion_CanvasStreamDatabase(ByVal ConnectionString As String, ByVal ProviderType As DBProviderType, ByVal ModelID As Integer) As ECCanvasDatabaseVersion 'C0345
            Dim res As ECCanvasDatabaseVersion = GetCurrentDBVersion()
            If Not CheckDBConnection(ProviderType, ConnectionString) Then Return res

            Using dbConnection As DbConnection = GetDBConnection(ProviderType, ConnectionString)
                dbConnection.Open()
                Dim dbReader As DbDataReader
                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection

                Try
                    oCommand.CommandText = "SELECT * FROM ModelStructure WHERE ProjectID=? AND StructureType=?"
                    oCommand.Parameters.Clear()
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "StructureType", CInt(StructureType.stModelVersion)))

                    dbReader = DBExecuteReader(ProviderType, oCommand)

                    Dim strVersion As String = ""

                    If Not dbReader Is Nothing Then
                        While dbReader.Read
                            strVersion = ByteArrayToString(dbReader("Stream"))
                        End While
                    End If

                    Dim dbversion As New ECCanvasDatabaseVersion
                    If dbversion.ParseString(strVersion) Then
                        res = dbversion
                    End If
                    dbReader.Close()
                Catch ex As Exception
                    'res = GetCurrentDBVersion() 'C0399
                Finally
                    oCommand = Nothing
                    dbConnection.Close()
                End Try
            End Using

            Return res
        End Function

        'Public Function GetDBVersion_AHPSStream(Streaam As MemoryStream) As ECCanvasDatabaseVersion
        '    Return Nothing
        'End Function
        Public Function GetDBVersion_AHPSFile(ByVal FilePath As String) As ECCanvasDatabaseVersion
            Dim res As ECCanvasDatabaseVersion = GetCurrentDBVersion()

            Dim FS As FileStream = Nothing
            Try
                FS = New FileStream(FilePath, FileMode.Open, FileAccess.Read)

                FS.Seek(0, SeekOrigin.Begin)
                Dim BR As New BinaryReader(FS)

                BR.ReadInt32()
                BR.ReadInt32()
                Dim length As Integer = BR.ReadInt32()

                Dim strVersion As String = ""

                strVersion = ByteArrayToString(BR.ReadBytes(length))

                Dim dbversion As New ECCanvasDatabaseVersion
                If dbversion.ParseString(strVersion) Then
                    res = dbversion
                End If
            Finally
                If FS IsNot Nothing Then
                    FS.Close()
                End If
            End Try

            Return res
        End Function

        Public Function GetDBVersion(ByVal StorageType As ECModelStorageType, ByVal Location As String, ByVal ProviderType As DBProviderType, Optional ByVal ModelID As Integer = -1) As ECCanvasDatabaseVersion
            Select Case StorageType
                Case ECModelStorageType.mstCanvasStreamDatabase
                    Return GetDBVersion_CanvasStreamDatabase(Location, ProviderType, ModelID)
                Case ECModelStorageType.mstAHPSFile
                    Return GetDBVersion_CanvasStreamDatabase(Location, ProviderType, ModelID)
            End Select
            Return GetUndefinedDBVersion()
        End Function

        Public Function IsEqualCanvasDBVersions(ByVal DBVersion1 As ECCanvasDatabaseVersion, ByVal DBVersion2 As ECCanvasDatabaseVersion) As Boolean
            If DBVersion1 Is Nothing OrElse DBVersion2 Is Nothing Then Return False
            Return (DBVersion1.CanvasVersion = DBVersion2.CanvasVersion) And (DBVersion1.MajorVersion = DBVersion2.MajorVersion) And (DBVersion1.MinorVersion = DBVersion2.MinorVersion)
        End Function

        Public Enum ECAltsDefaultContribution
            adcNone = 0
            adcFull = 1
        End Enum

        <Serializable()> Public Class clsAHPUserData
            Public Weight As Single = UNDEFINED_SINGLE_VALUE 'C0306
            Public Organization As String = UNDEFINED_STRING_VALUE 'C0306
            Public Keypad As Integer = UNDEFINED_INTEGER_VALUE 'C0306
            Public Wave As Integer = UNDEFINED_INTEGER_VALUE 'C0306
            Public Location As String = UNDEFINED_STRING_VALUE 'C0306
            Public Eval As String = UNDEFINED_STRING_VALUE 'C0306
            Public EvalCluster As String = UNDEFINED_STRING_VALUE 'C0306
            Public RoleWritingType As Integer = UNDEFINED_INTEGER_VALUE 'C0306
            Public RoleViewingType As Integer = UNDEFINED_INTEGER_VALUE 'C0306
            'Public LastChanged As DateTime
            'Public ProgressStatus As Integer

            Public Function FromStream(ByVal Stream As MemoryStream) As Boolean
                Dim BR As New BinaryReader(Stream)

                Dim res As Boolean = True
                Try
                    Weight = BR.ReadSingle
                    Organization = BR.ReadString
                    Keypad = BR.ReadInt32
                    Wave = BR.ReadInt32
                    Location = BR.ReadString
                    Eval = BR.ReadString
                    EvalCluster = BR.ReadString
                    RoleWritingType = BR.ReadInt32
                    RoleViewingType = BR.ReadInt32
                Catch ex As Exception
                    res = False
                Finally
                    BR.Close()
                End Try

                Return res
            End Function

            Public Function ToStream() As MemoryStream
                Dim MS As New MemoryStream
                Dim BW As New BinaryWriter(MS)

                BW.Write(Weight)
                BW.Write(Organization)
                BW.Write(Keypad)
                BW.Write(Wave)
                BW.Write(Location)
                BW.Write(Eval)
                BW.Write(EvalCluster)
                BW.Write(RoleWritingType)
                BW.Write(RoleViewingType)

                BW.Close()

                Return MS
            End Function
        End Class

        <Serializable()> Public Class clsUser
            Public Property UserID() As Integer

            Public Property UserGuidID() As Guid = Guid.NewGuid

            Public Property UserName() As String = ""

            Public Property UserEMail() As String = ""

            Public Property Active() As Boolean = True

            Public Property EvaluationGroup() As clsGroup = Nothing

            Public Property AHPUserData() As clsAHPUserData 'C0304

            Public Property SyncEvaluationMode() As SynchronousEvaluationMode = SynchronousEvaluationMode.semOnline

            Public Property IncludedInSynchronous() As Boolean = False

            Public Property VotingBoxID() As Integer 'C0086

            Public Property DataInstanceID() As Integer = UNDEFINED_DATA_INSTANCE_ID

            Public Property Weight() As Single = DEFAULT_USER_WEIGHT

            Public LastJudgmentTime As DateTime = VERY_OLD_DATE

            'A2003 ===
            Public ReadOnly Property DisplayName As String
                Get
                    Return If(UserName = "", UserEMail, UserName)
                End Get
            End Property

            Public ReadOnly Property HTMLDisplayName As String
                Get
                    Return String.Format("<span title=""{1}"">{0}</span>", DisplayName, UserEMail)
                End Get
            End Property
            'A2003 ==

            Public Function Clone() As clsUser
                Dim newUser As New clsUser
                newUser.UserID = Me.UserID
                newUser.UserEMail = Me.UserEMail
                newUser.UserName = Me.UserName
                newUser.Active = Me.Active
                newUser.DataInstanceID = Me.DataInstanceID 'C0113
                newUser.Weight = Me.Weight 'C0745
                newUser.VotingBoxID = Me.VotingBoxID 'C0113
                newUser.IncludedInSynchronous = Me.IncludedInSynchronous 'C0113
                newUser.SyncEvaluationMode = Me.SyncEvaluationMode 'C0113
                newUser.EvaluationGroup = Me.EvaluationGroup 'C0148
                newUser.UserGuidID = Me.UserGuidID 'C0259
                newUser.AHPUserData = Me.AHPUserData 'C0304
                Return newUser
            End Function
        End Class

        <Serializable()> Public Class clsHierarchyStringsParser
            Private mHierarchy As clsHierarchy
            Private mParentNode As clsNode
            Private mAnalyzeStructure As Boolean = True
            Private mStartNodeID As Integer

            Public Property Hierarchy() As clsHierarchy
                Get
                    Return mHierarchy
                End Get
                Set(ByVal value As clsHierarchy)
                    mHierarchy = value
                End Set
            End Property

            Public Property ParentNode() As clsNode
                Get
                    Return mParentNode
                End Get
                Set(ByVal value As clsNode)
                    mParentNode = value
                End Set
            End Property

            Public Property AnalyzeStructure() As Boolean
                Get
                    Return mAnalyzeStructure
                End Get
                Set(ByVal value As Boolean)
                    mAnalyzeStructure = value
                End Set
            End Property

            Private Function GetStringIdent(ByVal str As String) As Integer
                Return (str.Length - String.Copy(str).TrimStart(Nothing).Length)
            End Function

            Private Sub ParseNode(ByVal strings As ArrayList, ByRef tNewNodesIDs As List(Of Guid), ByRef StringNumber As Integer, ByVal node As clsNode) 'A1506
                If strings Is Nothing OrElse node Is Nothing Then
                    Return
                End If

                Dim nd As clsNode

                Dim ident As Integer = GetStringIdent(strings(StringNumber))
                StringNumber = StringNumber + 1

                Dim stringSeparator() As String = {vbTab} 'C0644

                While (StringNumber < strings.Count) AndAlso (GetStringIdent(strings(StringNumber)) > ident)
                    If AnalyzeStructure Then
                        nd = Hierarchy.AddNode(node.NodeID)
                    Else
                        'nd = Hierarchy.AddNode(-1) 'C0031
                        nd = Hierarchy.AddNode(mStartNodeID) 'C0031
                    End If
                    'nd.NodeName = String.Copy(strings(StringNumber)).TrimStart(Nothing) 'C0644
                    'C0644===
                    Dim s As String = String.Copy(strings(StringNumber)).TrimStart(Nothing)
                    Dim res() As String = s.Split(stringSeparator, 2, StringSplitOptions.None)
                    Dim extra() As String = res(0).Split(New String() {"--extra--"}, StringSplitOptions.RemoveEmptyEntries)
                    If extra.Length > 0 Then nd.NodeName = extra(0) Else nd.NodeName = res(0)
                    If extra.Length > 1 AndAlso extra(1) <> "" Then nd.RiskNodeType = If(extra(1) = "1" Or extra(1).ToLower = "true", RiskNodeType.ntCategory, RiskNodeType.ntUncertainty)
                    'nd.NodeName = res(0)

                    If res.Length > 1 Then 'C0649
                        If res(1) <> "" Then
                            nd.InfoDoc = res(1)
                        End If
                    End If
                    'C0644==
                    tNewNodesIDs.Add(nd.NodeGuidID) 'A1506
                    ParseNode(strings, tNewNodesIDs, StringNumber, nd)
                End While
            End Sub

            Public Sub Parse(ByVal Strings As ArrayList, ByRef tNewNodesIDs As List(Of Guid), Optional fAddOnTopOfAlternatives As Boolean = False) 'A1506
                If Strings Is Nothing OrElse Strings.Count = 0 Then
                    Return
                End If

                Dim minSpace As Integer = Integer.MaxValue
                Dim tmpStr As String = ""
                Dim wsCount As Integer = 0 ' whitespace characters count

                ' determine minimum whitespace characters count
                For Each str As String In Strings
                    tmpStr = String.Copy(str)
                    tmpStr = tmpStr.TrimStart(Nothing)
                    wsCount = str.Length - tmpStr.Length
                    If wsCount < minSpace Then
                        If GetStringIdent(str) <> CStr(str).Length Then
                            minSpace = wsCount
                        End If
                    End If
                Next

                For j As Integer = Strings.Count - 1 To 0 Step -1
                    If GetStringIdent(Strings(j)) = CStr(Strings(j)).Length Then
                        Strings.RemoveAt(j)
                    End If
                Next

                ' delete all strings from the beginning of the list that have more than minSpace whitespace characters in the beginning of the string
                'While (Strings(0).Length - String.Copy(Strings(0)).TrimStart(Nothing).Length) > minSpace
                '    Strings.RemoveAt(0)
                'End While

                ' shorten all strings by minSpace from the beginning
                Dim ident As Integer
                Dim FoundFirstGoal As Boolean = False
                For j As Integer = 0 To Strings.Count - 1
                    ident = Strings(j).Length - String.Copy(Strings(j)).TrimStart(Nothing).Length
                    If ident = minSpace Then
                        FoundFirstGoal = True
                    End If
                    Strings(j) = Strings(j).Remove(0, If((ident > minSpace) And Not FoundFirstGoal, ident, minSpace))
                Next

                If ParentNode Is Nothing Then
                    mStartNodeID = -1
                Else
                    mStartNodeID = ParentNode.NodeID
                End If

                Dim node As clsNode
                Dim i As Integer = 0
                Dim k As Integer = 0
                While (i < Strings.Count) AndAlso (GetStringIdent(Strings(i)) = 0)
                    'C0031===
                    'If AnalyzeStructure Then
                    '    node = Hierarchy.AddNode(startNodeID)
                    'Else
                    '    node = Hierarchy.AddNode(-1)
                    'End If
                    If ParentNode IsNot Nothing AndAlso ParentNode.IsAlternative Then mStartNodeID = -1
                    'node = Hierarchy.AddNode(mStartNodeID, True)
                    node = Hierarchy.AddNode(mStartNodeID)
                    'C0031==

                    If ParentNode IsNot Nothing AndAlso ParentNode.IsAlternative AndAlso Hierarchy.Nodes.IndexOf(ParentNode) + k < Hierarchy.Nodes.Count Then
                        Hierarchy.MoveNode(node, Hierarchy.Nodes(Hierarchy.Nodes.IndexOf(ParentNode) + k), NodeMoveAction.nmaAfterNode)
                        k += 1
                    End If

                    If Hierarchy.HierarchyType = ECHierarchyType.htAlternative AndAlso fAddOnTopOfAlternatives AndAlso k < Hierarchy.Nodes.Count Then
                        Hierarchy.MoveNode(node, Hierarchy.Nodes(k), NodeMoveAction.nmaBeforeNode)
                        k += 1
                    End If
                    'node.NodeName = Strings(i) 'C0644

                    'C0644===
                    Dim stringSeparator() As String = {vbTab}

                    Dim s As String = String.Copy(Strings(i)).TrimStart(Nothing)
                    Dim res() As String = s.Split(stringSeparator, 2, StringSplitOptions.None)
                    Dim extra() As String = res(0).Split(New String() {"--extra--"}, StringSplitOptions.RemoveEmptyEntries)
                    If extra.Length > 0 Then node.NodeName = extra(0) Else node.NodeName = res(0)
                    If extra.Length > 1 AndAlso extra(1) <> "" Then node.RiskNodeType = If(extra(1) = "1" Or extra(1).ToLower = "true", RiskNodeType.ntCategory, RiskNodeType.ntUncertainty)

                    If res.Length > 1 Then 'C0649
                        If res(1) <> "" Then
                            node.InfoDoc = res(1)
                        End If
                    End If
                    'C0644==
                    tNewNodesIDs.Add(node.NodeGuidID) 'A1506
                    ParseNode(Strings, tNewNodesIDs, i, node) 'A1506
                End While
                Hierarchy.ProjectManager.CreateHierarchyLevelValues(Hierarchy)
            End Sub

            Public Sub New(ByVal Hierarchy As clsHierarchy, Optional ByVal ParentNode As clsNode = Nothing, Optional ByVal AnalyzeStructure As Boolean = True)
                mHierarchy = Hierarchy
                mParentNode = ParentNode
                mAnalyzeStructure = AnalyzeStructure
            End Sub
        End Class

        <Serializable()> Public Class clsECIntensity
            Public ID As Integer
            Public Name As String = ""
            Public Comment As String = ""
        End Class

        Public Function GetCurrentECCoreVersion() As Version 'C0030
            Return System.Reflection.Assembly.GetExecutingAssembly.GetName.Version
        End Function

        'Public Function GetAHPDBVersion(ByVal ConnectionString As String) As Single 'C0069 'C0236
        Public Function GetAHPDBVersion(ByVal ConnectionString As String, ByVal ProviderType As DBProviderType) As Single 'C0236
            Dim res As Single
            'If Not DBFuncs.CheckDBConnection(ProviderType, ConnectionString) Then 'C0247
            If Not CheckDBConnection(ProviderType, ConnectionString) Then 'C0247
                Return AHP_DB_DEFAULT_VERSION
            End If

            'C0236===
            'Dim dbConnection As New odbc.odbcConnection(ConnectionString)
            'dbConnection.Open()
            'Dim dbReader As odbc.odbcDataReader
            'Dim oCommand As odbc.odbcCommand
            Using dbConnection As DbConnection = GetDBConnection(ProviderType, ConnectionString)    ' D2227
                dbConnection.Open()
                Dim dbReader As DbDataReader
                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection
                'C0236==

                Try
                    If TableExists(ConnectionString, ProviderType, "MProperties") Then 'AS/10-8-15 enclosed and added Else part
                        oCommand = New Odbc.OdbcCommand("SELECT * FROM MProperties WHERE PropertyName LIKE 'Version'", dbConnection)
                        dbReader = DBExecuteReader(ProviderType, oCommand)

                        Dim strVersion As String = ""

                        If Not dbReader Is Nothing Then
                            While dbReader.Read
                                strVersion = dbReader("PValue")
                            End While
                        End If

                        Dim v As Single
                        If Single.TryParse(MiscFuncs.FixStringWithSingleValue(strVersion), v) Then
                            res = v
                        Else
                            res = AHP_DB_DEFAULT_VERSION
                        End If
                        dbReader.Close()
                    Else 'AS/10-8-15===
                        oCommand = New Odbc.OdbcCommand("SELECT Version FROM Properties", dbConnection)
                        dbReader = DBExecuteReader(ProviderType, oCommand)

                        Dim strVersion As String = ""

                        If Not dbReader Is Nothing Then
                            While dbReader.Read
                                strVersion = dbReader("Version")
                            End While
                        End If

                        Dim v As Single
                        If Single.TryParse(MiscFuncs.FixStringWithSingleValue(strVersion), v) Then
                            res = v
                        Else
                            res = AHP_DB_DEFAULT_VERSION
                        End If
                        dbReader.Close()
                    End If 'AS/10-8-15==

                Catch ex As Exception
                    res = AHP_DB_DEFAULT_VERSION
                Finally
                    oCommand = Nothing
                    dbConnection.Close()
                End Try
            End Using
            Return res
        End Function


        Public Function GetAHPLastUploadToECC(ByVal ConnectionString As String, ByVal ProviderType As DBProviderType) As String 'AS/6-28-16 mimiced from Public Function GetAHPDBVersion
            Dim res As String = ""
            If Not CheckDBConnection(ProviderType, ConnectionString) Then
                Return "1" 'the model WAS opened by ECD
            End If

            Using dbConnection As DbConnection = GetDBConnection(ProviderType, ConnectionString)
                dbConnection.Open()
                Dim dbReader As DbDataReader
                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection

                Try
                    If TableExists(ConnectionString, ProviderType, "MProperties") Then
                        oCommand = New Odbc.OdbcCommand("SELECT * FROM MProperties WHERE PropertyName LIKE 'LastUploadedToECC'", dbConnection)
                        dbReader = DBExecuteReader(ProviderType, oCommand)

                        If Not dbReader Is Nothing Then
                            If dbReader.HasRows Then
                                While dbReader.Read
                                    res = dbReader("PValue")
                                End While
                            Else
                                res = ""
                            End If

                            dbReader.Close()
                        Else
                            res = "1"
                        End If
                    End If

                Catch ex As Exception
                    res = "1"
                Finally
                    oCommand = Nothing
                    dbConnection.Close()
                End Try
            End Using
            Return res

        End Function

        Public Function IsCombinedUserID(ByVal UserID As Integer) As Boolean
            Return (UserID = COMBINED_USER_ID) OrElse (UserID <= COMBINED_GROUPS_USERS_START_ID) AndAlso (UserID > Integer.MinValue)
        End Function

        Public Function StringToByteArray(ByVal str As String) As Byte() 'C0345
            Dim encoding As New System.Text.ASCIIEncoding()
            Return encoding.GetBytes(str)
        End Function

        Public Function ByteArrayToString(ByVal bytes As Byte()) As String 'C0345
            Dim str As String
            Dim enc As New System.Text.ASCIIEncoding()
            str = enc.GetString(bytes)
            Return str
        End Function

        <Serializable()> Public Class clsNodesByNameIncreasingComparer
            Implements IComparer(Of clsNode)

            Public Function Compare(ByVal A As clsNode, ByVal B As clsNode) As Integer Implements IComparer(Of ECCore.clsNode).Compare 'A0962
                Return If(A.NodeName.ToLower = B.NodeName.ToLower, 0, If(A.NodeName.ToLower < B.NodeName.ToLower, -1, 1))
            End Function
        End Class

        <Serializable()> Public Class clsNodesByNameDecreasingComparer
            Implements IComparer(Of clsNode)

            Public Function Compare(ByVal A As clsNode, ByVal B As clsNode) As Integer Implements IComparer(Of ECCore.clsNode).Compare 'A0962
                Return If(A.NodeName.ToLower = B.NodeName.ToLower, 0, If(A.NodeName.ToLower > B.NodeName.ToLower, -1, 1))
            End Function
        End Class

        <Serializable()> Public Class clsNodesByPriorityIncreasingComparer
            Implements IComparer(Of clsNode)

            Public Function Compare(ByVal A As clsNode, ByVal B As clsNode) As Integer Implements IComparer(Of ECCore.clsNode).Compare 'A0962
                Return If(A.LocalPriority(COMBINED_USER_ID) = B.LocalPriority(COMBINED_USER_ID), 0, If(A.LocalPriority(COMBINED_USER_ID) < B.LocalPriority(COMBINED_USER_ID), -1, 1))
            End Function
        End Class

        <Serializable()> Public Class clsNodesByPriorityDecreasingComparer
            Implements IComparer(Of clsNode)

            Public Function Compare(ByVal A As clsNode, ByVal B As clsNode) As Integer Implements IComparer(Of ECCore.clsNode).Compare 'A0962
                Return If(A.LocalPriority(COMBINED_USER_ID) = B.LocalPriority(COMBINED_USER_ID), 0, If(A.LocalPriority(COMBINED_USER_ID) > B.LocalPriority(COMBINED_USER_ID), -1, 1))
            End Function
        End Class

        <Serializable()> Public Class clsAlternativesByPriorityIncreasingComparer 'A0809
            Implements IComparer(Of clsNode)

            Public Function Compare(ByVal A As clsNode, ByVal B As clsNode) As Integer Implements IComparer(Of ECCore.clsNode).Compare 'A0962
                Return If(A.WRTGlobalPriority = B.WRTGlobalPriority, 0, If(A.WRTGlobalPriority < B.WRTGlobalPriority, -1, 1))
            End Function
        End Class

        <Serializable()> Public Class clsAlternativesByPriorityDecreasingComparer 'A0809
            Implements IComparer(Of clsNode)

            Public Function Compare(ByVal A As clsNode, ByVal B As clsNode) As Integer Implements IComparer(Of ECCore.clsNode).Compare 'A0962
                Return If(A.WRTGlobalPriority = B.WRTGlobalPriority, 0, If(A.WRTGlobalPriority > B.WRTGlobalPriority, -1, 1))
            End Function
        End Class

        <Serializable()> Public Class clsAlternativesByRiskIncreasingComparer 'A0828
            Implements IComparer(Of clsNode)

            Public Function Compare(ByVal A As clsNode, ByVal B As clsNode) As Integer Implements IComparer(Of ECCore.clsNode).Compare 'A0962
                Return If(A.RiskValue = B.RiskValue, 0, If(A.RiskValue < B.RiskValue, -1, 1))
            End Function
        End Class

        <Serializable()> Public Class clsAlternativesByRiskDecreasingComparer 'A0828
            Implements IComparer(Of clsNode)

            Public Function Compare(ByVal A As clsNode, ByVal B As clsNode) As Integer Implements IComparer(Of ECCore.clsNode).Compare 'A0962
                Return If(A.RiskValue = B.RiskValue, 0, If(A.RiskValue > B.RiskValue, -1, 1))
            End Function
        End Class

        <Serializable()> Public Class clsCombinedUserIDIncreasingComparer
            Implements IComparer(Of clsCombinedGroup)

            Public Function Compare(ByVal x As clsCombinedGroup, ByVal y As clsCombinedGroup) As Integer _
                 Implements IComparer(Of clsCombinedGroup).Compare
                Return If(x.CombinedUserID = y.CombinedUserID, 0, If(x.CombinedUserID < y.CombinedUserID, -1, 1))
            End Function
        End Class

        <Serializable()> Public Class clsCombinedUserIDDecreasingComparer
            Implements IComparer(Of clsCombinedGroup)

            Public Function Compare(ByVal x As clsCombinedGroup, ByVal y As clsCombinedGroup) As Integer _
                 Implements IComparer(Of clsCombinedGroup).Compare
                Return If(x.CombinedUserID = y.CombinedUserID, 0, If(x.CombinedUserID > y.CombinedUserID, -1, 1))
            End Function
        End Class

        'A0962 ===
        <Serializable()> Public Class clsNodesByIDIncreasingComparer
            Implements IComparer(Of clsNode)

            Public Function Compare(ByVal A As clsNode, ByVal B As clsNode) As Integer Implements IComparer(Of ECCore.clsNode).Compare
                Return A.NodeID.CompareTo(B.NodeID)
            End Function
        End Class

        <Serializable()> Public Class clsNodesByIDDecreasingComparer
            Implements IComparer(Of clsNode)

            Public Function Compare(ByVal A As clsNode, ByVal B As clsNode) As Integer Implements IComparer(Of ECCore.clsNode).Compare
                Return B.NodeID.CompareTo(A.NodeID)
            End Function            
        End Class

        <Serializable()> Public Class clsNodesByInfodocIncreasingComparer
            Implements IComparer(Of clsNode)

            Public Function Compare(ByVal A As clsNode, ByVal B As clsNode) As Integer Implements IComparer(Of ECCore.clsNode).Compare
                Return String.IsNullOrEmpty(A.InfoDoc).CompareTo(String.IsNullOrEmpty(B.InfoDoc))
            End Function
        End Class

        <Serializable()> Public Class clsNodesByInfodocDecreasingComparer
            Implements IComparer(Of clsNode)

            Public Function Compare(ByVal A As clsNode, ByVal B As clsNode) As Integer Implements IComparer(Of ECCore.clsNode).Compare
                Return String.IsNullOrEmpty(B.InfoDoc).CompareTo(String.IsNullOrEmpty(A.InfoDoc))
            End Function
        End Class
        'A0962 ==

        <Serializable()> _
        Public Enum FilterCombinations
            fcAnd = 0
            fcOr = 1
        End Enum

        <Serializable()> _
        Public Enum FilterOperations
            None = -1
            Contains = 0
            Equal = 1
            NotEqual = 2
            StartsWith = 3

            GreaterThan = 4
            GreaterThanOrEqual = 5
            LessThan = 6
            LessThanOrequal = 7

            IsTrue = 8
            IsFalse = 9
        End Enum

        <Serializable()>
        Public Enum EventType
            Risk = 0
            Opportunity = 1
        End Enum

        ' -D4078 // See ProjectType
        '<Serializable()>
        'Public Enum RiskionProjectType
        '    Risk = 0
        '    Opportunity = 1
        '    Mixed = 2
        'End Enum

        <Serializable()> _
        Public Class clsFilterItem

            Public IsChecked As Boolean = True
            Public SelectedAttributeID As Guid = Guid.Empty
            Public FilterText As String = ""
            Public FilterEnumItemID As Guid = Guid.Empty
            Public FilterEnumItemsIDs As List(Of Guid) = Nothing
            Public FilterCombination As FilterCombinations = FilterCombinations.fcAnd
            Public FilterOperation As FilterOperations = FilterOperations.None

        End Class


        Public Function IsPWMeasurementType(MT As ECMeasureType) As Boolean
            Return MT = ECMeasureType.mtPairwise Or MT = ECMeasureType.mtPWAnalogous
        End Function
    End Module
End Namespace