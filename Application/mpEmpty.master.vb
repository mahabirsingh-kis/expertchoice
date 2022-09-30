Partial Public Class clsEmptyMasterPage
    Inherits clsMasterPageBase

    ' D7383 ===
    Public Function ShowHelpIcon() As Boolean
        Dim tRes As Boolean = False
        Select Case _Page.CurrentPageID
            Case _PGID_EVALUATION, _PGID_EVALUATE_READONLY
                tRes = True
        End Select
        Return tRes
    End Function
    ' D7383 ==

End Class