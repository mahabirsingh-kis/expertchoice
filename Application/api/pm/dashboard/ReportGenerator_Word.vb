Imports GemBox.Document
Imports System.Globalization
Imports GemBox.Document.Drawing
Imports GemBox.Document.Tables
Imports System.IO

<Serializable> Public Class ReportGenerator_Word 'AS/17533p added the class
    Private App As clsComparionCore
    Private PM As clsProjectManager
    Private docxReport As DocumentModel

    Private Textboxes As List(Of wordTextbox)
    Private Const NODE_PATH_DELIMITER As String = " | " '  & vbCrLf 'AS/18712q
    Private TEMP_SINGLE_VALUE As Single = 0.123456789 'AS/18712q
    Private TEMP_STRING_VALUE As String = "temp string value" 'AS/18712q

    Private colObjectivesWidth As Double 'AS/18712t -- legnth of the most long objective name
    Private colAlternativesWidth As Double 'AS/18712t -- legnth of the most long alternative name

    Function GenerateWordReport(filePath As String, server As HttpServerUtility, Options As ReportGeneratorOptions) As DocumentModel    ' D6455
        ' D6455 ===
        If Options Is Nothing Then
            Options = New ReportGeneratorOptions
            Options.ReportTitle = PM.ProjectName
        End If

        docxReport.DefaultCharacterFormat.FontName = Options.ReportFontName
        docxReport.DefaultCharacterFormat.Size = Options.ReportFontSize
        ' D6455 ==

        'calc width for columns ===== 'AS/18712t===
        Dim objH As clsHierarchy = App.ActiveProject.HierarchyObjectives
        Dim colH As Double
        Dim longestName As String = ""
        For Each node As clsNode In objH.Nodes
            If Len(longestName) < Len(node.NodeName) Then longestName = node.NodeName
        Next
        calcTextboxSize(longestName, colObjectivesWidth, colH, Options.ReportFontSize)

        Dim altH As clsHierarchy = PM.AltsHierarchy(PM.ActiveAltsHierarchy)
        For Each alt As clsNode In objH.Nodes
            If Len(longestName) < Len(alt.NodeName) Then longestName = alt.NodeName
        Next
        calcTextboxSize(longestName, colAlternativesWidth, colH, Options.ReportFontSize) 'AS/18712t==

        Dim docsection As New Section(docxReport)

        'create and insert the first section: model title and description 'AS/18226b
        docsection = GetReportTitleSection(Options)
        If Not IsNothing(docsection) Then docxReport.Sections.Add(docsection)

        'create and insert section with infodoc
        If Options.IncludeGoalDescription Then 'AS/18226a enclosed
            docsection = GetInfodocSection(Options, "Goal")
            If Not IsNothing(docsection) Then docxReport.Sections.Add(docsection)
        End If 'AS/18226


        'create and insert section with hierarchy - IMAGE 'AS/18226b
        If Options.IncludeHierarchyImage Then
            docsection = GetPictureSection(server, Options, "Hierarchy")
            If Not IsNothing(docsection) Then docxReport.Sections.Add(docsection)
        End If

        'create and insert section with hierarchy - EDITABLE
        If Options.IncludeHierarchyEditable Then 'AS/18226a enclosed
            Dim TVsections As List(Of Section) 'AS/17533q=== LATER will replace with picture
            'Dim bIncludeInfodocs As Boolean = ReportInclude("NodesInfodocs", Options)
            TVsections = GetHierarchySections(Options)
            For Each TVsection As Section In TVsections
                If Not IsNothing(TVsection) Then docxReport.Sections.Add(TVsection)
            Next 'AS/17533q==
        End If

        'create and insert section with Datagrid
        If Options.IncludeDataGrid Then 'AS/18226a enclosed
            Dim UserIDs As New List(Of Integer) 'AS/18712b===
            UserIDs.Add(-1) 'AS/18712c===
            UserIDs.Add(-1000)
            UserIDs.Add(2) 'AS/18712c===
            docsection = GetDatagridSection(Options, UserIDs)    ' D6455 + 'AS/18712c

            If Not IsNothing(docsection) Then docxReport.Sections.Add(docsection)
        End If

        If Options.IncludeObjectives Then 'AS/18226a===
            docsection = GetObjectivesSection(Options)
            If Not IsNothing(docsection) Then docxReport.Sections.Add(docsection)
        End If

        If Options.IncludeAlternatives Then 'AS/18226a enclosed
            docsection = GetAlternativesSection(Options)
            If Not IsNothing(docsection) Then docxReport.Sections.Add(docsection)
        End If

        If Options.IncludeSurveyResults Then
            docsection = GetSurveySection(Options)
            If Not IsNothing(docsection) Then docxReport.Sections.Add(docsection)
        End If

        If Options.IncludeInconsistency Then 'AS/18226a enclosed
            docsection = GetInconsistencySection(Options)
            If Not IsNothing(docsection) Then docxReport.Sections.Add(docsection)
        End If

        If Options.IncludeOverallResults Then 'AS/18712r
            docsection = GetOverallResultsSection(Options)
            If Not IsNothing(docsection) Then docxReport.Sections.Add(docsection)
        End If

        If Options.IncludeObjectivesAlternativesPriorities Then 'AS/18712s
            docsection = GetObjAltsPrioritiesSection(Options)
            If Not IsNothing(docsection) Then docxReport.Sections.Add(docsection)
        End If

        If Options.IncludeObjectivesPriorities Then 'AS/18226a enclosed
            docsection = GetObjectivesPrioritiesSection(Options)
            If Not IsNothing(docsection) Then docxReport.Sections.Add(docsection)
        End If

        If Options.IncludeAlternativesPriorities Then 'AS/18226a enclosed
            docsection = GetAlternativesPrioritiesSection(Options)
            If Not IsNothing(docsection) Then docxReport.Sections.Add(docsection)
        End If

        If Options.IncludeEvaluationProgress Then 'AS/18226a enclosed
            docsection = GetEvaluationProgressSection(Options)
            If Not IsNothing(docsection) Then docxReport.Sections.Add(docsection)
        End If



        If Options.IncludeJudgmentsOfAlternatives Then 'AS/18226a enclosed
            docsection = GetJudgmentsOfAltsSection(Options)
            If Not IsNothing(docsection) Then docxReport.Sections.Add(docsection)
        End If

        'If ReportInclude("JudgmentsOverview", Options) Then
        If Options.IncludeJudgmentsOverview Then 'AS/18226a enclosed
            docsection = GetJudgmentsOverviewSection(Options)
            If Not IsNothing(docsection) Then docxReport.Sections.Add(docsection)
        End If

        If Options.IncludeObjectivesAlternativesPriorities Then 'AS/18226a enclosed
            docsection = GetObjAltsPrioritiesSection(Options)
            If Not IsNothing(docsection) Then docxReport.Sections.Add(docsection)
        End If

        If Options.IncludeJudgmentsOfObjectives Then
            'docsection = GetJudgmentsOfObjectivesSection(Options)
            docsection = GetPictureSection(server, Options, "JudgmentsOfObjectives")
            If Not IsNothing(docsection) Then docxReport.Sections.Add(docsection)
        End If

        If Options.IncludeJudgmentsOfAlternatives Then
            docsection = GetJudgmentsOfAlternativesSection(Options)
            If Not IsNothing(docsection) Then docxReport.Sections.Add(docsection)
        End If

        If Options.IncludeContributions Then
            docsection = GetContributionsSection(Options)
            If Not IsNothing(docsection) Then docxReport.Sections.Add(docsection)
        End If 'AS/18226a==

        'create and insert section with picture (chart)
        If Options.IncludeAlternativesChart Then
            docsection = GetPictureSection(server, Options, "Chart")
            docxReport.Sections.Add(docsection)
        End If

        docxReport.Save(filePath)
        Return docxReport

    End Function

