Partial Class CombinedReportPage
    Inherits clsComparionCorePage

    Private _SESS_REPORT_OPTIONS As String = "ReportOptions"  ' D6455
    Private _Options As ReportGeneratorOptions = Nothing    ' D6455

    Public Sub New()
        MyBase.New(_PGID_REPORT_COMBINED)
    End Sub

    Private Sub CombinedReportPage_Init(sender As Object, e As EventArgs) Handles Me.Init
        AlignVerticalCenter = True
    End Sub

    ' D6455 ===
    Public Property ReportOptions As ReportGeneratorOptions
        Get
            If _Options Is Nothing Then
                If Session(_SESS_REPORT_OPTIONS) IsNot Nothing Then
                    _Options = CType(Session(_SESS_REPORT_OPTIONS), ReportGeneratorOptions)
                End If
                If _Options Is Nothing Then ' init
                    _Options = New ReportGeneratorOptions
                    Session.Add(_SESS_REPORT_OPTIONS, _Options)
                End If
            End If
            Return _Options
        End Get
        Set(value As ReportGeneratorOptions)
            Session(_SESS_REPORT_OPTIONS) = value
        End Set
    End Property
    ' D6455 ==

End Class