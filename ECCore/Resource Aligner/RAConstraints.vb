Option Strict On    ' D2882

Imports ECCore
Imports Canvas
Imports System.Collections.Generic
Imports System.Linq

<Serializable()> Public Class RAConstraint
    Public Property ID As Integer

    Private mName As String = ""
    Public Property Name As String
        Get
            Return mName
        End Get
        Set(value As String)
            mName = value.Trim
        End Set
    End Property

    ' D6464 ===
    ReadOnly Property isDollarValue As Boolean
        Get
            Return RA_OPT_CC_ALLOW_DOLLAR_VALUES AndAlso IsLinked AndAlso Name.EndsWith("$") AndAlso Not LinkedEnumID.Equals(Guid.Empty)
        End Get
    End Property
    ' D6464 ==

    Public Property IsLinkedToResource As Boolean = False

    Private mECD_CCID As String = Integer.MinValue.ToString 'AS/11-4-15===
    Public Property ECD_CCID As String
        Get
            If mECD_CCID Is Nothing Then mECD_CCID = ""
            Return mECD_CCID
        End Get
        Set(value As String)
            mECD_CCID = value
        End Set
    End Property

    Private mECD_AssociatedUDcolKey As String = "" 'AS/11-10-15===
    Public Property ECD_AssociatedUDcolKey As String
        Get
            If mECD_AssociatedUDcolKey Is Nothing Then mECD_AssociatedUDcolKey = ""
            Return mECD_AssociatedUDcolKey
        End Get
        Set(value As String)
            mECD_AssociatedUDcolKey = value
        End Set
    End Property

    Private mECD_AssociatedCVID As String = "" 'AS/11-10-15===
    Public Property ECD_AssociatedCVID As String
        Get
            If mECD_AssociatedCVID Is Nothing Then mECD_AssociatedCVID = ""
            Return mECD_AssociatedCVID
        End Get
        Set(value As String)
            mECD_AssociatedCVID = value
        End Set
    End Property

    Private mECD_AID As String = "" 'AS/11-18-15===
    Public Property ECD_AID As String
        Get
            If mECD_AID Is Nothing Then mECD_AID = ""
            Return mECD_AID
        End Get
        Set(value As String)
            mECD_AID = value
        End Set
    End Property

    Public Property ECD_SOrder As Integer = 0 'AS/11-18-15

    Public Property Enabled As Boolean = True

    ' D3143 ===
    Public Property LinkedAttributeID As Guid = Guid.Empty

    Public Property LinkedEnumID As Guid = Guid.Empty

    Public Property IsReadOnly As Boolean = False

    Public ReadOnly Property IsLinked As Boolean
        Get
            Return Not Guid.Equals(LinkedEnumID, Guid.Empty)
        End Get
    End Property

    Public Property LinkSourceID As Guid = Guid.Empty

    Public Property LinkSourceMode As Integer

    Public Property MinValue As Double = UNDEFINED_INTEGER_VALUE
    Public ReadOnly Property MinValueSet As Boolean
        Get
            Return MinValue <> UNDEFINED_INTEGER_VALUE
        End Get
    End Property

    Public Property MaxValue As Double = UNDEFINED_INTEGER_VALUE
    Public ReadOnly Property MaxValueSet As Boolean
        Get
            Return MaxValue <> UNDEFINED_INTEGER_VALUE
        End Get
    End Property

    Public ReadOnly Property TotalCost(Alternatives As List(Of RAAlternative)) As Double    ' D2882 + D3884
        Get
            'A0961 ===
            Dim retVal As Double = 0
            If Alternatives IsNot Nothing Then
                ' -D3884
                'Dim vals As IEnumerable(Of Double) = AlternativesData.Values.Where(Function(v) v <> UNDEFINED_INTEGER_VALUE)
                'If vals.Count > 0 Then
                '    retVal = vals.Sum()
                'End If
                ' D3884 ===
                For Each tAlt As RAAlternative In Alternatives
                    Dim tVal As Double = GetAlternativeValue(tAlt.ID)
                    If tVal <> UNDEFINED_INTEGER_VALUE Then retVal += tVal
                Next
                ' D3884 ==
            End If
            'Return AlternativesData.Values.Sum()
            Return retVal
            'A0961
        End Get
    End Property

    Public AlternativesData As New Dictionary(Of String, Double)

    Public Function GetAlternativeValue(ID As String) As Double
        Return If(AlternativesData.ContainsKey(ID), AlternativesData(ID), UNDEFINED_INTEGER_VALUE)
    End Function

    ' D3123 ===
    Public Function Clone() As RAConstraint 'A0922
        Dim tNewConstr As New RAConstraint
        tNewConstr.ID = ID
        tNewConstr.Name = Name
        For Each sKey As String In AlternativesData.Keys
            tNewConstr.AlternativesData.Add(sKey, AlternativesData(sKey))
        Next
        tNewConstr.Enabled = Enabled
        tNewConstr.MaxValue = MaxValue
        tNewConstr.MinValue = MinValue
        ' D3143 ===
        tNewConstr.LinkedAttributeID = LinkedAttributeID
        tNewConstr.LinkedEnumID = LinkedEnumID
        tNewConstr.IsReadOnly = IsReadOnly
        ' D3143 ==
        ' D4478 ===
        tNewConstr.IsLinkedToResource = IsLinkedToResource
        tNewConstr.ECD_AID = ECD_AID
        tNewConstr.ECD_AssociatedCVID = ECD_AssociatedCVID
        tNewConstr.ECD_AssociatedUDcolKey = ECD_AssociatedUDcolKey
        tNewConstr.ECD_CCID = ECD_CCID
        tNewConstr.ECD_SOrder = ECD_SOrder
        ' D4478 ==
        Return tNewConstr
    End Function
    ' D3123 ==

    Public Sub New()
    End Sub
