Option Strict On

Imports ECCore.MathFuncs
Imports ECCore
Imports System.Math
Imports System.Xml 'A0031
Imports System.Linq

Namespace ECCore

    <Serializable()> Public Enum ECMeasureType
        mtNone = -1
        mtPairwise = 0
        mtRatings = 1
        mtRegularUtilityCurve = 2
        'mtDecreasingUC = 3
        mtCustomUtilityCurve = 4
        mtDirect = 5
        mtStep = 6
        mtAdvancedUtilityCurve = 7 'C0025
        mtPWOutcomes = 8
        'mtPWPercentages = 9
        mtPWAnalogous = 10
    End Enum

    Public Enum RatingScaleType
        rsRegular = 0
        rsExpectedValues = 1
        rsOutcomes = 2
        rsPWOfPercentages = 3
    End Enum

    Public Enum ECInterpolationMethod 'C0037
        imLagrange = 0
        imAkima = 1
    End Enum

    <Serializable()> Public Enum ScaleType
        stShared = 0
        stLikelihood = 1
        stImpact = 2
        stControls = 3
        stVulnerability = 4
    End Enum

    <Serializable()> Public Class AggregatedData
        Public Property Value As Double
        Public Property Weight As Double

        Public Sub New(aValue As Double, aWeight As Double)
            Value = aValue
            Weight = aWeight
        End Sub
    End Class


    <Serializable()> Public Class clsCustomMeasureData
        Public Property Comment() As String
        Public Property UserID() As Integer

        Protected mIsUndefined As Boolean
        Public Overridable Property IsUndefined() As Boolean
            Get
                Return mIsUndefined
            End Get
            Set(ByVal value As Boolean)
                mIsUndefined = value
            End Set
        End Property

        Private mModifyTime As DateTime

        Public Property AggregatedValue As Double = 0
        Public Property AggregatedValues As New List(Of AggregatedData)
        Public Property AggregatedValues2 As New List(Of Double)
        Public Property UsersCount As Integer = 0

        Public ModifyDate As DateTime
    End Class

    <Serializable()> Public MustInherit Class clsNonPairwiseMeasureData
        Inherits clsCustomMeasureData
        Public Property CtrlObjectiveID As Guid
        Public Property CtrlEventID As Guid
        Public Property AltNormalizedValue As Single
        Public Property NodeID() As Integer
        Public Property ParentNodeID() As Integer
        Public MustOverride ReadOnly Property SingleValue As Single
        Public MustOverride Property ObjectValue As Object

        Public Sub New()
            AggregatedValue = 0
            UsersCount = 0
        End Sub
    End Class

