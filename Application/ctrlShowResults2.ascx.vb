Imports Telerik.Web.UI  ' D0535
Imports System.Collections.Generic 'C0384
Imports Canvas.CanvasTypes      ' D0937

Namespace ExpertChoice.Web.Controls

    <Serializable()> Partial Public Class ctrlShowResults2

#Const TREE_PRTY_SMART_VALUE = True      ' D1606

        Inherits UserControl

        <Serializable()> Public Class clsShowResultsItem
            Private mPosition As Integer  ' D0604
            Private mName As String
            Private mLikelihood As Single   ' D2691
            Private mValueMy As Single
            Private mValueCombined As Single

            Public mNodeID As Integer       ' D2112

            ' D0604 ===
            Public Property Position() As Integer
                Get
                    Return mPosition
                End Get
                Set(value As Integer)
                    mPosition = value
                End Set
            End Property
            ' D0604 ==

            Public Property Name() As String
                Get
                    Return mName
                End Get
                Set(value As String)
                    mName = value
                End Set
            End Property

            ' D2691 ===
            Public Property KnownLikelihood() As Single
                Get
                    Return mLikelihood
                End Get
                Set(value As Single)
                    mLikelihood = value
                End Set
            End Property
            ' D2691 ==

            Public Property ValueMy() As Single
                Get
                    Return mValueMy
                End Get
                Set(value As Single)
                    mValueMy = value
                End Set
            End Property

            Public Property ValueCombined() As Single
                Get
                    Return mValueCombined
                End Get
                Set(value As Single)
                    mValueCombined = value
                End Set
            End Property

        End Class

        ' D0123 ===
        <Serializable()> Public Class clsShowResultsItemComparer
            Implements IComparer

            Private _SortField As ResultsSortMode = ResultsSortMode.rsmNumber   ' D3004 ResultsSortMode.rsmPriority ' D0937 ShowResultsSortField = ShowResultsSortField.IndividualResults
            Private _Direction As SortDirection = SortDirection.Ascending   ' D3004  SortDirection.Descending

            Public Sub New(tSortField As ResultsSortMode, tDirection As SortDirection)  ' D0937
                _SortField = tSortField
                _Direction = tDirection
            End Sub

            Public Function Compare(x As Object, y As Object) As Integer Implements IComparer.Compare
                Dim A As clsShowResultsItem = CType(x, clsShowResultsItem)
                Dim B As clsShowResultsItem = CType(y, clsShowResultsItem)
                Dim Res As Integer = 0
                ' D0937 ===
                Select Case _SortField
                    ' D0604 ===
                    Case ResultsSortMode.rsmNumber
                        Res = CInt(IIf(A.Position = B.Position, 0, IIf(A.Position > B.Position, 1, -1)))
                    Case ResultsSortMode.rsmName
                        Res = String.Compare(A.Name, B.Name)
                    Case ResultsSortMode.rsmPriority
                        Res = CInt(IIf(A.ValueMy = B.ValueMy, 0, IIf(A.ValueMy > B.ValueMy, 1, -1)))
                    Case ResultsSortMode.rsmCombined
                        Res = CInt(IIf(A.ValueCombined = B.ValueCombined, 0, IIf(A.ValueCombined > B.ValueCombined, 1, -1)))
                End Select
                ' D0937 ==
                If _Direction = SortDirection.Descending AndAlso Res <> 0 Then Return -Res Else Return Res
            End Function
        End Class
        ' D0123 ==


        Private _Data As Object
        Private _UserID As Integer
        Private _UserID4Tree As Integer     ' D1606

        ' D0123 ===
        Private _GridData As ArrayList
        Private _MaxValue As Single = 0

        ' D0394 ===
        Public _SESS_NID As String = "ShowResults_NID"
        Public _SESS_SORT As String = "ShowResults_Sort"
        Public _SESS_DIR As String = "ShowResults_Dir"
        Public _SESS_NORM_MODE As String = "ShowResults_NormMode"  ' D0631
        Const _COOKIE_NORM_MODE As String = "ResNormMode"

        Private _nodeID As Integer = -1
        Private _SortField As ResultsSortMode = ResultsSortMode.rsmNumber      ' D0937 + D3032
        Private _SortDir As SortDirection = SortDirection.Ascending ' D3032
        ' D0394 ==

        Private _precInconsist As Integer = 2
        ' D0123 ==
        Private _precValue As Integer = 2

        Private _valueMultiplier As Single = 100
        Private _valueTemplate As String = "{0}%"
        ' D0123 ===
        Private _InconsistTemplateMy As String = "Inconsistency Ratio: {0}"
        Private _InconsistTemplateCombine As String = "Inconsistency Ratio: {0}"
        Private _sortAscTemplate As String = _SORT_ASC      ' D0162
        Private _sortDescTemplate As String = _SORT_DESC    ' D0162
        ' D0123 ==

        Private _sPosition As String = "No."    ' D0604
        Private _sName As String = "Name"
        Private _sValueMy As String = "Priority (My)"
        Private _sValueCombined As String = "Priority (Combined)"
        Private _sGraph As String = "Graph Bar"
        Public columnLikelihood As String = "Known Likelihood"


        ' D0123 ===
        Private _sNoMyResults As String = "No Individual Results"
        Private _sNoCombinedResults As String = "No Combined Results"
        Private _sNoAnyResults As String = "No Any Results"

        Private _fShowInconsit As Boolean = False
        Private _fShowGraphForMax As Boolean = False
        Private _fAllowSorting As Boolean = True
        Private _fHideCombined4OneUser As Boolean = True    ' D0346
        ' D0123 ==


        Public TreeBarWidth As Integer = 50    ' D2518 + D3874
        Private _barWidth As Integer = 150
        Private _barHeight As Integer = 10
        Private _barImageBlankPath As String = "images/blank.gif"   ' D0136

        Private _minWidth As Integer = 600 ' D0136

        Private _isMyVisisble As Boolean = False
        Private _isCombinedVisible As Boolean = False

        Private _showTree As Boolean = False                ' D0193
        Private _Hierarchy As clsHierarchy = Nothing        ' D0193 + D0394
        Private _CurrentNode As clsNode = Nothing           ' D0193 + D0394

        Private _ShowMaxAlternatives As Integer = 0         ' D0349
        'Private _ShowRefreshButton As Boolean = False       ' D0356 -D2608

        Private Const colPosition As Integer = 0            ' D0604
        Private Const colName As Integer = 1
        Private Const colKnownLikelihood As Integer = 2     ' D2692
        Private Const colValueMy As Integer = 3
        Private Const colValueCombine As Integer = 4
        Private Const colGraph As Integer = 5

        ' D0152 ===
        Public StyleGraphMy As String = "graph_my"
        Public StyleGraphCombined As String = "graph_combined"
        Public StyleLabelMy As String = "label_my"
        Public StyleLabelCombined As String = "label_combined"
        ' D0152 ==

        Public lblExpectedValueIndividual As String = "Expected value, individual:" ' D2130
        Public lblExpectedValueCombined As String = "Expected value, combined:"     ' D2130

        Public ShowOverallExpectedValueMode As ResultsView = ResultsView.rvNone     ' D3691

        Private _NormalizationMode As AlternativeNormalizationOptions = AlternativeNormalizationOptions.anoUnnormalized     ' D0360 + D1761

        Public Show_Normalization As Boolean = False    ' D1761 + D7238
        Public Show_Bars As Boolean = True              ' D7561

        Public ShowTreePriorities As Boolean = True     ' D0202

        Private _isCISEnabled As Boolean = True         ' D1456
        Private _messageTurnCIS As String = ""          ' D1456

        Private _msg_Custom As String = ""              ' D1506
        Private _btnReturnURL As String = ""            ' D1506
        Private _btnReturnCaption As String = ""        ' D1506

        Public NormalizationCaptions() As String = {"Priority", "% of maximum", "Multiple of minimum", "Unnormalized"}
        Private _NormalizationModeCaption As String = "Show results as" ' D0631

        Public ShowInfodocs As Boolean = False          ' D2112
        Public ImgPath As String = ""                   ' D2112
        Public InfodocURL As String = ""                ' D2112
        Public CanEditInfodocs As Boolean = False       ' D3538

        ' D3538 ===
        Public imageInfodoc As String = "info12.png"
        Public imageInfodocEmpty As String = "info12_dis.png"
        Public imageWRTInfodoc As String = "readme.gif"
        Public imageWRTInfodocEmpty As String = "readme_dis.gif"
        ' D3538 ==

        Public ShowAltsIdx As Boolean = True    ' D3786

        Public CombinedUserID As Integer = COMBINED_USER_ID     ' D4375

        ' D0123 ===
        Public Sub New()
            _Data = Nothing
            _UserID = -1
            _GridData = Nothing
        End Sub
        ' D0123 ==

        ' D0604 ===
        Public Property columnPosition() As String
            Get
                Return _sPosition
            End Get
            Set(value As String)
                _sPosition = value
            End Set
        End Property
        ' D0604 ==

        Public Property columnName() As String
            Get
                Return _sName
            End Get
            Set(value As String)
                _sName = value
            End Set
        End Property

        Public Property columnValueMy() As String
            Get
                Return _sValueMy
            End Get
            Set(value As String)
                _sValueMy = value
            End Set
        End Property

        Public Property columnValueCombined() As String
            Get
                Return _sValueCombined
            End Get
            Set(value As String)
                _sValueCombined = value
            End Set
        End Property

        Public Property columnGraph() As String
            Get
                Return _sGraph
            End Get
            Set(value As String)
                _sGraph = value
            End Set
        End Property

        ' D0123 ===
        Public Property messageNoIndividualResults() As String
            Get
                Return _sNoMyResults
            End Get
            Set(value As String)
                _sNoMyResults = value
            End Set
        End Property

        Public Property messageNoCombinedResults() As String
            Get
                Return _sNoCombinedResults
            End Get
            Set(value As String)
                _sNoCombinedResults = value
            End Set
        End Property

        Public Property messageNoAnyResults() As String
            Get
                Return _sNoAnyResults
            End Get
            Set(value As String)
                _sNoAnyResults = value
            End Set
        End Property

        ' D1506 ===
        Public Property MessageCustom As String
            Get
                Return _msg_Custom
            End Get
            Set(value As String)
                _msg_Custom = value
            End Set
        End Property

        Public Property ButtonReturnURL As String
            Get
                Return _btnReturnURL
            End Get
            Set(value As String)
                _btnReturnURL = value
            End Set
        End Property

        Public Property ButtonReturnCaption As String
            Get
                Return _btnReturnCaption
            End Get
            Set(value As String)
                _btnReturnCaption = value
            End Set
        End Property
        ' D1506 ==

        ' D0394 ===
        ' -D2610 ===
        'Public Sub SetCookie(ByVal sName As String, ByVal sValue As String)
        '    Dim cookie As New HttpCookie(sName, sValue)
        '    If Request.Url.Port = 443 Then cookie.Secure = True
        '    Response.Cookies.Add(cookie)
        'End Sub

        'Public Function GetCookie(ByVal sName As String, Optional ByVal sDefValue As String = "") As String
        '    If (Request.Cookies(sName) Is Nothing) Then
        '        Return sDefValue
        '    Else
        '        Return Request.Cookies(sName).Value
        '    End If
        'End Function
        ' -D2610 ==

        Private ReadOnly Property ModelID() As Integer
            Get
                If Not Hierarchy Is Nothing Then Return Hierarchy.ProjectManager.StorageManager.ModelID
                Return 0
            End Get
        End Property

        Public Property ActiveNodeID() As String
            Get
                Return _nodeID.ToString
            End Get
            Set(value As String)
                If _nodeID.ToString <> value Then   ' D3388
                    _nodeID = CInt(value)
                    Session(_SESS_NID + ModelID.ToString) = _nodeID ' D2610
                End If
            End Set
        End Property

        Public Property SortExpression() As ResultsSortMode     ' D0937
            Get
                Return _SortField
            End Get
            Set(value As ResultsSortMode) ' D0937
                If _SortField <> value Then
                    '_SortDir = IIf(value = ResultsSortMode.rsmName Or value = ResultsSortMode.rsmNumber, SortDirection.Ascending, SortDirection.Descending) ' D1017
                    _SortField = value
                    Session(_SESS_SORT) = _SortField    ' D2610
                End If
            End Set
        End Property
        ' D0394 ==

        ' -D2610 ===
        'Public Function SortExpressionToString(ByVal Expression As ResultsSortMode) As String   ' D0937
        '    Select Case Expression
        '        ' D0937 ===
        '        Case ResultsSortMode.rsmNumber
        '            Return "position"
        '        Case ResultsSortMode.rsmName
        '            Return "name"
        '        Case ResultsSortMode.rsmPriority
        '            Return "my"
        '        Case ResultsSortMode.rsmCombined
        '            Return "combined"
        '            ' D0937 ==
        '        Case Else
        '            Return ""
        '    End Select
        'End Function

        'Public Function StringToSortExpression(ByVal Expression As String) As ResultsSortMode ' D0937
        '    Select Case Expression
        '        Case "name"
        '            Return ResultsSortMode.rsmName  ' D0937
        '        Case "my"
        '            Return ResultsSortMode.rsmPriority  ' D0937
        '        Case "combined"
        '            Return ResultsSortMode.rsmCombined  ' D0937
        '        Case Else
        '            _SortDir = WebControls.SortDirection.Ascending  ' D1017
        '            Return ResultsSortMode.rsmNumber    ' D0937
        '    End Select
        'End Function
        ' -D2610 ==

        ' D0394 ===
        Public Property SortDirection() As SortDirection
            Get
                Return _SortDir
            End Get
            Set(value As SortDirection)
                If _SortDir <> value Then
                    _SortDir = value
                    Session(_SESS_DIR) = _SortDir  ' D2610
                End If
            End Set
        End Property
        ' D0394 ==

        Public Property AllowSorting() As Boolean
            Get
                Return _fAllowSorting
            End Get
            Set(value As Boolean)
                _fAllowSorting = value
                If Not _fAllowSorting Then
                    SortExpression = ResultsSortMode.rsmNumber ' D0937
                    SortDirection = SortDirection.Ascending ' D1017
                End If
            End Set
        End Property
        ' D0123 ==

        ' D4375 ===
        Private Function CanShowCombinedResults(fGlobal As Boolean) As Boolean
            If CombinedUserID = COMBINED_USER_ID Then
                If fGlobal Then Return GlobalResults IsNot Nothing AndAlso GlobalResults.CanShowGroupResults Else Return LocalResults IsNot Nothing AndAlso LocalResults.CanShowGroupResults()
            Else
                If fGlobal Then Return GlobalResults IsNot Nothing AndAlso GlobalResults.CanShowIndividualResults(CombinedUserID) Else Return LocalResults IsNot Nothing AndAlso LocalResults.CanShowResultsForUser(CombinedUserID)
            End If
            Return False
        End Function
        ' D4375 ==

        Public Property Data() As Object
            Get
                Return _Data
            End Get
            Set(value As Object)
                _Data = value
