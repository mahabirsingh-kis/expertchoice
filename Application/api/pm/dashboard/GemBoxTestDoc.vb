Imports GemBox.Document
Imports System.Globalization
Imports GemBox.Document.Drawing
Imports GemBox.Document.Tables 'AS/17663c
Imports System.IO 'AS/17663c

<Serializable> Public Class GemBoxTestDoc 'AS/17663b added the class
    Private App As clsComparionCore
    Private PM As clsProjectManager
    Private document As DocumentModel

    Private Textboxes As List(Of wordTextbox)

    Function Test_SplitTable(filePath As String) As DocumentModel 'AS/17533l

        Dim docGB As DocumentModel = New DocumentModel()

        'draw test table
        Dim tableRowCount As Integer = 5
        Dim tableColumnCount As Integer = 50

        Dim table As New Table(docGB)
        'table.TableFormat.PreferredWidth = New TableWidth(100, TableWidthUnit.Percentage)

        For i As Integer = 0 To tableRowCount - 1
            Dim row As New TableRow(docGB)
            table.Rows.Add(row)

            For j As Integer = 0 To tableColumnCount - 1
                Dim para As New Paragraph(docGB, String.Format("Cell {0}-{1}", i + 1, j + 1))

                row.Cells.Add(New TableCell(docGB, para))
                row.Cells(j).CellFormat.PreferredWidth = New TableWidth(50, TableWidthUnit.Auto)
                row.Cells(j).CellFormat.WrapText = False
            Next
        Next

        docGB.Sections.Add(New Section(docGB, table))

        docGB.Save(filePath)
        Return docGB

    End Function

    Function Test_AutosizeTextbox(filePath As String) As DocumentModel 'AS/17533k

        ' Create new empty document
        Dim docGB As DocumentModel = New DocumentModel()
        Dim section As New Section(docGB)
        docGB.Sections.Add(section)

        Dim sA As New String("A"c, 10)
        sA = New String("A"c, 20)
        sA = New String("A"c, 40)

        Dim sB As New String("B"c, 10)
        sB = New String("B"c, 20)
        sB = New String("B"c, 40)

        Dim fontsize As Double = docGB.DefaultCharacterFormat.Size
        Dim newFontsize As Double

        Dim nodeLeft As Double, nodeTop As Double, nodeWidth As Double, nodeHeight As Double
        nodeLeft = 10
        nodeTop = 10
        nodeWidth = 120
        nodeHeight = 20


        Dim Paragraph = New Paragraph(docGB)
        docGB.Sections(0).Blocks.Add(Paragraph)

        Dim tvGroup = New Group(docGB, Layout.Inline(New Size(nodeWidth, nodeHeight)))
        Paragraph.Inlines.Add(tvGroup)

        'draw textbox for Goal
        newFontsize = 24
        nodeWidth = newFontsize * Len(sA)
        nodeHeight = newFontsize * 2
        Dim tvTexbBox = New TextBox(docGB,
            Layout.Floating(
                New HorizontalPosition(nodeLeft, LengthUnit.Point, HorizontalPositionAnchor.TopLeftCorner),
                New VerticalPosition(nodeTop, LengthUnit.Point, VerticalPositionAnchor.TopLeftCorner),
                New Size(nodeWidth, nodeHeight)),
            New Paragraph(docGB, New Run(docGB, sA) With {.CharacterFormat = New CharacterFormat With {.Bold = True, .Size = newFontsize}}) With {.ParagraphFormat = New ParagraphFormat() With {.Alignment = HorizontalAlignment.Left}})
        tvGroup.Add(tvTexbBox)

        newFontsize = 8
        nodeWidth = newFontsize * Len(sB)
        nodeHeight = newFontsize * 2
        nodeTop = nodeTop + 100
        tvTexbBox = New TextBox(docGB,
            Layout.Floating(
                New HorizontalPosition(nodeLeft, LengthUnit.Point, HorizontalPositionAnchor.TopLeftCorner),
                New VerticalPosition(nodeTop, LengthUnit.Point, VerticalPositionAnchor.TopLeftCorner),
                New Size(nodeWidth, nodeHeight)),
            New Paragraph(docGB, New Run(docGB, sB) With {.CharacterFormat = New CharacterFormat With {.Bold = True, .Size = newFontsize}}) With {.ParagraphFormat = New ParagraphFormat() With {.Alignment = HorizontalAlignment.Left}})

        'tvTexbBox.Outline.Fill.SetEmpty() 'set textbox with no borders 
        tvGroup.Add(tvTexbBox)

        docGB.Save(filePath)
        Return docGB
    End Function

    Function Test_PageSetup(filePath As String) As DocumentModel

        ' Create new empty docGB
        Dim docGB As DocumentModel = New DocumentModel()
        Dim section As New Section(docGB,
                New Paragraph(docGB,
                    New Run(docGB, "First line"),
                    New SpecialCharacter(docGB, SpecialCharacterType.LineBreak),
                    New Run(docGB, "Second line"),
                    New SpecialCharacter(docGB, SpecialCharacterType.LineBreak),
                    New Run(docGB, "Third line")),
                New Paragraph(docGB,
                    New SpecialCharacter(docGB, SpecialCharacterType.ColumnBreak),
                    New Run(docGB, "First line"),
                    New SpecialCharacter(docGB, SpecialCharacterType.LineBreak),
                    New Run(docGB, "Second line"),
                    New SpecialCharacter(docGB, SpecialCharacterType.LineBreak),
                    New Run(docGB, "Third line")))

        Dim pageSetup As PageSetup = section.PageSetup

        ' Specify text columns.
        pageSetup.TextColumns = New TextColumnCollection(2) With {
            .LineBetween = True,
            .EvenlySpaced = False
        }

        pageSetup.TextColumns(0).Width = LengthUnitConverter.Convert(1, LengthUnit.Inch, LengthUnit.Point)
        pageSetup.TextColumns(1).Width = LengthUnitConverter.Convert(2.3, LengthUnit.Inch, LengthUnit.Point)

        ' Specify paper type.
        pageSetup.PaperType = PaperType.A5

        docGB.Sections.Add(section)

        ' Specify line numbering.
        docGB.Sections.Add(
            New Section(docGB,
                New Paragraph(docGB,
                    New Run(docGB, "First line"),
                    New SpecialCharacter(docGB, SpecialCharacterType.LineBreak),
                    New Run(docGB, "Second line"),
                    New SpecialCharacter(docGB, SpecialCharacterType.LineBreak),
                    New Run(docGB, "Third line"))) With {
                        .PageSetup = New PageSetup() With {
                            .PaperType = PaperType.A5,
                            .LineNumberRestartSetting = LineNumberRestartSetting.NewPage
         }})

        docGB.Sections(1).PageSetup.Orientation = Orientation.Landscape

        docGB.Save(filePath)
        Return docGB

    End Function


    Function Draw_HierarchyVertical(filePath As String) As DocumentModel 'AS/17533g

        ' Create new empty document
        Dim docGB As DocumentModel = New DocumentModel()
        Dim section As New Section(docGB)
        docGB.Sections.Add(section)

        Dim objH As clsHierarchy = App.ActiveProject.HierarchyObjectives
        Dim nodeLeft As Double, nodeTop As Double, nodeWidth As Double, nodeHeight As Double
        nodeLeft = 10
        nodeTop = 10
        nodeWidth = 120
        nodeHeight = 20

        Dim goalLeft As Double, goalTop As Double, parentLeft As Double, parentTop As Double
        goalLeft = nodeLeft
        goalTop = nodeTop
        parentLeft = nodeLeft
        parentTop = nodeTop

        Dim line As Shape
        Dim lineLeft As Double, lineTop As Double, lineW As Double, lineH As Double, lineIndent As Double
        lineIndent = 15 'nodeWidth / 10

        Dim Paragraph = New Paragraph(docGB)
        docGB.Sections(0).Blocks.Add(Paragraph)

        Dim Group = New Group(docGB, Layout.Inline(New Size(nodeWidth, nodeHeight)))
        Paragraph.Inlines.Add(Group)

        'draw textbox for Goal
        Dim TextBox = New TextBox(docGB,
        Layout.Floating(
            New HorizontalPosition(nodeLeft, LengthUnit.Point, HorizontalPositionAnchor.TopLeftCorner),
            New VerticalPosition(nodeTop, LengthUnit.Point, VerticalPositionAnchor.TopLeftCorner),
            New Size(nodeWidth, nodeHeight)),
                New Paragraph(docGB, objH.Nodes(0).NodeName) With {.ParagraphFormat = New ParagraphFormat() With {.Alignment = HorizontalAlignment.Left}})
        Group.Add(TextBox)

        'save the textbox position in the Word doc and size 
        Textboxes = New List(Of wordTextbox)
        Dim tbox As New wordTextbox(nodeLeft, nodeTop, nodeWidth, nodeHeight, objH.Nodes(0).NodeID)
        Textboxes.Add(tbox)

        'draw textboxes (nodes names) and connectors
        For i As Integer = 1 To objH.Nodes.Count - 1
            Dim N As clsNode = objH.Nodes(i)
            nodeLeft = goalLeft + lineIndent * 2 * N.Level
            nodeTop = nodeTop + nodeHeight * 2

            TextBox = New TextBox(docGB,
            Layout.Floating(
                New HorizontalPosition(nodeLeft, LengthUnit.Point, HorizontalPositionAnchor.TopLeftCorner),
                New VerticalPosition(nodeTop, LengthUnit.Point, VerticalPositionAnchor.TopLeftCorner),
                New Size(nodeWidth, nodeHeight)),
                    New Paragraph(docGB, N.NodeName) With {.ParagraphFormat = New ParagraphFormat() With {.Alignment = HorizontalAlignment.Left}})
            Group.Add(TextBox)

            'save textboxes positions in the Word doc and sizes 
            tbox = New wordTextbox(nodeLeft, nodeTop, nodeWidth, nodeHeight, N.NodeID)
            Textboxes.Add(tbox)

            'horizontal connector
            lineLeft = nodeLeft - lineIndent
            lineTop = nodeTop + nodeHeight / 2
            lineW = nodeLeft - lineLeft
            lineH = 0
            Debug.Print(N.NodeName & "  HlineTop = " & lineTop.ToString & " HlineW = " & lineW.ToString)

            line = New Shape(docGB, ShapeType.Line,
            Layout.Floating(
                New HorizontalPosition(lineLeft, LengthUnit.Point, HorizontalPositionAnchor.TopLeftCorner),
                New VerticalPosition(lineTop, LengthUnit.Point, VerticalPositionAnchor.TopLeftCorner),
                New Size(lineW, lineH)))
            Group.Add(line)

            'draw vertical connector from parent to current
            lineLeft = nodeLeft - lineIndent
            Dim parentTextbox As wordTextbox = getParentTextbox(N.ParentNodeID) 'get parent of current textbox and 
            lineTop = parentTextbox.tboxTop + nodeHeight
            lineW = 0
            lineH = nodeTop - lineTop + nodeHeight / 2
            line = New Shape(docGB, ShapeType.Line,
            Layout.Floating(
                New HorizontalPosition(lineLeft, LengthUnit.Point, HorizontalPositionAnchor.TopLeftCorner),
                New VerticalPosition(lineTop, LengthUnit.Point, VerticalPositionAnchor.TopLeftCorner),
                New Size(lineW, lineH)))
            Group.Add(line)
        Next i

        docGB.Save(filePath)
        Return docGB

    End Function

    Function Draw_Datagrid(filePath As String) As DocumentModel 'AS/17533i

        'calc priorities for Goal and userID=-1 (Combined)
        Dim wrtNode As clsNode = PM.ActiveObjectives.GetNodeByID(1)
        Dim CalcTarget As clsCalculationTarget
        PM.StorageManager.Reader.LoadUserData(PM.GetUserByID(-1))
        CalcTarget = New clsCalculationTarget(CalculationTargetType.cttCombinedGroup, PM.CombinedGroups.GetCombinedGroupByUserID(-1))
        PM.CalculationsManager.Calculate(CalcTarget, wrtNode, PM.ActiveHierarchy, PM.ActiveAltsHierarchy)

        Dim docGB As DocumentModel = New DocumentModel
        Dim table As New Table(docGB)
        table.TableFormat.AutomaticallyResizeToFitContents = False 'AS/17533p

        Dim row As New TableRow(docGB)
        'table.Rows.Add(row)

        Dim objH As clsHierarchy = App.ActiveProject.HierarchyObjectives
        Dim altH As clsHierarchy = App.ActiveProject.HierarchyAlternatives
        Dim columnCount As Integer = objH.Nodes.Count + 1
        Dim rowCount As Integer = altH.Nodes.Count

        Dim colAltWidth As Integer = 150 'AS/17533p===
        Dim colTotalWidth As Integer = 60
        Dim colCovobjWidth As Integer = 60
        Dim colAttributeWidth As Integer = 100 'AS/17533p==

        'Create row with Titles
        row = New TableRow(docGB)
        table.Rows.Add(row)
        'create cell for "Alternatives", merge it with the cell below, center, and set backcolor
        Dim para As New Paragraph(docGB, New Run(docGB, "Alternatives") With {.CharacterFormat = New CharacterFormat With {.Bold = True}}) With {
                    .ParagraphFormat = New ParagraphFormat() With {
                        .Alignment = HorizontalAlignment.Center}}
        row.Cells.Add(New TableCell(docGB, para) With {.RowSpan = 2, .CellFormat = New TableCellFormat() With {
                .VerticalAlignment = VerticalAlignment.Center,
                .BackgroundColor = Color.LightGray,
                .PreferredWidth = New TableWidth(colAltWidth, TableWidthUnit.Point)'AS/17533p
         }})

        'create cell for "Total", merge it with the cell below, center, and set backcolor
        para = New Paragraph(docGB, New Run(docGB, "Total") With {.CharacterFormat = New CharacterFormat With {.Bold = True}})
        row.Cells.Add(New TableCell(docGB, para) With {.RowSpan = 2, .CellFormat = New TableCellFormat() With {
                .VerticalAlignment = VerticalAlignment.Center,
                .BackgroundColor = Color.LightGray,
                .PreferredWidth = New TableWidth(colTotalWidth, TableWidthUnit.Point) 'AS/17533p
         }})

        'create cell for model name, merge it with the cells all way down to the right, set alignments and backcolor
        para = New Paragraph(docGB, New Run(docGB, PM.ProjectName) With {.CharacterFormat = New CharacterFormat With {.Bold = True}}) With {
                    .ParagraphFormat = New ParagraphFormat() With {
                        .Alignment = HorizontalAlignment.Center}}
        row.Cells.Add(New TableCell(docGB, para) With {
                      .ColumnSpan = objH.TerminalNodes.Count, .CellFormat = New TableCellFormat() With {
                .VerticalAlignment = VerticalAlignment.Center,
                .BackgroundColor = Color.LightGray
         }})

        row = New TableRow(docGB)
        table.Rows.Add(row)

        'create cells for covering objectives' names
        For Each node In objH.TerminalNodes
            para = New Paragraph(docGB, New Run(docGB, node.NodeName) With {.CharacterFormat = New CharacterFormat With {.Bold = True}}) With {
                    .ParagraphFormat = New ParagraphFormat() With {
                        .Alignment = HorizontalAlignment.Center}}
            row.Cells.Add(New TableCell(docGB, para) With {
                      .CellFormat = New TableCellFormat() With {
                .VerticalAlignment = VerticalAlignment.Center,
                .BackgroundColor = Color.LightGray,
                .PreferredWidth = New TableWidth(colCovobjWidth, TableWidthUnit.Point)'AS/17533p
         }})
        Next


        'create rows for alternatives
        For i As Integer = 0 To altH.Nodes.Count - 1
            'add alts names to the first col
            Dim alt = altH.Nodes(i)
            row = New TableRow(docGB)
            table.Rows.Add(row)
            para = New Paragraph(docGB, alt.NodeName)
            row.Cells.Add(New TableCell(docGB, para))

            para = New Paragraph(docGB, alt.UnnormalizedPriority.ToString)
            row.Cells.Add(New TableCell(docGB, para))

            'add data to the data columns for each covering objective 
            'FOR THE TIME BEING, DO IT FOR COMBINED (UserID = -1)
            For Each node As clsNode In objH.Nodes
                If node.IsTerminalNode Then
                    Dim sValue As String = "" 'AS/17533i=== mimiced from doImportJudgmentsMatrix
                    Select Case node.MeasureType
                        Case ECMeasureType.mtRatings
                            Dim currJ As clsRatingMeasureData = CType(GetJudgment(node, alt.NodeID, -1), clsRatingMeasureData)
                            Try
                                If Not IsNothing(currJ.Rating.RatingScale) Then
                                    sValue = currJ.Rating.Name
                                Else
                                    sValue = JS_SafeNumber(currJ.Rating.Value)
                                End If
                            Catch
                                sValue = " "
                            End Try

                        Case ECMeasureType.mtDirect, ECMeasureType.mtStep, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtCustomUtilityCurve
                            Dim currJ As clsNonPairwiseMeasureData = GetJudgment(node, alt.NodeID, -1)
                            Try
                                Dim aVal As Double = CDbl(currJ.SingleValue)
                                If aVal = Int32.MinValue Or aVal.Equals(Double.NaN) Then
                                    sValue = " "
                                Else
                                    sValue = JS_SafeNumber(aVal)
                                End If
                            Catch
                                sValue = " "
                            End Try
                        Case Else 'for PW 
                            Dim aVal As Double = node.Judgments.Weights.GetUserWeights(-1, ECSynthesisMode.smIdeal, PM.CalculationsManager.IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(alt.NodeID)
                            sValue = aVal.ToString
                    End Select

                    para = New Paragraph(docGB, sValue)
                    row.Cells.Add(New TableCell(docGB, para))
                End If
            Next
        Next i

        'docGB.Sections.Add(New Section(docGB, table)) 'AS/17533p==
        'docGB.Sections(0).PageSetup.Orientation = Orientation.Landscape 'AS/17533p==

        Dim section As New Section(docGB, table) 'AS/17533p===
        docGB.Sections.Add(section)

        Dim pageSetup = section.PageSetup
        pageSetup.Orientation = Orientation.Landscape
        pageSetup.PaperType = PaperType.A3
        pageSetup.PageMargins.Left = 20
        pageSetup.PageMargins.Right = 20 'AS/17533p==

        docGB.Save(filePath)
        Return docGB

    End Function

    Function Draw_Datagrid_Test(filePath As String) As DocumentModel 'AS/17533p

        Dim docGB As DocumentModel = New DocumentModel
        Dim table As New Table(docGB)
        Dim row As New TableRow(docGB)

        Dim columnCount As Integer = 15
        Dim rowCount As Integer = 10

        'Create row with Titles
        row = New TableRow(docGB)
        table.Rows.Add(row)
        'create cell for "Alternatives", merge it with the cell below, center, and set backcolor
        Dim para As New Paragraph(docGB, New Run(docGB, "Alternatives") With {.CharacterFormat = New CharacterFormat With {.Bold = True}}) With {
                    .ParagraphFormat = New ParagraphFormat() With {
                        .Alignment = HorizontalAlignment.Center}}
        row.Cells.Add(New TableCell(docGB, para) With {.RowSpan = 2, .CellFormat = New TableCellFormat() With {
                .VerticalAlignment = VerticalAlignment.Center,
                .BackgroundColor = Color.LightGray,
                .PreferredWidth = New TableWidth(250, TableWidthUnit.Point)
         }})

        'create cell for "Total", merge it with the cell below, center, and set backcolor
        para = New Paragraph(docGB, New Run(docGB, "Total") With {.CharacterFormat = New CharacterFormat With {.Bold = True}})
        row.Cells.Add(New TableCell(docGB, para) With {.RowSpan = 2, .CellFormat = New TableCellFormat() With {
                .VerticalAlignment = VerticalAlignment.Center,
                .BackgroundColor = Color.LightGray,
                .PreferredWidth = New TableWidth(200, TableWidthUnit.Point)
         }})

        'create cell for model name, merge it with the cells all way down to the right, set alignments and backcolor
        para = New Paragraph(docGB, New Run(docGB, "Model name goes here") With {.CharacterFormat = New CharacterFormat With {.Bold = True}}) With {
                    .ParagraphFormat = New ParagraphFormat() With {
                        .Alignment = HorizontalAlignment.Center}}
        row.Cells.Add(New TableCell(docGB, para) With {
                .ColumnSpan = columnCount, .CellFormat = New TableCellFormat() With {
                .VerticalAlignment = VerticalAlignment.Center,
                .BackgroundColor = Color.LightGray
         }})

        row = New TableRow(docGB)
        table.Rows.Add(row)

        'create cells for covering objectives' names
        For i = 2 To columnCount
            para = New Paragraph(docGB, New Run(docGB, "data column " & i.ToString) With {.CharacterFormat = New CharacterFormat With {.Bold = True}}) With {
                    .ParagraphFormat = New ParagraphFormat() With {
                        .Alignment = HorizontalAlignment.Center}}
            row.Cells.Add(New TableCell(docGB, para) With {
                      .CellFormat = New TableCellFormat() With {
                .VerticalAlignment = VerticalAlignment.Center,
                .BackgroundColor = Color.LightGray,
                .PreferredWidth = New TableWidth(75, TableWidthUnit.Point)
         }})
        Next


        For i As Integer = 0 To rowCount - 1
            row = New TableRow(docGB)
            table.Rows.Add(row)

            For j As Integer = 0 To columnCount
                para = New Paragraph(docGB, String.Format("Cell {0}-{1}", i + 1, j + 1))

                row.Cells.Add(New TableCell(docGB, para))
                row.Cells(j).CellFormat.PreferredWidth = New TableWidth(75, TableWidthUnit.Auto)
                row.Cells(j).CellFormat.WrapText = False
            Next
        Next
        docGB.Sections.Add(New Section(docGB, table))
        docGB.Sections(0).PageSetup.Orientation = Orientation.Landscape 'AS/17533p

        docGB.Save(filePath)
        Return docGB

    End Function

    Private Function GetJudgment(ByVal CovObj As clsNode, altID As Integer, userID As Integer) As clsNonPairwiseMeasureData 'AS/17533i
        Return CType(CovObj.Judgments, clsNonPairwiseJudgments).GetJudgement(altID, CovObj.NodeID, userID)
    End Function


    Private Function getParentTextbox(parentID As Integer) As wordTextbox
        For Each tbox As wordTextbox In Textboxes
            If tbox.nodeID = parentID Then
                Return tbox
            End If
        Next
        Return Nothing

    End Function

    Function Test_Infodoc(filePath As String) As DocumentModel 'AS/17533g

        ' Create new empty document
        Dim docGB As DocumentModel = New DocumentModel()

        docGB.DefaultCharacterFormat.FontName = "Arial"
        docGB.DefaultCharacterFormat.Size = 12

        Dim sModelName As String = PM.ProjectName
        Dim sText As String = Infodoc2Text(App.ActiveProject, PM.Hierarchies(0).Nodes(0).InfoDoc)
        sText = sText.Trim 'remove vbCrLf from the end of the string

        docGB.Sections.Add(
            New Section(docGB,
                New Paragraph(docGB,
                    New Run(docGB, "Model Name:   " & sModelName) With {.CharacterFormat = New CharacterFormat With {.Bold = True, .Size = 16}}) With
                {
                    .ParagraphFormat = New ParagraphFormat() With
                    {
                        .Alignment = HorizontalAlignment.Center
                    }
                },
                New Paragraph(docGB, sText) With
                {
                    .ParagraphFormat = New ParagraphFormat() With
                    {
                        .LeftIndentation = 10,
                        .RightIndentation = 20,
                        .LineSpacingRule = LineSpacingRule.Exactly,
                        .SpaceAfter = 10,
                        .SpaceBefore = 0
                    }
                }))

        docGB.Save(filePath)
        Return docGB

    End Function

    Function Test_InfodocAndPictureAndHierarchy(filePath As String, server As HttpServerUtility) As DocumentModel 'AS/17533e
        ' Create new empty document
        Dim docGB As DocumentModel = New DocumentModel() 'AS/17533a

        docGB.DefaultCharacterFormat.FontName = "Arial"
        docGB.DefaultCharacterFormat.Size = 12

        Dim sModelName As String = PM.ProjectName
        Dim picture1 As New Picture(docGB, server.MapPath("~/Images/favicon/Chart_Car_Alts.png"), 500, 200, LengthUnit.Pixel)

        docGB.Sections.Add(
            New Section(docGB,
                New Paragraph(docGB,
                    New Run(docGB, "Model Name: " & sModelName) With {.CharacterFormat = New CharacterFormat With {.Bold = True, .Size = 16}}) With
                {
                    .ParagraphFormat = New ParagraphFormat() With
                    {
                        .Alignment = HorizontalAlignment.Center
                    }
                },
                New Paragraph(docGB, Infodoc2Text(App.ActiveProject, PM.Hierarchies(0).Nodes(0).InfoDoc)) With
                {
                    .ParagraphFormat = New ParagraphFormat() With
                    {
                        .LeftIndentation = 10,
                        .RightIndentation = 20,
                        .LineSpacingRule = LineSpacingRule.Exactly,
                        .SpaceAfter = 0,
                        .SpaceBefore = 0
                    }
                }))

        'insert picture
        Dim Paragraph = New Paragraph(docGB)
        docGB.Sections(0).Blocks.Add(Paragraph)

        Dim Group = New Group(docGB, Layout.Inline(New Size(440, 100)))
        Paragraph.Inlines.Add(picture1)

        'create new title
        Paragraph = New Paragraph(docGB,
                    New Run(docGB, "Objectives Hierarchy") With {.CharacterFormat = New CharacterFormat With {.Bold = True, .Size = 16}}) With
                {
                    .ParagraphFormat = New ParagraphFormat() With
                    {
                        .Alignment = HorizontalAlignment.Center,
                        .SpaceBefore = 10,
                        .SpaceAfter = 10
                    }
                }
        docGB.Sections(0).Blocks.Add(Paragraph)

        'draw hierarchy as a vertical tree view
        Paragraph = New Paragraph(docGB)
        docGB.Sections(0).Blocks.Add(Paragraph)

        Group = New Group(docGB, Layout.Inline(New Size(440, 100)))
        Paragraph.Inlines.Add(Group)


        Dim TextBox = New TextBox(docGB,
        Layout.Floating(
            New HorizontalPosition(20, LengthUnit.Point, HorizontalPositionAnchor.TopLeftCorner),
            New VerticalPosition(20, LengthUnit.Point, VerticalPositionAnchor.TopLeftCorner),
            New Size(120, 20)),
                New Paragraph(docGB, "Purchase a new car") With {.ParagraphFormat = New ParagraphFormat() With {.Alignment = HorizontalAlignment.Center}})
        Group.Add(TextBox)

        TextBox = New TextBox(docGB,
        Layout.Floating(
            New HorizontalPosition(40, LengthUnit.Point, HorizontalPositionAnchor.TopLeftCorner),
            New VerticalPosition(60, LengthUnit.Point, VerticalPositionAnchor.TopLeftCorner),
            New Size(120, 20)),
                New Paragraph(docGB, "Cost of Ownership") With {.ParagraphFormat = New ParagraphFormat() With {.Alignment = HorizontalAlignment.Center}})
        Group.Add(TextBox)

        TextBox = New TextBox(docGB,
        Layout.Floating(
            New HorizontalPosition(40, LengthUnit.Point, HorizontalPositionAnchor.TopLeftCorner),
            New VerticalPosition(100, LengthUnit.Point, VerticalPositionAnchor.TopLeftCorner),
            New Size(120, 20)),
                New Paragraph(docGB, "Performance") With {.ParagraphFormat = New ParagraphFormat() With {.Alignment = HorizontalAlignment.Center}})
        Group.Add(TextBox)

        TextBox = New TextBox(docGB,
        Layout.Floating(
            New HorizontalPosition(40, LengthUnit.Point, HorizontalPositionAnchor.TopLeftCorner),
            New VerticalPosition(140, LengthUnit.Point, VerticalPositionAnchor.TopLeftCorner),
            New Size(120, 20)),
                New Paragraph(docGB, "Style") With {.ParagraphFormat = New ParagraphFormat() With {.Alignment = HorizontalAlignment.Center}})
        Group.Add(TextBox)

        Dim line = New Shape(docGB, ShapeType.Line,
        Layout.Floating(
            New HorizontalPosition(30, LengthUnit.Point, HorizontalPositionAnchor.TopLeftCorner),
            New VerticalPosition(40, LengthUnit.Point, VerticalPositionAnchor.TopLeftCorner),
            New Size(0, 110)))
        Group.Add(line)

        line = New Shape(docGB, ShapeType.Line,
        Layout.Floating(
            New HorizontalPosition(30, LengthUnit.Point, HorizontalPositionAnchor.TopLeftCorner),
            New VerticalPosition(70, LengthUnit.Point, VerticalPositionAnchor.TopLeftCorner),
            New Size(10, 0)))
        Group.Add(line)

        line = New Shape(docGB, ShapeType.Line,
        Layout.Floating(
            New HorizontalPosition(30, LengthUnit.Point, HorizontalPositionAnchor.TopLeftCorner),
            New VerticalPosition(110, LengthUnit.Point, VerticalPositionAnchor.TopLeftCorner),
            New Size(10, 0)))
        Group.Add(line)

        line = New Shape(docGB, ShapeType.Line,
        Layout.Floating(
            New HorizontalPosition(30, LengthUnit.Point, HorizontalPositionAnchor.TopLeftCorner),
            New VerticalPosition(150, LengthUnit.Point, VerticalPositionAnchor.TopLeftCorner),
            New Size(10, 0)))
        Group.Add(line)


        docGB.Save(filePath) 'AS/17533a
        Return docGB 'AS/17533a

    End Function

    Function Test_InfodocAndHierarchy(filePath As String) As DocumentModel 'AS/17663b 'AS/17533a added docGB 
        'Sub Test_InfodocAndHierarchy(filePath As String) 'AS/17663b 'AS/17533a

        ' Create new empty document
        Dim docGB As DocumentModel = New DocumentModel() 'AS/17533a

        docGB.DefaultCharacterFormat.FontName = "Arial"
        docGB.DefaultCharacterFormat.Size = 12

        Dim sModelName As String = PM.ProjectName

        docGB.Sections.Add(
            New Section(docGB,
                New Paragraph(docGB,
                    New Run(docGB, "Model Name: " & sModelName) With {.CharacterFormat = New CharacterFormat With {.Bold = True, .Size = 16}}) With
                {
                    .ParagraphFormat = New ParagraphFormat() With
                    {
                        .Alignment = HorizontalAlignment.Center
                    }
                },
                New Paragraph(docGB, Infodoc2Text(App.ActiveProject, PM.Hierarchies(0).Nodes(0).InfoDoc)) With
                {
                    .ParagraphFormat = New ParagraphFormat() With
                    {
                        .LeftIndentation = 10,
                        .RightIndentation = 20,
                        .LineSpacingRule = LineSpacingRule.Exactly,
                        .SpaceAfter = 10,
                        .SpaceBefore = 0
                    }
                },
                New Paragraph(docGB,
                    New Run(docGB, "Objectives Hierarchy -- Horizontal Tree View") With {.CharacterFormat = New CharacterFormat With {.Bold = True, .Size = 16}}) With
                {
                    .ParagraphFormat = New ParagraphFormat() With
                    {
                        .Alignment = HorizontalAlignment.Center,
                        .SpaceBefore = 10,
                        .SpaceAfter = 10
                    }
                }))

        'draw hierarchy as a horizontal tree view
        Dim Paragraph = New Paragraph(docGB)
        docGB.Sections(0).Blocks.Add(Paragraph)

        Dim Group = New Group(docGB, Layout.Inline(New Size(440, 100)))
        Paragraph.Inlines.Add(Group)

        Dim TextBox = New TextBox(docGB,
                Layout.Floating(
                    New HorizontalPosition(160, LengthUnit.Point, HorizontalPositionAnchor.TopLeftCorner),
                    New VerticalPosition(20, LengthUnit.Point, VerticalPositionAnchor.TopLeftCorner),
                    New Size(120, 20)),
                New Paragraph(docGB, "Purchase a new car") With {.ParagraphFormat = New ParagraphFormat() With {.Alignment = HorizontalAlignment.Center}})
        Group.Add(TextBox)

        TextBox = New TextBox(docGB,
                Layout.Floating(
                    New HorizontalPosition(20, LengthUnit.Point, HorizontalPositionAnchor.TopLeftCorner),
                    New VerticalPosition(60, LengthUnit.Point, VerticalPositionAnchor.TopLeftCorner),
                    New Size(120, 20)),
                New Paragraph(docGB, "Cost of Ownership") With {.ParagraphFormat = New ParagraphFormat() With {.Alignment = HorizontalAlignment.Center}})
        Group.Add(TextBox)

        TextBox = New TextBox(docGB,
                Layout.Floating(
                    New HorizontalPosition(160, LengthUnit.Point, HorizontalPositionAnchor.TopLeftCorner),
                    New VerticalPosition(60, LengthUnit.Point, VerticalPositionAnchor.TopLeftCorner),
                    New Size(120, 20)),
                New Paragraph(docGB, "Performance") With {.ParagraphFormat = New ParagraphFormat() With {.Alignment = HorizontalAlignment.Center}})
        Group.Add(TextBox)

        TextBox = New TextBox(docGB,
                Layout.Floating(
                    New HorizontalPosition(300, LengthUnit.Point, HorizontalPositionAnchor.TopLeftCorner),
                    New VerticalPosition(60, LengthUnit.Point, VerticalPositionAnchor.TopLeftCorner),
                    New Size(120, 20)),
                New Paragraph(docGB, "Style") With {.ParagraphFormat = New ParagraphFormat() With {.Alignment = HorizontalAlignment.Center}})
        Group.Add(TextBox)

        Dim line = New Shape(docGB, ShapeType.Line,
                Layout.Floating(
                    New HorizontalPosition(220, LengthUnit.Point, HorizontalPositionAnchor.TopLeftCorner),
                    New VerticalPosition(40, LengthUnit.Point, VerticalPositionAnchor.TopLeftCorner),
                    New Size(0, 20)))
        Group.Add(line)

        line = New Shape(docGB, ShapeType.Line,
                Layout.Floating(
                    New HorizontalPosition(80, LengthUnit.Point, HorizontalPositionAnchor.TopLeftCorner),
                    New VerticalPosition(50, LengthUnit.Point, VerticalPositionAnchor.TopLeftCorner),
                    New Size(0, 10)))
        Group.Add(line)

        line = New Shape(docGB, ShapeType.Line,
                Layout.Floating(
                    New HorizontalPosition(220, LengthUnit.Point, HorizontalPositionAnchor.TopLeftCorner),
                    New VerticalPosition(50, LengthUnit.Point, VerticalPositionAnchor.TopLeftCorner),
                    New Size(0, 10)))
        Group.Add(line)

        line = New Shape(docGB, ShapeType.Line,
                Layout.Floating(
                    New HorizontalPosition(360, LengthUnit.Point, HorizontalPositionAnchor.TopLeftCorner),
                    New VerticalPosition(50, LengthUnit.Point, VerticalPositionAnchor.TopLeftCorner),
                    New Size(0, 10)))
        Group.Add(line)

        line = New Shape(docGB, ShapeType.Line,
                Layout.Floating(
                    New HorizontalPosition(80, LengthUnit.Point, HorizontalPositionAnchor.TopLeftCorner),
                    New VerticalPosition(50, LengthUnit.Point, VerticalPositionAnchor.TopLeftCorner),
                    New Size(280, 0)))
        Group.Add(line)

        Paragraph = New Paragraph(docGB,
            New Run(docGB, "Objectives Hierarchy -- Vertical Tree View") With {.CharacterFormat = New CharacterFormat With {.Bold = True, .Size = 16}}) With
                {
                    .ParagraphFormat = New ParagraphFormat() With
                    {
                        .Alignment = HorizontalAlignment.Center,
                        .SpaceBefore = 30,
                        .SpaceAfter = 10
                    }
                }
        docGB.Sections(0).Blocks.Add(Paragraph)

        'draw hierarchy as a vertical tree view
        Paragraph = New Paragraph(docGB)
        docGB.Sections(0).Blocks.Add(Paragraph)

        Group = New Group(docGB, Layout.Inline(New Size(440, 100)))
        Paragraph.Inlines.Add(Group)

        TextBox = New TextBox(docGB,
        Layout.Floating(
            New HorizontalPosition(20, LengthUnit.Point, HorizontalPositionAnchor.TopLeftCorner),
            New VerticalPosition(20, LengthUnit.Point, VerticalPositionAnchor.TopLeftCorner),
            New Size(120, 20)),
                New Paragraph(docGB, "Purchase a new car") With {.ParagraphFormat = New ParagraphFormat() With {.Alignment = HorizontalAlignment.Center}})
        Group.Add(TextBox)

        TextBox = New TextBox(docGB,
        Layout.Floating(
            New HorizontalPosition(40, LengthUnit.Point, HorizontalPositionAnchor.TopLeftCorner),
            New VerticalPosition(60, LengthUnit.Point, VerticalPositionAnchor.TopLeftCorner),
            New Size(120, 20)),
                New Paragraph(docGB, "Cost of Ownership") With {.ParagraphFormat = New ParagraphFormat() With {.Alignment = HorizontalAlignment.Center}})
        Group.Add(TextBox)

        TextBox = New TextBox(docGB,
        Layout.Floating(
            New HorizontalPosition(40, LengthUnit.Point, HorizontalPositionAnchor.TopLeftCorner),
            New VerticalPosition(100, LengthUnit.Point, VerticalPositionAnchor.TopLeftCorner),
            New Size(120, 20)),
                New Paragraph(docGB, "Performance") With {.ParagraphFormat = New ParagraphFormat() With {.Alignment = HorizontalAlignment.Center}})
        Group.Add(TextBox)

        TextBox = New TextBox(docGB,
        Layout.Floating(
            New HorizontalPosition(40, LengthUnit.Point, HorizontalPositionAnchor.TopLeftCorner),
            New VerticalPosition(140, LengthUnit.Point, VerticalPositionAnchor.TopLeftCorner),
            New Size(120, 20)),
                New Paragraph(docGB, "Style") With {.ParagraphFormat = New ParagraphFormat() With {.Alignment = HorizontalAlignment.Center}})
        Group.Add(TextBox)

        line = New Shape(docGB, ShapeType.Line,
        Layout.Floating(
            New HorizontalPosition(30, LengthUnit.Point, HorizontalPositionAnchor.TopLeftCorner),
            New VerticalPosition(40, LengthUnit.Point, VerticalPositionAnchor.TopLeftCorner),
            New Size(0, 110)))
        Group.Add(line)

        line = New Shape(docGB, ShapeType.Line,
        Layout.Floating(
            New HorizontalPosition(30, LengthUnit.Point, HorizontalPositionAnchor.TopLeftCorner),
            New VerticalPosition(70, LengthUnit.Point, VerticalPositionAnchor.TopLeftCorner),
            New Size(10, 0)))
        Group.Add(line)

        line = New Shape(docGB, ShapeType.Line,
        Layout.Floating(
            New HorizontalPosition(30, LengthUnit.Point, HorizontalPositionAnchor.TopLeftCorner),
            New VerticalPosition(110, LengthUnit.Point, VerticalPositionAnchor.TopLeftCorner),
            New Size(10, 0)))
        Group.Add(line)

        line = New Shape(docGB, ShapeType.Line,
        Layout.Floating(
            New HorizontalPosition(30, LengthUnit.Point, HorizontalPositionAnchor.TopLeftCorner),
            New VerticalPosition(150, LengthUnit.Point, VerticalPositionAnchor.TopLeftCorner),
            New Size(10, 0)))
        Group.Add(line)


        'add title for the next part
        Paragraph = New Paragraph(docGB,
            New Run(docGB, "Objectives Hierarchy -- Numbered Multi-Level List View") With {.CharacterFormat = New CharacterFormat With {.Bold = True, .Size = 16}}) With
                {
                    .ParagraphFormat = New ParagraphFormat() With
                    {
                        .Alignment = HorizontalAlignment.Center,
                        .SpaceBefore = 30,
                        .SpaceAfter = 30
                    }
                }
        docGB.Sections(0).Blocks.Add(Paragraph)

        ' draw hierarchy as a numbered multi-level list
        Dim numberList As New ListStyle(ListTemplateType.NumberWithDot)

        ' Create number list items.
        Dim H As clsHierarchy = App.ActiveProject.HierarchyObjectives
        For i = 0 To H.Nodes.Count - 1
            Dim N As clsNode = H.Nodes(i)
            Paragraph = New Paragraph(docGB, N.NodeName)

            Paragraph.ListFormat.Style = numberList
            Paragraph.ListFormat.ListLevelNumber = N.Level

            docGB.Sections(0).Blocks.Add(Paragraph)

        Next

        docGB.Save(filePath) 'AS/17533a
        Return docGB 'AS/17533a

    End Function

    Function Test_InlineTextboxAndPicture(filePath As String, server As HttpServerUtility) As DocumentModel 'AS/17533e

        Dim document As DocumentModel = DocumentModel.Load("D:\17533 temp\Text Boxes - TEST.docx")
        Dim section As New Section(document)
        document.Sections.Add(section)

        Dim paragraph = document.GetChildElements(True).OfType(Of Paragraph)().Last()

        '' Create and add an inline textbox.
        'Dim textbox1 As New TextBox(document,
        '    Layout.Inline(New Size(10, 5, LengthUnit.Centimeter)),
        '    ShapeType.Diamond)

        'textbox1.Blocks.Add(New Paragraph(document, "An inline TextBox created with GemBox.Document."))
        'textbox1.Blocks.Add(New Paragraph(document, "It has blue fill and outline."))

        'textbox1.Fill.SetSolid(New Color(222, 235, 247))
        'textbox1.Outline.Fill.SetSolid(Color.Blue)

        'paragraph.Inlines.Insert(0, textbox1)


        paragraph = New Paragraph(document)
        section.Blocks.Add(paragraph)

        Dim picture1 As New Picture(document, server.MapPath("~/Images/favicon/apple-icon.png"), 100, 100, LengthUnit.Pixel)
        paragraph.Inlines.Add(picture1)

        '' Create and add a floating textbox.
        'Dim textBox2 As New TextBox(document,
        '    Layout.Floating(
        '        New HorizontalPosition(HorizontalPositionType.Right, HorizontalPositionAnchor.Margin),
        '        New VerticalPosition(VerticalPositionType.Bottom, VerticalPositionAnchor.Margin),
        '        New Size(6, 3, LengthUnit.Centimeter)),
        '    New Paragraph(document, "A floating TextBox created with GemBox.Document."),
        '    New Paragraph(document, "It has default fill and outline."))

        'paragraph.Inlines.Add(textBox2)

        document.Save(filePath)
        Return document

    End Function

    Function Test_InsertPicture(filePath As String, server As HttpServerUtility) As DocumentModel 'AS/17533d
        document.DefaultCharacterFormat.FontName = "Arial"
        Dim section As New Section(document)
        document.Sections.Add(section)

        Dim paragraph As New Paragraph(document)
        section.Blocks.Add(paragraph)

        ' Create and add an inline picture with GIF image.
        'Dim picture1 As New Picture(document, "Zahnrad.gif", 61, 53, LengthUnit.Pixel)
        Dim picture1 As New Picture(document, server.MapPath("~/Images/favicon/apple-icon.png"), 61, 53, LengthUnit.Pixel)
        paragraph.Inlines.Add(picture1)

        '' Create and add a floating picture with PNG image.
        'Dim picture2 As New Picture(document, "Dices.png")
        'Dim layout2 As New FloatingLayout(
        '    New HorizontalPosition(HorizontalPositionType.Left, HorizontalPositionAnchor.Page),
        '    New VerticalPosition(2, LengthUnit.Inch, VerticalPositionAnchor.Page),
        '    picture2.Layout.Size)
        'layout2.WrappingStyle = TextWrappingStyle.InFrontOfText

        'picture2.Layout = layout2
        'paragraph.Inlines.Add(picture2)

        '' Create and add a floating picture with WMF image.
        'Dim picture3 As New Picture(document, "Graphics1.wmf", 378, 189, LengthUnit.Pixel)
        'Dim layout3 As New FloatingLayout(
        '    New HorizontalPosition(3.5, LengthUnit.Inch, HorizontalPositionAnchor.Page),
        '    New VerticalPosition(2, LengthUnit.Inch, VerticalPositionAnchor.Page),
        '    picture3.Layout.Size)
        'layout3.WrappingStyle = TextWrappingStyle.BehindText

        'picture3.Layout = layout3
        'paragraph.Inlines.Add(picture3)


        document.Save(filePath)
        Return document

    End Function

    Function Example_InlineTextbox(filePath As String) As DocumentModel 'AS/17533e

        Dim document As DocumentModel = DocumentModel.Load("D:\17533 temp\Text Boxes.docx")
        Dim paragraph = document.GetChildElements(True).OfType(Of Paragraph)().First()

        ' Create and add an inline textbox.
        Dim textbox1 As New TextBox(document,
            Layout.Inline(New Size(10, 5, LengthUnit.Centimeter)),
            ShapeType.Diamond)

        textbox1.Blocks.Add(New Paragraph(document, "An inline TextBox created with GemBox.Document."))
        textbox1.Blocks.Add(New Paragraph(document, "It has blue fill and outline."))

        textbox1.Fill.SetSolid(New Color(222, 235, 247))
        textbox1.Outline.Fill.SetSolid(Color.Blue)

        paragraph.Inlines.Insert(0, textbox1)

        ' Create and add a floating textbox.
        Dim textBox2 As New TextBox(document,
            Layout.Floating(
                New HorizontalPosition(HorizontalPositionType.Right, HorizontalPositionAnchor.Margin),
                New VerticalPosition(VerticalPositionType.Bottom, VerticalPositionAnchor.Margin),
                New Size(6, 3, LengthUnit.Centimeter)),
            New Paragraph(document, "A floating TextBox created with GemBox.Document."),
            New Paragraph(document, "It has default fill and outline."))

        paragraph.Inlines.Add(textBox2)

        document.Save(filePath)
        Return document

    End Function

    Function Example_InsertPicture(filePath As String) As DocumentModel
        document.DefaultCharacterFormat.FontName = "Arial"
        Dim section As New Section(document)
        document.Sections.Add(section)

        Dim paragraph As New Paragraph(document)
        section.Blocks.Add(paragraph)

        ' Create and add an inline picture with GIF image.
        'Dim picture1 As New Picture(document, "Zahnrad.gif", 61, 53, LengthUnit.Pixel)
        Dim picture1 As New Picture(document, "E:\DropboxAAA\Public\EC\Models EC\AS Test Models\17533 Combined Report\Flowers.jpg", 61, 53, LengthUnit.Pixel)
        paragraph.Inlines.Add(picture1)

        '' Create and add a floating picture with PNG image.
        'Dim picture2 As New Picture(document, "Dices.png")
        'Dim layout2 As New FloatingLayout(
        '    New HorizontalPosition(HorizontalPositionType.Left, HorizontalPositionAnchor.Page),
        '    New VerticalPosition(2, LengthUnit.Inch, VerticalPositionAnchor.Page),
        '    picture2.Layout.Size)
        'layout2.WrappingStyle = TextWrappingStyle.InFrontOfText

        'picture2.Layout = layout2
        'paragraph.Inlines.Add(picture2)

        '' Create and add a floating picture with WMF image.
        'Dim picture3 As New Picture(document, "Graphics1.wmf", 378, 189, LengthUnit.Pixel)
        'Dim layout3 As New FloatingLayout(
        '    New HorizontalPosition(3.5, LengthUnit.Inch, HorizontalPositionAnchor.Page),
        '    New VerticalPosition(2, LengthUnit.Inch, VerticalPositionAnchor.Page),
        '    picture3.Layout.Size)
        'layout3.WrappingStyle = TextWrappingStyle.BehindText

        'picture3.Layout = layout3
        'paragraph.Inlines.Add(picture3)


        document.Save(filePath)
        Return document

    End Function 'AS/17533d

    Sub Test_CreateDataTable(filePath As String) 'AS/17663c
        'Using the previously downloaded CSV file (ECC functionality), first import it to DataTable and then insert the DataTble to the Word doc
        'source 1: https://www.codeproject.com/Questions/600823/ImportingplusCSVplustoplusDataTableplusVB-Net
        'source 2: https://www.gemboxsoftware.com/document/examples/c-sharp-vb-net-insert-datatable-to-word/1202

        Dim SR As StreamReader = New StreamReader("E:\Dropbox\Public\EC\Models EC\AS Test Models\17533 Combined Report\17663 Car Purchase-Alternatives - Shortened.csv")
        Dim line As String = SR.ReadLine()
        Dim strArray As String() = line.Split(";"c)
        Dim dt As DataTable = New DataTable()
        Dim row As DataRow

        ' Populate DataTable
        For Each s As String In strArray
            dt.Columns.Add(New DataColumn())
        Next

        If Not line = String.Empty Then 'insert the first row - the titles
            row = dt.NewRow()
            row.ItemArray = line.Split(";"c)
            dt.Rows.Add(row)
        End If

        Do 'insert the rest of the rows
            line = SR.ReadLine
            If Not line = String.Empty Then
                row = dt.NewRow()
                row.ItemArray = line.Split(";"c)
                dt.Rows.Add(row)
            Else
                Exit Do
            End If
        Loop

        Dim tableRowCount As Integer = dt.Rows.Count
        Dim tableColumnCount As Integer = dt.Columns.Count

        ' Initialize Table in the Wolrd doc
        Dim table As New Table(document, tableRowCount, tableColumnCount,
                               Function(rowIndex As Integer, columnIndex As Integer) New TableCell(document, New Paragraph(document, dt.Rows(rowIndex)(columnIndex).ToString())))

        table.TableFormat.PreferredWidth = New TableWidth(100, TableWidthUnit.Percentage)

        document.Sections.Add(New Section(document, table))

        document.Save(filePath)

    End Sub

    Sub Test_FormatCharacters(filePath As String)
        document.DefaultCharacterFormat.FontName = "Arial"
        document.DefaultCharacterFormat.Size = 16

        Dim lineBreakElement As New SpecialCharacter(document, SpecialCharacterType.LineBreak)

        document.Sections.Add(
            New Section(document,
                New Paragraph(document,
                    New Run(document, "Bold text 24 points") With {.CharacterFormat = New CharacterFormat With {.Bold = True, .Size = 24}}) With
                {
                    .ParagraphFormat = New ParagraphFormat() With
                    {
                        .Alignment = HorizontalAlignment.Center,
                        .SpaceBefore = 30,
                        .SpaceAfter = 30
                    }
                }
                )
            )

        document.Save(filePath)

    End Sub

    Sub Test_FormatInfodoc(filePath As String)
        document.Sections.Add(
            New Section(document,
                New Paragraph(document, "Infodoc for Goal") With
                {
                    .ParagraphFormat = New ParagraphFormat() With
                    {
                        .Alignment = HorizontalAlignment.Center
                    }
                },
                New Paragraph(document, Infodoc2Text(App.ActiveProject, PM.Hierarchies(0).Nodes(0).InfoDoc)) With
                {
                    .ParagraphFormat = New ParagraphFormat() With
                    {
                        .LeftIndentation = 10,
                        .RightIndentation = 20,
                        .LineSpacingRule = LineSpacingRule.Exactly,
                        .SpaceAfter = 10,
                        .SpaceBefore = 0
                    }
                }))


        document.Save(filePath)

    End Sub

    Sub Test_ListHierarchy(filePath As String)

        Dim listItemsCount As Integer = 12
        Dim section As New Section(document)
        document.Sections.Add(section)

        ' Create number list style.
        Dim numberList As New ListStyle(ListTemplateType.NumberWithDot)

        ' Customize list level formats.
        'For level = 0 To numberList.ListLevelFormats.Count - 1

        '    Dim levelFormat As ListLevelFormat = numberList.ListLevelFormats(level)

        '    levelFormat.ParagraphFormat.NoSpaceBetweenParagraphsOfSameStyle = True
        '    levelFormat.Alignment = HorizontalAlignment.Left
        '    levelFormat.NumberStyle = NumberStyle.Decimal

        '    levelFormat.NumberPosition = 18 * level
        '    levelFormat.NumberFormat = String.Concat(Enumerable.Range(1, level + 1).Select(Function(i) $"%{i}."))

        'Next

        ' Create number list items.
        Dim H As clsHierarchy = App.ActiveProject.HierarchyObjectives
        For i = 0 To 10 'H.Nodes.Count - 1
            Dim N As clsNode = H.Nodes(i)
            Dim paragraph As New Paragraph(document, N.NodeName)

            paragraph.ListFormat.Style = numberList
            paragraph.ListFormat.ListLevelNumber = N.Level

            section.Blocks.Add(paragraph)

        Next

        document.Save(filePath)

    End Sub

    Sub Example_CreateDataTable(filePath As String) 'AS/17663c

        Dim tableRowCount As Integer = 4
        Dim tableColumnCount As Integer = 5

        ' Initialize DataTable
        Dim dt As New DataTable()

        For i As Integer = 0 To tableColumnCount - 1
            dt.Columns.Add()
        Next

        For i As Integer = 0 To tableRowCount - 1
            Dim row As DataRow = dt.NewRow()
            For j As Integer = 0 To tableColumnCount - 1
                row(j) = String.Format("Cell {0}-{1}", i + 1, j + 1)
            Next
            dt.Rows.Add(row)
        Next

        ' Initialize Table
        Dim table As New Table(document, tableRowCount, tableColumnCount,
                               Function(rowIndex As Integer, columnIndex As Integer) New TableCell(document, New Paragraph(document, dt.Rows(rowIndex)(columnIndex).ToString())))

        table.TableFormat.PreferredWidth = New TableWidth(100, TableWidthUnit.Percentage)

        document.Sections.Add(New Section(document, table))

        document.Save(filePath)

    End Sub

    Sub Example_CreateSimpleTable(filePath As String) 'AS/17663c
        Dim tableRowCount As Integer = 4
        Dim tableColumnCount As Integer = 5

        Dim table As New Table(document)
        table.TableFormat.PreferredWidth = New TableWidth(100, TableWidthUnit.Percentage)

        For i As Integer = 0 To tableRowCount - 1
            Dim row As New TableRow(document)
            table.Rows.Add(row)

            For j As Integer = 0 To tableColumnCount - 1
                Dim para As New Paragraph(document, String.Format("Cell {0}-{1}", i + 1, j + 1))

                row.Cells.Add(New TableCell(document, para))
            Next
        Next

        document.Sections.Add(New Section(document, table))

        document.Save(filePath)

    End Sub

    Sub Example_Treeview_Vertical(filePath As String)

        document.DefaultCharacterFormat.FontName = "Arial"
        document.DefaultCharacterFormat.Size = 8

        Dim Section = New Section(document)
        document.Sections.Add(Section)

        Dim Paragraph = New Paragraph(document)
        Section.Blocks.Add(Paragraph)

        Dim Group = New Group(document, Layout.Inline(New Size(280, 180)))
        Paragraph.Inlines.Add(Group)

        Dim TextBox = New TextBox(document,
        Layout.Floating(
            New HorizontalPosition(20, LengthUnit.Point, HorizontalPositionAnchor.TopLeftCorner),
            New VerticalPosition(20, LengthUnit.Point, VerticalPositionAnchor.TopLeftCorner),
            New Size(120, 20)),
                New Paragraph(document, "Purchase a new car") With {.ParagraphFormat = New ParagraphFormat() With {.Alignment = HorizontalAlignment.Center}})
        Group.Add(TextBox)

        TextBox = New TextBox(document,
        Layout.Floating(
            New HorizontalPosition(40, LengthUnit.Point, HorizontalPositionAnchor.TopLeftCorner),
            New VerticalPosition(60, LengthUnit.Point, VerticalPositionAnchor.TopLeftCorner),
            New Size(120, 20)),
                New Paragraph(document, "Cost of Ownership") With {.ParagraphFormat = New ParagraphFormat() With {.Alignment = HorizontalAlignment.Center}})
        Group.Add(TextBox)

        TextBox = New TextBox(document,
        Layout.Floating(
            New HorizontalPosition(40, LengthUnit.Point, HorizontalPositionAnchor.TopLeftCorner),
            New VerticalPosition(100, LengthUnit.Point, VerticalPositionAnchor.TopLeftCorner),
            New Size(120, 20)),
                New Paragraph(document, "Performance") With {.ParagraphFormat = New ParagraphFormat() With {.Alignment = HorizontalAlignment.Center}})
        Group.Add(TextBox)

        TextBox = New TextBox(document,
        Layout.Floating(
            New HorizontalPosition(40, LengthUnit.Point, HorizontalPositionAnchor.TopLeftCorner),
            New VerticalPosition(140, LengthUnit.Point, VerticalPositionAnchor.TopLeftCorner),
            New Size(120, 20)),
                New Paragraph(document, "Style") With {.ParagraphFormat = New ParagraphFormat() With {.Alignment = HorizontalAlignment.Center}})
        Group.Add(TextBox)

        Dim line = New Shape(document, ShapeType.Line,
        Layout.Floating(
            New HorizontalPosition(30, LengthUnit.Point, HorizontalPositionAnchor.TopLeftCorner),
            New VerticalPosition(40, LengthUnit.Point, VerticalPositionAnchor.TopLeftCorner),
            New Size(0, 110)))
        Group.Add(line)

        line = New Shape(document, ShapeType.Line,
        Layout.Floating(
            New HorizontalPosition(30, LengthUnit.Point, HorizontalPositionAnchor.TopLeftCorner),
            New VerticalPosition(70, LengthUnit.Point, VerticalPositionAnchor.TopLeftCorner),
            New Size(10, 0)))
        Group.Add(line)

        line = New Shape(document, ShapeType.Line,
        Layout.Floating(
            New HorizontalPosition(30, LengthUnit.Point, HorizontalPositionAnchor.TopLeftCorner),
            New VerticalPosition(110, LengthUnit.Point, VerticalPositionAnchor.TopLeftCorner),
            New Size(10, 0)))
        Group.Add(line)

        line = New Shape(document, ShapeType.Line,
        Layout.Floating(
            New HorizontalPosition(30, LengthUnit.Point, HorizontalPositionAnchor.TopLeftCorner),
            New VerticalPosition(150, LengthUnit.Point, VerticalPositionAnchor.TopLeftCorner),
            New Size(10, 0)))
        Group.Add(line)

        document.Save(filePath)
    End Sub

    Sub Example_Treeview_Horizontal(filePath As String)

        'From texh support:
        'You 'll need to create this diagram (AS: treeview) using the shapes and textboxes:
        'https://www.gemboxsoftware.com/document/examples/word-shapes/203
        'https://www.gemboxsoftware.com/document/examples/word-textboxes/202

        document.DefaultCharacterFormat.FontName = "Arial"
        document.DefaultCharacterFormat.Size = 8

        Dim Section = New Section(document)
        document.Sections.Add(Section)

        Dim Paragraph = New Paragraph(document)
        Section.Blocks.Add(Paragraph)

        Dim Group = New Group(document, Layout.Inline(New Size(440, 100)))
        Paragraph.Inlines.Add(Group)

        Dim TextBox = New TextBox(document,
                Layout.Floating(
                    New HorizontalPosition(160, LengthUnit.Point, HorizontalPositionAnchor.TopLeftCorner),
                    New VerticalPosition(20, LengthUnit.Point, VerticalPositionAnchor.TopLeftCorner),
                    New Size(120, 20)),
                New Paragraph(document, "Purchase a new car") With {.ParagraphFormat = New ParagraphFormat() With {.Alignment = HorizontalAlignment.Center}})
        Group.Add(TextBox)

        TextBox = New TextBox(document,
                Layout.Floating(
                    New HorizontalPosition(20, LengthUnit.Point, HorizontalPositionAnchor.TopLeftCorner),
                    New VerticalPosition(60, LengthUnit.Point, VerticalPositionAnchor.TopLeftCorner),
                    New Size(120, 20)),
                New Paragraph(document, "Cost of Ownership") With {.ParagraphFormat = New ParagraphFormat() With {.Alignment = HorizontalAlignment.Center}})
        Group.Add(TextBox)

        TextBox = New TextBox(document,
                Layout.Floating(
                    New HorizontalPosition(160, LengthUnit.Point, HorizontalPositionAnchor.TopLeftCorner),
                    New VerticalPosition(60, LengthUnit.Point, VerticalPositionAnchor.TopLeftCorner),
                    New Size(120, 20)),
                New Paragraph(document, "Performance") With {.ParagraphFormat = New ParagraphFormat() With {.Alignment = HorizontalAlignment.Center}})
        Group.Add(TextBox)

        TextBox = New TextBox(document,
                Layout.Floating(
                    New HorizontalPosition(300, LengthUnit.Point, HorizontalPositionAnchor.TopLeftCorner),
                    New VerticalPosition(60, LengthUnit.Point, VerticalPositionAnchor.TopLeftCorner),
                    New Size(120, 20)),
                New Paragraph(document, "Style") With {.ParagraphFormat = New ParagraphFormat() With {.Alignment = HorizontalAlignment.Center}})
        Group.Add(TextBox)

        Dim line = New Shape(document, ShapeType.Line,
                Layout.Floating(
                    New HorizontalPosition(220, LengthUnit.Point, HorizontalPositionAnchor.TopLeftCorner),
                    New VerticalPosition(40, LengthUnit.Point, VerticalPositionAnchor.TopLeftCorner),
                    New Size(0, 20)))
        Group.Add(line)

        line = New Shape(document, ShapeType.Line,
                Layout.Floating(
                    New HorizontalPosition(80, LengthUnit.Point, HorizontalPositionAnchor.TopLeftCorner),
                    New VerticalPosition(50, LengthUnit.Point, VerticalPositionAnchor.TopLeftCorner),
                    New Size(0, 10)))
        Group.Add(line)

        line = New Shape(document, ShapeType.Line,
                Layout.Floating(
                    New HorizontalPosition(220, LengthUnit.Point, HorizontalPositionAnchor.TopLeftCorner),
                    New VerticalPosition(50, LengthUnit.Point, VerticalPositionAnchor.TopLeftCorner),
                    New Size(0, 10)))
        Group.Add(line)

        line = New Shape(document, ShapeType.Line,
                Layout.Floating(
                    New HorizontalPosition(360, LengthUnit.Point, HorizontalPositionAnchor.TopLeftCorner),
                    New VerticalPosition(50, LengthUnit.Point, VerticalPositionAnchor.TopLeftCorner),
                    New Size(0, 10)))
        Group.Add(line)

        line = New Shape(document, ShapeType.Line,
                Layout.Floating(
                    New HorizontalPosition(80, LengthUnit.Point, HorizontalPositionAnchor.TopLeftCorner),
                    New VerticalPosition(50, LengthUnit.Point, VerticalPositionAnchor.TopLeftCorner),
                    New Size(280, 0)))
        Group.Add(line)

        document.Save(filePath)
    End Sub

    Sub Example_FormatCharacters(filePath As String)
        document.DefaultCharacterFormat.FontName = "Arial"
        document.DefaultCharacterFormat.Size = 16

        Dim lineBreakElement As New SpecialCharacter(document, SpecialCharacterType.LineBreak)

        document.Sections.Add(
            New Section(document,
                New Paragraph(document,
                    New Run(document, "All caps") With {.CharacterFormat = New CharacterFormat With {.AllCaps = True}},
                    lineBreakElement,
                    New Run(document, "Text with background color") With {.CharacterFormat = New CharacterFormat With {.BackgroundColor = Color.Cyan}},
                    lineBreakElement.Clone(),
                    New Run(document, "Bold text") With {.CharacterFormat = New CharacterFormat With {.Bold = True}},
                    lineBreakElement.Clone(),
                    New Run(document, "Text with borders") With {.CharacterFormat = New CharacterFormat With {.Border = New SingleBorder(BorderStyle.Single, Color.Red, 1)}},
                    lineBreakElement.Clone(),
                    New Run(document, "Double strikethrough text") With {.CharacterFormat = New CharacterFormat With {.DoubleStrikethrough = True}},
                    lineBreakElement.Clone(),
                    New Run(document, "Blue text") With {.CharacterFormat = New CharacterFormat With {.FontColor = Color.Blue}},
                    lineBreakElement.Clone(),
                    New Run(document, "Text with 'Consolas' font") With {.CharacterFormat = New CharacterFormat With {.FontName = "Consolas"}},
                    lineBreakElement.Clone(),
                    New Run(document, "Hidden text") With {.CharacterFormat = New CharacterFormat With {.Hidden = True}},
                    lineBreakElement.Clone(),
                    New Run(document, "Text with highlight color") With {.CharacterFormat = New CharacterFormat With {.HighlightColor = Color.Yellow}},
                    lineBreakElement.Clone(),
                    New Run(document, "Italic text") With {.CharacterFormat = New CharacterFormat With {.Italic = True}},
                    lineBreakElement.Clone(),
                    New Run(document, "Kerning is 15 points") With {.CharacterFormat = New CharacterFormat With {.Kerning = 15}},
                    lineBreakElement.Clone(),
                    New Run(document, "Position is 3 points") With {.CharacterFormat = New CharacterFormat With {.Position = 3}},
                    lineBreakElement.Clone(),
                    New Run(document, "Scale is 125%") With {.CharacterFormat = New CharacterFormat With {.Scaling = 125}},
                    lineBreakElement.Clone(),
                    New Run(document, "Font size is 24 points") With {.CharacterFormat = New CharacterFormat With {.Size = 24}},
                    lineBreakElement.Clone(),
                    New Run(document, "Small caps") With {.CharacterFormat = New CharacterFormat With {.SmallCaps = True}},
                    lineBreakElement.Clone(),
                    New Run(document, "Spacing is 3 point") With {.CharacterFormat = New CharacterFormat With {.Spacing = 3}},
                    lineBreakElement.Clone(),
                    New Run(document, "Strikethrough text") With {.CharacterFormat = New CharacterFormat With {.Strikethrough = True}},
                    lineBreakElement.Clone(),
                    New Run(document, "Subscript text") With {.CharacterFormat = New CharacterFormat With {.Subscript = True}},
                    lineBreakElement.Clone(),
                    New Run(document, "Superscript text") With {.CharacterFormat = New CharacterFormat With {.Superscript = True}},
                    lineBreakElement.Clone(),
                    New Run(document, "Underline color is orange") With {.CharacterFormat = New CharacterFormat With {.UnderlineColor = Color.Orange, .UnderlineStyle = UnderlineType.Single}},
                    lineBreakElement.Clone(),
                    New Run(document, "Underline style is double") With {.CharacterFormat = New CharacterFormat With {.UnderlineStyle = UnderlineType.Double}},
                    lineBreakElement.Clone(),
                    New Field(document, FieldType.Date, "\@ ""dddd, d. MMMM yyyy""") With {.CharacterFormat = New CharacterFormat With {.Language = CultureInfo.GetCultureInfo("de-DE")}})))

        document.Save(filePath)

    End Sub

    Sub Example_FormatParagraph(filePath As String)
        document.Sections.Add(
            New Section(document,
                New Paragraph(document, "This paragraph has centered text.") With
                {
                    .ParagraphFormat = New ParagraphFormat() With
                    {
                        .Alignment = HorizontalAlignment.Center
                    }
                },
                New Paragraph(document, "This paragraph has the following properties:" & vbLf & "Left indentation is 10 points." & vbLf & "Right indentation is 20 points." & vbLf & "Hanging indentation is 30 points.") With
                {
                    .ParagraphFormat = New ParagraphFormat() With
                    {
                        .LeftIndentation = 10,
                        .RightIndentation = 20,
                        .SpecialIndentation = 30
                    }
                },
                New Paragraph(document, "This paragraph has the following properties:" & vbLf & "First line indentation is 40 points." & vbLf & "Line spacing is exactly 30 points." & vbLf & "Space after and before are 10 points.") With
                {
                    .ParagraphFormat = New ParagraphFormat() With
                    {
                        .SpecialIndentation = -40,
                        .LineSpacing = 30,
                        .LineSpacingRule = LineSpacingRule.Exactly,
                        .SpaceAfter = 10,
                        .SpaceBefore = 10
                    }
                }))

        Dim paragraph As New Paragraph(document, "This paragraph has borders and background color.")
        paragraph.ParagraphFormat.Borders.SetBorders(MultipleBorderTypes.Outside, BorderStyle.Single, New Color(237, 125, 49), 2)
        paragraph.ParagraphFormat.BackgroundColor = New Color(251, 228, 213)
        document.Sections(0).Blocks.Add(paragraph)

        document.Save(filePath)

    End Sub

    Sub Example_CreateCustomizedList(filePath As String)

        Dim listItemsCount As Integer = 12

        Dim section As New Section(document)
        document.Sections.Add(section)

        ' Create number list style.
        Dim numberList As New ListStyle(ListTemplateType.NumberWithDot)

        ' Customize list level formats.
        For level = 0 To numberList.ListLevelFormats.Count - 1

            Dim levelFormat As ListLevelFormat = numberList.ListLevelFormats(level)

            levelFormat.ParagraphFormat.NoSpaceBetweenParagraphsOfSameStyle = True
            levelFormat.Alignment = HorizontalAlignment.Left
            levelFormat.NumberStyle = NumberStyle.Decimal

            levelFormat.NumberPosition = 18 * level
            levelFormat.NumberFormat = String.Concat(Enumerable.Range(1, level + 1).Select(Function(i) $"%{i}."))

        Next

        ' Create number list items.
        For i = 0 To listItemsCount - 1

            Dim paragraph As New Paragraph(document, "Lorem ipsum")

            paragraph.ListFormat.Style = numberList
            paragraph.ListFormat.ListLevelNumber = i Mod 9

            section.Blocks.Add(paragraph)

        Next

        document.Save(filePath)

    End Sub

    Sub Example_CreateNumberedList(filePath As String)

        Dim section As New Section(document)
        document.Sections.Add(section)

        Dim blocks = section.Blocks

        ' Create bullet list style.
        Dim bulletList As New ListStyle(ListTemplateType.Bullet)
        bulletList.ListLevelFormats(0).ParagraphFormat.NoSpaceBetweenParagraphsOfSameStyle = True
        bulletList.ListLevelFormats(0).CharacterFormat.FontColor = Color.Red

        ' Create bullet list items.
        blocks.Add(New Paragraph(document, "First item.") With
        {
            .ListFormat = New ListFormat() With {.Style = bulletList}
        })
        blocks.Add(New Paragraph(document, "Second item.") With
        {
            .ListFormat = New ListFormat() With {.Style = bulletList}
        })
        blocks.Add(New Paragraph(document, "Third item.") With
        {
            .ListFormat = New ListFormat() With {.Style = bulletList}
        })

        blocks.Add(New Paragraph(document))

        ' Create number list style.
        Dim numberList As New ListStyle(ListTemplateType.NumberWithDot)

        ' Create number list items.
        blocks.Add(New Paragraph(document, "First item.") With
        {
            .ListFormat = New ListFormat() With {.Style = numberList}
        })
        blocks.Add(New Paragraph(document, "Sub item 1. a.") With
        {
            .ListFormat = New ListFormat() With {.Style = numberList, .ListLevelNumber = 1}
        })
        blocks.Add(New Paragraph(document, "Item below sub item 1. a.") With
        {
            .ListFormat = New ListFormat() With {.Style = numberList, .ListLevelNumber = 2}
        })
        blocks.Add(New Paragraph(document, "Sub item 1. b.") With
        {
            .ListFormat = New ListFormat() With {.Style = numberList, .ListLevelNumber = 1}
        })
        blocks.Add(New Paragraph(document, "Second item.") With
        {
            .ListFormat = New ListFormat() With {.Style = numberList}
        })

        document.Save(filePath)
    End Sub

    Sub Example_CreateParagraphs(filePath As String)
        'source -- https://www.gemboxsoftware.com/document/examples/c-sharp-vb-net-write-word-file/302

        Dim text1 As String = Infodoc2Text(App.ActiveProject, PM.Hierarchies(0).Nodes(0).InfoDoc)

        ' Add new section with two paragraphs, containing some text and symbols.
        document.Sections.Add(
            New Section(document,
                New Paragraph(document,
                    New Run(document, "This is our first paragraph with symbols added on a new line."),
                    New SpecialCharacter(document, SpecialCharacterType.LineBreak),
                    New Run(document, ChrW(&HFC) & ChrW(&HF0) & ChrW(&H32)) With {.CharacterFormat = New CharacterFormat() With {.FontName = "Wingdings", .Size = 48}}),
                New Paragraph(document, text1),
                New Paragraph(document, "This is our third paragraph.")))

        ' Save Word document to file's path.
        document.Save(filePath)

    End Sub

    Sub HelloWorld(filePath As String)

        Dim document As New DocumentModel()

        Dim section As New Section(document)
        document.Sections.Add(section)

        Dim paragraph As New Paragraph(document)
        section.Blocks.Add(paragraph)

        Dim run As New Run(document, "Hello World!")
        paragraph.Inlines.Add(run)

        document.Save("D:\Temp\Hello World.docx")
    End Sub

    Public Sub New(_App As clsComparionCore)
        App = _App
        PM = App.ActiveProject.ProjectManager

        ' If using Professional version, put your serial key below.
        ComponentInfo.SetLicense("DN-2019Oct23-o9cPV8FqJpXV/58cTjlRakTEbT0YmbK2qfwJS4OkrVTGqcdjqxHlgRDfWwwv2q3j9qruUP258vApmepOW0Z46sRTc6Q==A")

        ' Create new empty document.
        document = New DocumentModel()

    End Sub

    Private Class wordTextbox
        Public tboxLeft As Double
        Public tboxTop As Double
        Public tboxbW As Double
        Public tboxH As Double
        Public nodeID As Integer

        Public Sub New(L As Double, T As Double, W As Double, H As Double, nID As Integer)
            tboxLeft = L
            tboxTop = T
            tboxbW = W
            tboxH = H
            nodeID = nID
        End Sub
    End Class


End Class


