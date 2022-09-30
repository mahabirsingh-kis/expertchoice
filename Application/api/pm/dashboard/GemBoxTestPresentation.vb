Imports System.Linq
Imports GemBox.Presentation 'AS/17533b
Imports GemBox.Presentation.Tables 'AS/17533c
Imports System.IO
Imports System.Runtime.Serialization

<Serializable> Public Class GemBoxTestPresentation
    Private App As clsComparionCore
    Private PM As clsProjectManager
    Private pptxReport As PresentationDocument

    Function GeneratePPTReport(filePath As String, server As HttpServerUtility, Options As ReportGeneratorOptions, DoForUser As Integer) As PresentationDocument

        '==============================================
        If Options Is Nothing Then
            Options = New ReportGeneratorOptions
            Options.ReportTitle = PM.ProjectName
        End If

        ' Create the first slide: model title and description
        Dim pptSlide As Slide
        pptSlide = GetModelTitleSlide(Options)
        pptSlide = GetInfodocSlide(Options, "Goal")
        pptSlide = GetPictureSlide(server, Options, "Hierarchy")
        pptSlide = GetPictureSlide(server, Options, "JudgmentsOfObjectives")
        pptSlide = GetPictureSlide(server, Options, "Chart_ITP_Alts")
        pptSlide = GetDatagridSlide(Options, "DataGrid", DoForUser)
        pptSlide = GetAttributesSlide(Options, "DataGrid", DoForUser) 'AS/18422d
        '==============================================

        '========================================
        'Dim slide As Slide
        'Dim comment As Comment
        'For i As SlideLayoutTy        'Dim slide As Slide 'create pptx file with table styles available in Gembox 'AS/18422g===
        'Dim styleName As TableStyleName
        'For styleName = TableStyleName.NoStyleNoGrid To TableStyleName.DarkStyle2Accent56
        '    slide = GetDatagridSlide(Options, "DataGrid", DoForUser, styleName)
        'Next styleName 'AS/18422g==pe = SlideLayoutType.Custom To SlideLayoutType.PictureAndCaption
        '    slide = pptxReport.Slides.AddNew(i)
        '    comment = slide.Comments.Add("Anatoly", "AS", i.ToString)
        'Next i


        '=========================================
        pptxReport.Save(filePath)
        Return pptxReport

    End Function

    Private Function GetModelTitleSlide(Options As ReportGeneratorOptions) As Slide

        Dim pptxSlide = pptxReport.Slides.AddNew(SlideLayoutType.Custom)
        Dim slideW As Double = pptxReport.SlideSize.Width
        Dim slideH As Double = pptxReport.SlideSize.Height

        ' Create New text box.
        Dim textBox = pptxSlide.Content.AddTextBox(ShapeGeometryType.Rectangle, 25, 25, slideW - 50, slideH - 50, LengthUnit.Point)
        textBox.Format.AutoFit = TextAutoFit.ResizeShapeToFitText

        ' Create New paragraph - Model name
        Dim paragraph = textBox.AddParagraph()
        paragraph.Format.Alignment = HorizontalAlignment.Center

        Dim sModelName As String = PM.ProjectName
        Dim run = paragraph.AddRun("Model Name: " & sModelName)
        run.Format.Size = 24
        run.Format.Bold = True
        run.Format.Font = "Arial"

        paragraph.AddLineBreak()

        ' Create New paragraph  - Model Description
        paragraph = textBox.AddParagraph()
        paragraph.Format.Alignment = HorizontalAlignment.Left

        run = paragraph.AddRun("Model Description:")
        run.Format.Size = 18
        run.Format.Font = "Arial"

        paragraph.AddLineBreak()

        'run = paragraph.AddRun(Infodoc2Text(App.ActiveProject, PM.ProjectDescription)) 'AS/18422a===
        'run.Format.Size = 12
        'run.Format.Font = "Arial" 'AS/18422a==

        ' Text should not contain characters '\v', '\r' and '\n', so split text for each line break character and add "TextRun" and "TextLineBreak" elements. 'AS/18422a===
        Dim sText As String = Infodoc2Text(App.ActiveProject, PM.ProjectDescription)
        For Each line In sText.Split(New String() {vbVerticalTab, vbCr, vbLf}, StringSplitOptions.RemoveEmptyEntries)
            run = paragraph.AddRun(line)
            run.Format.Size = 12
            run.Format.Font = "Arial"
            paragraph.AddLineBreak()
        Next
        ' Remove last "TextLineBreak" element.
        paragraph.Elements.RemoveAt(paragraph.Elements.Count - 1)
        paragraph.Format.Alignment = HorizontalAlignment.Left 'AS/18422a==

        Return pptxSlide

    End Function

    Private Function GetInfodocSlide(Options As ReportGeneratorOptions, infodoc As String) As Slide
        Dim pptxSlide = pptxReport.Slides.AddNew(SlideLayoutType.Custom)
        Dim slideW As Double = pptxReport.SlideSize.Width
        Dim slideH As Double = pptxReport.SlideSize.Height

        ' Create New text box.
        Dim textBox = pptxSlide.Content.AddTextBox(ShapeGeometryType.Rectangle, 25, 25, slideW - 50, slideH - 50, LengthUnit.Point)
        textBox.Format.AutoFit = TextAutoFit.ResizeShapeToFitText

        ' Create New paragraph - Model name
        Dim paragraph = textBox.AddParagraph()
        paragraph.Format.Alignment = HorizontalAlignment.Center

        ' Create New paragraph  - Goal Description
        Dim run = paragraph.AddRun("Goal Description:")
        run.Format.Size = 18
        run.Format.Font = "Arial"

        paragraph.AddLineBreak()

        ' Text should not contain characters '\v', '\r' and '\n', so split text for each line break character and add "TextRun" and "TextLineBreak" elements. 'AS/18422a===
        Dim sText As String = Infodoc2Text(App.ActiveProject, PM.Hierarchies(0).Nodes(0).InfoDoc)
        For Each line In sText.Split(New String() {vbVerticalTab, vbCr, vbLf}, StringSplitOptions.RemoveEmptyEntries)
            run = paragraph.AddRun(line)
            run.Format.Size = 12
            run.Format.Font = "Arial"
            paragraph.AddLineBreak()
        Next
        ' Remove last "TextLineBreak" element.
        paragraph.Elements.RemoveAt(paragraph.Elements.Count - 1)
        paragraph.Format.Alignment = HorizontalAlignment.Left 'AS/18422a==
        'OR use this way (actually, the below still keeps the center alignment for some reason)
        'paragraph.TextContent.LoadText(sText)
        'paragraph.Format.Alignment = HorizontalAlignment.Left 'AS/18422a==

        Return pptxSlide
    End Function

    Private Function GetPictureSlide(server As HttpServerUtility, ReportOptions As ReportGeneratorOptions, sReportPartName As String) As Slide 'AS/18422b

        Dim pptxSlide = pptxReport.Slides.AddNew(SlideLayoutType.Custom)
        Dim slideW As Double = pptxReport.SlideSize.Width
        Dim slideH As Double = pptxReport.SlideSize.Height

        Dim picName As String
        Dim picTitle As String
        Dim picW As Double = 600 ' default size 4x6
        Dim picH As Double = 400
        Dim picOrientation As Integer = 0

        Select Case sReportPartName
            Case "Hierarchy"
                picName = "ITP_Reports_Treeview.png"
                picTitle = "Objectives Hierarchy"
                picOrientation = ReportGeneratorOptions.PictureOrientation.picPortrait

            Case "JudgmentsOfObjectives"
                picName = "ITP_Reports_Judgments-of-Objectives_page1.png"
                picTitle = "Judgments Of Objectives"
                picOrientation = ReportGeneratorOptions.PictureOrientation.picPortrait

            Case Else
                picName = "Chart_ITP_Alts.png"
                picTitle = "Alternatives Chart"
                picOrientation = ReportGeneratorOptions.PictureOrientation.picLandscape
        End Select

        ' Create New text box.
        Dim textBox = pptxSlide.Content.AddTextBox(ShapeGeometryType.Rectangle, 25, 25, slideW - 50, slideH - 50, LengthUnit.Point)
        textBox.Format.AutoFit = TextAutoFit.ResizeShapeToFitText

        ' Create slide title
        Dim paragraph = textBox.AddParagraph()
        paragraph.Format.Alignment = HorizontalAlignment.Center

        Dim sModelName As String = PM.ProjectName
        Dim run = paragraph.AddRun(picTitle)
        run.Format.Size = 24
        run.Format.Bold = True
        run.Format.Font = "Arial"

        'calculate height of the title and then - picture size
        Dim stringFont As System.Drawing.Font = New System.Drawing.Font("Arial", 24)
        Dim s As System.Drawing.Size = System.Windows.Forms.TextRenderer.MeasureText(picTitle, stringFont)
        Dim titleH As Double = s.Height * 1.3
        picH = slideH - 50 - titleH
        'picW = picH * 2 / 3 ' keep ratio 4:6 
        If picOrientation = 0 Then
            picW = picH * 2 / 3 ' keep ratio 4:6
        Else
            picW = picH * 3 / 2 ' keep ratio 6:4 
        End If

        ' Insert piucture
        'Dim pic As New Picture(docxReport, server.MapPath("~/Images/favicon/" & picName), Options.PictureWidth, Options.PictureHeght, LengthUnit.Pixel)
        Dim pic As Picture = Nothing
        Using stream As Stream = File.OpenRead(server.MapPath("~/Images/favicon/" & picName))
            pic = pptxSlide.Content.AddPicture(PictureContentType.Png, stream, 25, 25 + titleH, picW, picH, LengthUnit.Point)
        End Using

        Return pptxSlide
    End Function

    Private Function GetDatagridSlide(Options As ReportGeneratorOptions, sReportPartName As String, UserID As Integer) As Slide 'AS/18422c
        'Private Function GetDatagridSlide(Options As ReportGeneratorOptions, sReportPartName As String, UserID As Integer, Optional styleName As TableStyleName = TableStyleName.NoStyleNoGrid) As Slide 'AS/18422g

        'calc priorities for Goal and userID=-1 (Combined)
        Dim wrtNode As clsNode = PM.ActiveObjectives.GetNodeByID(1)
        'Dim CalcTarget As clsCalculationTarget
        PM.StorageManager.Reader.LoadUserData(PM.GetUserByID(UserID))
        'CalcTarget = New clsCalculationTarget(CalculationTargetType.cttCombinedGroup, PM.CombinedGroups.GetCombinedGroupByUserID(UserID)) 'AS/18422f
        Dim CalcTarget As clsCalculationTarget = PM.CalculationsManager.GetCalculationTargetByUserID(UserID) 'AS/18422f
        PM.CalculationsManager.Calculate(CalcTarget, wrtNode, PM.ActiveHierarchy, PM.ActiveAltsHierarchy)

        Dim pptxSlide = pptxReport.Slides.AddNew(SlideLayoutType.Custom)
        Dim slideW As Double = pptxReport.SlideSize.Width
        Dim slideH As Double = pptxReport.SlideSize.Height

        Dim tableTitle As String = "Data Grid"
        Dim tableW As Double = 600 ' default size 4x6
        Dim tableH As Double = 400

        Select Case UserID'AS/18422e===
            Case -1
                tableTitle = tableTitle & " (All Participants)"
                'tableTitle = tableTitle & " (table style =  " & styleName.ToString & ")" 'AS/18422g
            Case >= 0
                tableTitle = tableTitle & " (" & PM.GetUserByID(UserID).UserName & ")"
        End Select 'AS/18422e==

        ' Create New text box.
        Dim textBox = pptxSlide.Content.AddTextBox(ShapeGeometryType.Rectangle, 5, 5, slideW - 5, slideH - 5, LengthUnit.Point) 'AS/18422e
        textBox.Format.AutoFit = TextAutoFit.ResizeShapeToFitText

        ' Create slide title
        Dim paragraph = textBox.AddParagraph()
        paragraph.Format.Alignment = HorizontalAlignment.Center

        Dim sModelName As String = PM.ProjectName
        Dim run = paragraph.AddRun(tableTitle)
        run.Format.Size = 24
        run.Format.Bold = True
        run.Format.Font = "Arial"

        'calculate height of the title and then - tableture size
        Dim stringFont As System.Drawing.Font = New System.Drawing.Font("Arial", 24)
        Dim s As System.Drawing.Size = System.Windows.Forms.TextRenderer.MeasureText(tableTitle, stringFont)
        Dim titleH As Double = s.Height * 1.3
        tableH = slideH - 5 - titleH 'AS/18422e
        tableW = slideW - 10 'AS/18422e

        ' Insert table
        Dim table = pptxSlide.Content.AddTable(5, 0 + titleH, tableW, tableH, LengthUnit.Point) 'AS/18422e

        ' Format table with no-style grid.
        table.Format.Style = pptxReport.TableStyles.GetOrAdd(TableStyleName.NoStyleTableGrid)

        Dim objH As clsHierarchy = App.ActiveProject.HierarchyObjectives
        Dim altH As clsHierarchy = App.ActiveProject.HierarchyAlternatives
        'Dim columnCount As Integer = objH.Nodes.Count + 1
        Dim columnCount As Integer = 5 'objH.TerminalNodes.Count
        Dim rowCount As Integer = altH.Nodes.Count 'AS/18422e
        Dim alt As clsNode, node As clsNode
        Dim cell As TableCell
        Dim row As TableRow
        'Dim attr As clsAttribute

        For i As Integer = 0 To columnCount - 1
            ' Create new table column.
            table.Columns.AddNew(tableW / columnCount)
        Next

        '===== create top row with column' titles
        row = table.Rows.AddNew(tableH / (rowCount + 1)) 'AS/18422e extra row for columns headers

        'Alts names column
        cell = row.Cells.AddNew()
        'cell.Text.AddParagraph().AddRun("Alternatives") 
        paragraph = cell.Text.AddParagraph()
        run = paragraph.AddRun("Alternatives")
        run.Format.Size = 12
        run.Format.Font = "Arial"

        'Total column
        cell = row.Cells.AddNew()
        'cell.Text.AddParagraph().AddRun("Total") 
        paragraph = cell.Text.AddParagraph()
        run = paragraph.AddRun("Total")
        run.Format.Size = 12
        run.Format.Font = "Arial"

        'Attributes columns (TEMPORARY commented out)
        'For i As Integer = 0 To PM.Attributes.AttributesList.Count - 1
        '    attr = PM.Attributes.AttributesList(i)
        '    If Not attr.IsDefault Then
        '        cell = row.Cells.AddNew()
        '        cell.Text.AddParagraph().AddRun(attr.Name)
        '    End If
        'Next

        'Covering objectives columns
        For i As Integer = 0 To columnCount - 1 'objH.TerminalNodes.Count - 1
            node = objH.TerminalNodes.Item(i)
            cell = row.Cells.AddNew()
            'cell.Text.AddParagraph().AddRun(node.NodeName) 
            paragraph = cell.Text.AddParagraph()
            run = paragraph.AddRun(node.NodeName)
            run.Format.Size = 12
            run.Format.Font = "Arial"

        Next

        'create rows for alternatives
        For i As Integer = 0 To rowCount - 1
            alt = altH.Nodes(i)
            ' Create new table row.
            row = table.Rows.AddNew(tableH / (rowCount + 1)) 'AS/18422e extra row for columns headers

            'add alts names to the first col
            cell = row.Cells.AddNew()
            'cell.Text.AddParagraph().AddRun(alt.NodeName) 
            paragraph = cell.Text.AddParagraph()
            run = paragraph.AddRun(alt.NodeName)
            run.Format.Size = 12
            run.Format.Font = "Arial"


            'add Total values to the second col
            cell = row.Cells.AddNew()
            'cell.Text.AddParagraph().AddRun((Math.Round(alt.UnnormalizedPriority, Options.DatagridNumberOfDecimals)).ToString) 
            paragraph = cell.Text.AddParagraph()
            run = paragraph.AddRun((Math.Round(alt.UnnormalizedPriority, Options.DatagridNumberOfDecimals)).ToString)
            run.Format.Size = 12
            run.Format.Font = "Arial"


            'add data to the data columns for each covering objective 
            'FOR THE TIME BEING, DO IT FOR COMBINED (UserID = -1)
            'For Each node In objH.TerminalNodes
            For j As Integer = 0 To columnCount - 1 'objH.TerminalNodes.Count - 1
                node = objH.TerminalNodes.Item(j)
                Dim sValue As String = ""

                'Select Case node.MeasureType 'AS/18422f===
                '    Case ECMeasureType.mtRatings
                '        Dim currJ As clsRatingMeasureData = CType(GetJudgment(node, alt.NodeID, -1), clsRatingMeasureData)
                '        Try
                '            If Not IsNothing(currJ.Rating.RatingScale) Then
                '                sValue = currJ.Rating.Name
                '            Else
                '                sValue = JS_SafeNumber(Math.Round(currJ.Rating.Value, Options.DatagridNumberOfDecimals))
                '            End If
                '        Catch
                '            sValue = " "
                '        End Try

                '    Case ECMeasureType.mtDirect, ECMeasureType.mtStep, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtCustomUtilityCurve
                '        Dim currJ As clsNonPairwiseMeasureData = GetJudgment(node, alt.NodeID, -1)
                '        Try
                '            Dim aVal As Double = CDbl(currJ.SingleValue)
                '            If aVal = Int32.MinValue Or aVal.Equals(Double.NaN) Then
                '                sValue = " "
                '            Else
                '                sValue = JS_SafeNumber(Math.Round(aVal, Options.DatagridNumberOfDecimals))
                '            End If
                '        Catch
                '            sValue = " "
                '        End Try
                '    Case Else 'for PW 
                '        Dim aVal As Double = node.Judgments.Weights.GetUserWeights(-1, ECSynthesisMode.smIdeal, PM.CalculationsManager.IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(alt.NodeID)
                '        aVal = Math.Round(aVal, Options.DatagridNumberOfDecimals)
                '        sValue = aVal.ToString
                'End Select 'AS/18422f==

                Select Case node.MeasureType 'AS/18422f===
                    Case ECMeasureType.mtPairwise, ECMeasureType.mtPWOutcomes, ECMeasureType.mtPWAnalogous
                        sValue = JS_SafeNumber(node.Judgments.Weights.GetUserWeights(UserID, ECSynthesisMode.smIdeal, PM.CalculationsManager.IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(alt.NodeID))
                    Case ECMeasureType.mtRatings
                        Dim rData As clsRatingMeasureData = CType(CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(alt.NodeID, node.NodeID, UserID), clsRatingMeasureData)
                        If rData IsNot Nothing AndAlso rData.Rating IsNot Nothing Then
                            If rData.Rating.ID <> -1 Then
                                sValue = rData.Rating.Name
                            Else
                                sValue = JS_SafeNumber(rData.Rating.Value)
                            End If
                        Else
                            sValue = " "
                        End If
                    Case Else
                        Dim nonpwData As clsNonPairwiseMeasureData = CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(alt.NodeID, node.NodeID, UserID)
                        If nonpwData IsNot Nothing AndAlso Not nonpwData.IsUndefined Then
                            sValue = JS_SafeNumber(nonpwData.ObjectValue)
                        Else
                            sValue = " "
                        End If
                End Select 'AS/18422f==

                ' Create new table cell and set text
                cell = row.Cells.AddNew()
                'cell.Text.AddParagraph().AddRun(sValue) 
                paragraph = cell.Text.AddParagraph()
                run = paragraph.AddRun(sValue)
                run.Format.Size = 12
                run.Format.Font = "Arial"

            Next
        Next

        ' ===== Set table style 'AS/18422g===
        'table.Format.Style = pptxReport.TableStyles.GetOrAdd(styleName) 'AS/18422g
        table.Format.Style = pptxReport.TableStyles.GetOrAdd(TableStyleName.MediumStyle2Accent2)

        ' Set table style options.
        'table.Format.StyleOptions = TableStyleOptions.FirstRow 'Or TableStyleOptions.LastRow Or TableStyleOptions.BandedRows 'AS/18422g==

        Return pptxSlide
    End Function

    Private Function GetAttributesSlide(Options As ReportGeneratorOptions, sReportPartName As String, DoForUser As Integer) As Slide 'AS/18422d

        Dim pptxSlide = pptxReport.Slides.AddNew(SlideLayoutType.Custom)
        Dim slideW As Double = pptxReport.SlideSize.Width
        Dim slideH As Double = pptxReport.SlideSize.Height

        Dim tableTitle As String = "Alternatives Attributes"
        Dim tableW As Double = 600 ' default size 4x6
        Dim tableH As Double = 400

        ' Create New text box.
        Dim textBox = pptxSlide.Content.AddTextBox(ShapeGeometryType.Rectangle, 5, 5, slideW - 5, slideH - 5, LengthUnit.Point)
        textBox.Format.AutoFit = TextAutoFit.ResizeShapeToFitText

        ' Create slide title
        Dim paragraph = textBox.AddParagraph()
        paragraph.Format.Alignment = HorizontalAlignment.Center

        Dim sModelName As String = PM.ProjectName
        Dim run = paragraph.AddRun(tableTitle)
        run.Format.Size = 24
        run.Format.Bold = True
        run.Format.Font = "Arial"

        'calculate height of the title and then - tableture size
        Dim stringFont As System.Drawing.Font = New System.Drawing.Font("Arial", 24)
        Dim s As System.Drawing.Size = System.Windows.Forms.TextRenderer.MeasureText(tableTitle, stringFont)
        Dim titleH As Double = s.Height * 1.3
        tableH = slideH - 5 - titleH
        tableW = slideW - 5

        ' Insert table
        Dim table = pptxSlide.Content.AddTable(0, 0 + titleH, tableW, tableH, LengthUnit.Point)

        ' Format table with no-style grid.
        table.Format.Style = pptxReport.TableStyles.GetOrAdd(TableStyleName.NoStyleTableGrid)

        Dim objH As clsHierarchy = App.ActiveProject.HierarchyObjectives
        Dim altH As clsHierarchy = App.ActiveProject.HierarchyAlternatives
        Dim customAttributes As List(Of clsAttribute) = PM.Attributes.GetAlternativesAttributes(True)
        Dim columnCount As Integer = customAttributes.Count + 1 'plus alts names column
        Dim rowCount As Integer = altH.Nodes.Count
        Dim alt As clsNode
        Dim cell As TableCell
        Dim row As TableRow
        Dim sValue As String = " "

        For i As Integer = 0 To columnCount - 1
            ' Create new table column.
            table.Columns.AddNew(tableW / columnCount)
        Next

        '===== create top row with column' titles
        row = table.Rows.AddNew(tableH / (rowCount + 1))

        'Alts names column
        cell = row.Cells.AddNew()
        cell.Text.AddParagraph().AddRun("Alternatives")

        'Attributes columns
        For Each attr As clsAttribute In customAttributes
            cell = row.Cells.AddNew()
            cell.Text.AddParagraph().AddRun(attr.Name)
        Next

        'create rows for alternatives
        For i As Integer = 0 To rowCount - 1
            alt = altH.Nodes(i)
            ' Create new table row.
            row = table.Rows.AddNew(tableH / (rowCount + 1))

            'add alts names to the first col
            cell = row.Cells.AddNew()
            cell.Text.AddParagraph().AddRun(alt.NodeName)

            'define attributes columns and write values into them
            For Each attr As clsAttribute In customAttributes
                Select Case attr.ValueType'AS/18387b===
                    Case AttributeValueTypes.avtEnumeration, AttributeValueTypes.avtEnumerationMulti
                        Dim aEnum As clsAttributeEnumeration = PM.Attributes.GetEnumByID(attr.EnumID)
                        If Not (aEnum Is Nothing OrElse attr.EnumID.Equals(Guid.Empty)) Then
                            Dim itemGUID As String = (PM.Attributes.GetAttributeValue(attr.ID, alt.NodeGuidID)).ToString
                            sValue = aEnum.GetItemByID(New Guid(itemGUID)).Value
                        End If
                    Case Else
                        Dim val As Object = PM.Attributes.GetAttributeValue(attr.ID, alt.NodeGuidID)
                        If Not IsNothing(val) Then
                            sValue = val.ToString
                        End If
                End Select

                ' Create new table cell and set text
                cell = row.Cells.AddNew()
                cell.Text.AddParagraph().AddRun(sValue)
            Next
        Next


        Return pptxSlide
    End Function

    Private Function GetJudgment(ByVal CovObj As clsNode, altID As Integer, userID As Integer) As clsNonPairwiseMeasureData
        Return CType(CovObj.Judgments, clsNonPairwiseJudgments).GetJudgement(altID, CovObj.NodeID, userID)
    End Function

    Function Example_TableFormat_PPTX(filepath As String) As PresentationDocument 'AS/18422g

        Dim presentation = New PresentationDocument()
        Dim slide = presentation.Slides.AddNew(SlideLayoutType.Custom)

        ' Create new table.
        Dim table = slide.Content.AddTable(5, 5, 20, 5, LengthUnit.Centimeter)

        ' Format table with no-style grid.
        table.Format.Style = presentation.TableStyles.GetOrAdd(
            TableStyleName.NoStyleTableGrid)

        table.Format.Fill.SetSolid(Color.FromName(ColorName.Orange))

        table.Columns.AddNew(Length.From(7, LengthUnit.Centimeter))
        table.Columns.AddNew(Length.From(10, LengthUnit.Centimeter))
        table.Columns.AddNew(Length.From(5, LengthUnit.Centimeter))

        Dim row = table.Rows.AddNew(Length.From(5, LengthUnit.Centimeter))

        Dim cell = row.Cells.AddNew()

        cell.Format.Fill.SetSolid(Color.FromName(ColorName.Red))

        cell.Text.Format.VerticalAlignment = VerticalAlignment.Top

        cell.Text.AddParagraph().AddRun("Cell 1-1")

        cell = row.Cells.AddNew()

        Dim border = cell.Format.DiagonalDownBorderLine

        border.Fill.SetSolid(Color.FromName(ColorName.White))
        border.Width = Length.From(5, LengthUnit.Millimeter)

        border = cell.Format.DiagonalUpBorderLine

        border.Fill.SetSolid(Color.FromName(ColorName.White))
        border.Width = Length.From(5, LengthUnit.Millimeter)

        cell.Text.Format.VerticalAlignment = VerticalAlignment.Middle

        cell.Text.AddParagraph().AddRun("Cell 1-2")

        cell = row.Cells.AddNew()

        cell.Format.Fill.SetSolid(Color.FromName(ColorName.DarkBlue))

        cell.Text.Format.VerticalAlignment = VerticalAlignment.Bottom

        cell.Text.AddParagraph().AddRun("Cell 1-3")
        presentation.Save(filepath)

        Return presentation
    End Function

    Function Example_Table_PPTX(filepath As String) As PresentationDocument 'AS/18422g

        Dim presentation = New PresentationDocument()
        Dim slide = presentation.Slides.AddNew(SlideLayoutType.Custom)

        ' Create new table.
        Dim table = slide.Content.AddTable(5, 5, 20, 12, LengthUnit.Centimeter)

        ' Format table with no-style grid.
        table.Format.Style = presentation.TableStyles.GetOrAdd(
            TableStyleName.NoStyleTableGrid)

        Dim columnCount As Integer = 4
        Dim rowCount As Integer = 10

        For i As Integer = 0 To columnCount - 1
            ' Create new table column.
            table.Columns.AddNew(Length.From(5, LengthUnit.Centimeter))
        Next

        For i As Integer = 0 To rowCount - 1

            ' Create new table row.
            Dim row = table.Rows.AddNew(
                Length.From(1.2, LengthUnit.Centimeter))

            For j As Integer = 0 To columnCount - 1

                ' Create new table cell.
                Dim cell = row.Cells.AddNew()

                ' Set table cell text.
                cell.Text.AddParagraph().AddRun(
                    String.Format(Nothing, "Cell {0}-{1}", i + 1, j + 1))
            Next
        Next
        presentation.Save(filepath)

        Return presentation
    End Function


    Function Test_InfodocAndHierarchy_PPTX(filePath As String) As PresentationDocument 'AS/17533c

        Dim presentation As New PresentationDocument()

        ' Create New slide.
        Dim slide = presentation.Slides.AddNew(SlideLayoutType.Custom)

        ' Create New text box.
        Dim textBox = slide.Content.AddTextBox(ShapeGeometryType.Rectangle, 2, 2, 5, 4, LengthUnit.Centimeter)
        textBox.Format.AutoFit = TextAutoFit.ResizeShapeToFitText

        ' Create New paragraph.
        Dim paragraph = textBox.AddParagraph()

        'create new Run - the first is Model name
        Dim sModelName As String = PM.ProjectName
        Dim run = paragraph.AddRun("Model Name: " & sModelName)

        'format paragraph and characters
        Dim format = paragraph.Format
        run.Format.Size = 16
        run.Format.Bold = True



        presentation.Save(filePath)
        Return presentation

    End Function

    Function Test_TemplateUse_PPTX(filepath As String) As PresentationDocument 'AS/17533c

        Dim sTplName As String = String.Format("{0}{1}", _FILE_REPORT_TEMPLATES, "GemBoxTestReport_1.pptx")
        Dim presentation = PresentationDocument.Load(sTplName)

        ' Retrieve first slide.
        Dim slide = presentation.Slides(0)

        ' Retrieve "Title" placeholder And set shape text.
        Dim shape = slide.Content.Drawings.OfType(Of Shape).Where(Function(item) item.Placeholder IsNot Nothing AndAlso item.Placeholder.PlaceholderType = PlaceholderType.Title).First()
        shape.Text.Paragraphs(0).Elements.Clear()
        shape.Text.Paragraphs(0).AddRun("Model Name: " & PM.ProjectName)

        ' Retrieve "Content" placeholder and set the text
        shape = slide.Content.Drawings.OfType(Of Shape).Where(Function(item) item.Placeholder IsNot Nothing AndAlso item.Placeholder.PlaceholderType = PlaceholderType.Content).First()
        shape.Text.Paragraphs(0).Elements.Clear()
        Dim sText As String = Infodoc2Text(App.ActiveProject, PM.Hierarchies(0).Nodes(0).InfoDoc)
        sText = sText.Trim 'remove vbCrLf from the end of the string
        shape.Text.Paragraphs(0).AddRun(sText)

        ' Retrieve second slide.
        slide = presentation.Slides(1)

        ' Retrieve "Title" placeholder And set shape text.
        shape = slide.Content.Drawings.OfType(Of Shape).Where(Function(item) item.Placeholder IsNot Nothing AndAlso item.Placeholder.PlaceholderType = PlaceholderType.Title).First()
        shape.Text.Paragraphs(0).Elements.Clear()
        shape.Text.Paragraphs(0).AddRun("Objectives Hierarchy")

        ' Retrieve and clear the "Content" placeholder.
        shape = slide.Content.Drawings.OfType(Of Shape).Where(Function(item) item.Placeholder IsNot Nothing AndAlso item.Placeholder.PlaceholderType = PlaceholderType.Content).First()
        shape.Text.Paragraphs(0).Elements.Clear()

        Dim objH As clsHierarchy = App.ActiveProject.HierarchyObjectives
        Dim L As Double, T As Double, W As Double, H As Double
        Dim L_goal As Double, T_goal As Double
        L = 5 '1
        T = 5 '1
        W = 6
        H = 1
        L_goal = L
        T_goal = T
        Dim textbox = slide.Content.AddTextBox(ShapeGeometryType.Rectangle, L, T, W, H, LengthUnit.Centimeter)
        textbox.Shape.Format.Outline.Fill.SetSolid(Color.FromName(ColorName.Black))
        Dim paragraph = textbox.AddParagraph()
        paragraph.Format.Alignment = HorizontalAlignment.Center
        paragraph.AddRun(objH.Nodes(0).NodeName)

        For i As Integer = 1 To objH.Nodes.Count - 1
            Dim N As clsNode = objH.Nodes(i)
            L = L_goal + N.Level '2
            T = T + 2
            W = 6
            H = 1
            textbox = slide.Content.AddTextBox(ShapeGeometryType.Rectangle, L, T, W, H, LengthUnit.Centimeter)
            textbox.Shape.Format.Outline.Fill.SetSolid(Color.FromName(ColorName.Black))
            paragraph = textbox.AddParagraph()
            paragraph.Format.Alignment = HorizontalAlignment.Center
            paragraph.AddRun(N.NodeName)
            'draw horizontal connector
            slide.Content.AddConnector(ShapeGeometryType.Line, L_goal + N.Level / 2, T + H / 2, 0.5, 0, LengthUnit.Centimeter)
        Next i
        'draw vertical connector
        slide.Content.AddConnector(ShapeGeometryType.Line, L_goal + 0.5, T_goal + H, 0, (T + H / 2) - (T_goal + H), LengthUnit.Centimeter)

        ' Retrieve third slide.
        slide = presentation.Slides(2)

        ' Retrieve "Title" placeholder And set shape text.
        shape = slide.Content.Drawings.OfType(Of Shape).Where(Function(item) item.Placeholder IsNot Nothing AndAlso item.Placeholder.PlaceholderType = PlaceholderType.Title).First()
        shape.Text.Paragraphs(0).Elements.Clear()
        shape.Text.Paragraphs(0).AddRun("Alternatives")

        ' Retrieve a table.
        Dim table = slide.Content.Drawings.OfType(Of GraphicFrame).Where(Function(item) item.Table IsNot Nothing).Select(Function(item) item.Table).First()

        ''HERE -- add new rows and columns if needed, add cells to new rows
        'Dim row = table.Rows.AddNew(2)
        'For j As Integer = 0 To table.Columns.Count - 1
        '    ' Create new table cell.
        '    Dim cell = row.Cells.AddNew()
        '    ' Set cell text.
        '    cell.Text.AddParagraph().AddRun("AAA")
        'Next

        ' Fill table data.
        Dim altH As clsHierarchy = App.ActiveProject.HierarchyAlternatives
        Dim columnCount As Integer = table.Columns.Count
        Dim rowCount As Integer = table.Rows.Count

        For i As Integer = 1 To rowCount - 1
            table.Rows(i).Cells(0).Text.Paragraphs(0).Elements.OfType(Of TextRun).First().Text = altH.Nodes(i - 1).NodeName
            table.Rows(i).Cells(1).Text.Paragraphs(0).Elements.OfType(Of TextRun).First().Text = altH.Nodes(i - 1).LocalPriority(0).ToString
        Next i

        presentation.Save(filepath)
        Return presentation

    End Function

    Function Example_TemplateUse_PPTX(filepath As String) As PresentationDocument 'AS/17533c

        Dim sTplName As String = String.Format("{0}{1}", _FILE_REPORT_TEMPLATES, "Template Use.pptx")
        Dim presentation = PresentationDocument.Load(sTplName)

        ' Retrieve first slide.
        Dim slide = presentation.Slides(0)

        ' Retrieve "Title" placeholder And set shape text.
        Dim shape = slide.Content.Drawings.OfType(Of Shape).Where(Function(item) item.Placeholder IsNot Nothing AndAlso item.Placeholder.PlaceholderType = PlaceholderType.CenteredTitle).First()
        shape.Text.Paragraphs(0).AddRun("ACME Corp - 4th Quarter Financial Results")

        ' Retrieve second slide.
        slide = presentation.Slides(1)

        ' Retrieve "Title" placeholder And set shape text.
        shape = slide.Content.Drawings.OfType(Of Shape).Where(Function(item) item.Placeholder IsNot Nothing AndAlso item.Placeholder.PlaceholderType = PlaceholderType.Title).First()
        shape.Text.Paragraphs(0).AddRun("4th Quarter Summary")

        ' Retrieve "Content" placeholder.
        shape = slide.Content.Drawings.OfType(Of Shape).Where(Function(item) item.Placeholder IsNot Nothing AndAlso item.Placeholder.PlaceholderType = PlaceholderType.Content).First()

        ' Set list text.
        shape.Text.Paragraphs(0).Elements.Clear()
        shape.Text.Paragraphs(0).AddRun("3 new products/services in Research and Development.")

        shape.Text.Paragraphs(1).Elements.Clear()
        shape.Text.Paragraphs(1).AddRun("Rollout planned for new division.")

        shape.Text.Paragraphs(2).Elements.Clear()
        shape.Text.Paragraphs(2).AddRun("Campaigns targeting new markets.")

        ' Retrieve third slide.
        slide = presentation.Slides(2)

        ' Retrieve "Title" placeholder And set shape text.
        shape = slide.Content.Drawings.OfType(Of Shape).Where(Function(item) item.Placeholder IsNot Nothing AndAlso item.Placeholder.PlaceholderType = PlaceholderType.Title).First()
        shape.Text.Paragraphs(0).AddRun("4th Quarter Financial Highlights")

        ' Retrieve a table.
        Dim table = slide.Content.Drawings.OfType(Of GraphicFrame).Where(Function(item) item.Table IsNot Nothing).Select(Function(item) item.Table).First()

        ' Fill table data.
        table.Rows(1).Cells(1).Text.Paragraphs(0).Elements.OfType(Of TextRun).First().Text = "$14.2M"
        table.Rows(1).Cells(2).Text.Paragraphs(0).Elements.OfType(Of TextRun).First().Text = "(0.5%)"

        table.Rows(2).Cells(1).Text.Paragraphs(0).Elements.OfType(Of TextRun).First().Text = "$1.6M"
        table.Rows(2).Cells(2).Text.Paragraphs(0).Elements.OfType(Of TextRun).First().Text = "0.7%"

        table.Rows(3).Cells(1).Text.Paragraphs(0).Elements.OfType(Of TextRun).First().Text = "$12.5M"
        table.Rows(3).Cells(2).Text.Paragraphs(0).Elements.OfType(Of TextRun).First().Text = "0.3%"

        table.Rows(4).Cells(1).Text.Paragraphs(0).Elements.OfType(Of TextRun).First().Text = "$2.3M"
        table.Rows(4).Cells(2).Text.Paragraphs(0).Elements.OfType(Of TextRun).First().Text = "(0.2%)"

        presentation.Save(filepath)
        Return presentation

    End Function

    Function Example_Treeview_PPTX(filepath As String) As PresentationDocument 'AS/17533b

        Dim presentation = New PresentationDocument()
        Dim slide = presentation.Slides.AddNew(SlideLayoutType.Custom)

        Dim textbox = slide.Content.AddTextBox(ShapeGeometryType.Rectangle, 1, 1, 6, 1, LengthUnit.Centimeter)
        textbox.Shape.Format.Outline.Fill.SetSolid(Color.FromName(ColorName.Black))
        Dim paragraph = textbox.AddParagraph()
        paragraph.Format.Alignment = HorizontalAlignment.Center
        paragraph.AddRun("Purchase a new car")

        textbox = slide.Content.AddTextBox(ShapeGeometryType.Rectangle, 2, 3, 6, 1, LengthUnit.Centimeter)
        textbox.Shape.Format.Outline.Fill.SetSolid(Color.FromName(ColorName.Black))
        paragraph = textbox.AddParagraph()
        paragraph.Format.Alignment = HorizontalAlignment.Center
        paragraph.AddRun("Cost of Ownership")

        textbox = slide.Content.AddTextBox(ShapeGeometryType.Rectangle, 2, 5, 6, 1, LengthUnit.Centimeter)
        textbox.Shape.Format.Outline.Fill.SetSolid(Color.FromName(ColorName.Black))
        paragraph = textbox.AddParagraph()
        paragraph.Format.Alignment = HorizontalAlignment.Center
        paragraph.AddRun("Performance")

        textbox = slide.Content.AddTextBox(ShapeGeometryType.Rectangle, 2, 7, 6, 1, LengthUnit.Centimeter)
        textbox.Shape.Format.Outline.Fill.SetSolid(Color.FromName(ColorName.Black))
        paragraph = textbox.AddParagraph()
        paragraph.Format.Alignment = HorizontalAlignment.Center
        paragraph.AddRun("Style")

        slide.Content.AddConnector(ShapeGeometryType.Line, 1.5, 2, 0, 5.5, LengthUnit.Centimeter)
        slide.Content.AddConnector(ShapeGeometryType.Line, 1.5, 3.5, 0.5, 0, LengthUnit.Centimeter)
        slide.Content.AddConnector(ShapeGeometryType.Line, 1.5, 5.5, 0.5, 0, LengthUnit.Centimeter)
        slide.Content.AddConnector(ShapeGeometryType.Line, 1.5, 7.5, 0.5, 0, LengthUnit.Centimeter)
        presentation.Save(filepath)

        Return presentation
    End Function

    Public Sub New(_App As clsComparionCore)
        App = _App
        PM = App.ActiveProject.ProjectManager

        ' If using Professional version, put your serial key below.
        ComponentInfo.SetLicense("PN-2019Oct23-7esqnSuMJVQEFrRWgpgmjvPBQJyxMtrJ+gyZpUocTKpiNFnadOxsvCrZJYM5/nk1w+KbQKspn1xCUQDmPWsvtZqJJNQ==A")

        ' Create new empty document.
        pptxReport = New PresentationDocument()

    End Sub
End Class
