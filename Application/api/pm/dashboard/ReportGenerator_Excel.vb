Imports System.IO
Imports GemBox.Spreadsheet
Imports GemBox.Spreadsheet.Charts
Imports GemBox.Spreadsheet.Drawing
Imports GemBox.Spreadsheet.Tables 'AS/18712f
Imports System.Drawing

Imports System.Collections.ObjectModel 'AS/19203e


<Serializable> Public Class ReportGenerator_Excel
    Private App As clsComparionCore
    Private PM As clsProjectManager
    Private workbook As ExcelFile
    Private WRTNodeID As Integer 'AS/19464a

    Private Const NODE_PATH_DELIMITER As String = " | " '  & vbCrLf 'AS/18712h
    Private TEMP_SINGLE_VALUE As Single = 0.1111111 'AS/18712f
    Private TEMP_STRING_VALUE As String = "temp string value"

    Dim ecColors As ECColors 'AS/18712w
    Dim ColorScheme As String = "green" 'green by default 'AS/18712w

    Dim UserIDs As ArrayList = Nothing ''AS/18712x

    Function GenerateExcelReport(filePath As String, server As HttpServerUtility, Options As ReportGeneratorOptions) As ExcelFile 'AS/18712d

        Dim TestUserIDs As New List(Of Integer)
        TestUserIDs.Add(-1)
        'TestUserIDs.Add(-1000)
        'TestUserIDs.Add(2)

        If Options Is Nothing Then
            Options = New ReportGeneratorOptions
            Options.ReportTitle = PM.ProjectName
        End If
        ColorScheme = Options.ReportColorScheme 'AS/18712w
        PrepareUsersData(UserIDs) '

        Dim worksheet As ExcelWorksheet
        'Dim worksheets As List(Of ExcelWorksheet)
        Dim startElapsedTime As DateTime = Now

        ''  ===== REPORTS TAB =====                                                === REPORT NAME as it is on the REPORTS tab ===

        'worksheets = AddDatagridWorksheets(Options, TestUserIDs) '                      "Data Grid"
        'worksheet = AddObjectivesWorksheet(Options) '                                   "Objectives" 
        'worksheet = AddAlternativesWorksheet(Options) '                                 "Alternatives"
        'worksheet = AddSurveyWorksheet(Options) '                                       "Survey Results"
        'worksheet = AddInconsistencyWorksheet(Options) '                                "Inconsistency"

        ''===== Model Documentation  
        'worksheet = AddOverallResultsWorksheet(Options) '                               "Overall Results" 
        'worksheet = AddObjAltsPrioritiesWorksheet(Options) '                            "Objectives / Alternatives Piorities"
        'worksheet = AddPriorityOfObjectivesWorksheet(Options) 'AS/18712                 "Priority Of Objectives" 'AS/18712w 
        'worksheet = AddJudgmentsOfAlternativesWorksheet(Options) 'AS/18712i             "Judgments Of Alternatives"  
        ''worksheet = AddJudgmentsOfObjectivesWorksheet(Options, True) 'AS/18712j          "Judgments Of Objectives"
        'worksheet = AddPictureWorksheet(server, Options, "JudgmentsOfObjectives") '     Judgments of Objectives (image) 'AS/18712u==
        'worksheet = AddObjectivesAndAlternativesWorksheet(Options) 'AS/18712k           "Objectives And Alternatives"
        'worksheet = AddContributionsWorksheet(Options) 'AS/18712l                       "Contributions"

        ''===== Ad-Hog Reports
        'worksheet = AddJudgmentsOverviewWorksheet(Options) 'AS/18712m                   "Judgments Overview"   
        'worksheet = AddObjectivesPrioritiesWorksheet(Options) 'AS/18712n                "Objectives Priorities" 
        'worksheet = AddAlternativesPrioritiesWorksheet(Options) 'AS/18712o              "Alternatives Priorities" 
        'worksheet = AddEvaluationProgressWorksheet(Options) 'AS/18712p                  "Evaluation Progress"

        ''''===== Additional report parts (not on the REPORTS tab)
        'worksheet = AddPictureWorksheet(server, Options, "Hierarchy") '                 'Objectives Hierarchy (image) 'AS/18712u===
        ''worksheet = AddPictureWorksheet(server, Options, "Chart") '                    'Alternatives chart (image)
        'worksheet = AddHierarchyWorksheet(Options) '                                    'Hierarchy (treeview)'AS/18712v
        ''worksheet = AddTitleWorksheet(Options) 'Model Title and Description 
        ''worksheet = AddInfodocWorksheet(Options, "Goal") 'Goal Description 

        ''''  ===== SYNTHESIS TAB =====                                                   === MENU NAME as it is on the SYNTHESIS tab ===
        worksheet = AddAlternativesGrid(Options) 'AS/19203a                                'Alternatives Grid
        worksheet = AddObjectivesGrid(Options) 'AS/19203b                                  'Objectives Grid
        'worksheet = AddAlternativesBarChart(Options) 'AS/18712z===                      'Alternatives Chart
        ''worksheet = AddAlternativesComponentsChart(Options)                             'Alternatives Chart (with Components selected)
        worksheet = AddObjectivesBarChart(Options)                                      'Objectives Chart
        ''worksheet = AddObjectivesBarChart_MultiUsers()                                 'not finished, put on hold since Hal thinks it doesn't make sense to do charts for multiple users, it may be thousands of them. So focusing on single user or group.
        ''worksheet = AddObjectivesComponentsChart(Options)                              'Objectives Chart (with Components selected)

        ''worksheet = Example_ComponentsChart(Options)
        ''worksheet = Example_CreateSamplePieChart(Options) 'AS/18712z==

        Debug.Print("Overall Elapsed time: " & (Math.Round((Now - startElapsedTime).TotalSeconds, 2).ToString) & " sec")

        workbook.Save(filePath)
        Return workbook

    End Function

    Function GenerateExcelReport_DEBUG(filePath As String, server As HttpServerUtility, Options As ReportGeneratorOptions) As ExcelFile 'AS/18712d

        Dim TestUserIDs As New List(Of Integer)
        TestUserIDs.Add(-1)
        TestUserIDs.Add(-1000)
        TestUserIDs.Add(2)

        If Options Is Nothing Then
            Options = New ReportGeneratorOptions
            Options.ReportTitle = PM.ProjectName
        End If
        ColorScheme = Options.ReportColorScheme 'AS/18712w

        ''======= Built-in Gembox Spreadsheet Colors and Table Styles (enable to create the corresponding xlsx files) 'AS/18712e===
        '=== Colors
        'Dim worksheet1 = GemboxSpreadsheetColors()
        'Dim worksheet1 = ECColorsGuide()

        ''=== Table Styles===
        'Dim worksheets1 As List(Of ExcelWorksheet)
        'worksheets1 = SpreadsheetTableStyles()'Table Styles==

        ''=== Multi-line text
        'Dim worksheet1 = workbook.Worksheets.Add("Multi-line text")
        'Dim cell = worksheet1.Cells("A1")
        'cell.Value = "First line." & vbCrLf & "Second line." & vbCrLf & "Third line."

        ''=== Split function
        'Dim worksheet1 = Test_AutofitReplace()

        ''=== Insert Picture and Text
        'Dim worksheet1 = Test_InsertImageAndText(server, Options)
        Dim worksheet1 = Test_SizeImageInCell(server, Options) 'AS/19203h

        ''=== Add Border To Image
        worksheet1 = Test_AddBorderToImage(Options, server)

        '=== Common part
        workbook.Save(filePath)
        Return workbook
        Exit Function
        ''============================================ 'AS/18712e==

        PrepareUsersData(UserIDs) '

        Dim worksheet As ExcelWorksheet
        'Dim worksheets As List(Of ExcelWorksheet)
        Dim startTime As DateTime = Now
        Dim startElapsedTime As DateTime = Now

        '  ===== REPORTS TAB =====                                                  === MENU name as it is on the REPORTS tab ===
        ''startTime = Now
        'worksheets = AddDatagridWorksheets(Options, TestUserIDs) '                      "Data Grid"
        ''Debug.Print("Report 1: " & (Now - startTime).TotalSeconds.ToString & " sec") 
        ''startTime = Now
        'worksheet = AddObjectivesWorksheet(Options) '                                   "Objectives" 
        ''Debug.Print("Report 2: " & (Now - startTime).TotalSeconds.ToString & " sec") 
        ''startTime = Now
        'worksheet = AddAlternativesWorksheet(Options) '                                 "Alternatives"
        ''Debug.Print("Report 3: " & (Now - startTime).TotalSeconds.ToString & " sec") 
        ''startTime = Now
        'worksheet = AddSurveyWorksheet(Options) '                                       "Survey Results"
        ''Debug.Print("Report 4: " & (Now - startTime).TotalSeconds.ToString & " sec") 
        ''startTime = Now
        'worksheet = AddInconsistencyWorksheet(Options) '                                "Inconsistency"
        ''Debug.Print("Report 5: " & (Now - startTime).TotalSeconds.ToString & " sec") 

        ''===== Model Documentation  
        ''startTime = Now
        'worksheet = AddOverallResultsWorksheet(Options) '                               "Overall Results" 
        ''Debug.Print("Report 6: " & (Now - startTime).TotalSeconds.ToString & " sec") 
        ''startTime = Now
        'worksheet = AddObjAltsPrioritiesWorksheet(Options) '                            "Objectives / Alternatives Piorities"
        ''Debug.Print("Report 7: " & (Now - startTime).TotalSeconds.ToString & " sec") 
        ''startTime = Now
        'worksheet = AddPriorityOfObjectivesWorksheet(Options) 'AS/18712                 "Priority Of Objectives" 'AS/18712w 
        ''Debug.Print("Report 8: " & (Now - startTime).TotalSeconds.ToString & " sec") 
        ''startTime = Now
        ''worksheets = AddPriorityOfObjectivesWorksheets(Options) 'AS/18712               "Priority Of Objectives" -- 4 color schemes 'AS/18712w 
        ''Debug.Print("Report 9: " & (Now - startTime).TotalSeconds.ToString & " sec") 
        ''startTime = Now
        'worksheet = AddJudgmentsOfAlternativesWorksheet(Options) 'AS/18712i             "Judgments Of Alternatives"  
        ''Debug.Print("Report 10: " & (Now - startTime).TotalSeconds.ToString & " sec") 
        ''startTime = Now
        'worksheet = AddJudgmentsOfObjectivesWorksheet(Options) 'AS/18712j               "Judgments Of Objectives"
        ''Debug.Print("Report 11: " & (Now - startTime).TotalSeconds.ToString & " sec") 
        ''startTime = Now
        'worksheet = AddObjectivesAndAlternativesWorksheet(Options) 'AS/18712k           "Objectives And Alternatives"
        ''Debug.Print("Report 12: " & (Now - startTime).TotalSeconds.ToString & " sec") 
        ''startTime = Now
        'worksheet = AddContributionsWorksheet(Options) 'AS/18712l                       "Contributions"
        ''Debug.Print("Report 13: " & (Now - startTime).TotalSeconds.ToString & " sec") 

        ''===== Ad-Hog Reports

        ''''startTime = Now
        ''worksheet = AddJudgmentsOverviewWorksheet_byUsers(Options) 'AS/18712m          "Judgments Overview"   
        ''''Debug.Print("Report 14a: " & (Now - startTime).TotalSeconds.ToString & " sec") 
        ''startTime = Now
        'worksheet = AddJudgmentsOverviewWorksheet(Options) 'AS/18712m                   "Judgments Overview"   
        ''Debug.Print("Report 14: " & (Now - startTime).TotalSeconds.ToString & " sec") 
        ''startTime = Now
        'worksheet = AddObjectivesPrioritiesWorksheet(Options) 'AS/18712n                "Objectives Priorities" 
        ''Debug.Print("Report 15: " & (Now - startTime).TotalSeconds.ToString & " sec") 
        ''startTime = Now
        'worksheet = AddAlternativesPrioritiesWorksheet(Options) 'AS/18712o              "Alternatives Priorities" 
        ''Debug.Print("Report 16: " & (Now - startTime).TotalSeconds.ToString & " sec") 
        ''startTime = Now
        'worksheet = AddEvaluationProgressWorksheet(Options) 'AS/18712p                  "Evaluation Progress"

        ''===== Additional report parts (not on the REPORTS tab)
        ''Debug.Print("Report 17: " & (Now - startTime).TotalSeconds.ToString & " sec") 
        ''startTime = Now
        'worksheet = AddPictureWorksheet(server, Options, "Hierarchy") '                 Objectives Hierarchy (image) 'AS/18712u===
        ''Debug.Print("Report 18: " & (Now - startTime).TotalSeconds.ToString & " sec") 
        ''startTime = Now
        'worksheet = AddPictureWorksheet(server, Options, "Chart") '                     Alternatives chart (image)
        ''Debug.Print("Report 19: " & (Now - startTime).TotalSeconds.ToString & " sec") 
        ''startTime = Now
        'worksheet = AddPictureWorksheet(server, Options, "JudgmentsOfObjectives") '     Judgments of Objectives (image) 'AS/18712u==
        ''Debug.Print("Report 20: " & (Now - startTime).TotalSeconds.ToString & " sec") 
        ''startTime = Now
        'worksheet = AddHierarchyWorksheet(Options) '                                    Hierarchy (treeview)'AS/18712v
        ''Debug.Print("Report 21: " & (Now - startTime).TotalSeconds.ToString & " sec") 
        ''startTime = Now
        'worksheet = AddTitleWorksheet(Options) 'Model Title and Description 
        ''Debug.Print("Report 22: " & (Now - startTime).TotalSeconds.ToString & " sec") 
        ''startTime = Now
        'worksheet = AddInfodocWorksheet(Options, "Goal") 'Goal Description 
        ''Debug.Print("Report 23: " & (Now - startTime).TotalSeconds.ToString & " sec") 

        '  ===== SYNTHESIS TAB =====                                                === MENU NAME as it is on the SYNTHESIS tab ===
        'startTime = Now
        ' worksheet = AddAlternativesGrid(Options) 'AS/19203a                           "Alternatives Grid"
        'Debug.Print("Report 24: " & (Now - startTime).TotalSeconds.ToString & " sec")
        'startTime = Now
        'worksheet = AddObjectivesGrid_xxx(Options) 'AS/19203b                          "Objectives Grid xxx" version after moving the piece up out of the loop
        'Debug.Print("Report 25: " & (Now - startTime).TotalSeconds.ToString & " sec")
        startTime = Now
        worksheet = AddObjectivesGrid(Options) 'AS/19203b                               "Objectives Grid" faster version
        Debug.Print("Report 26: " & (Now - startTime).TotalSeconds.ToString & " sec")

        Debug.Print("Overall Elapsed time: " & (Now - startElapsedTime).TotalSeconds.ToString & " sec")

        workbook.Save(filePath)
        Return workbook

    End Function