#If TREE_PRTY_SMART_VALUE Then
                ' D2033 ===
                '_UserID4Tree = CInt(IIf(_Data IsNot Nothing AndAlso isGlobalResults AndAlso GlobalResults.CanShowGroupResults AndAlso (GlobalResults.ResultsViewMode = ResultsView.rvGroup OrElse (GlobalResults.ResultsViewMode = ResultsView.rvBoth AndAlso Not GlobalResults.CanShowIndividualResults(UserID))), COMBINED_USER_ID, UserID))
                If _Data IsNot Nothing Then
                    If isGlobalResults AndAlso
                      (GlobalResults.ResultsViewMode = ResultsView.rvGroup OrElse
                      (GlobalResults.ResultsViewMode = ResultsView.rvBoth AndAlso Not GlobalResults.CanShowIndividualResults(UserID))) _
                      AndAlso CanShowCombinedResults(True) Then ' D4374
                        _UserID4Tree = CombinedUserID   ' D4374
                    Else
                        _UserID4Tree = UserID
                    End If
                End If
                ' D2033 ==
#Else
                Return _UserID
#End If
            End Set
        End Property

        Public Property ValuePrecision() As Integer ' D0123
            Get
                Return _precValue
            End Get
            Set(value As Integer)
                _precValue = value
            End Set
        End Property

        ' D0123 ===
        Public Property InconsistencyPrecision() As Integer
            Get
                Return _precInconsist
            End Get
            Set(value As Integer)
                _precInconsist = value
            End Set
        End Property

        Public Property ShowInconsistencyRatio() As Boolean
            Get
                Return _fShowInconsit
            End Get
            Set(value As Boolean)
                _fShowInconsit = value
            End Set
        End Property

        Public Property InconsistencyRatioMyTemplate() As String    ' D1223
            Get
                Return _InconsistTemplateMy
            End Get
            Set(value As String)
                _InconsistTemplateMy = value
            End Set
        End Property

        ' D1223 ===
        Public Property InconsistencyRatioCombinedTemplate() As String
            Get
                Return _InconsistTemplateCombine
            End Get
            Set(value As String)
                _InconsistTemplateCombine = value
            End Set
        End Property
        ' D1223 ==

        Public Property ValueTemplate() As String
            Get
                Return _valueTemplate
            End Get
            Set(value As String)
                _valueTemplate = value
            End Set
        End Property

        Public Property ValueMultiplier() As Single
            Get
                Return _valueMultiplier
            End Get
            Set(value As Single)
                _valueMultiplier = value
            End Set
        End Property

        ' -D2608
        '' D0356 ===
        'Public Property ShowRefreshButton() As Boolean
        '    Get
        '        Return imgBtnRefresh.Visible
        '    End Get
        '    Set(ByVal value As Boolean)
        '        imgBtnRefresh.Visible = value
        '    End Set
        'End Property
        '' D0356 ==

        ' D0193 ===
        Public Property ShowTree() As Boolean
            Get
                Return _showTree
            End Get
            Set(value As Boolean)
                _showTree = value
            End Set
        End Property
        ' D0193 ==

        ' D0349 ===
        Public Property ShowMaxAlternatives() As Integer
            Get
                Return _ShowMaxAlternatives
            End Get
            Set(value As Integer)
                _ShowMaxAlternatives = value
            End Set
        End Property
        ' D0349 ==

        ' D0136 ===
        Public Property GridMinWidth() As Integer
            Get
                Return _minWidth
            End Get
            Set(value As Integer)
                _minWidth = value
            End Set
        End Property

        Public Property BlankImagePath() As String
            Get
                Return _barImageBlankPath
            End Get
            Set(value As String)
                _barImageBlankPath = value
            End Set
        End Property
        ' D0136 ==

        Public Property GraphWidth() As Integer
            Get
                Return _barWidth
            End Get
            Set(value As Integer)
                _barWidth = value
            End Set
        End Property

        Public Property GraphHeight() As Integer
            Get
                Return _barHeight
            End Get
            Set(value As Integer)
                _barHeight = value
            End Set
        End Property

        ' D0123 ===
        Public Property ShowGraphForMax() As Boolean
            Get
                Return _fShowGraphForMax
            End Get
            Set(value As Boolean)
                _fShowGraphForMax = value
            End Set
        End Property
        ' D0123 ==

        ' D0360 ===
        Public Property NormalizationMode() As AlternativeNormalizationOptions
            Get
                Return _NormalizationMode
            End Get
            Set(value As AlternativeNormalizationOptions)
                If _NormalizationMode <> value Then ' D3532
                    _NormalizationMode = value
                    Session(_SESS_NORM_MODE) = _NormalizationMode   ' D2610
                    Response.Cookies.Remove(_COOKIE_NORM_MODE)  ' D3531
                    Response.Cookies.Add(New HttpCookie(_COOKIE_NORM_MODE, CInt(value).ToString))    ' D3531
                End If
            End Set
        End Property

        Public Property NormalizationModeCaption() As String
            Get
                Return _NormalizationModeCaption
            End Get
            Set(value As String)
                _NormalizationModeCaption = value
            End Set
        End Property
        ' D0360 ==

        ' D0346 ===
        Public Property HideCombinedResults4OneUser() As Boolean
            Get
                Return _fHideCombined4OneUser
            End Get
            Set(value As Boolean)
                _fHideCombined4OneUser = value
            End Set
        End Property
        ' D0346 ==

        Public Property UserID() As Integer
            Get
                Return _UserID
            End Get
            Set(value As Integer)
                _UserID = value
            End Set
        End Property

        ' D1907 ===
        Public ReadOnly Property UserID4Tree() As Integer
            Get
                Return _UserID4Tree
            End Get
        End Property
        ' D1907 ==

        ' D0394 ===
        Public ReadOnly Property isGlobalResults() As Boolean
            Get
                If Data Is Nothing Then Return False
                Return TypeOf (Data) Is clsShowGlobalResultsActionData 'C0464
            End Get
        End Property

        Public ReadOnly Property isLocalResults() As Boolean
            Get
                If Data Is Nothing Then Return False
                Return TypeOf (Data) Is clsShowLocalResultsActionData 'C0464
            End Get
        End Property

        Public ReadOnly Property GlobalResults() As clsShowGlobalResultsActionData 'C0464v
            Get
                If Not Data Is Nothing Then
                    Return TryCast(Data, clsShowGlobalResultsActionData) 'C0464
                End If
                Return Nothing
            End Get
        End Property

        Public ReadOnly Property LocalResults() As clsShowLocalResultsActionData 'C0464
            Get
                If Not Data Is Nothing Then
                    Return TryCast(Data, clsShowLocalResultsActionData) 'C0464
                End If
                Return Nothing
            End Get
        End Property

        ' D2691 ===
        Private Function HasKnownLikelihood() As Boolean
            Return Data IsNot Nothing AndAlso isLocalResults AndAlso LocalResults.ParentNode IsNot Nothing AndAlso LocalResults.ParentNode.MeasureType = ECMeasureType.mtPWAnalogous
        End Function
        ' D2691 ==

        ' D1456 ===
        Public Property isCISEnabled As Boolean
            Get
                Return _isCISEnabled
            End Get
            Set(value As Boolean)
                _isCISEnabled = value
            End Set
        End Property

        Public Property messageTurnCIS As String
            Get
                Return _messageTurnCIS
            End Get
            Set(value As String)
                _messageTurnCIS = value
            End Set
        End Property
        ' D1456 ==

        Public Property Hierarchy() As clsHierarchy
            Get
                If _Hierarchy Is Nothing And Not Data Is Nothing Then
                    If isGlobalResults Then
                        _Hierarchy = GlobalResults.ProjectManager.Hierarchy(GlobalResults.ProjectManager.ActiveHierarchy)
                    Else
                        If Not LocalResults.ParentNode Is Nothing Then _Hierarchy = LocalResults.ParentNode.Hierarchy
                    End If
                End If
                Return _Hierarchy
            End Get
            Set(value As clsHierarchy)
                _Hierarchy = value
            End Set
        End Property

        Public Property CurrentNode() As clsNode
            Get
                If _CurrentNode Is Nothing AndAlso Not Hierarchy Is Nothing Then

                    Dim NID As Integer
                    If Integer.TryParse(ActiveNodeID, NID) Then _CurrentNode = Hierarchy.GetNodeByID(NID)

                    If _CurrentNode Is Nothing Then

                        If isGlobalResults Then
                            If GlobalResults.WRTNode Is Nothing Then
                                If Hierarchy.Nodes.Count > 0 Then _CurrentNode = Hierarchy.Nodes(0)
                            Else
                                _CurrentNode = GlobalResults.WRTNode
                            End If

                        Else
                            _CurrentNode = LocalResults.ParentNode
                        End If
                    End If

                End If
                Return _CurrentNode
            End Get
            Set(value As clsNode)
                _CurrentNode = value
                If value Is Nothing Then ActiveNodeID = "" Else ActiveNodeID = CurrentNode.NodeID.ToString
            End Set
        End Property
        ' D0394 ==

        Private Sub FillData(ByRef Rows As ArrayList, UserResults As ArrayList, isCombined As Boolean)
            If Not UserResults Is Nothing Then

                Dim fIsNew As Boolean = Rows.Count = 0 Or Rows.Count <> UserResults.Count
                Dim L As New List(Of KnownLikelihoodDataContract)   ' D2691
                If isLocalResults AndAlso HasKnownLikelihood() Then L = LocalResults.ParentNode.GetKnownLikelihoods() ' D2691

                ' D3518 ===
                Dim Coeff As Double = 0
                If isLocalResults AndAlso NormalizationMode = AlternativeNormalizationOptions.anoPercentOfMax Then
                    For i As Integer = 0 To UserResults.Count - 1
                        Dim tUserRow As clsResultsItem = CType(UserResults.Item(i), clsResultsItem)
                        If tUserRow.Value > Coeff Then Coeff = tUserRow.Value
                    Next
                End If
                If Coeff <= 0 Then Coeff = 1
                ' D3518 ==

                For i As Integer = 0 To UserResults.Count - 1
                    Dim tUserRow As clsResultsItem = CType(UserResults.Item(i), clsResultsItem)
                    Dim tDataRow As clsShowResultsItem
                    If fIsNew Then
                        tDataRow = New clsShowResultsItem
                        tDataRow.Name = tUserRow.Name
                        tDataRow.mNodeID = tUserRow.ObjectID    ' D2112
                    Else
                        tDataRow = CType(Rows.Item(i), clsShowResultsItem)
                    End If
                    ' D2691 ===
                    If L IsNot Nothing Then
                        For Each tLikelihood As KnownLikelihoodDataContract In L
                            If tLikelihood.ID = tUserRow.ObjectID Then
                                tDataRow.KnownLikelihood = CSng(tLikelihood.Value)
                                Exit For
                            End If
                        Next
                    End If
                    ' D2691 ==
                    tDataRow.Position = i + 1     ' D0604
                    Dim tVal As Single = CSng(If(NormalizationMode = AlternativeNormalizationOptions.anoUnnormalized, tUserRow.UnnormalizedValue, tUserRow.Value / Coeff))   ' D1592 + D3518
                    If isCombined Then tDataRow.ValueCombined = tVal Else tDataRow.ValueMy = tVal ' D1592
                    If ShowGraphForMax And tVal > _MaxValue Then _MaxValue = tVal ' D0123 + D1592
                    If fIsNew Then Rows.Add(tDataRow) Else Rows.Item(i) = tDataRow
                Next
            End If
        End Sub

        ' D0394 ===
        Protected Sub GridResults_PreRender(sender As Object, e As EventArgs) Handles GridResults.PreRender
            GridInit()
        End Sub
        ' D0394 ==

        Protected Sub GridResults_RowDataBound(sender As Object, e As GridViewRowEventArgs) Handles GridResults.RowDataBound
            Dim tRow As clsShowResultsItem = TryCast(e.Row.DataItem, clsShowResultsItem)
            If Not tRow Is Nothing Then
                e.Row.Cells(colGraph).Text = ""
                e.Row.Cells(colName).Text = CreateNodeInfodoc(tRow.mNodeID.ToString, reObjectType.Alternative) + JS_SafeHTML(tRow.Name) + CreateNodeInfodoc(tRow.mNodeID.ToString, reObjectType.AltWRTNode)  ' D0123 + D2112
                If _isMyVisisble Then
                    e.Row.Cells(colValueMy).Text = String.Format("<span class='{0}'>{1}</span >", StyleLabelMy, String.Format(ValueTemplate, Double2String(ValueMultiplier * tRow.ValueMy, ValuePrecision, False)))   ' D0152 + D0189 + D3082
                    e.Row.Cells(colGraph).Text += HTMLCreateGraphBar(tRow.ValueMy, _MaxValue, GraphWidth, GraphHeight, StyleGraphMy, BlankImagePath, CSng(100 * tRow.ValueMy).ToString("F2") + "%")  ' D0123 + D0136 + D0152 + D0323
                End If
                If _isCombinedVisible Then
                    e.Row.Cells(colValueCombine).Text = String.Format("<span class='{0}'>{1}</span >", StyleLabelCombined, String.Format(ValueTemplate, Double2String(ValueMultiplier * tRow.ValueCombined, ValuePrecision, False)))  ' D0152 + D0189 + D3082
                    e.Row.Cells(colGraph).Text += HTMLCreateGraphBar(tRow.ValueCombined, _MaxValue, GraphWidth, GraphHeight, StyleGraphCombined, BlankImagePath, CSng(100 * tRow.ValueCombined).ToString("F2") + "%")  ' D0123 + D0136 + D0152 + D0323
                End If
                If ShowMaxAlternatives > 0 And ShowMaxAlternatives <= e.Row.RowIndex Then e.Row.Visible = False ' D0349
                ' D2691 ===
                If e.Row.Cells(colKnownLikelihood).Visible Then
                    If tRow.KnownLikelihood > 0 Then e.Row.Cells(colKnownLikelihood).Text = Double2String(100 * tRow.KnownLikelihood, ValuePrecision, True) Else e.Row.Cells(colKnownLikelihood).Text = "&nbsp;" ' D3082
                End If
                ' D2691 ==
                e.Row.Cells(colGraph).Visible = Show_Bars   ' D7561
            End If
            ' D2610 ===
            If AllowSorting AndAlso e.Row.RowType = DataControlRowType.Header Then
                Dim idx As Integer = 0
                For Each tCell As DataControlFieldHeaderCell In e.Row.Cells
                    ' D3388 ===
                    If idx = colGraph Then tCell.Visible = Show_Bars        ' D7561
                    If tCell.Visible Then
                        Dim fld As Integer
                        Select Case idx
                            Case colPosition
                                fld = CInt(ResultsSortMode.rsmNumber)
                            Case colName
                                fld = CInt(ResultsSortMode.rsmName)
                            Case colValueMy
                                fld = CInt(ResultsSortMode.rsmPriority)
                            Case colValueCombine
                                fld = CInt(ResultsSortMode.rsmCombined)
                            Case Else
                                fld = -1
                        End Select
                        If fld <> -1 Then
                            Dim sSort As String = ""
                            fld += 1
                            If CInt(SortExpression) + 1 = fld Then
                                If SortDirection = SortDirection.Ascending Then fld = -fld
                                sSort = CStr(IIf(SortDirection = SortDirection.Ascending, _sortAscTemplate, _sortDescTemplate))
                            End If
                            tCell.Text = String.Format("<a href='' onclick='onSort(""{0}""); return false;' class='actions'>{1}</a>{2}", fld, tCell.Text, sSort)
                        End If
                    End If
                    idx += 1
                    ' D3388 ==
                Next
            End If
            ' D2610 ==
        End Sub

        Private Sub TreeInit()
            If ShowTree And Hierarchy Is Nothing Then ShowTree = False
            If Hierarchy Is Nothing Then Exit Sub ' D0394
            If TypeOf (Data) Is clsShowLocalResultsActionData Then ShowTree = False ' D0201 'C0464

            ' D0306 ===
            If ShowTree And Not Hierarchy Is Nothing Then
                Dim _AltsHierarchy As clsHierarchy = Hierarchy.ProjectManager.AltsHierarchy(Hierarchy.ProjectManager.ActiveAltsHierarchy)
                If Not _AltsHierarchy Is Nothing Then
                    Dim fGridVisible As Boolean = _AltsHierarchy.Nodes.Count > 0
                    radPanelGrid.Visible = fGridVisible
                    RadSplitbarLeft.Visible = fGridVisible
                    'If fGridVisible Then AddHandler RadTreeViewHierarchy.NodeClick, AddressOf RadTreeViewHierarchy_NodeClick ' D0535 -D1856
                    ' RadTreeViewHierarchy.AutoPostBack = fGridVisible  ' -D0535
                End If
                RadTreeViewHierarchy.Nodes.Clear()
                Dim ct As New clsCalculationTarget(If(IsCombinedUserID(UserID4Tree), CalculationTargetType.cttCombinedGroup, CalculationTargetType.cttUser), IIf(IsCombinedUserID(UserID4Tree), Hierarchy.ProjectManager.CombinedGroups.GetCombinedGroupByUserID(UserID4Tree), Hierarchy.ProjectManager.GetUserByID(UserID4Tree)))
                Hierarchy.ProjectManager.CalculationsManager.Calculate(ct, Hierarchy.Nodes(0))
                AddNodesToRadTree(Hierarchy.GetLevelNodes(0), RadTreeViewHierarchy.Nodes, False, 0)    ' D0125 + D0394
            End If
            ' D0306 ==

            ' D2664 ===
            'If ShowTree AndAlso TypeOf (Data) Is clsShowGlobalResultsActionData AndAlso GlobalResults IsNot Nothing _
            '   AndAlso GlobalResults.ProjectManager.IsRiskProject AndAlso RadTreeViewHierarchy.GetAllNodes.Count < 2 Then ShowTree = False ' D2669
            If ShowTree AndAlso TypeOf (Data) Is clsShowGlobalResultsActionData AndAlso GlobalResults IsNot Nothing AndAlso RadTreeViewHierarchy.GetAllNodes.Count < 2 Then ShowTree = False ' D2669 + D4375

            radPanelTree.Visible = ShowTree
            RadSplitbarLeft.Visible = ShowTree AndAlso radPanelGrid.Visible
            ' D2664 ==
        End Sub

        Private Sub GridInit()
            'GridResults.AllowSorting = AllowSorting    ' D2610
            If Not Data Is Nothing Then
                ' D0123 ===
                If _GridData Is Nothing Then
                    _GridData = New ArrayList
                    Dim fCanShowCombinedResults As Boolean = False
                    Dim fCanShowMyResults As Boolean = False
                    lblInconsistency.Visible = False
                    lblInconsistency.Text = ""  ' D2130
                    lblMessage.Visible = False
                    lblMessage.Text = ""

                    GridResults.Columns(colPosition).Visible = ShowAltsIdx  ' D3786
                    If Not ShowAltsIdx AndAlso SortExpression = ResultsSortMode.rsmNumber Then SortExpression = ResultsSortMode.rsmName ' D3786

                    GridResults.Columns(colPosition).Visible = ShowAltsIdx  ' D3786
                    If Not ShowAltsIdx AndAlso SortExpression = ResultsSortMode.rsmNumber Then SortExpression = ResultsSortMode.rsmName ' D3786

                    ' D7238 ===
                    If Show_Normalization Then
                        If ddResultsMode IsNot Nothing AndAlso ddResultsMode.SelectedItem IsNot Nothing Then NormalizationMode = CType(ddResultsMode.SelectedValue, AlternativeNormalizationOptions) ' D0631
                    Else
                        ddResultsMode.Visible = False
                    End If
                    ' D0123 + D7238 ==

                    If isGlobalResults Then ' D0394
                        GridResults.Columns(colKnownLikelihood).Visible = False ' D2691

                        If Not GlobalResults.WRTNode Is CurrentNode Then GlobalResults.WRTNode = CurrentNode ' D0394

                        _isMyVisisble = GlobalResults.ResultsViewMode = ResultsView.rvBoth Or GlobalResults.ResultsViewMode = ResultsView.rvIndividual  ' D0394
                        fCanShowMyResults = True
                        If UserID = CombinedUserID Then _isMyVisisble = False ' D0400 + D4375
                        GridResults.Columns(colValueMy).Visible = _isMyVisisble
                        If UserID <> CombinedUserID And _isMyVisisble Then 'C0117 + D4375

                            'If GlobalResults.CanShowIndividualResults(UserID) Then 'C0565
                            If GlobalResults.CanShowIndividualResults(UserID, GlobalResults.WRTNode) Then 'C0565
                                FillData(_GridData, GlobalResults.ResultsList(UserID, UserID, NormalizationMode), False) 'C0092 + D0631
                            Else
                                fCanShowMyResults = False
                                _isMyVisisble = False   ' D0123
                            End If
                        End If

                        _isCombinedVisible = GlobalResults.ResultsViewMode = ResultsView.rvBoth Or GlobalResults.ResultsViewMode = ResultsView.rvGroup
                        If GlobalResults.ResultsViewMode = ResultsView.rvBoth And HideCombinedResults4OneUser And GlobalResults.ProjectManager.UsersList.Count <= 1 And UserID <> CombinedUserID Then _isCombinedVisible = False ' D0345 + D0407 + D4375
                        fCanShowCombinedResults = True
                        If _isCombinedVisible Then
                            ' D4520 ===
                            If CombinedUserID <> COMBINED_USER_ID Then
                                If Not GlobalResults.ProjectManager.StorageManager.Reader.StoredUserJudgmentsUpToDate(GlobalResults.ProjectManager.GetUserByID(CombinedUserID)) Then
                                    GlobalResults.ProjectManager.StorageManager.Reader.LoadUserJudgments(GlobalResults.ProjectManager.GetUserByID(CombinedUserID))
                                End If
                            End If
                            ' D4520 ==
                            If CanShowCombinedResults(True) Then    ' D4375
                                FillData(_GridData, GlobalResults.ResultsList(CombinedUserID, UserID, NormalizationMode), True) 'C0092 + D0631 + D4375
                            Else
                                fCanShowCombinedResults = False
                                _isCombinedVisible = False  ' D0123
                            End If
                        End If
                        GridResults.Columns(colValueCombine).Visible = _isCombinedVisible

                    End If ' Global

                    ' D0123 ===
                    If isLocalResults Then  ' D0394

                        _isMyVisisble = LocalResults.ResultsViewMode = ResultsView.rvBoth Or LocalResults.ResultsViewMode = ResultsView.rvIndividual
                        fCanShowMyResults = True
                        GridResults.Columns(colKnownLikelihood).Visible = HasKnownLikelihood() ' D2691

                        If UserID <> CombinedUserID And _isMyVisisble Then 'C0117 + D4375

                            'C0216===
                            If Not LocalResults.ParentNode.Hierarchy.ProjectManager.StorageManager.Reader.StoredUserJudgmentsUpToDate(LocalResults.ParentNode.Hierarchy.ProjectManager.GetUserByID(UserID)) Then
                                LocalResults.ParentNode.Hierarchy.ProjectManager.StorageManager.Reader.LoadUserJudgments(LocalResults.ParentNode.Hierarchy.ProjectManager.GetUserByID(UserID))
                            End If
                            'C0216==

                            If LocalResults.CanShowIndividualResults() Then
                                FillData(_GridData, LocalResults.ResultsList(UserID, UserID), False) 'C0092
                                If ShowInconsistencyRatio AndAlso fCanShowMyResults AndAlso (LocalResults.ParentNode.MeasureType = ECMeasureType.mtPairwise OrElse LocalResults.ParentNode.MeasureType = ECMeasureType.mtPWOutcomes OrElse LocalResults.ParentNode.MeasureType = ECMeasureType.mtPWAnalogous) Then    ' D0124 + D2315
                                    lblInconsistency.Text = String.Format(InconsistencyRatioMyTemplate, LocalResults.InconsistencyIndividual.ToString(String.Format("F{0}", InconsistencyPrecision)))   ' D0188 + D1223
                                    lblInconsistency.Visible = True
                                End If
                            Else
                                fCanShowMyResults = False
                                _isMyVisisble = False   ' D0123
                            End If
                        End If
                        GridResults.Columns(colValueMy).Visible = _isMyVisisble ' D0516

                        _isCombinedVisible = LocalResults.ResultsViewMode = ResultsView.rvBoth Or LocalResults.ResultsViewMode = ResultsView.rvGroup
                        If LocalResults.ResultsViewMode = ResultsView.rvBoth And HideCombinedResults4OneUser And LocalResults.ParentNode.Hierarchy.ProjectManager.UsersList.Count <= 1 Then _isCombinedVisible = False ' D0345 
                        fCanShowCombinedResults = True
                        If _isCombinedVisible Then
                            'Dim sSessName As String = String.Format("LRJ{0}-{1}", ModelID, CombinedUserID) ' -D4376
                            ' D4375 ===
                            'If Session(sSessName) Is Nothing AndAlso CombinedUserID <> COMBINED_USER_ID Then  ' D4375
                            If CombinedUserID <> COMBINED_USER_ID Then  ' D4375 + D4376
                                LocalResults.ParentNode.Hierarchy.ProjectManager.StorageManager.Reader.LoadUserData(LocalResults.ParentNode.Hierarchy.ProjectManager.GetUserByID(CombinedUserID))   ' D4382
                                'Session.Add(sSessName, True)   ' -D4376
                            End If
                            ' D4375 ==
                            If CanShowCombinedResults(False) Then   ' D4375
                                FillData(_GridData, LocalResults.ResultsList(CombinedUserID, UserID), True) 'C0092 + D4375
                                ' D1223 ===
                                If ShowInconsistencyRatio AndAlso fCanShowCombinedResults AndAlso LocalResults.ParentNode.MeasureType = ECMeasureType.mtPairwise AndAlso CombinedUserID = COMBINED_USER_ID Then  ' D1260 + D4385
                                    lblInconsistency.Text += CStr(IIf(lblInconsistency.Text = "", "", "<br>")) + String.Format(InconsistencyRatioCombinedTemplate, LocalResults.InconsistencyCombined.ToString(String.Format("F{0}", InconsistencyPrecision)))
                                    lblInconsistency.Visible = True
                                End If
                                ' D1223
                            Else
                                fCanShowCombinedResults = False
                                _isCombinedVisible = False  ' D0123
                            End If
                        End If

                        ' D2130 ===
                        If (fCanShowMyResults OrElse fCanShowCombinedResults) AndAlso LocalResults.ShowExpectedValue Then
                            lblInconsistency.Visible = True
                            If lblInconsistency.Text <> "" Then lblInconsistency.Text += "<br>"

                            If _isMyVisisble AndAlso Not IsCombinedUserID(UserID) Then lblInconsistency.Text += String.Format(" &nbsp;<b>{0}</b> <span class='{1}'>{2}</span>&nbsp; ", lblExpectedValueIndividual, StyleLabelMy, LocalResults.ExpectedValue(UserID).ToString(String.Format("F{0}", 2 + InconsistencyPrecision)))
                            If _isCombinedVisible Then lblInconsistency.Text += String.Format(" &nbsp;<b>{0}</b> <span class='{1}'>{2}</span>&nbsp; ", lblExpectedValueCombined, StyleLabelCombined, LocalResults.ExpectedValue(CombinedUserID).ToString(String.Format("F{0}", 2 + InconsistencyPrecision))) ' D2315 + D4375
                        End If
                        ' D2130 ==

                    End If ' Local
                    GridResults.Columns(colValueCombine).Visible = _isCombinedVisible   ' D0516
                    'GridResults.Columns(colPosition).Visible = isLocalResults AndAlso CurrentNode IsNot Nothing AndAlso Not CurrentNode.IsTerminalNode     ' D0604 show only for objectives

                    If _GridData.Count = 0 Or (Not fCanShowCombinedResults And Not fCanShowMyResults) Then
                        'GridResults.Visible = False ' D0123
                        lblInconsistency.Visible = False    ' D0146
                        lblMessage.Text = String.Format("<h6 style='margin-top:6em'>{0}</h6>", messageNoAnyResults)    ' D2331
                        tdNormalizationMode.Visible = False ' D0631
                    Else
                        'GridResults.Visible = True
                        If _MaxValue < 0.0001 Or Not ShowGraphForMax Then _MaxValue = 1 ' D0123
                        If Not fCanShowMyResults Then lblMessage.Text = messageNoIndividualResults
                        If Not fCanShowCombinedResults Then lblMessage.Text = messageNoCombinedResults
                    End If
                    If lblMessage.Text <> "" Then
                        If isGlobalResults AndAlso Not isCISEnabled Then lblMessage.Text += String.Format("<p style='font-weight:normal; color: #333333; text-align:left; padding:0px 3em;'>{0}</p>", messageTurnCIS)
                        lblMessage.Visible = True
                    End If
                End If

                'tdNormalizationMode.Visible = GridResults.Visible   ' D3518
                tdNormalizationMode.Visible = ddResultsMode.Visible AndAlso Hierarchy.ProjectManager.PipeParameters.ProjectType <> ProjectType.ptMyRiskReward AndAlso GridResults.Visible AndAlso _GridData IsNot Nothing AndAlso _GridData.Count > 0   ' D3518 + D3556 + D6981 + D7238

                If GridResults.Visible And Not _GridData Is Nothing Then
                    If _GridData.Count > 0 Then

                        ' D0394 ===
                        If Not _isMyVisisble AndAlso _isCombinedVisible AndAlso SortExpression = ResultsSortMode.rsmPriority Then SortExpression = ResultsSortMode.rsmCombined ' D0937
                        If _isMyVisisble AndAlso Not _isCombinedVisible AndAlso SortExpression = ResultsSortMode.rsmCombined Then SortExpression = ResultsSortMode.rsmPriority ' D0937
                        'If Not _isMyVisisble And _isCombinedVisible And SortExpression = ShowResultsSortField.IndividualResults Then SortExpression = ShowResultsSortField.CombinedResults
                        'If _isMyVisisble And Not _isCombinedVisible And SortExpression = ShowResultsSortField.CombinedResults Then SortExpression = ShowResultsSortField.IndividualResults
                        ' D0394 ==

                        ' D0136 ===
                        For i As Integer = 0 To GridResults.Columns.Count - 1
                            GridResults.Columns(i).HeaderText = GetColumnName(i)
                        Next
                        GridResults.Columns(colGraph).ItemStyle.Width = GraphWidth + 4
                        ' D0136 ==

                        Dim Comparer As New clsShowResultsItemComparer(SortExpression, SortDirection)
                        _GridData.Sort(Comparer)
                        Comparer = Nothing

                        ' -D2610 ===
                        'Dim colSortIdx As Integer = -1
                        'Select Case SortExpression
                        '    ' D0937 ===
                        '    Case ResultsSortMode.rsmNumber
                        '        colSortIdx = colPosition
                        '    Case ResultsSortMode.rsmName
                        '        colSortIdx = colName
                        '    Case ResultsSortMode.rsmPriority
                        '        colSortIdx = colValueMy
                        '    Case ResultsSortMode.rsmCombined
                        '        colSortIdx = colValueCombine
                        '        ' D0937 ==
                        'End Select
                        'If colSortIdx >= 0 Then GridResults.Columns(colSortIdx).HeaderText = GetColumnName(colSortIdx) + IIf(SortDirection = SortDirection.Ascending, _sortAscTemplate, _sortDescTemplate)
                        ''End If ' -D0604

                        'GridResults.Columns(colValueCombine).HeaderText = String.Format("<span class='{0}'>{1}</span>", StyleLabelCombined, GridResults.Columns(colValueCombine).HeaderText)    ' D0773
                        ' -D2610 ==

                        'GridResults.AllowSorting = AllowSorting
                        GridResults.DataSource = _GridData
                        GridResults.DataBind()
                        If isGlobalResults Then RadSplitterResults.BorderSize = 1 ' D2335
                    End If
                End If
                ' D0123 ==

                ' D2130 ===
                If isGlobalResults AndAlso ShowOverallExpectedValueMode <> ResultsView.rvNone Then

                    Dim CanShowHiddenExpectedValue As Boolean = False
                    Dim IndivExpectedValue As Double = 0
                    Dim CombinedExpectedValue As Double = 0

                    For Each tRow As clsShowResultsItem In _GridData
                        Dim tmpStr As String() = tRow.Name.Split(CChar(" "))
                        If tmpStr.Length > 0 Then
                            Dim tVal As Double = 0
                            If String2Double(tmpStr(0), tVal) Then
                                CanShowHiddenExpectedValue = True
                                IndivExpectedValue += tVal * tRow.ValueMy
                                CombinedExpectedValue += tVal * tRow.ValueCombined
                            End If
                        End If
                    Next

                    If CanShowHiddenExpectedValue Then
                        lblInconsistency.Visible = True
                        If lblInconsistency.Text <> "" Then lblInconsistency.Text += "<br>"

                        If ShowOverallExpectedValueMode <> ResultsView.rvGroup AndAlso IndivExpectedValue > 0 AndAlso _isMyVisisble Then lblInconsistency.Text += String.Format(" &nbsp;<b>{0}</b> <span class='{1}'>{2}</span>&nbsp; ", lblExpectedValueIndividual, StyleLabelMy, IndivExpectedValue.ToString(String.Format("F{0}", 2 + InconsistencyPrecision))) ' D3694
                        If ShowOverallExpectedValueMode <> ResultsView.rvIndividual AndAlso CombinedExpectedValue > 0 AndAlso _isCombinedVisible Then lblInconsistency.Text += String.Format(" &nbsp;<b>{0}</b> <span class='{1}'>{2}</span>&nbsp; ", lblExpectedValueCombined, StyleLabelCombined, CombinedExpectedValue.ToString(String.Format("F{0}", 2 + InconsistencyPrecision))) ' D3694
                    End If
                End If
                ' D2130 ==

                ' D1506 ===
                If MessageCustom <> "" Then
                    lblMessage.Text += CStr(IIf(lblMessage.Text = "", "", "<br><br>")) + MessageCustom
                    lblMessage.Visible = True
                End If
                If ButtonReturnURL <> "" AndAlso lblMessage.Visible Then
                    lblMessage.Text += CStr(IIf(lblMessage.Text = "", "", "<br>")) + String.Format("<p align=center><input type='button' class='button' value='{0}' onclick='document.location.href=""{1}""; return false' style='width:{2}ex;'></p>", JS_SafeHTML(ButtonReturnCaption), JS_SafeString(ButtonReturnURL), ButtonReturnCaption.Length + 6)
                End If
                ' D1506 ==

            End If
        End Sub

        '' D0123 ===
        'Protected Sub GridResults_Sorting(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewSortEventArgs) Handles GridResults.Sorting
        '    If Not AllowSorting Then e.Cancel = True
        '    ' D0157 ===
        '    Dim newSE As ResultsSortMode = StringToSortExpression(e.SortExpression.ToString)    ' D0937
        '    If SortExpression = newSE Then
        '        SortDirection = IIf(SortDirection = SortDirection.Descending, SortDirection.Ascending, SortDirection.Descending)
        '    Else
        '        SortDirection = IIf(newSE = ResultsSortMode.rsmName Or newSE = ResultsSortMode.rsmNumber, WebControls.SortDirection.Ascending, WebControls.SortDirection.Descending)   ' D0394 + D0604 + D0937
        '        SortExpression = newSE
        '    End If
        '    ' D0157 ==
        'End Sub
        '' D0123 ==

        ' D0136 ===
        Private ReadOnly Property GetColumnName(idx As Integer) As String
            Get
                Select Case idx
                    Case colPosition    ' D0604
                        Return columnPosition   ' D0604
                    Case colName
                        Return columnName
                    Case colKnownLikelihood         ' D2691
                        Return columnLikelihood     ' D2691
                    Case colValueMy
                        Return columnValueMy
                    Case colValueCombine
                        Return columnValueCombined
                    Case colGraph
                        Return columnGraph
                    Case Else
                        Return ""
                End Select
            End Get
        End Property
        ' D0136 ==

        ' D0193 ===
        Protected Sub RadTreeViewHierarchy_PreRender(sender As Object, e As EventArgs) Handles RadTreeViewHierarchy.PreRender
            If Not IsPostBack Then  ' D3388
                TreeInit()  ' D0394
                RadTreeViewHierarchy.DataBind()
            End If
        End Sub

        Private Sub Node2RadTreeNode(sCaption As String, ID As Integer, Level As Integer, ByRef rNode As RadTreeNode, isActive As Boolean)
            ' D0202 ===
            Dim LenOffset As Integer = CInt(IIf(isActive, 3, 0))
            If ShowInfodocs Then Level += 1
            rNode.Text = JS_SafeHTML(sCaption)  ' D2280
            rNode.ToolTip = JS_SafeHTML(sCaption)   ' D2280
            ' D0202 ==
            rNode.Value = CStr(ID)
        End Sub

        ' D2112 ===
        Public Function CreateNodeInfodoc(sNodeID As String, NodeType As reObjectType) As String
            Dim sImg As String = ""
            Dim H As clsHierarchy = Hierarchy
            If NodeType <> reObjectType.Node Then H = Hierarchy.ProjectManager.AltsHierarchy(Hierarchy.ProjectManager.ActiveAltsHierarchy)
            If ShowInfodocs AndAlso Not String.IsNullOrEmpty(sNodeID) AndAlso H IsNot Nothing Then
                Dim NID As Integer = -1
                If Integer.TryParse(sNodeID, NID) Then
                    Dim tNode As clsNode = H.GetNodeByID(NID)
                    If tNode IsNot Nothing Then
                        Dim sInfodoc As String = ""
                        Dim WRTNodeID As Integer = -1
                        Dim PGuid As String = ""    ' D3538
                        If NodeType = reObjectType.AltWRTNode Then
                            If CurrentNode IsNot Nothing Then
                                WRTNodeID = CurrentNode.NodeID
                                PGuid = CurrentNode.NodeGuidID.ToString   ' D3538
                                sInfodoc = H.ProjectManager.InfoDocs.GetNodeWRTInfoDoc(tNode.NodeGuidID, CurrentNode.NodeGuidID)
                            End If
                        Else
                            sInfodoc = tNode.InfoDoc
                        End If
                        Dim sURL As String = ""
                        Dim sImgID As String = String.Format("{0}{1}_{2}", If(NodeType = reObjectType.AltWRTNode, "wrt", "node"), CInt(NodeType), NID) ' D3538
                        If sInfodoc <> "" OrElse CanEditInfodocs Then   ' D3538
                            'sInfodoc = Infodoc_Unpack(H.ProjectManager.StorageManager.ModelID, H.ProjectManager.ActiveHierarchy, NodeType, sNodeID, sInfodoc, False, True, WRTNodeID)   ' D2267    ' -D3538 due to no need to show it on that control
                            If InfodocURL <> "" Then sURL = String.Format("{0}?field=infodoc&type={1}&{2}={3}&guid={6}&pid={4}&pguid={7}&callback={8}&r={5}", InfodocURL, CInt(NodeType), _PARAM_ID, sNodeID, WRTNodeID.ToString, GetRandomString(6, True, False), tNode.NodeGuidID, PGuid, sImgID) ' D3538
                        End If
                        'If sInfodoc <> "" OrElse CanEditInfodocs OrElse NodeType <> reObjectType.AltWRTNode Then sImg = String.Format("<img src='{0}{5}{1}' width='{4}' height='{4}' border=0 style='margin-{6}:4px;{8}' {7} id='{9}'/>", ImgPath, IIf(sInfodoc = "", "_dis.", ".") + IIf(NodeType = reObjectType.AltWRTNode, "gif", "png"), sNodeID, SafeFormString(sInfodoc), IIf(NodeType = reObjectType.AltWRTNode, 16, 12), IIf(NodeType = reObjectType.AltWRTNode, "readme", "info12"), IIf(NodeType = reObjectType.AltWRTNode, "left", "right"), IIf(sURL = "", "", String.Format(" onclick='res_open_infodoc(""{0}"", {1}); return false;'", sURL, IIf(CanEditInfodocs, 1, 0))), IIf(sURL = "", "", "cursor:hand;"), sImgID) ' D3538
                        If sInfodoc <> "" OrElse CanEditInfodocs Then sImg = String.Format("<img src='{0}{5}{1}' width='{4}' height='{4}' border=0 style='margin-{6}:4px;{8}' {7} id='{9}'/>", ImgPath, If(sInfodoc = "", "_dis.", ".") + If(NodeType = reObjectType.AltWRTNode, "gif", "png"), sNodeID, SafeFormString(sInfodoc), If(NodeType = reObjectType.AltWRTNode, 16, 12), If(NodeType = reObjectType.AltWRTNode, "readme", "info12"), If(NodeType = reObjectType.AltWRTNode, "left", "right"), If(sURL = "", "", String.Format(" onclick='res_open_infodoc(""{0}"", {1}); return false;'", sURL, If(CanEditInfodocs, 1, 0))), If(sURL = "", "", "cursor:hand;"), sImgID) ' D3538 + D4522
                    End If
                End If
            End If
            Return sImg
        End Function

        Public Function GetNodePrty(tTreeNode As Object) As String
            Dim sValue = "&nbsp;"
            If tTreeNode IsNot Nothing AndAlso TypeOf (tTreeNode) Is RadTreeNode AndAlso ShowTreePriorities AndAlso Hierarchy IsNot Nothing Then
                Dim tRadNode As RadTreeNode = CType(tTreeNode, RadTreeNode)
                Dim NID As Integer = -1
                If Integer.TryParse(tRadNode.Value, NID) Then
                    Dim tNode As clsNode = Hierarchy.GetNodeByID(NID)
                    ' D2682 ===
                    If Hierarchy.ProjectManager.IsRiskProject AndAlso Hierarchy.HierarchyID = ECHierarchyID.hidLikelihood AndAlso (tNode.ParentNode Is Nothing OrElse tNode.RiskNodeType = RiskNodeType.ntCategory) Then
                        ' no need to show prty (Case 6524)
                    Else
                        Dim CT As New clsCalculationTarget(If(IsCombinedUserID(UserID4Tree), CalculationTargetType.cttCombinedGroup, CalculationTargetType.cttUser), IIf(IsCombinedUserID(UserID4Tree), tNode.Hierarchy.ProjectManager.CombinedGroups.GetDefaultCombinedGroup, tNode.Hierarchy.ProjectManager.GetUserByID(UserID4Tree)))
                        Dim PNode As clsNode = Nothing
                        If tRadNode.ParentNode IsNot Nothing Then
                            Dim PID As Integer = -1
                            If Integer.TryParse(tRadNode.ParentNode.Value, PID) Then PNode = Hierarchy.GetNodeByID(PID)
                        End If
                        ' D3874 ===
                        Dim sBar As String = "&nbsp;"
                        tNode.CalculateLocal(UserID4Tree) 'C0159
                        If tNode.LocalPriority(CT, PNode) > 0 Then
                            sBar = String.Format("<div class='{0}' style='width:{1}px; height:3px;'><img src='{2}blank.gif' width=1 height=1 title='' border=0></div>", IIf(UserID4Tree = COMBINED_USER_ID, StyleGraphCombined, StyleGraphMy), Math.Round(TreeBarWidth * (tNode.LocalPriority(CT, PNode))), ImgPath)
                        End If
                        If tNode IsNot Nothing Then sValue = String.Format("<div class='text small' style='text-align:right'>{0}%</div><div style='width:{1}px; height:3px; background:#f0f0f0; text-align:left; font-size:1px; margin-bottom:1px;'>{2}</div>", Double2String(100 * tNode.LocalPriority(CT, PNode), ValuePrecision), TreeBarWidth, sBar)    ' D7568
                        ' D3874 ==
                    End If
                    ' D2682 ==
                End If
            End If
            Return sValue
        End Function
        ' D2112 ==

        Private Function AddRadTreeNode(sCaption As String, ID As Integer, ByRef RadNodes As RadTreeNodeCollection, Level As Integer, isAlts As Boolean, fHasChilds As Boolean, fIsCategorical As Boolean) As RadTreeNode ' D2682
            Dim rNode As New RadTreeNode
            Dim isCurrent As Boolean = False
            If Not CurrentNode Is Nothing Then
                isCurrent = (ID = CurrentNode.NodeID)
                If isCurrent Then
                    rNode.ExpandParentNodes()
                    rNode.Selected = True
                End If
            Else
                isCurrent = Level = 0
            End If
            Node2RadTreeNode(sCaption, CInt(IIf(isAlts, -ID, ID)), Level, rNode, isCurrent)  ' D0202
            If isAlts Then Level += 1
            Dim asRoot As Boolean = Level = 0
            ' D0146 ===
            '            rNode.Text = String.Format("<span class='{1}'>{0}</span>", rNode.Text, IIf(asRoot, "tree_root_popup", "tree_node_popup") + IIf(isCurrent, " tree_current", "")) ' D0148
            rNode.CssClass = CStr(IIf(asRoot, "tree_root_popup", "tree_node_popup"))
            ' D2682 ===
            If fIsCategorical Then
                rNode.Text = String.Format("<span class='tree_cat'>{0}</span>", rNode.Text)
                rNode.ToolTip = String.Format("{0}: {1}", "Category", rNode.ToolTip)
            End If
            ' D2682 ==
            ' D0146 ==
            rNode.HoveredCssClass = rNode.CssClass
            rNode.SelectedCssClass = rNode.CssClass
            'rNode.SelectedCssClass = "tree_node_popup"
            RadNodes.Add(rNode)
            Return rNode
        End Function

        Private Sub AddNodesToRadTree(Nodes As List(Of clsNode), ByRef RadNodes As RadTreeNodeCollection, isAlts As Boolean, LevelOffset As Integer) 'C0384
            If Nodes Is Nothing Or RadNodes Is Nothing Then Exit Sub
            For Each tNode As clsNode In Nodes
                If Not tNode.DisabledForUser(UserID) Then   ' D1897
                    Dim Childs As List(Of clsNode) = tNode.Children    'tNode.GetNodesBelow.Clone ' D0125  - D0146 'C0384
                    Dim rNode As RadTreeNode = AddRadTreeNode(tNode.NodeName, tNode.NodeID, RadNodes, tNode.Level + LevelOffset, isAlts, Childs.Count > 0, Hierarchy.ProjectManager.IsRiskProject AndAlso tNode.RiskNodeType = RiskNodeType.ntCategory) 'C0159 + D1606 + D2682
                    If Childs.Count > 0 Then
                        rNode.Expanded = True
                        AddNodesToRadTree(Childs, rNode.Nodes, tNode.IsTerminalNode, LevelOffset)   ' D0125
                    Else
                        'rNode.Enabled = False
                        'If Not rNode.Parent Is Nothing And tNode.ParentNodeID <> 0 Then rNode.Parent.Expanded = False
                    End If
                End If
            Next
        End Sub

        ' -D1856
        'Protected Sub RadTreeViewHierarchy_NodeClick(ByVal sender As Object, ByVal e As Telerik.Web.UI.RadTreeNodeEventArgs) Handles RadTreeViewHierarchy.NodeClick ' D0535
        '    'LoadData() -D0394
        '    Dim tRadNode As RadTreeNode = e.Node    ' D0535
        '    If Not Hierarchy Is Nothing And Not tRadNode Is Nothing Then
        '        Dim NodeID As Integer = -1
        '        If Integer.TryParse(tRadNode.Value, NodeID) Then CurrentNode = Hierarchy.GetNodeByID(NodeID)
        '        If Not CurrentNode Is Nothing And Not Data Is Nothing Then
        '            If isGlobalResults Then
        '                'GlobalResults.WRTNode = CurrentNode
        '                _GridData = Nothing
        '                _MaxValue = 0   ' D0313
        '            End If
        '        End If
        '    End If
        'End Sub
        ' D0193 ==

        ' -D2608
        '' D0279 ===
        'Protected Sub imgBtnRefresh_Click(ByVal sender As Object, ByVal e As System.Web.UI.ImageClickEventArgs) Handles imgBtnRefresh.Click
        '    If Data Is Nothing Then Exit Sub
        '    If TypeOf (Data) Is clsShowGlobalResultsActionData Then 'C0464
        '        Dim GlobalRes As clsShowGlobalResultsActionData = TryCast(Data, clsShowGlobalResultsActionData) 'C0464

        '        'If _isCombinedVisible Then
        '        If (GlobalRes.ResultsViewMode = ResultsView.rvGroup) Or (GlobalRes.ResultsViewMode = ResultsView.rvBoth) Then
        '            For Each node As clsNode In GlobalRes.ProjectManager.Hierarchy(GlobalRes.ProjectManager.ActiveHierarchy).Nodes
        '                node.Judgments.Weights.ClearUserWeights(COMBINED_USER_ID) 'C0181
        '                node.Judgments.ClearCombinedJudgments() 'C0184
        '            Next
        '            GlobalRes.ProjectManager.StorageManager.Writer.DeleteCombinedJudgments(Integer.MinValue, Integer.MinValue, Integer.MinValue) 'C0185
        '        End If
        '        'If _isMyVisisble Then
        '        If (GlobalRes.ResultsViewMode = ResultsView.rvIndividual) Or (GlobalRes.ResultsViewMode = ResultsView.rvBoth) Then
        '            For Each node As clsNode In GlobalRes.ProjectManager.Hierarchy(GlobalRes.ProjectManager.ActiveHierarchy).Nodes
        '                node.Judgments.Weights.ClearUserWeights(UserID) 'C0181
        '            Next
        '        End If
        '    End If
        '    If TypeOf (Data) Is clsShowLocalResultsActionData Then 'C0464
        '        Dim LocalRes As clsShowLocalResultsActionData = TryCast(Data, clsShowLocalResultsActionData) 'C0464

        '        'If _isCombinedVisible Then
        '        If (LocalRes.ResultsViewMode = ResultsView.rvGroup) Or (LocalRes.ResultsViewMode = ResultsView.rvBoth) Then
        '            LocalRes.ParentNode.Judgments.Weights.ClearUserWeights(COMBINED_USER_ID) 'C0180
        '            LocalRes.ParentNode.Hierarchy.ProjectManager.StorageManager.Writer.DeleteCombinedJudgments(COMBINED_USER_ID, LocalRes.ParentNode.NodeID, Integer.MinValue) 'C0185
        '            LocalRes.ParentNode.Judgments.ClearCombinedJudgments() 'C0184
        '        End If
        '        'If _isMyVisisble Then
        '        If (LocalRes.ResultsViewMode = ResultsView.rvIndividual) Or (LocalRes.ResultsViewMode = ResultsView.rvBoth) Then
        '            LocalRes.ParentNode.Judgments.Weights.ClearUserWeights(UserID) 'C0180
        '        End If
        '    End If
        'End Sub
        '' D0279 ==

        ' D0631 ===
        Protected Sub ddResultsMode_Load(sender As Object, e As EventArgs) Handles ddResultsMode.Load
            If ddResultsMode.Visible AndAlso ddResultsMode.Items.Count = 0 Then ' D0672
                Dim opt As Array
                opt = [Enum].GetValues(GetType(AlternativeNormalizationOptions))
                For Each St As AlternativeNormalizationOptions In opt
                    If St <> AlternativeNormalizationOptions.anoMultipleOfMin Then  ' D2114
                        Dim Name As String = St.ToString    ' D2114
                        Dim Idx = CInt(St)
                        If Idx >= NormalizationCaptions.GetLowerBound(0) AndAlso Idx <= NormalizationCaptions.GetUpperBound(0) Then Name = NormalizationCaptions(Idx)
                        ddResultsMode.Items.Add(New ListItem(Name, CStr(Idx)))
                    End If
                Next
                ddResultsMode.SelectedValue = CInt(NormalizationMode).ToString
                ddResultsMode.Attributes.Add("onchange", String.Format("setTimeout('__doPostBack(\'{1}\',\'\')', 10); $('#{0}').css('opacity', 0); return true;", GridResults.ClientID, ddResultsMode.ClientID))
            End If
        End Sub
        ' D0631 ==

        ' D1856 ===
        Protected Sub RadAjaxPanelResults_AjaxRequest(sender As Object, e As AjaxRequestEventArgs) Handles RadAjaxPanelResults.AjaxRequest
            ' D2610 ===
            Dim sParams As NameValueCollection = HttpUtility.ParseQueryString(e.Argument)
            Select Case GetParam(sParams, _PARAM_ACTION).ToLower

                Case "node"
                    Dim ID As Integer = -1
                    If Hierarchy IsNot Nothing AndAlso Integer.TryParse(GetParam(sParams, "nid"), ID) Then
                        CurrentNode = Hierarchy.GetNodeByID(ID)
                        If Not CurrentNode Is Nothing And Not Data Is Nothing Then
                            If isGlobalResults Then
                                _GridData = Nothing
                                _MaxValue = 0
                            End If
                        End If
                    End If

                Case "sort"
                    Dim fld As Integer = -1
                    If Integer.TryParse(GetParam(sParams, "fld"), fld) Then
                        SortExpression = CType(Math.Abs(fld) - 1, ResultsSortMode)
                        If fld < 0 Then SortDirection = SortDirection.Descending Else SortDirection = SortDirection.Ascending
                    End If

            End Select
            ' D2610 ==
        End Sub
        ' D1856 ==

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            ' D3388 ===
            'If Not IsPostBack Then
            If Session(_SESS_NID + ModelID.ToString) IsNot Nothing Then ActiveNodeID = CInt(Session(_SESS_NID + ModelID.ToString)).ToString  ' D2610
            If Session(_SESS_SORT) IsNot Nothing Then SortExpression = CType(Session(_SESS_SORT), ResultsSortMode) ' D2610
            If Session(_SESS_DIR) IsNot Nothing Then SortDirection = CType(Session(_SESS_DIR), SortDirection) ' D2610
            If Session(_SESS_NORM_MODE) IsNot Nothing Then NormalizationMode = CType(Session(_SESS_NORM_MODE), AlternativeNormalizationOptions) ' D2610
            Dim NM As HttpCookie = Request.Cookies(_COOKIE_NORM_MODE)   ' D3532
            If NM IsNot Nothing AndAlso NM.Value <> "" Then NormalizationMode = CType(NM.Value, AlternativeNormalizationOptions) ' D3532
            'End If
            ' D3388 ==
        End Sub

        ' D2827 ===
        Protected Sub Page_PreRender(sender As Object, e As EventArgs) Handles Me.PreRender
            If Not IsPostBack Then ScriptManager.RegisterStartupScript(Me, GetType(String), "InitGrid", "setTimeout('onResultsResized();', 500);", True) ' D2827 + D3388 + D5073
        End Sub
        ' D2827 ==

    End Class

End Namespace
