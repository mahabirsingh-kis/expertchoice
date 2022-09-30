Imports System.IO
Imports GemBox.Spreadsheet
Imports GemBox.Spreadsheet.Charts
Imports GemBox.Spreadsheet.Drawing
Imports System.Drawing
Imports System.Collections.ObjectModel
Imports SpyronControls.Spyron.Core
Imports System.Security.Cryptography 'AS/21039b

<Serializable> Public Class ExcelExport
    Inherits clsComparionCorePage 'AS/20761b

    Private PM As clsProjectManager
    Private RA As ResourceAligner 'AS/20761a
    Private workbook As ExcelFile
    Private WRTNodeID As Integer
    Private ecColors As ECColors
    'Private UserIDs As ArrayList = Nothing
    Private _UsersList As List(Of clsApplicationUser) = Nothing

    Private Const NODE_PATH_DELIMITER As String = " | " '  & vbCrLf 
    Private TEMP_SINGLE_VALUE As Single = 0.1111111
    Private TEMP_STRING_VALUE As String = "temp string value"
    Private _currentScenarioID As Integer = 0 'AS/20761h

    Dim wrtNode As clsNode 'AS/19486m===
    'Get objectives
    Dim objH As clsHierarchy
    Dim altH As clsHierarchy
    Dim objs As New Dictionary(Of Integer, List(Of Tuple(Of Integer, Integer, NodePriority)))
    Dim alts As New Dictionary(Of Integer, List(Of NodePriority))
    Dim users As List(Of Integer) 'AS/19486m==

    Private Const OPT_ALT_NAME_LEN As Integer = 40 'AS/20761a
    Const REPORT_ALT_PREFIX As String = "A" 'AS/20761h

    Private Enum FundedFilteringModes 'AS/20761f
        ffAll = 0
        ffFunded = 1
        ffPartiallyFunded = 2
        ffFullyFunded = 3
        ffNotFunded = 4
    End Enum

    Function GenerateExcelReport(filePath As String, server As HttpServerUtility, Options As ReportGeneratorOptions) As ExcelFile

        If Options Is Nothing Then
            Options = New ReportGeneratorOptions
            Options.ReportTitle = PM.ProjectName
        End If

        objH = App.ActiveProject.HierarchyObjectives
        wrtNode = objH.Nodes(0) 'AS/19486t
        altH = PM.AltsHierarchy(PM.ActiveAltsHierarchy)
        PM.CalculationsManager.ShowDueToPriorities = Not PM.Parameters.ShowLikelihoodsGivenSources
        users = getAllUsersAndGroupsIDs()
        PM.CalculationsManager.GetAlternativesGrid(wrtNode.NodeID, users, objs, alts)

        Dim worksheet As ExcelWorksheet
        If Options.IncludeModelDescription Then worksheet = AddOverview(Options)

        If Options.IncludeAlternatives AndAlso altH.Nodes.Count > 0 Then 'AS/19486q enclosed
            worksheet = AddAlternatives(Options, server)
        End If 'AS/19486q

        If Options.IncludeObjectives Then worksheet = AddObjectives(Options, server)

        If Options.IncludeObjectivesDescriptions Then worksheet = AddObjectiveInfodocs(Options)

        If Options.IncludeAlternativesDescriptions AndAlso altH.Nodes.Count > 0 Then 'AS/19486q enclosed
            worksheet = AddAlernativeInfodocs(Options)
        End If 'AS/19486q

        If Options.IncludeAlternativesDescriptions OrElse Options.IncludeObjectivesDescriptions Then worksheet = AddAlternativesWRTObjectivesInfodocs(Options)

        If Options.IncludeContributions AndAlso altH.Nodes.Count > 0 Then 'AS/19486q enclosed
            worksheet = AddContributionMatrix(Options)
        End If 'AS/19486q

        If Options.IncludeParticipants Then worksheet = AddParticipants(Options)

        If Options.IncludeMeasurementMethods Then worksheet = AddMeasurementMethods(Options)

        If Options.IncludeSurveyResults Then AddSurveys(Options)

        If Options.IncludeInconsistency Then worksheet = AddInconsistency(Options)

        If Options.IncludeConsensus Then worksheet = AddConsensus(Options)

        If App.isRAAvailable AndAlso Options.IncludeResourceAligner Then
            ' generate RA reports 'AS/12-3-2020===
            worksheet = AddRA_GridMain(Options)
            worksheet = AddRA_TP_header(Options)
            worksheet = AddRA_TP_details(Options)
            worksheet = AddRA_Scenarios(Options)
            worksheet = AddRA_Dependencies(Options)
            worksheet = AddRA_Groups(Options)
            worksheet = AddRA_FundingPools(Options)
            worksheet = AddRA_CustomConstraints(Options)
            worksheet = AddRA_StrategicBuckets(Options)
            worksheet = AddRA_Vizualizations(Options)
            worksheet = AddRA_ScenarioComparisons(Options)
            worksheet = AddRA_ModelSpecification(Options)
            worksheet = AddRA_RelevantConstraints(Options)
        End If

        If workbook.Worksheets.Count = 0 Then workbook.Worksheets.Add("Worksheet")

        workbook.Save(filePath)
        Return workbook

    End Function

    'Function GenerateExcelReport_RA(filePath As String, server As HttpServerUtility, Options As ReportGeneratorOptions) As ExcelFile 'AS/20761 'AS/12-3-2020 disabled the sub

    '    If Options Is Nothing Then 'AS/12-3-2020===
    '        Options = New ReportGeneratorOptions
    '        Options.ReportTitle = PM.ProjectName
    '    End If

    '    objH = App.ActiveProject.HierarchyObjectives
    '    wrtNode = objH.Nodes(0)
    '    altH = PM.AltsHierarchy(PM.ActiveAltsHierarchy)
    '    PM.CalculationsManager.ShowDueToPriorities = Not PM.Parameters.ShowLikelihoodsGivenSources
    '    users = getAllUsersAndGroupsIDs()
    '    'PM.CalculationsManager.GetAlternativesGrid(wrtNode.NodeID, users, objs, alts) 'AS/12-3-2020==

    '    Dim worksheet As ExcelWorksheet
    '    worksheet = AddRA_GridMain(Options)
    '    worksheet = AddRA_TP_header(Options)
    '    worksheet = AddRA_TP_details(Options)
    '    worksheet = AddRA_Scenarios(Options)
    '    worksheet = AddRA_Dependencies(Options)
    '    worksheet = AddRA_Groups(Options)
    '    worksheet = AddRA_FundingPools(Options)
    '    worksheet = AddRA_CustomConstraints(Options)
    '    worksheet = AddRA_StrategicBuckets(Options)
    '    worksheet = AddRA_Vizualizations(Options)
    '    worksheet = AddRA_ScenarioComparisons(Options)
    '    worksheet = AddRA_ModelSpecification(Options)
    '    worksheet = AddRA_RelevantConstraints(Options)

    '    workbook.Save(filePath)
    '    Return workbook

    'End Function

