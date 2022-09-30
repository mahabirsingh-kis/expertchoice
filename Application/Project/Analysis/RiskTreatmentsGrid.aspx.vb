Partial Class RiskTreatmentsGridPage
    Inherits clsComparionCorePage

    Public Sub New()
        MyBase.New(_PGID_RISK_CONTROLS_DATAGRID)
    End Sub

    ReadOnly Property PRJ As clsProject
        Get
            Return App.ActiveProject
        End Get
    End Property

    ReadOnly Property PM As clsProjectManager
        Get
            Return If(PRJ IsNot Nothing, PRJ.ProjectManager, Nothing)
        End Get
    End Property

    Public ReadOnly Property IsReadOnly As Boolean
        Get
            With App
                If CurrentPageID = _PGID_ADMIN_USERSLIST Then Return Not .CanUserDoAction(ecActionType.at_alManageWorkgroupUsers, .ActiveUserWorkgroup, .ActiveWorkgroup)
                Return App.IsActiveProjectStructureReadOnly
            End With
        End Get
    End Property

End Class