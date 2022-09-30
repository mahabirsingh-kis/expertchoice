Option Strict On

Imports ECCore

Namespace Canvas
    <Serializable> Public Class EfficientFrontierSettings
        Public Property ConstraintType As EfficientFrontierConstraintType = EfficientFrontierConstraintType.BudgetLimit
        Public Property RiskConstraintType As RiskOptimizationType = RiskOptimizationType.BudgetLimit
        Public Property ConstraintID As Integer = -1
        Public Property WRTNodeGuid As Guid = Guid.Empty 'A1484
        Public Property ScenarioID As Integer = 0 'A1484
        Public Property ScenarioIndex As Integer = 0 'A1708
        Public Property Intervals As New List(Of EfficientFrontierInterval)
        Public Property IsIncreasing As Boolean = False
        Public Property CalculateLECValues As Boolean = False 'A1617 
        Public Property RedLineValue As Double = 0 'A1617
        Public Property GreenLineValue As Double = 0 'A1617
        Public Property KeepFundedAlts As Boolean = False
        Public Property SolveToken As Integer = 0
    End Class

    <Serializable> Public Class EfficientFrontierInterval
        Public Property DeltaType As EfficientFrontierDeltaType = EfficientFrontierDeltaType.DeltaValue
        Public Property DeltaValue As Double = EFFICIENT_FRONTIER_DEFAULT_NUMBER_OF_STEPS
        Public Property MinValue As Double = UNDEFINED_INTEGER_VALUE
        Public Property MaxValue As Double = UNDEFINED_INTEGER_VALUE
        Public Property Results As New List(Of EfficientFrontierResults)
    End Class

    <Serializable> Public Enum EfficientFrontierDeltaType
        NumberOfSteps = 0
        DeltaValue = 1
        'imMinCost = 2 'A1484
        'imMinDifferenceOfCosts = 3 'A1484
        AllSolutions = 4 'A1484
        '-A1604 DeltaValueIncreasing = 5
        '-A1604 NumberOfStepsIncreasing = 6 'A1604
        MinBenefitIncrease = 7
    End Enum

    <Serializable> Public Enum EfficientFrontierConstraintType
        BudgetLimit = 0
        Risk = 1
        CustomConstraint = 2
    End Enum

    <Serializable> Public Class EfficientFrontierResults
        Public Property SolverState As raSolverState = raSolverState.raNone
        Public Property Value As Double
        Public Property FundedBenefits As Double
        Public Property FundedBenefitsOriginal As Double
        Public Property FundedCost As Double
        Public Property LeverageGlobal As Double
        Public Property RiskReductionGlobal As Double
        Public Property RiskReductionMonetaryGlobal As Double
        Public Property ExpectedSavings As Double 'A1611
        Public Property ScenarioID As Integer
        Public Property ScenarioIndex As Integer 'A1708
        Public Property DeltaValue As Double
        Public Property DeltaMonetaryValue As Double
        Public Property DeltaCost As Double
        Public Property DeltaLeverage As Double
        Public Property DeltaExpectedSavings As Double 'A1611
        Public Property RedLineIntersection As Double = UNDEFINED_INTEGER_VALUE 'A1617
        Public Property GreenLineIntersection As Double = UNDEFINED_INTEGER_VALUE 'A1617
        Public Property AlternativesData As New Dictionary(Of String, Double)
        Public Property SolveToken As Integer = 0
    End Class

    Public Delegate Sub EfficientFrontierStepResultsSub(StepResult As EfficientFrontierResults, Progress As Integer, ByRef RefIsCancelled As Boolean)
    Public Delegate Sub EfficientFrontierCancelSub(ByRef RefIsCancelled As Boolean)

End Namespace