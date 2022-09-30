Imports DevExpress.Web.ASPxCallback

Partial Class UserDetailsPage
    Inherits clsComparionCorePage

    Public Sub New()
        MyBase.New(_PGID_ADMIN_USER_INFO)
    End Sub

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        If Not App.isAuthorized OrElse Not App.ActiveUser.CannotBeDeleted Then FetchAccess(_PGID_ADMIN_USERSLIST)
        AlignHorizontalCenter = True
        AlignVerticalCenter = True
        If Not IsCallback AndAlso Not IsPostBack Then
            ClientScript.RegisterStartupScript(GetType(String), "Init", String.Format("setTimeout('theForm.{0}.focus();', 150);", tbEmail.ClientID), True)
            Dim sEmail As String = EcSanitizer.GetSafeHtmlFragment(CheckVar(_PARAM_ID, "")).Trim()    ' Anti-XSS
            If sEmail <> "" Then
                tbEmail.Text = EcSanitizer.GetSafeHtmlFragment(sEmail)    ' Anti-XSS
                ClientScript.RegisterStartupScript(GetType(String), "InitLoad", "setTimeout('GetUserInfo()', 200);", True)
            End If
        End If
    End Sub

    Private Function Bool2Str(fValue As Boolean) As String
        If fValue Then Return ResString("lblYes") Else Return ResString("lblNo")
    End Function

    Protected Sub ASPxCallbackServices_Callback(source As Object, e As CallbackEventArgs) Handles ASPxCallbackServices.Callback
        Dim sRes As String = ""
        Dim args As NameValueCollection = HttpUtility.ParseQueryString(URLDecode(e.Parameter))
        Dim sEmail As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, _PARAM_EMAIL)).Trim()   ' Anti-XSS
        If Not String.IsNullOrEmpty(sEmail) Then
            Dim tUser As clsApplicationUser = App.DBUserByEmail(sEmail)
            If tUser Is Nothing Then
                sRes = "<p class='error' style='text-align:center'>Error: User not found!</p>"
            Else

                Dim sAction As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, _PARAM_ACTION)).Trim() ' Anti-XSS

                Select Case sAction.ToLower

                    Case "common"

                        Dim sStatus As String = "normal"
                        If tUser.Status = ecUserStatus.usDisabled Then sStatus = "disabled"
                        If tUser.CannotBeDeleted Then sStatus = "Administrator account"

                        Dim sDefWkgID As String = ""
                        Dim tWkg As clsWorkgroup = Nothing
                        If App.ActiveWorkgroup.ID = tUser.DefaultWorkgroupID Then tWkg = App.ActiveWorkgroup Else tWkg = App.DBWorkgroupByID(tUser.DefaultWorkgroupID, False, False)
                        If tWkg IsNot Nothing Then sDefWkgID = String.Format("'{0}'", tWkg.Name)

                        Dim sOwner As String = "—"
                        Dim tOwner As clsApplicationUser = App.DBUserByID(tUser.OwnerID)
                        If tOwner IsNot Nothing Then sOwner = String.Format("<a href='{0}?{1}={2}{4}' class='actions'>{3}</a>", PageURL(CurrentPageID), _PARAM_ID, HttpUtility.UrlEncode(tOwner.UserEmail), tOwner.UserEmail, GetTempThemeURI(True))

                        sRes = String.Format("<ul type=square><li>ID: #{0}</li><li>E-mail: {1}</li><li>Full Name: {2}</li>{3}</li><div id='divDetails'><a href='' onclick='return Expand(""Details"");' class='actions'>See more…</a></div>", tUser.UserID, tUser.UserEmail, tUser.UserName, If(isSSO(), "", String.Format("<li>Has password: {0}", Bool2Str(tUser.HasPassword)))) ' D6552
                        sRes += String.Format("<div id=""Details"" style='display:none'><li>Comment: '{0}'</li><li>Status: {1}</li><li>Owner: {2}</li><li>Created: {3}</li><li>Last modified: {4}</li><li>Default Workgroup: {5}</li><li>On-line: {6}</li>", tUser.Comment, sStatus, sOwner, tUser.Created, tUser.LastModify, sDefWkgID, Bool2Str(tUser.isOnline))

                        'Dim tList As List(Of Dictionary(Of String, Object)) = App.Database.SelectFromTable(clsComparionCore._TABLE_USERS, clsComparionCore._FLD_USERS_ID, tUser.UserID)
                        Dim tList As List(Of Dictionary(Of String, Object)) = App.Database.SelectBySQL(String.Format("SELECT U.*, U.ID as UserID, UW.LastProjectID FROM Users U LEFT JOIN UserWorkgroups UW ON U.ID=UW.UserID AND UW.WorkgroupID=U.LastWorkgroupID WHERE U.ID={0} ORDER BY U.Email", tUser.UserID))
                        If tList.Count = 0 Then
                            sRes += "<li>Last Session: no information</li>"
                        Else

                            'tList(0).Add("UserID", tUser.UserID)
                            tUser.Session = App.DBParse_Session(tList(0))
                            Dim sLastWkgID As String = ""
                            tWkg = Nothing
                            If App.ActiveWorkgroup.ID = tUser.Session.WorkgroupID Then tWkg = App.ActiveWorkgroup Else tWkg = App.DBWorkgroupByID(tUser.Session.WorkgroupID, False, False)
                            If tWkg IsNot Nothing Then sLastWkgID = String.Format("'{0}'", tWkg.Name)

                            Dim sLastPrj As String = ""
                            Dim tPrj As clsProject = App.DBProjectByID(tUser.Session.ProjectID)
                            If tPrj IsNot Nothing Then sLastPrj = String.Format("'{0}'", ShortString(tPrj.ProjectName, 35))

                            If Not tUser.Session.LastAccess.HasValue AndAlso tUser.Session.WorkgroupID < 1 Then
                                sRes += "<li>Last session: never logged</li>"
                            Else
                                sRes += String.Format("<li>Last visited: {0}</li><li>Last page: {1}</li><li>Last URL: {2}</li><li>Last Workgroup: {3}</li><li>Last project: {4}</li><li>Last session ID: {5}</li>", tUser.Session.LastAccess, IIf(tUser.Session.PageID > 0, String.Format("'{0}'", PageTitle(tUser.Session.PageID)), ""), String.Format("<a href='{0}'>{1}</a>", tUser.Session.URL, ShortString(tUser.Session.URL, 35)), sLastWkgID, sLastPrj, tUser.Session.SessionID)
                            End If
                        End If
                        sRes += "</div>"

                        Dim tUWList As List(Of clsUserWorkgroup) = App.DBUserWorkgroupsByUserID(tUser.UserID)
                        sRes += String.Format("<li>Attached workgroups: <a href='' onclick='GetWorkgroups(); return false;' class='actions'>{0}</a><div id='lblWorkgroups'></div></li>", tUWList.Count)

                        sRes += "</ul>"

                    Case "wkglist"

                        sRes = "<ul type='square' style='margin-left:2em'>"

                        Dim tUWList As List(Of clsUserWorkgroup) = App.DBUserWorkgroupsByUserID(tUser.UserID)
                        Dim tWkgList As List(Of clsWorkgroup) = App.AvailableWorkgroups(tUser, tUWList, True)
                        Dim tWSList As List(Of clsWorkspace) = App.DBWorkspacesByUserID(tUser.UserID)

                        For Each tWkg As clsWorkgroup In tWkgList

                            Dim sLicense As String = "OK"
                            If Not tWkg.License.isValidLicense() Then
                                sLicense = "Not valid"
                            Else
                                Dim WrongID As Long = -1
                                If Not tWkg.License.CheckAllParameters(WrongID) Then sLicense = ParseAllTemplates(App.LicenseErrorMessage(tWkg.License, CType(WrongID, ecLicenseParameter)), App.ActiveUser, App.ActiveProject) ' D2904
                            End If

                            Dim sOwner As String = ""
                            Dim tOwnerUser As clsApplicationUser = App.DBUserByID(tWkg.OwnerID)
                            If tOwnerUser IsNot Nothing Then sOwner = String.Format("<a href='{0}?{1}={2}{4}' class='actions'>{3}</a>", PageURL(CurrentPageID), _PARAM_ID, HttpUtility.UrlEncode(tOwnerUser.UserEmail), tOwnerUser.UserEmail, GetTempThemeURI(True))

                            sRes += String.Format("<li>{0} <a href='' onclick='return Expand(""wkg{1}"");'>&raquo;</a><ul id='wkg{1}' type='circle' style='display:none; margin-left:2em;'><li>ID: {1}</li><li>Comment: {2}</li><li>Status: {3}</li><li>Owner: {4}</li><li>License status: {5}</li><li>Created: {6}</li><li>Last modified: {7}</li>", tWkg.Name, tWkg.ID, tWkg.Comment, ResString("lbl_" + tWkg.Status.ToString), sOwner, sLicense, tWkg.Created, tWkg.LastModify)

                            Dim tUW As clsUserWorkgroup = clsUserWorkgroup.UserWorkgroupByUserIDAndWorkgroupID(tUser.UserID, tWkg.ID, tUWList)
                            If tUW IsNot Nothing Then

                                Dim sRole As String = ""
                                Dim tRole As clsRoleGroup = tWkg.RoleGroup(tUW.RoleGroupID, tWkg.RoleGroups)
                                If tRole IsNot Nothing Then sRole = tRole.Name

                                sRes += String.Format("<li>Attached to workgroup: {0}</li><li>User workgroup status: {1}</li><li>Workgroup access expires: {2}</li><li>Workgroup user role: {3}</li>", tUW.Created, ResString("lbl_" + tUW.Status.ToString), tUW.ExpirationDate, sRole)
                            End If

                            Dim sPrjCount As Integer = 0
                            Dim Cnt As Object = App.Database.ExecuteScalarSQL(String.Format("SELECT COUNT(W.ID) FROM Workspace W LEFT JOIN PROJECTS P ON P.ID=W.ProjectID WHERE P.WorkgroupID={0} AND W.UserID={1}", tWkg.ID, tUser.UserID))
                            If Not IsDBNull(Cnt) Then sPrjCount = CInt(Cnt)

                            sRes += String.Format("<li>Available projects: {0}</li></ul></li>", sPrjCount)
                        Next

                        sRes += "</ul>"

                End Select


            End If
        End If
        e.Result = sRes
    End Sub

End Class