#Region "Ratings classes"
    <Serializable()> Public Class clsRating
        Public Property ID() As Integer
        Public Property GuidID() As Guid = Guid.NewGuid

        Private mName As String = ""
        Public Property Name() As String
            Get
                Return mName
            End Get
            Set(ByVal value As String)
                value = value.Trim  ' D4090
                If value.Length > RATING_INTENSITY_NAME_MAX_LENGTH Then
                    mName = value.Remove(RATING_INTENSITY_NAME_MAX_LENGTH)
                Else
                    mName = value
                End If
            End Set
        End Property

        Public Property AvailableForPW As Boolean
        Public Property Priority As Single
        Public Property Value2 As Single
        Public Property Comment As String = ""
        Public Property Value As Single
        Public ReadOnly Property RatingScale() As clsRatingScale

        Public ReadOnly Property RatingScaleID() As Integer
            Get
                Return If(RatingScale IsNot Nothing, RatingScale.ID, -1)
            End Get
        End Property

        Public Sub New()
        End Sub

        Public Sub New(ByVal ID As Integer, ByVal Name As String, ByVal Value As Single, ByVal RatingScale As clsRatingScale, Optional ByVal Comment As String = "")
            Me.ID = ID
            Me.Name = Name.Trim
            Me.Value = Value
            Me.RatingScale = RatingScale
            Me.Comment = Comment
        End Sub
    End Class

    <Serializable()> Public Class clsRatingIncreasingComparer
        Implements IComparer(Of clsRating)

        Public Function Compare(ByVal x As clsRating, ByVal y As clsRating) As Integer _
             Implements IComparer(Of clsRating).Compare
            Return If(x.Value = y.Value, 0, If(x.Value > y.Value, -1, 1))
        End Function
    End Class

    <Serializable()> Public Class clsRatingDecreasingComparer
        Implements IComparer(Of clsRating)

        Public Function Compare(ByVal x As clsRating, ByVal y As clsRating) As Integer _
             Implements IComparer(Of clsRating).Compare
            Return If(x.Value = y.Value, 0, If(x.Value < y.Value, -1, 1))
        End Function
    End Class

    <Serializable()> Public Class clsRatingIDIncreasingComparer
        Implements IComparer(Of clsRating)

        Public Function Compare(ByVal x As clsRating, ByVal y As clsRating) As Integer _
             Implements IComparer(Of clsRating).Compare
            Return If(x.ID = y.ID, 0, If(x.ID < y.ID, -1, 1))
        End Function
    End Class

    <Serializable()> Public Class clsRatingIDDecreasingComparer
        Implements IComparer(Of clsRating)

        Public Function Compare(ByVal x As clsRating, ByVal y As clsRating) As Integer _
             Implements IComparer(Of clsRating).Compare
            Return If(x.ID = y.ID, 0, If(x.ID > y.ID, -1, 1))
        End Function
    End Class

    <Serializable()> Public Class clsRatingMeasureDataComparer
        Implements IComparer(Of clsCustomMeasureData)

        Public Function Compare(ByVal x As clsCustomMeasureData, ByVal y As clsCustomMeasureData) As Integer _
             Implements IComparer(Of clsCustomMeasureData).Compare

            Dim A As clsRatingMeasureData = CType(x, clsRatingMeasureData)
            Dim B As clsRatingMeasureData = CType(y, clsRatingMeasureData)

            If (A.ParentNodeID = B.ParentNodeID) And (A.NodeID = B.NodeID) Then Return 0
            If (A.ParentNodeID = B.ParentNodeID) Then Return If((A.NodeID < B.NodeID), -1, 1)
            Return If((A.ParentNodeID < B.ParentNodeID), -1, 1)
        End Function
    End Class

    <Serializable()> Public MustInherit Class clsMeasurementScale
        Public Property ID() As Integer
        Public Property GuidID() As Guid = Guid.NewGuid

        Private mName As String = ""
        Public Property Name() As String
            Get
                Return mName
            End Get
            Set(ByVal value As String)
                value = value.Trim  ' D4090
                If value.Length > MEASUREMENT_SCALE_NAME_LENGTH Then
                    mName = value.Remove(MEASUREMENT_SCALE_NAME_LENGTH)
                Else
                    mName = value
                End If
            End Set
        End Property
        Public Property Comment() As String = ""
        Public Property IsDefault() As Boolean = False
        Public Property Type As ScaleType = ScaleType.stShared

        Public Sub New()
        End Sub
    End Class

    <Serializable()> Public Class clsRatingScale
        Inherits clsMeasurementScale

        Public Property RatingSet As New List(Of clsRating)
        Public Property IsOutcomes As Boolean
        Public Property IsPWofPercentages As Boolean
        Public Property IsExpectedValues As Boolean

        Public Function Clone() As clsRatingScale
            Dim newRS As New clsRatingScale(-1)
            newRS.IsDefault = IsDefault
            newRS.IsExpectedValues = IsExpectedValues
            newRS.IsOutcomes = IsOutcomes
            newRS.IsPWofPercentages = IsPWofPercentages
            newRS.Comment = Comment
            newRS.Type = Type
            newRS.Name = "Copy of " + Name
            For Each RI As clsRating In RatingSet
                Dim newRI As New clsRating(RI.ID, RI.Name, RI.Value, newRS, RI.Comment)
                newRI.GuidID = RI.GuidID
                newRI.AvailableForPW = RI.AvailableForPW
                newRI.Priority = RI.Priority
                newRI.Value2 = RI.Value2
                newRS.RatingSet.Add(newRI)
            Next
            Return newRS
        End Function

        Public Function AddIntensity() As clsRating
            Dim newID As Integer = RatingSet.Select(Function(r) (r.ID)).DefaultIfEmpty(-1).Max + 1
            Dim res As clsRating = New clsRating(newID, "New intensity " + newID.ToString, 0, Me)
            RatingSet.Add(res)
            Return res
        End Function

        Public Overloads Function GetRatingByID(ByVal RatingID As Integer) As clsRating
            Return RatingSet.FirstOrDefault(Function(r) (r.ID = RatingID))
        End Function

        Public Function GetRatingByName(ByVal RatingName As String) As clsRating
            Return RatingSet.FirstOrDefault(Function(r) (r.Name.ToLower = RatingName.ToLower))
        End Function

        Public Overloads Function GetRatingByID(ByVal RatingID As Guid) As clsRating
            Return RatingSet.FirstOrDefault(Function(r) (r.GuidID.Equals(RatingID)))
        End Function

        Public Function GetMaxRating() As clsRating
            Dim res As clsRating = Nothing
            Dim max As Single = -1
            For Each R As clsRating In RatingSet
                If R.Value > max Then
                    max = R.Value
                    res = R
                End If
            Next
            Return res
        End Function

        Public Function GetMinRating() As clsRating
            Dim res As clsRating = Nothing
            Dim min As Single = 2
            For Each R As clsRating In RatingSet
                If R.Value < min Then
                    min = R.Value
                    res = R
                End If
            Next
            Return res
        End Function

        Public Sub Sort(Optional ByVal Increasing As Boolean = True)
            If Increasing Then
                RatingSet.Sort(New clsRatingIncreasingComparer)
            Else
                RatingSet.Sort(New clsRatingDecreasingComparer)
            End If
        End Sub

        Public Sub SortByID(Optional ByVal Increasing As Boolean = True)
            If Increasing Then
                RatingSet.Sort(New clsRatingIDIncreasingComparer)
            Else
                RatingSet.Sort(New clsRatingIDDecreasingComparer)
            End If
        End Sub

        Public Function AdjustValues(IntensityID As Guid, Value As Single) As Boolean
            If Value < 0 Or Value > 1 Then Return False
            Dim R As clsRating = GetRatingByID(IntensityID)
            If R Is Nothing Then Return False
            Dim CurValue As Single = R.Value
            If CurValue = Value Then Return True

            Dim delta As Single = Abs(CurValue - Value)
            Dim Increased As Boolean = Value > CurValue

            Dim sum As Single = 0
            For Each RI As clsRating In RatingSet
                If RI IsNot R Then
                    sum += RI.Value
                End If
            Next
            If sum <> 0 Then
                For Each RI As clsRating In RatingSet
                    If RI IsNot R Then
                        If Increased Then
                            RI.Value -= delta * RI.Value / sum
                        Else
                            RI.Value += delta * RI.Value / sum
                        End If
                    End If
                Next
            End If
            Return True
        End Function

        Public Sub New(ByVal nID As Integer)
            ID = nID
            IsOutcomes = False
            IsPWofPercentages = False
            IsExpectedValues = False
        End Sub
    End Class

    <Serializable()> Public Class clsRatingMeasureData
        Inherits clsNonPairwiseMeasureData

        Public ReadOnly Property RatingScale() As clsRatingScale
        Public Property Rating As clsRating
        Public Overrides Property IsUndefined() As Boolean
            Get
                Return If(Rating Is Nothing, True, mIsUndefined)
            End Get
            Set(ByVal value As Boolean)
                mIsUndefined = value
                If value Then Rating = Nothing
            End Set
        End Property

        Public Overrides ReadOnly Property SingleValue() As Single
            Get
                Return If(IsUndefined, 0, If(Rating.Value > 1, 1, Rating.Value))
            End Get
        End Property

        Public Overrides Property ObjectValue() As Object
            Get
                Return Rating
            End Get
            Set(value As Object)
                Rating = CType(value, clsRating)
            End Set
        End Property

        Public Sub New(ByVal NodeID As Integer, ByVal ParentNodeID As Integer, ByVal UserID As Integer,
            ByVal Rating As clsRating, ByVal RatingScale As clsRatingScale, Optional ByVal IsUndefined As Boolean = False, Optional ByVal Comment As String = "")

            Me.NodeID = NodeID
            Me.ParentNodeID = ParentNodeID
            Me.Rating = Rating
            Me.RatingScale = RatingScale
            Me.UserID = UserID
            Me.IsUndefined = IsUndefined
            Me.Comment = Comment
        End Sub
    End Class