#Region "Export - Synthesize tab"

    Private Function AddObjectivesGrid(Options As ReportGeneratorOptions) As ExcelWorksheet 'AS/19203b
        Dim worksheet = workbook.Worksheets.Add("Objectives Grid")

        Dim uid As Integer = -1 'All Participants
        Dim wrtNode As clsNode = PM.ActiveObjectives.GetNodeByID(1)

        'Get objectives and alternatives
        Dim objH As clsHierarchy = App.ActiveProject.HierarchyObjectives
        Dim altH As clsHierarchy = PM.AltsHierarchy(PM.ActiveAltsHierarchy)

        'define rows and columns
        Dim rowColHeader As Integer = 0 'column header
        Dim colUserName As Integer = 0
        Dim colObjective As Integer = 1
        Dim colPath As Integer = 2
        Dim colLocalPriority As Integer = 3
        Dim colGlobalPriority As Integer = 4
        Dim colInfodoc As Integer = 5
        Dim colRightMostColumn As Integer = colInfodoc 'AS/19203j

        Dim jValue As Double = UNDEFINED_SINGLE_VALUE

        Dim row As Integer = 1 '3
        Dim col As Integer = colInfodoc
        Dim NodePath As String = ""

        Dim MyComputer As New Devices.Computer 'AS/19203e
        Dim lstInfoPictures As New List(Of KeyValuePair(Of String, ExcelPicture))

        With worksheet

            .Cells(rowColHeader, colUserName).Value = "Participant Name"
            .Cells(rowColHeader, colObjective).Value = "Objective"
            .Cells(rowColHeader, colPath).Value = "Path"
            .Cells(rowColHeader, colLocalPriority).Value = "Local Priority"
            .Cells(rowColHeader, colGlobalPriority).Value = "Global Priority"
            .Cells(rowColHeader, colInfodoc).Value = "Infodoc"

            Dim users As List(Of Integer) = SelectedUsersAndGroupsIDs 'AS/19203d

            For Each UserID As Integer In users 'AS/19203d
                'Dim u As clsUser = PM.GetUserByID(UserID) 'AS/19203c===
                Dim username As String = ""
                Select Case UserID
                    Case >= 0 'individual user
                        username = PM.GetUserByID(UserID).UserName
                    Case Else 'group
                        username = PM.CombinedGroups.GetCombinedGroupByUserID(UserID).Name
                End Select 'AS/19203c==

                'Dim CalcTarget As clsCalculationTarget = PM.CalculationsManager.GetCalculationTargetByUserID(UserID) 'AS/19203d 
                'PM.CalculationsManager.Calculate(CalcTarget, wrtNode)'AS/19203d 

                Dim objs As New Dictionary(Of Integer, List(Of Tuple(Of Integer, Integer, NodePriority)))
                Dim alts As New Dictionary(Of Integer, List(Of NodePriority))
                Dim usersIDlist As List(Of Integer) = New List(Of Integer) 'AS/19203d
                usersIDlist.Add(UserID) 'AS/19203d
                PM.CalculationsManager.ShowDueToPriorities = Not PM.Parameters.ShowLikelihoodsGivenSources
                PM.CalculationsManager.GetAlternativesGrid(wrtNode.NodeID, usersIDlist, objs, alts) 'AS/19203d

                'insert row for Goal 'AS/19203c===
                Dim node As clsNode = objH.GetNodeByID(1)
                .Cells(row, colUserName).Value = username ' u.UserName
                .Cells(row, colObjective).Value = node.NodeName
                Dim sText As String = Infodoc2Text(App.ActiveProject, node.InfoDoc)
                .Cells(row, colInfodoc).Value = sText 'AS/19203c 
                row += 1 'AS/19203c==

                If objs.Count > 0 AndAlso objs.First.Value.Count > 0 Then
                    For Each t As Tuple(Of Integer, Integer, clsNode) In PM.Hierarchy(PM.ActiveHierarchy).NodesInLinearOrder
                        Dim NodeId As Integer = t.Item1
                        Dim ParentNodeID As Integer = t.Item2
                        node = objH.GetNodeByID(NodeId)
                        If objs.First.Value.Find(Function(x) x.Item1 = NodeId AndAlso x.Item2 = ParentNodeID) IsNot Nothing Then
                            For Each pair As KeyValuePair(Of Integer, List(Of Tuple(Of Integer, Integer, NodePriority))) In objs
                                Dim nPriority As NodePriority = pair.Value.Find(Function(x) x.Item1 = NodeId AndAlso x.Item2 = ParentNodeID).Item3
                                .Cells(row, colLocalPriority).Value = nPriority.LocalPriority
                                .Cells(row, colGlobalPriority).Value = nPriority.GlobalPriority

                                .Cells(row, colUserName).Value = username 'u.UserName
                                .Cells(row, colObjective).Value = node.NodeName
                                NodePath = ""
                                GetFullNodePath(node, NodePath)
                                .Cells(row, colPath).Value = NodePath
                                sText = Infodoc2Text(App.ActiveProject, node.InfoDoc) 'AS/19203c
                                .Cells(row, colInfodoc).Value = sText 'AS/19203c

                                col = colInfodoc + 1 'AS/19203j===

                                Dim ProjectID As Integer = App.ActiveProject.ID
                                Dim ActiveHierarchyID As Integer = PM.ActiveHierarchy
                                Dim InfoDocType As reObjectType = reObjectType.Alternative
                                Dim sInfodocID As String = node.NodeID.ToString
                                Dim WRTParentNode As Integer = node.ParentNodeID

                                Dim anchor As AnchorCell = New AnchorCell(.Columns(col), .Rows(row), True)
                                Dim picPosition As String = anchor.Column.Name & anchor.Row.Name

                                Dim sContent As String = node.InfoDoc
                                If sContent <> "" Then
                                    sContent = Infodoc_Unpack(ProjectID, ActiveHierarchyID, InfoDocType, sInfodocID, sContent, True, True, -1)

                                    Dim path As String = Infodoc_Path(ProjectID, ActiveHierarchyID, InfoDocType, sInfodocID, WRTParentNode)

                                    Dim files As ReadOnlyCollection(Of String)
                                    files = MyComputer.FileSystem.GetFiles(path, FileIO.SearchOption.SearchAllSubDirectories, _MHT_SearchFilesList)

                                    For Each sFile As String In files
                                        .Cells(rowColHeader, col).Value = "Infodoc Image(s)"
                                        Dim mediaFileData As FileInfo = MyComputer.FileSystem.GetFileInfo(sFile)
                                        Dim pic As ExcelPicture = .Pictures.Add(mediaFileData.FullName, picPosition)
                                        Dim position = pic.Position
                                        colRightMostColumn = position.To.Column.Index
                                        lstInfoPictures.Add(New KeyValuePair(Of String, ExcelPicture)(mediaFileData.Extension, pic))
                                        col += 1
                                    Next

                                    'get images downloaded as part of HTM file

                                    Dim imageList As List(Of KeyValuePair(Of String, MemoryStream)) = Infodoc_GetInlineImages(sContent)  ' D6952
                                    For Each imagePair As KeyValuePair(Of String, MemoryStream) In imageList
                                        Dim imageType As String = imagePair.Key
                                        Dim imageStream As MemoryStream = imagePair.Value
                                        Dim anchor1 = New AnchorCell(.Columns(col), .Rows(row), True)
                                        Dim anchor2 = New AnchorCell(.Columns(col + 1), .Rows(row + 1), True)
                                        Dim picType As ExcelPictureFormat = getExcelPictureFormat(imageType)

                                        Dim pic = .Pictures.Add(imageStream, picType, anchor1, anchor2)
                                        lstInfoPictures.Add(New KeyValuePair(Of String, ExcelPicture)("htm", pic))

                                        colRightMostColumn = col
                                        col += 1
                                    Next
                                    If colRightMostColumn < col - 1 Then colRightMostColumn = col - 1 'AS/19203f==

                                End If 'AS/19203j=
                                row += 1
                            Next
                        End If
                    Next
                End If
            Next 'user

            'freeze top row
            .Panes = New WorksheetPanes(PanesState.Frozen, 0, 1, "A2", PanePosition.TopRight)

            'format column headers
            Dim cellRange As CellRange = .Cells.GetSubrangeAbsolute(rowColHeader, colUserName, rowColHeader, col)
            cellRange.Style = style_ColumnHeaderBW()
            cellRange.Style.Borders.SetBorders(MultipleBorders.All, Color.Black, LineStyle.Thin)

            'format table body
            cellRange = .Cells.GetSubrangeAbsolute(rowColHeader + 1, colUserName, row, col)
            cellRange.Style = style_TableBodyNormal()

            cellRange = .Cells.GetSubrangeAbsolute(rowColHeader, colUserName, row, col)
            cellRange.Style.Borders.SetBorders(MultipleBorders.All, Color.Black, LineStyle.Thin)

            'set cols widths and number format
            Dim colW As Integer
            .Columns(colUserName).AutoFit()
            .Columns(colObjective).AutoFit()
            colW = CInt(.Columns(colObjective).Width * 1.1)
            .Columns(colObjective).Width = colW
            .Columns(colPath).Width = colW
            .Columns(colInfodoc).Width = colW
            .Columns(colLocalPriority).Style.NumberFormat = "0.00%"
            .Columns(colGlobalPriority).Style.NumberFormat = "0.00%"

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

    Private Function AddAlternativesGrid(Options As ReportGeneratorOptions) As ExcelWorksheet 'AS/19203a
        Dim worksheet = workbook.Worksheets.Add("Alternatives Grid")

        Dim uid As Integer = -1 'All Participants
        Dim wrtNode As clsNode = PM.ActiveObjectives.GetNodeByID(1)

        'Get objectives and alternatives
        Dim objH As clsHierarchy = App.ActiveProject.HierarchyObjectives
        Dim altH As clsHierarchy = PM.AltsHierarchy(PM.ActiveAltsHierarchy)

        'define rows and columns
        Dim rowColHeader As Integer = 0 'column header
        Dim colUserName As Integer = 0
        Dim colAlternative As Integer = 1
        Dim colAltPriority As Integer = 2
        Dim colCost As Integer = 3
        Dim colPFailure As Integer = 4
        Dim colEnabled As Integer = 5
        Dim customAttributes As List(Of clsAttribute) = PM.Attributes.GetAlternativesAttributes(True)
        Dim colInfodoc As Integer = colEnabled + customAttributes.Count + 1 'AS/19203e
        Dim colRightMostColumn As Integer = colInfodoc 'AS/19203f

        Dim jValue As Double = UNDEFINED_SINGLE_VALUE

        Dim row As Integer = 1 '3
        Dim col As Integer = 0
        Dim MyComputer As New Devices.Computer 'AS/19203e
        Dim lstInfoPictures As New List(Of KeyValuePair(Of String, ExcelPicture))

        With worksheet

            .Cells(rowColHeader, colUserName).Value = "Participant Name"
            .Cells(rowColHeader, colAlternative).Value = "Alternative Name"
            .Cells(rowColHeader, colAltPriority).Value = "Priority"
            .Cells(rowColHeader, colCost).Value = "Cost"
            .Cells(rowColHeader, colPFailure).Value = "P. Failure"
            .Cells(rowColHeader, colEnabled).Value = "Enabled"
            .Cells(rowColHeader, colInfodoc).Value = "Infodoc" 'AS/19203e

            For Each UserID As Integer In UserIDs
                Dim u As clsUser = PM.GetUserByID(UserID)

                Dim CalcTarget As clsCalculationTarget = PM.CalculationsManager.GetCalculationTargetByUserID(UserID)
                PM.CalculationsManager.Calculate(CalcTarget, wrtNode)

                Dim tNormMode As LocalNormalizationType = CType(PM.Parameters.Normalization, LocalNormalizationType)
                Dim objs As New Dictionary(Of Integer, List(Of Tuple(Of Integer, Integer, NodePriority)))
                Dim alts As New Dictionary(Of Integer, List(Of NodePriority))
                'Dim users As List(Of Integer) = SelectedUsersAndGroupsIDs
                Dim users As List(Of Integer) = New List(Of Integer)
                users.Add(UserID)

                PM.CalculationsManager.ShowDueToPriorities = Not PM.Parameters.ShowLikelihoodsGivenSources
                PM.CalculationsManager.GetAlternativesGrid(wrtNode.NodeID, users, objs, alts)

                .Cells(row, colUserName).Value = u.UserName

                If alts.Count > 0 AndAlso alts.First.Value.Count > 0 Then
                    For i As Integer = 0 To alts.First.Value.Count - 1
                        For Each pair As KeyValuePair(Of Integer, List(Of NodePriority)) In alts
                            Dim altPriority As Double
                            Dim alt As clsNode = GetNodeByID(altH.Nodes, pair.Value(i).NodeID)
                            .Cells(row, colAlternative).Value = alt.NodeName
                            If tNormMode = LocalNormalizationType.ntUnnormalized Then
                                altPriority = pair.Value(i).GlobalPriority
                            Else
                                altPriority = pair.Value(i).NormalizedGlobalPriority
                            End If
                            .Cells(row, colAltPriority).Value = altPriority
                            .Cells(row, colUserName).Value = u.UserName

                            .Cells(row, colCost).Value = PM.ResourceAligner.Scenarios.ActiveScenario.AlternativesFull.Find(Function(a) (a.ID.ToLower = alt.NodeGuidID.ToString.ToLower)).Cost 'AS/18712c
                            .Cells(row, colPFailure).Value = PM.ResourceAligner.Scenarios.ActiveScenario.AlternativesFull.Find(Function(a) (a.ID.ToLower = alt.NodeGuidID.ToString.ToLower)).RiskOriginal 'AS/18712c
                            .Cells(row, colEnabled).Value = PM.ResourceAligner.Scenarios.ActiveScenario.AlternativesFull.Find(Function(a) (a.ID.ToLower = alt.NodeGuidID.ToString.ToLower)).Enabled

                            'define attributes columns and write values into them
                            col = colEnabled + 1
                            For Each attr As clsAttribute In customAttributes
                                .Cells.Item(rowColHeader, col).Value = attr.Name
                                Select Case attr.ValueType'AS/18387b===
                                    Case AttributeValueTypes.avtEnumeration, AttributeValueTypes.avtEnumerationMulti
                                        Dim aEnum As clsAttributeEnumeration = PM.Attributes.GetEnumByID(attr.EnumID) '(mimiced the piece from Public Function AddEnumAttributeItem in CWSw\CoreWS_OperationContracts.vb)
                                        If Not (aEnum Is Nothing OrElse attr.EnumID.Equals(Guid.Empty)) Then
                                            Dim itemGUID As String = (PM.Attributes.GetAttributeValue(attr.ID, alt.NodeGuidID)).ToString
                                            Dim val As String = aEnum.GetItemByID(New Guid(itemGUID)).Value
                                            .Cells.Item(row, col).Value = val
                                        End If
                                    Case Else 'AS/18387b==
                                        Dim val As Object = PM.Attributes.GetAttributeValue(attr.ID, alt.NodeGuidID)
                                        If IsNothing(val) Then
                                            .Cells.Item(row, col).Value = String.Empty
                                        Else
                                            .Cells.Item(row, col).Value = val.ToString
                                        End If
                                End Select
                                col = col + 1
                            Next

                            Dim sText As String = Infodoc2Text(App.ActiveProject, alt.InfoDoc) 'AS/19203e===
                            .Cells(row, colInfodoc).Value = sText
                            col = colInfodoc + 1 'AS/19203f

                            Dim ProjectID As Integer = App.ActiveProject.ID
                            Dim ActiveHierarchyID As Integer = PM.ActiveHierarchy
                            Dim InfoDocType As reObjectType = reObjectType.Alternative
                            Dim sInfodocID As String = alt.NodeID.ToString
                            Dim WRTParentNode As Integer = alt.ParentNodeID

                            Dim anchor As AnchorCell = New AnchorCell(.Columns(col), .Rows(row), True)
                            Dim picPosition As String = anchor.Column.Name & anchor.Row.Name

                            Dim sContent As String = alt.InfoDoc
                            If sContent <> "" Then
                                sContent = Infodoc_Unpack(ProjectID, ActiveHierarchyID, InfoDocType, sInfodocID, sContent, True, True, -1)

                                Dim path As String = Infodoc_Path(ProjectID, ActiveHierarchyID, InfoDocType, sInfodocID, WRTParentNode)

                                Dim files As ReadOnlyCollection(Of String)
                                files = MyComputer.FileSystem.GetFiles(path, FileIO.SearchOption.SearchAllSubDirectories, _MHT_SearchFilesList)

                                For Each sFile As String In files
                                    .Cells(rowColHeader, col).Value = "Infodoc Image(s)"
                                    Dim mediaFileData As FileInfo = MyComputer.FileSystem.GetFileInfo(sFile)
                                    Dim pic As ExcelPicture = .Pictures.Add(mediaFileData.FullName, picPosition) 'AS/19203g
                                    Dim position = pic.Position
                                    colRightMostColumn = position.To.Column.Index
                                    lstInfoPictures.Add(New KeyValuePair(Of String, ExcelPicture)(mediaFileData.Extension, pic)) 'AS/19203h
                                    col += 1
                                Next

                                'get images downloaded as part of HTM file

                                Dim imageList As List(Of KeyValuePair(Of String, MemoryStream)) = Infodoc_GetInlineImages(sContent)     ' D6952
                                For Each imagePair As KeyValuePair(Of String, MemoryStream) In imageList
                                    Dim imageType As String = imagePair.Key
                                    Dim imageStream As MemoryStream = imagePair.Value
                                    Dim anchor1 = New AnchorCell(.Columns(col), .Rows(row), True)
                                    Dim anchor2 = New AnchorCell(.Columns(col + 1), .Rows(row + 1), True)
                                    Dim picType As ExcelPictureFormat = getExcelPictureFormat(imageType)

                                    Dim pic = .Pictures.Add(imageStream, picType, anchor1, anchor2)
                                    lstInfoPictures.Add(New KeyValuePair(Of String, ExcelPicture)("htm", pic))

                                    colRightMostColumn = col
                                    col += 1
                                Next
                                If colRightMostColumn < col - 1 Then colRightMostColumn = col - 1 'AS/19203f==

                            End If
                            row += 1
                        Next
                    Next
                End If
            Next 'user

            'freeze top row
            .Panes = New WorksheetPanes(PanesState.Frozen, 0, 1, "A2", PanePosition.TopRight)

            'format column headers
            Dim cellRange As CellRange = .Cells.GetSubrangeAbsolute(rowColHeader, colUserName, rowColHeader, colRightMostColumn)
            cellRange.Style = style_ColumnHeaderBW()
            cellRange.Style.Borders.SetBorders(MultipleBorders.All, Color.Black, LineStyle.Thin)

            cellRange = .Cells.GetSubrangeAbsolute(rowColHeader, colCost, rowColHeader, colInfodoc - 1) 'AS/19203f
            cellRange.Style.Rotation = 90

            'format table body
            cellRange = .Cells.GetSubrangeAbsolute(rowColHeader + 1, colUserName, row, col)
            cellRange.Style = style_TableBodyNormal()

            cellRange = .Cells.GetSubrangeAbsolute(rowColHeader, colUserName, row, colRightMostColumn)
            cellRange.Style.Borders.SetBorders(MultipleBorders.All, Color.Black, LineStyle.Thin)

            cellRange = .Cells.GetSubrangeAbsolute(rowColHeader, colInfodoc + 1, rowColHeader, colRightMostColumn) 'AS/19203f
            cellRange.Merged = True 'AS/19203f

            'set cols widths and number format
            Dim colW As Integer
            .Columns(colUserName).AutoFit()
            .Columns(colAlternative).AutoFit()
            colW = CInt(.Columns(colAlternative).Width * 1.1)
            .Columns(colAlternative).Width = colW
            .Columns(colInfodoc).Width = colW
            .Columns(colAltPriority).Style.NumberFormat = "0.00%"

            For i As Integer = 1 To row
                .Rows(i).AutoFit()
            Next i
            For i As Integer = colInfodoc + 1 To colRightMostColumn
                .Columns(i).AutoFit()
            Next i

            AdjustImages(worksheet, lstInfoPictures) 'AS/19203j
        End With

        Return worksheet
    End Function

    Function AddAlternativesBarChart(Options As ReportGeneratorOptions) As ExcelWorksheet 'AS/18712z 'AS/19464b

        'get selected users
        Dim users As List(Of Integer) = SelectedUsersAndGroupsIDs

        Dim wrtNode As clsNode = PM.ActiveObjectives.GetNodeByID(WRTNodeID)

        If users.Count > 1 Then
            Dim sMsg As String = "You've selected more than one user. Please go to the Synthesize tab, select a single user or group and retry."
            MsgBox(sMsg)
            Return Nothing
        End If
        Dim userID As Integer = users(0)

        Dim worksheet = workbook.Worksheets.Add("Alternatives Chart")

        'get username
        Dim username As String = ""
        Select Case userID
            Case >= 0 'individual user
                username = PM.GetUserByID(userID).UserName
            Case Else 'group
                username = PM.CombinedGroups.GetCombinedGroupByUserID(userID).Name
        End Select

        'calc priorities 
        PM.StorageManager.Reader.LoadUserData(PM.GetUserByID(userID))

        Dim CalcTarget As clsCalculationTarget
        CalcTarget = New clsCalculationTarget(CalculationTargetType.cttCombinedGroup, PM.CombinedGroups.GetCombinedGroupByUserID(userID))
        PM.CalculationsManager.Calculate(CalcTarget, wrtNode, PM.ActiveHierarchy, PM.ActiveAltsHierarchy)

        'List of Alternatives
        Dim altH As clsHierarchy = PM.AltsHierarchy(PM.ActiveAltsHierarchy)
        worksheet.Cells("A1").Value = "Alternative"
        worksheet.Cells("B1").Value = "Priority"

        Dim chartNodes As List(Of clsNode) = New List(Of clsNode)
        Dim barColors As List(Of String) = New List(Of String)

        chartNodes = altH.TerminalNodes

        Dim rowTitle As Integer = 0
        Dim rowUsername As Integer = 1
        Dim rowDataTop As Integer = 2
        Dim colAltName As Integer = 0
        Dim colPriority As Integer = 1

        Dim row As Integer = rowUsername + 1 'row number 
        Dim col As Integer = 0 'col number

        Dim i As Integer = 1
        For Each alt As clsNode In chartNodes
            worksheet.Cells(row, colAltName).Value = alt.NodeName
            worksheet.Cells(row, colPriority).Value = alt.UnnormalizedPriority
            row += 1

            'get bars colors
            Dim sAltColor As String = ""
            Dim altColor As Long = CLng(PM.Attributes.GetAttributeValue(ATTRIBUTE_DEFAULT_BRUSH_COLOR_ID, alt.NodeGuidID))
            sAltColor = If(altColor > 0, LongToBrush(altColor), GetPaletteColor(CurrentPaletteID(PM), alt.NodeID, True))
            barColors.Add(sAltColor)
        Next

        ' Set header row and formatting.
        worksheet.Rows(0).Style.Font.Weight = ExcelFont.BoldWeight
        worksheet.Columns(0).Width = CInt(GemBox.Spreadsheet.LengthUnitConverter.Convert(3, GemBox.Spreadsheet.LengthUnit.Centimeter, GemBox.Spreadsheet.LengthUnit.ZeroCharacterWidth256thPart))
        worksheet.Columns(1).Style.NumberFormat = "0.00%"

        ' Make entire sheet print on a single page.
        worksheet.PrintOptions.FitWorksheetWidthToPages = 1
        worksheet.PrintOptions.FitWorksheetHeightToPages = 1

        ' Create Excel chart and select data for it.
        Dim chart = worksheet.Charts.Add(ChartType.Bar, "D" & rowDataTop, "M" & (rowDataTop + chartNodes.Count) * 2)
        chart.SelectData(worksheet.Cells.GetSubrangeAbsolute(rowDataTop - 1, colAltName, rowDataTop + chartNodes.Count - 1, colPriority), True)

        ' Set chart legend.
        chart.Legend.IsVisible = False 'True
        'chart.Legend.Position = ChartLegendPosition.Right

        ' Define colors
        Dim chartBackColor = DrawingColor.FromRgb(ecColors.ecGray9.R, ecColors.ecGray9.G, ecColors.ecGray9.B)
        Dim seriesColor = DrawingColor.FromName(DrawingColorName.Gold)
        Dim textColor = DrawingColor.FromName(DrawingColorName.Yellow)
        Dim borderColor = DrawingColor.FromName(DrawingColorName.Black)
        Dim plotareaColor = DrawingColor.FromName(DrawingColorName.White)
        Dim outlineColor = DrawingColor.FromName(DrawingColorName.Red)

        ' Format chart
        'chart.Fill.SetSolid(chartBackColor)

        '=============================================
        Dim barchart = CType(chart, BarChart)
        barchart.Fill.SetSolid(chartBackColor)
        barchart.Axes.Vertical.IsVisible = True 'False
        barchart.DataLabels.LabelPosition = DataLabelPosition.OutsideEnd
        'barchart.DataLabels.LabelContainsCategoryName = True
        barchart.DataLabels.LabelContainsValue = True
        barchart.Position.To.Column = worksheet.Columns("Q")
        barchart.SeriesGapWidth = 0.3

        Dim series = barchart.Series(0)
        series.Outline.Fill.SetNone()
        Dim dataPoints = series.DataPoints

        'apply colors to bars
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
        chart.PlotArea.Fill.SetSolid(plotareaColor)

        outline = chart.PlotArea.Outline
        outline.Width = Length.From(1.5, GemBox.Spreadsheet.LengthUnit.Point)
        outline.Fill.SetSolid(borderColor)

        ' Format chart title 
        chart.Title.Text = username
        Dim textFormat = chart.Title.TextFormat
        textFormat.Size = Length.From(20, GemBox.Spreadsheet.LengthUnit.Point)
        textFormat.Font = "Arial"
        'textFormat.Fill.SetSolid(textColor)

        ' Format vertical axis
        'textFormat = chart.Axes.Vertical.TextFormat
        'textFormat.Fill.SetSolid(textColor)
        'textFormat.Italic = True

        ' Format horizontal axis
        'textFormat = chart.Axes.Horizontal.TextFormat
        'textFormat.Fill.SetSolid(textColor)
        'textFormat.Size = Length.From(12, GemBox.Spreadsheet.LengthUnit.Point)
        'textFormat.Bold = True

        ' Format vertical major gridlines
        'chart.Axes.Vertical.MajorGridlines.Outline.Width = Length.From(0.5, GemBox.Spreadsheet.LengthUnit.Point)

        ' Format series
        'Dim series = chart.Series(0) 
        'outline = series.Outline
        'outline.Width = Length.From(3, GemBox.Spreadsheet.LengthUnit.Point)
        'outline.Fill.SetSolid(seriesColor)

        'series.Fill.SetSolid(seriesColor) 'AS/17533f=== bars color, legend icon color
        'outline.Fill.SetSolid(outlineColor) 'bars border color, legend icon border color

        'AS/17533f==

        ' Format series markers

        'series.Marker.MarkerType = MarkerType.Circle
        'series.Marker.Size = 10
        'series.Marker.Fill.SetSolid(textColor)
        'series.Marker.Outline.Fill.SetSolid(seriesColor)
        '==========================================================

        Return worksheet

    End Function

    Function AddAlternativesComponentsChart(Options As ReportGeneratorOptions) As ExcelWorksheet 'AS/18712z

        Dim worksheet = workbook.Worksheets.Add("Alternatives Components Chart")
        Dim UserID As Integer = -1 'All Participants

        'calc priorities for Goal for 'All Participants'
        Dim wrtNode As clsNode = PM.ActiveObjectives.GetNodeByID(1)
        Dim CalcTarget As clsCalculationTarget = PM.CalculationsManager.GetCalculationTargetByUserID(UserID)
        PM.CalculationsManager.Calculate(CalcTarget, wrtNode)

        'Get lists of alternatives and objectives
        Dim altH As clsHierarchy = PM.AltsHierarchy(PM.ActiveAltsHierarchy)

        Dim rowTitle As Integer = 0
        Dim rowColHeader As Integer = 1
        Dim colAltName As Integer = 0

        Dim numberOfComponents As Integer = 4
        Dim colComponent1 As Integer = 1 'AS_hardcoded===
        Dim colComponent2 As Integer = 2
        Dim colComponent3 As Integer = 3
        Dim colComponent4 As Integer = 4 'AS_hardcoded==

        Dim colPriorities As Integer = numberOfComponents + 1
        Dim random = New Random()

        Dim row As Integer = 2 'row number 
        Dim col As Integer = 0 'col number

        With worksheet
            'get user name and write title and columns headers
            Dim userName As String = "Priority For: "
            Select Case UserID
                Case -1 'combined 
                    userName = "All Participants" 'userName & "All Participants"
                Case >= 0 'individual user
                    userName = PM.GetUserByID(UserID).UserName'userName & PM.GetUserByID(UserID).UserName
                Case < -1 'group
                    userName = PM.CombinedGroups.GetCombinedGroupByUserID(UserID).Name 'userName & PM.CombinedGroups.GetCombinedGroupByUserID(UserID).Name
            End Select

            .Cells(rowTitle, colAltName).Value = "Alternatives Components Chart (" & userName & ")"
            .Cells(rowColHeader, colAltName).Value = "Alternative"
            .Cells(rowColHeader, colPriorities).Value = "Priority" 'userName

            'write values into the columns
            For Each alt As clsNode In altH.TerminalNodes
                .Cells(row, colAltName).Value = alt.NodeName

                .Cells(row, colComponent1).Value = random.Next(10, 999) / 100 'AS_hardcoded===
                .Cells(row, colComponent2).Value = random.Next(10, 999) / 100
                .Cells(row, colComponent3).Value = random.Next(10, 999) / 100
                .Cells(row, colComponent4).Value = random.Next(10, 999) / 100 'AS_hardcoded==

                .Cells(row, colPriorities).Value = alt.SAGlobalPriority

                row += 1
            Next

            Dim chart = worksheet.Charts.Add(Of BarChart)(ChartGrouping.Stacked, "H2", "T20")
            chart.SelectData("A3:F18", True, False, True)
            chart.Axes.Horizontal.IsVisible = False
            chart.DataLabels.LabelContainsValue = True
            chart.DataLabels.LabelPosition = DataLabelPosition.Center
            chart.SeriesGapWidth = 0.5

            'color components
            chart.Series(0).Fill.SetSolid(DrawingColor.FromRgb(102, 153, 102))
            chart.Series(1).Fill.SetSolid(DrawingColor.FromRgb(102, 153, 204))
            chart.Series(2).Fill.SetSolid(DrawingColor.FromRgb(204, 204, 102))
            chart.Series(3).Fill.SetSolid(DrawingColor.FromRgb(153, 204, 153))

            'set up priorities next to bars (actually, as another series with transparent background)
            chart.Series(4).Fill.SetSolid(DrawingColor.FromName(DrawingColorName.Transparent))
            chart.Series(4).IsLegendEntryVisible = False
            chart.Series(4).DataLabels.LabelPosition = DataLabelPosition.InsideBase
            chart.Series(4).DataLabels.TextFormat.Size = 11

            'apply formats/styles
            Dim cellRange As CellRange = .Cells.GetSubrangeAbsolute(rowTitle, colAltName, rowTitle, 21)
            cellRange.Merged = True
            cellRange.Style = style_TableTitle()

            cellRange = .Cells.GetSubrangeAbsolute(rowColHeader, colAltName, rowColHeader, colPriorities)
            cellRange.Style = style_ColumnHeader()

            cellRange = .Cells.GetSubrangeAbsolute(rowColHeader + 1, colAltName, row - 1, colPriorities)
            cellRange.Style = style_TableBodyNormal()

            .Cells(row + 2, colAltName).Value = "NOTE: components values are hardcoded for now"
            cellRange = .Cells.GetSubrangeAbsolute(row + 2, colAltName, row + 2, colPriorities)
            cellRange.Merged = True
            cellRange.Style.Font.Color = Color.DarkRed

            .Columns(colAltName).AutoFit()
            'Dim fSize As Double = .Cells(row, colAltName).Style.Font.Size / 20
            'Dim fName As String = .Cells(row, colAltName).Style.Font.Name
            'Dim colW As Double, colH As Double
            'getCellSize("Alternative", fName, colW, colH, CSng(fSize.ToString))
            '.Columns(colAltName).SetWidth(colW, LengthUnit.Point)

            .Columns(colPriorities).AutoFit()
            .Columns(colPriorities).Style.NumberFormat = "0.00%"

            ' Make entire sheet print horizontally centered on a single page with headings and gridlines.
            Dim printOptions = worksheet.PrintOptions
            printOptions.HorizontalCentered = True
            printOptions.PrintHeadings = True
            printOptions.PrintGridlines = True
            printOptions.FitWorksheetWidthToPages = 1
            printOptions.FitWorksheetHeightToPages = 1
        End With

        Return worksheet

    End Function

    Function AddObjectivesBarChart(Options As ReportGeneratorOptions) As ExcelWorksheet 'AS/19464a

        'get selected users
        Dim users As List(Of Integer) = SelectedUsersAndGroupsIDs

        Dim wrtNode As clsNode = PM.ActiveObjectives.GetNodeByID(WRTNodeID)

        If users.Count > 1 Then
            Dim sMsg As String = "You've selected more than one user. Please go to the Synthesize tab, select a single user or group and retry."
            MsgBox(sMsg)
            Return Nothing
        End If
        Dim userID As Integer = users(0)

        PM.StorageManager.Reader.LoadUserData(PM.GetUserByID(userID)) 'NEED!

        Dim worksheet = workbook.Worksheets.Add("Objectives Chart") ' moved down here (after the checks)

        'List of Objectives
        Dim objH As clsHierarchy = App.ActiveProject.HierarchyObjectives

        Dim rowTitle As Integer = 0
        Dim rowUsername As Integer = 1
        Dim rowDataTop As Integer = 2
        Dim colObjName As Integer = 0
        Dim colPriority As Integer = 1

        Dim row As Integer = rowUsername + 1 'row number 
        Dim col As Integer = 0 'col number
        Dim i As Integer = 1

        Dim chartNodes As List(Of clsNode) = New List(Of clsNode)
        Dim barColors As List(Of String) = New List(Of String)

        worksheet.Cells(rowTitle, colObjName).Value = "Objective"
        worksheet.Cells(rowTitle, colPriority).Value = "Priority"

        'calculate for the user
        Dim CalcTarget As clsCalculationTarget = PM.CalculationsManager.GetCalculationTargetByUserID(userID)
        PM.CalculationsManager.Calculate(CalcTarget, wrtNode)

        'get username
        Dim username As String = ""
        Select Case userID
            Case >= 0 'individual user
                username = PM.GetUserByID(userID).UserName
            Case Else 'group
                username = PM.CombinedGroups.GetCombinedGroupByUserID(userID).Name
        End Select
        worksheet.Cells(rowUsername, colObjName).Value = username

        chartNodes = wrtNode.GetNodesBelow(userID)
        For Each obj As clsNode In chartNodes
            worksheet.Cells(row, colObjName).Value = obj.NodeName
            worksheet.Cells(row, colPriority).Value = obj.UnnormalizedPriority
            row += 1

            'get bars colors
            Dim sObjColor As String = ""
            Dim objColor As Long = CLng(PM.Attributes.GetAttributeValue(ATTRIBUTE_DEFAULT_BRUSH_COLOR_ID, obj.NodeGuidID))
            sObjColor = If(objColor > 0, LongToBrush(objColor), GetPaletteColor(CurrentPaletteID(PM), obj.NodeID, True))
            barColors.Add(sObjColor)
        Next

        ' Set header row and formatting.
        worksheet.Rows(0).Style.Font.Weight = ExcelFont.BoldWeight
        worksheet.Columns(0).Width = CInt(GemBox.Spreadsheet.LengthUnitConverter.Convert(3, GemBox.Spreadsheet.LengthUnit.Centimeter, GemBox.Spreadsheet.LengthUnit.ZeroCharacterWidth256thPart))
        worksheet.Columns(1).Style.NumberFormat = "0.00%"

        ' Make entire sheet print on a single page.
        worksheet.PrintOptions.FitWorksheetWidthToPages = 1
        worksheet.PrintOptions.FitWorksheetHeightToPages = 1

        ' Create Excel chart and select data for it.
        Dim chart = worksheet.Charts.Add(ChartType.Bar, "D" & rowDataTop, "M" & (rowDataTop + chartNodes.Count) * 2)
        chart.SelectData(worksheet.Cells.GetSubrangeAbsolute(rowDataTop - 1, colObjName, rowDataTop + chartNodes.Count - 1, colPriority), True)

        ' Set chart legend.
        chart.Legend.IsVisible = False 'True
        'chart.Legend.Position = ChartLegendPosition.Right

        ' Define colors
        'Dim chartBackColor = DrawingColor.FromName(DrawingColorName.LightGreen)
        Dim chartBackColor = DrawingColor.FromRgb(ecColors.ecGray9.R, ecColors.ecGray9.G, ecColors.ecGray9.B)
        Dim seriesColor = DrawingColor.FromName(DrawingColorName.Gold)
        Dim textColor = DrawingColor.FromName(DrawingColorName.Yellow)
        Dim borderColor = DrawingColor.FromName(DrawingColorName.Black)
        Dim plotareaColor = DrawingColor.FromName(DrawingColorName.White)
        Dim outlineColor = DrawingColor.FromName(DrawingColorName.Red)

        ' Format chart
        'chart.Fill.SetSolid(chartBackColor)

        '=============================================
        Dim barchart = CType(chart, BarChart)
        barchart.Fill.SetSolid(chartBackColor)
        barchart.Axes.Vertical.IsVisible = True 'False
        barchart.DataLabels.LabelPosition = DataLabelPosition.OutsideEnd
        'barchart.DataLabels.LabelContainsCategoryName = True
        barchart.DataLabels.LabelContainsValue = True
        barchart.Position.To.Column = worksheet.Columns("Q")
        barchart.SeriesGapWidth = 0.3

        Dim series = barchart.Series(0)
        series.Outline.Fill.SetNone()
        Dim dataPoints = series.DataPoints

        'apply colors to the bars
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
        chart.PlotArea.Fill.SetSolid(plotareaColor)

        outline = chart.PlotArea.Outline
        outline.Width = Length.From(1.5, GemBox.Spreadsheet.LengthUnit.Point)
        outline.Fill.SetSolid(borderColor)

        ' Format chart title 
        chart.Title.Text = username
        Dim textFormat = chart.Title.TextFormat
        'textFormat.Bold = True
        textFormat.Size = Length.From(20, GemBox.Spreadsheet.LengthUnit.Point)
        textFormat.Font = "Arial"
        'textFormat.Fill.SetSolid(textColor)

        ' Format vertical axis
        'textFormat = chart.Axes.Vertical.TextFormat
        'textFormat.Fill.SetSolid(textColor)
        'textFormat.Italic = True

        ' Format horizontal axis
        'textFormat = chart.Axes.Horizontal.TextFormat
        'textFormat.Fill.SetSolid(textColor)
        'textFormat.Size = Length.From(12, GemBox.Spreadsheet.LengthUnit.Point)
        'textFormat.Bold = True

        ' Format vertical major gridlines
        'chart.Axes.Vertical.MajorGridlines.Outline.Width = Length.From(0.5, GemBox.Spreadsheet.LengthUnit.Point)

        ' Format series
        'Dim series = chart.Series(0) 
        'outline = series.Outline
        'outline.Width = Length.From(3, GemBox.Spreadsheet.LengthUnit.Point)
        'outline.Fill.SetSolid(seriesColor)

        'series.Fill.SetSolid(seriesColor) 'AS/17533f=== bars color, legend icon color
        'outline.Fill.SetSolid(outlineColor) 'bars border color, legend icon border color

        ' Format series markers
        'series.Marker.MarkerType = MarkerType.Circle
        'series.Marker.Size = 10
        'series.Marker.Fill.SetSolid(textColor)
        'series.Marker.Outline.Fill.SetSolid(seriesColor)
        '==========================================================

        Return worksheet

    End Function

    Function AddObjectivesBarChart_MultiUsers(Options As ReportGeneratorOptions) As ExcelWorksheet 'AS/19464a

        Dim worksheet = workbook.Worksheets.Add("Objectives Chart")

        'get selected users
        Dim users As List(Of Integer) = SelectedUsersAndGroupsIDs

        Dim wrtNode As clsNode = PM.ActiveObjectives.GetNodeByID(1) 'LATER - replace with SELECTED node (probably Public Property GridWRTNodeID As Integer)
        'PM.StorageManager.Reader.LoadUserData(PM.GetUserByID(userID)) 'NEED!

        'List of Objectives
        Dim objH As clsHierarchy = App.ActiveProject.HierarchyObjectives

        Dim rowTitle As Integer = 0
        Dim rowUsername As Integer = 1
        Dim colObjName As Integer = 0
        Dim colPriority As Integer = 1

        Dim firstTableRow As Integer = 1
        Dim lastTableRow As Integer = 1
        Dim firstChartRow As Integer = 1
        Dim lastChartRow As Integer = 1

        Dim row As Integer = rowUsername + 1 'row number 
        Dim col As Integer = 0 'col number
        Dim i As Integer = 1

        Dim chartNodes As List(Of clsNode) = New List(Of clsNode)
        Dim barColors As List(Of String) = New List(Of String)

        worksheet.Cells(rowTitle, colObjName).Value = "Objective"
        worksheet.Cells(rowTitle, colPriority).Value = "Priority"

        For Each userID In users
            'calculate for the user
            Dim CalcTarget As clsCalculationTarget = PM.CalculationsManager.GetCalculationTargetByUserID(userID)
            PM.CalculationsManager.Calculate(CalcTarget, wrtNode)

            'get username
            Dim username As String = ""
            Select Case userID
                Case >= 0 'individual user
                    username = PM.GetUserByID(userID).UserName
                Case Else 'group
                    username = PM.CombinedGroups.GetCombinedGroupByUserID(userID).Name
            End Select

            worksheet.Cells(rowUsername, colObjName).Value = username

            For Each obj As clsNode In objH.Nodes
                If Not obj.IsTerminalNode And obj.NodeID > 1 Then
                    worksheet.Cells(row, colObjName).Value = obj.NodeName
                    worksheet.Cells(row, colPriority).Value = obj.UnnormalizedPriority
                    row += 1

                    'get bars colors
                    Dim sAltColor As String = ""
                    Dim altColor As Long = CLng(PM.Attributes.GetAttributeValue(ATTRIBUTE_DEFAULT_BRUSH_COLOR_ID, obj.NodeGuidID))
                    sAltColor = If(altColor > 0, LongToBrush(altColor), GetPaletteColor(CurrentPaletteID(PM), obj.NodeID, True))
                    barColors.Add(sAltColor)
                    chartNodes.Add(obj)
                End If
            Next
            lastChartRow = firstTableRow + row

            ' Set header row and formatting.
            worksheet.Rows(0).Style.Font.Weight = ExcelFont.BoldWeight
            worksheet.Columns(0).Width = CInt(GemBox.Spreadsheet.LengthUnitConverter.Convert(3, GemBox.Spreadsheet.LengthUnit.Centimeter, GemBox.Spreadsheet.LengthUnit.ZeroCharacterWidth256thPart))
            worksheet.Columns(1).Style.NumberFormat = "0.00%"

            ' Make entire sheet print on a single page.
            worksheet.PrintOptions.FitWorksheetWidthToPages = 1
            worksheet.PrintOptions.FitWorksheetHeightToPages = 1

            ' Create Excel chart and select data for it.
            'Dim chart = worksheet.Charts.Add(ChartType.Bar, "D2", "M25") 
            Dim chart = worksheet.Charts.Add(ChartType.Bar, "D" & firstChartRow, "M" & lastChartRow)
            'Dim chart = worksheet.Charts.Add(ChartType.Bar, "D27", "M53") 
            chart.SelectData(worksheet.Cells.GetSubrangeAbsolute(firstTableRow, colObjName, lastChartRow, colPriority), True)

            ' Set chart legend.
            chart.Legend.IsVisible = False 'True
            'chart.Legend.Position = ChartLegendPosition.Right

            ' Define colors
            Dim chartBackColor = DrawingColor.FromName(DrawingColorName.LightGreen)
            Dim seriesColor = DrawingColor.FromName(DrawingColorName.Gold)
            Dim textColor = DrawingColor.FromName(DrawingColorName.Yellow)
            Dim borderColor = DrawingColor.FromName(DrawingColorName.Black)
            Dim plotareaColor = DrawingColor.FromName(DrawingColorName.White)
            Dim outlineColor = DrawingColor.FromName(DrawingColorName.Red)

            ' Format chart
            'chart.Fill.SetSolid(chartBackColor)

            '=============================================
            Dim barchart = CType(chart, BarChart)
            barchart.Fill.SetSolid(chartBackColor)
            barchart.Axes.Vertical.IsVisible = True 'False
            barchart.DataLabels.LabelPosition = DataLabelPosition.OutsideEnd
            'barchart.DataLabels.LabelContainsCategoryName = True
            barchart.DataLabels.LabelContainsValue = True
            barchart.Position.To.Column = worksheet.Columns("Q")
            barchart.SeriesGapWidth = 0.3

            Dim series = barchart.Series(0)
            series.Outline.Fill.SetNone()
            Dim dataPoints = series.DataPoints

            'apply colors to the bars
            'Dim count As Integer = series.Values.Cast(Of Object)().Count() 
            'For i = 0 To count - 1 
            '    Dim R = ColorTranslator.FromHtml(barColors(i)).R
            '    Dim G = ColorTranslator.FromHtml(barColors(i)).G
            '    Dim B = ColorTranslator.FromHtml(barColors(i)).B
            '    dataPoints(i).Fill.SetSolid(DrawingColor.FromRgb(R, G, B))
            'Next

            Dim outline = chart.Outline
            outline.Width = Length.From(2, GemBox.Spreadsheet.LengthUnit.Point)
            outline.Fill.SetSolid(borderColor)

            ' Format plot area
            chart.PlotArea.Fill.SetSolid(plotareaColor)

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
            'Dim series = chart.Series(0) 
            'outline = series.Outline
            'outline.Width = Length.From(3, GemBox.Spreadsheet.LengthUnit.Point)
            'outline.Fill.SetSolid(seriesColor)

            'series.Fill.SetSolid(seriesColor) 'AS/17533f=== bars color, legend icon color
            'outline.Fill.SetSolid(outlineColor) 'bars border color, legend icon border color

            'AS/17533f==

            ' Format series markers

            'series.Marker.MarkerType = MarkerType.Circle
            'series.Marker.Size = 10
            'series.Marker.Fill.SetSolid(textColor)
            'series.Marker.Outline.Fill.SetSolid(seriesColor)
            '==========================================================
            lastTableRow = row
            rowUsername = lastChartRow + 2
            firstTableRow = rowUsername + 1
            firstChartRow = firstTableRow
            row = firstTableRow

        Next

        Return worksheet

    End Function

    Function Example_ComponentsChart(Options As ReportGeneratorOptions) As ExcelWorksheet 'AS/18712z

        Dim worksheet = workbook.Worksheets.Add("Example_ComponentsChart")

        Dim numberOfYears As Integer = 5

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
            cell.SetValue(random.Next(0, 100))
            cell.Style.NumberFormat = "##.#0" '"""$""#,##0"
        Next

        ' Set header row And formatting.
        worksheet.Rows(0).Style.Font.Weight = ExcelFont.BoldWeight
        worksheet.Rows(0).Style.HorizontalAlignment = HorizontalAlignmentStyle.Center
        worksheet.Columns(0).Width = CInt(GemBox.Spreadsheet.LengthUnitConverter.Convert(3, GemBox.Spreadsheet.LengthUnit.Centimeter, GemBox.Spreadsheet.LengthUnit.ZeroCharacterWidth256thPart))

        '=========================================================
        Dim chart = worksheet.Charts.Add(Of BarChart)(ChartGrouping.Stacked, "I2", "U12")
        chart.SelectData("A2:F5", True, False, True)
        chart.Axes.Horizontal.IsVisible = False
        chart.DataLabels.LabelContainsValue = True
        chart.DataLabels.LabelPosition = DataLabelPosition.Center
        chart.SeriesGapWidth = 0.5

        chart.Series(0).Fill.SetSolid(DrawingColor.FromRgb(102, 153, 102))
        chart.Series(1).Fill.SetSolid(DrawingColor.FromRgb(102, 153, 204))
        chart.Series(2).Fill.SetSolid(DrawingColor.FromRgb(204, 204, 102))
        chart.Series(3).Fill.SetSolid(DrawingColor.FromRgb(153, 204, 153))
        'chart.Series(4).Fill.SetSolid(DrawingColor.FromRgb(153, 153, 51))

        chart.Series(4).Fill.SetSolid(DrawingColor.FromName(DrawingColorName.Transparent))
        chart.Series(4).IsLegendEntryVisible = False
        chart.Series(4).DataLabels.LabelPosition = DataLabelPosition.InsideBase
        chart.Series(4).DataLabels.TextFormat.Size = 12
        '===========================================================

        '' Create chart And select data for it.
        'Dim chart = worksheet.Charts.Add(Of ColumnChart)(ChartGrouping.Clustered, "B7", "O27")
        'chart.SelectData(worksheet.Cells.GetSubrangeAbsolute(0, 0, 4, numberOfYears))

        '' Set chart title.
        'chart.Title.Text = "Column Chart"

        '' Set chart legend.
        'chart.Legend.IsVisible = True
        'chart.Legend.Position = ChartLegendPosition.Right

        '' Set axis titles.
        'chart.Axes.Horizontal.Title.Text = "Years"
        'chart.Axes.Vertical.Title.Text = "Salaries"

        '' Set value axis scaling, units, gridlines and tick marks.
        'Dim valueAxis = chart.Axes.VerticalValue
        'valueAxis.Minimum = 0
        'valueAxis.Maximum = 6000
        'valueAxis.MajorUnit = 1000
        'valueAxis.MinorUnit = 500
        'valueAxis.MajorGridlines.IsVisible = True
        'valueAxis.MinorGridlines.IsVisible = True
        'valueAxis.MajorTickMarkType = TickMarkType.Outside
        'valueAxis.MinorTickMarkType = TickMarkType.Cross

        ' Make entire sheet print horizontally centered on a single page with headings and gridlines.
        Dim printOptions = worksheet.PrintOptions
        printOptions.HorizontalCentered = True
        printOptions.PrintHeadings = True
        printOptions.PrintGridlines = True
        printOptions.FitWorksheetWidthToPages = 1
        printOptions.FitWorksheetHeightToPages = 1

        Return worksheet

    End Function

    Function Example_CreateSamplePieChart(Options As ReportGeneratorOptions) As ExcelWorksheet 'AS/18712z

        Dim worksheet = workbook.Worksheets.Add("PieChartExample")

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
        Dim chartsheet = workbook.Worksheets.Add(SheetType.Chart, "PieChart")

        ' Create Excel chart and select data for it.
        ' You cannot set the size of the chart area when the chart is located on a chart sheet, it will snap to maximum size on the chart sheet.
        Dim chart = chartsheet.Charts.Add(ChartType.Pie, 0, 0, 10, 10, GemBox.Spreadsheet.LengthUnit.Centimeter)
        chart.SelectData(worksheet.Cells.GetSubrangeAbsolute(0, 0, 4, 1), True)

        ' Set chart legend.
        chart.Legend.IsVisible = True
        chart.Legend.Position = ChartLegendPosition.Right

        Return worksheet

    End Function

