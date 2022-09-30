Option Strict On

Imports Canvas.RAGlobalSettings
Imports ECCore.Attributes
Imports ECCore.ECTypes
Imports System.Collections.Generic
Imports System.Linq

Namespace Canvas

    ' D4359 ===
    Public Module RAOption

        Public Const RA_OPT_DEF_PRECISION As Integer = 4  ' D3185
        Public Const RA_OPT_DEF_SORTING As raColumnID = raColumnID.ID   ' D3185
        'Public Const RA_OPT_PARETO_CURVE_DEF_COST_PRECISION As Integer = 2   ' A0941
        'Public Const RA_OPT_PARETO_CURVE_DEF_INCREMENTS As Integer = 26 ' A0941

        Public Const RA_OPT_CC_ALLOW_ENABLED_PROPERTY As Boolean = True     ' D3539
        Public Const RA_OPT_CC_EDIT_DISABLED As Boolean = True              ' D3539
        Public Const RA_OPT_CC_ALLOW_DOLLAR_VALUES As Boolean = True        ' D6464
        Public Const RA_OPT_FORCE_CC_USE_IN_TIMEPERIODS As Boolean = True   ' D3826
        Public Const RA_OPT_USE_SOLVER_PRIORITIES As Boolean = True             ' D4359
        Public Const RA_OPT_USE_FPOOLS_EXHAUSTED As Boolean = False             ' D4365
        Public Const RA_OPT_FPOOLS_ALLOW_ENABLED_PROPERTY As Boolean = True     ' D4367 + A1568 (FB 14214)
        Public Const RA_OPT_SOLVER_PRIORITIES_GUROBI_ONLY As Boolean = False    ' D4360 + D4361
        Public Const RA_OPT_SOLVER_PRIORITIES_DATA_VER_PREFIX As String = "Ver. "   ' D4364
        Public Const RA_OPT_SOLVER_PRIORITIES_DATA_VERSION As Integer = 1           ' D4364
        Public Const RA_OPT_ALLOW_LASTFUNDEDPERIOD_GROUPS_COLUMNS As Boolean = True ' D4808
        Public RA_OPT_ALLOW_COST_TOLERANCE_COLUMNS As Boolean = True            ' D4930


        Public Const RA_OPT_ALLOW_FREEZE_TABLES As Boolean = False          ' D3950 + D4802 + D6464 ' render lags

        Public Const RA_OPT_USE_SELECTED_ALTERNATIVES As Boolean = True     ' D3837

        Public RA_BIG_MODEL_ALTS_COUNT As Integer = 100 ' D3236 + D3246

        Public ReadOnly RA_FUNDED_COST_ID As Integer = -1       ' D4360
        Public RA_FUNDED_COST_NAME As String = "Funded Cost"    ' D4360

    End Module
    ' D4359 ==

    ' D3180 ===
    <Serializable()> Public Class RAGlobalSettings

        <Serializable()> Public Enum raOptionName
            raAutoSolve = 1 ' stream
            raScenarioComparisonSettings = 2    ' in-memory only
            raPrecision = 3 ' streams
            raSortBy = 4    ' streams
            raFrozenHeaders = 5     ' streams
            'raShowCustomConstraints = 6    ' attribs
            'raAllocationColumns = 7        ' attribs
            'raAllocationColumnsVersion = 8 ' attribs
            'raDependenciesView = 9         ' attribs
        End Enum

        Public Const CurrentUIColumnsVersion As Integer = 8     ' D3174 + D3781 + A1143 + D4348 + D4353 + D4930

        <Serializable()> Public Enum raColumnID
            GUID = 0
            ID = 1
            Name = 2
            isFunded = 3
            LastFundedPeriod = 4    ' D4803
            Benefit = 5
            EBenefit = 6
            Risk = 7            ' D4348 + D4353
            ProbSuccess = 8     ' D2843 + D4353
            ProbFailure = 9     ' D4348 + D4353
            Cost = 10
            isCostTolerance = 11    ' D4930
            CostTolerance = 12      ' D4930
            Groups = 13         ' D4803
            isPartial = 14
            MinPercent = 15
            Musts = 16
            MustNot = 17
            CustomConstraintsStart = MustNot + 1
        End Enum

        ' D3213 ===
        <Serializable()> Public Enum raScenarioField
            Index = 0
            ID = 1
            Name = 2
            Description = 3
            InfeasOptimalValue = 4  ' D6475
        End Enum
        ' D3213 ==

        Public Shared RA_BIG_MODEL_ALTS_COUNT As Integer = 200 ' D3236 + D3246 + D4522
        Private mOptionsList As Dictionary(Of raOptionName, Object) = Nothing
        Public ResourceAligner As ResourceAligner = Nothing

        ' D3236 ===
        Public Function isBigModel() As Boolean
            If RA_BIG_MODEL_ALTS_COUNT <= 10 Then RA_BIG_MODEL_ALTS_COUNT = 200
            If ResourceAligner IsNot Nothing AndAlso ResourceAligner.Scenarios.ActiveScenario.Alternatives.Count > RA_BIG_MODEL_ALTS_COUNT Then
                Return True
            Else
                If ResourceAligner IsNot Nothing AndAlso (ResourceAligner.Scenarios.ActiveScenario.Alternatives.Count * (1 + ResourceAligner.Scenarios.ActiveScenario.Constraints.Constraints.Count) * (1 + ResourceAligner.Scenarios.ActiveScenario.TimePeriods.Periods.Count)) > RA_BIG_MODEL_ALTS_COUNT * 20 Then Return True ' D4149 + D4522
            End If
            Return False
        End Function
        ' D3236 ==

        Public Property OptionsList As Dictionary(Of raOptionName, Object)
            Get
                Return mOptionsList
            End Get
            Set(value As Dictionary(Of raOptionName, Object))
                mOptionsList = value
            End Set
        End Property

        Public Property OptionValue(tOption As raOptionName) As Object
            Get
                If OptionsList IsNot Nothing AndAlso OptionsList.ContainsKey(tOption) Then Return OptionsList(tOption) Else Return Nothing
            End Get
            Set(value As Object)
                Dim fDoSave As Boolean = False  ' D3240
                If OptionsList Is Nothing Then OptionsList = New Dictionary(Of raOptionName, Object)
                If OptionsList.ContainsKey(tOption) Then
                    If OptionsList(tOption) IsNot value Then
                        OptionsList(tOption) = value
                        fDoSave = True  ' D3240
                    End If
                Else
                    OptionsList.Add(tOption, value)
                    fDoSave = True  ' D3240
                End If
                'If fDoSave AndAlso ResourceAligner IsNot Nothing AndAlso Not ResourceAligner.isLoading Then ResourceAligner.Save() ' D3240 -D4380
            End Set
        End Property

        Public ReadOnly Property OptionValue(tOption As raOptionName, tDefValue As Boolean) As Boolean
            Get
                Dim sVal As Object = OptionValue(tOption)
                If sVal Is Nothing OrElse Not TypeOf (sVal) Is Boolean Then
                    Return tDefValue
                Else
                    Return CBool(sVal)
                End If
            End Get
        End Property

        Public ReadOnly Property OptionValue(tOption As raOptionName, tDefValue As Integer) As Integer
            Get
                Dim sVal As Object = OptionValue(tOption)
                If sVal Is Nothing OrElse Not TypeOf (sVal) Is Integer Then
                    Return tDefValue
                Else
                    Return CInt(sVal)
                End If
            End Get
        End Property

        Public ReadOnly Property OptionValue(tOption As raOptionName, tDefValue As Double) As Double
            Get
                Dim sVal As Object = OptionValue(tOption)
                If sVal Is Nothing OrElse Not TypeOf (sVal) Is Integer Then
                    Return tDefValue
                Else
                    Return CDbl(sVal)
                End If
            End Get
        End Property

        Public ReadOnly Property OptionValue(tOption As raOptionName, tDefValue As String) As String
            Get
                Dim sVal As Object = OptionValue(tOption)
                If sVal Is Nothing OrElse Not TypeOf (sVal) Is String Then
                    Return tDefValue
                Else
                    Return CStr(sVal)
                End If
            End Get
        End Property

        Public Property isAutoSolve As Boolean
            Get
                ' D3236 ===
                Dim tAutoSolve As Boolean = OptionValue(raOptionName.raAutoSolve, Not isBigModel())
                'If tAutoSolve AndAlso isBigModel() Then isAutoSolve = False    ' -D3347
                If ResourceAligner.Solver.OPT_GUROBI_USE_CLOUD AndAlso ResourceAligner.Solver.SolverLibrary = raSolverLibrary.raGurobi Then tAutoSolve = False ' D3900
                Return tAutoSolve
                ' D3236 ==
            End Get
            Set(value As Boolean)
                'If value AndAlso isBigModel() Then value = False ' D3236 -D3347
                OptionValue(raOptionName.raAutoSolve) = value
            End Set
        End Property

        ' D3185 ===
        Public Property Precision As Integer
            Get
                Return OptionValue(raOptionName.raPrecision, RA_OPT_DEF_PRECISION)
            End Get
            Set(value As Integer)
                OptionValue(raOptionName.raPrecision) = value
            End Set
        End Property

        Public ReadOnly Property PrecisionFormat As String
            Get
                Return "F" + Precision.ToString
            End Get
        End Property

        Public Property SortBy As raColumnID
            Get
                Dim sVal As Object = OptionValue(raOptionName.raSortBy)
                If sVal Is Nothing Then
                    sVal = RA_OPT_DEF_SORTING
                End If
                Return CType(sVal, raColumnID)
            End Get
            Set(value As raColumnID)
                OptionValue(raOptionName.raSortBy) = value
            End Set
        End Property

        Public Property ShowFrozenHeaders As Boolean
            Get
                If RA_OPT_ALLOW_FREEZE_TABLES Then Return OptionValue(raOptionName.raFrozenHeaders, False) Else Return False ' D3950
            End Get
            Set(value As Boolean)
                If RA_OPT_ALLOW_FREEZE_TABLES Then OptionValue(raOptionName.raFrozenHeaders) = value ' D3950
            End Set
        End Property
        ' D3185 ==

        'A0941 ===
        'Public Property ParetoCurveSpecifiedIncrement As Double
        '    Get
        '        Dim sVal As Object = OptionValue(raOptionName.raParetoCurveSpecifiedIncrement)
        '        If sVal Is Nothing Then
        '            sVal = Double.MinValue
        '        End If
        '        Return CDbl(sVal)
        '    End Get
        '    Set(value As Double)
        '        OptionValue(raOptionName.raParetoCurveSpecifiedIncrement) = value
        '    End Set
        'End Property

        'Public Property ParetoCurveNumberOfIncrements As Integer
        '    Get
        '        Dim sVal As Object = OptionValue(raOptionName.raParetoCurveNumberOfIncrements)
        '        If sVal Is Nothing Then
        '            sVal = RA_OPT_PARETO_CURVE_DEF_INCREMENTS
        '        End If
        '        Return CInt(sVal)
        '    End Get
        '    Set(value As Integer)
        '        OptionValue(raOptionName.raParetoCurveNumberOfIncrements) = value
        '    End Set
        'End Property

        'Public Property ParetoCurveCostPrecision As Integer
        '    Get
        '        Dim sVal As Object = OptionValue(raOptionName.raParetoCurvePrecision)
        '        If sVal Is Nothing Then
        '            sVal = RA_OPT_PARETO_CURVE_DEF_COST_PRECISION
        '        End If
        '        Return CInt(sVal)
        '    End Get
        '    Set(value As Integer)
        '        OptionValue(raOptionName.raParetoCurvePrecision) = value
        '    End Set
        'End Property
        'A0941 ==

        Public Property ScenarioComparisonSettings As RASettings
            Get
                Dim sVal As Object = OptionValue(raOptionName.raScenarioComparisonSettings)
                If sVal Is Nothing OrElse Not (TypeOf (sVal) Is RASettings) Then
                    sVal = New RASettings() With {.Musts = True, .MustNots = True, .CustomConstraints = True, .Groups = True, .FundingPools = True, .Dependencies = True, .Risks = True, .UseBaseCase = True, .BaseCaseForGroups = True}
                    If ResourceAligner IsNot Nothing AndAlso Not ResourceAligner.isLoading Then OptionValue(raOptionName.raScenarioComparisonSettings) = sVal
                End If
                Return CType(sVal, RASettings)
            End Get
            Set(value As RASettings)
                OptionValue(raOptionName.raScenarioComparisonSettings) = value
            End Set
        End Property

        ' D3781 ===
        Public Property DependenciesView As Boolean
            Get
                Dim tVal As Object = ResourceAligner.ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_RA_DEPENDENCIES_VIEW_ID, Guid.Empty, Guid.Empty) ' D3788
                '-A1210 If tVal Is Nothing Then Return (ResourceAligner.Scenarios.ActiveScenario.Alternatives.Count > 50) Else Return CBool(tVal) ' D3788
                Return CBool(tVal) 'A12010
            End Get
            Set(value As Boolean)
                With ResourceAligner.ProjectManager
                    .Attributes.SetAttributeValue(ATTRIBUTE_RA_DEPENDENCIES_VIEW_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtBoolean, value, Guid.Empty, Guid.Empty)
                    .Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID, UNDEFINED_USER_ID)
                End With
            End Set
        End Property

        Public Property ShowCustomConstraints As Boolean
            Get
                Return CBool(ResourceAligner.ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_RA_SHOW_CC_ID, Guid.Empty, Guid.Empty))
            End Get
            Set(value As Boolean)
                With ResourceAligner.ProjectManager
                    .Attributes.SetAttributeValue(ATTRIBUTE_RA_SHOW_CC_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtBoolean, value, Guid.Empty, Guid.Empty)
                    .Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID, UNDEFINED_USER_ID)
                End With
            End Set
        End Property

        Public Property ColumnsVersion As Integer
            Get
                Return CInt(ResourceAligner.ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_RA_COLUMNS_VERSION_ID, Guid.Empty, Guid.Empty))
            End Get
            Set(value As Integer)
                With ResourceAligner.ProjectManager
                    .Attributes.SetAttributeValue(ATTRIBUTE_RA_COLUMNS_VERSION_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtLong, CLng(value), Guid.Empty, Guid.Empty)
                    .Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID, UNDEFINED_USER_ID)
                End With
            End Set
        End Property

        Public Property ColumnsList As Integer()
            Get
                Dim sList As String() = CStr(ResourceAligner.ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_RA_COLUMNS_LIST_ID, Guid.Empty, Guid.Empty)).Split(CChar(","))
                Dim tLst(sList.Length - 1) As Integer
                For i As Integer = 0 To tLst.Count - 1
                    Dim tID As Integer
                    If Integer.TryParse(sList(i), tID) Then tLst(i) = (tID)
                Next
                Return tLst
            End Get
            Set(value As Integer())
                With ResourceAligner.ProjectManager
                    Dim sList As String = ""
                    For Each tVal As Integer In value
                        sList += String.Format("{0}{1}", If(sList = "", "", ","), tVal.ToString)
                    Next
                    .Attributes.SetAttributeValue(ATTRIBUTE_RA_COLUMNS_LIST_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtString, sList, Guid.Empty, Guid.Empty)
                    .Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID, UNDEFINED_USER_ID)
                End With
            End Set
        End Property
        ' D3781 ==

        'A1143 ===
        Public ReadOnly Property IsIndexColumnVisible As Boolean
            Get
                Return ColumnsVersion <> CurrentUIColumnsVersion OrElse ColumnsList.Length < 2 OrElse ColumnsList.Contains(raColumnID.ID)
            End Get
        End Property
        'A1143 ==

        Public Sub New(ByRef Aligner As ResourceAligner)
            ResourceAligner = Aligner
            OptionsList = New Dictionary(Of raOptionName, Object)
        End Sub


    End Class
    ' D3180 ==