#Region "GetSections"
    Private Function GetReportTitleSection(Options As ReportGeneratorOptions) As Section 'AS/18226b
        Dim secTitle As New Section(docxReport,
                New Paragraph(docxReport,
                    New Run(docxReport, "Model Name:  " & PM.ProjectName) With {.CharacterFormat = New CharacterFormat With {.Bold = True, .Size = Options.ReportTitleFontSize}}) With
                {
                    .ParagraphFormat = New ParagraphFormat() With
                    {
                        .Alignment = HorizontalAlignment.Center
                    }
                })

        If Options.IncludeModelDescription Then
            Dim para As New Paragraph(docxReport,
                 New Run(docxReport, "Model Description: ") With {.CharacterFormat = New CharacterFormat With {.Bold = True, .Size = Options.ReportFontSize}}) 'AS/18712t

            secTitle.Blocks.Add(para)

            para = New Paragraph(docxReport,
                 New Run(docxReport, Infodoc2Text(App.ActiveProject, PM.ProjectDescription)) With {.CharacterFormat = New CharacterFormat With {.Bold = False, .Size = Options.ReportFontSize}}) With
            {
                .ParagraphFormat = New ParagraphFormat() With
                {
                    .LeftIndentation = Options.DocLeftIndent,
                    .RightIndentation = Options.DocRightIndent,
                    .SpaceAfter = Options.DocSpaceAfter,
                    .SpaceBefore = Options.DocSpaceBefore
                }
            }

            secTitle.Blocks.Add(para)
        End If

        Return secTitle
    End Function

    Private Function GetInfodocSection(Options As ReportGeneratorOptions, infodoc As String) As Section 'AS/18226a added infodoc

        Dim secInfodoc As New Section(docxReport,
                New Paragraph(docxReport,
                    New Run(docxReport, "Goal Description") With {.CharacterFormat = New CharacterFormat With {.Bold = True, .Size = Options.InfodocFontSize}}) With
                {
                    .ParagraphFormat = New ParagraphFormat() With
                    {
                        .Alignment = HorizontalAlignment.Left
                    }
                },
                New Paragraph(docxReport, PM.Hierarchies(0).Nodes(0).NodeName) With
                {
                    .ParagraphFormat = New ParagraphFormat() With
                    {
                        .LeftIndentation = Options.DocLeftIndent,
                        .RightIndentation = Options.DocRightIndent,
                        .SpaceAfter = 12,'Options.DocSpaceAfter,
                        .SpaceBefore = Options.DocSpaceBefore
                    }
                },
                New Paragraph(docxReport, Infodoc2Text(App.ActiveProject, PM.Hierarchies(0).Nodes(0).InfoDoc)) With
                {
                    .ParagraphFormat = New ParagraphFormat() With
                    {
                        .LeftIndentation = Options.DocLeftIndent,
                        .RightIndentation = Options.DocRightIndent,
                        .SpaceAfter = Options.DocSpaceAfter,
                        .SpaceBefore = Options.DocSpaceBefore
                    }
                })

        Return secInfodoc

    End Function

    Private Function GetAlternativesSection(Options As ReportGeneratorOptions) As Section 'AS/1822d
        Dim secAlts As New Section(docxReport,
                New Paragraph(docxReport,
                    New Run(docxReport, "Alternatives") With {.CharacterFormat = New CharacterFormat With {.Bold = True, .Size = Options.ReportTitleFontSize}}) With
                {
                    .ParagraphFormat = New ParagraphFormat() With
                    {
                        .Alignment = HorizontalAlignment.Center
                    }
                })

        ' Create plain list
        Dim H As clsHierarchy = App.ActiveProject.HierarchyAlternatives
        For i = 0 To H.Nodes.Count - 1
            Dim N As clsNode = H.Nodes(i)
            Dim para As New Paragraph(docxReport,
                    New Run(docxReport, N.NodeName) With {.CharacterFormat = New CharacterFormat With {.Bold = True, .Size = Options.ReportFontSize}}) With
                {
                    .ParagraphFormat = New ParagraphFormat() With
                    {
                        .SpaceAfter = Options.DocSpaceAfter '= 0
                    }
                }

            secAlts.Blocks.Add(para)
        Next
        Return secAlts
    End Function

    Private Function GetAlternativesPrioritiesSection(Options As ReportGeneratorOptions) As Section 'AS/18226
        'TBD
    End Function

    Private Function GetEvaluationProgressSection(Options As ReportGeneratorOptions) As Section 'AS/18226
        'TBD
    End Function

    Private Function GetInconsistencySection(Options As ReportGeneratorOptions) As Section 'AS/18712q
        Dim objH As clsHierarchy = App.ActiveProject.HierarchyObjectives

        'add new section to the doc 
        Dim secIncon As New Section(docxReport)
        Dim sectionName As String = "Inconsistency"

        'add table
        Dim table As New Table(docxReport)
        table.TableFormat.AutomaticallyResizeToFitContents = False

        Dim maxNodeName As String = ""
        Dim maxUserName As String = ""
        Dim maxEmail As String = ""
        Dim colUserNameWidth As Double = 0
        Dim colEmailWidth As Double = 0
        Dim colNodeNameWidth As Double = 0
        Dim colPathWidth As Double = 0
        Dim colInconsistencyWidth As Double = 0
        Dim colNumOfChildrenWidth As Double = 0

        Dim row As New TableRow(docxReport)

        'write section name
        Dim Paragraph = New Paragraph(docxReport,
                        New Run(docxReport, sectionName) With {.CharacterFormat = New CharacterFormat With {.Bold = True, .Size = 16}}) With
                    {
                        .ParagraphFormat = New ParagraphFormat() With
                        {
                            .Alignment = HorizontalAlignment.Center,
                            .SpaceAfter = 20
                        }
                    }

        secIncon.Blocks.Add(Paragraph)

        'calc width for columns =====
        Dim colH As Double
        For Each node As clsNode In objH.Nodes
            If Len(maxNodeName) < Len(node.NodeName) Then maxNodeName = node.NodeName
        Next
        calcTextboxSize(maxNodeName, colNodeNameWidth, colH, Options.ReportFontSize)
        calcTextboxSize(maxNodeName & "WWWW", colPathWidth, colH, Options.ReportFontSize)

        For Each u As clsUser In PM.UsersList
            If Len(maxUserName) < Len(u.UserName) Then maxUserName = u.UserName
            If Len(maxEmail) < Len(u.UserEMail) Then maxEmail = u.UserEMail
        Next
        calcTextboxSize(maxUserName, colUserNameWidth, colH, Options.ReportFontSize)
        calcTextboxSize(maxEmail, colEmailWidth, colH, Options.ReportFontSize)

        calcTextboxSize("Inconsistency", colInconsistencyWidth, colH, Options.ReportFontSize)
        calcTextboxSize("Number of Children", colNumOfChildrenWidth, colH, Options.ReportFontSize)

        Dim pageSetup = secIncon.PageSetup
        pageSetup.Orientation = Orientation.Landscape
        'pageSetup.PaperType = PaperType.A3
        pageSetup.PageMargins.Left = 20
        pageSetup.PageMargins.Right = 20
        Dim pageWidth As Double = pageSetup.PageWidth - pageSetup.PageMargins.Left - pageSetup.PageMargins.Right
        Dim r As Double = pageWidth / (colUserNameWidth + colEmailWidth + colNodeNameWidth + colPathWidth + colInconsistencyWidth + colNumOfChildrenWidth)

        colUserNameWidth = colUserNameWidth * r
        colEmailWidth = colEmailWidth * r
        colNodeNameWidth = colNodeNameWidth * r
        colPathWidth = colPathWidth * r
        colInconsistencyWidth = colInconsistencyWidth * r
        colNumOfChildrenWidth = colNumOfChildrenWidth * r
        '=====


        'Create row with columns headers
        row = New TableRow(docxReport)
        table.Rows.Add(row)
        Dim para As New Paragraph(docxReport, New Run(docxReport, "Name") With {.CharacterFormat = New CharacterFormat With {.Bold = True}}) With {
                        .ParagraphFormat = New ParagraphFormat() With {
                            .Alignment = HorizontalAlignment.Center}}
        row.Cells.Add(New TableCell(docxReport, para) With {.RowSpan = 2, .CellFormat = New TableCellFormat() With {
                    .VerticalAlignment = VerticalAlignment.Center,
                    .BackgroundColor = Color.LightGray,
                    .PreferredWidth = New TableWidth(colUserNameWidth, TableWidthUnit.Point)
             }})


        para = New Paragraph(docxReport, New Run(docxReport, "Examine") With {.CharacterFormat = New CharacterFormat With {.Bold = True}})
        row.Cells.Add(New TableCell(docxReport, para) With {.RowSpan = 2, .CellFormat = New TableCellFormat() With {
                    .VerticalAlignment = VerticalAlignment.Center,
                    .BackgroundColor = Color.LightGray,
                    .PreferredWidth = New TableWidth(colEmailWidth, TableWidthUnit.Point)
             }})

        para = New Paragraph(docxReport, New Run(docxReport, "Objective") With {.CharacterFormat = New CharacterFormat With {.Bold = True}})
        row.Cells.Add(New TableCell(docxReport, para) With {.RowSpan = 2, .CellFormat = New TableCellFormat() With {
                    .VerticalAlignment = VerticalAlignment.Center,
                    .BackgroundColor = Color.LightGray,
                    .PreferredWidth = New TableWidth(colNodeNameWidth, TableWidthUnit.Point)
             }})

        para = New Paragraph(docxReport, New Run(docxReport, "Path") With {.CharacterFormat = New CharacterFormat With {.Bold = True}})
        row.Cells.Add(New TableCell(docxReport, para) With {.RowSpan = 2, .CellFormat = New TableCellFormat() With {
                    .VerticalAlignment = VerticalAlignment.Center,
                    .BackgroundColor = Color.LightGray,
                    .PreferredWidth = New TableWidth(colPathWidth, TableWidthUnit.Point)
             }})

        para = New Paragraph(docxReport, New Run(docxReport, "Inconsistency") With {.CharacterFormat = New CharacterFormat With {.Bold = True}})
        row.Cells.Add(New TableCell(docxReport, para) With {.RowSpan = 2, .CellFormat = New TableCellFormat() With {
                    .VerticalAlignment = VerticalAlignment.Center,
                    .BackgroundColor = Color.LightGray,
                    .PreferredWidth = New TableWidth(colInconsistencyWidth, TableWidthUnit.Point)
             }})

        para = New Paragraph(docxReport, New Run(docxReport, "Number Of Children") With {.CharacterFormat = New CharacterFormat With {.Bold = True}})
        row.Cells.Add(New TableCell(docxReport, para) With {.RowSpan = 2, .CellFormat = New TableCellFormat() With {
                    .VerticalAlignment = VerticalAlignment.Center,
                    .BackgroundColor = Color.LightGray,
                    .PreferredWidth = New TableWidth(colNumOfChildrenWidth, TableWidthUnit.Point)
             }})


        row = New TableRow(docxReport)
        table.Rows.Add(row)

        Dim user As clsUser = Nothing
        Dim Group As clsCombinedGroup = PM.CombinedGroups.GetCombinedGroupByUserID(-1)
        Dim UserIDs As ArrayList = GetUserIDsList(user, Group)
        Dim NodePath As String = ""

        'create rows for participants and add cells with data
        For Each UserID As Integer In UserIDs
            user = PM.GetUserByID(UserID)
            PM.StorageManager.Reader.LoadUserData(user)

            'row = New TableRow(docxReport)
            'table.Rows.Add(row)

            For Each node As clsNode In objH.Nodes
                If node.MeasureType = ECMeasureType.mtPairwise AndAlso node.IsAllowed(user.UserID) Then
                    If CType(node.Judgments, clsPairwiseJudgments).HasSpanningSet(user.UserID) Then
                        node.CalculateLocal(user.UserID)
                        row = New TableRow(docxReport)
                        table.Rows.Add(row)

                        para = New Paragraph(docxReport, user.UserName)
                        row.Cells.Add(New TableCell(docxReport, para))

                        para = New Paragraph(docxReport, user.UserEMail)
                        row.Cells.Add(New TableCell(docxReport, para))

                        para = New Paragraph(docxReport, node.NodeName)
                        row.Cells.Add(New TableCell(docxReport, para))

                        NodePath = ""
                        GetFullNodePath(node, NodePath)
                        para = New Paragraph(docxReport, NodePath)
                        row.Cells.Add(New TableCell(docxReport, para))

                        Dim incon As Double = Math.Round(CType(node.Judgments, clsPairwiseJudgments).EigenCalcs.InconIndex, Options.DatagridNumberOfDecimals)
                        para = New Paragraph(docxReport, incon.ToString)
                        row.Cells.Add(New TableCell(docxReport, para))

                        para = New Paragraph(docxReport, node.GetNodesBelow(user.UserID).Count.ToString)
                        row.Cells.Add(New TableCell(docxReport, para))

                    End If
                End If

            Next
        Next


        secIncon.Blocks.Add(table)
        secIncon.Blocks.Add(New Paragraph(docxReport, New SpecialCharacter(docxReport, SpecialCharacterType.PageBreak)))

        'Dim pageSetup = secIncon.PageSetup
        'pageSetup.Orientation = Orientation.Landscape
        ''pageSetup.PaperType = PaperType.A3
        'pageSetup.PageMargins.Left = 20
        'pageSetup.PageMargins.Right = 20

        Return secIncon
    End Function

    Private Function GetOverallResultsSection(Options As ReportGeneratorOptions) As Section 'AS/18712r
        Dim docSection As New Section(docxReport)

        'calc priorities for Goal and userID=-1 (Combined)
        Dim wrtNode As clsNode = PM.ActiveObjectives.GetNodeByID(1)
        Dim CalcTarget As clsCalculationTarget
        PM.StorageManager.Reader.LoadUserData(PM.GetUserByID(-1))
        CalcTarget = New clsCalculationTarget(CalculationTargetType.cttCombinedGroup, PM.CombinedGroups.GetCombinedGroupByUserID(-1))
        PM.CalculationsManager.Calculate(CalcTarget, wrtNode, PM.ActiveHierarchy, PM.ActiveAltsHierarchy)

        Dim table As New Table(docxReport)
        table.TableFormat.AutomaticallyResizeToFitContents = False

        Dim row As New TableRow(docxReport)

        Dim altH As clsHierarchy = App.ActiveProject.HierarchyAlternatives
        Dim rowCount As Integer = altH.Nodes.Count

        Dim colAltWidth As Integer = 350
        Dim colW As Double, colH As Double
        calcTextboxSize("All Participants", colW, colH, Options.ReportFontSize)
        Dim colPrtyWidth As Integer = CInt(colW)

        Dim Paragraph = New Paragraph(docxReport,
                    New Run(docxReport, "Overall Results") With {.CharacterFormat = New CharacterFormat With {.Bold = True, .Size = 16}}) With
                {
                    .ParagraphFormat = New ParagraphFormat() With
                    {
                        .Alignment = HorizontalAlignment.Center,
                        .SpaceAfter = 20
                    }
                }

        docSection.Blocks.Add(Paragraph)

        'Create row for columns headers
        row = New TableRow(docxReport)
        table.Rows.Add(row)
        'create cell for "Alternatives"
        Dim para As New Paragraph(docxReport, New Run(docxReport, "Alternatives") With {.CharacterFormat = New CharacterFormat With {.Bold = True}}) With {
                    .ParagraphFormat = New ParagraphFormat() With {
                        .Alignment = HorizontalAlignment.Left}}
        row.Cells.Add(New TableCell(docxReport, para) With {.CellFormat = New TableCellFormat() With {
                .VerticalAlignment = VerticalAlignment.Center,
                .PreferredWidth = New TableWidth(colAltWidth, TableWidthUnit.Point)
         }})

        'create cell for "All Participants"
        para = New Paragraph(docxReport, New Run(docxReport, "All Participants") With {.CharacterFormat = New CharacterFormat With {.Bold = True}})
        row.Cells.Add(New TableCell(docxReport, para) With {.CellFormat = New TableCellFormat() With {
                .VerticalAlignment = VerticalAlignment.Center,
                .PreferredWidth = New TableWidth(colPrtyWidth, TableWidthUnit.Point)
         }})

        'create rows for alternatives
        For i As Integer = 0 To altH.Nodes.Count - 1
            'add alts names to the first col
            Dim alt = altH.Nodes(i)
            row = New TableRow(docxReport)
            table.Rows.Add(row)
            para = New Paragraph(docxReport, alt.NodeName)
            row.Cells.Add(New TableCell(docxReport, para))

            'para = New Paragraph(docxReport, (Math.Round(alt.WRTGlobalPriority, Options.DatagridNumberOfDecimals)).ToString("#0.##%"))
            para = New Paragraph(docxReport, alt.WRTGlobalPriority.ToString("#0.##%"))
            para.ParagraphFormat.Alignment = HorizontalAlignment.Right
            row.Cells.Add(New TableCell(docxReport, para))

        Next i

        docSection.Blocks.Add(table)
        Return docSection

    End Function

    Private Function GetObjAltsPrioritiesSection(Options As ReportGeneratorOptions) As Section 'AS/18712s

        Dim docSection As New Section(docxReport)

        'calc priorities for Goal and userID=-1 (Combined)
        Dim UserID As Integer = -1 'All Participants
        Dim wrtNode As clsNode = PM.ActiveObjectives.GetNodeByID(1)
        PM.StorageManager.Reader.LoadUserData(PM.GetUserByID(UserID))
        Dim CalcTarget As clsCalculationTarget = PM.CalculationsManager.GetCalculationTargetByUserID(UserID)
        PM.CalculationsManager.Calculate(CalcTarget, wrtNode, PM.ActiveHierarchy, PM.ActiveAltsHierarchy)

        'Get lists of alternatives and objectives
        Dim objH As clsHierarchy = App.ActiveProject.HierarchyObjectives
        Dim altH As clsHierarchy = PM.AltsHierarchy(PM.ActiveAltsHierarchy)

        Dim table As New Table(docxReport)
        table.TableFormat.AutomaticallyResizeToFitContents = False

        'calc cols widths
        Dim colW As Double, colH As Double
        calcTextboxSize("All ParticipantsW", colW, colH, Options.ReportFontSize)
        Dim colPrtyWidth As Double = CInt(colW)
        Dim pW As Double = docSection.PageSetup.PageWidth - docSection.PageSetup.PageMargins.Left - docSection.PageSetup.PageMargins.Right
        Dim colAltWidth As Double = pW - colPrtyWidth

        'section title
        Dim Paragraph = New Paragraph(docxReport,
                    New Run(docxReport, "Objectives/Alternatives Priorities") With {.CharacterFormat = New CharacterFormat With {.Bold = True, .Size = 16}}) With
                {
                    .ParagraphFormat = New ParagraphFormat() With
                    {
                        .Alignment = HorizontalAlignment.Center,
                        .SpaceAfter = 20
                    }
                }

        docSection.Blocks.Add(Paragraph)

        Dim row As New TableRow(docxReport)
        Dim r As Integer = 0 'row number to shade alternate rows 

        'Create row for columns headers
        row = New TableRow(docxReport)
        table.Rows.Add(row)

        'create cell for objective/alternatives names
        Dim para As New Paragraph(docxReport, New Run(docxReport, "Objective/Alternative") With {.CharacterFormat = New CharacterFormat With {.Bold = True, .FontColor = Color.White}}) With {
                    .ParagraphFormat = New ParagraphFormat() With {
                        .Alignment = HorizontalAlignment.Center}}
        row.Cells.Add(New TableCell(docxReport, para) With {.CellFormat = New TableCellFormat() With {
                .VerticalAlignment = VerticalAlignment.Center,
                .PreferredWidth = New TableWidth(colAltWidth, TableWidthUnit.Point),
                .BackgroundColor = Color.DarkGreen
         }})

        'create cell for priority
        Dim colPrioritiesWidth As Double = 0
        calcTextboxSize("Alternative Priority ", colPrioritiesWidth, colH, Options.ReportFontSize)
        para = New Paragraph(docxReport, New Run(docxReport, "Alternative Priority") With
              {.CharacterFormat = New CharacterFormat With {.Bold = True, .FontColor = Color.White}}) With {
                    .ParagraphFormat = New ParagraphFormat() With {.Alignment = HorizontalAlignment.Center}}

        row.Cells.Add(New TableCell(docxReport, para) With {.CellFormat = New TableCellFormat() With {
                .VerticalAlignment = VerticalAlignment.Center,
                .PreferredWidth = New TableWidth(colPrtyWidth, TableWidthUnit.Point),
                .BackgroundColor = Color.DarkGreen
         }})

        Dim rColor As Color

        For Each node As clsNode In objH.Nodes
            row = New TableRow(docxReport)
            r = r + 1

            ' Set cell's background
            If r Mod 2 = 0 Then
                rColor = Color.White
            Else
                rColor = Color.LightGray
            End If
            table.Rows.Add(row)

            para = New Paragraph(docxReport, node.NodeName & "[L:" & Strings.Format(TEMP_SINGLE_VALUE, "0.00%") & "] [G:" & Strings.Format(node.SAGlobalPriority, "0.00%") & "]")
            If node.Level > 0 Then
                para.ListFormat.Style = New ListStyle(ListTemplateType.None)
                para.ListFormat.ListLevelNumber = node.Level - 1
            End If
            row.Cells.Add(New TableCell(docxReport, para) With {.ColumnSpan = 2, .CellFormat = New TableCellFormat() With {
                .BackgroundColor = rColor}})

            'para = New Paragraph(docxReport, "")
            'row.Cells.Add(New TableCell(docxReport, para))

            If node.IsTerminalNode Then 'covering objective -- create rows for alternatives
                For Each alt As clsNode In altH.Nodes

                    Dim sValue As String = ""
                    Select Case node.MeasureType
                        Case ECMeasureType.mtRatings
                            Dim currJ As clsRatingMeasureData = CType(GetJudgment(node, alt.NodeID, UserID), clsRatingMeasureData)
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
                            Dim currJ As clsNonPairwiseMeasureData = GetJudgment(node, alt.NodeID, UserID)
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
                            Dim aVal As Double = node.Judgments.Weights.GetUserWeights(UserID, ECSynthesisMode.smIdeal, PM.CalculationsManager.IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(alt.NodeID)
                            sValue = aVal.ToString
                    End Select

                    row = New TableRow(docxReport)
                    r = r + 1

                    ' Set cell's background
                    If r Mod 2 = 0 Then
                        rColor = Color.White
                    Else
                        rColor = Color.LightGray
                    End If
                    table.Rows.Add(row)

                    para = New Paragraph(docxReport, alt.NodeName)
                    para.ListFormat.Style = New ListStyle(ListTemplateType.None)
                    para.ListFormat.ListLevelNumber = node.Level

                    Dim cell As New TableCell(docxReport, para) With {.CellFormat = New TableCellFormat() With {
                        .BackgroundColor = rColor}}
                    cell.CellFormat.Borders.SetBorders(MultipleBorderTypes.None, BorderStyle.Double, Color.Red, 1)
                    row.Cells.Add(cell)

                    para = New Paragraph(docxReport, FormatPercent(CSng(sValue), 2))
                    para.ParagraphFormat.Alignment = HorizontalAlignment.Right
                    cell = New TableCell(docxReport, para) With {.CellFormat = New TableCellFormat() With {
                        .BackgroundColor = rColor}}
                    cell.CellFormat.Borders.SetBorders(MultipleBorderTypes.None, BorderStyle.Double, Color.Red, 1)
                    row.Cells.Add(cell)
                Next
            End If

        Next

        docSection.Blocks.Add(table)
        Return docSection

    End Function

    Private Function GetJudgmentsOfAltsSection(Options As ReportGeneratorOptions) As Section 'AS/18226
        'TBD
    End Function

    Private Function GetJudgmentsOverviewSection(Options As ReportGeneratorOptions) As Section 'AS/18226
        'TBD
    End Function

    Private Function GetObjectivesPrioritiesSection(Options As ReportGeneratorOptions) As Section 'AS/18226
        'TBD
    End Function

    Private Function GetObjectivesSection(Options As ReportGeneratorOptions) As Section 'AS/18226c

        Dim secObjectives As New Section(docxReport,
                New Paragraph(docxReport,
                    New Run(docxReport, "Objectives") With {.CharacterFormat = New CharacterFormat With {.Bold = True, .Size = Options.ReportTitleFontSize}}) With
                {
                    .ParagraphFormat = New ParagraphFormat() With
                    {
                        .Alignment = HorizontalAlignment.Center
                    }
                })


        ' Create number list items.
        Dim H As clsHierarchy = App.ActiveProject.HierarchyObjectives
        For i = 0 To H.Nodes.Count - 1
            Dim N As clsNode = H.Nodes(i)
            Dim para As New Paragraph(docxReport,
                    New Run(docxReport, N.NodeName) With {.CharacterFormat = New CharacterFormat With {.Bold = True, .Size = Options.ReportFontSize}}) With
                {
                    .ParagraphFormat = New ParagraphFormat() With
                    {
                        .SpaceAfter = Options.DocSpaceAfter '= 0
                    }
                }

            para.ListFormat.Style = New ListStyle(ListTemplateType.None)
            para.ListFormat.ListLevelNumber = N.Level
            secObjectives.Blocks.Add(para)

            If Options.IncludeObjectivesDescriptions Then
                Dim sText As String = Infodoc2Text(App.ActiveProject, N.InfoDoc)
                If sText <> "" Then
                    para = New Paragraph(docxReport, sText) With
                {
                    .ParagraphFormat = New ParagraphFormat() With
                    {
                        .LeftIndentation = Options.DocLeftIndent,
                        .RightIndentation = Options.DocRightIndent,
                        .SpaceAfter = 10, 'Options.DocSpaceAfter,
                        .SpaceBefore = Options.DocSpaceBefore
                    }
                }
                    'also may want to set paragraph borders and backcolor - new (RGB) or predefined
                    'para.ParagraphFormat.Borders.SetBorders(MultipleBorderTypes.Outside, BorderStyle.Single, New Color(237, 125, 49), 2)
                    para.ParagraphFormat.Borders.SetBorders(MultipleBorderTypes.Outside, BorderStyle.Single, Color.Black, 1)
                    'para.ParagraphFormat.BackgroundColor = New Color(251, 228, 213)

                    secObjectives.Blocks.Add(para)
                End If
            End If

        Next
        Return secObjectives

    End Function

    Private Function GetSurveySection(Options As ReportGeneratorOptions) As Section 'AS/18226a
        'TBD
    End Function

    Private Function GetJudgmentsOfObjectivesSection(Options As ReportGeneratorOptions) As Section 'AS/18226a
        'TBD
    End Function

    Private Function GetJudgmentsOfAlternativesSection(Options As ReportGeneratorOptions) As Section 'AS/18226a
        'TBD
    End Function

    Private Function GetContributionsSection(Options As ReportGeneratorOptions) As Section 'AS/18226a
        'TBD
    End Function

    Private Function GetDatagridSection(Options As ReportGeneratorOptions, UserIDs As List(Of Integer)) As Section 'AS/17533p + D6455 + 'AS/18712c

        Dim objH As clsHierarchy = App.ActiveProject.HierarchyObjectives
        Dim altH As clsHierarchy = App.ActiveProject.HierarchyAlternatives
        Dim columnCount As Integer = objH.Nodes.Count + 1
        Dim rowCount As Integer = altH.Nodes.Count

        Dim colAltWidth As Integer = 150 'AS/17533p===
        Dim colTotalWidth As Integer = 60
        Dim colCovobjWidth As Integer = 60
        Dim colAttributeWidth As Integer = 100 'AS/17533p==
        Dim secDatagrid As New Section(docxReport) 'AS/17533r=== moved up 'AS/18712c moved up

        For Each UserID As Integer In UserIDs 'AS/18712c enclosed
            'set section name 
            Dim sectionName As String = "Data Grid"
            Select Case UserID
                Case -1 'combined 
                    sectionName = sectionName & ": All Participants"
                Case >= 0 'individual user
                    sectionName = sectionName & ": " & PM.GetUserByID(UserID).UserName
                Case < -1 'group
                    sectionName = sectionName & ": " & PM.CombinedGroups.GetCombinedGroupByUserID(UserID).Name
            End Select

            'calc priorities for Goal for UserID 'AS/18712c=== modified and moved down
            Dim wrtNode As clsNode = PM.ActiveObjectives.GetNodeByID(1)
            PM.StorageManager.Reader.LoadUserData(PM.GetUserByID(UserID))
            Dim CalcTarget As clsCalculationTarget = PM.CalculationsManager.GetCalculationTargetByUserID(UserID)
            PM.CalculationsManager.Calculate(CalcTarget, wrtNode, PM.ActiveHierarchy, PM.ActiveAltsHierarchy)

            Dim table As New Table(docxReport)
            table.TableFormat.AutomaticallyResizeToFitContents = False 'AS/17533p

            Dim row As New TableRow(docxReport) 'AS/18712c==

            Dim Paragraph = New Paragraph(docxReport,
                        New Run(docxReport, sectionName) With {.CharacterFormat = New CharacterFormat With {.Bold = True, .Size = 16}}) With
                    {
                        .ParagraphFormat = New ParagraphFormat() With
                        {
                            .Alignment = HorizontalAlignment.Center,
                            .SpaceAfter = 20
                        }
                    }

            secDatagrid.Blocks.Add(Paragraph) 'AS/17533r==

            'Create row with Titles
            row = New TableRow(docxReport)
            table.Rows.Add(row)
            'create cell for "Alternatives", merge it with the cell below, center, and set backcolor
            Dim para As New Paragraph(docxReport, New Run(docxReport, "Alternatives") With {.CharacterFormat = New CharacterFormat With {.Bold = True}}) With {
                        .ParagraphFormat = New ParagraphFormat() With {
                            .Alignment = HorizontalAlignment.Center}}
            row.Cells.Add(New TableCell(docxReport, para) With {.RowSpan = 2, .CellFormat = New TableCellFormat() With {
                    .VerticalAlignment = VerticalAlignment.Center,
                    .BackgroundColor = Color.LightGray,
                    .PreferredWidth = New TableWidth(colAltWidth, TableWidthUnit.Point)'AS/17533p
             }})


            'create cell for "Cost", merge it with the cell below, center, and set backcolor 'AS/18712c===
            para = New Paragraph(docxReport, New Run(docxReport, "Cost") With {.CharacterFormat = New CharacterFormat With {.Bold = True}})
            row.Cells.Add(New TableCell(docxReport, para) With {.RowSpan = 2, .CellFormat = New TableCellFormat() With {
                    .VerticalAlignment = VerticalAlignment.Center,
                    .BackgroundColor = Color.LightGray,
                    .PreferredWidth = New TableWidth(colTotalWidth, TableWidthUnit.Point)
             }})

            'create cell for "P.Failure", merge it with the cell below, center, and set backcolor
            para = New Paragraph(docxReport, New Run(docxReport, "P.Failure") With {.CharacterFormat = New CharacterFormat With {.Bold = True}})
            row.Cells.Add(New TableCell(docxReport, para) With {.RowSpan = 2, .CellFormat = New TableCellFormat() With {
                    .VerticalAlignment = VerticalAlignment.Center,
                    .BackgroundColor = Color.LightGray,
                    .PreferredWidth = New TableWidth(colTotalWidth, TableWidthUnit.Point)
             }}) 'AS/18712c==

            'create cell for "Total", merge it with the cell below, center, and set backcolor
            para = New Paragraph(docxReport, New Run(docxReport, "Total") With {.CharacterFormat = New CharacterFormat With {.Bold = True}})
            row.Cells.Add(New TableCell(docxReport, para) With {.RowSpan = 2, .CellFormat = New TableCellFormat() With {
                    .VerticalAlignment = VerticalAlignment.Center,
                    .BackgroundColor = Color.LightGray,
                    .PreferredWidth = New TableWidth(colTotalWidth, TableWidthUnit.Point) 'AS/17533p
             }})

            'create cell for model name, merge it with the cells all way down to the right, set alignments and backcolor
            para = New Paragraph(docxReport, New Run(docxReport, PM.ProjectName) With {.CharacterFormat = New CharacterFormat With {.Bold = True}}) With {
                        .ParagraphFormat = New ParagraphFormat() With {
                            .Alignment = HorizontalAlignment.Center}}
            row.Cells.Add(New TableCell(docxReport, para) With {
                          .ColumnSpan = objH.TerminalNodes.Count, .CellFormat = New TableCellFormat() With {
                    .VerticalAlignment = VerticalAlignment.Center,
                    .BackgroundColor = Color.LightGray
             }})

            row = New TableRow(docxReport)
            table.Rows.Add(row)

            'create cells for covering objectives' names
            For Each node In objH.TerminalNodes
                para = New Paragraph(docxReport, New Run(docxReport, node.NodeName) With {.CharacterFormat = New CharacterFormat With {.Bold = True}}) With {
                        .ParagraphFormat = New ParagraphFormat() With {
                            .Alignment = HorizontalAlignment.Center}}
                row.Cells.Add(New TableCell(docxReport, para) With {
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
                row = New TableRow(docxReport)
                table.Rows.Add(row)
                para = New Paragraph(docxReport, alt.NodeName)
                row.Cells.Add(New TableCell(docxReport, para))

                para = New Paragraph(docxReport, PM.ResourceAligner.Scenarios.ActiveScenario.AlternativesFull.Find(Function(a) (a.ID.ToLower = alt.NodeGuidID.ToString.ToLower)).Cost.ToString) 'tue===
                row.Cells.Add(New TableCell(docxReport, para))

                para = New Paragraph(docxReport, PM.ResourceAligner.Scenarios.ActiveScenario.AlternativesFull.Find(Function(a) (a.ID.ToLower = alt.NodeGuidID.ToString.ToLower)).RiskOriginal.ToString)
                row.Cells.Add(New TableCell(docxReport, para)) 'AS/18712c==

                para = New Paragraph(docxReport, (Math.Round(alt.UnnormalizedPriority, Options.DatagridNumberOfDecimals)).ToString) 'AS/17533u
                row.Cells.Add(New TableCell(docxReport, para))

                'add data to the data columns for each covering objective 
                For Each node As clsNode In objH.Nodes
                    If node.IsTerminalNode Then
                        Dim sValue As String = "" 'AS/17533i=== mimiced from doImportJudgmentsMatrix
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

                        para = New Paragraph(docxReport, sValue)
                        row.Cells.Add(New TableCell(docxReport, para))
                    End If
                Next
            Next i

            secDatagrid.Blocks.Add(table)
            secDatagrid.Blocks.Add(New Paragraph(docxReport, New SpecialCharacter(docxReport, SpecialCharacterType.PageBreak)))
        Next 'AS/18712c

        Dim pageSetup = secDatagrid.PageSetup
        pageSetup.Orientation = Orientation.Landscape
        pageSetup.PaperType = PaperType.A3
        pageSetup.PageMargins.Left = 20
        pageSetup.PageMargins.Right = 20

        Return secDatagrid

    End Function

    Private Function GetHierarchySections(Options As ReportGeneratorOptions) As List(Of Section) 'AS/17533q 'AS/18226 added bIncludeInfodocs
        'TBD: implement Options.IncludeGoalDescription

        Dim objH As clsHierarchy = App.ActiveProject.HierarchyObjectives
        Dim nodeLeft As Double, nodeTop As Double, nodeWidth As Double, nodeHeight As Double
        nodeLeft = Options.HierarchyNodeLeft
        nodeTop = Options.HierarchyNodeTop
        nodeWidth = Options.HierarchyNodeWidth
        nodeHeight = Options.HierarchyNodeHeight

        Dim goalLeft As Double, goalTop As Double, parentLeft As Double, parentTop As Double
        goalLeft = nodeLeft
        goalTop = nodeTop
        parentLeft = nodeLeft
        parentTop = nodeTop

        Dim IsNextSection As Boolean = False 'AS/17533q

        Dim line As Shape
        Dim lineLeft As Double, lineTop As Double, lineW As Double, lineH As Double, lineIndent As Double
        lineIndent = Options.HierarchyConnectorIndent

        Dim TVsections As New List(Of Section) 'AS/17533q
        Dim secHierarchy As New Section(docxReport)
        Dim Paragraph = New Paragraph(docxReport,
                    New Run(docxReport, "Objectives Hierarchy (editable)") With {.CharacterFormat = New CharacterFormat With {.Bold = True, .Size = Options.HierarchyFontSize}}) With
                {
                    .ParagraphFormat = New ParagraphFormat() With
                    {
                        .Alignment = HorizontalAlignment.Center,
                        .SpaceAfter = Options.ReportSpaceAfterTitle
                    }
                }

        secHierarchy.Blocks.Add(Paragraph)

        Paragraph = New Paragraph(docxReport)
        secHierarchy.Blocks.Add(Paragraph)

        'Dim Group = New Group(docxReport, Layout.Inline(New Size(nodeWidth, nodeHeight)))
        Dim Group = New Group(docxReport, Layout.Inline(New Size(100, 100)))
        Paragraph.Inlines.Add(Group)

        Dim FontSize = docxReport.DefaultCharacterFormat.Size

        'draw textbox for Goal
        calcTextboxSize(objH.Nodes(0).NodeName, nodeWidth, nodeHeight, CSng(FontSize.ToString)) 'AS/17533s
        Dim TextBox = New TextBox(docxReport, Layout.Floating(
            New HorizontalPosition(nodeLeft, LengthUnit.Point, HorizontalPositionAnchor.TopLeftCorner),
            New VerticalPosition(nodeTop, LengthUnit.Point, VerticalPositionAnchor.TopLeftCorner),
            New Size(nodeWidth, nodeHeight)),
                New Paragraph(docxReport, objH.Nodes(0).NodeName) With {.ParagraphFormat = New ParagraphFormat() With {.Alignment = HorizontalAlignment.Left}})
        Group.Add(TextBox)

        'save the textbox position in the Word doc and size 
        Textboxes = New List(Of wordTextbox)
        Dim tbox As New wordTextbox(nodeLeft, nodeTop, nodeWidth, nodeHeight, objH.Nodes(0).NodeID, TVsections.Count) 'AS/17533q added tvSections.Count
        Textboxes.Add(tbox)

        Dim parentTextbox As wordTextbox 'AS/17533r

        'draw textboxes (nodes names) and connectors
        For i As Integer = 1 To objH.Nodes.Count - 1
            Dim N As clsNode = objH.Nodes(i)
            nodeLeft = goalLeft + lineIndent * 2 * N.Level
            nodeTop = nodeTop + nodeHeight * 2

            Dim pageSetup As PageSetup = secHierarchy.PageSetup 'AS/17533q===
            If nodeTop + nodeHeight + 50 >= pageSetup.PageHeight - (pageSetup.PageMargins.Bottom + pageSetup.PageMargins.Top) Then

                '========================================================
                'draw vertical connector from the parent to the bottom of the section 'AS/17533r===
                lineLeft = nodeLeft - lineIndent
                parentTextbox = getParentTextbox(N.ParentNodeID) 'get parent of current textbox

                lineTop = parentTextbox.tboxTop + nodeHeight
                lineW = 0
                lineH = nodeTop - lineTop ' + nodeHeight / 2
                line = New Shape(docxReport, ShapeType.Line, Layout.Floating(
                    New HorizontalPosition(lineLeft, LengthUnit.Point, HorizontalPositionAnchor.TopLeftCorner),
                    New VerticalPosition(lineTop, LengthUnit.Point, VerticalPositionAnchor.TopLeftCorner),
                    New Size(lineW, lineH)))
                Group.Add(line) 'AS/17533r==

                Dim nd As clsNode = objH.GetNodeByID(N.ParentNode.NodeGuidID).ParentNode
                'get parent of the parent. LATER -- insert a loop
                If IsNothing(nd) Then 'nd.Level = 0 Then
                    parentTextbox = getParentTextbox(1)
                Else
                    If nd.Level > 0 Then 'AS/17533u enclosed and added Else part
                        parentTextbox = getParentTextbox(nd.ParentNodeID)
                    Else 'AS/17533u===
                        parentTextbox = getParentTextbox(1)
                    End If 'AS/17533u==
                End If

                lineLeft = parentTextbox.tboxLeft + lineIndent
                lineTop = parentTextbox.tboxTop + nodeHeight
                lineW = 0
                lineH = nodeTop - lineTop ' + nodeHeight / 2
                line = New Shape(docxReport, ShapeType.Line, Layout.Floating(
                    New HorizontalPosition(lineLeft, LengthUnit.Point, HorizontalPositionAnchor.TopLeftCorner),
                    New VerticalPosition(lineTop, LengthUnit.Point, VerticalPositionAnchor.TopLeftCorner),
                    New Size(lineW, lineH)))
                Group.Add(line)
                '===============================================================

                TVsections.Add(secHierarchy)
                secHierarchy = New Section(docxReport)
                Paragraph = New Paragraph(docxReport)
                secHierarchy.Blocks.Add(Paragraph)

                Group = New Group(docxReport, Layout.Inline(New Size(nodeWidth, nodeHeight)))
                Paragraph.Inlines.Add(Group)
                pageSetup = secHierarchy.PageSetup
                nodeTop = 0 'pageSetup.PageMargins.Top

                TVsections.Add(secHierarchy)
                IsNextSection = True
            End If 'AS/17533q==

            calcTextboxSize(N.NodeName, nodeWidth, nodeHeight, CSng(FontSize.ToString)) 'AS/17533s
            TextBox = New TextBox(docxReport, Layout.Floating(
                New HorizontalPosition(nodeLeft, LengthUnit.Point, HorizontalPositionAnchor.TopLeftCorner),
                New VerticalPosition(nodeTop, LengthUnit.Point, VerticalPositionAnchor.TopLeftCorner),
                New Size(nodeWidth, nodeHeight)),
                    New Paragraph(docxReport, N.NodeName) With {.ParagraphFormat = New ParagraphFormat() With {.Alignment = HorizontalAlignment.Left}})
            Group.Add(TextBox)

            'save textboxes positions in the Word doc and sizes 
            tbox = New wordTextbox(nodeLeft, nodeTop, nodeWidth, nodeHeight, N.NodeID, TVsections.Count) 'AS/17533q added tvSections.Count
            Textboxes.Add(tbox)

            'horizontal connector
            lineLeft = nodeLeft - lineIndent
            lineTop = nodeTop + nodeHeight / 2
            lineW = nodeLeft - lineLeft
            lineH = 0
            Debug.Print(N.NodeName & "  HlineTop = " & lineTop.ToString & " HlineW = " & lineW.ToString)

            line = New Shape(docxReport, ShapeType.Line, Layout.Floating(
                New HorizontalPosition(lineLeft, LengthUnit.Point, HorizontalPositionAnchor.TopLeftCorner),
                New VerticalPosition(lineTop, LengthUnit.Point, VerticalPositionAnchor.TopLeftCorner),
                New Size(lineW, lineH)))
            Group.Add(line)

            'draw vertical connector from parent to current
            lineLeft = nodeLeft - lineIndent
            parentTextbox = getParentTextbox(N.ParentNodeID) 'get parent of current textbox

            'lineTop = parentTextbox.tboxTop + nodeHeight 'AS/17533q
            If IsNextSection Then 'AS/17533q===
                parentTextbox.tboxTop = 0 'secHierarchy.PageSetup.PageMargins.Top
                lineTop = parentTextbox.tboxTop
                IsNextSection = False
            Else
                If parentTextbox.tboxPage < TVsections.Count Then
                    lineTop = 0
                Else
                    lineTop = parentTextbox.tboxTop + nodeHeight
                End If
            End If 'AS/17533q==

            lineW = 0
            lineH = nodeTop - lineTop + nodeHeight / 2
            line = New Shape(docxReport, ShapeType.Line, Layout.Floating(
                New HorizontalPosition(lineLeft, LengthUnit.Point, HorizontalPositionAnchor.TopLeftCorner),
                New VerticalPosition(lineTop, LengthUnit.Point, VerticalPositionAnchor.TopLeftCorner),
                New Size(lineW, lineH)))
            Group.Add(line)
        Next i

        Return TVsections 'secHierarchy 'AS/17533q

    End Function

    Private Function GetPictureSection(server As HttpServerUtility, Options As ReportGeneratorOptions, sReportPartName As String) As Section 'AS/17533p

        'Dim picture1 As New Picture(docxReport, server.MapPath("~/Images/favicon/Chart_ITP_Alts.png"), Options.PictureWidth, Options.PictureHeght, LengthUnit.Pixel)
        Dim picName As String 'AS/18226b===
        Dim picTitle As String
        Select Case sReportPartName
            Case "Hierarchy"
                picName = "ITP_Reports_Treeview.png"
                picTitle = "Objectives Hierarchy (image)"
            Case "JudgmentsOfObjectives"
                picName = "ITP_Reports_Judgments-Of-Objectives_page1.png"
                picTitle = "Judgments Of Objectives (image)"
            Case Else
                picName = "Chart_ITP_Alts.png"
                picTitle = "Alternatives Chart (image)"
        End Select
        Dim pic As New Picture(docxReport, server.MapPath("~/Images/favicon/" & picName), Options.PictureWidth, Options.PictureHeght, LengthUnit.Pixel) 'AS/18226b==

        Dim secPicture As New Section(docxReport)
        'Dim Paragraph = New Paragraph(docxReport)
        Dim Paragraph = New Paragraph(docxReport,
                    New Run(docxReport, picTitle) With {.CharacterFormat = New CharacterFormat With {.Bold = True, .Size = Options.PictureTitleFontSize}}) With
                {
                    .ParagraphFormat = New ParagraphFormat() With
                    {
                        .Alignment = HorizontalAlignment.Center,
                        .SpaceAfter = Options.ReportSpaceAfterTitle
                    }
                }
        secPicture.Blocks.Add(Paragraph)

        'Dim Group = New Group(docxReport, Layout.Inline(New Size(Options.PictureWidth, Options.PictureHeght)))'AS/17533t
        Paragraph = New Paragraph(docxReport) 'AS/17533t fix to: it was putting the chart on the same line with the title 
        Paragraph.Inlines.Add(pic)
        secPicture.Blocks.Add(Paragraph) 'AS/17533t

        Return secPicture

    End Function

#End Region

#Region "Common (reusable) functions"
    Private Function ReportInclude(sPartName As String, Options As ReportGeneratorOptions) As Boolean 'AS/18226
        For Each part As ReportGeneratorPart In Options.ReportIncludeParts
            If part.PartName = sPartName Then
                Return True
            End If
        Next
        Return False

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

    Sub calcTextboxSize(tbText As String, ByRef W As Double, ByRef H As Double, FSize As Single) 'AS/17533s
        'source -- vbcity.com/forums/t/163824.aspx
        'TIP (AS): to speed up the calculation, do it just once for 10 characters and then proportionally apply to the actual number of charachters
        Dim stringFont As System.Drawing.Font = New System.Drawing.Font("Arial", FSize)
        Dim s As System.Drawing.Size = System.Windows.Forms.TextRenderer.MeasureText(tbText, stringFont)
        W = s.Width
        H = s.Height * 1.3
    End Sub

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
#End Region

    Public Sub New(_App As clsComparionCore)
        App = _App
        PM = App.ActiveProject.ProjectManager
        PM.ProjectName = App.ActiveProject.ProjectName 'AS/18712t

        ' If using Professional version, put your serial key below.
        ComponentInfo.SetLicense("DN-2019Oct23-o9cPV8FqJpXV/58cTjlRakTEbT0YmbK2qfwJS4OkrVTGqcdjqxHlgRDfWwwv2q3j9qruUP258vApmepOW0Z46sRTc6Q==A")

        ' Create new empty document.
        docxReport = New DocumentModel()

    End Sub

    Private Class wordTextbox
        Public tboxLeft As Double
        Public tboxTop As Double
        Public tboxbW As Double
        Public tboxH As Double
        Public nodeID As Integer
        Public tboxPage As Integer 'AS/17533q

        Public Sub New(L As Double, T As Double, W As Double, H As Double, nID As Integer, P As Integer)
            tboxLeft = L
            tboxTop = T
            tboxbW = W
            tboxH = H
            nodeID = nID
            tboxPage = P
        End Sub
    End Class

End Class
