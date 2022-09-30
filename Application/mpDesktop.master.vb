Partial Public Class clsDefaultMasterPage
    Inherits clsMasterPageBase
    Private Sub clsDefaultMasterPage_Init(sender As Object, e As EventArgs) Handles Me.Init
        _Page.ShowNavigation = App.isAuthorized ' D4916 + D4917
        _Page.ShowTopNavigation = _Page.ShowNavigation AndAlso App.HasActiveProject AndAlso _Page.CanViewActiveProject()    ' D4916 + D4917 + D4939 + D4943 + D5079 + D5084
        ' D5072 ===
        If (_Page.ShowNavigation OrElse _Page.ShowTopNavigation) AndAlso _Page.isJustEvaluator() Then ' D5074
            _Page.ShowNavigation = False
            _Page.ShowTopNavigation = False
        End If
        ' D5072 ==
    End Sub

End Class