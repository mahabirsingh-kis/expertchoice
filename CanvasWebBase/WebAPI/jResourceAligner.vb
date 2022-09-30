Imports Canvas

Namespace ExpertChoice.WebAPI

    <Serializable> Public Class jRAScenario
        Inherits clsJsonObject

        Public Property ID As Integer
        Public Property Name As String
        Public Property Description As String

        Overloads Shared Function CreateFromBaseObject(tScenario As RAScenario) As jRAScenario
            If tScenario IsNot Nothing Then
                Return New jRAScenario With {
                .ID = tScenario.ID,
                .Name = tScenario.Name,
                .Description = tScenario.Description
            }
            Else
                Return Nothing
            End If
        End Function

    End Class

    <Serializable> Public Class jRAConstraint
        Inherits clsJsonObject
        Public Property id As Integer
        Public Property name As String
        Public Property isSoft As Boolean
        Public Property min As Double
        Public Property max As Double
        Public Property values As Dictionary(Of String, Double)
        Overloads Shared Function CreateFromBaseObject(tConstr As RAConstraint) As jRAConstraint
            If tConstr IsNot Nothing Then
                Dim tValues As New Dictionary(Of String, Double)
                For Each tVal As KeyValuePair(Of String, Double) In tConstr.AlternativesData
                    tValues.Add(tVal.Key, tVal.Value)
                Next

                Return New jRAConstraint With {
                    .id = tConstr.ID,
                    .name = tConstr.Name,
                    .isSoft = Not tConstr.Enabled,
                    .min = tConstr.MinValue,
                    .max = tConstr.MaxValue,
                    .values = tValues
                }
            Else
                Return Nothing
            End If
        End Function

    End Class

    <Serializable> Public Class jRAAlternative
        Inherits clsJsonObject

        Public Property ID As String
        Public Property SortOrder As Integer
        Public Property Name As String
        Public Property Funded As Double
        Public Property Benefit As Double
        Public Property EBenefit As Double
        Public Property Risk As Double
        Public Property RiskOriginal As Double
        Public Property Cost As Double
        Public Property Must As Boolean
        Public Property MustNot As Boolean

        Overloads Shared Function CreateFromBaseObject(tAlt As RAAlternative) As jRAAlternative
            If tAlt IsNot Nothing Then
                Return New jRAAlternative With {
                    .ID = tAlt.ID,
                    .SortOrder = tAlt.SortOrder,
                    .Name = tAlt.Name,
                    .Funded = tAlt.Funded,
                    .Benefit = tAlt.BenefitOriginal,
                    .EBenefit = tAlt.Benefit,
                    .Risk = tAlt.Risk,
                    .RiskOriginal = tAlt.RiskOriginal,
                    .Cost = tAlt.Cost,
                    .Must = tAlt.Must,
                    .MustNot = tAlt.MustNot
            }
            Else
                Return Nothing
            End If
        End Function

    End Class

    <Serializable> Public Class jRAGrid
        Inherits clsJsonObject

        Public Property BudgetLimit As Double = 0

        Public SolverState As raSolverState = raSolverState.raNone

        Public SolverMessage As String = ""

        Public Property Alternatives As List(Of jRAAlternative)
        Public Property Attributes As Object = Nothing
        Public Property constraints As List(Of jRAConstraint)

        Overloads Shared Function CreateFromBaseObject(RA As ResourceAligner) As jRAGrid
            Dim tRes As New jRAGrid

            Dim Scenario As RAScenario = RA.Scenarios.ActiveScenario

            tRes.BudgetLimit = Scenario.Budget
            tRes.SolverState = RA.Solver.SolverState
            tRes.SolverMessage = RA.Solver.LastError

            tRes.Alternatives = New List(Of jRAAlternative)
            For Each tAlt As RAAlternative In Scenario.Alternatives
                tRes.Alternatives.Add(jRAAlternative.CreateFromBaseObject(tAlt))
            Next

            tRes.constraints = New List(Of jRAConstraint)
            For Each tConstrKey As Integer In Scenario.Constraints.Constraints.Keys
                tRes.constraints.Add(jRAConstraint.CreateFromBaseObject(Scenario.Constraints.Constraints(tConstrKey)))
            Next

            Return tRes
        End Function

    End Class


End Namespace
