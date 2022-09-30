Option Strict On

Imports ExpertChoice.Service
Imports System.Linq
Imports ECCore
Imports Canvas
Imports ECCore.MiscFuncs
Imports System.Collections.Concurrent
Imports System.Threading.Tasks

Namespace Canvas
    Public Enum ObjectiveFunctionType
        SumProduct = 0
        Combinatorial = 1
    End Enum

    Partial Public Class RASolver

        Private Const UseActiveControlsOnly As Boolean = False 'A1232
        Public Property isOpportunityModel As Boolean = False 'A1436 - 'TODO: AC to get this property grom clsProject

        Public isEfficientFrontierRunning As Boolean = False

        Private IsCancelled As Boolean = False
        <NonSerialized> Private timer As New Stopwatch
        Private updateIntervalMilliseconds As Integer = 1000

        Private EFValues As New SortedDictionary(Of Double, Double) ' used in MinBenefitIncrease EF type
        Private EFFundedCostValues As New HashSet(Of Double) ' used in MinBenefitIncrease EF type
        Public Property WRTNode As clsNode = Nothing

        Public Property ObjectiveFunctionType As ObjectiveFunctionType = ObjectiveFunctionType.SumProduct

        Public ReadOnly Property Controls As clsControls
            Get
                Return ProjectManager.Controls
            End Get
        End Property

        Public ReadOnly Property Sources As List(Of clsNode)
            Get
                Return ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).TerminalNodes
            End Get
        End Property

        Public ReadOnly Property Objectives As List(Of clsNode)
            Get
                Return ProjectManager.Hierarchy(ECHierarchyID.hidImpact).TerminalNodes
            End Get
        End Property

        Public ReadOnly Property IsUsingSimulations As Boolean
            Get
                Return ProjectManager.Parameters.Riskion_Use_Simulated_Values = SimulatedValuesUsageMode.SimulatedOutput OrElse ProjectManager.Parameters.Riskion_Use_Simulated_Values = SimulatedValuesUsageMode.SimulatedInputAndOutput
            End Get
        End Property

        Public Function GetRisk(UseControls As ControlsUsageMode, Optional GenerateChartValues As Boolean = False, Optional StoreStepValues As Boolean = False) As Double
            Dim tOldConsequenceSimulationsMode As ConsequencesSimulationsMode = ProjectManager.CalculationsManager.ConsequenceSimulationsMode
            If ProjectManager.CalculationsManager.ConsequenceSimulationsMode <> ConsequencesSimulationsMode.Diluted Then
                ProjectManager.CalculationsManager.ConsequenceSimulationsMode = ConsequencesSimulationsMode.Diluted
            End If
            'A1530 ===
            Dim tSimulatedRisk As Double = 0
            If IsUsingSimulations Then
                ProjectManager.RiskSimulations.SimulateCommon(COMBINED_USER_ID, UseControls, WRTNode,, GenerateChartValues, StoreStepValues) 'A1587
                tSimulatedRisk = ProjectManager.RiskSimulations.SimulatedRisk
            End If
            If ProjectManager.CalculationsManager.ConsequenceSimulationsMode <> tOldConsequenceSimulationsMode Then
                ProjectManager.CalculationsManager.ConsequenceSimulationsMode = tOldConsequenceSimulationsMode
            End If
            Return If(IsUsingSimulations, tSimulatedRisk, GetComputedRisk(UseControls))
            'A1530 ==
        End Function

        Public Function GetComputedRisk(UseControls As ControlsUsageMode, Optional EventsList As List(Of Guid) = Nothing) As Double
            'ProjectManager.CalculationsManager.UseSimulatedValues = IsUsingSimulations
            'If ProjectManager.CalculationsManager.UseSimulatedValues AndAlso Not isEfficientFrontierRunning Then 'A1415
            '    'LEC.InitSimulatedValues(UseControls, isOpportunityModel)
            '    Dim dMaxLossValue As Double
            '    Dim simData As List(Of clsSimulationData) = LEC.SimulateLossExceedance(COMBINED_USER_ID, "", ProjectManager.ActiveAlternatives.TerminalNodes.Where(Function(a) a.Enabled).Select(Function(x) x.NodeID).ToList, clsLEC._SIMULATE_SOURCES, False, UseControls, dMaxLossValue, New Random(LEC.RandSeed), True, False, 0, Nothing, Guid.Empty, False, Guid.Empty, isOpportunityModel)
            'End If
            Dim oldUseSimulations As Boolean = ProjectManager.CalculationsManager.UseSimulatedValues
            ProjectManager.CalculationsManager.UseSimulatedValues = False
            Dim riskData As RiskDataWRTNodeDataContract = ProjectManager.CalculationsManager.GetRiskDataWRTNode(COMBINED_USER_ID, "", ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).Nodes(0).NodeGuidID, UseControls, EventsList)
            ProjectManager.CalculationsManager.UseSimulatedValues = oldUseSimulations
            'ProjectManager.CalculationsManager.UseSimulatedValues = False 'A1388
            Return riskData.AlternativesData.Sum(Function(alt) alt.RiskValue) 'A1430
        End Function

        Public Sub InitOriginalValues()
            With Me
                Dim SelectedEventIDs As New List(Of Guid)
                Dim AH As clsHierarchy = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy)
                If AH IsNot Nothing Then
                    Dim Alts As IEnumerable(Of clsNode) = AH.TerminalNodes.Where(Function(alt) alt.Enabled)
                    If Alts IsNot Nothing AndAlso Alts.Count > 0 Then
                        For Each tAlt As clsNode In Alts
                            If tAlt.Enabled Then SelectedEventIDs.Add(tAlt.NodeGuidID) 'A1435
                        Next
                    End If
                End If

                CurrentScenario.OptimizedRiskValue = .GetRisk(ControlsUsageMode.UseOnlyActive)
                CurrentScenario.OriginalRiskValue = .GetRisk(ControlsUsageMode.DoNotUse)
                CurrentScenario.OriginalRiskValueWithControls = .GetRisk(ControlsUsageMode.UseAll)
            End With
        End Sub
        'A1238 ==

        'A1357
        Public Function GetControlRiskReductions(ControlID As Guid, SelectedEventIDs As List(Of Guid)) As Double
            Dim retVal As Double

            Dim PM As clsProjectManager = ProjectManager
            Dim ctrl As clsControl = PM.Controls.GetControlByID(ControlID)
            Dim oldSeed As Integer = PM.RiskSimulations.RandomSeed
            Dim oldKeepSeed As Boolean = PM.RiskSimulations.KeepRandomSeed
            If oldSeed <> 0 Then PM.RiskSimulations.RandomSeed = 0
            If Not oldKeepSeed Then PM.RiskSimulations.KeepRandomSeed = True
            Dim riskWithoutControls As Double = GetRisk(ControlsUsageMode.DoNotUse)
            Dim oldSelectedControls As New List(Of clsControl)
            oldSelectedControls.AddRange(PM.Controls.Controls)

            PM.Controls.Controls.Clear()

            If ctrl IsNot Nothing Then
                'For Each ctrl As clsControl In oldSelectedControls
                Dim oldCtrlIsEnabled As Boolean = ctrl.Enabled
                Dim oldCtrlIsActive As Boolean = ctrl.Active
                ctrl.Enabled = True
                ctrl.Active = True
                If ctrl.Enabled AndAlso ctrl.Type <> ControlType.ctUndefined Then 'A1383
                    Dim oldMust As Boolean = ctrl.Must
                    Dim oldMustNot As Boolean = ctrl.MustNot

                    ctrl.Must = False
                    ctrl.MustNot = False
                    PM.Controls.Controls.Add(ctrl)

                    Dim riskWithControl As Double = GetRisk(ControlsUsageMode.UseAll)
                    'Dim TildaRiskReduction As Double = Math.Round((riskWithoutControls - riskWithControl) * 100, 2)
                    Dim TildaRiskReduction As Double = riskWithoutControls - riskWithControl
                    'If Not retVal.ContainsKey(ctrl.ID) Then retVal.Add(ctrl.ID, TildaRiskReduction)
                    retVal = TildaRiskReduction

                    ' restore musts/must nots
                    ctrl.Must = oldMust
                    ctrl.MustNot = oldMustNot

                    PM.Controls.Controls.Clear()
                End If
                ctrl.Enabled = oldCtrlIsEnabled
                ctrl.Active = oldCtrlIsActive
                'Next
            End If

            ' restore controls list
            PM.Controls.Controls.AddRange(oldSelectedControls)

            If oldSeed <> PM.RiskSimulations.RandomSeed Then PM.RiskSimulations.RandomSeed = oldSeed
            If oldKeepSeed <> PM.RiskSimulations.KeepRandomSeed Then PM.RiskSimulations.KeepRandomSeed = oldKeepSeed

            Return retVal
        End Function
        'A1357 ==    

        Public Function HasControlsForOptimization(wrtnode As clsNode, EventIDs As List(Of Guid)) As Boolean
            Dim result As Boolean = False
            For Each control As clsControl In Controls.EnabledControls '-A1392 DefinedControls '-A1383 Controls.Controls
                If control.VarID <> 0 AndAlso ControlHasApplications(control, wrtnode, EventIDs) Then
                    result = True
                    Exit For
                End If
            Next
            Return result
        End Function

        Public Function ControlHasApplications(Control As clsControl, WRTNode As clsNode, EventIDs As List(Of Guid), Optional contributedEvents As HashSet(Of Guid) = Nothing) As Boolean
            If WRTNode Is Nothing Then Return True

            Dim isWRTForLikelihood As Boolean = WRTNode.Hierarchy.HierarchyID = ECHierarchyID.hidLikelihood

            Dim eventsWithContributionsHS As New HashSet(Of Guid)

            If contributedEvents Is Nothing Then
                'Dim contributedSources As List(Of clsNode) = If(WRTNode IsNot Nothing AndAlso isWRTForLikelihood, ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).GetRespectiveTerminalNodes(WRTNode), Sources)
                'Dim contributedObjectives As List(Of clsNode) = If(WRTNode IsNot Nothing AndAlso Not isWRTForLikelihood, ProjectManager.Hierarchy(ECHierarchyID.hidImpact).GetRespectiveTerminalNodes(WRTNode), Objectives)

                If WRTNode IsNot Nothing Then
                    Dim H As clsHierarchy = ProjectManager.Hierarchy(If(isWRTForLikelihood, ECHierarchyID.hidLikelihood, ECHierarchyID.hidImpact))
                    For Each source As clsNode In H.GetRespectiveTerminalNodes(WRTNode)
                        For Each e As clsNode In source.GetNodesBelow(UNDEFINED_USER_ID)
                            If Not eventsWithContributionsHS.Contains(e.NodeGuidID) Then
                                eventsWithContributionsHS.Add(e.NodeGuidID)
                            End If
                        Next
                    Next
                Else
                    For Each eID As Guid In EventIDs
                        eventsWithContributionsHS.Add(eID)
                    Next
                End If
            Else
                eventsWithContributionsHS = contributedEvents
            End If

            Dim uncontributedEvents As List(Of clsNode) = ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).GetUncontributedAlternatives.Where(Function(a) a.Enabled).ToList
            For Each e As clsNode In uncontributedEvents
                If Not eventsWithContributionsHS.Contains(e.NodeGuidID) AndAlso EventIDs.Contains(e.NodeGuidID) Then
                    eventsWithContributionsHS.Add(e.NodeGuidID)
                End If
            Next

            Dim hasApplications As Boolean = False
            For Each assignment As clsControlAssignment In Control.Assignments
                Dim covObj As clsNode = Nothing
                Select Case Control.Type
                    Case ControlType.ctCause, ControlType.ctCauseToEvent
                        covObj = ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(assignment.ObjectiveID)
                        If covObj IsNot Nothing AndAlso (Not isWRTForLikelihood OrElse isWRTForLikelihood AndAlso WRTNode.Hierarchy.IsChildOf(covObj, WRTNode)) Then
                            If Control.Type = ControlType.ctCauseToEvent Then
                                Dim alt As clsNode = ProjectManager.ActiveAlternatives.GetNodeByID(assignment.EventID)
                                If alt IsNot Nothing AndAlso eventsWithContributionsHS.Contains(alt.NodeGuidID) AndAlso (covObj.GetNodesBelow(UNDEFINED_USER_ID).Contains(alt) OrElse covObj Is ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).Nodes(0)) Then
                                    hasApplications = True
                                    Exit For
                                End If
                            Else
                                hasApplications = True
                                Exit For
                            End If
                        End If
                    Case ControlType.ctConsequence, ControlType.ctConsequenceToEvent
                        covObj = ProjectManager.Hierarchy(ECHierarchyID.hidImpact).GetNodeByID(assignment.ObjectiveID)
                        If covObj IsNot Nothing AndAlso (isWRTForLikelihood OrElse Not isWRTForLikelihood AndAlso WRTNode.Hierarchy.IsChildOf(covObj, WRTNode)) Then
                            Dim alt As clsNode = ProjectManager.ActiveAlternatives.GetNodeByID(assignment.EventID)
                            If alt IsNot Nothing AndAlso eventsWithContributionsHS.Contains(alt.NodeGuidID) AndAlso covObj.GetNodesBelow(UNDEFINED_USER_ID).Contains(alt) Then
                                hasApplications = True
                                Exit For
                            End If
                        End If
                End Select
            Next

            Return hasApplications
        End Function

        Private Sub GetBARONStrings(Path As String, ResName As String, EventIDs As List(Of Guid), ByRef Options As String, ByRef Variables As String, ByRef Budget As String, ByRef Musts As String, ByRef MustNots As String, ByRef Groups As String, ByRef Dependencies As String, ByRef TimePeriods As String, ByRef TimePeriodsDependencies As String, ByRef EquationsList As List(Of String), ByRef Risk As String, Optional IsForEfficientFrontier As Boolean = False)
            'If Not HasControlsForOptimization(WRTNode) Then
            '    Exit Sub
            'End If

            Controls.SetControlsVars(EventIDs)

            Dim includeZeroEffectivenessControls As Boolean = False

            Dim tOldUseReductions As ControlsUsageMode = ProjectManager.CalculationsManager.ControlsUsageMode

            Dim isWRTForLikelihood As Boolean = False
            If WRTNode IsNot Nothing Then
                If WRTNode.Hierarchy.HierarchyID = ECHierarchyID.hidLikelihood Then
                    isWRTForLikelihood = True
                Else
                    isWRTForLikelihood = False
                End If
            End If

            Dim ct As New clsCalculationTarget(CalculationTargetType.cttCombinedGroup, ProjectManager.CombinedGroups.GetDefaultCombinedGroup)
            Dim oldH As Integer = ProjectManager.ActiveHierarchy
            ProjectManager.ActiveHierarchy = ECHierarchyID.hidImpact
            ProjectManager.CalculationsManager.Calculate(ct, If(WRTNode IsNot Nothing AndAlso Not isWRTForLikelihood, WRTNode, ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).Nodes(0)))
            ProjectManager.ActiveHierarchy = ECHierarchyID.hidLikelihood
            ProjectManager.CalculationsManager.Calculate(ct, If(WRTNode IsNot Nothing AndAlso isWRTForLikelihood, WRTNode, ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).Nodes(0)))
            ProjectManager.ActiveHierarchy = oldH

            Dim sDescription As String = String.Format("// === Created with ExpertChoice Riskion Optimizer ===" + vbNewLine + "// Project: {0} [#{4}]" + vbNewLine + "// Date: {1}" + vbNewLine + "// User: <{2}> {3}" + vbNewLine + vbNewLine, ProjectManager.PipeParameters.ProjectName, Now.ToString, ProjectManager.User.UserEMail, ProjectManager.User.UserName, ProjectManager.StorageManager.ModelID)
            Options = String.Format("{2}OPTIONS {{ " + vbNewLine + " ResName: ""{0}{1}"";" + vbNewLine + " LocRes: 0; " + vbNewLine + "DeltaTerm: 1;" + vbNewLine + "DeltaT: 30;" + vbNewLine + " Summary: 0;" + vbNewLine + " Times: 0;" + vbNewLine + "}}" + vbNewLine + vbNewLine, Path, ResName, sDescription)
            If IsForEfficientFrontier Then
                Options = sDescription + "OPTIONS {{ " + vbNewLine + " ResName: ""{0}{1}"";" + vbNewLine + " LocRes: 0; " + vbNewLine + "DeltaTerm: 1;" + vbNewLine + "DeltaT: 30;" + vbNewLine + " Summary: 0;" + vbNewLine + " Times: 0;" + vbNewLine + "}}" + vbNewLine + vbNewLine
            Else
                Options = String.Format("{2}OPTIONS {{ " + vbNewLine + " ResName: ""{0}{1}"";" + vbNewLine + " LocRes: 0; " + "DeltaTerm: 1;" + vbNewLine + "DeltaT: 30;" + vbNewLine + vbNewLine + " Summary: 0;" + vbNewLine + vbNewLine + " Times: 0;" + vbNewLine + "}}" + vbNewLine + vbNewLine, Path, ResName, sDescription)
            End If
            'Options = String.Format("{2}OPTIONS {{ " + vbNewLine + " ResName: ""{0}{1}"";" + vbNewLine + " LocRes: 0; " + vbNewLine + " Summary: 0;" + vbNewLine + " NumSol: 10;" + vbNewLine + " Times: 0;" + vbNewLine + "}}" + vbNewLine + vbNewLine, Path, ResName, sDescription)
            'If IsForEfficientFrontier Then
            '    Options = sDescription + "OPTIONS {{ " + vbNewLine + " ResName: ""{0}{1}"";" + vbNewLine + " LocRes: 0; " + vbNewLine + " Summary: 0;" + vbNewLine + " NumSol: 10;" + vbNewLine + " Times: 0;" + vbNewLine + "}}" + vbNewLine + vbNewLine
            'Else
            '    Options = String.Format("{2}OPTIONS {{ " + vbNewLine + " ResName: ""{0}{1}"";" + vbNewLine + " LocRes: 0; " + vbNewLine + " Summary: 0;" + vbNewLine + " NumSol: 10;" + vbNewLine + " Times: 0;" + vbNewLine + "}}" + vbNewLine + vbNewLine, Path, ResName, sDescription)
            'End If

            Budget = ""
            Variables = "BINARY_VARIABLES "

            Musts = ""
            MustNots = ""
            Dim mustsCount As Integer = 0
            Dim mustnotsCount As Integer = 0

            Dim includeGroups As Boolean = False
            If CurrentScenario.Settings.Groups And CurrentScenario.GetAvailableGroups.Count > 0 Then
                For Each group As RAGroup In CurrentScenario.GetAvailableGroups
                    If group.Enabled AndAlso group.Alternatives.Count > 0 Then
                        includeGroups = True
                    End If
                Next
            End If

            Dim supportingControls As New HashSet(Of Integer)
            Dim gVars As String = ""
            If includeGroups Then
                Dim gIntID As Integer = 0
                For Each group As RAGroup In CurrentScenario.GetAvailableGroups
                    If group.Enabled AndAlso group.Condition = RAGroupCondition.gcAllOrNothing AndAlso group.Alternatives.Values.Where(Function(x) x.Enabled).Count > 1 Then
                        Dim actualCount As Integer = 0 ' in order to avoid controls with no assignments
                        For Each alt As RAAlternative In group.Alternatives.Values.Where(Function(x) x.Enabled)
                            Dim control As clsControl = Controls.GetControlByID(New Guid(alt.ID))
                            If control IsNot Nothing AndAlso control.VarID <> 0 Then
                                If Not supportingControls.Contains(control.VarID) Then
                                    supportingControls.Add(control.VarID)
                                End If
                                actualCount += 1
                            End If
                        Next
                        If actualCount > 1 Then
                            gVars += If(gVars = "", "g_" + gIntID.ToString, ", " + "g_" + gIntID.ToString)
                        End If
                        gIntID += 1
                    End If
                Next
            End If

            Dim uncontributedEvents As List(Of clsNode) = ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).GetUncontributedAlternatives.Where(Function(a) a.Enabled).ToList 'A1435


            Dim contributedSources As List(Of clsNode) = If(WRTNode IsNot Nothing AndAlso isWRTForLikelihood, ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).GetRespectiveTerminalNodes(WRTNode), Sources)
            Dim contributedObjectives As List(Of clsNode) = If(WRTNode IsNot Nothing AndAlso Not isWRTForLikelihood, ProjectManager.Hierarchy(ECHierarchyID.hidImpact).GetRespectiveTerminalNodes(WRTNode), Objectives)

            Dim eventsWithContributions As New List(Of Guid)
            Dim eventsWithContributionsHS As New HashSet(Of Guid)
            If WRTNode IsNot Nothing Then
                Dim H As clsHierarchy = ProjectManager.Hierarchy(If(isWRTForLikelihood, ECHierarchyID.hidLikelihood, ECHierarchyID.hidImpact))
                For Each source As clsNode In H.GetRespectiveTerminalNodes(WRTNode)
                    For Each e As clsNode In source.GetNodesBelow(UNDEFINED_USER_ID)
                        If Not eventsWithContributionsHS.Contains(e.NodeGuidID) Then
                            eventsWithContributionsHS.Add(e.NodeGuidID)
                            eventsWithContributions.Add(e.NodeGuidID)
                        End If
                    Next
                Next
            Else
                eventsWithContributions = EventIDs
            End If

            Dim controlsHS As New HashSet(Of Integer)

            For Each control As clsControl In Controls.EnabledControls '-A1392 DefinedControls '-A1383 Controls.Controls
                If control.VarID <> 0 AndAlso ((ControlHasApplications(control, WRTNode, EventIDs, eventsWithContributionsHS) OrElse supportingControls.Contains(control.VarID))) Then
                    controlsHS.Add(control.VarID)
                    Dim xName As String = "x_" + control.VarID.ToString
                    Variables += CStr(If(Variables = "BINARY_VARIABLES ", xName, ", " + xName))
                    Dim str As String = JS_SafeNumber(CDbl(If(Not control.IsCostDefined, 0, control.Cost))) + " * " + "x_" + control.VarID.ToString
                    Budget += CStr(If(Budget = "", str, " + " + str))
                    If Settings.Musts And control.Must Then
                        mustsCount += 1
                        Musts += CStr(If(Musts = "", "x_" + control.VarID.ToString, " + " + "x_" + control.VarID.ToString))
                    End If
                    If Settings.MustNots And control.MustNot Then
                        mustnotsCount += 1
                        MustNots += CStr(If(MustNots = "", "x_" + control.VarID.ToString, " + " + "x_" + control.VarID.ToString))
                    End If
                End If
            Next
            If gVars <> "" Then
                Variables += ", " + gVars
            End If
            'Variables += ";" + vbNewLine

            'Res += VarsStr + vbNewLine

            If Settings.Musts And mustsCount > 0 Then
                Musts += " == " + mustsCount.ToString + ";"
            End If

            If Settings.MustNots And mustnotsCount > 0 Then
                MustNots += " == 0;"
            End If

            'Budget += " <= " + JS_SafeNumber(BudgetLimit) + ";" + vbNewLine

            Dim mustEq As String = ""
            If Settings.Musts Then mustEq = CStr((If(mustsCount > 0, ", musts", "")))
            Dim mustnotsEq As String = ""
            If Settings.MustNots Then mustnotsEq = CStr((If(mustnotsCount > 0, ", mustnots", "")))

            Dim groupsEq As String = ""
            If includeGroups Then
                Dim gIntID As Integer = 0
                For Each group As RAGroup In CurrentScenario.GetAvailableGroups
                    If group.Enabled AndAlso group.Alternatives.Count > 0 Then
                        groupsEq += CStr(If(groupsEq = "", ", Group_" + gIntID.ToString, ", " + "Group_" + gIntID.ToString))
                        gIntID += 1
                    End If
                Next
            End If

            Dim includeDependencies As Boolean = False
            If CurrentScenario.Settings.Dependencies And CurrentScenario.GetAvailableDependencies.Count > 0 Then
                For Each dependency As RADependency In CurrentScenario.GetAvailableDependencies
                    If dependency.Enabled Then
                        includeDependencies = True
                    End If
                Next
            End If

            Dim dependenciesEq As String = ""
            If includeDependencies Then
                Dim dIntID As Integer = 0
                For Each dependency As RADependency In CurrentScenario.GetAvailableDependencies
                    If dependency.Enabled Then
                        dependenciesEq += CStr(If(dependenciesEq = "", ", Dependency_" + dIntID.ToString, ", " + "Dependency_" + dIntID.ToString))
                        dIntID += 1
                    End If
                Next
            End If


            Risk = ""
            Dim precision1 As Integer = 15

            For Each eventID As Guid In eventsWithContributions
                If uncontributedEvents.FirstOrDefault(Function(p) p.NodeGuidID.Equals(eventID)) Is Nothing Then
                    Dim e As clsNode = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(eventID)
                    If Not e.Enabled Then Exit For 'A1435

                    Dim eventIntID As Integer = e.NodeID

                    Dim ObjStr As String = ""

                    Select Case ProjectManager.CalculationsManager.LikelihoodsCalculationMode
                        Case LikelihoodsCalculationMode.Regular
                            For Each source As clsNode In contributedSources
                                If source.GetNodesBelow(UNDEFINED_USER_ID).Contains(e) Then 'A1435
                                    If ObjStr = "" Then ObjStr = "("

                                    Dim L As Double = source.UnnormalizedPriority

                                    Dim V As Double
                                    V = source.Judgments.Weights.GetUserWeights(COMBINED_USER_ID, ProjectManager.CalculationsManager.SynthesisMode, ProjectManager.CalculationsManager.IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(eventIntID)

                                    If L <> 0 AndAlso V <> 0 Then
                                        Dim sControls As List(Of clsControl) = Controls.GetControlsForSource(source.NodeGuidID, UseActiveControlsOnly) 'A1232
                                        Dim vControls As List(Of clsControl) = Controls.GetControlsForVulnerabilities(source.NodeGuidID, eventID, UseActiveControlsOnly) 'A1232

                                        Dim sMult As String = JS_SafeNumber(Math.Round(L, precision1)) + " * " + JS_SafeNumber(Math.Round(V, precision1))
                                        ObjStr += CStr(If(ObjStr(ObjStr.Length - 1) = "(", sMult, " + " + sMult))
                                        For Each control As clsControl In sControls
                                            If controlsHS.Contains(control.VarID) Then
                                                Dim xName As String = "x_" + control.VarID.ToString
                                                Dim value As Double = Controls.GetAssignmentValue(control.ID, source.NodeGuidID, Guid.Empty)
                                                If includeZeroEffectivenessControls OrElse Not includeZeroEffectivenessControls AndAlso value <> 0 Then
                                                    ObjStr += " * (1 - " + JS_SafeNumber(Math.Round(value, precision1)) + " * " + xName + ")"
                                                End If
                                            End If
                                        Next

                                        For Each control As clsControl In vControls
                                            If controlsHS.Contains(control.VarID) Then
                                                Dim xName As String = "x_" + control.VarID.ToString
                                                Dim value As Double = Controls.GetAssignmentValue(control.ID, source.NodeGuidID, eventID)
                                                If includeZeroEffectivenessControls OrElse Not includeZeroEffectivenessControls AndAlso value <> 0 Then
                                                    ObjStr += " * (1 - " + JS_SafeNumber(Math.Round(value, precision1)) + " * " + xName + ")"
                                                End If
                                            End If

                                        Next
                                    Else
                                        Debug.Print("L = " + L.ToString + ";   V = " + V.ToString + ";  event: " + e.NodeName + ";  source: " + source.NodeName)
                                    End If
                                End If
                            Next
                        Case LikelihoodsCalculationMode.Probability

                            For Each source As clsNode In Sources
                                If source.GetNodesBelow(UNDEFINED_USER_ID).Contains(e) Then 'A1435
                                    If ObjStr = "" Then ObjStr = "(1 - "

                                    ObjStr += CStr(If(ObjStr(ObjStr.Length - 1) = ")", " * (1 - ", " (1 - "))

                                    Dim L As Double = source.UnnormalizedPriority
                                    Dim V As Double
                                    V = source.Judgments.Weights.GetUserWeights(COMBINED_USER_ID, ProjectManager.CalculationsManager.SynthesisMode, ProjectManager.CalculationsManager.IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(eventIntID)

                                    If L <> 0 AndAlso V <> 0 Then
                                        Dim sControls As List(Of clsControl) = Controls.GetControlsForSource(source.NodeGuidID, UseActiveControlsOnly) 'A1232
                                        Dim vControls As List(Of clsControl) = Controls.GetControlsForVulnerabilities(source.NodeGuidID, eventID, UseActiveControlsOnly) 'A1232

                                        Dim sMult As String = JS_SafeNumber(Math.Round(L, precision1)) + " * " + JS_SafeNumber(Math.Round(V, precision1))
                                        'ObjStr += CStr(If(ObjStr(ObjStr.Length - 1) = "(", sMult, " + " + sMult))
                                        ObjStr += sMult
                                        For Each control As clsControl In sControls
                                            Dim xName As String = "x_" + control.VarID.ToString
                                            Dim value As Double = Controls.GetAssignmentValue(control.ID, source.NodeGuidID, Guid.Empty)
                                            If includeZeroEffectivenessControls OrElse Not includeZeroEffectivenessControls AndAlso value <> 0 Then
                                                ObjStr += " * (1 - " + JS_SafeNumber(Math.Round(value, precision1)) + " * " + xName + ")"
                                            End If
                                        Next

                                        For Each control As clsControl In vControls
                                            Dim xName As String = "x_" + control.VarID.ToString
                                            Dim value As Double = Controls.GetAssignmentValue(control.ID, source.NodeGuidID, eventID)
                                            If includeZeroEffectivenessControls OrElse Not includeZeroEffectivenessControls AndAlso value <> 0 Then
                                                ObjStr += " * (1 - " + JS_SafeNumber(Math.Round(value, precision1)) + " * " + xName + ")"
                                            End If
                                        Next
                                    Else
                                        Debug.Print("L = " + L.ToString + ";   V = " + V.ToString + ";  event: " + e.NodeName + ";  source: " + source.NodeName)
                                    End If
                                    ObjStr += ")"
                                End If
                            Next
                    End Select

                    ObjStr += CStr(If(ObjStr = "", " (", ") * ("))

                    Dim hasAtLeastOneObjective As Boolean = False
                    For Each objective As clsNode In contributedObjectives
                        If objective.GetNodesBelow(UNDEFINED_USER_ID).Contains(e) Then 'A1435
                            Dim P As Double = objective.UnnormalizedPriority
                            Dim C As Double
                            C = objective.Judgments.Weights.GetUserWeights(COMBINED_USER_ID, ProjectManager.CalculationsManager.SynthesisMode, ProjectManager.CalculationsManager.IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(eventIntID)

                            If P <> 0 AndAlso C <> 0 Then
                                hasAtLeastOneObjective = True
                                Dim sMult As String = JS_SafeNumber(Math.Round(P, precision1)) + " * " + JS_SafeNumber(Math.Round(C, precision1))
                                ObjStr += CStr(If(ObjStr(ObjStr.Length - 1) = "(", sMult, " + " + sMult))

                                Dim cControls As List(Of clsControl) = Controls.GetControlsForConsequences(objective.NodeGuidID, eventID, UseActiveControlsOnly) 'A1232
                                For Each control As clsControl In cControls
                                    If controlsHS.Contains(control.VarID) Then
                                        Dim xName As String = "x_" + control.VarID.ToString
                                        Dim value As Double = Controls.GetAssignmentValue(control.ID, objective.NodeGuidID, eventID)
                                        If includeZeroEffectivenessControls OrElse Not includeZeroEffectivenessControls AndAlso value <> 0 Then
                                            ObjStr += " * (1 - " + JS_SafeNumber(Math.Round(value, precision1)) + " * " + xName + ")"
                                        End If
                                    End If
                                Next
                            Else
                                Debug.Print("P = " + P.ToString + ";   C = " + C.ToString + ";  event: " + e.NodeName + ";  objective: " + objective.NodeName)
                            End If
                        End If
                    Next
                    ObjStr += ")"

                    If ObjStr.Length > 4 Then
                        Dim s As String = ObjStr.Substring(ObjStr.Length - 4)
                        If s.Substring(s.Length - 4) = "* ()" Then
                            'ObjStr = ObjStr.Substring(0, ObjStr.Length - 4)
                            ObjStr = ""
                        End If
                    End If

                    If ObjStr <> "() " AndAlso ObjStr <> "" Then
                        'Risk += CStr(If(Risk = "", ObjStr, " + " + ObjStr))
                        Dim isOpportunity As Boolean = If(ProjectManager.PipeParameters.ProjectType = Canvas.CanvasTypes.ProjectType.ptMixed OrElse ProjectManager.PipeParameters.ProjectType = Canvas.CanvasTypes.ProjectType.ptMyRiskReward, If(ProjectManager.CalculationsManager.IsOpportunity(ProjectManager.ActiveAlternatives.GetNodeByID(eventID)), True, False), True) ' D6798

                        Risk += CStr(If(Risk = "", If(isOpportunity, "", " - ") + ObjStr, If(isOpportunity, " + ", " - ") + ObjStr))
                    End If
                End If
                Risk += vbNewLine
            Next

            If uncontributedEvents.Count > 0 Then
                For Each e As clsNode In uncontributedEvents
                    Dim V As Double
                    V = e.UnnormalizedPriority

                    Dim eventRisk As String = ""

                    Dim vControls As List(Of clsControl) = Controls.GetControlsForVulnerabilities(ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).Nodes(0).NodeGuidID, e.NodeGuidID, UseActiveControlsOnly)
                    If V <> 0 Then
                        Dim sMult As String = JS_SafeNumber(Math.Round(V, precision1))
                        'Risk += CStr(If(Risk = "", sMult, " + " + sMult))
                        'Dim isOpportunity As Boolean = ProjectManager.CalculationsManager.IsOpportunity(e)
                        Dim isOpportunity As Boolean = If(ProjectManager.PipeParameters.ProjectType = Canvas.CanvasTypes.ProjectType.ptMixed OrElse ProjectManager.PipeParameters.ProjectType = Canvas.CanvasTypes.ProjectType.ptMyRiskReward, If(ProjectManager.CalculationsManager.IsOpportunity(e), True, False), True)  ' D6798

                        eventRisk = CStr(If(isOpportunity, "", " - ") + sMult)

                        For Each control As clsControl In vControls
                            If controlsHS.Contains(control.VarID) Then
                                Dim xName As String = "x_" + control.VarID.ToString
                                Dim value As Double = Controls.GetAssignmentValue(control.ID, ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).Nodes(0).NodeGuidID, e.NodeGuidID)
                                If includeZeroEffectivenessControls OrElse Not includeZeroEffectivenessControls AndAlso value <> 0 Then
                                    eventRisk += " * (1 - " + JS_SafeNumber(Math.Round(value, precision1)) + " * " + xName + ")"
                                End If
                            End If
                        Next

                        Dim added As Boolean = False
                        'TotalObjStr += " * ("

                        For Each objective As clsNode In contributedObjectives
                            If objective.GetNodesBelow(UNDEFINED_USER_ID).Contains(e) Then 'A1435
                                Dim P As Double = objective.UnnormalizedPriority
                                Dim C As Double
                                C = objective.Judgments.Weights.GetUserWeights(COMBINED_USER_ID, ProjectManager.CalculationsManager.SynthesisMode, ProjectManager.CalculationsManager.IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(e.NodeID)

                                If P <> 0 AndAlso C <> 0 Then
                                    If Not added Then
                                        eventRisk += " * ("
                                        added = True
                                    End If
                                    sMult = JS_SafeNumber(Math.Round(P, precision1)) + " * " + JS_SafeNumber(Math.Round(C, precision1))
                                    eventRisk += CStr(If(eventRisk(eventRisk.Length - 1) = "(", sMult, " + " + sMult))
                                    Dim cControls As List(Of clsControl) = Controls.GetControlsForConsequences(objective.NodeGuidID, e.NodeGuidID, UseActiveControlsOnly)
                                    For Each control As clsControl In cControls
                                        If controlsHS.Contains(control.VarID) Then
                                            Dim xName As String = "x_" + control.VarID.ToString
                                            Dim value As Double = Controls.GetAssignmentValue(control.ID, objective.NodeGuidID, e.NodeGuidID)
                                            If includeZeroEffectivenessControls OrElse Not includeZeroEffectivenessControls AndAlso value <> 0 Then
                                                eventRisk += " * (1 - " + JS_SafeNumber(Math.Round(value, precision1)) + " * " + xName + ")"
                                            End If
                                        End If
                                    Next
                                Else
                                    Debug.Print("P = " + P.ToString + ";   C = " + C.ToString + ";  event with no source: " + e.NodeName + ";  objective: " + objective.NodeName)
                                End If
                            End If
                        Next
                        If added Then eventRisk += ")"
                        If added Then Risk += " + " + eventRisk
                    Else
                        Debug.Print("V = " + V.ToString + ";  event with no source: " + e.NodeName)
                    End If
                Next
            End If

            Groups = ""
            If includeGroups Then
                Dim gIntID As Integer = 0
                For Each group As RAGroup In CurrentScenario.GetAvailableGroups
                    If group.Enabled AndAlso group.Alternatives.Values.Where(Function(x) x.Enabled).Count > 1 Then
                        Dim gStr As String = ""
                        Dim actualCount As Integer = 0 ' in order to avoid controls with no assignments
                        For Each alt As RAAlternative In group.Alternatives.Values.Where(Function(x) x.Enabled)
                            Dim control As clsControl = Controls.GetControlByID(New Guid(alt.ID))
                            If control IsNot Nothing AndAlso control.VarID <> 0 AndAlso (ControlHasApplications(control, WRTNode, EventIDs, eventsWithContributionsHS) OrElse supportingControls.Contains(control.VarID)) Then

                                gStr += CStr(If(gStr = "", "x_" + control.VarID.ToString, " + " + "x_" + control.VarID.ToString))
                                actualCount += 1
                            End If
                        Next

                        Select Case group.Condition
                            Case RAGroupCondition.gcLessOrEqualsOne
                                ' none of one alterantive from the group if funded
                                gStr += " <= 1;"
                            Case RAGroupCondition.gcEqualsOne
                                ' exactly one alterantive from the group is funded
                                gStr += " == 1;"
                            Case RAGroupCondition.gcGreaterOrEqualsOne
                                ' at least one alternative from the group is funded
                                gStr += " >= 1;"
                            Case RAGroupCondition.gcAllOrNothing
                                gStr += " - " + actualCount.ToString + " * g_" + gIntID.ToString + " == 0;"
                        End Select
                        If actualCount > 1 Then
                            gStr = "Group_" + gIntID.ToString + ": " + gStr
                            Groups += gStr + vbNewLine
                        End If
                        gIntID += 1
                    End If
                Next
            End If

            Dependencies = ""
            If includeDependencies Then
                Dim dIntID As Integer = 0
                For Each dependency As RADependency In CurrentScenario.GetAvailableDependencies
                    If dependency.Enabled Then
                        Dim dStr As String = ""
                        Dim control1 As clsControl = Controls.GetControlByID(New Guid(dependency.FirstAlternativeID))
                        Dim control2 As clsControl = Controls.GetControlByID(New Guid(dependency.SecondAlternativeID))
                        Dim group1 As RAGroup = Nothing
                        Dim group2 As RAGroup = Nothing
                        If control1 Is Nothing Then
                            group1 = CurrentScenario.Groups.GetGroupByID(dependency.FirstAlternativeID)
                        End If
                        If control2 Is Nothing Then
                            group2 = CurrentScenario.Groups.GetGroupByID(dependency.SecondAlternativeID)
                        End If
                        If control1 IsNot Nothing AndAlso control1.VarID <> 0 AndAlso control1.Enabled AndAlso control2 IsNot Nothing AndAlso control2.VarID <> 0 AndAlso control2.Enabled AndAlso
                            ControlHasApplications(control1, WRTNode, EventIDs, eventsWithContributionsHS) AndAlso ControlHasApplications(control2, WRTNode, EventIDs, eventsWithContributionsHS) Then

                            Select Case dependency.Value
                                Case RADependencyType.dtDependsOn
                                    ' first is not funded unless second is funded
                                    dStr = "x_" + control2.VarID.ToString + "-" + "x_" + control1.VarID.ToString + " >= 0;"
                                Case RADependencyType.dtMutuallyDependent
                                    dStr = "x_" + control1.VarID.ToString + "-" + "x_" + control2.VarID.ToString + " == 0;"
                                Case RADependencyType.dtMutuallyExclusive
                                    dStr = "x_" + control1.VarID.ToString + "+" + "x_" + control2.VarID.ToString + " <= 1;"
                            End Select
                            dStr = "Dependency_" + dIntID.ToString + ": " + dStr
                            EquationsList.Add("Dependency_" + dIntID.ToString)
                            Dependencies += dStr + vbNewLine
                        Else
                            Dim dID As Integer = 1
                            If control1 IsNot Nothing AndAlso group2 IsNot Nothing Then
                                For Each alt As RAAlternative In group2.Alternatives.Values.Where(Function(x) x.Enabled)
                                    Dim control As clsControl = Controls.GetControlByID(New Guid(alt.ID))
                                    If control IsNot Nothing AndAlso control.VarID <> 0 AndAlso ControlHasApplications(control, WRTNode, EventIDs, eventsWithContributionsHS) Then
                                        Select Case dependency.Value
                                            Case RADependencyType.dtDependsOn
                                                ' first is not funded unless second is funded
                                                dStr = "x_" + control.VarID.ToString + "-" + "x_" + control1.VarID.ToString + " >= 0;"
                                            Case RADependencyType.dtMutuallyDependent
                                                dStr = "x_" + control1.VarID.ToString + "-" + "x_" + control.VarID.ToString + " == 0;"
                                            Case RADependencyType.dtMutuallyExclusive
                                                dStr = "x_" + control1.VarID.ToString + "+" + "x_" + control.VarID.ToString + " <= 1;"
                                        End Select
                                        dStr = "Dependency_" + dIntID.ToString + "_" + dID.ToString + ": " + dStr
                                        EquationsList.Add("Dependency_" + dIntID.ToString + "_" + dID.ToString)
                                        Dependencies += dStr + vbNewLine
                                        dID += 1
                                    End If
                                Next
                            Else
                                If group1 IsNot Nothing AndAlso control2 IsNot Nothing Then
                                    For Each alt As RAAlternative In group1.Alternatives.Values.Where(Function(x) x.Enabled)
                                        Dim control As clsControl = Controls.GetControlByID(New Guid(alt.ID))
                                        If control IsNot Nothing AndAlso control.VarID <> 0 AndAlso ControlHasApplications(control, WRTNode, EventIDs, eventsWithContributionsHS) Then
                                            Select Case dependency.Value
                                                Case RADependencyType.dtDependsOn
                                                    ' first is not funded unless second is funded
                                                    dStr = "x_" + control2.VarID.ToString + "-" + "x_" + control.VarID.ToString + " >= 0;"
                                                Case RADependencyType.dtMutuallyDependent
                                                    dStr = "x_" + control.VarID.ToString + "-" + "x_" + control2.VarID.ToString + " == 0;"
                                                Case RADependencyType.dtMutuallyExclusive
                                                    dStr = "x_" + control.VarID.ToString + "+" + "x_" + control2.VarID.ToString + " <= 1;"
                                            End Select
                                            dStr = "Dependency_" + dIntID.ToString + "_" + dID.ToString + ": " + dStr
                                            EquationsList.Add("Dependency_" + dIntID.ToString + "_" + dID.ToString)
                                            Dependencies += dStr + vbNewLine
                                            dID += 1
                                        End If
                                    Next
                                End If
                            End If
                        End If
                        dIntID += 1
                    End If
                Next
            End If

            If Settings.TimePeriods And CurrentScenario.TimePeriods.Periods.Count > 1 Then
                Dim i As Integer = 1
                If CurrentScenario.OptimizationType = RiskOptimizationType.BudgetLimit Then
                    'Risk = "-1000 * (" + Risk + ")"
                    ''Risk += " + ("
                End If
                For Each control As clsControl In Controls.EnabledControls
                    If control.VarID <> 0 AndAlso ControlHasApplications(control, WRTNode, EventIDs, eventsWithContributionsHS) Then
                        Dim apd As AlternativePeriodsData = CurrentScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(control.ID.ToString)
                        Dim v As Integer = 0
                        Dim c As Double = 0.01
                        For j As Integer = apd.GetMinPeriod To apd.GetMaxPeriod
                            Dim tpVarName As String = "TPVar_" + control.VarID.ToString + "_" + j.ToString

                            Variables += ", " + tpVarName

                            If CurrentScenario.OptimizationType = RiskOptimizationType.BudgetLimit Then

                                ''Risk += "+ " + (c * (1 - CurrentScenario.TimePeriods.DiscountFactor * v)).ToString + " * " + tpVarName
                            End If

                            'If CurrentScenario.TimePeriods.UseDiscountFactor Then
                            '    altVar = gModel.AddVar(0, 1, CDbl(If(Settings.Risks, alt.BenefitOriginal * (1 - alt.RiskOriginal), alt.BenefitOriginal)) * (1 - CurrentScenario.TimePeriods.DiscountFactor * v), GRB.BINARY, "TPVar_" + alt.ID.ToString + "_" + j.ToString)
                            'Else
                            '    altVar = gModel.AddVar(0, 1, 0, GRB.BINARY, "TPVar_" + alt.ID.ToString + "_" + j.ToString)
                            'End If
                            v += 1
                            i += 1
                        Next
                    End If
                Next
                If CurrentScenario.OptimizationType = RiskOptimizationType.BudgetLimit Then
                    'Risk = "-1000 * (" + Risk + ")"
                    ''Risk += ")"
                End If

                Dim TPRowVariants As String = ""
                For Each control As clsControl In Controls.EnabledControls
                    If control.VarID <> 0 AndAlso ControlHasApplications(control, WRTNode, EventIDs, eventsWithContributionsHS) Then
                        Dim TPRowVariantEquation As String = "TPRowVariantEq_" + control.VarID.ToString + ": "
                        EquationsList.Add("TPRowVariantEq_" + control.VarID.ToString)
                        Dim apd As AlternativePeriodsData = CurrentScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(control.ID.ToString)
                        Dim v As Integer = 0
                        For j As Integer = apd.GetMinPeriod To apd.GetMaxPeriod
                            v += 1
                            Dim s As String = "TPVar_" + control.VarID.ToString + "_" + j.ToString
                            TPRowVariantEquation += If(j = apd.GetMinPeriod, s, " + " + s)
                        Next
                        TPRowVariantEquation += " - " + "x_" + control.VarID.ToString
                        TPRowVariantEquation += " == 0;"
                        TPRowVariants += TPRowVariantEquation + vbNewLine
                    End If
                Next

                Dim tpResList As New Dictionary(Of String, String)
                Dim gConstraints As New Dictionary(Of String, String)
                Dim TPResources As String = ""

                Dim t As Integer = 0
                For Each period As RATimePeriod In CurrentScenario.TimePeriods.Periods
                    For Each resource As RAResource In CurrentScenario.TimePeriods.Resources.Values
                        Dim cc As RAConstraint = CurrentScenario.Constraints.GetConstraintByID(resource.ConstraintID)
                        If resource.ID.Equals(RA_Cost_GUID) Or (resource.Enabled AndAlso (resource.ConstraintID <> -1) AndAlso (cc Is Nothing Or cc IsNot Nothing AndAlso cc.Enabled) AndAlso Settings.CustomConstraints) Then 'A1130
                            For Each control As clsControl In Controls.EnabledControls
                                If control.VarID <> 0 AndAlso ControlHasApplications(control, WRTNode, EventIDs, eventsWithContributionsHS) Then
                                    Dim apd As AlternativePeriodsData = CurrentScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(control.ID.ToString)
                                    If t >= apd.MinPeriod AndAlso t <= apd.MaxPeriod + apd.Duration - 1 Then
                                        For k As Integer = Math.Max(apd.MinPeriod, t - apd.Duration + 1) To t
                                            Dim c As Double = CurrentScenario.TimePeriods.PeriodsData.GetResourceValue(t - k, control.ID.ToString, resource.ID)
                                            If c <> UNDEFINED_INTEGER_VALUE Then
                                                Dim cTPRes As String = ""
                                                If Not tpResList.ContainsKey("TPRes_Main_" + resource.ID.ToString.Replace("-", "_") + "_" + t.ToString) Then
                                                    tpResList.Add("TPRes_Main_" + resource.ID.ToString.Replace("-", "_") + "_" + t.ToString, cTPRes)
                                                Else
                                                    cTPRes = tpResList("TPRes_Main_" + resource.ID.ToString.Replace("-", "_") + "_" + t.ToString)
                                                End If
                                                Dim s As String = c.ToString + " * TPVar_" + control.VarID.ToString + "_" + k.ToString
                                                tpResList("TPRes_Main_" + resource.ID.ToString.Replace("-", "_") + "_" + t.ToString) += If(cTPRes = "", s, " + " + s)
                                                'cTPRes.AddTerm(c, gVariables("TPVar_" + alt.ID + "_" + k.ToString))
                                            End If
                                        Next
                                    End If
                                End If
                            Next
                        End If

                        If resource.ID.Equals(RA_Cost_GUID) Or (resource.Enabled AndAlso (resource.ConstraintID <> -1) AndAlso (cc Is Nothing Or cc IsNot Nothing AndAlso cc.Enabled) AndAlso Settings.CustomConstraints) Then 'A1130
                            Dim minValue As Double = period.GetResourceMinValue(resource.ID)      ' D3918
                            Dim maxValue As Double = period.GetResourceMaxValue(resource.ID)      ' D3918

                            Dim tpresMainConstr As String = ""
                            If minValue <> UNDEFINED_INTEGER_VALUE And maxValue <> UNDEFINED_INTEGER_VALUE Then
                                If tpResList.ContainsKey("TPRes_Main_" + resource.ID.ToString.Replace("-", "_") + "_" + t.ToString) Then
                                    If Settings.ResourcesMax Then
                                        'Dim tpresMainConstr As String = gModel.AddConstr(tpResList("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString), GRB.LESS_EQUAL, maxValue, "TPRes" + t.ToString + "_" + resource.ID.ToString + "_Max")
                                        tpresMainConstr = tpResList("TPRes_Main_" + resource.ID.ToString.Replace("-", "_") + "_" + t.ToString) + " <= " + maxValue.ToString
                                        tpresMainConstr = "TPRes" + t.ToString + "_" + resource.ID.ToString.Replace("-", "_") + "_Max" + ": " + tpresMainConstr
                                        EquationsList.Add("TPRes" + t.ToString + "_" + resource.ID.ToString.Replace("-", "_") + "_Max")
                                    End If
                                    If Settings.ResourcesMin Then
                                        'Dim tpresMainConstr As GRBConstr = gModel.AddConstr(tpResList("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString), GRB.GREATER_EQUAL, minValue, "TPRes" + t.ToString + "_" + resource.ID.ToString + "_Min")
                                        tpresMainConstr = tpResList("TPRes_Main_" + resource.ID.ToString.Replace("-", "_") + "_" + t.ToString) + " >= " + minValue.ToString
                                        tpresMainConstr = "TPRes" + t.ToString + "_" + resource.ID.ToString.Replace("-", "_") + "_Min" + ": " + tpresMainConstr
                                        EquationsList.Add("TPRes" + t.ToString + "_" + resource.ID.ToString.Replace("-", "_") + "_Min")
                                    End If
                                End If
                            End If
                            If minValue = UNDEFINED_INTEGER_VALUE And maxValue <> UNDEFINED_INTEGER_VALUE And Settings.ResourcesMax Then
                                If tpResList.ContainsKey("TPRes_Main_" + resource.ID.ToString.Replace("-", "_") + "_" + t.ToString) Then
                                    tpresMainConstr = tpResList("TPRes_Main_" + resource.ID.ToString.Replace("-", "_") + "_" + t.ToString) + " <= " + maxValue.ToString
                                    tpresMainConstr = "TPRes" + t.ToString + "_" + resource.ID.ToString.Replace("-", "_") + "_Max" + ": " + tpresMainConstr
                                    EquationsList.Add("TPRes" + t.ToString + "_" + resource.ID.ToString.Replace("-", "_") + "_Max")
                                End If
                            End If
                            If minValue <> UNDEFINED_INTEGER_VALUE And Settings.ResourcesMin And maxValue = UNDEFINED_INTEGER_VALUE Then
                                If tpResList.ContainsKey("TPRes_Main_" + resource.ID.ToString.Replace("-", "_") + "_" + t.ToString) Then
                                    tpresMainConstr = tpResList("TPRes_Main_" + resource.ID.ToString.Replace("-", "_") + "_" + t.ToString) + " >= " + minValue.ToString
                                    tpresMainConstr = "TPRes" + t.ToString + "_" + resource.ID.ToString.Replace("-", "_") + "_Min" + ": " + tpresMainConstr
                                    EquationsList.Add("TPRes" + t.ToString + "_" + resource.ID.ToString.Replace("-", "_") + "_Min")
                                End If
                            End If
                            If tpresMainConstr <> "" Then
                                TPResources += tpresMainConstr + "; " + vbNewLine
                            End If
                        End If
                    Next
                    t += 1
                Next
                TimePeriods += TPRowVariants + TPResources
            End If


            If Settings.Dependencies And CurrentScenario.TimePeriods.Periods.Count > 1 Then
                Dim k As Integer = 1
                For Each dependency As RADependency In CurrentScenario.TimePeriodsDependencies.Dependencies
                    If dependency.Enabled And dependency.Value <> RADependencyType.dtConcurrent Then
                        Dim control1 As clsControl = Controls.GetControlByID(New Guid(dependency.FirstAlternativeID))
                        Dim control2 As clsControl = Controls.GetControlByID(New Guid(dependency.SecondAlternativeID))
                        Dim group1 As RAGroup = Nothing
                        Dim group2 As RAGroup = Nothing
                        If control1 Is Nothing Then
                            group1 = CurrentScenario.Groups.GetGroupByID(dependency.FirstAlternativeID)
                        End If
                        If control2 Is Nothing Then
                            group2 = CurrentScenario.Groups.GetGroupByID(dependency.SecondAlternativeID)
                        End If
                        Dim apd1 As AlternativePeriodsData = CurrentScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(dependency.FirstAlternativeID)
                        Dim apd2 As AlternativePeriodsData = CurrentScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(dependency.SecondAlternativeID)
                        If control1 IsNot Nothing AndAlso control1.VarID <> 0 AndAlso control2 IsNot Nothing AndAlso control2.VarID <> 0 Then
                            Dim tdepName As String = "TPDependency_" + k.ToString
                            EquationsList.Add(tdepName)
                            Dim cDep As String = ""

                            For j As Integer = apd1.GetMinPeriod To apd1.GetMaxPeriod
                                'cDep.AddTerm(j + 1, gVariables("TPVar_" + dependency.FirstAlternativeID + "_" + j.ToString))
                                Dim s As String = (j + 1).ToString + " * " + "TPVar_" + control1.VarID.ToString + "_" + j.ToString
                                cDep += If(cDep = "", s, " + " + s)
                            Next
                            For j As Integer = apd2.GetMinPeriod To apd2.GetMaxPeriod
                                'cDep.AddTerm(-(j + 1), gVariables("TPVar_" + dependency.SecondAlternativeID + "_" + j.ToString))
                                Dim s As String = "(" + (-(j + 1)).ToString + ") * " + "TPVar_" + control2.VarID.ToString + "_" + j.ToString
                                cDep += If(cDep = "", s, " + " + s)
                            Next

                            'Dim tpDepConstr As GRBConstr = Nothing
                            'Dim tpIndDepConstr As GRBGenConstr = Nothing

                            'Dim dtpVar As GRBVar = gModel.AddVar(0, 1, 0, GRB.BINARY, "dTPVar_" + k.ToString)
                            'gVariables.Add("dTPVar_" + k.ToString, dtpVar)
                            'gIndVariables.Add("dTPVar_" + k.ToString, dtpVar)


                            Select Case dependency.Value
                                Case RADependencyType.dtSuccessive
                                    ' formulation using indicator constraints
                                    'tpIndDepConstr = gModel.AddGenConstrIndicator(gVariables(dependency.FirstAlternativeID), 1, cDep, GRB.GREATER_EQUAL, apd2.Duration, "TPDependency_" + k.ToString)

                                    cDep += " - " + apd2.Duration.ToString
                                    cDep = "x_" + control1.VarID.ToString + " * (" + cDep + ")"
                                    cDep += " >= 0"

                                    ' formulation using bigM approach:
                                    'Dim bigM As Integer = 100
                                    'cDep.AddTerm(-bigM, gVariables(dependency.FirstAlternativeID))
                                    'tpDepConstr = gModel.AddConstr(cDep, GRB.GREATER_EQUAL, apd2.Duration - bigM, "TPDependency_" + k.ToString)
                                Case RADependencyType.dtConcurrent
                                    cDep += control1.VarID.ToString + " * (" + (-apd2.GetMaxPeriod).ToString + ")"
                                    cDep += " >= " + (-apd2.GetMaxPeriod).ToString
                                Case RADependencyType.dtLag
                                    Select Case dependency.LagCondition
                                        Case LagCondition.lcEqual
                                            cDep += " - " + apd2.Duration.ToString
                                            cDep = "x_" + control1.VarID.ToString + " * (" + cDep + ")"
                                            cDep += " == " + dependency.Lag.ToString
                                        Case LagCondition.lcGreaterOrEqual
                                            cDep += " - " + apd2.Duration.ToString
                                            cDep = "x_" + control1.VarID.ToString + " * (" + cDep + ")"
                                            cDep += " >= " + dependency.Lag.ToString
                                        Case LagCondition.lcLessOrEqual
                                            cDep += " - " + apd2.Duration.ToString
                                            cDep = "x_" + control1.VarID.ToString + " * (" + cDep + ")"
                                            cDep += " <= " + dependency.Lag.ToString
                                    End Select
                            End Select
                            TimePeriodsDependencies += tdepName + ": " + cDep + ";" + vbNewLine
                        Else
                            Dim dID As Integer = 1
                            If control1 IsNot Nothing AndAlso group2 IsNot Nothing Then
                                For Each alt As RAAlternative In group2.Alternatives.Values.Where(Function(x) x.Enabled)
                                    Dim control As clsControl = Controls.GetControlByID(New Guid(alt.ID))
                                    If control IsNot Nothing AndAlso control.VarID <> 0 Then
                                        Dim tdepName As String = "TPDependency_" + k.ToString + "_" + dID.ToString
                                        EquationsList.Add(tdepName)
                                        Dim cDep As String = ""

                                        For j As Integer = apd1.GetMinPeriod To apd1.GetMaxPeriod
                                            'cDep.AddTerm(j + 1, gVariables("TPVar_" + dependency.FirstAlternativeID + "_" + j.ToString))
                                            Dim s As String = (j + 1).ToString + " * " + "TPVar_" + control1.VarID.ToString + "_" + j.ToString
                                            cDep += If(cDep = "", s, " + " + s)
                                        Next
                                        For j As Integer = apd2.GetMinPeriod To apd2.GetMaxPeriod
                                            'cDep.AddTerm(-(j + 1), gVariables("TPVar_" + dependency.SecondAlternativeID + "_" + j.ToString))
                                            Dim s As String = "(" + (-(j + 1)).ToString + ") * " + "TPVar_" + control.VarID.ToString + "_" + j.ToString
                                            cDep += If(cDep = "", s, " + " + s)
                                        Next
                                        Select Case dependency.Value
                                            Case RADependencyType.dtSuccessive
                                                ' formulation using indicator constraints
                                                'tpIndDepConstr = gModel.AddGenConstrIndicator(gVariables(dependency.FirstAlternativeID), 1, cDep, GRB.GREATER_EQUAL, apd2.Duration, "TPDependency_" + k.ToString)

                                                cDep += " - " + apd2.Duration.ToString
                                                cDep = "x_" + control1.VarID.ToString + " * (" + cDep + ")"
                                                cDep += " >= 0"

                                                ' formulation using bigM approach:
                                                'Dim bigM As Integer = 100
                                                'cDep.AddTerm(-bigM, gVariables(dependency.FirstAlternativeID))
                                                'tpDepConstr = gModel.AddConstr(cDep, GRB.GREATER_EQUAL, apd2.Duration - bigM, "TPDependency_" + k.ToString)
                                            Case RADependencyType.dtConcurrent
                                                cDep += control1.VarID.ToString + " * (" + (-apd2.GetMaxPeriod).ToString + ")"
                                                cDep += " >= " + (-apd2.GetMaxPeriod).ToString
                                            Case RADependencyType.dtLag
                                                Select Case dependency.LagCondition
                                                    Case LagCondition.lcEqual
                                                        cDep += " - " + apd2.Duration.ToString
                                                        cDep = "x_" + control1.VarID.ToString + " * (" + cDep + ")"
                                                        cDep += " == " + dependency.Lag.ToString
                                                    Case LagCondition.lcGreaterOrEqual
                                                        cDep += " - " + apd2.Duration.ToString
                                                        cDep = "x_" + control1.VarID.ToString + " * (" + cDep + ")"
                                                        cDep += " >= " + dependency.Lag.ToString
                                                    Case LagCondition.lcLessOrEqual
                                                        cDep += " - " + apd2.Duration.ToString
                                                        cDep = "x_" + control1.VarID.ToString + " * (" + cDep + ")"
                                                        cDep += " <= " + dependency.Lag.ToString
                                                End Select
                                        End Select
                                        TimePeriodsDependencies += tdepName + ": " + cDep + ";" + vbNewLine
                                        dID += 1
                                    End If
                                Next
                            Else
                                If group1 IsNot Nothing AndAlso control2 IsNot Nothing Then
                                    For Each alt As RAAlternative In group1.Alternatives.Values.Where(Function(x) x.Enabled)
                                        Dim control As clsControl = Controls.GetControlByID(New Guid(alt.ID))
                                        If control IsNot Nothing AndAlso control.VarID <> 0 Then
                                            Dim tdepName As String = "TPDependency_" + k.ToString + "_" + dID.ToString
                                            EquationsList.Add(tdepName)
                                            Dim cDep As String = ""

                                            For j As Integer = apd1.GetMinPeriod To apd1.GetMaxPeriod
                                                'cDep.AddTerm(j + 1, gVariables("TPVar_" + dependency.FirstAlternativeID + "_" + j.ToString))
                                                Dim s As String = (j + 1).ToString + " * " + "TPVar_" + control.VarID.ToString + "_" + j.ToString
                                                cDep += If(cDep = "", s, " + " + s)
                                            Next
                                            For j As Integer = apd2.GetMinPeriod To apd2.GetMaxPeriod
                                                'cDep.AddTerm(-(j + 1), gVariables("TPVar_" + dependency.SecondAlternativeID + "_" + j.ToString))
                                                Dim s As String = "(" + (-(j + 1)).ToString + ") * " + "TPVar_" + control2.VarID.ToString + "_" + j.ToString
                                                cDep += If(cDep = "", s, " + " + s)
                                            Next
                                            Select Case dependency.Value
                                                Case RADependencyType.dtSuccessive
                                                    ' formulation using indicator constraints
                                                    'tpIndDepConstr = gModel.AddGenConstrIndicator(gVariables(dependency.FirstAlternativeID), 1, cDep, GRB.GREATER_EQUAL, apd2.Duration, "TPDependency_" + k.ToString)

                                                    cDep += " - " + apd2.Duration.ToString
                                                    cDep = "x_" + control.VarID.ToString + " * (" + cDep + ")"
                                                    cDep += " >= 0"

                                                ' formulation using bigM approach:
                                                'Dim bigM As Integer = 100
                                                'cDep.AddTerm(-bigM, gVariables(dependency.FirstAlternativeID))
                                                'tpDepConstr = gModel.AddConstr(cDep, GRB.GREATER_EQUAL, apd2.Duration - bigM, "TPDependency_" + k.ToString)
                                                Case RADependencyType.dtConcurrent
                                                    cDep += control.VarID.ToString + " * (" + (-apd2.GetMaxPeriod).ToString + ")"
                                                    cDep += " >= " + (-apd2.GetMaxPeriod).ToString
                                                Case RADependencyType.dtLag
                                                    Select Case dependency.LagCondition
                                                        Case LagCondition.lcEqual
                                                            cDep += " - " + apd2.Duration.ToString
                                                            cDep = "x_" + control1.VarID.ToString + " * (" + cDep + ")"
                                                            cDep += " == " + dependency.Lag.ToString
                                                        Case LagCondition.lcGreaterOrEqual
                                                            cDep += " - " + apd2.Duration.ToString
                                                            cDep = "x_" + control1.VarID.ToString + " * (" + cDep + ")"
                                                            cDep += " >= " + dependency.Lag.ToString
                                                        Case LagCondition.lcLessOrEqual
                                                            cDep += " - " + apd2.Duration.ToString
                                                            cDep = "x_" + control1.VarID.ToString + " * (" + cDep + ")"
                                                            cDep += " <= " + dependency.Lag.ToString
                                                    End Select
                                            End Select
                                            TimePeriodsDependencies += tdepName + ": " + cDep + ";" + vbNewLine
                                            dID += 1
                                        End If
                                    Next
                                End If
                            End If
                        End If
                    End If
                    k += 1
                Next
            End If

            Variables += CStr(If(Variables(Variables.Count - 1) = ";", "", ";"))

        End Sub

        Private Function GetBaronBudgetLimit(Path As String, ResName As String, EventIDs As List(Of Guid), Optional IsForEfficientFrontier As Boolean = False) As String
            Dim Options, Variables, Budget, Musts, MustNots, Groups, Dependencies, TimePeriods, TimePeriodsDependencies, Risk As String
            Dim equationsList As New List(Of String)
            GetBARONStrings(Path, ResName, EventIDs, Options, Variables, Budget, Musts, MustNots, Groups, Dependencies, TimePeriods, TimePeriodsDependencies, equationsList, Risk, IsForEfficientFrontier)

            Dim res As String = ""
            res += Options
            res += Variables

            Budget += " <= " + If(IsForEfficientFrontier, "%%paramBudget%%", JS_SafeNumber(BudgetLimit)) + ";" + vbNewLine

            Dim mustsCount As Integer = If(Settings.Musts, Controls.EnabledControls.Where(Function(x) x.Must AndAlso x.Enabled AndAlso x.VarID <> 0).Count, 0)
            Dim mustnotsCount As Integer = If(Settings.MustNots, Controls.EnabledControls.Where(Function(x) x.MustNot AndAlso x.Enabled AndAlso x.VarID <> 0).Count, 0)
            Dim mustEq As String = If(Settings.Musts, CStr((If(mustsCount > 0, ", musts", ""))), "")
            Dim mustnotsEq As String = If(Settings.MustNots, CStr((If(mustnotsCount > 0, ", mustnots", ""))), "")

            Dim includeGroups As Boolean = False
            If CurrentScenario.Settings.Groups And CurrentScenario.GetAvailableGroups.Count > 0 Then
                includeGroups = CurrentScenario.GetAvailableGroups.Where(Function(g) g.Enabled).Count > 0
            End If

            Dim groupsEq As String = ""
            If includeGroups Then
                Dim gIntID As Integer = 0
                For Each group As RAGroup In CurrentScenario.GetAvailableGroups.Where(Function(g) g.Enabled)
                    If group.Enabled AndAlso group.Alternatives.Count > 0 Then
                        groupsEq += CStr(If(groupsEq = "", ", Group_" + gIntID.ToString, ", " + "Group_" + gIntID.ToString))
                        gIntID += 1
                    End If
                Next
            End If

            Dim includeDependencies As Boolean = False
            If CurrentScenario.Settings.Dependencies And CurrentScenario.GetAvailableDependencies.Count > 0 Then
                includeDependencies = CurrentScenario.GetAvailableDependencies.Where(Function(d) d.Enabled).Count > 0
            End If

            Dim dependenciesEq As String = ""
            If includeDependencies Then
                Dim dIntID As Integer = 0
                For Each dependency As RADependency In CurrentScenario.GetAvailableDependencies.Where(Function(d) d.Enabled)
                    dependenciesEq += CStr(If(dependenciesEq = "", ", Dependency_" + dIntID.ToString, ", " + "Dependency_" + dIntID.ToString))
                    dIntID += 1
                Next
            End If

            Dim TPEquations As String = ""
            For Each equation As String In equationsList
                TPEquations += ", " + equation
            Next

            'res += vbNewLine + vbNewLine + "EQUATIONS budget" + mustEq + mustnotsEq + groupsEq + dependenciesEq + TPEquations + If(IsForEfficientFrontier, "", ", KeepFunded") + " ;" + vbNewLine
            res += vbNewLine + vbNewLine + "EQUATIONS budget" + mustEq + mustnotsEq + groupsEq + dependenciesEq + TPEquations + ", KeepFunded" + " ;" + vbNewLine

            res += "budget: " + Budget + vbNewLine
            If Settings.Musts And mustsCount > 0 Then res += "musts: " + Musts + vbNewLine
            If Settings.MustNots And mustnotsCount > 0 Then res += "mustnots: " + MustNots + vbNewLine

            If includeGroups Then res += Groups
            If includeDependencies Then res += Dependencies

            res += TimePeriods
            res += TimePeriodsDependencies

            If IsForEfficientFrontier Then
                res += "%%paramKeepFunded%%"
            End If

            'res += "OBJ: minimize" + vbNewLine
            res += "OBJ: " + If(ProjectManager.PipeParameters.ProjectType = Canvas.CanvasTypes.ProjectType.ptMixed OrElse ProjectManager.PipeParameters.ProjectType = Canvas.CanvasTypes.ProjectType.ptMyRiskReward, "maximize", "minimize") + vbNewLine    ' D6798
            If Risk <> "" Then
                'Risk = "1000 * (" + Risk + ")"
            End If
            Risk += If(Risk = "", "0;", ";")
            res += Risk + vbNewLine

            Return res
        End Function

        Private Function GetBaronMaxRisk(Path As String, ResName As String, EventIDs As List(Of Guid), Optional IsForEfficientFrontier As Boolean = False) As String
            Dim MaxRiskValue As Double = 0
            Select Case CurrentScenario.OptimizationType
                Case RiskOptimizationType.MaxRisk
                    MaxRiskValue = CurrentScenario.MaxRisk
                Case RiskOptimizationType.MinReduction
                    MaxRiskValue = CurrentScenario.OriginalRiskValue * (1 - CurrentScenario.MinReduction)
            End Select

            Dim Options, Variables, Budget, Musts, MustNots, Groups, Dependencies, TimePeriods, TimePeriodsDependencies, Risk As String
            Dim equationsList As New List(Of String)
            GetBARONStrings(Path, ResName, EventIDs, Options, Variables, Budget, Musts, MustNots, Groups, Dependencies, TimePeriods, TimePeriodsDependencies, equationsList, Risk, IsForEfficientFrontier)

            Dim res As String = ""
            res += Options
            res += Variables

            Dim mustsCount As Integer = If(Settings.Musts, Controls.EnabledControls.Where(Function(x) x.Must AndAlso x.Enabled AndAlso x.VarID <> 0).Count, 0)
            Dim mustnotsCount As Integer = If(Settings.MustNots, Controls.EnabledControls.Where(Function(x) x.MustNot AndAlso x.Enabled AndAlso x.VarID <> 0).Count, 0)
            Dim mustEq As String = If(Settings.Musts, CStr((If(mustsCount > 0, ", musts", ""))), "")
            Dim mustnotsEq As String = If(Settings.MustNots, CStr((If(mustnotsCount > 0, ", mustnots", ""))), "")

            Dim includeGroups As Boolean = False
            If CurrentScenario.Settings.Groups And CurrentScenario.GetAvailableGroups.Count > 0 Then
                includeGroups = CurrentScenario.GetAvailableGroups.Where(Function(g) g.Enabled).Count > 0
            End If

            Dim groupsEq As String = ""
            If includeGroups Then
                Dim gIntID As Integer = 0
                For Each group As RAGroup In CurrentScenario.GetAvailableGroups.Where(Function(g) g.Enabled)
                    If group.Enabled AndAlso group.Alternatives.Count > 0 Then
                        groupsEq += CStr(If(groupsEq = "", ", Group_" + gIntID.ToString, ", " + "Group_" + gIntID.ToString))
                        gIntID += 1
                    End If
                Next
            End If

            Dim includeDependencies As Boolean = False
            If CurrentScenario.Settings.Dependencies And CurrentScenario.GetAvailableDependencies.Count > 0 Then
                includeDependencies = CurrentScenario.GetAvailableDependencies.Where(Function(d) d.Enabled).Count > 0
            End If

            Dim dependenciesEq As String = ""
            If includeDependencies Then
                Dim dIntID As Integer = 0
                For Each dependency As RADependency In CurrentScenario.GetAvailableDependencies.Where(Function(d) d.Enabled)
                    dependenciesEq += CStr(If(dependenciesEq = "", ", Dependency_" + dIntID.ToString, ", " + "Dependency_" + dIntID.ToString))
                    dIntID += 1
                Next
            End If

            Dim TPEquations As String = ""
            For Each equation As String In equationsList
                TPEquations += ", " + equation
            Next

            res += vbNewLine + vbNewLine + "EQUATIONS MaxRisk" + mustEq + mustnotsEq + groupsEq + dependenciesEq + TPEquations + " ;" + vbNewLine

            res += "MaxRisk: " + Risk + " <= " + JS_SafeNumber(MaxRiskValue) + ";" + vbNewLine
            If Settings.Musts And mustsCount > 0 Then res += "musts: " + Musts + vbNewLine
            If Settings.MustNots And mustnotsCount > 0 Then res += "mustnots: " + MustNots + vbNewLine

            If includeGroups Then res += Groups
            If includeDependencies Then res += Dependencies

            res += TimePeriods
            res += TimePeriodsDependencies

            If IsForEfficientFrontier Then
                res += "%%paramKeepFunded%%"
            End If

            res += "OBJ: minimize" + vbNewLine
            Budget += ";"
            res += Budget + vbNewLine

            Return res
        End Function

        Public Function GetBaron(Path As String, ResName As String, EventIDs As List(Of Guid), Optional IsForEfficientFrontier As Boolean = False) As String
            Select Case CurrentScenario.OptimizationType
                Case RiskOptimizationType.BudgetLimit
                    Return GetBaronBudgetLimit(Path, ResName, EventIDs, IsForEfficientFrontier)
                Case RiskOptimizationType.MaxRisk, RiskOptimizationType.MinReduction
                    Return GetBaronMaxRisk(Path, ResName, EventIDs, IsForEfficientFrontier)
            End Select
            Return "Unknown optimization type"
        End Function

        Public Function ParseBaron(ResText As String, Output As String, EventIDs As List(Of Guid), ByRef FundedControls As List(Of Guid), ByRef TotalCost As Double, ByRef SolverState As raSolverState) As Double
            Dim startStr As String = "The best solution found is:" + vbNewLine + vbNewLine
            Dim valueStr As String = "The above solution has an objective value of:"

            Dim result As Double = -1

            If ResText.Contains("Problem is infeasible") Then
                SolverState = raSolverState.raInfeasible
            Else
                SolverState = raSolverState.raSolved
            End If

            CurrentScenario.TimePeriods.TimePeriodResults.Clear()

            Dim i As Integer = ResText.IndexOf(startStr)
            If i > 0 Then
                Dim sMatrix As String = ResText.Substring(i + startStr.Length)
                i = sMatrix.IndexOf(vbNewLine + vbNewLine)
                If i > 0 Then
                    sMatrix = sMatrix.Substring(0, i)
                    Dim rows As String() = sMatrix.Split(CChar(vbCr))

                    FundedControls = New List(Of Guid)
                    TotalCost = 0
                    For j As Integer = 1 To rows.Length - 1
                        rows(j) = rows(j).TrimEnd(CChar(vbLf)).Trim.Replace(vbTab + vbTab, vbTab).Replace(vbTab + vbTab, vbTab)
                        Dim cells As String() = rows(j).Split(CChar(vbTab))

                        Dim xName As String = cells(0)
                        Dim xValue As Integer 'A1251 ===
                        Dim dValue As Double = 0
                        If String2Double(cells(2), dValue) Then
                            xValue = CInt(dValue)
                        End If
                        'A1251 ==
                        If xValue = 1 Then
                            Select Case xName(0)
                                Case CChar("x")
                                    Dim control As clsControl = Controls.GetControlByVarID(CInt(xName.Substring(2)))
                                    FundedControls.Add(control.ID)
                                    TotalCost += CDbl(If(Not control.Enabled OrElse Not control.IsCostDefined, 0, control.Cost)) 'A1381
                                Case CChar("T")
                                    Dim e1 As Integer = xName.IndexOf("_")
                                    Dim e2 As Integer = xName.IndexOf("_", e1 + 1)
                                    Dim controlID As String = xName.Substring(e1 + 1, e2 - e1 - 1)
                                    Dim periodID As String = xName.Substring(e2 + 1)

                                    Dim control As clsControl = Controls.GetControlByVarID(CInt(controlID))
                                    If Settings.TimePeriods And CurrentScenario.TimePeriods.Periods.Count > 1 Then
                                        Dim apd As AlternativePeriodsData = CurrentScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(control.ID.ToString)
                                        Dim fundedPeriod As Integer
                                        If Integer.TryParse(periodID, fundedPeriod) Then
                                            CurrentScenario.TimePeriods.TimePeriodResults.Add(control.ID.ToString, fundedPeriod)
                                        End If
                                    End If
                            End Select
                        End If
                    Next
                End If
            End If

            i = ResText.IndexOf(valueStr)
            If i > 0 Then
                Dim value As String = ResText.Substring(i + valueStr.Length)
                i = value.IndexOf(vbNewLine)
                If i > 0 Then
                    value = value.Substring(0, i)
                End If
                String2Double(value, result)
            End If

            'A1356 === - make only funded controls active
            For Each ctrl As clsControl In ProjectManager.Controls.EnabledControls '-A1383 Controls.Controls + A1392
                Dim newIsActive As Boolean = False
                If FundedControls IsNot Nothing Then newIsActive = FundedControls.Contains(ctrl.ID)
                'ProjectManager.Controls.SetControlActive(ctrl.ID, newIsActive)
                ctrl.Active = newIsActive
            Next
            'A1356 ==

            Dim UseSimulatedValues As Boolean = ProjectManager.Parameters.Riskion_Use_Simulated_Values = SimulatedValuesUsageMode.SimulatedOutput OrElse ProjectManager.Parameters.Riskion_Use_Simulated_Values = SimulatedValuesUsageMode.SimulatedInputAndOutput
            If UseSimulatedValues Or CurrentScenario.OptimizationType = RiskOptimizationType.MaxRisk Or CurrentScenario.OptimizationType = RiskOptimizationType.MinReduction Then
                Dim oldGenerateValue As Boolean = ProjectManager.RiskSimulations.GenerateChartValues
                Dim oldStoreStepValues As Boolean = ProjectManager.RiskSimulations.StoreStepValues
                result = GetRisk(ControlsUsageMode.UseOnlyActive, True, True)
                ProjectManager.RiskSimulations.GenerateChartValues = oldGenerateValue
                ProjectManager.RiskSimulations.StoreStepValues = oldStoreStepValues
            End If

            Return result
        End Function
        Public Function Optimize(EventIDs As List(Of Guid), WRTNodeID As Guid, ByRef FundedControls As List(Of Guid), ByRef TotalCost As Double, Optional OutputPath As String = "", Optional BaronModel As String = "", Optional ByRef SolverState As raSolverState = raSolverState.raSolved) As Double 'A1229
            If SolverLibrary = raSolverLibrary.raBaron Then
                If ProjectManager IsNot Nothing AndAlso ProjectManager.BaronSolverCallback2 IsNot Nothing Then   ' D4071
                    If Not WRTNodeID.Equals(Guid.Empty) Then
                        WRTNode = ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(WRTNodeID)
                        If WRTNode Is Nothing Then
                            WRTNode = ProjectManager.Hierarchy(ECHierarchyID.hidImpact).GetNodeByID(WRTNodeID)
                        End If
                    End If
                    Return ProjectManager.BaronSolverCallback2(Me, EventIDs, FundedControls, TotalCost, BaronModel, SolverState)   ' D4071
                Else
                    Return -1
                End If
            End If
        End Function

        ''' <summary>
        ''' Randomly assign controls applications and generate controls effectivenesses
        ''' </summary>
        Public Sub RandomizeControls() 'A1360
            'TODO: AC        
        End Sub

        Public Sub Solve_Baron_EfficientFrontier(ByRef Settings As EfficientFrontierSettings, EventIDs As List(Of Guid), WRTNodeID As Guid, Optional IsPointByPoint As Boolean = False)
            If ProjectManager Is Nothing Then Exit Sub

            Dim tOldOptimizationType As RiskOptimizationType = ResourceAligner.RiskOptimizer.CurrentScenario.OptimizationType
            ResourceAligner.RiskOptimizer.CurrentScenario.OptimizationType = RiskOptimizationType.BudgetLimit

            Dim tOldConsequenceSimulationsMode As ConsequencesSimulationsMode = ProjectManager.CalculationsManager.ConsequenceSimulationsMode
            If ProjectManager.CalculationsManager.ConsequenceSimulationsMode <> ConsequencesSimulationsMode.Diluted Then 
                ProjectManager.CalculationsManager.ConsequenceSimulationsMode = ConsequencesSimulationsMode.Diluted
            End If

            Dim riskWithoutControls As Double = GetRisk(ControlsUsageMode.DoNotUse)
            Dim valueOfEnterpriseDefined As Boolean = ProjectManager.DollarValueOfEnterprise <> UNDEFINED_INTEGER_VALUE
            Dim riskMonetaryValueWithoutControls As Double = If(valueOfEnterpriseDefined, riskWithoutControls * ProjectManager.DollarValueOfEnterprise, 0)

            PrintDebugInfo("Solve BARON Efficient Frontier started: ")

            Dim bModel As String = GetBaron("", "", EventIDs, True)

            Dim fundedControls As New List(Of Guid)
            Dim totalCost As Double
            Dim totalBenefits As Double

            ProjectManager.RiskSimulations.RedLineValue = Settings.RedLineValue 'A1620
            ProjectManager.RiskSimulations.GreenLineValue = Settings.GreenLineValue 'A1620
            Dim tOldGenerateChartValues As Boolean = ProjectManager.RiskSimulations.GenerateChartValues
            ProjectManager.RiskSimulations.GenerateChartValues = Settings.CalculateLECValues 'A1620

            IsCancelled = False

            Dim isIncreasing As Boolean = Settings.IsIncreasing
            Dim scenarioID As Integer = Settings.ScenarioID
            Dim sw As New Stopwatch
            sw.Start()

            timer = New Stopwatch

            Dim sumOfMusts As Double = 0
            If CurrentScenario.Settings.Musts Then
                For Each control As clsControl In Controls.EnabledControls '-A1392 DefinedControls '-A1383 Controls.Controls
                    If control.Must AndAlso control.VarID <> 0 AndAlso ControlHasApplications(control, WRTNode, EventIDs) Then
                        sumOfMusts += control.Cost
                    End If
                Next
            End If

            For Each interval As EfficientFrontierInterval In Settings.Intervals
                If interval.DeltaType = EfficientFrontierDeltaType.AllSolutions Then Settings.IsIncreasing = False
                Select Case interval.DeltaType
                    Case EfficientFrontierDeltaType.NumberOfSteps, EfficientFrontierDeltaType.DeltaValue, EfficientFrontierDeltaType.AllSolutions
                        Dim stepValue As Double = If(interval.DeltaType = EfficientFrontierDeltaType.DeltaValue, interval.DeltaValue, (interval.MaxValue - interval.MinValue) / interval.DeltaValue)
                        Dim i As Double = If((interval.DeltaType = EfficientFrontierDeltaType.DeltaValue Or interval.DeltaType = EfficientFrontierDeltaType.NumberOfSteps) And Settings.IsIncreasing, If(interval.MinValue < sumOfMusts, sumOfMusts, interval.MinValue), interval.MaxValue)
                        'While If(interval.DeltaType = EfficientFrontierDeltaType.DeltaValueIncreasing, i <= interval.MaxValue AndAlso (interval.Results.Count = 0 OrElse ((interval.Results.Count < 2 OrElse interval.Results(interval.Results.Count - 1).FundedBenefits <> interval.Results(interval.Results.Count - 2).FundedBenefits)), i >= interval.MinValue AndAlso (interval.Results.Count = 0 OrElse interval.Results(interval.Results.Count - 1).SolverState = raSolverState.raSolved))
                        Dim prevBenefit As Double = -1
                        Dim KeepFunded As Boolean = Settings.KeepFundedAlts
                        Dim keepFundedStr As String = ""


                        timer.Start()
                        While If((interval.DeltaType = EfficientFrontierDeltaType.DeltaValue Or interval.DeltaType = EfficientFrontierDeltaType.NumberOfSteps) And Settings.IsIncreasing, i <= interval.MaxValue, i >= interval.MinValue AndAlso (interval.Results.Count = 0 OrElse interval.Results(interval.Results.Count - 1).SolverState = raSolverState.raSolved))
                            If IsCancelled Then Exit For

                            PrintDebugInfo("i = " + i.ToString)
                            Dim stepBaron As String = bModel.Replace("%%paramBudget%%", i.ToString)
                            stepBaron = stepBaron.Replace("%%paramKeepFunded%%", If(KeepFunded, keepFundedStr, ""))
                            totalBenefits = Optimize(EventIDs, WRTNodeID, fundedControls, totalCost, "", stepBaron)
                            PrintDebugInfo("Baron model solved: ")

                            Dim stepResults As EfficientFrontierResults = New EfficientFrontierResults
                            stepResults.SolveToken = Settings.SolveToken
                            stepResults.SolverState = If(totalBenefits = -1, raSolverState.raInfeasible, raSolverState.raSolved)

                            For Each id As Guid In fundedControls
                                stepResults.AlternativesData.Add(id.ToString, 1)
                            Next

                            'If stepResults.SolverState = raSolverState.raSolved AndAlso (ProjectManager.Parameters.Riskion_Use_Simulated_Values = SimulatedValuesUsageMode.SimulatedOutput OrElse ProjectManager.Parameters.Riskion_Use_Simulated_Values = SimulatedValuesUsageMode.SimulatedInputAndOutput) Then
                            If stepResults.SolverState = raSolverState.raSolved Then
                                stepResults.FundedBenefits = GetRisk(ControlsUsageMode.UseOnlyActive, Settings.CalculateLECValues)
                                stepResults.FundedBenefitsOriginal = stepResults.FundedBenefits
                            End If
                            'stepResults.FundedBenefits = totalBenefits
                            'stepResults.FundedBenefitsOriginal = totalBenefits
                            stepResults.FundedCost = totalCost
                            stepResults.Value = i
                            If stepResults.SolverState = raSolverState.raSolved And valueOfEnterpriseDefined Then
                                stepResults.RiskReductionGlobal = riskWithoutControls - stepResults.FundedBenefitsOriginal
                                stepResults.RiskReductionMonetaryGlobal = stepResults.RiskReductionGlobal * ProjectManager.DollarValueOfEnterprise
                                stepResults.LeverageGlobal = If(stepResults.FundedCost = 0, 0, stepResults.RiskReductionMonetaryGlobal / stepResults.FundedCost)
                                stepResults.ExpectedSavings = stepResults.RiskReductionMonetaryGlobal - stepResults.FundedCost 'A1611
                                stepResults.GreenLineIntersection = ProjectManager.RiskSimulations.GreenLineIntersection
                                stepResults.RedLineIntersection = ProjectManager.RiskSimulations.RedLineIntersection
                            End If
                            stepResults.ScenarioID = Settings.ScenarioID
                            stepResults.ScenarioIndex = Settings.ScenarioIndex 'A1708


                            'Dim includeStep As Boolean = Math.Abs(totalBenefits - prevBenefit) > 0.000000001
                            Debug.Print(totalBenefits.ToString)
                            Dim includeStep As Boolean = False
                            If interval.Results.Count > 0 Then
                                If interval.Results(interval.Results.Count - 1).AlternativesData.Count = fundedControls.Count Then
                                    For Each id As Guid In fundedControls
                                        If Not interval.Results(interval.Results.Count - 1).AlternativesData.Keys.Contains(id.ToString) Then
                                            includeStep = True
                                            Exit For
                                        End If
                                    Next
                                Else
                                    If Settings.IsIncreasing Then
                                        If totalBenefits = -1 OrElse (interval.Results(interval.Results.Count - 1).FundedBenefits <> -1 AndAlso interval.Results(interval.Results.Count - 1).FundedBenefits < stepResults.FundedBenefits) Then
                                            includeStep = False
                                        Else
                                            includeStep = True
                                        End If
                                    Else
                                        If interval.Results(interval.Results.Count - 1).FundedBenefits > stepResults.FundedBenefits Then
                                            includeStep = False
                                        Else
                                            includeStep = True
                                        End If
                                    End If
                                End If
                            Else
                                includeStep = True
                            End If
                            If includeStep Then
                                interval.Results.Add(stepResults)
                                If KeepFunded AndAlso fundedControls.Count > 0 Then
                                    keepFundedStr = ""
                                    For Each id As Guid In fundedControls
                                        Dim c As clsControl = Controls.GetControlByID(id)
                                        If c IsNot Nothing Then
                                            keepFundedStr += If(keepFundedStr = "", "x_" + c.VarID.ToString, " + x_" + c.VarID.ToString)
                                        End If
                                    Next
                                    keepFundedStr = vbNewLine + "KeepFunded: " + keepFundedStr + " == " + fundedControls.Count.ToString + ";" + vbNewLine + vbNewLine
                                End If
                            End If
                            prevBenefit = stepResults.FundedBenefits
                            LastError = ""
                            LastErrorReal = ""

                            If interval.DeltaType = EfficientFrontierDeltaType.AllSolutions Then
                                i = stepResults.FundedCost - interval.DeltaValue
                            Else
                                Dim prevStepBudget As Double = i ' A1611
                                If Settings.IsIncreasing Then
                                    i += stepValue
                                    If prevStepBudget < interval.MaxValue And i > interval.MaxValue Then i = interval.MaxValue 'A1611
                                Else
                                    i -= stepValue
                                    If prevStepBudget > interval.MinValue And i < interval.MinValue Then i = interval.MinValue 'A1611
                                End If
                            End If

                            If IsPointByPoint AndAlso includeStep Then
                                Dim j As Integer = interval.Results.Count - 1
                                If Settings.IsIncreasing And j > 0 Then
                                    interval.Results(j).DeltaValue = interval.Results(j - 1).FundedBenefitsOriginal - interval.Results(j).FundedBenefitsOriginal
                                    If valueOfEnterpriseDefined Then
                                        interval.Results(j).DeltaMonetaryValue = interval.Results(j).DeltaValue * ProjectManager.DollarValueOfEnterprise
                                        interval.Results(j).DeltaCost = interval.Results(j).FundedCost - interval.Results(j - 1).FundedCost
                                        interval.Results(j).DeltaLeverage = If(interval.Results(j).DeltaCost = 0, 0, interval.Results(j).DeltaMonetaryValue / interval.Results(j).DeltaCost)
                                        interval.Results(j).DeltaExpectedSavings = interval.Results(j).ExpectedSavings - interval.Results(j - 1).ExpectedSavings 'A1611
                                    End If
                                End If
                                EfficientFrontierCallback(stepResults, -1, IsCancelled)
                            End If
                            If timer.ElapsedMilliseconds >= updateIntervalMilliseconds Then
                                EfficientFrontierIsCancelledCallback(IsCancelled)
                                timer.Restart()
                            End If
                        End While
                    Case EfficientFrontierDeltaType.MinBenefitIncrease
                        EFValues.Clear()
                        EFFundedCostValues.Clear()
                        CalculateInterval(Settings, interval, IsPointByPoint, riskWithoutControls, valueOfEnterpriseDefined, bModel, interval.MinValue, interval.MaxValue, EventIDs, WRTNodeID, ProjectManager.DollarValueOfEnterprise / interval.DeltaValue)
                End Select
                timer.Stop()
                interval.Results = interval.Results.OrderBy(Function(a) a.Value).ToList
                For j As Integer = 1 To interval.Results.Count - 1
                    Debug.Print(interval.Results(j).Value.ToString + " = " + interval.Results(j).FundedBenefits.ToString)
                    interval.Results(j).DeltaValue = interval.Results(j - 1).FundedBenefitsOriginal - interval.Results(j).FundedBenefitsOriginal
                    If valueOfEnterpriseDefined Then
                        interval.Results(j).DeltaMonetaryValue = interval.Results(j).DeltaValue * ProjectManager.DollarValueOfEnterprise
                        interval.Results(j).DeltaCost = interval.Results(j).FundedCost - interval.Results(j - 1).FundedCost
                        interval.Results(j).DeltaLeverage = If(interval.Results(j).DeltaCost = 0, 0, interval.Results(j).DeltaMonetaryValue / interval.Results(j).DeltaCost)
                        interval.Results(j).DeltaExpectedSavings = interval.Results(j).ExpectedSavings - interval.Results(j - 1).ExpectedSavings 'A1611
                    End If
                Next
            Next

            Dim t As Long = sw.ElapsedMilliseconds

            PrintDebugInfo("Elapsed milliseconds: " + t.ToString)

            If IsPointByPoint Then
                EfficientFrontierCallback(Nothing, -1, IsCancelled)
            End If

            PrintDebugInfo("BARON efficient frontier solve completed: ")

            ProjectManager.RiskSimulations.GenerateChartValues = tOldGenerateChartValues 'A1620
            ResourceAligner.RiskOptimizer.CurrentScenario.OptimizationType = tOldOptimizationType
            If ProjectManager.CalculationsManager.ConsequenceSimulationsMode <> tOldConsequenceSimulationsMode Then
                ProjectManager.CalculationsManager.ConsequenceSimulationsMode = tOldConsequenceSimulationsMode
            End If
        End Sub

        Private Sub CalculateInterval(ByRef Settings As EfficientFrontierSettings, interval As EfficientFrontierInterval, IsPointByPoint As Boolean, riskWithoutControls As Double, valueOfEnterpriseDefined As Boolean, bModel As String, Xmin As Double, Xmax As Double, EventIDs As List(Of Guid), WRTNodeID As Guid, Eps As Double)
            Dim maxXstep As Double = (interval.MaxValue - interval.MinValue) / 50
            If Math.Abs(Xmin - Xmax) < maxXstep Then Exit Sub

            Debug.Print("Xmin = " + Xmin.ToString + "; Xmax = " + Xmax.ToString)
            Dim stepBaron As String
            Dim fundedControlsMin As New List(Of Guid)
            Dim fundedControlsMax As New List(Of Guid)
            Dim totalCostMin As Double
            Dim totalCostMax As Double
            Dim totalBenefitsMin As Double
            Dim totalBenefitsMax As Double

            Dim KeepFunded As Boolean = Settings.KeepFundedAlts
            Dim keepFundedStr As String = ""

            If Not EFValues.ContainsKey(Xmin) Then
                stepBaron = bModel.Replace("%%paramBudget%%", Xmin.ToString)
                stepBaron = stepBaron.Replace("%%paramKeepFunded%%", If(KeepFunded, keepFundedStr, ""))
                totalBenefitsMin = Optimize(EventIDs, WRTNodeID, fundedControlsMin, totalCostMin, "", stepBaron)
                EFValues.Add(Xmin, totalBenefitsMin)

                ' ADD RESULTS
                Dim stepResults As EfficientFrontierResults = New EfficientFrontierResults
                stepResults.SolveToken = Settings.SolveToken
                stepResults.FundedBenefits = totalBenefitsMin
                stepResults.FundedBenefitsOriginal = totalBenefitsMin
                stepResults.FundedCost = totalCostMin
                stepResults.Value = Xmin
                stepResults.SolverState = If(totalBenefitsMin = -1, raSolverState.raInfeasible, raSolverState.raSolved)
                If stepResults.SolverState = raSolverState.raSolved And valueOfEnterpriseDefined Then
                    stepResults.RiskReductionGlobal = riskWithoutControls - stepResults.FundedBenefitsOriginal
                    stepResults.RiskReductionMonetaryGlobal = stepResults.RiskReductionGlobal * ProjectManager.DollarValueOfEnterprise
                    stepResults.LeverageGlobal = If(stepResults.FundedCost = 0, 0, stepResults.RiskReductionMonetaryGlobal / stepResults.FundedCost)
                    stepResults.ExpectedSavings = stepResults.RiskReductionMonetaryGlobal - stepResults.FundedCost 'A1611
                    stepResults.GreenLineIntersection = ProjectManager.RiskSimulations.GreenLineIntersection
                    stepResults.RedLineIntersection = ProjectManager.RiskSimulations.RedLineIntersection
                End If
                stepResults.ScenarioID = Settings.ScenarioID
                stepResults.ScenarioIndex = Settings.ScenarioIndex 'A1708
                For Each id As Guid In fundedControlsMin
                    stepResults.AlternativesData.Add(id.ToString, 1)
                Next

                If stepResults.SolverState = raSolverState.raSolved AndAlso (ProjectManager.Parameters.Riskion_Use_Simulated_Values = SimulatedValuesUsageMode.SimulatedOutput OrElse ProjectManager.Parameters.Riskion_Use_Simulated_Values = SimulatedValuesUsageMode.SimulatedInputAndOutput) Then
                    stepResults.FundedBenefits = GetRisk(ControlsUsageMode.UseOnlyActive)
                    stepResults.FundedBenefitsOriginal = stepResults.FundedBenefits
                End If
                Debug.Print(" - " + stepResults.Value.ToString + " : " + stepResults.FundedBenefits.ToString)

                If Not EFFundedCostValues.Contains(stepResults.FundedCost) Then
                    EFFundedCostValues.Add(stepResults.FundedCost)
                    interval.Results.Add(stepResults)
                End If

                interval.Results = interval.Results.OrderBy(Function(a) a.Value).ToList
                For j As Integer = 1 To interval.Results.Count - 1
                    Debug.Print(interval.Results(j).Value.ToString + " = " + interval.Results(j).FundedBenefits.ToString)
                    interval.Results(j).DeltaValue = interval.Results(j - 1).FundedBenefitsOriginal - interval.Results(j).FundedBenefitsOriginal
                    If valueOfEnterpriseDefined Then
                        interval.Results(j).DeltaMonetaryValue = interval.Results(j).DeltaValue * ProjectManager.DollarValueOfEnterprise
                        interval.Results(j).DeltaCost = interval.Results(j).FundedCost - interval.Results(j - 1).FundedCost
                        interval.Results(j).DeltaLeverage = If(interval.Results(j).DeltaCost = 0, 0, interval.Results(j).DeltaMonetaryValue / interval.Results(j).DeltaCost)
                        interval.Results(j).DeltaExpectedSavings = interval.Results(j).ExpectedSavings - interval.Results(j - 1).ExpectedSavings 'A1611
                    End If
                Next

                If IsPointByPoint Then
                    EfficientFrontierCallback(stepResults, -1, IsCancelled)

                    If timer.ElapsedMilliseconds >= updateIntervalMilliseconds Then
                        EfficientFrontierIsCancelledCallback(IsCancelled)
                        timer.Restart()
                    End If
                End If
            Else
                totalBenefitsMin = EFValues(Xmin)
            End If


            If Not EFValues.ContainsKey(Xmax) Then
                stepBaron = bModel.Replace("%%paramBudget%%", Xmax.ToString)
                stepBaron = stepBaron.Replace("%%paramKeepFunded%%", If(KeepFunded, keepFundedStr, ""))
                totalBenefitsMax = Optimize(EventIDs, WRTNodeID, fundedControlsMax, totalCostMax, "", stepBaron)
                EFValues.Add(Xmax, totalBenefitsMax)
                ' ADD RESULTS

                Dim stepResults As EfficientFrontierResults = New EfficientFrontierResults
                stepResults.SolveToken = Settings.SolveToken
                stepResults.FundedBenefits = totalBenefitsMax
                stepResults.FundedBenefitsOriginal = totalBenefitsMax
                stepResults.FundedCost = totalCostMax
                stepResults.Value = Xmax
                stepResults.SolverState = If(totalBenefitsMax = -1, raSolverState.raInfeasible, raSolverState.raSolved)
                If stepResults.SolverState = raSolverState.raSolved And valueOfEnterpriseDefined Then
                    stepResults.RiskReductionGlobal = riskWithoutControls - stepResults.FundedBenefitsOriginal
                    stepResults.RiskReductionMonetaryGlobal = stepResults.RiskReductionGlobal * ProjectManager.DollarValueOfEnterprise
                    stepResults.LeverageGlobal = If(stepResults.FundedCost = 0, 0, stepResults.RiskReductionMonetaryGlobal / stepResults.FundedCost)
                    stepResults.ExpectedSavings = stepResults.RiskReductionMonetaryGlobal - stepResults.FundedCost 'A1611
                    stepResults.GreenLineIntersection = ProjectManager.RiskSimulations.GreenLineIntersection
                    stepResults.RedLineIntersection = ProjectManager.RiskSimulations.RedLineIntersection
                End If
                stepResults.ScenarioID = Settings.ScenarioID
                stepResults.ScenarioIndex = Settings.ScenarioIndex 'A1708
                For Each id As Guid In fundedControlsMax
                    stepResults.AlternativesData.Add(id.ToString, 1)
                Next

                If stepResults.SolverState = raSolverState.raSolved AndAlso (ProjectManager.Parameters.Riskion_Use_Simulated_Values = SimulatedValuesUsageMode.SimulatedOutput OrElse ProjectManager.Parameters.Riskion_Use_Simulated_Values = SimulatedValuesUsageMode.SimulatedInputAndOutput) Then
                    stepResults.FundedBenefits = GetRisk(ControlsUsageMode.UseOnlyActive)
                    stepResults.FundedBenefitsOriginal = stepResults.FundedBenefits
                End If
                Debug.Print(" - " + stepResults.Value.ToString + " : " + stepResults.FundedBenefits.ToString)
                If EFValues(Xmin) <> totalBenefitsMax Then
                    If Not EFFundedCostValues.Contains(stepResults.FundedCost) Then
                        EFFundedCostValues.Add(stepResults.FundedCost)
                        interval.Results.Add(stepResults)
                    End If
                End If

                interval.Results = interval.Results.OrderBy(Function(a) a.Value).ToList
                For j As Integer = 1 To interval.Results.Count - 1
                    Debug.Print(interval.Results(j).Value.ToString + " = " + interval.Results(j).FundedBenefits.ToString)
                    interval.Results(j).DeltaValue = interval.Results(j - 1).FundedBenefitsOriginal - interval.Results(j).FundedBenefitsOriginal
                    If valueOfEnterpriseDefined Then
                        interval.Results(j).DeltaMonetaryValue = interval.Results(j).DeltaValue * ProjectManager.DollarValueOfEnterprise
                        interval.Results(j).DeltaCost = interval.Results(j).FundedCost - interval.Results(j - 1).FundedCost
                        interval.Results(j).DeltaLeverage = If(interval.Results(j).DeltaCost = 0, 0, interval.Results(j).DeltaMonetaryValue / interval.Results(j).DeltaCost)
                        interval.Results(j).DeltaExpectedSavings = interval.Results(j).ExpectedSavings - interval.Results(j - 1).ExpectedSavings 'A1611
                    End If
                Next

                If IsPointByPoint Then
                    EfficientFrontierCallback(stepResults, -1, IsCancelled)

                    If timer.ElapsedMilliseconds >= updateIntervalMilliseconds Then
                        EfficientFrontierIsCancelledCallback(IsCancelled)
                        timer.Restart()
                    End If
                End If
            Else
                totalBenefitsMax = EFValues(Xmax)
            End If

            If Xmin = interval.MinValue And Xmax = interval.MaxValue Then
                Eps = Math.Abs(totalBenefitsMin - totalBenefitsMax) / 100 * interval.DeltaValue
            End If

            Dim diff As Double = Math.Abs(totalBenefitsMin - totalBenefitsMax)
            'If valueOfEnterpriseDefined Then
            '    diff = Math.Abs(totalBenefitsMin - totalBenefitsMax) * ProjectManager.DollarValueOfEnterprise
            'End If
            If diff <= Eps Then
                Exit Sub
            Else
                Dim Xmid As Double = 0.5 * Xmin + 0.5 * Xmax ' instead of (Xmin + Xmax)/2 in order to avoid overflow
                CalculateInterval(Settings, interval, IsPointByPoint, riskWithoutControls, valueOfEnterpriseDefined, bModel, Xmin, Xmid, EventIDs, WRTNodeID, Eps)
                CalculateInterval(Settings, interval, IsPointByPoint, riskWithoutControls, valueOfEnterpriseDefined, bModel, Xmid, Xmax, EventIDs, WRTNodeID, Eps)
            End If
            'Dim Xmid As Double = 0.5 * Xmin + 0.5 * Xmax ' instead of (Xmin + Xmax)/2 in order to avoid overflow
            'CalculateInterval(Settings, interval, riskWithoutControls, valueOfEnterpriseDefined, bModel, Xmin, Xmid, EventIDs, WRTNodeID, Eps)
            'CalculateInterval(Settings, interval, riskWithoutControls, valueOfEnterpriseDefined, bModel, Xmid, Xmax, EventIDs, WRTNodeID, Eps)
        End Sub
    End Class
End Namespace