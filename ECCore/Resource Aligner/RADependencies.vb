Option Strict On    ' D2882

Imports System.Collections.Generic
Imports System.Linq

Namespace Canvas
    <Serializable()>
    Public Enum RADependencyType
        dtDependsOn = 0
        dtMutuallyDependent = 1
        dtMutuallyExclusive = 2
        dtConcurrent = 3 'A1160
        dtSuccessive = 4 'A1160
        dtLag = 5 'A1169
        '-A1179 dtDependsOnConcurrent = 6
        '-A1179 dtDependsOnSuccessive = 7
    End Enum

    <Serializable()>
    Public Enum LagCondition
        lcEqual = 0
        lcLessOrEqual = 1
        lcGreaterOrEqual = 2
        lcRange = 3
    End Enum

    <Serializable()>
    Public Class RADependency
        Public Property ID As Guid = Guid.NewGuid
        Public Property FirstAlternativeID As String
        Public Property SecondAlternativeID As String
        Public Property Value As RADependencyType

        Public Property LagCondition As LagCondition = LagCondition.lcEqual
        Public Property Lag As Integer = ECCore.UNDEFINED_INTEGER_VALUE
        Public Property LagUpperBound As Integer = ECCore.UNDEFINED_INTEGER_VALUE

        Public Property Enabled As Boolean

        Public Function Clone() As RADependency
            Dim tDep As RADependency = New RADependency(FirstAlternativeID, SecondAlternativeID, Value, Enabled)
            tDep.Lag = Lag
            tDep.LagCondition = LagCondition
            tDep.LagUpperBound = LagUpperBound
            tDep.ID = ID
            Return tDep
        End Function

        Public Sub New(FirstAltID As String, SecondAltID As String, Value As RADependencyType, Optional Enabled As Boolean = True)
            Me.FirstAlternativeID = FirstAltID
            Me.SecondAlternativeID = SecondAltID
            Me.Value = Value
            Me.Enabled = Enabled
        End Sub
    End Class

    <Serializable()>
    Public Class RADependencies
        Public Dependencies As List(Of RADependency)

        Public Function GetDependency(FirstAltID As String, SecondAltID As String) As RADependency
            For Each D As RADependency In Dependencies
                If FirstAltID = D.FirstAlternativeID And SecondAltID = D.SecondAlternativeID Then
                    Return D
                End If
            Next
            Return Nothing
        End Function

        Public Function SetDependency(FirstAltID As String, SecondAltID As String, Value As RADependencyType) As RADependency 'A1169
            Dim D As RADependency = GetDependency(FirstAltID, SecondAltID)
            If D Is Nothing Then
                D = New RADependency(FirstAltID, SecondAltID, Value)
                Dependencies.Add(D)
            Else
                D.Value = Value
            End If
            Return D
        End Function

        Public Function IsAlwaysInteger(AltID As String) As Boolean
            For Each dependency As RADependency In Dependencies
                Select Case dependency.Value
                    Case RADependencyType.dtDependsOn
                        Return False
                        If dependency.SecondAlternativeID.ToLower = AltID.ToLower Then
                            Return True
                        End If
                    Case RADependencyType.dtMutuallyDependent
                        Return False
                        If dependency.FirstAlternativeID.ToLower = AltID.ToLower Or dependency.SecondAlternativeID.ToLower = AltID.ToLower Then
                            Return True
                        End If
                    Case RADependencyType.dtMutuallyExclusive
                        Return False
                        If dependency.FirstAlternativeID.ToLower = AltID.ToLower Or dependency.SecondAlternativeID.ToLower = AltID.ToLower Then
                            Return True
                        End If
                End Select
            Next
            Return False
        End Function

        Public Function DeleteDependencyOneDirection(FirstAltID As String, SecondAltID As String) As Boolean 'A1416
            return Dependencies.RemoveAll(Function(d) (d.FirstAlternativeID = FirstAltID AndAlso d.SecondAlternativeID = SecondAltID)) > 0
        End Function

        Public Function DeleteDependency(FirstAltID As String, SecondAltID As String) As Integer
            Return Dependencies.RemoveAll(Function(d) ((d.FirstAlternativeID = FirstAltID AndAlso d.SecondAlternativeID = SecondAltID) OrElse (d.FirstAlternativeID = SecondAltID AndAlso d.SecondAlternativeID = FirstAltID)))
        End Function

        Public Function Clone() As RADependencies
            Dim tNewList As New RADependencies
            For Each tDep As RADependency In Dependencies
                tNewList.Dependencies.Add(tDep.Clone)
            Next
            Return tNewList
        End Function

        Public Function HasData(Alternatives As List(Of RAAlternative), Groups As RAGroups) As Boolean
            For Each tDep As RADependency In Dependencies
                Dim A1 As RAAlternative = Alternatives.Find(Function(x) x.ID = tDep.FirstAlternativeID)
                Dim A2 As RAAlternative = Alternatives.Find(Function(x) x.ID = tDep.SecondAlternativeID)
                Dim G1 As RAGroup = If(Groups.Groups.ContainsKey(tDep.FirstAlternativeID), Groups.Groups(tDep.FirstAlternativeID), Nothing)
                Dim G2 As RAGroup = If(Groups.Groups.ContainsKey(tDep.SecondAlternativeID), Groups.Groups(tDep.SecondAlternativeID), Nothing)
                If (A1 IsNot Nothing OrElse G1 IsNot Nothing) AndAlso (A2 IsNot Nothing OrElse G2 IsNot Nothing) Then Return True
            Next
            Return False
        End Function

        Public Sub New()
            Dependencies = New List(Of RADependency)
        End Sub
    End Class

End Namespace