#End Region

#Region "Pairwise classes"

    <Serializable()> Public Class clsPairwiseComparer
        Implements IComparer(Of clsCustomMeasureData)

        Private mNode As clsNode
        Private mNodesBelow As List(Of clsNode) 'C0384

        Public Property Node() As clsNode
            Get
                Return mNode
            End Get
            Set(ByVal value As clsNode)
                mNode = value
                mNodesBelow = Node.GetNodesBelow(UNDEFINED_USER_ID) 'C0450
            End Set
        End Property

        Public Function Compare(ByVal x As clsCustomMeasureData, ByVal y As clsCustomMeasureData) As Integer _
            Implements IComparer(Of clsCustomMeasureData).Compare

            Dim A As clsPairwiseMeasureData = CType(x, clsPairwiseMeasureData)
            Dim B As clsPairwiseMeasureData = CType(y, clsPairwiseMeasureData)

            Dim AChild1Index As Integer
            Dim AChild2Index As Integer
            Dim BChild1Index As Integer
            Dim BChild2Index As Integer

            Dim i As Integer

            If mNodesBelow IsNot Nothing Then
                For i = 0 To mNodesBelow.Count - 1
                    Dim nodeID As Integer = CType(mNodesBelow(i), clsNode).NodeID

                    If nodeID = A.FirstNodeID Then
                        AChild1Index = i
                    End If

                    If nodeID = A.SecondNodeID Then
                        AChild2Index = i
                    End If

                    If nodeID = B.FirstNodeID Then
                        BChild1Index = i
                    End If

                    If nodeID = B.SecondNodeID Then
                        BChild2Index = i
                    End If
                Next
            End If

            If (AChild1Index = BChild1Index) And (AChild2Index = BChild2Index) Then
                Return 0
            End If

            If (AChild1Index = BChild1Index) Then
                Return If((AChild2Index < BChild2Index), -1, 1)
            End If

            Return If((AChild1Index < BChild1Index), -1, 1)
        End Function
    End Class

    <Serializable()> Public Class clsPairwiseOutcomesComparer
        Implements IComparer(Of clsCustomMeasureData)

        Public Property RatingScale As clsRatingScale

        Public Function Compare(ByVal x As clsCustomMeasureData, ByVal y As clsCustomMeasureData) As Integer _
            Implements IComparer(Of clsCustomMeasureData).Compare

            Dim A As clsPairwiseMeasureData = CType(x, clsPairwiseMeasureData)
            Dim B As clsPairwiseMeasureData = CType(y, clsPairwiseMeasureData)

            Dim AChild1Index As Integer
            Dim AChild2Index As Integer
            Dim BChild1Index As Integer
            Dim BChild2Index As Integer

            Dim i As Integer

            If RatingScale IsNot Nothing Then
                For i = 0 To RatingScale.RatingSet.Count - 1
                    'C0326===
                    Dim nodeID As Integer = CType(RatingScale.RatingSet(i), clsRating).ID

                    If nodeID = A.FirstNodeID Then
                        AChild1Index = i
                    End If

                    If nodeID = A.SecondNodeID Then
                        AChild2Index = i
                    End If

                    If nodeID = B.FirstNodeID Then
                        BChild1Index = i
                    End If

                    If nodeID = B.SecondNodeID Then
                        BChild2Index = i
                    End If
                Next
            End If

            If (AChild1Index = BChild1Index) And (AChild2Index = BChild2Index) Then
                Return 0
            End If

            If (AChild1Index = BChild1Index) Then
                Return If((AChild2Index < BChild2Index), -1, 1)
            End If

            Return If((AChild1Index < BChild1Index), -1, 1)
        End Function
    End Class

    ''' <summary>
    ''' This class was designed to represent the pairwise comparison's data.
    ''' </summary>
    ''' <remarks></remarks>
    <Serializable()> Public Class clsPairwiseMeasureData
        Inherits clsCustomMeasureData
        Public Property ParentNodeID() As Integer
        Public Property FirstNodeID() As Integer
        Public Property SecondNodeID() As Integer
        Public Property OutcomesNodeID As Integer
        Public Property Advantage() As Integer
        Public Property Value() As Double

        Public Overrides Property IsUndefined() As Boolean
            Get
                If Not mIsUndefined Then
                    If Double.IsInfinity(Value) OrElse Double.IsNaN(Value) Then
                        mIsUndefined = True
                    End If
                End If
                Return mIsUndefined
            End Get
            Set(ByVal value As Boolean)
                mIsUndefined = value
                If value Then
                    Advantage = 0
                    Me.Value = 0
                End If
            End Set
        End Property

        Public Sub New(ByVal FirstNodeID As Integer, ByVal SecondNodeID As Integer, ByVal Advantage As Integer, ByVal Value As Double, ByVal ParentNodeID As Integer,
            ByVal UserID As Integer, Optional ByVal IsUndefined As Boolean = False, Optional ByVal Comment As String = "")

            Me.FirstNodeID = FirstNodeID
            Me.SecondNodeID = SecondNodeID
            Me.Advantage = Advantage
            Me.Value = Value
            Me.ParentNodeID = ParentNodeID

            Me.UserID = UserID
            Me.Comment = Comment
            Me.IsUndefined = IsUndefined

            AggregatedValue = 1
            UsersCount = 0
        End Sub

        Public Sub WriteXML(ByRef writer As XmlTextWriter)
            writer.WriteStartElement("FirstNode")
            writer.WriteAttributeString("ID", FirstNodeID.ToString)
            writer.WriteEndElement()
            writer.WriteStartElement("SecondNode")
            writer.WriteAttributeString("ID", SecondNodeID.ToString)
            writer.WriteEndElement()
            writer.WriteStartElement("ParentNode")
            writer.WriteAttributeString("ID", ParentNodeID.ToString)
            writer.WriteEndElement()
            writer.WriteStartElement("Advantage")
            writer.WriteAttributeString("Advantage", Advantage.ToString)
            writer.WriteEndElement()
            writer.WriteStartElement("Value")
            If IsUndefined Then
                writer.WriteAttributeString("Value", String.Empty)
            Else
                writer.WriteAttributeString("Value", Value.ToString)
            End If
            writer.WriteEndElement()
        End Sub
    End Class
