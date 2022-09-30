Option Strict On

Imports System.Linq
Imports Canvas
Imports ECCore

Namespace ECCore

    <Serializable> Public Class clsSimulationData
        Public Property Total As Double = 0
        Public Property Events As List(Of clsSimulationEvent) ' Event ID, is triggered/Rand
        Public Property Sources As Dictionary(Of Integer, clsSimulationSource) ' Source ID, is triggered/Rand

        'Public Property NetConsequences As Dictionary(Of Integer, Double) ' keep track of consequences of occured objectives
        'Public Property SumConsequences As Dictionary(Of Integer, Double) ' keep track of consequences of occured objectives
    End Class

    <Serializable> Public Class clsSimulationEvent
        Public Property EventID As Integer = -1
        Public Property Fired As Boolean = False
        Public Property Rand As Double = -1
        Public Property SourceID As Integer = -1
        Public Property WRTLikelihood As Double = 0 ' Likelihood of the event WRT the source that fired
        Public Property SimulatedImpact As Double = 0
        'Public Property Vulnerabilities As Dictionary(Of Integer, Integer) ' keep track of vulnerabilities of occured sources
        'Public Property Consequences As Dictionary(Of Integer, Double) ' keep track of consequences of occured objectives. X - Consequence, Y - priority of Objective
        Public Property SumConsequences As Double = 0 ' for nomalization of consequences
    End Class

    <Serializable> Public Class clsSimulationSource
        Public Property ID As Integer = -1
        Public Property Priority As Double = 0
        Public Property Fired As Boolean = False
        Public Property Rand As Double = -1
    End Class

    <Serializable> Public Class DblPoint
        Public Property X As Double = 0
        Public Property Y As Double = 0
        Public Sub New(mX As Double, mY As Double)
            X = mX
            Y = mY
        End Sub
    End Class

    <Serializable> Public Class clsLEC
        Public Const _OPT_EPS As Double = 0.0000000001

        Public Shared Function LinearInterpolation(ByVal A As DblPoint, ByVal B As DblPoint, ByVal X As Double) As Double
            Dim retVal As Double = UNDEFINED_INTEGER_VALUE
            If Math.Abs(B.X - A.X) < _OPT_EPS Then
                retVal = A.Y
            Else
                Dim ai As Double = (B.Y - A.Y) / (B.X - A.X)
                Dim bi As Double = A.Y - ai * A.X
                retVal = ai * X + bi
            End If
            Return retVal
        End Function

        <Serializable> Public Class clsEventData
            Public Property ID As Integer = -1
            Public Property GuidID As Guid = Guid.Empty ' we need this Guid for getting attribute values when "Attributes" feature is On
            Public Property FiredSourceID As Integer = -1
            Public Property Vulnerability As Double = UNDEFINED_INTEGER_VALUE
        End Class

        Public Shared _SIMULATE_SOURCES As Integer = 0
        Public Shared _SIMULATE_EVENTS As Integer = 1

        Private PM As clsProjectManager

        Public Delegate Function FCancellation() As Boolean
        Private IsCancellationPending As FCancellation

        Public Sub New(tPM As clsProjectManager, tCancellationFunc As FCancellation)
            IsCancellationPending = tCancellationFunc
            PM = tPM
        End Sub

        Private maxNumSimulations As Integer = 1000000
        Public Const NumStoredSteps As Integer = 200
        Public Property NumberOfSimulations As Integer
            Get
                Dim retVal As Integer = CInt(PM.Attributes.GetAttributeValue(ATTRIBUTE_LEC_NUM_SIMULATIONS_ID, UNDEFINED_USER_ID))
                Return If(retVal < maxNumSimulations, retVal, maxNumSimulations)
            End Get
            Set(value As Integer)                
                value = If(value < maxNumSimulations, value, maxNumSimulations)
                PM.Attributes.SetAttributeValue(ATTRIBUTE_LEC_NUM_SIMULATIONS_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtLong, value, Guid.Empty, Guid.Empty)
                With PM
                    .Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID, UNDEFINED_USER_ID)
                End With
            End Set
        End Property

        Public ReadOnly Property RandSeed As Integer
            Get
                Return CInt(PM.Attributes.GetAttributeValue(ATTRIBUTE_LEC_RAND_SEED_ID, UNDEFINED_USER_ID))
            End Get
        End Property

        Public DataDictImpacts As Dictionary(Of Integer, List(Of AlternativeRiskDataDataContract)) = Nothing
        Public DataDictLikelihoods As Dictionary(Of Integer, List(Of AlternativeRiskDataDataContract)) = Nothing

        Private Function GetDictionaryOfNodes(H As clsHierarchy) As Dictionary(Of Integer, clsNode)
            Dim res As New Dictionary(Of Integer, clsNode)
            For Each node As clsNode In H.Nodes
                res.Add(node.NodeID, node)
            Next
            If H.HierarchyID = ECHierarchyID.hidLikelihood Then
                'res.Add(-1, H.Nodes(0))
            End If
            Return res
        End Function

        Private Function GetTerminalNodesDictionary(H As clsHierarchy) As Dictionary(Of Integer, HashSet(Of Integer))
            Dim res As New Dictionary(Of Integer, HashSet(Of Integer))
            For Each node As clsNode In H.TerminalNodes
                node.NumFired = 0
                node.SimulatedPriority = 0
                node.SimulatedAltLikelihood = 0
                node.SimulatedAltImpact = 0
                node.SimulatedConsequences.Clear()
                node.SimulatedVulnerabilities.Clear()
                node.SumNetConsequences = 0
                Dim hs As New HashSet(Of Integer)
                For Each alt As clsNode In node.GetNodesBelow(UNDEFINED_USER_ID).Where(Function (tAlt) tAlt.Enabled).ToList
                    hs.Add(alt.NodeID)
                Next
                res.Add(node.NodeID, hs)
            Next
            Return res
        End Function

        Public Function SimulateLossExceedance(UserID As Integer, UserEmail As String, tSimulationMode As Integer, isExclusiveEvents As Boolean, tControlsUsageMode As ControlsUsageMode, ByRef dMaxLossValue As Double, rand As Random, tLimitImpactsTo100 As Boolean, tAllowEventFireMultipleTimes As Boolean, tSourcesType As Integer, tAttributesList As List(Of clsAttribute), tSelectedCategoryID As Guid, tUseEventGroups As Boolean, tSimulationWRTNodeGuid As Guid, IsOpportunityModel As Boolean) As List(Of clsSimulationData)
            If IsCancellationPending() Then Return Nothing

            Dim Events As List(Of clsSimulationEvent) = Nothing
            Dim Sources As Dictionary(Of Integer, clsSimulationSource) = Nothing

            Dim NetConsequences As Dictionary(Of Integer, Double) = Nothing
            Dim SumConsequences As Dictionary(Of Integer, Double) = Nothing

            MiscFuncs.PrintDebugInfo("-- SimulateLossExceedance start: ")

            Dim simData As List(Of clsSimulationData) = New List(Of clsSimulationData)
            'Dim tTruncateImpacts As Boolean = TruncateImpacts            
            Dim DataDictSources As List(Of clsSimulationSource) = Nothing
            Dim tNumSimulations As Integer = NumberOfSimulations
            Dim tAllEventGroups As New List(Of String)
            Dim isOneFromCategory As Boolean = Not tSelectedCategoryID.Equals(Guid.Empty) AndAlso tAttributesList.Where(Function(attr) attr.ID.Equals(tSelectedCategoryID)).Count > 0

            Dim altH As clsHierarchy = PM.AltsHierarchy(PM.ActiveAltsHierarchy)
            Dim IH As clsHierarchy = PM.Hierarchy(ECHierarchyID.hidImpact)
            Dim LH As clsHierarchy = PM.Hierarchy(ECHierarchyID.hidLikelihood)

            Dim LikelihoodDic As Dictionary(Of Integer, clsNode) = GetDictionaryOfNodes(LH)
            Dim ImpactDic As Dictionary(Of Integer, clsNode) = GetDictionaryOfNodes(IH)
            Dim EventsDic As Dictionary(Of Integer, clsNode) = GetDictionaryOfNodes(altH)

            ' In GetTerminalNodesDictionary we make SimulatedPriority = 0
            Dim terminalNodesLikelihood As Dictionary(Of Integer, HashSet(Of Integer)) = GetTerminalNodesDictionary(LH)
            Dim terminalNodesImpact As Dictionary(Of Integer, HashSet(Of Integer)) = GetTerminalNodesDictionary(IH)
            Dim terminalNodesAlts As Dictionary(Of Integer, HashSet(Of Integer)) = GetTerminalNodesDictionary(altH)

            Dim uncontributedAlts As List(Of clsNode) = LH.GetUncontributedAlternatives().Where(Function(alt) alt.Enabled).ToList
            Dim impWrtNode As clsNode = IH.GetNodeByID(tSimulationWRTNodeGuid)
            Dim lkhWrtNode As clsNode = LH.GetNodeByID(tSimulationWRTNodeGuid)

            Dim overallWrtNode As clsNode = LH.Nodes(0)
            If impWrtNode Is Nothing Then
                impWrtNode = IH.Nodes(0)
                overallWrtNode = impWrtNode
            End If

            If lkhWrtNode Is Nothing Then
                lkhWrtNode = LH.Nodes(0)
                overallWrtNode = lkhWrtNode
            End If

            Dim respectiveImpactTerminalNodes As List(Of clsNode) = IH.GetRespectiveTerminalNodes(impWrtNode)

            Dim isLkhWrtNodeSelected As Boolean = lkhWrtNode IsNot Nothing
            Dim isImpWrtNodeSelected As Boolean = impWrtNode IsNot Nothing

            Dim tControlsForConsequences As IEnumerable(Of clsControl) = PM.Controls.EnabledControls.Where(Function (ctrl) ctrl.Type = ControlType.ctConsequenceToEvent)

            clsCalculationsManager.CalculateCount = 0

            Dim CalcTarget As clsCalculationTarget = GetCalcTarget(UserID, UserEmail)
            Dim oldCacheValue As Boolean = PrioritiesCacheManager.Enabled
            If Not oldCacheValue Then
                PM.CalculationsManager.PrioritiesCacheManager.ClearCache()
            End If
            PrioritiesCacheManager.Enabled = True
            Dim oldHID As Integer = PM.ActiveHierarchy
            PM.ActiveHierarchy = ECHierarchyID.hidLikelihood
            PM.CalculationsManager.Calculate(CalcTarget, LH.Nodes(0), LH.HierarchyID)
            PM.ActiveHierarchy = ECHierarchyID.hidImpact
            PM.CalculationsManager.Calculate(CalcTarget, IH.Nodes(0), IH.HierarchyID)
            PM.ActiveHierarchy = oldHID

            If tNumSimulations > 0 Then
                Dim maxValue As Double = 0

                Dim simEvents As New List(Of clsEventData)
                'For Each ID As Integer In SelectedEventIDs
                For Each tAlt As clsNode In EventsDic.Values
                    Dim e As New clsEventData With {.ID = tAlt.NodeID, .GuidID = tAlt.NodeGuidID}
                    'If isOneFromCategory Then 
                    'Dim tAlt As clsNode = EventsDic(ID)
                    'If tAlt IsNot Nothing Then e.GuidID = tAlt.NodeGuidID ' Guids needed for isOneFromCategory option
                    'End If
                    simEvents.Add(e)
                Next

                Debug.Print("Calculations count = " + clsCalculationsManager.CalculateCount.ToString)

                If tLimitImpactsTo100 Then
                    ' calculate Impacts
                    DataDictImpacts = New Dictionary(Of Integer, List(Of AlternativeRiskDataDataContract))
                    GetRiskDataWRTEvents(UserID, UserEmail, ECHierarchyID.hidImpact, overallWrtNode, DataDictImpacts, ControlsUsageMode.DoNotUse)  ' always use "without controls" for Impact. The controls will be applied after the simulation

                    Debug.Print("Calculations count = " + clsCalculationsManager.CalculateCount.ToString)
                End If

                If tSimulationMode = _SIMULATE_SOURCES Then
                    ' calculate Likelihoods
                    DataDictLikelihoods = New Dictionary(Of Integer, List(Of AlternativeRiskDataDataContract))
                    GetRiskDataWRTEvents(UserID, UserEmail, ECHierarchyID.hidLikelihood, overallWrtNode, DataDictLikelihoods, tControlsUsageMode)

                    Debug.Print("Calculations count = " + clsCalculationsManager.CalculateCount.ToString)

                    ' calculate for Sources
                    DataDictSources = New List(Of clsSimulationSource)
                    GetRiskDataForSources(UserID, UserEmail, lkhWrtNode, DataDictSources, tControlsUsageMode)

                    Debug.Print("Calculations count = " + clsCalculationsManager.CalculateCount.ToString)

                    ' calculate for goal and get vulnerabilities for events with no sources
                    If uncontributedAlts.Count > 0 Then

                        Dim oldUseReduction As ControlsUsageMode = PM.CalculationsManager.ControlsUsageMode
                        PM.CalculationsManager.ControlsUsageMode = tControlsUsageMode

                        PM.CalculationsManager.Calculate(CalcTarget, lkhWrtNode, ECHierarchyID.hidLikelihood)

                        Debug.Print("Calculations count = " + clsCalculationsManager.CalculateCount.ToString)

                        PM.CalculationsManager.ControlsUsageMode = oldUseReduction

                        For Each tNoSourcesAlt As clsNode In uncontributedAlts
                            For Each e As clsEventData In simEvents
                                If e.ID = tNoSourcesAlt.NodeID Then
                                    e.Vulnerability = tNoSourcesAlt.UnnormalizedPriority
                                    e.FiredSourceID = -1
                                End If
                            Next
                        Next
                    End If
                End If

                If tLimitImpactsTo100 Then ' calculate for impacts so that the priorities of terminal nodes could be used in the synthesis step
                    Dim oldUseReduction As ControlsUsageMode = PM.CalculationsManager.ControlsUsageMode
                    PM.CalculationsManager.ControlsUsageMode = ControlsUsageMode.DoNotUse ' always use "without controls" for Impact. The controls will be applied after the simulation

                    PM.CalculationsManager.Calculate(CalcTarget, IH.Nodes(0), ECHierarchyID.hidImpact)

                    Debug.Print("Calculations count = " + clsCalculationsManager.CalculateCount.ToString)

                    PM.CalculationsManager.ControlsUsageMode = oldUseReduction
                End If
                MiscFuncs.PrintDebugInfo("-- Start cycle: ")
                For i As Integer = 0 To tNumSimulations - 1
                    If i Mod 100 = 0 AndAlso IsCancellationPending() Then Exit For 'if cancellation is pending then exit. Check only one in 100 times

                    'Dim curStep As clsSimulationData = New clsSimulationData With {.Events = New List(Of clsSimulationEvent)}
                    Dim curStep As clsSimulationData = New clsSimulationData
                    Events = New List(Of clsSimulationEvent)
                    Sources = New Dictionary(Of Integer, clsSimulationSource)

                    Dim sum As Double = 0
                    'Dim numTriggered As Double = 0
                    'Dim exclusiveRand As Double = 0
                    'If isExclusiveEvents Then exclusiveRand = rand.NextDouble() ' events are mutually exclusive 
                    'Dim stopSimulation = false

                    Dim occuredCategroies As List(Of Guid) = Nothing
                    If isOneFromCategory Then occuredCategroies = New List(Of Guid)

                    'occuredCategroies.Add(Guid.Empty) 'No category, treated as mutually exclusive

                    If tLimitImpactsTo100 Then
                        'curStep.NetConsequences = New Dictionary(Of Integer, Double)
                        'curStep.SumConsequences = New Dictionary(Of Integer, Double)
                        NetConsequences = New Dictionary(Of Integer, Double)
                        SumConsequences = New Dictionary(Of Integer, Double)
                    End If

                    Dim eventsToCheck As List(Of clsEventData) = simEvents
                    Dim eventsToCheckHashset As New HashSet(Of Integer)

                    If tSimulationMode = _SIMULATE_SOURCES Then
                        eventsToCheck = New List(Of clsEventData)
                        'Dim sumForSources As Double = 0

                        For Each simulationSource As clsSimulationSource In DataDictSources
                            'If tSourcesType = 0 AndAlso sumForSources > 1 Then Exit For
                            Dim randForSource As Double = rand.NextDouble()
                            'if source fired
                            Dim bFired As Boolean = randForSource <= simulationSource.Priority

                            'If curStep.Sources Is Nothing Then curStep.Sources = New Dictionary(Of Integer, clsSimulationSource)
                            'curStep.Sources.Add(e.ID, New clsSimulationSource With {.ID = e.ID, .Rand = randForSource, .Fired = bFired, .Priority = e.Priority})
                            Sources.Add(simulationSource.ID, New clsSimulationSource With {.ID = simulationSource.ID, .Rand = randForSource, .Fired = bFired, .Priority = simulationSource.Priority})
                            
                            If bFired Then ' source fired
                                Dim tSource As clsNode = LikelihoodDic(simulationSource.ID)
                                tSource.NumFired += 1
                                'Dim tEvents As List(Of clsNode) = tSource.GetContributedAlternatives().Where(Function(alt) alt.Enabled).ToList
                                Dim eventsBelowSource As List(Of clsNode) = tSource.GetNodesBelow(UNDEFINED_USER_ID)

                                For Each childEvent As clsNode In eventsBelowSource
                                    Dim tSimEvent As clsEventData = GetSimulationEventByID(childEvent, simEvents, childEvent.NodeID)
                                    'If tSimEvent IsNot Nothing AndAlso (tAllowEventFireMultipleTimes OrElse Not eventsToCheck.Contains(tSimEvent)) Then
                                    If tSimEvent IsNot Nothing Then
                                        If eventsToCheckHashset.Contains(tSimEvent.ID) Then
                                            'Dim tSimEventCopy As clsEventData = New clsEventData With {.EventGroupName = tSimEvent.EventGroupName, .GuID = tSimEvent.GuID, .ID = tSimEvent.ID, .Name = tSimEvent.Name, .Precedence = tSimEvent.Precedence, .Priorities = tSimEvent.Priorities, .PrioritiesWithControls = tSimEvent.PrioritiesWithControls}
                                            Dim tSimEventCopy As clsEventData = New clsEventData With {.GuidID = tSimEvent.GuidID, .ID = tSimEvent.ID}
                                            tSimEventCopy.FiredSourceID = simulationSource.ID
                                            eventsToCheck.Add(tSimEventCopy)
                                        Else
                                            tSimEvent.FiredSourceID = simulationSource.ID
                                            eventsToCheck.Add(tSimEvent)
                                            eventsToCheckHashset.Add(tSimEvent.ID)
                                        End If
                                        'If eventsToCheck.Contains(tSimEvent) Then
                                        '    'Dim tSimEventCopy As clsEventData = New clsEventData With {.EventGroupName = tSimEvent.EventGroupName, .GuID = tSimEvent.GuID, .ID = tSimEvent.ID, .Name = tSimEvent.Name, .Precedence = tSimEvent.Precedence, .Priorities = tSimEvent.Priorities, .PrioritiesWithControls = tSimEvent.PrioritiesWithControls}
                                        '    Dim tSimEventCopy As clsEventData = New clsEventData With {.GuidID = tSimEvent.GuidID, .ID = tSimEvent.ID}
                                        '    tSimEventCopy.FiredSourceID = e.ID
                                        '    eventsToCheck.Add(tSimEventCopy)
                                        'Else
                                        '    tSimEvent.FiredSourceID = e.ID
                                        '    eventsToCheck.Add(tSimEvent)
                                        'End If
                                    End If
                                Next
                                If tSourcesType = 0 Then ' independent sources - can fire any number of sources until sum of them <= 1
                                    'If sumForSources <= 1 Then 
                                    '    sumForSources += e.Priority
                                    'Else 
                                    '    Exit For
                                    'End If
                                Else ' mutually exclusive sources - only one source can fire
                                    Exit For
                                End If
                            End If
                        Next

                        ' Add "events with no sources"
                        For Each tNoSourcesEvent As clsNode In uncontributedAlts
                            Dim tSimEvent As clsEventData = GetSimulationEventByID(tNoSourcesEvent, simEvents, tNoSourcesEvent.NodeID)
                            If tSimEvent IsNot Nothing AndAlso Not eventsToCheck.Contains(tSimEvent) Then
                                tSimEvent.FiredSourceID = -1
                                eventsToCheck.Add(tSimEvent)
                            End If
                        Next
                    End If

                    'If isExclusiveEvents Then ' events are mutually exclusive

                    If i < clsLEC.NumStoredSteps Then curStep.Sources = Sources

                    Dim eventTriggered As Boolean = False

                    For k As Integer = 0 To eventsToCheck.Count - 1
                        Dim e As clsEventData = eventsToCheck(k)
                        'Dim se As clsSimulationEvent = New clsSimulationEvent With {.Rand = exclusiveRand}
                        'Dim se As clsSimulationEvent = New clsSimulationEvent With {.EventID = e.ID, .Rand = rand.NextDouble(), .SourceID = e.FiredSourceID, .Vulnerabilities = New Dictionary(Of Integer, Integer)}
                        Dim se As clsSimulationEvent = New clsSimulationEvent With {.EventID = e.ID, .Rand = rand.NextDouble(), .SourceID = e.FiredSourceID}

                        Dim tCanProceed As Boolean = True
                        If Not tAllowEventFireMultipleTimes Then
                            ' check if this event already fired from another source 
                            'For Each existingEvent As clsSimulationEvent In curStep.Events
                            For Each existingEvent As clsSimulationEvent In Events
                                If existingEvent.EventID = e.ID AndAlso existingEvent.Fired Then
                                    tCanProceed = False
                                    Exit For
                                End If
                            Next

                        End If

                        'If sum < 1 Then 'A1301
                        If tCanProceed AndAlso (Not isExclusiveEvents OrElse Not eventTriggered) Then
                            'Dim e_prty As clsEventPriorities = e.Priorities
                            'If ControlsUsageMode <> ControlsUsageMode.DoNotUse Then e_prty = e.PrioritiesWithControls
                            Dim tAlt As clsNode = EventsDic(e.ID)

                            If tSimulationMode = _SIMULATE_SOURCES Then
                                Dim tSource As clsNode = If(LikelihoodDic.ContainsKey(e.FiredSourceID), LikelihoodDic(e.FiredSourceID), Nothing)
                                Dim AltWRTPriority As Double = 0
                                Dim tData As List(Of AlternativeRiskDataDataContract) = DataDictLikelihoods(tAlt.NodeID)
                                se.WRTLikelihood = 0
                                If tSource Is Nothing Then
                                    'If e.FiredSourceID = -1 Then
                                    ' event with no sources
                                    'se.WRTLikelihood = tAlt.UnnormalizedPriority
                                    se.WRTLikelihood = e.Vulnerability
                                    se.Fired = se.Rand <= se.WRTLikelihood
                                    If se.Fired Then
                                        tAlt.NumFired += 1
                                        'If Not se.Vulnerabilities.ContainsKey(-1) Then se.Vulnerabilities.Add(-1, 0)
                                        'se.Vulnerabilities(-1) += 1
                                        If Not tAlt.SimulatedVulnerabilities.ContainsKey(-1) Then tAlt.SimulatedVulnerabilities.Add(-1, 0)
                                        tAlt.SimulatedVulnerabilities(-1) += 1
                                    End If
                                Else
                                    For Each altVal As AlternativeRiskDataDataContract In tData
                                        If altVal.AlternativeID.Equals(tSource.NodeGuidID) Then
                                            se.WRTLikelihood = altVal.LikelihoodValue '* tSource.UnnormalizedPriority
                                            se.Fired = se.Rand <= se.WRTLikelihood
                                            If se.Fired Then
                                                'If Not se.Vulnerabilities.ContainsKey(tSource.NodeID) Then se.Vulnerabilities.Add(tSource.NodeID, 0)
                                                'se.Vulnerabilities(tSource.NodeID) += 1
                                                If Not tAlt.SimulatedVulnerabilities.ContainsKey(tSource.NodeID) Then tAlt.SimulatedVulnerabilities.Add(tSource.NodeID, 0)
                                                tAlt.SimulatedVulnerabilities(tSource.NodeID) += 1
                                            End If
                                            Exit For
                                        End If
                                    Next
                                End If
                                'se.WRTLikelihood = Math.Round(se.WRTLikelihood, d)
                                'se.Fired = se.Rand <= se.WRTLikelihood

                                If (se.Fired) Then
                                    eventTriggered = True

                                    'For Each tOtherSources As KeyValuePair(Of Integer, clsSimulationSource) In curStep.Sources
                                    For Each tOtherSource As clsNode In LH.TerminalNodes
                                        If tOtherSource Is tSource Then
                                            tAlt.NumFired += 1
                                            Exit For
                                        End If
                                        'Dim tOtherSourceValue As clsSimulationSource = curStep.Sources(tOtherSource.NodeID)
                                        Dim tOtherSourceValue As clsSimulationSource = Sources(tOtherSource.NodeID)
                                        If tOtherSourceValue.Fired AndAlso tSource IsNot Nothing AndAlso tOtherSourceValue.ID <> tSource.NodeID Then
                                            'Dim tOtherSource As clsNode = LikelihoodDic(tOtherSourceValue.ID)
                                            'Dim tOtherEvents As List(Of clsNode) = tOtherSource.GetContributedAlternatives()
                                            If terminalNodesLikelihood(tOtherSourceValue.ID).Contains(tAlt.NodeID) Then
                                                For Each altVal As AlternativeRiskDataDataContract In tData
                                                    If altVal.AlternativeID.Equals(tOtherSource.NodeGuidID) Then
                                                        se.WRTLikelihood = altVal.LikelihoodValue '* tSource.UnnormalizedPriority
                                                        If se.Rand <= se.WRTLikelihood Then
                                                            'If Not se.Vulnerabilities.ContainsKey(tOtherSource.NodeID) Then se.Vulnerabilities.Add(tOtherSource.NodeID, 0)
                                                            'se.Vulnerabilities(tOtherSource.NodeID) += 1
                                                            If Not tAlt.SimulatedVulnerabilities.ContainsKey(tOtherSource.NodeID) Then tAlt.SimulatedVulnerabilities.Add(tOtherSource.NodeID, 0)
                                                            tAlt.SimulatedVulnerabilities(tOtherSource.NodeID) += 1
                                                        End If
                                                        Exit For
                                                    End If
                                                Next
                                            End If
                                        End If
                                    Next
                                End If
                            Else
                                'se.Fired = se.Rand <= e_prty.Likelihood
                                se.Fired = se.Rand <= tAlt.UnnormalizedPriority
                                if se.Fired Then eventTriggered = True
                            End If

                            'Dim e_1_prty As clsEventPriorities = Nothing
                            'If k > 0 Then
                            '    If ControlsUsageMode Then
                            '        e_1_prty = eventsToCheck(k - 1).PrioritiesWithControls
                            '    Else
                            '        e_1_prty = eventsToCheck(k - 1).Priorities
                            '    End If
                            'End If

                            'se.Fired = (k = 0 OrElse se.Rand > e_1_prty.Likelihood) AndAlso se.Rand <= e_prty.Likelihood

                            If se.Fired AndAlso isOneFromCategory Then ' need to check category
                                Dim oCat As Object = PM.Attributes.GetAttributeValue(tSelectedCategoryID, e.GuidID)
                                Dim tCat As Guid = Guid.Empty
                                If oCat IsNot Nothing Then tCat = CType(oCat, Guid)
                                If occuredCategroies.Contains(tCat) Then
                                    se.Fired = False
                                    Dim alt As clsNode = EventsDic(e.ID)
                                    alt.NumFired -= 1
                                    If eventTriggered Then eventTriggered = False
                                Else
                                    If Not tCat.Equals(Guid.Empty) Then occuredCategroies.Add(tCat)
                                End If
                            End If

                            If se.Fired AndAlso tLimitImpactsTo100 Then
                                'se.Consequences = New Dictionary(Of Integer, Double)
                                Dim alt As clsNode = EventsDic(e.ID)
                                If alt IsNot Nothing AndAlso alt.Enabled Then
                                    Dim tData As List(Of AlternativeRiskDataDataContract) = DataDictImpacts(alt.NodeID)
                                    ' get priorities
                                    For Each node As clsNode In respectiveImpactTerminalNodes
                                        If terminalNodesImpact(node.NodeID).Contains(alt.NodeID) Then
                                            Dim ConsequenceOfEventWRTObjective As Double = 0
                                            For Each altVal As AlternativeRiskDataDataContract In tData
                                                If altVal.AlternativeID.Equals(node.NodeGuidID) Then
                                                    ConsequenceOfEventWRTObjective = altVal.ImpactValue ' This  value is always without controls

                                                    'Apply controls
                                                    Dim AltWRTReduction As Double = 1
                                                    If tControlsUsageMode = ControlsUsageMode.UseAll Or tControlsUsageMode = ControlsUsageMode.UseOnlyActive Then
                                                        For Each control As clsControl In tControlsForConsequences
                                                            If (tControlsUsageMode = ControlsUsageMode.UseAll) OrElse (tControlsUsageMode = ControlsUsageMode.UseOnlyActive AndAlso control.Active) Then
                                                                For Each assignment As clsControlAssignment In control.Assignments
                                                                    If (tControlsUsageMode = ControlsUsageMode.UseAll) OrElse assignment.IsActive Then
                                                                        If assignment.ObjectiveID.Equals(node.NodeGuidID) And assignment.EventID.Equals(alt.NodeGuidID) Then
                                                                            If Not IsOpportunityModel Then
                                                                                AltWRTReduction *= 1 - assignment.Value
                                                                            Else
                                                                                AltWRTReduction *= 1 + assignment.Value
                                                                            End If
                                                                        End If
                                                                    End If
                                                                Next
                                                            End If
                                                        Next
                                                    End If
                                                    ConsequenceOfEventWRTObjective *= AltWRTReduction
                                                    Exit For
                                                End If
                                            Next

                                            'If Not occuredImpacts.ContainsKey(node.NodeID) Then occuredImpacts.Add(node.NodeID, 0)
                                            'If occuredImpacts(node.NodeID) + NodeWRTPriority <= 1 Then
                                            '    occuredImpacts(node.NodeID) += NodeWRTPriority
                                            'End If

                                            'If Not curStep.Consequences.ContainsKey(node.NodeID) Then curStep.Consequences.Add(node.NodeID, 0)
                                            'curStep.Consequences(node.NodeID) += ConsequenceOfEventWRTObjective
                                            'If curStep.Consequences(node.NodeID) >= 1 Then
                                            '    curStep.Consequences(node.NodeID) = 1
                                            '    'stopSimulation = true
                                            'End If

                                            'If Not se.Consequences.ContainsKey(node.NodeID) Then se.Consequences.Add(node.NodeID, 0)
                                            'se.Consequences(node.NodeID) = ConsequenceOfEventWRTObjective
                                            If Not tAlt.SimulatedConsequences.ContainsKey(node.NodeID) Then tAlt.SimulatedConsequences.Add(node.NodeID, 0)
                                            tAlt.SimulatedConsequences(node.NodeID) += ConsequenceOfEventWRTObjective
                                            'se.Consequences(node.NodeID).Y = node.UnnormalizedPriority
                                        End If
                                    Next
                                End If
                            End If

                            ' event groups
                            'If Not isExclusiveEvents AndAlso se.Fired AndAlso tUseEventGroups AndAlso Not String.IsNullOrEmpty(e.EventGroupName) AndAlso tAllEventGroups.Count > 0 Then ' treat the events with no group as independent events
                            '    'Dim randNumber As Double = rand.NextDouble()

                            '    ' check to see if event from this group already fired
                            '    Dim isEventWithHigherPrecedenceAlreadyFired As Boolean = False
                            '    For Each evt As clsEventData In eventsToCheck
                            '        If isEventWithHigherPrecedenceAlreadyFired Then Exit For
                            '        If e.EventGroupName = evt.EventGroupName Then
                            '            Dim u As Integer = 0
                            '            For Each cse As clsSimulationEvent In curStep.Events 'check fired events
                            '                If cse.Fired AndAlso evt.EventGroupName = eventsToCheck(u).EventGroupName AndAlso evt.Precedence <= eventsToCheck(u).Precedence Then
                            '                    isEventWithHigherPrecedenceAlreadyFired = True
                            '                    Exit For
                            '                    'Dim fe = eventsToCheck.where(Function (sse) sse.ID = cse.Key)(0) ' fired event
                            '                    'If (fe.Precedence <> UNDEFINED_INTEGER_VALUE AndAlso e.Precedence <> UNDEFINED_INTEGER_VALUE AndAlso fe.Precedence > e.Precedence) Then 
                            '                    '    numberOfHighestPrecedenceEventsInGroup += 1
                            '                    'End If
                            '                End If
                            '                u += 1
                            '            Next
                            '        End If
                            '    Next

                            '    If isEventWithHigherPrecedenceAlreadyFired Then
                            '        se.Fired = False
                            '    End If
                            'End If


                            'If se.Fired Then
                            '    If isDollValue Then
                            '        sumDoll += e_prty.CostImpact
                            '        'A1301 ===
                            '        'If tTruncateImpacts AndAlso sumDoll > tValueOfEnterprise Then
                            '        '    sumDoll = tValueOfEnterprise
                            '        'End If
                            '        'A1301
                            '    End If
                            '    sum += e_prty.Impact
                            '    'numTriggered += 1
                            '    'A1301 ===
                            '    'If tTruncateImpacts AndAlso sum > 1 Then
                            '    '    sum = 1
                            '    'End If
                            '    'A1301 ==
                            '
                            '    eventTriggered = True
                            'End If
                        End If

                        If Not isExclusiveEvents AndAlso eventTriggered AndAlso Not se.Fired Then se.Rand = -1 ' so that the generated random number would not be shown in the "Step Data" screen, just show blank cell in this case
                        'If Not curStep.Events.ContainsKey(e.ID) Then curStep.Events.Add(e.ID, se)

                        'curStep.Events.Add(se)
                        Events.Add(se)
                    Next
                    'End If


                    'EF: if the first event has an impact of 80% then the remaining value is 20%.  If the second event fires has an impact of 50%, then that is 50% of 20% or 10% so the remaining is 10%, etc.                    
                    For Each Obj As clsNode In IH.TerminalNodes
                        Dim tObjectiveConsequence As Double = 1
                        'For Each tSe As clsSimulationEvent In curStep.Events
                        For Each tSe As clsSimulationEvent In Events
                            If tSe.Fired Then
                                'For k As Integer = 0 To tSe.Consequences.Values.Count - 1
                                '    If tSe.Consequences.Keys(k) = Obj.NodeID Then
                                '        tObjectiveConsequence = tObjectiveConsequence - tObjectiveConsequence * tSe.Consequences.Values(k)
                                '    End If
                                'Next

                                Dim tAlt As clsNode = EventsDic(tSe.EventID)
                                If tAlt.SimulatedConsequences.ContainsKey(Obj.NodeID) Then
                                    tObjectiveConsequence = tObjectiveConsequence - tObjectiveConsequence * tAlt.SimulatedConsequences(Obj.NodeID)
                                End If

                                'For k As Integer = 0 To tSe.Consequences.Values.Count - 1
                                '    Dim node As clsNode = IH.GetNodeByID(tSe.Consequences.Keys(k))
                                '    If node IsNot Nothing Then
                                '        tSe.SimulatedImpact += tSe.Consequences.Values(k) * node.UnnormalizedPriority
                                '    End If
                                'Next
                            End If
                            'sum += tSe.SimulatedImpact
                        Next
                        'curStep.NetConsequences.Add(Obj.NodeID, 1 - tObjectiveConsequence)
                        'TODO: remove NetConsequence
                        NetConsequences.Add(Obj.NodeID, 1 - tObjectiveConsequence)
                        Obj.SumNetConsequences += 1 - tObjectiveConsequence
                    Next

                    ' get sums of event consequences SumConsequences
                    'For Each tSe As clsSimulationEvent In curStep.Events
                    For Each tSe As clsSimulationEvent In Events
                        'If tSe.Consequences IsNot Nothing Then
                        '    For k As Integer = 0 To tSe.Consequences.Values.Count - 1
                        '        'If Not curStep.SumConsequences.ContainsKey(tSe.Consequences.Keys(k)) Then curStep.SumConsequences.Add(tSe.Consequences.Keys(k), 0)
                        '        'curStep.SumConsequences(tSe.Consequences.Keys(k)) += tSe.Consequences.Values(k)
                        '        If Not SumConsequences.ContainsKey(tSe.Consequences.Keys(k)) Then SumConsequences.Add(tSe.Consequences.Keys(k), 0)
                        '        SumConsequences(tSe.Consequences.Keys(k)) += tSe.Consequences.Values(k)
                        '    Next
                        'End If
                        Dim tAlt As clsNode = EventsDic(tSe.EventID)
                        For Each kvp As KeyValuePair(Of Integer, Double) In tAlt.SimulatedConsequences
                            If Not SumConsequences.ContainsKey(kvp.Key) Then SumConsequences.Add(kvp.Key, 0)
                            SumConsequences(kvp.Key) += kvp.Value
                        Next
                    Next

                    'For Each tSe As clsSimulationEvent In curStep.Events
                    For Each tSe As clsSimulationEvent In Events
                        'If tSe.Consequences IsNot Nothing Then
                        '    For k As Integer = 0 To tSe.Consequences.Values.Count - 1
                        '        'If (curStep.SumConsequences(tSe.Consequences.Keys(k)) <> 0) Then
                        '        '    tSe.Consequences(tSe.Consequences.Keys(k)) = (tSe.Consequences.Values(k) / curStep.SumConsequences(tSe.Consequences.Keys(k))) * curStep.NetConsequences(tSe.Consequences.Keys(k))
                        '        If (SumConsequences(tSe.Consequences.Keys(k)) <> 0) Then
                        '            tSe.Consequences(tSe.Consequences.Keys(k)) = (tSe.Consequences.Values(k) / SumConsequences(tSe.Consequences.Keys(k))) * NetConsequences(tSe.Consequences.Keys(k))
                        '        Else
                        '            tSe.Consequences(tSe.Consequences.Keys(k)) = 0
                        '        End If
                        '    Next
                        'End If

                        Dim tAlt As clsNode = EventsDic(tSe.EventID)
                        For k As Integer = 0 To tAlt.SimulatedConsequences.Values.Count - 1
                            Dim key As Integer = tAlt.SimulatedConsequences.Keys(k)
                            If SumConsequences(key) <> 0 Then
                                tAlt.SimulatedConsequences(key) = tAlt.SimulatedConsequences(key) / SumConsequences(key) * NetConsequences(key)
                            Else
                                tAlt.SimulatedConsequences(key) = 0
                            End If

                        Next
                    Next

                    'For Each tSe As clsSimulationEvent In curStep.Events
                    For Each tSe As clsSimulationEvent In Events
                        If tSe.Fired Then
                            'If tSe.Consequences IsNot Nothing Then
                            '    For k As Integer = 0 To tSe.Consequences.Values.Count - 1
                            '        Dim node As clsNode = ImpactDic(tSe.Consequences.Keys(k))
                            '        If node IsNot Nothing Then
                            '            'tSe.SimulatedImpact += tSe.Consequences.Values(k) * node.UnnormalizedPriority
                            '            EventsDic(tSe.EventID).SimulatedAltImpact += tSe.Consequences.Values(k) * node.UnnormalizedPriority
                            '        End If
                            '    Next
                            'End If
                            Dim tAlt As clsNode = EventsDic(tSe.EventID)
                            Dim simImpact As Double = 0
                            For Each kvp As KeyValuePair(Of Integer, Double) In tAlt.SimulatedConsequences
                                Dim node As clsNode = ImpactDic(kvp.Key)
                                If node IsNot Nothing Then
                                    'tSe.SimulatedImpact += tSe.Consequences.Values(k) * node.UnnormalizedPriority

                                    'tAlt.SimulatedAltImpact += kvp.Value * node.UnnormalizedPriority
                                    simImpact += kvp.Value * node.UnnormalizedPriority
                                End If
                            Next
                            tAlt.SimulatedAltImpact += If(simImpact > 1, 1, simImpact)
                        End If
                    Next

                    curStep.Total = 0
                    'For Each kvp As KeyValuePair(Of Integer, Double) In curStep.NetConsequences
                    For Each kvp As KeyValuePair(Of Integer, Double) In NetConsequences
                        Dim node As clsNode = ImpactDic(kvp.Key)
                        If node IsNot Nothing Then
                            curStep.Total += kvp.Value * node.UnnormalizedPriority
                        End If
                    Next

                    If tLimitImpactsTo100 Then

                        'calculate simulated impact
                        'Normalize                                
                        'Dim sumConsequencesForThisObjective As New Dictionary(Of Integer, Double)
                        'For Each evt As clsSimulationEvent In curStep.Events
                        '    If evt.Fired AndAlso evt.Consequences IsNot Nothing Then
                        '        For Each cons As KeyValuePair(Of Integer, Double) In evt.Consequences
                        '            If Not sumConsequencesForThisObjective.ContainsKey(cons.Key) Then sumConsequencesForThisObjective.Add(cons.Key, 0)
                        '            sumConsequencesForThisObjective(cons.Key) += cons.Value
                        '        Next
                        '    End If
                        'Next

                        'For Each tSe As clsSimulationEvent In curStep.Events
                        '    If tSe.Fired Then
                        '        For k As Integer = 0 To tSe.Consequences.Values.Count - 1
                        '            'tSe.Consequences(tSe.Consequences.Keys(k)).X = tSe.Consequences.Values(k).X

                        '            If sumConsequencesForThisObjective.ContainsKey(tSe.Consequences.Keys(k)) AndAlso sumConsequencesForThisObjective(tSe.Consequences.Keys(k)) > 1 Then
                        '                tSe.Consequences(tSe.Consequences.Keys(k)) = tSe.Consequences.Values(k) / sumConsequencesForThisObjective(tSe.Consequences.Keys(k))
                        '            End If

                        '            tSe.SimulatedImpact += tSe.Consequences.Values(k) '* tSe.Consequences.Values(k).Y
                        '        Next
                        '    End If
                        'Next

                    End If

                    If i < clsLEC.NumStoredSteps Then curStep.Events = Events

                    'curStep.Total = sum
                    'curStep.Total = Math.Round(sum, 6)
                    'curStep.ExpectedValue = sum / numTriggered
                    If curStep.Total > maxValue Then maxValue = curStep.Total

                    simData.Add(curStep)
                Next

                dMaxLossValue = maxValue
            End If

            '' calculate simulated event likelihoods and event impacts
            For Each alt As clsNode In PM.AltsHierarchy(PM.ActiveAltsHierarchy).TerminalNodes.Where(Function(a) a.Enabled).ToList
                alt.SimulatedAltLikelihood = alt.NumFired / tNumSimulations ' EventSimulatedLikelihood(simData, alt.NodeID, tNumSimulations)
                alt.SimulatedAltImpact = If(alt.NumFired = 0, 0, alt.SimulatedAltImpact / alt.NumFired) ' EventSimulatedImpact(simData, alt.NodeID, tNumSimulations)
                        If tNumSimulations > 1 Then
                            For Each NodeID As Integer In LikelihoodDic.Keys
                                If alt.SimulatedVulnerabilities.ContainsKey(NodeID) Then
                                    alt.SimulatedVulnerabilities(NodeID) /= tNumSimulations
                                End If
                            Next
                        End If
            Next

            ' calculate source likelihoods
            For Each source As clsNode In PM.Hierarchy(ECHierarchyID.hidLikelihood).TerminalNodes.Where(Function(a) a.Enabled).ToList
                source.SimulatedPriority = CDbl(source.NumFired / tNumSimulations)
            Next

            ' calculate average impacts
            For Each obj As clsNode In PM.Hierarchy(ECHierarchyID.hidImpact).TerminalNodes.Where(Function(a) a.Enabled).ToList
                obj.SimulatedPriority = obj.SumNetConsequences / tNumSimulations
            Next

            PrioritiesCacheManager.Enabled = oldCacheValue
            If Not oldCacheValue Then
                PM.CalculationsManager.PrioritiesCacheManager.ClearCache()
            End If

            Debug.Print("Calculations count = " + clsCalculationsManager.CalculateCount.ToString)

            ' renormalize X-axis based on scrum discussion on Aug 07, 2018
            If (dMaxLossValue > 1) Then 
                For Each tStep as clsSimulationData In simData 
                    tStep.Total /= dMaxLossValue
                Next
                ' and reset max value
                dMaxLossValue = 1
            End If 

            MiscFuncs.PrintDebugInfo("-- SimulateLossExceedance end: ")
            Return simData
        End Function

        Private Function GetCalcTarget(UserID As Integer, UserEmail As String) As clsCalculationTarget
            Dim CalcTarget As clsCalculationTarget = Nothing
            If IsCombinedUserID(UserID) Then
                Dim CG As clsCombinedGroup = PM.CombinedGroups.GetCombinedGroupByUserID(UserID)
                If CG IsNot Nothing Then
                    CalcTarget = New clsCalculationTarget(CalculationTargetType.cttCombinedGroup, CG)
                End If
            Else
                Dim user As clsUser = PM.GetUserByEMail(UserEmail)
                If user IsNot Nothing Then
                    CalcTarget = New clsCalculationTarget(CalculationTargetType.cttUser, user)
                    PM.StorageManager.Reader.LoadUserData(user)
                End If
            End If
            Return CalcTarget
        End Function

        Public Sub GetRiskDataWRTEvents(UserID As Integer, UserEmail As String, tHierarchyID As ECHierarchyID, wrtNode As clsNode, ByRef DataDict As Dictionary(Of Integer, List(Of AlternativeRiskDataDataContract)), ControlsUsageMode As ControlsUsageMode)
            With PM
                Dim oldHierarchyID As ECHierarchyID = CType(.ActiveHierarchy, ECHierarchyID)
                .ActiveHierarchy = tHierarchyID

                '.StorageManager.Reader.LoadUserData()
                'TODO: AC - calculate Sources or Objectives priorities WRT the Event
                Dim CalcTarget As clsCalculationTarget = GetCalcTarget(UserID, UserEmail)

                Dim oldUseReduction As ControlsUsageMode = .CalculationsManager.ControlsUsageMode
                .CalculationsManager.ControlsUsageMode = ControlsUsageMode

                .CalculationsManager.GetRiskDataWRTNode(UserID, UserEmail, wrtNode.NodeGuidID, ControlsUsageMode) 'A1064

                For Each tAlt As clsNode In .AltsHierarchy(.ActiveAltsHierarchy).TerminalNodes.Where(Function (a) a.Enabled).ToList
                    DataDict.Add(tAlt.NodeID, New List(Of AlternativeRiskDataDataContract))
                Next

                Dim calculatedCount As Integer = 0
                For Each node As clsNode In .Hierarchy(tHierarchyID).TerminalNodes
                    If node.NodeID > 0 OrElse .Hierarchy(tHierarchyID).Nodes.Count = 1 Then 'Skip Goal node OrElse use only Goal
                        If calculatedCount Mod 5 > 0 OrElse Not IsCancellationPending() Then 'if not cancellation pending check for every decade of calculations
                            '.CalculationsManager.Calculate(CalcTarget, node, tHierarchyID)
                            Dim contributedAlternatives As List(Of clsNode) = node.GetContributedAlternatives
                            For Each alt As clsNode In .AltsHierarchy(.ActiveAltsHierarchy).TerminalNodes.Where(Function(a) a.Enabled).ToList
                                If DataDict.ContainsKey(alt.NodeID) AndAlso contributedAlternatives.Contains(alt) Then
                                    Dim tEventData As AlternativeRiskDataDataContract = New AlternativeRiskDataDataContract
                                    tEventData.AlternativeID = node.NodeGuidID
                                    Dim wrtPriority As Double = node.Judgments.Weights.GetUserWeights(COMBINED_USER_ID, PM.CalculationsManager.SynthesisMode, PM.CalculationsManager.IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(alt.NodeID)
                                    'If alt.NodeGuidID.Equals(EventID) Then
                                    If tHierarchyID = ECHierarchyID.hidLikelihood Then
                                        'tEventData.LikelihoodValue = alt.WRTRelativeAPriority ' Vulnerability of Event (likelihood of the event wrt the source)
                                        tEventData.LikelihoodValue = wrtPriority
                                    Else
                                        tEventData.ImpactValue = wrtPriority
                                    End If
                                    'End If
                                    'res.Add(tEventData)
                                    DataDict(alt.NodeID).Add(tEventData)
                                End If
                            Next

                            calculatedCount += 1
                        End If
                    End If
                Next

                .ActiveHierarchy = oldHierarchyID
                .CalculationsManager.ControlsUsageMode = oldUseReduction
            End With
        End Sub

        Public Sub GetRiskDataForSources(UserID As Integer, UserEmail As String, ByVal wrtNode As clsNode, ByRef DataDict As List(Of clsSimulationSource), tControlsUsageMode As ControlsUsageMode)
            With PM
                '.StorageManager.Reader.LoadUserData()
                'TODO: AC - calculate Sources or Objectives priorities WRT the Event
                Dim CalcTarget As clsCalculationTarget = GetCalcTarget(UserID, UserEmail)

                Dim oldUseReduction As ControlsUsageMode = .CalculationsManager.ControlsUsageMode
                .CalculationsManager.ControlsUsageMode = tControlsUsageMode

                .CalculationsManager.Calculate(CalcTarget, wrtNode, ECHierarchyID.hidLikelihood)

                Dim calculatedCount As Integer = 0
                For Each node As clsNode In .Hierarchy(ECHierarchyID.hidLikelihood).GetRespectiveTerminalNodes(wrtNode)
                    If node.NodeID > 0 OrElse .Hierarchy(ECHierarchyID.hidLikelihood).Nodes.Count = 1 Then 'Skip Goal node OrElse use only Goal
                        If calculatedCount Mod 50 > 0 OrElse Not IsCancellationPending() Then 'if not cancellation pending check for every decade of calculations

                            Dim tSourceData As New clsSimulationSource With {.ID = node.NodeID}
                            tSourceData.Priority = node.UnnormalizedPriority
                            DataDict.Add(tSourceData)

                            calculatedCount += 1
                        End If
                    End If
                Next

                .CalculationsManager.ControlsUsageMode = oldUseReduction
            End With
        End Sub

        'Public Function InitSimulatedValues(tControlsUsageMode As ControlsUsageMode, IsOpportunityModel As Boolean) As List(Of clsSimulationData)
        '    'Dim riskData As RiskDataWRTNodeDataContract = PM.CalculationsManager.GetRiskDataWRTNode(COMBINED_USER_ID, "", Guid.Empty, tControlsUsageMode)
        '    'Dim riskDataWithoutControls As RiskDataWRTNodeDataContract = PM.CalculationsManager.GetRiskDataWRTNode(COMBINED_USER_ID, "", Guid.Empty, ControlsUsageMode.DoNotUse)

        '    'Dim SelectedEventIDs As List(Of Integer) = New List(Of Integer)
        '    'For Each tAlt As clsNode In PM.AltsHierarchy(PM.ActiveAltsHierarchy).TerminalNodes.Where(Function (a) a.Enabled).ToList
        '    '    SelectedEventIDs.Add(tAlt.NodeID)
        '    'Next
        '    Dim SelectedEventIDs As List(Of Integer) = PM.AltsHierarchy(PM.ActiveAltsHierarchy).TerminalNodes.Where(Function(a) a.Enabled).Select(Function(x) x.NodeID).ToList


        '    Dim tNumSimulations As Integer = NumberOfSimulations
        '    Dim dMaxValue As Double = 0

        '    Dim simData As List(Of clsSimulationData) = SimulateLossExceedance(COMBINED_USER_ID, "", SelectedEventIDs, clsLEC._SIMULATE_SOURCES, False, tControlsUsageMode, dMaxValue, New Random(RandSeed), True, False, 0, Nothing, Guid.Empty, False, Guid.Empty, IsOpportunityModel)

        '    '' calculate simulated event likelihoods and event impacts
        '    For Each alt As clsNode In PM.AltsHierarchy(PM.ActiveAltsHierarchy).TerminalNodes.Where(Function (a) a.Enabled).ToList
        '        For Each seID As Integer In SelectedEventIDs
        '            If alt.NodeID = seID Then
        '                alt.SimulatedAltLikelihood = alt.NumFired / tNumSimulations ' EventSimulatedLikelihood(simData, alt.NodeID, tNumSimulations)
        '                alt.SimulatedAltImpact = If(alt.NumFired = 0, 0, alt.SimulatedAltImpact / alt.NumFired) ' EventSimulatedImpact(simData, alt.NodeID, tNumSimulations)
        '                Exit For
        '            End If
        '        Next
        '    Next

        '    ' calculate source likelihoods
        '    For Each source As clsNode In PM.Hierarchy(ECHierarchyID.hidLikelihood).TerminalNodes.Where(Function (a) a.Enabled).ToList
        '        'Dim tNumFired As Integer = 0
        '        'For Each d As clsSimulationData In simData
        '        '    'TODO: simulations
        '        '    '    If d.Sources IsNot Nothing Then
        '        '    '        For Each se As KeyValuePair(Of Integer, clsSimulationSource) In d.Sources
        '        '    '            If se.Value.Fired AndAlso se.Value.ID = source.NodeID Then tNumFired += 1
        '        '    '        Next
        '        '    '    End If
        '        'Next
        '        source.SimulatedPriority = CDbl(source.NumFired / tNumSimulations)
        '    Next

        '    ' calculate average impacts
        '    For Each obj As clsNode In PM.Hierarchy(ECHierarchyID.hidImpact).TerminalNodes.Where(Function (a) a.Enabled).ToList
        '        'Dim tSumConsequence As Double = 0
        '        'If simData IsNot Nothing Then
        '        '    For Each d As clsSimulationData In simData
        '        '        'TODO: simulated value calculations
        '        '        'If d.NetConsequences IsNot Nothing AndAlso d.NetConsequences.ContainsKey(obj.NodeID) Then
        '        '        '    tSumConsequence += d.NetConsequences(obj.NodeID)
        '        '        'End If
        '        '    Next
        '        'End If
        '        'obj.SimulatedPriority = tSumConsequence / tNumSimulations
        '        obj.SimulatedPriority = obj.SumNetConsequences / tNumSimulations
        '    Next

        '    Return simData
        'End Function

        Public Function GetSimulatedRisk(ByVal tControlsUsageMode As ControlsUsageMode, IsOpportunityModel As Boolean) As Double
            'Dim simData As List(Of clsSimulationData) = InitSimulatedValues(tControlsUsageMode, IsOpportunityModel)
            Dim dMaxLossValue As Double
            Dim simData As List(Of clsSimulationData) = SimulateLossExceedance(COMBINED_USER_ID, "", clsLEC._SIMULATE_SOURCES, False, tControlsUsageMode, dMaxLossValue, New Random(RandSeed), True, False, 0, Nothing, Guid.Empty, False, Guid.Empty, IsOpportunityModel)

            Dim retVal As Double = 0
            Dim tNumSimulations As Integer = NumberOfSimulations

            ' calculate simulated risk
            For Each alt As clsNode In PM.AltsHierarchy(PM.ActiveAltsHierarchy).TerminalNodes.Where(Function(a) a.Enabled).ToList
                'alt.RiskValue = EventSimulatedLikelihood(simData, alt.NodeID, tNumSimulations) * EventSimulatedImpact(simData, alt.NodeID, tNumSimulations)
                alt.RiskValue = alt.SimulatedAltLikelihood * alt.SimulatedAltImpact
                retVal += alt.RiskValue
            Next

            Return retVal
        End Function

        Private Function GetSimulationEventByID(node As clsNode, simEvents As List(Of clsEventData), Id As Integer) As clsEventData
            If node.Tag IsNot Nothing AndAlso TypeOf node.Tag Is clsEventData Then Return CType(node.Tag, clsEventData)

            For Each e As clsEventData In simEvents
                If e.ID = Id Then
                    node.Tag = e
                    Return e
                End If
            Next
            Return Nothing
        End Function

        Protected Overrides Sub Finalize()
            MyBase.Finalize()
        End Sub

        'Private ReadOnly Property EventGroupName(AltID As Guid) As String
        '    Get
        '        Return CStr(PM.Attributes.GetAttributeValue(ATTRIBUTE_LEC_EVENT_EXCL_GROUP_NAME_ID, AltID))
        '    End Get
        'End Property
        '
        'Private ReadOnly Property EventGroupPrecedence(AltID As Guid) As Integer
        '    Get
        '        Return CInt(PM.Attributes.GetAttributeValue(ATTRIBUTE_LEC_EVENT_EXCL_GROUP_PRECEDENCE_ID, AltID))
        '    End Get
        'End Property

        'Public Function EventSimulatedLikelihood(simData As List(Of clsSimulationData), AltID As Integer, tNumSimulations As Integer) As Double
        '    Dim tNumFired As Integer = 0
        '    For Each d As clsSimulationData In simData
        '        'TODO: simulations
        '        'For Each se As clsSimulationEvent In d.Events
        '        '    If se.EventID = AltID AndAlso se.Fired Then tNumFired += 1
        '        'Next
        '    Next
        '    If tNumSimulations = 0 Then tNumSimulations = 1
        '    Return tNumFired / tNumSimulations
        'End Function

        'Public Function EventSimulatedImpact(simData As List(Of clsSimulationData), AltID As Integer, tNumSimulations As Integer) As Double
        '    Dim tImp As Double = 0
        '    Dim tNumFired As Integer = 0
        '    For Each d As clsSimulationData In simData
        '        'TODO: simulations
        '        'For Each se As clsSimulationEvent In d.Events
        '        '    If se.EventID = AltID AndAlso se.Fired Then
        '        '        tImp += se.SimulatedImpact
        '        '        tNumFired += 1
        '        '    End If
        '        'Next
        '    Next
        '    If tNumSimulations = 0 Then tNumSimulations = 1
        '    If tNumFired = 0 Then tNumFired = 1
        '    'Return tImp / tNumSimulations
        '    Return tImp / tNumFired
        'End Function

        'Public Function EventSimulatedVulnerability(simData As List(Of clsSimulationData), AltID As Integer, SourceID As Integer, tNumSimulations As Integer) As Double
        '    Dim tNumFired As Integer = 0
        '    For Each d As clsSimulationData In simData
        '        TODO: simulations
        '        For Each se As clsSimulationEvent In d.Events
        '            If se.EventID = AltID AndAlso se.Fired AndAlso se.Vulnerabilities.ContainsKey(SourceID) Then
        '                tNumFired += se.Vulnerabilities(SourceID)
        '            End If
        '        Next
        '    Next
        '    If tNumSimulations = 0 Then tNumSimulations = 1
        '    Return tNumFired / tNumSimulations
        'End Function


        'Public Function EventSimulatedConsequence(simData As List(Of clsSimulationData), AltID As Integer, ObjID As Integer, tNumSimulations As Integer) As Double
        '    Dim retVal As Double = 0
        '    For Each d As clsSimulationData In simData
        '        'TODO: simulations
        '        'For Each se As clsSimulationEvent In d.Events
        '        '    If se.EventID = AltID AndAlso se.Fired AndAlso se.Consequences.ContainsKey(ObjID) Then
        '        '        retVal += se.Consequences(ObjID)
        '        '    End If
        '        'Next
        '    Next
        '    If tNumSimulations = 0 Then tNumSimulations = 1
        '    Return retVal / tNumSimulations
        'End Function

    End Class

End Namespace
