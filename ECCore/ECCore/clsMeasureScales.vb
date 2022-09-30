Imports System.Linq

Namespace ECCore
    <Serializable()> Public Class clsMeasureScales
        Public Property RatingsScales() As New List(Of clsRatingScale)

        Public Property RegularUtilityCurves() As New List(Of clsRegularUtilityCurve)

        Public Property AdvancedUtilityCurves() As New List(Of clsAdvancedUtilityCurve)

        Public Property StepFunctions() As New List(Of clsStepFunction)

        Public ReadOnly Property ProjectManager As clsProjectManager

        Public Sub New(ProjectManager As clsProjectManager)
            Me.ProjectManager = ProjectManager
        End Sub

        Private ReadOnly Property Attributes As clsAttributes
            Get
                Return ProjectManager.Attributes
            End Get
        End Property

        Private ReadOnly Property StorageManager As clsStorageManager
            Get
                Return ProjectManager.StorageManager
            End Get
        End Property

        Public ReadOnly Property AllScales() As List(Of clsMeasurementScale)
            Get
                'Return CType(RatingsScales.Concat(CType(RegularUtilityCurves, IEnumerable(Of clsMeasurementScale))).Concat(CType(StepFunctions, IEnumerable(Of clsMeasurementScale))).Concat(CType(AddAdvancedUtilityCurve(), IEnumerable(Of clsMeasurementScale))), IEnumerable(Of clsMeasurementScale)).ToList
                Dim res As New List(Of clsMeasurementScale)
                For Each MS As clsMeasurementScale In RatingsScales
                    res.Add(MS)
                Next
                For Each MS As clsMeasurementScale In StepFunctions
                    res.Add(MS)
                Next
                For Each MS As clsMeasurementScale In RegularUtilityCurves
                    res.Add(MS)
                Next
                For Each MS As clsMeasurementScale In AdvancedUtilityCurves
                    res.Add(MS)
                Next
                Return res
            End Get
        End Property

        Private Function GetScaleByID(Scales As IEnumerable(Of clsMeasurementScale), ByVal ID As Integer) As clsMeasurementScale
            Return Scales.FirstOrDefault(Function(s) (s.ID = ID))
        End Function

        Private Function GetScaleByID(Scales As IEnumerable(Of clsMeasurementScale), ByVal GuidID As Guid) As clsMeasurementScale
            Return Scales.FirstOrDefault(Function(s) (s.GuidID.Equals(GuidID)))
        End Function

        Private Function GetScaleByName(Scales As IEnumerable(Of clsMeasurementScale), ByVal Name As String) As clsMeasurementScale
            Return Scales.FirstOrDefault(Function(s) (s.Name.ToLower = Name.ToLower))
        End Function
        Private Function GetDefaultScale(Scales As IEnumerable(Of clsMeasurementScale), Optional Type As ScaleType = ScaleType.stShared) As clsMeasurementScale
            Return Scales.FirstOrDefault(Function(s) (s.IsDefault AndAlso If(Type = ScaleType.stShared, s.Type = Type, (Type = ScaleType.stLikelihood AndAlso s.Type = ScaleType.stLikelihood) OrElse (Type = ScaleType.stImpact AndAlso s.Type = ScaleType.stImpact)))) 'A1729
        End Function

        Public Function GetRatingScaleByID(ByVal ID As Integer) As clsRatingScale
            Return CType(GetScaleByID(RatingsScales, ID), clsRatingScale)
        End Function

        Public Function GetRatingScaleByID(ByVal GuidID As Guid) As clsRatingScale
            Return CType(GetScaleByID(RatingsScales, GuidID), clsRatingScale)
        End Function

        Public Function GetRatingScaleByName(ByVal Name As String) As clsRatingScale
            Return CType(GetScaleByName(RatingsScales, Name), clsRatingScale)
        End Function

        Public Function GetDefaultRatingScale(Type As ScaleType) As clsRatingScale
            Return CType(GetDefaultScale(RatingsScales, Type), clsRatingScale) 'A1729
        End Function

        Public Function GetDefaultStepFunction(Optional Type As ScaleType = ScaleType.stShared) As clsStepFunction
            Return CType(GetDefaultScale(StepFunctions, Type), clsStepFunction) 'A1729
        End Function

        Public Function GetDefaultRegularUtilityCurve(Optional Type As ScaleType = ScaleType.stShared) As clsRegularUtilityCurve
            Return CType(GetDefaultScale(RegularUtilityCurves, Type), clsRegularUtilityCurve) 'A1729
        End Function

        Public Function GetRegularUtilityCurveByID(ByVal ID As Integer) As clsRegularUtilityCurve
            Return CType(GetScaleByID(RegularUtilityCurves, ID), clsRegularUtilityCurve)
        End Function

        Public Function GetRegularUtilityCurveByID(ByVal GuidID As Guid) As clsRegularUtilityCurve
            Return CType(GetScaleByID(RegularUtilityCurves, GuidID), clsRegularUtilityCurve)
        End Function

        Public Function GetRegularUtilityCurveByName(ByVal Name As String) As clsRegularUtilityCurve
            Return CType(GetScaleByName(RegularUtilityCurves, Name), clsRegularUtilityCurve)
        End Function

        'Public Function GetDefaultRegularUtilityCurve(Optional Type As ScaleType = ScaleType.stShared) As clsRegularUtilityCurve
        '    For Each RUC As clsRegularUtilityCurve In mRegularUtilityCurves
        '        If RUC.IsDefault And Type = Type Then
        '            Return RUC
        '        End If
        '    Next
        '    Return Nothing
        'End Function


        Public Function GetAdvancedUtilityCurveByID(ByVal GuidID As Guid) As clsAdvancedUtilityCurve
            Return CType(GetScaleByID(AdvancedUtilityCurves, GuidID), clsAdvancedUtilityCurve)
        End Function

        Public Function GetAdvancedUtilityCurveByName(ByVal Name As String) As clsAdvancedUtilityCurve
            Return CType(GetScaleByID(AdvancedUtilityCurves, Name), clsAdvancedUtilityCurve)
        End Function

        Public Function GetStepFunctionByID(ByVal ID As Integer) As clsStepFunction
            Return CType(GetScaleByID(StepFunctions, ID), clsStepFunction)
        End Function

        Public Function GetStepFunctionByID(ByVal GuidID As Guid) As clsStepFunction
            Return CType(GetScaleByID(StepFunctions, GuidID), clsStepFunction)
        End Function

        Public Function GetStepFunctionByName(ByVal Name As String) As clsStepFunction
            Return CType(GetScaleByID(StepFunctions, Name), clsStepFunction)
        End Function

        Private Function GetNextScaleID(Scales As IEnumerable(Of clsMeasurementScale)) As Integer
            Return Scales.Select(Function(s) (s.ID)).DefaultIfEmpty(-1).Max + 1
        End Function

        Private Function GetNextRatingScaleID() As Integer
            Return GetNextScaleID(RatingsScales)
        End Function

        Public Function AddRatingScale() As clsRatingScale
            Dim id As Integer = GetNextRatingScaleID()
            Dim res As New clsRatingScale(id)
            res.Name = "New Rating Scale " + id.ToString
            RatingsScales.Add(res)
            Return res
        End Function

        Private Function GetNextRegularUtilityCurveID() As Integer
            Return GetNextScaleID(RegularUtilityCurves)
        End Function

        Public Function AddRegularUtilityCurve() As clsRegularUtilityCurve
            Dim id As Integer = GetNextRegularUtilityCurveID()
            Dim res As New clsRegularUtilityCurve(id, 0, 1, 0)
            res.Name = "New Utility Curve " + id.ToString
            RegularUtilityCurves.Add(res)
            Return res
        End Function

        Public Function AddAdvancedUtilityCurve() As clsAdvancedUtilityCurve
            Dim newID As Integer = GetNextScaleID(AdvancedUtilityCurves)
            Dim res As New clsAdvancedUtilityCurve(newID)
            res.Name = "New Utility Curve " + newID.ToString
            AdvancedUtilityCurves.Add(res)
            Return res
        End Function

        Private Function GetNextStepFunctionID() As Integer
            Return GetNextScaleID(StepFunctions)
        End Function

        Public Function AddStepFunction() As clsStepFunction
            Dim id As Integer = GetNextStepFunctionID()
            Dim res As New clsStepFunction(id)
            res.Name = "New Step Function " + id.ToString
            StepFunctions.Add(res)
            Return res
        End Function

        Private Function InitRatingScale(ScaleName As String, ScaleDescription As String, DefNames() As String, DefComments() As String, DefValue() As Single, Optional Type As ScaleType = ScaleType.stShared) As clsRatingScale
            Dim tRatingScale As clsRatingScale = AddRatingScale()
            tRatingScale.Name = ScaleName
            tRatingScale.Type = Type
            If ScaleDescription IsNot Nothing Then tRatingScale.Comment = ScaleDescription

            Dim rating As clsRating
            For i As Integer = 0 To DefNames.Length - 1 Step 1
                rating = tRatingScale.AddIntensity()
                rating.Name = DefNames(i)
                rating.Value = DefValue(i)
                If DefComments IsNot Nothing AndAlso DefComments.Length > i Then rating.Comment = If(DefComments(i), "")
            Next
            Return tRatingScale
        End Function

        Public Sub AddDefaultRatingScaleForImpact()
            Dim rs As clsRatingScale = InitRatingScale(DEFAULT_RATING_SCALE_NAME_FOR_IMPACT, DEFAULT_RATING_SCALE_DESC_FOR_IMPACT, DEFAULT_RATING_SCALE_IMPACT_NAMES, DEFAULT_RATING_SCALE_IMPACT_COMMENTS, DEFAULT_RATING_SCALE_IMPACT_VALUES, ScaleType.stImpact)    ' D2394
            rs.IsDefault = True
        End Sub

        Public Sub AddDefaultRatingScaleForLikelihood()
            Dim rs As clsRatingScale = InitRatingScale(DEFAULT_RATING_SCALE_NAME_FOR_LIKELIHOOD, DEFAULT_RATING_SCALE_DESC_FOR_LIKELIHOOD, DEFAULT_RATING_SCALE_LIKELIHOOD_NAMES, DEFAULT_RATING_SCALE_LIKELIHOOD_COMMENTS, DEFAULT_RATING_SCALE_LIKELIHHOD_VALUES, ScaleType.stLikelihood)
            With rs  ' D2394
                .IsOutcomes = False
                .IsPWofPercentages = False
                .IsDefault = True
            End With
        End Sub

        Public Sub AddDefaultVulnerabilityRatingScale()
            InitRatingScale(DEFAULT_RATING_SCALE_NAME_VULNERABILITY, DEFAULT_RATING_SCALE_DESC_VULNERABILITY, DEFAULT_RATING_SCALE_VULNERABILTIES_NAMES, DEFAULT_RATING_SCALE_VULNERABILTIES_COMMENTS, DEFAULT_RATING_SCALE_VULNERABILTIES_VALUES, ScaleType.stVulnerability)
        End Sub

        Public Sub AddDefaultControlsRatingScale()
            Dim rs As clsRatingScale = InitRatingScale(DEFAULT_RATING_SCALE_NAME_CONTROLS, DEFAULT_RATING_SCALE_DESC_CONTROLS, DEFAULT_RATING_SCALE_CONTROLS_NAMES, DEFAULT_RATING_SCALE_CONTROLS_COMMENTS, DEFAULT_RATING_SCALE_CONTROLS_VALUES, ScaleType.stControls)
            rs.IsDefault = True
        End Sub

        Public Sub AddDefaultRatingScale()
            Dim rs As clsRatingScale = InitRatingScale(DEFAULT_RATING_SCALE_NAME, DEFAULT_RATING_SCALE_DESC, DEFAULT_RATING_SCALE_NAMES, DEFAULT_RATING_SCALE_COMMENTS, DEFAULT_RATING_SCALE_VALUES, ScaleType.stShared)
            rs.IsDefault = True
        End Sub

        Public Sub AddDefaultOutcomesScale()
            Dim rs As clsRatingScale = InitRatingScale(DEFAULT_OUTCOMES_SCALE_NAME, DEFAULT_OUTCOMES_SCALE_DESC, DEFAULT_OUTCOMES_NAMES, DEFAULT_OUTCOMES_COMMENTS, DEFAULT_OUTCOMES_VALUES)
            With rs   ' D2394
                .IsOutcomes = True
                .IsPWofPercentages = False
                .IsExpectedValues = False 'A0780
                .IsDefault = True
            End With
        End Sub

        Public Sub AddDefaultPWofPercentagesScale()
            Dim rs As clsRatingScale = InitRatingScale(DEFAULT_PWOP_SCALE_NAME, DEFAULT_PWOP_SCALE_DESC, DEFAULT_PWOP_NAMES, DEFAULT_PWOP_COMMENTS, DEFAULT_PWOP_VALUES)
            With rs ' D2394
                .IsOutcomes = False
                .IsPWofPercentages = True
                .IsExpectedValues = False
                .IsDefault = True
            End With
        End Sub

        Public Sub AddDefaultExpectedValuesScale()
            Dim rs As clsRatingScale = InitRatingScale(DEFAULT_EXPECTED_VALUES_SCALE_NAME, DEFAULT_EXPECTED_VALUES_SCALE_DESC, DEFAULT_EXPECTED_VALUES_NAMES, DEFAULT_EXPECTED_VALUES_COMMENTS, DEFAULT_EXPECTED_VALUES_VALUES)
            With rs   ' D2394
                .IsOutcomes = False
                .IsPWofPercentages = False
                .IsExpectedValues = True
                .IsDefault = True
            End With
        End Sub

        Public Sub AddDefaultRegularUtilityCurve()
            Dim DefaultRUC As clsRegularUtilityCurve = AddRegularUtilityCurve()
            DefaultRUC.Name = DEFAULT_REGULAR_UTILITY_CURVE_NAME
            DefaultRUC.Low = 0
            DefaultRUC.High = 1
            DefaultRUC.IsIncreasing = True
            DefaultRUC.IsLinear = True
            DefaultRUC.Curvature = 0
            DefaultRUC.Type = ScaleType.stShared
            DefaultRUC.IsDefault = True
        End Sub

        Public Sub AddDefaultRegularUtilityCurveForLikelihood()
            Dim DefaultRUC As clsRegularUtilityCurve = AddRegularUtilityCurve()
            DefaultRUC.Name = DEFAULT_REGULAR_UTILITY_CURVE_FOR_LIKELIHOOD_NAME
            DefaultRUC.Low = 0
            DefaultRUC.High = 1
            DefaultRUC.IsIncreasing = True
            DefaultRUC.IsLinear = True
            DefaultRUC.Curvature = 0
            DefaultRUC.Type = ScaleType.stLikelihood
            DefaultRUC.IsDefault = True
        End Sub

        Public Sub AddDefaultRegularUtilityCurveForImpact()
            Dim DefaultRUC As clsRegularUtilityCurve = AddRegularUtilityCurve()
            DefaultRUC.Name = DEFAULT_REGULAR_UTILITY_CURVE_FOR_IMPACT_NAME
            DefaultRUC.Low = 0
            DefaultRUC.High = 1
            DefaultRUC.IsIncreasing = True
            DefaultRUC.IsLinear = True
            DefaultRUC.Curvature = 0
            DefaultRUC.Type = ScaleType.stImpact
            DefaultRUC.IsDefault = True
        End Sub

        Public Sub AddDefaultRegularUtilityCurveForControls()
            Dim DefaultRUC As clsRegularUtilityCurve = AddRegularUtilityCurve()
            DefaultRUC.Name = DEFAULT_REGULAR_UTILITY_CURVE_FOR_CONTROLS_NAME
            DefaultRUC.Low = 0
            DefaultRUC.High = 1
            DefaultRUC.IsIncreasing = True
            DefaultRUC.IsLinear = True
            DefaultRUC.Curvature = 0
            DefaultRUC.Type = ScaleType.stControls
            DefaultRUC.IsDefault = True
        End Sub

        Public Sub AddDefaultRegularUtilityCurveForVulnerability()
            Dim DefaultRUC As clsRegularUtilityCurve = AddRegularUtilityCurve()
            DefaultRUC.Name = DEFAULT_REGULAR_UTILITY_CURVE_FOR_VULNERABILITY_NAME
            DefaultRUC.Low = 0
            DefaultRUC.High = 1
            DefaultRUC.IsIncreasing = True
            DefaultRUC.IsLinear = True
            DefaultRUC.Curvature = 0
            DefaultRUC.Type = ScaleType.stVulnerability
        End Sub

        Public Sub AddDefaultAdvancedUtilityCurve() 'C0706
            Dim DefaultAUC As clsAdvancedUtilityCurve = AddAdvancedUtilityCurve()
            DefaultAUC.Name = DEFAULT_ADVANCED_UTILITY_CURVE_NAME
            DefaultAUC.Low = 0
            DefaultAUC.High = 1
            DefaultAUC.AddUCPoint(0.5, 0.5)
            DefaultAUC.IsDefault = True
        End Sub

        Public Sub AddDefaultStepFunction()
            Dim DefaultSF As clsStepFunction = AddStepFunction()
            DefaultSF.Name = DEFAULT_STEP_FUNCTION_NAME
            Dim SI As clsStepInterval = DefaultSF.AddInterval
            SI.Low = 0
            SI.High = 1
            SI.Value = 1
            DefaultSF.IsDefault = True
        End Sub

        Public Sub AddDefaultStepFunctionForLikelihood()
            Dim DefaultSF As clsStepFunction = AddStepFunction()
            DefaultSF.Name = DEFAULT_STEP_FUNCTION_FOR_LIKELIHOOD_NAME
            DefaultSF.Type = ScaleType.stLikelihood
            Dim SI As clsStepInterval = DefaultSF.AddInterval
            SI.Low = 0
            SI.High = 1
            SI.Value = 1
            DefaultSF.IsDefault = True
        End Sub

        Public Sub AddDefaultStepFunctionForImpact()
            Dim DefaultSF As clsStepFunction = AddStepFunction()
            DefaultSF.Name = DEFAULT_STEP_FUNCTION_FOR_IMPACT_NAME
            DefaultSF.Type = ScaleType.stImpact
            Dim SI As clsStepInterval = DefaultSF.AddInterval
            SI.Low = 0
            SI.High = 1
            SI.Value = 1
            DefaultSF.IsDefault = True
        End Sub

        Public Sub AddDefaultStepFunctionForControls()
            Dim DefaultSF As clsStepFunction = AddStepFunction()
            DefaultSF.Name = DEFAULT_STEP_FUNCTION_FOR_CONTROLS_NAME
            DefaultSF.Type = ScaleType.stControls
            Dim SI As clsStepInterval = DefaultSF.AddInterval
            SI.Low = 0
            SI.High = 1
            SI.Value = 1
            DefaultSF.IsDefault = True
        End Sub

        Public Sub AddDefaultStepFunctionForVulnerabilities()
            Dim DefaultSF As clsStepFunction = AddStepFunction()
            DefaultSF.Name = DEFAULT_STEP_FUNCTION_FOR_VULNERABILITY_NAME
            DefaultSF.Type = ScaleType.stVulnerability
            Dim SI As clsStepInterval = DefaultSF.AddInterval
            SI.Low = 0
            SI.High = 1
            SI.Value = 1
        End Sub
        Public Sub SetScaleDefault(ScaleID As Guid, IsDeafult As Boolean)
            If AllScales.Count > 0 Then
                For Each Scale As Object In AllScales
                    If CType(Scale, ECCore.clsMeasurementScale).GuidID.Equals(ScaleID) Then
                        If TypeOf (Scale) Is ECCore.clsRatingScale Then
                            Dim defScale = CType(Scale, ECCore.clsRatingScale)
                            If IsDeafult Then
                                For Each rs As ECCore.clsRatingScale In RatingsScales
                                    If (rs.Type = ScaleType.stShared OrElse rs.Type = defScale.Type) AndAlso rs.IsExpectedValues = defScale.IsExpectedValues AndAlso rs.IsOutcomes = defScale.IsOutcomes AndAlso rs.IsPWofPercentages = defScale.IsPWofPercentages Then
                                        rs.IsDefault = False
                                    End If
                                Next
                            End If
                            defScale.IsDefault = IsDeafult
                        End If
                        If TypeOf (Scale) Is ECCore.clsStepFunction Then
                            Dim defScale = CType(Scale, ECCore.clsStepFunction)
                            If IsDeafult Then
                                For Each sf As ECCore.clsStepFunction In StepFunctions
                                    If (sf.Type = ScaleType.stShared OrElse sf.Type = defScale.Type) Then sf.IsDefault = False
                                Next
                            End If
                            defScale.IsDefault = IsDeafult
                        End If
                        If TypeOf (Scale) Is ECCore.clsRegularUtilityCurve Then
                            Dim defScale = CType(Scale, ECCore.clsRegularUtilityCurve)
                            If IsDeafult Then
                                For Each uc As ECCore.clsRegularUtilityCurve In RegularUtilityCurves
                                    If (uc.Type = ScaleType.stShared OrElse uc.Type = defScale.Type) Then uc.IsDefault = False
                                Next
                            End If
                            defScale.IsDefault = IsDeafult
                        End If
                        If TypeOf (Scale) Is ECCore.clsAdvancedUtilityCurve Then
                            Dim defScale = CType(Scale, ECCore.clsAdvancedUtilityCurve)
                            If IsDeafult Then
                                For Each uc As ECCore.clsAdvancedUtilityCurve In AdvancedUtilityCurves
                                    If (uc.Type = ScaleType.stShared OrElse uc.Type = defScale.Type) Then uc.IsDefault = False
                                Next
                            End If
                            defScale.IsDefault = IsDeafult
                        End If
                    End If
                Next
            End If
        End Sub

        Public Function GetScaleByID(tID As Guid) As clsMeasurementScale
            Return AllScales.FirstOrDefault(Function(s) (s.GuidID.Equals(tID)))
        End Function

        Public Sub DeleteScaleByID(ScaleID As Guid)
            Dim ms As clsMeasurementScale = GetScaleByID(ScaleID)
            If TypeOf ms Is clsRatingScale Then
                Dim rs As clsRatingScale = CType(ms, clsRatingScale)
                Dim rs_count As Integer = 0
                For Each s As clsRatingScale In RatingsScales
                    If s.IsExpectedValues = rs.IsExpectedValues AndAlso s.IsOutcomes = rs.IsOutcomes AndAlso s.IsPWofPercentages = rs.IsPWofPercentages Then rs_count += 1
                Next
                If rs_count > 1 Then
                    RatingsScales.RemoveAll(Function(s) (s.GuidID.Equals(ScaleID)))
                End If
            End If

            If RegularUtilityCurves.Count > 1 Then RegularUtilityCurves.RemoveAll(Function(s) (s.GuidID.Equals(ScaleID))) 'A1591
            If AdvancedUtilityCurves.Count > 1 Then AdvancedUtilityCurves.RemoveAll(Function(s) (s.GuidID.Equals(ScaleID))) 'A1591
            If StepFunctions.Count > 1 Then StepFunctions.RemoveAll(Function(s) (s.GuidID.Equals(ScaleID))) 'A1591
        End Sub

        Public Function GetScaleType(Scale As clsMeasurementScale) As ScaleType
            Return Scale.Type
        End Function

        Public Function GetRatingScaleType(scale As clsMeasurementScale) As RatingScaleType
            Dim retVal As Integer = RatingScaleType.rsRegular
            If TypeOf (scale) Is clsRatingScale Then
                Dim rs As clsRatingScale = CType(scale, clsRatingScale)
                If rs.IsExpectedValues Then retVal = RatingScaleType.rsExpectedValues
                If rs.IsOutcomes Then retVal = RatingScaleType.rsOutcomes
                If rs.IsPWofPercentages Then retVal = RatingScaleType.rsPWOfPercentages
            End If
            Return retVal
        End Function

        Public Sub ClearAllScales()
            RatingsScales.Clear()
            StepFunctions.Clear()
            RegularUtilityCurves.Clear()
            AdvancedUtilityCurves.Clear()
        End Sub

        Public Sub MoveScale(ScaleID As Guid, isMoveUp As Boolean)
            Dim scale As clsMeasurementScale = GetScaleByID(ScaleID)
            If scale IsNot Nothing Then
                Dim scale_type As ScaleType = GetScaleType(scale)
                Dim RST As RatingScaleType = GetRatingScaleType(scale)
                ' find prev scale
                Dim prevScale As clsMeasurementScale = Nothing
                Dim idx0 As Integer = 0
                Dim idxN As Integer = AllScales.Count - 1
                Dim st As Integer = 1
                If Not isMoveUp Then
                    idx0 = idxN
                    idxN = 0
                    st = -1
                End If
                For i As Integer = idx0 To idxN Step st
                    Dim scale2 As clsMeasurementScale = CType(AllScales(i), clsMeasurementScale)
                    If scale2.GuidID = ScaleID Then Exit For
                    Dim scale2type As ScaleType = GetScaleType(scale2)
                    Dim RST2 As RatingScaleType = GetRatingScaleType(scale2)
                    If scale.GetType() Is scale2.GetType() AndAlso RST = RST2 AndAlso (scale2type = ScaleType.stShared OrElse scale2type = scale_type) Then
                        prevScale = scale2
                    End If
                Next
                If prevScale IsNot Nothing Then
                    If TypeOf (scale) Is ECCore.clsRatingScale Then
                        Dim i0 As Integer = RatingsScales.IndexOf(prevScale)
                        Dim i1 As Integer = RatingsScales.IndexOf(scale)
                        Dim tmp As Object = RatingsScales(i0)
                        RatingsScales(i0) = RatingsScales(i1)
                        RatingsScales(i1) = tmp
                    End If
                    If TypeOf (scale) Is ECCore.clsStepFunction Then
                        Dim i0 As Integer = StepFunctions.IndexOf(prevScale)
                        Dim i1 As Integer = StepFunctions.IndexOf(scale)
                        Dim tmp As Object = StepFunctions(i0)
                        StepFunctions(i0) = StepFunctions(i1)
                        StepFunctions(i1) = tmp
                    End If
                    If TypeOf (scale) Is ECCore.clsRegularUtilityCurve Then
                        Dim i0 As Integer = RegularUtilityCurves.IndexOf(prevScale)
                        Dim i1 As Integer = RegularUtilityCurves.IndexOf(scale)
                        Dim tmp As Object = RegularUtilityCurves(i0)
                        RegularUtilityCurves(i0) = RegularUtilityCurves(i1)
                        RegularUtilityCurves(i1) = tmp
                    End If
                    If TypeOf (scale) Is ECCore.clsAdvancedUtilityCurve Then
                        Dim i0 As Integer = AdvancedUtilityCurves.IndexOf(prevScale)
                        Dim i1 As Integer = AdvancedUtilityCurves.IndexOf(scale)
                        Dim tmp As Object = AdvancedUtilityCurves(i0)
                        AdvancedUtilityCurves(i0) = AdvancedUtilityCurves(i1)
                        AdvancedUtilityCurves(i1) = tmp
                    End If
                End If
            End If
        End Sub
        'A1047 ==

        Public Function CloneRatingScale(RatingScale As clsRatingScale, Optional CopyJudgments As Boolean = False) As clsRatingScale
            If RatingScale Is Nothing Then Return Nothing

            Dim newRS As clsRatingScale = RatingScale.Clone
            newRS.ID = GetNextRatingScaleID()
            newRS.IsDefault = False
            RatingsScales.Add(newRS)
            Return newRS
        End Function

        Public Function CloneStepFunction(StepFunction As clsStepFunction, Optional CopyJudgments As Boolean = False) As clsStepFunction
            If StepFunction Is Nothing Then Return Nothing

            Dim newSF As clsStepFunction = StepFunction.Clone
            newSF.ID = GetNextStepFunctionID()
            newSF.IsDefault = False
            StepFunctions.Add(newSF)
            Return newSF
        End Function

        Public Function CloneRegularUtilityCurve(UtilityCurve As clsRegularUtilityCurve) As clsRegularUtilityCurve
            If UtilityCurve Is Nothing Then Return Nothing

            Dim newRUC As clsRegularUtilityCurve = UtilityCurve.Clone
            newRUC.ID = GetNextRegularUtilityCurveID()
            newRUC.IsDefault = False
            RegularUtilityCurves.Add(newRUC)
            Return newRUC
        End Function

        Public Sub FixScalesIsDefaultProperty()
            Dim defaultScale As clsMeasurementScale = Nothing
            Dim defaultScaleExists As Boolean = False
            Dim defaultScaleExistsForPWOutcomes As Boolean = False
            Dim defaultScaleExistsForPWPercentages As Boolean = False

            Dim scales As List(Of clsRatingScale)
            Dim defScale As clsRatingScale

            scales = RatingsScales.Where(Function(x) x.Type = ScaleType.stShared AndAlso Not x.IsExpectedValues AndAlso Not x.IsOutcomes AndAlso Not x.IsPWofPercentages).ToList
            defScale = scales.FirstOrDefault(Function(x) x.IsDefault)
            If Not ProjectManager.IsRiskProject AndAlso defScale Is Nothing AndAlso scales.Count > 0 Then
                scales(0).IsDefault = True
            End If

            scales = RatingsScales.Where(Function(x) x.Type = ScaleType.stLikelihood).ToList
            defScale = scales.FirstOrDefault(Function(x) x.IsDefault)
            If defScale Is Nothing AndAlso scales.Count > 0 Then
                scales(0).IsDefault = True
            End If

            scales = RatingsScales.Where(Function(x) x.Type = ScaleType.stImpact).ToList
            defScale = scales.FirstOrDefault(Function(x) x.IsDefault)
            If defScale Is Nothing AndAlso scales.Count > 0 Then
                scales(0).IsDefault = True
            End If

            scales = RatingsScales.Where(Function(x) x.Type = ScaleType.stControls).ToList
            defScale = scales.FirstOrDefault(Function(x) x.IsDefault)
            If defScale Is Nothing AndAlso scales.Count > 0 Then
                scales(0).IsDefault = True
            End If

            scales = RatingsScales.Where(Function(x) x.Type = ScaleType.stVulnerability).ToList
            defScale = scales.FirstOrDefault(Function(x) x.IsDefault)
            If defScale Is Nothing AndAlso scales.Count > 0 Then
                scales(0).IsDefault = True
            End If

            scales = RatingsScales.Where(Function(x) x.Type = ScaleType.stShared AndAlso x.IsExpectedValues).ToList
            defScale = scales.FirstOrDefault(Function(x) x.IsDefault)
            If defScale Is Nothing AndAlso scales.Count > 0 Then
                scales(0).IsDefault = True
            End If

            scales = RatingsScales.Where(Function(x) x.Type = ScaleType.stShared AndAlso x.IsOutcomes).ToList
            defScale = scales.FirstOrDefault(Function(x) x.IsDefault)
            If defScale Is Nothing AndAlso scales.Count > 0 Then
                scales(0).IsDefault = True
            End If

            scales = RatingsScales.Where(Function(x) x.Type = ScaleType.stShared AndAlso x.IsPWofPercentages).ToList
            defScale = scales.FirstOrDefault(Function(x) x.IsDefault)
            If defScale Is Nothing AndAlso scales.Count > 0 Then
                scales(0).IsDefault = True
            End If

            'For Each RS As clsRatingScale In RatingsScales
            '    If RS.IsDefault Then defaultScaleExists = True
            '    If RS.Name = DEFAULT_RATING_SCALE_NAME Then defaultScale = RS
            '    If RS.Name = DEFAULT_RATING_SCALE_NAME Then RS.IsDefault = True
            '    If RS.Name = DEFAULT_RATING_SCALE_NAME_FOR_LIKELIHOOD Then RS.IsDefault = True
            '    If RS.Name = DEFAULT_RATING_SCALE_NAME_FOR_IMPACT Then RS.IsDefault = True
            '    If RS.Name = DEFAULT_RATING_SCALE_NAME_FOR_LIKELIHOOD Then RS.IsDefault = True
            '    If RS.Name = DEFAULT_RATING_SCALE_NAME_CONTROLS Then RS.IsDefault = True
            '    If RS.Name = DEFAULT_PWOP_SCALE_NAME Then RS.IsDefault = True
            '    If RS.Name = DEFAULT_EXPECTED_VALUES_SCALE_NAME Then RS.IsDefault = True
            '    If RS.Name = DEFAULT_OUTCOMES_SCALE_NAME Then RS.IsDefault = True
            '    If RS.Name = DEFAULT_RATING_SCALE_NAME_VULNERABILITY Then RS.IsDefault = True

            '    If RS.IsOutcomes AndAlso RS.IsDefault Then defaultScaleExistsForPWOutcomes = True
            '    If RS.IsPWofPercentages AndAlso RS.IsDefault Then defaultScaleExistsForPWPercentages = True
            'Next
            'If Not defaultScaleExists AndAlso defaultScale IsNot Nothing Then
            '    defaultScale.IsDefault = True
            'Else
            '    For Each RS As clsRatingScale In RatingsScales
            '        If Not RS.IsOutcomes AndAlso Not RS.IsPWofPercentages AndAlso Not RS.IsExpectedValues Then
            '            RS.IsDefault = True
            '            Exit For
            '        End If
            '    Next
            'End If

            'If Not defaultScaleExistsForPWOutcomes Then
            '    For Each RS As clsRatingScale In RatingsScales
            '        If RS.IsOutcomes Then RS.IsDefault = True
            '        Exit For
            '    Next
            'End If

            'If Not defaultScaleExistsForPWPercentages Then
            '    For Each RS As clsRatingScale In RatingsScales
            '        If RS.IsPWofPercentages Then RS.IsDefault = True
            '        Exit For
            '    Next
            'End If

            defaultScale = Nothing
            defaultScaleExists = False
            For Each SF As clsStepFunction In StepFunctions
                If SF.IsDefault Then defaultScaleExists = True
                If defaultScale Is Nothing Then defaultScale = SF
                If SF.Name = DEFAULT_STEP_FUNCTION_NAME Then defaultScale = SF
                If SF.Name = DEFAULT_STEP_FUNCTION_NAME Then SF.IsDefault = True
                If SF.Name = DEFAULT_STEP_FUNCTION_FOR_LIKELIHOOD_NAME Then SF.IsDefault = True
                If SF.Name = DEFAULT_STEP_FUNCTION_FOR_IMPACT_NAME Then SF.IsDefault = True
                If SF.Name = DEFAULT_STEP_FUNCTION_FOR_CONTROLS_NAME Then SF.IsDefault = True
                If SF.Name = DEFAULT_STEP_FUNCTION_FOR_VULNERABILITY_NAME Then SF.IsDefault = True
            Next
            If Not defaultScaleExists AndAlso defaultScale IsNot Nothing Then
                defaultScale.IsDefault = True
                For Each h As clsHierarchy In ProjectManager.Hierarchies
                    For Each node As clsNode In h.Nodes
                        If node.MeasureType = ECMeasureType.mtStep AndAlso node.MeasurementScale Is Nothing Then
                            node.StepFunctionID = defaultScale.ID
                        End If
                    Next
                Next
            End If

            defaultScale = Nothing
            defaultScaleExists = False
            For Each UC As clsRegularUtilityCurve In RegularUtilityCurves
                If UC.IsDefault Then defaultScaleExists = True
                If defaultScale Is Nothing Then defaultScale = uc
                If UC.Name = DEFAULT_REGULAR_UTILITY_CURVE_NAME Then defaultScale = UC
                If UC.Name = DEFAULT_REGULAR_UTILITY_CURVE_NAME Then UC.IsDefault = True
                If UC.Name = DEFAULT_REGULAR_UTILITY_CURVE_FOR_LIKELIHOOD_NAME Then UC.IsDefault = True
                If UC.Name = DEFAULT_REGULAR_UTILITY_CURVE_FOR_IMPACT_NAME Then UC.IsDefault = True
                If UC.Name = DEFAULT_REGULAR_UTILITY_CURVE_FOR_CONTROLS_NAME Then UC.IsDefault = True
                If UC.Name = DEFAULT_REGULAR_UTILITY_CURVE_FOR_VULNERABILITY_NAME Then UC.IsDefault = True
            Next
            If Not defaultScaleExists AndAlso defaultScale IsNot Nothing Then
                defaultScale.IsDefault = True
                For Each h As clsHierarchy In ProjectManager.Hierarchies
                    For Each node As clsNode In h.Nodes
                        If node.MeasureType = ECMeasureType.mtRegularUtilityCurve AndAlso node.MeasurementScale Is Nothing Then
                            node.RegularUtilityCurveID = defaultScale.ID
                        End If
                    Next
                Next
            End If
        End Sub

        Public Function FixRatingScales() As Boolean
            Dim WasFixed As Boolean = False
            For Each RS As clsRatingScale In RatingsScales
                If RS.RatingSet.Count > 1 Then
                    ' sorting ratings by ID (increasing)
                    RS.SortByID()

                    Dim newID As Integer = CType(RS.RatingSet(RS.RatingSet.Count - 1), clsRating).ID + 1

                    For i As Integer = RS.RatingSet.Count - 1 To 0 Step -1
                        Dim CurRating As clsRating = CType(RS.RatingSet(i), clsRating)
                        Dim WasBefore As Boolean = False

                        Dim j As Integer = 0
                        While j < i And Not WasBefore
                            If CurRating.ID = CType(RS.RatingSet(j), clsRating).ID Then
                                WasBefore = True
                            End If
                            j += 1
                        End While

                        If WasBefore Then
                            CurRating.ID = newID
                            newID += 1
                            WasFixed = True
                        End If
                    Next
                End If
            Next

            Return WasFixed
        End Function

        Public Function GetRatingScalePairwiseType(RS As clsRatingScale) As Canvas.PairwiseType
            Dim res As Object = Attributes.GetAttributeValue(ATTRIBUTE_RATING_SCALE_PAIRWISE_TYPE_ID, RS.GuidID)
            If res IsNot Nothing Then
                Return CType(res, Canvas.PairwiseType)
            Else
                Return ProjectManager.PipeParameters.PairwiseType
            End If
        End Function

        Public Function SetRatingScalePairwiseType(RS As clsRatingScale, Value As Canvas.PairwiseType) As Boolean
            Dim res As Boolean = Attributes.SetAttributeValue(ATTRIBUTE_RATING_SCALE_PAIRWISE_TYPE_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtLong, Value, RS.GuidID, Guid.Empty)
            If res Then
                Return Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, StorageManager.ProjectLocation, StorageManager.ProviderType, StorageManager.ModelID, UNDEFINED_USER_ID)
            Else
                Return False
            End If
        End Function

        Public Function GetRatingScaleDiagonalsEvaluation(RS As clsRatingScale) As DiagonalsEvaluation
            Dim res As Object = Attributes.GetAttributeValue(ATTRIBUTE_RATING_SCALE_EVALUATE_DIAGONALS_MODE_ID, RS.GuidID)
            If res IsNot Nothing Then
                Return CType(res, DiagonalsEvaluation)
            Else
                Return ProjectManager.PipeParameters.EvaluateDiagonals
            End If
        End Function

        Public Function SetRatingScaleDiagonalsEvaluation(RS As clsRatingScale, Value As DiagonalsEvaluation) As Boolean
            Dim res As Boolean = Attributes.SetAttributeValue(ATTRIBUTE_RATING_SCALE_EVALUATE_DIAGONALS_MODE_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtLong, Value, RS.GuidID, Guid.Empty)
            If res Then
                Return Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, StorageManager.ProjectLocation, StorageManager.ProviderType, StorageManager.ModelID, UNDEFINED_USER_ID)
            Else
                Return False
            End If
        End Function

        Public Function GetRatingScaleForceGraphical(RS As clsRatingScale) As Boolean
            Dim res As Object = Attributes.GetAttributeValue(ATTRIBUTE_RATING_SCALE_FORCE_GRAPHICAL_ID, RS.GuidID)
            If res IsNot Nothing Then
                Return CType(res, Boolean)
            Else
                Return ProjectManager.PipeParameters.ForceGraphical
            End If
        End Function

        Public Function SetRatingScaleForceGraphical(RS As clsRatingScale, Value As Boolean) As Boolean
            Dim res As Boolean = Attributes.SetAttributeValue(ATTRIBUTE_RATING_SCALE_FORCE_GRAPHICAL_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtBoolean, Value, RS.GuidID, Guid.Empty)
            If res Then
                Return Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, StorageManager.ProjectLocation, StorageManager.ProviderType, StorageManager.ModelID, UNDEFINED_USER_ID)
            Else
                Return False
            End If
        End Function

        Public Function GetRatingScaleShowInconsistencRatioy(RS As clsRatingScale) As Boolean
            Dim res As Object = Attributes.GetAttributeValue(ATTRIBUTE_RATING_SCALE_SHOW_INCONSISTENCY_RATIO_ID, RS.GuidID)
            If res IsNot Nothing Then
                Return CType(res, Boolean)
            Else
                Return ProjectManager.PipeParameters.ShowConsistencyRatio
            End If
        End Function

        Public Function SetRatingScaleShowInconsistencyRatio(RS As clsRatingScale, Value As Boolean) As Boolean
            Dim res As Boolean = Attributes.SetAttributeValue(ATTRIBUTE_RATING_SCALE_SHOW_INCONSISTENCY_RATIO_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtBoolean, Value, RS.GuidID, Guid.Empty)
            If res Then
                Return Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, StorageManager.ProjectLocation, StorageManager.ProviderType, StorageManager.ModelID, UNDEFINED_USER_ID)
            Else
                Return False
            End If
        End Function

        Public Function GetRatingScaleShowMultiPairwise(RS As clsRatingScale) As Boolean
            Dim res As Object = Attributes.GetAttributeValue(ATTRIBUTE_RATING_SCALE_SHOW_MULTI_PW_ID, RS.GuidID)
            If res IsNot Nothing Then
                Return CType(res, Boolean)
            Else
                Return True
            End If
        End Function

        Public Function SetRatingScaleShowMultiPairwise(RS As clsRatingScale, Value As Boolean) As Boolean
            Dim res As Boolean = Attributes.SetAttributeValue(ATTRIBUTE_RATING_SCALE_SHOW_MULTI_PW_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtBoolean, Value, RS.GuidID, Guid.Empty)
            If res Then
                Return Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, StorageManager.ProjectLocation, StorageManager.ProviderType, StorageManager.ModelID, UNDEFINED_USER_ID)
            Else
                Return False
            End If
        End Function

        Public Function CreateHierarchyFromRatingScale(RS As clsRatingScale, sExcludeGUIDs As String()) As clsHierarchy
            If RS Is Nothing Then Return Nothing

            Dim sExcludeList As New List(Of String)
            For Each sGuid As String In sExcludeGUIDs
                sGuid = sGuid.ToLower
                sExcludeList.Add(sGuid)
            Next

            Dim goalNode As clsNode

            Dim Hierarchy As clsHierarchy = ProjectManager.GetHierarchyByID(RS.GuidID)
            If Hierarchy Is Nothing Then
                Hierarchy = ProjectManager.AddMeasureHierarchy()
                Hierarchy.HierarchyName = RS.Name
                Hierarchy.HierarchyGuidID = RS.GuidID

                goalNode = Hierarchy.AddNode(-1)
                goalNode.NodeName = RS.Name
                goalNode.NodeGuidID = RS.GuidID
                goalNode.InfoDoc = RS.Comment
            Else
                goalNode = Hierarchy.Nodes(0)
                goalNode.NodeName = RS.Name
                goalNode.InfoDoc = RS.Comment
            End If

            Hierarchy.ResetNodesDictionaries()

            Dim rNode As clsNode
            Dim R As clsRating

            For i As Integer = Hierarchy.Nodes.Count - 1 To 0 Step -1
                If Not Hierarchy.Nodes(i).NodeGuidID.Equals(goalNode.NodeGuidID) Then
                    R = RS.GetRatingByID(Hierarchy.Nodes(i).NodeGuidID)
                    If R Is Nothing OrElse sExcludeList.Contains(Hierarchy.Nodes(i).NodeGuidID.ToString.ToLower) Then
                        Hierarchy.DeleteNode(Hierarchy.Nodes(i))
                    End If
                End If
            Next

            For Each R In RS.RatingSet
                Dim node As clsNode = Hierarchy.GetNodeByID(R.GuidID)
                If node Is Nothing Then
                    If Not sExcludeList.Contains(R.GuidID.ToString.ToLower) Then
                        rNode = Hierarchy.AddNode(goalNode.NodeID)
                        rNode.NodeName = R.Name
                        rNode.NodeGuidID = R.GuidID
                        rNode.InfoDoc = R.Comment
                    End If
                Else
                    node.NodeName = R.Name
                    node.InfoDoc = R.Comment
                End If
            Next

            If goalNode.Children.Count = 0 Then Return Nothing

            If goalNode.Children.Count = 0 Then Return Nothing

            Dim tmpNode As clsNode = goalNode.Children(0)
            R = RS.RatingSet(0)
            rNode = Hierarchy.GetNodeByID(R.GuidID)
            Hierarchy.MoveNode(rNode, tmpNode, NodeMoveAction.nmaBeforeNode)
            tmpNode = rNode

            For i As Integer = 1 To RS.RatingSet.Count - 1
                R = RS.RatingSet(i)
                rNode = Hierarchy.GetNodeByID(R.GuidID)
                Hierarchy.MoveNode(rNode, tmpNode, NodeMoveAction.nmaAfterNode)
                tmpNode = rNode
            Next

            goalNode.MeasureType = ECMeasureType.mtPairwise

            Hierarchy.ResetNodesDictionaries()

            StorageManager.Writer.SaveProject(True)

            Return Hierarchy
        End Function

        Public Function CreateHierarchyFromStepFunction(SF As clsStepFunction, sExcludeGUIDs As String()) As clsHierarchy
            If SF Is Nothing Then Return Nothing

            Dim sExcludeList As New List(Of String)
            For Each sGuid As String In sExcludeGUIDs
                sGuid = sGuid.ToLower
                sExcludeList.Add(sGuid)
            Next

            Dim goalNode As clsNode

            Dim Hierarchy As clsHierarchy = ProjectManager.GetHierarchyByID(SF.GuidID)
            If Hierarchy Is Nothing Then
                Hierarchy = ProjectManager.AddMeasureHierarchy()
                Hierarchy.HierarchyName = SF.Name
                Hierarchy.HierarchyGuidID = SF.GuidID

                goalNode = Hierarchy.AddNode(-1)
                goalNode.NodeName = SF.Name
                goalNode.NodeGuidID = SF.GuidID
            Else
                goalNode = Hierarchy.Nodes(0)
                goalNode.NodeName = SF.Name
            End If

            Hierarchy.ResetNodesDictionaries()

            For i As Integer = Hierarchy.Nodes.Count - 1 To 0 Step -1
                If Not Hierarchy.Nodes(i).NodeGuidID.Equals(goalNode.NodeGuidID) Then
                    Dim SI As clsStepInterval = SF.GetIntervalByComment(Hierarchy.Nodes(i).NodeGuidID.ToString.ToLower)
                    If SI Is Nothing OrElse sExcludeList.Contains(Hierarchy.Nodes(i).NodeGuidID.ToString.ToLower) Then
                        Hierarchy.DeleteNode(Hierarchy.Nodes(i))
                    End If
                End If
            Next

            For Each SI As clsStepInterval In SF.Intervals
                Dim tGuid As Guid
                If String.IsNullOrEmpty(SI.Comment) OrElse Not Guid.TryParse(SI.Comment, tGuid) Then
                    SI.Comment = Guid.NewGuid.ToString
                End If

                Dim node As clsNode = Hierarchy.GetNodeByID(New Guid(SI.Comment.ToLower))
                If node Is Nothing Then
                    If Not sExcludeList.Contains(SI.Comment.ToLower) Then
                        Dim siNode As clsNode = Hierarchy.AddNode(goalNode.NodeID)
                        siNode.NodeName = SI.Name + " (Low: " + SI.Low.ToString + ")"
                        siNode.NodeGuidID = New Guid(SI.Comment.ToLower)
                    End If
                Else
                    node.NodeName = SI.Name + " (Low: " + SI.Low.ToString + ")"
                End If
            Next

            goalNode.MeasureType = ECMeasureType.mtPairwise

            Hierarchy.ResetNodesDictionaries()

            StorageManager.Writer.SaveProject(True)

            Return Hierarchy
        End Function

        Public Function ExtractRatingScaleFromHierarchy(Hierarchy As clsHierarchy, RS As clsRatingScale, Optional UserID As Integer = UNDEFINED_USER_ID, Optional ByName As Boolean = False) As Boolean
            If Hierarchy Is Nothing Or RS Is Nothing Then Return False
            If Hierarchy.HierarchyType <> ECHierarchyType.htMeasure Or Hierarchy.Nodes.Count < 2 Then Return False

            Dim CT As clsCalculationTarget
            Dim user As clsUser = Nothing
            If UserID <> UNDEFINED_USER_ID Then
                user = ProjectManager.GetUserByID(UserID)
            End If

            CT = New clsCalculationTarget(CalculationTargetType.cttUser, ProjectManager.User)

            Dim goal As clsNode = Hierarchy.Nodes(0)
            goal.CalculateLocal(CT)

            For Each R As clsRating In RS.RatingSet
                R.Value = 0
            Next

            For Each node As clsNode In goal.GetNodesBelow(UNDEFINED_USER_ID)
                Dim R As clsRating
                If ByName Then
                    R = RS.GetRatingByName(node.NodeName)
                Else
                    R = RS.GetRatingByID(node.NodeGuidID)
                End If
                If R IsNot Nothing Then
                    R.Value = node.LocalPriority(CT)
                End If
            Next

            Dim max As Single = 0
            For Each R As clsRating In RS.RatingSet
                If R.Value > max Then
                    max = R.Value
                End If
            Next
            If max <> 0 Then
                For Each R As clsRating In RS.RatingSet
                    R.Value = R.Value / max
                Next
            End If

            Return True
        End Function

        Public Function ExtractStepFunctionFromHierarchy(Hierarchy As clsHierarchy, SF As clsStepFunction, Optional UserID As Integer = UNDEFINED_USER_ID) As Boolean
            If Hierarchy Is Nothing Or SF Is Nothing Then Return False
            If Hierarchy.HierarchyType <> ECHierarchyType.htMeasure Or Hierarchy.Nodes.Count < 2 Then Return False

            Dim CT As clsCalculationTarget
            Dim user As clsUser = Nothing
            If UserID <> UNDEFINED_USER_ID Then
                user = ProjectManager.GetUserByID(UserID)
            End If

            CT = New clsCalculationTarget(CalculationTargetType.cttUser, ProjectManager.User)

            Dim goal As clsNode = Hierarchy.Nodes(0)
            goal.CalculateLocal(CT)

            For Each SI As clsStepInterval In SF.Intervals
                SI.Value = 0
            Next

            Dim max As Double = -1
            For Each node As clsNode In goal.GetNodesBelow(UNDEFINED_USER_ID)
                Dim SI As clsStepInterval = SF.GetIntervalByComment(node.NodeGuidID.ToString.ToLower)
                If SI IsNot Nothing Then
                    SI.Value = node.LocalPriority(CT)
                    If SI.Value > max Then
                        max = SI.Value
                    End If
                End If
            Next

            If max <> -1 And max <> 0 Then
                For Each SI As clsStepInterval In SF.Intervals
                    SI.Value /= max
                Next
            End If

            Return True
        End Function

        Private Function GetIntensityIDByName(SourceScale As clsRatingScale, SourceScaleIntensityID As Integer, DestScale As clsRatingScale) As Integer
            Dim rating As clsRating = SourceScale.GetRatingByID(SourceScaleIntensityID)
            If rating Is Nothing Then Return -1
            Dim destRating As clsRating = DestScale.GetRatingByName(rating.Name)
            If destRating Is Nothing Then Return -1 Else Return destRating.ID
        End Function

        Public Function RatingScaleMaintainIntensitiesAndJudgments(SourceScaleID As Guid, DestScaleID As Guid) As Boolean
            Dim sourceScale As clsRatingScale = GetRatingScaleByID(SourceScaleID)
            If sourceScale Is Nothing Then Return False

            Dim destScale As clsRatingScale = GetScaleByID(DestScaleID)
            If destScale Is Nothing Then Return False

            For Each r As clsRating In sourceScale.RatingSet
                Dim destR As clsRating = destScale.GetRatingByName(r.Name)
                If destR IsNot Nothing Then
                    destR.Value = r.Value
                Else
                    destR = destScale.AddIntensity()
                    destR.Name = r.Name
                    destR.Value = r.Value
                End If
            Next

            Dim sourceH As clsHierarchy = ProjectManager.GetAnyHierarchyByID(SourceScaleID)

            If sourceH IsNot Nothing Then
                Dim destH As clsHierarchy = ProjectManager.GetAnyHierarchyByID(DestScaleID)
                If destH Is Nothing Then
                    destH = ProjectManager.AddMeasureHierarchy()
                Else
                    destH.Nodes.Clear()
                    destH.ResetNodesDictionaries()
                End If
                destH.HierarchyName = destScale.Name
                destH.HierarchyGuidID = destScale.GuidID

                For Each node As clsNode In sourceH.Nodes
                    If node.ParentNodeID <> -1 AndAlso destScale.GetRatingByName(node.NodeName) Is Nothing Then
                        Dim r As clsRating = destScale.AddIntensity()
                        r.Name = node.NodeName
                    End If

                    Dim newNode As New clsNode(destH)
                    If node.ParentNode Is Nothing Then
                        newNode.NodeID = 100
                        newNode.NodeGuidID = New Guid
                    Else
                        newNode.NodeID = destScale.GetRatingByName(node.NodeName).ID
                        newNode.NodeGuidID = destScale.GetRatingByName(node.NodeName).GuidID
                    End If
                    'newNode.NodeID = node.NodeID
                    'newNode.NodeGuidID = node.NodeGuidID
                    newNode.NodeName = node.NodeName
                    newNode.Comment = node.Comment
                    If destH.Nodes.Count > 0 Then
                        newNode.ParentNodeID = destH.Nodes(0).NodeID
                        newNode.ParentNode = destH.Nodes(0)
                    Else
                        newNode.NodeName = destScale.Name
                        newNode.ParentNodeID = -1
                        newNode.ParentNode = Nothing
                    End If

                    destH.Nodes.Add(newNode)

                    If node.ParentNodeID <> -1 AndAlso destScale.GetRatingByName(node.NodeName) Is Nothing Then
                        Dim r As clsRating = destScale.AddIntensity()
                        r.Name = node.NodeName
                    End If
                Next

                destH.ResetNodesDictionaries()

                StorageManager.Reader.LoadUserJudgments(ProjectManager.User)

                Dim needSave As Boolean = sourceH.Nodes(0).Judgments.JudgmentsFromAllUsers.Count > 0
                If needSave Then
                    For Each J As clsPairwiseMeasureData In sourceH.Nodes(0).Judgments.JudgmentsFromAllUsers
                        Dim firstRating As clsRating = destScale.GetRatingByName(sourceH.GetNodeByID(J.FirstNodeID).NodeName)
                        Dim secondRating As clsRating = destScale.GetRatingByName(sourceH.GetNodeByID(J.SecondNodeID).NodeName)
                        If firstRating IsNot Nothing And secondRating IsNot Nothing Then
                            Dim newPWData As New clsPairwiseMeasureData(firstRating.ID, secondRating.ID, J.Advantage, J.Value, 100, J.UserID, J.IsUndefined, J.Comment)
                            destH.Nodes(0).Judgments.AddMeasureData(newPWData, True)
                        End If
                    Next

                    ExtractRatingScaleFromHierarchy(destH, destScale, , True)

                    StorageManager.Writer.SaveUserJudgments(ProjectManager.User.UserID)
                End If
            End If

            StorageManager.Writer.SaveModelStructure()

            Return True
        End Function
    End Class
End Namespace