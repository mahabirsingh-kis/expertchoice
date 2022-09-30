Public Class AdminsRedirectPage
    Inherits clsComparionCorePage

    Public Sub New()
        MyBase.New(_PGID_UNKNOWN)
    End Sub

    Protected Sub Page_PreInit(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreInit
        Response.Redirect(PageURL(If(App.ActiveWorkgroup IsNot Nothing AndAlso App.ActiveWorkgroup.Status = ecWorkgroupStatus.wsSystem, _PGID_ADMIN_WORKGROUPS, _PGID_PROJECTSLIST)), True)
    End Sub

End Class