#Region "Comparers"

    ' D2982 ===
    Public Class RAAlternatives_Comparer
        Implements IComparer(Of RAAlternative)

        Private _SortField As raColumnID = RA_OPT_DEF_SORTING
        Private _IsDescending As Boolean = False    ' D3185
        Private RA As ResourceAligner = Nothing
        Private PM As ECCore.clsProjectManager = Nothing 'A1010
        Private Attributes As List(Of ECCore.clsAttribute) = New List(Of ECCore.clsAttribute)
        Private EnumValuesAll As Dictionary(Of Guid, String) = New Dictionary(Of Guid, String)
        Private v1 As Integer
        Private v2 As Boolean

        Public Sub New(ByVal tSortField As raColumnID, ByVal isDesc As Boolean, tRA As ResourceAligner)
            _SortField = tSortField
            _IsDescending = isDesc
            RA = tRA
            PM = RA.ProjectManager
            Attributes = PM.Attributes.GetAlternativesAttributes(True)
            For Each attr As ECCore.clsAttribute In Attributes
                If attr.ValueType = ECCore.AttributeValueTypes.avtEnumeration Or attr.ValueType = ECCore.AttributeValueTypes.avtEnumerationMulti Then
                    Dim tEnum As ECCore.clsAttributeEnumeration = PM.Attributes.GetEnumByID(attr.EnumID)
                    If tEnum IsNot Nothing Then
                        For Each item As ECCore.clsAttributeEnumerationItem In tEnum.Items
                            If Not EnumValuesAll.ContainsKey(item.ID) Then EnumValuesAll.Add(item.ID, item.Value)
                        Next
                    End If
                End If
            Next
        End Sub

        'Public Sub New(v1 As Integer, v2 As Boolean, rA As ResourceAligner)
        '    Me.v1 = v1
        '    Me.v2 = v2
        '    Me.rA = rA
        'End Sub

        Public Function Compare(ByVal A As RAAlternative, ByVal B As RAAlternative) As Integer Implements IComparer(Of RAAlternative).Compare
            Dim Res As Integer = 0
            Select Case _SortField
                Case raColumnID.ID
                    Res = CInt(If(A.SortOrder = B.SortOrder, 0, If(A.SortOrder < B.SortOrder, -1, 1)))  ' D3076
                Case raColumnID.Name
                    Res = String.Compare(A.Name, B.Name, True)
                Case raColumnID.isFunded
                    Res = CInt(If(A.Funded = B.Funded, 0, If(A.Funded < B.Funded, -1, 1)))
                    'Case RABasePage.COL_BENEFIT
                    '    If RA Is Nothing OrElse RA.Scenarios.ActiveScenario.Settings.Risks Then
                    '        Res = If(A.Benefit = B.Benefit, 0, If(A.Benefit < B.Benefit, -1, 1))
                    '    Else
                    '        Res = If(A.Benefit = B.BenefitOriginal, 0, If(A.BenefitOriginal < B.BenefitOriginal, -1, 1))
                    '    End If
                    ' D3174 ===
                Case raColumnID.EBenefit     ' D3175
                    Res = CInt(If(A.Benefit = B.Benefit, 0, If(A.Benefit < B.Benefit, -1, 1)))
                Case raColumnID.Benefit     ' D3175
                    Res = CInt(If(A.Benefit = B.BenefitOriginal, 0, If(A.BenefitOriginal < B.BenefitOriginal, -1, 1)))
                    ' D3174 ==
                Case raColumnID.ProbFailure
                    Res = CInt(If(A.RiskOriginal = B.RiskOriginal, 0, If(A.RiskOriginal < B.RiskOriginal, -1, 1)))
                    ' D4348 ===
                Case raColumnID.Risk
                    Res = CInt(If(A.Risk = B.Risk, 0, If(A.Risk < B.Risk, -1, 1)))
                Case raColumnID.ProbSuccess
                    Res = CInt(If(A.RiskOriginal = B.RiskOriginal, 0, If(A.RiskOriginal < B.RiskOriginal, 1, -1)))
                    ' D4348 ==
                Case raColumnID.Cost
                    Res = CInt(If(A.Cost = B.Cost, 0, If(A.Cost < B.Cost, -1, 1)))
                Case raColumnID.isPartial
                    Res = CInt(If(A.IsPartial = B.IsPartial, 0, If(A.IsPartial AndAlso Not B.IsPartial, 1, -1)))
                Case raColumnID.MinPercent
                    Dim AP As Double = A.MinPercent
                    If Not A.IsPartial Then AP = -1
                    Dim BP As Double = B.MinPercent
                    If Not B.IsPartial Then BP = -1
                    Res = CInt(If(AP = BP, 0, If(AP < BP, -1, 1)))
                Case raColumnID.Musts
                    Res = CInt(If(A.Must = B.Must, 0, If(A.Must AndAlso Not B.Must, 1, -1)))
                Case raColumnID.MustNot
                    Res = CInt(If(A.MustNot = B.MustNot, 0, If(A.MustNot AndAlso Not B.MustNot, 1, -1)))

                Case Else
                    If _SortField >= raColumnID.CustomConstraintsStart AndAlso _SortField < raColumnID.CustomConstraintsStart + RA.Scenarios.ActiveScenario.Constraints.Constraints.Count Then 'A1010
                        Dim idx As Integer = _SortField - CInt(raColumnID.CustomConstraintsStart)
                        With RA.Scenarios.ActiveScenario.Constraints
                            If idx < .Constraints.Keys.Count Then
                                Dim tID As Integer = .Constraints.Keys(idx)
                                If .Constraints.ContainsKey(tID) Then
                                    Dim tConstr As RAConstraint = .Constraints(tID)
                                    Dim AC As Double = .GetConstraintValue(tConstr.ID, A.ID)
                                    Dim BC As Double = .GetConstraintValue(tConstr.ID, B.ID)
                                    Res = CInt(If(AC = BC, 0, If(AC < BC, -1, 1)))
                                End If
                            End If
                        End With
                    End If

                    'A1010 ===
                    If _SortField >= raColumnID.CustomConstraintsStart + RA.Scenarios.ActiveScenario.Constraints.Constraints.Count Then
                        ' attributes columns
                        Dim index As Integer = _SortField - (raColumnID.CustomConstraintsStart + RA.Scenarios.ActiveScenario.Constraints.Constraints.Count)
                        If index < Attributes.Count Then
                            Dim tAttr As ECCore.clsAttribute = Attributes(index)
                            Dim AC As Object = PM.Attributes.GetAttributeValue(tAttr.ID, New Guid(A.ID))
                            Dim BC As Object = PM.Attributes.GetAttributeValue(tAttr.ID, New Guid(B.ID))
                            Dim sVal As String = ""
                            Select Case tAttr.ValueType
                                Case ECCore.AttributeValueTypes.avtLong, ECCore.AttributeValueTypes.avtDouble
                                    If AC IsNot Nothing AndAlso BC IsNot Nothing Then
                                        Dim tAC As Double = CDbl(AC)
                                        Dim tBC As Double = CDbl(BC)
                                        Res = CInt(If(tAC = tBC, 0, If(tAC < tBC, -1, 1)))
                                    End If
                                Case ECCore.AttributeValueTypes.avtEnumeration
                                    Dim tAC As Guid = Guid.Empty
                                    If AC IsNot Nothing Then tAC = CType(AC, Guid)
                                    Dim tBC As Guid = Guid.Empty
                                    If BC IsNot Nothing Then tBC = CType(BC, Guid)
                                    Dim sAC As String = ""
                                    If Not tAC.Equals(Guid.Empty) AndAlso EnumValuesAll.ContainsKey(tAC) Then sAC = EnumValuesAll(tAC)
                                    Dim sBC As String = ""
                                    If Not tBC.Equals(Guid.Empty) AndAlso EnumValuesAll.ContainsKey(tBC) Then sBC = EnumValuesAll(tBC)
                                    Res = sAC.CompareTo(sBC)
                                Case ECCore.AttributeValueTypes.avtEnumerationMulti
                                    'not implemented
                            End Select
                        End If
                    End If
                    'A1010 ==
            End Select

            If Res = 0 AndAlso _SortField <> raColumnID.ID Then   ' D3159 + D4804
                Res = If(A.SortOrder = B.SortOrder, 0, If(A.SortOrder < B.SortOrder, -1, 1))
            End If

            If _IsDescending AndAlso Res <> 0 Then Return -Res Else Return Res
        End Function

    End Class
    ' D2982 ==

    ' D3213 ===
    Public Class RAScenarios_Comparer
        Implements IComparer(Of RAScenario)

        Private _SortField As raScenarioField = raScenarioField.Index
        Private _IsDescending As Boolean = False

        Public Sub New(ByVal tSortField As raScenarioField, ByVal isDesc As Boolean)
            _SortField = tSortField
            _IsDescending = isDesc
        End Sub

        Public Function Compare(ByVal A As RAScenario, ByVal B As RAScenario) As Integer Implements IComparer(Of RAScenario).Compare
            Dim Res As Integer = 0
            Select Case _SortField
                Case raScenarioField.ID
                    Res = CInt(If(A.ID = B.ID, 0, If(A.ID < B.ID, -1, 1)))
                Case raScenarioField.Index
                    Res = CInt(If(A.Index = B.Index OrElse A.Index = 0 OrElse B.Index = 0, If(A.ID = B.ID, 0, If(A.ID < B.ID, -1, 1)), If(A.Index < B.Index, -1, 1)))   ' D3215
                Case raScenarioField.Index
                    Res = String.Compare(A.Name, B.Name, True)
                Case raScenarioField.Description
                    Res = String.Compare(A.Description, B.Description, True)
                Case raScenarioField.InfeasOptimalValue ' D6475
                    Res = CInt(If(A.InfeasibilityOptimalValue = B.InfeasibilityOptimalValue, 0, If(A.InfeasibilityOptimalValue < B.InfeasibilityOptimalValue, -1, 1)))  ' D6475
            End Select

            If _IsDescending And Res <> 0 Then Return -Res Else Return Res
        End Function

    End Class
    ' D3213 ==

    ' D4357 ===
    Public Class RASolverPriority_Comparer
        Implements IComparer(Of RASolverPriority)

        'Private _SortByPrty As Boolean = False

        'Public Sub New(ByVal SortByPrty As Boolean)
        '    _SortByPrty = SortByPrty
        'End Sub

        Public Function Compare(ByVal A As RASolverPriority, ByVal B As RASolverPriority) As Integer Implements IComparer(Of RASolverPriority).Compare
            'Dim Res As Integer = 0

            'If _SortByPrty OrElse A.Priority = B.Priority Then
            '    Res = CInt(If(A.Rank = B.Rank, 0, If(A.Rank < B.Rank, -1, 1)))
            'Else
            '    Res = CInt(If(A.Priority > B.Priority, -1, 1))
            'End If

            ''If A.ConstraintGUID.Equals(RA_FUNDED_COST_GUID) OrElse B.ConstraintGUID.Equals(RA_FUNDED_COST_GUID) Then Res = -1

            'Return Res
            Return CInt(If(A.Rank = B.Rank, 0, If(A.Rank < B.Rank, -1, 1)))
        End Function

    End Class
    ' D4357 ==

#End Region

End Namespace
