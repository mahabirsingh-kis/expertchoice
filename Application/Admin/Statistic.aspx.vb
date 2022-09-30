Option Strict Off

Partial Class StatisticPage
    Inherits clsComparionCorePage

    Private Const _SESS_CUR_WKG As String = "WkgStartWkgID"
    Private Const _SESS_WKG_LST As String = "WkgStatLst"

    Private _WkgList As List(Of clsWorkgroup) = Nothing
    Private RowIdx As Integer = 0

    Delegate Function funcGetStatValue(tParam1 As String, tPeriod As Nullable(Of Date)) As String

    Public Property CurrentWkgID As Integer
        Get
            If Session(_SESS_CUR_WKG) Is Nothing Then
                ' D3527 ===
                Dim WkgID As Integer
                If App.CanUserDoSystemWorkgroupAction(ecActionType.at_slManageAnyWorkgroup, App.ActiveUser.UserID) Then WkgID = -1 Else WkgID = App.ActiveWorkgroup.ID
                Session(_SESS_CUR_WKG) = WkgID
                Return WkgID
                ' D3527 ==
            Else
                Return CInt(Session(_SESS_CUR_WKG))
            End If
        End Get
        Set(value As Integer)
            Session(_SESS_CUR_WKG) = value
        End Set
    End Property

    Public ReadOnly Property isWorkgroupSelected As Boolean
        Get
            Return CurrentWkgID >= 0
        End Get
    End Property

    Public ReadOnly Property WorkgroupsList As List(Of clsWorkgroup)
        Get
            If _WkgList Is Nothing Then
                If Session(_SESS_WKG_LST) Is Nothing Then
                    _WkgList = App.AvailableWorkgroups(App.ActiveUser)
                    Session(_SESS_WKG_LST) = _WkgList
                Else
                    _WkgList = CType(Session(_SESS_WKG_LST), List(Of clsWorkgroup))
                End If
            End If
            Return _WkgList
        End Get
    End Property

    Public ReadOnly Property CurrentWorkgrop As clsWorkgroup
        Get
            Return clsWorkgroup.WorkgroupByID(CurrentWkgID, WorkgroupsList)
        End Get
    End Property

    Public Sub New()
        MyBase.New(_PGID_ADMIN_STATISTIC)
    End Sub

    Public Function GetTitle() As String
        Dim sTitle As String = "All workgroups"
        If isWorkgroupSelected Then sTitle = CurrentWorkgrop.Name
        Return String.Format("{0} ({1})", PageTitle(CurrentPageID), sTitle)
    End Function

    Public Function GetProjectsCount(mask As Integer, bits As Integer, value As Integer) As Integer
        Dim tCond As New List(Of String)
        If mask <> 0 Then
            tCond.Add(String.Format("(Status&{0}=16384 AND (Status&{1})/{2}={3})", clsProject.mask_EncodingVersion, mask, Math.Pow(2, bits), CInt(value)))
        End If
        If isWorkgroupSelected Then tCond.Add("WorkgroupID=" + CurrentWkgID.ToString)
        Dim sSQL As String = "SELECT COUNT(ID) as cnt FROM PROJECTS" + CType(IIf(tCond.Count = 0, "", " WHERE " + String.Join(" AND ", tCond.ToArray)), String)
        Return CInt(App.Database.ExecuteScalarSQL(sSQL))
    End Function

    Public Function GetWorkgroups() As String
        Dim sLst As String = ""
        If App.CanUserDoSystemWorkgroupAction(ecActionType.at_slManageAnyWorkgroup, App.ActiveUser.UserID) Then ' D3527
            For Each tWkg As clsWorkgroup In WorkgroupsList
                If tWkg.Status <> ecWorkgroupStatus.wsSystem Then
                    Dim fIsValid As Boolean = tWkg.Status = ecWorkgroupStatus.wsEnabled AndAlso tWkg.License.isValidLicense AndAlso tWkg.License.CheckParameterByID(ecLicenseParameter.ExpirationDate) AndAlso tWkg.License.CheckParameterByID(ecLicenseParameter.InstanceID)   ' D3947
                    sLst += String.Format("<option value='{0}'{1}{3}>{2}</option>", tWkg.ID, IIf(tWkg.ID = CurrentWkgID, " selected", ""), ShortString(tWkg.Name, 40), IIf(fIsValid, "", " class='gray'"))
                End If
            Next
            sLst = String.Format("<div><b>Select workgroup</b>: <select name='wkg' onchange='onSetWkg(this.value);'><option value='-1'{0}>{1}</option>{2}</select></div>", IIf(isWorkgroupSelected, "", " selected"), "- no filter -", sLst)   ' D3527
        End If
        Return sLst
    End Function

    Private Function GetPercentage(tVal As Integer, tMax As Integer) As String
        Dim sRes As String = ""
        If tMax > 0 Then
            sRes = String.Format(" ({0})", Double2String(CDbl(100 * tVal / tMax), 1, True))
        End If
        Return String.Format("<b>{0}</b>{1}", tVal, sRes)
    End Function

    Public Function GetSummaryInfo() As String
        Dim cntAll As Integer = GetProjectsCount(0, 0, 0)
        Dim cntOnline As Integer = GetProjectsCount(clsProject.mask_OnlineStatus, clsProject.bits_OnlineStatus, 1)
        Dim cntArcs As Integer = GetProjectsCount(clsProject.mask_ProjectStatus, clsProject.bits_ProjectStatus, ecProjectStatus.psArchived)
        Dim cntTpls As Integer = GetProjectsCount(clsProject.mask_ProjectStatus, clsProject.bits_ProjectStatus, ecProjectStatus.psTemplate)
        Dim sInfo As String = String.Format("Total count of projects: <b>{0}</b>, on-line: {1}, archived: {2}, templates: {3}", cntAll, GetPercentage(cntOnline, cntAll), GetPercentage(cntArcs, cntAll), GetPercentage(cntTpls, cntAll))
        Return sInfo
    End Function

    Function GetProjectsCountDate(sField As String, tPeriod As Nullable(Of Date)) As Integer
        Dim tCond As New List(Of String)
        Dim tParams As New List(Of Object)
        If tPeriod.HasValue Then
            tCond.Add(sField + ">? AND " + sField + " IS NOT NULL")
            tParams.Add(tPeriod.Value)
        End If
        If isWorkgroupSelected Then tCond.Add("WorkgroupID=" + CurrentWkgID.ToString)
        Dim sSQL As String = "SELECT COUNT(ID) as cnt FROM Projects" + CType(IIf(tCond.Count = 0, "", " WHERE " + String.Join(" AND ", tCond.ToArray)), String)
        Return CInt(App.Database.ExecuteScalarSQL(sSQL, tParams))
    End Function

    ' D3527 ===
    Function GetUsersCountDate(sField As String, tPeriod As Nullable(Of Date)) As Integer
        Dim tCond As New List(Of String)
        Dim tParams As New List(Of Object)
        If tPeriod.HasValue Then
            tCond.Add(sField + ">? AND " + sField + " IS NOT NULL")
            tParams.Add(tPeriod.Value)
        Else
            tCond.Add(sField + " IS NOT NULL")
        End If
        If isWorkgroupSelected Then tCond.Add("WorkgroupID=" + CurrentWkgID.ToString)
        Dim sSQL As String = "SELECT COUNT(DISTINCT(UserID)) as cnt FROM UserWorkgroups" + CType(IIf(tCond.Count = 0, "", " WHERE " + String.Join(" AND ", tCond.ToArray)), String)
        Return CInt(App.Database.ExecuteScalarSQL(sSQL, tParams))
    End Function

    Function GetLogsCountDateCommon(sParams As String, tPeriod As Nullable(Of Date), fDistinct As Boolean) As Integer
        Dim tCond As New List(Of String)
        Dim tParams As New List(Of Object)
        If tPeriod.HasValue Then
            tCond.Add("DT>?")
            tParams.Add(tPeriod.Value)
        End If
        If sParams <> "" Then tCond.Add(sParams)
        If isWorkgroupSelected Then tCond.Add("WorkgroupID=" + CurrentWkgID.ToString)
        Dim sSQL As String = String.Format("SELECT COUNT({0}) as cnt FROM Logs{1}", IIf(fDistinct, "DISTINCT(UserID)", "ID"), IIf(tCond.Count = 0, "", " WHERE " + String.Join(" AND ", tCond.ToArray)))
        Return CInt(App.Database.ExecuteScalarSQL(sSQL, tParams))
    End Function

    ' D4438 ===
    Function GetResponsiveCountDateCommon(sParams As String, tPeriod As Nullable(Of Date)) As Integer
        Dim tCond As New List(Of String)
        Dim tParams As New List(Of Object)
        If tPeriod.HasValue Then
            tCond.Add("DT>?")
            tParams.Add(tPeriod.Value)
        End If
        If sParams <> "" Then tCond.Add(sParams)
        If isWorkgroupSelected Then tCond.Add("WorkgroupID=" + CurrentWkgID.ToString)
        Dim sSQL As String = String.Format("SELECT COUNT(ID) as cnt FROM Logs{0} GROUP BY TypeID", " WHERE " + String.Join(" AND ", tCond.ToArray))
        Return CInt(App.Database.ExecuteScalarSQL(sSQL, tParams))
    End Function
    ' D4438 ==

    Function GetLogsCountDate(sParams As String, tPeriod As Nullable(Of Date)) As Integer
        Return GetLogsCountDateCommon(sParams, tPeriod, False)
    End Function

    Function GetLogsCountDateDistinct(sParams As String, tPeriod As Nullable(Of Date)) As Integer
        Return GetLogsCountDateCommon(sParams, tPeriod, True)
    End Function
    ' D3527 ==

    Function GetDataRow(sName As String, sField As String, tFunc As funcGetStatValue) As String
        Dim t1m As String = tFunc(sField, Now.AddMonths(-1))
        Dim t3m As String = tFunc(sField, Now.AddMonths(-3))
        Dim t6m As String = tFunc(sField, Now.AddMonths(-6))
        Dim t1y As String = tFunc(sField, Now.AddYears(-1))
        Dim tall As String = tFunc(sField, Nothing)
        RowIdx += 1
        Return String.Format("""idx"":{0},""name"":""{1}"",""1m"":{2},""3m"":{3},""6m"":{4},""1y"":{5},""all"":{6}", RowIdx, JS_SafeString(sName), t1m, t3m, t6m, t1y, tall)
    End Function

    Public Function GetData() As String
        RowIdx = 0

        Dim sData As String = ""
        sData += String.Format("{{{0}}},", GetDataRow("Count of created projects", "Created", AddressOf GetProjectsCountDate))
        sData += String.Format("{{{0}}},", GetDataRow("Count of modified projects", clsComparionCore._FLD_PROJECTS_LASTMODIFY, AddressOf GetProjectsCountDate))
        sData += String.Format("{{{0}}},", GetDataRow("Count of opened/accessed projects", clsComparionCore._FLD_PROJECTS_LASTVISITED, AddressOf GetProjectsCountDate))
        sData += String.Format("{{{0}}},", GetDataRow("Count of attached users", "Created", AddressOf GetUsersCountDate))
        sData += String.Format("{{{0}}},", GetDataRow("Distinct users logging in", clsComparionCore._FLD_USERS_LASTVISITED, AddressOf GetUsersCountDate))
        sData += String.Format("{{{0}}},", GetDataRow("Total user logins", String.Format("(ActionID={0} OR ActionID={1} Or ActionID={2} Or ActionID={3}) AND TypeID={4}", CInt(dbActionType.actLogon), CInt(dbActionType.actCredentialsLogon), CInt(dbActionType.actTokenizedURLLogon), CInt(dbActionType.actSSOLogin), CInt(dbObjectType.einfUser)), AddressOf GetLogsCountDate))
        sData += String.Format("{{{0}}},", GetDataRow("Regular logins", String.Format("ActionID={0}", CInt(dbActionType.actLogon)), AddressOf GetLogsCountDate))
        sData += String.Format("{{{0}}},", GetDataRow("Join TeamTime meeting logins", String.Format("ActionID={0}", CInt(dbActionType.actJoinMeeting)), AddressOf GetLogsCountDate))
        sData += String.Format("{{{0}}},", GetDataRow("Token URLs logins", String.Format("ActionID={0}", CInt(dbActionType.actTokenizedURLLogon)), AddressOf GetLogsCountDate))
        If isSSO() Then sData += String.Format("{{{0}}}," + vbCrLf, GetDataRow("SSO logins", String.Format("ActionID={0}", CInt(dbActionType.actSSOLogin)), AddressOf GetLogsCountDate))  ' D6532
        sData += String.Format("{{{0}}},", GetDataRow("Responsive link logins", String.Format("(ActionID={0} OR ActionID={1} OR ActionID={2}) AND (Comment LIKE '%://r.%' OR Comment LIKE '%://gecko.%') AND TypeID={3}", CInt(dbActionType.actLogon), CInt(dbActionType.actCredentialsLogon), CInt(dbActionType.actTokenizedURLLogon), CInt(dbObjectType.einfUser)), AddressOf GetResponsiveCountDateCommon))   ' D4438
        sData += String.Format("{{{0}}},", GetDataRow("SSO logins", String.Format("ActionID={0}", CInt(dbActionType.actSSOLogin)), AddressOf GetLogsCountDate)) ' D7443
        sData += String.Format("{{{0}}},", GetDataRow("Project downloads", String.Format("ActionID={0} AND TypeID={1}", CInt(dbActionType.actDownload), CInt(dbObjectType.einfProject)), AddressOf GetLogsCountDate))
        sData += String.Format("{{{0}}},", GetDataRow("Survey downloads", String.Format("ActionID={0} AND TypeID={1}", CInt(dbActionType.actDownload), CInt(dbObjectType.einfSurvey)), AddressOf GetLogsCountDate))
        sData += String.Format("{{{0}}},", GetDataRow("Datagrid uploads", String.Format("ActionID={0}", CInt(dbActionType.actDatagridUpload)), AddressOf GetLogsCountDate))
        sData += String.Format("{{{0}}},", GetDataRow("Anytime judgments made", String.Format("ActionID={0}", CInt(dbActionType.actMakeJudgment)), AddressOf GetLogsCountDate))
        sData += String.Format("{{{0}}}", GetDataRow("RTE", String.Format("ActionID={0}", CInt(dbActionType.actShowRTE)), AddressOf GetLogsCountDate))

        'Select all login URLs
        'SELECT DISTINCT(SUBSTRING(Comment, CHARINDEX('http', Comment, 0), CHARINDEX(' ', Comment, CHARINDEX('http', Comment, 0))-CHARINDEX('http', Comment, 0))) as url FROM Logs WHERE (ActionID=1 OR ActionID=4 OR ActionID=6) AND Comment LIKE '%http%' GROUP BY Comment

        Return sData
    End Function

    Protected Sub Page_Init(sender As Object, e As EventArgs) Handles Me.Init   ' D7593
        If Not App.isAuthorized Then FetchAccess()  ' D7593
        Dim tmpWkgID As Integer = CheckVar("wkg", CurrentWkgID)
        If tmpWkgID <> CurrentWkgID AndAlso CurrentWorkgrop IsNot Nothing Then  ' D7607
            CurrentWkgID = tmpWkgID
            'Response.Redirect(PageURL(CurrentPageID, GetTempThemeURI(False)), True)
        End If
        'CustomWorkgroupPermissions = CurrentWorkgrop    ' D7270
    End Sub

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        AlignHorizontalCenter = False
        ' D7309 ===
        AlignVerticalCenter = True
        If isAJAX Then
            Dim sAction As String = CheckVar(_PARAM_ACTION, "").ToLower
            Select Case sAction
                Case "load"
                    Dim tRes As New jActionResult With {
                        .Result = ecActionResult.arSuccess,
                        .Data = JsonConvert.DeserializeObject(String.Format("[{0}]", GetData())),
                        .Message = GetSummaryInfo(),
                        .URL = GetTitle()
                        }
                    SendResponseJSON(tRes)
            End Select
        End If
        ' D7309 ==
    End Sub

End Class