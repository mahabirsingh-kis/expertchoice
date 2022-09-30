Option Strict On

Imports System.Collections
Imports System.IO
Imports System.Linq
Imports ECCore

Namespace ECCore
    Public Class Contributions
        Public ReadOnly Property Hierarchy As clsHierarchy
        Public Sub New(Hierarchy As clsHierarchy)
            Me.Hierarchy = Hierarchy
        End Sub

        Public Property Contributions As New Dictionary(Of Guid, HashSet(Of Guid))

        Public Function IsUsingDefaultContribution() As Boolean
            Return Contributions.Count = 0
        End Function

        Public Sub SetContribution(CoveringObjectives As List(Of Guid), Alternatives As List(Of Guid), Values As List(Of Boolean))

        End Sub

        Public Function GetContributedAlternatives(CoveringObjectiveID As Guid) As HashSet(Of Guid)
            Return If(Contributions.ContainsKey(CoveringObjectiveID), Contributions(CoveringObjectiveID), New HashSet(Of Guid))
        End Function

        Public Function GetContributedObjectives(AlternativeID As Guid) As HashSet(Of Guid)
            Dim res As New HashSet(Of Guid)

            Return res
        End Function

        Public Sub Verify()
            ' removes broken entries for non-existent covering objectives and alternatives

            Dim objHS As HashSet(Of Guid) = Hierarchy.TerminalNodesGuidHashset
            Dim altHS As HashSet(Of Guid) = Hierarchy.ProjectManager.ActiveAlternatives.TerminalNodesGuidHashset

            'Dim hs As New HashSet(Of Guid)
            'For Each CovObjID As Guid In AlternativesByObjectives.Keys
            '    If Not objHS.Contains(CovObjID) Then
            '        hs.Add(CovObjID)
            '    End If
            'Next

            'For Each CovObjID As Guid In hs
            '    AlternativesByObjectives.Remove(CovObjID)
            'Next
        End Sub

    End Class

End Namespace