#End Region

#Region "Utility Curve classes"
    <Serializable()> Public MustInherit Class clsCustomUtilityCurve
        Inherits clsMeasurementScale

        Public MustOverride Property Low() As Single

        Public MustOverride Property High() As Single

        Public MustOverride Function GetValue(ByVal X As Single) As Single
    End Class

    <Serializable()> Public Class clsRegularUtilityCurve
        Inherits clsCustomUtilityCurve

        Private v1 As Integer
        Private xLow As Double
        Private xHigh As Double
        Private xCurvature As Double
        Private v2 As Boolean
        Public Overrides Property Low() As Single

        Public Overrides Property High() As Single

        Public Property Curvature() As Single

        Public Property IsLinear() As Boolean

        Public Property IsIncreasing() As Boolean

        Public Function Clone() As clsRegularUtilityCurve
            Dim newRUC As New clsRegularUtilityCurve(-1, Low, High, Curvature, IsLinear, IsIncreasing)
            newRUC.Comment = Comment
            newRUC.Type = Type
            newRUC.IsDefault = IsDefault
            newRUC.Name = "Copy of " + Name
            Return newRUC
        End Function

        Public Overrides Function GetValue(ByVal X As Single) As Single
            If (Low >= High) Then Return 0

            Dim n As Single

            If IsIncreasing Then
                If (X < Low) Then
                    Return 0
                End If

                If (X > High) Then
                    Return 1
                End If

                n = (High - Low) * Curvature
                If n = 0 Then
                    Return (X - Low) / (High - Low)
                Else
                    Return CSng((1 - Exp(-(X - Low) / n)) / (1 - Exp(-(High - Low) / n)))
                End If
            Else
                If (X < Low) Then
                    Return 1
                End If

                If (X > High) Then
                    Return 0
                End If

                n = (High - Low) * Curvature
                If n = 0 Then
                    Return (High - X) / (High - Low)
                Else
                    Return CSng((1 - Exp(-(High - X) / n)) / (1 - Exp(-(High - Low) / n)))
                End If
            End If
        End Function

        Public Sub New(ByVal ID As Integer, ByVal Low As Single, ByVal High As Single, ByVal Curvature As Single, Optional ByVal IsLinear As Boolean = True, Optional ByVal IsIncreasing As Boolean = True)
            Me.ID = ID
            Me.Low = Low
            Me.High = High
            Me.Curvature = Curvature
            Me.IsLinear = IsLinear
            Me.IsIncreasing = IsIncreasing
        End Sub

        Public Sub New(v1 As Integer, xLow As Double, xHigh As Double, xCurvature As Double, isLinear As Boolean, v2 As Boolean)
            Me.v1 = v1
            Me.xLow = xLow
            Me.xHigh = xHigh
            Me.xCurvature = xCurvature
            Me.IsLinear = isLinear
            Me.v2 = v2
        End Sub
    End Class

    <Serializable()> Public Class clsUtilityCurveMeasureData
        Inherits clsNonPairwiseMeasureData

        Private mData As Single
        Private mValue As Single
        Private mValueExplicitlySet As Boolean

        Public ReadOnly Property UtilityCurve As clsCustomUtilityCurve

        Public Property Data As Single
            Get
                Return mData
            End Get
            Set(ByVal value As Single)
                mData = value
            End Set
        End Property

        Public Overrides Property IsUndefined As Boolean
            Get
                Return If(Single.IsNaN(mData), True, mIsUndefined)
            End Get
            Set(ByVal value As Boolean)
                mIsUndefined = value
                If value Then mData = Single.NaN
            End Set
        End Property

        Public Overrides Property ObjectValue As Object
            Get
                Return mData
            End Get
            Set(ByVal value As Object)
                mData = CSng(value)
            End Set
        End Property

        Public Overrides ReadOnly Property SingleValue As Single
            Get
                If IsCombinedUserID(UserID) Then Return CSng(ObjectValue)

                If mValueExplicitlySet Then
                    If Single.IsInfinity(mValue) OrElse Single.IsNaN(mValue) Then
                        Return 0
                    Else
                        Return If(mValue > 1, 1, mValue)
                    End If
                Else
                    If Single.IsInfinity(mData) OrElse Single.IsNaN(mData) Then
                        Return 0
                    Else
                        Dim v As Single = UtilityCurve.GetValue(mData)
                        Return If(v > 1, 1, UtilityCurve.GetValue(mData))
                    End If
                End If
            End Get
        End Property

        Public Sub New(ByVal NodeID As Integer, ByVal ParentNodeID As Integer, ByVal UserID As Integer,
            ByVal Data As Single, ByVal UtilityCurve As clsCustomUtilityCurve, Optional ByVal IsUndefined As Boolean = False, Optional ByVal Comment As String = "",
            Optional ByVal ExplicitlySet As Boolean = False, Optional ByVal ExplicitValue As Single = 0) 'C0060

            Me.NodeID = NodeID
            Me.ParentNodeID = ParentNodeID
            mData = Data
            Me.UtilityCurve = UtilityCurve

            Me.UserID = UserID
            Me.IsUndefined = IsUndefined
            Me.Comment = Comment

            mValueExplicitlySet = ExplicitlySet
            mValue = ExplicitValue
        End Sub
    End Class

    <Serializable()> Public Class clsUCPoint
        Public Property X As Single = 0
        Public Property Y As Single = 0

        Public Sub New(Optional ByVal X As Single = 0, Optional ByVal Y As Single = 0)
            Me.X = X
            Me.Y = Y
        End Sub
    End Class

    <Serializable()> Public Class clsAdvancedUtilityCurve
        Inherits clsCustomUtilityCurve

        Private mPoints As New ArrayList
        Private mInterpolationMethod As ECInterpolationMethod = ECInterpolationMethod.imLagrange 'C0037 'C0039

        Public Property InterpolationMethod() As ECInterpolationMethod 'C0037 'C0039
            Get
                Return mInterpolationMethod
            End Get
            Set(ByVal value As ECInterpolationMethod)
                mInterpolationMethod = value
            End Set
        End Property

        Private Sub InitPoints()
            mPoints.Add(New clsUCPoint(0, 0))
            mPoints.Add(New clsUCPoint(1, 1))
        End Sub

        Private Function GetLowXValue() As Single
            If mPoints.Count = 0 Then
                InitPoints()
            End If

            Dim min As Single = Single.MaxValue
            For Each point As clsUCPoint In mPoints
                If point.X < min Then
                    min = point.X
                End If
            Next
            Return min
        End Function

        Private Function GetHighXValue() As Single
            If mPoints.Count = 0 Then
                InitPoints()
            End If

            Dim max As Single = Single.MinValue
            For Each point As clsUCPoint In mPoints
                If point.X > max Then
                    max = point.X
                End If
            Next
            Return max
        End Function

        Private Sub SetLowXValue(ByVal value As Single)
            Dim min As Single = GetLowXValue()
            If value < min Then
                Dim newPoint As New clsUCPoint(value, 0)
            End If
        End Sub

        Private Sub SetHighXValue(ByVal value As Single)
            Dim max As Single = GetHighXValue()
            If value > max Then
                Dim newPoint As New clsUCPoint(value, 1)
            End If
        End Sub

        Public Overrides Property Low() As Single
            Get
                Return GetLowXValue()
            End Get
            Set(ByVal value As Single)
                SetLowXValue(value)
            End Set
        End Property

        Public Overrides Property High() As Single
            Get
                Return GetHighXValue()
            End Get
            Set(ByVal value As Single)
                SetHighXValue(value)
            End Set
        End Property

        Private Function PointExistsByX(ByVal X As Single) As Boolean
            For Each point As clsUCPoint In mPoints
                If point.X = X Then
                    Return True
                End If
            Next
            Return False
        End Function

        Private Function PointExistsByXY(ByVal X As Single, ByVal Y As Single) As Boolean
            For Each point As clsUCPoint In mPoints
                If (point.X = X) And (point.Y = Y) Then
                    Return True
                End If
            Next
            Return False
        End Function

        Public Function AddUCPoint(ByVal X As Single, ByVal Y As Single) As clsUCPoint
            If PointExistsByX(X) Or (Y < 0) Or (Y > 1) Then
                Return Nothing
            End If

            For Each point As clsUCPoint In mPoints
                If point.X = X Then
                    point.Y = Y
                    Return point
                End If
            Next

            Dim P As New clsUCPoint(X, Y)
            mPoints.Add(P)
            Return P
        End Function

        Public Function GetUCPointByX(ByVal X As Single) As clsUCPoint
            For Each point As clsUCPoint In mPoints
                If point.X = X Then
                    Return point
                End If
            Next
            Return Nothing
        End Function

        Public Function GetUCPointsByY(ByVal Y As Single) As ArrayList
            Dim res As New ArrayList
            For Each point As clsUCPoint In mPoints
                If point.Y = Y Then
                    res.Add(point)
                End If
            Next
            Return res
        End Function

        Public Function GetUCPointByXY(ByVal X As Single, ByVal Y As Single) As clsUCPoint
            For Each point As clsUCPoint In mPoints
                If (point.X = X) And (point.Y = Y) Then
                    Return point
                End If
            Next
            Return Nothing
        End Function

        Public ReadOnly Property Points() As ArrayList
            Get
                Return mPoints
            End Get
        End Property

        Private Function LagrangeInterpolation(ByVal X As Single) As Single
            Dim sum As Single = 0
            Dim tmp As Single

            For i As Integer = 0 To Points.Count - 1
                tmp = CType(Points(i), clsUCPoint).Y
                For j As Integer = 0 To Points.Count - 1
                    If i <> j Then
                        tmp *= (X - CType(Points(j), clsUCPoint).X) / (CType(Points(i), clsUCPoint).X - CType(Points(j), clsUCPoint).X)
                    End If
                Next
                sum += tmp
            Next

            'C0046===
            'Return sum 
            If sum > 1 Then
                Return 1
            End If
            If sum < 0 Then
                Return 0
            End If
            Return sum
            'C0046==
        End Function

        Public Overrides Function GetValue(ByVal X As Single) As Single
            Select Case InterpolationMethod 'C0037 'C0039
                Case ECInterpolationMethod.imLagrange
                    Return LagrangeInterpolation(X)
            End Select
        End Function

        Public Sub New(ByVal nID As Integer) 'C0032
            MyBase.New() 'C0260
            ID = nID 'C0032
            InitPoints()
        End Sub
    End Class
