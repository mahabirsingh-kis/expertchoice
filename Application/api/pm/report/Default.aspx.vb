Imports GemBox.Document
Imports GemBox.Presentation
Imports GemBox.Spreadsheet
Imports GemBox.Spreadsheet.Charts

Partial Public Class ReportWebAPI
    Inherits clsComparionCorePage

    Private Const _TEMPL_LOG_REPORT As String = "#{0} {1}"
    Private Const _TEMPL_LOG_ITEM As String = "#{0}-{1} {2}"

    Private Const SpaceAfterReportPart As Integer = 20

    Private CanEditProject As Boolean = False       ' D6911

    Public Sub New()
        MyBase.New(_PGID_WEBAPI)
    End Sub

    Private Function _Page() As mpWebAPI
        Return CType(Master, mpWebAPI)
    End Function

#Region "Generate Documents"

    ' D6528 ==
    Private Function Doc_AddTitle(ByRef doc As DocumentModel, text As String) As Boolean
        Dim sec = doc.Sections.Last()
        sec.Blocks.Add(New Paragraph(doc, New Run(doc, text) With
                {.CharacterFormat = New CharacterFormat() With {.Bold = True, .AllCaps = True, .Size = 16}}) With
                {
                    .ParagraphFormat = New ParagraphFormat() With
                    {
                        .Alignment = GemBox.Document.HorizontalAlignment.Center,
                        .KeepWithNext = True
                    }
                })
        Return True
    End Function

    Private Function Doc_AddText(ByRef doc As DocumentModel, text As String) As Boolean
        Dim sec = doc.Sections.Last()
        sec.Blocks.Add(New Paragraph(doc, text))
        Return True
    End Function

    Private Function Doc_AddInfodoc(ByRef doc As DocumentModel, InfodocHTMLString As String) As Boolean
        Dim BaseAddress = ApplicationURL(False, False)
        Dim sec = doc.Sections.Last()
        sec.Blocks.Add(New Paragraph(doc, "") With
                {
                    .ParagraphFormat = New ParagraphFormat() With
                    {
                       .SpaceAfter = SpaceAfterReportPart
                    }
                })
        sec.Blocks.Last().Content.Start.LoadText(InfodocHTMLString, New GemBox.Document.HtmlLoadOptions With {.BaseAddress = BaseAddress})
        Return True
    End Function

    Private Function Doc_AddProjectDescription(ByRef doc As DocumentModel) As Boolean
        Dim sContent As String = PM.ProjectDescription
        If sContent <> "" Then sContent = Infodoc_Unpack(App.ActiveProject.ID, PM.ActiveHierarchy, reObjectType.Description, "", sContent, True, True, -1)
        If sContent = "" Then sContent = SafeFormString(App.ActiveProject.Comment).Replace(vbLf, "<br>")
        If sContent <> "" Then
            Doc_AddInfodoc(doc, sContent)
            Doc_AddPageBreak(doc)
        End If
        Return True
    End Function


    Private Function Doc_AddPageBreak(ByRef doc As DocumentModel) As Boolean
        Dim sec = doc.Sections.Last()
        sec.Blocks.Add(New Paragraph(doc, New SpecialCharacter(doc, SpecialCharacterType.PageBreak)))
        Return True
    End Function

    Private Function Doc_AddImage(ByRef doc As DocumentModel, imageFile As String) As Boolean
        Dim fResult As Boolean = False
        Dim sec = doc.Sections.Last()
        If Not String.IsNullOrEmpty(imageFile) Then
            Dim sFile As String = Server.MapPath(imageFile)
            If MyComputer.FileSystem.FileExists(sFile) Then
                Dim pic = New GemBox.Document.Picture(doc, sFile)
                Dim pgSize As Size = New Size(sec.PageSetup.PageWidth - sec.PageSetup.PageMargins.Left - sec.PageSetup.PageMargins.Right, sec.PageSetup.PageHeight - sec.PageSetup.PageMargins.Top - sec.PageSetup.PageMargins.Bottom)
                Dim KX = pic.Layout.Size.Width / pic.Layout.Size.Height
                pic.Layout = New InlineLayout(New Size(pgSize.Width, pgSize.Width / KX)) With {.LockAspectRatio = True}
                sec.Blocks.Add(New Paragraph(doc, pic) With
                {
                    .ParagraphFormat = New ParagraphFormat() With
                    {
                       .SpaceAfter = SpaceAfterReportPart
                    }
                })
                fResult = True
            End If
        End If
        Return fResult
    End Function

    Private Function Doc_AddHierarchy(ByRef doc As DocumentModel) As Boolean
        Dim section = doc.Sections.Last()

        ' Create number list style.
        Dim numberList As New ListStyle(ListTemplateType.NumberWithDot)

        ' Customize list level formats.
        For level = 0 To numberList.ListLevelFormats.Count - 1
            Dim levelFormat As ListLevelFormat = numberList.ListLevelFormats(level)
            levelFormat.ParagraphFormat.NoSpaceBetweenParagraphsOfSameStyle = True
            levelFormat.Alignment = GemBox.Document.HorizontalAlignment.Left
            levelFormat.NumberStyle = NumberStyle.Decimal
            levelFormat.NumberPosition = 16 * level
            levelFormat.NumberFormat = String.Concat(Enumerable.Range(1, level + 1).Select(Function(i) $"%{i}.")) + " "
        Next

        ' Create number list items.
        For Each obj As Tuple(Of Integer, Integer, clsNode) In App.ActiveProject.ProjectManager.ActiveObjectives.NodesInLinearOrder
            Dim paragraph As New Paragraph(doc, obj.Item3.NodeName)
            paragraph.ListFormat.Style = numberList
            paragraph.ListFormat.ListLevelNumber = obj.Item3.Level
            section.Blocks.Add(paragraph)
        Next
        section.Blocks.Add(New Paragraph(doc, "") With
                {
                    .ParagraphFormat = New ParagraphFormat() With
                    {
                       .SpaceAfter = SpaceAfterReportPart
                    }
                })
        Return True
    End Function

    Private Function Doc_AddAlternativesList(ByRef doc As DocumentModel) As Boolean
        Dim section = doc.Sections.Last()
        ' Create number list style.
        Dim numberList As New ListStyle(ListTemplateType.BulletCircle)
        numberList.ListLevelFormats(0).ParagraphFormat.NoSpaceBetweenParagraphsOfSameStyle = True
        ' Create number list items.
        For Each alt As clsNode In PM.ActiveAlternatives.Nodes
            Dim paragraph As New Paragraph(doc, alt.NodeName)
            paragraph.ListFormat.Style = numberList
            section.Blocks.Add(paragraph)
        Next
        section.Blocks.Add(New Paragraph(doc, "") With
                {
                    .ParagraphFormat = New ParagraphFormat() With
                    {
                       .SpaceAfter = SpaceAfterReportPart
                    }
                })
        Return True
    End Function

    Private Function Doc_AddDatagrid(ByRef doc As DocumentModel, Optional CombinedUserID As Integer = -1) As Boolean
        Dim decimals As Integer = 2
        Dim section As New Section(doc)
        doc.Sections.Add(section)

        'calc priorities for Goal and userID=-1 (Combined)
        Dim wrtNode As clsNode = PM.ActiveObjectives.RootNodes(0)

        Dim CalcTarget As clsCalculationTarget
        PM.StorageManager.Reader.LoadUserData(PM.GetUserByID(CombinedUserID))
        Dim UserName As String = ""
        If IsCombinedUserID(CombinedUserID) Then
            Dim aGroup As clsCombinedGroup = PM.CombinedGroups.GetCombinedGroupByUserID(CombinedUserID)
            UserName = aGroup.Name + " Group"
            CalcTarget = New clsCalculationTarget(CalculationTargetType.cttCombinedGroup, aGroup)
        Else
            Dim aUser As clsUser = PM.GetUserByID(CombinedUserID)
            UserName = aUser.UserName
            CalcTarget = New clsCalculationTarget(CalculationTargetType.cttUser, aUser)
        End If
        Dim JA As clsJudgmentsAnalyzer = New clsJudgmentsAnalyzer(ECSynthesisMode.smDistributive, PM)
        Dim CanShowResults = If(CalcTarget.GetUserID >= 0, JA.CanShowIndividualResults(CalcTarget.GetUserID, wrtNode), Not (Not PM.UsersRoles.IsAllowedObjective(wrtNode.NodeGuidID, CalcTarget.GetUserID) AndAlso Not PM.CalculationsManager.UseCombinedForRestrictedNodes))
        If CanShowResults Then
            PM.CalculationsManager.Calculate(CalcTarget, wrtNode, PM.ActiveHierarchy, PM.ActiveAltsHierarchy)

            Dim table As New GemBox.Document.Tables.Table(doc)
            table.TableFormat.AutomaticallyResizeToFitContents = False

            Dim row As New GemBox.Document.Tables.TableRow(doc)
            'table.Rows.Add(row)

            Dim objH As clsHierarchy = App.ActiveProject.HierarchyObjectives
            Dim altH As clsHierarchy = App.ActiveProject.HierarchyAlternatives
            Dim columnCount As Integer = objH.Nodes.Count + 1
            Dim rowCount As Integer = altH.Nodes.Count

            Dim secDatagrid = doc.Sections.Last()
            Dim pageSetup = secDatagrid.PageSetup
            pageSetup.Orientation = GemBox.Document.Orientation.Landscape
            pageSetup.PaperType = GemBox.Document.PaperType.A4
            pageSetup.PageMargins.Left = 20
            pageSetup.PageMargins.Right = 20
            pageSetup.PageMargins.Top = 10
            pageSetup.PageMargins.Bottom = 10

            Dim colAltWidth As Integer = 150 'AS/17533p===
            Dim colTotalWidth As Integer = 40
            Dim colAttributeWidth As Integer = 100 'AS/17533p==
            Dim colCovobjWidth As Integer = CInt(Math.Round((pageSetup.PageWidth - colAltWidth - colTotalWidth - pageSetup.PageMargins.Left - pageSetup.PageMargins.Right) / objH.TerminalNodes.Count))

            'Create row with Titles
            row = New GemBox.Document.Tables.TableRow(doc)
            table.Rows.Add(row)
            'create cell for "Alternatives", merge it with the cell below, center, and set backcolor
            Dim para As New Paragraph(doc, New Run(doc, "Alternatives") With {.CharacterFormat = New CharacterFormat With {.Bold = True}}) With {
                            .ParagraphFormat = New ParagraphFormat() With {
                            .Alignment = GemBox.Document.HorizontalAlignment.Center}}
            row.Cells.Add(New GemBox.Document.Tables.TableCell(doc, para) With {.RowSpan = 2, .CellFormat = New GemBox.Document.Tables.TableCellFormat() With {
                            .VerticalAlignment = GemBox.Document.VerticalAlignment.Center,
                            .BackgroundColor = GemBox.Document.Color.LightGray,
                            .PreferredWidth = New GemBox.Document.Tables.TableWidth(colAltWidth, GemBox.Document.Tables.TableWidthUnit.Point)
                             }})

            'create cell for "Total", merge it with the cell below, center, and set backcolor
            para = New Paragraph(doc, New Run(doc, "Total") With {.CharacterFormat = New CharacterFormat With {.Bold = True}})
            row.Cells.Add(New GemBox.Document.Tables.TableCell(doc, para) With {.RowSpan = 2, .CellFormat = New GemBox.Document.Tables.TableCellFormat() With {
                            .VerticalAlignment = GemBox.Document.VerticalAlignment.Center,
                            .BackgroundColor = GemBox.Document.Color.LightGray,
                            .PreferredWidth = New GemBox.Document.Tables.TableWidth(colTotalWidth, GemBox.Document.Tables.TableWidthUnit.Point)
                             }})

            'create cell for model name, merge it with the cells all way down to the right, set alignments and backcolor
            para = New Paragraph(doc, New Run(doc, PM.ActiveObjectives.RootNodes(0).NodeName) With {.CharacterFormat = New CharacterFormat With {.Bold = True}}) With {
                            .ParagraphFormat = New ParagraphFormat() With {
                            .Alignment = GemBox.Document.HorizontalAlignment.Center}}
            row.Cells.Add(New GemBox.Document.Tables.TableCell(doc, para) With {
                        .ColumnSpan = objH.TerminalNodes.Count, .CellFormat = New GemBox.Document.Tables.TableCellFormat() With {
                        .VerticalAlignment = GemBox.Document.VerticalAlignment.Center,
                        .BackgroundColor = GemBox.Document.Color.LightGray
                         }})

            row = New GemBox.Document.Tables.TableRow(doc)
            table.Rows.Add(row)

            'create cells for covering objectives' names
            For Each node In objH.TerminalNodes
                para = New Paragraph(doc, New Run(doc, node.NodeName) With {.CharacterFormat = New CharacterFormat With {.Bold = True}}) With {
                                        .ParagraphFormat = New ParagraphFormat() With {
                                        .Alignment = GemBox.Document.HorizontalAlignment.Center}}
                row.Cells.Add(New GemBox.Document.Tables.TableCell(doc, para) With {
                                        .CellFormat = New GemBox.Document.Tables.TableCellFormat() With {
                                        .VerticalAlignment = GemBox.Document.VerticalAlignment.Center,
                                        .BackgroundColor = GemBox.Document.Color.LightGray,
                                        .PreferredWidth = New GemBox.Document.Tables.TableWidth(colCovobjWidth, GemBox.Document.Tables.TableWidthUnit.Point)
                                         }})
            Next


            'create rows for alternatives
            For i As Integer = 0 To altH.Nodes.Count - 1
                'add alts names to the first col
                Dim alt = altH.Nodes(i)
                row = New GemBox.Document.Tables.TableRow(doc)
                table.Rows.Add(row)
                para = New Paragraph(doc, alt.NodeName)
                row.Cells.Add(New GemBox.Document.Tables.TableCell(doc, para))

                para = New Paragraph(doc, (Math.Round(alt.UnnormalizedPriority, decimals)).ToString)
                row.Cells.Add(New GemBox.Document.Tables.TableCell(doc, para))

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
                                        sValue = JS_SafeNumber(Math.Round(currJ.Rating.Value, decimals)) 'AS/17533u inserted Rouns function
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
                                        sValue = JS_SafeNumber(Math.Round(aVal, decimals)) 'AS/17533u inserted Rouns function
                                    End If
                                Catch
                                    sValue = " "
                                End Try
                            Case Else 'for PW 
                                Dim aVal As Double = node.Judgments.Weights.GetUserWeights(-1, ECSynthesisMode.smIdeal, PM.CalculationsManager.IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(alt.NodeID)
                                aVal = Math.Round(aVal, decimals) 'AS/17533u
                                sValue = aVal.ToString
                        End Select

                        para = New Paragraph(doc, sValue)
                        row.Cells.Add(New GemBox.Document.Tables.TableCell(doc, para))
                    End If
                Next
            Next i

            Doc_AddTitle(doc, "Datagrid for " + UserName)
            secDatagrid.Blocks.Add(table)
        End If
        Return True
    End Function

    ' D6521 ===
    Private Sub DOC_Download(tReport As clsReport, Optional format As String = "pdf")   ' D6501 + D6528
        If tReport Is Nothing Then Exit Sub ' D6528
        ' Create an empty document
        Dim Doc As New DocumentModel
        Dim section As New Section(Doc)
        Doc.Sections.Add(section)
        Dim pageSetup = section.PageSetup
        pageSetup.PageMargins.Top = 30
        pageSetup.PageMargins.Bottom = 30

        'Fill document with sections
        Dim AddedItems As Integer = 0
        For Each tPair As KeyValuePair(Of Integer, clsReportItem) In tReport.Items
            Dim tItem As clsReportItem = tPair.Value
            With tItem
                If Not .Disabled Then
                    Dim fAdded As Boolean = False
                    Select Case .ItemType

                        Case ecReportItemType.Alternatives.ToString
                            If PRJ.HierarchyAlternatives.Nodes.Count > 0 Then
                                Doc_AddTitle(Doc, ParseString("%%Alternatives%% List"))
                                Doc_AddAlternativesList(Doc)
                                fAdded = True
                            End If

                        Case ecReportItemType.AlternativesChart.ToString
                            'Dim img As String = GetParam(_Page.Params, "img", True)
                            'If Not String.IsNullOrEmpty(img) Then
                            '    If AddedItems > 0 AndAlso .StartWithPageBreak Then AddPageBreak(Doc)
                            '    AddTitle(Doc, ParseString("%%Alternatives%% Chart"))
                            '    AddImage(Doc, img)
                            '    fAdded = True
                            'End If

                        Case ecReportItemType.DataGrid.ToString
                            Doc_AddDatagrid(Doc, -1)
                            fAdded = True

                        Case ecReportItemType.EvalProgress.ToString

                        Case ecReportItemType.ModelDescription.ToString
                            Doc_AddTitle(Doc, App.ActiveProject.ProjectName)
                            Doc_AddProjectDescription(Doc)
                            fAdded = True

                        Case ecReportItemType.Objectives.ToString
                            If PRJ.HierarchyObjectives.Nodes.Count > 1 Then
                                Doc_AddTitle(Doc, ParseString("%%Objectives%% Hierarchy"))
                                Doc_AddHierarchy(Doc)
                                fAdded = True
                            End If

                        Case ecReportItemType.PageBreak.ToString     ' D6525
                            If AddedItems > 0 Then Doc_AddPageBreak(Doc)    ' D6525

                    End Select

                    If fAdded Then AddedItems += 1
                End If
            End With
        Next

        Response.AppendCookie(New HttpCookie("dl_token", CheckVar("t", "")))

        If AddedItems = 0 Then
            Doc_AddTitle(Doc, App.ActiveProject.ProjectName)
            Doc_AddText(Doc, "Nothing to export in this report.")
        End If

        ' Start saving document
        ' D6501 ===
        Dim sFName As String = SafeFileName(GetProjectFileName(App.ActiveProject.ProjectName, "Report", "", "." + If(format = "", "pdf", format)))
        App.DBSaveLog(dbActionType.actDownload, dbObjectType.einfProjectReport, App.ProjectID, String.Format("Download file ({0})", dbObjectType.einfProjectReport.ToString.Substring(4)), String.Format("Filename: {0}", sFName))
        Doc.Save(Response, sFName)
        ' D6501 + D6522 ==
    End Sub
    ' D6493 + D6521 ==

#End Region

#Region "Generate spreadsheet"

    ' D6528 ===
    Private Sub XLS_Download(tReport As clsReport, Optional format As String = "xlsx")
        If tReport Is Nothing Then Exit Sub
        ' Create an empty xls
        Dim Doc As New ExcelFile
        Doc.Worksheets.Add("Page 1")

        'Fill document with sections
        Dim AddedItems As Integer = 0
        For Each tPair As KeyValuePair(Of Integer, clsReportItem) In tReport.Items
            Dim worksheet As ExcelWorksheet = Doc.Worksheets.Last
            Dim tItem As clsReportItem = tPair.Value
            With tItem
                If Not .Disabled Then
                    Dim fAdded As Boolean = False
                    Select Case .ItemType

                        Case ecReportItemType.PageBreak.ToString
                            'If AddedItems > 0 Then Xls_AddSheet(Doc)

                    End Select

                    If fAdded Then AddedItems += 1
                End If
            End With
        Next

        Response.AppendCookie(New HttpCookie("dl_token", CheckVar("t", "")))

        If AddedItems = 0 Then
            'xls_AddTitle(Doc, App.ActiveProject.ProjectName)
            'xls_AddText(Doc, "Nothing to export in this report.")
        End If

        ' Start saving spreadsheet
        Dim sFName As String = SafeFileName(GetProjectFileName(App.ActiveProject.ProjectName, "Report", "", "." + If(format = "", "xlsx", format)))
        App.DBSaveLog(dbActionType.actDownload, dbObjectType.einfProjectReport, App.ProjectID, String.Format("Download file ({0})", dbObjectType.einfProjectReport.ToString.Substring(4)), String.Format("Filename: {0}", sFName))
        Doc.Save(Response, sFName)
    End Sub
    ' D6528 ==

#End Region

#Region "Generate spreadsheet"

    ' D6528 ===
    Private Sub PPT_Download(tReport As clsReport, Optional format As String = "xlsx")
        If tReport Is Nothing Then Exit Sub
        ' Create an empty xls
        Dim Doc As New PresentationDocument

        'Fill document with sections
        Dim AddedItems As Integer = 0
        For Each tPair As KeyValuePair(Of Integer, clsReportItem) In tReport.Items
            Dim tItem As clsReportItem = tPair.Value
            With tItem
                If Not .Disabled Then
                    Dim fAdded As Boolean = False
                    Select Case .ItemType

                        Case ecReportItemType.PageBreak.ToString
                            'If AddedItems > 0 Then Ppt_AddSheet(Doc)

                    End Select

                    If fAdded Then AddedItems += 1
                End If
            End With
        Next

        Response.AppendCookie(New HttpCookie("dl_token", CheckVar("t", "")))

        If AddedItems = 0 Then
            'ppt_AddTitle(Doc, App.ActiveProject.ProjectName)
            'ppt_AddText(Doc, "Nothing to export in this report.")
        End If

        ' Start saving spreadsheet
        Dim sFName As String = SafeFileName(GetProjectFileName(App.ActiveProject.ProjectName, "Report", "", "." + If(format = "", "pptx", format)))
        App.DBSaveLog(dbActionType.actDownload, dbObjectType.einfProjectReport, App.ProjectID, String.Format("Download file ({0})", dbObjectType.einfProjectReport.ToString.Substring(4)), String.Format("Filename: {0}", sFName))
        Doc.Save(Response, sFName)
    End Sub
    ' D6528 ==

#End Region

#Region "Working with Reports"

    ' D6521 ===
    Public Function Add_Report(ReportType As ecReportType, Name As String, Comment As String, Optional Options As String = "") As jActionResult ' D6575
        If Not CanEditProject Then FetchNoPermissions() ' D6911
        Dim tReport As clsReport = PM.Reports.AddReport(ReportType, Name, Comment)
        If tReport Is Nothing Then
            Return New jActionResult With {
                .Result = ecActionResult.arError,
                .Message = "Unable to add report"
            }
        Else
            ParseOptions(tReport.Options, Options)  ' D6575
            ' D6605 ===
            Dim sLayout As String = GetParam(_Page.Params, "layout", True)
            If Not String.IsNullOrEmpty(sLayout) Then Import_Items(tReport, sLayout)
            ' D6605 ==
            PRJ.SaveProjectOptions("Add " + getReportTypeName(tReport),,, String.Format(_TEMPL_LOG_REPORT, tReport.ID, Name))   ' D6585
            Return New jActionResult With {
                .Result = ecActionResult.arSuccess,
                .ObjectID = tReport.ID,
                .Data = tReport.Serialize()
            }
        End If
    End Function

    Public Function Edit_Report(ID As Integer, Name As String, Comment As String, Optional Options As String = "") As jActionResult ' D6575
        If Not CanEditProject Then FetchNoPermissions() ' D6911
        If Not PM.Reports.Reports.ContainsKey(ID) Then _Page.FetchWrongObject()
        With PM.Reports.Reports(ID)
            .Name = Name
            .Comment = Comment
            ParseOptions(.Options, Options) ' D6575
        End With
        PRJ.SaveProjectOptions("Edit" + getReportTypeName(PM.Reports.Reports(ID)),,, String.Format(_TEMPL_LOG_REPORT, ID, Name)) ' D6585
        Return New jActionResult With {
            .Result = ecActionResult.arSuccess,
            .ObjectID = ID,
            .Data = PM.Reports.Reports(ID).Serialize()
        }
    End Function

    Public Function Clone_Report(ID As Integer, Name As String, Comment As String, Optional Options As String = "") As jActionResult    ' D6575
        If Not CanEditProject Then FetchNoPermissions() ' D6911
        If Not PM.Reports.Reports.ContainsKey(ID) Then _Page.FetchWrongObject()
        Dim tReport As clsReport = PM.Reports.CloneReport(ID)
        If tReport Is Nothing Then
            Return New jActionResult With {
                .Result = ecActionResult.arError,
                .Message = "Unable to clone report"
            }
        Else
            tReport.Name = Name
            tReport.Comment = Comment
            ParseOptions(tReport.Options, Options)  ' D6575
            PRJ.SaveProjectOptions("Add " + getReportTypeName(tReport),,, String.Format(_TEMPL_LOG_REPORT + ", cloned from ", tReport.ID, Name) + String.Format(_TEMPL_LOG_REPORT + ", cloned from ", ID, PM.Reports.Reports(ID).Name)) ' D6585
            ' D7401 ===
            Dim fDoUpdate As Boolean = False
            For Each itemID As Integer In tReport.Items.Keys
                If tReport.Items(itemID).ItemType = ecReportItemType.Infodoc.ToString Then
                    cloneItemInfodoc(ID, itemID, tReport.ID, itemID, False)
                    fDoUpdate = True
                End If
            Next
            If fDoUpdate Then PRJ.SaveStructure("", False, False, "")
            ' D7401 ==
            Return New jActionResult With {
                .Result = ecActionResult.arSuccess,
                .ObjectID = tReport.ID,
                .Data = tReport.Serialize()
            }
        End If
    End Function

    Public Function Delete_Report(ID As Integer) As jActionResult
        If Not CanEditProject Then FetchNoPermissions() ' D6911
        With PM.Reports
            If Not .Reports.ContainsKey(ID) Then _Page.FetchWrongObject()
            Dim sName As String = .Reports(ID).Name
            Dim sType As String = getReportTypeName(.Reports(ID))  ' D6585
            ' D7401 ===
            Dim fDoUpdate As Boolean = False
            For Each itemID As Integer In .Reports(ID).Items.Keys
                If .Reports(ID).Items(itemID).ItemType = ecReportItemType.Infodoc.ToString Then
                    deleteItemInfodoc(ID, itemID, False)
                    fDoUpdate = True
                End If
            Next
            If fDoUpdate Then PRJ.SaveStructure("", False, False, "")
            ' D7401 ==
            .Reports.Remove(ID)
            PRJ.SaveProjectOptions("Delete " + sType, False, True, String.Format(_TEMPL_LOG_REPORT, ID, sName))
        End With
        Return New jActionResult With {
            .Result = ecActionResult.arSuccess,
            .ObjectID = ID
            }
    End Function

    Public Sub Download(Optional Category As ecReportCategory = ecReportCategory.All, Optional ID As Integer = -1)   ' D6869
        'If ID < 0 OrElse Not PM.Reports.Reports.ContainsKey(ID) Then _Page.FetchWrongObject()   '-D6898
        Dim sFName As String = File_CreateTempName()
        ' D6869 + D6971 ===
        Dim Meta As New clsReportsMeta
        Dim Data As String = ""
        With Meta
            .Title = PRJ.ProjectName
            .Version = clsReportsCollection.ActualVersion
            .ProjectReff = PRJ.ProjectGUID.ToString
            .Comment = String.Format("{0}", Now.ToUniversalTime)

            If Not PM.Reports.Reports.ContainsKey(ID) Then ID = -1  ' D6898

            If ID < 0 Then
                If Category = ecReportCategory.All Then
                    .Type = ecReportsStreamType.ECAllReports
                    Data = PM.Reports.Serialize()
                Else
                    Select Case Category
                        Case ecReportCategory.Dashboard
                            .Type = ecReportsStreamType.ECDashboards
                        Case ecReportCategory.Report
                            .Type = ecReportsStreamType.ECReports
                    End Select
                    Data = PM.Reports.ByCategoryAsCollection(Category).Serialize
                End If
            Else
                .Type = CType(PM.Reports.Reports(ID).ReportType, ecReportsStreamType)
                Data = PM.Reports.Reports(ID).Serialize
            End If
        End With
        ' D6869 ==
        Dim sJSON = String.Format("[{0},{1}{2}]", Meta.Serialize(), vbNewLine, Data)
        ' D6971 ==
        MyComputer.FileSystem.WriteAllText(sFName, sJSON, False)
        DownloadFile(sFName, "application/octet-stream", GetProjectFileName(If(ID < 0, App.ActiveProject.ProjectName, PM.Reports.Reports(ID).Name) + " [" + If(ID < 0, ResString("titleReportDashboards"), ResString("titleReportDashboard")) + "]", App.ActiveProject.ProjectName, "", If(Category = ecReportCategory.Dashboard, ".dash", ".rep")), dbObjectType.einfProjectReport, App.ProjectID, True)  ' D6573 + D6593
    End Sub

    ' D6901 ===
    Public Function Add_Sample(Category As ecReportCategory, ID As Integer, Optional Dest As Integer = -1) As jActionResult  ' D6907
        If Not CanEditProject Then FetchNoPermissions() ' D6911
        Dim tRes As New jActionResult
        Dim Samples As clsReportsCollection = GetReportSamples()
        If Samples IsNot Nothing AndAlso Samples.Reports.ContainsKey(ID) Then
            Dim Dash As clsReport = Samples.Reports(ID).Clone() ' D6904
            Dim tmpCollection As New clsReportsCollection(Nothing)
            tmpCollection.Reports.Add(ID, Dash)
            Dim AddedReports As List(Of clsReport) = Nothing
            Dim AddedPanels As List(Of clsReportItem) = Nothing
            ' D6907 ===
            If Dest > 0 AndAlso Not PM.Reports.Reports.ContainsKey(Dest) Then Dest = -1
            If Dest > 0 Then
                AddedReports = New List(Of clsReport)
                AddedReports.Add(PM.Reports.Reports(Dest))
                AddedPanels = PM.Reports.AddToReport(tmpCollection, AddedReports(0), Category)
            Else
                AddedReports = PM.Reports.AddToCollection(tmpCollection, Category)
            End If
            ' D6907 ==
            If AddedReports Is Nothing OrElse AddedReports.Count = 0 Then
                tRes.Message = "Unable to add this sample due to a different type."
                tRes.Result = ecActionResult.arError
            Else
                PM.Reports.Save()
                tRes.Result = ecActionResult.arSuccess
                tRes.ObjectID = AddedReports(0).ID
                tRes.Tag = Dash.ReportType
                tRes.Message = If(Dest > 0, String.Format("Added {0} panel(s) to current dashboard.", AddedPanels.Count), String.Format("Added #{0} ""{1}"" with {2} panel(s).", AddedReports(0).ID, SafeFormString(AddedReports(0).Name), AddedReports(0).Items.Count))    ' D6907
                tRes.Data = ReportsListJSON(PM.Reports, Category, True)
            End If
            PRJ.SaveProjectOptions("Add from samples",,, tRes.Message)  ' D6911
        Else
            tRes.Message = String.Format("Can't find the specific sample (#{0})", ID)
            tRes.Result = ecActionResult.arError
        End If
        Return tRes
    End Function
    ' D6901 ==

    ' D6869 ===
    Public Function Upload(Category As ecReportCategory, File As HttpPostedFile) As jActionResult   ' D6884
        If Not CanEditProject Then FetchNoPermissions() ' D6911
        Dim tRes As New jActionResult
        If File IsNot Nothing Then
            Dim sTmpName As String = File_CreateTempName()
            Try
                File.SaveAs(sTmpName)
            Catch ex As Exception
                tRes.Result = ecActionResult.arError
                tRes.Message = "Unable to save uploaded file"
            End Try

            If MyComputer.FileSystem.FileExists(sTmpName) Then
                ' D6971 + D6899 ===
                Dim sJSON As String = MyComputer.FileSystem.ReadAllText(sTmpName)
                Dim Data As clsReportsCollection = Nothing
                Dim Meta As clsReportsMeta = clsReportsMeta.ParseJSON(PM, sJSON, Data, tRes.Message)

                If Meta IsNot Nothing AndAlso Meta.Type <> ecReportsStreamType.Unspecified AndAlso Data IsNot Nothing AndAlso Data.Reports.Count > 0 Then
                    tRes.Result = ecActionResult.arSuccess
                    Dim Added As List(Of clsReport) = PM.Reports.AddToCollection(Data, Category)
                    If Added Is Nothing OrElse Added.Count = 0 Then
                        tRes.Message = String.Format("Can't find {0} in this file", getReportCategoryName(Category))
                    Else
                        tRes.ObjectID = Added(0).ID
                        If Added.Count = 1 Then
                            With PM.Reports.Reports(tRes.ObjectID)
                                tRes.Message = String.Format("Added #{0} ""{1}"" with {2} panels.", .ID, SafeFormString(.Name), .Items.Count)
                            End With
                        Else
                            tRes.Message = String.Format("Added {0} dashboards", Added.Count)
                        End If
                        PM.Reports.Save()
                        PRJ.SaveProjectOptions("Upload " + getReportCategoryName(Category),,, tRes.Message)    ' D6911
                        tRes.Tag = Meta.Type    ' D6884
                        tRes.Data = ReportsListJSON(PM.Reports, Category, True)  ' D6884
                    End If
                    ' D6886 + D6899 ==
                Else
                    tRes.Result = ecActionResult.arError
                    Dim sError As String = "<b>Unable to parse data</b>.<br> File can be corrupted or have unsupported data format."
                    If tRes.Message <> "" Then tRes.Message = "<div style='margin-top:1ex'>Details: " + tRes.Message + "</div>"
                    tRes.Message = sError + tRes.Message
                End If

                ' D6971 ==
                File_Erase(sTmpName)
            End If
        Else
            FetchNotFound(True, "Can't find/read uploaded file")
        End If
        Return tRes
    End Function
    ' D6869 ==

    ' D6575 ===
    Private Sub ParseOptions(ByRef Options As Dictionary(Of String, Object), StringValues As String)
        If Not String.IsNullOrEmpty(StringValues) Then
            If Options Is Nothing Then Options = New Dictionary(Of String, Object)
            Dim Params As NameValueCollection = ParseJSONParams(StringValues)
            If Params IsNot Nothing Then
                For Each sKey As String In Params
                    Dim sVal As String = Params(sKey)
                    Dim tVal As Object = sVal
                    Dim vDate As DateTime
                    If (sVal.IndexOf("-") > 0 OrElse sVal.IndexOf("/") > 0) AndAlso DateTime.TryParse(sVal, vDate) Then ' D6594
                        tVal = vDate
                    Else
                        Dim vInt As Integer
                        If Integer.TryParse(sVal, vInt) Then
                            tVal = vInt
                        Else
                            Dim vDbl As Double
                            If (sVal.IndexOf(".") >= 0 OrElse sVal.IndexOf(",") >= 0) AndAlso String2Double(sVal, vDbl) Then   ' D6594
                                tVal = vDbl
                            Else
                                If sVal.ToLower = "true" OrElse sVal.ToLower = "false" OrElse sVal.ToLower = "yes" OrElse sVal.ToLower = "no" Then  ' D6594
                                    tVal = Str2Bool(sVal)
                                End If
                            End If
                        End If
                    End If
                    If tVal IsNot Nothing Then
                        If Options.ContainsKey(sKey) Then Options(sKey) = tVal Else Options.Add(sKey, tVal)
                    End If
                Next
            End If
        End If
    End Sub
    ' D6575 ==

    ' D6548 + D6572 ===
    Private Function GetReportCategory(Optional ParamName As String = "category", Optional sVal As String = Nothing) As ecReportCategory    ' D6605
        If String.IsNullOrEmpty(sVal) Then sVal = GetParam(_Page.Params, ParamName, True)   ' D6605
        Dim tRes As ecReportCategory = ecReportCategory.All
        If [Enum].IsDefined(GetType(ecReportCategory), sVal) Then
            tRes = DirectCast([Enum].Parse(GetType(ecReportCategory), sVal, True), ecReportCategory)    ' D6900
        Else
            [Enum].TryParse(Of ecReportCategory)(sVal, tRes)
        End If
        Return tRes
    End Function
    ' D6548 ==

    Private Function GetReportType(Optional ParamName As String = "type", Optional sVal As String = Nothing) As ecReportType    ' D6605
        If String.IsNullOrEmpty(sVal) Then sVal = GetParam(_Page.Params, ParamName, True)   ' D6605
        Dim tRes As ecReportType = ecReportType.Document
        If [Enum].IsDefined(GetType(ecReportType), sVal) Then
            tRes = DirectCast([Enum].Parse(GetType(ecReportType), sVal, True), ecReportType)    ' D6900
        Else
            [Enum].TryParse(Of ecReportType)(sVal, tRes)
        End If
        Return tRes
    End Function

    Private Function GetReportItemType(Optional ParamName As String = "type", Optional sVal As String = Nothing) As ecReportItemType    ' D6605
        If String.IsNullOrEmpty(sVal) Then sVal = GetParam(_Page.Params, ParamName, True)   ' D6605
        Dim tRes As ecReportItemType = ecReportItemType.Unspecified
        If [Enum].IsDefined(GetType(ecReportItemType), sVal) Then
            tRes = DirectCast([Enum].Parse(GetType(ecReportItemType), sVal, True), ecReportItemType)    ' D6900
        Else
            [Enum].TryParse(Of ecReportItemType)(sVal, tRes)
        End If
        If tRes = ecReportItemType.Unspecified AndAlso ParamName.ToLower <> "itemtype" Then tRes = GetReportItemType("ItemType")  ' D6578
        Return tRes
    End Function
    ' D6521 + D6572 ==

    ' D6511 ===
    Public Function List(Optional Category As ecReportCategory = ecReportCategory.All, Optional [Short] As Boolean = False) As jActionResult  ' D6529 + D6548
        Return New jActionResult With {
            .Result = ecActionResult.arSuccess,
            .Data = ReportsListJSON(PM.Reports, Category, Not [Short])   ' D6529 + D6548 + D6899
        }
    End Function
    ' D6511 ==

    ' D6524 ===
    Public Shared Function ReportsListJSON(Reports As clsReportsCollection, Optional Category As ecReportCategory = ecReportCategory.All, Optional WithItems As Boolean = True) As String ' D6529 + D6548 + D6899
        ' D6529 + D6548 ===
        Dim SrcList As New Dictionary(Of Integer, clsReport)    ' D6597
        SrcList = Reports.ByCategory(Category)   ' D6869 + D6899

        If WithItems Then
            Return JsonConvert.SerializeObject(SrcList)
        Else
            ' D6529 + D6548 ==
            Dim Lst As New Dictionary(Of Integer, clsReport)
            For Each tPair As KeyValuePair(Of Integer, clsReport) In SrcList    ' D6548
                Dim tRep As clsReport = tPair.Value.Clone()
                tRep.Items = Nothing
                Lst.Add(tPair.Key, tRep)
            Next
            Return JsonConvert.SerializeObject(Lst)
        End If
    End Function
    ' D6524 ==

    '' D6528 ===
    'Public Sub Download(ID As Integer, Optional format As String = "pdf")
    '    If Not PM.Reports.Reports.ContainsKey(ID) Then _Page.FetchWrongObject()
    '    Dim tReport As clsReport = PM.Reports.CloneReport(ID)
    '    Select Case tReport.ReportType
    '        Case ecReportType.Document
    '            DOC_Download(tReport, format)
    '        Case ecReportType.Presentation
    '            PPT_Download(tReport, format)
    '        Case ecReportType.Spreadsheet
    '            XLS_Download(tReport, format)
    '        Case Else   ' D6548
    '            _Page.FetchWrongObject(, "DOWNLOAD_NOT_SUPPORTED_FOR_THIS_TYPE")    ' D6548
    '    End Select
    'End Sub
    '' D6528 ==

#End Region

#Region "Working with report items"

    Public Function Add_Item(ReportID As Integer, ItemType As ecReportItemType, PageID As Integer, Name As String, Comment As String, EditURL As String, ExportURL As String, Optional ItemOptions As String = "", Optional ContentOptions As String = "") As jActionResult   ' D6525 + D6548 + D6578
        If Not CanEditProject Then FetchNoPermissions() ' D6911
        ' D6524 ===
        If ReportID < 0 Then
            Dim sReportName As String = GetParam(_Page.Params, "report_name", True)
            If Not String.IsNullOrEmpty(sReportName) Then
                Dim newReport As clsReport = PM.Reports.AddReport(GetReportType("report_type"), sReportName)    ' D6527
                If newReport IsNot Nothing Then ReportID = newReport.ID
            End If
        End If
        ' D6524 ==
        If Not PM.Reports.Reports.ContainsKey(ReportID) Then _Page.FetchWrongObject()
        Dim tReport As clsReport = PM.Reports.Reports(ReportID)
        Dim tItem As clsReportItem = Nothing
        If ItemType <> ecReportItemType.Unspecified Then tItem = tReport.AddItem(ItemType, Name, Comment)
        If tItem Is Nothing Then
            Return New jActionResult With {
                .Result = ecActionResult.arError,
                .Message = If(ItemType = ecReportItemType.Unspecified, "Unspecified or unsupported report item type", "Unable to add new item")
            }
        Else
            SetItemProperties(tItem, tReport, ItemType, PageID, Name, Comment, EditURL, ExportURL, ItemOptions, ContentOptions, GetParam(_Page.Params, "index", True)) ' D6578 + D6605
            PRJ.SaveProjectOptions("Add " + getReportTypeName(tReport) + " Item",,, String.Format(_TEMPL_LOG_ITEM, tReport.ID, tItem.ID, Name)) ' D6585
            ' D6548 ===
            Dim Category As ecReportCategory = ecReportCategory.All
            Select Case tReport.ReportType
                Case ecReportType.Dashboard
                    Category = ecReportCategory.Dashboard
                Case Else
                    Category = ecReportCategory.Report
            End Select
            Return New jActionResult With {
                .Result = ecActionResult.arSuccess,
                .ObjectID = tReport.ID,
                .Data = tItem.Serialize(),
                .Tag = ReportsListJSON(PM.Reports, Category, False) ' D6524 + D6529 + D6899
            }
            ' D6548 ==
        End If
    End Function

    Private Function Import_Items(ByRef tReport As clsReport, sItems As String) As Boolean
        If Not CanEditProject Then FetchNoPermissions() ' D6911
        Dim fUpdated As Boolean = False
        If tReport IsNot Nothing AndAlso Not String.IsNullOrEmpty(sItems) Then
            Try
                Dim Lst As NameValueCollection = ParseJSONParams(sItems)
                If Lst IsNot Nothing Then
                    For Each sKey As String In Lst.Keys
                        Dim tVal As String = Lst(sKey)
                        If Not String.IsNullOrEmpty(tVal) Then
                            Dim sParams As New NameValueCollection  ' D7321
                            If tVal.Contains("{") Then sParams = ParseJSONParams(tVal)  ' D7321
                            If sParams IsNot Nothing Then
                                Dim sType As ecReportItemType = GetReportItemType("ItemType", GetParam(sParams, "ItemType"))
                                If sType <> ecReportItemType.Unspecified Then
                                    Dim sIndex As String = GetParam(sParams, "index")
                                    Dim sComment As String = GetParam(sParams, "comment")
                                    Dim sEditURL As String = GetParam(sParams, "edit")
                                    Dim sExport As String = GetParam(sParams, "export")
                                    Dim sIOptions As String = GetParam(sParams, "ItemOptions")
                                    Dim sCOptions As String = GetParam(sParams, "ContentOptions")
                                    Dim sName As String = GetParam(sParams, "name")
                                    If String.IsNullOrEmpty(sName) Then sName = String.Format("{0} #{1}", If(sType = ecReportItemType.NewItem, ResString("lblPlaceholder"), "Panel"), (tReport.Items.Count + 1))    ' D6834
                                    Dim tItem As clsReportItem = tReport.AddItem(sType, sName, sComment)
                                    If tItem IsNot Nothing Then
                                        SetItemProperties(tItem, tReport, sType, -1, sName, sComment, sEditURL, sExport, sIOptions, sCOptions, sIndex)
                                    End If
                                End If
                            End If
                        End If
                    Next
                End If
            Catch ex As Exception
            End Try
        End If
        Return fUpdated
    End Function


    ' D6578 ===
    Public Function Edit_Item(ReportID As Integer, ID As Integer, ItemType As ecReportItemType, PageID As Integer, Name As String, Comment As String, EditURL As String, ExportURL As String, Optional ItemOptions As String = "", Optional ContentOptions As String = "") As jActionResult
        If Not CanEditProject Then FetchNoPermissions() ' D6911
        ' D7032 ===
        If Not PM.Reports.Reports.ContainsKey(ReportID) Then
            '_Page.FetchWrongObject()
            Return New jActionResult With {
                .Result = ecActionResult.arError,
                .Message = "Wrong Object ID",
                .ObjectID = ReportID
            }
        End If
        Dim tReport As clsReport = PM.Reports.Reports(ReportID)
        If Not tReport.Items.ContainsKey(ID) Then
            '_Page.FetchWrongObject()
            Return New jActionResult With {
                .Result = ecActionResult.arError,
                .Message = "Wrong Item ID",
                .ObjectID = ID
            }
        End If
        ' D7032 ==
        Dim tItem As clsReportItem = tReport.Items(ID)
        ' D6882 ===
        'If tItem.ItemType <> ItemType  Then
        If Not tItem.ItemType.Equals(ItemType.ToString, StringComparison.InvariantCultureIgnoreCase) Then   ' D7020
            tItem.ContentOptions.Clear()
            ContentOptions = ""
        End If
        ' D6882 ==
        SetItemProperties(tItem, tReport, ItemType, PageID, Name, Comment, EditURL, ExportURL, ItemOptions, ContentOptions, GetParam(_Page.Params, "index", True))  ' D6605
        PRJ.SaveProjectOptions("Edit " + getReportTypeName(tReport) + " Item",,, String.Format(_TEMPL_LOG_ITEM, tReport.ID, tItem.ID, Name))    ' D6585
        Dim Category As ecReportCategory = ecReportCategory.All
        Select Case tReport.ReportType
            Case ecReportType.Dashboard
                Category = ecReportCategory.Dashboard
            Case Else
                Category = ecReportCategory.Report
        End Select
        Return New jActionResult With {
            .Result = ecActionResult.arSuccess,
            .ObjectID = tReport.ID,
            .Data = tItem.Serialize(),
            .Tag = ReportsListJSON(PM.Reports, Category, False) ' D6899
        }
    End Function

    Public Function Clone_Item(ReportID As Integer, ID As Integer, Name As String, Comment As String, EditURL As String, ExportURL As String, Optional ItemOptions As String = "", Optional ContentOptions As String = "") As jActionResult
        If Not CanEditProject Then FetchNoPermissions() ' D6911
        If Not PM.Reports.Reports.ContainsKey(ReportID) Then _Page.FetchWrongObject()
        Dim tReport As clsReport = PM.Reports.Reports(ReportID)
        If Not tReport.Items.ContainsKey(ID) Then _Page.FetchWrongObject()
        Dim tItem As clsReportItem = tReport.CloneItem(ID)
        If tItem Is Nothing Then
            Return New jActionResult With {
                .Result = ecActionResult.arError,
                .Message = "Unable to clone panel"
            }
        Else
            'SetItemProperties(tItem, tReport, tItem.ItemType, tItem.PageID, Name, Comment, EditURL, ExportURL, ItemOptions, ContentOptions, GetParam(_Page.Params, "index", True))  ' D6605
            SetItemProperties(tItem, tReport, GetReportItemType("", tItem.ItemType), tItem.PageID, Name, Comment, EditURL, ExportURL, ItemOptions, ContentOptions, GetParam(_Page.Params, "index", True))  ' D6605 + D7020 + D7226
            PRJ.SaveProjectOptions("Add " + getReportTypeName(tReport) + " Item",,, String.Format(_TEMPL_LOG_ITEM + ", cloned from " + _TEMPL_LOG_ITEM, tReport.ID, ID, tReport.Items(ID).Name, tReport.ID, tItem.ID, Name)) ' D6585
            Dim Category As ecReportCategory = ecReportCategory.All
            Select Case tReport.ReportType
                Case ecReportType.Dashboard
                    Category = ecReportCategory.Dashboard
                Case Else
                    Category = ecReportCategory.Report
            End Select
            If tItem.ItemType = ecReportItemType.Infodoc.ToString Then cloneItemInfodoc(ReportID, ID, ReportID, tItem.ID, True)   ' D7401
            Return New jActionResult With {
                .Result = ecActionResult.arSuccess,
                .ObjectID = tReport.ID,
                .Data = tItem.Serialize(),
                .Tag = ReportsListJSON(PM.Reports, Category, False)  ' D6899
            }
        End If
    End Function

    Private Sub SetItemProperties(ByRef tItem As clsReportItem, tReport As clsReport, ItemType As ecReportItemType, PageID As Integer, Name As String, Comment As String, EditURL As String, ExportURL As String, Optional ItemOptions As String = "", Optional ContentOptions As String = "", Optional Index As String = "")   ' D6605
        If tItem IsNot Nothing Then
            With tItem
                Dim tPg As clsPageAction = PageByID(PageID)
                If tPg Is Nothing Then
                    Select Case ItemType
                        Case ecReportItemType.Alternatives
                            PageID = _PGID_STRUCTURE_ALTERNATIVES
                        Case ecReportItemType.AlternativesChart
                            PageID = _PGID_ANALYSIS_CHARTS_ALTS
                        Case ecReportItemType.ObjectivesChart
                            PageID = _PGID_ANALYSIS_CHARTS_OBJS
                        Case ecReportItemType.DataGrid
                            PageID = _PGID_REPORT_DATAGRID
                        Case ecReportItemType.EvalProgress
                            PageID = _PGID_MEASURE_EVAL_PROGRESS
                        Case ecReportItemType.ModelDescription
                            PageID = _PGID_PROJECT_DESCRIPTION
                        Case ecReportItemType.Objectives
                            PageID = _PGID_STRUCTURE_HIERARCHY
                        Case ecReportItemType.DSA
                            PageID = _PGID_ANALYSIS_DSA
                        Case ecReportItemType.PSA
                            PageID = _PGID_ANALYSIS_PSA
                        Case ecReportItemType.GSA
                            PageID = _PGID_ANALYSIS_GSA
                        Case ecReportItemType.Analysis2D
                            PageID = _PGID_ANALYSIS_2D
                        Case ecReportItemType.ASA
                            PageID = _PGID_ANALYSIS_ASA
                        Case ecReportItemType.HTH
                            PageID = _PGID_ANALYSIS_HEAD2HEAD
                        Case ecReportItemType.ProsAndCons
                            PageID = _PGID_ANTIGUA_MEETING
                            ' D6873 ===
                        Case ecReportItemType.PortfolioGrid
                            PageID = _PGID_RA_BASE
                        Case ecReportItemType.AltsGrid
                            PageID = _PGID_ANALYSIS_OVERALL_ALTS
                        Case ecReportItemType.ObjsGrid
                            PageID = _PGID_ANALYSIS_OVERALL_OBJS
                            ' D6873 ==
                        Case ecReportItemType.Participants  ' D7302
                            PageID = _PGID_PROJECT_USERS    ' D7302
                    End Select
                End If
                If EditURL = "" OrElse EditURL = "/" Then EditURL = PageURL(PageID) ' D6873
                If Not EditURL.StartsWith("/") AndAlso Not EditURL.Contains(":") Then EditURL = "/" + EditURL   ' D7302

                .PageID = PageID
                .EditURL = EditURL
                .ExportURL = ExportURL
                .Name = Name
                .Comment = Comment  ' D6594
                '.ItemType = ItemType
                .ItemType = ItemType.ToString   ' D7020

                ParseOptions(tItem.ItemOptions, ItemOptions)
                ParseOptions(tItem.ContentOptions, ContentOptions) ' D6585

                Dim tIdx As Integer
                If Not String.IsNullOrEmpty(Index) AndAlso Integer.TryParse(Index, tIdx) AndAlso tReport IsNot Nothing Then ' D6605
                    If tIdx > 0 AndAlso tIdx < tReport.Items.Count AndAlso tIdx <> .Index Then
                        ReorderReportItems(tReport, .Index, tIdx)
                    End If
                End If
            End With
        End If
    End Sub

    Private Sub ReorderReportItems(ByRef tReport As clsReport, Idx_Old As Integer, Idx_New As Integer)
        If tReport IsNot Nothing Then
            Dim diff As Integer = Idx_New - Idx_Old
            For Each tPair As KeyValuePair(Of Integer, clsReportItem) In tReport.Items
                Dim idx As Integer = tPair.Value.Index
                If (idx = Idx_Old) Then tPair.Value.Index = Idx_New
                If (diff > 0) Then
                    If (idx > Idx_Old AndAlso idx <= Idx_New) Then tPair.Value.Index -= 1
                End If
                If (diff < 0) Then
                    If (idx >= Idx_New AndAlso idx < Idx_Old) Then tPair.Value.Index += 1
                End If
            Next
            tReport.SortItemsByIndex()  ' D6578
        End If
    End Sub
    ' D6578 ==

    ' D6573 ===
    Public Function Item_Index(ReportID As Integer, Idx_Old As Integer, Idx_New As Integer) As jActionResult
        If Not PM.Reports.Reports.ContainsKey(ReportID) Then _Page.FetchWrongObject()
        ReorderReportItems(PM.Reports.Reports(ReportID), Idx_Old, Idx_New)    ' D6578
        PM.Reports.Save()
        Return New jActionResult With {
            .Result = ecActionResult.arSuccess,
            .ObjectID = ReportID
        }
    End Function
    ' D6573 ==

    ' D6575 ===
    Public Function Delete_Item(ReportID As Integer, ID As Integer) As jActionResult
        If Not CanEditProject Then FetchNoPermissions() ' D6911
        If Not PM.Reports.Reports.ContainsKey(ReportID) Then _Page.FetchWrongObject()
        Dim tReport As clsReport = PM.Reports.Reports(ReportID)
        If Not tReport.Items.ContainsKey(ID) Then _Page.FetchWrongObject()
        If tReport.Items(ID).ItemType = ecReportItemType.Infodoc.ToString Then deleteItemInfodoc(ReportID, ID, True)    ' D7401
        Dim sName As String = tReport.Items(ID).Name
        tReport.Items.Remove(ID)
        tReport.SortItemsByIndex()  ' D6578
        PRJ.SaveProjectOptions("Delete " + getReportTypeName(tReport) + " Item", False, True, String.Format(_TEMPL_LOG_REPORT, ID, sName))  ' D6585
        Return New jActionResult With {
            .Result = ecActionResult.arSuccess,
            .ObjectID = ID,
            .Tag = ReportID
            }
    End Function
    ' D6575 ==

#End Region

#Region "Service functions"

    ' D7401 ===
    Private Function deleteItemInfodoc(DashboardID As Integer, ItemID As Integer, Optional saveDB As Boolean = False) As Boolean
        Dim tRes As Boolean = False
        If PM IsNot Nothing Then
            Dim tGUID As New Guid(ItemID, CShort(reObjectType.DashboardInfodoc), CShort(DashboardID), 0, 0, 0, 0, 0, 0, 0, 0)
            PM.InfoDocs.SetCustomInfoDoc("", tGUID, Guid.Empty)
            If saveDB AndAlso PRJ IsNot Nothing Then PRJ.SaveStructure("", False, False, "")
            tRes = True
        End If
        Return tRes
    End Function

    Private Function cloneItemInfodoc(tSrcDashID As Integer, tSrcItemID As Integer, tDestDashID As Integer, tDestItemID As Integer, Optional saveDB As Boolean = False) As Boolean
        Dim tRes As Boolean = False
        If PM IsNot Nothing Then
            Dim tSrcGUID As New Guid(tSrcItemID, CShort(reObjectType.DashboardInfodoc), CShort(tSrcDashID), 0, 0, 0, 0, 0, 0, 0, 0)
            Dim tDestGUID As New Guid(tDestItemID, CShort(reObjectType.DashboardInfodoc), CShort(tDestDashID), 0, 0, 0, 0, 0, 0, 0, 0)
            Dim sInfodoc = PM.InfoDocs.GetCustomInfoDoc(tSrcGUID, Guid.Empty)
            PM.InfoDocs.SetCustomInfoDoc(sInfodoc, tDestGUID, Guid.Empty)
            If saveDB AndAlso PRJ IsNot Nothing Then PRJ.SaveStructure("", False, False, "")
            tRes = True
        End If
        Return tRes
    End Function
    ' D7401 ==

    Private Function GetJudgment(ByVal CovObj As clsNode, altID As Integer, userID As Integer) As clsNonPairwiseMeasureData
        Return CType(CovObj.Judgments, clsNonPairwiseJudgments).GetJudgement(altID, CovObj.NodeID, userID)
    End Function

#End Region

    Private Sub ReportWebAPI_Init(sender As Object, e As EventArgs) Handles Me.Init
        GemBox.Document.ComponentInfo.SetLicense("DN-2019Oct23-o9cPV8FqJpXV/58cTjlRakTEbT0YmbK2qfwJS4OkrVTGqcdjqxHlgRDfWwwv2q3j9qruUP258vApmepOW0Z46sRTc6Q==A")
        GemBox.Presentation.ComponentInfo.SetLicense("PN-2019Oct23-7esqnSuMJVQEFrRWgpgmjvPBQJyxMtrJ+gyZpUocTKpiNFnadOxsvCrZJYM5/nk1w+KbQKspn1xCUQDmPWsvtZqJJNQ==A")
        SpreadsheetInfo.SetLicense("SN-2019Oct23-AKAqJAF7VQbNwpzNxMw+YMGUPQKtiac8SV2w3r4AR43N/ctUteUub15BB7pw2rqTqZB6PzdDiG4vHXzKw5SRVIMv9Gw==A")
        'SpreadsheetInfo.SetLicense("AZNK-TASR-VC9J-1SFL")
    End Sub

    Private Sub ProjectManagerWebAPI_Load(sender As Object, e As EventArgs) Handles Me.Load
        FetchIfNoActiveProject() ' D6511
        CanEditProject = App.CanUserModifyProject(App.ActiveUser.UserID, App.ProjectID, App.ActiveUserWorkgroup, App.ActiveWorkspace, App.ActiveWorkgroup)  ' D6911
        Select Case _Page.Action

            ' D6511 ===
            Case "list"
                _Page.ResponseData = List(GetReportCategory(), Str2Bool(GetParam(_Page.Params, "short", True), False))   ' D6529 + D6548
            ' D6511 ==

            ' D6521 ===
            Case "add_report"
                _Page.ResponseData = Add_Report(GetReportType(), GetParam(_Page.Params, "name", True), GetParam(_Page.Params, "comment", True), GetParam(_Page.Params, "Options", True))    ' D6575

            Case "edit_report"
                Dim ID As Integer = -1
                Integer.TryParse(GetParam(_Page.Params, "id", True), ID)
                _Page.ResponseData = Edit_Report(ID, GetParam(_Page.Params, "name", True), GetParam(_Page.Params, "comment", True), GetParam(_Page.Params, "Options", True))    ' D6575

            Case "clone_report"
                Dim ID As Integer = -1
                Integer.TryParse(GetParam(_Page.Params, "id", True), ID)
                _Page.ResponseData = Clone_Report(ID, GetParam(_Page.Params, "name", True), GetParam(_Page.Params, "comment", True), GetParam(_Page.Params, "Options", True))   ' D6575

            Case "delete_report"
                Dim ID As Integer = -1
                Integer.TryParse(GetParam(_Page.Params, "id", True), ID)
                _Page.ResponseData = Delete_Report(ID)

            Case "download" ' D6869
                Dim ID As Integer = -1
                If Not Integer.TryParse(GetParam(_Page.Params, "id", True), ID) Then ID = -1
                Download(GetReportCategory(), ID)    ' D6869

                ' D6869 ===
            Case "upload"
                Dim File As HttpPostedFile = Nothing
                If Request IsNot Nothing AndAlso Request.Files.Count > 0 Then File = Request.Files(0)
                _Page.ResponseData = Upload(GetReportCategory(), File)
                ' D6869 ==

                ' D6901 ===
            Case "add_sample"
                Dim ID As Integer = -1
                If Not Integer.TryParse(GetParam(_Page.Params, "id", True), ID) Then ID = -1
                Dim DestID As Integer = -1  ' D6907
                If Not Integer.TryParse(GetParam(_Page.Params, "dest", True), DestID) Then DestID = -1  ' D6907
                _Page.ResponseData = Add_Sample(GetReportCategory(), ID, DestID)    ' D6907
                ' D6901 ==

                ' D6522 ===
            Case "add_item"
                Dim ReportID As Integer = -1
                Integer.TryParse(GetParam(_Page.Params, "report_id", True), ReportID)
                Dim PgID As Integer = -1
                Integer.TryParse(GetParam(_Page.Params, "pgid", True), PgID)
                _Page.ResponseData = Add_Item(ReportID, GetReportItemType(), PgID, GetParam(_Page.Params, "name", True), GetParam(_Page.Params, "comment", True), GetParam(_Page.Params, "edit", True), GetParam(_Page.Params, "export", True), GetParam(_Page.Params, "ItemOptions", True), GetParam(_Page.Params, "ContentOptions", True))   ' D6525 + D6548 + D6578
                ' D6522 ==

                ' D6578 ===
            Case "edit_item"
                Dim ReportID As Integer = -1
                Integer.TryParse(GetParam(_Page.Params, "report_id", True), ReportID)
                Dim PgID As Integer = -1
                Integer.TryParse(GetParam(_Page.Params, "pgid", True), PgID)
                Dim ID As Integer = -1
                Integer.TryParse(GetParam(_Page.Params, "id", True), ID)
                _Page.ResponseData = Edit_Item(ReportID, ID, GetReportItemType(), PgID, GetParam(_Page.Params, "name", True), GetParam(_Page.Params, "comment", True), GetParam(_Page.Params, "edit", True), GetParam(_Page.Params, "export", True), GetParam(_Page.Params, "ItemOptions", True), GetParam(_Page.Params, "ContentOptions", True))

            Case "clone_item"
                Dim ReportID As Integer = -1
                Integer.TryParse(GetParam(_Page.Params, "report_id", True), ReportID)
                Dim ID As Integer = -1
                Integer.TryParse(GetParam(_Page.Params, "id", True), ID)
                _Page.ResponseData = Clone_Item(ReportID, ID, GetParam(_Page.Params, "name", True), GetParam(_Page.Params, "comment", True), GetParam(_Page.Params, "edit", True), GetParam(_Page.Params, "export", True), GetParam(_Page.Params, "ItemOptions", True), GetParam(_Page.Params, "ContentOptions", True))
                ' D6578 ==

                ' D6573 ===
            Case "item_index"
                Dim ReportID As Integer = -1
                Integer.TryParse(GetParam(_Page.Params, "report_id", True), ReportID)
                Dim IdxOld As Integer = -1
                Integer.TryParse(GetParam(_Page.Params, "idx_old", True), IdxOld)
                Dim IdxNew As Integer = -1
                Integer.TryParse(GetParam(_Page.Params, "idx_new", True), IdxNew)
                _Page.ResponseData = Item_Index(ReportID, IdxOld, IdxNew)
                ' D6573 ==

                ' D6575 ===
            Case "delete_item"
                Dim ReportID As Integer = -1
                Integer.TryParse(GetParam(_Page.Params, "report_id", True), ReportID)
                Dim ID As Integer = -1
                Integer.TryParse(GetParam(_Page.Params, "id", True), ID)
                _Page.ResponseData = Delete_Item(ReportID, ID)
                ' D6575 ==

        End Select
    End Sub

End Class