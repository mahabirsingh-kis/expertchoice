Option Strict On

Imports ECCore
Imports Canvas
Imports ExpertChoice.Service

Public Delegate Function SolveBaronFunction(sender As RiskOptimizer, EventIDs As List(Of Guid), ByRef FundedControls As List(Of Guid), ByRef TotalCost As Double) As Double
Public Delegate Function SolveBaronFunction2(sender As RASolver, EventIDs As List(Of Guid), ByRef FundedControls As List(Of Guid), ByRef TotalCost As Double, BaronModel As String, ByRef SolverState As raSolverState) As Double

<Serializable> Public Enum RiskOptimizationType
    BudgetLimit = 0
    MaxRisk = 1
    MinReduction = 2
End Enum

<Serializable> Public Class RiskOptimizer
    Public Property ProjectManager As clsProjectManager
    Public Property SolverLibrary As raSolverLibrary = raSolverLibrary.raBaron

    'Public SolveBaron As SolveBaronFunction    ' -D4071
    Private Const UseActiveControlsOnly As Boolean = False 'A1232

    Public Property BudgetLimit As Double = 600
    Public Property MaxRisk As Double = 0

    Private mMinReduction As Double

    Public Property Options As RASettings
        Get
            Return Controls.Options
        End Get
        Set(value As RASettings)
            Controls.Options = value
        End Set
    End Property

    Public Property MinReduction As Double
        Get
            Return mMinReduction
        End Get
        Set(value As Double)
            mMinReduction = value ' / 100  '-A1260
        End Set
    End Property

    Public Property OriginalRiskValue As Double = 0
    Public Property OriginalRiskValueWithControls As Double = 0 'A1238
    Public Property OptimizedRiskValue As Double = 0 'A1265
    Public Property OriginalAllControlsCost As Double = 0 'A1265
    Public Property OriginalSelectedControlsCost As Double = 0 'A1265
    Public Property OriginalSelectedControlsCount As Double = 0 'A1265

    Public Property OptimizationType As RiskOptimizationType = RiskOptimizationType.BudgetLimit

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

    Private Function GetBaronBudgetLimit(Path As String, ResName As String, EventIDs As List(Of Guid)) As String
        Controls.SetControlsVars(EventIDs)

        Dim ct As New clsCalculationTarget(CalculationTargetType.cttCombinedGroup, ProjectManager.CombinedGroups.GetDefaultCombinedGroup)
        Dim oldH As Integer = ProjectManager.ActiveHierarchy
        ProjectManager.ActiveHierarchy = ECHierarchyID.hidImpact
        ProjectManager.CalculationsManager.Calculate(ct, ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).Nodes(0))
        ProjectManager.ActiveHierarchy = ECHierarchyID.hidLikelihood
        ProjectManager.CalculationsManager.Calculate(ct, ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).Nodes(0))
        ProjectManager.ActiveHierarchy = oldH

        Dim sDescription As String = String.Format("// === Created with ExpertChoice Riskion Optimizer ===" + vbNewLine + "// Project: {0} [#{4}]" + vbNewLine + "// Date: {1}" + vbNewLine + "// User: <{2}> {3}" + vbNewLine + vbNewLine, ProjectManager.PipeParameters.ProjectName, Now.ToString, ProjectManager.User.UserEMail, ProjectManager.User.UserName, ProjectManager.StorageManager.ModelID)
        Dim res As String = String.Format("{2}OPTIONS {{ " + vbNewLine + " ResName: ""{0}{1}"";" + vbNewLine + " LocRes: 0; " + vbNewLine + " Summary: 0;" + vbNewLine + " Times: 0;" + vbNewLine + "}}" + vbNewLine + vbNewLine, Path, ResName, sDescription)

        Dim BudgetStr As String = ""
        Dim VarsStr As String = "BINARY_VARIABLES "

        Dim MustsStr As String = ""
        Dim MustNotsStr As String = ""
        Dim mustsCount As Integer = 0
        Dim mustnotsCount As Integer = 0

        For Each control As clsControl In Controls.EnabledControls '-A1392 DefinedControls '-A1383 Controls.Controls
            If control.VarID <> 0 Then
                Dim xName As String = "x_" + control.VarID.ToString
                VarsStr += CStr(If(VarsStr = "BINARY_VARIABLES ", xName, ", " + xName))
                Dim str As String = JS_SafeNumber(CDbl(If(Not control.IsCostDefined, 0, control.Cost))) + " * " + "x_" + control.VarID.ToString 'A1381
                BudgetStr += CStr(If(BudgetStr = "", str, " + " + str))
                If Options.Musts And control.Must Then
                    mustsCount += 1
                    MustsStr += CStr(If(MustsStr = "", "x_" + control.VarID.ToString, " + " + "x_" + control.VarID.ToString))
                End If
                If Options.MustNots And control.MustNot Then
                    mustnotsCount += 1
                    MustNotsStr += CStr(If(MustNotsStr = "", "x_" + control.VarID.ToString, " + " + "x_" + control.VarID.ToString))
                End If
            End If
        Next
        VarsStr += ";" + vbNewLine
        res += VarsStr + vbNewLine

        If Options.Musts And mustsCount > 0 Then
            MustsStr += " == " + mustsCount.ToString + ";"
        End If

        If Options.MustNots And mustnotsCount > 0 Then
            MustNotsStr += " == 0;"
        End If

        BudgetStr += " <= " + JS_SafeNumber(BudgetLimit) + ";" + vbNewLine

        Dim mustEq As String = ""
        If Options.Musts Then mustEq = CStr((If(mustsCount > 0, ", musts", "")))
        Dim mustnotsEq As String = ""
        If Options.MustNots Then mustnotsEq = CStr((If(mustnotsCount > 0, ", mustnots", "")))

        res += "EQUATIONS budget" + mustEq + mustnotsEq + " ;" + vbNewLine
        res += "budget: " + BudgetStr + vbNewLine
        If Options.Musts And mustsCount > 0 Then res += "musts: " + MustsStr + vbNewLine
        If Options.MustNots And mustnotsCount > 0 Then res += "mustnots: " + MustNotsStr + vbNewLine

        Dim TotalObjStr As String = ""

        Dim uncontributedEvents As List(Of clsNode) = ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).GetUncontributedAlternatives

        For Each eventID As Guid In EventIDs
            If uncontributedEvents.FirstOrDefault(Function(p) p.NodeGuidID.Equals(eventID)) Is Nothing Then
                Dim e As clsNode = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(eventID)
                Dim eventIntID As Integer = e.NodeID
                Dim isWithNoSource As Boolean = uncontributedEvents.Contains(e)

                Dim ObjStr As String = ""

                For Each source As clsNode In Sources
                    If source.GetNodesBelow(UNDEFINED_USER_ID).Contains(ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(eventID)) Then
                        If ObjStr = "" Then ObjStr = "("

                        Dim L As Double = source.UnnormalizedPriority
                        Dim V As Double = source.Judgments.Weights.GetUserWeights(COMBINED_USER_ID, ProjectManager.CalculationsManager.SynthesisMode, ProjectManager.CalculationsManager.IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(eventIntID)
                        Dim sControls As List(Of clsControl) = Controls.GetControlsForSource(source.NodeGuidID, UseActiveControlsOnly) 'A1232
                        Dim vControls As List(Of clsControl) = Controls.GetControlsForVulnerabilities(source.NodeGuidID, eventID, UseActiveControlsOnly) 'A1232

                        Dim sMult As String = JS_SafeNumber(L) + " * " + JS_SafeNumber(V)
                        ObjStr += CStr(If(ObjStr(ObjStr.Length - 1) = "(", sMult, " + " + sMult))
                        For Each control As clsControl In sControls
                            Dim xName As String = "x_" + control.VarID.ToString
                            Dim value As Double = Controls.GetAssignmentValue(control.ID, source.NodeGuidID, Guid.Empty)
                            If value <> 0 Then
                                ObjStr += " * (1 - " + JS_SafeNumber(value) + " * " + xName + ")"
                            End If
                        Next

                        For Each control As clsControl In vControls
                            Dim xName As String = "x_" + control.VarID.ToString
                            Dim value As Double = Controls.GetAssignmentValue(control.ID, source.NodeGuidID, eventID)
                            If value <> 0 Then
                                ObjStr += " * (1 - " + JS_SafeNumber(value) + " * " + xName + ")"
                            End If
                        Next
                    End If
                Next

                ObjStr += CStr(If(ObjStr = "", " (", ") * ("))

                For Each objective As clsNode In Objectives
                    If objective.GetNodesBelow(UNDEFINED_USER_ID).Contains(ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(eventID)) Then
                        Dim P As Double = objective.UnnormalizedPriority
                        Dim C As Double = objective.Judgments.Weights.GetUserWeights(COMBINED_USER_ID, ProjectManager.CalculationsManager.SynthesisMode, ProjectManager.CalculationsManager.IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(eventIntID)

                        Dim sMult As String = JS_SafeNumber(P) + " * " + JS_SafeNumber(C)
                        ObjStr += CStr(If(ObjStr(ObjStr.Length - 1) = "(", sMult, " + " + sMult))

                        Dim cControls As List(Of clsControl) = Controls.GetControlsForConsequences(objective.NodeGuidID, eventID, UseActiveControlsOnly) 'A1232
                        For Each control As clsControl In cControls
                            Dim xName As String = "x_" + control.VarID.ToString
                            Dim value As Double = Controls.GetAssignmentValue(control.ID, objective.NodeGuidID, eventID)
                            If value <> 0 Then
                                ObjStr += " * (1 - " + JS_SafeNumber(value) + " * " + xName + ")"
                            End If
                        Next
                    End If
                Next
                ObjStr += ")"

                TotalObjStr += CStr(If(TotalObjStr = "", ObjStr, " + " + ObjStr))
            End If
        Next

        If uncontributedEvents.Count > 0 Then
            'Dim riskData As RiskDataWRTNodeDataContract = ProjectManager.CalculationsManager.GetRiskDataWRTNode(COMBINED_USER_ID, "", ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).Nodes(0).NodeGuidID, ECCore.ControlsUsageMode.DoNotUse, EventIDs)

            For Each e As clsNode In uncontributedEvents
                Dim V As Double = e.UnnormalizedPriority
                Dim vControls As List(Of clsControl) = Controls.GetControlsForVulnerabilities(ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).Nodes(0).NodeGuidID, e.NodeGuidID, UseActiveControlsOnly)
                Dim sMult As String = JS_SafeNumber(V)
                TotalObjStr += CStr(If(TotalObjStr = "", sMult, " + " + sMult))
                For Each control As clsControl In vControls
                    Dim xName As String = "x_" + control.VarID.ToString
                    Dim value As Double = Controls.GetAssignmentValue(control.ID, ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).Nodes(0).NodeGuidID, e.NodeGuidID)
                    If value <> 0 Then
                        TotalObjStr += " * (1 - " + JS_SafeNumber(value) + " * " + xName + ")"
                    End If
                Next

                Dim added As Boolean = False
                'TotalObjStr += " * ("

                For Each objective As clsNode In Objectives
                    If objective.GetNodesBelow(UNDEFINED_USER_ID).Contains(ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(e.NodeGuidID)) Then
                        If Not added Then
                            TotalObjStr += " * ("
                            added = True
                        End If
                        Dim P As Double = objective.UnnormalizedPriority
                        Dim C As Double = objective.Judgments.Weights.GetUserWeights(COMBINED_USER_ID, ProjectManager.CalculationsManager.SynthesisMode, ProjectManager.CalculationsManager.IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(e.NodeID)

                        sMult = JS_SafeNumber(P) + " * " + JS_SafeNumber(C)
                        TotalObjStr += CStr(If(TotalObjStr(TotalObjStr.Length - 1) = "(", sMult, " + " + sMult))
                        Dim cControls As List(Of clsControl) = Controls.GetControlsForConsequences(objective.NodeGuidID, e.NodeGuidID, UseActiveControlsOnly)
                        For Each control As clsControl In cControls
                            Dim xName As String = "x_" + control.VarID.ToString
                            Dim value As Double = Controls.GetAssignmentValue(control.ID, objective.NodeGuidID, e.NodeGuidID)
                            If value <> 0 Then
                                TotalObjStr += " * (1 - " + JS_SafeNumber(value) + " * " + xName + ")"
                            End If
                        Next
                    End If
                Next
                If added Then TotalObjStr += ")"
            Next
        End If


        TotalObjStr += ";"

        res += "OBJ: minimize" + vbNewLine
        res += TotalObjStr + vbNewLine

        Return res
    End Function

    Private Function GetBaronMaxRisk(Path As String, ResName As String, EventIDs As List(Of Guid)) As String
        Dim MaxRiskValue As Double = 0
        Select Case OptimizationType
            Case RiskOptimizationType.MaxRisk
                MaxRiskValue = MaxRisk
            Case RiskOptimizationType.MinReduction
                MaxRiskValue = OriginalRiskValue * (1 - MinReduction)
        End Select

        Controls.SetControlsVars(EventIDs)

        Dim ct As New clsCalculationTarget(CalculationTargetType.cttCombinedGroup, ProjectManager.CombinedGroups.GetDefaultCombinedGroup)
        Dim oldH As Integer = ProjectManager.ActiveHierarchy
        ProjectManager.ActiveHierarchy = ECHierarchyID.hidImpact
        ProjectManager.CalculationsManager.Calculate(ct, ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).Nodes(0))
        ProjectManager.ActiveHierarchy = ECHierarchyID.hidLikelihood
        ProjectManager.CalculationsManager.Calculate(ct, ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).Nodes(0))
        ProjectManager.ActiveHierarchy = oldH

        Dim sDescription As String = String.Format("// === Created with ExpertChoice Riskion Optimizer ===" + vbNewLine + "// Project: {0} [#{4}]" + vbNewLine + "// Date: {1}" + vbNewLine + "// User: <{2}> {3}" + vbNewLine + vbNewLine, ProjectManager.PipeParameters.ProjectName, Now.ToString, ProjectManager.User.UserEMail, ProjectManager.User.UserName, ProjectManager.StorageManager.ModelID)
        Dim res As String = String.Format("{2}OPTIONS {{ " + vbNewLine + " ResName: ""{0}{1}"";" + vbNewLine + " LocRes: 0; " + vbNewLine + " Summary: 0;" + vbNewLine + " Times: 0;" + vbNewLine + "}}" + vbNewLine + vbNewLine, Path, ResName, sDescription)

        Dim BudgetStr As String = ""
        Dim VarsStr As String = "BINARY_VARIABLES "

        Dim MustsStr As String = ""
        Dim MustNotsStr As String = ""
        Dim mustsCount As Integer = 0
        Dim mustnotsCount As Integer = 0

        For Each control As clsControl In Controls.EnabledControls '-A1392 DefinedControls '-A1383 Controls.Controls
            If control.VarID <> 0 Then
                Dim xName As String = "x_" + control.VarID.ToString
                VarsStr += CStr(If(VarsStr = "BINARY_VARIABLES ", xName, ", " + xName))
                Dim str As String = JS_SafeNumber(CDbl(If(Not control.IsCostDefined, 0, control.Cost))) + " * " + "x_" + control.VarID.ToString 'A1381
                BudgetStr += CStr(If(BudgetStr = "", str, " + " + str))

                If Options.Musts And control.Must Then
                    mustsCount += 1
                    MustsStr += CStr(If(MustsStr = "", "x_" + control.VarID.ToString, " + " + "x_" + control.VarID.ToString))
                End If
                If Options.MustNots And control.MustNot Then
                    mustnotsCount += 1
                    MustNotsStr += CStr(If(MustNotsStr = "", "x_" + control.VarID.ToString, " + " + "x_" + control.VarID.ToString))
                End If
            End If
        Next
        VarsStr += ";" + vbNewLine
        res += VarsStr + vbNewLine

        If Options.Musts And mustsCount > 0 Then
            MustsStr += " == " + mustsCount.ToString + ";"
        End If

        If Options.MustNots And mustnotsCount > 0 Then
            MustNotsStr += " == 0;"
        End If

        Dim mustEq As String = ""
        If Options.Musts Then mustEq = CStr((If(mustsCount > 0, ", musts", "")))
        Dim mustnotsEq As String = ""
        If Options.MustNots Then mustnotsEq = CStr((If(mustnotsCount > 0, ", mustnots", "")))

        Dim TotalObjStr As String = ""

        Dim uncontributedEvents As List(Of clsNode) = ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).GetUncontributedAlternatives

        For Each eventID As Guid In EventIDs
            If uncontributedEvents.FirstOrDefault(Function(p) p.NodeGuidID.Equals(eventID)) Is Nothing Then
                Dim e As clsNode = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(eventID)
                Dim eventIntID As Integer = e.NodeID
                Dim isWithNoSource As Boolean = uncontributedEvents.Contains(e)

                Dim ObjStr As String = ""

                For Each source As clsNode In Sources
                    If source.GetNodesBelow(UNDEFINED_USER_ID).Contains(ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(eventID)) Then
                        If ObjStr = "" Then ObjStr = "("

                        Dim L As Double = source.UnnormalizedPriority
                        Dim V As Double = source.Judgments.Weights.GetUserWeights(COMBINED_USER_ID, ProjectManager.CalculationsManager.SynthesisMode, ProjectManager.CalculationsManager.IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(eventIntID)
                        Dim sControls As List(Of clsControl) = Controls.GetControlsForSource(source.NodeGuidID, UseActiveControlsOnly) 'A1232
                        Dim vControls As List(Of clsControl) = Controls.GetControlsForVulnerabilities(source.NodeGuidID, eventID, UseActiveControlsOnly) 'A1232

                        Dim sMult As String = JS_SafeNumber(L) + " * " + JS_SafeNumber(V)
                        ObjStr += CStr(If(ObjStr(ObjStr.Length - 1) = "(", sMult, " + " + sMult))
                        For Each control As clsControl In sControls
                            Dim xName As String = "x_" + control.VarID.ToString
                            Dim value As Double = Controls.GetAssignmentValue(control.ID, source.NodeGuidID, Guid.Empty)
                            If value <> 0 Then
                                ObjStr += " * (1 - " + JS_SafeNumber(value) + " * " + xName + ")"
                            End If
                        Next

                        For Each control As clsControl In vControls
                            Dim xName As String = "x_" + control.VarID.ToString
                            Dim value As Double = Controls.GetAssignmentValue(control.ID, source.NodeGuidID, eventID)
                            If value <> 0 Then
                                ObjStr += " * (1 - " + JS_SafeNumber(value) + " * " + xName + ")"
                            End If
                        Next
                    End If
                Next

                ObjStr += CStr(If(ObjStr = "", " (", ") * ("))

                For Each objective As clsNode In Objectives
                    If objective.GetNodesBelow(UNDEFINED_USER_ID).Contains(ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(eventID)) Then
                        Dim P As Double = objective.UnnormalizedPriority
                        Dim C As Double = objective.Judgments.Weights.GetUserWeights(COMBINED_USER_ID, ProjectManager.CalculationsManager.SynthesisMode, ProjectManager.CalculationsManager.IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(eventIntID)

                        Dim sMult As String = JS_SafeNumber(P) + " * " + JS_SafeNumber(C)
                        ObjStr += CStr(If(ObjStr(ObjStr.Length - 1) = "(", sMult, " + " + sMult))

                        Dim cControls As List(Of clsControl) = Controls.GetControlsForConsequences(objective.NodeGuidID, eventID, UseActiveControlsOnly) 'A1232
                        For Each control As clsControl In cControls
                            Dim xName As String = "x_" + control.VarID.ToString
                            Dim value As Double = Controls.GetAssignmentValue(control.ID, objective.NodeGuidID, eventID)
                            If value <> 0 Then
                                ObjStr += " * (1 - " + JS_SafeNumber(value) + " * " + xName + ")"
                            End If
                        Next
                    End If
                Next
                ObjStr += ")"

                TotalObjStr += CStr(If(TotalObjStr = "", ObjStr, " + " + ObjStr))
            End If
        Next

        If uncontributedEvents.Count > 0 Then
            'Dim riskData As RiskDataWRTNodeDataContract = ProjectManager.CalculationsManager.GetRiskDataWRTNode(COMBINED_USER_ID, "", ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).Nodes(0).NodeGuidID, ECCore.ControlsUsageMode.DoNotUse, EventIDs)

            For Each e As clsNode In uncontributedEvents
                Dim V As Double = e.UnnormalizedPriority
                Dim vControls As List(Of clsControl) = Controls.GetControlsForVulnerabilities(ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).Nodes(0).NodeGuidID, e.NodeGuidID, UseActiveControlsOnly)
                Dim sMult As String = JS_SafeNumber(V)
                TotalObjStr += CStr(If(TotalObjStr = "", sMult, " + " + sMult))
                For Each control As clsControl In vControls
                    Dim xName As String = "x_" + control.VarID.ToString
                    Dim value As Double = Controls.GetAssignmentValue(control.ID, ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).Nodes(0).NodeGuidID, e.NodeGuidID)
                    If value <> 0 Then
                        TotalObjStr += " * (1 - " + JS_SafeNumber(value) + " * " + xName + ")"
                    End If
                Next

                Dim added As Boolean = False
                'TotalObjStr += " * ("

                For Each objective As clsNode In Objectives
                    If objective.GetNodesBelow(UNDEFINED_USER_ID).Contains(ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(e.NodeGuidID)) Then
                        If Not added Then
                            TotalObjStr += " * ("
                            added = True
                        End If
                        Dim P As Double = objective.UnnormalizedPriority
                        Dim C As Double = objective.Judgments.Weights.GetUserWeights(COMBINED_USER_ID, ProjectManager.CalculationsManager.SynthesisMode, ProjectManager.CalculationsManager.IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(e.NodeID)

                        sMult = JS_SafeNumber(P) + " * " + JS_SafeNumber(C)
                        TotalObjStr += CStr(If(TotalObjStr(TotalObjStr.Length - 1) = "(", sMult, " + " + sMult))
                        Dim cControls As List(Of clsControl) = Controls.GetControlsForConsequences(objective.NodeGuidID, e.NodeGuidID, UseActiveControlsOnly)
                        For Each control As clsControl In cControls
                            Dim xName As String = "x_" + control.VarID.ToString
                            Dim value As Double = Controls.GetAssignmentValue(control.ID, objective.NodeGuidID, e.NodeGuidID)
                            If value <> 0 Then
                                TotalObjStr += " * (1 - " + JS_SafeNumber(value) + " * " + xName + ")"
                            End If
                        Next
                    End If
                Next
                If added Then TotalObjStr += ")"
            Next
        End If

        res += "EQUATIONS MaxRisk" + mustEq + mustnotsEq + " ;" + vbNewLine
        res += "MaxRisk: " + TotalObjStr + " <= " + JS_SafeNumber(MaxRiskValue) + ";" + vbNewLine
        If Options.Musts And mustsCount > 0 Then res += "musts: " + MustsStr + vbNewLine
        If Options.MustNots And mustnotsCount > 0 Then res += "mustnots: " + MustNotsStr + vbNewLine

        res += "OBJ: minimize" + vbNewLine
        res += BudgetStr + ";"

        Return res
    End Function

    Public Function GetBaron(Path As String, ResName As String, EventIDs As List(Of Guid)) As String
        Select Case OptimizationType
            Case RiskOptimizationType.BudgetLimit
                Return GetBaronBudgetLimit(Path, ResName, EventIDs)
            Case RiskOptimizationType.MaxRisk, RiskOptimizationType.MinReduction
                Return GetBaronMaxRisk(Path, ResName, EventIDs)
        End Select
        Return "Unknown optimization type"
    End Function

    Public Function ParseBaron(ResText As String, Output As String, EventIDs As List(Of Guid), ByRef FundedControls As List(Of Guid), ByRef TotalCost As Double) As Double        
        Return 0 'Unused function 'A1430
        Dim startStr As String = "The best solution found is:" + vbNewLine + vbNewLine
        Dim valueStr As String = "The above solution has an objective value of:"

        Dim result As Double = -1

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
                        Dim control As clsControl = Controls.GetControlByVarID(CInt(xName.Substring(2)))
                        FundedControls.Add(control.ID)
                        TotalCost += CDbl(If(Not control.Enabled OrElse Not control.IsCostDefined, 0, control.Cost)) 'A1381
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
        For Each ctrl As clsControl In ProjectManager.Controls.EnabledControls '-A1392 DefinedControls '-A1383 Controls.Controls
            Dim newIsActive As Boolean = FundedControls.Contains(ctrl.ID)
            ProjectManager.Controls.SetControlActive(ctrl.ID, newIsActive)
        Next
        'A1356 ==

        'If OptimizationType = RiskOptimizationType.MaxRisk Or OptimizationType = RiskOptimizationType.MinReduction Then
        '    result = GetModelRisk(ControlsUsageMode.UseOnlyActive, EventIDs) 'A1430
        'End If

        Return result
    End Function

    'Private Function OptimizeOld_Linearization(EventIDs As List(Of Guid), ByRef FundedControls As List(Of Guid), ByRef TotalCost As Double, Optional OutputPath As String = "") As Double
    '    Dim EventID As Guid = EventIDs(0) 'A1229

    '    Dim isXA As Boolean = SolverLibrary = raSolverLibrary.raXA
    '    'isXA = True
    '    Dim startTime As DateTime = Now
    '    Dim prepareTime As DateTime
    '    Dim endTime As DateTime

    '    Dim Reserve1 As Integer = 16222694  ' Confidential  Activation Codes
    '    Dim Reserve2 As Integer = 989284725

    '    Dim maxMemory As Integer = 0 '100000
    '    Dim model As New Optimizer.XA.Optimize(maxMemory, 0, 0, 0, 100, 100)

    '    Dim gEnv As Gurobi.GRBEnv = Nothing
    '    Dim gModel As Gurobi.GRBModel = Nothing
    '    Dim gurobiVars As New Dictionary(Of String, Gurobi.GRBVar)

    '    If isXA Then
    '        model.setActivationCodes(Reserve1, Reserve2)    ' Activation codes
    '        model.setXAMessageWindowOff()                   ' Remove XA Message Window

    '        model.openConnection()
    '        model.setMinimizeObjective()
    '    Else
    '        gEnv = New GRBEnv(OutputPath, "a9ce3faa-185a-4419-a7ec-d3f4a19df18e", "fS2ENZd0TmeDbicgLnW9Ng", "134005-default")
    '        gModel = New Gurobi.GRBModel(gEnv)
    '    End If

    '    Dim res As Double = 0

    '    Dim rowCount As Integer = 0

    '    Try
    '        Dim variables As New Dictionary(Of String, Double)

    '        Dim eventIntID As Integer = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(EventID).NodeID

    '        Dim ct As New clsCalculationTarget(CalculationTargetType.cttCombinedGroup, ProjectManager.CombinedGroups.GetDefaultCombinedGroup)
    '        Dim oldH As Integer = ProjectManager.ActiveHierarchy
    '        ProjectManager.ActiveHierarchy = ECHierarchyID.hidLikelihood
    '        ProjectManager.CalculationsManager.Calculate(ct, ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).Nodes(0))
    '        ProjectManager.ActiveHierarchy = ECHierarchyID.hidImpact
    '        ProjectManager.CalculationsManager.Calculate(ct, ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).Nodes(0))
    '        ProjectManager.ActiveHierarchy = oldH

    '        Controls.SetControlsVars(EventIDs)

    '        Dim includeIndividualConstraints As Boolean = True

    '        Dim constantValue As Double = 0
    '        For Each source As clsNode In Sources
    '            For Each objective As clsNode In Objectives
    '                Dim L As Double = source.UnnormalizedPriority
    '                Dim V As Double = source.Judgments.Weights.GetUserWeights(COMBINED_USER_ID, ProjectManager.CalculationsManager.SynthesisMode, ProjectManager.CalculationsManager.IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(eventIntID)
    '                Dim P As Double = objective.UnnormalizedPriority
    '                Dim C As Double = objective.Judgments.Weights.GetUserWeights(COMBINED_USER_ID, ProjectManager.CalculationsManager.SynthesisMode, ProjectManager.CalculationsManager.IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(eventIntID)
    '                Dim multiplier As Double = L * V * P * C
    '                constantValue += multiplier

    '                Dim sControls As List(Of clsControl) = Controls.GetControlsForSource(source.NodeGuidID, UseActiveControlsOnly) 'A1232
    '                Dim vControls As List(Of clsControl) = Controls.GetControlsForVulnerabilities(source.NodeGuidID, EventID, UseActiveControlsOnly) 'A1232
    '                Dim cControls As List(Of clsControl) = Controls.GetControlsForConsequences(objective.NodeGuidID, EventID, UseActiveControlsOnly) 'A1232
    '                Dim N As Integer = sControls.Count + vControls.Count + cControls.Count ' number of controls

    '                ' creating list of VarIDs that are use in this set of parathesis
    '                Dim controlsArray() As clsControl
    '                ReDim controlsArray(N)

    '                Dim m As Integer = 1
    '                For Each control As clsControl In sControls
    '                    controlsArray(m) = control
    '                    m += 1
    '                Next
    '                For Each control As clsControl In vControls
    '                    controlsArray(m) = control
    '                    m += 1
    '                Next
    '                For Each control As clsControl In cControls
    '                    controlsArray(m) = control
    '                    m += 1
    '                Next

    '                For i As Integer = 1 To N
    '                    Dim IDs() As Integer
    '                    ReDim IDs(i)
    '                    IDs(0) = 0
    '                    For t As Integer = 1 To i
    '                        IDs(t) = t
    '                    Next

    '                    'Dim N As Integer = 5
    '                    Dim k As Integer = i
    '                    Dim j As Integer = 1
    '                    While (j <> 0)
    '                        Dim name As String = "x"
    '                        Dim coeff As Double = 1
    '                        Dim s As String = ""
    '                        For i1 As Integer = 1 To k
    '                            name += "_" + controlsArray(IDs(i1)).VarID.ToString
    '                            s += IDs(i1).ToString
    '                            Dim value As Double
    '                            Select Case controlsArray(IDs(i1)).Type
    '                                Case ControlType.ctCause
    '                                    value = Controls.GetAssignmentValue(controlsArray(IDs(i1)).ID, source.NodeGuidID, Guid.Empty)
    '                                Case ControlType.ctCauseToEvent
    '                                    value = Controls.GetAssignmentValue(controlsArray(IDs(i1)).ID, source.NodeGuidID, EventID)
    '                                Case ControlType.ctConsequenceToEvent
    '                                    value = Controls.GetAssignmentValue(controlsArray(IDs(i1)).ID, objective.NodeGuidID, EventID)
    '                            End Select
    '                            coeff *= value
    '                        Next
    '                        coeff *= multiplier * CInt(If(k Mod 2 = 0, 1, -1))
    '                        If Not variables.ContainsKey(name) Then
    '                            variables.Add(name, coeff)
    '                            If isXA Then
    '                                model.setColumnObjective(name, coeff)
    '                                model.setColumnBinary(name)
    '                            Else
    '                                Dim gvar As GRBVar = gModel.AddVar(0, 1, coeff, GRB.BINARY, name)
    '                                gurobiVars.Add(name, gvar)
    '                                'gModel.Update()
    '                            End If

    '                            If k > 1 Then
    '                                If isXA Then
    '                                    For i1 As Integer = 1 To k
    '                                        If includeIndividualConstraints Then
    '                                            model.loadPoint(name + "_" + i1.ToString, name, 1)
    '                                            model.loadPoint(name + "_" + i1.ToString, "x_" + controlsArray(IDs(i1)).VarID.ToString, -1)
    '                                            model.setRowMax(name + "_" + i1.ToString, 0)
    '                                            rowCount += 1
    '                                        End If

    '                                        model.loadPoint(name, "x_" + controlsArray(IDs(i1)).VarID.ToString, -1)
    '                                    Next
    '                                    model.loadPoint(name, name, 1)
    '                                    model.setRowMin(name, -(k - 1))
    '                                Else
    '                                    'Dim constr1 As GRBLinExpr = 0.0
    '                                    'For i1 As Integer = 1 To k
    '                                    '    Dim constr As GRBLinExpr = 0.0

    '                                    '    constr.AddTerm(1, gurobiVars(name))
    '                                    '    constr.AddTerm(-1, gurobiVars("x_" + controlsArray(IDs(i1)).VarID.ToString))
    '                                    '    gModel.AddConstr(constr, GRB.LESS_EQUAL, 0, name + "_" + i1.ToString)
    '                                    '    rowCount += 1

    '                                    '    constr1.AddTerm(-1, gurobiVars("x_" + controlsArray(IDs(i1)).VarID.ToString))
    '                                    'Next
    '                                    'constr1.AddTerm(1, gurobiVars(name))
    '                                    'gModel.AddConstr(constr1, GRB.GREATER_EQUAL, -(k - 1), name)
    '                                End If
    '                                rowCount += 1
    '                            End If
    '                        Else
    '                            variables(name) += coeff
    '                            If isXA Then
    '                                model.setColumnObjective(name, variables(name))
    '                            Else
    '                                'gModel.(gurobiVars(name), variables(name))
    '                                'gurobiVars(name).Set( Set(GRB_)
    '                            End If
    '                        End If
    '                        'Debug.Print(name + " = " + " s = " + coeff.ToString)
    '                        j = k
    '                        While (IDs(j) = N - k + j) And j <> 0
    '                            j = j - 1
    '                        End While
    '                        IDs(j) += 1
    '                        For i1 As Integer = j + 1 To k
    '                            IDs(i1) = IDs(i1 - 1) + 1
    '                        Next
    '                    End While
    '                Next
    '            Next
    '        Next

    '        variables.Add("x0", constantValue)
    '        rowCount += 1
    '        If isXA Then
    '            model.setColumnObjective("x0", constantValue)
    '            model.setColumnBinary("x0")
    '            model.loadPoint("x0_1", "x0", 1)
    '            model.setRowFix("x0_1", 1)
    '        Else
    '            Dim gvar As GRBVar = gModel.AddVar(0, 1, constantValue, GRB.BINARY, "x0")
    '            gurobiVars.Add("x0", gvar)
    '            gModel.Update()

    '            If Not isXA Then
    '                For Each kvp As KeyValuePair(Of String, GRBVar) In gurobiVars
    '                    Dim strings() As String = kvp.Key.Split(CChar("_"))
    '                    If strings.Length > 2 Then
    '                        Dim constr1 As GRBLinExpr = 0.0
    '                        For i1 As Integer = 1 To strings.Length - 1
    '                            If includeIndividualConstraints Then
    '                                Dim constr As GRBLinExpr = 0.0

    '                                constr.AddTerm(1, gurobiVars(kvp.Key))
    '                                constr.AddTerm(-1, gurobiVars("x_" + strings(i1).ToString))
    '                                gModel.AddConstr(constr, GRB.LESS_EQUAL, 0, kvp.Key + "_" + i1.ToString)
    '                                rowCount += 1
    '                            End If

    '                            constr1.AddTerm(-1, gurobiVars("x_" + strings(i1).ToString))
    '                        Next
    '                        constr1.AddTerm(1, gurobiVars(kvp.Key))
    '                        gModel.AddConstr(constr1, GRB.GREATER_EQUAL, -(strings.Length - 1 - 1), kvp.Key)
    '                    End If
    '                Next
    '            End If
    '        End If

    '        ' budget limit constraint
    '        If isXA Then
    '            For Each control As clsControl In Controls.EnabledControls '-A1392 DefinedControls '-A1383 Controls.Controls
    '                If variables.ContainsKey("x_" + control.VarID.ToString) Then
    '                    model.loadPoint("budget", "x_" + control.VarID.ToString, CDbl(If(Not control.IsCostDefined, 0, control.Cost))) 'A1381
    '                End If
    '            Next
    '            model.setRowMax("budget", BudgetLimit)
    '        Else
    '            Dim constraintCost As GRBLinExpr = 0.0
    '            For Each control As clsControl In Controls.EnabledControls '-A1392 DefinedControls '-A1383 Controls.Controls
    '                If variables.ContainsKey("x_" + control.VarID.ToString) Then
    '                    constraintCost.AddTerm(CDbl(If(Not control.IsCostDefined, 0, control.Cost)), gurobiVars("x_" + control.VarID.ToString)) 'A1381
    '                End If
    '            Next
    '            gModel.AddConstr(constraintCost, GRB.LESS_EQUAL, BudgetLimit, "cost")

    '            Dim objControls As GRBLinExpr = 0.0
    '            For Each kvp As KeyValuePair(Of String, Double) In variables
    '                objControls.AddTerm(kvp.Value, gurobiVars(kvp.Key))
    '            Next
    '            gModel.SetObjective(objControls, GRB.MINIMIZE)
    '        End If
    '        rowCount += 1

    '        If isXA Then
    '            prepareTime = Now
    '            If OutputPath <> "" Then
    '                OutputPath = OutputPath.Replace("\", "\\")
    '                model.setCommand(String.Format("MatList Equ Output {0}xa_model.log Set CmprsName _   ListInput Yes ", OutputPath))
    '                model.setCommand(String.Format("FileName {0}xa_rcc_model    ToRcc Yes", OutputPath))
    '            End If

    '            'model.setCommand("Presolve 1")
    '            model.solve()
    '            endTime = Now
    '            Select Case model.getModelStatus
    '                Case 1
    '                    FundedControls = New List(Of Guid)
    '                    TotalCost = 0
    '                    For Each control As clsControl In Controls.EnabledControls '-A1392 DefinedControls '-A1383 Controls.Controls
    '                        If variables.ContainsKey("x_" + control.VarID.ToString) Then
    '                            Dim x As Double = model.getColumnPrimalActivity("x_" + control.VarID.ToString)
    '                            If x > 0 Then
    '                                FundedControls.Add(control.ID)
    '                                TotalCost += CDbl(If(Not control.IsCostDefined, 0, control.Cost)) 'A1381
    '                            End If
    '                            'Debug.Print("Control = " + control.Name + "  X" + control.VarID.ToString + " = " + x.ToString)
    '                        End If
    '                    Next
    '                    res = model.getIPObj
    '                    'Debug.Print("Result = " + res.ToString)
    '                    'Debug.Print("Total cost = " + TotalCost.ToString)
    '                Case 4
    '                    'Debug.Print("infeasible")
    '            End Select
    '        Else
    '            Dim usePresolve As Boolean = False
    '            If usePresolve Then gModel = gModel.Presolve

    '            prepareTime = Now

    '            gModel.Optimize()

    '            endTime = Now
    '            Dim status As Integer = gModel.Get(GRB.IntAttr.Status)

    '            Select Case status
    '                Case GRB.Status.OPTIMAL
    '                    FundedControls = New List(Of Guid)
    '                    TotalCost = 0
    '                    For Each control As clsControl In Controls.EnabledControls '-A1392 DefinedControls '-A1383 Controls.Controls
    '                        If variables.ContainsKey("x_" + control.VarID.ToString) Then
    '                            Dim x As Double
    '                            If usePresolve Then
    '                                x = gModel.GetVarByName("x_" + control.VarID.ToString).Get(GRB.DoubleAttr.X)
    '                            Else
    '                                x = gurobiVars("x_" + control.VarID.ToString).Get(GRB.DoubleAttr.X)
    '                            End If
    '                            If x > 0 Then
    '                                FundedControls.Add(control.ID)
    '                                TotalCost += CDbl(If(Not control.IsCostDefined, 0, control.Cost)) 'A1381
    '                            End If
    '                            'Debug.Print("Control = " + control.Name + "  X" + control.VarID.ToString + " = " + x.ToString)
    '                        End If
    '                    Next
    '                    res = gModel.ObjVal
    '                    'Debug.Print("Result = " + res.ToString)
    '                    'Debug.Print("Total cost = " + TotalCost.ToString)

    '                Case GRB.Status.INFEASIBLE
    '                    'Debug.Print("Infeasible")
    '            End Select

    '        End If

    '    Finally
    '        model.closeConnection()

    '        If gModel IsNot Nothing Then gModel.Dispose()
    '        If gEnv IsNot Nothing Then gEnv.Dispose()
    '    End Try
    '    Return res
    'End Function

    Public Function Optimize(EventIDs As List(Of Guid), ByRef FundedControls As List(Of Guid), ByRef TotalCost As Double, Optional OutputPath As String = "") As Double 'A1229
        If SolverLibrary = raSolverLibrary.raBaron Then
            If ProjectManager IsNot Nothing AndAlso ProjectManager.BaronSolverCallback IsNot Nothing Then   ' D4071
                Return ProjectManager.BaronSolverCallback(Me, EventIDs, FundedControls, TotalCost)   ' D4071
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

    Public Sub New(ProjectManager As clsProjectManager)
        Me.ProjectManager = ProjectManager
    End Sub
End Class