End Class

<Serializable()> Public Class RAConstraints
    Public ReadOnly Property Scenario As RAScenario
    Public Constraints As New Dictionary(Of Integer, RAConstraint)

    ' D7106 ===
    Public Function GetConstraintGuidID(ID As Integer) As Guid
        Dim r As RAResource = Scenario.TimePeriods.Resources.Values.FirstOrDefault(Function(x) x.ConstraintID = ID)
        Return If(r Is Nothing, Guid.Empty, r.ID)   ' D7107
    End Function
    ' D7106 ==

    Public Function GetConstraintGuidID(constraint As RAConstraint) As Guid
        Dim r As RAResource = Scenario.TimePeriods.Resources.Values.FirstOrDefault(Function(x) x.ConstraintID = constraint.ID)
        Return If(r Is Nothing, Guid.Empty, r.ID)   ' D7107
    End Function

    Public Function AddConstraint(CCID As String, Name As String) As RAConstraint 'AS/11-4-15
        Dim newConstraint As New RAConstraint
        If Constraints.Count = 0 Then newConstraint.ID = 0 Else newConstraint.ID = Constraints.Max(Function(i) (i.Key)) + 1
        newConstraint.ECD_CCID = CCID 'AS/11-4-15
        newConstraint.Name = Name   ' D2882
        Constraints.Add(newConstraint.ID, newConstraint)
        If Scenario IsNot Nothing Then Scenario.SolverPriorities.CheckAndSort() ' D4365
        Return newConstraint
    End Function

    Public Function DeleteConstraint(ID As Integer) As Boolean  ' D2882
        If Constraints.ContainsKey(ID) Then
            Constraints.Remove(ID)
            If Scenario IsNot Nothing Then Scenario.SolverPriorities.CheckAndSort() ' D4365
            Dim resource As RAResource = Scenario.TimePeriods.Resources.FirstOrDefault(Function(r) (r.Value.ConstraintID = ID)).Value
            If resource IsNot Nothing Then
                Scenario.TimePeriods.Resources.Remove(resource.ID)
            End If
            Return True
        Else
            Return False
        End If
    End Function

    Public Function GetConstraintByID(ID As Integer) As RAConstraint
        Return If(Constraints.ContainsKey(ID), Constraints(ID), Nothing)
    End Function

    Public Function GetConstraintByName(Name As String) As RAConstraint
        Return Constraints.Values.FirstOrDefault(Function(c) (c.Name.Trim.ToLower = Name.Trim.ToLower))
    End Function

    Public Function GetConstraintByCCID(CCID As String) As RAConstraint 'AS/11-4-15
        Return Constraints.Values.FirstOrDefault(Function(c) (c.ECD_CCID = CCID))
    End Function

    Public Function GetConstraintValue(ConstraintID As Integer, AlternativeID As String) As Double
        Dim constraint As RAConstraint = GetConstraintByID(ConstraintID)
        Return If(constraint Is Nothing, UNDEFINED_INTEGER_VALUE, constraint.GetAlternativeValue(AlternativeID))
    End Function

    Public Function SetConstraintValue(ConstraintID As Integer, AlternativeID As String, Value As Double) As Boolean
        Dim constraint As RAConstraint = GetConstraintByID(ConstraintID)
        If constraint Is Nothing Then Return False

        If constraint.AlternativesData.ContainsKey(AlternativeID) Then
            If Value <> UNDEFINED_INTEGER_VALUE Then
                constraint.AlternativesData(AlternativeID) = Value
            Else
                constraint.AlternativesData.Remove(AlternativeID)
            End If
        Else
            If Value <> UNDEFINED_INTEGER_VALUE Then
                constraint.AlternativesData.Add(AlternativeID, Value)
            End If
        End If
        Return True
    End Function

    Public Function Clone(Scenario As RAScenario) As RAConstraints
        Dim tNewList As New RAConstraints(Scenario)
        For Each tConstID As Integer In Constraints.Keys
            Dim tNewConstr As RAConstraint = Constraints(tConstID).Clone
            tNewList.Constraints.Add(tConstID, tNewConstr)
        Next
        Return tNewList
    End Function

    Public Sub New(Scenario As RAScenario)
        Me.Scenario = Scenario
    End Sub

End Class