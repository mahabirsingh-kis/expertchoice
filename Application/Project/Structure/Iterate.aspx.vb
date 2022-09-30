Imports System.Xml

Partial Class ProjectIteratePage
    Inherits clsComparionCorePage

    Public Sub New()
        MyBase.New(_PGID_ITERATE_STRUCTURING)
    End Sub

    Private Sub ProjectIteratePage_InitComplete(sender As Object, e As EventArgs) Handles Me.InitComplete
        Dim sMode = EcSanitizer.GetSafeHtmlFragment(CheckVar("mode", "")).Trim().ToLower()   ' Anti-XSS
        Dim sHid = EcSanitizer.GetSafeHtmlFragment(CheckVar("hid", "")).Trim().ToLower()   ' Anti-XSS
        If Not String.IsNullOrEmpty(sMode) AndAlso sMode.Trim = "structure" Then
            CurrentPageID = _PGID_ITERATE_STRUCTURING
            If Not String.IsNullOrEmpty(sHid) AndAlso sHid.Trim = "2" Then CurrentPageID = _PGID_ITERATE_STRUCTURING_IMPACT
            Title = ResString("titleIterateStructuring")
        End If
        If Not String.IsNullOrEmpty(sMode) AndAlso sMode.Trim = "measure" Then
            CurrentPageID = _PGID_ITERATE_MEASUREMENT
            If Not String.IsNullOrEmpty(sHid) AndAlso sHid.Trim = "2" Then CurrentPageID = _PGID_ITERATE_MEASUREMENT_IMPACT
            Title = ResString("titleIterateMeasurement")
        End If
        'lblHeader.innerHTML = Title
    End Sub

    Private Sub hbNavAction_PreRender(sender As Object, e As EventArgs) Handles hbNavAction.PreRender
        if CurrentPageID = _PGID_ITERATE_STRUCTURING Then hbNavAction.InnerHtml = ResString("lblIterateStructuring")
        if CurrentPageID = _PGID_ITERATE_MEASUREMENT Then hbNavAction.InnerHtml = ResString("lblIterateMeasurement")
    End Sub

End Class