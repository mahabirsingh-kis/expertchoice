

Partial Class WhoIsOnlinePage
    Inherits clsComparionCorePage

    Private Const _cell_DT As Integer = 0
    Private Const _cell_Email As Integer = 1
    Private Const _cell_Workgroup As Integer = 2    ' D0187
    Private Const _cell_Project As Integer = 3      ' D0187
    Private Const _cell_URL As Integer = 4
    Private Const _cell_SessionID As Integer = 5    ' D0501

    Private _AllProjects As List(Of clsProject) = Nothing   ' D0187 + D0501
    Private _Workgroups As List(Of clsWorkgroup) = Nothing  ' D0501

    Public Sub New()
        MyBase.New(_PGID_ADMIN_ONLINE_USERS)
    End Sub

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        AlignHorizontalCenter = True
        AlignVerticalCenter = False
        If Not IsCallback AndAlso Not IsPostBack Then
            imgBtnRefresh.Attributes.Add("align", "right")
            imgBtnRefresh.Attributes.Add("style", "margin-top:6px")
        End If
    End Sub

    Private Sub LoadUsersList()
        GridViewUsers.Columns(_cell_DT).HeaderText = ResString("tblLastAccess")
        GridViewUsers.Columns(_cell_Workgroup).HeaderText = ResString("tblWorkgroup")   ' D0187
        GridViewUsers.Columns(_cell_Project).HeaderText = ResString("tblProject")       ' D0187
        GridViewUsers.Columns(_cell_Email).HeaderText = ResString("tblUser")
        GridViewUsers.Columns(_cell_URL).HeaderText = ResString("tblLastPage")
        GridViewUsers.Columns(_cell_SessionID).HeaderText = ResString("tblSessionID")   ' D0501

        ' D0296 ===
        'Dim fCanAllView As Boolean = App.CanUserDoAction(ecActionType.at_slViewAnyWorkgroupLogs, App.ActiveUserWorkgroup)   ' D0478
        Dim fCanAllView As Boolean = App.ActiveUser.CannotBeDeleted OrElse (App.ActiveWorkgroup IsNot Nothing AndAlso App.ActiveWorkgroup.Status = ecWorkgroupStatus.wsSystem)  ' D0746 + D2237
        Dim WkgID As Integer = CheckVar(_PARAM_ID, App.ActiveWorkgroup.ID)  ' D0742
        Dim Users As New ArrayList
        For Each tSession As clsOnlineUserSession In App.DBOnlineSessions
            If fCanAllView Or tSession.WorkgroupID = WkgID Then Users.Add(tSession) ' D0742
        Next
        ' D0296 ==

        GridViewUsers.DataSource = Users
        GridViewUsers.DataBind()
    End Sub

    Protected Sub GridViewUsers_PreRender(sender As Object, e As EventArgs)
        LoadUsersList()
    End Sub

    Protected Sub GridViewUsers_RowDataBound(sender As Object, e As GridViewRowEventArgs)
        If Not e.Row.DataItem Is Nothing Then
            Dim tSess As clsOnlineUserSession = CType(e.Row.DataItem, clsOnlineUserSession)

            e.Row.Cells(_cell_Email).Text = HTMLEmailLink(tSess.UserEmail, tSess.UserEmail)  ' D0459 + D0501

            e.Row.Cells(_cell_Workgroup).Text = ""
            If tSess.WorkgroupID >= 0 Then
                If _Workgroups Is Nothing Then _Workgroups = App.DBWorkgroupsAll(False, False) ' D0501 
                Dim WG As clsWorkgroup = clsWorkgroup.WorkgroupByID(tSess.WorkgroupID, _Workgroups) ' D0501
                If Not WG Is Nothing Then e.Row.Cells(_cell_Workgroup).Text = ShortString(WG.Name, 30)
            End If

            Dim sPage As String = ""
            Dim sURL As String = tSess.URL  ' D1133
            If Not sURL.Contains("://") Then sURL = "../" + sURL ' D1133
            ' D0501 ===
            Dim PGID As Integer = tSess.PageID
            sPage = HTMLTextLink(CType(IIf(tSess.URL = "", PageURL(PGID), sURL), String), PageTitle(PGID)) ' D11311
            If sPage = "" And tSess.URL <> "" Then sPage = HTMLTextLink(sURL, ShortString(tSess.URL, 50)) ' D1133
            e.Row.Cells(_cell_URL).Text = sPage
            ' D0501 ==

            e.Row.Cells(_cell_Project).Text = "&nbsp;"
            If tSess.ProjectID >= 0 And PGID <> _PGID_UNKNOWN Then
                Dim Pg As clsPageAction = PageAction(PGID)
                If Not Pg Is Nothing Then
                    If Pg.RoleLevel = ecRoleLevel.rlModelLevel Then
                        If _AllProjects Is Nothing Then _AllProjects = App.DBProjectsAll ' D0501
                        Dim Prj As clsProject = clsProject.ProjectByID(tSess.ProjectID, _AllProjects)   ' D0501
                        If Not Prj Is Nothing Then
                            e.Row.Cells(_cell_Project).Text = ShortString(Prj.ProjectName, 35)
                            e.Row.Cells(_cell_Project).ToolTip = SafeFormString(Prj.Passcode)   ' D0563
                        End If
                    End If
                End If
            End If

        End If
    End Sub

End Class