#End Region

#Region "Get Worksheets - Report tab"

    Private Function AddPriorityOfObjectivesWorksheet(Options As ReportGeneratorOptions) As ExcelWorksheet 'AS/18712e 

        Dim worksheet = workbook.Worksheets.Add("Priority of Objectives - ")

        Dim UserID As Integer = -1 'All Participants

        'calc priorities for Goal for 'All Participants'
        Dim wrtNode As clsNode = PM.ActiveObjectives.GetNodeByID(1)
        Dim CalcTarget As clsCalculationTarget = PM.CalculationsManager.GetCalculationTargetByUserID(UserID)
        PM.CalculationsManager.Calculate(CalcTarget, wrtNode, PM.ActiveHierarchy, PM.ActiveAltsHierarchy)

        'Get lists of alternatives and objectives
        Dim objH As clsHierarchy = App.ActiveProject.HierarchyObjectives

        Dim rowTitle As Integer = 0
        Dim rowColHeader As Integer = 1
        Dim colObjName As Integer = 0
        Dim colLocalPriority As Integer = 1
        Dim colGlobalPriority As Integer = 2
        Dim maxLenObjName As Integer = 10

        With worksheet
            'wrtite table tytle
            .Cells(rowTitle, colObjName).Value = "Priority Of Objectives"

            'write headers for the columns
            .Cells(rowColHeader, colObjName).Value = "Objectives"
            .Cells(rowColHeader, colLocalPriority).Value = "Local Priority"
            .Cells(rowColHeader, colGlobalPriority).Value = "Global Priority"

            Dim row As Integer = rowColHeader + 1 'start row number 
            Dim col As Integer = 0 'col number

            For Each obj As clsNode In objH.Nodes
                'write values into Altenatives and Total columns
                .Cells(row, colObjName).Value = obj.NodeName
                .Cells(row, colObjName).Style.Indent = obj.Level * 3

                If maxLenObjName < Len(obj.NodeName) Then maxLenObjName = Len(obj.NodeName)

                .Cells(row, colLocalPriority).Value = obj.SALocalPriority
                .Cells(row, colGlobalPriority).Value = obj.SAGlobalPriority

                row += 1
            Next

            'Apply formatting
            Dim cellRange As CellRange = .Cells.GetSubrangeAbsolute(rowTitle, colObjName, rowTitle, colGlobalPriority)
            cellRange.Merged = True
            cellRange.Style = style_TableTitle()

            ' shade alternate rows in a worksheet using 'Formula' based conditional formatting.
            cellRange = .Cells.GetSubrangeAbsolute(rowColHeader + 1, colObjName, row - 1, colGlobalPriority)
            ShadeAlternateRows(worksheet, cellRange)

            'Format header -- set up style with alignment, colors, and borders
            .Cells.GetSubrangeAbsolute(rowColHeader, colObjName, rowColHeader, colGlobalPriority).Style = style_ColumnHeader()

            .Columns(colObjName).Style.WrapText = True
            .Columns(colObjName).AutoFit()
            Dim colW As Integer = .Columns(colObjName).Width
            .Columns(colObjName).Width = CInt(colW * 1.05)
            .Columns(colLocalPriority).Style.NumberFormat = "0.00%"
            .Columns(colLocalPriority).AutoFit()
            .Columns(colGlobalPriority).Style.NumberFormat = "0.00%"
            .Columns(colGlobalPriority).AutoFit()


            ' Make entire sheet print on a single page.
            .PrintOptions.FitWorksheetWidthToPages = 1
            .PrintOptions.FitWorksheetHeightToPages = 1
        End With

        Return worksheet

    End Function

    Private Function AddPriorityOfObjectivesWorksheets(Options As ReportGeneratorOptions) As List(Of ExcelWorksheet) 'AS/18712w
        ' Private Function AddPriorityOfObjectivesWorksheet(Options As ReportGeneratorOptions) As ExcelWorksheet 'AS/18712e 'AS/18712w=== temp for testing table colors
        Dim worksheets As New List(Of ExcelWorksheet)
        Dim styles As New List(Of String)
        styles.Add("green")
        styles.Add("blue")
        styles.Add("orange")
        styles.Add("gray")


        For Each s As String In styles
            ColorScheme = s
            Dim worksheet = workbook.Worksheets.Add("Priority of Objectives - " & s) 'AS/18712w==

            Dim UserID As Integer = -1 'All Participants

            'calc priorities for Goal for 'All Participants'
            Dim wrtNode As clsNode = PM.ActiveObjectives.GetNodeByID(1)
            Dim CalcTarget As clsCalculationTarget = PM.CalculationsManager.GetCalculationTargetByUserID(UserID)
            PM.CalculationsManager.Calculate(CalcTarget, wrtNode, PM.ActiveHierarchy, PM.ActiveAltsHierarchy)

            'Get lists of alternatives and objectives
            Dim objH As clsHierarchy = App.ActiveProject.HierarchyObjectives
            'Dim altH As clsHierarchy = PM.AltsHierarchy(PM.ActiveAltsHierarchy)

            Dim rowTitle As Integer = 0
            Dim rowColHeader As Integer = 1
            Dim colObjName As Integer = 0
            Dim colLocalPriority As Integer = 1
            Dim colGlobalPriority As Integer = 2
            Dim maxLenObjName As Integer = 10

            With worksheet
                'wrtite table tytle
                .Cells(rowTitle, colObjName).Value = "Priority Of Objectives"

                'write headers for the columns
                .Cells(rowColHeader, colObjName).Value = "Objectives"
                .Cells(rowColHeader, colLocalPriority).Value = "Local Priority"
                .Cells(rowColHeader, colGlobalPriority).Value = "Global Priority"

                Dim row As Integer = rowColHeader + 1 'start row number 
                Dim col As Integer = 0 'col number

                For Each obj As clsNode In objH.Nodes
                    'write values into Altenatives and Total columns
                    .Cells(row, colObjName).Value = obj.NodeName
                    .Cells(row, colObjName).Style.Indent = obj.Level * 3

                    If maxLenObjName < Len(obj.NodeName) Then maxLenObjName = Len(obj.NodeName)

                    .Cells(row, colLocalPriority).Value = obj.SALocalPriority
                    .Cells(row, colGlobalPriority).Value = obj.SAGlobalPriority

                    row += 1
                Next

                'Apply formatting
                Dim cellRange As CellRange = .Cells.GetSubrangeAbsolute(rowTitle, colObjName, rowTitle, colGlobalPriority)
                cellRange.Merged = True
                cellRange.Style = style_TableTitle()

                'Format header -- set up style with alignment, colors, and borders
                .Cells.GetSubrangeAbsolute(rowColHeader, colObjName, rowColHeader, colGlobalPriority).Style = style_ColumnHeader()

                ' shade alternate rows in a worksheet using 'Formula' based conditional formatting.
                cellRange = .Cells.GetSubrangeAbsolute(rowColHeader + 1, colObjName, row - 1, colGlobalPriority)
                ShadeAlternateRows(worksheet, cellRange)

                .Columns(colObjName).Style.WrapText = True
                .Columns(colObjName).AutoFit()
                Dim colW As Integer = .Columns(colObjName).Width
                .Columns(colObjName).Width = CInt(colW * 1.05)
                .Columns(colLocalPriority).Style.NumberFormat = "0.00%"
                .Columns(colLocalPriority).AutoFit()
                .Columns(colGlobalPriority).Style.NumberFormat = "0.00%"
                .Columns(colGlobalPriority).AutoFit()


                ' Make entire sheet print on a single page.
                .PrintOptions.FitWorksheetWidthToPages = 1
                .PrintOptions.FitWorksheetHeightToPages = 1
            End With

        Next


        Return worksheets

    End Function

    Private Function AddOverallResultsWorksheet(Options As ReportGeneratorOptions) As ExcelWorksheet 'AS/18712d

        Dim worksheet = workbook.Worksheets.Add("Overall Results")
        Dim UserID As Integer = -1 'All Participants

        'calc priorities for Goal for 'All Participants'
        Dim wrtNode As clsNode = PM.ActiveObjectives.GetNodeByID(1)
        Dim CalcTarget As clsCalculationTarget = PM.CalculationsManager.GetCalculationTargetByUserID(UserID)
        PM.CalculationsManager.Calculate(CalcTarget, wrtNode, PM.ActiveHierarchy, PM.ActiveAltsHierarchy)

        'Get lists of alternatives and objectives
        Dim altH As clsHierarchy = PM.AltsHierarchy(PM.ActiveAltsHierarchy)

        Dim rowTitle As Integer = 0
        Dim rowColHeader As Integer = 2
        Dim colAltName As Integer = 0
        Dim colPriorities As Integer = 1
        Dim maxLenAltName As Integer = 10

        Dim row As Integer = rowColHeader + 1 'row number 
        Dim col As Integer = 0 'col number

        With worksheet
            'wrtite table title
            .Cells(rowTitle, colPriorities).Value = "Overall Results"

            'write headers for the columns
            .Cells(rowColHeader, colAltName).Value = "Alternatives"
            .Cells(rowColHeader, colPriorities).Value = "All Participants"

            For Each alt As clsNode In altH.TerminalNodes
                'write values into Altenatives and Total columns
                .Cells(row, colAltName).Value = alt.NodeName
                If maxLenAltName < Len(alt.NodeName) Then maxLenAltName = Len(alt.NodeName)

                .Cells(row, colPriorities).Value = alt.SAGlobalPriority

                row += 1
            Next

            'text format, alignment, colors, and borders
            Dim cellRange As CellRange = .Cells.GetSubrangeAbsolute(rowTitle, colAltName, rowTitle, colPriorities)
            cellRange.Merged = True
            cellRange.Style = style_TableTitle()

            cellRange = .Cells.GetSubrangeAbsolute(rowColHeader, colAltName, rowColHeader, colPriorities)
            cellRange.Style = style_ColumnHeader()

            cellRange = .Cells.GetSubrangeAbsolute(rowColHeader + 1, colAltName, row - 1, colPriorities)
            cellRange.Style = style_TableBodyNormal()
            ShadeAlternateRows(worksheet, cellRange)

            .Columns(colAltName).AutoFit()
            .Columns(colAltName).Style.WrapText = True
            .Columns(colPriorities).AutoFit()
            .Columns(colPriorities).Style.NumberFormat = "0.00%"

            ' Make entire sheet print on a single page.
            .PrintOptions.FitWorksheetWidthToPages = 1
            .PrintOptions.FitWorksheetHeightToPages = 1
        End With

        Return worksheet

    End Function

    Function AddDatagridWorksheets(Options As ReportGeneratorOptions, UserIDs As List(Of Integer)) As List(Of ExcelWorksheet)

        'UserID = -1001
        'UserID = -1002
        'UserID = 2
        'UserID = -1

        Dim worksheets As New List(Of ExcelWorksheet)

        For Each UserID As Integer In UserIDs 'AS/18712b enclosed
            'set sheet name 'AS/18712a===
            Dim sheetName As String = "Data Grid"
            Select Case UserID
                Case -1 'combined 
                    sheetName = sheetName & " (All Participants)"
                Case >= 0 'individual user
                    sheetName = sheetName & " (" & PM.GetUserByID(UserID).UserName & ")"
                Case < -1 'group
                    sheetName = sheetName & " (" & PM.CombinedGroups.GetCombinedGroupByUserID(UserID).Name & ")"
            End Select

            Dim worksheet = workbook.Worksheets.Add(sheetName)

            'calc priorities for Goal for UserID
            Dim wrtNode As clsNode = PM.ActiveObjectives.GetNodeByID(1)
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
            Dim rowColHeader As Integer = 2 'names of attributes and covering objectives
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

            With worksheet
                'write columns groups names
                .Cells(rowModelName, colAltName).Value = sheetName '"Model name: " & PM.ProjectName
                .Cells(rowColsGroups, colAltName).Value = "Alternatives"
                .Cells(rowColsGroups, colCost).Value = "Attributes"
                .Cells(rowColsGroups, colFirstCovObjective + 1).Value = objH.Nodes(0).NodeName

                'write headers for attribute columns
                .Cells(rowColHeader, colCost).Value = "Cost"
                .Cells(rowColHeader, colRisks).Value = "P.Failure"

                For Each alt As clsNode In altH.TerminalNodes
                    'write values into Altenatives and Total columns
                    .Cells(row, colAltName).Value = alt.NodeName
                    If maxLenAltName < Len(alt.NodeName) Then maxLenAltName = Len(alt.NodeName)

                    .Cells(row, colCost).Value = PM.ResourceAligner.Scenarios.ActiveScenario.AlternativesFull.Find(Function(a) (a.ID.ToLower = alt.NodeGuidID.ToString.ToLower)).Cost 'AS/18712c
                    .Cells(row, colRisks).Value = PM.ResourceAligner.Scenarios.ActiveScenario.AlternativesFull.Find(Function(a) (a.ID.ToLower = alt.NodeGuidID.ToString.ToLower)).RiskOriginal 'AS/18712c

                    col = colRisks + 1 'AS/18712a

                    'define attributes columns and write values into them
                    For Each attr As clsAttribute In customAttributes
                        .Cells.Item(rowColHeader, col).Value = attr.Name
                        Select Case attr.ValueType'AS/18387b===
                            Case AttributeValueTypes.avtEnumeration, AttributeValueTypes.avtEnumerationMulti
                                Dim aEnum As clsAttributeEnumeration = PM.Attributes.GetEnumByID(attr.EnumID) '(mimiced the piece from Public Function AddEnumAttributeItem in CWSw\CoreWS_OperationContracts.vb)
                                If Not (aEnum Is Nothing OrElse attr.EnumID.Equals(Guid.Empty)) Then
                                    Dim itemGUID As String = (PM.Attributes.GetAttributeValue(attr.ID, alt.NodeGuidID)).ToString
                                    Dim val As String = aEnum.GetItemByID(New Guid(itemGUID)).Value
                                    .Cells.Item(row, col).Value = val
                                    If maxLenAttrName < Len(val) Then maxLenAttrName = Len(val)
                                End If
                            Case Else 'AS/18387b==
                                Dim val As Object = PM.Attributes.GetAttributeValue(attr.ID, alt.NodeGuidID)
                                If IsNothing(val) Then
                                    .Cells.Item(row, col).Value = String.Empty
                                Else
                                    .Cells.Item(row, col).Value = val.ToString
                                    If maxLenAttrName < Len(val.ToString) Then maxLenAttrName = Len(val.ToString)
                                End If
                        End Select

                        col = col + 1
                    Next

                    'insert Total col
                    .Cells(rowColHeader, colTotal).Value = "Total"
                    .Cells(row, colTotal).Value = alt.UnnormalizedPriority 'AS/18712a
                    col = colTotal + 1 'AS/18712a

                    'define data columns and write values into them
                    For Each node As clsNode In objH.Nodes
                        If node.IsTerminalNode Then '=====================
                            Dim sValue As String = " "
                            Select Case node.MeasureType
                                Case ECMeasureType.mtRatings
                                    Dim currJ As clsRatingMeasureData = CType(GetJudgment(node, alt.NodeID, UserID), clsRatingMeasureData)
                                    If Not IsNothing(currJ) Then ''AS/18712x replaced Try with If
                                        If Not IsNothing(currJ.Rating) Then
                                            If Not IsNothing(currJ.Rating.RatingScale) Then
                                                sValue = currJ.Rating.Name
                                            Else
                                                sValue = JS_SafeNumber(Math.Round(currJ.Rating.Value, Options.DatagridNumberOfDecimals)) 'AS/17533u inserted Rouns function
                                            End If
                                        End If
                                    End If

                                Case ECMeasureType.mtDirect, ECMeasureType.mtStep, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtCustomUtilityCurve
                                    Dim currJ As clsNonPairwiseMeasureData = GetJudgment(node, alt.NodeID, UserID)
                                    If Not IsNothing(currJ) Then ''AS/18712x replaced Try with If
                                        Dim aVal As Double = CDbl(currJ.SingleValue)
                                        If aVal = Int32.MinValue Or aVal.Equals(Double.NaN) Then
                                            sValue = " "
                                        Else
                                            sValue = JS_SafeNumber(Math.Round(aVal, Options.DatagridNumberOfDecimals)) 'AS/17533u inserted Rouns function
                                        End If
                                    End If

                                Case Else 'for PW 
                                    Dim aVal As Double = node.Judgments.Weights.GetUserWeights(UserID, ECSynthesisMode.smIdeal, PM.CalculationsManager.IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(alt.NodeID)
                                    aVal = Math.Round(aVal, Options.DatagridNumberOfDecimals) 'AS/17533u
                                    sValue = aVal.ToString
                            End Select

                            'data columns headers and values
                            .Cells.Item(rowColHeader, col).Value = node.NodeName
                            .Cells.Item(row, col).Value = sValue

                            col = col + 1

                        End If
                    Next '======================

                    row += 1
                Next
                colLastCovObjective = col - 1 'AS/18712y

                'merge cells and apply styles -- layout, colors, fonts, borders, etc.'AS/18712y===
                Dim range As CellRange = .Cells.GetSubrangeAbsolute(rowModelName, colAltName, rowModelName, colLastCovObjective) 'model name
                range.Merged = True
                range.Style = style_TableTitle()

                range = .Cells.GetSubrangeAbsolute(rowColsGroups, colCost, rowColsGroups, colTotal) 'attributes group
                range.Merged = True
                range.Style = style_ColumnHeader()

                range = .Cells.GetSubrangeAbsolute(rowColsGroups, colFirstCovObjective, rowColsGroups, colLastCovObjective) 'cov objectives group
                range.Merged = True
                range.Style = style_ColumnHeader()

                range = .Cells.GetSubrangeAbsolute(rowColsGroups, colAltName, rowColHeader, colAltName) 'alt col name
                range.Merged = True
                range.Style = style_ColumnHeader()

                range = .Cells.GetSubrangeAbsolute(rowColsGroups, colAltName, rowColHeader, colLastCovObjective)
                range.Style = style_ColumnHeader()
                range.Style.Borders.SetBorders(MultipleBorders.All, Color.White, LineStyle.Thick)

                ' shade alternate rows in a worksheet using 'Formula' based conditional formatting.
                range = .Cells.GetSubrangeAbsolute(rowColHeader + 1, colAltName, row - 1, colLastCovObjective)
                ShadeAlternateRows(worksheet, range)

                .Cells.GetSubrangeAbsolute(rowColHeader, colCost, rowColHeader, colTotal - 1).Style.Rotation = 90

                '.Columns(colAltName).SetWidth(maxLenAltName, LengthUnit.ZeroCharacterWidth)
                '.Rows(rowColHeader).AutoFit()
                .Rows(rowColHeader).SetHeight(maxLenAttrName * 2, LengthUnit.ZeroCharacterWidth)
                .Columns(colAltName).AutoFit()
                .Columns(colAltName).Style.WrapText = True

                ' Make entire sheet print on a single page.
                .PrintOptions.FitWorksheetWidthToPages = 1
                .PrintOptions.FitWorksheetHeightToPages = 1

                worksheets.Add(worksheet)
            End With

        Next 'AS/18712b

        Return worksheets

    End Function

    Private Function AddObjAltsPrioritiesWorksheet(Options As ReportGeneratorOptions) As ExcelWorksheet 'AS/18712f
        Dim worksheet = workbook.Worksheets.Add("Obj-Alts Priorities")

        Dim UserID As Integer = -1 'All Participants

        'calc priorities for Goal for 'All Participants'
        Dim wrtNode As clsNode = PM.ActiveObjectives.GetNodeByID(1)
        Dim CalcTarget As clsCalculationTarget = PM.CalculationsManager.GetCalculationTargetByUserID(UserID)
        PM.CalculationsManager.Calculate(CalcTarget, wrtNode, PM.ActiveHierarchy, PM.ActiveAltsHierarchy)

        'Get lists of alternatives and objectives
        Dim objH As clsHierarchy = App.ActiveProject.HierarchyObjectives
        Dim altH As clsHierarchy = PM.AltsHierarchy(PM.ActiveAltsHierarchy)

        Dim rowTitle As Integer = 0
        Dim rowColHeader As Integer = 2
        Dim colObjAltName As Integer = 0
        Dim colAltPriority As Integer = 1
        Dim maxLenObjName As Integer = 10
        Dim maxLenAltName As Integer = 10

        Dim row As Integer = rowColHeader + 1 'row number 
        Dim col As Integer = 0 'col number

        With worksheet
            'wrtite table title
            .Cells(rowTitle, colObjAltName).Value = "Objectives/Alternatives Priorities"

            'write headers for the columns
            .Cells(rowColHeader, colObjAltName).Value = "Objective/Alternative"
            .Cells(rowColHeader, colAltPriority).Value = "Alternative Priority"

            For Each node As clsNode In objH.Nodes
                'write objectives names and values into the columns
                .Cells(row, colObjAltName).Value = node.NodeName & " [L:" & Format(TEMP_SINGLE_VALUE, "0.00%") & "] [G:" & Format(node.SAGlobalPriority, "0.00%") & "]"
                .Cells(row, colObjAltName).Style.Indent = node.Level * 3

                If maxLenObjName < Len(node.NodeName) Then maxLenObjName = Len(node.NodeName)
                row += 1
                If node.IsTerminalNode Then
                    For Each alt As clsNode In altH.TerminalNodes
                        Dim sValue As String = " "
                        Select Case node.MeasureType
                            Case ECMeasureType.mtRatings
                                Dim currJ As clsRatingMeasureData = CType(GetJudgment(node, alt.NodeID, UserID), clsRatingMeasureData)

                                If Not IsNothing(currJ) Then ''AS/18712x replaced Try with If
                                    If Not IsNothing(currJ.Rating) Then
                                        If Not IsNothing(currJ.Rating.RatingScale) Then
                                            sValue = currJ.Rating.Name
                                        Else
                                            sValue = JS_SafeNumber(currJ.Rating.Value)
                                        End If
                                    End If
                                End If

                            Case ECMeasureType.mtDirect, ECMeasureType.mtStep, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtCustomUtilityCurve
                                Dim currJ As clsNonPairwiseMeasureData = GetJudgment(node, alt.NodeID, UserID)
                                If Not IsNothing(currJ) Then ''AS/18712x replaced Try with If
                                    Dim aVal As Double = CDbl(currJ.SingleValue)
                                    If aVal = Int32.MinValue Or aVal.Equals(Double.NaN) Then
                                        sValue = " "
                                    Else
                                        sValue = JS_SafeNumber(aVal)
                                    End If
                                End If

                            Case Else 'for PW 
                                Dim aVal As Double = node.Judgments.Weights.GetUserWeights(UserID, ECSynthesisMode.smIdeal, PM.CalculationsManager.IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(alt.NodeID)
                                sValue = aVal.ToString
                        End Select

                        'write alternatives values into the columns - alts names and priorities
                        .Cells(row, colObjAltName).Value = alt.NodeName
                        If maxLenAltName < Len(alt.NodeName) Then maxLenAltName = Len(alt.NodeName)

                        If IsNumeric(sValue) Then
                            .Cells(row, colAltPriority).Value = CSng(sValue)
                        End If
                        .Cells(row, colObjAltName).Style.Indent = (node.Level + 1) * 3
                        row += 1
                    Next
                End If
            Next

            'Format -- set up style with alignment, colors, and borders
            Dim cellRange As CellRange = .Cells.GetSubrangeAbsolute(rowTitle, colObjAltName, rowTitle, colAltPriority)
            cellRange.Merged = True
            cellRange.Style = style_TableTitle()

            cellRange = .Cells.GetSubrangeAbsolute(rowColHeader, colObjAltName, rowColHeader, colAltPriority)
            cellRange.Style = style_ColumnHeader()

            cellRange = .Cells.GetSubrangeAbsolute(rowColHeader + 1, colObjAltName, row - 1, colAltPriority)
            cellRange.Style.Font.Size = 9 * 20 'style_TableBodyNormal() -- cannot apply the style since it overwrites the indents, so just set the font here
            ShadeAlternateRows(worksheet, cellRange)

            .Columns(colObjAltName).AutoFit()
            .Columns(colObjAltName).Style.WrapText = True
            .Columns(colAltPriority).AutoFit()
            .Columns(colAltPriority).Style.NumberFormat = "0.00%"

            ' Make entire sheet print on a single page.
            .PrintOptions.FitWorksheetWidthToPages = 1
            .PrintOptions.FitWorksheetHeightToPages = 1
        End With

        Return worksheet
    End Function

    Private Function AddObjectivesWorksheet(Options As ReportGeneratorOptions) As ExcelWorksheet 'AS/18712g
        Dim worksheet = workbook.Worksheets.Add("Objectives")

        'Get lists of alternatives and objectives
        Dim objH As clsHierarchy = App.ActiveProject.HierarchyObjectives

        Dim row As Integer = 2 'row number 
        Dim col As Integer = 0 'col number

        Dim rowTitle As Integer = 0
        Dim colObjName As Integer = 0
        Dim maxLenObjName As Integer = 10
        Dim colW As Double ', rowH As Double

        With worksheet
            'write headers for the columns
            .Cells(rowTitle, colObjName).Value = "Objective"
            .Columns(colObjName).Style.Font.Name = "Arial"
            .Columns(colObjName).Style.Font.Color = Color.Black
            Dim drawCellBorder As Boolean

            For Each node As clsNode In objH.Nodes
                'write objectives names and values into the columns
                .Cells(row, colObjName).Value = node.NodeName
                .Cells(row, colObjName).Style = style_TableBodyBold()
                '.Cells(row, colObjName).Style.Font.Size = 12 * 20 'AS/18712y
                '.Cells(row, colObjName).Style.Font.Weight = ExcelFont.BoldWeight 'AS/18712y

                .Cells(row, colObjName).Style.Indent = node.Level * 3
                If drawCellBorder Then
                    .Cells(row, colObjName).Style.Borders.SetBorders(MultipleBorders.Top, Color.Black, LineStyle.Thin)
                    drawCellBorder = False
                End If

                If maxLenObjName < Len(node.NodeName) Then maxLenObjName = Len(node.NodeName)

                Dim fSize As Double = .Cells(row, colObjName).Style.Font.Size / 20
                Dim fName As String = .Columns(colObjName).Style.Font.Name

                Dim newW As Double, newH As Double
                getCellSize(node.NodeName, fName, newW, newH, CSng(fSize.ToString))
                If colW < newW Then colW = newW

                row += 1

                If CBool(1) Then 'Options.IncludeObjectivesDescriptions Then
                    Dim sText As String = Infodoc2Text(App.ActiveProject, node.InfoDoc)
                    If sText <> "" Then
                        .Cells(row, colObjName).Value = sText
                        '.Cells(row, colObjName).Style = style_TableBodyBold() 'AS/18712y===
                        '.Cells(row, colObjName).Style.Font.Size = 9 * 20
                        '.Cells(row, colObjName).Style.Font.Weight = ExcelFont.NormalWeight 'AS/18712y==
                        .Cells(row, colObjName).Style = style_TableBodyNormal() 'AS/18712y
                        .Cells(row, colObjName).Style.Indent = node.Level * 3
                        .Cells(row, colObjName).Style.Borders.SetBorders(MultipleBorders.All, Color.Black, LineStyle.Thin)

                        drawCellBorder = True
                        row += 1
                    End If
                End If
            Next

            'format
            .Cells(rowTitle, colObjName).Style = style_TableTitle() 'AS/18712y

            '.Columns(colObjName).AutoFit()
            .Columns(colObjName).SetWidth(colW, LengthUnit.Point)
            .Columns(colObjName).Style.WrapText = True

            ' Make entire sheet print on a single page.
            '.PrintOptions.FitWorksheetWidthToPages = 1
            '.PrintOptions.FitWorksheetHeightToPages = 1
        End With

        Return worksheet
    End Function

    Private Function AddAlternativesWorksheet(Options As ReportGeneratorOptions) As ExcelWorksheet 'AS/18712f
        Dim worksheet = workbook.Worksheets.Add("Alternatives")

        'Get lists of alternatives
        Dim altH As clsHierarchy = PM.AltsHierarchy(PM.ActiveAltsHierarchy)

        Dim rowTitle As Integer = 0
        Dim rowColHeader As Integer = 1
        Dim colAltName As Integer = 0
        Dim maxLenAltName As Integer = 10
        Dim row As Integer = rowColHeader 'row number 

        With worksheet
            'write and format worksheet title
            .Cells(rowTitle, colAltName).Value = "Alternatives"
            .Cells(rowTitle, colAltName).Style = style_TableTitle()

            'write and format alts names
            For Each alt As clsNode In altH.TerminalNodes
                .Cells(row, colAltName).Value = alt.NodeName
                .Cells(row, colAltName).Style = style_TableBodyBold()
                If maxLenAltName < Len(alt.NodeName) Then maxLenAltName = Len(alt.NodeName)
                row += 1
            Next

            'adjust column width
            .Columns(colAltName).AutoFit()
            Dim W As Integer = .Columns(colAltName).Width
            .Columns(colAltName).Width = W * 2

            ' Make entire sheet print on a single page.
            .PrintOptions.FitWorksheetWidthToPages = 1
            .PrintOptions.FitWorksheetHeightToPages = 1
        End With

        Return worksheet
    End Function

    Private Function AddInconsistencyWorksheet(Options As ReportGeneratorOptions) As ExcelWorksheet 'AS/18712h

        Dim worksheet = workbook.Worksheets.Add("Inconsistencies")


        'Get lists of alternatives and objectives
        Dim objH As clsHierarchy = App.ActiveProject.HierarchyObjectives

        Dim rowTitle As Integer = 0
        Dim rowColHeaders As Integer = 1
        Dim colUserName As Integer = 0
        Dim colExamine As Integer = 1
        Dim colObjective As Integer = 2
        Dim colPath As Integer = 3
        Dim colInconsistency As Integer = 4
        Dim colNumOfChildren As Integer = 5

        Dim maxLenObjName As Integer = 10
        Dim colW As Double

        Dim row As Integer = 2 'row number 
        Dim col As Integer = 0 'col number

        With worksheet
            'title
            .Cells(rowTitle, colUserName).Value = "Inconsistencies"

            'write headers for the columns
            .Cells(rowColHeaders, colUserName).Value = "Name"
            .Cells(rowColHeaders, colExamine).Value = "Examine"
            .Cells(rowColHeaders, colObjective).Value = "Objective"
            .Cells(rowColHeaders, colPath).Value = "Path"
            .Cells(rowColHeaders, colInconsistency).Value = "Inconsistency"
            .Cells(rowColHeaders, colNumOfChildren).Value = "Number of Children"

            '====================================================== mimiced from clsProjectDataProvider.AddInconsistencyTable
            If objH.Nodes.Count > 0 Then
                For Each UserID As Integer In UserIDs
                    Dim u As clsUser = PM.GetUserByID(UserID)
                    For Each node As clsNode In objH.Nodes
                        If node.MeasureType = ECMeasureType.mtPairwise AndAlso node.IsAllowed(u.UserID) Then
                            If CType(node.Judgments, clsPairwiseJudgments).HasSpanningSet(u.UserID) Then
                                node.CalculateLocal(u.UserID)

                                Dim NodePath As String = ""
                                GetFullNodePath(node, NodePath)
                                .Cells(row, colUserName).Value = u.UserName
                                .Cells(row, colExamine).Value = u.UserEMail
                                .Cells(row, colObjective).Value = node.NodeName
                                If maxLenObjName < Len(node.NodeName) Then maxLenObjName = Len(node.NodeName)
                                .Cells(row, colPath).Value = NodePath
                                .Cells(row, colInconsistency).Value = CType(node.Judgments, clsPairwiseJudgments).EigenCalcs.InconIndex
                                .Cells(row, colNumOfChildren).Value = node.GetNodesBelow(u.UserID).Count
                                row += 1
                            End If
                        End If
                    Next
                Next
            End If
            '===============================

            'format title
            Dim cellRange = .Cells.GetSubrangeAbsolute(rowTitle, colUserName, rowTitle, colNumOfChildren)
            cellRange.Style = style_TableTitle()
            cellRange.Merged = True

            'Format header -- set up style with alignment, colors, and borders
            cellRange = .Cells.GetSubrangeAbsolute(rowColHeaders, colUserName, rowColHeaders, colNumOfChildren)
            cellRange.Style = style_ColumnHeader()
            'cellRange.Style.FillPattern.SetSolid(Color.Gray)
            'cellRange.Style.WrapText = True

            'Format body
            cellRange = .Cells.GetSubrangeAbsolute(rowColHeaders + 1, colUserName, row - 1, colNumOfChildren)
            cellRange.Style = style_TableBodyNormal()

            ' shade alternate rows in a worksheet using 'Formula' based conditional formatting.
            ShadeAlternateRows(worksheet, cellRange)

            .Columns(colUserName).AutoFit()
            .Columns(colObjective).AutoFit()
            .Columns(colExamine).AutoFit()
            .Columns(colInconsistency).AutoFit()
            .Columns(colNumOfChildren).AutoFit()

            colW = .Columns(colObjective).Width
            .Columns(colPath).Width = CInt(colW * 1.1)
        End With

        Return worksheet
    End Function

    Private Function AddJudgmentsOfAlternativesWorksheet(Options As ReportGeneratorOptions) As ExcelWorksheet 'AS/18712i

        Dim worksheet = workbook.Worksheets.Add("Judgments Of Alternatives")

        'Get lists of alternatives and objectives
        Dim objH As clsHierarchy = App.ActiveProject.HierarchyObjectives
        Dim altH As clsHierarchy = PM.AltsHierarchy(PM.ActiveAltsHierarchy)


        'Dim rowPath As New List(Of Integer)
        'Dim maxLevel As Integer = objH.GetMaxLevel
        'For level As Integer = 0 To maxLevel
        '    rowPath.Add(level)
        'Next
        'Dim rowUserName As Integer = rowPath.Count + 1

        Dim rowTitle As Integer = 0
        Dim colAltName As Integer = 0
        Dim colAltPriority As Integer = 1
        Dim colAltJudgment As Integer = 2
        Dim maxLenObjName As Integer = 10
        Dim maxLenAltName As Integer = 10

        Dim row As Integer = 2 'rowUserName + 1 'row number 
        Dim col As Integer = 0 'col number

        With worksheet
            'wrtite table tytle
            .Cells(rowTitle, colAltName).Value = "Judgments Of Alternatives"
            Dim cellRange As CellRange = .Cells.GetSubrangeAbsolute(rowTitle, colAltName, rowTitle, colAltJudgment)
            cellRange.Merged = True
            cellRange.Style = style_TableTitle()

            For Each node As clsNode In objH.Nodes
                ' rows with objectives names
                Dim NodePath As String = ""
                GetFullNodePath(node, NodePath)
                '.Cells(row, colAltName).Value = NodePath 'AS/5-6-20===
                'cellRange = .Cells.GetSubrangeAbsolute(row, colAltName, row, colAltJudgment)
                'cellRange.Merged = True
                'cellRange.Style = style_ColumnHeader()

                Dim indentVal As Integer = 0
                Dim s As String() = NodePath.Split("|"c)
                For Each nodeName As String In s
                    .Cells(row, colAltName).Value = nodeName.Trim
                    cellRange = .Cells.GetSubrangeAbsolute(row, colAltName, row, colAltJudgment)
                    cellRange.Merged = True
                    cellRange.Style = style_ColumnHeader(True)
                    .Cells(row, colAltName).Style.Indent = indentVal * 3
                    indentVal += 1
                    row += 1
                Next 'wed==

                If maxLenObjName < Len(node.NodeName) Then maxLenObjName = Len(node.NodeName)
                row += 1 '2 'AS/5-6-20==

                If node.IsTerminalNode Then
                    For Each UserID As Integer In UserIDs
                        Dim u As clsUser = PM.GetUserByID(UserID)

                        .Cells(row, colAltName).Value = u.UserName
                        cellRange = .Cells.GetSubrangeAbsolute(row, colAltName, row, colAltJudgment)
                        cellRange.Merged = True
                        cellRange.Style = style_RowsGroupName()

                        row += 1
                        For Each alt As clsNode In altH.TerminalNodes
                            Dim stringValue As String = ""
                            Dim singleValue As Single = UNDEFINED_SINGLE_VALUE
                            Select Case node.MeasureType
                                Case ECMeasureType.mtRatings
                                    Dim currJ As clsRatingMeasureData = CType(GetJudgment(node, alt.NodeID, UserID), clsRatingMeasureData)
                                    If Not IsNothing(currJ) Then 'AS/18712x===
                                        If Not IsNothing(currJ.Rating) Then
                                            If Not IsNothing(currJ.Rating.RatingScale) Then
                                                stringValue = currJ.Rating.Name
                                                singleValue = currJ.SingleValue
                                            Else
                                                stringValue = JS_SafeNumber(currJ.Rating.Value)
                                            End If
                                        Else
                                            stringValue = " "
                                        End If
                                    Else
                                        stringValue = " "
                                    End If'AS/18712x===

                                Case ECMeasureType.mtDirect, ECMeasureType.mtStep, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtCustomUtilityCurve
                                    Dim currJ As clsNonPairwiseMeasureData = GetJudgment(node, alt.NodeID, UserID)
                                    If Not IsNothing(currJ) Then 'AS/18712x===
                                        Dim aVal As Double = CDbl(currJ.SingleValue)
                                        If aVal = Int32.MinValue Or aVal.Equals(Double.NaN) Then
                                            stringValue = " "
                                        Else
                                            stringValue = JS_SafeNumber(aVal)
                                            singleValue = currJ.SingleValue
                                        End If
                                    Else
                                        stringValue = " "
                                    End If 'AS/18712x==

                                Case Else 'for PW 
                                    Dim aVal As Double = node.Judgments.Weights.GetUserWeights(UserID, ECSynthesisMode.smIdeal, PM.CalculationsManager.IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(alt.NodeID)
                                    stringValue = aVal.ToString
                                    singleValue = TEMP_SINGLE_VALUE 'CSng(aVal)
                            End Select

                            If singleValue <> UNDEFINED_SINGLE_VALUE Then 'AS/18712ia enclosed
                                'write alternatives values into the columns - alts names and priorities
                                .Cells(row, colAltName).Value = alt.NodeName
                                If maxLenAltName < Len(alt.NodeName) Then maxLenAltName = Len(alt.NodeName)

                                .Cells(row, colAltPriority).Value = stringValue
                                .Cells(row, colAltJudgment).Value = singleValue
                                row += 1
                            End If
                        Next 'next alt
                    Next 'next user
                End If
            Next 'next node (objective)

            .Columns(colAltName).AutoFit()
            .Columns(colAltName).Style.WrapText = True

            .Rows(3).AutoFit() 'AS/18712ya===
            .Rows(4).AutoFit()
            .Rows(5).AutoFit()
            .Rows(6).AutoFit()
            .Rows(7).AutoFit()
            .Rows(8).AutoFit()
            .Rows(9).AutoFit()
            .Rows(10).AutoFit() 'AS/18712ya==

            .Columns(colAltPriority).AutoFit()
            .Columns(colAltJudgment).Style.NumberFormat = "0.00"

            '' Make entire sheet print on a single page.
            '.PrintOptions.FitWorksheetWidthToPages = 1
            '.PrintOptions.FitWorksheetHeightToPages = 1
        End With

        Return worksheet
    End Function

    Private Function AddJudgmentsOfObjectivesWorksheet(Options As ReportGeneratorOptions, Optional viewChart As Boolean = False) As ExcelWorksheet 'AS/18712j
        Dim worksheet = workbook.Worksheets.Add("Judgments Of Objectives")

        'Get lists of alternatives and objectives
        Dim objH As clsHierarchy = App.ActiveProject.HierarchyObjectives

        Dim rowTitle As Integer = 0

        Dim rowPath As New List(Of Integer)
        Dim maxLevel As Integer = objH.GetMaxLevel
        For level As Integer = 0 To maxLevel
            rowPath.Add(level)
        Next

        Dim rowUserName As Integer = rowPath.Count + 1

        Dim colObjNameLeft As Integer = 0
        Dim colBarL1 As Integer = 1 'cols from 1 to 9 to use for blue bars
        Dim colBarL2 As Integer = 2
        Dim colBarL3 As Integer = 3
        Dim colBarL4 As Integer = 4
        Dim colBarL5 As Integer = 5
        Dim colBarL6 As Integer = 6
        Dim colBarL7 As Integer = 7
        Dim colBarL8 As Integer = 8
        Dim colBarL9 As Integer = 9
        Dim colObjJudgment As Integer = 10 '1 '10
        Dim colBarR1 As Integer = 11 'cols from 11 to 19 to use for red bars
        Dim colBarR2 As Integer = 12
        Dim colBarR3 As Integer = 13
        Dim colBarR4 As Integer = 14
        Dim colBarR5 As Integer = 15
        Dim colBarR6 As Integer = 16
        Dim colBarR7 As Integer = 17
        Dim colBarR8 As Integer = 18
        Dim colBarR9 As Integer = 19

        Dim colObjNameRight As Integer = 20 '2 '20

        Dim maxLenObjName As Integer = 10
        Dim maxLenPath As Integer = 10
        Dim barUnitChar As String = "W" 'use to calculate cell width = char width * num of chars (which is the judgment)
        Dim maxLenBarLeft As Integer = 0 'use to calculate the column width
        Dim maxLenBarRight As Integer = 0

        Dim sNameLeft As String = "LName"
        Dim sNameRight As String = "RName"
        Dim jValue As Double = UNDEFINED_SINGLE_VALUE

        Dim row As Integer = 2 'rowUserName + 1 'row number 
        Dim col As Integer = 0 'col number

        With worksheet
            'wrtite table tytle
            .Cells(rowTitle, colObjNameLeft).Value = "Judgments Of Objectives"
            Dim cellRange As CellRange = .Cells.GetSubrangeAbsolute(rowTitle, colObjNameLeft, rowTitle, colObjNameRight)
            cellRange.Merged = True
            cellRange.Style = style_TableTitle()

            Dim Judgments As List(Of clsCustomMeasureData)

            For Each node As clsNode In objH.Nodes
                ' rows with objectives names
                Dim NodePath As String = ""
                GetFullNodePath(node, NodePath)
                .Cells(row, colObjNameLeft).Value = NodePath
                cellRange = .Cells.GetSubrangeAbsolute(row, colObjNameLeft, row, colObjNameRight)
                cellRange.Merged = True
                cellRange.Style = style_ColumnHeader()

                If maxLenObjName < Len(node.NodeName) Then maxLenObjName = Len(node.NodeName)
                If maxLenPath < Len(NodePath) Then maxLenPath = Len(NodePath)
                row += 2

                Judgments = node.Judgments.JudgmentsFromUsers(UserIDs)

                For Each UserID As Integer In UserIDs

                    If Not node.IsTerminalNode Then
                        Dim u As clsUser = PM.GetUserByID(UserID)

                        If Judgments.Count > 0 Then
                            .Cells(row, colObjNameLeft).Value = u.UserName
                            cellRange = .Cells.GetSubrangeAbsolute(row, colObjNameLeft, row, colObjNameRight)
                            cellRange.Merged = True
                            'cellRange.Style.FillPattern.SetSolid(Color.LightGray)
                            cellRange.Style = style_RowsGroupName()
                            row += 1

                            Select Case node.MeasureType
                                Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous
                                    For Each J As clsCustomMeasureData In Judgments
                                        sNameLeft = ""
                                        sNameRight = ""
                                        jValue = 0

                                        If Not J.IsUndefined Then
                                            If J.UserID = UserID Then
                                                .Rows(row).Height = 100 'AS/18712za
                                                row += 1 'AS/18712za

                                                Dim pwData As clsPairwiseMeasureData = CType(J, clsPairwiseMeasureData)
                                                sNameLeft = PM.Hierarchy(PM.ActiveHierarchy).GetNodeByID(pwData.FirstNodeID).NodeName
                                                sNameRight = PM.Hierarchy(PM.ActiveHierarchy).GetNodeByID(pwData.SecondNodeID).NodeName
                                                jValue = pwData.Value * pwData.Advantage

                                                If viewChart Then 'AS/18712za===
                                                    If jValue - CInt(jValue) <> 0 Then
                                                        jValue = Math.Round(jValue, 1)
                                                    End If
                                                    DrawBars(worksheet, jValue, row)
                                                End If 'AS/18712za==

                                                .Cells(row, colObjNameLeft).Value = sNameLeft
                                                .Cells(row, colObjJudgment).Value = jValue.ToString
                                                .Cells(row, colObjNameRight).Value = sNameRight

                                                row += 1
                                            End If
                                        End If
                                    Next

                                Case ECMeasureType.mtRatings
                                    For Each J As clsRatingMeasureData In Judgments
                                        sNameLeft = ""
                                        sNameRight = ""
                                        jValue = 0

                                        If Not J.IsUndefined Then
                                            If J.UserID = UserID Then
                                                sNameLeft = PM.Hierarchy(PM.ActiveHierarchy).GetNodeByID(J.NodeID).NodeName
                                                jValue = J.Rating.Value

                                                .Cells(row, colObjNameLeft).Value = sNameLeft
                                                .Cells(row, colObjJudgment).Value = jValue.ToString
                                                .Cells(row, colObjNameRight).Value = sNameRight
                                                row += 1
                                                'End If

                                            End If
                                        End If
                                    Next

                                Case Else
                                    If (node.MeasureType <> ECMeasureType.mtRatings) And (node.MeasureType <> ECMeasureType.mtPWOutcomes) And Not IsPWMeasurementType(node.MeasureType) Then
                                        Judgments = node.Judgments.JudgmentsFromUsers(UserIDs)
                                        For Each J As clsNonPairwiseMeasureData In Judgments
                                            sNameLeft = ""
                                            sNameRight = ""
                                            jValue = 0

                                            If Not J.IsUndefined Then
                                                If J.UserID >= 0 Then 'C0170
                                                    sNameLeft = PM.Hierarchy(PM.ActiveHierarchy).GetNodeByID(J.NodeID).NodeName
                                                    jValue = CSng(J.ObjectValue)

                                                    .Cells(row, colObjNameLeft).Value = sNameLeft
                                                    .Cells(row, colObjJudgment).Value = jValue.ToString
                                                    .Cells(row, colObjNameRight).Value = sNameRight
                                                    row += 1
                                                End If
                                            End If
                                        Next 'judgment
                                    End If
                            End Select
                        End If
                    End If
                Next 'user
            Next 'node

            '.Columns(colObjNameLeft).Style.Indent = 10
            .Columns(colObjNameLeft).AutoFit()
            Dim W As Integer = .Columns(colObjNameLeft).Width
            .Columns(colObjNameLeft).Width = W + 1000
            .Columns(colObjNameLeft).Style.HorizontalAlignment = HorizontalAlignmentStyle.Right
            '.Columns(colObjNameLeft).Style.WrapText = True

            .Columns(colObjNameRight).AutoFit()
            W = .Columns(colObjNameRight).Width
            .Columns(colObjNameRight).Width = W + 1000
            '.Columns(colObjNameRight).Style.WrapText = True

            .Columns(colObjJudgment).Style.HorizontalAlignment = HorizontalAlignmentStyle.Center
            .Columns(colObjJudgment).Style.NumberFormat = "0.00"

            For i As Integer = colBarL1 To colBarL9 'AS/18712za===
                .Columns(i).Width = 500
            Next
            For i As Integer = colBarR1 To colBarR9
                .Columns(i).Width = 500
            Next 'AS/18712za==

            '' Make entire sheet print on a single page.
            '.PrintOptions.FitWorksheetWidthToPages = 1
            '.PrintOptions.FitWorksheetHeightToPages = 1
        End With

        Return worksheet
    End Function

    Private Function AddObjectivesAndAlternativesWorksheet(Options As ReportGeneratorOptions) As ExcelWorksheet 'AS/18712k
        Dim worksheet = workbook.Worksheets.Add("Objectives and Alternatives")

        'Get lists of alternatives and objectives
        Dim objH As clsHierarchy = App.ActiveProject.HierarchyObjectives
        Dim altH As clsHierarchy = PM.AltsHierarchy(PM.ActiveAltsHierarchy)

        Dim rowTitle As Integer = 0
        Dim rowColHeader As Integer = 2
        Dim colObjName As Integer = 0
        Dim colBlanc As Integer = 1
        Dim colAltName As Integer = 2
        Dim maxRow As Integer = 0

        Dim row As Integer = rowColHeader + 1 'row number 
        Dim col As Integer = 0 'col number

        With worksheet
            'write table title
            .Cells(rowTitle, colObjName).Value = "Objective and Alternatives"

            'write headers for the columns
            .Cells(rowColHeader, colObjName).Value = "Objectives"
            .Cells(rowColHeader, colAltName).Value = "Alternatives"

            'write values into Objectives column
            For Each obj As clsNode In objH.Nodes
                .Cells(row, colObjName).Value = obj.NodeName
                .Cells(row, colObjName).Style.Indent = obj.Level * 3
                row += 1
            Next
            maxRow = row

            'write values into the Alternatives column
            row = rowColHeader + 1
            For Each alt As clsNode In altH.Nodes
                .Cells(row, colAltName).Value = alt.NodeName
                row += 1
            Next
            If maxRow < row Then maxRow = row

            'format
            Dim cellRange As CellRange = .Cells.GetSubrangeAbsolute(rowTitle, colObjName, rowTitle, colAltName)
            cellRange.Merged = True
            cellRange.Style = style_TableTitle()

            cellRange = .Cells.GetSubrangeAbsolute(rowColHeader, colObjName, rowColHeader, colAltName)
            cellRange.Style = style_ColumnHeader()

            cellRange = .Cells.GetSubrangeAbsolute(rowColHeader + 1, colObjName, maxRow, colAltName)
            cellRange.Style.Font.Size = 9 * 20 'cannot apply style since it overwrites the indents, so just set the font here

            .Columns(colObjName).AutoFit()
            .Columns(colBlanc).SetWidth(15, LengthUnit.Pixel)
            .Columns(colAltName).AutoFit()


            ' Make entire sheet print on a single page.
            .PrintOptions.FitWorksheetWidthToPages = 1
            .PrintOptions.FitWorksheetHeightToPages = 1
        End With

        Return worksheet
    End Function

    Private Function AddContributionsWorksheet(Options As ReportGeneratorOptions) As ExcelWorksheet 'AS/18712l
        Dim worksheet = workbook.Worksheets.Add("Contributions")

        'Get lists of alternatives and objectives
        Dim objH As clsHierarchy = App.ActiveProject.HierarchyObjectives
        Dim altH As clsHierarchy = PM.AltsHierarchy(PM.ActiveAltsHierarchy)

        Dim rowTitle As Integer = 0
        Dim colObjName As Integer = 0
        Dim colMeasureType As Integer = 1
        Dim cellRange As CellRange = Nothing 'AS/18712yb
        Dim keepIndent As Boolean = True 'AS/18712yb
        Dim row As Integer = 1 'row number 
        Dim col As Integer = 0 'col number

        With worksheet
            'write table title
            .Cells(rowTitle, colObjName).Value = "Contributions"

            'write values into the columns cell by cell
            row = rowTitle + 2 'leave one row blank
            Dim rowFirstAlt As Integer = 0
            Dim rowLastAlt As Integer = 0

            For Each obj As clsNode In objH.Nodes
                .Cells(row, colObjName).Value = obj.NodeName
                .Cells(row, colMeasureType).Value = GetMeasureTypeName(obj.MeasureType)
                '.Cells(row, colObjName).Style.Font.Weight = ExcelFont.BoldWeight 'AS/18712yb
                .Cells(row, colObjName).Style = style_ColumnHeader(keepIndent) 'AS/18712yb
                .Cells(row, colObjName).Style.Indent = obj.Level * 3

                row += 1
                rowFirstAlt = row

                If obj.IsTerminalNode Then
                    For Each alt As clsNode In obj.GetNodesBelow(UNDEFINED_USER_ID)
                        If Not alt Is Nothing Then
                            .Cells(row, colObjName).Value = alt.NodeName
                            .Cells(row, colObjName).Style = style_TableBodyNormal() 'AS/18712yb
                            .Cells(row, colObjName).Style.Indent = (obj.Level + 1) * 3
                            row += 1
                        End If
                    Next
                    rowLastAlt = row - 1
                    If rowLastAlt >= rowFirstAlt Then
                        cellRange = .Cells.GetSubrangeAbsolute(rowFirstAlt, colObjName, rowLastAlt, colObjName)
                        'cellRange.Style.Font.Color = Color.Blue 'AS/18712yb
                    End If
                End If

            Next

            'apply format to title (the other paorts are formatted on fly)
            cellRange = .Cells.GetSubrangeAbsolute(rowTitle, colObjName, rowTitle, colMeasureType)
            cellRange.Merged = True
            cellRange.Style = style_TableTitle()

            'cellRange = .Cells.GetSubrangeAbsolute(rowTitle + 1, colObjName, rowLastAlt, colMeasureType) 'AS/18712yb===
            'cellRange.Style.Font.Name = "Arial"
            'cellRange.Style.Font.Size = 12 * 20 'AS/18712yb==
            .Columns(colObjName).AutoFit()
            .Columns(colMeasureType).AutoFit()


            '' Make entire sheet print on a single page.
            '.PrintOptions.FitWorksheetWidthToPages = 1
            '.PrintOptions.FitWorksheetHeightToPages = 1
        End With

        Return worksheet
    End Function

    Private Function AddJudgmentsOverviewWorksheet_byUsers(Options As ReportGeneratorOptions) As ExcelWorksheet 'AS/18712m
        Dim worksheet = workbook.Worksheets.Add("Judgments Overview by Users")

        'Get lists of alternatives and objectives
        Dim objH As clsHierarchy = App.ActiveProject.HierarchyObjectives
        Dim altH As clsHierarchy = PM.AltsHierarchy(PM.ActiveAltsHierarchy)

        'special rows:
        Dim rowTitle As Integer = 0
        Dim rowColHeader As Integer = 2 'column header

        'special columns:
        Dim colUserName As Integer = 0
        Dim colEmail As Integer = 1
        Dim colWRT As Integer = 2
        Dim colElement1 As Integer = 3
        Dim colElement2 As Integer = 4
        Dim colValue As Integer = 5
        Dim colComment As Integer = 6
        Dim colMeasureType As Integer = 7

        Dim sElement1 As String = "LName"
        Dim sElement2 As String = "RName"
        Dim jValue As Double = UNDEFINED_SINGLE_VALUE

        Dim row As Integer = 3 'rowUserName + 1 'row number 
        Dim col As Integer = 0 'col number

        With worksheet
            'wrtite table tytle
            .Cells(rowTitle, colElement1).Value = "Judgments Overview"
            Dim cellRange As CellRange = .Cells.GetSubrangeAbsolute(rowTitle, colUserName, rowTitle, colMeasureType)
            cellRange.Merged = True
            cellRange.Style = style_TableTitle()

            .Cells(rowColHeader, colUserName).Value = "Name"
            .Cells(rowColHeader, colEmail).Value = "Email"
            .Cells(rowColHeader, colWRT).Value = "Value"
            .Cells(rowColHeader, colElement1).Value = "Element 1"
            .Cells(rowColHeader, colElement2).Value = "Element 2"
            .Cells(rowColHeader, colComment).Value = "Comment"
            .Cells(rowColHeader, colMeasureType).Value = "Measure Type"
            cellRange = .Cells.GetSubrangeAbsolute(rowColHeader, colUserName, rowColHeader, colMeasureType)
            cellRange.Style = style_ColumnHeader()
            cellRange.Style.FillPattern.SetSolid(Color.LightGray)

            Dim Judgments As List(Of clsCustomMeasureData)

            For Each UserID As Integer In UserIDs 'AS/18712x=== moved up
                Dim u As clsUser = PM.GetUserByID(UserID)

                For Each node As clsNode In objH.Nodes
                    Judgments = node.Judgments.JudgmentsFromUsers(UserIDs)

                    If Not node.IsTerminalNode Then

                        If Judgments.Count > 0 Then

                            Select Case node.MeasureType
                                Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous
                                    For Each J As clsCustomMeasureData In Judgments
                                        sElement1 = ""
                                        sElement2 = ""
                                        jValue = 0

                                        If Not J.IsUndefined Then
                                            If J.UserID = UserID Then
                                                Dim pwData As clsPairwiseMeasureData = CType(J, clsPairwiseMeasureData)
                                                sElement1 = PM.Hierarchy(PM.ActiveHierarchy).GetNodeByID(pwData.FirstNodeID).NodeName
                                                sElement2 = PM.Hierarchy(PM.ActiveHierarchy).GetNodeByID(pwData.SecondNodeID).NodeName
                                                jValue = pwData.Value * pwData.Advantage

                                                .Cells(row, colUserName).Value = u.UserName
                                                .Cells(row, colEmail).Value = u.UserEMail
                                                .Cells(row, colWRT).Value = node.NodeName
                                                .Cells(row, colElement1).Value = sElement1
                                                .Cells(row, colElement2).Value = sElement2
                                                .Cells(row, colValue).Value = jValue.ToString
                                                .Cells(row, colComment).Value = node.Comment
                                                .Cells(row, colMeasureType).Value = GetMeasureTypeName(node.MeasureType)

                                                row += 1
                                                'End If
                                            End If
                                        End If
                                    Next

                                Case ECMeasureType.mtRatings
                                    For Each J As clsRatingMeasureData In Judgments
                                        sElement1 = ""
                                        sElement2 = ""
                                        jValue = 0

                                        If Not J.IsUndefined Then
                                            If J.UserID = UserID Then
                                                sElement1 = node.NodeName 'PM.Hierarchy(PM.ActiveHierarchy).GetNodeByID(J.NodeID).NodeName
                                                jValue = J.Rating.Value

                                                .Cells(row, colUserName).Value = u.UserName
                                                .Cells(row, colEmail).Value = u.UserEMail
                                                .Cells(row, colWRT).Value = node.NodeName
                                                .Cells(row, colElement1).Value = sElement1
                                                .Cells(row, colElement2).Value = sElement2
                                                .Cells(row, colValue).Value = jValue.ToString
                                                .Cells(row, colComment).Value = node.Comment
                                                .Cells(row, colMeasureType).Value = GetMeasureTypeName(node.MeasureType)

                                                row += 1
                                                'End If

                                            End If
                                        End If
                                    Next

                                Case Else
                                    If (node.MeasureType <> ECMeasureType.mtRatings) And (node.MeasureType <> ECMeasureType.mtPWOutcomes) And Not IsPWMeasurementType(node.MeasureType) Then
                                        Judgments = node.Judgments.JudgmentsFromUsers(UserIDs)
                                        For Each J As clsNonPairwiseMeasureData In Judgments
                                            sElement1 = ""
                                            sElement2 = ""
                                            jValue = 0

                                            If Not J.IsUndefined Then
                                                If J.UserID >= 0 Then 'C0170
                                                    sElement1 = node.NodeName 'PM.Hierarchy(PM.ActiveHierarchy).GetNodeByID(J.NodeID).NodeName
                                                    jValue = CSng(J.ObjectValue)

                                                    .Cells(row, colUserName).Value = u.UserName
                                                    .Cells(row, colEmail).Value = u.UserEMail
                                                    .Cells(row, colWRT).Value = node.NodeName
                                                    .Cells(row, colElement1).Value = sElement1
                                                    .Cells(row, colElement2).Value = sElement2
                                                    .Cells(row, colValue).Value = jValue.ToString
                                                    .Cells(row, colComment).Value = node.Comment
                                                    .Cells(row, colMeasureType).Value = GetMeasureTypeName(node.MeasureType)
                                                    row += 1
                                                End If
                                            End If
                                        Next 'judgment
                                    End If
                            End Select
                        End If

                    Else ' node.IsTerminalNode
                        For Each alt As clsNode In altH.TerminalNodes
                            Dim stringValue As String = " "
                            Dim singleValue As Single = UNDEFINED_SINGLE_VALUE
                            Select Case node.MeasureType
                                Case ECMeasureType.mtRatings
                                    Dim currJ As clsRatingMeasureData = CType(GetJudgment(node, alt.NodeID, UserID), clsRatingMeasureData)
                                    If Not IsNothing(currJ) Then
                                        If Not currJ.IsUndefined Then
                                            'Try 'AS/18712x removed
                                            If Not IsNothing(currJ.Rating.RatingScale) Then
                                                stringValue = currJ.Rating.Name
                                                singleValue = currJ.SingleValue
                                            Else
                                                stringValue = JS_SafeNumber(currJ.Rating.Value)
                                            End If
                                            .Cells(row, colUserName).Value = u.UserName
                                            .Cells(row, colEmail).Value = u.UserEMail
                                            .Cells(row, colWRT).Value = node.NodeName
                                            .Cells(row, colElement1).Value = alt.NodeName
                                            .Cells(row, colElement2).Value = ""
                                            .Cells(row, colValue).Value = singleValue
                                            .Cells(row, colComment).Value = node.Comment
                                            .Cells(row, colMeasureType).Value = GetMeasureTypeName(node.MeasureType)
                                            row += 1
                                            'Catch
                                            '    stringValue = " "
                                            'End Try
                                        End If
                                    End If

                                Case ECMeasureType.mtDirect, ECMeasureType.mtStep, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtCustomUtilityCurve
                                    Dim currJ As clsNonPairwiseMeasureData = GetJudgment(node, alt.NodeID, UserID)
                                    If Not IsNothing(currJ) Then
                                        If Not currJ.IsUndefined Then
                                            'Try 'AS/18712x removed
                                            Dim aVal As Double = CDbl(currJ.SingleValue)
                                            If aVal = Int32.MinValue Or aVal.Equals(Double.NaN) Then
                                                stringValue = " "
                                            Else
                                                stringValue = JS_SafeNumber(aVal)
                                                singleValue = currJ.SingleValue
                                            End If
                                            .Cells(row, colUserName).Value = u.UserName
                                            .Cells(row, colEmail).Value = u.UserEMail
                                            .Cells(row, colWRT).Value = node.NodeName
                                            .Cells(row, colElement1).Value = alt.NodeName
                                            .Cells(row, colElement2).Value = ""
                                            .Cells(row, colValue).Value = singleValue
                                            .Cells(row, colComment).Value = node.Comment
                                            .Cells(row, colMeasureType).Value = GetMeasureTypeName(node.MeasureType)
                                            row += 1
                                            'Catch
                                            '    stringValue = " "
                                            'End Try
                                        End If
                                    End If

                                Case Else 'for PW 
                                    Dim aVal As Double = node.Judgments.Weights.GetUserWeights(UserID, ECSynthesisMode.smIdeal, PM.CalculationsManager.IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(alt.NodeID)
                                    stringValue = aVal.ToString
                                    singleValue = TEMP_SINGLE_VALUE 'CSng(aVal)

                                    .Cells(row, colUserName).Value = u.UserName
                                    .Cells(row, colEmail).Value = u.UserEMail
                                    .Cells(row, colWRT).Value = node.NodeName
                                    .Cells(row, colElement1).Value = alt.NodeName
                                    .Cells(row, colElement2).Value = ""
                                    .Cells(row, colValue).Value = singleValue
                                    .Cells(row, colComment).Value = node.Comment
                                    .Cells(row, colMeasureType).Value = GetMeasureTypeName(node.MeasureType)
                                    row += 1
                            End Select


                        Next 'next alt
                        'Next 'next user
                    End If
                    '=================
                Next 'user
            Next 'node

            .Columns(colUserName).AutoFit()
            .Columns(colEmail).AutoFit()
            .Columns(colWRT).AutoFit()
            .Columns(colElement1).AutoFit()
            .Columns(colElement2).AutoFit()
            .Columns(colValue).AutoFit()
            .Columns(colComment).AutoFit()
            .Columns(colMeasureType).AutoFit()
        End With

        Return worksheet
    End Function

    Private Function AddJudgmentsOverviewWorksheet(Options As ReportGeneratorOptions) As ExcelWorksheet 'AS/18712m
        Dim worksheet = workbook.Worksheets.Add("Judgments Overview")

        'Get lists of alternatives and objectives
        Dim objH As clsHierarchy = App.ActiveProject.HierarchyObjectives
        Dim altH As clsHierarchy = PM.AltsHierarchy(PM.ActiveAltsHierarchy)

        'special rows:
        Dim rowTitle As Integer = 0
        Dim rowColHeader As Integer = 2 'column header

        'special columns:
        Dim colUserName As Integer = 0
        Dim colEmail As Integer = 1
        Dim colWRT As Integer = 2
        Dim colElement1 As Integer = 3
        Dim colElement2 As Integer = 4
        Dim colValue As Integer = 5
        Dim colComment As Integer = 6
        Dim colMeasureType As Integer = 7

        Dim sElement1 As String = "LName"
        Dim sElement2 As String = "RName"
        Dim jValue As Double = UNDEFINED_SINGLE_VALUE

        Dim row As Integer = 3 'rowUserName + 1 'row number 
        Dim col As Integer = 0 'col number

        With worksheet
            'wrtite table tytle
            .Cells(rowTitle, colElement1).Value = "Judgments Overview"

            .Cells(rowColHeader, colUserName).Value = "Name"
            .Cells(rowColHeader, colEmail).Value = "Email"
            .Cells(rowColHeader, colWRT).Value = "Value"
            .Cells(rowColHeader, colElement1).Value = "Element 1"
            .Cells(rowColHeader, colElement2).Value = "Element 2"
            .Cells(rowColHeader, colComment).Value = "Comment"
            .Cells(rowColHeader, colMeasureType).Value = "Measure Type"

            Dim Judgments As New List(Of clsCustomMeasureData)

            For Each node As clsNode In objH.Nodes
                If Not node.IsTerminalNode Then
                    Judgments = node.Judgments.JudgmentsFromUsers(UserIDs)
                End If 'AS/18712x==

                For Each UserID As Integer In UserIDs
                    Dim u As clsUser = PM.GetUserByID(UserID)

                    If Not node.IsTerminalNode Then
                        If Judgments.Count > 0 Then
                            Select Case node.MeasureType
                                Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous
                                    For Each J As clsCustomMeasureData In Judgments
                                        sElement1 = ""
                                        sElement2 = ""
                                        jValue = 0

                                        If Not J.IsUndefined Then
                                            If J.UserID = UserID Then
                                                Dim pwData As clsPairwiseMeasureData = CType(J, clsPairwiseMeasureData)
                                                sElement1 = PM.Hierarchy(PM.ActiveHierarchy).GetNodeByID(pwData.FirstNodeID).NodeName
                                                sElement2 = PM.Hierarchy(PM.ActiveHierarchy).GetNodeByID(pwData.SecondNodeID).NodeName
                                                jValue = pwData.Value * pwData.Advantage

                                                .Cells(row, colUserName).Value = u.UserName
                                                .Cells(row, colEmail).Value = u.UserEMail
                                                .Cells(row, colWRT).Value = node.NodeName
                                                .Cells(row, colElement1).Value = sElement1
                                                .Cells(row, colElement2).Value = sElement2
                                                .Cells(row, colValue).Value = jValue.ToString
                                                .Cells(row, colComment).Value = node.Comment
                                                .Cells(row, colMeasureType).Value = GetMeasureTypeName(node.MeasureType)

                                                row += 1
                                                'End If
                                            End If
                                        End If
                                    Next

                                Case ECMeasureType.mtRatings
                                    For Each J As clsRatingMeasureData In Judgments
                                        sElement1 = ""
                                        sElement2 = ""
                                        jValue = 0

                                        If Not J.IsUndefined Then
                                            If J.UserID = UserID Then
                                                sElement1 = node.NodeName 'PM.Hierarchy(PM.ActiveHierarchy).GetNodeByID(J.NodeID).NodeName
                                                jValue = J.Rating.Value

                                                .Cells(row, colUserName).Value = u.UserName
                                                .Cells(row, colEmail).Value = u.UserEMail
                                                .Cells(row, colWRT).Value = node.NodeName
                                                .Cells(row, colElement1).Value = sElement1
                                                .Cells(row, colElement2).Value = sElement2
                                                .Cells(row, colValue).Value = jValue.ToString
                                                .Cells(row, colComment).Value = node.Comment
                                                .Cells(row, colMeasureType).Value = GetMeasureTypeName(node.MeasureType)

                                                row += 1
                                                'End If

                                            End If
                                        End If
                                    Next

                                Case Else
                                    If (node.MeasureType <> ECMeasureType.mtRatings) And (node.MeasureType <> ECMeasureType.mtPWOutcomes) And Not IsPWMeasurementType(node.MeasureType) Then
                                        Judgments = node.Judgments.JudgmentsFromUsers(UserIDs)
                                        For Each J As clsNonPairwiseMeasureData In Judgments
                                            sElement1 = ""
                                            sElement2 = ""
                                            jValue = 0

                                            If Not J.IsUndefined Then
                                                If J.UserID >= 0 Then 'C0170
                                                    sElement1 = node.NodeName 'PM.Hierarchy(PM.ActiveHierarchy).GetNodeByID(J.NodeID).NodeName
                                                    jValue = CSng(J.ObjectValue)

                                                    .Cells(row, colUserName).Value = u.UserName
                                                    .Cells(row, colEmail).Value = u.UserEMail
                                                    .Cells(row, colWRT).Value = node.NodeName
                                                    .Cells(row, colElement1).Value = sElement1
                                                    .Cells(row, colElement2).Value = sElement2
                                                    .Cells(row, colValue).Value = jValue.ToString
                                                    .Cells(row, colComment).Value = node.Comment
                                                    .Cells(row, colMeasureType).Value = GetMeasureTypeName(node.MeasureType)
                                                    row += 1
                                                End If
                                            End If
                                        Next 'judgment
                                    End If
                            End Select
                        End If

                    Else ' node.IsTerminalNode
                        For Each alt As clsNode In altH.TerminalNodes
                            Dim stringValue As String = ""
                            Dim singleValue As Single = UNDEFINED_SINGLE_VALUE
                            Select Case node.MeasureType
                                Case ECMeasureType.mtRatings
                                    Dim currJ As clsRatingMeasureData = CType(GetJudgment(node, alt.NodeID, UserID), clsRatingMeasureData)
                                    If Not IsNothing(currJ) Then
                                        If Not currJ.IsUndefined Then
                                            'Try
                                            If Not IsNothing(currJ.Rating.RatingScale) Then
                                                stringValue = currJ.Rating.Name
                                                singleValue = currJ.SingleValue
                                            Else
                                                stringValue = JS_SafeNumber(currJ.Rating.Value)
                                            End If
                                            .Cells(row, colUserName).Value = u.UserName
                                            .Cells(row, colEmail).Value = u.UserEMail
                                            .Cells(row, colWRT).Value = node.NodeName
                                            .Cells(row, colElement1).Value = alt.NodeName
                                            .Cells(row, colElement2).Value = ""
                                            .Cells(row, colValue).Value = singleValue
                                            .Cells(row, colComment).Value = node.Comment
                                            .Cells(row, colMeasureType).Value = GetMeasureTypeName(node.MeasureType)
                                            row += 1
                                            'Catch
                                            '    stringValue = " "
                                            'End Try
                                        End If
                                    End If

                                Case ECMeasureType.mtDirect, ECMeasureType.mtStep, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtCustomUtilityCurve
                                    Dim currJ As clsNonPairwiseMeasureData = GetJudgment(node, alt.NodeID, UserID)
                                    If Not IsNothing(currJ) Then
                                        If Not currJ.IsUndefined Then
                                            'Try
                                            Dim aVal As Double = CDbl(currJ.SingleValue)
                                            If aVal = Int32.MinValue Or aVal.Equals(Double.NaN) Then
                                                stringValue = " "
                                            Else
                                                stringValue = JS_SafeNumber(aVal)
                                                singleValue = currJ.SingleValue
                                            End If
                                            .Cells(row, colUserName).Value = u.UserName
                                            .Cells(row, colEmail).Value = u.UserEMail
                                            .Cells(row, colWRT).Value = node.NodeName
                                            .Cells(row, colElement1).Value = alt.NodeName
                                            .Cells(row, colElement2).Value = ""
                                            .Cells(row, colValue).Value = singleValue
                                            .Cells(row, colComment).Value = node.Comment
                                            .Cells(row, colMeasureType).Value = GetMeasureTypeName(node.MeasureType)
                                            row += 1
                                            'Catch
                                            '    stringValue = " "
                                            'End Try
                                        End If
                                    End If

                                Case Else 'for PW 
                                    Dim aVal As Double = node.Judgments.Weights.GetUserWeights(UserID, ECSynthesisMode.smIdeal, PM.CalculationsManager.IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(alt.NodeID)
                                    stringValue = aVal.ToString
                                    singleValue = TEMP_SINGLE_VALUE 'CSng(aVal)

                                    .Cells(row, colUserName).Value = u.UserName
                                    .Cells(row, colEmail).Value = u.UserEMail
                                    .Cells(row, colWRT).Value = node.NodeName
                                    .Cells(row, colElement1).Value = alt.NodeName
                                    .Cells(row, colElement2).Value = ""
                                    .Cells(row, colValue).Value = singleValue
                                    .Cells(row, colComment).Value = node.Comment
                                    .Cells(row, colMeasureType).Value = GetMeasureTypeName(node.MeasureType)
                                    row += 1
                            End Select


                        Next 'next alt
                        'Next 'next user
                    End If
                    '=================
                Next 'user
            Next 'node

            'format -- apply styles (alignment, colors, borders. etc.) 'AS/18712yc===
            .Cells(rowTitle, colElement1).Value = "Judgments Overview"
            Dim cellRange As CellRange = .Cells.GetSubrangeAbsolute(rowTitle, colUserName, rowTitle, colMeasureType)
            cellRange.Merged = True
            cellRange.Style = style_TableTitle()

            cellRange = .Cells.GetSubrangeAbsolute(rowColHeader, colUserName, rowColHeader, colMeasureType)
            cellRange.Style = style_ColumnHeader()

            cellRange = .Cells.GetSubrangeAbsolute(rowColHeader + 1, colUserName, row - 1, colMeasureType)
            cellRange.Style = style_TableBodyNormal()
            ShadeAlternateRows(worksheet, cellRange) 'AS/18712yc==

            .Columns(colUserName).AutoFit()
            .Columns(colEmail).AutoFit()
            .Columns(colWRT).AutoFit()
            .Columns(colElement1).AutoFit()
            .Columns(colElement2).AutoFit()
            .Columns(colValue).AutoFit()
            .Columns(colComment).AutoFit()
            .Columns(colMeasureType).AutoFit()
        End With

        Return worksheet
    End Function

    Private Function AddObjectivesPrioritiesWorksheet(Options As ReportGeneratorOptions) As ExcelWorksheet 'AS/18712n
        Dim worksheet = workbook.Worksheets.Add("Objectives Priorities")

        Dim uid As Integer = -1 'All Participants

        'calculate
        Dim wrtNode As clsNode = PM.ActiveObjectives.GetNodeByID(1)
        Dim CalcTarget As clsCalculationTarget = PM.CalculationsManager.GetCalculationTargetByUserID(uid)
        PM.CalculationsManager.Calculate(CalcTarget, wrtNode)

        'Get list of objectives
        Dim objH As clsHierarchy = App.ActiveProject.HierarchyObjectives

        'special rows:
        Dim rowTitle As Integer = 0
        Dim rowColHeader As Integer = 2 'column header

        'special columns:
        Dim colEmail As Integer = 0
        Dim colUserName As Integer = 1
        Dim colObjective As Integer = 2
        Dim colPath As Integer = 3
        Dim colLocalPriority As Integer = 4
        Dim colGlobalPriority As Integer = 5
        Dim colInconsistency As Integer = 6

        Dim jValue As Double = UNDEFINED_SINGLE_VALUE

        Dim row As Integer = 3 'rowUserName + 1 'row number 
        Dim col As Integer = 0 'col number

        With worksheet
            'wrtite table title
            .Cells(rowTitle, colUserName).Value = "Objectives Priorities"

            .Cells(rowColHeader, colEmail).Value = "Email"
            .Cells(rowColHeader, colUserName).Value = "Name"
            .Cells(rowColHeader, colObjective).Value = "Objective"
            .Cells(rowColHeader, colPath).Value = "Path"
            .Cells(rowColHeader, colLocalPriority).Value = "Local Priority"
            .Cells(rowColHeader, colGlobalPriority).Value = "Global Priority"
            .Cells(rowColHeader, colInconsistency).Value = "Inconsistency"

            For Each UserID As Integer In UserIDs
                Dim u As clsUser = PM.GetUserByID(UserID) 'AS/18712x moved up

                For Each node As clsNode In objH.Nodes
                    Dim NodePath As String = ""
                    GetFullNodePath(node, NodePath)

                    .Cells(row, colUserName).Value = u.UserName
                    .Cells(row, colEmail).Value = u.UserEMail
                    .Cells(row, colObjective).Value = node.NodeName
                    .Cells(row, colPath).Value = NodePath

                    If node.MeasureType = ECMeasureType.mtPairwise AndAlso node.IsAllowed(u.UserID) Then
                        If CType(node.Judgments, clsPairwiseJudgments).HasSpanningSet(u.UserID) Then
                            node.CalculateLocal(u.UserID)
                            .Cells(row, colLocalPriority).Value = node.SALocalPriority
                            .Cells(row, colGlobalPriority).Value = node.SAGlobalPriority
                            .Cells(row, colInconsistency).Value = CType(node.Judgments, clsPairwiseJudgments).EigenCalcs.InconIndex
                        End If

                        row += 1
                    Else
                        'node.CalculateLocal(u.UserID)
                        .Cells(row, colLocalPriority).Value = node.SALocalPriority
                        .Cells(row, colGlobalPriority).Value = node.SAGlobalPriority
                        .Cells(row, colInconsistency).Value = 0

                        row += 1

                    End If
                Next 'node
            Next 'user

            'format table title
            Dim cellRange As CellRange = .Cells.GetSubrangeAbsolute(rowTitle, colEmail, rowTitle, colInconsistency)
            cellRange.Merged = True
            cellRange.Style = style_TableTitle()

            'format column headers
            cellRange = .Cells.GetSubrangeAbsolute(rowColHeader, colEmail, rowColHeader, colInconsistency)
            cellRange.Style = style_ColumnHeader()

            'format table body
            cellRange = .Cells.GetSubrangeAbsolute(rowColHeader + 1, colEmail, row, colInconsistency)
            cellRange.Style = style_TableBodyNormal()
            ShadeAlternateRows(worksheet, cellRange) 'AS/18712yc

            'set cols widths and number format
            .Columns(colUserName).AutoFit()
            .Columns(colEmail).AutoFit()
            .Columns(colObjective).AutoFit()
            '.Columns(colPath).AutoFit()
            Dim colW As Integer = .Columns(colObjective).Width
            .Columns(colPath).Width = CInt(colW * 1.1)

            .Columns(colLocalPriority).AutoFit()
            .Columns(colLocalPriority).Style.NumberFormat = "0.00%"
            .Columns(colGlobalPriority).AutoFit()
            .Columns(colGlobalPriority).Style.NumberFormat = "0.00%"
            .Columns(colInconsistency).AutoFit()
            .Columns(colInconsistency).Style.NumberFormat = "0.00"
        End With

        Return worksheet
    End Function

    Private Function AddAlternativesPrioritiesWorksheet(Options As ReportGeneratorOptions) As ExcelWorksheet 'AS/18712o
        Dim worksheet = workbook.Worksheets.Add("Alternatives Priorities")
        Dim UserID As Integer = -1 'All Participants

        'calc priorities for Goal for 'All Participants'
        Dim wrtNode As clsNode = PM.ActiveObjectives.GetNodeByID(1)
        Dim CalcTarget As clsCalculationTarget = PM.CalculationsManager.GetCalculationTargetByUserID(UserID)
        PM.CalculationsManager.Calculate(CalcTarget, wrtNode)

        'Get lists of alternatives and objectives
        Dim altH As clsHierarchy = PM.AltsHierarchy(PM.ActiveAltsHierarchy)

        Dim rowTitle As Integer = 0
        Dim rowColHeader As Integer = 2
        Dim colAltName As Integer = 0
        Dim colPriorities As Integer = 1

        Dim row As Integer = 3 'row number 
        Dim col As Integer = 0 'col number

        With worksheet
            'write table title
            .Cells(rowTitle, colAltName).Value = "Alternatives Priorities"

            'write columns headers
            .Cells(rowColHeader, colAltName).Value = "Alternative"
            Dim userName As String = "Priority For: "
            Select Case UserID
                Case -1 'combined 
                    userName = userName & "All Participants"
                Case >= 0 'individual user
                    userName = userName & PM.GetUserByID(UserID).UserName
                Case < -1 'group
                    userName = userName & PM.CombinedGroups.GetCombinedGroupByUserID(UserID).Name
            End Select
            .Cells(rowColHeader, colPriorities).Value = userName

            'write values into the columns
            For Each alt As clsNode In altH.TerminalNodes
                .Cells(row, colAltName).Value = alt.NodeName
                .Cells(row, colPriorities).Value = alt.SAGlobalPriority

                row += 1
            Next

            'apply formats
            Dim cellRange As CellRange = .Cells.GetSubrangeAbsolute(rowTitle, colAltName, rowTitle, colPriorities)
            cellRange.Merged = True
            cellRange.Style = style_TableTitle()

            cellRange = .Cells.GetSubrangeAbsolute(rowColHeader, colAltName, rowColHeader, colPriorities)
            cellRange.Style = style_ColumnHeader()

            cellRange = .Cells.GetSubrangeAbsolute(rowColHeader + 1, colAltName, row - 1, colPriorities) 'AS/18712yc===
            cellRange.Style = style_TableBodyNormal()
            ShadeAlternateRows(worksheet, cellRange) 'AS/18712yc==

            .Columns(colAltName).AutoFit()
            .Columns(colAltName).Style.WrapText = True
            .Columns(colPriorities).AutoFit()
            .Columns(colPriorities).Style.NumberFormat = "0.00%"

            ' Make entire sheet print on a single page.
            .PrintOptions.FitWorksheetWidthToPages = 1
            .PrintOptions.FitWorksheetHeightToPages = 1
        End With

        Return worksheet
    End Function

    Private Function AddEvaluationProgressWorksheet(Options As ReportGeneratorOptions) As ExcelWorksheet 'AS/18712p
        Dim worksheet = workbook.Worksheets.Add("Evaluation Progress")

        'special rows:
        Dim rowTitle As Integer = 0
        Dim rowColHeader As Integer = 2 'column header

        'special columns:
        Dim colUserName As Integer = 0
        Dim colEmail As Integer = 1
        Dim colJudgmentsMade As Integer = 2
        Dim colJudgmentsTotal As Integer = 3
        Dim colPercentage As Integer = 4
        Dim colJudgmentsTime As Integer = 5

        Dim row As Integer = 3 'row number 
        Dim col As Integer = 0 'col number

        With worksheet
            'wrtite table tytle
            .Cells(rowTitle, colUserName).Value = "Evaluation Progress"

            .Cells(rowColHeader, colUserName).Value = "Name"
            .Cells(rowColHeader, colEmail).Value = "Email"
            .Cells(rowColHeader, colJudgmentsMade).Value = "Judgments Made"
            .Cells(rowColHeader, colJudgmentsTotal).Value = "Judgments Total"
            .Cells(rowColHeader, colPercentage).Value = "Percentage"
            .Cells(rowColHeader, colJudgmentsTime).Value = "Judgment Time"

            Dim user As clsUser
            Dim defTotal As Integer = PM.GetDefaultTotalJudgmentCount(PM.ActiveHierarchy)

            Dim uCount As Integer = PM.UsersList.Count
            Dim StartDT As Date = Now

            For i As Integer = 0 To uCount - 1
                user = PM.UsersList(i)
                'If Worker Is Nothing OrElse (Worker IsNot Nothing AndAlso Not Worker.CancellationPending) Then
                If user IsNot Nothing Then
                    Dim userLastJudgmentTime As Nullable(Of DateTime) = user.LastJudgmentTime
                    If userLastJudgmentTime Is Nothing Then
                        userLastJudgmentTime = VERY_OLD_DATE
                    End If

                    Dim totalCount As Integer = PM.ProjectAnalyzer.GetTotalJudgmentsCount(user.UserID, PM.ActiveHierarchy,, True)
                    Dim madeCount As Integer = PM.GetMadeJudgmentCount(PM.ActiveHierarchy, user.UserID, CDate(userLastJudgmentTime))

                    If user IsNot PM.User Then
                        For Each node As clsNode In PM.Hierarchy(PM.ActiveHierarchy).Nodes
                            'node.ClearCalculatedNodesBelow(user.UserID)
                            node.Judgments.DeleteJudgmentsFromUser(user.UserID)
                        Next
                        PM.UsersRoles.CleanUpUserRoles(user.UserID)
                    End If

                    If madeCount = 0 Then userLastJudgmentTime = Nothing

                    .Cells(row, colUserName).Value = user.UserName
                    .Cells(row, colEmail).Value = user.UserEMail
                    .Cells(row, colJudgmentsMade).Value = madeCount
                    .Cells(row, colJudgmentsTotal).Value = totalCount
                    .Cells(row, colPercentage).Value = madeCount / totalCount
                    If userLastJudgmentTime IsNot Nothing Then
                        .Cells(row, colJudgmentsTime).Value = userLastJudgmentTime.ToString
                    Else
                        .Cells(row, colJudgmentsTime).Value = ""
                    End If

                    row += 1
                End If
                'Else
                '    Exit For
                'End If
            Next



            'format table title
            Dim cellRange As CellRange = .Cells.GetSubrangeAbsolute(rowTitle, colUserName, rowTitle, colJudgmentsTime)
            cellRange.Merged = True
            cellRange.Style = style_TableTitle()

            'format column headers
            cellRange = .Cells.GetSubrangeAbsolute(rowColHeader, colUserName, rowColHeader, colJudgmentsTime)
            cellRange.Style = style_ColumnHeader()

            'format table body
            cellRange = .Cells.GetSubrangeAbsolute(rowColHeader + 1, colUserName, row, colJudgmentsTime)
            cellRange.Style = style_TableBodyNormal()
            ShadeAlternateRows(worksheet, cellRange) 'AS/18712yc

            'set cols widths and number format
            .Columns(colUserName).AutoFit()
            .Columns(colEmail).AutoFit()
            .Columns(colJudgmentsMade).AutoFit()
            .Columns(colJudgmentsTotal).AutoFit()

            .Columns(colPercentage).AutoFit()
            .Columns(colPercentage).Style.NumberFormat = "0.00%"
            .Columns(colJudgmentsTime).AutoFit()

        End With

        Return worksheet
    End Function

    Private Function AddPictureWorksheet(server As HttpServerUtility, Options As ReportGeneratorOptions, sReportPartName As String) As ExcelWorksheet 'AS/18712u
        Dim picName As String
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

        Dim worksheet = workbook.Worksheets.Add(picTitle)

        Dim rowTitle As Integer = 0
        Dim colPictureLeft As String = "A"
        Dim rowPictureTop As Integer = 3
        Dim positionFromCell As String = colPictureLeft & rowPictureTop.ToString

        Dim colTitle As Integer = 0

        With worksheet
            'write and format table title
            .Cells(rowTitle, colTitle).Value = picTitle
            .Cells(rowTitle, colTitle).Style = style_TableTitle()
            .Columns(colTitle).SetWidth(Options.PictureWidth, LengthUnit.Pixel)

            ' PNG added by using two anchors.
            '.Pictures.Add(server.MapPath("~/Images/favicon/ITP_Reports_Treeview.png"), "J16", "K20")
            .Pictures.Add(server.MapPath("~/Images/favicon/" & picName), positionFromCell, Options.PictureWidth, Options.PictureHeght, LengthUnit.Pixel)
        End With

        Return worksheet
    End Function

    Private Function AddHierarchyWorksheet(Options As ReportGeneratorOptions) As ExcelWorksheet 'AS/18712v
        Dim worksheet = workbook.Worksheets.Add("Objectives Hierarchy")

        'Get lists of alternatives and objectives
        Dim objH As clsHierarchy = App.ActiveProject.HierarchyObjectives

        Dim row As Integer = 2 'row number 
        Dim col As Integer = 0 'col number

        Dim rowTitle As Integer = 0
        Dim colObjName As Integer = 0
        Dim maxLenObjName As Integer = 10
        Dim colW As Double ', rowH As Double


        With worksheet
            'write headers for the columns
            .Cells(rowTitle, colObjName).Value = "Objectives Hierarchy"
            .Cells(rowTitle, colObjName).Style = style_TableTitle()
            .Columns(colObjName).Style.Font.Name = "Arial"
            .Columns(colObjName).Style.Font.Color = Color.Black

            'write objectives names
            For Each node As clsNode In objH.Nodes
                .Cells(row, colObjName).Value = node.NodeName
                .Cells(row, colObjName).Style = style_TableBodyBold()
                .Cells(row, colObjName).Style.Font.Size = CInt(Options.HierarchyFontSize) * 20 '12 * 20
                .Cells(row, colObjName).Style.Font.Weight = ExcelFont.NormalWeight
                .Cells(row, colObjName).Style.Indent = node.Level * 3

                If maxLenObjName < Len(node.NodeName) Then maxLenObjName = Len(node.NodeName)

                Dim fSize As Double = .Cells(row, colObjName).Style.Font.Size / 20
                Dim fName As String = .Columns(colObjName).Style.Font.Name

                Dim newW As Double, newH As Double
                getCellSize(node.NodeName, fName, newW, newH, CSng(fSize.ToString))
                If colW < newW Then colW = newW

                row += 1

            Next

            '.Columns(colObjName).AutoFit()
            .Columns(colObjName).SetWidth(colW, LengthUnit.Point)
            .Columns(colObjName).Style.WrapText = True

            ' Make entire sheet print on a single page.
            .PrintOptions.FitWorksheetWidthToPages = 1
            .PrintOptions.FitWorksheetHeightToPages = 1
        End With

        Return worksheet
    End Function

    'Private Function AddSurveyWorksheet(Options As ReportGeneratorOptions) As ExcelWorksheet
    '    'TBD
    'End Function

    'Private Function AddTitleWorksheet(Options As ReportGeneratorOptions) As ExcelWorksheet
    '    'TBD
    'End Function

    'Private Function AddInfodocWorksheet(Options As ReportGeneratorOptions, infodoc As String) As ExcelWorksheet
    '    'TBD
    'End Function
#End Region

#Region "Common (reusable) functions"

    Private Sub AdjustImages(ws As ExcelWorksheet, lstInfoPictures As List(Of KeyValuePair(Of String, ExcelPicture))) 'AS/19203j

        For Each pair As KeyValuePair(Of String, ExcelPicture) In lstInfoPictures
            Dim pic As ExcelPicture = pair.Value
            Dim picType As String = pair.Key

            Dim position As ExcelDrawingPosition = pic.Position
            Dim picRow As Integer = position.From.Row.Index
            Dim picCol As Integer = position.From.Column.Index

            Select Case picType
                Case "htm"
                    Dim picW As Integer = 288
                    Dim picH As Integer = 175
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

    Public Property SelectedUsersAndGroupsIDs As List(Of Integer) 'AS/19203c
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

    Public Property StringSelectedUsersAndGroupsIDs As String 'AS/19203c
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

    Private Sub DrawBars(worksheet As ExcelWorksheet, jValue As Double, row As Integer) 'AS/18712za
        Dim intValue As Integer = Math.Abs(CInt(jValue))
        Dim numCellsLBar As Integer = 0
        Dim numCellsRBar As Integer = 0
        Dim midCell As Integer = 10
        Dim endLBar As Integer = 9
        Dim startRBar As Integer = 11
        Dim cellRange As CellRange

        If jValue < 1 Then
            numCellsLBar = intValue
            numCellsRBar = 9 - intValue
        ElseIf jValue > 1 Then
            numCellsLBar = 9 - intValue
            numCellsRBar = intValue
        ElseIf jValue = 1 Then
            numCellsLBar = 4
            numCellsRBar = 4
        End If

        cellRange = worksheet.Cells.GetSubrangeAbsolute(row, midCell - numCellsLBar, row, endLBar)
        cellRange.Merged = True
        cellRange.Style.FillPattern.SetSolid(Color.Blue)

        cellRange = worksheet.Cells.GetSubrangeAbsolute(row, startRBar, row, startRBar + numCellsRBar - 1)
        cellRange.Merged = True
        cellRange.Style.FillPattern.SetSolid(Color.Red)

    End Sub

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
#End Region

#Region "Styles"

    Private Function style_TableTitle() As CellStyle 'AS/18712f
        Dim cStyle = New CellStyle
        cStyle.Font.Name = "Arial"
        cStyle.Font.Weight = ExcelFont.BoldWeight
        cStyle.Font.Color = Color.Black
        cStyle.Font.Size = 18 * 20
        cStyle.FillPattern.SetSolid(GetCellColorFor("table_title")) 'AS/18712w
        cStyle.WrapText = True
        cStyle.HorizontalAlignment = HorizontalAlignmentStyle.Center
        cStyle.VerticalAlignment = VerticalAlignmentStyle.Center
        Return cStyle
    End Function

    Private Function style_TableBodyNormal() As CellStyle 'AS/18712n
        Dim cStyle = New CellStyle
        cStyle.Font.Name = "Arial"
        cStyle.Font.Weight = ExcelFont.NormalWeight
        cStyle.Font.Color = Color.Black
        cStyle.Font.Size = 9 * 20
        cStyle.WrapText = True 'WATCH! -- for one-line texts it causes an empty next line when doing Column.AutoFit
        cStyle.HorizontalAlignment = HorizontalAlignmentStyle.Left
        cStyle.VerticalAlignment = VerticalAlignmentStyle.Top

        cStyle.FillPattern.SetSolid(GetCellColorFor("table_body")) 'AS/18712w
        Return cStyle
    End Function

    Private Function style_TableBodyBold() As CellStyle 'AS/18712f
        Dim cStyle = New CellStyle
        cStyle.Font.Name = "Arial"
        cStyle.Font.Weight = ExcelFont.BoldWeight
        cStyle.Font.Color = Color.Black
        cStyle.Font.Size = 14 * 20
        cStyle.WrapText = True
        cStyle.HorizontalAlignment = HorizontalAlignmentStyle.Left
        cStyle.VerticalAlignment = VerticalAlignmentStyle.Top

        cStyle.FillPattern.SetSolid(GetCellColorFor("table_body")) 'AS/18712w
        Return cStyle
    End Function

    Private Function style_RowsGroupName() As CellStyle 'AS/18712ya
        Dim cStyle = New CellStyle
        cStyle.Font.Name = "Arial"
        cStyle.Font.Weight = ExcelFont.NormalWeight
        cStyle.Font.Color = Color.Black
        cStyle.WrapText = True
        cStyle.HorizontalAlignment = HorizontalAlignmentStyle.Left
        cStyle.VerticalAlignment = VerticalAlignmentStyle.Center

        cStyle.FillPattern.SetSolid(GetCellColorFor("rows_group"))
        Return cStyle
    End Function

    Private Function style_ColumnHeader(Optional keepIndent As Boolean = False) As CellStyle 'AS/18712f 'AS/18712yb added keepIndent
        Dim cStyle = New CellStyle
        cStyle.Font.Name = "Arial"
        cStyle.Font.Weight = ExcelFont.BoldWeight
        cStyle.Font.Color = Color.White
        cStyle.WrapText = True
        If Not keepIndent Then 'AS/18712yb enclosed
            cStyle.HorizontalAlignment = HorizontalAlignmentStyle.Center
        Else
            cStyle.HorizontalAlignment = HorizontalAlignmentStyle.Left 'AS/5-6-20 added Else part
        End If
        cStyle.VerticalAlignment = VerticalAlignmentStyle.Center

        cStyle.FillPattern.SetSolid(GetCellColorFor("column_header")) 'AS/18712w
        Return cStyle
    End Function

    Private Function style_ColumnHeaderBW(Optional keepIndent As Boolean = False) As CellStyle
        Dim cStyle = New CellStyle
        cStyle.Font.Name = "Arial"
        cStyle.Font.Weight = ExcelFont.BoldWeight
        cStyle.Font.Color = Color.Black
        cStyle.WrapText = True
        If Not keepIndent Then 'AS/18712yb enclosed
            cStyle.HorizontalAlignment = HorizontalAlignmentStyle.Center
        Else
            cStyle.HorizontalAlignment = HorizontalAlignmentStyle.Left
        End If
        cStyle.VerticalAlignment = VerticalAlignmentStyle.Center

        cStyle.FillPattern.SetSolid(ecColors.ecGray6)
        Return cStyle
    End Function

    Private Sub ShadeAlternateRows(worksheet As ExcelWorksheet, cellRange As CellRange) 'AS/18712y create cellRange before calling the sub
        'Private Sub ShadeAlternateRows(worksheet As ExcelWorksheet, startRow As Integer, startCol As Integer, endRow As Integer, endCol As Integer) 'AS/18712y

        Dim color1 As Color, color2 As Color 'AS/18712w
        GetShadeColors(color1, color2) 'AS/18712w
        With worksheet
            'Dim cellRange = .Cells.GetSubrangeAbsolute(startRow, startCol, endRow, endCol)
            .ConditionalFormatting.AddFormula(cellRange.Name, "MOD(ROW(),2)=0").
                Style.FillPattern.PatternBackgroundColor = color2 'SpreadsheetColor.FromName(shade1)
            .ConditionalFormatting.AddFormula(cellRange.Name, "MOD(ROW(),2)=1").
                Style.FillPattern.PatternBackgroundColor = color1 'SpreadsheetColor.FromName(shade2)
        End With
    End Sub

    Private Sub GetShadeColors(ByRef color1 As Color, ByRef color2 As Color) 'AS/18712w
        Select Case ColorScheme
            Case "green"
                color1 = ecColors.ecGreen6
                color2 = ecColors.ecGreen3
            Case "blue"
                color1 = ecColors.ecBlue2
                color2 = ecColors.ecBlue1
            Case "orange"
                color1 = ecColors.ecOrange2
                color2 = ecColors.ecOrange1
            Case "gray"
                color1 = ecColors.ecGray9
                color2 = ecColors.ecGray7
        End Select
    End Sub

    Private Function GetCellColorFor(tableElement As String) As Color 'AS/18712w
        Dim cellColor As Color
        Select Case tableElement
            Case "table_title"
                Select Case ColorScheme
                    Case "green"
                        cellColor = ecColors.ecGreen5
                    Case "blue"
                        cellColor = ecColors.ecBlue1
                    Case "orange"
                        cellColor = ecColors.ecOrange1
                    Case "gray"
                        cellColor = ecColors.ecGray3
                End Select

            Case "column_header"
                Select Case ColorScheme
                    Case "green"
                        cellColor = ecColors.ecGreen8_Prim
                    Case "blue"
                        cellColor = ecColors.ecBlue3_Prim
                    Case "orange"
                        cellColor = ecColors.ecOrange3_Prim
                    Case "gray"
                        cellColor = ecColors.ecGray11
                End Select

            Case "table_body"
                Select Case ColorScheme
                    Case "green"
                        cellColor = ecColors.ecGreen1
                    Case "blue"
                        cellColor = ecColors.ecBlue1
                    Case "orange"
                        cellColor = ecColors.ecWhite
                    Case "gray"
                        cellColor = ecColors.ecGray2
                End Select

            Case "rows_group" 'AS/18712ya
                Select Case ColorScheme
                    Case "green"
                        cellColor = ecColors.ecGreen4
                    Case "blue"
                        cellColor = ecColors.ecBlue2
                    Case "orange"
                        cellColor = ecColors.ecWhite
                    Case "gray"
                        cellColor = ecColors.ecGray5
                End Select
        End Select

        Return cellColor

    End Function
#End Region

#Region "Examples, tests, etc."

    Private Function Test_AddBorderToImage(Options As ReportGeneratorOptions, server As HttpServerUtility) As ExcelWorksheet 'AS/19486b 
        Dim worksheet = workbook.Worksheets.Add("Alternatives")

        Dim picName As String = "Sens-Objectives.png"
        Dim pic = worksheet.Pictures.Add(server.MapPath("~/Images/favicon/" & picName), "B3")

        Dim picPosition = pic.Position
        picPosition.Mode = PositioningMode.MoveAndSize
        picPosition.To = New AnchorCell(picPosition.To.Column, picPosition.To.Row, 0, 0)

        Dim cellRange = worksheet.Cells.GetSubrangeAbsolute(
        picPosition.From.Row.Index,
        picPosition.From.Column.Index,
        picPosition.To.Row.Index - 1,
        picPosition.To.Column.Index - 1)

        cellRange.Style.Borders.SetBorders(MultipleBorders.Outside, System.Drawing.Color.Black, LineStyle.Thick)

        Return worksheet
    End Function

    Private Function SpreadsheetTableStyles() As List(Of ExcelWorksheet)

        Dim worksheets1 As New List(Of ExcelWorksheet)
        Dim counter As Integer = 0

        For tStyle As BuiltInTableStyleName = 0 To BuiltInTableStyleName.TableStyleDark11
            Dim worksheet1 = workbook.Worksheets.Add(tStyle.ToString)

            ' Add some data.
            Dim data(,) = New Object(4, 2) _
        {
            {"Worker", "Hours", "Price"},
            {"John Doe", 25, 35.0},
            {"Jane Doe", 27, 35.0},
            {"Jack White", 18, 32.0},
            {"George Black", 31, 35.0}
        }

            For i As Integer = 0 To 4
                For j As Integer = 0 To 2
                    worksheet1.Cells.Item(i, j).Value = data(i, j)
                Next
            Next

            ' Set column widths and formats.
            worksheet1.Columns(0).SetWidth(200, LengthUnit.Pixel)
            worksheet1.Columns(1).SetWidth(150, LengthUnit.Pixel)
            worksheet1.Columns(2).SetWidth(150, LengthUnit.Pixel)
            worksheet1.Columns(3).SetWidth(150, LengthUnit.Pixel)
            worksheet1.Columns(2).Style.NumberFormat = """$""#,##0.00"
            worksheet1.Columns(3).Style.NumberFormat = """$""#,##0.00"

            ' Create table And enable totals row.
            Dim table = worksheet1.Tables.Add("Table" & counter.ToString, "A1:C5", True)

            ' Set table style.
            'table.BuiltInStyle = BuiltInTableStyleName.TableStyleMedium2
            table.BuiltInStyle = tStyle

            worksheets1.Add(worksheet1)
            counter += 1
        Next
        Return worksheets1

    End Function

    Private Function GemboxSpreadsheetColors() As ExcelWorksheet
        Dim worksheet1 = workbook.Worksheets.Add("Gembox Spreadsheet Colors")
        For r As ColorName = 0 To ColorName.Purple
            worksheet1.Cells(r, 0).Value = r.ToString
            worksheet1.Cells(r, 1).Value = SpreadsheetColor.FromName(r).ToString
            worksheet1.Cells(r, 2).Style.FillPattern.SetSolid(SpreadsheetColor.FromName(r))
        Next
        Return worksheet1
    End Function

    Private Function ECColorsGuide() As ExcelWorksheet
        Dim worksheet1 = workbook.Worksheets.Add("EC Colors")

        With worksheet1
            'Blues
            .Cells(0, 0).Value = "$alabster"
            .Cells(0, 1).Style.FillPattern.SetSolid(ecColors.ecBlue0)
            .Cells(1, 0).Value = "$ec-blue-selected"
            .Cells(1, 1).Style.FillPattern.SetSolid(ecColors.ecBlue0)
            .Cells(2, 0).Value = "$light-blue"
            .Cells(2, 1).Style.FillPattern.SetSolid(ecColors.ecBlue1)
            .Cells(3, 0).Value = "$ec-blue-lite"
            .Cells(3, 1).Style.FillPattern.SetSolid(ecColors.ecBlue3_Prim)
            .Cells(4, 0).Value = "$cornflower_blue"
            .Cells(4, 1).Style.FillPattern.SetSolid(ecColors.ecBlue4)
            .Cells(5, 0).Value = "$mariner"
            .Cells(5, 1).Style.FillPattern.SetSolid(ecColors.ecBlue5)
            .Cells(6, 0).Value = "$ec-blue-normal"
            .Cells(6, 1).Style.FillPattern.SetSolid(ecColors.ecBlue6_Prim)
            .Cells(7, 0).Value = "$ec-blue-hover"
            .Cells(7, 1).Style.FillPattern.SetSolid(ecColors.ecBlue7)
            .Cells(8, 0).Value = "$ec-blue-dark"
            .Cells(8, 1).Style.FillPattern.SetSolid(ecColors.ecBlue8)

            'Greens
            .Cells(1, 3).Value = "$limed-ash"
            .Cells(1, 4).Style.FillPattern.SetSolid(ecColors.ecGreen1)
            .Cells(2, 3).Value = "$saltpan"
            .Cells(2, 4).Style.FillPattern.SetSolid(ecColors.ecGreen2)
            .Cells(3, 3).Value = "$sharp"
            .Cells(3, 4).Style.FillPattern.SetSolid(ecColors.ecGreen3)
            .Cells(4, 3).Value = "$zanah"
            .Cells(4, 4).Style.FillPattern.SetSolid(ecColors.ecGreen4)
            .Cells(5, 3).Value = "$surf-crest"
            .Cells(5, 4).Style.FillPattern.SetSolid(ecColors.ecGreen5)
            .Cells(6, 3).Value = "$sea-mist"
            .Cells(6, 4).Style.FillPattern.SetSolid(ecColors.ecGreen6)
            .Cells(7, 3).Value = "$ec-green-lite"
            .Cells(7, 4).Style.FillPattern.SetSolid(ecColors.ecGreen7)
            .Cells(8, 3).Value = "$ec-green-normal"
            .Cells(8, 4).Style.FillPattern.SetSolid(ecColors.ecGreen8_Prim)
            .Cells(9, 3).Value = "$ec-green-hover"
            .Cells(9, 4).Style.FillPattern.SetSolid(ecColors.ecGreen9)
            .Cells(10, 3).Value = "$ec-green-dark"
            .Cells(10, 4).Style.FillPattern.SetSolid(ecColors.ecGreen10)
            .Cells(11, 3).Value = ""
            .Cells(12, 3).Value = "$ec-blue-green"
            .Cells(12, 4).Style.FillPattern.SetSolid(ecColors.ecBlueGreen)
            .Cells(13, 3).Value = "$aqua"
            .Cells(13, 4).Style.FillPattern.SetSolid(ecColors.ecGreenBlue)

            'Reds
            .Cells(1, 5).Value = "$ec-red-lit"
            .Cells(1, 6).Style.FillPattern.SetSolid(ecColors.ecRed1)
            .Cells(2, 5).Value = "$ec-red-normal"
            .Cells(2, 6).Style.FillPattern.SetSolid(ecColors.ecRed2)
            .Cells(3, 5).Value = "$ec-red-hover"
            .Cells(3, 6).Style.FillPattern.SetSolid(ecColors.ecRed3)
            .Cells(4, 5).Value = "$ec-red-dark"
            .Cells(4, 6).Style.FillPattern.SetSolid(ecColors.ecRed4)

            'Oranges
            .Cells(1, 7).Value = "$ec-orange-lite"
            .Cells(1, 8).Style.FillPattern.SetSolid(ecColors.ecOrange1)
            .Cells(2, 7).Value = "$ec-orange-normal"
            .Cells(2, 8).Style.FillPattern.SetSolid(ecColors.ecOrange3_Prim)
            .Cells(3, 7).Value = "$ec-orange-hover"
            .Cells(3, 8).Style.FillPattern.SetSolid(ecColors.ecOrange2)
            .Cells(4, 7).Value = "$ec-orange-dark"
            .Cells(4, 8).Style.FillPattern.SetSolid(ecColors.ecOrange4)

            'Grays
            .Cells(0, 9).Value = "$white"
            .Cells(0, 10).Style.FillPattern.SetSolid(ecColors.ecWhite)
            .Cells(1, 9).Value = "$ghost"
            .Cells(1, 10).Style.FillPattern.SetSolid(ecColors.ecGray1)
            .Cells(2, 9).Value = "$snow"
            .Cells(2, 10).Style.FillPattern.SetSolid(ecColors.ecGray2)
            .Cells(3, 9).Value = "$vapor"
            .Cells(3, 10).Style.FillPattern.SetSolid(ecColors.ecGray3)
            .Cells(4, 9).Value = "$white-smoke"
            .Cells(4, 10).Style.FillPattern.SetSolid(ecColors.ecGray4)
            .Cells(5, 9).Value = "$gallery"
            .Cells(5, 10).Style.FillPattern.SetSolid(ecColors.ecGray5)
            .Cells(6, 9).Value = "$smoke"
            .Cells(6, 10).Style.FillPattern.SetSolid(ecColors.ecGray6)
            .Cells(7, 9).Value = "$silver"
            .Cells(7, 10).Style.FillPattern.SetSolid(ecColors.ecGray7)
            .Cells(8, 9).Value = "$mercury"
            .Cells(8, 10).Style.FillPattern.SetSolid(ecColors.ecGray8)
            .Cells(9, 9).Value = "$gainsboro"
            .Cells(9, 10).Style.FillPattern.SetSolid(ecColors.ecGray9)
            .Cells(10, 9).Value = "$iron"
            .Cells(10, 10).Style.FillPattern.SetSolid(ecColors.ecGray10)
            .Cells(11, 9).Value = "$base"
            .Cells(11, 10).Style.FillPattern.SetSolid(ecColors.ecGray11)
            .Cells(12, 9).Value = "$aluminum"
            .Cells(12, 10).Style.FillPattern.SetSolid(ecColors.ecGray12)
            .Cells(13, 9).Value = "$jumbo"
            .Cells(13, 10).Style.FillPattern.SetSolid(ecColors.ecGray13)
            .Cells(14, 9).Value = "$monsoon"
            .Cells(14, 10).Style.FillPattern.SetSolid(ecColors.ecGray14)
            .Cells(15, 9).Value = "$steel"
            .Cells(15, 10).Style.FillPattern.SetSolid(ecColors.ecGray15)
            .Cells(16, 9).Value = "$charcoal"
            .Cells(16, 10).Style.FillPattern.SetSolid(ecColors.ecGray16)
            .Cells(17, 9).Value = "$tuatara"
            .Cells(17, 10).Style.FillPattern.SetSolid(ecColors.ecGray17)
            .Cells(18, 9).Value = "$oil"
            .Cells(18, 10).Style.FillPattern.SetSolid(ecColors.ecGray18)
            .Cells(19, 9).Value = "$jet"
            .Cells(19, 10).Style.FillPattern.SetSolid(ecColors.ecGray19)
            .Cells(20, 9).Value = "$black"
            .Cells(20, 10).Style.FillPattern.SetSolid(ecColors.ecBlack)


            .Columns(0).AutoFit()
            .Columns(3).AutoFit()
            .Columns(5).AutoFit()
            .Columns(7).AutoFit()
            .Columns(9).AutoFit()

        End With
        Return worksheet1
    End Function

    Private Function Test_AutofitReplace() As ExcelWorksheet

        Dim worksheet = workbook.Worksheets.Add("AutoFit Issue")

        Dim objH As clsHierarchy = App.ActiveProject.HierarchyObjectives

        Dim rowTitle As Integer = 0
        Dim colAltName As Integer = 0
        Dim colAltPriority As Integer = 1
        Dim colAltJudgment As Integer = 2
        Dim maxLenObjName As Integer = 10
        Dim maxLenAltName As Integer = 10

        Dim row As Integer = 2 'rowUserName + 1 'row number 
        Dim col As Integer = 0 'col number

        With worksheet
            For Each node As clsNode In objH.Nodes
                ' rows with objectives names
                Dim NodePath As String = ""
                GetFullNodePath(node, NodePath)
                Dim indentVal As Integer = 0

                Dim s As String() = NodePath.Split("|"c)
                For Each nodeName As String In s
                    Debug.Print(nodeName)
                    .Cells(row, colAltName).Value = nodeName.Trim
                    Dim cellRange As CellRange = .Cells.GetSubrangeAbsolute(row, colAltName, row, colAltJudgment)
                    cellRange.Merged = True
                    cellRange.Style = style_ColumnHeader(True)

                    .Cells(row, colAltName).Style.Indent = indentVal * 3
                    indentVal += 1
                    row += 1
                Next
                row += 1
            Next
        End With
        Return worksheet

    End Function

    Private Function Test_SizeImageInCell(server As HttpServerUtility, Options As ReportGeneratorOptions) As ExcelWorksheet 'AS/19203h

        Dim worksheet = workbook.Worksheets.Add("Insert Image and Text")

        worksheet.Rows(0).SetHeight(80, LengthUnit.Point)
        worksheet.Columns(0).SetWidth(100, LengthUnit.Point)

        'Dim picture = worksheet.Pictures.Add(server.MapPath("~/Images/favicon/Dices.png"), "A1")
        Dim picture As ExcelPicture = worksheet.Pictures.Add("D:\ECC Alpha\Application\DocMedia\MHTFiles\2271\alternative_1\media\object0.jpeg", "A1")

        Dim position = picture.Position
        Dim ratio = position.Width / position.Height
        Dim columnWidth = position.From.Column.GetWidth(LengthUnit.Point)
        Dim rowHeight = position.From.Row.GetHeight(LengthUnit.Point)

        If position.Width > columnWidth Then
            position.SetWidth(columnWidth, LengthUnit.Point)
            position.SetHeight(columnWidth / ratio, LengthUnit.Point)
        End If

        If position.Height > rowHeight Then
            position.SetHeight(rowHeight, LengthUnit.Point)
            position.SetWidth(rowHeight * ratio, LengthUnit.Point)
        End If

        Return worksheet

    End Function

    Private Function Test_InsertImageAndText(server As HttpServerUtility, Options As ReportGeneratorOptions) As ExcelWorksheet 'AS/19203a

        Dim worksheet = workbook.Worksheets.Add("Insert Image and Text")

        worksheet.Columns(0).SetWidth(200, LengthUnit.Pixel)
        worksheet.Rows(0).SetHeight(100, LengthUnit.Pixel)

        Dim cell As ExcelCell = worksheet.Cells("A1")
        cell.Value = "Sample text. Example of the layout after inserting picture and text in one cell."

        'Dim picture As ExcelPicture = worksheet.Pictures.Add("Image.png", "A1", "A1")
        Dim picture As ExcelPicture = worksheet.Pictures.Add(server.MapPath("~/Images/favicon/Dices.png"), "A1", "A1")

        ' Set picture on right half of "A1" cell with 20px padding on top And bottom.
        picture.Position.From.SetColumnOffset(100, LengthUnit.Pixel)
        picture.Position.From.SetRowOffset(20, LengthUnit.Pixel)
        picture.Position.To.SetRowOffset(-20, LengthUnit.Pixel)

        Return worksheet

    End Function


#End Region

    Public Sub New(_App As clsComparionCore, GridWRTNodeID As Integer) 'AS/19464a added GridWRTNodeID

        SpreadsheetInfo.SetLicense("SN-2019Oct23-AKAqJAF7VQbNwpzNxMw+YMGUPQKtiac8SV2w3r4AR43N/ctUteUub15BB7pw2rqTqZB6PzdDiG4vHXzKw5SRVIMv9Gw==A")
        ' Create new empty document.
        workbook = New ExcelFile
        App = _App
        PM = App.ActiveProject.ProjectManager
        PM.ProjectName = App.ActiveProject.ProjectName 'AS/18712t
        ecColors = New ECColors 'AS/18712w
        WRTNodeID = GridWRTNodeID 'AS/19464a

    End Sub

End Class
