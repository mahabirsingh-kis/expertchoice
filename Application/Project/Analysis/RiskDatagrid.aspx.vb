Option Strict On
Imports System.Collections.ObjectModel
Imports System.Data
Imports System.Drawing
Imports Chilkat

Partial Class RiskDatagridPage
    Inherits clsComparionCorePage

    Public Const _PRIORITY_DECIMALS As Integer = 4
    Public Const _PRIORITY_FORMAT As String = "f4"
    Public Const _COST_FORMAT As String = "f2"

    Public Use_Controls_Effectiveness As Boolean = False

    Public Sub New()
        MyBase.New(_PGID_REPORT_DATAGRID_RISK)
    End Sub

    ReadOnly Property PRJ As clsProject
        Get
            Return App.ActiveProject
        End Get
    End Property

    ReadOnly Property PM As clsProjectManager
        Get
            Return App.ActiveProject.ProjectManager
        End Get
    End Property

    Public ReadOnly Property ShowDollarValue As Boolean
        Get
            Dim retVal As Boolean = False
            If DollarValue <> UNDEFINED_INTEGER_VALUE Then
                retVal = CBool(App.ActiveProject.ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_SHOW_DOLLAR_VALUE_ID, UNDEFINED_USER_ID))
            End If
            Return retVal
        End Get
    End Property

    Public ReadOnly Property DollarValue As Double
        Get
            Return CDbl(App.ActiveProject.ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_DOLLAR_VALUE_ID, UNDEFINED_USER_ID))
        End Get
    End Property

    Public Property SelectedUserID As Integer
        Get
            Dim tUserID As Integer = CInt(PM.Attributes.GetAttributeValue(ATTRIBUTE_SYNTHESIS_SA_USER_ID, UNDEFINED_USER_ID))
            If PM.UserExists(tUserID) OrElse PM.CombinedGroups.CombinedGroupExists(tUserID) Then Return tUserID
            Return COMBINED_USER_ID
        End Get
        Set(value As Integer)
            WriteSetting(PRJ, ATTRIBUTE_SYNTHESIS_SA_USER_ID, AttributeValueTypes.avtLong, value)
        End Set
    End Property

    Public Function getGroupsUsersList() As String
        Dim resVal As String = ""
        Dim groupsString = ""
        For Each Group As clsCombinedGroup In App.ActiveProject.ProjectManager.CombinedGroups.GroupsList
            resVal += CStr(IIf(resVal = "", "", ",")) + String.Format("{{""id"":{0},""text"":""[{1}]""}}", Group.CombinedUserID, JS_SafeString(Group.Name))
        Next
        For Each User As clsUser In App.ActiveProject.ProjectManager.UsersList
            resVal += CStr(IIf(resVal = "", "", ",")) + String.Format("{{""id"":{0},""text"":""{1}""}}", User.UserID, JS_SafeString(User.UserName))
        Next
        Return String.Format("[{0}]", resVal)
    End Function

    Public Function GetDatagridColumns() As String
        Dim retVal As String = ""
        'retVal += String.Format("  [{0},'{1}',{2},'{3}']", 0, "GUID", "false", "td_right")
        'retVal += String.Format(",[{0},'{1}',{2},'{3}']", 1, "ID", "true", "td_right")
        'retVal += String.Format(",[{0},'{1}',{2},'{3}']", 2, ParseString("Is Active"), "true", "td_center")
        'retVal += String.Format(",[{0},'{1}',{2},'{3}']", 3, ParseString("%%Control%% Name"), "true", "td_left")
        'retVal += String.Format(",[{0},'{1}',{2},'{3}']", 4, ParseString("%%Control%% for"), "true", "td_left")
        'retVal += String.Format(",[{0},'{1}',{2},'{3}']", 5, ParseString("Funded"), "true", "td_center")
        'retVal += String.Format(",[{0},'{1}',{2},'{3}']", 6, ParseString("Cost"), "true", "td_right")
        'retVal += String.Format(",[{0},'{1}',{2},'{3}']", 7, ParseString("Applications"), "true", "td_center")
        'retVal += String.Format(",[{0},'{1}',{2},'{3}']", 7, ResString("tblMust"), "true", "td_center")
        'retVal += String.Format(",[{0},'{1}',{2},'{3}']", 7, ResString("tblMustNot"), "true", "td_center")
        retVal += String.Format(" [{0},'{1}',{2},'{3}','{4}','{5}']", 0, ResString("dgNum"), "true", "right", "number", "idx")
        retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}','{5}']", 1, ResString("dgAlt"), "true", "left", "string", "name")
        retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}','{5}']", 3, ResString("dgTotalLikelihood"), "true", "right", "number", "lkh")
        retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}','{5}']", 2, ResString("dgTotalImpact") + CStr(IIf(ShowDollarValue, ", $", "")), "true", "right", "number", "imp")
        retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}','{5}']", 4, ResString("dgRisk") + CStr(IIf(ShowDollarValue, ", $", "")), "true", "right", "number", "risk")

        Dim index As Integer = 5

        Dim nodeIdx As Integer = 0
        For Each Hierarchy As clsHierarchy In App.ActiveProject.ProjectManager.Hierarchies
            Dim APrefix As String = ""
            If Hierarchy.HierarchyID = ECHierarchyID.hidImpact Then
                APrefix = "Impact: "
            End If
            If Hierarchy.HierarchyID = ECHierarchyID.hidLikelihood Then
                APrefix = "Likelihood: "
            End If
            For Each node As clsNode In Hierarchy.TerminalNodes
                retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}','n{5}']", index, APrefix + JS_SafeString(SafeFormString(node.NodeName)), "true", "right", "number", nodeIdx)
                index += 1
                nodeIdx += 1
            Next
        Next

        Dim attrIdx As Integer = 0
        For Each altAttr As clsAttribute In App.ActiveProject.ProjectManager.Attributes.GetAlternativesAttributes
            If PM.Attributes.IsAltAttrVisibleInReport(altAttr.ID, PM) Then
                retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}','v{5}']", index, JS_SafeString(SafeFormString(altAttr.Name)), "true", "right", "string", attrIdx)
                index += 1
            End If
            attrIdx += 1
        Next

        Return String.Format("[{0}]", retVal)
    End Function

    Public Function GetDatagridData() As String
        Dim retVal As String = ""
        Dim tDataSet As DataSet = PM.ProjectDataProvider.ReportDataSet(CType(IIf(Use_Controls_Effectiveness, CanvasReportType.crtDataGridRiskWithControls, CanvasReportType.crtDataGridRisk), CanvasReportType), , SelectedUserID)
        Dim tTable As DataTable = tDataSet.Tables("DataGridRisk")
        Dim tTableColCount As Integer = tTable.Columns.Count 

        If tTable.Rows.Count > 0 Then

            For each tRow As DataRow In tTable.Rows
                Dim sRow As String = """idx"":" + CStr(tRow(0)) ' ID
                sRow += ",""name"":" + String.Format("'{0}'", JS_SafeString(CStr(tRow(1)))) ' Name
                sRow += ",""lkh"":" + JS_SafeNumber(CStr(tRow(2))) ' Total Likelihood
                sRow += ",""imp"":" + JS_SafeNumber(CStr(tRow(3))) ' Total Impact
                sRow += ",""risk"":" + JS_SafeNumber(CStr(tRow(4))) ' Total Risk

                Dim index As Integer = 5
                Dim nodeIdx As Integer = 0
                For Each Hierarchy As clsHierarchy In App.ActiveProject.ProjectManager.Hierarchies
                    For Each node As clsNode In Hierarchy.TerminalNodes
                        If tTableColCount > index AndAlso tTable.Columns(index).ColumnName = node.NodeGuidID.ToString Then
                            sRow += String.Format(",""n{0}"":", nodeIdx)
                            If Not IsDBNull(tRow(index)) AndAlso Not String.IsNullOrEmpty(CStr(tRow(index))) Then sRow += "'" + JS_SafeString(CStr(tRow(index))) + "'" Else sRow += "''"
                            index += 1
                            nodeIdx += 1
                        End If
                    Next
                Next

                Dim attrIdx As Integer = 0
                For Each altAttr As clsAttribute In App.ActiveProject.ProjectManager.Attributes.GetAlternativesAttributes
                    If PM.Attributes.IsAltAttrVisibleInReport(altAttr.ID, PM) AndAlso tTableColCount > index AndAlso tTable.Columns(index).ColumnName = altAttr.ID.ToString Then
                        sRow += String.Format(",""v{0}"":", attrIdx)
                        If Not IsDBNull(tRow(index)) AndAlso Not String.IsNullOrEmpty(CStr(tRow(index))) Then
                            Select Case altAttr.ValueType
                                Case AttributeValueTypes.avtBoolean, AttributeValueTypes.avtLong, AttributeValueTypes.avtString
                                    sRow += String.Format("'{0}'", JS_SafeString(CStr(tRow(index))))
                                Case AttributeValueTypes.avtDouble
                                    sRow += JS_SafeNumber(CDbl(tRow(index)))
                                Case AttributeValueTypes.avtEnumeration, AttributeValueTypes.avtEnumerationMulti
                                    sRow += String.Format("'{0}'", JS_SafeString(CStr(tRow(index)))) 'TODO: DA
                            End Select
                        Else
                            sRow += "''"
                        End If

                        index += 1
                    End If
                    attrIdx += 1
                Next

                retVal += CStr(IIf(retVal = "", "", ",")) + String.Format("{{{0}}}", sRow)                
            Next

        End If

        Return String.Format("[{0}]", retVal)
    End Function

    Public Function GetTitle() As String
        Return String.Format(ParseString("%%Risk%% Data Grid for model &quot;{0}&quot;"), SafeFormString(ShortString(App.ActiveProject.ProjectName, 70)))
    End Function

    Protected Sub Page_Init(sender As Object, e As EventArgs) Handles Me.Init
        If Not isCallback AndAlso Not IsPostBack Then
            PM.ProjectDataProvider.FullAlternativePath = False

            With App.ActiveProject.ProjectManager
                .Attributes.ReadAttributes(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID)
            End With
        End If

        If CheckVar("with_controls", "0") = "1" Then
            Use_Controls_Effectiveness = True
            CurrentPageID = _PGID_REPORT_DATAGRID_RISK_TREATMENTS
        End If
    End Sub
    
    Protected Sub Page_InitComplete(sender As Object, e As EventArgs) Handles Me.InitComplete        
        Dim sAction = EcSanitizer.GetSafeHtmlFragment(CheckVar(_PARAM_ACTION, "")).Trim.ToLower   ' Anti-XSS
        Ajax_Callback(Request.Form.ToString)
    End Sub

    Protected Sub Ajax_Callback(data As String)
        Dim args As NameValueCollection = HttpUtility.ParseQueryString(data)
        Dim sAction As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "action")).ToLower ' Anti-XSS
        Dim tResult As String = CStr(IIf(String.IsNullOrEmpty(sAction), "", sAction))

        Select Case sAction
            Case "select_user"
                SelectedUserID = CInt(EcSanitizer.GetSafeHtmlFragment(GetParam(args, "id")).Trim())
                tResult = AllCallbackData(sAction)
        End Select

        If tResult <> "" Then
            Response.Clear()
            Response.ContentType = "text/plain"
            Response.Write(tResult)
            Response.End()
        End If
    End Sub

    Private Function AllCallbackData(sCommand As String) As String
        Return String.Format("['{0}',{1},{2}]", sCommand, GetDatagridColumns(), GetDatagridData())
    End Function

End Class