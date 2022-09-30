Imports Canvas.CanvasTypes

Namespace ExpertChoice.Results

    Public Enum OperationID
        oNone = -1
        oGetPipeStepData = 0
        oJudgmentUpdate = 1
        oResetJudgments = 2
        oRestoreJudgments = 3
        oSaveJudgments = 4
        oInvertAllJudgments = 5
        oUpdatePWNL = 6 'Pairwise with known likelihoods
        oInvertCurrentJudgment = 7
    End Enum

    Public Class StepsPairs
        Public Obj1 As Integer
        Public Obj2 As Integer
        Public StepNumber As Integer
        Public Value As Double
        Public Rank As Integer
        Public BestFitValue As Double 'A0605
        Public BestFitAdvantage As Integer 'A0605
        Public Advantage As Integer
        'Public GridAdvantage As Integer = 1
        'Public ClientAdvantage As Integer = 1
        Public IsUndefined As Boolean
    End Class

    Public Class BarCellModel
        Private _Value As Double = 0
        Private _CombinedValue As Double = 0

        Private _ValueBarVisible As Boolean = False
        Private _CombinedBarVisible As Boolean = False

        Public Property Value() As Double
            Get
                Return _Value
            End Get
            Set(ByVal value As Double)
                _Value = value
            End Set
        End Property

        Public Property CombinedValue() As Double
            Get
                Return _CombinedValue
            End Get
            Set(ByVal value As Double)
                _CombinedValue = value
            End Set
        End Property

        Public Property ValueBarVisible() As Boolean
            Get
                Return _ValueBarVisible
            End Get
            Set(ByVal value As Boolean)
                _ValueBarVisible = value
            End Set
        End Property

        Public Property CombinedBarVisible() As Boolean
            Get
                Return _CombinedBarVisible
            End Get
            Set(ByVal value As Boolean)
                _CombinedBarVisible = value
            End Set
        End Property
    End Class

    Public Enum Visibility
        Visible
        Collapsed
    End Enum

    Public Class Objective
        'A0328 ===
        Private _Index As Integer = 0
        Public Property Index() As Integer
            Get
                Return _Index
            End Get
            Set(ByVal value As Integer)
                _Index = value
            End Set
        End Property
        'A0328 ==

        'A1370 ===
        Private _SortOrder As Integer = 0
        Public Property SortOrder() As Integer
            Get
                Return _SortOrder
            End Get
            Set(ByVal value As Integer)
                _SortOrder = value
            End Set
        End Property
        'A1370 ==

        Private _Parent As List(Of Objective)
        Public Property Parent() As List(Of Objective)
            Get
                Return _Parent
            End Get
            Set(ByVal value As List(Of Objective))
                _Parent = value
            End Set
        End Property

        Private _Name As String
        Public Property Name() As String
            Get
                'A0309 ===
                If _Name IsNot Nothing Then
                    If _Name.Length > 120 Then
                        _Name = _Name.Substring(0, 117) + "..."
                    End If
                End If
                'A0309 ==
                Return _Name
            End Get
            Set(ByVal value As String)
                _Name = value
            End Set
        End Property

        Private _UnnormalizedValue As Double
        Public Property UnnormalizedValue() As Double
            Get
                Return _UnnormalizedValue
            End Get
            Set(ByVal value As Double)
                _UnnormalizedValue = value
            End Set
        End Property

        Private _Value As Double
        Public Property Value() As Double
            Get
                Return _Value
            End Get
            Set(ByVal value As Double)
                _Value = value
            End Set
        End Property

        Private _GlobalValue As Double
        Public Property GlobalValue() As Double ' Value * Likelihood of the Parent Source
            Get
                Return _GlobalValue
            End Get
            Set(ByVal value As Double)
                _GlobalValue = value
            End Set
        End Property

        Private _GlobalValueCombined As Double
        Public Property GlobalValueCombined() As Double ' CombinedValue * CombinedLikelihood of the Parent Source
            Get
                Return _GlobalValueCombined
            End Get
            Set(ByVal value As Double)
                _GlobalValueCombined = value
            End Set
        End Property


        Private _UnnormalizedCombinedValue As Double
        Public Property UnnormalizedCombinedValue() As Double
            Get
                Return _UnnormalizedCombinedValue
            End Get
            Set(ByVal value As Double)
                _UnnormalizedCombinedValue = value
            End Set
        End Property

        'A0321 ===
        Private _CombinedValue As Double
        Public Property CombinedValue() As Double
            Get
                Return _CombinedValue
            End Get
            Set(ByVal value As Double)
                _CombinedValue = value
            End Set
        End Property
        'A0321 ==

        Private _ID As Integer
        Public Property ID() As Integer
            Get
                Return _ID
            End Get
            Set(ByVal value As Integer)
                _ID = value
            End Set
        End Property

        Private _IsChecked As Boolean = False
        Public Property IsChecked() As Boolean
            Get
                Return _IsChecked
            End Get
            Set(ByVal value As Boolean)
                _IsChecked = value
                'MessageBox.Show(String.Format("You checked - {0}", Name), "Message", MessageBoxButton.OK)
                Dim counter As Integer = 0
                For Each obj In Parent
                    If (obj.IsChecked) And (obj IsNot Me) Then counter += 1
                Next
                If counter >= 2 Then Me._IsChecked = False
                CheckIfStepPairExists() 'A0452
            End Set
        End Property

        Public StepPairs As List(Of StepsPairs) 'A0452

        Private Sub CheckIfStepPairExists() 'A0452
            Dim Obj1 As Objective = Nothing
            Dim Obj2 As Objective = Nothing

            If Parent IsNot Nothing Then
                For Each obj In Parent
                    obj.IsMarked = False
                    If obj.IsChecked Then
                        If Obj1 Is Nothing Then Obj1 = obj Else Obj2 = obj
                    End If
                Next
            End If

            If StepPairs IsNot Nothing AndAlso Obj1 IsNot Nothing AndAlso Obj2 IsNot Nothing Then
                Dim pairexists As Boolean = False
                For Each pair In StepPairs
                    If (pair.Obj1 = Obj1.ID AndAlso pair.Obj2 = Obj2.ID) OrElse (pair.Obj1 = Obj2.ID AndAlso pair.Obj2 = Obj1.ID) Then
                        pairexists = True
                        Exit For
                    End If
                Next

                If Not pairexists Then
                    Obj1.IsMarked = True
                    Obj2.IsMarked = True
                End If
            End If

        End Sub

        Private _IsMarked As Boolean = False 'A0452
        Public Property IsMarked() As Boolean 'A0452
            Get
                Return _IsMarked
            End Get
            Set(ByVal value As Boolean)
                _IsMarked = value
            End Set
        End Property

        Private _AltWithKnownLikelihoodID As Integer
        Public Property AltWithKnownLikelihoodID() As Integer
            Get
                Return _AltWithKnownLikelihoodID
            End Get
            Set(ByVal value As Integer)
                _AltWithKnownLikelihoodID = value
            End Set
        End Property

        Private _AltWithKnownLikelihoodGuidID As Guid = Guid.Empty
        Public Property AltWithKnownLikelihoodGuidID() As Guid
            Get
                Return _AltWithKnownLikelihoodGuidID
            End Get
            Set(ByVal value As Guid)
                _AltWithKnownLikelihoodGuidID = value
            End Set
        End Property

        Private _AltWithKnownLikelihoodName As String = ""
        Public Property AltWithKnownLikelihoodName() As String
            Get
                Return _AltWithKnownLikelihoodName
            End Get
            Set(ByVal value As String)
                _AltWithKnownLikelihoodName = value
            End Set
        End Property

        Private _AltWithKnownLikelihoodValue As Double = 0
        Public Property AltWithKnownLikelihoodValue() As Double
            Get
                Return _AltWithKnownLikelihoodValue
            End Get
            Set(ByVal value As Double)
                _AltWithKnownLikelihoodValue = value
            End Set
        End Property

        Private _AltWithKnownLikelihoodValueString As String = ""
        Public Property AltWithKnownLikelihoodValueString As String
            Get
                Return _AltWithKnownLikelihoodValueString
            End Get
            Set(value As String)
                _AltWithKnownLikelihoodValueString = value
            End Set
        End Property

    End Class


    Public Class DataModel
        Public HideHintMessages As Boolean = False

        Private _Inconsistency As Double
        Public Property Inconsistency() As Double
            Get
                Return _Inconsistency
            End Get
            Set(ByVal value As Double)
                _Inconsistency = value
            End Set
        End Property

        Private _InconsistencyVisible As Boolean = False
        Public Property InconsistencyVisible() As Boolean
            Get
                Return _InconsistencyVisible
            End Get
            Set(ByVal value As Boolean)
                _InconsistencyVisible = value
            End Set
        End Property

        Public ReadOnly Property InconsistencyBtnVisible() As Boolean
            Get
                'If ReadOnlyUI Then Return False
                Return InconsistencyVisible
            End Get
        End Property

        Public ReadOnly Property NotInconsistencyBtnVisible() As Boolean
            Get
                If ReadOnlyUI Then Return False
                Return Not InconsistencyVisible
            End Get
        End Property

        Private _ExpectedValueIndiv As Double
        Public Property ExpectedValueIndiv() As Double
            Get
                Return _ExpectedValueIndiv
            End Get
            Set(ByVal value As Double)
                _ExpectedValueIndiv = value
            End Set
        End Property

        Private _ExpectedValueIndivVisible As Boolean = False
        Public Property ExpectedValueIndivVisible As Boolean
            Get
                Return _ExpectedValueIndivVisible
            End Get
            Set(value As Boolean)
                _ExpectedValueIndivVisible = value
            End Set
        End Property

        Private _ExpectedValueComb As Double
        Public Property ExpectedValueComb() As Double
            Get
                Return _ExpectedValueComb
            End Get
            Set(ByVal value As Double)
                _ExpectedValueComb = value
            End Set
        End Property

        Private _ExpectedValueCombVisible As Boolean = False
        Public Property ExpectedValueCombVisible As Boolean
            Get
                Return _ExpectedValueCombVisible
            End Get
            Set(value As Boolean)
                _ExpectedValueCombVisible = value
            End Set
        End Property

        Private _ObjectivesData As New List(Of Objective)
        Public Property ObjectivesData() As List(Of Objective)
            Get
                Return _ObjectivesData
            End Get
            Set(ByVal value As List(Of Objective))
                _ObjectivesData = value
            End Set
        End Property

        Private _ObjectivesDataSorted As New List(Of Objective)
        Public Property ObjectivesDataSorted() As List(Of Objective)
            Get
                Return _ObjectivesDataSorted
            End Get
            Set(value As List(Of Objective))
                _ObjectivesDataSorted = value
            End Set
        End Property

        Public MaxAltPriority As Double 'for calculating the graph bar width

        Public ReadOnly Property InconsistencyString() As String
            Get
                Return Inconsistency.ToString("F2")
            End Get
        End Property

        Public ReadOnly Property ExpectedValueIndivString() As String
            Get
                Return ExpertChoice.Service.Double2String(ExpectedValueIndiv * 100, , True)
            End Get
        End Property

        Public ReadOnly Property ExpectedValueCombString() As String
            Get
                Return ExpertChoice.Service.Double2String(ExpectedValueComb * 100, , True)
            End Get
        End Property

        Public InsufficientInfo As Boolean
        Public ShowIndividualResults As Boolean
        Public ShowGroupResults As Boolean
        Public CanShowIndividualResults As Boolean
        Public CanShowGroupResults As Boolean
        Public CanEditModel As Boolean = False
        Public ShowKnownLikelihoods As Boolean = False
        Public IsForAlternatives As Boolean = False
        Public PWMode As PairwiseType = PairwiseType.ptGraphical    ' D3637
        Public IsPWNLandNormalizedParticipantResults As Boolean = False
        Public IsPWNLandNormalizedGroupResults As Boolean = False

        'A0452 ===
        Private _ParentID As Integer = -1
        Public Property ParentID() As Integer
            Get
                Return _ParentID
            End Get
            Set(ByVal value As Integer)
                _ParentID = value
            End Set
        End Property
        'A0452 ==

        Public ParentNode As ECCore.clsNode
        Public ParentNodeName As String = ""
        Public ParentNodeKnownLikelihood As String = ""
        Public ParentNodeGlobalPriority As Double = 0
        Public ParentNodeGlobalPriorityCombined As Double = 0
        Public IsParentNodeGoal As Boolean = False

        Public CanNotShowLocalResults As Boolean = False  'A0322
        Public StepPairs As List(Of StepsPairs)
        Public ReadOnlyUI As Boolean = False
    End Class

    'Public Class ObjectivesIndexComparer
    '    Implements IComparer(Of Objective)

    '    Public Function Compare(ByVal x As Objective, ByVal y As Objective) As Integer Implements System.Collections.Generic.IComparer(Of Objective).Compare
    '        Return x.Index.CompareTo(y.Index)
    '    End Function
    'End Class

    'Public Class ObjectivesNamesComparer
    '    Implements IComparer(Of Objective)

    '    Public Function Compare(ByVal x As Objective, ByVal y As Objective) As Integer Implements System.Collections.Generic.IComparer(Of Objective).Compare
    '        Return x.Name.CompareTo(y.Name)
    '    End Function
    'End Class

    'Public Class ObjectivesPriorityComparer
    '    Implements IComparer(Of Objective)

    '    Public Function Compare(ByVal x As Objective, ByVal y As Objective) As Integer Implements System.Collections.Generic.IComparer(Of Objective).Compare
    '        Return y.Value.CompareTo(x.Value)
    '    End Function
    'End Class

    'Public Class ObjectivesCombinedPriorityComparer
    '    Implements IComparer(Of Objective)

    '    Public Function Compare(ByVal x As Objective, ByVal y As Objective) As Integer Implements System.Collections.Generic.IComparer(Of Objective).Compare
    '        Return y.CombinedValue.CompareTo(x.CombinedValue)
    '    End Function
    'End Class

End Namespace
