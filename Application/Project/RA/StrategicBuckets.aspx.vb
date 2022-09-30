Option Strict On

Imports Telerik.Web.UI
Imports System.Data
Imports System.Drawing

Partial Class RAStrategicBucketsPage
    Inherits clsComparionCorePage

    Public Const COL_GUID As Integer = 0
    Public Const COL_ID As Integer = 1
    Public Const COL_NAME As Integer = 2
    Public Const COL_FUNDED As Integer = 3
    Public Const COL_TOTAL As Integer = 4
    Public Const COL_COST As Integer = 5
    Public Const COL_ATRIBUTES_START_INDEX As Integer = 6

    Private FUNDED_CELL_FILL_COLOR As Color = Color.FromArgb(209, 230, 181)

    Public Const ACTION_DELETE_NODE As String = "delete_node"
    Public Const ACTION_ADD_ALTERNATIVES As String = "add_alternatives"

    Private Sub SaveSetting(ID As Guid, valueType As AttributeValueTypes, value As Object)
        With App.ActiveProject.ProjectManager
            .Attributes.SetAttributeValue(ID, UNDEFINED_USER_ID, ValueType, value, Guid.Empty, Guid.Empty)
            .Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID, UNDEFINED_USER_ID)
        End With
    End Sub

    Public Property NormalizeMode As Integer
        Get
            Return CInt(App.ActiveProject.ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_RA_SB_NORMALIZATION_MODE_ID, UNDEFINED_USER_ID))            
        End Get
        Set(value As Integer)
            SaveSetting(ATTRIBUTE_RA_SB_NORMALIZATION_MODE_ID, AttributeValueTypes.avtLong, value)
        End Set
    End Property

    Public Property SynthesisMode As Integer
        Get
            Return CInt(App.ActiveProject.ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_RA_SB_SYNTHESIS_MODE_ID, UNDEFINED_USER_ID))
        End Get
        Set(value As Integer)
            SaveSetting(ATTRIBUTE_RA_SB_SYNTHESIS_MODE_ID, AttributeValueTypes.avtLong, value)
        End Set
    End Property

    Public ReadOnly Property RAAlternatives As List(Of RAAlternative)
        Get
            'If CurrentPageID = _PGID_RA_STRATEGIC_BUCKETS Then Return RA.Scenarios.ActiveScenario.Alternatives
            'If CurrentPageID = _PGID_STRUCTURE_ALTERNATIVES Then Return RA.Scenarios.Scenarios(0).AlternativesFull
            Return RA.Scenarios.ActiveScenario.Alternatives
        End Get
    End Property

    Public ReadOnly Property Alternatives As List(Of clsNode)
        Get
            Return PM.ActiveAlternatives.TerminalNodes
        End Get
    End Property

    Public ReadOnly Property AlternativesCount As Integer
        Get
            'If CurrentPageID = _PGID_RA_STRATEGIC_BUCKETS Then Return RAAlternatives.Count
            'If CurrentPageID = _PGID_STRUCTURE_ALTERNATIVES Then Return Alternatives.Count
            Return RAAlternatives.Count
        End Get
    End Property

    Private _SESS_SELECTED_ALT_ID As String = "RA_SB_SELECTED_ALT_ID"
    Public Property SelectedAltID As Guid
        Get
            Dim retVal As Guid = Guid.Empty

            Dim tSess As String = SessVar(_SESS_SELECTED_ALT_ID)
            If Not String.IsNullOrEmpty(tSess) Then
                retVal = New Guid(tSess)
            End If

            If retVal.Equals(Guid.Empty) AndAlso AlternativesCount > 0 Then
                retVal = Alternatives(0).NodeGuidID
            End If
            Return retVal
        End Get
        Set(value As Guid)
            SessVar(_SESS_SELECTED_ALT_ID) = value.ToString
        End Set
    End Property

    Public Const OPT_BAR_WIDTH As Integer = 40
    Public Const OPT_BAR_HEIGHT As Integer = 2
    Public Const OPT_BAR_COLOR_FILLED As String = "#8899cc"
    Public Const OPT_BAR_COLOR_EMPTY As String = "#d0d0d0"

    Public Sub New()
        MyBase.New(_PGID_RA_STRATEGIC_BUCKETS)
    End Sub

    ReadOnly Property RA As ResourceAligner
        Get
            Return App.ActiveProject.ProjectManager.ResourceAligner
        End Get
    End Property

    Public ReadOnly Property BaseScenario As RAScenario
        Get
            Return RA.Scenarios.Scenarios(0)
        End Get
    End Property

    ReadOnly Property PM As clsProjectManager
        Get
            Return App.ActiveProject.ProjectManager
        End Get
    End Property

    Public Property RASortColumn As Integer
        Get
            Return Math.Abs(CInt(App.ActiveProject.ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_RA_SB_SORT_COLUMN_ID, UNDEFINED_USER_ID)))
        End Get
        Set(value As Integer)
            Dim tDir As Integer = CInt(IIf(RASortDirection = "DESC", -1, 1))
            SaveSetting(ATTRIBUTE_RA_SB_SORT_COLUMN_ID, AttributeValueTypes.avtLong, Math.Abs(value) * tDir)
        End Set
    End Property

    Public Property RASortDirection As String
        Get
            Dim tVal As Integer = CInt(App.ActiveProject.ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_RA_SB_SORT_COLUMN_ID, UNDEFINED_USER_ID))
            If tVal < 0 Then Return "DESC" Else Return "ASC"
        End Get
        Set(value As String)
            Dim tVal = RASortColumn
            If value.ToUpper = "DESC" Then
                tVal = -tVal
            End If
            SaveSetting(ATTRIBUTE_RA_SB_SORT_COLUMN_ID, AttributeValueTypes.avtLong, tVal)
        End Set
    End Property

    Public Const OPT_SCENARIO_LEN As Integer = 45       'A0965
    Public Const OPT_DESCRIPTION_LEN As Integer = 200   'A0965

    Public Function GetScenarios() As String
        Dim sRes As String = ""
        For Each tScen As Integer In RA.Scenarios.Scenarios.Keys
            sRes += String.Format("<option value='{0}'{2}>{1}</option>", tScen, ShortString(SafeFormString(RA.Scenarios.Scenarios(tScen).Name), OPT_SCENARIO_LEN, True) + CStr(IIf(String.IsNullOrEmpty(SafeFormString(RA.Scenarios.Scenarios(tScen).Description)), "", String.Format(" ({0})", ShortString(SafeFormString(RA.Scenarios.Scenarios(tScen).Description), OPT_DESCRIPTION_LEN)))), IIf(tScen = RA.Scenarios.ActiveScenarioID, " selected", ""))
        Next
        Return String.Format("<select id='cbScenarios' style='width:130px; margin-top:3px; margin-right:2px;' onchange='onSetScenario(this.value);'>{0}</select>", sRes)
    End Function

    Public Function GetAttributeItemsData(attr As clsAttribute, items As clsAttributeEnumeration) As String
        Dim sItems As String = ""
        If items IsNot Nothing Then

            Dim defaultItemsGuids As new List(Of Guid)
            If attr.DefaultValue IsNot Nothing Then
                If attr.ValueType = AttributeValueTypes.avtEnumeration Then
                    If TypeOf attr.DefaultValue Is Guid Then defaultItemsGuids.Add(CType(attr.DefaultValue, Guid))
                    If TypeOf attr.DefaultValue Is String Then defaultItemsGuids.Add(New Guid(CStr(attr.DefaultValue)))
                End If
                If attr.ValueType = AttributeValueTypes.avtEnumerationMulti Then
                    If TypeOf attr.DefaultValue Is Guid Then defaultItemsGuids.Add(CType(attr.DefaultValue, Guid))
                    If TypeOf attr.DefaultValue Is String Then
                        Dim sGuids As String() = CStr(attr.DefaultValue).Split(CChar(";"))
                        For Each sGuid As String In sGuids
                            defaultItemsGuids.Add(New Guid(sGuid))
                        Next
                    End If
                End If
            End If

            Dim itemIndex As Integer = 0
            For Each item As clsAttributeEnumerationItem In items.Items
                Dim isDefault As Boolean = defaultItemsGuids.Contains(item.ID)
                sItems += CStr(IIf(sItems = "", "", ",")) + String.Format("['{0}','{1}',{2}]", itemIndex, JS_SafeString(item.Value), CStr(IIf(isDefault, "1", "0")))
                itemIndex += 1
            Next
        End If
        Return sItems
    End Function

    Public Function GetAttributesData() As String
        Dim sAttrs As String = ""
        Dim attrIndex As Integer = 0
        For Each attr As clsAttribute In PM.Attributes.GetAlternativesAttributes(True)
            Dim sValue As String = "''" ' enum attribute items or string/int/double/bool default value
            If attr.DefaultValue IsNot Nothing Then sValue = attr.DefaultValue.ToString()

            Select Case attr.ValueType
                Case AttributeValueTypes.avtString '0
                    If attr.DefaultValue IsNot Nothing Then sValue = String.Format("'{0}'", JS_SafeString(attr.DefaultValue))
                Case AttributeValueTypes.avtBoolean '1
                    If attr.DefaultValue IsNot Nothing Then sValue = String.Format("{0}", CInt(IIf(CBool(attr.DefaultValue), 1, 0)))
                Case AttributeValueTypes.avtLong '2
                    If attr.DefaultValue IsNot Nothing Then sValue = String.Format("{0}", JS_SafeNumber(attr.DefaultValue))
                Case AttributeValueTypes.avtDouble '3
                    If attr.DefaultValue IsNot Nothing Then sValue = String.Format("{0}", JS_SafeNumber(attr.DefaultValue))
                Case AttributeValueTypes.avtEnumeration '4
                    sValue = String.Format("[{0}]", GetAttributeItemsData(attr, PM.Attributes.GetEnumByID(attr.EnumID)))
                Case AttributeValueTypes.avtEnumerationMulti '5
                    sValue = String.Format("[{0}]", GetAttributeItemsData(attr, PM.Attributes.GetEnumByID(attr.EnumID)))
            End Select

            sAttrs += CStr(IIf(sAttrs = "", "", ",")) + String.Format("[{0},'{1}',{2},{3}]", attrIndex, JS_SafeString(attr.Name), sValue, CInt(attr.ValueType))
            attrIndex += 1
        Next
        Return String.Format("var attr_data = [{0}];", sAttrs)
    End Function

    'Protected Sub Page_PreRender(sender As Object, e As System.EventArgs) Handles Me.PreRender
    '    If AlternativesCount > 0 Then RA.Save()
    'End Sub

    Protected Sub Page_Init(sender As Object, e As EventArgs) Handles Me.Init
        Dim sAction = EcSanitizer.GetSafeHtmlFragment(CheckVar(_PARAM_ACTION, "")).Trim.ToLower   ' Anti-XSS
        Select Case sAction
            Case "scenario"
                Dim ID As Integer = CheckVar("sid", RA.Scenarios.ActiveScenarioID)
                If ID <> RA.Scenarios.ActiveScenarioID AndAlso RA.Scenarios.Scenarios.ContainsKey(ID) Then
                    RA.Scenarios.ActiveScenarioID = ID
                    RA.Solver.ResetSolver()
                End If
                Response.Redirect(PageURL(CurrentPageID, GetTempThemeURI(False)), True)
                'Case "alts"
                '    CurrentPageID = _PGID_STRUCTURE_ALTERNATIVES
        End Select
    End Sub

    Protected Sub Page_InitComplete(sender As Object, e As EventArgs) Handles Me.InitComplete
        If Not IsPostBack AndAlso Not isCallback Then
            'If RA.Scenarios.Scenarios.Count = 0 Then RA.Load() ' -D4857
            RA.Load()
            ReCalculate(SynthesisMode = 0) 'A0926
            AlignVerticalCenter = False
            RA.Scenarios.CheckModel() 'A1324
            If RA.Solver.SolverState <> raSolverState.raSolved AndAlso RA.Scenarios.GlobalSettings.isAutoSolve Then RA.Solve() ' D3900
            InitAttributesList()
            'InitAlternatives()
            ClientScript.RegisterStartupScript(GetType(String), "init", "InitPage();checkGlyphButtons();", True)
            InitDataGrid()
        End If

        If Not IsPostBack AndAlso Not IsCallback Then InitToolBarResources()

        Ajax_Callback(Request.Form.ToString)
    End Sub

    Private Sub InitToolBarResources()
        Dim btn As RadToolBarItem = Nothing
        'btn = RadToolBarMain.FindItemByValue("lbl_totals")
        'If btn IsNot Nothing Then btn.Text = ResString("lblTotals") + ":"
        'btn = RadToolBarMain.FindItemByValue("lbl_mode")
        'If btn IsNot Nothing Then btn.Text = ResString("lblMode") + ":"
        'btn = RadToolBarMain.FindItemByValue("lbl_columns")
        'If btn IsNot Nothing Then btn.Text = ResString("lblRAColumns") + ":"
        'btn = RadToolBarMain.FindItemByValue("lbl_items")
        'If btn IsNot Nothing Then btn.Text = ResString("lblItems") + ":"
        'btn = RadToolBarMain.FindItemByValue("lbl_decimals")
        'If btn IsNot Nothing Then btn.Text = ResString("lblDecimals") + ":"
        btn = RadToolBarMain.FindItemByValue("btn_edit_attributes")
        If btn IsNot Nothing Then btn.Text = ResString("btnRAEditAttributes")
        'btn = RadToolBarMain.FindItemByValue("btn_edit_attributes")
        'If btn IsNot Nothing Then btn.Enabled = Not IsReadOnly()
        'RadToolBarMain.FindItemByValue("lbl_scenarios").Text = ResString("lblScenario") + ":"
        If CurrentPageID <> _PGID_RA_STRATEGIC_BUCKETS Then
            btn = RadToolBarMain.FindItemByValue("lblOptScenario")
            If btn IsNot Nothing Then btn.Visible = False
            btn = RadToolBarMain.FindItemByValue("cbOptScenario")
            If btn IsNot Nothing Then btn.Visible = False
        End If

        'btn = RadToolBarMain.FindItemByValue(ACTION_ADD_ALTERNATIVES)
        'If btn IsNot Nothing then
        '    btn.Text = ResString("btnAddAlternatives")
        '    If CurrentPageID = _PGID_STRUCTURE_ALTERNATIVES Then btn.Visible = True
        'End If
        'btn = RadToolBarMain.FindItemByValue(ACTION_DELETE_NODE)
        'If btn IsNot Nothing then
        '    btn.Text = ResString("btnDelete")
        '    If CurrentPageID = _PGID_STRUCTURE_ALTERNATIVES Then btn.Visible = True
        'End If

    End Sub

    Public Sub OnLoadEditButton(sender As Object, e As EventArgs)
        If Not IsPostBack AndAlso Not IsCallback Then CType(sender, RadToolBarButton).Attributes.Add("style", "margin-left:10px; margin-right:2px;")
    End Sub

    Private Sub InitAttributesList()
        PM.Attributes.ReadAttributes(AttributesStorageType.astStreamsDatabase, PM.StorageManager.ProjectLocation, PM.StorageManager.ProviderType, PM.StorageManager.ModelID)
        AttributesList.Clear()
        Dim HasAttributes As Boolean = PM.Attributes IsNot Nothing AndAlso PM.Attributes.AttributesList IsNot Nothing AndAlso PM.Attributes.AttributesList.Count > 0
        If HasAttributes Then
            PM.Attributes.ReadAttributeValues(AttributesStorageType.astStreamsDatabase, PM.StorageManager.ProjectLocation, PM.StorageManager.ProviderType, PM.StorageManager.ModelID, -1)
            AttributesList = PM.Attributes.GetAlternativesAttributes(True) '.Where(Function(attr) attr.ValueType = AttributeValueTypes.avtEnumeration).ToList
        End If
    End Sub

    'Private Sub InitAlternatives()
    '    For Each alt As RAAlternative In RAAlternatives
    '        alt.SBPriority = alt.BenefitOriginal
    '    Next
    'End Sub

    Private Sub InitDataGrid()
        If GridAlternatives.DataSource Is Nothing Then
            Dim DS As DataView = CreateDataSource()
            GridAlternatives.DataSource = DS
            GridAlternatives.DataBind()
        End If
    End Sub

    Private Function GetNewColumnName(tDataTable As DataTable, tSuggestedName As String, tUniqueID As String) As String
        If tDataTable.Columns IsNot Nothing AndAlso tDataTable.Columns.Count > 0 Then
            For Each clmn As DataColumn In tDataTable.Columns
                If clmn.ColumnName = tSuggestedName Then Return String.Format("{0}/{1}", tSuggestedName, tUniqueID)
            Next
        End If
        Return tSuggestedName
    End Function

    Private AttributesList As New List(Of clsAttribute)

    Private Function CreateDataSource() As DataView
        Dim dt As New DataTable()
        Dim dr As DataRow

        If AlternativesCount > 0 Then
            dt.Columns.Add(New DataColumn("GUID", GetType(String)))
            dt.Columns.Add(New DataColumn("No", GetType(Int32)))
            dt.Columns.Add(New DataColumn(ResString("tblAlternativeName"), GetType(String)))    ' D7046
            dt.Columns.Add(New DataColumn("Funded", GetType(String)))
            dt.Columns.Add(New DataColumn("Total", GetType(String)))
            dt.Columns.Add(New DataColumn("Costs", GetType(String)))

            For Each attr In AttributesList                
                    Dim colType As Type = GetType(String)
                    Select Case attr.ValueType
                        Case AttributeValueTypes.avtString, AttributeValueTypes.avtEnumeration
                            colType = GetType(String)
                        Case AttributeValueTypes.avtBoolean
                            colType = GetType(Boolean)
                        Case AttributeValueTypes.avtDouble, AttributeValueTypes.avtLong
                            colType = GetType(Double)
                        Case AttributeValueTypes.avtEnumerationMulti
                            colType = GetType(String)
                    End Select
                Dim dc As DataColumn = New DataColumn(GetNewColumnName(dt, attr.Name.Trim, attr.ID.ToString), colType)
                dt.Columns.Add(dc)
            Next

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

                For Each alt As RAAlternative In RAAlternatives
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

            If Math.Abs(coef) < 0.0000001 Then coef = 1
            Dim IsSolved As Boolean = RA.Solver.SolverState = raSolverState.raSolved

            For Each alt As RAAlternative In RAAlternatives
                dr = dt.NewRow()
                dr(COL_GUID) = alt.ID
                dr(COL_ID) = alt.SortOrder
                dr(COL_NAME) = alt.Name
                If IsSolved Then
                    dr(COL_FUNDED) = CStr(IIf(alt.DisplayFunded >= 1, ResString("lblYes"), IIf(alt.DisplayFunded > 0, String.Format("{0}% ({1})", CostString(alt.Funded * 100), CostString(alt.Funded * alt.Cost)), ""))) 'A0939
                Else
                    dr(COL_FUNDED) = ""
                End If
                alt.SBTotal = alt.SBPriority / coef
                dr(COL_TOTAL) = alt.SBTotal.ToString("F" + PM.Parameters.DecimalDigits.ToString)
                dr(COL_COST) = CostString(alt.Cost) ' D3199

                'fill attributes data
                Dim tAltGuid As Guid = New Guid(alt.ID)
                For Each attr In AttributesList
                    Dim attrValue As Object = PM.Attributes.GetAttributeValue(attr.ID, tAltGuid)
                    Select Case attr.ValueType
                        Case AttributeValueTypes.avtString
                            dr(COL_ATRIBUTES_START_INDEX + AttributesList.IndexOf(attr)) = CStr(attrValue)
                        Case AttributeValueTypes.avtBoolean
                            dr(COL_ATRIBUTES_START_INDEX + AttributesList.IndexOf(attr)) = CBool(attrValue)
                        Case AttributeValueTypes.avtDouble, AttributeValueTypes.avtLong
                            dr(COL_ATRIBUTES_START_INDEX + AttributesList.IndexOf(attr)) = CDbl(attrValue)
                        Case AttributeValueTypes.avtEnumeration, AttributeValueTypes.avtEnumerationMulti
                            dr(COL_ATRIBUTES_START_INDEX + AttributesList.IndexOf(attr)) = PM.Attributes.GetAttributeValueString(attr.ID, tAltGuid)
                    End Select

                Next
                dt.Rows.Add(dr)
            Next
        End If

        Dim dv As New DataView(dt)
        If RASortColumn > 0 AndAlso Not String.IsNullOrEmpty(RASortDirection) AndAlso dt.Columns.Count > 0 AndAlso dt.Columns.Count > RASortColumn Then
            dv.Sort = dt.Columns(RASortColumn).ColumnName + " " + RASortDirection
        End If
        Return dv
    End Function

    Private Function GetSortCaption(idx As Integer, sName As String) As String
        Dim sExtra As String = ""
        Dim Dir As Integer = CInt(IIf(RASortDirection = "ASC", SortDirection.Descending, SortDirection.Ascending))
        If RASortColumn = idx Then
            sExtra = CStr(IIf(RASortDirection = "DESC", _SORT_DESC, _SORT_ASC))
        Else
            Dir = CInt(IIf(Dir = SortDirection.Ascending, SortDirection.Descending, SortDirection.Ascending))
        End If
        Return String.Format(CStr("<a href='' onclick='DoSort({0},{1}); return false;' class='actions'>{2}{3}</a>"), idx, Dir, ShortString(SafeFormString(sName), 80, True), sExtra)
    End Function

    Private Sub CopyPasteHeaderButtons(Cell As TableCell, tUniqueID As String, HasCopyButton As Boolean, HasPasteButton As Boolean, IsCatAttributeColumn As Boolean)
        If HasPasteButton OrElse HasCopyButton OrElse IsCatAttributeColumn Then
            'Cell.Text += String.Format("&nbsp;&nbsp;<img align='right' style='vertical-align:middle;cursor:context-menu;' id='mnu_img_{0}' src='{1}menu_dots.png' alt='' onclick='showMenu(event, ""{0}"", {2}, {3});' />", tUniqueID, ImagePath, CStr(IIf(HasCopyButton, "true", "false")), CStr(IIf(HasPasteButton, "true", "false")))
            'DA: AD's version when icon aligner at the right, but caption enclosed to the table
            Cell.Text = String.Format("<table border=0 cellspacing=0 cellpadding=0 style='width:100%'><tr valign='middle' class='grid_clear'>" + _
                                      "<td class='text' align='center' width='99%'>{4}</td>" + _
                                      "<td style='width:15px' align='right'><img src='{1}menu_dots.png' width='12' height='12' style='cursor:context-menu; padding-left:3px;' id='mnu_img_{0}' alt='' onclick='showMenu(event, ""{0}"", {2}, {3}, {5});' oncontextmenu='showMenu(event, ""{0}"", {2}, {3}, {5}); return false;'/></td>" + _
                                      "</tr></table>", tUniqueID, "../../images/ra/", IIf(HasCopyButton, "true", "false"), IIf(HasPasteButton, "true", "false"), Cell.Text, IIf(IsCatAttributeColumn, "true", "false"))
        End If
    End Sub

    Private Function GetAltIndex(ID As String) As integer
        Dim tAlt As RAAlternative 
        If CurrentPageID = _PGID_RA_STRATEGIC_BUCKETS Then
            tAlt = RA.Scenarios.ActiveScenario.GetAvailableAlternativeById(ID)
        Else
            tAlt = RAAlternatives.Where(Function (alt) alt.ID = ID)(0)
        End If
        If tAlt Is Nothing Then Return -1
        Return RAAlternatives.IndexOf(tAlt)
    End Function

    Protected Sub GridAlternatives_RowDataBound(sender As Object, e As GridViewRowEventArgs) Handles GridAlternatives.RowDataBound
        Dim tIndexVisible As Boolean = RA.Scenarios.GlobalSettings.IsIndexColumnVisible 'A1143

        If e.Row.RowType = DataControlRowType.Header AndAlso e.Row.Cells.Count > 0 Then

            e.Row.Cells(COL_GUID).Visible = False
            e.Row.Cells(COL_ID).Width = 50

            If Not tIndexVisible Then e.Row.Cells(COL_ID).CssClass = "ra_hidden" 'A1143

            e.Row.Cells(COL_NAME).Width = New Unit(100, UnitType.Percentage)
            e.Row.Cells(COL_FUNDED).Width = 100
            'e.Row.Cells(COL_NAME).Text = String.Format("<div style='width: 250px;cursor:hand;'>{0}</div>", e.Row.Cells(COL_NAME).Text)
            e.Row.Cells(COL_TOTAL).Width = 100
            e.Row.Cells(COL_COST).Width = 100

            For i As Integer = 0 To COL_ATRIBUTES_START_INDEX - 1
                e.Row.Cells(i).Text = GetSortCaption(i, e.Row.Cells(i).Text)
                'If e.Row.Cells(i).Text.Contains("(%%") Then e.Row.Cells(i).Text = e.Row.Cells(i).Text.Substring(0, e.Row.Cells(i).Text.IndexOf("(%%"))
                e.Row.Cells(i).VerticalAlign = VerticalAlign.Middle
                ' incert a hidden button image into header to align it with other headers
                'Dim btnHidden As String = "<input type='button' class='btn_glyph22 btn_glyph22_copy'style='visibility:hidden; width:1px;' />"
                'e.Row.Cells(i).Text = String.Format("<b>{0}</b>{1}", e.Row.Cells(i).Text, btnHidden)
            Next

            CopyPasteHeaderButtons(e.Row.Cells(COL_COST), "cost", True, True, False)
            CopyPasteHeaderButtons(e.Row.Cells(COL_NAME), "name", True, False, False)
            CopyPasteHeaderButtons(e.Row.Cells(COL_TOTAL), "total", True, False, False)

            For i As Integer = COL_ATRIBUTES_START_INDEX To e.Row.Cells.Count - 1
                e.Row.Cells(i).Text = GetSortCaption(i, e.Row.Cells(i).Text)
                Dim attr As clsAttribute = AttributesList(i - COL_ATRIBUTES_START_INDEX)
                CopyPasteHeaderButtons(e.Row.Cells(i), CStr(i - COL_ATRIBUTES_START_INDEX), True, True, attr.ValueType = AttributeValueTypes.avtEnumeration OrElse attr.ValueType = AttributeValueTypes.avtEnumerationMulti)
                e.Row.Cells(i).Text = e.Row.Cells(i).Text.Replace("/" + AttributesList(i - COL_ATRIBUTES_START_INDEX).ID.ToString, "")
                e.Row.Cells(i).Width = 150
                e.Row.Cells(i).VerticalAlign = VerticalAlign.Middle

                'hide the headers of non-categorical attributes
                e.Row.Cells(i).Visible = False
                'If CurrentPageID = _PGID_STRUCTURE_ALTERNATIVES OrElse attr.ValueType = AttributeValueTypes.avtEnumeration OrElse attr.ValueType = AttributeValueTypes.avtEnumerationMulti Then
                If attr.ValueType = AttributeValueTypes.avtEnumeration OrElse attr.ValueType = AttributeValueTypes.avtEnumerationMulti Then
                    e.Row.Cells(i).Visible = True
                End If
            Next
        End If

        If e.Row.RowType = DataControlRowType.DataRow Then
            If e.Row.Cells.Count > 0 Then e.Row.Cells(COL_GUID).Visible = False

            With e.Row.Attributes
                .Remove("onmouseover")
                .Add("onmouseover", String.Format("RowHover(this,1,{0});", IIf(e.Row.RowState = DataControlRowState.Alternate, 1, 0)))
                .Remove("onmouseout")
                .Add("onmouseout", String.Format("RowHover(this,0,{0});", IIf(e.Row.RowState = DataControlRowState.Alternate, 1, 0)))
            End With
        End If

        If e.Row.DataItem IsNot Nothing Then
            Dim tAlt As DataRowView = CType(e.Row.DataItem, DataRowView)
            Dim raAlt As RAAlternative = RA.Scenarios.ActiveScenario.GetAvailableAlternativeById(CStr(tAlt(COL_GUID)))
            Dim sAltGuid As String = raAlt.ID
            Dim tAltGuid As Guid = New Guid(sAltGuid)
            Dim disabled As String = "" 'disabled='disabled'

            With e.Row.Attributes
                .Remove("guid")
                .Add("guid", raAlt.ID)
            End With

            Dim Cell As DataControlFieldCell = CType(e.Row.Cells(COL_NAME), DataControlFieldCell)
            Cell.Attributes.Add("style", "text-align: left; padding: 0px 1ex;")
            Cell.Attributes.Add("clip_data", JS_SafeString(CStr(IIf(String.IsNullOrEmpty(Cell.Text), CLIPBOARD_CHAR_UNDEFINED_VALUE, Cell.Text)))) 'A0961
            Cell.ToolTip = Cell.Text
            Cell.Text = ShortString(Cell.Text, 65)
            'Dim sImg As String = "" 'Dim sImg As String = "&nbsp;"
            'Cell.Text = String.Format("{0}{1}", sImg, ShortString(Cell.Text, 45, True))

            'e.Row.Cells(COL_ID).Text = raAlt.SortOrder.ToString

            e.Row.Cells(COL_ID).HorizontalAlign = HorizontalAlign.Right
            e.Row.Cells(COL_NAME).HorizontalAlign = HorizontalAlign.Left
            If Not tIndexVisible Then e.Row.Cells(COL_ID).CssClass = "ra_hidden" 'A1143
            e.Row.Cells(COL_FUNDED).HorizontalAlign = HorizontalAlign.Center
            e.Row.Cells(COL_TOTAL).HorizontalAlign = HorizontalAlign.Right
            e.Row.Cells(COL_COST).HorizontalAlign = HorizontalAlign.Right

            e.Row.Cells(COL_TOTAL).Attributes.Add("clip_data", JS_SafeString(e.Row.Cells(COL_TOTAL).Text))
            e.Row.Cells(COL_COST).Attributes.Add("clip_data", JS_SafeString(CStr(IIf(String.IsNullOrEmpty(e.Row.Cells(COL_COST).Text), CLIPBOARD_CHAR_UNDEFINED_VALUE, e.Row.Cells(COL_COST).Text.Replace("&#160;", " "))))) 'A0961

            If String.IsNullOrEmpty(e.Row.Cells(COL_FUNDED).Text) Then e.Row.Cells(COL_FUNDED).Text = "&nbsp;"

            For i As Integer = COL_ATRIBUTES_START_INDEX To e.Row.Cells.Count - 1
                Dim backgroundColor As String = CStr(IIf(RA.Solver.SolverState = raSolverState.raSolved AndAlso raAlt.DisplayFunded > 0, "transparent", "white"))
                Dim s As String = ""            'cell content
                Dim clip_data As String = ""    'clipboard data
                Dim attr As clsAttribute = AttributesList(i - COL_ATRIBUTES_START_INDEX)
                Dim attrValue As Object = PM.Attributes.GetAttributeValue(attr.ID, tAltGuid)
                'Dim txtBox As String = "<input type='text' class='input' style='width:100%;background-color:" + backgroundColor + ";' value='{0}' onchange='txtAttrValueChange({1},{2},this.value);' " + disabled + ">" ' current value, alternative index, attribute index
                Dim txtBox As String = "<input type='text' class='input attrinput' style='width:100%;background-color:" + backgroundColor + ";' value='{0}' onchange='txtAttrValueChange({1},{2},this.value);' autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false' " + disabled + ">" ' current value, alternative index, attribute index

                Select Case attr.ValueType
                    Case AttributeValueTypes.avtString '0
                        If attrValue Is Nothing OrElse Not TypeOf attrValue Is String Then attrValue = ""
                        s = String.Format(txtBox, JS_SafeString(CStr(attrValue)), GetAltIndex(CStr(tAlt(COL_GUID))), (i - COL_ATRIBUTES_START_INDEX).ToString)
                        clip_data = CStr(attrValue)
                    Case AttributeValueTypes.avtBoolean '1
                        's = String.Format(txtBox, JS_SafeString(CStr(attrValue)), GetAltIndex(CStr(tAlt(COL_GUID))), (i - COL_ATRIBUTES_START_INDEX).ToString)
                        If attrValue Is Nothing OrElse Not TypeOf attrValue Is Boolean Then attrValue = False
                        s = "<select class='select' style='width:120px; margin-left:10px;background-color:" + backgroundColor + ";' onchange='boolAttrValueChange(" + GetAltIndex(CStr(tAlt(COL_GUID))).ToString + "," + (i - COL_ATRIBUTES_START_INDEX).ToString + ",this.value);' " + disabled + ">" + vbCr + "<option value='1' " + CStr(IIf(CBool(attrValue), "selected", "")) + ">" + ResString("lblYes") + "</option>" + vbCr + "<option value='0' " + CStr(IIf(Not CBool(attrValue), "selected", "")) + ">" + ResString("lblNo") + "</option></select>"
                        clip_data = CStr(IIf(CBool(attrValue), ResString("lblYes"), ResString("lblNo")))
                    Case AttributeValueTypes.avtLong '2
                        If attrValue Is Nothing OrElse Not (TypeOf attrValue Is Long OrElse TypeOf attrValue Is Integer) Then attrValue = ""
                        s = String.Format(txtBox, JS_SafeString(CStr(attrValue)), GetAltIndex(CStr(tAlt(COL_GUID))), (i - COL_ATRIBUTES_START_INDEX).ToString)
                        clip_data = CStr(attrValue).Replace("&#160;", " ")
                    Case AttributeValueTypes.avtDouble '3
                        If attrValue Is Nothing OrElse Not (TypeOf attrValue Is Double OrElse TypeOf attrValue Is Integer OrElse TypeOf attrValue Is Long OrElse TypeOf attrValue Is Single) Then attrValue = ""
                        s = String.Format(txtBox, JS_SafeString(CStr(attrValue)), GetAltIndex(CStr(tAlt(COL_GUID))), (i - COL_ATRIBUTES_START_INDEX).ToString)
                        clip_data = CStr(attrValue).Replace("&#160;", " ")
                    Case AttributeValueTypes.avtEnumeration '4
                        Dim attrValueEnumID As Guid = Guid.Empty

                        If attrValue IsNot Nothing AndAlso TypeOf attrValue Is Guid Then
                            attrValueEnumID = CType(attrValue, Guid)
                        End If

                        If attrValue IsNot Nothing AndAlso TypeOf attrValue Is String AndAlso CStr(attrValue) <> "" AndAlso CStr(attrValue).Trim.Length = 32 + 4 Then
                            attrValueEnumID = New Guid(CStr(attrValue))
                        End If

                        Dim items As clsAttributeEnumeration = PM.Attributes.GetEnumByID(AttributesList(i - COL_ATRIBUTES_START_INDEX).EnumID)
                        s = "<select class='select' style='width:120px; margin-right:10px; margin-left:10px;background-color:" + backgroundColor + ";' onchange='cbCategoryChange(" + GetAltIndex(CStr(tAlt(COL_GUID))).ToString + "," + (i - COL_ATRIBUTES_START_INDEX).ToString + ",this.value);'>" + vbCr + "<option value='-1' selected>- undefined -</option>" 'A0939
                        If items IsNot Nothing Then
                            Dim j As Integer = 0
                            For Each item In items.Items
                                Dim isSelected As String = ""
                                If Not attrValueEnumID.Equals(Guid.Empty) AndAlso attrValueEnumID.Equals(item.ID) Then
                                    clip_data = item.Value
                                    isSelected = "selected"
                                End If
                                s += vbCr + "<option value='" + j.ToString + "' " + isSelected + ">" + SafeFormString(item.Value) + "</option>"
                                j += 1
                            Next
                        End If
                        s += "</select>"
                    Case AttributeValueTypes.avtEnumerationMulti '5
                        Dim sValue As String = PM.Attributes.GetAttributeValueString(attr.ID, New Guid(CStr(tAlt(COL_GUID))))
                        If sValue = "" Then sValue = "- undefined -"
                        s = String.Format("<span title='{0}'>&nbsp;&nbsp;{1}</span>", JS_SafeString(sValue), JS_SafeString(ShortString(sValue, 50))) ' string representation of values
                        s += String.Format("<a href='' style='float:right;' onclick='onEditMultiCategoricalAttributeValue(" + GetAltIndex(CStr(tAlt(COL_GUID))).ToString + "," + (i - COL_ATRIBUTES_START_INDEX).ToString + "); return false;'><img src='../../images/ra/edit_small.gif' width='16' height='16' border='0'></a>")
                End Select

                e.Row.Cells(i).Text = s
                e.Row.Cells(i).Attributes.Add("clip_data", JS_SafeString(CStr(IIf(String.IsNullOrEmpty(clip_data), CLIPBOARD_CHAR_UNDEFINED_VALUE, clip_data)))) 'A0961

                'hide the data column of non-categorical attributes
                e.Row.Cells(i).Visible = False
                'If CurrentPageID = _PGID_STRUCTURE_ALTERNATIVES OrElse attr.ValueType = AttributeValueTypes.avtEnumeration OrElse attr.ValueType = AttributeValueTypes.avtEnumerationMulti Then
                If attr.ValueType = AttributeValueTypes.avtEnumeration OrElse attr.ValueType = AttributeValueTypes.avtEnumerationMulti Then
                    e.Row.Cells(i).Visible = True
                End If
            Next

            'highlight funded rows with yellow color
            If RA.Solver.SolverState = raSolverState.raSolved AndAlso raAlt.DisplayFunded > 0 Then 'A0939
                For Each tCell As TableCell In e.Row.Cells
                    tCell.BackColor = FUNDED_CELL_FILL_COLOR
                    tCell.Attributes.Add("class", "funded_cell")
                Next
            End If
        End If
    End Sub

    Protected Sub radAjaxManagerMain_AjaxRequest(sender As Object, e As AjaxRequestEventArgs) Handles RadAjaxManagerMain.AjaxRequest
        PasteAttributeData = ""

        Dim args As NameValueCollection = HttpUtility.ParseQueryString(e.Argument)
        Dim sAction As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "action")).ToLower ' Anti-XSS
        
        AttributesList = PM.Attributes.GetAlternativesAttributes(True)

        Select Case sAction.ToLower

            Case "normalize"
                NormalizeMode = CInt(GetParam(args, "value"))

            Case "synthesis"
                SynthesisMode = CInt(GetParam(args, "value"))
                ReCalculate(SynthesisMode = 0)

            Case "decimals"
                PM.Parameters.DecimalDigits = CInt(GetParam(args, "value"))

            Case "sort"
                Dim dir As Integer = CInt(GetParam(args, "dir"))
                RASortColumn = CInt(GetParam(args, "idx"))
                RASortDirection = CStr(IIf(dir = 0, "ASC", "DESC"))
                ' COLUMNS

            Case "rename_column"
                Dim AttrIndex As Integer = CInt(GetParam(args, "clmn"))
                Dim NewName As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "name")).Trim  ' Anti-XSS
                If AttrIndex >= 0 AndAlso AttrIndex < AttributesList.Count AndAlso NewName.Length > 0 Then
                    AttributesList(AttrIndex).Name = NewName
                    Dim attr As clsAttribute = AttributesList(AttrIndex) 'PM.Attributes.GetAttributeByID(AttributesList(AttrIndex).ID)
                    If attr IsNot Nothing Then
                        attr.Name = NewName
                        SaveAttributes(String.Format("Rename column '{0}'", ShortString(NewName, 40, True)))    ' D3790
                    End If
                End If

            Case "add_column"
                Dim NewName As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "name")).Trim  ' Anti-XSS
                Dim NewType As AttributeValueTypes = CType(GetParam(args, "type"), AttributeValueTypes)
                Dim attr As clsAttribute = PM.Attributes.AddAttribute(Guid.NewGuid(), NewName, AttributeTypes.atAlternative, NewType, Nothing, False, Guid.Empty)
                If attr IsNot Nothing Then
                    'try to assign the default value
                    If NewType = AttributeValueTypes.avtString OrElse NewType = AttributeValueTypes.avtLong OrElse NewType = AttributeValueTypes.avtDouble OrElse NewType = AttributeValueTypes.avtBoolean Then
                        Dim DefVal As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "def_val")).Trim    ' Anti-XSS
                        If Not String.IsNullOrEmpty(DefVal) Then
                            Select Case NewType
                                Case AttributeValueTypes.avtString
                                    attr.DefaultValue = DefVal
                                Case AttributeValueTypes.avtLong
                                    Dim tIntVal As Integer
                                    If Integer.TryParse(DefVal, tIntVal) Then attr.DefaultValue = tIntVal
                                Case AttributeValueTypes.avtDouble
                                    Dim tDblVal As Double
                                    If String2Double(DefVal, tDblVal) Then attr.DefaultValue = tDblVal
                                Case AttributeValueTypes.avtBoolean
                                    Dim tIntVal As Integer
                                    If Integer.TryParse(DefVal, tIntVal) Then attr.DefaultValue = CBool(IIf(tIntVal = 1, True, False))
                            End Select
                        End If
                    End If

                    AttributesList.Add(attr)
                    SaveAttributes(String.Format("Add column '{0}'", ShortString(NewName, 40)))    ' D3790
                End If

            Case "del_column"
                Dim AttrIndex As Integer = CInt(GetParam(args, "clmn"))
                If AttrIndex >= 0 AndAlso AttrIndex < AttributesList.Count Then
                    Dim attr As clsAttribute = AttributesList(AttrIndex)
                    If attr IsNot Nothing Then
                        Dim sName As String = AttributesList(AttrIndex).Name    ' D3790
                        PM.Attributes.RemoveAttribute(attr.ID)
                        AttributesList.RemoveAt(AttrIndex)
                        SaveAttributes(String.Format("Delete column '{0}'", ShortString(sName, 40)))    ' D3790
                    End If
                End If

                'ITEMS
            Case "rename_item"
                Dim AttrIndex As Integer = CInt(GetParam(args, "clmn"))
                Dim ItemIndex As Integer = CInt(GetParam(args, "item"))
                Dim NewName As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "name")).Trim  ' Anti-XSS
                If AttrIndex >= 0 AndAlso AttrIndex < AttributesList.Count AndAlso NewName.Length > 0 Then
                    Dim attr As clsAttribute = AttributesList(AttrIndex)
                    If attr IsNot Nothing Then
                        Dim items As clsAttributeEnumeration = PM.Attributes.GetEnumByID(attr.EnumID)
                        If items IsNot Nothing AndAlso items.Items IsNot Nothing AndAlso items.Items.Count > 0 AndAlso ItemIndex >= 0 AndAlso ItemIndex < items.Items.Count Then
                            items.Items(ItemIndex).Value = NewName
                            SaveAttributes(String.Format("Rename item '{0}'", ShortString(NewName, 40)))    ' D3790
                        End If
                    End If
                End If

            Case "add_item"
                Dim AttrIndex As Integer = CInt(GetParam(args, "clmn"))
                Dim NewName As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "name")).Trim  ' Anti-XSS
                If AttrIndex >= 0 AndAlso AttrIndex < AttributesList.Count AndAlso NewName.Length > 0 Then
                    Dim attr As clsAttribute = AttributesList(AttrIndex)
                    Dim aEnum As clsAttributeEnumeration = PM.Attributes.GetEnumByID(attr.EnumID)
                    If aEnum Is Nothing OrElse attr.EnumID.Equals(Guid.Empty) Then
                        aEnum = New clsAttributeEnumeration()
                        aEnum.ID = Guid.NewGuid
                        aEnum.Name = attr.Name
                        attr.EnumID = aEnum.ID
                        PM.Attributes.Enumerations.Add(aEnum)
                    End If

                    Dim eItem As clsAttributeEnumerationItem = aEnum.AddItem(NewName)
                    SaveAttributes(String.Format("Add item '{0}'", ShortString(NewName, 40)))    ' D3790
                End If

            Case "del_item"
                Dim AttrIndex As Integer = CInt(GetParam(args, "clmn"))
                Dim ItemIndex As Integer = CInt(GetParam(args, "item"))
                If AttrIndex >= 0 AndAlso AttrIndex < AttributesList.Count Then
                    Dim attr As clsAttribute = AttributesList(AttrIndex)
                    Dim items As clsAttributeEnumeration = PM.Attributes.GetEnumByID(attr.EnumID)
                    If items IsNot Nothing AndAlso items.Items IsNot Nothing AndAlso items.Items.Count > 0 AndAlso ItemIndex >= 0 AndAlso ItemIndex < items.Items.Count Then
                        Dim sName As String = items.Items(ItemIndex).Value
                        items.Items.RemoveAt(ItemIndex)
                        SaveAttributes(String.Format("Delete item '{0}'", ShortString(sName, 40)))    ' D3790
                    End If
                End If

            Case "set_attr_value"
                Dim AttrIndex As Integer = CInt(GetParam(args, "attr_idx"))
                Dim AltIndex As Integer = CInt(GetParam(args, "alt_idx"))
                Dim fValueChanged As Boolean = False

                If AltIndex >= 0 AndAlso AltIndex < AlternativesCount Then
                    Dim alt As RAAlternative = RAAlternatives(AltIndex)
                    If AttrIndex >= 0 AndAlso AttrIndex < AttributesList.Count Then
                        Dim attr As clsAttribute = AttributesList(AttrIndex)
                        Dim sValue As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "value")).Trim  ' Anti-XSS

                        Select Case attr.ValueType
                            Case AttributeValueTypes.avtString
                                If PM.Attributes.SetAttributeValue(attr.ID, UNDEFINED_USER_ID, attr.ValueType, sValue, New Guid(alt.ID), Guid.Empty) Then
                                    fValueChanged = True
                                End If
                            Case AttributeValueTypes.avtBoolean
                                If sValue = "1" OrElse sValue = "0" Then
                                    If PM.Attributes.SetAttributeValue(attr.ID, UNDEFINED_USER_ID, attr.ValueType, CBool(IIf(sValue = "1", True, False)), New Guid(alt.ID), Guid.Empty) Then
                                        fValueChanged = True
                                    End If
                                End If
                            Case AttributeValueTypes.avtLong
                                Dim intValue As Long
                                If (String.IsNullOrEmpty(sValue) OrElse Long.TryParse(sValue, intValue)) AndAlso PM.Attributes.SetAttributeValue(attr.ID, UNDEFINED_USER_ID, attr.ValueType, IIf(String.IsNullOrEmpty(sValue), Nothing, intValue), New Guid(alt.ID), Guid.Empty) Then
                                    fValueChanged = True
                                End If
                            Case AttributeValueTypes.avtDouble
                                Dim dblValue As Double
                                If (String.IsNullOrEmpty(sValue) OrElse String2Double(sValue, dblValue)) AndAlso PM.Attributes.SetAttributeValue(attr.ID, UNDEFINED_USER_ID, attr.ValueType, IIf(String.IsNullOrEmpty(sValue), Nothing, dblValue), New Guid(alt.ID), Guid.Empty) Then
                                    fValueChanged = True
                                End If
                            Case AttributeValueTypes.avtEnumeration
                                Dim items As clsAttributeEnumeration = PM.Attributes.GetEnumByID(attr.EnumID)
                                Dim ItemIndex As Integer = CInt(GetParam(args, "enum_idx"))
                                If items IsNot Nothing AndAlso items.Items IsNot Nothing AndAlso items.Items.Count > 0 AndAlso ItemIndex < items.Items.Count Then


                                    Dim tValue As Object = Nothing
                                    If ItemIndex >= 0 Then tValue = items.Items(ItemIndex).ID

                                    If PM.Attributes.SetAttributeValue(attr.ID, UNDEFINED_USER_ID, AttributeValueTypes.avtEnumeration, tValue, New Guid(alt.ID), Guid.Empty) Then
                                        fValueChanged = True
                                    End If
                                End If
                            Case AttributeValueTypes.avtEnumerationMulti
                                'not implemented
                        End Select

                        If fValueChanged Then
                            SaveAttributesValues(String.Format("Set attribute value '{0}'", ShortString(attr.Name, 40)))    ' D3790
                        End If
                    End If
                End If

            Case "paste_attribute_data"
                PasteAttributeData = ""
                Dim AttrIndex As Integer = CInt(GetParam(args, "attr_idx"))
                Dim data As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "data"))  ' Anti-XSS
                Dim cells As String() = data.Split(Chr(10))
                Dim cells_count As Integer = cells.Count
                Dim fItemsChanged As Boolean = False
                Dim fValueChanged As Boolean = False
                Dim lblYes As String = ResString("lblYes").ToLower
                Dim tGuids As List(Of Guid) = Param2GuidList(GetParam(args, "guids"))

                If AttrIndex >= 0 AndAlso AttrIndex < AttributesList.Count Then
                    Dim attr As clsAttribute = AttributesList(AttrIndex)
                    If attr IsNot Nothing Then
                        Dim aEnum As clsAttributeEnumeration = Nothing
                        If attr.ValueType = AttributeValueTypes.avtEnumeration OrElse attr.ValueType = AttributeValueTypes.avtEnumerationMulti Then aEnum = PM.Attributes.GetEnumByID(attr.EnumID)
                        Dim alts_count As Integer = AlternativesCount
                        If alts_count < cells.Count Then cells_count = alts_count
                        For i As Integer = 0 To cells_count - 1
                            If tGuids.Count <= i Then Exit For
                            Dim value As String = cells(i).Trim
                            Dim alt As RAAlternative = RA.Scenarios.ActiveScenario.GetAvailableAlternativeById(tGuids(i).ToString)
                            Select Case attr.ValueType
                                Case AttributeValueTypes.avtString
                                    If PM.Attributes.SetAttributeValue(attr.ID, UNDEFINED_USER_ID, AttributeValueTypes.avtString, value, New Guid(alt.ID), Guid.Empty) Then fValueChanged = True
                                Case AttributeValueTypes.avtBoolean
                                    If PM.Attributes.SetAttributeValue(attr.ID, UNDEFINED_USER_ID, AttributeValueTypes.avtBoolean, CBool(IIf(value.ToLower = lblYes OrElse value.ToLower = "1" OrElse value.ToLower = "true", True, False)), New Guid(alt.ID), Guid.Empty) Then fValueChanged = True
                                Case AttributeValueTypes.avtLong
                                    Dim intValue As Integer
                                    If String.IsNullOrEmpty(value) Then value = Nothing
                                    If String.IsNullOrEmpty(value) OrElse Integer.TryParse(value, intValue) AndAlso PM.Attributes.SetAttributeValue(attr.ID, UNDEFINED_USER_ID, AttributeValueTypes.avtLong, intValue, New Guid(alt.ID), Guid.Empty) Then fValueChanged = True
                                Case AttributeValueTypes.avtDouble
                                    Dim dblValue As Double
                                    If String.IsNullOrEmpty(value) Then value = Nothing
                                    If String.IsNullOrEmpty(value) OrElse String2Double(value, dblValue) AndAlso PM.Attributes.SetAttributeValue(attr.ID, UNDEFINED_USER_ID, AttributeValueTypes.avtDouble, dblValue, New Guid(alt.ID), Guid.Empty) Then fValueChanged = True
                                Case AttributeValueTypes.avtEnumeration
                                    If Not String.IsNullOrEmpty(value) Then
                                        ' check if enum item exists and create new if not
                                        Dim enumItem As clsAttributeEnumerationItem = Nothing

                                        If aEnum IsNot Nothing AndAlso Not attr.EnumID.Equals(Guid.Empty) Then
                                            enumItem = aEnum.GetItemByValue(value)
                                        Else
                                            aEnum = New clsAttributeEnumeration()
                                            aEnum.ID = Guid.NewGuid
                                            aEnum.Name = attr.Name
                                            attr.EnumID = aEnum.ID
                                            PM.Attributes.Enumerations.Add(aEnum)
                                        End If

                                        If enumItem Is Nothing Then
                                            enumItem = aEnum.AddItem(value)
                                            fItemsChanged = True
                                        End If
                                        ' assign attribute value
                                        If PM.Attributes.SetAttributeValue(attr.ID, UNDEFINED_USER_ID, AttributeValueTypes.avtEnumeration, enumItem.ID, New Guid(alt.ID), Guid.Empty) Then fValueChanged = True
                                    Else
                                        If PM.Attributes.SetAttributeValue(attr.ID, UNDEFINED_USER_ID, AttributeValueTypes.avtEnumeration, Nothing, New Guid(alt.ID), Guid.Empty) Then fValueChanged = True
                                    End If
                            End Select
                        Next
                        PasteAttributeData = GetAttributeItemsData(attr, aEnum)
                    End If
                    If fItemsChanged Then SaveAttributes(String.Format("Paste attrib data '{0}'", ShortString(attr.Name, 40))) ' D3790
                    If fValueChanged Then SaveAttributesValues(CStr(IIf(fItemsChanged, "", String.Format("Paste attrib data '{0}'", ShortString(attr.Name, 40))))) ' D3790
                    If fItemsChanged OrElse fValueChanged AndAlso RA.Scenarios.GlobalSettings.isAutoSolve Then RA.Solve() ' D3900
                End If

            Case "paste_default_column"
                Dim column_id As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "column"))   ' Anti-XSS
                Dim data As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "data"))  ' Anti-XSS
                Dim cells As String() = data.Split(Chr(10))
                Dim cells_count As Integer = cells.Count
                Dim fValueChanged As Boolean = False
                Dim alts_count As Integer = AlternativesCount
                Dim tGuids As List(Of Guid) = Param2GuidList(GetParam(args, "guids"))
                If alts_count < cells.Count Then cells_count = alts_count

                For i As Integer = 0 To cells_count - 1
                    If tGuids.Count <= i Then Exit For
                    Dim value As String = cells(i).Trim
                    If String.IsNullOrEmpty(value) Then value = Nothing
                    Dim alt As RAAlternative = RA.Scenarios.ActiveScenario.GetAvailableAlternativeById(tGuids(i).ToString)
                        ' assign attribute value
                    Select Case column_id
                        'Case "name"
                        '    If alt.Name <> value Then
                        '        alt.Name = value
                        '        fValueChanged = True
                        '    End If
                        Case "cost"
                            Dim tDblValue As Double = 0
                            If value Is Nothing OrElse Not String2Double(value, tDblValue) Then tDblValue = UNDEFINED_INTEGER_VALUE
                            If alt.Cost <> tDblValue Then
                                alt.Cost = tDblValue
                                fValueChanged = True
                            End If
                    End Select
                Next
                If fValueChanged Then
                    App.ActiveProject.SaveRA("Edit strategic buckets", , , "Paste default column") ' D3790
                    If RA.Scenarios.GlobalSettings.isAutoSolve Then RA.Solve() ' D3900
                End If
                'Case "copy_data_to_clipboard"
                '    PasteAttributeData = ""
                '    Dim column_id As String = GetParam(args, "column")
                '    Dim AttrIndex As Integer = -1
                '    Integer.TryParse(column_id, AttrIndex)
                '    Dim attr As clsAttribute = Nothing
                '    Dim aEnum As clsAttributeEnumeration = Nothing
                '    If AttrIndex >= 0 AndAlso AttrIndex < AttributesList.Count Then
                '        attr = AttributesList(AttrIndex)
                '        If attr IsNot Nothing Then
                '            aEnum = PM.Attributes.GetEnumByID(attr.EnumID)
                '        End If
                '    End If
                '    For Each alt As RAAlternative In RAAlternatives
                '        Dim sValue As String = " " 'should contain at least one space char by deafult 
                '        Select Case column_id
                '            Case "name"
                '                sValue = alt.Name
                '            Case "cost"
                '                sValue = alt.Cost.ToString(COST_FORMAT)
                '            Case "total"
                '                sValue = alt.SBTotal.ToString("F" + DecimalDigits.ToString) '"Total" value with current normalization
                '            Case Else
                '                If AttrIndex >= 0 AndAlso attr IsNot Nothing AndAlso aEnum IsNot Nothing Then
                '                    Dim attrValue As Object = PM.Attributes.GetAttributeValue(attr.ID, New Guid(alt.ID))
                '                    Dim attrValueEnumID As Guid = Guid.Empty
                '                    If attrValue IsNot Nothing AndAlso TypeOf attrValue Is Guid Then
                '                        attrValueEnumID = CType(attrValue, Guid)
                '                    End If
                '                    If Not attrValueEnumID.Equals(Guid.Empty) Then
                '                        Dim item As clsAttributeEnumerationItem = aEnum.GetItemByID(attrValueEnumID)
                '                        If item IsNot Nothing Then
                '                            sValue = item.Value
                '                        End If
                '                    End If
                '                End If
                '        End Select
                '        PasteAttributeData += CStr(IIf(PasteAttributeData = "", "", "\n")) + JS_SafeString(sValue)
                '    Next
                '    PasteAttributeData = String.Format("'{0}'", PasteAttributeData)

            Case "attributes_reorder"
                Dim fChanged As Boolean = False
                Dim lst As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "lst"))    ' Anti-XSS
                If Not String.IsNullOrEmpty(lst) Then
                    Dim attrList As List(Of clsAttribute) = PM.Attributes.GetAlternativesAttributes(True)
                    Dim globalI As New List(Of Integer)
                    For Each attr In attrList
                        globalI.Add(PM.Attributes.AttributesList.IndexOf(attr))
                    Next
                    Dim indices As String() = lst.Split(CChar(","))
                    Dim i As Integer = 0
                    For Each id As String In indices
                        Dim idx As Integer = CInt(id)
                        If idx >= 0 AndAlso idx < attrList.Count AndAlso idx <> i Then
                            Dim attrN As clsAttribute = attrList(idx)
                            PM.Attributes.AttributesList.RemoveAt(globalI(i))
                            PM.Attributes.AttributesList.Insert(globalI(i), attrN)
                            fChanged = True
                        End If
                        i += 1
                    Next
                End If
                If fChanged Then
                    SaveAttributes("Reorder attributes")    ' D3790
                    InitAttributesList()
                End If

            Case "enum_items_reorder"
                Dim fChanged As Boolean = False
                Dim lst As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "lst"))    ' Anti-XSS
                Dim attrIdx As Integer = CInt(GetParam(args, "clmn"))
                Dim attrList As List(Of clsAttribute) = PM.Attributes.GetAlternativesAttributes(True)
                If Not String.IsNullOrEmpty(lst) AndAlso attrIdx >= 0 AndAlso attrIdx < attrList.Count Then
                    Dim attr As clsAttribute = attrList(attrIdx)
                    If attr.ValueType = AttributeValueTypes.avtEnumeration OrElse attr.ValueType = AttributeValueTypes.avtEnumerationMulti Then
                        Dim aEnum As clsAttributeEnumeration = PM.Attributes.GetEnumByID(attr.EnumID)
                        If aEnum IsNot Nothing Then
                            Dim indices As String() = lst.Split(CChar(","))
                            Dim i As Integer = 0
                            Dim enumList As New List(Of clsAttributeEnumerationItem)
                            For Each item As clsAttributeEnumerationItem In aEnum.Items
                                enumList.Add(item)
                            Next
                            For Each id As String In indices
                                Dim idx As Integer = CInt(id)
                                If idx >= 0 AndAlso idx < aEnum.Items.Count AndAlso idx <> i Then
                                    Dim itemN As clsAttributeEnumerationItem = enumList(idx)
                                    aEnum.Items.RemoveAt(i)
                                    aEnum.Items.Insert(i, itemN)
                                    fChanged = True
                                End If
                                i += 1
                            Next
                        End If
                    End If
                End If
                If fChanged Then SaveAttributes("Items reorder") ' D3790

            Case "set_default_value"
                Dim AttrIndex As Integer = CInt(GetParam(args, "clmn"))
                Dim value As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "def_val")).Trim ' Anti-XSS
                Dim fValueChanged As Boolean = False
                If AttrIndex >= 0 AndAlso AttrIndex < AttributesList.Count Then
                    Dim attr As clsAttribute = AttributesList(AttrIndex)
                    If attr IsNot Nothing Then
                        Select Case attr.ValueType
                            Case AttributeValueTypes.avtString
                                If String.IsNullOrEmpty(value) Then
                                    attr.DefaultValue = Nothing
                                Else
                                    attr.DefaultValue = value
                                End If
                                fValueChanged = True
                            Case AttributeValueTypes.avtBoolean
                                If value = "1" OrElse value = "0" Then
                                    attr.DefaultValue = value = "1"
                                    fValueChanged = True
                                End If
                            Case AttributeValueTypes.avtLong
                                If String.IsNullOrEmpty(value) Then
                                    attr.DefaultValue = Nothing
                                    fValueChanged = True
                                Else
                                    Dim intValue As Integer
                                    If Integer.TryParse(value, intValue) Then
                                        attr.DefaultValue = intValue
                                        fValueChanged = True
                                    End If
                                End If
                            Case AttributeValueTypes.avtDouble
                                If String.IsNullOrEmpty(value) Then
                                    attr.DefaultValue = Nothing
                                    fValueChanged = True
                                Else
                                    Dim dblValue As Double
                                    If String2Double(value, dblValue) Then
                                        attr.DefaultValue = dblValue
                                        fValueChanged = True
                                    End If
                                End If
                            Case AttributeValueTypes.avtEnumeration
                                Dim ItemIndex As Integer = CInt(GetParam(args, "item_index"))
                                If value = "1" Then
                                    Dim aEnum As clsAttributeEnumeration = PM.Attributes.GetEnumByID(attr.EnumID)
                                    If aEnum IsNot Nothing Then
                                        If ItemIndex >= 0 AndAlso ItemIndex < aEnum.Items.Count Then
                                            attr.DefaultValue = aEnum.Items(ItemIndex).ID
                                            fValueChanged = True
                                        End If
                                    End If
                                Else
                                    attr.DefaultValue = Nothing
                                    fValueChanged = True
                                End If
                            Case AttributeValueTypes.avtEnumerationMulti
                                'not implemented
                        End Select
                        If fValueChanged Then SaveAttributes(String.Format("Set default for '{0}'", ShortString(attr.Name, 40))) ' D3790
                    End If
                End If
            Case "set_multi_cat_values"
                Dim sLst As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "lst")).ToLower
                Dim fValueChanged As Boolean = False
                Dim AltIndex As Integer = CInt(GetParam(args, "alt_idx"))
                Dim AttrIndex As Integer = CInt(GetParam(args, "clmn"))
                If AttrIndex >= 0 AndAlso AttrIndex < AttributesList.Count AndAlso AltIndex >= 0 AndAlso AltIndex < AlternativesCount Then
                    Dim alt As RAAlternative = RAAlternatives(AltIndex)
                    Dim attr As clsAttribute = AttributesList(AttrIndex)
                    If attr IsNot Nothing AndAlso attr.ValueType = AttributeValueTypes.avtEnumerationMulti Then
                        If PM.Attributes.SetAttributeValue(attr.ID, UNDEFINED_USER_ID, AttributeValueTypes.avtEnumerationMulti, sLst, New Guid(alt.ID), Guid.Empty) Then
                            fValueChanged = True
                        End If
                    End If
                    If fValueChanged Then SaveAttributesValues(String.Format("Set multi-categorical attribute '{0}' values for '{1}'", ShortString(attr.Name, 40), ShortString(alt.Name, 40)))
                End If
                'tResult = String.Format("['{0}']", tAction)
        End Select

        GridAlternatives.DataSource = Nothing
        InitDataGrid()
    End Sub

    Private Sub Ajax_Callback(data As String)
        Dim args As NameValueCollection = HttpUtility.ParseQueryString(data)

        Dim tAction As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "action")).ToLower ' Anti-XSS
        Dim tResult As String = ""

        AttributesList = PM.Attributes.GetAlternativesAttributes(True)

        If Not String.IsNullOrEmpty(tAction) Then
            Select Case tAction

                Case "selected_alt_changed"
                    Dim AltID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "alt_guid")).Trim    ' Anti-XSS
                    SelectedAltID = New Guid(AltID)
                    tResult = String.Format("['{0}']", tAction)

                Case "set_attr_value"
                    Dim AttrIndex As Integer = CInt(GetParam(args, "attr_idx"))
                    Dim AltIndex As Integer = CInt(GetParam(args, "alt_idx"))
                    Dim fValueChanged As Boolean = False

                    If AltIndex >= 0 AndAlso AltIndex < AlternativesCount Then
                        Dim alt As RAAlternative = RAAlternatives(AltIndex)
                        If AttrIndex >= 0 AndAlso AttrIndex < AttributesList.Count Then
                            Dim attr As clsAttribute = AttributesList(AttrIndex)
                            Dim sValue As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "value")).Trim  ' Anti-XSS

                            Select Case attr.ValueType
                                Case AttributeValueTypes.avtEnumeration
                                    Dim items As clsAttributeEnumeration = PM.Attributes.GetEnumByID(attr.EnumID)
                                    Dim ItemIndex As Integer = CInt(GetParam(args, "enum_idx"))
                                    If items IsNot Nothing AndAlso items.Items IsNot Nothing AndAlso items.Items.Count > 0 AndAlso ItemIndex < items.Items.Count Then


                                        Dim tValue As Object = Nothing
                                        If ItemIndex >= 0 Then tValue = items.Items(ItemIndex).ID

                                        If PM.Attributes.SetAttributeValue(attr.ID, UNDEFINED_USER_ID, AttributeValueTypes.avtEnumeration, tValue, New Guid(alt.ID), Guid.Empty) Then
                                            fValueChanged = True
                                        End If
                                    End If
                                Case AttributeValueTypes.avtEnumerationMulti
                                    'not implemented
                            End Select

                            If fValueChanged Then
                                SaveAttributesValues(String.Format("Set attribute value '{0}'", ShortString(attr.Name, 40)))    ' D3790
                            End If
                        End If
                    End If
                    tResult = String.Format("['{0}']", tAction)
                Case "get_multi_cat_values"
                    Dim AttrIndex As Integer = CInt(GetParam(args, "attr_idx"))
                    Dim AltIndex As Integer = CInt(GetParam(args, "alt_idx"))
                    Dim retVal As String = ""

                    If AltIndex >= 0 AndAlso AltIndex < AlternativesCount Then
                        Dim alt As RAAlternative = RAAlternatives(AltIndex)
                        If AttrIndex >= 0 AndAlso AttrIndex < AttributesList.Count Then
                            Dim attr As clsAttribute = AttributesList(AttrIndex)

                            Select Case attr.ValueType
                                Case AttributeValueTypes.avtEnumerationMulti
                                    Dim items As clsAttributeEnumeration = PM.Attributes.GetEnumByID(attr.EnumID)
                                    Dim tValue As Object = PM.Attributes.GetAttributeValue(attr.ID, New Guid(alt.ID))
                                    Dim tGuids As New List(Of Guid)
                                    If TypeOf(tValue) Is Guid then tGuids.Add(CType(tValue, Guid))
                                    If TypeOf(tValue) Is String then
                                        Dim sGuids As String() = CStr(tValue).Split(CChar(";"))
                                        For Each sGuid As String In sGuids
                                            Dim tGuid As Guid
                                            If Guid.TryParse(sGuid, tGuid) Then                                            
                                                tGuids.Add(tGuid)
                                            End If
                                        Next
                                    End If
                                    If items IsNot Nothing AndAlso items.Items IsNot Nothing AndAlso items.Items.Count > 0 Then
                                        For each row In items.Items 
                                            retVal += CStr(IIf(retVal = "", "", ",")) + String.Format("[{0}, '{1}', '{2}']", Bool2JS(tGuids.Contains(row.ID)), row.ID.ToString, JS_SafeString(row.Value))
                                        Next                                        
                                    End If
                            End Select

                        End If
                    End If

                    tResult = String.Format("['{0}',[{1}], {2}]", tAction, retVal, AltIndex)
                
            End Select            
        End If

        If tResult <> "" Then
            Response.Clear()
            Response.ContentType = "text/plain"
            Response.Write(tResult)
            Response.End()
        End If
    End Sub

    Private PasteAttributeData As String = ""

    Private Sub SaveAttributes(sComment As String)  ' D3790
        PM.Attributes.WriteAttributes(AttributesStorageType.astStreamsDatabase, PM.StorageManager.ProjectLocation, PM.StorageManager.ProviderType, PM.StorageManager.ModelID)
        RA.Scenarios.SyncLinkedConstraintsValues(Nothing) ' D3379
        If sComment <> "" Then App.ActiveProject.SaveRA("Edit strategic buckets", , , sComment) ' D3790
    End Sub

    Private Sub SaveAttributesValues(sComment As String)    ' D3790
        PM.Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, PM.StorageManager.ProjectLocation, PM.StorageManager.ProviderType, PM.StorageManager.ModelID, UNDEFINED_USER_ID)
        RA.Scenarios.SyncLinkedConstraintsValues(Nothing) ' D3379
        If sComment <> "" Then App.ActiveProject.SaveRA("Edit strategic buckets", , , sComment) ' D3790
    End Sub

    Private Sub ReCalculate(IsIdealMode As Boolean)
        'With PM
        '    Dim CalcMode As ECSynthesisMode = ECSynthesisMode.smDistributive
        '    If IsIdealMode Then CalcMode = ECSynthesisMode.smIdeal
        '    .CalculationsManager.SynthesisMode = CalcMode
        '    Dim CG As clsCombinedGroup = .CombinedGroups.GetDefaultCombinedGroup
        '    .CalculationsManager.Calculate(New clsCalculationTarget(CalculationTargetType.cttCombinedGroup, CG), .Hierarchy(.ActiveHierarchy).Nodes(0), .ActiveHierarchy, .ActiveAltsHierarchy)
        '    For Each alt As clsNode In .AltsHierarchy(.ActiveAltsHierarchy).TerminalNodes
        '        For Each raAlt In RAAlternatives
        '            If alt.NodeGuidID.ToString = raAlt.ID Then raAlt.SBPriority = alt.UnnormalizedPriority
        '        Next
        '    Next
        'End With
    End Sub

    Protected Sub divSrvData_PreRender(sender As Object, e As EventArgs) Handles divSrvData.PreRender
        divSrvData.InnerText = String.Format("[{0}]", PasteAttributeData)
    End Sub

    Protected Sub divSelectedAltID_PreRender(sender As Object, e As EventArgs) Handles divSelectedAltID.PreRender        
        divSelectedAltID.InnerText = String.Format("{0}", SelectedAltID.ToString)
    End Sub

End Class