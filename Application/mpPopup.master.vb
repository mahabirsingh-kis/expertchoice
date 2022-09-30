Partial Public Class clsComparionCoreMasterPopupPage
    Inherits MasterPage

    Public ReadOnly Property _Page() As clsComparionCorePage
        Get
            Return CType(Page, clsComparionCorePage)
        End Get
    End Property

    Public Property ShowButtonsLine() As Boolean
        Get
            Return tdButtons.Visible
        End Get
        Set(value As Boolean)
            tdButtons.Visible = value
        End Set
    End Property

    Protected Sub Page_PreRender(sender As Object, e As EventArgs) Handles Me.PreRender
        Dim sTitle As String = _Page.GetPageTitle
        If sTitle = "" AndAlso Page.Title <> "" Then sTitle = Page.Title
        Page.Title = _Page.ApplicationName  ' D0090
        If sTitle <> "" AndAlso sTitle.ToLower <> Page.Title.ToLower Then Page.Title = String.Format(_Page.ResString("titleApplicationHeader"), sTitle, Page.Title)
        tdContent.Attributes.Add("align", CStr(IIf(_Page.AlignHorizontalCenter, "center", "left")))   ' D0112
        tdContent.Attributes.Add("valign", CStr(IIf(_Page.AlignVerticalCenter, "middle", "top")))     ' D0112

        DebugInfo("Page Rendered (popup template)")
    End Sub

End Class
