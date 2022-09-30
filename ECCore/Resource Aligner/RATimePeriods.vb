Option Strict On

Imports System.Collections.Generic
Imports System.Linq
Imports ECCore
Imports System.IO
Imports System.Data.Common

Namespace Canvas
    <Serializable()>
    Public Class RAResource
        Public Property ID As Guid = Guid.NewGuid
        Public Property Name As String = ""
        Public Property Comment As String = ""
        Public Property ConstraintID As Integer = -1
        Public Property Enabled As Boolean = True

        Public Function Clone() As RAResource
            Dim tNew As New RAResource
            tNew.ID = New Guid(Me.ID.ToString)
            tNew.Name = Me.Name
            tNew.Comment = Me.Comment
            tNew.ConstraintID = Me.ConstraintID
            tNew.Enabled = Me.Enabled
            Return tNew
        End Function
    End Class

    <Serializable()>
    Public Class RATimePeriod
        Private mName As String = ""

        Public Property ShortName As String = ""
        Public Property Comment As String = ""
        Public Property ID As Integer

        Public Property TimePeriods As RATimePeriods

        Friend Property ResourceMaxValues As New Dictionary(Of Guid, Double)    ' D3918
        Friend Property ResourceMinValues As New Dictionary(Of Guid, Double)    ' D3918

        Public Property Name As String
            Get
                If mName = "" Then
                    Return TimePeriods.NamePrefix + TimePeriods.Periods.IndexOf(Me).ToString
                Else
                    Return mName
                End If
            End Get
            Set(value As String)
                mName = value
            End Set
        End Property

        Public Function GetResourceMaxValue(ResourceID As Guid) As Double
            Return If(ResourceMaxValues.ContainsKey(ResourceID), ResourceMaxValues(ResourceID), UNDEFINED_INTEGER_VALUE)
        End Function

        Public Function GetResourceMinValue(ResourceID As Guid) As Double
            Return If(ResourceMinValues.ContainsKey(ResourceID), ResourceMinValues(ResourceID), UNDEFINED_INTEGER_VALUE)
        End Function

        Public Sub SetResourceMaxValue(ResourceID As Guid, Value As Double)
            If Value = UNDEFINED_INTEGER_VALUE Then
                ResourceMaxValues.Remove(ResourceID)
            Else
                If ResourceMaxValues.ContainsKey(ResourceID) Then
                    ResourceMaxValues(ResourceID) = Value
                Else
                    ResourceMaxValues.Add(ResourceID, Value)
                End If
            End If
        End Sub

        Public Sub SetResourceMinValue(ResourceID As Guid, Value As Double)
            If Value = UNDEFINED_INTEGER_VALUE Then
                ResourceMinValues.Remove(ResourceID)
            Else
                If ResourceMinValues.ContainsKey(ResourceID) Then
                    ResourceMinValues(ResourceID) = Value
                Else
                    ResourceMinValues.Add(ResourceID, Value)
                End If
            End If
        End Sub

        Public Sub New(TimePeriods As RATimePeriods)
            Me.TimePeriods = TimePeriods
        End Sub

        Public Function Clone(aTimePeriods As RATimePeriods) As RATimePeriod
            Dim tNew As New RATimePeriod(aTimePeriods)
            tNew.Name = Me.Name
            tNew.ShortName = Me.ShortName
            tNew.Comment = Me.Comment
            tNew.ID = Me.ID
            tNew.ResourceMaxValues = New Dictionary(Of Guid, Double)    ' D3918
            For Each sKey As Guid In Me.ResourceMaxValues.Keys  ' D3918
                tNew.ResourceMaxValues.Add(sKey, Me.ResourceMaxValues(sKey))
            Next
            tNew.ResourceMinValues = New Dictionary(Of Guid, Double)    ' D3918
            For Each sKey As Guid In Me.ResourceMinValues.Keys  ' D3918
                tNew.ResourceMinValues.Add(sKey, Me.ResourceMinValues(sKey))
            Next
            Return tNew
        End Function
    End Class

    <Serializable()>
    Public Enum TimePeriodsType
        tptDay = 0
        tptWeek = 1
        tptMonth = 2
        tptQuarter = 3
        tptYear = 4
        tptCustom = 5
    End Enum

    <Serializable()>
    Public Class AlternativePeriodsData
        Public Property Duration As Integer = 1
        Public Property StartPeriod As Integer = 0

        Public Property MinPeriod As Integer = 0
        Public Function GetMinPeriod() As Integer
            Return If(HasMust AndAlso MustPeriod <> UNDEFINED_INTEGER_VALUE, MinPeriod + MustPeriod, MinPeriod)
        End Function

        Public Property MaxPeriod As Integer = 0
        Public Function GetMaxPeriod() As Integer
            Return If(HasMust AndAlso MustPeriod <> UNDEFINED_INTEGER_VALUE, MinPeriod + MustPeriod, MaxPeriod)
        End Function

        Private mMustPeriod As Integer = UNDEFINED_INTEGER_VALUE
        Public Property MustPeriod As Integer
            Get
                If mMustPeriod > MaxPeriod - MinPeriod Then mMustPeriod = MaxPeriod - MinPeriod
                Return mMustPeriod
            End Get
            Set(value As Integer)
                mMustPeriod = value
            End Set
        End Property

        Public Property HasMust As Boolean = False
        ' dictionary of TimePeriodID, ResourceID
        Public Property ResourceData As New Dictionary(Of Integer, Dictionary(Of Guid, Double)) ' D3918
    End Class

    <Serializable()>
    Public Class RAPeriodsData
        Friend Property PeriodsData As New Dictionary(Of String, AlternativePeriodsData)
        Private Const DefaultValue As Double = UNDEFINED_INTEGER_VALUE

        Public Function GetAlternativePeriodsData(AlternativeID As String) As AlternativePeriodsData
            If Not PeriodsData.ContainsKey(AlternativeID) Then
                PeriodsData.Add(AlternativeID, New AlternativePeriodsData)
            End If
            Return PeriodsData(AlternativeID)
        End Function

        Public Sub SetDuration(AlternativeID As String, Duration As Integer)
            Dim APD As AlternativePeriodsData = GetAlternativePeriodsData(AlternativeID)
            APD.Duration = Duration
        End Sub

        Public Sub SetStartPeriod(AlternativeID As String, StartPeriod As Integer)
            Dim APD As AlternativePeriodsData = GetAlternativePeriodsData(AlternativeID)
            APD.StartPeriod = StartPeriod
        End Sub

        Public Sub SetMinPeriod(AlternativeID As String, MinPeriod As Integer)
            Dim APD As AlternativePeriodsData = GetAlternativePeriodsData(AlternativeID)
            APD.MinPeriod = MinPeriod
        End Sub

        Public Sub SetMaxPeriod(AlternativeID As String, MaxPeriod As Integer)
            Dim APD As AlternativePeriodsData = GetAlternativePeriodsData(AlternativeID)
            APD.MaxPeriod = MaxPeriod
        End Sub

        Public Sub SetAlternativePeriodData(AlternativeID As String, PeriodData As AlternativePeriodsData)
            If PeriodsData.ContainsKey(AlternativeID) Then
                PeriodsData(AlternativeID) = PeriodData
            Else
                PeriodsData.Add(AlternativeID, PeriodData)
            End If
        End Sub

        Public Function SetResourceValue(PeriodID As Integer, AlternativeID As String, ResourceID As Guid, Value As Double) As Boolean    ' D3918
            Dim APD As AlternativePeriodsData = GetAlternativePeriodsData(AlternativeID)
            If APD.ResourceData.ContainsKey(PeriodID) Then
                'If Value = UNDEFINED_INTEGER_VALUE Then
                '    APD.ResourceData(PeriodID).Remove(ResourceID)
                'Else
                If APD.ResourceData(PeriodID).ContainsKey(ResourceID) Then
                    APD.ResourceData(PeriodID)(ResourceID) = Value
                Else
                    APD.ResourceData(PeriodID).Add(ResourceID, Value)
                End If
                'End If
            Else
                Dim newRData As New Dictionary(Of Guid, Double) ' D3918
                newRData.Add(ResourceID, Value)
                APD.ResourceData.Add(PeriodID, newRData)
            End If
            Return True
        End Function

        Public Function GetResourceValue(PeriodID As Integer, AlternativeID As String, ResourceID As Guid) As Double  ' D3918
            Dim APD As AlternativePeriodsData = GetAlternativePeriodsData(AlternativeID)
            If APD Is Nothing Then Return DefaultValue
            If Not APD.ResourceData.ContainsKey(PeriodID) Then Return DefaultValue
            If Not APD.ResourceData(PeriodID).ContainsKey(ResourceID) Then Return DefaultValue
            Return APD.ResourceData(PeriodID)(ResourceID)
        End Function

        ' D4290 ===
        Public Function Clone(sKey As String) As AlternativePeriodsData
            Dim tNew As AlternativePeriodsData = Nothing
            Dim tData As AlternativePeriodsData = PeriodsData(sKey)
            If tData IsNot Nothing Then
                tNew = New AlternativePeriodsData
                tNew.Duration = tData.Duration
                tNew.HasMust = tData.HasMust
                tNew.MinPeriod = tData.MinPeriod
                tNew.MaxPeriod = tData.MaxPeriod
                tNew.MustPeriod = tData.MustPeriod
                tNew.StartPeriod = tData.StartPeriod
                tNew.ResourceData = New Dictionary(Of Integer, Dictionary(Of Guid, Double))
                For Each tKey As Integer In tData.ResourceData.Keys
                    Dim tVals As New Dictionary(Of Guid, Double)
                    For Each tPair As KeyValuePair(Of Guid, Double) In tData.ResourceData(tKey)
                        tVals.Add(New Guid(tPair.Key.ToString), tPair.Value)
                    Next
                    tNew.ResourceData.Add(tKey, tVals)
                Next
            End If
            Return tNew
        End Function
        ' D4290 ==

    End Class

    <Serializable()>
    Public Class RATimePeriods
        Public Property Periods As New List(Of RATimePeriod)
        Public Property NamePrefix As String = "Period #"
        Public Property PeriodsType As TimePeriodsType = TimePeriodsType.tptYear
        Public Property PeriodsStep As Integer = 1
        Public Property TimelineStartDate As Date
        Public Property UseDiscountFactor As Boolean = True ' D3975
        Public Property DiscountFactor As Double = 0.00001  ' D4086

        Public Property MainResourceID As Guid = RA_Cost_GUID

        Public Property PeriodsData As New RAPeriodsData

        Public Property Resources As New Dictionary(Of Guid, RAResource)    ' D3918

        Public ReadOnly Property Scenario As RAScenario

        Public ReadOnly Property EnabledResources As List(Of RAResource)
            Get
                Return Resources.Select(Function(r) (r.Value)).Where(Function(x) (x.Enabled)).ToList()
            End Get
        End Property

        Public Sub LinkResourcesFromConstraints()
            For Each constraint As RAConstraint In Scenario.Constraints.Constraints.Values
                Dim resource As RAResource = Resources.FirstOrDefault(Function(r) (r.Value.ConstraintID = constraint.ID)).Value
                If constraint.IsLinkedToResource Then
                    If resource Is Nothing Then
                        resource = AddResource(constraint.Name)
                        resource.ConstraintID = constraint.ID
                    Else
                        resource.Name = constraint.Name
                        resource.Enabled = True
                    End If
                Else
                    If resource IsNot Nothing Then
                        resource.Enabled = False
                    End If
                End If
            Next

            Dim invalidIDs As New List(Of Guid)
            For Each resource As RAResource In Resources.Values
                If Not resource.ID.Equals(RA_Cost_GUID) AndAlso Scenario.Constraints.GetConstraintByID(resource.ConstraintID) Is Nothing Then
                    invalidIDs.Add(resource.ID)
                End If
            Next
            For Each id As Guid In invalidIDs
                Resources.Remove(id)
            Next
            If Not Resources.ContainsKey(RA_Cost_GUID) Then
                AddCostResource()
            End If
        End Sub

        Public Function AddResource(name As String) As RAResource
            Dim r As New RAResource
            r.Name = name
            Resources.Add(r.ID, r)
            Return r
        End Function

        Public Function DeleteResource(id As Guid) As Boolean   ' D3918
            If Resources.ContainsKey(id) Then   ' D3918
                Resources.Remove(id)    ' D3918
                For Each alt As RAAlternative In Scenario.AlternativesFull
                    Dim APD As AlternativePeriodsData = PeriodsData.GetAlternativePeriodsData(alt.ID)
                    If APD IsNot Nothing Then
                        For Each period As RATimePeriod In Periods
                            If APD.ResourceData(period.ID).ContainsKey(id) Then
                                APD.ResourceData(period.ID).Remove(id)
                            End If
                        Next
                    End If
                Next
                Return True
            End If
            Return False
        End Function

        Private Function GetMaxID() As Integer
            Dim id As Integer = -1
            For Each period As RATimePeriod In Periods
                If period.ID > id Then id = period.ID
            Next
            Return id
        End Function

        Public Function AddPeriod() As RATimePeriod
            Dim res As New RATimePeriod(Me)
            res.ID = GetMaxID() + 1
            Periods.Add(res)
            Return res
        End Function

        Public Function GetPeriod(PeriodID As Integer) As RATimePeriod
            For i As Integer = 0 To Periods.Count - 1
                If Periods(i).ID = PeriodID Then
                    Return Periods(i)
                End If
            Next
            Return Nothing
        End Function

        ' D3905 ===
        Public Function GetPeriodName(id As Integer, Optional ShortName As Boolean = False) As String
            Dim DT As DateTime = TimelineStartDate
            Dim sName As String = ""
            Select Case PeriodsType
                Case TimePeriodsType.tptCustom
                    sName = CStr(If(NamePrefix.Contains("#"), NamePrefix.Replace("#", CStr(id + 1)), String.Format("{0}{1}", NamePrefix, id + 1)))
                Case TimePeriodsType.tptDay
                    sName = DT.AddDays(id).ToString(CStr(If(ShortName, "yy-MM-dd", "yyyy-MM-dd")))
                Case TimePeriodsType.tptMonth
                    sName = DT.AddMonths(id).ToString(CStr(If(ShortName, "MMM yy", "MMMM yyyy")))
                Case TimePeriodsType.tptQuarter
                    sName = String.Format(CStr(If(ShortName, "Q{1}", "{0} (Q{1})")), DT.AddMonths(3 * id).ToString("MMMM yyyy"), id + 1)
                Case TimePeriodsType.tptWeek
                    sName = String.Format(CStr(If(ShortName, "Week {1}", "{0} (Week {1})")), DT.AddDays(7 * id).ToShortDateString, id + 1)
                Case TimePeriodsType.tptYear
                    sName = DT.AddYears(id).ToString("yyyy")
            End Select
            Return sName
        End Function
        ' D3905 ==

        Public Function DeletePeriod(PeriodID As Integer) As Boolean
            Dim tp As RATimePeriod = GetPeriod(PeriodID)
            If tp IsNot Nothing Then
                Periods.Remove(tp)
                Return True
            Else
                Return False
            End If
        End Function

        Public Sub ParseStream(Stream As MemoryStream)
            Periods.Clear()
            PeriodsData.PeriodsData.Clear()
            Resources.Clear()

            Stream.Seek(0, SeekOrigin.Begin)

            Dim BR As New BinaryReader(Stream)

            NamePrefix = BR.ReadString
            PeriodsType = CType(BR.ReadInt32, TimePeriodsType)
            PeriodsStep = BR.ReadInt32
            TimelineStartDate = DateTime.FromBinary(BR.ReadInt64)

            Dim PeriodsCount As Integer = BR.ReadInt32
            For i As Integer = 1 To PeriodsCount
                Dim period As New RATimePeriod(Me)
                period.ID = BR.ReadInt32
                period.Name = BR.ReadString
                period.ShortName = BR.ReadString
                period.Comment = BR.ReadString
                Periods.Add(period)

                Dim count As Integer = BR.ReadInt32
                For j As Integer = 1 To count
                    Dim key As New Guid(BR.ReadString)   ' D3918
                    Dim value As Double = BR.ReadDouble
                    period.ResourceMaxValues.Add(key, value)
                Next
            Next

            Dim rCount As Integer = BR.ReadInt32
            For i As Integer = 1 To rCount
                Dim resource As New RAResource
                resource.ID = New Guid(BR.ReadString)  ' D3918
                resource.Name = BR.ReadString
                resource.Comment = BR.ReadString
                Resources.Add(resource.ID, resource)
            Next

            Dim PeriodsDataCount As Integer = BR.ReadInt32
            For i As Integer = 1 To PeriodsDataCount
                Dim PeriodData As New AlternativePeriodsData
                Dim id As String = BR.ReadString
                PeriodData.Duration = BR.ReadInt32
                PeriodData.StartPeriod = BR.ReadInt32
                PeriodData.MinPeriod = BR.ReadInt32
                PeriodData.MaxPeriod = BR.ReadInt32
                PeriodsData.PeriodsData.Add(id, PeriodData)

                Dim tpDataCount As Integer = BR.ReadInt32
                For j As Integer = 1 To tpDataCount
                    Dim tpID As Integer = BR.ReadInt32
                    Dim tpCount As Integer = BR.ReadInt32

                    Dim newRdata As New Dictionary(Of Guid, Double) ' D3918
                    For k As Integer = 1 To tpCount
                        Dim resourceID As New Guid(BR.ReadString)    ' D3918
                        Dim resourceValue As Double = BR.ReadDouble
                        newRdata.Add(resourceID, resourceValue)
                    Next

                    PeriodData.ResourceData.Add(tpID, newRdata)
                Next
            Next

            BR.Close()
        End Sub

        Public Sub ParseStream_v_37(Stream As MemoryStream)
            Periods.Clear()
            PeriodsData.PeriodsData.Clear()
            Resources.Clear()

            Stream.Seek(0, SeekOrigin.Begin)

            Dim BR As New BinaryReader(Stream)

            NamePrefix = BR.ReadString
            PeriodsType = CType(BR.ReadInt32, TimePeriodsType)
            PeriodsStep = BR.ReadInt32
            TimelineStartDate = DateTime.FromBinary(BR.ReadInt64)

            Dim PeriodsCount As Integer = BR.ReadInt32
            For i As Integer = 1 To PeriodsCount
                Dim period As New RATimePeriod(Me)
                period.ID = BR.ReadInt32
                period.Name = BR.ReadString
                period.ShortName = BR.ReadString
                period.Comment = BR.ReadString
                Periods.Add(period)

                Dim count As Integer = BR.ReadInt32
                For j As Integer = 1 To count
                    Dim key As New Guid(BR.ReadString)   ' D3918
                    Dim value As Double = BR.ReadDouble
                    period.ResourceMaxValues.Add(key, value)
                Next

                count = BR.ReadInt32
                For j As Integer = 1 To count
                    Dim key As New Guid(BR.ReadString)  ' D3918
                    Dim value As Double = BR.ReadDouble
                    period.ResourceMinValues.Add(key, value)
                Next
            Next

            Dim rCount As Integer = BR.ReadInt32
            For i As Integer = 1 To rCount
                Dim resource As New RAResource
                resource.ID = New Guid(BR.ReadString)  ' D3918
                resource.Name = BR.ReadString
                resource.Comment = BR.ReadString
                Resources.Add(resource.ID, resource)
            Next

            Dim PeriodsDataCount As Integer = BR.ReadInt32
            For i As Integer = 1 To PeriodsDataCount
                Dim PeriodData As New AlternativePeriodsData
                Dim id As String = BR.ReadString
                PeriodData.Duration = BR.ReadInt32
                PeriodData.StartPeriod = BR.ReadInt32
                PeriodData.MinPeriod = BR.ReadInt32
                PeriodData.MaxPeriod = BR.ReadInt32
                PeriodsData.PeriodsData.Add(id, PeriodData)

                Dim tpDataCount As Integer = BR.ReadInt32
                For j As Integer = 1 To tpDataCount
                    Dim tpID As Integer = BR.ReadInt32
                    Dim tpCount As Integer = BR.ReadInt32

                    Dim newRdata As New Dictionary(Of Guid, Double) ' D3918
                    For k As Integer = 1 To tpCount
                        Dim resourceID As New Guid(BR.ReadString)   ' D3918
                        Dim resourceValue As Double = BR.ReadDouble
                        newRdata.Add(resourceID, resourceValue)
                    Next

                    PeriodData.ResourceData.Add(tpID, newRdata)
                Next
            Next

            BR.Close()
        End Sub

        Public Sub ParseStream_v_39(Stream As MemoryStream)
            Periods.Clear()
            PeriodsData.PeriodsData.Clear()
            Resources.Clear()

            Stream.Seek(0, SeekOrigin.Begin)

            Dim BR As New BinaryReader(Stream)

            NamePrefix = BR.ReadString
            PeriodsType = CType(BR.ReadInt32, TimePeriodsType)
            PeriodsStep = BR.ReadInt32
            TimelineStartDate = DateTime.FromBinary(BR.ReadInt64)
            Dim DF As Double = BR.ReadDouble    ' D4479
            If DF > 0 Then DiscountFactor = DF ' D4479
            UseDiscountFactor = BR.ReadBoolean

            Dim PeriodsCount As Integer = BR.ReadInt32
            For i As Integer = 1 To PeriodsCount
                Dim period As New RATimePeriod(Me)
                period.ID = BR.ReadInt32
                period.Name = BR.ReadString
                period.ShortName = BR.ReadString
                period.Comment = BR.ReadString
                If GetPeriod(period.ID) IsNot Nothing Then
                    period.ID = GetMaxID() + 1
                End If
                Periods.Add(period)

                Dim count As Integer = BR.ReadInt32
                For j As Integer = 1 To count
                    Dim key As New Guid(BR.ReadString) ' D3918
                    Dim value As Double = BR.ReadDouble
                    period.ResourceMaxValues.Add(key, value)
                Next

                count = BR.ReadInt32
                For j As Integer = 1 To count
                    Dim key As New Guid(BR.ReadString)  ' D3918
                    Dim value As Double = BR.ReadDouble
                    period.ResourceMinValues.Add(key, value)
                Next
            Next

            Dim rCount As Integer = BR.ReadInt32
            For i As Integer = 1 To rCount
                Dim resource As New RAResource
                resource.ID = New Guid(BR.ReadString)    ' D3918
                resource.Name = BR.ReadString
                resource.Comment = BR.ReadString

                ' v1.1.39
                resource.Enabled = BR.ReadBoolean
                resource.ConstraintID = BR.ReadInt32

                Resources.Add(resource.ID, resource)
            Next

            Dim PeriodsDataCount As Integer = BR.ReadInt32
            For i As Integer = 1 To PeriodsDataCount
                Dim PeriodData As New AlternativePeriodsData
                Dim id As String = BR.ReadString
                PeriodData.Duration = BR.ReadInt32
                PeriodData.StartPeriod = BR.ReadInt32
                PeriodData.MinPeriod = BR.ReadInt32
                PeriodData.MaxPeriod = BR.ReadInt32
                PeriodsData.PeriodsData.Add(id, PeriodData)

                Dim tpDataCount As Integer = BR.ReadInt32
                For j As Integer = 1 To tpDataCount
                    Dim tpID As Integer = BR.ReadInt32
                    Dim tpCount As Integer = BR.ReadInt32

                    Dim newRdata As New Dictionary(Of Guid, Double) ' D3918
                    For k As Integer = 1 To tpCount
                        Dim resourceID As New Guid(BR.ReadString)   ' D3918
                        Dim resourceValue As Double = BR.ReadDouble
                        newRdata.Add(resourceID, resourceValue)
                    Next

                    PeriodData.ResourceData.Add(tpID, newRdata)
                Next
            Next

            BR.Close()
        End Sub

        Public Sub ParseStream_v_40(Stream As MemoryStream)
            Periods.Clear()
            PeriodsData.PeriodsData.Clear()
            Resources.Clear()

            Stream.Seek(0, SeekOrigin.Begin)

            Dim BR As New BinaryReader(Stream)

            NamePrefix = BR.ReadString
            PeriodsType = CType(BR.ReadInt32, TimePeriodsType)
            PeriodsStep = BR.ReadInt32
            TimelineStartDate = DateTime.FromBinary(BR.ReadInt64)
            Dim DF As Double = BR.ReadDouble    ' D4479
            If DF > 0 Then DiscountFactor = DF ' D4479
            UseDiscountFactor = BR.ReadBoolean

            Dim PeriodsCount As Integer = BR.ReadInt32
            For i As Integer = 1 To PeriodsCount
                Dim period As New RATimePeriod(Me)
                period.ID = BR.ReadInt32
                period.Name = BR.ReadString
                period.ShortName = BR.ReadString
                period.Comment = BR.ReadString
                If GetPeriod(period.ID) IsNot Nothing Then
                    period.ID = GetMaxID() + 1
                End If
                Periods.Add(period)

                Dim count As Integer = BR.ReadInt32
                For j As Integer = 1 To count
                    Dim key As New Guid(BR.ReadString) ' D3918
                    Dim value As Double = BR.ReadDouble
                    period.ResourceMaxValues.Add(key, value)
                Next

                count = BR.ReadInt32
                For j As Integer = 1 To count
                    Dim key As New Guid(BR.ReadString)  ' D3918
                    Dim value As Double = BR.ReadDouble
                    period.ResourceMinValues.Add(key, value)
                Next
            Next

            Dim rCount As Integer = BR.ReadInt32
            For i As Integer = 1 To rCount
                Dim resource As New RAResource
                resource.ID = New Guid(BR.ReadString)    ' D3918
                resource.Name = BR.ReadString
                resource.Comment = BR.ReadString

                ' v1.1.39
                resource.Enabled = BR.ReadBoolean
                resource.ConstraintID = BR.ReadInt32

                Resources.Add(resource.ID, resource)
            Next

            Dim PeriodsDataCount As Integer = BR.ReadInt32
            For i As Integer = 1 To PeriodsDataCount
                Dim PeriodData As New AlternativePeriodsData
                Dim id As String = BR.ReadString
                PeriodData.Duration = BR.ReadInt32
                PeriodData.StartPeriod = BR.ReadInt32
                PeriodData.MinPeriod = BR.ReadInt32
                PeriodData.MaxPeriod = BR.ReadInt32

                ' new to 1.1.40
                PeriodData.HasMust = BR.ReadBoolean
                PeriodData.MustPeriod = BR.ReadInt32

                PeriodsData.PeriodsData.Add(id, PeriodData)

                Dim tpDataCount As Integer = BR.ReadInt32
                For j As Integer = 1 To tpDataCount
                    Dim tpID As Integer = BR.ReadInt32
                    Dim tpCount As Integer = BR.ReadInt32

                    Dim newRdata As New Dictionary(Of Guid, Double) ' D3918
                    For k As Integer = 1 To tpCount
                        Dim resourceID As New Guid(BR.ReadString)   ' D3918
                        Dim resourceValue As Double = BR.ReadDouble
                        if Not newRdata.ContainsKey(resourceID) then
                            newRdata.Add(resourceID, resourceValue)
                        End If
                    Next

                    PeriodData.ResourceData.Add(tpID, newRdata)
                Next
            Next

            BR.Close()
        End Sub

        Public Sub ParseStream_v_38(Stream As MemoryStream)
            Periods.Clear()
            PeriodsData.PeriodsData.Clear()
            Resources.Clear()

            Stream.Seek(0, SeekOrigin.Begin)

            Dim BR As New BinaryReader(Stream)

            NamePrefix = BR.ReadString
            PeriodsType = CType(BR.ReadInt32, TimePeriodsType)
            PeriodsStep = BR.ReadInt32
            TimelineStartDate = DateTime.FromBinary(BR.ReadInt64)
            DiscountFactor = BR.ReadDouble
            UseDiscountFactor = BR.ReadBoolean

            Dim PeriodsCount As Integer = BR.ReadInt32
            For i As Integer = 1 To PeriodsCount
                Dim period As New RATimePeriod(Me)
                period.ID = BR.ReadInt32
                period.Name = BR.ReadString
                period.ShortName = BR.ReadString
                period.Comment = BR.ReadString
                Periods.Add(period)

                Dim count As Integer = BR.ReadInt32
                For j As Integer = 1 To count
                    Dim key As New Guid(BR.ReadString) ' D3918
                    Dim value As Double = BR.ReadDouble
                    period.ResourceMaxValues.Add(key, value)
                Next

                count = BR.ReadInt32
                For j As Integer = 1 To count
                    Dim key As New Guid(BR.ReadString) ' D3918
                    Dim value As Double = BR.ReadDouble
                    period.ResourceMinValues.Add(key, value)
                Next
            Next

            Dim rCount As Integer = BR.ReadInt32
            For i As Integer = 1 To rCount
                Dim resource As New RAResource
                resource.ID = New Guid(BR.ReadString) ' D3918
                resource.Name = BR.ReadString
                resource.Comment = BR.ReadString
                Resources.Add(resource.ID, resource)
            Next

            Dim PeriodsDataCount As Integer = BR.ReadInt32
            For i As Integer = 1 To PeriodsDataCount
                Dim PeriodData As New AlternativePeriodsData
                Dim id As String = BR.ReadString
                PeriodData.Duration = BR.ReadInt32
                PeriodData.StartPeriod = BR.ReadInt32
                PeriodData.MinPeriod = BR.ReadInt32
                PeriodData.MaxPeriod = BR.ReadInt32
                PeriodsData.PeriodsData.Add(id, PeriodData)

                Dim tpDataCount As Integer = BR.ReadInt32
                For j As Integer = 1 To tpDataCount
                    Dim tpID As Integer = BR.ReadInt32
                    Dim tpCount As Integer = BR.ReadInt32

                    Dim newRdata As New Dictionary(Of Guid, Double) ' D3918
                    For k As Integer = 1 To tpCount
                        Dim resourceID As New Guid(BR.ReadString) ' D3918
                        Dim resourceValue As Double = BR.ReadDouble
                        newRdata.Add(resourceID, resourceValue)
                    Next

                    PeriodData.ResourceData.Add(tpID, newRdata)
                Next
            Next

            BR.Close()
        End Sub

        Public Function CreateStream_v_39() As MemoryStream
            Dim MS As New MemoryStream
            Dim BW As New BinaryWriter(MS)

            BW.Write(NamePrefix)
            BW.Write(PeriodsType)
            BW.Write(PeriodsStep)
            BW.Write(TimelineStartDate.ToBinary)
            BW.Write(DiscountFactor)
            BW.Write(UseDiscountFactor)

            BW.Write(Periods.Count)
            For Each period As RATimePeriod In Periods
                BW.Write(period.ID)
                BW.Write(period.Name)
                BW.Write(period.ShortName)
                BW.Write(period.Comment)

                BW.Write(period.ResourceMaxValues.Count)
                For Each kvp As KeyValuePair(Of Guid, Double) In period.ResourceMaxValues   ' D3918
                    BW.Write(kvp.Key.ToString) ' D3918
                    BW.Write(kvp.Value)
                Next

                BW.Write(period.ResourceMinValues.Count)
                For Each kvp As KeyValuePair(Of Guid, Double) In period.ResourceMinValues   ' D3918
                    BW.Write(kvp.Key.ToString) ' D3918
                    BW.Write(kvp.Value)
                Next
            Next

            BW.Write(Resources.Count)
            For Each kvp As KeyValuePair(Of Guid, RAResource) In Resources  ' D3918
                BW.Write(kvp.Value.ID.ToString)    ' D3918
                BW.Write(kvp.Value.Name)
                BW.Write(kvp.Value.Comment)

                ' v1.1.39
                BW.Write(kvp.Value.Enabled)
                BW.Write(kvp.Value.ConstraintID)
            Next

            BW.Write(PeriodsData.PeriodsData.Count)
            For Each kvp As KeyValuePair(Of String, AlternativePeriodsData) In PeriodsData.PeriodsData
                BW.Write(kvp.Key)
                BW.Write(kvp.Value.Duration)
                BW.Write(kvp.Value.StartPeriod)
                BW.Write(kvp.Value.MinPeriod)
                BW.Write(kvp.Value.MaxPeriod)

                BW.Write(kvp.Value.ResourceData.Count)
                For Each kvpPeriods As KeyValuePair(Of Integer, Dictionary(Of Guid, Double)) In kvp.Value.ResourceData ' D3918
                    BW.Write(kvpPeriods.Key)
                    BW.Write(kvpPeriods.Value.Count)
                    For Each kvpResources As KeyValuePair(Of Guid, Double) In kvpPeriods.Value ' D3918
                        BW.Write(kvpResources.Key.ToString)     ' D3918
                        BW.Write(kvpResources.Value)
                    Next
                Next
            Next

            Return MS
        End Function

        Public Function CreateStream_v_40() As MemoryStream
            Dim MS As New MemoryStream
            Dim BW As New BinaryWriter(MS)

            BW.Write(NamePrefix)
            BW.Write(PeriodsType)
            BW.Write(PeriodsStep)
            BW.Write(TimelineStartDate.ToBinary)
            BW.Write(DiscountFactor)
            BW.Write(UseDiscountFactor)

            BW.Write(Periods.Count)
            For Each period As RATimePeriod In Periods
                BW.Write(period.ID)
                BW.Write(period.Name)
                BW.Write(period.ShortName)
                BW.Write(period.Comment)

                BW.Write(period.ResourceMaxValues.Count)
                For Each kvp As KeyValuePair(Of Guid, Double) In period.ResourceMaxValues   ' D3918
                    BW.Write(kvp.Key.ToString) ' D3918
                    BW.Write(kvp.Value)
                Next

                BW.Write(period.ResourceMinValues.Count)
                For Each kvp As KeyValuePair(Of Guid, Double) In period.ResourceMinValues   ' D3918
                    BW.Write(kvp.Key.ToString) ' D3918
                    BW.Write(kvp.Value)
                Next
            Next

            BW.Write(Resources.Count)
            For Each kvp As KeyValuePair(Of Guid, RAResource) In Resources  ' D3918
                BW.Write(kvp.Value.ID.ToString)    ' D3918
                BW.Write(kvp.Value.Name)
                BW.Write(kvp.Value.Comment)

                ' v1.1.39
                BW.Write(kvp.Value.Enabled)
                BW.Write(kvp.Value.ConstraintID)
            Next

            BW.Write(PeriodsData.PeriodsData.Count)
            For Each kvp As KeyValuePair(Of String, AlternativePeriodsData) In PeriodsData.PeriodsData
                BW.Write(kvp.Key)
                BW.Write(kvp.Value.Duration)
                BW.Write(kvp.Value.StartPeriod)
                BW.Write(kvp.Value.MinPeriod)
                BW.Write(kvp.Value.MaxPeriod)

                'new to 1.1.40
                BW.Write(kvp.Value.HasMust)
                BW.Write(kvp.Value.MustPeriod)

                BW.Write(kvp.Value.ResourceData.Count)
                For Each kvpPeriods As KeyValuePair(Of Integer, Dictionary(Of Guid, Double)) In kvp.Value.ResourceData ' D3918
                    BW.Write(kvpPeriods.Key)
                    BW.Write(kvpPeriods.Value.Count)
                    For Each kvpResources As KeyValuePair(Of Guid, Double) In kvpPeriods.Value ' D3918
                        BW.Write(kvpResources.Key.ToString)     ' D3918
                        BW.Write(kvpResources.Value)
                    Next
                Next
            Next

            Return MS
        End Function

        Public Function CreateStream_v_38() As MemoryStream
            Dim MS As New MemoryStream
            Dim BW As New BinaryWriter(MS)

            BW.Write(NamePrefix)
            BW.Write(PeriodsType)
            BW.Write(PeriodsStep)
            BW.Write(TimelineStartDate.ToBinary)
            BW.Write(DiscountFactor)
            BW.Write(UseDiscountFactor)

            BW.Write(Periods.Count)
            For Each period As RATimePeriod In Periods
                BW.Write(period.ID)
                BW.Write(period.Name)
                BW.Write(period.ShortName)
                BW.Write(period.Comment)

                BW.Write(period.ResourceMaxValues.Count)
                For Each kvp As KeyValuePair(Of Guid, Double) In period.ResourceMaxValues ' D3918
                    BW.Write(kvp.Key.ToString) ' D3918
                    BW.Write(kvp.Value)
                Next

                BW.Write(period.ResourceMinValues.Count)
                For Each kvp As KeyValuePair(Of Guid, Double) In period.ResourceMinValues ' D3918
                    BW.Write(kvp.Key.ToString) ' D3918
                    BW.Write(kvp.Value)
                Next
            Next

            BW.Write(Resources.Count)
            For Each kvp As KeyValuePair(Of Guid, RAResource) In Resources ' D3918
                BW.Write(kvp.Value.ID.ToString) ' D3918
                BW.Write(kvp.Value.Name)
                BW.Write(kvp.Value.Comment)
            Next

            BW.Write(PeriodsData.PeriodsData.Count)
            For Each kvp As KeyValuePair(Of String, AlternativePeriodsData) In PeriodsData.PeriodsData
                BW.Write(kvp.Key)
                BW.Write(kvp.Value.Duration)
                BW.Write(kvp.Value.StartPeriod)
                BW.Write(kvp.Value.MinPeriod)
                BW.Write(kvp.Value.MaxPeriod)

                BW.Write(kvp.Value.ResourceData.Count)
                For Each kvpPeriods As KeyValuePair(Of Integer, Dictionary(Of Guid, Double)) In kvp.Value.ResourceData ' D3918
                    BW.Write(kvpPeriods.Key)
                    BW.Write(kvpPeriods.Value.Count)
                    For Each kvpResources As KeyValuePair(Of Guid, Double) In kvpPeriods.Value ' D3918
                        BW.Write(kvpResources.Key.ToString) ' D3918
                        BW.Write(kvpResources.Value)
                    Next
                Next
            Next

            Return MS
        End Function

        Public Function CreateStream_v_37() As MemoryStream
            Dim MS As New MemoryStream
            Dim BW As New BinaryWriter(MS)

            BW.Write(NamePrefix)
            BW.Write(PeriodsType)
            BW.Write(PeriodsStep)
            BW.Write(TimelineStartDate.ToBinary)

            BW.Write(Periods.Count)
            For Each period As RATimePeriod In Periods
                BW.Write(period.ID)
                BW.Write(period.Name)
                BW.Write(period.ShortName)
                BW.Write(period.Comment)

                BW.Write(period.ResourceMaxValues.Count)
                For Each kvp As KeyValuePair(Of Guid, Double) In period.ResourceMaxValues ' D3918
                    BW.Write(kvp.Key.ToString) ' D3918
                    BW.Write(kvp.Value)
                Next

                BW.Write(period.ResourceMinValues.Count)
                For Each kvp As KeyValuePair(Of Guid, Double) In period.ResourceMinValues ' D3918
                    BW.Write(kvp.Key.ToString) ' D3918
                    BW.Write(kvp.Value)
                Next
            Next

            BW.Write(Resources.Count)
            For Each kvp As KeyValuePair(Of Guid, RAResource) In Resources ' D3918
                BW.Write(kvp.Value.ID.ToString) ' D3918
                BW.Write(kvp.Value.Name)
                BW.Write(kvp.Value.Comment)
            Next

            BW.Write(PeriodsData.PeriodsData.Count)
            For Each kvp As KeyValuePair(Of String, AlternativePeriodsData) In PeriodsData.PeriodsData
                BW.Write(kvp.Key)
                BW.Write(kvp.Value.Duration)
                BW.Write(kvp.Value.StartPeriod)
                BW.Write(kvp.Value.MinPeriod)
                BW.Write(kvp.Value.MaxPeriod)

                BW.Write(kvp.Value.ResourceData.Count)
                For Each kvpPeriods As KeyValuePair(Of Integer, Dictionary(Of Guid, Double)) In kvp.Value.ResourceData ' D3918
                    BW.Write(kvpPeriods.Key)
                    BW.Write(kvpPeriods.Value.Count)
                    For Each kvpResources As KeyValuePair(Of Guid, Double) In kvpPeriods.Value ' D3918
                        BW.Write(kvpResources.Key.ToString) ' D3918
                        BW.Write(kvpResources.Value)
                    Next
                Next
            Next

            Return MS
        End Function

        Public Function CreateStream() As MemoryStream
            Dim MS As New MemoryStream
            Dim BW As New BinaryWriter(MS)

            BW.Write(NamePrefix)
            BW.Write(PeriodsType)
            BW.Write(PeriodsStep)
            BW.Write(TimelineStartDate.ToBinary)

            BW.Write(Periods.Count)
            For Each period As RATimePeriod In Periods
                BW.Write(period.ID)
                BW.Write(period.Name)
                BW.Write(period.ShortName)
                BW.Write(period.Comment)

                BW.Write(period.ResourceMaxValues.Count)
                For Each kvp As KeyValuePair(Of Guid, Double) In period.ResourceMaxValues ' D3918
                    BW.Write(kvp.Key.ToString) ' D3918
                    BW.Write(kvp.Value)
                Next
            Next

            BW.Write(Resources.Count)
            For Each kvp As KeyValuePair(Of Guid, RAResource) In Resources ' D3918
                BW.Write(kvp.Value.ID.ToString) ' D3918
                BW.Write(kvp.Value.Name)
                BW.Write(kvp.Value.Comment)
            Next

            BW.Write(PeriodsData.PeriodsData.Count)
            For Each kvp As KeyValuePair(Of String, AlternativePeriodsData) In PeriodsData.PeriodsData
                BW.Write(kvp.Key)
                BW.Write(kvp.Value.Duration)
                BW.Write(kvp.Value.StartPeriod)
                BW.Write(kvp.Value.MinPeriod)
                BW.Write(kvp.Value.MaxPeriod)

                BW.Write(kvp.Value.ResourceData.Count)
                For Each kvpPeriods As KeyValuePair(Of Integer, Dictionary(Of Guid, Double)) In kvp.Value.ResourceData ' D3918
                    BW.Write(kvpPeriods.Key)
                    BW.Write(kvpPeriods.Value.Count)
                    For Each kvpResources As KeyValuePair(Of Guid, Double) In kvpPeriods.Value ' D3918
                        BW.Write(kvpResources.Key.ToString) ' D3918
                        BW.Write(kvpResources.Value)
                    Next
                Next
            Next

            'BW.Close()

            Return MS
        End Function

        Private _Results As New Dictionary(Of String, Integer)
        Public Property TimePeriodResults() As Dictionary(Of String, Integer)
            Get
                If _Results Is Nothing Then _Results = New Dictionary(Of String, Integer)
                Return _Results
            End Get
            Set(value As Dictionary(Of String, Integer))
                _Results = value
            End Set
        End Property

        Public Function AllocateResourceValues() As Boolean
            Dim fUpdated As Boolean = False
            For Each alt As RAAlternative In Scenario.Alternatives
                Dim apd As AlternativePeriodsData = PeriodsData.GetAlternativePeriodsData(alt.ID)
                For Each resource As RAResource In Resources.Values
                    Dim total As Double = CDbl(If(alt.Cost = UNDEFINED_ATTRIBUTE_DEFAULT_DOUBLE_VALUE, 0, alt.Cost)) 'A1381
                    If resource.ConstraintID >= 0 Then
                        total = Scenario.Constraints.GetConstraintValue(resource.ConstraintID, alt.ID)
                    End If
                    Dim rTotalCost As Double = UNDEFINED_INTEGER_VALUE  ' D3962
                    Dim fHasUndefined As Boolean = False
                    For i As Integer = 0 To apd.Duration - 1
                        Dim rValue As Double = PeriodsData.GetResourceValue(i, alt.ID, resource.ID)
                        If rValue <> UNDEFINED_INTEGER_VALUE Then
                            If rTotalCost = UNDEFINED_INTEGER_VALUE Then rTotalCost = 0 ' D3962
                            rTotalCost += rValue
                        Else
                            fHasUndefined = True
                        End If
                    Next
                    If rTotalCost <> total OrElse fHasUndefined Then
                        For i As Integer = 0 To apd.Duration - 1
                            ' D3963 ===
                            Dim tVal As Double = PeriodsData.GetResourceValue(i, alt.ID, resource.ID)
                            Dim tNewVal As Double = CDbl(If(total = UNDEFINED_INTEGER_VALUE, UNDEFINED_INTEGER_VALUE, total / apd.Duration))
                            If tVal <> tNewVal Then
                                PeriodsData.SetResourceValue(i, alt.ID, resource.ID, tNewVal)
                                ' D3963 ==
                                fUpdated = True
                            End If
                        Next
                    End If
                Next
            Next
            Return fUpdated
        End Function

        ' D3840 ===
        Public Function Clone(aScenario As RAScenario) As RATimePeriods
            Dim tNew As New RATimePeriods(aScenario)
            tNew.Periods = New List(Of RATimePeriod)
            For Each tPeriod As RATimePeriod In Me.Periods
                tNew.Periods.Add(tPeriod.Clone(tNew))
            Next
            tNew.NamePrefix = Me.NamePrefix
            tNew.PeriodsType = Me.PeriodsType
            tNew.PeriodsStep = Me.PeriodsStep
            tNew.TimelineStartDate = Me.TimelineStartDate
            tNew.UseDiscountFactor = Me.UseDiscountFactor
            tNew.DiscountFactor = Me.DiscountFactor
            tNew.PeriodsData = New RAPeriodsData()
            For Each sKey As String In Me.PeriodsData.PeriodsData.Keys
                tNew.PeriodsData.PeriodsData.Add(sKey, Me.PeriodsData.Clone(sKey))  ' D4290
            Next
            tNew.Resources = New Dictionary(Of Guid, RAResource) ' D3918
            For Each tResName As Guid In Me.Resources.Keys ' D3918
                tNew.Resources.Add(tResName, Me.Resources(tResName).Clone())    ' D4290
            Next
            ' -D3943
            'With Scenario.Scenarios.ResourceAligner.ProjectManager.Parameters
            '    .TimeperiodsHasData(aScenario.ID) = .TimeperiodsHasData(Scenario.ID)
            'End With
            Return tNew
        End Function
        ' D3840 ==

        Private Sub AddCostResource()
            Dim CostResource As New RAResource
            CostResource.ID = RA_Cost_GUID     ' D3918
            CostResource.Name = "Cost"
            Resources.Add(CostResource.ID, CostResource)
        End Sub

        Public Sub New(Scenario As RAScenario)
            Me.Scenario = Scenario
            AddCostResource()
        End Sub

    End Class

End Namespace
