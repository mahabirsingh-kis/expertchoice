Imports DevExpress.XtraCharts
Imports DevExpress.XtraCharts.Web

Partial Class Test_Chart
    Inherits clsComparionCorePage   ' D1585

    Dim xWidth As Integer = 300
    Dim xHeight As Integer = 220

    Dim StepsCount As Integer = 20

    Public Sub New()
        MyBase.New(_PGID_EVALUATE_INFODOC)
    End Sub

    ' D4094 ===
    Public Function GetLabel() As String
        Dim sName As String = "tblSyncPriority"
        If App.HasActiveProject AndAlso App.isRiskEnabled Then
            sName = CStr(IIf(App.ActiveProject.ProjectManager.ActiveHierarchy = ECHierarchyID.hidImpact, "tblSyncPriority_Impact", "tblSyncPriority_Likelihood"))
        End If
        Return sName
    End Function
    ' D4094 ==

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        ' D1585 ===
        If Ctrl.Controls.Count = 0 AndAlso App.HasActiveProject Then

            Dim sType As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("type", "uc")).ToLower    ' D2671 + Anti-XSS

            ' Create a new chart.
            Dim splineChart As New WebChartControl()
            splineChart.Width = xWidth
            splineChart.Height = xHeight

            Select Case sType   ' D2671
                Case "uc"   ' D2671

                    ' D1858 ===
                    Dim xLow As Double = 0
                    Dim xHigh As Double = 1
                    Dim xCurvature As Double = 0.33
                    ' D1858 ==
                    Dim isLinear As Boolean = False
                    Dim isDescreasing As Boolean = True ' D1585

                    String2Double(CheckVar("low", xLow.ToString), xLow)
                    String2Double(CheckVar("high", xHigh.ToString), xHigh)
                    String2Double(CheckVar("curv", xCurvature.ToString), xCurvature)
                    isLinear = CheckVar("linear", isLinear)
                    isDescreasing = CheckVar("decr", isDescreasing)
                    ' D1585 ==

                    ' Create a spline series.
                    'Dim series1 As New Series("Series 1", ViewType.Spline)
                    Dim series1 As New Series(EcSanitizer.GetSafeHtmlFragment(CheckVar("name", "Utility Curve")), ViewType.Spline) 'A0957 + Anti-XSS

                    Dim RUC As New clsRegularUtilityCurve(-1, CSng(xLow), CSng(xHigh), CSng(xCurvature), isLinear, Not isDescreasing) ' D1585 + A1504

                    If isLinear Then StepsCount = 2
                    For i As Integer = 0 To StepsCount - 1
                        Dim x As Single = RUC.Low + (RUC.High - RUC.Low) / (StepsCount - 1) * i
                        series1.Points.Add(New SeriesPoint(x, RUC.GetValue(x)))
                    Next

                    splineChart.Series.Add(series1)

                    series1.ArgumentScaleType = ScaleType.Numerical
                    series1.Label.Visible = False

                    CType(series1.View, SplineSeriesView).LineTensionPercent = 90
                    CType(series1.View, SplineSeriesView).LineMarkerOptions.Size = 3 ' .Visible = False ' .Kind = MarkerKind.Square

                    ' D2671 ===
                Case "sf"

                    Dim Cnt As Integer
                    If Integer.TryParse(CheckVar("cnt", "0"), Cnt) Then

                        Dim xLow As Double = 0
                        Dim xHigh As Double = 1
                        Dim xVal As Double = 1

                        Dim isLinear As Boolean = CheckVar("linear", False) ' D2680

                        ' Create a spline series.
                        'Dim series1 As New Series("Series 1", ViewType.Line)
                        Dim series1 As New Series(CheckVar("name", "Step Function"), ViewType.Line) 'A0957
                        For i = 0 To Cnt - 1 Step 1
                            Dim sI As String = i.ToString

                            String2Double(CheckVar("l" + sI, xLow.ToString), xLow)
                            String2Double(CheckVar("h" + sI, xHigh.ToString), xHigh)
                            String2Double(CheckVar("v" + sI, xVal.ToString), xVal)

                            If i = Cnt - 1 AndAlso (xHigh > xLow * 100 OrElse xHigh > Integer.MaxValue / 2) Then
                                xHigh = xLow * 1.1
                                If xHigh < 1 Then xHigh = 1
                            End If

                            ' D2680 ===
                            'If i = 0 Then
                            '    Dim xNegInf As Double = -0.1
                            '    If xLow < xNegInf * 1.1 Then xNegInf = xLow * 1.1
                            '    series1.Points.Add(New SeriesPoint(xNegInf, xVal))
                            'End If
                            ' D2680 ==

                            series1.Points.Add(New SeriesPoint(xLow, xVal))
                            If Not isLinear OrElse i = Cnt - 1 Then series1.Points.Add(New SeriesPoint(xHigh, xVal)) ' D2680
                        Next
                        splineChart.Series.Add(series1)

                        series1.ArgumentScaleType = ScaleType.Numerical
                        series1.Label.Visible = False

                        CType(series1.View, LineSeriesView).LineMarkerOptions.Size = 4
                    End If
                    ' D2671 ==

            End Select

            splineChart.Legend.Visible = False
            splineChart.BorderOptions.Visible = False
            Ctrl.Controls.Add(splineChart)
        End If
    End Sub

End Class