#End Region

#Region "Step Functions classes"
    <Serializable()> Public Class clsStepInterval
        Public Property ID() As Integer

        Private mName As String = ""
        Public Property Name() As String
            Get
                Return mName
            End Get
            Set(ByVal value As String)
                value = value.Trim  ' D4090
                If value.Length > STEP_INTERVAL_NAME_MAX_LENGTH Then
                    mName = value.Remove(STEP_INTERVAL_NAME_MAX_LENGTH)
                Else
                    mName = value
                End If
            End Set
        End Property
        Public Property Comment() As String = ""
        Public Property Low() As Single
        Public Property High() As Single
        Public Property Value() As Single

        Public ReadOnly Property StepFunction() As clsStepFunction

        Public Sub New()
            Comment = Guid.NewGuid.ToString
        End Sub

        Public Sub New(ByVal ID As Integer, ByVal Name As String, ByVal Low As Single, ByVal High As Single, ByVal Value As Single, ByVal StepFunction As clsStepFunction, Optional ByVal comment As String = "")
            Me.ID = ID
            Me.Name = Name.Trim
            Me.Low = Low
            Me.High = High
            Me.Value = Value
            StepFunction = StepFunction
            If comment <> "" Then
                Me.Comment = comment
            Else
                Me.Comment = Guid.NewGuid.ToString
            End If
        End Sub
    End Class

    <Serializable()> Public Class clsStepFunction
        Inherits clsMeasurementScale

        Public Property Intervals() As New List(Of clsStepInterval)

        Public Function Clone() As clsStepFunction
            Dim newSF As New clsStepFunction(-1)
            newSF.IsDefault = IsDefault
            newSF.IsPiecewiseLinear = IsPiecewiseLinear
            newSF.Comment = Comment
            newSF.Type = Type
            newSF.Name = "Copy of " + Name
            For Each SI As clsStepInterval In Intervals
                Dim newSI As New clsStepInterval(SI.ID, SI.Name, SI.Low, SI.High, SI.Value, newSF, SI.Comment)
                newSF.Intervals.Add(newSI)
            Next
            Return newSF
        End Function

        Public Property IsPiecewiseLinear() As Boolean = True

        Public Function AddInterval() As clsStepInterval
            Dim newID As Integer = Intervals.Select(Function(i) (i.ID)).DefaultIfEmpty(-1).Max
            Dim res As clsStepInterval = New clsStepInterval(newID, "New step interval " + newID.ToString, 0, 0, 0, Me)
            Intervals.Add(res)
            Return res
        End Function

        Public Function GetIntervalByID(ByVal IntervalID As Integer) As clsStepInterval
            Return Intervals.FirstOrDefault(Function(i) (i.ID = IntervalID))
        End Function

        Public Function GetIntervalByName(ByVal IntervalName As String) As clsStepInterval
            Return Intervals.FirstOrDefault(Function(i) (i.Name.ToLower = IntervalName.ToLower))
        End Function

        Public Function GetIntervalByComment(ByVal IntervalComment As String) As clsStepInterval
            Return Intervals.FirstOrDefault(Function(i) (i.Comment.ToLower = IntervalComment.ToLower))
        End Function

        Public Function GetMaxValueInterval() As clsStepInterval
            Return Intervals.OrderByDescending(Function(i) (i.Value)).FirstOrDefault()
        End Function

        Public Function GetMinValueInterval() As clsStepInterval
            Return Intervals.OrderBy(Function(i) (i.Value)).FirstOrDefault()
        End Function

        Public Function GetUpperBound() As Single
            Dim maxSI As clsStepInterval = GetMaxValueInterval()
            Return If(maxSI IsNot Nothing, maxSI.High, 0)
        End Function

        Public Function GetLowerBound() As Single
            Dim minSI As clsStepInterval = GetMinValueInterval()
            Return If(minSI IsNot Nothing, minSI.Low, 0)
        End Function

        Public Function GetValue(ByVal X As Single) As Single
            SortByInterval() 'C0158
            For i As Integer = 0 To Intervals.Count - 2 'C0316
                CType(Intervals(i), clsStepInterval).High = CType(Intervals(i + 1), clsStepInterval).Low
            Next

            X = CSng(Math.Round(X, 3))

            'For Each SI As clsStepInterval In Intervals 'C0316
            For i As Integer = 0 To Intervals.Count - 1 'C0316
                Dim SI As clsStepInterval = CType(Intervals(i), clsStepInterval)
                If (X >= SI.Low) And (X < SI.High) Then
                    'Return SI.Value 'C0300
                    'C0300===
                    If IsPiecewiseLinear Then
                        'TODO: ENTER CODE HERE FOR PIECEWISE LINEAR CALCULATIONS
                        'C0316===
                        ' for -infinity
                        'If i = 0 Then 'C0319
                        If (i = 0) And (SI.Low = NEGATIVE_INFINITY) Then 'C0319
                            Return SI.Value
                        End If

                        ' for intervals in the middle
                        If i < Intervals.Count - 1 Then 'C0319
                            Dim SINext As clsStepInterval = CType(Intervals(i + 1), clsStepInterval)

                            Dim k As Single = (SINext.Value - SI.Value) / (SI.High - SI.Low)
                            Dim b As Single = SI.Value - k * SI.Low

                            Return (k * X + b)
                        Else
                            Return SI.Value 'C0319
                        End If

                        ' for +infinity
                        If i = Intervals.Count - 1 Then
                            Return SI.Value
                        End If
                    Else
                        Return SI.Value
                    End If
                End If
            Next

            If Intervals.Count > 1 Then
                If (X >= CType(Intervals(Intervals.Count - 1), clsStepInterval).High) Then
                    Return CType(Intervals(Intervals.Count - 1), clsStepInterval).Value
                End If

                If (X < CType(Intervals(0), clsStepInterval).Low) Then
                    Return CType(Intervals(0), clsStepInterval).Value
                End If
            End If
            'C0946==
        End Function

        Public Sub SortByPriority(Optional ByVal Increasing As Boolean = True)
            If Increasing Then
                Intervals.Sort(New clsStepIntervalPriorityIncreasingComparer)
            Else
                Intervals.Sort(New clsStepIntervalPriorityDecreasingComparer)
            End If
        End Sub

        Public Sub SortByInterval(Optional ByVal Increasing As Boolean = True) 'C0158
            If Increasing Then
                Intervals.Sort(New clsStepIntervalIntervalIncreasingComparer)
            Else
                Intervals.Sort(New clsStepIntervalIntervalDecreasingComparer)
            End If
        End Sub

        Public Sub New(ByVal ID As Integer)
            Me.ID = ID
        End Sub
    End Class

    <Serializable()> Public Class clsStepIntervalPriorityIncreasingComparer
        Implements IComparer(Of clsStepInterval)

        Public Function Compare(ByVal x As clsStepInterval, ByVal y As clsStepInterval) As Integer _
             Implements IComparer(Of clsStepInterval).Compare
            Return If(x.Value = y.Value, 0, If(x.Value < y.Value, -1, 1))
        End Function
    End Class

    <Serializable()> Public Class clsStepIntervalPriorityDecreasingComparer
        Implements IComparer(Of clsStepInterval)

        Public Function Compare(ByVal x As clsStepInterval, ByVal y As clsStepInterval) As Integer _
             Implements IComparer(Of clsStepInterval).Compare
            Return If(x.Value = y.Value, 0, If(x.Value > y.Value, -1, 1))
        End Function
    End Class

    'C0158===
    Public Class clsStepIntervalIntervalIncreasingComparer
        Implements IComparer(Of clsStepInterval)

        Public Function Compare(ByVal x As clsStepInterval, ByVal y As clsStepInterval) As Integer _
             Implements IComparer(Of clsStepInterval).Compare
            Return If(x.Low = y.Low, 0, If(x.Low < y.Low, -1, 1))
        End Function
    End Class

    <Serializable()> Public Class clsStepIntervalIntervalDecreasingComparer
        Implements IComparer(Of clsStepInterval)

        Public Function Compare(ByVal x As clsStepInterval, ByVal y As clsStepInterval) As Integer _
             Implements IComparer(Of clsStepInterval).Compare
            Return If(x.Low = y.Low, 0, If(x.Low > y.Low, -1, 1))
        End Function
    End Class

    <Serializable()> Public Class clsStepMeasureData
        Inherits clsNonPairwiseMeasureData

        Private mValueExplicitlySet As Boolean = False
        Public ReadOnly Property StepFunction() As clsStepFunction

        Private mValue As Single = Single.NaN
        Public Property Value As Single
            Get
                Return mValue
            End Get
            Set(value As Single)
                mValue = value
            End Set
        End Property

        Public Overrides Property IsUndefined() As Boolean
            Get
                Return If(Single.IsNaN(Value), True, mIsUndefined)
            End Get
            Set(ByVal value As Boolean)
                mIsUndefined = value
                If value Then mValue = Single.NaN
            End Set
        End Property

        Public Overrides ReadOnly Property SingleValue() As Single
            Get
                If mValueExplicitlySet Then
                    If Single.IsInfinity(Value) OrElse Single.IsNaN(Value) Then
                        Return 0
                    Else
                        Return If(Value > 1, 1, Value)
                    End If
                Else
                    Dim v As Single = StepFunction.GetValue(Value)
                    Return If(IsUndefined, 0, If(v > 1, 1, v))
                End If
            End Get
        End Property

        Public Overrides Property ObjectValue() As Object
            Get
                Return mValue
            End Get
            Set(ByVal value As Object)
                mValue = CSng(value)
            End Set
        End Property

        Public Sub New(ByVal NodeID As Integer, ByVal ParentNodeID As Integer, ByVal UserID As Integer,
            ByVal Value As Single, ByVal StepFunction As clsStepFunction, Optional ByVal IsUndefined As Boolean = False, Optional ByVal Comment As String = "",
            Optional ByVal ExplicitlySet As Boolean = False, Optional ByVal ExplicitValue As Single = 0)

            Me.NodeID = NodeID
            Me.ParentNodeID = ParentNodeID
            mValue = Value
            Me.StepFunction = StepFunction
            Me.UserID = UserID
            Me.IsUndefined = IsUndefined
            Me.Comment = Comment

            mValueExplicitlySet = ExplicitlySet
            If mValueExplicitlySet Then mValue = ExplicitValue
        End Sub
    End Class

