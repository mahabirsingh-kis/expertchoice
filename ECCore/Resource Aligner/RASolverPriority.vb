Option Strict On

Imports ECCore
Imports Canvas
Imports System.Collections.Generic
Imports System.Linq

<Serializable()> Public Class RASolverPriority

    <Serializable()> Public Enum raSolverCondition
        raMinimize = 0
        raMaximize = 1
    End Enum

    Public Property Rank As Integer
    Public Property ConstraintID As Integer ' D4360
    Public Property InUse As Boolean
    Public Property Condition As raSolverCondition
    Public Property Priority As Double

    Public Function Clone() As RASolverPriority
        Dim tNewPrty As New RASolverPriority
        tNewPrty.Rank = Me.Rank
        tNewPrty.ConstraintID = Me.ConstraintID
        tNewPrty.Condition = Me.Condition
        tNewPrty.InUse = Me.InUse
        tNewPrty.Priority = Me.Priority
        Return tNewPrty
    End Function
End Class

<Serializable()> Public Class RASolverPriorities

    Private mScenario As RAScenario = Nothing   ' D4360

    Public Priorities As List(Of RASolverPriority)

    Public Function GetSolverPriorityByID(tID As Integer) As RASolverPriority
        Return Priorities?.FirstOrDefault(Function(p) (p.ConstraintID = tID))
    End Function

    Public Function GetSolverPriorityByRank(tRank As Integer) As RASolverPriority
        Return Priorities?.FirstOrDefault(Function(p) (p.Rank = tRank))
    End Function

    Public Function GetSolverPriorityName(tID As Integer) As String
        Return GetSolverPriorityName(GetSolverPriorityByID(tID))
    End Function

    Public Function GetSolverPriorityName(tSolverPrty As RASolverPriority) As String
        If tSolverPrty IsNot Nothing Then
            If tSolverPrty.ConstraintID.Equals(RA_FUNDED_COST_ID) Then
                Return RA_FUNDED_COST_NAME
            Else
                Dim tCC As RAConstraint = GetConstraintByID(tSolverPrty)    ' D4362
                If tCC IsNot Nothing Then Return tCC.Name
            End If
            Return String.Format("#{0}", tSolverPrty.ConstraintID)
        End If
        Return ""
    End Function
    ' D4360 ==

    ' D4362 ===
    Public Function GetConstraintByID(tSolverPrty As RASolverPriority) As RAConstraint
        If tSolverPrty IsNot Nothing AndAlso mScenario IsNot Nothing Then Return mScenario.Constraints.GetConstraintByID(tSolverPrty.ConstraintID)
        Return Nothing
    End Function

    Public Function GetConstraintByID(tConstraintID As Integer) As RAConstraint
        Return mScenario?.Constraints.GetConstraintByID(tConstraintID)
        Return Nothing
    End Function

    Public Function CheckAndSort() As Boolean ' D4360
        Dim fUpdated As Boolean = False ' D4360
        If Priorities Is Nothing Then Priorities = New List(Of RASolverPriority)

        Dim tFunded As RASolverPriority = GetSolverPriorityByID(RA_FUNDED_COST_ID)  ' D4360
        If tFunded Is Nothing Then
            tFunded = New RASolverPriority()
            tFunded.Rank = 0
            tFunded.InUse = True
            tFunded.ConstraintID = RA_FUNDED_COST_ID
            tFunded.Condition = RASolverPriority.raSolverCondition.raMinimize
            Priorities.Insert(0, tFunded)
            fUpdated = True     ' D4360
        End If

        ' D4360 ===
        If mScenario IsNot Nothing AndAlso mScenario.Constraints.Constraints.Count > 0 Then
            Dim tMaxRank As Integer = 0
            For i As Integer = Priorities.Count - 1 To 0 Step -1
                Dim tPrty As RASolverPriority = Priorities(i)
                If tPrty.Rank > tMaxRank Then tMaxRank = tPrty.Rank
                If tPrty IsNot tFunded Then
                    Dim tCC As RAConstraint = GetConstraintByID(tPrty)  ' D4362
                    If tCC Is Nothing Then
                        Priorities.Remove(tPrty)
                        fUpdated = True     ' D4360
                    End If
                End If
            Next
            For Each tCID As Integer In mScenario.Constraints.Constraints.Keys
                Dim tPrty As RASolverPriority = GetSolverPriorityByID(tCID)
                If tPrty Is Nothing Then
                    tMaxRank += 1
                    tPrty = New RASolverPriority
                    tPrty.Rank = tMaxRank
                    tPrty.InUse = False
                    tPrty.ConstraintID = tCID
                    tPrty.Condition = RASolverPriority.raSolverCondition.raMinimize
                    Priorities.Add(tPrty)
                    fUpdated = True     ' D4360
                End If
            Next
        End If

        Priorities.Sort(New RASolverPriority_Comparer())
        ' D4360 ==

        Dim tMax As Integer = Priorities.Count
        For i As Integer = 1 To Priorities.Count
            ' D4360 ===
            Dim Prty As Double = (tMax + 1 - i) / tMax
            If Priorities(i - 1).Rank <> i OrElse Priorities(i - 1).Priority <> Prty Then
                Priorities(i - 1).Rank = i
                Priorities(i - 1).Priority = Prty
                fUpdated = True
                ' D4360 ==
            End If
        Next

        Return fUpdated ' D4360
    End Function

    Public Function Clone(Scenario As RAScenario) As RASolverPriorities
        Dim tNewList As New RASolverPriorities(Scenario)    ' D4360
        For Each tPrty As RASolverPriority In Priorities
            Dim tNewPrty As RASolverPriority = tPrty.Clone
            tNewList.Priorities.Add(tNewPrty)  ' D6724
        Next
        Return tNewList
    End Function

    Public Sub New(Scenario As RAScenario)  ' D4360
        mScenario = Scenario    ' D4360
        Priorities = New List(Of RASolverPriority)
    End Sub

End Class
