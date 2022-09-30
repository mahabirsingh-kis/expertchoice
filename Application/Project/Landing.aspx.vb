Option Strict On

Partial Class ComparionLandingPage
    Inherits clsComparionCorePage

    Public Const Landing_Structure = "structure"
    Public Const Landing_Measure = "measure"
    Public Const Landing_Allocate = "allocate"
    Public Const Landing_Results = "results"
    Public Const Landing_Reports = "reports"

    Public Sub New()
        MyBase.New(_PGID_LANDING_COMMON)
    End Sub

    ' D4989 ===
    Private Sub LandingPage_Load(sender As Object, e As EventArgs) Handles Me.Load
        ' D4994 ===
        Select Case CheckVar("page", "").ToLower
            Case Landing_Structure  ' D6027
                CurrentPageID = If(App.isRiskEnabled, _PGID_LANDING_RISK_IDENTIFY, _PGID_LANDING_STRUCTURE) ' D6027 + D6061
            Case Landing_Measure
                CurrentPageID = _PGID_LANDING_MEASURE
            Case Landing_Results
                CurrentPageID = _PGID_LANDING_RESULTS
            Case Landing_Allocate
                CurrentPageID = _PGID_LANDING_ALLOCATE
            Case Landing_Reports
                CurrentPageID = _PGID_LANDING_REPORTS
            Case Else
                NavigationPageID = CheckVar(_PARAM_PAGE, CheckVar("pg", CheckVar(_PARAM_NAV_PAGE, CurrentPageID)))  ' D6027
        End Select
        ' D4994 ==
    End Sub
    ' D4989 ==

End Class