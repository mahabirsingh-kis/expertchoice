Option Strict On    ' D2882

Imports System.Collections.Generic
Imports System.Linq

Namespace Canvas
    <Serializable()> Public Class RAAlternative
        Public Property ID As String = ""
        Public ReadOnly Property IDVar As String
            Get
                Return ID
                'Return "a_" + mID.Replace(CChar("-"), "_")
            End Get
        End Property
        Public Property Name() As String = ""
        Public Property Enabled As Boolean = True
        Public Property SortOrder() As Integer
        Public Property BenefitOriginal As Double
        Public Property Benefit As Double

        <NonSerialized()> Public BenefitPartial As Double

        Private mRiskOriginal As Double
        Public Property RiskOriginal As Double
            Get
                If mRiskOriginal = ECCore.UNDEFINED_INTEGER_VALUE Then
                    Return 0
                Else
                    Return mRiskOriginal
                End If
            End Get
            Set(value As Double)
                mRiskOriginal = value
            End Set
        End Property

        ' D4348 ===
        Public ReadOnly Property Risk As Double
            Get
                If mRiskOriginal = ECCore.UNDEFINED_INTEGER_VALUE Then
                    Return 0
                Else
                    Return mRiskOriginal * BenefitOriginal  ' D4349 + D4353
                End If
            End Get
        End Property
        ' D4348 ==

        Private mCost As Double
        Public Property Cost As Double
            Get
                If mCost = ECCore.UNDEFINED_INTEGER_VALUE Or mCost = ECCore.UNDEFINED_ATTRIBUTE_DEFAULT_DOUBLE_VALUE Then
                    Return 0
                Else
                    Return mCost
                End If
            End Get
            Set(value As Double)
                mCost = value
            End Set
        End Property

        <NonSerialized()> Public CostPartial As Double

        Public Property IsPartial As Boolean

        Public Property Must As Boolean

        Public Property MustNot As Boolean

        ''' <summary>
        ''' TmpMust is used for temporarily storing the Must value for Efficient Frontier
        ''' </summary>
        <NonSerialized> Public TmpMust As Boolean
        '''' <summary>
        '''' TmpMustNot is used for temporarily storing the MustNot value for Efficient Frontier
        '''' </summary>
        '<NonSerialized> Public TmpMustNot As Boolean

        Public Property Funded As Double

        Public Property FundedOriginal As Double

        ''' <summary>
        ''' Rounded value of the Funded property, used to display the Funded/Not Funded in the UI
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property DisplayFunded As Double 'A0939
            Get
                Return Math.Round(Funded, ExpertChoice.FUNDED_ROUND_PRECISION)
            End Get
        End Property

        <NonSerialized()> Public FundedPartial As Double

        Private mMinPercent As Double = 0
        Public Property MinPercent As Double
            Get
                Return CDbl(If(IsPartial, mMinPercent, 0)) ' D2882
            End Get
            Set(value As Double)
                mMinPercent = value
            End Set
        End Property

        Public Property AllowCostTolerance As Boolean = False
        ' D4930 ===
        Private mCostTolerance As Double = 0.1
        Public Property CostTolerance As Double
            Get
                Return CDbl(If(AllowCostTolerance, mCostTolerance, 0))
            End Get
            Set(value As Double)
                mCostTolerance = value
            End Set
        End Property
        ' D4930 ==
        Public ReadOnly Property CostDelta As Double
            Get
                Return If(AllowCostTolerance, Cost * CostTolerance, 0)
            End Get
        End Property
        Public Property FundedCost As Double = 0

        ''' <summary>
        ''' Strategic Buckets page only property, don't use in other places
        ''' </summary>
        ''' <remarks></remarks>
        Public Property SBPriority As Double
            Get
                Return Benefit
            End Get
            Set(value As Double)

            End Set
        End Property

        ''' <summary>
        ''' Strategic Buckets page only property, don't use in other places
        ''' </summary>
        ''' <remarks></remarks>
        Public Property SBTotal As Double

        Public ReadOnly Property IsPartiallyFunded As Boolean 'A0933
            Get
                Return DisplayFunded > 0 AndAlso DisplayFunded < 1 'A0939
            End Get
        End Property

        Public Function Clone() As RAAlternative
            Dim newAlt As RAAlternative = CType(Me.MemberwiseClone, RAAlternative)  ' D2882
            newAlt.ID = String.Copy(ID)
            newAlt.Name = String.Copy(Name)
            Return newAlt
        End Function
    End Class

    <Serializable()> Public Class RASettings

        ' D4530 ===
        ''' <summary>
        ''' Only for Scenario Comparison and Increasing Budgets settings (for all scenarios settings - .ScenarioComparisonSettings) 
        ''' </summary>
        ''' <remarks></remarks>
        Public Property UseIgnoreOptions As Boolean = False
        Public Property UseBaseCaseOptions As Boolean = False
        Public Property Musts As Boolean = True
        Public Property MustNots As Boolean = True
        Public Property CustomConstraints As Boolean = True
        Public Property Groups As Boolean = True
        Public Property FundingPools As Boolean = True
        Public Property Dependencies As Boolean = True
        Public Property Risks As Boolean = True
        Public Property TimePeriods As Boolean = True
        Public Property ResourcesMin As Boolean = True
        Public Property ResourcesMax As Boolean = True
        Public Property CostTolerance As Boolean = True

        Public Property UseBaseCase As Boolean = True
        Public Property BaseCaseForGroups As Boolean = True
        Public Property BaseCaseForConstraints As Boolean = True
        Public Property BaseCaseForDependencies As Boolean = True
        Public Property BaseCaseForFundingPools As Boolean = True
        Public Property BaseCaseForMustNots As Boolean = True
        Public Property BaseCaseForMusts As Boolean = True
        ' D4530 ==

        Public Function Clone() As RASettings
            Dim newSettings As RASettings = CType(Me.MemberwiseClone, RASettings)
            Return newSettings
        End Function
    End Class

End Namespace