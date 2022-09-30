Option Strict On    ' D2882

Imports Canvas
Imports System.Collections.Generic
Imports System.Linq

<Serializable()> Public Enum RAGroupCondition
    gcLessOrEqualsOne = 0
    gcEqualsOne = 1
    gcGreaterOrEqualsOne = 2
    gcAllOrNothing = 3
End Enum

<Serializable()> Public Class RAGroup
    <NonSerialized()>
    Public IntID As Integer

    Public Property Condition As RAGroupCondition
    Public Property Enabled As Boolean = True

    Public Alternatives As New Dictionary(Of String, RAAlternative)

    Public Property ID As String = ""

    Private mName As String = ""
    Public Property Name As String
        Get
            Return mName
        End Get
        Set(value As String)
            mName = value.Trim  ' D3251
        End Set
    End Property

    Public Function GetFilteredAlternatives(FilteredAlternatives As List(Of RAAlternative)) As List(Of RAAlternative)
        Return Alternatives.Values.Where(Function(a) (FilteredAlternatives.FirstOrDefault(Function(p) (p.ID.ToLower = a.ID.ToLower)) IsNot Nothing)).ToList
    End Function

    ' D3123 ===
    Public Function Clone(Optional tDestAlts As List(Of RAAlternative) = Nothing) As RAGroup
        Dim tNewGroup As New RAGroup()
        tNewGroup.Condition = Condition
        tNewGroup.Enabled = Enabled
        tNewGroup.ID = ID
        tNewGroup.IntID = IntID
        tNewGroup.Name = Name
        For Each sKey As String In Alternatives.Keys
            If tDestAlts Is Nothing Then
                tNewGroup.Alternatives.Add(sKey, Alternatives(sKey))
            Else
                For Each tAlt As RAAlternative In tDestAlts
                    If tAlt.ID = Alternatives(sKey).ID Then
                        tNewGroup.Alternatives.Add(sKey, tAlt)
                        Exit For
                    End If
                Next
            End If
        Next
        Return tNewGroup
    End Function

End Class

<Serializable()> Public Class RAGroups
    Public Groups As Dictionary(Of String, RAGroup)

    Public Function AddGroup(Optional ID As String = "", Optional Name As String = "") As RAGroup
        Dim newGroup As New RAGroup
        If ID = "" Then ID = Guid.NewGuid().ToString.ToLower
        If Name = "" Then Name = "New group " + (Groups.Count + 1).ToString
        newGroup.ID = ID
        newGroup.Name = Name
        Groups.Add(ID, newGroup)
        Return newGroup
    End Function

    Public Sub DeleteGroup(ID As String)
        Groups.Remove(ID)
    End Sub

    Public Function GetGroupsByCondition(Condition As RAGroupCondition) As List(Of RAGroup)
        Return Groups.Values.Where(Function(g) (g.Condition = Condition)).ToList
    End Function

    Public Function GetGroupByID(ID As String) As RAGroup
        Return Groups.Values.FirstOrDefault(Function(g) (g.ID = ID))
    End Function

    Public Function GetGroupByIntID(IntID As Integer) As RAGroup
        Return Groups.Values.FirstOrDefault(Function(g) (g.IntID = IntID))
    End Function

    Public Function GetGroupByName(Name As String) As RAGroup
        Return Groups.Values.FirstOrDefault(Function(g) (g.Name.Trim = Name.Trim))
    End Function

    Public Function Clone(Optional tDestAlts As List(Of RAAlternative) = Nothing) As RAGroups
        Dim tNewLst As New RAGroups()
        For Each sKey As String In Groups.Keys
            tNewLst.Groups.Add(sKey, Groups(sKey).Clone(tDestAlts))
        Next
        Return tNewLst
    End Function

    Public Function HasData(Alternatives As List(Of RAAlternative)) As Boolean
        For Each tGrp As KeyValuePair(Of String, RAGroup) In Groups
            For Each tAlt As KeyValuePair(Of String, RAAlternative) In tGrp.Value.Alternatives
                If tAlt.Value IsNot Nothing AndAlso Alternatives.Find(Function(x) x.ID = tAlt.Value.ID) IsNot Nothing Then Return True
            Next
        Next
        Return False
    End Function

    Public Sub New()
        Groups = New Dictionary(Of String, RAGroup)
    End Sub

End Class