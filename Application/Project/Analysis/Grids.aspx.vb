Partial Class GridsPage
    Inherits clsComparionCorePage

    Public Sub New()
        MyBase.New(_PGID_ANALYSIS_GRIDS)
    End Sub

    Private Sub GridsPage_InitComplete(sender As Object, e As EventArgs) Handles Me.InitComplete
        Select Case CurrentPageID 
            Case _PGID_ANALYSIS_GRIDS_ALTS
                Response.Redirect(PageURL(_PGID_ANALYSIS_OVERALL_ALTS), True)
            Case Else '_PGID_ANALYSIS_GRIDS_OBJS
                Response.Redirect(PageURL(_PGID_ANALYSIS_OVERALL_OBJS), True)
        End Select

    End Sub
End Class