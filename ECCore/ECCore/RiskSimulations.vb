Option Strict On
Imports System.Linq
Imports Canvas
Imports ECCore
Imports System.Collections.Concurrent
Imports System.Threading.Tasks

Namespace ECCore

    <Serializable>
    Public Class EventGroupData
        Public Property EventGuidID As Guid
        Public Property EventIntID As Integer
        Public Property Precedence As Integer = Integer.MaxValue
    End Class

    <Serializable>
    Public Enum GroupBehaviour
        Independent = 0
        MutuallyExclusive = 1
        MutuallyExclusiveExhaustive = 2
    End Enum

    <Serializable>
    Public Enum GroupRule
        None = 0
        ExactlyNFired = 1
        AtLeastNFired = 2
        AtMostNFired = 3
        AllFired = 4
    End Enum

    <Serializable>
    Public Class EventsGroup
        Public Property ID As Guid = Guid.NewGuid
        Public Property Name As String = ""
        Public Property Enabled As Boolean = True 'todo AC - save to db
        Public Property Behaviour As GroupBehaviour = GroupBehaviour.Independent
        Public Property Rule As GroupRule = GroupRule.None
        Public Property RuleParameterValue As Integer = 1
        Public Property TempFiredID As Integer = -1
        Public Property TempFiredSourceID As Integer = -1
        Public Property TempFiredPrecedence As Integer = Integer.MaxValue
        Public Property TempDidNotFiredCount As Integer = 0
        Public Property Events As New List(Of EventGroupData)

        Public Function AddEvent(e As clsNode, Optional Precedence As Integer = Integer.MaxValue) As EventGroupData
            DeleteEvent(e)
            Dim res As New EventGroupData
            res.EventGuidID = e.NodeGuidID
            res.EventIntID = e.NodeID
            res.Precedence = Precedence
            Events.Add(res)
            Return res
        End Function

        Public Function DeleteEvent(e As clsNode) As Boolean
            Return Events.RemoveAll(Function(x) x.EventGuidID.Equals(e.NodeGuidID)) > 0
        End Function

        Public Function DeleteEvent(id As Guid) As Boolean
            Return Events.RemoveAll(Function(x) x.EventGuidID.Equals(id)) > 0
        End Function

        Public Overloads Function GetEventData(id As Integer) As EventGroupData
            Return Events.FirstOrDefault(Function(e) e.EventIntID = id)
        End Function

        Public Overloads Function GetEventData(id As Guid) As EventGroupData
            Return Events.FirstOrDefault(Function(e) e.EventGuidID.Equals(id))
        End Function
    End Class

    <Serializable>
    Public Class EventsGroups
        Public Sub New(ProjectManager As clsProjectManager)
            Me.ProjectManager = ProjectManager
        End Sub

        Public ReadOnly Property ProjectManager As clsProjectManager

        Public Property Groups As New List(Of EventsGroup)

        Public Function AddGroup(name As String) As EventsGroup
            Dim res As New EventsGroup
            res.Name = name
            Groups.Add(res)
            Return res
        End Function

        Public Function DeleteGroup(id As Guid) As Boolean
            Return Groups.RemoveAll(Function(x) x.ID.Equals(id)) > 0
        End Function

        Public Function GetGroup(id As Guid) As EventsGroup
            Return Groups.FirstOrDefault(Function(x) x.ID.Equals(id))
        End Function

        Public Function GetGroupsWithEvent(EventID As Guid) As List(Of EventsGroup)
            Return Groups.Where(Function(x) x.Events.Select(Function(a) a.EventGuidID).Contains(EventID)).ToList
        End Function

        Public Function GetGroup(name As String) As EventsGroup
            name = name.Trim.ToLower
            Return Groups.FirstOrDefault(Function(x) x.Name.Trim.ToLower = name)
        End Function

        Public Function GetEventGroup(GroupID As Guid) As EventsGroup
            For Each group As EventsGroup In Groups                
                If group.ID = GroupID Then Return group
            Next
            Return Nothing
        End Function

        Public ReadOnly Property GroupName(e As clsNode) As String ' Riskion Event Groups for LEC
            Get
                'Dim tGrp As EventsGroup = GetEventGroup(e.NodeGuidID)
                'If tGrp IsNot Nothing Then Return tGrp.Name
                'Return ""
                Dim retVal As String = ""
                Dim list As List(Of EventsGroup) = GetGroupsWithEvent(e.NodeGuidID)
                For Each item As EventsGroup In list
                    retVal += If(retVal = "", "", ", ") + item.Name
                Next
                Return If(retVal = "", "- None -", retVal)
            End Get
            'Set(value As String)
            '    Dim tSave As Boolean = False

            '    Dim tGrp As EventsGroup = GetEventGroup(e.NodeGuidID)
            '    If tGrp IsNot Nothing Then 
            '        tGrp.DeleteEvent(e.NodeGuidID)
            '        tSave = True
            '    End If
            '    If (value.Trim <> "") Then
            '        tGrp = GetGroup(value)
            '        If tGrp Is Nothing Then 
            '            tGrp = AddGroup(value)
            '        End If

            '        tGrp.AddEvent(e)

            '        tSave = True
            '    End If
            '    If tSave Then Save()
            'End Set
        End Property

        Public Property GroupPrecedence(e As clsNode) As Integer
            Get
                Dim tGrp As EventsGroup = GetEventGroup(e.NodeGuidID)
                If tGrp IsNot Nothing Then Return tGrp.GetEventData(e.NodeGuidID).Precedence
                Return Integer.MaxValue
            End Get
            Set(value As Integer)
                Dim tSave As Boolean = False

                Dim tGrp As EventsGroup = GetEventGroup(e.NodeGuidID)
                If tGrp IsNot Nothing Then
                    tGrp.GetEventData(e.NodeGuidID).Precedence = value
                    tSave = True
                End If

                If tSave Then Save()
            End Set
        End Property

        Public Function Save() As Boolean
            'todo Save source groups
            Return ProjectManager.StorageManager.Writer.SaveEventsGroups
        End Function
    End Class

    <Serializable>
    Friend Class RiskValuesInfo
        Public Property CalculatedLikelihood As Double
        Public Property CalculatedImpact As Double
        Public Property SimulatedLikelihood As Double
        Public Property SimulatedImpact As Double

    End Class

    <Serializable>
    Public Class TupleBDD
        Public Item1 As Boolean
        Public Item2 As Double
        Public Item3 As Double
    End Class

    <Serializable>
    Public Class TupleIDD
        Public Item1 As Integer
        Public Item2 As Double
        Public Item3 As Double
        Public Item4 As Double
        Public Item5 As Double
        Public Item6 As Double
        Public Item7 As Boolean 'Fired
    End Class

    <Serializable>
    Public Class SimulationStepInfo
        ''' <summary>
        ''' Information about sources on each step. Key = source ID. Value = Tuple, where 1st item is whether source was fired, 2nd item is computed source likelihood and 3rd item is random value that was generated.
        ''' </summary>
        ''' <returns></returns>
        Public Property SourcesInfo As New Dictionary(Of Integer, TupleBDD)
        ''' <summary>
        ''' Information about fired events on each step. Key = event ID. Value = Tuple, where 1st item is source ID even fired from, 2nd item is computed vulnerability, 3rd item is random number that was generated, 4th is impact of event, 5th is risk value = vulnerability * impact, 6th is risk = vulnerability * priority of source * impact
        ''' </summary>
        ''' <returns></returns>
        Public Property FiredEventsInfo As New Dictionary(Of Integer, TupleIDD)
        ''' <summary>
        ''' Impact value for current step
        ''' </summary>
        ''' <returns></returns>
        Public Property Impact As Double

    End Class

    <Serializable>
    Partial Public Class RiskSimulations
        Public ReadOnly Property ProjectManager As clsProjectManager

        Public Property UserID As Integer = -1

        Public Property UseShuffling As Boolean = True

        Public Property WRTNode As clsNode = Nothing
        Private likelihoodWRTNode As clsNode
        Private impactWRTNode As clsNode
        Public Property UseRelativeLikelihoods As Boolean = False 'A1545

        Public Property UseSourceGroups As Boolean 
            Get
                Return CBool(ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_LEC_SOURCE_GROUPS_ENABLED_ID, UNDEFINED_USER_ID))
            End Get
            Set(value As Boolean)
                With ProjectManager
                    .Attributes.SetAttributeValue(ATTRIBUTE_LEC_SOURCE_GROUPS_ENABLED_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtBoolean, value, Guid.Empty, Guid.Empty)
                    .Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID, UNDEFINED_USER_ID)
                End With
            End Set
        End Property        

        Public Property UseEventsGroups As Boolean 
            Get
                Return CBool(ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_LEC_EVENT_EXCL_GROUPS_ENABLED_ID, UNDEFINED_USER_ID))
            End Get
            Set(value As Boolean)
                With ProjectManager
                    .Attributes.SetAttributeValue(ATTRIBUTE_LEC_EVENT_EXCL_GROUPS_ENABLED_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtBoolean, value, Guid.Empty, Guid.Empty)
                    .Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID, UNDEFINED_USER_ID)
                End With
            End Set
        End Property

        Private terminalLikelihoods As List(Of clsNode)
        Private terminalImpacts As List(Of clsNode)
        Private terminalEvents As List(Of clsNode)


        Public Property StoreStepValues As Boolean = False
        Public Property NumberOfStepsToStore As Integer = 250
        ''' <summary>
        ''' Information about simulation step. Key = step number. Value = simulation information.
        ''' </summary>
        ''' <returns></returns>
        Public Property StepsInfo As Dictionary(Of Integer, SimulationStepInfo)

        Public Property AverageLossValue As Double = 0 'A1530
        Public Property RedLineValue As Double = 0.2
        Public Property RedLineIntersection As Double = -1
        Public Property GreenLineValue As Double = 0.2
        Public Property GreenLineIntersection As Double = -1
        Public Property GenerateChartValues As Boolean = False
        Public Property DataPointsCount As Integer = 50
        Public Property LossExceedanceValues As New SortedDictionary(Of Double, Double)
        Public Property FrequencyValues As New Dictionary(Of Double, Integer)

        Public Property CVARLevel As Double = 0.95
        Private mCVARValue As Double
        Public ReadOnly Property CVARValue As Double
            Get
                Return mCVARValue
            End Get
        End Property

        Private ReadOnly Property hEvents As clsHierarchy
            Get
                Return ProjectManager.ActiveAlternatives
            End Get
        End Property

        Private ReadOnly Property hLikelihood As clsHierarchy
            Get
                Return ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood)
            End Get
        End Property

        Private ReadOnly Property hImpact As clsHierarchy
            Get
                Return ProjectManager.Hierarchy(ECHierarchyID.hidImpact)
            End Get
        End Property


        ''' <summary>
        ''' Priorities of sources in Likelihood hierarchy by NodeID.
        ''' </summary>
        Private computedLikelihoods As New Dictionary(Of Integer, Double)

        ''' <summary>
        ''' Computed vulnerabilities. Key = covering source ID. Value = dictionary, where key = event ID, value = priority of event wrt covering source.
        ''' </summary>
        Private computedVulnerabilities As New Dictionary(Of Integer, Dictionary(Of Integer, Double))

        ''' <summary>
        ''' Priorities of objectives in Impact hierarchy by NodeID.
        ''' </summary>
        Private computedPriorities As New Dictionary(Of Integer, Double)

        ''' <summary>
        ''' Computed consequences. Key = covering objective ID. Value = dictionary where key = event ID, value = priority of event wrt covering objective.
        ''' </summary>
        Private computedConsequences As New Dictionary(Of Integer, Dictionary(Of Integer, Double))

        Private contributedEventsForSources As Dictionary(Of Integer, List(Of Integer))
        Private contributedEventsForObjectives As Dictionary(Of Integer, List(Of Integer))
        Private uncontributedEvents As List(Of clsNode)

        Private mSimulatedVulnerabilities As New Dictionary(Of Integer, Double)
        Public ReadOnly Property SimulatedVulnerabilities As Dictionary(Of Integer, Double)
            Get
                Return mSimulatedVulnerabilities
            End Get
        End Property

        Private mSimulatedConsequences As New Dictionary(Of Integer, Dictionary(Of Integer, Double))
        Public ReadOnly Property SimulatedConsequences As Dictionary(Of Integer, Dictionary(Of Integer, Double))
            Get
                Return mSimulatedConsequences
            End Get
        End Property

        Private simulatedConsequencesConcurrent As ConcurrentDictionary(Of Integer, Dictionary(Of Integer, Double))

        Private controlsCoefficientsForVulnerabilities As New Dictionary(Of Integer, Dictionary(Of Integer, Double))

        Private _RandomSeed As Integer = Integer.MinValue
        Public Property RandomSeed As Integer
            Get
                If _RandomSeed = Integer.MinValue Then 
                    _RandomSeed = CInt(ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_LEC_RAND_SEED_ID, UNDEFINED_USER_ID))
                    If _RandomSeed < 0 Then
                        _RandomSeed = DateTime.Now.Millisecond
                        SaveSeed(_RandomSeed)
                    End If
                End If
                Return _RandomSeed
            End Get
            Set(value As Integer)
                    If _RandomSeed <> value AndAlso value >= 0 Then
                        _RandomSeed = value
                        SaveSeed(_RandomSeed)
                    End If
            End Set
        End Property

        Private Sub SaveSeed(value As Integer)
            With ProjectManager
                .Attributes.SetAttributeValue(ATTRIBUTE_LEC_RAND_SEED_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtLong, value, Guid.Empty, Guid.Empty)
                .Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID, UNDEFINED_USER_ID)
            End With
        End Sub

        Private _KeepRandomSeed As Boolean? = Nothing 
        Public Property KeepRandomSeed As Boolean
            Get
                If _KeepRandomSeed Is Nothing Then 
                    _KeepRandomSeed = CBool(ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_LEC_KEEP_RAND_SEED_ID, UNDEFINED_USER_ID))
                End If
                Return _KeepRandomSeed.Value
            End Get
            Set(value As Boolean)
                _KeepRandomSeed = value
                With ProjectManager
                    .Attributes.SetAttributeValue(ATTRIBUTE_LEC_KEEP_RAND_SEED_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtBoolean, value, Guid.Empty, Guid.Empty)
                    .Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID, UNDEFINED_USER_ID)
                End With
            End Set
        End Property

        'A1530 ===        
        Public Shared _SIMULATE_SOURCES As Integer = 0
        Public Shared _SIMULATE_EVENTS As Integer = 1

        Public _MAX_NUM_SIMULATIONS As Integer = 1000000
        Private _NumberOfTrials As Integer = -1
        Public Property NumberOfTrials As Integer
            Get
                If _NumberOfTrials < 0 Then
                    Dim retVal As Integer = CInt(ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_LEC_NUM_SIMULATIONS_ID, UNDEFINED_USER_ID))
                    Return If(retVal < _MAX_NUM_SIMULATIONS, retVal, _MAX_NUM_SIMULATIONS)
                End If
                Return _NumberOfTrials
            End Get
            Set(value As Integer)
                value = If(value < _MAX_NUM_SIMULATIONS, value, _MAX_NUM_SIMULATIONS)
                _NumberOfTrials = value
                ProjectManager.Attributes.SetAttributeValue(ATTRIBUTE_LEC_NUM_SIMULATIONS_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtLong, value, Guid.Empty, Guid.Empty)
                With ProjectManager
                    .Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID, UNDEFINED_USER_ID)
                End With
            End Set
        End Property
        'A1530 ==

        Private mSimulatedRisk As Double = 0
        Public ReadOnly Property SimulatedRisk As Double
            Get
                Return mSimulatedRisk
            End Get
        End Property

        Private mComputedRisk As Double = 0
        Public ReadOnly Property ComputedRisk As Double
            Get
                Return mComputedRisk
            End Get
        End Property

        'Private Values As New Dictionary(Of Integer, RiskValuesInfo)

        ' for each covering objective in the hierarchy H (cov. obj.'s ID is a Key here) we get a Dictionary of contributed alternatives (Key = alt.NodeID, Value = alt)
        Public Function GetTerminalNodesDictionary(H As clsHierarchy) As Dictionary(Of Integer, List(Of Integer))
            Dim res As New Dictionary(Of Integer, List(Of Integer))
            Dim tNodes As List(Of clsNode)
            If impactWRTNode Is Nothing Then impactWRTNode = hImpact.Nodes(0)
            If likelihoodWRTNode Is Nothing Then likelihoodWRTNode = hLikelihood.Nodes(0)
            Select Case H.HierarchyID
                Case ECHierarchyID.hidLikelihood
                    tNodes = hLikelihood.GetRespectiveTerminalNodes(likelihoodWRTNode)
                Case ECHierarchyID.hidImpact
                    tNodes = hImpact.GetRespectiveTerminalNodes(impactWRTNode)
                Case Else
                    tNodes = H.TerminalNodes
            End Select

            For Each node As clsNode In tNodes
                node.NumFired = 0
                'node.SimulatedPriority = 0
                node.SimulatedAltLikelihood = 0
                node.SimulatedAltImpact = 0
                node.SimulatedConsequences.Clear()
                node.SimulatedVulnerabilities.Clear()
                Dim list As New List(Of Integer)
                For Each alt As clsNode In node.GetNodesBelow(UNDEFINED_USER_ID)
                    list.Add(alt.NodeID)
                Next
                res.Add(node.NodeID, list)
            Next
            Return res
        End Function

        ''' <summary>
        ''' Calculate coefficients vulnerabilities when we use controls. This is needed when we check if vulnerability is fired.
        ''' </summary>
        Private Sub CalculateVulnerabilitiesReductionsCoefficients()
            Dim tEnabledControls As List(Of clsControl) = ProjectManager.Controls.EnabledControls

            Dim ControlsUsageMode As ControlsUsageMode = ProjectManager.CalculationsManager.ControlsUsageMode

            controlsCoefficientsForVulnerabilities.Clear()

            For Each source As clsNode In terminalLikelihoods
                Dim eventsCoeffs As New Dictionary(Of Integer, Double)
                controlsCoefficientsForVulnerabilities.Add(source.NodeID, eventsCoeffs)
                For Each e As clsNode In source.GetNodesBelow(UNDEFINED_USER_ID)
                    Dim AltWRTReduction As Double = 1
                    For Each control As clsControl In tEnabledControls
                        If (ControlsUsageMode = ControlsUsageMode.UseAll) OrElse (ControlsUsageMode = ControlsUsageMode.UseOnlyActive AndAlso control.Active) Then
                            For Each assignment As clsControlAssignment In control.Assignments
                                If (ControlsUsageMode = ControlsUsageMode.UseAll) OrElse assignment.IsActive Then
                                    If Not assignment.EventID.Equals(Guid.Empty) Then
                                        If assignment.ObjectiveID.Equals(source.NodeGuidID) AndAlso assignment.EventID.Equals(e.NodeGuidID) Then
                                            AltWRTReduction *= 1 + If(ProjectManager.CalculationsManager.IsOpportunity(e), assignment.Value, -assignment.Value)
                                        End If
                                    End If
                                End If
                            Next
                        End If
                    Next
                    eventsCoeffs.Add(e.NodeID, AltWRTReduction)
                Next
            Next
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        Public Sub CreateComputedValues()
            Dim calcTarget As clsCalculationTarget = ProjectManager.CalculationsManager.GetCalculationTargetByUserID(UserID)

            Dim oldHID As Integer = ProjectManager.ActiveHierarchy

            Dim riskValues As New Dictionary(Of Integer, Double)

            ' calculate likelihood values (likelihoods for sources and vulnerabilities)
            ProjectManager.ActiveHierarchy = ECHierarchyID.hidLikelihood
            ProjectManager.CalculationsManager.Calculate(calcTarget, hLikelihood.Nodes(0))

            ' fill likelihood and vulnerability values for sources
            'MiscFuncs.PrintDebugInfo(vbNewLine + "Computed likelihoods: " + ProjectManager.CalculationsManager.ControlsUsageMode.ToString)
            computedLikelihoods.Clear()
            computedVulnerabilities.Clear()
            For Each source As clsNode In terminalLikelihoods
                computedLikelihoods.Add(source.NodeID, If(UseRelativeLikelihoods, source.WRTRelativeBPriority, source.UnnormalizedPriority)) 'A1545
                'MiscFuncs.PrintDebugInfo(source.NodeName + " = " + computedLikelihoods(source.NodeID).ToString)
                computedVulnerabilities.Add(source.NodeID, New Dictionary(Of Integer, Double))
                'MiscFuncs.PrintDebugInfo("Vulnerabilities: ")
                For Each eventID As Integer In contributedEventsForSources(source.NodeID)
                    'A1545 ===
                    Dim value As Double = 0
                    If UseRelativeLikelihoods Then
                        value = source.Judgments.Weights.GetUserWeights(calcTarget.GetUserID, ProjectManager.CalculationsManager.SynthesisMode, ProjectManager.CalculationsManager.IncludeIdealAlternative).GetNormalizedWeightValueByNodeID(eventID)
                    Else
                        value = source.Judgments.Weights.GetUserWeights(calcTarget.GetUserID, ProjectManager.CalculationsManager.SynthesisMode, ProjectManager.CalculationsManager.IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(eventID)
                    End If
                    'A1545 ==
                    'MiscFuncs.PrintDebugInfo("  " + hEvents.GetNodeByID(eventID).NodeName + " = " + value.ToString)
                    computedVulnerabilities(source.NodeID).Add(eventID, value)
                Next
            Next

            If uncontributedEvents.Count > 0 Then
                computedVulnerabilities.Add(-1, New Dictionary(Of Integer, Double))
                For Each e As clsNode In uncontributedEvents
                    computedVulnerabilities(-1).Add(e.NodeID, If(UseRelativeLikelihoods, e.WRTRelativeBPriority, e.UnnormalizedPriority)) 'A1545
                Next
            End If

            'MiscFuncs.PrintDebugInfo("Computed likelihoods: ")
            For Each e As clsNode In hEvents.TerminalNodes
                riskValues.Add(e.NodeID, If(UseRelativeLikelihoods, e.WRTRelativeBPriority, e.UnnormalizedPriority)) 'A1545
                'MiscFuncs.PrintDebugInfo(e.NodeName + " = " + e.UnnormalizedPriority.ToString)
            Next

            ' calculate impact values (priorities of objectives and consequences)
            ProjectManager.ActiveHierarchy = ECHierarchyID.hidImpact
            ProjectManager.CalculationsManager.Calculate(calcTarget, hImpact.Nodes(0))

            Dim tEnabledControls As List(Of clsControl) = ProjectManager.Controls.EnabledControls
            Dim ControlsUsageMode As ControlsUsageMode = ProjectManager.CalculationsManager.ControlsUsageMode
            Dim UseControls As Boolean = ProjectManager.CalculationsManager.ControlsUsageMode <> ControlsUsageMode.DoNotUse

            ' fill priority and consequence values for objectives
            computedPriorities.Clear()
            computedConsequences.Clear()
            For Each objective As clsNode In terminalImpacts
                computedPriorities.Add(objective.NodeID, objective.UnnormalizedPriority)
                computedConsequences.Add(objective.NodeID, New Dictionary(Of Integer, Double))
                For Each eventID As Integer In contributedEventsForObjectives(objective.NodeID)
                    Dim value As Double = objective.Judgments.Weights.GetUserWeights(calcTarget.GetUserID, ProjectManager.CalculationsManager.SynthesisMode, ProjectManager.CalculationsManager.IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(eventID)

                    Dim AltWRTReduction As Double = 1
                    If UseControls Then
                        For Each control As clsControl In tEnabledControls
                            If (ControlsUsageMode = ControlsUsageMode.UseAll) OrElse (ControlsUsageMode = ControlsUsageMode.UseOnlyActive AndAlso control.Active) Then
                                For Each assignment As clsControlAssignment In control.Assignments
                                    If (ControlsUsageMode = ControlsUsageMode.UseAll) OrElse assignment.IsActive Then
                                        If Not assignment.EventID.Equals(Guid.Empty) Then
                                            If assignment.ObjectiveID.Equals(objective.NodeGuidID) AndAlso assignment.EventID.Equals(hEvents.GetNodeByID(eventID).NodeGuidID) Then
                                                AltWRTReduction *= 1 + If(ProjectManager.CalculationsManager.IsOpportunity(hEvents.GetNodeByID(eventID)), assignment.Value, -assignment.Value)
                                            End If
                                        End If
                                    End If
                                Next
                            End If
                        Next
                    End If
                    value *= AltWRTReduction

                    computedConsequences(objective.NodeID).Add(eventID, value)
                Next
            Next

            'MiscFuncs.PrintDebugInfo("Computed impacts: ")
            For Each e As clsNode In hEvents.TerminalNodes
                riskValues(e.NodeID) *= e.UnnormalizedPriority

                'Values(e.NodeID).CalculatedImpact = e.UnnormalizedPriority
                'MiscFuncs.PrintDebugInfo(e.NodeName + " = " + e.UnnormalizedPriority.ToString)
            Next

            mComputedRisk = riskValues.Values.Sum

            ProjectManager.ActiveHierarchy = oldHID
        End Sub

        Public Sub SimulateCommon(tUserID As Integer, tControlsUsageMode As ControlsUsageMode, tWRTNode As clsNode, Optional tUseRelativeLikelihoods As Boolean = False, Optional GenerateChartValues As Boolean = False, Optional StoreStepValues As Boolean = False) 'A1530 + A1545
            Dim tOldControlsUsageMode As ControlsUsageMode = ProjectManager.CalculationsManager.ControlsUsageMode
            Me.StoreStepValues = StoreStepValues
            Me.GenerateChartValues = GenerateChartValues
            UserID = tUserID
            'SimulationMode ?
            'isExclusiveEvents
            WRTNode = tWRTNode
            UseRelativeLikelihoods = tUseRelativeLikelihoods 'A1545
            ProjectManager.CalculationsManager.ControlsUsageMode = tControlsUsageMode
            ProjectManager.RiskSimulations.Simulate()            
            ProjectManager.CalculationsManager.ControlsUsageMode = tOldControlsUsageMode
            WRTNode = Nothing
        End Sub

        ' FOR DEBUG PURPOSES ONLY. USE ONLY AFTER SIMULATIONS.
        Private Sub PrintResults()
            Debug.Print(vbNewLine + "Results of simulation. Number of trials: " + NumberOfTrials.ToString + vbNewLine)
            Debug.Print("Likelihoods for terminal sources: " + vbNewLine)
            For Each source As clsNode In hLikelihood.TerminalNodes
                Debug.Print(source.NodeName + ": computed = " + computedLikelihoods(source.NodeID).ToString + "; simulated = " + source.SimulatedPriority.ToString)
                For Each eventID As Integer In contributedEventsForSources(source.NodeID)
                    Debug.Print("   " + hEvents.GetNodeByID(eventID).NodeName + ": computed = " + computedVulnerabilities(source.NodeID)(eventID).ToString + "; simulated = " + source.SimulatedVulnerabilities(eventID).ToString)
                Next
            Next

            Debug.Print(vbNewLine + "Consequences: ")
            For Each objective As clsNode In hImpact.TerminalNodes
                Debug.Print(objective.NodeName + ":")
                For Each eventID As Integer In contributedEventsForObjectives(objective.NodeID)
                    Debug.Print("   " + hEvents.GetNodeByID(eventID).NodeName + ": computed = " + computedConsequences(objective.NodeID)(eventID).ToString + "; simulated = " + SimulatedConsequences(objective.NodeID)(eventID).ToString)
                Next
            Next

            Debug.Print(vbNewLine + "Simulated event Likelihoods and Impacts: ")
            For Each e As clsNode In hEvents.TerminalNodes
                Debug.Print(e.NodeName + ": likelihood = " + e.SimulatedAltLikelihood.ToString + "; impact = " + e.SimulatedAltImpact.ToString + "; risk = " + (e.SimulatedAltLikelihood * e.SimulatedAltImpact).ToString)
            Next

            Debug.Print(vbNewLine + "Total simulated risk = " + SimulatedRisk.ToString)

            If GenerateChartValues Then
                Debug.Print("Frequency Values count = " + FrequencyValues.Count.ToString + vbNewLine)
                Debug.Print("Green line intersection = " + GreenLineIntersection.ToString + vbNewLine)
                Debug.Print("Red line intersection = " + RedLineIntersection.ToString + vbNewLine)
                Debug.Print("Loss Exceedance Values: ")
                For Each kvp As KeyValuePair(Of Double, Double) In LossExceedanceValues
                    Debug.Print(kvp.Key.ToString + " | " + kvp.Value.ToString)
                Next
            End If
        End Sub

        Public Sub New(ProjectManager As clsProjectManager)
            Me.ProjectManager = ProjectManager
        End Sub

        Private Function Shuffle(ByRef list As List(Of Integer), Rand As Random) As List(Of Integer)
            If Not UseShuffling Then Return list

            Dim j As Integer
            Dim tmp As Integer
            For i As Integer = 0 To list.Count - 1
                ' j - random, 0 <= j < i
                j = Rand.Next(i)
                tmp = list(i)
                list(i) = list(j)
                list(j) = tmp
            Next
            Return list
        End Function

        Public Delegate Function SimulationCancelFunc() As Boolean
        Private SimuationCancelled As SimulationCancelFunc = Nothing

        Public Sub Simulate(Optional cancelFunc As SimulationCancelFunc = Nothing)
            SimuationCancelled = cancelFunc
            Dim b As Boolean = True
            If b Then
                SimulateOld()
            Else
                SimulateNew()
            End If
        End Sub
        Public Sub SimulateOld()
            'Values.Clear()

            MiscFuncs.PrintDebugInfo("-- start old simulations -- ")

            Dim UseControls As Boolean = ProjectManager.CalculationsManager.ControlsUsageMode <> ControlsUsageMode.DoNotUse
            Dim Rand As Random
            If Not KeepRandomSeed AndAlso Not UseControls Then
                RandomSeed = DateTime.Now.Millisecond
                Rand = New Random(RandomSeed)
            Else
                Rand = New Random(RandomSeed)
            End If

            Dim oldCacheValue As Boolean = ProjectManager.CalculationsManager.PrioritiesCacheManager.Enabled
            If Not oldCacheValue Then
                ProjectManager.CalculationsManager.PrioritiesCacheManager.ClearCache()
            End If
            ProjectManager.CalculationsManager.PrioritiesCacheManager.Enabled = True

            Dim _ConsequenceSimulationsMode As ConsequencesSimulationsMode = ProjectManager.CalculationsManager.ConsequenceSimulationsMode

            likelihoodWRTNode = hLikelihood.Nodes(0)
            impactWRTNode = hImpact.Nodes(0)

            If hLikelihood.Nodes.Contains(WRTNode) Then likelihoodWRTNode = WRTNode
            If hImpact.Nodes.Contains(WRTNode) Then impactWRTNode = WRTNode

            contributedEventsForSources = GetTerminalNodesDictionary(hLikelihood)
            contributedEventsForObjectives = GetTerminalNodesDictionary(hImpact)

            uncontributedEvents = hLikelihood.GetUncontributedAlternatives().Where(Function(alt) alt.Enabled).ToList
            Dim uncontributedEventsIDs As List(Of Integer) = uncontributedEvents.Select(Function(x) x.NodeID).ToList
            Dim uncontributedEventsCount As Integer = uncontributedEventsIDs.Count

            terminalLikelihoods = hLikelihood.GetRespectiveTerminalNodes(likelihoodWRTNode)
            terminalImpacts = hImpact.GetRespectiveTerminalNodes(impactWRTNode)
            terminalEvents = hEvents.TerminalNodes

            Dim terminalSourcesIDs As List(Of Integer) = terminalLikelihoods.Select(Function(x) x.NodeID).ToList
            Dim terminalSourcesCount As Integer = terminalSourcesIDs.Count
            Dim terminalEventsIDs As List(Of Integer) = terminalEvents.Select(Function(x) x.NodeID).ToList
            Dim terminalEventsCount As Integer = terminalEventsIDs.Count

            Dim sourcesCount As Integer = terminalLikelihoods.Count
            Dim impactsCount As Integer = terminalImpacts.Count
            Dim eventsCount As Integer = terminalEvents.Count

            Dim contributedEventsForObjectivesHashSet As New Dictionary(Of Integer, HashSet(Of Integer))(impactsCount)
            For Each objective As clsNode In terminalImpacts
                Dim hs As New HashSet(Of Integer)
                For Each e As clsNode In objective.GetNodesBelow(UNDEFINED_USER_ID)
                    hs.Add(e.NodeID)
                Next
                contributedEventsForObjectivesHashSet.Add(objective.NodeID, hs)
            Next

            Dim contributedSourcesForEvents As New Dictionary(Of Integer, List(Of Integer))
            For Each alt As clsNode In hEvents.TerminalNodes
                Dim L As New List(Of Integer)
                For Each node As clsNode In ProjectManager.GetContributedCoveringObjectives(alt, hLikelihood)
                    L.Add(node.NodeID)
                Next
                contributedSourcesForEvents.Add(alt.NodeID, L)
            Next

            Dim isMixedModel As Boolean = ProjectManager.PipeParameters.ProjectType = Canvas.CanvasTypes.ProjectType.ptMixed
            Dim isMyRiskRewardModel As Boolean = ProjectManager.PipeParameters.ProjectType = Canvas.CanvasTypes.ProjectType.ptMyRiskReward  ' D6798
            Dim isOpportunityEvent As New Dictionary(Of Integer, Boolean)
            For Each e As clsNode In terminalEvents
                isOpportunityEvent.Add(e.NodeID, If(isMixedModel OrElse isMyRiskRewardModel, If(ProjectManager.CalculationsManager.IsOpportunity(e), True, False), True))   ' D6798
            Next

            CreateComputedValues()

            'UseControls = False
            If UseControls Then
                CalculateVulnerabilitiesReductionsCoefficients()
            End If

            FrequencyValues.Clear()
            LossExceedanceValues.Clear()

            RedLineIntersection = UNDEFINED_INTEGER_VALUE
            GreenLineIntersection = UNDEFINED_INTEGER_VALUE
            AverageLossValue = UNDEFINED_INTEGER_VALUE

            Dim eFrequency As New Dictionary(Of Integer, Dictionary(Of Double, Integer))

            Dim maxImpact As Double = 0
            Dim minImpact As Double = 0

            If Not isMixedModel AndAlso Not isMyRiskRewardModel Then   ' D6798
                For Each objectiveID As Integer In contributedEventsForObjectives.Keys
                    Dim sum As Double = computedConsequences(objectiveID).Values.Sum
                    Dim k As Double = If(sum > 1, sum, 1)
                    For Each eventID As Integer In contributedEventsForObjectives(objectiveID)
                        maxImpact += computedConsequences(objectiveID)(eventID) / k * computedPriorities(objectiveID)
                    Next
                Next
                Dim stepValue As Double = maxImpact / DataPointsCount

                For i As Integer = 0 To DataPointsCount - 1
                    If Not LossExceedanceValues.ContainsKey(stepValue * i) Then LossExceedanceValues.Add(stepValue * i, 0) 'A1550
                Next
            Else
                For Each objectiveID As Integer In contributedEventsForObjectives.Keys
                    Dim sumOpportunities As Double = 0
                    Dim sumRisks As Double = 0
                    For Each eventID As Integer In contributedEventsForObjectives(objectiveID)
                        If isOpportunityEvent(eventID) Then
                            sumOpportunities += computedConsequences(objectiveID)(eventID)
                        Else
                            sumRisks += computedConsequences(objectiveID)(eventID)
                        End If
                    Next

                    Dim kOpportunities As Double = If(sumOpportunities > 1, sumOpportunities, 1)
                    Dim kRisks As Double = If(sumRisks > 1, sumRisks, 1)
                    For Each eventID As Integer In contributedEventsForObjectives(objectiveID)
                        If isOpportunityEvent(eventID) Then
                            maxImpact += computedConsequences(objectiveID)(eventID) / kOpportunities * computedPriorities(objectiveID)
                        Else
                            minImpact -= computedConsequences(objectiveID)(eventID) / kRisks * computedPriorities(objectiveID)
                        End If
                    Next
                Next
                Dim stepValue As Double = (maxImpact - minImpact) / DataPointsCount

                For i As Integer = 0 To DataPointsCount - 1
                    If Not LossExceedanceValues.ContainsKey(minImpact + stepValue * i) Then LossExceedanceValues.Add(minImpact + stepValue * i, 0) 'A1550
                Next
            End If

            ' store how many times each source / event has fired
            ' key = source ID, value = num fired
            Dim sourcesFiredCount As New Dictionary(Of Integer, Integer)(sourcesCount)
            ' add source to the dictionary to track how many times it was fired during all trials
            For Each source As clsNode In terminalLikelihoods
                sourcesFiredCount.Add(source.NodeID, 0)
            Next

            Dim eventsFiredCount As New Dictionary(Of Integer, Integer)(eventsCount)
            For Each e As clsNode In terminalEvents
                eventsFiredCount.Add(e.NodeID, 0)
            Next

            ' dictionary of vulnerabilities for simulation. Value here is the number vulnerability fired
            Dim vulnerabilitiesFiredCount As New Dictionary(Of Integer, Dictionary(Of Integer, Integer))(sourcesCount)
            For Each source As clsNode In terminalLikelihoods
                vulnerabilitiesFiredCount.Add(source.NodeID, New Dictionary(Of Integer, Integer)(contributedEventsForSources(source.NodeID).Count))
                For Each eventID As Integer In contributedEventsForSources(source.NodeID)
                    vulnerabilitiesFiredCount(source.NodeID).Add(eventID, 0)
                Next
            Next

            mSimulatedConsequences = New Dictionary(Of Integer, Dictionary(Of Integer, Double))(impactsCount)
            Dim mStepSimulatedConsequences As New Dictionary(Of Integer, Dictionary(Of Integer, Double))(impactsCount)
            ' initialize simulated consequences
            For Each objective As clsNode In terminalImpacts
                mSimulatedConsequences.Add(objective.NodeID, New Dictionary(Of Integer, Double)(contributedEventsForObjectives(objective.NodeID).Count))
                If GenerateChartValues Then
                    mStepSimulatedConsequences.Add(objective.NodeID, New Dictionary(Of Integer, Double)(contributedEventsForObjectives(objective.NodeID).Count))
                End If
            Next

            If StoreStepValues Then
                StepsInfo = New Dictionary(Of Integer, SimulationStepInfo)(NumberOfStepsToStore)
            End If
            Dim info As New SimulationStepInfo

            AverageLossValue = 0

            Dim sourceGroupsLookup As New Dictionary(Of Integer, EventsGroup)
            Dim sourcePrecedencesLookup As New Dictionary(Of Integer, Integer)
            For Each group As EventsGroup In ProjectManager.SourceGroups.Groups
                For Each eData As EventGroupData In group.Events
                    sourceGroupsLookup.Add(eData.EventIntID, group)
                    sourcePrecedencesLookup.Add(eData.EventIntID, eData.Precedence)
                Next
            Next

            Dim eventsGroupsLookup As New Dictionary(Of Integer, List(Of EventsGroup))
            Dim eventsPrecedencesLookup As New Dictionary(Of Integer, Dictionary(Of Guid, Integer)) ' EventID, GroupID, Precedence
            For Each group As EventsGroup In ProjectManager.EventsGroups.Groups
                For Each eData As EventGroupData In group.Events
                    If eventsGroupsLookup.ContainsKey(eData.EventIntID) Then
                        eventsGroupsLookup(eData.EventIntID).Add(group)
                    Else
                        eventsGroupsLookup.Add(eData.EventIntID, New List(Of EventsGroup) From {group})
                    End If
                    'eventsGroupsLookup.Add(eData.EventIntID, group)

                    If Not eventsPrecedencesLookup.ContainsKey(eData.EventIntID) Then
                        eventsPrecedencesLookup.Add(eData.EventIntID, New Dictionary(Of Guid, Integer))
                    End If
                    eventsPrecedencesLookup(eData.EventIntID).Add(group.ID, eData.Precedence)

                    'eventsPrecedencesLookup.Add(eData.EventIntID, eData.Precedence)
                Next
            Next

            MiscFuncs.PrintDebugInfo("-- start old trials cycle -- ")
            ' starting simulation

            Dim sID As Integer ' will be used in the loop for sources
            Dim eID As Integer ' will be used in the loop for events with no source

            Dim mUseSourceGroups As Boolean = UseSourceGroups
            Dim mUseEventsGroups As Boolean = UseEventsGroups

            For i As Integer = 1 To NumberOfTrials

                If SimuationCancelled IsNot Nothing Then 
                    If i Mod 100 = 0 AndAlso SimuationCancelled() Then 
                        Exit For
                    End If
                End If

                If mUseSourceGroups Then
                    For Each group As EventsGroup In ProjectManager.SourceGroups.Groups
                        group.TempFiredID = -1
                        group.TempFiredPrecedence = Integer.MaxValue
                        group.TempDidNotFiredCount = 0
                    Next
                End If

                If mUseEventsGroups Then
                    For Each group As EventsGroup In ProjectManager.EventsGroups.Groups
                        group.TempFiredID = -1
                        group.TempFiredPrecedence = Integer.MaxValue
                    Next
                End If

                If StoreStepValues AndAlso i <= NumberOfStepsToStore Then
                    info = New SimulationStepInfo
                    info.SourcesInfo = New Dictionary(Of Integer, TupleBDD)(terminalSourcesCount)
                    StepsInfo.Add(i, info)
                End If

                ' dictionary of sources that have fired on this current trial
                Dim firedSources As New HashSet(Of Integer)

                For j As Integer = 0 To terminalSourcesCount - 1
                    sID = terminalSourcesIDs(j)
                    Dim randForSource As Double = Rand.NextDouble
                    'lastRandom = randForSource
                    Dim fired As Boolean = randForSource <= computedLikelihoods(sID)

                    If mUseSourceGroups Then
                        Dim group As EventsGroup = If(sourceGroupsLookup.ContainsKey(sID), sourceGroupsLookup(sID), Nothing)
                        If group IsNot Nothing Then
                            Select Case group.Behaviour
                                Case GroupBehaviour.MutuallyExclusiveExhaustive
                                    If fired Then
                                        If group.TempFiredID = -1 Then
                                            group.TempFiredID = sID
                                            group.TempFiredSourceID = sID
                                        Else
                                            fired = False
                                        End If
                                    Else
                                        If group.TempDidNotFiredCount = group.Events.Count - 1 Then
                                            fired = True
                                        Else
                                            group.TempDidNotFiredCount += 1
                                        End If
                                    End If
                                Case GroupBehaviour.MutuallyExclusive
                                    If fired Then
                                        If group.TempFiredID = -1 Then
                                            group.TempFiredID = sID
                                            group.TempFiredSourceID = sID
                                        Else
                                            fired = False
                                        End If
                                    End If
                                Case GroupBehaviour.Independent
                            End Select

                            'If group.TempFiredPrecedence < sourcePrecedencesLookup(sID) Then
                            '    fired = False
                            'Else
                            '    If group.TempFiredID <> -1 AndAlso firedSources.Contains(group.TempFiredID) Then
                            '        firedSources.Remove(group.TempFiredID)
                            '        sourcesFiredCount(group.TempFiredID) -= 1
                            '    End If

                            '    group.TempFiredID = sID
                            '    group.TempFiredSourceID = sID
                            '    group.TempFiredPrecedence = sourcePrecedencesLookup(sID)
                            'End If
                        End If
                    End If

                    If fired Then
                        ' add source to fired source list for this current trial
                        firedSources.Add(sID)
                        ' increase num fired in the dictionary
                        sourcesFiredCount(sID) += 1
                    End If
                    If StoreStepValues AndAlso i <= NumberOfStepsToStore Then
                        info.SourcesInfo.Add(sID, New TupleBDD With {.Item1 = fired, .Item2 = computedLikelihoods(sID), .Item3 = randForSource})
                    End If
                Next

                ' dictionary of events that have fired on this current trial
                Dim firedEvents As New List(Of Integer)(eventsCount)

                ' check if events with no source fired
                For j As Integer = 0 To uncontributedEventsCount - 1
                    eID = uncontributedEventsIDs(j)
                    Dim randForEvent As Double = Rand.NextDouble
                    'lastRandom = randForEvent
                    Dim fired As Boolean = randForEvent <= computedVulnerabilities(-1)(eID)
                    If fired Then
                        If mUseEventsGroups Then
                            Dim groups As List(Of EventsGroup) = If(eventsGroupsLookup.ContainsKey(eID), eventsGroupsLookup(eID), Nothing)

                            If groups IsNot Nothing Then
                                For Each group As EventsGroup In groups
                                    If group IsNot Nothing Then
                                        If group.TempFiredPrecedence < eventsPrecedencesLookup(eID)(group.ID) Then
                                            fired = False
                                        Else
                                            If group.TempFiredID <> -1 AndAlso firedEvents.Contains(group.TempFiredID) Then
                                                firedEvents.Remove(group.TempFiredID)
                                                eventsFiredCount(group.TempFiredID) -= 1
                                                'vulnerabilitiesFiredCount(group.TempFiredSourceID)(group.TempFiredID) -= 1
                                                If StoreStepValues AndAlso i <= NumberOfStepsToStore Then
                                                    info.FiredEventsInfo.Remove(group.TempFiredID)
                                                End If
                                            End If

                                            group.TempFiredID = eID
                                            group.TempFiredSourceID = eID
                                            group.TempFiredPrecedence = eventsPrecedencesLookup(eID)(group.ID)
                                        End If
                                    End If
                                Next
                            End If
                        End If
                        If fired Then
                            If Not firedEvents.Contains(eID) Then
                                ' we count event as fired only once
                                firedEvents.Add(eID)
                                eventsFiredCount(eID) += 1
                                'vulnerabilitiesFiredCount(sourceID)(eID) += 1

                                If StoreStepValues AndAlso i <= NumberOfStepsToStore Then
                                    info.FiredEventsInfo.Add(eID, New TupleIDD With {.Item1 = -1, .Item2 = computedVulnerabilities(-1)(eID), .Item3 = randForEvent, .Item7 = fired})
                                End If
                            End If
                        End If
                    End If
                    'firedEvents.Add(eID)
                    'eventsFiredCount(eID) += 1

                    'If StoreStepValues AndAlso i <= NumberOfStepsToStore Then
                    '    info.FiredEventsInfo.Add(eID, New TupleIDD With {.Item1 = -1, .Item2 = computedVulnerabilities(-1)(eID), .Item3 = randForEvent, .Item7 = fired})
                    'End If
                Next

                '' NEW WAY - going from events with shuffling
                'For Each eventID As Integer In terminalEventsIDs
                '    Dim contributedSources As List(Of Integer) = Shuffle(contributedSourcesForEvents(eventID))

                '    For Each sourceID As Integer In contributedSources
                '        If firedSources.Contains(sourceID) Then
                '            Dim v As Double = 0
                '            If computedVulnerabilities.ContainsKey(sourceID) AndAlso computedVulnerabilities(sourceID).ContainsKey(eventID) Then
                '                v = computedVulnerabilities(sourceID)(eventID)
                '                If UseControls Then
                '                    v *= controlsCoefficientsForVulnerabilities(sourceID)(eventID)
                '                End If
                '            End If
                '            Dim randForEventWRTSource As Double = Rand.NextDouble
                '            'lastRandom = randForEventWRTSource
                '            Dim fired As Boolean = randForEventWRTSource <= v
                '            If StoreStepValues AndAlso i <= NumberOfStepsToStore AndAlso Not info.FiredEventsInfo.ContainsKey(eventID) Then
                '                info.FiredEventsInfo.Add(eventID, New TupleIDD With {.Item1 = sourceID, .Item2 = v, .Item3 = randForEventWRTSource, .Item7 = fired})
                '            End If
                '            If fired Then
                '                If UseEventsGroups Then
                '                    Dim group As EventsGroup = If(eventsGroupsLookup.ContainsKey(eventID), eventsGroupsLookup(eventID), Nothing)
                '                    If group IsNot Nothing Then
                '                        If group.TempFiredPrecedence < eventsPrecedencesLookup(eventID) Then
                '                            fired = False
                '                        Else
                '                            If group.TempFiredID <> -1 AndAlso firedEvents.Contains(group.TempFiredID) Then
                '                                firedEvents.Remove(group.TempFiredID)
                '                                eventsFiredCount(group.TempFiredID) -= 1
                '                                vulnerabilitiesFiredCount(group.TempFiredSourceID)(group.TempFiredID) -= 1
                '                                If StoreStepValues AndAlso i <= NumberOfStepsToStore Then
                '                                    info.FiredEventsInfo.Remove(group.TempFiredID)
                '                                End If
                '                            End If

                '                            group.TempFiredID = eventID
                '                            group.TempFiredSourceID = sourceID
                '                            group.TempFiredPrecedence = eventsPrecedencesLookup(eventID)
                '                        End If
                '                    End If
                '                End If
                '                If fired Then
                '                    If Not firedEvents.Contains(eventID) Then
                '                        ' we count event as fired only once
                '                        firedEvents.Add(eventID)
                '                        eventsFiredCount(eventID) += 1
                '                        vulnerabilitiesFiredCount(sourceID)(eventID) += 1
                '                    End If
                '                End If
                '            End If
                '        Else
                '            ' dummy values for random seed preservation
                '        End If
                '    Next
                'Next

                ' for each fired source we check events that are contributed to it
                For Each sourceID As Integer In Shuffle(terminalSourcesIDs, Rand)
                    Dim eventsBelow As List(Of Integer) = Shuffle(contributedEventsForSources(sourceID), Rand)

                    'Dim str As String = ""
                    'For Each x As Integer In eventsBelow
                    '    str += x.ToString
                    'Next
                    'Debug.Print(str)

                    If Not firedSources.Contains(sourceID) Then
                        Dim dummyRandForEventWRTSource As Double
                        For t As Integer = 1 To eventsBelow.Count
                            dummyRandForEventWRTSource = Rand.NextDouble
                        Next
                        'lastRandom = dummyRandForEventWRTSource
                    Else
                        For Each eventID As Integer In eventsBelow
                            ' v = vunlerability of event "e" wrt covering source "source"
                            Dim v As Double = 0
                            If computedVulnerabilities.ContainsKey(sourceID) AndAlso computedVulnerabilities(sourceID).ContainsKey(eventID) Then
                                v = computedVulnerabilities(sourceID)(eventID)
                                If UseControls Then
                                    v *= controlsCoefficientsForVulnerabilities(sourceID)(eventID)
                                End If
                            End If
                            Dim randForEventWRTSource As Double = Rand.NextDouble
                            'lastRandom = randForEventWRTSource
                            Dim fired As Boolean = randForEventWRTSource <= v
                            If StoreStepValues AndAlso i <= NumberOfStepsToStore AndAlso Not info.FiredEventsInfo.ContainsKey(eventID) Then
                                info.FiredEventsInfo.Add(eventID, New TupleIDD With {.Item1 = sourceID, .Item2 = v, .Item3 = randForEventWRTSource, .Item7 = fired})
                            End If
                            If fired Then
                                If mUseEventsGroups Then
                                    Dim groups As List(Of EventsGroup) = If(eventsGroupsLookup.ContainsKey(eventID), eventsGroupsLookup(eventID), Nothing)
                                    If groups IsNot Nothing Then
                                        For Each group As EventsGroup In groups
                                            If group IsNot Nothing Then
                                                If group.TempFiredPrecedence < eventsPrecedencesLookup(eventID)(group.ID) Then
                                                    fired = False
                                                Else
                                                    group.TempFiredID = eventID
                                                    group.TempFiredSourceID = sourceID
                                                    group.TempFiredPrecedence = eventsPrecedencesLookup(eventID)(group.ID)

                                                    If group.TempFiredID <> -1 AndAlso firedEvents.Contains(group.TempFiredID) Then
                                                        firedEvents.Remove(group.TempFiredID)
                                                        eventsFiredCount(group.TempFiredID) -= 1
                                                        vulnerabilitiesFiredCount(group.TempFiredSourceID)(group.TempFiredID) -= 1
                                                        If StoreStepValues AndAlso i <= NumberOfStepsToStore Then
                                                            info.FiredEventsInfo.Remove(group.TempFiredID)
                                                        End If
                                                    End If

                                                    'group.TempFiredID = eventID
                                                    'group.TempFiredSourceID = sourceID
                                                    'group.TempFiredPrecedence = eventsPrecedencesLookup(eventID)
                                                End If
                                            End If
                                        Next
                                    End If
                                End If
                                If fired Then
                                    If Not firedEvents.Contains(eventID) Then
                                        ' we count event as fired only once
                                        firedEvents.Add(eventID)
                                        eventsFiredCount(eventID) += 1
                                        vulnerabilitiesFiredCount(sourceID)(eventID) += 1
                                    End If
                                End If
                            End If
                        Next
                    End If
                Next

                For Each e As clsNode In terminalEvents
                    If firedEvents.Contains(e.NodeID) Then
                        If ProjectManager.Edges.Edges.ContainsKey(e.NodeGuidID) Then
                            For Each edge As Edge In ProjectManager.Edges.Edges(e.NodeGuidID)
                                If Not firedEvents.Contains(edge.ToNode.NodeID) Then
                                    Dim nonpwData As clsNonPairwiseMeasureData = edge.FromNode.EventsJudgments.GetJudgement(edge.ToNode.NodeID, edge.FromNode.NodeID, COMBINED_USER_ID)
                                    If nonpwData IsNot Nothing Then
                                        Dim v As Double = nonpwData.SingleValue
                                        Dim randForEventWRTEvent As Double = Rand.NextDouble
                                        Dim fired As Boolean = randForEventWRTEvent <= v
                                        If fired Then
                                            ' we count event as fired only once
                                            firedEvents.Add(edge.ToNode.NodeID)
                                            eventsFiredCount(edge.ToNode.NodeID) += 1
                                            'vulnerabilitiesFiredCount(sourceID)(eventID) += 1
                                        End If
                                    End If
                                End If
                            Next
                        End If
                    End If
                Next

                ' for each covering objective in Impact hiearchy
                For Each objectiveID As Integer In contributedEventsForObjectives.Keys
                    ' for each contributed event for this objective
                    ' first, check if the sum of consequences from fired events to this objective is more than 1
                    ' if it is, then make fired consequences proportional to it's value

                    ' For instance, objective 1 has 3 events with some consequences (e1 = 0.6, e2 = 0.2, e3 = 0.7) and all of them are fired
                    ' In this case, total consequence to objective 1 = 0.6 + 0.2 + 0.7 = 1.5, but impact to objective cannot be more than 100%
                    ' So, we have to distribute full impact (100%) between e1, e2 and e3 proportionally to its value.
                    ' To do that, we divide 0.6, 0.2 and 0.7 by the sum (1.5)

                    ' get the list of fired contributed events
                    'Dim firedEventsForObjective As List(Of Integer) = firedEvents.Where(Function(x) contributedEventsForObjectivesHashSet(objectiveID).Contains(x)).ToList

                    ' using for loop for optimization since it is working faster than linq query
                    ' also, calculate sumOfFiredConsequenses as well in this cycle to speed things up
                    Dim multOfFiredConsequenses As Double = 1
                    Dim sumOfFiredConsequenses As Double = 0
                    Dim multOfFiredConsequensesForOpportunities As Double = 1
                    Dim sumOfFiredConsequensesForOpportunities As Double = 0

                    Dim eCount As Integer = firedEvents.Count
                    Dim firedEventsForObjective As New List(Of Integer)
                    Dim currentFiredEventID As Integer
                    For j As Integer = 0 To eCount - 1
                        currentFiredEventID = firedEvents(j)
                        If contributedEventsForObjectivesHashSet(objectiveID).Contains(currentFiredEventID) Then
                            firedEventsForObjective.Add(currentFiredEventID)
                            If (isMixedModel OrElse isMyRiskRewardModel) AndAlso isOpportunityEvent(currentFiredEventID) Then   ' D6798
                                sumOfFiredConsequensesForOpportunities += computedConsequences(objectiveID)(currentFiredEventID)
                                multOfFiredConsequensesForOpportunities *= 1 - computedConsequences(objectiveID)(currentFiredEventID)
                            Else
                                sumOfFiredConsequenses += computedConsequences(objectiveID)(currentFiredEventID)
                                multOfFiredConsequenses *= 1 - computedConsequences(objectiveID)(currentFiredEventID)
                            End If
                        End If
                    Next
                    multOfFiredConsequenses = 1 - multOfFiredConsequenses
                    multOfFiredConsequensesForOpportunities = 1 - multOfFiredConsequensesForOpportunities

                    ' if the sum of fired consequences is > 1, we normalize
                    Dim normalizationFactor As Double = If(sumOfFiredConsequenses = 0, 1, sumOfFiredConsequenses)
                    Dim normalizationFactorForOpportunities As Double = If(sumOfFiredConsequensesForOpportunities = 0, 1, sumOfFiredConsequensesForOpportunities)

                    If mStepSimulatedConsequences.ContainsKey(objectiveID) Then mStepSimulatedConsequences(objectiveID).Clear() Else mStepSimulatedConsequences.Add(objectiveID, New Dictionary(Of Integer, Double)) 'A1533

                    For Each eventID As Integer In firedEventsForObjective
                        If Not mSimulatedConsequences(objectiveID).ContainsKey(eventID) Then mSimulatedConsequences(objectiveID).Add(eventID, 0)
                        Dim M As Double
                        If (isMixedModel OrElse isMyRiskRewardModel) AndAlso isOpportunityEvent(eventID) Then   ' D6798
                            M = multOfFiredConsequensesForOpportunities / normalizationFactorForOpportunities
                        Else
                            M = multOfFiredConsequenses / normalizationFactor
                        End If

                        Dim d As Double = If(_ConsequenceSimulationsMode = ConsequencesSimulationsMode.Diluted, computedConsequences(objectiveID)(eventID) * M, computedConsequences(objectiveID)(eventID))

                        If Not eFrequency.ContainsKey(eventID) Then eFrequency.Add(eventID, New Dictionary(Of Double, Integer))
                        If Not eFrequency(eventID).ContainsKey(d) Then eFrequency(eventID).Add(d, 0)
                        eFrequency(eventID)(d) += 1

                        mSimulatedConsequences(objectiveID)(eventID) += d
                        mStepSimulatedConsequences(objectiveID).Add(eventID, d)
                    Next
                Next

                Dim stepImpact As Double = 0
                For Each e As clsNode In terminalEvents
                    Dim eventImpact As Double = 0
                    For Each objective As clsNode In terminalImpacts
                        If mStepSimulatedConsequences(objective.NodeID).ContainsKey(e.NodeID) Then
                            'stepImpact += mStepSimulatedConsequences(objective.NodeID)(e.NodeID) * computedPriorities(objective.NodeID)
                            stepImpact += If(ProjectManager.PipeParameters.ProjectType = Canvas.CanvasTypes.ProjectType.ptMixed OrElse ProjectManager.PipeParameters.ProjectType = Canvas.CanvasTypes.ProjectType.ptMyRiskReward, If(ProjectManager.CalculationsManager.IsOpportunity(e), 1, -1), 1) * mStepSimulatedConsequences(objective.NodeID)(e.NodeID) * computedPriorities(objective.NodeID)    ' D6798

                            If (StoreStepValues AndAlso i <= NumberOfStepsToStore) Then
                                eventImpact += mStepSimulatedConsequences(objective.NodeID)(e.NodeID) * computedPriorities(objective.NodeID)
                            End If
                        End If
                    Next
                    If (StoreStepValues AndAlso i <= NumberOfStepsToStore AndAlso info.FiredEventsInfo.ContainsKey(e.NodeID)) Then
                        info.FiredEventsInfo(e.NodeID).Item4 = eventImpact
                        info.FiredEventsInfo(e.NodeID).Item5 = info.FiredEventsInfo(e.NodeID).Item2 * eventImpact
                        info.FiredEventsInfo(e.NodeID).Item6 = info.FiredEventsInfo(e.NodeID).Item5 * If(info.FiredEventsInfo(e.NodeID).Item1 >= 0, computedLikelihoods(info.FiredEventsInfo(e.NodeID).Item1), 1) 'A1600
                    End If
                Next

                If (StoreStepValues AndAlso i <= NumberOfStepsToStore) Then
                    info.Impact = stepImpact
                End If
                AverageLossValue += stepImpact

                If GenerateChartValues Then
                    If Not FrequencyValues.ContainsKey(stepImpact) Then FrequencyValues.Add(stepImpact, 0)
                    FrequencyValues(stepImpact) += 1
                    For k As Integer = 0 To LossExceedanceValues.Keys.Count - 1
                        If LossExceedanceValues.Keys(k) < stepImpact Then
                            LossExceedanceValues(LossExceedanceValues.Keys(k)) += 1
                        End If
                    Next
                End If
            Next
            MiscFuncs.PrintDebugInfo("-- end trials cycle -- ")

            ' calculate simulated likelihoods for sources:
            ' take the number a source fired and divide by the number of trials
            ' inside the loop we're also calculating simulated vulnerabilities for each contributed event for this source (num fired / num trials)
            For Each source As clsNode In terminalLikelihoods
                source.SimulatedPriority = sourcesFiredCount(source.NodeID) / NumberOfTrials

                ' calculate simulated vulnerabilities
                source.SimulatedVulnerabilities.Clear()
                mSimulatedVulnerabilities.Clear()
                Dim eventsBelow As List(Of Integer) = contributedEventsForSources(source.NodeID)
                For Each eventID As Integer In eventsBelow
                    source.SimulatedVulnerabilities.Add(eventID, vulnerabilitiesFiredCount(source.NodeID)(eventID) / NumberOfTrials)
                    mSimulatedVulnerabilities.Add(eventID, vulnerabilitiesFiredCount(source.NodeID)(eventID) / NumberOfTrials)
                Next
            Next

            ' calculate simulated impacts for objectives
            For Each objective As clsNode In terminalImpacts
                objective.SimulatedPriority = mSimulatedConsequences(objective.NodeID).Sum(Function(obj) obj.Value) / NumberOfTrials
            Next

            ' calculate simulated likelihoods for events with no source:
            ' take the number an event with no source fired and divide by the number of trials
            'MiscFuncs.PrintDebugInfo(vbNewLine + "Events fired count (" + UseControls.ToString + ") :")
            For Each e As clsNode In terminalEvents
                'MiscFuncs.PrintDebugInfo(e.NodeName + " - " + eventsFiredCount(e.NodeID).ToString)
                e.SimulatedAltLikelihood = eventsFiredCount(e.NodeID) / NumberOfTrials
            Next

            ' calculate simulated consequences:
            ' for each consequence of e to objective O we take aggregated simulated consequence value and divide it by the number e has fired
            ' inside the loop we're also calculating simulated vulnerabilities for each contributed event for this source (num fired / num trials)


            For Each objectiveID As Integer In contributedEventsForObjectives.Keys
                For Each eventID As Integer In contributedEventsForObjectives(objectiveID)
                    Dim numFired As Integer = eventsFiredCount(eventID)
                    If mSimulatedConsequences(objectiveID).ContainsKey(eventID) Then mSimulatedConsequences(objectiveID)(eventID) /= If(numFired = 0, 1, numFired) 'A1545
                Next
            Next

            ' calculate simulated impact values for each event e:
            ' event impact = sum of ((consequence of e to objective j) * priority of objective j), for each objective that this event contributes to
            For Each e As clsNode In terminalEvents
                e.SimulatedAltImpact = 0
                For Each objective As clsNode In terminalImpacts
                    If mSimulatedConsequences(objective.NodeID).ContainsKey(e.NodeID) Then
                        objective.SimulatedConsequences.Add(e.NodeID, mSimulatedConsequences(objective.NodeID)(e.NodeID)) 'A1622
                        e.SimulatedAltImpact += mSimulatedConsequences(objective.NodeID)(e.NodeID) * computedPriorities(objective.NodeID)
                    End If
                Next
                e.SimulatedAltImpact *= If(ProjectManager.PipeParameters.ProjectType = Canvas.CanvasTypes.ProjectType.ptMixed OrElse ProjectManager.PipeParameters.ProjectType = Canvas.CanvasTypes.ProjectType.ptMyRiskReward, If(ProjectManager.CalculationsManager.IsOpportunity(e), 1, -1), 1)  ' D6798
            Next

            ' calculate simulated risk value: sum of (SimulatedLikelihood * SimulatedImpact) for each event
            mSimulatedRisk = 0
            For Each e As clsNode In terminalEvents
                mSimulatedRisk += e.SimulatedAltLikelihood * e.SimulatedAltImpact
            Next

            AverageLossValue /= NumberOfTrials

            If GenerateChartValues Then
                For k As Integer = 0 To LossExceedanceValues.Keys.Count - 1
                    LossExceedanceValues(LossExceedanceValues.Keys(k)) /= NumberOfTrials
                Next

                ' calculate red line value
                For k As Integer = 0 To LossExceedanceValues.Count - 2
                    If LossExceedanceValues.Values(k + 1) <= RedLineValue AndAlso RedLineValue < LossExceedanceValues.Values(k) Then
                        Dim coeff1 As Double = (LossExceedanceValues.Values(k) - LossExceedanceValues.Values(k + 1)) / (LossExceedanceValues.Keys(k) - LossExceedanceValues.Keys(k + 1))
                        Dim coeff2 As Double = LossExceedanceValues.Values(k) - LossExceedanceValues.Keys(k) * coeff1
                        RedLineIntersection = (RedLineValue - coeff2) / coeff1
                        Exit For
                    End If
                Next
                ' calculate green line value
                For k As Integer = 0 To LossExceedanceValues.Count - 2
                    If LossExceedanceValues.Keys(k) <= GreenLineValue AndAlso GreenLineValue < LossExceedanceValues.Keys(k + 1) Then
                        Dim coeff1 As Double = (LossExceedanceValues.Values(k) - LossExceedanceValues.Values(k + 1)) / (LossExceedanceValues.Keys(k) - LossExceedanceValues.Keys(k + 1))
                        Dim coeff2 As Double = LossExceedanceValues.Values(k) - LossExceedanceValues.Keys(k) * coeff1
                        GreenLineIntersection = coeff1 * GreenLineValue + coeff2
                        Exit For

                    End If
                Next
            End If

            ProjectManager.CalculationsManager.PrioritiesCacheManager.Enabled = oldCacheValue
            MiscFuncs.PrintDebugInfo("-- end old simulations -- ")
        End Sub

        Public Sub SimulateNew()
            MiscFuncs.PrintDebugInfo("-- start new simulations -- ")

            Dim Rand As Random
            If Not KeepRandomSeed Then
                Rand = New Random
            Else
                Rand = New Random(RandomSeed)
            End If

            Dim oldCacheValue As Boolean = ProjectManager.CalculationsManager.PrioritiesCacheManager.Enabled
            If Not oldCacheValue Then
                ProjectManager.CalculationsManager.PrioritiesCacheManager.ClearCache()
            End If
            ProjectManager.CalculationsManager.PrioritiesCacheManager.Enabled = True

            likelihoodWRTNode = hLikelihood.Nodes(0)
            impactWRTNode = hImpact.Nodes(0)

            If hLikelihood.Nodes.Contains(WRTNode) Then likelihoodWRTNode = WRTNode
            If hImpact.Nodes.Contains(WRTNode) Then impactWRTNode = WRTNode

            contributedEventsForSources = GetTerminalNodesDictionary(hLikelihood)
            contributedEventsForObjectives = GetTerminalNodesDictionary(hImpact)

            uncontributedEvents = hLikelihood.GetUncontributedAlternatives().Where(Function(alt) alt.Enabled).ToList
            Dim uncontributedEventsIDs As List(Of Integer) = uncontributedEvents.Select(Function(x) x.NodeID).ToList
            Dim uncontributedEventsCount As Integer = uncontributedEventsIDs.Count

            terminalLikelihoods = hLikelihood.GetRespectiveTerminalNodes(likelihoodWRTNode)
            terminalImpacts = hImpact.GetRespectiveTerminalNodes(impactWRTNode)
            terminalEvents = hEvents.TerminalNodes

            Dim terminalSourcesIDs As List(Of Integer) = terminalLikelihoods.Select(Function(x) x.NodeID).ToList
            Dim terminalSourcesCount As Integer = terminalSourcesIDs.Count
            Dim terminalEventsIDs As List(Of Integer) = terminalEvents.Select(Function(x) x.NodeID).ToList
            Dim terminalEventsCount As Integer = terminalEventsIDs.Count

            Dim sourcesCount As Integer = terminalLikelihoods.Count
            Dim impactsCount As Integer = terminalImpacts.Count
            Dim eventsCount As Integer = terminalEvents.Count

            Dim contributedEventsForObjectivesHashSet As New Dictionary(Of Integer, HashSet(Of Integer))(impactsCount)
            For Each objective As clsNode In terminalImpacts
                Dim hs As New HashSet(Of Integer)
                For Each e As clsNode In objective.GetNodesBelow(UNDEFINED_USER_ID)
                    hs.Add(e.NodeID)
                Next
                contributedEventsForObjectivesHashSet.Add(objective.NodeID, hs)
            Next

            Dim contributedSourcesForEvents As New Dictionary(Of Integer, List(Of Integer))
            Dim contributedObjectivesForEvents As New Dictionary(Of Integer, List(Of Integer))
            For Each alt As clsNode In hEvents.TerminalNodes
                Dim L As New List(Of Integer)
                Dim O As New List(Of Integer)
                For Each node As clsNode In ProjectManager.GetContributedCoveringObjectives(alt, hLikelihood)
                    L.Add(node.NodeID)
                Next
                For Each node As clsNode In ProjectManager.GetContributedCoveringObjectives(alt, hImpact)
                    O.Add(node.NodeID)
                Next
                contributedSourcesForEvents.Add(alt.NodeID, L)
                contributedObjectivesForEvents.Add(alt.NodeID, O)
            Next

            CreateComputedValues()

            Dim UseControls As Boolean = ProjectManager.CalculationsManager.ControlsUsageMode <> ControlsUsageMode.DoNotUse
            If UseControls Then
                CalculateVulnerabilitiesReductionsCoefficients()
            End If

            FrequencyValues.Clear()
            LossExceedanceValues.Clear()

            RedLineIntersection = UNDEFINED_INTEGER_VALUE
            GreenLineIntersection = UNDEFINED_INTEGER_VALUE
            AverageLossValue = UNDEFINED_INTEGER_VALUE

            Dim maxImpact As Double = 0
            For Each objectiveID As Integer In contributedEventsForObjectives.Keys
                Dim sum As Double = computedConsequences(objectiveID).Values.Sum
                Dim k As Double = If(sum > 1, sum, 1)
                For Each eventID As Integer In contributedEventsForObjectives(objectiveID)
                    maxImpact += computedConsequences(objectiveID)(eventID) / k * computedPriorities(objectiveID)
                Next
            Next
            Dim stepValue As Double = maxImpact / DataPointsCount

            For i As Integer = 0 To DataPointsCount - 1
                If Not LossExceedanceValues.ContainsKey(stepValue * i) Then LossExceedanceValues.Add(stepValue * i, 0) 'A1550
            Next

            ' store how many times each source / event has fired
            ' key = source ID, value = num fired
            Dim sourcesFiredCount As New Dictionary(Of Integer, Integer)(sourcesCount)
            ' add source to the dictionary to track how many times it was fired during all trials
            For Each source As clsNode In terminalLikelihoods
                sourcesFiredCount.Add(source.NodeID, 0)
            Next

            Dim eventsFiredCount As New Dictionary(Of Integer, Integer)(eventsCount)
            For Each e As clsNode In terminalEvents
                eventsFiredCount.Add(e.NodeID, 0)
            Next

            ' dictionary of vulnerabilities for simulation. Value here is the number vulnerability has fired.
            Dim vulnerabilitiesFiredCount As New Dictionary(Of Integer, Dictionary(Of Integer, Integer))(sourcesCount)
            For Each source As clsNode In terminalLikelihoods
                vulnerabilitiesFiredCount.Add(source.NodeID, New Dictionary(Of Integer, Integer)(contributedEventsForSources(source.NodeID).Count))
                For Each eventID As Integer In contributedEventsForSources(source.NodeID)
                    vulnerabilitiesFiredCount(source.NodeID).Add(eventID, 0)
                Next
            Next

            mSimulatedConsequences = New Dictionary(Of Integer, Dictionary(Of Integer, Double))(impactsCount)
            'Dim mStepSimulatedConsequences As New Dictionary(Of Integer, Dictionary(Of Integer, Double))(impactsCount)
            ' initialize simulated consequences
            For Each objective As clsNode In terminalImpacts
                mSimulatedConsequences.Add(objective.NodeID, New Dictionary(Of Integer, Double)(contributedEventsForObjectives(objective.NodeID).Count))
                'If GenerateChartValues Then
                '    mStepSimulatedConsequences.Add(objective.NodeID, New Dictionary(Of Integer, Double)(contributedEventsForObjectives(objective.NodeID).Count))
                'End If
            Next

            If StoreStepValues Then
                StepsInfo = New Dictionary(Of Integer, SimulationStepInfo)(NumberOfStepsToStore)
            End If
            Dim info As New SimulationStepInfo

            AverageLossValue = 0

            Dim sourceGroupsLookup As New Dictionary(Of Integer, EventsGroup)
            Dim sourcePrecedencesLookup As New Dictionary(Of Integer, Integer)
            For Each group As EventsGroup In ProjectManager.SourceGroups.Groups
                For Each eData As EventGroupData In group.Events
                    sourceGroupsLookup.Add(eData.EventIntID, group)
                    sourcePrecedencesLookup.Add(eData.EventIntID, eData.Precedence)
                Next
            Next

            Dim eventsGroupsLookup As New Dictionary(Of Integer, EventsGroup)
            Dim eventsPrecedencesLookup As New Dictionary(Of Integer, Integer)
            For Each group As EventsGroup In ProjectManager.EventsGroups.Groups
                For Each eData As EventGroupData In group.Events
                    eventsGroupsLookup.Add(eData.EventIntID, group)
                    eventsPrecedencesLookup.Add(eData.EventIntID, eData.Precedence)
                Next
            Next

            ' For each event calculate "simulated" consequences in advance
            For Each eventID As Integer In terminalEventsIDs
                Dim multOfConsequenses As Double = 1
                Dim sumOfConsequenses As Double = 0
                For Each objID As Integer In contributedObjectivesForEvents(eventID)
                    sumOfConsequenses += computedConsequences(objID)(eventID)
                    multOfConsequenses *= 1 - computedConsequences(objID)(eventID)
                Next
                multOfConsequenses = 1 - multOfConsequenses
                Dim normalizationFactor As Double = If(sumOfConsequenses = 0, 1, sumOfConsequenses)
                For Each objID As Integer In contributedObjectivesForEvents(eventID)
                    If Not mSimulatedConsequences(objID).ContainsKey(eventID) Then mSimulatedConsequences(objID).Add(eventID, 0)
                    Dim d As Double = computedConsequences(objID)(eventID) * multOfConsequenses / normalizationFactor
                    mSimulatedConsequences(objID)(eventID) = d
                Next
            Next

            Dim eventMultipliers As New Dictionary(Of Integer, Integer)
            Dim isMixedMode As Boolean = ProjectManager.PipeParameters.ProjectType = Canvas.CanvasTypes.ProjectType.ptMixed
            Dim isMyRiskRewardMode As Boolean = ProjectManager.PipeParameters.ProjectType = Canvas.CanvasTypes.ProjectType.ptMyRiskReward   ' D6798
            For Each e As clsNode In terminalEvents
                eventMultipliers.Add(e.NodeID, If(isMixedMode OrElse isMyRiskRewardMode, If(ProjectManager.CalculationsManager.IsOpportunity(e), 1, -1), 1))    ' D6798
            Next

            MiscFuncs.PrintDebugInfo("-- start new trials cycle -- ")
            ' starting simulation

            Dim sID As Integer ' will be used in the loop for sources
            Dim eID As Integer ' will be used in the loop for events with no source

            Dim UseSourceGroupsAndExist As Boolean = UseSourceGroups And ProjectManager.SourceGroups.Groups.Count > 0
            Dim UseEventsGroupsAndExist As Boolean = UseEventsGroups And ProjectManager.EventsGroups.Groups.Count > 0

            For i As Integer = 1 To NumberOfTrials

                If SimuationCancelled IsNot Nothing Then 
                    If i Mod 100 = 0 AndAlso SimuationCancelled() Then 
                        Exit For
                    End If
                End If

                If UseSourceGroupsAndExist Then
                    For Each group As EventsGroup In ProjectManager.SourceGroups.Groups
                        group.TempFiredID = -1
                        group.TempFiredPrecedence = Integer.MaxValue
                    Next
                End If

                If UseEventsGroupsAndExist Then
                    For Each group As EventsGroup In ProjectManager.EventsGroups.Groups
                        group.TempFiredID = -1
                        group.TempFiredPrecedence = Integer.MaxValue
                    Next
                End If

                If StoreStepValues AndAlso i <= NumberOfStepsToStore Then
                    info = New SimulationStepInfo
                    info.SourcesInfo = New Dictionary(Of Integer, TupleBDD)(terminalSourcesCount)
                    StepsInfo.Add(i, info)
                End If

                ' dictionary of sources that have fired on this current trial
                Dim firedSources As New HashSet(Of Integer)

                For j As Integer = 0 To terminalSourcesCount - 1
                    sID = terminalSourcesIDs(j)
                    Dim randForSource As Double = Rand.NextDouble
                    'lastRandom = randForSource
                    Dim fired As Boolean = randForSource <= computedLikelihoods(sID)

                    If UseSourceGroupsAndExist Then
                        Dim group As EventsGroup = If(sourceGroupsLookup.ContainsKey(sID), sourceGroupsLookup(sID), Nothing)
                        If group IsNot Nothing Then
                            If group.TempFiredPrecedence < sourcePrecedencesLookup(sID) Then
                                fired = False
                            Else
                                If group.TempFiredID <> -1 AndAlso firedSources.Contains(group.TempFiredID) Then
                                    firedSources.Remove(group.TempFiredID)
                                    sourcesFiredCount(group.TempFiredID) -= 1
                                End If

                                group.TempFiredID = sID
                                group.TempFiredSourceID = sID
                                group.TempFiredPrecedence = sourcePrecedencesLookup(sID)
                            End If
                        End If
                    End If

                    If fired Then
                        ' add source to fired source list for this current trial
                        firedSources.Add(sID)
                        ' increase num fired in the dictionary
                        sourcesFiredCount(sID) += 1
                    End If
                    If StoreStepValues AndAlso i <= NumberOfStepsToStore Then
                        info.SourcesInfo.Add(sID, New TupleBDD With {.Item1 = fired, .Item2 = computedLikelihoods(sID), .Item3 = randForSource})
                    End If
                Next

                ' dictionary of events that have fired on this current trial
                Dim firedEvents As New List(Of Integer)(eventsCount)

                ' check if events with no source fired
                For j As Integer = 0 To uncontributedEventsCount - 1
                    eID = uncontributedEventsIDs(j)
                    Dim randForEvent As Double = Rand.NextDouble
                    'lastRandom = randForEvent
                    Dim fired As Boolean = randForEvent <= computedVulnerabilities(-1)(eID)
                    If fired Then
                        firedEvents.Add(eID)
                        eventsFiredCount(eID) += 1
                    End If
                    If StoreStepValues AndAlso i <= NumberOfStepsToStore Then
                        info.FiredEventsInfo.Add(eID, New TupleIDD With {.Item1 = -1, .Item2 = computedVulnerabilities(-1)(eID), .Item3 = randForEvent, .Item7 = fired})
                    End If
                Next

                Dim sourceID As Integer
                Dim eventID As Integer
                ' for each fired source we check events that are contributed to it
                Dim sIDs As List(Of Integer) = Shuffle(terminalSourcesIDs, Rand)
                For j As Integer = 0 To sIDs.Count - 1
                    sourceID = sIDs(j)
                    Dim eventsBelow As List(Of Integer) = Shuffle(contributedEventsForSources(sourceID), Rand)
                    If Not firedSources.Contains(sourceID) Then
                        Dim dummyRandForEventWRTSource As Double
                        For t As Integer = 1 To eventsBelow.Count
                            dummyRandForEventWRTSource = Rand.NextDouble
                        Next
                        'lastRandom = dummyRandForEventWRTSource
                    Else
                        For k As Integer = 0 To eventsBelow.Count - 1
                            eventID = eventsBelow(k)
                            ' v = vunlerability of event "e" wrt covering source "source"
                            Dim v As Double = 0
                            If computedVulnerabilities.ContainsKey(sourceID) AndAlso computedVulnerabilities(sourceID).ContainsKey(eventID) Then
                                v = computedVulnerabilities(sourceID)(eventID)
                                If UseControls Then
                                    v *= controlsCoefficientsForVulnerabilities(sourceID)(eventID)
                                End If
                            End If
                            Dim randForEventWRTSource As Double = Rand.NextDouble
                            'lastRandom = randForEventWRTSource
                            Dim fired As Boolean = randForEventWRTSource <= v
                            If StoreStepValues AndAlso i <= NumberOfStepsToStore AndAlso Not info.FiredEventsInfo.ContainsKey(eventID) Then
                                info.FiredEventsInfo.Add(eventID, New TupleIDD With {.Item1 = sourceID, .Item2 = v, .Item3 = randForEventWRTSource, .Item7 = fired})
                            End If
                            If fired Then
                                If UseEventsGroupsAndExist Then
                                    Dim group As EventsGroup = If(eventsGroupsLookup.ContainsKey(eventID), eventsGroupsLookup(eventID), Nothing)
                                    If group IsNot Nothing Then
                                        If group.TempFiredPrecedence < eventsPrecedencesLookup(eventID) Then
                                            fired = False
                                        Else
                                            If group.TempFiredID <> -1 AndAlso firedEvents.Contains(group.TempFiredID) Then
                                                firedEvents.Remove(group.TempFiredID)
                                                eventsFiredCount(group.TempFiredID) -= 1
                                                vulnerabilitiesFiredCount(group.TempFiredSourceID)(group.TempFiredID) -= 1
                                                If StoreStepValues AndAlso i <= NumberOfStepsToStore Then
                                                    info.FiredEventsInfo.Remove(group.TempFiredID)
                                                End If
                                            End If

                                            group.TempFiredID = eventID
                                            group.TempFiredSourceID = sourceID
                                            group.TempFiredPrecedence = eventsPrecedencesLookup(eventID)
                                        End If
                                    End If
                                End If
                                If fired Then
                                    If Not firedEvents.Contains(eventID) Then
                                        ' we count event as fired only once
                                        firedEvents.Add(eventID)
                                        eventsFiredCount(eventID) += 1
                                        vulnerabilitiesFiredCount(sourceID)(eventID) += 1
                                    End If
                                End If
                            End If
                        Next
                    End If
                Next

                Dim stepImpact As Double = 0

                For j As Integer = 0 To firedEvents.Count - 1
                    eventID = firedEvents(j)
                    Dim eventImpact As Double = 0
                    Dim objID As Integer
                    For k As Integer = 0 To contributedObjectivesForEvents(eventID).Count - 1
                        objID = contributedObjectivesForEvents(eventID)(k)
                        'stepImpact += If(ProjectManager.PipeParameters.ProjectType = Canvas.CanvasTypes.ProjectType.ptMixed, If(ProjectManager.CalculationsManager.IsOpportunity(hEvents.GetNodeByID(eventID)), 1, -1), 1) * mSimulatedConsequences(objID)(eventID) * computedPriorities(objID)
                        stepImpact += eventMultipliers(eventID) * mSimulatedConsequences(objID)(eventID) * computedPriorities(objID)

                        If (StoreStepValues AndAlso i <= NumberOfStepsToStore) Then
                            eventImpact += mSimulatedConsequences(objID)(eventID) * computedPriorities(objID)
                        End If
                    Next
                    If (StoreStepValues AndAlso i <= NumberOfStepsToStore AndAlso info.FiredEventsInfo.ContainsKey(eventID)) Then
                        info.FiredEventsInfo(eventID).Item4 = eventImpact
                        info.FiredEventsInfo(eventID).Item5 = info.FiredEventsInfo(eventID).Item2 * eventImpact
                        info.FiredEventsInfo(eventID).Item6 = info.FiredEventsInfo(eventID).Item5 * If(info.FiredEventsInfo(eventID).Item1 >= 0, computedLikelihoods(info.FiredEventsInfo(eventID).Item1), 1) 'A1600
                    End If
                Next

                If (StoreStepValues AndAlso i <= NumberOfStepsToStore) Then
                    info.Impact = stepImpact
                End If
                AverageLossValue += stepImpact

                If GenerateChartValues Then
                    If Not FrequencyValues.ContainsKey(stepImpact) Then FrequencyValues.Add(stepImpact, 0)
                    FrequencyValues(stepImpact) += 1
                    For k As Integer = 0 To LossExceedanceValues.Keys.Count - 1
                        If LossExceedanceValues.Keys(k) < stepImpact Then
                            LossExceedanceValues(LossExceedanceValues.Keys(k)) += 1
                        End If
                    Next
                End If
            Next
            MiscFuncs.PrintDebugInfo("-- end new trials cycle -- ")

            ' calculate simulated likelihoods for sources:
            ' take the number a source fired and divide by the number of trials
            ' inside the loop we're also calculating simulated vulnerabilities for each contributed event for this source (num fired / num trials)
            For Each source As clsNode In terminalLikelihoods
                source.SimulatedPriority = sourcesFiredCount(source.NodeID) / NumberOfTrials

                ' calculate simulated vulnerabilities
                source.SimulatedVulnerabilities.Clear()
                mSimulatedVulnerabilities.Clear()
                Dim eventsBelow As List(Of Integer) = contributedEventsForSources(source.NodeID)
                For Each eventID As Integer In eventsBelow
                    source.SimulatedVulnerabilities.Add(eventID, vulnerabilitiesFiredCount(source.NodeID)(eventID) / NumberOfTrials)
                    mSimulatedVulnerabilities.Add(eventID, vulnerabilitiesFiredCount(source.NodeID)(eventID) / NumberOfTrials)
                Next
            Next

            ' calculate simulated impacts for objectives
            For Each objective As clsNode In terminalImpacts
                objective.SimulatedPriority = mSimulatedConsequences(objective.NodeID).Sum(Function(obj) obj.Value) / NumberOfTrials
            Next

            ' calculate simulated likelihoods for events with no source:
            ' take the number an event with no source fired and divide by the number of trials
            'MiscFuncs.PrintDebugInfo(vbNewLine + "Events fired count (" + UseControls.ToString + ") :")
            For Each e As clsNode In terminalEvents
                'MiscFuncs.PrintDebugInfo(e.NodeName + " - " + eventsFiredCount(e.NodeID).ToString)
                e.SimulatedAltLikelihood = eventsFiredCount(e.NodeID) / NumberOfTrials
            Next

            ' calculate simulated consequences:
            ' for each consequence of e to objective O we take aggregated simulated consequence value and divide it by the number e has fired
            ' inside the loop we're also calculating simulated vulnerabilities for each contributed event for this source (num fired / num trials)


            'For Each objectiveID As Integer In contributedEventsForObjectives.Keys
            '    For Each eventID As Integer In contributedEventsForObjectives(objectiveID)
            '        'Dim numFired As Integer = eventsFiredCount(eventID)
            '        'If mSimulatedConsequences(objectiveID).ContainsKey(eventID) Then mSimulatedConsequences(objectiveID)(eventID) /= If(numFired = 0, 1, numFired) 'A1545
            '        If mSimulatedConsequences(objectiveID).ContainsKey(eventID) Then mSimulatedConsequences(objectiveID)(eventID) *= hEvents.GetNodeByID(eventID).SimulatedAltLikelihood
            '    Next
            'Next

            ' calculate simulated impact values for each event e:
            ' event impact = sum of ((consequence of e to objective j) * priority of objective j), for each objective that this event contributes to
            For Each e As clsNode In terminalEvents
                e.SimulatedAltImpact = 0
                For Each objective As clsNode In terminalImpacts
                    If mSimulatedConsequences(objective.NodeID).ContainsKey(e.NodeID) Then
                        objective.SimulatedConsequences.Add(e.NodeID, mSimulatedConsequences(objective.NodeID)(e.NodeID)) 'A1622
                        e.SimulatedAltImpact += mSimulatedConsequences(objective.NodeID)(e.NodeID) * computedPriorities(objective.NodeID)
                    End If
                Next
                e.SimulatedAltImpact *= If(ProjectManager.PipeParameters.ProjectType = Canvas.CanvasTypes.ProjectType.ptMixed OrElse ProjectManager.PipeParameters.ProjectType = Canvas.CanvasTypes.ProjectType.ptMyRiskReward, If(ProjectManager.CalculationsManager.IsOpportunity(e), 1, -1), 1)  ' D6798
            Next

            ' calculate simulated risk value: sum of (SimulatedLikelihood * SimulatedImpact) for each event
            mSimulatedRisk = 0
            For Each e As clsNode In terminalEvents
                mSimulatedRisk += e.SimulatedAltLikelihood * e.SimulatedAltImpact
            Next

            AverageLossValue /= NumberOfTrials

            If GenerateChartValues Then
                For k As Integer = 0 To LossExceedanceValues.Keys.Count - 1
                    LossExceedanceValues(LossExceedanceValues.Keys(k)) /= NumberOfTrials
                Next

                ' calculate red line value
                For k As Integer = 0 To LossExceedanceValues.Count - 2
                    If LossExceedanceValues.Values(k) >= RedLineValue AndAlso RedLineValue > LossExceedanceValues.Values(k + 1) Then
                        Dim coeff1 As Double = (LossExceedanceValues.Values(k) - LossExceedanceValues.Values(k + 1)) / (LossExceedanceValues.Keys(k) - LossExceedanceValues.Keys(k + 1))
                        Dim coeff2 As Double = LossExceedanceValues.Values(k) - LossExceedanceValues.Keys(k) * coeff1
                        RedLineIntersection = (RedLineValue - coeff2) / coeff1
                        Exit For
                    End If
                Next
                ' calculate green line value
                For k As Integer = 0 To LossExceedanceValues.Count - 2
                    If LossExceedanceValues.Keys(k) <= GreenLineValue AndAlso GreenLineValue < LossExceedanceValues.Keys(k + 1) Then
                        Dim coeff1 As Double = (LossExceedanceValues.Values(k) - LossExceedanceValues.Values(k + 1)) / (LossExceedanceValues.Keys(k) - LossExceedanceValues.Keys(k + 1))
                        Dim coeff2 As Double = LossExceedanceValues.Values(k) - LossExceedanceValues.Keys(k) * coeff1
                        GreenLineIntersection = coeff1 * GreenLineValue + coeff2
                        Exit For

                    End If
                Next
            End If

            ProjectManager.CalculationsManager.PrioritiesCacheManager.Enabled = oldCacheValue
            MiscFuncs.PrintDebugInfo("-- end new simulations -- ")
        End Sub

    End Class

End Namespace