#Region "RA Reports"

    Private Function AddRA_GridMain(Options As ReportGeneratorOptions) As ExcelWorksheet 'AS/20761
        'AS/20761q and 'AS/20761p - (no labels) applied numerous changes to this sub to implement the time periods in the Main grid tab

        Dim worksheet = workbook.Worksheets.Add("RA_Grid_Main")
        Dim maxPeriodsCount = getMaxPeriodsCount() 'AS/20761q

        'special rows:
        Dim rowColNamesGroups As Integer = 0
        Dim rowColNames As Integer = 1

        'special columns:
        Dim colScenario As Integer = 0 'include all scenarios in one table
        Dim colAltName As Integer = 1 'AS/20761n===
        Dim colAltID As Integer = 2
        Dim colAltGIID As Integer = 3
        Dim colFunded As Integer = 4
        Dim colBenefit As Integer = 5
        Dim colEBenefit As Integer = 6
        Dim colRisk As Integer = 7
        Dim colPSuccess As Integer = 8
        Dim colPFailure As Integer = 9
        Dim colCostTotal As Integer = 10 'AS/20761n== 
        Dim colCostPeriodsLast As Integer = colCostTotal + maxPeriodsCount
        Dim colGroupConstraints As Integer = colCostPeriodsLast + 1
        Dim colPartial As Integer = colCostPeriodsLast + 2
        Dim colMinPercent As Integer = colCostPeriodsLast + 3
        Dim colMust As Integer = colCostPeriodsLast + 4
        Dim colMustNot As Integer = colCostPeriodsLast + 5
        Dim colCustomConstraintFirst As Integer = colMustNot + 1
        Dim colEndTable As Integer = colMustNot

        Dim maxLenAltName As Integer = 10
        Dim maxLenAttrName As Integer = 10

        RA.Scenarios.CheckModel(, True) 'AS/20761p

        'Get list of attributes
        Dim customAttributes As List(Of clsAttribute) = PM.Attributes.GetAlternativesAttributes(True)

        Dim rowScenarioName As Integer = rowColNames
        Dim row As Integer = 1
        Dim col As Integer = 0

        Dim CellRange As CellRange = Nothing

        With worksheet
            'write columns names
            .Cells(rowColNames, colScenario).Value = "Scenario"
            .Cells(rowColNames, colAltGIID).Value = "Alt GUID"
            .Cells(rowColNames, colAltID).Value = "Alt ID"
            .Cells(rowColNames, colAltName).Value = "Alt Name"
            .Cells(rowColNames, colFunded).Value = "Funded"
            .Cells(rowColNames, colBenefit).Value = "Benefit"
            .Cells(rowColNames, colEBenefit).Value = "E.Benefit"
            .Cells(rowColNames, colRisk).Value = "   Risk    "
            .Cells(rowColNames, colPSuccess).Value = "P.Success"
            .Cells(rowColNames, colPFailure).Value = "P.Failure"
            .Cells(rowColNames, colCostTotal).Value = "  Total   " '"Total Cost" 'AS/20761n

            .Cells(rowColNamesGroups, colCostTotal).Value = "Cost"
            .Cells(rowColNames, colGroupConstraints).Value = "Group" & vbCrLf & "Constraints"
            .Cells(rowColNames, colPartial).Value = "Partial"
            .Cells(rowColNames, colMinPercent).Value = "Min %"
            .Cells(rowColNames, colMust).Value = "Must"
            .Cells(rowColNames, colMustNot).Value = "Must Not"

            'define Cost columns for the periods
            col = colCostTotal + 1
            Dim lstPeriodsCols As New List(Of Integer)
            For i = 0 To maxPeriodsCount - 1
                .Cells(rowScenarioName, col).Value = "Period" & (i + 1).ToString
                lstPeriodsCols.Add(col)

                .Columns(col).OutlineLevel = 1
                .Columns(col).Hidden = True
                .Columns(col).Collapsed = True

                col += 1
            Next

            Dim ii As Integer = 0 'AS/20761v
            While ii < RA.Scenarios.Scenarios.Count 'AS/20761v
                Dim tID As Integer = RA.Scenarios.Scenarios.Keys(ii)
                Dim sc As RAScenario = RA.Scenarios.Scenarios(tID)
                .Cells(row, colScenario).Value = sc.Name
                rowScenarioName = row
                row += 1

                ' reset solver and solve
                RA.Scenarios.ActiveScenarioID = sc.ID 'AS/20761v
                RA.Solver.ResetSolver() 'AS/20761m
                RA.Solver.Solve(raSolverExport.raNone) 'AS/20761m

                Dim TPResults As Dictionary(Of String, Integer) = sc.TimePeriods.TimePeriodResults 'AS/20761y

                Dim customConstraints As RAConstraints = sc.Constraints

                For Each tAlt As RAAlternative In sc.Alternatives
                    .Cells(row, colAltName).Value = tAlt.Name
                    .Cells(row, colAltID).Value = getAlternativeIDbyGUID(tAlt.ID)
                    .Cells(row, colAltGIID).Value = tAlt.ID
                    .Cells(row, colAltName).Value = tAlt.Name
                    .Cells(row, colFunded).Value = tAlt.Funded                      'Funded 
                    .Cells(row, colBenefit).Value = tAlt.BenefitOriginal            'Benefit
                    .Cells(row, colEBenefit).Value = tAlt.Benefit                   'EBenefit
                    .Cells(row, colRisk).Value = tAlt.Risk                          'Risk
                    .Cells(row, colPSuccess).Value = 1 - tAlt.RiskOriginal          'PSuccess
                    .Cells(row, colPFailure).Value = tAlt.RiskOriginal              'PFailure
                    .Cells(row, colCostTotal).Value = tAlt.Cost                     'totalCost
                    .Cells(row, colPartial).Value = Math.Abs(CInt(tAlt.IsPartial))  'isPartial
                    .Cells(row, colMinPercent).Value = tAlt.MinPercent              'MinPercent
                    .Cells(row, colMust).Value = Math.Abs(CInt(tAlt.Must))          'Must
                    .Cells(row, colMustNot).Value = Math.Abs(CInt(tAlt.MustNot))    'MustNot

                    Dim AltTPData = sc.TimePeriods.PeriodsData.GetAlternativePeriodsData(tAlt.ID)
                    Dim AltTPResult As Integer = AltTPData.MinPeriod 'AS/20761y===
                    If TPResults.ContainsKey(tAlt.ID) Then
                        AltTPResult = TPResults(tAlt.ID)
                    End If 'AS/20761y==

                    'group constraints===
                    For Each gr As KeyValuePair(Of String, RAGroup) In sc.Groups.Groups
                        For Each tAltinGroup As KeyValuePair(Of String, RAAlternative) In gr.Value.Alternatives
                            If tAltinGroup.Key = tAlt.ID Then
                                .Cells(row, colGroupConstraints).Value = "#" & gr.Value.IntID 'Group #
                                Exit For
                            End If
                        Next
                    Next 'group==

                    'custom constraints===
                    Dim colCustomConstraintLast As Integer = colCustomConstraintFirst
                    col = colCustomConstraintLast 'initially colCustomConstraintLast = colCustomConstraintFirst

                    For Each constraint As KeyValuePair(Of Integer, RAConstraint) In customConstraints.Constraints
                        .Cells(rowColNamesGroups, col).Value = constraint.Value.Name
                        .Cells(rowColNames, col).Value = "Total"

                        'fill out Total column for each custom constraint
                        If Not constraint.Value.LinkedAttributeID.Equals(Guid.Empty) Then 'AS/20761o
                            For Each attr As clsAttribute In customAttributes
                                Select Case attr.ValueType'AS/18387b===
                                    Case AttributeValueTypes.avtEnumeration, AttributeValueTypes.avtEnumerationMulti
                                        Dim aEnum As clsAttributeEnumeration = PM.Attributes.GetEnumByID(attr.EnumID) '(mimiced the piece from Public Function AddEnumAttributeItem in CWSw\CoreWS_OperationContracts.vb)
                                        If Not (aEnum Is Nothing OrElse attr.EnumID.Equals(Guid.Empty)) Then
                                            Dim itemGUID As Object = PM.Attributes.GetAttributeValue(attr.ID, New Guid(tAlt.ID))
                                            If Not IsNothing(itemGUID) AndAlso itemGUID.ToString <> Guid.Empty.ToString Then 'enclosed and added Else part in case categorical item not selected and there is no default value 'AS/2-12-2021
                                                ' D7534 ===
                                                Dim lstGuids As String() = itemGUID.ToString.Split(CChar(";"))
                                                For Each sGuid As String In lstGuids
                                                    Dim val As String = aEnum.GetItemByID(New Guid(sGuid)).Value
                                                    If val.ToLower = constraint.Value.Name.ToLower Then
                                                        .Cells.Item(row, col).Value = 1
                                                    End If
                                                Next
                                                ' D7534 ==
                                            Else
                                                .Cells.Item(row, col).Value = ""
                                            End If

                                            'continue in the same row - fill out the Periods columns
                                            col += 1
                                            For Each tResKey As Guid In sc.TimePeriods.Resources.Keys
                                                Dim rs As RAResource = sc.TimePeriods.Resources(tResKey)
                                                If constraint.Value.ID = rs.ConstraintID Then
                                                    If Trim(rs.Name.ToLower) <> "cost" Then
                                                        For i = AltTPResult To AltTPResult + AltTPData.Duration - 1
                                                            If i < sc.TimePeriods.Periods.Count Then
                                                                Dim tPeriod As RATimePeriod = sc.TimePeriods.Periods(i)
                                                                Dim tpType As Integer = sc.TimePeriods.PeriodsType
                                                                Dim tpStep As Integer = sc.TimePeriods.PeriodsStep
                                                                Dim tpStartDate As String = Now.ToString 'AS/21104b===
                                                                If tpType <> TimePeriodsType.tptYear Then
                                                                    tpStartDate = getTPstartDate(sc)
                                                                Else
                                                                    tpStartDate = getTPstartDate(sc, , True)
                                                                End If 'AS/21104b==

                                                                'periods names for current scenario
                                                                Dim tpName As String = getPeriodName(i, tpType, AltTPData.MinPeriod, tpStep, tpStartDate) 'AS/20761l===
                                                                .Cells(rowScenarioName, col + i).Value = tpName 'AS/21104c

                                                                'period data (cost)
                                                                Dim tpCost As Double = UNDEFINED_INTEGER_VALUE
                                                                tpCost = sc.TimePeriods.PeriodsData.GetResourceValue(i - AltTPResult, tAlt.ID, rs.ID) 'AS/20761y
                                                                If tpCost <> UNDEFINED_INTEGER_VALUE Then
                                                                    .Cells(row, col + i).Value = tpCost
                                                                End If
                                                            End If
                                                        Next 'period

                                                    End If
                                                Else 'fill out cost columns
                                                    If Trim(rs.Name.ToLower) = "cost" Then
                                                        For i = AltTPResult To sc.TimePeriods.Periods.Count - 1 'AS/21104c 'AS/20761y
                                                            Dim tPeriod As RATimePeriod = sc.TimePeriods.Periods(i)
                                                            Dim tpType As Integer = sc.TimePeriods.PeriodsType
                                                            Dim tpStep As Integer = sc.TimePeriods.PeriodsStep
                                                            Dim tpStartDate As String = Now.ToString 'AS/21104b===
                                                            If tpType <> TimePeriodsType.tptYear Then
                                                                tpStartDate = getTPstartDate(sc)
                                                            Else
                                                                tpStartDate = getTPstartDate(sc, , True)
                                                            End If 'AS/21104b==

                                                            'periods names for current scenario
                                                            Dim tpName As String = getPeriodName(i, tpType, AltTPData.MinPeriod, tpStep, tpStartDate) 'AS/20761l===
                                                            .Cells(rowScenarioName, lstPeriodsCols(i)).Value = tpName 'AS/21104c 'AS/20761t

                                                            'period data (cost)
                                                            Dim tpCost As Double = UNDEFINED_INTEGER_VALUE
                                                            tpCost = sc.TimePeriods.PeriodsData.GetResourceValue(i - AltTPResult, tAlt.ID, rs.ID) 'AS/20761y
                                                            If tpCost <> UNDEFINED_INTEGER_VALUE Then
                                                                .Cells(row, lstPeriodsCols(i)).Value = tpCost
                                                            End If
                                                        Next 'period  
                                                    End If 'cost
                                                End If
                                            Next

                                        End If
                                    Case Else
                                        Dim val As Object = PM.Attributes.GetAttributeValue(attr.ID, New Guid(tAlt.ID))
                                        If IsNothing(val) Then
                                            .Cells.Item(row, col).Value = String.Empty
                                        Else
                                            .Cells.Item(row, col).Value = val.ToString
                                            If maxLenAttrName < Len(val.ToString) Then maxLenAttrName = Len(val.ToString)
                                        End If
                                End Select
                            Next 'alts attributes
                        Else 'AS/20761o=== 'not linked
                            Dim rsAmount As Double = getResourceAmount(sc, constraint, AltTPResult, tAlt) 'AS/20761z
                            .Cells.Item(row, col).Value = rsAmount

                            'continue in the same row - fill out the Periods columns
                            col += 1
                            For Each tResKey As Guid In sc.TimePeriods.Resources.Keys
                                Dim rs As RAResource = sc.TimePeriods.Resources(tResKey)
                                If constraint.Value.ID = rs.ConstraintID Then
                                    If Trim(rs.Name.ToLower) <> "cost" Then
                                        For i = AltTPData.MinPeriod To sc.TimePeriods.Periods.Count - 1 'AS/21104c
                                            Dim tPeriod As RATimePeriod = sc.TimePeriods.Periods(i)
                                            Dim tpType As Integer = sc.TimePeriods.PeriodsType
                                            Dim tpStep As Integer = sc.TimePeriods.PeriodsStep
                                            Dim tpStartDate As String = Now.ToString 'AS/21104b===
                                            If tpType <> TimePeriodsType.tptYear Then
                                                tpStartDate = getTPstartDate(sc)
                                            Else
                                                tpStartDate = getTPstartDate(sc, , True)
                                            End If 'AS/21104b==

                                            'periods names for current scenario
                                            Dim tpName As String = getPeriodName(i, tpType, AltTPData.MinPeriod, tpStep, tpStartDate) 'AS/20761l===
                                            .Cells(rowScenarioName, col + i).Value = tpName 'AS/21104c 

                                            'period data (cost)
                                            Dim tpCost As Double = UNDEFINED_INTEGER_VALUE
                                            'tpCost = getPeriodCost(sc, i, tpStep, tPeriod.ID, AltTPData.MinPeriod, tAlt.ID, rs.ID) 'AS/20761z
                                            tpCost = sc.TimePeriods.PeriodsData.GetResourceValue(i - AltTPResult, tAlt.ID, rs.ID) 'AS/20761z
                                            If tpCost <> UNDEFINED_INTEGER_VALUE Then
                                                .Cells(row, col + i).Value = tpCost
                                            End If
                                        Next 'period
                                    End If
                                End If
                            Next

                        End If 'AS/20761o==

                        'periods names===-- write and format TP columns headers 
                        Dim colStartMerge As Integer = col - 1
                        For i = 0 To maxPeriodsCount - 1 'sc.TimePeriods.Periods.Count - 1
                            'col += 1
                            .Cells(rowColNames, col).Value = "Period" & (i + 1).ToString

                            .Columns(col).OutlineLevel = 1
                            .Columns(col).Hidden = True
                            .Columns(col).Collapsed = True

                            col += 1
                        Next

                        'adjust columns width including the columns with merged cells 'AS/20761s===
                        For i = colStartMerge To col - 1
                            .Columns(i).AutoFit()
                        Next i

                        CellRange = .Cells.GetSubrangeAbsolute(rowColNamesGroups, colStartMerge, rowColNamesGroups, col - 1)
                        CellRange.Merged = True
                        CellRange.Style.HorizontalAlignment = HorizontalAlignmentStyle.Center 'periods names== 
                        Dim fSize As Double = .Cells(rowColNamesGroups, colStartMerge).Style.Font.Size / 20
                        Dim fName As String = .Columns(colStartMerge).Style.Font.Name
                        Dim colW As Double, rowH As Double
                        If Not IsNothing(.Cells(rowColNamesGroups, colStartMerge).Value) Then 'AS/24176 enclosed
                            getCellSize(.Cells(rowColNamesGroups, colStartMerge).Value.ToString, fName, colW, rowH, CSng(fSize.ToString))
                        End If

                        .Columns(colStartMerge).SetWidth(colW, LengthUnit.Point) 'AS/20761s==

                        colCustomConstraintLast = col - 1

                    Next 'constraint
                    If colEndTable < colCustomConstraintLast Then colEndTable = colCustomConstraintLast
                    row += 1

                Next 'alternative
                ii += 1 'AS/20761v
            End While 'scenarios count 'AS/20761v

            'freeze top row
            .Panes = New WorksheetPanes(PanesState.Frozen, 0, 2, "A3", PanePosition.TopRight)

            'format
            'merge Costs cell
            CellRange = .Cells.GetSubrangeAbsolute(rowColNamesGroups, colCostTotal, rowColNamesGroups, colCostPeriodsLast)
            CellRange.Merged = True
            CellRange.Style.HorizontalAlignment = HorizontalAlignmentStyle.Center

            'title cells - wrap and bold
            CellRange = .Cells.GetSubrangeAbsolute(rowColNamesGroups, colScenario, rowColNamesGroups + 1, colEndTable)
            CellRange.Style.WrapText = True
            CellRange.Style.Font.Weight = ExcelFont.BoldWeight

            For i = 0 To colMustNot
                .Columns(i).AutoFit()
            Next i

            .Columns(colBenefit).Style.NumberFormat = "0.0000"
            .Columns(colEBenefit).Style.NumberFormat = "0.0000"
            .Columns(colRisk).Style.NumberFormat = "0.0000"
            .Columns(colPSuccess).Style.NumberFormat = "0.0000"
            .Columns(colPFailure).Style.NumberFormat = "0.0000"

            .ViewOptions.OutlineColumnButtonsRight = False

        End With

        Return worksheet

    End Function


    Private Function AddRA_TP_header(Options As ReportGeneratorOptions) As ExcelWorksheet 'AS/20761j
        Dim worksheet = workbook.Worksheets.Add("RA_TP_header")

        Dim rowColumnHeader As Integer = 0 'column header
        Dim rowPeriodFormat As Integer = 1
        Dim rowPeriodStep As Integer = 2
        Dim rowStartDate As Integer = 3
        Dim rowUseDiscount As Integer = 4
        Dim rowDiscountValue As Integer = 5

        Dim colName As Integer = 0
        Dim col As Integer = 0 'col number

        With worksheet
            'write columns headers
            .Cells(rowColumnHeader, colName).Value = "Scenario: "
            .Cells(rowColumnHeader, colName).Style.HorizontalAlignment = HorizontalAlignmentStyle.Right

            .Cells(rowPeriodFormat, colName).Value = "Period format"
            .Cells(rowPeriodStep, colName).Value = "Period step"
            .Cells(rowStartDate, colName).Value = "Start date/year"
            .Cells(rowUseDiscount, colName).Value = "Use discount factor" '0/1
            .Cells(rowDiscountValue, colName).Value = "Discount factor used"

            col = colName + 1
            For Each tID As Integer In RA.Scenarios.Scenarios.Keys
                Dim sc As RAScenario = RA.Scenarios.Scenarios(tID)
                .Cells(rowColumnHeader, col).Value = sc.Name
                .Cells(rowPeriodFormat, col).Value = getTPformat(sc.TimePeriods.PeriodsType)
                .Cells(rowPeriodStep, col).Value = sc.TimePeriods.PeriodsStep
                .Cells(rowStartDate, col).Value = getTPstartDate(sc) 'getTPstartDate(sc,, True) 'AS/21104a
                .Cells(rowUseDiscount, col).Value = Math.Abs(CInt(sc.TimePeriods.UseDiscountFactor)) 'AS/21104a
                .Cells(rowDiscountValue, col).Value = sc.TimePeriods.DiscountFactor
                col += 1
            Next

            'freeze top row
            .Panes = New WorksheetPanes(PanesState.Frozen, 0, 1, "A2", PanePosition.TopRight)

            'format
            For i = 0 To col
                .Columns(i).AutoFit()
            Next i

        End With

        Return worksheet
    End Function

    Private Function AddRA_TP_details(Options As ReportGeneratorOptions) As ExcelWorksheet 'AS/20761k
        Dim worksheet = workbook.Worksheets.Add("RA_TP_details")

        Dim rowTPname As Integer = 0

        Dim colScenario As Integer = 0
        Dim colResource As Integer = 1
        Dim colAltName As Integer = 2
        Dim colAltID As Integer = 3
        Dim colAltGUID As Integer = 4
        Dim colResValue As Integer = 5
        Dim colEarliest As Integer = 6
        Dim colLatest As Integer = 7
        Dim colDuration As Integer = 8
        Dim colMust As Integer = 9
        Dim colPeriodFormat As Integer = 10
        Dim colFunded As Integer = 11

        Dim maxTPcolumn As Integer = colFunded + 1 'AS/21104c
        Dim rowScenarioName As Integer = 0 'AS/21104c
        Dim row As Integer = 0
        Dim col As Integer = 0

        With worksheet
            .Cells(row, colScenario).Value = "Scenario"
            .Cells(row, colResource).Value = "Resource Name"
            .Cells(row, colAltName).Value = "Alternative Name"
            .Cells(row, colAltID).Value = "Alternative ID"
            .Cells(row, colAltGUID).Value = "Alternative GUID"
            .Cells(row, colResValue).Value = "Resource" & vbCrLf & "amount"
            .Cells(row, colEarliest).Value = "Earliest"
            .Cells(row, colLatest).Value = "Latest "
            .Cells(row, colDuration).Value = "Duration "
            .Cells(row, colMust).Value = "Musts for" & vbCrLf & "specific periods"
            .Cells(row, colPeriodFormat).Value = "Period" & vbCrLf & "format"
            .Cells(row, colFunded).Value = "Funded"

            row += 1
            Dim ii As Integer = 0 'AS/20761v
            While ii < RA.Scenarios.Scenarios.Count 'AS/20761v
                Dim tID As Integer = RA.Scenarios.Scenarios.Keys(ii)

                Dim sc As RAScenario = RA.Scenarios.Scenarios(tID)
                .Cells(row, colScenario).Value = sc.Name
                rowScenarioName = row 'AS/21104c

                If Not sc.Settings.TimePeriods Then
                    .Cells(row, colScenario).Value = sc.Name & vbCrLf & "Time periods disabled" ' "Time periods are ignored for this scenario"
                End If
                .Cells(row, colPeriodFormat).Value = getTPformat(sc.TimePeriods.PeriodsType)

                row += 1

                ' reset solver and solve
                RA.Scenarios.ActiveScenarioID = sc.ID 'AS/20761v
                RA.Solver.ResetSolver()
                RA.Solver.Solve(raSolverExport.raNone)

                Dim TPResults As Dictionary(Of String, Integer) = sc.TimePeriods.TimePeriodResults 'AS/20761y

                For Each tResKey As Guid In sc.TimePeriods.Resources.Keys
                    Dim rs As RAResource = sc.TimePeriods.Resources(tResKey)
                    .Cells(row, colResource).Value = rs.Name
                    .Rows(row).OutlineLevel = 1
                    .Rows(row).Hidden = True
                    row += 1

                    For Each tAlt As RAAlternative In sc.Alternatives
                        .Cells(row, colAltName).Value = tAlt.Name
                        .Cells(row, colAltID).Value = getAlternativeIDbyGUID(tAlt.ID)
                        .Cells(row, colAltGUID).Value = tAlt.ID
                        .Rows(row).OutlineLevel = 2
                        .Rows(row).Hidden = True

                        Dim AltTPData = sc.TimePeriods.PeriodsData.GetAlternativePeriodsData(tAlt.ID)
                        Dim AltTPResult As Integer = AltTPData.MinPeriod 'AS/20761y===
                        If TPResults.ContainsKey(tAlt.ID) Then
                            AltTPResult = TPResults(tAlt.ID)
                        End If 'AS/20761y==

                        .Cells(row, colEarliest).Value = AltTPData.MinPeriod + 1 'indexation starts with 0 so in the table we add 1
                        .Cells(row, colLatest).Value = AltTPData.MaxPeriod + 1 'AS/20761p
                        .Cells(row, colDuration).Value = AltTPData.Duration
                        .Cells(row, colMust).Value = Math.Abs(CInt(AltTPData.HasMust))
                        .Cells(row, colFunded).Value = CInt(tAlt.Funded)

                        col = colFunded + 1
                        Dim resourceAmount As Double = 0 'AS/21104c

                        For i = AltTPData.MinPeriod To sc.TimePeriods.Periods.Count - 1 'AS/21104c 'AS/20761y
                            Dim tPeriod As RATimePeriod = sc.TimePeriods.Periods(i)
                            Dim tpType As Integer = sc.TimePeriods.PeriodsType
                            Dim tpStep As Integer = sc.TimePeriods.PeriodsStep
                            Dim tpStartDate As String = Now.ToString 'AS/21104b===
                            If tpType <> TimePeriodsType.tptYear Then
                                tpStartDate = getTPstartDate(sc)
                            Else
                                tpStartDate = getTPstartDate(sc, , True)
                            End If 'AS/21104b==

                            'period name
                            Dim tpName As String = getPeriodName(i, tpType, AltTPData.MinPeriod, tpStep, tpStartDate) 'AS/20761l===
                            .Cells(rowScenarioName, col + AltTPData.MinPeriod).Value = tpName 'AS/21104c

                            'period data (cost)
                            Dim tpCost As Double = UNDEFINED_INTEGER_VALUE
                            tpCost = sc.TimePeriods.PeriodsData.GetResourceValue(i - AltTPResult, tAlt.ID, rs.ID) 'AS/20761y
                            If tpCost <> UNDEFINED_INTEGER_VALUE Then
                                .Cells(row, col + AltTPData.MinPeriod).Value = tpCost
                                resourceAmount = resourceAmount + tpCost
                            End If

                            If maxTPcolumn < col Then maxTPcolumn = col 'AS/21104c

                            col += 1
                        Next 'period

                        'Resource amount
                        If resourceAmount <> 0 Then .Cells(row, colResValue).Value = resourceAmount 'AS/21104c

                        '==================
                        row += 1
                    Next 'alternative
                Next 'resource
                ii += 1 'AS/20761v
            End While 'scenarios count 'AS/20761v

            .Rows(row).Collapsed = True

            'freeze top row
            .Panes = New WorksheetPanes(PanesState.Frozen, 0, 1, "A2", PanePosition.TopRight)

            'format
            Dim CellRange As CellRange = .Cells.GetSubrangeAbsolute(rowTPname, colScenario, rowTPname, colFunded)
            CellRange.Style.WrapText = True

            .Cells(rowTPname, colFunded + 1).Value = "Time Periods" 'AS/21104c===
            CellRange = .Cells.GetSubrangeAbsolute(rowTPname, colFunded + 1, rowTPname, maxTPcolumn)
            CellRange.Merged = True
            CellRange.Style.HorizontalAlignment = HorizontalAlignmentStyle.Center 'AS/21104c

            For i = 0 To col
                .Columns(i).AutoFit()
            Next i

            .ViewOptions.OutlineRowButtonsBelow = False

        End With

        Return worksheet
    End Function

    Private Function AddRA_Scenarios(Options As ReportGeneratorOptions) As ExcelWorksheet 'AS/19486i
        Dim worksheet = workbook.Worksheets.Add("RA_Scenarios")

        Dim rowColumnHeader As Integer = 0 'column header

        Dim colName As Integer = 0
        Dim colDescription As Integer = 1
        Dim colCombinedGroup As Integer = 2

        Dim row As Integer = 1 'row number 
        Dim col As Integer = 0 'col number


        With worksheet
            'wrtite columns headers
            .Cells(rowColumnHeader, colName).Value = "Name"
            .Cells(rowColumnHeader, colDescription).Value = "Description"
            .Cells(rowColumnHeader, colCombinedGroup).Value = "Combined Group"

            For Each tID As Integer In RA.Scenarios.Scenarios.Keys
                Dim sc As RAScenario = RA.Scenarios.Scenarios(tID)
                .Cells(row, colName).Value = sc.Name
                .Cells(row, colDescription).Value = sc.Description
                If sc.CombinedGroupUserID <> Integer.MinValue Then 'AS/21104b===
                    .Cells(row, colCombinedGroup).Value = PM.CombinedGroups.GetGroupByCombinedID(sc.CombinedGroupUserID).Name
                End If 'AS/21104b==
                row += 1
            Next

            'freeze top row
            .Panes = New WorksheetPanes(PanesState.Frozen, 0, 1, "A2", PanePosition.TopRight) 'AS/19486_g

            'format
            .Columns(colName).AutoFit()
            .Columns(colDescription).AutoFit()
            .Columns(colCombinedGroup).AutoFit()

        End With

        Return worksheet
    End Function

    Private Function AddRA_Dependencies(Options As ReportGeneratorOptions) As ExcelWorksheet 'AS/20761a
        Dim worksheet = workbook.Worksheets.Add("RA_Dependencies")

        Dim rowColumnHeader As Integer = 0 'column header

        Dim colScenario As Integer = 0
        Dim colDependency As Integer = 1

        Dim row As Integer = 0 'row number 
        Dim col As Integer = 0 'col number

        With worksheet
            .Cells(row, colScenario).Value = "Scenario"
            .Cells(row, colDependency).Value = "Dependency"
            row += 1

            For Each tID As Integer In RA.Scenarios.Scenarios.Keys
                Dim sc As RAScenario = RA.Scenarios.Scenarios(tID)

                For Each tDep As RADependency In sc.Dependencies.Dependencies
                    .Cells(row, colScenario).Value = sc.Name
                    '.Cells(row, colDependency).Value = GetAltName(tDep.FirstAlternativeID) & " depends on " & GetAltName(tDep.SecondAlternativeID) ' CInt(tDep.Value) 'AS/21104a

                    Dim tpDep As RADependency = sc.TimePeriodsDependencies.GetDependency(tDep.FirstAlternativeID, tDep.SecondAlternativeID) 'AS/21104a===
                    Dim isTP As Boolean = isTimePeriodsVisible()
                    Dim sDependancyName As String = GetDependencyName(tDep, tpDep, isTP, True)  'true - to get full alt name
                    .Cells(row, colDependency).Value = sDependancyName 'AS/21104a==

                    row += 1
                Next
                row += 1
            Next

            'freeze top row
            .Panes = New WorksheetPanes(PanesState.Frozen, 0, 1, "A2", PanePosition.TopRight)

            'format
            .Columns(colScenario).AutoFit()
            .Columns(colDependency).AutoFit()

        End With

        Return worksheet
    End Function

    Private Function AddRA_Groups(Options As ReportGeneratorOptions) As ExcelWorksheet 'AS/20761b
        Dim worksheet = workbook.Worksheets.Add("RA_Groups")

        Dim rowColumnHeader As Integer = 0 'column header

        Dim colAltName As Integer = 3 '0 'AS/21104d===
        Dim colScenario As Integer = 0 '1
        Dim colGroupName As Integer = 1 '2
        Dim colGroupType As Integer = 2 'AS/21104d==

        Dim row As Integer = 0 'row number 
        Dim col As Integer = 0 'col number

        With worksheet
            .Cells(row, colScenario).Value = "Scenario"
            .Cells(row, colGroupName).Value = "Group constraint name"
            .Cells(row, colAltName).Value = "Alternatives in group"
            .Cells(row, colGroupType).Value = "Group constraint type"
            row += 1


            For Each tID As Integer In RA.Scenarios.Scenarios.Keys
                Dim sc As RAScenario = RA.Scenarios.Scenarios(tID)
                Dim scName As String = sc.Name 'AS/21104d
                .Cells(row, colScenario).Value = scName 'AS/21104d

                For Each gr As KeyValuePair(Of String, RAGroup) In sc.Groups.Groups
                    Dim sGroupName As String = gr.Value.Name 'AS/21104d===
                    .Cells(row, colGroupName).Value = sGroupName

                    Dim sType As String = String.Format(ResString("optGroupType" + CInt(gr.Value.Condition).ToString), "alternatives")
                    sType = ReplaceSpecialChars(sType)
                    .Cells(row, colGroupType).Value = sType 'AS/21104d==

                    For Each tAlt As KeyValuePair(Of String, RAAlternative) In gr.Value.Alternatives
                        If tAlt.Value IsNot Nothing Then
                            'Dim scName As String = sc.Name 'AS/21104d
                            '.Cells(row, colScenario).Value = scName 'AS/21104d

                            Dim sAltName As String = tAlt.Value.Name
                            .Cells(row, colAltName).Value = sAltName

                            'Dim sGroupName As String = gr.Value.Name 'AS/21104d===
                            '.Cells(row, colGroupName).Value = sGroupName

                            'Dim sType As String = String.Format(ResString("optGroupType" + CInt(gr.Value.Condition).ToString), "alternatives")
                            'sType = ReplaceSpecialChars(sType)
                            '.Cells(row, colGroupType).Value = sType 'AS/21104d==

                            row += 1
                        End If
                    Next ' Alt In gr.Value.Alternatives
                Next ' gr In sc.Groups.Groups
            Next ' ID in Scenarios.Keys


            'freeze top row
            .Panes = New WorksheetPanes(PanesState.Frozen, 0, 1, "A2", PanePosition.TopRight)

            'format
            .Columns(colAltName).AutoFit()
            .Columns(colScenario).AutoFit()
            .Columns(colGroupName).AutoFit()
            .Columns(colGroupType).AutoFit()

            'sort by alt name
            '.Cells.GetSubrangeAbsolute(1, colAltName, row, colGroupType).Sort(False).By(0).Apply() 'AS/21149a 'AS/21104d

        End With

        Return worksheet
    End Function

    Private Function AddRA_FundingPools(Options As ReportGeneratorOptions) As ExcelWorksheet 'AS/20761c
        Dim worksheet = workbook.Worksheets.Add("RA_FundingPools")

        Dim rowColumnHeader As Integer = 0 'column header

        Dim colScenario As Integer = 0
        Dim colAltGUID As Integer = 1
        Dim colAltID As Integer = 2
        Dim colAltName As Integer = 3
        Dim colResourceAmount As Integer = 4
        Dim colFirstPool As Integer = 5
        Dim numPools As Integer = RA.Scenarios.ActiveScenario.FundingPools.Pools.Count

        Dim colRightMostColumn As Integer = colFirstPool + numPools
        Dim row As Integer = 0 'row number 
        Dim col As Integer = 0 'col number

        With worksheet
            .Cells(row, colScenario).Value = "Scenario"
            .Cells(row, colAltGUID).Value = "Alternative GUID"
            .Cells(row, colAltID).Value = "Alternative ID"
            .Cells(row, colAltName).Value = "Alternative Name"
            .Cells(row, colResourceAmount).Value = "Cost" 'former "Resource Amount"

            row = rowColumnHeader + 1
            For Each scID As Integer In RA.Scenarios.Scenarios.Keys
                Dim sc As RAScenario = RA.Scenarios.Scenarios(scID)

                If RA.Solver.SolverState <> raSolverState.raSolved Then
                    If sc.Alternatives.Count > 0 Then 'AndAlso sc.Settings.FundingPools Then   
                        RA.Solve()
                    End If
                End If

                If RA.Solver.SolverState = raSolverState.raSolved Then
                    For Each tAlt As RAAlternative In sc.Alternatives
                        .Cells(row, colScenario).Value = sc.Name
                        .Cells(row, colAltGUID).Value = tAlt.ID
                        .Cells(row, colAltID).Value = getAlternativeIDbyGUID(tAlt.ID)
                        .Cells(row, colAltName).Value = tAlt.Name
                        .Cells(row, colResourceAmount).Value = tAlt.Cost

                        col = colFirstPool
                        For Each tID As Integer In sc.FundingPools.GetPoolsOrderAsList
                            Dim pool As RAFundingPool = sc.FundingPools.Pools(tID)
                            With pool
                                Debug.Print(sc.Name & ", " & tAlt.Name & ", " & ", " & .Name & ", " & .PoolLimit.ToString)

                                worksheet.Cells(rowColumnHeader, col).Value = .Name & " " & ResString("lblRAFP_Limit")
                                Dim tVal As Double = .GetAlternativeValue(tAlt.ID)
                                If tVal <> UNDEFINED_INTEGER_VALUE Then worksheet.Cells(row, col).Value = tVal

                                col += 1
                                worksheet.Cells(rowColumnHeader, col).Value = .Name & " " & ResString("lblRAFP_Allocated")
                                tVal = .GetAlternativeAllocatedValue(tAlt.ID)
                                If tVal <> UNDEFINED_INTEGER_VALUE Then worksheet.Cells(row, col).Value = tVal
                                col += 1
                                If colRightMostColumn < col - 1 Then colRightMostColumn = col - 1

                            End With
                        Next 'pool
                        row += 1
                    Next 'alternative
                End If
                row += 1
            Next ' scenario

            'freeze top row
            .Panes = New WorksheetPanes(PanesState.Frozen, 0, 1, "A2", PanePosition.TopRight)

            'format
            For col = colScenario To colRightMostColumn
                .Columns(col).AutoFit()
            Next

        End With

        Return worksheet
    End Function

    Private Function AddRA_CustomConstraints(Options As ReportGeneratorOptions) As ExcelWorksheet 'AS/20761d

        Dim worksheet = workbook.Worksheets.Add("RA_CustomConstraints")

        Dim rowColumnHeader As Integer = 0 'names of attributes and covering objectives

        Dim colScenario As Integer = 0
        Dim colConstraintName As Integer = 1 'is also a resource
        Dim colAltAtributeLink As Integer = 2 'no link, link only, link w auto updating
        Dim colIgnore As Integer = 3 '0/1

        Dim maxLenAltName As Integer = 10
        Dim maxLenAttrName As Integer = 10

        Dim row As Integer = 1
        Dim col As Integer = 0

        With worksheet
            'write columns names
            .Cells(rowColumnHeader, colScenario).Value = "Scenario"
            .Cells(rowColumnHeader, colConstraintName).Value = "Constraint Name"
            .Cells(rowColumnHeader, colAltAtributeLink).Value = "Alternative Atribute Link"
            .Cells(rowColumnHeader, colIgnore).Value = "Ignore"

            For Each scID As Integer In RA.Scenarios.Scenarios.Keys
                Dim sc As RAScenario = RA.Scenarios.Scenarios(scID)
                Dim customConstraints As RAConstraints = sc.Constraints

                If sc.Constraints.Constraints.Count > 0 Then
                    For Each pair As KeyValuePair(Of Integer, RAConstraint) In customConstraints.Constraints
                        .Cells(row, colScenario).Value = sc.Name
                        Dim constraint As RAConstraint = pair.Value

                        Dim sLink As String = getConstraintLinkedInfo(constraint)
                        .Cells(row, colAltAtributeLink).Value = sLink

                        Dim sName As String = constraint.Name
                        .Cells(row, colConstraintName).Value = sName

                        Dim ignore As Boolean = constraint.Enabled
                        .Cells(row, colIgnore).Value = Math.Abs(CInt(Not (ignore))) 'AS/21104b

                        row += 1
                    Next 'constraint
                    row += 1
                End If
            Next 'scenario

            'freeze top row
            .Panes = New WorksheetPanes(PanesState.Frozen, 0, 1, "A2", PanePosition.TopRight)

            'format
            For i = colScenario To colIgnore
                .Columns(i).AutoFit()
            Next i

        End With

        Return worksheet

    End Function

    Private Function AddRA_StrategicBuckets(Options As ReportGeneratorOptions) As ExcelWorksheet 'AS/20761e

        Dim worksheet = workbook.Worksheets.Add("RA_StrategicBuckets")

        'Get list of attributes, periods
        Dim customAttributes As List(Of clsAttribute) = PM.Attributes.GetAlternativesAttributes(True)

        Dim row As Integer = 1
        Dim col As Integer = 0

        'special rows:
        Dim rowColumnHeader As Integer = 0 'names of attributes and covering objectives

        'special columns:
        Dim colScenario As Integer = 0 'include all scenarios in one table
        Dim colAltName As Integer = 1
        Dim colFunded As Integer = 2 'AS/20761q===
        Dim colTotal As Integer = 3
        Dim colTotalCost As Integer = 4
        Dim colCustomAttributesFirst As Integer = 5
        Dim colCustomAttributesLast As Integer = colCustomAttributesFirst + customAttributes.Count 'AS/20761q==

        Dim maxLenAltName As Integer = 10
        Dim maxLenAttrName As Integer = 10

        '===================
        RA.Load()
        With PM
            Dim CalcMode As ECSynthesisMode = CType(CInt(.Attributes.GetAttributeValue(ATTRIBUTE_RA_SB_SYNTHESIS_MODE_ID, UNDEFINED_USER_ID)), ECSynthesisMode)
            .CalculationsManager.SynthesisMode = CalcMode
            Dim CG As clsCombinedGroup = .CombinedGroups.GetDefaultCombinedGroup
            .CalculationsManager.Calculate(New clsCalculationTarget(CalculationTargetType.cttCombinedGroup, CG), .Hierarchy(.ActiveHierarchy).Nodes(0), .ActiveHierarchy, .ActiveAltsHierarchy)
            For Each alt As clsNode In .AltsHierarchy(.ActiveAltsHierarchy).TerminalNodes
                For Each raAlt In RA.Scenarios.ActiveScenario.Alternatives
                    If alt.NodeGuidID.ToString = raAlt.ID Then raAlt.SBPriority = alt.UnnormalizedPriority
                Next
            Next
        End With
        RA.Scenarios.CheckModel() 'A1324
        'If RA.Solver.SolverState <> raSolverState.raSolved AndAlso RA.Scenarios.GlobalSettings.isAutoSolve Then RA.Solve() ' D3900 'AS/20761p moved down into scenarios loop
        '======================

        With worksheet
            'write columns names
            .Cells(rowColumnHeader, colScenario).Value = "Scenario"
            .Cells(rowColumnHeader, colAltName).Value = "Alt Name"
            .Cells(rowColumnHeader, colFunded).Value = "Funded" 'AS/20761q
            .Cells(rowColumnHeader, colTotal).Value = "Total" 'AS/20761q
            .Cells(rowColumnHeader, colTotalCost).Value = "Total Cost"

            'For Each alt As RAAlternative In RA.Scenarios.ActiveScenario.Alternatives
            '    Dim SBTotal As Double = alt.SBTotal
            '    Debug.Print(SBTotal.ToString)
            'Next

            'For Each scID As Integer In RA.Scenarios.Scenarios.Keys 'AS/20761v-
            Dim ii As Integer = 0 'AS/20761v
            While ii < RA.Scenarios.Scenarios.Count 'AS/20761v
                Dim tID As Integer = RA.Scenarios.Scenarios.Keys(ii)
                Dim sc As RAScenario = RA.Scenarios.Scenarios(tID)

                'If RA.Solver.SolverState <> raSolverState.raSolved AndAlso RA.Scenarios.GlobalSettings.isAutoSolve Then RA.Solve() ' D3900 'AS/20761p moved down into scenarios loop 'AS/20761u
                ' reset solver and solve 'AS/20761u===
                RA.Scenarios.ActiveScenarioID = sc.ID 'AS/20761v
                RA.Solver.ResetSolver()
                RA.Solver.Solve(raSolverExport.raNone) 'AS/20761u=

                Dim coef As Double = getSBCoef(sc) 'AS/22708

                For Each raAlt As RAAlternative In sc.Alternatives
                    .Cells(row, colScenario).Value = sc.Name
                    .Cells(row, colAltName).Value = raAlt.Name
                    If maxLenAltName < Len(raAlt.Name) Then maxLenAltName = Len(raAlt.Name)

                    .Cells(row, colTotalCost).Value = raAlt.Cost
                    If CBool(raAlt.Funded) Then .Cells(row, colFunded).Value = "Yes" 'AS/20761q 
                    '.Cells(row, colTotal).Value = raAlt.SBTotal 'AS/22708
                    .Cells(row, colTotal).Value = raAlt.SBPriority / coef 'AS/22708

                    col = colCustomAttributesFirst

                    'define attributes columns and write values into them
                    For Each attr As clsAttribute In customAttributes
                        Select Case attr.ValueType'AS/18387b===
                            Case AttributeValueTypes.avtEnumeration ', AttributeValueTypes.avtEnumerationMulti 'AS/21169 moved avtEnumerationMulti to new case
                                .Cells.Item(rowColumnHeader, col).Value = attr.Name
                                Dim aEnum As clsAttributeEnumeration = PM.Attributes.GetEnumByID(attr.EnumID) '(mimiced the piece from Public Function AddEnumAttributeItem in CWSw\CoreWS_OperationContracts.vb)
                                If Not (aEnum Is Nothing OrElse attr.EnumID.Equals(Guid.Empty)) Then
                                    Dim itemGUID As Object = PM.Attributes.GetAttributeValue(attr.ID, New Guid(raAlt.ID))
                                    'If Not IsNothing(itemGUID) Then 'enclosed and added Else part in case categorical item not selected and there is no default value 'AS/2-12-2021
                                    If Not IsNothing(itemGUID) AndAlso itemGUID.ToString <> Guid.Empty.ToString Then 'enclosed and added Else part in case categorical item not selected and there is no default value 'AS/2-12-2021
                                        If Not IsNothing(aEnum.GetItemByID(New Guid(itemGUID.ToString))) Then 'AS/25701 enclosed
                                            Dim val As String = aEnum.GetItemByID(New Guid(itemGUID.ToString)).Value
                                            .Cells.Item(row, col).Value = val
                                            If maxLenAttrName < Len(val) Then maxLenAttrName = Len(val)
                                        Else 'AS/25701===
                                            .Cells.Item(row, col).Value = ""
                                        End If 'AS/25701==
                                    Else
                                            .Cells.Item(row, col).Value = ""
                                    End If
                                End If
                                col = col + 1

                            Case AttributeValueTypes.avtEnumerationMulti 'AS/21169 (from function CopyAttributeValuesToText in CWSw\CoreWS_OperationContracts.vb)
                                Dim sValue As String = ""
                                .Cells.Item(rowColumnHeader, col).Value = attr.Name
                                Dim aEnum As clsAttributeEnumeration = PM.Attributes.GetEnumByID(attr.EnumID)
                                Dim tValue As Object = PM.Attributes.GetAttributeValue(attr.ID, New Guid(raAlt.ID))
                                If aEnum IsNot Nothing Then
                                    Dim attrValueEnumIDs() As String = Nothing
                                    If TypeOf tValue Is String Then
                                        attrValueEnumIDs = CStr(tValue).Split(CChar(";"))
                                    End If
                                    If attrValueEnumIDs IsNot Nothing AndAlso attrValueEnumIDs.Count > 0 Then
                                        For Each sID As String In attrValueEnumIDs
                                            Try
                                                Dim ID As Guid = New Guid(sID)
                                                Dim item As clsAttributeEnumerationItem = aEnum.GetItemByID(ID)
                                                If item IsNot Nothing Then
                                                    sValue += CStr(IIf(sValue = "", "", ",")) + item.Value
                                                End If
                                            Catch
                                            End Try
                                        Next
                                    End If
                                    .Cells.Item(row, col).Value = sValue
                                End If

                                'Case Else
                                '    Dim val As Object = PM.Attributes.GetAttributeValue(attr.ID, alt.NodeGuidID)
                                '    If IsNothing(val) Then
                                '        .Cells.Item(row, col).Value = String.Empty
                                '    Else
                                '        .Cells.Item(row, col).Value = val.ToString
                                '        If maxLenAttrName < Len(val.ToString) Then maxLenAttrName = Len(val.ToString)
                                '    End If
                                col = col + 1
                        End Select

                    Next

                    row += 1
                Next 'alternative
                ii += 1 'AS/20761v
            End While 'scenarios count 'AS/20761v

            'freeze top row
            .Panes = New WorksheetPanes(PanesState.Frozen, 0, 1, "A2", PanePosition.TopRight) 'AS/2-12-2021

            'format
            For i = colScenario To col
                .Columns(i).AutoFit()
                .Columns(i).Style.WrapText = True
            Next

            Dim range = .Cells.GetSubrangeAbsolute(1, colTotal, row, colTotal) 'AS/21149a 'AS/20761q
            range.Style.NumberFormat = "0.000"

        End With

        Return worksheet

    End Function

    Private Function AddRA_Vizualizations(Options As ReportGeneratorOptions) As ExcelWorksheet 'AS/20761f
        Dim worksheet = workbook.Worksheets.Add("RA_Vizualizations")

        Dim rowColumnHeader As Integer = 0 'column header
        Dim rowScenarioName As Integer = 1 'AS/20761r

        Dim colScenario As Integer = 0
        Dim colFundedFilter As Integer = 1
        Dim colAttrName As Integer = 2
        Dim colAttrCategory As Integer = 3
        Dim colAltCount As Integer = 4
        Dim colCost As Integer = 5
        Dim colCostPrc As Integer = 6
        Dim colEBenefit As Integer = 7
        Dim colEBenefitPrc As Integer = 8

        Dim row As Integer = 1 'row number 
        Dim col As Integer = 0 'col number


        With worksheet
            'write columns headers
            .Cells(rowColumnHeader, colScenario).Value = "Scenario" 'AS/20761r
            .Cells(rowColumnHeader, colFundedFilter).Value = "Category" 'Funded, Not Funded
            .Cells(rowColumnHeader, colAttrName).Value = "Attribute"
            .Cells(rowColumnHeader, colAttrCategory).Value = "Attribute Item"
            .Cells(rowColumnHeader, colAltCount).Value = "Alt.Count"
            .Cells(rowColumnHeader, colCost).Value = "Cost"
            .Cells(rowColumnHeader, colCostPrc).Value = "Cost, %"
            .Cells(rowColumnHeader, colEBenefit).Value = "E. Benefit"
            .Cells(rowColumnHeader, colEBenefitPrc).Value = "E. Benefit, %"

            Dim AttributesList As New List(Of clsAttribute)
            InitAttributesList(AttributesList)
            RA.Scenarios.CheckModel() 'A1324
            'If RA.Solver.SolverState <> raSolverState.raSolved Then -A0888
            'If RA.Solver.SolverState <> raSolverState.raSolved AndAlso RA.Scenarios.GlobalSettings.isAutoSolve Then RA.Solve() ' D3900
            'If RA.Solver.SolverState <> raSolverState.raSolved OrElse RA.Scenarios.GlobalSettings.isAutoSolve Then RA.Solve() ' D3900 + D4585 'AS/20761r

            '======= below -- used Public Function GetData() in RA\PlotAlternatives as a prototype
            'Dim tSelectedCategoryID As Guid = SelectedCategoryID(AttributesList) 'AS/20761x=== moved down
            'Dim tSelectedCategoryEnum As clsAttributeEnumeration = Nothing
            'Dim tSelectedAttr As clsAttribute = Nothing
            'Dim tSelectedCategoryMultiValues As List(Of String) = New List(Of String) 'AS/20761x==

            Dim Funded As New List(Of FundedFilteringModes) 'AS/20761f===
            Funded.Add(FundedFilteringModes.ffFunded)
            Funded.Add(FundedFilteringModes.ffNotFunded)

            'For Each tID As Integer In RA.Scenarios.Scenarios.Keys 'AS/20761r=== 'AS/20761v-
            Dim ii As Integer = 0 'AS/20761v
            While ii < RA.Scenarios.Scenarios.Count 'AS/20761v
                Dim tID As Integer = RA.Scenarios.Scenarios.Keys(ii)
                Dim sc As RAScenario = RA.Scenarios.Scenarios(tID)

                .Cells(row, colScenario).Value = sc.Name
                rowScenarioName = row
                row += 1

                ' reset solver and solve
                RA.Scenarios.ActiveScenarioID = sc.ID 'AS/20761v
                RA.Solver.ResetSolver() 'AS/20761m
                RA.Solver.Solve(raSolverExport.raNone) 'AS/20761m 'AS/20761r==

                If RA.Solver.SolverState = raSolverState.raSolved Then 'AS/20761v enclosed
                    If AttributesList.Count > 0 Then 'AS/20761w
                        .Columns(colAttrName).Hidden = False 'AS/20761x===
                        .Columns(colAttrCategory).Hidden = False

                        Dim tSelectedCategoryID As Guid = SelectedCategoryID(AttributesList)
                        Dim tSelectedCategoryEnum As clsAttributeEnumeration = Nothing
                        Dim tSelectedAttr As clsAttribute = Nothing
                        Dim tSelectedCategoryMultiValues As List(Of String) = New List(Of String) 'AS/20761x==

                        For Each attr In AttributesList
                            Dim sAttrName As String = attr.Name

                            For Each ff As FundedFilteringModes In Funded
                                Dim sFunded As String = ""
                                Select Case ff
                                    Case FundedFilteringModes.ffFunded
                                        sFunded = "Funded"
                                    Case FundedFilteringModes.ffPartiallyFunded
                                        sFunded = "Partially Funded"
                                    Case FundedFilteringModes.ffFullyFunded
                                        sFunded = "Fully Funded"
                                    Case FundedFilteringModes.ffNotFunded
                                        sFunded = "Not Funded"
                                End Select 'AS/20761f==

                                Dim fCountProjects As Integer = 0
                                Dim fSumVisibleCosts As Double = 0 ' D3913
                                Dim fSumVisibleBenefits As Double = 0
                                Dim dCountProjectsByCategory As New Dictionary(Of String, Integer)
                                Dim dCostsByCategory As New Dictionary(Of String, Double)
                                Dim dBenefitByCategory As New Dictionary(Of String, Double)

                                tSelectedAttr = attr
                                'If (tSelectedCategoryID.Equals(Guid.Empty) OrElse attr.ID.Equals(tSelectedCategoryID)) AndAlso AttributesList.Count > 0 Then tSelectedCategoryID = attr.ID
                                tSelectedCategoryID = attr.ID
                                If tSelectedAttr IsNot Nothing AndAlso Not tSelectedAttr.EnumID.Equals(Guid.Empty) Then tSelectedCategoryEnum = PM.Attributes.GetEnumByID(tSelectedAttr.EnumID)

                                If tSelectedCategoryEnum IsNot Nothing Then
                                    For Each item In tSelectedCategoryEnum.Items
                                        Dim cat As String = CStr(IIf(String.IsNullOrEmpty(item.ID.ToString), Guid.Empty.ToString, item.ID.ToString))
                                        If Not dCountProjectsByCategory.ContainsKey(cat) Then dCountProjectsByCategory.Add(cat, 0)
                                        If Not dCostsByCategory.ContainsKey(cat) Then dCostsByCategory.Add(cat, 0)
                                        If Not dBenefitByCategory.ContainsKey(cat) Then dBenefitByCategory.Add(cat, 0)
                                    Next
                                End If

                                If Not dCountProjectsByCategory.ContainsKey(Guid.Empty.ToString) Then dCountProjectsByCategory.Add(Guid.Empty.ToString, 0)
                                If Not dCostsByCategory.ContainsKey(Guid.Empty.ToString) Then dCostsByCategory.Add(Guid.Empty.ToString, 0)
                                If Not dBenefitByCategory.ContainsKey(Guid.Empty.ToString) Then dBenefitByCategory.Add(Guid.Empty.ToString, 0)

                                Dim tColorLegendVisible As Boolean = False

                                For Each tAlt0 As RAAlternative In sc.AlternativesFull   ' D3837
                                    Dim tAlt0ID As String = tAlt0.ID
                                    Dim tAlt As RAAlternative = sc.Alternatives.Find(Function(fAlt) fAlt.ID = tAlt0ID)
                                    If tAlt IsNot Nothing Then
                                        Dim tAltVisible As Boolean = True
                                        'Select Case ff 'AS/20761f=== 'AS/20761v===
                                        '    Case FundedFilteringModes.ffFunded
                                        '        tAltVisible = RA.Solver.SolverState = raSolverState.raSolved AndAlso tAlt.DisplayFunded > 0
                                        '    Case FundedFilteringModes.ffPartiallyFunded
                                        '        tAltVisible = RA.Solver.SolverState = raSolverState.raSolved AndAlso tAlt.IsPartiallyFunded
                                        '    Case FundedFilteringModes.ffFullyFunded
                                        '        tAltVisible = RA.Solver.SolverState = raSolverState.raSolved AndAlso tAlt.DisplayFunded = 1
                                        '    Case FundedFilteringModes.ffNotFunded
                                        '        tAltVisible = RA.Solver.SolverState = raSolverState.raSolved AndAlso tAlt.DisplayFunded = 0
                                        'End Select 'AS/20761f== 'AS/20761v==

                                        'If raSolverState.raSolved = 1 Then 'AS/20761v
                                        If RA.Solver.SolverState = raSolverState.raSolved Then
                                            Select Case ff 'AS/20761f=== 'AS/20761v===
                                                Case FundedFilteringModes.ffFunded
                                                    tAltVisible = CBool(tAlt.DisplayFunded > 0)
                                                Case FundedFilteringModes.ffPartiallyFunded
                                                    tAltVisible = tAlt.IsPartiallyFunded
                                                Case FundedFilteringModes.ffFullyFunded
                                                    tAltVisible = CBool(tAlt.DisplayFunded = 1)
                                                Case FundedFilteringModes.ffNotFunded
                                                    tAltVisible = CBool(tAlt.DisplayFunded = 0)
                                            End Select 'AS/20761f== 
                                            'Else 'infeasable 'AS/20761v===
                                            '    .Cells(row, colFundedFilter).Value = "Infeasable"
                                            '    row += 1
                                            '    'tAltVisible = False
                                            '    Exit For 'AS/20761v==
                                        End If 'AS/20761v==

                                        If tAltVisible Then 'AS/20761r- 'AS/20761v+
                                            Dim sCategory As String = ""
                                            Dim altGuid As Guid = New Guid(tAlt.ID)
                                            'If AttributesList.Count > 0 AndAlso Not tSelectedCategoryID.Equals(Guid.Empty) Then 'AS/20761w
                                            If Not tSelectedCategoryID.Equals(Guid.Empty) Then 'AS/20761w
                                                tColorLegendVisible = True
                                                Dim mAlt As clsNode = PM.AltsHierarchy(PM.ActiveAltsHierarchy).GetNodeByID(altGuid)
                                                If mAlt IsNot Nothing Then
                                                    Dim attrVal As Object = PM.Attributes.GetAttributeValue(tSelectedCategoryID, altGuid)
                                                    If attrVal IsNot Nothing AndAlso tSelectedCategoryEnum IsNot Nothing AndAlso tSelectedCategoryEnum.Items.Count > 0 Then
                                                        Select Case tSelectedAttr.ValueType
                                                            Case AttributeValueTypes.avtEnumeration
                                                                Dim item As clsAttributeEnumerationItem = Nothing
                                                                If TypeOf attrVal Is String Then item = tSelectedCategoryEnum.GetItemByID(New Guid(CStr(attrVal)))
                                                                If TypeOf attrVal Is Guid Then item = tSelectedCategoryEnum.GetItemByID(CType(attrVal, Guid))
                                                                If item IsNot Nothing Then
                                                                    Dim indx As Integer = tSelectedCategoryEnum.Items.IndexOf(item)
                                                                    If indx >= 0 Then
                                                                        sCategory = item.ID.ToString
                                                                    End If
                                                                End If
                                                            Case AttributeValueTypes.avtEnumerationMulti
                                                                Dim sVal As String = CStr(attrVal)
                                                                If Not String.IsNullOrEmpty(sVal) Then
                                                                    sCategory = sVal
                                                                    If Not tSelectedCategoryMultiValues.Contains(sVal) Then tSelectedCategoryMultiValues.Add(sVal)
                                                                End If
                                                        End Select
                                                    End If
                                                End If
                                            End If

                                            Dim cat As String = CStr(IIf(String.IsNullOrEmpty(sCategory), Guid.Empty.ToString, sCategory))
                                            If Not dCountProjectsByCategory.ContainsKey(cat) Then dCountProjectsByCategory.Add(cat, 0)
                                            If Not dCostsByCategory.ContainsKey(cat) Then dCostsByCategory.Add(cat, 0)
                                            If Not dBenefitByCategory.ContainsKey(cat) Then dBenefitByCategory.Add(cat, 0)

                                            fCountProjects += 1
                                            dCountProjectsByCategory(cat) += 1
                                            If tAlt.Cost <> UNDEFINED_INTEGER_VALUE Then
                                                fSumVisibleCosts += tAlt.Cost
                                                dCostsByCategory(cat) += tAlt.Cost
                                            End If

                                            If tAlt.Benefit <> UNDEFINED_INTEGER_VALUE AndAlso Not Double.IsNaN(tAlt.Benefit) Then
                                                fSumVisibleBenefits += tAlt.Benefit
                                                dBenefitByCategory(cat) += tAlt.Benefit
                                            End If

                                        End If 'AS/20761r
                                    End If
                                Next

                                If tColorLegendVisible Then 'AS/20761r- 'AS/20761v+
                                    Dim index As Integer = 0
                                    Select Case tSelectedAttr.ValueType
                                        Case AttributeValueTypes.avtEnumeration
                                            If tSelectedCategoryEnum IsNot Nothing AndAlso tSelectedCategoryEnum.Items IsNot Nothing Then 'A0899
                                                For Each item As clsAttributeEnumerationItem In tSelectedCategoryEnum.Items
                                                    Dim cat As String = item.ID.ToString
                                                    index += 1

                                                    'Debug.Print(index & "===")
                                                    'Debug.Print("color" & " = " & GetPaletteColor(CurrentPaletteID(PM), index, True))
                                                    'Debug.Print("item.Value" & " = " & JS_SafeString(item.Value))
                                                    'Debug.Print("item.ID" & " = " & JS_SafeString(item.ID.ToString))
                                                    'Debug.Print("dCountProjectsByCategory(cat)" & " = " & JS_SafeNumber(dCountProjectsByCategory(cat)))
                                                    'Debug.Print("dCountProjectsByCategory(cat), fCountProjects)" & " = " & JS_Percent(dCountProjectsByCategory(cat), fCountProjects))
                                                    'Debug.Print("dCostsByCategory(cat)" & " = " & CostString(dCostsByCategory(cat)))
                                                    'Debug.Print("dCostsByCategory(cat), fSumVisibleCosts)" & " = " & JS_Percent(dCostsByCategory(cat), fSumVisibleCosts))
                                                    'Debug.Print("dCostsByCategory(cat)" & " = " & JS_SafeNumber(dCostsByCategory(cat)))
                                                    'Debug.Print("dBenefitByCategory(cat), 4)" & " = " & JS_SafeNumber(Math.Round(dBenefitByCategory(cat), 4)))
                                                    'Debug.Print("dBenefitByCategory(cat), 4), fSumVisibleBenefits)" & " = " & JS_Percent(Math.Round(dBenefitByCategory(cat), 4), fSumVisibleBenefits))
                                                    'Debug.Print(index & "==")

                                                    .Cells(row, colFundedFilter).Value = sFunded 'Funded, Not Funded
                                                    .Cells(row, colAttrName).Value = sAttrName
                                                    .Cells(row, colAttrCategory).Value = JS_SafeString(item.Value)
                                                    .Cells(row, colAltCount).Value = dCountProjectsByCategory(cat)
                                                    .Cells(row, colCost).Value = dCostsByCategory(cat)
                                                    If fSumVisibleCosts > 0 Then 'AS/20761r enclosed
                                                        .Cells(row, colCostPrc).Value = dCostsByCategory(cat) / fSumVisibleCosts
                                                    End If 'AS/20761r
                                                    .Cells(row, colEBenefit).Value = Math.Round(dBenefitByCategory(cat), 4)
                                                    If fSumVisibleBenefits > 0 Then 'AS/20761r enclosed
                                                        .Cells(row, colEBenefitPrc).Value = Math.Round(dBenefitByCategory(cat), 4) / fSumVisibleBenefits
                                                    End If 'AS/20761r
                                                    row += 1
                                                Next
                                            End If
                                        Case AttributeValueTypes.avtEnumerationMulti
                                            If tSelectedCategoryEnum IsNot Nothing AndAlso tSelectedCategoryEnum.Items IsNot Nothing Then 'A0899
                                                For Each s As String In tSelectedCategoryMultiValues
                                                    Dim sVals As String() = s.Split(CType(";", Char()))
                                                    If sVals.Count > 0 Then
                                                        Dim sName As String = ""
                                                        For Each v As String In sVals
                                                            Dim cat_name As String = v
                                                            Dim enum_item As clsAttributeEnumerationItem = tSelectedCategoryEnum.GetItemByID(New Guid(v))
                                                            If enum_item IsNot Nothing Then cat_name = enum_item.Value
                                                            sName += CStr(IIf(sName = "", "", " AND ")) + String.Format("{0}", cat_name)
                                                        Next
                                                    End If
                                                    index += 1
                                                Next
                                            End If
                                    End Select

                                    'include "No Category" items 'AS/20761w===
                                    Dim key = Guid.Empty.ToString
                                    .Cells(row, colFundedFilter).Value = sFunded 'Funded, Not Funded
                                    .Cells(row, colAttrName).Value = sAttrName
                                    .Cells(row, colAttrCategory).Value = ResString("lblNoCategory")
                                    .Cells(row, colAltCount).Value = dCountProjectsByCategory(key)
                                    .Cells(row, colCost).Value = dCostsByCategory(key)
                                    If fSumVisibleCosts > 0 Then
                                        .Cells(row, colCostPrc).Value = dCostsByCategory(key) / fSumVisibleCosts
                                    End If
                                    .Cells(row, colEBenefit).Value = Math.Round(dBenefitByCategory(key), 4)
                                    If fSumVisibleBenefits > 0 Then
                                        .Cells(row, colEBenefitPrc).Value = Math.Round(dBenefitByCategory(key), 4) / fSumVisibleBenefits
                                    End If
                                    row += 1 'AS/20761w==

                                End If 'AS/20761r 'AS/20761v
                            Next '"funded" filter
                        Next 'attribute
                        'Else 'infeaseble or error, actually -- any other than solved 'AS/20761v=== 'AS/20761x===
                        '    .Cells(row, colFundedFilter).Value = "Infeasable"
                        '    row += 1 'AS/20761x==
                    Else 'AS/20761x=== no attributes
                        .Columns(colAttrName).Hidden = True
                        .Columns(colAttrCategory).Hidden = True

                        For Each ff As FundedFilteringModes In Funded
                            Dim sFunded As String = ""
                            Select Case ff
                                Case FundedFilteringModes.ffFunded
                                    sFunded = "Funded"
                                Case FundedFilteringModes.ffPartiallyFunded
                                    sFunded = "Partially Funded"
                                Case FundedFilteringModes.ffFullyFunded
                                    sFunded = "Fully Funded"
                                Case FundedFilteringModes.ffNotFunded
                                    sFunded = "Not Funded"
                            End Select 'AS/20761f==

                            Dim fCountProjects As Integer = 0
                            Dim fSumVisibleCosts As Double = 0 ' D3913
                            Dim fSumVisibleBenefits As Double = 0
                            Dim dCountProjectsByCategory As New Dictionary(Of String, Integer)
                            Dim dCostsByCategory As New Dictionary(Of String, Double)
                            Dim dBenefitByCategory As New Dictionary(Of String, Double)

                            If Not dCountProjectsByCategory.ContainsKey(Guid.Empty.ToString) Then dCountProjectsByCategory.Add(Guid.Empty.ToString, 0)
                            If Not dCostsByCategory.ContainsKey(Guid.Empty.ToString) Then dCostsByCategory.Add(Guid.Empty.ToString, 0)
                            If Not dBenefitByCategory.ContainsKey(Guid.Empty.ToString) Then dBenefitByCategory.Add(Guid.Empty.ToString, 0)

                            Dim tColorLegendVisible As Boolean = False

                            For Each tAlt0 As RAAlternative In sc.AlternativesFull   ' D3837
                                Dim tAlt0ID As String = tAlt0.ID
                                Dim tAlt As RAAlternative = sc.Alternatives.Find(Function(fAlt) fAlt.ID = tAlt0ID)
                                If tAlt IsNot Nothing Then
                                    Dim tAltVisible As Boolean = True
                                    If RA.Solver.SolverState = raSolverState.raSolved Then
                                        Select Case ff 'AS/20761f=== 'AS/20761v===
                                            Case FundedFilteringModes.ffFunded
                                                tAltVisible = CBool(tAlt.DisplayFunded > 0)
                                            Case FundedFilteringModes.ffPartiallyFunded
                                                tAltVisible = tAlt.IsPartiallyFunded
                                            Case FundedFilteringModes.ffFullyFunded
                                                tAltVisible = CBool(tAlt.DisplayFunded = 1)
                                            Case FundedFilteringModes.ffNotFunded
                                                tAltVisible = CBool(tAlt.DisplayFunded = 0)
                                        End Select 'AS/20761f== 
                                    End If 'AS/20761v==

                                    If tAltVisible Then 'AS/20761r- 'AS/20761v+
                                        Dim sCategory As String = ""
                                        Dim altGuid As Guid = New Guid(tAlt.ID)

                                        Dim cat As String = CStr(IIf(String.IsNullOrEmpty(sCategory), Guid.Empty.ToString, sCategory))
                                        If Not dCountProjectsByCategory.ContainsKey(cat) Then dCountProjectsByCategory.Add(cat, 0)
                                        If Not dCostsByCategory.ContainsKey(cat) Then dCostsByCategory.Add(cat, 0)
                                        If Not dBenefitByCategory.ContainsKey(cat) Then dBenefitByCategory.Add(cat, 0)

                                        fCountProjects += 1
                                        dCountProjectsByCategory(cat) += 1
                                        If tAlt.Cost <> UNDEFINED_INTEGER_VALUE Then
                                            fSumVisibleCosts += tAlt.Cost
                                            dCostsByCategory(cat) += tAlt.Cost
                                        End If

                                        If tAlt.Benefit <> UNDEFINED_INTEGER_VALUE AndAlso Not Double.IsNaN(tAlt.Benefit) Then
                                            fSumVisibleBenefits += tAlt.Benefit
                                            dBenefitByCategory(cat) += tAlt.Benefit
                                        End If

                                    End If 'AS/20761r
                                End If
                            Next

                            Dim key = Guid.Empty.ToString
                            .Cells(row, colFundedFilter).Value = sFunded 'Funded, Not Funded
                            '.Cells(row, colAttrName).Value = ""
                            '.Cells(row, colAttrCategory).Value = ""
                            .Cells(row, colAltCount).Value = dCountProjectsByCategory(key)
                            .Cells(row, colCost).Value = dCostsByCategory(key)
                            If fSumVisibleCosts > 0 Then
                                .Cells(row, colCostPrc).Value = dCostsByCategory(key) / fSumVisibleCosts
                            End If
                            .Cells(row, colEBenefit).Value = Math.Round(dBenefitByCategory(key), 4)
                            If fSumVisibleBenefits > 0 Then
                                .Cells(row, colEBenefitPrc).Value = Math.Round(dBenefitByCategory(key), 4) / fSumVisibleBenefits
                            End If
                            row += 1 'AS/20761w==
                        Next '"funded" filter 'AS/20761x==
                    End If 'AS/20761v==
                End If 'AS/20761w If AttributesList.Count > 0 
                ii += 1 'AS/20761v
            End While 'scenarios count 'AS/20761v
            '==============================================================================

            'freeze top row
            .Panes = New WorksheetPanes(PanesState.Frozen, 0, 1, "A2", PanePosition.TopRight) 'AS/19486_g

            'format
            .Columns(colCostPrc).Style.NumberFormat = "0.0%"
            .Columns(colEBenefitPrc).Style.NumberFormat = "0.0%"

            For i = colScenario To colEBenefitPrc 'AS/20761r
                .Columns(i).AutoFit()
            Next
        End With

        Return worksheet
    End Function

    Private Function AddRA_ScenarioComparisons(Options As ReportGeneratorOptions) As ExcelWorksheet 'AS/20761g
        Dim worksheet = workbook.Worksheets.Add("RA_ScenarioComparisons")

        Dim rowScenarioNames As Integer = 0
        Dim rowDescription As Integer = 1
        Dim rowBudget As Integer = 2
        Dim rowEffectivePrc As Integer = 3
        Dim rowFundedCost As Integer = 4
        Dim rowSolverState As Integer = 5
        Dim rowColumnHeaders As Integer = 6

        Dim colNumber As Integer = 0
        Dim colName As Integer = 1
        Dim colFirstScenario As Integer = 2
        Dim colsScenarios As New List(Of Integer) 'indexes of the columns

        Dim row As Integer = 1 'row number 
        Dim col As Integer = 0 'col number

        With worksheet
            .Cells(rowScenarioNames, colName).Value = "Scenario Name:"
            .Cells(rowDescription, colName).Value = "Description:"
            .Cells(rowBudget, colName).Value = "Budget:"
            .Cells(rowEffectivePrc, colName).Value = "% Effective:"
            .Cells(rowFundedCost, colName).Value = "Funded Cost:"
            .Cells(rowSolverState, colName).Value = "Solver State:"
            .Cells(rowColumnHeaders, colNumber).Value = "No." 'alt ID
            .Cells(rowColumnHeaders, colName).Value = "Alternative name"
            .Cells(rowColumnHeaders, colFirstScenario).Value = "Funded"

            col = colFirstScenario

            currentScenarioID = RA.Scenarios.ActiveScenarioID 'AS/20761h

            Dim ii As Integer = 0
            While ii < RA.Scenarios.Scenarios.Count
                Dim tID As Integer = RA.Scenarios.Scenarios.Keys(ii)
                Dim sc As RAScenario = RA.Scenarios.Scenarios(tID)

                RA.Scenarios.ActiveScenarioID = sc.ID
                RA.Solver.ResetSolver()
                RA.Solve()

                .Cells(rowScenarioNames, col).Value = sc.Name
                .Cells(rowDescription, col).Value = sc.Description
                .Cells(rowBudget, col).Value = sc.Budget
                .Cells(rowEffectivePrc, col).Value = RA.Solver.FundedBenefit / RA.Solver.BaseCaseMaximum
                .Cells(rowFundedCost, col).Value = RA.Solver.FundedCost
                .Cells(rowSolverState, col).Value = SolverStateString(RA.Solver)

                row = rowColumnHeaders + 1
                For Each raAlt As RAAlternative In sc.Alternatives
                    .Cells(row, colNumber).Value = row - rowColumnHeaders 'getAlternativeIDbyGUID(raAlt.ID)
                    .Cells(row, colName).Value = raAlt.Name
                    If CBool(raAlt.Funded) Then .Cells(row, col).Value = "Yes"
                    row += 1
                Next 'alternative
                col += 1
                ii += 1
            End While 'scenarios count

            RA.Scenarios.ActiveScenarioID = currentScenarioID 'AS/20761h

            'freeze top row
            .Panes = New WorksheetPanes(PanesState.Frozen, 0, 1, "A2", PanePosition.TopRight)

            'format
            Dim cellRange As CellRange = .Cells.GetSubrangeAbsolute(rowScenarioNames, colName, rowSolverState, colName)
            cellRange.Style.HorizontalAlignment = HorizontalAlignmentStyle.Right

            cellRange = .Cells.GetSubrangeAbsolute(rowColumnHeaders, colFirstScenario, rowColumnHeaders, col)
            cellRange.Merged = True
            cellRange.Style.HorizontalAlignment = HorizontalAlignmentStyle.Center

            cellRange = .Cells.GetSubrangeAbsolute(rowEffectivePrc, colFirstScenario, rowEffectivePrc, col)
            cellRange.Style.NumberFormat = "0.0%"

            cellRange = .Cells.GetSubrangeAbsolute(rowBudget, colFirstScenario, rowBudget, col)
            cellRange.Style.NumberFormat = NumberFormatBuilder.Number(0, useThousandsSeparator:=True)

            cellRange = .Cells.GetSubrangeAbsolute(rowFundedCost, colFirstScenario, rowFundedCost, col)
            cellRange.Style.NumberFormat = NumberFormatBuilder.Number(0, useThousandsSeparator:=True)


            For i = colNumber To col
                .Columns(i).AutoFit()
            Next

            'sort by No
            .Cells.GetSubrangeAbsolute(rowColumnHeaders + 1, colNumber, row - -1, col).Sort(False).By(colNumber, True).Apply()

        End With
        Return worksheet

    End Function

    Private Function AddRA_ModelSpecification(Options As ReportGeneratorOptions) As ExcelWorksheet 'AS/20761h
        'see Private Sub GetReportData(reportID As RAReportIDs) in Reports.aspx.vb

        Dim worksheet = workbook.Worksheets.Add("RA_ModelSpecification")

        Dim rowScenarioName As Integer = 0
        Dim rowConstraintsNames As Integer = 1
        Dim rowConstraintsDefined As Integer = 2
        Dim rowConstraintsIgnore As Integer = 3
        Dim rowBaseCaseInclude As Integer = 4
        Dim rowMaxBenefits As Integer = 6
        Dim rowBudget As Integer = 8
        Dim rowMusts As Integer = 10


        Dim colNames As Integer = 0
        Dim colMusts As Integer = 1
        Dim colMustNots As Integer = 2
        Dim colCustomConstraints As Integer = 3
        Dim colDependencies As Integer = 4
        Dim colGroups As Integer = 5
        Dim colFundingPools As Integer = 6
        Dim colRisks As Integer = 7

        Dim row As Integer = 1 'row number 
        Dim col As Integer = 0

        'Dim sc As RAScenario = RA.Scenarios.ActiveScenario
        Dim sc As RAScenario = RA.Scenarios.Scenarios(currentScenarioID)
        RA.Solver.ResetSolver()
        RA.Solve()

        Dim lstMergedRows As New List(Of Integer)
        Dim Alts As IEnumerable(Of RAAlternative) = sc.Alternatives.OrderBy(Function(alt) alt.SortOrder)
        Dim sDecimals As String = "F" + CStr(DecimalDigits)
        Dim sValue As String = ""

        ' reset solver
        'RA.Solver.ResetSolver()
        'RA.Solver.Solve(raSolverExport.raNone)

        'RA.Scenarios.ActiveScenarioID = sc.ID
        'RA.Solver.ResetSolver()
        'RA.Solve()

        With worksheet

            'title -- active scenario name
            .Cells(rowScenarioName, colNames).Value = sc.Name
            .Cells(rowScenarioName, colNames).Style.HorizontalAlignment = HorizontalAlignmentStyle.Center
            lstMergedRows.Add(rowScenarioName)

            'constraints names
            row = rowConstraintsNames
            .Cells(row, colMusts).Value = "Musts"
            .Cells(row, colMustNots).Value = "Must Nots"
            .Cells(row, colCustomConstraints).Value = "Custom Constraints"
            .Cells(row, colDependencies).Value = "Dependencies"
            .Cells(row, colGroups).Value = "Groups"
            .Cells(row, colFundingPools).Value = "Funding Pools"
            .Cells(row, colRisks).Value = "Risks"
            .Rows(row).Style.WrapText = True

            'params - defined
            row = rowConstraintsDefined
            .Cells(row, colNames).Value = "Defined:"
            .Cells(row, colMusts).Value = IIf(sc.Alternatives.Sum(Function(a) CInt(IIf(a.Must, 1, 0))) > 0, 1, 0)
            .Cells(row, colMustNots).Value = IIf(sc.Alternatives.Sum(Function(a) CInt(IIf(a.MustNot, 1, 0))) > 0, 1, 0)
            .Cells(row, colCustomConstraints).Value = IIf(sc.Constraints.Constraints.Count > 0, 1, 0)
            .Cells(row, colDependencies).Value = IIf(sc.Dependencies.Dependencies.Count > 0, 1, 0)
            .Cells(row, colGroups).Value = IIf(sc.Groups.Groups.Count > 0, 1, 0)
            .Cells(row, colFundingPools).Value = IIf(sc.FundingPools.Pools.Count > 0, 1, 0)
            .Cells(row, colRisks).Value = IIf(sc.Alternatives.Sum(Function(a) a.RiskOriginal) > 0, 1, 0)

            'params - ignore
            row = rowConstraintsIgnore
            .Cells(row, colNames).Value = "Ignore:"
            .Cells(row, colMusts).Value = IIf(Not sc.Settings.Musts, 1, 0)
            .Cells(row, colMustNots).Value = IIf(Not sc.Settings.MustNots, 1, 0)
            .Cells(row, colCustomConstraints).Value = IIf(Not sc.Settings.CustomConstraints, 1, 0)
            .Cells(row, colDependencies).Value = IIf(Not sc.Settings.Dependencies, 1, 0)
            .Cells(row, colGroups).Value = IIf(Not sc.Settings.Groups, 1, 0)
            .Cells(row, colFundingPools).Value = IIf(Not sc.Settings.FundingPools, 1, 0)
            .Cells(row, colRisks).Value = IIf(Not sc.Settings.Risks, 1, 0)

            'params - base case
            row = rowBaseCaseInclude
            .Cells(row, colNames).Value = "Base Case Includes:"
            .Cells(row, colMusts).Value = IIf(sc.Settings.UseBaseCase AndAlso sc.Settings.BaseCaseForMusts AndAlso False, 1, 0)
            .Cells(row, colMustNots).Value = IIf(sc.Settings.UseBaseCase AndAlso sc.Settings.BaseCaseForMustNots AndAlso False, 1, 0)
            .Cells(row, colCustomConstraints).Value = IIf(sc.Settings.UseBaseCase AndAlso sc.Settings.BaseCaseForConstraints AndAlso False, 1, 0)
            .Cells(row, colDependencies).Value = IIf(sc.Settings.UseBaseCase AndAlso sc.Settings.BaseCaseForDependencies AndAlso False, 1, 0)
            .Cells(row, colGroups).Value = IIf(sc.Settings.UseBaseCase AndAlso sc.Settings.BaseCaseForGroups, 1, 0)
            .Cells(row, colFundingPools).Value = IIf(sc.Settings.UseBaseCase AndAlso sc.Settings.BaseCaseForFundingPools AndAlso False, 1, 0)
            .Cells(row, colRisks).Value = 0 'Risks - IIf(.Settings.UseBaseCase AndAlso .Settings.BaseCaseForRisks AndAlso False, 1, 0)

            'maximize benefits
            row = rowMaxBenefits
            .Cells(row, colNames).Value = "Maximaize Benefits"
            row += 1
            sValue = String.Join(" + ", (From alt As RAAlternative In sc.Alternatives Select CDbl(IIf(sc.Settings.Risks, alt.Benefit, alt.BenefitOriginal)).ToString(sDecimals) + " * " + REPORT_ALT_PREFIX + alt.SortOrder.ToString).ToArray)
            .Cells(row, colNames).Value = sValue
            lstMergedRows.Add(row)

            'budget
            row = rowBudget
            .Cells(row, colNames).Value = "BudgetConstraint"
            row += 1
            .Cells(row, colNames).Value = "<=" & CostString(sc.Budget)

            'musts
            If sc.Settings.Musts Then
                row = rowMusts
                .Cells(row, colNames).Value = "Musts"
                For Each tAlt As RAAlternative In Alts.Where(Function(alt) alt.Must)
                    row += 1
                    .Cells(row, colNames).Value = GetShortAltName(tAlt)
                    lstMergedRows.Add(row)
                Next
            End If

            'must nots
            If sc.Settings.MustNots Then
                row += 1
                .Cells(row, colNames).Value = "Must Nots"
                For Each tAlt As RAAlternative In Alts.Where(Function(alt) alt.MustNot)
                    row += 1
                    .Cells(row, colNames).Value = GetShortAltName(tAlt)
                    lstMergedRows.Add(row)
                Next
            End If

            'custom constraints
            If sc.Settings.CustomConstraints Then
                row += 1
                .Cells(row, colNames).Value = "Custom Constraints"
                For Each kvp As KeyValuePair(Of Integer, RAConstraint) In sc.Constraints.Constraints
                    Dim cc As RAConstraint = kvp.Value
                    Dim i As Integer = 0
                    While i < cc.AlternativesData.Count
                        Dim alt As String = cc.AlternativesData.Keys(i)
                        If sc.GetAvailableAlternativeById(alt) Is Nothing Then
                            cc.AlternativesData.Remove(alt)
                        Else
                            i += 1
                        End If
                    End While

                    Dim sNotConstrained As String = CStr(IIf(cc.MinValue = UNDEFINED_INTEGER_VALUE AndAlso cc.MaxValue = UNDEFINED_INTEGER_VALUE, "Not CONSTRAINED", ""))
                    If Not cc.Enabled Then sNotConstrained = "SOFT CONSTRAINT"
                    If sNotConstrained = "" Then
                        row += 1
                        sValue = String.Format("{0} {1} {2}" + CStr(IIf(sNotConstrained = "", "", " - " + sNotConstrained)), String.Join(" + ", (From alt As KeyValuePair(Of String, Double) In cc.AlternativesData Select alt.Value.ToString(sDecimals) + " * " + REPORT_ALT_PREFIX + sc.GetAvailableAlternativeById(alt.Key).SortOrder.ToString).ToArray), IIf(cc.MinValue <> UNDEFINED_INTEGER_VALUE, ">= " + cc.MinValue.ToString("F2"), ""), IIf(cc.MaxValue <> UNDEFINED_INTEGER_VALUE, "<= " + cc.MaxValue.ToString("F2"), ""))
                        .Cells(row, colNames).Value = Chr(34) & cc.Name & Chr(34) & ": " & sValue
                        lstMergedRows.Add(row)
                    End If
                Next
            End If

            'dependencies
            If sc.Settings.Dependencies Then
                row += 1
                .Cells(row, colNames).Value = "Dependencies"
                For Each dep As RADependency In sc.Dependencies.Dependencies
                    Dim alt0 As RAAlternative = sc.GetAvailableAlternativeById(dep.FirstAlternativeID)
                    Dim alt1 As RAAlternative = sc.GetAvailableAlternativeById(dep.SecondAlternativeID)
                    If alt0 IsNot Nothing AndAlso alt1 IsNot Nothing Then
                        Dim alt0name As String = REPORT_ALT_PREFIX + alt0.SortOrder.ToString
                        Dim alt1name As String = REPORT_ALT_PREFIX + alt1.SortOrder.ToString
                        row += 1
                        sValue = CStr(IIf(dep.Value = RADependencyType.dtDependsOn, String.Format("{0} {1} {2}", alt0name, ResString("lblDependsOn"), alt1name), String.Format("{0} {1} {2} {3} {4}", alt0name, ResString("lblAnd"), alt1name, ResString("lblAre"), IIf(dep.Value = RADependencyType.dtMutuallyDependent, ResString("lblMutuallyDependent"), ResString("lblMutuallyExclusive")))))
                        .Cells(row, colNames).Value = sValue
                        lstMergedRows.Add(row)

                    End If
                Next
            End If

            'groups
            If sc.Settings.Groups Then
                row += 1
                .Cells(row, colNames).Value = "Groups"
                For Each kvp As KeyValuePair(Of String, RAGroup) In sc.Groups.Groups
                    Dim grp As RAGroup = kvp.Value
                    row += 1
                    sValue = String.Join(", ", (From alt As KeyValuePair(Of String, RAAlternative) In grp.Alternatives Select REPORT_ALT_PREFIX + alt.Value.SortOrder.ToString).ToArray) 'alt.Value.Name
                    .Cells(row, colNames).Value = Chr(34) & grp.Name & Chr(34) & ": " & CStr(IIf(grp.Condition = RAGroupCondition.gcEqualsOne, ResString("lblRAGroupEq1"), IIf(grp.Condition = RAGroupCondition.gcGreaterOrEqualsOne, ResString("lblRAGroupMoreOrEq1"), ResString("lblRAGroupLessOrEq1")))) + ": " & sValue
                    lstMergedRows.Add(row)
                Next
            End If

            'funding pools
            If sc.Settings.FundingPools Then
                row += 1
                .Cells(row, colNames).Value = "Funding Pools"
                For Each kvp As KeyValuePair(Of Integer, RAFundingPool) In sc.FundingPools.Pools
                    Dim fp As RAFundingPool = kvp.Value

                    Dim i As Integer = 0
                    While i < fp.Values.Count
                        Dim alt As String = fp.Values.Keys(i)
                        If sc.GetAvailableAlternativeById(alt) Is Nothing Then
                            fp.Values.Remove(alt)
                        Else
                            i += 1
                        End If
                    End While

                    Dim sNotConstrained As String = CStr(IIf(fp.PoolLimit <= 0, "NOT CONSTRAINED", ""))
                    If sNotConstrained = "" Then
                        row += 1
                        sValue = String.Format("{0}{1}", String.Join(" + ", (From alt As KeyValuePair(Of String, Double) In fp.Values Select CostString(CDbl(alt.Value)) + " * """ + REPORT_ALT_PREFIX + sc.GetAvailableAlternativeById(alt.Key).SortOrder.ToString + """").ToArray), CStr(IIf(sNotConstrained = "", " <= " + CostString(fp.PoolLimit), " - " + sNotConstrained)))
                        .Cells(row, colNames).Value = Chr(34) & fp.Name & Chr(34) & ": " & sValue
                        lstMergedRows.Add(row)
                    End If
                Next
            End If

            'do merge where appropriate before inserting alts legend
            Dim cellRange As CellRange
            For Each r As Integer In lstMergedRows
                cellRange = .Cells.GetSubrangeAbsolute(r, colNames, r, colRisks)
                cellRange.Merged = True
                cellRange.Style.WrapText = True
            Next

            'alts legend
            row += 1
            .Cells(row, colNames).Value = "Alternatives Legend"
            For Each tAlt As RAAlternative In Alts
                row += 1
                .Cells(row, colNames).Value = GetShortAltName(tAlt)
                .Cells(row, colMusts).Value = tAlt.Name
            Next
            '=================================================

            '.Columns(colNames).AutoFit()
            Dim W As Double = .Columns(colNames).GetWidth(LengthUnit.Point)
            .Columns(colNames).SetWidth(W * 3, LengthUnit.Point)

        End With

        Return worksheet

    End Function

    Private Function AddRA_RelevantConstraints(Options As ReportGeneratorOptions) As ExcelWorksheet 'AS/20761i
        'see Private Sub GetReportData(reportID As RAReportIDs) in Reports.aspx.vb as a prototype

        Dim worksheet = workbook.Worksheets.Add("RA_RelevantConstraints")

        Dim rowTitle As Integer = 0
        Dim rowScenarioName As Integer = 1
        Dim rowRelevantsBegin As Integer = 3
        Dim rowRelevantsText As Integer = 4

        Dim colNames As Integer = 0
        Dim colRelevancies As Integer = 1

        Dim row As Integer = 0 'row number 
        Dim col As Integer = 0

        Dim sc As RAScenario = RA.Scenarios.Scenarios(currentScenarioID)
        RA.Solver.ResetSolver()
        RA.Solve()

        Dim lstMergedRows As New List(Of Integer)
        Dim Alts As IEnumerable(Of RAAlternative) = sc.Alternatives.OrderBy(Function(alt) alt.SortOrder)
        Dim sDecimals As String = "F" + CStr(DecimalDigits)
        Dim sValue As String = ""

        Dim lstRelevancies As New List(Of Tuple(Of Boolean, String, String))

        With worksheet

            'titles 
            .Cells(rowTitle, colNames).Value = "Relevancies in This Solution"
            lstMergedRows.Add(rowTitle)
            .Cells(rowTitle, colNames).Style.HorizontalAlignment = HorizontalAlignmentStyle.Center

            .Cells(rowScenarioName, colNames).Value = sc.Name
            .Cells(rowScenarioName, colNames).Style.HorizontalAlignment = HorizontalAlignmentStyle.Center
            lstMergedRows.Add(rowScenarioName)

            'relevancies
            row = rowRelevantsBegin
            .Cells(row, colNames).Value = "The following constraints are: RELEVANT"
            lstMergedRows.Add(row)
            row = rowRelevantsText
            .Cells(row, colNames).Value = "If a constraint were removed (or modified) the current solution would change"
            lstMergedRows.Add(row)

            'get a copy of active scenario
            Dim tmpActiveScenario As Integer = RA.Scenarios.ActiveScenarioID
            Dim CopyScenario As RAScenario = RA.Scenarios.AddScenario(RA.Scenarios.ActiveScenarioID)
            RA.Scenarios.ActiveScenarioID = CopyScenario.ID

            ' get relevant budget
            Dim dRelevanBudget As Double = RA.Solver.GetMaxRelevantBudget()

            ' reset solver and solve
            RA.Solver.ResetSolver()
            RA.Solver.Solve(raSolverExport.raNone)

            ' write budget constraint
            Dim isBudgetRelevant As Boolean = RA.Solver.SolverState = raSolverState.raSolved AndAlso CopyScenario.Budget < dRelevanBudget
            AddRelevancyData("Total budget of " + CostString(CopyScenario.Budget) + CStr(IIf(isBudgetRelevant, " is relevant. ", "")) + CStr(IIf(isBudgetRelevant, "It would remain relevant until increased by " + CostString(dRelevanBudget - CopyScenario.Budget) + " (up to: " + CostString(dRelevanBudget) + ").", " is irrelevant.")), "", isBudgetRelevant, lstRelevancies)

            If RA.Solver.SolverState = raSolverState.raSolved Then
                With CopyScenario
                    Dim origAltsFunded As List(Of Double) = getFundedList(.Alternatives)
                    'Musts
                    If .Settings.Musts Then
                        For Each tAlt As RAAlternative In .Alternatives
                            If tAlt.Must Then
                                tAlt.Must = False

                                RA.Solver.ResetSolver()
                                RA.Solver.Solve(raSolverExport.raNone)

                                tAlt.Must = True

                                Dim curAltsFunded As List(Of Double) = getFundedList(.Alternatives)
                                AddRelevancyData(ParseString("Must for alternative"), GetShortAltName(tAlt), Not Equal(origAltsFunded, curAltsFunded), lstRelevancies)

                            End If
                        Next
                    End If

                    'Must nots
                    If .Settings.MustNots Then
                        For Each tAlt As RAAlternative In .Alternatives
                            If tAlt.MustNot Then
                                tAlt.MustNot = False

                                RA.Solver.ResetSolver()
                                RA.Solver.Solve(raSolverExport.raNone)

                                tAlt.MustNot = True

                                Dim curAltsFunded As List(Of Double) = getFundedList(.Alternatives)
                                AddRelevancyData(ParseString("Must-not for alternative"), GetShortAltName(tAlt), Not Equal(origAltsFunded, curAltsFunded), lstRelevancies)
                            End If
                        Next
                    End If

                    'Custom Constraints
                    If .Settings.CustomConstraints Then
                        For Each Constraint As RAConstraint In .Constraints.Constraints.Values
                            If Constraint.Enabled AndAlso (Constraint.MinValueSet OrElse Constraint.MaxValueSet) Then
                                Constraint.Enabled = False

                                RA.Solver.ResetSolver()
                                RA.Solver.Solve(raSolverExport.raNone)

                                Constraint.Enabled = True

                                Dim curAltsFunded As List(Of Double) = getFundedList(.Alternatives)
                                AddRelevancyData("Custom Constraint", """" + Constraint.Name + """", Not Equal(origAltsFunded, curAltsFunded), lstRelevancies)
                            End If
                        Next
                    End If

                    'Dependencies
                    If .Settings.Dependencies Then
                        .CheckDependencies(.Dependencies)
                        .CheckDependencies(.TimePeriodsDependencies)

                        Dim j As Integer = 0
                        While j < .Dependencies.Dependencies.Count
                            Dim d1 = .Dependencies.Dependencies(j)
                            Dim dep = d1.Value
                            If dep = RADependencyType.dtMutuallyDependent OrElse dep = RADependencyType.dtMutuallyExclusive Then
                                Dim k As Integer = j + 1
                                While k < .Dependencies.Dependencies.Count
                                    Dim d2 = .Dependencies.Dependencies(k)
                                    If d2.SecondAlternativeID = d1.FirstAlternativeID AndAlso d2.FirstAlternativeID = d1.SecondAlternativeID AndAlso d2.Value = d1.Value Then
                                        .Dependencies.Dependencies.RemoveAt(k)
                                    Else
                                        k += 1
                                    End If
                                End While
                            End If
                            j += 1
                        End While

                        Dim isTP As Boolean = isTimePeriodsVisible()
                        Dim tpDep As RADependency = Nothing

                        For i As Integer = 0 To .Dependencies.Dependencies.Count - 1
                            Dim dep As RADependency = .Dependencies.Dependencies(i)
                            If dep.Enabled Then
                                dep.Enabled = False
                                tpDep = Nothing
                                tpDep = .TimePeriodsDependencies.GetDependency(dep.FirstAlternativeID, dep.SecondAlternativeID)
                                If tpDep IsNot Nothing AndAlso tpDep.Enabled Then
                                    tpDep.Enabled = False
                                Else
                                    tpDep = Nothing
                                End If

                                RA.Solver.ResetSolver()
                                RA.Solver.Solve(raSolverExport.raNone)  ' D3236

                                dep.Enabled = True
                                If tpDep IsNot Nothing Then
                                    tpDep.Enabled = True
                                End If

                                If Not isTP Then tpDep = Nothing ' to skip TP dependency name in the report

                                Dim curAltsFunded As List(Of Double) = getFundedList(.Alternatives)
                                AddRelevancyData("Dependency", GetDependencyName(dep, tpDep, isTP), Not Equal(origAltsFunded, curAltsFunded), lstRelevancies)
                            End If
                        Next

                    End If

                    'Groups
                    If .Settings.Groups Then
                        For Each grp As RAGroup In .Groups.Groups.Values
                            If grp.Enabled Then
                                grp.Enabled = False

                                RA.Solver.ResetSolver()
                                RA.Solver.Solve(raSolverExport.raNone) ' D3236

                                grp.Enabled = True

                                Dim curAltsFunded As List(Of Double) = getFundedList(.Alternatives)
                                AddRelevancyData("Group", GetGroupName(grp), Not Equal(origAltsFunded, curAltsFunded), lstRelevancies)
                            End If
                        Next
                    End If

                    'Funding Pools
                    If .Settings.FundingPools Then
                        For Each pool As RAFundingPool In .FundingPools.Pools.Values
                            If pool.Enabled Then
                                pool.Enabled = False

                                RA.Solver.ResetSolver()
                                RA.Solver.Solve(raSolverExport.raNone) ' D3236

                                pool.Enabled = True

                                Dim curAltsFunded As List(Of Double) = getFundedList(.Alternatives)
                                AddRelevancyData("Funding Pool", pool.Name, Not Equal(origAltsFunded, curAltsFunded), lstRelevancies)
                            End If
                        Next
                    End If

                End With
            End If

            RA.Scenarios.DeleteScenario(CopyScenario.ID)
            RA.Scenarios.ActiveScenarioID = tmpActiveScenario

            'write relevant constraints
            For Each tuple In lstRelevancies
                If tuple.Item1 Then
                    row += 1
                    If tuple.Item3 = "" Then
                        .Cells(row, colNames).Value = tuple.Item2
                    Else
                        .Cells(row, colNames).Value = tuple.Item2 & ": " & tuple.Item3
                    End If
                    lstMergedRows.Add(row)
                End If
            Next

            'write irrelevant constraints
            row += 2
            .Cells(row, colRelevancies).Value = "The following constraints are: IRRELEVANT"
            lstMergedRows.Add(row)
            row += 1
            .Cells(row, colRelevancies).Value = "If a constraint were removed, the current solution would not change"
            lstMergedRows.Add(row)

            For Each tuple In lstRelevancies
                If Not tuple.Item1 Then
                    row += 1
                    If tuple.Item3 = "" Then
                        .Cells(row, colRelevancies).Value = tuple.Item2
                    Else
                        .Cells(row, colRelevancies).Value = tuple.Item2 & ": " & tuple.Item3
                    End If
                    lstMergedRows.Add(row)
                End If
            Next

            'do merge where appropriate before inserting alts legend
            Dim cellRange As CellRange
            For Each r As Integer In lstMergedRows
                cellRange = .Cells.GetSubrangeAbsolute(r, colNames, r, colRelevancies)
                cellRange.Merged = True
                cellRange.Style.WrapText = True
            Next

            'alts legend
            row += 2
            .Cells(row, colNames).Value = "Alternatives Legend"
            For Each tAlt As RAAlternative In Alts
                row += 1
                .Cells(row, colNames).Value = GetShortAltName(tAlt)
                .Cells(row, colRelevancies).Value = tAlt.Name
            Next

            Dim W As Double = .Columns(colRelevancies).GetWidth(LengthUnit.Point)
            .Columns(colRelevancies).SetWidth(W * 7, LengthUnit.Point)

        End With

        Return worksheet

    End Function

#End Region

#Region "Comparion Reports"
    Function GenerateExcelReport_DEBUG(filePath As String, server As HttpServerUtility, Options As ReportGeneratorOptions) As ExcelFile 'AS/18712d

        If Options Is Nothing Then
            Options = New ReportGeneratorOptions
            Options.ReportTitle = PM.ProjectName
        End If

        Dim startTime As DateTime = Now
        Dim startElapsedTime As DateTime = Now

        'PrepareUsersData(UserIDs) '

        Dim sTimePrep As String = "Data Preparation Timing" & vbCrLf & vbCrLf

        startTime = Now
        objH = App.ActiveProject.HierarchyObjectives
        Debug.Print("objH: " & (Math.Round((Now - startTime).TotalSeconds, 2).ToString) & " sec")
        sTimePrep = sTimePrep & "objH: " & Math.Round((Now - startTime).TotalSeconds, 2).ToString & " sec" & vbCrLf

        wrtNode = objH.Nodes(0) 'AS/19486t

        startTime = Now
        altH = PM.AltsHierarchy(PM.ActiveAltsHierarchy)
        Debug.Print("altH: " & (Math.Round((Now - startTime).TotalSeconds, 2).ToString) & " sec")
        sTimePrep = sTimePrep & "altH: " & Math.Round((Now - startTime).TotalSeconds, 2).ToString & " sec" & vbCrLf

        startTime = Now
        PM.CalculationsManager.ShowDueToPriorities = Not PM.Parameters.ShowLikelihoodsGivenSources
        Debug.Print("ShowDueToPriorities: " & (Math.Round((Now - startTime).TotalSeconds, 2).ToString) & " sec")
        sTimePrep = sTimePrep & "ShowDueToPriorities: " & Math.Round((Now - startTime).TotalSeconds, 2).ToString & " sec" & vbCrLf

        startTime = Now
        users = getAllUsersAndGroupsIDs()
        Debug.Print("getAllUsersAndGroupsIDs: " & (Math.Round((Now - startTime).TotalSeconds, 2).ToString) & " sec")
        sTimePrep = sTimePrep & "getAllUsersAndGroupsIDs: " & Math.Round((Now - startTime).TotalSeconds, 2).ToString & " sec" & vbCrLf

        startTime = Now
        PM.CalculationsManager.GetAlternativesGrid(wrtNode.NodeID, users, objs, alts) 'AS/19486m==
        Debug.Print("GetAlternativesGrid: " & (Math.Round((Now - startTime).TotalSeconds, 2).ToString) & " sec")
        sTimePrep = sTimePrep & "GetAlternativesGrid: " & Math.Round((Now - startTime).TotalSeconds, 2).ToString & " sec" & vbCrLf

        Debug.Print("Data prep Elapsed time: " & (Math.Round((Now - startElapsedTime).TotalSeconds, 2).ToString) & " sec")
        sTimePrep = sTimePrep & "Data prep Elapsed time: " & (Math.Round((Now - startElapsedTime).TotalSeconds, 2).ToString) & " sec"

        Dim worksheet As ExcelWorksheet
        startElapsedTime = Now
        Dim sTimeReport As String = "Individual Reports Timing" & vbCrLf & vbCrLf '"Individual Reports Timing for :" & PM.ProjectName & vbCrLf & vbCrLf

        startTime = Now
        worksheet = AddOverview(Options)
        Debug.Print("1 Overview: " & (Math.Round((Now - startTime).TotalSeconds, 2).ToString) & " sec")
        sTimeReport = sTimeReport & "1 Overview: " & Math.Round((Now - startTime).TotalSeconds, 2).ToString & " sec" & vbCrLf

        startTime = Now
        If altH.Nodes.Count > 0 Then 'AS/19486q enclosed
            worksheet = AddAlternatives(Options, server)
            Debug.Print("2 Alternatives: " & (Math.Round((Now - startTime).TotalSeconds, 2).ToString) & " sec")
            sTimeReport = sTimeReport & "2 Alternatives: " & Math.Round((Now - startTime).TotalSeconds, 2).ToString & " sec" & vbCrLf
        Else
            sTimeReport = sTimeReport & "2 Alternatives: 0 (no alternatives in the model)" & vbCrLf
        End If 'AS/19486q

        startTime = Now
        worksheet = AddObjectives(Options, server)
        Debug.Print("3 Objectives: " & (Math.Round((Now - startTime).TotalSeconds, 2).ToString) & " sec")
        sTimeReport = sTimeReport & "3 Objectives: " & Math.Round((Now - startTime).TotalSeconds, 2).ToString & " sec" & vbCrLf

        startTime = Now
        worksheet = AddObjectiveInfodocs(Options)
        Debug.Print("4 ObjectiveInfodocs: " & (Math.Round((Now - startTime).TotalSeconds, 2).ToString) & " sec")
        sTimeReport = sTimeReport & "4 ObjectiveInfodocs: " & Math.Round((Now - startTime).TotalSeconds, 2).ToString & " sec" & vbCrLf

        startTime = Now
        If altH.Nodes.Count > 0 Then 'AS/19486q enclosed
            worksheet = AddAlernativeInfodocs(Options)
            Debug.Print("5 AlernativeInfodocs: " & (Math.Round((Now - startTime).TotalSeconds, 2).ToString) & " sec")
            sTimeReport = sTimeReport & "5 AlernativeInfodocs: " & Math.Round((Now - startTime).TotalSeconds, 2).ToString & " sec" & vbCrLf
        Else
            sTimeReport = sTimeReport & "5 AlernativeInfodocs: 0 (no alternatives in the model)" & vbCrLf
        End If 'AS/19486q

        startTime = Now
        worksheet = AddAlternativesWRTObjectivesInfodocs(Options)
        Debug.Print("7 AlternativesWRTObjectivesInfodocs: " & (Math.Round((Now - startTime).TotalSeconds, 2).ToString) & " sec")
        sTimeReport = sTimeReport & "7 AlternativesWRTObjectivesInfodocs: " & Math.Round((Now - startTime).TotalSeconds, 2).ToString & " sec" & vbCrLf

        startTime = Now
        If altH.Nodes.Count > 0 Then 'AS/19486q enclosed
            worksheet = AddContributionMatrix(Options)
            Debug.Print("6 ContributionMatrix: " & (Math.Round((Now - startTime).TotalSeconds, 2).ToString) & " sec")
            sTimeReport = sTimeReport & "6 ContributionMatrix: " & Math.Round((Now - startTime).TotalSeconds, 2).ToString & " sec" & vbCrLf
        Else
            sTimeReport = sTimeReport & "6 ContributionMatrix: 0 (no alternatives in the model)" & vbCrLf
        End If 'AS/19486q

        startTime = Now
        worksheet = AddParticipants(Options)
        Debug.Print("8 Participants: " & (Math.Round((Now - startTime).TotalSeconds, 2).ToString) & " sec")
        sTimeReport = sTimeReport & "8 Participants: " & Math.Round((Now - startTime).TotalSeconds, 2).ToString & " sec" & vbCrLf

        startTime = Now
        worksheet = AddMeasurementMethods(Options)
        Debug.Print("9 MeasurementMethods: " & (Math.Round((Now - startTime).TotalSeconds, 2).ToString) & " sec")
        sTimeReport = sTimeReport & "9 MeasurementMethods: " & Math.Round((Now - startTime).TotalSeconds, 2).ToString & " sec" & vbCrLf

        startTime = Now
        If AddSurveys(Options) Then 'AS/19486s
            Debug.Print("10 Surveys (all three): " & (Math.Round((Now - startTime).TotalSeconds, 2).ToString) & " sec")
            sTimeReport = sTimeReport & "10 Surveys (all three): " & Math.Round((Now - startTime).TotalSeconds, 2).ToString & " sec" & vbCrLf
        Else
            sTimeReport = sTimeReport & "10 Surveys (all three): 0 (no survey in the model)" & vbCrLf
        End If '

        startTime = Now
        worksheet = AddInconsistency(Options)
        Debug.Print("11 Inconsistency: " & (Math.Round((Now - startTime).TotalSeconds, 2).ToString) & " sec")
        sTimeReport = sTimeReport & "11 Inconsistency: " & Math.Round((Now - startTime).TotalSeconds, 2).ToString & " sec" & vbCrLf

        startTime = Now
        worksheet = AddConsensus(Options)
        Debug.Print("12 Consensus: " & (Math.Round((Now - startTime).TotalSeconds, 2).ToString) & " sec")
        sTimeReport = sTimeReport & "12 Consensus: " & Math.Round((Now - startTime).TotalSeconds, 2).ToString & " sec" & vbCrLf

        'add Debug_Timing sheet
        Debug.Print("Reports Overall Elapsed time: " & (Math.Round((Now - startElapsedTime).TotalSeconds, 2).ToString) & " sec")
        sTimeReport = sTimeReport & "Reports Overall Elapsed time: " & (Math.Round((Now - startElapsedTime).TotalSeconds, 2).ToString) & " sec"
        worksheet = Debug_Timing(sTimePrep, sTimeReport) 'AS/19486r

        '==================================
        ''Lock the workbook - don't allow user to edit it 'AS/19486y===
        'For Each ws As ExcelWorksheet In workbook.Worksheets
        '    ws.Protected = True
        'Next
        'Dim protectionSettings = workbook.ProtectionSettings
        'protectionSettings.ProtectStructure = True 'AS/19486y==

        'SurveyTest() 'AS/19486n
        'Debug_getNodesIDs 'AS/19486v

        workbook.Save(filePath)
        Return workbook

    End Function

    Private Function AddOverview(Options As ReportGeneratorOptions) As ExcelWorksheet 'AS/19486a 
        Dim worksheet = workbook.Worksheets.Add("Overview")

        Dim rowModelName As Integer = 0
        Dim rowModelDescription As Integer = 2
        Dim colInfodoc As Integer = 0

        With worksheet
            .Columns(colInfodoc).Style.Font.Name = "Calibri"

            'write model name
            .Cells(rowModelName, colInfodoc).Value = "Decision Model: " & PM.ProjectName
            .Cells(rowModelName, colInfodoc).Style.Font.Weight = ExcelFont.BoldWeight
            .Cells(rowModelName, colInfodoc).Style.Font.Size = 16 * 20

            'write model description
            Dim sText As String = Infodoc2Text(App.ActiveProject, PM.ProjectDescription)
            If sText <> "" Then
                .Cells(rowModelDescription, colInfodoc).Value = sText
                .Cells(rowModelDescription, colInfodoc).Style.Font.Size = 14 * 20
            End If

            Dim lstInfoPictures As New List(Of KeyValuePair(Of String, ExcelPicture))
            Dim colRightMostColumn As Integer = colInfodoc
            'Dim col As Integer = colInfodoc + 1 'AS/21022a
            Dim col As Integer = colInfodoc 'AS/21022a

            Dim InfoDocType As reObjectType = reObjectType.Description
            Dim sInfodocID As String = "description"
            Dim WRTParentNode As Integer = -1
            Dim sContent As String = PM.ProjectDescription

            InsertInfodocImages(worksheet, InfoDocType, sInfodocID, sContent, col, rowModelDescription, WRTParentNode, colRightMostColumn, lstInfoPictures) 'AS/19486_i

            'format
            .Cells(rowModelDescription, colInfodoc).Style.Borders.SetBorders(MultipleBorders.All, Color.Black, LineStyle.Thin)

            Dim fSize As Double = .Cells(rowModelName, colInfodoc).Style.Font.Size / 20
            Dim fName As String = .Columns(colInfodoc).Style.Font.Name

            Dim colW As Double, rowH As Double
            getCellSize(.Cells(rowModelName, colInfodoc).Value.ToString, fName, colW, rowH, CSng(fSize.ToString))

            .Columns(colInfodoc).SetWidth(colW, LengthUnit.Point)
            .Columns(colInfodoc).Style.WrapText = True

            .Rows(rowModelDescription).AutoFit() 'AS/19486w===
            'For i As Integer = colInfodoc + 1 To colRightMostColumn 'AS/19486_c===
            '    .Columns(i).AutoFit()
            'Next i 'AS/19486w==

            AdjustImages(worksheet, lstInfoPictures)

            'Make entire sheet print on a single page.
            .PrintOptions.FitWorksheetWidthToPages = 1
            .PrintOptions.FitWorksheetHeightToPages = 1
        End With

        Return worksheet
    End Function

    Private Function AddAlternatives(Options As ReportGeneratorOptions, server As HttpServerUtility) As ExcelWorksheet 'AS/19486b 

        If altH.Nodes.Count = 0 Then Exit Function 'AS/19486n

        Dim worksheet = workbook.Worksheets.Add("Alternatives")

        'define rows and columns
        Dim rowParticipants As Integer = 0 'columns headers
        Dim rowPriorities As Integer = 1
        Dim colAlternative As Integer = 0

        Dim row As Integer = 2
        Dim col As Integer = 1

        Dim tNormMode As LocalNormalizationType = CType(PM.Parameters.Normalization, LocalNormalizationType)
        Dim altPriorityG As Double
        Dim username As String = ""
        Dim barColors As List(Of String) = New List(Of String)

        With worksheet

            'write headers
            .Cells(rowParticipants, colAlternative).Value = "Model Name: " & PM.ProjectName
            .Cells(rowPriorities, colAlternative).Value = "Alternative"

            For Each UserID As Integer In users
                'get username
                Select Case UserID
                    Case >= 0 'individual user
                        username = PM.GetUserByID(UserID).UserName
                    Case Else 'group
                        username = PM.CombinedGroups.GetCombinedGroupByUserID(UserID).Name
                End Select

                .Cells(rowParticipants, col).Value = username
                .Cells(rowPriorities, col).Value = "Global Priority"
                col += 1
            Next

            'write data - alts names and priorities
            For i As Integer = 0 To alts.First.Value.Count - 1
                col = 1
                For Each pair As KeyValuePair(Of Integer, List(Of NodePriority)) In alts
                    If tNormMode = LocalNormalizationType.ntUnnormalized Then
                        altPriorityG = pair.Value(i).GlobalPriority
                    Else
                        altPriorityG = pair.Value(i).NormalizedGlobalPriority
                    End If

                    Dim alt As clsNode = GetNodeByID(altH.Nodes, pair.Value(i).NodeID)
                    .Cells(row, colAlternative).Value = alt.NodeName
                    .Cells(row, col).Value = altPriorityG
                    col += 1 '2 'AS/19486u
                Next 'pair
                row += 1
            Next i

            'freeze top row
            .Panes = New WorksheetPanes(PanesState.Frozen, 0, 1, "A2", PanePosition.TopRight)

            .Columns(colAlternative).AutoFit() 'AS/19486z===
            For i = colAlternative + 1 To col
                .Columns(i).AutoFit()
                .Columns(i).Style.NumberFormat = "0.00%"
            Next i 'AS/19486z==

            ' ===== Create Excel chart and select data for it. 'AS/19486b1===
            Dim rowTableEnd As Integer = row
            Dim rowChartEnd = rowTableEnd + altH.Nodes.Count * 2
            If rowChartEnd < 20 Then rowChartEnd = 20 'AS/19486u
            Dim chart = .Charts.Add(ChartType.Bar, "B" & rowTableEnd + 2, "J" & rowChartEnd)
            Dim dataRange As CellRange = .Cells.GetSubrangeAbsolute(rowPriorities, colAlternative, rowTableEnd - 1, colAlternative + 1)
            'dataRange.Style.Borders.SetBorders(MultipleBorders.All, Color.Black, LineStyle.Thick) 'AS/19486u
            chart.SelectData(dataRange, True)

            ' Set chart legend.
            chart.Legend.IsVisible = False 'True
            'chart.Legend.Position = ChartLegendPosition.Right

            ' Define colors
            Dim chartBackColor = DrawingColor.FromRgb(ecColors.ecGray9.R, ecColors.ecGray9.G, ecColors.ecGray9.B)
            Dim borderColor = DrawingColor.FromName(DrawingColorName.Black)

            ' Format chart
            Dim barchart = CType(chart, BarChart)
            barchart.Fill.SetSolid(chartBackColor)
            barchart.Axes.Vertical.IsVisible = True 'False
            barchart.Axes.Vertical.ReverseOrder = True 'AS/19486_j
            barchart.DataLabels.LabelPosition = DataLabelPosition.OutsideEnd
            'barchart.DataLabels.LabelContainsCategoryName = True
            barchart.DataLabels.LabelContainsValue = True
            barchart.Position.From.Column = .Columns("A") 'AS/19486_j
            barchart.Position.To.Column = .Columns("N") 'AS/19486_j replaced "Q" with "N"
            barchart.SeriesGapWidth = 0.35

            Dim series = barchart.Series(0)
            series.Outline.Fill.SetNone()
            Dim dataPoints = series.DataPoints

            'get bars colors from ECC and apply colors to the chart bars
            For Each alt In altH.Nodes
                Dim sAltColor As String = ""
                Dim altColor As Long = CLng(PM.Attributes.GetAttributeValue(ATTRIBUTE_DEFAULT_BRUSH_COLOR_ID, alt.NodeGuidID))
                sAltColor = If(altColor > 0, LongToBrush(altColor), GetPaletteColor(CurrentPaletteID(PM), alt.NodeID, True))
                barColors.Add(sAltColor)
            Next

            Dim count As Integer = series.Values.Cast(Of Object)().Count()
            For i = 0 To count - 1
                Dim R = ColorTranslator.FromHtml(barColors(i)).R
                Dim G = ColorTranslator.FromHtml(barColors(i)).G
                Dim B = ColorTranslator.FromHtml(barColors(i)).B
                dataPoints(i).Fill.SetSolid(DrawingColor.FromRgb(R, G, B))
            Next

            Dim outline = chart.Outline
            outline.Width = Length.From(2, GemBox.Spreadsheet.LengthUnit.Point)
            outline.Fill.SetSolid(borderColor)

            ' Format plot area
            outline = chart.PlotArea.Outline
            outline.Width = Length.From(1.5, GemBox.Spreadsheet.LengthUnit.Point)
            outline.Fill.SetSolid(borderColor)

            ' Format chart title 
            chart.Title.Text = "All Participants" 'username
            Dim textFormat = chart.Title.TextFormat
            textFormat.Size = Length.From(20, GemBox.Spreadsheet.LengthUnit.Point)
            textFormat.Font = "Arial" 'AS/19486b1==

            'AS/19486x===
            ''===== insert sensitivity graphs as images 'AS/19486b2===
            ''Performance Sensitivity ∆ Objectives
            'Dim rowPictureStart As Integer = rowChartEnd + 3
            'Dim colPictureLeft As String = "B"
            'Dim positionFromCell As String = colPictureLeft & rowPictureStart.ToString
            'Dim picName As String = "Sens-Objectives.png"
            'Dim picPath As String = "~/Images/favicon/" & picName 'AS/19486d===
            'Dim pic As ExcelPicture = .Pictures.Add(server.MapPath(picPath), positionFromCell)

            ''''draw borders around the picture (see example: Test_AddBorderToImage in ReportGenerator_Excel.vb)
            ''Dim picPosition = pic.Position
            ''picPosition.Mode = PositioningMode.MoveAndSize
            ''picPosition.To = New AnchorCell(picPosition.To.Column, picPosition.To.Row, 0, 0)
            ''cellRange = worksheet.Cells.GetSubrangeAbsolute(picPosition.From.Row.Index, picPosition.From.Column.Index, picPosition.To.Row.Index - 1, picPosition.To.Column.Index - 1)
            ''cellRange.Style.Borders.SetBorders(MultipleBorders.Outside, System.Drawing.Color.Black, LineStyle.Thick)

            ''Performance Sensitivity ∆ Alternatives
            'rowPictureStart = pic.Position.To.Row.Index + 2
            'positionFromCell = colPictureLeft & rowPictureStart.ToString
            'picName = "Sens-Alternatives.png"
            'picPath = "~/Images/favicon/" & picName
            'pic = .Pictures.Add(server.MapPath(picPath), positionFromCell)

            ''''draw borders around the picture (see example: Test_AddBorderToImage in ReportGenerator_Excel.vb)
            ''picPosition = pic.Position
            ''picPosition.Mode = PositioningMode.MoveAndSize
            ''picPosition.To = New AnchorCell(picPosition.To.Column, picPosition.To.Row, 0, 0)
            ''cellRange = worksheet.Cells.GetSubrangeAbsolute(picPosition.From.Row.Index, picPosition.From.Column.Index, picPosition.To.Row.Index - 1, picPosition.To.Column.Index - 1)
            ''cellRange.Style.Borders.SetBorders(MultipleBorders.Outside, System.Drawing.Color.Black, LineStyle.Thick) 'AS/19486d==
            'AS/19486x==
        End With

        Return worksheet
    End Function

    Private Function AddObjectives(Options As ReportGeneratorOptions, server As HttpServerUtility) As ExcelWorksheet 'AS/19486c
        Dim worksheet = workbook.Worksheets.Add("Objectives")

        'define rows and columns
        Dim rowParticipantName As Integer = 0 'columns headers
        Dim rowPriorityType As Integer = 1
        Dim maxLevel As Integer = objH.GetMaxLevel
        Dim colObjectives As Integer = 0
        Dim colPriorities As Integer = objH.GetMaxLevel

        Dim row As Integer = 2
        Dim col As Integer = maxLevel + 1

        Dim tNormMode As LocalNormalizationType = CType(PM.Parameters.Normalization, LocalNormalizationType)
        Dim objPriorityG As Double
        Dim objPriorityL As Double
        Dim username As String = ""
        Dim barColors As List(Of String) = New List(Of String)

        With worksheet

            'write headers
            .Cells(rowParticipantName, colObjectives).Value = "Model Name: " & PM.ProjectName
            .Cells(rowPriorityType, colObjectives).Value = "Objectives"

            For Each UserID As Integer In users
                'get username
                Select Case UserID
                    Case >= 0 'individual user
                        username = PM.GetUserByID(UserID).UserName
                    Case Else 'group
                        username = PM.CombinedGroups.GetCombinedGroupByUserID(UserID).Name
                End Select

                .Cells(rowParticipantName, col).Value = username
                .Cells(rowPriorityType, col).Value = "Global Priority"
                col += 1

                .Cells(rowParticipantName, col).Value = username
                .Cells(rowPriorityType, col).Value = "Local Priority"
                col += 1
            Next

            'write data - objs names and priorities
            Dim goalnode As clsNode = objH.Nodes(0) 'objH.GetNodeByID(1) 'AS/19486t
            .Cells(row, colObjectives).Value = goalnode.NodeName
            row += 1

            If objs.Count > 0 AndAlso objs.First.Value.Count > 0 Then
                For Each t As Tuple(Of Integer, Integer, clsNode) In PM.Hierarchy(PM.ActiveHierarchy).NodesInLinearOrder
                    col = maxLevel + 1
                    Dim NodeId As Integer = t.Item1
                    Dim ParentNodeID As Integer = t.Item2
                    Dim node As clsNode = t.Item3 'objH.GetNodeByID(NodeId) 'AS/19486t
                    If objs.First.Value.Find(Function(x) x.Item1 = NodeId AndAlso x.Item2 = ParentNodeID) IsNot Nothing Then
                        For Each pair As KeyValuePair(Of Integer, List(Of Tuple(Of Integer, Integer, NodePriority))) In objs
                            Dim nPriority As NodePriority = pair.Value.Find(Function(x) x.Item1 = NodeId AndAlso x.Item2 = ParentNodeID).Item3
                            objPriorityL = nPriority.LocalPriority
                            objPriorityG = nPriority.GlobalPriority

                            .Cells(row, colObjectives + node.Level).Value = node.NodeName
                            .Cells(row, col).Value = objPriorityG
                            .Cells(row, col + 1).Value = objPriorityL
                            col += 2
                        Next 'pair
                        row += 1
                    End If
                Next
            End If

            Dim cellRange As CellRange = .Cells.GetSubrangeAbsolute(rowParticipantName, colObjectives, rowParticipantName, colPriorities)
            cellRange.Merged = True
            cellRange = .Cells.GetSubrangeAbsolute(rowPriorityType, colObjectives, rowPriorityType, colPriorities)
            cellRange.Merged = True

            For i = colPriorities + 1 To col
                .Columns(i).AutoFit()
                .Columns(i).Style.NumberFormat = "0.00%"
                .Cells(rowPriorityType + 1, i).Value = 1
            Next i

            'freeze top row
            .Panes = New WorksheetPanes(PanesState.Frozen, 0, 1, "A2", PanePosition.TopRight)

            'AS/19486x===
            ''===== insert treeview (images)
            'Dim rowPictureStart As Integer = row + 3
            'Dim colPictureLeft As String = "B"
            'Dim positionFromCell As String = colPictureLeft & rowPictureStart.ToString
            'Dim picName As String = "Objectives_Treeview_-_ITP(Hal).png"
            'Dim picPath As String = "~/Images/favicon/" & picName 'AS/19486d===
            'Dim pic As ExcelPicture = .Pictures.Add(server.MapPath(picPath), positionFromCell)

            ''''draw borders around the picture (see example: Test_AddBorderToImage in ReportGenerator_Excel.vb)
            ''Dim picPosition = pic.Position
            ''picPosition.Mode = PositioningMode.MoveAndSize
            ''picPosition.To = New AnchorCell(picPosition.To.Column, picPosition.To.Row, 0, 0)
            ''cellRange = worksheet.Cells.GetSubrangeAbsolute(picPosition.From.Row.Index, picPosition.From.Column.Index, picPosition.To.Row.Index - 1, picPosition.To.Column.Index - 1)
            ''cellRange.Style.Borders.SetBorders(MultipleBorders.Outside, System.Drawing.Color.Black, LineStyle.Thick) 

            ''===== insert pie chart (images)
            'rowPictureStart = pic.Position.To.Row.Index + 2
            'positionFromCell = colPictureLeft & rowPictureStart.ToString
            'picName = "Objectives_PieChart_-_ITP(Hal).png"
            'picPath = "~/Images/favicon/" & picName
            'pic = .Pictures.Add(server.MapPath(picPath), positionFromCell)

            ''''draw borders around the picture (see example: Test_AddBorderToImage in ReportGenerator_Excel.vb)
            ''picPosition = pic.Position
            ''picPosition.Mode = PositioningMode.MoveAndSize
            ''picPosition.To = New AnchorCell(picPosition.To.Column, picPosition.To.Row, 0, 0)
            ''cellRange = worksheet.Cells.GetSubrangeAbsolute(picPosition.From.Row.Index, picPosition.From.Column.Index, picPosition.To.Row.Index - 1, picPosition.To.Column.Index - 1)
            ''cellRange.Style.Borders.SetBorders(MultipleBorders.Outside, System.Drawing.Color.Black, LineStyle.Thick) 'AS/19486d==
            'AS/19486x==
        End With

        Return worksheet
    End Function

    Private Function AddObjectiveInfodocs(Options As ReportGeneratorOptions) As ExcelWorksheet 'AS/19486e
        Dim worksheet = workbook.Worksheets.Add("ObjectiveInfodocs")

        Dim row As Integer = 1 'row number 
        Dim col As Integer = 0 'col number

        Dim rowColumnHeader As Integer = 0
        Dim colObjName As Integer = 0
        Dim colInfodoc As Integer = 1
        Dim colRightMostColumn As Integer = colInfodoc

        Dim maxLenObjName As Integer = 10
        Dim colW As Double

        Dim lstInfoPictures As New List(Of KeyValuePair(Of String, ExcelPicture))
        Dim dataFound As Boolean = False 'AS/19486_g
        Dim InfoDocType As reObjectType = reObjectType.Node

        With worksheet
            For Each node As clsNode In objH.Nodes
                Dim sText As String = Infodoc2Text(App.ActiveProject, node.InfoDoc)
                If sText <> "" Then
                    dataFound = True
                    .Cells(row, colObjName).Value = node.NodeName
                    .Cells(row, colInfodoc).Value = sText

                    If maxLenObjName < Len(node.NodeName) Then maxLenObjName = Len(node.NodeName)

                    Dim fSize As Double = .Cells(row, colObjName).Style.Font.Size / 20
                    Dim fName As String = .Columns(colObjName).Style.Font.Name

                    Dim newW As Double, newH As Double
                    getCellSize(node.NodeName, fName, newW, newH, CSng(fSize.ToString))
                    If colW < newW Then colW = newW

                    'col = colInfodoc + 1 'AS/19203j=== 'AS/21022a
                    col = colInfodoc 'AS/21022a
                    Dim sInfodocID As String = node.NodeID.ToString
                    Dim WRTParentNode As Integer = -1 'node.ParentNodeID
                    Dim sContent As String = node.InfoDoc

                    InsertInfodocImages(worksheet, InfoDocType, sInfodocID, sContent, col, row, WRTParentNode, colRightMostColumn, lstInfoPictures) 'AS/19486_i

                    row += 1
                End If

                'get infodoc for objective WRT parent objective 'AS/19486v===
                Dim ssContent As String = ""
                If Not IsNothing(node.ParentNode) Then
                    ssContent = PM.InfoDocs.GetNodeWRTInfoDoc(node.NodeGuidID, node.ParentNode.NodeGuidID)
                    If ssContent <> "" Then
                        dataFound = True
                        sText = Infodoc2Text(App.ActiveProject, ssContent)
                        .Cells(row, colObjName).Value = node.NodeName & " WRT " & node.ParentNode.NodeName
                        .Cells(row, colInfodoc).Value = sText
                        row += 1
                    End If
                End If
                'AS/19486v==
            Next
            If Not dataFound Then 'AS/19486_g===
                workbook.Worksheets.Remove("ObjectiveInfodocs")
                Return Nothing
            End If 'AS/19486_g==

            'freeze top row
            .Panes = New WorksheetPanes(PanesState.Frozen, 0, 1, "A2", PanePosition.TopRight) 'AS/19486_g

            'format
            .Cells(rowColumnHeader, colObjName).Value = "Infodocs for Objectives"
            .Cells(rowColumnHeader, colObjName).Style.Font.Weight = ExcelFont.BoldWeight


            .Columns(colObjName).SetWidth(colW, LengthUnit.Point)
            .Columns(colObjName).Style.VerticalAlignment = VerticalAlignmentStyle.Top
            .Columns(colObjName).Style.WrapText = True

            .Columns(colInfodoc).SetWidth(colW, LengthUnit.Point)
            .Columns(colInfodoc).Style.VerticalAlignment = VerticalAlignmentStyle.Top
            .Columns(colInfodoc).Style.WrapText = True

            For i As Integer = 1 To row 'AS/19203j===
                .Rows(i).AutoFit()
            Next i

            AdjustImages(worksheet, lstInfoPictures) 'AS/19203j== 

        End With

        Return worksheet

    End Function

    Private Function AddAlernativeInfodocs(Options As ReportGeneratorOptions) As ExcelWorksheet 'AS/19486f
        Dim worksheet = workbook.Worksheets.Add("AlternativeInfodocs")

        Dim row As Integer = 1 'row number 
        Dim col As Integer = 0 'col number

        Dim rowColumnHeader As Integer = 0
        Dim colAltName As Integer = 0
        Dim colInfodoc As Integer = 1
        Dim colRightMostColumn As Integer = colInfodoc

        Dim maxLenObjName As Integer = 10
        Dim colW As Double

        Dim lstInfoPictures As New List(Of KeyValuePair(Of String, ExcelPicture))
        Dim dataFound As Boolean = False 'AS/19486_g
        Dim InfoDocType As reObjectType = reObjectType.Alternative

        With worksheet
            For Each node As clsNode In altH.Nodes
                Dim sText As String = Infodoc2Text(App.ActiveProject, node.InfoDoc)
                If sText <> "" Then
                    dataFound = True 'AS/19486_g
                    .Cells(row, colAltName).Value = node.NodeName
                    .Cells(row, colInfodoc).Value = sText

                    If maxLenObjName < Len(node.NodeName) Then maxLenObjName = Len(node.NodeName)

                    Dim fSize As Double = .Cells(row, colAltName).Style.Font.Size / 20
                    Dim fName As String = .Columns(colAltName).Style.Font.Name

                    Dim newW As Double, newH As Double
                    getCellSize(node.NodeName, fName, newW, newH, CSng(fSize.ToString))
                    If colW < newW Then colW = newW

                    'col = colInfodoc + 1 'AS/19203j=== 'AS/21022a
                    col = colInfodoc 'AS/21022a

                    Dim sInfodocID As String = node.NodeID.ToString
                    Dim WRTParentNode As Integer = -1 'node.ParentNodeID
                    Dim sContent As String = node.InfoDoc

                    InsertInfodocImages(worksheet, InfoDocType, sInfodocID, sContent, col, row, WRTParentNode, colRightMostColumn, lstInfoPictures) 'AS/19486_i

                    row += 1
                End If
            Next

            If Not dataFound Then 'AS/19486_g===
                workbook.Worksheets.Remove("AlternativeInfodocs")
                Return Nothing
            End If 'AS/19486_g==

            'freeze top row
            .Panes = New WorksheetPanes(PanesState.Frozen, 0, 1, "A2", PanePosition.TopRight) 'AS/19486_g

            'format
            .Cells(rowColumnHeader, colAltName).Value = "Infodocs for Alternatives"
            .Cells(rowColumnHeader, colAltName).Style.Font.Weight = ExcelFont.BoldWeight

            .Columns(colAltName).SetWidth(colW, LengthUnit.Point)
            .Columns(colAltName).Style.VerticalAlignment = VerticalAlignmentStyle.Top
            .Columns(colAltName).Style.WrapText = True

            .Columns(colInfodoc).SetWidth(colW, LengthUnit.Point)
            .Columns(colInfodoc).Style.VerticalAlignment = VerticalAlignmentStyle.Top
            .Columns(colInfodoc).Style.WrapText = True

            For i As Integer = 1 To row 'AS/19203j===
                .Rows(i).AutoFit()
            Next i
            For i As Integer = colInfodoc + 1 To colRightMostColumn
                .Columns(i).AutoFit()
            Next i

            AdjustImages(worksheet, lstInfoPictures) 'AS/19203j==
        End With

        Return worksheet

    End Function

    Private Function AddAlternativesWRTObjectivesInfodocs(Options As ReportGeneratorOptions) As ExcelWorksheet 'AS/19486h
        'AS/19486_a redid the entire function to display the infodocs similar to the infodocs grid
        'also 'AS/19486_d insert infodoc image in the neighboring cell

        If altH.Nodes.Count = 0 Then Return Nothing

        Dim worksheet = workbook.Worksheets.Add("AlternativesWRTObjectives")
        Dim InfoDocType As reObjectType = reObjectType.AltWRTNode

        Dim rowCovObjective As Integer = 0
        Dim colAltName As Integer = 0
        Dim colInfodoc As Integer = 1
        Dim colRightMostColumn As Integer = colInfodoc

        Dim row As Integer = 1 'row number 
        Dim col As Integer = colInfodoc 'col number
        Dim lstInfoPictures As New List(Of KeyValuePair(Of String, ExcelPicture))
        Dim lstColumnsWithDocs As New List(Of Integer)
        Dim dataFound As Boolean = False 'AS/19486_g
        Dim infoExists As Boolean = False 'AS/21022a

        Dim MyComputer As New Devices.Computer

        With worksheet
            'For Each obj In objH.TerminalNodes 'AS/21039 
            For Each t As Tuple(Of Integer, Integer, clsNode) In objH.NodesInLinearOrder 'AS/21039===
                Dim obj As clsNode = t.Item3
                If obj.IsTerminalNode Then 'AS/21039==
                    row = 1
                    .Cells(rowCovObjective, col).Value = obj.NodeName
                    infoExists = False 'AS/21039 moved up

                    For Each alt As clsNode In altH.Nodes
                        .Cells(row, colAltName).Value = alt.NodeName

                        Dim sInfodocID As String = alt.NodeID.ToString
                        Dim WRTParentNode As Integer = obj.NodeID
                        Dim sInfodoc As String = PM.InfoDocs.GetNodeWRTInfoDoc(alt.NodeGuidID, obj.NodeGuidID)
                        colInfodoc = col 'AS/21022a
                        'infoExists = False 'AS/21039 moved up

                        If sInfodoc <> "" Then 'AS/19486_i===
                            dataFound = True
                            infoExists = True 'AS/21022a
                            .Cells(row, col).Value = Infodoc2Text(App.ActiveProject, sInfodoc)
                            lstColumnsWithDocs.Add(col)
                            'col = col + 1 'AS/21022

                            InsertInfodocImages(worksheet, InfoDocType, sInfodocID, sInfodoc, col, row, WRTParentNode, colRightMostColumn, lstInfoPictures)
                        End If 'AS/19486_i==

                        col = colInfodoc 'AS/21022a
                        row += 1
                    Next 'alternative
                    'col += 1 'AS/21022a
                    'If Not infoExists Then 'AS/21022a=== 'AS/21039
                    If infoExists Then 'AS/21039
                        colRightMostColumn += 1
                        col = colRightMostColumn 'AS/21039
                    Else
                        col += 1 'AS/21039
                    End If
                    'col = colRightMostColumn '+ 1 'AS/21022a== 'AS/21039
                End If 'AS/21039
            Next 'cov objective

            If colRightMostColumn < col - 1 Then colRightMostColumn = col - 1

            If Not dataFound Then 'AS/19486_g===
                workbook.Worksheets.Remove("AlternativesWRTObjectives")
                Return Nothing
            End If

            'freeze top row
            .Panes = New WorksheetPanes(PanesState.Frozen, 0, 1, "A2", PanePosition.TopRight) 'AS/19486_g

            'format
            .Cells(rowCovObjective, colAltName).Value = "Alternatives Infodocs WRT Covering Objectives" 'write table name
            .Cells(rowCovObjective, colAltName).Style.Font.Weight = ExcelFont.BoldWeight

            Dim cellRange As CellRange = .Cells.GetSubrangeAbsolute(rowCovObjective, colAltName, row, col - 1)
            cellRange.Style.VerticalAlignment = VerticalAlignmentStyle.Top
            cellRange.Style.WrapText = True

            .Columns(colAltName).AutoFit()

            For i As Integer = 0 To row 'AS/19203j===
                .Rows(i).AutoFit()
            Next i

            Dim colW As Double = .Columns(colAltName).Width
            For i As Integer = colAltName + 1 To colRightMostColumn
                If lstColumnsWithDocs.Contains(i) Then
                    .Columns(i).Width = CInt(colW)
                End If
            Next i

            AdjustImages(worksheet, lstInfoPictures) 'AS/19203j==
        End With

        Return worksheet

    End Function

    Private Function AddContributionMatrix(Options As ReportGeneratorOptions) As ExcelWorksheet 'AS/19486g
        Dim worksheet = workbook.Worksheets.Add("ContributionMatrix")

        Dim rowObjName As Integer = 0
        Dim colAltName As Integer = 0
        Dim cellRange As CellRange = Nothing 'AS/18712yb
        Dim row As Integer = rowObjName + 1 'row number 
        Dim col As Integer = colAltName + 1 'col number

        With worksheet
            'write table title
            .Cells(rowObjName, colAltName).Value = "Contribution Matrix"
            .Cells(rowObjName, colAltName).Style.Font.Weight = ExcelFont.BoldWeight

            For Each obj As clsNode In objH.TerminalNodes
                .Cells(rowObjName, col).Value = obj.NodeName
                row = rowObjName + 1
                For Each alt As clsNode In altH.Nodes
                    .Cells(row, colAltName).Value = alt.NodeName
                    If obj.GetContributedAlternatives.Contains(alt) Then
                        .Cells(row, col).Value = 1
                    Else
                        .Cells(row, col).Value = 0
                    End If
                    row += 1
                Next
                col += 1
            Next

            'freeze top row
            .Panes = New WorksheetPanes(PanesState.Frozen, 0, 1, "A2", PanePosition.TopRight) 'AS/19486_g

            .Columns(colAltName).AutoFit()

            cellRange = .Cells.GetSubrangeAbsolute(rowObjName, colAltName + 1, rowObjName, col - 1)
            cellRange.Style.WrapText = True
        End With

        Return worksheet
    End Function

    Private Function AddParticipants(Options As ReportGeneratorOptions) As ExcelWorksheet 'AS/19486i
        Dim worksheet = workbook.Worksheets.Add("Participants")

        'special rows:
        Dim rowTitle As Integer = 0
        Dim rowColumnHeader As Integer = 1 'column header

        'special columns:
        Dim colEmail As Integer = 0
        Dim colUserName As Integer = 1
        Dim colPriority As Integer = 2
        Dim colPermission As Integer = 3
        Dim colEvaluationProgress As Integer = 4
        Dim colLastJudgmentsTime As Integer = 5
        Dim colDisabled As Integer = 6
        Dim colFirstAttribute As Integer = 7
        Dim userAttributes As New List(Of clsAttribute)
        For Each attr In PM.Attributes.GetUserAttributes()
            If Not attr.IsDefault Then userAttributes.Add(attr)
        Next

        Dim usersGroups As New List(Of clsGroup)
        For Each group As clsCombinedGroup In PM.CombinedGroups.GroupsList
            If group.CombinedUserID <> -1 Then usersGroups.Add(group)
        Next

        Dim row As Integer = 2 'row number 
        Dim col As Integer = 0 'col number


        With worksheet
            'wrtite columns headers
            .Cells(rowColumnHeader, colEmail).Value = "Email"
            .Cells(rowColumnHeader, colUserName).Value = "Participant Name"
            .Cells(rowColumnHeader, colPriority).Value = "Priority"
            .Cells(rowColumnHeader, colPermission).Value = "Permission"
            .Cells(rowColumnHeader, colEvaluationProgress).Value = "Evaluation Progress"
            .Cells(rowColumnHeader, colLastJudgmentsTime).Value = "Last Judgment Time"
            .Cells(rowColumnHeader, colDisabled).Value = "Disabled?"
            col = colFirstAttribute
            For Each attr As clsAttribute In userAttributes
                .Cells(rowTitle, col).Value = "Attribute"
                .Cells(rowColumnHeader, col).Value = attr.Name
                col += 1
            Next

            For Each group As clsCombinedGroup In usersGroups
                .Cells(rowTitle, col).Value = "Group"
                .Cells(rowColumnHeader, col).Value = group.Name
                col += 1
            Next

            Using PgUsers As New ProjectParticipantsPage()
                PgUsers.CurrentPageID = _PGID_PROJECT_USERS
                Dim sData As String = PgUsers.GetUsersData()
                If Not String.IsNullOrEmpty(sData) Then
                    Dim Data As JArray = CType(JsonConvert.DeserializeObject(sData), JArray)
                    If Data IsNot Nothing Then
                        For i As Integer = 0 To Data.Count - 1
                            '"id","email","name","ahp_id","role","priority","has_data","dis","has_psw","link","grp_id","pm","can_edit","link_enabled","linkgo_enabled","preview_enabled","last_judg","lst_judg_sort","total","made","progress"
                            .Cells(row, colEmail).Value = CStr(Data(i)("email"))
                            .Cells(row, colUserName).Value = CStr(Data(i)("name"))
                            .Cells(row, colPermission).Value = CStr(Data(i)("role"))
                            .Cells(row, colPriority).Value = CDbl(Data(i)("priority"))
                            .Cells(row, colEvaluationProgress).Value = CStr(Data(i)("progress"))
                            .Cells(row, colLastJudgmentsTime).Value = CStr(Data(i)("last_judg"))
                            .Cells(row, colDisabled).Value = CBool(Data(i)("dis")).ToString

                            col = colFirstAttribute
                            For a_idx As Integer = 0 To userAttributes.Count - 1
                                .Cells(row, col).Value = CStr(Data(i)(String.Format("v{0}", a_idx)))
                                col += 1
                            Next
                            For g_idx As Integer = 0 To usersGroups.Count - 1
                                .Cells(row, col).Value = CBool(Data(i)(String.Format("g{0}", g_idx))).ToString
                                col += 1
                            Next
                            row += 1
                        Next
                    End If
                End If
                PgUsers.Dispose()
            End Using

            'freeze top 2 rows
            '.Panes = New WorksheetPanes(PanesState.Frozen, 0, 1, "A2", PanePosition.TopRight) 'AS/19486_g 'AS/21022
            .Panes = New WorksheetPanes(PanesState.Frozen, 0, 2, "A3", PanePosition.TopLeft) 'AS/21022

            'format
            .Columns(colUserName).AutoFit()
            .Columns(colEmail).AutoFit()
            .Columns(colEvaluationProgress).AutoFit()
            .Columns(colLastJudgmentsTime).AutoFit()
            .Columns(colPermission).AutoFit()
            .Columns(colPriority).Style.NumberFormat = "0.0%"

            Dim cellRange As CellRange = worksheet.Cells.GetSubrangeAbsolute(rowTitle, colEmail, rowColumnHeader, col - 1)
            cellRange.Style.Font.Weight = ExcelFont.BoldWeight

        End With

        Return worksheet
    End Function

    Private Function AddMeasurementMethods(Options As ReportGeneratorOptions) As ExcelWorksheet 'AS/19486j
        Dim worksheet = workbook.Worksheets.Add("MeasurementMethods")

        Dim rowTitle As Integer = 0
        Dim colObjName As Integer = 0
        Dim colObjOrAlt As Integer = 1
        Dim colMeasureType As Integer = 2
        Dim colNumOfComparisons As Integer = 3
        Dim colScaleName As Integer = 4

        Dim row As Integer = 1 'row number 
        Dim col As Integer = 0 'col number

        With worksheet
            'write table title
            .Cells(rowTitle, colObjName).Value = "Measurement Method"
            .Cells(rowTitle, colObjOrAlt).Value = "Obj or Alt"
            .Cells(rowTitle, colMeasureType).Value = "Measurement Type"
            .Cells(rowTitle, colNumOfComparisons).Value = "Number of Comparisons (pairwise only)"
            .Cells(rowTitle, colScaleName).Value = "Custom Scale Name" & vbCrLf & "(Rating scale, utility curve, or step function)"

            'write values into the columns cell by cell
            row = rowTitle + 1
            If objs.Count > 0 AndAlso objs.First.Value.Count > 0 Then 'AS/19486t
                For Each t As Tuple(Of Integer, Integer, clsNode) In PM.Hierarchy(PM.ActiveHierarchy).NodesInLinearOrder 'AS/19486t
                    Dim obj As clsNode = t.Item3
                    .Cells(row, colObjName).Value = obj.NodeName
                    .Cells(row, colMeasureType).Value = GetMeasureTypeName(obj.MeasureType)
                    .Cells(row, colObjName).Style.Indent = obj.Level * 3
                    If obj.MeasureType <> ECMeasureType.mtPairwise Then
                        If Not IsNothing(obj.MeasurementScale) Then 'AS/19486_d enclosed
                            .Cells(row, colScaleName).Value = obj.MeasurementScale.Name
                        End If
                    Else 'AS/19486t===
                        Dim NumOfComparisons As String = getNumOfComparisons(obj.NodeGuidID)
                        .Cells(row, colNumOfComparisons).Value = NumOfComparisons
                    End If 'AS/19486t==

                    If obj.IsTerminalNode Then
                        .Cells(row, colObjOrAlt).Value = "A"
                    Else
                        .Cells(row, colObjOrAlt).Value = "O"
                    End If

                    row += 1
                Next
            End If 'AS/19486t

            'freeze top row
            .Panes = New WorksheetPanes(PanesState.Frozen, 0, 1, "A2", PanePosition.TopRight) 'AS/19486_g

            'format 
            .Columns(colObjName).AutoFit()
            .Columns(colMeasureType).AutoFit()
            .Columns(colScaleName).AutoFit()

            Dim W As Double = .Columns(colMeasureType).GetWidth(LengthUnit.Point)
            .Columns(colNumOfComparisons).SetWidth(W * 1.2, LengthUnit.Point)
            .Columns(colNumOfComparisons).Style.WrapText = True

            Dim cellRange As CellRange = .Cells.GetSubrangeAbsolute(rowTitle, colObjName, rowTitle, colScaleName)
            cellRange.Style.Font.Weight = ExcelFont.BoldWeight
            cellRange.Style.HorizontalAlignment = HorizontalAlignmentStyle.Center
            cellRange.Style.VerticalAlignment = VerticalAlignmentStyle.Center

            cellRange = .Cells.GetSubrangeAbsolute(rowTitle + 1, colObjOrAlt, row, colObjOrAlt) 'AS/21149
            cellRange.Style.HorizontalAlignment = HorizontalAlignmentStyle.Center
        End With

        Return worksheet

    End Function

    Private Function AddInconsistency(Options As ReportGeneratorOptions) As ExcelWorksheet 'AS/19486k

        Dim worksheet = workbook.Worksheets.Add("Inconsistency")

        Dim rowColumnHeaders As Integer = 0
        Dim colUserName As Integer = 0
        Dim colEmail As Integer = 1
        Dim colObjective As Integer = 2
        Dim colPath As Integer = 3
        Dim colInconsistency As Integer = 4
        Dim colNumOfChildren As Integer = 5

        Dim row As Integer = 1 'row number 
        Dim col As Integer = 0 'col number
        Dim dataFound As Boolean = False 'AS/19486_g

        With worksheet
            'write headers for the columns
            .Cells(rowColumnHeaders, colUserName).Value = "Name"
            .Cells(rowColumnHeaders, colEmail).Value = "Email"
            .Cells(rowColumnHeaders, colObjective).Value = "Objective"
            .Cells(rowColumnHeaders, colPath).Value = "Path"
            .Cells(rowColumnHeaders, colInconsistency).Value = "Inconsistency"
            .Cells(rowColumnHeaders, colNumOfChildren).Value = "Number of Children"

            If objH.Nodes.Count > 0 Then
                For Each UserID As Integer In users 'AS/19486s
                    If UserID > -1 Then 'AS/19486s
                        Dim u As clsUser = PM.GetUserByID(UserID)
                        PM.StorageManager.Reader.LoadUserData(u) 'AS/19486m
                        For Each node As clsNode In objH.Nodes
                            If node.MeasureType = ECMeasureType.mtPairwise AndAlso node.IsAllowed(u.UserID) Then
                                If CType(node.Judgments, clsPairwiseJudgments).HasSpanningSet(u.UserID) Then
                                    dataFound = True 'AS/19486_g

                                    node.CalculateLocal(u.UserID)

                                    Dim NodePath As String = ""
                                    GetFullNodePath(node, NodePath)
                                    .Cells(row, colUserName).Value = u.UserName
                                    .Cells(row, colEmail).Value = u.UserEMail
                                    .Cells(row, colObjective).Value = node.NodeName
                                    .Cells(row, colPath).Value = NodePath
                                    .Cells(row, colInconsistency).Value = CType(node.Judgments, clsPairwiseJudgments).EigenCalcs.InconIndex
                                    .Cells(row, colNumOfChildren).Value = node.GetNodesBelow(u.UserID).Count
                                    row += 1
                                End If
                            End If
                        Next
                    End If 'AS/19486s
                Next
            End If
            If Not dataFound Then 'AS/19486_g===
                workbook.Worksheets.Remove("Inconsistency")
                Return Nothing
            End If 'AS/19486_g==

            'freeze top row
            .Panes = New WorksheetPanes(PanesState.Frozen, 0, 1, "A2", PanePosition.TopRight) 'AS/19486_g

            'format 
            .Columns(colInconsistency).Style.NumberFormat = "0.0000"

            .Columns(colUserName).AutoFit()
            .Columns(colObjective).AutoFit()
            .Columns(colEmail).AutoFit()
            .Columns(colInconsistency).AutoFit()
            .Columns(colNumOfChildren).AutoFit()

            Dim colW As Double = .Columns(colObjective).Width
            .Columns(colPath).Width = CInt(colW * 1.5)
        End With

        Return worksheet
    End Function

    Private Function AddConsensus(Options As ReportGeneratorOptions) As ExcelWorksheet 'AS/19486l

        Dim worksheet As ExcelWorksheet = Nothing

        Using SynPage As New SynthesisPage()
            SynPage.CurrentPageID = _PGID_ANALYSIS_CONSENSUS
            Try 'AS/19486_f enclosed
                SynPage.SynthesisView = SynthesisPage.SynthesisViews.vtCV
                Dim sData As String = SynPage.GetConsensusViewData("")
                If Not String.IsNullOrEmpty(sData) Then
                    Dim Data As JArray = CType(JsonConvert.DeserializeObject(sData), JArray) 'AS/19486_g===
                    If Data IsNot Nothing Then
                        If Data.Count > 0 Then 'AS/19486_g==
                            Dim rowColumnHeaders As Integer = 0 'AS/19486_f===
                            Dim colRank As Integer = 0
                            Dim colObjAltName As Integer = 1
                            Dim colWRT As Integer = 2
                            Dim colStdDeviation As Integer = 3
                            Dim colVariance As Integer = 4 'AS/19486_h
                            Dim colStep As Integer = 5
                            Dim maxLenObjName As Integer = 10
                            Dim row As Integer = 1

                            worksheet = workbook.Worksheets.Add("Consensus")
                            With worksheet
                                .Cells(rowColumnHeaders, colRank).Value = "Rank"
                                .Cells(rowColumnHeaders, colObjAltName).Value = "Objective / Alternative"
                                .Cells(rowColumnHeaders, colWRT).Value = "With respect to:" & vbCrLf & "Objective / Covering objective"
                                .Cells(rowColumnHeaders, colStdDeviation).Value = "Standard" & vbCrLf & "Deviation, %"
                                .Cells(rowColumnHeaders, colVariance).Value = "Variance" 'AS/19486_h
                                .Cells(rowColumnHeaders, colStep).Value = "Step" 'AS/19486_f==

                                For i As Integer = 0 To Data.Count - 1
                                    'rank, command, is_alt, child_id, child_name, cov_obj_id, cov_obj_name, variance, std_dev, step_num, is_step_avail
                                    .Cells(row, colRank).Value = CInt(Data(i)("rank")) 'AS/19486_h=== removed convertion numeric values to string
                                    .Cells(row, colObjAltName).Value = CStr(Data(i)("child_name"))
                                    .Cells(row, colWRT).Value = CStr(Data(i)("cov_obj_name"))
                                    .Cells(row, colStdDeviation).Value = Math.Round(CDbl(Data(i)("std_dev")), 2)
                                    .Cells(row, colVariance).Value = Math.Round(CDbl(Data(i)("variance")), 2)
                                    .Cells(row, colStep).Value = CInt(Data(i)("step_num")) 'AS/19486_h==

                                    row += 1
                                Next

                                'freeze top row
                                .Panes = New WorksheetPanes(PanesState.Frozen, 0, 1, "A2", PanePosition.TopRight) 'AS/19486_g

                                'format
                                .Columns(colStdDeviation).Style.NumberFormat = "0.00" 'AS/19486_h
                                .Columns(colVariance).Style.NumberFormat = "0.00" 'AS/19486_h

                                .Columns(colRank).AutoFit()
                                .Columns(colObjAltName).AutoFit()
                                .Columns(colWRT).AutoFit()
                                .Columns(colStdDeviation).AutoFit()
                                .Columns(colVariance).AutoFit()
                                .Columns(colStep).AutoFit()

                                Dim cellRange As CellRange = .Cells.GetSubrangeAbsolute(rowColumnHeaders, colRank, rowColumnHeaders, colStep)
                                cellRange.Style.Font.Weight = ExcelFont.BoldWeight
                                cellRange.Style.HorizontalAlignment = HorizontalAlignmentStyle.Center
                                cellRange.Style.VerticalAlignment = VerticalAlignmentStyle.Center

                                .Columns(colRank).Style.HorizontalAlignment = HorizontalAlignmentStyle.Center
                                .Columns(colStep).Style.HorizontalAlignment = HorizontalAlignmentStyle.Center

                            End With
                            SynPage.Dispose()
                        End If
                    End If
                End If
            Catch 'AS/19486_f===
                SynPage.Dispose()
            End Try 'AS/19486_f==
        End Using
        Return worksheet

    End Function

    Private Function AddSurveys(Options As ReportGeneratorOptions) As Boolean 'AS/19486s
        Dim AUsersList As New Dictionary(Of String, clsComparionUser)
        Dim AGroup = CType(App.ActiveProject.ProjectManager.CombinedGroups.GroupsList(0), clsCombinedGroup)
        For Each AUser In AGroup.UsersList
            AUsersList.Add(AUser.UserEMail, New clsComparionUser() With {.ID = AUser.UserID, .UserName = AUser.UserName})
        Next
        Dim fSurveyInfo As clsSurveyInfo = App.SurveysManager.GetSurveyInfoByProjectID(App.ProjectID, SurveyType.stWelcomeSurvey, AUsersList)
        Dim ASurvey As clsSurvey = fSurveyInfo.Survey("")

        If Not IsNothing(ASurvey) Then
            Dim worksheet As ExcelWorksheet
            worksheet = AddSurveyQuestions(Options, ASurvey, fSurveyInfo.ProjectID)
            If Not IsNothing(worksheet) Then
                worksheet = AddSurveySummary(Options, ASurvey)
                worksheet = AddSurveyAnswers(Options, ASurvey)
                Return True
            Else
                Return False
            End If
        Else
            Return False
        End If

    End Function

    Private Function AddSurveyQuestions(Options As ReportGeneratorOptions, ASurvey As clsSurvey, projectID As Integer) As ExcelWorksheet 'AS/19486n 'AS/19486s

        Dim questionExists As Boolean = False 'AS/19486s===
        For Each APage As clsSurveyPage In ASurvey.Pages
            If APage.Questions.Count > 0 Then
                For Each AQuestion As clsQuestion In APage.Questions
                    If AQuestion.Type <> QuestionType.qtComment And AQuestion.Type <> QuestionType.qtAlternativesSelect And AQuestion.Type <> QuestionType.qtObjectivesSelect Then
                        questionExists = True 'at least one question exists
                        Exit For
                    End If
                Next
            End If
            If questionExists Then Exit For
        Next
        If Not questionExists Then Return Nothing 'AS/19486s==

        Dim rowColumnHeaders As Integer = 0
        Dim colWelcomeOrThank As Integer = 0
        Dim colPageNumber As Integer = 1
        Dim colQuestionNumber As Integer = 2
        Dim colQuestionType As Integer = 3
        Dim colAllowedAnswers As Integer = 4
        Dim colQuestion As Integer = 5

        Dim worksheet = workbook.Worksheets.Add("SurveyQuestions")
        With worksheet
            .Cells(rowColumnHeaders, colWelcomeOrThank).Value = "Welcome or " & vbCrLf & "Thank You"
            .Cells(rowColumnHeaders, colPageNumber).Value = "Page " & vbCrLf & "Number"
            .Cells(rowColumnHeaders, colQuestionNumber).Value = "Question " & vbCrLf & "Number"
            .Cells(rowColumnHeaders, colQuestionType).Value = "Question Type"
            .Cells(rowColumnHeaders, colAllowedAnswers).Value = "Allowed Answers"
            .Cells(rowColumnHeaders, colQuestion).Value = "Question"

            Dim row As Integer = 1
            .Cells(row, colWelcomeOrThank).Value = "Welcome" 'getSurveyType(ASurvey)

            Dim pageNum As Integer = 1
            For Each APage As clsSurveyPage In ASurvey.Pages
                .Cells(row, colPageNumber).Value = pageNum.ToString
                pageNum += 1

                For Each AQuestion As clsQuestion In APage.Questions
                    If AQuestion.Type <> QuestionType.qtComment And AQuestion.Type <> QuestionType.qtAlternativesSelect And AQuestion.Type <> QuestionType.qtObjectivesSelect Then

                        Dim QText As String = ""
                        If isMHT(AQuestion.Text) Then
                            QText = HTML2Text(Infodoc_Unpack(projectID, 0, reObjectType.SurveyQuestion, AQuestion.AGUID.ToString, AQuestion.Text, True, True, -1))
                        Else
                            If clsComparionCorePage.OPT_SURVEY_PARSE_LINKS Then QText = ParseTextHyperlinks(AQuestion.Text) Else QText = AQuestion.Text
                        End If
                        'Debug.Print("QText = " & QText)
                        .Cells(row, colQuestion).Value = QText
                        .Cells(row, colQuestionNumber).Value = ASurvey.GetQuestionPageIndex(AQuestion.AGUID)
                        .Cells(row, colQuestionType).Value = getQuestionTypeString(AQuestion)
                        If AQuestion.AllowVariants Then
                            Dim AVariantText As String = ""
                            For Each AVariant As clsVariant In AQuestion.Variants
                                AVariantText = AVariantText & AVariant.Text & vbCrLf
                                'Debug.Print("AVariantText = " & AVariantText)
                                .Cells(row, colAllowedAnswers).Value = AVariantText
                            Next
                        End If
                        row += 1
                    End If
                Next
            Next

            'freeze top row
            .Panes = New WorksheetPanes(PanesState.Frozen, 0, 1, "A2", PanePosition.TopRight)

            'format
            .Columns(colWelcomeOrThank).AutoFit()
            .Columns(colPageNumber).AutoFit()
            .Columns(colQuestionNumber).AutoFit()
            .Columns(colQuestionType).AutoFit()
            .Columns(colAllowedAnswers).AutoFit()
            .Columns(colQuestion).Width = .Columns(colAllowedAnswers).Width * 2

            Dim cellRange As CellRange = .Cells.GetSubrangeAbsolute(rowColumnHeaders, colWelcomeOrThank, rowColumnHeaders, colQuestion)
            cellRange.Style.Font.Weight = ExcelFont.BoldWeight
            cellRange.Style.HorizontalAlignment = HorizontalAlignmentStyle.Center
            cellRange.Style.VerticalAlignment = VerticalAlignmentStyle.Center

            cellRange = .Cells.GetSubrangeAbsolute(rowColumnHeaders + 1, colWelcomeOrThank, row, colQuestion)
            cellRange.Style.VerticalAlignment = VerticalAlignmentStyle.Top
            cellRange.Style.HorizontalAlignment = HorizontalAlignmentStyle.Left
            cellRange.Style.WrapText = True
        End With
        Return worksheet

    End Function

    Function AddSurveySummary(Options As ReportGeneratorOptions, ASurvey As clsSurvey) As ExcelWorksheet 'AS/19486o 'AS/19486s

        Dim rowColumnHeaders As Integer = 0
        Dim colParticipantID As Integer = 0
        Dim colRespondentName As Integer = 1
        Dim colRespondentEmail As Integer = 2
        Dim colTotalAnswers As Integer = 3

        Dim worksheet = workbook.Worksheets.Add("SurveySummary")
        With worksheet
            .Cells(rowColumnHeaders, colParticipantID).Value = "Participant " & vbCrLf & "ID"
            .Cells(rowColumnHeaders, colRespondentName).Value = "Respondent " & vbCrLf & "Name"
            .Cells(rowColumnHeaders, colRespondentEmail).Value = "Respondent " & vbCrLf & "Email"
            .Cells(rowColumnHeaders, colTotalAnswers).Value = "Total " & vbCrLf & "Answers"

            Dim row As Integer = 1
            For Each ARespondent As clsRespondent In ASurvey.Respondents
                'Debug.Print(ARespondent.ID & "  " & ARespondent.Name & "  " & ARespondent.Email & "  " & ARespondent.Answers.Count)
                .Cells(row, colParticipantID).Value = ARespondent.ID.ToString
                .Cells(row, colRespondentName).Value = ARespondent.Name
                .Cells(row, colRespondentEmail).Value = ARespondent.Email
                .Cells(row, colTotalAnswers).Value = ARespondent.Answers.Count.ToString
                row += 1
            Next

            'freeze top row
            .Panes = New WorksheetPanes(PanesState.Frozen, 0, 1, "A2", PanePosition.TopRight)

            'format
            .Columns(colParticipantID).AutoFit()
            .Columns(colRespondentName).AutoFit()
            .Columns(colRespondentEmail).AutoFit()
            .Columns(colTotalAnswers).AutoFit()

            Dim cellRange As CellRange = .Cells.GetSubrangeAbsolute(rowColumnHeaders, colParticipantID, rowColumnHeaders, colTotalAnswers)
            cellRange.Style.Font.Weight = ExcelFont.BoldWeight
            cellRange.Style.HorizontalAlignment = HorizontalAlignmentStyle.Center
            cellRange.Style.VerticalAlignment = VerticalAlignmentStyle.Center

            cellRange = .Cells.GetSubrangeAbsolute(rowColumnHeaders + 1, colParticipantID, row, colTotalAnswers)
            cellRange.Style.VerticalAlignment = VerticalAlignmentStyle.Top
            cellRange.Style.HorizontalAlignment = HorizontalAlignmentStyle.Left
            cellRange.Style.WrapText = True
        End With
        Return worksheet

    End Function

    Function AddSurveyAnswers(Options As ReportGeneratorOptions, ASurvey As clsSurvey) As ExcelWorksheet 'AS/19486p 'AS/19486s

        Dim answerExists As Boolean = False 'AS/19486s===
        For Each ARespondent As clsRespondent In ASurvey.Respondents
            If ARespondent.Answers.Count > 0 Then
                answerExists = True 'at least one answer exists
                Exit For
            End If
        Next 'AS/19486s==

        If answerExists Then 'AS/19486s
            Dim rowColumnHeaders As Integer = 0
            Dim colParticipantID As Integer = 0
            Dim colRespondentName As Integer = 1
            Dim colRespondentEmail As Integer = 2
            Dim colFirstAnswer As Integer = 3

            Dim worksheet = workbook.Worksheets.Add("SurveyAnswers")
            With worksheet
                .Cells(rowColumnHeaders, colParticipantID).Value = "Participant " & vbCrLf & "ID"
                .Cells(rowColumnHeaders, colRespondentName).Value = "Respondent " & vbCrLf & "Name"
                .Cells(rowColumnHeaders, colRespondentEmail).Value = "Respondent " & vbCrLf & "Email"

                Dim questionNumber As Integer = 0
                Dim NumOfQuestions As Integer = 0
                For Each APage As clsSurveyPage In ASurvey.Pages
                    For Each Question As clsQuestion In APage.Questions
                        If Question.Type <> QuestionType.qtComment Then
                            NumOfQuestions += 1
                        End If
                    Next
                Next
                For questionNumber = 1 To NumOfQuestions 'colFirstAnswer + NumOfQuestions
                    .Cells(rowColumnHeaders, questionNumber + colFirstAnswer - 1).Value = "Question " & questionNumber.ToString & vbCrLf & "response"
                Next

                Dim row As Integer = 1
                Dim col As Integer
                For Each ARespondent As clsRespondent In ASurvey.Respondents
                    If ARespondent.Answers.Count > 0 Then
                        'Debug.Print(ARespondent.ID  "  " & ARespondent.Name & "  " & ARespondent.Email & "  " & ARespondent.Answers.Count)
                        .Cells(row, colParticipantID).Value = ARespondent.ID.ToString
                        .Cells(row, colRespondentName).Value = ARespondent.Name
                        .Cells(row, colRespondentEmail).Value = ARespondent.Email
                        col = colFirstAnswer
                        For Each Page As clsSurveyPage In ASurvey.Pages
                            For Each Question As clsQuestion In Page.Questions
                                If ARespondent.AnswerByQuestionGUID(Question.AGUID) IsNot Nothing Then
                                    .Cells(row, col).Value = ARespondent.AnswerByQuestionGUID(Question.AGUID).AnswerValuesString
                                    col += 1
                                End If
                            Next
                        Next

                        row += 1
                    End If
                Next

                'freeze top row
                .Panes = New WorksheetPanes(PanesState.Frozen, 0, 1, "A2", PanePosition.TopRight)

                'format
                Dim cellRange As CellRange = .Cells.GetSubrangeAbsolute(rowColumnHeaders, colParticipantID, rowColumnHeaders, col - 1)
                cellRange.Style.Font.Weight = ExcelFont.BoldWeight
                cellRange.Style.HorizontalAlignment = HorizontalAlignmentStyle.Center
                cellRange.Style.VerticalAlignment = VerticalAlignmentStyle.Center

                cellRange = .Cells.GetSubrangeAbsolute(rowColumnHeaders + 1, colParticipantID, row, col - 1)
                cellRange.Style.VerticalAlignment = VerticalAlignmentStyle.Top
                cellRange.Style.HorizontalAlignment = HorizontalAlignmentStyle.Left
                cellRange.Style.WrapText = True

                For col = colParticipantID To col - 1
                    .Columns(col).AutoFit()
                Next
            End With

            Return worksheet
        End If

    End Function

#End Region

#Region "Supporting functions"

    Private Property NamePrefix As String = "Period #" 'AS/20761j

    Private Function getMaxPeriodsCount() As Integer 'AS/20761q
        Dim rv As Integer = 0
        For Each tID As Integer In RA.Scenarios.Scenarios.Keys
            Dim sc As RAScenario = RA.Scenarios.Scenarios(tID)
            If rv < sc.TimePeriods.Periods.Count Then rv = sc.TimePeriods.Periods.Count
        Next
        Return rv
    End Function

    Private Function getResourceAmount(sc As RAScenario, constraint As KeyValuePair(Of Integer, RAConstraint), AltTPResult As Integer, tAlt As RAAlternative) As Double 'AS/20761o 'AS/20761z aded AltTPResult

        Dim resourceAmount As Double = 0

        For Each tResKey As Guid In sc.TimePeriods.Resources.Keys
            Dim rs As RAResource = sc.TimePeriods.Resources(tResKey)
            If rs.Name = constraint.Value.Name Then
                Dim AltTPData = sc.TimePeriods.PeriodsData.GetAlternativePeriodsData(tAlt.ID)

                For i = AltTPData.MinPeriod To sc.TimePeriods.Periods.Count - 1
                    Dim tPeriod As RATimePeriod = sc.TimePeriods.Periods(i)
                    Dim tpStep As Integer = sc.TimePeriods.PeriodsStep

                    'period data (cost)
                    Dim tpCost As Double = UNDEFINED_INTEGER_VALUE
                    'tpCost = getPeriodCost(sc, i, tpStep, tPeriod.ID, AltTPData.MinPeriod, tAlt.ID, rs.ID) 'AS/20761z
                    tpCost = sc.TimePeriods.PeriodsData.GetResourceValue(i - AltTPResult, tAlt.ID, rs.ID) 'AS/20761z
                    If tpCost <> UNDEFINED_INTEGER_VALUE Then
                        resourceAmount = resourceAmount + tpCost
                    End If
                    'Debug.Print("getResourceAmount, " & sc.Name & "  " & rs.Name & "  " & tPeriod.Name & "  " & tAlt.Name & "  " & tpCost.ToString)
                    Exit For
                Next 'period

            End If
        Next 'resource
        Return resourceAmount

    End Function

    'Private Function getPeriodCost(sc As RAScenario, i As Integer, tpStep As Integer, tPeriodID As Integer, MinPeriod As Integer, tAltID As String, rsID As Guid) As Double 'AS/20761m 'AS/20761z removed
    '    Dim tpCost As Double = UNDEFINED_INTEGER_VALUE
    '    If Not (i > 0 And tpStep = 0) Then
    '        ' tpCost = sc.TimePeriods.PeriodsData.GetResourceValue(tPeriodID - MinPeriod, tAltID, rsID) 'AS/20761y
    '        tpCost = sc.TimePeriods.PeriodsData.GetResourceValue(tPeriodID, tAltID, rsID) 'AS/20761y
    '    End If
    '    Return tpCost
    'End Function

    Private Function getPeriodName(i As Integer, periodType As Integer, firstPeriod As Integer, periodStep As Integer, startDate As String) As String 'AS/20761l
        Dim periodVal = firstPeriod + (i * periodStep)
        Dim periodName = ChrW(80).ToString + periodVal.ToString 'ChrW(80) = "P" 

        If periodType <> 5 Then
            Dim monthNames() As String = {"January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"}

            If IsDate(startDate) Then
                Dim tpStartDate As DateTime = Convert.ToDateTime(startDate)
                ' Debug.Print(tpStartDate.Day & " " & tpStartDate.Month & "  " & tpStartDate.Year)

                Dim dd = tpStartDate.Day
                Dim mm = tpStartDate.Month
                Dim yyyy = tpStartDate.Year

                Select Case periodType'AS/20761l===
                    Case 0
                        dd = tpStartDate.Day + (i * periodStep)
                    Case 1
                        dd = tpStartDate.Day + (i * periodStep * 7)
                    Case 2
                        mm = tpStartDate.Month + (i * periodStep) '+ 1
                    Case 3
                        mm = tpStartDate.Month + (i * periodStep * 3) '+ 1
                    Case 4
                        yyyy = tpStartDate.Year + (i * periodStep)
                End Select 'AS/20761l==


                If (dd < 10) Then
                    dd = CInt((ChrW(48).ToString + dd.ToString))
                End If

                If (mm < 10) Then
                    mm = CInt(ChrW(48).ToString + mm.ToString)
                End If

                Select Case periodType
                    Case 0
                        periodName = mm.ToString + ChrW(47) + (dd.ToString + ChrW(47) + yyyy.ToString)
                    Case 1
                        periodName = mm.ToString + ChrW(47) + (dd.ToString + ChrW(47) + (yyyy.ToString + ChrW(32) + ((i + 1).ToString + ChrW(41).ToString)))
                    Case 2
                        periodName = monthNames(mm - 1) + ChrW(32).ToString + yyyy.ToString
                    Case 3
                        periodName = monthNames(mm - 1) + ChrW(32).ToString + (yyyy.ToString + ChrW(32) + ((i + 1).ToString + ChrW(41).ToString))
                    Case 4
                        periodName = yyyy.ToString
                End Select
            End If
        Else
            periodName = Replace(NamePrefix, "#", (periodVal + 1).ToString)
        End If

        Return periodName
    End Function

    Function md_5(ByVal file_name As String) As Object
        Dim hash = MD5.Create()
        Dim hashValue() As Byte
        Dim fileStream As FileStream = File.OpenRead(file_name)
        fileStream.Position = 0
        hashValue = hash.ComputeHash(fileStream)
        Dim hash_hex = PrintByteArray(hashValue)
        fileStream.Close()
        Return hash_hex
    End Function

    Public Function PrintByteArray(ByVal array() As Byte) As Object
        Dim hex_value As String = ""
        Dim i As Integer
        For i = 0 To array.Length - 1
            hex_value += array(i).ToString("X2")
        Next i
        Return hex_value.ToLower
    End Function

    Private Function getTPformat(TPtype As TimePeriodsType) As String 'AS/20761j
        Dim sFormat As String = ""
        Dim ID As Integer = 0

        Select Case TPtype
            Case TimePeriodsType.tptCustom
                'sFormat = CStr(If(NamePrefix.Contains("#"), NamePrefix.Replace("#", CStr(ID + 1)), String.Format("{0}{1}", NamePrefix, ID + 1))) 'AS/20761p
                sFormat = "Period"'AS/20761p
            Case TimePeriodsType.tptDay
                sFormat = "Days"
            Case TimePeriodsType.tptWeek
                sFormat = "Weeks"
            Case TimePeriodsType.tptMonth
                sFormat = "Months"
            Case TimePeriodsType.tptQuarter
                sFormat = "Quoters"
            Case TimePeriodsType.tptYear
                sFormat = "Years"
        End Select
        Return sFormat
    End Function

    Private Function getTPstartDate(sc As RAScenario, Optional ShortName As Boolean = False, Optional returnDate As Boolean = False) As String 'AS/20761j 'AS/20761l added returnDate

        Dim DT As DateTime = sc.TimePeriods.TimelineStartDate 'AS/20761l===
        If DT = DateTime.MinValue Then
            DT = DateTime.Now
        End If

        If returnDate Then
            Return DT.ToShortDateString
        End If 'AS/20761l==

        Dim TPtype As TimePeriodsType = sc.TimePeriods.PeriodsType
        Dim sDate As String = ""
        Dim ID As Integer = 0

        Select Case TPtype
            Case TimePeriodsType.tptCustom
                sDate = CStr(If(NamePrefix.Contains("#"), NamePrefix.Replace("#", CStr(ID + 1)), String.Format("{0}{1}", NamePrefix, ID + 1)))
            Case TimePeriodsType.tptDay
                sDate = DT.AddDays(ID).ToString(CStr(If(ShortName, "yy-MM-dd", "yyyy-MM-dd")))
            Case TimePeriodsType.tptMonth
                sDate = DT.AddMonths(ID).ToString(CStr(If(ShortName, "MMM yy", "MMMM yyyy")))
            Case TimePeriodsType.tptQuarter
                sDate = String.Format(CStr(If(ShortName, "Q{1}", "{0} (Q{1})")), DT.AddMonths(3 * ID).ToString("MMMM yyyy"), ID + 1)
            Case TimePeriodsType.tptWeek
                sDate = String.Format(CStr(If(ShortName, "Week {1}", "{0} (Week {1})")), DT.AddDays(7 * ID).ToShortDateString, ID + 1)
            Case TimePeriodsType.tptYear
                sDate = DT.AddYears(ID).ToString("yyyy")
        End Select
        Return sDate
    End Function
    Public Function isTimePeriodsVisible() As Boolean
        Return RA.Scenarios.ActiveScenario.TimePeriods.Periods.Count > 0
    End Function

    Private Function Equal(alts0 As List(Of Double), alts1 As List(Of Double)) As Boolean 'AS/20761i
        For i As Integer = 0 To alts0.Count - 1
            If alts0(i) <> alts1(i) Then Return False
        Next
        Return True
    End Function

    Private Sub AddRelevancyData(sName As String, sValue As String, isRelevant As Boolean, lstRelevancies As List(Of Tuple(Of Boolean, String, String))) 'AS/20761i
        Dim tRelevant As New Tuple(Of Boolean, String, String)(isRelevant, sName, sValue)
        lstRelevancies.Add(tRelevant)
    End Sub

    Private Function GetDependencyName(tDep As RADependency, tTPDep As RADependency, isTP As Boolean, Optional getFullAltName As Boolean = False) As String 'AS/20761i 'AS/21104a added getFullAltName
        Dim retVal As String = ""
        Dim tAlt0 As RAAlternative = RA.Scenarios.ActiveScenario.GetAvailableAlternativeById(tDep.FirstAlternativeID)
        Dim tAlt1 As RAAlternative = RA.Scenarios.ActiveScenario.GetAvailableAlternativeById(tDep.SecondAlternativeID)
        If tAlt0 IsNot Nothing AndAlso tAlt1 IsNot Nothing Then

            'Dim alt0name As String = GetShortAltName(tAlt0) 'AS/21104a===
            'Dim alt1name As String = GetShortAltName(tAlt1)
            Dim alt0name As String = CType(IIf(getFullAltName, tAlt0.Name, GetShortAltName(tAlt0)), String)
            Dim alt1name As String = CType(IIf(getFullAltName, tAlt1.Name, GetShortAltName(tAlt1)), String) 'AS/21104a==

            Select Case tDep.Value
                Case RADependencyType.dtDependsOn
                    retVal = String.Format("{0} Depends On {1}", alt0name, alt1name)
                    If isTP AndAlso tTPDep Is Nothing Then retVal += " (Non-concurrent)"
                    If isTP AndAlso tTPDep IsNot Nothing Then
                        Select Case tTPDep.Value
                            Case RADependencyType.dtConcurrent
                                retVal += " (Can be concurrent)"
                            Case RADependencyType.dtSuccessive
                                retVal += " (Non-concurrent)"
                            Case RADependencyType.dtLag
                                Select Case tTPDep.LagCondition
                                    Case LagCondition.lcEqual
                                        retVal += String.Format(" ({0} starts {2} periods after {1})", alt0name, alt1name, tTPDep.Lag)
                                    Case LagCondition.lcGreaterOrEqual
                                        retVal += String.Format(" ({0} starts at least {2} periods after {1})", alt0name, alt1name, tTPDep.Lag)
                                    Case LagCondition.lcLessOrEqual
                                        retVal += String.Format(" ({0} starts at most {2} periods after {1})", alt0name, alt1name, tTPDep.Lag)
                                    Case LagCondition.lcRange
                                        retVal += String.Format(" ({0} starts In the interval Of [{2} : {3}] periods after {1})", alt0name, alt1name, tTPDep.Lag, IIf(tTPDep.LagUpperBound = UNDEFINED_INTEGER_VALUE, "undefined", tTPDep.LagUpperBound))
                                End Select
                        End Select
                    End If
                Case RADependencyType.dtMutuallyDependent
                    retVal = String.Format("{0} Mutually Dependent with {1}", alt0name, alt1name)
                Case RADependencyType.dtMutuallyExclusive
                    retVal = String.Format("{0} Mutually Exclusive with {1}", alt0name, alt1name)
                Case RADependencyType.dtConcurrent
                    retVal = String.Format("{0} and {1} are Concurrent", alt0name, alt1name)
                Case RADependencyType.dtSuccessive
                    retVal = String.Format("{0} and {1} are Successive", alt0name, alt1name)
                Case RADependencyType.dtLag
                    retVal = String.Format("{0} starts {2} periods before {1} starts", alt0name, alt1name, IIf(tDep.Lag <> Integer.MinValue, tDep.Lag, "undefined"))
            End Select
        End If
        Return retVal
    End Function

    Private Function GetGroupName(tGroup As RAGroup) As String 'AS/20761i
        Dim sCondition As String = ""
        Select Case tGroup.Condition
            Case RAGroupCondition.gcLessOrEqualsOne
                sCondition = "LE1"
            Case RAGroupCondition.gcEqualsOne
                sCondition = "EQ1"
            Case RAGroupCondition.gcGreaterOrEqualsOne
                sCondition = "GE1"
        End Select
        Return String.Format("""{0}"" ({1})", tGroup.Name, sCondition)
    End Function
    Private Function getFundedList(alts As List(Of RAAlternative)) As List(Of Double) 'AS/20761i
        Dim retVal As List(Of Double) = New List(Of Double)
        For Each alt As RAAlternative In alts
            retVal.Add(alt.DisplayFunded) 'A0939
        Next
        Return retVal
    End Function

    Private Property currentScenarioID As Integer 'AS/20761h
        Get
            Return _currentScenarioID
        End Get
        Set(value As Integer)
            _currentScenarioID = value
        End Set
    End Property

    Public Property DecimalDigits As Integer 'AS/20761h from Reports.aspx.vb
        Get
            Return App.ActiveProject.ProjectManager.Parameters.DecimalDigits
        End Get
        Set(value As Integer)
            App.ActiveProject.ProjectManager.Parameters.DecimalDigits = value
            App.ActiveProject.ProjectManager.Parameters.Save()
        End Set
    End Property

    Private Function GetShortAltName(tAlt As RAAlternative) As String 'AS/20761h
        Return REPORT_ALT_PREFIX + tAlt.SortOrder.ToString 'tAlt.Name
    End Function

    Public Function SolverStateString(tSolver As RASolver) As String
        Dim sRes As String = ""
        If tSolver IsNot Nothing Then
            Select Case tSolver.SolverState
                Case raSolverState.raError
                    If Not String.IsNullOrEmpty(tSolver.LastError) Then
                        sRes = tSolver.LastError
                    End If
                Case raSolverState.raInfeasible
                    sRes = "The model is infeasible"
                Case raSolverState.raExceedLimits
                    sRes = "This model exceeds some of the capability of the default solver engine"
            End Select
        End If

        Return sRes
    End Function

    Private Sub InitAttributesList(AttributesList As List(Of clsAttribute)) 'AS/20761f
        If Not App.ActiveProject.IsRisk Then
            PM.Attributes.ReadAttributes(AttributesStorageType.astStreamsDatabase, PM.StorageManager.ProjectLocation, PM.StorageManager.ProviderType, PM.StorageManager.ModelID)
            AttributesList.Clear()
            Dim HasAttributes As Boolean = PM.Attributes IsNot Nothing AndAlso PM.Attributes.AttributesList IsNot Nothing AndAlso PM.Attributes.AttributesList.Count > 0
            If HasAttributes Then
                PM.Attributes.ReadAttributeValues(AttributesStorageType.astStreamsDatabase, PM.StorageManager.ProjectLocation, PM.StorageManager.ProviderType, PM.StorageManager.ModelID, -1)
                For Each attr In PM.Attributes.GetAlternativesAttributes
                    If attr.ValueType = AttributeValueTypes.avtEnumeration OrElse attr.ValueType = AttributeValueTypes.avtEnumerationMulti Then
                        AttributesList.Add(attr)
                    End If
                Next
            End If
        End If
    End Sub

    Public Property SelectedCategoryID(AttributesList As List(Of clsAttribute)) As Guid 'AS/20761f
        Get
            Dim retVal As Guid = Guid.Empty
            Dim s = SessVar(SESSION_SELECTED_CATEGORY_GUID)
            If Not String.IsNullOrEmpty(s) Then retVal = New Guid(s)
            If (retVal.Equals(Guid.Empty) OrElse AttributesList.Where(Function(attr) attr.ID.Equals(retVal)).Count = 0) AndAlso AttributesList.Count > 0 Then retVal = AttributesList(0).ID
            Return retVal
        End Get
        Set(value As Guid)
            SessVar(SESSION_SELECTED_CATEGORY_GUID) = value.ToString
        End Set
    End Property

    ReadOnly Property SESSION_SELECTED_CATEGORY_GUID As String 'AS/20761f
        Get
            Return String.Format("RA_SelecteCategoryGuid_{0}", App.ProjectID)
        End Get
    End Property

    Private Function getConstraintLinkedInfo(CC As RAConstraint) As String 'AS/20761d
        Dim sLinkInfo As String = ""
        If CC.LinkedAttributeID.Equals(Guid.Empty) Then
            sLinkInfo = "no link"
        ElseIf CC.IsLinked And Not CC.IsReadOnly Then
            sLinkInfo = "link only"
        ElseIf CC.IsLinked And CC.IsReadOnly Then
            sLinkInfo = "link w auto updating"
        End If
        Return sLinkInfo
    End Function

    Private Function getAlternativeGUIDbyID(ID As Integer) As Guid 'AS/20761c
        For Each alt As clsNode In altH.Nodes
            If alt.NodeID = ID Then
                Return alt.NodeGuidID
            End If
        Next
        Return Guid.Empty

    End Function

    Private Function getAlternativeIDbyGUID(guid As String) As Integer 'AS/20761c
        For Each alt As clsNode In altH.Nodes
            If alt.NodeGuidID.ToString = guid Then
                Return alt.NodeID
            End If
        Next
        Return UNDEFINED_INTEGER_VALUE

    End Function

    Private Function ReplaceSpecialChars(profileString As String) As String 'AS/20761b
        'source: https://www.vbforums.com/showthread.php?663684-RESOLVED-Replace-special-XML-characters

        profileString = profileString.Replace("&amp;", "&")
        profileString = profileString.Replace("&gt;", ">")
        profileString = profileString.Replace("&lt;", "<")
        profileString = profileString.Replace("%", "%")
        profileString = profileString.Replace("&quot;", "\")
        profileString = profileString.Replace(" & apos;", "'")
        profileString = profileString.Replace("&reg;", "®") ' -This did Not work On his End
        profileString = profileString.Replace("-REGSYM-", "®")
        profileString = profileString.Replace("-COPYSYM-", "©")
        profileString = profileString.Replace("-TMSYM-", "™")
        profileString = profileString.Replace("-ARROWSYM-", "«")
        Return profileString
    End Function

    ReadOnly Property Optimizer As RASolver 'AS/20761a
        Get
            Return RA.Solver
        End Get
    End Property

    Private Function GetAltName(sID As String) As String 'AS/20761a
        For Each tAlt As RAAlternative In Optimizer.Alternatives
            If tAlt.ID = sID Then Return ShortString(SafeFormString(tAlt.Name), OPT_ALT_NAME_LEN, True)
        Next
        For Each tGroup As KeyValuePair(Of String, RAGroup) In Optimizer.Groups.Groups
            If tGroup.Value.ID = sID Then Return ShortString(SafeFormString(tGroup.Value.Name), OPT_ALT_NAME_LEN, True)
        Next
        Return ""
    End Function

    Private Sub InsertInfodocImages(worksheet As ExcelWorksheet, ByVal InfoDocType As reObjectType, ByVal sInfodocID As String, ByVal sContent As String,
                                    ByRef col As Integer, row As Integer, WRTParentNode As Integer, ByRef colRightMostColumn As Integer,
                                    lstInfoPictures As List(Of KeyValuePair(Of String, ExcelPicture))) 'AS/19486_i 'AS/21022a: changed to ByRef col, ByRef colRightMostColumn

        Dim MyComputer As New Devices.Computer
        Dim ProjectID As Integer = App.ActiveProject.ID
        Dim ActiveHierarchyID As Integer = PM.ActiveHierarchy

        With worksheet
            Dim anchor As AnchorCell = New AnchorCell(.Columns(col), .Rows(row), True)
            Dim picPosition As String = anchor.Column.Name & anchor.Row.Name

            If sContent <> "" Then
                sContent = Infodoc_Unpack(ProjectID, ActiveHierarchyID, InfoDocType, sInfodocID, sContent, True, True, WRTParentNode)

                Dim path As String = Infodoc_Path(ProjectID, ActiveHierarchyID, InfoDocType, sInfodocID, WRTParentNode)
                Dim files As ReadOnlyCollection(Of String)

                'get image files - *.gif", "*.png", ".bmp", "*.jpeg", "*.jpg" 
                files = MyComputer.FileSystem.GetFiles(path, FileIO.SearchOption.SearchAllSubDirectories, _MHT_SearchFilesList)

                Dim lstCheckSum As New List(Of String)
                For Each sFile As String In files
                    col += 1 'AS/21022a moved up
                    Dim mediaFileData As FileInfo = MyComputer.FileSystem.GetFileInfo(sFile)

                    Dim md As Object = md_5(sFile) 'AS/21039b=== 'source: stackoverflow.com/questions/7930302/checking-the-md5-of-file-in-vb-net
                    If Not lstCheckSum.Contains(md.ToString) Then
                        lstCheckSum.Add(md.ToString) 'AS/21039b==

                        anchor = New AnchorCell(.Columns(col), .Rows(row), True)
                        picPosition = anchor.Column.Name & anchor.Row.Name
                        Dim pic As ExcelPicture = .Pictures.Add(mediaFileData.FullName, picPosition)
                        lstInfoPictures.Add(New KeyValuePair(Of String, ExcelPicture)(mediaFileData.Extension, pic))
                    End If 'AS/21039b

                    'col += 1 'AS/21022a moved up
                Next

                'get images downloaded as part of HTM file
                Dim imageList As List(Of KeyValuePair(Of String, MemoryStream)) = Infodoc_GetInlineImages(sContent) ' D6952
                For Each imagePair As KeyValuePair(Of String, MemoryStream) In imageList
                    col += 1 'AS/21022a moved up
                    Dim imageType As String = imagePair.Key
                    Dim imageStream As MemoryStream = imagePair.Value
                    Dim anchor1 = New AnchorCell(.Columns(col), .Rows(row), True)
                    Dim anchor2 = New AnchorCell(.Columns(col + 1), .Rows(row + 1), True)
                    Dim picType As ExcelPictureFormat = getExcelPictureFormat(imageType)

                    Dim pic = .Pictures.Add(imageStream, picType, anchor1, anchor2)
                    lstInfoPictures.Add(New KeyValuePair(Of String, ExcelPicture)("htm", pic))

                    'col += 1 'AS/21022a moved up
                Next
                'If colRightMostColumn < col - 1 Then colRightMostColumn = col - 1 'AS/19203f== 'AS/21022a
                If colRightMostColumn < col Then colRightMostColumn = col 'AS/21022a

            End If 'AS/19203j=
        End With

    End Sub

    Private Function getNumOfComparisons(NodeID As Guid) As String 'AS/19486t
        'see function 
        Dim NumOfComparisons As DiagonalsEvaluation = DiagonalsEvaluation.deAll
        NumOfComparisons = CType(PM.Attributes.GetAttributeValue(ATTRIBUTE_EVALUATE_DIAGONALS_MODE_ID, NodeID), DiagonalsEvaluation)

        Dim NumOfComparisonsToText As String = "All pairs"
        Select Case NumOfComparisons
            Case DiagonalsEvaluation.deAll
                NumOfComparisonsToText = "All pairs"
            Case DiagonalsEvaluation.deFirst
                NumOfComparisonsToText = "One diagonal"
            Case DiagonalsEvaluation.deFirstAndSecond
                NumOfComparisonsToText = "Two diagonals"
        End Select
        Return NumOfComparisonsToText

    End Function

    Private Function getQuestionTypeString(question As clsQuestion) As String 'AS/19486n
        Dim sType As String = ""
        Select Case question.Type
            Case QuestionType.qtAlternativesSelect
                sType = "Drop Down List"
            Case QuestionType.qtComment
                sType = "Comment"
            Case QuestionType.qtImageCheck
                sType = "Image Check List"
            Case QuestionType.qtMatrixCheck
                sType = "Matrix Check List"
            Case QuestionType.qtMatrixOpen
                sType = "Open Matrix"
            Case QuestionType.qtMatrixVariantsCombo
                sType = "Matrix Drop Down"
            Case QuestionType.qtNumber
                sType = "Number"
            Case QuestionType.qtNumberColumn
                sType = "Number Column"
            Case QuestionType.qtObjectivesSelect
                sType = "Drop Down List"
            Case QuestionType.qtOpenLine
                sType = "Open Line"
            Case QuestionType.qtOpenMemo
                sType = "Open Memo"
            Case QuestionType.qtPairwiseScale
                sType = "Pairwise Scale"
            Case QuestionType.qtRatingScale
                sType = "Rating Scale"
            Case QuestionType.qtVariantsCheck
                sType = "Check List"
            Case QuestionType.qtVariantsCombo
                sType = "Drop Down List"
            Case QuestionType.qtVariantsRadio
                sType = "Radio Buttons"
        End Select
        'sType = App.ResString("Survey_qtComment")
        Return sType
    End Function

    Private Sub AdjustImages(ws As ExcelWorksheet, lstInfoPictures As List(Of KeyValuePair(Of String, ExcelPicture))) 'AS/19203j

        For Each pair As KeyValuePair(Of String, ExcelPicture) In lstInfoPictures
            Dim pic As ExcelPicture = pair.Value
            Dim picType As String = pair.Key

            Dim position As ExcelDrawingPosition = pic.Position
            Dim picRow As Integer = position.From.Row.Index
            Dim picCol As Integer = position.From.Column.Index

            'get image height and width by creating bitmap 'AS/19486_c===
            Dim picRatio As Double = 1
            Try 'AS/19486_e enclosed to avoid rte in some cases
                Dim bmp As Bitmap = New Bitmap(pic.ToImage)
                picRatio = bmp.Width / bmp.Height 'AS/19486_c==
            Catch
                picRatio = 1
            End Try

            Select Case picType
                Case "htm"
                    Dim colWidth = ws.Columns(picCol - 1).GetWidth(LengthUnit.Point) 'AS/19203i===

                    'set column width = width of the previous column (that can be colInfodoc or prev picture)
                    ws.Columns(picCol).SetWidth(colWidth / 2, LengthUnit.Point) 'AS/19486_c added /2

                    'set position width = column width
                    colWidth = ws.Columns(picCol).GetWidth(LengthUnit.Point)
                    position.SetWidth(colWidth, LengthUnit.Point)

                    'set position height and padding
                    position.SetHeight(colWidth / picRatio, LengthUnit.Point)
                    'position.From.SetRowOffset(5, LengthUnit.Point) 'AS/19486_c===
                    'position.From.SetColumnOffset(5, LengthUnit.Point)
                    'position.To.SetColumnOffset(-5, LengthUnit.Point) 'AS/19486_c==

                    'adjust row height if needed
                    Dim rowHeight = ws.Rows(picRow).GetHeight(LengthUnit.Point)
                    If position.Height > rowHeight Then
                        ws.Rows(picRow).SetHeight(position.Height, LengthUnit.Point)
                        position.SetHeight(colWidth / picRatio, LengthUnit.Point) 'AS/19486_c -- pic height followed the row height so need to set it back
                    End If 'AS/19203i==

                    ''===== Set picture in the cell with padding (offset) on top and bottom =====
                    'Dim rowOffsetFrom As Integer = CInt((rowHeight - picH) / 2) '200
                    'Dim rowOffsetTo As Integer = -CInt((rowHeight - picH) / 2) '-200

                    'position.From.SetRowOffset(rowOffsetFrom, LengthUnit.Point)
                    'position.To.SetRowOffset(rowOffsetTo, LengthUnit.Point)

                Case Else
                    ws.Columns(picCol).SetWidth(position.Width, LengthUnit.Point)
                    '.Rows(picRow).AutoFit() 

                    Dim rowHeight = ws.Rows(picRow).GetHeight(LengthUnit.Point)
                    Dim positionHeight = position.GetHeight(LengthUnit.Point)

                    'Debug.Print(rowHeight.ToString & "  " & positionHeight.ToString)

                    If position.Height > rowHeight Then
                        ws.Rows(picRow).SetHeight(position.Height, LengthUnit.Point)
                    End If
            End Select
        Next

    End Sub

    Private Sub AdjustImages(ws As ExcelWorksheet, lstInfoPictures As List(Of Tuple(Of String, Integer, ExcelPicture))) 'AS/19486_b

        For Each t As Tuple(Of String, Integer, ExcelPicture) In lstInfoPictures
            Dim picType As String = t.Item1
            Dim picCol As Integer = t.Item2
            Dim pic As ExcelPicture = t.Item3

            Dim position As ExcelDrawingPosition = pic.Position
            Dim picRow As Integer = position.From.Row.Index
            'Dim picCol As Integer = position.From.Column.Index

            'get image height and width by creating bitmap 
            Dim bmp As Bitmap = New Bitmap(pic.ToImage)
            Dim picW As Integer = bmp.Width
            Dim picH As Integer = bmp.Height

            Select Case picType
                Case "htm"
                    Dim picRatio = picW / picH

                    Dim colWidth = ws.Columns(picCol - 1).GetWidth(LengthUnit.Point) 'AS/19203i===
                    'set column width = width of the previous column (that can be colInfodoc or prev picture)
                    ws.Columns(picCol).SetWidth(colWidth, LengthUnit.Point)
                    '.Rows(picRow).AutoFit() 

                    'set position width = column width
                    colWidth = ws.Columns(picCol).GetWidth(LengthUnit.Point)
                    position.SetWidth(colWidth, LengthUnit.Point)

                    'set position height and padding
                    position.SetHeight(colWidth / picRatio, LengthUnit.Point)
                    position.From.SetRowOffset(5, LengthUnit.Point)
                    position.From.SetColumnOffset(5, LengthUnit.Point)
                    position.To.SetColumnOffset(-5, LengthUnit.Point)

                    'adjust row height if needed
                    Dim rowHeight = ws.Rows(picRow).GetHeight(LengthUnit.Point)
                    If position.Height > rowHeight Then
                        ws.Rows(picRow).SetHeight(position.Height, LengthUnit.Point)
                    End If 'AS/19203i==

                    ''===== Set picture in the cell with padding (offset) on top and bottom =====
                    'Dim rowOffsetFrom As Integer = CInt((rowHeight - picH) / 2) '200
                    'Dim rowOffsetTo As Integer = -CInt((rowHeight - picH) / 2) '-200

                    'position.From.SetRowOffset(rowOffsetFrom, LengthUnit.Point)
                    'position.To.SetRowOffset(rowOffsetTo, LengthUnit.Point)

                Case Else
                    ws.Columns(picCol).SetWidth(position.Width, LengthUnit.Point)
                    '.Rows(picRow).AutoFit() 

                    Dim rowHeight = ws.Rows(picRow).GetHeight(LengthUnit.Point)
                    Dim positionHeight = position.GetHeight(LengthUnit.Point)

                    'Debug.Print(rowHeight.ToString & "  " & positionHeight.ToString)

                    If position.Height > rowHeight Then
                        ws.Rows(picRow).SetHeight(position.Height, LengthUnit.Point)
                    End If
            End Select
        Next

    End Sub

    Private Sub getImageSize() 'AS/19203g
        'get image height and width by creating bitmap 
        Dim bmp As Bitmap = New Bitmap("D:\ECC Alpha\Application\Images\favicon\Dices.png")
        Dim picW As Integer = bmp.Width
        Dim picH As Integer = bmp.Height

        'OR ANOTHER WAY: get image height and width WITHOUT creating bitmap
        Dim fullSizeImg As System.Drawing.Image = System.Drawing.Image.FromFile("D:\ECC Alpha\Application\Images\favicon\Dices.png") 'source: https://forums.asp.net/t/1460262.aspx?get+image+height+and+width+without+creating+bitmap
        Debug.Print("Width (no bmp) :  " & fullSizeImg.Width.ToString & "  Height (no bmp): " & fullSizeImg.Height.ToString)
    End Sub

    Private Function getExcelPictureFormat(imageType As String) As ExcelPictureFormat 'AS/19203f
        Dim picFormat As ExcelPictureFormat
        Select Case imageType
            Case "Jpeg"
                picFormat = ExcelPictureFormat.Jpeg
            Case "Jpeg"
                picFormat = ExcelPictureFormat.Png
            Case "Jpeg"
                picFormat = ExcelPictureFormat.Tiff
            Case "Jpeg"
                picFormat = ExcelPictureFormat.Gif
            Case "Jpeg"
                picFormat = ExcelPictureFormat.Emf
            Case "Jpeg"
                picFormat = ExcelPictureFormat.Wmf
            Case "Jpeg"
                picFormat = ExcelPictureFormat.Bmp
            Case "Jpeg"
                picFormat = ExcelPictureFormat.Exif
            Case "Jpeg"
                picFormat = ExcelPictureFormat.Ico
        End Select
        Return picFormat
    End Function

    Private Function getAllUsersAndGroupsIDs() As List(Of Integer) 'AS/19486b
        Dim users As List(Of Integer) = New List(Of Integer)
        users.Add(COMBINED_USER_ID)
        Dim groups As List(Of clsGroup) = PM.CombinedGroups.GroupsList
        For Each group As clsCombinedGroup In groups
            If group.CombinedUserID <> COMBINED_USER_ID Then
                users.Add(group.CombinedUserID)
            End If
        Next

        For Each user As clsUser In PM.UsersList
            users.Add(user.UserID)
        Next

        Return users

    End Function

    Private Property getSelectedUsersAndGroupsIDs As List(Of Integer) 'AS/19203c
        Get
            Dim sUsers As String = StringSelectedUsersAndGroupsIDs
            If String.IsNullOrEmpty(sUsers) Then Return Nothing
            Dim tList As String() = sUsers.Split(CChar(","))
            If tList.Count > 0 Then
                Dim res As New List(Of Integer)
                For Each item In tList
                    Dim tUserID As Integer
                    If Integer.TryParse(item, tUserID) AndAlso (PM.UserExists(tUserID) OrElse PM.CombinedGroups.CombinedGroupExists(tUserID)) Then
                        res.Add(tUserID)
                    End If
                Next
                If res.Count = 0 Then res.Add(COMBINED_USER_ID)
                Return res
            End If
            Dim auto = New List(Of Integer)
            auto.Add(COMBINED_USER_ID)
            Return auto
        End Get
        Set(value As List(Of Integer))
            Dim sUsers As String = ""
            For i As Integer = 0 To value.Count - 1
                sUsers = sUsers + value(i).ToString
                If i <> value.Count - 1 Then sUsers += ","
            Next
            WriteSetting(App.ActiveProject, ATTRIBUTE_SYNTHESIS_USERS_ID, AttributeValueTypes.avtString, sUsers)
        End Set
    End Property

    Private Property StringSelectedUsersAndGroupsIDs As String 'AS/19203c
        Get
            Dim value As String = CStr(PM.Attributes.GetAttributeValue(ATTRIBUTE_SYNTHESIS_USERS_ID, UNDEFINED_USER_ID))
            If String.IsNullOrEmpty(value) OrElse value = UNDEFINED_USER_ID.ToString Then value = COMBINED_USER_ID.ToString
            Return value
        End Get
        Set(value As String)
            If String.IsNullOrEmpty(value) OrElse value = UNDEFINED_USER_ID.ToString Then value = COMBINED_USER_ID.ToString
            WriteSetting(App.ActiveProject, ATTRIBUTE_SYNTHESIS_USERS_ID, AttributeValueTypes.avtString, value)
        End Set
    End Property

    Private Sub PrepareUsersData(ByRef userIDs As ArrayList) ''AS/18712x
        Dim user As clsUser = Nothing
        Dim Group As clsCombinedGroup = PM.CombinedGroups.GetCombinedGroupByUserID(-1)
        userIDs = GetUserIDsList(user, Group)

        For Each UserID As Integer In userIDs
            Dim u As clsUser = PM.GetUserByID(UserID)
            PM.StorageManager.Reader.LoadUserData(u)
        Next
    End Sub

    Private Function GetMeasureTypeName(mt As ECMeasureType) As String 'AS/18712l
        Dim s As String = "mt"
        Select Case mt
            Case ECMeasureType.mtNone '= -1
                s = "None"
            Case ECMeasureType.mtPairwise '= 0
                s = "Pairwise"
            Case ECMeasureType.mtRatings '= 1
                s = "Ratings"
            Case ECMeasureType.mtRegularUtilityCurve '= 2
                s = "Regular Utility Curve"
            Case ECMeasureType.mtCustomUtilityCurve '= 4
                s = "Custom Utility Curve"
            Case ECMeasureType.mtDirect '= 5
                s = "Direct"
            Case ECMeasureType.mtStep '= 6
                s = "Step Function"
            Case ECMeasureType.mtAdvancedUtilityCurve '= 7 'C0025
                s = "Advanced Utility Curve"
            Case ECMeasureType.mtPWOutcomes '= 8
                s = "PW Outcomes"
            Case ECMeasureType.mtPWAnalogous '= 10
                s = "PW Analogous"
        End Select
        Return s
    End Function

    Private Function GetUserIDsList(User As clsUser, Group As clsCombinedGroup) As ArrayList 'AS/18712h copied from clsProjectDataProvider
        Dim res As New ArrayList
        If User IsNot Nothing Then
            res.Add(User.UserID)
        Else
            If Group IsNot Nothing Then
                For Each u As clsUser In Group.UsersList
                    res.Add(u.UserID)
                Next
            End If
        End If
        Return res
    End Function

    Private Sub GetFullNodePath(ByVal node As clsNode, ByRef NodePath As String, Optional ByVal NodePathDelimiter As String = NODE_PATH_DELIMITER)  'AS/18712h copied from clsProjectDataProvider
        If node Is Nothing Then
            Exit Sub
        End If

        If node.ParentNode Is Nothing Then
            NodePath = node.NodeName + NodePath
        Else
            NodePath = NodePathDelimiter + node.NodeName + NodePath 'C0389
            GetFullNodePath(node.ParentNode, NodePath)
        End If
    End Sub

    Sub getCellSize(tbText As String, fontName As String, ByRef W As Double, ByRef H As Double, FSize As Single) 'AS/18712g
        'source -- vbcity.com/forums/t/163824.aspx
        'TIP (AS): to speed up the calculation, do it just once for 10 characters and then proportionally apply to the actual number of charachters
        Dim stringFont As System.Drawing.Font = New System.Drawing.Font(fontName, FSize)
        Dim s As System.Drawing.Size = System.Windows.Forms.TextRenderer.MeasureText(tbText, stringFont)
        W = s.Width
        H = s.Height * 1.3
    End Sub

    Private Function GetJudgment(ByVal CovObj As clsNode, altID As Integer, userID As Integer) As clsNonPairwiseMeasureData 'AS/18387a
        Return CType(CovObj.Judgments, clsNonPairwiseJudgments).GetJudgement(altID, CovObj.NodeID, userID)
    End Function

    Private Function getSBCoef(sc As RAScenario) As Double 'AS/22708
        Dim coef As Double = 1
        Dim _norm = NormalizeMode
        If _norm > 0 Then
            Select Case _norm
                Case 1
                    coef = Double.MinValue
                Case 2
                    coef = 0
                Case 3
                    coef = Double.MaxValue
            End Select

            For Each alt As RAAlternative In sc.Alternatives
                Select Case _norm
                    Case 1
                        If alt.SBPriority > coef Then coef = alt.SBPriority
                    Case 2
                        coef += alt.SBPriority
                    Case 3
                        If alt.SBPriority < coef Then coef = alt.SBPriority
                End Select
            Next
        End If
        Return coef
    End Function

    Private ReadOnly Property NormalizeMode As Integer 'AS/22708
        Get
            Return CInt(App.ActiveProject.ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_RA_SB_NORMALIZATION_MODE_ID, UNDEFINED_USER_ID))
        End Get
    End Property
#End Region

#Region "Testing and debugging functions"

    Private Sub Debug_getNodesIDs() 'AS/19486v
        Dim nodeName As String = ""
        Dim nodeID As String = ""
        Dim parentName As String = ""
        Dim parentID As String = ""

        For Each node In objH.Nodes
            nodeName = node.NodeName
            nodeID = node.NodeID.ToString
            parentName = node.ParentNode.NodeName
            parentID = node.ParentNodeID.ToString

            Debug.Print(nodeName & "  " & nodeID & ", " & parentName & "  " & parentID)
        Next
    End Sub

    Private Function Debug_Timing(sTimePrep As String, sTimeReport As String) As ExcelWorksheet 'AS/19486r
        'adds new sheet with the timing results
        Dim worksheet = workbook.Worksheets.Add("Debug_Timing")

        Dim rowModelName As Integer = 0
        Dim rowTimingResults As Integer = 2
        Dim colTipmePrep As Integer = 0
        Dim colTimeReports As Integer = 1

        With worksheet

            'write model name
            .Cells(rowModelName, colTipmePrep).Value = "Model: " & PM.ProjectName
            Dim cellRange As CellRange = .Cells.GetSubrangeAbsolute(rowModelName, colTipmePrep, rowModelName, colTimeReports)
            cellRange.Merged = True

            'write timing results
            .Cells(rowTimingResults, colTipmePrep).Value = sTimePrep
            .Cells(rowTimingResults, colTimeReports).Value = sTimeReport

            'format
            Dim fSize As Double = .Cells(rowModelName, colTipmePrep).Style.Font.Size / 20
            Dim fName As String = .Columns(colTipmePrep).Style.Font.Name

            Dim colW As Double, rowH As Double
            getCellSize("6 ContributionMatrix: 0 (no alternatives in the model)", fName, colW, rowH, CSng(fSize.ToString))

            .Columns(colTipmePrep).SetWidth(colW, LengthUnit.Point)
            .Columns(colTipmePrep).Style.WrapText = True

            .Columns(colTimeReports).SetWidth(colW, LengthUnit.Point)
            .Columns(colTimeReports).Style.WrapText = True

        End With

        Return worksheet
    End Function

    Function SurveyTest() As Boolean
        Dim AUsersList As New Dictionary(Of String, clsComparionUser)
        Dim AGroup = CType(App.ActiveProject.ProjectManager.CombinedGroups.GroupsList(0), clsCombinedGroup)
        For Each AUser In AGroup.UsersList
            AUsersList.Add(AUser.UserEMail, New clsComparionUser() With {.ID = AUser.UserID, .UserName = AUser.UserName})
        Next
        Dim fSurveyInfo As clsSurveyInfo = App.SurveysManager.GetSurveyInfoByProjectID(App.ProjectID, SurveyType.stWelcomeSurvey, AUsersList)
        Dim ASurvey As clsSurvey = fSurveyInfo.Survey("")
        For Each APage As clsSurveyPage In ASurvey.Pages
            For Each AQuestion As clsQuestion In APage.Questions
                If AQuestion.Type <> QuestionType.qtComment And AQuestion.Type <> QuestionType.qtAlternativesSelect And AQuestion.Type <> QuestionType.qtObjectivesSelect Then
                    Dim QText As String = ""
                    If isMHT(AQuestion.Text) Then
                        QText = HTML2Text(Infodoc_Unpack(fSurveyInfo.ProjectID, 0, reObjectType.SurveyQuestion, AQuestion.AGUID.ToString, AQuestion.Text, True, True, -1))
                    Else
                        If clsComparionCorePage.OPT_SURVEY_PARSE_LINKS Then QText = ParseTextHyperlinks(AQuestion.Text) Else QText = AQuestion.Text
                    End If
                    Debug.Print("QText = " & QText)
                    If AQuestion.AllowVariants Then
                        For Each AVariant As clsVariant In AQuestion.Variants
                            Dim AVariantText As String = ""
                            AVariantText = AVariant.Text
                            Debug.Print("AVariantText = " & AVariantText)
                        Next
                    End If
                End If
            Next
        Next

        Return True
    End Function
#End Region

    Public Sub New(_App As clsComparionCore, GridWRTNodeID As Integer)

        MyBase.New(_PGID_REPORT_COMBINED) 'AS/20761b

        SpreadsheetInfo.SetLicense("SN-2019Oct23-AKAqJAF7VQbNwpzNxMw+YMGUPQKtiac8SV2w3r4AR43N/ctUteUub15BB7pw2rqTqZB6PzdDiG4vHXzKw5SRVIMv9Gw==A")

        workbook = New ExcelFile
        App = _App
        PM = App.ActiveProject.ProjectManager
        RA = PM.ResourceAligner 'AS/20761a
        PM.ProjectName = App.ActiveProject.ProjectName
        ecColors = New ECColors
        WRTNodeID = GridWRTNodeID

    End Sub

End Class