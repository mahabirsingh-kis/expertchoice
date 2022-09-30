Option Strict On    ' D2882

Imports System.Collections.Generic
Imports System.Linq
Imports ECCore

Namespace Canvas
    <Serializable()>
    Public Class RAFundingPool
        Public Values As Dictionary(Of String, Double)
        Public AllocatedValues As Dictionary(Of String, Double)

        Public Priorities As Dictionary(Of String, Double)


        Public Property ID As Integer   ' D2909

        Private mName As String
        Public Property Name As String
            Get
                Return mName
            End Get
            Set(value As String)
                mName = value.Trim  ' D3251
            End Set
        End Property

        Public Property PoolLimit As Double

        Private mEnabled As Boolean = True
        Public Property Enabled As Boolean 'A0922 
            Get
                If RA_OPT_FPOOLS_ALLOW_ENABLED_PROPERTY Then Return mEnabled Else Return True ' D4367
            End Get
            Set(value As Boolean)
                mEnabled = value
            End Set
        End Property

        Public Function GetAlternativeValue(ID As String) As Double
            Return If(Values.ContainsKey(ID), Values(ID), UNDEFINED_INTEGER_VALUE)
        End Function

        ' D7092 ===
        Public Function GetAlternativePriority(ID As String) As Double
            Return If(Priorities.ContainsKey(ID), Priorities(ID), UNDEFINED_INTEGER_VALUE)
        End Function
        ' D7092 ==

        Public Function GetAlternativeAllocatedValue(ID As String) As Double
            Return If(AllocatedValues.ContainsKey(ID), AllocatedValues(ID), UNDEFINED_INTEGER_VALUE)
        End Function

        Public Sub SetAlternativeValue(ID As String, Value As Double)
            If Values.ContainsKey(ID) Then
                Values(ID) = Value
            Else
                Values.Add(ID, Value)
            End If
        End Sub

        ' D7092 ===
        Public Sub SetAlternativePriority(ID As String, Value As Double)
            If Value < 0 Then Value = 0 Else If Value > 1 Then Value = 1
            If Priorities.ContainsKey(ID) Then
                Priorities(ID) = Value
            Else
                Priorities.Add(ID, Value)
            End If
        End Sub
        ' D7092 ==

        Public Sub SetAlternativeAllocatedValue(ID As String, Value As Double)
            If AllocatedValues.ContainsKey(ID) Then
                AllocatedValues(ID) = Value
            Else
                AllocatedValues.Add(ID, Value)
            End If
        End Sub

        Public Sub ClearAllocatedValues()
            AllocatedValues.Clear()
        End Sub

        Public Function Clone() As RAFundingPool
            Dim tNewPool As New RAFundingPool(Name)
            tNewPool.ID = ID
            tNewPool.PoolLimit = PoolLimit
            For Each sKey As String In Values.Keys
                tNewPool.Values.Add(sKey, Values(sKey))
            Next
            Return tNewPool
        End Function

        Public Sub New(Optional Name As String = "")
            Values = New Dictionary(Of String, Double)
            Priorities = New Dictionary(Of String, Double)
            AllocatedValues = New Dictionary(Of String, Double)
            Me.Name = Name
        End Sub
    End Class

    <Serializable()>
    Public Class RAFundingPools
        Public Property ResourceID As Guid
        Public Pools As New Dictionary(Of Integer, RAFundingPool)
        Private mOrder As New List(Of Integer)

        Private _OPT_SEPARATOR As Char = CChar(",") ' D4367

        Public Function AddPool(Name As String) As RAFundingPool
            Dim FP As New RAFundingPool(Name)
            If Pools.Count = 0 Then FP.ID = 0 Else FP.ID = Pools.Max(Function(i) (i.Key)) + 1 ' D2909
            Pools.Add(FP.ID, FP)
            SetPoolsOrderByString("")       ' D4367
            Return FP
        End Function

        ' D2909 ===
        Public Function GetPoolByID(ID As Integer) As RAFundingPool
            If Pools.ContainsKey(ID) Then
                Return Pools(ID)
            Else
                Return Nothing
            End If
        End Function
        ' D2909 ==

        Public Function GetPoolByName(Name As String) As RAFundingPool
            Return Pools.Values.FirstOrDefault(Function(p) (p.Name.ToLower = Name.ToLower))
        End Function

        Public Function DeletePool(ID As Integer) As Boolean    ' D2909
            If Pools.ContainsKey(ID) Then
                Pools.Remove(ID)
                SetPoolsOrderByString("")       ' D4367
                Return True
            Else
                Return False
            End If
        End Function

        ' D4367 ===
        Public Function GetPoolsOrderAsList() As List(Of Integer)
            If mOrder Is Nothing OrElse mOrder.Count <> Pools.Count Then SetPoolsOrderByList(mOrder)    ' D7106
            Return mOrder
        End Function

        Public Function GetPoolsOrderAsString() As String
            Return String.Join(_OPT_SEPARATOR, GetPoolsOrderAsList)
        End Function

        Public Sub SetPoolsOrderByList(IDsList As List(Of Integer))
            Dim tNewLst As New List(Of Integer)
            If IDsList IsNot Nothing Then
                For Each tID As Integer In IDsList
                    If Not tNewLst.Contains(tID) AndAlso Pools.ContainsKey(tID) Then tNewLst.Add(tID)
                Next
            End If
            For Each tID As Integer In Pools.Keys
                If Not tNewLst.Contains(tID) Then tNewLst.Add(tID)
            Next
            mOrder = tNewLst
        End Sub

        Public Sub SetPoolsOrderByString(IDsList As String)
            Dim tLst As New List(Of Integer)
            If Not String.IsNullOrEmpty(IDsList) Then
                Dim ID As Integer
                Dim sIDs As String() = IDsList.Split(_OPT_SEPARATOR)
                For Each sID As String In sIDs
                    If Integer.TryParse(sID, ID) Then tLst.Add(ID)
                Next
            End If
            SetPoolsOrderByList(tLst)
        End Sub

        Public Function GetPoolsByOrder(SkipIgnored As Boolean) As List(Of RAFundingPool)
            If mOrder Is Nothing OrElse mOrder.Count <> Pools.Count Then SetPoolsOrderByList(mOrder)
            Dim tLst As New List(Of RAFundingPool)
            For Each tID As Integer In GetPoolsOrderAsList()
                If Pools.ContainsKey(tID) Then tLst.Add(Pools(tID))
            Next
            Return tLst
        End Function
        ' D4367 ==

        ' D3123 ===
        Public Function Clone() As RAFundingPools
            Dim tNewList As New RAFundingPools
            For Each tKey As Integer In Pools.Keys
                tNewList.Pools.Add(tKey, Pools(tKey).Clone)
            Next
            tNewList.mOrder = mOrder    ' D4367
            Return tNewList
        End Function
        ' D3123 ==

        Public Sub New()
            Pools = New Dictionary(Of Integer, RAFundingPool)   ' D2909
            mOrder = New List(Of Integer)   ' D4367
        End Sub

    End Class

End Namespace