Imports ExpertChoice.Service
Imports ECCore

Namespace ExpertChoice.Data

    Public Class clsUtilityCurveDataParser

        Private Const _PARAM_UC_ID As String = "id"

        Private Const _PARAM_AUC_COUNT As String = "cnt"
        ' D0173 ===
        Private Const _PARAM_AUC_NAME As String = "name"
        Private Const _PARAM_AUC_X As String = "x{0}"
        Private Const _PARAM_AUC_Y As String = "y{0}"

        Private Const _PARAM_RUC_NAME As String = "UCName"
        Private Const _PARAM_RUC_MIN As String = "XMin"
        Private Const _PARAM_RUC_MAX As String = "XMax"
        Private Const _PARAM_RUC_DECREASING As String = "Decr"  ' D0174
        Private Const _PARAM_RUC_CURVATURE As String = "Curvature"
        ' D0173 ==

        ' D0155 ===
        Private _UC As clsCustomUtilityCurve

        Public Sub New(Optional ByVal UCData As clsCustomUtilityCurve = Nothing)
            _UC = UCData
        End Sub

        Public Property UCData() As clsCustomUtilityCurve
            Get
                Return _UC
            End Get
            Set(ByVal value As clsCustomUtilityCurve)
                _UC = value
            End Set
        End Property

        Private Property AdvancedUC() As clsAdvancedUtilityCurve
            Get
                If UCData Is Nothing Then Return Nothing Else Return CType(UCData, clsAdvancedUtilityCurve)
            End Get
            Set(ByVal value As clsAdvancedUtilityCurve)
                UCData = value
            End Set
        End Property

        ' D0173 ===
        Private Property RegularUC() As clsRegularUtilityCurve
            Get
                If UCData Is Nothing Then Return Nothing Else Return CType(UCData, clsRegularUtilityCurve)
            End Get
            Set(ByVal value As clsRegularUtilityCurve)
                UCData = value
            End Set
        End Property
        ' D0173 ==

        Private ReadOnly Property UCMeasureType() As ECMeasureType
            Get
                Dim Type As ECMeasureType = ECMeasureType.mtNone
                If Not UCData Is Nothing Then
                    If TypeOf (UCData) Is clsAdvancedUtilityCurve Then Type = ECMeasureType.mtAdvancedUtilityCurve
                    If TypeOf (UCData) Is clsRegularUtilityCurve Then Type = ECMeasureType.mtRegularUtilityCurve ' D0173
                End If
                Return Type
            End Get
        End Property

        Public Function GetUCData() As String
            Dim sContent As String = ""
            Select Case UCMeasureType
                Case ECMeasureType.mtAdvancedUtilityCurve
                    sContent = GetAdvancedUCData(AdvancedUC)
                Case ECMeasureType.mtRegularUtilityCurve    ' D0173
                    sContent = GetRegularUCData(RegularUC)        ' D0173
            End Select
            Return sContent
        End Function

        Public Overridable Function ParseData(ByVal sContent As String) As Boolean
            Dim args As Specialized.NameValueCollection = HttpUtility.ParseQueryString(URLDecode(sContent)) ' D0178
            Return ParseData(args)
        End Function

        Public Overridable Function ParseData(ByVal Params As Specialized.NameValueCollection) As Boolean
            Dim fParsed As Boolean = False
            If Not Params Is Nothing And Not UCData Is Nothing Then
                Select Case UCMeasureType
                    Case ECMeasureType.mtAdvancedUtilityCurve
                        fParsed = ParseAdvancedUCData(AdvancedUC, Params)
                    Case ECMeasureType.mtRegularUtilityCurve                ' D0173
                        fParsed = ParseRegularUCData(RegularUC, Params)     ' D0173
                End Select
            End If
            Return fParsed
        End Function
        ' D0155 ==

        Private Function GetAdvancedUCData(ByVal UC As clsAdvancedUtilityCurve) As String   ' D0155
            Dim sContent As String = ""
            If Not UC Is Nothing Then
                sContent += String.Format(_PARAM_UC_ID + "={0}&" + _PARAM_AUC_COUNT + "={1}&" + _PARAM_AUC_NAME + "={2}", UC.ID, UC.Points.Count, HttpUtility.UrlEncode(UC.Name))
                Dim tPoint As clsUCPoint
                For i As Integer = 0 To UC.Points.Count - 1
                    tPoint = CType(UC.Points(i), clsUCPoint)
                    sContent += String.Format("&" + _PARAM_AUC_X + "={1}&" + _PARAM_AUC_Y + "={2}", i + 1, JS_SafeNumber(tPoint.X), JS_SafeNumber(tPoint.Y))
                Next
            End If
            Return sContent
        End Function

        Private Function ParseAdvancedUCData(ByRef UC As clsAdvancedUtilityCurve, ByVal URIParams As Specialized.NameValueCollection) As Boolean ' D0155
            Dim fParsed As Boolean = False
            If Not UC Is Nothing And UCMeasureType = ECMeasureType.mtAdvancedUtilityCurve Then  ' D0173

                Dim sName As String = GetParam(URIParams, _PARAM_AUC_NAME).Trim  ' D0155
                If sName <> "" Then UC.Name = sName

                Dim Cnt As Integer = 0
                Dim sCnt As String = GetParam(URIParams, _PARAM_AUC_COUNT)   ' D0155
                If Integer.TryParse(sCnt, Cnt) Then
                    Dim Points As New ArrayList
                    Dim xMin As Single
                    Dim xMax As Single
                    Dim X, Y As Double  ' D1858
                    For i As Integer = 1 To Cnt
                        Dim tPoint As New clsUCPoint
                        If String2Double(GetParam(URIParams, String.Format(_PARAM_AUC_X, i)), X) And _
                           String2Double(GetParam(URIParams, String.Format(_PARAM_AUC_Y, i)), Y) Then ' D0155 + D1858
                            tPoint.X = CSng(X)  ' D1858
                            tPoint.Y = CSng(Y)  ' D1858
                            Points.Add(tPoint)
                            If i = 1 Or tPoint.X < xMin Then xMin = tPoint.X
                            If i = 1 Or tPoint.X > xMax Then xMax = tPoint.X
                        End If
                    Next
                    If Points.Count > 1 Then
                        UC.Low = xMin
                        UC.High = xMax
                        UC.Points.Clear()
                        For Each tPoint As clsUCPoint In Points
                            UC.AddUCPoint(tPoint.X, tPoint.Y)
                        Next
                    End If
                    fParsed = True
                    Points = Nothing
                End If

            End If
            Return fParsed
        End Function

        ' D0173 ===
        Private Function GetRegularUCData(ByVal RUC As clsRegularUtilityCurve) As String
            Dim sContent As String = ""
            If Not RUC Is Nothing Then
                sContent += String.Format(_PARAM_RUC_MIN + "={0}&" + _PARAM_RUC_MAX + "={1}&" + _PARAM_RUC_DECREASING + "={2}&" + _PARAM_RUC_CURVATURE + "={3}&" + _PARAM_RUC_NAME + "={4}", _
                                          JS_SafeNumber(RUC.Low), JS_SafeNumber(RUC.High), (Not RUC.IsIncreasing).ToString.ToLower, _
                                          JS_SafeNumber(RUC.Curvature), HttpUtility.UrlEncode(RUC.Name))    ' D0174
            End If
            Return sContent
        End Function

        Private Function ParseRegularUCData(ByRef RUC As clsRegularUtilityCurve, ByVal URIParams As Specialized.NameValueCollection) As Boolean
            Dim fParsed As Boolean = False
            If Not RUC Is Nothing And UCMeasureType = ECMeasureType.mtRegularUtilityCurve Then

                Dim sName As String = GetParam(URIParams, _PARAM_RUC_NAME).Trim
                If sName <> "" Then RUC.Name = sName

                Dim Xmin As Double = RUC.Low    ' D1885
                If String2Double(GetParam(URIParams, _PARAM_RUC_MIN), Xmin) Then RUC.Low = CSng(Xmin) ' D1858

                Dim Xmax As Double = RUC.High  ' D1858
                If String2Double(GetParam(URIParams, _PARAM_RUC_MAX), Xmax) Then RUC.High = CSng(Xmax) ' D1858

                Dim sDecr As String = GetParam(URIParams, _PARAM_RUC_DECREASING).ToLower
                If sDecr = False.ToString.ToLower Then RUC.IsIncreasing = True
                If sDecr = True.ToString.ToLower Then RUC.IsIncreasing = False

                Dim Curvature As Double = RUC.Curvature    ' D1858
                If String2Double(GetParam(URIParams, _PARAM_RUC_CURVATURE), Curvature) Then RUC.Curvature = CSng(Curvature) ' D1858

                fParsed = True
            End If
            Return fParsed
        End Function
        ' D0173 ==

    End Class

End Namespace
