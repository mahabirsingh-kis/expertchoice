Partial Class MyRiskRewardPage
    Inherits clsComparionCorePage

    Public Sub New()
        MyBase.New(_PGID_STRUCTURE_MYRISKREWARD)
    End Sub

    ReadOnly Property PRJ As clsProject
        Get
            Return App.ActiveProject
        End Get
    End Property

    ReadOnly Property PM As clsProjectManager
        Get
            Return PRJ.ProjectManager
        End Get
    End Property

    Public ReadOnly Property IsReadOnly As Boolean
        Get
            With App
                Return Not .CanUserModifyProject(.ActiveUser.UserID, PRJ.ID, .ActiveUserWorkgroup, .DBWorkspaceByUserIDProjectID(.ActiveUser.UserID, PRJ.ID), .ActiveWorkgroup)
            End With
        End Get
    End Property

    ' D6813 ===
    Private Sub MyRiskRewardPage_Init(sender As Object, e As EventArgs) Handles Me.Init
        If App.Options.RiskionRiskRewardMode Then
            ShowNavigation = False
            ShowTopNavigation = False
        End If
    End Sub
    ' D6813 ==

End Class