'Option Strict On
Imports Microsoft.VisualBasic
Imports <xmlns="urn:schemas-microsoft-com:office:spreadsheet">
Imports <xmlns:o="urn:schemas-microsoft-com:office:office">
Imports <xmlns:x="urn:schemas-microsoft-com:office:excel">
Imports <xmlns:ss="urn:schemas-microsoft-com:office:spreadsheet">
Imports <xmlns:html="http://www.w3.org/TR/REC-html40">
Imports System.IO
Imports System.Drawing
Imports OfficeOpenXml.Style
Imports OfficeOpenXml
Imports System.Diagnostics 'AS/11339

Public Class clsDataGrid

    Public Shared ReadOnly TotalGUID As New Guid("{4202a57f-dc3b-4072-98b3-e1253e23e41b}")

    Class CellValue

        Sub New(RowID As Integer, ColumnID As Integer, RowGUID As Guid, ColumnGUID As Guid, Data As Object, Optional Priority As Double = -1)
            Me.RowID = RowID
            Me.ColumnID = ColumnID
            Me.RowGUID = RowGUID
            Me.ColumnGUID = ColumnGUID
            If Data Is Nothing Then
                IsNull = True
                Me.Data = ""
                Me.Priority = 0
            Else
                If Priority = -1 Then
                    Me.Priority = CDbl(Data)
                End If
                Me.Priority = Priority
                Me.Data = Data
                IsNull = False
            End If
        End Sub

        Public RowID, ColumnID As Integer
        Public RowGUID, ColumnGUID As Guid
        Public Data As Object
        Public Priority As Double
        Public IsNull As Boolean

    End Class

    Class AltAttribute
        Sub New(ID As Integer, AttrType As CellType, Name As String)
            Me.ID = ID
            Me.AttrType = AttrType
            Me.Name = Name
        End Sub

        Public ID As Integer
        Public AttrType As CellType
        Public Name As String
    End Class

    Public Enum CellType
        [String] = 0
        [Number] = 1
    End Enum

    Public Alts As New List(Of clsNode)
    Public Objs As New List(Of clsNode)
    Public CurrentUser As clsUser = Nothing

    Public CurrentGroup As clsCombinedGroup = Nothing
    Public ProjectName As String = ""
    Public RatingValues As New List(Of CellValue)
    Public AltAttributeNames As New List(Of clsAttribute)
    Public AltAttributeValues As New Dictionary(Of Guid, Dictionary(Of Guid, String))
    Public CalcMode As String

    Public AddNewAltsLabel As String = ""

    Private mAttributesReadOnly As Boolean
    Public Property AttributesReadOnly() As Boolean
        Get
            Return mAttributesReadOnly
        End Get
        Set(ByVal value As Boolean)
            mAttributesReadOnly = value
        End Set
    End Property

    Public Function FindRatingCell(Row As Guid, Column As Guid) As CellValue
        Return FindCell(RatingValues, Row, Column)
    End Function

    Private Function FindCell(Values As List(Of CellValue), Row As Guid, Column As Guid) As CellValue
        Dim c As CellValue
        Try
            c = (From val As CellValue In Values Where val.RowGUID = Row And val.ColumnGUID = Column).FirstOrDefault()
        Catch ex As Exception
            c = Nothing
        End Try
        Return c
    End Function

    Public AltData As New Dictionary(Of String, AltDataItem)

    Class AltDataItem
        Sub New()
        End Sub
        Public ID As Guid
        Public Name As String
        Public CoveringData As Dictionary(Of Guid, String)
        Public CustomAttrData As Dictionary(Of Guid, String)
        Public NewAlt As Boolean = False
    End Class

    Public Shared Function Read(Filename As String, Nodes As Dictionary(Of Guid, clsNode), PM As clsProjectManager, Optional IsProjectManager As Boolean = False) As Dictionary(Of String, AltDataItem)
        Dim retVal As New Dictionary(Of String, AltDataItem)

        Try
            'This code does NOT check to see if data is bi-directional, before writing back to DB you must check if you CAN write to that value

            Using package As New ExcelPackage(New FileInfo(Filename))
                Dim Datagrid As ExcelWorksheet = package.Workbook.Worksheets.Where(Function(x) x.Name = dDatagrid).First

                Dim dupMsg As String = "duplicateguid:"
                Dim cGui As New Collection

                For row = Datagrid.Workbook.Names(rAltInfo).Start.Row To Datagrid.Workbook.Names(rAltInfo).End.Row - 1
                    Dim alt As New AltDataItem

                    If CStr(Datagrid.Cells(row, Address(Datagrid, cAltGUID).Column).Value) IsNot Nothing Then
                        alt.ID = New Guid(Datagrid.Cells(row, Address(Datagrid, cAltGUID).Column).Text)
                        alt.Name = Datagrid.Cells(row, Address(Datagrid, cAltName).Column).Text
                    ElseIf IsProjectManager Then
                        alt.ID = Guid.NewGuid()
                        alt.Name = Datagrid.Cells(row, Address(Datagrid, cAltName).Column).Text
                        alt.NewAlt = True
                    End If

                    If IsProjectManager Then
                        Try
                            cGui.Add("", alt.ID.ToString)
                        Catch ex As Exception
                            Throw New DataGridExceptionDuplicateGUID(row & Convert(Address(Datagrid, cAltGUID).Column))
                        End Try
                    End If

                    Dim objdata As New Dictionary(Of Guid, String)
                    Dim attrdata As New Dictionary(Of Guid, String)

                    For col As Integer = Datagrid.Workbook.Names(rAltObjData).Start.Column To Datagrid.Workbook.Names(rAltObjData).End.Column
                        Dim CovObjID As Guid = New Guid(Datagrid.Cells(Address(Datagrid, cCovGUID).Row, col).Text)
                        'Dim Data As Object = Datagrid.Cells(row, col).Text
                        Dim sData As String = CStr(Datagrid.Cells(row, col).Value)    ' D7512
                        If sData Is Nothing Then sData = Datagrid.Cells(row, col).Text  ' D7525
                        If sData Is Nothing Then sData = ""
                        Dim mt As ECMeasureType = ECMeasureType.mtDirect
                        Dim aNode As clsNode = Nothing
                        If Nodes.ContainsKey(CovObjID) Then
                            aNode = Nodes.Item(CovObjID)
                            mt = aNode.MeasureType
                        End If
                        Select Case mt
                            Case ECMeasureType.mtDirect
                                'Dim tVal As Single = 0
                                'Try
                                '    tVal = CSng(Data)
                                'Catch
                                'End Try
                                'If tVal < 0 Or tVal > 1 Then
                                '    Throw New DataGridExceptionInvalidDirect(Datagrid.Cells(row, col).Address)
                                'End If
                                ' D7512 ===
                                Dim tVal As Double
                                If String2Double(sData, tVal) Then
                                    If tVal < 0 Or tVal > 1 Then
                                        Throw New DataGridExceptionInvalidDirect(Datagrid.Cells(row, col).Address)
                                    End If
                                End If
                                ' D7512 ==
                            Case ECMeasureType.mtRatings
                                If aNode IsNot Nothing Then
                                    If aNode.MeasurementScale IsNot Nothing Then
                                        Dim ms As clsRatingScale = CType(aNode.MeasurementScale, clsRatingScale)
                                        Dim RatingExists As Boolean = False
                                        Dim sVal As String = sData.Trim.ToLower  ' D7512
                                        For Each R As clsRating In ms.RatingSet
                                            If R.Name.Equals(sVal, StringComparison.InvariantCultureIgnoreCase) Then    ' D7512
                                                RatingExists = True
                                                Exit For
                                            End If
                                        Next
                                        If Not RatingExists Then
                                            'Dim tVal As Single = 0
                                            'Try
                                            '    tVal = CSng(Data)
                                            'Catch
                                            'End Try
                                            'If tVal < 0 Or tVal > 1 Then
                                            '    Throw New DataGridExceptionInvalidDirect(Datagrid.Cells(row, col).Address)
                                            'End If
                                            ' D7512 ===
                                            Dim tVal As Double
                                            If String2Double(sData, tVal) Then
                                                If tVal < 0 Or tVal > 1 Then
                                                    Throw New DataGridExceptionInvalidRating(Datagrid.Cells(row, col).Address)  ' D7517
                                                End If
                                            End If
                                            ' D7512 ==
                                        End If
                                    End If
                                End If
                        End Select
                        'objdata.Add(CovObjID, CStr(Data))
                        objdata.Add(CovObjID, sData)    ' D7512
                    Next
                    If IsProjectManager Then
                        For col As Integer = Datagrid.Workbook.Names(rAltAttrData).Start.Column To Datagrid.Workbook.Names(rAltAttrData).End.Column
                            Dim CovattrID As Guid = New Guid(Datagrid.Cells(Address(Datagrid, cCovGUID).Row, col).Text)
                            Dim Data As Object = Datagrid.Cells(row, col).Text
                            Select Case CovattrID
                                Case ATTRIBUTE_RISK_ID
                                    Dim aVal As Single = 0
                                    Try
                                        aVal = CSng(Data)
                                    Catch
                                    End Try
                                    If aVal < 0 Or aVal > 1 Then
                                        Throw New DataGridExceptionInvalidRisk(Datagrid.Cells(row, col).Address)
                                    End If
                            End Select
                            attrdata.Add(CovattrID, CStr(Data))
                        Next
                        alt.CustomAttrData = attrdata
                    End If
                    alt.CoveringData = objdata
                    retVal.Add(alt.ID.ToString, alt)
                Next
            End Using

        Catch ex As DataGridException
            Throw ex
        Catch ex As Exception
            Throw New DataGridException("unknown error")
        End Try

        Return retVal
    End Function

    Private Function GetFullPath(ANode As clsNode) As String
        If ANode Is Nothing OrElse ANode.ParentNode Is Nothing OrElse ANode.ParentNodesGuids.Count > 1 Then
            Return ""
        Else
            Dim ParentPath As String = GetFullPath(ANode.ParentNode)
            If ParentPath = "" Then
                Return ANode.NodeName
            Else
                Return ParentPath + "|" + ANode.NodeName
            End If

        End If
    End Function

    Private Function GetGoalName(ANode As clsNode) As String
        If ANode.ParentNode Is Nothing Then
            Return ANode.NodeName
        Else
            Return GetGoalName(ANode.ParentNode)
        End If
    End Function

    Public Sub Write(Filename As String)
        'erase the file if it already exists
        Try
            If IO.File.Exists(Filename) Then
                IO.File.Delete(Filename)
            End If
        Catch
        End Try

        Dim MasterXLSX = System.AppDomain.CurrentDomain.BaseDirectory & "App_Data/Datagrid/Master.xlsx"
        System.IO.File.Copy(MasterXLSX, Filename)

        Dim fi = New FileInfo(Filename)
        Using package As New ExcelPackage(fi)

            Dim Datagrid As ExcelWorksheet = package.Workbook.Worksheets.Where(Function(x) x.Name = dDatagrid).First
            Dim TotalsDG As ExcelWorksheet = package.Workbook.Worksheets.Where(Function(x) x.Name = dTotalsDG).First
            Dim TotalsCalc As ExcelWorksheet = package.Workbook.Worksheets.Where(Function(x) x.Name = dTotalsCalc).First
            Dim Styles As ExcelWorksheet = package.Workbook.Worksheets.Where(Function(x) x.Name = dStyles).First

            If CurrentUser IsNot Nothing Then
                Datagrid.Workbook.Names(cUserName).Value = CurrentUser.UserEMail
            End If
            If CurrentGroup IsNot Nothing Then
                Datagrid.Workbook.Names(cUserName).Value = CurrentGroup.Name
            End If
            Datagrid.Workbook.Names(cFilename).Value = ProjectName
            Datagrid.Workbook.Names(cCalcMode).Value = CalcMode

            Dim startObjCol As Integer = Datagrid.Cells(Datagrid.Workbook.Names(cTotal).Address).Start.Column + 1

            'build cov objective headers

            Dim CalcTarget As clsCalculationTarget = Nothing
            If CurrentUser IsNot Nothing Then
                CalcTarget = New clsCalculationTarget(CalculationTargetType.cttUser, CurrentUser)
            ElseIf CurrentGroup IsNot Nothing Then
                CalcTarget = New clsCalculationTarget(CalculationTargetType.cttCombinedGroup, CurrentGroup)
            End If
            If CalcTarget IsNot Nothing And Objs.Count > 0 Then
                If CalcTarget.TargetType = CalculationTargetType.cttUser Then
                    Objs(0).Hierarchy.ProjectManager.StorageManager.Reader.LoadUserData(CurrentUser)
                End If
                Objs(0).Hierarchy.ProjectManager.CalculationsManager.Calculate(CalcTarget, Objs(0).Hierarchy.Nodes(0))
            End If

            Dim colGuid As New Dictionary(Of String, Cell)
            Dim NotInitGoal As Boolean = True
            For row As Integer = 1 To Address(Datagrid, cTotal).Row
                Dim col As Integer = Address(Datagrid, cTotal).Column
                For Each c In Objs
                    col += 1

                    Select Case row
                        Case 1
                            If NotInitGoal Then
                                Datagrid.Cells(row, col).Value = GetGoalName(Objs(0))
                                Datagrid.SelectedRange(row, col, row, col + Objs.Count - 1).Merge = True
                                Datagrid.Cells(row, col).StyleName = sStyleHeaderText
                                NotInitGoal = False
                            End If
                        Case 7 'Type
                            Dim strMeasurementType As String = ""
                            Select Case c.MeasureType
                                Case ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtCustomUtilityCurve, ECMeasureType.mtRegularUtilityCurve
                                    strMeasurementType = "Utility Curve"
                                Case ECMeasureType.mtDirect
                                    strMeasurementType = "Direct"
                                Case ECMeasureType.mtNone
                                    strMeasurementType = "None"
                                Case ECMeasureType.mtPairwise
                                    strMeasurementType = "Pairwise"
                                Case ECMeasureType.mtRatings
                                    strMeasurementType = "Ratings"
                                Case ECMeasureType.mtStep
                                    strMeasurementType = "Step"
                            End Select
                            Datagrid.Cells(row, col).Value = strMeasurementType
                            Datagrid.Cells(row, col).StyleName = sStyleHeaderBottom
                        Case 2 'Full Path
                            Datagrid.Cells(row, col).Value = GetFullPath(c.ParentNode)
                            Datagrid.Cells(row, col).StyleName = sStyleHeaderText
                        Case 4 'Local
                            Datagrid.Cells(row, col).Value = c.LocalPriority(CalcTarget)
                            Datagrid.Cells(row, col).StyleName = sStyleHeaderNum
                        Case 5 'Global
                            Datagrid.Cells(row, col).Value = c.WRTGlobalPriority
                            Datagrid.Cells(row, col).StyleName = sStyleHeaderNum
                        Case 6 'CovOBJGuid
                            colGuid.Add(c.NodeGuidID.ToString, New Cell(row, col))
                        Case 3 'Name
                            Datagrid.Cells(row, col).Value = c.NodeName
                            Datagrid.Cells(row, col).StyleName = sStyleHeaderText
                    End Select
                Next

                col = Address(Datagrid, cTotal).Column + Objs.Count
                For Each a As clsAttribute In AltAttributeNames
                    col = col + 1
                    If row = Address(Datagrid, cCovGUID).Row + 1 Then
                        Datagrid.Cells(row, col).Value = a.Name
                        Datagrid.Cells(row, col).StyleName = sStyleHeaderBottom
                    ElseIf row = Address(Datagrid, cCovGUID).Row Then
                        colGuid.Add(a.ID.ToString, New Cell(row, col))
                    End If
                Next
            Next

            Dim altNum As Integer = 0
            Dim cAltTotal As New List(Of Double)
            For Each a In Alts
                cAltTotal.Add(CDbl(FindRatingCell(a.NodeGuidID, TotalGUID).Data))
            Next

            altNum = 0
            Dim rowStart As Integer = Address(Datagrid, cTotal).Row
            For Each a In Alts
                altNum += 1

                '% AltNum
                Datagrid.Cells(rowStart + altNum, Address(Datagrid, cAltNum).Column).Value = altNum
                Datagrid.Cells(rowStart + altNum, Address(Datagrid, cAltNum).Column).StyleName = sStyleReadOnly

                '% AltGUID
                Datagrid.Cells(rowStart + altNum, Address(Datagrid, cAltGUID).Column).Value = a.NodeGuidID
                Datagrid.Cells(rowStart + altNum, Address(Datagrid, cAltGUID).Column).StyleName = sStyleGUID

                '% AltName
                Datagrid.Cells(rowStart + altNum, Address(Datagrid, cAltName).Column).Value = a.NodeName
                Datagrid.Cells(rowStart + altNum, Address(Datagrid, cAltName).Column).StyleName = CStr(IIf(AttributesReadOnly = True, sStyleReadOnlyTitle, sStyleBiDirectionTitle))

                Dim totalVal As Double = CDbl(FindRatingCell(a.NodeGuidID, clsDataGrid.TotalGUID).Data)

                Dim aTotal = Address(Datagrid, cTotal)

                '% Min
                Datagrid.Cells(rowStart + altNum, Address(Datagrid, cPercentMin).Column).Formula = String.Format("{0}/{1}!{2}", Convert(aTotal.Column) & CStr(rowStart + altNum), dTotalsDG, cTotalMinDG)
                Datagrid.Cells(rowStart + altNum, Address(Datagrid, cPercentMin).Column).StyleName = sStyleReadOnly

                '% Max
                Datagrid.Cells(rowStart + altNum, Address(Datagrid, cPercentMax).Column).Formula = String.Format("{0}/{1}!{2}", Convert(aTotal.Column) & CStr(rowStart + altNum), dTotalsDG, cTotalMaxDG)
                Datagrid.Cells(rowStart + altNum, Address(Datagrid, cPercentMax).Column).StyleName = sStyleReadOnly

                'Normalized
                Datagrid.Cells(rowStart + altNum, Address(Datagrid, cNormalized).Column).Formula = String.Format("{0}/{1}!{2}", Convert(aTotal.Column) & CStr(rowStart + altNum), dTotalsDG, cTotalSumDG)
                Datagrid.Cells(rowStart + altNum, Address(Datagrid, cNormalized).Column).StyleName = sStyleReadOnly

                'Total
                Datagrid.Cells(rowStart + altNum, Address(Datagrid, cTotal).Column).Value = totalVal
                Datagrid.Cells(rowStart + altNum, Address(Datagrid, cTotal).Column).StyleName = sStyleReadOnly

                'Add data for covering objectives for currentalt

                Dim colStart As Integer = Address(Datagrid, cTotal).Column
                Dim colNum As Integer = 0
                For Each c In Objs
                    colNum += 1
                    Dim cell As CellValue = FindRatingCell(a.NodeGuidID, c.NodeGuidID)
                    If CurrentUser IsNot Nothing Then
                        Datagrid.Cells(rowStart + altNum, colStart + colNum).StyleName = CStr(IIf(c.MeasureType = ECMeasureType.mtPairwise, sStyleReadOnly, sStyleBiDirection))
                    Else
                        Datagrid.Cells(rowStart + altNum, colStart + colNum).StyleName = sStyleReadOnly
                    End If

                    If cell IsNot Nothing AndAlso Not cell.IsNull Then 'A1706
                        Datagrid.Cells(rowStart + altNum, colStart + colNum).Value = cell.Data
                    End If
                Next

                'Add data for attributes for currentalt
                Dim AValues As Dictionary(Of Guid, String) = AltAttributeValues(a.NodeGuidID)
                For Each attr As clsAttribute In AltAttributeNames
                    colNum += 1
                    Dim aStyle As String = CStr(IIf(AttributesReadOnly = True, sStyleReadOnly, sStyleBiDirection))
                    Select Case attr.ValueType
                        Case AttributeValueTypes.avtBoolean
                            If AValues(attr.ID) <> "" Then
                                Datagrid.Cells(rowStart + altNum, colStart + colNum).Value = CBool(AValues(attr.ID))
                            End If
                        Case AttributeValueTypes.avtDouble
                            If AValues(attr.ID) <> "" Then
                                Datagrid.Cells(rowStart + altNum, colStart + colNum).Value = CDbl(AValues(attr.ID))
                            End If
                        Case AttributeValueTypes.avtLong
                            If AValues(attr.ID) <> "" Then
                                Datagrid.Cells(rowStart + altNum, colStart + colNum).Value = CLng(AValues(attr.ID))
                            End If
                        Case AttributeValueTypes.avtString
                            Datagrid.Cells(rowStart + altNum, colStart + colNum).Value = CStr(AValues(attr.ID))
                        Case AttributeValueTypes.avtEnumeration
                            Datagrid.Cells(rowStart + altNum, colStart + colNum).Value = AValues(attr.ID).ToString
                        Case AttributeValueTypes.avtEnumerationMulti
                            Datagrid.Cells(rowStart + altNum, colStart + colNum).Value = AValues(attr.ID).ToString
                            aStyle = sStyleReadOnly
                    End Select
                    Datagrid.Cells(rowStart + altNum, colStart + colNum).StyleName = aStyle
                Next
            Next

            Dim MsgRow As Integer = Address(Datagrid, cAltName).Row + Alts.Count + 1
            If Not AttributesReadOnly Then
                Datagrid.Cells(Convert(Address(Datagrid, cAltNum).Column) & MsgRow.ToString).Value = AddNewAltsLabel
            End If

            For i As Integer = 1 To Address(Datagrid, cTotal).Column + Objs.Count + AltAttributeNames.Count
                Datagrid.Cells(Convert(i) & MsgRow.ToString).StyleName = sStyleNewAltMarker
            Next

            Dim mRange As String = String.Format("${0}${1}:${2}${3}",
                                        Convert(Address(Datagrid, cAltGUID).Column),
                                        Address(Datagrid, cAltGUID).Row + 1,
                                        Convert(Address(Datagrid, cAltName).Column),
                                        Address(Datagrid, cAltName).Row + Alts.Count + 1)

            Dim AltStartRow As Integer = Address(Datagrid, cAltGUID).Row + 1
            Dim AltEndRow As Integer = Address(Datagrid, cAltName).Row + Alts.Count + 1

            Dim covGUIDRow As Integer = Address(Datagrid, cCovGUID).Row
            Dim covGUIDStart As String = Convert(Address(Datagrid, cCovGUID).Column + 1)
            Dim covGUIDEnd As String = Convert(Address(Datagrid, cCovGUID).Column + Objs.Count)
            Dim attrGUIDStart As String = Convert(Address(Datagrid, cCovGUID).Column + Objs.Count + 1)
            Dim attrGUIDEnd As String = Convert(Address(Datagrid, cCovGUID).Column + Objs.Count + AltAttributeNames.Count)
            Dim AltInfoStart As String = Convert(Address(Datagrid, cAltNum).Column)
            Dim AltInfoEnd As String = Convert(Address(Datagrid, cAltName).Column)

            mRange = String.Format("{0}!${1}${3}:${2}${3}", dDatagrid, covGUIDStart, covGUIDEnd, covGUIDRow)
            Datagrid.Workbook.Names.Add(rColGUID, Datagrid.Cells(mRange))

            mRange = String.Format("{0}!${1}${3}:${2}${4}", dDatagrid, covGUIDStart, covGUIDEnd, AltStartRow, AltEndRow)
            Datagrid.Workbook.Names.Add(rAltObjData, Datagrid.Cells(mRange))

            mRange = String.Format("{0}!${1}${3}:${2}${4}", dDatagrid, attrGUIDStart, attrGUIDEnd, AltStartRow, AltEndRow)
            Datagrid.Workbook.Names.Add(rAltAttrData, Datagrid.Cells(mRange))

            mRange = String.Format("{0}!${1}${3}:${2}${4}", dDatagrid, AltInfoStart, AltInfoEnd, AltStartRow, AltEndRow)
            Datagrid.Workbook.Names.Add(rAltInfo, Datagrid.Cells(mRange))

            mRange = String.Format("{0},{1},{2},{3}", Datagrid.Workbook.Names(rColGUID).Address, Datagrid.Workbook.Names(rAltInfo).Address, Datagrid.Workbook.Names(rAltObjData).Address, Datagrid.Workbook.Names(rAltAttrData).Address)
            Datagrid.Workbook.Names.Add(rData, Datagrid.Cells(mRange))

            'get max/min from the total columns
            Dim r = Address(Datagrid, cTotal)
            Dim t = Address(TotalsDG, cTotalMinDG)
            TotalsDG.Cells(t.Row, t.Column).Formula = String.Format("MIN({0}!{1}:{2})", dDatagrid, Convert(r.Column) & r.Row + 1, Convert(r.Column) & r.Row + 1 + Alts.Count)
            t = Address(TotalsDG, cTotalMaxDG)
            TotalsDG.Cells(t.Row, t.Column).Formula = String.Format("MAX({0}!{1}:{2})", dDatagrid, Convert(r.Column) & r.Row + 1, Convert(r.Column) & r.Row + 1 + Alts.Count)
            t = Address(TotalsDG, cTotalSumDG)
            TotalsDG.Cells(t.Row, t.Column).Formula = String.Format("SUM({0}!{1}:{2})", dDatagrid, Convert(r.Column) & r.Row + 1, Convert(r.Column) & r.Row + 1 + Alts.Count)

            For i = 1 To Address(Datagrid, cTotal).Column + Objs.Count + Alts.Count
                If Not (Datagrid.Column(i).Hidden) Then
                    Datagrid.Column(i).Width = defaultColWidth
                End If
            Next

            For Each kvp As KeyValuePair(Of String, Cell) In colGuid
                Dim c As Cell = kvp.Value
                Datagrid.Cells(c.Row, c.Col).Value = kvp.Key
            Next

            '========= Setup the Calcuated sheet
            Dim globalRow As Integer = Address(Datagrid, cGlobalCell).Row
            package.Workbook.Worksheets.Add(dCalculated, Datagrid)
            Dim Calculated As ExcelWorksheet = package.Workbook.Worksheets.Where(Function(x) x.Name = dCalculated).First
            package.Workbook.Worksheets.MoveAfter(dCalculated, dDatagrid)

            For Each kvp As KeyValuePair(Of String, Cell) In colGuid
                Dim c As Cell = kvp.Value
                Calculated.Cells(c.Row, c.Col).Value = ""
            Next

            altNum = 0
            rowStart = Address(Datagrid, cTotal).Row
            For Each a In Alts
                altNum += 1

                '% AltNum
                Calculated.Cells(rowStart + altNum, Address(Datagrid, cAltNum).Column).StyleName = sStyleReadOnly

                '% AltGUID
                Calculated.Cells(rowStart + altNum, Address(Datagrid, cAltGUID).Column).Value = a.NodeGuidID
                Calculated.Cells(rowStart + altNum, Address(Datagrid, cAltGUID).Column).StyleName = sStyleGUID

                '% AltName
                Calculated.Cells(rowStart + altNum, Address(Datagrid, cAltName).Column).StyleName = sStyleReadOnlyTitle

                Dim aTotal = Address(Datagrid, cTotal)
                '% Min
                Calculated.Cells(rowStart + altNum, Address(Datagrid, cPercentMin).Column).Formula = String.Format("{0}/{1}!{2}", Convert(aTotal.Column) & CStr(rowStart + altNum), dTotalsDG, cTotalMinDG)
                Calculated.Cells(rowStart + altNum, Address(Datagrid, cPercentMin).Column).StyleName = sStyleReadOnly

                '% Max
                Calculated.Cells(rowStart + altNum, Address(Datagrid, cPercentMax).Column).Formula = String.Format("{0}/{1}!{2}", Convert(aTotal.Column) & CStr(rowStart + altNum), dTotalsDG, cTotalMaxDG)
                Calculated.Cells(rowStart + altNum, Address(Datagrid, cPercentMax).Column).StyleName = sStyleReadOnly

                'Normalized
                Calculated.Cells(rowStart + altNum, Address(Datagrid, cNormalized).Column).Formula = String.Format("{0}/{1}!{2}", Convert(aTotal.Column) & CStr(rowStart + altNum), dTotalsDG, cTotalSumDG)
                Calculated.Cells(rowStart + altNum, Address(Datagrid, cNormalized).Column).StyleName = sStyleReadOnly

                'Add data for covering objectives for currentalt
                Dim colStart As Integer = Address(Datagrid, cTotal).Column
                Dim colNum As Integer = 0
                Dim TotalFormula As String = ""
                For Each c In Objs
                    colNum += 1
                    Dim cell As CellValue = FindRatingCell(a.NodeGuidID, c.NodeGuidID)
                    Calculated.Cells(rowStart + altNum, colStart + colNum).StyleName = sStyleReadOnly
                    If cell IsNot Nothing AndAlso Not cell.IsNull Then 'A1706
                        Calculated.Cells(rowStart + altNum, colStart + colNum).Value = cell.Priority
                    End If
                    TotalFormula += String.Format("+({0}*{1})", Calculated.Cells(rowStart + altNum, colStart + colNum).Address, Calculated.Cells(globalRow, colStart + colNum).Address)
                Next
                'Total
                Calculated.Cells(rowStart + altNum, Address(Datagrid, cTotal).Column).Formula = TotalFormula
                Calculated.Cells(rowStart + altNum, Address(Datagrid, cTotal).Column).StyleName = sStyleReadOnly
            Next

            'get max/min from the total columns
            r = Address(Datagrid, cTotal)
            t = Address(TotalsDG, cTotalMinDG)
            TotalsCalc.Cells(t.Row, t.Column).Formula = String.Format("MIN({0}!{1}:{2})", dCalculated, Convert(r.Column) & r.Row + 1, Convert(r.Column) & r.Row + 1 + Alts.Count)
            t = Address(TotalsDG, cTotalMaxDG)
            TotalsCalc.Cells(t.Row, t.Column).Formula = String.Format("MAX({0}!{1}:{2})", dCalculated, Convert(r.Column) & r.Row + 1, Convert(r.Column) & r.Row + 1 + Alts.Count)
            t = Address(TotalsDG, cTotalSumDG)
            TotalsCalc.Cells(t.Row, t.Column).Formula = String.Format("SUM({0}!{1}:{2})", dCalculated, Convert(r.Column) & r.Row + 1, Convert(r.Column) & r.Row + 1 + Alts.Count)

            For i = 1 To Address(Datagrid, cTotal).Column + Objs.Count + Alts.Count
                If Not (Calculated.Column(i).Hidden) Then
                    Calculated.Column(i).Width = defaultColWidth
                End If
            Next

            For Each kvp As KeyValuePair(Of String, Cell) In colGuid
                Dim c As Cell = kvp.Value
                Calculated.Cells(c.Row, c.Col).Value = kvp.Key
            Next

            '==============cleanup========================
            Calculated.Row(Address(Datagrid, cCovGUID).Row).Hidden = True
            Calculated.Column(Address(Datagrid, cAltGUID).Column).Hidden = True
            TotalsCalc.Hidden = eWorkSheetHidden.VeryHidden
            TotalsDG.Hidden = eWorkSheetHidden.VeryHidden
            Styles.Hidden = eWorkSheetHidden.VeryHidden
            Datagrid.Row(Address(Datagrid, cCovGUID).Row).Hidden = True
            Datagrid.Column(Address(Datagrid, cAltGUID).Column).Width = 8

            Datagrid.Workbook.Names(cUserName).IsNameHidden = True
            Datagrid.Workbook.Names(cFilename).IsNameHidden = True
            Datagrid.Workbook.Names(cCalcMode).IsNameHidden = True
            Datagrid.Workbook.Names(cTotal).IsNameHidden = True
            Datagrid.Workbook.Names(cAltNum).IsNameHidden = True
            Datagrid.Workbook.Names(cAltName).IsNameHidden = True
            Datagrid.Workbook.Names(cAltGUID).IsNameHidden = True
            Datagrid.Workbook.Names(cCovGUID).IsNameHidden = True
            Datagrid.Workbook.Names(cPercentMin).IsNameHidden = True
            Datagrid.Workbook.Names(cPercentMax).IsNameHidden = True
            Datagrid.Workbook.Names(cNormalized).IsNameHidden = True
            Datagrid.Workbook.Names(cGlobalCell).IsNameHidden = True
            TotalsDG.Workbook.Names(cTotalMinDG).IsNameHidden = True
            TotalsDG.Workbook.Names(cTotalMaxDG).IsNameHidden = True
            TotalsDG.Workbook.Names(cTotalSumDG).IsNameHidden = True
            package.Workbook.Worksheets(dDatagrid).View.TabSelected = True
            package.Save()
        End Using
    End Sub

    Public Sub WriteTotalsByInstance(Filename As String, pm As clsProjectManager)
        'erase the file if it already exists
        Try
            If IO.File.Exists(Filename) Then
                IO.File.Delete(Filename)
            End If
        Catch
        End Try

        Dim MasterXLSX = System.AppDomain.CurrentDomain.BaseDirectory & "App_Data/Datagrid2/Master2.xlsx"
        System.IO.File.Copy(MasterXLSX, Filename)

        Dim fi = New FileInfo(Filename)
        Using package As New ExcelPackage(fi)

            Dim Datagrid As ExcelWorksheet = package.Workbook.Worksheets.Where(Function(x) x.Name = dDatagrid).First
            Dim startObjCol As Integer = Datagrid.Cells(Datagrid.Workbook.Names(cTotal).Address).Start.Column + 1

            Dim altNum As Integer = 0
            Dim cAltTotal As New List(Of Double)
            For Each a As clsNode In Alts
                cAltTotal.Add(CDbl(FindRatingCell(a.NodeGuidID, TotalGUID).Data))
            Next

            altNum = 0
            Static instNum As Integer 'number of participants
            Dim rowStart As Integer = Address(Datagrid, cTotal).Row ' + instanceNum

            Dim instRow As Integer = 2 + instNum 'Address(Datagrid, cInstance).Row
            Dim instStart As String = "B" 'Convert(Address(Datagrid, cInstance).Column + 1)
            Dim instEnd As String = "B" 'Convert(Address(Datagrid, cInstance).Column + Objs.Count)


            Dim mRange As String = String.Format("{0}!${1}${3}:${2}${3}", dDatagrid, instStart, instEnd, instRow)
            Datagrid.Workbook.Names.Add(rInstance, Datagrid.Cells(mRange))

            '=====================
            For Each alt As clsNode In Alts 'loop through all alternatives
                For Each user As clsUser In pm.UsersList 'loop through all participants
                    If user IsNot Nothing Then
                        'load user data and calculale priorities
                        pm.StorageManager.Reader.LoadUserData(user)
                        Dim CT As clsCalculationTarget = Nothing
                        CT = New clsCalculationTarget(CalculationTargetType.cttUser, user)
                        pm.CalculationsManager.Calculate(CT, pm.Hierarchy(pm.ActiveHierarchy).Nodes(0))

                        altNum += 1

                        Datagrid.Cells(rowStart + altNum, Address(Datagrid, cAltName).Column).Value = alt.NodeName
                        Datagrid.Cells(rowStart + altNum, Address(Datagrid, cAltName).Column).StyleName = CStr(IIf(AttributesReadOnly = True, sStyleReadOnlyTitle, sStyleBiDirectionTitle))

                        Datagrid.Cells(rowStart + altNum, Address(Datagrid, cInstance).Column).Value = user.UserName
                        Datagrid.Cells(rowStart + altNum, Address(Datagrid, cInstance).Column).StyleName = sStyleReadOnly

                        'calk totals
                        Dim totalVal As Double = 0
                        For Each covObj As clsNode In pm.Hierarchy(pm.ActiveHierarchy).TerminalNodes 'AS/11339===
                            Dim DGPriority As Double = Nothing
                            Select Case covObj.MeasureType
                                Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous, ECMeasureType.mtPWOutcomes
                                    DGPriority = covObj.Judgments.Weights.GetUserWeights(user.UserID, ECSynthesisMode.smIdeal, pm.CalculationsManager.IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(alt.NodeID)
                                Case ECMeasureType.mtRatings
                                    Dim rData As clsRatingMeasureData = CType(CType(covObj.Judgments, clsNonPairwiseJudgments).GetJudgement(alt.NodeID, covObj.NodeID, user.UserID), clsRatingMeasureData)
                                    If rData IsNot Nothing AndAlso rData.Rating IsNot Nothing Then
                                        DGPriority = rData.Rating.Value
                                    Else
                                        DGPriority = 0
                                    End If
                                Case Else
                                    Dim nonpwData As clsNonPairwiseMeasureData = CType(covObj.Judgments, clsNonPairwiseJudgments).GetJudgement(alt.NodeID, covObj.NodeID, user.UserID)
                                    If nonpwData IsNot Nothing AndAlso Not nonpwData.IsUndefined Then
                                        DGPriority = nonpwData.SingleValue
                                    Else
                                        DGPriority = 0
                                    End If
                            End Select
                            totalVal = totalVal + DGPriority * covObj.UnnormalizedPriority
                        Next 'AS/11339==

                        'write Total
                        Datagrid.Cells(rowStart + altNum, Address(Datagrid, cTotal).Column).Value = totalVal
                        Datagrid.Cells(rowStart + altNum, Address(Datagrid, cTotal).Column).StyleName = sStyleReadOnly
                        totalVal = 0

                    End If
                Next
            Next


            'Dim MsgRow As Integer = Address(Datagrid, cAltName).Row + Alts.Count + 1
            'If Not AttributesReadOnly Then
            '    Datagrid.Cells(Convert(Address(Datagrid, cAltNum).Column) & MsgRow.ToString).Value = AddNewAltsLabel
            'End If

            'For i As Integer = 1 To Address(Datagrid, cTotal).Column + Objs.Count + AltAttributeNames.Count
            '    Datagrid.Cells(Convert(i) & MsgRow.ToString).StyleName = sStyleNewAltMarker
            'Next

            '==============cleanup========================

            Datagrid.Workbook.Names(cUserName).IsNameHidden = True
            Datagrid.Workbook.Names(cFilename).IsNameHidden = True
            Datagrid.Workbook.Names(cCalcMode).IsNameHidden = True
            Datagrid.Workbook.Names(cTotal).IsNameHidden = True
            Datagrid.Workbook.Names(cAltNum).IsNameHidden = True
            Datagrid.Workbook.Names(cAltName).IsNameHidden = True
            Datagrid.Workbook.Names(cAltGUID).IsNameHidden = True
            Datagrid.Workbook.Names(cCovGUID).IsNameHidden = True
            Datagrid.Workbook.Names(cPercentMin).IsNameHidden = True
            Datagrid.Workbook.Names(cPercentMax).IsNameHidden = True
            Datagrid.Workbook.Names(cNormalized).IsNameHidden = True
            Datagrid.Workbook.Names(cGlobalCell).IsNameHidden = True
            package.Workbook.Worksheets(dDatagrid).View.TabSelected = True
            package.Save()
        End Using
    End Sub

    Private Const cUserName As String = "username"
    Private Const cFilename As String = "filename"
    Private Const cCalcMode As String = "calcmode"
    Private Const cTotal As String = "total"
    Private Const cAltNum As String = "altnum"
    Private Const cAltName As String = "altname"
    Private Const cInstance As String = "instance" 'AS/11339
    Private Const cAltDataRange As String = "Data"
    Private Const cAltGUID As String = "altguid"
    Private Const cCovGUID As String = "covguid"
    Private Const cPercentMin As String = "percentmin"
    Private Const cPercentMax As String = "percentmax"
    Private Const cNormalized As String = "normalized"
    Private Const cTotalMinDG As String = "totalminDG"
    Private Const cTotalMaxDG As String = "totalmaxDG"
    Private Const cTotalSumDG As String = "totalsumDG"
    Private Const cGlobalCell As String = "globalcell"

    Private Const rAltInfo As String = "AltInfo"
    Private Const rAltObjData As String = "AltObjData"
    Private Const rAltAttrData As String = "AltAttrData"
    Private Const rColGUID As String = "colGUID"
    Private Const rData As String = "Data"
    Private Const rInstance As String = "Instance" 'AS/11339

    Private Const dDatagrid As String = "Datagrid"
    Private Const dCalculated As String = "Calculated"
    Private Const dTotalsDG As String = "TotalsDG"
    Private Const dTotalsCalc As String = "TotalsCalc"
    Private Const dStyles As String = "Styles"

    Private Const sStyleHeaderText As String = "styleHeaderText"
    Private Const sStyleHeaderNum As String = "styleHeaderNum"
    Private Const sStyleHeaderBottom As String = "styleHeaderBottom"
    Private Const sStyleReadOnly As String = "styleReadOnly"
    Private Const sStyleReadOnlyTitle As String = "styleReadOnlyTitle"
    Private Const sStyleGUID As String = "styleGUID"
    Private Const sStyleNewAltMarker As String = "styleNewAltMarker"

    Private Const sStyleBiDirection As String = "styleBiDirection"
    Private Const sStyleBiDirectionTitle As String = "styleBiDirectionTitle"
    Private Const defaultColWidth As Integer = 18

    Private Shared Function Address(Worksheet As ExcelWorksheet, Range As String) As ExcelCellAddress
        Return Worksheet.Cells(Worksheet.Workbook.Names(Range).Address).Start
    End Function

    Private Function AddressEnd(Worksheet As ExcelWorksheet, Range As String) As ExcelCellAddress
        Return Worksheet.Cells(Worksheet.Workbook.Names(Range).Address).End
    End Function

    Private Class Cell
        Sub New(Row As Integer, col As Integer)
            Me.Row = Row
            Me.Col = col
        End Sub
        Public Row As Integer
        Public Col As Integer
    End Class

    Private Shared Function Convert(n As Integer) As String
        Dim abc As String = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
        Dim res As String = ""

        While n > 26
            Dim q As Integer = n Mod 26
            If q = 0 Then q = 26
            res = abc.Substring(q - 1, 1) + res
            n = (n - 1) \ 26
        End While
        res = abc.Substring(n - 1, 1) + res

        Return res
    End Function

End Class

Public Class DataGridException
    Inherits Exception
    Sub New(Message As String)
        MyBase.New(Message)
    End Sub
End Class

Public Class DataGridExceptionInvalidRisk
    Inherits DataGridException
    Sub New(Message As String)
        MyBase.New(Message)
    End Sub
End Class

Public Class DataGridExceptionDuplicateGUID
    Inherits DataGridException
    Sub New(Message As String)
        MyBase.New(Message)
    End Sub
End Class

Public Class DataGridExceptionInvalidDirect
    Inherits DataGridException
    Sub New(Message As String)
        MyBase.New(Message)
    End Sub
End Class

' D7517 ===
Public Class DataGridExceptionInvalidRating
    Inherits DataGridException
    Sub New(Message As String)
        MyBase.New(Message)
    End Sub
End Class
' D7517 ==