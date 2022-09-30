'Imports GemBox.Document
'Imports System.Globalization
'Imports GemBox.Document.Drawing
'Imports GemBox.Document.Tables
Imports System.IO
Imports GemBox.Spreadsheet
Imports GemBox.Spreadsheet.Charts
Imports GemBox.Spreadsheet.Drawing 'AS/17533f
Imports System.Drawing 'AS/18387a
Imports GemBox.Spreadsheet.ConditionalFormatting 'AS/18712e

<Serializable> Public Class GemboxTestSpreadsheet
    Private App As clsComparionCore
    Private PM As clsProjectManager
    Private workbook As ExcelFile

    Function GenerateExcelReport(filePath As String, server As HttpServerUtility, Options As ReportGeneratorOptions, UserID As Integer) As ExcelFile  'AS/18387a 'AS/18387d added UserID

        'workbook = GetDatagridWorkbook(filePath, server, Options, UserID) 'AS/18712b
        Dim UserIDs As New List(Of Integer) 'AS/18712b===
        UserIDs.Add(-1)
        UserIDs.Add(-1000)
        UserIDs.Add(2)
        workbook = GetDatagridWorkbook(filePath, server, Options, UserIDs) 'AS/18712b==
        Return workbook
    End Function

    Function GetDatagridWorkbook(filePath As String, server As HttpServerUtility, Options As ReportGeneratorOptions, UserIDs As List(Of Integer)) As ExcelFile 'AS/18387a 'AS/18387d added UserID 'AS/18712b replaces UserID with list if UserID's

        'UserID = -1001
        'UserID = -1002
        'UserID = 2
        'UserID = -1

        For Each UserID As Integer In UserIDs 'AS/18712b enclosed
            'set sheet name 'AS/18712a===
            Dim sheetName As String = "Data Grid"
            Select Case UserID
                Case -1 'combined 
                    sheetName = "All Participants"
                Case >= 0 'individual user
                    sheetName = PM.GetUserByID(UserID).UserName
                Case < -1 'group
                    sheetName = PM.CombinedGroups.GetCombinedGroupByUserID(UserID).Name
            End Select

            Dim worksheet = workbook.Worksheets.Add(sheetName)

            'calc priorities for Goal for UserID
            Dim wrtNode As clsNode = PM.ActiveObjectives.GetNodeByID(1)
            PM.StorageManager.Reader.LoadUserData(PM.GetUserByID(UserID))
            Dim CalcTarget As clsCalculationTarget = PM.CalculationsManager.GetCalculationTargetByUserID(UserID)
            PM.CalculationsManager.Calculate(CalcTarget, wrtNode, PM.ActiveHierarchy, PM.ActiveAltsHierarchy) 'AS/18712a==

            'Get lists of objectives, alternatives, and attributes
            Dim objH As clsHierarchy = App.ActiveProject.HierarchyObjectives
            Dim altH As clsHierarchy = PM.AltsHierarchy(PM.ActiveAltsHierarchy)
            Dim customAttributes As List(Of clsAttribute) = PM.Attributes.GetAlternativesAttributes(True)

            Dim row As Integer = 3 'row number; start i from 3 to add empty row(s) to be able to merge cells in the headers later when formatting 
            Dim col As Integer = 0 ' col number

            'special rows:
            Dim rowModelName As Integer = 0
            Dim rowColsGroups As Integer = 1 'for now it includes "Alternatives", "Atributes", and "Goal: <goal name>"
            Dim rowColName As Integer = 2 'names of attributes and covering objectives
            'special columns:
            Dim colAltName As Integer = 0
            Dim colCost As Integer = 1 'colAltName + abs(cint(Option.IncludeCost))
            Dim colRisks As Integer = 2 'colAltName + abs(cint(Option.IncludeCost)) +  abs(cint(Option.IncludeRisks))
            'Dim colMapKey As Integer
            'Dim colStartDate As Integer
            'Dim colFinishDate As Integer
            Dim colTotal As Integer = colRisks + customAttributes.Count + 1 '- abs(cint(Option.IncludeCost)) - abs(cint(Option.IncludeRisks) 'AS/18712a
            Dim colFirstCovObjective As Integer = colTotal + 1
            Dim colLastCovObjective As Integer = 0 'to be set up below
            Dim maxLenAltName As Integer = 10
            Dim maxLenAttrName As Integer = 10

            'write columns groups names
            worksheet.Cells(rowModelName, colAltName).Value = "Model name: " & PM.ProjectName
            worksheet.Cells(rowColsGroups, colAltName).Value = "Alternatives"
            worksheet.Cells(rowColsGroups, colCost).Value = "Attributes"
            worksheet.Cells(rowColsGroups, colFirstCovObjective + 1).Value = objH.Nodes(0).NodeName

            'write headers for attribute columns
            worksheet.Cells(rowColName, colCost).Value = "Cost"
            worksheet.Cells(rowColName, colRisks).Value = "P.Failure"

            For Each alt As clsNode In altH.TerminalNodes
                'write values into Altenatives and Total columns
                worksheet.Cells(row, colAltName).Value = alt.NodeName
                If maxLenAltName < Len(alt.NodeName) Then maxLenAltName = Len(alt.NodeName)

                worksheet.Cells(row, colCost).Value = PM.ResourceAligner.Scenarios.ActiveScenario.AlternativesFull.Find(Function(a) (a.ID.ToLower = alt.NodeGuidID.ToString.ToLower)).Cost 'AS/18712c
                worksheet.Cells(row, colRisks).Value = PM.ResourceAligner.Scenarios.ActiveScenario.AlternativesFull.Find(Function(a) (a.ID.ToLower = alt.NodeGuidID.ToString.ToLower)).RiskOriginal 'AS/18712c

                col = colRisks + 1 'AS/18712a

                'define attributes columns and write values into them
                For Each attr As clsAttribute In customAttributes
                    worksheet.Cells.Item(rowColName, col).Value = attr.Name
                    Select Case attr.ValueType'AS/18387b===
                        Case AttributeValueTypes.avtEnumeration, AttributeValueTypes.avtEnumerationMulti
                            Dim aEnum As clsAttributeEnumeration = PM.Attributes.GetEnumByID(attr.EnumID) '(mimiced the piece from Public Function AddEnumAttributeItem in CWSw\CoreWS_OperationContracts.vb)
                            If Not (aEnum Is Nothing OrElse attr.EnumID.Equals(Guid.Empty)) Then
                                Dim itemGUID As String = (PM.Attributes.GetAttributeValue(attr.ID, alt.NodeGuidID)).ToString
                                Dim val As String = aEnum.GetItemByID(New Guid(itemGUID)).Value
                                worksheet.Cells.Item(row, col).Value = val
                                If maxLenAttrName < Len(val) Then maxLenAttrName = Len(val)
                            End If
                        Case Else 'AS/18387b==
                            Dim val As Object = PM.Attributes.GetAttributeValue(attr.ID, alt.NodeGuidID)
                            If IsNothing(val) Then
                                worksheet.Cells.Item(row, col).Value = String.Empty
                            Else
                                worksheet.Cells.Item(row, col).Value = val.ToString
                                If maxLenAttrName < Len(val.ToString) Then maxLenAttrName = Len(val.ToString)
                            End If
                    End Select

                    col = col + 1
                Next

                'insert Total col
                worksheet.Cells(rowColName, colTotal).Value = "Total"
                worksheet.Cells(row, colTotal).Value = alt.UnnormalizedPriority 'AS/18712a
                col = colTotal + 1 'AS/18712a

                'define data columns and write values into them
                For Each node As clsNode In objH.Nodes
                    If node.IsTerminalNode Then '=====================
                        Dim sValue As String = ""
                        Select Case node.MeasureType
                            Case ECMeasureType.mtRatings
                                Dim currJ As clsRatingMeasureData = CType(GetJudgment(node, alt.NodeID, UserID), clsRatingMeasureData)
                                Try
                                    If Not IsNothing(currJ.Rating.RatingScale) Then
                                        sValue = currJ.Rating.Name
                                    Else
                                        sValue = JS_SafeNumber(Math.Round(currJ.Rating.Value, Options.DatagridNumberOfDecimals)) 'AS/17533u inserted Rouns function
                                    End If
                                Catch
                                    sValue = " "
                                End Try

                            Case ECMeasureType.mtDirect, ECMeasureType.mtStep, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtCustomUtilityCurve
                                Dim currJ As clsNonPairwiseMeasureData = GetJudgment(node, alt.NodeID, UserID)
                                Try
                                    Dim aVal As Double = CDbl(currJ.SingleValue)
                                    If aVal = Int32.MinValue Or aVal.Equals(Double.NaN) Then
                                        sValue = " "
                                    Else
                                        sValue = JS_SafeNumber(Math.Round(aVal, Options.DatagridNumberOfDecimals)) 'AS/17533u inserted Rouns function
                                    End If
                                Catch
                                    sValue = " "
                                End Try
                            Case Else 'for PW 
                                Dim aVal As Double = node.Judgments.Weights.GetUserWeights(UserID, ECSynthesisMode.smIdeal, PM.CalculationsManager.IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(alt.NodeID)
                                aVal = Math.Round(aVal, Options.DatagridNumberOfDecimals) 'AS/17533u
                                sValue = aVal.ToString
                        End Select

                        'data columns headers and values
                        worksheet.Cells.Item(rowColName, col).Value = node.NodeName
                        worksheet.Cells.Item(row, col).Value = sValue

                        col = col + 1

                    End If
                Next '======================

                row += 1
            Next
            colLastCovObjective = col

            'merge cells
            worksheet.Cells.GetSubrangeAbsolute(rowModelName, colAltName, rowModelName, colLastCovObjective).Merged = True 'model name
            worksheet.Cells.GetSubrangeAbsolute(rowColsGroups, colCost, rowColsGroups, colTotal).Merged = True 'attributes group
            worksheet.Cells.GetSubrangeAbsolute(rowColsGroups, colFirstCovObjective, rowColsGroups, colLastCovObjective).Merged = True 'cov objectives group
            worksheet.Cells.GetSubrangeAbsolute(rowColsGroups, colAltName, rowColName, colAltName).Merged = True 'alt col name

            'text format, alignment, colors, and borders
            Dim styleColHeader = New CellStyle
            styleColHeader.Font.Weight = ExcelFont.BoldWeight
            styleColHeader.WrapText = True
            styleColHeader.HorizontalAlignment = HorizontalAlignmentStyle.Center
            styleColHeader.VerticalAlignment = VerticalAlignmentStyle.Center
            styleColHeader.FillPattern.SetSolid(Color.LightGray)
            'styleColHeader.Borders.SetBorders(MultipleBorders.All, Color.White, LineStyle.Thick) 'makes frame to entire row!
            worksheet.Cells.GetSubrangeAbsolute(rowColsGroups, colAltName, rowColName, colLastCovObjective).Style = styleColHeader

            worksheet.Cells.GetSubrangeAbsolute(rowColsGroups, colAltName, rowColName, colLastCovObjective).Style.Borders.SetBorders(MultipleBorders.All, Color.White, LineStyle.Thick)
            worksheet.Cells.GetSubrangeAbsolute(rowColName, colCost, rowColName, colTotal - 1).Style.Rotation = 90

            'worksheet.Columns(colAltName).SetWidth(maxLenAltName, LengthUnit.ZeroCharacterWidth)
            'worksheet.Rows(rowColName).AutoFit()
            worksheet.Rows(rowColName).SetHeight(maxLenAttrName * 2, LengthUnit.ZeroCharacterWidth)
            worksheet.Columns(colAltName).AutoFit()
            worksheet.Columns(colAltName).Style.WrapText = True
            'worksheet.Columns(colTotal).Style.NumberFormat = "0.00%" 'AS/18712a

            ' Make entire sheet print on a single page.
            worksheet.PrintOptions.FitWorksheetWidthToPages = 1
            worksheet.PrintOptions.FitWorksheetHeightToPages = 1

            'worksheet = workbook.Worksheets.Add("Steve & Kris")

            'worksheet = workbook.Worksheets.Add("Marcia")
        Next 'AS/18712b

        ' Save
        workbook.Save(filePath)
        Return workbook

    End Function

    Private Function GetJudgment(ByVal CovObj As clsNode, altID As Integer, userID As Integer) As clsNonPairwiseMeasureData 'AS/18387a
        Return CType(CovObj.Judgments, clsNonPairwiseJudgments).GetJudgement(altID, CovObj.NodeID, userID)
    End Function
    Public Sub New(_App As clsComparionCore)
        App = _App
        PM = App.ActiveProject.ProjectManager

        SpreadsheetInfo.SetLicense("SN-2019Oct23-AKAqJAF7VQbNwpzNxMw+YMGUPQKtiac8SV2w3r4AR43N/ctUteUub15BB7pw2rqTqZB6PzdDiG4vHXzKw5SRVIMv9Gw==A")

        ' Create new empty document.
        workbook = New ExcelFile
    End Sub

    Function Test_AlternativesBarChart(filePath As String) As ExcelFile 'AS/17533f

        Dim workbook = New ExcelFile
        Dim worksheet = workbook.Worksheets.Add("Chart")

        'calc priorities for Goal and userID=-1 (Combined)
        Dim wrtNode As clsNode = PM.ActiveObjectives.GetNodeByID(1)
        Dim CalcTarget As clsCalculationTarget
        PM.StorageManager.Reader.LoadUserData(PM.GetUserByID(-1))
        CalcTarget = New clsCalculationTarget(CalculationTargetType.cttCombinedGroup, PM.CombinedGroups.GetCombinedGroupByUserID(-1))
        PM.CalculationsManager.Calculate(CalcTarget, wrtNode, PM.ActiveHierarchy, PM.ActiveAltsHierarchy)

        'List of Alternatives
        Dim altH As clsHierarchy = PM.AltsHierarchy(PM.ActiveAltsHierarchy)
        worksheet.Cells("A1").Value = "Alternative"
        worksheet.Cells("B1").Value = "Priority"
        Dim i As Integer = 1
        For Each alt As clsNode In altH.TerminalNodes
            worksheet.Cells("A" & i + 1).Value = alt.NodeName
            Dim sAltColor As String = ""
            Dim altColor As Long = CLng(PM.Attributes.GetAttributeValue(ATTRIBUTE_DEFAULT_BRUSH_COLOR_ID, alt.NodeGuidID))
            sAltColor = If(altColor > 0, LongToBrush(altColor), GetPaletteColor(CurrentPaletteID(PM), alt.NodeID, True))

            worksheet.Cells("B" & i + 1).Value = alt.UnnormalizedPriority

            i += 1
        Next

        ' Set header row and formatting.
        worksheet.Rows(0).Style.Font.Weight = ExcelFont.BoldWeight
        worksheet.Columns(0).Width = CInt(GemBox.Spreadsheet.LengthUnitConverter.Convert(3, GemBox.Spreadsheet.LengthUnit.Centimeter, GemBox.Spreadsheet.LengthUnit.ZeroCharacterWidth256thPart))
        worksheet.Columns(1).Style.NumberFormat = "0.00%"

        ' Make entire sheet print on a single page.
        worksheet.PrintOptions.FitWorksheetWidthToPages = 1
        worksheet.PrintOptions.FitWorksheetHeightToPages = 1

        ' Create Excel chart and select data for it.
        Dim chart = worksheet.Charts.Add(ChartType.Bar, "D2", "M25")
        chart.SelectData(worksheet.Cells.GetSubrangeAbsolute(0, 0, altH.TerminalNodes.Count, 1), True)

        ' Set chart legend.
        chart.Legend.IsVisible = True
        chart.Legend.Position = ChartLegendPosition.Right

        '=============================================
        ' Define colors
        Dim backgroundColor = DrawingColor.FromName(DrawingColorName.DarkRed)
        Dim seriesColor = DrawingColor.FromName(DrawingColorName.Green)
        Dim textColor = DrawingColor.FromName(DrawingColorName.White)
        Dim borderColor = DrawingColor.FromName(DrawingColorName.Black)

        ' Format chart
        chart.Fill.SetSolid(backgroundColor)

        Dim outline = chart.Outline
        outline.Width = Length.From(2, GemBox.Spreadsheet.LengthUnit.Point)
        outline.Fill.SetSolid(borderColor)

        ' Format plot area
        chart.PlotArea.Fill.SetSolid(DrawingColor.FromName(DrawingColorName.White))

        outline = chart.PlotArea.Outline
        outline.Width = Length.From(1.5, GemBox.Spreadsheet.LengthUnit.Point)
        outline.Fill.SetSolid(borderColor)

        ' Format chart title 
        Dim textFormat = chart.Title.TextFormat
        textFormat.Size = Length.From(20, GemBox.Spreadsheet.LengthUnit.Point)
        textFormat.Font = "Arial"
        textFormat.Fill.SetSolid(textColor)

        ' Format vertical axis
        'textFormat = chart.Axes.Vertical.TextFormat
        textFormat.Fill.SetSolid(textColor)
        textFormat.Italic = True

        ' Format horizontal axis
        'textFormat = chart.Axes.Horizontal.TextFormat
        textFormat.Fill.SetSolid(textColor)
        textFormat.Size = Length.From(12, GemBox.Spreadsheet.LengthUnit.Point)
        textFormat.Bold = True

        ' Format vertical major gridlines
        'chart.Axes.Vertical.MajorGridlines.Outline.Width = Length.From(0.5, GemBox.Spreadsheet.LengthUnit.Point)

        ' Format series
        Dim series = chart.Series(0)
        outline = series.Outline
        outline.Width = Length.From(3, GemBox.Spreadsheet.LengthUnit.Point)
        outline.Fill.SetSolid(seriesColor)

        series.Fill.SetSolid(DrawingColor.FromName(DrawingColorName.Red)) 'AS/17533f=== bars color, legend icon color
        outline.Fill.SetSolid(DrawingColor.FromName(DrawingColorName.Gold)) 'bars border color, legend icon border color

        'AS/17533f==

        ' Format series markers

        'series.Marker.MarkerType = MarkerType.Circle
        'series.Marker.Size = 10
        'series.Marker.Fill.SetSolid(textColor)
        'series.Marker.Outline.Fill.SetSolid(seriesColor)
        '==========================================================
        ' Save a document.
        workbook.Save(filePath)
        Return workbook

    End Function

    Function Exsample_CondtionalFormatting(filePath As String) As ExcelFile 'AS/18712e

        Dim worksheet = workbook.Worksheets.Add("Conditional Formatting")

        Dim rowCount As Integer = 20

        ' Specify sheet formatting.
        worksheet.Rows(0).Style.Font.Weight = ExcelFont.BoldWeight
        worksheet.Columns(0).SetWidth(3, LengthUnit.Centimeter)
        worksheet.Columns(1).SetWidth(3, LengthUnit.Centimeter)
        worksheet.Columns(2).SetWidth(3, LengthUnit.Centimeter)
        worksheet.Columns(3).SetWidth(3, LengthUnit.Centimeter)
        worksheet.Columns(3).Style.NumberFormat = "[$$-409]#,##0.00"
        worksheet.Columns(4).SetWidth(3, LengthUnit.Centimeter)
        worksheet.Columns(4).Style.NumberFormat = "yyyy-mm-dd"

        Dim cells = worksheet.Cells

        ' Specify header row.
        cells(0, 0).Value = "Departments"
        cells(0, 1).Value = "Names"
        cells(0, 2).Value = "Years of Service"
        cells(0, 3).Value = "Salaries"
        cells(0, 4).Value = "Deadlines"

        ' Insert random data to sheet.
        Dim random = New Random()
        Dim departments = New String() {"Legal", "Marketing", "Finance", "Planning", "Purchasing"}
        Dim names = New String() {"John Doe", "Fred Nurk", "Hans Meier", "Ivan Horvat"}
        For i As Integer = 0 To rowCount - 1

            cells(i + 1, 0).Value = departments(random.Next(departments.Length))
            cells(i + 1, 1).Value = names(random.Next(names.Length)) + " "c + (i + 1).ToString()
            cells(i + 1, 2).SetValue(random.Next(1, 31))
            cells(i + 1, 3).SetValue(random.Next(10, 101) * 100)
            cells(i + 1, 4).SetValue(DateTime.Now.AddDays(random.Next(-1, 2)))
        Next

        ' Apply shading to alternate rows in a worksheet using 'Formula' based conditional formatting.
        worksheet.ConditionalFormatting.AddFormula(worksheet.Cells.Name, "MOD(ROW(),2)=0").
            Style.FillPattern.PatternBackgroundColor = SpreadsheetColor.FromName(ColorName.Accent1Lighter40Pct)
        worksheet.ConditionalFormatting.AddFormula(worksheet.Cells.Name, "MOD(ROW(),2)=1").
            Style.FillPattern.PatternBackgroundColor = SpreadsheetColor.FromName(ColorName.Accent5Lighter80Pct)

        ' Apply '2-Color Scale' conditional formatting to 'Years of Service' column.
        worksheet.ConditionalFormatting.Add2ColorScale("C2:C" & (rowCount + 1))

        ' Apply '3-Color Scale' conditional formatting to 'Salaries' column.
        worksheet.ConditionalFormatting.Add3ColorScale("D2:D" & (rowCount + 1))

        ' Apply 'Data Bar' conditional formatting to 'Salaries' column.
        worksheet.ConditionalFormatting.AddDataBar("D2:D" & (rowCount + 1))

        ' Apply 'Icon Set' conditional formatting to 'Years of Service' column.
        worksheet.ConditionalFormatting.AddIconSet("C2:C" & (rowCount + 1)).IconStyle = SpreadsheetIconStyle.FourTrafficLights

        ' Apply green font color to cells in a 'Years of Service' column which have values between 15 and 20.
        worksheet.ConditionalFormatting.AddContainValue("C2:C" & (rowCount + 1), ContainValueOperator.Between, 15, 20).
            Style.Font.Color = SpreadsheetColor.FromName(ColorName.Green)

        ' Apply double red border to cells in a 'Names' column which contain text 'Doe'.
        worksheet.ConditionalFormatting.AddContainText("B2:B" & (rowCount + 1), ContainTextOperator.Contains, "Doe").
            Style.Borders.SetBorders(MultipleBorders.Outside, SpreadsheetColor.FromName(ColorName.Red), LineStyle.Double)

        ' Apply red shading to cells in a 'Deadlines' column which are equal to yesterday's date.
        worksheet.ConditionalFormatting.AddContainDate("E2:E" & (rowCount + 1), ContainDateOperator.Yesterday).
            Style.FillPattern.PatternBackgroundColor = SpreadsheetColor.FromName(ColorName.Red)

        ' Apply bold font weight to cells in a 'Salaries' column which have top 10 values.
        worksheet.ConditionalFormatting.AddTopOrBottomRanked("D2:D" & (rowCount + 1), False, 10).
            Style.Font.Weight = ExcelFont.BoldWeight

        ' Apply double underline to cells in a 'Years of Service' column which have below average value.
        worksheet.ConditionalFormatting.AddAboveOrBelowAverage("C2:C" & (rowCount + 1), True).
            Style.Font.UnderlineStyle = UnderlineStyle.Double

        ' Apply italic font style to cells in a 'Departments' column which have duplicate values.
        worksheet.ConditionalFormatting.AddUniqueOrDuplicate("A2:A" & (rowCount + 1), True).
            Style.Font.Italic = True

        workbook.Save(filePath)
        Return workbook

    End Function

    Function Example_CreateTaBle(filePath As String, server As HttpServerUtility) As ExcelFile 'AS/18387a

        Dim worksheet = workbook.Worksheets.Add("Writing")

        ' Tabular sample data for writing into an Excel file.
        Dim skyscrapers = New Object(20, 7) _
        {
            {"Rank", "Building", "City", "Country", "Metric", "Imperial", "Floors", "Built (Year)"},
            {1, "Burj Khalifa", "Dubai", "United Arab Emirates", 828, 2717, 163, 2010},
            {2, "Shanghai Tower", "Shanghai", "China", 632, 2073, 128, 2015},
            {3, "Abraj Al-Bait Clock Tower", "Mecca", "Saudi Arabia", 601, 1971, 120, 2012},
            {4, "Ping An Finance Centre", "Shenzhen", "China", 599, 1965, 115, 2017},
            {5, "Lotte World Tower", "Seoul", "South Korea", 554.5, 1819, 123, 2016},
            {6, "One World Trade Center", "New York City", "United States", 541.3, 1776, 104, 2014},
            {7, "Guangzhou CTF Finance Centre", "Guangzhou", "China", 530, 1739, 111, 2016},
            {7, "Tianjin CTF Finance Centre", "Tianjin", "China", 530, 1739, 98, 2018},
            {9, "China Zun", "Beijing", "China", 528, 1732, 108, 2018},
            {10, "Taipei 101", "Taipei", "Taiwan", 508, 1667, 101, 2004},
            {11, "Shanghai World Financial Center", "Shanghai", "China", 492, 1614, 101, 2008},
            {12, "International Commerce Centre", "Hong Kong", "China", 484, 1588, 118, 2010},
            {13, "Lakhta Center", "St. Petersburg", "Russia", 462, 1516, 86, 2018},
            {14, "Landmark 81", "Ho Chi Minh City", "Vietnam", 461.2, 1513, 81, 2018},
            {15, "Changsha IFS Tower T1", "Changsha", "China", 452.1, 1483, 88, 2017},
            {16, "Petronas Tower 1", "Kuala Lumpur", "Malaysia", 451.9, 1483, 88, 1998},
            {16, "Petronas Tower 2", "Kuala Lumpur", "Malaysia", 451.9, 1483, 88, 1998},
            {16, "The Exchange 106", "Kuala Lumpur", "Malaysia", 451.9, 1483, 97, 2018},
            {19, "Zifeng Tower", "Nanjing", "China", 450, 1476, 89, 2010},
            {19, "Suzhou IFS", "Suzhou", "China", 450, 1476, 92, 2017}
        }

        worksheet.Cells("A1").Value = "Example of writing typical table - tallest buildings in the world (2019):"

        ' Column width of 8, 30, 16, 20, 9, 11, 9, 9, 4 and 5 characters.
        worksheet.Columns("A").SetWidth(8, LengthUnit.ZeroCharacterWidth) ' Rank
        worksheet.Columns("B").SetWidth(30, LengthUnit.ZeroCharacterWidth) ' Building
        worksheet.Columns("C").SetWidth(16, LengthUnit.ZeroCharacterWidth) ' City
        worksheet.Columns("D").SetWidth(20, LengthUnit.ZeroCharacterWidth) ' Country
        worksheet.Columns("E").SetWidth(9, LengthUnit.ZeroCharacterWidth) ' Metric
        worksheet.Columns("F").SetWidth(11, LengthUnit.ZeroCharacterWidth) ' Imperial
        worksheet.Columns("G").SetWidth(9, LengthUnit.ZeroCharacterWidth) ' Floors
        worksheet.Columns("H").SetWidth(9, LengthUnit.ZeroCharacterWidth) ' Built (Year)
        worksheet.Columns("I").SetWidth(4, LengthUnit.ZeroCharacterWidth)
        worksheet.Columns("J").SetWidth(5, LengthUnit.ZeroCharacterWidth)

        Dim j As Integer
        ' Write header data to Excel cells.
        For j = 0 To 8 - 1 Step j + 1
            worksheet.Cells(3, j).Value = skyscrapers(0, j)
        Next

        worksheet.Cells.GetSubrange("A3:A4").Merged = True ' Rank
        worksheet.Cells.GetSubrange("B3:B4").Merged = True  ' Building
        worksheet.Cells.GetSubrange("C3:C4").Merged = True  ' City
        worksheet.Cells.GetSubrange("D3:D4").Merged = True  ' Country
        worksheet.Cells.GetSubrange("E3:F3").Merged = True ' Height
        worksheet.Cells("E3").Value = "Height"
        worksheet.Cells.GetSubrange("G3:G4").Merged = True  ' Floors
        worksheet.Cells.GetSubrange("H3:H4").Merged = True  ' Built (Year)

        Dim style = New CellStyle
        style.HorizontalAlignment = HorizontalAlignmentStyle.Center
        style.VerticalAlignment = VerticalAlignmentStyle.Center
        style.FillPattern.SetSolid(Color.Chocolate)
        style.Font.Weight = ExcelFont.BoldWeight
        style.Font.Color = Color.White
        style.WrapText = True
        style.Borders.SetBorders(MultipleBorders.Right Or MultipleBorders.Top, Color.Black, LineStyle.Thin)

        worksheet.Cells.GetSubrange("A3:H4").Style = style

        style = New CellStyle
        style.HorizontalAlignment = HorizontalAlignmentStyle.Center
        style.VerticalAlignment = VerticalAlignmentStyle.Center
        style.Font.Weight = ExcelFont.BoldWeight

        Dim mergedRange = worksheet.Cells.GetSubrange("I5:I14")
        mergedRange.Merged = True
        mergedRange.Value = "T o p   1 0"
        style.Rotation = -90
        style.FillPattern.SetSolid(Color.Lime)
        mergedRange.Style = style

        mergedRange = worksheet.Cells.GetSubrange("J5:J24")
        mergedRange.Merged = True
        mergedRange.Value = "T o p   2 0"
        style.IsTextVertical = True
        style.FillPattern.SetSolid(Color.Gold)
        mergedRange.Style = style

        mergedRange = worksheet.Cells.GetSubrange("I15:I24")
        mergedRange.Merged = True
        mergedRange.Style = style

        ' Write and format sample data to Excel cells.
        For i = 0 To 19
            For j = 0 To 7

                Dim cell = worksheet.Cells(i + 4, j)

                cell.Value = skyscrapers(i + 1, j)

                If i Mod 2 = 0 Then
                    cell.Style.FillPattern.SetSolid(Color.LightSkyBlue)
                Else
                    cell.Style.FillPattern.SetSolid(Color.FromArgb(210, 210, 230))
                End If

                If j = 4 Then
                    cell.Style.NumberFormat = "#"" m"""
                End If

                If j = 5 Then
                    cell.Style.NumberFormat = "#"" ft"""
                End If

                If j > 3 Then
                    cell.Style.Font.Name = "Courier New"
                End If

                cell.Style.Borders(IndividualBorder.Right).LineStyle = LineStyle.Thin
            Next j
        Next i

        worksheet.Cells.GetSubrange("A5", "J24").Style.Borders.SetBorders(MultipleBorders.Outside, Color.Black, LineStyle.Double)
        worksheet.Cells.GetSubrange("A3", "H4").Style.Borders.SetBorders(MultipleBorders.Vertical Or MultipleBorders.Top, Color.Black, LineStyle.Double)
        worksheet.Cells.GetSubrange("A5", "I14").Style.Borders.SetBorders(MultipleBorders.Bottom Or MultipleBorders.Right, Color.Black, LineStyle.Double)

        worksheet.Cells("A27").Value = "Notes:"
        worksheet.Cells("A28").Value = "a) 'Metric' and 'Imperial' columns use custom number formatting."
        worksheet.Cells("A29").Value = "b) All number columns use 'Courier New' font for improved number readability."
        worksheet.Cells("A30").Value = "c) Multiple merged ranges were used for table header and categories header."

        worksheet.PrintOptions.FitWorksheetWidthToPages = 1
        '==============================================================
        ' Save
        workbook.Save(filePath)
        Return workbook

    End Function

    Function Example_ComponentsChart(filePath As String) As ExcelFile 'AS/17533f

        Dim workbook = New ExcelFile()
        Dim worksheet = workbook.Worksheets.Add("Chart")

        Dim numberOfYears As Integer = 4

        ' Add data which Is used by the chart.
        worksheet.Cells("A1").Value = "Name"
        worksheet.Cells("A2").Value = "John Doe"
        worksheet.Cells("A3").Value = "Fred Nurk"
        worksheet.Cells("A4").Value = "Hans Meier"
        worksheet.Cells("A5").Value = "Ivan Horvat"

        ' Generate column titles
        For i As Integer = 0 To numberOfYears - 1
            worksheet.Cells(0, i + 1).Value = DateTime.Now.Year - numberOfYears + i
        Next

        Dim random = New Random()
        Dim range = worksheet.Cells.GetSubrangeAbsolute(1, 1, 4, numberOfYears)

        ' Fill the values
        For Each cell In range
            cell.SetValue(random.Next(1000, 5000))
            cell.Style.NumberFormat = """$""#,##0"
        Next

        ' Set header row And formatting.
        worksheet.Rows(0).Style.Font.Weight = ExcelFont.BoldWeight
        worksheet.Rows(0).Style.HorizontalAlignment = HorizontalAlignmentStyle.Center
        worksheet.Columns(0).Width = CInt(GemBox.Spreadsheet.LengthUnitConverter.Convert(3, GemBox.Spreadsheet.LengthUnit.Centimeter, GemBox.Spreadsheet.LengthUnit.ZeroCharacterWidth256thPart))

        ' Create chart And select data for it.
        Dim chart = worksheet.Charts.Add(Of ColumnChart)(ChartGrouping.Clustered, "B7", "O27")
        chart.SelectData(worksheet.Cells.GetSubrangeAbsolute(0, 0, 4, numberOfYears))

        ' Set chart title.
        chart.Title.Text = "Column Chart"

        ' Set chart legend.
        chart.Legend.IsVisible = True
        chart.Legend.Position = ChartLegendPosition.Right

        ' Set axis titles.
        chart.Axes.Horizontal.Title.Text = "Years"
        chart.Axes.Vertical.Title.Text = "Salaries"

        ' Set value axis scaling, units, gridlines and tick marks.
        Dim valueAxis = chart.Axes.VerticalValue
        valueAxis.Minimum = 0
        valueAxis.Maximum = 6000
        valueAxis.MajorUnit = 1000
        valueAxis.MinorUnit = 500
        valueAxis.MajorGridlines.IsVisible = True
        valueAxis.MinorGridlines.IsVisible = True
        valueAxis.MajorTickMarkType = TickMarkType.Outside
        valueAxis.MinorTickMarkType = TickMarkType.Cross

        ' Make entire sheet print horizontally centered on a single page with headings and gridlines.
        Dim printOptions = worksheet.PrintOptions
        printOptions.HorizontalCentered = True
        printOptions.PrintHeadings = True
        printOptions.PrintGridlines = True
        printOptions.FitWorksheetWidthToPages = 1
        printOptions.FitWorksheetHeightToPages = 1

        ' Save a document.
        workbook.Save(filePath)
        Return workbook

    End Function

    Function Example_CreateSamplePieChart(filePath As String) As ExcelFile 'AS/17533f

        Dim workbook = New ExcelFile
        Dim worksheet = workbook.Worksheets.Add("Data")

        ' Add data which is used by the Excel chart.
        worksheet.Cells("A1").Value = "Name"
        worksheet.Cells("A2").Value = "John Doe"
        worksheet.Cells("A3").Value = "Fred Nurk"
        worksheet.Cells("A4").Value = "Hans Meier"
        worksheet.Cells("A5").Value = "Ivan Horvat"

        worksheet.Cells("B1").Value = "Salary"
        worksheet.Cells("B2").Value = 3600
        worksheet.Cells("B3").Value = 2580
        worksheet.Cells("B4").Value = 3200
        worksheet.Cells("B5").Value = 4100

        ' Set header row and formatting.
        worksheet.Rows(0).Style.Font.Weight = ExcelFont.BoldWeight
        worksheet.Columns(0).Width = CInt(GemBox.Spreadsheet.LengthUnitConverter.Convert(3, GemBox.Spreadsheet.LengthUnit.Centimeter, GemBox.Spreadsheet.LengthUnit.ZeroCharacterWidth256thPart))
        worksheet.Columns(1).Style.NumberFormat = """$""#,##0"

        ' Make entire sheet print on a single page.
        worksheet.PrintOptions.FitWorksheetWidthToPages = 1
        worksheet.PrintOptions.FitWorksheetHeightToPages = 1

        ' Create Excel chart sheet.
        Dim chartsheet = workbook.Worksheets.Add(SheetType.Chart, "Chart")

        ' Create Excel chart and select data for it.
        ' You cannot set the size of the chart area when the chart is located on a chart sheet, it will snap to maximum size on the chart sheet.
        Dim chart = chartsheet.Charts.Add(ChartType.Pie, 0, 0, 10, 10, GemBox.Spreadsheet.LengthUnit.Centimeter)
        chart.SelectData(worksheet.Cells.GetSubrangeAbsolute(0, 0, 4, 1), True)

        ' Set chart legend.
        chart.Legend.IsVisible = True
        chart.Legend.Position = ChartLegendPosition.Right

        ' Save a document.
        workbook.Save(filePath)
        Return workbook

    End Function

    Function Example_CreateSampleBarChart(filePath As String) As ExcelFile 'AS/17533f


        Dim workbook = New ExcelFile
        Dim worksheet = workbook.Worksheets.Add("Chart")

        ' Add data which is used by the Excel chart.
        worksheet.Cells("A1").Value = "Name"
        worksheet.Cells("A2").Value = "John Doe"
        worksheet.Cells("A3").Value = "Fred Nurk"
        worksheet.Cells("A4").Value = "Hans Meier"
        worksheet.Cells("A5").Value = "Ivan Horvat"

        worksheet.Cells("B1").Value = "Salary"
        worksheet.Cells("B2").Value = 3600
        worksheet.Cells("B3").Value = 2580
        worksheet.Cells("B4").Value = 3200
        worksheet.Cells("B5").Value = 4100

        ' Set header row and formatting.
        worksheet.Rows(0).Style.Font.Weight = ExcelFont.BoldWeight
        worksheet.Columns(0).Width = CInt(GemBox.Spreadsheet.LengthUnitConverter.Convert(3, GemBox.Spreadsheet.LengthUnit.Centimeter, GemBox.Spreadsheet.LengthUnit.ZeroCharacterWidth256thPart))
        worksheet.Columns(1).Style.NumberFormat = """$""#,##0"

        ' Make entire sheet print on a single page.
        worksheet.PrintOptions.FitWorksheetWidthToPages = 1
        worksheet.PrintOptions.FitWorksheetHeightToPages = 1

        ' Create Excel chart and select data for it.
        Dim chart = worksheet.Charts.Add(ChartType.Bar, "D2", "M25")
        chart.SelectData(worksheet.Cells.GetSubrangeAbsolute(0, 0, 4, 1), True)

        ' Set chart legend.
        chart.Legend.IsVisible = True
        chart.Legend.Position = ChartLegendPosition.Right

        ' Save a document.
        workbook.Save(filePath)
        Return workbook

    End Function

    'Function Test_CreateChartInWordViaExcel(filePath As String) As DocumentModel 'AS/17533f

    '    ' Create chart as an image stream.
    '    Dim chartStream As MemoryStream = Test_CreateBarChart(
    '        New String() {"John", "Fred", "Hans", "Pete", "Mary"},
    '        New Double() {3300, 2200, 1100, 2000, 3000})

    '    ' Create a document.
    '    Dim document = New DocumentModel()

    '    ' Add an image.
    '    document.Sections.Add(
    '        New Section(document,
    '            New Paragraph(document,
    '                New Picture(document, chartStream))))

    '    ' Save a document.
    '    document.Save(filePath)
    '    Return document

    'End Function

    Private Function Test_CreateBarChart(names As String(), values As Double()) As MemoryStream 'AS/17533f
        ' Create a spreadsheet.
        Dim workbook = New ExcelFile()
        Dim worksheet = workbook.Worksheets.Add("Sheet1")

        ' Add data that will be used by chart.
        worksheet.Cells("A1").Value = "Name"
        worksheet.Cells("B1").Value = "Salary"

        Dim count = names.Length
        For i = 0 To count - 1
            worksheet.Cells(i + 1, 0).Value = names(i)
            worksheet.Cells(i + 1, 1).Value = values(i)
        Next

        ' Set header row and formatting.
        worksheet.Rows(0).Style.Font.Weight = ExcelFont.BoldWeight
        worksheet.Columns(0).Width = CInt(GemBox.Spreadsheet.LengthUnitConverter.Convert(3, GemBox.Spreadsheet.LengthUnit.Centimeter, GemBox.Spreadsheet.LengthUnit.ZeroCharacterWidth256thPart))
        worksheet.Columns(1).Style.NumberFormat = """$""#,##0"

        ' Make entire sheet print on a single page.
        worksheet.PrintOptions.FitWorksheetWidthToPages = 1
        worksheet.PrintOptions.FitWorksheetHeightToPages = 1

        ' Create Excel chart and select data for it.
        Dim chart = worksheet.Charts.Add(ChartType.Bar, "D2", "M25")
        chart.SelectData(worksheet.Cells.GetSubrangeAbsolute(0, 0, 5, count), True)

        ' Save chart to an image stream.
        Dim imageStream = New MemoryStream()
        Dim imageOptions = GemBox.Spreadsheet.SaveOptions.ImageDefault
        chart.Format().Save(imageStream, imageOptions)

        Return imageStream
    End Function

    'Function Example_CreateChartInWordViaExcel(filePath As String) As DocumentModel 'AS/17533f

    '    ' Create chart as an image stream.
    '    Dim chartStream As MemoryStream = CreateChart(
    '        New String() {"John Doe", "Fred Nurk", "Hans Meier"},
    '        New Double() {3300, 2200, 1100})

    '    ' Create a document.
    '    Dim document = New DocumentModel()

    '    ' Add an image.
    '    document.Sections.Add(
    '        New Section(document,
    '            New Paragraph(document,
    '                New Picture(document, chartStream))))

    '    ' Save a document.
    '    document.Save(filePath)
    '    Return document

    'End Function

    Private Function CreateChart(names As String(), values As Double()) As MemoryStream 'AS/17533f
        ' Create a spreadsheet.
        Dim workbook = New ExcelFile()
        Dim worksheet = workbook.Worksheets.Add("Sheet1")

        ' Add data that will be used by chart.
        Dim count = names.Length
        For i = 0 To count - 1
            worksheet.Cells(i, 0).Value = names(i)
            worksheet.Cells(i, 1).Value = values(i)
        Next

        ' Create chart and select data for it.
        Dim chart = DirectCast(worksheet.Charts.Add(ChartType.Bar, "D2", "J15"), BarChart)
        chart.SelectData(worksheet.Cells.GetSubrangeRelative(0, 0, 2, count), True, False, True)
        chart.Series(0).IsLegendEntryVisible = False

        ' Save chart to an image stream.
        Dim imageStream = New MemoryStream()
        Dim imageOptions = GemBox.Spreadsheet.SaveOptions.ImageDefault
        chart.Format().Save(imageStream, imageOptions)

        Return imageStream
    End Function
End Class
