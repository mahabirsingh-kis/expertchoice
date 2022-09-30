Imports System.Drawing
Imports System.Globalization
Imports Telerik.Web.UI

Partial Class RAPlotAlternatives
    Inherits clsComparionCorePage

    Public Enum FundedStates
        fsBecameFunded = 0
        fsBecameNotFunded = 1
        fsBothFunded = 2
        fsBothNotFunded = 3
        fsFundedPercentChanged = 4
    End Enum
    'A0891 ==

    Public Enum FundedFilteringModes 'A0888
        ffAll = 0
        ffFunded = 1
        ffPartiallyFunded = 2
        ffFullyFunded = 3
        ffNotFunded = 4
    End Enum

    Public Const OPT_RISK_OFFSET As Double = 0.01   ' D2849
    Public Const OPT_BUBBLE_SIZE As Integer = 12    ' D2849 + D3309

    Public Const OPT_SCENARIO_LEN As Integer = 45       'A0965
    Public Const OPT_DESCRIPTION_LEN As Integer = 200   'A0965

    Public AttributesList As New List(Of clsAttribute) 'A0888

    'Private Const CHARTS_IDX As Integer = 4    ' D3016

    Public Const COL_LEGEND_WIDTH As Integer = 190  ' D3091 + A1030
    Public Const LABELS_MAXLEN As Integer = 30       ' D3108

    Public SpinIDs() As String = {"", "", "", ""}   ' D3101

    ReadOnly Property SESSION_BEFORE_SCENARIO_NAME As String
        Get
            Return String.Format("RA_BeforeScenarioName_{0}", App.ProjectID)
        End Get
    End Property

    Public Property BeforeScenarioName As String
        Get
            Dim retVal As String = CType(Session(SESSION_BEFORE_SCENARIO_NAME), String)
            If retVal Is Nothing Then retVal = ""
            Return retVal
        End Get
        Set(value As String)
            Session(SESSION_BEFORE_SCENARIO_NAME) = value
        End Set
    End Property

    'A0891 ===
    ReadOnly Property SESSION_FUNDED_BEFORE As String
        Get
            Return String.Format("RA_FundedBefore_{0}", App.ProjectID)
        End Get
    End Property

    Private ReadOnly Property FundedBefore As Dictionary(Of String, Double)
        Get
            If Session(SESSION_FUNDED_BEFORE) Is Nothing Then
                Dim tFundedBefore As Dictionary(Of String, Double) = New Dictionary(Of String, Double)
                Session(SESSION_FUNDED_BEFORE) = tFundedBefore
            End If
            Return CType(Session(SESSION_FUNDED_BEFORE), Dictionary(Of String, Double))
        End Get
    End Property

    ReadOnly Property SESSION_SHOW_CHANGES As String
        Get
            Return String.Format("RA_FundedShowChanges_{0}", App.ProjectID)
        End Get
    End Property

    Public Property ShowChanges As Boolean
        Get
            If SessVar(SESSION_SHOW_CHANGES) Is Nothing Then Return False
            Return SessVar(SESSION_SHOW_CHANGES) = "1"
        End Get
        Set(value As Boolean)
            SessVar(SESSION_SHOW_CHANGES) = CStr(IIf(value, "1", "0"))
        End Set
    End Property

    ReadOnly Property SESSION_FUNDED_FILTERING_MODE As String 'A0888
        Get
            Return String.Format("RA_FundedFilteringMode_{0}", App.ProjectID)
        End Get
    End Property

    Public Property FundedFilteringMode As FundedFilteringModes
        Get
            Dim retVal As FundedFilteringModes = FundedFilteringModes.ffAll
            Dim s = SessVar(SESSION_FUNDED_FILTERING_MODE)
            If Not String.IsNullOrEmpty(s) Then retVal = CType(CInt(s), FundedFilteringModes)
            Return retVal
        End Get
        Set(value As FundedFilteringModes)
            SessVar(SESSION_FUNDED_FILTERING_MODE) = CInt(value).ToString
        End Set
    End Property

    ReadOnly Property SESSION_COLOR_BUBBLES_BY_CATEGORY As String 'A0888
        Get
            Return String.Format("RA_ColorBubblesByCategory_{0}", App.ProjectID)
        End Get
    End Property

    Public Property ColorBubblesByCategory As Boolean 'A0888
        Get
            If SessVar(SESSION_COLOR_BUBBLES_BY_CATEGORY) Is Nothing Then Return (AttributesList.Count > 0) ' D3108
            Return SessVar(SESSION_COLOR_BUBBLES_BY_CATEGORY) = "1"
        End Get
        Set(value As Boolean)
            SessVar(SESSION_COLOR_BUBBLES_BY_CATEGORY) = CStr(IIf(value, "1", "0"))
        End Set
    End Property

    ReadOnly Property SESSION_SELECTED_CATEGORY_GUID As String 'A0888
        Get
            Return String.Format("RA_SelecteCategoryGuid_{0}", App.ProjectID)
        End Get
    End Property

    ' D3146 ===
    ReadOnly Property COOKIE_BUBBLE_COEFF As String
        Get
            Return String.Format("RA_BubbleC_{0}", App.ProjectID)
        End Get
    End Property

    Public Property BubbleCoeff As Double
        Get
            Dim sVal As String = GetCookie(COOKIE_BUBBLE_COEFF, "")
            Dim tVal As Double
            If Not String2Double(sVal, tVal) Then tVal = 1
            Return tVal
        End Get
        Set(value As Double)
            SetCookie(COOKIE_BUBBLE_COEFF, value.ToString, False, False)
        End Set
    End Property

    ReadOnly Property COOKIE_AREA(tMode As Integer) As String
        Get
            Return String.Format("RA_Area_{0}_{1}", App.ProjectID, tMode)
        End Get
    End Property

    Public Property ViewArea(tMode As Integer) As Double()
        Get
            Dim Vals As Double() = {Double.MinValue, Double.MinValue, Double.MinValue, Double.MinValue}
            Dim sVals As String = SessVar(COOKIE_AREA(tMode))
            If String.IsNullOrEmpty(sVals) Then sVals = GetCookie(COOKIE_AREA(tMode), "")
            Dim Params As String() = sVals.Split(CChar(":"))
            If Params.Count = 4 Then
                For i As Integer = 0 To 3
                    String2Double(Params(i), Vals(i))
                Next
            End If
            Return Vals
        End Get
        Set(value As Double())
            Dim sVals As String = ""
            If value.Count = 4 Then
                For i As Integer = 0 To 3
                    sVals += CStr(IIf(i = 0, "", ":")) + value(i).ToString
                Next
            End If
            SetCookie(COOKIE_AREA(tMode), sVals, False, False)
            SessVar(COOKIE_AREA(tMode)) = sVals
        End Set
    End Property

    ReadOnly Property SESSION_CHART_ID As String
        Get
            Return String.Format("RA_ChartID_{0}", App.ProjectID)
        End Get
    End Property

    Public Property ChartID As Integer
        Get
            If SessVar(SESSION_CHART_ID) Is Nothing Then Return CInt(IIf(HasRisks, 1, 0)) Else Return CInt(SessVar(SESSION_CHART_ID)) ' D3230
        End Get
        Set(value As Integer)
            SessVar(SESSION_CHART_ID) = CStr(value)
        End Set
    End Property
    ' D3146 ==

    Public Property SelectedCategoryID As Guid
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

    Public Sub New()
        MyBase.New(_PGID_RA_PLOT_ALTS)
    End Sub

    ReadOnly Property RA As ResourceAligner
        Get
            Return App.ActiveProject.ProjectManager.ResourceAligner
        End Get
    End Property

    ReadOnly Property PM As clsProjectManager
        Get
            Return App.ActiveProject.ProjectManager
        End Get
    End Property

    ' D3016 ===
    Public ReadOnly Property BenefitName(fPlural As Boolean, fShort As Boolean) As String 'A1033
        Get
            If RA.Scenarios.ActiveScenario.Settings.Risks AndAlso HasRisks() Then   ' D3232
                Return ResString(String.Format("lbl{0}Benefit{1}", IIf(fShort, "E", "Expected"), IIf(fPlural, "s", ""))) ' D3120
            Else
                Return ResString(String.Format("lblBenefit{0}", IIf(fPlural, "s", ""))) ' D3120
            End If
        End Get
    End Property
    ' D3016 ==

    ' D3230 ===
    Public Function HasCosts() As Boolean
        Return RA.Scenarios.ActiveScenario.Alternatives.Sum(Function(a) a.Cost) > 0
    End Function

    Public Function HasRisks() As Boolean
        Return RA.Scenarios.ActiveScenario.Alternatives.Sum(Function(a) a.RiskOriginal) > 0
    End Function
    ' D3230 ==

    ' D3309 ===
    Private Function RoundAxisLimit(tMaxX As Double) As Double
        If tMaxX > 1000 Then tMaxX = Math.Ceiling(tMaxX / 95) * 100
        If tMaxX > 100 AndAlso tMaxX < 1000 Then tMaxX = Math.Ceiling(tMaxX / 9) * 10
        If tMaxX > 10 AndAlso tMaxX < 100 Then tMaxX = Math.Ceiling(tMaxX / 10) * 10
        If tMaxX < 10 AndAlso tMaxX > 1.5 Then tMaxX = Math.Ceiling(tMaxX)
        If tMaxX < 1.5 Then tMaxX = Math.Ceiling(tMaxX * 10) / 10 'A0891
        If tMaxX <= 0 Then tMaxX = 0.1
        Return tMaxX
    End Function
    ' D3309 ==

    ' D2848 + D2849 ===
    ''' <summary>
    ''' Get Jscript array with data for plot chart
    ''' </summary>
    ''' <param name="Mode">0 - [E]B vs C; 1 - [E]B vs C /P.F; 2 - [E]B vs P.F (C), 4 - B vs C</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetData(Mode As Integer, Optional isArr As Boolean = False) As String
        Dim sData As String = ""
        Dim tMaxX As Double = Integer.MinValue
        Dim tMaxY As Double = Integer.MinValue
        Dim tMinX As Double = Integer.MaxValue
        Dim tMinY As Double = Integer.MaxValue
        Dim fCountProjects As Integer = 0
        Dim fSumVisibleCosts As Double = 0 ' D3913
        Dim fSumVisibleBenefits As Double = 0
        Dim dCountProjectsByCategory As New Dictionary(Of String, Integer)
        Dim dCostsByCategory As New Dictionary(Of String, Double)
        Dim dBenefitByCategory As New Dictionary(Of String, Double)
        'A0888 ===
        Dim tSelectedCategoryID As Guid = SelectedCategoryID
        Dim tSelectedCategoryEnum As clsAttributeEnumeration = Nothing
        If (tSelectedCategoryID.Equals(Guid.Empty) OrElse AttributesList.Where(Function(attr) attr.ID.Equals(tSelectedCategoryID)).Count = 0) AndAlso AttributesList.Count > 0 Then tSelectedCategoryID = AttributesList(0).ID
        Dim tSelectedAttr As clsAttribute = Nothing
        Dim tSelectedCategoryMultiValues As List(Of String) = New List(Of String)
        For Each attr In AttributesList
            If attr.ID = tSelectedCategoryID Then tSelectedAttr = attr
        Next
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

        Dim sColorLegend As String = ""
        Dim tColorLegendVisible As Boolean = False
        'A0888 ==
        Dim FixSize As Boolean = True   ' D3015
        Dim tMaxCost As Double = Double.MinValue 'A0959
        If RA.Scenarios.Scenarios(0).AlternativesFull.Count > 0 Then tMaxCost = RA.Scenarios.Scenarios(0).AlternativesFull.Max(Function(a) Math.Abs(a.Cost)) ' D3309 + A0959 + A1026 + D3837
        If tMaxCost = 0 Then tMaxCost = 0.0001 ' D3309

        ' D3913 ===
        Dim Coeff As Double = 1
        Dim sCoeff As String = ""
        If tMaxCost > 1000000 Then
            Coeff = 1000
            sCoeff = "K"
        End If
        If tMaxCost >= 1000000000 Then
            Coeff = 1000000
            sCoeff = "M"
        End If
        ' D3913 ==

        Dim altIndex As Integer = 0
        For Each tAlt0 As RAAlternative In RA.Scenarios.Scenarios(0).AlternativesFull   ' D3837
            Dim tAlt0ID As String = tAlt0.ID
            Dim tAlt As RAAlternative = RA.Scenarios.ActiveScenario.Alternatives.Find(Function(fAlt) fAlt.ID = tAlt0ID)
            If tAlt IsNot Nothing Then
                'A0888 ===
                Dim tAltVisible As Boolean = True
                Select Case FundedFilteringMode
                    Case FundedFilteringModes.ffFunded
                        tAltVisible = RA.Solver.SolverState = raSolverState.raSolved AndAlso tAlt.DisplayFunded > 0   ' D3091 + A0889 + A0939
                    Case FundedFilteringModes.ffPartiallyFunded
                        tAltVisible = RA.Solver.SolverState = raSolverState.raSolved AndAlso tAlt.IsPartiallyFunded 'A0933 + A0939
                    Case FundedFilteringModes.ffFullyFunded
                        tAltVisible = RA.Solver.SolverState = raSolverState.raSolved AndAlso tAlt.DisplayFunded = 1    ' D3091 + 'A0939
                    Case FundedFilteringModes.ffNotFunded
                        tAltVisible = RA.Solver.SolverState = raSolverState.raSolved AndAlso tAlt.DisplayFunded = 0    'A0939
                End Select
                Dim sRadius As String = OPT_BUBBLE_SIZE.ToString ' real size of the bubble
                Dim tRisk As String = "0" ' risk label
                Select Case Mode
                    Case 1
                        If tAlt.RiskOriginal < 0.001 Then sRadius = "1" Else sRadius = Math.Ceiling(OPT_RISK_OFFSET + 5 * Math.Sqrt((OPT_RISK_OFFSET + tAlt.RiskOriginal) / Math.PI) * OPT_BUBBLE_SIZE).ToString() ' D3015 + A0946 + D3309
                        'If tAlt.Risk < 0.0001 Then sRadius = "0.0001" Else sRadius = (Math.Sqrt(((OPT_RISK_OFFSET + tAlt.Risk) / Math.PI))).ToString(RA.Scenarios.GlobalSettings.PrecisionFormat) ' D3015 + A0946
                        'If tAlt.Risk > 0.001 Then FixSize = False ' D3015
                        tRisk = tAlt.RiskOriginal.ToString(RA.Scenarios.GlobalSettings.PrecisionFormat) 'A0946
                    Case 2
                        'If Math.Abs(tAlt.Cost) < 0.0001 Then sRadius = "1" Else sRadius = CStr(IIf(tAlt.Cost < 0, "-", "")) + Math.Ceiling(OPT_RISK_OFFSET + 4 * Math.Sqrt(Math.Abs(tAlt.Cost) / tMaxCost / Math.PI) * OPT_BUBBLE_SIZE).ToString ' D2850 + A0946 + D3309 -D4540
                        'If tAlt.Cost < 0.0001 Then sRadius = "0.0001" Else sRadius = Math.Sqrt(tAlt.Cost / Math.PI).ToString(RA.Scenarios.GlobalSettings.PrecisionFormat) ' D2850 + A0946
                        'If Math.Abs(tAlt.Cost) > 0.0001 Then FixSize = False ' D3015
                        If tAlt.Risk < 0.001 Then sRadius = "1" Else sRadius = Math.Ceiling(OPT_RISK_OFFSET + 5 * Math.Sqrt((OPT_RISK_OFFSET + tAlt.Risk) / Math.PI) * OPT_BUBBLE_SIZE).ToString() ' D4540
                        tRisk = tAlt.Risk.ToString(RA.Scenarios.GlobalSettings.PrecisionFormat) ' D4540
                End Select
                ' [X, Y, Radius, Title] OR [X, Y, Radius, {label:Title, color:Color}] 'A0888
                'Dim x As Double = CDbl(IIf(Mode = 2, tAlt.RiskOriginal, tAlt.Cost / Coeff)) ' D3913 -D4540
                Dim x As Double = tAlt.Cost / Coeff     ' D3913 +  D4540
                Dim y As Double = If(Mode <> 0, tAlt.BenefitOriginal, tAlt.Benefit) ' D4540
                If x > tMaxX Then tMaxX = x
                If y > tMaxY Then tMaxY = y
                If x < tMinX Then tMinX = x ' D3309
                If y < tMinY Then tMinY = y ' D3309
                If tAltVisible Then 'A0888 ==
                    'If tAlt.Cost > 0 Then sData += IIf(sData = "", "", ",") + String.Format("[{0},{1},{2},'{3}']", JS_SafeNumber(x.ToString("F4")), JS_SafeNumber(y.ToString("F4")), JS_SafeNumber(sRadius), JS_SafeString(tAlt.Name))
                    Dim sCategory As String = ""
                    'A0888 ===
                    Dim altGuid As Guid = New Guid(tAlt.ID)
                    Dim tColor As String = ""
                    If ColorBubblesByCategory AndAlso AttributesList.Count > 0 AndAlso Not tSelectedCategoryID.Equals(Guid.Empty) Then
                        tColorLegendVisible = True
                        Dim mAlt As clsNode = PM.AltsHierarchy(PM.ActiveAltsHierarchy).GetNodeByID(altGuid)
                        If mAlt IsNot Nothing Then
                            tColor = NoSpecificColor
                            Dim attrVal As Object = PM.Attributes.GetAttributeValue(tSelectedCategoryID, altGuid)
                            If attrVal IsNot Nothing AndAlso tSelectedCategoryEnum IsNot Nothing AndAlso tSelectedCategoryEnum.Items.Count > 0 Then
                                Select Case tSelectedAttr.ValueType
                                    Case AttributeValueTypes.avtEnumeration
                                        Dim item As clsAttributeEnumerationItem = Nothing
                                        If TypeOf attrVal Is String Then item = tSelectedCategoryEnum.GetItemByID(New Guid(CStr(attrVal)))
                                        If TypeOf attrVal Is Guid Then item = tSelectedCategoryEnum.GetItemByID(CType(attrVal, Guid))
                                        If item IsNot Nothing Then
                                            Dim index As Integer = tSelectedCategoryEnum.Items.IndexOf(item)
                                            If index >= 0 Then
                                                sCategory = item.ID.ToString
                                                tColor = GetPaletteColor(CurrentPaletteID(PM), index, True)
                                            End If
                                        End If
                                    Case AttributeValueTypes.avtEnumerationMulti
                                        Dim sVal As String = CStr(attrVal)
                                        If Not String.IsNullOrEmpty(sVal) Then
                                            sCategory = sVal
                                            If Not tSelectedCategoryMultiValues.Contains(sVal) Then tSelectedCategoryMultiValues.Add(sVal)
                                            tColor = GetPaletteColor(CurrentPaletteID(PM), tSelectedCategoryMultiValues.IndexOf(sVal), True)
                                        End If
                                End Select
                            End If
                        End If
                    Else
                        'tColor = GetAlternativeColor(PM, RA.Scenarios.Scenarios(0).AlternativesFull.IndexOf(tAlt0), altGuid)
                        tColor = GetAlternativeColor(PM, altIndex, altGuid)
                    End If 'A0888 ==

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

                    'A0891 ===
                    Dim tFundedState As String = ""
                    Dim tIsFunded As Boolean = False
                    If RA.Solver.SolverState = raSolverState.raSolved AndAlso tAlt.DisplayFunded > 0 Then tIsFunded = True
                    If FundedBefore IsNot Nothing AndAlso ShowChanges Then
                        Dim tFundedPercent As Double = 0
                        If tIsFunded Then tFundedPercent = tAlt.DisplayFunded
                        Dim sFundedPercent As String = JS_SafeNumber(tFundedPercent)
                        If tIsFunded AndAlso Not FundedBefore.Keys.Contains(tAlt.ID) Then
                            tFundedState = String.Format(",'+',{0},{1}", CInt(FundedStates.fsBecameFunded), sFundedPercent) 'Became funded
                        End If
                        If Not tIsFunded AndAlso FundedBefore.Keys.Contains(tAlt.ID) Then
                            tFundedState = String.Format(",'-',{0},0", CInt(FundedStates.fsBecameNotFunded)) 'stopped being funded
                        End If
                        If tIsFunded AndAlso FundedBefore.Keys.Contains(tAlt.ID) Then
                            If tAlt.DisplayFunded <> FundedBefore(tAlt.ID) Then
                                tFundedState = String.Format(",'%',{0},{1}", CInt(FundedStates.fsFundedPercentChanged), sFundedPercent) 'funded % changed
                            Else
                                tFundedState = String.Format(",'F',{0},{1}", CInt(FundedStates.fsBothFunded), sFundedPercent) 'funded in both scenarios
                            End If
                        End If
                        If Not tIsFunded AndAlso Not FundedBefore.Keys.Contains(tAlt.ID) Then
                            tFundedState = String.Format(",'N',{0},0", CInt(FundedStates.fsBothNotFunded)) 'funded in both scenarios
                        End If
                    End If
                    'A0891 == 

                    'sData += IIf(sData = "", "", ",") + String.Format("[{0},{1},{2},'{3}']", JS_SafeNumber(x.ToString("F4")), JS_SafeNumber(y.ToString("F4")), JS_SafeNumber(sRadius), JS_SafeString(ShortString(tAlt.Name, 60, True))) '-A0888
                    sData += CStr(IIf(sData = "", "", ",")) + String.Format("[{0},{1},{2},{{label:'{3}', color:'{4}', cost:'{8}', risk:'{9}'}},{10},'{5}','{6}'{7}]", JS_SafeNumber(x.ToString(RA.Scenarios.GlobalSettings.PrecisionFormat)), JS_SafeNumber(y.ToString(RA.Scenarios.GlobalSettings.PrecisionFormat)), JS_SafeNumber(sRadius.Replace("-", "")), JS_SafeString(ShortString(tAlt.Name, LABELS_MAXLEN, True)), tColor, tAlt.ID, sCategory, tFundedState, CostString(tAlt.Cost / Coeff), JS_SafeNumber(tRisk), JS_SafeNumber(sRadius)) 'A0888 + D3108 + A0891 + A0902 + A0907 + D3199 + A0946 + D3309 + D3913
                    ' D2849 ==
                End If
            End If
            altIndex += 1
        Next

        ' Names: "Title", "X axis", "Y axis", "Radius", "Show radius/mode" ?
        Dim sNames As String = ""
        Select Case Mode
            Case 0
                ' D3120 ===
                sNames = String.Format("'{0}','{1}, ${4}','{2}','{3}'", JS_SafeString(String.Format("{0} {1}", BenefitName(True, False), ResString("lblRAvsCost"))), JS_SafeString(ResString("lblRACost")), JS_SafeString(BenefitName(False, True)), JS_SafeString(ResString("optPFailure")), sCoeff) ' D3913 + D4356
            Case 1
                sNames = String.Format("'{0}','{1}, ${4}','{2}','{3}'", JS_SafeString(String.Format("{0} {1} ({2})", ResString("lblBenefits"), ResString("lblRAvsCost"), ResString("lblRAPFailureSize"))), JS_SafeString(ResString("lblRACost")), JS_SafeString(ResString("lblBenefits")), JS_SafeString(ResString("optPFailure")), sCoeff)   ' D4356 + D4540
            Case 2
                sNames = String.Format("'{0}','{1}, ${4}','{2}','{3}'", JS_SafeString(String.Format("{0} {1} ({2})", ResString("lblBenefits"), ResString("lblRAvsCost"), ResString("lblRARiskSize"))), JS_SafeString(ResString("lblRACost")), JS_SafeString(ResString("lblBenefits")), JS_SafeString(ResString("lblRARisks")), sCoeff)   ' D3913 + D4356 + D4540
                ' D3120 ==
            Case 3  ' D4533
                sNames = String.Format("'{0}','{1}, ${4}','{2}','{3}'", JS_SafeString(String.Format("{0} {1}", ResString("lblBenefits"), ResString("lblRAvsCost"))), JS_SafeString(ResString("lblRACost")), JS_SafeString(ResString("lblBenefits")), JS_SafeString(ResString("optPFailure")), sCoeff) ' D4533
        End Select
        tMaxX *= 1.03
        tMaxY *= 1.02
        ' D3309 ===
        tMaxX = RoundAxisLimit(tMaxX)
        tMaxY = RoundAxisLimit(tMaxY)
        If tMinX < 0 Then tMinX = -RoundAxisLimit(Math.Abs(tMinX)) Else tMinX = 0
        If tMinY < 0 Then tMinY = -RoundAxisLimit(Math.Abs(tMinY)) Else tMinY = 0
        ' D3309 ==
        If tColorLegendVisible Then
            Dim index As Integer = 0
            Select Case tSelectedAttr.ValueType
                Case AttributeValueTypes.avtEnumeration
                    If tSelectedCategoryEnum IsNot Nothing AndAlso tSelectedCategoryEnum.Items IsNot Nothing Then 'A0899
                        For Each item As clsAttributeEnumerationItem In tSelectedCategoryEnum.Items
                            Dim cat As String = item.ID.ToString
                            sColorLegend += CStr(IIf(sColorLegend = "", "", ",")) + String.Format("['{0}','{1}','{2}',{3},'{4}','{5}','{6}',{7},{8},'{9}']", GetPaletteColor(CurrentPaletteID(PM), index, True), JS_SafeString(item.Value), JS_SafeString(item.ID.ToString), JS_SafeNumber(dCountProjectsByCategory(cat)), JS_Percent(dCountProjectsByCategory(cat), fCountProjects), CostString(dCostsByCategory(cat)), JS_Percent(dCostsByCategory(cat), fSumVisibleCosts), JS_SafeNumber(dCostsByCategory(cat)), JS_SafeNumber(Math.Round(dBenefitByCategory(cat), 4)), JS_Percent(Math.Round(dBenefitByCategory(cat), 4), fSumVisibleBenefits))
                            index += 1
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
                                sColorLegend += CStr(IIf(sColorLegend = "", "", ",")) + String.Format("['{0}','{1}']", GetPaletteColor(CurrentPaletteID(PM), index, True), JS_SafeString(sName))
                            End If
                            index += 1
                        Next
                    End If
            End Select
        End If
        Dim key = Guid.Empty.ToString
        sColorLegend += CStr(IIf(sColorLegend = "", "", ",")) + String.Format("['{0}','{1}','{2}',{3},'{4}','{5}','{6}',{7},{8},'{9}']", NoSpecificColor, ResString("lblNoCategory"), JS_SafeString(Guid.Empty.ToString), JS_SafeNumber(dCountProjectsByCategory(key)), JS_Percent(dCountProjectsByCategory(key), fCountProjects), CostString(dCostsByCategory(key)), JS_Percent(dCostsByCategory(key), fSumVisibleCosts), JS_SafeNumber(dCostsByCategory(key)), JS_SafeNumber(Math.Round(dBenefitByCategory(key), 4)), JS_Percent(Math.Round(dBenefitByCategory(key), 4), fSumVisibleBenefits))
        'A0888 ==
        'A0902 ===
        Dim tSelectedAttrType As Integer = -1
        Dim tSelectedAttrItems As String = "[]"
        If tSelectedAttr IsNot Nothing Then
            tSelectedAttrItems = ""
            If tSelectedAttr.ValueType = AttributeValueTypes.avtEnumeration Then tSelectedAttrItems = String.Format("['{0}','{1}']", JS_SafeString(Guid.Empty.ToString), " ")
            tSelectedAttrType = CInt(tSelectedAttr.ValueType)
            If tSelectedCategoryEnum IsNot Nothing AndAlso tSelectedCategoryEnum.Items.Count > 0 Then
                For Each item As clsAttributeEnumerationItem In tSelectedCategoryEnum.Items
                    tSelectedAttrItems += CStr(IIf(tSelectedAttrItems = "", "", ",")) + String.Format("['{0}','{1}']", item.ID, JS_SafeString(ShortString(item.Value, 25)))
                Next
            End If

            'If tSelectedAttr.ValueType = AttributeValueTypes.avtEnumeration Then tSelectedAttrItems += CStr(IIf(tSelectedAttrItems = "", "", ",")) + String.Format("['{0}','{1}','{2}']", NoSpecificColor, ResString("lblNoCategory"), JS_SafeString(Guid.Empty.ToString))
            tSelectedAttrItems = "[" + tSelectedAttrItems + "]"
        End If
        'A0902 ==
        ' D3146 ===
        'Dim tMinX As Double = 0
        'Dim tMinY As Double = 0
        If tMaxX <= 0.1 AndAlso tMinX >= 0 Then tMinX = -tMaxX ' D3230
        Dim Area As Double() = ViewArea(Mode)
        If Area(0) <> Double.MinValue Then tMinX = Area(0)
        If Area(1) <> Double.MinValue Then tMaxX = Area(1)
        If Area(2) <> Double.MinValue Then tMinY = Area(2)
        If Area(3) <> Double.MinValue Then tMaxY = Area(3)
        sNames = String.Format("{0},{1},{2},{3},{4},{5},{6}", sNames, Mode, JS_SafeNumber(tMinX.ToString(RA.Scenarios.GlobalSettings.PrecisionFormat)), JS_SafeNumber(tMaxX.ToString(RA.Scenarios.GlobalSettings.PrecisionFormat)), JS_SafeNumber(tMinY.ToString(RA.Scenarios.GlobalSettings.PrecisionFormat)), JS_SafeNumber(tMaxY.ToString(RA.Scenarios.GlobalSettings.PrecisionFormat)), IIf(FixSize OrElse Mode = 0 OrElse Mode = 3, 1, 0)) 'A0888 + A0902 + D3230
        ' D3146 ==
        Dim sCatData = String.Format("[{0}],{1},{2}", sColorLegend, tSelectedAttrType, tSelectedAttrItems) 'A0902
        'Return String.Format("[{0}],[{1}]", sNames, sData)
        Return String.Format("{3}[{0}],[{1}],[{2}]{4}", sNames, sData, sCatData, IIf(isArr, "[", ""), IIf(isArr, "]", "")) 'A0888 + A0902
    End Function
    ' D2848 ==

    Private Function JS_Percent(value As Double, total As Double) As String
        'Return CStr(IIf(value <> 0, JS_SafeString("<span style='color:#777;'>" + JS_SafeNumber(Math.Round(value * 100 / total, 1)) + "</span>"), ""))
        If Math.Abs(total) < 0.0000001 Then Return "0"
        Return JS_SafeNumber((value * 100 / total).ToString("f1"))
    End Function

    'A0902 ===
    Private Function GetStandardColors() As ColorPickerItemCollection
        Dim retVal As ColorPickerItemCollection = New ColorPickerItemCollection
        For Each item As String In GetPalette(2)
            Dim cpi As ColorPickerItem = New ColorPickerItem(ColorTranslator.FromHtml(item))
            retVal.Add(cpi)
        Next
        Return retVal
    End Function

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        If Not IsPostBack AndAlso Not IsCallback Then
            'Dim colors As ColorPickerItemCollection = GetStandardColors()
            'RadColorPickerBubble.Items.AddRange(colors)
            pnlView.Attributes("style") += "width:" + (COL_LEGEND_WIDTH - 4).ToString + "px"
        End If
    End Sub
    'A0902 ==

    ' D2849 ===
    Protected Sub Page_Init(sender As Object, e As EventArgs) Handles Me.Init
        RA.Load()
        InitAttributesList() 'A1323
        If Not IsPostBack AndAlso Not isCallback Then
            'ClientScript.RegisterStartupScript(GetType(String), "InitChart", "InitPage(); setTimeout('drawChart();', 300);", True) ' D2848 - A0902
            RA.Scenarios.CheckModel() 'A1324
            'If RA.Solver.SolverState <> raSolverState.raSolved Then -A0888
            'If RA.Solver.SolverState <> raSolverState.raSolved AndAlso RA.Scenarios.GlobalSettings.isAutoSolve Then RA.Solve() ' D3900
            If RA.Solver.SolverState <> raSolverState.raSolved OrElse RA.Scenarios.GlobalSettings.isAutoSolve Then RA.Solve() ' D3900 + D4585
            If Session(SESSION_CHART_ID) Is Nothing AndAlso HasRisks() Then ChartID = 1 ' D3108 + D3146
            Dim cID As Integer = CheckVar("chart", ChartID)
            If cID >= 0 AndAlso cID < 4 Then ChartID = cID
            ShowChanges = CheckVar("show_changes", False)
            If ChartID = 1 AndAlso Not HasRisks() Then ChartID = 0 ' D3230
            If ChartID = 2 AndAlso Not HasCosts() Then ChartID = 1 ' D3230
        End If
    End Sub
    ' D2849 ==

    ' D3016 ===
    Protected Sub Page_InitComplete(sender As Object, e As EventArgs) Handles Me.InitComplete
        Dim sAction = EcSanitizer.GetSafeHtmlFragment(CheckVar(_PARAM_PAGE_ACTION, "")).Trim.ToLower    'Anti-XSS
        Select Case sAction
            Case "scenario"
                Dim ID As Integer = CheckVar("sid", RA.Scenarios.ActiveScenarioID)
                If ID <> RA.Scenarios.ActiveScenarioID AndAlso RA.Scenarios.Scenarios.ContainsKey(ID) Then
                    'A0891 ===
                    ShowChanges = True
                    FundedBefore.Clear()
                    BeforeScenarioName = RA.Scenarios.ActiveScenario.Name
                    For Each tAlt In RA.Scenarios.ActiveScenario.Alternatives
                        If RA.Solver.SolverState = raSolverState.raSolved AndAlso tAlt.DisplayFunded > 0 Then FundedBefore.Add(tAlt.ID, tAlt.DisplayFunded) 'A0939
                    Next
                    'A0891 ==
                    RA.Scenarios.ActiveScenarioID = ID
                    RA.Solver.ResetSolver()
                End If                
                Response.Redirect(PageURL(CurrentPageID, GetTempThemeURI(False) + "&chart=" + EcSanitizer.GetSafeHtmlFragment(CheckVar("chart", ChartID.ToString))) + "&show_changes=1", True)  'Anti-XSS
        End Select
        'A0888 ===
        If Not IsCallback AndAlso Not IsPostBack Then
            '-A1323 InitAttributesList()
            ' D3120 ===
            'RadToolBarMain.Items(0).Text = ResString("lblScenario") + ":" '-A1018
            'RadToolBarMain.Items(3).Text = ResString("lblChart") + ":" '-A1018
            RadToolBarMain.Items(6).Text = ResString("lblShowLabels")
            RadToolBarMain.Items(7).Text = ResString("lblRAPlotAltsViewArea")
            'RadToolBarMain.Items(9).Text = ResString("lblBubbleSize") + ":" '-A1018
            RadToolBarMain.Items(10).Text = ResString("lblBubbleIncrease")
            RadToolBarMain.Items(11).Text = ResString("lblBubbleDecrease")
            RadToolBarMain.Items(12).Text = ResString("lblBubbleReset")
            ' D3120 ==            
            'ClientScript.RegisterStartupScript(GetType(String), "InitChart", "InitPage(); setTimeout('drawChart();', 150);", True) ' D2848 + A0902
        End If
        Ajax_Callback(Request.Form.ToString)
        'A0888 ==
    End Sub

    'Protected Sub RadToolBarMain_Init(sender As Object, e As System.EventArgs) Handles RadToolBarMain.Init
    '    If Not IsPostBack AndAlso Not IsCallback Then
    '        '-A0888 ===
    '        'RadToolBarMain.Items(CHARTS_IDX).Text = BenefitName(True, True) + " vs. Cost"
    '        'RadToolBarMain.Items(CHARTS_IDX + 1).Text = BenefitName(True, True) + " vs. Cost (Risk)"
    '        'RadToolBarMain.Items(CHARTS_IDX + 2).Text = BenefitName(True, True) + " vs. Risk (Cost)"
    '        'CType(RadToolBarMain.Items(CHARTS_IDX + ChartID), RadToolBarButton).Checked = True
    '        '-A0888 ==
    '    End If
    'End Sub

    Public Function GetScenarios() As String
        Dim sRes As String = ""
        For Each tScen As Integer In RA.Scenarios.Scenarios.Keys
            sRes += String.Format("<option value='{0}'{2}>{1}</option>", tScen, ShortString(SafeFormString(RA.Scenarios.Scenarios(tScen).Name), OPT_SCENARIO_LEN, True) + CStr(IIf(String.IsNullOrEmpty(RA.Scenarios.Scenarios(tScen).Description), "", String.Format(" ({0})", ShortString(SafeFormString(RA.Scenarios.Scenarios(tScen).Description), OPT_DESCRIPTION_LEN)))), IIf(tScen = RA.Scenarios.ActiveScenarioID, " selected", ""))  ' D3020
        Next
        Return String.Format("<select id='cbScenarios' style='width:130px; margin-top:3px; margin-right:4px;' onchange='onSetScenario(this.value);'>{0}</select>", sRes)
    End Function
    ' D3016 ==

    'A0888 ===
    Public Function GetCharts() As String
        Dim sRes As String = ""
        ' D3120 ===
        If RA.Scenarios.ActiveScenario.Settings.Risks Then sRes += String.Format("<option value='3' " + CStr(IIf(ChartID = 3, "selected='selected'", "")) + ">" + String.Format("{0} {1}", ResString("lblBenefits"), ResString("lblRAvsCost")) + "</option>") ' D4533
        sRes += String.Format("<option value='0' " + CStr(IIf(ChartID = 0 OrElse (Not HasRisks() AndAlso ChartID = 1), "selected='selected'", "")) + ">" + String.Format("{0} {1}", BenefitName(True, True), ResString("lblRAvsCost")) + "</option>")   ' D3230
        sRes += String.Format("<option value='1' " + CStr(IIf(ChartID = 1 AndAlso HasRisks(), "selected='selected'", "")) + CStr(IIf(HasRisks, "", " disabled")) + ">" + String.Format("{0} {1} ({2})", ResString("lblBenefits"), ResString("lblRAvsCost"), ResString("optPFailure")) + "</option>")  ' D3230 + D4356
        sRes += String.Format("<option value='2' " + CStr(IIf(ChartID = 2, "selected='selected'", "")) + CStr(IIf(HasCosts, "", " disabled")) + ">" + String.Format("{0} {1} ({2})", ResString("lblBenefits"), ResString("lblRAvsCost"), ResString("lblRARisks")) + "</option>")
        ' D3120 ==
        Return String.Format("<select id='cbCharts' style='width:180px; margin-top:3px; margin-right:4px;' onchange='onSetChart(this.value);'>{0}</select>", sRes)
    End Function

    Public Function GetCategories() As String
        Dim sRes As String = ""
        For Each tAttr As clsAttribute In AttributesList
            sRes += String.Format("<option value='{0}' {2}>{1}</option>", tAttr.ID, ShortString(SafeFormString(tAttr.Name), 40), IIf(tAttr.ID = SelectedCategoryID, " selected='selected' ", ""))
        Next
        Return String.Format("<select id='cbCategories' style='width:145px;margin-top:2px;margin-bottom:3px;' onchange='onSetCategory(this.value);' {1}>{0}</select>", sRes, IIf(AttributesList.Count = 0 OrElse Not ColorBubblesByCategory, " disabled='disabled' ", ""))
    End Function

    Private Sub InitAttributesList()
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

    Private Sub SetAltsColorByCategory(CatID As Guid)
        Dim tSelectedCategoryID As Guid = CatID        
        If (tSelectedCategoryID.Equals(Guid.Empty) OrElse AttributesList.Where(Function(attr) attr.ID.Equals(CatID)).Count = 0) AndAlso AttributesList.Count > 0 Then tSelectedCategoryID = AttributesList(0).ID
        Dim tSelectedAttr As clsAttribute = AttributesList.Where(Function(attr) attr.ID.Equals(tSelectedCategoryID))(0)
        Dim tSelectedCategoryEnum As clsAttributeEnumeration = PM.Attributes.GetEnumByID(tSelectedAttr.EnumID)
        Dim tSelectedCategoryMultiValues As List(Of String) = New List(Of String)

        If tSelectedAttr IsNot Nothing AndAlso tSelectedCategoryEnum IsNot Nothing Then
            Dim tChanged As Boolean = False
            Dim tColor As String = ""

            For Each mAlt As clsNode In PM.AltsHierarchy(PM.ActiveAltsHierarchy).TerminalNodes
                tColor = NoSpecificColor
                Dim attrVal As Object = PM.Attributes.GetAttributeValue(tSelectedCategoryID, mAlt.NodeGuidID)
                If attrVal IsNot Nothing AndAlso tSelectedCategoryEnum IsNot Nothing AndAlso tSelectedCategoryEnum.Items.Count > 0 Then
                    Select Case tSelectedAttr.ValueType
                        Case AttributeValueTypes.avtEnumeration
                            Dim item As clsAttributeEnumerationItem = Nothing
                            If TypeOf attrVal Is String Then item = tSelectedCategoryEnum.GetItemByID(New Guid(CStr(attrVal)))
                            If TypeOf attrVal Is Guid Then item = tSelectedCategoryEnum.GetItemByID(CType(attrVal, Guid))
                            If item IsNot Nothing Then
                                Dim index As Integer = tSelectedCategoryEnum.Items.IndexOf(item)
                                If index >= 0 Then tColor = GetPaletteColor(CurrentPaletteID(PM), index, True)
                            End If
                        Case AttributeValueTypes.avtEnumerationMulti
                            Dim sVal As String = CStr(attrVal)
                            If Not String.IsNullOrEmpty(sVal) Then
                                If Not tSelectedCategoryMultiValues.Contains(sVal) Then tSelectedCategoryMultiValues.Add(sVal)
                                tColor = GetPaletteColor(CurrentPaletteID(PM), tSelectedCategoryMultiValues.IndexOf(sVal), True)
                            End If
                    End Select
                End If
                If PM.Attributes.SetAttributeValue(ATTRIBUTE_DEFAULT_BRUSH_COLOR_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtLong, HTMLColor2Long(tColor), mAlt.NodeGuidID, Guid.Empty) Then
                    tChanged = True
                End If
            Next

            If tChanged Then PM.Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, PM.StorageManager.ProjectLocation, PM.StorageManager.ProviderType, PM.StorageManager.ModelID, UNDEFINED_USER_ID)
        End If
    End Sub

    Private Sub Ajax_Callback(data As String)
        Dim args As NameValueCollection = HttpUtility.ParseQueryString(data)

        Dim tResult As String = ""
        'Dim tSuccess As Boolean = False
        Dim tAction As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, _PARAM_ACTION)).ToLower  'Anti-XSS

        If Not String.IsNullOrEmpty(tAction) Then
            'do some action
            Select Case tAction

                Case "funded_filter"
                    FundedFilteringMode = CType(CheckVar("filter_id", FundedFilteringModes.ffAll), FundedFilteringModes)

                Case "set_chart"
                    Dim ID As Integer = CheckVar("id", ChartID) ' D3146
                    If ID >= 0 AndAlso ID < 4 Then ChartID = ID ' D4533

                Case "color_by_category"
                    ColorBubblesByCategory = CheckVar("chk", False)

                Case "select_category"
                    SelectedCategoryID = New Guid(CheckVar("cat_guid", Guid.Empty.ToString))

                Case "set_alts_color_by_category"
                    SetAltsColorByCategory(SelectedCategoryID)

                Case "set_color" 'A0902
                    Dim alt_id As Guid = New Guid(CheckVar("alt_id", Guid.Empty.ToString))
                    Dim tColor As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("color", ""))   'Anti-XSS
                    If Not alt_id.Equals(Guid.Empty) AndAlso Not String.IsNullOrEmpty(tColor) Then
                        If PM.Attributes.SetAttributeValue(ATTRIBUTE_DEFAULT_BRUSH_COLOR_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtLong, Long.Parse("FF" + tColor.Replace("#", ""), NumberStyles.HexNumber), alt_id, Guid.Empty) Then
                            PM.Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, PM.StorageManager.ProjectLocation, PM.StorageManager.ProviderType, PM.StorageManager.ModelID, UNDEFINED_USER_ID)
                        End If
                    End If

                Case "set_category" 'A0902
                    Dim alt_id As Guid = New Guid(CheckVar("alt_id", Guid.Empty.ToString))
                    Dim cat_id As Guid = New Guid(CheckVar("cat_id", Guid.Empty.ToString))
                    If Not alt_id.Equals(Guid.Empty) AndAlso Not SelectedCategoryID.Equals(Guid.Empty) Then
                        If PM.Attributes.SetAttributeValue(SelectedCategoryID, UNDEFINED_USER_ID, AttributeValueTypes.avtEnumeration, IIf(cat_id.Equals(Guid.Empty), Nothing, cat_id), alt_id, Guid.Empty) Then
                            PM.Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, PM.StorageManager.ProjectLocation, PM.StorageManager.ProviderType, PM.StorageManager.ModelID, UNDEFINED_USER_ID)
                        End If
                    End If

                Case "set_multi_category" 'A0902
                    Dim alt_id As Guid = New Guid(CheckVar("alt_id", Guid.Empty.ToString))
                    Dim cat_ids As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("cat_ids", ""))    'Anti-XSS
                    Dim cur_value As String = CStr(PM.Attributes.GetAttributeValue(SelectedCategoryID, alt_id))
                    If cur_value <> cat_ids Then
                        If PM.Attributes.SetAttributeValue(SelectedCategoryID, UNDEFINED_USER_ID, AttributeValueTypes.avtEnumerationMulti, IIf(String.IsNullOrEmpty(cat_ids), Nothing, cat_ids), alt_id, Guid.Empty) Then
                            PM.Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, PM.StorageManager.ProjectLocation, PM.StorageManager.ProviderType, PM.StorageManager.ModelID, UNDEFINED_USER_ID)
                        End If
                    End If

                Case "reset_funded_before" 'A0905
                    FundedBefore.Clear()
                    BeforeScenarioName = ""
                    ShowChanges = False

                    ' D3146 ===
                Case "bubble"
                    Dim sVal As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("coeff", "")) 'Anti-XSS
                    Dim tVal As Double
                    If String2Double(sVal, tVal) Then BubbleCoeff = tVal

                Case "axis_reset"
                    Dim CID As Integer = CheckVar("mode", ChartID)
                    SessVar(COOKIE_AREA(CID)) = ":"
                    SetCookie(COOKIE_AREA(CID), "")

                Case "axis"
                    Dim CID As Integer = CheckVar("mode", ChartID)
                    Dim sVals As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("area", "")).Trim    'Anti-XSS
                    If sVals <> "" Then
                        Dim Params As String() = sVals.Split(CChar(";"))
                        If Params.Count = 4 Then
                            Dim CanSave As Boolean = True
                            Dim Vals As Double() = {Double.MinValue, Double.MinValue, Double.MinValue, Double.MinValue}
                            For i As Integer = 0 To 3
                                If Not String2Double(Params(i), Vals(i)) Then CanSave = False
                            Next
                            If CanSave AndAlso CheckVar("mode", "") = CID.ToString Then
                                ViewArea(CID) = Vals
                            End If
                        End If
                    End If
                    ' D3146 ==

            End Select
            'ChartID = CheckVar("chart", 0)
            tResult = GetData(ChartID, True)
        End If

        'If tSuccess Then RA.Save()

        If tResult <> "" Then
            Response.Clear()
            Response.ContentType = "text/plain"
            Response.Write(tResult)
            Response.End()
        End If
    End Sub
    'A0888 ==

    ' D3101 ===
    Protected Sub ntbXMin_Init(sender As Object, e As EventArgs)
        Dim IDs() As String = {"ntbXMin", "ntbXMax", "ntbYMin", "ntbYMax"}
        For i As Integer = 0 To IDs.Length - 1
            If IDs(i) = CType(sender, RadNumericTextBox).ID Then
                SpinIDs(i) = CType(sender, RadNumericTextBox).ClientID
            End If
        Next
    End Sub
    ' D3101 ==

End Class