#End Region

#Region "Direct Judgments classes"
    <Serializable()> Public Class clsDirectMeasureData 'C0007
        Inherits clsNonPairwiseMeasureData

        Private mDirectData As Single

        Public Property DirectData() As Single
            Get
                Return mDirectData
            End Get
            Set(ByVal value As Single)
                mDirectData = value
            End Set
        End Property

        Public Overrides Property IsUndefined() As Boolean
            Get
                Return Single.IsNaN(mDirectData)
            End Get
            Set(ByVal value As Boolean)
                mIsUndefined = value
                If value Then
                    mDirectData = Single.NaN
                End If
            End Set
        End Property

        Public Overrides ReadOnly Property SingleValue() As Single
            Get
                Return If(IsUndefined, 0, If(mDirectData > 1, 1, mDirectData))
            End Get
        End Property

        Public Overrides Property ObjectValue() As Object
            Get
                Return mDirectData
            End Get
            Set(ByVal value As Object)
                mDirectData = CSng(value)
            End Set
        End Property

        Public Sub New(ByVal NodeID As Integer, ByVal ParentNodeID As Integer, ByVal UserID As Integer,
            ByVal DirectData As Single, Optional ByVal IsUndefined As Boolean = False, Optional ByVal Comment As String = "")

            Me.NodeID = NodeID
            Me.ParentNodeID = ParentNodeID
            mDirectData = DirectData
            Me.UserID = UserID
            Me.IsUndefined = IsUndefined
            Me.Comment = Comment
        End Sub
    End Class
#End Region

End Namespace